# Oráculo «normalizacion-cerrada» (ejemplo 04, demostración): la salida del modelo es una propuesta que
# TryGetResult acepta y, normalizada como lo hace el código, todas sus claves están en Catalogos y la
# frecuencia también; si no, «falla» (el código la rechazó con error). El parseo y el plegado viven en
# normalizacion-comun.jq, compartido con normalizacion-esperada. Una llamada con
# truncado_posible = true excluye la entrada.
include "normalizacion-comun" {search: "./"};
if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
elif (.llamadas | sort_by(.llamada_n) | last | .salida // "" | normalizar | valida) then "pasa"
else "falla" end
