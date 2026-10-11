# N4 — Congelamiento de dependencias: laboratorio

Cinco experimentos mínimos que muestran, en NuGet, npm/pnpm y Python (uv, pip, Poetry), la diferencia entre **declarar rangos** y **congelar el grafo resuelto**, y qué comando hace cumplir el congelamiento.

Cada experimento es un `run.sh` que corre **dentro de un contenedor**, sobre una copia en `/tmp`. El directorio del ejemplo se monta en sólo lectura (`:ro`), así que no quedan `bin/`, `obj/`, `node_modules/`, `.venv/` ni lock files generados en el repositorio.

## Índice

- [Requisitos](#requisitos)
- [Cómo correrlos](#cómo-correrlos)
- [Qué demuestra cada uno](#qué-demuestra-cada-uno)
- [Advertencias de reproducción](#advertencias-de-reproducción)

## Requisitos

- Docker con acceso a internet (nuget.org, registry.npmjs.org y pypi.org). E3 no usa red: sus feeds son carpetas locales.
- Imágenes: `mcr.microsoft.com/dotnet/sdk:8.0`, `node:22-bookworm-slim` y `python:3.12-slim`.

## Cómo correrlos

Desde esta carpeta, con bash (Linux, WSL o un devcontainer):

```bash
# E1 — packages.lock.json, locked mode, contentHash y RID
docker run --rm -v "$PWD/e1-nuget-lock:/src:ro" mcr.microsoft.com/dotnet/sdk:8.0 bash /src/run.sh

# E2 — Central Package Management y transitive pinning
docker run --rm -v "$PWD/e2-nuget-cpm-pinning:/src:ro" mcr.microsoft.com/dotnet/sdk:8.0 bash /src/run.sh

# E3 — package source mapping y paquete impostor (sin red)
docker run --rm -v "$PWD/e3-nuget-source-mapping:/src:ro" mcr.microsoft.com/dotnet/sdk:8.0 bash /src/run.sh

# E4 — package-lock.json, npm ci frente a npm install, integrity, binarios por plataforma, pnpm
docker run --rm -v "$PWD/e4-npm-lock:/src:ro" node:22-bookworm-slim bash /src/run.sh

# E5 — uv.lock, uv pip compile con hashes, pylock.toml (PEP 751), pip lock y Poetry
docker run --rm -v "$PWD/e5-python-lock:/src:ro" python:3.12-slim bash /src/run.sh
```

Las versiones de las herramientas que se instalan dentro del contenedor están fijadas en variables al principio de cada `run.sh` (`PNPM_VERSION`, `UV_VERSION`, `PIP_VERSION`, `POETRY_VERSION`) y se pueden cambiar con `-e`, por ejemplo `-e UV_VERSION=0.12.23`.

## Qué demuestra cada uno

| Exp. | Carpeta | Demuestra | Comando que falla | Comando que reescribe el lock sin avisar |
|---|---|---|---|---|
| E1 | `e1-nuget-lock` | El lock guarda el grafo completo (directas y transitivas) con `contentHash`; el modo bloqueado rechaza la deriva, el contenido alterado y un cambio en el conjunto de RID. | `dotnet restore --locked-mode` → `NU1004`, `NU1403` | `dotnet restore` |
| E2 | `e2-nuget-cpm-pinning` | CPM fija las versiones **declaradas**; con *transitive pinning* también gobierna transitivas; no permite bajar versiones (`NU1109`) ni `Version` en el `.csproj` (`NU1008`); `Version` es un **mínimo**: si la versión pedida no existe, NuGet resuelve la siguiente con `NU1603`; con lock, sin aviso. Las salidas conservan el rótulo anterior de 5c («la línea comentada de A.4»); el resultado no cambia. | `dotnet restore` → `NU1109`, `NU1008` | — |
| E3 | `e3-nuget-source-mapping` | Con dos feeds que publican el mismo `Id` y versión, el mapeo de fuentes determina de dónde baja el paquete; con la caché caliente el mapeo **no se consulta**; el `contentHash` del lock sí detecta al impostor; un mapeo incompleto falla con `NU1100`. | `dotnet restore --locked-mode` → `NU1403`; `dotnet restore` → `NU1100` | — |
| E4 | `e4-npm-lock` | `package-lock.json` (v3) registra `resolved` + `integrity` y los binarios opcionales de **todas** las plataformas con `os`/`cpu`; `npm ci` rechaza la deriva y el `integrity` alterado; pnpm congela por defecto con `CI=true`. | `npm ci` → `EUSAGE`, `EINTEGRITY`; `pnpm install` (CI) → `ERR_PNPM_OUTDATED_LOCKFILE` | `npm install` |
| E5 | `e5-python-lock` | `uv.lock` es universal (incluye `colorama` sólo para Windows); `uv pip compile` sin `--universal` lo omite; el modo hash de pip rechaza contenido alterado; `uv export --format pylock.toml` y `pip install -r pylock.toml` (experimental); `pip lock` produce un lock sólo para la plataforma actual; Poetry rechaza un lock desactualizado. | `uv lock --check`, `uv sync --locked`, `pip install --require-hashes`, `poetry check --lock`, `poetry install` | `uv sync`, `uv run` (`uv lock` lo regenera a pedido) |

## Advertencias de reproducción

- Los proyectos de este laboratorio declaran **rangos** y **no** traen lock commiteado: el lock se genera en cada corrida a propósito, para mostrar su nacimiento. Por eso las versiones resueltas dependen del día en que se corra (por ejemplo, `tqdm` resolvió `4.70.1` el 2026-10-07). En un repositorio real el lock se commitea.
- E3 empaqueta el mismo código dos veces; el `contentHash` del `.nupkg` cambió entre corridas del 2026-10-07 aunque la fuente era idéntica. La causa no se investigó en este núcleo (ver N6, determinismo de artefactos).
- Ningún experimento corrió en Windows. Lo que la guía afirma sobre Windows proviene de la documentación citada en la bitácora de N4, no de estos experimentos.

## Salidas registradas

Las salidas completas de las corridas del 2026-10-07 están en [`salidas/`](salidas/) (`e1-nuget-lock.txt` … `e5-python-lock.txt`). Se copiaron acá en la fase de integración para que la guía pueda citarlas con una ruta relativa al laboratorio.

## Ampliaciones de la revisión (2026-10-09)

- **E4-2, paso 5.** Cuatro variantes con caché fría: con lock (sin aviso; `requested [8.0.2, )`, `resolved 9.0.0`), sin lock (`warning NU1603`), sin lock con `NU1603` como error (rc=1) y con lock con `NU1603` como error (rc=0: el lock desarma el aviso). SDK 8 en `salidas/e2-nuget-cpm-pinning.txt`; SDK 10 en `salidas/e2-nuget-cpm-pinning-sdk10.txt` (`docker run … mcr.microsoft.com/dotnet/sdk:10.0 bash /src/run.sh`).
- **E4-4, pnpm y Yarn.** `e4-npm-lock/multiplataforma-pnpm-yarn.sh` (en `node:24-bookworm-slim`): `pnpm-lock.yaml` (pnpm 10.18.0) y `yarn.lock` (Yarn 4.10.3) registran las 26 variantes de `@esbuild/*` con su `cpu`/`os` e instalan sólo `linux-x64` (`salidas/e4-pnpm-yarn-multiplataforma.txt`).
