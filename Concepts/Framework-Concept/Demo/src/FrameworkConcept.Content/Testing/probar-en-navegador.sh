#!/usr/bin/env bash
# Prueba el contenido Blazor en un navegador headless con el PUENTE SIMULADO, sin cascarón ni teléfono.
#
#   src/FrameworkConcept.Content/Testing/probar-en-navegador.sh [carpeta-de-capturas]
#
# Pasos:
#   1. publica las dos variantes del contenido (v1 = versionado/contenido.json, v2 = 1.1.0) con scripts/contenedor.sh;
#   2. en el contenedor de Playwright, sirve cada variante con Testing/puente-simulado.js en lugar de
#      _framework/hybridwebview.js y recorre las pantallas (Testing/recorrido.mjs);
#   3. deja capturas contenido-navegador-*.png, cada una con su .txt de tres líneas.
# Sale con código 1 si alguna comprobación falla.
#
# Qué NO prueba: el cascarón real, el WebView de Android ni el transporte de HybridWebView. Eso es la
# integración en el teléfono.
#
# Variables:
#   FC_TRABAJO     carpeta para lo publicado (por defecto salida/contenido-navegador dentro de la demo; no se versiona)
#   FC_PLAYWRIGHT  imagen de Playwright (por defecto mcr.microsoft.com/playwright:v1.62.1-noble)
#   FC_NPM         volumen Docker donde se instala playwright-core una sola vez (por defecto fc-npm)
#   FC_SIN_PUBLICAR=1  salta el paso 1 y usa lo ya publicado en FC_TRABAJO (para repetir sólo el recorrido)
set -euo pipefail

CONTENIDO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEMO="$(cd "$CONTENIDO/../.." && pwd)"
TRABAJO="${FC_TRABAJO:-$DEMO/salida/contenido-navegador}"
CAPTURAS="${1:-$TRABAJO/capturas}"
IMAGEN="${FC_PLAYWRIGHT:-mcr.microsoft.com/playwright:v1.62.1-noble}"
NPM="${FC_NPM:-fc-npm}"
VERSION_PLAYWRIGHT=1.62.1

mkdir -p "$TRABAJO" "$CAPTURAS"
TRABAJO="$(cd "$TRABAJO" && pwd)"
CAPTURAS="$(cd "$CAPTURAS" && pwd)"

if [ "${FC_SIN_PUBLICAR:-0}" != 1 ]; then
echo "== 1. Publicar v1 y v2 del contenido"
# Se recompila entera cada variante (--no-incremental): la v2 cambia una constante de compilación.
FC_DOCKER="-v $TRABAJO:/trabajo" "$DEMO/scripts/contenedor.sh" bash -c '
  set -e
  P=src/FrameworkConcept.Content
  dotnet build   $P -c Release --no-incremental                          | grep -E "Advertencia|Errores|error"
  dotnet publish $P -c Release --no-build -o /trabajo/v1                 | tail -1
  dotnet build   $P -c Release --no-incremental -p:ContenidoVersion=1.1.0 | grep -E "Advertencia|Errores|error"
  dotnet publish $P -c Release --no-build -p:ContenidoVersion=1.1.0 -o /trabajo/v2 | tail -1
'
fi

echo "== 2. Recorrido en Chromium headless con el puente simulado"
if ! docker volume inspect "$NPM" >/dev/null 2>&1; then
  # Un volumen nuevo es de root: se le da a quien corre la prueba, que es quien instala ahí.
  docker volume create "$NPM" >/dev/null
  docker run --rm -v "$NPM":/npm "$IMAGEN" chown "$(id -u):$(id -g)" /npm
fi
docker run --rm --name "fc-content-navegador-$$" --ipc=host \
  -u "$(id -u):$(id -g)" -e HOME=/tmp \
  -e PLAYWRIGHT_BROWSERS_PATH=/ms-playwright \
  -e PLAYWRIGHT_CORE=/npm/node_modules/playwright-core \
  -e V1=/trabajo/v1/wwwroot -e V2=/trabajo/v2/wwwroot \
  -e SIM=/pruebas/puente-simulado.js -e DEFINICIONES=/definiciones -e SALIDA=/capturas \
  -v "$NPM":/npm \
  -v "$CONTENIDO/Testing":/pruebas:ro \
  -v "$DEMO/definiciones":/definiciones:ro \
  -v "$TRABAJO":/trabajo:ro \
  -v "$CAPTURAS":/capturas \
  "$IMAGEN" bash -c "
    set -e
    [ -d /npm/node_modules/playwright-core ] || npm install --silent --prefix /npm playwright-core@$VERSION_PLAYWRIGHT
    node /pruebas/recorrido.mjs
  "

echo "== Capturas en $CAPTURAS"
