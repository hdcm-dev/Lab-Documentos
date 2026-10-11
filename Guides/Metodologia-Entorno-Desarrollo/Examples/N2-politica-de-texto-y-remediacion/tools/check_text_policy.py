#!/usr/bin/env python3
"""Compuerta de política de texto (N2).

Verifica, sobre el CONTENIDO DEL ÍNDICE de git (lo que se va a commitear o lo
que ya está commiteado), que cada archivo de texto cumpla la política que el
repositorio declara en .editorconfig y .gitattributes:

  1. Es UTF-8 válido (si no lo es, casi siempre es cp1252/latin1 heredado).
  2. Tiene o no tiene BOM según el `charset` que .editorconfig resuelve para
     esa ruta (`utf-8` prohíbe BOM, `utf-8-bom` lo exige).
  3. Está guardado con LF en el índice (si git lo ve como CRLF o mixto, el
     .gitattributes no lo está normalizando o se lo declaró -text).

La fuente de verdad es el propio .editorconfig: el script no repite la política,
la lee con la implementación de referencia de EditorConfig para Python
(paquete PyPI `EditorConfig`, https://github.com/editorconfig/editorconfig-core-py).

Modos:
  --staged          archivos en stage (para el hook pre-commit)
  --base <rev>      archivos cambiados entre <rev> y HEAD (para un PR en CI)
  (sin argumentos)  todos los archivos versionados

Sale con código 1 si encuentra al menos una violación.
"""
import argparse
import os
import subprocess
import sys

import editorconfig  # pip install EditorConfig==0.17.1

BOM = b"\xef\xbb\xbf"


def git(*args: str) -> str:
    return subprocess.run(["git", *args], check=True, capture_output=True, text=True).stdout


def git_bytes(*args: str) -> bytes:
    return subprocess.run(["git", *args], check=True, capture_output=True).stdout


def candidate_paths(args) -> list[str]:
    if args.staged:
        out = git("diff", "--cached", "--name-only", "--diff-filter=ACMR", "-z")
    elif args.base:
        out = git("diff", "--name-only", "--diff-filter=ACMR", "-z", f"{args.base}...HEAD")
    else:
        out = git("ls-files", "-z")
    return [p for p in out.split("\0") if p]


def index_eol_info(paths: list[str]) -> dict[str, str]:
    """Devuelve la columna i/... de `git ls-files --eol` (eol en el índice)."""
    info = {}
    if not paths:
        return info
    out = git("ls-files", "--eol", "-z", "--", *paths)
    for rec in out.split("\0"):
        if not rec:
            continue
        meta, path = rec.split("\t", 1)
        info[path] = meta.split()[0]  # p. ej. "i/lf", "i/crlf", "i/mixed", "i/-text"
    return info


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--staged", action="store_true")
    ap.add_argument("--base")
    args = ap.parse_args()

    root = git("rev-parse", "--show-toplevel").strip()
    os.chdir(root)
    paths = candidate_paths(args)
    eol = index_eol_info(paths)
    violations = []

    for path in paths:
        if eol.get(path) in ("i/-text", "i/none"):
            continue  # binario o vacío: fuera de la política de texto
        blob = git_bytes("show", f":{path}")  # bytes tal como están en el índice
        props = editorconfig.get_properties(os.path.abspath(path))
        charset = props.get("charset")

        try:
            blob.decode("utf-8")
        except UnicodeDecodeError as e:
            violations.append(f"{path}: no es UTF-8 válido (byte 0x{blob[e.start]:02x} en offset {e.start}; ¿cp1252?)")
            continue

        has_bom = blob.startswith(BOM)
        if charset == "utf-8" and has_bom:
            violations.append(f"{path}: tiene BOM y .editorconfig declara charset=utf-8 (sin BOM)")
        elif charset == "utf-8-bom" and not has_bom:
            violations.append(f"{path}: falta el BOM que .editorconfig declara (charset=utf-8-bom)")

        if eol.get(path) in ("i/crlf", "i/mixed"):
            violations.append(f"{path}: {eol[path]} en el índice (el repositorio debe guardar LF)")

    for v in violations:
        print(f"POLITICA-TEXTO: {v}")
    print(f"POLITICA-TEXTO: {len(paths)} archivo(s) revisado(s), {len(violations)} violación(es)")
    return 1 if violations else 0


if __name__ == "__main__":
    sys.exit(main())
