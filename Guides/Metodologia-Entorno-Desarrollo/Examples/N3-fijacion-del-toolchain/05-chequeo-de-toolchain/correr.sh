#!/bin/sh
# Corre el mismo chequeo en cuatro imágenes: dos cumplen y dos no.
# Uso (desde esta carpeta): sh correr.sh
for img in mcr.microsoft.com/dotnet/sdk:8.0 node:24-alpine node:22-alpine python:3.12-slim python:3.11-slim; do
  echo "=== $img"
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_NOLOGO=1 -v "$PWD/repo":/repo:ro "$img" sh /repo/verificar-toolchain.sh
  echo "rc=$?"
done
