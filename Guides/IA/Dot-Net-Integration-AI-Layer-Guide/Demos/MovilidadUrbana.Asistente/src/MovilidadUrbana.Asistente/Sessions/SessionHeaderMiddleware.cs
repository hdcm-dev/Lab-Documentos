// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Sessions/SessionHeaderMiddleware.cs @ 8fb4c3a
// Delta: Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using MovilidadUrbana.Asistente.Infrastructure.Sessions;

namespace MovilidadUrbana.Asistente.Sessions;

/// <summary>
/// Identifica el espacio de datos del cliente por el encabezado <c>X-Sesion-Id</c>, que es la forma
/// idiomática en una API —la web lo hace por cookie—. Si el cliente no lo manda, la petición recibe
/// un identificador nuevo y lo devuelve en el mismo encabezado, para que lo repita en las siguientes.
/// </summary>
public sealed class SessionHeaderMiddleware(RequestDelegate siguiente)
{
    public const string HeaderName = "X-Sesion-Id";

    public async Task InvokeAsync(HttpContext context, SessionContext session)
    {
        var id = context.Request.Headers[HeaderName].ToString();
        if (SessionContext.IsValid(id)) session.Set(id);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = session.Id;
            return Task.CompletedTask;
        });

        await siguiente(context);
    }
}
