#!/usr/bin/env bash
# registrar-entorno.sh <dir-del-repo>
# Emite en stdout un manifiesto JSON con UNA ENTRADA POR CAPA del modelo N0.
# No es una atestación SLSA (no está firmado ni lo genera una plataforma de build):
# es el registro mínimo que permite comparar dos entornos y decir en qué capa difieren.
# Variables opcionales que sólo el llamador conoce: IMAGEN_REF, IMAGEN_DIGEST.
set -u
repo=${1:-.}
cd "$repo" || exit 2
h() { [ -f "$1" ] && sha256sum "$1" | cut -c1-16 || echo null; }
# v: versión de una herramienta; si el comando existe pero falla (p. ej. global.json no satisfecho)
# se registra el fallo, nunca una línea suelta de la salida de error.
v() { command -v "$1" >/dev/null 2>&1 || { echo null; return; }
      local out; if out=$("$@" 2>/dev/null); then echo "$out" | head -1; else echo "ERROR: '$*' falló (código $?)"; fi; }
q() { [ "$1" = null ] && echo null || printf '"%s"' "$1"; }
commit=$(git rev-parse HEAD 2>/dev/null || echo null)
sucio=$( [ -n "$(git status --porcelain 2>/dev/null)" ] && echo true || echo false )
locks=$(find . -name packages.lock.json -o -name package-lock.json -o -name uv.lock -o -name pylock.toml 2>/dev/null \
        | grep -v node_modules | sort | while read -r f; do printf '"%s":"%s",' "$f" "$(h "$f")"; done)
cat <<JSON
{
  "capa1_fuente":      { "commit": $(q "$commit"), "arbol_con_cambios_sin_commit": $sucio,
                         "gitattributes": $(q "$(h .gitattributes)"), "editorconfig": $(q "$(h .editorconfig)") },
  "capa2_toolchain":   { "dotnet_sdk": $(q "$(v dotnet --version)"), "node": $(q "$(v node --version)"),
                         "npm": $(q "$(v npm --version)"), "python": $(q "$(v python3 --version)"),
                         "uv": $(q "$(v uv --version)"), "global_json": $(q "$(h global.json)") },
  "capa3_grafo":       { "lock_files": { ${locks%,} } },
  "capa4_so_imagen":   { "os": $(q "$(. /etc/os-release 2>/dev/null; echo "${PRETTY_NAME:-null}")"), "arquitectura": $(q "$(uname -m)"),
                         "imagen_ref": $(q "${IMAGEN_REF:-null}"), "imagen_digest": $(q "${IMAGEN_DIGEST:-null}") },
  "capa5_proceso":     { "LANG": $(q "${LANG:-null}"), "TZ": $(q "${TZ:-null}"),
                         "SOURCE_DATE_EPOCH": $(q "${SOURCE_DATE_EPOCH:-null}"), "ruta_de_trabajo": $(q "$PWD") },
  "capa6_registro":    { "registrado_utc": "$(date -u +%Y-%m-%dT%H:%M:%SZ)" }
}
JSON
