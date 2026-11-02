# Oráculo «cache-y-telemetria» (ejemplo 17, plan §E.2). La misma cuenta que
# Application/OraculoCacheYTelemetria.cs, leyendo sólo el registro:
#   1. sin llamadas, o truncado_posible = true en alguna → excluida;
#   2. llegaron al modelo exactamente p1 y p3, en ese orden (p2 fue acierto de caché) → si no, falla;
#   3. las dos llamadas salieron con temperaturas distintas (la temperatura es parte de la clave) → si no, falla;
#   4. ninguna salida vacía → si no, falla;
#   5. toda llamada tuvo su span («span: sí» en paso) → si no, falla;
#   6. contenido en el span sólo con «ambiente: Development» → si no, falla.
if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
else
  (.llamadas | sort_by(.llamada_n)) as $ll
  | if ($ll | map(.paso | split(" ")[0])) != ["p1", "p3"] then "falla"
    elif ($ll | map(.temperature) | unique | length) != ($ll | length) then "falla"
    elif any($ll[]; (.salida // "") | test("\\S") | not) then "falla"
    elif any($ll[]; .paso | contains("span: sí") | not) then "falla"
    elif any($ll[]; ((.paso | ascii_downcase | contains("ambiente: development")) != (.paso | contains("contenido en el span: sí")))) then "falla"
    else "pasa" end
end
