// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Application/DependencyInjection.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — registra ResultadosDeEncuestaService, AltaDesdeTextoService y EncuestaDesdeTextoService (§F.2); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application;

/// <summary>Registra los casos de uso. Lo invoca cada punto de composición: la web y la API.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LocalidadService>();
        services.AddScoped<EncuestaService>();
        services.AddScoped<ResultadosDeEncuestaService>();
        services.AddScoped<AltaDesdeTextoService>();
        services.AddScoped<EncuestaDesdeTextoService>();
        return services;
    }
}
