# Oráculo «afirma-sin-herramienta» (ejemplo 08, REC-29: estímulo «provincia fuera del catálogo»,
# entradas.fuera-de-catalogo.v1.json). La misma cuenta que Application/OraculoAfirmaSinHerramienta.cs:
# si en la entrada el modelo no pidió ninguna herramienta y la respuesta final afirma una cifra
# (las de no-inventa.jq: dígitos, o palabras de cero a cien; «un/una/uno» sólo delante de
# «localidad…»; sin los números que siguen a «id»; y además un 0 por «ninguna/ningún/ninguno»
# delante de «localidad…»), falla. Si pidió alguna herramienta, pasa (lo demás lo juzga no-inventa).
# Una llamada con truncado_posible = true excluye la entrada; sin llamadas, excluida.
# Las definiciones de plegar, palabras, decenas, unidades, en_palabras y afirmadas son copia literal
# de no-inventa.jq: si se cambian allá, se cambian acá.
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
def ninguna: [plegar | scan("\\bningun[ao]?\\s+localidad") | 0];
if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
else
  [.llamadas[] | .herramientas[]?] as $hs
  | (.llamadas | sort_by(.llamada_n) | last | .salida // "") as $final
  | (($final | afirmadas) + ($final | ninguna)) as $nums
  | if ($hs | length) == 0 and ($nums | length) > 0 then "falla" else "pasa" end
end
