// Nuevo. La sesión como identidad mínima (plan §F.5): API por encabezado, circuito por cookie.
using System.Net.Http.Json;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Contracts;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Sessions;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class SesionesTests : PruebaConHost
{
    [Test]
    [Description("Dos sesiones de API concurrentes: cada una ve sólo sus localidades (N6 Q4)")]
    public async Task Api_DosSesionesConcurrentes_CadaUnaVeLasSuyas()
    {
        HttpClient Cliente(string s) { var c = Fabrica.CreateClient(); c.DefaultRequestHeaders.Add(SessionHeaderMiddleware.HeaderName, s); return c; }
        var a = Cliente("a-" + Guid.NewGuid().ToString("n")[..10]);
        var b = Cliente("b-" + Guid.NewGuid().ToString("n")[..10]);

        await Task.WhenAll(
            a.PostAsJsonAsync("/api/v1/localidades", new LocalidadRequest("Goya", "Corrientes", "3450", 90000)),
            b.PostAsJsonAsync("/api/v1/localidades", new LocalidadRequest("Bella Vista", "Corrientes", "3432", 30000)));
        var deA = await a.GetFromJsonAsync<List<LocalidadDto>>("/api/v1/localidades");
        var deB = await b.GetFromJsonAsync<List<LocalidadDto>>("/api/v1/localidades");

        Assert.That(deA!.Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia", "Goya" }));
        Assert.That(deB!.Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia", "Bella Vista" }));
    }

    [Test]
    [Description("Dos circuitos con distinto SessionId: las herramientas armadas desde cada scope ven sólo su sesión")]
    public async Task Circuito_DosSesiones_NoSeVen()
    {
        using var uno = NuevoCircuito();
        using var otro = NuevoCircuito();
        await uno.Get<LocalidadService>().SaveAsync(new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 });

        var deUno = await uno.Get<HerramientasDelAsistente>().ListarLocalidadesAsync();
        var deOtro = await otro.Get<HerramientasDelAsistente>().ListarLocalidadesAsync();

        Assert.That(deUno.Select(l => l.Nombre), Does.Contain("Goya"));
        Assert.That(deOtro.Select(l => l.Nombre), Does.Not.Contain("Goya"));
        Assert.That(deOtro.Select(l => l.Id), Does.Not.Contain(deUno.Single(l => l.Nombre == "Goya").Id));
    }
}
