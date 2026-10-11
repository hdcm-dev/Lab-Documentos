#!/usr/bin/env bash
# Orquestador de la build políglota (variante "el orquestador llama a todos").
# Orden: compuerta -> Python (genera código C#) -> Node (genera el bundle) ->
# .NET (consume ambos) -> registro del entorno. MSBuild NO vuelve a llamar a
# npm ni a uv (ConstruirPoliglota=false): cada ecosistema corre una sola vez.
set -euo pipefail
cd "$(dirname "$0")"
./verificar.sh
(cd tools/gen && uv run --locked python gen.py dist/Datos.g.cs)
(cd web && npm run build)
dotnet build src/Saludo -c Release --no-restore -p:ConstruirPoliglota=false \
  -p:ContinuousIntegrationBuild=true -v q --nologo
mkdir -p dist
./registrar-entorno.sh > dist/entorno.txt
cat dist/entorno.txt
