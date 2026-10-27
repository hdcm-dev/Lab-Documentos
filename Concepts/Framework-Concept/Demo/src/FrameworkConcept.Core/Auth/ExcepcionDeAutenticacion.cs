using FrameworkConcept.Core.Contracts.Puente;

namespace FrameworkConcept.Core.Auth;

/// <summary>
/// Un auth.* que no terminó: la persona canceló (navegador cerrado, diálogo del sistema cancelado) o el ingreso falló
/// (invalid_grant, clave invalidada, sin bloqueo de pantalla, emisor inalcanzable). El despachador del puente la
/// devuelve como error del sobre con <see cref="Codigo"/> (DISENO §6.4); el mensaje es para la persona.
/// </summary>
public sealed class ExcepcionDeAutenticacion(int codigo, string mensaje, Exception? causa = null) : Exception(mensaje, causa)
{
    /// <summary><see cref="CodigosDeError.AutenticacionCancelada"/> o <see cref="CodigosDeError.AutenticacionFallida"/>.</summary>
    public int Codigo { get; } = codigo;

    public static ExcepcionDeAutenticacion Cancelada(string mensaje) => new(CodigosDeError.AutenticacionCancelada, mensaje);

    public static ExcepcionDeAutenticacion Fallida(string mensaje, Exception? causa = null) =>
        new(CodigosDeError.AutenticacionFallida, mensaje, causa);
}
