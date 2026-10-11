#!/usr/bin/env bash
# E7-03 — Qué le muestra al revisor un PR que toca el entorno.
set -u
source "$(dirname "$0")/../comun.sh"; preparar 03; cd "$W"
bash "$N7/compuerta/sembrar-repo.sh" "$W/repo"; cd "$W/repo"
LISTA="python3 $N7/03-revisor-cambios-de-entorno/cambios-de-entorno.py"

titulo "PR A: descongelamiento legítimo + un BOM colado + cambio de política"
git checkout -q -b pr-a
sed -i 's/13.0.3/13.0.4/' App.csproj && dotnet restore -v q >/dev/null       # paquete + lock, juntos
git commit -qam "Newtonsoft.Json 13.0.4"
printf '\xef\xbb\xbf' | cat - Program.cs > t && mv t Program.cs                   # el IDE guardó con BOM
git commit -qam "retoque en Program.cs"
printf '*.ps1 text eol=crlf\n' >> .gitattributes
git commit -qam "scripts de PowerShell con CRLF"
echo "-- diff de Program.cs que ve el revisor en la web:"; git diff main...pr-a -- Program.cs | sed -n '1,8p' | sed 's/^/   /'
$LISTA main pr-a
echo "-- y la compuerta del PR (misma de siempre):"; rc tools/compuerta.sh --base main

titulo "PR B: lock editado a mano, sin manifiesto"
git checkout -q main; git checkout -q -b pr-b
sed -i 's/"resolved": "13.0.3"/"resolved": "13.0.4"/' packages.lock.json
git commit -qam "ajuste del lock"
$LISTA main pr-b
echo "-- la compuerta del PR:"; rc tools/compuerta.sh --base main

titulo "PR C: SDK subido en global.json sin tocar la imagen de CI"
git checkout -q main; git checkout -q -b pr-c
python3 - <<'PY'
import json; g = json.load(open("global.json")); g["sdk"]["version"] = "8.0.426"; open("global.json", "w").write(json.dumps(g, indent=2) + "\n")
PY
git commit -qam "SDK 8.0.426"
$LISTA main pr-c
echo "-- la compuerta del PR, en la imagen de CI que sigue con 8.0.425:"; rc tools/compuerta.sh --base main
