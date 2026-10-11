# N2 — Política de texto del repositorio y remediación de código heredado

Laboratorio del núcleo N2 de la guía *Metodología del entorno de desarrollo*. Cada experimento crea repositorios git de juguete en `work/` (ignorado por `.gitignore`), los manipula y deja la salida real en `salidas/`. El código heredado es **sintético**: un proyecto .NET mínimo con archivos `.cs` y `.razor` guardados en Windows-1252 (cp1252) con CRLF, como los que produce una estación Windows sin política declarada. No reproduce ningún repositorio real.

## Índice

- [Qué hay en la carpeta](#qué-hay-en-la-carpeta)
- [Cómo correrlo](#cómo-correrlo)
- [Experimentos](#experimentos)
- [Versiones con que se obtuvieron las salidas](#versiones-con-que-se-obtuvieron-las-salidas)

## Qué hay en la carpeta

| Ruta | Qué es |
|---|---|
| `politica/.editorconfig` | Plantilla de política: UTF-8 sin BOM y LF por defecto; BOM en `*.razor`, `*.cshtml` y `*.sln`; CRLF en `*.cmd`/`*.bat`; variante B comentada para `*.cs` con BOM. |
| `politica/.gitattributes` | Plantilla de normalización en git: `* text=auto eol=lf`, excepciones CRLF y binarios explícitos. |
| `tools/check_text_policy.py` | Compuerta: lee la política del propio `.editorconfig` (implementación de referencia `EditorConfig` 0.17.1) y revisa el **índice** de git: UTF-8 válido, BOM según `charset`, LF en el índice. Modos `--staged` (hook) y `--base <rev>` (PR). |
| `tools/migrar-a-utf8.sh` | Migración mecánica: `iconv` desde la codificación de origen para lo que no es UTF-8 válido, BOM según `.editorconfig`, `git add --renormalize .`. No commitea. |
| `tools/verificar-commit-mecanico.sh` | Comprueba que un commit de migración sólo cambió codificación, BOM y EOL; falla si se coló un cambio de contenido. |
| `tools/Dockerfile` | Imagen del laboratorio: `dotnet/sdk:8.0` fijada por digest + Python + `EditorConfig==0.17.1`. |
| `e1-…` a `e6-…` | Un `run.sh` por experimento (E2-1 a E2-6 en la bitácora del núcleo). |
| `correr-todo.sh` | Construye la imagen y corre los seis experimentos (en el host, requiere docker). |
| `salidas/` | Salida real de la última corrida. |

## Cómo correrlo

Todo corre en contenedores: no hace falta tener .NET, Node ni Python en el host.

```bash
cd Examples/N2-politica-de-texto-y-remediacion
./correr-todo.sh                     # los seis, ~1.5 min con las imágenes ya bajadas
```

Uno por uno:

```bash
docker build -t lab-n2-texto:local tools/
# E2-1..E2-4 dentro de la imagen del laboratorio
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab lab-n2-texto:local bash e1-precedencia/run.sh
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab lab-n2-texto:local bash e2-migracion/run.sh
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab lab-n2-texto:local bash e3-compuerta/run.sh
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab lab-n2-texto:local bash e4-working-tree-encoding/run.sh
# E2-5 en el host: lanza un contenedor por ecosistema (node, python, dotnet)
bash e5-bom-por-lector/run.sh
# E2-6 con cada SDK
for v in 8.0 10.0; do
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab mcr.microsoft.com/dotnet/sdk:$v bash e6-plantillas-dotnet/run.sh
done
```

## Experimentos

| ID | Carpeta | Pregunta | Resultado observado (ver `salidas/`) |
|---|---|---|---|
| E2-1 | `e1-precedencia` | ¿Quién manda entre `core.autocrlf`, `.gitattributes` y `.editorconfig`? | Sin atributos, el mismo archivo CRLF entra como `i/crlf` o `i/lf` según el `core.autocrlf` de quien commitea. Con `* text=auto eol=lf`, el checkout da LF con cualquier `core.autocrlf`/`core.eol`, y `*.cmd eol=crlf` da CRLF. Con `* text=auto` sin `eol`, el índice queda en LF pero el working tree depende de la máquina. `.editorconfig` no cambia nada en git. Un literal verbatim de C# compila a `61-0A-62` con LF y a `61-0D-0A-62` con CRLF. |
| E2-2 | `e2-migracion` | ¿Cómo se migra un heredado cp1252/CRLF sin mezclar ni ensuciar `blame`? | Antes: `error CS1056: Unexpected character` en Linux. Commit de política (2 archivos) → commit mecánico (7 archivos, verificado byte a byte) → `.git-blame-ignore-revs`. Después: compila y escribe `Atención…`/`Señal…` bien. `git blame` vuelve a atribuir las líneas a sus autores. Un `--amend` cambia el SHA y rompe el aislamiento. La contraprueba con un cambio colado se rechaza. |
| E2-3 | `e3-compuerta` | ¿Cómo se detecta en un PR que volvió cp1252, un BOM o CRLF? | La compuerta en modo `--base main` informa las 4 regresiones y sale con 1; como hook `pre-commit` rechaza el commit, pero `--no-verify` lo saltea. Un archivo que ya estaba con CRLF en git sigue `i/crlf` aunque exista `text=auto`. Con la variante B, el `.cs` sin BOM pasa a ser violación. |
| E2-4 | `e4-working-tree-encoding` | ¿Cuándo sirve `working-tree-encoding`? | Hace que un `.ps1` UTF-16LE tenga diff de texto (sin él: `Binary files differ`). Sobre un `.cs` en cp1252 el índice queda UTF-8 pero el working tree de cualquier clon vuelve a cp1252 y la build en Linux falla igual. |
| E2-5 | `e5-bom-por-lector` | ¿Qué hace un BOM en archivos de configuración y scripts? | Shebang con BOM: `Exec format error` por `execve`; `sh` lo "rescata" ignorando la línea. `JSON.parse` (Node), `json.load(encoding='utf-8')` y `tomllib` (Python) y `JsonDocument.Parse(bytes)` (.NET) fallan; `require()`, `npm run`, `utf-8-sig` y `File.ReadAllText` lo toleran. Un `.csproj` con BOM compila. |
| E2-6 | `e6-plantillas-dotnet` | ¿Con qué BOM/EOL escribe el propio SDK? ¿`dotnet format` hace cumplir `charset`? | Las plantillas no son homogéneas: `*.razor` y `*.cshtml` siempre con BOM, `Program.cs` con o sin BOM según la plantilla, muchas con CRLF incluso en Linux. `dotnet new editorconfig` del SDK 8 declara `end_of_line = crlf` para `*.cs`; el del SDK 10 ya no. `dotnet sln add` (SDK 8) vuelve a poner BOM al `.sln`; sin BOM, el `.slnx` y el `.csproj` se respetan; con BOM, el `.csproj` lo conserva y el `.slnx` (SDK 10) lo pierde; las plantillas de ítem crean con BOM y `dotnet format whitespace` no se lo repone a un `.razor`. `dotnet format whitespace --verify-no-changes` falla con `CHARSET` en ambos sentidos y `dotnet format whitespace` lo corrige. |

## Versiones con que se obtuvieron las salidas

Corrida del 2026-10-07.

| Componente | Versión | Fijación |
|---|---|---|
| Imagen del laboratorio | `mcr.microsoft.com/dotnet/sdk:8.0` | `sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743` |
| SDK .NET (E2-1–E2-4, E2-6) | 8.0.425 y 10.0.400 | sdk:10.0 `sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c` |
| git en la imagen | 2.39.5 | la del Debian 12 de la imagen |
| EditorConfig (Python) | 0.17.1 | `pip install EditorConfig==0.17.1` |
| Node.js / npm (E2-5) | 22.23.2 / 10.9.8 | `node:22-bookworm-slim` `sha256:d649c27dae7ba0137b3cef5dd75baa422c08dc3d9e3fc0c23dfb172dc3cc6436` |
| Python (E2-5) | 3.12.14 | `python:3.12-slim` `sha256:78387bc3881b8273120a12ebe6c1ab22b018ccc2c9adf565ae1ac9b536e184ea` |

El paquete `python3` de la imagen del laboratorio sale de los repositorios de Debian sin versión fijada; la compuerta sólo depende de que sea Python 3.9 o superior.

## Ampliaciones de la revisión (2026-10-09)

| ID | Carpeta | Pregunta | Resultado observado |
|---|---|---|---|
| E2-2 (caso 11) | `e2-migracion/squash-y-blame.sh` | ¿Un PR fusionado con *squash* rompe `.git-blame-ignore-revs`? | No, si cada commit va en su PR y se lista el SHA del *squash* ya en `main`: `blame` vuelve a la autora (git 2.39.5 en la imagen, `salidas/e2-migracion.txt`; git 2.47.3 en el host, `salidas/e2-squash-y-blame-host.txt`). |
| E2-7 | `e7-editorconfig-checker/run.sh` | ¿editorconfig-checker detecta lo mismo que `check_text_policy.py`? | 6 errores y rc=1 sobre siete archivos sintéticos: cp1252 (como `ISO-8859-1`), BOM indebido, BOM faltante y CRLF (`salidas/e7-editorconfig-checker.txt`). Revisa el working tree, no el índice. |
| E2-8 | `e8-repos-de-referencia/relevar.sh` | ¿Qué declaran seis repos públicos `dotnet/*` sobre BOM, EOL, locks y `rollForward`? | Relevamiento **fechado** el 2026-10-07 (`salidas/e8-repos-de-referencia.txt`); no se re-corre, porque los repos cambian. |
