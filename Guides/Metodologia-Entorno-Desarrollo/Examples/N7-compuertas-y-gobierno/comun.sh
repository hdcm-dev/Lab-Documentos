# Funciones comunes de los experimentos N7 (se incluye con `source`).
N7=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
preparar() {   # preparar <nombre-de-carpeta-de-trabajo>
  W=$N7/work/$1; rm -rf "$W"; mkdir -p "$W"
  export GIT_CONFIG_GLOBAL=$W/gitconfig GIT_CONFIG_NOSYSTEM=1
  git config --global user.name lab; git config --global user.email lab@example.invalid
  git config --global init.defaultBranch main; git config --global advice.detachedHead false
}
titulo() { echo; echo "=== $*"; }
rc() { "$@"; local r=$?; echo "   -> rc=$r"; return 0; }
codif() {      # codif <archivo>: dice si es UTF-8 válido y si tiene BOM (sin depender de `file`)
  python3 - "$1" <<'PY'
import sys
b = open(sys.argv[1], 'rb').read()
try:
    b.decode('utf-8'); v = 'UTF-8 válido'
except UnicodeDecodeError as e:
    v = f'NO es UTF-8 (byte 0x{b[e.start]:02x})'
bom = 'sí' if b.startswith(bytes([0xEF, 0xBB, 0xBF])) else 'no'
crlf = 'sí' if bytes([13, 10]) in b else 'no'
fin = 'sí' if b.endswith(bytes([10])) else 'no'
print(f"   {sys.argv[1]}: {v}, BOM={bom}, CRLF={crlf}, salto final={fin}")
PY
}
