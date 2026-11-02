// Nuevo: ejemplo 13 (§8.9 · OA-D8). Servidor MCP HTTP que publica una lectura (listar_localidades,
// en la sesión fija «demo-mcp») sólo en loopback, y cliente MCP que la consume como AIFunction,
// montando sólo lo que está en su lista permitida por nombre.
//
//   dotnet run -- --fake [--verificar]         guion, sin servidor de modelos
//   dotnet run -- --verificar [--plazo-s N]    contra el modelo (OLLAMA_ENDPOINT, OLLAMA_MODEL)
using MovilidadUrbana.Asistente;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;

var a = ArgumentosDeEjemplo.Leer(args);
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

var r = await Corrida.CorrerAsync(a, config, Console.Out);
foreach (var i in r.InvariantesQueFallan) Console.Error.WriteLine($"INVARIANTE: {i}");
Console.WriteLine(r.InvariantesQueFallan.Count == 0 ? "invariantes de código: se cumplen" : $"invariantes de código: {r.InvariantesQueFallan.Count} fallan");
return a.Verificar && r.InvariantesQueFallan.Count > 0 ? 1 : 0;
