// Nuevo: ejemplo 11 (§8.7 · OA-D6, OA-S4). Un AIAgent de MAF (AsAIAgent) con eliminar_localidad
// aprobable, en el escalón 1: el pedido y la decisión llegan en dos peticiones separadas y la
// AgentSession se conserva entre las dos (SerializeSessionAsync). El texto del pedido lo arma el código.
//
//   dotnet run -- --fake [--verificar]         guion, sin red
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
// Sin registro: no se pudo correr contra el modelo (predicado de salto); eso nunca es un 0.
if (r.RutaDelRegistro.Length == 0) return 1;
return a.Verificar && r.InvariantesQueFallan.Count > 0 ? 1 : 0;
