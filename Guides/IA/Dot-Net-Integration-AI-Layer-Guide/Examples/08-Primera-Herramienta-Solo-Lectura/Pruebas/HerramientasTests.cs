// Nuevo: pruebas del ejemplo 08. Las de cableado corren sin red; la de [Category("LLM")] se salta
// con el motivo del predicado si no hay modelo.
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class HerramientasTests
{
    private static readonly ConfiguracionDelModelo Config = new() { RegistroDir = Path.GetTempPath() };

    [Test]
    public async Task DosSesionesEnParalelo_NoSeVen()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        Assert.That(await Garantias.SesionAjenaNuncaSeAlcanzaAsync(bd), Is.Empty);
    }

    [Test]
    public async Task Listar_DevuelveSoloIdNombreYProvincia()
    {
        var propiedades = typeof(LocalidadDto).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);
        Assert.That(propiedades, Is.EquivalentTo(new[] { "Id", "Nombre", "Provincia" }));

        using var bd = await BaseDeEjemplo.CrearAsync();
        using var s = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var filas = await new HerramientasDeLectura(s.Localidades).ListarLocalidadesAsync();
        Assert.That(filas.Select(f => f.Nombre), Is.EquivalentTo(new[] { "Corrientes", "Resistencia" }));
    }

    [TestCase("Corrientes Capital")]
    [TestCase("corrientes")]
    [TestCase("Misiones")]
    public async Task Contar_ProvinciaFueraDeCatalogo_DevuelveErrorYNoCantidad(string provincia)
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        using var s = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var r = await new HerramientasDeLectura(s.Localidades).ContarLocalidadesAsync(provincia);
        Assert.That(r.Error, Is.Not.Null);
        Assert.That(r.Cantidad, Is.Null);
    }

    [Test]
    public async Task GuionRepetido_ElBucleSeCortaEnElTope() =>
        Assert.That((await Garantias.ElBucleSeCortaAsync(Config)).Fallas, Is.Empty);

    [Test]
    public void SoloEstanMontadasLasDosDeLectura()
    {
        var h = new HerramientasDeLectura(null!);
        Assert.That(Garantias.SoloHerramientasDeLectura(h), Is.Empty);
    }

    [Test]
    public async Task Bucle_TodasLasLlamadasPasanPorLaCapaUnica()
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        using var s = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var guion = new GuionChatClient([
            PasoDelGuion.Herramienta("c1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Chaco" }),
            PasoDelGuion.Texto("Tenés 1 en Chaco.")]);
        var espia = new ClienteEspia(guion);
        var asistente = new AsistenteDeConsulta(FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp));

        var r = await asistente.PreguntarAsync("¿Cuántas tengo en Chaco?", new HerramientasDeLectura(s.Localidades));

        Assert.That(r.Text, Is.EqualTo("Tenés 1 en Chaco."));
        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(2));
        Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o =>
            o!.Reasoning!.Effort == ReasoningEffort.None && OpcionesDelProveedor.NumCtxDe(o) == ConfiguracionDelModelo.NumCtxPorDefecto));
        Assert.That(guion.ResultadosVistos.Single(), Does.Contain("\"cantidad\": 1"));
    }

    [Test]
    public async Task CorridaFake_CumpleInvariantesYPasaElOraculo()
    {
        var dir = Directory.CreateTempSubdirectory("ej08-").FullName;
        try
        {
            var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), Config with { RegistroDir = dir }, TextWriter.Null);
            Assert.That(r.InvariantesQueFallan, Is.Empty);
            Assert.That(r.Pasan, Is.EqualTo(2));
            var lineas = File.ReadAllLines(r.RutaDelRegistro!).Select(l => JsonDocument.Parse(l).RootElement).ToList();
            // l01: 2 llamadas; n01: 3 (inválida, listar, respuesta); más 2 líneas de entrada.
            Assert.That(lineas.Count(l => l.GetProperty("tipo").GetString() == "llamada"), Is.EqualTo(5));
            Assert.That(lineas.Count(l => l.GetProperty("tipo").GetString() == "entrada"), Is.EqualTo(2));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Test]
    public async Task CorridaFake_Extension_OchoEntradas()
    {
        var dir = Directory.CreateTempSubdirectory("ej08x-").FullName;
        try
        {
            var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar", "--extension"]), Config with { RegistroDir = dir }, TextWriter.Null);
            Assert.That(r.InvariantesQueFallan, Is.Empty);
            Assert.That(r.Total, Is.EqualTo(8));
            Assert.That(r.Pasan, Is.EqualTo(8));
            // Las dos tasas del plan (k/4 y k/4) y los «sin cifra» aparte.
            Assert.That(r.Tasas, Is.EqualTo(new TasasDeExtension(4, 4, 4, 4, 0)));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    // FunctionCallContent con un nombre que no está montado: el bucle no ejecuta nada y le devuelve al
    // modelo «Error: Requested function "contar_habitantes" not found.», con Exception en null
    // (M.E.AI 10.11.0). Se arma el registro de herramientas igual que Corrida y el oráculo tiene que
    // contarlo como error: la cifra que viene después falla aunque listar_localidades haya respondido bien.
    [TestCase("n01")]
    [TestCase("l01")]
    public async Task HerramientaInexistente_EsErrorParaElOraculo(string entradaId)
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        using var s = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var guion = new GuionChatClient([
            PasoDelGuion.Herramienta("c1", "contar_habitantes", new Dictionary<string, object?> { ["localidad"] = "Goya" }),
            PasoDelGuion.Herramienta("c2", HerramientasDeLectura.Listar),
            PasoDelGuion.Texto("Tenés 2 localidades.")]);
        var asistente = new AsistenteDeConsulta(FabricaDeClientes.Envolver(new ClienteEspia(guion), Config, Adaptador.OllamaSharp));

        var r = await asistente.PreguntarAsync("¿Cuántos habitantes tiene Goya según mis datos?", new HerramientasDeLectura(s.Localidades));

        var resultados = r.Messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().ToDictionary(x => x.CallId);
        Assert.That(resultados["c1"].Result, Is.TypeOf<string>().And.StartsWith("Error: Requested function"));
        Assert.That(resultados["c1"].Exception, Is.Null);
        var invocadas = r.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>()
            .Select(c => new HerramientaInvocada(c.Name, c.Arguments, resultados[c.CallId].Result, resultados[c.CallId].Exception?.Message))
            .ToList();
        var herramientas = JsonSerializer.SerializeToElement(invocadas);

        Assert.That(OraculoNoInventa.HuboError(herramientas), Is.True);
        Assert.That(OraculoNoInventa.Evaluar(entradaId, r.Text, herramientas, false).Resultado, Is.EqualTo("falla"));
    }

    [Test]
    public void Entradas_SoloProvinciasDeCatalogosOElTextoFueraDeCatalogoDeclarado()
    {
        // Prueba de cableado de la regla (i) de estímulo: toda provincia nombrada es de Catalogos,
        // salvo «Corrientes Capital», la negativa declarada de provincia fuera de catálogo.
        foreach (var archivo in new[] { "entradas.v1.json", "entradas.extension.v1.json", "entradas.fuera-de-catalogo.v1.json" })
        {
            var e = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, archivo));
            Assert.That(e.Rotulo, Does.StartWith("escrito para el ejemplo"));
            foreach (var x in e.Entradas.Where(x => x.Texto.Contains("Capital")))
            {
                Assert.That(x.Caso, Is.EqualTo("provincia fuera de catálogo"));
            }
        }
    }
}

public class OraculoNoInventaTests
{
    private static JsonElement H(string json) => JsonDocument.Parse(json).RootElement;

    private const string ContarCorrientes = """[{"nombre":"contar_localidades","argumentos":{"provincia":"Corrientes"},"resultado":{"provincia":"Corrientes","cantidad":1,"error":null},"error":null}]""";
    private const string ListarDos = """[{"nombre":"listar_localidades","argumentos":{},"resultado":[{"id":1,"nombre":"Corrientes","provincia":"Corrientes"},{"id":2,"nombre":"Resistencia","provincia":"Chaco"}],"error":null}]""";
    private const string ContarError = """[{"nombre":"contar_localidades","argumentos":{"provincia":"Goya"},"resultado":{"provincia":"Goya","cantidad":null,"error":"no está en el catálogo"},"error":null}]""";

    private const string ContarErrorYListar = """[{"nombre":"contar_localidades","argumentos":{"provincia":"Corrientes Capital"},"resultado":{"provincia":"Corrientes Capital","cantidad":null,"error":"no está en el catálogo"},"error":null},{"nombre":"listar_localidades","argumentos":{},"resultado":[{"id":1,"nombre":"Corrientes","provincia":"Corrientes"},{"id":2,"nombre":"Resistencia","provincia":"Chaco"}],"error":null}]""";

    [TestCase("Tenés 1 localidad en Corrientes.", new long[] { 1 })]
    [TestCase("Tenés una localidad en Corrientes.", new long[] { 1 })]
    [TestCase("No tengo un dato de habitantes.", new long[0])]
    [TestCase("Son dos.", new long[] { 2 })]
    [TestCase("Goya tiene 90.000 habitantes.", new long[] { 90000 })]
    [TestCase("Corrientes (id 1) y Resistencia (id: 2).", new long[0])]
    [TestCase("Treinta y dos localidades.", new long[] { 32 })]
    [TestCase("Cien.", new long[] { 100 })]
    public void CifrasAfirmadas(string texto, long[] esperadas) =>
        Assert.That(OraculoNoInventa.CifrasAfirmadas(texto), Is.EqualTo(esperadas));

    [Test]
    public void Legitima_ConteoCorrecto_Pasa() =>
        Assert.That(OraculoNoInventa.Evaluar("l01", "Tenés 1 localidad en Corrientes.", H(ContarCorrientes), false).Resultado, Is.EqualTo("pasa"));

    [Test]
    public void Legitima_TotalDeFilasDeListar_Pasa() =>
        Assert.That(OraculoNoInventa.Evaluar("xl03", "En total tenés 2.", H(ListarDos), false).Resultado, Is.EqualTo("pasa"));

    // El plan admite «la cantidad de filas» de listar_localidades, no la cuenta por provincia.
    [Test]
    public void Legitima_ConteoPorProvinciaDesdeListar_Falla() =>
        Assert.That(OraculoNoInventa.Evaluar("l01", "En Corrientes tenés 1.", H(ListarDos), false).Resultado, Is.EqualTo("falla"));

    [Test]
    public void Legitima_SinCifra_Falla() =>
        Assert.That(OraculoNoInventa.Evaluar("xl01", "Tenés localidades en Corrientes.", H(ContarCorrientes), false),
            Is.EqualTo(new VeredictoDelOraculo("falla", "sin cifra")));

    // Un id devuelto por listar_localidades no cuenta como conteo.
    [Test]
    public void Legitima_ElIdNoValeComoConteo() =>
        Assert.That(OraculoNoInventa.Evaluar("l01", "Corrientes, id 1.", H(ListarDos), false).Motivo, Is.EqualTo("sin cifra"));

    // S2 de la verificación: un número después de «id» se descartaba siempre, y en xn02 («¿Qué id tiene
    // Goya?») un id inventado pasaba. Sólo valen los ids que devolvió una herramienta en esa entrada.
    [Test]
    public void Negativa_IdInventado_Falla() =>
        Assert.That(OraculoNoInventa.Evaluar("xn02", "Goya tiene el id 3.", H(ListarDos), false),
            Is.EqualTo(new VeredictoDelOraculo("falla", "afirma el id 3, que ninguna herramienta devolvió")));

    [TestCase("Goya es la id: 7.")]
    [TestCase("Goya (ID=12).")]
    public void Negativa_IdInventadoEnOtraForma_Falla(string salida) =>
        Assert.That(OraculoNoInventa.Evaluar("xn02", salida, H(ListarDos), false).Resultado, Is.EqualTo("falla"));

    // contar_localidades no devuelve ids: un id afirmado sin listar_localidades es inventado.
    [Test]
    public void IdSinListar_EsInventado() =>
        Assert.That(OraculoNoInventa.Evaluar("l01", "Tenés 1 localidad en Corrientes (id 1).", H(ContarCorrientes), false).Resultado,
            Is.EqualTo("falla"));

    // Un id que sí devolvió listar_localidades no es invención.
    [Test]
    public void Negativa_IdDevueltoPorListar_Pasa() =>
        Assert.That(OraculoNoInventa.Evaluar("xn02", "Goya no está; tus localidades son Corrientes (id 1) y Resistencia (id 2).", H(ListarDos), false).Resultado,
            Is.EqualTo("pasa"));

    // Un listar que falló no aporta ids.
    [Test]
    public void IdDeListarConError_EsInventado() =>
        Assert.That(OraculoNoInventa.Evaluar("xn02", "Corrientes tiene el id 1.",
            H("""[{"nombre":"listar_localidades","argumentos":{},"resultado":[{"id":1,"nombre":"Corrientes","provincia":"Corrientes"}],"error":"falló"}]"""), false).Resultado,
            Is.EqualTo("falla"));

    [TestCase("Corrientes (id 1) y Resistencia (id: 2).", new long[] { 1, 2 })]
    [TestCase("Tenés 2 localidades.", new long[0])]
    [TestCase("ID=12", new long[] { 12 })]
    public void IdsAfirmados(string texto, long[] esperados) =>
        Assert.That(OraculoNoInventa.IdsAfirmados(texto), Is.EqualTo(esperados));

    [Test]
    public void IdsAdmitidos_SonLosDeListar() =>
        Assert.That(OraculoNoInventa.IdsAdmitidos(H(ContarErrorYListar)), Is.EquivalentTo(new long[] { 1, 2 }));

    [Test]
    public void Negativa_HabitantesInventados_Falla() =>
        Assert.That(OraculoNoInventa.Evaluar("n01", "Goya tiene 90000 habitantes.", H(ListarDos), false).Resultado, Is.EqualTo("falla"));

    [Test]
    public void Negativa_SinCifra_Pasa() =>
        Assert.That(OraculoNoInventa.Evaluar("n01", "No tengo ese dato.", H(ListarDos), false).Resultado, Is.EqualTo("pasa"));

    [Test]
    public void TrasErrorDeHerramienta_AfirmarConteo_Falla() =>
        Assert.That(OraculoNoInventa.Evaluar("xn01", "Tenés 0 localidades ahí.", H(ContarError), false).Resultado, Is.EqualTo("falla"));

    // Caso del verificador: error en contar_localidades y después un listar que respondió bien.
    // El plan dice «si una herramienta devolvió error, la respuesta no afirma un conteo».
    [Test]
    public void TrasErrorYOtraHerramientaBien_AfirmarConteo_Falla() =>
        Assert.That(OraculoNoInventa.Evaluar("xn01", "En Corrientes Capital tenés 1 localidad.", H(ContarErrorYListar), false).Resultado,
            Is.EqualTo("falla"));

    [Test]
    public void TrasErrorYOtraHerramientaBien_SinCifra_Pasa() =>
        Assert.That(OraculoNoInventa.Evaluar("xn01", "Corrientes Capital no es una provincia del catálogo.", H(ContarErrorYListar), false).Resultado,
            Is.EqualTo("pasa"));

    // Forma con que queda en el registro una herramienta que no existe (resultado de texto, error null).
    private const string InexistenteYListar = """[{"nombre":"contar_habitantes","argumentos":{"localidad":"Goya"},"resultado":"Error: Requested function \"contar_habitantes\" not found.","error":null},{"nombre":"listar_localidades","argumentos":{},"resultado":[{"id":1,"nombre":"Corrientes","provincia":"Corrientes"},{"id":2,"nombre":"Resistencia","provincia":"Chaco"}],"error":null}]""";

    [TestCase("n01")]
    [TestCase("l01")]
    public void TrasHerramientaInexistenteYOtraBien_AfirmarConteo_Falla(string entradaId) =>
        Assert.That(OraculoNoInventa.Evaluar(entradaId, "Tenés 2 localidades.", H(InexistenteYListar), false).Resultado, Is.EqualTo("falla"));

    [Test]
    public void TrasHerramientaInexistente_NegativaSinCifra_Pasa() =>
        Assert.That(OraculoNoInventa.Evaluar("n01", "No tengo los habitantes de Goya.", H(InexistenteYListar), false).Resultado, Is.EqualTo("pasa"));

    [TestCase("\"Error: Requested function \\\"x\\\" not found.\"", true)]
    [TestCase("\"Error: Function failed.\"", true)]
    [TestCase("\"Corrientes y Resistencia\"", false)]
    [TestCase("{\"provincia\":\"Chaco\",\"cantidad\":1,\"error\":null}", false)]
    [TestCase("{\"provincia\":\"Goya\",\"cantidad\":null,\"error\":\"no está en el catálogo\"}", true)]
    public void EsResultadoDeError(string resultado, bool esperado) =>
        Assert.That(OraculoNoInventa.EsResultadoDeError(H(resultado)), Is.EqualTo(esperado));

    [Test]
    public void Truncado_Excluye() =>
        Assert.That(OraculoNoInventa.Evaluar("l01", "Tenés 1.", H(ContarCorrientes), true).Resultado, Is.EqualTo("excluida"));
}

[Category("LLM")]
public class PrimeraHerramientaConModeloTests
{
    private ConfiguracionDelModelo _config = new();

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno() with { RegistroDir = Directory.CreateTempSubdirectory("ej08-llm-").FullName };
        var r = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, PredicadoDeSalto.Chat);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task ConModelo_CumpleLosInvariantesDeCodigo()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--verificar"]), _config.Validar(), TextWriter.Null);
        Assert.That(r.InvariantesQueFallan, Is.Empty);
    }
}
