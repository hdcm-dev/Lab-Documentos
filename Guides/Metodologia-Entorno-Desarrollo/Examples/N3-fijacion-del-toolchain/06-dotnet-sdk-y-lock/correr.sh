#!/bin/sh
# El lock file de NuGet registra paquetes que aporta el SDK: cambiar de SDK rompe --locked-mode.
# Uso (desde esta carpeta): sh correr.sh
# Trabaja en un directorio temporal fuera del repo: no deja lock, bin ni obj.
T=$(mktemp -d); cp -r Lib "$T/"
NUGET_CACHE="${NUGET_CACHE:-$HOME/.cache/n3-nuget}"; mkdir -p "$NUGET_CACHE"
run() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 \
  -e NUGET_PACKAGES=/nuget -v "$NUGET_CACHE":/nuget -v "$T/Lib":/w -w /w "$1" sh -c "$2" 2>&1; }
echo "--- 1. Genera packages.lock.json con el SDK 8"
run mcr.microsoft.com/dotnet/sdk:8.0 'dotnet --version; dotnet restore -v q; echo rc=$?'
grep -A3 '"Microsoft.NET.ILLink.Tasks"' "$T/Lib/packages.lock.json"
echo "--- 2. Restauración estricta con el mismo SDK 8"
run mcr.microsoft.com/dotnet/sdk:8.0 'dotnet restore --locked-mode -v q; echo rc=$?'
echo "--- 3. Restauración estricta con el SDK 10 (mismo lock, mismo código)"
run mcr.microsoft.com/dotnet/sdk:10.0 'dotnet --version; dotnet restore --locked-mode -v q; echo rc=$?' | grep -E "^[0-9]|NU1004|rc="
rm -rf "$T"
