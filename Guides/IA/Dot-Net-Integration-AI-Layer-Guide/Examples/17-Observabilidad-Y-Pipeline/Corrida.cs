// Nuevo (ejemplo 17). Raíz de composición: acá, y sólo acá, se elige el cliente (OllamaSharp por la
// fábrica de la capa única, o el guionado de --fake envuelto en la MISMA capa) y se arma el pipeline
// de caché, telemetría y registro encima.
using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

namespace MovilidadUrbana.Asistente;

/// <summary>Resultado de una corrida: los invariantes de código que fallaron y los veredictos por entrada.</summary>
public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyDictionary<string, string> Veredictos, string RutaDelRegistro);

/// <summary>Lo que se vio al pasar los pedidos por una variante del pipeline.</summary>
public sealed record ObservacionDeVariante(
    OrdenDeCapas Orden,
    string Ambiente,
    IReadOnlyList<LlamadaAlModelo> Llamadas,
    IReadOnlyList<string> PedidoDeCadaLlamada,
    IReadOnlyList<string> Aciertos,
    IReadOnlyList<Activity> Spans,
    IReadOnlyList<RenglonDeLog> Logs,
    IReadOnlyList<string> Instrumentos,
    IReadOnlyList<string> MetricasMedidas,
    IReadOnlyList<ChatResponse> Respuestas,
    long AciertosContados);

public static class Corrida
{
    public const string Ejemplo = "17-Observabilidad-Y-Pipeline";

    /// <summary>Guion del modo --fake: dos respuestas, una por cada pedido que llega al «modelo».</summary>
    public static GuionChatClient CrearGuion() => new(
    [
        PasoDelGuion.Texto("Para el alta de una localidad se cargan el nombre, la provincia, el código postal y los habitantes."),
        PasoDelGuion.Texto("El alta pide nombre, provincia del catálogo, código postal de 4 dígitos y cantidad de habitantes.")
    ]);

    /// <summary>
    /// Pasa los pedidos, en orden, por una variante del pipeline armada sobre <paramref name="conCapaUnica" />.
    /// Un pedido que no llega al registrador es un acierto de caché. Los pedidos de una entrada son de
    /// una misma sesión (<paramref name="contexto" />; si no se pasa, una sola sesión sin escrituras).
    /// </summary>
    public static async Task<ObservacionDeVariante> ObservarAsync(
        IChatClient conCapaUnica, OrdenDeCapas orden, string ambiente, LogLevel nivelDeLogs,
        IReadOnlyList<Pedido> pedidos, CancellationToken cancellationToken = default, ContextoDeLaCache? contexto = null)
    {
        using var captura = new CapturaDeTelemetria();
        var registrador = new RegistradorDeLlamadas(conCapaUnica);
        var logs = new LogsEnMemoria(nivelDeLogs);
        contexto ??= new ContextoDeLaCache("sesion-1");
        var pipeline = PipelineObservado.Construir(
            conCapaUnica, orden, PipelineObservado.CacheEnMemoria(), logs, ambiente, registrador, contexto.ValoresDeLaClave);
        var cache = pipeline.GetService<CacheConContador>()!;
        var asistente = new AsistenteDeAyuda(pipeline);

        var pedidoDeCadaLlamada = new List<string>();
        var aciertos = new List<string>();
        var respuestas = new List<ChatResponse>();
        foreach (var pedido in pedidos)
        {
            var antes = registrador.Llamadas.Count;
            respuestas.Add(await asistente.PedirAsync(pedido, cancellationToken));
            if (registrador.Llamadas.Count == antes) aciertos.Add(pedido.Id);
            else pedidoDeCadaLlamada.Add(pedido.Id);
        }

        return new ObservacionDeVariante(orden, ambiente, [.. registrador.Llamadas], pedidoDeCadaLlamada, aciertos,
            [.. captura.Spans], [.. logs.Renglones], captura.InstrumentosPublicados, captura.MetricasConMediciones(), respuestas,
            cache.Aciertos);
    }

    public static async Task<ResultadoDeCorrida> CorrerAsync(
        ArgumentosDeEjemplo argumentos, ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
        var entrada = EntradaDeEjemplo.Cargar(rutaEntradas).Single();
        var plazo = argumentos.CrearPlazo();
        var fallan = new List<string>();
        var veredictos = new Dictionary<string, string>();

        GuionChatClient? guion = null;
        IChatClient cliente;
        string? version = null, digest = null;
        if (argumentos.Fake)
        {
            guion = CrearGuion();
            cliente = FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp);
        }
        else
        {
            var predicado = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, PredicadoDeSalto.Chat, cancellationToken);
            if (predicado.Saltar)
            {
                return new ResultadoDeCorrida([$"sin modelo: {predicado.Motivo}"], veredictos, string.Empty);
            }
            version = predicado.VersionDeOllama;
            digest = predicado.Digest;
            cliente = FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);
        }

        using var registro = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo,
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,
            Modelo = argumentos.Fake ? "guion" : config.Modelo,
            Digest = digest,
            OllamaVersion = version,
            EntradasVersion = "v1",
            EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
            PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
            Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
        });

        var ambiente = Ambientes.DelProceso();
        if (plazo.Vencido)
        {
            Excluir(registro, entrada.Id, "plazo", veredictos);
            salida.WriteLine($"registro: {registro.RenglonesEscritos} líneas en {Path.GetFileName(registro.Ruta)}");
            return new ResultadoDeCorrida(fallan, veredictos, registro.Ruta);
        }

        string? estadoCache = null;
        if (!argumentos.Fake && config.Servidor == TipoDeServidor.Compartido &&
            await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, cancellationToken) == false)
        {
            estadoCache = "frio";
        }

        // 1. La variante que va contra el modelo: caché más afuera, contenido según el ambiente, logs en Debug.
        salida.WriteLine($"[{entrada.Id}] caché más afuera · ambiente {ambiente} · {entrada.Pedidos.Count} pedidos");
        ObservacionDeVariante obs;
        try
        {
            obs = await ObservarAsync(cliente, OrdenDeCapas.CacheAfuera, ambiente, LogLevel.Debug, entrada.Pedidos, cancellationToken);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            salida.WriteLine($"  la llamada falló: {e.GetType().Name}");
            Excluir(registro, entrada.Id, $"error de llamada: {e.GetType().Name}", veredictos);
            return new ResultadoDeCorrida(fallan, veredictos, registro.Ruta);
        }

        var observadas = new List<LlamadaObservada>();
        for (var i = 0; i < obs.Llamadas.Count; i++)
        {
            var llamada = obs.Llamadas[i];
            // Opciones efectivas: con --fake, las OBSERVADAS (las que el guionado recibió DEBAJO de la
            // capa). Con el modelo, RECONSTRUIDAS: se clonan las de arriba de la capa y se les aplica el
            // mismo método de la capa. Por eso, con el modelo, razonamiento, num_ctx y temperature del
            // registro y los dos invariantes de abajo se cumplen por construcción; lo que garantiza que
            // la capa de verdad las aplica es la prueba con espía
            // CapaDeOpciones_DebajoDelPipeline_ApagaElRazonamientoYFijaNumCtx. Ver README.
            ChatOptions efectivas;
            if (guion is not null)
            {
                efectivas = guion.Llamadas[i].Opciones ?? new ChatOptions();
            }
            else
            {
                efectivas = llamada.Opciones?.Clone() ?? new ChatOptions();
                OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
            }

            var conSpan = i < obs.Spans.Count;
            var contenido = conSpan && CapturaDeTelemetria.TraeContenido(obs.Spans[i]);
            var linea = registro.Llamada(entrada.Id, i + 1, llamada.Enviados, efectivas, llamada.Respuesta, llamada.Reloj,
                paso: OraculoCacheYTelemetria.Paso(obs.PedidoDeCadaLlamada[i], conSpan, contenido, ambiente),
                estadoCacheForzado: i == 0 ? estadoCache : null);
            registro.Escribir(linea);
            observadas.Add(new LlamadaObservada(linea.Paso!, linea.Salida, linea.Temperature, linea.TruncadoPosible));

            if (linea.Razonamiento != nameof(ReasoningEffort.None)) fallan.Add($"llamada {i + 1}: razonamiento {linea.Razonamiento ?? "sin fijar"}");
            if (linea.NumCtx is not > 0) fallan.Add($"llamada {i + 1}: sin num_ctx");
            salida.WriteLine($"  llamada {i + 1} ({obs.PedidoDeCadaLlamada[i]}): temperature {linea.Temperature?.ToString(System.Globalization.CultureInfo.InvariantCulture)} · razonamiento {linea.Razonamiento} · num_ctx {linea.NumCtx} ({linea.NumCtxOrigen}) · «{Recortar(linea.Salida)}»");
        }

        var (resultado, motivo) = OraculoCacheYTelemetria.Evaluar(observadas);
        registro.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = OraculoCacheYTelemetria.Nombre, Resultado = resultado, Motivo = motivo });
        veredictos[entrada.Id] = resultado;

        Informar(salida, obs, entrada);
        salida.WriteLine($"  oráculo {OraculoCacheYTelemetria.Nombre}: {resultado} ({motivo})");
        fallan.AddRange(InvariantesDeLaVariantePrincipal(obs, entrada));

        // 2. Las comparaciones: siempre con el cliente guionado (no gastan llamadas al modelo).
        fallan.AddRange(await CompararAsync(config, entrada, salida, cancellationToken));

        if (argumentos.Fake && resultado != ResultadoDeEntrada.Pasa) fallan.Add("--fake: el oráculo no pasa con el guion");

        salida.WriteLine($"registro: {registro.RenglonesEscritos} líneas en {Path.GetFileName(registro.Ruta)}");
        return new ResultadoDeCorrida(fallan, veredictos, registro.Ruta);
    }

    /// <summary>Invariantes de código de la variante «caché más afuera»; no dependen de la conducta del modelo.</summary>
    public static IReadOnlyList<string> InvariantesDeLaVariantePrincipal(ObservacionDeVariante obs, EntradaDeEjemplo entrada)
    {
        var fallan = new List<string>();
        if (obs.Llamadas.Count != entrada.LlamadasEsperadas) fallan.Add($"caché afuera: {obs.Llamadas.Count} llamadas al modelo (se esperaban {entrada.LlamadasEsperadas})");
        if (obs.Spans.Count != entrada.SpansConCacheAfuera) fallan.Add($"caché afuera: {obs.Spans.Count} spans (se esperaban {entrada.SpansConCacheAfuera})");
        if (!obs.Aciertos.SequenceEqual(["p2"])) fallan.Add($"caché afuera: aciertos [{string.Join(",", obs.Aciertos)}] (se esperaba p2)");
        // El contador de la caché (REC-12) tiene que dar lo mismo que la cuenta por el registrador.
        if (obs.AciertosContados != obs.Aciertos.Count) fallan.Add($"caché afuera: el contador de la caché da {obs.AciertosContados} aciertos y el registrador {obs.Aciertos.Count}");

        var contenidoEsperado = Ambientes.ContenidoEnTelemetria(obs.Ambiente);
        if (obs.Spans.Any(s => CapturaDeTelemetria.TraeContenido(s) != contenidoEsperado))
            fallan.Add($"contenido en los spans no corresponde al ambiente {obs.Ambiente}");

        // La telemetría está ARRIBA de la capa de opciones: ve la temperatura que pidió la aplicación.
        if (obs.Spans.Count == 2)
        {
            if (CapturaDeTelemetria.Atributo(obs.Spans[0], "gen_ai.request.temperature") is not null)
                fallan.Add("span de p1: trae gen_ai.request.temperature aunque la aplicación no la pidió");
            if (CapturaDeTelemetria.Atributo(obs.Spans[1], "gen_ai.request.temperature") != "0.7")
                fallan.Add("span de p3: no trae gen_ai.request.temperature=0.7");
        }

        // Logs en Debug: cuentan que hubo llamadas, sin el texto del pedido.
        var texto = entrada.Pedidos[0].Texto;
        if (obs.Logs.Count == 0) fallan.Add("UseLogging en Debug no escribió nada");
        if (obs.Logs.Any(l => l.Texto.Contains(texto, StringComparison.Ordinal))) fallan.Add("UseLogging en Debug escribió el texto del pedido");

        // Métricas: M.E.AI 10.11.0 publica los cuatro nombres de semconv v1.41.0 (N5 E6, V9).
        if (!obs.Instrumentos.Order(StringComparer.Ordinal).SequenceEqual(ConvencionGenAi.Semconv141))
            fallan.Add($"instrumentos publicados [{string.Join(", ", obs.Instrumentos)}] distintos de los de semconv v1.41.0: re-verificar el desfase (N5 V9)");
        if (obs.MetricasMedidas.Any(m => !obs.Instrumentos.Contains(m))) fallan.Add("una métrica medida no está entre los instrumentos publicados");
        return fallan;
    }

    /// <summary>
    /// Las tres comparaciones de la guía, con el cliente guionado envuelto en la misma capa:
    /// telemetría más afuera (el acierto sí aparece), contenido encendido en desarrollo y logs en Trace.
    /// </summary>
    public static async Task<IReadOnlyList<string>> CompararAsync(
        ConfiguracionDelModelo config, EntradaDeEjemplo entrada, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var fallan = new List<string>();
        IChatClient Guionado() => FabricaDeClientes.Envolver(CrearGuion(), config, Adaptador.OllamaSharp);

        var telAfuera = await ObservarAsync(Guionado(), OrdenDeCapas.TelemetriaAfuera, Ambientes.Produccion, LogLevel.Debug, entrada.Pedidos, cancellationToken);
        salida.WriteLine($"comparación (guion) · telemetría más afuera: {telAfuera.Llamadas.Count} llamadas, {telAfuera.Spans.Count} spans; aciertos: {string.Join(",", telAfuera.Aciertos)}");
        foreach (var span in telAfuera.Spans) salida.WriteLine($"    span: {CapturaDeTelemetria.Describir(span)}");
        if (telAfuera.Llamadas.Count != entrada.LlamadasEsperadas) fallan.Add($"telemetría afuera: {telAfuera.Llamadas.Count} llamadas al modelo");
        if (telAfuera.Spans.Count != entrada.SpansConTelemetriaAfuera) fallan.Add($"telemetría afuera: {telAfuera.Spans.Count} spans (se esperaban {entrada.SpansConTelemetriaAfuera})");
        // El span del acierto trae tokens: la caché devuelve la respuesta guardada con su Usage (N5 V3).
        if (telAfuera.Spans.Count == 3 && CapturaDeTelemetria.Atributo(telAfuera.Spans[1], "gen_ai.usage.input_tokens") is null)
            fallan.Add("telemetría afuera: el span del acierto no trae gen_ai.usage.input_tokens");

        var desarrollo = await ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Desarrollo, LogLevel.Debug, entrada.Pedidos, cancellationToken);
        var conContenido = desarrollo.Spans.Count(CapturaDeTelemetria.TraeContenido);
        var claves = desarrollo.Spans.SelectMany(s => s.TagObjects.Select(t => t.Key)).Where(CapturaDeTelemetria.AtributosDeContenido.Contains).Distinct().Order();
        salida.WriteLine($"comparación (guion) · ambiente Development: {conContenido} de {desarrollo.Spans.Count} spans con contenido ({string.Join(", ", claves)})");
        if (desarrollo.Spans.Count == 0 || conContenido != desarrollo.Spans.Count) fallan.Add("Development: los spans no traen el contenido");
        if (!desarrollo.Spans.Any(s => s.TagObjects.Any(t => t.Value?.ToString()?.Contains(entrada.Pedidos[0].Texto, StringComparison.Ordinal) == true)))
            fallan.Add("Development: el texto del pedido no aparece en ningún span");

        var trace = await ObservarAsync(Guionado(), OrdenDeCapas.CacheAfuera, Ambientes.Produccion, LogLevel.Trace, entrada.Pedidos, cancellationToken);
        var logsConTexto = trace.Logs.Count(l => l.Texto.Contains(entrada.Pedidos[0].Texto, StringComparison.Ordinal));
        salida.WriteLine($"comparación (guion) · UseLogging en Trace: {logsConTexto} de {trace.Logs.Count} líneas de log traen el texto del pedido (en Debug: 0)");
        if (logsConTexto == 0) fallan.Add("Trace: UseLogging no escribió el contenido (el contraejemplo no se reproduce)");
        return fallan;
    }

    private static void Informar(TextWriter salida, ObservacionDeVariante obs, EntradaDeEjemplo entrada)
    {
        salida.WriteLine($"  pedidos: {string.Join(", ", entrada.Pedidos.Select(p => $"{p.Id}{(p.Temperatura is { } t ? $" (temperatura {t.ToString(System.Globalization.CultureInfo.InvariantCulture)})" : string.Empty)}"))}");
        salida.WriteLine($"  llegaron al modelo: {string.Join(", ", obs.PedidoDeCadaLlamada)} · aciertos de caché: {string.Join(", ", obs.Aciertos)}");
        salida.WriteLine($"  spans: {obs.Spans.Count}");
        foreach (var span in obs.Spans)
        {
            salida.WriteLine($"    span: {CapturaDeTelemetria.Describir(span)} | contenido: {(CapturaDeTelemetria.TraeContenido(span) ? "sí" : "no")}");
        }
        salida.WriteLine($"  logs (Debug): {obs.Logs.Count} líneas, ninguna con el texto del pedido: {!obs.Logs.Any(l => l.Texto.Contains(entrada.Pedidos[0].Texto, StringComparison.Ordinal))}");
        salida.WriteLine("  métricas publicadas por M.E.AI frente a la convención:");
        foreach (var (nombre, enV141, enVigente) in ConvencionGenAi.Comparar(obs.Instrumentos))
        {
            var medida = obs.MetricasMedidas.Contains(nombre) ? "medida" : "sin mediciones";
            salida.WriteLine($"    {nombre} ({medida}): semconv v1.41.0 {(enV141 ? "sí" : "no")} · vigente {(enVigente ? "sí" : "no")}");
        }
        var faltan = ConvencionGenAi.Vigente.Where(n => !obs.Instrumentos.Contains(n));
        salida.WriteLine($"    de la convención vigente, no emitidas: {string.Join(", ", faltan)}");
    }

    private static string Recortar(string? texto) =>
        texto is null ? string.Empty : texto.Length <= 90 ? texto : string.Concat(texto.AsSpan(0, 90), "…");

    private static void Excluir(EscritorDeRegistro registro, string id, string motivo, Dictionary<string, string> veredictos)
    {
        registro.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = id, Oraculo = OraculoCacheYTelemetria.Nombre, Resultado = ResultadoDeEntrada.Excluida, Motivo = motivo });
        veredictos[id] = ResultadoDeEntrada.Excluida;
    }
}
