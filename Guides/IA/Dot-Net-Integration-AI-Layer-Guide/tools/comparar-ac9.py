# comparar-ac9.py — compuerta AC-9 (plan §F.10): los bloques de código de la guía coinciden con sus archivos,
# y las cifras de laboratorio de la prosa tienen respaldo (ESTA-07: repone el comparador que se borró en la
# limpieza de la construcción).
#
#   python3 -I tools/comparar-ac9.py bloques [<guía.md>]
#   python3 -I tools/comparar-ac9.py cifras <guía.md> <fuente>[#<encabezado>]...
#
# bloques: un bloque cercado se compara con un archivo cuando la línea no vacía que lo precede es sólo una
#   ruta entre comillas invertidas, de Examples/ o Demos/, a un archivo («`Examples/…/Archivo.cs`»). El bloque
#   se parte en tramos por las líneas de elisión («// …», «# …», «<!-- … -->», «…»); cada tramo tiene que
#   aparecer entero y seguido en el archivo, y los tramos en orden. Se compara línea a línea sin la sangría
#   ni los espacios finales, y sin las líneas en blanco (la guía puede quitar sangría y separar tramos).
#   Una ruta citada dentro de un párrafo no rotula el bloque que sigue. Sale con 1 si algún bloque difiere o
#   su archivo no existe.
# cifras: lista las cifras de medición de la prosa (fuera de bloques, de código en línea, de [REF-nn], de §,
#   de versiones y de fechas): ms y s, miles, decimales, k/M, «k de M», %, IC95 [a; b], p = x y hit@k.
#   Descarta las que figuran literalmente (en forma normalizada, sin espacios) en las fuentes dadas; una
#   fuente con «#<encabezado>» se lee sólo desde ese encabezado hasta el siguiente del mismo nivel. Imprime
#   «forma · apariciones · líneas» de las que quedan, para clasificarlas a mano. Sale con 0 (informa).
import os, re, sys, collections

ELISION = re.compile(r'^\s*(//|#|--)?\s*(…|\.\.\.)\s*$|^\s*<!--\s*…\s*-->\s*$')
ROTULO = re.compile(r'^`((?:Examples|Demos)/[^`\s]+\.[A-Za-z0-9]+)`\s*$')
CERCO = re.compile(r'^\s*(`{3,}|~{3,})')

def bloques_de(lineas):
    """(línea del cerco, rótulo o None, [líneas del bloque])"""
    previo, i, n = '', 0, len(lineas)
    while i < n:
        m = CERCO.match(lineas[i])
        if not m:
            if lineas[i].strip(): previo = lineas[i].strip()
            i += 1; continue
        cerco, ini, cuerpo = m.group(1), i + 1, []
        i += 1
        while i < n and not lineas[i].strip().startswith(cerco):
            cuerpo.append(lineas[i]); i += 1
        r = ROTULO.match(previo)
        yield ini, (r.group(1) if r else None), cuerpo
        previo = ''; i += 1

def tramos(cuerpo):
    actual, todos = [], []
    for l in cuerpo:
        if ELISION.match(l):
            if actual: todos.append(actual)
            actual = []
        elif l.strip():
            actual.append(l.strip())
    if actual: todos.append(actual)
    return todos

def contiene_en_orden(archivo, partes):
    pos = 0
    for t in partes:
        k = len(t); hallado = -1
        for j in range(pos, len(archivo) - k + 1):
            if archivo[j:j + k] == t: hallado = j; break
        if hallado < 0: return False, t[0]
        pos = hallado + k
    return True, None

def cmd_bloques(guia):
    raiz = os.path.dirname(os.path.abspath(guia))
    lineas = open(guia, encoding='utf-8').read().split('\n')
    total = difieren = 0
    for ini, rotulo, cuerpo in bloques_de(lineas):
        if not rotulo: continue
        total += 1
        ruta = os.path.join(raiz, rotulo)
        if not os.path.isfile(ruta):
            difieren += 1; print(f'L{ini}\t{rotulo}\tarchivo inexistente'); continue
        archivo = [l.strip() for l in open(ruta, encoding='utf-8').read().split('\n') if l.strip()]
        ok, primera = contiene_en_orden(archivo, tramos(cuerpo))
        if not ok:
            difieren += 1; print(f'L{ini}\t{rotulo}\tun tramo no coincide (empieza con: {primera[:60]})')
    print(f'bloques {total} con diferencias {difieren}')
    return 1 if difieren else 0

FIGURAS = [
    r'IC95\s*\[[0-9.,]+;\s*[0-9.,]+\]',
    r'\bp\s*=\s*[0-9]+,[0-9]+',
    r'\bhit@[0-9]+\s*=\s*[0-9/,.]+',
    r'\b[0-9]+ de [0-9]+\b',
    r'\b[0-9]+/[0-9]+\b',
    r'\b[0-9]{1,3}(?:\.[0-9]{3})+(?:,[0-9]+)?(?:\s?(?:ms|s)\b)?',
    r'\b[0-9]+,[0-9]+(?:\s?(?:ms|s)\b)?',
    r'\b[0-9]+\s?%',
    r'\b[0-9]+\s?(?:ms|s)\b',
]
FIG = re.compile('|'.join(f'(?:{f})' for f in FIGURAS))
QUITAR = [re.compile(x) for x in (r'`[^`]*`', r'\[REF-[0-9]+\]', r'§[0-9A-Z.]+', r'\b[0-9]{4}-[0-9]{2}(?:-[0-9]{2})?\b',
                                  r'\bv?[0-9]+\.[0-9]+\.[0-9]+(?:[-.][0-9A-Za-z.]+)?\b', r'\(#[^)]*\)', r'<[^>]+>')]

def normal(x): return re.sub(r'\s+', '', x)

def texto_de_fuente(arg):
    ruta, _, enc = arg.partition('#')
    lineas = open(ruta, encoding='utf-8').read().split('\n')
    if not enc: return '\n'.join(lineas)
    dentro, nivel, salida = False, 0, []
    for l in lineas:
        m = re.match(r'^(#{1,6})\s+(.*)$', l)
        if m and dentro and len(m.group(1)) <= nivel: break
        if m and not dentro and m.group(2).strip().startswith(enc): dentro, nivel = True, len(m.group(1))
        if dentro: salida.append(l)
    if not salida: sys.exit(f'cifras: no encontré «{enc}» en {ruta}')
    return '\n'.join(salida)

def cmd_cifras(guia, fuentes):
    respaldo = set()
    for f in fuentes:
        t = texto_de_fuente(f)
        for q in QUITAR[:2]: t = q.sub(' ', t)
        respaldo.update(normal(m.group(0)) for m in FIG.finditer(t))
    lineas = open(guia, encoding='utf-8').read().split('\n')
    en_cerco, cuenta, donde = None, collections.Counter(), collections.defaultdict(list)
    for i, l in enumerate(lineas, 1):
        m = CERCO.match(l)
        if m:
            en_cerco = None if en_cerco else m.group(1)[:3]; continue
        if en_cerco: continue
        t = l
        for q in QUITAR: t = q.sub(' ', t)
        for m in FIG.finditer(t):
            x = normal(m.group(0))
            if x in respaldo: continue
            cuenta[x] += 1; donde[x].append(f'L{i}')
    for x, c in sorted(cuenta.items(), key=lambda kv: (-kv[1], int(donde[kv[0]][0][1:]))):
        print(f'{x}\t{c}\t{",".join(dict.fromkeys(donde[x]))}')
    print(f'# cifras sin respaldo literal en las fuentes: {len(cuenta)} formas, {sum(cuenta.values())} apariciones')
    return 0

if __name__ == '__main__':
    aqui = os.path.dirname(os.path.abspath(__file__))
    por_defecto = os.path.join(aqui, '..', 'Dot-Net-Integration-AI-Layer-Guide.md')
    if len(sys.argv) < 2 or sys.argv[1] not in ('bloques', 'cifras'):
        sys.exit('uso: comparar-ac9.py bloques [<guía.md>] | cifras <guía.md> <fuente>[#<encabezado>]...')
    if sys.argv[1] == 'bloques':
        sys.exit(cmd_bloques(sys.argv[2] if len(sys.argv) > 2 else por_defecto))
    sys.exit(cmd_cifras(sys.argv[2], sys.argv[3:]))
