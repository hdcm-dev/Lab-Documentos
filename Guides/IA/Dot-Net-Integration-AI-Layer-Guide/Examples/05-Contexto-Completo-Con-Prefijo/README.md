# 05 · Contexto completo con prefijo estable

**Sección de la guía**: §7.3 a §7.5 (Parte IV, «Conocimiento»). **Objetivo de aprendizaje**: OA-D3, elegir entre contexto fijo, contexto completo con prefijo estable y RAG para un corpus dado, con los criterios para pasar de uno a otro.

## Qué muestra

Si el corpus entra en la ventana, se puede mandar **entero** en cada consulta. Lo que cuesta es que el servidor lo procese cada vez. Ollama puede reutilizar el procesamiento de un prefijo que ya vio, y la aplicación controla una sola cosa de ese mecanismo: **el texto que manda**. El prefijo tiene que ser idéntico byte a byte entre consultas, y la pregunta va al final.

El ejemplo manda las mismas 6 preguntas sobre el mismo padrón de dos formas:

- **sin-prefijo**: antes del prefijo va un *nonce* distinto en cada consulta (`Consulta <16 hex>`). Como el texto ya cambia en el primer token, el servidor no puede reutilizar nada y procesa todo.
- **con-prefijo**: el prefijo exacto, sin cambios.

Además hace una **vuelta a frío**: el mismo prefijo con un solo carácter cambiado, el primero (`P` → `p`). Alcanza con eso para perder toda la reutilización.

Sólo se miden duraciones y conteos **del servidor** (`prompt_eval_duration_ms`, `load_duration_ms`, `prompt_eval_count`), nunca el reloj, porque el reloj suma la red entre máquinas. El orden de las llamadas es fijo y alternado (sin-prefijo 1, con-prefijo 1, sin-prefijo 2, …), y la vuelta a frío va al final.

**La calidad de la respuesta no se mide.** El modelo de chat del ejemplo es `qwen3.5:4b`, como en el resto de la guía; el ejemplo mide duraciones del servidor, no aciertos. No hay variante RAG: la comparación de calidad entre contexto completo y RAG queda como deuda declarada (§15.6, RV-02). Con `qwen3.5:4b` (estación 1, Ollama 0.40.11, digest `d8b0f5e9760c`, `num_ctx` 8.192, 2026-11-01) ningún par bajó de la mitad: desde p02 el con-prefijo tardó cerca del 61 % del sin-prefijo y `estado_cache` quedó «frio» en todas las llamadas. Las corridas con `qwen2.5:0.5b` que siguen son anteriores a la versión 0.12.0 del libro y se conservan porque muestran el mecanismo; su procedencia está en el Anexo H.

## Corpus y entradas (escritos para el ejemplo)

- `corpus/padron-de-localidades.v1.json`: 70 `LocalidadModel` ficticios, «Localidad 01» a «Localidad 70». La provincia sale de `Catalogos`, el código postal tiene 4 dígitos y cada registro pasa `LocalidadRules`. Los genera `PadronDeLocalidades.Generar()`, se serializaron una sola vez (`dotnet run -- --generar-padron …`) y quedan congelados con su SHA-256 en `ejemplo.json`. Una prueba comprueba que el archivo es exactamente lo que da el generador. Es un estímulo del mismo autor, y también lo son las cifras que produce.
- `entradas.v1.json`: 6 preguntas sobre el padrón, también congeladas.
- En texto, el prefijo mide 3.098 caracteres. El servidor informa unos 1.710 a 1.735 tokens de prompt.

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 05-Contexto-Completo-Con-Prefijo                # --fake --verificar, sin servidor
tools/verificar.sh correr 05-Contexto-Completo-Con-Prefijo --con-modelo   # contra el segundo servidor (turno modelo-propio, fase 3a)
```

A mano, en contenedor, desde `Examples/`:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD":/src -w /src/05-Contexto-Completo-Con-Prefijo \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

Contra el segundo servidor hay que exportar `OLLAMA_PROPIO_ENDPOINT` (por defecto `http://localhost:11434`) y `OLLAMA_PROPIO_MODEL` (por defecto `qwen2.5:0.5b`). `num_ctx` sale de la capa única: 8192, explícito. Opciones del programa: `--orden alternado` (el del plan, por defecto, y el único que cuenta) o `--orden calentamiento` (una exploración que **no se adoptó**, ver abajo), y `--plazo-s N`.

`--verificar` sale con 0 sólo si se cumplen los invariantes **de código**, es decir, lo que la aplicación controla:

- las 13 llamadas del plan, en su orden;
- el prefijo primero y la pregunta al final;
- con-prefijo idéntico byte a byte;
- en sin-prefijo, un nonce distinto al principio y el resto igual al prefijo;
- en la vuelta a frío, un solo carácter cambiado, el primero;
- razonamiento apagado y `num_ctx` 8192 en toda llamada.

Lo que haga el servidor no entra en el código de salida: lo decide el oráculo.

## Qué observar

**Oráculo** `par-prefijo` (`Application/OraculoDelPar.cs`, recalculable con `../../tools/oraculos/par-prefijo.jq`). Un par es **a favor** si el `prompt_eval_duration_ms` de con-prefijo es menor que la mitad del de sin-prefijo. En el último par, además, la vuelta a frío **no** tiene que bajar de esa mitad. Si una llamada es `truncado_posible`, o si faltan sus duraciones, el par queda excluido. Por la regla de §10.4, «con-prefijo ≪ sin-prefijo» se afirma sólo con 6 de 6 a favor.

**Modo falso**: el guion pasa por un `SimuladorDeCacheDePrefijo` de dos ranuras con duraciones **inventadas**, y el registro lo marca con modelo y máquina `guion`. Reproduce la forma de lo observado, no las cifras.

**Observado en el laboratorio** (`qwen2.5:0.5b` @ `a8b0c5157701`, Ollama 0.40.11 propio, sólo CPU, 2026-10-27; estímulo del mismo autor). Registros en la carpeta R-LLM del registro de trabajo de la edición, que no se publica (procedencia en el Anexo H de la guía):

| Corrida | sin-prefijo (ms) | con-prefijo (ms) | Vuelta a frío (ms) | Pares a favor |
|---|---|---|---|---|
| Corrida 1 · plan, alternado (13 llamadas) | 9.073 a 14.121 | p01: 13.585 · p02 a p06: 230 a 439 | 13.779 | **5 de 6** (p01 falla) |
| Corrida 2 · exploración con calentamiento (no adoptada), modelo ya cargado (ver abajo) | 9.318 a 14.011 | 224 a 329 | 87 (cacheada) | 5 de 6 (p06 falla por la vuelta) |
| Corrida 4 · plan, alternado, repetición (13 llamadas; modelo descargado, 3.096 ms de carga) | 17.583 a 23.873 | p01: 19.001 · p02 a p06: 385 a 487 | 17.881 | **5 de 6** (p01 falla) |

La corrida 4 repite la 1 con el mismo código y las mismas entradas (`R-LLM/05-Contexto-Completo-Con-Prefijo.modelo.corrida-4.jsonl`). Las duraciones absolutas cambian entre corridas por la carga de la máquina, y por eso no se comparan; se comparan cocientes: lo que se repite es la forma (cada con-prefijo de p02 a p06 queda entre 0,017 y 0,024 del sin-prefijo de su par, y p01 sale en contra).

**Resultado del ejemplo: 5 de 6 pares a favor** (corrida 4, la del registro que cuenta; la corrida 1 dio lo mismo). Por la regla de §10.4, «con-prefijo ≪ sin-prefijo» **no se afirma**: hacen falta 6 de 6. Lo que sí se puede decir, de forma descriptiva, es que en p02 a p06 el con-prefijo tardó entre 0,017 y 0,024 del sin-prefijo de su par, y que p01 salió en contra por la razón que sigue.

- **Por qué p01 llega en frío.** La primera llamada de la corrida es la sin-prefijo de p01, que empieza con un *nonce*: su texto ya difiere del prefijo en el primer token, así que no deja nada que el con-prefijo pueda reutilizar. La segunda llamada, la con-prefijo de p01, es entonces la **primera vez** que el servidor ve ese prefijo y lo procesa entero (19.001 ms en la corrida 4, del orden de su sin-prefijo). Recién desde p02 el con-prefijo encuentra el prefijo ya procesado. Con el orden del plan, 6 de 6 no se alcanza por diseño, cualquiera sea el servidor.
- **El prefijo sobrevive a la consulta intercalada.** Con `OLLAMA_NUM_PARALLEL=1`, entre dos con-prefijo se mete una sin-prefijo con otro comienzo, y aun así el segundo con-prefijo sale cacheado (en la corrida 1, de 230 a 439 ms, contra unos 14.000). Es una conducta del motor, no un contrato (§6.3).
- **La caché sobrevive a la corrida.** La corrida 2 empezó 2 minutos después de la 1, con el modelo todavía cargado (`OLLAMA_KEEP_ALIVE=10m`). Su calentamiento salió cacheado (192 ms), y su vuelta a frío también (87 ms): el mismo prefijo «p…» se había mandado en la corrida anterior. Una vuelta a frío vale sólo si ese texto no se mandó dentro del `keep_alive`. Por eso el programa avisa cuando el modelo ya estaba cargado al empezar.
- `prompt_eval_count` casi no cambia entre frío y cacheado (de 1.710 a 1.735 tokens). En estas corridas sólo la duración distinguió los dos estados, como ya había medido N3b X4b: el programa todavía no leía `prompt_eval_cached_count` (ver «Lo que no se registró»).

## Exploración no adoptada: calentar el prefijo antes del primer par

**Decisión de la presidencia (2026-10-27): 05 mantiene el orden del plan.** La variante con calentamiento no reemplaza al orden alternado, sus corridas no son el resultado del ejemplo y la guía no cita su 6 de 6 para afirmar «con-prefijo ≪ sin-prefijo». Queda acá como registro de lo que se probó y porque explica, por contraste, el primer par. El plan (§E.2) fija el orden alternado y pide 6 de 6 a favor para afirmar «con-prefijo ≪ sin-prefijo». Con ese orden, el primer par sale siempre en contra (ver arriba). La variante `--orden calentamiento` agrega **una** llamada con-prefijo antes del primer par (`c00`, paso `calentamiento`), que no cuenta para ningún par, y deja el resto del plan igual: 14 llamadas. Su registro va a otro archivo (`05-Contexto-Completo-Con-Prefijo.calentamiento.jsonl`). La corrida 3 se hizo con el modelo descargado: antes de lanzarla se esperó, con el turno tomado, a que `/api/ps` saliera vacío. Registros en la Evidencia: `R-LLM/05-Contexto-Completo-Con-Prefijo.modelo.jsonl` (corrida 1), `….calentamiento.corrida-2.jsonl` y `….calentamiento.corrida-3.jsonl`.

| Corrida | sin-prefijo (ms) | con-prefijo (ms) | Vuelta a frío (ms) | Pares a favor |
|---|---|---|---|---|
| Corrida 3 · exploración con calentamiento (no adoptada), modelo descargado (14 llamadas) | 11.758 a 13.005 | 202 a 297 | 12.960 | **6 de 6** |

En la corrida 3 el calentamiento `c00` llega en frío (8.499 ms, con 828 ms de carga del modelo), los seis con-prefijo salen cacheados (de 0,016 a 0,025 del sin-prefijo de su par) y la vuelta a frío vuelve a frío. Esa corrida muestra que, si el prefijo ya fue procesado antes del primer par, el primer par también sale a favor; es lo que explica el 5 de 6 del orden del plan. **No** convierte el resultado del ejemplo en 6 de 6: el calentamiento agrega una llamada que el plan no tiene y la variante no se adoptó. Cifras de un estímulo del mismo autor, `qwen2.5:0.5b` @ `a8b0c5157701`, Ollama 0.40.11 propio, sólo CPU, 2026-10-27.

## Lo que no se registró

- `estado_cache`, en las corridas 1 a 4: ninguna llamada quedó clasificada como fría o cacheada. En esas corridas `_Comun` lo deducía sólo por la duración, con el umbral en ms por token que calibra DEP-03 (`RLLM_UMBRAL_MS_POR_TOKEN`), y DEP-03 no corrió. Desde REC-22 el cliente de 05 pasa por el lector de `_Comun` (`LectorDePrefijoCacheado`, en el `HttpClient` de `CrearChatOllama`, y `LecturaDelServidorEnLaRespuesta`, en `Envolver`), que toma de la línea final de `/api/chat` el `prompt_eval_cached_count` y no guarda el cuerpo. El registro lleva ese número y `estado_cache` sale `cacheado` si el servidor reutilizó más de la mitad del prompt, `frio` si no; si el servidor no lo informa, decide la deducción por duración de antes. En `--fake` el número lo informa el simulador y es **inventado** (`CorridaFalsaTests.EstadoCache_SaleDelValorObservado`). Con el servidor todavía no está medido. El oráculo no lo usa: compara duraciones del mismo par.
- `carga_1min` va `null`: el gestor de recursos no expone la carga de la máquina del Ollama propio al contenedor de la corrida. La regla «excluida por carga ajena» (umbral de DEP-03) no se pudo aplicar. En la corrida 3 se tomó a mano, fuera del registro, la carga de un minuto de esa máquina: 0,90 antes y 3,49 después (en la Evidencia, `Arnes/05-Contexto-Completo-Con-Prefijo.calentamiento.corrida-3.log`). Las corridas 1 y 2 no la tienen.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 7; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/05-Contexto-Completo-Con-Prefijo/Application/PrefijoEstable.cs`
```csharp
public static string Construir(string padronEnTexto) =>
    "Padrón de localidades de Movilidad Urbana (escrito para el ejemplo: 70 registros ficticios, " +
    "«Localidad 01» a «Localidad 70»; no son datos reales).\n" +
    "Respondé en castellano, en una o dos oraciones, usando sólo este padrón. " +
    "Si el dato no está en el padrón, decí «no está en el padrón».\n\n" +
    padronEnTexto;

/// <summary>
/// Variante «sin-prefijo»: un nonce distinto AL PRINCIPIO. Desde el primer token el texto ya
/// no coincide con nada cacheado, así que el servidor tiene que procesar todo el prefijo.
/// </summary>
public static string ConNonce(string prefijo, string nonce) => $"Consulta {nonce}\n{prefijo}";
```

`Examples/05-Contexto-Completo-Con-Prefijo/Application/CorridaDelPrefijo.cs`
```csharp
foreach (var consulta in plan)
{
    // …
    List<ChatMessage> mensajes = [new(ChatRole.System, consulta.Sistema), new(ChatRole.User, consulta.Pregunta)];
    var opciones = new ChatOptions();
    // …
        respuesta = await cliente.GetResponseAsync(mensajes, opciones, cancellationToken);
```

`Examples/05-Contexto-Completo-Con-Prefijo/Application/OraculoDelPar.cs`
```csharp
public const double UmbralAFavor = 0.5;

// …
    var razon = con.PromptEvalDurationMs.Value / sin.PromptEvalDurationMs.Value;
    var aFavor = razon < UmbralAFavor;
    // La vuelta a frío NO tiene que aprovechar la caché: si también saliera rápida, lo que se midió
    // no es el prefijo (p. ej. el servidor cachea otra cosa) y el par falla.
    var vuelta = llamadas.FirstOrDefault(l => l.Paso == Pasos.VueltaAFrio);
```

Qué mirar: el prefijo va primero y la pregunta al final. A diferencia de 01–04 y 07, el prefijo va como mensaje de sistema en la lista y no en `ChatOptions.Instructions`: así `--verificar` comprueba byte a byte el primer mensaje que sale (`CorridaDelPrefijo.cs`), lo único que la aplicación controla. No se verificó que el adaptador ponga `Instructions` en la misma posición.

Salida con `--fake` (cliente guionado), sin modelo; las duraciones y los tokens reutilizados los inventa el simulador del ejemplo, y `estado_cache` sale de ese valor, como saldría del que informa el servidor (§6.7 del libro):

```text
05 · orden alternado (plan) · modo falso: duraciones SIMULADAS
padrón: 70 registros, sha256 cefe78c3b49a; prefijo 3098 caracteres
entrada | paso          | prompt_eval_count | prompt_eval_ms | load_ms | estado_cache
p01     | sin-prefijo   |               799 |          15980 |       1 | frio
p01     | con-prefijo   |               793 |        15820.4 |       1 | frio
p02     | sin-prefijo   |               795 |        15820.8 |       1 | frio
p02     | con-prefijo   |               788 |          355.6 |       1 | cacheado
[…]
p06     | vuelta-a-frio |               787 |        15700.4 |       1 | frio
par p01: falla (con/sin = 0.990) — con-prefijo no bajó de la mitad
par p02: pasa (con/sin = 0.022)
[…]
pares a favor: 5 de 6 válidos (0 excluidos). No se afirma «con-prefijo ≪ sin-prefijo» (hace falta 6 de 6, §10.4).
```

El «cacheado» de p02 es del simulador: las corridas reales con `qwen3.5:4b` (2026-10-29 y 2026-11-01) no reprodujeron ese estado.

Probá `--orden calentamiento` (`docker run … -w /src/05-Contexto-Completo-Con-Prefijo … dotnet run -- --fake --orden calentamiento`, como en E.5), una exploración no adoptada que agrega una con-prefijo antes del primer par. Explica por qué p01 llega en frío, pero no es el resultado del ejemplo.
