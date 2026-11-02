// Nuevo: ejemplo 14 (§9.7 · OA-SK1). La misma contar_localidades anotada para Semantic Kernel y para
// M.E.AI, compiladas juntas; se comparan los metadatos que ve el modelo y el resultado de invocarlas.
// Sin modelo: ninguna llamada a un IChatClient.
//
//   dotnet run -- [--fake] [--verificar] [--plazo-s N]   (--fake se acepta por contrato; no cambia nada)
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
