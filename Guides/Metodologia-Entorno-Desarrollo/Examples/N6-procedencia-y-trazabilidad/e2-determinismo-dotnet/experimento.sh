#!/usr/bin/env bash
# E6-2 · ¿Dos builds del MISMO commit dan los mismos bytes? Qué lo logra y qué lo rompe.
# Corre en el HOST. Cada build es un clon independiente, en otra ruta y en otro contenedor (otra "máquina").
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
build() { # $1 etiqueta  $2 imagen  $3 ruta-en-contenedor  $4 CI(true|false)  resto: propiedades extra
  local et=$1 img=$2 ruta=$3 ci=$4; shift 4
  "$raiz/comun/preparar-repo.sh" "$raiz/fuente-dotnet" "$tmp/$et" >/dev/null
  [ "$img" != mcr.microsoft.com/dotnet/sdk:8.0 ] && rm "$tmp/$et/global.json"  # otra máquina, sin global.json (ver N3)
  docker run --rm --user "$(id -u):$(id -g)" ${SDE:+-e SOURCE_DATE_EPOCH=$SDE} -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 \
    -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e CI="$ci" -v "$tmp/$et":"$ruta" -w "$ruta" "$img" \
    bash -c "dotnet pack -c Release -o out -nologo -v q $* >/tmp/b.log 2>&1 || tail -5 /tmp/b.log; echo \"\$(dotnet --version)\" > out/sdk.txt"
}
h() { [ -f "$1" ] && sha256sum "$1" | cut -c1-12 || echo "------------"; }
fila() { # compara dos builds: dll, pdb, nupkg
  local a=$1 b=$2 titulo=$3 v
  local da db pa pb na nb
  da=$(h "$tmp/$a/bin/Release/net8.0/Pieza.dll"); db=$(h "$tmp/$b/bin/Release/net8.0/Pieza.dll")
  pa=$(h "$tmp/$a/bin/Release/net8.0/Pieza.pdb"); pb=$(h "$tmp/$b/bin/Release/net8.0/Pieza.pdb")
  na=$(h "$tmp/$a/out/Pieza.1.4.0.nupkg");        nb=$(h "$tmp/$b/out/Pieza.1.4.0.nupkg")
  [ "$da$pa$na" = "$db$pb$nb" ] && v="IGUALES" || v="DISTINTOS"
  printf '%s\n   dll %s | %s   pdb %s | %s   nupkg %s | %s   => %s\n' "$titulo" "$da" "$db" "$pa" "$pb" "$na" "$nb" "$v"
}
S8=mcr.microsoft.com/dotnet/sdk:8.0; S10=mcr.microsoft.com/dotnet/sdk:10.0; S11=mcr.microsoft.com/dotnet/sdk:11.0-preview
echo "imagen 8.0 : $(docker image inspect $S8 --format '{{index .RepoDigests 0}}')"
echo "imagen 10.0: $(docker image inspect $S10 --format '{{index .RepoDigests 0}}')"
echo "imagen 11.0-preview: $(docker image inspect $S11 --format '{{index .RepoDigests 0}}')"
build ci_a  $S8  /w/a          true;  build ci_b  $S8  /home/x/otra  true
build dev_a $S8  /w/a          false; build dev_b $S8  /home/x/otra  false
build nd_a  $S8  /w/a          true -p:Deterministic=false; build nd_b $S8 /w/a true -p:Deterministic=false
build pm_a  $S8  /w/a          false -p:PathMap=/w/a=/_/;   build pm_b $S8 /home/x/otra false -p:PathMap=/home/x/otra=/_/
build s10   $S10 /w/a          true
# El .nupkg es un zip: además del compilador, importan las fechas de cada entrada.
SDE=1767225600  # 2026-01-01T00:00:00Z = fecha del commit (git log -1 --pretty=%ct)
build e8_a  $S8  /w/a true; build e8_b  $S8  /home/x/otra true
build e11_a $S11 /w/a true; build e11_b $S11 /home/x/otra true
unset SDE
build n11_a $S11 /w/a true; build n11_b $S11 /home/x/otra true
echo "SDK usados: ci_a=$(cat $tmp/ci_a/out/sdk.txt) s10=$(cat $tmp/s10/out/sdk.txt) n11=$(cat $tmp/n11_a/out/sdk.txt)"
echo
fila ci_a  ci_b  "(1) mismo commit, rutas y contenedores distintos, ContinuousIntegrationBuild=true"
fila dev_a dev_b "(2) mismo commit, rutas distintas, build de desarrollador (sin CI)"
fila nd_a  nd_b  "(3) misma ruta, CI=true pero Deterministic=false"
fila pm_a  pm_b  "(4) rutas distintas, sin CI, PathMap explícito ruta=/_/"
fila ci_a  s10   "(5) misma ruta y CI=true, pero SDK 8 frente a SDK 10 (sin global.json)"
fila e8_a  e8_b  "(6) como (1) + SOURCE_DATE_EPOCH (SDK 8)"
fila n11_a n11_b "(7) como (1) con SDK 11 preview (pack determinista por defecto según NuGet)"
fila e11_a e11_b "(8) como (7) + SOURCE_DATE_EPOCH"
echo
echo "--- contenido del .nupkg de (1): hash de cada entrada, ignorando las fechas del zip"
python3 - "$tmp/ci_a/out/Pieza.1.4.0.nupkg" "$tmp/ci_b/out/Pieza.1.4.0.nupkg" <<'PY2'
import sys, zipfile, hashlib
a, b = (zipfile.ZipFile(p) for p in sys.argv[1:3])
for n in sorted(set(a.namelist()) | set(b.namelist())):
    ha = hashlib.sha256(a.read(n)).hexdigest()[:12] if n in a.namelist() else "(no está)"
    hb = hashlib.sha256(b.read(n)).hexdigest()[:12] if n in b.namelist() else "(no está)"
    print(f"   {'=' if ha == hb else '≠'} {n:60} {ha} | {hb}")
PY2
echo "--- mapa de SourceLink en el PDB de (4): PathMap a mano no lo normaliza"
for et in pm_a pm_b; do printf '   %-6s %s\n' "$et" "$(grep -a -o '{"documents":{[^}]*}}' "$tmp/$et/bin/Release/net8.0/Pieza.pdb")"; done
echo
echo "--- ruta del PDB grabada en la DLL (lo que cambia entre (2)a y (2)b):"
for et in ci_a dev_a dev_b; do printf '   %-6s %s\n' "$et" "$(grep -a -o '[[:print:]]*Pieza\.pdb' "$tmp/$et/bin/Release/net8.0/Pieza.dll" | head -1)"; done
for et in ci_a e8_a n11_a e11_a; do
  echo "--- fechas dentro del .nupkg de $et (zip): primeras entradas"
  python3 -c "import zipfile,sys; z=zipfile.ZipFile(sys.argv[1]); [print('  ',i.date_time,i.filename) for i in z.infolist()[:3]]" "$tmp/$et/out/Pieza.1.4.0.nupkg"
done
