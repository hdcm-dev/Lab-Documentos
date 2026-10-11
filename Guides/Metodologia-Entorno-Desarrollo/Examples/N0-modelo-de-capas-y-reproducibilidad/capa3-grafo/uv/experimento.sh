#!/usr/bin/env bash
# E0-3c · Capa 3 (grafo) en Python con uv. Corre dentro de python:3.12-slim.
# La versión de uv se FIJA (capa 2: el gestor también es toolchain).
set -u
pip install -q --disable-pip-version-check --root-user-action=ignore "uv==0.12.23" >/dev/null 2>&1
trabajo=$(mktemp -d); cd "$trabajo"
echo "$(python --version) · $(uv --version)"
cat > pyproject.toml <<TOML
[project]
name = "demo-rango"
version = "0.1.0"
requires-python = ">=3.12"
dependencies = ["six>=1.10"]   # rango: mínimo inclusivo, sin techo
TOML
echo "--- (a) uv lock: el rango >=1.10 resuelve a la más alta disponible"
uv lock -q && grep -A1 '^name = "six"' uv.lock | tail -1
echo "--- (b) se edita el rango a six>=1.10,<1.16 y NO se regenera el lock: uv lock --locked"
sed -i 's/"six>=1.10"/"six>=1.10,<1.16"/' pyproject.toml
uv lock --locked 2>&1 | head -3
echo "    código de salida: ${PIPESTATUS[0]}"
echo "--- (c) uv sync --locked (la instalación también se niega)"
uv sync --locked -q 2>&1 | head -2
echo "    código de salida: ${PIPESTATUS[0]}"
cd /; rm -rf "$trabajo"
