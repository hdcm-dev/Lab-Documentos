#!/usr/bin/env bash
# docs-guard local: los mismos pasos, en el mismo orden, que .github/workflows/docs-guard.yml.
#
# Uso:  scripts/ci-local.sh [todo|herramientas|enlaces|estructura] [RUTA]
#   RUTA  carpeta a verificar, relativa a la raíz del repositorio que se verifica.
#         Por defecto, la guía del sistema documentador (GUIA-DIR).
#
# Pasos:
#   1. herramientas  descarga lychee (versión y SHA-256 fijos) a $DOCS_GUARD_CACHE.
#   2. enlaces       lychee --offline --include-fragments sobre RUTA (enlaces internos y anclas).
#   3. estructura    markdownlint-cli2 (versión fija) con reglas sólo estructurales sobre RUTA.
#
# Requiere bash, curl, tar, sha256sum y Node.js (npx). No usa contenedores: el runner tampoco.
set -euo pipefail

PASO="${1:-todo}"
RUTA="${2:-Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion}"

AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LYCHEE_VERSION="0.18.1"
LYCHEE_SHA256="0d48569e2dc11c1917fa98321dfd7aa675fba03b79f41e8e74afa3223e30d379"
MARKDOWNLINT_CLI2_VERSION="0.17.2"
CACHE="${DOCS_GUARD_CACHE:-${RUNNER_TEMP:-${TMPDIR:-/tmp}}/docs-guard}"
LYCHEE="$CACHE/lychee-$LYCHEE_VERSION/lychee"

herramientas() {
  echo "== 1/3 herramientas: lychee $LYCHEE_VERSION"
  if [[ -x "$LYCHEE" ]]; then echo "ya está en $LYCHEE"; return 0; fi
  mkdir -p "$(dirname "$LYCHEE")"
  local tgz="$CACHE/lychee-$LYCHEE_VERSION.tar.gz"
  curl -fsSL -o "$tgz" \
    "https://github.com/lycheeverse/lychee/releases/download/lychee-v$LYCHEE_VERSION/lychee-x86_64-unknown-linux-gnu.tar.gz"
  echo "$LYCHEE_SHA256  $tgz" | sha256sum -c -
  tar -xzf "$tgz" -C "$(dirname "$LYCHEE")"
  "$LYCHEE" --version
}

enlaces() {
  echo "== 2/3 enlaces: lychee --offline --include-fragments $RUTA"
  "$LYCHEE" --offline --include-fragments --no-progress \
    --config "$AQUI/docs-guard.lychee.toml" "$RUTA"
}

estructura() {
  echo "== 3/3 estructura: markdownlint-cli2 $MARKDOWNLINT_CLI2_VERSION $RUTA"
  npx --yes "markdownlint-cli2@$MARKDOWNLINT_CLI2_VERSION" \
    --config "$AQUI/docs-guard.markdownlint-cli2.jsonc" "$RUTA/**/*.md"
}

[[ -d "$RUTA" ]] || { echo "RUTA no existe: $RUTA" >&2; exit 2; }

case "$PASO" in
  herramientas) herramientas ;;
  enlaces)      enlaces ;;
  estructura)   estructura ;;
  todo)         herramientas; enlaces; estructura; echo "== docs-guard: verde" ;;
  *) echo "paso desconocido: $PASO" >&2; exit 2 ;;
esac
