#!/bin/sh
# E3-2, ampliación (mesa, ciclo 1): ¿qué compilador usa una build que corre en el MSBuild de Visual Studio?
# No se puede correr Visual Studio aquí: se inspeccionan los *targets* de las imágenes oficiales del SDK,
# que son los que decide la selección, y el índice de versiones del paquete *toolset* en nuget.org.
# Uso (desde esta carpeta, en el host con docker y curl): sh inspeccion-targets.sh
set -u
S8=mcr.microsoft.com/dotnet/sdk:8.0; S10=mcr.microsoft.com/dotnet/sdk:10.0
echo "## SDK 8: ¿hay selección automática del compilador?"
docker run --rm "$S8" sh -c 'v=$(dotnet --version); echo "SDK $v"; f=/usr/share/dotnet/sdk/$v/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.targets;
  echo "-- grep RoslynCompilerType|BuildWithNetFrameworkHostedCompiler|DOTNET_HOST_PATH en Microsoft.NET.Sdk.targets:";
  grep -n "RoslynCompilerType\|BuildWithNetFrameworkHostedCompiler\|DOTNET_HOST_PATH" "$f";
  echo "-- RoslynCompilerType en todo el SDK: $(grep -rl RoslynCompilerType /usr/share/dotnet/sdk/$v/ 2>/dev/null | wc -l) archivo(s)"'
echo
echo "## SDK 10: la selección (Microsoft.NET.Sdk.BeforeCommon.targets) y el umbral de MSBuild"
docker run --rm "$S10" sh -c 'v=$(dotnet --version); echo "SDK $v"; d=/usr/share/dotnet/sdk/$v;
  f=$d/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.BeforeCommon.targets;
  n=$(grep -n "Use Roslyn deployed with SDK" "$f" | head -1 | cut -d: -f1); echo "-- $f, desde la línea $n:";
  sed -n "${n},$((n+40))p" "$f" | grep -E "Roslyn|When|Otherwise|RoslynCompilerType|DOTNET_HOST_PATH|_IsDisjointMSBuildVersion|MSBuildRuntimeType" | sed "s/^[[:space:]]*/    /";
  echo "-- _IsDisjointMSBuildVersion en Microsoft.NETCoreSdk.BundledMSBuildInformation.props:";
  grep -n "_IsDisjointMSBuildVersion" $d/Microsoft.NETCoreSdk.BundledMSBuildInformation.props | sed "s/^[[:space:]]*/    /"'
echo
echo "## nuget.org: versiones de Microsoft.Net.Sdk.Compilers.Toolset"
curl -s https://api.nuget.org/v3-flatcontainer/microsoft.net.sdk.compilers.toolset/index.json | python3 -I -c '
import json,sys
v=json.load(sys.stdin)["versions"]
print("versiones=%d primera=%s ultima=%s versiones_8.x=%d" % (len(v), v[0], v[-1], sum(x.startswith("8.") for x in v)))'
echo "fecha: $(date -u +%Y-%m-%d)"
