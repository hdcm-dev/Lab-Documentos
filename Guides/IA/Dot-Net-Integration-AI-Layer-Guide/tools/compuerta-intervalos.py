# compuerta-intervalos.py — cada tasa de la guía, recalculada (REC-19; §10.4, A.10, E.6).
#
#   python3 -I tools/compuerta-intervalos.py <guía.md> <carpeta de registros R-LLM> [<mapa.tsv>]
#
# Reconoce en la prosa de la guía cada «IC95 [a; b]», cada «k/M» y cada «k de M», fuera del Anexo D (DOI,
# arXiv) y de las fechas. En los bloques de código sólo reconoce la salida de «verificar.sh tasa»
# («k/M = … IC95 Clopper-Pearson [a; b]»): las demás salidas de los bloques son del cliente guionado.
# Cada figura reconocida tiene que estar en el mapa (por defecto tools/compuerta-intervalos.tsv): una fila
# dice en qué línea está (por un texto único de esa línea), cuánto valen k y M, de qué clase es y, si es una
# medición, de qué registro sale y cómo se cuenta. La compuerta:
#   1. comprueba que el IC95 escrito es el de Clopper-Pearson al 95 % de k/M (tres decimales; el mismo
#      cálculo que «tools/verificar.sh tasa»);
#   2. si la fila nombra un registro, cuenta M (las líneas que pasan el filtro) y k (las que además pasan la
#      condición de éxito) y los compara con los de la guía; con «-» en la condición, compara sólo M;
#   3. falla si una figura de la guía no está en el mapa, si una fila del mapa no encuentra su figura, o si
#      algo no coincide.
# Clases: medicion (con registro), medicion-parcial (el registro no separa la métrica: sólo M, o sólo el
# intervalo), umbral, calculo, criterio, estimulo (definiciones y datos, no mediciones: sólo el intervalo,
# si lo hay).
# Filtro y condición: «campo=valor», «campo!=valor», «campo~regex» o «campo!~regex», unidos por «;» (todas
# se cumplen). Sale con 0 si todo coincide, 1 si algo no, 2 si falta un archivo.
# Procedencia: adaptado de una versión anterior que usaba Wilson (bitácora de la guía); acá el intervalo
# es el de Clopper-Pearson y la unidad, la entrada distinta, como en el resto de la guía.
import json, math, os, re, sys

def lfact(x): return math.lgamma(x + 1)
def cdf(x, n, p):
    if p <= 0: return 1.0
    if p >= 1: return 1.0 if x >= n else 0.0
    return sum(math.exp(lfact(n) - lfact(i) - lfact(n - i) + i * math.log(p) + (n - i) * math.log(1 - p)) for i in range(0, x + 1))
def biseca(f):
    lo, hi = 0.0, 1.0
    for _ in range(200):
        m = (lo + hi) / 2
        if f(m): lo = m
        else: hi = m
    return (lo + hi) / 2
def clopper_pearson(k, n, a=0.05):
    inf = 0.0 if k == 0 else biseca(lambda p: 1 - cdf(k - 1, n, p) < a / 2)
    sup = 1.0 if k == n else biseca(lambda p: cdf(k, n, p) > a / 2)
    return inf, sup

def num(x): return float(x.replace('.', '').replace(',', '.')) if ',' in x else float(x)

IC = re.compile(r'IC95(?: Clopper-Pearson)?\s*\[([0-9.,]+);\s*([0-9.,]+)\]')
KM = re.compile(r'(?<![0-9/])(?<![0-9][.,])([0-9]+)(/| de )([0-9]+)(?![0-9/])(?![.,][0-9])')
FECHA = re.compile(r'[0-9]{4}-[0-9]{2}-[0-9]{2}/[0-9]{2}|[0-9]{4}/[0-9]{2}|[0-9]+/[0-9]{4}')

def figuras(lineas):
    """(n.º de línea, forma, texto de la línea) de cada figura reconocida."""
    anexo_d, cerco = False, None
    for i, l in enumerate(lineas, 1):
        m = re.match(r'^\s*(`{3,}|~{3,})', l)
        if m:
            cerco = None if cerco else m.group(1)[:3]; continue
        # En los bloques de código sólo cuenta la salida de «verificar.sh tasa»; lo demás es salida guionada.
        if cerco and 'IC95 Clopper-Pearson' not in l: continue
        if re.match(r'^## Anexo D\b', l): anexo_d = True
        elif re.match(r'^## Anexo [E-Z]\b', l): anexo_d = False
        if anexo_d: continue
        t = FECHA.sub(' ', re.sub(r'`[^`]*`', lambda m: ' ' if 'IC95' not in m.group(0) else m.group(0), l))
        t = re.sub(r'\]\([^)]*\)', ']', t)          # destinos de enlaces
        for m in IC.finditer(t): yield i, 'IC95 [%s; %s]' % (m.group(1), m.group(2)), l
        for m in KM.finditer(t): yield i, m.group(0).strip(), l

def condicion(texto):
    conds = []
    for c in filter(None, texto.split(';')):
        m = re.match(r'^([A-Za-z_]+)(!~|~|!=|=)(.*)$', c)
        if not m: sys.exit(f'condición ilegible: {c}')
        conds.append(m.groups())
    def cumple(fila):
        for campo, op, valor in conds:
            v = fila.get(campo); v = '' if v is None else str(v)
            if op == '=' and v != valor: return False
            if op == '!=' and v == valor: return False
            if op == '~' and not re.search(valor, v): return False
            if op == '!~' and re.search(valor, v): return False
        return True
    return cumple

def main():
    if len(sys.argv) < 3: sys.exit('uso: compuerta-intervalos.py <guía.md> <registros> [<mapa.tsv>]')
    guia, registros = sys.argv[1], sys.argv[2]
    mapa = sys.argv[3] if len(sys.argv) > 3 else os.path.join(os.path.dirname(os.path.abspath(__file__)), 'compuerta-intervalos.tsv')
    for r in (guia, mapa):
        if not os.path.isfile(r): print(f'no existe: {r}', file=sys.stderr); sys.exit(2)
    lineas = open(guia, encoding='utf-8').read().split('\n')
    filas = []
    for t in open(mapa, encoding='utf-8'):
        if not t.strip() or t.startswith('#'): continue
        c = t.rstrip('\n').split('\t')
        if len(c) != 10: sys.exit(f'fila del mapa con {len(c)} columnas (se esperan 10): {c[0]}')
        filas.append(dict(zip(('id', 'ancla', 'forma', 'ic', 'k', 'm', 'clase', 'registro', 'filtro', 'exito'), c)))
    rc, usadas, cubiertas = 0, set(), 0
    reconocidas = list(figuras(lineas))
    for i, forma, l in reconocidas:
        f = next((f for f in filas if f['ancla'] in l and forma in (f['forma'], f['ic'])), None)
        if not f:
            print(f'L{i}\t{forma}\tno está en el mapa'); rc = 1; continue
        usadas.add(f['id']); cubiertas += 1
    resumen = {}
    for f in filas:
        lin = [i for i, l in enumerate(lineas, 1) if f['ancla'] in l]
        if len(lin) != 1:
            print(f"{f['id']}\tel ancla aparece en {len(lin)} líneas (tiene que ser una)"); rc = 1; continue
        if f['id'] not in usadas:
            print(f"{f['id']}\tL{lin[0]}\tla figura del mapa no está en la guía"); rc = 1; continue
        k, m = int(f['k']), int(f['m']); estado = []
        if f['ic'] != '-':
            a, b = IC.search(f['ic']).groups(); inf, sup = clopper_pearson(k, m)
            if abs(num(a) - round(inf, 3)) > 0.0005 or abs(num(b) - round(sup, 3)) > 0.0005:
                print(f"{f['id']}\tL{lin[0]}\t{k}/{m}: la guía dice [{a}; {b}], Clopper-Pearson da [{inf:.3f}; {sup:.3f}]"); rc = 1; continue
            estado.append('IC95')
        if f['registro'] != '-':
            ruta = os.path.join(registros, f['registro'])
            if not os.path.isfile(ruta): print(f'no existe: {ruta}', file=sys.stderr); sys.exit(2)
            datos = [json.loads(x) for x in open(ruta, encoding='utf-8') if x.strip()]
            sel = [d for d in datos if condicion(f['filtro'])(d)]
            if len(sel) != m:
                print(f"{f['id']}\tL{lin[0]}\tM: la guía dice {m}, el registro da {len(sel)}"); rc = 1; continue
            estado.append('M')
            if f['exito'] != '-':
                kr = sum(1 for d in sel if condicion(f['exito'])(d))
                if kr != k: print(f"{f['id']}\tL{lin[0]}\tk: la guía dice {k}, el registro da {kr}"); rc = 1; continue
                estado.append('k')
        resumen[f['id']] = (lin[0], f"{k}/{m}", f['clase'], '+'.join(estado) or 'clase sin cálculo', f['registro'])
    for id_, (lin, km, clase, est, reg) in resumen.items():
        print(f'{id_}\tL{lin}\t{km}\t{clase}\t{est}\t{reg}')
    print(f'compuerta-intervalos: {len(reconocidas)} figuras reconocidas, {cubiertas} en el mapa; '
          f'{len(resumen)} de {len(filas)} filas coinciden')
    sys.exit(rc)

if __name__ == '__main__':
    main()
