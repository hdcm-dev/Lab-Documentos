# 13 · Servidor y cliente MCP

**Sección de la guía**: §9.9 · **Objetivo de aprendizaje**: OA-D8 (exponer lecturas por MCP en loopback y consumir herramientas MCP como `AIFunction`) · **Nivel**: intermedio · **Tipo**: demostración de 1 pregunta (TURNO-2).

## Qué enseña

MCP sirve para que **otro proceso** use las herramientas de la aplicación. Si la herramienta la usa sólo tu app, alcanza con un `AIFunction` local (ejemplo 08). Este ejemplo pone las dos puntas en el mismo proceso: el servidor publica y el cliente consume.

**Servidor** (`Infrastructure/Mcp/`): es un **adaptador de entrada**, como un controller. Traduce el protocolo y llama al caso de uso.

1. **Sólo lecturas.** Publica una herramienta, `listar_localidades` (`[McpServerToolType]` / `[McpServerTool]`, con `ReadOnly = true`), sobre `LocalidadService` de `_Comun`. Devuelve id, nombre y provincia.
2. **Sesión fija `demo-mcp`, puesta por el código.** Ninguna firma recibe un id de sesión, así que quien llama por MCP no puede pedir otra. La base tiene además una sesión que hace de «sesión de la API», con Goya, y por MCP Goya no aparece nunca (H-014).
3. **Sólo loopback.** Kestrel escucha en `IPAddress.Loopback`, en un puerto que elige el sistema. `MapMcp("/mcp")` usa el transporte HTTP en modo `Stateless`.
4. **Host y `Origin`.** `AllowedHosts` admite sólo `localhost`, `127.0.0.1` y `[::1]`: con otro `Host`, `HostFiltering` responde 400. Un filtro responde 403 a todo `Origin` que no sea loopback (`null` incluido), porque la especificación MCP 2026-07-28 (Streamable HTTP) obliga al servidor a validar `Origin`. Un pedido sin `Origin`, de un cliente que no es navegador, pasa.

**Ninguna de las tres cosas es control de acceso.** El servidor no autentica a nadie: cualquier proceso de la máquina lo puede usar. Lo que lo hace aceptable es que sólo publica una lectura, y en una sesión de demostración (H-013).

**Cliente** (`Infrastructure/AI/ClienteMcp.cs`): `McpClient.ListToolsAsync` devuelve `McpClientTool`, que ya es un `AIFunction` y se puede poner tal cual en `ChatOptions.Tools`. Pero todo lo que manda el servidor es **no confiable** (MCP 2026-07-28, Tools): los nombres, las descripciones, las anotaciones y, después, los resultados. Por eso:

- el cliente monta **sólo** lo que está en su lista permitida por nombre (`listar_localidades`) y descarta el resto;
- la descripción que ve el modelo es la que escribe el cliente (`WithDescription`), no la que mandó el servidor, en las lecturas y también en las escrituras;
- las anotaciones (`ReadOnly`, `Destructive`) no deciden nada: el servidor guionado de las pruebas anuncia `borrar_todo` marcada `ReadOnly = true` y no destructiva, un filtro por anotación la dejaría pasar, y la lista por nombre no la monta;
- si alguna vez se permite una escritura, se monta con la descripción del cliente y envuelta en `ApprovalRequiredAIFunction` (`PoliticaDeMontaje.Escrituras`, nombre → descripción; hoy está vacía).

Al conectar, el cliente lee la versión del protocolo que quedó negociada con el servidor (`McpClient.NegotiatedProtocolVersion`, la respuesta a `initialize`) y la escribe en la salida y en el registro. Es lo que se usó en esa corrida, no una inferencia a partir de las versiones que trae el SDK.

El asistente (`AsistenteConMcp`) es el mismo escalón 1 de §8.2, con `UseFunctionInvocation` y un tope de 3 iteraciones: la herramienta es remota y ejecutarla es un pedido MCP. El razonamiento apagado, `num_ctx`, la temperatura y la semilla los pone la capa única (`FabricaDeClientes`). Este ejemplo no fija ninguno.

## Cómo se corre

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh correr 13-Servidor-Y-Cliente-MCP                 # --fake --verificar, sin servidor de modelos
tools/verificar.sh correr 13-Servidor-Y-Cliente-MCP --con-modelo    # contra qwen3.5:4b (turno modelo-compartido)
tools/recalcular-oraculos.sh "$RLLM_DIR"/13-Servidor-Y-Cliente-MCP.jsonl
```

A mano, en el contenedor (`bin/` y `obj/` quedan en la carpeta y el `.gitignore` los excluye):

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/13-Servidor-Y-Cliente-MCP \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet test --filter "Category!=LLM" && dotnet run -- --fake --verificar'
```

Opciones de la consola: `--fake` usa el cliente guionado, pero el servidor MCP y la llamada a la herramienta son reales; `--verificar` sale con 1 si algún invariante de código no se cumple; `--plazo-s N` excluye con motivo «plazo» las entradas que no llegan a correr.

## Qué observar

Salida esperada con `--fake` (el puerto cambia en cada corrida):

```
servidor MCP en loopback (puerto 35533); anuncia: listar_localidades
protocolo MCP negociado: 2026-07-28
cliente: monta listar_localidades; descarta nada
m01 (lectura por MCP en la sesión fija demo-mcp): «¿Qué localidades tengo cargadas? Usá la herramienta.»
  herramienta MCP listar_localidades({}) → {"$type":"text","Text":"[{\"id\":1,\"nombre\":\"Corrientes\",\"provincia\":\"Corrientes\"},{\"id\":2,\"nombre\":\"Resistencia\",\"provincia\":\"Chaco\"}]",…}
  respuesta del guion: Tenés 2 localidades cargadas: Corrientes (Corrientes) y Resistencia (Chaco).
  llamadas al modelo: 2 · herramientas ejecutadas: 1
  oráculo mcp-sesion-fija: pasa
servidor guionado (anuncia borrar_todo con ReadOnly = true): el cliente descarta borrar_todo; borrar_todo no se ejecutó
Host ajeno → 400 · Origin ajeno → 403 · Origin «null» → 403 · Origin de loopback → 200
demostración: 1 de 1 entradas pasan el oráculo mcp-sesion-fija (sin tasa)
invariantes de código: se cumplen
```

- **El resultado de la herramienta MCP llega como texto.** El servidor serializa la lista a JSON, el cliente lo recibe como un bloque de texto, y eso es lo que lee el modelo. Es contenido que esta app no escribió: la regla de origen de §F.4 lo trata como no confiable.
- **Garantías del código** (se comprueban en toda corrida, con modelo o sin él):
  - el servidor escucha sólo en loopback;
  - el cliente tiene una versión de protocolo negociada, y queda en la salida y en el registro (una línea `tipo: "mcp-negociacion"` con `protocolo_negociado`, antes de las líneas R-LLM; las herramientas que leen el registro filtran por `tipo` y no la cuentan);
  - anuncia sólo `listar_localidades`, marcada de lectura;
  - por MCP vuelven Corrientes y Resistencia, de `demo-mcp`, y nunca Goya, que la sesión de la API sí tiene;
  - el servidor guionado anuncia `borrar_todo`: el cliente no la monta, y aunque el «modelo» la pida, no se ejecuta (contador en 0);
  - la descripción montada es la del cliente, en las lecturas y en las escrituras; una escritura montada va con aprobación, y nada se monta fuera de la política;
  - `Host` ajeno da 400; `Origin` ajeno o `null`, 403; `Origin` de loopback pasa;
  - toda llamada al modelo sale con razonamiento apagado y `num_ctx` explícito.
- **Conducta del modelo**: es una sola pregunta y **no publica tasa**. El oráculo `mcp-sesion-fija` (`Application/OraculoSesionFija.cs`, y `tools/oraculos/mcp-sesion-fija.jq`, que lo recalcula leyendo sólo el registro) exige seis cosas:
  1. con `truncado_posible`, la entrada se excluye;
  2. toda herramienta invocada está en la lista permitida;
  3. `listar_localidades` respondió sin error;
  4. ningún resultado trae Goya;
  5. la respuesta no nombra Goya;
  6. la respuesta nombra Corrientes y Resistencia.
- **Corrida contra el modelo**: ver «Corrida» abajo.

## Pruebas

`Pruebas/McpTests.cs`, sin servidor de modelos (28 casos; el servidor MCP corre en el proceso de la prueba, en loopback):

- `Mcp_HostYOrigenAjenos_Rechaza`: `Host` ajeno → 400; `Origin` ajeno → 403;
- el servidor escucha sólo en loopback;
- `OrigenPermitido`, caso por caso: sin `Origin`, `localhost`, `127.0.0.1` y `[::1]` pasan; `ajeno.example`, `localhost.ajeno.example`, `null` y `file://` no pasan;
- el listado tiene sólo lecturas y la sesión fija no ve la de la API;
- el servidor guionado con `borrar_todo`: no se monta y no se ejecuta;
- la descripción montada es la del cliente, y una escritura permitida se monta como `ApprovalRequiredAIFunction`;
- `Montaje_UnaEscrituraPermitidaLlevaLaDescripcionDelCliente_YLaGarantiaLaCubre`: la escritura permitida llega al modelo con la descripción del cliente, y la garantía extendida detecta una escritura montada con la descripción del servidor, una sin aprobación y una fuera de la política;
- `HerramientaPeligrosaMarcadaReadOnly_NoDecideLaListaBlanca`: `borrar_todo` viene anunciada de lectura y no destructiva, un filtro por anotación la aceptaría y la lista por nombre la descarta;
- `ToolsList_EsElEsquemaQueRecibeElBucle`: lo que llega al adaptador dentro del bucle tiene el nombre y el esquema de parámetros de `tools/list`, con la descripción del cliente;
- `Conexion_RegistraLaVersionNegociada`: el cliente tiene una versión negociada con forma de fecha;
- la llamada a la herramienta MCP dentro del bucle pasa por la capa única (un `ClienteEspia` debajo ve razonamiento `None` y `num_ctx`);
- la corrida `--fake` completa, con la línea de la negociación en el registro;
- las entradas están rotuladas;
- el oráculo, caso por caso.

`McpConModeloTests` (`[Category("LLM")]`) se salta con el motivo del predicado si el servidor de modelos no responde.

## Corrida

Es **una sola pregunta, sin tasa**: muestra el cableado de punta a punta con un modelo real. No muestra que el modelo use la herramienta siempre. La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

**Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-13, estación 2, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): una entrada, existe; protocolo negociado 2026-07-28.

## Límites declarados

- **No hay control de acceso.** Exponer esto fuera de loopback, o publicar una escritura, pide autorización (la especificación MCP la define sobre OAuth) y la aprobación del cliente. Nada de eso se enseña acá (H-013).
- **Validar `Origin` no protege de un proceso local**, y `AllowedHosts` tampoco: los dos cortan el *rebinding* de DNS desde un navegador. Por eso no se los llama control de acceso.
- El servidor no valida los argumentos: `listar_localidades` no tiene.
- El oráculo compara nombres de localidad por inclusión de texto, en minúsculas. «Corrientes» es a la vez localidad y provincia, así que nombrar la provincia ya lo cumple. Resistencia, en cambio, sólo aparece si se la nombra.
- La demo (`Demos/`) no expone `/mcp`: la frontera escuchar / publicar / validar `Host` y `Origin` vive acá (plan §F.1, P3-09).

## Datos

`entradas.v1.json` es un estímulo escrito para el ejemplo y está congelado con su SHA-256 en `ejemplo.json`. La sesión `demo-mcp` tiene las 2 localidades sembradas del caso base: Corrientes, Corrientes, y Resistencia, Chaco (`SessionSeeder.cs` @ `8fb4c3a`). La sesión de la API tiene además Goya, Corrientes, 3450, 90000 (`LocalidadesTests.cs` l. 50 @ `8fb4c3a`). `ajeno.example` es un dominio reservado para ejemplos. No es una dirección real.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 9; `// …` marca un recorte. Las salidas son del modo `--fake` (cliente guionado o generador estable), sin modelo.

`Examples/13-Servidor-Y-Cliente-MCP/Infrastructure/AI/ClienteMcp.cs`
```csharp
public static Montaje Montar(IEnumerable<McpClientTool> anunciadas, PoliticaDeMontaje politica)
{
    var montadas = new List<AITool>();
    var descartadas = new List<string>();
    foreach (var herramienta in anunciadas)
    {
        if (politica.Lecturas.TryGetValue(herramienta.Name, out var descripcion))
        {
            // La descripción que ve el modelo es la del cliente, no la que mandó el servidor.
            montadas.Add(herramienta.WithDescription(descripcion));
        }
        else if (politica.Escrituras.TryGetValue(herramienta.Name, out var descripcionDeLaEscritura))
        {
            // También en una escritura: la descripción del cliente, y la aprobación por fuera.
            montadas.Add(new ApprovalRequiredAIFunction(herramienta.WithDescription(descripcionDeLaEscritura)));
        }
        else
        {
            descartadas.Add(herramienta.Name);
        }
    }
    return new Montaje(montadas, descartadas);
}
```

Qué mirar: el `else` final descarta todo lo que el cliente no permitió por nombre, diga lo que diga el servidor de sí mismo. Las dos ramas permitidas reemplazan la descripción; la de escrituras, además, la envuelve en la aprobación.

Con `--fake`, la herramienta y el servidor MCP son reales y sólo la respuesta del modelo es del guion. El resultado de la herramienta llega como texto: es contenido que la aplicación no escribió.

### Para el capítulo 12 del libro

Fragmento que el libro comenta en §12.5 (exponer por MCP):

`Examples/13-Servidor-Y-Cliente-MCP/Infrastructure/Mcp/ServidorMcp.cs`
```csharp
builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
builder.Services.Configure<HostFilteringOptions>(o =>
{
    o.AllowedHosts = [.. HostsPermitidos];
    o.AllowEmptyHosts = false;
});
// …
app.UseHostFiltering();
app.Use(async (contexto, siguiente) =>
{
    if (!OrigenPermitido(contexto.Request.Headers.Origin.ToString()))
    {
        Interlocked.Increment(ref rechazos[0]);
        contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await siguiente(contexto);
});
app.MapMcp(Ruta);
```

Qué mirar: tres líneas de defensa (loopback, `Host`, `Origin`) y ninguna que pregunte quién llama.
