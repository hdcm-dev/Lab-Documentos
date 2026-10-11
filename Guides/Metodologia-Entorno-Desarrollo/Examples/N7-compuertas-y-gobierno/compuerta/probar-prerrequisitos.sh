#!/usr/bin/env bash
# Mesa, ciclo 1 (P-22 L13): una herramienta que falta no es una violación.
# Corre la compuerta en un repo de juguete (a) sin python3 en el PATH y (b) sin check_text_policy.py.
# Mesa, ciclo 2 (P-23 L2): (d) sin el paquete EditorConfig de Python, simulado con un módulo que falla al importarse.
# Uso (en el host, sólo bash y git): bash compuerta/probar-prerrequisitos.sh
set -u
AQUI=$(cd "$(dirname "$0")" && pwd)
T=$(mktemp -d); trap 'rm -rf "$T"' EXIT
mkdir -p "$T/nopy" "$T/r/tools/ci"
for c in bash git dirname sed tr cat sh env printf; do ln -s "$(command -v $c)" "$T/nopy/$c"; done
cd "$T/r" && git init -q && printf 'root = true\n[*]\ncharset = utf-8\n' > .editorconfig && git add . \
  && git -c user.name=lab -c user.email=lab@example.invalid commit -qm "repo de juguete"
cp "$AQUI/compuerta.sh" tools/ci/
echo "== (a) sin python3 en el PATH"
env PATH="$T/nopy" bash tools/ci/compuerta.sh | grep -E 'texto|resultado'; echo "rc=${PIPESTATUS[0]}"
echo "== (b) con python3, sin check_text_policy.py junto a compuerta.sh"
bash tools/ci/compuerta.sh | grep -E 'texto|resultado'; echo "rc=${PIPESTATUS[0]}"
echo "== (c) con python3 y check_text_policy.py (la política se cumple)"
cp "$AQUI/../../N2-politica-de-texto-y-remediacion/tools/check_text_policy.py" tools/ci/
if python3 -c 'import editorconfig' 2>/dev/null; then bash tools/ci/compuerta.sh | grep -E 'texto|resultado'; echo "rc=${PIPESTATUS[0]}"
else echo "(el host no tiene el paquete EditorConfig: este caso lo cubre E7-1 en su imagen)"; fi
echo "== (d) con python3 y check_text_policy.py, sin el paquete EditorConfig (se simula su ausencia)"
mkdir -p "$T/sinmod" && echo 'raise ImportError("simulado: paquete ausente")' > "$T/sinmod/editorconfig.py"
PYTHONPATH="$T/sinmod" bash tools/ci/compuerta.sh | grep -E 'texto|resultado'; echo "rc=${PIPESTATUS[0]}"
