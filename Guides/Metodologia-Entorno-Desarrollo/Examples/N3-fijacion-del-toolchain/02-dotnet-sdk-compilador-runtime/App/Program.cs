using System.Runtime.InteropServices;

#if MOSTRAR_VERSION
// CS8304: el compilador informa su propia versión y la versión de C# elegida.
#error version
#endif

var c = new Caja();
#if USAR_CS14
// Asignación con ?. : sintaxis de C# 14. Con el C# por defecto de net8.0 (12) no compila.
c?.Valor = 42;
#else
c.Valor = 42;
#endif

Console.WriteLine($"Valor={c.Valor}");
Console.WriteLine($"Runtime que ejecuta: {RuntimeInformation.FrameworkDescription}");

class Caja { public int Valor { get; set; } }
