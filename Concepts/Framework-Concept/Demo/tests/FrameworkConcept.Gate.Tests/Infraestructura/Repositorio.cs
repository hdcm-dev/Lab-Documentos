namespace FrameworkConcept.Gate.Tests.Infraestructura;

/// <summary>
/// Dónde están los archivos que la compuerta revisa.
///
/// <para><b>La demo</b>: la carpeta que contiene <c>FrameworkConcept.Demo.sln</c>. Se busca subiendo desde la
/// carpeta de salida de las pruebas (bin/...), o se fuerza con la variable <c>FC_DEMO_ROOT</c>.</para>
///
/// <para><b>La base del PR</b>: una copia de la demo tal como está en la rama de destino (main), es decir, lo
/// que ya se publicó. La prepara quien corre la compuerta (el workflow o <c>scripts/ci-local.sh</c>) y la
/// pasa en <c>FC_GATE_BASE</c>. Sin base, las pruebas que comparan contra lo publicado no se pueden hacer:
/// en integración continua (<c>CI=true</c>) eso es un rojo; en una corrida a mano, se ignoran con aviso.</para>
/// </summary>
public static class Repositorio
{
    public const string Solucion = "FrameworkConcept.Demo.sln";

    private static readonly Lazy<string> RaizPerezosa = new(BuscarRaiz);

    /// <summary>Raíz de la demo (ruta absoluta).</summary>
    public static string Raiz => RaizPerezosa.Value;

    /// <summary>Ruta absoluta de un archivo de la demo, a partir de su ruta relativa con «/».</summary>
    public static string Ruta(string relativa) => Path.Combine(Raiz, relativa.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>¿Corre en integración continua? GitHub Actions y ci-local.sh definen CI=true.</summary>
    public static bool EnIntegracionContinua =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// La base del PR, o <c>null</c> si no se configuró. Una ruta relativa se resuelve contra la raíz de la
    /// demo (así funciona igual en el host y dentro del contenedor, donde la demo está en /work).
    /// </summary>
    public static string? Base
    {
        get
        {
            var valor = Environment.GetEnvironmentVariable("FC_GATE_BASE");
            if (string.IsNullOrWhiteSpace(valor)) return null;
            return Path.IsPathRooted(valor) ? valor : Path.GetFullPath(Path.Combine(Raiz, valor));
        }
    }

    /// <summary>
    /// Devuelve la base del PR o corta la prueba: rojo en CI, «ignorada» a mano. Si la carpeta existe pero no
    /// trae la demo (el PR que la crea), devuelve la carpeta igual: quien llama decide qué significa.
    /// </summary>
    public static string BaseObligatoria()
    {
        var baseDelPr = Base;
        const string comoSeArma = "La arma scripts/ci-local.sh (o el workflow) con la demo de la rama de destino y la pasa en FC_GATE_BASE.";

        if (baseDelPr is null)
        {
            if (EnIntegracionContinua)
                Assert.Fail("CI=true y falta FC_GATE_BASE: sin la base del PR no se puede comparar con lo publicado. " + comoSeArma);
            Assert.Ignore("Sin FC_GATE_BASE no hay base del PR para comparar. " + comoSeArma);
        }
        if (!Directory.Exists(baseDelPr))
            Assert.Fail($"FC_GATE_BASE apunta a «{baseDelPr}», que no existe. " + comoSeArma);

        return baseDelPr!;
    }

    private static string BuscarRaiz()
    {
        var forzada = Environment.GetEnvironmentVariable("FC_DEMO_ROOT");
        if (!string.IsNullOrWhiteSpace(forzada))
            return Path.GetFullPath(forzada);

        // AppContext.BaseDirectory (y no TestContext) porque también se usa al descubrir los casos.
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, Solucion)))
                return dir.FullName;
        }
        throw new InvalidOperationException(
            $"No encontré {Solucion} subiendo desde {AppContext.BaseDirectory}. Definí FC_DEMO_ROOT.");
    }
}
