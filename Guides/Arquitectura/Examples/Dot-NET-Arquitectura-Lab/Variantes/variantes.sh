#!/usr/bin/env bash
# Variantes de la guía de arquitectura .NET (§3.7 y §4.10): programas aparte, fuera de lab.sh y de MyProject/.
# Se ejecuta dentro de mcr.microsoft.com/dotnet/sdk:10.0 con esta carpeta montada en /variantes:
#   docker run --rm --user "$(id -u):$(id -g)" -e IMAGEN=... -e DIGEST=... \
#     -v "$PWD":/variantes -w /variantes mcr.microsoft.com/dotnet/sdk:10.0 bash variantes.sh
# Produce: capturas/Vnn-*.txt con el mismo encabezado que las capturas del laboratorio.
set -u
export HOME=/tmp/home DOTNET_CLI_HOME=/tmp/home DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p "$HOME"
BASE=/variantes
CAP=$BASE/capturas
rm -rf "$CAP"; mkdir -p "$CAP"

cap() {
  local id=$1 name=$2; shift 2
  local f="$CAP/$id-$name.txt"
  {
    echo "# $id — $name"
    echo "# fecha: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
    echo "# imagen: ${IMAGEN:-desconocida}"
    echo "# digest: ${DIGEST:-desconocido}"
    echo "# sdk: $(dotnet --version)"
    echo "# directorio: $(pwd)"
    echo "# comando: $*"
    echo "# ---"
  } > "$f"
  bash -c "$*" >> "$f" 2>&1
  printf "\n# --- código de salida: %s\n" "$?" >> "$f"
}

cd "$BASE"
cap V01 build-producto-sin-precio "dotnet build ProductoSinPrecio/Demo"
cap V01 producto-sin-precio "dotnet run --no-build --project ProductoSinPrecio/Demo"
cap V02 build-pedidos "dotnet build Pedidos/Demo"
cap V02 registrar-pedido "dotnet run --no-build --project Pedidos/Demo"
find . -name bin -o -name obj | xargs rm -rf
