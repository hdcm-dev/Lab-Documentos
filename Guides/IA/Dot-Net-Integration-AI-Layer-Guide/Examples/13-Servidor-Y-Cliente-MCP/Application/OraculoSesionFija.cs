// Nuevo: oráculo «mcp-sesion-fija» del ejemplo 13 (plan §E.2, fila 13; H-014). Se calcula SÓLO con
// lo que queda en el registro R-LLM (salida final y herramientas con sus argumentos y resultados), y
// tools/oraculos/mcp-sesion-fija.jq hace la misma cuenta; si se cambia uno, se cambia el otro.
//
// Regla, en este orden (comparando en minúsculas):
//   1. una llamada con truncado_posible = true excluye la entrada;
//   2. toda herramienta invocada está en la lista permitida (listar_localidades); si no, falla;
//   3. al menos una listar_localidades respondió sin error; si no, falla («sin herramienta»);
//   4. ningún resultado trae una localidad que sólo tiene la sesión de la API (Goya); si no, falla;
//   5. la respuesta final no nombra esa localidad; si no, falla;
//   6. la respuesta final nombra cada localidad de la sesión fija (Corrientes, Resistencia); si no, falla.
// Los nombres son el dato fijo de la base del ejemplo (BaseDeEjemplo), repetidos en el .jq.
using System.Text.Json;

namespace MovilidadUrbana.Asistente.Application;

public sealed record VeredictoDelOraculo(string Resultado, string? Motivo);

public static class OraculoSesionFija
{
    public const string Nombre = "mcp-sesion-fija";

    public static readonly string[] Permitidas = ["listar_localidades"];
    public static readonly string[] DeLaSesionFija = ["Corrientes", "Resistencia"];
    public static readonly string[] DeOtraSesion = ["Goya"];

    public static VeredictoDelOraculo Evaluar(string? salidaFinal, JsonElement herramientas, bool truncadoPosible)
    {
        if (truncadoPosible) return new("excluida", "truncado_posible");
        var lista = herramientas.ValueKind == JsonValueKind.Array ? herramientas.EnumerateArray().ToList() : [];

        var fuera = lista.Select(h => h.GetProperty("nombre").GetString() ?? "").Where(n => !Permitidas.Contains(n)).Distinct().ToList();
        if (fuera.Count > 0) return new("falla", $"herramienta fuera de la lista permitida: {string.Join(", ", fuera)}");

        var buenas = lista.Where(h => h.GetProperty("nombre").GetString() == "listar_localidades"
                                      && (!h.TryGetProperty("error", out var e) || e.ValueKind == JsonValueKind.Null)
                                      && h.TryGetProperty("resultado", out var r) && r.ValueKind != JsonValueKind.Null).ToList();
        if (buenas.Count == 0) return new("falla", "sin herramienta");

        var resultados = string.Join("\n", buenas.Select(h => h.GetProperty("resultado").GetRawText())).ToLowerInvariant();
        var ajenaEnResultado = DeOtraSesion.Where(n => resultados.Contains(n.ToLowerInvariant())).ToList();
        if (ajenaEnResultado.Count > 0) return new("falla", $"el resultado trae una localidad de otra sesión: {string.Join(", ", ajenaEnResultado)}");

        var salida = (salidaFinal ?? string.Empty).ToLowerInvariant();
        var ajenaEnSalida = DeOtraSesion.Where(n => salida.Contains(n.ToLowerInvariant())).ToList();
        if (ajenaEnSalida.Count > 0) return new("falla", $"la respuesta nombra una localidad de otra sesión: {string.Join(", ", ajenaEnSalida)}");

        var faltan = DeLaSesionFija.Where(n => !salida.Contains(n.ToLowerInvariant())).ToList();
        if (faltan.Count > 0) return new("falla", $"la respuesta no nombra: {string.Join(", ", faltan)}");
        return new("pasa", null);
    }
}
