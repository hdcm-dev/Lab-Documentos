#!/usr/bin/env python3
"""Compuerta de texto mínima para la solución políglota (N8).

Versión reducida de la compuerta de N2 (que lee .editorconfig con la biblioteca
de referencia). Acá las reglas por extensión se escriben a mano para no
depender de paquetes: deben coincidir con .editorconfig.
  - todo archivo de texto es UTF-8 válido;
  - BOM sólo en .razor, .cshtml, .sln y .ps1 (y obligatorio ahí);
  - CR sólo en .cmd y .bat.
"""
import sys
from pathlib import Path

BOM = b"\xef\xbb\xbf"
CON_BOM = {".razor", ".cshtml", ".sln", ".ps1"}
CON_CRLF = {".cmd", ".bat"}
TEXTO = {".cs", ".razor", ".cshtml", ".sln", ".csproj", ".props", ".targets", ".json",
         ".js", ".mjs", ".ts", ".py", ".toml", ".lock", ".sh", ".cmd", ".bat", ".ps1", ".md",
         ".yml", ".yaml", ".txt", ".config", ".npmrc", ".editorconfig", ".gitattributes",
         ".nvmrc", ".python-version"}
EXCLUIR = {"bin", "obj", "node_modules", ".venv", "dist", ".git"}

raiz = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
fallas = 0
for p in sorted(raiz.rglob("*")):
    if not p.is_file() or EXCLUIR & set(p.relative_to(raiz).parts):
        continue
    ext = p.suffix.lower() or p.name.lower()
    if ext not in TEXTO:
        continue
    datos = p.read_bytes()
    rel = p.relative_to(raiz)
    problemas = []
    try:
        datos.decode("utf-8")
    except UnicodeDecodeError as e:
        problemas.append(f"no es UTF-8 (byte 0x{datos[e.start]:02x} en {e.start})")
    if ext in CON_BOM and not datos.startswith(BOM):
        problemas.append("falta BOM")
    if ext not in CON_BOM and datos.startswith(BOM):
        problemas.append("BOM no permitido")
    if ext not in CON_CRLF and b"\r" in datos:
        problemas.append("tiene CR (CRLF)")
    for pr in problemas:
        print(f"texto: {rel}: {pr}")
        fallas += 1
print(f"texto: {'OK' if fallas == 0 else f'{fallas} violación(es)'}")
sys.exit(1 if fallas else 0)
