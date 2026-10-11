#!/usr/bin/env bash
# E8-4 — El proyecto JavaScript de Visual Studio (.esproj) dentro de la solución:
# ¿hace cumplir el lock de npm? Necesita la imagen multi-runtime con SDK 10
# (lab-n8-poliglota:sdk10, ver README): la versión 1.0.6887863 del SDK de JSPS
# no carga en el MSBuild del SDK 8.
set -uo pipefail
AQUI=$(cd "$(dirname "$0")" && pwd)
IMG8=${IMG8:-lab-n8-poliglota:local}
IMG10=${IMG10:-lab-n8-poliglota:sdk10}
W=$(mktemp -d); trap 'rm -rf "$W"' EXIT
preparar() { rm -rf "$W/repo"; cp -a "$AQUI/../solucion" "$W/repo"; cp "$AQUI/proyecto/$1" "$W/repo/web/web.esproj"; }
run() { local img=$1; shift; docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 -v "$W/repo:/w" -w /w "$img" bash -c "$*"; }
lock() { python3 -c 'import json,sys;print("lock: is-odd", json.load(open(sys.argv[1]))["packages"]["node_modules/is-odd"]["version"])' "$W/repo/web/package-lock.json"; }

echo "### 1. Con el MSBuild del SDK 8 (imagen multi-runtime de la solución)"
preparar web.esproj; run "$IMG8" 'dotnet build web/web.esproj -v m 2>&1 | grep -E "error MSB|System.Text.Json, Version" | head -2 | cut -c1-220'

echo; echo "### 2. SDK 10: el .esproj de fábrica sobre un repo SIN deriva"
preparar web.esproj; printf '{"sdk":{"version":"10.0.400","rollForward":"disable"}}\n' > "$W/repo/global.json"
run "$IMG10" 'dotnet build web/web.esproj -v n 2>&1 | grep -E "^ *(npm install|npm ci|npm run build)$|Failed to run npm audit|web:|Build succeeded|Build FAILED" | sed "s/^ *//" | sort -u'
echo "--- directorios que dejó en web/:"; (cd "$W/repo/web" && ls -d */ | tr '\n' ' '; echo)

echo; echo "### 3. SDK 10: package.json cambiado a is-odd 2.0.0 sin regenerar el lock (deriva)"
preparar web.esproj; printf '{"sdk":{"version":"10.0.400","rollForward":"disable"}}\n' > "$W/repo/global.json"
sed -i 's/"is-odd": "3.0.1"/"is-odd": "2.0.0"/' "$W/repo/web/package.json"; lock
run "$IMG10" 'dotnet build web/web.esproj -v m 2>&1 | grep -E "Build succeeded|Build FAILED|error" | sed "s/^ *//" | sort -u | head -3'
lock; echo "(el lock se reescribió en silencio: la build «pasó» con otro grafo)"

echo; echo "### 4. SDK 10: misma deriva con ShouldRunNpmInstall=false + npm ci enganchado antes de RunNpmInstall"
preparar web.estricto.esproj; printf '{"sdk":{"version":"10.0.400","rollForward":"disable"}}\n' > "$W/repo/global.json"
sed -i 's/"is-odd": "3.0.1"/"is-odd": "2.0.0"/' "$W/repo/web/package.json"; lock
run "$IMG10" 'dotnet build web/web.esproj -v m 2>&1 | grep -E "EUSAGE|in sync|error MSB3073|Build succeeded|Build FAILED" | sed "s/^ *//" | sort -u | head -5 | cut -c1-200'
lock
echo "--- y sin deriva, la variante estricta construye"
preparar web.estricto.esproj; printf '{"sdk":{"version":"10.0.400","rollForward":"disable"}}\n' > "$W/repo/global.json"
run "$IMG10" 'dotnet build web/web.esproj -v n 2>&1 | grep -E "^ *(npm install|npm ci --no-audit --no-fund|npm run build)$|web:|Build succeeded|Build FAILED" | sed "s/^ *//" | sort -u'
