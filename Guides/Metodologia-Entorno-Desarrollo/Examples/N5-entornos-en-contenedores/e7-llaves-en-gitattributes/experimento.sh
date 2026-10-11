#!/usr/bin/env bash
# E5-7 — ¿Git entiende llaves {a,b} en los patrones de .gitattributes?
# Varias recetas (incluida la de VS Code) escriben `*.{cmd,bat} text eol=crlf`. Los patrones de
# .gitattributes siguen las reglas de .gitignore (git-scm.com/docs/gitattributes), que no
# documentan expansión de llaves. Se prueba con `git check-attr` en el host y en la imagen del SDK.
set -u
IMG='mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743'
W="$(mktemp -d)"; trap 'rm -rf "$W"' EXIT
probar() { # $1 = descripción; el .gitattributes ya está escrito en $W
  echo "-- $1"
  echo "   host ($(git --version)):"
  git -C "$W" check-attr eol -- a.cmd b.BAT c.bat d.sh | sed 's/^/     /'
  echo "   contenedor:"
  docker run --rm -v "$W":/w -w /w "$IMG" bash -c 'git config --global --add safe.directory /w; echo "     ($(git --version))"; git check-attr eol -- a.cmd b.BAT c.bat d.sh | sed "s/^/     /"'
}
git init -q "$W"; touch "$W"/a.cmd "$W"/b.BAT "$W"/c.bat "$W"/d.sh
printf '* text=auto eol=lf\n*.{cmd,bat} text eol=crlf\n' > "$W/.gitattributes"
echo "## 1. Patrón con llaves"; cat "$W/.gitattributes" | sed 's/^/   | /'
probar "con llaves"
printf '* text=auto eol=lf\n*.{cmd,[cC][mM][dD]} text eol=crlf\n*.{bat,[bB][aA][tT]} text eol=crlf\n' > "$W/.gitattributes"
echo; echo "## 1b. Receta literal de code.visualstudio.com/docs/devcontainers/tips-and-tricks"; cat "$W/.gitattributes" | sed 's/^/   | /'
probar "receta de VS Code"
printf '* text=auto eol=lf\n*.cmd text eol=crlf\n*.bat text eol=crlf\n*.[cC][mM][dD] text eol=crlf\n*.[bB][aA][tT] text eol=crlf\n' > "$W/.gitattributes"
echo; echo "## 2. Un patrón por extensión (con clases para mayúsculas)"; cat "$W/.gitattributes" | sed 's/^/   | /'
probar "sin llaves"
