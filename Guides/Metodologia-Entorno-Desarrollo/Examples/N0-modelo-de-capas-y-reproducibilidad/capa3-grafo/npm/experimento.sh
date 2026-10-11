#!/usr/bin/env bash
# E0-3a · Capa 3 (grafo de dependencias) en Node.js/npm. Corre dentro de node:22-bookworm-slim.
# Un RANGO declarado (^2.0.0) no es una versión: el resultado depende del día en que se resuelve.
# El lock file CONGELA la resolución; npm ci la hace cumplir.
set -u
trabajo=$(mktemp -d); cd "$trabajo"
echo "node $(node --version) · npm $(npm --version)"
echo "--- versiones publicadas de 'ms' que satisfacen ^2.0.0:"
npm view 'ms@^2.0.0' version 2>/dev/null | tr '\n' ' '; echo

echo '{ "name": "demo-rango", "version": "1.0.0", "private": true, "dependencies": { "ms": "^2.0.0" } }' > package.json
echo "--- (a) npm install sin lock: el rango resuelve a la MÁS ALTA compatible"
npm install --no-audit --no-fund --loglevel=error >/dev/null
node -e 'console.log("    instalada:", require("ms/package.json").version)'

echo "--- (b) lock congelado en 2.0.0 (simula el lock de hace años) + npm ci"
rm -rf node_modules package-lock.json
npm install ms@2.0.0 --save-exact=false --no-audit --no-fund --loglevel=error >/dev/null
# package.json vuelve al rango original; el lock queda con 2.0.0 (el rango la admite)
node -e 'const p=require("./package.json");p.dependencies.ms="^2.0.0";require("fs").writeFileSync("package.json",JSON.stringify(p,null,2))'
rm -rf node_modules
npm ci --no-audit --no-fund --loglevel=error >/dev/null
node -e 'console.log("    instalada:", require("ms/package.json").version)'
echo "    integrity en el lock: $(node -e 'console.log(require("./package-lock.json").packages["node_modules/ms"].integrity.slice(0,30)+"…")')"

echo "--- (c) alguien cambia el rango a ^2.1.0 y no regenera el lock: npm ci"
node -e 'const p=require("./package.json");p.dependencies.ms="^2.1.0";require("fs").writeFileSync("package.json",JSON.stringify(p,null,2))'
npm ci --no-audit --no-fund 2>&1 | grep -E 'npm error' | head -4
echo "    código de salida: ${PIPESTATUS[0]}"
cd /; rm -rf "$trabajo"
