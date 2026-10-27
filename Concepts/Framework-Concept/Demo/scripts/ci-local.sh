#!/usr/bin/env bash
# Equivalente local de la compuerta del PR: .github/workflows/framework-concept-demo.yml (en la raíz del
# repositorio Lab-Documentos). Corre los MISMOS pasos, en el MISMO orden:
#
#   0. base del PR      copia de la demo en la rama de destino (lo publicado), para comparar contrato y definiciones
#   1. build            FrameworkConcept.Demo.SinMaui.slnf, avisos como error (CI=true)
#   2. Core.Tests       dominio, reglas portadas y contrato
#   3. Update.Tests     actualizador
#   4. Gate.Tests       instantánea del contrato, definiciones, catálogo frente al manifiesto, contenido sin AOT
#   5. contenido        dotnet publish del contenido Blazor + «empaquetar» (zip plano con manifiesto)
#   6. APK              el cascarón MAUI para android-arm64
#
# Uso, desde cualquier carpeta:
#   scripts/ci-local.sh                  # todo; la base es origin/main
#   scripts/ci-local.sh --hasta 4        # sólo los pasos 0 a 4 (lo rápido, sin publicar ni APK)
#   FC_BASE_REF=main scripts/ci-local.sh # otra rama de destino
#   FC_GATE_BASE=/ruta/a/otra/demo scripts/ci-local.sh   # una base armada a mano (así se provocan rojos)
#
# Si la máquina no tiene dotnet, cada paso corre dentro del contenedor de compilación (scripts/contenedor.sh).
# El paso 0 usa git en la máquina: el contenedor sólo ve la carpeta de la demo, no el repositorio.
# Las salidas quedan en salida/compuerta/ (excluida por .gitignore) y se pueden borrar.
set -euo pipefail

DEMO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SALIDA_REL="salida/compuerta"
HASTA=6

while [ "$#" -gt 0 ]; do
  case "$1" in
    --hasta) HASTA="$2"; shift 2 ;;
    --pasos) shift; MODO_PASOS=1 ;;   # uso interno: ya estamos donde está dotnet
    *) echo "opción desconocida: $1" >&2; exit 2 ;;
  esac
done

titulo() { printf '\n=== %s ===\n' "$*"; }

# --------------------------------------------------------------------------------------------------------
# Pasos 1 a 6. Corren con la demo como carpeta actual (en el host o en /work dentro del contenedor).
# --------------------------------------------------------------------------------------------------------
pasos() {
  cd "$DEMO"
  export CI=true                       # avisos como error (Directory.Build.props) y base obligatoria en Gate.Tests
  export DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1

  titulo "Cadena de herramientas (global.json fija el SDK con rollForward latestPatch)"
  dotnet --version

  titulo "1. Build sin el cascarón MAUI, avisos como error"
  dotnet build FrameworkConcept.Demo.SinMaui.slnf -c Release

  titulo "2. Core.Tests"
  dotnet test tests/FrameworkConcept.Core.Tests -c Release --no-build

  titulo "3. Update.Tests"
  dotnet test tests/FrameworkConcept.Update.Tests -c Release --no-build

  titulo "4. Gate.Tests (FC_GATE_BASE=${FC_GATE_BASE:-})"
  dotnet test tests/FrameworkConcept.Gate.Tests -c Release --no-build
  [ "$HASTA" -le 4 ] && return 0

  titulo "5. Publish del contenido y empaquetado"
  rm -rf "$SALIDA_REL/publicado" "$SALIDA_REL/canal"
  dotnet publish src/FrameworkConcept.Content -c Release -o "$SALIDA_REL/publicado"
  local version commit
  version="$(sed -n 's/^ *"version": *"\([^"]*\)".*/\1/p' versionado/contenido.json | head -n 1)"
  commit="${FC_COMMIT:-local}"
  dotnet run --project tools/FrameworkConcept.Tools -c Release --no-build -- empaquetar \
    --raiz "$SALIDA_REL/canal" --publicado "$SALIDA_REL/publicado" --version "$version" --commit "$commit"
  [ "$HASTA" -le 5 ] && return 0

  titulo "6. APK del cascarón (android-arm64)"
  # Como el workflow: CI=true también rige acá, así que un aviso del cascarón es un error.
  dotnet build src/FrameworkConcept.Shell -c Debug -p:RuntimeIdentifier=android-arm64 -p:AndroidPackageFormat=apk
  ls -l src/FrameworkConcept.Shell/bin/Debug/net10.0-android/android-arm64/*-Signed.apk
}

if [ "${MODO_PASOS:-0}" = 1 ]; then
  pasos
  exit 0
fi

# --------------------------------------------------------------------------------------------------------
# Paso 0: la base del PR. Se extrae la demo tal como está en FC_BASE_REF (por defecto origin/main) a
# salida/compuerta/base. Si la rama de destino todavía no tiene la demo, la carpeta queda vacía y las
# pruebas lo informan («nada publicado con qué comparar»).
# --------------------------------------------------------------------------------------------------------
titulo "0. Base del PR"
if [ -n "${FC_GATE_BASE:-}" ]; then
  echo "FC_GATE_BASE dada a mano: $FC_GATE_BASE"
else
  BASE_REL="$SALIDA_REL/base"
  rm -rf "${DEMO:?}/$BASE_REL" && mkdir -p "$DEMO/$BASE_REL"
  REF="${FC_BASE_REF:-origin/main}"
  if RAIZ_GIT="$(git -C "$DEMO" rev-parse --show-toplevel 2>/dev/null)"; then
    PREFIJO="$(git -C "$DEMO" rev-parse --show-prefix)"; PREFIJO="${PREFIJO%/}"
    # git archive toma las rutas relativas a la carpeta actual: se corre desde la raíz del repositorio.
    if git -C "$RAIZ_GIT" cat-file -e "$REF:$PREFIJO" 2>/dev/null; then
      NIVELES="$(printf '%s' "$PREFIJO" | awk -F/ '{print NF}')"
      git -C "$RAIZ_GIT" archive "$REF" "$PREFIJO" | tar -x -C "$DEMO/$BASE_REL" --strip-components="$NIVELES"
      echo "base: $REF:$PREFIJO → $BASE_REL"
    else
      echo "base: $REF no tiene $PREFIJO (la demo es nueva en esta rama) → base vacía"
    fi
  else
    echo "base: la demo no está en un repositorio git → base vacía"
  fi
  # Relativa a la demo: así vale igual en el host y dentro del contenedor.
  export FC_GATE_BASE="$BASE_REL"
fi

if command -v dotnet >/dev/null 2>&1; then
  HASTA="$HASTA" pasos
else
  # FC_GATE_BASE absoluta en el host no existe dentro del contenedor: se monta en /gate-base.
  EXTRA="-e FC_GATE_BASE=$FC_GATE_BASE -e FC_COMMIT=${FC_COMMIT:-local}"
  if [[ "$FC_GATE_BASE" = /* ]]; then
    EXTRA="-v $FC_GATE_BASE:/gate-base:ro -e FC_GATE_BASE=/gate-base -e FC_COMMIT=${FC_COMMIT:-local}"
  fi
  FC_DOCKER="${FC_DOCKER:-} $EXTRA" FC_NOMBRE="${FC_NOMBRE:-fc-ci-local-$$}" \
    "$DEMO/scripts/contenedor.sh" bash scripts/ci-local.sh --hasta "$HASTA" --pasos
fi
