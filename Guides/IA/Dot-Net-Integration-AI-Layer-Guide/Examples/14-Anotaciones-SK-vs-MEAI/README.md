# 14 · Anotaciones: Semantic Kernel frente a M.E.AI

**Dificultad:** intermedio · **Sección de la guía:** §10.7 (si venís de Semantic Kernel) · **Objetivo:** OA-SK1 (pasar una herramienta `[KernelFunction]` a `[Description]` + `AIFunctionFactory` sin cambiar su esquema).

**Ruta opcional**, fuera de la secuencia principal: es para quien ya tiene herramientas escritas con Semantic Kernel. Si no venís de SK, la forma que enseña la guía es la de 08 y podés saltear este ejemplo.

**Sin modelo.** La consola no llama a ningún `IChatClient`. Las pruebas de las trampas de la migración (abajo) usan uno guionado, escrito en la prueba, que no habla con ningún servidor. Todo corre en la fase `build`.

## Qué enseña

La misma herramienta, `contar_localidades(provincia)`, escrita dos veces en el mismo proyecto y sobre el mismo `LocalidadService`:

| Forma | Archivo | Qué la anota |
|---|---|---|
| Semantic Kernel 1.81.0 (la anterior) | `Infrastructure/AI/PluginSk.cs` | `[KernelFunction("contar_localidades")]` + `[Description]`, agrupada en un plugin con `KernelPluginFactory.CreateFromObject` |
| M.E.AI (la de hoy, la que usan 08 y MAF) | `Infrastructure/AI/HerramientaMeai.cs` | sólo `[Description]` (de `System.ComponentModel`), y `AIFunctionFactory.Create` |

El cuerpo vive una sola vez en `Application/Localidades/ConteoDeLocalidades.cs`: las dos anotaciones delegan ahí en una línea. Migrar es **borrar `[KernelFunction]` y cambiar la fábrica**; la `[Description]` es la misma en los dos marcos, así que el esquema que ve el modelo no cambia. Eso es lo que el ejemplo prueba, leyendo los metadatos que arma cada marco (no las constantes del código): nombre, descripción y, por parámetro, nombre, tipo JSON, descripción y obligatoriedad, más el esquema JSON del parámetro tal cual. Después invoca las dos con la misma `FunctionCallContent` y compara el resultado.

Tres cosas que hay que saber al migrar (las tres observadas con SK 1.81.0 y M.E.AI 10.11.0 al construir este ejemplo, y fijadas por pruebas que avisan si una versión nueva las cambia):

1. **`FunctionCallContent` existe en los dos espacios de nombres** (también `TextContent`, `FunctionResultContent`…). Un archivo con `using Microsoft.Extensions.AI;` y `using Microsoft.SemanticKernel;` no compila sin calificarlo: `error CS0104: 'FunctionCallContent' is an ambiguous reference between 'Microsoft.Extensions.AI.FunctionCallContent' and 'Microsoft.SemanticKernel.FunctionCallContent'`. En `Comparador.cs`, el único archivo con los dos `using`, va calificado.
2. **La `KernelFunction` ya es un `AIFunction`, pero con el nombre del plugin adelante**: montada tal cual en `ChatOptions.Tools`, el modelo ve `Localidades_contar_localidades`, no `contar_localidades`. Si el *prompt* o los registros nombran la herramienta, ese cambio de nombre importa.
3. **El esquema de la función entera no es idéntico**: el de SK trae una clave `condition` con la descripción de la función, que el de M.E.AI no tiene. El de los parámetros sí es idéntico.

Y una diferencia en lo que vuelve: M.E.AI devuelve el resultado ya serializado y sin las propiedades `null`; SK devuelve el objeto, con `"error":null`. El ejemplo los compara **normalizados** (leídos como `ConteoDto` y vueltos a escribir igual) y muestra los dos crudos.

Y tres trampas de la migración que **no dan error**, fijadas como casos rojos sin servidor en `Pruebas/TrampasDeMigracionTests.cs` (observadas con SK 1.81.0, M.E.AI 10.11.0 y MAF 1.26.0; el «modelo» es un `IChatClient` guionado que pide `contar_localidades` una vez y repite el resultado):

- **Sin `UseKernelFunctionInvocation`**, SK sobre un `IChatClient` devuelve la llamada a la herramienta como respuesta, **sin ejecutarla y sin error**: 0 ejecuciones, 1 llamada al cliente, texto vacío. Con `UseKernelFunctionInvocation` (la prueba de control) la ejecuta una vez y hace 2 llamadas.
- **`AIFunctionFactory` no lee `[KernelFunction]`.** Si se deja la anotación vieja y no se pasa `name`, la función se llama `ContarLocalidades` (el método sin el sufijo `Async`), no `contar_localidades`: el nombre que veía el modelo cambia sin aviso. `[Description]` sí se lee.
- **Entregar la `KernelFunction` tal cual a un agente de MAF** compila (es un `AIFunction`) y se ofrece al modelo como `Localidades_contar_localidades`, pero al ejecutarla falla: 0 ejecuciones, y el modelo recibe `Error: Function failed.` sin que la aplicación vea una excepción. Con `IncludeDetailedErrors` el motivo es `` No service for type 'System.Collections.Generic.IEnumerable`1[Microsoft.SemanticKernel.KernelPlugin]' has been registered. `` Migrar es reescribir la anotación, no reusar el objeto del plugin.

## Cómo correrlo

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 14-Anotaciones-SK-vs-MEAI   # compila con -warnaserror y corre --fake --verificar
tools/verificar.sh build                              # todo Examples/, con estas pruebas incluidas
```

A mano, en contenedor:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/14-Anotaciones-SK-vs-MEAI \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test && dotnet run -- --verificar'
```

El proyecto es a la vez consola y pruebas (`GenerateProgramFile=false`). Argumentos: `--verificar` (sale con 1 si falla algún invariante), `--plazo-s N` y `--fake`, que se acepta por contrato y no cambia nada: no hay modelo que reemplazar.

## Qué observar

```text
M.E.AI · nombre para el modelo «contar_localidades» · esquema {"type":"object","properties":{"provincia":{…,"type":"string"}},"required":["provincia"]}
SK     · plugin «Localidades», función «contar_localidades» · como AIFunction se llama «Localidades_contar_localidades» · esquema {"type":"object","condition":"Cuenta cuántas localidades…","properties":{…},"required":["provincia"]}
e01: provincia del catálogo con tres localidades en la sesión
  firma igual: True · esquema del parámetro igual: True
  M.E.AI → {"provincia":"Corrientes","cantidad":3}
  SK     → {"provincia":"Corrientes","cantidad":3,"error":null}
  normalizados iguales: True
  oráculo anotaciones-coinciden: pasa
...
e04: negativo: una localidad en lugar de una provincia; las dos devuelven el error, no un 0
  M.E.AI → {"provincia":"Goya","error":"La provincia «Goya» no está en el catálogo (…)."}
  SK     → {"provincia":"Goya","cantidad":null,"error":"La provincia «Goya» no está en el catálogo (…)."}
  oráculo anotaciones-coinciden: pasa
registro: 4 renglones en 14-Anotaciones-SK-vs-MEAI.jsonl
invariantes de código: se cumplen
```

- La **firma** (lo que decide si el modelo elige bien la herramienta) es igual en las dos: migrar no cambia el esquema.
- El **nombre como `AIFunction`** de la versión SK y la clave **`condition`** son las dos diferencias que sobreviven; las dos están en las dos primeras líneas.
- En **e04** las dos responden el error del catálogo, no un `0`: la regla es del caso de uso, no de la anotación.

Invariantes de código que comprueba `--verificar`, por entrada: las dos firmas son iguales; la firma es `contar_localidades(provincia)`; los resultados normalizados son iguales; y coinciden con lo esperado (cantidad, y si hubo error o no).

## Entradas, oráculo y registro

| Archivo | Qué es |
|---|---|
| `entradas.v1.json` | 4 entradas, «escrito para el ejemplo»: cada una es la `FunctionCallContent` que mandaría el modelo, escrita a mano. Provincias de `Catalogos`; las localidades de la sesión en memoria son las dos que siembra `SessionSeeder` (Corrientes, Resistencia) y Goya y Bella Vista con sus valores del caso base (`LocalidadesTests.cs` l. 50 y l. 102 @ `8fb4c3a`). e04 es un negativo declarado («Goya» como provincia). SHA-256 congelado en `ejemplo.json`. Estímulo del mismo autor; sin tasa (no hay modelo). |
| `../../tools/oraculos/anotaciones-coinciden.jq` | Oráculo. Recalcula, leyendo sólo el registro, lo mismo que `Corrida.Invariantes`, sin compartir código con él. |

Sin modelo no hay renglones `llamada`: el registro tiene un renglón `entrada` por entrada, y lo comparado viaja entero en su `motivo`, como JSON. Una entrada que el plazo deja afuera queda `excluida` con motivo `plazo`.

Pruebas (`Pruebas/AnotacionesTests.cs`, 13, todas sin servidor): las cuatro entradas por las dos definiciones; los metadatos (nombre, descripción y parámetro) coinciden y valen lo que dice la `[Description]`; el `CancellationToken` no llega al esquema en ninguna de las dos; el esquema del parámetro es el mismo JSON; el plugin agrupa la función; el nombre calificado de la `KernelFunction` como `AIFunction`; la clave `condition`; los crudos difieren en los `null` y normalizados son iguales; `Invariantes` detecta una descripción, una obligatoriedad o un resultado distintos; y el cableado del estímulo (toda provincia de una entrada no negativa está en `Catalogos`, y la negativa declarada no).

Pruebas de las trampas (`Pruebas/TrampasDeMigracionTests.cs`, 4, sin servidor, `Category("Cableado")`): sin `UseKernelFunctionInvocation` no se ejecuta ni falla, y su control con `UseKernelFunctionInvocation`; `AIFunctionFactory` no lee `[KernelFunction]`; la `KernelFunction` en un agente de MAF da `Error: Function failed.`, y con errores detallados, el motivo. El «modelo» guionado está en `Pruebas/ClienteGuionado.cs`. En total, 17 pruebas.

## Evidencia

Corrida por el arnés, pruebas, la variante rota (`FunctionCallContent` sin calificar → CS0104) y el recálculo del oráculo, con una prueba negativa, en `14-Anotaciones-SK-vs-MEAI/`; el registro, en `14-Anotaciones-SK-vs-MEAI.jsonl` de la carpeta R-LLM (registro de trabajo de la edición, que no se publica; procedencia en el Anexo H de la guía).

## El código, paso a paso

Los fragmentos son los que el libro comenta en §10.7; `// …` marca un recorte. La salida es del modo sin modelo: el ejemplo no llama a ningún `IChatClient`.

`Examples/14-Anotaciones-SK-vs-MEAI/Infrastructure/AI/PluginSk.cs`
```csharp
public sealed class PluginSk(LocalidadService localidades)
{
    public const string NombreDelPlugin = "Localidades";

    [KernelFunction("contar_localidades")]
    [Description("Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.")]
    public Task<ConteoDto> ContarLocalidadesAsync(
        [Description("Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.")] string provincia,
        CancellationToken cancellationToken = default) =>
        ConteoDeLocalidades.ContarAsync(localidades, provincia, cancellationToken);

    // …
    public KernelPlugin Crear() => KernelPluginFactory.CreateFromObject(this, NombreDelPlugin);
}
```

`Examples/14-Anotaciones-SK-vs-MEAI/Infrastructure/AI/HerramientaMeai.cs`
```csharp
public sealed class HerramientaMeai(LocalidadService localidades)
{
    public const string Nombre = "contar_localidades";

    [Description("Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.")]
    public Task<ConteoDto> ContarLocalidadesAsync(
        [Description("Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.")] string provincia,
        CancellationToken cancellationToken = default) =>
        ConteoDeLocalidades.ContarAsync(localidades, provincia, cancellationToken);

    // …
    public AIFunction Crear() => AIFunctionFactory.Create(ContarLocalidadesAsync, Nombre);
}
```

Qué mirar: las dos `[Description]` son idénticas y las dos delegan en `ConteoDeLocalidades`; lo único que cambia es `[KernelFunction]` y la fábrica de la última línea.

Salida sin modelo, 2026-10-27:

```text
M.E.AI · nombre para el modelo «contar_localidades» · esquema {"type":"object","properties":{"provincia":{"description":"Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.","type":"string"}},"required":["provincia"]}
SK     · plugin «Localidades», función «contar_localidades» · como AIFunction se llama «Localidades_contar_localidades» · esquema {"type":"object","condition":"Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.","properties":{"provincia":{"description":"Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.","type":"string"}},"required":["provincia"]}
e01: provincia del catálogo con tres localidades en la sesión
  firma igual: True · esquema del parámetro igual: True
  M.E.AI → {"provincia":"Corrientes","cantidad":3}
  SK     → {"provincia":"Corrientes","cantidad":3,"error":null}
  normalizados iguales: True
  oráculo anotaciones-coinciden: pasa
[…]
e04: negativo: una localidad en lugar de una provincia; las dos devuelven el error, no un 0
  firma igual: True · esquema del parámetro igual: True
  M.E.AI → {"provincia":"Goya","error":"La provincia «Goya» no está en el catálogo (Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe)."}
  SK     → {"provincia":"Goya","cantidad":null,"error":"La provincia «Goya» no está en el catálogo (Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe)."}
  normalizados iguales: True
  oráculo anotaciones-coinciden: pasa
```

