#!/usr/bin/env bash
# diff-dominio.sh — el Domain/ de Examples/_Comun es idéntico al de la demo (plan §E.1, regla H2-008):
# mismo namespace, sin normalización. Sale 0 si el diff es vacío, 1 si difieren, 2 si falta una carpeta.
set -uo pipefail
GUIA="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
A="$GUIA/Examples/_Comun/MovilidadUrbana.Dominio/Domain"
B="${DEMO_DOMAIN:-$(find "$GUIA/Demos/MovilidadUrbana.Asistente/src" -type d -name Domain -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null | head -1)}"
[ -d "$A" ] || { echo "falta $A" >&2; exit 2; }
[ -n "$B" ] && [ -d "$B" ] || { echo "diff-dominio: la demo todavía no tiene Domain/ (Demos/MovilidadUrbana.Asistente/src/…/Domain)" >&2; exit 2; }
diff -r "$A" "$B" && echo "diff-dominio: idénticos"
