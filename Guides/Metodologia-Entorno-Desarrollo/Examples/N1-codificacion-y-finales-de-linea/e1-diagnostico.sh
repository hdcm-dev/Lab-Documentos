#!/bin/sh
# E1-N1-09 — Diagnosticar la codificación real y el EOL de un archivo, sin abrirlo en un editor.
# Se corre DENTRO del contenedor del SDK (iconv, od y git vienen en la imagen).
# `file` es opcional: si está instalado se usa (en el host suele estarlo).
set -u
cd "$(dirname "$0")/work" || exit 1
diag () {  # $1 = archivo
  f="$1"
  bom=$(head -c3 "$f" | od -An -tx1 | tr -d ' \n')
  [ "$bom" = "efbbbf" ] && b="BOM UTF-8" || b="sin BOM"
  if iconv -f UTF-8 -t UTF-8 "$f" >/dev/null 2>&1; then u="UTF-8 válido"; else u="NO es UTF-8"; fi
  if LC_ALL=C grep -q "$(printf '[\200-\377]')" "$f"; then a="tiene bytes >0x7F"; else a="ASCII puro"; fi
  crlf=$(tr -cd '\r' < "$f" | wc -c | tr -d ' ')
  printf '%-34s %-10s %-14s %-18s CR=%s' "$f" "$b" "$u" "$a" "$crlf"
  command -v file >/dev/null 2>&1 && printf ' | file: %s' "$(file -b "$f")"
  echo
}
for f in dotnet/cp1252-lf/Mensajes.cs dotnet/utf8-lf/Mensajes.cs dotnet/utf8bom-lf/Mensajes.cs \
         dotnet/utf8-crlf/Mensajes.cs dotnet/cp1252-lf/Program.cs; do diag "$f"; done

echo "== Lectura clave: 'NO es UTF-8' + 'tiene bytes >0x7F' => probablemente cp1252/Latin-1."
echo "   Confirmar mirando los bytes de un carácter conocido (ó: cp1252=f3, UTF-8=c3 b3):"
LC_ALL=C grep -ao 'Atenci.\{1,2\}n' dotnet/cp1252-lf/Mensajes.cs | od -An -tx1 | head -1
echo "== Búsqueda masiva: todo .cs/.razor/.js/.py del árbol que NO sea UTF-8 válido"
find . -type f \( -name '*.cs' -o -name '*.razor' -o -name '*.js' -o -name '*.py' \) | sort | while read -r f; do
  iconv -f UTF-8 -t UTF-8 "$f" >/dev/null 2>&1 || echo "   no-UTF-8: $f"
done

echo "== Shell: un BOM o un CRLF rompen el shebang/los comandos"
cd shell && chmod +x ./*.sh 2>/dev/null  # en el host work/ es de root: si ya tienen +x alcanza
for s in sin-bom.sh con-bom.sh crlf.sh; do printf '   ./%-11s -> ' "$s"; ./"$s" 2>&1 | head -1 | cat -A | cut -c1-70; done
