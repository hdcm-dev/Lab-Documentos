#!/usr/bin/env bash
# verificar.sh — arnés único de Examples/ (plan §E.1, «Arnés»; H-058, H2-004, H2-018, H2-019).
#
# Corre en el HOST con bash, docker, jq y GNU grep (R-HOST): todo lo de .NET va en contenedores
# mcr.microsoft.com/dotnet/sdk:10.0, uno por fase. Cada fase pide su turno por separado y ninguna
# se anida en otra. Orden global de pedido: build → modelo-propio (3a) → modelo-compartido → modelo-propio (3b).
# Los recursos son papeles («modelo-compartido», «modelo-propio»), no equipos: el envoltorio TURNO_CMD
# los traduce a los recursos de su gestor.
#
# Uso:
#   tools/verificar.sh build                     fase build: restore, build -warnaserror, pruebas sin
#                                                servidor (Category!=LLM) y cada consola con --fake --verificar
#   tools/verificar.sh correr <carpeta> [--con-modelo]
#                                                un ejemplo: sin modelo, con turno build; con modelo, con
#                                                el turno del recurso que declara su ejemplo.json
#   tools/verificar.sh fase <3a|lan|3b>          los ejemplos con modelo de esa fase (necesita build antes)
#   tools/verificar.sh --con-modelo              las cuatro fases en orden
#   tools/verificar.sh <vía> [<carpeta>...] [--fake]
#                                                vía = extension | embeddings | estabilidad: la clave homónima
#                                                de ejemplo.json (§E.1, §E.5). Sin --fake, con el turno del
#                                                recurso de la vía (extension y estabilidad: modelo-compartido, con
#                                                la regla de cola de §E.5; embeddings: modelo-propio, fase 3a). Con
#                                                --fake, corrida en seco: turno build, compila y corre la vía con
#                                                --fake, sin red, con su registro aparte en RLLM_DIR/seco/
#   tools/verificar.sh medidor                   uso de cada recurso: registro del gestor + turnos.jsonl, y el
#                                                tope restante de modelo-compartido
#   tools/verificar.sh tasa <k> <m>              k/m con su intervalo Clopper-Pearson al 95 %
#
# Variables (ninguna tiene valor en el repo):
#   TURNO_CMD   envoltorio de turno; se invoca «$TURNO_CMD <recurso> -- <comando>». Vacía: corre sin
#               turno y lo avisa (lo que haría un lector con su propio Ollama).
#   TURNO_MAPA  (opcional) TSV «papel<TAB>recurso del gestor», fuera del repo: el medidor traduce con él
#               los papeles a los nombres que usa TURNO_REGISTRO.
#   RLLM_DIR    dónde quedan el registro R-LLM y turnos.jsonl. En los modos con modelo es OBLIGATORIA y
#               tiene que estar fuera del árbol de la guía; en build, en seco (--fake), medidor y tasa,
#               el defecto es ${TMPDIR:-/tmp}/guia-ia-registros (fuera del árbol).
#   Modos con modelo (fase, --con-modelo, correr --con-modelo y las vías sin --fake): estas variables
#   NO tienen defecto; si falta una, el arnés sale con 2 antes de pedir el turno y sin llamar a ningún
#   servidor. Sólo build y --fake usan el rótulo neutro «sin-servidor» / máquina «guion».
#     OLLAMA_SERVIDOR      compartido | propio | nube (va al campo «servidor» del registro).
#     RLLM_MAQUINA         rol de la máquina del servidor de chat compartido (p. ej. «estacion-1»), nunca
#                          el nombre de un equipo.
#     RLLM_MAQUINA_PROPIO  rol de la máquina del Ollama propio (p. ej. «estacion-2»), nunca un nombre de equipo.
#   Fase del servidor compartido (precondición, igual que la del propio; sin defecto):
#     OLLAMA_VERSION_ESPERADA  versión de Ollama que tiene que informar /api/version.
#     DIGEST_4B_ESPERADO       digest (prefijo de 12) del modelo de chat en /api/tags.
#     DIGEST_EMB_ESPERADO      digest del modelo de embeddings en ese servidor; «-» si la fase no lo usa.
#   OLLAMA_ENDPOINT, OLLAMA_MODEL               servidor de chat de la fase compartida.
#   OLLAMA_PROPIO_ENDPOINT, OLLAMA_PROPIO_URL_CMD
#               el Ollama propio; si está OLLAMA_PROPIO_URL_CMD, se ejecuta DENTRO del turno y su salida
#               es el endpoint (no se escribe en ningún archivo).
#   OLLAMA_PROPIO_SALUD_CMD  si está y falla antes del primer turno build, rige EMB-C (§E.5).
#   TOPE_COMPARTIDO_S  tope de modelo-compartido del ciclo (default 14400 s = 4 h, §E.5); vale para un
#               servidor ajeno: con uno propio, sin gestor, el tope no hace falta.
#   VOLUMEN_NUGET, VOLUMEN_ARTEFACTOS, ETIQUETA  volúmenes de docker y etiqueta de lo que crea el arnés
#               (defaults neutros: nuget, artefactos, proyecto=guia-ia).
#   COMPUERTAS_DIR  (opcional en build) carpeta fuera del repo con las listas de las compuertas; si trae
#               rpriv.patrones / rpriv.lista, la compuerta R-PRIV del build las usa (tools/compuerta-rpriv.sh).
#   TURNO_REGISTRO  registro append-only del gestor de turnos (TSV: fecha ISO-8601 · recurso · dueño ·
#               evento · motivo). Si está, el medidor suma de ahí los tramos concedido → liberado
#               (o vencida, huerfana, revocado) de TODAS las corridas, también las hechas a mano, y de
#               turnos.jsonl suma sólo las fases que corrieron sin turno (las otras ya están en el gestor).
#               Vacía: el medidor suma sólo turnos.jsonl y subestima lo que se corrió fuera del arnés.
#   TURNO_DESDE inicio del ciclo (fecha que entienda «date -d»); el medidor ignora lo anterior.
set -uo pipefail

AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GUIA="$(dirname "$AQUI")"
EX="$GUIA/Examples"
IMG="${SDK_IMAGEN:-mcr.microsoft.com/dotnet/sdk:10.0}"
TOPE_COMPARTIDO_S="${TOPE_COMPARTIDO_S:-14400}"
VOLUMEN_NUGET="${VOLUMEN_NUGET:-nuget}"
VOLUMEN_ARTEFACTOS="${VOLUMEN_ARTEFACTOS:-artefactos}"
ETIQUETA="${ETIQUETA:-proyecto=guia-ia}"
REC_COMPARTIDO=modelo-compartido
REC_PROPIO=modelo-propio
OLLAMA_VERSION_PROPIO="0.40.11"
DIGEST_CHAT_PROPIO="d8b0f5e9760c"   # qwen3.5:4b (Historia: a8b0c5157701, qwen2.5:0.5b)
DIGEST_EMBED_PROPIO="ac6da0dfba84"  # qwen3-embedding:0.6b
NUM_CTX=8192                        # REC-08: el mismo num_ctx en todas las fases y en todos los clientes
PUERTO_CERRADO="http://127.0.0.1:9"

decir() { printf '[verificar] %s\n' "$*" >&2; }

# ---------------------------------------------------------------- modos con modelo (M-A, M-K, ENTO-19)
# es_modo_con_modelo <acción> <args...>: 0 si la acción llama a un servidor de modelos.
es_modo_con_modelo() {
  local a="$1"; shift
  case "$a" in
    fase|--con-modelo) return 0 ;;
    correr) [ "${2:-}" = "--con-modelo" ] ;;
    extension|embeddings|estabilidad) local x; for x in "$@"; do [ "$x" = "--fake" ] && return 1; done; return 0 ;;
    *) return 1 ;;
  esac
}
# usa_compartido <acción> <args...>: 0 si el modo puede tocar el servidor compartido (precondición M-K).
usa_compartido() {
  case "$1" in
    fase) [ "${2:-}" = lan ] ;;
    --con-modelo|extension|estabilidad) return 0 ;;
    correr) [ -f "$EX/${2%/}/ejemplo.json" ] && [ "$(jq -r .recurso "$EX/${2%/}/ejemplo.json" 2>/dev/null)" = "$REC_COMPARTIDO" ] ;;
    *) return 1 ;;
  esac
}
# exigir_variables <nombre>...: sale con 2, con mensaje y sin ninguna llamada, si falta alguna.
exigir_variables() {
  local v falta=()
  for v in "$@"; do [ -n "${!v:-}" ] || falta+=("$v"); done
  [ ${#falta[@]} -eq 0 ] && return 0
  decir "modo con modelo sin ${falta[*]}: no tienen defecto (M-A, M-K); se aborta antes de pedir el turno, sin llamar a ningún servidor"
  exit 2
}
# fuera_del_arbol <ruta>: 0 si la ruta no queda dentro de la carpeta de la guía.
fuera_del_arbol() {
  local r; r="$(realpath -m "$1")"
  case "$r/" in "$GUIA"/*) return 1 ;; *) return 0 ;; esac
}
validar_modo_con_modelo() {
  exigir_variables OLLAMA_SERVIDOR RLLM_MAQUINA RLLM_MAQUINA_PROPIO RLLM_DIR
  case "$OLLAMA_SERVIDOR" in compartido|propio|nube) ;;
    *) decir "OLLAMA_SERVIDOR inválido: «$OLLAMA_SERVIDOR» (compartido | propio | nube)"; exit 2 ;; esac
  fuera_del_arbol "$RLLM_DIR" || { decir "RLLM_DIR queda dentro de la carpeta de la guía: tiene que estar fuera del árbol (ENTO-19)"; exit 2; }
  if usa_compartido "$@"; then exigir_variables OLLAMA_VERSION_ESPERADA DIGEST_4B_ESPERADO DIGEST_EMB_ESPERADO; fi
}

ACCION0="${1:-build}"
if es_modo_con_modelo "$@"; then
  validar_modo_con_modelo "$@"
else
  case "$ACCION0" in _*) ;; *) export OLLAMA_SERVIDOR="${OLLAMA_SERVIDOR:-sin-servidor}" ;; esac
  RLLM_DIR="${RLLM_DIR:-${TMPDIR:-/tmp}/guia-ia-registros}"
fi
case "$ACCION0" in tasa|-h|--help|ayuda) ;; *) mkdir -p "$RLLM_DIR"; RLLM_DIR="$(cd "$RLLM_DIR" && pwd)"; export RLLM_DIR ;; esac
TURNOS="$RLLM_DIR/turnos.jsonl"

ahora() { date -u +%Y-%m-%dT%H:%M:%SZ; }
epoch() { date -u +%s; }

volumenes() {
  local v
  for v in "$VOLUMEN_NUGET" "$VOLUMEN_ARTEFACTOS"; do
    docker volume inspect "$v" >/dev/null 2>&1 || docker volume create --label "$ETIQUETA" "$v" >/dev/null
  done
  # Los volúmenes nacen de root; el SDK corre como 1000:1000.
  docker run --rm --label "$ETIQUETA" -v "$VOLUMEN_NUGET":/nuget -v "$VOLUMEN_ARTEFACTOS":/artefactos "$IMG" \
    sh -c 'chown 1000:1000 /nuget /artefactos' >/dev/null
}

# sdk <args de docker ...> -- <comando>: un contenedor efímero del SDK con el árbol de Examples/ y
# el registro montados. Las variables se pasan por nombre (-e VAR), nunca con su valor en un archivo.
sdk() {
  local extra=()
  while [ $# -gt 0 ] && [ "$1" != "--" ]; do extra+=("$1"); shift; done
  shift
  docker run --rm --label "$ETIQUETA" -u 1000:1000 \
    -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 -e NUGET_PACKAGES=/nuget \
    -e IA_LAYER_ARTEFACTOS=/artefactos -e RLLM_DIR=/registros -e RLLM_TURNO_ID -e EMB_PLAN \
    -v "$VOLUMEN_NUGET":/nuget -v "$VOLUMEN_ARTEFACTOS":/artefactos \
    -v "$EX":/src -v "$RLLM_DIR":/registros -w /src "${extra[@]}" "$IMG" "$@"
}

# curl desde un contenedor (el host no necesita curl: R-HOST). Sale con el código de curl.
http() { docker run --rm --label "$ETIQUETA" "$IMG" curl -sS --max-time "${HTTP_MAX:-15}" "$@"; }

# ---------------------------------------------------------------- turnos y medidor (H2-004)
# turno <recurso> <fase> <comando...>: pide el turno, corre, libera y agrega una línea a turnos.jsonl.
turno() {
  local recurso="$1" fase="$2"; shift 2
  local id; id="$(date -u +%Y%m%dT%H%M%S)-$recurso-$$-$RANDOM"
  local marca; marca="$(mktemp "$RLLM_DIR/.concedido.XXXXXX")"
  local pedido; pedido="$(ahora)"
  export RLLM_TURNO_ID="$id"
  local rc
  if [ -z "${TURNO_CMD:-}" ]; then
    decir "TURNO_CMD vacío: la fase «$fase» corre SIN turno (válido para un lector con su propio servidor)"
    "$0" _concedido "$marca" "$@"; rc=$?
  else
    # shellcheck disable=SC2086
    $TURNO_CMD "$recurso" -- "$0" _concedido "$marca" "$@"; rc=$?
  fi
  local liberado; liberado="$(ahora)"
  local concedido; concedido="$(cat "$marca" 2>/dev/null)"; rm -f "$marca"
  local dur=0
  if [ -n "$concedido" ]; then dur=$(( $(date -u -d "$liberado" +%s) - $(date -u -d "$concedido" +%s) )); fi
  # Rótulos de la fila: con modelo, los del entorno (M-A); en build y en seco, «sin-servidor» y «guion».
  local srv=sin-servidor maq=guion
  case "$recurso" in
    "$REC_COMPARTIDO") srv="$OLLAMA_SERVIDOR"; maq="$RLLM_MAQUINA" ;;
    "$REC_PROPIO") srv="$OLLAMA_SERVIDOR"; maq="$RLLM_MAQUINA_PROPIO" ;;
  esac
  jq -nc --arg id "$id" --arg r "$recurso" --arg f "$fase" --arg p "$pedido" --arg c "$concedido" \
        --arg l "$liberado" --argjson d "$dur" --argjson rc "$rc" --arg sin "${TURNO_CMD:+no}" \
        --arg srv "$srv" --arg maq "$maq" \
    '{turno_id:$id, recurso:$r, fase:$f, pedido_en:$p, concedido_en:(if $c=="" then null else $c end),
      liberado_en:$l, duracion_s:$d, rc:$rc, sin_turno:($sin==""), servidor:$srv, maquina:$maq}' >> "$TURNOS"
  return $rc
}

# Medidor. Dos fuentes: el registro del gestor (TURNO_REGISTRO), que ve todas las concesiones, y
# turnos.jsonl, que ve sólo las fases del arnés. Una fase con turno está en las dos: se cuenta una vez.
gestor_disponible() { [ -n "${TURNO_REGISTRO:-}" ] && [ -r "$TURNO_REGISTRO" ]; }
desde_epoch() { if [ -n "${TURNO_DESDE:-}" ]; then date -u -d "$TURNO_DESDE" +%s; else echo 0; fi; }

# gestor_usado <recurso>: segundos concedidos según el gestor. Cada «concedido» de (recurso, dueño) se
# cierra con el siguiente liberado | vencida | huerfana | revocado del mismo par; una concesión todavía
# abierta cuenta hasta ahora (así incluye el turno en curso). vencida y huerfana se anotan cuando el
# gestor las recupera, después del vencimiento real: el error es por exceso, del lado seguro del tope.
# recurso_del_gestor <papel>: el nombre que usa el gestor (TURNO_MAPA, fuera del repo) o el mismo papel.
recurso_del_gestor() {
  local r=""
  if [ -n "${TURNO_MAPA:-}" ] && [ -r "$TURNO_MAPA" ]; then r="$(awk -F'\t' -v p="$1" '$1==p {print $2; exit}' "$TURNO_MAPA")"; fi
  printf '%s\n' "${r:-$1}"
}

gestor_usado() {
  local recurso lineas
  recurso="$(recurso_del_gestor "$1")"
  gestor_disponible || return 1
  lineas="$(awk -F'\t' -v r="$recurso" '$2==r && ($4=="concedido" || $4=="liberado" || $4=="vencida" || $4=="huerfana" || $4=="revocado") { print $1 "\t" $3 "\t" $4 }' "$TURNO_REGISTRO")"
  [ -n "$lineas" ] || { echo 0; return 0; }
  paste <(cut -f1 <<<"$lineas" | date -u -f - +%s) <(cut -f2,3 <<<"$lineas") \
    | awk -F'\t' -v ahora="$(epoch)" -v desde="$(desde_epoch)" '
        $3 == "concedido" { abierta[$2] = $1; next }
        ($2 in abierta) { c = abierta[$2]; delete abierta[$2]; if (c >= desde) s += $1 - c }
        END { for (d in abierta) if (abierta[d] >= desde) s += ahora - abierta[d]; print s + 0 }'
}

# jsonl_usado <recurso> <todas|sin-turno>: suma de duracion_s de turnos.jsonl desde TURNO_DESDE.
jsonl_usado() {
  [ -f "$TURNOS" ] || { echo 0; return 0; }
  jq -s --arg r "$1" --arg q "$2" --argjson desde "$(desde_epoch)" '
    [ .[] | select(.recurso == $r and (.duracion_s | type) == "number")
          | select($q == "todas" or .sin_turno == true)
          | select(((.concedido_en // .liberado_en) | fromdateiso8601) >= $desde) | .duracion_s ] | add // 0' "$TURNOS"
}

# usado <recurso>: con gestor, gestor + fases sin turno; sin gestor, todo turnos.jsonl.
usado() {
  local g
  if g="$(gestor_usado "$1")"; then echo $(( g + $(jsonl_usado "$1" sin-turno) ))
  else jsonl_usado "$1" todas; fi
}

# Tope restante de modelo-compartido: tope del ciclo menos lo ya usado (y lo usado en el turno en curso, que
# con gestor ya está en su concesión abierta; sin gestor se suma desde la hora de concesión «desde»).
tope_restante() {
  local desde="${1:-}" curso=0
  if ! gestor_disponible && [ -n "$desde" ]; then curso=$(( $(epoch) - $(date -u -d "$desde" +%s) )); fi
  echo $(( TOPE_COMPARTIDO_S - $(usado "$REC_COMPARTIDO") - curso ))
}

medidor() {
  local r g fuente
  if gestor_disponible; then fuente="gestor + fases sin turno de turnos.jsonl"; else fuente="sólo turnos.jsonl (TURNO_REGISTRO vacía)"; fi
  printf 'recurso\tgestor_s\tturnos_jsonl_s\tsin_turno_s\tusado_s\n'
  for r in build "$REC_PROPIO" "$REC_COMPARTIDO"; do
    g="$(gestor_usado "$r" || echo -)"
    printf '%s\t%s\t%s\t%s\t%s\n' "$r" "$g" "$(jsonl_usado "$r" todas)" "$(jsonl_usado "$r" sin-turno)" "$(usado "$r")"
  done
  decir "fuente: $fuente; desde: ${TURNO_DESDE:-el principio de los registros}"
  decir "$REC_COMPARTIDO: tope $TOPE_COMPARTIDO_S s, restante $(tope_restante) s"
}

# ---------------------------------------------------------------- manifiestos (ejemplo.json)
manifiestos() {
  find "$EX" -name ejemplo.json -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/registros/*' | sort
}

# ejemplos_de <recurso> <fase>: carpetas (relativas a Examples/) cuyo manifiesto declara ese recurso.
ejemplos_de() {
  local recurso="$1" fase="${2:-}" m
  while read -r m; do
    jq -e --arg r "$recurso" --arg f "$fase" \
      '.recurso==$r and ($f=="" or (.fase // "3a")==$f or $r!="modelo-propio")' "$m" >/dev/null 2>&1 \
      && dirname "${m#"$EX"/}"
  done < <(manifiestos)
}

# Comprueba que las entradas congeladas no cambiaron (H2-023): SHA-256 del manifiesto = del archivo.
entradas_congeladas() {
  local dir="$1" ok=0 ruta sha real
  while IFS=$'\t' read -r ruta sha; do
    [ -z "$ruta" ] && continue
    real="$(sha256sum "$EX/$dir/$ruta" 2>/dev/null | cut -d' ' -f1)"
    if [ "$real" != "$sha" ]; then decir "$dir: $ruta no coincide con su SHA-256 congelado"; ok=1; fi
  done < <(jq -r '.entradas[]? | [.ruta, .sha256] | @tsv' "$EX/$dir/ejemplo.json")
  return $ok
}

# ---------------------------------------------------------------- fase build
fase_build() {
  local proyectos pruebas consolas p
  proyectos="$(cd "$EX" && find . -name '*.csproj' -not -path '*/bin/*' -not -path '*/obj/*' | sort | sed 's|^\./||')"
  pruebas="$(cd "$EX" && grep -l 'Microsoft.NET.Test.Sdk' $proyectos 2>/dev/null)"
  consolas="$(ejemplos_de ninguno; ejemplos_de "$REC_PROPIO"; ejemplos_de "$REC_COMPARTIDO")"
  consolas="$(printf '%s\n' "$consolas" | sed '/^$/d' | sort -u)"
  for p in $consolas; do entradas_congeladas "$p" || return 1; done
  # La guía va montada en sólo lectura para que ArnesTests corra el arnés (casos sin variables, M-A/M-K).
  sdk -e PROYECTOS="$proyectos" -e PRUEBAS="$pruebas" -e CONSOLAS="$consolas" \
      -e OLLAMA_ENDPOINT="$PUERTO_CERRADO" -e OLLAMA_PROPIO_ENDPOINT="$PUERTO_CERRADO" \
      -e OLLAMA_EMBED_ENDPOINT="$PUERTO_CERRADO" -e RLLM_MAQUINA=guion \
      -v "$GUIA":/guia-ro:ro -e ARNES_GUIA=/guia-ro -- sh -ec '
    echo "== SDK"; dotnet --version
    echo "== restore (con red a nuget.org)"
    for p in $PROYECTOS; do dotnet restore "$p" --nologo -v q; done
    echo "== build -warnaserror"
    for p in $PROYECTOS; do dotnet build "$p" --no-restore -warnaserror --nologo -v q -clp:NoSummary; done
    echo "== test sin servidor de modelos (Category!=LLM)"
    for p in $PRUEBAS; do dotnet test "$p" --no-build --nologo --filter "Category!=LLM" -v q; done
    echo "== consolas --fake --verificar"
    for c in $CONSOLAS; do
      csp=$(ls "$c"/*.csproj | head -1)
      [ -n "$csp" ] || { echo "$c: sin .csproj"; exit 1; }
      echo "-- $c"; dotnet run --no-build --project "$csp" -- --fake --verificar
    done
    echo "== paquetes (AC-6)"
    rm -rf /registros/paquetes; mkdir -p /registros/paquetes   # ESTA-10: sin listados de corridas anteriores
    for p in $PROYECTOS; do
      dotnet list "$p" package --include-transitive --format json > "/registros/paquetes/$(basename "$p" .csproj).json"
    done
  ' || return 1
  ac6 || return 1
  # Compuerta R-PRIV (RPRI-17/19) sobre lo versionado de la guía, como paso del build.
  "$AQUI/compuerta-rpriv.sh" "$GUIA" || { decir "compuerta R-PRIV: hay hallazgos (ver arriba)"; return 1; }
}

# AC-6: cada familia en una sola versión y ninguna preview que el Anexo F no declare.
ac6() {
  local archivos=("$RLLM_DIR"/paquetes/*.json)
  [ -e "${archivos[0]}" ] || { decir "AC-6: no hay listados de paquetes"; return 1; }
  # Previews que el Anexo F declara: Compuertas/ac6.excepciones, «<id> <versión exacta> <motivo>».
  local excepciones="$GUIA/Compuertas/ac6.excepciones" admitidas='[]'
  if [ -f "$excepciones" ]; then
    admitidas="$(grep -v '^[[:space:]]*\(#\|$\)' "$excepciones" \
      | jq -Rnc '[inputs | split(" ") | select(length >= 3) | "\(.[0]) \(.[1])"]')"
  fi
  local salida
  salida="$(jq -rs --argjson admitidas "$admitidas" '
    [ .[] | .projects[]? | .frameworks[]? | ((.topLevelPackages // []) + (.transitivePackages // []))[]
      | {id, v: (.resolvedVersion)} ] as $todos
    | ["Microsoft.Extensions.AI", "Microsoft.Extensions.VectorData", "Microsoft.Agents.AI",
       "ModelContextProtocol", "OpenTelemetry", "Microsoft.EntityFrameworkCore"] as $familias
    | ( $familias[] as $f
        | [ $todos[] | select(.id == $f or (.id | startswith($f + "."))) | .v ] | unique
        | select(length > 1) | "familia \($f) en varias versiones: \(join(", "))" ),
      ( $todos[] | select(.v | test("-(preview|rc|alpha|beta)"))
        | select(("\(.id) \(.v)") as $k | $admitidas | index($k) | not) | "preview: \(.id) \(.v)" ),
      ( ([ $todos[] | "\(.id) \(.v)" ] | unique) as $vistos
        | $admitidas[] | select(. as $k | $vistos | index($k) | not)
        | "excepción vencida (no se resuelve en ningún proyecto): \(.)" )
  ' "${archivos[@]}" | sort -u)"
  # Microsoft.Extensions.AI.Evaluation* y Microsoft.Extensions.AI.Abstractions comparten la familia M.E.AI.
  if [ -n "$salida" ]; then decir "AC-6 falla:"; printf '%s\n' "$salida" >&2; return 1; fi
  decir "AC-6: familias en una sola versión; previews sólo las de Compuertas/ac6.excepciones: $(jq -r 'join(", ") | if .=="" then "ninguna" else . end' <<<"$admitidas")"
}

# ---------------------------------------------------------------- fases con modelo
# Lectura de JSON sin jq (la precondición también corre en el contenedor del SDK, dentro de ArnesTests).
# json_texto <clave>: el primer valor de texto de esa clave en la entrada estándar.
json_texto() { sed -n "s/.*\"$1\"[[:space:]]*:[[:space:]]*\"\([^\"]*\)\".*/\1/p" | head -1; }
# digest_de <modelo>: el digest que /api/tags (entrada estándar) informa para ese nombre de modelo.
digest_de() {
  sed 's/{[[:space:]]*"name"/\n&/g' | grep -F "\"$1\"" | grep -E "^\{[[:space:]]*\"name\"[[:space:]]*:[[:space:]]*\"$(printf '%s' "$1" | sed 's/[.]/\\./g')\"" \
    | sed -n 's/.*"digest"[[:space:]]*:[[:space:]]*"\([0-9a-f]*\)".*/\1/p' | head -1
}
limpio() { printf '%s' "$1" | tr -cd 'A-Za-z0-9._:+-'; }
# coincide_digest <real> <esperado>: el esperado es el prefijo (12 o 64 caracteres) del real.
coincide_digest() { [ -n "$1" ] && [ -n "$2" ] && [ "${1#"$2"}" != "$1" ]; }

# precondicion_compartido: dentro del turno de modelo-compartido, antes de cualquier medición (M-K).
# Lee /api/version y /api/tags y compara con OLLAMA_VERSION_ESPERADA, DIGEST_4B_ESPERADO y
# DIGEST_EMB_ESPERADO (sin defecto; «-» en el de embeddings si la fase no lo usa). Escribe una fila
# {tipo:"servidor-compartido", …, coincide} en turnos.jsonl. Si algo no coincide: rc 4, «no medido».
precondicion_compartido() {
  local ep="${OLLAMA_ENDPOINT:?OLLAMA_ENDPOINT sin definir para la fase compartida}" modelo="${OLLAMA_MODEL:-qwen3.5:4b}"
  local memb="${OLLAMA_EMBED_MODEL:-qwen3-embedding:0.6b}" vesp="${OLLAMA_VERSION_ESPERADA:?}" desp="${DIGEST_4B_ESPERADO:?}"
  local eesp="${DIGEST_EMB_ESPERADO:?}" version tags d4b demb ok=true
  version="$(http "$ep/api/version" | json_texto version)" || version=""
  tags="$(http "$ep/api/tags")" || tags=""
  d4b="$(printf '%s' "$tags" | digest_de "$modelo")"
  [ "$version" = "$vesp" ] || ok=false
  coincide_digest "$d4b" "$desp" || ok=false
  if [ "$eesp" = "-" ]; then demb="-"
  else demb="$(printf '%s' "$tags" | digest_de "$memb")"; coincide_digest "$demb" "$eesp" || ok=false; fi
  printf '{"tipo":"servidor-compartido","fecha":"%s","turno_id":"%s","servidor":"%s","maquina":"%s","ollama_version":"%s","version_esperada":"%s","modelo":"%s","digest":"%s","digest_esperado":"%s","modelo_embeddings":"%s","digest_embeddings":"%s","digest_embeddings_esperado":"%s","coincide":%s}\n' \
    "$(ahora)" "$(limpio "${RLLM_TURNO_ID:-}")" "$(limpio "${OLLAMA_SERVIDOR:-}")" "$(limpio "${RLLM_MAQUINA:-}")" \
    "$(limpio "$version")" "$(limpio "$vesp")" "$(limpio "$modelo")" "$(limpio "${d4b:0:12}")" "$(limpio "$desp")" \
    "$(limpio "$memb")" "$(limpio "${demb:0:12}")" "$(limpio "$eesp")" "$ok" >> "$TURNOS"
  if [ "$ok" != true ]; then
    decir "el servidor compartido no es el esperado (versión «$version», digest «${d4b:0:12}», embeddings «${demb:0:12}»): no se mide («no medido»)"
    return 4
  fi
  decir "servidor compartido: versión y digests coinciden con los esperados"
}

# cargar_con_num_ctx <endpoint> <modelo> <fase>: REC-08. Carga el modelo con num_ctx = NUM_CTX dentro del
# turno (una llamada de calentamiento, presupuestada), lee /api/ps y registra lo que informa en turnos.jsonl.
# Si /api/ps no informa NUM_CTX, la fase no mide (rc 1): los clientes envían NUM_CTX y recargarían el modelo.
cargar_con_num_ctx() {
  local ep="$1" modelo="$2" fase="$3" ps ctx
  HTTP_MAX=900 http -X POST "$ep/api/generate" \
    -d "{\"model\":\"$modelo\",\"prompt\":\"\",\"stream\":false,\"options\":{\"num_ctx\":$NUM_CTX}}" >/dev/null \
    || { decir "no se pudo cargar $modelo con num_ctx $NUM_CTX"; return 1; }
  ps="$(http "$ep/api/ps")" || { decir "no responde /api/ps"; return 1; }
  ctx="$(jq -r --arg m "$modelo" '[.models[]? | select(.name==$m or .model==$m) | .context_length][0] // empty' <<<"$ps")"
  jq -nc --arg f "$(ahora)" --arg t "${RLLM_TURNO_ID:-}" --arg fase "$fase" --arg m "$modelo" --argjson pedido "$NUM_CTX" \
        --arg ps "$ctx" '{tipo:"num-ctx", fecha:$f, turno_id:$t, fase:$fase, modelo:$m, num_ctx_pedido:$pedido,
                          num_ctx_ps:(if $ps=="" then null else ($ps|tonumber) end), num_ctx_origen:"arnes-ps-tras-carga"}' >> "$TURNOS"
  [ "$ctx" = "$NUM_CTX" ] || { decir "/api/ps informa num_ctx «$ctx» tras cargar $modelo con $NUM_CTX: no se mide"; return 1; }
  export OLLAMA_NUM_CTX="$NUM_CTX" OLLAMA_NUM_CTX_ORIGEN=arnes-ps-tras-carga
  decir "num_ctx = $ctx ($OLLAMA_NUM_CTX_ORIGEN, $modelo)"
}

# descargar <endpoint> <modelo>...: keep_alive 0 al cerrar la fase (ENTO-10); sólo los que /api/ps lista.
descargar() {
  local ep="$1" m ps; shift
  ps="$(http "$ep/api/ps" 2>/dev/null)" || return 0
  for m in "$@"; do
    jq -e --arg m "$m" '[.models[]? | select(.name==$m or .model==$m)] | length > 0' <<<"$ps" >/dev/null 2>&1 || continue
    if [ "$m" = "${OLLAMA_EMBED_MODEL:-qwen3-embedding:0.6b}" ]; then
      http -X POST "$ep/api/embed" -d "{\"model\":\"$m\",\"input\":\"\",\"keep_alive\":0}" >/dev/null 2>&1
    else
      http -X POST "$ep/api/generate" -d "{\"model\":\"$m\",\"keep_alive\":0}" >/dev/null 2>&1
    fi
    decir "descargado $m (keep_alive 0)"
  done
  return 0
}

# num_ctx_compartido: dentro del turno de modelo-compartido, antes de la primera medición (§E.1, REC-08, M-K).
num_ctx_compartido() {
  local ep="${OLLAMA_ENDPOINT:?OLLAMA_ENDPOINT sin definir para la fase compartida}" modelo="${OLLAMA_MODEL:-qwen3.5:4b}"
  local ps otros
  precondicion_compartido || return $?
  ps="$(http "$ep/api/ps")" || { decir "no responde /api/ps"; return 1; }
  otros="$(jq -r --arg m "$modelo" '[.models[]? | select(.name!=$m and .model!=$m) | .name] | join(", ")' <<<"$ps")"
  if [ -n "$otros" ]; then decir "/api/ps lista otro modelo ($otros): se aborta sin llamar (R-04)"; return 3; fi
  cargar_con_num_ctx "$ep" "$modelo" compartida || return 1
  export RLLM_MAQUINA
}

# propio: dentro del turno de modelo-propio. Endpoint del gestor (sin escribirlo), versión y digests.
propio() {
  if [ -n "${OLLAMA_PROPIO_URL_CMD:-}" ]; then
    OLLAMA_PROPIO_ENDPOINT="$($OLLAMA_PROPIO_URL_CMD)" || { decir "OLLAMA_PROPIO_URL_CMD falló"; return 1; }
  fi
  export OLLAMA_PROPIO_ENDPOINT="${OLLAMA_PROPIO_ENDPOINT:-http://localhost:11434}"
  local version tags
  version="$(http "$OLLAMA_PROPIO_ENDPOINT/api/version" | jq -r .version)" || return 1
  tags="$(http "$OLLAMA_PROPIO_ENDPOINT/api/tags")" || return 1
  local dchat demb
  dchat="$(jq -r '[.models[] | select(.name=="qwen3.5:4b") | .digest[0:12]][0] // ""' <<<"$tags")"
  demb="$(jq -r '[.models[] | select(.name=="qwen3-embedding:0.6b") | .digest[0:12]][0] // ""' <<<"$tags")"
  jq -nc --arg t "$RLLM_TURNO_ID" --arg v "$version" --arg c "$dchat" --arg e "$demb" --arg f "$(ahora)" \
    '{tipo:"servidor-propio", fecha:$f, turno_id:$t, ollama_version:$v, digests:{"qwen3.5:4b":$c, "qwen3-embedding:0.6b":$e}}' \
    >> "$RLLM_DIR/servidor-propio.jsonl"
  if [ "$version" != "$OLLAMA_VERSION_PROPIO" ] || [ "$dchat" != "$DIGEST_CHAT_PROPIO" ] || [ "$demb" != "$DIGEST_EMBED_PROPIO" ]; then
    decir "Ollama propio no es el de la corrida (versión $version; digests $dchat / $demb)"; return 1
  fi
  cargar_con_num_ctx "$OLLAMA_PROPIO_ENDPOINT" "${OLLAMA_PROPIO_MODEL:-qwen3.5:4b}" propia || return 1
  export OLLAMA_EMBED_ENDPOINT="${OLLAMA_EMBED_ENDPOINT:-$OLLAMA_PROPIO_ENDPOINT}"
  # M-A: OLLAMA_SERVIDOR sale del entorno (sin defecto); la máquina es el rol del Ollama propio.
  export RLLM_MAQUINA="$RLLM_MAQUINA_PROPIO"
}

# correr_con_modelo <carpeta> [plazo] [args...]: la consola del ejemplo, sin compilar (lo hizo la fase
# build). Sin args, «--verificar»; una vía pasa los suyos (args_de_via).
correr_con_modelo() {
  local dir="$1" plazo="${2:-}" csp
  shift; [ $# -gt 0 ] && shift
  [ $# -gt 0 ] || set -- --verificar
  entradas_congeladas "$dir" || return 1
  csp="$(cd "$EX" && ls "$dir"/*.csproj | head -1)"
  sdk -e OLLAMA_ENDPOINT -e OLLAMA_MODEL -e OLLAMA_SERVIDOR -e OLLAMA_NUM_CTX -e OLLAMA_NUM_CTX_ORIGEN \
      -e OLLAMA_PROPIO_ENDPOINT -e OLLAMA_PROPIO_MODEL -e OLLAMA_EMBED_ENDPOINT -e OLLAMA_EMBED_MODEL \
      -e RLLM_MAQUINA -e RLLM_UMBRAL_MS_POR_TOKEN -e RLLM_CARACTERES_POR_TOKEN -- \
      dotnet run --no-build --project "$csp" -- "$@" ${plazo:+--plazo-s "$plazo"}
}

# ---------------------------------------------------------------- vías (§E.1, §E.5)
# Una vía es una corrida más de un ejemplo, declarada en su ejemplo.json con una clave propia:
#   "extension":   { "turno": "TURNO-2", "comando": "... -- --verificar --extension", "pesimista_s": N }
#   "estabilidad": { "turno": "TURNO-1", "comando": "... -- --verificar --estabilidad", "pesimista_s": N }
#   "embeddings":  { "recurso": "modelo-propio", "fase": "3a", "comando": "... -- --precomputar-embeddings" }
# El arnés toma los argumentos de la consola de «comando» (lo que sigue a « -- »), sin --fake; el
# recurso, de la vía o, si no lo dice, del manifiesto.
# vias_de <vía> [recurso] [turno|fase]: carpetas cuyo manifiesto declara la vía (con ese recurso y turno o fase).
vias_de() {
  local via="$1" recurso="${2:-}" filtro="${3:-}" m
  while read -r m; do
    jq -e --arg v "$via" --arg r "$recurso" --arg f "$filtro" '
      .recurso as $base | (.[$v] // null) as $x | $x != null
      and ($r == "" or (($x.recurso // $base) == $r))
      and ($f == "" or (if ($f | startswith("TURNO-")) then ($x.turno // "") == $f else ($x.fase // "3a") == $f end))' "$m" >/dev/null 2>&1 \
      && dirname "${m#"$EX"/}"
  done < <(manifiestos)
}

campo_via() { jq -r --arg v "$2" --arg c "$3" '(.[$v][$c] // (if $c == "recurso" then .recurso else empty end)) // empty' "$EX/$1/ejemplo.json"; }

# args_de_via <carpeta> <vía>: un argumento por línea.
args_de_via() {
  campo_via "$1" "$2" comando | sed -n 's/^.* -- //p' | tr ' ' '\n' | sed '/^$/d' | grep -vx -- '--fake'
}

# cabe_en_cola <carpeta> <vía> <restante>: regla de cola de §E.5 (el pesimista entra entero en el tope).
cabe_en_cola() {
  local p; p="$(campo_via "$1" "$2" pesimista_s)"
  [ -z "$p" ] && return 0
  [ "$p" -le "$3" ] && return 0
  decir "$1 ($2): pesimista $p s no entra en los $3 s que quedan → «no ejecutado contra el modelo» (regla de cola, §E.5)"
  return 1
}

# correr_via_con_modelo <carpeta> <vía> [plazo]: dentro del turno ya concedido.
correr_via_con_modelo() {
  local dir="$1" via="$2" plazo="${3:-}" args
  mapfile -t args < <(args_de_via "$dir" "$via")
  [ ${#args[@]} -gt 0 ] || { decir "$dir: la vía $via no declara argumentos en «comando»"; return 1; }
  decir "$dir · vía $via: ${args[*]}${plazo:+ (plazo $plazo s)}"
  correr_con_modelo "$dir" "$plazo" "${args[@]}"
}

# via <vía> [--fake] [carpeta...]
via() {
  local via="$1"; shift
  local seco=0 dirs=() d rec rc=0
  for d in "$@"; do
    if [ "$d" = "--fake" ]; then seco=1; else d="${d%/}"; dirs+=("${d#Examples/}"); fi
  done
  [ ${#dirs[@]} -gt 0 ] || mapfile -t dirs < <(vias_de "$via")
  [ ${#dirs[@]} -gt 0 ] || { decir "ningún ejemplo.json declara la vía $via"; return 2; }
  for d in "${dirs[@]}"; do
    [ -n "$(campo_via "$d" "$via" comando)" ] || { decir "$d no declara la vía $via en su ejemplo.json"; return 2; }
  done
  if [ $seco = 1 ]; then
    # Corrida en seco: un solo turno build para todas las carpetas; registro aparte (RLLM_DIR/seco/),
    # para que un archivo «guion» nunca quede donde lo lee una corrida que cuenta.
    volumenes || return 2
    for d in "${dirs[@]}"; do entradas_congeladas "$d" || return 1; done
    mkdir -p "$RLLM_DIR/seco"
    turno build "seco $via" env RLLM_DIR="$RLLM_DIR/seco" "$0" _via_seco "$via" "${dirs[@]}"
    return $?
  fi
  for d in "${dirs[@]}"; do
    rec="$(campo_via "$d" "$via" recurso)"
    case "$rec" in
      "$REC_COMPARTIDO")
        cabe_en_cola "$d" "$via" "$(tope_restante)" || continue
        turno "$REC_COMPARTIDO" "$via $d" "$0" _via_uno lan "$d" "$via" || rc=1 ;;
      "$REC_PROPIO")
        turno "$REC_PROPIO" "$via $d" "$0" _via_uno propio "$d" "$via" || rc=1 ;;
      *) decir "$d: la vía $via no usa modelo (recurso $rec)"; rc=1 ;;
    esac
  done
  return $rc
}

fase_modelo() {
  local fase="$1" concedido="$2" dir rc=0 restante
  case "$fase" in
    lan)   # nombre histórico de la fase del papel modelo-compartido (no se renombra: rótulos de capturas)
      num_ctx_compartido || return $?
      # Cada turno: primero los ejemplos, después las vías de ese turno (estabilidad en TURNO-1,
      # extensiones en TURNO-2), éstas con la regla de cola.
      for t in TURNO-1 TURNO-2; do
        for dir in $(ejemplos_de "$REC_COMPARTIDO"); do
          jq -e --arg t "$t" '.turno==$t' "$EX/$dir/ejemplo.json" >/dev/null || continue
          restante="$(tope_restante "$concedido")"
          if [ "$restante" -le 0 ]; then decir "$dir: tope de $REC_COMPARTIDO agotado → «no ejecutado contra el modelo»"; continue; fi
          decir "$dir ($t), plazo $restante s"; correr_con_modelo "$dir" "$restante" || rc=1
        done
        for v in estabilidad extension; do
          for dir in $(vias_de "$v" "$REC_COMPARTIDO" "$t"); do
            restante="$(tope_restante "$concedido")"
            if [ "$restante" -le 0 ]; then decir "$dir ($v): tope de $REC_COMPARTIDO agotado → «no ejecutado contra el modelo»"; continue; fi
            cabe_en_cola "$dir" "$v" "$restante" || continue
            correr_via_con_modelo "$dir" "$v" "$restante" || rc=1
          done
        done
      done
      descargar "$OLLAMA_ENDPOINT" "${OLLAMA_MODEL:-qwen3.5:4b}" ;;
    3a|3b)
      propio || return $?
      for dir in $(ejemplos_de "$REC_PROPIO" "$fase"); do decir "$dir (fase $fase)"; correr_con_modelo "$dir" || rc=1; done
      # Fase de embeddings (3a): vectoriza y persiste en RLLM_DIR/embeddings/ lo que lee después la fase compartida.
      for dir in $(vias_de embeddings "$REC_PROPIO" "$fase"); do correr_via_con_modelo "$dir" embeddings || rc=1; done
      descargar "$OLLAMA_PROPIO_ENDPOINT" "${OLLAMA_PROPIO_MODEL:-qwen3.5:4b}" "${OLLAMA_EMBED_MODEL:-qwen3-embedding:0.6b}" ;;
  esac
  return $rc
}

# ---------------------------------------------------------------- Clopper-Pearson (§E.1, §10.4)
tasa() {
  awk -v k="$1" -v n="$2" '
    function lfact(x,  s, i) { s = 0; for (i = 2; i <= x; i++) s += log(i); return s }
    function cdf(x, p,  s, i) { if (p <= 0) return 1; if (p >= 1) return (x >= n) ? 1 : 0
      s = 0; for (i = 0; i <= x; i++) s += exp(lfact(n) - lfact(i) - lfact(n - i) + i * log(p) + (n - i) * log(1 - p)); return s }
    function biseca(f, objetivo, x,  lo, hi, m, j) { lo = 0; hi = 1
      for (j = 0; j < 200; j++) { m = (lo + hi) / 2; if (f == "inf") { if (1 - cdf(x - 1, m) < objetivo) lo = m; else hi = m }
        else { if (cdf(x, m) > objetivo) lo = m; else hi = m } } return (lo + hi) / 2 }
    BEGIN { a = 0.05
      inf = (k == 0) ? 0 : biseca("inf", a / 2, k)
      sup = (k == n) ? 1 : biseca("sup", a / 2, k)
      printf "%d/%d = %.3f  IC95 Clopper-Pearson [%.3f; %.3f]\n", k, n, (n ? k / n : 0), inf, sup }'
}

# ---------------------------------------------------------------- entrada
accion="${1:-build}"; shift || true
case "$accion" in
  _concedido)  # interno: corre ya con el turno concedido; anota la hora de concesión
    marca="$1"; shift; ahora > "$marca"; exec "$@" ;;
  _fase_build) fase_build ;;
  _fase_modelo) fase_modelo "$1" "$(cat "$2" 2>/dev/null || ahora)" ;;
  build)
    volumenes || exit 2
    turno build build "$0" _fase_build ;;
  fase)
    f="${1:?fase: 3a | lan | 3b}"
    case "$f" in 3a|3b|lan) ;; *) decir "fase desconocida: $f (3a | lan | 3b)"; exit 2 ;; esac
    rec="$REC_PROPIO"; [ "$f" = lan ] && rec="$REC_COMPARTIDO"
    marca_fase="$(mktemp "$RLLM_DIR/.fase.XXXXXX")"
    turno "$rec" "$f" sh -c 'date -u +%Y-%m-%dT%H:%M:%SZ > "$1"; shift; exec "$@"' _ "$marca_fase" "$0" _fase_modelo "$f" "$marca_fase"
    rc=$?; rm -f "$marca_fase"; exit $rc ;;
  correr)
    dir="${1:?correr: carpeta del ejemplo, relativa a Examples/}"; dir="${dir%/}"; dir="${dir#Examples/}"
    [ -f "$EX/$dir/ejemplo.json" ] || { decir "$dir no tiene ejemplo.json"; exit 2; }
    if [ "${2:-}" = "--con-modelo" ]; then
      rec="$(jq -r .recurso "$EX/$dir/ejemplo.json")"
      case "$rec" in
        "$REC_COMPARTIDO") turno "$REC_COMPARTIDO" "correr $dir" sh -c "\"$0\" _uno lan \"$dir\"" ;;
        "$REC_PROPIO") turno "$REC_PROPIO" "correr $dir" sh -c "\"$0\" _uno propio \"$dir\"" ;;
        *) decir "$dir no usa modelo (recurso $rec)"; exit 0 ;;
      esac
    else
      volumenes || exit 2
      entradas_congeladas "$dir" || exit 1
      csp="$(cd "$EX" && ls "$dir"/*.csproj | head -1)"
      turno build "correr $dir" "$0" _sdk_correr "$csp"
    fi ;;
  _sdk_correr)  # <csproj> [args...]: compila y corre sin red; sin args, --fake --verificar
    csp="$1"; shift; [ $# -gt 0 ] || set -- --fake --verificar
    sdk -e OLLAMA_ENDPOINT="$PUERTO_CERRADO" -e OLLAMA_PROPIO_ENDPOINT="$PUERTO_CERRADO" \
        -e OLLAMA_EMBED_ENDPOINT="$PUERTO_CERRADO" -e RLLM_MAQUINA=guion -- sh -ec \
      'c="$1"; shift; dotnet build "$c" -warnaserror --nologo -v q -clp:NoSummary && dotnet run --no-build --project "$c" -- "$@"' _ "$csp" "$@" ;;
  _via_seco)  # <vía> <carpeta...>: dentro del turno build
    v="$1"; shift; rc=0
    for d in "$@"; do
      mapfile -t args < <(args_de_via "$d" "$v")
      csp="$(cd "$EX" && ls "$d"/*.csproj | head -1)"
      decir "$d · vía $v en seco: --fake ${args[*]}"
      "$0" _sdk_correr "$csp" --fake "${args[@]}" || { decir "$d · vía $v en seco: FALLA"; rc=1; }
    done
    exit $rc ;;
  _via_uno)  # <lan|propio> <carpeta> <vía>: dentro del turno del recurso
    if [ "$1" = lan ]; then num_ctx_compartido || exit $?
      restante="$(tope_restante)"; cabe_en_cola "$2" "$3" "$restante" || exit 0
      correr_via_con_modelo "$2" "$3" "$restante"; rc=$?
      descargar "$OLLAMA_ENDPOINT" "${OLLAMA_MODEL:-qwen3.5:4b}"
    else propio || exit $?; correr_via_con_modelo "$2" "$3"; rc=$?
      descargar "$OLLAMA_PROPIO_ENDPOINT" "${OLLAMA_PROPIO_MODEL:-qwen3.5:4b}" "${OLLAMA_EMBED_MODEL:-qwen3-embedding:0.6b}"; fi
    exit $rc ;;
  _precondicion_compartido)  # interno y de prueba (ArnesTests): sólo la precondición de M-K, sin medir
    precondicion_compartido ;;
  extension|embeddings|estabilidad) via "$accion" "$@" ;;
  medidor) medidor ;;
  _uno)
    if [ "$1" = lan ]; then num_ctx_compartido || exit $?
      correr_con_modelo "$2" "$(tope_restante)"; rc=$?
      descargar "$OLLAMA_ENDPOINT" "${OLLAMA_MODEL:-qwen3.5:4b}"
    else propio || exit $?; correr_con_modelo "$2"; rc=$?
      descargar "$OLLAMA_PROPIO_ENDPOINT" "${OLLAMA_PROPIO_MODEL:-qwen3.5:4b}" "${OLLAMA_EMBED_MODEL:-qwen3-embedding:0.6b}"; fi
    exit $rc ;;
  --con-modelo)
    if [ -n "${OLLAMA_PROPIO_SALUD_CMD:-}" ] && ! $OLLAMA_PROPIO_SALUD_CMD >/dev/null 2>&1; then
      export EMB_PLAN=EMB-C
      jq -nc --arg f "$(ahora)" '{tipo:"emb-plan", fecha:$f, emb_plan:"EMB-C", motivo:"salud del Ollama propio falló antes del primer turno build"}' >> "$TURNOS"
      decir "salud del Ollama propio falló: rige EMB-C (registrado en turnos.jsonl)"
    fi
    "$0" build || exit 1
    rc=0
    if [ "${EMB_PLAN:-EMB-B}" != EMB-C ]; then "$0" fase 3a || rc=1; fi
    "$0" fase lan || rc=1
    if [ "${EMB_PLAN:-EMB-B}" != EMB-C ]; then "$0" fase 3b || rc=1; fi
    exit $rc ;;
  tasa) tasa "${1:?k}" "${2:?m}" ;;
  -h|--help|ayuda) sed -n '2,/^set -uo/p' "$0" | sed '$d' ;;
  *) decir "acción desconocida: $accion (ver --help)"; exit 2 ;;
esac
