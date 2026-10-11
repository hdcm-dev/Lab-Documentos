#!/usr/bin/env bash
# E6-6 · SBOM de una pieza .NET con dos herramientas (CycloneDX y SPDX) y qué responde cada una.
# Corre en el HOST. Copia la pieza, le agrega UNA dependencia NuGet fijada y genera los dos SBOM.
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
"$raiz/comun/preparar-repo.sh" "$raiz/fuente-dotnet" "$tmp/src" >/dev/null
sed -i 's#</Project>#  <ItemGroup><PackageReference Include="Newtonsoft.Json" Version="[13.0.3]" /></ItemGroup>\n</Project>#' "$tmp/src/Pieza.csproj"
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -v "$tmp/src":/src -v "$aqui/dentro.sh":/dentro.sh:ro -w /src mcr.microsoft.com/dotnet/sdk:8.0 bash /dentro.sh
python3 - "$tmp/src" <<'PY'
import json, sys, glob
d = sys.argv[1]
c = json.load(open(f"{d}/sbom/pieza.cdx.json"))
print(f"=== CycloneDX (dotnet-CycloneDX 6.2.0): specVersion={c['specVersion']}")
print(f"    metadata.timestamp={c['metadata'].get('timestamp')}  herramienta={[t.get('name') for t in (c['metadata'].get('tools',{}).get('components',[]) if isinstance(c['metadata'].get('tools'),dict) else c['metadata'].get('tools',[]))]}")
for comp in c.get("components", []):
    print(f"    componente {comp['name']}@{comp['version']}  purl={comp.get('purl')}  hashes={[h['alg'] for h in comp.get('hashes',[])]}")
print(f"    dependencias declaradas: {len(c.get('dependencies', []))}")
s = json.load(open(glob.glob(f"{d}/drop/_manifest/spdx_2.2/manifest.spdx.json")[0]))
print(f"=== SPDX (sbom-tool 4.1.13): spdxVersion={s['spdxVersion']}  creado={s['creationInfo']['created']}  creadores={s['creationInfo']['creators']}")
print(f"    files (lo que hay en la carpeta de salida, con hash):")
for f in s.get("files", []):
    print(f"      {f['fileName']:32} {[x['algorithm'] for x in f.get('checksums', [])]}")
print(f"    packages:")
for p in s.get("packages", []):
    purl = [r['referenceLocator'] for r in p.get('externalRefs', []) if r.get('referenceType') == 'purl']
    print(f"      {p['name']}@{p.get('versionInfo')}  purl={purl[:1]}")
print("    relación raíz:", [r['relationshipType'] for r in s.get('relationships', [])][:4], "…")
PY
