#!/bin/sh
# E1-N1-07 — open() sin encoding= depende del locale (Python < 3.15, sin modo UTF-8).
# ascii() evita que la consola re-codifique la salida y confunda la lectura.
# Instala un locale ISO-8859-1 real dentro del contenedor (necesita red para apt).
#   docker run --rm -v "$PWD":/w -w /w python:3.12-slim sh e1-python-locale.sh
set -u
apt-get update -qq >/dev/null && apt-get install -y -qq locales >/dev/null 2>&1
localedef -i es_AR -f ISO-8859-1 es_AR.ISO-8859-1 2>/dev/null
cd "$(dirname "$0")/work/python" || exit 1
leer='import sys,locale;f=open("mod_utf8.py");print("open() usa", f.encoding, "| utf8_mode", sys.flags.utf8_mode);print("  lee:", ascii(f.readline().strip()[9:26]))'
echo "== $(python -V), LC_ALL=es_AR.ISO-8859-1 (locale existente)"
LC_ALL=es_AR.ISO-8859-1 python -I -c "$leer"
echo "== mismo locale, modo UTF-8 forzado (-X utf8; equivale a PYTHONUTF8=1, que -I ignora)"
LC_ALL=es_AR.ISO-8859-1 python -I -X utf8 -c "$leer"
echo "== mismo locale, encoding='utf-8' explícito (independiente del entorno)"
LC_ALL=es_AR.ISO-8859-1 python -I -c 'print("  lee:", ascii(open("mod_utf8.py", encoding="utf-8").readline().strip()[9:26]))'
