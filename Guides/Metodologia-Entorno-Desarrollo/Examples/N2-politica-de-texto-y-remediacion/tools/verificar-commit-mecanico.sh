#!/usr/bin/env bash
# Comprueba que un commit de migración sea PURAMENTE mecánico: para cada archivo
# que toca, el texto antes y después es idéntico una vez decodificado (origen
# cp1252 si no era UTF-8), sin BOM y sin CR. Cualquier otra diferencia es un
# cambio funcional colado en el commit de normalización.
#   uso: tools/verificar-commit-mecanico.sh <commit> [codificacion-origen]
set -euo pipefail
C=${1:?commit}; ORIGEN=${2:-CP1252}
norm() { # stdin -> texto UTF-8 sin BOM ni CR
  local t; t=$(mktemp); cat > "$t"
  if ! iconv -f UTF-8 -t UTF-8 "$t" >/dev/null 2>&1; then iconv -f "$ORIGEN" -t UTF-8 "$t"; else cat "$t"; fi \
    | sed '1s/^\xef\xbb\xbf//' | tr -d '\r'; rm -f "$t"; }
fallas=0; total=0
while IFS= read -r f; do
  total=$((total+1))
  if [ "$(git show "$C^:$f" | norm | sha256sum)" = "$(git show "$C:$f" | norm | sha256sum)" ]; then
    echo "  mecanico   $f"
  else
    echo "  CONTENIDO  $f"; fallas=$((fallas+1))
  fi
done < <(git diff-tree --no-commit-id --name-only -r --diff-filter=M "$C")
nuevos=$(git diff-tree --no-commit-id --name-only -r --diff-filter=ADR "$C" | wc -l)
echo "verificados $total archivo(s); $fallas con cambio de contenido; $nuevos alta/baja/renombre"
[ "$fallas" -eq 0 ] && [ "$nuevos" -eq 0 ]
