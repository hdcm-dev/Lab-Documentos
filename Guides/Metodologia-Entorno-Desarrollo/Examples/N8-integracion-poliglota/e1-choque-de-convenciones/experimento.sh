#!/usr/bin/env bash
# E8-1 — ¿Qué se rompe primero cuando .NET, Node y Python comparten repo?
# Toma la solución de referencia, le aplica una política GLOBAL (la que arregla
# C# en Linux: "UTF-8 con BOM para todo"; y el checkout de Windows sin
# .gitattributes: "CRLF para todo") y corre cada lector de los tres ecosistemas.
# Uso: ./experimento.sh   (requiere la imagen lab-n8-poliglota:local, ver README)
set -uo pipefail
AQUI=$(cd "$(dirname "$0")" && pwd)
IMG=${IMG:-lab-n8-poliglota:local}
W=$(mktemp -d); trap 'rm -rf "$W"' EXIT

cat > "$W/lectores.sh" <<'LECT'
#!/usr/bin/env bash
# Corre cada lector y resume: OK o la primera línea de error.
probar() { local desc=$1; shift; local out; out=$("$@" 2>&1); local rc=$?
  if [ $rc -eq 0 ]; then printf '  %-48s OK\n' "$desc"
  else printf '  %-48s FALLA rc=%s: %s\n' "$desc" "$rc" "$(echo "$out" | grep -m1 -iE 'error|not found|unexpected|invalid|failed|denied|esperado|BOM|No such' || echo "$out" | head -1)"; fi; }
cd /w
probar "sh: ./tools/hola.sh (shebang)"            ./tools/hola.sh
probar "python: ./tools/saluda.py (shebang)"       ./tools/saluda.py
probar "python: uv run python gen.py"              bash -c 'cd tools/gen && uv run --locked -q python gen.py /tmp/x.cs'
probar "python: json.load(datos.json, utf-8)"      python3 -c 'import json;json.load(open("tools/datos.json",encoding="utf-8"))'
probar "python: tomllib sobre pyproject.toml"      python3 -c 'import tomllib;tomllib.load(open("tools/gen/pyproject.toml","rb"))'
probar "uv: uv sync --locked (pyproject.toml)"     bash -c 'cd tools/gen && uv sync --locked -q'
probar "uv: lee .python-version"                   bash -c 'cd tools/gen && uv python find'
probar "node: JSON.parse(readFileSync(datos.json))" node -e 'JSON.parse(require("fs").readFileSync("tools/datos.json","utf8"))'
probar "node: require(datos.json)"                 node -e 'require("/w/tools/datos.json")'
probar "npm: npm ci (package.json)"                bash -c 'cd web && npm ci --no-audit --no-fund --loglevel=error'
probar "npm: npm run build (build.mjs)"            bash -c 'cd web && npm run build --silent'
probar "dotnet: lee global.json"                   dotnet --version
probar "dotnet: restore --locked-mode (lock)"      dotnet restore src/Saludo --locked-mode -v q --nologo
probar "dotnet: build (Program.cs + Datos.g.cs)"   dotnet build src/Saludo -v q --nologo -p:ConstruirPoliglota=false
probar "compuerta N3: .nvmrc contra node -v"       bash -c '[ "$(tr -d "[:space:]" < .nvmrc)" = "$(node -v | sed s/^v//)" ] || { echo "esperado $(tr -d "[:space:]" < .nvmrc | od -An -c | tr -s " ")"; exit 1; }'
LECT
chmod +x "$W/lectores.sh"

preparar() {  # copia limpia de la solución + dos archivos extra que la regla general debe servir
  rm -rf "$W/repo"; cp -a "$AQUI/../solucion" "$W/repo"
  printf '#!/bin/sh\necho "hola desde sh"\n' > "$W/repo/tools/hola.sh"
  printf '{"año": 2026, "acción": "construir"}\n' > "$W/repo/tools/datos.json"
  printf '#!/usr/bin/env python3\nprint("hola desde python")\n' > "$W/repo/tools/saluda.py"
  chmod +x "$W/repo/tools/hola.sh" "$W/repo/tools/saluda.py"
}
correr() {  # pre-genera las salidas de Node/Python con la copia limpia para aislar al lector
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$W/repo:/w" -v "$W/lectores.sh:/lectores.sh:ro" "$IMG" /lectores.sh
}
con_bom() { for f in "$@"; do printf '\xef\xbb\xbf' | cat - "$f" > "$f.tmp" && chmod --reference="$f" "$f.tmp" && mv "$f.tmp" "$f"; done; }
con_crlf() { for f in "$@"; do sed -i 's/$/\r/' "$f"; done; }

ARCH="tools/hola.sh tools/saluda.py tools/gen/gen.py tools/datos.json tools/gen/pyproject.toml tools/gen/.python-version web/package.json web/build.mjs global.json src/Saludo/packages.lock.json src/Saludo/Program.cs .nvmrc"

echo "### A. Política de referencia (UTF-8 sin BOM, LF; BOM sólo .razor/.cshtml/.sln)"
preparar
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$W/repo:/w" -w /w "$IMG" bash -c '(cd tools/gen && uv run --locked -q python gen.py dist/Datos.g.cs >/dev/null); (cd web && npm ci -s && npm run build -s >/dev/null)'
correr
docker run --rm --user "$(id -u):$(id -g)" -v "$W/repo:/w" -w /w "$IMG" python3 verificar-texto.py . | tail -1

echo; echo "### B. «UTF-8 con BOM para todo» (la corrección del caso C#, aplicada a ciegas)"
cp -a "$W/repo/tools/gen/dist" "$W/dist-gen"; cp -a "$W/repo/web/dist" "$W/dist-web"
preparar; cp -a "$W/dist-gen" "$W/repo/tools/gen/dist"; cp -a "$W/dist-web" "$W/repo/web/dist"
(cd "$W/repo" && con_bom $ARCH)
correr
docker run --rm --user "$(id -u):$(id -g)" -v "$W/repo:/w" -w /w "$IMG" python3 verificar-texto.py . | tail -4

echo; echo "### C. «CRLF para todo» (clon en Windows con core.autocrlf=true y sin .gitattributes)"
preparar; cp -a "$W/dist-gen" "$W/repo/tools/gen/dist"; cp -a "$W/dist-web" "$W/repo/web/dist"
(cd "$W/repo" && con_crlf $ARCH)
correr
docker run --rm --user "$(id -u):$(id -g)" -v "$W/repo:/w" -w /w "$IMG" python3 verificar-texto.py . | tail -4

echo; echo "### D. Código heredado: Program.cs y datos.json en cp1252, sin BOM (el caso disparador)"
preparar; cp -a "$W/dist-gen" "$W/repo/tools/gen/dist"; cp -a "$W/dist-web" "$W/repo/web/dist"
(cd "$W/repo" && for f in src/Saludo/Program.cs tools/datos.json; do iconv -f UTF-8 -t CP1252 "$f" > "$f.tmp" && mv "$f.tmp" "$f"; done)
correr
docker run --rm --user "$(id -u):$(id -g)" -v "$W/repo:/w" -w /w "$IMG" python3 verificar-texto.py . | tail -3

echo; echo "### E. Detalle de dos «OK» engañosos de la tabla (rc=0 con daño)"
rm -rf "$W/det"; mkdir -p "$W/det"
printf '\xef\xbb\xbf#!/bin/sh\necho "hola desde sh"\n' > "$W/det/hola.sh"; chmod +x "$W/det/hola.sh"
printf '{"a\xf1o": 2026}\n' > "$W/det/d.json"
docker run --rm --user "$(id -u):$(id -g)" -v "$W/det:/w" -w /w "$IMG" bash -c '
echo "--- bash ejecuta ./hola.sh con BOM (el kernel no ve el shebang; bash lo interpreta por su cuenta):"; ./hola.sh; echo "rc=$?"
echo "--- node lee un JSON en cp1252 como UTF-8 (no falla: reemplaza el byte por U+FFFD):"; node -e "console.log(JSON.parse(require(\"fs\").readFileSync(\"d.json\",\"utf8\")))"; echo "rc=$?"' 2>&1
