#!/usr/bin/env bash
# compuerta-ac7.sh — compuerta R-PRIV (plan §F.10, AC-7; H-053, H2-052, H3-001, H3-002).
#
#   COMPUERTAS_DIR=<carpeta fuera del repo> [R_PRIV_LISTA=<lista>] tools/compuerta-ac7.sh <ruta>...
#
# (1) direcciones IP de los rangos privados de RFC 1918 y del compartido de RFC 6598 (el patrón se arma
#     en tiempo de ejecución: el repo no escribe ningún rango);
# (2) los términos de $R_PRIV_LISTA (texto fijo, sin distinguir mayúsculas; por defecto
#     $COMPUERTAS_DIR/r-priv.lista). Un valor histórico declarado por valor exacto en
#     $COMPUERTAS_DIR/r-priv.historicos (una línea por valor) no cuenta: se quita de la línea antes de buscar;
# (3) en los registros .jsonl: todo «Nombre» de persona es «Persona NN» (01…20) o una localidad de
#     $COMPUERTAS_DIR/localidades-permitidas.txt. El formato de las listas está en Compuertas/README.md.
# Falla CERRADA: sin COMPUERTAS_DIR (o dentro de la guía), sin lista, sin líneas útiles o si la autoprueba
# de positivos (${R_PRIV_LISTA%.lista}.positivos, fuera del repo) no marca todas sus líneas, sale con 2.
# Las coincidencias se informan como ruta:línea, SIN el texto (la salida puede ir a la evidencia).
# Salida: 0 = cero coincidencias · 1 = hay coincidencias · 2 = error o compuerta cerrada.
set -uo pipefail
GREP=/usr/bin/grep
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; GUIA="$(dirname "$AQUI")"
cerrar() { echo "AC-7 CERRADA: $*" >&2; exit 2; }

[ -x "$GREP" ] && "$GREP" --version | head -1 | "$GREP" -q 'GNU grep' || cerrar "no hay GNU grep en $GREP"
command -v jq >/dev/null || cerrar "falta jq"
[ -n "${COMPUERTAS_DIR:-}" ] && [ -d "$COMPUERTAS_DIR" ] || cerrar "COMPUERTAS_DIR sin definir o inexistente (Compuertas/README.md)"
case "$(realpath -m "$COMPUERTAS_DIR")/" in "$GUIA"/*) cerrar "COMPUERTAS_DIR tiene que estar fuera de la guía";; esac
PERMITIDAS="$COMPUERTAS_DIR/localidades-permitidas.txt"
HISTORICOS="$COMPUERTAS_DIR/r-priv.historicos"
R_PRIV_LISTA="${R_PRIV_LISTA:-$COMPUERTAS_DIR/r-priv.lista}"
[ -f "$R_PRIV_LISTA" ] || cerrar "R_PRIV_LISTA no existe"
LISTA="$(mktemp)"; trap 'rm -f "$LISTA"' EXIT
"$GREP" -vE '^\s*(#|$)' "$R_PRIV_LISTA" > "$LISTA"
[ -s "$LISTA" ] || cerrar "R_PRIV_LISTA no tiene líneas útiles"
POS="${R_PRIV_LISTA%.lista}.positivos"
[ -f "$POS" ] || cerrar "falta el archivo de positivos junto a la lista"
npos=$("$GREP" -cvE '^\s*(#|$)' "$POS")
marcados=$("$GREP" -vE '^\s*(#|$)' "$POS" | "$GREP" -cFi -f "$LISTA"); r=$?
[ $r -le 1 ] || cerrar "error de grep en la autoprueba"
[ "$npos" -gt 0 ] && [ "$marcados" -eq "$npos" ] || cerrar "la autoprueba marca $marcados de $npos positivos"

# El patrón se arma en ejecución con aritmética: ningún octeto de los rangos queda escrito en el repo.
alt() { seq -s'|' "$1" "$2"; }
O1=$((5 * 2)); O2=$((86 * 2)); O3=$((96 * 2)); O3B=$((84 * 2)); O4=$((50 * 2))
N='[0-9]{1,3}'
IP="\\b(${O1}\\.${N}\\.${N}\\.${N}|${O2}\\.($(alt $((4 * 4)) $((4 * 8 - 1))))\\.${N}\\.${N}|${O3}\\.${O3B}\\.${N}\\.${N}|${O4}\\.($(alt $((8 * 8)) $((8 * 16 - 1))))\\.[0-9]+\\.[0-9]+)\\b"
# Autoprueba: una dirección de cada rango y una fuera de rango, también armadas en ejecución.
for muestra in "$O1.$((1 + 1)).3.4" "$O2.$((4 * 5)).0.1" "$O3.$O3B.0.9" "$O4.$((8 * 8)).0.1"; do
  printf '%s\n' "$muestra" | "$GREP" -qE "$IP" || cerrar "el patrón de IP no marca su autoprueba"
done
printf "%s\n" "$O2.$((4 * 8)).0.1" | "$GREP" -qE "$IP" && cerrar "el patrón de IP marca fuera de rango"

[ $# -gt 0 ] || cerrar "sin rutas para barrer"
EXCL=(--exclude-dir=.git --exclude-dir=bin --exclude-dir=obj)
hay=0
# (2) con los históricos por valor exacto: una línea marcada vuelve a mirarse sin esos valores.
terminos() {
  if [ -s "$HISTORICOS" ]; then
    "$GREP" -rnIFi "${EXCL[@]}" -f "$LISTA" "$@" | awk -v H="$HISTORICOS" -v L="$LISTA" '
      BEGIN { while ((getline h < H) > 0) if (h !~ /^[ \t]*(#|$)/) hist[++nh] = h
              while ((getline t < L) > 0) term[++nt] = tolower(t) }
      { ruta = $0; sub(/:.*/, "", ruta); resto = substr($0, length(ruta) + 2); lin = resto; sub(/:.*/, "", lin)
        texto = substr(resto, length(lin) + 2)
        for (i = 1; i <= nh; i++) while ((p = index(texto, hist[i])) > 0) texto = substr(texto, 1, p - 1) substr(texto, p + length(hist[i]))
        texto = tolower(texto); for (i = 1; i <= nt; i++) if (index(texto, term[i]) > 0) { print ruta ":" lin; break } }'
  else
    "$GREP" -rnIFi "${EXCL[@]}" -f "$LISTA" "$@"
  fi
}
salida="$({ "$GREP" -rnIE "${EXCL[@]}" "$IP" "$@"; terminos "$@"; } 2>/dev/null | cut -d: -f1,2 | sort -u)"
if [ -n "$salida" ]; then echo "AC-7 (1-2) coincidencias:"; printf '%s\n' "$salida"; hay=1; fi

# (3) Nombres en los registros.
mapfile -t registros < <(find "$@" -name '*.jsonl' -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null)
if [ ${#registros[@]} -gt 0 ]; then
  [ -f "$PERMITIDAS" ] || cerrar "falta \$COMPUERTAS_DIR/localidades-permitidas.txt"
  for reg in "${registros[@]}"; do
    malos="$(jq -r --rawfile perm "$PERMITIDAS" '
      ($perm | split("\n") | map(select(test("^\\s*(#|$)") | not))) as $ok
      | select(.tipo == "llamada")
      | [ .entrada, .salida, .contexto, (.herramientas[]? | (.argumentos, .resultado) | tojson) ]
      | map(select(. != null) | tostring) | .[]
      | ( scan("\\\\?\"[Nn]ombre\\\\?\"\\s*:\\s*\\\\?\"([^\"\\\\]*)") | .[0] ),
        ( scan("Persona ([0-9]+)") | .[0] | select(tonumber < 1 or tonumber > 20) | "Persona \(.)" )
      | select(test("^Persona (0[1-9]|1[0-9]|20)$") | not)
      | select(. as $n | $ok | index($n) | not)
    ' "$reg" 2>/dev/null)" || cerrar "jq no pudo leer $reg"
    if [ -n "$malos" ]; then echo "AC-7 (3) nombres fuera del conjunto sintético en $reg: $(printf '%s\n' "$malos" | sort -u | wc -l) valor(es)"; hay=1; fi
  done
fi
[ $hay = 0 ] && echo "AC-7: 0 coincidencias (autoprueba: $marcados de $npos positivos marcados; ${#registros[@]} registro(s) revisados)"
exit $hay
