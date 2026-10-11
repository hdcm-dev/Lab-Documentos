#!/bin/sh
# E1-N1-03 — -p:CodePage=1252 con el servidor de compilación (VBCSCompiler) en SDK 8 vs 10.
# Observado: en SDK 8 la build falla con CS2016 si el servidor arranca con esa compilación,
# y pasa si el servidor ya pasó por el camino de "fallback" (leyó antes un archivo no UTF-8). En SDK 10 pasa siempre.
#   docker run --rm -v "$PWD":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0  sh e1-roslyn-codepage-servidor.sh
#   docker run --rm -v "$PWD":/w -w /w -e TF=net10.0 mcr.microsoft.com/dotnet/sdk:10.0 sh e1-roslyn-codepage-servidor.sh
set -u
TF="${TF:-net8.0}"; export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
cd "$(dirname "$0")/work/dotnet" || exit 1
b () { rm -rf cp1252-lf/bin cp1252-lf/obj
  dotnet build cp1252-lf/App.csproj -nologo -v q -p:TF="$TF" -p:ConIdentificador=true "$@" 2>&1 \
  | grep -oE 'error CS[0-9]+|Build succeeded' | sort -u | tr '\n' ' '; echo; }
echo "SDK $(dotnet --version)"
printf '1) servidor frío, CodePage=1252:                 '; b -p:CodePage=1252
printf '2) otra vez (servidor ya levantado):             '; b -p:CodePage=1252
dotnet build-server shutdown >/dev/null 2>&1
printf '3a) servidor calentado con un .cs UTF-8:         '; rm -rf utf8-lf/bin utf8-lf/obj; dotnet build utf8-lf/App.csproj -nologo -v q -p:TF="$TF" >/dev/null 2>&1; b -p:CodePage=1252
dotnet build-server shutdown >/dev/null 2>&1
printf '3b) servidor calentado con un .cs cp1252 (fallback): '; b >/dev/null; b -p:CodePage=1252
printf '4) sin servidor (UseSharedCompilation=false):     '; b -p:CodePage=1252 -p:UseSharedCompilation=false
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
