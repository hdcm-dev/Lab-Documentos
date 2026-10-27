namespace FrameworkConcept.Core.Contracts.Encuestas;

/// <summary>
/// Una encuesta escrita como DATOS (P1, P13). Corregir una encuesta publica sólo una definición nueva, sin
/// contenido nuevo: las páginas Razor genéricas del contenido la dibujan.
///
/// Reglas (P13):
///   1. <see cref="Version"/> es un entero que sube en cada publicación; el roll-forward nunca lo baja.
///   8. Dentro de un mismo <see cref="EncuestaId"/>, el id y el tipo de una pregunta publicada no cambian.
///      Agregar preguntas, corregir textos y aflojar restricciones, sí. Un cambio destructivo es un
///      encuestaId nuevo.
/// El esquema JSON está en definiciones/encuesta-movilidad/esquema.json.
/// </summary>
/// <param name="EncuestaId">Identificador estable, en minúsculas con guiones.</param>
/// <param name="Version">Entero monótono, desde 1.</param>
/// <param name="CompatibleConContenido">P1 — «compatible-con-contenido»: <c>&gt;=A.B.C &lt;(A+1).0.0</c>.</param>
public sealed record DefinicionEncuesta(
    string EncuestaId,
    int Version,
    RangoDeVersion CompatibleConContenido,
    string Titulo,
    IReadOnlyList<PasoDeEncuesta> Pasos)
{
    public ReferenciaDeDefinicion Referencia => new(EncuestaId, Version);

    public static DefinicionEncuesta Leer(string json) => JsonDelContrato.Leer<DefinicionEncuesta>(json);

    public IEnumerable<PreguntaDeEncuesta> Preguntas => Pasos.SelectMany(p => p.Preguntas);
}

/// <summary>Un paso del asistente. Se avanza sólo si el paso no tiene errores.</summary>
public sealed record PasoDeEncuesta(string Id, string Titulo, IReadOnlyList<PreguntaDeEncuesta> Preguntas);

/// <summary>Una pregunta. <see cref="Id"/> es también el nombre del campo en la respuesta y en los errores.</summary>
/// <param name="Requisito">Lo que se muestra ANTES del intento (como las políticas del caso base).</param>
/// <param name="MensajeDeError">Lo que se muestra si el valor falta o no cumple las reglas.</param>
/// <param name="Opciones">Opciones fijas, para preguntas de opción.</param>
/// <param name="OrigenDeOpciones">O, en lugar de opciones fijas, de dónde salen: «localidades» = las del ABM.</param>
public sealed record PreguntaDeEncuesta(
    string Id,
    TipoDePregunta Tipo,
    string Etiqueta,
    string Requisito,
    string MensajeDeError,
    bool Obligatoria = true,
    ReglasDePregunta? Reglas = null,
    IReadOnlyList<OpcionDePregunta>? Opciones = null,
    string? OrigenDeOpciones = null);

public enum TipoDePregunta
{
    Texto,
    Entero,
    Decimal,
    OpcionUnica,
    OpcionMultiple
}

/// <summary>Restricciones. Las que no aplican al tipo se dejan sin valor.</summary>
public sealed record ReglasDePregunta(
    int? LargoMinimo = null,
    double? Minimo = null,
    double? Maximo = null,
    int? MinimoDeOpciones = null);

/// <summary>Una opción: la clave que se guarda y la etiqueta que se muestra (como los catálogos del caso base).</summary>
public sealed record OpcionDePregunta(string Clave, string Etiqueta);

/// <summary>P13.2 — Lo que cada respuesta guarda para saber con qué definición se tomó.</summary>
public sealed record ReferenciaDeDefinicion(string EncuestaId, int VersionDefinicion);
