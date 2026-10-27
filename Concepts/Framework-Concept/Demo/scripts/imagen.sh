#!/usr/bin/env bash
# Imprime la etiqueta de la imagen de compilación de la demo y, si no existe, la construye desde la receta
# versionada contenedor/Dockerfile (P11). La usan contenedor.sh y guion.sh.
#
#   scripts/imagen.sh            # → fc-demo-build:<huella>, construyéndola si hace falta
#
# Lo que imprime docker build va a la salida de error, para que la salida estándar sea sólo la etiqueta.
set -euo pipefail

DEMO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RECETA="$DEMO/contenedor/Dockerfile"
HUELLA="$(sha256sum "$RECETA" | cut -c1-12)"
ETIQUETA="fc-demo-build:$HUELLA"

if ! docker image inspect "$ETIQUETA" >/dev/null 2>&1; then
  echo "imagen: no existe $ETIQUETA; se construye desde contenedor/Dockerfile (la primera vez tarda varios minutos)" >&2
  docker build -t "$ETIQUETA" "$DEMO/contenedor" >&2
fi
echo "$ETIQUETA"
