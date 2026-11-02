// Nuevo: pruebas de cableado del ejemplo 12, sin red. La de [Category("LLM")] está en ConModeloTests.cs.
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Resumenes;
using MovilidadUrbana.Asistente.Application.Workflows;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente.Pruebas;

public class UnoOVariosAgentesTests
{
    private static ConfiguracionDelModelo Config() => new() { RegistroDir = Directory.CreateTempSubdirectory("ej12-reg-").FullName };

    private static readonly SolicitudDeResumen E01 = new("e01", "¿Qué porcentaje usa colectivo y bicicleta?", ["colectivo", "bicicleta"]);

    private sealed record Montaje(BaseDeEjemplo Base, FuenteDeResultados Fuente, ClienteEspia Espia, List<LlamadaGrabada> Grabadas, IChatClient Capa) : IDisposable
    {
        public void Dispose() => Base.Dispose();
    }

    private static Montaje Montar(params string[] respuestas)
    {
        var bd = BaseDeEjemplo.Crear();
        var espia = new ClienteEspia(new GuionChatClient(respuestas));
        return new Montaje(bd, new FuenteDeResultados(bd.Resultados), espia, [], FabricaDeClientes.Envolver(espia, Config(), Adaptador.OllamaSharp));
    }

    private static Workflow Armar(Montaje m, Variante v) => CaminoDelResumen.Construir(v, m.Fuente,
        new GrabadorDeLlamadas(m.Capa, "redactor", m.Grabadas),
        v == Variante.DosAgentes ? new GrabadorDeLlamadas(m.Capa, "corrector", m.Grabadas) : null);

    // ---------------------------------------------------------------- la siembra y la cuenta

    [Test]
    public async Task Siembra_DaLosPorcentajesQueUsaElGuion()
    {
        using var bd = BaseDeEjemplo.Crear();
        var r = await bd.Resultados.ResumirAsync();
        var p = r.Medios.ToDictionary(m => m.Clave, m => m.Porcentaje);
        Assert.That(r.Total, Is.EqualTo(20));
        Assert.That(p, Is.EquivalentTo(new Dictionary<string, double>
        {
            ["colectivo"] = 20, ["auto"] = 20, ["bicicleta"] = 35, ["moto"] = 15, ["caminata"] = 15, ["tren"] = 30
        }));
    }

    [TestCase("El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta.", true)]
    [TestCase("Colectivo: 20 %, bicicleta: 35 %.", true)]
    [TestCase("Colectivo (20,6 %) y Bicicleta (34,5 por ciento).", true)]
    [TestCase("El 20 % usa colectivo, el 35 % usa bicicleta.", true)]
    [TestCase("Colectivo y bicicleta: 20 % y 35 %.", false)]                  // límite declarado: sin cláusula propia
    [TestCase("El 35 % usa colectivo y el 20 % usa bicicleta.", false)]       // invertidos
    [TestCase("Colectivo: 21,5 %, bicicleta: 35 %.", false)]                  // fuera de ±1
    [TestCase("El 20 % usa colectivo.", false)]                               // falta bicicleta
    public void Comparador_AsociaCadaMedioConSuPorcentajeMasCercano(string texto, bool correcto)
    {
        var v = ComparadorDePorcentajes.Verificar(texto, [("colectivo", 20.0), ("bicicleta", 35.0)]);
        Assert.That(v.Correcto, Is.EqualTo(correcto), string.Join("; ", v.Errores));
    }

    [Test]
    public void Comparador_ElSegundoMedioSinPorcentajePropio_TomaElDeSuOracion()
    {
        // «… y el auto particular, el 20 %»: la cláusula corta («el auto particular») no tiene cifra; la larga sí.
        var v = ComparadorDePorcentajes.Verificar(GuionDelEjemplo.E02, [("tren", 30.0), ("auto", 20.0)]);
        Assert.That(v.Correcto, Is.True, string.Join("; ", v.Errores));
    }

    [Test]
    public void Comparador_SinPorcentajes_EsSinCifra()
    {
        var v = ComparadorDePorcentajes.Verificar("Mucha gente usa colectivo y bicicleta.", [("colectivo", 20.0), ("bicicleta", 35.0)]);
        Assert.That(v.SinCifra, Is.True);
        Assert.That(v.Errores.Single(), Does.StartWith("sin cifra"));
    }

    [Test]
    public void Comparador_ReconoceLaEtiquetaDelCatalogo()
    {
        Assert.That(ComparadorDePorcentajes.Terminos("caminata"), Is.EquivalentTo(new[] { "caminata", "a pie" }));
        Assert.That(ComparadorDePorcentajes.Verificar("Va a pie el 15 %.", [("caminata", 15.0)]).Correcto, Is.True);
        Assert.That(ComparadorDePorcentajes.Verificar("El auto particular: 20 %.", [("auto", 20.0)]).Correcto, Is.True);
    }

    // ---------------------------------------------------------------- el camino fijo

    [Test]
    public async Task UnAgente_ConError_DaUnaVueltaAlMismoRedactor()
    {
        using var m = Montar(GuionDelEjemplo.E01Equivocado, GuionDelEjemplo.E01Corregido);
        var c = await ConductorDelCamino.CorrerAsync(Armar(m, Variante.UnAgente), E01);

        Assert.That(c.Camino, Is.EqualTo(new[] { "Datos", "Redactor", "Verificador", "Redactor", "Verificador" }));
        Assert.That(c.Salida!.Correcto, Is.True);
        Assert.That(c.Salida.Vueltas, Is.EqualTo(1));
        Assert.That(m.Grabadas.Select(g => g.Nodo), Is.EqualTo(new[] { "redactor", "redactor" }));
        // El error que vuelve al modelo es el de la cuenta, con el valor del servicio.
        Assert.That(m.Grabadas[1].Mensajes[^1].Text, Does.Contain("colectivo: dice 25 %, el servicio da 20 %"));
    }

    [Test]
    public async Task DosAgentes_ConError_LaVueltaVaAlCorrectorConElMismoMensaje()
    {
        using var m = Montar(GuionDelEjemplo.E01Equivocado, GuionDelEjemplo.E01Corregido);
        var c = await ConductorDelCamino.CorrerAsync(Armar(m, Variante.DosAgentes), E01);

        Assert.That(c.Camino, Is.EqualTo(new[] { "Datos", "Redactor", "Verificador", "Corrector", "Verificador" }));
        Assert.That(c.Salida!.Correcto, Is.True);
        Assert.That(c.Salida.Autor, Is.EqualTo("Corrector"));
        Assert.That(m.Grabadas.Select(g => g.Nodo), Is.EqualTo(new[] { "redactor", "corrector" }));
        Assert.That(m.Grabadas[1].Mensajes.Last(x => x.Role == ChatRole.User).Text,
            Is.EqualTo(Indicaciones.Correccion(new PedidoDeCorreccion(
                new PedidoDeResumen(E01, FuenteDeResultados.Contexto(await m.Fuente.ResumirEncuestaAsync(), E01.MediosPedidos)),
                GuionDelEjemplo.E01Equivocado, ["colectivo: dice 25 %, el servicio da 20 %"], 1))));
    }

    [TestCase(Variante.UnAgente)]
    [TestCase(Variante.DosAgentes)]
    public async Task SinError_NoHayVuelta(Variante v)
    {
        using var m = Montar(GuionDelEjemplo.E01Corregido);
        var c = await ConductorDelCamino.CorrerAsync(Armar(m, v), E01);
        Assert.That(c.Camino, Is.EqualTo(new[] { "Datos", "Redactor", "Verificador" }));
        Assert.That(m.Grabadas, Has.Count.EqualTo(1));
    }

    [TestCase(Variante.UnAgente)]
    [TestCase(Variante.DosAgentes)]
    public async Task SigueMalDespuesDeLaVuelta_SeCortaYLoDice(Variante v)
    {
        using var m = Montar(GuionDelEjemplo.E01Equivocado, "Mucha gente usa colectivo.", "nunca se pide");
        var c = await ConductorDelCamino.CorrerAsync(Armar(m, v), E01);

        Assert.That(c.Camino, Has.Count.EqualTo(5));
        Assert.That(m.Grabadas, Has.Count.EqualTo(2), "una sola vuelta: el workflow no entra en un bucle abierto");
        Assert.That(c.Salida!.Correcto, Is.False);
        Assert.That(c.Salida.Errores.Single(), Does.StartWith("sin cifra"));
    }

    [Test]
    public void UnAgente_NoNecesitaCorrector_DosAgentesSi()
    {
        using var m = Montar();
        Assert.DoesNotThrow(() => CaminoDelResumen.Construir(Variante.UnAgente, m.Fuente, m.Capa));
        Assert.Throws<ArgumentNullException>(() => CaminoDelResumen.Construir(Variante.DosAgentes, m.Fuente, m.Capa));
    }

    // ---------------------------------------------------------------- la capa única, también para MAF

    [Test]
    public async Task TodasLasLlamadas_IncluidasLasDelAgente_PasanPorLaCapaUnica()
    {
        using var m = Montar(GuionDelEjemplo.E01Equivocado, GuionDelEjemplo.E01Corregido);
        await ConductorDelCamino.CorrerAsync(Armar(m, Variante.DosAgentes), E01);

        Assert.That(m.Espia.OpcionesVistas, Has.Count.EqualTo(2));
        Assert.That(m.Espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o =>
            o!.Reasoning!.Effort == ReasoningEffort.None && OpcionesDelProveedor.NumCtxDe(o) == ConfiguracionDelModelo.NumCtxPorDefecto));
        // La segunda es la del AIAgent: llega con las instrucciones del corrector.
        var delAgente = m.Grabadas[1];
        var instrucciones = (delAgente.Opciones?.Instructions ?? string.Empty) +
                            string.Concat(delAgente.Mensajes.Where(x => x.Role == ChatRole.System).Select(x => x.Text));
        Assert.That(instrucciones, Does.Contain("Sos el corrector"));
    }

    // ---------------------------------------------------------------- privacidad y verificador

    [Test]
    public void ResumirEncuesta_NoTieneNombreNiEdad_PorReflexion()
    {
        Assert.That(Garantias.ResumirEncuestaSinDatosDePersona(), Is.Empty);
        // Control: la reflexión sí encuentra los campos en la entidad que los tiene.
        Assert.That(Garantias.CamposDePersonaEn(typeof(Domain.Entities.RespuestaDeEncuesta)),
            Is.EquivalentTo(new[] { "RespuestaDeEncuesta.Nombre", "RespuestaDeEncuesta.Edad" }));
    }

    [Test]
    public void Verificador_NoRecibeModelo() => Assert.That(Garantias.VerificadorSinModelo(), Is.Empty);

    // ---------------------------------------------------------------- la corrida --fake completa

    [Test]
    public async Task CorridaFalsa_CumpleLosInvariantesYMideElCosto()
    {
        var config = Config();
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar"]), config, TextWriter.Null);

        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Costos.Select(c => (c.Variante, c.Llamadas, c.Vueltas, c.Pasan, c.Validas)), Is.EqualTo(new[]
        {
            ("un-agente", 3, 1, 2, 2),
            ("dos-agentes", 3, 1, 2, 2)
        }));

        var lineas = File.ReadAllLines(r.RutaDelRegistro!).Select(l => JsonDocument.Parse(l).RootElement).ToList();
        var entradas = lineas.Where(l => l.GetProperty("tipo").GetString() == "entrada").ToList();
        Assert.That(entradas.Select(e => e.GetProperty("entrada_id").GetString()),
            Is.EqualTo(new[] { "e01.un-agente", "e01.dos-agentes", "e02.un-agente", "e02.dos-agentes" }));
        Assert.That(lineas.Count(l => l.GetProperty("tipo").GetString() == "llamada"), Is.EqualTo(6));
        Assert.That(lineas.Where(l => l.GetProperty("tipo").GetString() == "llamada"),
            Has.All.Matches<JsonElement>(l => l.GetProperty("contexto").GetString()!.Contains("\"medios_pedidos\"")));
    }

    [Test]
    public async Task Oraculo_ConPlazoVencido_ExcluyeTodo()
    {
        var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--plazo-s", "0"]), Config(), TextWriter.Null);
        Assert.That(r.Costos.Sum(c => c.Llamadas), Is.Zero);
        var resultados = File.ReadAllLines(r.RutaDelRegistro!).Select(l => JsonDocument.Parse(l).RootElement)
            .Where(l => l.GetProperty("tipo").GetString() == "entrada").Select(l => l.GetProperty("motivo").GetString());
        Assert.That(resultados, Has.All.EqualTo("plazo"));
    }

    [Test]
    public void Oraculo_TruncadoExcluye_YSinCifraFalla()
    {
        const string contexto = """{"funcion":"resumir_encuesta","total":20,"medios_pedidos":["tren"],"medios":[{"clave":"tren","etiqueta":"Tren","cantidad":6,"porcentaje":30}]}""";
        RegistroDeLlamada Renglon(string salida, bool? truncado) => new()
        {
            Ejemplo = Corrida.Ejemplo, EntradaId = "x", LlamadaN = 1, Salida = salida, Contexto = contexto, TruncadoPosible = truncado
        };
        Assert.That(OraculoDePorcentajes.Evaluar([Renglon("El tren: 30 %.", false)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Pasa));
        Assert.That(OraculoDePorcentajes.Evaluar([Renglon("El tren: 30 %.", true)]).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
        Assert.That(OraculoDePorcentajes.Evaluar([Renglon("Muchos van en tren.", false)]),
            Is.EqualTo(new VeredictoDelOraculo(ResultadoDeEntrada.Falla, "sin cifra")));
        Assert.That(OraculoDePorcentajes.Evaluar([]).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
    }

    // ---------------------------------------------------------------- estímulos

    [Test]
    public void Entradas_RotuladasConMediosDelCatalogoYCongeladas()
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
        var archivo = ArchivoDeEntradas.Leer(ruta);
        Assert.That(archivo.Rotulo, Does.StartWith("escrito para el ejemplo"));
        Assert.That(archivo.Entradas, Has.Count.EqualTo(2));
        Assert.That(archivo.Entradas.SelectMany(e => e.Medios), Has.All.Matches<string>(c => Catalogos.Medios.Any(m => m.Clave == c)));

        // La carpeta del ejemplo sale de la ruta de este archivo al compilar: vale también con los
        // intermedios en el volumen del arnés, porque el contenedor compila y prueba sobre el mismo árbol.
        var manifiesto = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(EsteArchivo()))!, "ejemplo.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(manifiesto));
        var sha = doc.RootElement.GetProperty("entradas")[0].GetProperty("sha256").GetString();
        Assert.That(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(ruta))), Is.EqualTo(sha));
    }

    private static string EsteArchivo([System.Runtime.CompilerServices.CallerFilePath] string ruta = "") => ruta;

    [Test]
    public void Compartido_SinNumCtxDelArnes_Rechaza() =>
        Assert.Throws<ConfiguracionInvalidaException>(() =>
            ConfiguracionDelModelo.DesdeEntorno(n => n == "OLLAMA_SERVIDOR" ? "compartido" : null).Validar());
}
