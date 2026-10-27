using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Tools;

/// <summary>
/// La carpeta de UN canal y UN mayor del puente en el servidor estático (P2):
///
///   {raiz}/{canal}/puente-{mayor}/
///     indice.json                 el índice del canal (lo escribe «firmar»)
///     indice.json.sig             su firma
///     paquetes/contenido-X.Y.Z.zip           paquetes de contenido (los escribe «empaquetar»)
///     definiciones/{encuestaId}/vN.json      definiciones de encuesta (las escribe «publicar-definicion»)
///     estado-de-publicacion.json  lo que decidió quien publica (retiros, obligatorias); NO lo baja el teléfono
///     bitacora.jsonl              una línea por publicación, para la atribución y la revisión
///
/// La carpeta es la fuente de verdad: «firmar» arma el índice a partir de lo que hay adentro.
/// </summary>
internal sealed class CarpetaDelCanal
{
    public const string RaizPorDefecto = "salida/canal";

    public CarpetaDelCanal(string raiz, string canal, int puenteMayor)
    {
        if (!Canales.Todos.Contains(canal))
            throw new ErrorDeUso($"canal «{canal}» desconocido; los canales son {string.Join(", ", Canales.Todos)}");

        Raiz = raiz;
        Canal = canal;
        PuenteMayor = puenteMayor;
        Ruta = Path.Combine(raiz, RutasDelCanal.Carpeta(canal, puenteMayor));
    }

    public string Raiz { get; }
    public string Canal { get; }
    public int PuenteMayor { get; }

    /// <summary>{raiz}/{canal}/puente-{mayor}</summary>
    public string Ruta { get; }

    public string Indice => Path.Combine(Ruta, "indice.json");
    public string Firma => Path.Combine(Ruta, "indice.json.sig");
    public string Paquetes => Path.Combine(Ruta, "paquetes");
    public string Definiciones => Path.Combine(Ruta, "definiciones");
    private string Estado => Path.Combine(Ruta, "estado-de-publicacion.json");
    private string Bitacora => Path.Combine(Ruta, "bitacora.jsonl");

    /// <summary>Opciones comunes: --raiz, --canal (dev) y --puente (el mayor del puente de este código).</summary>
    public static CarpetaDelCanal Desde(Argumentos a) => new(
        a.Opcion("raiz", RaizPorDefecto),
        a.Opcion("canal", Canales.Dev),
        int.Parse(a.Opcion("puente", ContratoDelPuente.Version.Mayor.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture));

    /// <summary>Ruta relativa a la carpeta del índice, con «/» (como la escribe el índice).</summary>
    public string Relativa(string rutaCompleta) =>
        Path.GetRelativePath(Ruta, rutaCompleta).Replace(Path.DirectorySeparatorChar, '/');

    /// <summary>paquetes/contenido-X.Y.Z.zip. El nombre no lleva el +commit: la identidad X.Y.Z no se reutiliza (P7).</summary>
    public string RutaDelPaquete(VersionSemantica version) =>
        Path.Combine(Paquetes, $"contenido-{version.SinBuild()}.zip");

    public string RutaDeLaDefinicion(string encuestaId, int version) =>
        Path.Combine(Definiciones, encuestaId, $"v{version.ToString(CultureInfo.InvariantCulture)}.json");

    public EstadoDePublicacion LeerEstado() =>
        File.Exists(Estado)
            ? JsonDelContrato.Leer<EstadoDePublicacion>(File.ReadAllText(Estado))
            : new EstadoDePublicacion();

    public void GuardarEstado(EstadoDePublicacion estado)
    {
        Directory.CreateDirectory(Ruta);
        EscribirAtomico(Estado, JsonDelContrato.Escribir(estado) + "\n");
    }

    /// <summary>Agrega una línea a la bitácora de publicación (qué, cuándo y los campos que exige el PR, P6).</summary>
    public void Anotar(string accion, object detalle)
    {
        Directory.CreateDirectory(Ruta);
        var linea = JsonSerializer.Serialize(new { cuando = DateTimeOffset.UtcNow, accion, detalle });
        File.AppendAllText(Bitacora, linea + "\n");
    }

    /// <summary>Escribe un temporal y lo renombra: quien lee ve el archivo viejo o el nuevo, nunca uno a medias.</summary>
    public static void EscribirAtomico(string ruta, string texto) =>
        EscribirAtomico(ruta, System.Text.Encoding.UTF8.GetBytes(texto));

    public static void EscribirAtomico(string ruta, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ruta))!);
        var temporal = ruta + ".tmp";
        File.WriteAllBytes(temporal, bytes);
        File.Move(temporal, ruta, overwrite: true);
    }

    /// <summary>SHA-256 en hex minúscula, como lo escribe el índice.</summary>
    public static string Sha256(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    public static string Sha256DeArchivo(string ruta)
    {
        using var flujo = File.OpenRead(ruta);
        return Convert.ToHexStringLower(SHA256.HashData(flujo));
    }
}

/// <summary>
/// Lo que decidió quien publica y que el índice tiene que repetir cada vez que se firma de nuevo.
/// </summary>
internal sealed class EstadoDePublicacion
{
    /// <summary>P2 — Versiones de contenido retiradas.</summary>
    public List<VersionSemantica> Retiradas { get; set; } = [];

    /// <summary>P13.5 — Retiros de definiciones, con su fecha de anuncio y de retiro.</summary>
    public List<DefinicionRetirada> DefinicionesRetiradas { get; set; } = [];

    /// <summary>P2 — Versiones de contenido marcadas «mandatory».</summary>
    public List<VersionSemantica> Obligatorias { get; set; } = [];

    /// <summary>P2 — Política de soporte del cascarón (no es compatibilidad).</summary>
    public VersionSemantica? CascaronMinimoSoportado { get; set; }
}
