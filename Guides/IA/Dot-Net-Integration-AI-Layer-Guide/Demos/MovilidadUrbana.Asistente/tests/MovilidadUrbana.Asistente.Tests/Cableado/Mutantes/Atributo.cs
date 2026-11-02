// Nuevo. Mutación para MutacionesTests (REC-26), compilada sólo en el ensamblado de pruebas: un tipo de
// presentación que nombra un tipo de Infrastructure.AI sólo en un atributo. Ni la firma ni el cuerpo lo
// muestran; el atributo, sí. Es interno a propósito: así MVC no lo toma por un controller.
using Microsoft.AspNetCore.Mvc;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

namespace MovilidadUrbana.Asistente.Tests.Mutantes.Atributo;

[ServiceFilter(typeof(EstadoDelPedido))]
internal sealed class AccionConFiltroDeIa
{
    public string Nombre => "accion";
}
