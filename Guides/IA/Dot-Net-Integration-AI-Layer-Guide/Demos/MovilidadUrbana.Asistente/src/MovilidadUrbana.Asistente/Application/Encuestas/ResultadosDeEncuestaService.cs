// Nuevo: no existe en el caso base (N6 §1.1, hecho 1: IEncuestaRepository sólo tenía AddAsync y CountAsync).
// Delta: Agregado por la IA — caso de uso de lectura que el asistente necesita para responder sobre resultados.
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application.Encuestas;

/// <summary>Porcentaje de respuestas de la sesión que eligieron una clave del catálogo.</summary>
/// <param name="Clave">Clave de <see cref="Catalogos" />.</param>
/// <param name="Etiqueta">Etiqueta que se muestra.</param>
/// <param name="Cantidad">Respuestas que la eligieron.</param>
/// <param name="Porcentaje">Cantidad sobre el total, de 0 a 100, redondeado a un decimal.</param>
public sealed record PorcentajeDeClave(string Clave, string Etiqueta, int Cantidad, double Porcentaje);

/// <summary>
/// Agregados de la encuesta. Por construcción no lleva <c>Nombre</c> ni <c>Edad</c>: la ausencia
/// de datos personales es un invariante del tipo, no una revisión posterior (H-005).
/// </summary>
public sealed record ResultadosDeEncuesta(
    int Total,
    IReadOnlyList<PorcentajeDeClave> Medios,
    IReadOnlyList<PorcentajeDeClave> Frecuencias,
    IReadOnlyList<PorcentajeDeClave> Motivos);

/// <summary>Cuentas deterministas sobre las respuestas de la sesión actual.</summary>
public sealed class ResultadosDeEncuestaService(IEncuestaRepository repository)
{
    public async Task<ResultadosDeEncuesta> ResumirAsync(CancellationToken cancellationToken = default)
    {
        var respuestas = await repository.GetAllAsync(cancellationToken);
        var total = respuestas.Count;

        // Un medio cuenta una vez por respuesta: la encuesta es de elección múltiple, así que los
        // porcentajes de medios pueden sumar más de 100.
        var medios = Catalogos.Medios
            .Select(m => Porcentaje(m.Clave, m.Etiqueta, respuestas.Count(r => r.Medios.Contains(m.Clave)), total))
            .ToList();
        var frecuencias = Catalogos.Frecuencias
            .Select(f => Porcentaje(f.Clave, f.Etiqueta, respuestas.Count(r => r.Frecuencia == f.Clave), total))
            .ToList();
        var motivos = Catalogos.Motivos
            .Select(m => Porcentaje(m.Clave, m.Etiqueta, respuestas.Count(r => r.Motivo == m.Clave), total))
            .ToList();

        return new ResultadosDeEncuesta(total, medios, frecuencias, motivos);
    }

    private static PorcentajeDeClave Porcentaje(string clave, string etiqueta, int cantidad, int total) =>
        new(clave, etiqueta, cantidad, total == 0 ? 0 : Math.Round(100.0 * cantidad / total, 1));
}
