#!/usr/bin/env bash
# Mini demo MovilidadUrbana: verificación completa contra el código fijado en source.lock (expediente 0002, E1-U3).
# La corre el workflow .github/workflows/mini-demo-movilidad.yml y, con los mismos pasos, cualquiera en local.
#
# Uso:  verificar.sh [PASO]
#   fuente      git archive del SHA de source.lock (y de la rama de comparación) a la carpeta de trabajo.
#   contrato    compila y arranca la ApiWeb de cada árbol y pide GET /openapi/v1.json.
#   todo        (por defecto) fuente, contrato y los controles; agregador estricto.
#
# Controles de «todo»:
#   C1 contrato   el contrato versionado (contracts/openapi.json) contra el extraído del SHA. Bloquea.
#   C2 plantado   el mismo diff con una operación plantada en una copia tiene que dar exit 1. Bloquea si no.
#   C3 trazas     traces-resolve con anclas ruta#Símbolo@SHA contra el árbol del SHA. Bloquea.
#   C4 negativa   la ficha RN-LOC-01 sin corregir (fixture) tiene que dar rojo contra el mismo árbol. Bloquea si no.
#   A1 deriva     contrato y anclas contra la rama de comparación de source.lock (main). Sólo avisa.
#
# Requiere bash, git, curl, Python 3 y el SDK de .NET 10. No toca el repositorio del código: lo baja por SHA.
# Variables: TRABAJO (carpeta de trabajo; por defecto $RUNNER_TEMP o /tmp), PUERTO (por defecto 5099).
set -uo pipefail

PASO="${1:-todo}"
DEMO="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Raíz de Lab-Documentos: la primera carpeta hacia arriba que tiene tools/docs-guard/dg.py (sin depender de git).
RAIZ="$DEMO"
while [[ ! -f "$RAIZ/tools/docs-guard/dg.py" && "$RAIZ" != "/" ]]; do RAIZ="$(dirname "$RAIZ")"; done
[[ -f "$RAIZ/tools/docs-guard/dg.py" ]] || { echo "no encuentro tools/docs-guard/dg.py sobre $DEMO" >&2; exit 2; }
DG="$RAIZ/tools/docs-guard/dg.py"
NEGATIVA="$RAIZ/tools/docs-guard/fixtures/anclas/real-rn-loc-01"
TRABAJO="${TRABAJO:-${RUNNER_TEMP:-${TMPDIR:-/tmp}}/mini-demo-movilidad}"
PUERTO="${PUERTO:-5099}"

lock() { sed -n "s/^$1:[[:space:]]*//p" "$DEMO/source.lock" | head -1; }
REPO="$(lock repo)"
SHA="$(lock sha)"
COMPARAR="$(lock comparar)"
API="$(lock proyecto-api)"
RUTAS="$(lock rutas)"
py() { python3 -I "$DG" "$@"; }
aviso() { if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then echo "::warning::$*"; else echo "AVISO: $*"; fi; }

# fuente REF DESTINO: baja sólo ese commit y extrae las rutas de source.lock con git archive.
fuente_de() {
  local ref="$1" dst="$2" git_dir="$TRABAJO/repo.git"
  [[ -d "$git_dir" ]] || git init -q --bare "$git_dir" || return 1
  git --git-dir="$git_dir" fetch -q --depth 1 --no-tags "$REPO" "$ref" || return 1
  local commit; commit="$(git --git-dir="$git_dir" rev-parse FETCH_HEAD)" || return 1
  rm -rf "$dst" && mkdir -p "$dst" || return 1
  # shellcheck disable=SC2086  # RUTAS es una lista separada por espacios
  git --git-dir="$git_dir" archive "$commit" $RUTAS | tar -x -C "$dst" || return 1
  echo "$commit" > "$dst/.commit"
  echo "  $ref -> $commit ($(find "$dst" -type f | wc -l) archivos)"
}

fuente() {
  echo "== fuente: $REPO @ $SHA y $COMPARAR"
  fuente_de "$SHA" "$TRABAJO/sha" || return 1
  [[ "$(cat "$TRABAJO/sha/.commit")" == "$SHA" ]] || { echo "el SHA bajado no es el de source.lock"; return 1; }
  fuente_de "$COMPARAR" "$TRABAJO/comparar" || aviso "no se pudo bajar $COMPARAR para la comparación de deriva"
  return 0
}

# extraer ÁRBOL: compila la ApiWeb y pide el contrato a la aplicación en ejecución (C-10, R-E4).
extraer() {
  local arbol="$1" t0=$SECONDS
  [[ -f "$arbol/$API" ]] || { echo "  falta $API en $arbol"; return 1; }
  local salida="$arbol/.compilado" datos="$arbol/.datos"
  mkdir -p "$datos"
  dotnet build "$arbol/$API" -c Release -o "$salida" -nologo -v quiet -clp:ErrorsOnly --disable-build-servers || return 1
  local dll; dll="$salida/$(basename "${API%.csproj}").dll"
  ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://127.0.0.1:$PUERTO" \
    ConnectionStrings__BaseDeDatos="Data Source=$datos/movilidad.db" \
    dotnet "$dll" --contentRoot "$salida" > "$arbol/.api.log" 2>&1 &
  local pid=$! ok=1
  for _ in $(seq 1 60); do
    if curl -fsS -o "$arbol/openapi.json" "http://127.0.0.1:$PUERTO/openapi/v1.json" 2>/dev/null; then ok=0; break; fi
    kill -0 "$pid" 2>/dev/null || break
    sleep 1
  done
  kill "$pid" 2>/dev/null; wait "$pid" 2>/dev/null
  if [[ $ok -ne 0 ]]; then echo "  la ApiWeb no respondió; registro:"; tail -20 "$arbol/.api.log"; return 1; fi
  # «servers» lleva el puerto de esta corrida: se quita para que el contrato no dependa de dónde se extrajo.
  python3 -I -c 'import json,sys
d=json.load(open(sys.argv[1]))
d.pop("servers",None)
json.dump(d,open(sys.argv[1],"w"),indent=2,ensure_ascii=False); open(sys.argv[1],"a").write("\n")
ops=sum(1 for p in d["paths"].values() for m in p if m in ("get","put","post","delete","patch"))
print("  contrato:", len(d["paths"]), "rutas,", ops, "operaciones, OpenAPI", d["openapi"])' "$arbol/openapi.json"
  echo "  extraído en $((SECONDS - t0)) s"
}

contrato() {
  echo "== contrato: extracción en ejecución (SHA)"
  extraer "$TRABAJO/sha" || return 1
  if [[ -d "$TRABAJO/comparar" ]]; then
    echo "== contrato: extracción en ejecución ($COMPARAR)"
    PUERTO=$((PUERTO + 1)) extraer "$TRABAJO/comparar" || aviso "no se pudo extraer el contrato de $COMPARAR"
  fi
  return 0
}

# espera ESPERADO NOMBRE COMANDO...: «rojo» (rc=1, un hallazgo) o «verde» (rc=0). Un rc=2 es un error de uso o de
# lectura: nunca cuenta como el rojo esperado.
espera() {
  local esperado="$1" nombre="$2"; shift 2
  local s rc; s="$("$@" 2>&1)"; rc=$?
  printf '%s\n' "$s" | sed 's/^/    /'
  local obtenido=error; [[ $rc -eq 0 ]] && obtenido=verde; [[ $rc -eq 1 ]] && obtenido=rojo
  if [[ "$obtenido" == "$esperado" ]]; then echo "  ok    $nombre: $obtenido (rc=$rc)"; return 0; fi
  echo "  FALLA $nombre: esperaba $esperado y dio $obtenido (rc=$rc)"; return 1
}

plantar() {  # copia del contrato extraído con una operación más
  python3 -I -c 'import json,sys
d=json.load(open(sys.argv[1]))
d["paths"]["/api/v1/plantada"]={"get":{"responses":{"200":{"description":"OK"}}}}
json.dump(d,open(sys.argv[2],"w"),indent=2)' "$TRABAJO/sha/openapi.json" "$TRABAJO/plantado.json"
}

# C4: rojo, y por las dos anclas del defecto real (no por otro hallazgo).
negativa() {
  # Sin anotaciones de GitHub: el rojo esperado no se marca como error en el PR y el formato de línea es uno solo.
  local s rc; s="$(GITHUB_ACTIONS=false py trazas --raiz "$NEGATIVA" --fuente "$TRABAJO/sha" "$NEGATIVA" 2>&1)"; rc=$?
  printf '%s\n' "$s" | sed 's/^/    /'
  local motivo
  for motivo in 'NombreEntry.MaxLength: la ficha afirma 60 y el código tiene 80' \
                'LocalidadRules.NombreValido no contiene «LargoMaximoDelNombre»'; do
    printf '%s\n' "$s" | grep -qF "$motivo" || { echo "  FALLA C4: falta el hallazgo «$motivo» (rc=$rc)"; return 1; }
  done
  [[ $rc -eq 1 && "$(printf '%s\n' "$s" | grep -c ' ERROR ')" -eq 2 ]] \
    || { echo "  FALLA C4: se esperaban exactamente 2 errores y rc=1 (rc=$rc)"; return 1; }
  echo "  ok    C4: rojo por el defecto real 60/80 (rc=1, 2 anclas)"
}

controles() {
  local -A res
  echo "== C1 contrato: contracts/openapi.json frente al extraído de $SHA"
  espera verde C1 py contrato "$DEMO/contracts/openapi.json" "$TRABAJO/sha/openapi.json"; res[C1]=$?
  echo "== C2 plantado: una operación de más tiene que dar exit 1"
  plantar && GITHUB_ACTIONS=false espera rojo C2 py contrato "$DEMO/contracts/openapi.json" "$TRABAJO/plantado.json"; res[C2]=$?
  echo "== C3 trazas: traces-resolve y anclas contra $SHA"
  espera verde C3 py trazas --raiz "$RAIZ" --fuente "$TRABAJO/sha" "$DEMO"; res[C3]=$?
  echo "== C4 negativa: RN-LOC-01 sin corregir (defecto real 60/80) tiene que dar rojo"
  negativa; res[C4]=$?
  echo "== A1 deriva contra $COMPARAR (avisa, no bloquea)"
  if [[ -f "$TRABAJO/comparar/openapi.json" ]]; then
    local cmt; cmt="$(cat "$TRABAJO/comparar/.commit")"
    local s rc
    s="$(py contrato "$DEMO/contracts/openapi.json" "$TRABAJO/comparar/openapi.json" 2>&1)"; rc=$?
    printf '%s\n' "$s" | sed 's/^/    /'
    case $rc in
      0) echo "  contrato: sin deriva frente a $COMPARAR ($cmt)" ;;
      1) aviso "deriva del contrato frente a $COMPARAR ($cmt): al actualizar source.lock, actualizar contracts/openapi.json" ;;
      *) aviso "la comparación del contrato contra $COMPARAR no pudo correr (rc=$rc)" ;;
    esac
    s="$(py trazas --raiz "$RAIZ" --deriva --fuente "$TRABAJO/comparar" "$DEMO" 2>&1)"; rc=$?
    printf '%s\n' "$s" | sed 's/^/    /'
    if [[ $rc -ne 0 ]]; then
      aviso "la comparación de anclas contra $COMPARAR no pudo correr (rc=$rc)"
    elif printf '%s\n' "$s" | grep -q 'avisos=0'; then
      echo "  anclas: sin deriva frente a $COMPARAR ($cmt)"
    else
      aviso "deriva de anclas frente a $COMPARAR ($cmt): ver las líneas «aviso» de arriba"
    fi
  else
    aviso "sin árbol de $COMPARAR: la comparación de deriva no corrió"
  fi
  echo "== agregador mini demo"
  local c total=0
  for c in C1 C2 C3 C4; do
    printf '  %-3s %s\n' "$c" "$([[ ${res[$c]} -eq 0 ]] && echo verde || echo ROJO)"
    [[ ${res[$c]} -ne 0 ]] && total=1
  done
  if [[ $total -eq 0 ]]; then echo "== mini demo: verde"; else echo "== mini demo: ROJO"; fi
  return $total
}

todo() {
  local t0=$SECONDS
  mkdir -p "$TRABAJO"
  fuente || { echo "== mini demo: ROJO (fuente)"; return 1; }
  contrato || { echo "== mini demo: ROJO (contrato)"; return 1; }
  controles; local rc=$?
  echo "== tiempo de pared: $((SECONDS - t0)) s"
  return $rc
}

case "$PASO" in
  fuente) mkdir -p "$TRABAJO"; fuente ;;
  contrato) contrato ;;
  todo) todo ;;
  *) echo "paso desconocido: $PASO" >&2; exit 2 ;;
esac
