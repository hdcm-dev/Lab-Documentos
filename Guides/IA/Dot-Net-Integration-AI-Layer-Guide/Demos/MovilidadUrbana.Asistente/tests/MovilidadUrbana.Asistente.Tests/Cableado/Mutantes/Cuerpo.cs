// Nuevo. Mutación para MutacionesTests (REC-26), compilada sólo en el ensamblado de pruebas: un tipo de
// presentación que usa un tipo de Infrastructure.AI únicamente dentro del cuerpo de un método. La firma
// no lo muestra; el IL, sí.
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente.Tests.Mutantes.Cuerpo;

internal sealed class RotuloDelModo
{
    public string Rotulo(string modelo) => new ModoDelAsistente(false, modelo, "ninguno", "EMB-C").ToString();
}
