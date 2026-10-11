#!/usr/bin/env bash
# E2 de N4 — CPM fija versiones declaradas; el lock congela el grafo resuelto.
set -u
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 NUGET_PACKAGES=/tmp/nuget-packages
W=/tmp/w; rm -rf "$W"; mkdir -p "$W"; cp -r /src/App /src/Directory.Packages.props "$W"; cd "$W"
paso() { printf '\n===== %s =====\n' "$*"; }
entrada() { grep -A4 "\"$1\": {" App/packages.lock.json | grep -E '"type"|"requested"|"resolved"' | tr -s ' ' | tr '\n' ' '; echo; }

paso "1. CPM + transitive pinning: Options (transitiva) queda fijada por la versión central"
dotnet restore App 2>&1 | tail -n 1
echo "Logging   -> $(entrada Microsoft.Extensions.Logging)"
echo "Options   -> $(entrada Microsoft.Extensions.Options)"
echo "Primitives-> $(entrada Microsoft.Extensions.Primitives)"

paso "2. Sin transitive pinning la misma PackageVersion NO gobierna a la transitiva"
rm -f App/packages.lock.json
dotnet restore App -p:CentralPackageTransitivePinningEnabled=false 2>&1 | tail -n 1
echo "Options   -> $(entrada Microsoft.Extensions.Options)"

paso "3. Pinning no puede bajar una versión: Options 7.0.0 < 8.0.0 pedida por Logging"
rm -f App/packages.lock.json
sed -i 's/"Microsoft.Extensions.Options" Version="8.0.2"/"Microsoft.Extensions.Options" Version="7.0.0"/' Directory.Packages.props
dotnet restore App 2>&1 | grep -oE 'error NU1109.*' | head -n 1 | cut -c1-170; echo "exit=${PIPESTATUS[0]}"
sed -i 's/"Microsoft.Extensions.Options" Version="7.0.0"/"Microsoft.Extensions.Options" Version="8.0.2"/' Directory.Packages.props

paso "4. Con CPM, un Version en el .csproj es un error (la versión vive en un solo lugar)"
sed -i 's#<PackageReference Include="Microsoft.Extensions.Logging" />#<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.1" />#' App/App.csproj
dotnet restore App 2>&1 | grep -oE 'error NU1008.*' | head -n 1 | cut -c1-170; echo "exit=${PIPESTATUS[0]}"
sed -i 's#<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.1" />#<PackageReference Include="Microsoft.Extensions.Logging" />#' App/App.csproj

paso "5. 'Version' es un mínimo: pinear Primitives a 8.0.2, que NO existe en nuget.org"
rm -f App/packages.lock.json
sed -i 's#</ItemGroup>#  <PackageVersion Include="Microsoft.Extensions.Primitives" Version="8.0.2" />\n  </ItemGroup>#' Directory.Packages.props
frio() { rm -rf App/obj App/packages.lock.json "$NUGET_PACKAGES"; }   # caché fría entre variantes
echo "-- 5a. CON lock (RestorePackagesWithLockFile=true, el del .csproj):"
frio; dotnet restore App > r.log 2>&1; rc=$?
echo "   advertencias NU1603: $(grep -c 'NU1603' r.log)   restore rc=$rc"
echo "   Primitives-> $(entrada Microsoft.Extensions.Primitives)"
echo "-- 5b. SIN lock (-p:RestorePackagesWithLockFile=false):"
frio; dotnet restore App -p:RestorePackagesWithLockFile=false > r.log 2>&1; rc=$?
grep -oE 'warning NU1603: [^[]*' r.log | sort -u | sed 's/^/   /'; echo "   restore rc=$rc"
echo "-- 5c. SIN lock y NU1603 como error (-p:WarningsAsErrors=NU1603; en el repo, la línea de A.4):"
frio; dotnet restore App -p:RestorePackagesWithLockFile=false '-p:WarningsAsErrors=NU1603' > r.log 2>&1; rc=$?
grep -oE 'error NU1603: [^[]*' r.log | sort -u | sed 's/^/   /'; echo "   restore rc=$rc"
echo "-- 5d. CON lock y NU1603 como error: el lock desarma el aviso"
frio; dotnet restore App '-p:WarningsAsErrors=NU1603' > r.log 2>&1; rc=$?
echo "   advertencias o errores NU1603: $(grep -c 'NU1603' r.log)   restore rc=$rc"
echo "   entrada completa de Primitives en packages.lock.json:"
grep -A5 '"Microsoft.Extensions.Primitives": {' App/packages.lock.json | sed 's/^/   /'
echo "SDK: $(dotnet --version)"
