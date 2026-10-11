#!/usr/bin/env bash
# Control compensatorio en el ACTO IRREVERSIBLE (publicar/etiquetar).
# Sin protección de rama nada impide que main quede en rojo; lo que sí se puede
# impedir es PUBLICAR desde un commit que no cumple. Reglas:
#   1. el commit pertenece a origin/main;
#   2. la compuerta da VERDE sobre ESE commit, en un clon limpio;
#   3. la etiqueta no existe (no se reescribe una versión publicada).
# Uso: publicar.sh <remoto> <commit> <version>
set -u
remoto=$1; commit=$2; version=$3
tmp=$(mktemp -d); git clone -q "$remoto" "$tmp/r" && cd "$tmp/r" || exit 2
git merge-base --is-ancestor "$commit" origin/main || { echo "PUBLICAR: $commit no está en main -> NO se publica"; exit 1; }
git rev-parse -q --verify "refs/tags/v$version" >/dev/null && { echo "PUBLICAR: v$version ya existe -> NO se publica"; exit 1; }
git checkout -q "$commit"
if ! tools/compuerta.sh > compuerta.txt 2>&1; then
  grep -E 'FALLA|resultado' compuerta.txt | sed 's/^/   /'
  echo "PUBLICAR: compuerta en ROJO sobre $commit -> NO se publica"; exit 1
fi
git tag -a "v$version" -m "v$version" && git push -q origin "v$version"
echo "PUBLICAR: v$version etiquetada sobre $(git rev-parse --short "$commit"); registro de entorno:"
grep -E '^COMPUERTA' compuerta.txt | sed 's/^/   /'   # N6 puede adjuntar esto al artefacto
