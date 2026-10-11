#!/usr/bin/env bash
# E1 de N4 — packages.lock.json y modo bloqueado (locked mode) en NuGet.
# Corre DENTRO del contenedor del SDK; trabaja sobre una copia en /tmp para no dejar
# obj/, bin/ ni lock files generados en el repositorio.
set -u
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
export NUGET_PACKAGES=/tmp/nuget-packages   # carpeta global de paquetes limpia en cada corrida
W=/tmp/w; rm -rf "$W"; cp -r /src/App "$W"; cd "$W"
paso() { printf '\n===== %s =====\n' "$*"; }

paso "SDK"; dotnet --version

paso "1. Primer restore: genera packages.lock.json (grafo completo con contentHash)"
dotnet restore 2>&1 | tail -n 2
grep -nE '"Microsoft.Extensions.(Logging|Primitives)"|"type"|"resolved"|"contentHash"' packages.lock.json | head -n 12
cp packages.lock.json /tmp/lock.original.json

paso "2. Restore en modo bloqueado con el lock coherente: pasa"
dotnet restore --locked-mode 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"

paso "3. Deriva: alguien sube la dependencia directa en el .csproj y no regenera el lock"
sed -i 's/Version="8.0.0"/Version="8.0.1"/' App.csproj
dotnet restore --locked-mode 2>&1 | grep -E 'error|NU1004' | head -n 3
echo "exit=${PIPESTATUS[0]}"

paso "4. El mismo cambio con restore común: NO falla y reescribe el lock en silencio"
dotnet restore 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"
diff <(grep -E '^      "[A-Za-z.]+": \{|"(resolved|requested)"' /tmp/lock.original.json) <(grep -E '^      "[A-Za-z.]+": \{|"(resolved|requested)"' packages.lock.json)

paso "5. Integridad: se altera el contentHash de un paquete en el lock y se restaura desde cero"
cp /tmp/lock.original.json packages.lock.json
sed -i 's/Version="8.0.1"/Version="8.0.0"/' App.csproj
# Se reemplaza el contentHash de Microsoft.Extensions.Primitives por uno falso (mismo largo).
awk 'BEGIN{p=0} /"Microsoft.Extensions.Primitives"/{p=1} p && /"contentHash"/{sub(/"contentHash": "[^"]+"/,"\"contentHash\": \"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==\""); p=0} {print}' packages.lock.json > l.tmp && mv l.tmp packages.lock.json
rm -rf "$NUGET_PACKAGES" obj
dotnet restore --locked-mode 2>&1 | grep -E 'error|NU1403' | head -n 3
echo "exit=${PIPESTATUS[0]}"

paso "6. Plataforma: el lock se generó sin RID; un restore para linux-x64 en modo bloqueado"
cp /tmp/lock.original.json packages.lock.json; rm -rf obj
dotnet restore --locked-mode -r linux-x64 2>&1 | grep -E 'error|NU1004' | head -n 3
echo "exit=${PIPESTATUS[0]}"

paso "7. Declarando los RID de todas las plataformas el lock tiene una sección por RID"
sed -i 's#<RestorePackagesWithLockFile>#<RuntimeIdentifiers>win-x64;linux-x64</RuntimeIdentifiers>\n    <RestorePackagesWithLockFile>#' App.csproj
rm -f packages.lock.json; dotnet restore 2>&1 | tail -n 1
grep -nE '^    "net8.0[^"]*"' packages.lock.json
echo "-- restore bloqueado con el mismo conjunto de RID (sin -r):"
dotnet restore --locked-mode 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"
echo "-- restore bloqueado forzando un único RID con -r (el conjunto cambia):"
dotnet restore --locked-mode -r linux-x64 2>&1 | grep -E 'NU1004' | head -n 1 | cut -c1-160
echo "exit=${PIPESTATUS[0]}"
