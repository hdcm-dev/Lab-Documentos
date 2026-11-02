// Nuevo: pruebas sin servidor del ejemplo 03 (garantías del código; plan §E.2 fila 03).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Domain.Rules;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistencia;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class SalidaEstructuradaTests
{
    private static readonly ConfiguracionDelModelo Config = new();

    private static (AltaDesdeTextoService Alta, ExtractorDeLocalidadConModelo Extractor, RepositorioDeLocalidadesEnMemoria Repo, ClienteEspia Espia)
        Armar(params string[] respuestas)
    {
        // El espía va DEBAJO de la capa única: ve lo que saldría hacia el servidor.
        var espia = new ClienteEspia(new GuionChatClient(respuestas));
        var cliente = FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp);
        var extractor = new ExtractorDeLocalidadConModelo(cliente);
        var repo = new RepositorioDeLocalidadesEnMemoria();
        return (new AltaDesdeTextoService(extractor, new LocalidadService(repo)), extractor, repo, espia);
    }

    [Test]
    public async Task ConElDato_QuedaLista_YSeGuardaAlConfirmar()
    {
        var (alta, _, repo, _) = Armar("""{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""");
        var decision = await alta.ProponerAsync("Dar de alta Goya, Corrientes, código postal 3450, con 90000 habitantes.");

        Assert.That(decision.Codigo, Is.EqualTo("lista"));
        var guardado = await alta.ConfirmarAsync(decision);
        Assert.That(guardado.IsSuccess, Is.True);
        Assert.That(repo.Altas.Single().Habitantes, Is.EqualTo(90000));
    }

    [Test]
    public async Task HabitantesNull_ErrorDeDominioHabitantes_YNoSeEscribe()
    {
        var (alta, _, repo, _) = Armar("""{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":null}""");
        var decision = await alta.ProponerAsync("En Bella Vista, Corrientes, CP 3432, no sé cuántos viven");

        Assert.That(decision.Estado, Is.EqualTo(EstadoDeLaPropuesta.FaltanDatos));
        Assert.That(decision.Errores.Keys, Is.EquivalentTo(new[] { "habitantes" }));
        var guardado = await alta.ConfirmarAsync(decision);
        Assert.That(guardado.IsSuccess, Is.False);
        Assert.That(guardado.Errors.Keys, Does.Contain("habitantes"));
        Assert.That(repo.Altas, Is.Empty);
    }

    [Test]
    public async Task HabitantesInventados_SeDescartan_YLaPersonaPuedeConfirmarElDato()
    {
        // El «modelo» inventa 30000, un número plausible que pasaría HabitantesValidos.
        var (alta, _, repo, _) = Armar("""{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":30000}""");
        var decision = await alta.ProponerAsync("En Bella Vista, Corrientes, CP 3432, no sé cuántos viven");

        Assert.That(decision.Descartados, Is.EquivalentTo(new[] { "habitantes" }));
        Assert.That(decision.Modelo.Habitantes, Is.Null);
        Assert.That(decision.Codigo, Is.EqualTo("faltan:habitantes"));

        var guardado = await alta.ConfirmarAsync(decision, habitantesConfirmados: 30000);
        Assert.That(guardado.IsSuccess, Is.True);
        Assert.That(repo.Altas.Single().Habitantes, Is.EqualTo(30000));
    }

    [Test]
    public async Task CodigoPostalQueNoEstaEnElTexto_SeDescarta()
    {
        var (alta, _, _, _) = Armar("""{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""");
        var decision = await alta.ProponerAsync("Dar de alta Goya, Corrientes, con 90000 habitantes.");

        Assert.That(decision.Descartados, Is.EquivalentTo(new[] { "codigoPostal" }));
        Assert.That(decision.Codigo, Is.EqualTo("faltan:codigoPostal"));
    }

    // REC-16: «CP 34» en el texto y el «modelo» propone 3400, que tiene forma válida y pasaría
    // CodigoPostalValido. La garantía lo descarta porque no está en el texto, y no se escribe nada.
    [Test]
    public async Task CodigoPostalInventadoConFormaValida_CP34Propone3400_SeDescarta_YNoSeEscribe()
    {
        Assert.That(LocalidadRules.CodigoPostalValido("3400"), Is.True, "3400 pasa las reglas: sólo lo frena la garantía");
        var (alta, _, repo, _) = Armar("""{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3400","habitantes":89959}""");
        var decision = await alta.ProponerAsync("Agregá Goya, Corrientes, CP 34, 89959 habitantes");

        Assert.That(decision.Descartados, Is.EquivalentTo(new[] { "codigoPostal" }));
        Assert.That(decision.Modelo.CodigoPostal, Is.Empty);
        Assert.That(decision.Modelo.Habitantes, Is.EqualTo(89959));
        Assert.That(decision.Codigo, Is.EqualTo("faltan:codigoPostal"));
        Assert.That((await alta.ConfirmarAsync(decision)).IsSuccess, Is.False);
        Assert.That(repo.Altas, Is.Empty);
    }

    [Test]
    public async Task CodigoPostalDelTextoSinCuatroDigitos_NoSeDescarta_LoRechazanLasReglas()
    {
        var (alta, _, repo, _) = Armar("""{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"34","habitantes":89959}""");
        var decision = await alta.ProponerAsync("Agregá Goya, Corrientes, CP 34, 89959 habitantes");

        Assert.That(decision.Descartados, Is.Empty);
        Assert.That(decision.Codigo, Is.EqualTo("faltan:codigoPostal"));
        Assert.That((await alta.ConfirmarAsync(decision)).IsSuccess, Is.False);
        Assert.That(repo.Altas, Is.Empty);
    }

    // REC-16, «todos los campos»: los campos numéricos de la propuesta son los que cubre la garantía;
    // si se agrega un campo, esta prueba obliga a decidir si la garantía lo cubre.
    [Test]
    public void Garantia_CubreTodosLosCamposNumericosDeLaPropuesta()
    {
        var campos = typeof(LocalidadPropuesta).GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal);
        Assert.That(campos, Is.EqualTo(new[] { "CodigoPostal", "Habitantes", "Nombre", "Provincia" }));

        var (p, descartados) = GarantiaDelTexto.Aplicar(
            new LocalidadPropuesta { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3400", Habitantes = 30000 },
            "Agregá Goya, Corrientes, CP 34, 89959 habitantes");
        Assert.That(descartados, Is.EquivalentTo(new[] { "codigoPostal", "habitantes" }));
        Assert.That((p.CodigoPostal, p.Habitantes), Is.EqualTo(((string?)null, (int?)null)));
    }

    // REC-17 en el alta desde texto: las reglas endurecidas de _Comun también deciden acá, antes de
    // SaveAsync. El CP con dígitos no ASCII pasa la garantía (\d los reconoce) y lo frena la regla.
    [Test]
    public void CPConDigitosNoAscii_YNombreDe61_LosRechazanLasReglas_ConElMensajeDelLargo()
    {
        var decision = AltaDesdeTextoService.Decidir(new(new LocalidadPropuesta
            { Nombre = new string('a', 61), Provincia = "Corrientes", CodigoPostal = "٣٤٥٠", Habitantes = 90000 }, []));
        Assert.That(decision.Codigo, Is.EqualTo("faltan:codigoPostal,nombre"));
        Assert.That(decision.Errores["nombre"], Is.EqualTo("El nombre debe tener entre 3 y 60 caracteres."));
    }

    [Test]
    public async Task TextoNoJson_TryGetResultFalse_SinPropuesta_YNoSeEscribe()
    {
        var (alta, _, repo, _) = Armar("La localidad es Bella Vista, en Corrientes.");
        var decision = await alta.ProponerAsync("En Bella Vista, Corrientes, CP 3432, no sé cuántos viven");

        Assert.That(decision.Estado, Is.EqualTo(EstadoDeLaPropuesta.SinPropuesta));
        Assert.That((await alta.ConfirmarAsync(decision)).IsSuccess, Is.False);
        Assert.That(repo.Altas, Is.Empty);
    }

    [Test]
    public async Task TryGetResult_FalseConTextoNoJson()
    {
        var cliente = FabricaDeClientes.Envolver(new GuionChatClient("no es JSON"), Config, Adaptador.OllamaSharp);
        var respuesta = await cliente.GetResponseAsync<LocalidadPropuesta>("Goya");
        Assert.That(respuesta.TryGetResult(out _), Is.False);
    }

    [TestCase("corrientes", "Corrientes")]
    [TestCase(" Entre Rios ", "Entre Ríos")]
    [TestCase("CÓRDOBA", "Córdoba")]
    [TestCase("Corrientes Capital", "Corrientes Capital")]
    public void Provincia_SeNormalizaContraElCatalogo(string propuesta, string esperada) =>
        Assert.That(AltaDesdeTextoService.NormalizarProvincia(propuesta), Is.EqualTo(esperada));

    [Test]
    public void ProvinciaFueraDelCatalogo_LaRechazanLasReglas()
    {
        var decision = AltaDesdeTextoService.Decidir(new(new LocalidadPropuesta
            { Nombre = "Corrientes", Provincia = "Corrientes Capital", CodigoPostal = "3400", Habitantes = 346334 }, []));
        Assert.That(decision.Codigo, Is.EqualTo("faltan:provincia"));
    }

    [TestCase("tiene 90.000 habitantes", "90000")]
    [TestCase("CP 3432, 30000", "3432,30000")]
    [TestCase("sin números", "")]
    public void Numeros_DelTexto(string texto, string esperados) =>
        Assert.That(string.Join(",", GarantiaDelTexto.Numeros(texto)), Is.EqualTo(esperados));

    [Test]
    public void ElMismoNumeroNoSirveDeCodigoPostalYDeHabitantes()
    {
        var (p, descartados) = GarantiaDelTexto.Aplicar(
            new LocalidadPropuesta { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 3450 },
            "Goya, Corrientes, 3450");
        Assert.That(p.Habitantes, Is.Null);
        Assert.That(descartados, Is.EquivalentTo(new[] { "habitantes" }));
    }

    [Test]
    public async Task Cableado_TodaLlamadaSaleConRazonamientoApagadoNumCtxYEsquema()
    {
        var (_, extractor, _, espia) = Armar("""{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""");
        await extractor.ExtraerAsync("Goya, Corrientes, 3450, 90000");

        var opciones = espia.OpcionesVistas.Single()!;
        Assert.That(opciones.Reasoning?.Effort, Is.EqualTo(ReasoningEffort.None));
        Assert.That(OpcionesDelProveedor.NumCtxDe(opciones), Is.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
        Assert.That(opciones.ResponseFormat, Is.InstanceOf<ChatResponseFormatJson>());
        Assert.That(((ChatResponseFormatJson)opciones.ResponseFormat!).Schema, Is.Not.Null);
    }

    [Test]
    public void Compartido_SinNumCtxDelArnes_Rechaza() =>
        Assert.Throws<ConfiguracionInvalidaException>(() =>
            ConfiguracionDelModelo.DesdeEntorno(n => n == "OLLAMA_SERVIDOR" ? "compartido" : null).Validar());

    [TestCase("entradas.v1.json", 2, 1)]
    [TestCase("entradas.ext.v1.json", 8, 4)]
    [TestCase("entradas.cp34.v1.json", 1, 0)]
    public void Estimulos_RotuladosConProvinciasDelCatalogo(string archivo, int m, int sinDato)
    {
        var e = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, archivo));
        Assert.That(e.Rotulo, Is.EqualTo("escrito para el ejemplo"));
        Assert.That(e.Entradas, Has.Count.EqualTo(m));
        Assert.That(e.Entradas.Count(x => !x.TraeHabitantes), Is.EqualTo(sinDato));
        Assert.That(e.Entradas.Select(x => x.Id), Is.Unique);
        Assert.That(e.Entradas.Select(x => x.Provincia), Is.All.AnyOf(Catalogos.Provincias.Cast<object>().ToArray()));
        // Una entrada sin el dato no trae ningún número fuera del código postal.
        foreach (var x in e.Entradas.Where(x => !x.TraeHabitantes))
            Assert.That(GarantiaDelTexto.Numeros(x.Texto), Has.Count.EqualTo(1), x.Id);
    }

    [Test]
    public void GuionYDecisionesCubrenTodasLasEntradas()
    {
        foreach (var archivo in new[] { "entradas.v1.json", "entradas.ext.v1.json", "entradas.cp34.v1.json" })
        foreach (var x in ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, archivo)).Entradas)
        {
            Assert.That(GuionDelEjemplo.Respuestas.ContainsKey(x.Id) && GuionDelEjemplo.DecisionEsperada.ContainsKey(x.Id), Is.True, x.Id);
        }
    }
}
