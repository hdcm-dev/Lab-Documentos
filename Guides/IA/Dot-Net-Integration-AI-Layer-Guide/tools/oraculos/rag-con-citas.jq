# Oráculo «rag-con-citas» (ejemplo 07-RAG-Con-Citas, plan §E.2; H-029), recalculado SÓLO desde el
# registro: la salida, los ids recuperados («recuperados») y el texto de cada fragmento («contexto»,
# un renglón «[Fuente: <id>] <texto>»). Misma regla que Examples/07-RAG-Con-Citas/Application/
# OraculoDeCitas.cs y NumerosEnTexto.cs; si cambia una, cambia la otra.
#   · respondible con valor (7): cita válida, cita un relevante del oro, los valores del literal
#     relevante citado están en la respuesta y no hay otro valor fuera de los literales citados;
#   · respondible sin valor (5): cita válida;   · no respondible (4): «No sé» sin citas.
# Cita válida = al menos una «[Fuente: id]» y todas entre los recuperados. La tabla de tipos y
# relevantes es copia de Examples/06-Embeddings-Y-Busqueda-Semantica/oro/conjunto-de-oro.v1.json
# (congelado por SHA-256 en el ejemplo.json de 07), más el caso guionado e5 del modo falso.
def tabla: {
  "r01": {t: "valor", rel: ["LocalidadPolicy.RequisitoDelNombre"]},
  "r02": {t: "sin",   rel: ["LocalidadPolicy.RequisitoDeLaProvincia"]},
  "r03": {t: "valor", rel: ["LocalidadPolicy.RequisitoDelCodigoPostal"]},
  "r04": {t: "valor", rel: ["LocalidadPolicy.RequisitoDeLosHabitantes"]},
  "r05": {t: "valor", rel: ["EncuestaPolicy.RequisitoDelNombre", "LocalidadPolicy.RequisitoDelNombre"]},
  "r06": {t: "valor", rel: ["EncuestaPolicy.RequisitoDeLaEdad"]},
  "r07": {t: "sin",   rel: ["EncuestaPolicy.RequisitoDeLaLocalidad"]},
  "r08": {t: "sin",   rel: ["EncuestaPolicy.RequisitoDeLosMedios"]},
  "r09": {t: "sin",   rel: ["EncuestaPolicy.RequisitoDeLaFrecuencia"]},
  "r10": {t: "valor", rel: ["EncuestaPolicy.RequisitoDeLaDistancia"]},
  "r11": {t: "valor", rel: ["EncuestaPolicy.RequisitoDeLosMinutos"]},
  "r12": {t: "sin",   rel: ["EncuestaPolicy.RequisitoDelMotivo"]},
  "n01": {t: "no", rel: []}, "n02": {t: "no", rel: []}, "n03": {t: "no", rel: []}, "n04": {t: "no", rel: []},
  "e5":  {t: "valor", rel: ["EncuestaPolicy.RequisitoDeLosMinutos"]}
};
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
def citas: [match("\\[\\s*[Ff][Uu][Ee][Nn][Tt][Ee]:\\s*(?<id>[^\\]]+)\\]"; "g") | .captures[0].string | sub("\\s+$"; "")] | unique;
def esnose: norm | test("^\\s*no (lo )?se\\s*([.!,;:]|$)");
def literales: [ (. // "") | split("\n")[] | capture("^\\[Fuente: (?<id>[^\\]]+)\\] (?<texto>.*)$") ] | map({(.id): .texto}) | add // {};

(.entrada.entrada_id | split("#")[0]) as $id
| if .entrada.motivo == "plazo" or (.entrada.motivo // "" | startswith("error de llamada")) then "excluida"
  elif (.llamadas | length) == 0 then "excluida"
  elif any(.llamadas[]; .truncado_posible == true) then "excluida"
  else (.llamadas | sort_by(.llamada_n) | last) as $u
    | ($u.salida // "") as $s
    | ([$u.recuperados[]?.id]) as $rec
    | ($s | citas) as $c
    | (($c | length) > 0 and all($c[]; . as $x | $rec | index([$x]) != null)) as $valida
    | (($c | length) == 0 and ($s | esnose)) as $nose
    | ($u.contexto | literales) as $lit
    | (tabla[$id] // {t: "?"}) as $t
    | if $t.t == "no" then (if $nose then "pasa" else "falla" end)
      elif $t.t == "sin" then (if $valida then "pasa" else "falla" end)
      elif $t.t == "valor" then
        ($s | valores) as $vr
        | ([$c[] | $lit[.] // "" | valores[]] | unique) as $vc
        | ($valida and all($vr[]; . as $v | $vc | index([$v]) != null)) as $sinotro
        | ($sinotro and any($c[]; . as $x | ($t.rel | index([$x]) != null)
              and (($lit[$x] // "") | valores | all(.[]; . as $v | $vr | index([$v]) != null))))
        | if . then "pasa" else "falla" end
      else "falla" end
  end
