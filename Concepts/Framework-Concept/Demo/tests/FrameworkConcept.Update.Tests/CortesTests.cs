using FrameworkConcept.Core.Update;
using static FrameworkConcept.Update.Tests.Banco;

namespace FrameworkConcept.Update.Tests;

/// <summary>
/// N4 «corte durante la activación» y P8 «corte provocado en cada paso de la activación»: el proceso muere justo antes
/// de cada operación que cambia el estado, y el arranque siguiente (un actualizador nuevo sobre el mismo almacén) tiene
/// que encontrar el puntero apuntando a una versión COMPLETA.
/// </summary>
[TestFixture]
public class CortesTests
{
    private Banco _banco = null!;

    [SetUp]
    public void SetUp() => _banco = new Banco();

    [TearDown]
    public void TearDown() => _banco.Dispose();

    /// <summary>El invariante: el arranque muestra una versión que existe entera y es la del puntero.</summary>
    private void VersionCompleta(ResultadoDeArranque arranque)
    {
        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo), string.Join(" | ", arranque.Notas));
        Assert.That(_banco.Almacen.ExisteVersion(arranque.Version!), Is.True);
        Assert.That(_banco.Almacen.Puntero!.Current, Is.EqualTo(arranque.Version));
        Assert.That(_banco.Almacen.Versiones[arranque.Version!], Does.ContainKey("manifiesto.json"));
    }

    // Activar 1.0.2 sobre current 1.0.1 y previous 1.0.0 hace, en orden: promover, puntero, borrar (1.0.0, retención).
    [TestCase(AlmacenEnMemoria.PasoPromover, 1, "1.0.2", TestName = "Corte antes de promover: el staging sigue y se activa al rearrancar")]
    [TestCase(AlmacenEnMemoria.PasoPuntero, 1, "1.0.1", TestName = "Corte entre promover y el puntero: sigue la vieja; la huérfana se borra")]
    [TestCase(AlmacenEnMemoria.PasoBorrar, 1, "1.0.2", TestName = "Corte entre el puntero y la retención: la nueva no llegó a cargar, se reintenta")]
    public async Task CorteEnLaActivacion(string paso, int aparicion, string esperada)
    {
        _banco.Instalar("1.0.1", previous: "1.0.0");
        _banco.Publicar(contenido: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        _banco.Almacen.Pasos.Clear();
        _banco.Almacen.CortarEn = (paso, aparicion);

        Assert.ThrowsAsync<CorteProvocadoException>(() => _banco.Actualizador().PrepararArranqueAsync());
        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        VersionCompleta(arranque);
        Assert.That(arranque.Version, Is.EqualTo(V(esperada)));
        Assert.That(_banco.Almacen.Versiones.Keys, Has.Count.LessThanOrEqualTo(ParametrosDeLaPrueba.Retenidas));
    }

    [Test]
    [Description("Después del corte entre promover y el puntero, la versión se vuelve a bajar y se activa")]
    public async Task CorteEntrePromoverYPuntero_SeRecupera()
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        _banco.Almacen.Pasos.Clear();
        _banco.Almacen.CortarEn = (AlmacenEnMemoria.PasoPuntero, 1);
        Assert.ThrowsAsync<CorteProvocadoException>(() => _banco.Actualizador().PrepararArranqueAsync());
        await _banco.Actualizador().PrepararArranqueAsync();

        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        VersionCompleta(arranque);
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.2")));
    }

    // El rollback de 1.0.2 (pendiente) a 1.0.1 hace: fallida, puntero (borra la marca), puntero (a previous), borrar 1.0.2.
    [TestCase(AlmacenEnMemoria.PasoFallida, 1, TestName = "Corte antes de anotar la fallida")]
    [TestCase(AlmacenEnMemoria.PasoPuntero, 1, TestName = "Corte entre la fallida y borrar la marca")]
    [TestCase(AlmacenEnMemoria.PasoPuntero, 2, TestName = "Corte entre borrar la marca y volver a previous")]
    public async Task CorteEnElRollback(string paso, int aparicion)
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        // Activa 1.0.2, que empieza a cargar y no va a decir Listo, hasta que el arranque siguiente decida el rollback.
        for (var i = 0; i < Actualizador.CargasSinListoParaFallar; i++)
            await _banco.ArrancarYCargarSinListo();
        _banco.Almacen.Pasos.Clear();
        _banco.Almacen.CortarEn = (paso, aparicion);

        Assert.ThrowsAsync<CorteProvocadoException>(() => _banco.Actualizador().PrepararArranqueAsync());
        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        VersionCompleta(arranque);
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Almacen.Fallidas, Does.Contain(V("1.0.2")), "la mala queda anotada: no se reinstala en bucle");
    }

    [Test]
    [Description("Corte después de volver a previous y antes de cargarla: NO cuenta como falla; previous se reintenta (P2)")]
    public async Task CorteTrasVolverAPrevious_SeReintenta()
    {
        // Volver a previous es activarla, con su marca. Un corte antes de que el WebView empiece a cargarla no prueba
        // que previous falle: el arranque siguiente la vuelve a intentar (antes de la corrección de E02 esto terminaba
        // en el estado terminal, «doble falla», con previous sana anotada como fallida).
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.2"]);
        await _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio);
        for (var i = 0; i < Actualizador.CargasSinListoParaFallar; i++)
            await _banco.ArrancarYCargarSinListo();
        _banco.Almacen.Pasos.Clear();
        _banco.Almacen.CortarEn = (AlmacenEnMemoria.PasoBorrar, 1);

        Assert.ThrowsAsync<CorteProvocadoException>(() => _banco.Actualizador().PrepararArranqueAsync());
        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        Assert.That(arranque.Estado, Is.EqualTo(EstadoDeArranque.ContenidoListo));
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
        Assert.That(_banco.Almacen.Fallidas, Is.EquivalentTo(new[] { V("1.0.2") }));
    }

    [Test]
    [Description("Corte al descomprimir en staging durante la consulta: lo instalado no se entera")]
    public async Task CorteAlDescomprimir()
    {
        _banco.Instalar("1.0.1");
        _banco.Publicar(contenido: ["1.0.2"]);
        _banco.Almacen.CortarEn = (AlmacenEnMemoria.PasoStaging, 1);

        Assert.ThrowsAsync<CorteProvocadoException>(() => _banco.Actualizador().ConsultarAsync(DisparadorDeConsulta.ArranqueEnFrio));
        var arranque = await _banco.Actualizador().PrepararArranqueAsync();

        VersionCompleta(arranque);
        Assert.That(arranque.Version, Is.EqualTo(V("1.0.1")));
    }

    private static class ParametrosDeLaPrueba
    {
        public const int Retenidas = Core.Contracts.ParametrosDeProducto.VersionesRetenidasEnElTelefono;
    }
}
