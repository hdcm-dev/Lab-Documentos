// Nuevo. Capa única de opciones del proveedor (plan §E.1, H-007): el 100 % de las llamadas que salen
// hacia el servidor —chat con herramientas, extracción y ayuda— llevan razonamiento apagado y num_ctx.
using System.Net.Http.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Contracts;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Sessions;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
[NonParallelizable]
public class OpcionesTests : PruebaConHost
{
    [Test]
    [Description("Toda llamada del host sale con ReasoningEffort.None, temperatura 0, semilla y num_ctx explícito")]
    public async Task CapaUnica_TodaLlamada()
    {
        Fabrica.Espia.OpcionesVistas.Clear();
        var api = Fabrica.CreateClient();
        api.DefaultRequestHeaders.Add(SessionHeaderMiddleware.HeaderName, Guid.NewGuid().ToString("n"));

        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"respuesta\":\"4 dígitos.\",\"fuente\":\"LocalidadPolicy.RequisitoDelCodigoPostal\"}"));
        await api.PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest("¿Cuántos dígitos lleva el código postal?"));
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"nombre\":\"Goya\",\"provincia\":\"Corrientes\",\"codigoPostal\":\"3450\",\"habitantes\":90000}"));
        await api.PostAsJsonAsync("/api/v1/localidades/desde-texto", new TextoRequest("Agregá Goya, Corrientes, CP 3450, con 90000 habitantes."));
        using (var circuito = NuevoCircuito())
        {
            Fabrica.Guion.Poner(PasoDelGuion.Herramienta("c1", HerramientasDelAsistente.Contar, new Dictionary<string, object?> { ["provincia"] = "Chaco" }), PasoDelGuion.Texto("Una."));
            await circuito.Chat.EnviarAsync("¿Cuántas localidades tengo en Chaco?");
        }

        var vistas = Fabrica.Espia.OpcionesVistas;
        Assert.That(vistas, Has.Count.EqualTo(4));
        Assert.That(vistas.Select(o => o?.Reasoning?.Effort), Is.All.EqualTo(ReasoningEffort.None));
        Assert.That(vistas.Select(o => o?.Temperature), Is.All.EqualTo(0f));
        Assert.That(vistas.Select(o => o?.Seed), Is.All.Not.Null);
        Assert.That(vistas.Select(OpcionesDelProveedor.NumCtxDe), Is.All.EqualTo(ConfiguracionDelModelo.NumCtxPorDefecto));
    }
}
