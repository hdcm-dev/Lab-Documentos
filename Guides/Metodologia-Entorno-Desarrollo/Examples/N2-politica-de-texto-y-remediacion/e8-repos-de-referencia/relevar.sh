#!/bin/sh
# E2-8 (mesa, ciclo 1). ¿Qué declaran repos .NET públicos de referencia sobre
# BOM, finales de línea, lock files de NuGet y rollForward? Sólo lectura (raw + API pública de GitHub).
# Relevamiento FECHADO: la salida versionada es la del 2026-10-07 y no se re-corre en cada corrida
# (los repositorios cambian). Uso, desde la carpeta de N2: sh e8-repos-de-referencia/relevar.sh
set -u
echo "fecha de consulta: $(date -u +%Y-%m-%dT%H:%MZ)"
for r in dotnet/eShop dotnet/roslyn dotnet/aspnetcore dotnet/runtime dotnet/sdk dotnet/msbuild; do
  raw="https://raw.githubusercontent.com/$r/main"
  sha=$(curl -sf "https://api.github.com/repos/$r/commits/main" | python3 -I -c 'import json,sys; print(json.load(sys.stdin)["sha"][:12])' 2>/dev/null)
  echo "=================== $r @ main $sha"
  echo "-- .gitattributes (reglas globales y de eol, sin comentarios)"
  curl -sf "$raw/.gitattributes" | grep -v '^#' | grep -E '^\*[[:space:]]|eol=' | head -6
  echo "-- .editorconfig (secciones con charset)"
  curl -sf "$raw/.editorconfig" | awk '/^\[/{sec=$0} /^[[:space:]]*charset/{print sec" -> "$0}'
  echo "-- global.json: rollForward"
  curl -sf "$raw/global.json" | grep -E '"rollForward"|"version"' | head -2
  echo "-- packages.lock.json versionados (árbol completo de main)"
  curl -sf "https://api.github.com/repos/$r/git/trees/main?recursive=1" | python3 -I -c '
import json,sys
d=json.load(sys.stdin); p=[t["path"] for t in d["tree"]]
L=[x for x in p if x.endswith("packages.lock.json")]
print("archivos=%d truncado=%s packages.lock.json=%d Directory.Packages.props=%d" % (len(p), d.get("truncated"), len(L), sum(x.endswith("Directory.Packages.props") for x in p)))
for x in L: print("   ", x)'
done
