#!/usr/bin/env bash
# E5 de N4 — Python: uv.lock, pip-tools/uv pip compile con hashes, pylock.toml (PEP 751) y Poetry.
# Corre en python:3.12-slim sobre una copia en /tmp; instala las herramientas con versión fija.
set -u
UV_VERSION="${UV_VERSION:-0.12.23}"; PIP_VERSION="${PIP_VERSION:-26.2.1}"; POETRY_VERSION="${POETRY_VERSION:-2.5.1}"
export PIP_DISABLE_PIP_VERSION_CHECK=1 PIP_ROOT_USER_ACTION=ignore UV_NO_PROGRESS=1
python -m pip install -q "pip==$PIP_VERSION" "uv==$UV_VERSION" "poetry==$POETRY_VERSION" 2>&1 | grep -v WARNING
W=/tmp/w; rm -rf "$W"; mkdir -p "$W"; cp /src/pyproject.toml "$W"; cd "$W"
paso() { printf '\n===== %s =====\n' "$*"; }

paso "Versiones"; python --version; pip --version | cut -d' ' -f1-2; uv --version; poetry --version

paso "1. uv lock: resolución universal; colorama queda en el lock con su marcador de Windows"
uv lock 2>&1 | tail -n 1
grep -nE '^name = |marker|^version = |^requires-python' uv.lock | head -n 14
cp uv.lock /tmp/uv.lock.original

paso "2. Con el lock coherente: uv lock --check y uv sync --locked pasan"
uv lock --check 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"
uv sync --locked 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"

paso "3. Deriva: se restringe tqdm en pyproject.toml y no se regenera el lock"
sed -i 's/"tqdm>=4.66",/"tqdm>=4.66,<4.67",/' pyproject.toml
uv lock --check 2>&1 | grep -iE 'error|needs to be updated' | head -n 2; echo "exit=${PIPESTATUS[0]}"
uv sync --locked 2>&1 | grep -iE 'error|needs to be updated' | head -n 2; echo "exit=${PIPESTATUS[0]}"

paso "4. uv sync (sin --locked) acepta el cambio y reescribe el lock en silencio"
uv sync 2>&1 | tail -n 2; echo "exit=${PIPESTATUS[0]}"
diff <(grep -A1 '^name = "tqdm"' /tmp/uv.lock.original) <(grep -A1 '^name = "tqdm"' uv.lock)
cp /src/pyproject.toml pyproject.toml; cp /tmp/uv.lock.original uv.lock

paso "5. pip-tools style (uv pip compile): por defecto resuelve para ESTA plataforma"
uv pip compile pyproject.toml --generate-hashes -q -o req.linux.txt
echo "-- sin --universal (generado en Linux):"; grep -E '^[a-z]' req.linux.txt
uv pip compile pyproject.toml --generate-hashes --universal -q -o req.universal.txt
echo "-- con --universal:"; grep -E '^[a-z]' req.universal.txt

paso "6. Modo hash de pip: un hash alterado en requirements hace fallar la instalación"
python -m venv /tmp/v1; /tmp/v1/bin/python -m pip install -q "pip==$PIP_VERSION"
# Se reemplazan TODOS los hashes de tqdm: pip acepta el archivo si coincide con cualquiera de los listados.
awk '/^tqdm==/{t=1} /^[a-z]/ && !/^tqdm==/{t=0} t{gsub(/sha256:[0-9a-f]{64}/,"sha256:0000000000000000000000000000000000000000000000000000000000000000")} {print}' req.linux.txt > req.alterado.txt
/tmp/v1/bin/pip install -q --no-cache-dir --require-hashes -r req.linux.txt && echo "requirements con hashes correctos: instalación OK"
/tmp/v1/bin/pip uninstall -q -y tqdm
/tmp/v1/bin/pip install -q --no-cache-dir --require-hashes -r req.alterado.txt 2>&1 | grep -E 'DO NOT MATCH|Expected|Got' | head -n 3; echo "exit=${PIPESTATUS[0]}"

paso "7. PEP 751: uv export a pylock.toml (multiplataforma, con hashes) e instalación con pip"
uv export --format pylock.toml -q -o pylock.toml
grep -nE '^lock-version|^created-by|^requires-python|^environments|^name = |^marker|hashes' pylock.toml | cut -c1-90 | head -n 12
python -m venv /tmp/v2; /tmp/v2/bin/python -m pip install -q "pip==$PIP_VERSION"
/tmp/v2/bin/pip install -q --no-cache-dir -r pylock.toml 2>&1 | head -n 3; echo "exit=${PIPESTATUS[0]}"
/tmp/v2/bin/pip list --format=freeze 2>/dev/null | grep -iE 'tqdm|colorama'

paso "8. pip lock (experimental): pylock.toml válido sólo para esta versión de Python y plataforma"
pip lock -q . -o pylock.pip.toml 2>&1 | grep -v -i warning | head -n 3
grep -nE '^lock-version|^created-by|^name = |^marker' pylock.pip.toml | head -n 8
echo "colorama en el pylock de pip lock: $(grep -c 'colorama' pylock.pip.toml) (en el de uv export: $(grep -c '^name = \"colorama\"' pylock.toml))"

paso "9. Poetry: install con un lock desactualizado falla; check --lock lo detecta"
mkdir -p /tmp/p && cp /src/pyproject.toml /tmp/p && cd /tmp/p
poetry config virtualenvs.in-project true
poetry lock -q 2>&1 | tail -n 1
grep -nE '^name = |^markers|^colorama' poetry.lock | head -n 6
sed -i 's/"tqdm>=4.66",/"tqdm>=4.66,<4.67",/' pyproject.toml
poetry check --lock 2>&1 | head -n 2; echo "exit=${PIPESTATUS[0]}"
poetry install --no-root 2>&1 | grep -v '^$' | head -n 2; echo "exit=${PIPESTATUS[0]}"
