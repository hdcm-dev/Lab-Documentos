#!/usr/bin/env bash
# Emisor OIDC de ENSAYO para la demo (P4 del folio 035, D-07 = I1: «Keycloak en contenedor para ensayar»; folio 038).
# No es un emisor de producción: modo start-dev, base H2 dentro del contenedor, usuarios con clave conocida.
#
#   infra/keycloak/keycloak.sh certificado   certificado HTTPS del emisor, firmado por la CA de demostración
#                                            (claves/tls/keycloak.crt; hace falta «scripts/guion.sh tls» antes)
#   infra/keycloak/keycloak.sh levantar      contenedor fc-keycloak en https://$FC_KC_IP:$FC_KC_PUERTO con el realm fc-demo
#   infra/keycloak/keycloak.sh probar        recorre el flujo code + PKCE con curl (sin navegador) y valida el token
#   infra/keycloak/keycloak.sh estado        descubrimiento y JWKS
#   infra/keycloak/keycloak.sh parar         baja el contenedor (el realm se reimporta al levantar)
#
# Qué levanta (todo en el contenedor, nada en la máquina):
#   imagen   quay.io/keycloak/keycloak:26.7.5 fijada por digest (publicada el 2026-09-30)
#   realm    infra/keycloak/realm-fc-demo.json, importado al arrancar (--import-realm):
#              cliente público fc-demo-app, authorization code con PKCE S256 obligatorio y, SÓLO en este emisor de
#              ensayo, Direct Access Grant (grant_type=password) para el login tradicional de la demo
#              (-p:FcModoDeIngreso=Formulario). Un Keycloak ya levantado con el realm viejo contesta
#              unauthorized_client: «parar» y «levantar» para reimportarlo, o activarlo en la consola de admin,
#              redirect com.frameworkconcept.demo://callback (RFC 8252 §7.1),
#              mapper de audiencia «fc-receptor» (lo exige el receptor, DISENO §5),
#              access token 5 min, sesión 30 min sin uso / 10 h máximo,
#              usuarios encuestador / demo-encuestador y supervisora / demo-supervisora (dos dueños para P4)
#   TLS      claves/tls/keycloak.{crt,key} (no se versionan), firmados por claves/tls/ca_demo_tls.crt, la misma CA en la
#            que confía el APK de Debug (network_security_config, debug-overrides) y el receptor (--oidc-ca)
#   admin    usuario «admin», clave aleatoria en salida/keycloak/admin.txt (no se versiona)
#
# Emisor (issuer) resultante: https://$FC_KC_IP:$FC_KC_PUERTO/realms/fc-demo  → -p:FcOidcAutoridad del APK y
# --oidc-emisor del receptor.
#
# Variables: FC_KC_IP (203.0.113.10, la IP de la máquina que sirve Keycloak, por la que llega el emulador fc-android-emu-hpdev),
#            FC_KC_PUERTO (8443), FC_KC_NOMBRE (fc-keycloak).
#            FC_KC_HOSTNAME: URL pública con la que se anuncia el emisor si llega por un túnel (p. ej. un teléfono real
#            que no alcanza la máquina); vacía = https://$FC_KC_IP:$FC_KC_PUERTO.
set -euo pipefail

DEMO="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$DEMO"

IMAGEN="quay.io/keycloak/keycloak:26.7.5@sha256:37dbaf6f0722c9ec246335f36e1ef8b2e6cb960f7c27e0d8c615121a3d475a85"
IP="${FC_KC_IP:-203.0.113.10}"
PUERTO="${FC_KC_PUERTO:-8443}"
NOMBRE="${FC_KC_NOMBRE:-fc-keycloak}"
TLS=claves/tls
REALM=fc-demo
EMISOR="https://$IP:$PUERTO/realms/$REALM"
CLIENTE=fc-demo-app
REDIRECT="com.frameworkconcept.demo://callback"
SALIDA=salida/keycloak

paso() { printf '\n== %s\n' "$*"; }
falla() { echo "keycloak: $*" >&2; exit 1; }

cmd_certificado() {
  [ -f "$TLS/ca_demo_tls.crt" ] && [ -f "$TLS/ca_demo_tls.key" ] || falla "falta la CA de demostración: corré «FC_TLS_IPS=$IP scripts/guion.sh tls»"
  if [ -f "$TLS/keycloak.crt" ] && [ "${1:-}" != --forzar ]; then
    echo "certificado: ya existe $TLS/keycloak.crt (--forzar para regenerarlo)"; return 0
  fi
  paso "certificado del emisor (SAN localhost, 127.0.0.1, $IP), firmado por la CA de demostración"
  # openssl dentro del contenedor de compilación de la demo, como «guion.sh tls».
  FC_NOMBRE=fc-keycloak-cert "$DEMO/scripts/contenedor.sh" bash -c 'set -e; d="$0"; ip="$1"
    openssl req -newkey ec -pkeyopt ec_paramgen_curve:P-256 -nodes -subj "/CN=emisor-oidc-de-ensayo" \
      -keyout "$d/keycloak.key" -out "$d/keycloak.csr" 2>/dev/null
    printf "subjectAltName=DNS:localhost,IP:127.0.0.1,IP:%s\nbasicConstraints=CA:FALSE\nextendedKeyUsage=serverAuth\n" "$ip" > "$d/keycloak.ext"
    openssl x509 -req -in "$d/keycloak.csr" -CA "$d/ca_demo_tls.crt" -CAkey "$d/ca_demo_tls.key" -CAcreateserial \
      -days 30 -extfile "$d/keycloak.ext" -out "$d/keycloak.crt" 2>/dev/null
    rm -f "$d/keycloak.csr" "$d/keycloak.ext" "$d/ca_demo_tls.srl"
    openssl x509 -in "$d/keycloak.crt" -noout -subject -ext subjectAltName -enddate' "$TLS" "$IP"
}

cmd_levantar() {
  [ -f "$TLS/keycloak.crt" ] || cmd_certificado
  mkdir -p "$SALIDA"
  if [ ! -f "$SALIDA/admin.txt" ]; then
    (umask 077; head -c 18 /dev/urandom | base64 | tr -d '/+=' > "$SALIDA/admin.txt")
  fi
  docker rm -f "$NOMBRE" >/dev/null 2>&1 || true
  paso "levantar $NOMBRE en $EMISOR"
  # La imagen corre como uid 1000; la clave del certificado es 0600 de quien corre el guion. Se monta en sólo lectura.
  docker run -d --name "$NOMBRE" -p "$PUERTO:8443" \
    -v "$DEMO/$TLS/keycloak.crt:/opt/keycloak/conf/tls/keycloak.crt:ro" \
    -v "$DEMO/$TLS/keycloak.key:/opt/keycloak/conf/tls/keycloak.key:ro" \
    -v "$DEMO/infra/keycloak/realm-fc-demo.json:/opt/keycloak/data/import/realm-fc-demo.json:ro" \
    -e KC_BOOTSTRAP_ADMIN_USERNAME=admin \
    -e KC_BOOTSTRAP_ADMIN_PASSWORD="$(cat "$SALIDA/admin.txt")" \
    "$IMAGEN" start-dev --import-realm \
      --https-certificate-file=/opt/keycloak/conf/tls/keycloak.crt \
      --https-certificate-key-file=/opt/keycloak/conf/tls/keycloak.key \
      --http-enabled=false \
      --hostname="${FC_KC_HOSTNAME:-https://$IP:$PUERTO}" >/dev/null
  local i
  for i in $(seq 1 90); do
    if curl -fsS --cacert "$TLS/ca_demo_tls.crt" "$EMISOR/.well-known/openid-configuration" >/dev/null 2>&1; then
      echo "listo en ${i}s: $EMISOR"; return 0
    fi
    sleep 1
  done
  docker logs "$NOMBRE" 2>&1 | tail -30
  falla "el emisor no respondió en 90 s"
}

cmd_estado() {
  paso "descubrimiento"
  curl -fsS --cacert "$TLS/ca_demo_tls.crt" "$EMISOR/.well-known/openid-configuration" | python3 -c '
import json, sys
d = json.load(sys.stdin)
for k in ("issuer", "authorization_endpoint", "token_endpoint", "jwks_uri", "end_session_endpoint", "revocation_endpoint"):
    print(f"  {k}: {d.get(k)}")
print("  code_challenge_methods_supported:", d.get("code_challenge_methods_supported"))'
  paso "JWKS"
  curl -fsS --cacert "$TLS/ca_demo_tls.crt" "$EMISOR/protocol/openid-connect/certs" | python3 -c '
import json, sys
for k in json.load(sys.stdin)["keys"]:
    print("  kid=%s kty=%s alg=%s use=%s" % (k["kid"], k["kty"], k.get("alg"), k.get("use")))'
}

# El flujo del cascarón, sin navegador: pide la página de ingreso con PKCE, envía usuario y clave por el formulario,
# toma el «code» del redirect al esquema privado y lo canjea con el code_verifier. Comprueba además que sin el
# verificador el canje se rechaza (PKCE obligatorio). Usuario por defecto: encuestador.
cmd_probar() {
  local usuario="${1:-encuestador}" clave="${2:-demo-encuestador}"
  local trabajo; trabajo="$(mktemp -d "${TMPDIR:-/tmp}/fc-keycloak-XXXXXX")"
  trap 'rm -rf "$trabajo"' RETURN
  local ca="$TLS/ca_demo_tls.crt"
  local verificador desafio state
  verificador="$(head -c 32 /dev/urandom | base64 | tr '+/' '-_' | tr -d '=')"
  desafio="$(printf '%s' "$verificador" | openssl dgst -sha256 -binary | base64 | tr '+/' '-_' | tr -d '=')"
  state="$(head -c 12 /dev/urandom | base64 | tr '+/' '-_' | tr -d '=')"
  local redirect_enc; redirect_enc="$(python3 -c 'import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1],safe=""))' "$REDIRECT")"

  paso "1. autorización con PKCE (S256) como $usuario"
  curl -fsS --cacert "$ca" -c "$trabajo/jar" -o "$trabajo/login.html" \
    "$EMISOR/protocol/openid-connect/auth?client_id=$CLIENTE&response_type=code&scope=openid%20profile&redirect_uri=$redirect_enc&state=$state&code_challenge=$desafio&code_challenge_method=S256"
  local accion
  accion="$(python3 -c 'import re,html,sys;m=re.search(r"<form[^>]*id=\"kc-form-login\"[^>]*action=\"([^\"]+)\"",open(sys.argv[1]).read());print(html.unescape(m.group(1)) if m else "")' "$trabajo/login.html")"
  [ -n "$accion" ] || falla "no apareció el formulario de ingreso"
  echo "  formulario: ${accion%%\?*}"

  paso "2. usuario y clave → redirect al esquema privado"
  curl -sS --cacert "$ca" -b "$trabajo/jar" -c "$trabajo/jar" -o /dev/null -D "$trabajo/cabeceras" \
    --data-urlencode "username=$usuario" --data-urlencode "password=$clave" "$accion"
  local destino code
  destino="$(grep -i '^location:' "$trabajo/cabeceras" | tr -d '\r' | sed 's/^[Ll]ocation: //')"
  case "$destino" in "$REDIRECT"*) ;; *) falla "el redirect no fue al esquema privado: «$destino»" ;; esac
  python3 - "$destino" "$state" <<'PY' || falla "state distinto"
import sys, urllib.parse
q = urllib.parse.parse_qs(urllib.parse.urlsplit(sys.argv[1]).query)
sys.exit(0 if q.get("state", [""])[0] == sys.argv[2] else 1)
PY
  code="$(python3 -c 'import sys,urllib.parse;print(urllib.parse.parse_qs(urllib.parse.urlsplit(sys.argv[1]).query)["code"][0])' "$destino")"
  echo "  redirect: ${destino%%\?*}?state=…&code=… (state coincide)"

  paso "3. canje SIN code_verifier (tiene que fallar: PKCE obligatorio)"
  local sin
  sin="$(curl -sS --cacert "$ca" -o "$trabajo/sin.json" -w '%{http_code}' \
    -d grant_type=authorization_code -d client_id="$CLIENTE" --data-urlencode "redirect_uri=$REDIRECT" -d code="$code" \
    "$EMISOR/protocol/openid-connect/token")"
  echo "  HTTP $sin $(cat "$trabajo/sin.json")"
  [ "$sin" = 400 ] || falla "el canje sin verificador no se rechazó"

  # El code ya se intentó canjear: Keycloak lo invalida. Se repite el ingreso para el canje bueno.
  paso "4. ingreso nuevo y canje CON code_verifier"
  verificador="$(head -c 32 /dev/urandom | base64 | tr '+/' '-_' | tr -d '=')"
  desafio="$(printf '%s' "$verificador" | openssl dgst -sha256 -binary | base64 | tr '+/' '-_' | tr -d '=')"
  rm -f "$trabajo/jar"
  curl -fsS --cacert "$ca" -c "$trabajo/jar" -o "$trabajo/login.html" \
    "$EMISOR/protocol/openid-connect/auth?client_id=$CLIENTE&response_type=code&scope=openid%20profile&redirect_uri=$redirect_enc&state=$state&code_challenge=$desafio&code_challenge_method=S256"
  accion="$(python3 -c 'import re,html,sys;m=re.search(r"<form[^>]*id=\"kc-form-login\"[^>]*action=\"([^\"]+)\"",open(sys.argv[1]).read());print(html.unescape(m.group(1)) if m else "")' "$trabajo/login.html")"
  curl -sS --cacert "$ca" -b "$trabajo/jar" -c "$trabajo/jar" -o /dev/null -D "$trabajo/cabeceras" \
    --data-urlencode "username=$usuario" --data-urlencode "password=$clave" "$accion"
  destino="$(grep -i '^location:' "$trabajo/cabeceras" | tr -d '\r' | sed 's/^[Ll]ocation: //')"
  code="$(python3 -c 'import sys,urllib.parse;print(urllib.parse.parse_qs(urllib.parse.urlsplit(sys.argv[1]).query)["code"][0])' "$destino")"
  curl -fsS --cacert "$ca" -o "$trabajo/tokens.json" \
    -d grant_type=authorization_code -d client_id="$CLIENTE" --data-urlencode "redirect_uri=$REDIRECT" -d code="$code" \
    -d code_verifier="$verificador" "$EMISOR/protocol/openid-connect/token"
  python3 - "$trabajo/tokens.json" "$EMISOR" <<'PY'
import base64, json, sys
t = json.load(open(sys.argv[1]))
def claims(jwt):
    p = jwt.split(".")[1]
    return json.loads(base64.urlsafe_b64decode(p + "=" * (-len(p) % 4)))
a, i = claims(t["access_token"]), claims(t["id_token"])
aud = a["aud"] if isinstance(a["aud"], list) else [a["aud"]]
print(f"  token_type={t['token_type']} expires_in={t['expires_in']} refresh_expires_in={t.get('refresh_expires_in')} refresh_token={'sí' if t.get('refresh_token') else 'no'}")
print(f"  access: iss={a['iss']} aud={aud} azp={a.get('azp')} sub={a['sub']}")
print(f"  id:     sub={i['sub']} preferred_username={i.get('preferred_username')}")
assert a["iss"] == sys.argv[2], "issuer distinto"
assert "fc-receptor" in aud, "falta la audiencia fc-receptor"
assert a["sub"] == i["sub"], "sub distinto entre access e id token"
print("  OK: issuer, audiencia fc-receptor y sub coinciden")
PY
}

cmd_parar() { docker rm -f "$NOMBRE" >/dev/null 2>&1 && echo "parado $NOMBRE" || echo "$NOMBRE no estaba"; }

case "${1:-}" in
  certificado) shift; cmd_certificado "$@" ;;
  levantar) cmd_levantar ;;
  estado) cmd_estado ;;
  probar) shift; cmd_probar "$@" ;;
  parar) cmd_parar ;;
  *) sed -n '2,30p' "$0"; exit 2 ;;
esac
