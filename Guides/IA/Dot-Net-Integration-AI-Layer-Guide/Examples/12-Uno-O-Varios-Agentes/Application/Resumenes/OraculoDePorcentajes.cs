// Nuevo: el oráculo del ejemplo 12, definido antes de correr (plan §E.2): cada medio pedido aparece
// con su porcentaje y coincide (±1 punto) con ResultadosDeEncuestaService; un resumen sin porcentajes
// es falla («sin cifra»). Lee SÓLO lo que queda en el registro (el contexto de la primera llamada y la
// salida de la última), para que tools/oraculos/porcentajes-del-servicio.jq pueda recalcularlo.
using System.Text.Json;
using MovilidadUrbana.Asistente.Comun.Registro;

namespace MovilidadUrbana.Asistente.Application.Resumenes;

public sealed record VeredictoDelOraculo(string Resultado, string? Motivo);

public static class OraculoDePorcentajes
{
    public const string Nombre = "porcentajes-del-servicio";

    public static VeredictoDelOraculo Evaluar(IReadOnlyList<RegistroDeLlamada> llamadas)
    {
        if (llamadas.Count == 0) return new(ResultadoDeEntrada.Excluida, "sin llamadas");
        if (llamadas.Any(l => l.TruncadoPosible == true)) return new(ResultadoDeEntrada.Excluida, "truncado_posible");

        var ordenadas = llamadas.OrderBy(l => l.LlamadaN).ToList();
        var datos = JsonSerializer.Deserialize<DatosDelResumen>(ordenadas[0].Contexto ?? "null");
        if (datos is null) return new(ResultadoDeEntrada.Excluida, "sin contexto en el registro");

        var esperados = datos.MediosPedidos
            .Select(c => (c, datos.Medios.Single(m => m.Clave == c).Porcentaje)).ToList();
        var v = ComparadorDePorcentajes.Verificar(ordenadas[^1].Salida, esperados);
        return v.Correcto
            ? new(ResultadoDeEntrada.Pasa, null)
            : new(ResultadoDeEntrada.Falla, v.SinCifra ? "sin cifra" : string.Join("; ", v.Errores));
    }
}
