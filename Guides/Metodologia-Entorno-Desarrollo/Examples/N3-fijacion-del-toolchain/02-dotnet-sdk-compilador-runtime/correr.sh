#!/bin/sh
# Muestra que el SDK fija el compilador, el TFM fija el C# por defecto y el runtime se elige al ejecutar.
# Uso (desde esta carpeta): sh correr.sh
# El proyecto se copia a /tmp dentro del contenedor: no quedan bin/ ni obj/ en el repo.
NUGET_CACHE="${NUGET_CACHE:-$HOME/.cache/n3-nuget}"; mkdir -p "$NUGET_CACHE"
run() { # $1=imagen, $2=comando dentro del proyecto copiado
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 \
    -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e NUGET_PACKAGES=/nuget -v "$NUGET_CACHE":/nuget \
    -v "$PWD/App":/src:ro "$1" sh -c "cp -r /src /tmp/w && cd /tmp/w && $2" 2>&1
}
S8=mcr.microsoft.com/dotnet/sdk:8.0; S10=mcr.microsoft.com/dotnet/sdk:10.0
echo "## A. Mismo proyecto net8.0, dos SDK: ¿qué compilador y qué C#?"
for img in $S8 $S10; do echo "--- $img"; run $img 'dotnet build -p:DefineConstants=MOSTRAR_VERSION 2>&1 | grep -o "error CS8304: [^[]*" | head -1'; done
echo "## B. Sintaxis de C# 14 con LangVersion por defecto y con LangVersion=latest"
for img in $S8 $S10; do
  echo "--- $img (por defecto)"; run $img 'dotnet build -p:DefineConstants=USAR_CS14 2>&1 | grep -oE "error CS[0-9]+: [^[]*" | head -1'
  echo "--- $img (LangVersion=latest)"; run $img 'dotnet build -p:DefineConstants=USAR_CS14 -p:LangVersion=latest 2>&1 | grep -E "error CS[0-9]+|Build succeeded|warn|Compilación correcta" | sed "s#^.*Program#Program#" | head -2'
done
echo "## C. Compilado con SDK 10 para net8.0: ¿con qué runtime corre?"
run $S10 'dotnet build -v q -o out >/dev/null 2>&1; echo "runtimeconfig:"; cat out/App.runtimeconfig.json; echo "runtimes instalados:"; dotnet --list-runtimes | cut -d" " -f1,2; echo "--- dotnet out/App.dll"; dotnet out/App.dll; echo "rc=$?"; echo "--- con DOTNET_ROLL_FORWARD=Major"; DOTNET_ROLL_FORWARD=Major dotnet out/App.dll'
echo "--- mismo build ejecutado con el SDK 8 (trae runtime 8)"
run $S8 'dotnet build -v q -o out >/dev/null 2>&1; dotnet --list-runtimes | cut -d" " -f1,2 | grep NETCore; dotnet out/App.dll'
