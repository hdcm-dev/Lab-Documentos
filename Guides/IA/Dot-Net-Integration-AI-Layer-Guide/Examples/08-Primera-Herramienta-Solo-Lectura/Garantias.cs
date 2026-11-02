// Nuevo: las garantías de código del ejemplo 08 (plan §E.2, fila 08), las que comprueba --verificar.
// Ninguna depende de lo que conteste el modelo.
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente;

public static class Garantias
{
    /// <summary>Dos sesiones en paralelo (N6 Q4): cada una ve lo suyo y la propia nunca ve Goya.</summary>
    public static async Task<List<string>> SesionAjenaNuncaSeAlcanzaAsync(BaseDeEjemplo bd, CancellationToken cancellationToken = default)
    {
        var fallas = new List<string>();
        using var propia = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        using var ajena = bd.AbrirSesion(BaseDeEjemplo.SesionAjena);
        var hp = new HerramientasDeLectura(propia.Localidades);
        var ha = new HerramientasDeLectura(ajena.Localidades);

        var listas = await Task.WhenAll(hp.ListarLocalidadesAsync(cancellationToken), ha.ListarLocalidadesAsync(cancellationToken));
        var conteos = await Task.WhenAll(hp.ContarLocalidadesAsync("Corrientes", cancellationToken), ha.ContarLocalidadesAsync("Corrientes", cancellationToken));

        if (listas[0].Any(l => l.Nombre == "Goya")) fallas.Add("sesión propia: listar_localidades devolvió la localidad de la sesión ajena");
        if (listas[0].Count != 2) fallas.Add($"sesión propia: se esperaban las 2 sembradas y hay {listas[0].Count}");
        if (!listas[1].Any(l => l.Nombre == "Goya")) fallas.Add("sesión ajena: falta Goya (la prueba no probaría nada)");
        if (conteos[0].Cantidad != 1 || conteos[1].Cantidad != 2) fallas.Add($"contar_localidades(Corrientes): propia {conteos[0].Cantidad}, ajena {conteos[1].Cantidad}; se esperaba 1 y 2");
        if (listas[0].Select(l => l.Id).Intersect(listas[1].Select(l => l.Id)).Any()) fallas.Add("las dos sesiones comparten ids de localidad");
        return fallas;
    }

    /// <summary>
    /// Un guion que pide la misma herramienta sin parar: MaximumIterationsPerRequest corta el bucle.
    /// Se usa un guion también en la corrida con modelo: el corte es del código, no del modelo.
    /// </summary>
    public static async Task<(List<string> Fallas, int Ejecutadas, int Llamadas)> ElBucleSeCortaAsync(ConfiguracionDelModelo config, CancellationToken cancellationToken = default)
    {
        var fallas = new List<string>();
        using var bd = await BaseDeEjemplo.CrearAsync(cancellationToken);
        using var sesion = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
        var guion = new GuionChatClient(
            Enumerable.Range(1, 20).Select(i => PasoDelGuion.Herramienta($"rep{i}", HerramientasDeLectura.Listar)), "Corte.");
        var asistente = new AsistenteDeConsulta(FabricaDeClientes.Envolver(guion, config, Adaptador.OllamaSharp));
        await asistente.PreguntarAsync("¿Cuántas localidades tengo?", new HerramientasDeLectura(sesion.Localidades), cancellationToken);

        var ejecutadas = guion.ResultadosVistos.Count;
        if (ejecutadas > AsistenteDeConsulta.MaximoDeIteraciones)
        {
            fallas.Add($"corte: se ejecutaron {ejecutadas} herramientas con tope {AsistenteDeConsulta.MaximoDeIteraciones}");
        }
        if (guion.Llamadas.Count > AsistenteDeConsulta.MaximoDeIteraciones + 1)
        {
            fallas.Add($"corte: {guion.Llamadas.Count} llamadas al «modelo» con tope {AsistenteDeConsulta.MaximoDeIteraciones}");
        }
        return (fallas, ejecutadas, guion.Llamadas.Count);
    }

    /// <summary>Un argumento inválido vuelve como error con motivo, nunca como un «0» que parezca dato.</summary>
    public static async Task<List<string>> ProvinciaFueraDeCatalogoEsErrorAsync(HerramientasDeLectura h, CancellationToken cancellationToken = default)
    {
        var r = await h.ContarLocalidadesAsync("Corrientes Capital", cancellationToken);
        return r.Error is null || r.Cantidad is not null
            ? ["contar_localidades(«Corrientes Capital») no devolvió error"]
            : [];
    }

    /// <summary>Lo montado son exactamente las dos herramientas de lectura.</summary>
    public static List<string> SoloHerramientasDeLectura(HerramientasDeLectura h)
    {
        var nombres = h.Crear().Select(t => t.Name).Order(StringComparer.Ordinal).ToList();
        return nombres.SequenceEqual([HerramientasDeLectura.Contar, HerramientasDeLectura.Listar])
            ? []
            : [$"herramientas montadas: {string.Join(", ", nombres)}"];
    }

    /// <summary>Después de toda la corrida, la sesión sigue con sus 2 localidades: nada escribió.</summary>
    public static async Task<List<string>> LaBaseNoCambioAsync(Sesion propia, CancellationToken cancellationToken = default)
    {
        var todas = await propia.Localidades.GetAllAsync(cancellationToken);
        return todas.Count == 2 ? [] : [$"la sesión propia terminó con {todas.Count} localidades (se esperaban 2)"];
    }
}
