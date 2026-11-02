#!/usr/bin/env bash
# compuerta-referencias.sh — falla si la guía o un README de la guía cita un archivo que no existe (U-1.1).
#
# Uso:  tools/compuerta-referencias.sh [carpeta-de-la-guía]      (defecto: la carpeta padre de tools/)
#
# Qué mira (fuera de los bloques ``` y ~~~):
#   - destinos de enlaces Markdown relativos: [texto](ruta) — sin esquema (http:, mailto:) ni ancla sola;
#   - código en línea `…` que parece una ruta de archivo: lleva «/» o termina en una extensión de la lista EXT.
# Dónde busca cada cita, en orden (gana el primero que exista):
#   1. relativa a la carpeta del archivo que cita;
#   2. relativa a la carpeta de la guía, a Examples/ y a Demos/MovilidadUrbana.Asistente/;
#   3. si empieza con «/», relativa a la raíz del workspace (la carpeta que contiene LAB/);
#   4. si es un nombre sin «/», en cualquier lugar del árbol de la guía (sin bin/ ni obj/).
# Qué ignora: comodines (* ?), marcadores ({…}, <…>, $VAR, …), rutas de contenedor (/src, /tmp, /registros,
#   /artefactos, /nuget, /guia…), y lo listado en tools/compuerta-referencias.excepciones (una entrada por
#   línea, literal; «#» comenta). Una excepción lleva su motivo en el comentario de la misma línea.
# Salida: una línea «archivo:línea: cita» por cada referencia rota; rc 0 sin hallazgos, 1 con hallazgos, 2 uso.
set -uo pipefail
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GUIA="$(cd "${1:-$AQUI/..}" 2>/dev/null && pwd)" || { echo "compuerta-referencias: no existe la carpeta ${1:-}" >&2; exit 2; }
EXC="$GUIA/tools/compuerta-referencias.excepciones"
exec python3 -I - "$GUIA" "$EXC" <<'PY'
import os, re, sys
guia, exc = sys.argv[1], sys.argv[2]
EXT = ("cs", "csproj", "md", "sh", "py", "json", "jq", "tsv", "razor", "props", "targets", "slnx", "sln",
       "yml", "yaml", "excepciones", "css", "js", "html", "sql", "xml", "config")
CONTENEDOR = ("/src", "/tmp", "/registros", "/artefactos", "/nuget", "/guia", "/guia-ro", "/app",
              "/var/", "/etc/", "/usr/", "/dev/", "/proc/", "/root/", "/opt/")
excepciones = set()
if os.path.isfile(exc):
    for l in open(exc, encoding="utf-8"):
        l = l.split("#", 1)[0].strip()
        if l: excepciones.add(l)
# raíz del workspace: la carpeta que contiene LAB/ (la guía vive en …/LAB/…)
partes = guia.split(os.sep)
raiz = os.environ.get("RAIZ_WORKSPACE") or (os.sep.join(partes[:partes.index("LAB")]) if "LAB" in partes else None)
nombres, rutas = set(), []
for d, subdirs, fs in os.walk(guia):
    subdirs[:] = [s for s in subdirs if s not in ("bin", "obj", ".git")]
    nombres.update(fs); nombres.update(subdirs)
    rutas += [os.path.relpath(os.path.join(d, x), guia) for x in fs + subdirs]
bases = [guia, os.path.join(guia, "Examples"), os.path.join(guia, "Demos", "MovilidadUrbana.Asistente")]
archivos = [os.path.join(guia, f) for f in os.listdir(guia) if f.endswith(".md")]
for d, subdirs, fs in os.walk(guia):
    subdirs[:] = [s for s in subdirs if s not in ("bin", "obj", ".git")]
    archivos += [os.path.join(d, f) for f in fs if f == "README.md"]
archivos = sorted(set(archivos))
re_link = re.compile(r"\]\(([^)\s]+)(?:\s+\"[^\"]*\")?\)")
re_code = re.compile(r"`([^`\n]+)`")
re_ext = re.compile(r"\.(" + "|".join(EXT) + r")$")
def parece_ruta(c):
    if " " in c or c.startswith(("-", ".")) or "://" in c or any(x in c for x in "()\"'=,;"): return False
    if re.match(r"^[A-Z][A-Z0-9_]+/", c): return False
    if re.match(r"^(application|text|image|multipart)/", c): return False  # tipo MIME       # variable de entorno como raíz (RLLM_DIR/…)
    if "." in c.split("/", 1)[0] and "/" in c and not re_ext.search(c.split("/", 1)[0]): return False  # dominio o imagen
    return "/" in c or bool(re_ext.search(c))
def ignorar(c):
    if any(x in c for x in "*?{}<>$|…") or c in excepciones: return True
    if c.startswith("/") and not c.startswith("/LAB/"): return True
    if c.startswith(CONTENEDOR): return True
    if c.startswith("~") or c.startswith("http") or c.startswith("mailto:"): return True
    return False
def existe(c, desde):
    c0 = c.split("#", 1)[0].rstrip("/")
    if not c0: return True
    if c0.startswith("/"):
        return raiz is not None and os.path.exists(os.path.join(raiz, c0.lstrip("/")))
    for b in [desde] + bases:
        if os.path.exists(os.path.normpath(os.path.join(b, c0))): return True
    if "/" not in c0 and c0 in nombres: return True
    # sufijo de una ruta del árbol (p. ej. «Cableado/AprobacionTests.cs» dentro de la demo)
    if any(r == c0 or r.endswith("/" + c0) for r in rutas): return True
    # «Examples/13»: abreviatura de una carpeta «13-…»
    cab, _, ult = c0.rpartition("/")
    if ult and any(os.path.basename(r).startswith(ult + "-") and os.path.dirname(r).endswith(cab) for r in rutas): return True
    return False
rotas = 0
for a in archivos:
    fuera = False
    for n, l in enumerate(open(a, encoding="utf-8"), 1):
        if re.match(r"\s*(```|~~~)", l): fuera = not fuera; continue
        if fuera: continue
        citas = [m.group(1) for m in re_link.finditer(l)]
        citas += [m.group(1).strip() for m in re_code.finditer(l) if parece_ruta(m.group(1).strip())]
        for c in citas:
            if c.startswith("#") or ignorar(c) or re.match(r"^[a-z]+:", c): continue
            if c.split(":", 1)[0] != c and re.search(r":\d+(-\d+)?$", c): c = re.sub(r":\d+(-\d+)?$", "", c)  # [ruta:líneas]
            if not existe(c, os.path.dirname(a)):
                print(f"{os.path.relpath(a, guia)}:{n}: {c}"); rotas += 1
print(f"compuerta-referencias: {rotas} referencia(s) rota(s) en {len(archivos)} archivo(s)", file=sys.stderr)
sys.exit(1 if rotas else 0)
PY
