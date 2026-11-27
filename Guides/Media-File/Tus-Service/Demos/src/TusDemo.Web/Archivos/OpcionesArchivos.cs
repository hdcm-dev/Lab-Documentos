namespace TusDemo.Web.Archivos;

/// <summary>
/// Configuración del servicio de archivos. Se lee de la sección "Archivos" de appsettings.json
/// o de variables de entorno con doble guion bajo (por ejemplo, Archivos__Raiz=/data/tus).
/// </summary>
public sealed class OpcionesArchivos
{
    public const string Seccion = "Archivos";

    /// <summary>Carpeta donde el almacén de tus guarda los archivos y su estado.</summary>
    public string Raiz { get; set; } = "datos/tus";

    /// <summary>Archivo JSON que hace de tabla de archivos registrados (la "fila" de la demo).</summary>
    public string Registro { get; set; } = "datos/registro.json";

    /// <summary>Tamaño máximo de una suba, en bytes. Una suba más grande recibe 413.</summary>
    public long TopeBytes { get; set; } = 500L * 1024 * 1024;

    /// <summary>Extensiones permitidas, con el punto y en minúsculas.</summary>
    public string[] Extensiones { get; set; } = [];

    /// <summary>Horas sin actividad después de las cuales una suba incompleta vence.</summary>
    public int HorasDeVencimiento { get; set; } = 72;

    /// <summary>Tamaño de trozo que usan los clientes de la demo.</summary>
    public int TrozoBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>Clave compartida de la demo. Sólo para aprender: no es un esquema de producción.</summary>
    public string ClaveDemo { get; set; } = "";
}
