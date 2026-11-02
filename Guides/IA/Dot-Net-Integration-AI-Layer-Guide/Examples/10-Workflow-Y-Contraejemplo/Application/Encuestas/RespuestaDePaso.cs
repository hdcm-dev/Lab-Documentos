// Nuevo (ejemplo 10). Lo que contesta la persona en un paso de la encuesta. Lo usan las DOS versiones
// (la función y el workflow), así que la única diferencia entre ellas es cómo se encadenan los pasos.
using System.Text.Json.Serialization;

namespace MovilidadUrbana.Asistente.Application.Encuestas;

/// <summary>Una respuesta a un paso. Sólo se leen los campos del paso que indica <see cref="Paso"/>.</summary>
public sealed record RespuestaDePaso
{
    [JsonPropertyName("paso")] public int Paso { get; init; }

    // Paso 1
    [JsonPropertyName("nombre")] public string? Nombre { get; init; }
    [JsonPropertyName("edad")] public int? Edad { get; init; }
    [JsonPropertyName("localidad")] public string? Localidad { get; init; }

    // Paso 2
    [JsonPropertyName("medios")] public List<string>? Medios { get; init; }
    [JsonPropertyName("frecuencia")] public string? Frecuencia { get; init; }

    // Paso 3
    [JsonPropertyName("distancia")] public double? Distancia { get; init; }
    [JsonPropertyName("minutos")] public int? Minutos { get; init; }
    [JsonPropertyName("motivo")] public string? Motivo { get; init; }

    /// <summary>Estímulo negativo declarado: lleva a propósito un valor que ValidarPaso tiene que rechazar.</summary>
    [JsonPropertyName("negativo")] public bool Negativo { get; init; }

    /// <summary>Vuelca la respuesta en el modelo del caso base, como lo hace el asistente de tres pasos de la Web.</summary>
    public void AplicarA(EncuestaModel modelo)
    {
        switch (Paso)
        {
            case 1:
                modelo.Nombre = Nombre ?? string.Empty;
                modelo.Edad = Edad;
                modelo.Localidad = Localidad ?? string.Empty;
                break;
            case 2:
                modelo.Medios.Clear();
                foreach (var m in Medios ?? []) modelo.AlternarMedio(m, true);
                modelo.Frecuencia = Frecuencia ?? string.Empty;
                break;
            case 3:
                modelo.Distancia = Distancia;
                modelo.Minutos = Minutos;
                modelo.Motivo = Motivo ?? string.Empty;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Paso), Paso, "La encuesta tiene tres pasos.");
        }
    }
}

/// <summary>
/// La persona del otro lado: entrega sus respuestas en orden. Vive FUERA del workflow (es la UI), por eso
/// lo que ya contestó antes de una interrupción no se vuelve a contestar después de rehidratar.
/// </summary>
public sealed class Persona(IEnumerable<RespuestaDePaso> respuestas)
{
    private readonly Queue<RespuestaDePaso> _pendientes = new(respuestas);

    /// <summary>La próxima respuesta para el paso pedido, o null si la persona dejó la encuesta.</summary>
    public RespuestaDePaso? Contestar(int paso)
    {
        if (_pendientes.Count == 0) return null;
        var r = _pendientes.Dequeue();
        return r.Paso == paso
            ? r
            : throw new InvalidOperationException($"La entrada contesta el paso {r.Paso} cuando se pidió el {paso}.");
    }
}
