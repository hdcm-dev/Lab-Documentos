using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Content.Bridge;

// Folio 038 — Filas del puente 1.1 de red y acciones (DISENO-RED-Y-AUTENTICACION.md §6). Una función por fila, como
// el resto de BridgeClient. Archivo del constructor de red.
public sealed partial class BridgeClient
{
    /// <summary>network.status con el estado combinado del borrador §6 (campos opcionales desde 1.1).</summary>
    public Task<EstadoDeRedInformado> NetworkStatusCombinadoAsync() =>
        CallAsync<EstadoDeRedInformado>("network.status");

    /// <summary>actions.policy — todas las acciones declaradas, evaluadas por el cascarón.</summary>
    public async Task<IReadOnlyList<EvaluacionDeAccion>> ActionsPolicyAsync() =>
        (await CallAsync<ListaDeAcciones>("actions.policy")).Acciones;

    /// <summary>actions.check — una acción por id.</summary>
    public async Task<EvaluacionDeAccion> ActionsCheckAsync(string accion) =>
        (await CallAsync<UnaAccion>("actions.check", new { accion })).Evaluacion;

    /// <summary>outbox.sendNow — «Enviar ahora» (SoloEnLinea: sin servidor, BridgeException con AccionBloqueada).</summary>
    public Task<ResultadoDeEnviarAhora> OutboxSendNowAsync() =>
        CallAsync<ResultadoDeEnviarAhora>("outbox.sendNow");

    private sealed record ListaDeAcciones(IReadOnlyList<EvaluacionDeAccion> Acciones);

    private sealed record UnaAccion(EvaluacionDeAccion Evaluacion);
}

/// <summary>Respuesta de network.status y parámetros de network.changed. Estado y Motivo faltan con un cascarón 1.0.</summary>
public sealed record EstadoDeRedInformado(bool Conectado, EstadoDeRed? Estado = null, string? Motivo = null);

/// <summary>Respuesta de outbox.sendNow.</summary>
public sealed record ResultadoDeEnviarAhora(int Enviadas, int Pendientes, int Rechazadas);
