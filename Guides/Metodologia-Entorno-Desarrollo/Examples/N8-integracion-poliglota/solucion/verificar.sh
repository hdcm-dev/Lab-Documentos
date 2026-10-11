#!/usr/bin/env bash
# Compuerta ÚNICA de la solución políglota: la misma en la estación, el
# devcontainer y la CI (N7). Tres pasos, uno por capa del modelo N0:
#   1) bytes del fuente  2) toolchain  3) grafo de dependencias.
set -uo pipefail
cd "$(dirname "$0")"
fallas=0

echo "== 1. Texto (N2)"
python3 verificar-texto.py . || fallas=$((fallas+1))

echo "== 2. Toolchain (N3): lo instalado contra los archivos del repo"
esperado_sdk=$(python3 -c 'import json;print(json.load(open("global.json"))["sdk"]["version"])')
esperado_node=$(tr -d '[:space:]' < .nvmrc)
esperado_py=$(tr -d "[:space:]" < tools/gen/.python-version)
real_sdk=$(dotnet --version 2>&1 | tail -1)
real_node=$(node --version 2>/dev/null | sed 's/^v//')
real_py=$(cd tools/gen && uv python find --show-version 2>/dev/null)
for par in "dotnet-sdk $esperado_sdk $real_sdk" "node $esperado_node $real_node" "python $esperado_py $real_py"; do
  set -- $par
  if [ "$2" = "$3" ]; then echo "toolchain: $1 $3 OK"; else echo "toolchain: $1 esperado $2, encontrado ${3:-ausente}"; fallas=$((fallas+1)); fi
done

echo "== 3. Restauración estricta (N4): un lock por ecosistema"
dotnet restore src/Saludo --locked-mode -v q --nologo || fallas=$((fallas+1))
(cd web && npm ci --no-audit --no-fund --loglevel=error) || fallas=$((fallas+1))
(cd tools/gen && uv sync --locked -q) || fallas=$((fallas+1))

echo "== compuerta: $([ $fallas -eq 0 ] && echo OK || echo "FALLA ($fallas)")"
exit $fallas
