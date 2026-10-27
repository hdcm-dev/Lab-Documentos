using FrameworkConcept.Core.Contracts;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// Qué versión del contenido está corriendo en este proceso. La fija el arranque (una vez: no hay activación en
/// caliente, P2) y la leen el puente, el outbox (para atribuir cada respuesta, folio 023) y el diagnóstico.
/// </summary>
public sealed class ContenidoActivo
{
    public VersionSemantica? Version { get; private set; }

    public string? Directorio { get; private set; }

    /// <summary>Lo que el actualizador anotó en este arranque (rollback, activación, semilla…).</summary>
    public IReadOnlyList<string> NotasDelArranque { get; private set; } = [];

    public void Fijar(VersionSemantica? version, string? directorio, IReadOnlyList<string> notas)
    {
        Version = version;
        Directorio = directorio;
        NotasDelArranque = notas;
    }
}
