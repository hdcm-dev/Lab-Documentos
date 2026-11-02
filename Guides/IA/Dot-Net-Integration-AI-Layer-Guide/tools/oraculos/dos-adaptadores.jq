# Oráculo «dos-adaptadores» (ejemplo 02, plan §E.2): por las dos vías hay respuesta no vacía, los
# adaptadores registrados son los dos y el proveedor que informó cada cliente (campo «paso», de
# GetService<ChatClientMetadata>()) es distinto.
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada; por /v1
# truncado_posible es null («no verificable») y 02 es de demostración: no hay tasa que proteger.
if (.llamadas | length) < 2 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
elif ((.llamadas | map(.adaptador) | unique) == ["ollamasharp", "openai-v1"])
     and all(.llamadas[]; (.salida // "") | test("\\S"))
     and ((.llamadas | map(.paso) | unique | length) == 2)
     and all(.llamadas[]; .paso != null)
then "pasa" else "falla" end
