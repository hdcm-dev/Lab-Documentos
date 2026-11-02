// Nuevo: ejemplo 08 (§8.2–§8.3 · OA-D5). Primera herramienta, de SÓLO LECTURA: el modelo pide
// listar_localidades o contar_localidades y la app las ejecuta sobre LocalidadService, en la sesión
// que fija el código, con un tope de iteraciones que corta el bucle.
//
//   dotnet run -- --fake [--verificar]         guion, sin red
//   dotnet run -- --verificar [--plazo-s N]    contra el modelo (OLLAMA_ENDPOINT, OLLAMA_MODEL)
//   dotnet run -- ... --extension              las 8 entradas de la extensión de medición
//   dotnet run -- ... --fuera-de-catalogo      el estímulo «provincia fuera del catálogo» (REC-29)
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
