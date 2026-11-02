# Oráculo «acuerdo-del-juez» (ejemplo 15-Pruebas-Y-Evaluacion, plan §E.2; guía §10.6), recalculado
# SÓLO desde el registro: la salida de cada llamada al juez y su campo «paso»
# («evaluador=…;grupo=…;esperado=…;umbral=…[;tipo=…;anotador=…];meai=…[;diagnostico=…]»).
# Misma regla que Examples/15-Pruebas-Y-Evaluacion/Application/OraculoDelJuez.cs; si cambia una,
# cambia la otra.
#   · lectura: el entero entre <S2> y </S2>; sin él, la salida no es interpretable (falla, no excluida);
#   · «alta» = puntaje ≥ umbral;
#   · calibracion: pasa si cada evaluador coincide con la anotación (esperado alta | baja);
#   · salidas-07: pasa si «las dos altas» coincide con el oráculo determinista de 07 (esperado pasa | falla).
# Se excluye por truncado, plazo, error de llamada, caché del reporte o si no hubo las dos llamadas.
def paso: (.paso // "") | split(";") | map(capture("^(?<k>[^=]+)=(?<v>.*)$")) | map({(.k): .v}) | add // {};
def puntaje: (.salida // "") | [match("<S2>(?<v>[\\s\\S]*?)</S2>").captures[0].string] | first
  | if . == null then null else (gsub("^\\s+|\\s+$"; "")) as $t
    | if ($t | test("^[-+]?[0-9]+$")) then ($t | tonumber) else null end end;

if (.entrada.motivo // "") as $m | ($m == "plazo" or ($m | startswith("error de llamada")) or ($m | startswith("caché del reporte"))) then "excluida"
elif any(.llamadas[]; .truncado_posible == true) then "excluida"
else
  # Una lectura por evaluador: la última llamada de cada uno.
  [ .llamadas | sort_by(.llamada_n) | group_by(paso.evaluador)[] | last | {p: paso, s: puntaje} ] as $l
  | if ($l | length) != 2 then "excluida"
    elif any($l[]; .s == null) then "falla"
    else ($l[0].p.umbral | tonumber) as $u
      | if $l[0].p.grupo == "calibracion" then
          (if all($l[]; (.s >= $u) == (.p.esperado == "alta")) then "pasa" else "falla" end)
        else
          (if (all($l[]; .s >= $u)) == ($l[0].p.esperado == "pasa") then "pasa" else "falla" end)
        end
    end
end
