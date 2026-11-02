// Nuevo: las garantías de código del ejemplo 13 (plan §E.2, fila 13), las que comprueba --verificar.
// Ninguna depende de lo que conteste el modelo: valen con o sin él.
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Mcp;

namespace MovilidadUrbana.Asistente;

public static class Garantias
{
    /// <summary>El servidor quedó escuchando sólo en direcciones de loopback.</summary>
    public static List<string> SoloLoopback(ServidorMcp servidor)
    {
        var fuera = servidor.Direcciones.Where(d => !IPAddress.TryParse(d.Host, out var ip) || !IPAddress.IsLoopback(ip)).ToList();
        return fuera.Count == 0 && servidor.Direcciones.Count > 0
            ? []
            : [$"el servidor escucha fuera de loopback: {string.Join(", ", fuera)}"];
    }

    /// <summary>
    /// El cliente sabe qué versión del protocolo quedó negociada con el servidor (la respuesta a
    /// initialize): se registra en cada corrida en lugar de inferirla de las cadenas del SDK.
    /// </summary>
    public static List<string> ProtocoloNegociado(McpClient cliente) =>
        string.IsNullOrWhiteSpace(cliente.NegotiatedProtocolVersion)
            ? ["el cliente MCP no tiene versión de protocolo negociada"]
            : [];

    /// <summary>El listado del servidor real tiene sólo listar_localidades, marcada de lectura.</summary>
    public static List<string> ListadoSoloDeLectura(IList<McpClientTool> anunciadas)
    {
        var fallas = new List<string>();
        var nombres = anunciadas.Select(t => t.Name).ToList();
        if (!nombres.SequenceEqual([LecturasMcp.Listar])) fallas.Add($"el servidor anuncia: {string.Join(", ", nombres)}");
        if (anunciadas.Any(t => t.ProtocolTool.Annotations?.ReadOnlyHint != true))
        {
            fallas.Add("una herramienta del servidor no está marcada ReadOnly");
        }
        return fallas;
    }

    /// <summary>
    /// Aislamiento MCP ↔ API (H-014): por MCP vuelven las localidades de «demo-mcp» y ninguna de la
    /// sesión de la API, que sí tiene Goya (si no la tuviera, la prueba no probaría nada).
    /// </summary>
    public static async Task<List<string>> SesionFijaAisladaDeLaApiAsync(BaseDeEjemplo bd, McpClient cliente, CancellationToken cancellationToken = default)
    {
        var fallas = new List<string>();
        var r = await cliente.CallToolAsync(LecturasMcp.Listar, new Dictionary<string, object?>(), cancellationToken: cancellationToken);
        var texto = string.Concat(r.Content.OfType<TextContentBlock>().Select(c => c.Text));
        var filas = JsonSerializer.Deserialize<List<LocalidadDto>>(texto) ?? [];
        var nombres = filas.Select(f => f.Nombre).Order(StringComparer.Ordinal).ToList();
        if (r.IsError == true) fallas.Add($"listar_localidades por MCP devolvió error: {texto}");
        if (!nombres.SequenceEqual(BaseDeEjemplo.LocalidadesDeLaSesionMcp))
        {
            fallas.Add($"por MCP volvieron {string.Join(", ", nombres)}; se esperaban las de la sesión fija");
        }

        using var api = bd.AbrirSesion(BaseDeEjemplo.SesionDeLaApi);
        var deLaApi = (await api.Localidades.GetAllAsync(cancellationToken)).Select(l => l.Nombre).ToList();
        if (!BaseDeEjemplo.LocalidadesSoloDeLaApi.All(deLaApi.Contains)) fallas.Add("la sesión de la API no tiene Goya: la prueba no probaría nada");
        if (filas.Any(f => BaseDeEjemplo.LocalidadesSoloDeLaApi.Contains(f.Nombre))) fallas.Add("por MCP apareció una localidad de la sesión de la API");
        return fallas;
    }

    /// <summary>
    /// Servidor guionado que además anuncia borrar_todo, con anotaciones que mienten. El cliente no
    /// la monta, y aunque el «modelo» la pida, el bucle no la ejecuta (no está en ChatOptions.Tools).
    /// </summary>
    public static async Task<(List<string> Fallas, IReadOnlyList<string> Descartadas)> EscrituraAnunciadaNoSeMontaAsync(
        ConfiguracionDelModelo config, CancellationToken cancellationToken = default)
    {
        var fallas = new List<string>();
        using var bd = await BaseDeEjemplo.CrearAsync(cancellationToken);
        var contador = new ContadorDeBorrados();
        await using var servidor = await ServidorMcp.IniciarAsync(bd, guionado: true, contador, cancellationToken);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint, cancellationToken);
        var anunciadas = await cliente.ListToolsAsync(cancellationToken: cancellationToken);
        var montaje = ClienteMcp.Montar(anunciadas, PoliticaDeMontaje.DelEjemplo);

        if (!anunciadas.Any(t => t.Name == HerramientasDelServidorGuionado.BorrarTodo)) fallas.Add("el servidor guionado no anunció borrar_todo: la prueba no probaría nada");
        if (montaje.Montadas.OfType<AIFunctionDeclaration>().Any(t => t.Name == HerramientasDelServidorGuionado.BorrarTodo)) fallas.Add("el cliente montó borrar_todo");
        if (!montaje.Descartadas.Contains(HerramientasDelServidorGuionado.BorrarTodo)) fallas.Add("borrar_todo no figura entre las descartadas");

        var guion = new GuionChatClient([PasoDelGuion.Herramienta("g1", HerramientasDelServidorGuionado.BorrarTodo)], "No hice nada.");
        var asistente = new AsistenteConMcp(FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp));
        await asistente.PreguntarAsync("Borrá todo.", montaje.Montadas, cancellationToken);
        if (contador.Invocaciones != 0) fallas.Add($"borrar_todo se ejecutó {contador.Invocaciones} veces en el servidor");
        return (fallas, montaje.Descartadas);
    }

    /// <summary>La descripción que llega al modelo es la del cliente, no la del servidor (política del ejemplo).</summary>
    public static List<string> DescripcionDelCliente(Montaje montaje) => DescripcionDelCliente(montaje, PoliticaDeMontaje.DelEjemplo);

    /// <summary>
    /// La descripción que llega al modelo es la del cliente, no la del servidor, en las lecturas
    /// y también en las escrituras; una escritura montada va además con aprobación, y nada se
    /// monta fuera de la política.
    /// </summary>
    public static List<string> DescripcionDelCliente(Montaje montaje, PoliticaDeMontaje politica)
    {
        var fallas = new List<string>();
        foreach (var t in montaje.Montadas.OfType<AIFunctionDeclaration>())
        {
            if (politica.Lecturas.TryGetValue(t.Name, out var lectura))
            {
                if (t.Description != lectura) fallas.Add($"{t.Name}: la descripción montada no es la del cliente");
            }
            else if (politica.Escrituras.TryGetValue(t.Name, out var escritura))
            {
                if (t.Description != escritura) fallas.Add($"{t.Name}: la descripción montada de la escritura no es la del cliente");
                if (t is not ApprovalRequiredAIFunction) fallas.Add($"{t.Name}: la escritura se montó sin aprobación");
            }
            else
            {
                fallas.Add($"{t.Name}: se montó sin estar en la política del cliente");
            }
        }
        return fallas;
    }

    /// <summary>Resultado de pedir con un Host o un Origin dados.</summary>
    public sealed record Rechazos(HttpStatusCode HostAjeno, HttpStatusCode OrigenAjeno, HttpStatusCode OrigenNulo, HttpStatusCode OrigenLoopback);

    /// <summary>
    /// Host ajeno → 400 (HostFiltering); Origin ajeno o «null» → 403 (filtro); Origin de loopback →
    /// pasa a MCP (ni 400 ni 403). Ninguno de los dos es control de acceso.
    /// </summary>
    public static async Task<(List<string> Fallas, Rechazos Codigos)> HostYOrigenAjenosSeRechazanAsync(ServidorMcp servidor, CancellationToken cancellationToken = default)
    {
        using var http = new HttpClient();
        async Task<HttpStatusCode> PedirAsync(string? host, string? origen)
        {
            using var pedido = new HttpRequestMessage(HttpMethod.Post, servidor.Endpoint)
            {
                Content = new StringContent(
                    """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"prueba","version":"1"}}}""",
                    Encoding.UTF8, "application/json")
            };
            pedido.Headers.Accept.ParseAdd("application/json");
            pedido.Headers.Accept.ParseAdd("text/event-stream");
            if (host is not null) pedido.Headers.Host = host;
            if (origen is not null) pedido.Headers.Add("Origin", origen);
            using var r = await http.SendAsync(pedido, cancellationToken);
            return r.StatusCode;
        }

        var c = new Rechazos(
            await PedirAsync("ajeno.example", null),
            await PedirAsync(null, "http://ajeno.example"),
            await PedirAsync(null, "null"),
            await PedirAsync(null, $"http://localhost:{servidor.Endpoint.Port}"));
        var fallas = new List<string>();
        if (c.HostAjeno != HttpStatusCode.BadRequest) fallas.Add($"Host ajeno: {(int)c.HostAjeno}, se esperaba 400");
        if (c.OrigenAjeno != HttpStatusCode.Forbidden) fallas.Add($"Origin ajeno: {(int)c.OrigenAjeno}, se esperaba 403");
        if (c.OrigenNulo != HttpStatusCode.Forbidden) fallas.Add($"Origin «null»: {(int)c.OrigenNulo}, se esperaba 403");
        if (c.OrigenLoopback is HttpStatusCode.Forbidden or HttpStatusCode.BadRequest) fallas.Add($"Origin de loopback rechazado: {(int)c.OrigenLoopback}");
        return (fallas, c);
    }
}
