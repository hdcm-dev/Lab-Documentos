"""Ayudante de E6-5.

  buscar <provenance.json> <texto>   -> rutas JSON donde aparece <texto> (p. ej. el SHA del commit)
  editar <provenance.json> <id>      -> reemplaza runDetails.builder.id por <id> (simula una falsificación)
"""
import json
import sys


def buscar(o, ruta=""):
    if isinstance(o, dict):
        for k, v in o.items():
            yield from buscar(v, f"{ruta}.{k}")
    elif isinstance(o, list):
        for i, v in enumerate(o):
            yield from buscar(v, f"{ruta}[{i}]")
    elif isinstance(o, str) and texto in o:
        yield ruta, o


modo, archivo = sys.argv[1], sys.argv[2]
p = json.load(open(archivo))
if modo == "buscar":
    texto = sys.argv[3]
    hallazgos = list(buscar(p))
    print(f"   ¿menciona el commit {texto[:12]}? {'sí, en:' if hallazgos else 'NO'}")
    for r, v in hallazgos[:4]:
        print(f"      {r} = {v[:100]}")
elif modo == "editar":
    antes = p["predicate"]["runDetails"]["builder"].get("id")
    p["predicate"]["runDetails"]["builder"]["id"] = sys.argv[3]
    json.dump(p, open(archivo, "w"))
    print(f"   builder.id original: {antes!r}")
    print(f"   builder.id editado : {sys.argv[3]!r} -> sigue siendo JSON válido; nada en el archivo delata el cambio")
