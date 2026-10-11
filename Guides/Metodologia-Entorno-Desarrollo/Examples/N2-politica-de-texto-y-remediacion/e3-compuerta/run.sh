#!/usr/bin/env bash
# E3 — Verificar la política: la misma compuerta en el hook local y en el PR.
# Detecta cp1252 reintroducido, BOM indebido, BOM faltante y CRLF en el índice.
set -euo pipefail
cd "$(dirname "$0")/.."
LAB=$PWD
W=$LAB/work/e3
rm -rf "$W"; mkdir -p "$W"
export GIT_CONFIG_GLOBAL=$W/gitconfig GIT_CONFIG_NOSYSTEM=1
git config --global user.name lab; git config --global user.email lab@example.invalid
git config --global init.defaultBranch main
GATE="python3 $LAB/tools/check_text_policy.py"

git init -q "$W/repo"; cd "$W/repo"
# Un archivo que entró con CRLF ANTES de que existiera la política (heredado no renormalizado)
printf 'dato 1\r\ndato 2\r\n' > heredado.txt
git -c core.autocrlf=false add heredado.txt; git commit -qm "heredado con CRLF"
cp "$LAB/politica/.editorconfig" "$LAB/politica/.gitattributes" .
mkdir -p src scripts
printf 'namespace App;\npublic static class A { public const string S = "Atención"; }\n' > src/A.cs
printf 'print("hola")\n' > tool.py
git add .; git commit -qm "politica + codigo limpio"
echo "== 0. Base limpia salvo el heredado no renormalizado"
$GATE || true

echo; echo "== 1. Rama de PR que reintroduce los tres problemas"
git checkout -q -b pr
printf 'namespace App;\npublic static class B { public const string S = "Configuración"; }\n' | iconv -f UTF-8 -t CP1252 > src/B.cs  # editor en cp1252
printf '\xef\xbb\xbfprint("con BOM")\n' > tool.py                                                                     # editor que agrega BOM
printf '\xef\xbb\xbf#!/bin/sh\necho hola\n' > scripts/run.sh
printf 'dato 1\r\ndato 2\r\ndato 3\r\n' > heredado.txt                                                               # se edita el heredado en Windows
git add .; git commit -qm "PR con regresiones de texto"
echo "-- git ls-files --eol del heredado editado (text=auto no convierte lo que ya estaba con CRLF en git):"
git ls-files --eol heredado.txt | sed 's/^/    /'
echo "-- compuerta en modo PR (--base main):"
$GATE --base main || echo "   => exit $?: el PR se rechaza"

echo; echo "== 2. La misma compuerta como hook pre-commit"
git checkout -q main
mkdir -p .githooks
cat > .githooks/pre-commit <<'HOOK'
#!/bin/sh
# Compuerta de política de texto sobre lo que está en stage.
exec python3 "$LAB_TOOLS/check_text_policy.py" --staged
HOOK
chmod +x .githooks/pre-commit
git config core.hooksPath .githooks
export LAB_TOOLS=$LAB/tools
printf '\xef\xbb\xbfprint("con BOM")\n' > tool.py; git add tool.py
git commit -qm "intento con BOM" && echo "   commit aceptado" || echo "   => commit rechazado por el hook"
echo "-- el hook se saltea con --no-verify (por eso la CI es el árbitro, ver N7):"
git commit -q --no-verify -m "salteando el hook" && echo "   commit aceptado con --no-verify"
git reset -q --hard HEAD~1

echo; echo "== 3. La compuerta lee la política del .editorconfig: variante B (C# con BOM)"
printf '\n[*.cs]\ncharset = utf-8-bom\n' >> .editorconfig
echo "-- con la variante B, src/A.cs (sin BOM) pasa a violar la política:"
$GATE || true
git checkout -q -- .editorconfig

echo; echo "== 4. Arreglo del heredado: git add --renormalize (el hook revisa el commit; después, la base completa)"
git add --renormalize heredado.txt && git commit -qm "renormalizar heredado"
$GATE
