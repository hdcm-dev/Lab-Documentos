// Nuevo. Las entradas congeladas de la demo (entradas.v1.json, «escrito para el ejemplo»).
using System.Text.Json;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Tests;

public sealed record Entrada(string Id, string Paso, bool Humo, string Texto);

public static class Entradas
{
    public static string Ruta => Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");

    public static string Hash => Hashes.Sha256DeArchivo(Ruta);

    public static IReadOnlyList<Entrada> Todas()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Ruta));
        return doc.RootElement.GetProperty("entradas").EnumerateArray()
            .Select(e => new Entrada(e.GetProperty("id").GetString()!, e.GetProperty("paso").GetString()!,
                e.GetProperty("humo").GetBoolean(), e.GetProperty("texto").GetString()!))
            .ToList();
    }

    public static string Texto(string id) => Todas().Single(e => e.Id == id).Texto;
}
