#!/usr/bin/env bash
# diff-base.sh — AC-10 (plan §F.10, H-050, H2-051): todo cambio de la demo frente a la línea de base
# figura en la tabla de delta del README.
#
#   BASE_REPO=<clon de Lab-E2E.WebBlazor> tools/diff-base.sh
#
# 1. Exporta el commit 8fb4c3a con «git archive» (sólo lectura sobre el caso base) a un directorio
#    temporal FUERA del repo (DIFF_BASE_DIR, o mktemp), y lo borra al terminar.
# 2. En la copia exportada cambia MovilidadUrbana.ApiWeb por MovilidadUrbana.Asistente (H2-008).
# 3. Compara archivo por archivo contra la demo, sin las líneas de cabecera «Procedencia:»/«Delta:»
#    (el criterio E1 de procedencia las agrega a cada copia).
# 4. Cada archivo copiado que quedó distinto tiene que declarar su «Delta:» en la cabecera; cada
#    archivo de src/ o tests/ que no está en la base tiene que rotularse «Nuevo»; y todos tienen que
#    aparecer en el README de la demo (por su nombre o por la carpeta de su fila). Los .json no
#    admiten comentarios: su delta va sólo en el README.
# Sale con 0 si todo cambio está declarado; 1 si no; 2 si falta algo para correr.
set -uo pipefail

COMMIT=8fb4c3a
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEMO="$(dirname "$AQUI")"
README="$DEMO/README.md"
: "${BASE_REPO:?BASE_REPO debe apuntar a un clon de Lab-E2E.WebBlazor}"
git -C "$BASE_REPO" cat-file -e "$COMMIT^{commit}" 2>/dev/null || { echo "BASE_REPO no tiene el commit $COMMIT" >&2; exit 2; }
[ -f "$README" ] || { echo "falta $README" >&2; exit 2; }

TMP="${DIFF_BASE_DIR:-$(mktemp -d)}"; mkdir -p "$TMP"
trap 'rm -rf "$TMP"' EXIT
git -C "$BASE_REPO" archive "$COMMIT" src/MovilidadUrbana.ApiWeb tests/MovilidadUrbana.ApiWeb.Tests \
  src/MovilidadUrbana.Web/Sessions/SessionMiddleware.cs src/MovilidadUrbana.Web/wwwroot/css/Tokens.css \
  src/MovilidadUrbana.Web/wwwroot/css/Componentes.css | tar -x -C "$TMP" || exit 2

# base → demo
destino() {
  case "$1" in
    src/MovilidadUrbana.ApiWeb/MovilidadUrbana.ApiWeb.csproj) echo src/MovilidadUrbana.Asistente/MovilidadUrbana.Asistente.csproj ;;
    tests/MovilidadUrbana.ApiWeb.Tests/MovilidadUrbana.ApiWeb.Tests.csproj) echo tests/MovilidadUrbana.Asistente.Tests/MovilidadUrbana.Asistente.Tests.csproj ;;
    src/MovilidadUrbana.ApiWeb/*) echo "src/MovilidadUrbana.Asistente/${1#src/MovilidadUrbana.ApiWeb/}" ;;
    tests/MovilidadUrbana.ApiWeb.Tests/*) echo "tests/MovilidadUrbana.Asistente.Tests/${1#tests/MovilidadUrbana.ApiWeb.Tests/}" ;;
    src/MovilidadUrbana.Web/*) echo "src/MovilidadUrbana.Asistente/${1#src/MovilidadUrbana.Web/}" ;;
  esac
}
sin_cabecera() { grep -vE '^[[:space:]]*(//|/\*|<!--|@\*)?[[:space:]]*(Procedencia|Delta):' "$1"; }
en_readme() {  # nombre del archivo, o alguna carpeta de su ruta seguida de «/», en el README
  local rel="$1" dir
  grep -qF "$(basename "$rel")" "$README" && return 0
  dir="$(dirname "$rel")"
  while [ "$dir" != "." ] && [ "$dir" != "/" ]; do
    grep -qF "$(basename "$dir")/" "$README" && return 0
    dir="$(dirname "$dir")"
  done
  return 1
}

rc=0; iguales=0; distintos=0; nuevos=0; quitados=0
declare -A copiados=()
printf '%-11s %s\n' estado archivo
while IFS= read -r base; do
  rel="${base#"$TMP"/}"; dst="$(destino "$rel")"; copiados["$dst"]=1
  if [ ! -f "$DEMO/$dst" ]; then
    printf '%-11s %s\n' quitado "$dst"; quitados=$((quitados + 1))
    en_readme "$dst" || { echo "  ✗ quitado sin fila en el README: $dst"; rc=1; }
    continue
  fi
  sed -i 's/MovilidadUrbana\.ApiWeb/MovilidadUrbana.Asistente/g' "$base"
  if diff -q <(sin_cabecera "$base") <(sin_cabecera "$DEMO/$dst") >/dev/null; then
    iguales=$((iguales + 1)); continue
  fi
  distintos=$((distintos + 1)); printf '%-11s %s\n' distinto "$dst"
  case "$dst" in
    *.json) ;;
    *) head -n 15 "$DEMO/$dst" | grep -q 'Delta:' || { echo "  ✗ cambió y no declara «Delta:» en la cabecera"; rc=1; } ;;
  esac
  en_readme "$dst" || { echo "  ✗ sin fila en el README"; rc=1; }
done < <(find "$TMP" -type f | sort)

while IFS= read -r f; do
  rel="${f#"$DEMO"/}"
  [ -n "${copiados[$rel]:-}" ] && continue
  nuevos=$((nuevos + 1)); printf '%-11s %s\n' nuevo "$rel"
  head -n 3 "$f" | grep -qi 'nuevo' || { echo "  ✗ no está en la base y no se rotula «Nuevo»"; rc=1; }
  en_readme "$rel" || { echo "  ✗ sin fila en el README"; rc=1; }
done < <(find "$DEMO/src" "$DEMO/tests" -type f -not -path '*/bin/*' -not -path '*/obj/*' | sort)

echo "diff-base ($COMMIT): $iguales iguales tras la cabecera, $distintos distintos, $nuevos nuevos, $quitados quitados"
[ $rc = 0 ] && echo "AC-10: todo cambio figura en la tabla de delta" || echo "AC-10: hay cambios sin declarar"
exit $rc
