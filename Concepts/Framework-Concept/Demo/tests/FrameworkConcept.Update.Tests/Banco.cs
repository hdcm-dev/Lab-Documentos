using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Contenido;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// El banco de pruebas: un «teléfono» (almacén en memoria, entorno, reloj) y un «servidor del canal» (diccionario de
/// rutas a bytes) con claves de prueba generadas en cada corrida. Ninguna clave de prueba se guarda en disco.
///
/// Uso típico:
///   var banco = new Banco();
///   banco.Publicar(contenido: ["1.0.1"]);
///   var resultado = await banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
/// </summary>
public sealed class Banco : IDisposable
{
    public const string KeyIdActiva = "demo-activa";
    public const string KeyIdReserva = "demo-reserva";
    public const string KeyIdRevocada = "demo-revocada";

    public ECDsa ClaveActiva { get; } = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    public ECDsa ClaveReserva { get; } = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    public ECDsa ClaveRevocada { get; } = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    /// <summary>Una clave que el cascarón no conoce (C04, «otra clave»).</summary>
    public ECDsa ClaveAjena { get; } = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    public RelojFijo Reloj { get; } = new(new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero));
    public EntornoDePrueba Entorno { get; } = new();
    public CanalEnMemoria Canal { get; } = new();
    public AlmacenEnMemoria Almacen { get; } = new();
    public AlmacenDeDefinicionesEnMemoria Definiciones { get; } = new();
    public EstadoDeRedFijo Red { get; } = new();
    private ISemillaDeContenido? _semilla;

    /// <summary>La semilla del «APK». Al fijarla se arma su fuente, con la huella de una instalación («apk-1»).</summary>
    public ISemillaDeContenido? Semilla
    {
        get => _semilla;
        set
        {
            _semilla = value;
            FuenteDeSemilla = value is null ? null : new FuenteDeSemillaFija(value, "apk-1");
        }
    }

    /// <summary>La fuente de la semilla: cuenta cuántas veces el arranque la cargó; su huella simula reinstalar el APK.</summary>
    public FuenteDeSemillaFija? FuenteDeSemilla { get; private set; }

    public IVerificadorDeFirma Verificador => new VerificadorDeFirmaEs256(
    [
        new ClaveDePublicacion(KeyIdActiva, RolDeClave.Activa, FirmaEs256.ExportarPublica(ClaveActiva)),
        new ClaveDePublicacion(KeyIdReserva, RolDeClave.Reserva, FirmaEs256.ExportarPublica(ClaveReserva)),
        new ClaveDePublicacion(KeyIdRevocada, RolDeClave.Activa, FirmaEs256.ExportarPublica(ClaveRevocada), Revocada: true)
    ]);

    /// <summary>Un actualizador nuevo sobre el MISMO teléfono: es lo que pasa en cada arranque del proceso.</summary>
    public Actualizador Actualizador(IAlmacenDeContenido? almacen = null) =>
        new(almacen ?? Almacen, Definiciones, Canal, Verificador, new PredicadoDeCompatibilidad(), Entorno, Reloj, FuenteDeSemilla, Red);

    /// <summary>
    /// Un arranque en frío en el que el WebView EMPIEZA a cargar el contenido y el contenido no llega a avisar «Listo»
    /// (lo que hace un contenido roto, o una app cerrada en plena carga).
    /// </summary>
    public async Task<ResultadoDeArranque> ArrancarYCargarSinListo()
    {
        var arranque = await Actualizador().PrepararArranqueAsync();
        if (arranque.Version is { } version)
            await Actualizador().AnotarCargaIniciadaAsync(version);
        return arranque;
    }

    // ───────────── El teléfono ya con contenido instalado ─────────────

    /// <summary>Deja <paramref name="current"/> (y <paramref name="previous"/>) instaladas y confirmadas, como tras un uso normal.</summary>
    public void Instalar(string current, string? previous = null)
    {
        foreach (var v in new[] { previous, current }.OfType<string>())
            Almacen.Versiones[V(v)] = Paquetes.Archivos(Paquetes.Zip(V(v)));
        Almacen.Puntero = new PunteroDeContenido(V(current), previous is null ? null : V(previous), null);
    }

    // ───────────── El servidor del canal ─────────────

    /// <summary>
    /// Publica un índice firmado en /dev/puente-1/ con los paquetes y definiciones dados. Devuelve el índice para
    /// que la prueba pueda alterarlo y volver a publicarlo.
    /// </summary>
    public IndiceDelCanal Publicar(
        string[]? contenido = null,
        DefinicionEncuesta[]? definiciones = null,
        string[]? retiradas = null,
        DefinicionRetirada[]? definicionesRetiradas = null,
        Func<VersionSemantica, ManifiestoContenido>? manifiesto = null,
        string? cascaronMinimo = null,
        bool mandatory = false)
    {
        var entradasDeContenido = new List<EntradaDeContenido>();
        foreach (var texto in contenido ?? [])
        {
            var version = V(texto);
            var zip = Paquetes.Zip(version, manifiesto?.Invoke(version));
            var ruta = $"contenido/{version}.zip";
            Canal.Archivos[RutasDelCanal.Archivo(Canales.Dev, 1, ruta)] = zip;
            entradasDeContenido.Add(new EntradaDeContenido(version, ruta, zip.Length, Sha(zip), mandatory));
        }

        var entradasDeDefinicion = new List<EntradaDeDefinicion>();
        foreach (var definicion in definiciones ?? [])
        {
            var bytes = Encoding.UTF8.GetBytes(JsonDelContrato.Escribir(definicion));
            var ruta = $"definiciones/{definicion.EncuestaId}/v{definicion.Version}.json";
            Canal.Archivos[RutasDelCanal.Archivo(Canales.Dev, 1, ruta)] = bytes;
            entradasDeDefinicion.Add(new EntradaDeDefinicion(definicion.EncuestaId, definicion.Version, ruta, bytes.Length, Sha(bytes), definicion.CompatibleConContenido));
        }

        var indice = new IndiceDelCanal(
            IndiceDelCanal.FormatoActual, Canales.Dev, 1, KeyIdActiva,
            Reloj.Ahora, Reloj.Ahora + ParametrosDeProducto.VigenciaDelIndice,
            new EntradasDelIndice(entradasDeContenido, entradasDeDefinicion),
            [.. (retiradas ?? []).Select(V)],
            definicionesRetiradas ?? [],
            cascaronMinimo is null ? null : V(cascaronMinimo));

        PublicarIndice(indice);
        return indice;
    }

    /// <summary>Serializa, firma y publica un índice tal cual (para armar índices vencidos, de otro canal, etc.).</summary>
    public void PublicarIndice(IndiceDelCanal indice, ECDsa? clave = null, string? keyIdDeLaFirma = null)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonDelContrato.Escribir(indice));
        var firma = FirmaEs256.Firmar(bytes, clave ?? ClaveActiva, keyIdDeLaFirma ?? indice.KeyId);
        Canal.Archivos[RutasDelCanal.Indice(Canales.Dev, 1)] = bytes;
        Canal.Archivos[RutasDelCanal.FirmaDelIndice(Canales.Dev, 1)] = Encoding.UTF8.GetBytes(JsonDelContrato.Escribir(firma));
    }

    public byte[] IndicePublicado
    {
        get => Canal.Archivos[RutasDelCanal.Indice(Canales.Dev, 1)];
        set => Canal.Archivos[RutasDelCanal.Indice(Canales.Dev, 1)] = value;
    }

    public FirmaDelIndice FirmaPublicada
    {
        get => JsonDelContrato.Leer<FirmaDelIndice>(Encoding.UTF8.GetString(Canal.Archivos[RutasDelCanal.FirmaDelIndice(Canales.Dev, 1)]));
        set => Canal.Archivos[RutasDelCanal.FirmaDelIndice(Canales.Dev, 1)] = Encoding.UTF8.GetBytes(JsonDelContrato.Escribir(value));
    }

    public byte[] PaquetePublicado(string version)
    {
        return Canal.Archivos[RutasDelCanal.Archivo(Canales.Dev, 1, $"contenido/{V(version)}.zip")];
    }

    public void ReemplazarPaquete(string version, byte[] bytes) =>
        Canal.Archivos[RutasDelCanal.Archivo(Canales.Dev, 1, $"contenido/{V(version)}.zip")] = bytes;

    public static VersionSemantica V(string texto) => VersionSemantica.Parse(texto);

    public static string Sha(byte[] bytes) => VerificacionDelIndice.Sha256Hex(bytes);

    public void Dispose()
    {
        ClaveActiva.Dispose();
        ClaveReserva.Dispose();
        ClaveRevocada.Dispose();
        ClaveAjena.Dispose();
    }
}

/// <summary>Paquetes de contenido de prueba: un zip plano con manifiesto.json e index.html, como el del empaquetador.</summary>
public static class Paquetes
{
    public static ManifiestoContenido Manifiesto(
        VersionSemantica version,
        string puente = ">=1.0.0 <2.0.0",
        RequisitoDeCapacidad[]? requires = null,
        RequisitoDeCapacidad[]? optional = null,
        int webView = ParametrosDeProducto.WebViewMinimo) =>
        new(version, RangoDeVersion.Parse(puente),
            new CapacidadesDelContenido(requires ?? [new RequisitoDeCapacidad("device.info", RangoDeVersion.Parse(">=1.0.0 <2.0.0"))], optional ?? []),
            new MotorMinimo(webView, ParametrosDeProducto.IosMinimo));

    public static byte[] Zip(VersionSemantica version, ManifiestoContenido? manifiesto = null)
    {
        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            Escribir(zip, ManifiestoContenido.NombreDeArchivo, JsonDelContrato.Escribir(manifiesto ?? Manifiesto(version)));
            Escribir(zip, "index.html", $"<!doctype html><base href=\"/\"><title>contenido {version}</title>");
            Escribir(zip, "_framework/hybridwebview.js", "// transporte del cascarón (C-2)");
        }
        return memoria.ToArray();
    }

    public static Dictionary<string, byte[]> Archivos(byte[] zip)
    {
        using var archivo = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        return archivo.Entries.Where(e => e.Name.Length > 0).ToDictionary(e => e.FullName, e =>
        {
            using var flujo = e.Open();
            using var copia = new MemoryStream();
            flujo.CopyTo(copia);
            return copia.ToArray();
        });
    }

    private static void Escribir(ZipArchive zip, string nombre, string texto)
    {
        using var flujo = zip.CreateEntry(nombre).Open();
        flujo.Write(Encoding.UTF8.GetBytes(texto));
    }
}

public static class Definiciones
{
    /// <summary>Una definición mínima de prueba (las reales están en definiciones/ y las prueba Core.Tests).</summary>
    public static DefinicionEncuesta De(int version, string compatibleConContenido = ">=1.0.0 <2.0.0", string encuestaId = "encuesta-movilidad") =>
        new(encuestaId, version, RangoDeVersion.Parse(compatibleConContenido), $"Encuesta v{version}",
        [
            new PasoDeEncuesta("paso-1", "Datos", [new PreguntaDeEncuesta("nombre", TipoDePregunta.Texto, "Nombre", "Obligatorio", "Falta el nombre")])
        ]);
}

// ───────────────────────────── Dobles de prueba ─────────────────────────────

public sealed class RelojFijo(DateTimeOffset ahora) : IReloj
{
    public DateTimeOffset Ahora { get; set; } = ahora;
}

public sealed class EstadoDeRedFijo : IEstadoDeRed
{
    public bool HayRed { get; set; } = true;
}

public sealed class EntornoDePrueba : IEntornoDelCascaron
{
    public string Canal { get; set; } = Canales.Dev;
    public VersionSemantica VersionDelPuente { get; set; } = VersionSemantica.Parse("1.0.0");
    public IReadOnlyList<EntradaDelCatalogo> Catalogo { get; set; } = CatalogoDeCapacidades.Entradas;
    public int? VersionMayorDelWebView { get; set; } = 120;
    public VersionSemantica VersionDelCascaron { get; set; } = VersionSemantica.Parse("1.0.0");
}

/// <summary>El servidor del canal: rutas → bytes. Puede «quedarse sin red» o cortar una descarga a la mitad.</summary>
public sealed class CanalEnMemoria : IClienteDeCanal
{
    public Dictionary<string, byte[]> Archivos { get; } = [];
    public bool SinRed { get; set; }

    /// <summary>Si tiene valor, los paquetes se entregan sólo hasta ese byte y el flujo termina (respuesta truncada).</summary>
    public int? TruncarEn { get; set; }

    /// <summary>Si tiene valor, el flujo del paquete tira IOException después de ese byte (conexión caída a mitad).</summary>
    public int? CortarConexionEn { get; set; }

    public int Llamadas { get; private set; }
    public int DescargasDePaquetes { get; private set; }

    public Task<byte[]> ObtenerBytesAsync(string ruta, CancellationToken cancellationToken = default)
    {
        Llamadas++;
        if (SinRed) throw new CanalNoDisponibleException("sin red (prueba)");
        return Archivos.TryGetValue(ruta, out var bytes)
            ? Task.FromResult(bytes.ToArray())
            : throw new CanalNoDisponibleException($"404 {ruta}");
    }

    public Task<Stream> AbrirAsync(string ruta, CancellationToken cancellationToken = default)
    {
        Llamadas++;
        DescargasDePaquetes++;
        if (SinRed) throw new CanalNoDisponibleException("sin red (prueba)");
        if (!Archivos.TryGetValue(ruta, out var bytes)) throw new CanalNoDisponibleException($"404 {ruta}");
        if (TruncarEn is int n) bytes = bytes[..n];
        Stream flujo = new MemoryStream(bytes, writable: false);
        if (CortarConexionEn is int c) flujo = new FlujoQueSeCorta(flujo, c);
        return Task.FromResult(flujo);
    }

    private sealed class FlujoQueSeCorta(Stream interno, int limite) : Stream
    {
        private int _leidos;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => _leidos; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_leidos >= limite) throw new IOException("conexión cortada (prueba)");
            var n = interno.Read(buffer, offset, Math.Min(count, limite - _leidos));
            _leidos += n;
            return n;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) interno.Dispose();
            base.Dispose(disposing);
        }
    }
}

/// <summary>Lo que lanza el almacén cuando la prueba «corta la luz» en un paso.</summary>
public sealed class CorteProvocadoException(string paso) : Exception($"corte provocado en «{paso}»");

/// <summary>
/// El almacén del teléfono, en memoria. Cada operación que cambia algo tiene un nombre de paso; si la prueba pone
/// <see cref="CortarEn"/>, esa operación lanza ANTES de hacer nada, como un proceso que muere justo ahí.
/// </summary>
public sealed class AlmacenEnMemoria : IAlmacenDeContenido
{
    public const string PasoPromover = "promover";
    public const string PasoPuntero = "puntero";
    public const string PasoFallida = "fallida";
    public const string PasoBorrar = "borrar";
    public const string PasoStaging = "staging";

    public PunteroDeContenido? Puntero { get; set; }
    public HashSet<VersionSemantica> Fallidas { get; } = [];
    public Dictionary<VersionSemantica, Dictionary<string, byte[]>> Staging { get; } = [];
    public Dictionary<VersionSemantica, Dictionary<string, byte[]>> Versiones { get; } = [];
    public IndiceDelCanal? UltimoIndice { get; set; }

    /// <summary>Paso en el que se corta, y en qué aparición de ese paso (1 = la primera).</summary>
    public (string Paso, int Aparicion)? CortarEn { get; set; }

    public List<string> Pasos { get; } = [];

    private void Paso(string paso)
    {
        Pasos.Add(paso);
        if (CortarEn is { } corte && corte.Paso == paso && Pasos.Count(p => p == paso) == corte.Aparicion)
        {
            CortarEn = null;
            throw new CorteProvocadoException(paso);
        }
    }

    public Task<PunteroDeContenido?> LeerPunteroAsync(CancellationToken cancellationToken = default) => Task.FromResult(Puntero);

    public Task ReemplazarPunteroAsync(PunteroDeContenido puntero, CancellationToken cancellationToken = default)
    {
        Paso(PasoPuntero);
        Puntero = puntero;
        return Task.CompletedTask;
    }

    public Task<IReadOnlySet<VersionSemantica>> LeerFallidasAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlySet<VersionSemantica>>(Fallidas.ToHashSet());

    public Task AgregarFallidaAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        Paso(PasoFallida);
        Fallidas.Add(version);
        return Task.CompletedTask;
    }

    public Task DescomprimirEnStagingAsync(VersionSemantica version, Stream zip, CancellationToken cancellationToken = default)
    {
        Paso(PasoStaging);
        using var copia = new MemoryStream();
        zip.CopyTo(copia);
        var archivos = Paquetes.Archivos(copia.ToArray());   // InvalidDataException si no es un zip
        Staging.Clear();
        Staging[version] = archivos;
        return Task.CompletedTask;
    }

    public Task<VersionSemantica?> VersionEnStagingAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Staging.Keys.Max());

    public Task<ManifiestoContenido?> LeerManifiestoAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        var archivos = Staging.GetValueOrDefault(version) ?? Versiones.GetValueOrDefault(version);
        return Task.FromResult(archivos?.TryGetValue(ManifiestoContenido.NombreDeArchivo, out var json) == true
            ? JsonDelContrato.Leer<ManifiestoContenido>(Encoding.UTF8.GetString(json))
            : null);
    }

    public Task PromoverAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        Paso(PasoPromover);
        if (Versiones.ContainsKey(version)) throw new IOException($"versions/{version} ya existe: es inmutable");
        Versiones[version] = Staging[version];
        Staging.Remove(version);
        return Task.CompletedTask;
    }

    public bool ExisteVersion(VersionSemantica version) => Versiones.ContainsKey(version);

    public string DirectorioDe(VersionSemantica version) => $"content/versions/{version}";

    public Task BorrarVersionAsync(VersionSemantica version, CancellationToken cancellationToken = default)
    {
        Paso(PasoBorrar);
        Versiones.Remove(version);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VersionSemantica>> ListarVersionesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<VersionSemantica>>([.. Versiones.Keys]);

    public Task LimpiarStagingAsync(CancellationToken cancellationToken = default)
    {
        Staging.Clear();
        return Task.CompletedTask;
    }

    public Task<IndiceDelCanal?> LeerUltimoIndiceValidoAsync(CancellationToken cancellationToken = default) => Task.FromResult(UltimoIndice);

    public Task GuardarUltimoIndiceValidoAsync(IndiceDelCanal indice, CancellationToken cancellationToken = default)
    {
        UltimoIndice = indice;
        return Task.CompletedTask;
    }
}

public sealed class AlmacenDeDefinicionesEnMemoria : IAlmacenDeDefiniciones
{
    public List<DefinicionEncuesta> Guardadas { get; } = [];

    public Task GuardarAsync(DefinicionEncuesta definicion, CancellationToken cancellationToken = default)
    {
        Guardadas.RemoveAll(d => d.Referencia == definicion.Referencia);
        Guardadas.Add(definicion);
        return Task.CompletedTask;
    }

    public Task<DefinicionEncuesta?> ObtenerVigenteAsync(string encuestaId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Guardadas.Where(d => d.EncuestaId == encuestaId).MaxBy(d => d.Version));

    public Task<DefinicionEncuesta?> ObtenerAsync(ReferenciaDeDefinicion referencia, CancellationToken cancellationToken = default) =>
        Task.FromResult(Guardadas.FirstOrDefault(d => d.Referencia == referencia));
}

public sealed class SemillaDePrueba(VersionSemantica version, byte[]? zip = null, params DefinicionEncuesta[] definiciones) : ISemillaDeContenido
{
    private readonly byte[] _zip = zip ?? Paquetes.Zip(version);

    public VersionSemantica Version { get; } = version;
    public long Size => _zip.Length;
    public string Sha256 => Banco.Sha(_zip);
    public IReadOnlyList<DefinicionEncuesta> Definiciones { get; } = definiciones;

    public Task<Stream> AbrirAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream>(new MemoryStream(_zip, writable: false));
}
