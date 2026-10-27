using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Shell.Data;

/// <summary>
/// P13.3 — <see cref="IAlmacenDeDefiniciones"/> del actualizador, sobre la tabla «definiciones» de la base del cascarón.
/// Las definiciones se guardan junto a las respuestas e independientes de la retención de paquetes: borrar un paquete
/// de contenido nunca borra una definición.
/// </summary>
public sealed class AlmacenDeDefinicionesSqlite(IDb db) : IAlmacenDeDefiniciones
{
    public Task GuardarAsync(DefinicionEncuesta definicion, CancellationToken cancellationToken = default) =>
        db.GuardarDefinicionAsync(definicion, JsonDelContrato.Escribir(definicion), cancellationToken);

    public async Task<DefinicionEncuesta?> ObtenerVigenteAsync(string encuestaId, CancellationToken cancellationToken = default)
    {
        var filas = await db.ListarDefinicionesAsync(encuestaId, cancellationToken);
        return filas.Count == 0 ? null : DefinicionEncuesta.Leer(filas.MaxBy(f => f.Version)!.Json);
    }

    public async Task<DefinicionEncuesta?> ObtenerAsync(ReferenciaDeDefinicion referencia, CancellationToken cancellationToken = default)
    {
        var fila = await db.ObtenerDefinicionAsync(referencia.EncuestaId, referencia.VersionDefinicion, cancellationToken);
        return fila is null ? null : DefinicionEncuesta.Leer(fila.Json);
    }

    /// <summary>
    /// P1, P13.2 — La definición para EMPEZAR una encuesta nueva: la versión más alta que el contenido activo sabe
    /// dibujar (su «compatible-con-contenido» contiene la versión del contenido). La encuesta en curso sigue con la suya.
    /// </summary>
    public async Task<DefinicionEncuesta?> ObtenerParaEmpezarAsync(string encuestaId, VersionSemantica? versionDelContenido, CancellationToken ct = default)
    {
        var filas = await db.ListarDefinicionesAsync(encuestaId, ct);
        return filas
            .OrderByDescending(f => f.Version)
            .Select(f => DefinicionEncuesta.Leer(f.Json))
            .FirstOrDefault(d => versionDelContenido is null || d.CompatibleConContenido.Contiene(versionDelContenido));
    }
}
