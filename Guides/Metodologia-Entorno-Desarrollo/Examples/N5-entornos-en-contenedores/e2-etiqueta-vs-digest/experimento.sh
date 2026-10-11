#!/usr/bin/env bash
# E5-2 — Una etiqueta es un puntero móvil; un digest es el contenido.
# Compara el digest que la etiqueta tenía cuando se bajó en esta máquina con el que tiene hoy
# en el registro, sin re-etiquetar nada local (pull por digest no mueve la etiqueta).
set -u
REPO=mcr.microsoft.com/dotnet/sdk
TAG=8.0
LOCAL=$(docker image inspect "$REPO:$TAG" --format '{{index .RepoDigests 0}}' | cut -d@ -f2)
REMOTO=$(docker buildx imagetools inspect "$REPO:$TAG" --format '{{.Manifest.Digest}}')
FIJA=$(docker buildx imagetools inspect "$REPO:8.0.425" --format '{{.Manifest.Digest}}')
echo "## 1. ¿A qué contenido apunta la etiqueta?"
echo "$REPO:$TAG en esta máquina (bajada antes) -> $LOCAL"
echo "$REPO:$TAG hoy en el registro            -> $REMOTO"
echo "$REPO:8.0.425 (etiqueta 'fija') hoy       -> $FIJA"
[ "$LOCAL" = "$REMOTO" ] && echo "=> la etiqueta no se movió" || echo "=> la etiqueta se MOVIÓ: mismo nombre, otro contenido"

echo; echo "## 2. ¿Qué cambió? Versión de .NET declarada en la configuración de cada imagen"
docker image inspect "$REPO@$LOCAL" --format 'local : creada {{.Created}} {{range .Config.Env}}{{if or (eq (slice . 0 7) "DOTNET_") (eq (slice . 0 7) "ASPNET_")}}{{.}} {{end}}{{end}}'
docker buildx imagetools inspect "$REPO@$REMOTO" --format '{{json (index .Image "linux/amd64")}}' \
 | python3 -c 'import json,sys; d=json.load(sys.stdin); print("remota: creada", d["created"], " ".join(e for e in d["config"]["Env"] if e.startswith(("DOTNET_V","DOTNET_SDK","ASPNET_V"))))'

echo; echo "## 3. Mismo SDK, ¿mismo sistema? Paquetes del SO que difieren (requiere bajar la remota por digest)"
docker pull -q "$REPO@$REMOTO" >/dev/null
lista() { docker run --rm "$REPO@$1" dpkg-query -W -f '${Package} ${Version}\n' | sort; }
diff <(lista "$LOCAL") <(lista "$REMOTO") | grep '^[<>]' | sed 's/^</local :/; s/^>/remota:/' || echo "(sin diferencias de paquetes)"
echo; echo "## 4. Uso en Dockerfile / devcontainer.json (la etiqueta queda como comentario legible; manda el digest)"
echo "FROM $REPO:$TAG@$LOCAL"
# Limpieza opcional: docker rmi "$REPO@$REMOTO"
