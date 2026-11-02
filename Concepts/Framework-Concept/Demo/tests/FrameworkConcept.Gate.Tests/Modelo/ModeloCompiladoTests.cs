using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Data.Compilado;
using Microsoft.EntityFrameworkCore;

namespace FrameworkConcept.Gate.Tests.Modelo;

/// <summary>
/// P5 — El modelo compilado no se queda atrás. La app crea la base con SqlDeCreacion.Script (Data/Compilado/), que
/// genera scripts/modelo-compilado.sh. Si alguien cambia ContextoDelCascaron o Tablas.cs y no regenera, el cascarón
/// compila igual y crea el esquema VIEJO. Acá se compara el script versionado con el que EF arma hoy desde el modelo de
/// diseño (el mismo texto fuente, enlazado por tools/FrameworkConcept.Shell.Diseno): una columna o un índice nuevos lo
/// cambian. Los interceptores, en cambio, ya los vigila el compilador (se corren de línea y el cascarón no compila).
/// </summary>
[TestFixture]
public class ModeloCompiladoTests
{
    [Test]
    public void El_sql_de_creacion_versionado_es_el_que_genera_el_modelo_actual()
    {
        var opciones = new DbContextOptionsBuilder<ContextoDelCascaron>().UseSqlite("Data Source=:memory:").Options;
        using var contexto = new ContextoDelCascaron(opciones);

        Assert.That(Normalizar(SqlDeCreacion.Script), Is.EqualTo(Normalizar(contexto.Database.GenerateCreateScript())),
            "Data/Compilado/ quedó atrás del modelo: correr scripts/modelo-compilado.sh y versionar el resultado.");
    }

    /// <summary>Lo que el script de regeneración cambia de forma: renglones vacíos, sangría y fin de línea.</summary>
    private static string Normalizar(string sql) => string.Join('\n',
        sql.Replace("\r", "").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));
}
