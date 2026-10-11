#!/usr/bin/env bash
# Migración mecánica de texto heredado a la política declarada en el repo.
#   uso: tools/migrar-a-utf8.sh <codificacion-origen> [pathspec...]
#   ej.: tools/migrar-a-utf8.sh CP1252 '*.cs' '*.razor'
# Se corre desde la raíz del repositorio, sin cambios en stage y con
# .editorconfig/.gitattributes YA commiteados (la migración obedece a la
# política, no la inventa). No commitea: deja todo en stage para revisar.
# Nota: apenas se commitea un .gitattributes nuevo, `git status` puede mostrar
# como modificados archivos que nadie tocó (su CRLF del índice ya no coincide
# con la política). Es el síntoma esperado de que falta esta renormalización.
set -euo pipefail
ORIGEN=${1:?codificacion de origen, p. ej. CP1252}; shift
git diff --cached --quiet || { echo "hay cambios en stage: abortando" >&2; exit 2; }
for p in .editorconfig .gitattributes; do
  git cat-file -e "HEAD:$p" 2>/dev/null && git diff --quiet HEAD -- "$p" \
    || { echo "$p debe estar commiteado y sin cambios antes de migrar" >&2; exit 2; }
done
BOM=$'\xef\xbb\xbf'
convertidos=0; bom_puestos=0; bom_quitados=0
while IFS= read -r -d '' f; do
  # Saltar lo que git no considera texto (binarios, -text) y los vacíos.
  case "$(git ls-files --eol -- "$f" | awk '{print $1}')" in i/-text|i/none|"") continue;; esac
  # 1) Codificación: sólo se convierte lo que NO es UTF-8 válido.
  #    Límite conocido: un archivo cp1252 que casualmente sea UTF-8 válido
  #    (p. ej. contiene "Ã©") no se detecta; revisar el diff igual.
  if ! iconv -f UTF-8 -t UTF-8 "$f" >/dev/null 2>&1; then
    tmp=$(mktemp); iconv -f "$ORIGEN" -t UTF-8 "$f" > "$tmp"; cat "$tmp" > "$f"; rm -f "$tmp"
    echo "convertido $ORIGEN->UTF-8: $f"; convertidos=$((convertidos+1))
  fi
  # 2) BOM: lo que diga .editorconfig para esa ruta (editorconfig-core-py).
  charset=$(editorconfig "$PWD/$f" | sed -n 's/^charset=//p')
  primeros=$(head -c3 "$f")
  if [ "$charset" = "utf-8-bom" ] && [ "$primeros" != "$BOM" ]; then
    tmp=$(mktemp); { printf '%s' "$BOM"; cat "$f"; } > "$tmp"; cat "$tmp" > "$f"; rm -f "$tmp"
    echo "BOM agregado: $f"; bom_puestos=$((bom_puestos+1))
  elif [ "$charset" = "utf-8" ] && [ "$primeros" = "$BOM" ]; then
    tmp=$(mktemp); tail -c +4 "$f" > "$tmp"; cat "$tmp" > "$f"; rm -f "$tmp"
    echo "BOM quitado: $f"; bom_quitados=$((bom_quitados+1))
  fi
done < <(git ls-files -z -- "$@")
# 3) Fin de línea: lo resuelve git según .gitattributes (CRLF -> LF en el índice).
git add --renormalize .
git add -u
echo "resumen: $convertidos convertidos, $bom_puestos BOM agregados, $bom_quitados BOM quitados"
git status --short
