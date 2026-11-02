// Nuevo. Mutación para MutacionesTests (RL-23), compilada sólo en el ensamblado de pruebas: un tipo fuera
// de Infrastructure.AI y Program que usa un tipo de Comun.AI dentro del cuerpo de un método.
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Tests.Mutantes.Comun;

internal sealed class AdaptadorElegidoAFuera
{
    public string Nombre() => OpcionesDelProveedor.NumCtxDe(null)?.ToString() ?? string.Empty;
}
