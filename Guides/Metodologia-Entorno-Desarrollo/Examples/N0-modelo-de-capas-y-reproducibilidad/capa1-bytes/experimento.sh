#!/usr/bin/env bash
# E0-1 · Capa 1 (bytes del fuente). Corre DENTRO de mcr.microsoft.com/dotnet/sdk:8.0.
# Mismo texto, dos codificaciones: cp1252 (heredado) y UTF-8 (canónico).
set -u
trabajo=$(mktemp -d)
for variante in cp1252 utf8; do
  mkdir -p "$trabajo/$variante"
  cp /src/Saludo.csproj "$trabajo/$variante/"
  if [ "$variante" = cp1252 ]; then
    # Conversión con iconv de la imagen (glibc): UTF-8 -> WINDOWS-1252
    iconv -f UTF-8 -t WINDOWS-1252 /src/Program.cs > "$trabajo/$variante/Program.cs"
  else
    cp /src/Program.cs "$trabajo/$variante/Program.cs"
  fi
  echo "=== variante: $variante"
  echo "--- bytes de 'ÇodigoAcción' en el archivo (od):"
  grep -a 'var ' "$trabajo/$variante/Program.cs" | head -c 24 | od -An -tx1
  echo "--- dotnet build:"
  ( cd "$trabajo/$variante" && dotnet build -nologo -v q 2>&1 | grep -E 'error|Warn|Advert|Build succeeded|Compilación' | sort -u | head -5 )
  if [ -f "$trabajo/$variante/bin/Debug/net8.0/Saludo.dll" ]; then
    echo "--- ejecución:"; dotnet "$trabajo/$variante/bin/Debug/net8.0/Saludo.dll"
  fi
done
rm -rf "$trabajo"
