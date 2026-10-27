using Microsoft.Extensions.DependencyInjection;
using FrameworkConcept.Core.Application.Encuestas;
using FrameworkConcept.Core.Application.Localidades;

namespace FrameworkConcept.Core.Application;

/// <summary>Registra los casos de uso. Lo invoca cada punto de composición (en la demo, el contenido Blazor).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LocalidadService>();
        services.AddScoped<EncuestaService>();
        return services;
    }
}
