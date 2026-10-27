namespace FrameworkConcept.Core.Red;

// Borrador §6 — «hay red» no es «llego a mi servidor». El cascarón combina lo que dice el sistema
// (Connectivity) con un chequeo de salud propio (GET /salud del receptor) y expone UN estado.
// Diseño: DISENO-RED-Y-AUTENTICACION.md §2.

/// <summary>El estado de red combinado (borrador §6). En el puente viaja en camelCase: «sinRed», «enLinea»…</summary>
public enum EstadoDeRed
{
    /// <summary>El sistema dice que no hay conexión.</summary>
    SinRed,

    /// <summary>Hay red, pero el servidor no responde (portal cautivo, servidor caído, llamadas que fallan).</summary>
    RedSinServidor,

    /// <summary>El servidor respondió el chequeo de salud.</summary>
    EnLinea,

    /// <summary>En línea por datos móviles (perfil celular sin WiFi ni Ethernet).</summary>
    EnLineaMedida
}

/// <summary>
/// Lo que el cascarón sabe de la red en un momento: el estado combinado, lo que dice el sistema y, si no está en
/// línea, un motivo para mostrar.
/// </summary>
/// <param name="Estado">Estado combinado.</param>
/// <param name="Conectado">Lo que dice el sistema (<c>NetworkAccess.Internet</c>). Es el campo «conectado» del puente 1.0.</param>
/// <param name="Motivo">Texto para la persona cuando no está en línea («El servidor no responde»…).</param>
public sealed record EstadoDeRedCombinado(EstadoDeRed Estado, bool Conectado, string? Motivo = null)
{
    /// <summary>Hasta la primera lectura del sistema se supone sin red: no se ofrece nada que la necesite.</summary>
    public static EstadoDeRedCombinado Inicial { get; } = new(EstadoDeRed.SinRed, false, "Comprobando la conexión…");

    /// <summary>EnLinea o EnLineaMedida: se puede hablar con el servidor.</summary>
    public bool ConServidor => Estado is EstadoDeRed.EnLinea or EstadoDeRed.EnLineaMedida;
}

/// <summary>Lo que hace cambiar el estado de red (las flechas del diagrama del borrador §6).</summary>
public enum SenalDeRed
{
    /// <summary>ConnectivityChanged: el sistema dice que no hay red.</summary>
    SistemaSinRed,

    /// <summary>ConnectivityChanged: el sistema dice que hay red (todavía no se sabe si llega al servidor).</summary>
    SistemaConRed,

    /// <summary>GET /salud respondió 2xx dentro del timeout.</summary>
    SaludOk,

    /// <summary>GET /salud falló o se agotó el timeout.</summary>
    SaludFalla,

    /// <summary>Una llamada del framework falló por red o 5xx (lo informa el DelegatingHandler, borrador §6 «Actividad»).</summary>
    LlamadaFallida,

    /// <summary>El perfil de conexión pasó a celular sin WiFi.</summary>
    PerfilMedido,

    /// <summary>El perfil de conexión volvió a WiFi o Ethernet.</summary>
    PerfilNoMedido
}
