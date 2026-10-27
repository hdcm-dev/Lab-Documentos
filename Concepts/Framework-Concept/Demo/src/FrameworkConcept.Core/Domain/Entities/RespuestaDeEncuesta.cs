namespace FrameworkConcept.Core.Domain.Entities;

/// <summary>Una encuesta de transporte completada.</summary>
public class RespuestaDeEncuesta
{
    public int Id { get; set; }

    public string SessionId { get; set; } = string.Empty;

    // P13.2 — Cada respuesta sabe con qué definición se tomó, desde que se empieza a cargar.
    // Si mientras tanto llega una definición nueva, esta respuesta se termina y se envía con la suya.

    /// <summary>Identificador estable de la encuesta (por ejemplo, «encuesta-movilidad»).</summary>
    public string EncuestaId { get; set; } = string.Empty;

    /// <summary>Versión (entero) de la definición con la que se tomó.</summary>
    public int VersionDefinicion { get; set; }

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
