# Anexo A — Plantillas comentadas, verificadas en conjunto

[`repo/`](repo/) es un repositorio mínimo que contiene **todas** las plantillas del Anexo A de la guía (`Metodologia-Entorno-Desarrollo-Guide.md`), en las rutas donde las buscan sus herramientas. [`verificar.sh`](verificar.sh) (experimento **EA-1**) comprueba que funcionan juntas: copia `repo/` a un directorio temporal y corre siete pasos (con dos subpasos), cada uno en el contenedor de su ecosistema.

| Paso | Qué comprueba | Contenedor |
|---|---|---|
| 1 | Qué resuelve `.editorconfig` y qué aplica `.gitattributes` para nueve rutas de ejemplo; la compuerta de texto de N2 sobre las propias plantillas | `lab-n2-texto:local` (ver `../N2-politica-de-texto-y-remediacion/tools/Dockerfile`) |
| 1b | Un `dotnet new console` dentro del repo: con la variante A, dos violaciones (`Program.cs` y el `.csproj` con BOM); ninguna después de normalizarlos | `lab-n2-texto:local` |
| 2 | La condición de CI de `Directory.Build.props` (sin variable, `CI=true`, `TF_BUILD=True`) | `mcr.microsoft.com/dotnet/sdk:8.0` |
| 2b | `Directory.Build.targets`: lock en el ejecutable y no en una biblioteca; lock sin salto final; modo estricto en CI y su falla `NU1004` | `mcr.microsoft.com/dotnet/sdk:8.0` |
| 3 | `devEngines` + `engine-strict` en Node 24 (pasa) y Node 22 (falla) | `node:24-bookworm-slim`, `lab-n8-poliglota:local` |
| 4 | `pyproject.toml` + `.python-version` + `required-version` con uv | `lab-n8-poliglota:local` |
| 5 | Sintaxis de `devcontainer.json` (JSONC; `build.dockerfile` y `PYTHON_VERSION` coherentes con el repo), JSON, `ci.yml` y esquema de `dependabot.yml` (con el ecosistema `docker`) | `python:3.12-slim` |
| 6 | La imagen de A.10 (`imagen/Dockerfile`, Node 24) se construye y corre la plantilla `web/` adentro | `docker build` local |
| 7 | El hook de A.11 con el esqueleto de compuerta rechaza un commit con un `.cs` en cp1252 (el esqueleto falla a propósito hasta reemplazarlo) | `lab-n2-texto:local` |

```bash
bash verificar.sh | tee salidas/EA-1-plantillas.txt
```

Salida registrada el 2026-10-09 (revisión de la mesa, ciclo 1; la primera, del 2026-10-07): [`salidas/EA-1-plantillas.txt`](salidas/EA-1-plantillas.txt). Lo que **no** verifica: el workflow de CI y `dependabot.yml` sólo se validan por sintaxis y esquema (no se ejecutaron en GitHub); `tools/ci/compuerta.sh` es un esqueleto que **falla a propósito** (la compuerta completa es la de `../N7-compuertas-y-gobierno/compuerta/`, que encuentra los proyectos en la raíz o en subcarpetas).

## Ampliaciones de la revisión (ciclo 2, 2026-10-09)

`verificar.sh` corre ahora los pasos 1 a 10 (para el paso 10 necesita Node, con `npx`, en el host). Las variables `EXAMPLES` y `COMPUERTA` permiten apuntarlo a otra copia del laboratorio o a otra compuerta; por defecto usa la de N7.

| Paso | Qué comprueba | Contenedor |
|---|---|---|
| 1c | La variante B de A.1 (`[*.{cs,csproj,props,targets}]` con `charset = utf-8-bom`) sobre una solución creada por el SDK y convertida con BOM: 2 violaciones con los `Directory.Build.*` sin BOM y 0 con ellos guardados con BOM | `lab-n2-texto:local` |
| 2c | Un proyecto WinForms (`WinExe`, `EnableWindowsTargeting`) recibe `packages.lock.json` con la condición de A.4b | `mcr.microsoft.com/dotnet/sdk:8.0` |
| 8 | La compuerta completa (N7) dentro de la imagen de A.10, sobre las plantillas con sus locks: VERDE; un `.cs` en cp1252 → ROJO (`¿cp1252?`); `Microsoft.Extensions.Primitives 8.0.2` → sin `NU1603` y ROJO por `pide [8.0.2, ) y resolvió 9.0.0`; `web/package.json` sin regenerar su lock → ROJO | imagen de A.10 (`ea1-imagen:local`) |
| 9 | Los `run:` de A.12 en la misma imagen: `SOURCE_DATE_EPOCH` por `GITHUB_ENV`, build y registro del entorno de A.17 (`tools/ci/registrar-entorno.sh`) | imagen de A.10 |
| 10 | El devcontainer de A.9 con `@devcontainers/cli` 0.89.0 en host Linux: `"outcome":"success"` y `core.hooksPath=.githooks` | Docker, desde el host |

`repo/` suma `tools/ci/registrar-entorno.sh` (A.17, ejecutable) y `.gitignore` (A.18). Salida del 2026-10-09: [`salidas/EA-1-plantillas.txt`](salidas/EA-1-plantillas.txt).
