# Oráculo «agente-con-aprobacion» (ejemplo 11). Recalcula, leyendo SÓLO el registro, lo que decidió
# Corrida.Veredicto en C#, sin compartir código con él. El motivo de la entrada trae el esperado y el
# pedido que aprobó la persona, tal como lo armó el código: «esperado=baja;mostrado=<id>|<nombre>|<provincia>»
# (o «mostrado=ninguno»). Una ejecución real de eliminar_localidad es la que tiene un resultado objeto
# (el Result del caso de uso); un pedido rechazado sin ejecutar trae texto o nada.
#   baja: exactamente una baja con éxito; el pedido mostrado es Resistencia (Chaco) y el id borrado
#         (resultado.id) es el id que mostró el pedido. Así, «lo borrado = lo mostrado» se recalcula.
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada.
(.entrada.motivo // "") as $m
| ($m | capture("esperado=(?<e>[a-z]+)").e // "") as $esperado
| ($m | capture("mostrado=(?<id>[0-9]+)\\|(?<nombre>[^|;]*)\\|(?<provincia>[^|;]*)") // null) as $mostrado
| if (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  elif $esperado != "baja" then "falla"
  else [ .llamadas[] | .herramientas[]?
         | select(.nombre == "eliminar_localidad" and (.resultado | type) == "object" and .resultado.exito == true) ] as $ok
  | if ($ok | length) == 1 and $mostrado != null
       and $mostrado.nombre == "Resistencia" and $mostrado.provincia == "Chaco"
       and (($ok[0].resultado.id // -1) | tostring) == $mostrado.id
    then "pasa" else "falla" end
  end
