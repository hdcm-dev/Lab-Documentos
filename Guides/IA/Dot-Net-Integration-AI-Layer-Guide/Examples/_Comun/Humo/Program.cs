// Nuevo: humo del arnés. Una pregunta, una llamada, una línea de llamada y una de entrada.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;

var argumentos = ArgumentosDeEjemplo.Leer(args);
ConfiguracionDelModelo config;
try
{
    config = ConfiguracionDelModelo.DesdeEntorno().Validar();
}
catch (ConfiguracionInvalidaException e)
{
    Console.Error.WriteLine($"Configuración rechazada: {e.Message}");
    return 1;
}

var entradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
var guion = new GuionChatClient([PasoDelGuion.TextoEnFragmentos("Pide ", "nombre, ", "provincia, ", "código postal ", "y habitantes.")]);
IChatClient cliente = argumentos.Fake
    ? FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp)
    : FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);

var instrucciones = $"""
    Respondé en castellano, en una oración. El alta de una localidad pide:
    nombre ({LocalidadPolicy.RequisitoDelNombre}), provincia ({LocalidadPolicy.RequisitoDeLaProvincia}),
    código postal ({LocalidadPolicy.RequisitoDelCodigoPostal}) y habitantes ({LocalidadPolicy.RequisitoDeLosHabitantes}).
    """;
List<ChatMessage> mensajes = [new(ChatRole.User, "¿Qué datos pide el alta de una localidad?")];

using var registro = new EscritorDeRegistro(new ContextoDeCorrida
{
    Ejemplo = "humo",
    Configuracion = config,
    Adaptador = Adaptador.OllamaSharp,
    Modelo = argumentos.Fake ? "guion" : config.Modelo,
    EntradasVersion = "v1",
    EntradasHash = File.Exists(entradas) ? Hashes.Sha256DeArchivo(entradas) : null,
    PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
    Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
});

var reloj = EscritorDeRegistro.Cronometro();
var respuesta = await cliente.GetResponseAsync(mensajes, new ChatOptions { Instructions = instrucciones });
var opcionesEfectivas = argumentos.Fake ? guion.Llamadas[^1].Opciones : new ChatOptions { Instructions = instrucciones };
if (!argumentos.Fake) OpcionesDelProveedor.Aplicar(opcionesEfectivas!, config, Adaptador.OllamaSharp);
var linea = registro.Llamada("h01", 1, mensajes, opcionesEfectivas, respuesta, reloj.Elapsed);
registro.Escribir(linea);

// Oráculo «no-vacia»: hay texto y terminó por stop. Lo recalcula tools/oraculos/no-vacia.jq.
var pasa = !string.IsNullOrWhiteSpace(respuesta.Text) && respuesta.FinishReason == ChatFinishReason.Stop;
registro.Escribir(new RegistroDeEntrada
{
    Ejemplo = "humo",
    EntradaId = "h01",
    Oraculo = "no-vacia",
    Resultado = linea.TruncadoPosible == true ? ResultadoDeEntrada.Excluida : pasa ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla,
    Motivo = linea.TruncadoPosible == true ? "truncado_posible" : null
});

Console.WriteLine(respuesta.Text);
Console.WriteLine($"registro: {registro.RenglonesEscritos} líneas en {Path.GetFileName(registro.Ruta)}");

// Invariantes de código (los de --verificar): razonamiento apagado y num_ctx explícito en la llamada.
var invariantes = linea.Razonamiento == "None" && linea.NumCtx is > 0;
return argumentos.Verificar && !invariantes ? 1 : 0;
