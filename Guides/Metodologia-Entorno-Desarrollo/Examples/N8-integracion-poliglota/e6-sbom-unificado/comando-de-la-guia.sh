#!/usr/bin/env bash
# E8-6, ampliación (mesa, ciclo 1): el bloque de la guía §11.5, TAL COMO ESTÁ ESCRITO, en una máquina
# sin el builder n8-attest. Se corre desde la carpeta de N8:  bash e6-sbom-unificado/comando-de-la-guia.sh
# Deja salida/ en la carpeta de N8 mientras corre y la borra al final (lo que importa es el resumen).
set -uo pipefail
cd "$(dirname "$0")/.." || exit 1
docker buildx rm n8-attest >/dev/null 2>&1; echo "builder n8-attest antes de empezar: $(docker buildx inspect n8-attest >/dev/null 2>&1 && echo existe || echo no existe)"
rm -rf salida
set -x
# Una vez por máquina: un builder docker-container, que exporta procedencia y SBOM (§9.2.4).
docker buildx inspect n8-attest >/dev/null 2>&1 || docker buildx create --name n8-attest --driver docker-container
docker buildx build --builder n8-attest --target artefacto \
  --build-arg PY_IMAGE=python:3.12.15-slim@sha256:05cda9777409a9c3ffddd94a4c476b79f0769a0b4857f0c7ed9226b6800b0d6f \
  --provenance=mode=max,version=v1 --sbom=true \
  -f imagen/Dockerfile.por-componente --output type=local,dest=salida solucion
{ rc=$?; set +x; } 2>/dev/null
echo "docker buildx build rc=$rc"
echo "archivos exportados: $(ls salida | tr '\n' ' ')"
python3 - salida/provenance.json <<'PY'
import json, sys
p = json.load(open(sys.argv[1])); pred = p["predicate"]
print("procedencia:", p["_type"], "|", p["predicateType"])
print("builder.id:", repr(pred.get("runDetails", {}).get("builder", {}).get("id")))
for d in pred["buildDefinition"].get("resolvedDependencies", []):
    print("  material:", d["uri"].split("?")[0], "sha256:" + d["digest"]["sha256"][:16] + "…")
PY
rm -rf salida
exit $rc
