#!/bin/sh
# Hace cumplir lo que el repo DECLARA sobre el toolchain. Mismo script en local y en CI.
# Chequea sólo los ecosistemas cuya herramienta está presente; falla (rc=1) ante cualquier desvío.
# Imprime una línea "toolchain:" que la CI puede guardar como registro (ver núcleo N6).
cd "$(dirname "$0")"
fallas=0; registro=""
if command -v dotnet >/dev/null 2>&1 && [ -f global.json ]; then
  # global.json se hace cumplir solo: si no hay SDK compatible, `dotnet --version` falla.
  if v=$(dotnet --version 2>/dev/null); then echo "OK   dotnet sdk $v (global.json)"; registro="$registro dotnet-sdk=$v"
  else echo "FALLA dotnet: ningún SDK instalado satisface global.json"; fallas=1; fi
fi
if command -v node >/dev/null 2>&1 && [ -f .nvmrc ]; then
  pedido=$(tr -d ' \r\n' < .nvmrc); real=$(node -v | sed 's/^v//')
  case "$real" in "$pedido"|"$pedido".*) echo "OK   node $real (.nvmrc=$pedido)";;
    *) echo "FALLA node $real no coincide con .nvmrc=$pedido"; fallas=1;; esac
  registro="$registro node=$real npm=$(npm -v)"
fi
if command -v python3 >/dev/null 2>&1 && [ -f .python-version ]; then
  pedido=$(head -n1 .python-version | tr -d ' \r\n'); real=$(python3 -c 'import platform;print(platform.python_version())')
  case "$real" in "$pedido"|"$pedido".*) echo "OK   python $real (.python-version=$pedido)";;
    *) echo "FALLA python $real no coincide con .python-version=$pedido"; fallas=1;; esac
  registro="$registro python=$real"
fi
echo "toolchain:$registro"
exit $fallas
