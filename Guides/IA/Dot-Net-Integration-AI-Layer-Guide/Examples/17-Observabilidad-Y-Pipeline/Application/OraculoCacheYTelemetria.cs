// Nuevo (ejemplo 17; plan §E.2: «con la caché más afuera, los aciertos no aparecen en la telemetría;
// con contenido apagado, los spans no traen mensajes»). Lo recalcula tools/oraculos/cache-y-telemetria.jq
// leyendo sólo el registro: las dos implementaciones tienen que dar lo mismo. Alcance: el registro
// guarda sólo las llamadas que llegaron al modelo, así que el oráculo comprueba qué pedidos llegaron
// y que cada uno tuvo su span; que el acierto de caché NO generó span (cantidad total de spans) y la
// lista de métricas contra la convención son invariantes de código (Corrida.cs), con rastro en el log
// de la corrida, no en el registro.
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Lo que el oráculo mira de cada llamada registrada.</summary>
public sealed record LlamadaObservada(string Paso, string? Salida, float? Temperatura, bool? TruncadoPosible);

public static class OraculoCacheYTelemetria
{
    public const string Nombre = "cache-y-telemetria";

    /// <summary>
    /// Arma el campo <c>paso</c> de la línea de llamada. El oráculo lee de acá a qué pedido
    /// corresponde la llamada, si tuvo span y si el span trajo contenido.
    /// </summary>
    public static string Paso(string pedidoId, bool conSpan, bool contenidoEnSpan, string ambiente) =>
        $"{pedidoId} · caché más afuera · span: {SiNo(conSpan)} · contenido en el span: {SiNo(contenidoEnSpan)} · ambiente: {ambiente}";

    private static string SiNo(bool valor) => valor ? "sí" : "no";

    /// <summary>
    /// Reglas (las mismas del .jq): sin llamadas o con truncado_posible → excluida; si no, pasa
    /// cuando llegaron al modelo exactamente las llamadas de p1 y p3 (p2 fue acierto de caché),
    /// con temperaturas distintas, salida no vacía, un span por llamada y contenido en el span
    /// sólo si el ambiente es Development.
    /// </summary>
    public static (string Resultado, string Motivo) Evaluar(IReadOnlyList<LlamadaObservada> llamadas)
    {
        if (llamadas.Count == 0) return (ResultadoDeEntrada.Excluida, "sin llamadas");
        if (llamadas.Any(l => l.TruncadoPosible == true)) return (ResultadoDeEntrada.Excluida, "truncado_posible");

        var pedidos = llamadas.Select(l => l.Paso.Split(' ')[0]).ToList();
        if (!pedidos.SequenceEqual(["p1", "p3"]))
            return (ResultadoDeEntrada.Falla, $"llegaron al modelo {string.Join(",", pedidos)}; se esperaba p1,p3 (p2 es acierto de caché)");
        if (llamadas.Select(l => l.Temperatura).Distinct().Count() != llamadas.Count)
            return (ResultadoDeEntrada.Falla, "las dos llamadas salieron con la misma temperatura");
        if (llamadas.Any(l => string.IsNullOrWhiteSpace(l.Salida)))
            return (ResultadoDeEntrada.Falla, "salida vacía");
        if (llamadas.Any(l => !l.Paso.Contains("span: sí", StringComparison.Ordinal)))
            return (ResultadoDeEntrada.Falla, "una llamada al modelo sin span");
        foreach (var l in llamadas)
        {
            var desarrollo = l.Paso.Contains("ambiente: Development", StringComparison.OrdinalIgnoreCase);
            var conContenido = l.Paso.Contains("contenido en el span: sí", StringComparison.Ordinal);
            if (desarrollo != conContenido)
                return (ResultadoDeEntrada.Falla, conContenido ? "contenido en el span fuera de desarrollo" : "sin contenido en desarrollo");
        }
        return (ResultadoDeEntrada.Pasa, "llegaron al modelo p1 y p3 (p2 no llegó: acierto de caché), cada llamada con su span y contenido según el ambiente; la cantidad total de spans es un invariante de código, no se lee del registro");
    }
}
