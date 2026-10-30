using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// E04 (folio 050) — La decisión del actualizador en este arranque, compartida por la página de arranque, el servidor de
/// contenido y el puente. Con el arranque superpuesto el contenido carga mientras el actualizador decide: el servidor
/// espera acá si todavía no sabe qué versión servir, y el puente retiene el saludo hasta que se abre (P2: el contenido
/// no habla con el cascarón antes de la decisión). Sin E04 se abre igual al final de la preparación, antes de que
/// exista el contenido, y nadie llega a esperar.
/// </summary>
public sealed class PuertaDelArranque
{
    private readonly TaskCompletionSource<ResultadoDeArranque> _preparado =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Se completa cuando el actualizador decidió (o falla si la preparación falló).</summary>
    public Task<ResultadoDeArranque> Preparado => _preparado.Task;

    public void Abrir(ResultadoDeArranque resultado) => _preparado.TrySetResult(resultado);

    public void Fallar(Exception ex) => _preparado.TrySetException(ex);

#if FC_E04_V2
    // E04 V2 — La sesión, el sincronizador y lo demás corren DESPUÉS de abrir la puerta, fuera del camino crítico. Los
    // métodos del puente que dependen de la sesión (todo lo que no es bridge.* ni network.status) esperan esta tarea.
    private readonly TaskCompletionSource _segundoPlano = new(TaskCreationOptions.RunContinuationsAsynchronously);
#else
    // Sin V2 la sesión se restaura antes de abrir la puerta: el segundo plano ya terminó.
    private readonly TaskCompletionSource _segundoPlano = CompletadaDesdeElPrincipio();

    private static TaskCompletionSource CompletadaDesdeElPrincipio()
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        tcs.SetResult();
        return tcs;
    }
#endif

    /// <summary>Se completa cuando la sesión quedó restaurada (V2); falla si la restauración falló.</summary>
    public Task SegundoPlano => _segundoPlano.Task;

    public void SegundoPlanoTerminado() => _segundoPlano.TrySetResult();

    public void SegundoPlanoFallo(Exception ex) => _segundoPlano.TrySetException(ex);
}
