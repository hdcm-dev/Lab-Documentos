#!/usr/bin/env bash
# E5-5 — devcontainer.json con imagen por digest + feature congelada por devcontainer-lock.json.
# Trabaja sobre una copia temporal para no dejar contenedores ni archivos en el repo.
# Requiere: docker, node/npx (baja @devcontainers/cli en la versión fijada abajo).
set -u
CLI="npx -y @devcontainers/cli@0.89.0"
AQUI="$(cd "$(dirname "$0")" && pwd)"
W="$(mktemp -d)"; cp -r "$AQUI/.devcontainer" "$W/"
limpiar() {
  docker rm -f $(docker ps -aq --filter "label=devcontainer.local_folder=$W") >/dev/null 2>&1
  # La CLI deja una imagen derivada (imagen base + features) llamada vsc-<carpeta>-<hash>-features.
  docker rmi $(docker images --format '{{.Repository}}' | grep "^vsc-$(basename "$W" | tr 'A-Z' 'a-z')-") >/dev/null 2>&1
  rm -rf "$W"; }
trap limpiar EXIT
up() { $CLI up --workspace-folder "$W" --mount-workspace-git-root false "$@" 2>&1 | grep -E '"outcome"|^\[.*\] (v?[0-9]+\.[0-9]+\.[0-9]+)$|rror|ockfile' | sed 's/,"ociAuthDiagnostics.*//' ; }

echo "## 1. up con --frozen-lockfile y el lock versionado"
up --frozen-lockfile
echo "-- versiones efectivas dentro del contenedor:"
$CLI exec --workspace-folder "$W" bash -c 'echo "SDK $(dotnet --version) | node $(node --version) | $(grep PRETTY_NAME /etc/os-release)"' 2>/dev/null
docker rm -f $(docker ps -aq --filter "label=devcontainer.local_folder=$W") >/dev/null

echo; echo "## 2. Alguien cambia la referencia de la feature sin regenerar el lock"
sed -i 's#features/node:1"#features/node:1.6"#' "$W/.devcontainer/devcontainer.json"
grep -o '"ghcr[^"]*"' "$W/.devcontainer/devcontainer.json"
up --frozen-lockfile

echo; echo "## 3. ¿Hay versiones más nuevas de las features congeladas? (no modifica nada)"
cp "$AQUI/.devcontainer/devcontainer.json" "$W/.devcontainer/devcontainer.json"
$CLI outdated --workspace-folder "$W" 2>/dev/null
