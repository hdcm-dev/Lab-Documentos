// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Domain/Entities/Localidad.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
namespace MovilidadUrbana.Asistente.Domain.Entities;

/// <summary>
/// Localidad del ABM. `SesionId` aísla los datos de cada visitante: es lo que en la versión
/// estática de este laboratorio resolvía `localStorage`, y lo que permite que varias pruebas
/// E2E corran en paralelo contra el mismo servidor sin pisarse.
/// </summary>
public class Localidad
{
    public int Id { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Provincia { get; set; } = string.Empty;

    public string CodigoPostal { get; set; } = string.Empty;

    public int Habitantes { get; set; }
}
