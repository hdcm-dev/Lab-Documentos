using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Hosting;

namespace FrameworkConcept.Shell.Actualizacion;

/// <summary>
/// Arma el actualizador de Core (P2) con las piezas de Android. Se arma una sola vez y de forma asíncrona, porque
/// necesita leer del APK la clave pública, y eso no se hace en el hilo de la interfaz. La semilla NO se lee acá: el
/// actualizador la pide a <see cref="FuenteDeSemillaEmbebida"/> sólo cuando hace falta (P1, umbral TPR).
///
///   Actualizador (Core) ← AlmacenDeContenidoEnDisco (Core, sobre AppDataDirectory/content)
///                       ← AlmacenDeDefinicionesSqlite (este cascarón, tabla «definiciones»)
///                       ← ClienteDeCanalHttp, RelojDelSistema, EstadoDeRedMaui (adaptadores de este cascarón)
///                       ← VerificadorDeFirmaEs256 + PredicadoDeCompatibilidad (Core)
///                       ← EntornoDelCascaron (catálogo, puente, WebView)
///                       ← FuenteDeSemillaEmbebida (assets semilla/ del APK, se leen sólo si hace falta)
/// </summary>
public sealed class ProveedorDelActualizador(
    ConfiguracionDelCascaron configuracion,
    AlmacenDeDefinicionesSqlite definiciones,
    ClienteDeCanalHttp canal,
    EntornoDelCascaron entorno)
{
    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private IActualizador? _actualizador;

    /// <summary>content/ dentro de los datos privados de la app: staging/, versions/, puntero.json, fallidas.json.</summary>
    public static string RaizDelContenido => Path.Combine(FileSystem.AppDataDirectory, "content");

    /// <summary>Los archivos del contenido. Lo usa también la pantalla de diagnóstico para leer el puntero.</summary>
    public IAlmacenDeContenido Almacen { get; } = new AlmacenDeContenidoEnDisco(RaizDelContenido);

    /// <summary>La semilla del APK: se carga sólo en el primer arranque o con otro APK instalado.</summary>
    public FuenteDeSemillaEmbebida? Semilla { get; private set; }

    /// <summary>Cuántas claves públicas trae el cascarón (0 = no puede verificar ningún índice).</summary>
    public int ClavesEmbebidas { get; private set; }

    public async Task<IActualizador> ObtenerAsync()
    {
        if (_actualizador is not null) return _actualizador;

        await _cerrojo.WaitAsync();
        try
        {
            if (_actualizador is not null) return _actualizador;

            var claves = await ConjuntoDeClaves.CargarAsync(configuracion.KeyId);
            ClavesEmbebidas = claves.Count;
            var verificador = new VerificadorDeFirmaEs256(claves);
            Semilla = new FuenteDeSemillaEmbebida(verificador, configuracion.Canal, ContratoDelPuente.Version.Mayor);

            _actualizador = new Actualizador(
                Almacen, definiciones, canal, verificador, new PredicadoDeCompatibilidad(), entorno,
                new RelojDelSistema(), Semilla, new EstadoDeRedMaui());
            return _actualizador;
        }
        finally
        {
            _cerrojo.Release();
        }
    }
}
