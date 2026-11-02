#!/usr/bin/env bash
# compuerta-ac8.sh — barrido RD-PO-4 (plan §F.10, AC-8; H-052, H2-053, H3-022). Una sola regla:
#   (a) sin distinguir mayúsculas y sin borde de palabra, para que caigan los identificadores compuestos;
#   (b) sensible a mayúsculas, el identificador de P2 (la palabra «línea» de texto, con tilde o en minúscula, queda fuera);
#   (c) los nombres de otros proyectos, desde $RD_PO_LISTA (fuera del repo), con la misma falla cerrada que AC-7.
# Autopruebas: $COMPUERTAS_DIR/rd-po-4.negativos no puede marcar nada; rd-po-4.positivos tiene que marcar
# todas sus líneas. Excepciones: $COMPUERTAS_DIR/rd-po-4.excepciones («ruta:línea motivo», relativa a la guía).
# Las tres listas viven fuera del repo (formato en Compuertas/README.md); RD_PO_LISTA, por defecto ahí.
#   COMPUERTAS_DIR=<carpeta fuera del repo> [RD_PO_LISTA=<lista>] tools/compuerta-ac8.sh <ruta>...
# Salida: 0 = cero coincidencias fuera de excepciones · 1 = hay · 2 = error o compuerta cerrada.
set -uo pipefail
GREP=/usr/bin/grep
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; GUIA="$(dirname "$AQUI")"; CMP="${COMPUERTAS_DIR:-}"; RD_PO_LISTA="${RD_PO_LISTA:-$CMP/rd-po.lista}"
cerrar() { echo "AC-8 CERRADA: $*" >&2; exit 2; }
P1='reclamo|ticket|bache|(?<!co)incidente'
P2='Linea[A-Z_s]|[a-z]Linea\b|\bLinea\b'

[ -x "$GREP" ] && "$GREP" --version | head -1 | "$GREP" -q 'GNU grep' || cerrar "no hay GNU grep en $GREP"
[ -n "$CMP" ] && [ -d "$CMP" ] || cerrar "COMPUERTAS_DIR sin definir o inexistente (Compuertas/README.md)"
case "$(realpath -m "$CMP")/" in "$GUIA"/*) cerrar "COMPUERTAS_DIR tiene que estar fuera de la guía";; esac
[ -f "$RD_PO_LISTA" ] || cerrar "RD_PO_LISTA no existe"
LISTA="$(mktemp)"; trap 'rm -f "$LISTA"' EXIT
"$GREP" -vE '^\s*(#|$)' "$RD_PO_LISTA" > "$LISTA"
[ -s "$LISTA" ] || cerrar "RD_PO_LISTA no tiene líneas útiles"
POS="${RD_PO_LISTA%.lista}.positivos"
[ -f "$POS" ] || cerrar "falta el archivo de positivos junto a la lista"
npos=$("$GREP" -cvE '^\s*(#|$)' "$POS"); marc=$("$GREP" -vE '^\s*(#|$)' "$POS" | "$GREP" -cFi -f "$LISTA")
[ "$npos" -gt 0 ] && [ "$marc" -eq "$npos" ] || cerrar "la autoprueba de RD_PO_LISTA marca $marc de $npos"

for f in rd-po-4.negativos rd-po-4.positivos rd-po-4.excepciones; do [ -f "$CMP/$f" ] || cerrar "falta \$COMPUERTAS_DIR/$f"; done
neg=$( { "$GREP" -niP "$P1" "$CMP/rd-po-4.negativos"; "$GREP" -nE "$P2" "$CMP/rd-po-4.negativos"; } | cut -d: -f1 | sort -u | wc -l)
[ "$neg" -eq 0 ] || cerrar "el barrido marca $neg línea(s) de rd-po-4.negativos"
lpos=$("$GREP" -cvE '^\s*(#|$)' "$CMP/rd-po-4.positivos")
mpos=$( { "$GREP" -niP "$P1" "$CMP/rd-po-4.positivos"; "$GREP" -nE "$P2" "$CMP/rd-po-4.positivos"; } | cut -d: -f1 | sort -u | wc -l)
[ "$mpos" -eq "$lpos" ] && [ "$lpos" -eq 5 ] || cerrar "el barrido marca $mpos de $lpos líneas de rd-po-4.positivos (se esperan 5)"

[ $# -gt 0 ] || cerrar "sin rutas para barrer"
EXCL=(--exclude-dir=.git --exclude-dir=bin --exclude-dir=obj)
todas="$({ "$GREP" -rnIiP "${EXCL[@]}" "$P1" "$@"; "$GREP" -rnIE "${EXCL[@]}" "$P2" "$@"; "$GREP" -rnIFi "${EXCL[@]}" -f "$LISTA" "$@"; } 2>/dev/null \
  | cut -d: -f1,2 | while IFS=: read -r ruta linea; do printf '%s:%s\n' "$(realpath --relative-to="$GUIA" "$ruta")" "$linea"; done | sort -u)"
excep="$("$GREP" -vE '^\s*(#|$)' "$CMP/rd-po-4.excepciones" | awk '{print $1}' | sort -u)"
quedan="$(comm -23 <(printf '%s\n' "$todas" | sed '/^$/d') <(printf '%s\n' "$excep" | sed '/^$/d'))"
nex=$(comm -12 <(printf '%s\n' "$todas" | sed '/^$/d') <(printf '%s\n' "$excep" | sed '/^$/d') | wc -l)
if [ -n "$quedan" ]; then echo "AC-8 coincidencias:"; printf '%s\n' "$quedan"; exit 1; fi
echo "AC-8: 0 coincidencias fuera de excepciones ($nex exceptuadas; autopruebas: 0 negativos, $mpos/5 positivos, lista $marc/$npos)"
