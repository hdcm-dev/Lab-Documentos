namespace TusDemo.Web.Archivos;

/// <summary>
/// Avisa dentro del proceso que la lista de archivos cambió. Cada página Blazor abierta se suscribe
/// y se redibuja; el cambio le llega al navegador por el circuito de Blazor Server, que viaja sobre SignalR.
/// </summary>
public sealed class AvisoDeArchivos
{
    public event Func<Task>? Cambio;

    public async Task AvisarAsync()
    {
        var suscriptores = Cambio;
        if (suscriptores is null) return;
        foreach (Func<Task> s in suscriptores.GetInvocationList())
            await s();
    }
}
