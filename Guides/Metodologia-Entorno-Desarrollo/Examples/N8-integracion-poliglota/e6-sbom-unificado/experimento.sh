#!/usr/bin/env bash
# E8-6 — ¿Cómo se generan SBOM y procedencia que cubran los tres ecosistemas?
#  1) una herramienta multi-ecosistema sobre el repo (syft);
#  2) el exportador nativo de cada ecosistema + fusión (uv, npm, CycloneDX .NET, cyclonedx-cli);
#  3) BuildKit: procedencia SLSA v1 + SBOM del artefacto y de las etapas (Dockerfile.por-componente).
set -uo pipefail
AQUI=$(cd "$(dirname "$0")" && pwd)
IMG=${IMG:-lab-n8-poliglota:local}
SYFT=${SYFT:-anchore/syft:v1.54.1}
CDXCLI=${CDXCLI:-cyclonedx/cyclonedx-cli:0.33.1}
PY15=python:3.12.15-slim@sha256:05cda9777409a9c3ffddd94a4c476b79f0769a0b4857f0c7ed9226b6800b0d6f
W=$(mktemp -d); trap 'rm -rf "$W"' EXIT
cp -a "$AQUI/../solucion" "$W/repo"; mkdir -p "$W/out" "$W/attest"
U="--user $(id -u):$(id -g)"

echo "### 1. syft $SYFT sobre el repositorio (lee los tres lock files)"
docker run --rm $U -e HOME=/tmp -v "$W/repo:/src:ro" -v "$W/out:/out" "$SYFT" scan dir:/src --source-name solucion --source-version 1.0.0 -q -o cyclonedx-json=/out/syft.cdx.json
python3 "$AQUI/resumir.py" "$W/out/syft.cdx.json"

echo; echo "### 2. Exportador nativo por ecosistema, fusionado"
docker run --rm $U -e HOME=/tmp -v "$W/repo:/w" -v "$W/out:/out" -w /w "$IMG" bash -c '
 (cd tools/gen && uv export --locked --format cyclonedx1.5 -q -o /out/uv.cdx.json)
 (cd web && npm ci -s && npm sbom --sbom-format cyclonedx > /out/npm.cdx.json)
 dotnet tool install --tool-path /tmp/t CycloneDX --version 6.2.0 >/dev/null 2>&1
 /tmp/t/dotnet-CycloneDX src/Saludo/Saludo.csproj -o /out -fn dotnet.cdx.json -F Json -spv 1.5 >/dev/null 2>&1; echo "dotnet-CycloneDX rc=$?"' 
python3 "$AQUI/resumir.py" "$W/out/uv.cdx.json" "$W/out/npm.cdx.json" "$W/out/dotnet.cdx.json"
docker run --rm $U -v "$W/out:/out" "$CDXCLI" merge --input-files /out/uv.cdx.json /out/npm.cdx.json /out/dotnet.cdx.json \
  --output-file /out/fusion.cdx.json --hierarchical --name solucion --version 1.0.0 >/dev/null 2>&1; echo "cyclonedx-cli merge rc=$?"
python3 "$AQUI/resumir.py" "$W/out/fusion.cdx.json"

echo; echo "### 3. BuildKit: procedencia + SBOM del build por componentes (builder docker-container)"
docker buildx inspect n8-attest >/dev/null 2>&1 || docker buildx create --name n8-attest --driver docker-container >/dev/null
sed -e 's|^FROM ${PY_IMAGE} AS py$|FROM ${PY_IMAGE} AS py\nARG BUILDKIT_SBOM_SCAN_STAGE=true|' \
    -e 's|^FROM ${NODE_IMAGE} AS web$|FROM ${NODE_IMAGE} AS web\nARG BUILDKIT_SBOM_SCAN_STAGE=true|' \
    "$AQUI/../imagen/Dockerfile.por-componente" > "$W/Dockerfile"
docker buildx build --builder n8-attest -q --target artefacto --build-arg PY_IMAGE=$PY15 \
  --provenance=mode=max --sbom=true -f "$W/Dockerfile" --output type=local,dest="$W/attest" "$W/repo" >/dev/null 2>&1; echo "buildx rc=$?"
echo "archivos exportados: $(cd "$W/attest" && ls | tr '\n' ' ')"
echo "sha256 Saludo.dll = $(sha256sum "$W/attest/Saludo.dll" | cut -d' ' -f1)"
python3 - "$W/attest/provenance.json" <<'PY'
import json, sys
p = json.load(open(sys.argv[1])); pred = p["predicate"]
print("procedencia:", p["_type"], "|", p["predicateType"])
print("buildType:", pred["buildDefinition"]["buildType"])
for d in pred["buildDefinition"].get("resolvedDependencies", []):
    print("  material:", d["uri"].split("?")[0], "sha256:" + d["digest"]["sha256"][:16] + "…")
PY
python3 "$AQUI/resumir.py" "$W/attest/sbom.spdx.json" "$W/attest/sbom-py.spdx.json" "$W/attest/sbom-web.spdx.json"
