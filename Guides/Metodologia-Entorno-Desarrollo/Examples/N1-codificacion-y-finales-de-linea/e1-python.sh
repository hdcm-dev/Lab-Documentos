#!/bin/sh
# E1-N1-06 — Cómo lee Python el código fuente (PEP 263/3120) y cómo abre archivos (PEP 540/597/686).
# Se corre DENTRO de un contenedor python (ver README.md). Requiere generar.py.
set -u
cd "$(dirname "$0")/work/python" || exit 1
echo "$(python -VV | head -1) | LANG=${LANG:-<vacío>} LC_ALL=${LC_ALL:-<vacío>}"
for m in mod_cp1252 mod_cp1252_declarado mod_utf8 mod_utf8bom mod_utf8_crlf; do
  printf '%-22s -> ' "$m.py"
  python -I "$m.py" 2>&1 | tail -1
done

echo "-- open() sin encoding= sobre un archivo UTF-8, según locale y modo UTF-8"
leer='import sys,locale;f=open("mod_utf8.py");print("open() usa", f.encoding, "| utf8_mode", sys.flags.utf8_mode, "| getpreferredencoding", locale.getpreferredencoding(False));print("  primera línea:", f.readline().strip()[:30])'
python -I -c "$leer"
echo "-- con locale C/POSIX, Python activa por sí solo el modo UTF-8 (PEP 540):"
LC_ALL=C python -I -c "$leer" 2>&1 | tail -2
echo "   (el caso de un locale no UTF-8 real está en e1-python-locale.sh)"
echo "-- aviso de encoding implícito (PEP 597): -X warn_default_encoding"
python -I -X warn_default_encoding -c 'open("mod_utf8.py").close()' 2>&1 | tail -1
echo "-- utf-8 vs utf-8-sig al leer un archivo con BOM"
python -I -c 'print("utf-8     ->", repr(open("mod_utf8bom.py",encoding="utf-8").read(1)));print("utf-8-sig ->", repr(open("mod_utf8bom.py",encoding="utf-8-sig").read(1)))'
echo "-- newline: modo texto traduce CRLF->LF al leer; newline=\"\" lo conserva"
python -I -c 'print("por defecto:", repr(open("mod_utf8_crlf.py",encoding="utf-8").readline()[-3:]));print("newline=\"\":", repr(open("mod_utf8_crlf.py",encoding="utf-8",newline="").readline()[-3:]))'
