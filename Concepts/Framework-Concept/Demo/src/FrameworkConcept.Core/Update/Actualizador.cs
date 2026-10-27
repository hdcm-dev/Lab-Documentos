using System.Text.Json;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// El actualizador del cascarón (P2, P3, P13). Lee el contrato en <see cref="IActualizador"/>; acá está cómo se cumple.
///
/// La idea central: BAJAR no es ACTIVAR.
///   — <see cref="ConsultarAsync"/> baja y verifica, y deja lo nuevo en staging/. Corre después de mostrar.
///   — <see cref="PrepararArranqueAsync"/> activa lo que ya está verificado, en el arranque en frío, antes de mostrar,
///     y sin red. Así el contenido nunca cambia mientras la persona lo está usando.
///
/// Todo cambio de estado que importa queda en puntero.json, que se reemplaza con un solo rename: si el proceso se corta
/// en cualquier paso, el próximo arranque encuentra el puntero viejo o el nuevo, y los dos apuntan a una versión
/// completa (lo prueban los «cortes provocados» de FrameworkConcept.Update.Tests).
/// </summary>
public sealed class Actualizador(
    IAlmacenDeContenido almacen,
    IAlmacenDeDefiniciones definiciones,
    IClienteDeCanal canal,
    IVerificadorDeFirma verificador,
    IPredicadoDeCompatibilidad predicado,
    IEntornoDelCascaron entorno,
    IReloj reloj,
    IFuenteDeSemilla? fuenteDeSemilla = null,
    IEstadoDeRed? red = null) : IActualizador
{
    /// <summary>Un paquete más grande que esto no se baja: protege la memoria del teléfono de un índice mal escrito.</summary>
    public const long TamanoMaximoDePaquete = 64L * 1024 * 1024;

    /// <summary>
    /// P2 — Cuántos arranques con la carga EMPEZADA y sin «Listo» hacen falta para dar una versión por fallida.
    /// Criterio de la demo (corrección de E02), no valor de producto del folio 036: con 1, cerrar la app mientras
    /// carga (el TPR del teléfono de medida pasa de 8 s) dejaba una versión sana como fallida para siempre.
    /// </summary>
    public const int CargasSinListoParaFallar = 2;

    // Dos candados, siempre en este orden: _operacion (arranque y consulta no se pisan) y _puntero (toda lectura y
    // escritura de puntero.json; «Listo» sólo toma éste, así no espera a que termine una descarga).
    private readonly SemaphoreSlim _operacion = new(1, 1);
    private readonly SemaphoreSlim _puntero = new(1, 1);

    // ───────────────────────────── Arranque en frío ─────────────────────────────

    public async Task<ResultadoDeArranque> PrepararArranqueAsync(CancellationToken cancellationToken = default)
    {
        await _operacion.WaitAsync(cancellationToken);
        await _puntero.WaitAsync(cancellationToken);
        try
        {
            return await PrepararAsync(cancellationToken);
        }
        finally
        {
            _puntero.Release();
            _operacion.Release();
        }
    }

    private async Task<ResultadoDeArranque> PrepararAsync(CancellationToken ct)
    {
        var notas = new List<string>();
        var indice = await almacen.LeerUltimoIndiceValidoAsync(ct);

        // 1. P2 — Política de soporte (no es compatibilidad): sólo bloquea con red, para que haya cómo actualizar.
        if (indice?.CascaronMinimoSoportado is { } minimo && entorno.VersionDelCascaron < minimo)
        {
            if (red?.HayRed == true)
                return new ResultadoDeArranque(EstadoDeArranque.CascaronNoSoportado, null, null,
                    [$"cascarón {entorno.VersionDelCascaron} bajo el mínimo soportado {minimo}"]);
            notas.Add($"cascarón bajo el mínimo soportado {minimo}, pero sin red no se bloquea");
        }

        // 2. C-3 de E01 — Con un WebView viejo el contenido Blazor no corre: pantalla propia «actualizá el WebView».
        //    Si no se pudo leer la versión, se sigue: el predicado ya exigió el mínimo al activar.
        if (entorno.VersionMayorDelWebView is int webView && webView < ParametrosDeProducto.WebViewMinimo)
            return new ResultadoDeArranque(EstadoDeArranque.MotorDesactualizado, null, null,
                [$"WebView {webView} < mínimo {ParametrosDeProducto.WebViewMinimo}"]);

        var leido = await almacen.LeerPunteroAsync(ct);
        var primerArranque = leido is null;   // nunca hubo puntero: el único caso en que la semilla es el punto de partida
        var puntero = leido ?? new PunteroDeContenido(null, null, null);
        var fallidas = new HashSet<VersionSemantica>(await almacen.LeerFallidasAsync(ct));

        bool Retirada(VersionSemantica v) => indice is not null && PorPrecedencia(indice.Retiradas, v);
        bool Sana(VersionSemantica? v) =>
            v is not null && !PorPrecedencia(fallidas, v) && !Retirada(v) && almacen.ExisteVersion(v);

        // 3. P2 — Rollback por marca: el contenido activado no llamó a Listo(version). Cuenta como falla sólo si el
        //    WebView llegó a cargarlo (CargasSinListo, que anota AnotarCargaIniciadaAsync) en CargasSinListoParaFallar
        //    arranques: un arranque cortado antes de cargar no prueba nada y la versión se vuelve a intentar.
        //    Primero se anota la fallida y después se mueve el puntero: si el proceso se corta entre los dos, el próximo
        //    arranque encuentra la misma marca y repite este paso, sin reinstalar la mala.
        if (puntero.Pendiente is { } pendiente)
        {
            if (puntero.CargasSinListo >= CargasSinListoParaFallar)
            {
                await almacen.AgregarFallidaAsync(pendiente, ct);
                fallidas.Add(pendiente);
                notas.Add($"rollback: {pendiente} empezó a cargar {puntero.CargasSinListo} veces sin confirmar Listo; queda como fallida");
                puntero = puntero with { Pendiente = null, CargasSinListo = 0 };
                await almacen.ReemplazarPunteroAsync(puntero, ct);
            }
            else
            {
                notas.Add($"{pendiente} sigue pendiente de Listo ({puntero.CargasSinListo} de {CargasSinListoParaFallar} cargas sin Listo); se reintenta");
            }
        }

        // 4. P2 — Retiradas: una versión retirada en el índice se trata como fallida local, en este arranque en frío.
        if (puntero.Current is { } actual && Retirada(actual) && !PorPrecedencia(fallidas, actual))
        {
            await almacen.AgregarFallidaAsync(actual, ct);
            fallidas.Add(actual);
            notas.Add($"retirada: {actual} figura en «retiradas» del índice; queda como fallida");
        }

        // 5. Si current no está sana, se vuelve a previous si previous sí lo está. Si ninguna está sana, el puntero queda
        //    como está (con las versiones a la vista para la pantalla terminal) y NO se vuelve a la semilla.
        if (!primerArranque && !Sana(puntero.Current))
        {
            if (Sana(puntero.Previous))
            {
                // Volver a previous también es activar: lleva su marca «pendiente». Si tampoco llega a Listo, el
                // próximo arranque la anota como fallida y, sin otra sana, queda el estado terminal (doble falla).
                notas.Add($"vuelve a previous {puntero.Previous}");
                puntero = puntero with { Current = puntero.Previous, Previous = null, Pendiente = puntero.Previous, CargasSinListo = 0 };
                await almacen.ReemplazarPunteroAsync(puntero, ct);
            }
            else
            {
                notas.Add($"sin versión sana: current {Texto(puntero.Current)}, previous {Texto(puntero.Previous)}");
            }
        }

        // 6. Activación de lo verificado en staging/ (P2: una sola regla, en el arranque en frío y antes de mostrar).
        //    Mientras current espera su Listo, lo nuevo queda en staging/: si se activara, previous sería una versión
        //    sin confirmar y el rollback no tendría a dónde volver con seguridad.
        var activada = false;
        var esperaListo = puntero.Pendiente is not null;
        if (esperaListo && await almacen.VersionEnStagingAsync(ct) is { } enEspera)
        {
            notas.Add($"staging {enEspera} espera: {puntero.Pendiente} todavía no confirmó Listo");
        }
        else if (await almacen.VersionEnStagingAsync(ct) is { } enStaging)
        {
            var motivo = await MotivoParaNoActivarAsync(enStaging, puntero.Current, fallidas, Retirada, ct);
            if (motivo is null)
            {
                puntero = await ActivarAsync(enStaging, puntero, Sana, ct);
                activada = true;
                notas.Add($"activada {enStaging} (desde staging)");
            }
            else
            {
                await almacen.LimpiarStagingAsync(ct);
                notas.Add($"staging {enStaging} descartada: {motivo}");
            }
        }

        // 7. P2 — La semilla: en el primer arranque, o si un cascarón nuevo (por tienda) trae una más nueva que lo
        //    instalado. Se instala por el mismo camino que un paquete bajado.
        //    P1 — Leer y verificar la semilla cuesta segundos: sólo se carga en el primer arranque o cuando cambió la
        //    huella de la instalación del cascarón. Con la misma instalación, ya se revisó y no hay nada nuevo.
        var revisarSemilla = fuenteDeSemilla is not null
            && (primerArranque || puntero.SemillaRevisada != fuenteDeSemilla.Huella);
        var semilla = revisarSemilla ? await fuenteDeSemilla!.CargarAsync(ct) : null;
        if (!revisarSemilla && fuenteDeSemilla is not null)
            notas.Add("semilla: ya revisada para esta instalación del cascarón; no se carga");
        if (!activada && !esperaListo && semilla is not null
            && (primerArranque || (puntero.Current is not null && semilla.Version > puntero.Current)))
        {
            var motivo = await InstalarSemillaEnStagingAsync(semilla, fallidas, ct)
                ?? await MotivoParaNoActivarAsync(semilla.Version, puntero.Current, fallidas, Retirada, ct);
            if (motivo is null)
            {
                puntero = await ActivarAsync(semilla.Version, puntero, Sana, ct);
                notas.Add($"activada la semilla {semilla.Version}");
            }
            else
            {
                await almacen.LimpiarStagingAsync(ct);
                notas.Add($"semilla {semilla.Version} no instalada: {motivo}");
            }
        }
        // Las definiciones de la semilla se guardan si son más nuevas que las guardadas (nunca bajan, P13.9).
        if (semilla is not null)
        {
            await GuardarDefinicionesDeLaSemillaAsync(semilla, ct);
            // Se anota la revisión sólo con algo instalado: si la semilla no se pudo instalar en el primer arranque,
            // el próximo la vuelve a intentar (no queda un puntero vacío que la saltee).
            if (puntero.Current is not null && !esperaListo)
            {
                puntero = puntero with { SemillaRevisada = fuenteDeSemilla!.Huella };
                await almacen.ReemplazarPunteroAsync(puntero, ct);
            }
        }

        // 8. P2 — Retención del teléfono: current y previous; el resto se borra. También borra lo que haya dejado
        //    una activación cortada entre «promover» y «cambiar el puntero» (una versión que nunca estuvo activa).
        foreach (var version in await almacen.ListarVersionesAsync(ct))
        {
            if (!version.Equals(puntero.Current) && !version.Equals(puntero.Previous))
            {
                await almacen.BorrarVersionAsync(version, ct);
                notas.Add($"retención: borrada {version}");
            }
        }

        return Sana(puntero.Current)
            ? new ResultadoDeArranque(EstadoDeArranque.ContenidoListo, puntero.Current, almacen.DirectorioDe(puntero.Current!), notas)
            : new ResultadoDeArranque(EstadoDeArranque.SinContenidoSano, null, null, notas);
    }

    /// <summary>
    /// staging/vN → versions/vN → puntero con la marca «pendiente». El orden es el que hace segura la interrupción:
    /// hasta que el puntero cambia (un rename), lo activo sigue siendo lo anterior.
    /// </summary>
    private async Task<PunteroDeContenido> ActivarAsync(
        VersionSemantica version, PunteroDeContenido puntero, Func<VersionSemantica?, bool> sana, CancellationToken ct)
    {
        // Una versions/vN que ya existe y no es current es la huella de una activación cortada: nunca estuvo activa,
        // así que reemplazarla no rompe la inmutabilidad (que es de lo que alguna vez se mostró).
        if (almacen.ExisteVersion(version))
            await almacen.BorrarVersionAsync(version, ct);

        await almacen.PromoverAsync(version, ct);

        var nuevo = puntero with
        {
            Current = version,
            Previous = sana(puntero.Current) ? puntero.Current : null,
            Pendiente = version,
            CargasSinListo = 0
        };
        await almacen.ReemplazarPunteroAsync(nuevo, ct);
        return nuevo;
    }

    /// <summary>null si se puede activar; si no, por qué no (re-evaluado en el arranque: el entorno pudo cambiar).</summary>
    private async Task<string?> MotivoParaNoActivarAsync(
        VersionSemantica version, VersionSemantica? actual, HashSet<VersionSemantica> fallidas,
        Func<VersionSemantica, bool> retirada, CancellationToken ct)
    {
        if (PorPrecedencia(fallidas, version)) return "fallida en este teléfono";
        if (retirada(version)) return "retirada";
        if (actual is not null && version <= actual) return $"no es mayor que la instalada {actual}";

        var manifiesto = await LeerManifiestoAsync(version, ct);
        if (manifiesto is null || !manifiesto.Version.Equals(version)) return "manifiesto ausente o de otra versión";

        var compatibilidad = predicado.Evaluar(manifiesto, entorno);   // P3
        return compatibilidad.EsCompatible ? null : "incompatible: " + string.Join("; ", compatibilidad.Motivos);
    }

    private async Task<string?> InstalarSemillaEnStagingAsync(
        ISemillaDeContenido s, HashSet<VersionSemantica> fallidas, CancellationToken ct)
    {
        if (PorPrecedencia(fallidas, s.Version)) return "fallida en este teléfono";

        await using var flujo = await s.AbrirAsync(ct);
        var zip = await LeerHastaAsync(flujo, s.Size + 1, ct);
        if (zip.Length != s.Size) return $"tamaño {zip.Length} ≠ {s.Size}";
        if (!string.Equals(VerificacionDelIndice.Sha256Hex(zip), s.Sha256, StringComparison.OrdinalIgnoreCase))
            return "sha256 distinto";

        try
        {
            await almacen.DescomprimirEnStagingAsync(s.Version, new MemoryStream(zip, writable: false), ct);
        }
        catch (InvalidDataException ex)
        {
            return "zip inválido: " + ex.Message;
        }
        return null;
    }

    private async Task GuardarDefinicionesDeLaSemillaAsync(ISemillaDeContenido s, CancellationToken ct)
    {
        foreach (var definicion in s.Definiciones)
        {
            var guardada = await definiciones.ObtenerVigenteAsync(definicion.EncuestaId, ct);
            if (guardada is null || definicion.Version > guardada.Version)
                await definiciones.GuardarAsync(definicion, ct);
        }
    }

    // ───────────────────────────── Listo(version) ─────────────────────────────

    public async Task ConfirmarListoAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        await _puntero.WaitAsync(cancellationToken);
        try
        {
            var puntero = await almacen.LeerPunteroAsync(cancellationToken);
            // Sólo borra la marca de ESA versión: un Listo viejo o de otra versión no confirma nada.
            if (puntero?.Pendiente is { } pendiente && pendiente.Equals(version))
                await almacen.ReemplazarPunteroAsync(puntero with { Pendiente = null, CargasSinListo = 0 }, cancellationToken);
        }
        finally
        {
            _puntero.Release();
        }
    }

    public async Task AnotarCargaIniciadaAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        await _puntero.WaitAsync(cancellationToken);
        try
        {
            var puntero = await almacen.LeerPunteroAsync(cancellationToken);
            // Sólo cuenta para la versión pendiente: una que ya avisó Listo no tiene nada que probar.
            if (puntero?.Pendiente is { } pendiente && pendiente.Equals(version))
                await almacen.ReemplazarPunteroAsync(puntero with { CargasSinListo = puntero.CargasSinListo + 1 }, cancellationToken);
        }
        finally
        {
            _puntero.Release();
        }
    }

    // ───────────────────────────── Consulta al canal ─────────────────────────────

    public async Task<ResultadoDeConsulta> ConsultarAsync(DisparadorDeConsulta disparador, CancellationToken cancellationToken = default)
    {
        await _operacion.WaitAsync(cancellationToken);
        try
        {
            return await ConsultarSinCandadoAsync(disparador, cancellationToken);
        }
        finally
        {
            _operacion.Release();
        }
    }

    private async Task<ResultadoDeConsulta> ConsultarSinCandadoAsync(DisparadorDeConsulta disparador, CancellationToken ct)
    {
        var puntero = await almacen.LeerPunteroAsync(ct);
        var ahora = reloj.Ahora;

        // P2 — Al reanudar, como mucho una consulta cada 15 minutos desde la última EXITOSA.
        if (disparador == DisparadorDeConsulta.Reanudacion
            && puntero?.UltimaConsultaExitosa is { } ultima
            && ahora - ultima < ParametrosDeProducto.IntervaloMinimoAlReanudar)
            return Resultado(EstadoDeConsulta.Omitida, MotivoDeRechazo.Ninguno, "reanudación antes de 15 minutos");

        var mayor = entorno.VersionDelPuente.Mayor;   // P2 — cada cascarón lee el índice de SU mayor del puente
        byte[] indiceJson, firmaJson;
        try
        {
            indiceJson = await canal.ObtenerBytesAsync(RutasDelCanal.Indice(entorno.Canal, mayor), ct);
            firmaJson = await canal.ObtenerBytesAsync(RutasDelCanal.FirmaDelIndice(entorno.Canal, mayor), ct);
        }
        catch (CanalNoDisponibleException ex)
        {
            return Resultado(EstadoDeConsulta.SinRed, MotivoDeRechazo.Ninguno, ex.Message);   // P2 — sin red no se fuerza nada
        }

        var verificado = VerificacionDelIndice.Verificar(indiceJson, firmaJson, verificador, entorno.Canal, mayor, ahora);
        if (!verificado.EsValido)
            return Resultado(EstadoDeConsulta.Rechazado, verificado.Motivo, verificado.Detalle);

        var indice = verificado.Indice!;
        await almacen.GuardarUltimoIndiceValidoAsync(indice, ct);   // retiradas y política de soporte, para el arranque
        await MarcarConsultaExitosaAsync(ahora, ct);

        // P1, P2 — Contenido y definiciones se aceptan POR SEPARADO: un rechazo de uno no frena al otro.
        var notas = new List<string>();
        var rechazos = new List<MotivoDeRechazo>();
        var sinRed = false;

        var contenido = await ConsultarContenidoAsync(indice, puntero?.Current, ct);
        notas.Add(contenido.Detalle);
        if (contenido.Motivo != MotivoDeRechazo.Ninguno) rechazos.Add(contenido.Motivo);
        sinRed |= contenido.SinRed;

        var nuevas = new List<ReferenciaDeDefinicion>();
        foreach (var entrada in indice.Entradas.Definiciones.GroupBy(d => d.EncuestaId).Select(g => g.MaxBy(d => d.Version)!))
        {
            var definicion = await ConsultarDefinicionAsync(entrada, indice, puntero?.Current, ahora, ct);
            notas.Add(definicion.Detalle);
            if (definicion.Motivo != MotivoDeRechazo.Ninguno) rechazos.Add(definicion.Motivo);
            if (definicion.Guardada is not null) nuevas.Add(definicion.Guardada);
            sinRed |= definicion.SinRed;
        }

        var estado = contenido.Preparada is not null || nuevas.Count > 0 ? EstadoDeConsulta.Preparado
            : rechazos.Count > 0 ? EstadoDeConsulta.Rechazado
            : sinRed ? EstadoDeConsulta.SinRed
            : EstadoDeConsulta.AlDia;

        return new ResultadoDeConsulta(estado, rechazos.FirstOrDefault(), string.Join("; ", notas), contenido.Preparada, nuevas);
    }

    private sealed record DecisionDeContenido(VersionSemantica? Preparada, MotivoDeRechazo Motivo, string Detalle, bool SinRed = false);

    private async Task<DecisionDeContenido> ConsultarContenidoAsync(IndiceDelCanal indice, VersionSemantica? actual, CancellationToken ct)
    {
        var entrada = indice.Entradas.Contenido
            .Where(e => !PorPrecedencia(indice.Retiradas, e.Version))
            .MaxBy(e => e.Version);
        if (entrada is null) return new(null, MotivoDeRechazo.Ninguno, "contenido: el índice no ofrece versiones");

        var version = entrada.Version;
        if (version.EsPreRelease)   // P7 — el contenido no lleva pre-release en ningún canal
            return new(null, MotivoDeRechazo.ContenidoConPreRelease, $"contenido: {version} es pre-release");

        // P2 — Anti-rollback: estrictamente mayor que la instalada, por precedencia SemVer (C06 versión vieja, C07 replay).
        if (actual is not null && version <= actual)
            return version.CompareTo(actual) == 0
                ? new(null, MotivoDeRechazo.Ninguno, $"contenido: al día ({actual})")
                : new(null, MotivoDeRechazo.VersionNoMayor, $"contenido: {version} <= instalada {actual}");

        if (PorPrecedencia(await almacen.LeerFallidasAsync(ct), version))
            return new(null, MotivoDeRechazo.FallidaLocal, $"contenido: {version} ya falló en este teléfono; no se reinstala");

        if (await almacen.VersionEnStagingAsync(ct) is { } enStaging && enStaging.Equals(version))
            return new(version, MotivoDeRechazo.Ninguno, $"contenido: {version} ya está preparada");

        if (entrada.Size <= 0 || entrada.Size > TamanoMaximoDePaquete)
            return new(null, MotivoDeRechazo.TamanoDistinto, $"contenido: size {entrada.Size} fuera de lo admitido");

        await almacen.LimpiarStagingAsync(ct);

        byte[] zip;
        try
        {
            await using var flujo = await canal.AbrirAsync(RutasDelCanal.Archivo(entorno.Canal, indice.PuenteMayor, entrada.Ruta), ct);
            zip = await LeerHastaAsync(flujo, entrada.Size + 1, ct);   // uno de más: así se nota si sobra
        }
        catch (Exception ex) when (ex is CanalNoDisponibleException or IOException)
        {
            return new(null, MotivoDeRechazo.Ninguno, $"contenido: descarga cortada ({ex.Message}); sigue lo instalado", SinRed: true);
        }

        // P2 — tamaño y sha256 ANTES de descomprimir: lo que no coincide no llega a tocar staging/ (C05).
        if (zip.Length != entrada.Size)
            return new(null, MotivoDeRechazo.TamanoDistinto, $"contenido: {zip.Length} bytes, el índice dice {entrada.Size}");

        var hash = VerificacionDelIndice.Sha256Hex(zip);
        if (!string.Equals(hash, entrada.Sha256, StringComparison.OrdinalIgnoreCase))
            return new(null, MotivoDeRechazo.HashDistinto, $"contenido: sha256 {hash} ≠ {entrada.Sha256}");

        try
        {
            await almacen.DescomprimirEnStagingAsync(version, new MemoryStream(zip, writable: false), ct);
        }
        catch (InvalidDataException ex)
        {
            await almacen.LimpiarStagingAsync(ct);
            return new(null, MotivoDeRechazo.PaqueteInvalido, $"contenido: zip inválido ({ex.Message})");
        }

        var manifiesto = await LeerManifiestoAsync(version, ct);
        if (manifiesto is null || !manifiesto.Version.Equals(version))
        {
            await almacen.LimpiarStagingAsync(ct);
            return new(null, MotivoDeRechazo.PaqueteInvalido, $"contenido: el manifiesto del paquete no es de {version}");
        }

        // P3 — El predicado, antes de dejarlo listo para activar. Lo incompatible no queda en staging/.
        var compatibilidad = predicado.Evaluar(manifiesto, entorno);
        if (!compatibilidad.EsCompatible)
        {
            await almacen.LimpiarStagingAsync(ct);
            return new(null, MotivoDeRechazo.Incompatible, "contenido: " + string.Join("; ", compatibilidad.Motivos));
        }

        var obligatoria = entrada.Mandatory ? " (obligatoria)" : "";
        return new(version, MotivoDeRechazo.Ninguno, $"contenido: {version}{obligatoria} preparada; se activa en el próximo arranque en frío");
    }

    private sealed record DecisionDeDefinicion(ReferenciaDeDefinicion? Guardada, MotivoDeRechazo Motivo, string Detalle, bool SinRed = false);

    private async Task<DecisionDeDefinicion> ConsultarDefinicionAsync(
        EntradaDeDefinicion entrada, IndiceDelCanal indice, VersionSemantica? contenidoActual, DateTimeOffset ahora, CancellationToken ct)
    {
        var referencia = new ReferenciaDeDefinicion(entrada.EncuestaId, entrada.Version);
        var nombre = $"definición {entrada.EncuestaId} v{entrada.Version}";

        if (PoliticaDeDefiniciones.EstaRetirada(referencia, indice, ahora))   // P13.5
            return new(null, MotivoDeRechazo.Ninguno, $"{nombre}: retirada, no se baja");

        var guardada = await definiciones.ObtenerVigenteAsync(entrada.EncuestaId, ct);
        if (!PoliticaDeDefiniciones.EsMasNueva(entrada, guardada))   // P13.9 — nunca baja
            return entrada.Version == guardada!.Version
                ? new(null, MotivoDeRechazo.Ninguno, $"{nombre}: al día")
                : new(null, MotivoDeRechazo.VersionNoMayor, $"{nombre} <= guardada v{guardada.Version}");

        // P1 — Se toma sólo si el contenido INSTALADO sabe dibujarla. Si necesita un contenido que todavía está en
        // staging/, se baja en la primera consulta después de activarlo.
        if (contenidoActual is null || !entrada.CompatibleConContenido.Contiene(contenidoActual))
            return new(null, MotivoDeRechazo.Ninguno, $"{nombre}: espera un contenido {entrada.CompatibleConContenido}");

        byte[] bytes;
        try
        {
            bytes = await canal.ObtenerBytesAsync(RutasDelCanal.Archivo(entorno.Canal, indice.PuenteMayor, entrada.Ruta), ct);
        }
        catch (CanalNoDisponibleException ex)
        {
            return new(null, MotivoDeRechazo.Ninguno, $"{nombre}: sin red ({ex.Message})", SinRed: true);
        }

        if (bytes.Length != entrada.Size)
            return new(null, MotivoDeRechazo.TamanoDistinto, $"{nombre}: {bytes.Length} bytes, el índice dice {entrada.Size}");
        if (!string.Equals(VerificacionDelIndice.Sha256Hex(bytes), entrada.Sha256, StringComparison.OrdinalIgnoreCase))
            return new(null, MotivoDeRechazo.HashDistinto, $"{nombre}: sha256 distinto");

        DefinicionEncuesta definicion;
        try
        {
            definicion = DefinicionEncuesta.Leer(System.Text.Encoding.UTF8.GetString(bytes));
        }
        catch (JsonException ex)
        {
            return new(null, MotivoDeRechazo.PaqueteInvalido, $"{nombre}: JSON inválido ({ex.Message})");
        }
        if (definicion.EncuestaId != entrada.EncuestaId || definicion.Version != entrada.Version)
            return new(null, MotivoDeRechazo.PaqueteInvalido, $"{nombre}: el archivo dice {definicion.EncuestaId} v{definicion.Version}");

        // P13.2 — Se guarda; la usa la PRÓXIMA encuesta. La que está en curso sigue con la suya, que no se borra (P13.3).
        await definiciones.GuardarAsync(definicion, ct);
        return new(referencia, MotivoDeRechazo.Ninguno, $"{nombre}: guardada; rige desde la próxima encuesta");
    }

    private async Task MarcarConsultaExitosaAsync(DateTimeOffset cuando, CancellationToken ct)
    {
        await _puntero.WaitAsync(ct);
        try
        {
            var puntero = await almacen.LeerPunteroAsync(ct);
            // Sin puntero todavía (no hubo arranque) no se crea uno: el primer arranque lo decide la semilla.
            if (puntero is not null)
                await almacen.ReemplazarPunteroAsync(puntero with { UltimaConsultaExitosa = cuando }, ct);
        }
        finally
        {
            _puntero.Release();
        }
    }

    // ───────────────────────────── Auxiliares ─────────────────────────────

    /// <summary>Un manifiesto ilegible cuenta como ausente: el paquete se descarta, no se rompe el arranque.</summary>
    private async Task<Contracts.Contenido.ManifiestoContenido?> LeerManifiestoAsync(VersionSemantica version, CancellationToken ct)
    {
        try
        {
            return await almacen.LeerManifiestoAsync(version, ct);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>¿Está la versión en la lista? Por precedencia: «1.0.1» retirada alcanza a «1.0.1+abc».</summary>
    private static bool PorPrecedencia(IEnumerable<VersionSemantica> lista, VersionSemantica version) =>
        lista.Any(v => v.CompareTo(version) == 0);

    private static async Task<byte[]> LeerHastaAsync(Stream flujo, long limite, CancellationToken ct)
    {
        using var memoria = new MemoryStream();
        var buffer = new byte[81920];
        int leidos;
        while (memoria.Length < limite && (leidos = await flujo.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, limite - memoria.Length)), ct)) > 0)
            memoria.Write(buffer, 0, leidos);
        return memoria.ToArray();
    }

    private static string Texto(VersionSemantica? v) => v?.ToString() ?? "ninguna";

    private static ResultadoDeConsulta Resultado(EstadoDeConsulta estado, MotivoDeRechazo motivo, string detalle) =>
        new(estado, motivo, detalle, null, []);
}
