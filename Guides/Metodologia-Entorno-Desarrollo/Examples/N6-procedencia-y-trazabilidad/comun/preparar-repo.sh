#!/usr/bin/env bash
# preparar-repo.sh <origen> <destino>
# Copia la pieza sintética a <destino> y la convierte en un repo git con UN commit de fecha y autor fijos,
# y un remoto ficticio (SourceLink necesita un remoto de un proveedor conocido para armar las URLs).
# Mismo contenido + mismo autor + misma fecha => mismo SHA en cualquier máquina.
set -eu
origen=$1; destino=$2
rm -rf "$destino"; mkdir -p "$destino"
cp -r "$origen"/. "$destino"/
cd "$destino"
git init -q -b main
git add -A
GIT_AUTHOR_DATE='2026-01-01T00:00:00Z' GIT_COMMITTER_DATE='2026-01-01T00:00:00Z' \
  git -c user.name=lab -c user.email=lab@example.invalid commit -qm "pieza 1.4.0"
git remote add origin https://github.com/ejemplo-lab/pieza.git
git rev-parse HEAD
