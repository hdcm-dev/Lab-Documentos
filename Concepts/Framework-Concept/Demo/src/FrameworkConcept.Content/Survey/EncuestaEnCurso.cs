using System.Globalization;
using System.Text.Json;
using FrameworkConcept.Core.Application.Definiciones;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Content.Survey;

/// <summary>
/// Una encuesta que se está cargando, guiada por su definición. No conoce ninguna pregunta en particular:
/// todo sale de <see cref="Definicion"/> (P1, encuestas como datos).
///
/// P13.2 — La definición queda fija desde que se empieza a cargar. Si mientras tanto llega otra versión,
/// ésta se termina con la suya; la nueva aplica a la próxima.
///
/// Los valores se guardan con el tipo que espera <see cref="DefinicionValidator"/>:
///   texto y opción única → string; entero → int?; decimal → double?; opción múltiple → HashSet&lt;string&gt;.
/// </summary>
public sealed class EncuestaEnCurso
{
    private readonly Dictionary<string, object?> _valores = [];

    public EncuestaEnCurso(DefinicionEncuesta definicion)
        : this(definicion, Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow, 1)
    {
    }

    private EncuestaEnCurso(DefinicionEncuesta definicion, string claveIdempotencia, DateTimeOffset iniciadaEn, int paso)
    {
        Definicion = definicion;
        ClaveIdempotencia = claveIdempotencia;
        IniciadaEn = iniciadaEn;
        Paso = paso;
    }

    public DefinicionEncuesta Definicion { get; }

    /// <summary>
    /// P5 — Se elige al empezar y no cambia: si el envío se repite (por ejemplo, se cortó a mitad), el
    /// cascarón y el receptor reconocen la misma respuesta y no la duplican.
    /// </summary>
    public string ClaveIdempotencia { get; }

    public DateTimeOffset IniciadaEn { get; }

    /// <summary>Paso actual, desde 1.</summary>
    public int Paso { get; private set; }

    public int TotalDePasos => Definicion.Pasos.Count;

    public PasoDeEncuesta PasoActual => Definicion.Pasos[Paso - 1];

    public bool EsUltimoPaso => Paso == TotalDePasos;

    /// <summary>Errores del último intento de avanzar, por id de pregunta.</summary>
    public IReadOnlyDictionary<string, string> Errores { get; private set; } = new Dictionary<string, string>();

    // --- Lectura y escritura de valores, por tipo ------------------------------------------------------------

    public string Texto(string id) => _valores.GetValueOrDefault(id) as string ?? string.Empty;

    public void PonerTexto(string id, string? valor) => _valores[id] = valor ?? string.Empty;

    /// <summary>El valor de un campo numérico tal como se muestra en el input (punto decimal).</summary>
    public string Numero(string id) => _valores.GetValueOrDefault(id) switch
    {
        int entero => entero.ToString(CultureInfo.InvariantCulture),
        double numero => numero.ToString(CultureInfo.InvariantCulture),
        _ => string.Empty
    };

    /// <summary>Un input de tipo número entrega el texto con punto decimal, sin importar el idioma.</summary>
    public void PonerNumero(PreguntaDeEncuesta pregunta, string? texto)
    {
        texto = texto?.Trim();
        _valores[pregunta.Id] = pregunta.Tipo == TipoDePregunta.Entero
            ? int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var entero) ? entero : null
            : double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var numero) ? numero : null;
    }

    public bool EstaElegida(string id, string clave) => Elegidas(id).Contains(clave);

    public void Alternar(string id, string clave, bool elegida)
    {
        var elegidas = Elegidas(id);
        if (elegida) elegidas.Add(clave);
        else elegidas.Remove(clave);
    }

    private HashSet<string> Elegidas(string id)
    {
        if (_valores.GetValueOrDefault(id) is not HashSet<string> elegidas)
        {
            elegidas = [];
            _valores[id] = elegidas;
        }
        return elegidas;
    }

    // --- Navegación ----------------------------------------------------------------------------------------

    /// <summary>
    /// Valida el paso actual con <see cref="DefinicionValidator"/> (las mismas reglas que las pruebas del
    /// dominio) y, si no hay errores, avanza. Devuelve si avanzó o, en el último paso, si está lista.
    /// </summary>
    public bool IntentarAvanzar()
    {
        Errores = DefinicionValidator.ValidarPaso(Definicion, Paso, _valores);
        if (Errores.Count > 0) return false;
        if (!EsUltimoPaso) Paso++;
        return true;
    }

    public void Retroceder()
    {
        Errores = new Dictionary<string, string>();
        if (Paso > 1) Paso--;
    }

    // --- Lo que se envía y lo que se muestra al terminar -------------------------------------------------------

    /// <summary>
    /// P13.2 — La respuesta lleva {encuestaId, versionDefinicion} de la definición con la que se tomó, y la
    /// versión del contenido para atribuir errores a versiones exactas (folio 023).
    /// </summary>
    public EnvioDeRespuesta ComoEnvio(VersionSemantica versionContenido) => new(
        ClaveIdempotencia,
        Definicion.EncuestaId,
        Definicion.Version,
        versionContenido,
        DateTimeOffset.UtcNow,
        Definicion.Preguntas
            .Where(p => _valores.ContainsKey(p.Id))
            .ToDictionary(p => p.Id, p => AJson(p, _valores[p.Id])));

    /// <summary>Filas «etiqueta: valor» para la ficha de revisión, en el orden de la definición.</summary>
    public IEnumerable<(string Etiqueta, string Valor)> Resumen(Func<PreguntaDeEncuesta, string, string>? etiquetaDeOpcion = null)
    {
        foreach (var pregunta in Definicion.Preguntas)
        {
            var valor = pregunta.Tipo switch
            {
                TipoDePregunta.OpcionMultiple => string.Join(", ", ElegidasEnOrden(pregunta).Select(c => Etiqueta(pregunta, c))),
                TipoDePregunta.OpcionUnica => Texto(pregunta.Id) is { Length: > 0 } c ? Etiqueta(pregunta, c) : string.Empty,
                TipoDePregunta.Entero or TipoDePregunta.Decimal => Numero(pregunta.Id),
                _ => Texto(pregunta.Id).Trim()
            };
            if (valor.Length > 0) yield return (pregunta.Etiqueta, valor);
        }

        string Etiqueta(PreguntaDeEncuesta pregunta, string clave) =>
            pregunta.Opciones?.FirstOrDefault(o => o.Clave == clave)?.Etiqueta
            ?? etiquetaDeOpcion?.Invoke(pregunta, clave)
            ?? clave;
    }

    // Las opciones elegidas se guardan en el orden de la definición, no en el de los toques (como el caso base).
    private IEnumerable<string> ElegidasEnOrden(PreguntaDeEncuesta pregunta)
    {
        var elegidas = Elegidas(pregunta.Id);
        return pregunta.Opciones is null
            ? elegidas.Order(StringComparer.Ordinal)
            : pregunta.Opciones.Select(o => o.Clave).Where(elegidas.Contains);
    }

    private JsonElement AJson(PreguntaDeEncuesta pregunta, object? valor) => pregunta.Tipo switch
    {
        TipoDePregunta.OpcionMultiple => JsonSerializer.SerializeToElement(ElegidasEnOrden(pregunta).ToArray(), JsonDelContrato.Opciones),
        TipoDePregunta.Texto => JsonSerializer.SerializeToElement((valor as string)?.Trim(), JsonDelContrato.Opciones),
        _ => JsonSerializer.SerializeToElement(valor, JsonDelContrato.Opciones)
    };

    // --- Borrador: la encuesta a medio cargar se guarda como documento, con su definición ----------------------

    /// <summary>
    /// Lo que se guarda al avanzar de paso. Lleva la definición entera: si cierran la app y llega otra
    /// versión, la encuesta se retoma con la definición con la que empezó (P13.2, P13.3).
    /// </summary>
    public sealed record Borrador(
        DefinicionEncuesta Definicion,
        string ClaveIdempotencia,
        DateTimeOffset IniciadaEn,
        int Paso,
        Dictionary<string, JsonElement> Valores);

    public Borrador ComoBorrador() => new(
        Definicion, ClaveIdempotencia, IniciadaEn, Paso,
        Definicion.Preguntas.Where(p => _valores.ContainsKey(p.Id)).ToDictionary(p => p.Id, p => AJson(p, _valores[p.Id])));

    public static EncuestaEnCurso DesdeBorrador(Borrador borrador)
    {
        var encuesta = new EncuestaEnCurso(borrador.Definicion, borrador.ClaveIdempotencia, borrador.IniciadaEn,
            Math.Clamp(borrador.Paso, 1, borrador.Definicion.Pasos.Count));

        foreach (var pregunta in borrador.Definicion.Preguntas)
        {
            if (!borrador.Valores.TryGetValue(pregunta.Id, out var json) || json.ValueKind == JsonValueKind.Null) continue;
            encuesta._valores[pregunta.Id] = pregunta.Tipo switch
            {
                TipoDePregunta.Entero => json.GetInt32(),
                TipoDePregunta.Decimal => json.GetDouble(),
                TipoDePregunta.OpcionMultiple => json.EnumerateArray().Select(e => e.GetString()!).ToHashSet(),
                _ => json.GetString()
            };
        }
        return encuesta;
    }
}
