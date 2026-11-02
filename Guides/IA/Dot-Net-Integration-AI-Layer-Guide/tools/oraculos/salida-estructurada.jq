# Oráculo «salida-estructurada» (ejemplo 03). Recalcula, leyendo SÓLO el registro, la decisión que
# el código tenía que tomar sobre la salida del modelo y la compara con la que anotó (motivo
# «decision=…»). Reimplementa, sin compartir código con el C#: TryGetResult (JSON del tipo pedido),
# la garantía del texto (un número propuesto se acepta sólo si está en la entrada), la
# normalización de provincia contra Catalogos y LocalidadRules.
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada.
def provincias: ["Buenos Aires","Chaco","Córdoba","Corrientes","Entre Ríos","Mendoza","Santa Fe"];
def plegar: ascii_downcase
  | gsub("[áÁ]";"a") | gsub("[éÉ]";"e") | gsub("[íÍ]";"i") | gsub("[óÓ]";"o") | gsub("[úÚüÜ]";"u") | gsub("[ñÑ]";"n");
def recortar: gsub("^\\s+|\\s+$";"");
def numeros: [ scan("[0-9]{1,3}(?:\\.[0-9]{3})+|[0-9]+") | gsub("\\.";"") ];
# TryGetResult: objeto con nombre/provincia/codigoPostal string|null y habitantes entero|null (sin distinguir mayúsculas en las claves).
def propuesta:
  (try fromjson catch null)
  | if type != "object" then null
    else (with_entries(.key |= ascii_downcase)) as $o
    | {nombre: $o.nombre, provincia: $o.provincia, cp: $o.codigopostal, hab: $o.habitantes}
    | if ([.nombre, .provincia, .cp] | all(. == null or type == "string"))
         and (.hab == null or (.hab | type == "number" and . == floor))
      then . else null end
    end;
def decidir($texto):
  ($texto | numeros) as $n
  | if . == null then "sin-propuesta"
    else
      ((.cp // "") | recortar) as $cp0
      | (if $cp0 != "" and ($n | index([$cp0]) == null) then null else $cp0 end) as $cp
      | (if .hab == null then null
         else (.hab | tostring) as $h
         | ([$n[] | select(. == $h)] | length) - (if $cp == $h then 1 else 0 end)
         | if . >= 1 then $h | tonumber else null end end) as $hab
      | ((.provincia // "") | recortar) as $p0
      | ([provincias[] | select(plegar == ($p0 | plegar))][0] // $p0) as $prov
      | [ (if ($cp // "" | test("^[0-9]{4}$")) then empty else "codigoPostal" end),
          (if $hab != null and $hab >= 1 then empty else "habitantes" end),
          (if ((.nombre // "") | recortar | length) >= 3 then empty else "nombre" end),
          (if (provincias | index([$prov])) != null then empty else "provincia" end) ]
      | sort | if length == 0 then "lista" else "faltan:" + join(",") end
    end;

if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
else
  (.llamadas | sort_by(.llamada_n) | last) as $l
  | ($l.salida // "" | propuesta | decidir($l.entrada // "")) as $d
  | ((.entrada.motivo // "") | capture("decision=(?<d>[^;]+)").d // "") as $anotada
  | if $d == $anotada and ((.entrada.motivo // "") | test(";") | not) then "pasa" else "falla" end
end
