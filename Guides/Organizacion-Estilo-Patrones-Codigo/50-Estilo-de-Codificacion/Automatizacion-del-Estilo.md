---
doc_id: TEM-AUTO
doc_type: tema
title: Automatización del estilo
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-EST, TEM-FORMATO, TEM-LENG, TEM-BUILD, FAM-NOM, ANEXO-PLANTILLAS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Automatización del estilo — `TEM-AUTO`

## Resumen ejecutivo

Una convención que depende de que alguien la recuerde no es una convención: es una aspiración con nombre. La pregunta que `ACT-03` debe hacerse ante cada regla que fija —¿esto lo verifica una herramienta o depende de un humano?— tiene una única respuesta operativa en .NET, y es un archivo `.editorconfig` acompañado de las propiedades de MSBuild que lo hacen valer en compilación.

Ese es el contenido de este documento. Cómo se declara una regla, qué familias de reglas existen y con qué prefijo se identifican, qué significa cada severidad, y —la decisión que más consecuencias tiene sobre el trabajo diario— dónde se verifica cada cosa: si el aviso rompe el build local del desarrollador o solo bloquea la integración. La respuesta que esta guía recomienda es asimétrica a propósito: severidad baja donde se itera, severidad de bloqueo donde se integra.

También es el documento donde `ESC-3` se resuelve. Normalizar código existente es fácil de ejecutar y difícil de integrar; la secuencia de commits dedicados, `.git-blame-ignore-revs` y activación de la regla en el mismo cambio es lo que separa una normalización que dura de una que hay que repetir en seis meses.

Le sirve sobre todo a `ACT-05`, que es quien decide dónde y con qué severidad se verifica, y a `ACT-03`, que decide qué se verifica. Esa división de trabajo es la que ordena el documento.

---

## Definición

### Qué es

El conjunto de mecanismos que convierten una convención escrita en una verificación automática: el archivo `.editorconfig` donde se declaran las reglas, los analizadores de Roslyn que las evalúan, las propiedades de MSBuild que los activan, la herramienta `dotnet format` que aplica las correcciones, y la compuerta de integración continua que impide que entre código que no cumple.

### Qué problema resuelve

Traslada el cumplimiento de convenciones desde la memoria humana hacia el compilador. El beneficio directo es que el código cumple; el indirecto, y probablemente mayor, es que **las revisiones de código dejan de hablar de estilo**. `ACT-04` no escribe «faltó ordenar los `using`» porque el archivo no habría compilado en la canalización con los `using` desordenados, y puede dedicar su atención a lo que ninguna herramienta ve.

Hay un tercer efecto que se nota al incorporar gente. Un desarrollador nuevo que abre el repositorio y empieza a escribir produce código con el estilo del equipo sin haber leído ningún documento, porque el editor lee el `.editorconfig` y aplica las reglas al guardar. La convención se transmite por la herramienta y no por la tradición oral.

### Qué NO es, y con qué se lo confunde

**No decide cuál es la convención.** `ACT-05` configura el mecanismo; `ACT-03` decide la regla. Confundir ambos roles produce el caso típico donde el ingeniero de plataforma activa `AnalysisMode=All`, aparecen cuatro mil avisos, y el equipo decide que los analizadores son un estorbo.

**No es análisis de seguridad.** Los analizadores de calidad `CAxxxx` incluyen reglas de seguridad, pero un `.editorconfig` bien configurado no reemplaza a un análisis de composición de dependencias ni a una revisión de seguridad. Son planos distintos.

**No es formato solamente.** El mismo archivo declara reglas de estilo, reglas de calidad y convenciones de nomenclatura. Tratarlas todas con la misma severidad es el error de configuración más frecuente, y el documento vuelve sobre eso más de una vez.

**No garantiza que el código sea bueno.** Un archivo que cumple las mil reglas del `.editorconfig` puede ser un método de cuatrocientas líneas con seis responsabilidades. La automatización elimina una clase de problemas —la más barata— y deja intactas todas las demás.

---

## `.editorconfig`

Es **el** mecanismo. Un archivo de texto plano, en formato INI, que declara las reglas por patrón de archivo y que leen tanto los editores como el compilador. Está normado en `N-07`, que documenta el catálogo completo de opciones de estilo configurables.

### Cómo funciona la herencia

La resolución es **por directorio y hacia arriba**. Al procesar un archivo, la herramienta busca `.editorconfig` en su carpeta, luego en la carpeta padre, y así sucesivamente hasta la raíz del disco o hasta encontrar uno que declare `root = true`. Las declaraciones de los archivos más cercanos al archivo procesado ganan sobre las de los más lejanos.

```
repositorio/
├── .editorconfig            root = true      ← reglas generales del repositorio
├── src/
│   └── Reservas.Servicio/
│       └── Program.cs                        ← hereda del de la raíz
└── tests/
    ├── .editorconfig                         ← relaja reglas solo para pruebas
    └── Reservas.Pruebas/
        └── ReservaTests.cs                   ← raíz + el de tests/
```

`root = true` en el archivo de la raíz es obligatorio en la práctica, y su omisión es un defecto silencioso: sin él, la búsqueda sigue subiendo por el sistema de archivos y puede recoger un `.editorconfig` del directorio del usuario que un desarrollador tiene y otro no. El resultado son reglas que aplican en una máquina y no en otra, que es exactamente lo contrario del propósito del archivo.

El `.editorconfig` de `tests/` del ejemplo es un patrón útil y no una concesión. El código de prueba tiene convenciones idiomáticas propias —nombres de método con guiones bajos al estilo BDD, campos públicos en clases de datos de prueba, métodos estáticos auxiliares— que en código de producción serían defectos y que en pruebas son legibilidad. Relajar reglas concretas en una sección específica es más honesto que suprimirlas en todo el repositorio.

### Secciones

Las secciones son patrones glob. Se acumulan: un archivo `.cs` recibe lo de `[*]`, lo de `[*.cs]` y lo de cualquier patrón más específico que lo alcance.

```ini
root = true

# Aplica a todo archivo del repositorio.
[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space

# C#: cuatro espacios, según N-06.
[*.cs]
indent_size = 4

# Marcado y datos: dos espacios, convención de esos formatos.
[*.{json,yml,yaml,razor,cshtml,csproj,props,targets}]
indent_size = 2

# Archivos generados: sin verificación de estilo.
[*.{Designer.cs,g.cs,g.i.cs}]
generated_code = true
```

---

## Las tres familias de reglas

Un `.editorconfig` de .NET declara tres cosas distintas que se distinguen por su prefijo, y confundirlas produce configuraciones incoherentes.

| Familia | Prefijo | Qué evalúa | Ejemplo |
|---------|---------|------------|---------|
| Reglas de estilo | `IDExxxx` | Cómo está escrito el código | `IDE0005` `using` innecesario |
| Reglas de calidad | `CAxxxx` | Qué hace el código y qué riesgo tiene | `CA2007` falta `ConfigureAwait` |
| Opciones | `dotnet_` / `csharp_` | Qué prefiere el equipo cuando hay alternativas | `csharp_style_var_when_type_is_apparent` |

La distinción que importa es entre las dos primeras. Una regla `IDExxxx` no señala un defecto: señala una desviación de una preferencia. Una regla `CAxxxx` puede señalar un defecto real —una comparación de cadenas dependiente de la cultura, un `IDisposable` no liberado, un `async void`—. Configurarlas con la misma severidad hace que el aviso importante se pierda entre los cosméticos, y el equipo termina ignorando todos.

### Reglas de estilo `IDExxxx`

Las evalúa el analizador de estilo del SDK. Se activan en compilación mediante `EnforceCodeStyleInBuild`, y sin esa propiedad solo las ve el editor.

```ini
[*.cs]
# `using` innecesario. De las de mejor relación entre costo y beneficio.
dotnet_diagnostic.IDE0005.severity = warning

# Modificador de accesibilidad explícito: nada de `class Foo` sin `internal`.
dotnet_diagnostic.IDE0040.severity = warning

# Orden de modificadores: public static readonly, no static public readonly.
dotnet_diagnostic.IDE0036.severity = warning

# Namespace con ámbito de archivo.
dotnet_diagnostic.IDE0161.severity = warning

# Nombres de miembro: la regla que verifica las convenciones dotnet_naming_*.
dotnet_diagnostic.IDE1006.severity = warning
```

`IDE0005` tiene una particularidad operativa que sorprende a más de un equipo: para que se aplique en compilación hace falta que el proyecto genere archivo de documentación (`GenerateDocumentationFile`), porque el analizador necesita el paso semántico completo para saber si un `using` se usa. Es un detalle de implementación y no un capricho, y explica el caso frecuente de la regla que funciona en el editor y no en la canalización.

### Reglas de calidad `CAxxxx`

Provienen de los analizadores de .NET, incluidos en el SDK desde .NET 5. Se activan con `EnableNETAnalyzers` —que está en `true` por defecto en proyectos que apuntan a .NET 5 o posterior— y el conjunto activo lo determina `AnalysisMode`.

```ini
[*.cs]
# Falta ConfigureAwait: correcto en CTX-3, ruido en ASP.NET Core.
dotnet_diagnostic.CA2007.severity = none

# Comparación de cadenas sin StringComparison explícito. Defecto real.
dotnet_diagnostic.CA1310.severity = warning

# Tipo con IDisposable no liberado.
dotnet_diagnostic.CA2000.severity = warning
```

`CA2007` es el ejemplo que mejor ilustra por qué la configuración de calidad depende del contexto y no puede copiarse entre repositorios. En una biblioteca (`CTX-3`) esa regla previene un problema real; en una aplicación ASP.NET Core (`CTX-1`, `CTX-2`) señala algo que no tiene efecto, y dejarla activa produce cientos de avisos que hay que ignorar. El razonamiento completo está en [`TEM-LENG`](Convenciones-de-Lenguaje.md).

### Opciones `dotnet_` y `csharp_`

No son diagnósticos: son las preferencias que los diagnósticos consultan para decidir qué es correcto. Las que empiezan con `dotnet_` aplican a C# y a Visual Basic; las que empiezan con `csharp_`, solo a C#.

```ini
[*.cs]
# Organización de using — TEM-FORMATO
dotnet_sort_system_directives_first = true
csharp_using_directive_placement = outside_namespace:warning

# Llaves Allman — N-06, F-06
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_prefer_braces = true:warning

# var — N-06, criterio del tipo evidente
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion

# Namespaces y using — TEM-LENG
csharp_style_namespace_declarations = file_scoped:warning
csharp_prefer_simple_using_statement = true:suggestion

# Modificadores
dotnet_style_require_accessibility_modifiers = for_non_interface_members:warning
csharp_preferred_modifier_order = public,private,protected,internal,file,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,required,volatile,async:warning
```

La sintaxis `valor:severidad` es un atajo heredado que declara la preferencia y su severidad en una línea. La forma equivalente y más explícita separa ambas cosas —la opción por un lado y `dotnet_diagnostic.IDExxxx.severity` por otro—; conviven, y mezclarlas en el mismo archivo no rompe nada aunque no ayude a leerlo.

### Las convenciones de nomenclatura `dotnet_naming_*`

Es la familia de opciones con la sintaxis más engorrosa, y por eso la que más se copia sin entender. Una convención de nombrado no se declara en una línea: se arma con **tres** entradas que se referencian entre sí por un identificador que el autor inventa.

- `dotnet_naming_symbols.<nombre>` — a qué símbolos aplica: `applicable_kinds` (campo, método, tipo…), `applicable_accessibilities` y `required_modifiers`.
- `dotnet_naming_style.<nombre>` — qué forma debe tener el nombre: `capitalization`, `required_prefix`, `required_suffix`, `word_separator`.
- `dotnet_naming_rule.<nombre>` — la regla que ata un conjunto de símbolos a un estilo, más su `severity`.

```ini
[*.cs]
# Campos privados con guion bajo (F-02): los tres bloques de una misma regla.
dotnet_naming_rule.campos_privados_con_guion_bajo.severity = warning
dotnet_naming_rule.campos_privados_con_guion_bajo.symbols = campo_privado
dotnet_naming_rule.campos_privados_con_guion_bajo.style = prefijo_guion_bajo

dotnet_naming_symbols.campo_privado.applicable_kinds = field
dotnet_naming_symbols.campo_privado.applicable_accessibilities = private

dotnet_naming_style.prefijo_guion_bajo.required_prefix = _
dotnet_naming_style.prefijo_guion_bajo.capitalization = camel_case
```

Dos cosas que conviene retener. Los identificadores intermedios —`campo_privado`, `prefijo_guion_bajo`— son arbitrarios y solo sirven para enlazar las tres entradas; un error de tipeo en uno de ellos no produce ningún diagnóstico, simplemente hace que la regla no exista. Y la severidad de todas las reglas de esta familia la reporta un único diagnóstico, `IDE1006`: silenciarlo apaga de golpe todas las convenciones de nombrado declaradas, que es exactamente lo que hace la sección `[tests/**/*.cs]` del anexo, y de forma deliberada.

El conjunto completo —con la regla `tipos_en_pascal` para tipos y miembros públicos (`N-02`)— está en [`ANEXO-PLANTILLAS`](../99-Anexos/Plantillas.md). Qué convención corresponde a cada símbolo lo decide [`FAM-NOM`](../40-Nomenclatura/README.md), no este documento.

---

## Severidad

Cinco valores, y la diferencia entre los dos primeros es la que más se malinterpreta.

| Severidad | En el editor | En compilación | Uso típico |
|-----------|--------------|----------------|------------|
| `none` | No se evalúa | Nada | Desactivar una regla que no aplica |
| `silent` | No se muestra, pero la corrección rápida está disponible | Nada | Regla opcional que se quiere ofrecer sin insistir |
| `suggestion` | Subrayado tenue, aparece en la lista de mensajes | Mensaje informativo | Preferencias donde el juicio humano decide |
| `warning` | Subrayado ondulado | **Aviso** | Reglas que el equipo espera que se cumplan |
| `error` | Subrayado rojo | **Error, no compila** | Reglas cuyo incumplimiento es un defecto |

`none` desactiva la evaluación por completo, con el beneficio secundario de no gastar tiempo de compilación. `silent` la evalúa pero no la reporta: la regla no molesta y la corrección automática sigue estando en el menú del editor.

### La decisión de `ACT-05`: local permisivo, canalización estricta

Es la decisión con más efecto sobre el trabajo diario de todo este documento, y no se toma en el `.editorconfig` sino en las propiedades de compilación.

El razonamiento: una regla que rompe el build local castiga la iteración. Un desarrollador que está probando una hipótesis y tiene un `using` de más no puede ejecutar nada hasta borrarlo, y ese costo se paga decenas de veces por día a cambio de nada, porque ese código no va a entrar así de todos modos. La misma regla como error solo en integración continua no interrumpe a nadie y bloquea igual la entrada.

```
Local:         dotnet build            → los avisos se muestran y no rompen
Canalización:  dotnet build -warnaserror → un aviso nuevo bloquea la integración
```

**Esta guía recomienda** esa asimetría como patrón por defecto, con una excepción: las reglas cuyo incumplimiento es un defecto de comportamiento y no de estilo —`async void` fuera de un manejador es el caso canónico— conviene declararlas `error` en el `.editorconfig`, de modo que rompan también en local. La distinción sigue el mismo eje que separa `IDExxxx` de `CAxxxx`.

Lo que no funciona es `TreatWarningsAsErrors` en `Directory.Build.props` sin condición. Rompe la iteración local sin comprar nada que la compuerta de la canalización no compre igual, y el resultado predecible es que alguien lo desactive con una variable de entorno y el equipo pierda la verificación entera.

---

## Aplicar y verificar

### `dotnet format`

Aplica las correcciones automáticas de las reglas declaradas. Es la herramienta de `ESC-3` y la que hace barata la normalización.

```bash
# Aplica todo lo corregible automáticamente.
dotnet format

# Solo formato de espacios en blanco, sin tocar estilo ni analizadores.
dotnet format whitespace

# Solo las reglas de estilo IDExxxx.
dotnet format style

# Solo las reglas de calidad CAxxxx que tengan corrección automática.
dotnet format analyzers

# No modifica: falla si hay algo que corregir. Útil como compuerta.
dotnet format --verify-no-changes
```

La separación en subcomandos es la que permite la normalización por etapas que `ESC-3` recomienda: `whitespace` primero, que es puramente mecánico y no puede romper nada; `style` después; `analyzers` al final, porque algunas correcciones automáticas de reglas `CAxxxx` cambian comportamiento y requieren revisión real.

`--verify-no-changes` como paso de la canalización es una alternativa a `-warnaserror` para las reglas que `dotnet format` conoce. Tiene la ventaja de un mensaje de error más claro —dice exactamente qué archivos hay que formatear— y la desventaja de ser un paso más y de no cubrir las reglas sin corrección automática.

### `EnforceCodeStyleInBuild`

Sin esta propiedad, las reglas `IDExxxx` solo existen en el editor. El compilador no las evalúa, la compilación pasa, y la canalización no detecta nada.

```xml
<PropertyGroup>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

Va en `Directory.Build.props` para que aplique a toda la solución desde un único lugar. El archivo y su mecánica de herencia se tratan en [`TEM-BUILD`](../20-Organizacion-de-Soluciones/Build-Compartido.md); acá interesa qué hace, no dónde vive.

---

## Analizadores

### Las propiedades

```xml
<PropertyGroup>
  <!-- Activa los analizadores de calidad CAxxxx del SDK. -->
  <EnableNETAnalyzers>true</EnableNETAnalyzers>

  <!-- Versión del conjunto de reglas. `latest` toma las del SDK instalado. -->
  <AnalysisLevel>latest</AnalysisLevel>

  <!-- Qué subconjunto de reglas se activa. -->
  <AnalysisMode>Default</AnalysisMode>

  <!-- Hace valer las reglas IDExxxx en compilación. -->
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

`EnableNETAnalyzers` está en `true` por defecto para proyectos que apuntan a .NET 5 o posterior; declararlo explícitamente sirve como documentación y cubre proyectos con destino múltiple.

`AnalysisLevel` fija a qué versión del conjunto de reglas se ajusta el proyecto. `latest` toma las del SDK instalado, lo cual significa que actualizar el SDK puede traer reglas nuevas y por lo tanto avisos nuevos. Fijar un número —`8.0`, `9.0`— vuelve reproducible el conjunto a costa de quedarse atrás. La decisión depende de cuánto le cuesta al equipo un lote de avisos nuevos el día que alguien actualiza el SDK.

### `AnalysisMode` y su compromiso real

Cuatro valores, ordenados por cantidad de reglas activas:

| Valor | Qué activa |
|-------|------------|
| `Minimum` | Solo las reglas más críticas, casi todas como error |
| `Default` | El conjunto que el SDK trae activo, mayormente como aviso |
| `Recommended` | Un conjunto amplio, con muchas reglas de diseño y rendimiento |
| `All` | Todas las reglas disponibles, incluidas las mutuamente contradictorias |

El compromiso no es «más reglas es mejor». Subir de `Default` a `Recommended` en un repositorio existente produce típicamente cientos o miles de avisos de golpe, y la reacción del equipo ante ese volumen es predecible: se ignoran en masa, y con ellos se ignoran los pocos que importaban. Un conjunto de avisos que nadie mira vale menos que ningún conjunto, porque además consume tiempo de compilación y da una falsa sensación de cobertura.

`All` tiene además un problema propio: incluye reglas que se contradicen entre sí, porque el catálogo contiene alternativas para equipos con criterios distintos. Un repositorio en `All` necesita una lista larga de supresiones solo para poder compilar, y esa lista es lo que hay que mantener después.

**Esta guía recomienda** `Default` como punto de partida, con elevación selectiva: subir a `warning` o `error` las reglas concretas que al equipo le importan, en lugar de subir el modo entero. Es más trabajo de configuración inicial y muchísimo menos ruido permanente. Para un repositorio nuevo (`ESC-1`) `Recommended` es defendible porque no hay código previo que produzca el lote inicial de avisos.

---

## Ejemplos concretos

El `Directory.Build.props` del sistema de reserva de salas —ejemplo sintético— implementa el reparto asimétrico que este documento recomienda. Lo que interesa acá no es el archivo entero —el mecanismo de herencia está en [`TEM-BUILD`](../20-Organizacion-de-Soluciones/Build-Compartido.md) y la versión copiable en [`ANEXO-PLANTILLAS`](../99-Anexos/Plantillas.md)— sino que el motivo esté escrito donde alguien lo va a encontrar:

```xml
<PropertyGroup>
  <!--
    Modo Default: conserva la línea base de cero avisos y el conjunto de reglas que
    ya viene activo por el SDK. No se sube a Recommended/All porque eso convertiría
    convenciones idiomáticas del código de prueba (nombres de método con guion bajo
    al estilo BDD, auxiliares estáticos) en avisos nuevos, y el criterio de
    aceptación del equipo exige "sin avisos nuevos".
    Los avisos NO se tratan como error en el build local, para no agregar fricción
    mientras se itera; la canalización los fuerza con `dotnet build -warnaserror`.
  -->
  <AnalysisMode>Default</AnalysisMode>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

La compuerta vive en el flujo de trabajo de integración, en un paso que ejecuta `dotnet build -c Release --no-restore -warnaserror`; los flujos de publicación y de imagen de contenedor repiten la misma invocación, porque una compuerta que solo cubre una de las tres rutas no cubre ninguna.

Lo que hace útil a esa decisión de `AnalysisMode` no es el valor elegido sino que esté argumentada contra un criterio explícito. Quedarse en `Default` por omisión y quedarse en `Default` tras evaluar `Recommended` producen el mismo XML y no son la misma decisión: en el segundo caso, el lector que dentro de dos años quiera subir el modo sabe exactamente qué argumento tiene que refutar. En el primero solo encuentra silencio, y lo más probable es que repita la evaluación desde cero.

### El par incompleto

Hay una configuración que aparece con frecuencia y conviene reconocer: `EnforceCodeStyleInBuild` en `true` sin ningún `.editorconfig` en el árbol.

La propiedad hace valer en compilación las reglas de estilo `IDExxxx`, pero las severidades que se aplican son entonces las **predeterminadas del SDK**, que para la mayoría de esas reglas son `silent` o `suggestion` y no producen ni aviso ni error. El mecanismo queda activado sobre un conjunto vacío de decisiones propias.

No es un defecto en sentido estricto: el proyecto compila y la compuerta sigue cumpliendo su función sobre los avisos de calidad `CAxxxx`. Lo que falta es la mitad del par que declara qué estilo quiere el equipo —llaves, `var`, orden de `using`, `using` innecesarios, espacios de nombres con ámbito de archivo—. Mientras no esté fijado, la uniformidad que exhiba el código depende de que las herramientas de todos los desarrolladores estén configuradas igual, que es exactamente la situación que un `.editorconfig` existe para evitar. Los tres repositorios de referencia de Microsoft —`dotnet/runtime`, `dotnet/aspnetcore` y `dotnet/efcore`— tienen el suyo en la raíz; el de `dotnet/runtime` es el que `O-08` documenta.

La corrección es barata y no toca nada más: un `.editorconfig` en la raíz con `root = true`, el conjunto mínimo de reglas declarado, y una pasada de `dotnet format` en un commit dedicado, siguiendo la secuencia de `ESC-3` que se describe abajo.

---

## `ESC-3` — La estrategia de normalización

Normalizar el estilo de un código existente es trivial de ejecutar y caro de integrar. `dotnet format` corre en segundos; el problema es todo lo que viene después.

Tres costos concretos, y conviene tenerlos presentes porque explican cada paso de la estrategia. El diff toca todos los archivos y sepulta cualquier cambio funcional que lo acompañe. `git blame` deja de servir: cada línea pasa a atribuirse al commit de normalización y a la persona que lo ejecutó, y la pregunta «¿quién escribió esto y por qué?» pierde su respuesta. Y toda rama abierta en ese momento entra en conflicto con todo.

### La secuencia

**Uno. Anunciar y elegir el momento.** La normalización compite con toda rama abierta. El momento correcto es aquel en que hay pocas, y el equipo tiene que saber que va a ocurrir para integrar lo que tenga en curso antes.

**Dos. Declarar la regla en `.editorconfig` antes de normalizar.** El archivo primero, el reformateo después. Si el archivo no está, la normalización aplica lo que la herramienta traiga por defecto en la máquina de quien la ejecutó, y eso no es una decisión de equipo.

**Tres. Un commit por clase de cambio, sin ninguna modificación funcional.** No uno solo con todo. El orden que esta guía recomienda va de lo más mecánico a lo que más criterio requiere:

```bash
git checkout -b normalizacion/estilo

# Commit 1 — espacios en blanco, finales de línea, indentación
dotnet format whitespace
git commit -am "estilo: normalizar espacios en blanco y finales de línea"

# Commit 2 — reglas de estilo IDExxxx con corrección automática
dotnet format style
git commit -am "estilo: aplicar reglas IDE del .editorconfig"

# Commit 3 — reglas de calidad con corrección automática, revisadas una a una
dotnet format analyzers
git commit -am "estilo: aplicar correcciones automáticas de analizadores"
```

El tercer commit es el único que requiere revisión real, porque algunas correcciones automáticas de reglas `CAxxxx` cambian comportamiento. Los dos primeros son verificables por construcción: si el ensamblado compilado es idéntico, el cambio no tocó nada.

**Cuatro. Registrar los commits en `.git-blame-ignore-revs`.** Un archivo de texto en la raíz con un hash por línea. `git blame` los salta y devuelve la atribución real.

```
# .git-blame-ignore-revs
# Commits de normalización de estilo, sin cambio funcional.
# Configurar una vez por clon:
#   git config blame.ignoreRevsFile .git-blame-ignore-revs

# estilo: normalizar espacios en blanco y finales de línea (2026-07-19)
a3f9c21b8e7d4f0a1c2b3d4e5f6a7b8c9d0e1f2a

# estilo: aplicar reglas IDE del .editorconfig (2026-07-19)
b4a0d32c9f8e5a1b2d3c4e5f6a7b8c9d0e1f2a3b
```

Los hashes del ejemplo son ilustrativos. La línea de `git config` conviene dejarla en el propio archivo y en el documento de incorporación del repositorio, porque el archivo no se aplica solo: cada clon debe configurarlo. Las plataformas de alojamiento que reconocen el archivo lo usan sin configuración adicional en su vista web, lo cual cubre el caso más frecuente de consulta.

**Cinco. Activar la regla en el análisis estático en el mismo cambio.** Es el paso que se olvida y el único que determina si la normalización dura. Normalizar sin elevar la severidad de la regla que se acaba de aplicar garantiza que el código nuevo vuelva a desviarse, y que en seis meses haya que repetir toda la operación con el mismo costo social.

El principio general, que `ESC-3` enuncia y que acá se concreta: **la severidad se eleva en el mismo commit que normaliza**. Antes no se puede porque el código no cumple; después no se hace porque nadie vuelve.

```ini
# En el mismo commit que ejecuta dotnet format style
[*.cs]
dotnet_diagnostic.IDE0005.severity = warning
csharp_style_namespace_declarations = file_scoped:warning
```

### Normalización parcial

Cuando el repositorio es grande, la operación completa puede ser inviable de coordinar. La alternativa es normalizar por carpeta, con un `.editorconfig` intermedio que eleve las severidades solo en la parte ya normalizada, y avanzar carpeta por carpeta. Es más lento y produce un período de convivencia de dos estilos, que es incómodo pero acotado y visible.

Lo que **no** funciona es la normalización oportunista: dejar que cada quien formatee el archivo que toca. Produce archivos con dos estilos adentro, diffs contaminados en cada *pull request*, y un estado final que nunca llega. Es el mismo antipatrón que [`TEM-FORMATO`](Formato-y-Llaves.md) nombra como reformateo oportunista, visto desde el lado del proceso.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

El `.editorconfig` va antes del primer archivo de código. `dotnet new editorconfig` genera uno con el conjunto de reglas de .NET y sus valores predeterminados, y ese archivo es un punto de partida razonable: lo que hay que hacer es revisar las severidades, no reescribirlo.

Las tres decisiones que conviene tomar el primer día: `root = true` en la raíz, la asimetría entre build local y canalización, y qué reglas se elevan a `error` porque su incumplimiento es un defecto y no una preferencia. Todo lo demás se puede ajustar sobre la marcha sin costo.

### `ESC-2` — Evolución estructural

Aplica al margen. Reorganizar la estructura no cambia el estilo, y el `.editorconfig` de la raíz sigue rigiendo sobre archivos que cambiaron de carpeta.

Hay un caso propio del escenario que sí importa: cuando se extrae un módulo a un proyecto nuevo, ese proyecto es una oportunidad barata para configuraciones más estrictas —anulables activados, un `AnalysisMode` más alto— sin arrastrar la deuda del resto. Un `.editorconfig` en la carpeta del proyecto extraído lo permite sin tocar nada más.

### `ESC-3` — Normalización de código existente

Es el escenario de este documento y está desarrollado arriba en su sección propia.

### `ESC-4` — Evaluación de código ajeno

Cinco verificaciones que se hacen en dos minutos y dicen mucho:

1. ¿Existe `.editorconfig`? ¿Declara `root = true`?
2. ¿`EnforceCodeStyleInBuild` está activado en algún lado, o el archivo es decorativo para el editor?
3. ¿Las severidades distinguen entre reglas de estilo y reglas de calidad, o está todo igual?
4. ¿La canalización verifica algo, o solo compila y prueba?
5. ¿Existe `.git-blame-ignore-revs`? Su presencia indica que hubo normalizaciones planificadas; su ausencia junto a un historial con commits enormes de reformateo indica lo contrario.

La combinación más informativa es `EnforceCodeStyleInBuild=true` sin `.editorconfig`, que es exactamente la que se documenta arriba con evidencia: la propiedad está y no hay reglas que hacer valer. No es grave y es un indicador de que la configuración se copió sin completarse.

### Variación por contexto

**`CTX-1`.** Los archivos `.razor` y `.cshtml` merecen sección propia con `indent_size = 2`. El soporte de las reglas de estilo sobre el C# incrustado en marcado es más limitado que sobre archivos `.cs`, y conviene no esperar la misma cobertura.

**`CTX-2`.** Sin particularidad. Vale la pena revisar las reglas `CAxxxx` de rendimiento y de asincronía, que en un servicio tienen más consecuencia que en una aplicación con usuarios humanos.

**`CTX-3`.** El contexto con la configuración más estricta. `GenerateDocumentationFile` activado, `CS1591` —miembro público sin comentario XML— tratado en serio, `CA2007` activo, y las reglas de diseño de API en su nivel más alto porque el costo de un error acá es un cambio ruptor.

**`CTX-4`.** Un `.editorconfig` por repositorio y varios repositorios. Distribuirlo como paquete NuGet de solo configuración, o mantenerlo sincronizado desde un repositorio de plantillas, evita que un desarrollador que rota entre tres servicios encuentre tres estilos. La alternativa —copiar el archivo y dejar que diverja— funciona los primeros meses.

---

## Preguntas guía

1. ¿Existe `.editorconfig` en este repositorio? ¿Declara `root = true`?
2. ¿Lo que declara se verifica en compilación, o solo lo lee el editor de quien tenga el editor adecuado?
3. ¿Las severidades distinguen entre lo que es preferencia y lo que es defecto, o está todo en el mismo nivel?
4. ¿Qué le pasa a un cambio que no cumple: lo detecta un humano, lo corrige el editor al guardar, o lo bloquea la canalización?
5. ¿El build local rompe por cosmética? Si sí, ¿cuánto tiempo por día cuesta eso al equipo?
6. Si se subiera `AnalysisMode` un escalón, ¿cuántos avisos aparecerían? ¿Alguien los va a resolver?
7. Si se normaliza el estilo esta semana, ¿queda la regla activada en el mismo commit, o se va a repetir el trabajo?
8. ¿Existe `.git-blame-ignore-revs`, y está configurado en los clones del equipo?
9. ¿Las supresiones que hay en el código tienen escrito su motivo, o son ruido acumulado?
10. En `CTX-3`: ¿está activado `GenerateDocumentationFile` y se trata `CS1591`?

---

## Criterios de calidad

### Cómo se distingue una aplicación buena de una pobre

**Buena.** Existe `.editorconfig` con `root = true`, y lo que declara se verifica en compilación. Las severidades distinguen reglas de estilo de reglas de defecto. El build local no rompe por cosmética y la canalización no deja pasar nada. Las revisiones de código no contienen observaciones de formato. Cuando el equipo cambia una regla, hay un commit de normalización acompañando el cambio y ese commit está en `.git-blame-ignore-revs`.

**Pobre.** El `.editorconfig` existe y `EnforceCodeStyleInBuild` no, de modo que el archivo solo afecta a quien use el editor que lo lee. O `AnalysisMode=All` con novecientos avisos que nadie mira. O `TreatWarningsAsErrors` sin condición, con la mitad del equipo compilando con una variable de entorno para desactivarlo.

La prueba de una línea: **¿qué le pasa concretamente a un cambio que no cumple la convención?** Si la respuesta es «alguien lo va a notar en la revisión», el sistema depende de memoria humana. Si es «no pasa la canalización», está resuelto.

### Antipatrones

**El `.editorconfig` decorativo.** Declarado y no verificado. Existe para que el editor lo lea y nada más; funciona para quien use Visual Studio o Rider y no para quien use otra cosa, y no funciona en absoluto en la canalización.

**El `root` faltante.** Sin `root = true`, la búsqueda sigue subiendo y puede recoger configuración del directorio del usuario. Produce reglas que aplican en una máquina y no en otra, y el diagnóstico de ese problema es desproporcionadamente difícil respecto de lo trivial que es la causa.

**El modo máximo.** `AnalysisMode=All` activado de un día para otro sobre un repositorio existente. Miles de avisos, ignorados en masa, y con ellos los pocos que señalaban algo real.

**Todo con la misma severidad.** Un `using` innecesario y un `IDisposable` sin liberar como el mismo tipo de aviso. La consecuencia inevitable es que el equipo aprenda a ignorar la categoría entera.

**`TreatWarningsAsErrors` sin condición.** Rompe la iteración local sin comprar nada que la compuerta de la canalización no compre. Se lo desactiva a los tres días y la verificación se pierde entera.

**La normalización sin regla.** Reformatear todo y no elevar la severidad. En seis meses hay que repetirlo, con el mismo costo social y la misma discusión.

**La normalización mezclada.** Un *pull request* que corrige un defecto y reformatea el archivo. El revisor aprueba trescientas líneas de las cuales tres importaban, y no vio ninguna de las tres.

**La supresión global.** `#pragma warning disable` en la primera línea de un archivo, o una entrada en `GlobalSuppressions.cs` para una regla entera. Cuando una regla no aplica al repositorio, corresponde configurarla en `.editorconfig` con su motivo; cuando no aplica a un caso puntual, corresponde suprimirla en ese caso puntual con un comentario que diga por qué.

**El `EnforceCodeStyleInBuild` sin reglas.** Documentado arriba con evidencia. No causa daño y revela que la configuración se copió de algún lado sin completarse.

---

## Anexo — Fragmento comentado

La plantilla completa de `.editorconfig`, lista para copiar, vive en [`ANEXO-PLANTILLAS`](../99-Anexos/Plantillas.md). Lo que sigue es el subconjunto que explica las decisiones, con el motivo de cada bloque.

```ini
# `root = true` detiene la búsqueda hacia arriba. Sin esto, un .editorconfig
# del directorio del usuario puede aplicar en una máquina y no en otra.
root = true

# ---------------------------------------------------------------------------
# Todo archivo
# ---------------------------------------------------------------------------
[*]
charset = utf-8
end_of_line = lf                 # Decisión de equipo: lf con contenedores Linux,
                                 # crlf si todo el equipo trabaja en Windows.
insert_final_newline = true
trim_trailing_whitespace = true  # Evita diffs espurios entre editores.
indent_style = space

# ---------------------------------------------------------------------------
# C# — TEM-FORMATO
# ---------------------------------------------------------------------------
[*.cs]
indent_size = 4                                    # N-06

# Llaves Allman. Primera regla del estilo de dotnet/runtime (F-06, O-08).
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true

# Llaves siempre presentes. Es la única regla de formato de esta guía con un
# argumento de corrección detrás, y por eso va como aviso y no como sugerencia.
csharp_prefer_braces = true:warning

# Organización de using. El orden fijo evita conflictos de fusión en el bloque.
dotnet_sort_system_directives_first = true
csharp_using_directive_placement = outside_namespace:warning
dotnet_diagnostic.IDE0005.severity = warning       # using innecesario

# Modificadores explícitos y en orden canónico.
dotnet_style_require_accessibility_modifiers = for_non_interface_members:warning
dotnet_diagnostic.IDE0036.severity = warning

# ---------------------------------------------------------------------------
# C# — TEM-LENG
# ---------------------------------------------------------------------------

# var según N-06: cuando el tipo es evidente del lado derecho.
# Severidad `suggestion` porque «evidente» es un juicio que la herramienta no
# emite bien, y un falso positivo acá erosiona la confianza en el resto.
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion

# Namespace con ámbito de archivo. Es lo que genera la plantilla del SDK.
csharp_style_namespace_declarations = file_scoped:warning

# `error` y no `warning`: una llamada asíncrona sin esperar es un defecto de
# comportamiento, no una preferencia. Conviene que rompa también en local.
dotnet_diagnostic.CS4014.severity = error          # llamada no esperada

# Nota: el SDK no trae una regla propia para `async void` fuera de un manejador
# de eventos. Detectarlo requiere el paquete Microsoft.VisualStudio.Threading.Analyzers,
# cuya regla VSTHRD100 cubre el caso y ofrece corrección automática a Task (N-16).

# CTX-1 y CTX-2: sin SynchronizationContext, ConfigureAwait no tiene efecto.
# En una biblioteca (CTX-3) este valor debe ser `warning`.
dotnet_diagnostic.CA2007.severity = none

# ---------------------------------------------------------------------------
# Pruebas — convenciones idiomáticas propias
# ---------------------------------------------------------------------------
[tests/**/*.cs]
# Los nombres de método con guion bajo al estilo BDD son legibilidad en pruebas
# y serían un defecto en producción. Relajar acá es más honesto que suprimir la
# regla en todo el repositorio.
dotnet_diagnostic.IDE1006.severity = none

# ---------------------------------------------------------------------------
# Marcado, datos y proyectos
# ---------------------------------------------------------------------------
[*.{json,yml,yaml,razor,cshtml,csproj,props,targets,slnx}]
indent_size = 2
```

---
