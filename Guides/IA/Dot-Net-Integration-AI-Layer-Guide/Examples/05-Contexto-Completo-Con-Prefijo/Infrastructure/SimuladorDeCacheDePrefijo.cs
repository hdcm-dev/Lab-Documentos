// Nuevo. SÓLO para el modo --fake y las pruebas: imita, de forma muy simplificada, la caché del
// prefijo de un servidor tipo Ollama con N ranuras (OLLAMA_NUM_PARALLEL). Cada llamada reutiliza
// el prefijo común más largo con alguna ranura; lo que no coincide se «procesa» a un costo fijo por
// token. Las duraciones son INVENTADAS (no son una medición) y el registro las marca con modelo
// «guion» y máquina «guion». Sirve para que el ejemplo se vea entero sin servidor, incluida la
// trampa de intercalar prefijos distintos cuando hay una sola ranura.
using Microsoft.Extensions.AI;
using OllamaSharp.Models.Chat;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed class SimuladorDeCacheDePrefijo(IChatClient interno, int ranuras = 1) : DelegatingChatClient(interno)
{
    public const double MsPorTokenFrio = 20.0;
    public const double MsPorTokenCacheado = 0.2;
    private const double CaracteresPorToken = 4.0;
    private const int MinimoReutilizable = 64;

    private readonly LinkedList<string> _ranuras = new();

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lista = messages.ToList();
        var respuesta = await base.GetResponseAsync(lista, options, cancellationToken);
        var prompt = string.Concat(lista.Select(m => $"<{m.Role.Value}>{m.Text}"));

        var comun = _ranuras.Select(r => PrefijoComun(r, prompt)).DefaultIfEmpty(0).Max();
        var total = (int)Math.Ceiling(prompt.Length / CaracteresPorToken);
        var cacheados = (int)Math.Floor(comun / CaracteresPorToken);
        var ms = (total - cacheados) * MsPorTokenFrio + cacheados * MsPorTokenCacheado;
        static long Ns(double milisegundos) => (long)(milisegundos * 1_000_000);

        // Si hay una ranura con un prefijo común que vale la pena (más que el encabezado de la
        // plantilla), se reutiliza y pasa al frente; si no, se ocupa una libre o sale la menos reciente.
        var usada = comun >= MinimoReutilizable ? _ranuras.FirstOrDefault(r => PrefijoComun(r, prompt) == comun) : null;
        if (usada is not null) _ranuras.Remove(usada);
        else if (_ranuras.Count >= ranuras) _ranuras.RemoveLast();
        _ranuras.AddFirst(prompt);

        var salida = (int)(respuesta.Usage?.OutputTokenCount ?? 1);
        respuesta.RawRepresentation = new ChatDoneResponseStream
        {
            Done = true,
            PromptEvalCount = total,
            PromptEvalDuration = Ns(ms),
            LoadDuration = Ns(1),
            EvalCount = salida,
            EvalDuration = Ns(salida * 10.0),
            TotalDuration = Ns(ms + salida * 10.0 + 1)
        };
        // REC-22: el servidor real informa prompt_eval_cached_count (lo lee LectorDePrefijoCacheado);
        // el simulador informa los tokens que descontó, con la misma clave. También es INVENTADO.
        (respuesta.AdditionalProperties ??= new AdditionalPropertiesDictionary())[MovilidadUrbana.Asistente.Comun.AI.LecturaDelServidorEnLaRespuesta.Clave] = (long)cacheados;
        return respuesta;
    }

    private static int PrefijoComun(string a, string b) => a.AsSpan().CommonPrefixLength(b);
}
