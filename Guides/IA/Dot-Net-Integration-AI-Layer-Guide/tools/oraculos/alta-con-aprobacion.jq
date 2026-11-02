# Oráculo «alta-con-aprobacion» (ejemplo 09). Recalcula, leyendo SÓLO el registro, lo que decidió
# Corrida.Veredicto en C#, sin compartir código con él. El esperado de la entrada viaja en el motivo
# («esperado=rechazo» | «esperado=alta»). Una ejecución real de agregar_localidad es la que tiene un
# resultado objeto (el Result del caso de uso); un pedido rechazado sin ejecutar trae texto o nada.
#   rechazo: hubo al menos una ejecución, ninguna con éxito, y alguna con error de «codigoPostal».
#   alta: exactamente una ejecución con éxito, con Goya, Corrientes, 3450 y 90000 habitantes.
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada.
def recortar: gsub("^\\s+|\\s+$";"");
def ejecutadas: [ .llamadas[] | .herramientas[]?
  | select(.nombre == "agregar_localidad" and (.resultado | type) == "object") ];
def es_goya: (.argumentos.nombre // "" | tostring | recortar | ascii_downcase) == "goya"
  and (.argumentos.provincia // "" | tostring) == "Corrientes"
  and (.argumentos.codigoPostal // "" | tostring) == "3450"
  and (.argumentos.habitantes // "" | tostring) == "90000";

((.entrada.motivo // "") | capture("esperado=(?<e>[a-z]+)").e // "") as $esperado
| if (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  else ejecutadas as $x
  | if $esperado == "rechazo" then
      (if ($x | length) >= 1 and all($x[]; .resultado.exito == false)
          and any($x[]; (.resultado.errores // {}) | has("codigoPostal")) then "pasa" else "falla" end)
    elif $esperado == "alta" then
      ([ $x[] | select(.resultado.exito == true) ]) as $ok
      | (if ($ok | length) == 1 and ($ok[0] | es_goya) then "pasa" else "falla" end)
    else "falla" end
  end
