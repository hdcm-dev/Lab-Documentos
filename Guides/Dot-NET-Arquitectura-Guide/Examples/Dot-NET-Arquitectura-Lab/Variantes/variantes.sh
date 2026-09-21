#!/usr/bin/env bash
# Variantes de la guía de arquitectura .NET (§3.7, §4.10, §5.5 y §3.3/§5.4): programas aparte, fuera de lab.sh y de MyProject/.
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
cap V03 cambiar-precio "dotnet run --no-build --project Pedidos/Demo -- cambiar-precio"
cap V04 build-fabrica-y-resultado "dotnet build FabricaYResultado/Demo && dotnet build FabricaYResultado/Demo -c Release && echo && echo '# avisos por ignorar el resultado de un metodo (CA1806/IDE0058), con el catalogo completo de analisis activado:' && dotnet build FabricaYResultado/Demo -p:AnalysisMode=All -p:EnforceCodeStyleInBuild=true --no-incremental 2>&1 | grep -E 'CA1806|IDE0058' | wc -l"
cap V05 ciclo-efcore "dotnet run --no-build --project FabricaYResultado/Demo -- ciclo"
cap V06 rechazos "ulimit -c 0; dotnet run --no-build -c Release --project FabricaYResultado/Demo -- rechazos"   # ulimit: el escenario termina con una excepción no atrapada y no debe dejar un volcado
cap V07 sin-ctor-privado "dotnet run --no-build --project FabricaYResultado/Demo -- sin-ctor"
cap V08 enum-y-valor "dotnet run --no-build --project FabricaYResultado/Demo -- valores"
cap V09 materializadores "dotnet run --no-build --project FabricaYResultado/Demo -- materializadores"
find . -name bin -o -name obj | xargs rm -rf
