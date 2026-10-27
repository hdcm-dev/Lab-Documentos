namespace FrameworkConcept.Shell.Data;

// P5 — Las tablas del cascarón. El cascarón es su dueño: el contenido no hace DDL, sólo guarda documentos JSON por
// el puente. C-1 (folio 023): el esquema es un «sobre fijo» de columnas estables. Una actualización de contenido
// nunca necesita migración; un cambio acá exige subir EsquemaDelCascaron.Version y sale por tienda.
//
// Las fechas se guardan como milisegundos Unix (long): SQLite no compara ni ordena DateTimeOffset.

/// <summary>Una fila del outbox: algo que hay que enviar al servidor cuando haya red.</summary>
public sealed class FilaDeOutbox
{
    public long Id { get; set; }

    /// <summary>Única. El receptor deduplica por ella: reenviar nunca duplica (P13.6).</summary>
    public string ClaveIdempotencia { get; set; } = "";

    /// <summary>P4 — Cada fila tiene dueño. En la demo, la sesión por dispositivo (no hay login).</summary>
    public string Dueno { get; set; } = "";

    /// <summary>Qué hay que hacer en el servidor, por ejemplo «respuestas.crear».</summary>
    public string Accion { get; set; } = "";

    /// <summary>P5 — El orden es por entidad: una fila trabada sólo frena a las que siguen de la misma entidad.</summary>
    public string Entidad { get; set; } = "";

    /// <summary>Versión de la forma de <see cref="Payload"/>.</summary>
    public int VersionPayload { get; set; }

    /// <summary>Versión del contenido con que se tomó (atribución, folio 023 ítem 4).</summary>
    public string VersionContenido { get; set; } = "";

    /// <summary>P13.2 — Con qué definición se tomó: {encuestaId, versionDefinicion}.</summary>
    public string EncuestaId { get; set; } = "";

    public int VersionDefinicion { get; set; }

    /// <summary>D-04 — Si la acción modifica algo existente, la versión que se espera encontrar (If-Match → 412).</summary>
    public int? VersionEsperada { get; set; }

    /// <summary>El cuerpo JSON que se envía.</summary>
    public string Payload { get; set; } = "";

    public EstadoDeFila Estado { get; set; }

    public int Intentos { get; set; }

    public long CreadaEn { get; set; }

    /// <summary>Backoff: no se reintenta antes de este momento.</summary>
    public long ProximoIntentoEn { get; set; }

    public long? EnviadaEn { get; set; }

    public string? UltimoError { get; set; }
}

public enum EstadoDeFila
{
    Pendiente = 0,
    Enviada = 1,

    /// <summary>Carta muerta: el servidor la rechazó. No se borra; se muestra al usuario (P5, P13.5).</summary>
    Rechazada = 2,

    /// <summary>Carta muerta: falló algo que reintentar no arregla (por ejemplo, un payload ilegible).</summary>
    Error = 3
}

/// <summary>P5 — Un documento JSON del contenido: colección, id, versión de forma y el JSON tal cual.</summary>
public sealed class FilaDeDocumento
{
    public string Coleccion { get; set; } = "";
    public string Id { get; set; } = "";
    public int VersionDeForma { get; set; }

    /// <summary>Sube en cada cambio; con versionEsperada el puente rechaza el cambio de otro (D-04).</summary>
    public int Version { get; set; }

    public string Json { get; set; } = "";
    public long ActualizadoEn { get; set; }
}

/// <summary>P13.3 — Una versión de una definición de encuesta. Se conservan las que tienen respuestas sin enviar.</summary>
public sealed class FilaDeDefinicion
{
    public string EncuestaId { get; set; } = "";
    public int Version { get; set; }
    public string Json { get; set; } = "";
    public long GuardadaEn { get; set; }
}

/// <summary>Versión de contenido: qué pasó con cada versión en este teléfono (activada, Listo, rollback…).</summary>
public sealed class FilaDeVersionDeContenido
{
    public long Id { get; set; }
    public string Version { get; set; } = "";
    public string Evento { get; set; } = "";
    public long En { get; set; }
}

/// <summary>Pares clave-valor del cascarón (id de instalación, última consulta, …).</summary>
public sealed class FilaDeConfiguracion
{
    public string Clave { get; set; } = "";
    public string Valor { get; set; } = "";
}
