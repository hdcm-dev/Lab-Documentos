#!/usr/bin/env bash
# Corre los experimentos de N2 (E2-1 a E2-7; E2-8 es un relevamiento fechado que no se re-corre) y deja la salida en salidas/. Se ejecuta en el HOST (necesita docker).
set -uo pipefail
cd "$(dirname "$0")"
docker build -q -t lab-n2-texto:local tools/ >/dev/null
lab() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab lab-n2-texto:local bash "$1"; }
for e in e1-precedencia e2-migracion e3-compuerta e4-working-tree-encoding; do
  lab "$e/run.sh" > "salidas/$e.txt" 2>&1; echo "$e: exit $?"
done
bash e5-bom-por-lector/run.sh > salidas/e5-bom-por-lector.txt 2>&1; echo "e5-bom-por-lector: exit $?"
for v in 8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743 \
         10.0@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c; do
  docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -v "$PWD":/lab -w /lab "mcr.microsoft.com/dotnet/sdk:$v" bash e6-plantillas-dotnet/run.sh
done > salidas/e6-plantillas-dotnet.txt 2>&1; echo "e6-plantillas-dotnet: exit $?"
bash e7-editorconfig-checker/run.sh > salidas/e7-editorconfig-checker.txt 2>&1; echo "e7-editorconfig-checker: exit $? (se espera 0: el script informa rc=1 de la herramienta en la salida)"
