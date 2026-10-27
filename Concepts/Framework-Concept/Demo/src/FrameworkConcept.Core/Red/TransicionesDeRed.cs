namespace FrameworkConcept.Core.Red;

/// <summary>
/// La máquina de estados de la red (borrador §6), como función pura: estado actual + señal → estado siguiente.
/// Sin MAUI ni reloj, para probarla en Core.Tests/Red. La tabla de transiciones está en
/// DISENO-RED-Y-AUTENTICACION.md §2.2.
/// </summary>
public static class TransicionesDeRed
{
    /// <param name="actual">Estado antes de la señal.</param>
    /// <param name="senal">Lo que pasó.</param>
    /// <param name="perfilMedido">Si el perfil de conexión vigente es celular sin WiFi (decide EnLinea o EnLineaMedida).</param>
    public static EstadoDeRed Siguiente(EstadoDeRed actual, SenalDeRed senal, bool perfilMedido)
    {
        // Lo que dice el sistema manda: sin red no hay nada que chequear.
        if (senal == SenalDeRed.SistemaSinRed) return EstadoDeRed.SinRed;

        var enLinea = perfilMedido ? EstadoDeRed.EnLineaMedida : EstadoDeRed.EnLinea;
        return actual switch
        {
            // Volvió la red: todavía no se sabe si llega al servidor (el monitor chequea enseguida). Cualquier otra
            // señal con el sistema sin red es una respuesta vieja (de antes del corte) y se ignora.
            EstadoDeRed.SinRed => senal == SenalDeRed.SistemaConRed ? EstadoDeRed.RedSinServidor : EstadoDeRed.SinRed,

            // Sólo el chequeo de salud saca de acá: «hay red» no es «llego a mi servidor» (§6).
            EstadoDeRed.RedSinServidor => senal == SenalDeRed.SaludOk ? enLinea : EstadoDeRed.RedSinServidor,

            // En línea (con o sin datos móviles): una falla lleva a RedSinServidor; el perfil decide si es medida.
            _ => senal switch
            {
                SenalDeRed.SaludFalla or SenalDeRed.LlamadaFallida => EstadoDeRed.RedSinServidor,
                SenalDeRed.PerfilMedido => EstadoDeRed.EnLineaMedida,
                SenalDeRed.PerfilNoMedido => EstadoDeRed.EnLinea,
                _ => enLinea   // SaludOk o SistemaConRed: se confirma, recalculando con el perfil vigente
            }
        };
    }

    /// <summary>
    /// El texto para la persona en cada estado (DISENO §2.1). En línea no hay nada que avisar. Lo envía el cascarón en
    /// «motivo» (network.status, network.changed) y lo muestra el aviso permanente del contenido.
    /// </summary>
    public static string? MotivoDe(EstadoDeRed estado) => estado switch
    {
        EstadoDeRed.SinRed => "Sin conexión. Lo que hagas se guarda y se envía cuando vuelva.",
        EstadoDeRed.RedSinServidor => "Hay red pero el servidor no responde. Reintentando…",
        EstadoDeRed.EnLineaMedida => "Datos móviles",
        _ => null
    };
}

/// <summary>
/// Criterios de la demo para el chequeo de salud y la reconexión (borrador §6: «timeout corto, caché de unos
/// segundos»). Son de la demo, no valores de producto del folio 036.
/// </summary>
public static class ParametrosDeRed
{
    /// <summary>Timeout del GET /salud.</summary>
    public static readonly TimeSpan TimeoutDeSalud = TimeSpan.FromSeconds(3);

    /// <summary>Un resultado de salud se reutiliza durante este tiempo (no se pregunta dos veces seguidas).</summary>
    public static readonly TimeSpan CacheDeSalud = TimeSpan.FromSeconds(10);

    /// <summary>En RedSinServidor se reintenta el chequeo con backoff desde acá…</summary>
    public static readonly TimeSpan ReintentoInicial = TimeSpan.FromSeconds(5);

    /// <summary>…hasta este máximo (reconexión automática, sin límite de intentos).</summary>
    public static readonly TimeSpan ReintentoMaximo = TimeSpan.FromSeconds(60);

    /// <summary>En línea, un chequeo de confirmación cada tanto (además de lo que informen las llamadas).</summary>
    public static readonly TimeSpan ChequeoEnLinea = TimeSpan.FromSeconds(60);

    /// <summary>Tamaño del registro circular de llamadas (en memoria; se ve en Diagnóstico).</summary>
    public const int RegistroDeLlamadas = 200;
}
