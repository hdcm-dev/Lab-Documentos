#!/usr/bin/env bash
# Compila la demo, la levanta y recorre los criterios de §6 de Media-File-Service-TUS que se miden sin Docker.
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
v=$(curl -s -i -X OPTIONS "$URL/archivos" | tr -d '\r' | sed -n 's/^Tus-Version: //Ip')
[[ "$v" == *1.0.0* ]] && ok "OPTIONS anuncia Tus-Version ($v)" || mal "OPTIONS sin Tus-Version 1.0.0 ($v)"

esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" -H 'Upload-Length: 10' -H "Upload-Metadata: $META" "$URL/archivos")" 401 "POST sin credenciales"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${H[@]}" "${K[@]}" -H 'Upload-Length: 10' "$URL/archivos")" 400 "POST sin metadatos"
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
esperar "$(curl -s -o /dev/null -w '%{http_code}' "${K[@]}" "$URL/descargas/..%2Fregistro")" 404 "GET con un id que intenta salir de la carpeta"
esperar "$(curl -s -o /dev/null -w '%{http_code}' -X POST "${K[@]}" -H 'Content-Type: application/json' -d '{"idSuba":"0123456789abcdef0123456789abcdef"}' "$URL/api/archivos")" 404 "registrar una suba que no existe"
esperar "$(curl -s -o /dev/null -w '%{http_code}' "$URL/openapi/v1.json")" 200 "documento OpenAPI"
esperar "$(curl -s -L -o /dev/null -w '%{http_code}' "$URL/scalar")" 200 "interfaz de Scalar (siguiendo la redirección)"
curl -s "$URL/" | grep -q "Subas reanudables" && ok "página Blazor" || mal "página Blazor"

echo "== cliente .NET con corte y retoma"
head -c 5000000 /dev/urandom | base64 > "$DATOS/prueba.txt"
dotnet src/TusDemo.ClienteConsola/bin/Release/net10.0/TusDemo.ClienteConsola.dll "$URL" "$CLAVE" "$DATOS/prueba.txt" 2 \
  && ok "cliente .NET: corte, retoma y misma huella" || mal "cliente .NET"

echo "== registro del servidor (últimas líneas)"
tail -n 5 "$DATOS/servidor.log"
echo "== FALLAS: $FALLAS"
exit $FALLAS
