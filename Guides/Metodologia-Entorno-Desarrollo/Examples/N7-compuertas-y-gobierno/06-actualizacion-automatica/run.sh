#!/usr/bin/env bash
# E7-06 — ¿Las configuraciones de los bots son válidas según sus esquemas publicados?
# Sólo valida SINTAXIS contra el esquema; el comportamiento del bot no se puede
# ejecutar fuera de la plataforma (se documenta con las fuentes oficiales).
set -u
cd "$(dirname "$0")"
python3 -m venv /tmp/v >/dev/null && /tmp/v/bin/pip install -q --disable-pip-version-check jsonschema==4.26.0 pyyaml==6.0.3
/tmp/v/bin/python - <<'PY'
import json, urllib.request, yaml, jsonschema
casos = [("dependabot.yml", "https://json.schemastore.org/dependabot-2.0.json", yaml.safe_load),
         ("renovate.json", "https://docs.renovatebot.com/renovate-schema.json", json.load)]
for archivo, url, carga in casos:
    esquema = json.load(urllib.request.urlopen(url, timeout=60))
    datos = carga(open(archivo, encoding="utf-8"))
    errores = sorted(jsonschema.Draft7Validator(esquema).iter_errors(datos), key=lambda e: list(e.path))
    print(f"{archivo}: esquema {url} -> {'VÁLIDO' if not errores else f'{len(errores)} error(es)'}")
    for e in errores[:6]:
        print(f"   en {'/'.join(map(str, e.path)) or '(raíz)'}: {e.message[:160]}")
    if archivo == "dependabot.yml":   # control negativo: el validador SÍ rechaza
        malo = dict(datos); malo["updates"] = [dict(datos["updates"][0], **{"package-ecosystem": "dotnet-sdks"}),
                                               dict(datos["updates"][1], cooldown={"default-days": "siete"})]
        errs = list(jsonschema.Draft7Validator(esquema).iter_errors(malo))
        print(f"   control negativo (ecosistema inexistente + cooldown no numérico): {len(errs)} error(es) detectado(s)")
PY
