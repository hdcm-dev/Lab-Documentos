// Nuevo (ejemplo 11). Las garantías de código que comprueba --verificar (plan §E.2, fila 11): sin
// aprobación, cero bajas; con «no», la base no cambia; lo borrado = lo mostrado en el pedido; la
// AgentSession se conserva entre las dos peticiones. Las que no dependen del modelo corren siempre
// con guion, también en la corrida contra el modelo.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Aprobacion;
using MovilidadUrbana.Asistente.Application.Herramientas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente;

public sealed record ResultadoDeGuionadas(IReadOnlyList<string> Comprobadas, IReadOnlyList<string> Fallan);

/// <summary>Lo que dejó un escenario guionado: la conversación y la base de cada sesión antes y después.</summary>
public sealed record Escenario(
    ResultadoDeConversacion Conversacion, ServidorDeAprobaciones Servidor, GuionChatClient Guion, ClienteEspia Espia,
    GrabadorDeLlamadas Grabador, IReadOnlyList<(int Id, string Nombre, string Provincia)> Antes,
    IReadOnlyList<(int Id, string Nombre, string Provincia)> Despues, IReadOnlyList<(int Id, string Nombre, string Provincia)> OtraSesion);

public static class Garantias
{
    public const string SesionA = "garantia-a";
    public const string SesionB = "garantia-b";

    public static IEnumerable<string> Comprobar(
        string id, string texto, ServidorDeAprobaciones servidor, ResultadoDeConversacion conversacion,
        IReadOnlyList<(int Id, string Nombre, string Provincia)> antes, IReadOnlyList<(int Id, string Nombre, string Provincia)> despues,
        IReadOnlyList<LlamadaGrabada> llamadas)
    {
        var fallan = new List<string>();
        var borrados = antes.Where(a => despues.All(d => d.Id != a.Id)).Select(a => a.Id).ToHashSet();
        var exitosas = conversacion.Ejecuciones.Where(e => e.Herramienta == HerramientasDelAgente.Eliminar && e.Resultado.Exito).ToList();

        // 1. Sin aprobación, cero bajas: toda baja con éxito tiene su CallId aprobado, y nada desaparece sin una.
        foreach (var e in exitosas.Where(e => !servidor.Aprobados.Contains(e.CallId)))
            fallan.Add($"{id}: se borró sin aprobación (llamada {e.CallId})");
        if (borrados.Count > 0 && exitosas.Count == 0)
            fallan.Add($"{id}: desaparecieron localidades sin ninguna baja aprobada");

        // 2. Lo borrado = lo mostrado: los ids que faltan son exactamente los que mostraron los pedidos aprobados.
        var mostradosAprobados = exitosas
            .Select(e => conversacion.Mostrados.FirstOrDefault(p => p.CallId == e.CallId)?.IdResuelto)
            .ToList();
        if (mostradosAprobados.Any(m => m is null) || !borrados.SetEquals(mostradosAprobados.OfType<int>()) ||
            exitosas.Any(e => e.Resultado.Id is not int b || !borrados.Contains(b)))
        {
            fallan.Add($"{id}: lo borrado ({string.Join(",", borrados)}) no es lo mostrado en el pedido ({string.Join(",", mostradosAprobados)})");
        }

        // 3. La AgentSession se conservó: la primera llamada de la petición 2 lleva el pedido de la petición 1.
        if (conversacion.Peticiones >= 2)
        {
            var primera = llamadas.FirstOrDefault(l => l.Peticion == "peticion-2");
            if (primera is null || !primera.Mensajes.Any(m => m.Role == ChatRole.User && m.Text.Trim() == texto.Trim()))
                fallan.Add($"{id}: la petición 2 no recibió el historial de la petición 1 (AgentSession no conservada)");
            if (conversacion.EstadosSerializados.Count < conversacion.Peticiones)
                fallan.Add($"{id}: la AgentSession no se serializó al final de cada petición");
        }
        return fallan;
    }

    /// <summary>Corre un escenario con guion sobre una base propia (SQLite temporal) y devuelve lo que dejó.</summary>
    public static async Task<Escenario> CorrerGuionAsync(
        ConfiguracionDelModelo config, string texto, IEnumerable<PasoDelGuion> pasos, Func<PedidoPendiente, bool> persona,
        Func<LocalidadService, Task>? prepararOtraSesion = null, CancellationToken cancellationToken = default)
    {
        var rutaDb = Path.Combine(Path.GetTempPath(), $"ejemplo-11-guion-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(rutaDb);
            if (prepararOtraSesion is not null)
            {
                await using var b = Corrida.AbrirSesion(servicios, SesionB);
                await prepararOtraSesion(b.ServiceProvider.GetRequiredService<LocalidadService>());
            }
            var antes = await ListarAsync(servicios, SesionA, cancellationToken);

            var guion = new GuionChatClient(pasos, "Listo.");
            var espia = new ClienteEspia(guion);
            var grabador = new GrabadorDeLlamadas(FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp));
            var servidor = new ServidorDeAprobaciones();
            var conversacion = await Corrida.ConversarAsync(servicios, SesionA, grabador, servidor, texto, persona, TextWriter.Null, cancellationToken);

            var despues = await ListarAsync(servicios, SesionA, cancellationToken);
            var otra = prepararOtraSesion is null ? [] : await ListarAsync(servicios, SesionB, cancellationToken);
            return new Escenario(conversacion, servidor, guion, espia, grabador, antes, despues, otra);
        }
        finally
        {
            foreach (var f in new[] { rutaDb, rutaDb + "-wal", rutaDb + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }
    }

    public static async Task<List<(int Id, string Nombre, string Provincia)>> ListarAsync(ServiceProvider servicios, string sesion, CancellationToken cancellationToken = default)
    {
        await using var s = Corrida.AbrirSesion(servicios, sesion);
        return [.. (await s.ServiceProvider.GetRequiredService<LocalidadService>().GetAllAsync(cancellationToken)).Select(l => (l.Id, l.Nombre, l.Provincia))];
    }

    /// <summary>
    /// Las garantías sin modelo (guion de N6 E5 y el del desajuste de texto e id). Devuelven lo que
    /// comprobaron y lo que falló; --verificar las suma a sus invariantes.
    /// </summary>
    public static async Task<ResultadoDeGuionadas> GuionadasAsync(ConfiguracionDelModelo config, CancellationToken cancellationToken = default)
    {
        var ok = new List<string>();
        var fallan = new List<string>();

        // a) Con «no» (CreateResponse(false)), la base no cambia.
        var no = await CorrerGuionAsync(config, "Borrá Resistencia.",
            [PasoDelGuion.Herramienta("g-no", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "Chaco")), PasoDelGuion.Texto("Entendido: no la borro.")],
            _ => false, cancellationToken: cancellationToken);
        if (no.Conversacion.Mostrados.Count == 1 && no.Conversacion.Ejecuciones.Count == 0 && no.Despues.SequenceEqual(no.Antes))
            ok.Add("con «no», cero bajas y la base no cambia");
        else fallan.Add("guion «no»: la base cambió o la baja llegó a ejecutarse");

        // b) Desajuste: el texto del modelo dice Resistencia y la llamada pide Corrientes. El pedido
        //    muestra Corrientes (lo que resolvió el código), avisa el desajuste y lo borrado = lo mostrado.
        var des = await CorrerGuionAsync(config, "Borrá Resistencia.",
            [
                new PasoDelGuion([new TextContent("Voy a borrar Resistencia."), new FunctionCallContent("g-des", HerramientasDelAgente.Eliminar, Corrida.Baja("Corrientes", "Corrientes"))]),
                PasoDelGuion.Texto("Listo.")
            ],
            _ => true, cancellationToken: cancellationToken);
        var pedido = des.Conversacion.Mostrados.SingleOrDefault();
        var corrientes = des.Antes.Single(l => l.Nombre == "Corrientes");
        if (pedido is { Desajuste: true, NombreResuelto: "Corrientes" } && pedido.Muestra.StartsWith("Borrar \"Corrientes\" (Corrientes)", StringComparison.Ordinal) &&
            des.Despues.All(l => l.Id != corrientes.Id) && des.Despues.Any(l => l.Nombre == "Resistencia") &&
            Comprobar("guion-desajuste", "Borrá Resistencia.", des.Servidor, des.Conversacion, des.Antes, des.Despues, des.Grabador.Llamadas).Any() == false)
            ok.Add("desajuste de texto e id: el pedido muestra Corrientes, lo avisa, y se borra lo mostrado");
        else fallan.Add("guion del desajuste: el pedido no mostró lo que resolvió el código, o lo borrado no fue lo mostrado");

        // c) Una respuesta con argumentos distintos de los del pedido guardado se rechaza.
        var guardado = new ToolApprovalRequestContent("g-alt", new FunctionCallContent("g-alt", HerramientasDelAgente.Eliminar, Corrida.Baja("Resistencia", "Chaco")));
        var rutaDb = Path.Combine(Path.GetTempPath(), $"ejemplo-11-alt-{Guid.NewGuid():n}.db");
        try
        {
            await using var servicios = Corrida.CrearServicios(rutaDb);
            await using var s = Corrida.AbrirSesion(servicios, SesionA);
            var servidor = new ServidorDeAprobaciones();
            var p = await servidor.RegistrarAsync([new ChatMessage(ChatRole.Assistant, [guardado])], s.ServiceProvider.GetRequiredService<LocalidadService>(), false, cancellationToken);
            var r = servidor.Responder(p.Single().Id, true, new FunctionCallContent("g-alt", HerramientasDelAgente.Eliminar, Corrida.Baja("Corrientes", "Corrientes")));
            if (!r.Approved && servidor.Aprobados.Count == 0) ok.Add("aprobación con argumentos alterados: se rechaza");
            else fallan.Add("una aprobación con argumentos alterados no se rechazó");
        }
        finally
        {
            foreach (var f in new[] { rutaDb, rutaDb + "-wal", rutaDb + "-shm" }) if (File.Exists(f)) File.Delete(f);
        }

        // d) Una localidad de otra sesión no se alcanza: se rechaza sin preguntar.
        //    Goya, Corrientes, 3450, 90000: LocalidadesTests.cs l. 50 @ 8fb4c3a (escrito para el ejemplo).
        var ajena = await CorrerGuionAsync(config, "Borrá Goya.",
            [PasoDelGuion.Herramienta("g-ajena", HerramientasDelAgente.Eliminar, Corrida.Baja("Goya", "Corrientes")), PasoDelGuion.Texto("No la encontré en tu sesión.")],
            _ => true,
            async b => await b.SaveAsync(new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 }),
            cancellationToken);
        if (ajena.Conversacion.Mostrados.Count == 0 && ajena.Servidor.RechazadosSinPreguntar.Count == 1 &&
            ajena.Despues.SequenceEqual(ajena.Antes) && ajena.OtraSesion.Any(l => l.Nombre == "Goya"))
            ok.Add("localidad de otra sesión: se rechaza sin preguntar y ninguna base cambia");
        else fallan.Add("guion de otra sesión: se preguntó o se borró una localidad ajena");

        return new ResultadoDeGuionadas(ok, fallan);
    }
}
