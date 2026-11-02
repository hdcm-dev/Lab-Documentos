// Nuevo. AC-6 en cada build: las versiones directas son las del Anexo F y cada familia queda en una
// sola versión, también entre las transitivas (se lee el deps.json que resolvió NuGet).
using System.Text.Json;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class VersionesTests
{
    private static readonly Dictionary<string, string> AnexoF = new()
    {
        ["Microsoft.Extensions.AI"] = "10.11.0",
        ["Microsoft.Extensions.AI.Abstractions"] = "10.11.0",
        ["OllamaSharp"] = "5.5.0",
        ["Microsoft.Extensions.VectorData.Abstractions"] = "10.11.0",
        ["CommunityToolkit.VectorData.InMemory"] = "1.0.1",
        ["Microsoft.EntityFrameworkCore.Sqlite"] = "10.0.14",
        ["Microsoft.AspNetCore.OpenApi"] = "10.0.11",
        ["Scalar.AspNetCore"] = "2.17.3",
        ["Microsoft.AspNetCore.Mvc.Testing"] = "10.0.11",
        ["NUnit"] = "4.3.2"
    };

    private static readonly string[] Familias =
        ["Microsoft.Extensions.AI", "Microsoft.Extensions.VectorData", "Microsoft.Agents.AI", "ModelContextProtocol", "OpenTelemetry", "Microsoft.EntityFrameworkCore"];

    private static List<(string Id, string Version)> Paquetes()
    {
        var deps = Directory.GetFiles(AppContext.BaseDirectory, "MovilidadUrbana.Asistente.Tests.deps.json").Single();
        using var doc = JsonDocument.Parse(File.ReadAllText(deps));
        return doc.RootElement.GetProperty("libraries").EnumerateObject()
            .Where(l => l.Value.GetProperty("type").GetString() == "package")
            .Select(l => (l.Name.Split('/')[0], l.Name.Split('/')[1]))
            .ToList();
    }

    [Test]
    [Description("Versiones directas = Anexo F")]
    public void VersionesDirectas_SonLasDelAnexoF()
    {
        var paquetes = Paquetes().ToDictionary(p => p.Id, p => p.Version);
        foreach (var (id, version) in AnexoF)
        {
            Assert.That(paquetes.GetValueOrDefault(id), Is.EqualTo(version), id);
        }
    }

    [Test]
    [Description("Cada familia en una sola versión y ninguna preview")]
    public void Familias_UnaSolaVersion_SinPreviews()
    {
        var paquetes = Paquetes();
        foreach (var familia in Familias)
        {
            var versiones = paquetes.Where(p => p.Id == familia || p.Id.StartsWith(familia + ".", StringComparison.Ordinal)).Select(p => p.Version).Distinct().ToList();
            Assert.That(versiones, Has.Count.LessThanOrEqualTo(1), $"{familia}: {string.Join(", ", versiones)}");
        }
        Assert.That(paquetes.Where(p => System.Text.RegularExpressions.Regex.IsMatch(p.Version, "-(preview|rc|alpha|beta)")).Select(p => $"{p.Id} {p.Version}"), Is.Empty);
    }
}
