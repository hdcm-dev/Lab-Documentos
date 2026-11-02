// Nuevo. Delta: Agregado por la IA — caso de uso que orquesta extractor y validación de pasos (PD-4; plan §A.4 punto 6).
using MovilidadUrbana.Asistente.Application.Abstractions;

namespace MovilidadUrbana.Asistente.Application.Encuestas;

/// <summary>Resultado de interpretar un relato. No registra: la persona revisa y completa el paso 1.</summary>
public sealed record PropuestaDeEncuesta(bool Interpretado, EncuestaPropuesta? Propuesta, IReadOnlyDictionary<string, string> Errores);

/// <summary>
/// «Voy en bondi y a veces en bici…»: el extractor propone claves de catálogo y
/// <see cref="EncuestaService.ValidarPaso" /> (endurecido, T2) decide. Un medio inventado no se
/// registra vacío: es un error del paso 2.
/// </summary>
public sealed class EncuestaDesdeTextoService(IExtractorDeEncuesta extractor, EncuestaService encuestas)
{
    public async Task<PropuestaDeEncuesta> ProponerAsync(string texto, CancellationToken cancellationToken = default)
    {
        var propuesta = await extractor.ExtraerAsync(texto, cancellationToken);
        if (propuesta is null)
        {
            return new PropuestaDeEncuesta(false, null, new Dictionary<string, string>
            {
                ["texto"] = "No pude interpretar el relato: completá la encuesta paso por paso."
            });
        }

        var model = new EncuestaModel
        {
            Edad = propuesta.Edad,
            Frecuencia = propuesta.Frecuencia ?? string.Empty,
            Distancia = propuesta.Distancia,
            Minutos = propuesta.Minutos,
            Motivo = propuesta.Motivo ?? string.Empty
        };
        foreach (var medio in propuesta.Medios) model.AlternarMedio(medio, elegido: true);

        // Sólo los pasos que un relato puede traer; el paso 1 (nombre, localidad) es del formulario.
        var errores = encuestas.ValidarPaso(2, model)
            .Concat(encuestas.ValidarPaso(3, model))
            .ToDictionary(e => e.Key, e => e.Value);
        return new PropuestaDeEncuesta(true, propuesta, errores);
    }
}
