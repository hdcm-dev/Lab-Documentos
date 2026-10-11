#!/usr/bin/env bash
# E7-07 — La MISMA compuerta en un repo Node y en un repo Python (monolenguaje).
# Uso: run.sh node   (en lab-n7-node:local, Dockerfile.node)
#      run.sh python (en lab-n7-python:local, Dockerfile.python)
set -u
source "$(dirname "$0")/../comun.sh"; preparar "07-$1"; cd "$W"
N2=$N7/../N2-politica-de-texto-y-remediacion
mkdir repo && cd repo && git init -q
cp "$N2/politica/.editorconfig" "$N2/politica/.gitattributes" .
mkdir tools && cp "$N7/compuerta/compuerta.sh" "$N2/tools/check_text_policy.py" tools/
printf 'node_modules/\n.venv/\n__pycache__/\n' > .gitignore
if [ "$1" = node ]; then
  node --version | tr -d v | cut -d. -f1 > .nvmrc
  printf '{ "name": "demo", "version": "1.0.0", "private": true, "dependencies": { "is-number": "7.0.0" } }\n' > package.json
  npm install -s --no-audit --no-fund >/dev/null
  romper() { sed -i 's/"7.0.0"/"6.0.0"/' package.json; }
else
  python3 -c 'import platform;print(".".join(platform.python_version_tuple()[:2]))' > .python-version
  printf '[project]\nname = "demo"\nversion = "1.0.0"\nrequires-python = ">=3.12"\ndependencies = ["six==1.17.0"]\n' > pyproject.toml
  uv lock -q
  romper() { sed -i 's/six==1.17.0/six==1.16.0/' pyproject.toml; }
fi
git add -A; git commit -qm "entorno declarado"
titulo "$1: compuerta sobre el repo recién congelado"
rc tools/compuerta.sh
titulo "$1: se cambia la dependencia declarada sin regenerar el lock"
romper; git commit -qam "dependencia cambiada sin lock"
rc tools/compuerta.sh
