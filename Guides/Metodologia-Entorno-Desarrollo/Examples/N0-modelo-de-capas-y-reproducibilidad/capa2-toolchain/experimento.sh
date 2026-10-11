#!/usr/bin/env bash
# E0-2 · Capa 2 (toolchain). Corre dentro de cualquier imagen mcr.microsoft.com/dotnet/sdk:<tag>.
# Muestra qué SDK y qué compilador elige el host cuando el repo NO declara la capa, y qué pasa
# cuando global.json la declara de forma exacta (rollForward=disable) o con un rango.
set -u
trabajo=$(mktemp -d); cd "$trabajo"
echo "=== imagen: ${IMAGEN:-?}"
echo "--- sin global.json: SDK elegido = $(dotnet --version)"
sdkdir=$(dirname "$(dotnet --list-sdks | tail -1 | sed -E 's/^([^ ]+) \[(.*)\]$/\2\/\1/')/x")
echo "--- compilador C# de ese SDK: $(dotnet "$sdkdir/Roslyn/bincore/csc.dll" -version 2>/dev/null)"

echo '{ "sdk": { "version": "8.0.100", "rollForward": "disable" } }' > global.json
echo "--- global.json exacto 8.0.100 + rollForward=disable:"
dotnet --version 2>&1 | grep -E "^[0-9]|compatible|Requested|global.json file|Installed SDKs|^ *[0-9]+\.[0-9]+\.[0-9]+ \[" | head -6

echo '{ "sdk": { "version": "8.0.100", "rollForward": "latestFeature" } }' > global.json
echo "--- global.json 8.0.100 + rollForward=latestFeature (rango dentro de 8.0):"
dotnet --version 2>&1 | grep -E "^[0-9]|compatible|Requested|global.json file|Installed SDKs|^ *[0-9]+\.[0-9]+\.[0-9]+ \[" | head -6
rm -f global.json
if [ -d /calc ]; then
  # Mismo fuente (capa5-proceso), misma ruta, mismo TargetFramework net8.0: sólo cambia el SDK.
  mkdir -p /w/a && cp /calc/Calculo.csproj /calc/Calculo.cs /w/a/
  ( cd /w/a && dotnet build -c Release -nologo -v q >/dev/null 2>&1 )
  echo "--- sha256 de Calculo.dll (net8.0, Release, ruta /w/a) compilada con este SDK: $(sha256sum /w/a/bin/Release/net8.0/Calculo.dll | cut -c1-16)"
fi
cd /; rm -rf "$trabajo"
