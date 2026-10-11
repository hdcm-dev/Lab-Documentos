#!/bin/sh
# Corre todos los experimentos del núcleo N1 desde el HOST (Linux, macOS o WSL) con Docker
# y deja la salida en ./salidas/. Imágenes: mcr.microsoft.com/dotnet/sdk:8.0 y :10.0,
# node:22-bookworm-slim, node:20-alpine, node:24-bookworm-slim, python:3.12-slim, python:3-slim.
set -u
cd "$(dirname "$0")" || exit 1
D="$PWD"; S="$D/salidas"; mkdir -p "$S"
run () { docker run --rm -v "$D":/w -w /w "$@"; }
run python:3.12-slim python generar.py
run mcr.microsoft.com/dotnet/sdk:8.0                    sh e1-roslyn.sh   > "$S/e1-roslyn-sdk8.txt" 2>&1
run -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0     sh e1-roslyn.sh   > "$S/e1-roslyn-sdk10.txt" 2>&1
run -e TF=net8.0 -w /w/sonda-codepage mcr.microsoft.com/dotnet/sdk:8.0 sh -c 'dotnet run -v q; rm -rf bin obj' > "$S/e1-sonda-codepage-sdk8.txt" 2>&1
run -e TF=net10.0 -w /w/sonda-codepage mcr.microsoft.com/dotnet/sdk:10.0 sh -c 'dotnet run -v q; rm -rf bin obj' > "$S/e1-sonda-codepage-sdk10.txt" 2>&1
{ run mcr.microsoft.com/dotnet/sdk:8.0 sh e1-roslyn-codepage-servidor.sh
  run -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0 sh e1-roslyn-codepage-servidor.sh; } > "$S/e1-roslyn-codepage-servidor.txt" 2>&1
run mcr.microsoft.com/dotnet/sdk:8.0                    sh e1-razor.sh    > "$S/e1-razor-sdk8.txt" 2>&1
run -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0     sh e1-razor.sh    > "$S/e1-razor-sdk10.txt" 2>&1
{ run mcr.microsoft.com/dotnet/sdk:8.0 sh e1-plantillas-dotnet.sh
  run mcr.microsoft.com/dotnet/sdk:10.0 sh e1-plantillas-dotnet.sh; } > "$S/e1-plantillas-dotnet.txt" 2>&1
{ run node:22-bookworm-slim node e1-node.mjs; run node:20-alpine node e1-node.mjs; } > "$S/e1-node.txt" 2>&1
run node:24-bookworm-slim node e1-node.mjs > "$S/e1-node-24.txt" 2>&1   # Node 24: LTS activa; Node 20 ya sin soporte
{ run python:3.12-slim sh e1-python.sh; echo; run python:3-slim sh e1-python.sh; } > "$S/e1-python.txt" 2>&1
{ run python:3.12-slim sh e1-python-locale.sh; run python:3-slim sh e1-python-locale.sh; } > "$S/e1-python-locale.txt" 2>&1
run mcr.microsoft.com/dotnet/sdk:8.0 sh e1-git.sh        > "$S/e1-git.txt" 2>&1
run mcr.microsoft.com/dotnet/sdk:8.0 sh e1-diagnostico.sh > "$S/e1-diagnostico.txt" 2>&1
run python:3.12-slim sh e1-shebang.sh > "$S/e1-shebang.txt" 2>&1
run node:24-bookworm-slim node e1-shebang-node.mjs > "$S/e1-shebang-node.txt" 2>&1
run python:3.12-slim python -I e1-unicode.py > "$S/e1-unicode.txt" 2>&1
command -v file >/dev/null 2>&1 && sh e1-diagnostico.sh > "$S/e1-diagnostico-host-con-file.txt" 2>&1
command -v file >/dev/null 2>&1 && sh e1-file.sh > "$S/e1-file.txt" 2>&1
echo "Listo. Salidas en $S"; ls -1 "$S"
