# Oráculo «par-prefijo» (ejemplo 05): mismo criterio que Application/OraculoDelPar.cs.
# Entrada: {entrada, llamadas}. Sólo duraciones del servidor (prompt_eval_duration_ms), nunca de reloj.
# pasa: con-prefijo < 0,5 × sin-prefijo y, si hay vuelta a frío, la vuelta NO baja de 0,5 × sin-prefijo.
(.llamadas) as $l
| ($l | map(select(.paso == "sin-prefijo")) | first) as $sin
| ($l | map(select(.paso == "con-prefijo")) | first) as $con
| ($l | map(select(.paso == "vuelta-a-frio")) | first) as $vuelta
| if ($l | length) == 0 then "excluida"
  elif any($l[]; .truncado_posible == true) then "excluida"
  elif $sin == null or $con == null then "excluida"
  elif ($sin.prompt_eval_duration_ms // 0) <= 0 or $con.prompt_eval_duration_ms == null then "excluida"
  elif $vuelta != null and $vuelta.prompt_eval_duration_ms == null then "excluida"
  elif $vuelta != null and $vuelta.prompt_eval_duration_ms < 0.5 * $sin.prompt_eval_duration_ms then "falla"
  elif $con.prompt_eval_duration_ms < 0.5 * $sin.prompt_eval_duration_ms then "pasa"
  else "falla" end
