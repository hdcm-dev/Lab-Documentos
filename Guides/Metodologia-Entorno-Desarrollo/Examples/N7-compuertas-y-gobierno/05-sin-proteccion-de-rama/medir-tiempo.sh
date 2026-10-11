#!/usr/bin/env bash
# Verificación adversarial (2026-10-07): ¿cuánto tarda la compuerta completa?
# Contexto: GitHub Enterprise Server da a TODOS los pre-receive de un repo un
# presupuesto acumulado de 5 s y desaconseja llamar servicios externos desde ellos.
# Corre dentro de lab-n2-texto:local (ver correr-todo.sh para el `docker run`).
set -u
source "$(dirname "$0")/../comun.sh"; preparar 05t; cd "$W"
bash "$N7/compuerta/sembrar-repo.sh" "$W/r" >/dev/null 2>&1; cd "$W/r"
seg() { python3 -c "import sys;print(round(float(sys.argv[2])-float(sys.argv[1]),2))" "$1" "$(date +%s.%N)"; }
for i in 1 2 3; do s=$(date +%s.%N); tools/compuerta.sh >/dev/null 2>&1; r=$?; echo "compuerta completa, caché NuGet tibia, corrida $i: rc=$r $(seg "$s") s"; done
s=$(date +%s.%N); NUGET_PACKAGES="$W/nuget-vacia" tools/compuerta.sh >/dev/null 2>&1; r=$?; echo "compuerta completa, caché NuGet vacía (baja de nuget.org): rc=$r $(seg "$s") s"
echo "(repo de juguete: 1 proyecto, 1 paquete; un repo real con decenas de proyectos y paquetes tarda más)"
