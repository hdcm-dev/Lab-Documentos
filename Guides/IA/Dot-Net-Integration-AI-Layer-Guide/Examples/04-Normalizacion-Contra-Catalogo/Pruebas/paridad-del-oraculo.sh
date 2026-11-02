#!/usr/bin/env bash
# paridad-del-oraculo.sh — corre los casos de paridad-del-oraculo.v1.json por los oráculos jq del
# ejemplo 04 y los compara con el veredicto del código C# (la prueba ParidadDelOraculo comprueba el
# otro lado). Corre en el host con bash y jq, como recalcular-oraculos.sh. Sale con 0 si coinciden todos.
#
#   Examples/04-Normalizacion-Contra-Catalogo/Pruebas/paridad-del-oraculo.sh
set -uo pipefail
AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ORACULOS="$AQUI/../../../tools/oraculos"
CASOS="$AQUI/paridad-del-oraculo.v1.json"
total=0; coinciden=0
while IFS= read -r caso; do
  total=$((total + 1))
  id="$(jq -r .id <<<"$caso")"; esperado="$(jq -r .veredicto <<<"$caso")"
  # Se corre normalizacion-cerrada, que decide como ValidarPaso. normalizacion-esperada usa el mismo
  # módulo (normalizacion-comun.jq) para parsear y plegar, y agrega sólo la comparación con su oro.
  entrada="$(jq -c '{entrada: {entrada_id: .id}, llamadas: [{llamada_n: 1, truncado_posible: false, salida: .salida}]}' <<<"$caso")"
  obtenido="$(jq -r -f "$ORACULOS/normalizacion-cerrada.jq" <<<"$entrada")" || obtenido="error del oráculo"
  if [ "$obtenido" = "$esperado" ]; then coinciden=$((coinciden + 1))
  else echo "$id: código «$esperado», oráculo «$obtenido»: $(jq -c .salida <<<"$caso")"; fi
done < <(jq -c '.casos[]' "$CASOS")
echo "paridad-del-oraculo: $coinciden de $total casos coinciden"
[ "$coinciden" = "$total" ]
