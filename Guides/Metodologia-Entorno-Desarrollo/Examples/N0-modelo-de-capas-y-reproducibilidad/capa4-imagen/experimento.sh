#!/usr/bin/env bash
# E0-4 · Capa 4 (SO / imagen base). Corre en el HOST (necesita docker con buildx), no descarga imágenes.
# Compara el digest de la etiqueta que este host tiene en caché con el digest al que apunta HOY
# la misma etiqueta en el registro. Si difieren, «sdk:8.0» ya no nombra los mismos bytes.
set -u
for ref in mcr.microsoft.com/dotnet/sdk:8.0 python:3.12-slim node:22-bookworm-slim; do
  local_digest=$(docker image inspect "$ref" --format '{{index .RepoDigests 0}}' 2>/dev/null | sed 's/.*@//')
  remoto=$(docker buildx imagetools inspect "$ref" 2>/dev/null | awk '/^Digest:/{print $2; exit}')
  creada=$(docker image inspect "$ref" --format '{{.Created}}' 2>/dev/null | cut -c1-10)
  if [ -z "$remoto" ]; then veredicto="SIN DATO (el registro no respondió: credenciales o límite de consultas)"
  elif [ "$local_digest" = "$remoto" ]; then veredicto="IGUAL"
  else veredicto="DISTINTO (la etiqueta se movió)"; fi
  printf '%s\n  caché local (creada %s): %s\n  registro hoy:              %s\n  => %s\n' "$ref" "$creada" "${local_digest:-—}" "${remoto:-—}" "$veredicto"
done
echo "--- versión del SDK dentro de la imagen local, invocada por DIGEST (inmutable):"
d=$(docker image inspect mcr.microsoft.com/dotnet/sdk:8.0 --format '{{index .RepoDigests 0}}')
docker run --rm "$d" dotnet --version
echo "--- ¿qué cambió entre ambos digests? Variables de versión declaradas en la configuración de cada imagen:"
echo "  local:    $(docker image inspect mcr.microsoft.com/dotnet/sdk:8.0 --format '{{json .}}' 2>/dev/null \
  | python3 -c 'import json,sys; i=json.load(sys.stdin); print(i["Created"], " ".join(e for e in i["Config"]["Env"] if e.startswith(("DOTNET_VERSION","DOTNET_SDK_VERSION"))))')"
echo "  registro: $(docker buildx imagetools inspect mcr.microsoft.com/dotnet/sdk:8.0 --format '{{json .Image}}' 2>/dev/null \
  | python3 -c 'import json,sys; d=json.load(sys.stdin); i=d.get("linux/amd64",d); print(i.get("created"), " ".join(e for e in i["config"]["Env"] if e.startswith(("DOTNET_VERSION","DOTNET_SDK_VERSION"))))')"
