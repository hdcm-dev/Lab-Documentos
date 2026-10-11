#!/usr/bin/env bash
# E4 de N4 — npm: lock, npm ci, integridad y binarios por plataforma; y el equivalente en pnpm.
# Corre en node:22-bookworm-slim sobre una copia en /tmp (no deja node_modules en el repo).
set -u
export npm_config_update_notifier=false npm_config_fund=false npm_config_audit=false
W=/tmp/w; rm -rf "$W"; mkdir -p "$W"; cp /src/package.json "$W"; cd "$W"
paso() { printf '\n===== %s =====\n' "$*"; }
PNPM_VERSION="${PNPM_VERSION:-10.18.0}"

paso "Versiones"; echo "node $(node -v) / npm $(npm -v)"

paso "1. npm install genera package-lock.json (lockfileVersion, resolved, integrity)"
npm install --ignore-scripts --no-audit 2>&1 | tail -n 1
node -e 'const l=require("./package-lock.json");const p=l.packages["node_modules/is-number"];console.log("lockfileVersion",l.lockfileVersion);console.log("is-number",p.version,p.resolved);console.log("integrity",p.integrity)'
cp package-lock.json /tmp/lock.original.json

paso "2. El lock es multiplataforma: registra los binarios opcionales de TODAS las plataformas"
node -e 'const l=require("./package-lock.json");const k=Object.keys(l.packages).filter(k=>k.startsWith("node_modules/@esbuild/"));console.log(k.length+" entradas @esbuild/* en el lock");for(const n of ["linux-x64","win32-x64","darwin-arm64"]){const p=l.packages["node_modules/@esbuild/"+n];console.log(n,"os=",p.os,"cpu=",p.cpu,"optional=",p.optional)}'
echo "instalados en este Linux: $(ls node_modules/@esbuild)"

paso "3. npm ci con lock coherente: instala exactamente el lock"
npm ci --ignore-scripts 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"

paso "4. Deriva: package.json pide is-odd ^2 y el lock no se regeneró"
sed -i 's/"is-odd": "\^3.0.0"/"is-odd": "^2.0.0"/' package.json
npm ci --ignore-scripts 2>&1 | grep -E '^npm error' | head -n 3; echo "exit=${PIPESTATUS[0]}"

paso "5. Con npm install el mismo cambio no falla: reescribe el lock en silencio"
npm install --ignore-scripts 2>&1 | tail -n 1; echo "exit=${PIPESTATUS[0]}"
diff <(node -e 'const l=require("/tmp/lock.original.json");for(const[k,v]of Object.entries(l.packages))if(k.includes("is-"))console.log(k,v.version)') \
     <(node -e 'const l=require("./package-lock.json");for(const[k,v]of Object.entries(l.packages))if(k.includes("is-"))console.log(k,v.version)')

paso "6. Integridad: se altera el integrity de is-number en el lock y se instala con caché vacía"
cp /tmp/lock.original.json package-lock.json; cp /src/package.json package.json
node -e 'const f="package-lock.json",l=require("./"+f);l.packages["node_modules/is-number"].integrity="sha512-"+Buffer.alloc(64,7).toString("base64");require("fs").writeFileSync(f,JSON.stringify(l,null,2))'
npm ci --ignore-scripts --cache /tmp/npm-cache-vacia 2>&1 | grep -E '^npm error (code|sha512)' | head -n 2 | cut -c1-140; echo "exit=${PIPESTATUS[0]}"

paso "7. pnpm $PNPM_VERSION: con CI=true el lock está congelado por defecto"
cp /src/package.json package.json; rm -rf node_modules package-lock.json
npx -y "pnpm@$PNPM_VERSION" install --ignore-scripts 2>&1 | tail -n 1
sed -i 's/"is-odd": "\^3.0.0"/"is-odd": "^2.0.0"/' package.json
CI=true npx -y "pnpm@$PNPM_VERSION" install --ignore-scripts 2>&1 | grep -E 'ERR_PNPM' | head -n 1 | cut -c1-160; echo "exit=${PIPESTATUS[0]}"
