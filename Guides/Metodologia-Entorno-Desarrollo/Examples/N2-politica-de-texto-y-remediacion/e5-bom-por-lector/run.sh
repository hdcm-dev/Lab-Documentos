#!/usr/bin/env bash
# E5 — ¿Qué hace cada lector con un BOM en ARCHIVOS DE CONFIGURACIÓN y scripts?
# (La matriz de lectores de código fuente está en el laboratorio de N1.)
# Se corre en el host: lanza un contenedor por ecosistema, con imagen fijada por digest.
set -uo pipefail
cd "$(dirname "$0")/.."
LAB=$PWD; W=$LAB/work/e5; rm -rf "$W"; mkdir -p "$W"
NODE=node@sha256:d649c27dae7ba0137b3cef5dd75baa422c08dc3d9e3fc0c23dfb172dc3cc6436        # node:22-bookworm-slim
PY=python@sha256:78387bc3881b8273120a12ebe6c1ab22b018ccc2c9adf565ae1ac9b536e184ea          # python:3.12-slim
SDK=mcr.microsoft.com/dotnet/sdk@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743  # sdk:8.0
BOM=$'\xef\xbb\xbf'
run() { docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_TELEMETRY_OPTOUT=1 -e DOTNET_NOLOGO=1 -v "$W":/w -w /w "$@"; }

printf '%s{"name":"demo","version":"1.0.0","scripts":{"hola":"echo script ok"}}\n' "$BOM" > "$W/package.json"
printf '%s{"clave":"valor"}\n' "$BOM" > "$W/datos.json"
printf '%s[project]\nname = "demo"\nversion = "1.0.0"\n' "$BOM" > "$W/pyproject.toml"
printf '%s#!/bin/sh\necho "script ok"\n' "$BOM" > "$W/con-bom.sh"; chmod +x "$W/con-bom.sh"

echo "== Shell: shebang precedido de BOM"
echo "  -- invocado desde un shell (el shell reintenta leyéndolo como script propio):"
run "$SDK" sh -c './con-bom.sh; echo "exit=$?"' 2>&1 | sed 's/^/    /'
echo "  -- invocado por execve directo, sin el reintento de la libc (os.execv de Python):"
run "$PY" python -c 'import os; os.execv("./con-bom.sh", ["con-bom.sh"])' 2>&1 | tail -1 | sed 's/^/    /'
echo "  -- como comando de un contenedor (docker run <imagen> ./con-bom.sh):"
run "$SDK" ./con-bom.sh 2>&1 | sed 's/^/    /'

echo; echo "== Node.js $(run "$NODE" node --version) / npm $(run "$NODE" npm --version)"
run "$NODE" node -e '
const fs=require("fs");
try { JSON.parse(fs.readFileSync("datos.json","utf8")); console.log("JSON.parse(readFileSync utf8): ok"); }
catch(e){ console.log("JSON.parse(readFileSync utf8): " + e.name + ": " + e.message.split("\n")[0]); }
console.log("require(\"./datos.json\"): " + JSON.stringify(require("./datos.json")));
' 2>&1 | sed 's/^/    /'
echo "    npm run hola con package.json con BOM:"; run "$NODE" npm run -s hola 2>&1 | sed 's/^/      /'

echo; echo "== Python $(run "$PY" python -c 'import sys;print(sys.version.split()[0])')"
run -i "$PY" python - <<'PY' 2>&1 | sed 's/^/    /'
import json, tomllib
for enc in ("utf-8", "utf-8-sig"):
    try:
        json.load(open("datos.json", encoding=enc)); print(f"json.load(encoding={enc!r}): ok")
    except Exception as e:
        print(f"json.load(encoding={enc!r}): {type(e).__name__}: {e}")
try:
    tomllib.load(open("pyproject.toml", "rb")); print("tomllib.load(pyproject.toml): ok")
except Exception as e:
    print(f"tomllib.load(pyproject.toml): {type(e).__name__}: {e}")
PY

echo; echo "== .NET $(run "$SDK" dotnet --version)"
mkdir -p "$W/net"; cp "$W/datos.json" "$W/net/"
printf '%s<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup>\n</Project>\n' "$BOM" > "$W/net/Net.csproj"
cat > "$W/net/Program.cs" <<'CS'
using System.Text.Json;
var bytes = File.ReadAllBytes("datos.json");
try { JsonDocument.Parse(bytes); Console.WriteLine("JsonDocument.Parse(bytes con BOM): ok"); }
catch (Exception e) { Console.WriteLine($"JsonDocument.Parse(bytes con BOM): {e.GetType().Name}: {e.Message.Split('\n')[0]}"); }
JsonDocument.Parse(File.ReadAllText("datos.json")); Console.WriteLine("JsonDocument.Parse(File.ReadAllText): ok");
CS
echo "    Net.csproj CON BOM; build + run:"
run -w /w/net "$SDK" sh -c 'dotnet run -v q 2>&1' | grep -v 'verifying workloads' | sed 's/^/      /'
