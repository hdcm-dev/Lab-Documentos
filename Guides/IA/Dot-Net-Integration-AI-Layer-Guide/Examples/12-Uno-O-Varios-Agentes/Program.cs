// Nuevo: ejemplo 12 (§8.6 variante «un-agente», §8.8 variante «dos-agentes» · OA-D7). Escalón 2 fijo
// —un workflow de MAF con el camino en código— y una sola variable: cuántos agentes. El verificador
// es una cuenta en código; ninguna variante usa un modelo para verificar cifras.
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
return a.Verificar && r.InvariantesQueFallan.Count > 0 ? 1 : 0;
