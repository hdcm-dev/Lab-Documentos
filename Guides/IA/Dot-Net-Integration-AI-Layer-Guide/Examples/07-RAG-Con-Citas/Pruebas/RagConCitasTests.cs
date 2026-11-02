// Nuevo: pruebas de cableado de 07 (sin servidor). Comprueban la garantía de código (toda cita
// apunta a un fragmento recuperado, o la respuesta se rechaza), el oráculo de la afirmación central
// con el caso N3 E5, el oro contra los literales y una corrida completa en modo falso.
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure;

namespace MovilidadUrbana.Asistente.Pruebas;

public class RagConCitasTests
{
    private static readonly ConjuntoDeOro Oro = ConjuntoDeOro.Leer(OroDe07.Ruta);
    private static readonly IReadOnlyList<Fragmento> Corpus = CorpusDePolicies.Fragmentos();

    private static Recuperacion Rec(string id) => new(id, Corpus.Single(f => f.Id == id).Texto, 0.5);
    private static PreguntaDeOro P(string id) => Oro.Preguntas.Single(p => p.Id == id);

    [Test]
    public void Validador_CitaRecuperada_SeAcepta()
    {
        var v = ValidadorDeCitas.Validar("Lleva 4 dígitos. [Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]", ["LocalidadPolicy.RequisitoDelCodigoPostal"]);
        Assert.That(v.Veredicto, Is.EqualTo(Veredicto.Aceptada));
        Assert.That(v.Mostrada, Is.EqualTo(v.Texto));
    }

    [Test]
    public void Validador_CitaNoRecuperada_SeRechazaSiempre()
    {
        var v = ValidadorDeCitas.Validar("Lleva 4 dígitos. [Fuente: LocalidadPolicy.RequisitoDelCodigoPostal][Fuente: EncuestaPolicy.RequisitoDeLaEdad]",
            ["LocalidadPolicy.RequisitoDelCodigoPostal"]);
        Assert.That(v.Veredicto, Is.EqualTo(Veredicto.RechazadaCitaNoRecuperada));
        Assert.That(v.Mostrada, Is.EqualTo(ValidadorDeCitas.AvisoDeRechazo));
    }

    [TestCase("No sé.", Veredicto.NoSe)]
    [TestCase("no lo sé", Veredicto.NoSe)]
    [TestCase("No se puede repetir.", Veredicto.RechazadaSinCita)]
    [TestCase("Cuatro dígitos.", Veredicto.RechazadaSinCita)]
    public void Validador_SinCita(string texto, Veredicto esperado) =>
        Assert.That(ValidadorDeCitas.Validar(texto, ["LocalidadPolicy.RequisitoDelCodigoPostal"]).Veredicto, Is.EqualTo(esperado));

    [TestCase("4 dígitos", new[] { 4 })]
    [TestCase("cuatro dígitos [Fuente: X.Y]", new[] { 4 })]
    [TestCase("Entre dieciséis y ciento diez años", new[] { 16, 110 })]
    [TestCase("Una de las provincias del catálogo.", new int[0])]
    [TestCase("Al menos uno.", new[] { 1 })]
    public void Numeros_DigitosYPalabras(string texto, int[] esperados) =>
        Assert.That(NumerosEnTexto.Valores(texto), Is.EqualTo(esperados));

    [Test]
    public void Oraculo_CasoE5_CitaYContradice_Falla()
    {
        IReadOnlyList<Recuperacion> recuperados =
            [Rec("EncuestaPolicy.RequisitoDeLosMinutos"), Rec("EncuestaPolicy.RequisitoDeLaDistancia"), Rec("EncuestaPolicy.RequisitoDeLaEdad")];
        // La cita es válida (las dos fuentes se recuperaron): por eso hace falta el oráculo además de la garantía.
        Assert.That(ValidadorDeCitas.Validar(GuionDelEjemplo.CasoE5, recuperados.Select(r => r.Fuente).ToList()).Veredicto, Is.EqualTo(Veredicto.Aceptada));

        var e = OraculoDeCitas.Evaluar(GuionDelEjemplo.PreguntaE5, GuionDelEjemplo.CasoE5, recuperados);

        Assert.That(e.Resultado, Is.EqualTo("falla"));
        Assert.That(e.CitaValida, Is.True);
        // «cero» es un valor de la distancia citada: la regla por unión de literales no lo cuenta como
        // contradicción, pero la afirmación central (los valores del literal de minutos) no está.
        Assert.That(e.Motivo, Is.EqualTo("no afirma el valor del literal relevante citado"));
    }

    [Test]
    public void Oraculo_ValorDelLiteral_Pasa_YOtroValor_Contradice()
    {
        IReadOnlyList<Recuperacion> rec = [Rec("LocalidadPolicy.RequisitoDelCodigoPostal"), Rec("LocalidadPolicy.RequisitoDeLosHabitantes"), Rec("LocalidadPolicy.RequisitoDelNombre")];
        Assert.That(OraculoDeCitas.Evaluar(P("r03"), "Lleva cuatro dígitos. [Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]", rec).Resultado, Is.EqualTo("pasa"));
        var mal = OraculoDeCitas.Evaluar(P("r03"), "Lleva 5 dígitos. [Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]", rec);
        Assert.That((mal.Resultado, mal.CitaYContradice), Is.EqualTo(("falla", true)));
        // Cita válida pero de un fragmento que no es el relevante: no afirma el valor pedido.
        Assert.That(OraculoDeCitas.Evaluar(P("r03"), "Al menos 1. [Fuente: LocalidadPolicy.RequisitoDeLosHabitantes]", rec).Resultado, Is.EqualTo("falla"));
    }

    [Test]
    public void Oraculo_NoRespondible_SoloPasaConNoSe()
    {
        IReadOnlyList<Recuperacion> rec = [Rec("LocalidadPolicy.RequisitoDeLosHabitantes"), Rec("LocalidadPolicy.RequisitoDelNombre"), Rec("LocalidadPolicy.RequisitoDeLaProvincia")];
        Assert.That(OraculoDeCitas.Evaluar(P("n01"), "No sé.", rec).Resultado, Is.EqualTo("pasa"));
        Assert.That(OraculoDeCitas.Evaluar(P("n01"), "Tiene 90000. [Fuente: LocalidadPolicy.RequisitoDeLosHabitantes]", rec).Resultado, Is.EqualTo("falla"));
    }

    [Test]
    public void Oro_TieneLaFormaDelPlan()
    {
        Assert.That(Oro.Preguntas.Count(p => p.Respondible), Is.EqualTo(12));
        Assert.That(Oro.Preguntas.Count(p => !p.Respondible), Is.EqualTo(5));
        Assert.That(Oro.Preguntas.Where(p => p.Respondible && p.SinValorVerificable).Select(p => p.Objetivo), Is.EquivalentTo(new[]
        {
            "LocalidadPolicy.RequisitoDeLaProvincia", "EncuestaPolicy.RequisitoDeLaLocalidad", "EncuestaPolicy.RequisitoDeLosMedios",
            "EncuestaPolicy.RequisitoDeLaFrecuencia", "EncuestaPolicy.RequisitoDelMotivo"
        }));
        Assert.That(Oro.Plantilla, Is.EqualTo(CorpusDePolicies.Plantilla));
        Assert.That(Oro.Version, Is.EqualTo(OroDe07.Version));
    }

    [Test]
    public void OroV2_EsElV1SinCambios_MasUnaNoRespondibleCercana()
    {
        // REC-28: el v1 queda como histórico; v2 lo contiene tal cual, en el mismo orden, y suma n05.
        var v1 = ConjuntoDeOro.Leer(OroDe07.RutaDelV1);
        // Comparación del JSON crudo de cada pregunta (incluye las «nota», que el modelo no lee).
        static List<string> Crudas(string ruta) =>
            System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(ruta))!["preguntas"]!.AsArray().Select(n => n!.ToJsonString()).ToList();
        Assert.That(Crudas(OroDe07.Ruta).Take(v1.Preguntas.Count), Is.EqualTo(Crudas(OroDe07.RutaDelV1)));
        var nueva = Oro.Preguntas.Skip(v1.Preguntas.Count).Single();
        Assert.That((nueva.Id, nueva.Tipo, nueva.Cercania, nueva.Relevantes.Count), Is.EqualTo(("n05", "no-respondible", "cercana", 0)));
        // Cercana a una regla real: el literal de los medios sólo fija el mínimo, sin un máximo que responder.
        Assert.That(NumerosEnTexto.Valores(Corpus.Single(f => f.Id == "EncuestaPolicy.RequisitoDeLosMedios").Literal), Is.EqualTo(new[] { 1 }));
        Assert.That(GuionDelEjemplo.Respuestas[nueva.Id], Is.EqualTo("No sé."));
    }

    [Test]
    public void Oro_ValoresEsperados_SonLosDelLiteralEnCodigo()
    {
        foreach (var p in Oro.Preguntas.Where(p => p.Respondible && !p.SinValorVerificable))
        {
            var literal = Corpus.Single(f => f.Id == p.Objetivo).Literal;
            var delOro = p.FormasAceptadas.Select(formas => formas.Select(NumerosEnTexto.Valores).Distinct(new SecuenciaIgual()).Single().Single()).ToList();
            Assert.That(delOro, Is.EquivalentTo(NumerosEnTexto.Valores(literal)), $"{p.Id}: oro vs. literal «{literal}»");
        }
    }

    [Test]
    public void Oro_ValoresDeCatalogo_PertenecenACatalogos()
    {
        var catalogo = Catalogos.Provincias.Concat(Catalogos.Medios.Select(m => m.Clave)).Concat(Catalogos.Frecuencias.Select(f => f.Clave))
            .Concat(Catalogos.Motivos.Select(m => m.Clave)).ToHashSet();
        Assert.That(Oro.Preguntas.SelectMany(p => p.ValoresDeCatalogo), Is.All.Matches<string>(catalogo.Contains));
    }

    [Test]
    public async Task Indice_DoceFragmentos_YTopTres()
    {
        var indice = await IndiceDeAyuda.CrearAsync(new GeneradorDeEmbeddingsEstable(FragmentoIndexado.Dimensiones));
        var hallados = await indice.BuscarAsync("¿Cuántos dígitos lleva el código postal?", 12);
        Assert.That(hallados.Select(h => h.Fuente).Distinct().Count(), Is.EqualTo(12));
        Assert.That((await indice.BuscarAsync("código postal", AsistenteDeAyuda.TopPorDefecto)).Count, Is.EqualTo(3));
    }

    [Test]
    public void Indice_RechazaOtraDimension() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => IndiceDeAyuda.CrearAsync(new GeneradorDeEmbeddingsEstable(384)));

    [Test]
    public void TablaDeEmbeddings_TextoSinVector_Falla()
    {
        var tabla = new TablaDeEmbeddings(new ArchivoDeEmbeddings
        {
            Pieza = "07-RAG-Con-Citas", Plantilla = CorpusDePolicies.Plantilla, Modelo = "m", OroSha256 = "x", Dimension = 2,
            Vectores = [new VectorGuardado("hola", [1f, 0f])]
        });
        Assert.ThrowsAsync<InvalidOperationException>(() => tabla.GenerateAsync(["otro"]));
    }

    [Test]
    public async Task CorridaFalsa_CumpleLosInvariantes_YVeLasTresMalas()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"rllm-07-{Guid.NewGuid():N}");
        try
        {
            var config = ConfiguracionDelModelo.DesdeEntorno(n => n == "RLLM_DIR" ? dir : null).Validar();
            var preguntas = Oro.Preguntas.Select(p => (EntradaId: p.Id, Pregunta: p)).Append(("e5", GuionDelEjemplo.PreguntaE5)).ToList();
            var guion = GuionDelEjemplo.Crear(preguntas.Select(p => p.Item1));
            var cliente = FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp);
            var asistente = new AsistenteDeAyuda(cliente, await IndiceDeAyuda.CrearAsync(new GeneradorDeEmbeddingsEstable(FragmentoIndexado.Dimensiones)));
            ResultadoDeLaCorrida r;
            using (var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "07-RAG-Con-Citas", Configuracion = config, Adaptador = Adaptador.OllamaSharp, Maquina = "guion" }))
            {
                r = await new CorridaDeAyuda(asistente, registro, _ => guion.Llamadas[^1].Opciones, "07-RAG-Con-Citas", "rag").CorrerAsync(preguntas, new Plazo(null));
            }

            Assert.That(r.InvariantesQueFallan, Is.Empty);
            Assert.That(guion.Llamadas, Has.Count.EqualTo(18));
            Assert.That(guion.Llamadas.All(l => l.Opciones?.Instructions == AsistenteDeAyuda.Instrucciones));
            Assert.That(guion.Llamadas.All(l => l.Opciones?.Reasoning?.Effort == Microsoft.Extensions.AI.ReasoningEffort.None && OpcionesDelProveedor.NumCtxDe(l.Opciones) == config.NumCtx));
            var por = r.Resultados.ToDictionary(x => x.EntradaId);
            Assert.That((por["e5"].Consulta!.Validada.Veredicto, por["e5"].Evaluacion.Resultado), Is.EqualTo((Veredicto.Aceptada, "falla")));
            Assert.That(por["r09"].Consulta!.Validada.Veredicto, Is.EqualTo(Veredicto.RechazadaCitaNoRecuperada));
            Assert.That(por["n02"].Consulta!.Validada.Veredicto, Is.EqualTo(Veredicto.RechazadaSinCita));
            var lineas = File.ReadAllLines(Path.Combine(dir, "07-RAG-Con-Citas.jsonl"));
            Assert.That(lineas, Has.Length.EqualTo(36));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }

    [Test]
    public async Task Plazo_Vencido_ExcluyeLoQueFalta()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"rllm-07-{Guid.NewGuid():N}");
        try
        {
            var config = ConfiguracionDelModelo.DesdeEntorno(n => n == "RLLM_DIR" ? dir : null).Validar();
            var preguntas = Oro.Preguntas.Select(p => (p.Id, p)).ToList();
            var guion = GuionDelEjemplo.Crear(preguntas.Select(p => p.Item1));
            var asistente = new AsistenteDeAyuda(FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp),
                await IndiceDeAyuda.CrearAsync(new GeneradorDeEmbeddingsEstable(FragmentoIndexado.Dimensiones)));
            using var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "07-RAG-Con-Citas", Configuracion = config, Adaptador = Adaptador.OllamaSharp });
            var r = await new CorridaDeAyuda(asistente, registro, _ => guion.Llamadas[^1].Opciones, "07-RAG-Con-Citas", "rag")
                .CorrerAsync(preguntas, new Plazo(DateTimeOffset.UtcNow.AddSeconds(-1)));
            Assert.That(r.Resultados.Select(x => x.Evaluacion.Motivo), Is.All.EqualTo("plazo"));
            Assert.That(guion.Llamadas, Is.Empty);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }

    private sealed class SecuenciaIgual : IEqualityComparer<IReadOnlyList<int>>
    {
        public bool Equals(IReadOnlyList<int>? x, IReadOnlyList<int>? y) => x!.SequenceEqual(y!);
        public int GetHashCode(IReadOnlyList<int> obj) => obj.Aggregate(17, (h, v) => h * 31 + v);
    }
}
