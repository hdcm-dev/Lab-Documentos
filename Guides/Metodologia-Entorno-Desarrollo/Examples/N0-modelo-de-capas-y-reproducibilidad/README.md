# N0 — Modelo de capas del entorno: laboratorio

Seis experimentos, uno por capa del modelo (más dos complementarios de la verificación adversarial), sobre un mismo principio: **un commit no alcanza para saber qué se construyó**. Cada experimento deja fija toda la pila salvo una capa, mueve esa capa y muestra qué cambia en el resultado. Ninguno deja `bin/`, `obj/` ni `node_modules/` en el repositorio: el build ocurre en directorios temporales dentro de los contenedores, y el `.gitignore` de `Examples/` cubre el resto.

## Índice

- [Requisitos](#requisitos)
- [Cómo correrlo](#cómo-correrlo)
- [Qué demuestra cada experimento](#qué-demuestra-cada-experimento)
- [Salidas de referencia](#salidas-de-referencia)
- [Límites](#límites)

## Requisitos

| Herramienta | Para qué | Versión usada en la corrida de referencia (2026-10-07) |
|---|---|---|
| Docker | Todos los experimentos | Docker con `buildx` (E0-4 lo usa para consultar el registro sin descargar) |
| `mcr.microsoft.com/dotnet/sdk:8.0` | E0-1, E0-2, E0-3b, E0-5, E0-6 | SDK 8.0.425, digest local `sha256:78235e09…2743` |
| `mcr.microsoft.com/dotnet/sdk:10.0` | E0-2, E0-6 | SDK 10.0.400, digest local `sha256:e1ffd2a9…a64c` |
| `node:22-bookworm-slim` | E0-3a | Node 22.23.2, npm 10.9.8 |
| `python:3.12-slim` | E0-3c | Python 3.12.14; uv 0.12.23 fijado dentro del script |
| Red hacia nuget.org, registry.npmjs.org, pypi.org | E0-3a/b/c | — |

No hace falta .NET, Node ni Python en el host.

## Cómo correrlo

Todo junto, desde esta carpeta (en Linux, macOS o WSL; en Windows nativo, con Git Bash):

```bash
./correr-todo.sh          # ≈ 1 minuto con las imágenes ya descargadas; reescribe salidas/
```

Uno por uno, con el comando exacto que usa `correr-todo.sh`:

```bash
# E0-1 · bytes del fuente: el mismo texto en cp1252 y en UTF-8, compilado en Linux
docker run --rm -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 \
  -v "$PWD/capa1-bytes":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/experimento.sh

# E0-2 · toolchain: qué SDK y qué compilador elige cada imagen, y qué hace global.json
for t in 8.0 10.0; do
  docker run --rm -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 -e IMAGEN=sdk:$t \
    -v "$PWD/capa2-toolchain":/src:ro -v "$PWD/capa5-proceso":/calc:ro \
    mcr.microsoft.com/dotnet/sdk:$t bash /src/experimento.sh
done

# E0-3 · grafo de dependencias: rango, lock y modo estricto en los tres ecosistemas
docker run --rm -v "$PWD/capa3-grafo/npm":/src:ro node:22-bookworm-slim bash /src/experimento.sh
docker run --rm -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 \
  -v "$PWD/capa3-grafo/nuget":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/experimento.sh
docker run --rm -v "$PWD/capa3-grafo/uv":/src:ro python:3.12-slim bash /src/experimento.sh

# E0-4 · SO / imagen base: ¿la etiqueta local apunta a los mismos bytes que hoy en el registro?
bash capa4-imagen/experimento.sh

# E0-5 · proceso de build: determinismo, ruta de trabajo y normalización de rutas
docker run --rm -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 \
  -v "$PWD/capa5-proceso":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/experimento.sh

# E0-6 · metadatos: manifiesto del entorno por capa, en dos imágenes, y su diff
bash capa6-procedencia/experimento.sh

# Complementarios de la verificación adversarial (2026-10-07)
docker run --rm -v "$PWD/capa2-toolchain":/src:ro node:22-bookworm-slim bash /src/engines-npm.sh
docker run --rm -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 \
  -v "$PWD/capa5-proceso":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/verificacion-sourceroot.sh
```

## Qué demuestra cada experimento

| ID | Capa que se mueve | Qué queda fijo | Resultado observado | Lo que enseña |
|---|---|---|---|---|
| E0-1 | 1 · bytes del fuente | SDK 8.0.425, proyecto, grafo vacío | cp1252: `error CS1056: Unexpected character '�'` (identificador `ÇodigoAcción`). UTF-8: compila y escribe `Texto: acción`. | El texto «igual» no es el mismo insumo: el compilador recibe bytes. En Linux, sin BOM ni `CodePage`, los bytes `c7` y `f3` de cp1252 no son UTF-8 válido. |
| E0-2 | 2 · toolchain | fuente, ruta `/w/a`, `TargetFramework=net8.0` | Sin `global.json`, `sdk:8.0` usa 8.0.425 (Roslyn 4.11) y `sdk:10.0` usa 10.0.400 (Roslyn 5.9); la misma DLL `net8.0` sale con hash `56abc6…` y `61fbcf…`. `rollForward: disable` con 8.0.100 falla en ambas («A compatible .NET SDK was not found»); `latestFeature` acepta 8.0.425 y rechaza 10.0.400. | Apuntar a `net8.0` no fija el compilador. Sin declarar la capa 2, la elige la máquina. |
| E0-3a | 3 · grafo (npm) | Node 22.23.2, npm 10.9.8 | `^2.0.0` sin lock instala `ms` 2.1.3; con lock congelado, `npm ci` instala 2.0.0; si `package.json` y el lock divergen, `npm ci` falla con `EUSAGE`. | Un rango no es una versión. El lock congela y `npm ci` hace cumplir. |
| E0-3b | 3 · grafo (NuGet) | SDK 8.0.425 | `Version="13.0"` resuelve 13.0.1 (la más baja); `13.*` resuelve 13.0.4 (la más alta); con lock y versión editada, `--locked-mode` falla con `NU1004`. | «Rango» no significa lo mismo en NuGet que en npm: cambia la política de resolución, no sólo la sintaxis. |
| E0-3c | 3 · grafo (uv) | Python 3.12.14, uv 0.12.23 | `six>=1.10` resuelve 1.17.0; al editar el rango sin relockear, `uv lock --locked` y `uv sync --locked` fallan. | Mismo patrón que npm y NuGet, con otro gestor. |
| E0-4 | 4 · SO / imagen | nombre de la etiqueta `sdk:8.0` | El digest en caché (creado 2026-09-19) difiere del que el registro sirve hoy (creado 2026-10-06), con el **mismo** `DOTNET_SDK_VERSION=8.0.425`. Para Docker Hub el registro no respondió (en la verificación del 2026-10-07, `401 Unauthorized` al pedir el token) y el script lo informa como «SIN DATO». | La etiqueta se mueve aunque la versión del SDK no cambie; que lo que cambió sean paquetes del SO es una inferencia, no se compararon los paquetes. Sólo el digest nombra bytes. |
| E0-5 | 5 · proceso de build | fuente, SDK 8.0.425, grafo | Misma ruta: hashes iguales. `Deterministic=false`: distintos. Rutas distintas: distintos (la DLL guarda `/home/x/otra/obj/…/Calculo.pdb`). `ContinuousIntegrationBuild=true` **sin** repo git: siguen distintos. **Con** repo git: iguales y la ruta queda `/_/obj/…`. | Determinista no es lo mismo que reproducible: la ruta es una entrada más. La normalización de rutas observada sólo actuó dentro de un repo git. |
| E0-2b | 2 · toolchain (npm) | Node 22.23.2, npm 10.9.8 | `engines.node: "<1"` en el propio proyecto: `npm install` sólo advierte (`EBADENGINE`, código 0); con `--engine-strict` falla (`notsup`, código 1). | `engines` declara; sólo `engine-strict` hace cumplir. |
| E0-5b | 5 · proceso de build | fuente, SDK 8.0.425, ruta `/home/x/otra` | Con git pero `EnableSourceControlManagerQueries=false`: la ruta NO se normaliza. Sin git y con un `SourceRoot` declarado a mano: se normaliza (`/_1/obj/…`). Sin git y con `PathMap` explícito: `/_/obj/…`. | La normalización de `ContinuousIntegrationBuild` mapea los ítems `SourceRoot`; dentro de un repo git los aporta la consulta al control de versiones. `PathMap` explícito no depende de git. |
| E0-6 | 6 · metadatos | commit `95e6a39…` (fecha fija) | El manifiesto por capa en `sdk:8.0` y `sdk:10.0` difiere en capa 2 (8.0.425 frente a «`dotnet --version` falló», porque `global.json` pide 8.0.x) y en capa 4 (Debian 12 frente a Ubuntu 24.04.4, dos digests). | Con un registro por capa, «¿por qué el mismo commit da distinto?» se contesta con un `diff`. |

## Salidas de referencia

La carpeta `salidas/` guarda la salida textual de la corrida del 2026-10-07 (`E0-1-…txt` a `E0-6-…txt`, más `E0-2b` y `E0-5b` de la verificación adversarial). Volver a correr reescribe esos archivos; las rutas temporales y la fecha de registro cambian en cada corrida, y E0-4 cambia cada vez que el publicador mueve una etiqueta. Los hashes de E0-2 y E0-5 deberían repetirse (salvo el caso (2) de E0-5, `Deterministic=false`, que cambia en cada corrida por diseño) mientras no cambien el SDK ni la imagen: en la corrida de referencia, la DLL de E0-5 caso (1) y la de E0-2 con `sdk:8.0` dieron el mismo `56abc6012297e83e` en dos contenedores distintos.

## Límites

- `registrar-entorno.sh` no es una atestación de procedencia: no está firmado ni lo genera una plataforma de build. Es el registro mínimo para comparar entornos; la procedencia verificable es tema del núcleo N6.
- E0-4 depende de que el registro responda. Con Docker Hub sin credenciales o con límite de consultas, el resultado es «SIN DATO», no «IGUAL».
- E0-5 se corrió con `sdk:8.0` en Linux. No se probó en Windows ni con otros SDK.
