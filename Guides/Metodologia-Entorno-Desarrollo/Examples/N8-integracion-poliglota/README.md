# N8 — Integración políglota: experimentos

Una solución mínima con **.NET + Node.js + Python** en un mismo repositorio ([`solucion/`](solucion/)) y seis experimentos que responden qué pasa cuando los tres ecosistemas comparten repo, imagen, build, SBOM y procedencia. Cada experimento copia la solución a un directorio temporal (`mktemp -d`) y lo borra al terminar: no deja `bin/`, `obj/`, `node_modules/`, `.venv/` ni `dist/` en el repositorio. Las salidas capturadas el 2026-10-07 están en [`salidas/`](salidas/).

**Requisitos del host:** Docker con buildx (probado con buildx 0.35 en Debian 13, x86_64), `python3`, `curl`. No hace falta .NET, Node ni Python en el host: todo corre en contenedores. Si el host tiene credenciales de Docker Hub rotas, alcanza con `export DOCKER_CONFIG=$(mktemp -d)` para tirar en modo anónimo.

## La solución de referencia

```
solucion/
├── .editorconfig            política de texto raíz, por extensión (N2 + .ps1 + lock files)
├── .gitattributes           LF para todo; CRLF sólo .cmd/.bat; locks como texto
├── global.json              SDK 8.0.425, rollForward: disable           (N3)
├── .nvmrc                   22.23.2                                      (N3)
├── Directory.Packages.props CPM: versiones NuGet directas                 (N4)
├── nuget.config             <clear/> + mapeo de fuentes                   (N4)
├── src/Saludo/              consola .NET: Saludo.csproj (targets que llaman a npm y uv),
│                            Program.cs (identificador no ASCII), packages.lock.json
├── web/                     package.json (engines), package-lock.json, .npmrc (engine-strict), build.mjs
├── tools/gen/               pyproject.toml (requires-python, required-version de uv), uv.lock,
│                            .python-version (3.12.15), gen.py (genera código C#)
├── verificar.sh             compuerta única: texto + toolchain + restauración estricta
├── verificar-texto.py       chequeo de texto por extensión (versión reducida del de N2)
├── build.sh                 orquestador: compuerta → Python → Node → .NET → registro
├── registrar-entorno.sh     versiones de los tres toolchains + hashes de los locks y la DLL
└── ci/github-actions.ejemplo.yml   ejemplo NO ejecutado de CI (imagen única y matriz)
```

La build tiene dos puntos de entrada que producen lo mismo:

- **MSBuild llama a todos** (`dotnet build src/Saludo`): los targets `ConstruirWeb` y `GenerarDatos` del `.csproj` ejecutan `npm ci` + `npm run build` y `uv run --locked python gen.py`, con `Inputs`/`Outputs` para que sean incrementales. Es lo que pasa cuando alguien aprieta F5 en Visual Studio.
- **El orquestador llama a todos** (`./build.sh`): corre la compuerta, cada ecosistema una sola vez y después `dotnet build -p:ConstruirPoliglota=false`, que apaga los targets anteriores.

## Imágenes

```bash
# Imagen única con los tres runtimes (SDK 8). Las versiones salen de los archivos del repo.
docker build -t lab-n8-poliglota:local \
  --build-arg PYTHON_VERSION=$(cat solucion/tools/gen/.python-version) imagen/

# Variante con SDK 10, sólo para E8-4 (el SDK de proyectos JavaScript no carga en el MSBuild del SDK 8).
docker build -t lab-n8-poliglota:sdk10 \
  --build-arg SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c \
  --build-arg PYTHON_VERSION=$(cat solucion/tools/gen/.python-version) imagen/
```

[`imagen/Dockerfile`](imagen/Dockerfile) parte del SDK oficial por digest y copia Node y npm desde `node:22-bookworm-slim` por digest y `uv` desde `ghcr.io/astral-sh/uv:0.12.23` por digest; Python 3.12.15 lo instala `uv`. [`imagen/Dockerfile.por-componente`](imagen/Dockerfile.por-componente) es la variante «una imagen por componente» en etapas (`py` → `web` → `net` → `artefacto`).

## Índice de experimentos

| ID | Carpeta | Qué demuestra | Salida |
|---|---|---|---|
| E8-1 | [`e1-choque-de-convenciones/`](e1-choque-de-convenciones/) | Con la política por extensión, los 15 lectores de los tres ecosistemas pasan. «BOM para todo» (la corrección del caso C# aplicada a ciegas) rompe el shebang de Python, `json.load` de Python, `tomllib`, `JSON.parse` de Node y la comparación de `.nvmrc`; `uv`, `npm` y `dotnet` lo toleran. «CRLF para todo» rompe los dos shebangs. El código heredado en cp1252 rompe sólo a C# y a Python estricto; Node lo lee **sin error** y con U+FFFD. Dos «OK» engañosos: bash ejecuta un `.sh` con BOM con rc=0 y un error en stderr. | [`E8-1`](salidas/E8-1-choque-de-convenciones.txt) |
| E8-2 | [`e2-punto-unico-de-versiones/`](e2-punto-unico-de-versiones/) | La compuerta compara lo instalado con `global.json`, `.nvmrc` y `tools/gen/.python-version` y falla si alguien sube `.nvmrc` sin reconstruir la imagen. mise 2026.10.3 no lee los archivos nativos por defecto; habilitados, los lee; un `mise.toml` propio gana en silencio sobre `.nvmrc`. Un `.python-version` en la raíz **no** gobierna al proyecto uv de `tools/gen/` (uv no busca fuera del proyecto) y uv usa otro parche sin avisar. | [`E8-2`](salidas/E8-2-punto-unico-de-versiones.txt) |
| E8-3 | [`e3-frontera-de-la-build/`](e3-frontera-de-la-build/) | `dotnet build` en la imagen oficial del SDK falla con `MSB3073 … exited with code 127` (no hay npm). En la imagen multi-runtime, MSBuild llama a npm y uv; la segunda build saltea los targets; tocar `uv.lock` rehace sólo el paso Python. MSBuild usa el Node del `PATH` (24) aunque `.nvmrc` diga 22: con `engine-strict=true` falla con `EBADENGINE`; sin él, construye con Node 24. `build.sh` produce la misma DLL. | [`E8-3`](salidas/E8-3-frontera-de-la-build.txt) |
| E8-4 | [`e4-esproj-de-visual-studio/`](e4-esproj-de-visual-studio/) | El proyecto JavaScript de Visual Studio (`.esproj`, SDK `Microsoft.VisualStudio.JavaScript.Sdk/1.0.6887863`) no carga con el MSBuild del SDK 8 (`System.Text.Json 9.0.0.0`). Con SDK 10 corre `npm install` (no `npm ci`) y **reescribe el lock** ante deriva: la build pasa con otro grafo. Con `ShouldRunNpmInstall=false` y `npm ci` enganchado, falla con `EUSAGE`. En Linux intenta `cmd.exe` para `npm audit` y deja un directorio llamado `obj\Debug`. | [`E8-4`](salidas/E8-4-esproj-de-visual-studio.txt) |
| E8-5 | [`e5-imagen-unica-vs-varias/`](e5-imagen-unica-vs-varias/) | Tamaños y versiones de las imágenes oficiales frente a la imagen única (1156 MB; 11 de 16 capas compartidas con el SDK). La variante por componente con `python:3.12-slim` local (3.12.14) falla contra `.python-version` (3.12.15); fijada a `python:3.12.15-slim@sha256:…` produce **la misma DLL byte a byte** (`cf57904b…`) que la imagen única. | [`E8-5`](salidas/E8-5-imagen-unica-vs-varias.txt) |
| E8-6 | [`e6-sbom-unificado/`](e6-sbom-unificado/) | syft 1.54.1 sobre el repo encuentra los tres ecosistemas en un CycloneDX 1.7. Los exportadores nativos (`uv export` en *preview*, `npm sbom`, CycloneDX .NET 6.2.0) dan tres CycloneDX 1.5 que `cyclonedx-cli merge` une. BuildKit emite procedencia SLSA v1 con las cuatro imágenes base por digest como materiales; su SBOM del artefacto sólo ve NuGet, y con `BUILDKIT_SBOM_SCAN_STAGE=true` agrega un SBOM por etapa (npm y PyPI, más paquetes Debian y 512 crates de Rust del binario de uv). | [`E8-6`](salidas/E8-6-sbom-unificado.txt) |

## Cómo correrlos

Desde esta carpeta, después de construir las imágenes:

```bash
./e1-choque-de-convenciones/experimento.sh
./e2-punto-unico-de-versiones/experimento.sh      # baja mise v2026.10.3 dentro del contenedor
./e3-frontera-de-la-build/experimento.sh          # necesita node:24-bookworm-slim (lo baja por digest)
./e4-esproj-de-visual-studio/experimento.sh       # necesita lab-n8-poliglota:sdk10
./e5-imagen-unica-vs-varias/experimento.sh        # docker build --no-cache de la variante por componente
./e6-sbom-unificado/experimento.sh                # crea el builder buildx "n8-attest" (docker-container)
```

Lo esencial que ejecutan, para leerlo sin abrir los scripts:

```bash
# Compuerta y build orquestada en la imagen única (E8-2, E8-3, E8-5)
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD/solucion:/src" -w /src \
  lab-n8-poliglota:local ./build.sh

# MSBuild como punto de entrada (E8-3)
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$W:/w" -w /w \
  lab-n8-poliglota:local dotnet build src/Saludo

# Una imagen por componente con procedencia y SBOM (E8-6)
docker buildx build --builder n8-attest --target artefacto \
  --build-arg PY_IMAGE=python:3.12.15-slim@sha256:05cda9777409a9c3ffddd94a4c476b79f0769a0b4857f0c7ed9226b6800b0d6f \
  --provenance=mode=max --sbom=true -f imagen/Dockerfile.por-componente \
  --output type=local,dest=/tmp/salida solucion
```

## Notas para reproducir

- **E8-5 y E8-6 dependen del momento.** `python:3.12-slim` local (bajada hace cinco semanas) trae 3.12.14; la etiqueta hoy apunta al mismo digest que `python:3.12.15-slim`. Ese corrimiento es justamente el punto del experimento.
- **La DLL es idéntica entre estrategias sólo si la ruta de build es la misma.** E8-3 monta el repo en `/w` (hash `4dd5d90f…`); E8-5 y E8-6 en `/src` (hash `cf57904b…` en las tres variantes). Sin repositorio git, `ContinuousIntegrationBuild` no normaliza rutas (ver N0 y N6).
- **El aviso «An issue was encountered verifying workloads»** aparece en todas las corridas del SDK 8 con `HOME=/tmp` y usuario no root; no afecta los resultados y no se investigó.
- **E8-4 lee un target interno.** `RunNpmInstall` es el nombre del target en `Sdk.targets` de la versión 1.0.6887863, no una propiedad documentada: al subir la versión del SDK de JavaScript, revisar que siga existiendo.
- **El builder `n8-attest`** queda creado después de E8-6; se borra con `docker buildx rm n8-attest`.
- **El ejemplo de CI** ([`solucion/ci/github-actions.ejemplo.yml`](solucion/ci/github-actions.ejemplo.yml)) no se ejecutó: este laboratorio no tiene una CI de GitHub.

## Ampliaciones de la revisión (2026-10-09)

- **`devEngines` en `solucion/web/package.json`** (`runtime` = Node 22.23.2, sin `onFail`). E8-3, sección 3, muestra tres casos con Node 24 antepuesto en el `PATH`: sin `devEngines` y con `engine-strict`, `EBADENGINE`; sin los dos, el bundle sale con Node 24; con `devEngines`, `EBADDEVENGINES` aunque no haya `engine-strict`.
- **`imagen/Dockerfile.por-componente`** fija por defecto `PY_IMAGE=python:3.12.15-slim@sha256:05cda977…`: construye sin `--build-arg` (`salidas/E8-5-por-componente-sin-build-arg.txt`). E8-5 pasa la copia local de `python:3.12-slim` de forma explícita para seguir mostrando la falla `No interpreter found for Python 3.12.15`.
- **El comando de la guía §11.5, tal como está escrito**, en una máquina sin el builder `n8-attest`: `e6-sbom-unificado/comando-de-la-guia.sh` → `salidas/E8-6-comando-de-la-guia.txt` (rc=0; procedencia `https://slsa.dev/provenance/v1`, `builder.id` vacío, materiales por digest). Con credenciales de Docker Hub rotas en el host, `export DOCKER_CONFIG=$(mktemp -d)` antes de correrlo.
