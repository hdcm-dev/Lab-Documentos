# N7 — Compuertas, gobierno y actualización controlada (laboratorio)

Experimentos ejecutables del núcleo N7. Demuestran la **mecánica** que mantiene en el tiempo lo que N2 (texto), N3 (toolchain) y N4 (dependencias) declaran: una sola compuerta que corre igual en el hook local, en la CI y en el servidor; qué puede saltearse y qué no; cómo se descongela una versión sin perder trazabilidad; y qué hacer cuando la plataforma no ofrece protección de rama.

Todo se ejecutó el **2026-10-07** en un host Linux con Docker. Las salidas reales están en [`salidas/`](salidas/). Los repositorios de juguete se crean en `work/` (ignorado por `.gitignore`) y `correr-todo.sh` los borra al terminar: no quedan artefactos de build.

## Índice

- [Piezas compartidas](#piezas-compartidas)
- [Cómo correr](#cómo-correr)
- [Experimentos](#experimentos)
- [Imágenes usadas](#imágenes-usadas)
- [Límites](#límites)

## Piezas compartidas

| Archivo | Qué es |
|---|---|
| [`compuerta/compuerta.sh`](compuerta/compuerta.sh) | **La compuerta única.** Tres pasos: texto (reutiliza `check_text_policy.py` del laboratorio N2), toolchain (`global.json`, `.nvmrc`, `.python-version`) y grafo congelado (`dotnet restore --locked-mode` y, si pasó, `requested` = `resolved`; `npm ci`; `uv lock --check`). `global.json` y `.nvmrc` los busca en la raíz; `.python-version` y los locks, en todo lo versionado (`git ls-files`), así que sirve con los proyectos en la raíz o en subcarpetas. Un paso sin archivo es `n/a`; un paso con archivo y sin herramienta **falla**. |
| [`compuerta/pre-commit`](compuerta/pre-commit) | Hook versionado en `.githooks/`: sólo delega en la compuerta con `--staged`, con el layout de N7 (`tools/compuerta.sh`); en el de A.11 es `tools/ci/`. |
| [`compuerta/ci-compuerta.yml`](compuerta/ci-compuerta.yml) | Workflow de ejemplo (no se ejecuta acá): la misma compuerta en la CI, acciones fijadas por SHA. |
| [`compuerta/sembrar-repo.sh`](compuerta/sembrar-repo.sh) | Crea un repo .NET de juguete con el entorno declarado: política de texto N2, `global.json` exacto con `rollForward: disable`, `packages.lock.json` y la compuerta. |
| [`comun.sh`](comun.sh) | Funciones de los scripts (`preparar`, `rc`, `codif`). |

## Cómo correr

Desde esta carpeta, en el host:

```bash
bash correr-todo.sh
```

Construye las imágenes que falten, corre los siete experimentos y reescribe `salidas/`. Para correr uno solo (ejemplo E7-01):

```bash
EX=$(cd .. && pwd)
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 \
  -v "$EX":/ex -w /ex/N7-compuertas-y-gobierno lab-n2-texto:local \
  bash 01-misma-compuerta-local-y-ci/run.sh
```

Se monta toda la carpeta `Examples/` porque la compuerta reutiliza el verificador de texto y la política del N2. Si Docker Hub rechaza el pull por credenciales vencidas, alcanza con `DOCKER_CONFIG=$(mktemp -d) docker pull <imagen>`.

## Experimentos

| ID | Carpeta | Qué demuestra | Resultado observado |
|---|---|---|---|
| E7-01 | [`01-misma-compuerta-local-y-ci/`](01-misma-compuerta-local-y-ci/) | La misma compuerta en el hook y en la CI. Al clonar, git **no** activa los hooks; `--no-verify` los saltea; un remoto sin control acepta el push; la CI, sobre un clon limpio, da el mismo diagnóstico que el hook. | Hook inactivo tras clonar: commit cp1252 entra (rc 0). Con `core.hooksPath`: rechazado (rc 1, `byte 0xf1`). `--no-verify` + push: rc 0. CI: `resultado=ROJO`, misma línea. Tras pasar a UTF-8: VERDE en los dos. |
| E7-02 | [`02-pre-commit-y-politica/`](02-pre-commit-y-politica/) | Un framework de hooks genérico (pre-commit 4.6.2 + pre-commit-hooks v6.0.0) puede **contradecir** la política declarada. | `fix-byte-order-marker` quita el BOM de un `.razor` que la política quiere con BOM y la compuerta lo reclama. `end-of-file-fixer` agrega un salto final a `packages.lock.json` que NuGet vuelve a quitar al regenerarlo (ida y vuelta). Con `exclude` alineados a `.editorconfig`: todo pasa; un `.razor` nuevo sin BOM lo ataja la compuerta, no el hook genérico. |
| E7-03 | [`03-revisor-cambios-de-entorno/`](03-revisor-cambios-de-entorno/) | Lista de verificación del revisor generada desde el diff ([`cambios-de-entorno.py`](03-revisor-cambios-de-entorno/cambios-de-entorno.py)) y [`CODEOWNERS`](03-revisor-cambios-de-entorno/CODEOWNERS) de ejemplo. | PR A: clasifica `.gitattributes`, lock y manifiesto; detecta que `Program.cs` cambió **sólo el BOM** (el diff web muestra la línea entera cambiada). PR B: «lock cambió sin su manifiesto»; la compuerta falla con `NU1403` (hash de contenido). PR C: SDK subido sin imagen; compuerta `A compatible .NET SDK was not found`. |
| E7-04 | [`04-descongelamiento-controlado/`](04-descongelamiento-controlado/) | Descongelar a propósito. Paquete: declarar sin regenerar falla; manifiesto + lock en el mismo commit pasa. SDK: `global.json` + receta de la imagen de CI + ADR en el mismo PR. Se corre desde el host: `bash 04-descongelamiento-controlado/correr.sh`. | A1 `NU1004 … changed from [13.0.3, ) to [13.0.4, )` → ROJO. A2 VERDE, commit con `App.csproj` + `packages.lock.json`. B1 imagen vieja: ROJO (SDK 10.0.400 no encontrado). B2 imagen nueva: VERDE. B3 commit con `ci/Dockerfile`, `docs/adr/0002-sdk-10.md`, `global.json`, con el hook en VERDE. |
| E7-05 | [`05-sin-proteccion-de-rama/`](05-sin-proteccion-de-rama/) | Sin protección de rama la CI **avisa pero no impide**. Dónde sí se impide: el servidor ([`pre-receive`](05-sin-proteccion-de-rama/pre-receive)) y el acto irreversible ([`publicar.sh`](05-sin-proteccion-de-rama/publicar.sh)). | Push roto a main aceptado; CI ROJO después. `publicar.sh` se niega sobre main en rojo, publica `v1.0.0` tras la corrección y se niega a reescribirla. Con `pre-receive`: push rechazado (`pre-receive hook declined`), main intacto; un push que cumple se acepta. |
| E7-06 | [`06-actualizacion-automatica/`](06-actualizacion-automatica/) | [`dependabot.yml`](06-actualizacion-automatica/dependabot.yml) y [`renovate.json`](06-actualizacion-automatica/renovate.json) válidos contra sus esquemas publicados. Sólo valida sintaxis: el comportamiento del bot no se puede correr fuera de la plataforma. | Ambos `VÁLIDO`; control negativo (ecosistema inexistente + `cooldown` no numérico): 2 errores detectados. |
| E7-07 | [`07-misma-compuerta-otros-ecosistemas/`](07-misma-compuerta-otros-ecosistemas/) | El mismo `compuerta.sh` en un repo Node y en uno Python (monolenguaje), cada uno en su imagen. | Node: VERDE; con `package.json` cambiado sin lock, `npm ci` → `EUSAGE … are in sync` → ROJO. Python: VERDE; con `pyproject.toml` cambiado, `uv lock --check` → `needs to be updated` → ROJO. |

## Imágenes usadas

| Imagen | Origen | Versión observada |
|---|---|---|
| `lab-n2-texto:local` | `../N2-politica-de-texto-y-remediacion/tools/Dockerfile` (base `dotnet/sdk:8.0@sha256:78235e09…`) | SDK 8.0.425, EditorConfig 0.17.1 |
| `lab-n7-ci-sdk10:local` | `04-descongelamiento-controlado/imagen-ci/Dockerfile` (base `dotnet/sdk:10.0@sha256:e1ffd2a9…`) | SDK 10.0.400 |
| `lab-n7-node:local` | `07-…/Dockerfile.node` (base `node:24@sha256:3d27e5c1…`) | Node 24.21.0 |
| `lab-n7-python:local` | `07-…/Dockerfile.python` (base `python:3.12-slim@sha256:78387bc3…`) | Python 3.12.14, uv 0.12.23 |
| `python:3.12-slim` | Docker Hub | jsonschema 4.26.0, PyYAML 6.0.3 |

## Límites

- La compuerta de E7-01…E7-05 restaura paquetes de nuget.org: necesita red. En un `pre-receive` real eso puede ser lento: GitHub Enterprise Server da a todos los `pre-receive` de un repositorio un presupuesto acumulado de 5 s y desaconseja llamar servicios externos. [`05-sin-proteccion-de-rama/medir-tiempo.sh`](05-sin-proteccion-de-rama/medir-tiempo.sh) midió 1,5–2,3 s en el repo de juguete (`salidas/V7-01-tiempo-de-la-compuerta.txt`); un repo real tarda más.
- El workflow `ci-compuerta.yml` y la configuración de los bots no se ejecutaron: sus afirmaciones se apoyan en la documentación oficial citada en la bitácora del N7.
- `cambios-de-entorno.py` es un **asistente** del revisor: clasifica por nombre de archivo y no entiende la semántica del cambio (por ejemplo, cuenta `global.json` como explicación válida de un lock regenerado).

## Ampliaciones de la revisión (2026-10-09)

- **Falta de herramienta ≠ violación.** `compuerta/compuerta.sh` comprueba `python3` y `check_text_policy.py` antes del paso de texto y, si faltan, lo dice (`falta python3 (prerrequisito de la compuerta, no una violación)`). `compuerta/probar-prerrequisitos.sh` lo muestra en un repo de juguete: `salidas/e7-prerrequisitos-de-la-compuerta.txt`. Los siete experimentos se volvieron a correr con la compuerta modificada y dieron los mismos resultados; las salidas versionadas son las del 2026-10-07, que la guía cita.
- **Alcance.** La compuerta busca los archivos nativos y los locks en la raíz del repo: sirve para un proyecto por ecosistema en la raíz. Con proyectos en subcarpetas, como la solución de N8, hace falta un inventario explícito (`../N8-integracion-poliglota/solucion/verificar.sh`).

## Ampliaciones de la revisión (ciclo 2, 2026-10-09)

- **Compuerta con descubrimiento.** `compuerta/compuerta.sh` busca `.python-version` y los locks en todo lo versionado (`git ls-files`) y, después de un `dotnet restore --locked-mode` exitoso, compara la versión pedida con la resuelta de cada paquete directo del lock (`requested` = `resolved`): si difieren, el paso de grafo falla con «resolvió otra versión que la pedida». El caso se ve en `../A-plantillas/salidas/EA-1-plantillas.txt`, paso 8.
- **Caso (d) de los prerrequisitos.** Sin el paquete EditorConfig de Python, la compuerta lo nombra como prerrequisito (`falta el paquete EditorConfig de Python (EditorConfig==0.17.1; prerrequisito, no una violación)`), rc=1: `compuerta/probar-prerrequisitos.sh` y `salidas/e7-prerrequisitos-de-la-compuerta.txt`.
- **Regresión.** `bash correr-todo.sh` se volvió a correr con la compuerta nueva: los veredictos (`resultado=`) y los códigos de salida de E7-01 a E7-07 son iguales a los anteriores; cambian sólo los hashes de commit de los repos de juguete, los tiempos de V7-01 y una línea `[ok] … requested = resolved` en E7-01.
