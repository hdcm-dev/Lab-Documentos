// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Controllers/ValidationProblems.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using Microsoft.AspNetCore.Mvc;

namespace MovilidadUrbana.Asistente.Controllers;

/// <summary>
/// Traduce los errores por campo de la capa de aplicación al <see cref="ValidationProblemDetails"/>
/// estándar (RFC 9457), sin que la aplicación sepa que existe HTTP.
/// </summary>
internal static class ValidationProblems
{
    public static ValidationProblemDetails De(IReadOnlyDictionary<string, string> errors, string? titulo = null)
    {
        var detalles = new ValidationProblemDetails(errors.ToDictionary(e => e.Key, e => new[] { e.Value }))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = titulo ?? "La solicitud tiene datos inválidos."
        };
        return detalles;
    }
}
