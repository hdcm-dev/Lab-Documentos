// Nuevo (ejemplo 10, §8.6). LA VERSIÓN QUE GANA: la encuesta de tres pasos como una función. Los pasos
// se conocen de antemano, no hay modelo que elija el camino y la validación ya es del caso de uso
// (ValidarPaso). Un for alcanza: «si podés escribir una función, hacelo» (N4 P3; N6 P7, C8).
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Application.Encuestas;

public static class EncuestaComoFuncion
{
    public static ResultadoDeEncuesta Completar(Persona persona, EncuestaService servicio)
    {
        var modelo = new EncuestaModel();
        var intentos = new int[EncuestaRules.TotalDePasos];
        var rechazos = new List<string>();

        for (var paso = 1; paso <= EncuestaRules.TotalDePasos; paso++)
        {
            while (true)
            {
                var respuesta = persona.Contestar(paso);
                if (respuesta is null)
                    return new ResultadoDeEncuesta(false, paso, intentos, rechazos, ResultadoDeEncuesta.DatosDe(modelo));

                intentos[paso - 1]++;
                respuesta.AplicarA(modelo);
                var errores = servicio.ValidarPaso(paso, modelo);
                if (errores.Count == 0) break;
                rechazos.AddRange(errores.Keys.Order(StringComparer.Ordinal).Select(c => $"{paso}.{c}"));
            }
        }
        return new ResultadoDeEncuesta(true, EncuestaRules.TotalDePasos, intentos, rechazos, ResultadoDeEncuesta.DatosDe(modelo));
    }
}
