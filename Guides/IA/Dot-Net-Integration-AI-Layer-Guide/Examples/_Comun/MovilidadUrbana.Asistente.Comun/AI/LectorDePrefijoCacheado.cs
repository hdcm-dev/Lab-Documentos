// Nuevo (REC-22; procedencia en un registro de trabajo no publicado; ver el Anexo H, PR-01). Portado de la línea archivada
// del 2026-10-28 y adaptado al espacio de nombres de _Comun. OllamaSharp 5.5.0 no expone
// prompt_eval_cached_count (Usage.CachedInputTokenCount queda null); este handler lo lee de la línea
// final del NDJSON de /api/chat. M-I: no persiste nada. Mira cada línea en memoria, descarta el texto
// apenas la procesó y sólo conserva dos contadores; el registro R-LLM recibe esos contadores, nunca
// el cuerpo.
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace MovilidadUrbana.Asistente.Comun.AI;

/// <summary>
/// Lo que informó el servidor en la última línea de <c>/api/chat</c>: sólo los dos contadores.
/// </summary>
public sealed class LecturaDelServidor
{
    public int? TokensDeEntrada { get; internal set; }
    public int? TokensCacheados { get; internal set; }
}

/// <summary>
/// <c>DelegatingHandler</c> del <c>HttpClient</c> del proveedor. Deja pasar la respuesta NDJSON de
/// <c>/api/chat</c> tal cual (también en streaming) y, al ver la línea final (<c>"done": true</c>), copia
/// <c>prompt_eval_count</c> y <c>prompt_eval_cached_count</c> en la <see cref="LecturaDelServidor"/> del flujo
/// actual. Quien quiere la lectura la abre con <see cref="Abrir"/> antes de llamar al cliente de chat
/// (lo hace <see cref="LecturaDelServidorEnLaRespuesta"/> en cada llamada). Sin lectura abierta, no mira nada.
/// </summary>
public sealed class LectorDePrefijoCacheado : DelegatingHandler
{
    private static readonly AsyncLocal<LecturaDelServidor?> Actual = new();

    /// <summary>Abre una lectura para los pedidos que siguen en este flujo asíncrono.</summary>
    public static LecturaDelServidor Abrir() => Actual.Value = new LecturaDelServidor();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var respuesta = await base.SendAsync(request, cancellationToken);
        if (Actual.Value is not { } lectura
            || request.RequestUri?.AbsolutePath.EndsWith("/api/chat", StringComparison.Ordinal) != true
            || !respuesta.IsSuccessStatusCode)
            return respuesta;

        var original = respuesta.Content;
        var flujo = new FlujoQueLeeElRenglonFinal(await original.ReadAsStreamAsync(cancellationToken), lectura);
        var nuevo = new StreamContent(flujo);
        foreach (var encabezado in original.Headers) nuevo.Headers.TryAddWithoutValidation(encabezado.Key, encabezado.Value);
        respuesta.Content = nuevo;
        return respuesta;
    }

    /// <summary>Pasa los bytes sin tocarlos y mira cada línea completa.</summary>
    private sealed class FlujoQueLeeElRenglonFinal(Stream interno, LecturaDelServidor lectura) : Stream
    {
        private readonly MemoryStream _linea = new();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int n = await interno.ReadAsync(buffer, cancellationToken);
            Mirar(buffer.Span[..n], fin: n == 0);
            return n;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = interno.Read(buffer, offset, count);
            Mirar(buffer.AsSpan(offset, n), fin: n == 0);
            return n;
        }

        private void Mirar(ReadOnlySpan<byte> bytes, bool fin)
        {
            foreach (var b in bytes)
            {
                if (b == (byte)'\n') { Procesar(); continue; }
                _linea.WriteByte(b);
            }
            if (fin) Procesar();
        }

        private void Procesar()
        {
            if (_linea.Length == 0) return;
            var texto = Encoding.UTF8.GetString(_linea.GetBuffer(), 0, (int)_linea.Length);
            _linea.SetLength(0);
            if (!texto.Contains("\"done\"", StringComparison.Ordinal)) return;
            try
            {
                if (JsonNode.Parse(texto) is JsonObject linea && linea["done"]?.GetValue<bool>() == true)
                {
                    lectura.TokensDeEntrada = (int?)linea["prompt_eval_count"];
                    lectura.TokensCacheados = (int?)linea["prompt_eval_cached_count"];
                }
            }
            catch (System.Text.Json.JsonException) { /* una línea que no es JSON no es la final */ }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) { interno.Dispose(); _linea.Dispose(); } base.Dispose(disposing); }
    }
}

/// <summary>
/// Abre una <see cref="LecturaDelServidor"/> por llamada y, si el servidor informó
/// <c>prompt_eval_cached_count</c>, lo deja en <c>ChatResponse.AdditionalProperties</c> con la clave
/// <see cref="Clave"/>: así lo encuentra el escritor del registro R-LLM sin que cada ejemplo abra la
/// lectura a mano. Si la respuesta ya trae el valor (el simulador del modo --fake de 05), no lo pisa.
/// En streaming deja pasar los fragmentos sin cambios y no agrega nada: quien necesite el valor en
/// streaming abre la lectura con <see cref="LectorDePrefijoCacheado.Abrir"/>.
/// </summary>
public sealed class LecturaDelServidorEnLaRespuesta(IChatClient interno) : DelegatingChatClient(interno)
{
    public const string Clave = "prompt_eval_cached_count";

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lectura = LectorDePrefijoCacheado.Abrir();
        var respuesta = await base.GetResponseAsync(messages, options, cancellationToken);
        if (lectura.TokensCacheados is int cacheados && respuesta.AdditionalProperties?.ContainsKey(Clave) != true)
        {
            (respuesta.AdditionalProperties ??= new AdditionalPropertiesDictionary())[Clave] = (long)cacheados;
        }
        return respuesta;
    }

    /// <summary>El valor observado de una respuesta, o null si el servidor no lo informó.</summary>
    public static long? CacheadosDe(ChatResponse respuesta) =>
        respuesta.AdditionalProperties?.TryGetValue(Clave, out var valor) == true && valor is not null
            ? Convert.ToInt64(valor, System.Globalization.CultureInfo.InvariantCulture)
            : null;
}
