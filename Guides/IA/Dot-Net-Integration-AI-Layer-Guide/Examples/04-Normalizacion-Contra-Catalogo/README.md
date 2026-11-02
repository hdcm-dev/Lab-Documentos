# 04 · Normalización contra el catálogo

**Dificultad:** básico · **Sección de la guía:** §6.3 (salida estructurada) · **Objetivo:** OA-D2, obtener salida tipada con `TryGetResult` y pasarla por las reglas antes de guardar.

## Qué enseña

La persona contesta en sus palabras («voy en bondi y a veces en bici, casi todos los días») y la encuesta guarda claves de `Catalogos` (`colectivo`, `bicicleta`, `diaria`). El ejemplo separa tres responsabilidades:

1. **El modelo propone** texto, en un JSON tipado (`GetResponseAsync<PropuestaDeMovilidad>` y `TryGetResult`). Es una propuesta: no son claves ni se guardan así.
2. **El adaptador normaliza** (`Infrastructure/AI/NormalizadorDeCatalogo.cs`): pasa cada texto a su clave con las claves y etiquetas del catálogo más una tabla de sinónimos «escrito para el ejemplo». Si un texto no tiene clave, **lo deja como está** (plegado: «Avión» → `avion`) y no lo descarta. El plegado es explícito y finito, a propósito: minúsculas ASCII, una tabla corta de tildes, sin marcas combinantes y sin signos en los bordes. No usa `ToLowerInvariant` ni `Normalize`, que dependen de las tablas Unicode de la plataforma y que el oráculo jq no puede reproducir (además, `Normalize` tira `ArgumentException` con U+FFFE).
3. **`ValidarPaso` decide** (`_Comun`, `EncuestaService`). Con el endurecimiento de `_Comun`, un medio fuera del catálogo es un error con su nombre («Medio fuera del catálogo: avion»). En el caso base, `["avion"]` pasaba la validación y se registraba como lista vacía (hueco H4).

La garantía no depende del modelo: **toda clave que resulta pertenece a `Catalogos`, o el paso tiene error; nunca queda una lista vacía sin error.** Eso incluye las respuestas raras que `TryGetResult` acepta. Por ejemplo, `{"medios": null}` es JSON válido: System.Text.Json deja la lista en `null`, porque el `= []` del record sólo rige cuando la propiedad falta. `Decidir` lo toma como lista vacía y `ValidarPaso` da el error de medios. Lo prueban `MediosNull_TerminaEnErrorDeMedios_SinExcepcion` y los 103 casos de paridad (abajo).

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 04-Normalizacion-Contra-Catalogo                 # --fake, sin red
tools/verificar.sh correr 04-Normalizacion-Contra-Catalogo --con-modelo    # contra el modelo del manifiesto (correr build antes)
```

A mano, en contenedor:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/04-Normalizacion-Contra-Catalogo \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test && dotnet run -- --fake --verificar'
```

El proyecto es a la vez consola y pruebas (`GenerateProgramFile=false`): `dotnet test` corre `Pruebas/` y `dotnet run` corre `Program.cs`. Argumentos: `--fake` (guion, sin red), `--verificar` (sale con 1 si una garantía de código falla), `--plazo-s N` y `--extension` (las 8 entradas de la extensión de medición). Variables: las de `Examples/README.md` (`OLLAMA_ENDPOINT`, `OLLAMA_MODEL`, `OLLAMA_NUM_CTX`…).

## Qué observar

Con `--fake`:

```text
d01: «Voy en bondi y a veces en bici, casi todos los días.»
  propuesta del guion: {"medios":["bondi","bici"],"frecuencia":"casi todos los días"}
  claves: medios [bicicleta, colectivo] · frecuencia «diaria»
  ValidarPaso(2): sin errores
  oráculo normalizacion-cerrada: pasa
d02: «Algunos días por semana voy caminando y otros en moto.»
  propuesta del guion: {"medios":["avion"],"frecuencia":"algunos días"}
  claves: medios [avion] · frecuencia «semanal»
  ValidarPaso(2): medios: Medio fuera del catálogo: avion.
  oráculo normalizacion-cerrada: falla
...
invariantes de código: se cumplen
```

- En d02 el guion **inventa** un medio a propósito: el código no lo convierte en nada ni lo tira; lo rechaza con error. Ese «falla» del oráculo es la conducta correcta del código ante una mala propuesta.
- Contra el modelo, mirá la propuesta cruda. En las corridas registradas devolvió las dos veces las palabras de la persona («bondi», «caminando»), no etiquetas del catálogo. Si devolviera una etiqueta («Auto particular»), el adaptador la lleva a la misma clave: eso lo prueba `Medio_SeNormalizaALaClave`, no el modelo.
- Si el modelo no devuelve el JSON pedido, `TryGetResult` da `false` y el paso queda con error de medios: nada llega a la encuesta sin las reglas.

## Entradas, oráculos y registro

| Archivo | Qué es |
|---|---|
| `entradas.v1.json` | Demostración: 2 expresiones, «escrito para el ejemplo». Sin tasa. |
| `entradas.extension.v1.json` | Extensión de medición: 8 expresiones con la clave esperada. Tasa k/8, descriptiva (§10.4), «estímulo del mismo autor». |
| `../../tools/oraculos/normalizacion-cerrada.jq` | Oráculo de la demostración: `TryGetResult` acepta la propuesta y, normalizada, todas sus claves están en `Catalogos`. |
| `../../tools/oraculos/normalizacion-esperada.jq` | Oráculo de la extensión: además, las claves son las esperadas. |
| `../../tools/oraculos/normalizacion-comun.jq` | Módulo que incluyen los dos oráculos (no es un oráculo): reproduce en jq lo que hace `TryGetResult` y el plegado con la tabla de sinónimos. |
| `Pruebas/paridad-del-oraculo.v1.json` | 103 salidas posibles del modelo («escrito para el ejemplo») con el veredicto que les da el código. |
| `Pruebas/paridad-del-oraculo.sh` | Corre esos casos por los oráculos jq (en el host, con jq) y sale con 1 si alguno no coincide con el código. |

Los dos SHA-256 de las entradas están congelados en `ejemplo.json`. Los oráculos se recalculan leyendo sólo el registro (`tools/recalcular-oraculos.sh`), y para eso tienen que decidir **lo mismo que el código** con cualquier salida, no sólo con las que se observaron. Por eso `normalizacion-comun.jq` reproduce las reglas de `TryGetResult`, medidas con una sonda el 2026-10-27 (M.E.AI 10.11.0):

- Lee el primer valor JSON del texto y deja de lado lo que venga después: `{…} gracias` vale.
- Delante del objeto sólo acepta espacios JSON. Un cerco `` ```json ``, un BOM o un texto previo dan `false`.
- JSON estricto: sin comas finales, `NaN`, `01` ni comentarios, y con una profundidad máxima de 64.
- Los nombres de propiedad no distinguen mayúsculas, y si una se repite gana la última.
- `medios` es una lista de cadenas o `null`. Una lista con otra cosa, por ejemplo `["bondi", 1]`, da `false`.

La paridad tiene dos lados. La prueba `ParidadDelOraculo` corre los 103 casos por el código, y `paridad-del-oraculo.sh`, por el oráculo. Si se cambia el código, el módulo jq o la tabla, se cambian juntos, se vuelven a correr los dos lados y las entradas pasan a `v2`.

En los dos modos, `--fake` y contra el modelo, el registro anota el `num_ctx`, el razonamiento y la temperatura que vio un `ClienteEspia` puesto debajo de la capa única, pegado al cliente base: es lo que llevó la llamada, no un recálculo.

La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

**Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-04, estación 1, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): 8/10 (v1 1/2, extensión 7/8), IC95 [0,444; 0,975], N entradas distintas; ninguna clave fuera de Catalogos llegó a la encuesta.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 6; `// …` marca un recorte.

`Examples/04-Normalizacion-Contra-Catalogo/Application/NormalizacionDeMovilidad.cs`
```csharp
public ResultadoDeNormalizacion Decidir(PropuestaDeMovilidad? propuesta)
{
    var modelo = new EncuestaModel();
    if (propuesta is null)
    {
        // El modelo no devolvió el JSON pedido: el paso queda vacío y ValidarPaso lo dice.
        return new(modelo, _encuestas.ValidarPaso(2, modelo), PropuestaValida: false);
    }

    // Cada medio propuesto entra a la encuesta: normalizado si hay clave, tal cual si no.
    // …
    foreach (var texto in propuesta.Medios ?? [])
    {
        if (texto is null) continue;
        var clave = normalizador.NormalizarMedio(texto);
        if (clave.Length > 0) modelo.AlternarMedio(clave, true); // «  » o «¿?» plegados quedan vacíos
    }
    modelo.Frecuencia = propuesta.Frecuencia is null ? string.Empty : normalizador.NormalizarFrecuencia(propuesta.Frecuencia);

    return new(modelo, _encuestas.ValidarPaso(2, modelo), PropuestaValida: true);
}
```

Qué mirar: un medio sin clave entra «tal cual», no se descarta, y `ValidarPaso` lo rechaza con su nombre.
