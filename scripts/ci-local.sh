#!/usr/bin/env bash
# docs-guard local: los mismos pasos, en el mismo orden, que .github/workflows/docs-guard.yml.
# Expediente 0002 de DocAsCode: compuerta mínima en E1-U0a; grupos PR-1 a PR-7 completos en E1-U2 (N09 §8.1).
#
# Uso:  scripts/ci-local.sh [PASO] [RUTA]
#   RUTA  carpeta a verificar, relativa a la raíz del repositorio que se verifica.
#         Por defecto, la guía del sistema documentador (GUIA-DIR).
#
# Pasos (cada grupo termina con su propio código; «todo» los corre todos y agrega):
#   herramientas  lychee (versión y SHA-256 fijos) y el parser de mermaid (npm ci sobre un lockfile) en la caché.
#   enlaces       PR-1  lychee --offline --include-fragments; enlaces relativos que salen del repositorio.
#   esquema       PR-2  frontmatter contra su esquema, registro cerrado de prefijos, esquema RRD.
#   ai            PR-3  anti-inyección AI-1..AI-5. AI-3 avisa; AI-4 avisa en el anexo de referencias.
#   estructura    PR-4  markdownlint-cli2 estructural (MD013 apagado) y mermaid que parsea.
#   trazas        PR-5  traces-resolve: cada ID@versión contra el registro generado de los encabezados. Las anclas
#                       ruta#Símbolo@SHA de las fichas las verifica el workflow de su source.lock (E1-U3).
#   contrato      PR-6  diff del contrato OpenAPI (si la ruta tiene contracts/openapi.json).
#   fijadas       PR-7  piezas fijadas con su SHA-256 (equivalente de «copiar.py --check») y reflint.
#   autoprueba    G-c y AK-7: cada control da rojo sobre su fixture rojo y verde sobre el verde; el fixture
#                 permanente AK-7 tiene que dar rojo en cada corrida.
#   avisos        url-fecha y cita-linea sobre lo heredado. Nunca bloquea.
#   todo          herramientas y después todos los grupos; agregador estricto: rojo si cualquiera da rojo.
#
# Requiere bash, curl, tar, sha256sum, Python 3 (sólo biblioteca estándar) y Node.js (npm, npx).
# No usa contenedores: el runner tampoco.
set -uo pipefail

PASO="${1:-todo}"
RUTA="${2:-Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion}"

AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DG="$(cd "$AQUI/../tools/docs-guard" && pwd)"
FX="$DG/fixtures"
LYCHEE_VERSION="0.18.1"
LYCHEE_SHA256="0d48569e2dc11c1917fa98321dfd7aa675fba03b79f41e8e74afa3223e30d379"
MARKDOWNLINT_CLI2_VERSION="0.17.2"
CACHE="${DOCS_GUARD_CACHE:-${RUNNER_TEMP:-${TMPDIR:-/tmp}}/docs-guard}"
LYCHEE="$CACHE/lychee-$LYCHEE_VERSION/lychee"
MERMAID_DIR="$CACHE/mermaid-$(sha256sum "$DG/mermaid/package-lock.json" | cut -c1-12)"

py() { python3 -I "$DG/dg.py" "$@"; }

# CL-03 (E1-U4e): el AGENTS.md de la raíz del repositorio que se verifica (el directorio actual) entra en los grupos
# enlaces, ai y estructura junto con la RUTA. Su exclusión de AI-3 va por huella en tools/docs-guard/excepciones.txt
# (AK-3). Si no existe, enlaces sólo avisa: el script lo usan también otros repositorios por el workflow reutilizable.
PUNTEROS=()
[[ -f AGENTS.md ]] && PUNTEROS+=(AGENTS.md)

herramientas() {
  echo "== herramientas: lychee $LYCHEE_VERSION, mermaid (lockfile), markdownlint-cli2 $MARKDOWNLINT_CLI2_VERSION"
  # Cada orden lleva su «|| return 1»: «set -e» no rige dentro de una función llamada desde «||».
  if [[ ! -x "$LYCHEE" ]]; then
    mkdir -p "$(dirname "$LYCHEE")" || return 1
    local tgz="$CACHE/lychee-$LYCHEE_VERSION.tar.gz"
    curl -fsSL -o "$tgz" \
      "https://github.com/lycheeverse/lychee/releases/download/lychee-v$LYCHEE_VERSION/lychee-x86_64-unknown-linux-gnu.tar.gz" || return 1
    echo "$LYCHEE_SHA256  $tgz" | sha256sum -c - || { rm -f "$tgz"; return 1; }
    tar -xzf "$tgz" -C "$(dirname "$LYCHEE")" || return 1
  fi
  "$LYCHEE" --version || return 1
  if [[ ! -d "$MERMAID_DIR/node_modules/mermaid" ]]; then
    mkdir -p "$MERMAID_DIR" || return 1
    cp "$DG/mermaid/package.json" "$DG/mermaid/package-lock.json" "$MERMAID_DIR/" || return 1
    (cd "$MERMAID_DIR" && npm ci --ignore-scripts --no-audit --no-fund --loglevel=error) || { rm -rf "$MERMAID_DIR"; return 1; }
  fi
  cp "$DG/mermaid/mermaid-parse.mjs" "$MERMAID_DIR/" || return 1
  echo "mermaid $(node -p "require('$MERMAID_DIR/node_modules/mermaid/package.json').version")"
}

lychee_offline() {
  "$LYCHEE" --offline --include-fragments --no-progress --config "$AQUI/docs-guard.lychee.toml" "$@"
}

markdownlint() {
  npx --yes "markdownlint-cli2@$MARKDOWNLINT_CLI2_VERSION" --config "$AQUI/docs-guard.markdownlint-cli2.jsonc" "$@"
}

mermaid() { node "$MERMAID_DIR/mermaid-parse.mjs" "$@"; }

# Cada grupo devuelve 0 (verde) o distinto de 0 (rojo). Se corren todos sus controles aunque uno falle.
enlaces() {
  echo "== PR-1 enlaces: lychee --offline --include-fragments y relativos fuera del repo ($RUTA ${PUNTEROS[*]})"
  local rc=0
  [[ ${#PUNTEROS[@]} -eq 0 ]] && echo "aviso: no hay AGENTS.md en la raíz del repositorio (CL-03)"
  lychee_offline "$RUTA" "${PUNTEROS[@]}" || rc=1
  py fuera "$RUTA" "${PUNTEROS[@]}" || rc=1
  return $rc
}
esquema()    { echo "== PR-2 esquema ($RUTA)"; py esquema "$RUTA"; }
ai()         { echo "== PR-3 anti-inyección ($RUTA ${PUNTEROS[*]})"; py ai "$RUTA" "${PUNTEROS[@]}"; }
estructura() {
  echo "== PR-4 estructura: markdownlint-cli2 y mermaid ($RUTA ${PUNTEROS[*]})"
  local rc=0
  markdownlint "$RUTA/**/*.md" "${PUNTEROS[@]}" || rc=1
  mermaid "$RUTA" "${PUNTEROS[@]}" || rc=1
  return $rc
}
trazas()     { echo "== PR-5 trazas ($RUTA)"; py trazas "$RUTA"; }
contrato()   { echo "== PR-6 contrato ($RUTA)"; py contratos "$RUTA"; }
fijadas() {
  echo "== PR-7 fijadas y reflint ($RUTA)"
  local rc=0
  py fijadas "$RUTA" || rc=1
  py reflint "$RUTA" || rc=1
  return $rc
}
avisos()     { echo "== avisos: url-fecha y cita-linea ($RUTA)"; py avisos "$RUTA"; return 0; }

# Autoprueba: «espera» es rojo (código distinto de 0) o verde (0). Un control que no da lo esperado es una falla
# de la compuerta misma, y la corrida da rojo.
FALLAS_AUTO=0
espera() {
  local esperado="$1" nombre="$2"; shift 2
  local salida rc
  salida="$("$@" 2>&1)"; rc=$?
  local obtenido=verde; [[ $rc -ne 0 ]] && obtenido=rojo
  if [[ "$obtenido" == "$esperado" ]]; then
    printf '  ok    %-28s %s (rc=%s)\n' "$nombre" "$obtenido" "$rc"
  else
    printf '  FALLA %-28s esperaba %s y dio %s (rc=%s)\n' "$nombre" "$esperado" "$obtenido" "$rc"
    printf '%s\n' "$salida" | tail -5 | sed 's/^/        /'
    FALLAS_AUTO=$((FALLAS_AUTO + 1))
  fi
}
cuenta() {  # cuenta CONTROL SALIDA → el número AI-n=N de la última línea
  printf '%s\n' "$2" | tail -1 | grep -oE "$1=[0-9]+" | cut -d= -f2
}
autoprueba() {
  echo "== autoprueba: pares rojo/verde por control (G-c) y fixture permanente AK-7"
  FALLAS_AUTO=0
  local c
  # Con GITHUB_ACTIONS los hallazgos de los fixtures no deben anotarse en el PR: se corren sin anotaciones.
  local -x GITHUB_ACTIONS=false
  espera rojo  'PR-1 lychee'          lychee_offline "$FX/enlaces/rojo"
  espera verde 'PR-1 lychee'          lychee_offline "$FX/enlaces/verde"
  declare -A grupo=([fuera]=PR-1 [esquema]=PR-2 [trazas]=PR-5 [fijadas]=PR-7 [reflint]=PR-7)
  for c in fuera esquema trazas fijadas reflint; do
    espera rojo  "${grupo[$c]} $c" py "$c" --raiz "$FX/$c/rojo"  "$FX/$c/rojo"
    espera verde "${grupo[$c]} $c" py "$c" --raiz "$FX/$c/verde" "$FX/$c/verde"
  done
  # Anclas de nivel 2 (E1-U3): la ficha contra una fuente de prueba con su source.lock.
  espera rojo  'PR-5 anclas'          py trazas --raiz "$FX/anclas/rojo"  --fuente "$FX/anclas/rojo/fuente"  "$FX/anclas/rojo"
  espera verde 'PR-5 anclas'          py trazas --raiz "$FX/anclas/verde" --fuente "$FX/anclas/verde/fuente" "$FX/anclas/verde"
  for c in ai-1 ai-2 ai-4 ai-5; do
    espera rojo  "PR-3 ${c^^}" py ai --raiz "$FX/$c/rojo" "$FX/$c/rojo"
  done
  espera verde 'PR-3 AI (página limpia)'   py ai --raiz "$FX/ai/verde" "$FX/ai/verde"
  espera verde 'PR-3 AI-3 avisa'           py ai --raiz "$FX/ai-3/aviso" "$FX/ai-3/aviso"
  espera verde 'PR-3 AI-4 avisa en anexo'  py ai --raiz "$FX/ai-4-anexo/aviso" "$FX/ai-4-anexo/aviso"
  espera rojo  'PR-4 markdownlint'    markdownlint "$FX/estructura/rojo/**/*.md"
  espera verde 'PR-4 markdownlint'    markdownlint "$FX/estructura/verde/**/*.md"
  espera rojo  'PR-4 mermaid'         mermaid "$FX/mermaid/rojo"
  espera verde 'PR-4 mermaid'         mermaid "$FX/mermaid/verde"
  espera rojo  'PR-6 contrato'        py contrato "$FX/contrato/rojo/esperado.json" "$FX/contrato/rojo/generado.json"
  espera verde 'PR-6 contrato'        py contrato "$FX/contrato/verde/esperado.json" "$FX/contrato/verde/generado.json"
  # AK-7: el fixture permanente da rojo y cada chequeo que bloquea lo detecta; AI-3 lo ve como aviso.
  local s rc
  s="$(py ai --raiz "$FX/ak7" "$FX/ak7" 2>&1)"; rc=$?
  local faltan=""
  for c in AI-1 AI-2 AI-3 AI-4 AI-5; do
    [[ "$(cuenta "$c" "$s")" -ge 1 ]] 2>/dev/null || faltan="$faltan $c"
  done
  if [[ $rc -eq 1 && -z "$faltan" ]]; then
    echo "  ok    AK-7 fixture permanente      rojo (rc=1; AI-1..AI-5 detectados)"
  else
    echo "  FALLA AK-7 fixture permanente      rc=$rc; no detectados:${faltan:- ninguno}"
    FALLAS_AUTO=$((FALLAS_AUTO + 1))
  fi
  echo "== autoprueba: $([[ $FALLAS_AUTO -eq 0 ]] && echo verde || echo "ROJO ($FALLAS_AUTO fallas)")"
  return $FALLAS_AUTO
}

GRUPOS=(enlaces esquema ai estructura trazas contrato fijadas autoprueba)

todo() {
  herramientas || { echo "== docs-guard: ROJO (herramientas)"; return 1; }
  local g rc total=0
  declare -A res
  for g in "${GRUPOS[@]}"; do
    "$g"; rc=$?
    res[$g]=$rc
    [[ $rc -ne 0 ]] && total=1
    echo
  done
  avisos >/dev/null
  echo "== agregador docs-guard"
  for g in "${GRUPOS[@]}"; do
    printf '  %-12s %s\n' "$g" "$([[ ${res[$g]} -eq 0 ]] && echo verde || echo ROJO)"
  done
  if [[ $total -eq 0 ]]; then echo "== docs-guard: verde"; else echo "== docs-guard: ROJO"; fi
  return $total
}

[[ -d "$RUTA" ]] || { echo "RUTA no existe: $RUTA" >&2; exit 2; }

case "$PASO" in
  herramientas) herramientas ;;
  enlaces|esquema|ai|estructura|trazas|contrato|fijadas|autoprueba|avisos) "$PASO" ;;
  todo) todo ;;
  *) echo "paso desconocido: $PASO" >&2; exit 2 ;;
esac
