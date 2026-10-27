using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Gate.Tests.Infraestructura;
using Json.Schema;

namespace FrameworkConcept.Gate.Tests.Definiciones;

/// <summary>
/// P8 — Compuerta de las definiciones de encuesta (carpeta <c>definiciones/</c>):
///   1. cada definición valida contra el <c>esquema.json</c> de su carpeta y se lee con el contrato;
///   2. dentro del repositorio, las versiones de una misma encuesta evolucionan bien entre sí (P13.8);
///   3. frente a la base del PR: lo publicado no se borra, y lo que cambió sube su versión (P13.9).
/// El rojo que esta suite tiene que dar: «editar una pregunta sin subir version».
/// </summary>
[TestFixture]
public class DefinicionesTests
{
    private const string Carpeta = "definiciones";
    private const string NombreDelEsquema = "esquema.json";

    /// <summary>Un esquema por carpeta, cargado una sola vez.</summary>
    private static readonly ConcurrentDictionary<string, JsonSchema> Esquemas = new();

    /// <summary>Todas las definiciones del repositorio, como rutas relativas a la demo («definiciones/x/v1.json»).</summary>
    public static IEnumerable<string> Archivos() => ArchivosEn(Repositorio.Raiz);

    private static IEnumerable<string> ArchivosEn(string raiz)
    {
        var dir = Path.Combine(raiz, Carpeta);
        if (!Directory.Exists(dir)) yield break;

        foreach (var archivo in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            if (Path.GetFileName(archivo) == NombreDelEsquema) continue;
            yield return Path.GetRelativePath(raiz, archivo).Replace(Path.DirectorySeparatorChar, '/');
        }
    }

    [Test]
    [Description("Hay al menos una definición: una compuerta que no encuentra nada no prueba nada")]
    public void HayDefiniciones() =>
        Assert.That(Archivos().ToList(), Is.Not.Empty, $"No hay definiciones en {Repositorio.Ruta(Carpeta)}.");

    [TestCaseSource(nameof(Archivos))]
    [Description("P8 — La definición valida contra el esquema JSON de su carpeta")]
    public void ValidaContraSuEsquema(string relativa)
    {
        var ruta = Repositorio.Ruta(relativa);
        var rutaEsquema = Path.Combine(Path.GetDirectoryName(ruta)!, NombreDelEsquema);
        Assert.That(File.Exists(rutaEsquema), Is.True, $"{relativa}: falta {NombreDelEsquema} en su carpeta.");

        var esquema = Esquemas.GetOrAdd(rutaEsquema, r => JsonSchema.FromText(File.ReadAllText(r)));
        var resultado = esquema.Evaluate(JsonDocument.Parse(File.ReadAllText(ruta)).RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });

        Assert.That(resultado.IsValid, Is.True, $"{relativa} no valida contra {NombreDelEsquema}:\n  - " + string.Join("\n  - ", Errores(resultado)));
    }

    [TestCaseSource(nameof(Archivos))]
    [Description("P1 — La definición se lee con el contrato (incluye el rango compatibleConContenido con techo)")]
    public void SeLeeConElContrato(string relativa) =>
        Assert.That(() => DefinicionEncuesta.Leer(File.ReadAllText(Repositorio.Ruta(relativa))), Throws.Nothing, relativa);

    [Test]
    [Description("P13.8 — En el repositorio, cada encuesta tiene versiones únicas y cada una evoluciona bien desde la anterior")]
    public void LasVersionesDelRepositorioEvolucionanBien()
    {
        var problemas = new List<string>();

        foreach (var encuesta in Leer(Repositorio.Raiz).GroupBy(d => d.Definicion.EncuestaId))
        {
            var ordenadas = encuesta.OrderBy(d => d.Definicion.Version).ToList();

            foreach (var repetida in ordenadas.GroupBy(d => d.Definicion.Version).Where(g => g.Count() > 1))
                problemas.Add($"«{encuesta.Key}» tiene la versión {repetida.Key} en más de un archivo: {string.Join(", ", repetida.Select(d => d.Ruta))}");

            for (var i = 1; i < ordenadas.Count; i++)
            {
                foreach (var v in EvolucionDeDefiniciones.Violaciones(ordenadas[i - 1].Definicion, ordenadas[i].Definicion))
                    problemas.Add($"{ordenadas[i - 1].Ruta} → {ordenadas[i].Ruta}: {v}");
            }
        }

        Assert.That(problemas, Is.Empty, "Evolución de las definiciones:\n  - " + string.Join("\n  - ", problemas));
    }

    [Test]
    [Description("P13.9 — Frente a la base del PR: lo publicado no se borra; lo que cambió sube su versión y respeta la evolución")]
    public void FrenteALaBaseDelPr()
    {
        var baseDelPr = Repositorio.BaseObligatoria();
        var publicadas = Leer(baseDelPr);
        if (publicadas.Count == 0)
            Assert.Pass("La base del PR no tiene definiciones: no hay nada publicado con qué comparar (el PR que crea la demo).");

        var propuestas = Leer(Repositorio.Raiz).ToDictionary(d => d.Ruta, StringComparer.Ordinal);
        var problemas = new List<string>();

        foreach (var publicada in publicadas)
        {
            if (!propuestas.TryGetValue(publicada.Ruta, out var propuesta))
            {
                // Retirar una versión es un acto explícito, anunciado en el índice firmado (P13.5), y el
                // servidor la retiene (P13.4): borrar el archivo no es la manera.
                problemas.Add($"{publicada.Ruta}: la definición publicada se borró del repositorio (retirarla es un acto explícito, P13.5)");
                continue;
            }
            if (JsonNode.DeepEquals(publicada.Json, propuesta.Json)) continue;

            foreach (var v in EvolucionDeDefiniciones.Violaciones(publicada.Definicion, propuesta.Definicion))
                problemas.Add($"{publicada.Ruta} cambió respecto de la base: {v}");
        }

        // Un archivo nuevo de una encuesta ya publicada no puede llegar con una versión menor o igual a la
        // mayor publicada: el roll-forward nunca baja la versión (P13.9).
        var maximaPublicada = publicadas.GroupBy(d => d.Definicion.EncuestaId).ToDictionary(g => g.Key, g => g.Max(d => d.Definicion.Version));
        foreach (var nueva in propuestas.Values.Where(p => publicadas.All(b => b.Ruta != p.Ruta)))
        {
            if (maximaPublicada.TryGetValue(nueva.Definicion.EncuestaId, out var maxima) && nueva.Definicion.Version <= maxima)
                problemas.Add($"{nueva.Ruta}: versión {nueva.Definicion.Version} nueva, pero «{nueva.Definicion.EncuestaId}» ya publicó la {maxima} (P13.9)");
        }

        Assert.That(problemas, Is.Empty, "Definiciones frente a la base del PR:\n  - " + string.Join("\n  - ", problemas));
    }

    private sealed record DefinicionLeida(string Ruta, JsonNode? Json, DefinicionEncuesta Definicion);

    private static List<DefinicionLeida> Leer(string raiz) =>
        [.. ArchivosEn(raiz).Select(relativa =>
        {
            var texto = File.ReadAllText(Path.Combine(raiz, relativa));
            return new DefinicionLeida(relativa, JsonNode.Parse(texto), DefinicionEncuesta.Leer(texto));
        })];

    private static IEnumerable<string> Errores(EvaluationResults resultado) =>
        (resultado.Details ?? [])
            .Where(d => d.Errors is { Count: > 0 })
            .SelectMany(d => d.Errors!.Select(e => $"{d.InstanceLocation} ({e.Key}): {e.Value}"));
}
