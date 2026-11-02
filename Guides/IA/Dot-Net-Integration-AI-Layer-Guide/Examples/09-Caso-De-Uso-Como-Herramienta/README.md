# 09 · Caso de uso como herramienta

**Dificultad:** intermedio · **Sección de la guía:** §9.4–§9.5 (herramientas de escritura sobre los casos de uso, con aprobación) · **Objetivos:** OA-D5 (exponer un caso de uso como herramienta) y OA-S2 (ninguna escritura sin aprobación humana).

## Qué enseña

El modelo no escribe en la base: **pide** que se ejecute un caso de uso, y el caso de uso decide. Dos herramientas de escritura sobre `LocalidadService` (de `_Comun`, copiado del caso base):

| Herramienta | Caso de uso | Control |
|---|---|---|
| `agregar_localidad(nombre, provincia, codigoPostal, habitantes)` | `SaveAsync` | `ApprovalRequiredAIFunction` |
| `eliminar_localidad(nombre, provincia)` | `GetByNombreYProvinciaAsync` + `DeleteAsync` (error si es ambiguo) | `ApprovalRequiredAIFunction` |

Las dos van bajo `UseFunctionInvocation`, **sin MAF**: la aprobación no necesita un agente (escalón 1). Lo que hay que mirar:

1. **La aprobación vive en el servidor y va en dos peticiones** (`Application/Aprobacion/ServidorDeAprobaciones.cs`). La primera petición termina con un `ToolApprovalRequestContent`; el código lo guarda y arma **lo que ve la persona** a partir de los argumentos, no de la prosa del modelo. La segunda petición trae sólo el id y la decisión, y la respuesta se arma con `CreateResponse` sobre el pedido guardado. Si llega una llamada con argumentos distintos de los que se mostraron, se rechaza (`Aprobacion_ArgumentosAlterados_SeRechaza`).
2. **Aprobar no es validar.** En la demostración la persona aprueba todo lo que ve, también el CP «34». El que no deja escribir es `SaveAsync` (`LocalidadRules`: 4 dígitos), y su `Result` **vuelve al modelo** como resultado de la herramienta (`ResultadoDeHerramienta`: `exito`, `mensaje`, `errores`, `id`). El modelo se lo cuenta a la persona.
3. **Una baja que no resuelve no se pregunta.** Si `eliminar_localidad` nombra una localidad que no está en la sesión, o un nombre que sin provincia es ambiguo, el código rechaza sin mostrarle nada a la persona y el modelo recibe el motivo. El modelo nunca da un id.
4. **La sesión la pone el código** (`SessionContext.Set`), una por entrada, con las 2 localidades sembradas del caso base (Corrientes y Resistencia).

Garantías de código que comprueba `--verificar` (sale con 1 si alguna falla):

- nunca queda en la base una localidad con CP inválido;
- ninguna escritura sin aprobación: todo `CallId` ejecutado está entre los aprobados;
- lo que se escribe es lo que mostró el pedido (la fila guardada coincide con los argumentos mostrados);
- una aprobación con argumentos alterados se rechaza;
- toda llamada al modelo sale con razonamiento apagado y `num_ctx` explícito (capa única de `_Comun`).

Si la sonda DEP-09 fallara, la aprobación se enseñaría en 11 (con `AgentSession`) y este README diría por qué. La corrida de abajo muestra el bucle de `FunctionInvokingChatClient` con aprobación andando en dos peticiones, sin agente, contra `qwen3.5:4b`; la sonda DEP-09 es una pieza aparte del presupuesto (§E.5) y su veredicto vive en su propio registro.

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 09-Caso-De-Uso-Como-Herramienta                 # --fake, sin red
tools/verificar.sh correr 09-Caso-De-Uso-Como-Herramienta --con-modelo    # contra el modelo del manifiesto (correr build antes)
```

A mano, en contenedor:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/09-Caso-De-Uso-Como-Herramienta \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

El proyecto es a la vez consola y pruebas (`GenerateProgramFile=false`). Argumentos: `--fake`, `--verificar`, `--plazo-s N`. Variables: las de `Examples/README.md` (`OLLAMA_ENDPOINT`, `OLLAMA_MODEL`, `OLLAMA_NUM_CTX`…). La base es un SQLite temporal que se borra al terminar.

## Qué observar

Con `--fake`:

```text
e01: «Agregá Goya, Corrientes, CP 34, 90000 habitantes.»
  pedido de aprobación [ficc_c-e01]: Dar de ALTA la localidad «Goya», provincia «Corrientes», CP «34», 90000 habitantes.
  la persona aprueba
  ejecutó agregar_localidad → rechazo: Revise los campos marcados en rojo. (codigoPostal: El código postal debe tener 4 dígitos.)
  asistente (guion): No pude agregar Goya: el código postal debe tener 4 dígitos. ¿Me pasás el código postal completo?
  llamadas al modelo: 2 · oráculo alta-con-aprobacion (esperado rechazo): pasa
e02: «Agregá Goya, Corrientes, CP 3450, 90000 habitantes.»
  pedido de aprobación [ficc_c-e02]: Dar de ALTA la localidad «Goya», provincia «Corrientes», CP «3450», 90000 habitantes.
  la persona aprueba
  ejecutó agregar_localidad → éxito: Se agregó la localidad Goya.
  asistente (guion): Listo: se agregó la localidad Goya, en Corrientes.
  llamadas al modelo: 2 · oráculo alta-con-aprobacion (esperado alta): pasa
registro: 6 renglones en 09-Caso-De-Uso-Como-Herramienta.jsonl
invariantes de código: se cumplen
```

- El id del pedido (`ficc_c-e01`) **no es** el de la llamada del modelo (`c-e01`): `FunctionInvokingChatClient` le pone prefijo. Las aprobaciones se cruzan con las ejecuciones por el `CallId`.
- Cada entrada son dos llamadas al modelo: la que pide la herramienta y la que, después de la aprobación, recibe el `Result` y contesta.
- Contra el modelo, mirá si corrige el CP por su cuenta: las instrucciones le piden que pase los datos «tal como los dio la persona». Si los corrigiera, el pedido mostraría el dato corregido y la persona lo vería antes de aprobar; el oráculo de e01 daría «falla» porque el caso de uso no llegó a rechazar nada.

## Entradas, oráculo y registro

| Archivo | Qué es |
|---|---|
| `entradas.v1.json` | Demostración: 2 pedidos, «escrito para el ejemplo», con Goya, Corrientes, 3450 y 90000 de `LocalidadesTests.cs` l. 50 @ `8fb4c3a`; el CP «34» es la negativa (X-006). Sin tasa. |
| `../../tools/oraculos/alta-con-aprobacion.jq` | Oráculo. `rechazo`: se ejecutó `agregar_localidad`, ninguna con éxito y alguna con error de `codigoPostal`. `alta`: exactamente una alta con éxito, con Goya, Corrientes, 3450 y 90000. Recalcula desde el registro lo que decide `Corrida.Veredicto`, sin compartir código. |

El SHA-256 de las entradas está congelado en `ejemplo.json`. El esperado de cada entrada viaja en el `motivo` de su renglón `entrada` (`esperado=rechazo`), para que el oráculo se recalcule leyendo sólo el registro.

## Corrida contra el modelo

La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

- **Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-09, estación 1, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): 2/2 entradas con afirma-sin-herramienta y con el oráculo del ejemplo, sin tasa; sonda de aprobación (R-D9): pasa, una entrada.

Registro y logs: `R-LLM/09-Caso-De-Uso-Como-Herramienta.{fake,modelo}.jsonl` y `Arnes/09-Caso-De-Uso-Como-Herramienta.*` (registro de trabajo de la edición, que no se publica; procedencia en el Anexo H de la guía). `tools/recalcular-oraculos.sh` recalcula los dos veredictos desde el registro.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 9; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/09-Caso-De-Uso-Como-Herramienta/Application/Herramientas/HerramientasDeLocalidades.cs`
```csharp
public IList<AITool> Crear() =>
[
    new ApprovalRequiredAIFunction(AIFunctionFactory.Create(AgregarAsync, Agregar,
        "Da de alta una localidad en el ABM. Pasá los datos tal como los dio la persona, sin corregirlos.")),
    new ApprovalRequiredAIFunction(AIFunctionFactory.Create(EliminarAsync, Eliminar,
        "Da de baja una localidad del ABM, buscándola por nombre y provincia."))
];

// …
    var model = new LocalidadModel { Nombre = nombre, Provincia = provincia, CodigoPostal = codigoPostal, Habitantes = habitantes };
    var resultado = ResultadoDeHerramienta.De(await localidades.SaveAsync(model, cancellationToken));
```

`Examples/09-Caso-De-Uso-Como-Herramienta/Application/Aprobacion/ServidorDeAprobaciones.cs`
```csharp
public ToolApprovalResponseContent Responder(string id, bool aprobar, FunctionCallContent? recibida = null)
{
    if (!_pendientes.Remove(id, out var pendiente))
    {
        throw new InvalidOperationException($"No hay un pedido de aprobación pendiente con id {id}.");
    }
    if (recibida is not null && (recibida.Name != pendiente.Herramienta || Canonico(Copiar(recibida.Arguments)) != Canonico(pendiente.Argumentos)))
    {
        return pendiente.Pedido.CreateResponse(false, "Los datos no coinciden con el pedido que se mostró.");
    }
    if (aprobar) Aprobados.Add(pendiente.CallId);
    return pendiente.Pedido.CreateResponse(aprobar, aprobar ? null : "La persona no lo aprobó.");
}
```

Qué mirar: `Responder` arma la respuesta desde `pendiente.Pedido`, el pedido guardado, y la rechaza si los argumentos recibidos no son los mostrados.

Con `--fake`, la respuesta del modelo es del guion; la aprobación, la ejecución y el rechazo de `SaveAsync` son reales.
