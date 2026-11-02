// Nuevo: raíz de composición del ejemplo 02 (plan §E.2; guía §5.6). Es el ÚNICO archivo que nombra
// los adaptadores: `grep -lE "OllamaSharp|using OpenAI" *.cs` tiene que listar sólo este (H-007).
// Las dos vías van contra el MISMO servidor, el Ollama propio: por OllamaSharp (API nativa, con
// num_ctx) y por el SDK de OpenAI contra /v1 (sin num_ctx). No es un cambio de proveedor.
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using OpenAI;

const string Ejemplo = "02-Cambio-De-Adaptador";
const string Oraculo = "dos-adaptadores";

var argumentos = ArgumentosDeEjemplo.Leer(args);
ConfiguracionDelModelo config;
try
{
    // 02 habla siempre con el Ollama propio (plan §E.1): el registro dice «propio».
    config = ConfiguracionDelModelo.DesdeEntorno().Validar() with { Servidor = TipoDeServidor.Propio };
}
catch (ConfiguracionInvalidaException e)
{
    Console.Error.WriteLine($"Configuración rechazada: {e.Message}");
    return 1;
}

// Selección por configuración: --adaptador ollamasharp | openai-v1; sin el argumento, las dos vías.
var vias = RaizDeComposicion.ViasPedidas(argumentos.Resto);
if (vias is null)
{
    Console.Error.WriteLine("--adaptador admite «ollamasharp» u «openai-v1».");
    return 1;
}

var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
using var entradasJson = JsonDocument.Parse(File.ReadAllText(rutaEntradas));
var entrada = entradasJson.RootElement.GetProperty("entradas")[0];
var entradaId = entrada.GetProperty("id").GetString()!;
var pregunta = entrada.GetProperty("texto").GetString()!;

string? ollamaVersion = null, digest = null;
if (!argumentos.Fake)
{
    var predicado = await PredicadoDeSalto.EvaluarAsync(config.PropioEndpoint, config.PropioModelo, PredicadoDeSalto.Chat);
    if (predicado.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el modelo: {predicado.Motivo}");
        return 1;
    }
    (ollamaVersion, digest) = (predicado.VersionDeOllama, predicado.Digest);
}

var plazo = argumentos.CrearPlazo();
var resultados = new List<ResultadoDeVia>();
foreach (var adaptador in vias)
{
    if (plazo.Vencido)
    {
        resultados.Add(new(adaptador, null, null, null, Excluida: true));
        continue;
    }
    // Con --fake, el espía va DEBAJO de la capa de opciones (ve lo que de verdad saldría). Con el
    // modelo, el cliente sale entero de la raíz de composición, con la capa ya puesta.
    ClienteEspia? espia = argumentos.Fake ? new ClienteEspia(RaizDeComposicion.Guionado(adaptador, config)) : null;
    using var cliente = espia is null
        ? RaizDeComposicion.Crear(adaptador, config)
        : FabricaDeClientes.Envolver(espia, config, adaptador);
    var asistente = new AsistenteDeAlta(cliente);

    using var registro = new EscritorDeRegistro(new ContextoDeCorrida
    {
        Ejemplo = Ejemplo,
        Configuracion = config,
        Adaptador = adaptador,
        Modelo = argumentos.Fake ? "guion" : config.PropioModelo,
        Digest = digest,
        OllamaVersion = ollamaVersion,
        EntradasVersion = "v1",
        EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
        PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
        Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
    });

    var mensajes = AsistenteDeAlta.Mensajes(pregunta);
    var reloj = EscritorDeRegistro.Cronometro();
    var respuesta = await asistente.ResponderAsync(mensajes);
    var metadatos = asistente.Metadatos;
    // «paso» lleva el proveedor que informa GetService<ChatClientMetadata>(): así el oráculo lo
    // recalcula desde el registro.
    var renglon = registro.Llamada(entradaId, resultados.Count + 1, mensajes, espia?.OpcionesVistas[^1] ?? RaizDeComposicion.OpcionesEfectivas(adaptador, config), respuesta,
        reloj.Elapsed, paso: metadatos?.ProviderName);
    registro.Escribir(renglon);
    resultados.Add(new(adaptador, metadatos?.ProviderName, respuesta.Text, renglon, Excluida: false));

    Console.WriteLine($"[{RaizDeComposicion.Nombre(adaptador)} · proveedor «{metadatos?.ProviderName}»] {respuesta.Text}");
    Console.WriteLine($"    num_ctx enviado: {renglon.NumCtx?.ToString() ?? "no (por /v1 no llega)"} · razonamiento: {renglon.Razonamiento} · truncado_posible: {renglon.TruncadoPosible?.ToString() ?? "no verificable"}");
}

// Oráculo «dos-adaptadores» (lo recalcula tools/oraculos/dos-adaptadores.jq): por las dos vías hay
// respuesta no vacía y los proveedores informados son distintos.
var hechas = resultados.Where(r => !r.Excluida).ToList();
var excluida = hechas.Count < vias.Count || hechas.Any(r => r.Renglon!.TruncadoPosible == true);
var pasa = !excluida && hechas.All(r => !string.IsNullOrWhiteSpace(r.Texto)) &&
           hechas.Select(r => r.Proveedor).Distinct().Count() == hechas.Count;
if (vias.Count < 2)
{
    // Con una sola vía pedida (--adaptador) se muestra la selección y nada más: el oráculo compara las
    // dos vías de una misma corrida, así que no se escribe línea «entrada» (el registro de una corrida
    // es lo que recalcula tools/recalcular-oraculos.sh).
    Console.WriteLine($"oráculo {Oraculo}: no se evalúa (una sola vía pedida)");
}
else
{
    using var registro = new EscritorDeRegistro(new ContextoDeCorrida
    {
        Ejemplo = Ejemplo, Configuracion = config, Adaptador = Adaptador.OllamaSharp,
        EntradasVersion = "v1", EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas)
    });
    registro.Escribir(new RegistroDeEntrada
    {
        Ejemplo = Ejemplo,
        EntradaId = entradaId,
        Oraculo = Oraculo,
        Resultado = excluida ? ResultadoDeEntrada.Excluida : pasa ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla,
        Motivo = resultados.Any(r => r.Excluida) ? "plazo"
            : hechas.Any(r => r.Renglon!.TruncadoPosible == true) ? "truncado_posible" : null
    });
    Console.WriteLine($"oráculo {Oraculo}: {(excluida ? "excluida" : pasa ? "pasa" : "falla")} · registro en {Path.GetFileName(registro.Ruta)}");
}

// Invariantes de código (--verificar): razonamiento apagado en toda llamada; num_ctx sólo por
// OllamaSharp (y con el valor configurado); por /v1, nunca; el oráculo no falla.
var invariantes = hechas.All(r => r.Renglon!.Razonamiento == "None") &&
                  hechas.All(r => r.Adaptador == Adaptador.OllamaSharp
                      ? r.Renglon!.NumCtx == config.NumCtx
                      : r.Renglon!.NumCtx is null) &&
                  (vias.Count < 2 || excluida || pasa);
return argumentos.Verificar && !invariantes ? 1 : 0;

internal sealed record ResultadoDeVia(Adaptador Adaptador, string? Proveedor, string? Texto, RegistroDeLlamada? Renglon, bool Excluida);

/// <summary>Dónde se elige el adaptador. Lo de abajo de esto es lo único que cambia entre las dos vías.</summary>
public static class RaizDeComposicion
{
    public static string Nombre(Adaptador adaptador) => adaptador == Adaptador.OllamaSharp ? "ollamasharp" : "openai-v1";

    public static IReadOnlyList<Adaptador>? ViasPedidas(IReadOnlyList<string> resto)
    {
        var i = resto.ToList().IndexOf("--adaptador");
        if (i < 0) return [Adaptador.OllamaSharp, Adaptador.OpenAIv1];
        return i + 1 < resto.Count
            ? resto[i + 1] switch
            {
                "ollamasharp" => [Adaptador.OllamaSharp],
                "openai-v1" => [Adaptador.OpenAIv1],
                _ => null
            }
            : null;
    }

    /// <summary>
    /// El adaptador real, ya envuelto en la capa única de opciones. La vía nativa la construye
    /// FabricaDeClientes (OllamaSharp, con su HttpClient de 10 min); la de /v1, el SDK de OpenAI acá
    /// abajo, y la capa se le pone con Envolver. Nadie más fija razonamiento, num_ctx ni temperatura.
    /// </summary>
    public static IChatClient Crear(Adaptador adaptador, ConfiguracionDelModelo config) => adaptador switch
    {
        Adaptador.OllamaSharp => FabricaDeClientes.CrearChatOllama(config.PropioEndpoint, config.PropioModelo, config),
        _ => FabricaDeClientes.Envolver(CrearOpenAIv1(config), config, Adaptador.OpenAIv1)
    };

    /// <summary>
    /// Las opciones que salen hacia el servidor por esa vía, para el registro: las de la aplicación más
    /// las que pone la capa (la prueba LasOpcionesDelRegistro_SonLasQueVeElEspia comprueba que coinciden).
    /// </summary>
    public static ChatOptions OpcionesEfectivas(Adaptador adaptador, ConfiguracionDelModelo config)
    {
        var opciones = AsistenteDeAlta.Opciones();
        OpcionesDelProveedor.Aplicar(opciones, config, adaptador);
        return opciones;
    }

    /// <summary>
    /// SDK de OpenAI contra el /v1 de Ollama: 10 min de NetworkTimeout y cero reintentos (H-056).
    /// Ollama no pide clave; el SDK exige una, así que va un valor fijo que el servidor ignora.
    /// </summary>
    public static IChatClient CrearOpenAIv1(ConfiguracionDelModelo config)
    {
        var opciones = new OpenAIClientOptions
        {
            Endpoint = new Uri(new Uri(config.PropioEndpoint), "/v1"),
            NetworkTimeout = FabricaDeClientes.TimeoutDelModelo,
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0)
        };
        return new OpenAIClient(new ApiKeyCredential("ollama"), opciones)
            .GetChatClient(config.PropioModelo)
            .AsIChatClient();
    }

    /// <summary>
    /// Modo --fake: responde el guion, pero los metadatos son los del adaptador real, construido sin
    /// llamar a la red. Así la selección por configuración se prueba sin servidor. Devuelve el cliente
    /// SIN la capa: Program.cs pone el espía y después la capa, con FabricaDeClientes.Envolver.
    /// </summary>
    public static IChatClient Guionado(Adaptador adaptador, ConfiguracionDelModelo config)
    {
        using var real = Crear(adaptador, config);
        var guion = new GuionChatClient([PasoDelGuion.Texto(
            $"Nombre, provincia, código postal y habitantes (guion, vía {Nombre(adaptador)}).")]);
        return new ClienteConMetadatos(guion, real.GetService<ChatClientMetadata>());
    }
}

/// <summary>Delegante que informa otros metadatos (sólo para el modo --fake).</summary>
public sealed class ClienteConMetadatos(IChatClient interno, ChatClientMetadata? metadatos) : DelegatingChatClient(interno)
{
    public override object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType == typeof(ChatClientMetadata) ? metadatos : base.GetService(serviceType, serviceKey);
}
