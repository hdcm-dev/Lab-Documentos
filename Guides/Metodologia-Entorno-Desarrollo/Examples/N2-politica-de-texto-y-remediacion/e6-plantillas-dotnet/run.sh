#!/usr/bin/env bash
# E6 — ¿Con qué BOM y EOL escribe el propio SDK? ¿dotnet format hace cumplir charset?
# Se corre dentro de una imagen del SDK (ver README: una vez con 8.0 y otra con 10.0).
set -uo pipefail
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
W=$(mktemp -d); cd "$W"
echo "== SDK $(dotnet --version)"
dotnet new console -o app >/dev/null 2>&1
( cd app && dotnet new sln -n app >/dev/null 2>&1; dotnet new editorconfig >/dev/null 2>&1; dotnet new gitignore >/dev/null 2>&1; dotnet new globaljson >/dev/null 2>&1 )
cd app
for f in $(ls -A); do
  [ -f "$f" ] || continue
  if [ "$(head -c3 "$f" | od -An -tx1 | tr -d ' \n')" = "efbbbf" ]; then bom="con BOM"; else bom="sin BOM"; fi
  printf '  %-18s %-8s líneas CRLF: %s\n' "$f" "$bom" "$(grep -c $'\r$' "$f")"
done
echo "-- reglas de texto en el .editorconfig que genera 'dotnet new editorconfig':"
tr -d '\r' < .editorconfig | grep -nE '^\[|charset|end_of_line|insert_final_newline' | head -12 | sed 's/^/    /'
echo "-- ¿la plantilla declara charset en algún lado?: $(grep -c '^charset' .editorconfig) línea(s)"

rm -f app.sln app.slnx global.json
for regla in utf-8-bom utf-8; do
  if [ "$regla" = utf-8-bom ]; then sed -i '1s/^\xef\xbb\xbf//' Program.cs; else sed -i '1s/^/\xef\xbb\xbf/' Program.cs; fi
  sed -i 's/\r$//' Program.cs
  echo "-- ¿dotnet format hace cumplir charset? [*.cs] charset = $regla; Program.cs (LF) con primeros bytes $(head -c3 Program.cs | od -An -tx1 | tr -d '\n'):"
  printf 'root = true\n[*.cs]\ncharset = %s\nend_of_line = lf\n' "$regla" > .editorconfig
  dotnet format whitespace app.csproj --verify-no-changes -v q 2>&1 | sed 's#/tmp/[^ ]*/app/##; s/ \[.*//' | head -3 | sed 's/^/    /'; echo "    --verify-no-changes exit=${PIPESTATUS[0]}"
  dotnet format whitespace app.csproj -v q >/dev/null 2>&1
  echo "    después de 'dotnet format whitespace': primeros bytes = $(head -c3 Program.cs | od -An -tx1 | tr -d '\n')"
done

echo "-- ¿La CLI respeta el BOM/EOL de un .csproj o una solución existentes al editarlos?"
W2=$(mktemp -d); cd "$W2"
dotnet new console -o a >/dev/null 2>&1; dotnet new classlib -o b >/dev/null 2>&1; dotnet new sln -n s >/dev/null 2>&1
SLN=$(ls s.sln* | head -1)
estado() { printf '    %-12s %s, líneas CRLF: %s\n' "$1" "$( [ "$(head -c3 "$1" | od -An -tx1 | tr -d ' \n')" = efbbbf ] && echo 'con BOM' || echo 'sin BOM')" "$(grep -c $'\r$' "$1")"; }
for f in a/a.csproj "$SLN"; do sed -i '1s/^\xef\xbb\xbf//; s/\r$//' "$f"; done
echo "   antes (se les quitó BOM y CR a mano):"; estado a/a.csproj; estado "$SLN"
dotnet sln "$SLN" add a/a.csproj >/dev/null 2>&1; dotnet add a/a.csproj reference b/b.csproj >/dev/null 2>&1
echo "   después de 'dotnet sln add' y 'dotnet add reference':"; estado a/a.csproj; estado "$SLN"

echo "-- Plantillas web (webapp, blazor, razorcomponent): BOM y CRLF por extensión (sin wwwroot/lib)"
W3=$(mktemp -d); cd "$W3"
dotnet new webapp -o w --no-restore >/dev/null 2>&1; dotnet new blazor -o bz --no-restore >/dev/null 2>&1; dotnet new razorcomponent -n Comp >/dev/null 2>&1
find . -type f ! -path '*/wwwroot/lib/*' ! -path '*/wwwroot/bootstrap/*' ! -name '*.png' ! -name '*.ico' | while read -r f; do
  ext=".${f##*.}"; case "$f" in *.cshtml.cs) ext=".cshtml.cs";; *.razor.css) ext=".razor.css";; *.cshtml.css) ext=".cshtml.css";; esac
  bom=0; [ "$(head -c3 "$f" | od -An -tx1 | tr -d ' \n')" = efbbbf ] && bom=1
  crlf=0; grep -q $'\r$' "$f" && crlf=1
  echo "$ext $bom $crlf"
done | awk '{t[$1]++; b[$1]+=$2; c[$1]+=$3} END {for (e in t) printf "    %-12s archivos=%-3d con BOM=%-3d con CRLF=%d\n", e, t[e], b[e], c[e]}' | sort

# --- Ampliación (ciclo 3): la dirección contraria (con BOM antes) y las plantillas de ítem ---
echo "-- ¿Y si el .csproj y la solución tienen BOM antes de editarlos?"
W4=$(mktemp -d); cd "$W4"
dotnet new console -o a >/dev/null 2>&1; dotnet new classlib -o b >/dev/null 2>&1; dotnet new sln -n s >/dev/null 2>&1
SLN=$(ls s.sln* | head -1)
for f in a/a.csproj "$SLN"; do sed -i 's/\r$//' "$f"; [ "$(head -c3 "$f" | od -An -tx1 | tr -d ' \n')" = efbbbf ] || sed -i '1s/^/\xef\xbb\xbf/' "$f"; done
echo "   antes (con BOM puesto a mano):"; estado a/a.csproj; estado "$SLN"
dotnet sln "$SLN" add a/a.csproj >/dev/null 2>&1; dotnet add a/a.csproj reference b/b.csproj >/dev/null 2>&1
echo "   después de 'dotnet sln add' y 'dotnet add reference':"; estado a/a.csproj; estado "$SLN"
echo "-- Plantillas de ítem (class, interface, razorcomponent) dentro de un proyecto: ¿con BOM?"
W5=$(mktemp -d); cd "$W5"; dotnet new console -o p >/dev/null 2>&1; cd p
dotnet new class -n Clase -o Items >/dev/null 2>&1; dotnet new interface -n IFace -o Items >/dev/null 2>&1; dotnet new razorcomponent -n Comp -o Items >/dev/null 2>&1
for f in Items/Clase.cs Items/IFace.cs Items/Comp.razor; do estado "$f"; done
echo "-- Sin BOM a mano: ¿'dotnet format whitespace' se lo repone al .razor? (.editorconfig sin charset)"
sed -i '1s/^\xef\xbb\xbf//' Items/Comp.razor; printf 'root = true\n[*]\nend_of_line = lf\n' > .editorconfig
dotnet format whitespace p.csproj -v q >/dev/null 2>&1; echo "   dotnet format whitespace rc=$?"; estado Items/Comp.razor
