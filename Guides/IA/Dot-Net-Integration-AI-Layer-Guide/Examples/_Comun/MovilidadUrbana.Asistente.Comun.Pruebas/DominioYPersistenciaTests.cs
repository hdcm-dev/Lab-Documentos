// Nuevo: el endurecimiento declarado (T2) y la siembra rotulada, con la persistencia real sobre
// SQLite en un archivo temporal. Sin red.
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

public class DominioYPersistenciaTests
{
    private string _db = string.Empty;
    private ServiceProvider _servicios = null!;

    [SetUp]
    public void Preparar()
    {
        _db = Path.Combine(Path.GetTempPath(), $"movilidad-{Guid.NewGuid():n}.db");
        var servicios = new ServiceCollection().AddInfrastructure($"Data Source={_db};Pooling=False");
        servicios.AddScoped<LocalidadService>();
        servicios.AddScoped<EncuestaService>();
        servicios.AddScoped<ResultadosDeEncuestaService>();
        _servicios = servicios.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        DatabaseInitializer.Initialize(_servicios);
    }

    [TearDown]
    public void Limpiar()
    {
        _servicios.Dispose();
        foreach (var f in new[] { _db, _db + "-wal", _db + "-shm" }) if (File.Exists(f)) File.Delete(f);
    }

    private AsyncServiceScope Sesion(string id)
    {
        var alcance = _servicios.CreateAsyncScope();
        alcance.ServiceProvider.GetRequiredService<SessionContext>().Set(id);
        return alcance;
    }

    [Test]
    public void Encuesta_MedioFueraDeCatalogo_EsErrorDeMedios_NuncaListaVacia()
    {
        var servicio = new EncuestaService(null!);
        var modelo = new EncuestaModel { Frecuencia = "diaria" };
        modelo.AlternarMedio("avion", true);

        var errores = servicio.ValidarPaso(2, modelo);

        Assert.That(errores, Contains.Key("medios"));
        Assert.That(errores["medios"], Does.Contain("avion"));
    }

    [TestCase("frecuencia", 2)]
    [TestCase("motivo", 3)]
    public void Encuesta_ValorLibreFueraDeCatalogo_SeRechaza(string campo, int paso)
    {
        var modelo = new EncuestaModel { Frecuencia = "a veces", Motivo = "laburo", Distancia = 3, Minutos = 20 };
        modelo.AlternarMedio("colectivo", true);

        Assert.That(new EncuestaService(null!).ValidarPaso(paso, modelo), Contains.Key(campo));
    }

    [Test]
    public void Encuesta_ClavesDelCatalogo_Pasan()
    {
        var modelo = new EncuestaModel { Frecuencia = "diaria", Motivo = "trabajo", Distancia = 3, Minutos = 20 };
        modelo.AlternarMedio("colectivo", true);
        modelo.AlternarMedio("bicicleta", true);
        var servicio = new EncuestaService(null!);

        Assert.That(servicio.ValidarPaso(2, modelo), Is.Empty);
        Assert.That(servicio.ValidarPaso(3, modelo), Is.Empty);
    }

    [Test]
    public async Task Localidad_ProvinciaFueraDeCatalogo_SeRechaza_YLaValidaInformaSuId()
    {
        await using var alcance = Sesion("sesion-a");
        var servicio = alcance.ServiceProvider.GetRequiredService<LocalidadService>();

        var mala = await servicio.SaveAsync(new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes Capital", CodigoPostal = "3450", Habitantes = 90000 });
        // Goya, Corrientes, 3450, 90000: LocalidadesTests.cs l. 50 @ 8fb4c3a.
        var buena = await servicio.SaveAsync(new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 });

        Assert.That(mala.Errors, Contains.Key("provincia"));
        Assert.That(buena.IsSuccess, Is.True);
        Assert.That(buena.Id, Is.Not.Null);
    }

    [Test]
    public async Task Sesiones_NoSeVen_YCadaUnaTieneSuSiembra()
    {
        await using (var a = Sesion("sesion-a"))
        {
            await a.ServiceProvider.GetRequiredService<LocalidadService>()
                .SaveAsync(new LocalidadModel { Nombre = "Bella Vista", Provincia = "Corrientes", CodigoPostal = "3432", Habitantes = 30000 });
        }

        await using var b = Sesion("sesion-b");
        var deB = await b.ServiceProvider.GetRequiredService<LocalidadService>().GetAllAsync();

        Assert.That(deB.Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia" }));
    }

    [Test]
    public async Task Resolucion_PorNombre_SinProvinciaYAmbigua_DaError()
    {
        await using var alcance = Sesion("sesion-c");
        var servicio = alcance.ServiceProvider.GetRequiredService<LocalidadService>();
        // Estímulo escrito para el ejemplo: una segunda «Corrientes» en otra provincia del catálogo.
        await servicio.SaveAsync(new LocalidadModel { Nombre = "Corrientes", Provincia = "Santa Fe", CodigoPostal = "2000", Habitantes = 10 });

        var ambigua = await servicio.GetByNombreYProvinciaAsync("corrientes", null);
        var exacta = await servicio.GetByNombreYProvinciaAsync("Resistencia", "Chaco");

        Assert.That(ambigua.Localidad, Is.Null);
        Assert.That(ambigua.Error!.Errors, Contains.Key("provincia"));
        Assert.That(exacta.Localidad!.Provincia, Is.EqualTo("Chaco"));
    }

    [Test]
    public async Task SiembraDeEncuestas_UsaSoloElCatalogoYPersonasSinteticas()
    {
        await using var alcance = Sesion("sesion-d");
        var resultados = await alcance.ServiceProvider.GetRequiredService<ResultadosDeEncuestaService>().ResumirAsync();
        var respuestas = SiembraDeEncuestas.Para("x").ToList();

        Assert.That(resultados.Total, Is.EqualTo(SiembraDeEncuestas.Cantidad));
        Assert.That(respuestas.Select(r => r.Nombre), Has.All.Match(@"^Persona [0-9]{2}$"));
        Assert.That(respuestas.SelectMany(r => r.Medios), Has.All.Matches<string>(m => Catalogos.Medios.Any(c => c.Clave == m)));
        Assert.That(respuestas.Select(r => r.Frecuencia), Has.All.Matches<string>(f => Catalogos.Frecuencias.Any(c => c.Clave == f)));
        Assert.That(respuestas.Select(r => r.Motivo), Has.All.Matches<string>(m => Catalogos.Motivos.Any(c => c.Clave == m)));
        Assert.That(respuestas.Select(r => r.Localidad), Has.All.Matches<string>(l => l is "Corrientes" or "Resistencia"));
        // Invariante de tipo: el resumen no tiene dónde llevar nombre ni edad (H-005).
        Assert.That(typeof(ResultadosDeEncuesta).GetProperties().Select(p => p.Name), Has.None.EqualTo("Nombre").And.None.EqualTo("Edad"));
    }
}
