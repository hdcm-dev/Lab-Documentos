using System;
using System.Reflection;

// La pieza imprime lo que lleva grabado: versión informativa (con SHA) y metadatos de build.
var asm = Assembly.GetExecutingAssembly();
var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
Console.WriteLine($"InformationalVersion = {info}");
foreach (var m in asm.GetCustomAttributes<AssemblyMetadataAttribute>())
    Console.WriteLine($"AssemblyMetadata {m.Key} = {m.Value}");
Console.WriteLine($"Calculo: área de un círculo r=2 -> {Geometria.AreaCirculo(2):F4}");

static class Geometria
{
    public static double AreaCirculo(double radio) => Math.PI * radio * radio;
}
