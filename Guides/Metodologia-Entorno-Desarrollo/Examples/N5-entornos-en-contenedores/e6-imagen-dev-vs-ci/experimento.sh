#!/usr/bin/env bash
# E5-6 — ¿Imagen de desarrollo = imagen de CI? Mismo código, dos imágenes "de .NET 10".
#  CI : imagen oficial del SDK, por digest.
#  DEV: imagen de devcontainer de .NET 10 (etiqueta 1-10.0), por digest, bajada en otro momento.
set -u
CI='mcr.microsoft.com/dotnet/sdk:10.0@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c'
DEV='mcr.microsoft.com/devcontainers/dotnet:1-10.0@sha256:f9abf00d067ae68b03c767d38d7020cb82d22ed705545807be7e89c755ecaf10'
AQUI="$(cd "$(dirname "$0")" && pwd)"
RAIZ="$(mktemp -d)"; trap 'rm -rf "$RAIZ"' EXIT
build() { # $1 etiqueta  $2 imagen  $3 con-global-json(si/no)
  local W="$RAIZ/$1"; mkdir -p "$W"; cp "$AQUI/fuente/"* "$W/"
  [ "$3" = si ] && cp "$AQUI/global.json.ci" "$W/global.json"
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 \
    -e NUGET_PACKAGES=/tmp/nuget -v "$W":/w -w /w "$2" bash -c '
      v=$(dotnet --version 2>&1 | head -1)
      if dotnet build -c Release -nologo -v q >/tmp/log 2>&1; then
        echo "SDK $v | sha256 $(sha256sum bin/Release/net10.0/Hola.dll | cut -c1-16)… | $(dotnet bin/Release/net10.0/Hola.dll)"
      else echo "SDK $v | FALLA: $(grep -m1 -iE "error|requested|solicit" /tmp/log)"; fi' 2>&1 | sed "s/^/$1 → /"
}
echo "## 1. Sin global.json: cada imagen usa el SDK que trae"
build ci-1  "$CI"  no
build ci-2  "$CI"  no
build dev-1 "$DEV" no
echo; echo "## 2. Con global.json versionado en el repo (10.0.400, latestPatch, sin prerelease)"
build ci-3  "$CI"  si
build dev-2 "$DEV" si
