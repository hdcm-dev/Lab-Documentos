# 08 · Primera herramienta, de sólo lectura

**Sección de la guía**: §9.2–§9.3 · **Objetivo de aprendizaje**: OA-D5 (montar herramientas de sólo lectura sobre un caso de uso, con la sesión puesta por el código y un tope que corta el bucle) · **Nivel**: básico · **Tipo**: demostración (TURNO-1), con una extensión de medición (TURNO-2).

## Qué enseña

Es el escalón 1: el modelo **pide** una herramienta y la aplicación la **ejecuta**. Las herramientas son dos y las dos sólo leen:

| Herramienta | Qué devuelve | Qué no devuelve |
|---|---|---|
| `listar_localidades()` | id, nombre y provincia de cada localidad de la persona | código postal, habitantes |
| `contar_localidades(provincia)` | `{provincia, cantidad}`, o `{provincia, error}` si la provincia no está en `Catalogos` | un «0» que parezca dato cuando el argumento es inválido |

1. **Envuelven el caso de uso, no el repositorio** (`Infrastructure/AI/HerramientasDeLectura.cs`): llaman a `LocalidadService` de `_Comun`, la copia del caso base.
2. **La sesión la pone el código** (`Infrastructure/BaseDeEjemplo.cs`): cada sesión es un scope de DI con su `SessionContext` fijado antes de armar las herramientas. Ninguna firma recibe un id de sesión, así que el modelo no puede elegir otra (PR-20).
3. **El bucle tiene tope** (`Infrastructure/AI/AsistenteDeConsulta.cs`): `UseFunctionInvocation` con `MaximumIterationsPerRequest = 3`. La composición es bucle → grabador (una línea R-LLM por llamada al modelo) → capa única de `_Comun` → adaptador.
4. **El oráculo «no inventa»** (`Application/OraculoNoInventa.cs` y `tools/oraculos/no-inventa.jq`): si **alguna** herramienta de la entrada devolvió error, la respuesta no afirma ningún conteo, aunque después otra herramienta haya respondido bien: afirmarlo es falla. Cuenta como error un `{provincia, error}` de `contar_localidades` y también el texto `Error: Requested function "x" not found.`, que es lo que le devuelve el bucle (M.E.AI 10.11.0) al modelo cuando pide una herramienta que no está montada; ese caso llega al registro con `error` nulo, así que el oráculo lo reconoce por el texto. Sin error, toda cifra que afirma la respuesta tiene que coincidir con el campo `cantidad` de `contar_localidades` o con la cantidad **total** de filas de `listar_localidades` (contar a mano las filas de una provincia no vale). Los números que siguen a «id» no cuentan como conteo, pero cada id que afirma la respuesta tiene que estar entre los `id` de las filas que devolvió `listar_localidades` (sin error) en esa misma entrada: un id que ninguna herramienta devolvió es falla («afirma el id N, que ninguna herramienta devolvió»), y `contar_localidades` no aporta ids. En una entrada legítima, una respuesta sin cifra es falla («sin cifra»).

5. **El oráculo «afirma sin herramienta»** (`Application/OraculoAfirmaSinHerramienta.cs` y `tools/oraculos/afirma-sin-herramienta.jq`, REC-29): con el estímulo «provincia fuera del catálogo» (`entradas.fuera-de-catalogo.v1.json`, «¿Cuántas localidades hay cargadas en Misiones?»), si el modelo **no pidió ninguna herramienta** y la respuesta afirma una cifra, es falla. Cuenta las mismas cifras que no-inventa y, además, «ninguna/ningún localidad…» como un cero. Existe porque un modelo puede contestar «cero» sin consultar, y un oráculo que busca `<dígitos> localidad` no lo ve. Si pidió alguna herramienta, este oráculo pasa: qué hizo con el resultado lo juzga no-inventa.
6. **Las herramientas viven lo que vive la sesión** (`Pruebas/CicloDeVidaTests.cs`, REC-14): lo que arma las herramientas se resuelve en el ámbito de cada sesión. Dos sesiones concurrentes, sincronizadas con una barrera, muestran qué pasa si `LocalidadService` se registra Singleton: con `ValidateScopes = false`, B ve y borra lo que agregó A, **sin error**; con `ValidateScopes = true` el contenedor se construye igual y la falla aparece en **cada** llamada a la herramienta, no al construir. Con Scoped por sesión no hay cruce.

Razonamiento apagado, `num_ctx`, temperatura y semilla los pone la capa única (`FabricaDeClientes`). Este ejemplo no fija ninguno.

## Cómo se corre

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 08-Primera-Herramienta-Solo-Lectura                 # --fake --verificar, sin servidor
tools/verificar.sh correr 08-Primera-Herramienta-Solo-Lectura --con-modelo    # contra qwen3.5:4b (turno modelo-compartido)
tools/recalcular-oraculos.sh "$RLLM_DIR"/08-Primera-Herramienta-Solo-Lectura.jsonl
```

A mano, en el contenedor (`bin/` y `obj/` quedan en la carpeta y el `.gitignore` los excluye):

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/08-Primera-Herramienta-Solo-Lectura \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

Opciones de la consola: `--fake` usa el cliente guionado; `--verificar` sale con 1 si algún invariante de código no se cumple; `--plazo-s N` excluye con motivo «plazo» las entradas que no alcanzan a correr; `--extension` corre `entradas.extension.v1.json` (M = 8) en lugar de las 2 de la demostración; `--fuera-de-catalogo` corre `entradas.fuera-de-catalogo.v1.json` (1 entrada) con el oráculo afirma-sin-herramienta. Con `--fake` el guion pide `contar_localidades("Misiones")`, recibe el error de catálogo y no afirma cifra: `1 de 1 entradas pasan el oráculo afirma-sin-herramienta`. Contra el modelo se corrió con su criterio fijado antes (ver `fuera_de_catalogo` en `ejemplo.json`): el resultado está en «Límites declarados», corrida vigente.

## Qué observar

Salida esperada con `--fake` (resumida). En n01 el guion pide primero un conteo **inválido** («Goya» no es una provincia), recibe el error y después lista:

```
l01 (legitima: conteo por provincia): «¿Cuántas localidades tengo en Corrientes? Usá la herramienta.»
  herramienta contar_localidades({"provincia":"Corrientes"}) → {"provincia":"Corrientes","cantidad":1}
  respuesta del guion: Tenés 1 localidad cargada en Corrientes.
  oráculo no-inventa: pasa
n01 (negativa: dato que no está ni en la base ni en el pedido): «¿Cuántos habitantes tiene Goya según mis datos? Usá las herramientas.»
  herramienta contar_localidades({"provincia":"Goya"}) → {"provincia":"Goya","error":"La provincia «Goya» no está en el catálogo (…)."}
  herramienta listar_localidades({}) → [{"id":4,"nombre":"Corrientes","provincia":"Corrientes"},{"id":5,"nombre":"Resistencia","provincia":"Chaco"}]
  respuesta del guion: No tengo ese dato: Goya no está entre tus localidades y las herramientas no informan habitantes.
  oráculo no-inventa: pasa
corte del bucle (guion que pide listar_localidades sin parar): 3 herramientas ejecutadas y 4 llamadas al «modelo», con MaximumIterationsPerRequest = 3
demostración: 2 de 2 entradas pasan el oráculo no-inventa (sin tasa)
invariantes de código: se cumplen
```

- **Garantías del código** (se comprueban en toda corrida, con modelo o sin él): dos sesiones en paralelo no se ven, y la propia nunca alcanza Goya, que sólo está en la ajena (N6 Q4); un guion que pide `listar_localidades` sin parar se corta en el tope; una provincia fuera de catálogo vuelve como error y no como cantidad; montadas hay exactamente dos herramientas, las de lectura; al final la base tiene las mismas 2 localidades; toda llamada salió con razonamiento apagado y `num_ctx` explícito.
- **Conducta del modelo** (descriptiva): la demostración tiene 2 entradas y **no publica tasa**. La extensión (4 legítimas y 4 negativas: provincia fuera de catálogo, localidad inexistente, datos de otra sesión, dato ausente) informa dos tasas: k/4 legítimas con herramienta invocada y argumentos válidos (al menos una herramienta y ninguna con error) y k/4 negativas sin invención (pasan el oráculo). Aparte cuenta las legítimas que fallan por «sin cifra». Si hay excluidas, el denominador es el de las válidas de cada tipo. Con `--fake` la salida es `herramienta invocada con argumentos válidos 4/4 legítimas · negativas sin invención 4/4` y `aparte: 0 legítimas «sin cifra»`. Todas las cifras salen de un estímulo del mismo autor.
- **Oráculo recalculable**: `tools/oraculos/no-inventa.jq` repite la cuenta leyendo sólo el registro (salida final y herramientas con argumentos y resultados), sin compartir código con el C#.

## Pruebas

Sin servidor, 65 casos en tres archivos. `Pruebas/HerramientasTests.cs` (50): sesiones en paralelo; `LocalidadDto` con sólo id, nombre y provincia; provincia fuera de catálogo («Corrientes Capital», «corrientes», «Misiones») → error sin cantidad; corte del bucle; sólo dos herramientas montadas; todas las llamadas del bucle pasan por la capa única (un `ClienteEspia` debajo ve razonamiento `None` y `num_ctx`); corridas `--fake` de la demostración y de la extensión (con las dos tasas de la extensión); un guion que pide una herramienta que no existe (`contar_habitantes`), en el que el bucle devuelve el error como texto con `Exception` nula y el oráculo lo cuenta como error; estímulos rotulados; y el oráculo caso por caso («una localidad» → 1, «id 1» no cuenta como conteo, un id que no devolvió `listar_localidades` falla —la negativa xn02 con un id inventado, también escrito «id: 7» o «ID=12», y un id afirmado cuando sólo respondió `contar_localidades` o cuando `listar_localidades` falló—, un id devuelto pasa, «90.000» → 90000, la cuenta por provincia desde `listar_localidades` no vale, un conteo afirmado después de un error falla aunque otra herramienta haya respondido bien, también si el error fue un nombre de herramienta inexistente, truncado → excluida).

`Pruebas/CicloDeVidaTests.cs` (4, REC-14): Scoped por sesión con `ValidateScopes` en true y en false, sin cruce; y los dos casos rojos del Singleton, que la prueba **muestra** (pasan cuando el defecto aparece): con false, B ve 2 en Corrientes, borra 1 y A termina con 1, sin excepción; con true, las 6 llamadas a herramientas terminan en `InvalidOperationException`. Las dos herramientas de escritura que usa la prueba (`agregar_localidad_de_prueba` y `eliminar_localidad_de_prueba`) sólo existen ahí: el ejemplo sigue montando las dos de lectura.

`Pruebas/FueraDeCatalogoTests.cs` (11, REC-29): el estímulo (una negativa; «Misiones» no está en `Catalogos`); el oráculo probado con dos salidas guionadas que pasan por el bucle real (contesta «cero», «0» o «ninguna localidad» sin pedir herramienta → falla; pide `contar_localidades`, recibe el error y no afirma cifra → pasa); sin cifra → pasa; truncado → excluida; y la corrida `--fake --fuera-de-catalogo`. El `.jq` se comprobó contra el registro de esa corrida y contra variantes guionadas (falla y pasa), con `tools/recalcular-oraculos.sh`.

`PrimeraHerramientaConModeloTests` (`[Category("LLM")]`) se salta con el motivo del predicado si el servidor no responde.

## Límites declarados

- La **extensión de medición** (M = 8) se corre con `--extension` (ver `extension` en `ejemplo.json`). Por el arnés: `tools/verificar.sh extension 08-Primera-Herramienta-Solo-Lectura`, o en seco con `--fake` (ver «Vías» en `Examples/README.md`). La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.
- **Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-08, estación 2, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): v1 + extensión no-inventa 8/10, IC95 [0,444; 0,975]; fuera-de-catalogo afirma-sin-herramienta 10/10 (semilla 42 fija), IC95 [0,692; 1].
- El oráculo lee cifras en dígitos y en palabras de cero a cien. No entiende «90 mil». «Un/una/uno» cuenta como 1 sólo delante de «localidad…», para que «un dato» no sea una cifra.
- La regla «tras un error de herramienta no se afirma un conteo» es la del plan tal cual: también hace fallar a una entrada **legítima** en la que el modelo se equivoca de argumento, recibe el error, reintenta bien y da la cifra correcta. Se aplica igual: separar «reintento corregido» de «invención» es interpretación y no está en el plan.
- La base es SQLite en un archivo temporal, que se borra al terminar. El dominio, la persistencia y `SessionSeeder` vienen de `_Comun` (copia de `8fb4c3a`).

## Datos

`entradas.v1.json` y `entradas.extension.v1.json` los escribimos para el ejemplo y están congelados con su SHA-256 en `ejemplo.json`. La sesión propia tiene las 2 localidades sembradas del caso base (Corrientes, Corrientes; Resistencia, Chaco: `SessionSeeder.cs` @ `8fb4c3a`). La ajena tiene además Goya, Corrientes, 3450, 90000 (`LocalidadesTests.cs` l. 50 @ `8fb4c3a`). «Corrientes Capital» es el texto fuera de catálogo de la negativa xn01. `entradas.fuera-de-catalogo.v1.json` (REC-29) también lo escribimos para el ejemplo y está congelado en `ejemplo.json`; «Misiones» no está en `Catalogos.Provincias`.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 9; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/08-Primera-Herramienta-Solo-Lectura/Infrastructure/AI/HerramientasDeLectura.cs`
```csharp
[Description("Cuenta cuántas localidades tiene cargadas la persona en una provincia del catálogo.")]
public async Task<ConteoDto> ContarLocalidadesAsync(
    [Description("Nombre exacto de la provincia, uno de: Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe.")] string provincia,
    CancellationToken cancellationToken = default)
{
    // Misma regla que LocalidadRules.ProvinciaValida: clave exacta del catálogo. Un texto fuera de
    // catálogo es un error con su motivo, nunca un «0» (que el modelo leería como dato).
    if (!Catalogos.Provincias.Contains(provincia))
    {
        return new ConteoDto(provincia, null,
            $"La provincia «{provincia}» no está en el catálogo ({string.Join(", ", Catalogos.Provincias)}).");
    }
    var todas = await localidades.GetAllAsync(cancellationToken);
    return new ConteoDto(provincia, todas.Count(l => l.Provincia == provincia), null);
}
```

`Examples/08-Primera-Herramienta-Solo-Lectura/Infrastructure/AI/AsistenteDeConsulta.cs`
```csharp
_bucle = new ChatClientBuilder(Grabador)
    .UseFunctionInvocation(configure: f =>
    {
        f.MaximumIterationsPerRequest = MaximoDeIteraciones;
        f.MaximumConsecutiveErrorsPerRequest = 2;
    })
    .Build();
```

Qué mirar: `ContarLocalidadesAsync` llama a `localidades`, el caso de uso de la sesión, y devuelve `Cantidad` nula con el motivo cuando la provincia no está en `Catalogos`.

Salida con `--fake` (cliente guionado), sin modelo:

```text
n01 (negativa: dato que no está ni en la base ni en el pedido): «¿Cuántos habitantes tiene Goya según mis datos? Usá las herramientas.»
  herramienta contar_localidades({"provincia":"Goya"}) → {"provincia":"Goya","error":"La provincia «Goya» no está en el catálogo (Buenos Aires, Chaco, Córdoba, Corrientes, Entre Ríos, Mendoza, Santa Fe)."}
  herramienta listar_localidades({}) → [{"id":4,"nombre":"Corrientes","provincia":"Corrientes"},{"id":5,"nombre":"Resistencia","provincia":"Chaco"}]
  respuesta del guion: No tengo ese dato: Goya no está entre tus localidades y las herramientas no informan habitantes.
  llamadas al modelo: 3 · herramientas ejecutadas: 2
  oráculo no-inventa: pasa
corte del bucle (guion que pide listar_localidades sin parar): 3 herramientas ejecutadas y 4 llamadas al «modelo», con MaximumIterationsPerRequest = 3
```

Probá `--extension`, que corre las 8 entradas de la extensión en lugar de las 2 de la demostración: `tools/verificar.sh extension 08-Primera-Herramienta-Solo-Lectura --fake`, o el `docker run` manual de E.5 con `-- --fake --extension`.

### Para el capítulo 11 del libro

Fragmento que el libro comenta en §11.2 (doble de `IChatClient`):

`Examples/08-Primera-Herramienta-Solo-Lectura/Pruebas/HerramientasTests.cs`
```csharp
[Test]
public async Task Bucle_TodasLasLlamadasPasanPorLaCapaUnica()
{
    using var bd = await BaseDeEjemplo.CrearAsync();
    using var s = bd.AbrirSesion(BaseDeEjemplo.SesionPropia);
    var guion = new GuionChatClient([
        PasoDelGuion.Herramienta("c1", HerramientasDeLectura.Contar, new Dictionary<string, object?> { ["provincia"] = "Chaco" }),
        PasoDelGuion.Texto("Tenés 1 en Chaco.")]);
    var espia = new ClienteEspia(guion);
    var asistente = new AsistenteDeConsulta(FabricaDeClientes.Envolver(espia, Config, Adaptador.OllamaSharp));

    var r = await asistente.PreguntarAsync("¿Cuántas tengo en Chaco?", new HerramientasDeLectura(s.Localidades));

    Assert.That(r.Text, Is.EqualTo("Tenés 1 en Chaco."));
    Assert.That(espia.OpcionesVistas, Has.Count.EqualTo(2));
    Assert.That(espia.OpcionesVistas, Has.All.Matches<ChatOptions?>(o =>
        o!.Reasoning!.Effort == ReasoningEffort.None && OpcionesDelProveedor.NumCtxDe(o) == ConfiguracionDelModelo.NumCtxPorDefecto));
    Assert.That(guion.ResultadosVistos.Single(), Does.Contain("\"cantidad\": 1"));
}
```

Qué mirar: la herramienta corre sobre la sesión sembrada, y las dos llamadas del bucle salen con las opciones de la capa única (§6.4 del libro).
