#!/bin/sh
# E4-4, ampliación (mesa, ciclo 1): ¿pnpm-lock.yaml y yarn.lock registran las variantes de otras plataformas?
# Paquete con binarios por plataforma (esbuild 0.25.10). Corre dentro de un contenedor node, sobre /tmp:
#   docker run --rm -v "$PWD/e4-npm-lock:/src:ro" node:24-bookworm-slim sh /src/multiplataforma-pnpm-yarn.sh
set -u
echo "node $(node -v), npm $(npm -v)"
npm i -g pnpm@10.18.0 >/dev/null 2>&1; echo "pnpm $(pnpm -v)"
mkdir -p /tmp/p && cd /tmp/p && printf '{"name":"p","version":"1.0.0","private":true,"dependencies":{"esbuild":"0.25.10"}}\n' > package.json
pnpm install --ignore-scripts >/dev/null 2>&1; echo "pnpm install rc=$?"
grep -A3 "^  '@esbuild/win32-x64@0.25.10':" pnpm-lock.yaml | head -4
echo "variantes @esbuild/* distintas en pnpm-lock.yaml: $(grep -oE "^  '@esbuild/[^@]+@0.25.10':" pnpm-lock.yaml | sort -u | wc -l) (cada una aparece en packages y en snapshots: $(grep -c "^  '@esbuild/.*@0.25.10':" pnpm-lock.yaml) líneas)"
echo "instaladas: $(ls node_modules/.pnpm | grep '^@esbuild' | tr '\n' ' ')"
cd / && mkdir -p /tmp/y && cd /tmp/y && printf '{"name":"y","version":"1.0.0","private":true,"packageManager":"yarn@4.10.3","dependencies":{"esbuild":"0.25.10"}}\n' > package.json
corepack enable >/dev/null 2>&1; echo "yarn $(COREPACK_ENABLE_DOWNLOAD_PROMPT=0 yarn --version)"
COREPACK_ENABLE_DOWNLOAD_PROMPT=0 YARN_ENABLE_SCRIPTS=0 YARN_NODE_LINKER=node-modules yarn install >/dev/null 2>&1; echo "yarn install rc=$?"
grep -A8 '"@esbuild/win32-x64@npm:0.25.10"' yarn.lock | grep -E 'esbuild/win32-x64|conditions'
echo "variantes @esbuild/* en yarn.lock: $(grep -c '^"@esbuild/.*@npm:0.25.10"' yarn.lock)"
echo "instaladas: $(ls node_modules/@esbuild | tr '\n' ' ')"
