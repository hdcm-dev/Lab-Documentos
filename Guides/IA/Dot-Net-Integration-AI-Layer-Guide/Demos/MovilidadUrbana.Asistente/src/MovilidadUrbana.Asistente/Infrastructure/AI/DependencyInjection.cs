// Nuevo. Delta: Agregado por la IA — AddAI(config): la raíz de composición de la capa de IA (plan §F.2, §F.3).
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure.AI.Ayuda;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;
using MovilidadUrbana.Asistente.Infrastructure.AI.Extraccion;

namespace MovilidadUrbana.Asistente.Infrastructure.AI;

/// <summary>Lo que la página muestra del modo en que corre: con modelo o con guion, y qué plan de embeddings.</summary>
public sealed record ModoDelAsistente(bool Guion, string Modelo, string Servidor, string PlanDeEmbeddings);

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa de IA. Ciclo de vida (ADR-12): el <see cref="IChatClient" /> base es singleton y
    /// ya viene envuelto en la capa única de opciones del proveedor (<see cref="FabricaDeClientes" />,
    /// Examples/_Comun); las herramientas, el estado del pedido y la conversación son scoped (por
    /// petición en la API, por circuito en Blazor).
    /// </summary>
    /// <param name="entorno">Variables propias de la demo (<c>ASISTENTE_MODO</c>, <c>EMB_PERSISTIDOS</c>); las canónicas ya están en <paramref name="config" />.</param>
    public static IServiceCollection AddAI(this IServiceCollection services, ConfiguracionDelModelo config, Func<string, string?> entorno)
    {
        var guion = entorno("ASISTENTE_MODO") == "guion";
        var persistidos = entorno("EMB_PERSISTIDOS");
        var plan = config.EmbPlan == "EMB-C"
            ? "EMB-C: no ejecutado contra un modelo de embeddings"
            : !string.IsNullOrEmpty(persistidos)
                ? $"{config.EmbPlan}: vectores persistidos de {config.EmbedModelo}"
                : $"{config.EmbPlan}: {config.EmbedModelo}";

        services.AddSingleton(config);
        services.AddSingleton(new ModoDelAsistente(guion, guion ? "guion" : config.Modelo, guion ? "ninguno" : config.ServidorTexto, plan));

        // Cliente base singleton: el SDK del proveedor sólo aparece acá y en la capa única.
        services.AddChatClient(_ => guion
            ? FabricaDeClientes.Envolver(new ClienteDeDemostracion(), config, Adaptador.OllamaSharp)
            : FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config));

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
            config.EmbPlan == "EMB-C" ? new GeneradorDeEmbeddingsEstable(RegistroDeAyuda.Dimensiones)
            : !string.IsNullOrEmpty(persistidos) ? GeneradorDeEmbeddingsPersistidos.Leer(persistidos)
            : FabricaDeClientes.CrearEmbeddingsOllama(config.EmbedEndpoint, config.EmbedModelo));

        services.AddSingleton(sp => new AyudaConRecuperacion(
            sp.GetRequiredService<IChatClient>(), sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(), plan));
        services.AddSingleton<IAyuda>(sp => sp.GetRequiredService<AyudaConRecuperacion>());

        services.AddSingleton<NormalizadorDeCatalogo>();
        services.AddScoped<IExtractorDeLocalidad, ExtractorDeLocalidad>();
        services.AddScoped<IExtractorDeEncuesta, ExtractorDeEncuesta>();

        services.AddScoped<EstadoDelPedido>();
        services.AddScoped<HerramientasDelAsistente>();
        services.AddScoped<ResolutorDeAprobaciones>();
        services.AddScoped<SesionDeChat>();
        return services;
    }

    /// <summary>
    /// Al arrancar: con EMB-A (embeddings en el mismo servidor del chat) o EMB-B (en un servidor
    /// aparte) en vivo, comprueba que el servidor de embeddings declare la capacidad <c>embedding</c>
    /// y, si no, corta con un error que dice cómo pasar a EMB-C. La demo no degrada sola. Después
    /// reconstruye el almacén. Los dos planes existen por papeles separados y por la recarga del modelo;
    /// historia: hasta el 2026-10-28 el servidor compartido de entonces respondía 501 a embeddings (Sondeo §5).
    /// </summary>
    public static async Task PrepararAIAsync(this IServiceProvider services, Func<string, string?> entorno, CancellationToken cancellationToken = default)
    {
        var config = services.GetRequiredService<ConfiguracionDelModelo>();
        if (config.EmbPlan != "EMB-C" && string.IsNullOrEmpty(entorno("EMB_PERSISTIDOS")))
        {
            var r = await PredicadoDeSalto.EvaluarAsync(config.EmbedEndpoint, config.EmbedModelo, PredicadoDeSalto.Embeddings, cancellationToken);
            if (r.Saltar)
            {
                throw new InvalidOperationException(
                    $"{config.EmbPlan} necesita un servidor de embeddings con «{config.EmbedModelo}» en OLLAMA_EMBED_ENDPOINT: {r.Motivo} " +
                    "Para correr sin modelo de embeddings, exportá EMB_PLAN=EMB-C (la ayuda queda rotulada «no ejecutado contra un modelo de embeddings»).");
            }
        }
        await services.GetRequiredService<AyudaConRecuperacion>().PrepararAsync(cancellationToken);
    }
}
