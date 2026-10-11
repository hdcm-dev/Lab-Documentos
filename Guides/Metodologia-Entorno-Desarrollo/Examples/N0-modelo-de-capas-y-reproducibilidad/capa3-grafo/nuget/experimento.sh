#!/usr/bin/env bash
# E0-3b · Capa 3 (grafo) en NuGet. Corre dentro de mcr.microsoft.com/dotnet/sdk:8.0.
# Muestra que la palabra «rango» no significa lo mismo en todos los ecosistemas:
# NuGet elige la versión MÁS BAJA que satisface un mínimo, y la más alta sólo con versión flotante.
# Después congela el grafo con packages.lock.json y lo hace cumplir con --locked-mode.
set -u
trabajo=$(mktemp -d); cd "$trabajo"
proyecto() { # $1 = valor del atributo Version
cat > Demo.csproj <<XML
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="Newtonsoft.Json" Version="$1" /></ItemGroup>
</Project>
XML
}
resuelta() { grep -A3 '"Newtonsoft.Json"' packages.lock.json | grep -m1 '"resolved"' | sed 's/^ *//'; }

echo "--- (a) Version=\"13.0\" (mínimo inclusivo): resuelve la más baja existente >= 13.0"
proyecto "13.0"; dotnet restore -v q >/dev/null; echo "    $(resuelta)"
echo "--- (b) Version=\"13.*\" (flotante): resuelve la más alta 13.x"
rm -f packages.lock.json; proyecto "13.*"; dotnet restore -v q >/dev/null; echo "    $(resuelta)"
echo "--- (c) lock congelado con Version=\"13.0.1\"; alguien sube a 13.0.3 sin regenerar; restore --locked-mode"
rm -f packages.lock.json; proyecto "13.0.1"; dotnet restore -v q >/dev/null; echo "    lock: $(resuelta)"
proyecto "13.0.3"
dotnet restore --locked-mode 2>&1 | grep -oE 'error NU[0-9]+: .*' | sed -E 's# \[/tmp[^]]*\]$##' | sort -u | head -2
echo "    código de salida: ${PIPESTATUS[0]}"
cd /; rm -rf "$trabajo"
