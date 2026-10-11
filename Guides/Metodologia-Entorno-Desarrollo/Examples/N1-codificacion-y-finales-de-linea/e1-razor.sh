#!/bin/sh
# E1-N1-04 — Cómo lee el generador de Razor un .razor según su codificación.
# Se corre DENTRO del contenedor del SDK (ver README.md). Requiere generar.py.
# Mira el C# que genera Razor (EmitCompilerGeneratedFiles=true) y busca el texto.
set -u
TF="${TF:-net8.0}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
echo "SDK: $(dotnet --version)  |  TF=$TF"
cd "$(dirname "$0")/work/razor" || exit 1
for v in cp1252 utf8 utf8bom cp1252-ident; do
  echo "== Aviso.razor en $v"
  if dotnet build "$v/Lib.csproj" -nologo -v q -p:TF="$TF" >/tmp/b.log 2>&1; then
    g=$(find "$v/obj" -name 'Aviso_razor.g.cs' | head -1)
    echo "  build OK; C# generado: $(basename "$g")"
    grep -aoE 'Atenci.{1,3}n: se.{1,3}al|"Configur.{1,3}ci.{1,3}n"' "$g" | sort -u | sed 's/^/  /'
    # bytes del primer carácter no ASCII que quedó en el .g.cs
    grep -ao 'Atenci.\{1,3\}n' "$g" | head -1 | od -An -tx1 | sed 's/^/  bytes:/'
  else
    echo "  build FALLA:"
    grep -oE "(error|warning) (RZ|CS)[0-9]+[^[]*" /tmp/b.log | sort -u | head -8 | sed 's/^/  /'
  fi
done
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
