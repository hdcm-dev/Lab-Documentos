# Oráculo «workflow-igual-funcion» (ejemplo 10). Sin modelo: no hay líneas «llamada»; todo lo que se
# compara viaja en el motivo del renglón «entrada», como JSON (ComparacionDeEntrada en C#). Recalcula,
# sin compartir código con Corrida.Invariantes, lo mismo que decide en C#:
#   pasa si la función da el esperado, el workflow y el rehidratado dan lo mismo que la función (estado y
#   datos), hubo un checkpoint por superstep (y al menos uno), hubo pausa con checkpoint y, tras
#   rehidratar, el pedido pendiente se volvió a emitir.
# Motivo «plazo» (o cualquier motivo que no sea JSON): excluida.
# Entrada: {entrada, llamadas}.
(.entrada.motivo // "") as $m
| if ($m | startswith("{")) | not then "excluida"
  else ($m | fromjson) as $c
  | if $c.Funcion == $c.Esperado
       and $c.Workflow == $c.Funcion and $c.DatosWorkflow == $c.DatosFuncion
       and $c.Rehidratado == $c.Funcion and $c.DatosRehidratado == $c.DatosFuncion
       and $c.Supersteps > 0 and $c.Checkpoints == $c.Supersteps
       and $c.Cortado == true and $c.PedidosTrasRehidratar >= 1
    then "pasa" else "falla" end
  end
