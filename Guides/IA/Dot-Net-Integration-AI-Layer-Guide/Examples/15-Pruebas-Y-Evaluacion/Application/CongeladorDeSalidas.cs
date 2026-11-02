// Nuevo (plan §E.2, fila 15: «las 16 salidas de 07 de la última corrida válida»; §E.1, entradas
// congeladas). Lee el registro R-LLM de 07 y arma entradas/salidas-07.v2.json: por pregunta del oro,
// la última llamada de la última corrida completa (las 16 entradas, ninguna excluida), con su
// contexto, sus recuperados y el veredicto que dio el oráculo de 07. Sin script aparte (DC-07).
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MovilidadUrbana.Asistente.Application;

public static class CongeladorDeSalidas
{
    public const string Ejemplo07 = "07-RAG-Con-Citas";

    public static SalidasDe07 Desde(string rutaDelRegistro, ConjuntoDeOro oro)
    {
        var lineas = File.ReadLines(rutaDelRegistro)
            .Where(l => l.Length > 0)
            .Select(l => JsonNode.Parse(l)!.AsObject())
            .Where(o => (string?)o["ejemplo"] == Ejemplo07)
            .ToList();

        // Corridas por turno_id. Las líneas «entrada» no llevan turno_id: van con la llamada que las precede.
        // Vale la última corrida en la que las 16 preguntas del oro tienen su entrada no excluida.
        var turno = "";
        var conTurno = lineas.Select(o =>
        {
            if ((string?)o["tipo"] == "llamada") turno = (string?)o["turno_id"] ?? "";
            return (Turno: turno, Renglon: o);
        }).ToList();
        var corrida = conTurno
            .GroupBy(x => x.Turno, x => x.Renglon)
            .Select(g => g.ToList())
            .Where(g => oro.Preguntas.All(p => g.Any(o => (string?)o["tipo"] == "entrada" && (string?)o["entrada_id"] == p.Id && (string?)o["resultado"] != "excluida")))
            .OrderBy(g => g.Min(o => (DateTimeOffset)o["fecha"]!))
            .LastOrDefault()
            ?? throw new InvalidDataException($"El registro no tiene una corrida completa y válida de {Ejemplo07}.");

        var salidas = oro.Preguntas.Select(p =>
        {
            var llamada = corrida.Last(o => (string?)o["tipo"] == "llamada" && (string?)o["entrada_id"] == p.Id);
            var entrada = corrida.Last(o => (string?)o["tipo"] == "entrada" && (string?)o["entrada_id"] == p.Id);
            return new SalidaDe07
            {
                Id = p.Id,
                Pregunta = p.Texto,
                Contexto = (string)llamada["contexto"]!,
                Recuperados = llamada["recuperados"]!.AsArray().Select(r => (string)r!["id"]!).ToList(),
                Salida = (string)llamada["salida"]!,
                Resultado07 = (string)entrada["resultado"]!,
                Motivo07 = (string?)entrada["motivo"]
            };
        }).ToList();

        var primera = corrida.First(o => (string?)o["tipo"] == "llamada");
        return new SalidasDe07
        {
            Rotulo = "salidas del modelo copiadas del registro R-LLM de 07 (corrida válida); las preguntas son estímulo escrito para el ejemplo (oro de 06)",
            Version = "v2",
            Procedencia = new ProcedenciaDe07
            {
                Ejemplo = Ejemplo07,
                Modelo = (string?)primera["modelo"],
                Digest = (string?)primera["digest"],
                OllamaVersion = (string?)primera["ollama_version"],
                Servidor = (string?)primera["servidor"],
                TurnoId = Enmascarar((string?)primera["turno_id"]),
                Fecha = (DateTimeOffset)primera["fecha"]!,
                EntradasHash07 = (string?)primera["entradas_hash"],
                Registro = $"{Path.GetFileName(rutaDelRegistro)} (registro de trabajo no publicado; ver Anexo H)"
            },
            Salidas = salidas
        };
    }

    // Un turno_id es «fecha-recurso-pid-n»: el recurso nombra un servicio del laboratorio y no se publica.
    public static string? Enmascarar(string? turnoId) =>
        turnoId is null ? null
        : System.Text.RegularExpressions.Regex.Replace(turnoId, @"^(\d{8}T\d{6})-.+-(\d+-\d+)$", "$1-<recurso>-$2");
}
