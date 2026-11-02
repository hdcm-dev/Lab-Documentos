// Nuevo. Límite de concurrencia del modelo en la entrada (REC-13): la API y la página comparten un solo
// limitador. Pruebas deterministas: una barrera retiene a los que entraron, así que el resultado no
// depende del orden en que lleguen los pedidos ni de cuánto tarden.
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Ayuda;
using MovilidadUrbana.Asistente.Concurrencia;
using MovilidadUrbana.Asistente.Contracts;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class LimitadorTests
{
    private static readonly TimeSpan Plazo = TimeSpan.FromSeconds(30);

    private static Dictionary<string, string?> Ajustes(int permisos, int cola) => new()
    {
        ["Concurrencia:Permisos"] = permisos.ToString(System.Globalization.CultureInfo.InvariantCulture),
        ["Concurrencia:Cola"] = cola.ToString(System.Globalization.CultureInfo.InvariantCulture),
        ["Concurrencia:ReintentarEnSegundos"] = "30"
    };

    [Test]
    [Description("Permiso 3, cola 0: de 4 pedidos simultáneos a /ayuda, 3 entran y quedan retenidos; el cuarto recibe 503 con Retry-After antes de que salga ninguno")]
    public async Task CuatroSimultaneos_TresEntran_ElCuartoRecibe503ConRetryAfter()
    {
        var ayuda = new AyudaRetenida();
        using var fabrica = new FabricaConAyudaRetenida(ayuda, Ajustes(permisos: 3, cola: 0));
        using var cliente = fabrica.CreateClient();

        try
        {
            var pedidos = Enumerable.Range(0, 4)
                .Select(i => cliente.PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest($"¿Cuántos dígitos lleva el código postal? ({i})")))
                .ToList();

            // Barrera: tres adentro (retenidos) y uno ya respondido. Mientras la barrera está cerrada nadie
            // libera su permiso, así que el que respondió sólo puede ser el rechazado, llegue cuando llegue.
            await ayuda.Dentro(3).WaitAsync(Plazo);
            var primero = await Task.WhenAny(pedidos).WaitAsync(Plazo);
            var rechazado = await primero;
            Assert.Multiple(() =>
            {
                Assert.That(rechazado.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
                Assert.That(rechazado.Headers.RetryAfter?.Delta, Is.EqualTo(TimeSpan.FromSeconds(30)));
                Assert.That(pedidos.Count(p => p.IsCompleted), Is.EqualTo(1), "con la barrera cerrada sólo respondió el rechazado");
            });

            ayuda.Abrir();
            var respuestas = await Task.WhenAll(pedidos).WaitAsync(Plazo);
            Assert.Multiple(() =>
            {
                Assert.That(respuestas.Count(r => r.StatusCode == HttpStatusCode.OK), Is.EqualTo(3));
                Assert.That(respuestas.Count(r => r.StatusCode == HttpStatusCode.ServiceUnavailable), Is.EqualTo(1));
                Assert.That(ayuda.Entraron, Is.EqualTo(3), "el rechazado no llegó al puerto");
                Assert.That(ayuda.MaximoSimultaneos, Is.EqualTo(3));
            });
        }
        finally
        {
            ayuda.Abrir();
        }
    }

    [Test]
    [Description("La página y la API comparten el limitador: con los permisos tomados por la entrada de la página, /ayuda y /desde-texto dan 503; al soltarlos, /ayuda vuelve a responder")]
    public async Task PaginaYApi_CompartenElLimitador()
    {
        var ayuda = new AyudaRetenida();
        ayuda.Abrir();
        using var fabrica = new FabricaConAyudaRetenida(ayuda, Ajustes(permisos: 1, cola: 0));
        using var cliente = fabrica.CreateClient();
        var limitador = fabrica.Services.GetRequiredService<LimitadorDelModelo>();

        var soltar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var adentro = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pagina = limitador.CorrerAsync(async () => { adentro.SetResult(); await soltar.Task; });
        await adentro.Task.WaitAsync(Plazo);

        var ayudaOcupada = await cliente.PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest("¿Cuántos dígitos lleva el código postal?"));
        var altaOcupada = await cliente.PostAsJsonAsync("/api/v1/localidades/desde-texto", new TextoRequest("Agregá Goya, Corrientes, CP 3450."));
        var otraPagina = await limitador.CorrerAsync(() => Task.CompletedTask);
        Assert.Multiple(() =>
        {
            Assert.That(ayudaOcupada.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(altaOcupada.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(otraPagina, Is.False, "una segunda entrada de la página tampoco pasa");
            Assert.That(ayuda.Entraron, Is.EqualTo(0));
        });

        soltar.SetResult();
        Assert.That(await pagina.WaitAsync(Plazo), Is.True);
        var libre = await cliente.PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest("¿Cuántos dígitos lleva el código postal?"));
        Assert.That(libre.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    [Description("El ABM del caso base no pasa por el limitador: con los permisos tomados, GET /localidades responde")]
    public async Task AbmDelCasoBase_NoPasaPorElLimitador()
    {
        using var fabrica = new FabricaConAyudaRetenida(new AyudaRetenida(), Ajustes(permisos: 1, cola: 0));
        using var cliente = fabrica.CreateClient();
        using var permiso = fabrica.Services.GetRequiredService<LimitadorDelModelo>().Limitador.AttemptAcquire();
        Assert.That(permiso.IsAcquired, Is.True);

        var lista = await cliente.GetAsync("/api/v1/localidades");
        Assert.That(lista.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    [Description("El middleware libera los limitadores ociosos de su partición: liberar la vista no cierra el limitador compartido")]
    public void LiberarLaVista_NoCierraElLimitadorCompartido()
    {
        using var limitador = new LimitadorDelModelo(Options.Create(new OpcionesDeConcurrencia { Permisos = 1, Cola = 0 }));
        limitador.Vista().Dispose();
        using var permiso = limitador.Limitador.AttemptAcquire();
        Assert.That(permiso.IsAcquired, Is.True);
        using var segundo = limitador.Vista().AttemptAcquire();
        Assert.That(segundo.IsAcquired, Is.False, "la vista ve los permisos del mismo limitador");
    }

    [Test]
    [Description("Concurrencia:Permisos = 0 se rechaza al arrancar")]
    public void PermisosEnCero_NoArranca()
    {
        using var fabrica = new FabricaConAyudaRetenida(new AyudaRetenida(), Ajustes(permisos: 0, cola: 0));
        var e = Assert.Catch(() => _ = fabrica.Server);
        var cadena = new List<Exception>();
        for (var x = e; x is not null; x = x.InnerException) cadena.Add(x);
        if (e is AggregateException ag) cadena.AddRange(ag.Flatten().InnerExceptions);
        Assert.That(cadena.OfType<OptionsValidationException>(), Is.Not.Empty, e!.ToString());
    }
}

/// <summary>El host de las pruebas con el puerto de la ayuda reemplazado por <see cref="AyudaRetenida" />.</summary>
public sealed class FabricaConAyudaRetenida(AyudaRetenida ayuda, IDictionary<string, string?> ajustes) : ApiWebApplicationFactory(ajustes)
{
    protected override void ConfigurarCliente(IServiceCollection services)
    {
        base.ConfigurarCliente(services);
        services.AddSingleton<IAyuda>(ayuda);
    }
}

/// <summary>
/// Puerto de la ayuda que cuenta a los que entran y los retiene hasta <see cref="Abrir" />. Sin modelo:
/// lo que se prueba es la entrada, no la respuesta.
/// </summary>
public sealed class AyudaRetenida : IAyuda
{
    private readonly TaskCompletionSource _barrera = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object _cerrojo = new();
    private readonly List<(int Cuantos, TaskCompletionSource Aviso)> _esperas = [];
    private int _adentro;

    public int Entraron { get; private set; }
    public int MaximoSimultaneos { get; private set; }

    public void Abrir() => _barrera.TrySetResult();

    /// <summary>Se completa cuando entraron <paramref name="cuantos" />.</summary>
    public Task Dentro(int cuantos)
    {
        lock (_cerrojo)
        {
            if (Entraron >= cuantos) return Task.CompletedTask;
            var aviso = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _esperas.Add((cuantos, aviso));
            return aviso.Task;
        }
    }

    public async Task<RespuestaDeAyuda> ResponderAsync(string pregunta, CancellationToken cancellationToken = default)
    {
        lock (_cerrojo)
        {
            Entraron++;
            _adentro++;
            MaximoSimultaneos = Math.Max(MaximoSimultaneos, _adentro);
            foreach (var (cuantos, aviso) in _esperas.Where(e => Entraron >= e.Cuantos).ToList())
            {
                aviso.TrySetResult();
                _esperas.Remove((cuantos, aviso));
            }
        }
        try
        {
            await _barrera.Task.WaitAsync(cancellationToken);
            return new RespuestaDeAyuda("Cuatro dígitos.", "LocalidadPolicy.RequisitoDelCodigoPostal", true, [], "EMB-C: no ejecutado contra un modelo de embeddings");
        }
        finally
        {
            lock (_cerrojo) _adentro--;
        }
    }
}
