using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Update;
using static FrameworkConcept.Update.Tests.Banco;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// El arranque en frío (P2): rollback por marca, retiradas, estado terminal, semilla, retención, y las pantallas
/// propias del cascarón (WebView viejo, cascarón no soportado). Nada de esto usa la red.
/// </summary>
[TestFixture]
public class ArranqueTests
{
    private Banco _banco = null!;

    [SetUp]
    public void SetUp() => _banco = new Banco();

    [TearDown]
    public void TearDown() => _banco.Dispose();

    private Task<ResultadoDeArranque> Arrancar() => _banco.Actualizador().PrepararArranqueAsync();

    /// <summary>Publica, baja y activa <paramref name="version"/>; el WebView empieza a cargarla y no llega Listo.</summary>
    private async Task PrepararYArrancar(string version)
    {
        _banco.Publicar(contenido: [version]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        await _banco.ArrancarYCargarSinListo();
    }

    /// <summary>Los arranques que hacen falta para que la versión activa, que nunca dice Listo, quede como fallida.</summary>
    private async Task<ResultadoDeArranque> ArrancarHastaElRollback()
    {
        // La activación ya sumó una carga sin Listo; faltan las demás y el arranque que decide.
        for (var i = 1; i < Actualizador.CargasSinListoParaFallar; i++)
            await _banco.ArrancarYCargarSinListo();
        return await _banco.ArrancarYCargarSinListo();
    }

    // ───────────── Rollback por marca ─────────────

    [Test]
    [Description("N4 / P2 — Rollback: el paquete activado empezó a cargar y nunca dijo Listo; tras CargasSinListoParaFallar cargas vuelve a previous y la anota como fallida")]
    public async Task SinListo_VuelveAPrevious()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");          // activa 1.0.2 con la marca «pendiente»; carga sin Listo

        var arranque = await ArrancarHastaElRollback();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Almacen.Fallidas, Does.Contain(V("1.0.2")));
        Assert.That(arranque.Notas, Has.Some.Contains("rollback"));
    }

    [Test]
    [Description("P2 — Una sola carga sin Listo (la app se cerró mientras cargaba) no alcanza: la versión se reintenta")]
    public async Task UnaCargaSinListo_SeReintenta()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");

        var arranque = await Arrancar();

        Assert.That(arranque.Version, Is.EqualTo(V("1.0.2")));
        Assert.That(_banco.Almacen.Fallidas, Is.Empty);
        Assert.That(_banco.Almacen.Puntero!.Pendiente, Is.EqualTo(V("1.0.2")));
        Assert.That(arranque.Notas, Has.Some.Contains("se reintenta"));
    }

    [Test]
    [Description("P2 — Arranques cortados ANTES de cargar (sin pedido de dotnet.js) no cuentan: nunca hay rollback por eso")]
    public async Task ArranquesSinCarga_NoSonFalla()
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        for (var i = 0; i < 5; i++)
            await Arrancar();                      // el proceso muere antes de que el WebView pida nada

        Assert.That(_banco.Almacen.Puntero!.Current, Is.EqualTo(V("1.0.2")));
        Assert.That(_banco.Almacen.Puntero!.CargasSinListo, Is.Zero);
        Assert.That(_banco.Almacen.Fallidas, Is.Empty);
    }

    [Test]
    [Description("P2 — Mientras current espera Listo, una versión nueva en staging/ no se activa (previous sería una sin confirmar)")]
    public async Task ConPendiente_StagingEspera()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");
        _banco.Publicar(contenido: ["1.0.3"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        var espera = await Arrancar();
        await _banco.Actualizador().ConfirmarListoAsync(V("1.0.2"));
        var activa = await Arrancar();

        Assert.That(espera.Version, Is.EqualTo(V("1.0.2")));
        Assert.That(espera.Notas, Has.Some.Contains("espera"));
        Assert.That(activa.Version, Is.EqualTo(V("1.0.3")));
        Assert.That(_banco.Almacen.Puntero!.Previous, Is.EqualTo(V("1.0.2")));
    }

    [Test]
    [Description("P2 — Con Listo(version) la marca se borra y el arranque siguiente no hace rollback")]
    public async Task ConListo_SeQueda()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");
        await _banco.Actualizador().ConfirmarListoAsync(V("1.0.2"));

        var arranque = await Arrancar();

        Assert.That(arranque.Version, Is.EqualTo(V("1.0.2")));
        Assert.That(_banco.Almacen.Puntero!.Pendiente, Is.Null);
        Assert.That(_banco.Almacen.Fallidas, Is.Empty);
    }

    [Test]
    [Description("P2 — Un Listo de otra versión no confirma la pendiente")]
    public async Task ListoDeOtraVersion_NoConfirma()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");
        await _banco.Actualizador().ConfirmarListoAsync(V("1.0.1"));

        Assert.That(_banco.Almacen.Puntero!.Pendiente, Is.EqualTo(V("1.0.2")));
    }

    [Test]
    [Description("P2 — Una fallida local no se reinstala aunque el índice la siga ofreciendo")]
    public async Task Fallida_NoSeReinstala()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");
        await ArrancarHastaElRollback();

        var consulta = await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        Assert.That(consulta.Motivo, Is.EqualTo(MotivoDeRechazo.FallidaLocal));
        Assert.That(_banco.Canal.DescargasDePaquetes, Is.EqualTo(1), "sólo la primera vez");
    }

    // ───────────── Retiradas y estado terminal ─────────────

    [Test]
    [Description("P2 — Retirada: current figura en «retiradas» del último índice válido → vuelve a previous en este arranque")]
    public async Task CurrentRetirada_VuelveAPrevious()
    {
        _banco.Instalar("1.0.2", previous: "1.0.1");
        _banco.Publicar(contenido: ["1.0.1", "1.0.2"], retiradas: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        var arranque = await Arrancar();

        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Almacen.Fallidas, Does.Contain(V("1.0.2")));
    }

    [Test]
    [Description("P2 — Retirar no rebaja la monotonía: una 1.0.0 vieja sigue sin aceptarse después de volver a 1.0.1")]
    public async Task Retirada_NoRebajaLaMonotonia()
    {
        _banco.Instalar("1.0.2", previous: "1.0.1");
        _banco.Publicar(contenido: ["1.0.0", "1.0.2"], retiradas: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        await Arrancar();

        var consulta = await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        Assert.That(consulta.Motivo, Is.EqualTo(MotivoDeRechazo.VersionNoMayor));
    }

    [Test]
    [Description("P2 — current y previous retiradas: estado terminal, sin volver a la semilla")]
    public async Task AmbasRetiradas_EstadoTerminal()
    {
        _banco.Semilla = new SemillaDePrueba(V("1.0.0"));
        _banco.Instalar("1.0.2", previous: "1.0.1");
        _banco.Publicar(contenido: ["1.0.2"], retiradas: ["1.0.1", "1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        var arranque = await Arrancar();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.SinContenidoSano));
        Assert.That(_banco.Almacen.ExisteVersion(V("1.0.0")), Is.False, "la semilla no es destino de rollback");
        Assert.That(arranque.Notas, Has.Some.Contains("current 1.0.2"));
    }

    [Test]
    [Description("P2 — Doble falla sin red: falla la nueva, se vuelve a previous, falla previous → estado terminal; los arranques siguientes lo repiten")]
    public async Task DobleFallaSinRed_EstadoTerminal()
    {
        _banco.Semilla = new SemillaDePrueba(V("1.0.0"));
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");
        _banco.Canal.SinRed = true;

        var primera = await ArrancarHastaElRollback();   // 1.0.2 sin Listo → vuelve a 1.0.1, con su marca
        var segunda = await ArrancarHastaElRollback();   // 1.0.1 tampoco dice Listo → no queda ninguna sana
        var tercera = await Arrancar();

        Assert.That(primera.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(segunda.Estado, Is.EqualTo(EstadoDeArranque.SinContenidoSano));
        Assert.That(tercera.Estado, Is.EqualTo(EstadoDeArranque.SinContenidoSano));
        Assert.That(_banco.Almacen.Fallidas, Is.EquivalentTo(new[] { V("1.0.1"), V("1.0.2") }));
        Assert.That(_banco.Almacen.ExisteVersion(V("1.0.0")), Is.False, "no se vuelve a la semilla");
    }

    [Test]
    [Description("P2 — Desde el estado terminal, con red, se baja una versión nueva y se activa al rearrancar")]
    public async Task EstadoTerminal_SeRecuperaConUnaNueva()
    {
        _banco.Instalar("1.0.1");
        await PrepararYArrancar("1.0.2");
        await ArrancarHastaElRollback();
        await ArrancarHastaElRollback();           // terminal

        _banco.Publicar(contenido: ["1.0.3"]);
        var consulta = await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        var arranque = await Arrancar();

        Assert.That(consulta.ContenidoPreparado, Is.EqualTo(V("1.0.3")));
        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.3")));
        Assert.That(_banco.Almacen.Puntero!.Previous, Is.Null, "una fallida no queda como previous");
    }

    // ───────────── Semilla ─────────────

    [Test]
    [Description("N4 — Primer arranque sin red: usa la semilla, que se instala como una versión completa")]
    public async Task PrimerArranqueSinRed_UsaLaSemilla()
    {
        _banco.Canal.SinRed = true;
        _banco.Semilla = new SemillaDePrueba(V("1.0.0+semilla"), null, Definiciones.De(1));

        var arranque = await Arrancar();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.0+semilla")));
        Assert.That(arranque.Directorio, Is.EqualTo("content/versions/1.0.0+semilla"));
        Assert.That(_banco.Almacen.Puntero!.Pendiente, Is.EqualTo(V("1.0.0+semilla")), "la semilla también confirma con Listo");
        Assert.That(_banco.Definiciones.Guardadas.Select(d => d.Version), Is.EqualTo(new[] { 1 }));
        Assert.That(_banco.Canal.Llamadas, Is.Zero);
    }

    [Test]
    [Description("P2 — Una semilla alterada (sha256) no se instala")]
    public async Task SemillaAlterada_NoSeInstala()
    {
        var zip = Paquetes.Zip(V("1.0.0"));
        var semilla = new SemillaDePrueba(V("1.0.0"), zip);
        var sha = semilla.Sha256;
        zip[^3] ^= 0xFF;
        _banco.Semilla = new SemillaConShaFijo(semilla, sha);

        var arranque = await Arrancar();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.SinContenidoSano));
    }

    [Test]
    [Description("P2 — Un cascarón nuevo (por tienda) con una semilla más nueva que lo instalado la activa")]
    public async Task SemillaMasNueva_SeActiva()
    {
        _banco.Instalar("1.0.1");
        _banco.Semilla = new SemillaDePrueba(V("1.1.0"));

        var arranque = await Arrancar();

        Assert.That(arranque.Version, Is.EqualTo(V("1.1.0")));
        Assert.That(_banco.Almacen.Puntero!.Previous, Is.EqualTo(V("1.0.1")));
    }

    [Test]
    [Description("P2 — Con algo instalado, una semilla igual o más vieja no se toca")]
    public async Task SemillaMasVieja_NoSeToca()
    {
        _banco.Instalar("1.0.1");
        _banco.Semilla = new SemillaDePrueba(V("1.0.0"));

        var arranque = await Arrancar();

        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Almacen.Pasos, Does.Not.Contain(AlmacenEnMemoria.PasoStaging));
    }

    [Test]
    [Description("P1 — Con la misma instalación del cascarón, la semilla se carga sólo en el primer arranque (fuera del camino crítico del TPR)")]
    public async Task SemillaYaRevisada_NoSeCargaDeNuevo()
    {
        _banco.Semilla = new SemillaDePrueba(V("1.0.0"), null, Definiciones.De(1));

        await Arrancar();
        await _banco.Actualizador().ConfirmarListoAsync(V("1.0.0"));
        var segundo = await Arrancar();
        var tercero = await Arrancar();

        Assert.That(_banco.FuenteDeSemilla!.Cargas, Is.EqualTo(1), "sólo el primer arranque la lee y la verifica");
        Assert.That(tercero.Version, Is.EqualTo(V("1.0.0")));
        Assert.That(segundo.Notas, Has.Some.Contains("ya revisada"));
        Assert.That(_banco.Almacen.Puntero!.SemillaRevisada, Is.EqualTo("apk-1"));
    }

    [Test]
    [Description("P2 — Un APK nuevo (otra huella) vuelve a revisar la semilla y activa la más nueva")]
    public async Task OtraInstalacion_RevisaLaSemilla()
    {
        _banco.Semilla = new SemillaDePrueba(V("1.0.0"));
        await Arrancar();
        await _banco.Actualizador().ConfirmarListoAsync(V("1.0.0"));

        _banco.Semilla = new SemillaDePrueba(V("1.1.0"));   // otro APK…
        _banco.FuenteDeSemilla!.Huella = "apk-2";           // …instalado encima
        var arranque = await Arrancar();

        Assert.That(_banco.FuenteDeSemilla.Cargas, Is.EqualTo(1));
        Assert.That(arranque.Version, Is.EqualTo(V("1.1.0")));
        Assert.That(_banco.Almacen.Puntero!.SemillaRevisada, Is.EqualTo("apk-2"));
    }

    [Test]
    [Description("P2 — Si la semilla no se pudo instalar en el primer arranque, el siguiente la vuelve a intentar")]
    public async Task SemillaQueNoSeInstalo_SeReintenta()
    {
        var zip = Paquetes.Zip(V("1.0.0"));
        var buena = new SemillaDePrueba(V("1.0.0"), zip);
        var sha = buena.Sha256;
        var alterado = (byte[])zip.Clone();
        alterado[^3] ^= 0xFF;
        _banco.Semilla = new SemillaConShaFijo(new SemillaDePrueba(V("1.0.0"), alterado), sha);
        var primero = await Arrancar();

        _banco.Semilla = buena;
        var segundo = await Arrancar();

        Assert.That(primero.Estado, Is.EqualTo(EstadoDeArranque.SinContenidoSano));
        Assert.That(segundo.Version, Is.EqualTo(V("1.0.0")));
    }

    // ───────────── Retención ─────────────

    [Test]
    [Description("P2 — Retención del teléfono: current + previous; lo demás se borra al activar")]
    public async Task Retencion_CurrentMasPrevious()
    {
        _banco.Instalar("1.0.2", previous: "1.0.1");
        await PrepararYArrancar("1.0.3");

        Assert.That(_banco.Almacen.Versiones.Keys, Is.EquivalentTo(new[] { V("1.0.3"), V("1.0.2") }));
    }

    [Test]
    [Description("P3 — Un staging que dejó de ser compatible (el WebView bajó de versión) no se activa")]
    public async Task StagingIncompatibleAlArrancar_SeDescarta()
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.2"], manifiesto: v => Paquetes.Manifiesto(v, webView: 110));
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        _banco.Entorno.VersionMayorDelWebView = 100;

        var arranque = await Arrancar();

        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Almacen.Staging, Is.Empty);
    }

    // ───────────── Pantallas propias del cascarón ─────────────

    [Test]
    [Description("C-3 de E01 — WebView menor que 94: pantalla «actualizá el WebView»")]
    public async Task WebViewViejo_PantallaPropia()
    {
        _banco.Instalar("1.0.1");
        _banco.Entorno.VersionMayorDelWebView = 83;   // el del emulador fc-android-emu

        Assert.That((await Arrancar()).Estado, Is.EqualTo(EstadoDeArranque.MotorDesactualizado));
    }

    [Test]
    [Description("N4 / P2 — Cascarón bajo el mínimo soportado, con red: pantalla «actualizá la app»")]
    public async Task CascaronNoSoportado_ConRed()
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.1"], cascaronMinimo: "2.0.0");
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

        Assert.That((await Arrancar()).Estado, Is.EqualTo(EstadoDeArranque.CascaronNoSoportado));
    }

    [Test]
    [Description("P2 — Política de soporte: sin red no bloquea")]
    public async Task CascaronNoSoportado_SinRedNoBloquea()
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.1"], cascaronMinimo: "2.0.0");
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        _banco.Red.HayRed = false;

        var arranque = await Arrancar();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Notas, Has.Some.Contains("sin red no se bloquea"));
    }

    /// <summary>Una semilla cuyo zip cambió después de calcular el sha256 (como un APK manipulado).</summary>
    private sealed class SemillaConShaFijo(SemillaDePrueba interna, string sha) : ISemillaDeContenido
    {
        public VersionSemantica Version => interna.Version;
        public long Size => interna.Size;
        public string Sha256 => sha;
        public IReadOnlyList<Core.Contracts.Encuestas.DefinicionEncuesta> Definiciones => interna.Definiciones;
        public Task<Stream> AbrirAsync(CancellationToken cancellationToken = default) => interna.AbrirAsync(cancellationToken);
    }
}
