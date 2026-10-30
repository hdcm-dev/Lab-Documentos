using System.Text.Json;
using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Auth;
using FrameworkConcept.Shell.Catalogo;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Hosting;
using FrameworkConcept.Shell.Red;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Bridge;

/// <summary>
/// Atiende cada sobre que llega por <see cref="PuenteDelCascaron.Call"/>: lo valida contra la tabla de métodos del
/// contrato (Core/Contracts/Puente/TablaDeMetodos.cs) y lo resuelve. Un caso por fila de la tabla.
///
/// Validación, en orden (P3):
///   1. JSON-RPC 2.0 con id y method          → si no, PedidoInvalido (-32600) o JsonInvalido (-32700)
///   2. el mayor de «v» es el de este puente   → si no, VersionDelPuenteIncompatible (-32001)
///   3. el método está en la tabla              → si no, MetodoInexistente (-32601)
///   4. su capacidad está en el catálogo        → si no, CapacidadNoDisponible (-32002)
///   5. vienen los parámetros obligatorios      → si no, ParametrosInvalidos (-32602)
///   6. si el método es una acción del negocio  → la guardia la evalúa; si no está permitida, AccionBloqueada (-32005)
///      (borrador §7: el cascarón evalúa; la interfaz sólo deshabilita el botón)
///
/// Folio 038 — Los métodos de red y acciones los resuelve Red/AtencionDeRed.cs; los auth.*, Auth/AtencionDeSesion.cs.
/// </summary>
public sealed class DespachadorDelPuente(
    IDb db,
    AlmacenDeDefinicionesSqlite definiciones,
    ProveedorDelActualizador proveedor,
    ContenidoActivo contenido,
    SincronizadorDelOutbox sincronizador,
    EmisorDeEventos emisor,
    ISesionDelUsuario sesion,
    IGuardiaDeAcciones guardia,
    AtencionDeRed atencionDeRed,
    AtencionDeSesion atencionDeSesion,
    PuertaDelArranque puerta,
    ServidorDeContenido servidor)
{
    /// <summary>Colección de documentos donde quedan las respuestas de encuesta, junto a su fila del outbox.</summary>
    public const string ColeccionDeRespuestas = "respuestas";

    /// <summary>El contenido confirmó «Listo(version)». El arranque lo usa para lanzar la consulta del canal (P2).</summary>
    public event Action<VersionSemantica>? ListoRecibido;

    public async Task<string> AtenderAsync(string textoDelSobre)
    {
        SobrePedido? pedido;
        try
        {
            pedido = JsonDelContrato.Leer<SobrePedido>(textoDelSobre);
        }
        catch (JsonException ex)
        {
            return Responder(Fallo("", CodigosDeError.JsonInvalido, "El sobre no es JSON válido: " + ex.Message));
        }

        var id = pedido.Id ?? "";
        try
        {
            var error = Validar(pedido);
            if (error is not null) return Responder(error);

#if FC_E04_V2
            // E04 V2 — La sesión se restaura en segundo plano: lo que depende de ella (outbox.status cuenta por dueño,
            // auth.*, documents.*, la guardia de acciones) espera a que termine. El saludo y network.status no esperan.
            if (!pedido.Method.StartsWith("bridge.", StringComparison.Ordinal) && pedido.Method != "network.status")
                await puerta.SegundoPlano;
#endif

            // Borrador §7 — La acción del negocio que ejecuta este método, si lo es, se evalúa ANTES de ejecutarla.
            if (PoliticaDeAcciones.DelMetodo(pedido.Method) is { } accion && guardia.Evaluar(accion.Id) is { Permitida: false } evaluacion)
                return Responder(Bloqueada(id, evaluacion));

            var parametros = pedido.Params is { ValueKind: JsonValueKind.Object } p ? p : default;
            var resultado = await ResolverAsync(pedido.Method, parametros);
            return Responder(SobreRespuesta.Exito(VersionDelPuente, id, JsonSerializer.SerializeToElement(resultado, JsonDelContrato.Opciones)));
        }
        catch (ConflictoDeVersionException ex)
        {
            return Responder(Fallo(id, CodigosDeError.ConflictoDeVersion, ex.Message));
        }
        catch (ExcepcionDeAutenticacion ex)
        {
            // Folio 038 — auth.* cancelado (-32006) o fallido (-32007), DISENO §6.4.
            return Responder(Fallo(id, ex.Codigo, ex.Message));
        }
        catch (Exception ex) when (ex is JsonException or FormatException or ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            return Responder(Fallo(id, CodigosDeError.ParametrosInvalidos, ex.Message));
        }
        catch (Exception ex)
        {
            Bitacora.Error($"puente {pedido.Method}", ex);
            return Responder(Fallo(id, CodigosDeError.ErrorInterno, ex.Message));
        }
    }

    private static string VersionDelPuente => ContratoDelPuente.Version.ToString();

    private static SobreRespuesta? Validar(SobrePedido pedido)
    {
        var id = pedido.Id ?? "";
        if (pedido.Jsonrpc != SobrePedido.VersionJsonRpc || string.IsNullOrEmpty(pedido.Id) || string.IsNullOrEmpty(pedido.Method))
            return Fallo(id, CodigosDeError.PedidoInvalido, "Falta jsonrpc «2.0», id o method.");

        if (!VersionSemantica.TryParse(pedido.V, out var v) || v.Mayor != ContratoDelPuente.Version.Mayor)
            return Fallo(id, CodigosDeError.VersionDelPuenteIncompatible,
                $"El contenido espera el puente «{pedido.V}» y este cascarón implementa el {ContratoDelPuente.Version}.");

        var metodo = TablaDeMetodos.Buscar(pedido.Method);
        if (metodo is null)
            return Fallo(id, CodigosDeError.MetodoInexistente, $"«{pedido.Method}» no está en la tabla de métodos.");

        if (metodo.Capacidad is { } capacidad
            && (CatalogoDeCapacidades.Buscar(capacidad) is null || !ImplementacionesDelCatalogo.Implementa(capacidad)))
            return Fallo(id, CodigosDeError.CapacidadNoDisponible, $"Este cascarón no tiene la capacidad «{capacidad}».");

        var parametros = pedido.Params is { ValueKind: JsonValueKind.Object } p ? p : default;
        foreach (var campo in metodo.Parametros.Where(c => !c.Opcional))
        {
            if (parametros.ValueKind != JsonValueKind.Object
                || !parametros.TryGetProperty(campo.Nombre, out var valor)
                || valor.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return Fallo(id, CodigosDeError.ParametrosInvalidos, $"Falta el parámetro «{campo.Nombre}» ({campo.Tipo}).");
        }
        return null;
    }

    private async Task<object> ResolverAsync(string metodo, JsonElement p)
    {
        // Folio 038 — Red, acciones y sesión viven en sus carpetas (DISENO §6).
        if (AtencionDeRed.Atiende(metodo)) return await atencionDeRed.ResolverAsync(metodo, p, CancellationToken.None);
        if (AtencionDeSesion.Atiende(metodo)) return await atencionDeSesion.ResolverAsync(metodo, p, CancellationToken.None);

        switch (metodo)
        {
            // --- Arranque (P2, P3) ---
            case "bridge.handshake":
            {
                Bitacora.Log($"puente: saludo recibido (último _framework {servidor.UltimoFramework})");
                // E04 (folio 050) — El saludo se retiene hasta que el actualizador decidió (P2). Sin E04 ya está abierta.
                var decidido = await puerta.Preparado;
#if FC_E04
                // La página que carga una versión especulada distinta de la decidida no habla con el cascarón: se recarga.
                // Sin raíz de carga también se descarta: un saludo legítimo viene del runtime ya servido, así que null es
                // el de la página vieja que saluda entre Reiniciar() y el dotnet.js de la recarga.
                var raiz = servidor.RaizDeLaCarga;
                if (!string.Equals(raiz, decidido.Directorio, StringComparison.Ordinal))
                    throw new SaludoDescartadoException($"El contenido cargó {raiz ?? "(sin runtime servido)"} y el actualizador decidió {decidido.Directorio}.");
#else
                _ = decidido;
#endif
                emisor.Habilitado = true;   // desde acá el contenido ya tiene su función de eventos
                Bitacora.Log("puente: saludo respondido");
                return new RespuestaDeHandshake(ContratoDelPuente.Version, CatalogoDeCapacidades.Versionadas(), ContratoDelPuente.Huella());
            }

            case "bridge.ready":
            {
                // P2 — «Listo(version)»: borra la marca «pendiente». Sin esta llamada, el próximo arranque vuelve a previous.
                var version = VersionSemantica.Parse(Texto(p, "version"));
                var actualizador = await proveedor.ObtenerAsync();
                await actualizador.ConfirmarListoAsync(version);
                await db.AnotarVersionDeContenidoAsync(version.ToString(), "listo");
                var aceptado = version.Equals(contenido.Version);
                Bitacora.Log($"puente Listo({version}){(aceptado ? "" : $" — la activa es {contenido.Version}")}");
                Bitacora.Log($"servidor: último _framework {servidor.UltimoFramework}");
                ListoRecibido?.Invoke(version);
                return new { aceptado };
            }

            // --- Documentos del contenido (P5) ---
            case "documents.get":
            {
                var fila = await db.ObtenerDocumentoAsync(Texto(p, "coleccion"), Texto(p, "id"));
                return fila is null ? new { } : (object)new { documento = ADocumento(fila) };
            }

            case "documents.list":
                return new { documentos = (await db.ListarDocumentosAsync(Texto(p, "coleccion"))).Select(ADocumento).ToList() };

            case "documents.put":
            {
                int? esperada = p.TryGetProperty("versionEsperada", out var e) && e.ValueKind == JsonValueKind.Number ? e.GetInt32() : null;
                var version = await db.GuardarDocumentoAsync(
                    Texto(p, "coleccion"), Texto(p, "id"), p.GetProperty("versionDeForma").GetInt32(),
                    p.GetProperty("json").GetRawText(), esperada);
                return new { version };
            }

            case "documents.delete":
                return new { borrado = await db.BorrarDocumentoAsync(Texto(p, "coleccion"), Texto(p, "id")) };

            // --- Encuestas (P1, P13) ---
            case "encuestas.getDefinicion":
            {
                // P13.2 — para EMPEZAR una encuesta: la más nueva que el contenido activo sabe dibujar.
                var encuestaId = Texto(p, "encuestaId");
                var definicion = await definiciones.ObtenerParaEmpezarAsync(encuestaId, contenido.Version)
                    ?? throw new KeyNotFoundException($"No hay una definición de «{encuestaId}» para el contenido {contenido.Version}.");
                return new { definicion };
            }

            case "encuestas.enqueueRespuesta":
                return await EncolarRespuestaAsync(p.GetProperty("envio"));

            case "outbox.status":
            {
                var (pendientes, rechazadas) = await db.ContarOutboxDeAsync(sesion.DuenoVisible);   // P4 — sólo lo del dueño visible
                return new { pendientes, rechazadas };
            }

            // --- Capacidades del dispositivo: sólo por el puente (P12) ---
            default:
                if (ImplementacionesDelCatalogo.Todas.TryGetValue(metodo, out var capacidad))
                    return capacidad();
                throw new InvalidOperationException($"«{metodo}» está en la tabla y este cascarón no lo implementa.");
        }
    }

    /// <summary>
    /// P5, P13 — La respuesta y su fila del outbox, en UNA transacción. La fila guarda desde ahora la acción, la versión
    /// del payload, la versión de contenido y {encuestaId, versionDefinicion}; nada de eso cambia después.
    /// </summary>
    private async Task<object> EncolarRespuestaAsync(JsonElement envioJson)
    {
        var envio = envioJson.Deserialize<EnvioDeRespuesta>(JsonDelContrato.Opciones)
            ?? throw new JsonException("envio vacío");
        if (string.IsNullOrWhiteSpace(envio.ClaveIdempotencia))
            throw new ArgumentException("El envío no trae claveIdempotencia.");

        // P13.3 — la definición con que se tomó tiene que estar guardada acá (se conserva mientras haya pendientes).
        if (await db.ObtenerDefinicionAsync(envio.EncuestaId, envio.VersionDefinicion) is null)
            throw new ArgumentException($"La definición {envio.EncuestaId} v{envio.VersionDefinicion} no está en este teléfono.");

        // La versión de contenido la pone el cascarón, que sabe cuál está corriendo (atribución, folio 023 ítem 4).
        if (contenido.Version is { } activa && !activa.Equals(envio.VersionContenido))
            envio = envio with { VersionContenido = activa };

        var payload = JsonDelContrato.Escribir(envio);
        var documento = new FilaDeDocumento
        {
            Coleccion = ColeccionDeRespuestas,
            Id = envio.ClaveIdempotencia,
            VersionDeForma = 1,
            Json = payload
        };
        var fila = new FilaDeOutbox
        {
            ClaveIdempotencia = envio.ClaveIdempotencia,
            Dueno = await sesion.ObtenerDuenoAsync(),   // P4 — la identidad presente (en modo Dispositivo, la instalación)
            Accion = ClienteDelReceptor.AccionCrearRespuesta,
            Entidad = $"{ColeccionDeRespuestas}/{envio.ClaveIdempotencia}",
            VersionPayload = 1,
            VersionContenido = envio.VersionContenido.ToString(),
            EncuestaId = envio.EncuestaId,
            VersionDefinicion = envio.VersionDefinicion,
            Payload = payload,
            Estado = EstadoDeFila.Pendiente
        };

        var encolada = await db.GuardarYEncolarAsync(documento, fila);
        Bitacora.Log($"outbox: {(encolada ? "encolada" : "ya estaba")} {envio.ClaveIdempotencia} ({envio.EncuestaId} v{envio.VersionDefinicion})");
        if (encolada) sincronizador.Despertar();
        return new { claveIdempotencia = envio.ClaveIdempotencia, encolada };
    }

    private static Documento ADocumento(FilaDeDocumento f)
    {
        using var json = JsonDocument.Parse(f.Json);
        return new Documento(f.Coleccion, f.Id, f.VersionDeForma, f.Version, json.RootElement.Clone());
    }

    private static string Texto(JsonElement p, string nombre) =>
        p.GetProperty(nombre).GetString() ?? throw new ArgumentException($"«{nombre}» vacío.");

    /// <summary>Borrador §7 — AccionBloqueada: «message» es el motivo para mostrar y «data» la evaluación completa.</summary>
    private static SobreRespuesta Bloqueada(string id, EvaluacionDeAccion evaluacion) =>
        new(SobrePedido.VersionJsonRpc, VersionDelPuente, id, Error: new ErrorDelSobre(
            CodigosDeError.AccionBloqueada,
            evaluacion.Motivo ?? $"La acción «{evaluacion.Accion}» no está disponible ahora.",
            JsonSerializer.SerializeToElement(evaluacion, JsonDelContrato.Opciones)));

    private static SobreRespuesta Fallo(string id, int codigo, string mensaje) =>
        SobreRespuesta.Fallo(VersionDelPuente, id, codigo, mensaje);

    private static string Responder(SobreRespuesta respuesta) => JsonDelContrato.Escribir(respuesta);
}

/// <summary>E04 — El saludo llegó de una versión especulada que el actualizador no eligió: responde ErrorInterno y se recarga.</summary>
public sealed class SaludoDescartadoException(string mensaje) : Exception(mensaje);
