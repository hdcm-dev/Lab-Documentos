#!/usr/bin/env bash
# E5-1 — Qué fija un contenedor y qué no.
# Usa la imagen del SDK de .NET 8 fijada por digest (la misma en cualquier máquina que la baje)
# y le monta un fuente en cp1252. Demuestra que el contenedor fija SO + toolchain,
# pero NO los bytes del código montado ni la configuración de git del host.
set -u
IMG='mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743'
AQUI="$(cd "$(dirname "$0")" && pwd)"
W="$(mktemp -d)"; trap 'rm -rf "$W"' EXIT
cp "$AQUI/fuente/"* "$W/"
# Simula el archivo heredado: misma fuente, pero guardada en Windows-1252.
iconv -f UTF-8 -t CP1252 "$AQUI/fuente/Program.cs" > "$W/Program.cs"
correr() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -e DOTNET_NOLOGO=1 -e NUGET_PACKAGES=/tmp/nuget -v "$W":/w -w /w "$IMG" bash -c "$1"; }

echo "## 1. Capas que trae la imagen (fijadas por el digest)"
correr 'grep PRETTY_NAME /etc/os-release; ldd --version | head -1; echo "SDK $(dotnet --version)"; git --version'

echo; echo "## 2. Capa que NO fija: los bytes del código montado"
file "$W/Program.cs"
correr 'dotnet build -nologo -v q 2>&1 | grep -E "error|Build succeeded|Compilación" | sort -u | head -5'

echo; echo "## 3. Capa que NO fija: la configuración de git del host"
# Se simula la configuración global de una estación Windows típica sin tocar la del usuario real.
export GIT_CONFIG_GLOBAL="$W/host.gitconfig"
git config --global core.autocrlf true
echo "host       -> core.autocrlf = $(git config --global --get core.autocrlf)  (origen: $GIT_CONFIG_GLOBAL)"
correr 'echo "contenedor -> core.autocrlf = $(git config --global --get core.autocrlf || echo "(no definido)")"'
unset GIT_CONFIG_GLOBAL

echo; echo "## 4. Mismo contenedor, mismos bytes corregidos en el repo (UTF-8)"
iconv -f CP1252 -t UTF-8 "$W/Program.cs" > "$W/p.tmp" && mv "$W/p.tmp" "$W/Program.cs"
file "$W/Program.cs"
correr 'dotnet build -nologo -v q 2>&1 | grep -E "error|Build succeeded|Compilación" | sort -u | head -3; dotnet bin/Debug/net8.0/Senal.dll'
