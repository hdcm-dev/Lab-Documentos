using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Update;
using static FrameworkConcept.Update.Tests.Banco;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// La consulta al canal: los casos de N4 §3.6 y C02–C08 de E-SEG-1 (folio 016), con el orden de verificación de P2.
/// Cada caso deja el teléfono como estaba: nada rechazado llega a staging/ ni cambia el puntero.
/// </summary>
[TestFixture]
public class ConsultaTests
{
    private Banco _banco = null!;

    [SetUp]
    public void SetUp()
    {
        _banco = new Banco();
        _banco.Instalar("1.0.1");
    }

    [TearDown]
    public void TearDown() => _banco.Dispose();

    private Task<ResultadoDeConsulta> Consultar(DisparadorDeConsulta disparador = DisparadorDeConsulta.ArranqueEnFrio) =>
        _banco.Actualizador().ConsultarAsync(disparador);

    private void NadaCambio()
    {
        Assert.That(_banco.Almacen.Staging, Is.Empty, "staging/ tiene que quedar vacío");
        Assert.That(_banco.Almacen.Puntero!.Current, Is.EqualTo(V("1.0.1")), "sigue la instalada");
    }

    // ───────────── N4 §3.6 ─────────────

    [Test]
    [Description("N4 — Sin red: la consulta no fuerza nada y el arranque usa lo instalado sin tocar la red (N = 0)")]
    public async Task SinRed_UsaLoInstalado()
    {
        _banco.Canal.SinRed = true;

        var consulta = await Consultar();
        var llamadasAntes = _banco.Canal.Llamadas;
        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        Assert.That(consulta.Estado, Is.EqualTo(EstadoDeConsulta.SinRed));
        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Canal.Llamadas, Is.EqualTo(llamadasAntes), "el arranque no usa la red");
    }

    [Test]
    [Description("N4 — Sin cambios: el índice ofrece la versión instalada y no se descarga nada")]
    public async Task SinCambios_NoDescarga()
    {
        _banco.Publicar(contenido: ["1.0.1"]);

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.AlDia));
        Assert.That(_banco.Canal.DescargasDePaquetes, Is.Zero);
        NadaCambio();
    }

    [Test]
    [Description("N4 — Actualización válida: queda en staging y se activa en el próximo arranque en frío; previous se conserva")]
    public async Task ActualizacionValida_SeActivaEnElProximoArranque()
    {
        _banco.Publicar(contenido: ["1.0.1", "1.0.2+abc1234"]);

        var consulta = await Consultar();

        Assert.That(consulta.Estado, Is.EqualTo(EstadoDeConsulta.Preparado));
        Assert.That(consulta.ContenidoPreparado, Is.EqualTo(V("1.0.2+abc1234")));
        Assert.That(_banco.Almacen.Puntero!.Current, Is.EqualTo(V("1.0.1")), "bajar no es activar: no hay activación en caliente");

        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.2+abc1234")));
        Assert.That(_banco.Almacen.Puntero, Is.EqualTo(new PunteroDeContenido(V("1.0.2+abc1234"), V("1.0.1"), V("1.0.2+abc1234"), _banco.Reloj.Ahora)));
        Assert.That(_banco.Almacen.ExisteVersion(V("1.0.1")), Is.True);
    }

    [Test]
    [Description("N4 — Descarga cortada (respuesta truncada): staging vacío, sigue la instalada")]
    public async Task DescargaTruncada_StagingLimpio()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        _banco.Canal.TruncarEn = 100;

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.Rechazado));
        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.TamanoDistinto));
        NadaCambio();
    }

    [Test]
    [Description("N4 — Descarga cortada (la conexión se cae a mitad): staging vacío, sigue la instalada")]
    public async Task ConexionCortada_StagingLimpio()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        _banco.Canal.CortarConexionEn = 100;

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.SinRed));
        NadaCambio();
    }

    [Test]
    [Description("N4 — Un staging viejo de otra versión se descarta antes de bajar la nueva")]
    public async Task StagingViejo_SeReemplaza()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        await Consultar();
        _banco.Publicar(contenido: ["1.0.3"]);

        var resultado = await Consultar();

        Assert.That(resultado.ContenidoPreparado, Is.EqualTo(V("1.0.3")));
        Assert.That(_banco.Almacen.Staging.Keys, Is.EquivalentTo(new[] { V("1.0.3") }));
    }

    [Test]
    [Description("Lo ya preparado no se vuelve a bajar")]
    public async Task YaPreparada_NoSeBajaDeNuevo()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        await Consultar();

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.Preparado));
        Assert.That(_banco.Canal.DescargasDePaquetes, Is.EqualTo(1));
    }

    // ───────────── C02–C08 de E-SEG-1 ─────────────

    [Test]
    [Description("C02 — Índice alterado después de firmar: firma inválida")]
    public async Task C02_IndiceAlterado()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        var texto = Encoding.UTF8.GetString(_banco.IndicePublicado).Replace("1.0.2", "1.0.9");
        _banco.IndicePublicado = Encoding.UTF8.GetBytes(texto);

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
        Assert.That(_banco.Canal.DescargasDePaquetes, Is.Zero);
        NadaCambio();
    }

    [Test]
    [Description("C03 — Firma alterada: firma inválida")]
    public async Task C03_FirmaAlterada()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        var firma = _banco.FirmaPublicada;
        var bytes = Convert.FromBase64String(firma.Firma);
        bytes[10] ^= 0xFF;
        _banco.FirmaPublicada = firma with { Firma = Convert.ToBase64String(bytes) };

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
        NadaCambio();
    }

    [Test]
    [Description("C04 — Firmado con otra clave que dice ser la activa: firma inválida")]
    public async Task C04_OtraClave()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice, _banco.ClaveAjena);

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
        NadaCambio();
    }

    [Test]
    [Description("C05 — Paquete alterado después de firmar el índice: sha256 distinto, no llega a staging")]
    public async Task C05_PaqueteAlterado()
    {
        _banco.Publicar(contenido: ["1.0.2"]);
        var paquete = _banco.PaquetePublicado("1.0.2");
        paquete[^5] ^= 0xFF;
        _banco.ReemplazarPaquete("1.0.2", paquete);

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.HashDistinto));
        Assert.That(_banco.Almacen.Pasos, Does.Not.Contain(AlmacenEnMemoria.PasoStaging), "no se descomprime lo que no coincide");
        NadaCambio();
    }

    [Test]
    [Description("C06 — Versión vieja con firma válida (rollback attack): rechazada")]
    public async Task C06_VersionVieja()
    {
        _banco.Publicar(contenido: ["1.0.0"]);

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.Rechazado));
        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.VersionNoMayor));
        Assert.That(_banco.Canal.DescargasDePaquetes, Is.Zero);
        NadaCambio();
    }

    [Test]
    [Description("C07 — Misma versión otra vez (replay), aunque con otro +commit: no se reinstala")]
    public async Task C07_MismaVersion()
    {
        _banco.Publicar(contenido: ["1.0.1+otrocommit"]);

        var resultado = await Consultar();

        Assert.That(resultado.Estado, Is.EqualTo(EstadoDeConsulta.AlDia));
        Assert.That(resultado.ContenidoPreparado, Is.Null);
        Assert.That(_banco.Canal.DescargasDePaquetes, Is.Zero);
        NadaCambio();
    }

    [Test]
    [Description("C08 — Índice vencido (freeze attack): rechazado aunque la firma sea buena")]
    public async Task C08_IndiceVencido()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { Expires = _banco.Reloj.Ahora.AddSeconds(-1) });

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.IndiceVencido));
        NadaCambio();
    }

    [Test]
    [Description("expires es exclusivo: en el instante exacto ya no vale")]
    public async Task IndiceVenceEnElInstanteExacto()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.Reloj.Ahora = indice.Expires;

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.IndiceVencido));
    }

    // ───────────── Claves (P2) ─────────────

    [Test]
    [Description("P2 — Clave revocada: rechazada aunque la firma cierre")]
    public async Task ClaveRevocada()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { KeyId = KeyIdRevocada }, _banco.ClaveRevocada);

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.ClaveRevocada));
        NadaCambio();
    }

    [Test]
    [Description("P2 — keyId que el cascarón no conoce")]
    public async Task ClaveDesconocida()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { KeyId = "quien-sabe" }, _banco.ClaveAjena);

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.ClaveDesconocida));
    }

    [Test]
    [Description("P2 — La reserva, no revocada, también se acepta (así se rota sin tienda)")]
    public async Task ClaveDeReserva_Aceptada()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { KeyId = KeyIdReserva }, _banco.ClaveReserva);

        Assert.That((await Consultar()).Estado, Is.EqualTo(EstadoDeConsulta.Preparado));
    }

    [Test]
    [Description("P2 — El keyId de adentro del índice tiene que ser el de la firma")]
    public async Task KeyIdDeAdentroDistinto()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { KeyId = KeyIdReserva }, _banco.ClaveActiva, keyIdDeLaFirma: KeyIdActiva);

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
    }

    // ───────────── Canal y mayor ─────────────

    [Test]
    [Description("P2 — Un índice de otro canal (beta servido como dev) se rechaza")]
    public async Task OtroCanal()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { Canal = Canales.Beta });

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.OtroCanal));
    }

    [Test]
    [Description("P2 — Un índice de otro mayor del puente se rechaza")]
    public async Task OtroMayorDelPuente()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { PuenteMayor = 2 });

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.OtroMayorDelPuente));
    }

    [Test]
    [Description("Un formato de índice que este cascarón no conoce")]
    public async Task FormatoDesconocido()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { Formato = 2 });

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.FormatoDesconocido));
    }

    [Test]
    [Description("P2 — Orden: la firma va antes que expires (un índice vencido Y mal firmado dice «firma»)")]
    public async Task Orden_FirmaAntesQueExpires()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { Expires = _banco.Reloj.Ahora.AddDays(-1) }, _banco.ClaveAjena);

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.FirmaInvalida));
    }

    [Test]
    [Description("P2 — Orden: expires va antes que el canal")]
    public async Task Orden_ExpiresAntesQueElCanal()
    {
        var indice = _banco.Publicar(contenido: ["1.0.2"]);
        _banco.PublicarIndice(indice with { Expires = _banco.Reloj.Ahora.AddDays(-1), Canal = Canales.Prod });

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.IndiceVencido));
    }

    // ───────────── Versión y paquete ─────────────

    [Test]
    [Description("P7 — El contenido no lleva pre-release en ningún canal")]
    public async Task PreRelease_Rechazada()
    {
        _banco.Publicar(contenido: ["1.1.0-beta.1"]);

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.ContenidoConPreRelease));
        NadaCambio();
    }

    [Test]
    [Description("P2 — Una versión retirada en el índice no se baja; se toma la mayor no retirada")]
    public async Task Retirada_NoSeBaja()
    {
        _banco.Publicar(contenido: ["1.0.2", "1.0.3"], retiradas: ["1.0.3"]);

        var resultado = await Consultar();

        Assert.That(resultado.ContenidoPreparado, Is.EqualTo(V("1.0.2")));
    }

    [Test]
    [Description("P3 — Un paquete incompatible no queda en staging")]
    public async Task Incompatible_NoQuedaEnStaging()
    {
        _banco.Publicar(contenido: ["1.0.2"], manifiesto: v => Paquetes.Manifiesto(v, puente: ">=2.0.0 <3.0.0"));

        var resultado = await Consultar();

        Assert.That(resultado.Motivo, Is.EqualTo(MotivoDeRechazo.Incompatible));
        Assert.That(resultado.Detalle, Does.Contain("puente 1.0.0 fuera de >=2.0.0 <3.0.0"));
        NadaCambio();
    }

    [Test]
    [Description("Un zip cuyo manifiesto dice otra versión que el índice se descarta")]
    public async Task ManifiestoDeOtraVersion()
    {
        _banco.Publicar(contenido: ["1.0.2"], manifiesto: _ => Paquetes.Manifiesto(V("1.0.9")));

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.PaqueteInvalido));
        NadaCambio();
    }

    [Test]
    [Description("Bytes que no son un zip, aunque el índice los firme, se descartan")]
    public async Task NoEsUnZip()
    {
        var basura = Encoding.UTF8.GetBytes("esto no es un zip");
        _banco.Publicar(contenido: ["1.0.2"]);
        _banco.ReemplazarPaquete("1.0.2", basura);
        var indice = JsonDelContrato.Leer<IndiceDelCanal>(Encoding.UTF8.GetString(_banco.IndicePublicado));
        var entrada = indice.Entradas.Contenido[0] with { Size = basura.Length, Sha256 = Sha(basura) };
        _banco.PublicarIndice(indice with { Entradas = indice.Entradas with { Contenido = [entrada] } });

        Assert.That((await Consultar()).Motivo, Is.EqualTo(MotivoDeRechazo.PaqueteInvalido));
        NadaCambio();
    }

    // ───────────── Momento (P2) ─────────────

    [Test]
    [Description("P2 — Al reanudar, no se consulta si pasaron menos de 15 minutos desde la última consulta exitosa")]
    public async Task Reanudacion_RespetaLos15Minutos()
    {
        _banco.Publicar(contenido: ["1.0.1"]);
        await Consultar();

        _banco.Reloj.Ahora = _banco.Reloj.Ahora.AddMinutes(14);
        var antes = await Consultar(DisparadorDeConsulta.Reanudacion);
        _banco.Reloj.Ahora = _banco.Reloj.Ahora.AddMinutes(1);
        var despues = await Consultar(DisparadorDeConsulta.Reanudacion);

        Assert.That(antes.Estado, Is.EqualTo(EstadoDeConsulta.Omitida));
        Assert.That(despues.Estado, Is.EqualTo(EstadoDeConsulta.AlDia));
    }

    [Test]
    [Description("P2 — Una consulta fallida no cuenta: al reanudar se vuelve a intentar")]
    public async Task Reanudacion_TrasUnaConsultaSinRed_Consulta()
    {
        _banco.Publicar(contenido: ["1.0.1"]);
        await Consultar();                                         // exitosa a las 12:00

        _banco.Reloj.Ahora = _banco.Reloj.Ahora.AddMinutes(16);
        _banco.Canal.SinRed = true;
        var sinRed = await Consultar(DisparadorDeConsulta.Reanudacion);   // 12:16, falla

        _banco.Reloj.Ahora = _banco.Reloj.Ahora.AddMinutes(1);
        _banco.Canal.SinRed = false;
        var otraVez = await Consultar(DisparadorDeConsulta.Reanudacion);  // 12:17: la fallida no reinició la cuenta

        Assert.That(sinRed.Estado, Is.EqualTo(EstadoDeConsulta.SinRed));
        Assert.That(otraVez.Estado, Is.EqualTo(EstadoDeConsulta.AlDia));
    }

    [Test]
    [Description("P2 — Obligatoria: misma regla, se activa en el primer arranque en frío desde que quedó verificada")]
    public async Task Obligatoria_UnArranqueEnFrio()
    {
        _banco.Publicar(contenido: ["1.0.2"], mandatory: true);
        await Consultar();

        Assert.That(_banco.Almacen.Puntero!.Current, Is.EqualTo(V("1.0.1")), "ni la obligatoria se activa en caliente");
        Assert.That((await _banco.Actualizador().PrepararArranqueAsync()).Version, Is.EqualTo(V("1.0.2")));
    }
}
