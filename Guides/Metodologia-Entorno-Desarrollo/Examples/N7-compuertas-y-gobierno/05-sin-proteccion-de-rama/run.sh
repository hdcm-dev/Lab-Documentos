#!/usr/bin/env bash
# E7-05 — Sin protección de rama la CI avisa pero no impide; dónde sí se puede impedir.
set -u
source "$(dirname "$0")/../comun.sh"; preparar 05; cd "$W"
bash "$N7/compuerta/sembrar-repo.sh" "$W/semilla" && git clone -q --bare "$W/semilla" remoto.git
git clone -q remoto.git dev; cd dev
roto() { printf 'namespace App;\npublic static class Señal { }\n' | iconv -f UTF-8 -t CP1252 > Senal.cs; git add Senal.cs; git commit -q --no-verify -m "$1"; }

titulo "1. Remoto SIN control del lado servidor (equivale a rama no protegida)"
roto "cp1252 directo a main"; rc git push -q origin main
echo "-- la CI corre DESPUÉS, sobre lo que ya está en main:"
git clone -q "$W/remoto.git" "$W/ci1"; (cd "$W/ci1" && tools/compuerta.sh | grep -E 'FALLA|resultado')
echo "-- main quedó en: $(git -C "$W/remoto.git" log --oneline -1 main)"

titulo "2. Control compensatorio en el acto irreversible: publicar desde main en rojo"
rc bash "$N7/05-sin-proteccion-de-rama/publicar.sh" "$W/remoto.git" "$(git rev-parse HEAD)" 1.0.0

titulo "3. Se corrige main y se publica; un segundo intento con la misma versión se rechaza"
iconv -f CP1252 -t UTF-8 Senal.cs > t && mv t Senal.cs; git commit -qam "Senal a UTF-8"; git push -q origin main
rc bash "$N7/05-sin-proteccion-de-rama/publicar.sh" "$W/remoto.git" "$(git rev-parse HEAD)" 1.0.0
rc bash "$N7/05-sin-proteccion-de-rama/publicar.sh" "$W/remoto.git" "$(git rev-parse HEAD)" 1.0.0

titulo "4. Remoto CON control del lado servidor (pre-receive que corre la compuerta)"
cp "$N7/05-sin-proteccion-de-rama/pre-receive" "$W/remoto.git/hooks/pre-receive"; chmod +x "$W/remoto.git/hooks/pre-receive"
roto "otra vez cp1252, otra vez --no-verify"; rc git push -q origin main
echo "-- main sigue en: $(git -C "$W/remoto.git" log --oneline -1 main)"

titulo "4b. Control positivo: el mismo hook acepta un push que cumple"
git reset -q --hard origin/main
printf 'namespace App;\npublic static class Señal2 { }\n' > Senal2.cs; git add Senal2.cs; git commit -qm "Senal2 en UTF-8"
rc git push -q origin main
echo "-- main quedó en: $(git -C "$W/remoto.git" log --oneline -1 main)"
