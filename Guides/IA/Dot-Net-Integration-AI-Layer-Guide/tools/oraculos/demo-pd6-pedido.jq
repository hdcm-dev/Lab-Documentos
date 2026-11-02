# Oráculo «demo-pd6-pedido» (demo, paso PD-6, humo). Es la regla de 11 «agente-con-aprobacion» (plan
# §E.5: el oráculo del paso es el del ejemplo hermano) en su rama SIN aprobación, recalculada leyendo
# SÓLO el registro. 11 aprueba y comprueba «lo borrado = lo mostrado»; PD-6 rechaza, así que compara
# lo mostrado con lo pedido. Pasa si:
#   · alguna llamada pidió eliminar_localidad con un «nombre» que contiene «resistencia» (sin
#     distinguir mayúsculas), y
#   · el pedido de aprobación que armó el código, guardado en el «motivo» de la entrada como
#     «Eliminar la localidad «<nombre>» (<provincia>), id <n>, de tu sesión.», es Resistencia (Chaco).
# Que la base no cambie sin aprobación no es conducta del modelo: es un invariante de código que
# Humo.Pd6 afirma siempre (AC-4) y no se recalcula de acá, porque eliminar_localidad devuelve texto
# tanto si borra como si no. La misma regla en C#: .../ConModelo/OraculosDeHumo.cs (Pd6).
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada.
((.entrada.motivo // "") | capture("^Eliminar la localidad «(?<nombre>[^»]*)» \\((?<provincia>[^)]*)\\), id (?<id>[0-9]+)") // null) as $mostrado
| if (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  elif any(.llamadas[] | .herramientas[]?;
        .nombre == "eliminar_localidad" and ((.argumentos.nombre // "") | tostring | ascii_downcase | contains("resistencia")))
       and $mostrado != null and $mostrado.nombre == "Resistencia" and $mostrado.provincia == "Chaco"
  then "pasa" else "falla" end
