using System.Reflection;

namespace FrameworkConcept.Core.Contracts.Puente;

/// <summary>
/// La identidad del transporte. P3: el transporte es parte del contrato y su dueño es el cascarón.
/// Si cambia (por ejemplo, porque un PR sube el mayor de MAUI y con él hybridwebview.js), cambia el mayor
/// del puente. Por eso el mayor de MAUI entra en la instantánea.
///
/// <c>Eventos</c> se escribe «receptor | mecanismo»: el receptor es lo que el contenido expone (la función global
/// y su argumento) y el mecanismo es cómo el cascarón la llama. La compuerta los clasifica por separado
/// (ClasificadorDeCambios): cambiar el receptor rompe al contenido publicado (MAYOR); cambiar sólo el mecanismo,
/// con el mismo receptor, no (MENOR).
/// </summary>
public sealed record TransporteDelPuente(
    string Script,
    string RutaDeInvocacion,
    string MetodoNativo,
    string Eventos,
    int MauiMayor)
{
    /// <summary>
    /// El transporte de esta compilación. <c>MauiMayor</c> sale de <c>$(MauiVersion)</c> de
    /// Directory.Build.props, que el .csproj de Core graba en el ensamblado.
    /// </summary>
    public static TransporteDelPuente Actual { get; } = new(
        Script: "_framework/hybridwebview.js",              // condición C-2 de E01
        RutaDeInvocacion: "POST /__hwvInvokeDotNet",        // el servidor de contenido la deja pasar (E01)
        MetodoNativo: "Call(sobre)",                        // SetInvokeJavaScriptTarget, un solo método
        Eventos: "window.puenteRecibirEvento(texto) | script evaluado en la vista (ScriptEnLaVista)",
        MauiMayor: LeerMauiMayor());

    private static int LeerMauiMayor()
    {
        var valor = typeof(TransporteDelPuente).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "MauiVersion")?.Value;
        return VersionSemantica.TryParse(valor, out var v) ? v.Mayor : 0;
    }
}
