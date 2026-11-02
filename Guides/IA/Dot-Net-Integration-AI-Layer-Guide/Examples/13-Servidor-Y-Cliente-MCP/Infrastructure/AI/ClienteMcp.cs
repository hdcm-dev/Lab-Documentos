// Nuevo: el lado cliente del ejemplo 13 (§8.9). McpClient.ListToolsAsync devuelve McpClientTool, que
// ya es AIFunction: se puede poner tal cual en ChatOptions.Tools. Pero lo que anuncia el servidor
// (nombres, descripciones, anotaciones y, después, resultados) es NO CONFIABLE (MCP 2026-07-28,
// Tools). Por eso el cliente monta sólo lo que está en SU lista permitida por nombre, con SU
// descripción, y descarta el resto. Las anotaciones del servidor (ReadOnly, Destructive) no deciden
// nada: un servidor puede mentir en ellas.
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>
/// Qué monta el cliente, por nombre y con la descripción que escribe el cliente: lecturas, y
/// escrituras (hoy ninguna) que, si alguna vez se agregan, van envueltas en
/// <see cref="ApprovalRequiredAIFunction" />. En las dos, la descripción de la herramienta es la
/// del cliente; el esquema y las descripciones de sus parámetros siguen siendo del servidor y son
/// no confiables.
/// </summary>
public sealed record PoliticaDeMontaje(IReadOnlyDictionary<string, string> Lecturas, IReadOnlyDictionary<string, string> Escrituras)
{
    /// <summary>La del ejemplo: sólo listar_localidades, y ninguna escritura.</summary>
    public static readonly PoliticaDeMontaje DelEjemplo = new(
        new Dictionary<string, string>
        {
            ["listar_localidades"] = "Lista las localidades cargadas por la persona: id, nombre y provincia de cada una."
        },
        new Dictionary<string, string>());
}

/// <summary>Lo montado (para ChatOptions.Tools) y los nombres que se descartaron.</summary>
public sealed record Montaje(IList<AITool> Montadas, IReadOnlyList<string> Descartadas);

public static class ClienteMcp
{
    /// <summary>Conecta por HTTP (Streamable HTTP) al endpoint del servidor.</summary>
    public static Task<McpClient> ConectarAsync(Uri endpoint, CancellationToken cancellationToken = default) =>
        McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint = endpoint }),
            cancellationToken: cancellationToken);

    /// <summary>Filtra lo anunciado por el servidor con la política del cliente.</summary>
    public static Montaje Montar(IEnumerable<McpClientTool> anunciadas, PoliticaDeMontaje politica)
    {
        var montadas = new List<AITool>();
        var descartadas = new List<string>();
        foreach (var herramienta in anunciadas)
        {
            if (politica.Lecturas.TryGetValue(herramienta.Name, out var descripcion))
            {
                // La descripción que ve el modelo es la del cliente, no la que mandó el servidor.
                montadas.Add(herramienta.WithDescription(descripcion));
            }
            else if (politica.Escrituras.TryGetValue(herramienta.Name, out var descripcionDeLaEscritura))
            {
                // También en una escritura: la descripción del cliente, y la aprobación por fuera.
                montadas.Add(new ApprovalRequiredAIFunction(herramienta.WithDescription(descripcionDeLaEscritura)));
            }
            else
            {
                descartadas.Add(herramienta.Name);
            }
        }
        return new Montaje(montadas, descartadas);
    }
}
