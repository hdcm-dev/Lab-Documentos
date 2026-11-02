#!/usr/bin/env bash
# Recorrido de humo del cascarón en el simulador de iOS (lo usa .github/workflows/framework-concept-ios.yml; corre
# igual a mano en una Mac con Xcode).
#
# Instala el .app, abre la app, espera la marca «FC-MARCA primer-render» en el registro unificado (subsistema
# com.frameworkconcept.demo) y deja como evidencia: la bitácora FC, la captura de pantalla, el video del arranque y
# el TPR. Sale con error si la marca no aparece en el plazo: eso es lo que prueba que el contenido web arrancó.
#
# No hay toques en pantalla: el contenido es web (HybridWebView) y la marca la escribe el propio contenido.
#
# Uso: scripts/humo-ios.sh <ruta al .app> <UDID del simulador> <carpeta de salida> [segundos de espera]
set -euo pipefail

APP="$1"
UDID="$2"
SALIDA="$3"
PLAZO="${4:-180}"
BUNDLE="com.frameworkconcept.demo"

mkdir -p "$SALIDA"
SALIDA="$(cd "$SALIDA" && pwd)"

echo "== Simulador $UDID"
xcrun simctl bootstatus "$UDID" -b
xcrun simctl list devices | grep "$UDID" || true

echo "== Instalar $APP"
codesign -dv "$APP" 2>&1 | grep -E "Identifier|Signature" || true
xcrun simctl uninstall "$UDID" "$BUNDLE" 2>/dev/null || true
xcrun simctl install "$UDID" "$APP"
xcrun simctl listapps "$UDID" | grep -q "$BUNDLE" || { echo "ERROR: la app no quedó registrada"; exit 1; }
echo "instalada: $BUNDLE"

echo "== Bitácora y video"
xcrun simctl spawn "$UDID" log stream --level info --style compact \
  --predicate "subsystem == \"$BUNDLE\"" > "$SALIDA/bitacora-fc.txt" 2>&1 &
LOG_PID=$!
# El video se corta con SIGINT para que el MP4 quede cerrado y se pueda reproducir.
xcrun simctl io "$UDID" recordVideo --codec h264 --force "$SALIDA/arranque.mp4" > "$SALIDA/video.txt" 2>&1 &
REC_PID=$!
sleep 3

echo "== Abrir"
xcrun simctl launch "$UDID" "$BUNDLE"

ESTADO=1
for ((i = 0; i < PLAZO; i++)); do
  if grep -q "FC-MARCA primer-render" "$SALIDA/bitacora-fc.txt"; then ESTADO=0; break; fi
  sleep 1
done
sleep 3   # que el primer render llegue a la captura

xcrun simctl io "$UDID" screenshot "$SALIDA/primer-render.png" || true
kill -s INT "$REC_PID" 2>/dev/null || true
wait "$REC_PID" 2>/dev/null || true
kill "$LOG_PID" 2>/dev/null || true
wait "$LOG_PID" 2>/dev/null || true

# Registro completo de la app, por si la marca no llegó (incluye lo que no es de la bitácora FC).
xcrun simctl spawn "$UDID" log show --last 10m --info --style compact \
  --predicate "process == \"FrameworkConcept.Shell\"" > "$SALIDA/registro-proceso.txt" 2>&1 || true
find "$HOME/Library/Logs/DiagnosticReports" -name "FrameworkConcept*" -mmin -30 -exec cp {} "$SALIDA/" \; 2>/dev/null || true

echo "== Bitácora FC"
cat "$SALIDA/bitacora-fc.txt"

if [ "$ESTADO" -ne 0 ]; then
  echo "ERROR: no apareció «FC-MARCA primer-render» en ${PLAZO} s"
  exit 1
fi
LINEA="$(grep -m1 "FC-MARCA primer-render" "$SALIDA/bitacora-fc.txt")"
TPR="$(printf '%s' "$LINEA" | grep -oE 'tpr=[0-9]+ms' || true)"
printf '%s\n' "$LINEA" > "$SALIDA/tpr.txt"
echo "== Primer render: ${TPR:-sin tpr en la línea}"
echo "$LINEA"
