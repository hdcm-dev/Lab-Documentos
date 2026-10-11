# Laboratorio de la guía «Metodología del entorno de desarrollo»

Experimentos ejecutables que respaldan las afirmaciones de [`../Metodologia-Entorno-Desarrollo-Guide.md`](../Metodologia-Entorno-Desarrollo-Guide.md). Todo corre en contenedores Docker: no hace falta instalar .NET, Node.js ni Python en el host. Las salidas de la corrida del 2026-10-07 están versionadas en la carpeta `salidas/` de cada experimento (en N1 y N3, también en su `README.md`).

## Cómo leer los identificadores

La guía cita cada experimento con un ID `E<n>-<m>`, donde `<n>` es el capítulo-núcleo que lo produjo. Algunos núcleos usaron otra numeración interna en sus scripts; la tabla dice a qué carpeta corresponde cada ID.

| Carpeta | Tema (capítulo de la guía) | IDs en la guía | Cómo correr todo |
|---|---|---|---|
| [`N0-modelo-de-capas-y-reproducibilidad/`](N0-modelo-de-capas-y-reproducibilidad/) | Modelo de capas (§3) | E0-1 … E0-6, E0-2b, E0-5b | `bash correr-todo.sh` |
| [`N1-codificacion-y-finales-de-linea/`](N1-codificacion-y-finales-de-linea/) | Codificación y EOL (§4) | E1-01 … E1-13 (en los scripts, `E1-N1-01` …) | `sh correr-todo.sh` |
| [`N2-politica-de-texto-y-remediacion/`](N2-politica-de-texto-y-remediacion/) | Política de texto (§5) | E2-1 … E2-8 (E2-8 es un relevamiento fechado: no se re-corre) | `bash correr-todo.sh` |
| [`N3-fijacion-del-toolchain/`](N3-fijacion-del-toolchain/) | Toolchain (§6) | E3-1 … E3-7 (en la carpeta, `01-…` a `06-…`; E3-7 es una inspección de imágenes del host) | `sh correr.sh` en cada subcarpeta |
| [`N4-congelamiento-de-dependencias/`](N4-congelamiento-de-dependencias/) | Dependencias (§7) | E4-1 … E4-5 (carpetas `e1-…` a `e5-…`) | ver su `README.md` |
| [`N5-entornos-en-contenedores/`](N5-entornos-en-contenedores/) | Contenedores (§8) | E5-1 … E5-7, E5-5b | `bash eN-…/experimento.sh` |
| [`N6-procedencia-y-trazabilidad/`](N6-procedencia-y-trazabilidad/) | Procedencia (§9) | E6-1 … E6-6, E6-2b | `bash eN-…/experimento.sh` |
| [`N7-compuertas-y-gobierno/`](N7-compuertas-y-gobierno/) | Compuertas y gobierno (§10) | E7-1 … E7-7, E7-5b (tiempo de la compuerta; en los archivos, `V7-01`) | `bash correr-todo.sh` |
| [`N8-integracion-poliglota/`](N8-integracion-poliglota/) | Solución políglota (§11) | E8-1 … E8-6 | `bash eN-…/experimento.sh` |
| [`A-plantillas/`](A-plantillas/) | Anexo A: todas las plantillas juntas | EA-1 | `bash verificar.sh` |

## Requisitos comunes

- Docker con buildx; acceso a `mcr.microsoft.com`, Docker Hub, `ghcr.io`, nuget.org, el registro de npm y PyPI.
- Si las credenciales de Docker Hub guardadas en el host están vencidas, `export DOCKER_CONFIG=$(mktemp -d)` permite descargar en modo anónimo.
- Algunas carpetas construyen imágenes propias a partir de imágenes oficiales fijadas por digest (`lab-n2-texto:local`, `lab-n7-*:local`, `lab-n8-poliglota:*`); cada `README.md` trae el comando `docker build`.
- Ningún experimento corrió en Windows: lo que la guía dice de Windows y Visual Studio proviene de documentación y está rotulado.

## Qué no se versiona

El [`.gitignore`](.gitignore) excluye `bin/`, `obj/`, `node_modules/`, `.venv/` y `__pycache__/`. N1 y N2 excluyen además sus carpetas `work/`, que contienen repositorios de juguete regenerados en cada corrida.

## Ampliaciones de la revisión (ciclo 2, 2026-10-09)

- **EA-1** (`A-plantillas/verificar.sh`) pasa de siete a diez pasos: variante B (1c), lock de un `WinExe` (2c), la compuerta completa en la imagen de A.10 (8), el registro del entorno de A.17 (9) y el devcontainer de A.9 con la CLI de referencia (10). El paso 10 necesita Node (`npx`) en el host.
- **Compuerta con descubrimiento.** `N7-compuertas-y-gobierno/compuerta/compuerta.sh` busca `.python-version` y los locks en todo lo versionado y compara `requested` con `resolved`; nombra el paquete EditorConfig de Python si falta (caso (d) de `compuerta/probar-prerrequisitos.sh`). Las salidas de N7 se regeneraron con veredictos iguales.
- **N8.** `e6-sbom-unificado/comando-de-la-guia.sh` usa `--provenance=mode=max,version=v1`; `solucion/registrar-entorno.sh` escribe `imagen=NO DECLARADA (…)` si la CI no pasa `IMAGEN_DIGEST`; el comentario de `ConstruirPoliglota` en `solucion/src/Saludo/Saludo.csproj` nombra sus dos usos. E8-3 se volvió a correr: misma DLL (`4dd5d90f…`).
- Para que `buildx` no cambie de versión con un `DOCKER_CONFIG` temporal, enlazar los *plugins*: `D=$(mktemp -d); ln -s ~/.docker/cli-plugins "$D/"; export DOCKER_CONFIG=$D`.

## Ampliaciones del ciclo 3 (2026-10-09)

- **E5-8** (`N5-entornos-en-contenedores/e8-capa4-y-artefacto/`): el mismo commit con el mismo SDK da la misma DLL y el mismo PDB en tres bases Linux y tres rutas; con otro SDK, otra DLL. Salida en `N5-entornos-en-contenedores/salidas/E5-8-capa4-y-artefacto.txt`.
- **E1-11 con Node** (`N1-codificacion-y-finales-de-linea/e1-shebang-node.mjs`): los scripts de E1-11 lanzados con `child_process.spawnSync`; salida en `N1-codificacion-y-finales-de-linea/salidas/e1-shebang-node.txt`.
- **E2-6 ampliado** (`N2-politica-de-texto-y-remediacion/e6-plantillas-dotnet/run.sh`): `.csproj` y solución con BOM antes de editarlos, plantillas de ítem (`class`, `interface`, `razorcomponent`) y `dotnet format whitespace` sobre un `.razor` sin BOM. Sólo se volvió a correr el bloque de E2-6 de `correr-todo.sh`; la salida previa no cambia, se le agregan renglones.
- **EA-1** (`A-plantillas/verificar.sh`): el paso 1c incluye `Directory.Packages.props`; el paso 8 suma las plantillas sin los locks de npm y uv (`n/a`, VERDE: el límite declarado); el paso 9b guarda el workflow con otro nombre (el registro falla) y lo pasa con `WORKFLOW_FILE`. Las plantillas de `A-plantillas/repo/` siguen al Anexo A de la guía 1.1.0.
