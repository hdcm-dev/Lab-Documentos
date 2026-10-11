#!/usr/bin/env bash
# E6-2b · Verificación adversarial: repite los casos (7) y (8) de E6-2 con el SDK 11 RC1 (la imagen
# 11.0-preview del laboratorio era preview.7). Mismo commit, dos rutas y dos contenedores.
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
IMG=mcr.microsoft.com/dotnet/sdk:11.0.100-rc.1
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
docker pull -q "$IMG" >/dev/null
echo "imagen: $(docker image inspect "$IMG" --format '{{index .RepoDigests 0}}')"
run() { # $1 etiqueta  $2 ruta  $3 SOURCE_DATE_EPOCH (opcional)
  local d=$tmp/$1
  "$raiz/comun/preparar-repo.sh" "$raiz/fuente-dotnet" "$d" >/dev/null; rm "$d/global.json"
  docker run --rm --user "$(id -u):$(id -g)" ${3:+-e SOURCE_DATE_EPOCH=$3} -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp \
    -e DOTNET_NOLOGO=1 -e CI=true -v "$d":"$2" -w "$2" "$IMG" \
    bash -c 'dotnet pack -c Release -o out -nologo -v q >/dev/null 2>&1; dotnet --version' >"$d.sdk"
}
run n_a /w/a; run n_b /home/x/otra
run s_a /w/a 1767225600; run s_b /home/x/otra 1767225600
echo "SDK: $(cat "$tmp/n_a.sdk")"
h() { sha256sum "$1" | cut -c1-12; }
for p in "n_a n_b (7) sin SOURCE_DATE_EPOCH" "s_a s_b (8) con SOURCE_DATE_EPOCH=1767225600"; do
  set -- $p; a=$1 b=$2; shift 2
  printf '%s\n   dll %s | %s   nupkg %s | %s\n' "$*" "$(h $tmp/$a/bin/Release/net8.0/Pieza.dll)" "$(h $tmp/$b/bin/Release/net8.0/Pieza.dll)" \
    "$(h $tmp/$a/out/Pieza.1.4.0.nupkg)" "$(h $tmp/$b/out/Pieza.1.4.0.nupkg)"
done
echo "--- entradas del .nupkg de (8):"
python3 -c "import zipfile,sys; [print('  ',i.date_time,i.filename) for i in zipfile.ZipFile(sys.argv[1]).infolist()]" "$tmp/s_a/out/Pieza.1.4.0.nupkg"
