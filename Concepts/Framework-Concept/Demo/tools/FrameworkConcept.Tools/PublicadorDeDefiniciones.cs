using System.Text.RegularExpressions;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Tools;

/// <summary>
/// «publicar-definicion»: copia una definición de encuesta al canal, después de comprobar las reglas de P13.
/// Corregir una encuesta publica SÓLO esto: no hace falta contenido nuevo (P1).
///
///   — La definición se lee con el tipo del contrato (<see cref="DefinicionEncuesta"/>): si no se lee, no se publica.
///     (La validación contra definiciones/…/esquema.json la hace la compuerta del PR; acá no hay validador de
///     JSON Schema.)
///   — P13.1 y P13.9: la versión es un entero que sólo sube; una versión ya publicada no cambia de contenido.
///   — P13.8: dentro de un encuestaId, ninguna pregunta publicada se borra ni cambia de tipo, y las
///     restricciones sólo se aflojan. Un cambio destructivo es un encuestaId nuevo.
///   — P6 (campo de la tienda): quien publica declara si la definición cambia los datos que se recogen o
///     comparten. Si cambia, la declaración de la tienda (Data safety) se actualiza antes de promover a prod.
/// </summary>
internal static partial class PublicadorDeDefiniciones
{
    public static int Publicar(Argumentos a)
    {
        var carpeta = CarpetaDelCanal.Desde(a);
        var origen = a.Requerida("definicion");
        var cambiaDatos = a.Opcion("cambia-datos") switch
        {
            "si" or "sí" => true,
            "no" => false,
            _ => throw new ErrorDeUso("falta --cambia-datos si|no: ¿la definición cambia los datos que se recogen o comparten? (P6)")
        };

        var bytes = File.ReadAllBytes(origen);
        DefinicionEncuesta nueva;
        try
        {
            nueva = DefinicionEncuesta.Leer(System.Text.Encoding.UTF8.GetString(bytes));
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or FormatException)
        {
            throw new PublicacionRechazada($"{origen} no es una definición de encuesta válida: {e.Message}");
        }

        ComprobarForma(nueva);

        var destino = carpeta.RutaDeLaDefinicion(nueva.EncuestaId, nueva.Version);
        if (File.Exists(destino))
        {
            if (CarpetaDelCanal.Sha256DeArchivo(destino) == CarpetaDelCanal.Sha256(bytes))
            {
                Console.WriteLine($"publicar-definicion: {nueva.EncuestaId} v{nueva.Version} ya estaba publicada, idéntica. Nada que hacer.");
                return 0;
            }
            throw new PublicacionRechazada($"P13.9: {nueva.EncuestaId} v{nueva.Version} ya está publicada con otro contenido. Revertir o corregir es publicar una versión nueva.");
        }

        var anteriores = Indexador.LeerDefiniciones(carpeta)
            .Select(d => d.Definicion)
            .Where(d => d.EncuestaId == nueva.EncuestaId)
            .ToList();
        var ultima = anteriores.MaxBy(d => d.Version);
        if (ultima is not null)
        {
            if (nueva.Version <= ultima.Version)
                throw new PublicacionRechazada($"P13.1: la versión {nueva.Version} no es mayor que la publicada ({ultima.Version}). El roll-forward nunca la baja.");
            var violaciones = ReglaDeEvolucion.Comparar(ultima, nueva);
            if (violaciones.Count > 0)
                throw new PublicacionRechazada("P13.8: cambio destructivo respecto de v" + ultima.Version + ":\n    - " +
                                               string.Join("\n    - ", violaciones) +
                                               "\n  Un cambio destructivo es un encuestaId nuevo.");
        }

        CarpetaDelCanal.EscribirAtomico(destino, bytes);
        carpeta.Anotar("publicar-definicion", new
        {
            encuestaId = nueva.EncuestaId,
            version = nueva.Version,
            compatibleConContenido = nueva.CompatibleConContenido.ToString(),
            cambiaLosDatosQueSeRecogen = cambiaDatos,
            sha256 = CarpetaDelCanal.Sha256(bytes)
        });

        Console.WriteLine($"publicar-definicion: {nueva.EncuestaId} v{nueva.Version} → {destino}");
        Console.WriteLine($"  compatibleConContenido: {nueva.CompatibleConContenido} · preguntas: {nueva.Preguntas.Count()}");
        if (cambiaDatos)
            Console.WriteLine("  ATENCIÓN (P6): cambia los datos que se recogen o comparten. Actualizá la declaración de la tienda (Data safety / privacidad de App Store Connect) antes de promover a prod.");
        Console.WriteLine("  Sigue: «firmar» para que el índice del canal la ofrezca. El teléfono la usa al empezar la próxima encuesta (P13.2).");
        return 0;
    }

    /// <summary>Lo mínimo para que el renderizador del contenido pueda dibujarla.</summary>
    private static void ComprobarForma(DefinicionEncuesta d)
    {
        var errores = new List<string>();
        if (!IdValido().IsMatch(d.EncuestaId)) errores.Add($"encuestaId «{d.EncuestaId}»: minúsculas, números y guiones");
        if (d.Version < 1) errores.Add("version tiene que ser 1 o mayor");
        if (d.Pasos.Count == 0) errores.Add("no tiene pasos");

        var ids = d.Preguntas.Select(p => p.Id).ToList();
        foreach (var repetido in ids.GroupBy(i => i).Where(g => g.Count() > 1))
            errores.Add($"la pregunta «{repetido.Key}» está repetida");

        foreach (var p in d.Preguntas.Where(p => p.Tipo is TipoDePregunta.OpcionUnica or TipoDePregunta.OpcionMultiple))
        {
            if ((p.Opciones is null || p.Opciones.Count == 0) && p.OrigenDeOpciones is null)
                errores.Add($"la pregunta «{p.Id}» es de opción y no tiene opciones ni origen");
        }

        if (errores.Count > 0)
            throw new PublicacionRechazada("la definición no está bien formada:\n    - " + string.Join("\n    - ", errores));
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex IdValido();
}

/// <summary>
/// P13.8 — La regla de evolución dentro de un mismo encuestaId: lo que se puede y lo que no.
///   Se puede: agregar preguntas, corregir textos, aflojar restricciones.
///   No se puede: borrar una pregunta, cambiar su tipo, volverla obligatoria, endurecer un mínimo o un máximo,
///   quitar una opción ya publicada (una respuesta tomada con ella quedaría inválida).
/// </summary>
internal static class ReglaDeEvolucion
{
    public static IReadOnlyList<string> Comparar(DefinicionEncuesta anterior, DefinicionEncuesta nueva)
    {
        var violaciones = new List<string>();
        var nuevas = nueva.Preguntas.ToDictionary(p => p.Id);

        foreach (var vieja in anterior.Preguntas)
        {
            if (!nuevas.TryGetValue(vieja.Id, out var actual))
            {
                violaciones.Add($"se borró la pregunta «{vieja.Id}»");
                continue;
            }
            if (actual.Tipo != vieja.Tipo)
                violaciones.Add($"«{vieja.Id}» cambió de tipo ({vieja.Tipo} → {actual.Tipo})");
            if (actual.Obligatoria && !vieja.Obligatoria)
                violaciones.Add($"«{vieja.Id}» pasó a ser obligatoria");

            var rv = vieja.Reglas;
            var ra = actual.Reglas;
            if (Sube(rv?.LargoMinimo, ra?.LargoMinimo)) violaciones.Add($"«{vieja.Id}»: sube el largo mínimo");
            if (Sube(rv?.Minimo, ra?.Minimo)) violaciones.Add($"«{vieja.Id}»: sube el mínimo");
            if (Baja(rv?.Maximo, ra?.Maximo)) violaciones.Add($"«{vieja.Id}»: baja el máximo");
            if (Sube(rv?.MinimoDeOpciones, ra?.MinimoDeOpciones)) violaciones.Add($"«{vieja.Id}»: sube el mínimo de opciones");

            var clavesNuevas = (actual.Opciones ?? []).Select(o => o.Clave).ToHashSet();
            foreach (var quitada in (vieja.Opciones ?? []).Select(o => o.Clave).Where(c => !clavesNuevas.Contains(c)))
                violaciones.Add($"«{vieja.Id}»: se quitó la opción «{quitada}»");
        }
        return violaciones;
    }

    // Una restricción que antes no estaba y ahora sí, también endurece.
    private static bool Sube(double? antes, double? ahora) => ahora is not null && (antes is null || ahora > antes);

    private static bool Baja(double? antes, double? ahora) => ahora is not null && (antes is null || ahora < antes);
}
