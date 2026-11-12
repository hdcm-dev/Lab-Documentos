#!/usr/bin/env python3
"""Verificador de la lámina LD-01. Sale con código 1 si algo falla.

Chequea: tamaño A3 del SVG y del JPG (4961 x 3508 px, 300 dpi), una sola
familia tipográfica, numeración secuencial y coincidente con el .def.json,
presencia de leyenda y rótulo, textos que no se solapan entre sí ni con
líneas (salvo la insignia sobre su propia conexión), textos de parte dentro
de su caja y ausencia de palabras prohibidas.
"""
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ld01 import DEF, SVG, JPG, ancho  # noqa: E402

NS = "{http://www.w3.org/2000/svg}"
PROHIBIDAS = [r"onesignal", r"\b100\.\d", r"\bgpt", r"\bclaude\b", r"\bllama", r"\bgemini\b", r"\bmistral",
              r"\bqwen", r"\bphi-?\d", r"\bdeepseek", r"\bollama", r"\bsonnet\b", r"\bopus\b", r"\bhaiku\b",
              r"\bgemma\b", r"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"]
fallas, avisos = [], []


def caja_texto(t):
    x, y = float(t.get("x")), float(t.get("y"))
    tam = float(t.get("font-size"))
    w = ancho(t.text or "", tam, t.get("font-weight") == "bold")
    a = t.get("text-anchor", "start")
    x0 = x - w / 2 if a == "middle" else (x - w if a == "end" else x)
    return (x0, y - 0.76 * tam, x0 + w, y + 0.24 * tam)


def cruza(seg, r):
    (x1, y1), (x2, y2) = seg
    rx0, ry0, rx1, ry1 = r
    if x1 == x2:
        return rx0 < x1 < rx1 and min(y1, y2) < ry1 and max(y1, y2) > ry0
    if y1 == y2:
        return ry0 < y1 < ry1 and min(x1, x2) < rx1 and max(x1, x2) > rx0
    # oblicuo: muestreo
    for i in range(51):
        x, y = x1 + (x2 - x1) * i / 50, y1 + (y2 - y1) * i / 50
        if rx0 < x < rx1 and ry0 < y < ry1:
            return True
    return False


def main():
    d = json.loads(DEF.read_text(encoding="utf-8"))
    raiz = ET.parse(SVG).getroot()
    if raiz.get("width") != "420mm" or raiz.get("height") != "297mm" or raiz.get("viewBox") != "0 0 420 297":
        fallas.append("el SVG no es A3 apaisada 420x297 mm")

    familias = {raiz.get("font-family")}
    for el in raiz.iter():
        if el.get("font-family"):
            familias.add(el.get("font-family"))
        m = re.search(r"font-family\s*:\s*([^;]+)", el.get("style", ""))
        if m:
            familias.add(m.group(1).strip())
    familias.discard(None)
    if len(familias) != 1:
        fallas.append(f"familias tipográficas: {familias}")

    textos = list(raiz.iter(NS + "text"))
    # numeración
    nums = [int(t.get("data-num")) for t in textos if t.get("data-num")]
    esperados = [p["n"] for p in d["partes"]]
    if nums != sorted(nums) or nums != list(range(1, len(nums) + 1)):
        fallas.append(f"numeración no secuencial: {nums}")
    if nums != esperados:
        fallas.append("la numeración no coincide con el .def.json")
    for t in textos:
        if t.get("data-num") and t.text != t.get("data-num"):
            fallas.append(f"círculo {t.get('data-num')} dice {t.text}")

    roles = {el.get("data-rol") for el in raiz.iter() if el.get("data-rol")}
    for r in ("titulo", "leyenda", "rotulo", "tabla", "conexiones"):
        if r not in roles:
            fallas.append(f"falta {r}")
    if not any(t.get("data-leyenda") for t in textos):
        fallas.append("leyenda vacía")
    if not any(t.get("data-rotulo") for t in textos):
        fallas.append("rótulo vacío")

    # palabras prohibidas en todo el texto visible y en el .def.json
    todo = " ".join(t.text or "" for t in textos) + " " + DEF.read_text(encoding="utf-8")
    for pat in PROHIBIDAS:
        m = re.search(pat, todo, re.I)
        if m:
            fallas.append(f"palabra prohibida: {m.group(0)!r}")

    # cajas de parte: texto adentro
    cajas = {el.get("data-part"): (float(el.get("x")), float(el.get("y")),
                                   float(el.get("x")) + float(el.get("width")),
                                   float(el.get("y")) + float(el.get("height")))
             for el in raiz.iter(NS + "rect") if el.get("data-part")}
    for t in textos:
        n = t.get("data-en")
        if n:
            a, b = caja_texto(t), cajas[n]
            if a[0] < b[0] + 0.3 or a[2] > b[2] - 0.3 or a[1] < b[1] + 0.2 or a[3] > b[3] - 0.2:
                fallas.append(f"texto fuera de la caja {n}: {t.text!r}")

    # solapes texto-texto
    bb = [(caja_texto(t), t) for t in textos]
    for i in range(len(bb)):
        for j in range(i + 1, len(bb)):
            a, b = bb[i][0], bb[j][0]
            if a[0] < b[2] - 0.05 and b[0] < a[2] - 0.05 and a[1] < b[3] - 0.05 and b[1] < a[3] - 0.05:
                fallas.append(f"textos solapados: {bb[i][1].text!r} / {bb[j][1].text!r}")

    # solapes texto-línea
    lineas = []
    for el in raiz.iter():
        if el.tag == NS + "polyline":
            p = [tuple(map(float, q.split(","))) for q in el.get("points").split()]
            lineas += [((p[k], p[k + 1]), el.get("data-con")) for k in range(len(p) - 1)]
        elif el.tag == NS + "line":
            lineas.append((((float(el.get("x1")), float(el.get("y1"))),
                            (float(el.get("x2")), float(el.get("y2")))), None))
    for r, t in bb:
        for seg, con in lineas:
            if con and con == t.get("data-con"):
                continue
            if cruza(seg, r):
                fallas.append(f"línea {con or '-'} cruza el texto {t.text!r}")
                break

    # cada conexión dibujada y con insignia
    dib = {el.get("data-con") for el in raiz.iter(NS + "polyline")}
    ins = {t.get("data-con") for t in textos if t.get("data-con")}
    for c in d["conexiones"]:
        if c["id"] not in dib or c["id"] not in ins:
            fallas.append(f"conexión {c['id']} sin dibujar o sin insignia")

    # JPG
    if not JPG.exists():
        fallas.append("falta el JPG")
    else:
        from PIL import Image
        im = Image.open(JPG)
        dpi = tuple(round(v) for v in im.info.get("dpi", (0, 0)))
        if im.size != (4961, 3508) or dpi != (300, 300) or im.format != "JPEG":
            fallas.append(f"JPG {im.format} {im.size} {dpi}: no es A3 a 300 dpi")

    print(f"LD-01 · textos {len(textos)} · partes {len(nums)} · conexiones {len(dib - {None})} · "
          f"familias {sorted(familias)}")
    for f in fallas:
        print("FALLA:", f)
    print("VERDE" if not fallas else f"ROJO ({len(fallas)} fallas)")
    return 1 if fallas else 0


if __name__ == "__main__":
    sys.exit(main())
