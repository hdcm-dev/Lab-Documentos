#!/usr/bin/env bash
# Corre los siete experimentos del N7 y deja la salida en salidas/. Desde el HOST:
#   bash correr-todo.sh
# Imágenes: lab-n2-texto:local (receta en ../N2-politica-de-texto-y-remediacion/tools/Dockerfile),
# lab-n7-ci-sdk10:local, lab-n7-node:local, lab-n7-python:local (recetas en este laboratorio),
# python:3.12-slim. Las construye si faltan.
set -u
cd "$(dirname "$0")"; N7=$PWD; EX=$(cd .. && pwd)
construir() { docker image inspect "$1" >/dev/null 2>&1 || docker build -q -t "$1" -f "$2" "$(dirname "$2")"; }
construir lab-n2-texto:local    "$EX/N2-politica-de-texto-y-remediacion/tools/Dockerfile"
construir lab-n7-ci-sdk10:local "$N7/04-descongelamiento-controlado/imagen-ci/Dockerfile"
construir lab-n7-node:local     "$N7/07-misma-compuerta-otros-ecosistemas/Dockerfile.node"
construir lab-n7-python:local   "$N7/07-misma-compuerta-otros-ecosistemas/Dockerfile.python"
en() { local img=$1; shift; docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 \
  -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e npm_config_cache=/tmp/npm -v "$EX":/ex -w /ex/N7-compuertas-y-gobierno "$img" "$@"; }
en lab-n2-texto:local bash 01-misma-compuerta-local-y-ci/run.sh   > salidas/E7-01-misma-compuerta-local-y-ci.txt 2>&1
en lab-n2-texto:local bash 02-pre-commit-y-politica/run.sh        > salidas/E7-02-pre-commit-y-politica.txt 2>&1
en lab-n2-texto:local bash 03-revisor-cambios-de-entorno/run.sh   > salidas/E7-03-revisor-cambios-de-entorno.txt 2>&1
bash 04-descongelamiento-controlado/correr.sh                       > salidas/E7-04-descongelamiento-controlado.txt 2>&1
en lab-n2-texto:local bash 05-sin-proteccion-de-rama/run.sh       > salidas/E7-05-sin-proteccion-de-rama.txt 2>&1
en lab-n2-texto:local bash 05-sin-proteccion-de-rama/medir-tiempo.sh > salidas/V7-01-tiempo-de-la-compuerta.txt 2>&1   # verificación adversarial
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$N7":/n7 -w /n7 python:3.12-slim \
  bash 06-actualizacion-automatica/run.sh                           > salidas/E7-06-actualizacion-automatica.txt 2>&1
en lab-n7-node:local   bash 07-misma-compuerta-otros-ecosistemas/run.sh node   > salidas/E7-07-otros-ecosistemas-node.txt 2>&1
en lab-n7-python:local bash 07-misma-compuerta-otros-ecosistemas/run.sh python > salidas/E7-07-otros-ecosistemas-python.txt 2>&1
rm -rf work                                   # repos de juguete: no se versionan
grep -c 'resultado=' salidas/*.txt
