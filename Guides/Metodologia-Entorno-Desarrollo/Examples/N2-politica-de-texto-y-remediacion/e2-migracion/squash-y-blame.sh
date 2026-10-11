#!/bin/sh
# E2-2, caso 11 (mesa, ciclo 1): ¿un PR de migración fusionado con SQUASH rompe .git-blame-ignore-revs?
# No, si cada commit va en su propio PR y el SHA que se lista es el del squash YA en main.
# Lo que el squash rompe es juntar política, migración e ignore-revs en un solo PR.
# Ejemplo sintético; sólo git e iconv. Lo llama run.sh; también corre suelto: sh squash-y-blame.sh
set -eu
W=$(mktemp -d); cd "$W"; git init -q -b main r; cd r
git config user.name autora; git config user.email autora@example.invalid
printf 'class Sensor\r\n{\r\n    int Leer() => 42; // Atenci\363n\r\n}\r\n' > Sensor.cs
git add Sensor.cs; git commit -qm "codigo original (cp1252, CRLF)"
git switch -qc migracion; git config user.name migrador
iconv -f CP1252 -t UTF-8 Sensor.cs | tr -d '\r' > t && mv t Sensor.cs
git commit -qam "migracion mecanica"; git commit -q --allow-empty -m "segundo commit de la rama (para que el squash junte dos)"
git switch -q main; git merge -q --squash migracion >/dev/null; git commit -qm "Migracion cp1252 -> UTF-8 (squash)"
S=$(git rev-parse HEAD); echo "  SHA del squash en main: $(git rev-parse --short=12 HEAD)"
echo "  -- blame sin ignore-revs"; git blame -s Sensor.cs | sed 's/^/     /'; git log -1 --format='     autor del commit atribuido: %an' "$S"
echo "$S" > .git-blame-ignore-revs; git add .git-blame-ignore-revs; git commit -qm "aislar la migracion de blame"
echo "  -- blame con ignore-revs del SHA del squash (PR aparte, despues de fusionar)"
git -c blame.ignoreRevsFile=.git-blame-ignore-revs blame Sensor.cs | sed -E 's/^/     /; s/[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9:]{8} [+-][0-9]{4}/<fecha>/'
echo "  $(git --version)"
rm -rf "$W"
