#!/usr/bin/env python3
"""E1-N1-12 — Datos de Unicode y de cp1252 que el núcleo usa como premisas.
  docker run --rm -v "$PWD":/w -w /w python:3.12-slim python -I e1-unicode.py
1) Categoría general de Ç, ó y U+FFFD (¿puede U+FFFD formar parte de un identificador C#?).
2) ¿En qué bytes difieren cp1252 y Latin-1 (ISO-8859-1)?
"""
import unicodedata, sys
print("Python", sys.version.split()[0], "| Unicode", unicodedata.unidata_version)
for ch in ("Ç", "ó", "�", "﻿"):
    print(f"  U+{ord(ch):04X} {unicodedata.name(ch)}: categoría {unicodedata.category(ch)}")
dif = []
for b in range(256):
    l1 = bytes([b]).decode("latin-1")
    try:
        c = bytes([b]).decode("cp1252")
    except UnicodeDecodeError:
        c = None
    if c != l1:
        dif.append(b)
print(f"  bytes donde cp1252 != Latin-1: {len(dif)}, rango {min(dif):#04x}-{max(dif):#04x}")
print("  en Latin-1 esos bytes son controles C1:", all(unicodedata.category(bytes([b]).decode('latin-1')) == "Cc" for b in dif))
nodef = []
for b in dif:
    try: bytes([b]).decode("cp1252")
    except UnicodeDecodeError: nodef.append(f"{b:#04x}")
print("  bytes sin carácter en cp1252 (Python):", " ".join(nodef))
