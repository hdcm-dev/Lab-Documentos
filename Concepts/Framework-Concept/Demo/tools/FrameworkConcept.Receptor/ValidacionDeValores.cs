using System.Text.Json;
using FrameworkConcept.Core.Application.Definiciones;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Receptor;

/// <summary>
/// El receptor valida los valores contra la definición CON LA QUE SE TOMÓ la respuesta (P13.2, P13.4), no contra
/// la vigente: una encuesta tomada con v1 y enviada después de publicada v2 se valida con v1.
/// Usa el mismo validador que la pantalla (<see cref="DefinicionValidator"/>), así ambos lados dicen lo mismo.
/// </summary>
internal static class ValidacionDeValores
{
    /// <returns>Los errores, por pregunta. Vacío si la respuesta es válida.</returns>
    public static IReadOnlyDictionary<string, string> Validar(DefinicionEncuesta definicion, IReadOnlyDictionary<string, JsonElement> valores)
    {
        var errores = new Dictionary<string, string>();
        foreach (var pregunta in definicion.Preguntas)
        {
            var valor = valores.TryGetValue(pregunta.Id, out var json) ? Convertir(pregunta.Tipo, json) : null;
            if (!DefinicionValidator.EsValida(pregunta, valor)) errores[pregunta.Id] = pregunta.MensajeDeError;
        }
        return errores;
    }

    /// <summary>
    /// JSON → la forma que espera el validador (texto y opción única: string; entero: int; decimal: double;
    /// opción múltiple: lista de string). Si el tipo JSON no corresponde, se devuelve el JsonElement tal cual,
    /// que el validador no acepta.
    /// </summary>
    private static object? Convertir(TipoDePregunta tipo, JsonElement json)
    {
        if (json.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;

        return tipo switch
        {
            TipoDePregunta.Texto or TipoDePregunta.OpcionUnica when json.ValueKind == JsonValueKind.String => json.GetString(),
            TipoDePregunta.Entero when json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var entero) => entero,
            TipoDePregunta.Decimal when json.ValueKind == JsonValueKind.Number => json.GetDouble(),
            TipoDePregunta.OpcionMultiple when json.ValueKind == JsonValueKind.Array
                                               && json.EnumerateArray().All(e => e.ValueKind == JsonValueKind.String)
                => json.EnumerateArray().Select(e => e.GetString()!).ToList(),
            _ => json
        };
    }
}
