# Oráculo «demo-pd2-cita» (demo, paso PD-2, humo). Es la regla de 07 «rag-con-citas» (H-029; plan §E.5:
# el oráculo del paso es el del ejemplo hermano), caso «respondible con valor», adaptada al formato de
# la demo: el modelo responde un objeto JSON {respuesta, fuente} con UNA fuente, no «[Fuente: id]» en
# el texto. Se recalcula leyendo SÓLO el registro; la misma regla en C# está en
# Demos/MovilidadUrbana.Asistente/tests/.../ConModelo/OraculosDeHumo.cs (Pd2). Pasa si:
#   · la cita es válida: «fuente» (sin corchetes ni espacios) es el id de uno de los recuperados;
#   · cita el relevante del oro (pd2-01 tiene el objetivo de r03: LocalidadPolicy.RequisitoDelCodigoPostal);
#   · afirmación central: los valores del literal citado (renglón «[id] texto» del campo «contexto»)
#     están en «respuesta» y en «respuesta» no hay otro valor («cita y contradice» falla).
# Los valores se leen como en rag-con-citas.jq (dígitos y palabras de número; «un»/«una» no cuentan).
# Entrada: {entrada, llamadas}. Una llamada con truncado_posible = true excluye la entrada.
def relevante: {"pd2-01": "LocalidadPolicy.RequisitoDelCodigoPostal"};
def recortar: gsub("^[\\[\\] ]+|[\\[\\] ]+$"; "");
def objeto: (. // "") | gsub("^\\s*```(json)?\\s*|\\s*```\\s*$"; "") | (try fromjson catch null);
def palabras: [["cero",0],["uno",1],["dos",2],["tres",3],["cuatro",4],["cinco",5],["seis",6],["siete",7],
  ["ocho",8],["nueve",9],["diez",10],["once",11],["doce",12],["trece",13],["catorce",14],["quince",15],
  ["dieciseis",16],["diecisiete",17],["dieciocho",18],["diecinueve",19],["veinte",20],["treinta",30],
  ["cuarenta",40],["cincuenta",50],["sesenta",60],["setenta",70],["ochenta",80],["noventa",90],["cien",100],
  ["doscientos",200],["trescientos",300],["cuatrocientos",400],["quinientos",500],["seiscientos",600],
  ["setecientos",700],["ochocientos",800],["novecientos",900],["mil",1000]];
def norm: ascii_downcase
  | gsub("[áÁ]"; "a") | gsub("[éÉ]"; "e") | gsub("[íÍ]"; "i") | gsub("[óÓ]"; "o") | gsub("[úÚüÜ]"; "u")
  | gsub("\\[fuente:[^\\]]*\\]"; " ");
def valores: norm | gsub("\\bciento diez\\b"; " 110 ")
  | reduce palabras[] as $p (.; gsub("\\b" + $p[0] + "\\b"; " \($p[1]) "))
  | [match("[0-9]+"; "g").string | tonumber] | unique;
def literales: [ (. // "") | split("\n")[] | capture("^\\[(?<id>[^\\]]+)\\] (?<texto>.*)$") ] | map({(.id): .texto}) | add // {};

if (.llamadas | length) == 0 then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
else (.llamadas | sort_by(.llamada_n) | last) as $u
  | ($u.salida | objeto) as $p
  | [ $u.recuperados[]?.id ] as $ids
  | ($u.contexto | literales) as $lit
  | (relevante[.entrada.entrada_id | split("#")[0]]) as $rel
  | if ($p | type) == "object"
       and (($p.respuesta // "") | tostring | test("\\S"))
       and (($p.fuente | type) == "string")
    then ($p.fuente | recortar) as $f
      | ($p.respuesta | tostring | valores) as $vr
      | (($lit[$f] // null) | if . == null then null else valores end) as $vl
      | if ($ids | index($f)) != null and $f == $rel and $vl != null
           and ($vr - $vl | length) == 0 and ($vl - $vr | length) == 0
        then "pasa" else "falla" end
    else "falla" end
end
