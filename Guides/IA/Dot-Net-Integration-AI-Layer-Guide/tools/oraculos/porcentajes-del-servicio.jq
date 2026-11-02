# Oráculo «porcentajes-del-servicio» (ejemplo 12-Uno-O-Varios-Agentes). Recalcula, leyendo SÓLO el
# registro, si cada medio pedido aparece con su porcentaje y coincide (±1 punto) con el que dio
# ResultadosDeEncuestaService. Los medios pedidos y los porcentajes esperados salen del «contexto» de la
# primera llamada (el JSON de resumir_encuesta que recibió el modelo); el resumen, de la «salida» de la
# última. Reglas, las mismas que Application/Resumenes/ComparadorDePorcentajes.cs (sin compartir código):
#   1. texto en minúsculas y vocales sin tilde;
#   2. un medio se nombra por su clave o su etiqueta al comienzo de una palabra; vale la primera mención
#      (menor posición; a igual posición, la más larga);
#   3. porcentaje = número (decimales con punto o coma) seguido de «%» o «por ciento»;
#   4. el porcentaje del medio se busca en la cláusula de la mención: primero la corta (cortes en «.»,
#      «;», salto de línea, «y», «e» y «,» que no está entre dígitos) y, si ahí no hay, la larga (sin la
#      coma); dentro de la cláusula, el más cercano a la mención y, a igual distancia, el que viene después;
#   5. coincide con diferencia ≤ 1; sin ningún porcentaje en el texto, falla («sin cifra»).
# Entrada: {entrada, llamadas}. Regla común: una llamada con truncado_posible = true excluye la entrada.
def norm: ascii_downcase | gsub("[áÁ]"; "a") | gsub("[éÉ]"; "e") | gsub("[íÍ]"; "i") | gsub("[óÓ]"; "o") | gsub("[úÚüÜ]"; "u");
def esc: gsub("(?<c>[.+*?()\\[\\]{}|^$\\\\])"; "\\\(.c)");
# Cláusula que contiene la posición $p, cortando con $re: {i, f}.
def clausula($re; $p): . as $t | [match($re; "g") | {i: .offset, f: (.offset + .length)}] as $cs
  | ([0] + ($cs | map(.f))) as $ini | (($cs | map(.i)) + [$t | length]) as $fin
  | [range(0; $ini | length) | {i: $ini[.], f: $fin[.]}] | map(select(.i <= $p and $p < .f)) | first;
def en($c): map(select(.i >= $c.i and .f <= $c.f));
def corto: "\\.(?![0-9])|[;\n]|\\by\\b|\\be\\b|,(?![0-9])";
def largo: "\\.(?![0-9])|[;\n]|\\by\\b|\\be\\b";
def distancia($m): if .i >= $m.f then .i - $m.f elif .f <= $m.i then $m.i - .f else 0 end;

if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
else
  (.llamadas | sort_by(.llamada_n)) as $ll
  | ($ll[0].contexto // "null" | try fromjson catch null) as $d
  | if $d == null then "excluida"
    else
      ($ll[-1].salida // "" | norm) as $t
      | [ $t | match("([0-9]+(?:[.,][0-9]+)?)[ \t\u00a0]*(?:%|por ?ciento)"; "g")
          | {i: .offset, f: (.offset + .length), v: (.captures[0].string | gsub(","; ".") | tonumber)} ] as $ps
      | if ($ps | length) == 0 then "falla"
        else
          [ $d.medios_pedidos[] as $c
            | ([ $d.medios[] | select(.clave == $c) ] | first) as $m
            | ([ $m.clave, $m.etiqueta ] | map(norm) | unique) as $terminos
            | ([ $terminos[] as $term | $t | match("\\b" + ($term | esc); "g") | {i: .offset, f: (.offset + .length)} ]
               | sort_by([.i, -(.f - .i)]) | first) as $mencion
            | if $mencion == null then false
              else
                ($ps | en($t | clausula(corto; $mencion.i))) as $c1
                | (if ($c1 | length) > 0 then $c1 else ($ps | en($t | clausula(largo; $mencion.i))) end) as $cand
                | if ($cand | length) == 0 then false
                  else
                    ([ $cand[] | {v, d: distancia($mencion), a: (if .i >= $mencion.f then 0 else 1 end)} ] | sort_by([.d, .a]) | first) as $p
                    | (($p.v - $m.porcentaje) | fabs) <= 1
                  end
              end ]
          | if all then "pasa" else "falla" end
        end
    end
end
