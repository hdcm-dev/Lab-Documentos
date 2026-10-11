#!/usr/bin/env bash
# E8-2 — ¿Cómo se fija el toolchain de tres ecosistemas desde un único punto de verdad?
# Los archivos NATIVOS (global.json, .nvmrc, tools/gen/.python-version) son la verdad;
# la compuerta verificar.sh la hace cumplir; mise puede LEERLOS, pero un mise.toml
# propio crea una segunda verdad que gana en silencio.
set -uo pipefail
AQUI=$(cd "$(dirname "$0")" && pwd)
IMG=${IMG:-lab-n8-poliglota:local}
MISE_VER=${MISE_VER:-v2026.10.3}
W=$(mktemp -d); trap 'rm -rf "$W"' EXIT
cp -a "$AQUI/../solucion" "$W/repo"
run() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$W/repo:/w" -w /w "$IMG" "$@"; }

echo "### 1. Imagen construida desde los archivos del repo: la compuerta pasa"
run ./verificar.sh 2>&1 | grep -E '^(toolchain|== compuerta)'

echo; echo "### 2. Alguien sube .nvmrc a 24 y no reconstruye la imagen"
cp "$W/repo/.nvmrc" "$W/nvmrc.orig"; printf '24.21.0\n' > "$W/repo/.nvmrc"
run ./verificar.sh 2>&1 | grep -E '^(toolchain|== compuerta)'; echo "rc=${PIPESTATUS[0]}"
cp "$W/nvmrc.orig" "$W/repo/.nvmrc"

echo; echo "### 3. mise ($MISE_VER) como gestor multi-runtime"
run bash -c "
curl -fsSL -o /tmp/mise https://github.com/jdx/mise/releases/download/$MISE_VER/mise-$MISE_VER-linux-x64 && chmod +x /tmp/mise
export MISE_YES=1 MISE_TRUSTED_CONFIG_PATHS=/w
/tmp/mise --version
echo '--- a) por defecto (archivos idiomáticos deshabilitados): mise no ve nada'
/tmp/mise ls --current
echo '--- b) habilitados para node, python y dotnet, desde la raíz: lee los MISMOS archivos nativos'
MISE_IDIOMATIC_VERSION_FILE_ENABLE_TOOLS=node,python,dotnet /tmp/mise ls --current
echo '--- c) además un mise.toml con node = \"24\": segunda verdad, gana mise.toml'
printf '[tools]\nnode = \"24\"\n' > mise.toml
MISE_IDIOMATIC_VERSION_FILE_ENABLE_TOOLS=node,python,dotnet /tmp/mise ls --current
rm mise.toml
echo '--- d) desde tools/gen/: recién ahí ve el .python-version del proyecto uv'
cd tools/gen && MISE_IDIOMATIC_VERSION_FILE_ENABLE_TOOLS=node,python,dotnet /tmp/mise ls --current
" 2>&1

echo; echo "### 4. ¿Un .python-version en la RAÍZ del repo gobierna al proyecto uv de tools/gen/?"
PY=python:3.12-slim@sha256:78387bc3881b8273120a12ebe6c1ab22b018ccc2c9adf565ae1ac9b536e184ea   # trae 3.12.14
UVIMG=ghcr.io/astral-sh/uv:0.12.23@sha256:61d393e44e249f2e4b526b6c7ddcecce245946826e608e11c93ad4f5bba55b21
docker create --name n8-uv "$UVIMG" x >/dev/null && docker cp -q n8-uv:/uv "$W/uv" && docker rm n8-uv >/dev/null
mv "$W/repo/tools/gen/.python-version" "$W/repo/.python-version"   # 3.12.15, ahora en la raíz
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e UV_PYTHON_DOWNLOADS=never -e UV_CACHE_DIR=/tmp/c \
  -v "$W/repo:/w" -v "$W/uv:/usr/local/bin/uv:ro" -w /w "$PY" sh -c '
echo "raíz/.python-version = $(cat .python-version)"
echo "--- uv python find desde la raíz:";     uv python find --show-version 2>&1 | tail -1
echo "--- uv run desde tools/gen (proyecto):"; cd tools/gen && uv run --locked -q python -c "import platform;print(\"usa python\", platform.python_version())" 2>&1 | tail -1
echo "--- por qué (uv -v):";                   uv python find -v 2>&1 | grep -m1 "Python request"'
