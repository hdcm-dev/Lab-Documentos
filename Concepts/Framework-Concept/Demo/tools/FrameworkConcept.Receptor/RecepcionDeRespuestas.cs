using System.Text.Json;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Receptor;

/// <summary>
/// POST /respuestas — recibe lo que envía el outbox del cascarón (P5, P13). Las reglas, en orden:
///
///   1. Idempotencia (P5, P13.6): la clave de idempotencia identifica la respuesta. Si la misma respuesta llega
///      dos veces (el teléfono no supo si la primera llegó), se contesta igual que la primera vez y no se duplica.
///   2. Concurrencia (D-04 = versión + 412): cada respuesta guardada tiene una versión, que viaja como ETag.
///      Para MODIFICAR una respuesta ya recibida hay que mandar If-Match con esa versión:
///        — sin If-Match y con otro contenido → 428 (hace falta If-Match);
///        — con un If-Match viejo → 412 (alguien la cambió antes: «último gana» no es el default).
///   3. Definición (P13.4): se acepta si la definición con que se tomó está publicada y no retirada, aunque ya
///      haya una más nueva. Si está retirada (P13.5) o nunca se publicó, o los valores no cumplen esa
///      definición, va a CARTA MUERTA: se guarda, visible, y se contesta 422 para que el teléfono la muestre.
///   4. P13.10: la versión de CONTENIDO con que se tomó no se mira. Retirar un contenido no rechaza respuestas.
///
/// Códigos: 201 creada · 200 repetida o modificada · 400 mal formada · 403 de otra identidad (corrector E03) ·
/// 412 / 428 concurrencia · 422 carta muerta.
/// </summary>
internal static class RecepcionDeRespuestas
{
    /// <param name="conAutenticacion">Folio 038 — con --oidc-emisor, POST /respuestas exige un token válido (401 si no) y
    /// guarda el «sub» como dueño de la respuesta (P4).</param>
    public static void Mapear(WebApplication app, AlmacenDeRespuestas almacen, RegistroDeDefiniciones registro, IReloj reloj, bool conAutenticacion = false)
    {
        var recepcion = app.MapPost("/respuestas", async (HttpContext http, ILogger<Program> log) =>
        {
            // P4 — La identidad del dueño: el «sub» del token ya validado (firma, iss, aud, exp).
            var dueno = http.User.FindFirst("sub")?.Value;
            var usuario = http.User.FindFirst("preferred_username")?.Value;

            EnvioDeRespuesta? envio;
            try
            {
                envio = await JsonSerializer.DeserializeAsync<EnvioDeRespuesta>(http.Request.Body, JsonDelContrato.Opciones);
            }
            catch (JsonException e)
            {
                return Problema(400, $"el cuerpo no es un envío de respuesta: {e.Message}");
            }
            if (envio is null || string.IsNullOrWhiteSpace(envio.ClaveIdempotencia) || envio.Valores is null)
                return Problema(400, "faltan claveIdempotencia o valores");

            // La clave también puede venir en el encabezado estándar; si viene, tiene que coincidir.
            var encabezado = http.Request.Headers["Idempotency-Key"].ToString();
            if (encabezado.Length > 0 && encabezado != envio.ClaveIdempotencia)
                return Problema(400, "Idempotency-Key no coincide con claveIdempotencia");

            var ifMatch = http.Request.Headers.IfMatch.ToString();
            var clasificacion = Clasificar(envio, registro);
            var ahora = reloj.Ahora;
            var cuerpo = JsonDelContrato.Escribir(envio);

            var (status, guardada, duplicada) = almacen.Con<(int Status, RespuestaRecibida? Guardada, bool Duplicada)>(respuestas =>
            {
                if (respuestas.TryGetValue(envio.ClaveIdempotencia, out var existente))
                {
                    // Corrector E03 — Una respuesta sólo la toca su dueño: otra identidad (otro «sub») recibe 403, ni
                    // reenvío ni modificación. Sin --oidc-emisor los dos dueños son null y no cambia nada (A1–A3).
                    if (!string.Equals(existente.Dueno, dueno, StringComparison.Ordinal))
                        return ((403, null, false), false);

                    if (ifMatch.Length == 0)
                    {
                        if (JsonDelContrato.Escribir(existente.Envio) != cuerpo)
                            return ((428, existente, false), false);

                        // 1. La misma respuesta otra vez: se cuenta y se contesta lo mismo que la primera vez.
                        var repetida = existente with { Reenvios = existente.Reenvios + 1 };
                        respuestas[repetida.Clave] = repetida;
                        return ((StatusDe(repetida, 200), repetida, true), true);
                    }

                    if (!Coincide(ifMatch, existente.Version))
                        return ((412, existente, false), false);

                    // 2. Modificación con la versión correcta, por su dueño: la versión sube.
                    var modificada = Nueva(envio, clasificacion, existente.Version + 1, existente.RecibidaEn, ahora, existente.Reenvios) with
                    {
                        Dueno = existente.Dueno,
                        Usuario = existente.Usuario ?? usuario
                    };
                    respuestas[modificada.Clave] = modificada;
                    return ((StatusDe(modificada, 200), modificada, false), true);
                }

                // If-Match sobre algo que no existe no se cumple nunca (RFC 9110 §13.1.1).
                if (ifMatch.Length > 0) return ((412, null, false), false);

                var creada = Nueva(envio, clasificacion, 1, ahora, null, 0) with { Dueno = dueno, Usuario = usuario };
                respuestas[creada.Clave] = creada;
                return ((StatusDe(creada, 201), creada, false), true);
            });

            log.LogInformation("respuestas: {Clave} {Encuesta} v{Version} → {Status}{Duplicada}{Dueno}",
                envio.ClaveIdempotencia, envio.EncuestaId, envio.VersionDefinicion, status, duplicada ? " (repetida, no se duplicó)" : "",
                dueno is null ? "" : $" dueño {usuario ?? "?"} ({dueno})");

            return status switch
            {
                403 => Problema(403, "la respuesta con esa clave es de otra identidad"),
                428 => Problema(428, "ya existe una respuesta con esa clave y otro contenido: para modificarla mandá If-Match con su versión (ETag)", guardada),
                412 => Problema(412, guardada is null
                    ? "If-Match sobre una respuesta que no existe"
                    : $"If-Match no coincide: la versión actual es {guardada.Version}", guardada),
                _ => Respuesta(http, status, guardada!, duplicada)
            };
        });

        if (conAutenticacion) recepcion.RequireAuthorization(AutenticacionDelReceptor.Politica);

        // Corrector E03 — Con --oidc-emisor, GET /respuestas* exige token y muestra sólo las respuestas de ese «sub»
        // (clave, versión y valores ajenos daban lo necesario para pisarlas). El operador lee el almacén
        // (salida/receptor/respuestas.json; «guion.sh respuestas» lo hace así en modo Oidc).
        var listado = app.MapGet("/respuestas", (HttpContext http) =>
        {
            var todas = almacen.Todas().Where(r => !conAutenticacion || r.Dueno == http.User.FindFirst("sub")?.Value).ToList();
            return Results.Json(new
            {
                aceptadas = todas.Where(r => r.Estado == EstadoDeRecepcion.Aceptada),
                cartaMuerta = todas.Where(r => r.Estado == EstadoDeRecepcion.CartaMuerta),
                reenviosQueNoSeDuplicaron = todas.Sum(r => r.Reenvios)
            }, JsonDelContrato.Opciones);
        });

        var una = app.MapGet("/respuestas/{clave}", (string clave, HttpContext http) =>
        {
            var encontrada = almacen.Todas().FirstOrDefault(r => r.Clave == clave);
            // Ajena = como si no existiera: no se confirma ni que la clave existe.
            if (encontrada is null || conAutenticacion && encontrada.Dueno != http.User.FindFirst("sub")?.Value) return Results.NotFound();
            http.Response.Headers.ETag = Etiqueta(encontrada.Version);
            return Results.Json(encontrada, JsonDelContrato.Opciones);
        });

        if (conAutenticacion)
        {
            listado.RequireAuthorization(AutenticacionDelReceptor.Politica);
            una.RequireAuthorization(AutenticacionDelReceptor.Politica);
        }
    }

    private sealed record Clasificacion(EstadoDeRecepcion Estado, string? Motivo, IReadOnlyDictionary<string, string>? Errores, string? Aviso);

    private static Clasificacion Clasificar(EnvioDeRespuesta envio, RegistroDeDefiniciones registro)
    {
        var consulta = registro.Buscar(envio.Definicion);
        switch (consulta.Situacion)
        {
            case RegistroDeDefiniciones.Situacion.NoPublicada:
                return new(EstadoDeRecepcion.CartaMuerta, $"la definición {envio.EncuestaId} v{envio.VersionDefinicion} no está publicada", null, null);
            case RegistroDeDefiniciones.Situacion.Retirada:
                return new(EstadoDeRecepcion.CartaMuerta, $"la definición {envio.EncuestaId} v{envio.VersionDefinicion} se retiró el {consulta.RetiroEl:yyyy-MM-dd} (P13.5)", null, null);
        }

        var errores = ValidacionDeValores.Validar(consulta.Definicion!, envio.Valores);
        if (errores.Count > 0)
            return new(EstadoDeRecepcion.CartaMuerta, $"los valores no cumplen la definición v{envio.VersionDefinicion}", errores, null);

        var aviso = consulta.RetiroEl is { } fecha ? $"esta versión de la definición se retira el {fecha:yyyy-MM-dd}" : null;
        return new(EstadoDeRecepcion.Aceptada, null, null, aviso);
    }

    private static RespuestaRecibida Nueva(EnvioDeRespuesta envio, Clasificacion c, int version, DateTimeOffset recibidaEn, DateTimeOffset? modificadaEn, int reenvios) =>
        new(envio.ClaveIdempotencia, version, c.Estado, c.Motivo, c.Errores, c.Aviso, recibidaEn, modificadaEn, reenvios, envio);

    private static int StatusDe(RespuestaRecibida r, int siAceptada) =>
        r.Estado == EstadoDeRecepcion.CartaMuerta ? 422 : siAceptada;

    private static IResult Respuesta(HttpContext http, int status, RespuestaRecibida r, bool duplicada)
    {
        http.Response.Headers.ETag = Etiqueta(r.Version);
        if (status == 201) http.Response.Headers.Location = $"/respuestas/{Uri.EscapeDataString(r.Clave)}";
        return Results.Json(new
        {
            clave = r.Clave,
            estado = r.Estado,
            dueno = r.Dueno,
            version = r.Version,
            duplicada,
            motivo = r.Motivo,
            errores = r.Errores,
            aviso = r.Aviso
        }, JsonDelContrato.Opciones, statusCode: status);
    }

    private static IResult Problema(int status, string detalle, RespuestaRecibida? actual = null) =>
        Results.Json(new { status, detalle, versionActual = actual?.Version }, JsonDelContrato.Opciones, statusCode: status);

    private static string Etiqueta(int version) => $"\"{version}\"";

    /// <summary>
    /// If-Match: una lista de ETags («"3"», «W/"3"»). Corrector E03 — «*» NO se acepta: modificar exige conocer la
    /// versión (D-04); con «*» cualquiera pisaba sin haber leído.
    /// </summary>
    private static bool Coincide(string ifMatch, int version) =>
        ifMatch.Split(',').Select(e => e.Trim().Replace("W/", "", StringComparison.Ordinal).Trim('"'))
                  .Contains(version.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
