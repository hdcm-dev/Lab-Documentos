# verificar-markdown.py — anclas, enlaces, REF y rutas de la guía (modo «guia») o de un árbol de .md (modo «arbol»).
#
#   python3 tools/verificar-markdown.py guia <guía.md> [<excepciones>]
#   python3 tools/verificar-markdown.py arbol <carpeta>...
#
# Excepciones de rutas (ESTA-06): por defecto tools/verificar-markdown.excepciones. Una línea por ruta que la
# guía nombra y que el extractor no puede resolver sola: «<ruta> # <motivo>» (un patrón, una carpeta que crea el
# arnés, una ruta de una fuente externa) o «<ruta> = <ruta completa>» (una ruta relativa a otra carpeta, que
# tiene que existir). Una excepción que ya no aparece en la guía, o cuya ruta completa no existe, se informa.
# Los enlaces que empiezan con «/LAB/» se resuelven desde la raíz del workspace (RAIZ_WORKSPACE o la carpeta
# que contiene «LAB»), como en compuerta-referencias.sh.
import re, sys, os, unicodedata, collections
def slug(h):
    h = re.sub(r'<[^>]+>', '', h).strip().lower()
    h = re.sub(r'[`*_~]', '', h)  # approx GitHub
    h = re.sub(r'\[([^\]]*)\]\([^)]*\)', r'\1', h)
    out = ''.join(c for c in h if c.isalnum() or c in ' -_')
    return out.replace(' ', '-')
def strip_fences(lines):
    res=[]; fence=None
    for i,l in enumerate(lines,1):
        m=re.match(r'^\s*(`{3,}|~{3,})',l)
        if m:
            if fence is None: fence=m.group(1)[0]*len(m.group(1)); res.append((i,'',True)); continue
            elif l.strip().startswith(fence): fence=None; res.append((i,'',True)); continue
        res.append((i,l,fence is not None))
    return res, fence
def leer_excepciones(ruta):
    exc = {}
    if not ruta or not os.path.isfile(ruta): return exc
    for linea in open(ruta, encoding='utf-8'):
        linea = linea.strip()
        if not linea or linea.startswith('#'): continue
        if ' = ' in linea:
            r, _, destino = linea.partition(' = '); exc[r.strip()] = ('=', destino.split('#')[0].strip())
        else:
            r, _, motivo = linea.partition('#'); exc[r.strip()] = ('#', motivo.strip())
    return exc
def check(path, guide=False, root=None, excepciones=None):
    lines=open(path,encoding='utf-8').read().split('\n')
    rows, openf = strip_fences(lines)
    issues=collections.defaultdict(list)
    if openf: issues['fence-abierto'].append('EOF')
    anchors=collections.Counter(); heads=[]; slugc=collections.Counter()
    for i,l,inf in rows:
        if inf: continue
        for a in re.findall(r'<a\s+(?:id|name)="([^"]+)"',l): anchors[a]+=1
        m=re.match(r'^(#{1,6})\s+(.*)$',l)
        if m:
            s=slug(m.group(2)); n=slugc[s]; slugc[s]+=1
            heads.append(s if n==0 else f'{s}-{n}')
    targets=set(anchors)|set(heads)
    for a,c in anchors.items():
        if c>1: issues['ancla-duplicada'].append(a)
    nlinks=0; nrel=0
    for i,l,inf in rows:
        if inf: continue
        t=re.sub(r'`[^`]*`','',l)
        for tgt in re.findall(r'\]\(([^)\s]+)(?:\s+"[^"]*")?\)',t):
            if tgt.startswith('#'):
                nlinks+=1
                if tgt[1:] not in targets: issues['ancla-rota'].append(f'{i}: {tgt}')
            elif not re.match(r'^[a-z]+:',tgt):
                nrel+=1
                p,_,frag=tgt.partition('#')
                from urllib.parse import unquote
                if p.startswith('/LAB/'):
                    # Enlace desde la raíz del workspace (convención del repositorio), como en compuerta-referencias.sh.
                    partes=os.path.abspath(path).split(os.sep)
                    raiz=os.environ.get('RAIZ_WORKSPACE') or (os.sep.join(partes[:partes.index('LAB')]) if 'LAB' in partes else '')
                    fp=os.path.normpath(os.path.join(raiz,unquote(p).lstrip('/')))
                else:
                    fp=os.path.normpath(os.path.join(os.path.dirname(path),unquote(p)))
                if p and not os.path.exists(fp): issues['enlace-relativo-roto'].append(f'{i}: {tgt}')
                elif frag and fp.endswith('.md') and os.path.isfile(fp):
                    r2,_=strip_fences(open(fp,encoding='utf-8').read().split('\n'))
                    ts=set(); sc=collections.Counter()
                    for _,ll,ii in r2:
                        if ii: continue
                        ts.update(re.findall(r'<a\s+(?:id|name)="([^"]+)"',ll))
                        mm=re.match(r'^(#{1,6})\s+(.*)$',ll)
                        if mm:
                            s=slug(mm.group(2)); n=sc[s]; sc[s]+=1; ts.add(s if n==0 else f'{s}-{n}')
                    if frag not in ts: issues['ancla-rota-externa'].append(f'{i}: {tgt}')
    info={'encabezados':len(heads),'anclas':sum(anchors.values()),'enlaces-internos':nlinks,'enlaces-relativos':nrel}
    if guide:
        text='\n'.join(l for _,l,inf in rows)
        defs=collections.Counter(re.findall(r'^- \[(REF-\d+)\]',text,re.M))
        body=re.sub(r'^- \[REF-\d+\]','',text,flags=re.M)
        cited=set(re.findall(r'\bREF-\d+\b',body))
        # rango REF-a a REF-b
        for a,b in re.findall(r'REF-(\d+) a REF-(\d+)',body):
            pass
        info['REF-definidas']=len(defs); info['REF-citadas']=len(cited)
        for r,c in defs.items():
            if c>1: issues['REF-duplicada'].append(r)
        for r in sorted(cited-set(defs)): issues['REF-sin-entrada'].append(r)
        for r in sorted(set(defs)-cited): issues['REF-no-citada'].append(r)
        info['sin-titulo-registrado']=len(re.findall(r'^- \[REF-\d+\].*sin título registrado',text,re.M))
        paths=set()
        for i,l,inf in rows:
            for p in re.findall(r'(?<![\w/.-])((?:Examples|Demos|tools|Compuertas)/[\w./{}*-]*)',l):
                paths.add(p.rstrip('.,;:)'))
        info['rutas']=len(paths)
        exc = excepciones or {}
        usadas = 0
        for p in sorted(paths):
            if any(ch in p for ch in '*{}'): continue
            if os.path.exists(os.path.join(root,p)): continue
            if p in exc:
                usadas += 1
                tipo, valor = exc[p]
                if tipo == '=' and not os.path.exists(os.path.join(root, valor)): issues['excepcion-sin-destino'].append(f'{p} = {valor}')
                continue
            issues['ruta-inexistente'].append(p)
        info['rutas-exceptuadas'] = usadas
        for p in sorted(set(exc) - paths): issues['excepcion-vencida'].append(p)
    return info, issues
if __name__=='__main__':
    mode=sys.argv[1]
    if mode=='guia':
        g=sys.argv[2]
        ex=sys.argv[3] if len(sys.argv)>3 else os.path.join(os.path.dirname(os.path.abspath(__file__)),'verificar-markdown.excepciones')
        info,iss=check(g,True,os.path.dirname(os.path.abspath(g)),leer_excepciones(ex))
        print('\n'.join(f'{k}: {v}' for k,v in info.items()))
        for k,v in iss.items():
            print(f'== {k}: {len(v)}'); [print('    '+x) for x in v]
        sys.exit(1 if iss else 0)
    else:
        tot=collections.Counter(); bad=0; n=0
        for base in sys.argv[2:]:
            for d,_,fs in os.walk(base):
                if any(x in d.split(os.sep) for x in ('bin','obj','.git')): continue
                for f in fs:
                    if not f.endswith('.md'): continue
                    n+=1; p=os.path.join(d,f); info,iss=check(p)
                    for k in ('enlaces-internos','enlaces-relativos'): tot[k]+=info[k]
                    for k,v in iss.items():
                        bad+=len(v)
                        for x in v: print(f'{k}\t{os.path.relpath(p,base)}\t{x}')
        print(f'archivos .md: {n}; enlaces internos: {tot["enlaces-internos"]}; enlaces relativos: {tot["enlaces-relativos"]}; marcas: {bad}')
        sys.exit(1 if bad else 0)
