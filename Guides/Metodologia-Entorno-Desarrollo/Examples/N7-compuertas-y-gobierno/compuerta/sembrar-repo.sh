#!/usr/bin/env bash
# Crea un repositorio de juguete .NET con el entorno ya declarado:
#   política de texto del N2, global.json exacto (N3), lock de NuGet (N4),
#   compuerta + hook versionados (N7).
# Uso: sembrar-repo.sh <directorio>   (corre dentro de la imagen lab-n2-texto)
set -euo pipefail
LAB=$(cd "$(dirname "$0")/../.." && pwd)          # .../Examples
N2=$LAB/N2-politica-de-texto-y-remediacion
N7=$LAB/N7-compuertas-y-gobierno
R=$1
rm -rf "$R"; mkdir -p "$R"; cd "$R"
git init -q -b main .
cp "$N2/politica/.editorconfig" "$N2/politica/.gitattributes" .
mkdir -p tools .githooks
cp "$N7/compuerta/compuerta.sh" "$N2/tools/check_text_policy.py" tools/
cp "$N7/compuerta/pre-commit" .githooks/pre-commit
chmod +x tools/compuerta.sh .githooks/pre-commit
printf 'bin/\nobj/\nnode_modules/\n.venv/\n' > .gitignore
SDK=$(cd /tmp && dotnet --version)
cat > global.json <<J
{
  "sdk": {
    "version": "$SDK",
    "rollForward": "disable"
  }
}
J
cat > App.csproj <<'P'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
P
printf 'Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new { Saludo = "Configuración" }));\n' > Program.cs
dotnet restore -v q >/dev/null
git add -A
git commit -qm "entorno declarado: política de texto, SDK exacto, lock de NuGet, compuerta"
