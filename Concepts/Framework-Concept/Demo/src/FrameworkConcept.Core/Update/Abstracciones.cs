using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Contenido;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Core.Update;

/// <summary>El reloj, detrás de una interfaz para que las pruebas puedan fijar «ahora» (expires, 15 minutos).</summary>
public interface IReloj
{
    DateTimeOffset Ahora { get; }
}

/// <summary>
/// La red hacia el servidor estático del canal. Las rutas son relativas a su raíz
/// (ver <see cref="RutasDelCanal"/>). Sólo HTTPS en producción (P2); la demo admite su túnel.
/// </summary>
public interface IClienteDeCanal
{
    /// <summary>Un archivo chico entero (el índice, su firma, una definición).</summary>
    /// <exception cref="CanalNoDisponibleException">No hay red o el servidor no responde.</exception>
    Task<byte[]> ObtenerBytesAsync(string ruta, CancellationToken cancellationToken = default);

    /// <summary>Un archivo grande como flujo (un paquete de contenido).</summary>
    /// <exception cref="CanalNoDisponibleException">No hay red o el servidor no responde.</exception>
    Task<Stream> AbrirAsync(string ruta, CancellationToken cancellationToken = default);
}

/// <summary>No hay red o el canal no contesta. No es un rechazo: lo instalado sigue funcionando.</summary>
public sealed class CanalNoDisponibleException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);

/// <summary>
/// Verifica la firma del índice con el conjunto de claves embebido en el cascarón (P2): clave conocida,
/// no revocada, ECDSA P-256 + SHA-256 sobre los bytes exactos de indice.json.
/// </summary>
public interface IVerificadorDeFirma
{
    ResultadoDeVerificacion Verificar(ReadOnlySpan<byte> indiceJson, FirmaDelIndice firma);
}

/// <summary><see cref="MotivoDeRechazo.Ninguno"/> si la firma es válida.</summary>
public sealed record ResultadoDeVerificacion(bool Valida, MotivoDeRechazo Motivo)
{
    public static ResultadoDeVerificacion Ok { get; } = new(true, MotivoDeRechazo.Ninguno);

    public static ResultadoDeVerificacion Rechazo(MotivoDeRechazo motivo) => new(false, motivo);
}

/// <summary>Lo que el predicado de compatibilidad necesita saber del cascarón que está corriendo (P3).</summary>
public interface IEntornoDelCascaron
{
    string Canal { get; }

    VersionSemantica VersionDelPuente { get; }

    IReadOnlyList<EntradaDelCatalogo> Catalogo { get; }

    /// <summary>Mayor del proveedor de WebView activo, o null si no se pudo leer.</summary>
    int? VersionMayorDelWebView { get; }

    /// <summary>ApplicationDisplayVersion del cascarón, para la política de soporte.</summary>
    VersionSemantica VersionDelCascaron { get; }
}

/// <summary>
/// P3 — Un solo predicado, evaluado antes de activar:
///   puente ∈ compatibleConPuente ∧ ∀ r ∈ requires: versión(catálogo[r.id]) ∈ r.rango ∧ motor ≥ mínimo.
/// <c>optional</c> no entra: lo resuelve el contenido con el handshake.
/// </summary>
public interface IPredicadoDeCompatibilidad
{
    ResultadoDeCompatibilidad Evaluar(ManifiestoContenido manifiesto, IEntornoDelCascaron entorno);
}

/// <summary>Si no es compatible, <see cref="Motivos"/> dice qué dimensión falló (una línea por cada una).</summary>
public sealed record ResultadoDeCompatibilidad(bool EsCompatible, IReadOnlyList<string> Motivos);

/// <summary>
/// Las definiciones de encuesta guardadas en el teléfono (P13.3): se conserva toda definición con respuestas
/// en curso o pendientes, independiente de la retención de paquetes. En el cascarón, una tabla de SQLite.
/// </summary>
public interface IAlmacenDeDefiniciones
{
    Task GuardarAsync(DefinicionEncuesta definicion, CancellationToken cancellationToken = default);

    /// <summary>La versión más alta guardada: la que usa la PRÓXIMA encuesta.</summary>
    Task<DefinicionEncuesta?> ObtenerVigenteAsync(string encuestaId, CancellationToken cancellationToken = default);

    /// <summary>Una versión exacta: la que usa una encuesta en curso o una respuesta pendiente.</summary>
    Task<DefinicionEncuesta?> ObtenerAsync(ReferenciaDeDefinicion referencia, CancellationToken cancellationToken = default);
}

/// <summary>
/// P2 — La semilla: una versión COMPLETA del contenido que viaja dentro del APK del cascarón, construida por el mismo
/// pipeline y firmada igual. Se instala como cualquier otra versión (staging → versions → puntero) cuando no hay nada
/// instalado o cuando un cascarón nuevo trae una semilla más nueva que lo instalado. No es destino de rollback.
/// </summary>
public interface ISemillaDeContenido
{
    VersionSemantica Version { get; }

    /// <summary>Tamaño del zip embebido, para comprobarlo igual que un paquete bajado.</summary>
    long Size { get; }

    /// <summary>sha256 del zip, en hex minúscula, tomado del índice firmado del release (lo escribe el guion).</summary>
    string Sha256 { get; }

    Task<Stream> AbrirAsync(CancellationToken cancellationToken = default);

    /// <summary>Las definiciones de encuesta que vienen con el cascarón (la v1 de la demo).</summary>
    IReadOnlyList<DefinicionEncuesta> Definiciones { get; }
}

/// <summary>
/// De dónde sale la semilla, sin cargarla (P1, umbral TPR). Leer el zip del APK y verificar su índice cuesta segundos
/// en un teléfono; el actualizador sólo llama a <see cref="CargarAsync"/> cuando hace falta: en el primer arranque o
/// cuando <see cref="Huella"/> cambió (se instaló otro cascarón, que puede traer otra semilla).
/// </summary>
public interface IFuenteDeSemilla
{
    /// <summary>Identifica la instalación del cascarón; leerla tiene que ser barato (en Android: versión y fecha de instalación).</summary>
    string Huella { get; }

    /// <summary>La semilla verificada, o null si el cascarón no trae una o no verifica.</summary>
    Task<ISemillaDeContenido?> CargarAsync(CancellationToken cancellationToken = default);
}

/// <summary>Una semilla ya cargada, con huella fija (pruebas y herramientas).</summary>
public sealed class FuenteDeSemillaFija(ISemillaDeContenido? semilla, string huella = "fija") : IFuenteDeSemilla
{
    /// <summary>Cuántas veces se pidió la semilla: las pruebas ven si el arranque la cargó o no.</summary>
    public int Cargas { get; private set; }

    public string Huella { get; set; } = huella;

    public Task<ISemillaDeContenido?> CargarAsync(CancellationToken cancellationToken = default)
    {
        Cargas++;
        return Task.FromResult(semilla);
    }
}

/// <summary>
/// ¿Hay red ahora? Sólo lo usa la política de soporte (P2): «sin red no bloquea», porque mandar a la tienda sin red
/// deja a la persona sin app y sin salida. No se usa para decidir nada más del arranque, que nunca toca la red.
/// </summary>
public interface IEstadoDeRed
{
    bool HayRed { get; }
}
