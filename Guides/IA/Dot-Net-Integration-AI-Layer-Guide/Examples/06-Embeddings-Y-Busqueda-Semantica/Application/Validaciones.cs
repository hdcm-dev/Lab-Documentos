// Nuevo: invariantes de código del corpus y del oro (plan §E.2). Si alguno falla, la corrida no
// llama al modelo y --verificar sale con 1.
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Application;

public static class Validaciones
{
    public static IEnumerable<string> DelCorpus(IReadOnlyList<Fragmento> corpus)
    {
        if (corpus.Count != 12) yield return $"el corpus tiene {corpus.Count} fragmentos, no 12";
        if (corpus.Count(f => f.Entidad == "Localidad") != 4) yield return "LocalidadPolicy no aporta 4 fragmentos";
        if (corpus.Count(f => f.Entidad == "Encuesta") != 8) yield return "EncuestaPolicy no aporta 8 fragmentos";
        if (corpus.Select(f => f.Id).Distinct().Count() != corpus.Count) yield return "hay ids de fragmento repetidos";
        foreach (var f in corpus.Where(f => !f.Id.Contains("Policy.", StringComparison.Ordinal)))
            yield return $"id sin calificar por clase: {f.Id}";
    }

    public static IEnumerable<string> DelOro(ConjuntoDeOro oro, IReadOnlyList<Fragmento> corpus)
    {
        var ids = corpus.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);
        if (oro.Plantilla != CorpusDePolicies.Plantilla) yield return $"el oro es de {oro.Plantilla} y el código genera {CorpusDePolicies.Plantilla}";
        if (oro.Rotulo != "escrito para el ejemplo") yield return "el oro no lleva el rótulo «escrito para el ejemplo»";
        var respondibles = oro.Preguntas.Where(p => p.Respondible).ToList();
        var no = oro.Preguntas.Where(p => !p.Respondible).ToList();
        if (respondibles.Count != 12) yield return $"{respondibles.Count} respondibles, no 12";
        if (no.Count != 4) yield return $"{no.Count} no respondibles, no 4";
        if (no.Count(p => p.Cercania == "cercana") != 2 || no.Count(p => p.Cercania == "lejana") != 2)
            yield return "las no respondibles no son 2 cercanas y 2 lejanas";
        // Una respondible por fragmento.
        var objetivos = respondibles.Select(p => p.Objetivo ?? string.Empty).ToList();
        if (!objetivos.ToHashSet().SetEquals(ids) || objetivos.Count != ids.Count) yield return "los objetivos no cubren cada fragmento una sola vez";
        foreach (var p in respondibles)
        {
            if (p.Relevantes.Count == 0 || !p.Relevantes.All(ids.Contains)) yield return $"{p.Id}: relevantes fuera del corpus";
            if (p.Objetivo is not null && !p.Relevantes.Contains(p.Objetivo)) yield return $"{p.Id}: el objetivo no está entre los relevantes";
        }
        if (respondibles.Count(p => p.Relevantes.Count == 2) != 1) yield return "debe haber exactamente una pregunta con dos relevantes (la del nombre)";
        foreach (var p in no.Where(p => p.Relevantes.Count > 0)) yield return $"{p.Id}: una no respondible no tiene relevantes";
        if (oro.Preguntas.Select(p => p.Id).Distinct().Count() != oro.Preguntas.Count) yield return "ids de pregunta repetidos";
        foreach (var falla in DelCatalogo(oro)) yield return falla;
    }

    /// <summary>Regla (i) del estímulo (plan §E.1): todo valor de catálogo que aparece en una pregunta pertenece a Catalogos.</summary>
    public static IEnumerable<string> DelCatalogo(ConjuntoDeOro oro)
    {
        var catalogo = Catalogos.Provincias
            .Concat(Catalogos.Medios.SelectMany(m => new[] { m.Clave, m.Etiqueta }))
            .Concat(Catalogos.Frecuencias.SelectMany(m => new[] { m.Clave, m.Etiqueta }))
            .Concat(Catalogos.Motivos.SelectMany(m => new[] { m.Clave, m.Etiqueta }))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var p in oro.Preguntas)
        {
            foreach (var v in p.ValoresDeCatalogo)
            {
                if (!catalogo.Contains(v)) yield return $"{p.Id}: «{v}» no está en Catalogos";
                if (!p.Texto.Contains(v, StringComparison.Ordinal)) yield return $"{p.Id}: declara «{v}» y el texto no lo nombra";
            }
        }
    }
}
