// Nuevo (ejemplo 10). Corre cada entrada TRES veces y compara: (1) la función; (2) el workflow de punta a
// punta; (3) el workflow cortado en la pausa del paso 3 y rehidratado desde el checkpoint en otra instancia
// (grafo y executors nuevos), con el almacén de checkpoints en JSON sobre disco. Sin modelo: no hay ninguna
// llamada a un IChatClient y el registro tiene sólo líneas «entrada».
using System.Text.Json;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Workflows;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;

namespace MovilidadUrbana.Asistente;

/// <summary>Lo que se compara por entrada; viaja entero en el motivo del renglón «entrada» (JSON) para recalcular el oráculo.</summary>
public sealed record ComparacionDeEntrada(
    string Esperado, string Funcion, string Workflow, string Rehidratado,
    string DatosFuncion, string DatosWorkflow, string DatosRehidratado,
    int Supersteps, int Checkpoints, bool Cortado, int PedidosTrasRehidratar);

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyDictionary<string, string> Veredictos, string RutaDelRegistro);

public static class Corrida
{
    public const string Ejemplo = "10-Workflow-Y-Contraejemplo";
    public const string Oraculo = "workflow-igual-funcion";

    /// <summary>Dónde se pausa el workflow para probar la rehidratación: el primer pedido del paso 3.</summary>
    public const int PasoDeCorte = 3;

    private static readonly JsonSerializerOptions JsonMotivo = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static EncuestaService CrearServicio() => new(new SinPersistencia());

    public static async Task<ComparacionDeEntrada> CompararAsync(EntradaDeEncuesta entrada, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var servicio = CrearServicio();

        // (1) La función.
        var funcion = EncuestaComoFuncion.Completar(new Persona(entrada.Respuestas), servicio);

        // (2) El workflow de punta a punta, con checkpoints en memoria.
        await using var directa = await InProcessExecution.RunStreamingAsync(
            EncuestaComoWorkflow.Construir(servicio), new IniciarEncuesta(), CheckpointManager.CreateInMemory(), entrada.Id, cancellationToken);
        var workflow = await ConductorDelWorkflow.ConducirAsync(directa, new Persona(entrada.Respuestas), cancellationToken: cancellationToken);

        // (3) Cortado en la pausa y rehidratado en OTRA instancia, con el almacén en disco (JSON).
        var dir = Directory.CreateTempSubdirectory("ej10-checkpoints-");
        try
        {
            using var almacen = new FileSystemJsonCheckpointStore(dir);
            var checkpoints = CheckpointManager.CreateJson(almacen, null);
            var persona = new Persona(entrada.Respuestas);   // la misma persona antes y después de la pausa

            CorridaDelWorkflow antes;
            await using (var primera = await InProcessExecution.RunStreamingAsync(
                             EncuestaComoWorkflow.Construir(servicio), new IniciarEncuesta(), checkpoints, entrada.Id + "-r", cancellationToken))
            {
                antes = await ConductorDelWorkflow.ConducirAsync(primera, persona, PasoDeCorte, cancellationToken);
            }

            CorridaDelWorkflow? despues = null;
            if (antes.CheckpointDeCorte is { } cp)
            {
                await using var segunda = await InProcessExecution.ResumeStreamingAsync(
                    EncuestaComoWorkflow.Construir(CrearServicio()), cp, checkpoints, cancellationToken);
                despues = await ConductorDelWorkflow.ConducirAsync(segunda, persona, cancellationToken: cancellationToken);
            }

            foreach (var t in workflow.Traza) salida.WriteLine($"  workflow: {t}");
            foreach (var t in antes.Traza) salida.WriteLine($"  antes de la pausa: {t}");
            foreach (var t in despues?.Traza ?? []) salida.WriteLine($"  rehidratado: {t}");

            return new ComparacionDeEntrada(
                entrada.Esperado, funcion.Estado, workflow.Resultado?.Estado ?? "sin-salida", despues?.Resultado?.Estado ?? "sin-salida",
                funcion.Datos, workflow.Resultado?.Datos ?? "", despues?.Resultado?.Datos ?? "",
                workflow.Supersteps, workflow.Checkpoints, antes.CheckpointDeCorte is not null,
                despues?.Pedidos ?? 0);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    /// <summary>Los invariantes de código de una entrada (los de --verificar). Lista vacía: se cumplen.</summary>
    public static IReadOnlyList<string> Invariantes(string id, ComparacionDeEntrada c)
    {
        var fallan = new List<string>();
        if (c.Funcion != c.Esperado) fallan.Add($"{id}: la función dio {c.Funcion}, se esperaba {c.Esperado}");
        if (c.Workflow != c.Funcion || c.DatosWorkflow != c.DatosFuncion) fallan.Add($"{id}: el workflow ({c.Workflow}) no coincide con la función ({c.Funcion})");
        if (c.Rehidratado != c.Funcion || c.DatosRehidratado != c.DatosFuncion) fallan.Add($"{id}: el rehidratado ({c.Rehidratado}) no coincide con la función ({c.Funcion})");
        if (c.Supersteps == 0 || c.Checkpoints != c.Supersteps) fallan.Add($"{id}: {c.Checkpoints} checkpoints para {c.Supersteps} supersteps");
        if (!c.Cortado) fallan.Add($"{id}: no hubo pausa en el paso {PasoDeCorte} con checkpoint");
        if (c.PedidosTrasRehidratar < 1) fallan.Add($"{id}: tras rehidratar no se volvió a emitir el pedido pendiente");
        return fallan;
    }

    public static async Task<ResultadoDeCorrida> CorrerAsync(ArgumentosDeEjemplo a, ConfiguracionDelModelo config, TextWriter salida)
    {
        var archivo = Entradas.Leer();
        var plazo = a.CrearPlazo();
        var fallan = new List<string>();
        var veredictos = new Dictionary<string, string>();

        using var registro = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo,
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,   // nominal: este ejemplo no llama a ningún modelo
            Modelo = null,
            EntradasVersion = archivo.Version,
            EntradasHash = Hashes.Sha256DeArchivo(Entradas.Ruta),
            PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
            Maquina = "sin-modelo"
        });

        salida.WriteLine("sin modelo: ninguna llamada a un IChatClient (el --fake no cambia nada en este ejemplo)");
        foreach (var e in archivo.Entradas)
        {
            if (plazo.Vencido)
            {
                registro.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = e.Id, Oraculo = Oraculo, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
                veredictos[e.Id] = ResultadoDeEntrada.Excluida;
                continue;
            }

            salida.WriteLine($"{e.Id}: {e.Descripcion}");
            using var tope = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var c = await CompararAsync(e, salida, tope.Token);
            var inv = Invariantes(e.Id, c);
            fallan.AddRange(inv);
            var resultado = inv.Count == 0 ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla;
            veredictos[e.Id] = resultado;

            salida.WriteLine($"  función: {c.Funcion} · workflow: {c.Workflow} · rehidratado: {c.Rehidratado}");
            salida.WriteLine($"  supersteps: {c.Supersteps} · checkpoints: {c.Checkpoints} · pedidos tras rehidratar: {c.PedidosTrasRehidratar}");
            salida.WriteLine($"  oráculo {Oraculo} (esperado {c.Esperado}): {resultado}");

            registro.Escribir(new RegistroDeEntrada
            {
                Ejemplo = Ejemplo, EntradaId = e.Id, Oraculo = Oraculo, Resultado = resultado,
                Motivo = JsonSerializer.Serialize(c, JsonMotivo)
            });
        }

        salida.WriteLine($"registro: {registro.RenglonesEscritos} renglones en {Path.GetFileName(registro.Ruta)}");
        return new ResultadoDeCorrida(fallan, veredictos, registro.Ruta);
    }
}
