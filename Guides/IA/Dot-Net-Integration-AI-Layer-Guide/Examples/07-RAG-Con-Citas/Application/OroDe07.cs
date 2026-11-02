// Nuevo (REC-28): el conjunto de oro que corre 07. Es el v1 de 06 (enlazado, sin cambios, histórico)
// más una no respondible cercana a una regla real (n05). Se lee con el mismo ConjuntoDeOro de 06;
// sólo cambia el archivo. Su SHA-256 va en ejemplo.json y en cada línea del registro.
namespace MovilidadUrbana.Asistente.Application;

public static class OroDe07
{
    public const string Archivo = "oro/conjunto-de-oro.v2.json";
    public const string Version = "v2";

    public static string Ruta => Path.Combine(AppContext.BaseDirectory, Archivo);

    /// <summary>El v1 de 06, que v2 contiene sin cambios (una prueba de cableado lo comprueba).</summary>
    public static string RutaDelV1 => ConjuntoDeOro.RutaPorDefecto;
}
