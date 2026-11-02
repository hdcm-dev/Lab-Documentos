// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Application/Localidades/LocalidadService.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — GetByNombreYProvinciaAsync (H3-027), SaveAsync informa el id creado (H2-015), mensaje de provincia fuera de catálogo (T2), mensaje del nombre con el largo máximo (REC-17); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain.Entities;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Application.Localidades;

/// <summary>Casos de uso del ABM: listar, dar de alta, modificar y dar de baja localidades.</summary>
public sealed class LocalidadService(ILocalidadRepository repository)
{
    public Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<Result> SaveAsync(LocalidadModel model, CancellationToken cancellationToken = default)
    {
        var errors = Validate(model);
        if (errors.Count > 0) return Result.Invalid(errors);

        var existentes = await repository.GetAllAsync(cancellationToken);
        var duplicada = existentes.Any(item =>
            item.Id != model.Id &&
            LocalidadRules.MismaLocalidad(item.Nombre, item.Provincia, model.Nombre, model.Provincia));

        if (duplicada)
        {
            return Result.Invalid("nombre", "Ya existe una localidad con ese nombre en la provincia.");
        }

        var nombre = model.Nombre.Trim();

        if (model.Id is int id)
        {
            var localidad = await repository.GetByIdAsync(id, cancellationToken);
            if (localidad is null)
            {
                return Result.Invalid("nombre", "La localidad ya no existe.");
            }

            localidad.Nombre = nombre;
            localidad.Provincia = model.Provincia;
            localidad.CodigoPostal = model.CodigoPostal.Trim();
            localidad.Habitantes = model.Habitantes!.Value;
            await repository.UpdateAsync(localidad, cancellationToken);

            return Result.Success($"Se actualizó la localidad {nombre}.");
        }

        var idCreado = await repository.AddAsync(
            new Localidad
            {
                Nombre = nombre,
                Provincia = model.Provincia,
                CodigoPostal = model.CodigoPostal.Trim(),
                Habitantes = model.Habitantes!.Value
            },
            cancellationToken);

        return Result.Success($"Se agregó la localidad {nombre}.", idCreado);
    }

    /// <summary>
    /// Nuevo (H3-027): resuelve una localidad de la sesión por nombre y provincia, para que una
    /// herramienta de baja no reciba un id del modelo. Sin provincia busca sólo por nombre; si eso
    /// deja más de una candidata, devuelve error de ambigüedad y no elige.
    /// </summary>
    public async Task<ResolucionDeLocalidad> GetByNombreYProvinciaAsync(
        string nombre, string? provincia, CancellationToken cancellationToken = default)
    {
        var existentes = await repository.GetAllAsync(cancellationToken);
        var candidatas = existentes
            .Where(item => string.Equals(item.Nombre.Trim(), (nombre ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(provincia) || string.Equals(item.Provincia, provincia, StringComparison.Ordinal))
            .ToList();

        return candidatas.Count switch
        {
            0 => new ResolucionDeLocalidad(null, Result.Invalid("nombre", "No hay una localidad con ese nombre en tu sesión.")),
            1 => new ResolucionDeLocalidad(candidatas[0], null),
            _ => new ResolucionDeLocalidad(null, Result.Invalid("provincia", "Hay más de una localidad con ese nombre: indicá la provincia."))
        };
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var localidad = await repository.GetByIdAsync(id, cancellationToken);
        if (localidad is null)
        {
            return Result.Invalid("nombre", "La localidad ya no existe.");
        }

        await repository.DeleteAsync(id, cancellationToken);
        return Result.Success($"Se eliminó la localidad {localidad.Nombre}.");
    }

    private static Dictionary<string, string> Validate(LocalidadModel model)
    {
        var errors = new Dictionary<string, string>();

        if (!LocalidadRules.NombreValido(model.Nombre))
        {
            errors["nombre"] = $"El nombre debe tener entre {LocalidadRules.LargoMinimoDelNombre} y {LocalidadRules.LargoMaximoDelNombre} caracteres.";
        }
        if (!LocalidadRules.ProvinciaValida(model.Provincia))
        {
            errors["provincia"] = string.IsNullOrWhiteSpace(model.Provincia)
                ? "Seleccione una provincia."
                : "La provincia no está en el catálogo.";
        }
        if (!LocalidadRules.CodigoPostalValido(model.CodigoPostal))
        {
            errors["codigoPostal"] =
                $"El código postal debe tener {LocalidadRules.DigitosDelCodigoPostal} dígitos.";
        }
        if (!LocalidadRules.HabitantesValidos(model.Habitantes))
        {
            errors["habitantes"] = "Ingrese una cantidad de habitantes mayor a cero.";
        }

        return errors;
    }
}

/// <summary>Nuevo (H3-027): la localidad resuelta, o el error que explica por qué no se resolvió.</summary>
public sealed record ResolucionDeLocalidad(Localidad? Localidad, Result? Error);
