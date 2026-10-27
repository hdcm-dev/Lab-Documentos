#!/usr/bin/env bash
# El guion único de la demo (P11): compila, prueba, publica, firma, sirve, instala y recorre v1 → v2.
# Todo lo que compila o corre .NET lo hace dentro del contenedor, con scripts/contenedor.sh.
#
#   scripts/guion.sh <subcomando> [argumentos]
#
# Subcomandos, en el orden de un recorrido completo:
#   claves [--forzar]       par ECDSA P-256 de demostración (claves/privada-demo.pem NO se versiona). En un clon
#                           nuevo falta la privada y hace falta --forzar: el par nuevo pisa claves/publica-demo.pem,
#                           así que después hay que rehacer publicar v1, definicion v1, firmar y semilla
#   build                   compila todo menos el cascarón (FrameworkConcept.Demo.SinMaui.slnf), avisos como error
#   test                    corre las tres suites (Core, Update, Gate)
#   publicar v1|v2|sin-listo  dotnet publish del contenido (v1 = versionado/contenido.json; v2 = el menor siguiente;
#                           sin-listo = el menor después de v2, defectuoso a propósito: no avisa «Listo») y lo
#                           empaqueta en el canal
#   definicion v1|v2        publica definiciones/encuesta-movilidad/vN.json en el canal
#   firmar [opciones]       arma y firma el índice (renueva expires: P2, «la renueva el guion»)
#   verificar               comprueba lo publicado como lo haría el teléfono
#   firma-invalida          altera el índice sin volver a firmarlo (demo de rechazo); «firmar» lo arregla
#   retirar <opciones>      ver «FrameworkConcept.Tools retirar»; después, «firmar»
#   semilla                 copia la carpeta del canal (sólo lo que ofrece el índice) a semilla/, que embebe el APK
#   tls [--forzar]          CA y certificado HTTPS de demostración para el receptor (claves/tls/, NO se versiona).
#                           Con él, «servir» escucha en HTTPS y el APK de Debug confía en esa CA (P2: sólo HTTPS)
#   servir                  levanta el receptor didáctico (canal + /respuestas) en el puerto $FC_PUERTO (8099);
#                           en HTTPS si existe claves/tls/receptor.crt
#   respuestas              lo que recibió el receptor (GET /respuestas)
#   tunel                   túnel efímero de cloudflared hacia el receptor (opcional; ver la nota P11 abajo)
#   parar                   baja el receptor y el túnel
#   apk                     compila el APK del cascarón ($FC_RID, por defecto android-arm64) apuntando a $FC_URL_CANAL
#                           (o al túnel). Para un emulador x86_64: FC_RID=android-x64
#   dispositivo [-H h -P p -s serie]   modelo, Android y versión del WebView del proveedor activo (P3, C-3)
#   instalar    [-H h -P p -s serie]   instala el APK en el teléfono
#   demo        [-H h -P p -s serie]   recorrido completo en el dispositivo: semilla, encuesta sin red (paso
#                           manual), v1 → v2 de contenido y definición, firma inválida y contenido sin «Listo».
#                           Exige FC_URL_CANAL (sin túnel)
#   tpr         [-H h -P p -s serie]   P1: $FC_CORRIDAS (5) arranques en frío medidos con la pantalla encendida y
#                           sin bloqueo (si no, no mide); el TPR de cada uno sale del logcat («FrameworkConcept.Tools tpr»)
#   humo                    prueba de humo de las herramientas y del receptor, sin teléfono
#   limpiar                 borra salida/, bin/ y obj/ (no toca claves/ ni semilla/)
#
# Variables:
#   FC_CANAL        canal (dev)                    FC_PUERTO     puerto del receptor (8099)
#   FC_URL_CANAL    URL HTTPS con la que el dispositivo llega al receptor (si falta, «apk» usa la del túnel, si hay)
#   FC_ESPERA       segundos de espera tras cada arranque en «demo» (50)
#   FC_CORRIDAS     arranques en frío de «tpr» (5)   FC_ESPERA_TPR  segundos tras cada uno (20)   FC_UMBRAL_TPR  ms (5000)
#   FC_ADB_CONTENEDOR  si el adb está en un contenedor (docker exec <él> adb …); si falta, adb del sistema
#   FC_LOCK         candado del teléfono compartido; cada tramo de adb corre dentro de «flock $FC_LOCK»
#   FC_SIN_BUILD=1  no recompila las herramientas antes de usarlas
#   FC_RID          android-arm64 (teléfono, por defecto) o android-x64 (emulador)
#   FC_TLS_IPS      IPs extra del certificado del receptor, separadas por coma (además de localhost y 127.0.0.1)
#   FC_OIDC_EMISOR  folio 038: emisor OIDC (p. ej. https://203.0.113.10:8443/realms/fc-demo, infra/keycloak/).
#                   «servir» exige su token en POST /respuestas y «apk» compila el modo Oidc; vacío = como A3
#
# P11 — «Un solo camino de red, medido antes de escribirlo; el túnel público queda excluido». El camino medido
# (E02, 2026-10-26): el dispositivo llega al receptor por la IP de Tailscale de la máquina que lo corre, en HTTPS con
# la CA de demostración de «tls» (FC_URL_CANAL=https://<ip>:8099, FC_TLS_IPS=<ip>). Se midió SÓLO con el emulador
# fc-android-emu-hpdev.
# DESVÍO DECLARADO DE P11 — El teléfono de medida (moto g42) NO alcanza por red a la máquina del receptor. Con él, el
# único medio que funcionó (E01) es «tunel»: un túnel efímero de cloudflared, MEDIO DE LABORATORIO, sólo con el
# contenido de la demo y sin secretos, que se baja con «parar». El candidato de P11, el reenvío inverso de puertos de
# adb, sigue NO VERIFICADO: con un servidor adb remoto (-H) el reenvío llega a la máquina de ese servidor, no a la del
# receptor.
set -euo pipefail

DEMO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
C="$DEMO/scripts/contenedor.sh"
cd "$DEMO"

CANAL="${FC_CANAL:-dev}"
PUERTO="${FC_PUERTO:-8099}"
RAIZ=salida/canal                                    # relativo a la demo (= /work dentro del contenedor)
DATOS=salida/receptor                                # donde el receptor guarda las respuestas recibidas
RECEPTOR=fc-demo-receptor                            # nombre del contenedor del receptor
PUENTE_MAYOR="$(sed -n 's/.*"version" *: *"\([0-9]*\)\..*/\1/p' versionado/puente.json | head -1)"
CARPETA="$RAIZ/$CANAL/puente-$PUENTE_MAYOR"
PAQUETE_APP=com.frameworkconcept.demo
RID="${FC_RID:-android-arm64}"                      # android-arm64 = teléfono; android-x64 = emulador
APK=src/FrameworkConcept.Shell/bin/Debug/net10.0-android/$RID/$PAQUETE_APP-Signed.apk
TOOLS_DLL=tools/FrameworkConcept.Tools/bin/Release/net10.0/FrameworkConcept.Tools.dll
RECEPTOR_DLL=tools/FrameworkConcept.Receptor/bin/Release/net10.0/FrameworkConcept.Receptor.dll
IMAGEN_TUNEL=cloudflare/cloudflared:2024.8.2
TLS=claves/tls                                       # CA y certificado HTTPS de demostración (no se versionan)
LOCK="${FC_LOCK:-${XDG_RUNTIME_DIR:-/tmp}/frameworkconcept-adb.lock}"

paso() { printf '\n== %s\n' "$*"; }
falla() { echo "guion: $*" >&2; exit 1; }

# ---------- .NET dentro del contenedor ----------

# Corre FrameworkConcept.Tools con las opciones del canal de este guion.
herramienta() {
  local comando="$1"; shift
  local opciones=()
  case "$comando" in claves|contrato|tpr) ;; *) opciones=(--raiz "$RAIZ" --canal "$CANAL" --puente "$PUENTE_MAYOR") ;; esac
  if [ "${FC_SIN_BUILD:-}" = 1 ] && [ -f "$TOOLS_DLL" ]; then
    "$C" dotnet "$TOOLS_DLL" "$comando" "${opciones[@]}" "$@"
  else
    "$C" bash -c 'dotnet build tools/FrameworkConcept.Tools -c Release -v q -nologo >/tmp/build.log 2>&1 || { cat /tmp/build.log; exit 1; }
                  exec dotnet "$0" "$@"' "$TOOLS_DLL" "$comando" "${opciones[@]}" "$@"
  fi
}

version_de() {  # v1 → la de versionado/contenido.json; v2 → el menor siguiente (1.0.0 → 1.1.0); sin-listo → 1.2.0
  local base; base="$(sed -n 's/.*"version" *: *"\([^"]*\)".*/\1/p' versionado/contenido.json | head -1)"
  case "$1" in
    v1) echo "$base" ;;
    v2) echo "$base" | awk -F. '{ printf "%d.%d.0\n", $1, $2 + 1 }' ;;
    sin-listo) echo "$base" | awk -F. '{ printf "%d.%d.0\n", $1, $2 + 2 }' ;;
    *) falla "versión «$1»: v1, v2 o sin-listo" ;;
  esac
}

commit_corto() { git -C "$DEMO" rev-parse --short HEAD 2>/dev/null || echo local; }

cmd_claves() { herramienta claves "$@"; }

cmd_build() {
  paso "build: todo menos el cascarón MAUI, avisos como error (P8)"
  "$C" env CI=true dotnet build FrameworkConcept.Demo.SinMaui.slnf -c Release
}

cmd_test() {
  paso "test: Core, Update y Gate"
  local s
  for s in Core Update Gate; do
    "$C" dotnet test "tests/FrameworkConcept.$s.Tests" -c Release
  done
}

cmd_publicar() {
  local variante="${1:?publicar v1|v2|sin-listo}" version commit sin_listo=false
  version="$(version_de "$variante")"; commit="$(commit_corto)"
  # P2 — «sin-listo» es un contenido defectuoso a propósito: se dibuja y nunca avisa «Listo» (demo del rollback).
  [ "$variante" = sin-listo ] && sin_listo=true
  paso "publicar $variante: contenido $version+$commit, sin AOT (P1), archivos planos (C-4)"
  "$C" dotnet publish src/FrameworkConcept.Content -c Release \
    -p:ContenidoVersion="$version" -p:ContenidoCommit="$commit" -p:ContenidoSinListo="$sin_listo" \
    -o "salida/publicacion/contenido-$version"
  # RYD, invariante 1: un paquete se construye UNA vez. Si ya hay uno distinto con esta versión, empaquetar lo rechaza.
  herramienta empaquetar --publicado "salida/publicacion/contenido-$version" --version "$version" --commit "$commit" \
    || falla "si cambiaste el contenido, subí versionado/contenido.json; para empezar de cero: «limpiar»"
}

cmd_definicion() {
  local variante="${1:?definicion v1|v2}"
  paso "definicion $variante: encuesta-movilidad (P13)"
  # P6 — v1 empieza a recoger datos y v2 agrega una pregunta de texto libre: las dos cambian lo que se recoge.
  herramienta publicar-definicion --definicion "definiciones/encuesta-movilidad/$variante.json" --cambia-datos "${FC_CAMBIA_DATOS:-si}"
}

cmd_firmar() { paso "firmar: índice de $CANAL/puente-$PUENTE_MAYOR (expires = ahora + 30 días)"; herramienta firmar "$@"; }
cmd_verificar() { herramienta verificar "$@"; }
cmd_firma_invalida() { herramienta firma-invalida "$@"; }
cmd_retirar() { herramienta retirar "$@"; }

cmd_semilla() {
  # P2 — La semilla es una versión completa del canal, firmada igual: se copia sólo lo que el índice ofrece hoy.
  [ -f "$CARPETA/indice.json" ] || falla "no hay índice en $CARPETA: «publicar v1», «definicion v1» y «firmar» primero"
  paso "semilla: $CARPETA → semilla/"
  find semilla -mindepth 1 ! -name .gitkeep ! -name 'README*' -delete 2>/dev/null || true
  mkdir -p semilla
  cp "$CARPETA/indice.json" "$CARPETA/indice.json.sig" semilla/
  local ruta
  for ruta in $(sed -n 's/.*"ruta" *: *"\([^"]*\)".*/\1/p' "$CARPETA/indice.json"); do
    mkdir -p "semilla/$(dirname "$ruta")"
    cp "$CARPETA/$ruta" "semilla/$ruta"
  done
  find semilla -type f | sort
}

# ---------- servidor ----------

# P2 — «Transporte: sólo HTTPS». Sin túnel público (P11), el receptor necesita su propio certificado. Esta CA de
# demostración vale 30 días, se genera en la máquina de quien corre la demo y NO se versiona. El APK la acepta sólo
# porque es una compilación depurable: va en los «debug-overrides» de la configuración de red de Android, que un APK
# de Release ignora (src/FrameworkConcept.Shell/Platforms/Android/Red/).
cmd_tls() {
  if [ -f "$TLS/receptor.crt" ] && [ "${1:-}" != --forzar ]; then
    echo "tls: ya existe $TLS/receptor.crt (--forzar para regenerarlo)"; return 0
  fi
  local san="DNS:localhost,IP:127.0.0.1" ip
  for ip in ${FC_TLS_IPS//,/ }; do san="$san,IP:$ip"; done
  paso "tls: CA de demostración y certificado del receptor ($san)"
  mkdir -p "$TLS"
  "$C" bash -c 'set -e; d="$0"; san="$1"
    openssl req -x509 -newkey ec -pkeyopt ec_paramgen_curve:P-256 -nodes -days 30 \
      -subj "/CN=FrameworkConcept CA de demostracion (solo pruebas)" \
      -addext "basicConstraints=critical,CA:TRUE" -addext "keyUsage=critical,keyCertSign,cRLSign" \
      -keyout "$d/ca_demo_tls.key" -out "$d/ca_demo_tls.crt" 2>/dev/null
    openssl req -newkey ec -pkeyopt ec_paramgen_curve:P-256 -nodes -subj "/CN=receptor-didactico" \
      -keyout "$d/receptor.key" -out "$d/receptor.csr" 2>/dev/null
    printf "subjectAltName=%s\nbasicConstraints=CA:FALSE\nextendedKeyUsage=serverAuth\n" "$san" > "$d/receptor.ext"
    openssl x509 -req -in "$d/receptor.csr" -CA "$d/ca_demo_tls.crt" -CAkey "$d/ca_demo_tls.key" -CAcreateserial \
      -days 30 -extfile "$d/receptor.ext" -out "$d/receptor.crt" 2>/dev/null
    rm -f "$d/receptor.csr" "$d/receptor.ext" "$d/ca_demo_tls.srl"
    openssl x509 -in "$d/receptor.crt" -noout -subject -ext subjectAltName -enddate' "$TLS" "$san"
  echo "  Recompilá el APK («apk») para que lleve esta CA."
}

cmd_servir() {
  [ -f "$RECEPTOR_DLL" ] && [ "${FC_SIN_BUILD:-}" = 1 ] || "$C" dotnet build tools/FrameworkConcept.Receptor -c Release -v q -nologo
  docker rm -f "$RECEPTOR" >/dev/null 2>&1 || true
  local esquema=http opciones="" curl_tls=()
  if [ -f "$TLS/receptor.crt" ]; then
    # Kestrel lee el certificado y la clave en PEM por configuración: el receptor no cambia.
    esquema=https
    opciones="-e ASPNETCORE_Kestrel__Certificates__Default__Path=/work/$TLS/receptor.crt"
    opciones="$opciones -e ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/work/$TLS/receptor.key"
    curl_tls=(--cacert "$TLS/ca_demo_tls.crt")
  fi
  paso "servir: receptor didáctico en $esquema://0.0.0.0:$PUERTO (canal en $RAIZ, respuestas en $DATOS)"
  FC_NOMBRE="$RECEPTOR" FC_DOCKER="-d -p $PUERTO:8099 $opciones" "$C" \
    dotnet "$RECEPTOR_DLL" --urls "$esquema://0.0.0.0:8099" --canal "$RAIZ" --datos "$DATOS" ${FC_AHORA:+--ahora "$FC_AHORA"} \
    ${FC_OIDC_EMISOR:+--oidc-emisor "$FC_OIDC_EMISOR" --oidc-ca "$TLS/ca_demo_tls.crt"} >/dev/null
  local i
  for i in $(seq 1 30); do
    curl -fsS "${curl_tls[@]}" "$esquema://localhost:$PUERTO/salud" >/dev/null 2>&1 && { curl -fsS "${curl_tls[@]}" "$esquema://localhost:$PUERTO/salud"; echo; return 0; }
    sleep 1
  done
  docker logs "$RECEPTOR" 2>&1 | tail -20
  falla "el receptor no respondió en 30 s"
}

# Lo que recibió el receptor: aceptadas, carta muerta y reenvíos que no se duplicaron.
cmd_respuestas() {
  # Corrector E03 — Con emisor OIDC, GET /respuestas exige token y filtra por dueño: el operador lee el almacén.
  if [ -n "${FC_OIDC_EMISOR:-}" ]; then cat "$DATOS/respuestas.json" 2>/dev/null || echo "[]"; echo; return 0; fi
  if [ -f "$TLS/receptor.crt" ]; then curl -fsS --cacert "$TLS/ca_demo_tls.crt" "https://localhost:$PUERTO/respuestas"
  else curl -fsS "http://localhost:$PUERTO/respuestas"; fi
  echo
}

cmd_tunel() {
  docker rm -f fc-demo-tunel >/dev/null 2>&1 || true
  # Si el receptor escucha en HTTPS («tls»), el túnel le habla en HTTPS. --no-tls-verify: el certificado es de la CA
  # de demostración, que cloudflared no conoce; el tramo es local (127.0.0.1). El teléfono ve el certificado público
  # de trycloudflare.com, no el de la demo.
  local origen="http://127.0.0.1:$PUERTO" tls=()
  if [ -f "$TLS/receptor.crt" ]; then origen="https://127.0.0.1:$PUERTO"; tls=(--no-tls-verify); fi
  paso "tunel: cloudflared efímero → $origen (medio de laboratorio: desvío declarado de P11)"
  docker run -d --name fc-demo-tunel --network host "$IMAGEN_TUNEL" \
    tunnel --no-autoupdate "${tls[@]}" --url "$origen" >/dev/null
  local i url=""
  for i in $(seq 1 40); do
    url="$(docker logs fc-demo-tunel 2>&1 | grep -o 'https://[a-z0-9-]*\.trycloudflare\.com' | head -1 || true)"
    [ -n "$url" ] && break
    sleep 1
  done
  [ -n "$url" ] || { docker logs fc-demo-tunel 2>&1 | tail -20; falla "el túnel no informó su URL"; }
  mkdir -p salida && echo "$url" > salida/url-del-canal.txt
  echo "$url  (guardada en salida/url-del-canal.txt)"
}

cmd_parar() {
  docker rm -f "$RECEPTOR" fc-demo-tunel 2>/dev/null || true
  rm -f salida/url-del-canal.txt   # la URL de un túnel efímero no sirve después de bajarlo
}

url_del_canal() {
  if [ -n "${FC_URL_CANAL:-}" ]; then echo "$FC_URL_CANAL"
  elif [ -f salida/url-del-canal.txt ]; then cat salida/url-del-canal.txt
  else falla "no sé cómo llega el teléfono al canal: definí FC_URL_CANAL o corré «tunel»"; fi
}

# ---------- cascarón y teléfono ----------

cmd_apk() {
  local url; url="$(url_del_canal)"
  [ -f claves/publica-demo.pem ] || falla "falta claves/publica-demo.pem: «claves» primero"
  # La clave de firma de depuración del APK vive en un volumen: así un APK nuevo se instala encima del anterior.
  docker volume inspect fc-demo-keystore >/dev/null 2>&1 || {
    docker volume create fc-demo-keystore >/dev/null
    docker run --rm -u 0 -v fc-demo-keystore:/k --entrypoint chown "$("$DEMO/scripts/imagen.sh")" "$(id -u):$(id -g)" /k
  }
  paso "apk: cascarón $RID con el canal en $url"
  # XDG_DATA_HOME lleva la carpeta de datos (donde el SDK de Android guarda debug.keystore) al volumen.
  FC_DOCKER="-v fc-demo-keystore:/fc-keystore -e XDG_DATA_HOME=/fc-keystore" "$C" \
    dotnet build src/FrameworkConcept.Shell -c Debug -p:RuntimeIdentifier="$RID" -p:AndroidPackageFormat=apk \
    -p:FcCanal="$CANAL" -p:FcCanalUrl="$url" ${FC_OIDC_EMISOR:+-p:FcOidcAutoridad="$FC_OIDC_EMISOR"}
  ls -l "$APK"
}

# Opciones -H/-P/-s del servidor adb; deja el comando en el arreglo ADB.
leer_adb() {
  local host="" puerto="" serie=""
  while [ "$#" -gt 0 ]; do
    case "$1" in
      -H) host="$2"; shift 2 ;;
      -P) puerto="$2"; shift 2 ;;
      -s) serie="$2"; shift 2 ;;
      *) falla "opción de adb desconocida: $1" ;;
    esac
  done
  ADB=()
  [ -n "${FC_ADB_CONTENEDOR:-}" ] && ADB=(docker exec "$FC_ADB_CONTENEDOR")
  ADB+=(adb)
  [ -n "$host" ] && ADB+=(-H "$host")
  [ -n "$puerto" ] && ADB+=(-P "$puerto")
  [ -n "$serie" ] && ADB+=(-s "$serie")
}

# Un tramo corto de adb dentro del candado del teléfono compartido: despierta la pantalla, hace lo suyo y, al
# terminar, detiene la app de la demo. Sólo toca el paquete com.frameworkconcept.demo.
tramo() {  # tramo <argumentos de adb>…
  local adb; adb="$(printf '%q ' "${ADB[@]}")"
  flock "$LOCK" bash -c "
    $adb shell input keyevent KEYCODE_WAKEUP
    rc=0; $adb $(printf '%q ' "$@") || rc=\$?
    $adb shell am force-stop $PAQUETE_APP || true
    exit \$rc"
}

cmd_dispositivo() {
  leer_adb "$@"
  paso "dispositivo: modelo, Android y WebView del proveedor activo (P3: adb shell dumpsys webviewupdate)"
  tramo shell 'getprop ro.product.model; getprop ro.build.version.release; dumpsys webviewupdate | grep -E "Current WebView package"; pm list packages com.frameworkconcept'
}

cmd_instalar() {
  leer_adb "$@"
  [ -f "$APK" ] || falla "no hay APK: «apk» primero"
  local destino="$APK"
  if [ -n "${FC_ADB_CONTENEDOR:-}" ]; then
    destino="/tmp/fc-demo-$PAQUETE_APP.apk"
    docker cp "$APK" "$FC_ADB_CONTENEDOR:$destino"
  fi
  paso "instalar: $PAQUETE_APP"
  tramo install -r "$destino"
  [ -n "${FC_ADB_CONTENEDOR:-}" ] && docker exec "$FC_ADB_CONTENEDOR" rm -f "$destino"
}

# Arranque en frío medido: force-stop + am start -W; espera, captura la pantalla y guarda el logcat de la app.
# El logcat se lee desde la hora del arranque (-T): no se borra el del teléfono, que otros también usan.
arrancar() {  # $1 = etiqueta, $2 = segundos de espera
  local etiqueta="$1" espera="$2" ev=salida/evidencia adb
  adb="$(printf '%q ' "${ADB[@]}")"
  mkdir -p "$ev"
  flock "$LOCK" bash -c "
    $adb shell input keyevent KEYCODE_WAKEUP
    $adb shell am force-stop $PAQUETE_APP
    desde=\$($adb shell \"date '+%m-%d %H:%M:%S.000'\" | tr -d '\r')
    actividad=\$($adb shell cmd package resolve-activity --brief $PAQUETE_APP | tail -1 | tr -d '\r')
    $adb shell am start -W -n \"\$actividad\" | tr -d '\r' | grep -E 'Status|TotalTime'
    sleep $espera
    $adb exec-out screencap -p > '$ev/captura-$etiqueta.png'
    $adb logcat -d -v time -T \"\$desde\" | tr -d '\r' | grep -E 'I/FC|FC-MARCA|AndroidRuntime' > '$ev/logcat-$etiqueta.txt' || true
    $adb shell am force-stop $PAQUETE_APP"
  echo "  evidencia: $ev/captura-$etiqueta.png, $ev/logcat-$etiqueta.txt"
}

# P1 — El TPR en el dispositivo (folio 042). En el cierre del expediente 0001 cinco arranques «sin primer-render» eran
# la pantalla apagada: am start -W devolvía «LaunchState: UNKNOWN» y el WebView no pedía el contenido. Por eso se
# despierta la pantalla antes de cada arranque y no se mide con el bloqueo puesto. Sólo toca $PAQUETE_APP.
cmd_tpr() {
  leer_adb "$@"
  local n="${FC_CORRIDAS:-5}" espera="${FC_ESPERA_TPR:-20}" ev=salida/evidencia adb
  adb="$(printf '%q ' "${ADB[@]}")"
  mkdir -p "$ev" && : > "$ev/tpr-logcat.txt" && : > "$ev/tpr-am-start.txt"
  paso "tpr: $n arranques en frío de $PAQUETE_APP (espera de $espera s cada uno)"
  flock "$LOCK" bash -c "
    $adb shell input keyevent KEYCODE_WAKEUP
    $adb shell wm dismiss-keyguard
    sleep 2
    if ! $adb shell dumpsys power | grep -q 'mWakefulness=Awake'; then echo 'tpr: la pantalla no se encendió' >&2; exit 3; fi
    if $adb shell dumpsys window | grep -q 'isKeyguardShowing=true'; then
      echo 'tpr: el dispositivo está bloqueado; desbloquealo a mano y repetí (con el bloqueo, el WebView no carga)' >&2; exit 3
    fi
    actividad=\$($adb shell cmd package resolve-activity --brief $PAQUETE_APP | tail -1 | tr -d '\r')
    for i in \$(seq 1 $n); do
      $adb shell am force-stop $PAQUETE_APP
      sleep 3
      $adb shell input keyevent KEYCODE_WAKEUP
      desde=\$($adb shell \"date '+%m-%d %H:%M:%S.000'\" | tr -d '\r')
      { echo \"== corrida \$i\"; $adb shell am start -W -n \"\$actividad\" | tr -d '\r' | grep -E 'LaunchState|TotalTime|WaitTime'; } | tee -a '$ev/tpr-am-start.txt'
      sleep $espera
      $adb logcat -d -v time -T \"\$desde\" | tr -d '\r' | grep -E 'I/FC|FC-MARCA' >> '$ev/tpr-logcat.txt' || true
    done
    $adb shell am force-stop $PAQUETE_APP" || return
  echo "  evidencia: $ev/tpr-am-start.txt, $ev/tpr-logcat.txt («LaunchState: COLD» en cada corrida)"
  herramienta tpr --logcat "$ev/tpr-logcat.txt" --umbral "${FC_UMBRAL_TPR:-5000}"
}

cmd_demo() {
  leer_adb "$@"
  export FC_SIN_BUILD=1
  # P11 — un solo camino de red y sin túnel público: el dispositivo llega al receptor por una IP propia
  # (en el laboratorio, la de Tailscale) y por HTTPS con la CA de demostración (P2).
  [ -n "${FC_URL_CANAL:-}" ] || falla "definí FC_URL_CANAL=https://<ip-que-ve-el-dispositivo>:$PUERTO (y FC_TLS_IPS=<esa ip>)"
  paso "demo: recorrido v1 → v2 (P11)"
  # Un clon nuevo no trae la privada: se genera un par nuevo, y la semilla y el APK se rehacen más abajo con él.
  if [ -f claves/privada-demo.pem ]; then cmd_claves; else cmd_claves --forzar; fi
  "$C" dotnet build tools/FrameworkConcept.Tools -c Release -v q -nologo
  cmd_publicar v1
  cmd_definicion v1
  cmd_firmar
  cmd_semilla
  cmd_tls
  cmd_servir
  cmd_apk
  cmd_instalar "$@"

  # Los tiempos de espera alcanzan para el emulador de referencia (el contenido avisa «Listo» a los ~35 s);
  # en el teléfono de medida bastan ~12 s. FC_ESPERA los cambia. Una espera menor que lo que tarda «Listo» corta la
  # app con la carga empezada: dos cortes así seguidos dejan una versión sana como fallida (P2, cargas sin Listo).
  local espera="${FC_ESPERA:-50}"
  paso "1. primer arranque: la semilla (contenido v1, definición v1)"
  arrancar 1-v1-semilla "$espera"
  paso "2. PASO MANUAL: con el receptor apagado, tomá una encuesta en el dispositivo (queda «Por enviar: 1»)"
  cmd_parar
  read -r -p "   Enter cuando la encuesta esté registrada… " _
  paso "3. se publican contenido v2 y definición v2, y vuelve el receptor"
  cmd_publicar v2
  cmd_definicion v2
  cmd_firmar
  cmd_servir
  paso "4. arranque: muestra v1, envía el outbox y baja v2 a staging (N = 0, P2)"
  arrancar 2-v1-baja-v2 "$espera"
  paso "5. arranque en frío: activa v2 antes de mostrar; la próxima encuesta usa la definición v2 (P13.2)"
  arrancar 3-v2-activa "$espera"
  paso "6. índice con firma inválida: se rechaza y sigue v2"
  cmd_firma_invalida
  arrancar 4-firma-invalida "$espera"
  cmd_firmar
  # P2 — Sólo cuenta como falla un arranque en que el WebView EMPEZÓ a cargar 1.2.0 sin que llegara «Listo»; con
  # Actualizador.CargasSinListoParaFallar = 2, la vuelta a previous llega en el cuarto arranque.
  paso "7. contenido que no avisa «Listo» (1.2.0): se baja, se activa, se reintenta y vuelve a previous"
  cmd_publicar sin-listo
  cmd_firmar
  arrancar 5-sin-listo-baja "$espera"
  arrancar 6-sin-listo-activa "$espera"
  arrancar 7-sin-listo-reintenta "$espera"
  arrancar 8-sin-listo-vuelve "$espera"
  paso "respuestas recibidas (la tomada con v1 tiene que figurar aceptada con versionDefinicion 1, P13)"
  cmd_respuestas
  echo "Fin. «parar» baja el receptor."
}

# ---------- prueba de humo (sin teléfono) ----------

cmd_humo() {
  # Usa canal, datos, puerto y contenedor propios, para no pisar la demo ni un receptor que esté corriendo, y
  # empieza siempre de cero (sin respuestas de una corrida anterior).
  local raiz_demo="$RAIZ"
  RAIZ=salida/humo/canal; CARPETA="$RAIZ/$CANAL/puente-$PUENTE_MAYOR"; PUERTO="${FC_PUERTO_HUMO:-8098}"
  DATOS=salida/humo/receptor; RECEPTOR=fc-demo-receptor-humo
  TLS=salida/humo/sin-tls   # el humo prueba el receptor en HTTP, aunque la demo tenga su certificado
  rm -rf salida/humo && mkdir -p salida/humo
  export FC_SIN_BUILD=1
  "$C" bash -c 'dotnet build tools/FrameworkConcept.Tools -c Release -v q -nologo && dotnet build tools/FrameworkConcept.Receptor -c Release -v q -nologo'
  local ok=0 malo=0
  espera() {  # $1 = código esperado, $2 = descripción, resto = comando
    local esperado="$1" que="$2"; shift 2
    local rc=0; "$@" || rc=$?
    if [ "$rc" = "$esperado" ]; then ok=$((ok+1)); echo "   OK   ($que: salió $rc)"; else malo=$((malo+1)); echo "   MAL  ($que: salió $rc, se esperaba $esperado)"; fi
  }

  # Claves propias del humo: generar un par en claves/ pisaría la pública versionada y dejaría a semilla/ (firmada
  # con el par anterior) sin verificar.
  local k=salida/humo/claves
  paso "humo 1: claves (en $k, no en claves/)"
  espera 0 "claves" herramienta claves --dir "$k"

  paso "humo 2: empaquetar un sitio publicado mínimo (sin compilar Blazor: index.html + _framework/)"
  local sitio=salida/humo/publicado/wwwroot
  mkdir -p "$sitio/_framework" "$sitio/js"
  printf '<!DOCTYPE html><html><head><base href="/" /></head><body><script src="_framework/hybridwebview.js"></script></body></html>\n' > "$sitio/index.html"
  printf 'console.log("runtime");\n' > "$sitio/_framework/dotnet.js"
  printf 'precomprimido' > "$sitio/_framework/dotnet.js.br"
  printf 'precomprimido' > "$sitio/_framework/dotnet.js.gz"
  printf 'export const v = 1;\n' > "$sitio/js/puente.js"
  espera 0 "empaquetar 1.0.0" herramienta empaquetar --publicado salida/humo/publicado --version 1.0.0 --commit humo001
  "$C" python3 -c '
import zipfile, json, sys
z = zipfile.ZipFile(sys.argv[1]); n = z.namelist()
print("   zip:", n)
assert n[0] == "manifiesto.json" and not any(x.endswith((".br", ".gz")) for x in n)
m = json.loads(z.read("manifiesto.json"))
print("   manifiesto:", json.dumps({k: m[k] for k in ("version", "compatibleConPuente", "capabilities", "motor")}, ensure_ascii=False))
assert all(len(a["sha256"]) == 64 for a in m["archivos"])' "$CARPETA/paquetes/contenido-1.0.0.zip" && ok=$((ok+1)) || malo=$((malo+1))
  espera 0 "empaquetar 1.0.0 otra vez, idéntico (mismo sha256)" herramienta empaquetar --publicado salida/humo/publicado --version 1.0.0 --commit humo001
  espera 1 "rojo: pre-release (P7)" herramienta empaquetar --publicado salida/humo/publicado --version 1.1.0-beta.1
  espera 1 "rojo: versión no mayor (P2)" herramienta empaquetar --publicado salida/humo/publicado --version 0.9.0
  printf '<Project><PropertyGroup><RunAOTCompilation>true</RunAOTCompilation></PropertyGroup></Project>\n' > salida/humo/ConAot.csproj
  espera 1 "rojo: AOT (P1)" herramienta empaquetar --publicado salida/humo/publicado --version 1.1.0 --csproj salida/humo/ConAot.csproj
  printf 'navigator.geolocation.getCurrentPosition(() => {});\n' > "$sitio/js/geo.js"
  espera 1 "rojo: API web con permiso (P12)" herramienta empaquetar --publicado salida/humo/publicado --version 1.1.0
  rm "$sitio/js/geo.js"; printf 'export const v = 2;\n' > "$sitio/js/puente.js"
  espera 0 "empaquetar 1.1.0" herramienta empaquetar --publicado salida/humo/publicado --version 1.1.0 --commit humo002

  paso "humo 3: definiciones"
  espera 2 "falta --cambia-datos (P6)" herramienta publicar-definicion --definicion definiciones/encuesta-movilidad/v1.json
  espera 0 "definición v1" herramienta publicar-definicion --definicion definiciones/encuesta-movilidad/v1.json --cambia-datos si
  "$C" python3 -c '
import json, sys
d = json.load(open(sys.argv[1])); d["pasos"][0]["preguntas"][1]["tipo"] = "texto"; d["version"] = 2
json.dump(d, open(sys.argv[2], "w"), ensure_ascii=False, indent=2)' definiciones/encuesta-movilidad/v1.json salida/humo/v2-destructiva.json
  espera 1 "rojo: cambio de tipo de una pregunta publicada (P13.8)" herramienta publicar-definicion --definicion salida/humo/v2-destructiva.json --cambia-datos no
  espera 0 "definición v2" herramienta publicar-definicion --definicion definiciones/encuesta-movilidad/v2.json --cambia-datos si
  espera 1 "rojo: republicar v2 con otro contenido (P13.9)" herramienta publicar-definicion --definicion salida/humo/v2-destructiva.json --cambia-datos no

  paso "humo 4: firmar y verificar"
  espera 0 "firmar" herramienta firmar --clave "$k/privada-demo.pem"
  espera 0 "verificar" herramienta verificar --publica "$k/publica-demo.pem"
  espera 1 "rojo: verificar con reloj después de expires (IndiceVencido)" herramienta verificar --publica "$k/publica-demo.pem" --ahora 2099-01-01T00:00:00Z
  espera 0 "firma-invalida" herramienta firma-invalida
  espera 1 "rojo: verificar índice alterado (FirmaInvalida)" herramienta verificar --publica "$k/publica-demo.pem"
  espera 0 "firmar de nuevo" herramienta firmar --clave "$k/privada-demo.pem"
  espera 0 "verificar después de firmar" herramienta verificar --publica "$k/publica-demo.pem"
  espera 1 "rojo: verificar con la pública versionada de claves/ (otro par: FirmaInvalida)" herramienta verificar --publica claves/publica-demo.pem

  paso "humo 5: retiros"
  espera 1 "rojo: retiro de definición sin 30 días de aviso (P13.5)" herramienta retirar --definicion encuesta-movilidad@1 --retiro-el "$(date -u -d '+10 days' +%F)"
  espera 0 "retiro de definición v1 con 31 días de aviso" herramienta retirar --definicion encuesta-movilidad@1 --retiro-el "$(date -u -d '+31 days' +%F)"
  espera 0 "retirar contenido 1.1.0" herramienta retirar --contenido 1.1.0
  espera 0 "firmar con los retiros" herramienta firmar --clave "$k/privada-demo.pem"
  grep -q '"retiradas": \[' "$CARPETA/indice.json" && sed -n '/"retiradas"/,/^  "cascaron\|^}/p' "$CARPETA/indice.json"

  paso "humo 6: receptor (canal estático y /respuestas) en el puerto $PUERTO"
  cmd_servir >/dev/null
  local base="http://localhost:$PUERTO" r
  http() {  # $1 = código esperado, $2 = descripción, resto = curl
    local esperado="$1" que="$2"; shift 2
    r="$(curl -sS -o salida/humo/cuerpo.json -w '%{http_code}' "$@")"
    if [ "$r" = "$esperado" ]; then ok=$((ok+1)); echo "   OK   $r $que"; else malo=$((malo+1)); echo "   MAL  $r $que (se esperaba $esperado)"; cat salida/humo/cuerpo.json; echo; fi
  }
  http 200 "GET indice.json" "$base/$CANAL/puente-$PUENTE_MAYOR/indice.json"
  http 200 "GET indice.json.sig" "$base/$CANAL/puente-$PUENTE_MAYOR/indice.json.sig"
  http 200 "GET paquetes/contenido-1.0.0.zip" "$base/$CANAL/puente-$PUENTE_MAYOR/paquetes/contenido-1.0.0.zip"
  http 404 "GET estado-de-publicacion.json (no se sirve)" "$base/$CANAL/puente-$PUENTE_MAYOR/estado-de-publicacion.json"
  http 404 "GET con ../ (no sale de la carpeta)" --path-as-is "$base/$CANAL/puente-$PUENTE_MAYOR/definiciones/../../../../scripts/guion.sh"
  http 404 "GET canal desconocido" "$base/otro/puente-$PUENTE_MAYOR/indice.json"
  curl -sS "$base/$CANAL/puente-$PUENTE_MAYOR/paquetes/contenido-1.0.0.zip" -o salida/humo/bajado.zip
  cmp salida/humo/bajado.zip "$CARPETA/paquetes/contenido-1.0.0.zip" && { ok=$((ok+1)); echo "   OK   el zip servido es idéntico byte a byte"; } || malo=$((malo+1))

  local envio1='{"claveIdempotencia":"humo-0001","encuestaId":"encuesta-movilidad","versionDefinicion":1,"versionContenido":"1.0.0+humo001","registradaEn":"2026-10-25T23:00:00Z","valores":{"nombre":"Ana Pérez","edad":34,"localidad":"1","medios":["colectivo","tren"],"frecuencia":"diaria","distancia":12.5,"minutos":45,"motivo":"trabajo"}}'
  local j=(-H 'Content-Type: application/json')
  http 201 "POST respuesta nueva (definición v1, publicada)" "${j[@]}" -X POST --data "$envio1" "$base/respuestas"
  cat salida/humo/cuerpo.json; echo
  http 200 "POST la misma otra vez → no duplica" "${j[@]}" -X POST --data "$envio1" "$base/respuestas"
  grep -q '"duplicada": true' salida/humo/cuerpo.json && { ok=$((ok+1)); echo "   OK   duplicada: true"; } || malo=$((malo+1))
  local envio1b="${envio1/Ana Pérez/Ana María Pérez}"
  http 428 "POST misma clave, otro contenido, sin If-Match" "${j[@]}" -X POST --data "$envio1b" "$base/respuestas"
  http 412 "POST con If-Match viejo" "${j[@]}" -H 'If-Match: "7"' -X POST --data "$envio1b" "$base/respuestas"
  http 200 "POST con If-Match correcto → versión 2" "${j[@]}" -H 'If-Match: "1"' -X POST --data "$envio1b" "$base/respuestas"
  http 412 "If-Match sobre una clave que no existe" "${j[@]}" -H 'If-Match: "1"' -X POST --data "${envio1/humo-0001/humo-9999}" "$base/respuestas"
  local envio2="${envio1/humo-0001/humo-0002}"; envio2="${envio2/\"versionDefinicion\":1/\"versionDefinicion\":7}"
  http 422 "POST con definición no publicada → carta muerta" "${j[@]}" -X POST --data "$envio2" "$base/respuestas"
  local envio3="${envio1/humo-0001/humo-0003}"; envio3="${envio3/\"edad\":34/\"edad\":12}"
  http 422 "POST con valores que no cumplen v1 → carta muerta" "${j[@]}" -X POST --data "$envio3" "$base/respuestas"
  local envio4="${envio1/humo-0001/humo-0004}"; envio4="${envio4/1.0.0+humo001/1.1.0+humo002}"
  http 201 "POST tomada con contenido 1.1.0 RETIRADO → se acepta (P13.10)" "${j[@]}" -X POST --data "$envio4" "$base/respuestas"
  http 400 "POST mal formado" "${j[@]}" -X POST --data '{"claveIdempotencia":' "$base/respuestas"

  paso "humo 7: el mismo receptor con el reloj 32 días adelante (retiro de la definición v1 cumplido)"
  FC_AHORA="$(date -u -d '+32 days' +%FT%TZ)" cmd_servir >/dev/null
  local envio5="${envio1/humo-0001/humo-0005}"
  http 422 "POST con definición v1 retirada → carta muerta (P13.5)" "${j[@]}" -X POST --data "$envio5" "$base/respuestas"
  cat salida/humo/cuerpo.json; echo
  local envio6="${envio1/humo-0001/humo-0006}"; envio6="${envio6/\"versionDefinicion\":1/\"versionDefinicion\":2}"
  http 201 "POST con definición v2 → aceptada" "${j[@]}" -X POST --data "$envio6" "$base/respuestas"
  http 200 "GET /respuestas" "$base/respuestas"
  "$C" python3 -c '
import json, sys
d = json.load(open(sys.argv[1]))
print("   aceptadas:", [ (r["clave"], r["version"]) for r in d["aceptadas"] ])
print("   cartaMuerta:", [ (r["clave"], r["motivo"]) for r in d["cartaMuerta"] ])
print("   reenviosQueNoSeDuplicaron:", d["reenviosQueNoSeDuplicaron"])' salida/humo/cuerpo.json
  docker rm -f "$RECEPTOR" >/dev/null
  RAIZ="$raiz_demo"

  paso "humo: $ok OK, $malo MAL"
  [ "$malo" = 0 ]
}

cmd_limpiar() {
  cmd_parar
  find "$DEMO" \( -name bin -o -name obj \) -type d -prune -exec rm -rf {} +
  rm -rf "$DEMO/salida"
}

uso() { sed -n '2,/^set -euo/p' "$0" | sed '$d' | sed 's/^# \{0,1\}//'; }

sub="${1:-}"; [ "$#" -gt 0 ] && shift
case "$sub" in
  claves) cmd_claves "$@" ;;
  build) cmd_build ;;
  test) cmd_test ;;
  publicar) cmd_publicar "$@" ;;
  definicion) cmd_definicion "$@" ;;
  firmar) cmd_firmar "$@" ;;
  verificar) cmd_verificar "$@" ;;
  firma-invalida) cmd_firma_invalida "$@" ;;
  retirar) cmd_retirar "$@" ;;
  semilla) cmd_semilla ;;
  tls) cmd_tls "$@" ;;
  respuestas) cmd_respuestas ;;
  servir) cmd_servir ;;
  tunel) cmd_tunel ;;
  parar) cmd_parar ;;
  apk) cmd_apk ;;
  dispositivo) cmd_dispositivo "$@" ;;
  instalar) cmd_instalar "$@" ;;
  demo) cmd_demo "$@" ;;
  tpr) cmd_tpr "$@" ;;
  humo) cmd_humo ;;
  limpiar) cmd_limpiar ;;
  *) uso; exit 2 ;;
esac
