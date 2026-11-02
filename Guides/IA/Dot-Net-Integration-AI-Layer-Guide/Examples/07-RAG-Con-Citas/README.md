# 07 · RAG con citas

**Dificultad:** avanzado · **Sección de la guía:** §8.3–§8.5 (RAG, citas y «no sé») · **Objetivo:** OA-D4, armar una recuperación con fuentes y validar en código lo que el modelo cita.

## Qué enseña

La ayuda del sistema responde preguntas sobre los requisitos del ABM y de la encuesta con los 12 literales de `LocalidadPolicy` (4) y `EncuestaPolicy` (8). El ejemplo arma el camino entero de un RAG chico y separa lo que decide el código de lo que propone el modelo:

1. **Índice** (`Application/IndiceDeAyuda.cs`): un `InMemoryVectorStore` (`CommunityToolkit.VectorData.InMemory`) reconstruido al arrancar desde los literales, un fragmento por propiedad, con el texto de la plantilla `plantilla.v1` de 06 y el id calificado por clase (`LocalidadPolicy.RequisitoDelNombre` ≠ `EncuestaPolicy.RequisitoDelNombre`). Si cambia una regla, cambia la ayuda.
2. **Recuperación top-3 y contexto con `Fuente`** (`Application/AsistenteDeAyuda.cs`): cada renglón del contexto lleva `[Fuente: <id>]` adelante; las instrucciones piden citar con ese formato y contestar exactamente «No sé.» si el contexto no alcanza. Las instrucciones **traen un ejemplo de cita** (`[Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]`), que es además una fuente real del corpus: el formato sale del ejemplo, y el modelo puede copiar con él el id. Se dejan como están para no mover la medición.
3. **Cita validada en código** (`Application/RespuestaConCitas.cs`): una respuesta se muestra sólo si **toda** cita apunta a un fragmento recuperado para esa pregunta, o si es el «No sé» pedido. Lo demás se rechaza y la persona ve un aviso. Esa es la garantía, y no depende del modelo: vale siempre.
4. **Oráculo de la afirmación central** (`Application/OraculoDeCitas.cs`, y el mismo en `tools/oraculos/rag-con-citas.jq`): citar no es estar fundamentado. Una cita válida puede contradecir lo que cita (el caso de N3 E5). El oráculo saca los valores del literal citado (dígitos o palabras: «4», «cuatro») y exige que estén en la respuesta y que no aparezca otro valor.

El corpus y el lector del oro **no se copian**: el `.csproj` enlaza `CorpusDePolicies.cs`, `ConjuntoDeOro.cs` y `oro/conjunto-de-oro.v1.json` desde `../06-Embeddings-Y-Busqueda-Semantica/` (una sola fuente; plan §F.6). El oro que corre 07 es `oro/conjunto-de-oro.v2.json`: las 16 preguntas del v1, idénticas y en el mismo orden, más **n05**, una no respondible cercana a una regla real («¿Cuántos medios de transporte se pueden marcar como máximo?»: el literal de los medios fija sólo el mínimo). El v1 queda como histórico. Los SHA-256 de los dos están congelados en `ejemplo.json`, y una prueba de cableado comprueba que v2 contiene al v1 sin cambios.

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 07-RAG-Con-Citas                 # --fake, sin red
tools/verificar.sh correr 07-RAG-Con-Citas --con-modelo    # contra el modelo de chat del manifiesto (correr build antes)
tools/verificar.sh embeddings 07-RAG-Con-Citas             # fase 3a: turno modelo-propio, escribe RLLM_DIR/embeddings/
tools/verificar.sh estabilidad 07-RAG-Con-Citas            # 3 repeticiones de r03, turno modelo-compartido
tools/verificar.sh embeddings 07-RAG-Con-Citas --fake      # cualquiera de las dos vías en seco (turno build, sin red)
```

En seco, `--fake --precomputar-embeddings` escribe el archivo con el generador estable y `modelo` «guion», en `RLLM_DIR/seco/`. La corrida de chat rechaza un archivo así.

Los embeddings se calculan **antes**, en el servidor de embeddings (con EMB-B, un servidor aparte del chat), y la corrida de chat los lee del disco (así no hacen falta los dos servidores a la vez). A mano, en contenedor:

```bash
# 1) pruebas y modo falso
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/07-RAG-Con-Citas \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
# 2) embeddings (segundo servidor con qwen3-embedding:0.6b): escribe RLLM_DIR/embeddings/07-RAG-Con-Citas.v1.json
dotnet run -- --precomputar-embeddings          # con OLLAMA_EMBED_ENDPOINT (o OLLAMA_PROPIO_ENDPOINT)
# 3) chat (qwen3.5:4b): las 17 preguntas del oro v2
dotnet run -- --verificar                       # con OLLAMA_ENDPOINT, OLLAMA_SERVIDOR y OLLAMA_NUM_CTX
dotnet run -- --verificar --estabilidad         # 3 repeticiones de r03; no cuentan para la tasa
```

Argumentos: `--fake` (guion y embeddings estables FNV-1a, sin red), `--verificar` (sale con 1 si una garantía de código falla), `--plazo-s N`, `--precomputar-embeddings` y `--estabilidad`. Con `EMB_PLAN=EMB-C` la corrida usa el generador estable y se rotula «no ejecutado contra un modelo de embeddings». Variables: las de `Examples/README.md`.

## Qué observar

Con `--fake` (cableado, no calidad: el generador estable sólo acerca textos que comparten palabras):

```text
r03: «¿Cuántos números lleva el código postal de una localidad?»
  recuperados: LocalidadPolicy.RequisitoDelCodigoPostal (0.463), EncuestaPolicy.RequisitoDeLaLocalidad (0.267), LocalidadPolicy.RequisitoDeLaProvincia (0.267)
  modelo: El código postal lleva 4 dígitos. [Fuente: LocalidadPolicy.RequisitoDelCodigoPostal]
  código: Aceptada
  oráculo rag-con-citas: pasa
r09: …
  modelo: Se pregunta con qué asiduidad viaja. [Fuente: EncuestaPolicy.RequisitoDelHorario]
  código: RechazadaCitaNoRecuperada → se muestra el aviso de rechazo
e5: «Si no viajo, ¿qué pongo en la distancia por día y en los minutos?»
  modelo: Si no viajás, tenés que poner cero kilómetros y cero minutos [Fuente: EncuestaPolicy.RequisitoDeLaDistancia][Fuente: EncuestaPolicy.RequisitoDeLosMinutos].
  código: Aceptada
  oráculo rag-con-citas: falla (no afirma el valor del literal relevante citado)
…
invariantes de código: se cumplen
```

- **r09** cita una fuente inventada: el código la rechaza. **n02** contesta sin cita: también. Eso es la garantía.
- **e5** es la respuesta de N3 E5 (de `qwen2.5:7b`, con los ids de este corpus), como caso guionado: sus dos citas son **válidas** (se recuperaron) y la respuesta igual contradice la regla de minutos (de 1 a 600). La garantía de citas no la ve; el oráculo sí. Por eso hacen falta las dos capas (§7.4 y §10.6).
- Con el generador estable varias citas correctas quedan rechazadas porque el fragmento no se recuperó (r04, r10, r11): en modo falso las tasas no dicen nada de calidad.
- Contra el modelo, mirá cuántas respuestas citan bien y cuántas el código tuvo que rechazar, y si el «No sé» aparece en las 5 no respondibles. En **n05** el fragmento que se recupera es el correcto (el de los medios), y aun así no alcanza: es la tentación de contestar «uno».

## Lo que se vio contra el modelo

**Observado en el laboratorio, hasta el 2026-10-28 (historia: oro v1, 16 preguntas, sin n05; servidor de laboratorio retirado): qwen3.5:4b, Ollama 0.32.0-rc0, sólo CPU, servidor compartido, 2026-10-27**; embeddings `qwen3-embedding:0.6b` en Ollama 0.40.11 (fase 3a). Estímulo del mismo autor; tasas descriptivas (§10.4, M ≤ 12), sin exclusiones:

| Medida | k/M | IC95 Clopper-Pearson |
|---|---|---|
| afirmación correcta (con valor verificable) | 7/7 | [0,590; 1] |
| cita válida (sin valor verificable) | 5/5 | [0,478; 1] |
| «No sé» (no respondibles) | 0/4 | [0; 0,602] |
| cita y contradice (respondibles) | 1/12 | [0,002; 0,385] |

Lo que conviene mirar en el registro:

- **Ninguna de las 4 no respondibles dijo el «No sé.» pedido.** El modelo explicó que no tenía el dato y, en tres, inventó una «fuente» (`[Fuente: No sé]`, `[Fuente: No disponible]`): el código las rechazó porque citan algo que no se recuperó, y la persona vio el aviso. En n01 citó un fragmento real y la respuesta se mostró: no afirma un número, pero tampoco es el «No sé» pedido, y el oráculo la marca. La garantía se cumplió en las 16; la conducta pedida, no.
- **r09** cita la regla de frecuencia (sin valor verificable, así que pasa por la cita) y agrega «500 km diarios», que es de la regla de distancia, no citada: es el único «cita y contradice».
- **r07** dice «Sí, podés escribir una localidad que no esté cargada», lo contrario de la regla que cita, y pasa: en las 5 sin valor verificable el oráculo sólo comprueba la cita. Es el límite declarado del oráculo, y el motivo de la evaluación de §10 (ejemplo 15).
- **Estabilidad** (aparte, no cuenta para la tasa): 3 repeticiones de r03 con `temperature` 0 y `seed` fijos dieron el mismo texto las 3 veces, con la misma recuperación.

## Entradas, oráculo y registro

| Archivo | Qué es |
|---|---|
| `oro/conjunto-de-oro.v2.json` | 17 preguntas, «escrito para el ejemplo»: 12 respondibles (7 con valor verificable, 5 sin valor) y 5 no respondibles (3 cercanas, con n05, y 2 lejanas). Es el que corre 07. Congelado por SHA-256 en `ejemplo.json`. |
| `../06-Embeddings-Y-Busqueda-Semantica/oro/conjunto-de-oro.v1.json` | El v1 (16 preguntas, 4 no respondibles), histórico: lo usan 06 y 15, y es el de la tabla de arriba. Congelado por SHA-256 en `ejemplo.json`. |
| `GuionDelEjemplo.cs` | Guion del modo falso (escrito para el ejemplo), con el caso E5. |
| `tools/oraculos/rag-con-citas.jq` | El oráculo, recalculado sólo desde el registro (`tools/recalcular-oraculos.sh`). |

Tasas que informa la consola (descriptivas, §10.4; **estímulo del mismo autor**): afirmación correcta k/7, cita válida k/5 en las sin valor, «No sé» k/5 y «cita y contradice» k/12. El intervalo lo da `tools/verificar.sh tasa k m`. Los 5 literales sin valor verificable (`RequisitoDeLaProvincia`, `RequisitoDeLaLocalidad`, `RequisitoDeLosMedios`, `RequisitoDeLaFrecuencia`, `RequisitoDelMotivo`) sólo se juzgan por la cita.

Límites declarados del oráculo: reconoce números en dígitos y en palabras sueltas (cero a mil, más «ciento diez»); «un» y «una» no cuentan como 1 porque casi siempre son artículos. «Cita y contradice» compara contra la **unión** de los literales citados: en el caso E5, «cero» es un valor válido de la distancia y no se cuenta como contradicción; la respuesta falla igual porque no afirma los valores de minutos.

El registro R-LLM queda en `RLLM_DIR/07-RAG-Con-Citas.jsonl`: por llamada, `recuperados` (id y puntaje), `contexto` (el texto de grounding) y, en `paso`, el veredicto del código y el modelo de embeddings con su digest. En el servidor compartido, si antes de una entrada `/api/ps` ya no lista el modelo, la llamada queda `estado_cache: frio`.

## Paquetes

`Microsoft.Extensions.AI`, `OllamaSharp`, `Microsoft.Extensions.VectorData.Abstractions` y `CommunityToolkit.VectorData.InMemory`; las versiones, en `Examples/Directory.Packages.props` (la guía las cita desde el Anexo F).

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 8; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/07-RAG-Con-Citas/Application/IndiceDeAyuda.cs`
```csharp
public sealed class FragmentoIndexado
{
    // …
    [VectorStoreKey] public string Fuente { get; set; } = "";
    [VectorStoreData] public string Literal { get; set; } = "";
    [VectorStoreData] public string Texto { get; set; } = "";
    [VectorStoreVector(Dimensiones, DistanceFunction = DistanceFunction.CosineSimilarity)] public string Vectorizado { get; set; } = "";
}

// …
public static async Task<IndiceDeAyuda> CrearAsync(
    IEmbeddingGenerator<string, Embedding<float>> embeddings, CancellationToken cancellationToken = default)
{
    var dimension = embeddings.GetService<EmbeddingGeneratorMetadata>()?.DefaultModelDimensions;
    if (dimension is { } d && d != FragmentoIndexado.Dimensiones)
    {
        throw new InvalidOperationException($"El generador da vectores de {d}; el índice espera {FragmentoIndexado.Dimensiones}.");
    }
    var store = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddings });
    var coleccion = store.GetCollection<string, FragmentoIndexado>(Coleccion);
    await coleccion.EnsureCollectionExistsAsync(cancellationToken);
    await coleccion.UpsertAsync(CorpusDePolicies.Fragmentos().Select(f => new FragmentoIndexado
    {
        Fuente = f.Id, Literal = f.Literal, Texto = f.Texto, Vectorizado = f.Texto
    }), cancellationToken);
    return new IndiceDeAyuda(coleccion);
}
```

Qué mirar: `Vectorizado` es un `string` y el almacén calcula el embedding con el generador que recibe; el texto se repite en `Texto` porque no se puede leer de vuelta [REF-91].

`Examples/07-RAG-Con-Citas/Application/AsistenteDeAyuda.cs`
```csharp
public static string ArmarContexto(IEnumerable<Recuperacion> recuperados)
{
    var sb = new StringBuilder();
    foreach (var r in recuperados) sb.Append("[Fuente: ").Append(r.Fuente).Append("] ").AppendLine(r.Texto);
    return sb.ToString();
}

public async Task<ConsultaDeAyuda> PreguntarAsync(string pregunta, CancellationToken cancellationToken = default)
{
    var recuperados = await indice.BuscarAsync(pregunta, Top, cancellationToken);
    var contexto = ArmarContexto(recuperados);
    // Las instrucciones van en ChatOptions.Instructions (como en 01–04); el contexto y la pregunta,
    // en el mensaje de la persona, con la pregunta al final.
    List<ChatMessage> mensajes = [new(ChatRole.User, $"Contexto:\n{contexto}\nPregunta: {pregunta}")];
    var opciones = new ChatOptions { Instructions = Instrucciones };
    var respuesta = await cliente.GetResponseAsync(mensajes, opciones, cancellationToken);
    var validada = ValidadorDeCitas.Validar(respuesta.Text ?? string.Empty, recuperados.Select(r => r.Fuente).ToHashSet(StringComparer.Ordinal));
    return new(recuperados, contexto, mensajes, opciones, respuesta, validada);
}
```

`Examples/07-RAG-Con-Citas/Application/RespuestaConCitas.cs`
```csharp
public static RespuestaConCitas Validar(string texto, IReadOnlyCollection<string> recuperados)
{
    var citas = Citas(texto);
    if (citas.Count == 0)
    {
        return EsNoSe(texto)
            ? new(texto, citas, Veredicto.NoSe, texto)
            : new(texto, citas, Veredicto.RechazadaSinCita, AvisoDeRechazo);
    }
    return citas.All(recuperados.Contains)
        ? new(texto, citas, Veredicto.Aceptada, texto)
        : new(texto, citas, Veredicto.RechazadaCitaNoRecuperada, AvisoDeRechazo);
}
```

Qué mirar: `Validar` recibe sólo los ids recuperados para esa pregunta; una cita a cualquier otro id, aunque exista en el corpus, termina en `RechazadaCitaNoRecuperada`. En la demo, el índice y el almacén viven en `Infrastructure/AI/Ayuda/`.

Salida con `--fake` (cliente guionado), sin modelo; el guion trae una respuesta con dos citas válidas que contradice la regla de minutos:

```text
e5: «Si no viajo, ¿qué pongo en la distancia por día y en los minutos?»
[…]
  modelo: Si no viajás, tenés que poner cero kilómetros y cero minutos [Fuente: EncuestaPolicy.RequisitoDeLaDistancia][Fuente: EncuestaPolicy.RequisitoDeLosMinutos].
  código: Aceptada
  oráculo rag-con-citas: falla (no afirma el valor del literal relevante citado)
```
