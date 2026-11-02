// Nuevo (ejemplo 10, REC-30). Sin servidor de modelos: corre en la fase build.
// Trampa de los executors con [MessageHandler]: lo que un handler produce por el contexto con
// YieldOutputAsync se declara en el atributo (Yield = [...]), igual que lo que envía (Send = [...]).
// Si no se declara, compila y falla al ejecutar (MAF 1.26.0). Patrón tomado de la línea archivada
// (TrampasDeExecutorsTests del ejemplo 10 del insumo), con el espacio de nombres de la base.
using Microsoft.Agents.AI.Workflows;

namespace MovilidadUrbana.Asistente.Pruebas;

[TestFixture, Category("Cableado")]
public sealed class TrampasDeExecutorsTests
{
    private static async Task<(string? Salida, Exception? Error)> Correr(ExecutorBinding executor)
    {
        var workflow = new WorkflowBuilder(executor).WithOutputFrom(executor).Build();
        string? salida = null;
        Exception? error = null;
        await using var corrida = await InProcessExecution.RunStreamingAsync(workflow, "Goya");
        await foreach (var evento in corrida.WatchStreamAsync())
        {
            if (evento is WorkflowOutputEvent o && o.Is(out string? s)) salida = s;
            if (evento is ExecutorFailedEvent f) error = f.Data;
            if (evento is WorkflowErrorEvent w) error ??= w.Exception;
        }
        return (salida, error);
    }

    // Caso rojo: el handler hace YieldOutputAsync sin declarar el tipo de salida. Compila; en ejecución
    // no hay salida y el executor falla con InvalidOperationException.
    [Test]
    public async Task SinDeclararYield_CompilaYFallaAlEjecutar()
    {
        var (salida, error) = await Correr(new YieldSinDeclarar());
        TestContext.Out.WriteLine($"sin declarar: salida={salida ?? "(ninguna)"}; error={error?.GetType().Name}: {error?.Message}");
        Assert.That(salida, Is.Null);
        Assert.That(error, Is.InstanceOf<InvalidOperationException>());
    }

    // El mismo handler con Yield = [typeof(string)]: produce la salida y no falla.
    [Test]
    public async Task ConYieldDeclarado_Funciona()
    {
        var (salida, error) = await Correr(new YieldDeclarado());
        TestContext.Out.WriteLine($"declarado: salida={salida}; error={error?.Message ?? "(ninguno)"}");
        Assert.That(error, Is.Null);
        Assert.That(salida, Is.EqualTo("localidad: Goya"));
    }
}

internal sealed partial class YieldSinDeclarar() : Executor("yield-sin-declarar")
{
    [MessageHandler]
    private async ValueTask HandleAsync(string nombre, IWorkflowContext contexto) =>
        await contexto.YieldOutputAsync($"localidad: {nombre}");
}

internal sealed partial class YieldDeclarado() : Executor("yield-declarado")
{
    [MessageHandler(Yield = [typeof(string)])]
    private async ValueTask HandleAsync(string nombre, IWorkflowContext contexto) =>
        await contexto.YieldOutputAsync($"localidad: {nombre}");
}
