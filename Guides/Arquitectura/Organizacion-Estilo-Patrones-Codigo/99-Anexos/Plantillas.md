---
doc_id: ANEXO-PLANTILLAS
doc_type: anexo
title: Plantillas comentadas
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-19
audience: [humano, agente]
traces: [TEM-SLN, TEM-SDK, TEM-BUILD, TEM-AUTO, ANEXO-REFERENCIAS]
---

# Plantillas comentadas — `ANEXO-PLANTILLAS`

## Resumen ejecutivo

Los archivos que hay que escribir una vez al arrancar un repositorio .NET y que después casi no se tocan. Cada plantilla lleva comentarios que explican qué guía la decisión de cada campo, de modo que se pueda adaptar en lugar de copiar a ciegas.

Las plantillas expresan el criterio de esta guía donde no hay norma. Lo normativo se identifica con la referencia correspondiente de [`ANEXO-REFERENCIAS`](Referencias.md); el resto es punto de partida razonable, no obligación.

---

## 1. Esqueleto de repositorio

Disposición de partida para una solución .NET con una unidad desplegable. Convención de facto (`F-01`), no especificación.

```text
mi-sistema/
├── .editorconfig                  # Reglas de estilo — TEM-AUTO
├── .gitattributes                 # Normalización de fin de línea
├── .git-blame-ignore-revs         # Commits de formato a ignorar en blame — ESC-3
├── .gitignore
├── Directory.Build.props          # Propiedades comunes a todos los proyectos — N-09
├── Directory.Packages.props       # Versiones centralizadas de NuGet — N-08
├── global.json                    # Versión fija del SDK — N-13
├── nuget.config                   # Orígenes de paquetes (si no son los públicos)
├── MiSistema.slnx                 # Solución en formato XML — N-11
├── README.md
├── docs/
│   └── adr/                       # Registros de decisión de arquitectura
├── src/
│   └── MiSistema.Web/
│       └── MiSistema.Web.csproj
├── tests/
│   ├── MiSistema.Web.Tests/       # Unitarias
│   └── MiSistema.Web.Integracion/ # Integración
└── perf/
    └── MiSistema.Web.Benchmarks/  # Opcional
```

Tres decisiones a tomar conscientemente, y conviene saber que solo la primera tiene respaldo firme.

Que el código productivo viva bajo `src/` y la infraestructura de build bajo `eng/` es unánime; que la carpeta de tests esté en la raíz y en plural, no. La evidencia de los tres repositorios de referencia está en [`TEM-SLN`](../20-Organizacion-de-Soluciones/Estructura-de-Solucion.md). Lo que importa acá: elegir `tests/` en la raíz es una decisión de equipo perfectamente razonable —es la disposición más simple de explicar— pero es una decisión, no una convención heredada.

La tercera es si los proyectos de `perf/` e integración entran en la solución. Dejarlos afuera aligera el build a costa de que `dotnet build` sobre la solución no los compile ([`TEM-SLN`](../20-Organizacion-de-Soluciones/Estructura-de-Solucion.md)).

---

## 2. `Directory.Build.props`

Propiedades heredadas por todos los proyectos bajo el directorio. Normativo: `N-09`.

```xml
<Project>

  <!--
    Propiedades comunes a toda la solución. MSBuild busca este archivo hacia
    arriba desde cada .csproj y lo importa ANTES del contenido del proyecto,
    de modo que un .csproj puede sobrescribir cualquier valor de acá.
  -->
  <PropertyGroup>
    <!-- Un solo marco de destino para toda la solución evita divergencias. -->
    <TargetFramework>net10.0</TargetFramework>

    <!-- Anulabilidad: no es estilo, es corrección. Activar desde el día uno;
         activarlo después obliga a revisar cada firma del sistema. -->
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- Análisis estático del runtime. AnalysisMode Default conserva el
         conjunto que trae el SDK; subir a Recommended o All convierte
         convenciones idiomáticas en avisos nuevos y suele generar ruido
         que nadie atiende. -->
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>Default</AnalysisMode>

    <!-- Verifica en compilación las reglas IDExxxx declaradas en .editorconfig.
         Sin .editorconfig solo fuerza los valores por defecto del SDK. -->
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>

    <!-- Los avisos NO rompen el build local: la fricción al iterar es real.
         La canalización de integración continua fuerza el rigor con `dotnet build -warnaserror`,
         de modo que un aviso nuevo bloquea la integración sin entorpecer
         el trabajo diario. Decisión de ACT-05. -->
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

Lo que **no** conviene poner acá: `PackageReference` que no aplique a todos los proyectos, y cualquier propiedad específica de un tipo de proyecto. Un paquete declarado en `Directory.Build.props` entra en los proyectos de prueba y en los de benchmark también.

---

## 3. `Directory.Packages.props`

Gestión centralizada de versiones. Normativo: `N-08`.

```xml
<Project>

  <PropertyGroup>
    <!-- Habilita el modo centralizado. Con esto activo, los .csproj declaran
         <PackageReference Include="..." /> SIN atributo Version. -->
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>

    <!-- Ancla también las dependencias transitivas a la versión declarada acá.
         Es el mecanismo para forzar una versión parcheada de un paquete que
         llega indirectamente, sin agregar una referencia directa artificial. -->
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.9" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.9" />
  </ItemGroup>

  <!-- Paquetes de prueba: agrupados aparte por legibilidad. La separación es
       organizativa; MSBuild no distingue los ItemGroup. -->
  <ItemGroup>
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

</Project>
```

En el `.csproj` la referencia queda sin versión:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
</ItemGroup>
```

Cuando un proyecto necesita una versión distinta —caso raro y que conviene justificar— existe `VersionOverride`:

```xml
<PackageReference Include="xunit" VersionOverride="2.9.0" />
```

El beneficio concreto aparece al actualizar. Sin gestión centralizada, subir EF Core obliga a editar cada `.csproj` que lo referencie y a verificar que ninguno quedó atrás; con ella, se edita una línea.

---

## 4. `global.json`

Fija la versión del SDK para que el build sea reproducible entre máquinas y en la canalización.

```json
{
  "sdk": {
    "version": "10.0.300",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

`rollForward` controla qué pasa cuando la versión exacta no está instalada. `latestFeature` acepta la última banda de características de la misma versión mayor y menor, que es el equilibrio habitual entre reproducibilidad y no obligar a cada desarrollador a instalar un parche puntual. `disable` exige la versión exacta.

---

## 5. `.editorconfig`

El mecanismo de las convenciones de estilo. Normativo: `N-07`. Fragmento de partida; el conjunto completo de opciones está en la referencia.

```ini
# Detiene la búsqueda hacia arriba: este es el archivo raíz del repositorio.
root = true

[*]
indent_style = space
end_of_line = lf
charset = utf-8
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csx}]
indent_size = 4
max_line_length = 120

# ── Organización de using ──────────────────────────────────────────────
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false
csharp_using_directive_placement = outside_namespace:warning

# ── Estilo de llaves: Allman, la convención de C# (N-06, F-06) ─────────
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true

# Llaves siempre, incluso en bloques de una sola sentencia. Evita la clase
# de error que se introduce al agregar una segunda línea a un if sin llaves.
csharp_prefer_braces = true:warning

# ── var: el criterio de N-06 es usarlo cuando el tipo es evidente ──────
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_elsewhere = false:suggestion

# ── Nomenclatura: campos privados con _ (F-02) ─────────────────────────
dotnet_naming_rule.campos_privados_con_guion_bajo.severity = warning
dotnet_naming_rule.campos_privados_con_guion_bajo.symbols = campo_privado
dotnet_naming_rule.campos_privados_con_guion_bajo.style = prefijo_guion_bajo

dotnet_naming_symbols.campo_privado.applicable_kinds = field
dotnet_naming_symbols.campo_privado.applicable_accessibilities = private
dotnet_naming_symbols.campo_privado.required_modifiers =

dotnet_naming_style.prefijo_guion_bajo.required_prefix = _
dotnet_naming_style.prefijo_guion_bajo.capitalization = camel_case

# ── Nomenclatura: tipos y miembros públicos en PascalCase (N-02) ───────
dotnet_naming_rule.tipos_en_pascal.severity = warning
dotnet_naming_rule.tipos_en_pascal.symbols = tipos
dotnet_naming_rule.tipos_en_pascal.style = pascal

dotnet_naming_symbols.tipos.applicable_kinds = class,struct,interface,enum,delegate
dotnet_naming_style.pascal.capitalization = pascal_case

# ── Modernización del lenguaje ─────────────────────────────────────────
csharp_style_namespace_declarations = file_scoped:warning
csharp_style_prefer_primary_constructors = true:suggestion
dotnet_style_require_accessibility_modifiers = for_non_interface_members:warning

# ── Reglas relajadas en pruebas ────────────────────────────────────────
# Los nombres de test estilo BDD usan guiones bajos deliberadamente.
[tests/**/*.cs]
dotnet_diagnostic.CA1707.severity = none
```

La sección final ilustra algo que se aprovecha poco: `.editorconfig` admite reglas por ruta, de modo que las convenciones de prueba pueden diferir de las del producto sin desactivarlas globalmente.

---

## 6. `.csproj` de aplicación web

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <!--
    TargetFramework, Nullable e ImplicitUsings vienen de Directory.Build.props.
    Repetirlos acá crea dos lugares donde cambiarlos.
  -->
  <PropertyGroup>
    <UserSecretsId>...</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <!-- Sin Version: la fija Directory.Packages.props -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />

    <!-- PrivateAssets=all: paquete de tiempo de diseño; no fluye como
         dependencia transitiva al consumidor ni al artefacto publicado. -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

## 7. `.csproj` de proyecto de prueba

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Un proyecto de prueba nunca se empaqueta. -->
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>

  <!-- Using global para no repetir la directiva en cada archivo de prueba. -->
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\MiSistema.Web\MiSistema.Web.csproj" />
  </ItemGroup>

</Project>
```

---

## 8. `.csproj` de biblioteca publicable (`CTX-3`)

Los campos adicionales existen porque la biblioteca se publica y su superficie es contrato.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Multi-target: amplía el alcance de consumidores, a costa de
         compilación condicional cuando las APIs difieren entre versiones. -->
    <TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>

    <PackageId>MiEmpresa.Reservas.Cliente</PackageId>
    <Description>Cliente del servicio de reserva de salas.</Description>
    <Authors>Mi Empresa</Authors>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>

    <!-- Documentación XML: en una biblioteca los comentarios de los miembros
         públicos llegan al consumidor por IntelliSense. -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- En una biblioteca los avisos SÍ rompen el build: un aviso de API
         pública mal formada es un problema que se publica. -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

---

## 9. `.git-blame-ignore-revs`

Para `ESC-3`. Los commits listados se saltan al atribuir autoría, de modo que una normalización masiva no sepulte la historia real.

```text
# Normalización de formato con dotnet format — sin cambios funcionales
# 2026-03-14
a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0

# Migración a namespaces con ámbito de archivo
# 2026-03-15
b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1
```

Requiere configurar el repositorio para que la herramienta lo use:

```bash
git config blame.ignoreRevsFile .git-blame-ignore-revs
```

El archivo solo sirve si los commits que lista **no contienen ningún cambio funcional**. Un commit mixto de formato y lógica no se puede ignorar sin ocultar la lógica.

---

## 10. `.gitattributes`

```text
* text=auto eol=lf
*.cs text diff=csharp
*.csproj text
*.sln text eol=crlf
*.slnx text
*.png binary
*.jpg binary
```

`diff=csharp` mejora la salida de diferencias al identificar el contexto de método en los encabezados de bloque.
