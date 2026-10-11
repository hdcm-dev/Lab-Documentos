#!/usr/bin/env bash
# E6-5 · Procedencia SLSA v1 real (BuildKit) + firma y verificación con cosign. Qué prueba y qué no.
# Corre en el HOST. Crea un builder BuildKit temporal (docker-container) y lo borra al terminar.
# Usa un DOCKER_CONFIG vacío para bajar imágenes públicas en forma anónima (no toca la configuración del usuario).
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
tmp=$(mktemp -d); export DOCKER_CONFIG="$tmp/docker-config"; mkdir -p "$DOCKER_CONFIG/cli-plugins"
for p in "$HOME/.docker/cli-plugins/docker-buildx" /usr/libexec/docker/cli-plugins/docker-buildx; do
  [ -x "$p" ] && { ln -s "$p" "$DOCKER_CONFIG/cli-plugins/docker-buildx"; break; }; done
builder=lab-n6-$$
trap 'docker buildx rm "$builder" >/dev/null 2>&1; [ -n "${CONSERVAR:-}" ] && cp "$tmp/out/provenance.json" "$CONSERVAR"; rm -rf "$tmp"' EXIT
COSIGN=ghcr.io/sigstore/cosign/cosign:v3.1.3
sha=$("$raiz/comun/preparar-repo.sh" "$raiz/fuente-dotnet" "$tmp/repo")
echo "commit: $sha   buildx: $(docker buildx version | cut -d' ' -f2)   cosign: $COSIGN"
docker buildx create --name "$builder" --driver docker-container >/dev/null
echo; echo "=== (1) build con procedencia SLSA (mode=max) y SBOM, exportado a un directorio"
docker buildx build --builder "$builder" -q --provenance=mode=max --sbom=true \
  -f "$aqui/Dockerfile" --output "type=local,dest=$tmp/out" "$tmp/repo" >/dev/null 2>"$tmp/bx.log" || { tail -5 "$tmp/bx.log"; exit 1; }
ls "$tmp/out" | sed 's/^/   /'
echo; echo "=== (2) Qué dice la procedencia"
python3 - "$tmp/out" <<'PY'
import json, sys, hashlib
d = sys.argv[1]; p = json.load(open(f"{d}/provenance.json"))
print(f"   _type={p['_type']}  predicateType={p['predicateType']}")
for s in p["subject"]:
    real = hashlib.sha256(open(f"{d}/{s['name']}", "rb").read()).hexdigest()
    print(f"   subject {s['name']:10} sha256={s['digest']['sha256'][:16]}…  coincide con el archivo: {real == s['digest']['sha256']}")
bd, rd = p["predicate"]["buildDefinition"], p["predicate"]["runDetails"]
print(f"   buildType={bd['buildType']}")
for r in bd.get("resolvedDependencies", []):
    print(f"   resolvedDependency {r['uri'][:70]}  sha256={r['digest'].get('sha256','')[:16]}…")
print(f"   externalParameters.configSource={bd['externalParameters'].get('configSource')}")
print(f"   runDetails.builder={rd.get('builder')}")
print(f"   runDetails.metadata: invocationId={rd['metadata'].get('invocationId')} startedOn={rd['metadata'].get('startedOn')}")
PY
python3 "$aqui/buscar-y-editar.py" buscar "$tmp/out/provenance.json" "$sha"
echo "   SBOM de la imagen: $(python3 -c "import json,sys; s=json.load(open(sys.argv[1])); print(s['predicateType'], '-', len(s['predicate'].get('packages',[])), 'paquetes')" "$tmp/out/sbom.spdx.json" 2>/dev/null)"
echo; echo "=== (3) ¿Es el mismo binario que una build independiente del mismo commit? (docker run, otra ruta)"
"$raiz/comun/preparar-repo.sh" "$raiz/fuente-dotnet" "$tmp/otro" >/dev/null
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e CI=true -e DOTNET_NOLOGO=1 \
  -v "$tmp/otro":/home/x/otra -w /home/x/otra mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743 \
  dotnet build -c Release -nologo -v q >/dev/null 2>&1
printf '   BuildKit : %s\n   docker run: %s\n' "$(sha256sum "$tmp/out/Pieza.dll" | cut -c1-16)" "$(sha256sum "$tmp/otro/bin/Release/net8.0/Pieza.dll" | cut -c1-16)"
echo; echo "=== (4) Procedencia SIN firma: cualquiera la edita y nada lo detecta"
cp "$tmp/out/provenance.json" "$tmp/prov-editada.json"
python3 "$aqui/buscar-y-editar.py" editar "$tmp/prov-editada.json" "https://ci.ejemplo.invalid/runner-oficial"
echo; echo "=== (5) Firmar la procedencia (envoltorio DSSE en un bundle de Sigstore) y verificar"
python3 -c "import json,sys; json.dump(json.load(open(sys.argv[1]))['predicate'], open(sys.argv[2],'w'))" "$tmp/out/provenance.json" "$tmp/out/predicado.json"
C="docker run --rm --user $(id -u):$(id -g) -e COSIGN_PASSWORD= -e HOME=/tmp -v $tmp/out:/w -w /w $COSIGN"
$C generate-key-pair >/dev/null 2>&1
# Sin log de transparencia ni servicio de firma: firma offline con clave local (sólo para el laboratorio).
$C attest-blob --predicate predicado.json --type slsaprovenance1 --key cosign.key --bundle Pieza.dll.sigstore.json \
   --use-signing-config=false --tlog-upload=false -y Pieza.dll >/dev/null 2>&1 && echo "   firmado: Pieza.dll.sigstore.json"
v() { $C verify-blob-attestation --key cosign.pub --bundle Pieza.dll.sigstore.json --type slsaprovenance1 --insecure-ignore-tlog "$1" 2>&1 | grep -v WARNING | tail -1; }
echo "   (a) artefacto original           -> $(v Pieza.dll)"
cp "$tmp/out/Pieza.dll" "$tmp/out/Pieza-alterada.dll"; printf '\0' >> "$tmp/out/Pieza-alterada.dll"
echo "   (b) artefacto con 1 byte de más  -> $(v Pieza-alterada.dll)"
python3 - "$tmp/out/Pieza.dll.sigstore.json" <<'PY'
import json, sys, base64
b = json.load(open(sys.argv[1])); env = b["dsseEnvelope"]
st = json.loads(base64.b64decode(env["payload"]))
st["predicate"]["runDetails"]["builder"]["id"] = "https://ci.ejemplo.invalid/runner-oficial"
env["payload"] = base64.b64encode(json.dumps(st).encode()).decode()
json.dump(b, open(sys.argv[1], "w"))
PY
echo "   (c) procedencia editada en el bundle -> $(v Pieza.dll)"
