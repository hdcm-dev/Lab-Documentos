// Nuevo (plan §E.1, «Opciones propias del proveedor, en un solo lugar»; H-007). Es el ÚNICO lugar
// donde se fijan razonamiento, num_ctx, temperatura y semilla: los ejemplos, el juez de 15 y los
// agentes de MAF reciben el cliente ya envuelto y no construyen el suyo.
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;

namespace MovilidadUrbana.Asistente.Comun.AI;

/// <summary>Qué adaptador hay debajo: sólo OllamaSharp lee <c>num_ctx</c> (por <c>/v1</c> no llega: H2-016).</summary>
public enum Adaptador { OllamaSharp, OpenAIv1 }

public static class OpcionesDelProveedor
{
    /// <summary>
    /// Agrega la capa de opciones. En TODA llamada: razonamiento apagado si nadie lo pidió,
    /// temperatura y semilla fijas si nadie las fijó, y, con OllamaSharp, <c>num_ctx</c> explícito
    /// (Ollama trunca en silencio sin él: R-03).
    /// </summary>
    public static ChatClientBuilder UseOpcionesDelProveedor(
        this ChatClientBuilder builder, ConfiguracionDelModelo configuracion, Adaptador adaptador)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        return builder.ConfigureOptions(options => Aplicar(options, configuracion, adaptador));
    }

    /// <summary>Lo mismo, sobre unas opciones sueltas (lo usa la capa y lo prueban las pruebas de cableado).</summary>
    public static void Aplicar(ChatOptions options, ConfiguracionDelModelo configuracion, Adaptador adaptador)
    {
        options.Reasoning ??= new ReasoningOptions { Effort = ReasoningEffort.None };
        options.Temperature ??= configuracion.Temperatura;
        options.Seed ??= configuracion.Semilla;
        if (adaptador == Adaptador.OllamaSharp)
        {
            options.AddOllamaOption(OllamaOption.NumCtx, configuracion.NumCtx);
        }
    }

    /// <summary>Lee el <c>num_ctx</c> que la capa dejó en las opciones, o null si no hay (vía /v1).</summary>
    public static int? NumCtxDe(ChatOptions? options) =>
        options?.AdditionalProperties is { } extra &&
        extra.TryGetValue(OllamaOption.NumCtx.Name, out var valor) && valor is not null
            ? Convert.ToInt32(valor, System.Globalization.CultureInfo.InvariantCulture)
            : null;
}
