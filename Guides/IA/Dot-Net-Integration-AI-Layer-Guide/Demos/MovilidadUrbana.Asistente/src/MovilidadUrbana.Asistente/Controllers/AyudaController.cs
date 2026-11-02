// Nuevo. Delta: Agregado por la IA — ayuda con recuperación (PD-2). Excepción de lectura declarada (§A.4). Límite de concurrencia del modelo (REC-13).
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovilidadUrbana.Asistente.Concurrencia;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Contracts;

namespace MovilidadUrbana.Asistente.Controllers;

/// <summary>
/// Controller → puerto directo (cruce de capas «b», plan §F.1). Es la excepción de lectura: no hay
/// caso de uso en el medio porque la ayuda no escribe ni lee datos de la sesión; responde desde los
/// requisitos de <c>*Policy</c>. Fuera del chat, así que el contexto recuperado nunca convive con
/// herramientas de escritura (política de origen, regla 1).
/// </summary>
[ApiController]
[Route("api/v1/ayuda")]
[EnableRateLimiting(LimitadorDelModelo.Politica)]
[Produces("application/json")]
public sealed class AyudaController(IAyuda ayuda) : ControllerBase
{
    /// <summary>Responde una pregunta sobre los requisitos del alta o de la encuesta, con la fuente citada.</summary>
    [HttpPost]
    [ProducesResponseType<RespuestaDeAyudaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RespuestaDeAyudaDto>> Preguntar(PreguntaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Pregunta))
        {
            return BadRequest(ValidationProblems.De(new Dictionary<string, string> { ["pregunta"] = "Escribí la pregunta." }));
        }
        var r = await ayuda.ResponderAsync(request.Pregunta, cancellationToken);
        return Ok(new RespuestaDeAyudaDto(r.Texto, r.Fuente, r.CitaValida,
            r.Recuperados.Select(f => new FragmentoDto(f.Id, f.Texto, f.Puntaje)).ToList(), r.PlanDeEmbeddings));
    }
}
