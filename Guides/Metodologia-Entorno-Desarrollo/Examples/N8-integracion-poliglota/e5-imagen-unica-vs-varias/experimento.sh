#!/usr/bin/env bash
# E8-5 — ¿Una imagen con los tres runtimes o una por componente?
# Compara versiones, tamaños y resultado de la build en las dos variantes.
set -uo pipefail
AQUI=$(cd "$(dirname "$0")" && pwd)
IMG=${IMG:-lab-n8-poliglota:local}
SDK=mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743
NODE=node:22-bookworm-slim@sha256:d649c27dae7ba0137b3cef5dd75baa422c08dc3d9e3fc0c23dfb172dc3cc6436
PY=python:3.12-slim@sha256:78387bc3881b8273120a12ebe6c1ab22b018ccc2c9adf565ae1ac9b536e184ea
W=$(mktemp -d); trap 'rm -rf "$W"' EXIT
mb() { docker image inspect "$1" --format '{{.Size}}' | awk '{printf "%.0f MB", $1/1000000}'; }

echo "### 1. Qué trae cada imagen (repo pide: SDK $(python3 -c 'import json;print(json.load(open("'"$AQUI"'/../solucion/global.json"))["sdk"]["version"])'), node $(cat "$AQUI/../solucion/.nvmrc"), python $(cat "$AQUI/../solucion/tools/gen/.python-version"))"
printf '%-34s %-9s %s\n' "imagen" "tamaño" "versiones"
printf '%-34s %-9s %s\n' "sdk:8.0 (oficial)"            "$(mb $SDK)"  "$(docker run --rm $SDK sh -c 'echo sdk $(dotnet --version); command -v node || echo "sin node"; command -v python3 || echo "sin python"' | tr '\n' ' ')"
printf '%-34s %-9s %s\n' "node:22-bookworm-slim (oficial)" "$(mb $NODE)" "$(docker run --rm $NODE sh -c 'echo node $(node -v) npm $(npm -v)')"
printf '%-34s %-9s %s\n' "python:3.12-slim (oficial)"     "$(mb $PY)"   "$(docker run --rm $PY sh -c 'echo python $(python -V | cut -d" " -f2); command -v uv || echo "sin uv"' | tr '\n' ' ')"
printf '%-34s %-9s %s\n' "lab-n8-poliglota:local (única)" "$(mb $IMG)"  "$(docker run --rm $IMG sh -c 'echo sdk $(dotnet --version) node $(node -v) python $(python3 -V | cut -d" " -f2) uv $(uv -V | cut -d" " -f2)')"
echo "capas compartidas entre la imagen única y sdk:8.0: $(comm -12 <(docker image inspect $SDK --format '{{range .RootFS.Layers}}{{println .}}{{end}}' | sort) <(docker image inspect $IMG --format '{{range .RootFS.Layers}}{{println .}}{{end}}' | sort) | grep -c sha256) de $(docker image inspect $IMG --format '{{len .RootFS.Layers}}')"

echo; echo "### 2. Imagen única: build.sh (compuerta + tres pasos + .NET), repo montado en /src"
cp -a "$AQUI/../solucion" "$W/repo"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$W/repo:/src" -w /src "$IMG" ./build.sh 2>&1 | grep -E '^(== compuerta|gen:|web:|sha256 Saludo)'

echo; echo "### 3. Una imagen por componente (py -> web -> net) con python:3.12-slim tal como está en esta máquina"
docker build --no-cache --progress=plain --build-arg PY_IMAGE=$PY -f "$AQUI/../imagen/Dockerfile.por-componente" "$AQUI/../solucion" 2>&1 \
  | grep -E 'error:|gen: |web: |Saludo.dll$|failed to solve' | sed -E 's/^#[0-9]+ [0-9.]+ //' | cut -c1-160 | head -4

echo; echo "### 4. La misma, con la imagen de Python fijada al parche que pide el repo"
PY15=python:3.12.15-slim@sha256:05cda9777409a9c3ffddd94a4c476b79f0769a0b4857f0c7ed9226b6800b0d6f
docker build --no-cache --progress=plain --build-arg PY_IMAGE=$PY15 -f "$AQUI/../imagen/Dockerfile.por-componente" "$AQUI/../solucion" 2>&1 \
  | grep -E 'error:|gen: |web: |Saludo.dll$|failed to solve' | sed -E 's/^#[0-9]+ [0-9.]+ //' | cut -c1-160
