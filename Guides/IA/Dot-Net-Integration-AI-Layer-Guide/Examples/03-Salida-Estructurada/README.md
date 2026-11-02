# 03 · Salida estructurada

**Sección de la guía**: §6.3 · **Objetivo de aprendizaje**: OA-D2 (obtener salida tipada con `TryGetResult`, con campos anulables para lo que puede faltar, y pasarla por las reglas antes de guardar) · **Escenario**: ESC-03 · **Tipo**: demostración (TURNO-1), con una extensión de medición (TURNO-2).

## Qué enseña

Una persona escribe «En Bella Vista, Corrientes, CP 3432, no sé cuántos viven» y la aplicación tiene que proponer un alta de localidad. El ejemplo recorre el camino completo:

1. **Salida tipada**: `GetResponseAsync<LocalidadPropuesta>` pide al modelo un JSON con esquema, y `TryGetResult` lo consume. Si el texto no es un JSON del tipo pedido, devuelve `false`, no lanza una excepción, y la aplicación vuelve al formulario de siempre (`sin-propuesta`).
2. **Campos anulables**: en `LocalidadPropuesta`, `Habitantes` es `int?`, como en `LocalidadModel` del caso base. Así el modelo tiene dónde poner «no está».
3. **Garantía del código contra la invención** (`Infrastructure/AI/GarantiaDelTexto.cs`): que el campo sea anulable no impide que el modelo invente, y un número plausible pasaría `HabitantesValidos`. Por eso un valor numérico propuesto (habitantes o código postal) se acepta sólo si aparece en el texto de entrada. Si no aparece, se trata como ausente y la persona lo puede confirmar.
4. **Las reglas deciden** (`Application/Localidades/AltaDesdeTextoService.cs`): la provincia se normaliza contra `Catalogos` («corrientes» → «Corrientes»; «Corrientes Capital» no coincide y la rechaza la regla), y `LocalidadRules` da el veredicto: `lista`, `faltan:<campos>` o `sin-propuesta`. La única forma de escribir es `LocalidadService.SaveAsync`, que valida otra vez.

Razonamiento apagado, `num_ctx`, temperatura y semilla los pone la capa única de `_Comun` (`FabricaDeClientes`). Este ejemplo no fija ninguno.

## Cómo se corre

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 03-Salida-Estructurada                 # --fake --verificar, sin servidor
tools/verificar.sh correr 03-Salida-Estructurada --con-modelo    # contra qwen3.5:4b (turno modelo-compartido)
tools/recalcular-oraculos.sh "$RLLM_DIR"/03-Salida-Estructurada.jsonl
```

A mano, en el contenedor (`bin/` y `obj/` quedan en la carpeta, y el `.gitignore` los excluye):

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/03-Salida-Estructurada \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

Opciones de la consola: `--fake` usa el cliente guionado, `--verificar` sale con 1 si algún invariante de código no se cumple, `--plazo-s N` excluye con motivo «plazo» las entradas que no alcanzan a correr, y `--extension` corre `entradas.ext.v1.json` (M = 8, 4 de ellas sin el dato) en lugar de las 2 entradas de la demostración, y `--cp34` corre `entradas.cp34.v1.json` (una entrada con «CP 34», REC-16). Las dos opciones no se combinan.

## Qué observar

Salida esperada con `--fake` (el guion **inventa** 30000 en e02 a propósito):

```
[e02] En Bella Vista, Corrientes, CP 3432, no sé cuántos viven
  modelo devolvió: {"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":30000}
  TryGetResult: true; descartado por no estar en el texto: habitantes
  decisión de las reglas: faltan:habitantes → Ingrese una cantidad de habitantes mayor a cero.
  oráculo: pasa
altas guardadas: 1
invariantes de código: se cumplen
```

- **Garantía del código** (se comprueba en cada corrida, con modelo o sin él): toda propuesta `lista` tiene un código postal de 4 dígitos, una provincia del catálogo y habitantes ≥ 1; ningún valor que no esté en la entrada llega a las reglas sin confirmación; no se escribe nada sin pasar por las reglas.
- **Conducta del modelo** (descriptiva y sin umbral, §10.4): la consola informa cuántas respuestas fueron un JSON válido (k/M) y cuántas veces el modelo propuso habitantes cuando el pedido no los traía. Esa **tasa de invención**, propuestas con `Habitantes != null` sobre las 4 entradas sin el dato, sólo se mide en la extensión. Con 2 entradas, la demostración no publica tasas. Todas las cifras salen de un estímulo del mismo autor.
- **Oráculo recalculable**: `tools/oraculos/salida-estructurada.jq` vuelve a calcular la decisión leyendo sólo el registro. Para eso reimplementa en jq `TryGetResult`, la garantía del texto, la normalización de provincia y las reglas, sin compartir código con el C#, y la compara con el `decision=…` anotado. Si el C# y el jq no coinciden, `recalcular-oraculos.sh` sale con 1.

## Pruebas

`Pruebas/SalidaEstructuradaTests.cs` corre sin servidor. Cubre: JSON con `habitantes: null` → error de dominio «habitantes»; texto que no es JSON → `TryGetResult = false` y ninguna escritura; habitantes o código postal inventados que se descartan, también un código postal inventado con forma válida («CP 34» en el texto y «3400» propuesto: se descarta y no se escribe; «34» tal cual no se descarta y lo rechazan las reglas); que la garantía cubre los dos campos numéricos de `LocalidadPropuesta`; CP con dígitos no ASCII y nombre de 61 caracteres rechazados por las reglas en el alta desde texto; confirmación de la persona; normalización de provincia; cableado (razonamiento `None`, `num_ctx` y esquema JSON, vistos por un `ClienteEspia` debajo de la capa); servidor compartido sin `num_ctx` del arnés → rechazo; estímulos rotulados y con provincias de `Catalogos`.

`Pruebas/ConModeloTests.cs` (`[Category("LLM")]`) se salta con motivo si el servidor no responde.

## Límites declarados

- La garantía cubre los campos numéricos (código postal y habitantes). Nombre y provincia son texto y no pasan por ella: la provincia la decide `Catalogos` y el nombre, `LocalidadRules`; un nombre o una provincia del catálogo que el pedido no trae los frena la confirmación de la persona.
- La garantía compara números como texto (`90.000` = `90000`). No entiende «90 mil», y un número que el modelo copie del texto en el campo equivocado pasa la garantía si la regla lo admite: por eso la persona confirma antes de guardar. El mismo número no puede ser a la vez código postal y habitantes.
- La confirmación de la persona está **simulada** en la consola (el ejemplo no tiene pantalla): se confirma lo que quedó `lista`, sin cambios.
- La **extensión de medición** (M = 8) se corre con `--extension` (ver `extension` en `ejemplo.json`). Por el arnés: `tools/verificar.sh extension 03-Salida-Estructurada`, o en seco con `--fake` (ver «Vías» en `Examples/README.md`). La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.
- **Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-03, estación 1, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): v1 + extensión 10/10 pasan salida-estructurada, IC95 [0,692; 1], N entradas distintas; cp34: el modelo copió el 34 en 10 de 10 repeticiones (semilla 42 fija) y las reglas lo rechazaron las 10, IC95 [0,692; 1].
- El repositorio está en memoria y lo escribimos para el ejemplo. El dominio y `LocalidadService` vienen de `_Comun` (copia de `8fb4c3a`).

## Datos

`entradas.v1.json`, `entradas.ext.v1.json` y `entradas.cp34.v1.json` los escribimos para el ejemplo y están congelados con su SHA-256 en `ejemplo.json`. `entradas.cp34.v1.json` copia tal cual el pedido «Agregá Goya, Corrientes, CP 34, 89959 habitantes» de la línea archivada del 2026-10-28, para comparar el mismo pedido con la composición de este ejemplo; su 89959 no es del caso base. Las otras dos usan sólo valores del caso base: Goya 3450/90000 y Bella Vista 3432/30000 (`LocalidadesTests.cs` l. 50 y l. 102 @ `8fb4c3a`), y Corrientes 3400/346334 y Resistencia 3500/291720 (`SessionSeeder.cs` @ `8fb4c3a`).

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 6; `// …` marca un recorte.

`Examples/03-Salida-Estructurada/Application/Localidades/LocalidadPropuesta.cs`
```csharp
public sealed record LocalidadPropuesta
{
    [JsonPropertyName("nombre")] public string? Nombre { get; init; }

    [JsonPropertyName("provincia")] public string? Provincia { get; init; }

    [JsonPropertyName("codigoPostal")] public string? CodigoPostal { get; init; }

    [JsonPropertyName("habitantes")] public int? Habitantes { get; init; }
}
```

`Examples/03-Salida-Estructurada/Infrastructure/AI/ExtractorDeLocalidadConModelo.cs`
```csharp
public async Task<DetalleDeExtraccion> ExtraerConDetalleAsync(string texto, CancellationToken cancellationToken = default)
{
    List<ChatMessage> mensajes = [new(ChatRole.User, texto)];
    var opciones = new ChatOptions { Instructions = Instrucciones };
    var respuesta = await cliente.GetResponseAsync<LocalidadPropuesta>(mensajes, opciones, cancellationToken: cancellationToken);

    // …
    if (!respuesta.TryGetResult(out var propuesta) || propuesta is null)
    {
        return new DetalleDeExtraccion(new ExtraccionDeLocalidad(null, []), mensajes, opciones, respuesta, false, null);
    }
    var (garantizada, descartados) = GarantiaDelTexto.Aplicar(propuesta, texto);
    return new DetalleDeExtraccion(new ExtraccionDeLocalidad(garantizada, descartados), mensajes, opciones, respuesta, true, propuesta);
}
```

El caso de uso de Application normaliza, valida y escribe:

`Examples/03-Salida-Estructurada/Application/Localidades/AltaDesdeTextoService.cs`
```csharp
public static DecisionDeAlta Decidir(ExtraccionDeLocalidad extraccion)
{
    if (extraccion.Propuesta is not { } p)
    {
        return new DecisionDeAlta(EstadoDeLaPropuesta.SinPropuesta, new LocalidadModel(),
            new Dictionary<string, string>(), extraccion.Descartados);
    }

    var modelo = new LocalidadModel
    {
        Nombre = (p.Nombre ?? string.Empty).Trim(),
        Provincia = NormalizarProvincia(p.Provincia),
        CodigoPostal = (p.CodigoPostal ?? string.Empty).Trim(),
        Habitantes = p.Habitantes
    };
    var errores = Validar(modelo);
    return new DecisionDeAlta(errores.Count == 0 ? EstadoDeLaPropuesta.Lista : EstadoDeLaPropuesta.FaltanDatos,
        modelo, errores, extraccion.Descartados);
}

// …
public Task<Result> ConfirmarAsync(DecisionDeAlta decision, int? habitantesConfirmados = null,
    string? codigoPostalConfirmado = null, CancellationToken cancellationToken = default)
{
    // …
    return localidades.SaveAsync(modelo, cancellationToken);
}
```

Qué mirar: el `false` de `TryGetResult` termina en `SinPropuesta`, el formulario de siempre, y `ConfirmarAsync` escribe sólo por `SaveAsync`.

Salida con `--fake` (cliente guionado), sin modelo; el guion inventa el dato a propósito para mostrar la garantía:

```text
[e02] En Bella Vista, Corrientes, CP 3432, no sé cuántos viven
  modelo devolvió: {"nombre":"Bella Vista","provincia":"Corrientes","codigoPostal":"3432","habitantes":30000}
  TryGetResult: true; descartado por no estar en el texto: habitantes
  decisión de las reglas: faltan:habitantes → Ingrese una cantidad de habitantes mayor a cero.
  oráculo: pasa
```

### Para el capítulo 11 del libro

Fragmento que el libro comenta en §11.3 (contrato de la salida estructurada):

`Examples/03-Salida-Estructurada/Pruebas/SalidaEstructuradaTests.cs`
```csharp
[Test]
public async Task TextoNoJson_TryGetResultFalse_SinPropuesta_YNoSeEscribe()
{
    var (alta, _, repo, _) = Armar("La localidad es Bella Vista, en Corrientes.");
    var decision = await alta.ProponerAsync("En Bella Vista, Corrientes, CP 3432, no sé cuántos viven");

    Assert.That(decision.Estado, Is.EqualTo(EstadoDeLaPropuesta.SinPropuesta));
    Assert.That((await alta.ConfirmarAsync(decision)).IsSuccess, Is.False);
    Assert.That(repo.Altas, Is.Empty);
}

// …
[Test]
public async Task Cableado_TodaLlamadaSaleConRazonamientoApagadoNumCtxYEsquema()
{
    // …
    Assert.That(opciones.ResponseFormat, Is.InstanceOf<ChatResponseFormatJson>());
    Assert.That(((ChatResponseFormatJson)opciones.ResponseFormat!).Schema, Is.Not.Null);
}
```

Qué mirar: ante texto libre no hay propuesta ni escritura. El tercer caso del contrato, un valor fuera de catálogo, lo prueba 15 (`Catalogo_MedioInventado_FallaConLaClave`).
