#!/usr/bin/env bash
# E6-3 · Node.js: ¿npm pack da el mismo tarball en dos máquinas? ¿Qué SBOM saca npm del lock?
# Corre en el HOST. Usa node:24-alpine (la versión de npm queda impresa).
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
img=node:24-alpine
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
echo "imagen: $(docker image inspect $img --format '{{index .RepoDigests 0}}')"
nd() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e NPM_CONFIG_UPDATE_NOTIFIER=false -v "$1":"$2" -w "$2" $img sh -c "$3"; }
cp -r "$raiz/fuente-node" "$tmp/a"; sleep 2; cp -r "$raiz/fuente-node" "$tmp/b"; touch "$tmp/b/index.js"  # otra copia, otras fechas
nd "$tmp/a" /w/a          'echo "node $(node --version) / npm $(npm --version)"; npm pack --json 2>/dev/null' > "$tmp/pack-a.json"
nd "$tmp/b" /home/x/otra  'npm pack --json 2>/dev/null' > "$tmp/pack-b.json"
head -1 "$tmp/pack-a.json"
python3 - "$tmp" <<'PY'
import json, sys, hashlib, tarfile
t = sys.argv[1]
a = json.loads(open(t + "/pack-a.json").read().split("\n", 1)[1])[0]
b = json.loads(open(t + "/pack-b.json").read())[0]
print("=== (1) npm pack del mismo fuente, en dos rutas y con fechas de archivo distintas")
for k in ("filename", "shasum", "integrity"):
    print(f"   {k:10} {a[k][:40]:42} | {b[k][:40]:42} {'=' if a[k] == b[k] else '≠'}")
ta = tarfile.open(f"{t}/a/{a['filename']}")
print("   entradas del tarball y su fecha (mtime):")
for m in ta.getmembers():
    print(f"     {m.name:24} mtime={m.mtime}  modo={oct(m.mode)}")
PY
echo; echo "=== (2) SBOM desde el lock (sin node_modules): SPDX y CycloneDX"
nd "$tmp/a" /w/a 'npm sbom --sbom-format spdx --package-lock-only 2>&1' > "$tmp/sbom.spdx.json"
nd "$tmp/a" /w/a 'npm sbom --sbom-format cyclonedx --sbom-type application --package-lock-only 2>&1' > "$tmp/sbom.cdx.json"
python3 - "$tmp" <<'PY'
import json, sys
t = sys.argv[1]
s = json.load(open(t + "/sbom.spdx.json")); c = json.load(open(t + "/sbom.cdx.json"))
print(f"   SPDX      spdxVersion={s['spdxVersion']}  creado={s['creationInfo']['created']}  herramienta={s['creationInfo']['creators']}")
for p in s["packages"]:
    print(f"             paquete {p['name']}@{p.get('versionInfo')}  checksums={[x['algorithm'] for x in p.get('checksums', [])]}  purl={[r['referenceLocator'] for r in p.get('externalRefs', []) if r['referenceType']=='purl']}")
print(f"   CycloneDX specVersion={c['specVersion']}  timestamp={c['metadata']['timestamp']}  serialNumber={c.get('serialNumber','')[:20]}…")
for comp in [c["metadata"]["component"]] + c.get("components", []):
    print(f"             componente {comp['name']}@{comp['version']} tipo={comp['type']} hashes={[h['alg'] for h in comp.get('hashes', [])]} purl={comp.get('purl')}")
print("   ojo: en CycloneDX el 'name' del componente raíz sale del NOMBRE DEL DIRECTORIO (/w/a), no del package.json; el purl sí es correcto")
PY
echo; echo "=== (3) Dos SBOM del mismo lock, generados con segundos de diferencia: ¿mismos bytes?"
sleep 1; nd "$tmp/a" /w/a 'npm sbom --sbom-format cyclonedx --sbom-type application --package-lock-only 2>&1' > "$tmp/sbom2.cdx.json"
printf '   sha256 %s | %s\n' "$(sha256sum < "$tmp/sbom.cdx.json" | cut -c1-12)" "$(sha256sum < "$tmp/sbom2.cdx.json" | cut -c1-12)"
diff <(python3 -m json.tool "$tmp/sbom.cdx.json") <(python3 -m json.tool "$tmp/sbom2.cdx.json") | grep '^[<>]' | sed 's/^/   /'
