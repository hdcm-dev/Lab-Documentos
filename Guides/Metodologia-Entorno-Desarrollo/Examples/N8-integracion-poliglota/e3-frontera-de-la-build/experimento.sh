#!/usr/bin/env bash
# E8-3 — ¿Dónde vive la frontera entre la build .NET y las demás (quién llama a quién)?
# Saludo.csproj tiene targets que llaman a npm y a uv (variante "MSBuild llama a
# todos"); build.sh es la variante "el orquestador llama a todos".
set -uo pipefail
AQUI=$(cd "$(dirname "$0")" && pwd)
IMG=${IMG:-lab-n8-poliglota:local}
SDK=mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743
NODE24=node:24-bookworm-slim@sha256:d6aa754f16b3197301076f047b5def2f02ea1dbbc2ca920407d46d7ec7f87b20
W=$(mktemp -d); trap 'rm -rf "$W"; docker rm -f n8-node24 >/dev/null 2>&1' EXIT
fresco() { rm -rf "$W/repo"; cp -a "$AQUI/../solucion" "$W/repo"; }
run() { local img=$1; shift; docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 -v "$W/repo:/w" -w /w "$@" "$img" "${CMD[@]}"; }

echo "### 1. dotnet build en la imagen oficial del SDK (sin Node ni Python)"
fresco; CMD=(dotnet build src/Saludo -v m --nologo); run "$SDK" 2>&1 | grep -E 'error|Build (succeeded|FAILED)' | sort -u | head -4

echo; echo "### 2. dotnet build en la imagen multi-runtime: MSBuild llama a npm ci y a uv"
fresco; CMD=(bash -c 'dotnet build src/Saludo -v n --nologo 2>&1 | grep -E "Target \"(ConstruirWeb|GenerarDatos)\"|Skipping target|npm ci|uv run|gen:|web:|Build succeeded" | sed "s/^ *//" | sort -u')
run "$IMG"
echo "--- segunda build sin cambios (Inputs/Outputs): los targets se saltean"
run "$IMG"
echo "--- se toca uv.lock: sólo se rehace el paso Python"
touch "$W/repo/tools/gen/uv.lock"; run "$IMG"

echo; echo "### 3. ¿Qué Node usa MSBuild? El que está primero en el PATH, no el de .nvmrc"
fresco
docker create --name n8-node24 "$NODE24" >/dev/null && docker cp -q n8-node24:/usr/local/bin/node "$W/node24" && docker rm n8-node24 >/dev/null
mkdir -p "$W/repo/.n24" && mv "$W/node24" "$W/repo/.n24/node"
CMD=(bash -c 'export PATH=/w/.n24:$PATH; echo ".nvmrc=$(cat .nvmrc)  node en PATH=$(node -v)"; dotnet build src/Saludo -v m --nologo 2>&1 | grep -E "EBADENGINE|EBADDEVENGINES|engine|error MSB|Build (succeeded|FAILED)|web:" | sed "s/^ *//" | sort -u | head -6')
cp "$W/repo/web/package.json" "$W/package.json.con-devEngines"
sed -i '/"devEngines"/d' "$W/repo/web/package.json"
echo "--- a) sin devEngines en web/package.json, con engine-strict=true en web/.npmrc"
run "$IMG"
echo "--- b) sin devEngines y sin engine-strict (engines sólo declara, no hace cumplir)"
sed -i 's/^engine-strict=true/engine-strict=false/' "$W/repo/web/.npmrc"
run "$IMG"
cat "$W/repo/web/dist/app.js" 2>/dev/null | head -1
echo "--- c) el repo tal como está: devEngines.runtime en web/package.json y sin engine-strict (falla por defecto)"
cp "$W/package.json.con-devEngines" "$W/repo/web/package.json"; rm -rf "$W/repo/web/dist" "$W/repo/web/node_modules"
run "$IMG"

echo; echo "### 4. Variante orquestador: build.sh llama a cada ecosistema una vez; MSBuild no"
fresco; CMD=(bash -c './build.sh 2>&1 | grep -E "^(== compuerta|gen:|web:|dotnet-sdk|node=|python=|sha256 Saludo)"')
run "$IMG"
echo "--- y si alguien construye la .NET sola con ConstruirPoliglota=false sin correr build.sh"
fresco; CMD=(dotnet build src/Saludo -v m --nologo -p:ConstruirPoliglota=false); run "$IMG" 2>&1 | grep -E 'error' | sed 's/^ *//' | sort -u | head -2
