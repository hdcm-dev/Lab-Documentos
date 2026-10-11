#!/bin/sh
# E1-N1-13 — ¿Distingue `file` cp1252 de Latin-1? Depende de si aparecen bytes 0x80-0x9F.
# Se corre en el HOST (necesita `file`); escribe en un directorio temporal y lo borra.
#   sh e1-file.sh
set -u
command -v file >/dev/null 2>&1 || { echo "falta 'file'"; exit 1; }
d=$(mktemp -d) || exit 1
file --version | head -1
printf 'atenci\363n se\361al\n'                > "$d/sin-80-9F.txt"       # ó ñ: iguales en cp1252 y Latin-1
printf 'Precio: 10 \200 atenci\363n\n'         > "$d/con-euro-0x80.txt"   # € = 0x80 sólo existe en cp1252
printf 'Dijo \223hola\224\n'                   > "$d/con-comillas-0x93.txt" # “ ” = 0x93/0x94 sólo en cp1252
# Mojibake: cp1252 cuyos bytes altos forman por casualidad UTF-8 válido (C3 B3 = "ó" en UTF-8,
# "Ã³" en cp1252). `file` lo informa como UTF-8: no es un detector de cp1252.
printf 'acentuaci\303\263n\n'                > "$d/moji.txt"
for f in sin-80-9F.txt con-euro-0x80.txt con-comillas-0x93.txt moji.txt; do
  printf '  %-24s -> %s\n' "$f" "$(file -b "$d/$f")"
done
rm -rf "$d"
