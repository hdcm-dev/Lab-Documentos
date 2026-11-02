// Nuevo, escrito para el ejemplo: lo que «responde el modelo» en --fake. Está armado para ejercer
// las garantías del código, no para imitar al modelo: en las entradas sin el dato, el guion
// INVENTA un número plausible (30000) para mostrar que la garantía lo descarta, y una respuesta de
// la extensión no es JSON, para mostrar el camino de TryGetResult = false. En c01 (REC-16) el texto
// trae «CP 34» y el guion INVENTA «3400», que tiene forma válida y pasaría CodigoPostalValido.
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente;

public static class GuionDelEjemplo
{
    public static readonly IReadOnlyDictionary<string, string> Respuestas = new Dictionary<string, string>
    {
        ["e01"] = """{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""",
        ["e02"] = """{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":30000}""",
        ["x01"] = """{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""",
        ["x02"] = """{"nombre":"Resistencia","provincia":"chaco","codigoPostal":"3500","habitantes":291720}""",
        ["x03"] = """{"nombre":"Corrientes","provincia":"Corrientes","codigoPostal":"3400","habitantes":346334}""",
        ["x04"] = """{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":30000}""",
        ["x05"] = """{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":null}""",
        ["x06"] = """{"nombre":"Resistencia","provincia":"Chaco","codigoPostal":"3500","habitantes":291720}""",
        ["x07"] = "La localidad es Corrientes, con código postal 3400.",
        ["x08"] = """{"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":null}""",
        ["c01"] = """{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3400","habitantes":89959}"""
    };

    public static GuionChatClient Para(IEnumerable<Entrada> entradas) =>
        new(entradas.Select(e => PasoDelGuion.Texto(Respuestas[e.Id])));

    /// <summary>Decisión que el código tiene que tomar sobre la respuesta guionada (invariante de --fake).</summary>
    public static readonly IReadOnlyDictionary<string, string> DecisionEsperada = new Dictionary<string, string>
    {
        ["e01"] = "lista",
        ["e02"] = "faltan:habitantes",
        ["x01"] = "lista",
        ["x02"] = "lista",
        ["x03"] = "lista",
        ["x04"] = "lista",
        ["x05"] = "faltan:habitantes",
        ["x06"] = "faltan:habitantes",
        ["x07"] = "sin-propuesta",
        ["x08"] = "faltan:habitantes",
        ["c01"] = "faltan:codigoPostal"
    };
}
