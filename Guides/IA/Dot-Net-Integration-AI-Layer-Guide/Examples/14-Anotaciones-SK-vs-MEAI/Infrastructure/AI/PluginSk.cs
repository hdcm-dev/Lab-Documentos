// Nuevo (ejemplo 14). La forma ANTERIOR (Semantic Kernel 1.81.0): el mismo método, marcado con
// [KernelFunction] y agrupado en un plugin con KernelPluginFactory. La [Description] es la MISMA de
// System.ComponentModel: lo que SK agrega es [KernelFunction] y el plugin. La guía de migración de
// Microsoft (N4 F29) pasa de esta forma a la de HerramientaMeai.cs borrando [KernelFunction] y
// reemplazando KernelPluginFactory por AIFunctionFactory.
using System.ComponentModel;
using Microsoft.SemanticKernel;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

public sealed class PluginSk(LocalidadService localidades)
{
    public const string NombreDelPlugin = "Localidades";

    [KernelFunction("contar_localidades")]
    [Description("Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.")]
    public Task<ConteoDto> ContarLocalidadesAsync(
        [Description("Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.")] string provincia,
        CancellationToken cancellationToken = default) =>
        ConteoDeLocalidades.ContarAsync(localidades, provincia, cancellationToken);

    /// <summary>El plugin, como se lo agregaría a kernel.Plugins.</summary>
    public KernelPlugin Crear() => KernelPluginFactory.CreateFromObject(this, NombreDelPlugin);
}
