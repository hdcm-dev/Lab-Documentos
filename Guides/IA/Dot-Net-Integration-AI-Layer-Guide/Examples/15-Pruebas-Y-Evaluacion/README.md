# 15 · Pruebas y evaluación

**Dificultad:** avanzado · **Sección de la guía:** §11 (probar y evaluar una capa de IA) · **Objetivos:** OA-D9, escribir pruebas de cableado con un `IChatClient` falso, separadas de las corridas contra el modelo, y registrar una corrida R-LLM; OA-O3, leer el puntaje de un LLM juez: decir si está calibrado y qué sesgo declara.

## Qué enseña

Las tres capas de prueba de §10, sobre lo que produjo el ejemplo 07 (RAG con citas):

1. **Código propio, sin modelo** (`Pruebas/CableadoTests.cs`, categoría por defecto). El arnés las corre en cada build, con el servidor de modelos apuntado a un puerto cerrado. El juez es un `GuionChatClient` (cliente guionado), así que se sabe de antemano qué tiene que salir.
2. **Evaluadores deterministas primero** (`Application/EvaluadoresDeterministas.cs`). Son tres `IEvaluator` de `Microsoft.Extensions.AI.Evaluation`, con la misma interfaz que los del juez, pero sin modelo:
   - **cita válida** y **afirmación central**: el mismo código que decidió en 07. El `.csproj` enlaza sus fuentes y no las copia;
   - **valor fuera de catálogo**: el contrato de la salida estructurada de 04 (§10.3). Una clave inventada (`"avion"`) falla, una lista vacía falla y un texto que no es JSON es un error de diagnóstico, nunca un «pasa».
3. **El juez LLM, después** (`Application/CorridaDeEvaluacion.cs`). `RelevanceEvaluator` y `GroundednessEvaluator` de `Microsoft.Extensions.AI.Evaluation.Quality`, corridos por el reporte de `.Reporting` (`DiskBasedReportingConfiguration`) con la **caché de respuestas** activada. En la corrida del 2026-10-29 (no medida: 41/52 salidas interpretables, mínimo 45/52; §10.6) el juez y el generador son el mismo modelo, `qwen3.5:4b`: el caso de mayor riesgo de autofavorecimiento. El control que este laboratorio no aplicó es un juez de otra familia (§11.6). El juez recibe el cliente de la capa única (`FabricaDeClientes`): razonamiento apagado, `num_ctx` 8192 y `temperature` 0. Una prueba con un cliente espía, puesto debajo de la capa, lo comprueba en las dos llamadas.

**Equivalencia con el oro, aparte, dentro de la tercera capa** (`Application/EquivalenciaConElOro.cs`, recuperación 2026-10-28). `EquivalenceEvaluator` compara cada salida de 07 cuya pregunta es respondible (12 de las 16) con la **referencia del oro**: el texto de los fragmentos que el oro marca como relevantes, el mismo texto que vio 07. Es **descriptiva**: no entra en el oráculo `acuerdo-del-juez`, no tiene línea de entrada en el registro (sus llamadas van con `entrada_id` `eq-s07-rNN`) y nunca es compuerta. El juez es el mismo y pasa por la misma capa única; una prueba con el cliente espía lo comprueba (`num_ctx` ≥ 8192). Cuidado con el formato: Equivalence **no** pide las etiquetas `<S0>`/`<S1>`/`<S2>`. Su plantilla termina en «stars:» y espera el entero solo, así que un juez que contesta con `<S2>5</S2>` queda sin valor. Una prueba compara la lectura propia (`LecturaEntera`) con la de M.E.AI en cinco formas de salida.

**Cuándo una métrica no es interpretable** (recuperación 2026-10-28). Para M.E.AI, una métrica no se interpreta si no tiene valor, si no tiene `Interpretation`, si su `Interpretation` es `Inconclusive` o `Unknown`, o si trae un diagnóstico de severidad Error (`InterpretacionDeMeai`). Si el oráculo leyó un puntaje y M.E.AI no lo interpreta, falla un invariante y `--verificar` sale con rc 1. Una prueba sin modelo corre las 26 entradas con un juez que nunca escribe las etiquetas: ningún veredicto «pasa» y el juez queda descartado por la regla de calibración. Una prueba que pasa en verde con todas las métricas Inconclusive no mide nada.

**Qué se evalúa** (entradas congeladas por SHA-256 en `ejemplo.json`):

| Archivo | Qué es |
|---|---|
| `entradas/salidas-07.v2.json` | Las 16 salidas de 07 de la última corrida válida, copiadas de su registro R-LLM con `--congelar-salidas-07`: pregunta, contexto, recuperados, respuesta y el veredicto del oráculo de 07. Las preguntas son estímulo escrito para el ejemplo (el oro de 06). |
| `entradas/calibracion.v2.json` | 10 casos de calibración, «escrito para el ejemplo», armados **antes de juzgar** desde salidas propias de 07: 5 correctas tal cual (c01–c05) y 5 perturbaciones controladas. Son dos «cita y contradice» (c06 y c07: cambia el valor y se mantiene la cita), una «sin cita» (c08) y dos «cita a un fragmento no recuperado» (c09 y c10, con una afirmación que no está en el contexto). **Anotado por el agente constructor, no por una persona**: §10.6 no lo llama calibración humana. |

**Cómo se lee el juez** (definido antes de juzgar; oráculo `acuerdo-del-juez`, igual en `Application/OraculoDelJuez.cs` y en `tools/oraculos/acuerdo-del-juez.jq`):

- El puntaje es el entero entre `<S2>` y `</S2>`, el formato que piden los dos evaluadores. Si no está, la salida «no es interpretable», y eso es una falla del juez, no una exclusión. La lectura del oráculo tiene que coincidir con la de M.E.AI en cada llamada; si difieren, falla un invariante.
- «Alta» quiere decir un puntaje ≥ 4.
- En la **calibración**, el juez acierta un caso si Relevance y Groundedness coinciden con la anotación: «relevante» con Relevance alta y «fundamentada» con Groundedness alta. En c08 (sin cita) la anotación dice «fundamentada», porque lo que afirma está en el contexto. La falta de cita la detecta el evaluador determinista, no el juez: no la mide.
- En las **16 salidas de 07**, el juez llama aceptable a una respuesta si tiene las dos métricas altas. Lo que se informa es el acuerdo de ese veredicto con el oráculo determinista de 07.
- **Regla de calibración**: si el juez acierta menos de 8 de 10, o si sus salidas no son interpretables, los puntajes se informan como «juez no calibrado: no se interpretan» y la conclusión se apoya sólo en los deterministas. Con 10 casos, la calibración sólo puede **descartar** un juez. Nunca lo valida: 8/10 da IC95 [0,444; 0,975] (Clopper-Pearson). Los puntajes del juez **nunca son compuerta**.

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 15-Pruebas-Y-Evaluacion                 # --fake --verificar, sin red
tools/verificar.sh correr 15-Pruebas-Y-Evaluacion --con-modelo    # fase 3b: turno modelo-propio
tools/verificar.sh tasa 8 10                                      # intervalo de una tasa
```

Las pruebas, con el SDK en contenedor (el host sólo necesita docker), desde la carpeta de la guía:

```bash
docker run --rm -u "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD/Examples":/src -w /src/15-Pruebas-Y-Evaluacion \
  mcr.microsoft.com/dotnet/sdk:10.0 dotnet test --filter "Category!=LLM"   # cableado, sin servidor
```

y lo mismo con `--filter "Category=LLM"`, que se salta con `Assert.Ignore` si el segundo servidor no responde o no tiene el modelo. La consola acepta además estas opciones:

- `--reporte <carpeta>` reusa un reporte anterior. Lo que está en su caché no vuelve a llamar al juez, y esas entradas quedan excluidas con el motivo «caché del reporte».
- `--congelar-salidas-07 <registro.jsonl>` vuelve a armar `entradas/salidas-07.v2.json`: guarda sólo el nombre del registro, sin ruta, y enmascara el recurso del `turno_id`. Cambiar las salidas crea una versión nueva y descarta las cifras de la anterior.

**`entradas/salidas-07.v2.json` reemplaza a la `v1`.** Es el estímulo del 2026-10-27, congelado desde una corrida de 07 en un servidor de laboratorio retirado; su SHA-256 está fijado en `ejemplo.json`. La `v2` tiene las mismas 16 salidas, byte a byte, que la `v1`: cambian sólo tres campos de `procedencia`. `version` pasa a `v2`; `registro` nombra el archivo sin su ruta, porque el registro de trabajo no se publica (ver el Anexo H de la guía); y `turno_id` enmascara el nombre del recurso de turnos («`<recurso>`»), que nombraba un servicio del laboratorio. Como el estímulo no cambió, las cifras medidas con la `v1` siguen valiendo para la `v2`. La `v1` se retiró del ejemplo. Por la misma razón `entradas/calibracion.v2.json` reemplaza a su `v1`: los diez casos, el umbral y las anotaciones no cambian; cambian sólo `version` y la nota, que ahora cita la `v2` de las salidas. Su SHA-256 nuevo está fijado en `ejemplo.json`.

Variables: `OLLAMA_PROPIO_ENDPOINT` y `OLLAMA_PROPIO_MODEL` (default `qwen2.5:0.5b`) para el juez, y `RLLM_DIR` para el registro y el reporte. En el laboratorio, el endpoint lo exporta el arnés dentro del turno y no queda escrito en ningún archivo.

## Salida esperada en modo falso

**Esta salida es de cableado, no una medición.** El juez es un cliente guionado: las cifras de abajo (52 llamadas, 51/52 interpretables, 15/16, 9/10, 12/12) las fija el guion de antemano y sólo dicen que el código cuenta bien. No dicen nada de ningún modelo.

El guion trae a propósito un desacuerdo con el oráculo (s07-r07: Groundedness 2 sobre una salida que en 07 pasó) y una salida sin etiquetas (c10, Groundedness: «Score: 2»). La consola tiene que mostrar:

```
deterministas sobre las 16 de 07: pasan 12/16 (coinciden con 07: 16/16)
deterministas sobre la calibración: detectan 5/5 perturbaciones y aceptan 5/5 correctas
llamadas al juez: 52 · salidas interpretables: 51/52
acuerdo juez-oráculo determinista (16 salidas de 07): 15/16 (excluidas 0)
acuerdo juez-anotador (10 de calibración, mismo origen que las 16): 9/10 (excluidas 0)
juez: no descartado (9/10 ≥ 8); con 10 casos no queda validado
equivalencia con la referencia del oro (salidas de 07 respondibles; descriptiva, no es compuerta): interpretables 12/12, altas 12 (excluidas 0)
registro: 90 renglones en 15-Pruebas-Y-Evaluacion.jsonl
invariantes de código: se cumplen
```

Qué conviene mirar:

- **c08** («sin cita») tiene puntajes altos del juez y falla en el determinista. Es el argumento del orden: el juez no reemplaza a la cuenta.
- **c10**: la lectura del oráculo y la de M.E.AI coinciden en «sin valor». El diagnóstico de M.E.AI («Failed to parse score…») queda en el campo `paso` del registro.
- En modo falso, las cifras dicen que el cableado cuenta bien. No dicen nada del juez.
- **Equivalence** guionada: 5 si la salida pasó en 07 y 2 si no, como entero solo. Las 12 quedan interpretables; el `12` de «altas» lo fija el guion.

## Lo que se vio contra el juez

**Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-15, estación 2, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c): no medido (cortada por una falla del entorno); la repetición con criterio propio (R-15b) no había cerrado al anotar este estado.

Para probar con otro juez basta cambiar `OLLAMA_PROPIO_MODEL`; un juez de otra familia es el control que este laboratorio no aplicó (§11.6). La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

## Registro y oráculo

El registro R-LLM queda en `RLLM_DIR/15-Pruebas-Y-Evaluacion.jsonl`. Tiene una línea por llamada al juez (`entrada_id` `s07-rNN` o `cNN`; `llamada_n` 1 = Relevance, 2 = Groundedness) y una por entrada, con el veredicto de `acuerdo-del-juez`. El campo `paso` lleva el evaluador, el grupo, lo esperado, el umbral, el tipo de perturbación, el anotador, la lectura de M.E.AI y su diagnóstico; `contexto` y `recuperados` son los de 07. `tools/recalcular-oraculos.sh` recalcula el oráculo leyendo sólo el registro. El reporte de M.E.AI, con su caché, queda en `RLLM_DIR/evaluacion-15/<ejecución>/`, fuera del repo.

Registros y logs: `R-LLM/15-Pruebas-Y-Evaluacion.{fake,modelo}.jsonl` y `15-Pruebas-Y-Evaluacion/` (registro de trabajo de la edición, que no se publica; procedencia en el Anexo H de la guía).

Las llamadas de Equivalence (`eq-s07-rNN`, `llamada_n` 1) llevan en `paso` el evaluador, la referencia del oro por id de fragmento (`referencia=oro:…`), el umbral, la lectura de M.E.AI y, si no se interpreta, el motivo y el diagnóstico; `contexto` es la referencia. `tools/recalcular-oraculos.sh` no las recalcula: no tienen línea de entrada ni oráculo.

Ningún evaluador usado está marcado `AIEVAL001`: el ejemplo no lleva ninguna supresión.

## Paquetes

`Microsoft.Extensions.AI`, `OllamaSharp`, `Microsoft.Extensions.AI.Evaluation`, `.Quality` y `.Reporting`, y NUnit. También `Microsoft.Extensions.VectorData.Abstractions` y `CommunityToolkit.VectorData.InMemory`, porque `IndiceDeAyuda.cs` de 07 (enlazado) define `Recuperacion` junto al almacén. Las versiones están en `Examples/Directory.Packages.props` (la guía las cita desde el Anexo F).

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 11; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado), sin modelo.

`Examples/15-Pruebas-Y-Evaluacion/Pruebas/ConModeloTests.cs`
```csharp
[Category("LLM")]
public class ConModeloTests
{
    private ConfiguracionDelModelo _config = null!;

    [OneTimeSetUp]
    public async Task Predicado()
    {
        _config = ConfiguracionDelModelo.DesdeEntorno().Validar();
        var r = await PredicadoDeSalto.EvaluarAsync(_config.PropioEndpoint, _config.PropioModelo, ["completion"]);
        if (r.Saltar) Assert.Ignore(r.Motivo);
    }
    // …
}
```

Qué mirar: sin servidor o sin el modelo, el predicado de salto ignora la clase entera con su motivo; nunca falla ni pasa.

`Examples/15-Pruebas-Y-Evaluacion/Application/EvaluadoresDeterministas.cs`
```csharp
public ValueTask<EvaluationResult> EvaluateAsync(
    IEnumerable<ChatMessage> messages, ChatResponse modelResponse, ChatConfiguration? chatConfiguration = null,
    IEnumerable<EvaluationContext>? additionalContext = null, CancellationToken cancellationToken = default)
{
    var ctx = ContextoDeRecuperacion.De(additionalContext);
    var v = ValidadorDeCitas.Validar(modelResponse.Text ?? string.Empty, ctx.Recuperados.Select(r => r.Fuente).ToHashSet(StringComparer.Ordinal));
    // En una no respondible, el «No sé» pedido (sin citas) es la respuesta válida.
    var ok = ctx.Pregunta.Respondible ? v.Veredicto == Veredicto.Aceptada : v.Veredicto == Veredicto.NoSe;
    var metrica = new BooleanMetric(Metrica, ok, v.Veredicto.ToString());
    // …
}
```

Qué mirar: el `ChatConfiguration` no se usa. El LLM juez entra donde no hay cuenta: en las preguntas de 07 cuyo literal no fija un valor, el oráculo sólo comprueba la cita.

`Examples/15-Pruebas-Y-Evaluacion/Application/CorridaDeEvaluacion.cs`
```csharp
foreach (var caso in casos)
{
    // 1. Deterministas: siempre, sin modelo y antes que el juez.
    var (determinista, citaValida, _) = await Deterministas.EvaluarAsync(caso, cancellationToken);
    var esperadoDet = caso.Grupo == GrupoDeEntrada.Calibracion ? caso.Anotacion!.Determinista : caso.Resultado07!;
    if (determinista != esperadoDet)
        fallan.Add($"{caso.EntradaId}: el determinista dio «{determinista}» y se esperaba «{esperadoDet}»");

    if (plazo.Vencido)
    {
        Cerrar(caso, determinista, citaValida, [], new("excluida", "plazo", null));
        continue;
    }
    // …
}
```

Qué mirar: un determinista que no dice lo esperado rompe un invariante; un puntaje del LLM juez nunca.

**Probar otro juez.** Exportá `OLLAMA_PROPIO_MODEL` y corré `tools/verificar.sh correr 15-Pruebas-Y-Evaluacion --con-modelo`: el arnés pasa la variable al contenedor, pero sólo corre contra un segundo servidor (`OLLAMA_PROPIO_*`; el nombre de la variable es histórico) con la versión y los digests del Anexo F del libro (E.3); con otro servidor, usá la vía de E.5. La corrida queda con tu rótulo y no se compara con la de la guía.
