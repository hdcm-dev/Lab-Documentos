#!/usr/bin/env bash
# E2 — Remediación de un código heredado en cp1252/CRLF:
#   decidir (política) -> declarar (commit chico) -> migrar (commit mecánico)
#   -> aislar de blame (.git-blame-ignore-revs) -> verificar.
# Ejemplo sintético: no reproduce ningún repositorio real.
set -euo pipefail
cd "$(dirname "$0")/.."
LAB=$PWD
W=$LAB/work/e2
rm -rf "$W"; mkdir -p "$W"
export GIT_CONFIG_GLOBAL=$W/gitconfig GIT_CONFIG_NOSYSTEM=1
git config --global init.defaultBranch main
git config --global core.autocrlf false       # simula el repo tal como lo dejó Windows
como() { GIT_AUTHOR_NAME=$1 GIT_AUTHOR_EMAIL=$1@example.invalid GIT_COMMITTER_NAME=$1 GIT_COMMITTER_EMAIL=$1@example.invalid "${@:2}"; }
cp1252_crlf() { iconv -f UTF-8 -t CP1252 | sed 's/$/\r/'; }   # stdin UTF-8 -> archivo "de Windows"

git init -q "$W/repo"; cd "$W/repo"
mkdir -p src Pages scripts
printf 'bin/\r\nobj/\r\n' > .gitignore
cat > src/Heredado.csproj <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
XML
cp1252_crlf > src/Sensor.cs <<'CS'
namespace Heredado;
public static class Sensor
{
    // identificador con letra no ASCII: en cp1252 el compilador en Linux no lo reconoce
    public static int ÚltimaLectura() => 42;
}
CS
cp1252_crlf > src/Mensajes.cs <<'CS'
namespace Heredado;
public static class Mensajes
{
    public const string Alerta = "Atención: lectura fuera de rango";
    public const string Pie = "Señal recibida";
}
CS
cp1252_crlf > src/Program.cs <<'CS'
using Heredado;
System.Console.WriteLine($"{Mensajes.Alerta} ({Sensor.ÚltimaLectura()})");
System.Console.WriteLine(Mensajes.Pie);
CS
printf 'namespace Heredado;\r\npublic static class Util { public static int Doble(int x) => x * 2; }\r\n' > src/Util.cs
printf '<h1>Configuración del panel</h1>\r\n' | iconv -f UTF-8 -t CP1252 > Pages/Panel.razor
printf '#!/bin/sh\r\necho desplegando\r\n' > scripts/deploy.sh; chmod +x scripts/deploy.sh
como Ana git add .; como Ana git commit -qm "Sistema heredado (cp1252, CRLF)"
sed -i 's/Se\xf1al recibida/Se\xf1al recibida y registrada/' src/Mensajes.cs
como Bruno git commit -qam "Cambio funcional: el pie informa el registro"

echo "== 1. Diagnóstico del heredado (índice): codificación y EOL"
for f in $(git ls-files); do
  if iconv -f UTF-8 -t UTF-8 "$f" >/dev/null 2>&1; then enc="utf-8/ascii"; else enc="NO utf-8"; fi
  printf '  %-22s %-12s %s\n' "$f" "$enc" "$(git ls-files --eol -- "$f" | awk '{print $1}')"
done

echo; echo "== 2. Build en Linux ANTES de migrar"
(cd src && dotnet build -v q -nologo 2>&1 | grep -E "error CS|Compilación|Build" | sed 's#/lab/work/e2/repo/##' | sort -u | head -8) || true

echo; echo "== 3. Commit de POLÍTICA (chico, revisable): .editorconfig + .gitattributes"
cp "$LAB/politica/.editorconfig" "$LAB/politica/.gitattributes" .
como Migrador git add .editorconfig .gitattributes
como Migrador git commit -qm "Política de texto: UTF-8 (BOM sólo en Razor y .sln), LF (salvo .cmd/.bat)"
git show --stat --format='  %h %s' HEAD | sed 's/^/  /'
echo "-- git status justo después de commitear la política (nadie tocó estos archivos):"
git status --short | sed 's/^/    /'

echo; echo "== 4. Commit de MIGRACIÓN (mecánico): iconv + git add --renormalize"
echo "-- sólo iconv; git status antes de renormalize (no garantiza listar los que sólo tienen CRLF):"
bash -c '
set -e
for f in $(git ls-files); do iconv -f UTF-8 -t UTF-8 "$f" >/dev/null 2>&1 || { t=$(mktemp); iconv -f CP1252 -t UTF-8 "$f" > "$t"; cat "$t" > "$f"; rm "$t"; }; done
git status --short | sed "s/^/    /"'
echo "-- después de git add --renormalize . (también entran los que sólo tenían CRLF):"
git add --renormalize .; git add -u; git status --short | sed 's/^/    /'
git reset -q --hard
echo "-- corrida del script completo tools/migrar-a-utf8.sh:"
bash "$LAB/tools/migrar-a-utf8.sh" CP1252 | sed 's/^/    /'
como Migrador git commit -qm "Normalización mecánica: cp1252->UTF-8 y CRLF->LF (sin cambios funcionales)"
MIG=$(git rev-parse HEAD)
git show --stat --format='  %h %s' HEAD | sed 's/^/  /'

echo; echo "== 5. Verificación objetiva de que el commit es puramente mecánico"
bash "$LAB/tools/verificar-commit-mecanico.sh" "$MIG" CP1252

echo "-- contraprueba: una migración con un cambio funcional colado"
git checkout -q -b mig-sucia "$MIG^"
bash "$LAB/tools/migrar-a-utf8.sh" CP1252 >/dev/null 2>&1
sed -i 's/=> 42;/=> 43;/' src/Sensor.cs; git add src/Sensor.cs
como Migrador git commit -qm "Normalización (con un cambio colado)"
bash "$LAB/tools/verificar-commit-mecanico.sh" HEAD CP1252 || echo "  => exit $?: el commit NO es mecánico, se rechaza"
git checkout -q main; git branch -qD mig-sucia

echo; echo "== 6. Refrescar el working tree (los archivos en disco siguen con CRLF hasta el próximo checkout)"
echo "  CR en src/Util.cs antes: $(grep -c $'\r$' src/Util.cs || true)"
git rm -rq --cached . && git reset -q --hard
echo "  CR en src/Util.cs después de 'git rm --cached -r . && git reset --hard': $(grep -c $'\r$' src/Util.cs || true)"

echo; echo "== 7. Build en Linux DESPUÉS de migrar"
(cd src && dotnet build -v q -nologo 2>&1 | grep -E "error|Advertencias|Warn|Errores|Error" | head -4; dotnet run --no-build 2>&1)

echo; echo "== 8. Aislar la migración de git blame"
printf '# Normalización mecánica cp1252->UTF-8 y CRLF->LF (ver tools/verificar-commit-mecanico.sh)\n%s\n' "$MIG" > .git-blame-ignore-revs
como Migrador git add .git-blame-ignore-revs; como Migrador git commit -qm "Ignorar la normalización en blame"
echo "-- git blame src/Mensajes.cs (sin ignorar):"
git blame --date=short src/Mensajes.cs | sed 's/^/    /'
echo "-- git blame --ignore-revs-file .git-blame-ignore-revs src/Mensajes.cs:"
git blame --date=short --ignore-revs-file .git-blame-ignore-revs src/Mensajes.cs | sed 's/^/    /'
git config blame.ignoreRevsFile .git-blame-ignore-revs
echo "-- con 'git config blame.ignoreRevsFile .git-blame-ignore-revs', git blame a secas ya lo ignora:"
git blame --date=short -L4,4 src/Mensajes.cs | sed 's/^/    /'

echo; echo "== 9. Lo que rompe el aislamiento: reescribir la historia cambia el SHA"
git branch -q mig-rebase "$MIG"
git checkout -q mig-rebase
como Otro git commit -q --amend --no-edit --reset-author
echo "  SHA original de la migración: ${MIG:0:12}"
echo "  SHA tras amend/rebase/squash:  $(git rev-parse --short=12 HEAD)  (ya no coincide con .git-blame-ignore-revs)"
git checkout -q main

echo; echo "== 10. Historia final"
git log --format='  %h %an: %s'

echo; echo "== 11. Fusión por squash con un PR por commit: el SHA que se lista es el del squash ya en main"
sh "$LAB/e2-migracion/squash-y-blame.sh"
