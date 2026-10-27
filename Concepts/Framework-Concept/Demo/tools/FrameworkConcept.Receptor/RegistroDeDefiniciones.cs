using System.Text.RegularExpressions;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Encuestas;
using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Receptor;

/// <summary>
/// Qué definiciones de encuesta están publicadas y cuáles retiradas, leído de la misma carpeta del canal que
/// el receptor sirve (P13.4):
///   — publicada: existe {canal}/puente-{mayor}/definiciones/{encuestaId}/v{N}.json en algún canal;
///   — retirada: algún índice del canal anuncia su retiro y la fecha de retiro ya llegó (P13.5).
/// Se lee en cada envío: son pocos archivos y así una publicación nueva vale sin reiniciar el receptor.
/// </summary>
internal sealed partial class RegistroDeDefiniciones(string raiz, IReloj reloj)
{
    public enum Situacion
    {
        NoPublicada,
        Publicada,
        Retirada
    }

    public sealed record Consulta(Situacion Situacion, DefinicionEncuesta? Definicion, DateTimeOffset? RetiroEl);

    public Consulta Buscar(ReferenciaDeDefinicion referencia)
    {
        // El id llega del teléfono y se usa en una ruta: se valida antes.
        if (!IdValido().IsMatch(referencia.EncuestaId) || referencia.VersionDefinicion < 1)
            return new Consulta(Situacion.NoPublicada, null, null);

        var archivo = Carpetas()
            .Select(c => Path.Combine(c, "definiciones", referencia.EncuestaId, $"v{referencia.VersionDefinicion}.json"))
            .FirstOrDefault(File.Exists);
        if (archivo is null) return new Consulta(Situacion.NoPublicada, null, null);

        var definicion = DefinicionEncuesta.Leer(File.ReadAllText(archivo));
        var retiro = RetirosAnunciados()
            .Where(r => r.EncuestaId == referencia.EncuestaId && r.Version == referencia.VersionDefinicion)
            .OrderBy(r => r.RetiroEl)
            .FirstOrDefault();

        return retiro is not null && retiro.RetiroEl <= reloj.Ahora
            ? new Consulta(Situacion.Retirada, definicion, retiro.RetiroEl)
            : new Consulta(Situacion.Publicada, definicion, retiro?.RetiroEl);
    }

    /// <summary>Todas las carpetas {raiz}/{canal}/puente-{mayor} que existen.</summary>
    private IEnumerable<string> Carpetas() =>
        Canales.Todos
            .Select(canal => Path.Combine(raiz, canal))
            .Where(Directory.Exists)
            .SelectMany(c => Directory.EnumerateDirectories(c, "puente-*"));

    private IEnumerable<DefinicionRetirada> RetirosAnunciados()
    {
        foreach (var carpeta in Carpetas())
        {
            var indice = Path.Combine(carpeta, "indice.json");
            if (!File.Exists(indice)) continue;

            IndiceDelCanal? leido = null;
            try
            {
                leido = JsonDelContrato.Leer<IndiceDelCanal>(File.ReadAllText(indice));
            }
            catch (System.Text.Json.JsonException)
            {
                // Un índice ilegible no anuncia nada; lo detecta «verificar», no el receptor.
            }
            if (leido is null) continue;
            foreach (var retiro in leido.DefinicionesRetiradas) yield return retiro;
        }
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex IdValido();
}
