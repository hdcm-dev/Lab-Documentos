#!/usr/bin/env bash
# E7-04 — Descongelamiento controlado. Se corre DESDE EL HOST (usa dos imágenes):
#   bash 04-descongelamiento-controlado/correr.sh
# Requiere: lab-n2-texto:local (N2) y lab-n7-ci-sdk10:local (imagen-ci/Dockerfile).
set -u
N7=$(cd "$(dirname "$0")/.." && pwd); EX=$(cd "$N7/.." && pwd)
en() { local img=$1; shift; docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 \
  -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e GIT_CONFIG_GLOBAL=/ex/N7-compuertas-y-gobierno/work/04b/gitconfig -e GIT_CONFIG_NOSYSTEM=1 \
  -v "$EX":/ex -w /ex/N7-compuertas-y-gobierno/work/04b/repo "$img" bash -c "$*"; echo "   -> rc=$?"; }
VIEJA=lab-n2-texto:local; NUEVA=lab-n7-ci-sdk10:local

echo "=== A. Descongelar un paquete (imagen $VIEJA)"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -v "$EX":/ex -w /ex/N7-compuertas-y-gobierno "$VIEJA" bash 04-descongelamiento-controlado/paquete.sh

echo; echo "=== B. Descongelar el SDK: global.json, imagen de CI y ADR en el mismo PR"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -v "$EX":/ex -w /ex/N7-compuertas-y-gobierno "$VIEJA" bash -c 'source comun.sh; preparar 04b; bash compuerta/sembrar-repo.sh "$W/repo"'
SDK10=$(docker run --rm "$NUEVA" sh -c 'cd /tmp && dotnet --version')
echo "-- SDK de la imagen nueva: $SDK10"
en "$VIEJA" "git checkout -q -b descongelar/sdk-10 && sed -i 's/\"version\": \"[^\"]*\"/\"version\": \"$SDK10\"/' global.json && cat global.json"
echo; echo "B1. Compuerta en la imagen VIEJA (estaciones y CI que no se movieron):"
en "$VIEJA" "tools/compuerta.sh"
echo; echo "B2. Compuerta en la imagen NUEVA:"
en "$NUEVA" "tools/compuerta.sh"
echo; echo "B3. Se commitean juntos global.json, la receta de la imagen de CI y el ADR (el hook corre en la imagen NUEVA):"
en "$NUEVA" "mkdir -p ci docs/adr && cp /ex/N7-compuertas-y-gobierno/04-descongelamiento-controlado/imagen-ci/Dockerfile ci/Dockerfile \
  && sed -e 's/NNNN/0002/' -e 's/<verbo + qué se descongela>.*/Subir el SDK de .NET a $SDK10/' \
     /ex/N7-compuertas-y-gobierno/04-descongelamiento-controlado/ADR-plantilla-entorno.md > docs/adr/0002-sdk-10.md \
  && git add -A && git -c core.hooksPath=.githooks commit -q -m 'SDK $SDK10: global.json + imagen de CI + ADR-0002' && git show --stat --format='   commit %h %s' HEAD"
