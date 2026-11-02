#!/usr/bin/env bash
# demo.sh — cómo se compila, se prueba y se corre la demo (plan §F.8, §F.9). Corre en el HOST con
# bash, docker y jq (R-HOST); todo lo de .NET va en contenedores mcr.microsoft.com/dotnet/sdk:10.0.
# Cada acción con recurso pide su turno por separado y ninguna se anida en otra.
#
#   tools/demo.sh build                 restore y build -warnaserror (turno build)
#   tools/demo.sh pruebas               build + Cableado/ y las pruebas del caso base, sin servidor de
#                                       modelos (Category!=LLM) + AC-6 sobre los paquetes (turno build)
#   tools/demo.sh embeddings            fase 3a: vectoriza el corpus y la pregunta de PD-2 con el Ollama
#                                       propio y los persiste en RLLM_DIR/embeddings/ (turno modelo-propio)
#   tools/demo.sh humo                  humo con modelo de PD-2, PD-3 y PD-6 contra el servidor de chat,
#                                       con los embeddings persistidos (turno modelo-compartido)
#   tools/demo.sh host [guion|modelo]   levanta el host en un contenedor, publicado en 127.0.0.1:8080
#   tools/demo.sh parar                 baja el contenedor del host
#
# Variables (ninguna tiene valor en el repo):
#   TURNO_CMD  envoltorio de turno, «$TURNO_CMD <recurso> -- <comando>»; vacía: corre sin turno y lo avisa.
#              Los recursos son papeles (build, modelo-propio, modelo-compartido): el envoltorio los traduce.
#   RLLM_DIR   registro R-LLM. En embeddings y humo es OBLIGATORIA y va fuera del árbol de la guía; en
#              build y pruebas, el defecto es ${TMPDIR:-/tmp}/guia-ia-registros-demo.
#   Con modelo (embeddings, humo) no tienen defecto; si falta una, sale con 2 sin pedir turno ni llamar:
#     OLLAMA_SERVIDOR      compartido | propio | nube (campo «servidor» del registro).
#     RLLM_MAQUINA         rol de la máquina del servidor de chat (p. ej. «estacion-1»), nunca un equipo.
#     RLLM_MAQUINA_PROPIO  rol de la máquina del Ollama propio (p. ej. «estacion-2»), nunca un equipo.
#   OLLAMA_ENDPOINT, OLLAMA_MODEL            servidor de chat del humo.
#   OLLAMA_PROPIO_ENDPOINT | OLLAMA_PROPIO_URL_CMD   Ollama propio de la fase de embeddings; el comando
#              se ejecuta DENTRO del turno y su salida es el endpoint (no se escribe en ningún archivo).
#   VOLUMEN_NUGET, VOLUMEN_ARTEFACTOS_DEMO, CONTENEDOR, ETIQUETA   volúmenes, contenedor del host y etiqueta
#              de docker (defaults neutros: nuget, artefactos-demo, asistente, proyecto=guia-ia).
#   PUERTO_HOST      puerto de loopback del host (default 8080).
#   HUMO_PASO        repite un solo paso del humo (Pd2, Pd3 o Pd6); vacío, los tres.
set -uo pipefail

AQUI="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEMO="$(dirname "$AQUI")"
GUIA="$(cd "$DEMO/../.." && pwd)"
REL="Demos/$(basename "$DEMO")"
IMG="${SDK_IMAGEN:-mcr.microsoft.com/dotnet/sdk:10.0}"
PUERTO_CERRADO="http://127.0.0.1:9"
VOLUMEN_NUGET="${VOLUMEN_NUGET:-nuget}"
VOL_ART="${VOLUMEN_ARTEFACTOS_DEMO:-artefactos-demo}"
CONTENEDOR="${CONTENEDOR:-asistente}"
ETIQUETA="${ETIQUETA:-proyecto=guia-ia}"
NUM_CTX=8192   # REC-08: el mismo num_ctx que el arnés de Examples/
TESTS="tests/MovilidadUrbana.Asistente.Tests/MovilidadUrbana.Asistente.Tests.csproj"
APP="src/MovilidadUrbana.Asistente/MovilidadUrbana.Asistente.csproj"

decir() { printf '[demo] %s\n' "$*" >&2; }

# Con modelo (M-A, ENTO-19): sin defecto, con mensaje, antes de pedir el turno y sin llamar a nadie.
case "${1:-pruebas}" in
  embeddings|humo)
    falta=(); for v in OLLAMA_SERVIDOR RLLM_MAQUINA RLLM_MAQUINA_PROPIO RLLM_DIR; do [ -n "${!v:-}" ] || falta+=("$v"); done
    if [ ${#falta[@]} -gt 0 ]; then decir "modo con modelo sin ${falta[*]}: no tienen defecto (M-A); se aborta sin pedir turno ni llamar a ningún servidor"; exit 2; fi
    case "$OLLAMA_SERVIDOR" in compartido|propio|nube) ;; *) decir "OLLAMA_SERVIDOR inválido: «$OLLAMA_SERVIDOR»"; exit 2 ;; esac
    case "$(realpath -m "$RLLM_DIR")/" in "$GUIA"/*) decir "RLLM_DIR queda dentro de la carpeta de la guía: tiene que estar fuera del árbol"; exit 2 ;; esac ;;
  *) RLLM_DIR="${RLLM_DIR:-${TMPDIR:-/tmp}/guia-ia-registros-demo}" ;;
esac
case "${1:-pruebas}" in -h|--help) ;; *) mkdir -p "$RLLM_DIR"; RLLM_DIR="$(cd "$RLLM_DIR" && pwd)" ;; esac

volumenes() {
  for v in "$VOLUMEN_NUGET" "$VOL_ART"; do
    docker volume inspect "$v" >/dev/null 2>&1 || docker volume create --label "$ETIQUETA" "$v" >/dev/null
  done
  docker run --rm --label "$ETIQUETA" -v "$VOLUMEN_NUGET":/nuget -v "$VOL_ART":/artefactos "$IMG" \
    sh -c 'chown 1000:1000 /nuget /artefactos' >/dev/null
}

# sdk [args de docker] -- comando: contenedor efímero del SDK con la guía montada (la demo referencia
# Examples/_Comun por ruta relativa). Las variables se pasan por nombre, nunca con su valor en un archivo.
sdk() {
  local extra=()
  while [ $# -gt 0 ] && [ "$1" != "--" ]; do extra+=("$1"); shift; done
  shift
  docker run --rm --label "$ETIQUETA" -u 1000:1000 \
    -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 -e NUGET_PACKAGES=/nuget \
    -e IA_LAYER_ARTEFACTOS=/artefactos -e RLLM_DIR=/registros -e RLLM_TURNO_ID \
    -v "$VOLUMEN_NUGET":/nuget -v "$VOL_ART":/artefactos -v "$GUIA":/guia -v "$RLLM_DIR":/registros \
    -w "/guia/$REL" "${extra[@]}" "$IMG" "$@"
}

http() { docker run --rm --label "$ETIQUETA" "$IMG" curl -sS --max-time "${HTTP_MAX:-15}" "$@"; }

turno() {
  local recurso="$1"; shift
  export RLLM_TURNO_ID="$(date -u +%Y%m%dT%H%M%S)-demo-$recurso-$$"
  if [ -z "${TURNO_CMD:-}" ]; then decir "TURNO_CMD vacío: corre SIN turno (válido para un lector con su propio servidor)"; "$@"
  else
    # shellcheck disable=SC2086
    $TURNO_CMD "$recurso" -- "$@"
  fi
}

entradas_congeladas() {
  local ruta sha real ok=0
  while IFS=$'\t' read -r ruta sha; do
    real="$(sha256sum "$DEMO/$ruta" | cut -d' ' -f1)"
    [ "$real" = "$sha" ] || { decir "$ruta no coincide con su SHA-256 congelado"; ok=1; }
  done < <(jq -r '.entradas[] | [.ruta, .sha256] | @tsv' "$DEMO/ejemplo.json")
  return $ok
}

_build() {
  sdk -- sh -ec "echo '== SDK'; dotnet --version
    echo '== restore'; dotnet restore MovilidadUrbana.Asistente.slnx --nologo -v q
    echo '== build -warnaserror'; dotnet build MovilidadUrbana.Asistente.slnx --no-restore -warnaserror --nologo -v q -clp:NoSummary
    echo '== build: ok'"
}

_pruebas() {
  _build || return 1
  sdk -e OLLAMA_ENDPOINT="$PUERTO_CERRADO" -e OLLAMA_PROPIO_ENDPOINT="$PUERTO_CERRADO" -e OLLAMA_EMBED_ENDPOINT="$PUERTO_CERRADO" -- sh -ec "
    echo '== test sin servidor de modelos (Category!=LLM)'
    dotnet test $TESTS --no-build --nologo --filter 'Category!=LLM' --logger 'console;verbosity=normal'
    echo '== paquetes (AC-6)'; rm -rf /registros/paquetes-demo; mkdir -p /registros/paquetes-demo
    dotnet list $APP package --include-transitive --format json > /registros/paquetes-demo/MovilidadUrbana.Asistente.json
    dotnet list $TESTS package --include-transitive --format json > /registros/paquetes-demo/MovilidadUrbana.Asistente.Tests.json" || return 1
  ac6
}

ac6() {
  local salida
  salida="$(jq -rs '
    [ .[] | .projects[]? | .frameworks[]? | ((.topLevelPackages // []) + (.transitivePackages // []))[] | {id, v: .resolvedVersion} ] as $todos
    | ["Microsoft.Extensions.AI", "Microsoft.Extensions.VectorData", "Microsoft.Agents.AI", "ModelContextProtocol", "OpenTelemetry", "Microsoft.EntityFrameworkCore"] as $f
    | ( $f[] as $x | [ $todos[] | select(.id == $x or (.id | startswith($x + "."))) | .v ] | unique | select(length > 1) | "familia \($x) en varias versiones: \(join(", "))" ),
      ( $todos[] | select(.v | test("-(preview|rc|alpha|beta)")) | "preview: \(.id) \(.v)" )' "$RLLM_DIR"/paquetes-demo/*.json | sort -u)"
  if [ -n "$salida" ]; then decir "AC-6 falla:"; printf '%s\n' "$salida" >&2; return 1; fi
  decir "AC-6: familias en una sola versión, sin previews"
}

# Fase 3a: dentro del turno modelo-propio. Versión y digests del recurso de la corrida.
_embeddings() {
  if [ -n "${OLLAMA_PROPIO_URL_CMD:-}" ]; then OLLAMA_PROPIO_ENDPOINT="$($OLLAMA_PROPIO_URL_CMD)" || return 1; fi
  export OLLAMA_PROPIO_ENDPOINT="${OLLAMA_PROPIO_ENDPOINT:-http://localhost:11434}" OLLAMA_EMBED_ENDPOINT="${OLLAMA_PROPIO_ENDPOINT}"
  local v rc; v="$(http "$OLLAMA_PROPIO_ENDPOINT/api/version" | jq -r .version)" || return 1
  decir "Ollama propio $v"
  sdk -e OLLAMA_EMBED_ENDPOINT -e OLLAMA_EMBED_MODEL -e OLLAMA_PROPIO_ENDPOINT -e OLLAMA_SERVIDOR -e RLLM_MAQUINA="$RLLM_MAQUINA_PROPIO" -- \
    dotnet test $TESTS --no-build --nologo --filter 'FullyQualifiedName~ConModelo.EmbeddingsDePd2' --logger 'console;verbosity=normal'
  rc=$?
  descargar "$OLLAMA_PROPIO_ENDPOINT" "${OLLAMA_EMBED_MODEL:-qwen3-embedding:0.6b}" embed
  return $rc
}

# descargar <endpoint> <modelo> [embed]: keep_alive 0 al cerrar la fase (ENTO-10).
descargar() {
  if [ "${3:-}" = embed ]; then http -X POST "$1/api/embed" -d "{\"model\":\"$2\",\"input\":\"\",\"keep_alive\":0}" >/dev/null 2>&1
  else http -X POST "$1/api/generate" -d "{\"model\":\"$2\",\"keep_alive\":0}" >/dev/null 2>&1; fi
  decir "descargado $2 (keep_alive 0)"; return 0
}

# Humo: dentro del turno modelo-compartido. Nunca con otro modelo cargado (R-04); el 4b se carga con
# num_ctx 8192 dentro del turno y se lee /api/ps (REC-08): si no informa 8192, no se mide.
_humo() {
  local ep="${OLLAMA_ENDPOINT:?OLLAMA_ENDPOINT sin definir}" modelo="${OLLAMA_MODEL:-qwen3.5:4b}" ps otros ctx rc
  ps="$(http "$ep/api/ps")" || { decir "no responde /api/ps"; return 1; }
  otros="$(jq -r --arg m "$modelo" '[.models[]? | select(.name!=$m and .model!=$m) | .name] | join(", ")' <<<"$ps")"
  [ -z "$otros" ] || { decir "/api/ps lista otro modelo ($otros): se aborta sin llamar (R-04)"; return 3; }
  HTTP_MAX=900 http -X POST "$ep/api/generate" -d "{\"model\":\"$modelo\",\"prompt\":\"\",\"stream\":false,\"options\":{\"num_ctx\":$NUM_CTX}}" >/dev/null || return 1
  ctx="$(http "$ep/api/ps" | jq -r --arg m "$modelo" '[.models[]? | select(.name==$m or .model==$m) | .context_length][0] // empty')"
  [ "$ctx" = "$NUM_CTX" ] || { decir "/api/ps informa num_ctx «$ctx» tras cargar con $NUM_CTX: no se mide"; return 1; }
  export OLLAMA_NUM_CTX="$NUM_CTX" OLLAMA_NUM_CTX_ORIGEN=arnes-ps-tras-carga OLLAMA_MODEL="$modelo"
  decir "num_ctx = $ctx ($OLLAMA_NUM_CTX_ORIGEN)"
  [ -f "$RLLM_DIR/embeddings/PD-2.v1.json" ] || decir "sin embeddings persistidos: PD-2 se saltará (correr antes «embeddings»)"
  sdk -e OLLAMA_ENDPOINT -e OLLAMA_MODEL -e OLLAMA_SERVIDOR -e OLLAMA_NUM_CTX -e OLLAMA_NUM_CTX_ORIGEN \
      -e EMB_PERSISTIDOS=/registros/embeddings/PD-2.v1.json -e RLLM_MAQUINA -- \
    dotnet test $TESTS --no-build --nologo --filter "FullyQualifiedName~ConModelo.Humo${HUMO_PASO:+&Name~$HUMO_PASO}" --logger 'console;verbosity=normal'
  rc=$?
  descargar "$ep" "$modelo"
  return $rc
}

_host() {
  local modo="${1:-guion}" puerto="${PUERTO_HOST:-8080}"
  docker rm -f "$CONTENEDOR" >/dev/null 2>&1
  local env=(-e ASPNETCORE_URLS=http://+:8080 -e ASPNETCORE_ENVIRONMENT=Development -e "ConnectionStrings__BaseDeDatos=Data Source=/tmp/datos/movilidad.db")
  if [ "$modo" = guion ]; then env+=(-e ASISTENTE_MODO=guion -e EMB_PLAN=EMB-C)
  else env+=(-e OLLAMA_ENDPOINT -e OLLAMA_MODEL -e OLLAMA_SERVIDOR -e OLLAMA_NUM_CTX -e OLLAMA_NUM_CTX_ORIGEN -e OLLAMA_EMBED_ENDPOINT -e OLLAMA_EMBED_MODEL -e EMB_PLAN); fi
  docker run -d --name "$CONTENEDOR" --label "$ETIQUETA" -u 1000:1000 -p "127.0.0.1:$puerto:8080" \
    -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 -e NUGET_PACKAGES=/nuget -e IA_LAYER_ARTEFACTOS=/artefactos \
    "${env[@]}" -v "$VOLUMEN_NUGET":/nuget -v "$VOL_ART":/artefactos -v "$GUIA":/guia -w "/guia/$REL" "$IMG" \
    dotnet run --no-build --project "$APP" --no-launch-profile >/dev/null || return 1
  decir "host «$modo» en http://127.0.0.1:$puerto (contenedor $CONTENEDOR); «tools/demo.sh parar» lo baja"
}

accion="${1:-pruebas}"; shift || true
case "$accion" in
  build)      volumenes || exit 2; turno build "$0" _build ;;
  pruebas)    volumenes || exit 2; entradas_congeladas || exit 1; turno build "$0" _pruebas ;;
  embeddings) volumenes || exit 2; entradas_congeladas || exit 1; turno modelo-propio "$0" _embeddings ;;
  humo)       volumenes || exit 2; entradas_congeladas || exit 1; turno modelo-compartido "$0" _humo ;;
  host)       _host "$@" ;;
  parar)      docker rm -f "$CONTENEDOR" >/dev/null 2>&1; decir "host bajado" ;;
  _build) _build ;; _pruebas) _pruebas ;; _embeddings) _embeddings ;; _humo) _humo ;;
  -h|--help) sed -n '2,/^set -uo/p' "$0" | sed '$d' ;;
  *) decir "acción desconocida: $accion"; exit 2 ;;
esac
