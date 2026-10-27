using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Receptor;

/// <summary>Cómo quedó una respuesta en el receptor.</summary>
public enum EstadoDeRecepcion
{
    /// <summary>Se tomó con una definición publicada y no retirada, y sus valores son válidos.</summary>
    Aceptada,

    /// <summary>
    /// No se pudo aceptar (definición retirada o nunca publicada, o valores inválidos). NO se borra: queda
    /// visible, para que el usuario la vea en el teléfono y alguien decida qué hacer (P5, P13.5).
    /// </summary>
    CartaMuerta
}

/// <param name="Version">Versión del recurso: el ETag. Sube en cada modificación con If-Match (D-04).</param>
/// <param name="Reenvios">Cuántas veces llegó de nuevo la misma respuesta y no se duplicó.</param>
/// <param name="Dueno">Folio 038, P4 — «sub» del token con que llegó (null si el receptor corre sin --oidc-emisor).</param>
/// <param name="Usuario">preferred_username del mismo token, para que el operador lo lea.</param>
public sealed record RespuestaRecibida(
    string Clave,
    int Version,
    EstadoDeRecepcion Estado,
    string? Motivo,
    IReadOnlyDictionary<string, string>? Errores,
    string? Aviso,
    DateTimeOffset RecibidaEn,
    DateTimeOffset? ModificadaEn,
    int Reenvios,
    EnvioDeRespuesta Envio,
    string? Dueno = null,
    string? Usuario = null);

/// <summary>
/// Las respuestas recibidas, en memoria y en un archivo JSON (para que sobrevivan a reiniciar el receptor).
/// Un receptor didáctico: un solo proceso, un candado, un archivo. Un servidor real usaría su base.
/// </summary>
internal sealed class AlmacenDeRespuestas
{
    private readonly object _candado = new();
    private readonly string _archivo;
    private readonly Dictionary<string, RespuestaRecibida> _respuestas;

    public AlmacenDeRespuestas(string carpeta)
    {
        Directory.CreateDirectory(carpeta);
        _archivo = Path.Combine(carpeta, "respuestas.json");
        _respuestas = File.Exists(_archivo)
            ? JsonDelContrato.Leer<List<RespuestaRecibida>>(File.ReadAllText(_archivo)).ToDictionary(r => r.Clave)
            : [];
    }

    /// <summary>Todo el trabajo sobre una clave se hace dentro del candado: dos envíos iguales a la vez no duplican.</summary>
    public T Con<T>(Func<Dictionary<string, RespuestaRecibida>, (T Resultado, bool Cambio)> accion)
    {
        lock (_candado)
        {
            var (resultado, cambio) = accion(_respuestas);
            if (cambio) Guardar();
            return resultado;
        }
    }

    public IReadOnlyList<RespuestaRecibida> Todas()
    {
        lock (_candado) return [.. _respuestas.Values.OrderBy(r => r.RecibidaEn)];
    }

    private void Guardar()
    {
        var temporal = _archivo + ".tmp";
        File.WriteAllText(temporal, JsonDelContrato.Escribir(_respuestas.Values.OrderBy(r => r.RecibidaEn).ToList()));
        File.Move(temporal, _archivo, overwrite: true);
    }
}
