// Nuevo (ejemplo 12, §8.6 y §8.8). El escalón 2 en su forma positiva: un workflow de MAF
// (Microsoft.Agents.AI.Workflows 1.26.0) cuyo CAMINO lo fija el código, con una sola variable —cuántos
// agentes—:
//
//   «un-agente»:   Datos → Redactor → Verificador ─(no coincide, 1 vez)→ Redactor → Verificador → salida
//   «dos-agentes»: Datos → Redactor → Verificador ─(no coincide, 1 vez)→ Corrector → Verificador → salida
//
// El Redactor es un executor que llama al IChatClient (no es un AIAgent). El Corrector es un segundo
// agente, hecho con AsAIAgent sobre el MISMO cliente de la capa única. El Verificador NO usa modelo:
// compara los porcentajes con ResultadosDeEncuestaService (escalón −1). En ningún punto un modelo
// elige el paso siguiente: eso sería el escalón 3 (handoff, Magentic), que la guía deja sin ejemplo.
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Resumenes;

namespace MovilidadUrbana.Asistente.Application.Workflows;

public enum Variante { UnAgente, DosAgentes }

public static class CaminoDelResumen
{
    public const string DatosId = "Datos";
    public const string RedactorId = "Redactor";
    public const string VerificadorId = "Verificador";
    public const string CorrectorId = "Corrector";

    /// <summary>Vueltas de corrección permitidas. Una: el workflow nunca entra en un bucle abierto.</summary>
    public const int MaximoDeVueltas = 1;

    public static string Nombre(Variante v) => v == Variante.UnAgente ? "un-agente" : "dos-agentes";

    /// <summary>
    /// Arma el grafo. <paramref name="clienteDelRedactor" /> y <paramref name="clienteDelCorrector" /> llegan
    /// ya envueltos por la capa única (FabricaDeClientes): nadie fija acá razonamiento, num_ctx ni temperatura.
    /// En «un-agente» no se crea ningún AIAgent.
    /// </summary>
    public static Workflow Construir(
        Variante variante, FuenteDeResultados fuente, IChatClient clienteDelRedactor, IChatClient? clienteDelCorrector = null)
    {
        var datos = new DatosExecutor(fuente);
        var redactor = new RedactorExecutor(clienteDelRedactor);
        var destino = variante == Variante.UnAgente ? RedactorId : CorrectorId;
        var verificador = new VerificadorExecutor(fuente, destino);

        var builder = new WorkflowBuilder(datos)
            .AddEdge(datos, redactor)
            .AddEdge(redactor, verificador);

        if (variante == Variante.UnAgente)
        {
            builder = builder.AddEdge(verificador, redactor);
        }
        else
        {
            var agente = (clienteDelCorrector ?? throw new ArgumentNullException(nameof(clienteDelCorrector)))
                .AsAIAgent(instructions: Indicaciones.DelCorrector, name: "corrector",
                           description: "Reescribe un resumen con los errores que encontró el verificador.");
            var corrector = new CorrectorExecutor(agente);
            builder = builder.AddEdge(verificador, corrector).AddEdge(corrector, verificador);
        }

        return builder.WithOutputFrom(verificador).WithName($"resumen-{Nombre(variante)}").Build();
    }
}

/// <summary>Nodo función: llama a resumir_encuesta y arma el contexto. No hay modelo.</summary>
public sealed partial class DatosExecutor(FuenteDeResultados fuente) : Executor(CaminoDelResumen.DatosId)
{
    [MessageHandler(Send = [typeof(PedidoDeResumen)])]
    private async ValueTask ResumirAsync(SolicitudDeResumen solicitud, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        var r = await fuente.ResumirEncuestaAsync(cancellationToken);
        var pedido = new PedidoDeResumen(solicitud, FuenteDeResultados.Contexto(r, solicitud.MediosPedidos));
        await contexto.SendMessageAsync(pedido, CaminoDelResumen.RedactorId, cancellationToken);
    }
}

/// <summary>El redactor: un executor que llama al modelo una vez por mensaje. No es un AIAgent.</summary>
public sealed partial class RedactorExecutor(IChatClient cliente) : Executor(CaminoDelResumen.RedactorId)
{
    private static ChatOptions Opciones() => new() { Instructions = Indicaciones.DelRedactor };

    [MessageHandler(Send = [typeof(Borrador)])]
    private async ValueTask RedactarAsync(PedidoDeResumen pedido, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        var r = await cliente.GetResponseAsync(Indicaciones.Redaccion(pedido), Opciones(), cancellationToken);
        await contexto.SendMessageAsync(new Borrador(pedido, r.Text, 0, CaminoDelResumen.RedactorId), CaminoDelResumen.VerificadorId, cancellationToken);
    }

    /// <summary>Sólo en «un-agente»: la vuelta llega al mismo redactor, con el error del verificador.</summary>
    [MessageHandler(Send = [typeof(Borrador)])]
    private async ValueTask ReescribirAsync(PedidoDeCorreccion correccion, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        var r = await cliente.GetResponseAsync(Indicaciones.Correccion(correccion), Opciones(), cancellationToken);
        await contexto.SendMessageAsync(new Borrador(correccion.Pedido, r.Text, correccion.Vuelta, CaminoDelResumen.RedactorId),
            CaminoDelResumen.VerificadorId, cancellationToken);
    }
}

/// <summary>Sólo en «dos-agentes»: el segundo agente (AsAIAgent) recibe el error y reescribe.</summary>
public sealed partial class CorrectorExecutor(AIAgent agente) : Executor(CaminoDelResumen.CorrectorId)
{
    [MessageHandler(Send = [typeof(Borrador)])]
    private async ValueTask CorregirAsync(PedidoDeCorreccion correccion, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        // Sin AgentSession: cada corrección es independiente; el agente no guarda estado entre entradas.
        var r = await agente.RunAsync(Indicaciones.Correccion(correccion), cancellationToken: cancellationToken);
        await contexto.SendMessageAsync(new Borrador(correccion.Pedido, r.Text, correccion.Vuelta, CaminoDelResumen.CorrectorId),
            CaminoDelResumen.VerificadorId, cancellationToken);
    }
}

/// <summary>
/// El verificador: una cuenta en código. Pide los resultados al caso de uso (no confía en el texto que
/// viajó por el workflow) y compara. Si no coincide y queda vuelta, manda la corrección; si no, emite la salida.
/// </summary>
public sealed partial class VerificadorExecutor(FuenteDeResultados fuente, string destinoDeLaVuelta) : Executor(CaminoDelResumen.VerificadorId)
{
    [MessageHandler(Send = [typeof(PedidoDeCorreccion)], Yield = [typeof(ResumenVerificado)])]
    private async ValueTask VerificarAsync(Borrador borrador, IWorkflowContext contexto, CancellationToken cancellationToken)
    {
        var resultados = await fuente.ResumirEncuestaAsync(cancellationToken);
        var v = ComparadorDePorcentajes.Verificar(borrador.Texto, borrador.Pedido.Solicitud.MediosPedidos, resultados);
        if (!v.Correcto && borrador.Vuelta < CaminoDelResumen.MaximoDeVueltas)
        {
            await contexto.SendMessageAsync(
                new PedidoDeCorreccion(borrador.Pedido, borrador.Texto, v.Errores, borrador.Vuelta + 1), destinoDeLaVuelta, cancellationToken);
            return;
        }
        await contexto.YieldOutputAsync(
            new ResumenVerificado(borrador.Pedido.Solicitud.EntradaId, borrador.Texto, v.Correcto, v.Errores, borrador.Vuelta, borrador.Autor),
            cancellationToken);
    }
}
