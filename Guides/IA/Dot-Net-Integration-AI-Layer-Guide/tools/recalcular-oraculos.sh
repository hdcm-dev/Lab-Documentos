#!/usr/bin/env bash
# recalcular-oraculos.sh — recalcula cada oráculo leyendo SÓLO el registro R-LLM (plan §E.1,
# «Recalculable»; H2-022) y lo compara con el «resultado» de cada línea tipo "entrada".
#
#   tools/recalcular-oraculos.sh [--oraculos <carpeta>]... <registro.jsonl>...
#
# Cada oráculo es un programa jq en tools/oraculos/<nombre>.jq que recibe
# {entrada, llamadas} y devuelve "pasa" | "falla" | "excluida". Con --oraculos (repetible) o con
# ORACULOS_EXTRA (carpetas separadas por «:»), se buscan también en otras carpetas, después de tools/oraculos/
# (por ejemplo, los oráculos de las sondas, que viven con su evidencia: ESTA-11).
# Sale con 0 si todo coincide, 1 si alguna entrada no coincide, 2 si falta un oráculo o hay error.
set -uo pipefail
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIRS=("$AQUI/oraculos")
IFS=: read -r -a extra <<<"${ORACULOS_EXTRA:-}"; for d in "${extra[@]}"; do [ -n "$d" ] && DIRS+=("$d"); done
while [ "${1:-}" = "--oraculos" ]; do [ -d "${2:-}" ] || { echo "--oraculos: no existe «${2:-}»" >&2; exit 2; }; DIRS+=("$2"); shift 2; done
[ $# -gt 0 ] || { echo "uso: $0 [--oraculos <carpeta>]... <registro.jsonl>..." >&2; exit 2; }
buscar_oraculo() { local d; for d in "${DIRS[@]}"; do [ -f "$d/$1.jq" ] && { printf '%s\n' "$d/$1.jq"; return 0; }; done; return 1; }
rc=0; total=0; coinciden=0
for reg in "$@"; do
  [ -f "$reg" ] || { echo "no existe: $reg" >&2; exit 2; }
  while IFS= read -r entrada; do
    total=$((total + 1))
    oraculo="$(jq -r .oraculo <<<"$entrada")"; id="$(jq -r .entrada_id <<<"$entrada")"
    ej="$(jq -r .ejemplo <<<"$entrada")"; esperado="$(jq -r .resultado <<<"$entrada")"
    prog="$(buscar_oraculo "$oraculo")" || { echo "$reg: entrada $id: no hay oráculo «$oraculo» en ${DIRS[*]##*/}" >&2; rc=2; continue; }
    obtenido="$(jq -s -r --argjson e "$entrada" --arg id "$id" --arg ej "$ej" \
      '{entrada: $e, llamadas: [ .[] | select(.tipo=="llamada" and .entrada_id==$id and .ejemplo==$ej) ]}' "$reg" \
      | jq -r -f "$prog")" || { echo "$reg: entrada $id: el oráculo falló" >&2; rc=2; continue; }
    if [ "$obtenido" = "$esperado" ]; then coinciden=$((coinciden + 1))
    else echo "$(basename "$reg"): $id ($oraculo): registro «$esperado», recalculado «$obtenido»"; [ $rc = 0 ] && rc=1; fi
  done < <(jq -c 'select(.tipo=="entrada")' "$reg")
done
echo "recalcular-oraculos: $coinciden de $total entradas coinciden"
exit $rc
