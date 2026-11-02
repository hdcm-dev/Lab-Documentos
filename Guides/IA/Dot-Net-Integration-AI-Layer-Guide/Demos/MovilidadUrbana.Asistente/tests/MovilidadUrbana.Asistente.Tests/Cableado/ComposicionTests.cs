// Nuevo. Ciclo de vida de la capa de IA (ADR-12; T1, H2-010) sobre la composición real.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class ComposicionTests : PruebaConHost
{
    [Test]
    [Description("Con ValidateScopes y ValidateOnBuild encendidos el host arranca; la conversación es scoped y el cliente base singleton")]
    public void Composicion_ValidateScopes()
    {
        // ValidateOnBuild ya corrió al arrancar el host (ApiWebApplicationFactory.CreateHost). ValidateScopes: pedir un scoped desde la raíz falla.
        Assert.That(() => Fabrica.Services.GetRequiredService<SesionDeChat>(), Throws.InvalidOperationException);

        using var a = NuevoCircuito();
        using var b = NuevoCircuito();
        Assert.That(a.Chat, Is.Not.SameAs(b.Chat), "una conversación por circuito");
        Assert.That(a.Get<HerramientasDelAsistente>(), Is.Not.SameAs(b.Get<HerramientasDelAsistente>()), "herramientas por circuito");
        Assert.That(a.Get<IChatClient>(), Is.SameAs(b.Get<IChatClient>()), "cliente base singleton");
        Assert.That(SesionDeChat.MaximoDeIteraciones, Is.GreaterThan(0));
        Assert.That(SesionDeChat.MaximoDeErroresSeguidos, Is.GreaterThan(0));
    }

    [Test]
    [Description("La persistencia se registra con AddDbContextFactory: la fábrica sale de la raíz (singleton) y cada CreateDbContext da un contexto propio; los repositorios reciben la fábrica, nunca el contexto. Un circuito de Blazor vive más que una petición (REC-14)")]
    public void Persistencia_ConFabricaDeDbContext()
    {
        var fabrica = Fabrica.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        Assert.That(Fabrica.Services.GetRequiredService<IDbContextFactory<AppDbContext>>(), Is.SameAs(fabrica), "fábrica singleton");
        using var a = fabrica.CreateDbContext();
        using var b = fabrica.CreateDbContext();
        Assert.That(a, Is.Not.SameAs(b), "un contexto por operación");

        var parametros = new[] { typeof(LocalidadRepository), typeof(EncuestaRepository), typeof(SessionSeeder) }
            .SelectMany(t => t.GetConstructors()).SelectMany(c => c.GetParameters()).Select(p => p.ParameterType).ToList();
        Assert.That(parametros, Has.Some.EqualTo(typeof(IDbContextFactory<AppDbContext>)));
        Assert.That(parametros, Has.None.EqualTo(typeof(AppDbContext)));
    }

    [Test]
    [Description("Ninguna herramienta tiene un parámetro que resuelva DI (IServiceProvider no aparece en ninguna firma)")]
    public void Herramientas_SinParametrosDeDi()
    {
        using var circuito = NuevoCircuito();
        var esquemas = circuito.Get<HerramientasDelAsistente>().Crear().OfType<AIFunctionDeclaration>().Select(f => f.JsonSchema.GetRawText());
        Assert.That(esquemas, Has.None.Contains("serviceProvider").And.None.Contains("cancellationToken"));
        var parametros = typeof(HerramientasDelAsistente).GetMethods().SelectMany(m => m.GetParameters()).Select(p => p.ParameterType);
        Assert.That(parametros, Has.None.EqualTo(typeof(IServiceProvider)));
    }
}

[TestFixture]
public class ConfiguracionTests
{
    [Test]
    [Description("Servidor compartido sin num_ctx del arnés: la configuración se rechaza y el host no arranca (R-04)")]
    public void Compartido_SinNumCtxDelArnes_Rechaza()
    {
        Assert.That(() => ConfiguracionDelModelo.DesdeEntorno(k => k == "OLLAMA_SERVIDOR" ? "compartido" : null).Validar(),
            Throws.TypeOf<ConfiguracionInvalidaException>());

        using var fabrica = new ApiWebApplicationFactory(new Dictionary<string, string?> { ["OLLAMA_SERVIDOR"] = "compartido" });
        Assert.That(() => fabrica.Server, Throws.Exception, "Program sale con 1 antes de construir el host");
    }

    [Test]
    [Description("EMB-B con un servidor sin la capacidad embedding: el host no arranca y el error dice cómo pasar a EMB-C")]
    public void EmbB_SinServidorDeEmbeddings_ArrancaConError()
    {
        using var fabrica = new ApiWebApplicationFactory(new Dictionary<string, string?> { ["EMB_PLAN"] = "EMB-B" });
        Assert.That(() => fabrica.Server, Throws.InvalidOperationException.With.Message.Contains("EMB_PLAN=EMB-C"));
    }
}
