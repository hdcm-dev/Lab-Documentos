# 02 · Cambio de adaptador

**Sección de la guía**: §6.6 · **Objetivo**: OA-D1 (consumir un modelo con `IChatClient` y cambiar de adaptador tocando sólo la raíz de composición) · **Tipo**: demostración (1 entrada, sin tasa) · **Recurso**: segundo servidor (`modelo-propio`), TURNO-3, fase 3a.

## Qué muestra

El mismo código de aplicación (`AsistenteDeAlta.cs`, que sólo conoce `IChatClient`) contra **dos adaptadores sobre el mismo servidor**:

| Vía | Adaptador | `num_ctx` | Proveedor que informa `GetService<ChatClientMetadata>()` |
|---|---|---|---|
| nativa | OllamaSharp (`/api/chat`) | 8192, por la capa única de opciones | `ollama` |
| `/v1` | SDK de OpenAI vía `Microsoft.Extensions.AI.OpenAI` (`/v1/chat/completions`) | no llega | `openai` |

Los dos se eligen en `Program.cs` (clase `RaizDeComposicion`), que es el **único** archivo que los nombra. La vía nativa la construye la fábrica común (`FabricaDeClientes.CrearChatOllama`); la de `/v1`, el SDK de OpenAI en la raíz, y se le pone la misma capa con `FabricaDeClientes.Envolver(…, Adaptador.OpenAIv1)`. Ni el ejemplo ni la aplicación fijan razonamiento, `num_ctx`, temperatura o semilla a mano. **No es un cambio de proveedor**: el servidor y el modelo (`qwen2.5:0.5b`) son los mismos. Lo que cambia entre proveedores de verdad (credenciales, nombres de modelo, opciones propias, costos) está en la tabla de diferencias de §6.6 (Tabla 6.1).

El SDK de OpenAI va con `NetworkTimeout` de 10 minutos y **cero reintentos** (`RetryPolicy = new ClientRetryPolicy(maxRetries: 0)`): un reintento contra un servidor en CPU duplica la carga. Ollama no pide clave; el SDK exige una, así que va un valor fijo que el servidor ignora.

## Cómo correrlo

Desde la carpeta de la guía:

```bash
tools/verificar.sh correr 02-Cambio-De-Adaptador               # compila y corre con --fake (sin red)
tools/verificar.sh correr 02-Cambio-De-Adaptador --con-modelo  # contra el segundo servidor (OLLAMA_PROPIO_ENDPOINT)
```

A mano: `dotnet run -- --fake`, o `dotnet run -- --adaptador openai-v1` para una sola vía (`ollamasharp` | `openai-v1`; sin el argumento, las dos). Con una sola vía se ve la selección, pero el oráculo no se evalúa (compara las dos vías de una misma corrida) y no se escribe línea `entrada`. Las pruebas: `dotnet test --filter "Category!=LLM"` (sin servidor) y `--filter Category=LLM` (se salta con su motivo si el segundo servidor no responde).

## Qué observar

- Las dos respuestas salen del mismo modelo; el proveedor informado es distinto (`ollama` y `openai`).
- `num_ctx enviado: 8192` sólo por la vía nativa; por `/v1`, «no (por /v1 no llega)» y `truncado_posible` queda «no verificable».
- En el registro R-LLM, por `/v1` faltan las duraciones del servidor (`prompt_eval_duration_ms`, `total_duration_ms`): el formato de OpenAI no las trae.
- El criterio de OA-D1, que se comprueba solo:

  ```bash
  grep -lE "OllamaSharp|using OpenAI" *.cs   # lista sólo Program.cs
  ```

  La prueba `SoloProgramCs_NombraLosAdaptadores` hace lo mismo.
- **Lo que invente el modelo no es dato del ejemplo.** No se corrige ni se filtra: es la conducta que el ejemplo tiene que dejar ver. El adaptador no cambia lo que el modelo hace, y quien decide son las reglas del dominio: `LocalidadRules.ProvinciaValida` exige una clave exacta de `Catalogos.Provincias` y `CodigoPostalValido` exige `^[0-9]{4}$`, así que una propuesta con una provincia fuera de catálogo o un código postal de 5 dígitos no pasa el alta. La entrada (lo que escribe el autor) sí cumple la regla de datos de Movilidad Urbana. La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.
- **Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-02, estación 1, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c): no medido (el pedido por /v1 con tope de salida no respondió en 600 s; sin reintento).

## Qué viaja por cada vía (prueba de cableado, sin modelo)

`Pruebas/CuerpoDelPedidoTests.cs` arma las dos vías como la raíz de composición, pero con un **servidor falso en memoria** (un `HttpMessageHandler`, sin red) que guarda el cuerpo de cada pedido. Con las mismas opciones (`TopK = 20`, `MaxOutputTokens = 3`, más lo que pone la capa):

| Qué | Vía nativa (`/api/chat`) | `/v1` (`/v1/chat/completions`) |
|---|---|---|
| `TopK` | viaja (`options.top_k`) | se descarta en silencio |
| `MaxOutputTokens` | `options.num_predict` | `max_completion_tokens` |
| `num_ctx` | viaja (8192) | no llega |
| modo de herramienta (`ChatToolMode.Auto`) | no hay campo equivalente | el cliente envía `tool_choice: "auto"` |
| mensaje `tool` | el contenido lleva el resultado de la función **entero** (id de la llamada y resultado) | el contenido es sólo el resultado; el id va en `tool_call_id` |
| clave del SDK | no se manda (Ollama no autentica) | encabezado HTTP `Authorization`, nunca en el cuerpo |

La prueba dice lo que **envía el cliente**, no lo que hace el servidor con eso: que el servidor cumpla `tool_choice`, o que `max_completion_tokens` acote la salida, no se mide acá (no hay servidor). Las credenciales van en el `HttpClient` (o en un `DelegatingHandler`), nunca en las cabeceras del pedido: hay adaptadores que las serializan dentro del cuerpo, que es lo que esta prueba captura.

Los cuerpos quedan en `RLLM_DIR/capturas/02-Cambio-De-Adaptador/` (`cuerpo-<vía>-<caso>.json`, cuatro archivos) y la prueba `LosCuerposCapturados_PasanLaCompuertaDeCabeceras` les pasa `tools/compuerta-cabeceras.sh` («cabeceras = 0»). A mano:

```bash
tools/compuerta-cabeceras.sh "$RLLM_DIR/capturas/02-Cambio-De-Adaptador"   # cabeceras: 0 en 4 cuerpo(s) capturado(s)
```

Este archivo de pruebas nombra los adaptadores porque arma las vías con el transporte falso; el criterio de OA-D1 mira el código de la carpeta del ejemplo (`*.cs`), no `Pruebas/`.

## Salida esperada (`--fake`)

```
[ollamasharp · proveedor «ollama»] Nombre, provincia, código postal y habitantes (guion, vía ollamasharp).
    num_ctx enviado: 8192 · razonamiento: None · truncado_posible: False
[openai-v1 · proveedor «openai»] Nombre, provincia, código postal y habitantes (guion, vía openai-v1).
    num_ctx enviado: no (por /v1 no llega) · razonamiento: None · truncado_posible: no verificable
oráculo dos-adaptadores: pasa · registro en 02-Cambio-De-Adaptador.jsonl
```

Con el modelo, el texto cambia; la forma (dos líneas, proveedores distintos, `num_ctx` sólo en la nativa) no.

## Oráculo y registro

Oráculo `dos-adaptadores` (`tools/oraculos/dos-adaptadores.jq`): por las dos vías hay respuesta no vacía, los adaptadores registrados son los dos y el proveedor (campo `paso` del registro) es distinto. Con `--verificar`, además: razonamiento apagado en toda llamada y `num_ctx` sólo por OllamaSharp. La entrada (`entradas.v1.json`) es **escrita para el ejemplo**.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 6; `// …` marca un recorte.

`Examples/02-Cambio-De-Adaptador/Program.cs`
```csharp
public static IChatClient Crear(Adaptador adaptador, ConfiguracionDelModelo config) => adaptador switch
{
    Adaptador.OllamaSharp => FabricaDeClientes.CrearChatOllama(config.PropioEndpoint, config.PropioModelo, config),
    _ => FabricaDeClientes.Envolver(CrearOpenAIv1(config), config, Adaptador.OpenAIv1)
};

// …
public static IChatClient CrearOpenAIv1(ConfiguracionDelModelo config)
{
    var opciones = new OpenAIClientOptions
    {
        Endpoint = new Uri(new Uri(config.PropioEndpoint), "/v1"),
        NetworkTimeout = FabricaDeClientes.TimeoutDelModelo,
        RetryPolicy = new ClientRetryPolicy(maxRetries: 0)
    };
    return new OpenAIClient(new ApiKeyCredential("ollama"), opciones)
        .GetChatClient(config.PropioModelo)
        .AsIChatClient();
}
```

Qué mirar: las dos ramas del `switch` terminan en la misma capa única (`Envolver`); el SDK de OpenAI exige una clave, y Ollama la ignora [REF-51].

Salida con `--fake` (cliente guionado), sin modelo:

```text
[ollamasharp · proveedor «ollama»] Nombre, provincia, código postal y habitantes (guion, vía ollamasharp).
    num_ctx enviado: 8192 · razonamiento: None · truncado_posible: False
[openai-v1 · proveedor «openai»] Nombre, provincia, código postal y habitantes (guion, vía openai-v1).
    num_ctx enviado: no (por /v1 no llega) · razonamiento: None · truncado_posible: no verificable
```
