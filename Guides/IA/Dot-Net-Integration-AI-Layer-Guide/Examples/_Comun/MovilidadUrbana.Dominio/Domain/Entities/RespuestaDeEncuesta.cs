// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Domain/Entities/RespuestaDeEncuesta.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
namespace MovilidadUrbana.Asistente.Domain.Entities;

/// <summary>Una encuesta de transporte completada.</summary>
public class RespuestaDeEncuesta
{
    public int Id { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int Edad { get; set; }

    public string Localidad { get; set; } = string.Empty;

    /// <summary>Medios de transporte elegidos. Se persiste como texto separado por comas.</summary>
    public IReadOnlyList<string> Medios { get; set; } = [];

    public string Frecuencia { get; set; } = string.Empty;

    public double Distancia { get; set; }

    public int Minutos { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public DateTimeOffset RegistradaEn { get; set; }
}
