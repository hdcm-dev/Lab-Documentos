// Nuevo (ejemplo 10). El resultado comparable de las dos versiones: cómo terminó, cuántos intentos llevó
// cada paso y los datos validados. Canonico() es lo que se compara y lo que viaja al registro.
using System.Globalization;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application.Encuestas;

public sealed record ResultadoDeEncuesta(
    bool Completa, int PasoFinal, IReadOnlyList<int> Intentos, IReadOnlyList<string> Rechazos, string Datos)
{
    /// <summary><c>completa[1,2,1]</c> o <c>incompleta@3[1,1,0]</c>: el mismo formato que el «esperado» de las entradas.</summary>
    public string Estado => Completa
        ? $"completa[{string.Join(",", Intentos)}]"
        : $"incompleta@{PasoFinal}[{string.Join(",", Intentos)}]";

    /// <summary>Estado y datos: dos versiones coinciden si y sólo si coincide esto.</summary>
    public string Canonico => $"{Estado} {Datos}";

    /// <summary>Los datos del modelo en el orden del catálogo (como RegistrarAsync), para que la comparación sea estable.</summary>
    public static string DatosDe(EncuestaModel m)
    {
        var medios = string.Join(",", Catalogos.Medios.Where(x => m.Medios.Contains(x.Clave)).Select(x => x.Clave));
        var ci = CultureInfo.InvariantCulture;
        return string.Join("|",
            m.Nombre, m.Edad?.ToString(ci) ?? "", m.Localidad, medios, m.Frecuencia,
            m.Distancia?.ToString(ci) ?? "", m.Minutos?.ToString(ci) ?? "", m.Motivo);
    }
}
