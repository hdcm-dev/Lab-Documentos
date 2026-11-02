# Oráculo «mcp-sesion-fija» (ejemplo 13, H-014). La misma cuenta que Application/OraculoSesionFija.cs,
# comparando en minúsculas y leyendo sólo el registro:
#   1. truncado_posible = true en alguna llamada → excluida;
#   2. toda herramienta invocada es listar_localidades (lista permitida del cliente); si no → falla;
#   3. al menos una listar_localidades respondió sin error → si no, falla («sin herramienta»);
#   4. ningún resultado trae «Goya» (sólo está en la sesión de la API) → si no, falla;
#   5. la respuesta final no nombra «Goya» → si no, falla;
#   6. la respuesta final nombra «Corrientes» y «Resistencia» (la sesión fija demo-mcp) → si no, falla.
["listar_localidades"] as $permitidas
| ["corrientes", "resistencia"] as $fija
| ["goya"] as $otra
| if (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  else
    [.llamadas[] | .herramientas[]?] as $h
    | ($h | map(select(.nombre == "listar_localidades" and .error == null and .resultado != null))) as $buenas
    | ($buenas | map(.resultado | tojson) | join("\n") | ascii_downcase) as $res
    | (.llamadas | sort_by(.llamada_n) | last | .salida // "" | ascii_downcase) as $sal
    | if any($h[]; .nombre as $n | ($permitidas | index($n)) == null) then "falla"
      elif ($buenas | length) == 0 then "falla"
      elif any($otra[]; . as $o | $res | contains($o)) then "falla"
      elif any($otra[]; . as $o | $sal | contains($o)) then "falla"
      elif any($fija[]; . as $f | ($sal | contains($f)) | not) then "falla"
      else "pasa" end
  end
