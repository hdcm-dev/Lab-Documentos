using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Core.Update;

/// <summary>P2 — Los tres disparadores de la MISMA rutina de consulta.</summary>
public enum DisparadorDeConsulta
{
    ArranqueEnFrio,

    /// <summary>Login con cuenta exitoso. En la demo no hay login (P4): no se ejecuta.</summary>
    LoginExitoso,

    /// <summary>Volver desde segundo plano: consulta sólo si pasaron 15 minutos desde la última consulta exitosa.</summary>
    Reanudacion
}

public enum EstadoDeArranque
{
    /// <summary>Hay contenido sano para mostrar.</summary>
    ContenidoListo,

    /// <summary>P2 — Ni current ni previous están sanos: pantalla propia del cascarón «no hay contenido sano».</summary>
    SinContenidoSano,

    /// <summary>C-3 de E01 — El WebView del sistema es menor que el mínimo: pantalla «actualizá el WebView».</summary>
    MotorDesactualizado,

    /// <summary>P2 — Política de soporte: el cascarón está bajo la versión mínima soportada.</summary>
    CascaronNoSoportado
}

/// <summary>Qué mostrar al arrancar y desde dónde.</summary>
/// <param name="Version">La versión de contenido activa, si hay.</param>
/// <param name="Directorio">Raíz de archivos de esa versión (versions/vN), que el servidor de contenido sirve.</param>
/// <param name="Notas">Lo que pasó (rollback, retirada, activación), para la bitácora y la pantalla de diagnóstico.</param>
public sealed record ResultadoDeArranque(
    EstadoDeArranque Estado,
    VersionSemantica? Version,
    string? Directorio,
    IReadOnlyList<string> Notas);

public enum EstadoDeConsulta
{
    /// <summary>Reanudación antes de los 15 minutos: no se consultó.</summary>
    Omitida,

    /// <summary>Sin red: no se fuerza nada (P2).</summary>
    SinRed,

    /// <summary>No hay nada nuevo.</summary>
    AlDia,

    /// <summary>Hay algo nuevo verificado: contenido en staging/ y/o definiciones guardadas.</summary>
    Preparado,

    /// <summary>Se rechazó el índice o un paquete; ver <see cref="ResultadoDeConsulta.Motivo"/>.</summary>
    Rechazado
}

/// <summary>Por qué se rechazó algo. Cada valor tiene su rojo provocado en la suite del actualizador (P8).</summary>
public enum MotivoDeRechazo
{
    Ninguno,
    FirmaInvalida,
    ClaveDesconocida,
    ClaveRevocada,
    IndiceVencido,
    OtroCanal,
    OtroMayorDelPuente,
    FormatoDesconocido,
    VersionNoMayor,
    TamanoDistinto,
    HashDistinto,
    PaqueteInvalido,
    ContenidoConPreRelease,
    Incompatible,
    Retirada,
    FallidaLocal
}

/// <summary>Resultado de una consulta al canal.</summary>
/// <param name="ContenidoPreparado">Versión que quedó verificada en staging/; se activa en el próximo arranque en frío.</param>
/// <param name="DefinicionesNuevas">Definiciones guardadas; se usan al empezar la próxima encuesta (P13.2).</param>
public sealed record ResultadoDeConsulta(
    EstadoDeConsulta Estado,
    MotivoDeRechazo Motivo,
    string? Detalle,
    VersionSemantica? ContenidoPreparado,
    IReadOnlyList<ReferenciaDeDefinicion> DefinicionesNuevas);
