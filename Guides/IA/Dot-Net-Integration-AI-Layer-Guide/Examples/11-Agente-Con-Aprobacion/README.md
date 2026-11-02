# 11 · Agente con aprobación

**Dificultad:** avanzado · **Sección de la guía:** §9.7 (agente: cuándo conviene y qué estado aporta) · **Objetivos:** OA-D6 (elegir entre función, llamada con herramientas, workflow y agente, y justificarlo) y OA-S4 (armar el pedido de aprobación con los argumentos que resolvió el código, no con la prosa del modelo).

## Qué enseña

Un `AIAgent` de Microsoft Agent Framework (`AsAIAgent` sobre el mismo `IChatClient` de la capa única) con dos herramientas que fija el código: `listar_localidades` (lectura: id, nombre y provincia) y `eliminar_localidad(nombre, provincia)`, envuelta en `ApprovalRequiredAIFunction`, sobre `LocalidadService.GetByNombreYProvinciaAsync` + `DeleteAsync`. El modelo nunca da un id.

**Sigue en el escalón 1 de la escalera de agencia**: el pedido es acotado («Borrá Resistencia»), las herramientas las monta el código y el bucle tiene tope (`TopeDeIteraciones = 3` por petición). Lo que agrega el agente es **estado**, que es otra dimensión y no sube el escalón: la `AgentSession` se conserva entre **dos peticiones separadas** (la del pedido y la de la decisión) con `SerializeSessionAsync` / `DeserializeSessionAsync`. Cada petición arma de nuevo herramientas y agente; entre una y otra el servidor guarda sólo la sesión serializada y el pedido pendiente.

> **Este ejemplo sube más de lo necesario para la aprobación sola.** La aprobación es de `Microsoft.Extensions.AI` y el ejemplo 09 la resuelve sin agente, con `UseFunctionInvocation`. El agente se justifica acá por la conversación que tiene que sobrevivir entre dos peticiones, no por la aprobación.

Lo que hay que mirar:

1. **El texto del pedido lo arma el código** (`Application/Aprobacion/ServidorDeAprobaciones.cs`). Toma la `FunctionCallContent` del `ToolApprovalRequestContent`, la resuelve contra el caso de uso **dentro de la sesión** y muestra `Borrar "Resistencia" (Chaco), id 2, de tu sesión.`. La prosa del modelo no entra; si nombra otra localidad de la sesión, el pedido lo avisa y se borra lo que dice el pedido.
2. **Si no resuelve en la sesión, no se pregunta.** Una baja de una localidad inexistente, ajena o ambigua se rechaza sin mostrarle nada a la persona; el modelo recibe el motivo como resultado de la herramienta.
3. **Política de origen (regla 2).** Si el modelo lista antes de borrar, el resultado de `listar_localidades` trae texto que la app no escribió (`Nombre`): el pedido queda **marcado** y lo dice («Este pedido viene después de leer texto que la app no escribió.»). La baja sigue necesitando la aprobación armada por el código.
4. **La decisión llega sola.** La segunda petición trae sólo (id del pedido, aprobar o rechazar); la respuesta se arma con `CreateResponse(bool)` sobre el pedido **guardado**. Una respuesta con argumentos distintos se rechaza. La herramienta, además, sólo borra si el id resuelto al ejecutar es el que se aprobó para ese `CallId`.
5. **La sesión guardada es frontera de confianza.** MAF 1.26.0 ejecuta la llamada que dice la `AgentSession` **deserializada**, no la del pedido aprobado: si el JSON guardado se altera entre las dos peticiones (Resistencia → Corrientes), la función recibe Corrientes. Lo muestra el caso rojo de `Pruebas/` (una función sin guarda). Con la composición del ejemplo, la misma alteración llega a la herramienta y la frena la comparación con el id aprobado: cero bajas. Las dos pruebas corren sin modelo (`Category!=LLM`). Límite: si se alteran los dos almacenes (la sesión y el pedido que guarda el servidor), la guarda no alcanza.

Garantías de código que comprueba `--verificar` (sale con 1 si alguna falla), también en la corrida contra el modelo:

- sin aprobación, cero bajas; con «no» (`CreateResponse(false)`), la base no cambia;
- **lo borrado = lo mostrado en el pedido** (ids que faltan en la base = ids de los pedidos aprobados);
- guion del desajuste: el «modelo» dice Resistencia y pide la baja de Corrientes; el pedido muestra Corrientes, lo avisa, y la prueba lo detecta;
- una localidad de otra sesión se rechaza sin preguntar;
- la petición 2 recibe el historial de la 1 sólo por la `AgentSession` deserializada;
- toda llamada al modelo sale con razonamiento apagado y `num_ctx` explícito, y hay un solo bucle de herramientas (el agente no suma otro).

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 11-Agente-Con-Aprobacion                 # --fake, sin red
tools/verificar.sh correr 11-Agente-Con-Aprobacion --con-modelo    # contra el modelo del manifiesto (correr build antes)
```

A mano, en contenedor:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/11-Agente-Con-Aprobacion \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

El proyecto es a la vez consola y pruebas (`GenerateProgramFile=false`). Argumentos: `--fake`, `--verificar`, `--plazo-s N`. Variables: las de `Examples/README.md` (`OLLAMA_ENDPOINT`, `OLLAMA_MODEL`, `OLLAMA_SERVIDOR`, `OLLAMA_NUM_CTX`…). La base es un SQLite temporal con las 2 localidades sembradas del caso base (Corrientes y Resistencia), y se borra al terminar.

## Qué observar

Con `--fake` (guion de 3 llamadas: lista, pide la baja; en la petición 2, cuenta el resultado):

```text
e01: «Borrá Resistencia.»
  pedido de aprobación [ficc_c-e01-b]: Borrar "Resistencia" (Chaco), id 2, de tu sesión. Este pedido viene después de leer texto que la app no escribió.
  la persona aprueba
  ejecutó eliminar_localidad → éxito: Se eliminó la localidad Resistencia.
  asistente (guion): Listo: se dio de baja Resistencia, de Chaco.
  peticiones: 2 · AgentSession serializada entre peticiones: 2 vez/veces · base: 2 → 1 localidades
  llamadas al modelo: 3 · oráculo agente-con-aprobacion (esperado baja): pasa
garantía guionada: con «no», cero bajas y la base no cambia
garantía guionada: desajuste de texto e id: el pedido muestra Corrientes, lo avisa, y se borra lo mostrado
garantía guionada: aprobación con argumentos alterados: se rechaza
garantía guionada: localidad de otra sesión: se rechaza sin preguntar y ninguna base cambia
registro: 4 renglones en 11-Agente-Con-Aprobacion.jsonl
invariantes de código: se cumplen
```

- El campo `paso` de cada renglón del registro dice en qué petición ocurrió la llamada (`peticion-1`, `peticion-2`).
- La primera llamada de la petición 2 lleva el «Borrá Resistencia.» de la 1: llegó por la sesión deserializada, no porque el código lo reenviara.
- Contra el modelo, mirá si lista antes de borrar (el pedido sale marcado) o si llama directo a `eliminar_localidad` (sin marca). Las dos conductas son válidas; la garantía es la misma.

## Entradas, oráculo y registro

| Archivo | Qué es |
|---|---|
| `entradas.v1.json` | Demostración: 1 pedido, «escrito para el ejemplo»: «Borrá Resistencia.». Resistencia, Chaco, 3500, 291720 vienen de `SessionSeeder.cs` @ `8fb4c3a`. Sin tasa. |
| `../../tools/oraculos/agente-con-aprobacion.jq` | Oráculo. `baja`: exactamente una baja con éxito; el pedido aprobado mostraba Resistencia (Chaco) y el id borrado es el id que mostró el pedido. Recalcula desde el registro lo que decide `Corrida.Veredicto`, sin compartir código. |

El SHA-256 de las entradas está congelado en `ejemplo.json`. El esperado y el pedido mostrado viajan en el `motivo` del renglón `entrada` (`esperado=baja;mostrado=<id>|<nombre>|<provincia>`), para que el oráculo se recalcule leyendo sólo el registro.

## Corrida contra el modelo

La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

- **Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-11, estación 2, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): agente-con-aprobacion 10/10, IC95 [0,692; 1], una entrada, 10 repeticiones con semilla 42 fija.
- Las cuatro garantías guionadas corrieron también en esta corrida y se cumplieron.

Registro y logs: `R-LLM/11-Agente-Con-Aprobacion.{fake,modelo}.jsonl` y `11-Agente-Con-Aprobacion/` (registro de trabajo de la edición, que no se publica; procedencia en el Anexo H de la guía). `tools/recalcular-oraculos.sh` recalcula los dos veredictos desde el registro.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 9; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/11-Agente-Con-Aprobacion/Corrida.cs`
```csharp
var agente = ArmarAgente(grabador, herramientas.Crear());
var sesion = await agente.CreateSessionAsync(cancellationToken);
var respuesta = await agente.RunAsync(texto, sesion, cancellationToken: cancellationToken);
// …
estado = (await agente.SerializeSessionAsync(sesion, cancellationToken: cancellationToken)).GetRawText();
// …
var agente = ArmarAgente(grabador, herramientas.Crear());
using var guardado = JsonDocument.Parse(estado);
var sesion = await agente.DeserializeSessionAsync(guardado.RootElement, cancellationToken: cancellationToken);
List<AIContent> respuestas = [.. automaticas, .. decisiones.Select(d => servidor.Responder(d.Id, d.Aprobar))];
var respuesta = await agente.RunAsync(new ChatMessage(ChatRole.User, respuestas), sesion, cancellationToken: cancellationToken);
```

Qué mirar: cada petición arma el agente de nuevo, y lo único que pasa de una a otra es `estado`, la `AgentSession` serializada.

Con `--fake`, el guion lista antes de borrar y el pedido sale marcado («Este pedido viene después de leer texto que la app no escribió.»).
