# Oráculo «anotaciones-coinciden» (ejemplo 14). Sin modelo: no hay líneas «llamada»; lo comparado viaja en
# el motivo del renglón «entrada», como JSON (ComparacionDeEntrada en C#). Recalcula, sin compartir código
# con Corrida.Invariantes, lo mismo que decide en C#:
#   pasa si la firma de M.E.AI y la de SK son iguales (nombre, descripción y, por parámetro, nombre, tipo,
#   descripción y obligatoriedad), la firma es contar_localidades(provincia), los resultados normalizados
#   de las dos invocaciones son iguales y coinciden con lo esperado (cantidad, y si hubo error o no).
# Motivo «plazo» (o cualquier motivo que no sea JSON): excluida.
# Entrada: {entrada, llamadas}.
(.entrada.motivo // "") as $m
| if ($m | startswith("{")) | not then "excluida"
  else ($m | fromjson) as $c
  | ($c.resultado_meai.normalizado) as $r
  | if $c.firma_meai == $c.firma_sk
       and $c.firma_meai.nombre == "contar_localidades"
       and ($c.firma_meai.parametros | map(.nombre)) == ["provincia"]
       and $c.resultado_meai.normalizado == $c.resultado_sk.normalizado
       and ($r.cantidad // null) == $c.esperado.cantidad
       and (($r.error | type) == "string") == $c.esperado.error
    then "pasa" else "falla" end
  end
