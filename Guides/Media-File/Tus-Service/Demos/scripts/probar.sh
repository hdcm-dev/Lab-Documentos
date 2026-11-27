#!/usr/bin/env bash
# Compila la demo, la levanta y recorre los casos de la sección 6 de la guía que se miden sin Docker.
# Corre dentro de la imagen mcr.microsoft.com/dotnet/sdk:10.0.404, desde la carpeta Demos.
set -uo pipefail
cd "$(dirname "$0")/.."

URL=http://127.0.0.1:5080
CLAVE=clave-de-prueba
DATOS=$(mktemp -d)
FALLAS=0
ok()  { echo "OK    $*"; }
mal() { echo "FALLA $*"; FALLAS=$((FALLAS+1)); }
esperar() { [ "$1" = "$2" ] && ok "$3 ($1)" || mal "$3: esperaba $2, vino $1"; }
b64() { printf '%s' "$1" | base64 -w0; }

echo "== compilar"
dotnet build TusDemo.slnx -c Release -nologo -v q || { echo "FALLA compilación"; exit 1; }

echo "== levantar"
Archivos__Raiz=$DATOS/tus Archivos__Registro=$DATOS/registro.json Archivos__ClaveDemo=$CLAVE \
Archivos__TopeBytes=20000000 ASPNETCORE_URLS=$URL ASPNETCORE_ENVIRONMENT=Development \
  bash -c 'cd src/TusDemo.Web && exec dotnet bin/Release/net10.0/TusDemo.Web.dll' > "$DATOS/servidor.log" 2>&1 &
PID=$!
trap 'kill $PID 2>/dev/null' EXIT
for _ in $(seq 1 60); do curl -s -o /dev/null "$URL/" && break; sleep 1; done

H=(-H "Tus-Resumable: 1.0.0")
K=(-H "X-Clave-Demo: $CLAVE")
META="filename $(b64 foto.jpg),filetype $(b64 image/jpeg)"

echo "== criterios"
opt=$(curl -s -i -X OPTIONS "$URL/archivos" | tr -d '\r')
v=$(printf '%s\n' "$opt" | sed -n 's/^Tus-Version: //Ip')
[[ "$v" == *1.0.0* ]] && ok "OPTIONS sin credenciales anuncia Tus-Version ($v)" || mal "OPTIONS sin Tus-Version 1.0.0 ($v)"
esperar "$(printf '%s\n' "$opt" | sed -n '1s/^HTTP[^ ]* \([0-9]*\).*/\1/p')" 204 "OPTIONS sin credenciales responde"

esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" -H 'Upload-Length: 10' -H "Upload-Metadata: $META" "$URL/archivos")" 401 "POST sin credenciales"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 10' "$URL/archivos")" 400 "POST sin metadatos"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 10' -H "Upload-Metadata: filename $(b64 foto.jpg)" "$URL/archivos")" 400 "POST con filename pero sin filetype"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 10' -H "Upload-Metadata: filename $(b64 virus.exe),filetype $(b64 application/octet-stream)" "$URL/archivos")" 400 "POST con extensión no permitida"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 30000000' -H "Upload-Metadata: $META" "$URL/archivos")" 413 "POST más grande que el tope"

loc=$(curl -s -i -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 10' -H "Upload-Metadata: $META" "$URL/archivos" | tr -d '\r' | sed -n 's/^Location: //Ip')
[ -n "$loc" ] && ok "POST válido devuelve Location ($loc)" || mal "POST válido sin Location"
id=${loc##*/}
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X PATCH "${H[@]}" "${K[@]}" -H 'Upload-Offset: 3' -H 'Content-Type: application/offset+octet-stream' --data-binary 'abc' "$URL$loc")" 409 "PATCH con offset equivocado"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X PATCH "${H[@]}" "${K[@]}" -H 'Upload-Offset: 0' -H 'Content-Type: application/offset+octet-stream' --data-binary 'abcde' "$URL$loc")" 204 "PATCH del primer trozo"
off=$(curl -s -I "${H[@]}" "${K[@]}" "$URL$loc" | tr -d '\r' | sed -n 's/^Upload-Offset: //Ip')
esperar "$off" 5 "HEAD informa lo recibido"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/$id")" 409 "GET de una suba incompleta"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d "{\"idSuba\":\"$id\"}" "$URL/api/archivos")" 409 "registrar una suba incompleta"
curl -s -o /dev/null -X PATCH "${H[@]}" "${K[@]}" -H 'Upload-Offset: 5' -H 'Content-Type: application/offset+octet-stream' --data-binary 'fghij' "$URL$loc"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "$URL/descargas/$id")" 401 "GET de descarga sin credenciales"
esperar "$(curl -s "${K[@]}" "$URL/descargas/$id")" abcdefghij "GET devuelve el contenido completo"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d "{\"idSuba\":\"$id\"}" "$URL/api/archivos")" 201 "registrar la suba completa"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d "{\"idSuba\":\"$id\"}" "$URL/api/archivos")" 200 "registrar otra vez no duplica"
doc=$(curl -s "${K[@]}" "$URL/api/archivos" | python3 -c 'import json,sys; print(json.load(sys.stdin)[0]["documento"])' 2>/dev/null || curl -s "${K[@]}" "$URL/api/archivos" | sed -n 's/.*"documento":"\([^"]*\)".*/\1/p')
esperar "$doc" "descargas/$id" "Documento es la ruta relativa"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/..%2Fregistro")" 404 "GET con ..%2F en el id (ASP.NET Core no decodifica %2F dentro del tramo)"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d '{"idSuba":"0123456789abcdef0123456789abcdef"}' "$URL/api/archivos")" 404 "registrar una suba que no existe"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "$URL/openapi/v1.json")" 200 "documento OpenAPI"
esperar "$(curl -s -L -o /dev/null -w '%{http_code}' "$URL/scalar")" 200 "interfaz de Scalar (siguiendo la redirección)"
curl -s "$URL/" | grep -q "Subas reanudables" && ok "página Blazor" || mal "página Blazor"

echo "== corte a mitad de un trozo"
# Un único PATCH con todo el cuerpo, frenado a 300 kB/s y cortado a los 4 s: el servidor tiene que
# guardar lo que llegó aunque no coincida con ningún borde de trozo.
head -c 6000000 /dev/urandom > "$DATOS/grande.bin"
loc2=$(curl -s -i -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 6000000' -H "Upload-Metadata: filename $(b64 grande.txt),filetype $(b64 application/octet-stream)" "$URL/archivos" | tr -d '\r' | sed -n 's/^Location: //Ip')
[ -n "$loc2" ] && ok "POST de 6000000 bytes ($loc2)" || mal "POST de 6000000 bytes sin Location"
id2=${loc2##*/}
curl -s -o /dev/null -X PATCH "${H[@]}" "${K[@]}" -H 'Upload-Offset: 0' -H 'Content-Type: application/offset+octet-stream' \
  --data-binary @"$DATOS/grande.bin" --limit-rate 300k --max-time 4 "$URL$loc2"
echo "      curl cortó el PATCH con código de salida $? (28 = tiempo agotado)"
sleep 2   # dar tiempo al servidor a notar el corte y soltar la suba
off2=$(curl -s -I "${H[@]}" "${K[@]}" "$URL$loc2" | tr -d '\r' | sed -n 's/^Upload-Offset: //Ip')
[[ "$off2" =~ ^[0-9]+$ ]] || off2=0
echo "      offset después del corte: $off2"
if [ "$off2" -gt 0 ] && [ "$off2" -lt 6000000 ] && [ $((off2 % 1048576)) -ne 0 ]; then
  ok "el corte deja un offset intermedio que no es múltiplo de 1048576 ($off2)"
else
  mal "offset después del corte: esperaba >0, <6000000 y no múltiplo de 1048576, vino $off2"
fi
cod=$(tail -c +$((off2+1)) "$DATOS/grande.bin" | curl -s -o /dev/null -w '%{http_code}' -X PATCH "${H[@]}" "${K[@]}" \
  -H "Upload-Offset: $off2" -H 'Content-Type: application/offset+octet-stream' --data-binary @- "$URL$loc2")
esperar "$cod" 204 "PATCH con el resto desde el offset $off2"
h_orig=$(sha256sum < "$DATOS/grande.bin" | cut -d' ' -f1)
h_bajada=$(curl -s "${K[@]}" "$URL/descargas/$id2" | sha256sum | cut -d' ' -f1)
esperar "$h_bajada" "$h_orig" "la descarga tras el corte tiene la misma huella sha256"

echo "== borrado por tus deshabilitado"
cod=$(curl -s -o /dev/null -w '%{http_code}' -X DELETE "${H[@]}" "${K[@]}" "$URL$loc")
[ "$cod" != 204 ] && ok "DELETE de una suba completa y registrada no la borra (vino $cod)" || mal "DELETE de una suba registrada devolvió 204"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/$id")" 200 "la suba sigue descargable después del DELETE"

echo "== filetype y traversal"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 10' -H "Upload-Metadata: filename $(b64 foto.jpg),filetype $(b64 'no es un tipo')" "$URL/archivos")" 400 "POST con filetype inválido"
# Validación del id con un blanco que existe: se copian los archivos de una suba completa a un id con guion
# ("x-<id>"). Ese id no es un GUID: la validación tiene que rechazarlo antes de llegar al almacén.
for f in "$DATOS/tus/$id"*; do cp "$f" "${f/$id/x-$id}"; done
ls "$DATOS/tus/x-$id" >/dev/null 2>&1 && ok "existe en disco una suba con id x-$id" || mal "no se pudo preparar la suba x-$id"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/x-$id")" 404 "GET con un id con caracteres no permitidos y archivo existente"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/zzz")" 404 "GET con un id alfanumérico que no es un GUID"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/0123456789abcdef0123456789abcdef")" 404 "GET con un GUID válido que no existe"

echo "== doble registro concurrente"
curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d "{\"idSuba\":\"$id2\"}" "$URL/api/archivos" > "$DATOS/reg1" &
P1=$!
curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d "{\"idSuba\":\"$id2\"}" "$URL/api/archivos" > "$DATOS/reg2" &
P2=$!
wait $P1 $P2   # sólo estos dos: un wait sin argumentos esperaría también al servidor
echo "      códigos de los dos registros: $(cat "$DATOS/reg1") y $(cat "$DATOS/reg2")"
n=$(curl -s "${K[@]}" "$URL/api/archivos" | grep -o "$id2" | wc -l)
esperar "$n" 1 "dos registros simultáneos del mismo idSuba dejan una sola fila"

echo "== cliente .NET con corte y retoma"
head -c 5000000 /dev/urandom | base64 > "$DATOS/prueba.txt"
dotnet src/TusDemo.ClienteConsola/bin/Release/net10.0/TusDemo.ClienteConsola.dll "$URL" "$CLAVE" "$DATOS/prueba.txt" 2 \
  && ok "cliente .NET: corte, retoma y misma huella" || mal "cliente .NET"

echo "== registro del servidor (últimas líneas)"
tail -n 5 "$DATOS/servidor.log"
echo "== tusdotnet: $(grep -o 'Include="tusdotnet" Version="[^"]*"' src/TusDemo.Web/TusDemo.Web.csproj | sed 's/.*Version="\([^"]*\)"/\1/')"
echo "== FALLAS: $FALLAS"
exit $FALLAS
