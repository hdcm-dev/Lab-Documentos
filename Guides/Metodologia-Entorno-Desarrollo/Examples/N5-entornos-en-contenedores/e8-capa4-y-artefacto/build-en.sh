#!/bin/sh
# E5-8 (parte que corre dentro de cada base).
# Uso: build-en.sh <ruta-de-trabajo> <etiqueta>
# Si no hay dotnet, lo instala con dotnet-install.sh (lo mismo que hace actions/setup-dotnet) en la versión exacta 8.0.425.
set -eu
RUTA=$1; ETQ=$2
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 HOME=/tmp
if ! command -v dotnet >/dev/null 2>&1; then
  if command -v apt-get >/dev/null 2>&1; then
    apt-get update -qq >/dev/null && DEBIAN_FRONTEND=noninteractive apt-get install -y -qq curl ca-certificates git libicu-dev >/dev/null 2>&1
  fi
  curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --version 8.0.425 --install-dir /opt/dotnet >/dev/null
  export PATH=/opt/dotnet:$PATH DOTNET_ROOT=/opt/dotnet
fi
. /etc/os-release
echo "== $ETQ | SO: $PRETTY_NAME | glibc: $(ldd --version 2>&1 | head -1 | sed 's/.* //') | SDK: $(dotnet --version) | ruta: $RUTA"
rm -rf "$RUTA" && mkdir -p "$RUTA" && cp -r /proy/. "$RUTA"/ && cd "$RUTA"
export GIT_AUTHOR_DATE=2026-10-01T00:00:00Z GIT_COMMITTER_DATE=2026-10-01T00:00:00Z
git init -q -b main . && git -c user.email=a@b -c user.name=lab add -A && git -c user.email=a@b -c user.name=lab -c commit.gpgsign=false commit -qm proyecto --date=2026-10-01T00:00:00Z
git remote add origin https://example.invalid/lab/pieza.git
dotnet build -c Release -p:ContinuousIntegrationBuild=true -v q -o out >/dev/null
echo "   dll $(sha256sum out/Pieza.dll | cut -c1-16)   pdb $(sha256sum out/Pieza.pdb | cut -c1-16)"
