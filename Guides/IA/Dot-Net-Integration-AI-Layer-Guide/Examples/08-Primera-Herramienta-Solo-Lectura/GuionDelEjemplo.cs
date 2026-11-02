// Nuevo: lo que «pide» y «responde» el guion del modo --fake (sin red). Cubre lo que pide el plan
// para 08: una FunctionCallContent válida, una inválida (provincia fuera de catálogo) y, en
// Garantias.ElBucleSeCortaAsync, una repetida hasta el corte.
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente;

public static class GuionDelEjemplo
{
    public static IEnumerable<PasoDelGuion> Pasos(IEnumerable<Entrada> entradas)
    {
        foreach (var e in entradas)
        {
            switch (e.Id)
            {
                case "l01":
                    yield return PasoDelGuion.Herramienta("l01-1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Corrientes" });
                    yield return PasoDelGuion.Texto("Tenés 1 localidad cargada en Corrientes.");
                    break;
                case "n01":
                    // Inválida: «Goya» no es una provincia; la herramienta devuelve error y el guion lista.
                    yield return PasoDelGuion.Herramienta("n01-1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Goya" });
                    yield return PasoDelGuion.Herramienta("n01-2", HerramientasDeLectura.Listar);
                    yield return PasoDelGuion.Texto("No tengo ese dato: Goya no está entre tus localidades y las herramientas no informan habitantes.");
                    break;
                case "xn01":
                    yield return PasoDelGuion.Herramienta("xn01-1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Corrientes Capital" });
                    yield return PasoDelGuion.Texto("«Corrientes Capital» no es una provincia del catálogo, así que no puedo contarla.");
                    break;
                case "fc01":
                    // Nuevo (REC-29): el guion pide la herramienta, recibe el error de catálogo y no afirma cifra.
                    yield return PasoDelGuion.Herramienta("fc01-1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Misiones" });
                    yield return PasoDelGuion.Texto("«Misiones» no es una provincia del catálogo, así que no puedo contar localidades ahí.");
                    break;
                case "xl01":
                case "xl02":
                    // Conteo por provincia: se pide a contar_localidades (el oráculo no admite contar a mano
                    // las filas de una provincia en listar_localidades; sólo su total).
                    var provincia = e.Id == "xl01" ? "Corrientes" : "Chaco";
                    yield return PasoDelGuion.Herramienta($"{e.Id}-1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = provincia });
                    yield return PasoDelGuion.Texto($"Tenés {e.Esperado} localidad cargada en {provincia}.");
                    break;
                default:
                    yield return PasoDelGuion.Herramienta($"{e.Id}-1", HerramientasDeLectura.Listar);
                    yield return PasoDelGuion.Texto(e.Esperado is int n
                        ? $"Según tus datos, son {n}."
                        : "No tengo ese dato en tus localidades.");
                    break;
            }
        }
    }
}
