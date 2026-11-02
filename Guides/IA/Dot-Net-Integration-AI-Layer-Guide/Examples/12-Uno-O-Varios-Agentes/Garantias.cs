// Nuevo: garantías del código del ejemplo 12. No dependen del modelo: se comprueban en toda corrida,
// con --fake o contra qwen3.5:4b, y las pruebas de cableado las repiten.
using System.Reflection;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Resumenes;
using MovilidadUrbana.Asistente.Application.Workflows;

namespace MovilidadUrbana.Asistente;

public static class Garantias
{
    private static readonly string[] CamposDePersona = ["Nombre", "Edad"];

    /// <summary>
    /// H-005: lo que devuelve resumir_encuesta (y lo que se serializa al modelo) no tiene Nombre ni Edad
    /// en ningún nivel. Se mira el TIPO, por reflexión, no una salida en particular.
    /// </summary>
    public static IReadOnlyList<string> ResumirEncuestaSinDatosDePersona()
    {
        var retorno = typeof(FuenteDeResultados).GetMethod(nameof(FuenteDeResultados.ResumirEncuestaAsync))!.ReturnType;
        var fallas = new List<string>();
        foreach (var raiz in new[] { retorno, typeof(DatosDelResumen) })
        {
            fallas.AddRange(CamposDePersonaEn(raiz).Select(c => $"{c}: resumir_encuesta expone un dato de persona"));
        }
        if (retorno != typeof(Task<ResultadosDeEncuesta>)) fallas.Add($"resumir_encuesta devuelve {retorno}, no Task<ResultadosDeEncuesta>");
        return fallas;
    }

    /// <summary>«Tipo.Propiedad» de cada Nombre o Edad alcanzable desde <paramref name="raiz" />.</summary>
    public static IReadOnlyList<string> CamposDePersonaEn(Type raiz) =>
        [.. Propiedades(raiz, []).Where(p => CamposDePersona.Contains(p.Propiedad, StringComparer.OrdinalIgnoreCase))
            .Select(p => $"{p.Tipo}.{p.Propiedad}")];

    /// <summary>El verificador no recibe ningún modelo: ni IChatClient ni AIAgent en su constructor.</summary>
    public static IReadOnlyList<string> VerificadorSinModelo()
    {
        var parametros = typeof(VerificadorExecutor).GetConstructors().SelectMany(c => c.GetParameters()).Select(p => p.ParameterType).ToList();
        return parametros.Any(t => typeof(IChatClient).IsAssignableFrom(t) || typeof(AIAgent).IsAssignableFrom(t))
            ? ["el verificador recibe un modelo: la cuenta tiene que ser determinista (escalón −1)"]
            : [];
    }

    private static IEnumerable<(string Tipo, string Propiedad)> Propiedades(Type tipo, HashSet<Type> vistos)
    {
        // Desenvuelve Task<T>, listas y arreglos hasta llegar a los records del dominio.
        if (tipo.IsGenericType && tipo.GetGenericArguments() is { Length: 1 } args &&
            (tipo.GetGenericTypeDefinition() == typeof(Task<>) || typeof(System.Collections.IEnumerable).IsAssignableFrom(tipo)))
        {
            foreach (var p in Propiedades(args[0], vistos)) yield return p;
            yield break;
        }
        if (tipo.IsPrimitive || tipo == typeof(string) || tipo.Namespace?.StartsWith("System", StringComparison.Ordinal) == true || !vistos.Add(tipo))
        {
            yield break;
        }
        foreach (var propiedad in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            yield return (tipo.Name, propiedad.Name);
            foreach (var p in Propiedades(propiedad.PropertyType, vistos)) yield return p;
        }
    }
}
