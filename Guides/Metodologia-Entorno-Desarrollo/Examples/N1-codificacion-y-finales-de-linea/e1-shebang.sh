#!/bin/sh
# E1-N1-11 — Qué pasa exactamente cuando un script tiene BOM o CRLF, según QUIÉN lo ejecuta:
# el shell que lo invoca (dash, bash) o un execve directo sin shell (Python subprocess con shell=False,
# es decir, cualquier programa que lance el archivo con execve sin pasar por un shell).
#   docker run --rm -v "$PWD":/w -w /w python:3.12-slim sh e1-shebang.sh
set -u
T=/tmp/shebang; rm -rf "$T"; mkdir -p "$T"
cp "$(dirname "$0")/work/shell/"*.sh "$T"/ && chmod +x "$T"/*.sh && cd "$T" || exit 1
for s in sin-bom.sh con-bom.sh crlf.sh; do
  echo "== $s"
  printf '   dash : '; dash -c "./$s" 2>&1 | cat -A | tr '\n' ' '; echo "(exit $(dash -c "./$s" >/dev/null 2>&1; echo $?))"
  printf '   bash : '; bash -c "./$s" 2>&1 | cat -A | tr '\n' ' '; echo "(exit $(bash -c "./$s" >/dev/null 2>&1; echo $?))"
  printf '   execve sin shell (subprocess, shell=False): '
  python3 -I -c 'import subprocess,sys
try: r=subprocess.run(["./"+sys.argv[1]],capture_output=True,text=True); print(repr(r.stdout), "exit", r.returncode)
except OSError as e: print(type(e).__name__, e.errno, e.strerror)' "$s"
done
