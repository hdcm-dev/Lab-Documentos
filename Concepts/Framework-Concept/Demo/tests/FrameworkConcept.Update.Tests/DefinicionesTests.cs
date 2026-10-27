using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Update;
using static FrameworkConcept.Update.Tests.Banco;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// P1 y P13 — Las definiciones de encuesta en el canal: entrada propia del índice, misma firma, aceptación separada del
/// contenido, nunca bajan de versión, y ninguna regla del actualizador borra una definición con respuestas pendientes.
/// </summary>
[TestFixture]
public class DefinicionesTests
{
    private Banco _banco = null!;

    [SetUp]
    public void SetUp()
    {
        _banco = new Banco();
        _banco.Instalar("1.0.1");
        _banco.Definiciones.Guardadas.Add(Definiciones.De(1));
    }

    [TearDown]
    public void TearDown() => _banco.Dispose();

    private Task<ResultadoDeConsulta> Consultar() => _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);

    [Test]
    [Description("P1 — Corregir una encuesta publica sólo la definición: se guarda sin contenido nuevo; la v1 queda (P13.3)")]
    public async Task DefinicionNueva_SinContenidoNuevo()
    {
        _banco.Publicar(contenido: ["1.0.1"], definiciones: [Definiciones.De(1), Definiciones.De(2)]);

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.Preparado));
        Assert.That(resultado.ContenidoPreparado, Is.Null);
        Assert.That(resultado.DefinicionesNuevas, Is.EqualTo(new[] { new ReferenciaDeDefinicion("encuesta-movilidad", 2) }));
        Assert.That((await _banco.Definiciones.ObtenerVigenteAsync("encuesta-movilidad"))!.Version, Is.EqualTo(2), "la próxima encuesta usa la v2");
        Assert.That(await _banco.Definiciones.ObtenerAsync(new("encuesta-movilidad", 1)), Is.Not.Null, "la encuesta en curso termina con la v1");
    }

    [Test]
    [Description("P13.9 — El roll-forward nunca baja la versión de una definición")]
    public async Task DefinicionMasVieja_Rechazada()
    {
        _banco.Definiciones.Guardadas.Add(Definiciones.De(3));
        _banco.Publicar(definiciones: [Definiciones.De(2)]);

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.VersionNoMayor));
        Assert.That((await _banco.Definiciones.ObtenerVigenteAsync("encuesta-movilidad"))!.Version, Is.EqualTo(3));
    }

    [Test]
    [Description("P2 — La definición tiene su sha256 en el índice firmado: alterada, no se guarda")]
    public async Task DefinicionAlterada_NoSeGuarda()
    {
        var indice = _banco.Publicar(definiciones: [Definiciones.De(2)]);
        var ruta = RutasDelCanal.Archivo(Canales.Dev, 1, indice.Entradas.Definiciones[0].Ruta);
        var texto = Encoding.UTF8.GetString(_banco.Canal.Archivos[ruta]).Replace("Encuesta v2", "Encuesta vX");
        _banco.Canal.Archivos[ruta] = Encoding.UTF8.GetBytes(texto);

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.HashDistinto));
        Assert.That(_banco.Definiciones.Guardadas, Has.Count.EqualTo(1));
    }

    [Test]
    [Description("P1 — Una definición que pide un contenido más nuevo espera a que ese contenido esté activo")]
    public async Task DefinicionQuePideOtroContenido_Espera()
    {
        _banco.Publicar(contenido: ["2.0.0"], definiciones: [Definiciones.De(2, compatibleConContenido: ">=2.0.0 <3.0.0")]);

        var primera = await Consultar();
        await _banco.Actualizador().PrepararArranqueAsync();             // activa el contenido 2.0.0
        await _banco.Actualizador().ConfirmarListoAsync(V("2.0.0"));
        var segunda = await Consultar();

        Assert.That(primera.DefinicionesNuevas, Is.Empty);
        Assert.That(segunda.DefinicionesNuevas.Select(r => r.VersionDefinicion), Is.EqualTo(new[] { 2 }));
    }

    [Test]
    [Description("P1, P2 — Contenido y definición se aceptan por separado: un paquete rechazado no frena la definición")]
    public async Task AceptacionPorSeparado()
    {
        _banco.Publicar(contenido: ["1.0.2"], definiciones: [Definiciones.De(2)]);
        var paquete = _banco.PaquetePublicado("1.0.2");
        paquete[^5] ^= 0xFF;
        _banco.ReemplazarPaquete("1.0.2", paquete);

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.Preparado));
        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.HashDistinto));
        Assert.That(resultado.DefinicionesNuevas, Has.Count.EqualTo(1));
    }

    [Test]
    [Description("P13.5 — Una versión de definición ya retirada no se baja")]
    public async Task DefinicionRetirada_NoSeBaja()
    {
        var hoy = _banco.Reloj.Ahora;
        _banco.Publicar(definiciones: [Definiciones.De(2)],
            definicionesRetiradas: [new DefinicionRetirada("encuesta-movilidad", 2, hoy.AddDays(-31), hoy.AddDays(-1))]);

        Assert.That((await Consultar()).DefinicionesNuevas, Is.Empty);
    }

    [Test]
    [Description("P13.5 — Un retiro anunciado con menos de 30 días de aviso no se adelanta: rige al cumplirse el plazo")]
    public void RetiroSinPlazoDeAviso_RigeAlCumplirse()
    {
        var anunciada = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var retiro = new DefinicionRetirada("encuesta-movilidad", 1, anunciada, anunciada.AddDays(5));
        var indice = IndiceCon(retiro);
        var referencia = new ReferenciaDeDefinicion("encuesta-movilidad", 1);

        Assert.That(PoliticaDeDefiniciones.RigeDesde(retiro), Is.EqualTo(anunciada.AddDays(30)));
        Assert.That(PoliticaDeDefiniciones.EstaRetirada(referencia, indice, anunciada.AddDays(10)), Is.False);
        Assert.That(PoliticaDeDefiniciones.EstaRetirada(referencia, indice, anunciada.AddDays(30)), Is.True);
    }

    [Test]
    [Description("P13.3 — Se conserva toda definición con respuestas en curso o pendientes, más la vigente de cada encuesta")]
    public void Conservables_IncluyeLasDePendientes()
    {
        ReferenciaDeDefinicion R(int v, string id = "encuesta-movilidad") => new(id, v);
        var guardadas = new[] { R(1), R(2), R(3), R(4), R(1, "otra") };
        var conPendientes = new[] { R(1), R(3) };

        var conservar = PoliticaDeDefiniciones.Conservables(guardadas, conPendientes);

        Assert.That(conservar, Is.EquivalentTo(new[] { R(1), R(3), R(4), R(1, "otra") }));
    }

    [Test]
    [Description("P13.3, P13.7 — Ni activar contenido nuevo ni el rollback borran definiciones guardadas")]
    public async Task ActivarYRollback_NoTocanDefiniciones()
    {
        _banco.Publicar(contenido: ["1.0.2"], definiciones: [Definiciones.De(2)]);
        await Consultar();

        // Activa 1.0.2, que carga sin decir Listo hasta el rollback.
        for (var i = 0; i <= Actualizador.CargasSinListoParaFallar; i++)
            await _banco.ArrancarYCargarSinListo();

        Assert.That(_banco.Definiciones.Guardadas.Select(d => d.Version), Is.EquivalentTo(new[] { 1, 2 }));
    }

    private static IndiceDelCanal IndiceCon(DefinicionRetirada retiro) =>
        new(1, Canales.Dev, 1, KeyIdActiva, retiro.AnunciadaEl, retiro.AnunciadaEl.AddDays(30),
            new EntradasDelIndice([], []), [], [retiro]);
}
