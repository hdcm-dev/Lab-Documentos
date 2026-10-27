using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Puente;
using static FrameworkConcept.Core.Contracts.Puente.ContratoDelPuente;

namespace FrameworkConcept.Gate.Tests.Contrato;

/// <summary>Cuánto tiene que subir la versión del puente por un cambio del contrato.</summary>
public enum NivelDeCambio
{
    /// <summary>El contrato no cambió: la versión tampoco tiene que cambiar.</summary>
    Ninguno = 0,

    /// <summary>Cambio compatible hacia atrás: sube el MENOR.</summary>
    Menor = 1,

    /// <summary>Cambio que rompe al contenido ya publicado: sube el MAYOR.</summary>
    Mayor = 2
}

/// <summary>Qué subió de la versión del puente.</summary>
public enum SubidaDeVersion
{
    Bajo,
    Igual,
    Parche,
    Menor,
    Mayor
}

/// <summary>El nivel que corresponde y, para el mensaje de la prueba, cada diferencia encontrada.</summary>
public sealed record ResultadoDeClasificacion(NivelDeCambio Nivel, IReadOnlyList<string> Motivos);

/// <summary>
/// Compara dos instantáneas del contrato (antes y después) y clasifica el cambio según P3:
///
///   MENOR: método nuevo, entrada nueva del catálogo, campo opcional nuevo en una respuesta.
///   MAYOR: método quitado, parámetro obligatorio nuevo, cambio de tipo o de forma de un sobre,
///          cambio del transporte.
///
/// Lo que P3 no nombra se trata como MAYOR (es lo que no rompe a nadie por error), salvo dos lecturas
/// que se declaran donde se aplican: un evento nuevo cuenta como «método nuevo» y la versión de una
/// entrada del catálogo sigue su propio SemVer.
/// La versión del puente NO se compara acá: es justamente lo que se decide con este resultado.
/// </summary>
public static class ClasificadorDeCambios
{
    public static ResultadoDeClasificacion Clasificar(Instantanea antes, Instantanea despues)
    {
        var motivos = new List<string>();
        var nivel = NivelDeCambio.Ninguno;

        void Anotar(NivelDeCambio n, string motivo)
        {
            motivos.Add($"[{n}] {motivo}");
            if (n > nivel) nivel = n;
        }

        // Transporte: hybridwebview.js, la ruta de invocación, el método nativo, los eventos y el mayor de MAUI.
        if (antes.Transporte != despues.Transporte)
            Anotar(NivelDeCambio.Mayor, $"cambió el transporte: {antes.Transporte} → {despues.Transporte}");

        CompararMetodos(antes.Metodos, despues.Metodos, Anotar);
        CompararEventos(antes.Eventos, despues.Eventos, Anotar);
        CompararCatalogo(antes.Catalogo, despues.Catalogo, Anotar);

        return new ResultadoDeClasificacion(nivel, motivos);
    }

    /// <summary>Qué componente de la versión del puente subió entre antes y después.</summary>
    public static SubidaDeVersion Subida(VersionSemantica antes, VersionSemantica despues)
    {
        var c = despues.CompareTo(antes);
        if (c < 0) return SubidaDeVersion.Bajo;
        if (c == 0) return SubidaDeVersion.Igual;
        if (despues.Mayor > antes.Mayor) return SubidaDeVersion.Mayor;
        if (despues.Menor > antes.Menor) return SubidaDeVersion.Menor;
        return SubidaDeVersion.Parche;
    }

    /// <summary>
    /// La regla de P3 para la versión: si el contrato no cambió, la versión no sube; si cambió, sube
    /// exactamente el componente que corresponde. Devuelve el problema, o <c>null</c> si está bien.
    /// </summary>
    public static string? ProblemaDeVersion(NivelDeCambio requerido, VersionSemantica antes, VersionSemantica despues)
    {
        var subida = Subida(antes, despues);
        var esperada = requerido switch
        {
            NivelDeCambio.Mayor => SubidaDeVersion.Mayor,
            NivelDeCambio.Menor => SubidaDeVersion.Menor,
            _ => SubidaDeVersion.Igual
        };
        if (subida == esperada) return null;

        return (requerido, subida) switch
        {
            (_, SubidaDeVersion.Bajo) => $"la versión del puente bajó ({antes} → {despues}); nunca baja (P7)",
            (NivelDeCambio.Ninguno, _) => $"la versión del puente subió ({antes} → {despues}) pero el contrato no cambió (P3: «si la versión subió y no difieren, rojo»)",
            (_, SubidaDeVersion.Igual) => $"el contrato cambió y la versión del puente sigue en {despues}; corresponde subir el {requerido.ToString().ToUpperInvariant()} (P3)",
            _ => $"el contrato tiene un cambio {requerido.ToString().ToUpperInvariant()} y la versión subió el {subida.ToString().ToUpperInvariant()} ({antes} → {despues}) (P3)"
        };
    }

    private static void CompararMetodos(
        IReadOnlyList<FilaDeMetodo> antes, IReadOnlyList<FilaDeMetodo> despues, Action<NivelDeCambio, string> anotar)
    {
        var nuevos = despues.ToDictionary(m => m.Nombre, StringComparer.Ordinal);
        foreach (var viejo in antes)
        {
            if (!nuevos.TryGetValue(viejo.Nombre, out var nuevo))
            {
                anotar(NivelDeCambio.Mayor, $"se quitó el método «{viejo.Nombre}»");
                continue;
            }
            if (!string.Equals(viejo.Capacidad, nuevo.Capacidad, StringComparison.Ordinal))
                anotar(NivelDeCambio.Mayor, $"«{viejo.Nombre}» cambió de capacidad: {viejo.Capacidad ?? "(ninguna)"} → {nuevo.Capacidad ?? "(ninguna)"}");

            CompararCampos($"«{viejo.Nombre}», parámetros", viejo.Parametros, nuevo.Parametros, esRespuesta: false, anotar);
            CompararCampos($"«{viejo.Nombre}», respuesta", viejo.Respuesta, nuevo.Respuesta, esRespuesta: true, anotar);
        }

        foreach (var nuevo in despues.Where(m => antes.All(a => a.Nombre != m.Nombre)))
            anotar(NivelDeCambio.Menor, $"método nuevo «{nuevo.Nombre}»");
    }

    private static void CompararEventos(
        IReadOnlyList<FilaDeEvento> antes, IReadOnlyList<FilaDeEvento> despues, Action<NivelDeCambio, string> anotar)
    {
        var nuevos = despues.ToDictionary(e => e.Nombre, StringComparer.Ordinal);
        foreach (var viejo in antes)
        {
            if (!nuevos.TryGetValue(viejo.Nombre, out var nuevo))
            {
                anotar(NivelDeCambio.Mayor, $"se quitó el evento «{viejo.Nombre}»");
                continue;
            }
            // Los parámetros de un evento los LEE el contenido: se comparan como una respuesta.
            CompararCampos($"evento «{viejo.Nombre}»", viejo.Parametros, nuevo.Parametros, esRespuesta: true, anotar);
        }

        // P3 no nombra eventos. Lectura: un evento nuevo es como un método nuevo; el contenido viejo no lo
        // escucha y no se rompe. Por eso MENOR.
        foreach (var nuevo in despues.Where(e => antes.All(a => a.Nombre != e.Nombre)))
            anotar(NivelDeCambio.Menor, $"evento nuevo «{nuevo.Nombre}»");
    }

    private static void CompararCampos(
        string donde, IReadOnlyList<CampoDelSobre> antes, IReadOnlyList<CampoDelSobre> despues, bool esRespuesta,
        Action<NivelDeCambio, string> anotar)
    {
        var nuevos = despues.ToDictionary(c => c.Nombre, StringComparer.Ordinal);
        foreach (var viejo in antes)
        {
            if (!nuevos.TryGetValue(viejo.Nombre, out var nuevo))
                anotar(NivelDeCambio.Mayor, $"{donde}: se quitó «{viejo.Nombre}»");
            else if (viejo.Tipo != nuevo.Tipo)
                anotar(NivelDeCambio.Mayor, $"{donde}: «{viejo.Nombre}» cambió de tipo {viejo.Tipo} → {nuevo.Tipo}");
            else if (viejo.Opcional != nuevo.Opcional)
                anotar(NivelDeCambio.Mayor, $"{donde}: «{viejo.Nombre}» pasó de {Opcionalidad(viejo)} a {Opcionalidad(nuevo)} (cambio de forma)");
        }

        foreach (var nuevo in despues.Where(c => antes.All(a => a.Nombre != c.Nombre)))
        {
            // P3: «campo opcional nuevo en una respuesta» es lo único que entra como menor. Un parámetro nuevo,
            // aunque sea opcional, o un campo obligatorio nuevo en la respuesta, cambian la forma del sobre.
            if (esRespuesta && nuevo.Opcional)
                anotar(NivelDeCambio.Menor, $"{donde}: campo opcional nuevo «{nuevo.Nombre}»");
            else
                anotar(NivelDeCambio.Mayor, $"{donde}: campo {Opcionalidad(nuevo)} nuevo «{nuevo.Nombre}»");
        }
    }

    private static void CompararCatalogo(
        IReadOnlyList<EntradaVersionada> antes, IReadOnlyList<EntradaVersionada> despues, Action<NivelDeCambio, string> anotar)
    {
        var nuevas = despues.ToDictionary(e => e.Id, StringComparer.Ordinal);
        foreach (var vieja in antes)
        {
            if (!nuevas.TryGetValue(vieja.Id, out var nueva))
            {
                anotar(NivelDeCambio.Mayor, $"se quitó la entrada del catálogo «{vieja.Id}»");
                continue;
            }
            if (vieja.Version.Equals(nueva.Version)) continue;

            // P7: cada entrada tiene su propio SemVer, y el contenido la pide con un rango con techo (P12).
            // Lectura: si la entrada cambia de mayor, el contenido publicado deja de encontrarla → MAYOR del
            // puente; si sube su menor o su parche, sigue dentro del rango → MENOR. Si baja, es un retroceso → MAYOR.
            var nivel = nueva.Version.Mayor == vieja.Version.Mayor && nueva.Version > vieja.Version
                ? NivelDeCambio.Menor
                : NivelDeCambio.Mayor;
            anotar(nivel, $"la entrada del catálogo «{vieja.Id}» pasó de {vieja.Version} a {nueva.Version}");
        }

        foreach (var nueva in despues.Where(e => antes.All(a => a.Id != e.Id)))
            anotar(NivelDeCambio.Menor, $"entrada nueva del catálogo «{nueva.Id}» {nueva.Version}");
    }

    private static string Opcionalidad(CampoDelSobre c) => c.Opcional ? "opcional" : "obligatorio";
}
