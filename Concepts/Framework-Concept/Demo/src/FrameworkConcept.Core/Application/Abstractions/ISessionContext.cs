namespace FrameworkConcept.Core.Application.Abstractions;

/// <summary>
/// Identifica el espacio de datos del visitante actual. La implementación vive fuera de la
/// aplicación (en la demo, en el cascarón: la sesión por dispositivo); la aplicación solo necesita el valor.
/// </summary>
public interface ISessionContext
{
    string Id { get; }
}
