#!/usr/bin/env bash
# Corre un comando dentro del contenedor de compilación de la demo, con la demo montada en /work.
#
#   scripts/contenedor.sh dotnet build FrameworkConcept.Demo.SinMaui.slnf
#   scripts/contenedor.sh dotnet test tests/FrameworkConcept.Core.Tests
#
# Por qué un contenedor: la máquina de quien corre la demo no necesita tener .NET ni el workload de
# Android. La imagen sale de la receta versionada contenedor/Dockerfile (P11, «cómo se corre»: SDK 10.0.400,
# workload set 10.0.400.1 con maui-android 10.0.20, JDK 17, SDK de Android, python3 y openssl). Si la imagen no
# existe, este script la construye (la primera vez tarda varios minutos) y la etiqueta fc-demo-build:<huella>, donde
# la huella son los primeros 12 caracteres del sha256 de la receta: si la receta cambia, se construye otra.
#
# Variables que se pueden cambiar:
#   FC_IMAGEN   imagen de compilación          (por defecto la de la receta; otra imagen sólo para pruebas)
#   FC_NUGET    volumen Docker de caché NuGet   (por defecto fc-nuget; se crea si falta)
#   FC_NOMBRE   nombre del contenedor           (por defecto fc-demo-<pid>)
#   FC_DOCKER   opciones extra para docker run  (por ejemplo «-p 8099:8099»)
set -euo pipefail

if [ "$#" -eq 0 ]; then
  echo "uso: $0 <comando> [argumentos]   (ej.: $0 dotnet build FrameworkConcept.Demo.SinMaui.slnf)" >&2
  exit 2
fi

DEMO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMAGEN="${FC_IMAGEN:-$("$DEMO/scripts/imagen.sh")}"
NUGET="${FC_NUGET:-fc-nuget}"
NOMBRE="${FC_NOMBRE:-fc-demo-$$}"

docker volume inspect "$NUGET" >/dev/null 2>&1 || docker volume create "$NUGET" >/dev/null

# Se corre con el usuario de quien llama, para que bin/ y obj/ queden a su nombre y se puedan borrar.
# HOME va a /tmp porque ese usuario puede no existir dentro de la imagen; la caché NuGet va al volumen.
# shellcheck disable=SC2086
exec docker run --rm --name "$NOMBRE" \
  -u "$(id -u):$(id -g)" \
  -e HOME=/tmp/home \
  -e DOTNET_CLI_HOME=/tmp/home \
  -e NUGET_PACKAGES=/nuget/packages \
  -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  -e DOTNET_NOLOGO=1 \
  -v "$NUGET":/nuget \
  -v "$DEMO":/work \
  -w /work \
  ${FC_DOCKER:-} \
  "$IMAGEN" "$@"
