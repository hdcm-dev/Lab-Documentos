#!/usr/bin/env bash
# E5-3 — Un repo clonado "a la Windows" (core.autocrlf=true, sin bit de ejecución) montado en un contenedor Linux.
# Se simula la estación Windows con opciones de git de un solo comando (-c), sin tocar la configuración real.
set -u
IMG='mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743'
W="$(mktemp -d)"; trap 'rm -rf "$W"' EXIT
G="git -c user.name=demo -c user.email=demo@example.invalid -c init.defaultBranch=main"
contenedor() { docker run --rm -v "$1":/w -w /w "$IMG" bash -c "$2"; }   # corre como root (default de la imagen)

# --- Repositorio de origen SIN política: el script se commiteó desde Windows (modo 100644) ---
$G init -q "$W/origen"
printf '#!/usr/bin/env bash\nset -euo pipefail\necho "build OK en $(uname -s)"\n' > "$W/origen/build.sh"
printf 'linea uno\nlinea dos\n' > "$W/origen/notas.txt"
$G -C "$W/origen" add . && $G -C "$W/origen" commit -qm "sin politica"

echo "## A. Sin .gitattributes: clon en 'Windows' (autocrlf=true), montado en Linux"
$G -c core.autocrlf=true -c core.fileMode=false clone -q "$W/origen" "$W/clon-a"
file "$W/clon-a/build.sh"
echo "-- git como root sobre archivos de otro UID:"
contenedor "$W/clon-a" 'git status --short 2>&1 | head -3'
echo "-- con safe.directory declarado para /w:"
contenedor "$W/clon-a" 'git config --global --add safe.directory /w; git status --short'
echo "-- ejecutar el script:"
contenedor "$W/clon-a" './build.sh; echo "exit=$?"; bash build.sh; echo "exit=$?"; chmod +x build.sh; ./build.sh; echo "exit=$?"' 2>&1 | cat -A | sed 's/\$$//'

echo; echo "## B. Con política en el repo: .gitattributes (eol=lf) + bit de ejecución registrado en git"
cat > "$W/origen/.gitattributes" <<'GA'
* text=auto eol=lf
*.sh text eol=lf
# Un patrón por extensión: git no expande llaves {a,b} en .gitattributes (E5-7).
*.[cC][mM][dD] text eol=crlf
*.[bB][aA][tT] text eol=crlf
GA
$G -C "$W/origen" add .gitattributes
$G -C "$W/origen" update-index --chmod=+x build.sh
$G -C "$W/origen" commit -qm "politica: eol=lf y +x"
$G -C "$W/origen" ls-files -s build.sh
$G -c core.autocrlf=true -c core.fileMode=false clone -q "$W/origen" "$W/clon-b"
file "$W/clon-b/build.sh" "$W/clon-b/notas.txt"
contenedor "$W/clon-b" 'git config --global --add safe.directory /w; echo "status: [$(git status --short)]"; ./build.sh || bash build.sh'
