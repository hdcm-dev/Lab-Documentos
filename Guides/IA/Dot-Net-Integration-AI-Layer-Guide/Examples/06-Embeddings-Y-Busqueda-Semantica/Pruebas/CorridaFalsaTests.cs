// Nuevo: la corrida completa con el generador estable (modo --fake), sin red. Comprueba cableado,
// registro y métricas; no dice nada de la calidad de la recuperación.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CorridaFalsaTests
{
    private string _dir = null!;

    [SetUp]
    public void Preparar() => _dir = Directory.CreateTempSubdirectory("ej06-").FullName;

    [TearDown]
    public void Limpiar() => Directory.Delete(_dir, recursive: true);

    private async Task<(ResultadoDeLaCorrida Resultado, List<JsonElement> Renglones)> Correr(
        IEmbeddingGenerator<string, Embedding<float>> generador, Plazo? plazo = null)
    {
        var config = new ConfiguracionDelModelo { RegistroDir = _dir };
        var ctx = new ContextoDeCorrida
        {
            Ejemplo = "06-Embeddings-Y-Busqueda-Semantica", Configuracion = config, Adaptador = Adaptador.OllamaSharp,
            Modelo = "fnv1a-estable", EntradasVersion = "v1", EntradasHash = Hashes.Sha256DeArchivo(ConjuntoDeOro.RutaPorDefecto), Maquina = "guion"
        };
        ResultadoDeLaCorrida r;
        using (var registro = new EscritorDeRegistro(ctx))
        {
            r = await new CorridaDeBusqueda(generador, config, ctx, ConjuntoDeOro.RutaPorDefecto, plazo ?? new Plazo(null), sinModelo: true)
                .CorrerAsync(registro, TextWriter.Null);
        }
        var lineas = File.ReadAllLines(Path.Combine(_dir, "06-Embeddings-Y-Busqueda-Semantica.jsonl"))
            .Select(l => JsonDocument.Parse(l).RootElement.Clone()).ToList();
        return (r, lineas);
    }

    [Test]
    public async Task Fake_44LlamadasY32EntradasConInvariantes()
    {
        var (r, lineas) = await Correr(new GeneradorDeEmbeddingsEstable(1024));
        Assert.That(r.InvariantesQueFallan, Is.Empty);
        Assert.That(r.Llamadas, Is.EqualTo(44), "12 fragmentos + 16 preguntas × 2 variantes (plan §E.5)");
        var llamadas = lineas.Where(l => l.GetProperty("tipo").GetString() == "llamada").ToList();
        var entradas = lineas.Where(l => l.GetProperty("tipo").GetString() == "entrada").ToList();
        Assert.That(llamadas, Has.Count.EqualTo(44));
        Assert.That(entradas, Has.Count.EqualTo(32));
        Assert.That(entradas.Select(e => e.GetProperty("oraculo").GetString()).Distinct(), Is.EqualTo(new[] { CorridaDeBusqueda.Oraculo }));
        // Cada consulta lleva el orden completo y los no respondibles quedan fuera del hit rate.
        Assert.That(llamadas.Where(l => l.GetProperty("paso").GetString()!.StartsWith("consulta:")).All(l => l.GetProperty("recuperados").GetArrayLength() == 12));
        Assert.That(entradas.Count(e => e.GetProperty("resultado").GetString() == ResultadoDeEntrada.Excluida), Is.EqualTo(8));
        // EMB-C / --fake: el rótulo va en el registro.
        Assert.That(llamadas.All(l => l.GetProperty("salida").GetString()!.Contains(CorridaDeBusqueda.RotuloSinModelo)));
        Assert.That(llamadas.All(l => l.GetProperty("adaptador").GetString() == "generador-estable"));
    }

    [Test]
    public async Task Fake_EsDeterminista()
    {
        var (a, _) = await Correr(new GeneradorDeEmbeddingsEstable(1024));
        var (b, _) = await Correr(new GeneradorDeEmbeddingsEstable(1024));
        Assert.That(a.Consultas.Select(c => c.Orden![0].Id), Is.EqualTo(b.Consultas.Select(c => c.Orden![0].Id)));
    }

    [Test]
    public async Task Plazo_Vencido_ExcluyeLasPreguntasConMotivo()
    {
        var (r, lineas) = await Correr(new GeneradorDeEmbeddingsEstable(1024), new Plazo(DateTimeOffset.MinValue));
        Assert.That(r.Llamadas, Is.EqualTo(12));
        var entradas = lineas.Where(l => l.GetProperty("tipo").GetString() == "entrada").ToList();
        Assert.That(entradas.All(e => e.GetProperty("motivo").GetString() == "plazo"));
    }

    [Test]
    public async Task DimensionDistintaDeLaDelAtributo_FallaElInvariante()
    {
        var (r, _) = await Correr(new GeneradorDeEmbeddingsEstable(512));
        Assert.That(r.InvariantesCumplidos, Is.False);
        Assert.That(r.Llamadas, Is.EqualTo(1), "se corta en la primera llamada, sin seguir gastando");
    }

    [Test]
    public void Veredicto_RespondibleEnTop3Pasa_NoRespondibleExcluida()
    {
        var oro = ConjuntoDeOro.Leer();
        var r03 = oro.Preguntas.Single(p => p.Id == "r03");
        var orden = new[] { "x", "y", "LocalidadPolicy.RequisitoDelCodigoPostal", "z" }.Select((id, i) => new Posicion(id, 4 - i)).ToList();
        Assert.That(CorridaDeBusqueda.Veredicto(r03, orden, false).Resultado, Is.EqualTo(ResultadoDeEntrada.Pasa));
        Assert.That(CorridaDeBusqueda.Veredicto(r03, orden.Skip(1).Prepend(new Posicion("w", 9)).Prepend(new Posicion("v", 10)).ToList(), false).Resultado,
            Is.EqualTo(ResultadoDeEntrada.Falla));
        Assert.That(CorridaDeBusqueda.Veredicto(r03, orden, true).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
        Assert.That(CorridaDeBusqueda.Veredicto(oro.Preguntas.Single(p => p.Id == "n01"), orden, false).Resultado, Is.EqualTo(ResultadoDeEntrada.Excluida));
    }

    [Test]
    public void Instruccion_SoloEnLaConsulta()
    {
        Assert.That(CorridaDeBusqueda.TextoDeConsulta("¿Y?", Variante.SinInstruccion), Is.EqualTo("¿Y?"));
        Assert.That(CorridaDeBusqueda.TextoDeConsulta("¿Y?", Variante.ConInstruccion), Does.StartWith("Instruct: ").And.EndWith("\nQuery:¿Y?"));
    }
}
