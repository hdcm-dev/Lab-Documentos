#!/usr/bin/env bash
# E4 — working-tree-encoding: qué resuelve (git ve texto UTF-8 en el índice)
# y qué NO resuelve (el compilador sigue leyendo los bytes del working tree).
set -euo pipefail
cd "$(dirname "$0")/.."
LAB=$PWD
W=$LAB/work/e4
rm -rf "$W"; mkdir -p "$W"
export GIT_CONFIG_GLOBAL=$W/gitconfig GIT_CONFIG_NOSYSTEM=1
git config --global user.name lab; git config --global user.email lab@example.invalid
git config --global init.defaultBranch main
valida() { if iconv -f UTF-8 -t UTF-8 >/dev/null 2>&1; then echo "UTF-8 válido"; else echo "NO es UTF-8"; fi; }

echo "== A. Archivo UTF-16LE (p. ej. un .ps1 o .rc que una herramienta exige así)"
git init -q "$W/a"; cd "$W/a"
printf 'Write-Host "Configuración"\r\n' | iconv -f UTF-8 -t UTF-16LE > sin-atributo.ps1
cp sin-atributo.ps1 con-atributo.ps1
printf 'con-atributo.ps1 text working-tree-encoding=UTF-16LE eol=crlf\n' > .gitattributes
git add .; git commit -qm v1
printf 'Write-Host "Configuración nueva"\r\n' | iconv -f UTF-8 -t UTF-16LE > sin-atributo.ps1
cp sin-atributo.ps1 con-atributo.ps1
echo "-- git diff del archivo SIN atributo:"; git diff --stat -- sin-atributo.ps1 | sed 's/^/    /'; git diff -- sin-atributo.ps1 | tail -1 | sed 's/^/    /'
echo "-- git diff del archivo CON working-tree-encoding=UTF-16LE:"; git diff -- con-atributo.ps1 | tail -2 | sed 's/^/    /'
git add .; git commit -qm v2
echo "-- índice del archivo con atributo: $(git show :con-atributo.ps1 | valida); working tree: $(valida < con-atributo.ps1)"

echo; echo "== B. working-tree-encoding=CP1252 sobre un .cs heredado"
git init -q "$W/b"; cd "$W/b"
printf '*.cs text working-tree-encoding=CP1252\n' > .gitattributes
cat > B.csproj <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup>
</Project>
XML
printf 'bin/\nobj/\n' > .gitignore
printf 'static int ÚltimaLectura() => 42;\nSystem.Console.WriteLine($"Atención {ÚltimaLectura()}");\n' | iconv -f UTF-8 -t CP1252 > Program.cs
git add .; git commit -qm "cs en cp1252 con working-tree-encoding"
echo "-- índice: $(git show :Program.cs | valida)   working tree: $(valida < Program.cs)"
echo "-- un clon nuevo en Linux materializa el working tree otra vez en cp1252:"
git clone -q "$W/b" "$W/b-clon"; cd "$W/b-clon"
echo "   working tree del clon: $(valida < Program.cs)"
echo "-- build en Linux del clon:"
dotnet build -v q -nologo 2>&1 | grep -E "error CS" | sed 's#/lab/work/e4/b-clon/##; s/ \[.*//' | sort -u | head -3 || true
echo "   => el compilador lee el working tree: el atributo no arregla el caso .NET; hay que convertir"
echo "-- y un cliente sin soporte (libgit2/JGit, git anterior a marzo de 2018, según gitattributes(5)) vería el contenido del índice (UTF-8):"
echo "   bytes del índice:      $(git show :Program.cs | head -c 24 | od -An -tx1 | tr -s ' \n' ' ')"
echo "   bytes del working tree: $(head -c 24 Program.cs | od -An -tx1 | tr -s ' \n' ' ')"
