// Nuevo. Delta: Agregado por la IA — alta y encuesta desde texto (PD-3, PD-4). Estilo controllers, fiel al caso base (H-057). Límite de concurrencia del modelo (REC-13).
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovilidadUrbana.Asistente.Concurrencia;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Contracts;

namespace MovilidadUrbana.Asistente.Controllers;

/// <summary>
/// Controller → caso de uso → puerto → adaptador (cruce de capas «a», plan §F.1). El controller no
/// sabe que hay un modelo: llama a <see cref="AltaDesdeTextoService" /> y a
/// <see cref="EncuestaDesdeTextoService" />. Ninguno guarda: devuelven la propuesta para confirmar.
/// </summary>
[ApiController]
[Route("api/v1")]
[EnableRateLimiting(LimitadorDelModelo.Politica)]
[Produces("application/json")]
public sealed class DesdeTextoController(AltaDesdeTextoService altas, EncuestaDesdeTextoService encuestas) : ControllerBase
{
    /// <summary>Interpreta un pedido de alta («Agregá Bella Vista, en Corrientes…») y dice qué falta.</summary>
    [HttpPost("localidades/desde-texto")]
    [ProducesResponseType<PropuestaDeAltaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PropuestaDeAltaDto>> Alta(TextoRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return BadRequest(ValidationProblems.De(new Dictionary<string, string> { ["texto"] = "Escribí el pedido." }));
        }
        var p = await altas.ProponerAsync(request.Texto, cancellationToken);
        var propuesta = p.Propuesta is { } l ? new LocalidadRequest(l.Nombre, l.Provincia, l.CodigoPostal, l.Habitantes) : null;
        return Ok(new PropuestaDeAltaDto(p.Interpretado, propuesta, p.Faltantes, p.ListaParaConfirmar));
    }

    /// <summary>Interpreta un relato de viaje («Voy en bondi…») en claves del catálogo y valida los pasos 2 y 3.</summary>
    [HttpPost("encuestas/desde-texto")]
    [ProducesResponseType<PropuestaDeEncuestaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PropuestaDeEncuestaDto>> Encuesta(TextoRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return BadRequest(ValidationProblems.De(new Dictionary<string, string> { ["texto"] = "Escribí el relato." }));
        }
        var p = await encuestas.ProponerAsync(request.Texto, cancellationToken);
        var e = p.Propuesta;
        return Ok(new PropuestaDeEncuestaDto(p.Interpretado, e?.Medios ?? [], e?.Frecuencia, e?.Distancia, e?.Minutos, e?.Motivo, e?.Edad, p.Errores));
    }
}
