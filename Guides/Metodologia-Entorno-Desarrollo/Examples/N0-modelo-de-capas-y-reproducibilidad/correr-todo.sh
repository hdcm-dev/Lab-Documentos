#!/usr/bin/env bash
# Corre los seis experimentos del núcleo N0 y deja la salida de cada uno en salidas/.
# Requiere docker en el host (y buildx para E0-4). Todo el build ocurre en directorios
# temporales DENTRO de los contenedores: no quedan bin/, obj/ ni node_modules/ en el repo.
set -u
cd "$(dirname "$0")"
env_net=(-e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1)
docker run --rm "${env_net[@]}" -v "$PWD/capa1-bytes":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/experimento.sh 2>&1 | tee salidas/E0-1-capa1-bytes.txt
for t in 8.0 10.0; do
  docker run --rm "${env_net[@]}" -e IMAGEN=sdk:$t -v "$PWD/capa2-toolchain":/src:ro -v "$PWD/capa5-proceso":/calc:ro mcr.microsoft.com/dotnet/sdk:$t bash /src/experimento.sh
done 2>&1 | tee salidas/E0-2-capa2-toolchain.txt
docker run --rm -v "$PWD/capa3-grafo/npm":/src:ro node:22-bookworm-slim bash /src/experimento.sh 2>&1 | tee salidas/E0-3a-capa3-grafo-npm.txt
docker run --rm "${env_net[@]}" -v "$PWD/capa3-grafo/nuget":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/experimento.sh 2>&1 | tee salidas/E0-3b-capa3-grafo-nuget.txt
docker run --rm -v "$PWD/capa3-grafo/uv":/src:ro python:3.12-slim bash /src/experimento.sh 2>&1 | tee salidas/E0-3c-capa3-grafo-uv.txt
bash capa4-imagen/experimento.sh 2>&1 | tee salidas/E0-4-capa4-imagen.txt
docker run --rm "${env_net[@]}" -v "$PWD/capa5-proceso":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/experimento.sh 2>&1 | tee salidas/E0-5-capa5-proceso.txt
bash capa6-procedencia/experimento.sh 2>&1 | tee salidas/E0-6-capa6-procedencia.txt
# Verificación adversarial (2026-10-07): experimentos complementarios E0-2b y E0-5b.
docker run --rm -v "$PWD/capa2-toolchain":/src:ro node:22-bookworm-slim bash /src/engines-npm.sh 2>&1 | tee salidas/E0-2b-engines-npm.txt
docker run --rm "${env_net[@]}" -v "$PWD/capa5-proceso":/src:ro mcr.microsoft.com/dotnet/sdk:8.0 bash /src/verificacion-sourceroot.sh 2>&1 | tee salidas/E0-5b-verificacion-sourceroot.txt
