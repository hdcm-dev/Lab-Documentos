# Oráculo «no-vacia» (humo del arnés): la última llamada de la entrada tiene salida no vacía.
# Entrada: {entrada: <línea tipo entrada>, llamadas: [<líneas tipo llamada de esa entrada>]}.
# Regla común a todos los oráculos: una llamada con truncado_posible = true excluye la entrada.
if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
elif ((.llamadas | sort_by(.llamada_n) | last | .salida // "") | test("\\S")) then "pasa"
else "falla" end
