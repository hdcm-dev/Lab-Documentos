// Nuevo (ejemplo 09). Raíz de composición y corrida de las entradas. Acá, y sólo acá, se elige el
// cliente (OllamaSharp por la fábrica de la capa única, o el guionado de --fake envuelto en la MISMA
// capa) y se arma el bucle de herramientas: UseFunctionInvocation, sin MAF.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Aprobacion;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Domain.Rules;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente;

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyDictionary<string, string> Veredictos, string RutaDelRegistro);

/// <summary>Lo que pasó en una conversación: los pedidos que vio la persona y el texto final del asistente.</summary>
public sealed record ResultadoDeConversacion(IReadOnlyList<PedidoPendiente> Mostrados, string TextoFinal);

public static class Corrida
{
    public const string Ejemplo = "09-Caso-De-Uso-Como-Herramienta";
    public const string Oraculo = "alta-con-aprobacion";

    /// <summary>Tope de vueltas de aprobación por entrada (cada vuelta es una petición más al modelo).</summary>
    public const int VueltasDeAprobacion = 2;

    public const string Instrucciones = """
        Sos el asistente del ABM de localidades de Movilidad Urbana.
        Para dar de alta una localidad, usá la herramienta agregar_localidad con los datos tal como los dio la persona:
        no corrijas ni completes ningún dato. Para dar de baja, usá eliminar_localidad con nombre y provincia.
        Cuando la herramienta responda, contale a la persona el resultado en una o dos oraciones, en castellano.
        """;

    /// <summary>Guion de --fake (N6 E1 §[2] y E5): rechazo del CP inválido; después, pedido, aprobación y alta.</summary>
    public static GuionChatClient CrearGuion() => new(
    [
        PasoDelGuion.Herramienta("c-e01", HerramientasDeLocalidades.Agregar, Alta("34")),
        PasoDelGuion.Texto("No pude agregar Goya: el código postal debe tener 4 dígitos. ¿Me pasás el código postal completo?"),
        PasoDelGuion.Herramienta("c-e02", HerramientasDeLocalidades.Agregar, Alta("3450")),
        PasoDelGuion.Texto("Listo: se agregó la localidad Goya, en Corrientes.")
    ]);

    private static Dictionary<string, object?> Alta(string cp) => new()
    {
        ["nombre"] = "Goya", ["provincia"] = "Corrientes", ["codigoPostal"] = cp, ["habitantes"] = 90000
    };

    /// <summary>El bucle de herramientas sobre el cliente ya envuelto por la capa única; el grabador queda entre los dos.</summary>
    public static IChatClient ArmarAsistente(GrabadorDeLlamadas grabador) =>
        new ChatClientBuilder(grabador)
            .UseFunctionInvocation(configure: f => f.MaximumIterationsPerRequest = 3)
            .Build();

    /// <summary>Servicios del caso: persistencia real (SQLite) y LocalidadService, como el caso base.</summary>
    public static ServiceProvider CrearServicios(string rutaDb)
    {
        var servicios = new ServiceCollection().AddInfrastructure($"Data Source={rutaDb};Pooling=False");
        servicios.AddScoped<LocalidadService>();
        var proveedor = servicios.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        DatabaseInitializer.Initialize(proveedor);
        return proveedor;
    }

    /// <summary>La sesión la pone el código, nunca el modelo.</summary>
    public static AsyncServiceScope AbrirSesion(ServiceProvider servicios, string sesion)
    {
        var alcance = servicios.CreateAsyncScope();
        alcance.ServiceProvider.GetRequiredService<SessionContext>().Set(sesion);
        return alcance;
    }

    /// <summary>
    /// Una conversación en DOS peticiones (DEP-09): la primera termina con el pedido de aprobación
    /// guardado en el servidor; la segunda trae sólo id y decisión, y la respuesta se arma desde el
    /// pedido guardado. La persona ve lo que armó el código, no la prosa del modelo.
    /// </summary>
    public static async Task<ResultadoDeConversacion> ConversarAsync(
        IChatClient asistente, IList<AITool> herramientas, ServidorDeAprobaciones servidor, string texto,
        Func<PedidoPendiente, bool> persona, TextWriter salida, CancellationToken cancellationToken = default)
    {
        List<ChatMessage> historial = [new(ChatRole.User, texto)];
        var opciones = new ChatOptions { Instructions = Instrucciones, Tools = herramientas };
        var mostrados = new List<PedidoPendiente>();

        var respuesta = await asistente.GetResponseAsync(historial, opciones, cancellationToken);
        historial.AddMessages(respuesta);
        for (var vuelta = 0; vuelta < VueltasDeAprobacion; vuelta++)
        {
            var pendientes = await servidor.RegistrarAsync(respuesta, cancellationToken);
            var automaticas = servidor.TomarAutomaticas();
            if (pendientes.Count == 0 && automaticas.Count == 0) break;

            // --- fin de la petición 1: lo pendiente queda en el servidor; la UI muestra esto ---
            var decisiones = new List<(string Id, bool Aprobar)>();
            foreach (var p in pendientes)
            {
                mostrados.Add(p);
                var aprueba = persona(p);
                salida.WriteLine($"  pedido de aprobación [{p.Id}]: {p.Muestra}");
                salida.WriteLine($"  la persona {(aprueba ? "aprueba" : "rechaza")}");
                decisiones.Add((p.Id, aprueba));
            }

            // --- petición 2: llega sólo (id, decisión) ---
            List<AIContent> respuestas = [.. automaticas, .. decisiones.Select(d => servidor.Responder(d.Id, d.Aprobar))];
            historial.Add(new ChatMessage(ChatRole.User, respuestas));
            respuesta = await asistente.GetResponseAsync(historial, opciones, cancellationToken);
            historial.AddMessages(respuesta);
        }
        return new ResultadoDeConversacion(mostrados, respuesta.Text);
    }

    public static async Task<ResultadoDeCorrida> CorrerAsync(
        ArgumentosDeEjemplo argumentos, ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
        var entradas = ArchivoDeEntradas.Leer(rutaEntradas).Entradas;
        var plazo = argumentos.CrearPlazo();
        var fallan = new List<string>();
        var veredictos = new Dictionary<string, string>();

        ClienteEspia? espia = null;
        IChatClient conCapa;
        string? version = null, digest = null;
        if (argumentos.Fake)
        {
            espia = new ClienteEspia(CrearGuion());
            conCapa = FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp);
        }
        else
        {
            var p = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, PredicadoDeSalto.Chat, cancellationToken);
            if (p.Saltar)
            {
                fallan.Add($"no se corre contra el modelo: {p.Motivo}");
                return new ResultadoDeCorrida(fallan, veredictos, string.Empty);
            }
            (version, digest) = (p.VersionDeOllama, p.Digest);
            conCapa = FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);
        }
        var grabador = new GrabadorDeLlamadas(conCapa);
        var asistente = ArmarAsistente(grabador);

        using var reg = new EscritorDeRegistro(new ContextoDeCorrida
        {
            Ejemplo = Ejemplo,
            Configuracion = config,
            Adaptador = Adaptador.OllamaSharp,
            Modelo = argumentos.Fake ? "guion" : config.Modelo,
            Digest = digest,
            OllamaVersion = version,
            EntradasVersion = "v1",
            EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
            PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
            Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
        });

        var rutaDb = Path.Combine(Path.GetTempPath(), $"ejemplo-09-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = CrearServicios(rutaDb);
            foreach (var entrada in entradas)
            {
                if (plazo.Vencido)
                {
                    reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = Oraculo, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
                    veredictos[entrada.Id] = ResultadoDeEntrada.Excluida;
                    salida.WriteLine($"{entrada.Id}: excluida (plazo)");
                    continue;
                }

                string? frio = null;
                if (!argumentos.Fake && config.Servidor == TipoDeServidor.Compartido &&
                    await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, cancellationToken) == false)
                {
                    frio = "frio";
                }

                await using var sesion = AbrirSesion(servicios, $"ejemplo-09-{entrada.Id}");
                var localidades = sesion.ServiceProvider.GetRequiredService<LocalidadService>();
                var herramientas = new HerramientasDeLocalidades(localidades);
                var servidor = new ServidorDeAprobaciones(localidades);
                var desde = grabador.Llamadas.Count;

                salida.WriteLine($"{entrada.Id}: «{entrada.Texto}»");
                // La persona de la demostración aprueba lo que ve: así se ve que aprobar NO valida; valida el caso de uso.
                var conversacion = await ConversarAsync(asistente, herramientas.Crear(), servidor, entrada.Texto, _ => true, salida, cancellationToken);

                var llamadas = grabador.Llamadas.Skip(desde).ToList();
                var truncado = false;
                for (var i = 0; i < llamadas.Count; i++)
                {
                    var l = llamadas[i];
                    ChatOptions? efectivas;
                    if (espia is not null)
                    {
                        efectivas = espia.OpcionesVistas[desde + i];
                    }
                    else
                    {
                        efectivas = l.Opciones?.Clone() ?? new ChatOptions();
                        OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
                    }
                    var renglon = reg.Llamada(entrada.Id, i + 1, l.Mensajes, efectivas, l.Respuesta, l.Duracion,
                        paso: i == 0 ? "pedido" : "tras-aprobacion",
                        herramientas: HerramientasDe(l, llamadas.Skip(i + 1)),
                        estadoCacheForzado: i == 0 ? frio : null);
                    reg.Escribir(renglon);
                    truncado |= renglon.TruncadoPosible == true;
                    if (renglon.Razonamiento != "None" || renglon.NumCtx is not > 0)
                        fallan.Add($"{entrada.Id}: la llamada {i + 1} no salió con razonamiento apagado y num_ctx explícito");
                }

                fallan.AddRange(await Garantias.ComprobarAsync(entrada.Id, herramientas, servidor, conversacion, localidades, cancellationToken));

                var pasa = Veredicto(entrada.Esperado, herramientas.Ejecuciones);
                var resultado = truncado ? ResultadoDeEntrada.Excluida : pasa ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla;
                reg.Escribir(new RegistroDeEntrada
                {
                    Ejemplo = Ejemplo,
                    EntradaId = entrada.Id,
                    Oraculo = Oraculo,
                    Resultado = resultado,
                    Motivo = truncado ? $"esperado={entrada.Esperado};truncado_posible" : $"esperado={entrada.Esperado}"
                });
                veredictos[entrada.Id] = resultado;

                foreach (var e in herramientas.Ejecuciones)
                    salida.WriteLine($"  ejecutó {e.Herramienta} → {(e.Resultado.Exito ? "éxito" : "rechazo")}: {e.Resultado.Mensaje}" +
                        (e.Resultado.Errores.Count > 0 ? $" ({string.Join("; ", e.Resultado.Errores.Select(x => $"{x.Key}: {x.Value}"))})" : ""));
                salida.WriteLine($"  asistente {(argumentos.Fake ? "(guion)" : "(modelo)")}: {conversacion.TextoFinal}");
                salida.WriteLine($"  llamadas al modelo: {llamadas.Count} · oráculo {Oraculo} (esperado {entrada.Esperado}): {resultado}");
            }
        }
        finally
        {
            foreach (var f in new[] { rutaDb, rutaDb + "-wal", rutaDb + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }

        fallan.AddRange(Garantias.AprobacionAlterada());
        salida.WriteLine($"registro: {reg.RenglonesEscritos} renglones en {Path.GetFileName(reg.Ruta)}");
        return new ResultadoDeCorrida(fallan, veredictos, reg.Ruta);
    }

    /// <summary>
    /// El oráculo, en C# (tools/oraculos/alta-con-aprobacion.jq lo recalcula desde el registro).
    /// rechazo: se ejecutó agregar_localidad y el caso de uso la rechazó por el código postal.
    /// alta: exactamente una alta con éxito, con los datos del pedido (Goya, Corrientes, 3450, 90000).
    /// </summary>
    public static bool Veredicto(string esperado, IReadOnlyList<Ejecucion> ejecuciones)
    {
        var altas = ejecuciones.Where(e => e.Herramienta == HerramientasDeLocalidades.Agregar).ToList();
        return esperado switch
        {
            "rechazo" => altas.Count >= 1 && altas.All(e => !e.Resultado.Exito) && altas.Any(e => e.Resultado.Errores.ContainsKey("codigoPostal")),
            "alta" => altas.Count(e => e.Resultado.Exito) == 1 && altas.Where(e => e.Resultado.Exito).All(e =>
                ServidorDeAprobaciones.Texto(e.Argumentos, "nombre").Trim().Equals("goya", StringComparison.OrdinalIgnoreCase) &&
                ServidorDeAprobaciones.Texto(e.Argumentos, "provincia") == "Corrientes" &&
                ServidorDeAprobaciones.Texto(e.Argumentos, "codigoPostal") == "3450" &&
                ServidorDeAprobaciones.Texto(e.Argumentos, "habitantes") == "90000"),
            _ => false
        };
    }

    /// <summary>Herramientas pedidas en una llamada, con el resultado que vio la llamada siguiente.</summary>
    private static List<HerramientaInvocada> HerramientasDe(LlamadaGrabada llamada, IEnumerable<LlamadaGrabada> siguientes)
    {
        var resultados = siguientes.SelectMany(s => s.Mensajes).SelectMany(m => m.Contents).OfType<FunctionResultContent>()
            .GroupBy(r => r.CallId).ToDictionary(g => g.Key, g => g.First());
        return llamada.Respuesta.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>()
            .Select(c => resultados.TryGetValue(c.CallId, out var r)
                ? new HerramientaInvocada(c.Name, c.Arguments, r.Result, r.Exception?.Message)
                : new HerramientaInvocada(c.Name, c.Arguments, null, null))
            .ToList();
    }
}

/// <summary>Las garantías de código que comprueba --verificar (plan §E.2, fila 09).</summary>
public static class Garantias
{
    public static async Task<IEnumerable<string>> ComprobarAsync(
        string id, HerramientasDeLocalidades herramientas, ServidorDeAprobaciones servidor, ResultadoDeConversacion conversacion,
        LocalidadService localidades, CancellationToken cancellationToken = default)
    {
        var fallan = new List<string>();
        // 1. Ninguna escritura sin aprobación.
        foreach (var e in herramientas.Ejecuciones.Where(e => !servidor.Aprobados.Contains(e.CallId)))
            fallan.Add($"{id}: {e.Herramienta} se ejecutó sin aprobación (llamada {e.CallId})");
        // 2. Nunca hay una localidad con CP inválido en la base.
        var enBase = await localidades.GetAllAsync(cancellationToken);
        foreach (var l in enBase.Where(l => !LocalidadRules.CodigoPostalValido(l.CodigoPostal)))
            fallan.Add($"{id}: quedó en la base {l.Nombre} con CP inválido «{l.CodigoPostal}»");
        // 3. Lo que se escribe es lo que mostró el pedido.
        foreach (var e in herramientas.Ejecuciones.Where(e => e.Herramienta == HerramientasDeLocalidades.Agregar && e.Resultado.Exito))
        {
            var mostrado = conversacion.Mostrados.FirstOrDefault(p => p.CallId == e.CallId);
            var fila = enBase.FirstOrDefault(l => l.Id == e.Resultado.Id);
            if (mostrado is null || fila is null ||
                fila.Nombre != ServidorDeAprobaciones.Texto(mostrado.Argumentos, "nombre").Trim() ||
                fila.Provincia != ServidorDeAprobaciones.Texto(mostrado.Argumentos, "provincia") ||
                fila.CodigoPostal != ServidorDeAprobaciones.Texto(mostrado.Argumentos, "codigoPostal").Trim() ||
                fila.Habitantes.ToString(System.Globalization.CultureInfo.InvariantCulture) != ServidorDeAprobaciones.Texto(mostrado.Argumentos, "habitantes"))
            {
                fallan.Add($"{id}: lo escrito por {e.CallId} no es lo que mostró el pedido");
            }
        }
        return fallan;
    }

    /// <summary>Sin modelo: una respuesta con argumentos distintos de los del pedido guardado se rechaza.</summary>
    public static IEnumerable<string> AprobacionAlterada()
    {
        var pedido = new ToolApprovalRequestContent("x-1", new FunctionCallContent("x-1", HerramientasDeLocalidades.Agregar,
            new Dictionary<string, object?> { ["nombre"] = "Goya", ["provincia"] = "Corrientes", ["codigoPostal"] = "3450", ["habitantes"] = 90000 }));
        var servidor = new ServidorDeAprobaciones(null!);
        servidor.RegistrarAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, [pedido]))).GetAwaiter().GetResult();
        var alterada = new FunctionCallContent("x-1", HerramientasDeLocalidades.Agregar,
            new Dictionary<string, object?> { ["nombre"] = "Goya", ["provincia"] = "Corrientes", ["codigoPostal"] = "3400", ["habitantes"] = 90000 });
        var r = servidor.Responder("x-1", true, alterada);
        if (r.Approved || servidor.Aprobados.Count > 0)
            yield return "una aprobación con argumentos alterados no se rechazó";
    }
}
