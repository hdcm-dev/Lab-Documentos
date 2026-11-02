// Nuevo (plan §E.2, fila 05). El orden de las llamadas es parte del experimento: el plan lo fija
// «fijo y alternado» (sin-prefijo 1, con-prefijo 1, sin-prefijo 2, …) y cierra con una vuelta a
// frío. La variante «calentamiento» es un DESVÍO documentado (README, «Desvío: el primer par»):
// el mismo orden con una llamada con-prefijo previa que no cuenta, para que el primer par no
// arranque sin nada en la caché.
namespace MovilidadUrbana.Asistente.Application;

public enum OrdenDeConsultas { Alternado, AlternadoConCalentamiento }

public static class Pasos
{
    public const string SinPrefijo = "sin-prefijo";
    public const string ConPrefijo = "con-prefijo";
    public const string VueltaAFrio = "vuelta-a-frio";
    /// <summary>Sólo en la variante con calentamiento: deja el prefijo en la caché antes del primer par; no cuenta.</summary>
    public const string Calentamiento = "calentamiento";
}

/// <summary>Una pregunta congelada de entradas.v1.json.</summary>
public sealed record Pregunta(string Id, string Texto);

/// <summary>Una llamada planificada: a qué entrada pertenece, qué variante es y qué texto de sistema lleva.</summary>
public sealed record ConsultaPlanificada(string EntradaId, int LlamadaN, string Paso, string Sistema, string Pregunta);

public static class PlanDeConsultas
{
    public const string IdCalentamiento = "c00";

    /// <param name="nonce">Fábrica de nonces (uno por consulta sin-prefijo); las pruebas la fijan.</param>
    public static IReadOnlyList<ConsultaPlanificada> Armar(
        IReadOnlyList<Pregunta> preguntas, string prefijo, OrdenDeConsultas orden, Func<string> nonce)
    {
        ArgumentOutOfRangeException.ThrowIfZero(preguntas.Count);
        var sin = preguntas.Select(p => new ConsultaPlanificada(p.Id, 1, Pasos.SinPrefijo, PrefijoEstable.ConNonce(prefijo, nonce()), p.Texto)).ToList();
        var con = preguntas.Select(p => new ConsultaPlanificada(p.Id, 2, Pasos.ConPrefijo, prefijo, p.Texto)).ToList();
        // La vuelta a frío va en la última entrada, como su tercera llamada, y es lo último que corre:
        // así nada se interpone entre la última con-prefijo y ella.
        var ultima = preguntas[^1];
        var vuelta = new ConsultaPlanificada(ultima.Id, 3, Pasos.VueltaAFrio, PrefijoEstable.ConPrimerCaracterCambiado(prefijo), ultima.Texto);

        var plan = new List<ConsultaPlanificada>();
        if (orden == OrdenDeConsultas.AlternadoConCalentamiento)
        {
            plan.Add(new ConsultaPlanificada(IdCalentamiento, 1, Pasos.Calentamiento, prefijo, "¿Cuántos registros tiene el padrón?"));
        }
        for (var i = 0; i < preguntas.Count; i++) { plan.Add(sin[i]); plan.Add(con[i]); }
        plan.Add(vuelta);
        return plan;
    }
}
