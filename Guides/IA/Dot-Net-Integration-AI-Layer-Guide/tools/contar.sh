#!/usr/bin/env bash
# contar.sh — regla de conteo de extensión de la guía (plan de integración §C; H2-047, H3-009).
#
#   tools/contar.sh [<guía.md>]        (sin argumento: la guía de esta carpeta)
#
# Palabras de prosa por Parte = tokens con al menos una letra o un dígito
# («tr -s '[:space:]' '\n' | grep -c '[[:alnum:]]'»), después de quitar:
#   - los bloques de código (``` y ~~~);
#   - los comentarios HTML (también los de varias líneas, p. ej. «<!-- fuente: … -->»);
#   - las anclas («<a id="…"></a>»);
#   - las filas separadoras de tablas («|---|…»);
#   - las tablas del Anexo F (todas sus filas).
# Las celdas de las demás tablas cuentan; los separadores sin letras ni dígitos («|», «·», «—», «→», «/»)
# no cuentan, porque no tienen ningún carácter alfanumérico.
# Partes: «Encabezado» (antes de «# Prólogo»), «Prólogo», «# Parte I» … «# Parte VII», «Cierre» (capítulo 17,
# desde «# Cierre» hasta el primer «## Anexo») y «Anexos» (desde «## Anexo A» hasta el final), con el detalle por anexo.
# Topes (tope votado en E2 del prompt 02: 60.000, el mismo que «tope_palabras» del front matter):
# Prólogo ≤ 2.200 · I ≤ 1.100 · II ≤ 3.700 · III ≤ 6.300 · IV ≤ 6.000 · V ≤ 8.300 · VI ≤ 8.800 · VII ≤ 7.500 ·
# Cierre ≤ 3.100 · Anexos ≤ 13.000 · total ≤ 60.000 (el total no incluye el encabezado). Reparto ratificado por
# la mesa de E10 del prompt 02 (H-5): 100 palabras de la Parte VII pasan a los Anexos, sin tocar el total.
# Salida: una fila por Parte (parte · palabras · tope · % del tope), los anexos, el total y, aparte, «wc -w»
# del archivo entero como control. Una fila sobre su tope lleva «SOBRE». Sale con 0 si pudo contar y nada
# supera su tope, 1 si alguna Parte, los Anexos o el total lo superan (H-5), y 2 si no encuentra la guía.
set -euo pipefail
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOC="${1:-$AQUI/../Dot-Net-Integration-AI-Layer-Guide.md}"
[ -f "$DOC" ] || { echo "contar.sh: no existe $DOC" >&2; exit 2; }

# 1) Rotula cada línea con su Parte y su anexo, y descarta lo que la regla no cuenta.
awk '
  function emitir(t) { if (t != "") print parte "\t" anexo "\t" t }
  BEGIN { parte = "Encabezado"; anexo = "-"; cerco = ""; comentario = 0 }
  {
    linea = $0
    # Bloques de código: la línea que abre o cierra el cerco tampoco cuenta.
    if (cerco == "" && match(linea, /^[ \t]*(```+|~~~+)/)) { cerco = substr(linea, RSTART, RLENGTH); gsub(/[ \t]/, "", cerco); cerco = substr(cerco, 1, 3); next }
    if (cerco != "") { t = linea; gsub(/^[ \t]+/, "", t); if (substr(t, 1, 3) == cerco) cerco = ""; next }
    # Encabezados de Parte y de anexo (sólo fuera de código).
    if (linea ~ /^# Prólogo/) { parte = "Prólogo"; anexo = "-" }
    else if (linea ~ /^# Parte /) { split(linea, p, " "); parte = "Parte " p[3]; anexo = "-" }
    else if (linea ~ /^# Cierre/) { parte = "Cierre"; anexo = "-" }
    else if (linea ~ /^## Anexo [A-Z]/) { parte = "Anexos"; split(linea, p, " "); anexo = p[3] }
    # Comentarios HTML, de una o de varias líneas.
    salida = ""
    while (linea != "") {
      if (comentario) { i = index(linea, "-->"); if (i == 0) { linea = "" } else { linea = substr(linea, i + 3); comentario = 0 } }
      else { i = index(linea, "<!--"); if (i == 0) { salida = salida linea; linea = "" } else { salida = salida substr(linea, 1, i - 1); linea = substr(linea, i + 4); comentario = 1 } }
    }
    linea = salida
    # Anclas.
    gsub(/<a id="[^"]*"><\/a>/, "", linea); gsub(/<a id="[^"]*">/, "", linea); gsub(/<\/a>/, "", linea)
    # Filas separadoras de tablas.
    if (linea ~ /^[ \t]*\|?[ \t]*:?-{3,}:?[ \t]*(\|[ \t]*:?-{3,}:?[ \t]*)*\|?[ \t]*$/) next
    # Tablas del Anexo F.
    if (parte == "Anexos" && anexo == "F" && linea ~ /^[ \t]*\|/) next
    emitir(linea)
  }' "$DOC" > "$(dirname "$(mktemp -u)")/.contar.$$"
TMP="$(dirname "$(mktemp -u)")/.contar.$$"; trap 'rm -f "$TMP"' EXIT

contar() { cut -f3- | tr -s '[:space:]' '\n' | grep -c '[[:alnum:]]' || true; }
tope() { case "$1" in "Prólogo") echo 2200;; "Parte I") echo 1100;; "Parte II") echo 3700;; "Parte III") echo 6300;;
  "Parte IV") echo 6000;; "Parte V") echo 8300;; "Parte VI") echo 8800;; "Parte VII") echo 7500;; Cierre) echo 3100;;
  Anexos) echo 13000;; Total) echo 60000;; *) echo -;; esac; }
fila() { local t; t="$(tope "$1")"; if [ "$t" = - ]; then printf '%-11s %7d %7s\n' "$1" "$2" -; else printf '%-11s %7d %7d %6s%s\n' "$1" "$2" "$t" "$(awk -v a="$2" -v b="$t" 'BEGIN{printf "%.0f%%", 100*a/b}')" "$([ "$2" -gt "$t" ] && echo ' SOBRE')"; [ "$2" -le "$t" ] || sobre=1; fi; }

printf '%-11s %7s %7s %6s\n' parte palabras tope '%'
total=0; sobre=0
for p in Encabezado "Prólogo" "Parte I" "Parte II" "Parte III" "Parte IV" "Parte V" "Parte VI" "Parte VII" Cierre Anexos; do
  n="$(awk -F'\t' -v p="$p" '$1 == p' "$TMP" | contar)"
  fila "$p" "$n"
  [ "$p" = Encabezado ] || total=$((total + n))
  if [ "$p" = Anexos ]; then
    for a in $(awk -F'\t' '$1 == "Anexos" { print $2 }' "$TMP" | sort -u); do
      printf '  Anexo %-3s %7d\n' "$a" "$(awk -F'\t' -v a="$a" '$1 == "Anexos" && $2 == a' "$TMP" | contar)"
    done
  fi
done
fila Total "$total"
printf 'control: wc -w del archivo = %d\n' "$(wc -w < "$DOC")"
exit "$sobre"
