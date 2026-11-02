# Oráculo «normalizacion-esperada» (ejemplo 04, extensión de medición): como normalizacion-cerrada,
# y además las claves son las esperadas para la entrada (oro congelado con entradas.extension.v1.json,
# escrito para el ejemplo). El parseo y el plegado viven en normalizacion-comun.jq. Una llamada con
# truncado_posible = true excluye la entrada.
include "normalizacion-comun" {search: "./"};
def oro: {"x01":{"medios":["colectivo"],"frecuencia":"diaria"},"x02":{"medios":["auto","caminata"],"frecuencia":"ocasional"},
  "x03":{"medios":["colectivo","tren"],"frecuencia":"diaria"},"x04":{"medios":["bicicleta"],"frecuencia":"semanal"},
  "x05":{"medios":["moto"],"frecuencia":"diaria"},"x06":{"medios":["caminata"],"frecuencia":"ocasional"},
  "x07":{"medios":["auto","colectivo"],"frecuencia":"semanal"},"x08":{"medios":["bicicleta","tren"],"frecuencia":"diaria"}};
.entrada.entrada_id as $id
| if (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  else (.llamadas | sort_by(.llamada_n) | last | .salida // "" | normalizar) as $n
    | if ($n | valida) and (oro[$id] != null) and ($n.medios == (oro[$id].medios | sort)) and ($n.frecuencia == oro[$id].frecuencia)
      then "pasa" else "falla" end
  end
