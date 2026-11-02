// Nuevo: pruebas del ejemplo 04. Las de cableado corren sin red; la de Category("LLM") se salta
// si el servidor no responde o el modelo no está.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Normalizacion;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

public class NormalizacionTests
{
    private static readonly NormalizacionDeMovilidad Caso = new(new NormalizadorDeCatalogo());

    [TestCase("bondi", "colectivo")]
    [TestCase("Bici", "bicicleta")]
    [TestCase("A pie", "caminata")]
    [TestCase("Auto particular", "auto")]
    [TestCase("ómnibus", "colectivo")]
    [TestCase("tren", "tren")]
    public void Medio_SeNormalizaALaClave(string texto, string clave) =>
        Assert.That(new NormalizadorDeCatalogo().NormalizarMedio(texto), Is.EqualTo(clave));

    [TestCase("casi todos los días", "diaria")]
    [TestCase("Algunos días por semana", "semanal")]
    [TestCase("de vez en cuando", "ocasional")]
    public void Frecuencia_SeNormalizaALaClave(string texto, string clave) =>
        Assert.That(new NormalizadorDeCatalogo().NormalizarFrecuencia(texto), Is.EqualTo(clave));

    [Test]
    public void MedioInventado_ProduceErrorDeMedios_NuncaListaVacia()
    {
        var r = Caso.Decidir(new PropuestaDeMovilidad { Medios = ["Avión"], Frecuencia = "diaria" });

        Assert.That(r.Modelo.Medios, Is.EquivalentTo(new[] { "avion" }));
        Assert.That(r.Errores, Contains.Key("medios"));
        Assert.That(r.Errores["medios"], Does.Contain("avion"));
        Assert.That(r.Valido, Is.False);
    }

    [Test]
    public void MedioInventadoJuntoAUnoValido_IgualSeRechaza()
    {
        var r = Caso.Decidir(new PropuestaDeMovilidad { Medios = ["bondi", "avion"], Frecuencia = "diaria" });
        Assert.That(r.Errores, Contains.Key("medios"));
    }

    [Test]
    public void FrecuenciaFueraDeCatalogo_ProduceError()
    {
        var r = Caso.Decidir(new PropuestaDeMovilidad { Medios = ["bici"], Frecuencia = "cuando pinta" });
        Assert.That(r.Errores, Contains.Key("frecuencia"));
    }

    [Test]
    public void PropuestaValida_TodasLasClavesEnCatalogos()
    {
        var r = Caso.Decidir(new PropuestaDeMovilidad { Medios = ["bondi", "bici"], Frecuencia = "casi todos los días" });

        Assert.That(r.Valido, Is.True);
        Assert.That(r.Modelo.Medios, Is.EquivalentTo(new[] { "colectivo", "bicicleta" }));
        Assert.That(r.Modelo.Medios, Is.SubsetOf(Catalogos.Medios.Select(m => m.Clave)));
        Assert.That(r.Modelo.Frecuencia, Is.EqualTo("diaria"));
    }

    [Test]
    public void SinPropuesta_ElPasoQuedaConError()
    {
        var r = Caso.Decidir(null);
        Assert.That(r.PropuestaValida, Is.False);
        Assert.That(r.Errores, Contains.Key("medios"));
    }

    [Test]
    public void MediosNull_TerminaEnErrorDeMedios_SinExcepcion()
    {
        // «"medios": null» es JSON válido: TryGetResult da true y Medios queda en null (el «= []» del
        // record no rige). Antes, Decidir tiraba ArgumentNullException (defecto S2 del verificador).
        var propuesta = Proponer("""{"medios":null,"frecuencia":"diaria"}""");
        Assert.That(propuesta, Is.Not.Null);
        Assert.That(propuesta!.Medios, Is.Null);

        var r = Caso.Decidir(propuesta);

        Assert.That(r.Errores, Contains.Key("medios"));
        Assert.That(r.Valido, Is.False);
        Assert.That(Garantia.Comprobar("medios-null", r), Is.Empty);
    }

    [Test]
    public void MediosConNullAdentro_SeSaltean()
    {
        var r = Caso.Decidir(Proponer("""{"medios":[null,"bondi"],"frecuencia":"diaria"}"""));
        Assert.That(r.Valido, Is.True);
        Assert.That(r.Modelo.Medios, Is.EquivalentTo(new[] { "colectivo" }));
    }

    [TestCase("\uFFFE")]
    [TestCase("bondi\uFFFE")]
    public void Plegar_NoTira_ConCaracteresQueNormalizeRechaza(string texto)
    {
        // Con Normalize(FormD), U+FFFE tiraba ArgumentException adentro de Decidir.
        var r = Caso.Decidir(new PropuestaDeMovilidad { Medios = [texto], Frecuencia = "diaria" });
        Assert.That(r.Errores, Contains.Key("medios"));
    }

    [TestCaseSource(nameof(CasosDeParidad))]
    public void ParidadDelOraculo_ElCodigoDaElVeredictoCongelado(string id, string salida, string veredicto)
    {
        // Los mismos casos corren por los oráculos jq en Pruebas/paridad-del-oraculo.sh: si el código
        // y el oráculo no coinciden, el registro deja de ser recalculable (plan §E.1, H2-022).
        var r = Caso.Decidir(Proponer(salida));

        Assert.That(r.Valido ? "pasa" : "falla", Is.EqualTo(veredicto), id);
        Assert.That(Garantia.Comprobar(id, r), Is.Empty, id);
    }

    private static IEnumerable<TestCaseData> CasosDeParidad()
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "Pruebas", "paridad-del-oraculo.v1.json");
        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(ruta));
        foreach (var c in doc.RootElement.GetProperty("casos").EnumerateArray())
        {
            var id = c.GetProperty("id").GetString()!;
            yield return new TestCaseData(id, c.GetProperty("salida").GetString()!, c.GetProperty("veredicto").GetString()!)
                .SetName($"ParidadDelOraculo({id})");
        }
    }

    /// <summary>La salida pasa por la capa única y por TryGetResult, como en Program.cs.</summary>
    private static PropuestaDeMovilidad? Proponer(string salida)
    {
        var cliente = FabricaDeClientes.Envolver(new GuionChatClient(salida), new ConfiguracionDelModelo(), Adaptador.OllamaSharp);
        var respuesta = cliente.GetResponseAsync<PropuestaDeMovilidad>("x").GetAwaiter().GetResult();
        return respuesta.TryGetResult(out var p) ? p : null;
    }

    [Test]
    public void TextoNoJson_TryGetResultEsFalse()
    {
        var config = new ConfiguracionDelModelo();
        var cliente = FabricaDeClientes.Envolver(new GuionChatClient("Uso el bondi."), config, Adaptador.OllamaSharp);

        var respuesta = cliente.GetResponseAsync<PropuestaDeMovilidad>("Uso el bondi.").GetAwaiter().GetResult();

        Assert.That(respuesta.TryGetResult(out _), Is.False);
    }

    [Test]
    public void GuionConAvion_PasaPorLaCapa_YTerminaEnErrorDeMedios()
    {
        var config = new ConfiguracionDelModelo();
        var espia = new ClienteEspia(new GuionChatClient("""{"medios":["avion"],"frecuencia":"diaria"}"""));
        var cliente = FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp);

        var respuesta = cliente.GetResponseAsync<PropuestaDeMovilidad>("Voy en avión.").GetAwaiter().GetResult();
        Assert.That(respuesta.TryGetResult(out var propuesta), Is.True);
        var r = Caso.Decidir(propuesta);

        Assert.That(r.Errores, Contains.Key("medios"));
        Assert.That(espia.OpcionesVistas[^1]!.Reasoning!.Effort, Is.EqualTo(ReasoningEffort.None));
        Assert.That(OpcionesDelProveedor.NumCtxDe(espia.OpcionesVistas[^1]), Is.EqualTo(config.NumCtx));
    }

    [TestCase("entradas.v1.json")]
    [TestCase("entradas.extension.v1.json")]
    public void Estimulo_SoloUsaValoresDeCatalogos_YEstaRotulado(string archivo)
    {
        // Prueba de cableado (i) de §E.1: todo valor de catálogo de un estímulo pertenece a Catalogos.
        var datos = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, archivo));
        Assert.That(datos.Rotulo, Is.EqualTo("escrito para el ejemplo"));
        foreach (var e in datos.Entradas)
        {
            Assert.That(e.Esperado.Medios, Is.Not.Empty.And.SubsetOf(Catalogos.Medios.Select(m => m.Clave)), e.Id);
            Assert.That(Catalogos.Frecuencias.Select(f => f.Clave), Does.Contain(e.Esperado.Frecuencia), e.Id);
        }
    }

    [Test]
    public void Sinonimos_SoloApuntanAClavesDeCatalogos()
    {
        Assert.That(NormalizadorDeCatalogo.SinonimosDeMedios.Values.Distinct(), Is.SubsetOf(Catalogos.Medios.Select(m => m.Clave)));
        Assert.That(NormalizadorDeCatalogo.SinonimosDeFrecuencias.Values.Distinct(), Is.SubsetOf(Catalogos.Frecuencias.Select(f => f.Clave)));
    }
}

[Category("LLM")]
public class NormalizacionConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.Endpoint, _config.Modelo, ["completion"]);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }

    [Test]
    public async Task PropuestaDelModelo_TerminaEnClavesDelCatalogoOEnError()
    {
        var cliente = FabricaDeClientes.CrearChatOllama(_config.Endpoint, _config.Modelo, _config.Validar());
        var respuesta = await cliente.GetResponseAsync<PropuestaDeMovilidad>(
            [new ChatMessage(ChatRole.User, "Voy en bondi y a veces en bici, casi todos los días.")],
            new ChatOptions { Instructions = Instrucciones.Texto });

        var r = new NormalizacionDeMovilidad(new NormalizadorDeCatalogo()).Decidir(respuesta.TryGetResult(out var p) ? p : null);

        // Propiedad, no texto literal: o todas las claves están en Catalogos, o hay error.
        Assert.That(Garantia.Comprobar("llm", r), Is.Empty);
    }
}
