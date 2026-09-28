---
doc_id: TEM-FORMATO
doc_type: tema
title: Formato y llaves
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-EST, TEM-LENG, TEM-AUTO, TEM-CAPS, TEM-NS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Formato y llaves — `TEM-FORMATO`

## Resumen ejecutivo

Los estilos de llaves tienen nombre propio desde hace cuarenta años: Allman, K&R, 1TBS, GNU, Whitesmiths. Conocerlos sirve para dos cosas concretas. La primera es poder nombrar lo que se ve —decir «este archivo está en K&R y el resto del proyecto en Allman» es más preciso y menos personal que decir «esto está mal formateado»—. La segunda es entender por qué el mismo desarrollador escribe distinto en C# que en JavaScript sin que ninguno de los dos códigos esté mal: cada comunidad de lenguaje eligió una convención distinta y ambas elecciones son igual de defendibles.

C# usa **Allman**: la llave de apertura va en línea propia. Lo dice `N-06` y lo enuncia como su primera regla el estilo de codificación de `dotnet/runtime` (`F-06`, `O-08`). Java y JavaScript usan K&R, y esa diferencia no responde a ningún argumento técnico que un lado haya ganado; responde a la historia de cada ecosistema.

Ese es el hecho que gobierna la voz de este documento. Casi todo lo que sigue es arbitrario, y decirlo no debilita la recomendación: la refuerza, porque desplaza el criterio desde «cuál es mejor» hacia «cuál usa este repositorio», que es la única pregunta con respuesta objetiva. Le sirve a `ACT-03` al fijar el conjunto inicial de convenciones, a `ACT-02` al incorporarse a un código ajeno y a `ACT-04` para saber qué observaciones ya no vale la pena escribir a mano.

---

## Definición

### Qué es

El conjunto de decisiones sobre la disposición visual del texto del programa: dónde se cortan las líneas, cuánto se indenta, dónde van las llaves, en qué orden aparecen los `using` y los miembros de una clase, dónde se dejan líneas en blanco. Ninguna de esas decisiones altera el árbol de sintaxis: el compilador produce exactamente el mismo ensamblado con cualquiera de ellas.

Esa propiedad —invisibilidad para el compilador— es lo que define la categoría y lo que explica su tratamiento. Como no hay consecuencia funcional, no hay experimento que resuelva la discusión, y por eso las discusiones de formato no terminan solas.

### Qué problema resuelve

Reduce el costo de lectura. Un lector que reconoce la forma del código antes de leerlo dedica su atención al contenido; un lector que tiene que reconstruir la estructura visualmente en cada archivo gasta atención en algo que no aporta información. El formato uniforme también hace que los diffs de control de versiones muestren cambios reales en lugar de reacomodos, lo cual tiene un efecto directo sobre la calidad de las revisiones.

Hay un segundo efecto, menos evidente y más valioso. Un formato automatizado elimina una clase entera de conversación de las revisiones de código. `ACT-04` deja de escribir «faltó una línea en blanco acá» y puede dedicar la revisión a lo que una herramienta no puede ver.

### Qué NO es, y con qué se lo confunde

**No es nomenclatura.** [`FAM-NOM`](../40-Nomenclatura/README.md) decide qué palabras se usan y con qué capitalización; este documento decide cómo se dispone el texto alrededor de esas palabras. La confusión es frecuente porque ambas cosas se declaran en el mismo archivo `.editorconfig`, pero tienen costos de cambio muy distintos: reformatear es mecánico, renombrar no.

**No es diseño.** Un método de doscientas líneas perfectamente formateado sigue siendo un método de doscientas líneas. El formato no mejora la estructura del código; a lo sumo hace visible que la estructura es mala, lo cual no es poco pero tampoco es lo mismo.

**No es una convención de lenguaje.** Elegir entre `var` y el tipo explícito, o entre `record` y `class`, se discute con el mismo tono que el formato pero tiene consecuencias reales sobre el significado del programa. Esas decisiones están en [`TEM-LENG`](Convenciones-de-Lenguaje.md), y la diferencia importa porque el argumento «da lo mismo mientras sea uniforme» vale acá y no vale allá.

**No es un tema de calidad de código.** Un analizador que señala un `using` fuera de orden y otro que señala un `IDisposable` sin liberar aparecen ambos en la lista de avisos de compilación, y el equipo que los trata igual va a terminar ignorando los dos. La distinción entre reglas `IDExxxx` y `CAxxxx` está en [`TEM-AUTO`](Automatizacion-del-Estilo.md) y no es cosmética.

---

## Los estilos de llaves con nombre propio

Cinco estilos con nombre reconocido. Se distinguen por dos preguntas: dónde va la llave de apertura, y a qué nivel se indentan las llaves respecto del bloque que delimitan.

### Allman

La llave de apertura ocupa una línea propia, alineada con la sentencia que introduce el bloque. La de cierre también. El cuerpo se indenta un nivel.

```csharp
public decimal CalcularImporte(Reserva reserva)
{
    if (reserva.EsBonificada)
    {
        return 0m;
    }

    return reserva.Duracion.TotalHours * TarifaHoraria;
}
```

Debe su nombre a Eric Allman. Se lo conoce también como estilo BSD, por el código de esa distribución de Unix. Es el estilo de C#, y el de C++ en buena parte del mundo Windows.

Su rasgo característico es que la llave de apertura y la de cierre quedan verticalmente alineadas, lo que hace visible el emparejamiento de bloques con un golpe de vista. El costo es que cada bloque consume una línea adicional, y en código con anidamiento profundo eso se nota.

### K&R

La llave de apertura va al final de la línea que introduce el bloque; la de cierre en línea propia, alineada con esa sentencia.

```c
int calcular_importe(reserva_t *reserva) {
    if (reserva->bonificada) {
        return 0;
    }
    return reserva->horas * TARIFA;
}
```

El nombre viene de Kernighan y Ritchie, por el estilo de *The C Programming Language*. En su forma original había una asimetría: las definiciones de función llevaban la llave de apertura en línea propia y las estructuras de control la llevaban al final de la línea. Esa asimetría se perdió en la mayoría de las variantes modernas.

Es el estilo dominante en Java, JavaScript, TypeScript, Go y Rust. Un desarrollador que escribe C# por la mañana y TypeScript por la tarde cambia de estilo dos veces al día sin pensarlo, y eso es correcto: la convención es del lenguaje, no de la persona.

### 1TBS

*One True Brace Style*. Es una variante de K&R con una regla adicional que no es cosmética: **las llaves no se omiten nunca**, ni siquiera cuando el bloque contiene una sola sentencia.

```java
if (reserva.esBonificada()) {
    return BigDecimal.ZERO;
}
```

En lugar de la forma sin llaves que el lenguaje permite:

```java
if (reserva.esBonificada()) return BigDecimal.ZERO;
```

El nombre es una broma de la comunidad de C, pero la regla que lo define tiene fundamento verificable: la omisión de llaves en bloques de una sentencia ha producido defectos reales cuando alguien agrega una segunda sentencia y la indentación sugiere que está dentro del bloque cuando no lo está. Que sea la única regla de esta familia con un argumento de corrección detrás es la razón por la que conviene retenerla aunque el estilo de llaves de uno sea otro: Allman con llaves siempre presentes captura el mismo beneficio.

### GNU

La llave de apertura va en línea propia, pero **indentada** respecto de la sentencia que introduce el bloque, y el cuerpo se indenta otro nivel más. La convención del proyecto GNU usa además dos espacios por nivel.

```c
int
calcular_importe (reserva_t *reserva)
{
  if (reserva->bonificada)
    {
      return 0;
    }
  return reserva->horas * TARIFA;
}
```

Se encuentra casi exclusivamente en código del proyecto GNU y en Emacs Lisp. Fuera de ese ámbito es raro, y nadie lo propone para C#.

### Whitesmiths

La llave de apertura va en línea propia e indentada al mismo nivel que el cuerpo, de modo que las llaves quedan alineadas con las sentencias que contienen y no con la sentencia que las introduce.

```c
int calcular_importe (reserva_t *reserva)
    {
    if (reserva->bonificada)
        {
        return 0;
        }
    return reserva->horas * TARIFA;
    }
```

Debe su nombre al compilador Whitesmiths C, de finales de los setenta. Es el más raro de los cinco en código contemporáneo. Su lógica interna es coherente —la llave pertenece al bloque, no al encabezado— y esa coherencia no bastó para que sobreviviera.

### Comparación

| Estilo | Llave de apertura | Indentación de la llave | Dónde se encuentra |
|--------|-------------------|-------------------------|--------------------|
| **Allman** | Línea propia | Nivel del encabezado | C#, C++ en Windows, C# de `dotnet/runtime` |
| **K&R** | Fin de línea | — | Java, JavaScript, TypeScript, Go, Rust |
| **1TBS** | Fin de línea | — | Variante de K&R; su aporte es no omitir llaves nunca |
| **GNU** | Línea propia | Un nivel adentro | Proyecto GNU |
| **Whitesmiths** | Línea propia | Nivel del cuerpo | Prácticamente en desuso |

---

## C# usa Allman

Es normativo, y la fuente es `N-06`: ahí está especificado. Lo que aporta `F-06` es otra cosa —una convención de facto que **reafirma** la regla normativa, no que la sustituya—: el estilo de codificación de `dotnet/runtime` (cuyo texto es `O-08`) enuncia Allman literalmente como su **primera** regla, con la llave de apertura de cualquier bloque en su propia línea. Un lector que solo conociera `F-06` podría creer que Allman en C# es práctica del ecosistema; es norma, y la práctica coincide.

Que sea la primera regla de ese documento no es casual y conviene leerlo bien. No significa que sea la regla más importante del estilo de C#; significa que es la que más ruido produce si no está fijada, porque afecta a cada bloque de cada archivo. Fijarla primero es una decisión de economía, no de jerarquía.

La comparación con Java y JavaScript es la que mejor explica de qué naturaleza es esta regla. El mismo lector que encuentra natural Allman en C# encuentra natural K&R en TypeScript, y no hay ninguna propiedad de los lenguajes que justifique la diferencia: C# y Java tienen sintaxis de bloques prácticamente idéntica. Lo que difiere es la historia. C# nació en Microsoft, donde la tradición de C++ era Allman; Java nació en Sun, donde la tradición de C era K&R. Cuarenta años después seguimos escribiendo según decisiones que tomaron otros por razones que ya no aplican, y eso está bien: el costo de coordinar una convención arbitraria es enormemente menor que el de no tener ninguna.

**Esta guía recomienda** no discutirlo. En un repositorio C#, Allman. En uno con archivos `.ts` junto a `.cs`, Allman para los `.cs` y K&R para los `.ts`, declarado en secciones distintas del mismo `.editorconfig`. Un equipo que decide unificar ambos lenguajes bajo un estilo único gana coherencia dentro del repositorio y la pierde contra todo el código del ecosistema que sus desarrolladores van a leer fuera de él.

### Llaves en bloques de una sentencia

`N-06` admite omitir las llaves cuando el bloque tiene una sola sentencia, y el estilo de `dotnet/runtime` (`O-08`) también las admite bajo la condición de que la sentencia vaya en la línea siguiente y de que las llaves nunca se omitan si otra rama del mismo `if`/`else` las usa.

**Esta guía recomienda** escribirlas siempre, por el argumento de 1TBS: el defecto que introduce omitirlas —agregar una segunda sentencia bajo una indentación que miente— es real, y el ahorro de dos líneas no lo compensa. Es una de las pocas reglas de este documento que no es arbitraria, y por eso se la enuncia con más firmeza que a las demás.

```csharp
// Riesgoso: un agregado posterior parece pertenecer al bloque y no pertenece.
if (!reserva.EstaConfirmada)
    return Results.Conflict();

// Preferido: el bloque tiene límites explícitos.
if (!reserva.EstaConfirmada)
{
    return Results.Conflict();
}
```

---

## Indentación

Cuatro espacios por nivel es la convención de C#, y así lo fija `N-06`. `O-08` la reafirma para `dotnet/runtime` y agrega la parte que se olvida: **espacios, no tabulaciones**.

La discusión entre espacios y tabulaciones tiene un argumento real de cada lado y por eso no se resuelve. A favor de la tabulación: es un carácter semántico, cada lector la configura al ancho que prefiere, y quien necesita indentación ancha por razones de accesibilidad puede obtenerla sin tocar el archivo. A favor del espacio: el archivo se ve igual en cualquier herramienta, incluidas las que no permiten configurar el ancho, y la alineación de continuaciones de línea no se rompe.

C# eligió espacios. Es lo que hay, y el `.editorconfig` lo declara en una línea:

```ini
[*.cs]
indent_style = space
indent_size = 4
```

Lo que sí es un problema objetivo, y no una preferencia, es **mezclar** ambos en el mismo archivo. Produce código que se ve alineado en un editor y desalineado en otro, y es de los pocos defectos de formato que llegan a costar tiempo de depuración real. Cualquier `.editorconfig` que declare `indent_style` lo evita.

---

## Longitud de línea

No hay un límite normativo. `N-06` recomienda evitar líneas largas sin fijar un número; el estilo de `dotnet/runtime` (`O-08`) tampoco impone una columna dura.

Los números que circulan en el ecosistema —80, 100, 120— vienen de restricciones que ya no existen: 80 es el ancho del terminal heredado de la tarjeta perforada. Lo que sigue teniendo vigencia es otra cosa: la comparación de código en dos columnas, que es como se revisan los *pull requests*, y que en una pantalla común deja de funcionar bastante antes de las 200 columnas.

**Esta guía recomienda** fijar un límite blando alrededor de 120 columnas, tratado como aviso y no como error, y aceptar que ciertas construcciones —una cadena de configuración fluida, una firma con genéricos anidados— lo excedan sin que eso signifique nada. Un límite duro produce cortes de línea artificiales que empeoran la lectura, que es exactamente lo contrario de lo que se buscaba.

Lo que sí conviene fijar es cómo se corta una línea larga cuando se corta, porque ahí la inconsistencia sí molesta: el operador al principio de la línea siguiente, no al final de la anterior, para que el ojo detecte la continuación en la columna izquierda.

```csharp
var disponibles = salas
    .Where(s => s.Aforo >= asistentes)
    .Where(s => !s.Reservas.Any(r => r.Periodo.SeSolapaCon(periodo)))
    .OrderBy(s => s.Aforo)
    .ToList();
```

---

## Ordenamiento de `using`

`N-06` fija dos reglas: las directivas `using` van **fuera** del espacio de nombres, y las que empiezan con `System` van **primero**, con el resto en orden alfabético a continuación.

La segunda regla es un caso interesante de convención con una justificación que se evaporó. Cuando el orden era manual, poner `System` primero servía para separar visualmente lo que viene de la plataforma de lo que viene de paquetes. Hoy la herramienta ordena sola y nadie mira el bloque, de modo que la regla sobrevive por inercia y por el único motivo que importa: si todos los archivos usan el mismo orden, un `using` agregado por dos ramas distintas no produce conflicto de fusión en el mismo lugar.

```ini
[*.cs]
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false
```

Los `using` no utilizados merecen mención aparte porque no son formato: son ruido que confunde sobre las dependencias reales de un archivo. La regla `IDE0005` los detecta, y elevarla a aviso o error es de las decisiones con mejor relación entre costo y beneficio de todo el `.editorconfig`.

---

## Líneas en blanco y espaciado

Las reglas concretas importan menos que el principio que las ordena: **una línea en blanco separa ideas, no elementos sintácticos**. Dos métodos consecutivos van separados porque son dos ideas; los campos de una clase pueden ir juntos porque son una sola.

Lo que `N-06` y `O-08` fijan con claridad:

- Un espacio después de una coma que separa argumentos, ninguno antes.
- Espacio alrededor de los operadores binarios; ninguno alrededor de los unarios.
- Ningún espacio entre el nombre de un método y su paréntesis de apertura —a diferencia del estilo GNU, que lo exige—.
- Ninguna línea en blanco entre la llave de apertura de un bloque y su primera sentencia.
- Sin espacios en blanco al final de la línea.

Ese último punto es el único con un efecto práctico medible: los espacios finales producen diffs espurios cuando un editor los recorta y otro no, y contaminan el historial con cambios que no cambian nada. Se resuelve con `trim_trailing_whitespace = true`, que junto a `insert_final_newline = true` y `end_of_line` son las tres líneas del `.editorconfig` que aplican a todo tipo de archivo, no solo a `.cs`.

```ini
[*]
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true
charset = utf-8-bom
```

El valor de `end_of_line` es una decisión de equipo, no una regla: `crlf` es lo habitual en repositorios donde todos trabajan en Windows —y es lo que `.editorconfig` de plantillas de Visual Studio suele traer—, `lf` es lo habitual cuando hay contenedores Linux o desarrollo mixto de por medio. Lo que no funciona es dejarlo sin declarar y confiar en la configuración de Git de cada uno.

---

## Orden de miembros dentro de una clase

`N-06` recomienda un orden. La convención más extendida en el ecosistema, coherente con esa recomendación:

1. Campos constantes y estáticos
2. Campos de instancia
3. Constructores
4. Propiedades
5. Métodos
6. Tipos anidados

Dentro de cada grupo, de más público a más privado.

El fundamento es que un lector que abre un tipo por primera vez quiere primero el estado y después el comportamiento, y quiere primero lo que puede usar desde afuera. Es un fundamento razonable y no es el único posible: hay equipos que agrupan por funcionalidad —el campo privado junto al método que lo usa— y ese criterio tiene su propia lógica, especialmente en tipos grandes.

**Esta guía recomienda** el orden canónico por una razón que no tiene que ver con cuál es mejor: es el que las herramientas conocen. Visual Studio y ReSharper pueden reordenar automáticamente según ese esquema, y una convención que la herramienta puede aplicar sola vale más que una que requiere disciplina. El agrupamiento por funcionalidad, en cambio, exige que cada persona lo mantenga a mano en cada edición.

### Orden de modificadores

El orden lo fija la opción `csharp_preferred_modifier_order`, documentada en `N-07` junto con el resto del catálogo de opciones de estilo, y es una de las pocas reglas de formato que el analizador verifica de forma automática y sin ambigüedad (`IDE0036`):

```
public protected internal private file
static
extern
new
virtual abstract sealed override
readonly
unsafe
required
volatile
async
```

En la práctica se traduce en `public static readonly`, nunca `static public readonly`. Es una regla que nadie discute porque es puramente mecánica, y por eso conviene declararla y olvidarla:

```ini
[*.cs]
csharp_preferred_modifier_order = public,private,protected,internal,file,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,required,volatile,async:warning
```

---

## `#region`

Está mal visto y conviene entender exactamente por qué, porque el argumento habitual —«es feo»— no convence a nadie.

La objeción real es de diagnóstico. Una región es un mecanismo para **ocultar** código en el editor, y el código que hace falta ocultar suele ser código que sobra. Un tipo con seis regiones que agrupan veinte métodos cada una es un tipo con seis responsabilidades, y la región permite convivir con eso en lugar de resolverlo. La región no causa el problema: lo hace tolerable, que es peor, porque quita la incomodidad que habría llevado a partir el tipo.

Hay un segundo efecto, más concreto. El contenido de una región colapsada no se ve al revisar, no aparece en las búsquedas visuales y tiende a acumular código muerto. Un `#region Código antiguo` es un lugar donde las cosas van a morir sin que nadie se entere.

**Esta guía recomienda** no usar `#region` en código escrito a mano. Las excepciones legítimas son dos, y ambas comparten un rasgo: el código de adentro no está pensado para leerse. Código generado por herramienta que convive con código manual en el mismo archivo, y agrupaciones que una herramienta genera y mantiene. Fuera de eso, si un tipo necesita regiones para ser navegable, lo que necesita es partirse.

`N-06` no prohíbe las regiones. La posición de este documento es criterio propio, y conviene registrarlo como tal.

---

## Comentarios

La distinción que ordena todo el tema: un comentario que explica **qué** hace el código compite con el código y pierde, porque el código es la fuente de verdad y el comentario se desactualiza. Un comentario que explica **por qué** aporta información que el código no puede contener.

```csharp
// Ruido: repite lo que la línea siguiente ya dice.
// Incrementa el contador de intentos.
intentos++;

// Aporta: la restricción no está en el código y explica un número mágico.
// El límite de 5 viene de CU-09 (AUTH_DEMASIADOS_INTENTOS); subirlo requiere
// revisar la ventana de bloqueo, que está calibrada contra ese valor.
const int MaximoIntentos = 5;
```

El segundo caso donde el comentario aporta es cuando el código es deliberadamente raro. Una implementación que parece innecesariamente complicada y que responde a un defecto de una dependencia, a una restricción de rendimiento medida o a un caso límite del dominio necesita una línea que lo diga, porque de lo contrario alguien la va a simplificar y a reintroducir el problema. Es el mismo argumento que sostiene el ADR, en escala pequeña.

`N-06` fija reglas de forma: el comentario en línea propia y no al final de la línea de código, una mayúscula al inicio, un espacio entre las barras y el texto. Son convención, y valen lo que valen.

### Comentarios XML de documentación

Los comentarios `///` producen documentación estructurada, alimentan IntelliSense y pueden generar un archivo `.xml` para consumidores del ensamblado. Su peso cambia radicalmente según el contexto.

En `CTX-3` —biblioteca reutilizable— son parte del producto. Un consumidor que no tiene el código fuente ve la firma y el comentario XML, y nada más; un miembro público sin `<summary>` es un miembro que obliga a adivinar. Ahí conviene activar `GenerateDocumentationFile` y tratar el aviso `CS1591` —miembro público sin documentar— como algo que hay que resolver.

En `CTX-1` y `CTX-2` el cálculo es distinto. Documentar con XML cada método de una aplicación interna produce mucho texto ceremonial —`/// <summary>Obtiene el identificador.</summary>` sobre una propiedad `Id`— que nadie lee y que se desactualiza igual que cualquier otro comentario. **Esta guía recomienda** en esos contextos reservar el comentario XML para lo que no es evidente: comportamiento ante casos límite, excepciones que el método puede lanzar, unidades y rangos de un parámetro.

```csharp
/// <summary>
/// Confirma una reserva previamente solicitada, en estado Pendiente.
/// </summary>
/// <param name="reservaId">Identificador de la reserva pendiente.</param>
/// <param name="cancelacion">Token de cancelación de la operación.</param>
/// <returns>
/// La reserva confirmada. Si otra confirmación ganó la carrera sobre el mismo
/// período, la operación falla con <see cref="ReservaSolapadaException"/> y
/// la reserva pendiente queda liberada.
/// </returns>
/// <exception cref="ReservaSolapadaException">
/// El período dejó de estar disponible entre la solicitud y la confirmación.
/// </exception>
Task<Reserva> ConfirmarAsync(Guid reservaId, CancellationToken cancelacion);
```

Ese bloque aporta porque la mitad de su contenido no está en la firma: qué pasa ante una carrera perdida, y qué queda de la reserva pendiente. Un `<summary>` que dijera «Confirma la reserva» y nada más habría sido ruido.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

El costo de fijar el estilo es mínimo y el momento es el primer día. Un `.editorconfig` en la raíz antes del primer archivo de código elimina el problema para siempre; el mismo archivo agregado en el mes seis obliga a normalizar doscientos archivos y a coordinar la operación con todas las ramas abiertas.

La recomendación operativa es empezar por lo que la plantilla de Visual Studio o `dotnet new editorconfig` genera, ajustar solo lo que el equipo tenga una razón concreta para ajustar, y no convertir la creación del archivo en un taller de dos horas. Las decisiones que importan de verdad —severidades, qué se rompe en la canalización de integración continua— están en [`TEM-AUTO`](Automatizacion-del-Estilo.md) y no son estas.

### `ESC-2` — Evolución estructural

Prácticamente no aplica. Reorganizar la estructura de un sistema no cambia su formato: los archivos se mueven de carpeta con el contenido intacto. La única intersección real es que un movimiento masivo de archivos es una oportunidad barata para aplicar una normalización pendiente, y esa oportunidad conviene **no** tomarla: mezclar el movimiento con el reformateo produce un diff donde no se distingue un archivo movido de uno reescrito, y hace irrevisable el cambio.

### `ESC-3` — Normalización de código existente

Es el escenario propio de este documento y donde el formato se vuelve un problema social. Reformatear es trivial de ejecutar y caro de integrar: el diff toca todos los archivos, `git blame` deja de servir, y cualquier rama abierta entra en conflicto.

La secuencia que esta guía recomienda —desarrollada en [`TEM-AUTO`](Automatizacion-del-Estilo.md)— es normalizar en commits sin cambio funcional, registrarlos en `.git-blame-ignore-revs` y activar la regla en el análisis estático en el mismo commit. El orden dentro del escenario también importa: primero el formato puro, que una herramienta aplica y verifica sola, y solo después lo que requiere criterio.

### `ESC-4` — Evaluación de código ajeno

Lo que se juzga no es el estilo sino la **consistencia** y el **mecanismo**. Tres preguntas alcanzan: ¿el estilo es uniforme entre archivos y entre autores?, ¿existe un `.editorconfig` que lo declare?, ¿ese archivo se verifica en compilación o es decorativo?

Un repositorio en K&R uniforme y verificado está mejor que uno en Allman aplicado a la mitad. Señalar la elección de estilo en una evaluación externa es el error de evaluador más común de esta familia y desacredita el resto del informe.

### Variación por contexto

**`CTX-1`.** Aparecen artefactos que no son C# y que tienen sus propias reglas: `.razor`, `.cshtml`, `.css`, `.json`. El `.editorconfig` los cubre con secciones propias, y conviene no forzarles la convención de C#: `indent_size = 2` es lo habitual en marcado y en JSON. Los archivos `.razor` mezclan marcado y C#, y el soporte de las reglas de estilo sobre ellos es más limitado que sobre `.cs`.

**`CTX-2`.** Sin particularidades de formato. La superficie que importa en este contexto es el contrato HTTP y JSON, que es nomenclatura, no formato.

**`CTX-3`.** El único contexto donde una decisión de este documento tiene consecuencia externa: los comentarios XML de los miembros públicos son parte de lo que el consumidor recibe. `GenerateDocumentationFile` activado y `CS1591` tratado en serio.

**`CTX-4`.** El estilo se decide por repositorio, y una solución distribuida suele tener varios. Conviene un `.editorconfig` compartido —copiado o distribuido como paquete— porque un desarrollador que rota entre tres servicios con tres estilos distintos pierde el beneficio completo de la convención.

---

## Ejemplos concretos

### El mismo método en tres estilos

Sistema de reserva de salas, ejemplo sintético. El primer bloque es el estilo de C# según `N-06`.

```csharp
public sealed class ServicioDisponibilidad
{
    private readonly IRepositorioSalas _salas;

    public ServicioDisponibilidad(IRepositorioSalas salas)
    {
        _salas = salas;
    }

    public async Task<IReadOnlyList<Sala>> BuscarAsync(
        RangoHorario periodo,
        int asistentes,
        CancellationToken cancelacion)
    {
        if (asistentes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(asistentes));
        }

        var candidatas = await _salas.ObtenerActivasAsync(cancelacion);

        return candidatas
            .Where(s => s.Aforo >= asistentes)
            .Where(s => !s.Reservas.Any(r => r.Periodo.SeSolapaCon(periodo)))
            .OrderBy(s => s.Aforo)
            .ToList();
    }
}
```

El mismo tipo en K&R, que es como se vería si el lenguaje fuera Java. Compila igual en C#, y ningún analizador con la configuración predeterminada lo rechaza; simplemente no es lo que hace el ecosistema:

```csharp
public sealed class ServicioDisponibilidad {
    private readonly IRepositorioSalas _salas;

    public ServicioDisponibilidad(IRepositorioSalas salas) {
        _salas = salas;
    }
}
```

Y el detalle que sí importa, que no es el estilo de llaves sino la omisión:

```csharp
// Este código es correcto hoy.
foreach (var sala in candidatas)
    if (sala.EstaDisponible(periodo))
        resultado.Add(sala);

// Y este es el defecto que aparece cuando alguien agrega una línea.
foreach (var sala in candidatas)
    if (sala.EstaDisponible(periodo))
        resultado.Add(sala);
        registro.Anotar(sala);   // Se ejecuta siempre. La indentación miente.
```

Ese es el argumento de 1TBS en una pantalla. No depende de qué estilo de llaves use el repositorio.

### Un archivo completo con el orden canónico

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reservas.Dominio.Salas;
using Reservas.Dominio.Excepciones;

namespace Reservas.Aplicacion.Disponibilidad;

/// <summary>
/// Resuelve qué salas admiten una reserva en un período dado.
/// </summary>
public sealed class ConsultaDisponibilidad
{
    // 1. Constantes y campos estáticos
    public const int AforoMaximoSala = 200;
    private static readonly TimeSpan s_duracionMinima = TimeSpan.FromMinutes(15);

    // 2. Campos de instancia
    private readonly IRepositorioSalas _salas;
    private readonly TimeProvider _reloj;

    // 3. Constructores
    public ConsultaDisponibilidad(IRepositorioSalas salas, TimeProvider reloj)
    {
        _salas = salas ?? throw new ArgumentNullException(nameof(salas));
        _reloj = reloj ?? throw new ArgumentNullException(nameof(reloj));
    }

    // 4. Propiedades
    public TimeSpan DuracionMinima => s_duracionMinima;

    // 5. Métodos, de más público a más privado
    public async Task<IReadOnlyList<Sala>> EjecutarAsync(
        RangoHorario periodo,
        CancellationToken cancelacion)
    {
        ValidarPeriodo(periodo);
        var activas = await _salas.ObtenerActivasAsync(cancelacion);
        return Filtrar(activas, periodo);
    }

    private void ValidarPeriodo(RangoHorario periodo)
    {
        if (periodo.Duracion < s_duracionMinima)
        {
            throw new PeriodoInvalidoException(periodo, s_duracionMinima);
        }
    }

    private static IReadOnlyList<Sala> Filtrar(
        IEnumerable<Sala> salas,
        RangoHorario periodo)
    {
        var resultado = new List<Sala>();

        foreach (var sala in salas)
        {
            if (sala.EstaDisponible(periodo))
            {
                resultado.Add(sala);
            }
        }

        return resultado;
    }
}
```

Ejemplo sintético. El prefijo `s_` de `s_duracionMinima` es estilo de `dotnet/runtime` (`F-03`), no una norma general de .NET: adoptarlo es legítimo y atribuirlo a las Framework Design Guidelines no lo es. El detalle está en [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md).

---

## Preguntas guía

1. ¿El estilo de este repositorio está declarado en un archivo, o vive en la configuración del editor de cada persona?
2. ¿Lo que estoy por señalar en esta revisión podría haberlo señalado una herramienta? Si sí, la observación correcta no es sobre el código sino sobre el `.editorconfig`.
3. ¿Estoy defendiendo Allman sobre K&R, o estoy defendiendo la uniformidad? Si es lo primero, el argumento no existe.
4. ¿Este *pull request* mezcla reformateo con cambio funcional?
5. ¿Este comentario dice qué hace el código o por qué lo hace así? Si dice qué, ¿el código no podría decirlo solo con un nombre mejor?
6. Si un tipo necesita regiones para navegarse, ¿cuántas responsabilidades tiene?
7. En `CTX-3`: ¿los miembros públicos tienen comentario XML, y ese comentario dice algo que la firma no diga?
8. ¿Qué pasa concretamente si alguien escribe con el estilo equivocado: lo detecta una persona, lo corrige el editor, o lo bloquea la compilación?

---

## Criterios de calidad

### Cómo se distingue una aplicación buena de una pobre

**Buena.** El estilo es uniforme y nadie lo recuerda, porque está declarado en `.editorconfig` y verificado en compilación. Un desarrollador nuevo escribe con el estilo del repositorio sin haberlo leído, porque el editor lo aplica solo. Las revisiones de código no contienen ni un comentario sobre formato. El historial no tiene commits que mezclen reformateo con cambio funcional.

**Pobre.** El estilo depende del editor que cada uno tenga configurado. Existe un documento de convenciones que nadie leyó y ninguna herramienta verifica. Las revisiones contienen observaciones de formato, y por lo tanto `ACT-04` gasta su atención donde una máquina habría alcanzado. Hay archivos con dos estilos adentro, herencia de que alguien reformateó la mitad.

La prueba de una línea: **¿qué pasa si alguien escribe con el estilo equivocado?** Si la respuesta es «un revisor lo va a notar», el sistema depende de memoria humana y va a fallar. Si es «no compila en la canalización» o «el editor lo corrige al guardar», está resuelto.

### Antipatrones

**El estilo por editor.** Cada desarrollador con su configuración, sin `.editorconfig`. El síntoma es el archivo cuyo diff muestra veinte líneas cambiadas cuando se cambió una: alguien lo abrió con otra configuración y el editor lo reformateó al guardar.

**El documento de estilo sin herramienta.** Un `CONVENCIONES.md` de treinta páginas y ningún archivo de configuración. Se lee una vez al ingresar y nunca más. Toda regla de ese documento que no está en `.editorconfig` es una regla que va a erosionarse; la pregunta de `ACT-03` —¿esto lo verifica una herramienta o depende de que alguien lo recuerde?— existe exactamente para detectarlo.

**El reformateo oportunista.** Un *pull request* que arregla un defecto y de paso reformatea el archivo entero. Sepulta el cambio real bajo trescientas líneas de ruido, y el revisor aprueba sin haber visto lo que importaba. Es el antipatrón más caro de esta familia porque su costo se paga en defectos que pasaron sin revisión.

**La región como archivador.** `#region Helpers` con novecientas líneas adentro. La región no es la causa del problema, pero es lo que permite que el problema no moleste.

**El comentario que repite el código.** `// Incrementa el contador` sobre `contador++`. No es inofensivo: entrena al lector a saltear los comentarios, y entonces el comentario que sí importa también se saltea.

**El comentario fósil.** Código comentado que nadie borra por si acaso. El control de versiones existe justamente para eso, y un bloque comentado de hace dos años no se puede reactivar de todos modos porque el resto del archivo cambió alrededor.

**La normalización sin regla.** Reformatear todo el repositorio y no activar la regla en el análisis estático. En seis meses hay que repetirlo. `ESC-3` insiste en este punto por una razón: es la falla más frecuente de las normalizaciones y la más fácil de evitar.

**El límite de línea dogmático.** Ochenta columnas como error de compilación. Produce cortes artificiales en cadenas fluidas y en firmas genéricas, y termina empeorando la legibilidad que decía proteger.
