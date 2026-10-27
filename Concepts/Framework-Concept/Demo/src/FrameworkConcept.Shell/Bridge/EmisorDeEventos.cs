using System.Text.Json;
using System.Text.Json.Serialization;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Shell.Bridge;

/// <summary>
/// P3 — Los eventos del cascarón al contenido (network.changed, outbox.changed): un sobre JSON-RPC sin id
/// («notification»), enviado con <c>HybridWebView.InvokeJavaScriptAsync</c>, que es el transporte del contrato.
///
/// Del lado del contenido, el cascarón llama a la función global <see cref="FuncionReceptora"/> con un único
/// argumento: el TEXTO del sobre. La define wwwroot/js/puente.js del contenido, con un nombre sin punto.
/// La función no devuelve nada: por eso se usa la sobrecarga de InvokeJavaScriptAsync sin tipo de retorno.
/// </summary>
public sealed class EmisorDeEventos
{
    public const string FuncionReceptora = "puenteRecibirEvento";

    private HybridWebView? _vista;

    /// <summary>Se habilita cuando el contenido hizo el handshake: antes, la función receptora puede no existir.</summary>
    public bool Habilitado { get; set; }

    public void Conectar(HybridWebView vista) => _vista = vista;

    public void Emitir(string evento, object parametros)
    {
        if (_vista is not { } vista || !Habilitado) return;

        var sobre = new SobreEvento(SobrePedido.VersionJsonRpc, ContratoDelPuente.Version.ToString(), evento,
            JsonSerializer.SerializeToElement(parametros, JsonDelContrato.Opciones));
        var texto = JsonDelContrato.Escribir(sobre);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await vista.InvokeJavaScriptAsync(FuncionReceptora, [texto], [JsonDelEmisor.Default.String]);
            }
            catch (Exception ex)
            {
                Bitacora.Error($"evento {evento}", ex);
            }
        });
    }
}

/// <summary>Tipos JSON que el transporte de eventos necesita, generados en compilación (sin reflexión).</summary>
[JsonSerializable(typeof(string))]
internal sealed partial class JsonDelEmisor : JsonSerializerContext;
