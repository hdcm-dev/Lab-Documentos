// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Infrastructure/Sessions/SessionContext.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — CookieName de la copia de Web (SessionMiddleware la usa; ADR-13); namespace raíz MovilidadUrbana.Asistente (H2-008)
using MovilidadUrbana.Asistente.Application.Abstractions;

namespace MovilidadUrbana.Asistente.Infrastructure.Sessions;

/// <summary>
/// Implementación con alcance de ámbito: una petición HTTP.
///
/// El valor lo establece el middleware de sesión por encabezado (`MiddlewareDeSesionPorEncabezado`) en cada petición. El identificador provisorio del constructor evita que un ámbito sin
/// sesión termine leyendo o escribiendo en un espacio de datos compartido.
/// </summary>
public sealed class SessionContext : ISessionContext
{
    /// <summary>Cookie de la página Blazor; igual que en src/MovilidadUrbana.Web/…/SessionContext.cs @ 8fb4c3a.</summary>
    public const string CookieName = "sesion-movilidad";

    public const int MaxLength = 64;

    public string Id { get; private set; } = Guid.NewGuid().ToString("n");

    public void Set(string id)
    {
        if (IsValid(id)) Id = id;
    }

    public static bool IsValid(string? id) =>
        !string.IsNullOrWhiteSpace(id) && id.Length <= MaxLength;
}
