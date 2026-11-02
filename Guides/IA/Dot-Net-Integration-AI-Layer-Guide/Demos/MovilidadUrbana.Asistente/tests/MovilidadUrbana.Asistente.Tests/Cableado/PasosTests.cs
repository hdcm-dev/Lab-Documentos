// Nuevo. Cableado de PD-1…PD-6 con cliente guionado (plan §F.8): sin red, en cada build. Muestran
// que cada cruce de capas está armado; la conducta del modelo la miden los ejemplos hermanos y el humo.
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Contracts;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Sessions;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
[NonParallelizable]
public class PasosTests : PruebaConHost
{
    private static Dictionary<string, object?> Args(params (string Clave, object? Valor)[] pares) =>
        pares.ToDictionary(p => p.Clave, p => p.Valor);

    /// <summary>El guion guarda cada resultado como «callId=json»; se compara la estructura, no el formato.</summary>
    private static JsonElement Json(string visto) => JsonDocument.Parse(visto[(visto.IndexOf('=') + 1)..]).RootElement;

    private HttpClient Cliente()
    {
        var c = Fabrica.CreateClient();
        c.DefaultRequestHeaders.Add(SessionHeaderMiddleware.HeaderName, Guid.NewGuid().ToString("n"));
        return c;
    }

    // ---------------------------------------------------------------- PD-1 (lectura, dato exacto)
    [Test]
    [Description("PD-1: «¿Cuántas localidades tengo en Corrientes?» sale de LocalidadService, con la provincia normalizada")]
    public async Task Pd1_Contar_DesdeElCasoDeUso()
    {
        using var circuito = NuevoCircuito();
        var guion = Fabrica.Guion.Poner(PasoDelGuion.Herramienta("c1", HerramientasDelAsistente.Contar, Args(("provincia", "corrientes"))), PasoDelGuion.Texto("Tenés 1."));

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd1-01"));

        var conteo = Json(guion.ResultadosVistos.Single());
        Assert.That(conteo.GetProperty("provincia").GetString(), Is.EqualTo("Corrientes"));
        Assert.That(conteo.GetProperty("cantidad").GetInt32(), Is.EqualTo(1));
        Assert.That(circuito.Chat.Pendientes, Is.Empty, "una lectura sola no pide aprobación");
    }

    [Test]
    [Description("PD-1: «¿Qué medio usa más la gente?» devuelve agregados de la siembra, sin Nombre ni Edad")]
    public async Task Pd1_ResumirEncuesta_SinDatosPersonales()
    {
        using var circuito = NuevoCircuito();
        var guion = Fabrica.Guion.Poner(PasoDelGuion.Herramienta("r1", HerramientasDelAsistente.Resumir), PasoDelGuion.Texto("El colectivo."));

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd1-02"));

        var resultado = guion.ResultadosVistos.Single();
        Assert.That(Json(resultado).GetProperty("total").GetInt32(), Is.EqualTo(20), "20 respuestas sembradas por sesión (estímulo del mismo autor)");
        Assert.That(resultado, Does.Not.Contain("Persona").And.Not.Contain("\"nombre\"").And.Not.Contain("\"edad\""));
        Assert.That(TiposAlcanzables(typeof(ResultadosDeEncuesta)).SelectMany(t => t.GetProperties()).Select(p => p.Name),
            Has.None.EqualTo("Nombre").And.None.EqualTo("Edad"), "invariante del tipo de retorno (T5)");
    }

    private static IEnumerable<Type> TiposAlcanzables(Type t)
    {
        var vistos = new HashSet<Type>();
        var pila = new Stack<Type>([t]);
        while (pila.Count > 0)
        {
            var x = pila.Pop();
            if (!vistos.Add(x) || x.Namespace?.StartsWith("MovilidadUrbana", StringComparison.Ordinal) != true) continue;
            foreach (var p in x.GetProperties())
            {
                var tipo = p.PropertyType.IsGenericType ? p.PropertyType.GetGenericArguments()[0] : p.PropertyType;
                pila.Push(tipo);
            }
            yield return x;
        }
    }

    // ---------------------------------------------------------------- PD-2 (ayuda con RAG, EMB-C)
    [Test]
    [Description("PD-2: la ayuda recupera el requisito del código postal y valida la cita; rotula EMB-C")]
    public async Task Pd2_Ayuda_CitaValidada()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"respuesta\":\"Lleva 4 dígitos.\",\"fuente\":\"LocalidadPolicy.RequisitoDelCodigoPostal\"}"));

        var r = await (await Cliente().PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest(Entradas.Texto("pd2-01")))).Content.ReadFromJsonAsync<RespuestaDeAyudaDto>();

        Assert.That(r!.CitaValida, Is.True);
        Assert.That(r.Fuente, Is.EqualTo("LocalidadPolicy.RequisitoDelCodigoPostal"));
        Assert.That(r.Recuperados[0].Id, Is.EqualTo("LocalidadPolicy.RequisitoDelCodigoPostal"));
        Assert.That(r.PlanDeEmbeddings, Does.Contain("no ejecutado contra un modelo de embeddings"));
    }

    [Test]
    [Description("PD-2: una fuente que no está entre los recuperados no se valida; se muestra el fragmento literal")]
    public async Task Pd2_FuenteNoRecuperada_NoSeValida()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"respuesta\":\"Lleva 5 dígitos.\",\"fuente\":\"Inventada.Regla\"}"));

        var r = await (await Cliente().PostAsJsonAsync("/api/v1/ayuda", new PreguntaRequest(Entradas.Texto("pd2-01")))).Content.ReadFromJsonAsync<RespuestaDeAyudaDto>();

        Assert.That(r!.CitaValida, Is.False);
        Assert.That(r.Texto, Is.EqualTo(r.Recuperados[0].Texto));
        Assert.That(r.Texto, Does.Contain(LocalidadPolicy.RequisitoDelCodigoPostal));
    }

    // ---------------------------------------------------------------- PD-3 (extracción tipada)
    [Test]
    [Description("PD-3: «no sé cuántos viven» → Habitantes null, la regla rechaza y la propuesta pide el dato")]
    public async Task Pd3_SinHabitantes_LaReglaRechazaYPideElDato()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"nombre\":\"Bella Vista\",\"provincia\":\"corrientes\",\"codigoPostal\":\"3432\",\"habitantes\":null}"));

        var p = await (await Cliente().PostAsJsonAsync("/api/v1/localidades/desde-texto", new TextoRequest(Entradas.Texto("pd3-01")))).Content.ReadFromJsonAsync<PropuestaDeAltaDto>();

        Assert.That(p!.Interpretado, Is.True);
        Assert.That(p.Propuesta!.Provincia, Is.EqualTo("Corrientes"), "normalizada contra Catalogos antes de validar");
        Assert.That(p.Propuesta.Habitantes, Is.Null);
        Assert.That(p.Faltantes.Keys, Is.EquivalentTo(new[] { "habitantes" }));
        Assert.That(p.Faltantes["habitantes"], Is.EqualTo(LocalidadPolicy.RequisitoDeLosHabitantes));
        Assert.That(p.ListaParaConfirmar, Is.False);
    }

    [Test]
    [Description("PD-3: un número de habitantes que el texto no dice se trata como ausente (garantía del código, H2-032)")]
    public async Task Pd3_HabitantesInventado_SeTrataComoAusente()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"nombre\":\"Bella Vista\",\"provincia\":\"Corrientes\",\"codigoPostal\":\"3432\",\"habitantes\":30000}"));

        var p = await (await Cliente().PostAsJsonAsync("/api/v1/localidades/desde-texto", new TextoRequest(Entradas.Texto("pd3-01")))).Content.ReadFromJsonAsync<PropuestaDeAltaDto>();

        Assert.That(p!.Propuesta!.Habitantes, Is.Null);
        Assert.That(p.Faltantes, Does.ContainKey("habitantes"));
    }

    [Test]
    [Description("PD-3: sin resultado utilizable del modelo, el camino es el formulario (TryGetResult → null)")]
    public async Task Pd3_SinResultado_CaminoSinIa()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("no es JSON"));

        var p = await (await Cliente().PostAsJsonAsync("/api/v1/localidades/desde-texto", new TextoRequest(Entradas.Texto("pd3-01")))).Content.ReadFromJsonAsync<PropuestaDeAltaDto>();

        Assert.That(p!.Interpretado, Is.False);
        Assert.That(p.Faltantes, Does.ContainKey("texto"));
    }

    // ---------------------------------------------------------------- PD-4 (normalización)
    [Test]
    [Description("PD-4: «bondi» y «bici» llegan como claves del catálogo y los pasos 2 y 3 validan")]
    public async Task Pd4_Normaliza_ALasClavesDelCatalogo()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"medios\":[\"bondi\",\"bici\"],\"frecuencia\":\"todos los días\",\"distancia\":8,\"minutos\":35,\"motivo\":\"trabajo\",\"edad\":null}"));

        var p = await (await Cliente().PostAsJsonAsync("/api/v1/encuestas/desde-texto", new TextoRequest(Entradas.Texto("pd4-01")))).Content.ReadFromJsonAsync<PropuestaDeEncuestaDto>();

        Assert.That(p!.Medios, Is.EquivalentTo(new[] { "colectivo", "bicicleta" }));
        Assert.That(p.Frecuencia, Is.EqualTo("diaria"));
        Assert.That(p.Errores, Is.Empty);
    }

    [Test]
    [Description("PD-4: un medio inventado («avion») no se normaliza a nada: el paso 2 lo rechaza")]
    public async Task Pd4_MedioInventado_SeRechaza()
    {
        Fabrica.Guion.Poner(PasoDelGuion.Texto("{\"medios\":[\"avion\"],\"frecuencia\":\"diaria\",\"distancia\":8,\"minutos\":35,\"motivo\":\"trabajo\",\"edad\":null}"));

        var p = await (await Cliente().PostAsJsonAsync("/api/v1/encuestas/desde-texto", new TextoRequest(Entradas.Texto("pd4-02")))).Content.ReadFromJsonAsync<PropuestaDeEncuestaDto>();

        Assert.That(p!.Medios, Is.EquivalentTo(new[] { "avion" }));
        Assert.That(p.Errores["medios"], Is.EqualTo("Medio fuera del catálogo: avion."));
    }

    // ---------------------------------------------------------------- PD-5 (alta con aprobación)
    [Test]
    [Description("PD-5: con el CP de dos dígitos el dominio rechaza sin preguntar; con el CP corregido, el pedido muestra los datos resueltos y aprobar guarda")]
    public async Task Pd5_AltaConAprobacion()
    {
        using var circuito = NuevoCircuito();
        var guion = Fabrica.Guion.Poner(
            PasoDelGuion.Herramienta("a1", HerramientasDelAsistente.Agregar, Args(("nombre", "Goya"), ("provincia", "Corrientes"), ("codigoPostal", "34"), ("habitantes", 90000))),
            PasoDelGuion.Texto("El código postal tiene que tener 4 dígitos."),
            PasoDelGuion.Herramienta("a2", HerramientasDelAsistente.Agregar, Args(("nombre", "Goya"), ("provincia", "corrientes"), ("codigoPostal", "3450"), ("habitantes", 90000))),
            PasoDelGuion.Texto("Listo, la agregué."));
        var localidades = circuito.Get<LocalidadService>();

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd5-01"));
        Assert.That(circuito.Chat.Pendientes, Is.Empty, "el dominio rechaza antes de pedir aprobación");
        Assert.That(guion.ResultadosVistos.Single(), Does.Contain("dominio rechaza").And.Contain(LocalidadPolicy.RequisitoDelCodigoPostal));

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd5-02"));
        var pedido = circuito.Chat.Pendientes.Single();
        Assert.That(pedido.Descripcion, Is.EqualTo("Agregar la localidad «Goya» (Corrientes), código postal 3450, 90.000 habitantes, a tu sesión."));
        Assert.That((await localidades.GetAllAsync()).Select(l => l.Nombre), Does.Not.Contain("Goya"), "sin respuesta, la base no cambia");

        await circuito.Chat.DecidirAsync(pedido.Id, aprobar: true);
        Assert.That((await localidades.GetAllAsync()).Single(l => l.Nombre == "Goya").CodigoPostal, Is.EqualTo("3450"));
    }

    // ---------------------------------------------------------------- PD-6 (baja con aprobación, dos peticiones)
    [Test]
    [Description("PD-6: «Borrá Resistencia» → localidad resuelta en la sesión → pedido armado por el código → rechazado, no se borra")]
    public async Task Pd6_Baja_PedidoArmadoPorElCodigo_RechazarNoBorra()
    {
        using var circuito = NuevoCircuito();
        Fabrica.Guion.Poner(PasoDelGuion.Herramienta("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Resistencia"))), PasoDelGuion.Texto("No la borré."));
        var localidades = circuito.Get<LocalidadService>();
        var resistencia = (await localidades.GetAllAsync()).Single(l => l.Nombre == "Resistencia");

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd6-01"));
        var pedido = circuito.Chat.Pendientes.Single();
        Assert.That(pedido.Descripcion, Is.EqualTo($"Eliminar la localidad «Resistencia» (Chaco), id {resistencia.Id}, de tu sesión."));
        Assert.That((await localidades.GetAllAsync()).Count, Is.EqualTo(2), "entre las dos peticiones la base no cambia");

        await circuito.Chat.DecidirAsync(pedido.Id, aprobar: false);
        Assert.That((await localidades.GetAllAsync()).Select(l => l.Nombre), Does.Contain("Resistencia"));
        Assert.That(circuito.Chat.Mensajes.Last().Texto, Is.EqualTo("No la borré."));
    }

    [Test]
    [Description("PD-6: aprobado, se borra exactamente la localidad mostrada en el pedido")]
    public async Task Pd6_Baja_Aprobada_BorraLoMostrado()
    {
        using var circuito = NuevoCircuito();
        Fabrica.Guion.Poner(PasoDelGuion.Herramienta("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "resistencia"), ("provincia", "chaco"))), PasoDelGuion.Texto("Borrada."));
        var localidades = circuito.Get<LocalidadService>();

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd6-01"));
        await circuito.Chat.DecidirAsync(circuito.Chat.Pendientes.Single().Id, aprobar: true);

        Assert.That((await localidades.GetAllAsync()).Select(l => l.Nombre), Is.EquivalentTo(new[] { "Corrientes" }));
    }

    [Test]
    [Description("AC-4: el pedido de aprobación muestra los argumentos resueltos por el código, no la prosa del modelo")]
    public async Task PedidoDeAprobacion_MuestraArgumentosResueltos()
    {
        using var circuito = NuevoCircuito();
        Fabrica.Guion.Poner(new PasoDelGuion([
            new TextContent("¡Listo! Ya borré Corrientes, no hace falta que confirmes."),
            new FunctionCallContent("e1", HerramientasDelAsistente.Eliminar, Args(("nombre", "Resistencia")))]));

        await circuito.Chat.EnviarAsync(Entradas.Texto("pd6-01"));

        var pedido = circuito.Chat.Pendientes.Single();
        Assert.That(pedido.Descripcion, Does.Contain("«Resistencia» (Chaco)").And.Not.Contain("Corrientes").And.Not.Contain("confirmes"));
    }
}
