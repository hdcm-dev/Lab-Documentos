#!/usr/bin/env bash
# E7-04 (parte A) — Descongelar un paquete: declarar sin regenerar falla; declarar + regenerar en el mismo commit pasa.
set -u
source "$(dirname "$0")/../comun.sh"; preparar 04a; cd "$W"
bash "$N7/compuerta/sembrar-repo.sh" "$W/repo"; cd "$W/repo"
git checkout -q -b descongelar/newtonsoft-13.0.4

titulo "A1. Se cambia la versión declarada y se intenta commitear sin regenerar el lock"
sed -i 's/13.0.3/13.0.4/' App.csproj; git add App.csproj
rc git -c core.hooksPath=.githooks commit -qm "Newtonsoft.Json 13.0.4 (sin lock)"

titulo "A2. Se regenera el lock con la herramienta (restore sin --locked-mode) y se commitea junto"
dotnet restore -v q >/dev/null; git add App.csproj packages.lock.json
rc git -c core.hooksPath=.githooks commit -qm "Newtonsoft.Json 13.0.3 -> 13.0.4 (manifiesto + lock)"
git show --stat --format='   commit %h %s' HEAD | sed 's/^/   /'
echo "-- lo que el revisor ve:"
python3 "$N7/03-revisor-cambios-de-entorno/cambios-de-entorno.py" main HEAD | sed 's/^/   /'
