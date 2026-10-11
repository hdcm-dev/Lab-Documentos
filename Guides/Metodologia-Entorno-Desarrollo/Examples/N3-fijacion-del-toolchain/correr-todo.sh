#!/bin/sh
# Corre los seis experimentos de N3 y versiona la salida completa de cada uno en salidas/E3-<m>.txt.
# Uso (desde esta carpeta, en el host con docker): sh correr-todo.sh
set -u
cd "$(dirname "$0")" || exit 1
mkdir -p salidas
m=1
for d in 01-dotnet-global-json 02-dotnet-sdk-compilador-runtime 03-node-engines-packagemanager \
         04-python-requires-vs-python-version 05-chequeo-de-toolchain 06-dotnet-sdk-y-lock; do
  { echo "# E3-$m — $d — $(date -u +%Y-%m-%d)"; ( cd "$d" && sh correr.sh )
    [ -f "$d/inspeccion-targets.sh" ] && { echo; echo "# E3-$m (ampliación) — inspeccion-targets.sh"; ( cd "$d" && sh inspeccion-targets.sh ); }
  } > "salidas/E3-$m.txt" 2>&1
  echo "E3-$m ($d): listo"; m=$((m+1))
done
