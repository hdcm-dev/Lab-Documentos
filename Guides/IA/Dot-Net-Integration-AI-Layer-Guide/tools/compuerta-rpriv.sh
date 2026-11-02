#!/usr/bin/env bash
# compuerta-rpriv.sh — compuerta R-PRIV con patrones (RPRI-17, RPRI-19): corre como paso de la fase build.
#
#   [COMPUERTAS_DIR=<carpeta fuera del repo>] tools/compuerta-rpriv.sh <ruta>...
#
# Clases que viven en el repo (genéricas, armadas en ejecución, sin ningún valor privado escrito):
#   ip-privada       direcciones de los rangos privados de RFC 1918 y del compartido de RFC 6598, también
#                    parciales («dos octetos y un punto»), en notación con x o con <…>; del rango de
#                    ocho bits sólo la dirección completa, porque la parcial choca con versiones (RPRI-19);
#   home             rutas bajo el directorio personal («/home/<usuario>», «-home-<usuario>-», «~/<carpeta>»);
#   usuario-mascara  un usuario pegado a «@» y a una máscara «<…>» o a una dirección de cuatro octetos. No marca
#                    «paquete@versión» ni recuentos como «hit@1» (RPRI-19).
# Clases privadas (sólo si COMPUERTAS_DIR las trae; el formato está en Compuertas/README.md):
#   rpriv.patrones   «clase<TAB>ERE», una por línea (nombres de contenedores, recursos del gestor, …);
#   r-priv.lista     términos fijos, por palabra entera y respetando mayúsculas (equipos, usuarios, …);
#   r-priv.historicos  valores exactos declarados históricos: se quitan de la línea antes de buscar.
# Sin COMPUERTAS_DIR corren sólo las clases genéricas y la salida lo dice (un lector no tiene las listas);
# con RPRIV_ESTRICTO=1, la falta de las listas cierra la compuerta (rc 2).
# Informa por clase y por archivo (ruta:línea), nunca el texto. Excluye bin/, obj/, .git/ y registros/.
# Salida: 0 = ningún hallazgo · 1 = hay hallazgos · 2 = compuerta cerrada (autoprueba o listas).
set -uo pipefail
GREP=/usr/bin/grep
cerrar() { echo "R-PRIV CERRADA: $*" >&2; exit 2; }
[ -x "$GREP" ] && "$GREP" --version | head -1 | "$GREP" -q 'GNU grep' || cerrar "no hay GNU grep en $GREP"
[ $# -gt 0 ] || cerrar "sin rutas para barrer"

# ---- clases genéricas, armadas en ejecución
alt() { seq -s'|' "$1" "$2"; }
O1=$((5 * 2)); O2=$((86 * 2)); O3=$((96 * 2)); O3B=$((84 * 2)); O4=$((50 * 2))
OCT='([0-9xX*]{1,3}|<)'
declare -A C=(
  [ip-privada]="(^|[^0-9A-Za-z.§])(${O1}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}([^0-9.]|$)|${O2}\\.($(alt $((4 * 4)) $((4 * 8 - 1))))\\.${OCT}|${O3}\\.${O3B}\\.${OCT}|${O4}\\.($(alt $((8 * 8)) $((8 * 16 - 1)))|[7-9][xX]|1[01][xX])\\.${OCT})"
  [home]='/ho''me/[A-Za-z]|-ho''me-[A-Za-z]|~/[A-Za-z]'
  [usuario-mascara]='[A-Za-z][A-Za-z0-9._-]*@(<[a-z][a-z-]*>|[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\b)'
)
ORDEN=(ip-privada home usuario-mascara)

# Autoprueba de las genéricas: positivos y negativos armados en ejecución.
autoprueba() {  # <clase> <texto> <esperado 0|1>
  local n; n=$(printf '%s\n' "$2" | "$GREP" -cE -e "${C[$1]}"); [ "$n" -eq "$3" ] || cerrar "autoprueba de $1: «$2» dio $n, se esperaba $3"
}
autoprueba ip-privada "la estación $O4.$((3 + 4))x.x.x" 1
autoprueba ip-privada "en $O3.$O3B.0.12" 1
autoprueba ip-privada "en $O2.$((4 * 5)).<red>" 1
autoprueba ip-privada "versión $O1.0.403 y §$O1.4" 0
autoprueba ip-privada "en $O2.$((4 * 8)).0.1" 0
autoprueba home "ruta /ho""me/alguien/x" 1
autoprueba home "carpeta -ho""me-alguien-y" 1
autoprueba usuario-mascara "alguien""@<equipo-principal>" 1
autoprueba usuario-mascara "alguien@$((96 * 2)).0.2.1" 1
autoprueba usuario-mascara "Microsoft.Extensions.AI""@10.4.0 y hit@1" 0

# ---- clases privadas (fuera del repo)
HIST=""; LISTA=""
if [ -n "${COMPUERTAS_DIR:-}" ] && [ -d "$COMPUERTAS_DIR" ]; then
  if [ -s "$COMPUERTAS_DIR/rpriv.patrones" ]; then
    while IFS=$'\t' read -r k v; do
      case "$k" in ''|\#*) continue ;; esac
      C[$k]="$v"; ORDEN+=("$k")
    done < "$COMPUERTAS_DIR/rpriv.patrones"
  fi
  [ -s "$COMPUERTAS_DIR/r-priv.lista" ] && { LISTA="$(mktemp)"; "$GREP" -vE '^\s*(#|$)' "$COMPUERTAS_DIR/r-priv.lista" > "$LISTA"; }
  [ -s "$COMPUERTAS_DIR/r-priv.historicos" ] && HIST="$COMPUERTAS_DIR/r-priv.historicos"
  # Autoprueba de la lista: cada positivo marca.
  if [ -n "$LISTA" ] && [ -s "$COMPUERTAS_DIR/r-priv.positivos" ]; then
    np=$("$GREP" -cvE '^\s*(#|$)' "$COMPUERTAS_DIR/r-priv.positivos"); mp=$("$GREP" -vE '^\s*(#|$)' "$COMPUERTAS_DIR/r-priv.positivos" | "$GREP" -cFi -f "$LISTA")
    [ "$np" -eq "$mp" ] || cerrar "la autoprueba de r-priv.lista marca $mp de $np positivos"
  fi
  privadas="patrones=$(( ${#ORDEN[@]} - 3 )) lista=$([ -n "$LISTA" ] && wc -l < "$LISTA" || echo 0) historicos=$([ -n "$HIST" ] && "$GREP" -cvE '^\s*(#|$)' "$HIST" || echo 0)"
else
  [ "${RPRIV_ESTRICTO:-0}" = 1 ] && cerrar "RPRIV_ESTRICTO=1 y COMPUERTAS_DIR sin definir"
  privadas="no configuradas (sin COMPUERTAS_DIR: sólo clases genéricas)"
fi
trap '[ -n "$LISTA" ] && rm -f "$LISTA"' EXIT

EXCL=(--exclude-dir=.git --exclude-dir=bin --exclude-dir=obj --exclude-dir=registros)
# sin_historicos <opción de grep> <patrón o lista>: lee «ruta:línea:texto», quita del texto los valores
# históricos y deja pasar «ruta:línea» sólo si el texto sigue marcando con el mismo grep.
sin_historicos() {
  local op="$1" pat="$2" r l x
  while IFS= read -r x; do
    r="${x%%:*}"; x="${x#*:}"; l="${x%%:*}"; x="${x#*:}"
    if [ -n "$HIST" ]; then
      x="$(awk -v H="$HIST" 'BEGIN { while ((getline h < H) > 0) if (h !~ /^[ \t]*(#|$)/) hist[++nh] = h }
        { for (i = 1; i <= nh; i++) while ((p = index($0, hist[i])) > 0) $0 = substr($0, 1, p - 1) substr($0, p + length(hist[i])); print }' <<<"$x")"
    fi
    if [ "$op" = "-E" ]; then printf '%s\n' "$x" | "$GREP" -qE -e "$pat" && printf '%s:%s\n' "$r" "$l"
    else printf '%s\n' "$x" | "$GREP" -qwF -f "$pat" && printf '%s:%s\n' "$r" "$l"; fi
  done
}

rc=0; total=0
for k in "${ORDEN[@]}"; do
  hall="$("$GREP" -rnIE "${EXCL[@]}" -e "${C[$k]}" "$@" 2>/dev/null | sin_historicos -E "${C[$k]}" | sort -u)"
  n=$(printf '%s' "$hall" | "$GREP" -c . ); total=$((total + n))
  echo "$k: $n"; [ "$n" -eq 0 ] || { printf '%s\n' "$hall" | sed 's/^/  /'; rc=1; }
done
if [ -n "$LISTA" ]; then
  hall="$("$GREP" -rnIwF "${EXCL[@]}" -f "$LISTA" "$@" 2>/dev/null | sin_historicos -F "$LISTA" | sort -u)"
  n=$(printf '%s' "$hall" | "$GREP" -c . ); total=$((total + n))
  echo "identificadores: $n"; [ "$n" -eq 0 ] || { printf '%s\n' "$hall" | sed 's/^/  /'; rc=1; }
fi
echo "R-PRIV: $total hallazgo(s); clases privadas: $privadas"
exit $rc
