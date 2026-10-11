#!/usr/bin/env bash
# E0-5b · Verificación adversarial de E0-5 (2026-10-07). Corre dentro de mcr.microsoft.com/dotnet/sdk:8.0.
# Pregunta: ¿por qué ContinuousIntegrationBuild sólo normalizó la ruta dentro de un repo git?
# Hipótesis: la normalización mapea los ítems SourceRoot, y sin git nadie los aporta.
# Se prueba (a) con git pero apagando las consultas al control de versiones,
# (b) sin git pero declarando un SourceRoot a mano, (c) sin git con PathMap explícito.
set -u
git config --global user.email lab@example.invalid; git config --global user.name lab
git config --global init.defaultBranch main
preparar() { # $1 = dir, $2 = con|sin git
  rm -rf "$1"; mkdir -p "$1"; cp /src/Calculo.csproj /src/Calculo.cs "$1/"
  if [ "$2" = con ]; then ( cd "$1" && git init -q && git add . && GIT_AUTHOR_DATE='2026-01-01T00:00:00Z' GIT_COMMITTER_DATE='2026-01-01T00:00:00Z' git commit -qm fuente ); fi
}
pdb() { grep -a -o '[[:print:]]*Calculo\.pdb' "$1/bin/Release/net8.0/Calculo.dll" | head -1; }
echo "SDK $(dotnet --version)"
preparar /home/x/otra con
( cd /home/x/otra && dotnet build -c Release -nologo -v q -p:ContinuousIntegrationBuild=true -p:EnableSourceControlManagerQueries=false >/dev/null 2>&1 )
echo "(a) con git + CI=true + EnableSourceControlManagerQueries=false: $(pdb /home/x/otra)"
preparar /home/x/otra sin
cat > /home/x/otra/Directory.Build.targets <<'XML'
<Project><ItemGroup><SourceRoot Include="$(MSBuildProjectDirectory)/" /></ItemGroup></Project>
XML
( cd /home/x/otra && dotnet build -c Release -nologo -v q -p:ContinuousIntegrationBuild=true >/dev/null 2>&1 )
echo "(b) sin git + CI=true + SourceRoot declarado a mano:            $(pdb /home/x/otra)"
preparar /home/x/otra sin
( cd /home/x/otra && dotnet build -c Release -nologo -v q "-p:PathMap=/home/x/otra/=/_/" >/dev/null 2>&1 )
echo "(c) sin git + PathMap=/home/x/otra/=/_/ explícito:              $(pdb /home/x/otra)"
