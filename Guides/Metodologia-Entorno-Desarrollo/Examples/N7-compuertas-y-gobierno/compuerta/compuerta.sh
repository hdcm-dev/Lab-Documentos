#!/usr/bin/env bash
# Compuerta única del entorno (N7).
#
# La MISMA invocación corre en los tres lugares; sólo cambia el alcance del
# chequeo de texto:
#   hook pre-commit (local) ...... compuerta.sh --staged
#   CI sobre un PR ................ compuerta.sh --base origin/main
#   CI en main / antes de publicar  compuerta.sh
#
# No define política: HACE CUMPLIR lo que el repositorio ya declara.
#   1. texto ........ .editorconfig + .gitattributes  (N2)
#   2. toolchain .... global.json / .nvmrc / .python-version  (N3)
#   3. grafo ........ packages.lock.json / package-lock.json / uv.lock  (N4)
# global.json y .nvmrc se buscan en la raíz; .python-version y los locks, en todo lo
# versionado (git ls-files): sirve igual con los proyectos en la raíz o en subcarpetas.
# Un paso cuyo archivo no existe se declara "n/a". Un paso cuyo archivo existe
# pero cuya herramienta falta FALLA: una compuerta que se saltea en silencio es
# un agujero, no una compuerta [criterio propio].
#
# Sale con 0 sólo si ningún paso falló. Imprime una línea por paso para que la
# salida sirva como registro (N6 puede guardarla junto al artefacto).
set -u
DIR=$(cd "$(dirname "$0")" && pwd)
cd "$(git rev-parse --show-toplevel)" || exit 2

fallas=0
ok()    { echo "COMPUERTA [ok]    $1"; }
mal()   { echo "COMPUERTA [FALLA] $1"; fallas=$((fallas + 1)); }
na()    { echo "COMPUERTA [n/a]   $1"; }
tiene() { command -v "$1" >/dev/null 2>&1; }

echo "COMPUERTA commit=$(git rev-parse --short HEAD 2>/dev/null || echo sin-commits) alcance=${*:-todo}"

# 1. Política de texto (N2) ---------------------------------------------------
# Reutiliza el verificador del laboratorio N2 (lee la política del propio
# .editorconfig con la implementación de referencia de EditorConfig).
if [ -f .editorconfig ]; then
  # Una herramienta que falta NO es una violación: se dice qué falta, para que nadie
  # apague el hook por un falso rojo (guía §10.2.2).
  if ! tiene python3; then
    mal "texto: falta python3 (prerrequisito de la compuerta, no una violación)"
  elif [ ! -f "$DIR/check_text_policy.py" ]; then
    mal "texto: falta check_text_policy.py junto a compuerta.sh (prerrequisito, no violación)"
  elif ! python3 -c 'import editorconfig' 2>/dev/null; then
    mal "texto: falta el paquete EditorConfig de Python (EditorConfig==0.17.1; prerrequisito, no una violación)"
  elif salida=$(python3 "$DIR/check_text_policy.py" "$@" 2>&1); then
    ok "texto: ${salida##*$'\n'}"
  else
    printf '%s\n' "$salida" | sed 's/^/    /'
    mal "texto: la política de .editorconfig/.gitattributes no se cumple"
  fi
else
  na "texto: no hay .editorconfig"
fi

# 2. Toolchain (N3): lo instalado debe ser lo declarado ----------------------
if [ -f global.json ]; then
  pedido=$(python3 -c 'import json;print(json.load(open("global.json"))["sdk"]["version"])')
  real=$(dotnet --version 2>/dev/null) || real=""
  if [ "$real" = "$pedido" ]; then ok "toolchain .NET: SDK $real = global.json"
  else mal "toolchain .NET: global.json pide $pedido; el entorno resuelve '${real:-ninguno}'"; fi
else na "toolchain .NET: no hay global.json"; fi

if [ -f .nvmrc ]; then
  pedido=$(tr -d ' \r\nv' < .nvmrc); real=$(node --version 2>/dev/null | tr -d v) || real=""
  case "$real" in "$pedido"|"$pedido".*) ok "toolchain Node: $real cumple .nvmrc=$pedido" ;;
                  *) mal "toolchain Node: .nvmrc pide $pedido; hay '${real:-ninguno}'" ;; esac
else na "toolchain Node: no hay .nvmrc"; fi

pvs=$(git ls-files '*.python-version')
if [ -n "$pvs" ]; then
  for pv in $pvs; do
    d=$(dirname "$pv"); pedido=$(tr -d ' \r\n' < "$pv")
    if tiene uv; then real=$(cd "$d" && uv run --no-sync python -c 'import platform;print(platform.python_version())' 2>/dev/null) || real=""
    else real=""; fi
    case "$real" in "$pedido"|"$pedido".*) ok "toolchain Python: $real cumple $pv=$pedido" ;;
                    *) mal "toolchain Python: $pv pide $pedido; uv resuelve '${real:-ninguno}'" ;; esac
  done
else na "toolchain Python: no hay .python-version"; fi

# 3. Grafo congelado (N4): restauración ESTRICTA, nunca regenerar ------------
locks=$(git ls-files '*packages.lock.json')
if [ -n "$locks" ]; then
  for lock in $locks; do
    d=$(dirname "$lock")
    if tiene dotnet && out=$(dotnet restore "$d" --locked-mode 2>&1); then ok "grafo NuGet: $lock coincide (--locked-mode)"
    else printf '%s\n' "${out:-dotnet no está instalado}" | grep -iE 'error|NU1|not found|requested' | sed 's/^ *//; s/^/    /' | head -4
         mal "grafo NuGet: restore --locked-mode falló para $lock"; continue; fi
    # Con lock, NuGet no avisa (NU1603) si aproximó una versión pedida que no existe:
    # se compara la cota inferior de "requested" con "resolved" en las entradas Direct y
    # CentralTransitive. [criterio] También marca una subida legítima: se resuelve subiendo
    # la versión declarada (PackageVersion) a la resuelta, en un PR que se revisa.
    if out=$(python3 - "$lock" 2>&1 <<'PY'
import json, sys
malos = []
for tfm, deps in json.load(open(sys.argv[1], encoding="utf-8"))["dependencies"].items():
    for nombre, d in deps.items():
        if d.get("type") in ("Direct", "CentralTransitive") and "requested" in d:
            piso = d["requested"].strip("[( ").split(",")[0].strip(" )]")
            if piso != d.get("resolved"):
                malos.append(f"{nombre} pide {d['requested']} y resolvió {d['resolved']} ({tfm})")
print("\n".join(malos)); sys.exit(1 if malos else 0)
PY
    ); then ok "grafo NuGet: $lock requested = resolved"
    else printf '%s\n' "$out" | sed 's/^/    /' | head -4
         mal "grafo NuGet: $lock resolvió otra versión que la pedida (revisar la declaración)"; fi
  done
else na "grafo NuGet: no hay packages.lock.json"; fi

locks=$(git ls-files '*package-lock.json')
if [ -n "$locks" ]; then
  for lock in $locks; do
    d=$(dirname "$lock")
    if tiene npm && out=$(cd "$d" && npm ci --ignore-scripts --no-audit --no-fund 2>&1); then ok "grafo npm: npm ci instaló exactamente $lock"
    else printf '%s\n' "${out:-npm no está instalado}" | grep -E 'npm (error|ERR)' | head -4 | sed 's/^/    /'
         mal "grafo npm: $lock no coincide con package.json"; fi
  done
else na "grafo npm: no hay package-lock.json"; fi

locks=$(git ls-files '*uv.lock')
if [ -n "$locks" ]; then
  for lock in $locks; do
    d=$(dirname "$lock")
    if tiene uv && out=$(cd "$d" && uv lock --check 2>&1); then ok "grafo uv: $lock al día (uv lock --check)"
    else printf '%s\n' "${out:-uv no está instalado}" | grep -v '^ *$' | tail -2 | sed 's/^/    /'
         mal "grafo uv: $lock no coincide con pyproject.toml"; fi
  done
else na "grafo uv: no hay uv.lock"; fi

if [ "$fallas" -eq 0 ]; then echo "COMPUERTA resultado=VERDE"; exit 0; fi
echo "COMPUERTA resultado=ROJO ($fallas paso/s)"; exit 1
