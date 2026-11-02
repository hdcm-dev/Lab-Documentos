// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Domain/Rules/EncuestaRules.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — endurecimiento T2 (medios, frecuencia y motivo contra Catalogos); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Domain.Rules;

/// <summary>Rangos aceptados por la encuesta de transporte, paso por paso.</summary>
public static class EncuestaRules
{
    public const int TotalDePasos = 3;

    public const int LargoMinimoDelNombre = 3;
    public const int LargoMaximoDelNombre = 80;

    public const int EdadMinima = 16;
    public const int EdadMaxima = 110;

    public const double DistanciaMinima = 0;
    public const double DistanciaMaxima = 500;

    public const int MinutosMinimos = 1;
    public const int MinutosMaximos = 600;

    public static bool NombreValido(string? nombre) =>
        (nombre ?? string.Empty).Trim().Length >= LargoMinimoDelNombre;

    public static bool EdadValida(int? edad) => edad is >= EdadMinima and <= EdadMaxima;

    public static bool DistanciaValida(double? distancia) =>
        distancia is not null && distancia >= DistanciaMinima && distancia <= DistanciaMaxima;

    public static bool MinutosValidos(int? minutos) => minutos is >= MinutosMinimos and <= MinutosMaximos;

    // Endurecimiento T2 (huecos H3–H5): el caso base exigía «no vacío» para frecuencia y motivo, y
    // contaba los medios sin mirar sus claves (H4: ["avion"] pasaba y se registraba vacío).
    public static bool MedioValido(string? clave) =>
        clave is not null && Catalogos.Medios.Any(m => m.Clave == clave);

    public static bool MediosValidos(IEnumerable<string>? claves) =>
        claves is not null && claves.Any() && claves.All(MedioValido);

    public static bool FrecuenciaValida(string? clave) =>
        clave is not null && Catalogos.Frecuencias.Any(f => f.Clave == clave);

    public static bool MotivoValido(string? clave) =>
        clave is not null && Catalogos.Motivos.Any(m => m.Clave == clave);
}
