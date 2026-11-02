// Nuevo (2026-10-29, REC-09; patrón: la prueba de cancelación del ejemplo de streaming del insumo).
// Pruebas de cableado sin servidor: el guionado de --fake, debajo de la MISMA capa, con un cliente que
// corta el streaming cuando se cancela, de las dos maneras que tienen los proveedores.
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

/// <summary>
/// Pasa los fragmentos del cliente de adentro y, si el token ya está cancelado antes del siguiente,
/// lanza <see cref="OperationCanceledException" /> o, con <c>cortaEnSilencio</c>, termina la enumeración sin lanzar.
/// </summary>
internal sealed class ClienteQueCortaAlCancelar(IChatClient interno, bool cortaEnSilencio) : DelegatingChatClient(interno)
{
    public int FragmentosEmitidos { get; private set; }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Al de adentro no se le pasa el token: el corte lo decide este cliente, como el proveedor real.
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, CancellationToken.None))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (cortaEnSilencio) yield break;
                cancellationToken.ThrowIfCancellationRequested();
            }
            FragmentosEmitidos++;
            yield return update;
        }
    }
}

public class CancelacionTests
{
    private string _dir = string.Empty;

    [SetUp]
    public void CrearDirectorio() =>
        _dir = Directory.CreateTempSubdirectory("ej01-cancel-").FullName;

    [TearDown]
    public void BorrarDirectorio() => Directory.Delete(_dir, recursive: true);

    [TestCase(false, TestName = "Cancelar a mitad (el proveedor lanza OperationCanceledException)")]
    [TestCase(true, TestName = "Cancelar a mitad (el proveedor termina la enumeración sin lanzar)")]
    public async Task CancelarAMitad_DevuelveLaVueltaCancelada_YNoTocaElHistorial(bool cortaEnSilencio)
    {
        var guion = Corrida.CrearGuion();
        var cortador = new ClienteQueCortaAlCancelar(guion, cortaEnSilencio);
        var asistente = new AsistenteDeAyuda(FabricaDeClientes.Envolver(cortador, new ConfiguracionDelModelo { RegistroDir = _dir }, Adaptador.OllamaSharp));

        var primera = await asistente.PreguntarAsync("¿Qué pide el alta?", _ => { });
        Assert.That(primera.Cancelada, Is.False);
        Assert.That(asistente.Historial, Has.Count.EqualTo(2));

        var emitidosAntes = cortador.FragmentosEmitidos;
        using var cts = new CancellationTokenSource();
        var vistos = new List<string>();
        var cortada = await asistente.PreguntarAsync("¿Y la encuesta?", f => { vistos.Add(f); cts.Cancel(); }, cts.Token);

        Assert.That(cortada.Cancelada, Is.True);
        Assert.That(vistos, Is.EqualTo(new[] { "La encuesta pide " }), "sólo el fragmento anterior al corte");
        Assert.That(cortada.Respuesta.Text, Is.EqualTo("La encuesta pide "), "el parcial se devuelve");
        Assert.That(cortador.FragmentosEmitidos - emitidosAntes, Is.EqualTo(1), "el proveedor cortó después del primero");
        Assert.That(asistente.Historial, Has.Count.EqualTo(2), "ni la pregunta ni el parcial entran al historial");
        Assert.That(asistente.Historial.Select(m => m.Text), Has.None.Contains("La encuesta pide"));

        // La vuelta siguiente reenvía sólo la conversación completa, sin el parcial.
        await asistente.PreguntarAsync("¿Y la encuesta?", _ => { });
        var recibidos = guion.Llamadas[^1].Mensajes;
        Assert.That(recibidos.Select(m => m.Role), Is.EqualTo(new[] { ChatRole.User, ChatRole.Assistant, ChatRole.User }));
        Assert.That(recibidos[1].Text, Is.EqualTo(primera.Respuesta.Text));
    }
}
