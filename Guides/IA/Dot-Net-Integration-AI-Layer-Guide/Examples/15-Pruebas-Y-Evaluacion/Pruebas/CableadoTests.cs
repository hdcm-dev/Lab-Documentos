// Nuevo: pruebas de cableado de 15 (sin servidor; el arnés las corre en la fase build). Cubren las
// tres capas de prueba de §10: el código propio (deterministas, contrato de catálogo), el contrato
// con el juez (formato <S2>, capa única de opciones, caché del reporte) y la corrida completa con un
// juez guionado cuyo resultado se conoce de antemano.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CableadoTests
{
    private static readonly ConjuntoDeOro Oro = ConjuntoDeOro.Leer();
    private static readonly SalidasDe07 Salidas = SalidasDe07.Leer();
    private static readonly CalibracionDelJuez Calibracion = CalibracionDelJuez.Leer();
    private static readonly IReadOnlyList<CasoDeEvaluacion> Casos = CasoDeEvaluacion.Armar(Oro, Salidas, "sha-salidas", Calibracion, "sha-calibracion");

    private string _dir = null!;

    [SetUp]
    public void Directorio() => _dir = Directory.CreateTempSubdirectory("ej15-").FullName;

    [TearDown]
    public void Limpiar() => Directory.Delete(_dir, recursive: true);

    private ConfiguracionDelModelo Config => new() { RegistroDir = _dir };

    // ------------------------------------------------------------ entradas congeladas

    [Test]
    public void SalidasDe07_SonLas16DelOro_ConSuVeredicto()
    {
        Assert.That(Salidas.Salidas.Select(s => s.Id), Is.EqualTo(Oro.Preguntas.Select(p => p.Id)));
        Assert.That(Salidas.Salidas.All(s => s.Resultado07 is "pasa" or "falla"), Is.True);
        Assert.That(Salidas.Salidas.All(s => s.Recuperados.Count == 3 && s.Recuperados.All(id => s.Contexto.Contains($"[Fuente: {id}]"))), Is.True);
        Assert.That(Salidas.Procedencia.Ejemplo, Is.EqualTo("07-RAG-Con-Citas"));
    }

    [Test]
    public void Calibracion_CincoCorrectasTalCual_YCincoPerturbacionesDeLasTresClases()
    {
        Assert.That(Calibracion.Casos, Has.Count.EqualTo(10));
        Assert.That(Calibracion.Anotador, Does.Contain("no por una persona"));
        var correctas = Calibracion.Casos.Where(c => c.Tipo == TipoDeCaso.Correcta).ToList();
        Assert.That(correctas, Has.Count.EqualTo(5));
        // «Tal cual»: el texto es exactamente la salida de 07, que en 07 pasó el oráculo.
        foreach (var c in correctas)
        {
            var s = Salidas.Salidas.Single(x => x.Id == c.Base);
            Assert.That((c.Respuesta, s.Resultado07), Is.EqualTo((s.Salida, "pasa")), c.Id);
        }
        Assert.That(Calibracion.Casos.Where(c => c.Tipo != TipoDeCaso.Correcta).Select(c => c.Tipo).Distinct(),
            Is.EquivalentTo(new[] { TipoDeCaso.CitaYContradice, TipoDeCaso.SinCita, TipoDeCaso.CitaNoRecuperada }));
        // Toda perturbación sale de una respuesta correcta de la misma lista.
        Assert.That(Calibracion.Casos.Where(c => c.Tipo != TipoDeCaso.Correcta).All(c => correctas.Any(x => x.Base == c.Base)), Is.True);
    }

    [Test]
    public void Estimulos_SoloCitanFragmentosDelCaso()
    {
        // Condición (i) de §E.1: el estímulo usa sólo entidades del caso; una cita a un id que no existe
        // sería un dato inventado, no una perturbación controlada.
        var ids = CorpusDePolicies.Fragmentos().Select(f => f.Id).ToHashSet();
        var citados = Calibracion.Casos.SelectMany(c => ValidadorDeCitas.Citas(c.Respuesta)).ToList();
        Assert.That(citados, Is.Not.Empty);
        Assert.That(citados.All(ids.Contains), Is.True, string.Join(", ", citados.Where(c => !ids.Contains(c))));
        // Las «cita no recuperada» citan un fragmento que existe pero que no se recuperó para esa pregunta.
        foreach (var c in Calibracion.Casos.Where(c => c.Tipo == TipoDeCaso.CitaNoRecuperada))
        {
            var rec = Salidas.Salidas.Single(s => s.Id == c.Base).Recuperados;
            Assert.That(ValidadorDeCitas.Citas(c.Respuesta).Any(id => !rec.Contains(id)), Is.True, c.Id);
        }
    }

    // ------------------------------------------------------------ deterministas (sin modelo)

    [Test]
    public async Task Deterministas_SobreLaCalibracion_DicenLoAnotado()
    {
        foreach (var caso in Casos.Where(c => c.Grupo == GrupoDeEntrada.Calibracion))
        {
            var (resultado, _, _) = await Deterministas.EvaluarAsync(caso);
            Assert.That(resultado, Is.EqualTo(caso.Anotacion!.Determinista), caso.EntradaId);
        }
    }

    [Test]
    public async Task Deterministas_SobreLas16_DicenLoMismoQueEn07()
    {
        foreach (var caso in Casos.Where(c => c.Grupo == GrupoDeEntrada.Salidas07))
        {
            var (resultado, _, _) = await Deterministas.EvaluarAsync(caso);
            Assert.That(resultado, Is.EqualTo(caso.Resultado07), caso.EntradaId);
        }
    }

    [Test]
    public async Task Deterministas_SinCita_LaDetectaSoloLaCita()
    {
        var caso = Casos.Single(c => c.EntradaId == "c08");
        var (resultado, citaValida, _) = await Deterministas.EvaluarAsync(caso);
        Assert.That((resultado, citaValida), Is.EqualTo(("falla", false)));
        // El texto dice lo mismo que el contexto: lo que falta es la cita, no el fundamento.
        Assert.That(caso.Anotacion!.Fundamentada, Is.True);
    }

    private static async Task<BooleanMetric> Catalogo(string json) =>
        (await new EvaluadorDeCatalogo().EvaluateAsync([], new ChatResponse(new ChatMessage(ChatRole.Assistant, json))))
        .Get<BooleanMetric>(EvaluadorDeCatalogo.Metrica);

    [Test]
    public async Task Catalogo_ClavesDelCatalogo_Pasa()
    {
        var m = await Catalogo("""{"medios":["colectivo","bicicleta"],"frecuencia":"diaria","motivo":"trabajo"}""");
        Assert.That((m.Value, m.Interpretation!.Failed), Is.EqualTo((true, false)));
    }

    [Test]
    public async Task Catalogo_MedioInventado_FallaConLaClave()
    {
        var m = await Catalogo("""{"medios":["colectivo","avion"],"frecuencia":"diaria"}""");
        Assert.That(m.Value, Is.False);
        Assert.That(m.Reason, Does.Contain("medios:avion"));
        Assert.That(m.Interpretation!.Failed, Is.True);
    }

    [Test]
    public async Task Catalogo_ListaVacia_FallaNuncaPasa()
    {
        var m = await Catalogo("""{"medios":[]}""");
        Assert.That(m.Value, Is.False);
    }

    [Test]
    public async Task Catalogo_Malformado_EsErrorDeDiagnostico()
    {
        var m = await Catalogo("Voy en bondi casi todos los días.");
        Assert.That(m.Value, Is.Null);
        Assert.That(m.Diagnostics!.Any(d => d.Severity == EvaluationDiagnosticSeverity.Error), Is.True);
        Assert.That(m.Interpretation!.Failed, Is.True);
    }

    // ------------------------------------------------------------ contrato con el juez

    [TestCase("<S0>a</S0><S1>b</S1><S2>4</S2>", 4)]
    [TestCase("<S2> 5 \n</S2>", 5)]
    [TestCase("Score: 2", null)]
    [TestCase("<S2>cinco</S2>", null)]
    [TestCase("<S2>3", null)]
    public void Lectura_EnteroEntreEtiquetasS2(string salida, int? esperado) =>
        Assert.That(LecturaDelJuez.Puntaje(salida), Is.EqualTo(esperado));

    [TestCase("<S0>a</S0><S1>b</S1><S2>4</S2>")]
    [TestCase("<S0>a</S0><S1>b</S1><S2>1</S2>")]
    [TestCase("Score: 2")]
    [TestCase("<S2>cinco</S2>")]
    public async Task Lectura_CoincideConLaDeMeai(string salida)
    {
        var juez = FabricaDeClientes.Envolver(new GuionChatClient(salida), Config, Adaptador.OllamaSharp);
        var r = await new RelevanceEvaluator().EvaluateAsync(
            [new ChatMessage(ChatRole.User, "¿Cuántos números lleva el código postal de una localidad?")],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "Lleva 4 dígitos.")),
            new ChatConfiguration(juez));
        var m = r.Get<NumericMetric>(NombresDeMetricas.Relevance);
        Assert.That(m.Value is double v ? (int?)(int)v : null, Is.EqualTo(LecturaDelJuez.Puntaje(salida)));
        if (m.Value is null) Assert.That(m.Diagnostics!.Any(d => d.Severity == EvaluationDiagnosticSeverity.Error), Is.True, "sin valor y sin diagnóstico");
    }

    [Test]
    public async Task Juez_TodaLlamada_SalePorLaCapaUnica()
    {
        // El espía va DEBAJO de la capa, pegado al «modelo»: ve lo que de verdad saldría al servidor.
        var guion = new GuionChatClient(GuionDelJuez.Salida(5, "a"), GuionDelJuez.Salida(4, "b"));
        var espia = new ClienteEspia(guion);
        var configuracion = new ChatConfiguration(FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp));
        var caso = Casos.Single(c => c.EntradaId == "c01");
        IEnumerable<ChatMessage> pregunta = [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)];
        var respuesta = new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta));

        await new RelevanceEvaluator().EvaluateAsync(pregunta, respuesta, configuracion);
        await new GroundednessEvaluator().EvaluateAsync(pregunta, respuesta, configuracion, [new GroundednessEvaluatorContext(caso.Contexto)]);

        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(2));
        Assert.That(espia.OpcionesVistas.All(o => o?.Reasoning?.Effort == ReasoningEffort.None), Is.True, "razonamiento");
        Assert.That(espia.OpcionesVistas.All(o => OpcionesDelProveedor.NumCtxDe(o) == ConfiguracionDelModelo.NumCtxPorDefecto), Is.True, "num_ctx");
        Assert.That(espia.OpcionesVistas.All(o => o?.Temperature == 0f), Is.True, "temperatura");
        // Lo que se juzga llega al juez: la respuesta, y en Groundedness también el contexto de 07.
        Assert.That(guion.Llamadas[1].Mensajes.Last().Text, Does.Contain(caso.Respuesta).And.Contain("Localidad · requisito del código postal"));
    }

    // ------------------------------------------------------------ corrida y reporte

    private (CorridaDeEvaluacion Corrida, GrabadorDeLlamadas Juez, EscritorDeRegistro Registro, GuionChatClient Guion) Armar(IEnumerable<CasoDeEvaluacion> casos, string almacen)
    {
        var guion = GuionDelJuez.Crear(casos);
        var juez = new GrabadorDeLlamadas(FabricaDeClientes.Envolver(guion, Config, Adaptador.OllamaSharp));
        var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "15-Pruebas-Y-Evaluacion", Configuracion = Config, Adaptador = Adaptador.OllamaSharp, Modelo = "guion", Maquina = "guion" });
        return (new CorridaDeEvaluacion(juez, registro, _ => guion.Llamadas[^1].Opciones, "15-Pruebas-Y-Evaluacion", almacen, "prueba", Calibracion), juez, registro, guion);
    }

    [Test]
    public async Task Corrida_ModoFalso_DaLosAcuerdosDelGuion()
    {
        var (corrida, _, registro, _) = Armar(Casos, Path.Combine(_dir, "reporte"));
        using (registro)
        {
            var r = await corrida.CorrerAsync(Casos, new Plazo(null));

            Assert.That(r.InvariantesQueFallan, Is.Empty, string.Join("\n", r.InvariantesQueFallan));
            Assert.That(r.LlamadasAlJuez, Is.EqualTo(52));
            Assert.That(r.Casos.Count(c => c.Caso.Grupo == GrupoDeEntrada.Salidas07 && c.Veredicto.Resultado == "pasa"), Is.EqualTo(GuionDelJuez.AcuerdoConOraculoEsperado));
            Assert.That(r.Casos.Count(c => c.Caso.Grupo == GrupoDeEntrada.Calibracion && c.Veredicto.Resultado == "pasa"), Is.EqualTo(GuionDelJuez.AcuerdoConAnotadorEsperado));
            var c10 = r.Casos.Single(c => c.Caso.EntradaId == "c10");
            Assert.That((c10.Veredicto.Resultado, c10.Veredicto.Motivo), Is.EqualTo(("falla", "salida del juez no interpretable (Groundedness)")));
        }
        var lineas = File.ReadAllLines(registro.Ruta);
        Assert.That(lineas.Count(l => l.Contains("\"tipo\":\"llamada\"")), Is.EqualTo(52));
        Assert.That(lineas.Count(l => l.Contains("\"tipo\":\"entrada\"")), Is.EqualTo(26));
        Assert.That(lineas.Where(l => l.Contains("\"entrada_id\":\"c0")).All(l => !l.Contains("\"tipo\":\"llamada\"") || l.Contains("anotador=anotado por el agente constructor")), Is.True);
    }

    [Test]
    public async Task Reporte_SegundaCorrida_SaleDeLaCache_SinLlamarAlJuez()
    {
        var dos = Casos.Where(c => c.EntradaId is "s07-r03" or "c01").ToList();
        var almacen = Path.Combine(_dir, "reporte");

        var (primera, juez1, reg1, _) = Armar(dos, almacen);
        using (reg1) await primera.CorrerAsync(dos, new Plazo(null));
        Assert.That(juez1.Total, Is.EqualTo(4));

        var (segunda, juez2, reg2, _) = Armar(dos, almacen);
        ResultadoDeLaEvaluacion r;
        using (reg2) r = await segunda.CorrerAsync(dos, new Plazo(null));
        Assert.That(juez2.Total, Is.Zero, "con el mismo reporte, el juez no se vuelve a llamar");
        Assert.That(r.Casos.All(c => c.Veredicto.Resultado == "excluida" && c.Veredicto.Motivo!.StartsWith("caché del reporte")), Is.True);
    }

    [Test]
    public async Task Corrida_PlazoVencido_ExcluyeSinLlamar()
    {
        var (corrida, juez, registro, _) = Armar(Casos.Take(3), Path.Combine(_dir, "reporte"));
        ResultadoDeLaEvaluacion r;
        using (registro) r = await corrida.CorrerAsync(Casos.Take(3).ToList(), new Plazo(DateTimeOffset.UtcNow.AddSeconds(-1)));
        Assert.That(juez.Total, Is.Zero);
        Assert.That(r.Casos.All(c => c.Veredicto is { Resultado: "excluida", Motivo: "plazo" }), Is.True);
    }

    // ------------------------------------------------------------ oráculo

    [Test]
    public void Oraculo_Calibracion_PorEvaluador()
    {
        IReadOnlyList<LecturaDeEvaluador> bien = [new("Relevance", 5, "alta"), new("Groundedness", 2, "baja")];
        IReadOnlyList<LecturaDeEvaluador> mal = [new("Relevance", 5, "alta"), new("Groundedness", 4, "baja")];
        Assert.That(OraculoDelJuez.Evaluar(GrupoDeEntrada.Calibracion, bien, 4).Resultado, Is.EqualTo("pasa"));
        Assert.That(OraculoDelJuez.Evaluar(GrupoDeEntrada.Calibracion, mal, 4).Resultado, Is.EqualTo("falla"));
    }

    [Test]
    public void Oraculo_Salidas07_AceptableContraElDeterminista()
    {
        IReadOnlyList<LecturaDeEvaluador> altas = [new("Relevance", 4, "pasa"), new("Groundedness", 5, "pasa")];
        IReadOnlyList<LecturaDeEvaluador> altasPeroFalla = [new("Relevance", 4, "falla"), new("Groundedness", 5, "falla")];
        Assert.That(OraculoDelJuez.Evaluar(GrupoDeEntrada.Salidas07, altas, 4).Resultado, Is.EqualTo("pasa"));
        Assert.That(OraculoDelJuez.Evaluar(GrupoDeEntrada.Salidas07, altasPeroFalla, 4).Resultado, Is.EqualTo("falla"));
        Assert.That(OraculoDelJuez.Evaluar(GrupoDeEntrada.Salidas07, [new("Relevance", null, "pasa"), new("Groundedness", 5, "pasa")], 4).Motivo,
            Does.StartWith("salida del juez no interpretable"));
        Assert.That(OraculoDelJuez.Evaluar(GrupoDeEntrada.Salidas07, [new("Relevance", 4, "pasa")], 4).Resultado, Is.EqualTo("excluida"));
    }

    // ------------------------------------------------------------ recuperación 2026-10-28 (REC-18)

    [Test]
    public void InterpretacionDeMeai_SinInterpretacion_InconclusiveOConError_NoEsInterpretable()
    {
        NumericMetric Con(double? valor, EvaluationRating? rating, bool error = false)
        {
            var m = new NumericMetric("Relevance", valor);
            if (rating is { } r) m.Interpretation = new EvaluationMetricInterpretation(r, failed: r is not EvaluationRating.Good);
            if (error) m.AddDiagnostics(EvaluationDiagnostic.Error("Failed to parse score"));
            return m;
        }
        Assert.That(InterpretacionDeMeai.Interpretable(Con(4, EvaluationRating.Good)), Is.True);
        Assert.That(InterpretacionDeMeai.Interpretable(Con(4, null), out var sinInterpretacion), Is.False);
        Assert.That(sinInterpretacion, Is.EqualTo("sin Interpretation"));
        Assert.That(InterpretacionDeMeai.Interpretable(Con(4, EvaluationRating.Inconclusive)), Is.False);
        Assert.That(InterpretacionDeMeai.Interpretable(Con(4, EvaluationRating.Good, error: true), out var conError), Is.False);
        Assert.That(conError, Is.EqualTo("diagnóstico Error"));
        Assert.That(InterpretacionDeMeai.Interpretable(Con(null, EvaluationRating.Inconclusive)), Is.False);
    }

    [TestCase("<S0>a</S0><S1>b</S1><S2>4</S2>", true)]
    [TestCase("Score: 2", false)]
    public async Task InterpretacionDeMeai_CoincideConLaLecturaDelOraculo(string salida, bool interpretable)
    {
        var juez = FabricaDeClientes.Envolver(new GuionChatClient(salida), Config, Adaptador.OllamaSharp);
        var r = await new RelevanceEvaluator().EvaluateAsync(
            [new ChatMessage(ChatRole.User, "¿Cuántos números lleva el código postal de una localidad?")],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "Lleva 4 dígitos.")),
            new ChatConfiguration(juez));
        var m = r.Get<NumericMetric>(NombresDeMetricas.Relevance);
        Assert.That(InterpretacionDeMeai.Interpretable(m), Is.EqualTo(interpretable));
        Assert.That(LecturaDelJuez.Puntaje(salida) is not null, Is.EqualTo(interpretable));
    }

    [Test]
    public async Task Corrida_JuezSinEtiquetasEnTodo_NoPasaEnVerde()
    {
        // Lo que vio el insumo: una prueba que pasaba con todas las métricas Inconclusive. Acá, un juez
        // que nunca escribe <S2> deja todo «no interpretable»: ningún veredicto «pasa» y el juez queda
        // descartado por la regla de calibración. El cableado no se confunde con una medición.
        var guion = new GuionChatClient([], "Score: 2");
        var juez = new GrabadorDeLlamadas(FabricaDeClientes.Envolver(guion, Config, Adaptador.OllamaSharp));
        using var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "15-Pruebas-Y-Evaluacion", Configuracion = Config, Adaptador = Adaptador.OllamaSharp, Modelo = "guion", Maquina = "guion" });
        var corrida = new CorridaDeEvaluacion(juez, registro, _ => guion.Llamadas[^1].Opciones, "15-Pruebas-Y-Evaluacion", Path.Combine(_dir, "reporte"), "prueba", Calibracion);

        var r = await corrida.CorrerAsync(Casos, new Plazo(null));

        Assert.That(r.LlamadasAlJuez, Is.EqualTo(52));
        Assert.That(r.Casos.SelectMany(c => c.Lecturas).Any(l => l.Interpretable), Is.False);
        Assert.That(r.Casos.Any(c => c.Veredicto.Resultado == "pasa"), Is.False);
        Assert.That(r.Casos.All(c => c.Veredicto.Motivo!.StartsWith("salida del juez no interpretable")), Is.True);
        Assert.That(CorridaDeEvaluacion.JuezNoDescartado(r, Calibracion), Is.False);
        // Lectura del oráculo y de M.E.AI coinciden en «sin valor»: no hay invariante roto, hay un juez descartado.
        Assert.That(r.InvariantesQueFallan, Is.Empty, string.Join("\n", r.InvariantesQueFallan));
    }

    [Test]
    public void Equivalencia_ReferenciaDelOro_SonLosFragmentosRelevantes_SoloEnRespondibles()
    {
        var r03 = Oro.Preguntas.Single(p => p.Id == "r03");
        var fragmentos = CorpusDePolicies.Fragmentos().ToDictionary(f => f.Id);
        Assert.That(ReferenciaDelOro.Para(r03), Is.EqualTo(string.Join("\n", r03.Relevantes.Select(id => fragmentos[id].Texto))));
        Assert.That(Oro.Preguntas.Where(p => !p.Respondible).All(p => ReferenciaDelOro.Para(p) is null), Is.True);
        Assert.That(CorridaDeEquivalencia.Elegibles(Casos).Count(), Is.EqualTo(Oro.Preguntas.Count(p => p.Respondible && p.Relevantes.Count > 0)));
        Assert.That(CorridaDeEquivalencia.Elegibles(Casos).All(c => c.Grupo == GrupoDeEntrada.Salidas07), Is.True);
    }

    [Test]
    public async Task Equivalencia_ModoFalso_SalePorLaCapaUnica_ConLaReferenciaDelOro()
    {
        var elegibles = CorridaDeEquivalencia.Elegibles(Casos).ToList();
        var guion = new GuionChatClient(elegibles.Select(c => PasoDelGuion.Texto(GuionDelJuez.PuntajeDeEquivalencia(c).ToString())), "Score: 2");
        var espia = new ClienteEspia(guion);
        var juez = new GrabadorDeLlamadas(FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp));
        var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "15-Pruebas-Y-Evaluacion", Configuracion = Config, Adaptador = Adaptador.OllamaSharp, Modelo = "guion", Maquina = "guion" });
        ResultadoDeEquivalencia r;
        using (registro)
            r = await new CorridaDeEquivalencia(juez, registro, _ => guion.Llamadas[^1].Opciones, Path.Combine(_dir, "reporte"), "prueba", Calibracion.Umbral)
                .CorrerAsync(Casos, new Plazo(null));

        Assert.That(r.InvariantesQueFallan, Is.Empty, string.Join("\n", r.InvariantesQueFallan));
        Assert.That(r.LlamadasAlJuez, Is.EqualTo(elegibles.Count));
        Assert.That(r.Lecturas.All(l => l.Interpretable), Is.True);
        Assert.That(r.Lecturas.Count(l => l.Puntaje >= Calibracion.Umbral), Is.EqualTo(elegibles.Count(c => c.Resultado07 == "pasa")));
        // La capa única rige también para Equivalence: razonamiento apagado, num_ctx ≥ 8192, temperatura 0.
        Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(elegibles.Count));
        Assert.That(espia.OpcionesVistas.All(o => o?.Reasoning?.Effort == ReasoningEffort.None), Is.True, "razonamiento");
        Assert.That(espia.OpcionesVistas.All(o => OpcionesDelProveedor.NumCtxDe(o) >= 8192), Is.True, "num_ctx");
        Assert.That(espia.OpcionesVistas.All(o => o?.Temperature == 0f), Is.True, "temperatura");
        // El juez recibe la referencia del oro.
        Assert.That(guion.Llamadas[0].Mensajes.Last().Text, Does.Contain(ReferenciaDelOro.Para(elegibles[0].Pregunta)!.Split('\n')[0]));
        // En el registro van con su propio prefijo y sin línea de entrada: el oráculo jq no las mezcla con s07-.
        var lineas = File.ReadAllLines(registro.Ruta);
        Assert.That(lineas.Count(l => l.Contains("\"entrada_id\":\"eq-s07-")), Is.EqualTo(elegibles.Count));
        Assert.That(lineas.Any(l => l.Contains("\"tipo\":\"entrada\"")), Is.False);
    }

    [TestCase("5", 5)]
    [TestCase(" 4 \n", 4)]
    [TestCase("<S0>a</S0><S1>b</S1><S2>5</S2>", null)]
    [TestCase("stars: 5", null)]
    [TestCase("Score: 2", null)]
    public async Task Equivalencia_LecturaEntera_CoincideConLaDeMeai(string salida, int? esperado)
    {
        // Equivalence no usa las etiquetas de Relevance y Groundedness: un juez que contesta con <S2>
        // queda sin valor. La lectura propia tiene que decir lo mismo que M.E.AI 10.11.0.
        var caso = CorridaDeEquivalencia.Elegibles(Casos).First();
        var juez = FabricaDeClientes.Envolver(new GuionChatClient(salida), Config, Adaptador.OllamaSharp);
        var r = await new EquivalenceEvaluator().EvaluateAsync(
            [new ChatMessage(ChatRole.User, caso.Pregunta.Texto)],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, caso.Respuesta)),
            new ChatConfiguration(juez), [new EquivalenceEvaluatorContext(ReferenciaDelOro.Para(caso.Pregunta)!)]);
        var m = r.Get<NumericMetric>(NombresDeMetricas.Equivalence);
        Assert.That(LecturaEntera.Puntaje(salida), Is.EqualTo(esperado));
        Assert.That(m.Value is double v ? (int?)(int)v : null, Is.EqualTo(esperado));
        Assert.That(InterpretacionDeMeai.Interpretable(m), Is.EqualTo(esperado is not null));
    }

    [Test]
    public async Task Equivalencia_SalidaQueNoEsUnEntero_NoEsInterpretable()
    {
        var caso = CorridaDeEquivalencia.Elegibles(Casos).First();
        var guion = new GuionChatClient("<S0>a</S0><S1>b</S1><S2>5</S2>");
        var juez = new GrabadorDeLlamadas(FabricaDeClientes.Envolver(guion, Config, Adaptador.OllamaSharp));
        ResultadoDeEquivalencia r;
        using (var registro = new EscritorDeRegistro(new ContextoDeCorrida { Ejemplo = "15-Pruebas-Y-Evaluacion", Configuracion = Config, Adaptador = Adaptador.OllamaSharp, Modelo = "guion", Maquina = "guion" }))
            r = await new CorridaDeEquivalencia(juez, registro, _ => guion.Llamadas[^1].Opciones, Path.Combine(_dir, "reporte"), "prueba", Calibracion.Umbral)
                .CorrerAsync([caso], new Plazo(null));
        Assert.That(r.Lecturas.Single(), Is.EqualTo(new LecturaDeEquivalencia("eq-" + caso.EntradaId, null, false, null)));
        Assert.That(r.InvariantesQueFallan, Is.Empty);
    }
}
