// Nuevo: «resumir_encuesta», la lectura que alimenta al redactor y al verificador. Devuelve
// ResultadosDeEncuesta de _Comun, que por construcción no tiene Nombre ni Edad: la privacidad es un
// invariante del TIPO de retorno (H-005), probado por reflexión en las pruebas. Por eso este ejemplo
// no tiene un «revisor de privacidad»: no hay nada que revisar.
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MovilidadUrbana.Asistente.Application.Encuestas;

namespace MovilidadUrbana.Asistente.Application.Resumenes;

public sealed class FuenteDeResultados(ResultadosDeEncuestaService servicio)
{
    public const string Nombre = "resumir_encuesta";

    private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>Los agregados de la sesión: cuentas deterministas, sin modelo.</summary>
    public Task<ResultadosDeEncuesta> ResumirEncuestaAsync(CancellationToken cancellationToken = default) =>
        servicio.ResumirAsync(cancellationToken);

    /// <summary>
    /// El texto de grounding que recibe el modelo (y que el registro guarda en <c>contexto</c>): el total,
    /// los medios pedidos y el porcentaje de cada medio del catálogo. Frecuencias y motivos no van: el
    /// pedido no los usa y el prompt queda corto (clase K, plan §E.5).
    /// </summary>
    public static string Contexto(ResultadosDeEncuesta r, IReadOnlyList<string> mediosPedidos) =>
        JsonSerializer.Serialize(new DatosDelResumen(
            Nombre, r.Total, [.. mediosPedidos],
            [.. r.Medios.Select(m => new MedioDelResumen(m.Clave, m.Etiqueta, m.Cantidad, m.Porcentaje))]), Json);
}

/// <summary>Lo que se serializa en el contexto. Tampoco tiene campos de persona.</summary>
public sealed record DatosDelResumen(
    [property: JsonPropertyName("funcion")] string Funcion,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("medios_pedidos")] List<string> MediosPedidos,
    [property: JsonPropertyName("medios")] List<MedioDelResumen> Medios);

public sealed record MedioDelResumen(
    [property: JsonPropertyName("clave")] string Clave,
    [property: JsonPropertyName("etiqueta")] string Etiqueta,
    [property: JsonPropertyName("cantidad")] int Cantidad,
    [property: JsonPropertyName("porcentaje")] double Porcentaje);
