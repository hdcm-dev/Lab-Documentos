#!/bin/sh
# Resuelve el SDK que elegiría `dotnet` en cada carpeta, con dos imágenes de SDK.
# Uso (desde esta carpeta): sh correr.sh
CASOS="exacto defecto-misma-banda defecto-otra-banda rango-feature otra-mayor piso-cualquier-mayor mensaje-propio"
for img in mcr.microsoft.com/dotnet/sdk:8.0 mcr.microsoft.com/dotnet/sdk:10.0; do
  echo "##### $img  (instalado: $(docker run --rm "$img" dotnet --list-sdks | cut -d' ' -f1))"
  for d in $CASOS; do
    out=$(docker run --rm -e DOTNET_NOLOGO=1 -v "$PWD/$d":/w -w /w "$img" dotnet --version 2>&1); rc=$?
    if [ $rc -eq 0 ]; then res="usa $out"; else res="FALLA rc=$rc: $(echo "$out" | grep -E 'Requested SDK|Este repo' | head -1 | sed 's/^ *//')"; fi
    printf '%-22s %s\n' "$d" "$res"
  done
done
