# 12 · ¿Uno o varios agentes?

**Sección de la guía**: §9.6 (variante «un-agente», la instancia positiva del escalón 2) y §9.8 (variante «dos-agentes», la dimensión cruzada «¿uno o varios?», después de §9.7) · **Objetivo de aprendizaje**: OA-D7 (decidir si hacen falta varios agentes o alcanza con uno bien diseñado, mirando lo que cuesta cada uno) · **Nivel**: avanzado · **Tipo**: demostración (TURNO-2), sin tasa.

## Qué enseña

El escalón queda **fijo en 2**: un workflow de MAF cuyo camino lo escribe el código. La única variable es **cuántos agentes** hay en ese camino:

```
«un-agente»:   Datos → Redactor → Verificador ─(no coincide, una vez)→ Redactor  → Verificador → salida
«dos-agentes»: Datos → Redactor → Verificador ─(no coincide, una vez)→ Corrector → Verificador → salida
```

| Nodo | Qué es | ¿Usa modelo? |
|---|---|---|
| `Datos` | executor que llama a `resumir_encuesta` (`ResultadosDeEncuestaService` de `_Comun`) y arma el contexto | no |
| `Redactor` | executor que llama al `IChatClient`. **No es un `AIAgent`** | sí |
| `Verificador` | executor que compara los porcentajes del texto con los del servicio. Es una cuenta (escalón −1) | **no, en ninguna variante** |
| `Corrector` | sólo en «dos-agentes»: un segundo agente hecho con `AsAIAgent` sobre el mismo cliente, que recibe el error y reescribe | sí |

1. **El camino lo fija el código** (`Application/Workflows/CaminoDelResumen.cs`). Ningún modelo elige el paso siguiente. Si lo eligiera, sería el escalón 3 (handoff, Magentic), que la guía explica sin ejemplo (§8.7; plan §E.4 d). Hay **una sola vuelta** de corrección (`MaximoDeVueltas = 1`). Si el texto sigue mal después de esa vuelta, el workflow termina y la salida lo dice (`Correcto = false`).
2. **El verificador no confía en el texto que viaja por el workflow**. Vuelve a pedirle los resultados al caso de uso y aplica `ComparadorDePorcentajes`. Esa función también es la del oráculo. El error que vuelve al modelo trae el valor del servicio («colectivo: dice 25 %, el servicio da 20 %»).
3. **El mismo mensaje de corrección** (`Indicaciones.Correccion`) llega al redactor en «un-agente» y al corrector en «dos-agentes». Cambia quién lo recibe y con qué instrucciones. El texto es el mismo.
4. **Sin revisor de privacidad**. `resumir_encuesta` devuelve `ResultadosDeEncuesta`, que no tiene `Nombre` ni `Edad` en ningún nivel. Es un invariante del **tipo** y se prueba por reflexión (`Garantias.ResumirEncuestaSinDatosDePersona`, H-005). Revisar lo que no puede estar no agrega nada.
5. **Una sola capa de opciones**. El redactor y el agente de MAF reciben el cliente de `FabricaDeClientes` (razonamiento apagado, `num_ctx`, temperatura y semilla). El agente no construye su propio cliente. Un `GrabadorDeLlamadas` encima de la capa deja una línea R-LLM por llamada, con el nodo que la hizo.

## Cómo se corre

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 12-Uno-O-Varios-Agentes                 # --fake --verificar, sin servidor
tools/verificar.sh correr 12-Uno-O-Varios-Agentes --con-modelo    # contra qwen3.5:4b (turno modelo-compartido)
tools/recalcular-oraculos.sh "$RLLM_DIR"/12-Uno-O-Varios-Agentes.jsonl
```

A mano, en el contenedor (`bin/` y `obj/` quedan en la carpeta y el `.gitignore` los excluye):

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/12-Uno-O-Varios-Agentes \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

Opciones de la consola: `--fake` usa el cliente guionado; `--verificar` sale con 1 si algún invariante de código no se cumple; `--plazo-s N` excluye con motivo «plazo» lo que no alcanza a correr. Contra el modelo, la configuración sale del entorno (`OLLAMA_ENDPOINT`, `OLLAMA_MODEL`, `OLLAMA_SERVIDOR`, `OLLAMA_NUM_CTX`). Con el servidor de chat (`OLLAMA_SERVIDOR=compartido`), el `num_ctx` lo pone el arnés desde `/api/ps`. Sin él, la corrida se rechaza.

## Qué observar

Salida con `--fake` (resumida). En e01 el guion escribe colectivo 25 % y el servicio da 20 %: hay una vuelta. En e02 el primer texto ya coincide:

```
siembra rotulada «escrito para el ejemplo»: 20 respuestas; colectivo 20 %, auto 20 %, bicicleta 35 %, moto 15 %, caminata 15 %, tren 30 % (estímulo del mismo autor)
e01.un-agente: …
  camino: Datos → Redactor → Verificador → Redactor → Verificador
  redactor (guion): El 25 % de las respuestas usa colectivo y el 35 % usa bicicleta.
  redactor (guion): El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta.
  oráculo porcentajes-del-servicio: pasa
e01.dos-agentes: …
  camino: Datos → Redactor → Verificador → Corrector → Verificador
  …
costo por variante (los mismos pedidos; sin tasa: con 2 entradas ilustra el costo, no responde «¿uno o varios?»):
  un-agente   llamadas al modelo 3 · tokens de entrada 808 · de salida 51 · vueltas 1 · oráculo 2 de 2
  dos-agentes llamadas al modelo 3 · tokens de entrada 802 · de salida 51 · vueltas 1 · oráculo 2 de 2
invariantes de código: se cumplen
```

Con el guion, los tokens son la estimación del adaptador (caracteres/4). Con el modelo, son los que informa el servidor.

- **Garantías del código** (se comprueban en toda corrida, con el modelo o sin él): el camino es el de la variante; hay a lo sumo una vuelta y una llamada al modelo por nodo y vuelta; el corrector existe sólo en «dos-agentes»; la salida es el último texto del modelo y su veredicto coincide con la cuenta; ningún «Persona NN» llega al modelo; el verificador no recibe ni un `IChatClient` ni un `AIAgent` (reflexión); toda llamada, también la del agente de MAF, sale con razonamiento `None` y `num_ctx` explícito.
- **Oráculo** (`porcentajes-del-servicio`, definido antes de correr): cada medio pedido aparece con su porcentaje y coincide, con ±1 punto, con `ResultadosDeEncuestaService`. Un resumen sin porcentajes es falla («sin cifra»). El porcentaje de un medio se busca en la cláusula de su mención: primero la corta (cortes en «.», «;», «y», «e» y «,»), si no la larga (sin la coma). Dentro de la cláusula vale el más cercano. Así, «El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta» asocia el 20 con colectivo aunque el 35 esté más cerca. `tools/oraculos/porcentajes-del-servicio.jq` repite estas reglas leyendo sólo el registro: el contexto de la primera llamada y la salida de la última.
- **Costo por variante**: llamadas al modelo, tokens de entrada y de salida informados por el servidor, y vueltas. Los tiempos no se comparan (§11.4). La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.
- **Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-12, estación 2, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): 2/2 entradas no-vacia y porcentajes-del-servicio en las dos variantes, sin tasa; 0 vueltas, el corrector no se llamó.
- **Lo que este ejemplo no responde**: con 2 entradas **ilustra el costo**. No dice si conviene uno o varios agentes (X-009). Las dos variantes tienen la misma cantidad de llamadas cuando hay una vuelta. Lo que el segundo agente agrega es un artefacto más que configurar, probar y observar, con sus propias instrucciones. El texto del pedido y el verificador no cambian.

## Pruebas

`Pruebas/UnoOVariosAgentesTests.cs`, sin servidor (27 casos):

- la siembra da los porcentajes que usa el guion;
- el comparador: orden «N % … medio» y «medio: N %», decimales con coma, «por ciento», invertidos, fuera de ±1, falta un medio, sin cifra, la etiqueta del catálogo («a pie», «auto particular»), y el límite declarado («colectivo y bicicleta: 20 % y 35 %» no se reconoce);
- el camino de cada variante con error y sin error;
- el corte después de una vuelta;
- el mensaje de corrección idéntico en las dos variantes;
- todas las llamadas, incluida la del `AIAgent`, por la capa única (un `ClienteEspia` debajo);
- privacidad por reflexión, con un control positivo sobre `RespuestaDeEncuesta`;
- el verificador sin modelo;
- la corrida `--fake` completa con su registro;
- el plazo vencido;
- el oráculo con truncado y sin cifra;
- los estímulos (rótulo, medios del catálogo y SHA-256 del manifiesto);
- el rechazo de `OLLAMA_SERVIDOR=compartido` sin `num_ctx` del arnés.

`ConModeloTests` (`[Category("LLM")]`) se salta con el motivo del predicado si el servidor no responde.

## Límites declarados

- **Demostración sin tasa**: 2 pedidos, los mismos en las dos variantes (diseño pareado). Toda cifra es un estímulo del mismo autor sobre la siembra rotulada.
- El comparador no reconoce un porcentaje compartido entre dos medios de una misma cláusula («colectivo y auto: 20 % cada uno»). Lo cuenta como falla.
- No hay checkpoints ni `RequestPort`: esos están en 10. Acá el workflow corre de punta a punta en una petición.
- La base es SQLite en un archivo temporal que se borra al terminar. El dominio, la persistencia y `SessionSeeder` con `SiembraDeEncuestas` vienen de `_Comun`.

## Datos

`entradas.v1.json` lo escribimos para el ejemplo y está congelado con su SHA-256 en `ejemplo.json`. Los medios pedidos son claves de `Catalogos.Medios`. Las 20 respuestas son `SiembraDeEncuestas` de `_Comun`: «Persona 01…20», con valores de `Catalogos` y las localidades sembradas del caso base. Al modelo le llegan sólo los agregados, nunca las filas.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 9; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/12-Uno-O-Varios-Agentes/Application/Workflows/CaminoDelResumen.cs`
```csharp
var builder = new WorkflowBuilder(datos)
    .AddEdge(datos, redactor)
    .AddEdge(redactor, verificador);

if (variante == Variante.UnAgente)
{
    builder = builder.AddEdge(verificador, redactor);
}
else
{
    var agente = (clienteDelCorrector ?? throw new ArgumentNullException(nameof(clienteDelCorrector)))
        .AsAIAgent(instructions: Indicaciones.DelCorrector, name: "corrector",
                   description: "Reescribe un resumen con los errores que encontró el verificador.");
    var corrector = new CorrectorExecutor(agente);
    builder = builder.AddEdge(verificador, corrector).AddEdge(corrector, verificador);
}

return builder.WithOutputFrom(verificador).WithName($"resumen-{Nombre(variante)}").Build();
```

Qué mirar: la única diferencia entre variantes es el destino de la arista que sale del verificador.

Salida con `--fake` (cliente guionado), sin modelo; el guion escribe un porcentaje equivocado para forzar la vuelta:

```text
e01.un-agente: «Escribí un resumen breve de la encuesta para el informe: qué porcentaje de las respuestas usa colectivo y qué porcentaje usa bicicleta.»
  camino: Datos → Redactor → Verificador → Redactor → Verificador
  redactor (guion): El 25 % de las respuestas usa colectivo y el 35 % usa bicicleta.
  redactor (guion): El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta.
  verificador: coincide
  oráculo porcentajes-del-servicio: pasa
e01.dos-agentes: «Escribí un resumen breve de la encuesta para el informe: qué porcentaje de las respuestas usa colectivo y qué porcentaje usa bicicleta.»
  camino: Datos → Redactor → Verificador → Corrector → Verificador
  redactor (guion): El 25 % de las respuestas usa colectivo y el 35 % usa bicicleta.
  corrector (guion): El 20 % de las respuestas usa colectivo y el 35 % usa bicicleta.
  verificador: coincide
  oráculo porcentajes-del-servicio: pasa
[…]
costo por variante (los mismos pedidos; sin tasa: con 2 entradas ilustra el costo, no responde «¿uno o varios?»):
  un-agente   llamadas al modelo 3 · tokens de entrada 808 · de salida 51 · vueltas 1 · oráculo 2 de 2
  dos-agentes llamadas al modelo 3 · tokens de entrada 802 · de salida 51 · vueltas 1 · oráculo 2 de 2
```

Salida guionada (`--fake`): los tokens los fija el guion; no es una medición.
