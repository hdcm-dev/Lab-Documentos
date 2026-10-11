#!/usr/bin/env python3
"""Genera, byte a byte, los archivos de prueba del núcleo N1.

Los fuentes con codificación "rara" (cp1252, UTF-8 con BOM, CRLF) NO se
versionan: se generan en ./work/ (ignorado por git) para que ningún editor ni
ningún `git checkout` los reescriba sin que nos demos cuenta. Así el
experimento siempre parte de los mismos bytes.

Uso:  python3 generar.py            (dentro o fuera de un contenedor)
"""
from pathlib import Path
import shutil

AQUI = Path(__file__).resolve().parent
WORK = AQUI / "work"

# --- Texto lógico (lo que el desarrollador "ve" en su editor) -------------
LITERAL = "Atención: señal recibida en Ñandubay"   # sólo un literal
IDENT = "Çaudal"                                     # identificador no ASCII

CS_MENSAJES = (
    "namespace Lab;\n"
    "public static class Mensajes\n"
    "{\n"
    f'    public const string Texto = "{LITERAL}";\n'
    "    // Literal crudo multilínea: sus finales de línea son los del archivo\n"
    '    public const string Bloque = """\n'
    "        uno\n"
    "        dos\n"
    '        """;\n'
    "}\n"
)

CS_ALARMA = (
    "namespace Lab;\n"
    "public static class Alarma\n"
    "{\n"
    f"    public static int {IDENT} = 1;\n"
    "}\n"
)

CS_PROGRAM = (  # ASCII puro: idéntico en cualquier codificación
    "using Lab;\n"
    "var t = Mensajes.Texto;\n"
    "System.Console.WriteLine($\"Texto  : {t}\");\n"
    "System.Console.WriteLine($\"Largo  : {t.Length} (esperado 36)\");\n"
    "System.Console.WriteLine(\"U+     : \" + string.Join(\" \", "
    "System.Linq.Enumerable.Select(t.Substring(6, 3), c => ((int)c).ToString(\"X4\"))));\n"
    "System.Console.WriteLine($\"Bloque : {Mensajes.Bloque.Length} chars "
    "(LF=7, CRLF=8) contieneCR={Mensajes.Bloque.Contains('\\r')}\");\n"
)

CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>$(TF)</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <!-- Alarma.cs (identificador no ASCII) se compila sólo con -p:ConIdentificador=true -->
  </PropertyGroup>
  <ItemGroup Condition="'$(ConIdentificador)' != 'true'">
    <Compile Remove="Alarma.cs" />
  </ItemGroup>
</Project>
"""

RAZOR = (
    "<p>@Saludo</p>\n"
    "<p>Atención: señal recibida</p>\n"
    "@code {\n"
    '    public string Saludo { get; set; } = "Configuración";\n'
    "}\n"
)
RAZOR_CSPROJ = """<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>$(TF)</TargetFramework>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
"""

JS = (
    "// literal no ASCII\n"
    f'const texto = "{LITERAL}";\n'
    "const bloque = `uno\ndos`;\n"
    "module.exports = { texto, bloque };\n"
)
# Identificador no ASCII en cp1252 (caso L3 de la mesa, ciclo 1): Node y Razor
# corrompen los literales sin error, pero un identificador los hace fallar.
JS_IDENT = (
    "// identificador no ASCII\n"
    "const año = 2026;\n"
    "module.exports = { año };\n"
)
RAZOR_IDENT = (
    f"<p>@{IDENT}</p>\n"
    "@code {\n"
    f"    public int {IDENT} {{get;set;}}\n"
    "}\n"
)
PY = (
    f'texto = "{LITERAL}"\n'
    'bloque = """uno\ndos"""\n'
    "print(repr(texto), len(texto), repr(bloque))\n"
)
PY_DECLARADO = "# -*- coding: cp1252 -*-\n" + PY

BOM = b"\xef\xbb\xbf"


def codificar(texto: str, variante: str, eol: str = "\n") -> bytes:
    texto = texto.replace("\n", eol)
    if variante == "cp1252":
        return texto.encode("cp1252")
    if variante == "utf8":
        return texto.encode("utf-8")
    if variante == "utf8bom":
        return BOM + texto.encode("utf-8")
    raise ValueError(variante)


def escribir(ruta: Path, datos: bytes) -> None:
    ruta.parent.mkdir(parents=True, exist_ok=True)
    ruta.write_bytes(datos)


def main() -> None:
    if WORK.exists():
        shutil.rmtree(WORK)
    for v in ("cp1252", "utf8", "utf8bom"):
        for eol_nombre, eol in (("lf", "\n"), ("crlf", "\r\n")):
            d = WORK / "dotnet" / f"{v}-{eol_nombre}"
            escribir(d / "App.csproj", CSPROJ.encode())
            escribir(d / "Program.cs", CS_PROGRAM.encode("ascii"))
            escribir(d / "Mensajes.cs", codificar(CS_MENSAJES, v, eol))
            escribir(d / "Alarma.cs", codificar(CS_ALARMA, v, eol))
        r = WORK / "razor" / v
        escribir(r / "Lib.csproj", RAZOR_CSPROJ.encode())
        escribir(r / "Aviso.razor", codificar(RAZOR, v))
        escribir(WORK / "node" / f"mod-{v}.js", codificar(JS, v))
        escribir(WORK / "python" / f"mod_{v}.py", codificar(PY, v))
    escribir(WORK / "node" / "mod-ident-cp1252.js", codificar(JS_IDENT, "cp1252"))
    escribir(WORK / "razor" / "cp1252-ident" / "Lib.csproj", RAZOR_CSPROJ.encode())
    escribir(WORK / "razor" / "cp1252-ident" / "Aviso.razor", codificar(RAZOR_IDENT, "cp1252"))
    escribir(WORK / "node" / "mod-utf8-crlf.js", codificar(JS, "utf8", "\r\n"))
    escribir(WORK / "python" / "mod_utf8_crlf.py", codificar(PY, "utf8", "\r\n"))
    escribir(WORK / "python" / "mod_cp1252_declarado.py", codificar(PY_DECLARADO, "cp1252"))
    escribir(WORK / "node" / "esm-utf8bom.mjs", BOM + f'export const texto = "{LITERAL}";\n'.encode())
    # JSON y script con BOM: casos donde el BOM sí rompe
    escribir(WORK / "node" / "config-utf8bom.json", BOM + b'{"ok": true}\n')
    escribir(WORK / "node" / "config-utf8.json", b'{"ok": true}\n')
    escribir(WORK / "shell" / "con-bom.sh", BOM + b"#!/bin/sh\necho hola\n")
    escribir(WORK / "shell" / "sin-bom.sh", b"#!/bin/sh\necho hola\n")
    escribir(WORK / "shell" / "crlf.sh", b"#!/bin/sh\r\necho hola\r\n")
    print(f"Fixtures generados en {WORK}")


if __name__ == "__main__":
    main()
