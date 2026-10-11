#!/usr/bin/env bash
# E6-1 · Qué metadatos de procedencia graba el SDK de .NET en la pieza, cuáles no, y cuándo mienten.
# Corre en el HOST (necesita docker, git y python3). No deja nada en el repo: trabaja en un directorio temporal.
set -u
aqui=$(cd "$(dirname "$0")" && pwd); raiz=$(cd "$aqui/.." && pwd)
img=mcr.microsoft.com/dotnet/sdk:8.0
dig=$(docker image inspect "$img" --format '{{index .RepoDigests 0}}' | sed 's/.*@//')
tmp=$(mktemp -d); trap 'rm -rf "$tmp"' EXIT
sha=$("$raiz/comun/preparar-repo.sh" "$raiz/fuente-dotnet" "$tmp/repo")
echo "commit del repo de juguete: $sha"
echo "imagen: $img@$dig"
dn() { # corre dotnet dentro de la imagen, con el usuario del host, sobre el repo temporal
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 \
    -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e CI=true -v "$tmp/repo":/src -w /src "$img" bash -c "$1"
}
echo; echo "=== (1) build + pack como en la CI (CI=true => ContinuousIntegrationBuild), con el digest de la imagen"
dn "dotnet pack -c Release -o /src/out -p:ImagenDigest=$dig -nologo -v q >/tmp/b.log || { tail -5 /tmp/b.log; exit 1; }; dotnet bin/Release/net8.0/Pieza.dll"
echo; echo "=== (2) SourceLink: el mapa ruta->URL va dentro del PDB (bloque JSON legible)"
grep -a -o '{"documents":{[^}]*}}' "$tmp/repo/bin/Release/net8.0/Pieza.pdb" || echo "(no hay SourceLink en el PDB)"
echo; echo "=== (3) El .nuspec del paquete lleva repositorio y commit (PublishRepositoryUrl)"
python3 - "$tmp/repo/out" <<'PY'
import sys, zipfile, glob, re
for p in sorted(glob.glob(sys.argv[1] + "/*nupkg")):
    z = zipfile.ZipFile(p)
    nuspec = [n for n in z.namelist() if n.endswith(".nuspec")][0]
    txt = z.read(nuspec).decode()
    repo = re.search(r"<repository[^>]*/>", txt)
    print(f"{p.split('/')[-1]:28} {repo.group(0) if repo else '(sin <repository>)'}")
    print(f"{'':28} contenido: {', '.join(n for n in z.namelist() if n.endswith(('.dll','.pdb')))}")
PY
echo; echo "=== (4) Árbol con cambios SIN commitear: la versión informativa sigue diciendo el mismo SHA"
h1=$(sha256sum "$tmp/repo/bin/Release/net8.0/Pieza.dll" | cut -c1-16)
sed -i 's/Math.PI \* radio \* radio/3.14 * radio * radio/' "$tmp/repo/Program.cs"
dn "dotnet build -c Release -nologo -v q >/tmp/b.log || { tail -5 /tmp/b.log; exit 1; }; dotnet bin/Release/net8.0/Pieza.dll | head -1; git status --porcelain"
h2=$(sha256sum "$tmp/repo/bin/Release/net8.0/Pieza.dll" | cut -c1-16)
echo "sha256 Pieza.dll del commit limpio : $h1"
echo "sha256 Pieza.dll con cambio local  : $h2  => el binario cambió y el metadato no lo dice"
