#!/usr/bin/env bash
# Corre DENTRO de python:3.12-slim. $1 = backend (hatchling|setuptools). Instala herramientas fijadas y construye.
set -eu
pip install -q --disable-pip-version-check --root-user-action=ignore build==1.6.1 hatchling==1.32.4 setuptools==84.0.0 cyclonedx-bom==7.5.0 six==1.17.0 2>/dev/null
if [ "$1" = hatchling ]; then
  # SBOM de lo que la pieza declara como dependencia de ejecución, generado ANTES de empaquetar.
  cyclonedx-py requirements ${SBOM_REPRO:+--output-reproducible} --of JSON -o sbom.cdx.json <(echo "six==1.17.0") 2>/dev/null
fi
python -m build --wheel --no-isolation -o dist . >/dev/null 2>&1 || { echo "build falló"; exit 1; }
# Devolver los archivos al usuario del host (pip necesita root en la imagen; el host no).
[ -n "${PROPIETARIO:-}" ] && chown -R "$PROPIETARIO" . || true
