"""Resume un SBOM (CycloneDX o SPDX, envuelto o no en una atestación in-toto)."""
import collections, json, sys

INTERES = ("Newtonsoft.Json", "is-odd", "is-number", "packaging")
for ruta in sys.argv[1:]:
    doc = json.load(open(ruta))
    doc = doc.get("predicate", doc)
    purls = []
    if "components" in doc:                      # CycloneDX (recorre componentes anidados)
        def recorrer(cs):
            for c in cs:
                yield c.get("purl", "")
                yield from recorrer(c.get("components", []))
        purls = list(recorrer(doc.get("components", [])))
        fmt = f"CycloneDX {doc.get('specVersion')}"
    else:                                        # SPDX
        for p in doc.get("packages", []):
            purls += [r["referenceLocator"] for r in p.get("externalRefs", []) if r.get("referenceType") == "purl"]
        fmt = doc.get("spdxVersion", "SPDX")
    tipos = collections.Counter(p.split("/")[0] if p else "(sin purl)" for p in purls)
    deps = sorted({p.split("?")[0] for p in purls if any(n in p for n in INTERES)})
    print(f"{ruta.split('/')[-1]:<22} {fmt:<15} {len(purls):>4} purl  {dict(tipos)}")
    for d in deps:
        print(f"{'':<24}{d}")
