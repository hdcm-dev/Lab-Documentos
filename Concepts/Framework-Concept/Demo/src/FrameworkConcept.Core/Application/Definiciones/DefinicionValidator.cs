using System.Collections;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Core.Application.Definiciones;

/// <summary>
/// Valida un paso de una encuesta escrita como datos (P1). Es el equivalente, guiado por la definición,
/// de <see cref="Encuestas.EncuestaService.ValidarPaso"/> del caso base: misma forma de resultado (campo →
/// mensaje) para que la pantalla no cambie. Con la definición v1, ambos dan los mismos errores
/// (lo prueba FrameworkConcept.Core.Tests).
///
/// Los valores llegan como los entrega la pantalla:
///   texto y opción única → string; entero → int?; decimal → double?; opción múltiple → colección de string.
/// </summary>
public static class DefinicionValidator
{
    /// <param name="paso">Número de paso, desde 1 (como en el caso base).</param>
    public static IReadOnlyDictionary<string, string> ValidarPaso(
        DefinicionEncuesta definicion,
        int paso,
        IReadOnlyDictionary<string, object?> valores)
    {
        var errores = new Dictionary<string, string>();
        if (paso < 1 || paso > definicion.Pasos.Count) return errores;

        foreach (var pregunta in definicion.Pasos[paso - 1].Preguntas)
        {
            valores.TryGetValue(pregunta.Id, out var valor);
            if (!EsValida(pregunta, valor)) errores[pregunta.Id] = pregunta.MensajeDeError;
        }

        return errores;
    }

    /// <summary>¿Cumple la pregunta con este valor? Vacío vale sólo si la pregunta no es obligatoria.</summary>
    public static bool EsValida(PreguntaDeEncuesta pregunta, object? valor)
    {
        if (EstaVacio(valor)) return !pregunta.Obligatoria;

        var reglas = pregunta.Reglas ?? new ReglasDePregunta();
        return pregunta.Tipo switch
        {
            TipoDePregunta.Texto => valor is string texto && texto.Trim().Length >= (reglas.LargoMinimo ?? 0),
            TipoDePregunta.Entero => valor is int entero && DentroDelRango(entero, reglas),
            TipoDePregunta.Decimal => valor is double numero && DentroDelRango(numero, reglas),
            TipoDePregunta.OpcionUnica => valor is string clave && EsOpcionValida(pregunta, clave),
            TipoDePregunta.OpcionMultiple => valor is IEnumerable<string> claves
                && claves.Count() >= (reglas.MinimoDeOpciones ?? 0)
                && claves.All(c => EsOpcionValida(pregunta, c)),
            _ => false
        };
    }

    private static bool EstaVacio(object? valor) => valor switch
    {
        null => true,
        string texto => string.IsNullOrWhiteSpace(texto),
        ICollection coleccion => coleccion.Count == 0,
        IEnumerable<string> claves => !claves.Any(),
        _ => false
    };

    private static bool DentroDelRango(double numero, ReglasDePregunta reglas) =>
        (reglas.Minimo is null || numero >= reglas.Minimo) && (reglas.Maximo is null || numero <= reglas.Maximo);

    // Con opciones fijas, la clave tiene que estar entre ellas. Con un origen (por ejemplo, «localidades»), la
    // lista la conoce la pantalla, no la definición: alcanza con que haya un valor.
    private static bool EsOpcionValida(PreguntaDeEncuesta pregunta, string clave) =>
        pregunta.Opciones is null || pregunta.Opciones.Any(o => o.Clave == clave);
}
