#!/usr/bin/env bash
# EA-1 — Las plantillas del Anexo A funcionan juntas y cumplen su propia política.
# Copia repo/ a un directorio temporal (no deja bin/, obj/, node_modules/, .venv/)
# y corre los pasos 1 a 10, cada uno en el contenedor de su ecosistema.
# Requisitos: Docker; imágenes de los laboratorios N2 (lab-n2-texto:local) y
# N8 (lab-n8-poliglota:local), más mcr.microsoft.com/dotnet/sdk:8.0, node:24-bookworm-slim
# y python:3.12-slim; para el paso 10, Node (npx) en el host. Necesita red (nuget.org, npm, PyPI,
# json.schemastore.org, Docker Hub).
# Uso, desde esta carpeta:  bash verificar.sh | tee salidas/EA-1-plantillas.txt
set -u
AQUI="$(cd "$(dirname "$0")" && pwd)"
EX="${EXAMPLES:-$(cd "$AQUI/.." && pwd)}"
COMPUERTA="${COMPUERTA:-$EX/N7-compuertas-y-gobierno/compuerta/compuerta.sh}"
T="$(mktemp -d)"; trap 'rm -rf "$T"; docker rmi -f ea1-imagen:local >/dev/null 2>&1' EXIT
cp -a "$AQUI/repo/." "$T/"
U="--user $(id -u):$(id -g) -e HOME=/tmp"

echo "== 1. Política de texto: qué resuelve .editorconfig y qué aplica .gitattributes"
docker run --rm $U -v "$T":/w -v "$EX/N2-politica-de-texto-y-remediacion/tools":/tools:ro -w /w lab-n2-texto:local bash -c '
  git init -q -b main . && git config user.email lab@ejemplo.invalid && git config user.name lab
  git add -A && git commit -qm plantillas
  for f in src/App/Program.cs Pages/X.razor App.sln tools/x.ps1 build.cmd tools/x.sh \
           src/App/packages.lock.json web/package-lock.json tools/gen/uv.lock; do
    ec=$(editorconfig "/w/$f" | grep -E "^(charset|end_of_line|insert_final_newline)=" | tr "\n" " ")
    ga=$(git check-attr text eol -- "$f" | sed "s/^[^:]*: //" | tr "\n" " ")
    printf "  %-28s editorconfig: %-62s gitattributes: %s\n" "$f" "$ec" "$ga"
  done
  echo "-- compuerta de texto de N2 sobre las propias plantillas:"
  python3 /tools/check_text_policy.py && echo "  rc=0"
  echo "== 1b. Un dotnet new console dentro del repo, con la variante A (*.cs sin BOM):"
  dotnet new console -o tmp/Nueva --framework net8.0 --no-restore >/dev/null 2>&1 && git add -A tmp
  python3 /tools/check_text_policy.py --staged 2>&1 | sed "s/^/  /"; echo "  rc=${PIPESTATUS[0]} (se esperan 2 violaciones: Program.cs y Nueva.csproj con BOM)"
  for f in tmp/Nueva/Program.cs tmp/Nueva/Nueva.csproj; do sed -i "1s/^\xEF\xBB\xBF//" "$f"; done
  git add -A tmp && echo "-- normalizados (sin BOM) antes del primer commit:"
  python3 /tools/check_text_policy.py --staged 2>&1 | sed "s/^/  /"; echo "  rc=${PIPESTATUS[0]}"
  git rm -rqf --cached tmp && rm -rf tmp'

echo "== 1c. Variante B (A.1) sobre una solución creada por el SDK y convertida con BOM"
docker run --rm $U -v "$AQUI/repo":/r:ro -v "$EX/N2-politica-de-texto-y-remediacion/tools":/tools:ro lab-n2-texto:local bash -c '
  W=/tmp/b && mkdir -p $W && cd $W && git init -q -b main .
  dotnet new sln -n Heredado >/dev/null; dotnet new console -n App -o src/App >/dev/null; dotnet new classlib -n Lib -o src/Lib >/dev/null
  for f in $(find src -name "*.cs" -o -name "*.csproj"); do head -c3 "$f" | od -An -tx1 | grep -q "ef bb bf" || { printf "\xef\xbb\xbf" | cat - "$f" > t && mv t "$f"; }; done
  cp /r/.editorconfig /r/.gitattributes /r/.gitignore /r/Directory.Build.props /r/Directory.Build.targets /r/Directory.Packages.props .
  sed -i "s/^# \[\*\.{cs,csproj,props,targets}\]$/[*.{cs,csproj,props,targets}]/; s/^# charset = utf-8-bom$/charset = utf-8-bom/" .editorconfig
  git add -A; python3 /tools/check_text_policy.py 2>&1 | sed "s/^/  /"; echo "  rc=${PIPESTATUS[0]} (se esperan 3: los Directory.Build.* y Directory.Packages.props sin BOM)"
  for f in Directory.Build.props Directory.Build.targets Directory.Packages.props; do printf "\xef\xbb\xbf" | cat - "$f" > t && mv t "$f"; done
  git add -A; python3 /tools/check_text_policy.py 2>&1 | sed "s/^/  /"; echo "  rc=${PIPESTATUS[0]}"'

echo "== 2. .NET: la condición de CI de Directory.Build.props"
docker run --rm $U -v "$T":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 bash -c '
  dotnet --version
  for caso in "local:" "github:CI=true" "gitlab:CI=true" "azure:TF_BUILD=True"; do
    nombre=${caso%%:*}; var=${caso#*:}
    salida=$(env $var dotnet msbuild src/App/App.csproj -nologo \
      -getProperty:ContinuousIntegrationBuild -getProperty:RestoreLockedMode \
      -getProperty:RestorePackagesWithLockFile -getProperty:LangVersion 2>/dev/null | tr -d "\n " | sed "s/^[^{]*//")
    printf "  %-7s %-16s %s\n" "$nombre" "${var:-(sin variable)}" "$salida"
  done
  echo "== 2b. Lock sólo en los ejecutables (Directory.Build.targets): App (Exe) y una biblioteca Lib"
  dotnet new classlib -o src/Lib --framework net8.0 --no-restore >/dev/null 2>&1
  printf "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net8.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" />\n  </ItemGroup>\n</Project>\n" > src/Lib/Lib.csproj
  for p in App Lib; do dotnet restore src/$p -v q >/dev/null 2>&1; echo "  restore local $p rc=$?; packages.lock.json: $( [ -f src/$p/packages.lock.json ] && echo SI || echo NO )"; done
  printf "  último byte del lock de App: %s\n" "$(tail -c1 src/App/packages.lock.json | od -An -c | tr -d " ")"
  CI=true dotnet restore src/App -v q >/dev/null && echo "  CI=true restore --locked rc=0"
  sed -i "s/13.0.3/13.0.1/" Directory.Packages.props
  CI=true dotnet restore src/App 2>&1 | grep -o "error NU1004: .*" | head -1 | cut -c1-140
  sed -i "s/13.0.1/13.0.3/" Directory.Packages.props
  rm -rf src/Lib
  echo "  CI=true dotnet run: $(CI=true dotnet run --project src/App 2>/dev/null | tail -1)"'

echo "== 2c. Un WinExe (WinForms) recibe lock con A.4b"
docker run --rm $U -v "$T":/w -w /w mcr.microsoft.com/dotnet/sdk:8.0 bash -c '
  mkdir -p src/Escritorio && printf "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <OutputType>WinExe</OutputType>\n    <TargetFramework>net8.0-windows</TargetFramework>\n    <UseWindowsForms>true</UseWindowsForms>\n    <EnableWindowsTargeting>true</EnableWindowsTargeting>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" />\n  </ItemGroup>\n</Project>\n" > src/Escritorio/Escritorio.csproj
  dotnet restore src/Escritorio -v q >/dev/null; echo "  restore local rc=$?; packages.lock.json: $([ -f src/Escritorio/packages.lock.json ] && echo SI || echo NO)"
  CI=true dotnet msbuild src/Escritorio -nologo -getProperty:OutputType -getProperty:RestorePackagesWithLockFile | tr -d "\n " | sed "s/^/  /"; echo
  rm -rf src/Escritorio'

echo "== 3. Node: devEngines + engine-strict"
for img in node:24-bookworm-slim lab-n8-poliglota:local; do
  docker run --rm $U -v "$T/web":/w -w /w $img sh -c '
    printf "  %s npm %s: " "$(node -v)" "$(npm -v)"
    npm install --no-audit --no-fund >/tmp/o 2>&1; rc=$?
    echo "rc=$rc $(grep -o "EBADDEVENGINES\|EBADENGINE" /tmp/o | head -1)"; rm -rf node_modules package-lock.json'
done

echo "== 4. Python: pyproject.toml + .python-version con uv"
docker run --rm $U -v "$T/tools/gen":/w -w /w lab-n8-poliglota:local sh -c '
  uv lock -q && uv sync --locked -q && uv run --locked python -c "import sys,tomllib;tomllib.load(open(\"pyproject.toml\",\"rb\"));print(\"  python\",sys.version.split()[0],\"pyproject OK\")"
  uv lock --check -q && echo "  uv lock --check rc=0"; rm -rf .venv uv.lock'

echo "== 5. JSON con comentarios y YAML: sintaxis y esquema de dependabot.yml"
docker run --rm $U -v "$T":/w -w /w python:3.12-slim bash -c '
  python3 -m venv /tmp/v >/dev/null && /tmp/v/bin/pip install -q --disable-pip-version-check jsonschema==4.26.0 pyyaml==6.0.3
  /tmp/v/bin/python - <<PY
import json, os, re, urllib.request, yaml, jsonschema
txt = open(".devcontainer/devcontainer.json", encoding="utf-8").read()
datos = json.loads(re.sub(r"^\s*//.*$", "", txt, flags=re.M))
df = os.path.normpath(os.path.join(".devcontainer", datos["build"]["dockerfile"]))
pv = open("tools/gen/.python-version", encoding="utf-8").read().strip()
print("  devcontainer.json: JSONC válido; build.dockerfile =", df, "existe:", os.path.exists(df),
      "| PYTHON_VERSION igual a tools/gen/.python-version:", datos["build"]["args"]["PYTHON_VERSION"] == pv)
for f in ["global.json", "web/package.json"]:
    json.load(open(f, encoding="utf-8")); print(f"  {f}: JSON válido")
yaml.safe_load(open(".github/workflows/ci.yml", encoding="utf-8")); print("  ci.yml: YAML válido")
dep = yaml.safe_load(open(".github/dependabot.yml", encoding="utf-8"))
esq = json.load(urllib.request.urlopen("https://json.schemastore.org/dependabot-2.0.json", timeout=60))
err = list(jsonschema.Draft7Validator(esq).iter_errors(dep))
print("  dependabot.yml contra el esquema comunitario de SchemaStore:", "VÁLIDO" if not err else err[:3],
      "| ecosistemas:", ", ".join(u["package-ecosystem"] for u in dep["updates"]))
PY'

echo "== 6. Imagen de A.10 (imagen/Dockerfile): se construye y corre la plantilla web/ adentro"
if docker build -q -t ea1-imagen:local --build-arg PYTHON_VERSION="$(cat "$T/tools/gen/.python-version")" "$T/imagen" >/dev/null 2>"$T/build.err"; then
  echo "  docker build rc=0"
  docker run --rm $U -v "$T":/w -w /w ea1-imagen:local sh -c '
    echo "  node $(node -v), npm $(npm -v), $(python3 -V), uv $(uv --version | cut -d" " -f2), dotnet $(dotnet --version), glibc $(ldd --version | head -1 | grep -oE "[0-9]+\.[0-9]+$")"
    cd web && npm install --no-audit --no-fund >/tmp/o 2>&1; echo "  web/: npm install rc=$? $(grep -o "EBADDEVENGINES" /tmp/o | head -1)"; npm run -s build; rm -rf node_modules package-lock.json'
else
  echo "  docker build FALLÓ:"; tail -5 "$T/build.err" | sed 's/^/  /'
fi

echo "== 7. El hook de A.11 con el esqueleto de compuerta rechaza un .cs en cp1252"
docker run --rm $U -v "$T":/w -w /w lab-n2-texto:local bash -c '
  rm -rf .git && git init -q -b main . && git config user.email lab@ejemplo.invalid && git config user.name lab
  git add -A && git commit -qm plantillas && git config core.hooksPath .githooks
  printf "class C { string s = \"Atenci\363n\"; }\n" > src/App/Cp1252.cs && git add src/App/Cp1252.cs
  git commit -qm "cp1252" 2>&1 | sed "s/^/  /"; echo "  git commit rc=${PIPESTATUS[0]} (el esqueleto falla a propósito hasta reemplazarlo)"'

echo "== 8. La compuerta completa de A.11, dentro de la imagen de A.10, sobre las plantillas con sus locks"
T8=$(mktemp -d); cp -a "$AQUI/repo/." "$T8/"
cp "$COMPUERTA" "$T8/tools/ci/compuerta.sh"; cp "$EX/N2-politica-de-texto-y-remediacion/tools/check_text_policy.py" "$T8/tools/ci/"
docker run --rm $U -v "$T8":/w -w /w ea1-imagen:local bash -c '
  git config --global user.email lab@ejemplo.invalid; git config --global user.name lab
  git init -q -b main . && dotnet restore src/App -v q >/dev/null && (cd web && npm install -s --no-audit --no-fund >/dev/null) && (cd tools/gen && uv lock -q)
  git add -A && git commit -qm "plantillas + locks"
  echo "-- plantillas:"; CI=true bash tools/ci/compuerta.sh | sed "s/commit=[0-9a-f]*/commit=…/"; echo "  rc=${PIPESTATUS[0]}"
  echo "-- un .cs en cp1252 en src/App, con el alcance del hook:"; printf "class Alarma { string \xc7audal = \"Atenci\xf3n\"; }\n" > src/App/Alarma.cs; git add src/App/Alarma.cs
  bash tools/ci/compuerta.sh --staged | grep -E "byte|resultado"; echo "  rc=${PIPESTATUS[0]}"; git rm -q --cached src/App/Alarma.cs; rm src/App/Alarma.cs
  echo "-- versión central inexistente (Primitives 8.0.2), con lock:"
  sed -i "s|<PackageVersion Include=\"Newtonsoft.Json\" Version=\"13.0.3\" />|&\n    <PackageVersion Include=\"Microsoft.Extensions.Primitives\" Version=\"8.0.2\" />|" Directory.Packages.props
  sed -i "s|<PackageReference Include=\"Newtonsoft.Json\" />|&\n    <PackageReference Include=\"Microsoft.Extensions.Primitives\" />|" src/App/App.csproj
  echo "  restore local, líneas NU1603: $(dotnet restore src/App 2>&1 | grep -c NU1603)"; git commit -qam primitives
  CI=true bash tools/ci/compuerta.sh | grep -E "grafo NuGet|pide|resultado"; echo "  rc=${PIPESTATUS[0]}"; git reset -q --hard HEAD~1; rm -rf src/App/obj
  echo "-- web/package.json cambiado sin regenerar su lock:"
  sed -i "s/\"private\": true,/\"private\": true,\n  \"dependencies\": { \"is-number\": \"7.0.0\" },/" web/package.json; git commit -qam dep
  CI=true bash tools/ci/compuerta.sh | grep -E "grafo npm|resultado"; echo "  rc=${PIPESTATUS[0]}"; git reset -q --hard HEAD~1
  echo "-- web/ y tools/gen/ versionados SIN su lock, como quedan al copiar las plantillas (límite declarado, 10.2.2):"
  git rm -q --cached web/package-lock.json tools/gen/uv.lock && git commit -qm "sin locks de npm y uv"
  CI=true bash tools/ci/compuerta.sh | grep -E "grafo (npm|uv)|resultado"; echo "  rc=${PIPESTATUS[0]} (verde: lo detecta la revisión, no la compuerta)"; git reset -q --hard HEAD~1'

echo "== 9. Los run: de A.12 en la misma imagen: SOURCE_DATE_EPOCH por GITHUB_ENV, build y registro del entorno (A.17)"
docker run --rm $U -v "$T8":/w -w /w ea1-imagen:local bash -c '
  GITHUB_ENV=/tmp/github_env; echo "SOURCE_DATE_EPOCH=$(git log -1 --pretty=%ct)" >> "$GITHUB_ENV"; set -a; . "$GITHUB_ENV"; set +a
  CI=true dotnet build src/App -c Release -o out -v q -nologo >/dev/null && echo "  dotnet build rc=0"
  bash tools/ci/registrar-entorno.sh out/App.dll | sed "s/^/  /"; echo "  rc=${PIPESTATUS[0]}"
  echo "== 9b. El workflow guardado con otro nombre: el registro falla en vez de escribir NO DECLARADA"
  mv .github/workflows/ci.yml .github/workflows/compuerta.yml
  bash tools/ci/registrar-entorno.sh out/App.dll 2>&1 | grep -E "^registro|^imagen" | sed "s/^/  /"; echo "  rc=${PIPESTATUS[0]} (se espera 1)"
  WORKFLOW_FILE=.github/workflows/compuerta.yml bash tools/ci/registrar-entorno.sh out/App.dll | grep "^imagen" | sed "s/^/  /"
  mv .github/workflows/compuerta.yml .github/workflows/ci.yml; rm -rf out'
rm -rf "$T8"

echo "== 10. El devcontainer de A.9 con la CLI de referencia, en host Linux"
T10=$(mktemp -d); cp -a "$AQUI/repo/." "$T10/"
( cd "$T10" && git init -q -b main . && git -c user.email=lab@ejemplo.invalid -c user.name=lab add -A \
  && git -c user.email=lab@ejemplo.invalid -c user.name=lab commit -qm plantillas )
npx -y @devcontainers/cli@0.89.0 up --workspace-folder "$T10" > "$T10.up" 2>/dev/null; grep -o '"outcome":"[a-z]*"' "$T10.up" | sed 's/^/  /'
npx -y @devcontainers/cli@0.89.0 exec --workspace-folder "$T10" sh -c 'echo "  uid=$(id -u) core.hooksPath=$(git config core.hooksPath)"'
docker rm -f "$(grep -o '"containerId":"[0-9a-f]*"' "$T10.up" | cut -d'"' -f4)" >/dev/null; rm -rf "$T10" "$T10.up"
