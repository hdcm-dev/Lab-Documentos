#!/bin/sh
# E1-N1-10 — Qué codificación y EOL producen las plantillas del propio SDK de .NET.
# Se corre DENTRO del contenedor del SDK. Trabaja en /tmp: no deja nada en el repo.
set -u
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
cd /tmp && rm -rf plantillas && mkdir plantillas && cd plantillas || exit 1
echo "SDK $(dotnet --version)"
dotnet new console -o c >/dev/null 2>&1
dotnet new editorconfig -o e >/dev/null 2>&1
for f in c/Program.cs c/c.csproj; do
  printf '  %-14s primeros bytes: %s | CR: %s\n' "$f" "$(head -c3 "$f" | od -An -tx1 | tr -s ' ')" "$(tr -cd '\r' < "$f" | wc -c)"
done
cs=$(grep -E '^charset' e/.editorconfig | sort -u | tr '\n' ' '); eol=$(grep -E '^end_of_line' e/.editorconfig | sort -u | tr '\n' ' ')
printf '  .editorconfig  charset: %s | end_of_line: %s\n' "${cs:-(no lo declara)}" "${eol:-(no lo declara)}"
