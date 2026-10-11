# N3 — Fijación del toolchain: experimentos

Seis experimentos mínimos que muestran qué fija y qué no fija cada archivo de toolchain en .NET, Node.js y Python, y la diferencia entre **declarar** una versión y **hacerla cumplir**. Todos corren en contenedores: no hace falta tener instalado ni .NET, ni Node, ni Python en la máquina, sólo Docker.

Ningún experimento deja artefactos en el repositorio: cada script copia el caso a `/tmp` dentro del contenedor (o a un directorio temporal del host, en el 06) y compila o instala ahí. Los paquetes NuGet descargados se guardan en `~/.cache/n3-nuget` del host para que la segunda corrida sea más rápida; se puede cambiar con la variable `NUGET_CACHE`.

## Requisitos

- Docker con acceso a `mcr.microsoft.com` y a Docker Hub.
- Imágenes usadas: `mcr.microsoft.com/dotnet/sdk:8.0`, `mcr.microsoft.com/dotnet/sdk:10.0`, `node:22-alpine`, `node:24-alpine`, `node:26-alpine`, `python:3.12-slim`, `python:3.11-slim`. Se usan **por etiqueta** a propósito: lo que se mide es justamente qué versión trae cada etiqueta. Las versiones concretas que trajeron el 2026-10-07 están en la tabla de abajo; otra fecha puede dar otro número de parche (eso es la deriva que N5 resuelve fijando por *digest*).
- Red para: el paquete `setuptools` (04-A), `uv` y los intérpretes que uv descarga (04-C a F), `pnpm` que descarga Corepack (03) y el *targeting pack* y `Microsoft.NET.ILLink.Tasks` desde nuget.org (02 y 06).

Versiones observadas al correrlos (2026-10-07):

| Imagen | Herramientas |
|---|---|
| `dotnet/sdk:8.0` | SDK 8.0.425, runtime 8.0.31, compilador C# 4.11.0 |
| `dotnet/sdk:10.0` | SDK 10.0.400, runtime 10.0.11, compilador C# 5.9.0 |
| `node:22-alpine` | Node 22.23.2, npm 10.9.8, Corepack 0.34.6 |
| `node:24-alpine` | Node 24.21.0, npm 11.19.0, Corepack 0.36.0 |
| `node:26-alpine` | Node 26.10.0, npm 11.19.1, **sin Corepack** |
| `python:3.12-slim` | Python 3.12.14, pip 25.0.1 (uv 0.12.23 instalado por el script) |
| `python:3.11-slim` | Python 3.11.16 |

## Cómo correrlos

Cada carpeta tiene un `correr.sh`. Se ejecuta desde esa carpeta:

```sh
cd 01-dotnet-global-json && sh correr.sh
```

## 01 — `global.json` y `rollForward`

**Demuestra:** qué SDK elige `dotnet` según la política `rollForward`, y que si no hay un SDK compatible el comando falla antes de compilar. Siete `global.json` (uno por carpeta) contra dos imágenes con un único SDK cada una.

```sh
cd 01-dotnet-global-json && sh correr.sh
```

Salida obtenida:

```text
##### mcr.microsoft.com/dotnet/sdk:8.0  (instalado: 8.0.425)
exacto                 FALLA rc=145: Requested SDK version: 8.0.100
defecto-misma-banda    usa 8.0.425
defecto-otra-banda     FALLA rc=145: Requested SDK version: 8.0.300
rango-feature          usa 8.0.425
otra-mayor             FALLA rc=145: Requested SDK version: 9.0.100
piso-cualquier-mayor   usa 8.0.425
mensaje-propio         FALLA rc=145: Requested SDK version: 10.0.999
##### mcr.microsoft.com/dotnet/sdk:10.0  (instalado: 10.0.400)
exacto                 FALLA rc=155: Requested SDK version: 8.0.100
defecto-misma-banda    FALLA rc=155: Requested SDK version: 8.0.400
defecto-otra-banda     FALLA rc=155: Requested SDK version: 8.0.300
rango-feature          FALLA rc=155: Requested SDK version: 8.0.100
otra-mayor             FALLA rc=155: Requested SDK version: 9.0.100
piso-cualquier-mayor   usa 10.0.400
mensaje-propio         FALLA rc=155: Este repo exige el SDK 10.0.999. Ver docs/toolchain.md para instalarlo.
```

Lectura: sin `rollForward` el default es `patch`, que acepta otro parche de la **misma banda** (8.0.400 → 8.0.425) pero no salta de banda (8.0.300 ✗). `disable` exige la versión exacta. `latestMajor` no fija nada: es un piso. `errorMessage` sólo lo entiende el SDK 10; el SDK 8 lo ignora y muestra su mensaje genérico.

## 02 — SDK, compilador, versión de C# y runtime

**Demuestra:** que el **SDK fija el compilador**, que el **TFM fija la versión de C# por defecto**, que `LangVersion=latest` hace que el mismo código compile en una máquina y no en otra, y que el **runtime se elige al ejecutar**, no al compilar.

```sh
cd 02-dotnet-sdk-compilador-runtime && sh correr.sh
```

Salida obtenida (resumida):

```text
## A. Mismo proyecto net8.0, dos SDK
--- sdk:8.0   error CS8304: Compiler version: '4.11.0-3.25569.22 (3fb752d4)'. Language version: 12.0.
--- sdk:10.0  error CS8304: Compiler version: '5.9.0-1.26379.115 (...)'. Language version: 12.0.
## B. Sintaxis de C# 14 (c?.Valor = 42)
--- sdk:8.0  (por defecto)        error CS0131: The left-hand side of an assignment must be a variable...
--- sdk:8.0  (LangVersion=latest) error CS0131: ...
--- sdk:10.0 (por defecto)        error CS9202: Feature 'null conditional assignment' is not available in C# 12.0...
--- sdk:10.0 (LangVersion=latest) Build succeeded.
## C. Compilado con SDK 10 para net8.0
runtimeconfig: "framework": { "name": "Microsoft.NETCore.App", "version": "8.0.0" }
runtimes instalados: Microsoft.NETCore.App 10.0.11
--- dotnet out/App.dll
You must install or update .NET to run this application. ... Framework: 'Microsoft.NETCore.App', version '8.0.0'
rc=150
--- con DOTNET_ROLL_FORWARD=Major
Runtime que ejecuta: .NET 10.0.11
--- mismo build con la imagen sdk:8.0 (runtime 8.0.31)
Runtime que ejecuta: .NET 8.0.31
```

El `CS8602` (advertencia de nulabilidad) que aparece en el caso B con SDK 10 es un efecto colateral de usar `c?.` y no cambia la conclusión.

## 03 — `engines`, `engine-strict`, `devEngines`, `packageManager` y Corepack

**Demuestra:** que `engines` sólo **avisa**, que `engine-strict=true` (en `.npmrc`) lo convierte en **error**, que `devEngines` con `onFail: "error"` falla sin configuración adicional, que Corepack hace cumplir `packageManager` sólo para los gestores que intercepta (pnpm y Yarn, no npm), y que Node 26 ya no trae Corepack. `.nvmrc` no lo lee ninguna de estas herramientas.

```sh
cd 03-node-engines-packagemanager && sh correr.sh
```

Salida obtenida (resumida):

```text
### node:22-alpine (node v22.23.2, npm 10.9.8)
engines         npm warn EBADENGINE Unsupported engine ... required: { node: '>=24 <25' } ...  rc=0
engines-strict  npm error code EBADENGINE ... Required: {"node":">=24 <25"} Actual: {"npm":"10.9.8","node":"v22.23.2"}  rc=1
dev-engines     npm error code EBADDEVENGINES ... Invalid semver version ">=24 <25" does not match "v22.23.2" for "runtime"  rc=1
package-manager This project is configured to use pnpm because /tmp/w/package.json has a "packageManager" field  rc=1
                pnpm que entrega corepack: 10.18.0
                npm install (corepack no intercepta npm por defecto): rc=0
### node:24-alpine (node v24.21.0, npm 11.19.0)
engines rc=0 / engines-strict rc=0 / dev-engines rc=0
package-manager (igual que en 22: yarn rechazado, pnpm 10.18.0, npm pasa)
### node:26-alpine (node v26.10.0, npm 11.19.1)
engines         npm warn EBADENGINE ... rc=0
engines-strict  npm error code EBADENGINE ... rc=1
dev-engines     npm error code EBADDEVENGINES ... rc=1
package-manager corepack: no viene con esta versión de Node
--- .nvmrc pide 24; node y npm no lo leen: npm install en node:22 con ese .nvmrc presente
    v22.23.2
    up to date in 408ms
    rc=0
```

## 04 — `requires-python` frente a `.python-version`

**Demuestra:** que `requires-python` es un **rango** que pip hace cumplir al instalar el paquete (y que se puede saltear), que `.python-version` es una **preferencia** del entorno que uv usa para elegir (y descargar) el intérprete, que uv rechaza una preferencia fuera del rango, y que `[tool.uv] required-version` fija la versión del **gestor**.

```sh
cd 04-python-requires-vs-python-version && sh correr.sh
# Otra versión de uv:  UV_VER=0.12.23 sh correr.sh
```

Salida obtenida:

```text
## intérprete de la imagen: Python 3.12.14
--- A. pip install . con requires-python >=3.13
ERROR: Package 'n3pkg' requires a different Python: 3.12.14 not in '>=3.13'
rc=1
--- B. lo mismo con --ignore-requires-python
rc=0
--- C. uv: .python-version=3.13 dentro del rango
uv 0.12.23 (x86_64-unknown-linux-gnu)
 Downloaded cpython-3.13.16-linux-x86_64-gnu (download)
Python 3.13.16
rc=0
--- D. uv: .python-version=3.11 contradice requires-python >=3.13
error: The Python request from `.python-version` resolved to Python 3.11.17, which is incompatible with the project's Python requirement: `>=3.13` (from `project.requires-python`)
rc=2
--- E. uv: .python-version=3.13 pero UV_PYTHON_DOWNLOADS=never
error: No interpreter found for Python 3.13 in managed installations or search path
hint: A managed Python download is available for Python 3.13, but Python downloads are set to 'never'
rc=2
--- F. uv: [tool.uv] required-version
error: Required uv version `==0.1.0` does not match the running version `0.12.23`. ...
rc=2
```

Nótese en C que `.python-version` con `3.13` no fija el parche: uv eligió 3.13.16, el más reciente que tenía disponible.

## 05 — Un chequeo único de toolchain

**Demuestra:** cómo pasar de declarar a hacer cumplir con un único script (`repo/verificar-toolchain.sh`) que lee `global.json`, `.nvmrc` y `.python-version`, compara con lo instalado, falla ante cualquier desvío e imprime una línea `toolchain:` que la CI puede guardar como registro. El mismo script sirve en local, en el devcontainer y en la CI.

```sh
cd 05-chequeo-de-toolchain && sh correr.sh
```

Salida obtenida (se omiten las líneas `toolchain:` de los casos Node y Python):

```text
=== mcr.microsoft.com/dotnet/sdk:8.0
OK   dotnet sdk 8.0.425 (global.json)
toolchain: dotnet-sdk=8.0.425
rc=0
=== node:24-alpine
OK   node 24.21.0 (.nvmrc=24)
rc=0
=== node:22-alpine
FALLA node 22.23.2 no coincide con .nvmrc=24
rc=1
=== python:3.12-slim
OK   python 3.12.14 (.python-version=3.12)
rc=0
=== python:3.11-slim
FALLA python 3.11.16 no coincide con .python-version=3.12
rc=1
```

## 06 — El SDK cambia el lock file de NuGet

**Demuestra:** por qué la documentación de `global.json` recomienda `rollForward: disable` cuando se usan lock files. Un proyecto **sin ninguna `PackageReference`**, con `IsTrimmable`, recibe del SDK un paquete implícito (`Microsoft.NET.ILLink.Tasks`) cuya versión depende del SDK. El lock generado con un SDK no pasa `--locked-mode` con otro.

```sh
cd 06-dotnet-sdk-y-lock && sh correr.sh
```

Salida obtenida:

```text
--- 1. Genera packages.lock.json con el SDK 8
8.0.425
rc=0
      "Microsoft.NET.ILLink.Tasks": {
        "type": "Direct",
        "requested": "[8.0.31, )",
        "resolved": "8.0.31",
--- 2. Restauración estricta con el mismo SDK 8
rc=0
--- 3. Restauración estricta con el SDK 10 (mismo lock, mismo código)
10.0.400
/w/Lib.csproj : error NU1004: The package reference Microsoft.NET.ILLink.Tasks version has changed from [8.0.31, ) to [8.0.30, ). The packages lock file is inconsistent with the project dependencies so restore can't be run in locked mode. ...
rc=1
```

El aviso `An issue was encountered verifying workloads` que imprime el SDK 8 al correr como usuario no root no afecta el resultado.

## Ampliaciones de la revisión (2026-10-09)

- **Salidas versionadas.** `sh correr-todo.sh` corre los seis `correr.sh` y deja la salida completa de cada uno en `salidas/E3-<m>.txt`.
- **E3-2, compilador en Visual Studio.** `02-dotnet-sdk-compilador-runtime/inspeccion-targets.sh` inspecciona los *targets* de las imágenes oficiales: el SDK 8.0.425 sólo trae `BuildWithNetFrameworkHostedCompiler` (ninguna selección automática); el 10.0.400 elige `Core` con `DOTNET_HOST_PATH`, el paquete *toolset* si el MSBuild es anterior a 18.9 (`_IsDisjointMSBuildVersion`) y, si no, el compilador del IDE; `Microsoft.Net.Sdk.Compilers.Toolset` no tiene versiones 8.x en nuget.org. No reemplaza una prueba en Visual Studio.
- **E3-3, pnpm sin Corepack.** Caso `pnpm-pm-on-fail/`: pnpm 12.10.1 instalado con npm, con `packageManager: pnpm@10.18.0`, usa 10.18.0 (`pmOnFail` por defecto, `download`) y con `--pm-on-fail=error` falla con `ERR_PNPM_BAD_PM_VERSION` (rc=1).
