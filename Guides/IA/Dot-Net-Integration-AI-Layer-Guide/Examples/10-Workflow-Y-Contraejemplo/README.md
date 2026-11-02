# 10 · Workflow y contraejemplo

**Dificultad:** avanzado · **Sección de la guía:** §9.6 (workflows: executors, aristas, checkpoints, `RequestPort`; el contraejemplo) · **Objetivo:** OA-D6 (elegir entre función, llamada con herramientas, workflow y agente, y justificarlo con el criterio de salto).

**Sin modelo.** No llama a ningún `IChatClient`: corre entero en la fase `build`, sin servidor.

## Qué enseña

La encuesta de tres pasos del caso base (datos de la persona → medios y frecuencia → distancia, minutos y motivo) se escribe **dos veces**, con las mismas entradas y el mismo `EncuestaService.ValidarPaso` de `_Comun`:

| Versión | Archivo | Líneas de código* |
|---|---|---|
| Función: un `for` por paso y un `while` hasta que el paso valida | `Application/Encuestas/EncuestaComoFuncion.cs` | 24 |
| Workflow de MAF: `EncuestaExecutor` (`partial`, dos `[MessageHandler]`), un `RequestPort` «Persona» para la pausa humana, un executor de cierre hecho con `BindAsExecutor`, el estado en el contexto para que viaje en el checkpoint | `Application/Workflows/EncuestaComoWorkflow.cs` + `ConductorDelWorkflow.cs` (lo que en la Web haría la pantalla) | 78 + 52 |

\* Sin comentarios, líneas en blanco, `using` ni `namespace`; contadas con `grep -vE` sobre los archivos de esta carpeta.

Las dos dan **lo mismo** en las cuatro entradas, y el workflow **funciona**: emite un checkpoint por superstep, se corta en la pausa del paso 3 y se rehidrata en **otra instancia** (grafo, executors y `EncuestaService` nuevos) desde un almacén JSON en disco (`FileSystemJsonCheckpointStore`), sin volver a preguntarle nada a la persona. Ése es el punto: el workflow no pierde por roto, sino porque **no compra nada** que este caso necesite. Los pasos se conocen de antemano, el camino lo decide el código (no hay modelo que elija) y no hay una espera de días que sobrevivir a un reinicio. «Si podés escribir una función, hacelo» (N4 P3; N6 P7, C8). La instancia positiva del escalón 2, con un modelo dentro del camino, es 12 (variante «un-agente»).

Cuándo sí conviene el workflow (para la guía, §8.6): pasos fijos **y** alguna de estas cosas: una pausa humana que tiene que sobrevivir a un reinicio (checkpoint + `RequestPort`), varios agentes o funciones que coordinar, o pasos que corren en paralelo (fan-out/fan-in).

Tres cosas de MAF 1.26.0 que hay que saber (las tres reproducidas al construir este ejemplo; ver «Evidencia»):

1. Un executor con `[MessageHandler]` **exige** el paquete `Microsoft.Agents.AI.Workflows.Generators`. Sin él: `error CS0534: 'EncuestaExecutor' does not implement inherited abstract member 'Executor.ConfigureProtocol(ProtocolBuilder)'`. Por eso el `.csproj` lo referencia y lo comenta.
2. Una función se vuelve executor con **`BindAsExecutor`**. El `BindExecutor` de la documentación no compila: `error CS0411: The type arguments for method 'ExecutorBindingExtensions.BindExecutor<TExecutor>(Func<string, string, ValueTask<TExecutor>>, string)' cannot be inferred from the usage`.
3. Un handler que envía mensajes declara qué envía (`[MessageHandler(Send = [typeof(PedidoDePaso), typeof(EncuestaTerminada)])]`); si no, falla en ejecución (N4 V01).

Lo mismo vale para lo que un handler produce con `YieldOutputAsync`: se declara con `Yield = [typeof(...)]`. Sin declararlo, compila y en ejecución el executor falla con `InvalidOperationException: Cannot output object of type String. Expecting one of [].` Lo reproduce una prueba sin modelo, `Pruebas/TrampasDeExecutorsTests.cs` (REC-30), con el caso rojo y el mismo handler declarado.

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 10-Workflow-Y-Contraejemplo   # compila con -warnaserror y corre --fake --verificar
tools/verificar.sh build                                # todo Examples/, con estas pruebas incluidas
```

A mano, en contenedor:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/10-Workflow-Y-Contraejemplo \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test && dotnet run -- --verificar'
```

El proyecto es a la vez consola y pruebas (`GenerateProgramFile=false`). Argumentos: `--verificar` (sale con 1 si falla algún invariante), `--plazo-s N` y `--fake`, que se acepta por contrato y no cambia nada: no hay modelo que reemplazar.

## Qué observar

```text
e02: un medio fuera del catálogo en el paso 2; la persona corrige
  workflow: pedido paso 1 (intento 1): contesta
  workflow: pedido paso 2 (intento 1): contesta
  workflow: pedido paso 2 (intento 2, errores: medios): contesta
  workflow: pedido paso 3 (intento 1): contesta
  antes de la pausa: pedido paso 1 (intento 1): contesta
  antes de la pausa: pedido paso 2 (intento 1): contesta
  antes de la pausa: pedido paso 2 (intento 2, errores: medios): contesta
  antes de la pausa: pedido paso 3: se deja sin contestar (pausa)
  antes de la pausa: superstep 11: checkpoint con el pedido pendiente; se corta
  rehidratado: pedido paso 3 (intento 1): contesta
  función: completa[1,2,1] · workflow: completa[1,2,1] · rehidratado: completa[1,2,1]
  supersteps: 14 · checkpoints: 14 · pedidos tras rehidratar: 1
  oráculo workflow-igual-funcion (esperado completa[1,2,1]): pasa
...
registro: 4 renglones en 10-Workflow-Y-Contraejemplo.jsonl
invariantes de código: se cumplen
```

- **Tres pedidos de la persona son 14 supersteps** (11 en e01): el `RequestPort` es un nodo más del grafo, y cada ida y vuelta por él cuesta pasos del modelo BSP. La función no tiene supersteps.
- **Tras rehidratar, el pedido pendiente vuelve a emitirse** (`pedidos tras rehidratar: 1`) y la persona contesta el paso 3 sin repetir el 1 ni el 2: lo aceptado viajó en el estado del checkpoint, no en campos del executor.
- **`completa[1,2,1]`** son los intentos por paso. El medio «avion» lo rechaza `ValidarPaso` (el endurecimiento T2 de `_Comun`) en las dos versiones: la regla es del caso de uso, no de la orquestación.
- **e04**: la persona deja la encuesta en el paso 3. Las dos versiones dan `incompleta@3[1,1,0]`. En el workflow eso se modela como una contestación vacía por el `RequestPort`; en la función, como un `null`.

Invariantes de código que comprueba `--verificar`, por entrada: la función da el esperado; el workflow y el rehidratado dan lo mismo que la función (estado y datos); hay un checkpoint por superstep (y al menos uno); hubo pausa con checkpoint; tras rehidratar se volvió a emitir el pedido pendiente.

## Entradas, oráculo y registro

| Archivo | Qué es |
|---|---|
| `entradas.v1.json` | 4 entradas, «escrito para el ejemplo»: cada una es la secuencia de respuestas que da la persona, paso por paso. Localidades Goya y Bella Vista (`LocalidadesTests.cs` l. 50 y l. 102 @ `8fb4c3a`), personas «Persona 01…04», valores de `Catalogos`. Las respuestas con `"negativo": true` llevan a propósito «avion», «laburo» o una edad de 12 años, que `ValidarPaso` rechaza. SHA-256 congelado en `ejemplo.json`. Estímulo del mismo autor; sin tasa (no hay modelo). |
| `../../tools/oraculos/workflow-igual-funcion.jq` | Oráculo. Recalcula, leyendo sólo el registro, lo mismo que `Corrida.Invariantes`, sin compartir código con él. |

Sin modelo no hay renglones `llamada`: el registro tiene un renglón `entrada` por entrada, y lo que se compara viaja entero en su `motivo`, como JSON. Una entrada que el plazo deja afuera queda `excluida` con motivo `plazo`.

Pruebas (`Pruebas/WorkflowYFuncionTests.cs`, todas sin servidor): las cuatro entradas por las tres vías; un checkpoint por superstep; el rechazo del medio fuera de catálogo; la encuesta sin respuestas; el cableado del estímulo (todo valor de catálogo de una respuesta no negativa está en `Catalogos`, y cada negativa declarada la rechaza `ValidarPaso`), y que `Invariantes` detecta un rehidratado distinto y checkpoints de menos. En `Pruebas/TrampasDeExecutorsTests.cs`, dos más: el `YieldOutputAsync` sin declarar falla en ejecución y, declarado, produce la salida.

## Evidencia

Compilación, pruebas, corrida, recálculo del oráculo y las dos variantes rotas (sin el generador; con `BindExecutor`) en `10-Workflow-Y-Contraejemplo/`; el registro, en `10-Workflow-Y-Contraejemplo.jsonl` de la carpeta R-LLM (registro de trabajo de la edición, que no se publica; procedencia en el Anexo H de la guía).

Re-verificación del 2026-10-27 (archivos `*.redespacho.*` en las mismas carpetas): la corrida por el arnés, las 9 pruebas, las dos variantes rotas con su código de salida (1 en las dos), el recálculo del oráculo (4 de 4) y una prueba negativa del oráculo: con un checkpoint de menos en el `motivo` de e02, el recálculo da «falla» y sale con 1. En `pruebas-y-variantes.redespacho.log`, el primer intento de las variantes falló por una ruta sin permiso en el contenedor (no por el ejemplo); la corrida válida es `variantes-rotas.redespacho.log`.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 9; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/10-Workflow-Y-Contraejemplo/Application/Encuestas/EncuestaComoFuncion.cs`
```csharp
for (var paso = 1; paso <= EncuestaRules.TotalDePasos; paso++)
{
    while (true)
    {
        var respuesta = persona.Contestar(paso);
        if (respuesta is null)
            return new ResultadoDeEncuesta(false, paso, intentos, rechazos, ResultadoDeEncuesta.DatosDe(modelo));

        intentos[paso - 1]++;
        respuesta.AplicarA(modelo);
        var errores = servicio.ValidarPaso(paso, modelo);
        if (errores.Count == 0) break;
        rechazos.AddRange(errores.Keys.Order(StringComparer.Ordinal).Select(c => $"{paso}.{c}"));
    }
}
```

`Examples/10-Workflow-Y-Contraejemplo/Application/Workflows/EncuestaComoWorkflow.cs`
```csharp
var encuesta = new EncuestaExecutor(servicio);
var persona = RequestPort.Create<PedidoDePaso, Contestacion>(PersonaId);
// …
var cierre = cerrar.BindAsExecutor(CierreId);

return new WorkflowBuilder(encuesta)
    .AddEdge(encuesta, persona)
    .AddEdge(persona, encuesta)
    .AddEdge(encuesta, cierre)
    .WithOutputFrom(cierre)
    .WithName("encuesta-de-tres-pasos")
    .Build();
```

Qué mirar: las dos versiones llaman al mismo `ValidarPaso`; lo único que agrega el workflow es el `RequestPort` como nodo y la arista de vuelta.

Salida sin modelo (no hay `IChatClient` que reemplazar), de la entrada con un medio fuera del catálogo:

```text
e02: un medio fuera del catálogo en el paso 2; la persona corrige
  […]
  antes de la pausa: pedido paso 3: se deja sin contestar (pausa)
  antes de la pausa: superstep 11: checkpoint con el pedido pendiente; se corta
  rehidratado: pedido paso 3 (intento 1): contesta
  función: completa[1,2,1] · workflow: completa[1,2,1] · rehidratado: completa[1,2,1]
  supersteps: 14 · checkpoints: 14 · pedidos tras rehidratar: 1
  oráculo workflow-igual-funcion (esperado completa[1,2,1]): pasa
```

`completa[1,2,1]` son los intentos por paso: el medio rechazado en el paso 2 lo rechaza `ValidarPaso` en las dos versiones, porque la regla es del caso de uso y no de la orquestación.
