#!/usr/bin/env bash
# E1 — ¿Quién manda: core.autocrlf, .gitattributes o .editorconfig?
# Corre dentro de la imagen del laboratorio (ver README). Crea repos de juguete
# en work/e1 y los borra al empezar.
set -euo pipefail
cd "$(dirname "$0")/.."
LAB=$PWD
W=$LAB/work/e1
rm -rf "$W"; mkdir -p "$W"
export GIT_CONFIG_GLOBAL=$W/gitconfig GIT_CONFIG_NOSYSTEM=1
git config --global user.name lab; git config --global user.email lab@example.invalid
git config --global init.defaultBranch main; git config --global advice.detachedHead false

eol() { git ls-files --eol -- "$@" | sed 's/\t/  /'; }
crlf_wt() { for f in "$@"; do printf '  %-12s CR en working tree: %s\n' "$f" "$(grep -c $'\r$' "$f" || true)"; done; }

echo "== A. Sin .gitattributes: la política la decide la máquina de cada uno"
git init -q "$W/a"; cd "$W/a"
printf 'linea 1\r\nlinea 2\r\n' > win.txt        # archivo escrito por un editor en Windows
git -c core.autocrlf=false add win.txt; git commit -qm "dev con autocrlf=false"
echo "-- dev 1 (core.autocrlf=false) commitea un archivo CRLF:"; eol win.txt
printf 'linea 1\r\nlinea 2\r\n' > win2.txt
git -c core.autocrlf=true add win2.txt; git commit -qm "dev con autocrlf=true" 2>/dev/null
echo "-- dev 2 (core.autocrlf=true) commitea el mismo contenido:"; eol win2.txt
echo "   => mismo archivo, distinto contenido en el repositorio según la config local"

echo; echo "== B. Con .gitattributes '* text=auto eol=lf': manda el repositorio"
git init -q "$W/b"; cd "$W/b"
cp "$LAB/politica/.gitattributes" .
printf 'linea 1\r\nlinea 2\r\n' > win.txt
printf '@echo off\necho hola\n' > build.cmd
git -c core.autocrlf=false add .; git commit -qm "politica + archivos"
echo "-- índice después de add con core.autocrlf=false:"; eol win.txt build.cmd
for cfg in "core.autocrlf=true" "core.autocrlf=input" "core.autocrlf=false" "core.eol=crlf"; do
  rm -f win.txt build.cmd
  git -c "$cfg" checkout -q -- win.txt build.cmd
  echo "-- checkout con $cfg:"; crlf_wt win.txt build.cmd
done
echo "   => eol=lf / eol=crlf del atributo se imponen a core.autocrlf y core.eol"

echo; echo "== B2. Con '* text=auto' SIN eol (convención de dotnet/runtime): el índice es del repo, el working tree es de la máquina"
git init -q "$W/b2"; cd "$W/b2"
echo '* text=auto' > .gitattributes
printf 'linea 1\r\nlinea 2\r\n' > win.txt
git -c core.autocrlf=false add .; git commit -qm "text=auto sin eol"
echo "-- índice:"; eol win.txt
for cfg in "core.autocrlf=true" "core.autocrlf=false" "core.eol=crlf"; do
  rm -f win.txt; git -c "$cfg" checkout -q -- win.txt
  echo "-- checkout con $cfg:"; crlf_wt win.txt
done
echo "   => el índice queda normalizado, pero los bytes que ve el compilador dependen de cada máquina"

echo; echo "== C. .editorconfig no participa en git"
git init -q "$W/c"; cd "$W/c"
printf 'root = true\n[*]\nend_of_line = crlf\ncharset = utf-8-bom\n' > .editorconfig
printf 'linea 1\nlinea 2\n' > unix.txt
git -c core.autocrlf=false add .; git commit -qm "editorconfig pide crlf+bom"
rm unix.txt; git -c core.autocrlf=false checkout -q -- unix.txt
echo "-- después de commit + checkout, unix.txt sigue con LF y sin BOM:"
eol unix.txt; printf '  primeros bytes: '; head -c3 unix.txt | od -An -c
echo "   => .editorconfig lo aplica el editor al guardar; git no lo lee"

echo; echo "== D. Por qué importa el EOL del working tree: un literal verbatim de C#"
mkdir -p "$W/d"; cd "$W/d"
cat > D.csproj <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup>
</Project>
XML
printf 'var s = @"a\nb";\nSystem.Console.WriteLine($"largo={s.Length} bytes={System.BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(s))}");\n' > Program.cs
echo "-- Program.cs con LF (checkout en Linux o con eol=lf):"
dotnet run -v q 2>&1 | tail -1
sed -i 's/$/\r/' Program.cs
echo "-- el mismo Program.cs con CRLF (checkout en Windows con autocrlf=true y sin eol):"
dotnet run -v q 2>&1 | tail -1
echo "   => el fin de línea del fuente entra en el binario: mismo commit, distinto programa"
