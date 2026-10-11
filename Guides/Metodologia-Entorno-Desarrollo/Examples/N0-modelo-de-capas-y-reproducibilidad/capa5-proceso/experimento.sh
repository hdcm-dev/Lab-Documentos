#!/usr/bin/env bash
# E0-5 · Capa 5 (proceso de build). Corre dentro de mcr.microsoft.com/dotnet/sdk:8.0.
# Mismo fuente, mismo SDK, mismo grafo: sólo cambian la RUTA de trabajo y propiedades de MSBuild.
set -u
git config --global user.email lab@example.invalid; git config --global user.name lab
git config --global init.defaultBranch main
compilar() { # $1 = directorio, $2 = con|sin git, resto = propiedades extra
  local dir=$1 conGit=$2; shift 2
  rm -rf "$dir"; mkdir -p "$dir"; cp /src/Calculo.csproj /src/Calculo.cs "$dir/"
  if [ "$conGit" = con ]; then
    # Commit con fecha fija: el SHA es el mismo en ambas rutas (mismo contenido, autor y fecha)
    ( cd "$dir" && git init -q && git add . && GIT_AUTHOR_DATE='2026-01-01T00:00:00Z' GIT_COMMITTER_DATE='2026-01-01T00:00:00Z' git commit -qm fuente )
  fi
  ( cd "$dir" && dotnet build -c Release -nologo -v q "$@" >/dev/null 2>&1 )
  sha256sum "$dir/bin/Release/net8.0/Calculo.dll" | cut -c1-16
}
pdb() { grep -a -o '[[:print:]]*Calculo\.pdb' "$1/bin/Release/net8.0/Calculo.dll" | head -1; }
caso() { local v; [ "$2" = "$3" ] && v="IGUALES" || v="DISTINTOS"; printf '%-66s %s  %s  => %s\n' "$1" "$2" "$3" "$v"; }
echo "SDK $(dotnet --version)"
caso "(1) misma ruta, dos builds (Deterministic=true por defecto)"   "$(compilar /w/a sin)" "$(compilar /w/a sin)"
caso "(2) misma ruta, Deterministic=false"                          "$(compilar /w/a sin -p:Deterministic=false)" "$(compilar /w/a sin -p:Deterministic=false)"
caso "(3) rutas distintas (/w/a vs /home/x/otra)"                   "$(compilar /w/a sin)" "$(compilar /home/x/otra sin)"
caso "(4) rutas distintas + ContinuousIntegrationBuild, SIN git"     "$(compilar /w/a sin -p:ContinuousIntegrationBuild=true)" "$(compilar /home/x/otra sin -p:ContinuousIntegrationBuild=true)"
caso "(5) rutas distintas + ContinuousIntegrationBuild, CON git"     "$(compilar /w/a con -p:ContinuousIntegrationBuild=true)" "$(compilar /home/x/otra con -p:ContinuousIntegrationBuild=true)"
echo "--- ruta del .pdb incrustada en la DLL:"
compilar /home/x/otra sin >/dev/null;                                     echo "    caso (3): $(pdb /home/x/otra)"
compilar /home/x/otra sin -p:ContinuousIntegrationBuild=true >/dev/null;  echo "    caso (4): $(pdb /home/x/otra)"
compilar /home/x/otra con -p:ContinuousIntegrationBuild=true >/dev/null;  echo "    caso (5): $(pdb /home/x/otra)"
