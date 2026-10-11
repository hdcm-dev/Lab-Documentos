#!/bin/sh
# E2-7 (mesa, ciclo 1). ¿Una herramienta existente detecta lo mismo que check_text_policy.py?
# editorconfig-checker 4.0.2 contra la política de §5.5 de la guía, sobre siete archivos sintéticos.
# Se corre en el HOST (necesita docker), desde la carpeta de N2:  bash e7-editorconfig-checker/run.sh
set -u
cd "$(dirname "$0")/.." || exit 1
W=work/e7; rm -rf "$W"; mkdir -p "$W"; cd "$W" || exit 1
cat > .editorconfig <<'EC'
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true

[*.{razor,cshtml,sln}]
charset = utf-8-bom
EC
printf 'class A { string s = "Atenci\363n"; }\n' > Cp1252.cs        # byte F3 = ó en cp1252
printf 'class B { string s = "Atenci\303\263n"; }\n' > Utf8.cs      # UTF-8 sin BOM: válido
printf '\357\273\277class C { }\n' > ConBom.cs                       # BOM donde la política dice utf-8
printf '<p>Atenci\303\263n</p>\n' > SinBom.razor                     # falta el BOM que pide la política
printf '\357\273\277<p>Atenci\303\263n</p>\n' > ConBom.razor          # válido
printf '\357\273\277{ "a": 1 }\n' > conbom.json                      # BOM indebido
printf 'echo hola\r\n' > crlf.sh                                     # CRLF indebido
docker run --rm -u "$(id -u):$(id -g)" --volume="$PWD":/check ghcr.io/editorconfig-checker/editorconfig-checker:4.0.2 > ../e7.out 2>&1; rc=$?
sed "s/\x1b\[[0-9;]*m//g" ../e7.out; rm -f ../e7.out
echo "rc=$rc"
