// Nuevo: guion del modo --fake de 07 (escrito para el ejemplo). Una respuesta por pregunta del oro,
// en su orden (oro v2: las 16 del v1 más n05), más el caso guionado «e5» (sólo en modo falso; no es
// una de las 17). Tres son malas a
// propósito, para que el código y el oráculo se vean actuar:
//   · r09 cita una fuente que no existe → el código la rechaza (cita no recuperada);
//   · e5 es la respuesta de N3 E5 (qwen2.5:7b), con los ids de este corpus: cita los requisitos de
//     distancia y de minutos, que SÍ se recuperaron, y contradice el de minutos («cero minutos»)
//     → la cita es válida y el oráculo la marca igual;
//   · n02 contesta sin cita algo que el contexto no dice → el código la rechaza (sin cita).
using MovilidadUrbana.Asistente.Application;
using MovilidadUrbana.Asistente.Comun.Pruebas;

namespace MovilidadUrbana.Asistente;

public static class GuionDelEjemplo
{
    public const string CasoE5 =
        "Si no viajás, tenés que poner cero kilómetros y cero minutos [Fuente: EncuestaPolicy.RequisitoDeLaDistancia][Fuente: EncuestaPolicy.RequisitoDeLosMinutos].";

    public static readonly IReadOnlyDictionary<string, string> Respuestas = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["r01"] = "El nombre lleva entre 3 y 60 caracteres y no se puede repetir en la misma provincia. [Fuente: LocalidadPolicy.RequisitoDelNombre]",
        ["r02"] = "No: tiene que ser una de las provincias del catálogo. [Fuente: LocalidadPolicy.RequisitoDeLaProvincia]",
        ["r03"] = "El código postal lleva 4 dígitos. [Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]",
        ["r04"] = "Como mínimo 1 habitante: un número entero de 1 en adelante. [Fuente: LocalidadPolicy.RequisitoDeLosHabitantes]",
        ["r05"] = "Al menos 3 caracteres. [Fuente: EncuestaPolicy.RequisitoDelNombre]",
        ["r06"] = "Entre 16 y 110 años. [Fuente: EncuestaPolicy.RequisitoDeLaEdad]",
        ["r07"] = "No: tiene que ser una de las localidades cargadas en el ABM. [Fuente: EncuestaPolicy.RequisitoDeLaLocalidad]",
        ["r08"] = "Hay que marcar al menos uno. [Fuente: EncuestaPolicy.RequisitoDeLosMedios]",
        ["r09"] = "Se pregunta con qué asiduidad viaja. [Fuente: EncuestaPolicy.RequisitoDelHorario]",
        ["r10"] = "Entre 0 y 500 km por día. [Fuente: EncuestaPolicy.RequisitoDeLaDistancia]",
        ["r11"] = "Entre 1 y 600 minutos. [Fuente: EncuestaPolicy.RequisitoDeLosMinutos]",
        ["r12"] = "Hay que contestar el motivo principal del viaje. [Fuente: EncuestaPolicy.RequisitoDelMotivo]",
        ["n01"] = "No sé.",
        ["n02"] = "Sí, se puede corregir desde el resumen.",
        ["n03"] = "No sé.",
        ["n04"] = "No sé.",
        ["n05"] = "No sé.",
        ["e5"] = CasoE5
    };

    /// <summary>
    /// La pregunta del caso E5, escrita para el ejemplo (N3 E5 preguntaba qué se declara si no se
    /// viaja). Sólo corre en modo falso: con el modelo real no se repite (§E.2: «caso guionado»).
    /// </summary>
    public static readonly PreguntaDeOro PreguntaE5 = new()
    {
        Id = "e5", Tipo = "respondible", Objetivo = "EncuestaPolicy.RequisitoDeLosMinutos",
        Texto = "Si no viajo, ¿qué pongo en la distancia por día y en los minutos?",
        Relevantes = ["EncuestaPolicy.RequisitoDeLosMinutos"],
        ValorEsperado = "1 y 600", FormasAceptadas = [["1", "uno"], ["600", "seiscientos"]]
    };

    public static GuionChatClient Crear(IEnumerable<string> entradaIds) =>
        new(entradaIds.Select(id => PasoDelGuion.Texto(Respuestas[id.Split('#')[0]])), "No sé.");
}
