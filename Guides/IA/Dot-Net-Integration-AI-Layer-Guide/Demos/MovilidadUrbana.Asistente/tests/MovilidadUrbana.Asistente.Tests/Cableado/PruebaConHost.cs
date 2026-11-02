// Nuevo. Base de las pruebas de cableado: un host en proceso por clase, sin red, y un scope por
// «circuito» con su sesión, como lo arma Routes.razor en la página.
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

public abstract class PruebaConHost
{
    protected ApiWebApplicationFactory Fabrica { get; private set; } = default!;

    [OneTimeSetUp]
    public void ArmarHost()
    {
        Fabrica = new ApiWebApplicationFactory();
        _ = Fabrica.Server; // arranca el host: ingesta de la ayuda incluida
    }

    [OneTimeTearDown]
    public void BajarHost() => Fabrica.Dispose();

    /// <summary>Un scope con su sesión: lo que es un circuito de Blazor para las herramientas.</summary>
    protected Circuito NuevoCircuito(string? sesion = null)
    {
        var scope = Fabrica.Services.CreateScope();
        var id = sesion ?? Guid.NewGuid().ToString("n");
        scope.ServiceProvider.GetRequiredService<SessionContext>().Set(id);
        return new Circuito(scope, id);
    }
}

public sealed class Circuito(IServiceScope scope, string sesion) : IDisposable
{
    public string Sesion { get; } = sesion;
    public IServiceProvider Servicios => scope.ServiceProvider;
    public SesionDeChat Chat => scope.ServiceProvider.GetRequiredService<SesionDeChat>();
    public T Get<T>() where T : notnull => scope.ServiceProvider.GetRequiredService<T>();
    public void Dispose() => scope.Dispose();
}
