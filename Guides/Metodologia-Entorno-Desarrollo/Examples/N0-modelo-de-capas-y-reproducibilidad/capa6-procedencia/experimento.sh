#!/usr/bin/env bash
# E0-6 · Capa 6 (metadatos). Corre en el HOST. Arma un repo de juguete con un commit de fecha fija,
# registra el entorno en dos imágenes distintas y compara los manifiestos capa por capa.
set -u
aqui=$(cd "$(dirname "$0")" && pwd)
repo=$(mktemp -d)
cp "$aqui/../capa5-proceso/Calculo.csproj" "$aqui/../capa5-proceso/Calculo.cs" "$repo/"
printf '* text=auto eol=lf\n' > "$repo/.gitattributes"
printf '{ "sdk": { "version": "8.0.100", "rollForward": "latestFeature" } }\n' > "$repo/global.json"
( cd "$repo" && git init -q && git add . && GIT_AUTHOR_DATE='2026-01-01T00:00:00Z' GIT_COMMITTER_DATE='2026-01-01T00:00:00Z' \
  git -c user.name=lab -c user.email=lab@example.invalid commit -qm fuente )
for ref in mcr.microsoft.com/dotnet/sdk:8.0 mcr.microsoft.com/dotnet/sdk:10.0; do
  dig=$(docker image inspect "$ref" --format '{{index .RepoDigests 0}}' | sed 's/.*@//')
  docker run --rm -e IMAGEN_REF="$ref" -e IMAGEN_DIGEST="$dig" -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 \
    -v "$repo":/repo:ro -v "$aqui":/tool:ro "$ref" \
    bash -c 'git config --global --add safe.directory /repo; bash /tool/registrar-entorno.sh /repo' > "$repo/../manifiesto-$(basename "$ref" | tr : -).json"
  echo "=== manifiesto en $ref"; cat "$repo/../manifiesto-$(basename "$ref" | tr : -).json"
done
echo "=== diferencias entre manifiestos (diff):"
diff "$repo/../manifiesto-sdk-8.0.json" "$repo/../manifiesto-sdk-10.0.json"
rm -rf "$repo" "$repo/../manifiesto-sdk-8.0.json" "$repo/../manifiesto-sdk-10.0.json"
