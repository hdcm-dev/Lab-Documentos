#!/usr/bin/env bash
# E3 de N4 — package source mapping y contentHash frente a un paquete impostor
# (mismo Id y versión, otro contenido). Todo con feeds locales: no necesita red.
set -u
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 NUGET_PACKAGES=/tmp/gp
paso() { printf '\n===== %s =====\n' "$*"; }
fuente() { grep -o '"source": *"[^"]*"' "$NUGET_PACKAGES/contoso.util/1.0.0/.nupkg.metadata"; }
limpiar() { rm -rf "$NUGET_PACKAGES" /tmp/app/obj /tmp/app/bin; }

paso "0. Se empaqueta Contoso.Util 1.0.0 dos veces: feed 'interno' (legítimo) y feed 'publico' (impostor)"
mkdir -p /tmp/feeds/interno /tmp/feeds/publico
cp -r /src/paquete /tmp/pkg && cd /tmp/pkg
dotnet pack -c Release -o /tmp/feeds/interno -p:Origen=interno  2>&1 | grep -o "Successfully created package.*" | sed 's#.*/feeds/#feeds/#'
rm -rf obj bin
dotnet pack -c Release -o /tmp/feeds/publico -p:Origen=impostor 2>&1 | grep -o "Successfully created package.*" | sed 's#.*/feeds/#feeds/#'
sha256sum /tmp/feeds/*/Contoso.Util.1.0.0.nupkg | sed 's#/tmp/##'
cp -r /src/App /tmp/app; cd /tmp/app

paso "1. CON mapeo (Contoso.* -> interno): la fuente queda determinada y se genera el lock"
cp /src/configs/nuget.con-mapeo.config nuget.config; limpiar
dotnet restore 2>&1 | tail -n 1; fuente
dotnet run --no-restore 2>/dev/null
grep -o '"contentHash": "[^"]*"' packages.lock.json

paso "2. Caché caliente: el paquete ya está en la carpeta global y el mapeo NO se consulta"
limpiar; rm -f packages.lock.json
printf '<configuration><packageSources><clear /><add key="publico" value="/tmp/feeds/publico" /></packageSources></configuration>' > nuget.config
dotnet restore >/dev/null 2>&1; echo "restore previo sólo con 'publico': $(fuente)"
cp /src/configs/nuget.con-mapeo.config nuget.config; rm -rf obj
dotnet restore 2>&1 | tail -n 1; echo "restore con mapeo y caché caliente: $(fuente)"
dotnet run --no-restore 2>/dev/null

paso "3. El impostor llega con el lock legítimo commiteado y restore bloqueado: el contentHash lo delata"
# Se regenera el lock legítimo (fuente 'interno', caché limpia), como el que estaría en el repo:
limpiar; rm -f packages.lock.json; dotnet restore >/dev/null 2>&1
grep -o '"contentHash": "[^"]*"' packages.lock.json
# Ahora la única fuente alcanzable sirve el impostor (feed comprometido o republicación):
limpiar
printf '<configuration><packageSources><clear /><add key="publico" value="/tmp/feeds/publico" /></packageSources></configuration>' > nuget.config
dotnet restore --locked-mode 2>&1 | grep -oE 'error NU1403.*' | head -n 1 | cut -c1-170; echo "exit=${PIPESTATUS[0]}"

paso "4. SIN mapeo y con ambas fuentes: la fuente elegida no está declarada en ningún lado"
cp /src/configs/nuget.sin-mapeo.config nuget.config; limpiar; rm -f packages.lock.json
dotnet restore 2>&1 | grep -E 'warning|Restored' | cut -c1-170; fuente

paso "5. Mapeo incompleto: un paquete sin patrón que lo cubra hace fallar el restore"
cp /src/configs/nuget.mapeo-incompleto.config nuget.config; limpiar; rm -f packages.lock.json
dotnet restore 2>&1 | grep -oE 'error NU1100.*' | head -n 1 | cut -c1-230; echo "exit=${PIPESTATUS[0]}"
