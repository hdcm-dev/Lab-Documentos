// Nuevo (ejemplo 14, REC-30). Tres trampas de la migración desde Semantic Kernel que no dan error,
// como casos rojos sin servidor: cada prueba afirma la falla observada con SK 1.81.0, M.E.AI 10.11.0
// y MAF 1.26.0, y queda verde mientras la falla siga así. Si una versión nueva la cambia, la prueba
// avisa y el README se corrige. Portadas de la línea archivada (PR-12 en Procedencia.md) y adaptadas
// a contar_localidades. El «modelo» es ClienteGuionado; las ejecuciones de la herramienta se cuentan
// como lecturas del almacén. SK va con alias, como en Corrida.cs: Comparador.cs sigue siendo el único
// archivo con los dos «using».
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain.Entities;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using NUnit.Framework;
using SK = Microsoft.SemanticKernel;

namespace MovilidadUrbana.Asistente.Pruebas;

[TestFixture, Category("Cableado")]
public sealed class TrampasDeMigracionTests
{
    private const string Pregunta = "¿Cuántas localidades tengo cargadas en Corrientes?";

    /// <summary>Control del primer caso rojo: con <c>UseKernelFunctionInvocation</c>, SK ejecuta la herramienta una vez.</summary>
    [Test]
    public async Task Sk_ConUseKernelFunctionInvocation_EjecutaLaHerramientaUnaVez()
    {
        var a = new Armado(conInvocacionDeFunciones: true);
        var texto = await a.PreguntarConSkAsync(Pregunta);

        a.Informar("SK con invocación", texto);
        Assert.That(a.Chat.NombrePedido, Is.EqualTo($"{PluginSk.NombreDelPlugin}_{HerramientaMeai.Nombre}"));
        Assert.That(a.Almacen.Lecturas, Is.EqualTo(1));
        Assert.That(a.Chat.Llamadas, Is.EqualTo(2));
        Assert.That(texto, Does.Contain("\"cantidad\":3"));
    }

    /// <summary>
    /// Caso rojo 1: el mismo IChatClient sin <c>UseKernelFunctionInvocation</c>. SK devuelve la llamada a la
    /// herramienta como respuesta, sin ejecutarla y sin error: 0 lecturas, 1 llamada, texto vacío.
    /// </summary>
    [Test]
    public async Task Sk_SinUseKernelFunctionInvocation_NoEjecutaNiFalla()
    {
        var a = new Armado(conInvocacionDeFunciones: false);
        var texto = await a.PreguntarConSkAsync(Pregunta);

        a.Informar("SK sin invocación", texto);
        Assert.That(a.Chat.NombrePedido, Is.EqualTo($"{PluginSk.NombreDelPlugin}_{HerramientaMeai.Nombre}"));
        Assert.That(a.Almacen.Lecturas, Is.EqualTo(0));
        Assert.That(a.Chat.Llamadas, Is.EqualTo(1));
        Assert.That(texto, Is.Empty);
    }

    /// <summary>
    /// Caso rojo 2: <c>AIFunctionFactory</c> no lee <c>[KernelFunction]</c>. Con la anotación vieja y sin
    /// <c>name</c>, el nombre sale del método (sin el sufijo Async); <c>[Description]</c> sí se lee.
    /// </summary>
    [Test]
    public void AIFunctionFactory_NoLeeElNombreDeKernelFunction()
    {
        var plugin = new PluginSk(new LocalidadService(new LocalidadesEnMemoria()));
        AIFunction f = AIFunctionFactory.Create(plugin.ContarLocalidadesAsync);

        TestContext.Out.WriteLine($"Name sin name: «{f.Name}»; Description «{f.Description}»");
        Assert.That(f.Name, Is.EqualTo("ContarLocalidades"), "el nombre sale del método, no de [KernelFunction(\"contar_localidades\")]");
        Assert.That(f.Description, Is.EqualTo(LasDosDefiniciones.Crear().Meai.Description), "[Description] sí se lee");
    }

    /// <summary>
    /// Caso rojo 3: la KernelFunction del plugin, entregada tal cual a un agente de MAF. Compila (es un
    /// AIFunction) y se ofrece al modelo con el prefijo del plugin, pero al ejecutarla falla: 0 lecturas y
    /// el modelo recibe «Error: Function failed.» sin que la aplicación vea una excepción. Con
    /// <c>IncludeDetailedErrors</c> se ve el motivo que el modo por defecto oculta.
    /// </summary>
    [Test]
    public async Task LaKernelFunction_EntregadaTalCualAUnAgenteDeMaf_FallaAlEjecutar()
    {
        var a = new Armado(conInvocacionDeFunciones: false);
        AIAgent agente = a.Chat.AsAIAgent(instructions: "x", tools: [a.FuncionSk]);
        AgentResponse r = await agente.RunAsync(Pregunta, await agente.CreateSessionAsync());

        a.Informar("MAF con KernelFunction", r.Text);
        Assert.That(a.Chat.HerramientasOfrecidas[0], Is.EqualTo(new[] { $"{PluginSk.NombreDelPlugin}_{HerramientaMeai.Nombre}" }));
        Assert.That(a.Almacen.Lecturas, Is.EqualTo(0));
        Assert.That(r.Text, Does.Contain("Error: Function failed."));

        var d = new Armado(conInvocacionDeFunciones: false);
        AIAgent detallado = d.Chat.AsBuilder()
            .UseFunctionInvocation(configure: f => f.IncludeDetailedErrors = true)
            .Build()
            .AsAIAgent(instructions: "x", tools: [d.FuncionSk]);
        AgentResponse rd = await detallado.RunAsync(Pregunta, await detallado.CreateSessionAsync());

        d.Informar("MAF con KernelFunction y errores detallados", rd.Text);
        Assert.That(d.Almacen.Lecturas, Is.EqualTo(0));
        Assert.That(rd.Text, Does.Contain("KernelPlugin"));
    }

    /// <summary>Las ejecuciones de la herramienta, contadas como lecturas del almacén en memoria.</summary>
    private sealed class AlmacenQueCuenta(ILocalidadRepository interno) : ILocalidadRepository
    {
        public int Lecturas { get; private set; }

        public Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            Lecturas++;
            return interno.GetAllAsync(cancellationToken);
        }

        public Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => interno.GetByIdAsync(id, cancellationToken);
        public Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default) => interno.AddAsync(localidad, cancellationToken);
        public Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default) => interno.UpdateAsync(localidad, cancellationToken);
        public Task DeleteAsync(int id, CancellationToken cancellationToken = default) => interno.DeleteAsync(id, cancellationToken);
    }

    /// <summary>El plugin de SK sobre un almacén que cuenta, un Kernel con el IChatClient guionado y el plugin.</summary>
    private sealed class Armado
    {
        public AlmacenQueCuenta Almacen { get; } = new(new LocalidadesEnMemoria());
        public ClienteGuionado Chat { get; } = new(HerramientaMeai.Nombre, new Dictionary<string, object?> { [Corrida.Parametro] = "Corrientes" });
        public SK.Kernel Kernel { get; }
        public SK.KernelFunction FuncionSk => Kernel.Plugins[PluginSk.NombreDelPlugin][HerramientaMeai.Nombre];

        public Armado(bool conInvocacionDeFunciones)
        {
            IChatClient cliente = conInvocacionDeFunciones
                // SK ejecuta las KernelFunction que pide el modelo. Es Chat.AsBuilder().UseKernelFunctionInvocation(),
                // escrito como llamada estática por el alias de SK.
                ? SK.KernelFunctionInvokingChatClientBuilderExtensions.UseKernelFunctionInvocation(Chat.AsBuilder()).Build()
                : Chat;
            // El Kernel sobre un contenedor con el IChatClient (lo mismo que Kernel.CreateBuilder() + Build(),
            // sin la extensión que el alias de SK deja fuera de vista).
            Kernel = new SK.Kernel(new ServiceCollection().AddSingleton(cliente).BuildServiceProvider());
            Kernel.Plugins.Add(new PluginSk(new LocalidadService(Almacen)).Crear());
        }

        public async Task<string> PreguntarConSkAsync(string pregunta)
        {
            var ajustes = new SK.PromptExecutionSettings { FunctionChoiceBehavior = SK.FunctionChoiceBehavior.Auto() };
            // Llamada estática: con SK como alias, la extensión InvokePromptAsync no queda a la vista.
            var r = await SK.KernelExtensions.InvokePromptAsync(Kernel, pregunta, new SK.KernelArguments(ajustes));
            return r.GetValue<string>() ?? "";
        }

        public void Informar(string marco, string texto)
        {
            TestContext.Out.WriteLine($"{marco}: llamadas al cliente {Chat.Llamadas}; lecturas {Almacen.Lecturas}; nombre pedido «{Chat.NombrePedido}»");
            for (var i = 0; i < Chat.HerramientasOfrecidas.Count; i++)
                TestContext.Out.WriteLine($"{marco}: llamada {i + 1} ofreció «{string.Join(", ", Chat.HerramientasOfrecidas[i])}»");
            TestContext.Out.WriteLine($"{marco}: texto final «{texto.ReplaceLineEndings(" ")}»");
        }
    }
}
