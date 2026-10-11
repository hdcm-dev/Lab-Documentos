#!/usr/bin/env bash
# E7-01 — La misma compuerta en el hook local y en la CI; el hook se saltea, la CI no.
set -u
source "$(dirname "$0")/../comun.sh"; preparar 01; cd "$W"

titulo "0. Repo con el entorno declarado y un remoto"
bash "$N7/compuerta/sembrar-repo.sh" "$W/semilla" && git clone -q --bare "$W/semilla" remoto.git
git clone -q remoto.git dev && cd dev
echo "core.hooksPath tras clonar: $(git config core.hooksPath || echo '(sin configurar: git no activa hooks al clonar)')"
ls .githooks

titulo "1. Sin activar el hook, un archivo en cp1252 entra sin que nadie lo note"
printf 'namespace App;\npublic static class Señal { public const string Texto = "Atención"; }\n' | iconv -f UTF-8 -t CP1252 > Senal.cs
codif Senal.cs
git add Senal.cs; rc git commit -qm "Senal en cp1252 (hook inactivo)"
git reset -q --hard HEAD~1

titulo "2. Se activa el hook versionado (una vez por clon) y el mismo cambio se rechaza"
git config core.hooksPath .githooks
printf 'namespace App;\npublic static class Señal { public const string Texto = "Atención"; }\n' | iconv -f UTF-8 -t CP1252 > Senal.cs
git add Senal.cs; rc git commit -qm "Senal en cp1252"

titulo "3. --no-verify saltea el hook; el push a un remoto sin protección se acepta"
rc git commit -q --no-verify -m "Senal en cp1252 (--no-verify)"
rc git push -q origin main

titulo "4. CI: clon limpio del remoto, misma compuerta, sin hook de por medio"
cd "$W"; git clone -q remoto.git ci && cd ci
rc tools/compuerta.sh

titulo "5. Corrección: el archivo pasa a UTF-8 y el ciclo se cierra en verde en los dos lados"
cd "$W/dev"
iconv -f CP1252 -t UTF-8 Senal.cs > t && mv t Senal.cs; codif Senal.cs
git add Senal.cs; rc git commit -qm "Senal a UTF-8"
rc git push -q origin main
cd "$W/ci"; git pull -q; rc tools/compuerta.sh
