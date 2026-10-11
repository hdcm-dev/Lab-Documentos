#!/bin/sh
# E1-N1-08 — Qué ve git cuando cambian el BOM, la codificación o los finales de línea,
#            y cómo actúan core.autocrlf y .gitattributes (text / eol / working-tree-encoding).
# Se corre DENTRO de un contenedor con git (la imagen del SDK de .NET lo trae). Requiere generar.py.
set -u
F="$(cd "$(dirname "$0")" && pwd)/work"
R=/tmp/repo-n1; rm -rf "$R"; mkdir -p "$R"; cd "$R" || exit 1
git --version
git init -q -b main . && git config user.email lab@example.invalid && git config user.name lab
git config core.autocrlf false   # partimos de "git no toca nada"

cp "$F/dotnet/utf8-lf/Mensajes.cs" Mensajes.cs
git add . && git commit -qm base

echo "== 1) agregar BOM: el diff muestra la primera línea cambiada, pero a la vista es idéntica"
cp "$F/dotnet/utf8bom-lf/Mensajes.cs" Mensajes.cs
git --no-pager diff --stat | tail -1
git --no-pager diff | sed -n '/^-namespace/,/^+.*namespace/p' | od -c | head -3
git checkout -q -- Mensajes.cs

echo "== 2) pasar a CRLF: TODAS las líneas cambian; git diff lo delata sólo con ^M o --ignore-cr-at-eol"
cp "$F/dotnet/utf8-crlf/Mensajes.cs" Mensajes.cs
git --no-pager diff --stat | tail -1
n=$(git --no-pager diff --ignore-cr-at-eol --numstat | wc -l | tr -d ' ')
echo "   con --ignore-cr-at-eol: $n archivo(s) con diferencias reales"
git ls-files --eol Mensajes.cs
git checkout -q -- Mensajes.cs

echo "== 3) cp1252: git lo trata como texto (no hay NUL) y el diff muestra bytes inválidos"
cp "$F/dotnet/cp1252-lf/Mensajes.cs" Mensajes.cs
git --no-pager diff --stat | tail -1
printf '   bytes de "Atención" en el diff: '; git --no-pager diff | grep -ao '^+.*Atenci.n' | grep -ao 'Atenci.n' | od -An -tx1 | head -1
echo "   git diff no avisa nada: para git son bytes; quien interpreta es el lector"
git checkout -q -- Mensajes.cs

echo "== 4) .gitattributes '* text=auto eol=lf': un CRLF agregado se normaliza al indexar"
printf '* text=auto eol=lf\n*.cs text diff=csharp\n' > .gitattributes
git add .gitattributes && git commit -qm attrs
cp "$F/dotnet/utf8-crlf/Mensajes.cs" Mensajes.cs
git add Mensajes.cs 2>&1 | head -2
echo "   índice vs árbol tras add:"; git ls-files --eol Mensajes.cs
echo "   ¿queda algo por commitear?: $(git diff --cached --quiet && echo 'no (el CRLF no entra al repo)' || echo sí)"
git checkout -q -- Mensajes.cs 2>/dev/null; git reset -q --hard

echo "== 5) working-tree-encoding: el repo guarda UTF-8 y el árbol de trabajo se escribe en cp1252"
printf 'Legado.cs text working-tree-encoding=CP1252 eol=crlf\n' >> .gitattributes
cp "$F/dotnet/cp1252-lf/Mensajes.cs" Legado.cs
git add .gitattributes Legado.cs && git commit -qm legado
printf '   en el índice (blob): '; git cat-file -p :Legado.cs | grep -o 'Atenci..n' | od -An -tx1 | head -1
printf '   en disco (checkout): '; rm Legado.cs; git checkout -q -- Legado.cs; grep -ao 'Atenci.n' Legado.cs | od -An -tx1 | head -1

echo "== 6) core.autocrlf=true (lo que Pro Git aconseja en Windows) sin .gitattributes: el checkout escribe CRLF"
R2=/tmp/repo-n1-b; rm -rf "$R2"; mkdir -p "$R2"; cd "$R2" || exit 1
git init -q -b main . && git config user.email lab@example.invalid && git config user.name lab
cp "$F/dotnet/utf8-lf/Mensajes.cs" Mensajes.cs; git add . && git commit -qm base
git config core.autocrlf true; rm Mensajes.cs; git checkout -q -- Mensajes.cs
git ls-files --eol Mensajes.cs
echo "   bytes en disco con CR: $(tr -cd '\r' < Mensajes.cs | wc -c)  | git status limpio: $(git status --porcelain | wc -l | tr -d ' ') cambios"

echo "== 7) git add --renormalize: aplicar .gitattributes a lo ya commiteado con CRLF"
git config core.autocrlf false
cp "$F/dotnet/utf8-crlf/Mensajes.cs" Mensajes.cs; git add Mensajes.cs; git commit -qm 'entra CRLF'
git ls-files --eol Mensajes.cs
printf '* text=auto eol=lf\n' > .gitattributes; git add .gitattributes
git add --renormalize . && git commit -qm renormalizar
git ls-files --eol Mensajes.cs
git --no-pager show --stat HEAD | tail -2
