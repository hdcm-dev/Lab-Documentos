#!/bin/sh
# requires-python (metadato/rango) frente a .python-version (preferencia del entorno), con pip y con uv.
# Uso (desde esta carpeta): sh correr.sh      Variable opcional: UV_VER (versión de uv a instalar).
UV_VER="${UV_VER:-0.12.23}"
IMG=python:3.12-slim
run() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e PIP_DISABLE_PIP_VERSION_CHECK=1 \
  -e PIP_ROOT_USER_ACTION=ignore -v "$PWD/$1":/src:ro $IMG sh -c "cp -r /src/. /tmp/w && cd /tmp/w && $2" 2>&1; }
UVINST="python -m pip install -q --user uv==$UV_VER >/dev/null 2>&1; export PATH=/tmp/.local/bin:\$PATH"
echo "## intérprete de la imagen: $(docker run --rm $IMG python -V)"
echo "--- A. pip install . con requires-python >=3.13"
run pip-requires-python 'pip install -q . ; echo "rc=$?"' | grep -E "ERROR|rc=" 
echo "--- B. lo mismo con --ignore-requires-python (el metadato se puede saltear)"
run pip-requires-python 'pip install -q --user --ignore-requires-python . ; echo "rc=$?"' | grep -E "ERROR|rc="
echo "--- C. uv: .python-version=3.13 dentro del rango (uv descarga el intérprete si falta)"
run uv-coincide "$UVINST; uv --version; uv run python -V; echo rc=\$?" | grep -vE '^(Using|Creating|Installed|Resolved|Audited|Downloading|Prepared| \+)' 
echo "--- D. uv: .python-version=3.11 contradice requires-python >=3.13"
run uv-contradice "$UVINST; uv run python -V; echo rc=\$?"
echo "--- E. uv: .python-version=3.13 pero descargas prohibidas (UV_PYTHON_DOWNLOADS=never)"
run uv-coincide "$UVINST; UV_PYTHON_DOWNLOADS=never uv run python -V; echo rc=\$?"
echo "--- F. uv: [tool.uv] required-version fija la versión del propio uv"
run uv-version-del-gestor "$UVINST; uv run python -V; echo rc=\$?"
