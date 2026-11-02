// Nuevo: el caso de uso del ejemplo. El modelo propone texto; el normalizador lo pasa a claves de
// Catalogos; ValidarPaso decide. Nada de lo que el modelo diga llega a la encuesta sin las reglas.
using MovilidadUrbana.Asistente.Application.Encuestas;

namespace MovilidadUrbana.Asistente.Application.Normalizacion;

/// <summary>
/// Puerto: texto libre de la persona → clave de catálogo, o el texto plegado si no hay clave
/// (vacío si no queda nada: un texto en blanco no es un medio).
/// </summary>
public interface INormalizadorDeCatalogo
{
    string NormalizarMedio(string texto);
    string NormalizarFrecuencia(string texto);
}

public sealed class NormalizacionDeMovilidad(INormalizadorDeCatalogo normalizador)
{
    private readonly EncuestaService _encuestas = new(null!); // ValidarPaso no toca el repositorio

    public ResultadoDeNormalizacion Decidir(PropuestaDeMovilidad? propuesta)
    {
        var modelo = new EncuestaModel();
        if (propuesta is null)
        {
            // El modelo no devolvió el JSON pedido: el paso queda vacío y ValidarPaso lo dice.
            return new(modelo, _encuestas.ValidarPaso(2, modelo), PropuestaValida: false);
        }

        // Cada medio propuesto entra a la encuesta: normalizado si hay clave, tal cual si no.
        // Nunca se descarta en silencio: un medio inventado tiene que producir un error (hueco H4).
        // «"medios": null» es JSON válido y System.Text.Json deja la lista en null (el «= []» del record
        // no rige cuando el JSON dice null), y la lista puede traer null adentro: los dos casos se toman
        // como «no nombró ese medio», y si no queda ninguno, ValidarPaso da el error de medios.
        foreach (var texto in propuesta.Medios ?? [])
        {
            if (texto is null) continue;
            var clave = normalizador.NormalizarMedio(texto);
            if (clave.Length > 0) modelo.AlternarMedio(clave, true); // «  » o «¿?» plegados quedan vacíos
        }
        modelo.Frecuencia = propuesta.Frecuencia is null ? string.Empty : normalizador.NormalizarFrecuencia(propuesta.Frecuencia);

        return new(modelo, _encuestas.ValidarPaso(2, modelo), PropuestaValida: true);
    }
}
