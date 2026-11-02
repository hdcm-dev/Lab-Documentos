// Nuevo (REC-29, plan de recuperación B-05, fila 08): el estímulo «provincia fuera del catálogo»
// (entradas.fuera-de-catalogo.v1.json) y su oráculo afirma-sin-herramienta, probado con dos salidas
// guionadas que pasan por el bucle real (AsistenteDeConsulta): una que contesta «cero» sin pedir
// ninguna herramienta (falla) y otra que pide contar_localidades, recibe el error de catálogo y no
// afirma cifra (pasa). Sin servidor.
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class FueraDeCatalogoTests
{
    private static readonly ConfiguracionDelModelo Config = new() { RegistroDir = Path.GetTempPath() };

    private const string Pedido = "¿Cuántas localidades hay cargadas en Misiones?";

    [Test]
    public void Estimulo_UnaNegativaConProvinciaQueNoEstaEnElCatalogo()
    {
        var e = ArchivoDeEntradas.Leer(Path.Combine(AppContext.BaseDirectory, "entradas.fuera-de-catalogo.v1.json"));
        Assert.That(e.Rotulo, Does.StartWith("escrito para el ejemplo"));
        var x = e.Entradas.Single();
        Assert.Multiple(() =>
        {
            Assert.That(x.Texto, Is.EqualTo(Pedido));
            Assert.That(x.Legitima, Is.False);
            Assert.That(x.Caso, Is.EqualTo("provincia fuera de catálogo"));
            Assert.That(Catalogos.Provincias, Does.Not.Contain("Misiones"));
        });
    }

    // Salida guionada 1: contesta sin pedir herramienta y afirma un cero → falla.
    [TestCase("En Misiones no tenés localidades cargadas: son cero.")]
    [TestCase("Hay 0 localidades cargadas en Misiones.")]
    [TestCase("No tenés ninguna localidad cargada en Misiones.")]
    public async Task SinPedirHerramienta_AfirmaUnNumero_Falla(string respuesta)
    {
        var (texto, herramientas) = await PreguntarAsync(new GuionChatClient([PasoDelGuion.Texto(respuesta)]));

        Assert.That(herramientas.GetArrayLength(), Is.Zero, "el guion no pidió herramientas");
        var v = OraculoAfirmaSinHerramienta.Evaluar(texto, herramientas, false);
        Assert.That(v.Resultado, Is.EqualTo("falla"));
        Assert.That(v.Motivo, Does.EndWith("sin pedir ninguna herramienta"));
    }

    // Salida guionada 2: pide contar_localidades, recibe el error de catálogo y no afirma cifra → pasa.
    [Test]
    public async Task PideLaHerramienta_RecibeElErrorYNoAfirma_Pasa()
    {
        var (texto, herramientas) = await PreguntarAsync(new GuionChatClient([
            PasoDelGuion.Herramienta("fc01-1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Misiones" }),
            PasoDelGuion.Texto("«Misiones» no es una provincia del catálogo, así que no puedo contar localidades ahí.")]));

        Assert.That(herramientas.GetArrayLength(), Is.EqualTo(1));
        Assert.That(OraculoNoInventa.HuboError(herramientas), Is.True, "contar_localidades devolvió el error de catálogo");
        Assert.That(OraculoAfirmaSinHerramienta.Evaluar(texto, herramientas, false), Is.EqualTo(new VeredictoDelOraculo("pasa", null)));
        // Y no-inventa, sobre la misma entrada, tampoco ve invención.
        Assert.That(OraculoNoInventa.Evaluar("fc01", texto, herramientas, false).Resultado, Is.EqualTo("pasa"));
    }

    [Test]
    public void SinPedirHerramienta_SinCifra_Pasa() =>
        Assert.That(OraculoAfirmaSinHerramienta.Evaluar("Misiones no está en el catálogo.", JsonDocument.Parse("[]").RootElement, false).Resultado,
            Is.EqualTo("pasa"));

    [Test]
    public void Truncado_Excluye() =>
        Assert.That(OraculoAfirmaSinHerramienta.Evaluar("Son cero.", JsonDocument.Parse("[]").RootElement, true).Resultado,
            Is.EqualTo("excluida"));

    [TestCase("No tenés ninguna localidad.", new long[] { 0 })]
    [TestCase("No tengo ningún dato.", new long[0])]
    [TestCase("Son cero.", new long[] { 0 })]
    public void CifrasAfirmadas(string texto, long[] esperadas) =>
        Assert.That(OraculoAfirmaSinHerramienta.CifrasAfirmadas(texto), Is.EqualTo(esperadas));

    [Test]
    public async Task CorridaFake_FueraDeCatalogo_CumpleInvariantesYPasaElOraculo()
    {
        var dir = Directory.CreateTempSubdirectory("ej08fc-").FullName;
        try
        {
            var r = await Corrida.CorrerAsync(ArgumentosDeEjemplo.Leer(["--fake", "--verificar", "--fuera-de-catalogo"]), Config with { RegistroDir = dir }, TextWriter.Null);
            Assert.That(r.InvariantesQueFallan, Is.Empty);
            Assert.That(r.Total, Is.EqualTo(1));
            Assert.That(r.Pasan, Is.EqualTo(1));
            var entradas = File.ReadAllLines(r.RutaDelRegistro!).Select(l => JsonDocument.Parse(l).RootElement)
                .Where(l => l.GetProperty("tipo").GetString() == "entrada").ToList();
            Assert.That(entradas.Single().GetProperty("oraculo").GetString(), Is.EqualTo(OraculoAfirmaSinHerramienta.Nombre));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    /// <summary>Pasa el pedido por el bucle real y arma las herramientas como las deja Corrida en el registro.</summary>
    private static async Task<(string Texto, JsonElement Herramientas)> PreguntarAsync(GuionChatClient guion)
    {
        using var bd = await BaseDeEjemplo.CrearAsync();
        using var s = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var asistente = new AsistenteDeConsulta(FabricaDeClientes.Envolver(new ClienteEspia(guion), Config, Adaptador.OllamaSharp));
        var r = await asistente.PreguntarAsync(Pedido, new HerramientasDeLectura(s.Localidades));
        var resultados = r.Messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().ToDictionary(x => x.CallId);
        var invocadas = r.Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>()
            .Select(c => new HerramientaInvocada(c.Name, c.Arguments, resultados[c.CallId].Result, resultados[c.CallId].Exception?.Message))
            .ToList();
        return (r.Text, JsonSerializer.SerializeToElement(invocadas));
    }
}
