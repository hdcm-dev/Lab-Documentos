// Nuevo (ejemplo 14). Sin servidor de modelos: todas corren en la fase build. La prueba central es sobre
// los METADATOS que arma cada marco (plan §E.2: «nombre, descripción y parámetros coinciden»).
using System.Text.Json;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public sealed class AnotacionesTests
{
    private static IEnumerable<TestCaseData> Casos() =>
        Entradas.Leer().Entradas.Select(e => new TestCaseData(e).SetName($"Entrada_{e.Id}"));

    [TestCaseSource(nameof(Casos))]
    public async Task LasDosDefiniciones_DanLaMismaFirmaYElMismoResultado(EntradaDeLlamada entrada)
    {
        var c = await Corrida.CompararAsync(LasDosDefiniciones.Crear(), entrada);

        Assert.That(Corrida.Invariantes(entrada.Id, c), Is.Empty);
    }

    [Test]
    public void Metadatos_NombreDescripcionYParametros_Coinciden()
    {
        var d = LasDosDefiniciones.Crear();

        var meai = Comparador.FirmaDe(d.Meai);
        var sk = Comparador.FirmaDe(d.Sk);

        Assert.That(sk, Is.EqualTo(meai));
        Assert.That(meai.Nombre, Is.EqualTo("contar_localidades"));
        Assert.That(meai.Descripcion, Does.StartWith("Cuenta cuántas localidades"));
        Assert.That(meai.Parametros, Has.Count.EqualTo(1));
        Assert.That(meai.Parametros[0], Is.EqualTo(new ParametroDeFirma(
            "provincia", "string",
            "Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.",
            true)));
    }

    [Test]
    public void ElCancellationToken_NoLlegaAlModelo_EnNingunaDeLasDos()
    {
        var d = LasDosDefiniciones.Crear();

        Assert.That(d.Meai.JsonSchema.GetProperty("properties").EnumerateObject().Select(p => p.Name), Is.EqualTo(new[] { "provincia" }));
        Assert.That(d.Sk.Metadata.Parameters.Select(p => p.Name), Is.EqualTo(new[] { "provincia" }));
    }

    [Test]
    public void EsquemaDelParametro_EsElMismoJson()
    {
        var d = LasDosDefiniciones.Crear();

        var meai = Comparador.EsquemaDelParametro(d.Meai, Corrida.Parametro);
        var sk = Comparador.EsquemaDelParametro(d.Sk, Corrida.Parametro);

        Assert.That(sk, Is.Not.Null);
        Assert.That(JsonElement.DeepEquals(meai, sk!.Value), Is.True, $"M.E.AI {meai.GetRawText()} · SK {sk.Value.GetRawText()}");
    }

    [Test]
    public void ElPlugin_AgrupaLaFuncionBajoSuNombre()
    {
        var d = LasDosDefiniciones.Crear();

        Assert.That(d.Sk.PluginName, Is.EqualTo(PluginSk.NombreDelPlugin));
        Assert.That(d.Kernel.Plugins[PluginSk.NombreDelPlugin][HerramientaMeai.Nombre], Is.SameAs(d.Sk));
    }

    // Observado con SK 1.81.0 (y lo que §9.7 le advierte a quien migra): la KernelFunction ES un AIFunction,
    // pero con el nombre calificado por el plugin. Montada tal cual en ChatOptions.Tools, el modelo vería
    // «Localidades_contar_localidades», no «contar_localidades».
    [Test]
    public void LaKernelFunctionComoAIFunction_LlevaElNombreDelPlugin()
    {
        var d = LasDosDefiniciones.Crear();

        Assert.That(d.Sk, Is.InstanceOf<Microsoft.Extensions.AI.AIFunction>());
        Assert.That(((Microsoft.Extensions.AI.AIFunction)d.Sk).Name, Is.EqualTo($"{PluginSk.NombreDelPlugin}_{HerramientaMeai.Nombre}"));
        Assert.That(d.Meai.Name, Is.EqualTo(HerramientaMeai.Nombre));
    }

    // Observado con SK 1.81.0: el esquema de la función entera (AIFunction.JsonSchema de la KernelFunction)
    // trae una clave «condition» con la descripción de la función, que el de M.E.AI no tiene. El de los
    // parámetros es idéntico. Si una versión nueva de SK lo cambia, esta prueba avisa y el README se corrige.
    [Test]
    public void ElEsquemaDeSk_TraeUnaClaveCondition_QueElDeMeaiNoTiene()
    {
        var d = LasDosDefiniciones.Crear();
        var sk = ((Microsoft.Extensions.AI.AIFunction)d.Sk).JsonSchema;

        Assert.That(sk.TryGetProperty("condition", out var condicion), Is.True);
        Assert.That(condicion.GetString(), Is.EqualTo(d.Meai.Description));
        Assert.That(d.Meai.JsonSchema.TryGetProperty("condition", out _), Is.False);
        Assert.That(JsonElement.DeepEquals(sk.GetProperty("properties"), d.Meai.JsonSchema.GetProperty("properties")), Is.True);
    }

    // Lo que vuelve al modelo no es el mismo texto: M.E.AI omite las propiedades null («error» en un conteo
    // válido); SK devuelve el objeto. Normalizados, son iguales.
    [Test]
    public async Task ElResultadoCrudo_DifiereEnLosNull_YNormalizadoEsIgual()
    {
        var d = LasDosDefiniciones.Crear();
        var c = await Corrida.CompararAsync(d, Entradas.Leer().Entradas.First(x => x.Id == "e01"));

        Assert.That(c.ResultadoMeai.Crudo, Does.Not.Contain("\"error\""));
        Assert.That(c.ResultadoSk.Crudo, Does.Contain("\"error\":null"));
        Assert.That(JsonElement.DeepEquals(c.ResultadoMeai.Normalizado, c.ResultadoSk.Normalizado), Is.True);
    }

    [Test]
    public async Task Invariantes_DetectanFirmaYResultadoDistintos()
    {
        var d = LasDosDefiniciones.Crear();
        var e = Entradas.Leer().Entradas.First(x => x.Id == "e01");
        var c = await Corrida.CompararAsync(d, e);

        var otraDescripcion = c with { FirmaSk = c.FirmaSk with { Descripcion = "Cuenta localidades." } };
        var otroParametro = c with { FirmaSk = c.FirmaSk with { Parametros = [c.FirmaSk.Parametros[0] with { Requerido = false }] } };
        var otroResultado = c with { ResultadoSk = ResultadoDeInvocacion.De(new { provincia = "Corrientes", cantidad = 2, error = (string?)null }) };

        Assert.That(Corrida.Invariantes("x", otraDescripcion), Has.Some.Contains("las firmas difieren"));
        Assert.That(Corrida.Invariantes("x", otroParametro), Has.Some.Contains("las firmas difieren"));
        Assert.That(Corrida.Invariantes("x", otroResultado), Has.Some.Contains("los resultados difieren"));
    }

    // Prueba de cableado del estímulo (plan §E.1, (i)): toda provincia de una entrada no negativa está en
    // Catalogos; la negativa declarada no lo está.
    [Test]
    public void Estimulo_ValoresDeCatalogo_YNegativoDeclarado()
    {
        var archivo = Entradas.Leer();
        Assert.That(archivo.Rotulo, Is.EqualTo("escrito para el ejemplo"));

        foreach (var e in archivo.Entradas)
        {
            var provincia = e.Argumentos[Corrida.Parametro];
            Assert.That(Catalogos.Provincias.Contains(provincia), Is.EqualTo(!e.Negativo), e.Id);
            Assert.That(e.Esperado.Error, Is.EqualTo(e.Negativo), e.Id);
        }
    }
}
