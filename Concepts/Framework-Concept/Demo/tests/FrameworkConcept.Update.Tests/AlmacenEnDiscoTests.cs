using System.IO.Compression;
using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Update;
using static FrameworkConcept.Update.Tests.Banco;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// El almacén real del cascarón, sobre un directorio temporal: el ciclo completo semilla → v2 → Listo con archivos de
/// verdad, el reemplazo del puntero por rename y lo que no tiene que pasar nunca (zip slip, restos de un corte).
/// </summary>
[TestFixture]
public class AlmacenEnDiscoTests
{
    private string _raiz = null!;
    private Banco _banco = null!;

    [SetUp]
    public void SetUp()
    {
        _raiz = Path.Combine(Path.GetTempPath(), "fc-update-tests-" + Guid.NewGuid().ToString("N"));
        _banco = new Banco();
    }

    [TearDown]
    public void TearDown()
    {
        _banco.Dispose();
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
    }

    private AlmacenDeContenidoEnDisco Almacen() => new(_raiz);

    [Test]
    [Description("Ciclo completo en disco: semilla, consulta, activación con rename, Listo y retención")]
    public async Task CicloCompleto()
    {
        _banco.Semilla = new SemillaDePrueba(V("1.0.0+semilla"));
        var almacen = Almacen();

        var primero = await _banco.Actualizador(almacen).PrepararArranqueAsync();
        await _banco.Actualizador(almacen).ConfirmarListoAsync(V("1.0.0+semilla"));
        _banco.Publicar(contenido: ["1.0.1+abc"]);
        var consulta = await _banco.Actualizador(Almacen()).ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        var segundo = await _banco.Actualizador(Almacen()).PrepararArranqueAsync();
        await _banco.Actualizador(Almacen()).ConfirmarListoAsync(V("1.0.1+abc"));

        Assert.That(primero.Version, Is.EqualTo(V("1.0.0+semilla")));
        Assert.That(consulta.Estado, Is.EqualTo(EstadoDeConsulta.Preparado));
        Assert.That(segundo.Version, Is.EqualTo(V("1.0.1+abc")));
        Assert.That(File.Exists(Path.Combine(segundo.Directorio!, "index.html")), Is.True);
        Assert.That(File.Exists(Path.Combine(segundo.Directorio!, "_framework", "hybridwebview.js")), Is.True);
        Assert.That(await Almacen().LeerPunteroAsync(),
            Is.EqualTo(new PunteroDeContenido(V("1.0.1+abc"), V("1.0.0+semilla"), null, _banco.Reloj.Ahora, 0, "apk-1")));
        Assert.That(Directory.GetDirectories(Path.Combine(_raiz, "staging")), Is.Empty);
        Assert.That(Directory.GetFiles(_raiz, "*.tmp"), Is.Empty, "el temporal del puntero se renombró");
    }

    [Test]
    [Description("Un temporal del puntero a medio escribir (corte durante la escritura) no cambia lo que se lee")]
    public async Task PunteroAMedioEscribir_SeLeeElAnterior()
    {
        var almacen = Almacen();
        await almacen.ReemplazarPunteroAsync(new PunteroDeContenido(V("1.0.1"), null, null));
        await File.WriteAllTextAsync(Path.Combine(_raiz, "puntero.json.tmp"), "{\"current\":\"1.0.");

        Assert.That((await Almacen().LeerPunteroAsync())!.Current, Is.EqualTo(V("1.0.1")));
    }

    [Test]
    [Description("Lo que quedó en tmp/ de una descompresión cortada se borra al abrir y nunca aparece como staging")]
    public async Task RestosDeUnCorte_SeLimpian()
    {
        Almacen();
        var resto = Path.Combine(_raiz, "tmp", "descomprimir-123");
        Directory.CreateDirectory(resto);
        await File.WriteAllTextAsync(Path.Combine(resto, "index.html"), "a medias");

        var almacen = Almacen();

        Assert.That(Directory.Exists(resto), Is.False);
        Assert.That(await almacen.VersionEnStagingAsync(), Is.Null);
    }

    [Test]
    [Description("Un zip con rutas que salen del directorio («zip slip») se rechaza y no escribe afuera")]
    public async Task ZipSlip_Rechazado()
    {
        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            using var flujo = zip.CreateEntry("../../afuera.txt").Open();
            flujo.Write(Encoding.UTF8.GetBytes("no"));
        }
        memoria.Position = 0;

        Assert.ThrowsAsync<InvalidDataException>(() => Almacen().DescomprimirEnStagingAsync(V("1.0.2"), memoria));
        Assert.That(File.Exists(Path.Combine(_raiz, "afuera.txt")), Is.False);
        Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(_raiz)!, "afuera.txt")), Is.False);
        Assert.That(await Almacen().VersionEnStagingAsync(), Is.Null);
    }

    [Test]
    [Description("Las fallidas sobreviven al reinicio del proceso")]
    public async Task Fallidas_Persisten()
    {
        await Almacen().AgregarFallidaAsync(V("1.0.2+abc"));
        await Almacen().AgregarFallidaAsync(V("1.0.2+abc"));

        Assert.That(await Almacen().LeerFallidasAsync(), Is.EquivalentTo(new[] { V("1.0.2+abc") }));
    }

    [Test]
    [Description("El último índice válido sobrevive al reinicio (lo usan las retiradas en el arranque sin red)")]
    public async Task UltimoIndice_Persiste()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"], retiradas: ["1.0.1"]);
        await Almacen().GuardarUltimoIndiceValidoAsync(indice);

        var leido = await Almacen().LeerUltimoIndiceValidoAsync();

        Assert.That(leido!.Retiradas, Is.EqualTo(new[] { V("1.0.1") }));
        Assert.That(leido.Expires, Is.EqualTo(indice.Expires));
    }
}
