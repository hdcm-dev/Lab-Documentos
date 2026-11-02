// Nuevo: pruebas sin red del estímulo de 05 (plan §E.1, regla de estímulo (i)–(iii)).
using System.Text.Json;
using System.Text.RegularExpressions;
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Domain.Rules;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class PadronYEntradasTests
{
    private static string Ruta(params string[] partes) => Path.Combine([AppContext.BaseDirectory, .. partes]);

    [Test]
    public void Padron_ArchivoCongelado_EsLaSalidaDelGenerador()
    {
        var texto = File.ReadAllText(Ruta("corpus", "padron-de-localidades.v1.json"));
        Assert.That(texto, Is.EqualTo(PadronDeLocalidades.Serializar(PadronDeLocalidades.Generar())));
    }

    [Test]
    public void Padron_SetentaRegistros_QuePasanLocalidadRules()
    {
        var padron = PadronDeLocalidades.Leer(Ruta("corpus", "padron-de-localidades.v1.json"));

        Assert.That(padron, Has.Count.EqualTo(70));
        Assert.That(padron.Select(l => l.Nombre), Is.EqualTo(Enumerable.Range(1, 70).Select(i => $"Localidad {i:00}")));
        Assert.That(padron, Has.All.Matches<Application.Localidades.LocalidadModel>(l =>
            LocalidadRules.NombreValido(l.Nombre) && LocalidadRules.ProvinciaValida(l.Provincia) &&
            LocalidadRules.CodigoPostalValido(l.CodigoPostal) && LocalidadRules.HabitantesValidos(l.Habitantes)));
    }

    [Test]
    public void Estimulos_LlevanElRotulo()
    {
        Assert.That(File.ReadAllText(Ruta("corpus", "padron-de-localidades.v1.json")), Does.Contain("\"rotulo\": \"escrito para el ejemplo\""));
        using var doc = JsonDocument.Parse(File.ReadAllText(Ruta("entradas.v1.json")));
        Assert.That(doc.RootElement.GetProperty("rotulo").GetString(), Is.EqualTo("escrito para el ejemplo"));
        Assert.That(PrefijoEstable.Construir(""), Does.Contain("escrito para el ejemplo"));
    }

    [Test]
    public void Entradas_SeisPreguntas_SoloConValoresDeCatalogos()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Ruta("entradas.v1.json")));
        var textos = doc.RootElement.GetProperty("entradas").EnumerateArray().Select(e => e.GetProperty("texto").GetString()!).ToList();

        Assert.That(textos, Has.Count.EqualTo(6));
        // Toda localidad nombrada es del padrón y toda provincia nombrada está en Catalogos (regla (i)).
        foreach (var t in textos)
        {
            foreach (Match m in Regex.Matches(t, @"Localidad (\d{2})"))
            {
                Assert.That(int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), Is.InRange(1, 70), t);
            }
        }
        var provinciasNombradas = new[] { "Mendoza", "Chaco", "Entre Ríos" };
        Assert.That(provinciasNombradas, Is.SubsetOf(Catalogos.Provincias));
        Assert.That(textos.Count(t => provinciasNombradas.Any(t.Contains)), Is.EqualTo(3));
    }
}
