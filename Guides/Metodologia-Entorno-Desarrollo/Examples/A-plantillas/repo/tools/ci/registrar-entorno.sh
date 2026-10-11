#!/bin/sh
# Anexo A.17. Registro del entorno (guía 9.6; 12, paso 8a). Lo corre la CI después de construir,
# en la misma imagen: tools/ci/registrar-entorno.sh <artefacto>... > registro-entorno.txt
# Sin firma: sirve para diagnosticar y reconstruir, no prueba nada ante un tercero (9.2.4).
set -eu
echo "commit=$(git rev-parse HEAD) arbol_limpio=$(git diff --quiet HEAD && echo si || echo NO)"
# El digest se declara UNA vez, en el container: del workflow (A.12); el registro lo lee de ahí.
W=${WORKFLOW_FILE:-.github/workflows/ci.yml}
[ -f "$W" ] || { echo "registro: no existe $W (indicá la ruta del workflow en WORKFLOW_FILE)" >&2; exit 1; }
img=$(sed -n 's/^ *\(container\|image\): *\([^ ]*@sha256:[0-9a-f]\{64\}\).*/\2/p' "$W" | head -1)
echo "imagen=${img:-NO DECLARADA (runner alojado: capa 4 en el log «Set up job», mientras dure su retención)}"
sdk=$(dotnet --version); raiz=$(dirname "$(readlink -f "$(command -v dotnet)")")
echo "dotnet_sdk=$sdk compilador_csharp=$(dotnet exec "$raiz/sdk/$sdk/Roslyn/bincore/csc.dll" -version)"
echo "node=$(node --version) npm=$(npm --version)"
echo "python=$(python3 -c 'import platform;print(platform.python_version())') uv=$(uv --version | cut -d' ' -f2)"
git ls-files '*packages.lock.json' '*package-lock.json' '*uv.lock' | while read -r f; do
  echo "sha256 $f=$(sha256sum "$f" | cut -d' ' -f1)"; done
for a in "$@"; do echo "sha256 $a=$(sha256sum "$a" | cut -d' ' -f1)"; done
