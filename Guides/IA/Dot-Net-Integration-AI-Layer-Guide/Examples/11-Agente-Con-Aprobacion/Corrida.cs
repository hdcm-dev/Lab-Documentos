// Nuevo (ejemplo 11). Raíz de composición y corrida de las entradas. Acá, y sólo acá, se elige el
// cliente (OllamaSharp por la fábrica de la capa única, o el guionado de --fake envuelto en la MISMA
// capa) y se arma el agente: AsAIAgent sobre ese cliente, con el bucle de herramientas y su tope.
//
// Escalón 1 con estado (plan §B.3, H2-033): el pedido es acotado («Borrá Resistencia»), el código fija
// las herramientas y el tope de iteraciones. El agente aporta la AgentSession entre DOS peticiones
// (SerializeSessionAsync / DeserializeSessionAsync, DEP-11); no sube el escalón ni hace falta para la
// aprobación, que es de M.E.AI (09 la resuelve sin agente).
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Aprobacion;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente;

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyDictionary<string, string> Veredictos, string RutaDelRegistro);

/// <summary>Lo que pasó en una conversación de dos (o más) peticiones.</summary>
/// <param name="Mostrados">Los pedidos que vio la persona, armados por el código.</param>
/// <param name="Ejecuciones">Las bajas que llegaron al código, de todas las peticiones.</param>
/// <param name="EstadosSerializados">La AgentSession serializada al final de cada petición (lo que guardaría el servidor).</param>
public sealed record ResultadoDeConversacion(
    IReadOnlyList<PedidoPendiente> Mostrados, string TextoFinal, IReadOnlyList<Ejecucion> Ejecuciones,
    int Peticiones, IReadOnlyList<string> EstadosSerializados);

public static class Corrida
{
    public const string Ejemplo = "11-Agente-Con-Aprobacion";
    public const string Oraculo = "agente-con-aprobacion";
    public const string NombreDelAgente = "AsistenteDeLocalidades";

    /// <summary>Tope de iteraciones del bucle por petición (escalón 1: el código lo fija).</summary>
    public const int TopeDeIteraciones = 3;

    /// <summary>Tope de vueltas de aprobación (cada vuelta es una petición más).</summary>
    public const int VueltasDeAprobacion = 2;

    public const string Instrucciones = """
        Sos el asistente del ABM de localidades de Movilidad Urbana.
        Para dar de baja una localidad, usá la herramienta eliminar_localidad con el nombre y la provincia.
        Si la persona no dijo la provincia, podés usar listar_localidades para buscarla, o dejarla vacía.
        La baja la tiene que aprobar la persona: no digas que la hiciste hasta que la herramienta responda.
        Cuando la herramienta responda, contale el resultado en una o dos oraciones, en castellano.
        """;

    /// <summary>
    /// Guion de --fake (N6 E5, ampliado): el «modelo» lista, pide la baja de Resistencia (Chaco) y,
    /// en la segunda petición, después de la aprobación, cuenta el resultado. Tres llamadas.
    /// </summary>
    public static GuionChatClient CrearGuion() => new(PasosDelGuion());

    public static PasoDelGuion[] PasosDelGuion() =>
    [
        PasoDelGuion.Herramienta("c-e01-l", HerramientasDelAgente.Listar),
        PasoDelGuion.Herramienta("c-e01-b", HerramientasDelAgente.Eliminar, Baja("Resistencia", "Chaco")),
        PasoDelGuion.Texto("Listo: se dio de baja Resistencia, de Chaco.")
    ];

    public static Dictionary<string, object?> Baja(string nombre, string provincia) => new() { ["nombre"] = nombre, ["provincia"] = provincia };

    /// <summary>
    /// El agente sobre el cliente ya envuelto por la capa única (el grabador queda entre los dos). El
    /// bucle de herramientas lo arma el código con su tope; ChatClientAgent no agrega otro si ya hay uno.
    /// </summary>
    public static AIAgent ArmarAgente(IChatClient grabador, IList<AITool> herramientas) =>
        new ChatClientBuilder(grabador)
            .UseFunctionInvocation(configure: f => f.MaximumIterationsPerRequest = TopeDeIteraciones)
            .Build()
            .AsAIAgent(instructions: Instrucciones, name: NombreDelAgente, tools: herramientas);

    /// <summary>Servicios del caso: persistencia real (SQLite) y LocalidadService, como el caso base.</summary>
    public static ServiceProvider CrearServicios(string rutaDb)
    {
        var servicios = new ServiceCollection().AddInfrastructure($"Data Source={rutaDb};Pooling=False");
        servicios.AddScoped<LocalidadService>();
        var proveedor = servicios.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        DatabaseInitializer.Initialize(proveedor);
        return proveedor;
    }

    /// <summary>La sesión de DATOS la pone el código, nunca el modelo (no confundir con la AgentSession).</summary>
    public static AsyncServiceScope AbrirSesion(ServiceProvider servicios, string sesion)
    {
        var alcance = servicios.CreateAsyncScope();
        alcance.ServiceProvider.GetRequiredService<SessionContext>().Set(sesion);
        return alcance;
    }

    /// <summary>
    /// Una conversación en peticiones separadas. Cada petición abre su alcance, arma herramientas y
    /// agente de nuevo (ADR-12: por petición, nunca en un singleton) y, al terminar, deja en el
    /// servidor sólo dos cosas: la AgentSession serializada y el pedido pendiente. La petición
    /// siguiente trae (id, decisión), deserializa la sesión y arma la respuesta desde el pedido guardado.
    /// </summary>
    public static async Task<ResultadoDeConversacion> ConversarAsync(
        ServiceProvider servicios, string sesionDeDatos, GrabadorDeLlamadas grabador, ServidorDeAprobaciones servidor,
        string texto, Func<PedidoPendiente, bool> persona, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var mostrados = new List<PedidoPendiente>();
        var ejecuciones = new List<Ejecucion>();
        var estados = new List<string>();
        var marcado = false;
        string textoFinal;
        IReadOnlyList<PedidoPendiente> pendientes;
        string estado;

        // --- petición 1: el pedido de la persona ---
        grabador.Peticion = "peticion-1";
        await using (var alcance = AbrirSesion(servicios, sesionDeDatos))
        {
            var localidades = alcance.ServiceProvider.GetRequiredService<LocalidadService>();
            var herramientas = new HerramientasDelAgente(localidades, servidor);
            var agente = ArmarAgente(grabador, herramientas.Crear());
            var sesion = await agente.CreateSessionAsync(cancellationToken);
            var respuesta = await agente.RunAsync(texto, sesion, cancellationToken: cancellationToken);
            marcado |= herramientas.LeyoTextoNoConfiable;
            pendientes = await servidor.RegistrarAsync(respuesta.Messages, localidades, marcado, cancellationToken);
            ejecuciones.AddRange(herramientas.Ejecuciones);
            textoFinal = respuesta.Text;
            estado = (await agente.SerializeSessionAsync(sesion, cancellationToken: cancellationToken)).GetRawText();
            estados.Add(estado);
        }

        var peticiones = 1;
        for (var vuelta = 0; vuelta < VueltasDeAprobacion; vuelta++)
        {
            var automaticas = servidor.TomarAutomaticas();
            if (pendientes.Count == 0 && automaticas.Count == 0) break;

            // --- fin de la petición: la UI muestra los pedidos armados por el código ---
            var decisiones = new List<(string Id, bool Aprobar)>();
            foreach (var p in pendientes)
            {
                mostrados.Add(p);
                var aprueba = persona(p);
                salida.WriteLine($"  pedido de aprobación [{p.Id}]: {p.Muestra}");
                salida.WriteLine($"  la persona {(aprueba ? "aprueba" : "rechaza")}");
                decisiones.Add((p.Id, aprueba));
            }

            // --- petición siguiente: llega sólo (id, decisión) ---
            peticiones++;
            grabador.Peticion = $"peticion-{peticiones}";
            await using var alcance = AbrirSesion(servicios, sesionDeDatos);
            var localidades = alcance.ServiceProvider.GetRequiredService<LocalidadService>();
            var herramientas = new HerramientasDelAgente(localidades, servidor);
            var agente = ArmarAgente(grabador, herramientas.Crear());
            using var guardado = JsonDocument.Parse(estado);
            var sesion = await agente.DeserializeSessionAsync(guardado.RootElement, cancellationToken: cancellationToken);
            List<AIContent> respuestas = [.. automaticas, .. decisiones.Select(d => servidor.Responder(d.Id, d.Aprobar))];
            var respuesta = await agente.RunAsync(new ChatMessage(ChatRole.User, respuestas), sesion, cancellationToken: cancellationToken);
            marcado |= herramientas.LeyoTextoNoConfiable;
            pendientes = await servidor.RegistrarAsync(respuesta.Messages, localidades, marcado, cancellationToken);
            ejecuciones.AddRange(herramientas.Ejecuciones);
            textoFinal = respuesta.Text;
            estado = (await agente.SerializeSessionAsync(sesion, cancellationToken: cancellationToken)).GetRawText();
            estados.Add(estado);
        }
        return new ResultadoDeConversacion(mostrados, textoFinal, ejecuciones, peticiones, estados);
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

        var rutaDb = Path.Combine(Path.GetTempPath(), $"ejemplo-11-{Guid.NewGuid():n}.db");
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

                var sesionDeDatos = $"ejemplo-11-{entrada.Id}";
                List<(int Id, string Nombre, string Provincia)> antes;
                await using (var s = AbrirSesion(servicios, sesionDeDatos))
                {
                    antes = [.. (await s.ServiceProvider.GetRequiredService<LocalidadService>().GetAllAsync(cancellationToken)).Select(l => (l.Id, l.Nombre, l.Provincia))];
                }

                var servidor = new ServidorDeAprobaciones();
                var desde = grabador.Llamadas.Count;
                salida.WriteLine($"{entrada.Id}: «{entrada.Texto}»");
                // La persona de la demostración aprueba lo que ve: lo que se ejecuta es lo que vio.
                var conversacion = await ConversarAsync(servicios, sesionDeDatos, grabador, servidor, entrada.Texto, _ => true, salida, cancellationToken);

                List<(int Id, string Nombre, string Provincia)> despues;
                await using (var s = AbrirSesion(servicios, sesionDeDatos))
                {
                    despues = [.. (await s.ServiceProvider.GetRequiredService<LocalidadService>().GetAllAsync(cancellationToken)).Select(l => (l.Id, l.Nombre, l.Provincia))];
                }

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
                        paso: l.Peticion,
                        herramientas: HerramientasDe(l, llamadas.Skip(i + 1)),
                        estadoCacheForzado: i == 0 ? frio : null);
                    reg.Escribir(renglon);
                    truncado |= renglon.TruncadoPosible == true;
                    if (renglon.Razonamiento != "None" || renglon.NumCtx is not > 0)
                        fallan.Add($"{entrada.Id}: la llamada {i + 1} no salió con razonamiento apagado y num_ctx explícito");
                }

                fallan.AddRange(Garantias.Comprobar(entrada.Id, entrada.Texto, servidor, conversacion, antes, despues, llamadas));

                var pasa = Veredicto(entrada, conversacion);
                var resultado = truncado ? ResultadoDeEntrada.Excluida : pasa ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla;
                var mostrado = conversacion.Mostrados.FirstOrDefault(p => p.Herramienta == HerramientasDelAgente.Eliminar && servidor.Aprobados.Contains(p.CallId));
                var motivo = $"esperado={entrada.Esperado};mostrado={(mostrado is null ? "ninguno" : $"{mostrado.IdResuelto}|{mostrado.NombreResuelto}|{mostrado.ProvinciaResuelta}")}";
                reg.Escribir(new RegistroDeEntrada
                {
                    Ejemplo = Ejemplo,
                    EntradaId = entrada.Id,
                    Oraculo = Oraculo,
                    Resultado = resultado,
                    Motivo = truncado ? $"{motivo};truncado_posible" : motivo
                });
                veredictos[entrada.Id] = resultado;

                foreach (var e in conversacion.Ejecuciones)
                    salida.WriteLine($"  ejecutó {e.Herramienta} → {(e.Resultado.Exito ? "éxito" : "rechazo")}: {e.Resultado.Mensaje}" +
                        (e.Resultado.Errores.Count > 0 ? $" ({string.Join("; ", e.Resultado.Errores.Select(x => $"{x.Key}: {x.Value}"))})" : ""));
                foreach (var (id, m) in servidor.RechazadosSinPreguntar)
                    salida.WriteLine($"  rechazado sin preguntar [{id}]: {m}");
                salida.WriteLine($"  asistente {(argumentos.Fake ? "(guion)" : "(modelo)")}: {conversacion.TextoFinal}");
                salida.WriteLine($"  peticiones: {conversacion.Peticiones} · AgentSession serializada entre peticiones: {conversacion.EstadosSerializados.Count} vez/veces · base: {antes.Count} → {despues.Count} localidades");
                salida.WriteLine($"  llamadas al modelo: {llamadas.Count} · oráculo {Oraculo} (esperado {entrada.Esperado}): {resultado}");
            }
        }
        finally
        {
            foreach (var f in new[] { rutaDb, rutaDb + "-wal", rutaDb + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }

        // Garantías que no dependen del modelo: corren siempre, con guion y su propia base.
        var guionadas = await Garantias.GuionadasAsync(config, cancellationToken);
        foreach (var g in guionadas.Comprobadas) salida.WriteLine($"garantía guionada: {g}");
        fallan.AddRange(guionadas.Fallan);
        salida.WriteLine($"registro: {reg.RenglonesEscritos} renglones en {Path.GetFileName(reg.Ruta)}");
        return new ResultadoDeCorrida(fallan, veredictos, reg.Ruta);
    }

    /// <summary>
    /// El oráculo, en C# (tools/oraculos/agente-con-aprobacion.jq lo recalcula desde el registro).
    /// baja: exactamente una baja con éxito; la localidad del pedido aprobado es la de la entrada
    /// (Resistencia, Chaco), y el id borrado es el id que mostró el pedido.
    /// </summary>
    public static bool Veredicto(Entrada entrada, ResultadoDeConversacion conversacion)
    {
        if (entrada.Esperado != "baja") return false;
        var ok = conversacion.Ejecuciones.Where(e => e.Herramienta == HerramientasDelAgente.Eliminar && e.Resultado.Exito).ToList();
        if (ok.Count != 1) return false;
        var mostrado = conversacion.Mostrados.FirstOrDefault(p => p.CallId == ok[0].CallId);
        return mostrado is not null &&
               mostrado.NombreResuelto == entrada.Nombre && mostrado.ProvinciaResuelta == entrada.Provincia &&
               ok[0].Resultado.Id == mostrado.IdResuelto;
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
