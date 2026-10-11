#!/usr/bin/env python3
"""Lista de verificación del revisor para cambios de ENTORNO en un PR (N7).

No aprueba ni rechaza: le dice al revisor humano qué mirar con lupa.
  1. Clasifica los archivos tocados por capa del entorno (N0): texto, toolchain,
     manifiesto, lock, fuentes, imagen, gobierno.
  2. Detecta cambios INVISIBLES: el diff existe pero, normalizando BOM, finales
     de línea y codificación, el contenido es el mismo.
  3. Cruza manifiesto y lock: uno sin el otro es sospechoso.

Uso: cambios-de-entorno.py <base> [<head>]      (sólo biblioteca estándar)
"""
import fnmatch
import os
import subprocess
import sys

CAPAS = [  # (capa, patrones sobre el nombre o la ruta, pregunta para el revisor)
    ("politica-texto", [".editorconfig", ".gitattributes"],
     "¿Cambia la política para todos? ¿Viene con su commit de renormalización aparte y con registro en .git-blame-ignore-revs?"),
    ("toolchain", ["global.json", ".nvmrc", ".node-version", ".python-version", ".tool-versions", "mise.toml", "rust-toolchain.toml"],
     "¿Subió la versión a propósito? ¿La imagen de CI/devcontainer y el lock se movieron en el MISMO PR? ¿Hay ADR o nota de la decisión?"),
    ("lock", ["packages.lock.json", "package-lock.json", "npm-shrinkwrap.json", "pnpm-lock.yaml", "yarn.lock", "uv.lock", "poetry.lock", "pylock.toml", "pylock.*.toml", "devcontainer-lock.json"],
     "¿Lo regeneró la herramienta (no a mano)? ¿Cambian sólo los paquetes esperados? ¿Aparecen fuentes o hashes nuevos?"),
    ("manifiesto", ["*.csproj", "*.fsproj", "*.vbproj", "Directory.Packages.props", "Directory.Build.props", "Directory.Build.targets", "package.json", "pyproject.toml", "requirements*.in"],
     "¿Qué dependencia o propiedad de build cambió y por qué? ¿Hay rangos flotantes nuevos (*, ^, >=)?"),
    ("fuentes", ["nuget.config", "NuGet.Config", ".npmrc", "uv.toml", "pip.conf"],
     "¿Aparece un registro nuevo o se quitó el mapeo de fuentes? (riesgo de cadena de suministro)"),
    ("imagen", ["Dockerfile", "Dockerfile.*", "*.Dockerfile", "devcontainer.json", "docker-compose*.yml", "compose*.yaml"],
     "¿La imagen sigue fijada por digest? ¿El SDK/runtime de la imagen coincide con los archivos de toolchain?"),
    ("gobierno", ["CODEOWNERS", "*.github/workflows/*", ".githooks/*", ".pre-commit-config.yaml", "dependabot.yml", "renovate.json", "tools/compuerta.sh"],
     "Cambia la compuerta o quién revisa: ¿lo aprueba quien es dueño de la política (ACT-ARQ/ACT-OPS) y no sólo el autor?"),
]
LOCK_DE = {  # lock -> manifiestos que lo explican
    "packages.lock.json": ["*.csproj", "Directory.Packages.props", "Directory.Build.props", "global.json"],
    "package-lock.json": ["package.json"],
    "uv.lock": ["pyproject.toml"],
}


def git(*a, binario=False):
    r = subprocess.run(["git", *a], capture_output=True, check=False)
    return r.stdout if binario else r.stdout.decode("utf-8", "replace")


def capa_de(ruta):
    nombre = os.path.basename(ruta)
    for capa, patrones, _ in CAPAS:
        if any(fnmatch.fnmatch(nombre, p) or fnmatch.fnmatch(ruta, p) for p in patrones):
            return capa
    return None


def normalizar(b):
    """Devuelve (texto, rasgos) quitando BOM, CRLF y la codificación de origen."""
    rasgos = []
    if b.startswith(b"\xef\xbb\xbf"):
        b = b[3:]; rasgos.append("BOM")
    if b"\r\n" in b:
        b = b.replace(b"\r\n", b"\n"); rasgos.append("CRLF")
    try:
        t = b.decode("utf-8"); rasgos.append("utf-8")
    except UnicodeDecodeError:
        t = b.decode("cp1252", "replace"); rasgos.append("cp1252?")
    return t.rstrip("\n"), rasgos


def main():
    base = sys.argv[1]
    head = sys.argv[2] if len(sys.argv) > 2 else "HEAD"
    # --name-status -z alterna estado y ruta (sin renombres: --no-renames)
    crudo = git("diff", "--name-status", "--no-renames", "-z", f"{base}...{head}").split("\0")
    pares = [(crudo[i], crudo[i + 1]) for i in range(0, len(crudo) - 1, 2)]
    print(f"# Cambios de entorno en {base}...{head}\n")
    tocados = {}
    invisibles = []
    for estado, ruta in pares:
        capa = capa_de(ruta)
        if capa:
            tocados.setdefault(capa, []).append(f"{estado} {ruta}")
        if estado == "M":
            antes = git("show", f"{base}:{ruta}", binario=True)
            despues = git("show", f"{head}:{ruta}", binario=True)
            ta, ra = normalizar(antes)
            td, rd = normalizar(despues)
            if ta == td and antes != despues:
                invisibles.append(f"{ruta}: mismo texto; cambia sólo {sorted(set(ra) ^ set(rd)) or ['salto final']}")
    if not tocados and not invisibles:
        print("Sin cambios de entorno: revisión normal.")
        return 0
    for capa, _, pregunta in CAPAS:
        if capa in tocados:
            print(f"## {capa}")
            for t in tocados[capa]:
                print(f"- `{t}`")
            print(f"- [ ] {pregunta}\n")
    if invisibles:
        print("## cambios invisibles (el diff no muestra texto distinto)")
        for i in invisibles:
            print(f"- `{i}`")
        print("- [ ] ¿Es el commit de normalización acordado? Si no, pedir que se revierta: es ruido que ensucia blame y el próximo merge.\n")
    nombres = {os.path.basename(p) for _, p in pares}
    rutas = [p for _, p in pares]
    print("## coherencia manifiesto ↔ lock")
    hubo = False
    for lock, mans in LOCK_DE.items():
        cambia_lock = lock in nombres
        cambia_man = any(fnmatch.fnmatch(os.path.basename(r), m) for r in rutas for m in mans)
        if cambia_lock and not cambia_man:
            hubo = True
            print(f"- [ ] **{lock} cambió sin su manifiesto**: ¿se regeneró con otra versión de la herramienta o del SDK (ver N3), o se editó a mano?")
        if cambia_man and not cambia_lock and any(os.path.basename(p) == lock for p in git("ls-files").splitlines()):
            hubo = True
            print(f"- [ ] **manifiesto cambió y {lock} no**: si cambió una dependencia, la restauración estricta debería fallar; si la CI pasó, ¿el cambio no toca dependencias o la compuerta no corre?")
    if not hubo:
        print("- coherente: cada lock tocado viene con su manifiesto (o ninguno cambió).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
