#!/bin/sh
# E1-N1-01 — Cómo lee Roslyn (vía MSBuild) un .cs según su codificación, BOM, EOL y CodePage.
# Se corre DENTRO del contenedor del SDK (ver README.md). Requiere haber corrido generar.py.
#   TF=net8.0 sh e1-roslyn.sh     (con mcr.microsoft.com/dotnet/sdk:8.0)
#   TF=net10.0 sh e1-roslyn.sh    (con mcr.microsoft.com/dotnet/sdk:10.0)
set -u
TF="${TF:-net8.0}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
echo "SDK: $(dotnet --version)  |  TF=$TF  |  LANG=${LANG:-<vacío>}"
cd "$(dirname "$0")/work/dotnet" || exit 1

compilar () {  # $1=carpeta  $2..=propiedades extra
  dir="$1"; shift
  out=$(dotnet build "$dir/App.csproj" -nologo -v q -p:TF="$TF" "$@" 2>&1)
  if [ $? -eq 0 ]; then
    echo "  build OK"
    dotnet "$dir/bin/Debug/$TF/App.dll" | sed 's/^/  /'
  else
    echo "$out" | grep -E "error CS" | sed -E 's#.*/(Alarma|Mensajes)\.cs#  \1.cs#' | sort -u | head -4
  fi
}

for v in cp1252-lf utf8-lf utf8bom-lf utf8-crlf; do
  echo "== $v | sólo literal"
  compilar "$v"
  echo "== $v | con identificador no ASCII"
  compilar "$v" -p:ConIdentificador=true
done

# CodePage: se compila sin servidor de compilación (UseSharedCompilation=false) para que el
# resultado no dependa del estado del servidor (ver e1-roslyn-codepage-servidor.sh).
echo "== cp1252-lf | con identificador y -p:CodePage=1252 (le dice a Roslyn la página de códigos)"
compilar cp1252-lf -p:ConIdentificador=true -p:CodePage=1252 -p:UseSharedCompilation=false
echo "== utf8-lf (SIN BOM) | -p:CodePage=1252: CodePage desactiva el intento UTF-8 => mojibake"
compilar utf8-lf -p:CodePage=1252 -p:UseSharedCompilation=false
echo "== utf8bom-lf (CON BOM) | -p:CodePage=1252: el BOM manda sobre CodePage"
compilar utf8bom-lf -p:CodePage=1252 -p:UseSharedCompilation=false

echo "== cp1252-lf | el mismo programa con una consola no UTF-8 (LC_ALL=es_AR.ISO-8859-1): U+FFFD se ve como '?'"
dotnet build cp1252-lf/App.csproj -nologo -v q -p:TF="$TF" >/dev/null 2>&1 && \
  LC_ALL=es_AR.ISO-8859-1 dotnet "cp1252-lf/bin/Debug/$TF/App.dll" | head -1 | sed 's/^/  /'

# Limpieza: no dejar artefactos de build
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
