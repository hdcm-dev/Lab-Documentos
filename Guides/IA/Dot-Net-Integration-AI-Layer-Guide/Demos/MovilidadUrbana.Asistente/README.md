# Demo: Movilidad Urbana con asistente

Una aplicación .NET 10 que **ya existía** (la API de Movilidad Urbana del laboratorio Lab-E2E.WebBlazor, en el commit `8fb4c3a`) y a la que se le **suma** una capa de IA sin reescribir sus casos de uso. Contexto de uso: CTX-EXISTENTE (app existente), servidor de modelos local o compartido, prototipo.

- **Un solo host** (ADR-13): la API `/api/v1` del caso base y una página Blazor de chat, que es la **única** presentación del chat. La API no tiene ruta de chat.
- **Capas como carpetas de un ensamblado**, como el caso base (ADR-11); una prueba de arquitectura reemplaza lo que el compilador no impide.
- **La capa de IA es puertos y adaptadores**: los puertos (`IExtractorDeLocalidad`, `IExtractorDeEncuesta`, `IAyuda`) viven en `Application/`; los adaptadores, en `Infrastructure/AI/`.
- **Las herramientas son casos de uso** y toda escritura pide aprobación con un pedido que arma el código (ADR-06, ADR-12).
- Sin `/mcp`: la exposición por MCP, con la validación de Host y `Origin`, está en `Examples/13`.

Todo lo que es dato del caso (entidades, `Catalogos`, requisitos de `*Policy`, localidades sembradas) sale de `8fb4c3a`. Las entradas de prueba, la siembra de encuestas y los sinónimos del normalizador están **escritos para el ejemplo**: usan sólo valores del caso base y personas «Persona 01…20». Toda cifra que produzcan es un estímulo del mismo autor.

Objetivos de aprendizaje que ilustra: OA-0.2 (ubicar cada pieza en su capa), OA-D2 (PD-3), OA-D4 (PD-2), OA-D5 (PD-1, PD-5), OA-D6 (PD-6), OA-S4 (PD-6), OA-A5 (`IExtractorDeLocalidad` frente al chat), OA-A6 (la tabla de delta) y OA-O2 (todo por variable, nada en el repo).

## Recorrido: de la lectura a la escritura

| Paso | Escenario | Dónde se ve | Qué hay que mirar | Cómo cierra |
|---|---|---|---|---|
| **PD-1** | ESC-01 | Página de chat | «¿Cuántas localidades tengo en Corrientes?» y «¿Qué medio usa más la gente?»: el modelo pide `contar_localidades` o `resumir_encuesta`; la respuesta sale de `LocalidadService` y de `ResultadosDeEncuestaService`. El resumen son agregados: el tipo de retorno no tiene `Nombre` ni `Edad` | Cableado (`Pd1_*`) y el registro de los ejemplos 08 y 12 |
| **PD-2** | ESC-01 | `POST /api/v1/ayuda` | «¿Cuántos dígitos lleva el código postal?»: recupera 3 de los 12 requisitos de `*Policy`, el modelo propone respuesta y fuente, y **el código valida la cita** contra lo recuperado. Sin cita válida, muestra el requisito más cercano, literal. Sube más escalones de los necesarios: con 12 requisitos cortos alcanzaría el contexto fijo | Humo con modelo + el ejemplo 07 |
| **PD-3** | ESC-03 | `POST /api/v1/localidades/desde-texto` | «Agregá Bella Vista, en Corrientes, código postal 3432. No sé cuántos viven.»: `Habitantes` llega `null`, la regla lo rechaza y la respuesta pide el dato con el requisito de `LocalidadPolicy`. Si el modelo inventa un número que no está en el texto, el adaptador lo trata como ausente | Humo con modelo + el ejemplo 03 |
| **PD-4** | ESC-03 | `POST /api/v1/encuestas/desde-texto` | «Voy en bondi y a veces en bici…»: el normalizador lleva lo que dice la persona a claves de `Catalogos` (`colectivo`, `bicicleta`); «avion» no se convierte en nada: la regla endurecida lo rechaza | Cableado (`Pd4_*`) y el registro del ejemplo 04 |
| **PD-5** | ESC-03 / ESC-04 | Página de chat | «Agregá Goya, Corrientes, CP 34, con 90000 habitantes»: el dominio rechaza el CP antes de preguntar nada; con «3450», el pedido de aprobación muestra los datos que resolvió el código | Cableado (`Pd5_*`) y el registro del ejemplo 09 |
| **PD-6** | ESC-04 | Página de chat | «Borrá Resistencia»: `eliminar_localidad(nombre, provincia)` → el código resuelve la localidad **en tu sesión** → pedido de aprobación armado por el código («Eliminar la localidad «Resistencia» (Chaco), id N, de tu sesión.») → sin aprobación no se borra. Son **dos peticiones**: el pedido queda guardado en el servidor y la página sólo manda la decisión | Humo con modelo + el ejemplo 11 |

Cada humo con modelo prueba un **cruce de capas**, no un paso suelto: PD-3, controller → caso de uso → puerto → adaptador; PD-2, controller → puerto directo (excepción de lectura) + recuperación; PD-6, página → `IChatClient` → herramientas = casos de uso, con el estado del circuito. La conducta del modelo se mide en los ejemplos; acá se prueba la composición.

## Dónde vive cada cosa

```
src/MovilidadUrbana.Asistente/
  Domain/                 entidades, Catalogos, LocalidadRules, EncuestaRules (endurecidas). Sin nada de IA.
  Application/            casos de uso (LocalidadService, EncuestaService, ResultadosDeEncuestaService,
                          AltaDesdeTextoService, EncuestaDesdeTextoService) y puertos (Abstractions/).
  Infrastructure/
    Persistence/          EF Core SQLite, SessionSeeder (con la siembra de encuestas rotulada).
    Sessions/             SessionContext (scoped).
    AI/                   AddAI(config): adaptadores de los puertos, normalizador de catálogo,
                          herramientas por circuito, aprobación, almacén vectorial e ingesta de *Policy.
  Controllers/            /api/v1: localidades y encuestas (caso base), desde-texto y ayuda (nuevos).
  Components/             App, Routes, un layout mínimo y la página de chat (Pages/Chat.razor).
  Concurrencia/           LimitadorDelModelo: un solo límite de pedidos al modelo para la API y la página.
  Sessions/               SessionHeaderMiddleware (sólo /api) y SessionMiddleware (sólo documentos).
tests/MovilidadUrbana.Asistente.Tests/
  Cableado/               sin red, en cada build.
  ConModelo/              [Category("LLM")], se saltan si no hay servidor.
```

Las opciones propias del proveedor (razonamiento apagado, `num_ctx`, temperatura 0, semilla) se fijan en **una sola capa**, la de `Examples/_Comun/MovilidadUrbana.Asistente.Comun` (`FabricaDeClientes`), que la demo referencia: el cliente real, el guionado de las pruebas y el modo sin modelo pasan por ella. Nadie más crea un cliente de Ollama.

Límite de concurrencia: `LimitadorDelModelo` es un `ConcurrencyLimiter` singleton en la **entrada**, no en la cadena del `IChatClient`. Lo comparten los controllers que llaman al modelo (`AyudaController`, `DesdeTextoController`, política `modelo`: al saturarse, 503 con `Retry-After`) y la página de chat, que corre cada pedido con un permiso del mismo limitador y, si no lo obtiene, dice que el servidor está ocupado. El ABM del caso base no pasa por él. Los valores por defecto (1 permiso, cola de 2, reintentar en 30 s) son elegidos, no medidos.

Ciclo de vida (ADR-12): el `IChatClient` base es singleton. El `FunctionInvokingChatClient`, las herramientas, el historial y los pedidos pendientes son del **circuito** de Blazor (scoped): se arman en `SesionDeChat` con `AIFunctionFactory.Create(método, instancia)` sobre instancias del scope, sin parámetros que resuelva DI. Nunca hay estado de conversación en un singleton.

## Herramientas y aprobación

| Herramienta | Caso de uso | Control |
|---|---|---|
| `listar_localidades` | `LocalidadService.GetAllAsync` | lectura; devuelve id, nombre y provincia. El nombre lo escribió una persona: es **texto no confiable** |
| `contar_localidades(provincia)` | `LocalidadService.GetAllAsync` | lectura; la provincia se normaliza y se valida contra `Catalogos` |
| `resumir_encuesta` | `ResultadosDeEncuestaService.ResumirAsync` | lectura; sólo agregados |
| `agregar_localidad(...)` | `LocalidadService.SaveAsync` | `ApprovalRequiredAIFunction`; el dominio decide antes de preguntar |
| `eliminar_localidad(nombre, provincia)` | `LocalidadService.GetByNombreYProvinciaAsync` + `DeleteAsync` | `ApprovalRequiredAIFunction`; error si es ambiguo |

`EncuestaService.RegistrarAsync` no se expone como herramienta.

- **El pedido de aprobación lo arma el código**: `ResolutorDeAprobaciones` toma la llamada, la resuelve contra el caso de uso dentro de la sesión y muestra eso; la prosa del modelo no aparece. Si la localidad no es de tu sesión, se rechaza sin preguntar.
- **Lo que se ejecuta es lo que se mostró**: la respuesta de aprobación se arma en el servidor con `CreateResponse` sobre el pedido guardado; una respuesta con argumentos alterados se rechaza.
- **Política de origen**: si una lectura devolvió texto que la app no escribió, el pedido queda marcado; desde ahí una escritura sólo se ejecuta con aprobación, el pedido lo dice («Este pedido viene después de leer texto que la app no escribió.») y una escritura no aprobable se rechaza sin ejecutar (`GuardaDeOrigen`). La ayuda (PD-2) va fuera del chat, así que el contexto recuperado nunca convive con herramientas de escritura.

## Sesión

- API: encabezado `X-Sesion-Id` (`SessionHeaderMiddleware`, sólo en `/api`).
- Página: cookie `sesion-movilidad` (`SessionMiddleware`, sólo en los documentos). `App.razor` pasa el id a `Routes`, que lo fija en el `SessionContext` del circuito: así las herramientas del circuito ven tu sesión. Ninguna herramienta recibe la sesión como argumento.
- El id **no autentica**: quien lo conoce ve esos datos. Es una identidad mínima para probar el aislamiento entre sesiones, no un control de acceso.

## Ayuda con recuperación y embeddings

El almacén (`CommunityToolkit.VectorData.InMemory`) se reconstruye al arrancar desde las 12 propiedades `public static string` de `LocalidadPolicy` (4) y `EncuestaPolicy` (8): si cambia una regla, cambia la ayuda. Cada fragmento tiene id calificado por clase (`LocalidadPolicy.RequisitoDelCodigoPostal`). El código no cambia según de dónde vengan los embeddings; cambia `EMB_PLAN`:

| `EMB_PLAN` | Embeddings | Qué muestra la página y la respuesta de `/ayuda` |
|---|---|---|
| `EMB-B` (default) | `qwen3-embedding:0.6b` en `OLLAMA_EMBED_ENDPOINT`, un servidor aparte del chat | el modelo de embeddings |
| `EMB-A` | el mismo modelo, en el mismo servidor del chat | el modelo de embeddings |
| `EMB-C` | generador de hash estable, sin modelo | «no ejecutado contra un modelo de embeddings» |

Con `EMB-A` o `EMB-B` y un servidor sin la capacidad `embedding`, la demo **no arranca** y el error dice cómo pasar a `EMB-C`: no degrada sola.

El código trata `EMB-A` y `EMB-B` igual; los dos planes existen por papeles separados y por la recarga del modelo cuando un servidor sirve chat y embeddings a la vez (no medido). Historia: hasta el 2026-10-28, el servidor compartido de entonces no tenía modelo de embeddings y respondía 501.

## Cómo se corre

Todo lo de .NET va en contenedores `mcr.microsoft.com/dotnet/sdk:10.0`; en el host sólo hacen falta bash, docker y jq. Ninguna dirección, puerto de infraestructura ni usuario tiene valor en el repo: se pasan por variable.

| Variable | Default en el código | Para qué |
|---|---|---|
| `OLLAMA_ENDPOINT` | `http://localhost:11434` | servidor de chat |
| `OLLAMA_MODEL` | `qwen3.5:4b` | modelo de chat |
| `OLLAMA_SERVIDOR` | `propio` | `compartido` \| `propio` \| `nube` |
| `OLLAMA_NUM_CTX` | `8192` | ventana; con un servidor `compartido`, el código exige que la fije quien corre (si no, no arranca) |
| `OLLAMA_EMBED_ENDPOINT` | el de `OLLAMA_PROPIO_ENDPOINT` | servidor de embeddings |
| `OLLAMA_EMBED_MODEL` | `qwen3-embedding:0.6b` | modelo de embeddings |
| `EMB_PLAN` | `EMB-B` | ver arriba |
| `ASISTENTE_MODO` | vacío | `guion`: sin ningún modelo (reconoce tres pedidos por patrón; sirve para recorrer la página y el cableado) |
| `RLLM_DIR` | `./registros` (ignorado por git) | registro de las corridas con modelo |
| `Concurrencia__Permisos`, `Concurrencia__Cola`, `Concurrencia__ReintentarEnSegundos` | `1`, `2`, `30` | límite de pedidos simultáneos al modelo (API y página); con `Permisos` fuera de 1–64 el host no arranca |
| `TURNO_CMD` | vacío | envoltorio de turno para servidores compartidos (`$TURNO_CMD <recurso> -- <comando>`); vacío, corre sin turno y lo avisa |

Dentro de un contenedor, `localhost` es el contenedor: un Ollama del host no se alcanza como `http://localhost:11434`.

### Con el script (desde esta carpeta)

```bash
tools/demo.sh build            # restore + build -warnaserror
tools/demo.sh pruebas          # build + Cableado/ y las pruebas del caso base, sin servidor de modelos, + chequeo de versiones
tools/demo.sh embeddings       # vectoriza los 12 requisitos y la pregunta de PD-2 con el servidor de embeddings y los guarda en RLLM_DIR
tools/demo.sh humo             # PD-2, PD-3 y PD-6 contra el servidor de chat (lee num_ctx del modelo cargado; usa los embeddings guardados)
tools/demo.sh host guion       # levanta el host sin modelo en http://127.0.0.1:8080 (PUERTO_HOST cambia el puerto)
tools/demo.sh host modelo      # lo mismo, con el cliente real (pasá las variables de arriba)
tools/demo.sh parar
BASE_REPO=<clon de Lab-E2E.WebBlazor> tools/diff-base.sh   # todo cambio frente a 8fb4c3a figura en la tabla de delta
```

`embeddings` y `humo` van en dos pasos porque cada uno usa un servidor distinto y ninguno pide el segundo mientras tiene el primero. Las entradas (`entradas.v1.json`) están congeladas por SHA-256 en `ejemplo.json`: si no coinciden, el script no corre.

### A mano, con `docker run`

```bash
docker run --rm -u 1000:1000 -p 127.0.0.1:8080:8080 \
  -v "$PWD/../..":/guia -w /guia/Demos/MovilidadUrbana.Asistente \
  -e HOME=/tmp -e ASPNETCORE_URLS=http://+:8080 \
  -e OLLAMA_ENDPOINT -e OLLAMA_MODEL -e OLLAMA_NUM_CTX -e OLLAMA_EMBED_ENDPOINT -e OLLAMA_EMBED_MODEL -e EMB_PLAN \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet run --project src/MovilidadUrbana.Asistente/MovilidadUrbana.Asistente.csproj --no-launch-profile
```

Se monta la carpeta de la guía entera porque la demo referencia `Examples/_Comun` por ruta relativa. Publicado sólo en el loopback del host.

### Con `compose.yaml`

```bash
docker compose up demo                                   # con tus servidores de chat y de embeddings
EMB_PLAN=EMB-C docker compose up demo                     # sin modelo de embeddings
ASISTENTE_MODO=guion EMB_PLAN=EMB-C docker compose up demo # sin ningún modelo
OLLAMA_EMBED_ENDPOINT=http://ollama-embed:11434 docker compose --profile embeddings up
docker compose exec ollama-embed ollama pull qwen3-embedding:0.6b   # una vez
```

El servicio opcional `ollama-embed` es Ollama 0.40.11 fijado por digest. Si tu configuración de docker tiene credenciales vencidas, bajá la imagen con `DOCKER_CONFIG` apuntando a una carpeta con un `config.json` vacío (`{}`).

## Pruebas

- **`Cableado/`** (sin red; corre con `tools/demo.sh pruebas`): un cliente guionado recorre PD-1…PD-6; aprobación (sin respuesta, la base no cambia; argumentos alterados se rechazan; el pedido muestra los argumentos resueltos); política de origen (`Escritura_ConResultadoNoConfiable_PideAprobacion`, `Escritura_NoAprobable_ConResultadoNoConfiable_Rechaza` y su control); sesiones (dos sesiones de API concurrentes, dos circuitos, un pedido que nombra una localidad de otra sesión); la arquitectura (cinco reglas por reflexión, cada una con al menos un tipo encontrado; la quinta acota la entrada de Presentation a `Infrastructure.AI` a `Pages/Chat.razor` y falla si esa excepción deja de existir; la regla lee firmas, atributos y el cuerpo de los métodos); los casos rojos de la regla (`MutacionesTests`: las mutaciones «Cuerpo» y «Atributo» de `Cableado/Mutantes/`, compiladas sólo en el ensamblado de pruebas, dan rojo); el límite de concurrencia (`LimitadorTests`: con 3 permisos y cola 0, de 4 pedidos simultáneos 3 entran y el cuarto recibe 503 con `Retry-After`, con una barrera que no depende del orden; la página y la API comparten el limitador; el ABM no pasa por él); la composición real con `ValidateScopes` y `ValidateOnBuild`, y la persistencia con `AddDbContextFactory` (`Persistencia_ConFabricaDeDbContext`); la capa única de opciones (con un espía debajo); `Compartido_SinNumCtxDelArnes_Rechaza`; los estímulos (todo valor de catálogo de una entrada pertenece a `Catalogos`); las versiones de los paquetes; y el humo del host (`Host_Humo_EstilosYEstados`, `Host_CookieSoloEnDocumentos`).
- **`ConModelo/`** (`[Category("LLM")]`): `EmbeddingsDePd2` (persiste los vectores) y `Humo` (PD-2, PD-3, PD-6). En `OneTimeSetUp`, un predicado de salto mira `/api/version` y `/api/show`: si el servidor no responde, el modelo no está o no declara la capacidad que hace falta, la prueba se salta con el motivo. Cada humo escribe su registro (una línea por llamada y una por entrada, con su oráculo) y afirma siempre los invariantes de código; la conducta del modelo queda en el oráculo del registro, que `tools/recalcular-oraculos.sh` (en la carpeta de la guía) recalcula sin volver a llamar: `demo-pd2-cita`, `demo-pd3-habitantes`, `demo-pd6-pedido`.
- **Los oráculos del humo son los de los ejemplos hermanos** (plan §E.5), adaptados a lo que la demo deja en el registro; la regla en C# está en `ConModelo/OraculosDeHumo.cs` y la recalculable, en `tools/oraculos/demo-pd*.jq`. `Cableado/OraculosDeHumoTests.cs` prueba, sin servidor, que separan los casos que tienen que separar.
  - `demo-pd2-cita` = regla de **07** (`rag-con-citas`, caso «respondible con valor»): cita válida, la fuente es el relevante del oro (pd2-01 tiene el objetivo de `r03`), los valores del literal citado están en la respuesta y no aparece otro valor. Una respuesta que cita bien y contradice el literal («5 dígitos») **falla**.
  - `demo-pd3-habitantes` = regla de **03** (`salida-estructurada`): la decisión de las reglas sobre la salida cruda, con la garantía del texto de 03, tiene que ser `faltan:habitantes`.
  - `demo-pd6-pedido` = regla de **11** (`agente-con-aprobacion`) en su rama **sin** aprobación: el modelo pidió `eliminar_localidad` por Resistencia y el pedido que armó el código, guardado en el `motivo` de la entrada, muestra «Resistencia» (Chaco). Que la base no cambie es invariante de código: `Humo.Pd6` lo afirma siempre (AC-4) y no se recalcula del registro.

> **Desvíos declarados frente a §E.5 («su oráculo [es] el del ejemplo hermano»).** (1) No se reusan los `.jq` de 07, 03 y 11 tal cual, porque el registro de la demo tiene otra forma: en PD-2 el modelo responde un objeto `{respuesta, fuente}` con una sola fuente, no `[Fuente: id]` en el texto, y el contexto lleva renglones `[id] texto`; en PD-3 el código de la demo no anota `decision=…` en el motivo, así que la decisión de 03 se compara con la **esperada** para la entrada; en PD-6 la persona **rechaza** y no hay baja que comparar con lo mostrado, así que se compara lo mostrado con lo pedido. La regla de cada hermano está copiada, no compartida: si cambia la del hermano, cambia esta. (2) En la primera versión de la demo los tres oráculos eran propios y más flojos (PD-2 sólo miraba la cita; PD-6 no recalculaba el pedido mostrado); el registro de la corrida del 2026-10-27 se recalcula con las reglas nuevas y coincide 3 de 3, sin volver a llamar al modelo. (3) La garantía numérica de la demo (`GarantiaNumerica`, sólo habitantes) es más laxa que la de 03 (`GarantiaDelTexto`, código postal y habitantes, sin reusar el token del código postal): el oráculo juzga con la de 03, y `Humo.Pd3` afirma, como invariante, que cuando 03 decide `faltan:habitantes` la demo también pide el dato.

### Salida esperada

- `tools/demo.sh pruebas`: `build -warnaserror` sin advertencias, todas las pruebas sin servidor en verde (las `ConModelo/` quedan fuera por el filtro) y `AC-6: familias en una sola versión, sin previews`.
- `tools/demo.sh host guion` y la página: «Modo guion: sin modelo. Ayuda: EMB-C: no ejecutado contra un modelo de embeddings.»; con «Borrá Resistencia» aparece el **Pedido de aprobación** con «Eliminar la localidad «Resistencia» (Chaco), id N, de tu sesión.» y los botones Aprobar y Rechazar; al rechazar, la localidad sigue.
- `GET /` → 200 con la cookie `sesion-movilidad`; la hoja `MovilidadUrbana.Asistente.<hash>.styles.css` → 200; `GET /api/v1/inexistente` → 404 `application/problem+json` sin cookie; `GET /inexistente` → 404 con la página «No encontrado».
- `tools/demo.sh humo`: por cada paso, una línea `pd2-01 (demo-pd2-cita): pasa|falla …`; los invariantes de código se cumplen siempre, pase o no el oráculo. Un modelo chico puede no pedir la herramienta, o citar bien y contradecir el literal: eso es un `falla` del oráculo, no una falla de la prueba.

## Criterios de aceptación que aplican

| Criterio | Qué pide | Cómo se verifica acá |
|---|---|---|
| AC-1 | build con `-warnaserror`, sin `NoWarn` | `tools/demo.sh build` |
| AC-2 | `Cableado/` en verde sin servidor de modelos | `tools/demo.sh pruebas` |
| AC-3 | humo de PD-2, PD-3 y PD-6 con registro válido y oráculo recalculable; PD-1, PD-4 y PD-5 por cableado y el registro de su ejemplo hermano | `tools/demo.sh embeddings`, `tools/demo.sh humo`, `tools/recalcular-oraculos.sh` |
| AC-4 | cero escrituras sin caso de uso o sin aprobación; lo ejecutado = lo mostrado | `Cableado/AprobacionTests.cs`, `Cableado/PasosTests.cs` |
| AC-5 | `Domain/` sin IA; `Application/` sin `Infrastructure` ni SDK; el SDK sólo en `Infrastructure.AI` y `Program`; herramientas sin repositorios; Presentation entra a `Infrastructure.AI` sólo por `Pages/Chat.razor` | `Cableado/ArquitecturaTests.cs` y sus casos rojos, `Cableado/MutacionesTests.cs` |
| AC-6 | versiones directas fijadas; cada familia en una sola versión, sin previews | `Cableado/VersionesTests.cs` y el chequeo de `tools/demo.sh pruebas` |
| AC-7, AC-8 | sin datos de la infraestructura ni de otros dominios | las compuertas de la carpeta de la guía (`tools/compuerta-ac7.sh`, `tools/compuerta-ac8.sh`) |
| AC-10 | todo cambio frente a la línea de base figura en la tabla de abajo | `tools/diff-base.sh` |
| AC-11 | humo del host: estilos aislados, páginas de estado por zona, cookie sólo en documentos | `Cableado/HostTests.cs`, y con el host levantado, `curl` |

## Delta frente al caso base

La demo empezó como **copia literal**, en `8fb4c3a`, de `src/MovilidadUrbana.ApiWeb/`, `tests/MovilidadUrbana.ApiWeb.Tests/`, `src/MovilidadUrbana.Web/Sessions/SessionMiddleware.cs` y `src/MovilidadUrbana.Web/wwwroot/css/` (`Tokens.css`, `Componentes.css`). Cada archivo copiado lleva en la cabecera `Procedencia: Lab-E2E.WebBlazor/<ruta> @ 8fb4c3a` y, si cambió, `Delta: <columna> — <motivo>`; cada archivo que no existía se rotula «Nuevo». Los `.json` no admiten comentarios: su delta está sólo acá. `tools/diff-base.sh` compara contra la base y falla si un cambio no tiene fila.

En **todos** los archivos copiados, el namespace raíz `MovilidadUrbana.ApiWeb` pasa a `MovilidadUrbana.Asistente` («modificado por otro motivo»); no se repite en cada fila.

| Pieza | Agregado por la IA | Modificado a causa de la IA | Modificado por otro motivo |
|---|---|---|---|
| Proyecto: `MovilidadUrbana.Asistente.csproj` (era `MovilidadUrbana.ApiWeb.csproj`) | paquetes de IA (`Microsoft.Extensions.AI`, `VectorData.Abstractions`, `CommunityToolkit.VectorData.InMemory`) y la referencia a `Examples/_Comun/MovilidadUrbana.Asistente.Comun` | — | versiones a `Directory.Packages.props` (con gestión central, una versión en línea es error); se quitan `GenerateDocumentationFile` y el `NoWarn` de CS1591 (la documentación XML de la API no es tema de la guía y no se admite `NoWarn`) |
| `Program.cs` | `AddAI(config)`, `PrepararAIAsync`, `AddRazorComponents().AddInteractiveServerComponents()`, `UseAntiforgery`, `MapStaticAssets`, `MapRazorComponents<App>()` (ADR-13); `AddLimitadorDelModelo` y `UseRateLimiter` (límite de concurrencia en la entrada) | `SessionHeaderMiddleware` sólo en `/api`; `SessionMiddleware` sólo fuera de `/api` | `UseStatusCodePages` sólo en `/api`; fuera de `/api`, la página «No encontrado» de Blazor (re-execute en el pipeline principal, como la web del caso base: dentro de una rama no se vuelve a enrutar) (ADR-13) |
| `Domain/` (`Catalogos.cs`, entidades) | — | — | sólo el namespace |
| `Domain/Rules/` `LocalidadRules.cs`, `EncuestaRules.cs` | — | **endurecimiento**: provincia, medios, frecuencia y motivo tienen que ser valores de `Catalogos` (con un modelo que propone texto, «no vacío» deja pasar valores inventados); en `LocalidadRules`, el código postal sólo con dígitos 0-9 y el largo máximo del nombre aplicado (REC-17, igual que `Examples/_Comun`) | — |
| `Application/Localidades/LocalidadService.cs` | — | `GetByNombreYProvinciaAsync(nombre, provincia)`, con error si es ambiguo (para que la baja no reciba un id del modelo); `SaveAsync` informa el id creado; mensaje propio para una provincia fuera del catálogo; el mensaje del nombre da el largo mínimo y el máximo (REC-17) | — |
| `Application/Result.cs` | — | `Id` de lo creado y `Success(mensaje, id)` | — |
| `Application/Encuestas/EncuestaService.cs` | — | `ValidarPaso` rechaza medios, frecuencia y motivo fuera de `Catalogos` | — |
| `Application/Abstractions/IEncuestaRepository.cs` | — | `GetAllAsync` de la sesión, para los agregados | — |
| `Application/DependencyInjection.cs` | — | registra `ResultadosDeEncuestaService`, `AltaDesdeTextoService` y `EncuestaDesdeTextoService` | — |
| `Application/` nuevos: `Abstractions/` (`IExtractorDeLocalidad.cs`, `IExtractorDeEncuesta.cs`, `IAyuda.cs`), `Ayuda/RespuestaDeAyuda.cs`, `ResultadosDeEncuestaService.cs`, `AltaDesdeTextoService.cs`, `LocalidadPropuesta.cs` (con `int? Habitantes`), `EncuestaDesdeTextoService.cs`, `EncuestaPropuesta.cs` | todo | — | — |
| `Infrastructure/Persistence/SessionSeeder.cs` | siembra de 20 respuestas de encuesta por sesión, **escrita para el ejemplo** (valores de `Catalogos`, «Persona 01…20», edades del rango de `EncuestaRules`) | — | — |
| `Infrastructure/Persistence/EncuestaRepository.cs` | — | `GetAllAsync` acotado a la sesión, con siembra previa | — |
| `Infrastructure/Persistence/` (resto) y `Infrastructure/DependencyInjection.cs` | — | — | sólo el namespace; EF Core Sqlite 10.0.11 → 10.0.14 en `Directory.Packages.props` |
| `Infrastructure/Sessions/SessionContext.cs` | — | — | `CookieName` («sesion-movilidad»), que trae la copia de `SessionMiddleware` de la web (ADR-13) |
| `Infrastructure/AI/` | todo: `DependencyInjection.cs` (`AddAI`), `NormalizadorDeCatalogo.cs`, `ClienteDeDemostracion.cs` (modo guion), `Extraccion/` (`ExtractorDeLocalidad.cs`, `ExtractorDeEncuesta.cs`, `GarantiaNumerica.cs`), `Ayuda/` (`AyudaConRecuperacion.cs`, `CorpusDeAyuda.cs`, `GeneradorDeEmbeddingsPersistidos.cs`), `Chat/` (`SesionDeChat.cs`, `HerramientasDelAsistente.cs`, `ResolutorDeAprobaciones.cs`, `EstadoDelPedido.cs`, `GuardaDeOrigen.cs`, `Argumentos.cs`) | — | — |
| `Sessions/SessionHeaderMiddleware.cs` | — | se aplica sólo a `/api` (en `Program.cs`) | — |
| `Sessions/SessionMiddleware.cs` (de la web) | — | se aplica sólo a los documentos fuera de `/api` | — |
| `wwwroot/css/` `Tokens.css`, `Componentes.css` (de la web) | — | — | se copian para conservar la temática del caso base (ADR-13) |
| `Components/` `App.razor`, `Routes.razor`, `_Imports.razor`, `Layout/MainLayout.razor`, `MainLayout.razor.css`, `Pages/Chat.razor`, `Chat.razor.css`, `Pages/NoEncontrado.razor` | **nuevos** (ADR-13): la página de chat, que corre cada pedido con el límite de concurrencia compartido; `Routes` repite el puente `SessionId` → `SessionContext` de la web del caso base (patrón, no copia); la hoja de estilos aislados con el nombre del ensamblado nuevo | — | — |
| `Contracts/` `Ayuda.cs`, `DesdeTexto.cs` | todo | — | — |
| `Controllers/` `DesdeTextoController.cs`, `AyudaController.cs` | todo (estilo controllers, fiel al caso base), con la política `modelo` del límite de concurrencia. Sin ruta de chat | — | — |
| `Concurrencia/LimitadorDelModelo.cs` | todo: el `ConcurrencyLimiter` singleton, sus opciones (sección `Concurrencia`) y la política `modelo` (503 con `Retry-After`), compartido por la API y `Pages/Chat.razor` | — | — |
| `Controllers/LocalidadesController.cs` | — | — | deja de leer del repositorio salteando el servicio; ubica lo creado por el id que informa `SaveAsync` |
| `Controllers/` (resto), `Contracts/` (resto) | — | — | sólo el namespace |
| `appsettings.json`, `appsettings.Development.json`, `Properties/launchSettings.json` | — | — | sin cambios |
| Pruebas: `MovilidadUrbana.Asistente.Tests.csproj` (era `MovilidadUrbana.ApiWeb.Tests.csproj`) | `Cableado/`, `ConModelo/`, `Entradas.cs` y el enlace a `entradas.v1.json` | — | versiones centrales (`Mvc.Testing` 10.0.11, `NUnit.Analyzers` 4.7.0, fieles al caso base) |
| `ApiWebApplicationFactory.cs` | — | sin servidor de modelos: cliente guionado (con un espía debajo de la capa de opciones), `EMB-C`, endpoints a un puerto cerrado; `ValidateScopes` y `ValidateOnBuild`; deja de ser `sealed` (las pruebas con modelo la extienden) | — |
| `EncuestasTests.cs` | — | — | el nombre de persona de la prueba pasa al conjunto sintético «Persona 01…20» (sólo nombres sintéticos en la guía) |
| `LocalidadesTests.cs`, `DocumentationTests.cs` | — | — | sólo el namespace |

Afuera de `src/` y `tests/` todo es nuevo: `README.md`, `compose.yaml`, `ejemplo.json`, `entradas.v1.json` (escritas para el ejemplo, congeladas), `MovilidadUrbana.Asistente.slnx`, `tools/demo.sh`, `tools/diff-base.sh`, y en `Demos/` el `Directory.Build.props`, el `Directory.Packages.props` y el `.gitignore`.

## Qué queda afuera y por qué

- **Texto libre de la encuesta**: el caso no tiene un campo de texto libre y no se agrega uno.
- **Workflow como contraejemplo**: va en `Examples/10`.
- **Contexto completo con prefijo estable**: va en `Examples/05`.
- **`/mcp`**: va en `Examples/13`, que monta MCP sobre los mismos casos de uso y valida Host y `Origin`. Para exponerlo acá harían falta esa misma validación y la decisión de qué lecturas se publican.
- **La app MAUI** y un cliente móvil: fuera de alcance; ningún concepto de la capa se ve mejor en un teléfono que en esta página.
- **`Dockerfile` y `PublishContainer`**: la demo corre con el SDK y `dotnet run`.
