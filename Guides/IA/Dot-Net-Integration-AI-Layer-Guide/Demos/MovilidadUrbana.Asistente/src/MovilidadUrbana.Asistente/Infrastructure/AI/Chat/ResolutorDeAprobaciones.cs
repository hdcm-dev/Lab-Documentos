// Nuevo. Delta: Agregado por la IA — el pedido de aprobación lo arma el código, no la prosa del modelo (plan §F.4; H-010, H2-009).
using System.Globalization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

/// <summary>
/// Un pedido de aprobación tal como lo ve la persona. <see cref="Descripcion" /> la escribe el código
/// con los datos resueltos dentro de la sesión; la prosa del modelo no aparece.
/// </summary>
public sealed record PedidoDeAprobacion(
    string Id,
    string Herramienta,
    string Descripcion,
    bool DespuesDeTextoNoConfiable,
    ToolApprovalRequestContent Solicitud,
    EscrituraResuelta? Escritura);

/// <summary>Resultado de resolver una solicitud: o un pedido para mostrar, o un rechazo sin preguntar.</summary>
public sealed record Resolucion(PedidoDeAprobacion? Pedido, string? RechazoSinPreguntar);

/// <summary>
/// Resuelve cada <see cref="ToolApprovalRequestContent" /> contra el caso de uso dentro de la sesión:
/// qué localidad, con nombre, provincia e id. Si la escritura no se puede resolver (no existe en la
/// sesión, es ambigua, o el dominio la rechaza), se rechaza sin preguntar y el motivo vuelve al modelo.
/// </summary>
public sealed class ResolutorDeAprobaciones(LocalidadService localidades, NormalizadorDeCatalogo normalizador, EstadoDelPedido estado)
{
    public const string AvisoDeOrigen = "Este pedido viene después de leer texto que la app no escribió.";

    private static readonly CultureInfo Cultura = new("es-AR");

    public async Task<Resolucion> ResolverAsync(ToolApprovalRequestContent solicitud, CancellationToken cancellationToken = default)
    {
        if (solicitud.ToolCall is not FunctionCallContent llamada)
        {
            return new(null, "Herramienta desconocida.");
        }

        switch (llamada.Name)
        {
            case HerramientasDelAsistente.Eliminar:
            {
                var nombre = Argumentos.Texto(llamada.Arguments, "nombre") ?? string.Empty;
                var provincia = normalizador.Provincia(Argumentos.Texto(llamada.Arguments, "provincia"));
                var resolucion = await localidades.GetByNombreYProvinciaAsync(nombre, provincia, cancellationToken);
                if (resolucion.Localidad is not { } l)
                {
                    return new(null, string.Join(" ", resolucion.Error!.Errors.Values));
                }
                var baja = new BajaResuelta(l.Id, l.Nombre, l.Provincia);
                return new(Pedido(solicitud, llamada.Name,
                    $"Eliminar la localidad «{l.Nombre}» ({l.Provincia}), id {l.Id}, de tu sesión.", baja), null);
            }

            case HerramientasDelAsistente.Agregar:
            {
                var modelo = new LocalidadModel
                {
                    Nombre = (Argumentos.Texto(llamada.Arguments, "nombre") ?? string.Empty).Trim(),
                    Provincia = normalizador.Provincia(Argumentos.Texto(llamada.Arguments, "provincia")) ?? string.Empty,
                    CodigoPostal = (Argumentos.Texto(llamada.Arguments, "codigoPostal") ?? string.Empty).Trim(),
                    Habitantes = Argumentos.Entero(llamada.Arguments, "habitantes")
                };
                // El dominio decide antes de preguntar: no se le pide a la persona que apruebe algo que no se puede guardar.
                var errores = new List<string>();
                if (!LocalidadRules.NombreValido(modelo.Nombre)) errores.Add($"Nombre: {LocalidadPolicy.RequisitoDelNombre}");
                if (!LocalidadRules.ProvinciaValida(modelo.Provincia)) errores.Add($"Provincia: {LocalidadPolicy.RequisitoDeLaProvincia}");
                if (!LocalidadRules.CodigoPostalValido(modelo.CodigoPostal)) errores.Add($"Código postal: {LocalidadPolicy.RequisitoDelCodigoPostal}");
                if (!LocalidadRules.HabitantesValidos(modelo.Habitantes)) errores.Add($"Habitantes: {LocalidadPolicy.RequisitoDeLosHabitantes}");
                if (errores.Count > 0) return new(null, "El dominio rechaza el alta. " + string.Join(" ", errores));

                return new(Pedido(solicitud, llamada.Name,
                    $"Agregar la localidad «{modelo.Nombre}» ({modelo.Provincia}), código postal {modelo.CodigoPostal}, " +
                    $"{modelo.Habitantes!.Value.ToString("N0", Cultura)} habitantes, a tu sesión.",
                    new AltaResuelta(modelo)), null);
            }

            default:
                // Una lectura que quedó en la misma respuesta que una escritura también pide aprobación (M.E.AI 10.11.0).
                return new(Pedido(solicitud, llamada.Name, $"Consultar «{llamada.Name}» (lectura, no cambia datos).", null), null);
        }
    }

    private PedidoDeAprobacion Pedido(ToolApprovalRequestContent solicitud, string herramienta, string descripcion, EscrituraResuelta? escritura) =>
        new(solicitud.RequestId, herramienta, estado.Marcado ? $"{descripcion} {AvisoDeOrigen}" : descripcion, estado.Marcado, solicitud, escritura);
}
