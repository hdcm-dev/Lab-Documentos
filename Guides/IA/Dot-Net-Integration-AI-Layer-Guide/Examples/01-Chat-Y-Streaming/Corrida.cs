// Nuevo (ejemplo 01). Raíz de composición: acá, y sólo acá, se elige el cliente (OllamaSharp por la
// fábrica de la capa única, o el guionado de --fake envuelto en la MISMA capa).
// Delta (2026-10-29, REC-09): una vuelta cancelada se excluye con motivo «cancelada» y corta la corrida.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

namespace MovilidadUrbana.Asistente;

/// <summary>Resultado de una corrida: los invariantes de código que fallaron y los veredictos por entrada.</summary>
public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyDictionary<string, string> Veredictos, string RutaDelRegistro);

public static class Corrida
{
    public const string Ejemplo = "01-Chat-Y-Streaming";

    /// <summary>Guion del modo --fake: dos respuestas, cada una en streaming de 5 fragmentos.</summary>
    public static GuionChatClient CrearGuion() => new(
    [
        PasoDelGuion.TextoEnFragmentos("Para el alta ", "cargá el nombre, ", "la provincia, ", "el código postal ", "y los habitantes."),
        PasoDelGuion.TextoEnFragmentos("La encuesta pide ", "nombre, edad y localidad; ", "medios de transporte y frecuencia; ", "distancia y minutos de viaje; ", "y el motivo principal.")
    ]);

    public static async Task<ResultadoDeCorrida> CorrerAsync(
        ArgumentosDeEjemplo argumentos, ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
        var entradas = OraculoDeCampos.Cargar(rutaEntradas);
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

        var asistente = new AsistenteDeAyuda(cliente);
        var vueltas = new List<VueltaDeChat>();
        foreach (var entrada in entradas)
        {
            if (plazo.Vencido)
            {
                Excluir(registro, entrada.Id, "plazo", veredictos);
                continue;
            }

            string? estadoCache = null;
            if (!argumentos.Fake && config.Servidor == TipoDeServidor.Compartido &&
                await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, cancellationToken) == false)
            {
                estadoCache = "frio";
            }

            salida.WriteLine($"> [{entrada.Id}] {entrada.Texto}");
            var reloj = EscritorDeRegistro.Cronometro();
            VueltaDeChat vuelta;
            try
            {
                vuelta = await asistente.PreguntarAsync(entrada.Texto, f => { salida.Write(f); salida.Flush(); }, cancellationToken);
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                salida.WriteLine();
                salida.WriteLine($"  la llamada falló: {e.GetType().Name}");
                Excluir(registro, entrada.Id, $"error de llamada: {e.GetType().Name}", veredictos);
                break;
            }
            salida.WriteLine();
            if (vuelta.Cancelada)
            {
                // REC-09: la vuelta cortada no se juzga ni entra al historial.
                salida.WriteLine("  la llamada se canceló a mitad del streaming");
                Excluir(registro, entrada.Id, "cancelada", veredictos);
                break;
            }
            vueltas.Add(vuelta);

            // Opciones efectivas: con --fake, las que el guionado recibió DEBAJO de la capa; con el
            // modelo, las mismas que aplica la capa (mismo método), para el registro.
            ChatOptions efectivas;
            if (guion is not null)
            {
                efectivas = guion.Llamadas[^1].Opciones ?? new ChatOptions();
            }
            else
            {
                efectivas = vuelta.Opciones.Clone();
                OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
            }

            var finish = vuelta.Respuesta.FinishReason?.Value ?? "ninguno";
            var linea = registro.Llamada(entrada.Id, 1, vuelta.Enviados, efectivas, vuelta.Respuesta, reloj.Elapsed,
                paso: $"streaming finish={finish}", estadoCacheForzado: estadoCache);
            registro.Escribir(linea);

            var faltan = OraculoDeCampos.Faltantes(vuelta.Respuesta.Text, entrada);
            string resultado, motivo;
            if (linea.TruncadoPosible == true) { resultado = ResultadoDeEntrada.Excluida; motivo = "truncado_posible"; }
            else if (faltan.Count == 0 && finish == ChatFinishReason.Stop.Value) { resultado = ResultadoDeEntrada.Pasa; motivo = "nombra todos los campos; finish=stop"; }
            else { resultado = ResultadoDeEntrada.Falla; motivo = $"finish={finish}; faltan: {(faltan.Count == 0 ? "ninguno" : string.Join(", ", faltan))}"; }
            registro.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = OraculoDeCampos.Nombre, Resultado = resultado, Motivo = motivo });
            veredictos[entrada.Id] = resultado;
            salida.WriteLine($"  oráculo {OraculoDeCampos.Nombre}: {resultado} ({motivo}); fragmentos: {vuelta.Fragmentos.Count}; razonamiento: {linea.Razonamiento}; num_ctx: {linea.NumCtx} ({linea.NumCtxOrigen})");

            // Invariantes de código de cada llamada (no dependen de la conducta del modelo).
            if (linea.Razonamiento != nameof(ReasoningEffort.None)) fallan.Add($"{entrada.Id}: razonamiento {linea.Razonamiento ?? "sin fijar"}");
            if (linea.NumCtx is not > 0) fallan.Add($"{entrada.Id}: sin num_ctx");
            if (vuelta.Opciones.Instructions != AsistenteDeAyuda.Instrucciones) fallan.Add($"{entrada.Id}: instrucciones fuera de ChatOptions");
            if (vuelta.Enviados.Any(m => m.Role == ChatRole.System)) fallan.Add($"{entrada.Id}: instrucciones metidas en el historial");
        }

        // Historial explícito: la segunda vuelta reenvía pregunta 1, respuesta 1 y pregunta 2.
        if (vueltas.Count >= 2)
        {
            var enviados = vueltas[1].Enviados;
            var roles = string.Join(",", enviados.Select(m => m.Role.Value));
            if (roles != "user,assistant,user" || enviados[1].Text != vueltas[0].Respuesta.Text)
            {
                fallan.Add($"historial: la segunda vuelta no reenvía la conversación ({roles})");
            }
        }

        if (argumentos.Fake)
        {
            // Con el guion, la conducta es fija: todo tiene que pasar y llegar en 5 fragmentos.
            if (vueltas.Count != entradas.Count) fallan.Add($"--fake: {vueltas.Count} de {entradas.Count} vueltas");
            if (vueltas.Any(v => v.Fragmentos.Count != 5)) fallan.Add("--fake: el streaming no llegó en 5 fragmentos");
            if (veredictos.Values.Any(v => v != ResultadoDeEntrada.Pasa)) fallan.Add("--fake: el oráculo no pasa con el guion");
            fallan.AddRange(await VarianteNegativaAsync(config, salida, cancellationToken));
        }
        else
        {
            salida.WriteLine("variante negativa (sin la capa): sólo con --fake; no gasta llamadas del presupuesto (ver README).");
        }

        salida.WriteLine($"registro: {registro.RenglonesEscritos} líneas en {Path.GetFileName(registro.Ruta)}");
        return new ResultadoDeCorrida(fallan, veredictos, registro.Ruta);
    }

    /// <summary>
    /// Variante negativa (Sondeo §2): el mismo pedido con presupuesto chico, sin la capa → Text vacío y
    /// finish=length; con la capa → texto. Corre contra un cliente simulado, nunca contra el servidor.
    /// </summary>
    public static async Task<IReadOnlyList<string>> VarianteNegativaAsync(ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var fallan = new List<string>();
        var opciones = new ChatOptions { MaxOutputTokens = AsistenteDeAyuda.TopeDeSalida };
        const string pregunta = "¿Qué datos tengo que cargar para dar de alta una localidad?";
        var simulado = new ModeloQueRazonaPorDefecto("Nombre, provincia, código postal y habitantes.");

        var sinCapa = await simulado.GetResponseAsync(pregunta, opciones.Clone(), cancellationToken);
        var conCapa = await FabricaDeClientes.Envolver(simulado, config, Adaptador.OllamaSharp).GetResponseAsync(pregunta, opciones.Clone(), cancellationToken);

        salida.WriteLine($"variante negativa (simulada): sin la capa → Text «{sinCapa.Text}», finish={sinCapa.FinishReason?.Value}; con la capa → «{conCapa.Text}», finish={conCapa.FinishReason?.Value}");
        if (sinCapa.Text.Length != 0 || sinCapa.FinishReason != ChatFinishReason.Length) fallan.Add("variante negativa: sin la capa no reproduce el Text vacío");
        if (string.IsNullOrWhiteSpace(conCapa.Text) || conCapa.FinishReason != ChatFinishReason.Stop) fallan.Add("variante negativa: con la capa no hay texto");
        return fallan;
    }

    private static void Excluir(EscritorDeRegistro registro, string id, string motivo, Dictionary<string, string> veredictos)
    {
        registro.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = id, Oraculo = OraculoDeCampos.Nombre, Resultado = ResultadoDeEntrada.Excluida, Motivo = motivo });
        veredictos[id] = ResultadoDeEntrada.Excluida;
    }
}
