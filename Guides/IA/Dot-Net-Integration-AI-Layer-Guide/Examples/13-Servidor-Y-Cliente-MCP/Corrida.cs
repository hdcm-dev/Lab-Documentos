// Nuevo: la corrida del ejemplo 13, separada de Program.cs para que la prueba [Category("LLM")] la
// reuse. Levanta el servidor MCP en loopback, conecta el cliente, monta lo permitido, corre las
// entradas (una línea R-LLM por llamada al modelo y una por entrada) y comprueba las garantías.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Mcp;

namespace MovilidadUrbana.Asistente;

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, int Pasan, int Validas, int Total, string? RutaDelRegistro,
    string? ProtocoloNegociado = null);

public static class Corrida
{
    private static readonly JsonSerializerOptions Legible = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public const string Ejemplo = "13-Servidor-Y-Cliente-MCP";

    public static async Task<ResultadoDeCorrida> CorrerAsync(
        ArgumentosDeEjemplo a, ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
        var entradas = ArchivoDeEntradas.Leer(rutaEntradas).Entradas;
        var invariantes = new List<string>();

        // Servidor y cliente en el mismo proceso (N6 E1 §[5]); el servidor sólo en loopback.
        using var bd = await BaseDeEjemplo.CrearAsync(cancellationToken);
        await using var servidor = await ServidorMcp.IniciarAsync(bd, cancellationToken: cancellationToken);
        await using var cliente = await ClienteMcp.ConectarAsync(servidor.Endpoint, cancellationToken);
        var anunciadas = await cliente.ListToolsAsync(cancellationToken: cancellationToken);
        var montaje = ClienteMcp.Montar(anunciadas, PoliticaDeMontaje.DelEjemplo);
        var protocolo = cliente.NegotiatedProtocolVersion;
        salida.WriteLine($"servidor MCP en loopback (puerto {servidor.Endpoint.Port}); anuncia: {string.Join(", ", anunciadas.Select(t => t.Name))}");
        salida.WriteLine($"protocolo MCP negociado: {protocolo ?? "ninguno"}");
        salida.WriteLine($"cliente: monta {string.Join(", ", montaje.Montadas.OfType<AIFunctionDeclaration>().Select(t => t.Name))}; descarta {(montaje.Descartadas.Count == 0 ? "nada" : string.Join(", ", montaje.Descartadas))}");

        // Cliente: siempre por la capa única. En --fake, el guion pide la herramienta MCP como lo
        // haría el modelo, y la ejecución es un pedido MCP real al servidor en loopback.
        var guion = new GuionChatClient(GuionDelEjemplo.Pasos(entradas), "No tengo ese dato.");
        var espia = new ClienteEspia(guion);
        var capa = a.Fake
            ? FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp)
            : FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);

        string? version = null, digest = null;
        if (!a.Fake)
        {
            var p = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, PredicadoDeSalto.Chat, cancellationToken);
            if (p.Saltar)
            {
                return new ResultadoDeCorrida([$"no se corre contra el modelo: {p.Motivo}"], 0, 0, entradas.Count, null, protocolo);
            }
            (version, digest) = (p.VersionDeOllama, p.Digest);
        }

        // La versión negociada va al registro como una línea propia, antes de abrir el escritor
        // R-LLM (que deja el archivo abierto hasta el final). Las herramientas que leen el registro
        // filtran por «tipo», así que esta línea no cambia lo que cuentan.
        RegistrarNegociacion(config, protocolo, a.Fake);

        using var reg = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo,
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,
            Modelo = a.Fake ? "guion" : config.Modelo,
            Digest = digest,
            OllamaVersion = version,
            EntradasVersion = "v1",
            EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
            PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
            Maquina = a.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
        });

        var asistente = new AsistenteConMcp(capa);
        var plazo = a.CrearPlazo();
        int pasan = 0, validas = 0;

        foreach (var entrada in entradas)
        {
            if (plazo.Vencido)
            {
                reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = OraculoSesionFija.Nombre, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
                salida.WriteLine($"{entrada.Id}: excluida (plazo)");
                continue;
            }

            string? frio = null;
            if (!a.Fake && config.Servidor == TipoDeServidor.Compartido &&
                await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, cancellationToken) == false)
            {
                frio = "frio";
            }

            var respuesta = await asistente.PreguntarAsync(entrada.Texto, montaje.Montadas, cancellationToken);

            // Resultados de las herramientas, tal como volvieron al modelo (son NO CONFIABLES: los
            // escribió el servidor MCP, no esta app).
            var resultados = respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>()
                .GroupBy(r => r.CallId).ToDictionary(g => g.Key, g => g.Last());
            var lineas = new List<RegistroDeLlamada>();
            var grabadas = asistente.Grabador.Llamadas;
            for (var i = 0; i < grabadas.Count; i++)
            {
                var g = grabadas[i];
                var efectivas = g.Opciones?.Clone() ?? new ChatOptions();
                OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
                var pedidas = g.Respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>()
                    .Select(c => resultados.TryGetValue(c.CallId, out var r)
                        ? new HerramientaInvocada(c.Name, c.Arguments, r.Result, r.Exception?.Message)
                        : new HerramientaInvocada(c.Name, c.Arguments, null, "sin ejecutar (corte del bucle)"))
                    .ToList();
                var linea = reg.Llamada(entrada.Id, i + 1, g.Mensajes, efectivas, g.Respuesta, g.Reloj,
                    paso: pedidas.Count > 0 ? "pedir-herramienta-mcp" : "responder", herramientas: pedidas,
                    estadoCacheForzado: i == 0 ? frio : null);
                reg.Escribir(linea);
                lineas.Add(linea);
                if (linea.Razonamiento != "None" || linea.NumCtx is not > 0)
                {
                    invariantes.Add($"{entrada.Id}, llamada {i + 1}: no salió con razonamiento apagado y num_ctx explícito");
                }
            }
            if (grabadas.Count > AsistenteConMcp.MaximoDeIteraciones + 1)
            {
                invariantes.Add($"{entrada.Id}: {grabadas.Count} llamadas al modelo, más que el tope del bucle");
            }

            var todas = JsonSerializer.SerializeToElement(lineas.SelectMany(l => l.Herramientas).ToList());
            var veredicto = lineas.Count == 0
                ? new VeredictoDelOraculo(ResultadoDeEntrada.Excluida, "sin llamadas")
                : OraculoSesionFija.Evaluar(lineas[^1].Salida, todas, lineas.Any(l => l.TruncadoPosible == true));
            reg.Escribir(new RegistroDeEntrada
            {
                Ejemplo = Ejemplo,
                EntradaId = entrada.Id,
                Oraculo = OraculoSesionFija.Nombre,
                Resultado = veredicto.Resultado,
                Motivo = veredicto.Motivo
            });
            if (veredicto.Resultado != ResultadoDeEntrada.Excluida) validas++;
            if (veredicto.Resultado == ResultadoDeEntrada.Pasa) pasan++;

            salida.WriteLine($"{entrada.Id} ({entrada.Caso}): «{entrada.Texto}»");
            foreach (var h in lineas.SelectMany(l => l.Herramientas))
            {
                salida.WriteLine($"  herramienta MCP {h.Nombre}({JsonSerializer.Serialize(h.Argumentos ?? new Dictionary<string, object?>(), Legible)}) → {JsonSerializer.Serialize(h.Resultado, Legible)}");
            }
            salida.WriteLine($"  respuesta {(a.Fake ? "del guion" : "del modelo")}: {respuesta.Text}");
            salida.WriteLine($"  llamadas al modelo: {grabadas.Count} · herramientas ejecutadas: {lineas.Sum(l => l.Herramientas.Count)}");
            salida.WriteLine($"  oráculo {OraculoSesionFija.Nombre}: {veredicto.Resultado}{(veredicto.Motivo is null ? "" : $" ({veredicto.Motivo})")}");
        }

        // Garantías de código: no dependen del modelo y se comprueban en toda corrida.
        invariantes.AddRange(Garantias.SoloLoopback(servidor));
        invariantes.AddRange(Garantias.ProtocoloNegociado(cliente));
        invariantes.AddRange(Garantias.ListadoSoloDeLectura(anunciadas));
        invariantes.AddRange(Garantias.DescripcionDelCliente(montaje));
        invariantes.AddRange(await Garantias.SesionFijaAisladaDeLaApiAsync(bd, cliente, cancellationToken));
        var (fallasGuionado, descartadas) = await Garantias.EscrituraAnunciadaNoSeMontaAsync(config, cancellationToken);
        invariantes.AddRange(fallasGuionado);
        salida.WriteLine($"servidor guionado (anuncia borrar_todo con ReadOnly = true): el cliente descarta {string.Join(", ", descartadas)}; borrar_todo no se ejecutó");
        var (fallasHost, codigos) = await Garantias.HostYOrigenAjenosSeRechazanAsync(servidor, cancellationToken);
        invariantes.AddRange(fallasHost);
        salida.WriteLine($"Host ajeno → {(int)codigos.HostAjeno} · Origin ajeno → {(int)codigos.OrigenAjeno} · Origin «null» → {(int)codigos.OrigenNulo} · Origin de loopback → {(int)codigos.OrigenLoopback}");
        if (a.Fake && espia.OpcionesVistas.Any(o => o?.Reasoning?.Effort != ReasoningEffort.None || OpcionesDelProveedor.NumCtxDe(o) is not > 0))
        {
            invariantes.Add("--fake: el cliente espía vio una llamada sin razonamiento apagado o sin num_ctx");
        }

        salida.WriteLine($"demostración: {pasan} de {entradas.Count} entradas pasan el oráculo {OraculoSesionFija.Nombre} (sin tasa)");
        salida.WriteLine($"registro: {reg.RenglonesEscritos} líneas en {Path.GetFileName(reg.Ruta)}, más la de la negociación MCP");
        return new ResultadoDeCorrida(invariantes, pasan, validas, entradas.Count, reg.Ruta, protocolo);
    }

    /// <summary>Línea <c>tipo: "mcp-negociacion"</c> del registro: la versión del protocolo que quedó negociada.</summary>
    private static void RegistrarNegociacion(ConfiguracionDelModelo config, string? protocolo, bool fake)
    {
        Directory.CreateDirectory(config.RegistroDir);
        var linea = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["tipo"] = "mcp-negociacion",
            ["fecha"] = DateTimeOffset.UtcNow,
            ["ejemplo"] = Ejemplo,
            ["protocolo_negociado"] = protocolo,
            ["maquina"] = fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA"),
            ["turno_id"] = Environment.GetEnvironmentVariable("RLLM_TURNO_ID")
        }, Legible);
        File.AppendAllText(Path.Combine(config.RegistroDir, $"{Ejemplo}.jsonl"), linea + "\n", new System.Text.UTF8Encoding(false));
    }
}
