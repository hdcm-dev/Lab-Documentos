# Oráculo «recuperacion-top3» (ejemplo 06): mismo criterio que CorridaDeBusqueda.Veredicto.
# Entrada: {entrada, llamadas}. La línea «entrada» lleva en «motivo» los relevantes del oro
# («relevantes=Id1,Id2»); la llamada de la consulta lleva el orden completo en «recuperados».
# pasa: algún relevante entre los 3 de mayor puntaje. Las no respondibles quedan fuera del hit rate.
(.entrada.motivo // "") as $m
| (.llamadas | map(select(.paso | startswith("consulta:")))) as $c
| if ($m | startswith("no-respondible")) or $m == "plazo" then "excluida"
  elif ($c | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  elif ($m | startswith("relevantes=")) | not then "excluida"
  else ($m | ltrimstr("relevantes=") | split(",")) as $rel
    | ($c | last | .recuperados | sort_by(-.puntaje) | .[0:3] | map(.id)) as $top
    | if any($top[]; . as $id | $rel | any(. == $id)) then "pasa" else "falla" end
  end
