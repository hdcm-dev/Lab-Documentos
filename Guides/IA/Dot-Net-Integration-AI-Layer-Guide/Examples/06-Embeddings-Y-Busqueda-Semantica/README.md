# 06 · Embeddings y búsqueda semántica

**Nivel:** intermedio · **Guía:** §8.1 (embedding y modelo de embeddings) y §8.5 (medir la recuperación antes que la respuesta) · **OA-D4**

## Qué muestra

- `IEmbeddingGenerator<string, Embedding<float>>` como único contrato: el código no sabe si del otro lado está `qwen3-embedding:0.6b` (segundo servidor, plan EMB-B) o el generador estable de `_Comun` (`--fake`, o `EMB_PLAN=EMB-C`). Lo elige `Program.cs`, y nada más.
- Búsqueda por similitud coseno, a mano, sobre un corpus chico: los **12 literales de `*Policy`** del caso base (4 de `LocalidadPolicy`, 8 de `EncuestaPolicy`), leídos por reflexión. Cada fragmento lleva id calificado por clase, porque `RequisitoDelNombre` existe en las dos (`LocalidadPolicy.RequisitoDelNombre` ≠ `EncuestaPolicy.RequisitoDelNombre`). El almacén vectorial (`InMemoryVectorStore`) recién entra en 07: acá se ve lo que hace por dentro.
- El texto que se vectoriza sale de una plantilla fija, `plantilla.v1`, generada en código: «{entidad} · {propiedad en palabras}: {literal}», por ejemplo «Localidad · requisito del código postal: 4 dígitos.». Si cambia una regla del dominio, cambia el texto.
- Cómo se mide una recuperación: **hit rate@1, hit rate@3 y MRR** sobre las 12 preguntas respondibles del conjunto de oro, siempre frente a dos líneas base **sin modelo**: azar y léxica (palabras compartidas con el mismo texto de plantilla). Una ventaja sólo se afirma con la regla pareada de §10.4: binomial exacta bilateral sobre las preguntas discordantes, p < 0,05 (hacen falta al menos 6, todas a favor).
- Dos variantes por pregunta, con y sin la instrucción de consulta de `qwen3-embedding` («Instruct: …\nQuery:…»; va sólo en la pregunta, nunca en el fragmento; la tarea está en inglés, como sugiere la ficha del modelo).
- Al arrancar se verifica que la dimensión del modelo coincide con la del atributo vectorial que usan 07 y PD-2 (1024).

## Entradas

`oro/conjunto-de-oro.v1.json` (**escrito para el ejemplo**, congelado: su SHA-256 está en `ejemplo.json` y en cada línea del registro). Su casa conceptual es §10.7, y lo reutilizan 07 y PD-2.

- 12 respondibles, una por fragmento (`objetivo`). La pregunta del nombre (`r05`) no dice de qué entidad habla, así que tiene **dos** relevantes; para ella se informa además recall@3.
- 4 no respondibles: 2 cercanas (un dato de la base, una acción que el caso no tiene) y 2 lejanas. No entran en el hit rate; se informa su mejor puntaje como insumo para el «no sé» de 07.
- Para 07, cada respondible declara su valor esperado y las formas aceptadas («4», «cuatro»), o `sin_valor_verificable`. Una prueba de cableado comprueba que esos valores salen de `LocalidadRules` y `EncuestaRules`, y que todo valor de catálogo que nombra una pregunta está en `Catalogos`.

`oro/calibracion.v1.json` (**escrito para el ejemplo**, congelado, con su SHA-256 en `ejemplo.json`): 10 preguntas **aparte** del oro (5 respondibles; 5 no respondibles, 3 cercanas y 2 lejanas) para fijar el umbral del «no sé» sin ajustarlo sobre el conjunto que se evalúa (REC-28). No se mezcla con el oro, no entra en el hit rate ni en el MRR, y ninguna pregunta repite una del oro: lo comprueba una prueba de cableado (`Pruebas/CalibracionTests.cs`). La corrida de esta versión todavía no lo lee; lo usa la re-medición con modelo.

## Cómo correrlo

Desde la carpeta de la guía:

```bash
tools/verificar.sh correr 06-Embeddings-Y-Busqueda-Semantica              # --fake: compila y corre con el generador estable
tools/verificar.sh correr 06-Embeddings-Y-Busqueda-Semantica --con-modelo # contra el segundo servidor (turno modelo-propio, fase 3a)
tools/recalcular-oraculos.sh <RLLM_DIR>/06-Embeddings-Y-Busqueda-Semantica.jsonl
```

A mano, sin el arnés (el modelo lo pone `OLLAMA_EMBED_MODEL`; el servidor, `OLLAMA_EMBED_ENDPOINT` u `OLLAMA_PROPIO_ENDPOINT`):

```bash
dotnet test --filter "Category!=LLM"    # cableado, sin red
dotnet run -- --fake --verificar         # la corrida completa con el generador estable
dotnet run -- --verificar                # contra el modelo de embeddings
```

Son 44 llamadas de embedding: 12 fragmentos + 16 preguntas × 2 variantes. Con `--verificar`, sale con 0 sólo si se cumplen los invariantes de código (12 fragmentos con id calificado, oro bien formado, dimensión 1024, orden completo y descendente en cada consulta, una línea R-LLM por llamada). La calidad de la recuperación **no** es un invariante: se informa.

## Salida esperada

- Siempre: `44 llamadas de embedding`, 32 líneas de entrada (24 respondibles con `pasa` o `falla`, 8 no respondibles `excluida`) e `invariantes de código: cumplidos`; con `--verificar`, código de salida 0.
- Con `--fake`, las cifras del generador estable (hit@1 7/12 y 5/12) son de cableado y no se interpretan.
- Contra `qwen3-embedding:0.6b`: se espera que le gane al azar con la regla pareada y que la diferencia con la línea base léxica **no** llegue a afirmarse con 12 preguntas. Las cifras observadas en el laboratorio están en la evidencia de la guía (estímulo del mismo autor), no en este README.

## Qué observar

La salida lista, por pregunta y variante, el rango del primer relevante y el fragmento que quedó primero; después las métricas, las comparaciones pareadas y los puntajes de las no respondibles.

- Con `--fake`, el generador estable sólo acerca textos que comparten palabras: sus cifras prueban el cableado y las métricas, **no** la calidad. Por eso se rotulan «no ejecutado contra un modelo de embeddings», en la consola y en el registro.
- Contra el modelo, mirá si la diferencia con la línea base léxica **se puede afirmar** o no. Con 12 preguntas, una métrica más alta no alcanza: hay que contar las discordantes.
- La pregunta del nombre muestra por qué el id calificado importa: si dos fragmentos comparten nombre de propiedad, sin la clase no se sabe cuál se recuperó.
- El registro (oráculo `recuperacion-top3`) da «pasa» si algún relevante queda entre los 3 primeros. Las no respondibles quedan `excluida`, con el motivo. `tools/oraculos/recuperacion-top3.jq` recalcula el veredicto leyendo sólo el registro: los relevantes van en el `motivo` de la línea de entrada y el orden completo en `recuperados`.
- Lo que el adaptador no expone: OllamaSharp 5.5.0 informa los tokens del embedding (`prompt_eval_count`, que alimenta la guarda de truncado), pero no las duraciones del servidor. Por eso `load_duration_ms` y `total_duration_ms` quedan en `null`, y 06 no compara tiempos.
- **Los embeddings salen sin `num_ctx`.** En el registro, `num_ctx` y `num_ctx_origen` van en `null` en las 44 llamadas. Es a propósito, y no es un hueco de la capa única: `num_ctx` es una opción **de chat** (`ChatOptions`, que aplica `OpcionesDelProveedor`) y existe para que el servidor no recorte en silencio un prompt largo. El generador de embeddings no recibe `ChatOptions`, y el servidor carga el modelo de embeddings con su propia ventana. Lo que `num_ctx` protege no puede pasar acá: el texto más largo que se vectoriza mide 50 tokens según el servidor (`prompt_eval_count`). La guarda de truncado sí corre igual, comparando ese conteo con la estimación por caracteres, y las 44 llamadas dan `truncado_posible` = `false`. Si un corpus futuro tuviera fragmentos largos, el recorte habría que controlarlo partiendo los fragmentos, no con `num_ctx`.

## Archivos

| Archivo | Qué es |
|---|---|
| `Program.cs` | Raíz de composición: elige el generador (modelo o estable) y escribe el registro |
| `Application/CorpusDePolicies.cs` | Los 12 fragmentos por reflexión y `plantilla.v1` |
| `Application/Busqueda.cs` | Coseno y ordenamiento (empates: gana el primero en el orden del corpus) |
| `Application/Metricas.cs`, `Referencias.cs` | hit rate@k, MRR, recall@3, líneas base y regla pareada |
| `Application/CorridaDeBusqueda.cs` | Las 44 llamadas, el veredicto por entrada y los vectores persistidos para 07 (`RLLM_DIR/embeddings/`) |
| `Application/Informe.cs` | Las cifras y cómo se leen |
| `Pruebas/` | Cableado sin red y una prueba `[Category("LLM")]` con predicado de salto |

Las cifras salen de un estímulo del mismo autor: el conjunto de oro y el corpus los eligió quien escribió el ejemplo. El 8/8 de N3 E3 se midió sobre otro corpus y no se usa acá.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 8; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/06-Embeddings-Y-Busqueda-Semantica/Application/CorpusDePolicies.cs`
```csharp
public static IReadOnlyList<Fragmento> Fragmentos() =>
    [.. DeLaClase(typeof(LocalidadPolicy)), .. DeLaClase(typeof(EncuestaPolicy))];

/// <summary>plantilla.v1: «{entidad} · {propiedad en palabras}: {literal}». La línea base léxica usa el mismo texto.</summary>
public static string Texto(string entidad, string propiedad, string literal) =>
    $"{entidad} · {EnPalabras(propiedad)}: {literal}";

// …
private static IEnumerable<Fragmento> DeLaClase(Type clase)
{
    var entidad = clase.Name[..^"Policy".Length];
    // Orden de declaración (MetadataToken): el de los archivos copiados de 8fb4c3a.
    return clase.GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.PropertyType == typeof(string))
        .OrderBy(p => p.MetadataToken)
        .Select(p =>
        {
            var literal = (string)p.GetValue(null)!;
            return new Fragmento($"{clase.Name}.{p.Name}", entidad, p.Name, literal, Texto(entidad, p.Name, literal));
        });
}
```

`Examples/06-Embeddings-Y-Busqueda-Semantica/Application/Metricas.cs`
```csharp
public static MedidaDePregunta Medir(string preguntaId, IReadOnlyList<Posicion> orden, IReadOnlyCollection<string> relevantes)
{
    var rango = Rango(orden, relevantes);
    var recall3 = relevantes.Count == 0 ? 0 : (double)orden.Take(3).Count(p => relevantes.Contains(p.Id)) / relevantes.Count;
    return new(preguntaId, rango, rango == 1, rango is >= 1 and <= 3, rango == 0 ? 0 : 1.0 / rango, recall3);
}
```

Qué mirar: el id `$"{clase.Name}.{p.Name}"` distingue los dos `RequisitoDelNombre`, y `Medir` saca de un mismo rango el hit@1, el hit@3 y el rango recíproco. La pregunta del nombre tiene dos relevantes; para ella se informa además recall@3.

Salida con `--fake` (generador estable), sin modelo:

```text
Corpus: 12 fragmentos (plantilla.v1), p. ej. «Localidad · requisito del nombre: Entre 3 y 60 caracteres, único por provincia.»
Generador estable FNV-1a: no ejecutado contra un modelo de embeddings. Las cifras prueban cableado y métricas, NO calidad.
```

### Para el capítulo 11 del libro

Fragmentos que el libro comenta en §11.4 (comparación pareada) y §11.7 (conjunto de oro):

`Examples/06-Embeddings-Y-Busqueda-Semantica/Application/Referencias.cs`
```csharp
public sealed record ComparacionPareada(string A, string B, string Medida, int SoloA, int SoloB, double P)
{
    public bool SeAfirmaDiferencia => SoloA + SoloB > 0 && P < 0.05;

    public static ComparacionPareada Comparar(string a, string b, string medida, IReadOnlyList<bool> aciertosA, IReadOnlyList<bool> aciertosB)
    {
        if (aciertosA.Count != aciertosB.Count) throw new ArgumentException("El diseño pareado pide las mismas preguntas por las dos variantes.");
        var soloA = aciertosA.Zip(aciertosB).Count(x => x.First && !x.Second);
        var soloB = aciertosA.Zip(aciertosB).Count(x => !x.First && x.Second);
        return new(a, b, medida, soloA, soloB, BinomialBilateral(soloA, soloB));
    }
    // …
}
```

Qué mirar: se afirma una diferencia sólo con pares discordantes y p < 0,05 de la binomial exacta bilateral.

`Examples/06-Embeddings-Y-Busqueda-Semantica/oro/conjunto-de-oro.v1.json`
```json
    { "id": "r03", "tipo": "respondible", "objetivo": "LocalidadPolicy.RequisitoDelCodigoPostal",
      "texto": "¿Cuántos números lleva el código postal de una localidad?",
      "relevantes": ["LocalidadPolicy.RequisitoDelCodigoPostal"],
      "valor_esperado": "4", "formas_aceptadas": [["4", "cuatro"]], "valores_de_catalogo": [] },
```

Qué mirar: `texto`, `relevantes` y `valor_esperado` son consulta, contexto y respuesta.
