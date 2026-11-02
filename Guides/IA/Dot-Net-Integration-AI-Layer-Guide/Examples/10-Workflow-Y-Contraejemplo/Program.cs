// Nuevo: ejemplo 10 (§8.6 · OA-D6). La encuesta de tres pasos como workflow de MAF y como función, con
// las mismas entradas. Sin modelo: es el contraejemplo del escalón 2 (N6 C8).
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
