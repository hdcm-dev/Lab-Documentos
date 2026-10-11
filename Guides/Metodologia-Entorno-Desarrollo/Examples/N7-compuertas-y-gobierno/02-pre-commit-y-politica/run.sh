#!/usr/bin/env bash
# E7-02 — Un framework de hooks genérico puede contradecir la política del repo.
set -u
source "$(dirname "$0")/../comun.sh"; preparar 02; cd "$W"
titulo "0. pre-commit fijado por versión en un venv propio"
python3 -m venv /tmp/pc >/dev/null && /tmp/pc/bin/pip install -q --disable-pip-version-check pre-commit==4.6.2
export PRE_COMMIT_HOME=$W/cache
pre-commit() { /tmp/pc/bin/pre-commit "$@"; }   # sin tocar PATH: python3 sigue siendo el de la imagen
pre-commit --version
bash "$N7/compuerta/sembrar-repo.sh" "$W/repo"; cd "$W/repo"
mkdir -p Pages; printf '\xef\xbb\xbf<h1>Configuración</h1>\n' > Pages/Index.razor   # con BOM, como pide la política (N2)
git add -A; git commit -qm "vista Razor con BOM, conforme a .editorconfig"; codif Pages/Index.razor
rc tools/compuerta.sh

titulo "1. Configuración ingenua: el hook genérico quita el BOM y la compuerta del repo lo reclama"
cp "$N7/02-pre-commit-y-politica/pre-commit-ingenuo.yaml" .pre-commit-config.yaml; git add .pre-commit-config.yaml
rc pre-commit run --all-files
codif Pages/Index.razor; codif packages.lock.json
git add -A; rc tools/compuerta.sh --staged

titulo "1b. El otro arreglo del hook genérico y la herramienta que genera el lock"
echo "-- en HEAD, tal como lo escribió NuGet:"; git show HEAD:packages.lock.json > /tmp/lock.head; codif /tmp/lock.head
echo "-- restore normal con el lock 'arreglado' por el hook (no hay cambios de dependencias):"
dotnet restore -v q >/dev/null; codif packages.lock.json
echo "-- restore que REGENERA el lock (--force-evaluate, como al descongelar):"
dotnet restore --force-evaluate -v q >/dev/null; codif packages.lock.json
git diff --stat HEAD -- packages.lock.json | sed 's/^/   diff contra HEAD: /'
git reset -q --hard HEAD

titulo "2. Configuración alineada con .editorconfig: nada que corregir"
cp "$N7/02-pre-commit-y-politica/pre-commit-alineado.yaml" .pre-commit-config.yaml; git add .pre-commit-config.yaml
rc pre-commit run --all-files
codif Pages/Index.razor

titulo "3. Un .razor nuevo sin BOM: lo ataja la compuerta, no el hook genérico"
printf '<p>Nueva</p>\n' > Pages/Nueva.razor; git add -A
rc pre-commit run
