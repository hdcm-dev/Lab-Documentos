#!/bin/sh
# E5-8 — ¿La capa 4 (SO de la imagen) cambia los bytes del artefacto .NET
# cuando la capa 2 (SDK exacto) y el commit son los mismos? Tres bases Linux: la imagen oficial del SDK por etiqueta
# (Debian 12), Ubuntu 24.04 (la base del runner alojado ubuntu-24.04) y python:3.12-slim (Debian 13), estas dos
# con el SDK instalado por dotnet-install.sh como hace actions/setup-dotnet. Rutas distintas a propósito.
D=$(cd "$(dirname "$0")" && pwd)
docker run --rm -v "$D/proy":/proy:ro -v "$D/build-en.sh":/b.sh:ro mcr.microsoft.com/dotnet/sdk:8.0 sh /b.sh /w/a "imagen oficial sdk:8.0"
docker run --rm -v "$D/proy":/proy:ro -v "$D/build-en.sh":/b.sh:ro ubuntu:24.04 sh /b.sh /home/runner/work/pieza/pieza "ubuntu:24.04 + dotnet-install"
docker run --rm -v "$D/proy":/proy:ro -v "$D/build-en.sh":/b.sh:ro python:3.12-slim sh /b.sh /srv/otra/ruta "python:3.12-slim + dotnet-install"
# Control de sensibilidad: misma base que la primera línea en familia, OTRO SDK (capa 2). Debe cambiar el hash.
docker run --rm -v "$D/proy":/proy:ro -v "$D/build-en.sh":/b.sh:ro mcr.microsoft.com/dotnet/sdk:10.0 sh /b.sh /w/a "CONTROL imagen oficial sdk:10.0 (otro SDK)"
