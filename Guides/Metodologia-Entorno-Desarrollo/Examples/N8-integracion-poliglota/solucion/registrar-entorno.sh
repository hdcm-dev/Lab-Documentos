#!/usr/bin/env bash
# Registro mínimo del entorno de los TRES ecosistemas (insumo de N6, sin firma).
set -euo pipefail
echo "dotnet-sdk=$(dotnet --version)"
echo "node=$(node --version)"
echo "npm=$(npm --version)"
echo "python=$(cd tools/gen && uv python find --show-version)"
echo "uv=$(uv --version | cut -d' ' -f2)"
echo "imagen=${IMAGEN_DIGEST:-NO DECLARADA (la CI la pasa en IMAGEN_DIGEST)}"
echo "sha256 src/Saludo/packages.lock.json=$(sha256sum src/Saludo/packages.lock.json | cut -d' ' -f1)"
echo "sha256 web/package-lock.json=$(sha256sum web/package-lock.json | cut -d' ' -f1)"
echo "sha256 tools/gen/uv.lock=$(sha256sum tools/gen/uv.lock | cut -d' ' -f1)"
echo "sha256 Saludo.dll=$(sha256sum src/Saludo/bin/Release/net8.0/Saludo.dll | cut -d' ' -f1)"
