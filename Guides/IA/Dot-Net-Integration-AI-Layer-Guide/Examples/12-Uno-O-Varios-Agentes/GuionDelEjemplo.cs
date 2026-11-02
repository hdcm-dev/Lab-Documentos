// Nuevo: lo que «responde» el guion del modo --fake (sin red), una cola por variante. En e01 el primer
// borrador trae un porcentaje equivocado (colectivo 25 % en lugar de 20 %): el verificador lo detecta y
// hay UNA vuelta, al redactor («un-agente») o al corrector («dos-agentes»). En e02 el primer borrador
// ya coincide y no hay vuelta. Los valores salen de la siembra rotulada de _Comun (estímulo del mismo autor).
using MovilidadUrbana.Asistente.Application.Workflows;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente;

public static class GuionDelEjemplo
{
    public const string E01Equivocado = "El 25 % de las respuestas usa colectivo y el 35 % usa bicicleta.";
    public const string E01Corregido = "El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta.";
    public const string E02 = "El tren lo eligió el 30 % de las respuestas y el auto particular, el 20 %.";

    /// <summary>
    /// Las respuestas en el orden en que el workflow llama al modelo dentro de una variante. En las dos
    /// variantes el texto es el mismo: lo que cambia es QUIÉN hace la segunda llamada de e01.
    /// </summary>
    public static IEnumerable<PasoDelGuion> Pasos(Variante variante, IEnumerable<Entrada> entradas)
    {
        _ = variante;
        foreach (var e in entradas)
        {
            switch (e.Id)
            {
                case "e01":
                    yield return PasoDelGuion.Texto(E01Equivocado);
                    yield return PasoDelGuion.Texto(E01Corregido);
                    break;
                case "e02":
                    yield return PasoDelGuion.Texto(E02);
                    break;
                default:
                    yield return PasoDelGuion.Texto("Sin datos para ese pedido.");
                    break;
            }
        }
    }
}
