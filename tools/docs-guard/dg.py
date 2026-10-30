#!/usr/bin/env python3
"""docs-guard: controles documentales del carril PR (expediente 0002 de DocAsCode, unidad E1-U2).

Sólo biblioteca estándar de Python 3 (el runner no trae paquetes). Cada subcomando recorre los .md de las rutas que
recibe y termina con un código de salida:

    0  en orden (puede haber avisos)
    1  hay al menos un hallazgo que bloquea
    2  error de uso o de lectura

Subcomandos (grupo del carril PR entre paréntesis; N09 §8.1):

    fuera      RUTA...            (PR-1) enlaces relativos que salen del repositorio
    esquema    RUTA...            (PR-2) frontmatter contra su esquema, registro de prefijos y esquema RRD;
                                         traces-vacio: avisa ante «traces: []» (válido por el anexo; D-17). Cuenta aparte los README.md
                                         del cuerpo sin frontmatter (readme-del-cuerpo-sin-frontmatter, D-24)
    ai         RUTA...            (PR-3) anti-inyección AI-1..AI-5; AI-3 avisa; AI-4 avisa en el anexo de referencias
    trazas     RUTA...            (PR-5) traces-resolve: cada ID@versión contra el registro generado de los encabezados;
                                         con --fuente DIR, además las anclas ruta#Símbolo@SHA de las fichas (nivel 2);
                                         id-duplicado: un ID que un archivo de RUTA declara y otro archivo también
                                         declara da rojo aunque nadie lo trace todavía (D-18)
    contrato   ESPERADO GENERADO  (PR-6) diff semántico de dos contratos OpenAPI (JSON)
    contratos  RUTA...            (PR-6) busca contracts/openapi.json; sin extracción declarada, «no aplica»
    fijadas    RUTA...            (PR-7) equivalente de «copiar.py --check»: cada pieza de BASE-FIJADA.md con su SHA-256
    reflint    RUTA...            (PR-7) formato de referencias APA con ancla ref-…; bloquea sólo en el cuerpo nuevo
    url-fecha  RUTA...            (PR-7) URL y citas por dominio en code spans sin fecha de consulta ni cita a #ref-…;
                                         bloquea en el cuerpo nuevo salvo status: obsoleto (E-F7, D28)
    avisos     RUTA...            (aviso) url-fecha y cita-linea sobre lo heredado; nunca bloquea. Las raíces del
                                         workspace de cita-linea salen de DOCS_GUARD_RAICES o de raices.local.txt
    frescura   RUTA...            (carril programado) last_review con más de 365 días (--dias N); nunca bloquea

Opciones comunes: --raiz DIR (raíz del repositorio que se verifica; por defecto, la de git o el directorio actual).
Opciones de trazas: --fuente DIR (árbol del código al SHA de source.lock, para las anclas); --deriva (DIR es otra
revisión, por ejemplo main: las anclas que no resuelven avisan y el SHA del ancla no se compara).

Los datos de configuración viven al lado de este archivo: dominios.txt (AK-4, C-14), prefijos.txt (registro cerrado de
prefijos, D09), excepciones.txt (AK-3, con huella) y esquemas/*.json. Ver README.md de esta carpeta.
"""

import hashlib
import json
import os
import re
import subprocess
import sys
from pathlib import Path

AQUI = Path(__file__).resolve().parent
FIXTURES = AQUI / 'fixtures'
OMITIR_DIRS = {'.git', 'node_modules', 'bin', 'obj', '.docs-guard'}
RE_ID = r'[A-Z][A-Z0-9]*(?:-[A-Z0-9]+)+'
ANOTAR = os.environ.get('GITHUB_ACTIONS') == 'true'

# --------------------------------------------------------------------------------------------------------------------
# Utilidades


class Informe:
    """Junta hallazgos; los que bloquean ponen el código de salida en 1."""

    def __init__(self, control):
        self.control = control
        self.errores = 0
        self.avisos = 0

    def _linea(self, nivel, ruta, n, regla, texto):
        if ANOTAR:
            print(f'::{nivel} file={ruta},line={n}::{regla}: {texto}')
        else:
            print(f'{ruta}:{n}: {"ERROR" if nivel == "error" else "aviso"} {regla}: {texto}')

    def error(self, ruta, n, regla, texto):
        self.errores += 1
        self._linea('error', ruta, n, regla, texto)

    def aviso(self, ruta, n, regla, texto):
        self.avisos += 1
        self._linea('warning', ruta, n, regla, texto)

    def cerrar(self, extra=''):
        estado = 'ROJO' if self.errores else 'verde'
        print(f'== {self.control}: {estado} (errores={self.errores} avisos={self.avisos}){extra}')
        return 1 if self.errores else 0


def raiz_repo(arg):
    if arg:
        return Path(arg).resolve()
    try:
        r = subprocess.run(['git', 'rev-parse', '--show-toplevel'], capture_output=True, text=True, check=True)
        return Path(r.stdout.strip()).resolve()
    except (subprocess.CalledProcessError, FileNotFoundError):
        return Path.cwd().resolve()


def markdowns(rutas):
    for r in rutas:
        p = Path(r)
        if p.is_file():
            if p.suffix == '.md':
                yield p
            continue
        if not p.is_dir():
            raise FileNotFoundError(r)
        for base, dirs, files in os.walk(p):
            dirs[:] = sorted(d for d in dirs if d not in OMITIR_DIRS)
            for f in sorted(files):
                if f.endswith('.md'):
                    yield Path(base) / f


def leer(p):
    return p.read_text(encoding='utf-8', errors='replace')


def rel(p, raiz):
    try:
        return str(Path(p).resolve().relative_to(raiz))
    except ValueError:
        return str(p)


def escalar(v):
    v = v.strip()
    if len(v) >= 2 and v[0] == v[-1] and v[0] in '"\'':
        return v[1:-1]
    if re.fullmatch(r'-?\d+', v):
        return int(v)
    return v


def frontmatter(texto):
    """Subconjunto de YAML del frontmatter del corpus: «clave: valor», «clave: [a, b]» y listas con «- item».

    Devuelve (dict, línea donde cierra) o (None, 0) si no hay frontmatter.
    """
    lineas = texto.split('\n')
    if not lineas or lineas[0].strip() != '---':
        return None, 0
    datos, clave = {}, None
    for i in range(1, len(lineas)):
        l = lineas[i]
        if l.strip() == '---':
            return datos, i + 1
        if not l.strip() or l.lstrip().startswith('#'):
            continue
        m = re.match(r'^\s+-\s+(.*)$', l)
        if m and clave is not None:
            if not isinstance(datos.get(clave), list):
                datos[clave] = []
            datos[clave].append(escalar(m.group(1)))
            continue
        m = re.match(r'^([A-Za-z_][\w-]*):\s*(.*?)\s*$', l)
        if not m:
            datos.setdefault('__invalidas__', []).append(i + 1)
            continue
        clave, v = m.group(1), m.group(2)
        if v.startswith('[') and v.endswith(']'):
            datos[clave] = [escalar(x) for x in v[1:-1].split(',') if x.strip()]
        elif v == '':
            datos[clave] = None
        else:
            if v[:1] not in '"\'' and (': ' in v or v.endswith(':')):
                # Un escalar YAML sin comillas no puede contener «: »: un lector YAML estricto rechaza el frontmatter.
                datos.setdefault('__no_yaml__', []).append(i + 1)
            datos[clave] = escalar(re.sub(r'\s+#.*$', '', v) if not v[:1] in '"\'' else v)
    return None, 0


def fuera_de_codigo(texto):
    """(número de línea, línea, dentro_de_código) para cada línea."""
    cod = False
    for i, l in enumerate(texto.split('\n'), 1):
        if re.match(r'^\s*(```|~~~)', l):
            cod = not cod
            yield i, l, True
            continue
        yield i, l, cod


def leer_lista(nombre):
    """Líneas sin comentarios de un archivo de datos de esta carpeta."""
    salida = []
    for l in (AQUI / nombre).read_text(encoding='utf-8').split('\n'):
        l = l.split('#', 1)[0].strip()
        if l:
            salida.append(l)
    return salida


# --------------------------------------------------------------------------------------------------------------------
# PR-1: relativos que salen del repositorio

RE_ENLACE = re.compile(r'!?\[[^\]]*\]\(\s*<?([^)\s>]+)>?(?:\s+"[^"]*")?\s*\)')


def cmd_fuera(rutas, raiz):
    inf = Informe('PR-1 fuera-del-repo')
    for f in markdowns(rutas):
        for n, l, cod in fuera_de_codigo(leer(f)):
            if cod:
                continue
            for m in RE_ENLACE.finditer(l):
                destino = m.group(1).split('#', 1)[0]
                if not destino or re.match(r'^[a-z][a-z0-9+.-]*:', destino, re.I) or destino.startswith('/'):
                    continue
                final = (f.parent / destino).resolve()
                try:
                    final.relative_to(raiz)
                except ValueError:
                    inf.error(rel(f, raiz), n, 'relativo-fuera-del-repo', destino)
    return inf.cerrar()


# --------------------------------------------------------------------------------------------------------------------
# PR-2: esquema de frontmatter, registro de prefijos y RRD


def validar(valor, esquema, camino='') -> list:
    """Validador del subconjunto de JSON Schema que usan esquemas/*.json."""
    errs = []
    t = esquema.get('type')
    tipos = {'string': str, 'array': list, 'integer': int, 'object': dict}
    if t and not isinstance(valor, tipos[t]):
        return [f'{camino or "valor"}: se esperaba {t}']
    if 'enum' in esquema and valor not in esquema['enum']:
        errs.append(f'{camino}: «{valor}» fuera del vocabulario {esquema["enum"]}')
    if isinstance(valor, str):
        if 'minLength' in esquema and len(valor) < esquema['minLength']:
            errs.append(f'{camino}: más corto que {esquema["minLength"]}')
        if 'maxLength' in esquema and len(valor) > esquema['maxLength']:
            errs.append(f'{camino}: {len(valor)} caracteres, máximo {esquema["maxLength"]}')
        if 'pattern' in esquema and not re.search(esquema['pattern'], valor):
            errs.append(f'{camino}: «{valor}» no cumple {esquema["pattern"]}')
    if isinstance(valor, list):
        if 'minItems' in esquema and len(valor) < esquema['minItems']:
            errs.append(f'{camino}: menos de {esquema["minItems"]} elementos')
        if esquema.get('uniqueItems') and len(set(map(str, valor))) != len(valor):
            errs.append(f'{camino}: elementos repetidos')
        for i, x in enumerate(valor):
            errs += validar(x, esquema.get('items', {}), f'{camino}[{i}]')
    if isinstance(valor, dict):
        for r in esquema.get('required', []):
            if valor.get(r) in (None, '', []):
                errs.append(f'falta «{r}»')
        for k, sub in esquema.get('properties', {}).items():
            if k in valor and valor[k] is not None:
                errs += validar(valor[k], sub, k)
        if 'if' in esquema and not validar(valor, esquema['if']):
            errs += validar(valor, esquema.get('then', {}), camino)
        if 'oneOf' in esquema:
            ok = sum(1 for s in esquema['oneOf'] if not validar(valor, s))
            if ok != 1:
                errs.append(f'{camino or "frontmatter"}: debe cumplir exactamente una de {len(esquema["oneOf"])} formas (oneOf)')
    return errs


def prefijos():
    reg = {}
    for l in leer_lista('prefijos.txt'):
        partes = l.split(None, 2)
        reg[partes[0]] = partes[1] if len(partes) > 1 else 'interno'
    return reg


def cuerpo_nuevo(f, raiz):
    """Una carpeta es «cuerpo nuevo» si ella o un ancestro dentro del repo tiene README.md con corpus_id."""
    d = f.parent.resolve()
    while True:
        r = d / 'README.md'
        if r.is_file():
            fm, _ = frontmatter(leer(r))
            if fm and fm.get('corpus_id'):
                return True
        if d == raiz or d.parent == d:
            return False
        d = d.parent


COLUMNAS_RRD = ['ID', 'Audiencia', 'Tarea', 'Pieza', 'Criterio', 'Decisión']
DECISIONES_RRD = {'aceptado', 'rechazado', 'diferido', 'pendiente'}


def validar_rrd(texto, inf, ruta):
    filas = [(n, l) for n, l, cod in fuera_de_codigo(texto) if not cod and l.strip().startswith('|')]
    cab = None
    for n, l in filas:
        celdas = [c.strip() for c in l.strip().strip('|').split('|')]
        if cab is None:
            if all(c in celdas for c in COLUMNAS_RRD):
                cab = celdas
            continue
        if set(''.join(celdas)) <= set('-: '):
            continue
        fila = dict(zip(cab, celdas))
        if not re.fullmatch(r'RD-\d{2,}', fila.get('ID', '')):
            inf.error(ruta, n, 'rrd-id', f'ID «{fila.get("ID")}» no es RD-nn')
        if fila.get('Decisión', '').lower() not in DECISIONES_RRD:
            inf.error(ruta, n, 'rrd-decision', f'«{fila.get("Decisión")}» fuera de {sorted(DECISIONES_RRD)}')
        if not re.search(r'\bDado\b.*\bCuando\b.*\bEntonces\b', fila.get('Criterio', ''), re.I):
            inf.error(ruta, n, 'rrd-criterio', 'el criterio no tiene la forma Dado/Cuando/Entonces')
    if cab is None:
        inf.error(ruta, 1, 'rrd-tabla', f'falta la tabla con las columnas {COLUMNAS_RRD}')


def cmd_esquema(rutas, raiz):
    inf = Informe('PR-2 esquema')
    esq = {n: json.loads((AQUI / 'esquemas' / f'{n}.schema.json').read_text(encoding='utf-8'))
           for n in ('cuerpo', 'ayuda', 'readme-cuerpo')}
    reg = prefijos()
    vistos = sin = readme_sin = 0
    for f in markdowns(rutas):
        texto = leer(f)
        ruta = rel(f, raiz)
        fm, _ = frontmatter(texto)
        nuevo = cuerpo_nuevo(f, raiz)
        if fm is None:
            if nuevo and f.name != 'README.md':
                inf.error(ruta, 1, 'sin-frontmatter', 'todo documento del cuerpo abre con frontmatter (N09 §7)')
            elif nuevo:
                readme_sin += 1  # README.md de una subcarpeta del cuerpo: no se le exige frontmatter
            else:
                sin += 1
            continue
        vistos += 1
        for n in fm.pop('__invalidas__', []):
            inf.error(ruta, n, 'frontmatter-ilegible', 'línea que no es «clave: valor» ni «- elemento»')
        for n in fm.pop('__no_yaml__', []):
            (inf.error if nuevo else inf.aviso)(ruta, n, 'frontmatter-no-yaml',
                                                'valor sin comillas que contiene «: »; un lector YAML no lo acepta: entrecomillarlo')
        if f.name == 'README.md' and fm.get('corpus_id'):
            forma = 'readme-cuerpo'
        elif 'doc_id' in fm:
            forma = 'cuerpo'
        elif 'roles' in fm or 'translation_of' in fm:
            forma = 'ayuda'
        else:
            (inf.error if nuevo else inf.aviso)(ruta, 1, 'frontmatter-sin-forma',
                                                'no tiene doc_id ni roles/translation_of: no se sabe qué esquema aplica')
            continue
        errs = validar(fm, esq[forma])
        if forma == 'cuerpo' and fm.get('traces') == []:
            # El anexo de esquema admite «traces: []» (Esquema-Frontmatter-Y-Prefijos.md:54; PE-2 del 290). La práctica
            # de D-17 (trazar al menos al README del cuerpo) queda como aviso, no como bloqueo.
            errs = [e for e in errs if e != 'falta «traces»']
            inf.aviso(ruta, 1, 'traces-vacio', 'traces: [] es válido, pero conviene trazar al menos al doc_id del '
                                               'README de su cuerpo (D-17)')
        for e in errs:
            inf.error(ruta, 1, f'esquema-{forma}', e)
        if forma == 'cuerpo':
            pre = str(fm.get('doc_id', '')).split('-', 1)[0]
            if pre not in reg:
                inf.error(ruta, 1, 'prefijo', f'«{pre}-» no está en el registro (prefijos.txt)')
            if fm.get('doc_type') == 'rrd' or f.name == '_requisitos.md':
                validar_rrd(texto, inf, ruta)
    return inf.cerrar(f' archivos-con-frontmatter={vistos} sin-frontmatter-fuera-del-cuerpo={sin}'
                      f' readme-del-cuerpo-sin-frontmatter={readme_sin}')


# --------------------------------------------------------------------------------------------------------------------
# PR-3: anti-inyección AI-1..AI-5

RE_COMENTARIO = re.compile(r'<!--.*?-->', re.S)
RE_INVISIBLE = re.compile('[\u200b-\u200f\u202a-\u202e\u2060-\u2064\u2066-\u2069\ufeff\U000e0000-\U000e007f]')
RE_FRASE = re.compile(
    r'(ignore (all |any )?(previous|prior|above) instructions|ignor[aá] (las|todas las) instrucciones'
    r'|olvid[aá] (las|todas las) instrucciones|instrucciones para (el |los )?agentes?|agent instructions'
    r'|note to (the )?(ai|llm|assistant|agent)|nota para (el|la) (ia|asistente|modelo|agente)'
    r'|system prompt|you are (now )?an? (ai|assistant|agent)|sos un asistente|eres un asistente'
    r'|ejecut[aá] (el siguiente|este) comando|run the following command)', re.I)
RE_HTML_OCULTO = re.compile(r'<[a-z][^>]*(\shidden\b|display\s*:\s*none|visibility\s*:\s*hidden|font-size\s*:\s*0)[^>]*>', re.I)
RE_URL = re.compile(r'(?:https?|ftp)://([A-Za-z0-9.-]+)(?::\d+)?[^\s)>\]`"\'|]*')


def dominios():
    return [d.lower() for d in leer_lista('dominios.txt')]


def permitido(host, lista):
    host = host.lower().rstrip('.')
    return any(host == d or host.endswith('.' + d) for d in lista)


def excepciones(raiz):
    """AK-3: «<sha256>  <ruta>  <control>». La excepción vale sólo mientras la huella del archivo coincida."""
    exc = {}
    for l in leer_lista('excepciones.txt'):
        partes = l.split()
        if len(partes) >= 3:
            exc[(partes[1], partes[2])] = partes[0]
    return exc


def exceptuado(exc, ruta, control, datos):
    h = exc.get((ruta, control))
    return h is not None and h == hashlib.sha256(datos).hexdigest()


def es_anexo_de_referencias(f, fm):
    return bool(re.search(r'referencias', f.name, re.I)) or (fm or {}).get('doc_type') == 'anexo-referencias'


def cmd_ai(rutas, raiz, ai3_bloquea=False):
    inf = Informe('PR-3 anti-inyeccion')
    lista = dominios()
    exc = excepciones(raiz)
    cuenta = {k: 0 for k in ('AI-1', 'AI-2', 'AI-3', 'AI-4', 'AI-5')}
    for f in markdowns(rutas):
        datos = f.read_bytes()
        texto = datos.decode('utf-8', errors='replace')
        ruta = rel(f, raiz)
        fm, _ = frontmatter(texto)
        anexo = es_anexo_de_referencias(f, fm)

        def hallazgo(ctrl, n, regla, txt, bloquea=True):
            if exceptuado(exc, ruta, ctrl, datos):
                return
            cuenta[ctrl] += 1
            (inf.error if bloquea else inf.aviso)(ruta, n, f'{ctrl} {regla}', txt)

        for m in RE_COMENTARIO.finditer(texto):
            hallazgo('AI-1', texto[:m.start()].count('\n') + 1, 'comentario-html', m.group(0)[:60].replace('\n', ' '))
        for n, l, _ in fuera_de_codigo(texto):
            for m in RE_INVISIBLE.finditer(l):
                hallazgo('AI-2', n, 'caracter-invisible', f'U+{ord(m.group(0)):04X}')
            m = RE_FRASE.search(l)
            if m:
                hallazgo('AI-3', n, 'frase-dirigida-al-agente', m.group(0), bloquea=ai3_bloquea)
            for m in RE_URL.finditer(l):
                host = m.group(1)
                if host and not permitido(host, lista):
                    hallazgo('AI-4', n, 'dominio-no-permitido', host, bloquea=not anexo)
            m = RE_HTML_OCULTO.search(l)
            if m:
                hallazgo('AI-5', n, 'html-oculto', m.group(0)[:60])
    extra = ' ' + ' '.join(f'{k}={v}' for k, v in cuenta.items())
    return inf.cerrar(extra)


# --------------------------------------------------------------------------------------------------------------------
# PR-5: traces-resolve

RE_TRAZA = re.compile(rf'^({RE_ID})(?:@(\d+)\.(\d+))?$')
RE_ENCABEZADO_ID = re.compile(rf'^#{{1,6}}\s+`?({RE_ID})`?(?=[\s:—.\-]|$)')
RE_VERSION = re.compile(r'^\*\*Versi[oó]n:\*\*\s*(\d+)\.(\d+)')


def en_fixtures(p):
    """Dentro de un tools/docs-guard/fixtures/: el de estas herramientas o el del repositorio que se verifica (en la CI
    las herramientas se sacan en .docs-guard/ y el repositorio trae su propia copia)."""
    r = Path(p).resolve()
    partes = r.parts
    return FIXTURES in r.parents or any(partes[i:i + 3] == ('tools', 'docs-guard', 'fixtures') for i in range(len(partes)))


def declaraciones(f, texto=None):
    """[(ID, versión o None, línea)] que declara un archivo: doc_id del frontmatter, filas RD-nn de un RRD y
    encabezados que empiezan con un ID (con la **Versión:** que los sigue)."""
    texto = leer(f) if texto is None else texto
    decl = []
    fm, _ = frontmatter(texto)
    if fm and fm.get('doc_id'):
        v = str(fm.get('version', '')) or None
        decl.append([str(fm['doc_id']), '.'.join(v.split('.')[:2]) if v else None, 1])
    if f.name == '_requisitos.md' or (fm or {}).get('doc_type') == 'rrd':
        for n, l, cod in fuera_de_codigo(texto):  # las filas RD-nn del RRD también declaran su ID
            m = None if cod else re.match(r'^\|\s*(RD-\d{2,})\s*\|', l)
            if m:
                decl.append([m.group(1), None, n])
    actual = None
    for n, l, cod in fuera_de_codigo(texto):
        if cod:
            continue
        m = RE_ENCABEZADO_ID.match(l)
        if m:
            actual = [m.group(1), None, n]
            decl.append(actual)
            continue
        m = RE_VERSION.match(l)
        if m and actual is not None and actual[1] is None:
            actual[1] = f'{m.group(1)}.{m.group(2)}'
    return decl


def registro(directorios):
    """ID → [(archivo, versión, línea)] desde doc_id del frontmatter y desde los encabezados que empiezan con un ID."""
    reg = {}
    for d in directorios:
        for f in markdowns([d]):
            if en_fixtures(f) and not en_fixtures(Path(d) / '_'):
                continue  # los fixtures declaran IDs de prueba: no entran al registro del repositorio
            for ident, v, n in declaraciones(f):
                reg.setdefault(ident, []).append((f, v, n))
    return reg


def source_lock(f, tope):
    d = f.parent.resolve()
    while True:
        if (d / 'source.lock').is_file():
            return d / 'source.lock'
        if d == tope or d.parent == d:
            return None
        d = d.parent


RE_ANCLA = re.compile(r'^`([^`#\s]+)#([A-Za-z_][\w.]*)@([0-9a-f]{7,40})`$')
RE_AFIRMA = re.compile(r'^(=|contiene|no contiene)\s+`([^`]*)`$')


def lock_sha(lock):
    for l in leer(lock).split('\n'):
        m = re.match(r'^sha:\s*([0-9a-f]{7,40})\s*$', l)
        if m:
            return m.group(1)
    return None


def simbolo_cs(texto, simbolo):
    """Declaración de Clase.Miembro (o Miembro) en C#: (texto de la declaración hasta «;» o «{», valor tras «=»)."""
    partes = simbolo.split('.')
    if len(partes) > 1 and not re.search(rf'\b(class|record|struct|interface|enum)\s+{re.escape(partes[-2])}\b', texto):
        return None
    nombre = re.escape(partes[-1])
    m = re.search(rf'^[ \t]*(?:\[[^\]\n]*\][ \t]*)*(?:(?:public|private|internal|protected|static|const|readonly|'
                  rf'partial|sealed|abstract|override|virtual|async|required)\s+)+[\w<>\[\]?,. ]*?\b{nombre}\b'
                  rf'\s*(?P<t>=>|=|\(|\{{|;|$)', texto, re.M)
    if not m:
        return None
    resto = texto[m.start():]
    k = m.start('t') - m.start()
    if m.group('t') == '(':  # método: se saltan los parámetros (pueden traer «= default»)
        nivel = 0
        for k in range(k, len(resto)):
            nivel += {'(': 1, ')': -1}.get(resto[k], 0)
            if nivel == 0:
                break
    t = re.compile(r'=>|=|;|\{').search(resto, k + 1 if m.group('t') == '(' else k)
    if t and t.group(0) in ('=>', '='):  # expresión: hasta el «;» que la cierra
        fin = resto.find(';', t.end())
        fin = fin if fin >= 0 else len(resto) - 1
    elif t:
        fin = t.start()
    else:
        fin = len(resto) - 1
    if resto[fin:fin + 1] == '{':  # cuerpo de bloque: hasta la llave que lo cierra
        nivel = 0
        for i in range(fin, len(resto)):
            nivel += {'{': 1, '}': -1}.get(resto[i], 0)
            if nivel == 0:
                fin = i
                break
    decl = resto[:fin + 1]
    v = re.search(rf'\b{nombre}\s*=(?!>)\s*([^;]+);', decl)
    return decl, (v.group(1).strip().strip('"') if v else None)


def simbolo_xaml(texto, simbolo):
    """Elemento por x:Name o AutomationId y, si se pide, su atributo: Nombre o Nombre.Atributo."""
    nombre, _, atributo = simbolo.partition('.')
    m = re.search(rf'<[\w:.]+\s[^>]*\b(?:x:Name|AutomationId)="{re.escape(nombre)}"[^>]*>', texto, re.S)
    if not m:
        return None
    if not atributo:
        return m.group(0), None
    v = re.search(rf'\b{re.escape(atributo)}="([^"]*)"', m.group(0))
    return (m.group(0), v.group(1)) if v else None


def anclas(f, raiz, fuente, deriva, inf):
    """Nivel 2 de D10: cada fila «| rol | `ruta#Símbolo@SHA` | afirmación |» de la tabla de anclas de una ficha."""
    texto = leer(f)
    ruta = rel(f, raiz)
    lock = source_lock(f, raiz)
    sha = lock_sha(lock) if lock else None
    total = ok = 0
    cab = None
    for n, l, cod in fuera_de_codigo(texto):
        if cod or not l.strip().startswith('|'):
            cab = None
            continue
        celdas = [c.strip() for c in l.strip().strip('|').split('|')]
        if cab is None:
            cab = celdas if ('Ancla' in celdas and 'Afirma' in celdas) else []
            continue
        if not cab or set(''.join(celdas)) <= set('-: '):
            continue
        fila = dict(zip(cab, celdas))
        total += 1
        m = RE_ANCLA.match(fila.get('Ancla', ''))
        if not m:
            inf.error(ruta, n, 'ancla-mal-formada', f'«{fila.get("Ancla")}» no es `ruta#Símbolo@SHA`')
            continue
        archivo, simbolo, sha_ancla = m.groups()
        reportar = inf.aviso if deriva else inf.error
        if not deriva:
            if sha is None:
                inf.error(ruta, n, 'ancla-sin-lock', 'la ficha no tiene source.lock en su carpeta o en una superior')
                continue
            if not (sha.startswith(sha_ancla) or sha_ancla.startswith(sha)):
                inf.error(ruta, n, 'ancla-sha-distinto', f'{sha_ancla} no es el SHA de source.lock ({sha[:12]})')
                continue
        p = Path(fuente) / archivo
        if not p.is_file():
            reportar(ruta, n, 'ancla-sin-archivo', f'{archivo} no existe en la fuente')
            continue
        fuente_txt = leer(p)
        hallado = simbolo_xaml(fuente_txt, simbolo) if p.suffix == '.xaml' else simbolo_cs(fuente_txt, simbolo)
        if hallado is None:
            reportar(ruta, n, 'ancla-sin-simbolo', f'{archivo}#{simbolo} no se encuentra')
            continue
        decl, valor = hallado
        afirma = fila.get('Afirma', '').strip()
        if afirma not in ('', '—', '-'):
            a = RE_AFIRMA.match(afirma)
            if not a:
                inf.error(ruta, n, 'afirmacion-mal-formada', f'«{afirma}»: se espera = `v`, contiene `t` o no contiene `t`')
                continue
            op, esperado = a.groups()
            if op == '=' and valor != esperado:
                reportar(ruta, n, 'ancla-valor-distinto', f'{simbolo}: la ficha afirma {esperado} y el código tiene {valor}')
                continue
            if op == 'contiene' and esperado not in decl:
                reportar(ruta, n, 'ancla-no-contiene', f'{simbolo} no contiene «{esperado}»')
                continue
            if op == 'no contiene' and esperado in decl:
                reportar(ruta, n, 'ancla-contiene', f'{simbolo} contiene «{esperado}»')
                continue
        ok += 1
    return total, ok


def es_ficha(f, fm):
    return (fm or {}).get('doc_type') == 'ficha'


def cmd_trazas(rutas, raiz, fuentes, deriva=False):
    inf = Informe('PR-5 traces-resolve' + (' (deriva: sólo avisa)' if deriva else ''))
    if deriva:  # la otra revisión sólo se compara por anclas; las trazas de nivel 1 ya se verificaron
        n_a = ok_a = 0
        for f in markdowns(rutas):
            fm, _ = frontmatter(leer(f))
            if es_ficha(f, fm):
                t, o = anclas(f, raiz, fuentes[0], True, inf)
                n_a, ok_a = n_a + t, ok_a + o
        inf.cerrar(f' anclas={n_a} resueltas={ok_a}')
        return 0
    reg = registro([raiz] + [Path(x) for x in fuentes])
    pre = prefijos()
    total = ok = duplicados = 0
    n_a = ok_a = sin_verificar = 0
    for f in markdowns(rutas):
        texto = leer(f)
        fm, _ = frontmatter(texto)
        # D-18: un ID que este archivo declara y que otro también declara da rojo aunque nadie lo trace todavía.
        # La traza que llegue después sería traza-ambigua; así se ve en el PR que introduce el duplicado.
        if not (en_fixtures(f) and not en_fixtures(raiz / '_')):
            ruta_f = rel(f, raiz)
            for ident, _, n in declaraciones(f, texto):
                otros = [h for h in reg.get(ident, []) if not (h[0].resolve() == f.resolve() and h[2] == n)]
                if otros:
                    duplicados += 1
                    inf.error(ruta_f, n, 'id-duplicado', f'{ident}: también declarado en '
                              + ', '.join(f'{rel(h[0], raiz)}:{h[2]}' for h in otros[:3])
                              + (f' y {len(otros) - 3} más' if len(otros) > 3 else ''))
        if es_ficha(f, fm):
            if fuentes:
                t, o = anclas(f, raiz, fuentes[0], False, inf)
                n_a, ok_a = n_a + t, ok_a + o
            else:
                sin_verificar += 1
        if not fm or not fm.get('traces'):
            continue
        ruta = rel(f, raiz)
        for t in fm['traces'] if isinstance(fm['traces'], list) else [fm['traces']]:
            total += 1
            t = str(t)
            m = RE_TRAZA.match(t)
            if not m:
                inf.error(ruta, 1, 'traza-mal-formada', f'«{t}» no es ID o ID@MAYOR.MENOR')
                continue
            ident, mayor, menor = m.groups()
            hits = reg.get(ident, [])
            if not hits:
                prefijo = ident.split('-', 1)[0]
                if pre.get(prefijo) == 'externo' and source_lock(f, raiz) is None and not fuentes:
                    inf.aviso(ruta, 1, 'traza-no-verificable',
                              f'{t}: ID de un sistema externo sin source.lock que fije contra qué SHA resolverla')
                    continue
                inf.error(ruta, 1, 'traza-no-resuelve', f'{t}: el ID no está declarado en el registro')
                continue
            if len(hits) > 1:
                inf.error(ruta, 1, 'traza-ambigua', f'{t}: declarado {len(hits)} veces ({", ".join(rel(h[0], raiz) for h in hits[:3])})')
                continue
            if mayor is not None:
                vd = hits[0][1]
                if vd is None:
                    inf.error(ruta, 1, 'traza-sin-version', f'{t}: la declaración no tiene **Versión:** ni version')
                    continue
                if vd != f'{mayor}.{menor}':
                    inf.error(ruta, 1, 'traza-version-desfasada', f'{t}: la declaración está en {vd}')
                    continue
            ok += 1
    if sin_verificar:
        print(f'anclas: {sin_verificar} fichas sin --fuente; sus anclas las verifica el workflow de su source.lock')
    return inf.cerrar(f' trazas={total} resueltas={ok} ids-duplicados={duplicados} anclas={n_a} anclas-resueltas={ok_a}')


# --------------------------------------------------------------------------------------------------------------------
# PR-6: contrato OpenAPI


def operaciones(doc):
    ops = {}
    for camino, metodos in (doc.get('paths') or {}).items():
        for metodo, op in (metodos or {}).items():
            if metodo.lower() in ('get', 'put', 'post', 'delete', 'patch', 'head', 'options', 'trace'):
                ops[f'{metodo.upper()} {camino}'] = json.dumps(op, sort_keys=True)
    return ops


def cmd_contrato(esperado, generado):
    inf = Informe('PR-6 contrato')
    try:
        a = json.loads(Path(esperado).read_text(encoding='utf-8'))
        b = json.loads(Path(generado).read_text(encoding='utf-8'))
    except (OSError, json.JSONDecodeError) as e:
        print(f'ERROR de lectura: {e}', file=sys.stderr)
        return 2
    oa, ob = operaciones(a), operaciones(b)
    for k in sorted(ob.keys() - oa.keys()):
        inf.error(generado, 1, 'operacion-nueva', f'{k} no está en el contrato versionado')
    for k in sorted(oa.keys() - ob.keys()):
        inf.error(esperado, 1, 'operacion-quitada', f'{k} ya no la emite el código')
    for k in sorted(oa.keys() & ob.keys()):
        if oa[k] != ob[k]:
            inf.error(esperado, 1, 'operacion-cambiada', k)
    if not inf.errores and json.dumps(a, sort_keys=True) != json.dumps(b, sort_keys=True):
        inf.error(esperado, 1, 'contrato-distinto', 'difiere fuera de paths (componentes, info o servidores)')
    return inf.cerrar(f' operaciones={len(oa)}')


def cmd_contratos(rutas, raiz):
    inf = Informe('PR-6 contratos')
    n = 0
    for r in rutas:
        for c in sorted(Path(r).rglob('contracts/openapi.json')):
            n += 1
            gen = c.parent / '.generado' / 'openapi.json'
            lock = source_lock(c, raiz)
            if gen.is_file():
                if cmd_contrato(str(c), str(gen)):
                    inf.error(rel(c, raiz), 1, 'contrato', 'difiere del extraído en el build')
            elif lock is not None and re.search(r'^workflow:\s*\S+', leer(lock), re.M):
                wf = re.search(r'^workflow:\s*(\S+)', leer(lock), re.M).group(1)
                print(f'{rel(c, raiz)}: lo extrae y compara {wf} (source.lock); este job no compila el código')
            else:
                inf.error(rel(c, raiz), 1, 'contrato-sin-extraccion',
                          'hay contrato versionado pero el build no dejó contracts/.generado/openapi.json')
    if n == 0:
        print('no aplica: la ruta no tiene contracts/openapi.json')
    return inf.cerrar(f' contratos={n}')


# --------------------------------------------------------------------------------------------------------------------
# PR-7: piezas fijadas y referencias


def cmd_fijadas(rutas, raiz):
    inf = Informe('PR-7 fijadas')
    n = 0
    for r in rutas:
        for b in sorted(Path(r).rglob('BASE-FIJADA.md')):
            destino = b.parent.parent
            cab = None
            for i, l in enumerate(leer(b).split('\n'), 1):
                if not l.startswith('|'):
                    continue
                celdas = [c.strip().strip('`') for c in l.strip().strip('|').split('|')]
                if cab is None:
                    if 'Archivo' in celdas and 'SHA-256' in celdas:
                        cab = celdas
                    continue
                if set(''.join(celdas)) <= set('-: '):
                    continue
                fila = dict(zip(cab, celdas))
                n += 1
                p = destino / fila['Archivo']
                if not p.is_file():
                    inf.error(rel(b, raiz), i, 'pieza-faltante', fila['Archivo'])
                elif hashlib.sha256(p.read_bytes()).hexdigest() != fila['SHA-256']:
                    inf.error(rel(b, raiz), i, 'huella-distinta', f'{fila["Archivo"]} cambió respecto de lo fijado')
    if n == 0:
        print('no aplica: la ruta no tiene piezas fijadas (BASE-FIJADA.md)')
    return inf.cerrar(f' piezas={n}')


RE_REF = re.compile(r'^<a id="(ref-[^"]+)"></a>(.*)$', re.M)


def cmd_reflint(rutas, raiz):
    inf = Informe('PR-7 reflint')
    total = 0
    for f in markdowns(rutas):
        texto = leer(f)
        refs = RE_REF.findall(texto)
        citadas = set(re.findall(r'\]\([^)#]*#(ref-[^)\s]+)\)', texto))
        if not refs and not citadas:
            continue
        ruta = rel(f, raiz)
        nuevo = cuerpo_nuevo(f, raiz)
        reportar = inf.error if nuevo else inf.aviso
        for rid, cuerpo in refs:
            total += 1
            n = texto[:texto.find(f'id="{rid}"')].count('\n') + 1
            if not re.search(r'\((\d{4}[a-z]?|s\. f\.)(,[^)]*)?\)', cuerpo):
                reportar(ruta, n, 'ref-sin-anio', rid)
            if not re.search(r'\*[^*]+\*', cuerpo):
                reportar(ruta, n, 'ref-sin-titulo-en-cursiva', rid)
            if re.search(r'https?://', cuerpo) and not re.search(r'consultad[oa]s? el \d{4}-\d{2}-\d{2}', cuerpo):
                reportar(ruta, n, 'ref-url-sin-fecha-de-consulta', rid)
        declaradas = {r for r, _ in refs}
        # Las citas a un anexo de otro archivo se resuelven por lychee (anclas); acá sólo las del mismo archivo.
        locales = set(re.findall(r'\]\(#(ref-[^)\s]+)\)', texto))
        for c in sorted(locales - declaradas):
            reportar(ruta, 1, 'ref-citada-sin-entrada', c)
    return inf.cerrar(f' referencias={total}')


# --------------------------------------------------------------------------------------------------------------------
# Avisos sobre lo heredado: url-fecha y cita-linea (D18: avisan)

# Raíces del workspace que reconoce cita-linea. No se listan acá (el repositorio es público y los nombres de las
# carpetas del workspace son de cada instalación): por defecto, toda ruta absoluta cuyo primer componente empieza con
# mayúscula, que es la convención de las raíces del workspace y deja afuera las del sistema (/etc, /usr, /tmp…). Una
# instalación puede fijar su lista exacta, sin publicarla, en la variable DOCS_GUARD_RAICES (nombres separados por
# espacios o comas) o en raices.local.txt al lado de este archivo (uno por línea; está en .gitignore).
def raices_ws():
    nombres = re.split(r'[\s,]+', os.environ.get('DOCS_GUARD_RAICES', '').strip())
    local = AQUI / 'raices.local.txt'
    if not any(nombres) and local.is_file():
        nombres = leer_lista('raices.local.txt')
    nombres = [n.strip('/') for n in nombres if n.strip('/')]
    if nombres:
        return re.compile(r'^/(' + '|'.join(map(re.escape, nombres)) + r')/')
    return re.compile(r'^/[A-Z][\w.-]*/')


RAICES_WS = raices_ws()


def cmd_avisos(rutas, raiz):
    inf = Informe('avisos url-fecha y cita-linea')
    for f in markdowns(rutas):
        ruta = rel(f, raiz)
        for n, l, cod in fuera_de_codigo(leer(f)):
            if cod:
                continue
            urls = re.findall(r'https?://[^\s)>\]`|]+', l)
            if urls and not re.search(r'consultad[oa]s? el \d{4}-\d{2}-\d{2}', l, re.I):
                inf.aviso(ruta, n, 'url-fecha', urls[0])
            for m in re.finditer(r'`(/[^`\s]+\.(md|cs|csproj|yml|yaml|json|props|razor|sh|txt|ps1))(:[0-9][0-9,\-]*)?`', l):
                if RAICES_WS.match(m.group(1)) and not m.group(3):
                    inf.aviso(ruta, n, 'cita-linea', m.group(1))
    inf.cerrar()
    return 0


# --------------------------------------------------------------------------------------------------------------------
# PR-7: url-fecha ampliado (E1-U4f, E-F7, D28). Una fuente externa citada en el texto lleva referencia completa:
# fecha de consulta en el mismo bloque o cita enlazada al anexo de referencias (#ref-…). Ve las URL con esquema y,
# además, las citas por dominio escritas en code spans (`docusaurus.io/docs/…`), que el aviso heredado no veía.
# Bloquea en el cuerpo nuevo salvo en documentos con status: obsoleto; en lo demás avisa.

# La fecha de consulta; también vale la de una observación propia fechada («medido el», «verificado el»).
RE_FECHA = re.compile(r'(?:consultad|medid|verificad)[oa]s? el \d{4}-\d{2}-\d{2}', re.I)
RE_URL_CITA = re.compile(r'https?://[^\s)>\]`|]+')
RE_SPAN = re.compile(r'`([^`\n]+)`')
# Dominio de primer nivel citable: los genéricos de la lista y cualquier código de país de dos letras, salvo los que
# coinciden con extensiones de archivo (`README.md`, `ci-local.sh`, `Program.cs` no son dominios).
TLD_GENERICOS = {'com', 'org', 'net', 'edu', 'gov', 'info', 'dev', 'app', 'cloud', 'page', 'site', 'tech', 'blog'}
TLD_EXTENSIONES = {'md', 'sh', 'cs', 'js', 'ts', 'py', 'rb', 'go', 'rs', 'ps', 'pl', 'mk', 'so', 'gz', 'xz', 'bz',
                   'db', 'fs', 'vb', 'cc', 'hh', 'el', 'jl', 'kt', 'lua', 'nu'}
RE_DOMINIO = re.compile(r'(?:https?://)?(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+([a-z]{2,6})(?:[/:?#][^\s]*)?')


def es_dominio(s):
    m = RE_DOMINIO.fullmatch(s)
    if not m:
        return False
    tld = m.group(1)
    return tld in TLD_GENERICOS or (len(tld) == 2 and tld not in TLD_EXTENSIONES)


# No son citas: direcciones locales, dominios reservados para ejemplos (RFC 2606 y 6761), plantillas con «…» y la
# dirección de clonado de un repositorio (termina en .git: es un argumento de una orden, no una fuente) y la imagen
# de contenedor con etiqueta.
RE_NO_CITA = re.compile(r'^(?:https?://)?(?:localhost|127\.|0\.0\.0\.0|\[::1\]|[^/]*\.(?:local|test|example|invalid)'
                        r'(?:[:/]|$)|(?:[^/]*\.)?example\.(?:com|org|net)(?:[:/]|$))|…|\.git$'
                        # Imagen de contenedor con etiqueta (registro/ruta:etiqueta): se nombra, no se cita.
                        r'|^[a-z0-9.-]+\.[a-z]+/[^\s:]+:[\w.-]+$')


def bloques_de_texto(texto):
    """(línea inicial, [(n, línea)]) por bloque: párrafo separado por línea vacía; cada fila de tabla y cada
    encabezado es su propio bloque. Omite el frontmatter y los bloques de código cercados."""
    _, fin_fm = frontmatter(texto)
    bloque = []
    for n, l, cod in fuera_de_codigo(texto):
        if n <= fin_fm or cod or not l.strip() or l.lstrip().startswith(('|', '#')):
            if bloque:
                yield bloque
                bloque = []
            if n > fin_fm and not cod and l.lstrip().startswith(('|', '#')):
                yield [(n, l)]
            continue
        bloque.append((n, l))
    if bloque:
        yield bloque


def citas_externas(texto):
    """(n, regla, cita) de cada fuente externa sin referencia completa en su bloque."""
    for bloque in bloques_de_texto(texto):
        junto = '\n'.join(l for _, l in bloque)
        if RE_FECHA.search(junto) or re.search(r'\]\([^)\s]*#ref-[^)\s]+\)', junto):
            continue
        for n, l in bloque:
            vistos = set()
            for u in RE_URL_CITA.findall(l):
                u = u.rstrip('.,;:')
                if not RE_NO_CITA.search(u):
                    vistos.add(u)
                    yield n, 'url-sin-fecha', u
            for m in RE_SPAN.finditer(l):
                s = m.group(1).strip()
                if es_dominio(s) and not RE_NO_CITA.search(s) and s not in vistos:
                    yield n, 'dominio-sin-referencia', s


def cmd_url_fecha(rutas, raiz):
    inf = Informe('PR-7 url-fecha')
    bloquean = 0
    for f in markdowns(rutas):
        texto = leer(f)
        fm, _ = frontmatter(texto)
        obsoleto = str((fm or {}).get('status') or '') == 'obsoleto'
        bloquea = cuerpo_nuevo(f, raiz) and not obsoleto
        bloquean += bloquea
        reportar = inf.error if bloquea else inf.aviso
        ruta = rel(f, raiz)
        for n, regla, cita in citas_externas(texto):
            reportar(ruta, n, regla, cita)
    return inf.cerrar(f' documentos-que-bloquean={bloquean}')


def cmd_frescura(rutas, raiz, dias):
    import datetime
    inf = Informe(f'frescura last_review > {dias} días')
    hoy = datetime.date.today()
    n = 0
    for f in markdowns(rutas):
        fm, _ = frontmatter(leer(f))
        lr = str((fm or {}).get('last_review') or '')
        if not re.fullmatch(r'\d{4}-\d{2}-\d{2}', lr):
            continue
        n += 1
        edad = (hoy - datetime.date.fromisoformat(lr)).days
        if edad > dias:
            inf.aviso(rel(f, raiz), 1, 'last_review-vencido', f'{lr} ({edad} días)')
    inf.cerrar(f' con-last_review={n}')
    return 0


# --------------------------------------------------------------------------------------------------------------------


def main(argv):
    args, opts = [], {'--raiz': None, '--fuente': [], '--dias': '365'}
    banderas = set()
    i = 0
    while i < len(argv):
        a = argv[i]
        if a in ('--raiz', '--dias'):
            opts[a] = argv[i + 1]
            i += 2
        elif a == '--fuente':
            opts['--fuente'].append(argv[i + 1])
            i += 2
        elif a.startswith('--'):
            banderas.add(a)
            i += 1
        else:
            args.append(a)
            i += 1
    if not args:
        print(__doc__)
        return 2
    cmd, rutas = args[0], args[1:]
    raiz = raiz_repo(opts['--raiz'])
    try:
        if cmd == 'contrato':
            if len(rutas) != 2:
                print('uso: dg.py contrato ESPERADO GENERADO', file=sys.stderr)
                return 2
            return cmd_contrato(*rutas)
        if not rutas:
            print(f'uso: dg.py {cmd} RUTA...', file=sys.stderr)
            return 2
        tabla = {
            'fuera': lambda: cmd_fuera(rutas, raiz),
            'esquema': lambda: cmd_esquema(rutas, raiz),
            'ai': lambda: cmd_ai(rutas, raiz, '--ai3-bloquea' in banderas),
            'trazas': lambda: cmd_trazas(rutas, raiz, opts['--fuente'], '--deriva' in banderas),
            'contratos': lambda: cmd_contratos(rutas, raiz),
            'fijadas': lambda: cmd_fijadas(rutas, raiz),
            'reflint': lambda: cmd_reflint(rutas, raiz),
            'url-fecha': lambda: cmd_url_fecha(rutas, raiz),
            'avisos': lambda: cmd_avisos(rutas, raiz),
            'frescura': lambda: cmd_frescura(rutas, raiz, int(opts['--dias'])),
        }
        if cmd not in tabla:
            print(f'subcomando desconocido: {cmd}', file=sys.stderr)
            return 2
        return tabla[cmd]()
    except FileNotFoundError as e:
        print(f'ERROR: no existe {e}', file=sys.stderr)
        return 2


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
