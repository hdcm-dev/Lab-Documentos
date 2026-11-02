// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Controllers/LocalidadesController.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — deja de leer del repositorio salteando el servicio (N6 §1.3, F04 §3.2) y ubica lo creado por el id que informa SaveAsync (H2-015); namespace raíz MovilidadUrbana.Asistente (H2-008)
using Microsoft.AspNetCore.Mvc;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Contracts;
using MovilidadUrbana.Asistente.Domain.Entities;

namespace MovilidadUrbana.Asistente.Controllers;

/// <summary>ABM de localidades. Cada sesión —encabezado <c>X-Sesion-Id</c>— ve solo las suyas.</summary>
[ApiController]
[Route("api/v1/localidades")]
[Produces("application/json")]
public sealed class LocalidadesController(LocalidadService service) : ControllerBase
{
    /// <summary>Lista las localidades de la sesión.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LocalidadDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LocalidadDto>>> GetAll(CancellationToken cancellationToken)
    {
        var localidades = await service.GetAllAsync(cancellationToken);
        return Ok(localidades.Select(ToDto).ToList());
    }

    /// <summary>Devuelve una localidad por su identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LocalidadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalidadDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var localidad = await BuscarAsync(id, cancellationToken);
        return localidad is null ? NotFound() : Ok(ToDto(localidad));
    }

    /// <summary>Da de alta una localidad.</summary>
    [HttpPost]
    [ProducesResponseType<LocalidadDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocalidadDto>> Create(LocalidadRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SaveAsync(ToModel(request), cancellationToken);
        if (!result.IsSuccess) return BadRequest(ValidationProblems.De(result.Errors));

        // El servicio informa el id de lo que creó (H2-015): ya no hace falta ubicarlo por nombre y provincia.
        var creada = (await BuscarAsync(result.Id!.Value, cancellationToken))!;

        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, ToDto(creada));
    }

    /// <summary>Modifica una localidad existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<LocalidadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalidadDto>> Update(int id, LocalidadRequest request, CancellationToken cancellationToken)
    {
        if (await BuscarAsync(id, cancellationToken) is null) return NotFound();

        var model = ToModel(request);
        model.Id = id;
        var result = await service.SaveAsync(model, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ValidationProblems.De(result.Errors));

        return Ok(ToDto((await BuscarAsync(id, cancellationToken))!));
    }

    /// <summary>Da de baja una localidad.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound();
    }

    // Lectura por el caso de uso, acotada a la sesión como toda lectura del servicio.
    private async Task<Localidad?> BuscarAsync(int id, CancellationToken cancellationToken) =>
        (await service.GetAllAsync(cancellationToken)).FirstOrDefault(l => l.Id == id);

    private static LocalidadDto ToDto(Localidad l) => new(l.Id, l.Nombre, l.Provincia, l.CodigoPostal, l.Habitantes);

    private static LocalidadModel ToModel(LocalidadRequest s) => new()
    {
        Nombre = s.Nombre ?? string.Empty,
        Provincia = s.Provincia ?? string.Empty,
        CodigoPostal = s.CodigoPostal ?? string.Empty,
        Habitantes = s.Habitantes
    };
}
