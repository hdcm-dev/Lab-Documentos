#!/usr/bin/env bash
# Compara lo instalado en esta Mac contra la receta (versiones.env). Sirve igual en el runner de CI, en la VM y en el
# escenario de agente. Sale con error si algo no coincide; no instala nada.
#
# Uso: ci/receta/ios/verificar.sh [ruta a versiones.env]
set -uo pipefail

AQUI="$(cd "$(dirname "$0")" && pwd)"
DEMO="$(cd "$AQUI/../../.." && pwd)"
RECETA="${1:-$AQUI/versiones.env}"
# shellcheck disable=SC1090
set -a; . "$RECETA"; set +a

FALLAS=0
chequear() {  # chequear <qué> <esperado> <obtenido>
  if [ "$2" = "$3" ]; then echo "OK    $1: $3"; else echo "FALLA $1: se esperaba «$2», hay «$3»"; FALLAS=$((FALLAS + 1)); fi
}

GJ="$(sed -n 's/.*"version" *: *"\([^"]*\)".*/\1/p' "$DEMO/global.json" | head -1)"
chequear "SDK de global.json" "$DOTNET_VERSION" "$GJ"

XV="$(xcodebuild -version 2>/dev/null | sed -n 's/^Xcode //p')"
XB="$(xcodebuild -version 2>/dev/null | sed -n 's/^Build version //p')"
chequear "Xcode" "$XCODE_VERSION" "$XV"
chequear "Build de Xcode" "$XCODE_BUILD" "$XB"

RT="$(xcrun simctl list runtimes 2>/dev/null | grep -c "iOS ${IOS_RUNTIME}.* - ${IOS_RUNTIME_BUILD})")"
chequear "Runtime iOS ${IOS_RUNTIME} (${IOS_RUNTIME_BUILD})" "1" "$RT"

DV="$(cd "$DEMO" && dotnet --version 2>/dev/null)"
chequear "SDK .NET" "$DOTNET_VERSION" "$DV"

WL="$(dotnet workload list 2>/dev/null)"
chequear "Workload ios" "$WORKLOAD_IOS_MANIFEST" "$(printf '%s\n' "$WL" | awk '$1=="ios"{print $2}')"
chequear "Workload maui-ios" "$WORKLOAD_MAUI_MANIFEST" "$(printf '%s\n' "$WL" | awk '$1=="maui-ios"{print $2}')"

if command -v gdown >/dev/null 2>&1; then
  chequear "gdown" "$GDOWN_VERSION" "$(gdown --version 2>/dev/null | awk '{print $2}')"
else
  echo "--    gdown: no instalado (sólo hace falta para bajar el .xip)"
fi

[ "$FALLAS" -eq 0 ] && echo "Receta: todo coincide" || { echo "Receta: ${FALLAS} diferencia(s)"; exit 1; }
