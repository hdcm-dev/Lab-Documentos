# Módulo común de los oráculos del ejemplo 04 (normalizacion-cerrada y normalizacion-esperada); no es
# un oráculo. Reproduce, leyendo sólo la «salida» del registro, lo que hace el código C#:
#   1. TryGetResult<PropuestaDeMovilidad> (Microsoft.Extensions.AI sobre System.Text.Json), y
#   2. NormalizadorDeCatalogo.Plegar + su tabla de sinónimos (escrito para el ejemplo).
# Si se cambia algo de esto o del C#, se cambian los dos y las entradas pasan a v2. Los casos en que
# tienen que coincidir están en Examples/04-Normalizacion-Contra-Catalogo/Pruebas/paridad-del-oraculo.v1.json:
# la prueba C# los corre por el código y Pruebas/paridad-del-oraculo.sh, por este módulo.
#
# Reglas de TryGetResult observadas (sonda del 2026-10-27, M.E.AI 10.11.0, SDK 10.0.403):
# - se lee el PRIMER valor JSON del texto y lo que sigue se ignora («{…} gracias» y «{…}\n```» valen);
# - antes sólo puede haber espacios JSON (espacio, \t, \n, \r): un cerco «```json», un BOM, un U+00A0
#   o un texto delante dan false; si el valor no es un objeto, también;
# - JSON estricto: sin comas finales, comentarios, comillas simples, NaN, 01, +1, .5 ni 1.;
#   profundidad máxima 64 (el objeto de arriba cuenta 1);
# - nombres de propiedad sin distinguir mayúsculas (sólo ASCII: «medioſ» no es «medios»);
#   si una propiedad se repite gana la última, pero cualquier aparición de tipo inválido da false;
# - «medios»: ausente o null → lista vacía; si no, lista de cadenas o null (otro tipo → false);
#   «frecuencia»: ausente, null o cadena (otro tipo → false); el resto de las propiedades se ignora;
# - un surrogate suelto (\ud800) en una cadena que se lee da false, y también en el NOMBRE de una
#   propiedad del objeto de arriba (se decodifica para compararlo); en el valor de una ignorada, no.

# ---------------------------------------------------------------- 1. TryGetResult
def _token_re: "\"(?:[^\"\\\\\\x00-\\x1f]|\\\\[\"\\\\/bfnrt]|\\\\u[0-9a-fA-F]{4})*\"|-?(?:0|[1-9][0-9]*)(?:\\.[0-9]+)?(?:[eE][+-]?[0-9]+)?|true|false|null|[{}\\[\\]:,]|[ \\t\\n\\r]+|[\\s\\S]";
def _es_cadena: length >= 2 and startswith("\"");
# Una cadena con un surrogate suelto se marca {mal: true}: no es una cadena válida para leer.
def _cadena:
  if (gsub("\\\\\\\\"; "") | gsub("\\\\u[dD][89abAB][0-9a-fA-F]{2}\\\\u[dD][c-fC-F][0-9a-fA-F]{2}"; "")
      | test("\\\\u[dD][89a-fA-F][0-9a-fA-F]{2}"))
  then {mal: true} else fromjson end;
# Parser estricto sobre los tokens (sin espacios). Devuelve {ok, v, i}; los objetos quedan como
# {objeto: [[clave, valor], ...]} para no perder las claves repetidas.
def _valor($t; $i; $d):
  def lista($i; $d; $acc):
    _valor($t; $i; $d) as $r
    | if ($r.ok | not) then $r
      else ($acc + [$r.v]) as $a
        | if $t[$r.i] == "," then lista($r.i + 1; $d; $a)
          elif $t[$r.i] == "]" then {ok: true, v: $a, i: ($r.i + 1)}
          else {ok: false} end end;
  def miembros($i; $d; $acc):
    ($t[$i] // "") as $k
    | if ($k | _es_cadena | not) or $t[$i + 1] != ":" then {ok: false}
      else _valor($t; $i + 2; $d) as $r
        | if ($r.ok | not) then $r
          else ($acc + [[($k | _cadena), $r.v]]) as $a
            | if $t[$r.i] == "," then miembros($r.i + 1; $d; $a)
              elif $t[$r.i] == "}" then {ok: true, v: {objeto: $a}, i: ($r.i + 1)}
              else {ok: false} end end end;
  ($t[$i] // "") as $k
  | if $k == "{" then
      if $d + 1 > 64 then {ok: false}
      elif $t[$i + 1] == "}" then {ok: true, v: {objeto: []}, i: ($i + 2)}
      else miembros($i + 1; $d + 1; []) end
    elif $k == "[" then
      if $d + 1 > 64 then {ok: false}
      elif $t[$i + 1] == "]" then {ok: true, v: [], i: ($i + 2)}
      else lista($i + 1; $d + 1; []) end
    elif ($k | _es_cadena) then {ok: true, v: ($k | _cadena), i: ($i + 1)}
    elif $k == "true" then {ok: true, v: true, i: ($i + 1)}
    elif $k == "false" then {ok: true, v: false, i: ($i + 1)}
    elif $k == "null" then {ok: true, v: null, i: ($i + 1)}
    elif ($k | test("\\A-?[0-9]")) then {ok: true, v: 0, i: ($i + 1)}
    else {ok: false} end;
def _de($e; $nombre): [$e[] | select((.[0] | type) == "string" and (.[0] | ascii_downcase) == $nombre) | .[1]];
# Texto de la salida → {medios: [cadena | null], frecuencia: cadena | null}, o null si TryGetResult da false.
def propuesta:
  [scan(_token_re) | select(test("\\A[ \\t\\n\\r]+\\z") | not)] as $t
  | if $t[0] != "{" then null
    else _valor($t; 0; 0) as $r
      | if ($r.ok | not) then null
        else _de($r.v.objeto; "medios") as $ms | _de($r.v.objeto; "frecuencia") as $fs
          | if any($r.v.objeto[]; (.[0] | type) != "string")
               or any($ms[]; . != null and (type != "array" or any(.[]; . != null and type != "string")))
               or any($fs[]; . != null and type != "string")
            then null
            else {medios: (($ms | last) // []), frecuencia: ($fs | last)} end end end;

# ---------------------------------------------------------------- 2. Plegado y tablas
# Igual que NormalizadorDeCatalogo.Plegar: minúsculas ASCII, la tabla de tildes, sin marcas
# combinantes U+0300–U+036F, sin bordes (espacios ASCII y . , ; : ! ? ¡ ¿ " ') y espacios simples.
def plegar: ascii_downcase
  | gsub("[áàâäÁÀÂÄ]"; "a") | gsub("[éèêëÉÈÊË]"; "e") | gsub("[íìîïÍÌÎÏ]"; "i")
  | gsub("[óòôöÓÒÔÖ]"; "o") | gsub("[úùûüÚÙÛÜ]"; "u") | gsub("[ñÑ]"; "n") | gsub("[çÇ]"; "c")
  | gsub("[̀-ͯ]"; "")
  | gsub("\\A[ \\t\\n\\r\\f\\x0b.,;:!?¡¿\"']+|[ \\t\\n\\r\\f\\x0b.,;:!?¡¿\"']+\\z"; "") | gsub(" +"; " ");
# Claves y etiquetas de Catalogos (plegadas) más los sinónimos de NormalizadorDeCatalogo.cs.
def medios: {"colectivo":"colectivo","auto":"auto","auto particular":"auto","bicicleta":"bicicleta","moto":"moto",
  "caminata":"caminata","a pie":"caminata","tren":"tren",
  "bondi":"colectivo","micro":"colectivo","omnibus":"colectivo","bus":"colectivo","colectivos":"colectivo",
  "coche":"auto","automovil":"auto","el auto":"auto","bici":"bicicleta","la bici":"bicicleta","motocicleta":"moto",
  "caminando":"caminata","camino":"caminata","caminar":"caminata","pie":"caminata","el tren":"tren","ferrocarril":"tren"};
def frecuencias: {"diaria":"diaria","todos los dias":"diaria","semanal":"semanal","algunos dias por semana":"semanal",
  "ocasional":"ocasional","ocasionalmente":"ocasional",
  "casi todos los dias":"diaria","a diario":"diaria","cada dia":"diaria","de lunes a domingo":"diaria",
  "algunos dias":"semanal","varias veces por semana":"semanal","un par de veces por semana":"semanal","una vez por semana":"semanal",
  "a veces":"ocasional","de vez en cuando":"ocasional","rara vez":"ocasional"};
def claves_medios: ["colectivo","auto","bicicleta","moto","caminata","tren"];
def claves_frecuencias: ["diaria","semanal","ocasional"];

# Salida → {medios: [claves o texto plegado], frecuencia}, como NormalizacionDeMovilidad.Decidir; null
# si no hay propuesta. Un medio null o que queda vacío al plegar no cuenta; la frecuencia null es "".
def normalizar: propuesta
  | if . == null then null
    else {medios: ([.medios[] | select(. != null) | plegar | select(. != "") | (medios[.] // .)] | unique),
          frecuencia: (if .frecuencia == null then "" else (.frecuencia | plegar | (frecuencias[.] // .)) end)} end;
# ValidarPaso(2) sin errores: al menos un medio, todos del catálogo, y la frecuencia del catálogo.
def valida: . != null and (.medios | length) > 0 and all(.medios[]; . as $m | claves_medios | index($m) != null)
  and (.frecuencia as $f | claves_frecuencias | index($f) != null);
