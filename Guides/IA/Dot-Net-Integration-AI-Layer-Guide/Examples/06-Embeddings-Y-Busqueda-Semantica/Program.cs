// Nuevo: raíz de composición del ejemplo 06 (plan §E.2; guía §7.1 y §7.5). Elige el generador de
// embeddings: el modelo del Ollama propio (EMB-B) o el generador estable (--fake, o EMB-C). El resto
// del código depende sólo de IEmbeddingGenerator<string, Embedding<float>>.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;

const string Ejemplo = "06-Embeddings-Y-Busqueda-Semantica";

var argumentos = ArgumentosDeEjemplo.Leer(args);
ConfiguracionDelModelo config;
try
{
    // Los embeddings van siempre al Ollama propio (el compartido responde 501: Sondeo §5).
    config = ConfiguracionDelModelo.DesdeEntorno().Validar() with { Servidor = TipoDeServidor.Propio };
}
catch (ConfiguracionInvalidaException e)
{
    Console.Error.WriteLine($"Configuración rechazada: {e.Message}");
    return 1;
}

var sinModelo = argumentos.Fake || config.EmbPlan == "EMB-C";
string? ollamaVersion = null, digest = null;
IEmbeddingGenerator<string, Embedding<float>> generador;
if (sinModelo)
{
    generador = new GeneradorDeEmbeddingsEstable(CorridaDeBusqueda.Dimension);
}
else
{
    var predicado = await PredicadoDeSalto.EvaluarAsync(config.EmbedEndpoint, config.EmbedModelo, PredicadoDeSalto.Embeddings);
    if (predicado.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el modelo de embeddings: {predicado.Motivo} (con EMB_PLAN=EMB-C corre sin modelo).");
        return 1;
    }
    (ollamaVersion, digest) = (predicado.VersionDeOllama, predicado.Digest);
    generador = FabricaDeClientes.CrearEmbeddingsOllama(config.EmbedEndpoint, config.EmbedModelo);
}

var rutaDelOro = ConjuntoDeOro.RutaPorDefecto;
var contexto = new ContextoDeCorrida
{
    Ejemplo = Ejemplo,
    Configuracion = config,
    Adaptador = Adaptador.OllamaSharp,
    Modelo = sinModelo ? "fnv1a-estable" : config.EmbedModelo,
    Digest = digest,
    OllamaVersion = ollamaVersion,
    EntradasVersion = "v1",
    EntradasHash = Hashes.Sha256DeArchivo(rutaDelOro),
    PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
    Maquina = argumentos.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
};

using (generador)
using (var registro = new EscritorDeRegistro(contexto))
{
    var corrida = new CorridaDeBusqueda(generador, config, contexto, rutaDelOro, argumentos.CrearPlazo(), sinModelo);
    var resultado = await corrida.CorrerAsync(registro, Console.Out);
    Console.WriteLine();
    Console.WriteLine($"{resultado.Llamadas} llamadas de embedding · registro en {Path.GetFileName(registro.Ruta)} · oráculo {CorridaDeBusqueda.Oraculo}");
    foreach (var falla in resultado.InvariantesQueFallan) Console.Error.WriteLine($"invariante: {falla}");
    Console.WriteLine($"invariantes de código: {(resultado.InvariantesCumplidos ? "cumplidos" : "FALLAN")}");
    return argumentos.Verificar && !resultado.InvariantesCumplidos ? 1 : 0;
}
