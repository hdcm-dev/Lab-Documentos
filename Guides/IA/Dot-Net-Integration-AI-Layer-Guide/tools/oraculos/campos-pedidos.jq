# Oráculo «campos-pedidos» (ejemplo 01-Chat-Y-Streaming, plan §E.2): la respuesta nombra cada campo
# que pide la pregunta (los de LocalidadPolicy para e01, los de EncuestaPolicy para e02) y la llamada
# terminó por stop. Las formas son copia de Examples/01-Chat-Y-Streaming/entradas.v1.json (congelado
# por SHA-256 en su ejemplo.json); si cambian, cambia la versión de las entradas y este archivo.
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada.
def norm: ascii_downcase
  | gsub("[áàä]"; "a") | gsub("[éèë]"; "e") | gsub("[íìï]"; "i") | gsub("[óòö]"; "o") | gsub("[úùü]"; "u")
  | gsub("Á"; "a") | gsub("É"; "e") | gsub("Í"; "i") | gsub("Ó"; "o") | gsub("Ú"; "u") | gsub("Ü"; "u");
def contiene($f): if ($f | length) <= 2 then test("\\b" + $f + "\\b") else contains($f) end;
{
  "e01": [["nombre"], ["provincia"], ["codigo postal", "cp"], ["habitante"]],
  "e02": [["nombre"], ["edad"], ["localidad"], ["medio"], ["frecuencia"], ["distancia"], ["minuto"], ["motivo"]]
} as $campos
| .entrada.entrada_id as $id
| if .entrada.motivo == "plazo" or (.entrada.motivo // "" | startswith("error de llamada")) then "excluida"
  elif (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  else (.llamadas | sort_by(.llamada_n) | last) as $u
    | ($u.salida // "" | norm) as $s
    | if ($campos[$id] | all(. as $formas | any($formas[]; . as $f | $s | contiene($f))))
         and (($u.paso // "") | test("finish=stop$"))
      then "pasa" else "falla" end
  end
