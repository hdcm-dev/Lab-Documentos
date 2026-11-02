// Nuevo (ejemplo 14). La forma de HOY (M.E.AI y MAF): un método común con [Description] de
// System.ComponentModel, y AIFunctionFactory lo convierte en AIFunction (nombre, descripción y esquema
// JSON de los parámetros). Es la que enseñan §8.2–§8.3 y el ejemplo 08. Ningún atributo de SK.
using System.ComponentModel;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed class HerramientaMeai(LocalidadService localidades)
{
    public const string Nombre = "contar_localidades";

    [Description("Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.")]
    public Task<ConteoDto> ContarLocalidadesAsync(
        [Description("Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.")] string provincia,
        CancellationToken cancellationToken = default) =>
        ConteoDeLocalidades.ContarAsync(localidades, provincia, cancellationToken);

    /// <summary>La herramienta, como la montaría un ChatOptions.Tools o un AIAgent de MAF.</summary>
    public AIFunction Crear() => AIFunctionFactory.Create(ContarLocalidadesAsync, Nombre);
}
