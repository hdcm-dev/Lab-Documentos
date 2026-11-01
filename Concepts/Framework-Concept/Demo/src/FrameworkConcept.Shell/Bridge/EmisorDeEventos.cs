using System.Text.Json;
using System.Text.Json.Serialization;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Shell.Hosting;

namespace FrameworkConcept.Shell.Bridge;

/// <summary>
/// P3 — Los eventos del cascarón al contenido (network.changed, outbox.changed, auth.changed, actions.changed): un
/// sobre JSON-RPC sin id («notification»).
///
/// Del lado del contenido, el cascarón llama a la función global <see cref="FuncionReceptora"/> con un único
/// argumento: el TEXTO del sobre. La define wwwroot/js/puente.js del contenido, con un nombre sin punto.
/// Se llama por <see cref="ScriptEnLaVista"/>, no con HybridWebView.InvokeJavaScriptAsync: en iOS esa API pasa por
/// «eval», que la CSP del contenido bloquea, y su tarea no terminaba nunca (ningún evento llegaba y la pantalla no se
/// bloqueaba al volver de segundo plano; simulador de iOS 26). El texto del sobre viaja como literal de cadena JSON
/// (JsonSerializer, con el codificador por defecto que escapa comillas, &lt;, &gt;, &amp; y todo lo no ASCII), nunca
/// concatenado a mano.
/// </summary>
public sealed class EmisorDeEventos
{
    public const string FuncionReceptora = "puenteRecibirEvento";

    /// <summary>Cuánto se espera a que la página ejecute el evento antes de dejarlo como error en la bitácora.</summary>
    private static readonly TimeSpan EsperaMaxima = TimeSpan.FromSeconds(5);

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

        // «(f(x), true)»: el valor de la expresión es un booleano, que WKWebView sabe devolver (el de la función es
        // undefined o una promesa).
        var script = $"(window.{FuncionReceptora}({JsonSerializer.Serialize(texto, JsonDelEmisor.Default.String)}), true)";

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // Con tope: un evento que no vuelve no deja una tarea colgada para siempre.
                var resultado = await ScriptEnLaVista.EvaluarAsync(vista, script).WaitAsync(EsperaMaxima);
                // Android no lanza si el receptor falla (evaluateJavascript devuelve "null"); iOS sí. El «, true» del
                // script deja ver el fallo en las dos plataformas.
                if (resultado != "true")
                    Bitacora.Log($"evento {evento}: la página no lo ejecutó (devolvió «{resultado ?? "null"}»)");
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
