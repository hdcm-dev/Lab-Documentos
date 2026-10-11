#!/usr/bin/env bash
# E6-4 · Python: ¿la rueda (wheel) del mismo fuente sale igual en dos máquinas? Papel de SOURCE_DATE_EPOCH,
# diferencia entre backends, y qué pasa al meter un SBOM dentro de la rueda (PEP 770).
# Corre en el HOST. Cada build: copia nueva del fuente, en otra ruta, con otras fechas de archivo.
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
img=python:3.12-slim
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
echo "imagen: $(docker image inspect $img --format '{{index .RepoDigests 0}}')"
build() { # $1 etiqueta $2 backend $3 ruta-en-contenedor ; variables opcionales SDE, SBOM_REPRO
  cp -r "$raiz/fuente-python/$2" "$tmp/$1"; touch -d "@$(( $(date +%s) - RANDOM ))" "$tmp/$1/src/pieza/__init__.py"
  docker run --rm --user 0 -e PROPIETARIO="$(id -u):$(id -g)" ${SDE:+-e SOURCE_DATE_EPOCH=$SDE} ${SBOM_REPRO:+-e SBOM_REPRO=1} \
    -v "$tmp/$1":"$3" -v "$aqui/dentro.sh":/dentro.sh:ro -w "$3" $img bash /dentro.sh "$2"
}
whl() { ls "$tmp/$1"/dist/*.whl; }
fila() {
  local a b v; a=$(sha256sum "$(whl "$1")" | cut -c1-12); b=$(sha256sum "$(whl "$2")" | cut -c1-12)
  [ "$a" = "$b" ] && v=IGUALES || v=DISTINTOS; printf '%-74s %s | %s => %s\n' "$3" "$a" "$b" "$v"
}
# SOURCE_DATE_EPOCH=1767225600 es la fecha del commit (git log -1 --pretty=%ct)
build st_a setuptools /w/a; build st_b setuptools /home/x/otra
SDE=1767225600 build st_c setuptools /w/a; SDE=1767225600 build st_d setuptools /home/x/otra
build ha_a hatchling /w/a; build ha_b hatchling /home/x/otra
SBOM_REPRO=1 build hr_a hatchling /w/a; SBOM_REPRO=1 build hr_b hatchling /home/x/otra
echo
fila st_a st_b "(1) setuptools 84.0.0, sin SOURCE_DATE_EPOCH"
fila st_c st_d "(2) setuptools 84.0.0, con SOURCE_DATE_EPOCH=1767225600"
fila ha_a ha_b "(3) hatchling 1.32.4 (reproducible por defecto) + SBOM generado en la build"
fila hr_a hr_b "(4) hatchling + SBOM generado con --output-reproducible"
echo
python3 - "$(whl st_a)" "$(whl st_c)" "$(whl ha_a)" "$(whl hr_a)" "$(whl hr_b)" <<'PY'
import sys, zipfile
st_a, st_c, ha_a, hr_a, hr_b = sys.argv[1:]
for nombre, p in (("setuptools sin SDE", st_a), ("setuptools con SDE", st_c), ("hatchling", ha_a)):
    i = zipfile.ZipFile(p).infolist()[0]
    print(f"--- fecha de la primera entrada ({nombre}): {i.date_time}  {i.filename}")
z = zipfile.ZipFile(hr_a)
print("--- entradas .dist-info de la rueda hatchling (PEP 770 => sboms/):")
for n in z.namelist():
    if ".dist-info/" in n: print("   ", n)
meta = [n for n in z.namelist() if n.endswith("METADATA")][0]
print("--- METADATA (lo que el estándar deja grabar sobre el origen):")
for l in z.read(meta).decode().splitlines():
    if l.split(":")[0] in ("Metadata-Version", "Name", "Version", "Project-URL", "Requires-Dist", "Requires-Python"): print("   ", l)
w = [n for n in z.namelist() if n.endswith("/WHEEL")][0]
print("--- WHEEL:", " | ".join(z.read(w).decode().split("\n")[:3]))
a, b = zipfile.ZipFile(hr_a), zipfile.ZipFile(hr_b)
import json
for p in (hr_a,):
    s = json.loads(zipfile.ZipFile(p).read([n for n in zipfile.ZipFile(p).namelist() if n.endswith("sbom.cdx.json")][0]))
    print(f"--- SBOM embebido: specVersion={s['specVersion']} serialNumber={s.get('serialNumber','(omitido)')} timestamp={s['metadata'].get('timestamp','(omitido)')} componentes={[c['name']+'@'+c['version'] for c in s.get('components',[])]}")
PY
