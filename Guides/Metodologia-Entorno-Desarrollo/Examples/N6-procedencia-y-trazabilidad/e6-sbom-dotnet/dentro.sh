#!/usr/bin/env bash
# Corre DENTRO de mcr.microsoft.com/dotnet/sdk:8.0, en /src (copia de la pieza con UNA dependencia NuGet).
set -eu
export PATH="$PATH:/tmp/herr"
dotnet tool install --tool-path /tmp/herr CycloneDX --version 6.2.0 >/dev/null
dotnet tool install --tool-path /tmp/herr Microsoft.Sbom.DotNetTool --version 4.1.13 >/dev/null
dotnet restore -v q >/dev/null
dotnet build -c Release --no-restore -nologo -v q -o /src/drop >/dev/null
# (a) CycloneDX: lee el grafo NuGet resuelto (project.assets.json) del proyecto.
dotnet-CycloneDX Pieza.csproj -o /src/sbom -fn pieza.cdx.json -F Json >/dev/null 2>&1 || echo "cyclonedx falló"
# (b) sbom-tool de Microsoft: SPDX 2.2 sobre la carpeta de salida (drop) + componentes detectados en el fuente.
sbom-tool generate -b /src/drop -bc /src -pn pieza -pv 1.4.0 -ps "Ejemplo Lab" -nsb https://ejemplo.invalid/sbom -V Error >/dev/null 2>&1 || echo "sbom-tool falló"
