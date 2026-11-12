#!/usr/bin/env python3
"""Generador determinista de la lámina LD-01.

Lee LD-01-arquitectura-conversacional.def.json y escribe el SVG maestro
(A3 apaisada, 420 x 297 mm). Con --jpg exporta además el JPG calidad 95
a 300 dpi (4961 x 3508 px) con cairosvg + Pillow.

Ningún texto de la lámina está escrito acá: todo sale del .def.json.
Este archivo sólo fija la geometría y el estilo.

Uso (dentro del contenedor efímero, ver LEEME.md):
    python3 Generadores/ld01.py [--jpg]
"""
import json
import sys
from pathlib import Path
from xml.sax.saxutils import escape

AQUI = Path(__file__).resolve().parent
LAMINAS = AQUI.parent
DEF = LAMINAS / "LD-01-arquitectura-conversacional.def.json"
SVG = LAMINAS / "LD-01-arquitectura-conversacional.svg"
JPG = LAMINAS / "LD-01-arquitectura-conversacional.jpg"

FAMILIA = "DejaVu Sans"
FUENTES = {
    False: "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
    True: "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
}

# ---------------------------------------------------------------- medidas
_cache = {}


def ancho(texto, tam, negrita=False):
    """Ancho en mm de un texto de tamaño `tam` mm (métrica real de la fuente)."""
    try:
        from PIL import ImageFont
        if negrita not in _cache:
            _cache[negrita] = ImageFont.truetype(FUENTES[negrita], 100)
        return _cache[negrita].getlength(texto) * tam / 100.0
    except Exception:  # sin Pillow o sin fuente: estimación conservadora
        return len(texto) * tam * (0.66 if negrita else 0.6)


def partir(texto, tam, maximo, negrita=False):
    palabras, lineas, actual = texto.split(), [], ""
    for p in palabras:
        prueba = (actual + " " + p).strip()
        if ancho(prueba, tam, negrita) <= maximo or not actual:
            actual = prueba
        else:
            lineas.append(actual)
            actual = p
    if actual:
        lineas.append(actual)
    return lineas


# ---------------------------------------------------------------- estilo
GRISES = ["#f6f6f4", "#efefec", "#e9e9e5", "#f3f3f0", "#ebebe7", "#f1f1ee", "#e6e6e2", "#eeeeea"]
TINTA = "#2b2b2b"
TRAZOS = {
    "continuo": ("#2b2b2b", None),
    "trazos": ("#236c9c", "2.4 1.2"),
    "punteado": ("#8a6418", "0.5 0.9"),
}

# ---------------------------------------------------------------- geometría
PW, PH = 420.0, 297.0
FRANJA_X0, FRANJA_X1 = 8.0, 30.0
COL_X0, COL_W, COL_G, NCOL = 35.0, 32.0, 7.0, 6
BANDA_Y0, BANDA_H, CANAL_H = 15.0, 18.5, 17.6
DER_X0, DER_X1 = 270.0, 412.0
LADO_SLOTS = [4.0 + 6.0 * i for i in range(5)]
SPINE_X = 32.5  # canal reservado para la seguridad transversal


def col_x(c):
    return COL_X0 + c * (COL_W + COL_G)


def banda_y(b):
    return BANDA_Y0 + b * (BANDA_H + CANAL_H)


def canal_y(k):  # canal entre la banda k y la k+1
    return banda_y(k) + BANDA_H


class Svg:
    def __init__(self):
        self.out = []

    def add(self, s):
        self.out.append(s)

    def texto(self, x, y, s, tam, negrita=False, ancla="start", color=TINTA, extra=""):
        w = " font-weight=\"bold\"" if negrita else ""
        self.add(f'<text x="{x:.2f}" y="{y:.2f}" font-size="{tam}"{w} text-anchor="{ancla}" '
                 f'fill="{color}" {extra}>{escape(s)}</text>')


def generar():
    d = json.loads(DEF.read_text(encoding="utf-8"))
    partes = {p["n"]: p for p in d["partes"]}
    zonas = d["zonas"]
    banda_de, col_de = {}, {}
    for b, z in enumerate(zonas):
        for c, n in enumerate(z["partes"]):
            banda_de[n], col_de[n] = b, c
    caja = {n: (col_x(col_de[n]), banda_y(banda_de[n]), COL_W, BANDA_H) for n in partes}

    s = Svg()
    s.add(f'<svg xmlns="http://www.w3.org/2000/svg" width="{PW:g}mm" height="{PH:g}mm" '
          f'viewBox="0 0 {PW:g} {PH:g}" font-family="{FAMILIA}">')
    s.add('<defs>')
    for nombre, (color, _) in TRAZOS.items():
        s.add(f'<marker id="f-{nombre}" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="2.6" '
              f'markerHeight="2.6" markerUnits="userSpaceOnUse" orient="auto">'
              f'<path d="M0,0 L10,5 L0,10 z" fill="{color}"/></marker>')
    s.add('</defs>')
    s.add(f'<rect x="0" y="0" width="{PW:g}" height="{PH:g}" fill="#ffffff"/>')
    s.add(f'<rect x="4" y="4" width="{PW-8:g}" height="{PH-8:g}" fill="none" stroke="{TINTA}" stroke-width="0.5"/>')

    # Encabezado
    s.texto(FRANJA_X0, 11.0, f'{d["lamina"]} · {d["titulo"]}', 4.6, True, extra='data-rol="titulo"')

    # Zonas (un gris por zona) y su nombre en la franja izquierda
    fin_x = col_x(NCOL - 1) + COL_W + 3.0
    for b, z in enumerate(zonas):
        y = banda_y(b)
        s.add(f'<rect x="{FRANJA_X0}" y="{y-1.2:.2f}" width="{fin_x-FRANJA_X0:.2f}" height="{BANDA_H+2.4:.2f}" '
              f'fill="{GRISES[b % len(GRISES)]}" stroke="#c9c9c4" stroke-width="0.25" data-zona="{z["id"]}"/>')
        lineas = partir(z["nombre"], 2.6, FRANJA_X1 - FRANJA_X0 - 3, True)
        y0 = y + BANDA_H / 2 - (len(lineas) - 1) * 1.6 + 0.9
        for i, ln in enumerate(lineas):
            s.texto(FRANJA_X0 + 1.5, y0 + i * 3.2, ln, 2.6, True, extra=f'data-zona-titulo="{z["id"]}"')

    # Cajas de parte
    for n, p in sorted(partes.items()):
        x, y, w, h = caja[n]
        s.add(f'<rect x="{x:.2f}" y="{y:.2f}" width="{w}" height="{h}" rx="1" fill="#ffffff" '
              f'stroke="{TINTA}" stroke-width="0.35" data-part="{n}"/>')
        s.add(f'<circle cx="{x+3.1:.2f}" cy="{y+3.0:.2f}" r="2.2" fill="{TINTA}"/>')
        s.texto(x + 3.1, y + 3.8, str(n), 2.2, True, "middle", "#ffffff", f'data-num="{n}" data-en="{n}"')
        tn = 2.3
        while max(ancho(pal, tn, True) for pal in p["nombre"].split()) > w - 3.4:
            tn = round(tn - 0.05, 2)  # achica sólo si una palabra no entra
        nl = partir(p["nombre"], tn, w - 8.0, True)
        if len(nl) > 1:  # desde la segunda línea, todo el ancho bajo el círculo
            nl = nl[:1] + partir(" ".join(nl[1:]), tn, w - 3.4, True)
        for i, ln in enumerate(nl):
            s.texto(x + (6.6 if i == 0 else 1.6), y + 4.1 + i * 2.8, ln, tn, True, extra=f'data-en="{n}"')
        if p.get("nota"):
            yn = y + 4.1 + max(len(nl), 1) * 2.8 + 0.4
            for i, ln in enumerate(partir(p["nota"], 1.95, w - 3.0)):
                s.texto(x + 1.6, yn + i * 2.4, ln, 1.95, color="#4a4a4a", extra=f'data-en="{n}"')

    # ------------------------------------------------ enrutado de conexiones
    usados = {}  # (parte, lado) -> set(slot)

    def puerto(n, lado, hacia_x):
        x, y, w, h = caja[n]
        libres = [i for i in range(len(LADO_SLOTS)) if i not in usados.setdefault((n, lado), set())]
        i = min(libres, key=lambda i: (abs(x + LADO_SLOTS[i] - hacia_x), i))
        usados[(n, lado)].add(i)
        return x + LADO_SLOTS[i], (y + h if lado == "abajo" else y)

    guttersx = [col_x(c) + COL_W + COL_G * f for c in range(NCOL) for f in (0.25, 0.5, 0.75)]
    gutter_usado = set()

    def gutter(cerca):
        libres = [g for g in guttersx if g not in gutter_usado]
        g = min(libres, key=lambda g: (abs(g - cerca), g))
        gutter_usado.add(g)
        return g

    pedidos_carril = {}  # canal -> [id]

    def carril(canal, cid):
        pedidos_carril.setdefault(canal, []).append(cid)
        return ("carril", canal, cid)

    rutas = []
    for c in d["conexiones"]:
        cid, a, t = c["id"], c["desde"], c["hasta"]
        if t == "todas":
            ba = banda_de[a]
            px, py = puerto(a, "arriba", SPINE_X)
            puntos = [(px, py), carril(ba - 1, cid), ("x", SPINE_X), ("y", banda_y(0) + BANDA_H / 2)]
            rutas.append((c, puntos, (px, py), "arriba", True))
            continue
        ba, bt = banda_de[a], banda_de[t]
        cxa = caja[a][0] + COL_W / 2
        cxt = caja[t][0] + COL_W / 2
        if bt == ba:
            lado = "abajo" if ba < len(zonas) - 1 else "arriba"
            canal = ba if lado == "abajo" else ba - 1
            pa = puerto(a, lado, cxt)
            pt = puerto(t, lado, cxa)
            puntos = [pa, carril(canal, cid), ("x", pt[0]), pt]
        else:
            baja = bt > ba
            la, lt = ("abajo", "arriba") if baja else ("arriba", "abajo")
            pa = puerto(a, la, cxt)
            pt = puerto(t, lt, cxa)
            ca = ba if baja else ba - 1
            ct = bt - 1 if baja else bt
            if ca == ct:
                puntos = [pa, carril(ca, cid), ("x", pt[0]), pt]
            else:
                g = gutter((pa[0] + pt[0]) / 2)
                puntos = [pa, carril(ca, cid), ("x", g), carril(ct, cid + "b"), ("x", pt[0]), pt]
            lado = la
        rutas.append((c, puntos, pa, lado, False))

    ycarril = {}
    for canal, ids in pedidos_carril.items():
        y0, y1 = canal_y(canal) + 4.0, canal_y(canal) + CANAL_H - 4.0
        k = len(ids)
        for i, cid in enumerate(ids):
            ycarril[(canal, cid)] = y0 + (y1 - y0) * (i + 0.5) / k

    for c, puntos, pa, lado, todas in rutas:
        pts, cur = [], None
        for p in puntos:
            if isinstance(p, tuple) and p and p[0] == "carril":
                y = ycarril[(p[1], p[2])]
                cur = (cur[0], y)
            elif isinstance(p, tuple) and p and p[0] == "x":
                cur = (p[1], cur[1])
            elif isinstance(p, tuple) and p and p[0] == "y":
                cur = (cur[0], p[1])
            else:
                cur = p
            pts.append(cur)
        color, dash = TRAZOS[c["trazo"]]
        da = f' stroke-dasharray="{dash}"' if dash else ""
        marca = "" if todas else f' marker-end="url(#f-{c["trazo"]})"'
        s.add(f'<polyline points="{" ".join(f"{x:.2f},{y:.2f}" for x, y in pts)}" fill="none" '
              f'stroke="{color}" stroke-width="0.35"{da}{marca} data-con="{c["id"]}"/>')
        if todas:
            for b in range(len(zonas)):
                yb = banda_y(b) + BANDA_H / 2
                s.add(f'<polyline points="{SPINE_X:.2f},{yb:.2f} {FRANJA_X1+0.2:.2f},{yb:.2f}" fill="none" '
                      f'stroke="{color}" stroke-width="0.35"{da} marker-end="url(#f-{c["trazo"]})" data-con="{c["id"]}"/>')
        # insignia con el id, sobre el tramo de salida
        bx = pa[0]
        by = pa[1] + 2.3 if lado == "abajo" else pa[1] - 2.3
        wb = ancho(c["id"], 1.9, True) + 1.4
        s.add(f'<rect x="{bx-wb/2:.2f}" y="{by-1.35:.2f}" width="{wb:.2f}" height="2.7" rx="0.6" '
              f'fill="#ffffff" stroke="{color}" stroke-width="0.25"/>')
        s.texto(bx, by + 0.68, c["id"], 1.9, True, "middle", color, f'data-con="{c["id"]}"')

    # ------------------------------------------------ columna derecha
    y = 16.0
    tp = d["tabla_preguntas"]
    s.texto(DER_X0, y, tp["titulo"], 3.4, True, extra='data-rol="tabla"')
    y += 4.2
    cols = [DER_X0, DER_X0 + 6.5, DER_X0 + 44.0, DER_X0 + 126.0]
    for x, nombre in zip(cols, tp["columnas"]):
        s.texto(x, y, nombre, 2.1, True, color="#4a4a4a")
    s.add(f'<line x1="{DER_X0}" y1="{y+0.9:.2f}" x2="{DER_X1}" y2="{y+0.9:.2f}" stroke="{TINTA}" stroke-width="0.3"/>')
    y += 3.6
    for f in tp["filas"]:
        pl = partir(f["pregunta"], 2.15, cols[2] - cols[1] - 2.0, True)
        rl = partir(f["respuesta"], 2.0, cols[3] - cols[2] - 2.0)
        pal = partir(" ".join(str(n) for n in f["partes"]), 2.0, DER_X1 - cols[3])
        filas = max(len(pl), len(rl), len(pal))
        s.texto(cols[0], y, f["q"], 2.15, True)
        for i, ln in enumerate(pl):
            s.texto(cols[1], y + i * 2.6, ln, 2.15, True)
        for i, ln in enumerate(rl):
            s.texto(cols[2], y + i * 2.6, ln, 2.0)
        for i, ln in enumerate(pal):
            s.texto(cols[3], y + i * 2.6, ln, 2.0, color="#4a4a4a")
        y += filas * 2.6 + 0.6
        s.add(f'<line x1="{DER_X0}" y1="{y-1.6:.2f}" x2="{DER_X1}" y2="{y-1.6:.2f}" stroke="#c9c9c4" stroke-width="0.2"/>')
        y += 0.9

    # Conexiones (rótulos), en dos sub-columnas
    y += 2.6
    s.texto(DER_X0, y, "Conexiones", 3.0, True, extra='data-rol="conexiones"')
    y += 3.8
    con = d["conexiones"]
    mitad = (len(con) + 1) // 2
    subw = (DER_X1 - DER_X0) / 2
    y_con = y
    for i, c in enumerate(con):
        x = DER_X0 + (0 if i < mitad else subw)
        yy = y_con + (i % mitad) * 2.55
        color = TRAZOS[c["trazo"]][0]
        s.texto(x, yy, c["id"], 1.9, True, color=color)
        txt = f'{c["desde"]}→{c["hasta"]} {c["rotulo"]}'
        s.texto(x + 5.2, yy, txt, 1.9)
    y = y_con + mitad * 2.55 + 2.0

    # Leyenda
    s.texto(DER_X0, y, "Leyenda", 3.0, True, extra='data-rol="leyenda"')
    y += 3.6
    muestras = {2: ["continuo", "trazos", "punteado"]}
    for i, item in enumerate(d["leyenda"]):
        lineas = partir(item, 2.0, DER_X1 - DER_X0 - 2.0)
        for j, ln in enumerate(lineas):
            s.texto(DER_X0, y + j * 2.5, ln, 2.0, extra='data-leyenda="1"')
        y += len(lineas) * 2.5 + 0.8
    for nombre in muestras[2]:
        color, dash = TRAZOS[nombre]
        da = f' stroke-dasharray="{dash}"' if dash else ""
        s.add(f'<polyline points="{DER_X0:.2f},{y-0.7:.2f} {DER_X0+12:.2f},{y-0.7:.2f}" fill="none" stroke="{color}" '
              f'stroke-width="0.35"{da} marker-end="url(#f-{nombre})" data-con="L-{nombre}"/>')
        s.texto(DER_X0 + 14, y, nombre, 2.0, extra='data-leyenda="1"')
        y += 2.8
    y += 1.0

    # Rótulo
    r = d["rotulo"]
    filas = [("Dispositivo", r["dispositivo"]), ("Lámina", f'{r["lamina"]} · v{r["version"]} · {r["fecha"]}'),
             ("Fuente", r["fuente"]), ("Conocimiento", r["conocimiento"])]
    lineas = [(k, partir(v, 1.9, DER_X1 - DER_X0 - 22.0)) for k, v in filas]
    alto = sum(len(v) for _, v in lineas) * 2.4 + len(lineas) * 0.9 + 2.4
    y0 = max(y, PH - 8.0 - alto)
    s.add(f'<rect x="{DER_X0-1:.2f}" y="{y0:.2f}" width="{DER_X1-DER_X0+2:.2f}" height="{alto:.2f}" fill="#ffffff" '
          f'stroke="{TINTA}" stroke-width="0.4" data-rol="rotulo"/>')
    yy = y0 + 2.9
    for k, v in lineas:
        s.texto(DER_X0 + 0.6, yy, k, 1.9, True, extra='data-rotulo="1"')
        for j, ln in enumerate(v):
            s.texto(DER_X0 + 21.0, yy + j * 2.4, ln, 1.9, extra='data-rotulo="1"')
        yy += len(v) * 2.4 + 0.9

    s.add('</svg>')
    SVG.write_text("\n".join(s.out) + "\n", encoding="utf-8")
    print(f"SVG: {SVG}")


def exportar_jpg():
    import io
    import cairosvg
    from PIL import Image
    png = cairosvg.svg2png(url=str(SVG), output_width=4961, output_height=3508, background_color="white")
    im = Image.open(io.BytesIO(png)).convert("RGB")
    im.save(JPG, "JPEG", quality=95, dpi=(300, 300), optimize=False)
    print(f"JPG: {JPG} {im.size}")


if __name__ == "__main__":
    generar()
    if "--jpg" in sys.argv:
        exportar_jpg()
