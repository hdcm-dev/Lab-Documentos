# 17 · Observabilidad y pipeline

**Nivel:** avanzado · **Guía:** §13.1–§13.3 · **Objetivo de aprendizaje:** OA-O1 (configurar `UseOpenTelemetry` y comprobar con un exportador en memoria qué contenido sale con `EnableSensitiveData` apagado y encendido).

## Qué muestra

- Un pipeline de `IChatClient` armado con `UseDistributedCache`, `UseOpenTelemetry` y `UseLogging` **encima** del cliente de la capa única (`FabricaDeClientes`). La capa de opciones (razonamiento apagado, `num_ctx`, temperatura y semilla por defecto) queda pegada al adaptador, debajo de todo (`Infrastructure/PipelineObservado.cs`).
- **El orden de las capas cambia lo que ve la telemetría.** Tres pedidos: dos iguales y uno con otra temperatura.
  - *Caché más afuera* (la variante principal): 2 llamadas al modelo y **2 spans**. El acierto de caché no aparece en la telemetría (reproduce N5 E2).
  - *Telemetría más afuera*: las mismas 2 llamadas y **3 spans**. El span del acierto trae tokens, porque la caché devuelve la respuesta guardada con su `Usage` (N5 V3). Para medir costo real, se cuentan las llamadas que llegan al modelo, no los spans.
  - La temperatura es parte de la clave de la caché: el tercer pedido no acierta.
- **La sesión y la versión de los datos van en la clave** (`Infrastructure/CacheConContador.cs`, REC-12 de la recuperación del 2026-10-28). La clave por defecto de `UseDistributedCache` sólo mira los mensajes y las opciones. Con esa clave, dos sesiones que mandan el mismo texto comparten la respuesta, y en CTX-SENS eso es una fuga entre visitantes. Además, después de una escritura confirmada, un acierto sirve la respuesta vieja: un acierto no llama a nada de abajo, tampoco a las herramientas. Por eso el pipeline usa `CacheConContador`, que suma a la clave `[SessionId, VersionDeDatos]` **leídos en cada pedido**. Si se leen al crear la instancia, la versión queda vieja. La capa cuenta los aciertos por los dos caminos de lectura: `ReadCacheAsync` y, en streaming con `CoalesceStreamingUpdates=false`, `ReadCacheStreamingAsync`. Un contador que mira sólo el primero da 0 en ese caso. Lo comprueban sin servidor las pruebas de `Pruebas/CacheDeSesionTests.cs`. Los casos rojos usan la clave por defecto o la versión leída al crear la instancia: B recibe la respuesta de A, la escritura devuelve un acierto viejo y el contador de un solo camino da 0. Con la clave nueva, la misma secuencia queda verde.
- **Contenido en la telemetría sólo en desarrollo.** `EnableSensitiveData` se fija siempre desde el ambiente (`DOTNET_ENVIRONMENT`). La propiedad gana sobre `OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT`. En producción los spans no traen `gen_ai.input.messages`, `gen_ai.output.messages` ni `gen_ai.system_instructions`; en `Development`, sí.
- **Los logs también pueden llevar contenido.** `UseLogging` en `Debug` cuenta que hubo llamadas, sin el texto. En `Trace` escribe los mensajes. Es un contraejemplo y corre sólo con el cliente guionado.
- **La telemetría ve lo que hay en su capa.** Como la capa de opciones está debajo, el span del primer pedido no trae `gen_ai.request.temperature`: la aplicación no la pidió y la completa la capa. El del tercero trae `0.7`. El registro R-LLM, en cambio, guarda las opciones efectivas (`temperature` 0 y 0,7): observadas con `--fake`, reconstruidas con el modelo (ver más abajo).
- **Métricas `gen_ai.*` contra la convención vigente.** El ejemplo lista lo que publica el medidor `Experimental.Microsoft.Extensions.AI` y lo compara con dos listas de `Infrastructure/ConvencionGenAi.cs`. M.E.AI 10.11.0 emite los cuatro nombres de semconv v1.41.0 (`gen_ai.client.token.usage`, `gen_ai.client.operation.*`). El repo vigente (`semantic-conventions-genai`) los reemplazó por `gen_ai.client.inference.*` y sólo conserva `gen_ai.client.operation.duration`, así que un dashboard armado con los nombres vigentes no encuentra las métricas de M.E.AI (desfase de N5 V9, reconfirmado en R5 el 2026-10-26). La convención está en estado Development, así que esas listas envejecen.

Los spans y las métricas se leen con `OpenTelemetry.Exporter.InMemory`, sin colector (`Infrastructure/CapturaDeTelemetria.cs`). En una app real se registra la misma fuente con `AddSource`/`AddMeter` y un exportador OTLP (guía §12.1).

## Cómo se corre

Desde la carpeta de la guía:

```bash
tools/verificar.sh correr 17-Observabilidad-Y-Pipeline                # --fake --verificar, sin servidor
tools/verificar.sh correr 17-Observabilidad-Y-Pipeline --con-modelo   # contra el servidor de su ejemplo.json (compila antes con «build»)
```

Las pruebas viven en el mismo proyecto (`Pruebas/`). `dotnet test --filter "Category!=LLM"` corre las de cableado sin red. La de `[Category("LLM")]` se salta con el motivo del predicado si no hay modelo.

## Entradas

`entradas.v1.json` (escrito para el ejemplo; su SHA-256 está congelado en `ejemplo.json`) tiene **una** entrada con tres pedidos sobre el alta de una localidad: `p1` y `p2` son idénticos y `p3` tiene el mismo texto con temperatura 0,7. Esa temperatura es la variable del experimento y la única opción que fija la entrada. Es una **demostración**: no hay tasa. Estímulo del mismo autor.

## Qué observar

Salida con `--fake` (extracto):

```text
[e01] caché más afuera · ambiente Production · 3 pedidos
  llamada 1 (p1): temperature 0 · razonamiento None · num_ctx 8192 (default) · «Para el alta de una localidad se cargan…»
  llamada 2 (p3): temperature 0.7 · razonamiento None · num_ctx 8192 (default) · «El alta pide nombre, provincia del catálogo…»
  llegaron al modelo: p1, p3 · aciertos de caché: p2
  spans: 2
    span: chat guion | gen_ai.operation.name=chat, …, gen_ai.usage.input_tokens=94, gen_ai.usage.output_tokens=25 | contenido: no
    span: chat guion | …, gen_ai.request.temperature=0.7, … | contenido: no
  logs (Debug): 4 líneas, ninguna con el texto del pedido: True
  métricas publicadas por M.E.AI frente a la convención:
    gen_ai.client.operation.duration (medida): semconv v1.41.0 sí · vigente sí
    gen_ai.client.operation.time_per_output_chunk (sin mediciones): semconv v1.41.0 sí · vigente no
    gen_ai.client.operation.time_to_first_chunk (sin mediciones): semconv v1.41.0 sí · vigente no
    gen_ai.client.token.usage (medida): semconv v1.41.0 sí · vigente no
  oráculo cache-y-telemetria: pasa (…)
comparación (guion) · telemetría más afuera: 2 llamadas, 3 spans; aciertos: p2
comparación (guion) · ambiente Development: 2 de 2 spans con contenido (gen_ai.input.messages, gen_ai.output.messages, gen_ai.system_instructions)
comparación (guion) · UseLogging en Trace: 2 de 4 líneas de log traen el texto del pedido (en Debug: 0)
```

Las dos métricas de streaming (`time_to_first_chunk`, `time_per_output_chunk`) se publican pero no miden nada, porque el ejemplo no hace streaming.

- **Oráculo `cache-y-telemetria`** (`tools/oraculos/cache-y-telemetria.jq`, recalculable desde el registro): llegaron al modelo `p1` y `p3`, en ese orden (`p2` no llegó: fue acierto); las dos llamadas salieron con temperaturas distintas; ninguna salida vacía; cada llamada tuvo su span, y el span trae contenido sólo si el ambiente es `Development`. El campo `paso` de cada línea de llamada guarda el pedido, el span y el ambiente.
- **Lo que el registro no permite recalcular.** El registro R-LLM guarda sólo las llamadas que llegaron al modelo. No guarda la cantidad total de spans ni los instrumentos publicados. Por eso, que el acierto de caché **no genere span** (2 spans con 3 pedidos) y la lista de métricas frente a la convención no los comprueba el oráculo: son invariantes de código, y su rastro queda en la salida de la corrida (`spans: 2`, la lista de métricas), no en el registro.
- **Invariantes de código** (`--verificar` sale con 1 si alguno falla): 2 llamadas al modelo y 2 spans con la caché afuera; el acierto es `p2`, y el contador de la caché da lo mismo que la cuenta por el registrador; el contenido de los spans corresponde al ambiente; toda llamada sale con razonamiento `None` y `num_ctx`; los logs en `Debug` no traen el texto; los instrumentos publicados son los cuatro de semconv v1.41.0 (si M.E.AI cambia los nombres, el invariante falla y hay que volver a verificar el desfase); con la telemetría afuera, 3 spans y el del acierto con tokens; en `Development` el texto del pedido aparece en los spans; en `Trace`, en los logs.
- **Opciones observadas y opciones reconstruidas.** Con `--fake`, las opciones del registro (`razonamiento`, `num_ctx`, `temperature`) se leen de lo que recibió el cliente guionado, debajo de la capa: son observadas. Con el modelo, OllamaSharp no expone lo que recibió, así que `Corrida.cs` las **reconstruye**: clona las opciones de arriba de la capa y les aplica el mismo método de la capa (`OpcionesDelProveedor.Aplicar`). En esa corrida, esos campos y los invariantes «razonamiento `None`» y «con `num_ctx`» se cumplen por construcción y no prueban nada por sí mismos. Lo que garantiza que la capa de verdad aplica esas opciones es la prueba con espía `CapaDeOpciones_DebajoDelPipeline_ApagaElRazonamientoYFijaNumCtx`, que corre sin red.

Con el modelo, sólo la variante principal va contra el servidor y gasta **2 llamadas** (los dos pedidos que no aciertan). Las tres comparaciones corren siempre con el cliente guionado envuelto en la misma capa y no gastan llamadas.

## En este laboratorio

**Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-17, estación 1, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): una entrada, cache-y-telemetria pasa, sin tasa.

La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 13; `// …` marca un recorte.

`Examples/17-Observabilidad-Y-Pipeline/Infrastructure/PipelineObservado.cs`
```csharp
public static bool ContenidoEnTelemetria(string ambiente) =>
    string.Equals(ambiente, Desarrollo, StringComparison.OrdinalIgnoreCase);

// …
var contenido = Ambientes.ContenidoEnTelemetria(ambiente);
// …
void Telemetria(OpenTelemetryChatClient c) => c.EnableSensitiveData = contenido;
// La clave por defecto (mensajes y opciones) cruza sesiones y sirve datos viejos: REC-12.
IChatClient Cache(IChatClient interno) => new CacheConContador(interno, cache, valoresDeLaClave);

var builder = new ChatClientBuilder(registrador);
// En ChatClientBuilder, la primera capa que se agrega es la de más afuera.
builder = orden switch
{
    OrdenDeCapas.CacheAfuera => builder.Use(Cache).UseOpenTelemetry(configure: Telemetria),
    OrdenDeCapas.TelemetriaAfuera => builder.UseOpenTelemetry(configure: Telemetria).Use(Cache),
    _ => throw new ArgumentOutOfRangeException(nameof(orden))
};
return builder.UseLogging(logs).Build();
```

`Examples/17-Observabilidad-Y-Pipeline/Infrastructure/CapturaDeTelemetria.cs`
```csharp
public const string Fuente = "Experimental.Microsoft.Extensions.AI";

// …
public CapturaDeTelemetria()
{
    _trazas = Sdk.CreateTracerProviderBuilder().AddSource(Fuente).AddInMemoryExporter(Spans).Build();
    _metricas = Sdk.CreateMeterProviderBuilder().AddMeter(Fuente).AddInMemoryExporter(_exportadas).Build();
    // …
}
```

Qué mirar: `EnableSensitiveData` se asigna en toda construcción, también cuando vale `false`; el `switch` es lo único que cambia entre los dos órdenes, y la caché es `CacheConContador`, con la sesión y la versión en la clave.
