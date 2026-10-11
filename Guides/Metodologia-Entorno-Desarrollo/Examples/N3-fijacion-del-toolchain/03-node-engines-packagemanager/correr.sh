#!/bin/sh
# engines / engine-strict / devEngines / packageManager+Corepack en tres líneas de Node.
# Uso (desde esta carpeta): sh correr.sh
# Cada caso se copia a /tmp dentro del contenedor: no quedan node_modules ni lock en el repo.
run() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e npm_config_update_notifier=false \
  -v "$PWD/$2":/src:ro "$1" sh -c "cp -r /src/. /tmp/w && cd /tmp/w && $3" 2>&1; }
for img in node:22-alpine node:24-alpine node:26-alpine; do
  echo "################ $img  (node $(docker run --rm $img node -v), npm $(docker run --rm $img npm -v))"
  for c in engines engines-strict dev-engines; do
    echo "--- $c: npm install"
    run $img $c 'npm install --no-audit --no-fund; echo "rc=$?"' | grep -vE '^$|^up to date' | sed 's/^/    /'
  done
  echo "--- package-manager: corepack (pide pnpm, se usa yarn)"
  run $img package-manager 'corepack --version >/dev/null 2>&1 || { echo "corepack: no viene con esta versión de Node"; exit 0; }; \
    export COREPACK_ENABLE_DOWNLOAD_PROMPT=0; mkdir -p /tmp/bin; corepack enable --install-directory /tmp/bin; \
    PATH=/tmp/bin:$PATH yarn --version; echo "rc=$?"; \
    echo "pnpm que entrega corepack: $(PATH=/tmp/bin:$PATH pnpm --version)"; \
    PATH=/tmp/bin:$PATH npm install --no-audit --no-fund >/dev/null 2>&1; echo "npm install (corepack no intercepta npm por defecto): rc=$?"' | sed 's/^/    /'
done
echo "--- .nvmrc pide $(cat .nvmrc); node y npm no lo leen: npm install en node:22 con ese .nvmrc presente"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e npm_config_update_notifier=false -v "$PWD":/src:ro node:22-alpine \
  sh -c 'mkdir /tmp/w && cp /src/.nvmrc /tmp/w/ && cd /tmp/w && echo "{\"name\":\"n3-nvmrc\",\"private\":true}" > package.json && node -v && npm install --no-audit --no-fund; echo "rc=$?"' 2>&1 | sed 's/^/    /'
echo "--- pnpm-pm-on-fail: pnpm 12.10.1 instalado con npm, SIN Corepack; el proyecto declara packageManager pnpm@10.18.0"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e npm_config_update_notifier=false -v "$PWD/pnpm-pm-on-fail":/src:ro node:24-alpine \
  sh -c 'npm i -g --prefix /tmp/g pnpm@12.10.1 >/dev/null 2>&1; export PATH=/tmp/g/bin:$PATH; cp -r /src/. /tmp/w && cd /tmp/w; \
    echo "pnpm global: $(cd / && pnpm --version)"; echo "pnpm --version en el proyecto (pmOnFail por defecto, download):"; pnpm --version; echo "rc=$?"; \
    echo "con --pm-on-fail=error:"; pnpm --pm-on-fail=error --version 2>&1 | grep -E "ERR_PNPM|configured to use"; echo "rc=$(pnpm --pm-on-fail=error --version >/dev/null 2>&1; echo $?)"' 2>&1 | sed 's/^/    /'
