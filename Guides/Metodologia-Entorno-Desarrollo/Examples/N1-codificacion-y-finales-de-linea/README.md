# N1 — Codificación de texto y finales de línea: laboratorio

Este laboratorio reproduce, con un ejemplo sintético, el caso que dispara la guía: un sistema .NET heredado con unos cien archivos `.cs`/`.razor` guardados en Windows-1252 (cp1252) que en Windows compila y en Linux falla en un identificador acentuado y corrompe los literales con tildes. Después lleva los **mismos bytes** a los otros lectores de una solución políglota (Razor, Node.js, Python, git, el shell) y muestra cómo diagnosticar la codificación real de un archivo sin abrirlo en un editor.

Ningún fuente con codificación «rara» está versionado: `generar.py` los escribe byte a byte en `work/` (ignorado por git), para que ni un editor ni un `git checkout` con `core.autocrlf` los cambie sin que se note. Las salidas capturadas el 2026-10-07 están en `salidas/`.

## Índice

- [Requisitos](#requisitos)
- [Cómo correrlo](#cómo-correrlo)
- [Qué demuestra cada experimento](#qué-demuestra-cada-experimento)
- [Archivos](#archivos)
- [Limpieza](#limpieza)
- [Límites](#límites)

## Requisitos

Docker y estas imágenes (las versiones son las observadas en la corrida del 2026-10-07):

| Imagen | Versión observada | Para qué |
|---|---|---|
| `mcr.microsoft.com/dotnet/sdk:8.0` | SDK 8.0.425 (Debian 12) | Roslyn, Razor, git 2.39.5, iconv, od |
| `mcr.microsoft.com/dotnet/sdk:10.0` | SDK 10.0.400 (Ubuntu 24.04) | Roslyn y Razor con el compilador actual |
| `node:22-bookworm-slim`, `node:20-alpine`, `node:24-bookworm-slim` | v22.23.2, v20.20.2 (sin soporte desde el 2026-04-30), v24.21.0 | lectura de módulos, `fs`, `JSON.parse`, `TextDecoder` |
| `python:3.12-slim`, `python:3-slim` | 3.12.14 (`LANG=C.UTF-8`), 3.14.7 (sin `LANG`) | PEP 263/3120, `open()`, modo UTF-8; shebang con BOM/CRLF; categorías Unicode |

`e1-python-locale.sh` instala el paquete `locales` con `apt-get`, así que necesita red. En el host sólo hace falta un shell POSIX; si además tiene `file` (se usó `file-5.46`), `correr-todo.sh` agrega su diagnóstico.

## Cómo correrlo

Todo de una vez, desde esta carpeta (Linux, macOS o WSL):

```sh
sh correr-todo.sh          # ~3 min; deja las salidas en ./salidas/
```

Uno por uno (primero siempre `generar.py`):

```sh
D="$PWD"
docker run --rm -v "$D":/w -w /w python:3.12-slim python generar.py

# E1-N1-01  Roslyn: cp1252 / UTF-8 / UTF-8+BOM / CRLF / CodePage
docker run --rm -v "$D":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 sh e1-roslyn.sh
docker run --rm -v "$D":/w -w /w -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0 sh e1-roslyn.sh
# E1-N1-02  Qué es la "página ANSI del sistema" para .NET en Linux
docker run --rm -v "$D":/w -w /w/sonda-codepage mcr.microsoft.com/dotnet/sdk:8.0 sh -c 'dotnet run -v q; rm -rf bin obj'
docker run --rm -v "$D":/w -w /w/sonda-codepage -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet run -v q; rm -rf bin obj'
# E1-N1-03  CodePage y el servidor de compilación (SDK 8 vs 10)
docker run --rm -v "$D":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 sh e1-roslyn-codepage-servidor.sh
docker run --rm -v "$D":/w -w /w -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0 sh e1-roslyn-codepage-servidor.sh
# E1-N1-04  Razor
docker run --rm -v "$D":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 sh e1-razor.sh
# E1-N1-05  Node.js
docker run --rm -v "$D":/w -w /w node:22-bookworm-slim node e1-node.mjs
# E1-N1-06  Python: fuente y open()
docker run --rm -v "$D":/w -w /w python:3.12-slim sh e1-python.sh
# E1-N1-07  Python: open() bajo un locale ISO-8859-1 real (necesita red)
docker run --rm -v "$D":/w -w /w python:3.12-slim sh e1-python-locale.sh
# E1-N1-08  git: BOM, CRLF, cp1252, .gitattributes, autocrlf, renormalize
docker run --rm -v "$D":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 sh e1-git.sh
# E1-N1-09  Diagnóstico de codificación y EOL; shebang con BOM/CRLF
docker run --rm -v "$D":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 sh e1-diagnostico.sh
# E1-N1-10  Qué escriben las plantillas del SDK (BOM, EOL, .editorconfig)
docker run --rm -v "$D":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 sh e1-plantillas-dotnet.sh
# E1-N1-11  Script con BOM o CRLF según quién lo lance (dash, bash, execve sin shell)
docker run --rm -v "$D":/w -w /w python:3.12-slim sh e1-shebang.sh
# E1-N1-12  Categorías Unicode (Ç, ó, U+FFFD) y bytes en que difieren cp1252 y Latin-1
docker run --rm -v "$D":/w -w /w python:3.12-slim python -I e1-unicode.py
# E1-N1-13  ¿`file` distingue cp1252 de Latin-1? (en el host, necesita `file`)
sh e1-file.sh
```

En Windows con PowerShell, reemplazar `"$D"` por `${PWD}` y correr cada `docker run` por separado; los scripts se ejecutan dentro del contenedor, así que no dependen del shell del host.

## Qué demuestra cada experimento

| ID | Script | Pregunta | Resultado observado (2026-10-07) | Salida |
|---|---|---|---|---|
| E1-N1-01 | `e1-roslyn.sh` | ¿Qué hace Roslyn en Linux con un `.cs` en cp1252? | Literal: compila, pero `ó` queda como U+FFFD. Identificador `Çaudal`: `CS1056 Unexpected character`. UTF-8 con y sin BOM: compila y los textos salen bien. CRLF: un literal crudo multilínea pasa de 7 a 8 caracteres. `CodePage=1252` arregla el cp1252 pero **rompe** el UTF-8 sin BOM (`Ã³`, largo 39); el UTF-8 con BOM no se ve afectado. Con una consola ISO-8859-1 la U+FFFD se imprime como `?`. Igual en SDK 8 y 10. | `salidas/e1-roslyn-sdk8.txt`, `…-sdk10.txt` |
| E1-N1-02 | `sonda-codepage/` | ¿Qué es «la página de códigos del sistema» en .NET sobre Linux? | `Encoding.GetEncoding(0)` devuelve `utf-8` aun con `CodePagesEncodingProvider` registrado; `1252` sólo existe con el proveedor. | `salidas/e1-sonda-codepage-sdk8.txt`, `…-sdk10.txt` |
| E1-N1-03 | `e1-roslyn-codepage-servidor.sh` | ¿Es confiable `-p:CodePage=1252` en Linux? | SDK 8.0.425: `CS2016 Code page '1252' is invalid or not installed` con el servidor de compilación, salvo que el servidor haya leído antes un archivo no UTF-8 o que se use `UseSharedCompilation=false`. SDK 10.0.400: compila siempre. | `salidas/e1-roslyn-codepage-servidor.txt` |
| E1-N1-04 | `e1-razor.sh` | ¿Razor se comporta igual? | Sí: el `.g.cs` generado contiene `EF BF BD` (U+FFFD) donde el `.razor` tenía `F3`; con UTF-8 (con o sin BOM) sale `C3 B3`. | `salidas/e1-razor-sdk8.txt`, `…-sdk10.txt` |
| E1-N1-05 | `e1-node.mjs` | ¿Y Node.js? | `require`/`import` decodifican como UTF-8: cp1252 da U+FFFD sin error. El BOM se ignora en módulos `.js`/`.mjs` y en `require()` de `.json`, pero **no** en `fs.readFileSync(…, 'utf8')` ni en `JSON.parse` (`SyntaxError`). Un template literal con CRLF queda con LF. `TextDecoder({fatal:true})` detecta el cp1252. | `salidas/e1-node.txt` |
| E1-N1-06 | `e1-python.sh` | ¿Y Python? | Fuente cp1252 sin declaración: `SyntaxError: Non-UTF-8 code…`. Con `# -*- coding: cp1252 -*-`: correcto. BOM y CRLF: correctos. `open()` sin `encoding=` usa la del locale; `utf-8` deja el BOM como `\ufeff`, `utf-8-sig` lo quita; el modo texto traduce CRLF→LF. | `salidas/e1-python.txt` |
| E1-N1-07 | `e1-python-locale.sh` | ¿Cuándo `open()` lee mal un UTF-8? | Con `LC_ALL=es_AR.ISO-8859-1` (3.12 y 3.14): `open()` usa ISO-8859-1 y lee `Ã³`. Con `-X utf8` o `encoding="utf-8"`: correcto. | `salidas/e1-python-locale.txt` |
| E1-N1-08 | `e1-git.sh` | ¿Qué ve git? | Agregar BOM: 1 línea cambiada, idéntica a la vista. Pasar a CRLF: todas las líneas cambian; `--ignore-cr-at-eol` muestra 0 diferencias. cp1252: git no avisa nada. `* text=auto eol=lf` impide que el CRLF entre al repo. `working-tree-encoding=CP1252` guarda UTF-8 en el repo y cp1252 en disco. `core.autocrlf=true` escribe CRLF en disco con `git status` limpio. `git add --renormalize .` corrige lo ya commiteado. | `salidas/e1-git.txt` |
| E1-N1-09 | `e1-diagnostico.sh` | ¿Cómo se sabe qué codificación tiene un archivo? | `iconv -f UTF-8 -t UTF-8` falla en cp1252; los 3 primeros bytes revelan el BOM; `tr -cd '\r'` cuenta CR. `file` dice `ISO-8859 text` para este cp1252 (no tiene bytes `0x80–0x9F`; ver E1-N1-13). Un BOM o un CRLF en un script de shell lo rompen (detalle por invocador en E1-N1-11). | `salidas/e1-diagnostico.txt`, `…-host-con-file.txt` |
| E1-N1-10 | `e1-plantillas-dotnet.sh` | ¿Qué escribe el propio SDK? | `dotnet new console` crea `Program.cs` y `.csproj` **con BOM** (SDK 8 y 10); en SDK 8 con CRLF, en SDK 10 con LF. `dotnet new editorconfig` no declara `charset`; en SDK 8 declara `end_of_line = crlf` para `*.cs`, en SDK 10 no declara EOL. | `salidas/e1-plantillas-dotnet.txt` |
| E1-N1-11 | `e1-shebang.sh` | ¿Qué pasa al ejecutar un script con BOM o con CRLF? | BOM: dash y bash ignoran el shebang, dan error en la línea 1 y **siguen ejecutando** el resto con ellos mismos (exit 0); un `execve` sin shell (`subprocess`, `shell=False`) falla con `Exec format error`. CRLF: los tres fallan (`not found`, `required file not found`, `ENOENT`). | `salidas/e1-shebang.txt` |
| E1-N1-12 | `e1-unicode.py` | ¿Por qué U+FFFD no puede estar en un identificador? ¿Dónde difieren cp1252 y Latin-1? | `Ç` = `Lu`, `ó` = `Ll`, U+FFFD = `So` (Unicode 15.0.0 de Python 3.12.14). cp1252 y Latin-1 difieren en los 32 bytes `0x80–0x9F`, que en Latin-1 son controles C1; cinco no tienen carácter en cp1252. | `salidas/e1-unicode.txt` |
| E1-N1-13 | `e1-file.sh` | ¿`file` distingue cp1252 de Latin-1? | Sólo si aparecen bytes `0x80–0x9F`: sin ellos dice `ISO-8859 text`; con `€` o comillas tipográficas, `Non-ISO extended-ASCII text`. | `salidas/e1-file.txt` |

## Archivos

| Archivo | Rol |
|---|---|
| `generar.py` | Escribe los fuentes de prueba en `work/` (cp1252, UTF-8, UTF-8 con BOM; LF y CRLF). |
| `e1-*.sh`, `e1-node.mjs`, `e1-unicode.py` | Un experimento por archivo; cada uno dice en su cabecera cómo se corre. |
| `sonda-codepage/` | Proyecto mínimo para E1-N1-02. |
| `correr-todo.sh` | Corre todo y regenera `salidas/`. |
| `salidas/` | Salida real capturada el 2026-10-07. |
| `.gitignore` | Excluye `work/`; `bin/`, `obj/` y demás los excluye el `.gitignore` de `Examples/`. |

## Limpieza

Los scripts borran `bin/` y `obj/` al terminar. `work/` lo crea un contenedor como `root`; para borrarlo:

```sh
docker run --rm -v "$PWD":/w -w /w python:3.12-slim rm -rf work
```

## Límites

- Nada de esto se corrió en Windows. Lo que la guía afirma sobre Windows (que Roslyn cae a la página ANSI del sistema, típicamente 1252) sale del código fuente de Roslyn y de la documentación de .NET, no de un experimento; se puede verificar copiando `work/dotnet/cp1252-lf` a una estación Windows y compilando con `dotnet build -p:ConIdentificador=true`.
- La explicación del `CS2016` de E1-N1-03 (el proveedor de páginas de códigos no está registrado en el proceso del servidor hasta que alguien pasa por el camino de *fallback*) es una interpretación coherente con lo observado y con el código de Roslyn; no se depuró el servidor.
- El comportamiento de los editores al guardar (Visual Studio, VS Code) no se automatizó: se documenta con su documentación y su código fuente.

## Ampliaciones de la revisión (2026-10-09)

- **E1-04 y E1-05, identificador no ASCII.** `generar.py` agrega `work/node/mod-ident-cp1252.js` (`const año = 2026;` en cp1252) y `work/razor/cp1252-ident/` (`<p>@Çaudal</p>` y `public int Çaudal {get;set;}` en `@code`, en cp1252). Node falla con `SyntaxError` (`salidas/e1-node.txt`, `e1-node-24.txt`); Razor, con `RZ1005` y `CS1056` (SDK 8) o `RZ1005` y `CS0246` (SDK 10) (`salidas/e1-razor-sdk8.txt`, `e1-razor-sdk10.txt`). En literales y HTML, los dos siguen corrompiendo sin error.
- **E1-13, mojibake.** `e1-file.sh` agrega `moji.txt` (`acentuaci\303\263n`): `file` 5.46 lo informa como `Unicode text, UTF-8 text`, aunque provenga de un cp1252 mal convertido.
- **Reproducción de la guía §1.2.** Los dos `docker run` de §1.2, copiados tal cual sobre una copia limpia y como usuario no root: `salidas/e1-reproduccion-guia-1.2.txt` (`CS1056`, `FFFD 006E 003A`, `work/` del usuario).
