using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// P13 — Las reglas de las definiciones de encuesta que el teléfono aplica. Son funciones puras; las usa el actualizador
/// y las puede usar el cascarón al limpiar su tabla de definiciones.
/// </summary>
public static class PoliticaDeDefiniciones
{
    /// <summary>
    /// P13.3 — Qué definiciones guardadas se CONSERVAN: la vigente de cada encuesta (la usa la próxima) y toda versión
    /// que tenga respuestas en curso o pendientes de envío, aunque sea vieja o esté retirada. La retención de paquetes
    /// de contenido no entra acá: borrar un paquete nunca borra una definición.
    /// </summary>
    /// <param name="guardadas">Todas las versiones que hay en la base local.</param>
    /// <param name="conRespuestasEnCursoOPendientes">Las referencias que usan las respuestas sin enviar.</param>
    public static IReadOnlySet<ReferenciaDeDefinicion> Conservables(
        IEnumerable<ReferenciaDeDefinicion> guardadas,
        IEnumerable<ReferenciaDeDefinicion> conRespuestasEnCursoOPendientes)
    {
        var lista = guardadas.ToList();
        var conservar = new HashSet<ReferenciaDeDefinicion>(conRespuestasEnCursoOPendientes.Where(lista.Contains));

        foreach (var vigente in lista.GroupBy(r => r.EncuestaId).Select(g => g.MaxBy(r => r.VersionDefinicion)!))
            conservar.Add(vigente);

        return conservar;
    }

    /// <summary>
    /// P13.5 — Cuándo rige un retiro anunciado: en su fecha, pero nunca antes de cumplir el plazo de aviso
    /// (<see cref="ParametrosDeProducto.PlazoDeAvisoDeRetiro"/>) desde que se anunció. Un retiro anunciado con menos aviso
    /// no se adelanta: rige al cumplirse el plazo.
    /// </summary>
    public static DateTimeOffset RigeDesde(DefinicionRetirada retiro)
    {
        var minimo = retiro.AnunciadaEl + ParametrosDeProducto.PlazoDeAvisoDeRetiro;
        return retiro.RetiroEl >= minimo ? retiro.RetiroEl : minimo;
    }

    /// <summary>¿Esa versión de definición está retirada a esta hora, según el último índice válido?</summary>
    public static bool EstaRetirada(ReferenciaDeDefinicion referencia, IndiceDelCanal? indice, DateTimeOffset ahora) =>
        indice is not null && indice.DefinicionesRetiradas.Any(r =>
            r.EncuestaId == referencia.EncuestaId && r.Version == referencia.VersionDefinicion && ahora >= RigeDesde(r));

    /// <summary>
    /// P13.1 y P13.9 — Una definición del índice se acepta sólo si su versión es ESTRICTAMENTE mayor que la guardada:
    /// el roll-forward nunca baja la versión, y revertir una encuesta es publicar una versión nueva.
    /// </summary>
    public static bool EsMasNueva(EntradaDeDefinicion entrada, DefinicionEncuesta? guardada) =>
        guardada is null || entrada.Version > guardada.Version;
}
