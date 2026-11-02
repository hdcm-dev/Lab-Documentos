#!/usr/bin/env bash
# compuerta-cabeceras.sh — «cabeceras = 0» en los cuerpos capturados (REC-11, §11.6 y A.12).
#
#   tools/compuerta-cabeceras.sh [<archivo o carpeta>...]     (sin argumentos: $RLLM_DIR)
#
# Una credencial va en el HttpClient o en un DelegatingHandler, nunca en las cabeceras del pedido: hay
# adaptadores que serializan esas cabeceras dentro del cuerpo JSON, y el cuerpo es lo que la guía enseña a
# capturar. Esta compuerta busca, en los cuerpos capturados (cuerpo*.json, *.cuerpo.json, cuerpo*.jsonl y
# los .ndjson de captura), cualquier clave de cabeceras con contenido (un objeto o una lista no vacíos bajo
# una clave cuyo nombre termina en «headers», sin distinguir mayúsculas, escapada o no), una clave de
# autorización («Authorization», «Api-Key», «X-Api-Key», «api_key») o un valor «Bearer <token>».
# Un objeto vacío o null no cuenta. Informa ruta:línea, nunca el texto.
# Salida: 0 = ninguna · 1 = hay cabeceras serializadas · 2 = sin nada que revisar o error.
set -uo pipefail
GREP=/usr/bin/grep
[ -x "$GREP" ] || { echo "cabeceras: no hay GNU grep en $GREP" >&2; exit 2; }
[ $# -gt 0 ] || set -- "${RLLM_DIR:?sin argumentos y sin RLLM_DIR}"

Q='\\?"'                                  # comilla, escapada o no
CLAVE="${Q}[A-Za-z_-]*[Hh][Ee][Aa][Dd][Ee][Rr][Ss]${Q}[[:space:]]*:[[:space:]]*(\\{[[:space:]]*${Q}|\\[[[:space:]]*(${Q}|\\{))"
AUT="${Q}(Authorization|authorization|Api-Key|api-key|X-Api-Key|x-api-key|api_key|apiKey)${Q}[[:space:]]*:"
BEARER='[Bb]earer[[:space:]]+[A-Za-z0-9._~+/=-]{8,}'

mapfile -t cuerpos < <(find "$@" -type f \( -name 'cuerpo*.json' -o -name '*.cuerpo.json' -o -name 'cuerpo*.jsonl' \
  -o -name '*.ndjson' \) -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null | sort)
[ ${#cuerpos[@]} -gt 0 ] || { echo "cabeceras: ningún cuerpo capturado en ${*}" >&2; exit 2; }

# JSON indentado: la clave abre el objeto al final de la línea y el contenido sigue en la próxima.
export ABRE='"[A-Za-z_-]*[Hh][Ee][Aa][Dd][Ee][Rr][Ss]"[[:space:]]*:[[:space:]]*[{[][[:space:]]*$'
indentados() {
  local f
  for f in "${cuerpos[@]}"; do
    awk -v F="$f" '
      pend && NF { if ($0 !~ /^[ \t]*[}\]]/) print F ":" pend; pend = 0 }
      $0 ~ ENVIRON["ABRE"] { pend = NR }' "$f"
  done
}
hall="$({ "$GREP" -nHE -e "$CLAVE" -e "$AUT" -e "$BEARER" "${cuerpos[@]}" 2>/dev/null | cut -d: -f1,2; indentados; } | sort -u)"
n=$(printf '%s' "$hall" | "$GREP" -c .)
if [ "$n" -gt 0 ]; then
  echo "cabeceras: $n línea(s) con cabeceras o credenciales serializadas en ${#cuerpos[@]} cuerpo(s):"; printf '%s\n' "$hall" | sed 's/^/  /'
  exit 1
fi
echo "cabeceras: 0 en ${#cuerpos[@]} cuerpo(s) capturado(s)"
