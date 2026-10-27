using FrameworkConcept.Core.Application.Encuestas;

namespace FrameworkConcept.Core.Tests;

/// <summary>
/// Adaptado de EncuestaViewModelTests del caso base. El ViewModel MAUI no viaja a la demo (las pantallas
/// pasan a Razor), así que cada caso se reescribe contra el servicio de aplicación, que es lo que decidía
/// las reglas: mismos datos, mismos mensajes.
/// </summary>
[TestFixture]
public class EncuestaServiceTests
{
    private TestEnvironment _entorno = default!;

    [SetUp] public void Create() => _entorno = new TestEnvironment();

    [Test]
    [Description("Arranca con cero encuestas registradas")]
    public async Task ArrancaSinRegistradas()
    {
        Assert.That(await _entorno.Encuestas.CountAsync(), Is.Zero);
    }

    [Test]
    [Description("El paso 1 con datos inválidos da un error por campo")]
    public void Paso1ConDatosInvalidos()
    {
        var model = new EncuestaModel { Edad = 12 };

        var errores = _entorno.Encuestas.ValidarPaso(1, model);

        Assert.That(errores["nombre"], Does.Contain("mínimo 3 caracteres"));
        Assert.That(errores["edad"], Does.Contain("entre 16 y 110"));
        Assert.That(errores.ContainsKey("localidad"), Is.True);
    }

    [Test]
    [Description("Marcar un medio y elegir la frecuencia deja el paso 2 sin errores")]
    public void CorregirElPaso2BorraSusErrores()
    {
        var model = new EncuestaModel();
        Assert.That(_entorno.Encuestas.ValidarPaso(2, model).Keys, Is.EquivalentTo(new[] { "medios", "frecuencia" }));

        model.AlternarMedio("colectivo", true);
        model.Frecuencia = "semanal";

        Assert.That(_entorno.Encuestas.ValidarPaso(2, model), Is.Empty);
    }

    [Test]
    [Description("Desmarcar un medio lo saca de la selección; volver a marcarlo no lo duplica")]
    public void AlternarMedio()
    {
        var model = new EncuestaModel();
        model.AlternarMedio("colectivo", true);
        model.AlternarMedio("bicicleta", true);
        model.AlternarMedio("colectivo", false);
        model.AlternarMedio("bicicleta", true);

        Assert.That(model.Medios, Is.EquivalentTo(new[] { "bicicleta" }));
    }

    [Test]
    [Description("Registra con los medios en el orden del catálogo y el resumen ya formateado")]
    public async Task RecorreYRegistra()
    {
        var model = EncuestasDePrueba.Completa();
        Assert.That(_entorno.Encuestas.ValidarPaso(1, model), Is.Empty);
        Assert.That(_entorno.Encuestas.ValidarPaso(2, model), Is.Empty);
        Assert.That(_entorno.Encuestas.ValidarPaso(3, model), Is.Empty);

        var respuesta = await _entorno.Encuestas.RegistrarAsync(model);
        var resumen = ResumenDeEncuesta.De(respuesta);

        Assert.That(resumen.Single(c => c.Clave == "medios").Valor, Is.EqualTo("Colectivo, Bicicleta"));
        Assert.That(resumen.Single(c => c.Clave == "distancia").Valor, Is.EqualTo("12,5 km"));
        Assert.That(resumen.Single(c => c.Clave == "persona").Valor, Is.EqualTo("Ana Pérez (34 años)"));
        Assert.That(await _entorno.Encuestas.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    [Description("El paso 3 fuera de rango no valida")]
    public void Paso3FueraDeRango()
    {
        var model = EncuestasDePrueba.Completa();
        model.Distancia = 900;

        var errores = _entorno.Encuestas.ValidarPaso(3, model);

        Assert.That(errores["distancia"], Does.Contain("entre 0 y 500 km"));
    }

    [Test]
    [Description("P13.2 — La respuesta guarda la definición con la que se empezó a cargar")]
    public async Task LaRespuestaGuardaSuDefinicion()
    {
        var model = EncuestasDePrueba.Completa();
        model.VersionDefinicion = 1;

        var respuesta = await _entorno.Encuestas.RegistrarAsync(model);

        Assert.That(respuesta.EncuestaId, Is.EqualTo("encuesta-movilidad"));
        Assert.That(respuesta.VersionDefinicion, Is.EqualTo(1));
    }

    [Test]
    [Description("P13.2 — Sin definición no se registra: no se podría enviar después")]
    public void SinDefinicionNoRegistra()
    {
        var model = EncuestasDePrueba.Completa();
        model.EncuestaId = string.Empty;

        Assert.ThrowsAsync<InvalidOperationException>(() => _entorno.Encuestas.RegistrarAsync(model));
        Assert.That(_entorno.EncuestasGuardadas.Guardadas, Is.Empty);
    }
}
