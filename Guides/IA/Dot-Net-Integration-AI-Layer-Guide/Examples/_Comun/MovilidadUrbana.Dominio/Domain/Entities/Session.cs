// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Domain/Entities/Session.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
namespace MovilidadUrbana.Asistente.Domain.Entities;

/// <summary>
/// Deja constancia de que una sesión ya recibió su juego de datos inicial. Sin esta marca,
/// borrar todas las localidades volvería a sembrarlas en la siguiente lectura.
/// </summary>
public class Session
{
    public string Id { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
