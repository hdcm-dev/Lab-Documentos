// Nuevo: ejemplo 09 (§8.4–§8.5 · OA-D5, OA-S2). Dos casos de uso del ABM como herramientas de
// escritura, las dos con ApprovalRequiredAIFunction bajo UseFunctionInvocation, sin MAF. El Result del
// caso de uso vuelve al modelo; la garantía (nunca un CP inválido en la base) es del caso de uso.
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
