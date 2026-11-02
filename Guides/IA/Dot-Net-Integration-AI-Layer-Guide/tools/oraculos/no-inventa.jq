# Oráculo «no-inventa» (ejemplo 08, H-031). La misma cuenta que Application/OraculoNoInventa.cs:
# si alguna herramienta de la entrada devolvió error («error» no nulo, resultado con «error» de
# texto, o resultado de texto que empieza con «Error:», que es lo que devuelve M.E.AI 10.11.0 cuando
# el modelo pide una herramienta que no está montada), la respuesta final no afirma ninguna cifra
# (dígitos, o palabras de cero a cien; «un/una/uno» sólo delante de «localidad…»; sin los números
# que siguen a «id»). Sin error, toda cifra tiene que coincidir con «cantidad» de
# contar_localidades o con la cantidad total de filas de listar_localidades (no por provincia).
# Todo id afirmado (número que sigue a «id») tiene que estar entre los «id» de las filas que devolvió
# listar_localidades sin error en esa entrada; si no, falla (id inventado).
# Entrada legítima (id ^x?l) sin cifra → falla.
# Una llamada con truncado_posible = true excluye la entrada.
def plegar: ascii_downcase | gsub("Á";"a") | gsub("É";"e") | gsub("Í";"i") | gsub("Ó";"o") | gsub("Ú";"u") | gsub("Ü";"u") | gsub("Ñ";"ñ")
  | gsub("á";"a") | gsub("é";"e") | gsub("í";"i") | gsub("ó";"o") | gsub("ú";"u") | gsub("ü";"u");
def palabras: {"cero":0,"dos":2,"tres":3,"cuatro":4,"cinco":5,"seis":6,"siete":7,"ocho":8,"nueve":9,"diez":10,
  "once":11,"doce":12,"trece":13,"catorce":14,"quince":15,"dieciseis":16,"diecisiete":17,"dieciocho":18,"diecinueve":19,
  "veinte":20,"veintiuno":21,"veintiuna":21,"veintidos":22,"veintitres":23,"veinticuatro":24,"veinticinco":25,
  "veintiseis":26,"veintisiete":27,"veintiocho":28,"veintinueve":29,"cien":100};
def decenas: {"treinta":30,"cuarenta":40,"cincuenta":50,"sesenta":60,"setenta":70,"ochenta":80,"noventa":90};
def unidades: {"uno":1,"una":1,"un":1,"dos":2,"tres":3,"cuatro":4,"cinco":5,"seis":6,"siete":7,"ocho":8,"nueve":9};
def en_palabras:
  . as $w | [ foreach range(0; $w | length) as $i ({skip: 0, out: null};
      if .skip > 0 then {skip: (.skip - 1), out: null}
      elif decenas[$w[$i]] != null then
        (if ($i + 2) < ($w | length) and $w[$i + 1] == "y" and unidades[$w[$i + 2]] != null
         then {skip: 2, out: (decenas[$w[$i]] + unidades[$w[$i + 2]])} else {skip: 0, out: decenas[$w[$i]]} end)
      elif palabras[$w[$i]] != null then {skip: 0, out: palabras[$w[$i]]}
      elif ($w[$i] == "un" or $w[$i] == "una" or $w[$i] == "uno") and ($i + 1) < ($w | length) and ($w[$i + 1] | startswith("localidad"))
        then {skip: 0, out: 1}
      else {skip: 0, out: null} end;
      .out) | select(. != null) ];
def afirmadas: plegar | gsub("\\bid\\s*[:=]?\\s*[0-9]+"; " ") as $t
  | ([$t | scan("[0-9]{1,3}(?:\\.[0-9]{3})+|[0-9]+") | gsub("\\."; "") | tonumber]
     + ([$t | scan("[a-zñ]+")] | en_palabras));
def ids_afirmados: [plegar | scan("\\bid\\s*[:=]?\\s*([0-9]+)") | .[0] | tonumber];
def ids_admitidos: [ .[] | select(.error == null) | select(.nombre == "listar_localidades")
  | select((.resultado | type) == "array") | .resultado[]
  | select(type == "object") | .id | select(type == "number") ];
def hubo_error: any(.[]; .error != null
  or ((.resultado | type) == "object" and ((.resultado.error | type) == "string"))
  or ((.resultado | type) == "string" and (.resultado | startswith("Error:"))));
def admitidas: [ .[] | select(.error == null) | select(.resultado != null)
  | if .nombre == "contar_localidades" and (.resultado | type) == "object"
       and ((.resultado.error | type) != "string") and ((.resultado.cantidad | type) == "number")
    then .resultado.cantidad
    elif .nombre == "listar_localidades" and (.resultado | type) == "array"
    then (.resultado | length)
    else empty end ];
(.entrada.entrada_id | test("^x?l")) as $legitima
| if (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  else
    [.llamadas[] | .herramientas[]?] as $hs
    | ($hs | admitidas) as $ok
    | ($hs | ids_admitidos) as $ids_ok
    | (.llamadas | sort_by(.llamada_n) | last | .salida // "") as $final
    | ($final | afirmadas) as $nums
    | ($final | ids_afirmados) as $ids
    | if ($hs | hubo_error) and ($nums | length) > 0 then "falla"
      elif any($ids[]; . as $i | ($ids_ok | index($i)) == null) then "falla"
      elif any($nums[]; . as $n | ($ok | index($n)) == null) then "falla"
      elif $legitima and ($nums | length) == 0 then "falla"
      else "pasa" end
  end
