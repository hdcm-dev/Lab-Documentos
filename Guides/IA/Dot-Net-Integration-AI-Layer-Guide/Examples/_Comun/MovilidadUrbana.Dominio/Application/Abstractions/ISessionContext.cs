// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Application/Abstractions/ISessionContext.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
namespace MovilidadUrbana.Asistente.Application.Abstractions;

/// <summary>
/// Identifica el espacio de datos del visitante actual. La implementación vive en
/// Infraestructura porque depende de cookies y de HTTP; la aplicación solo necesita el valor.
/// </summary>
public interface ISessionContext
{
    string Id { get; }
}
