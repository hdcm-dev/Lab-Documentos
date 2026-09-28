---
doc_id: TEM-LENG
doc_type: tema
title: Convenciones de lenguaje
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-EST, TEM-FORMATO, TEM-AUTO, TEM-NS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Convenciones de lenguaje — `TEM-LENG`

## Resumen ejecutivo

Hay decisiones que se discuten con el mismo tono que el estilo de llaves y que no pertenecen a esa categoría. Activar tipos de referencia anulables cambia qué compila. Elegir `record` en lugar de `class` cambia cómo se comparan dos instancias. Escribir `async void` cambia dónde termina una excepción no capturada: en lugar de propagarse por la tarea, derriba el proceso. Ninguna de esas cosas es cosmética, y tratarlas con el argumento de «da lo mismo mientras sea uniforme» es el error que este documento intenta prevenir.

La frontera no siempre es nítida y por eso hace falta recorrerla caso por caso. `var` frente al tipo explícito no cambia nada para el compilador —el tipo es el mismo, se infiere en compilación— y sin embargo cambia mucho para el lector. Un miembro con cuerpo de expresión produce el mismo IL que su equivalente con bloque. Esas están del lado del estilo, aunque generen más discusión que las que sí importan; lo cual es una regularidad conocida y algo desalentadora.

El documento le sirve a `ACT-03` para separar qué reglas del `.editorconfig` son preferencia negociable y cuáles son corrección disfrazada de preferencia, y a `ACT-02` para tomar estas decisiones en el momento de escribir sin abrir una discusión. La fuente principal es `N-06`; donde `N-06` no se pronuncia, se declara como criterio propio.

---

## Definición

### Qué es

Las elecciones entre construcciones del lenguaje que expresan lo mismo o casi lo mismo. C# ofrece varias formas de declarar una variable, de escribir un método corto, de comparar dos objetos, de recorrer casos. Elegir entre ellas es una decisión de convención cuando las alternativas son equivalentes, y una decisión de diseño cuando no lo son.

### Qué problema resuelve

Reduce la superficie de variación dentro de un repositorio, igual que el formato, pero con un beneficio adicional: algunas de estas elecciones eliminan clases enteras de defectos. Los tipos de referencia anulables convierten en error de compilación algo que antes era una excepción en producción, y esa es una ganancia de otro orden que la de tener los `using` ordenados.

### Qué NO es, y con qué se lo confunde

**No es formato.** [`TEM-FORMATO`](Formato-y-Llaves.md) trata lo que el compilador ignora. Acá hay al menos tres reglas —anulables, `async void`, `record` frente a `class`— cuyo efecto es observable en tiempo de ejecución.

**No es diseño de API.** Que un método devuelva `Task<Reserva>` o `Task<Result<Reserva>>` es una decisión de diseño con consecuencias sobre todos los llamadores; que dentro de ese método una variable local se declare con `var` no lo es. La confusión aparece porque ambas cosas se discuten en la misma revisión.

**No es una lista de novedades del lenguaje.** C# incorpora sintaxis nueva en cada versión, y adoptar una construcción porque es reciente no es un criterio. La pregunta correcta es si la construcción hace más legible el código concreto que se tiene enfrente, y a veces la respuesta es que no.

---

## `var` frente al tipo explícito

`N-06` fija la regla: usar `var` cuando el tipo es **evidente a partir del lado derecho** de la asignación, y el tipo explícito cuando no lo es.

```csharp
// El tipo es evidente: var.
var salas = new List<Sala>();
var periodo = new RangoHorario(inicio, fin);
var nombre = "Sala Directorio";

// El tipo no es evidente: explícito.
IReadOnlyList<Sala> disponibles = _consulta.Ejecutar(periodo);
decimal importe = CalcularImporte(reserva);
```

La regla es clara de enunciar y ambigua de aplicar, porque «evidente» depende de quién lea. `var resultado = await _servicio.ProcesarAsync(comando);` es evidente para quien escribió `ProcesarAsync` y opaco para quien llega al archivo por primera vez.

De ahí que existan **tres bandos** estables en el ecosistema, y conviene conocerlos porque la discusión reaparece en cada equipo:

**Siempre `var`.** El argumento es que el tipo explícito es redundancia que hay que mantener, que el editor muestra el tipo al pasar el cursor, y que un código con menos ruido a la izquierda se lee más rápido. El costo es la revisión de código en el navegador, donde no hay editor que muestre nada.

**`var` solo cuando el tipo aparece a la derecha.** Es la posición de `N-06` y la de `dotnet/runtime` (`O-08`). El costo es que el criterio no es mecánico, y por lo tanto no se puede verificar automáticamente sin arbitrariedad.

**Nunca `var`.** El argumento es que el tipo explícito documenta la intención en el propio texto y sobrevive a la refactorización del método llamado: si `ObtenerSalas` pasa de devolver `List<Sala>` a `IQueryable<Sala>`, con `var` el cambio se propaga en silencio a través de las variables locales. Es el argumento más fuerte de los tres y el menos popular.

La discusión no se resuelve sola porque los tres tienen razón sobre distintas cosas y ninguno tiene evidencia empírica de su lado. **Esta guía recomienda** seguir `N-06` por una razón que no es de fondo: es lo que las herramientas traen configurado, es lo que el ecosistema espera, y es la opción que menos explicación requiere a alguien que se incorpora. Lo que no conviene bajo ninguna posición es dejar la decisión sin tomar; un repositorio con los tres criterios conviviendo tiene el costo de todos y el beneficio de ninguno.

Las opciones del `.editorconfig` que lo declaran:

```ini
[*.cs]
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion
```

Severidad `suggestion` y no `warning` es deliberado: la regla depende de un juicio que la herramienta no puede emitir bien, y elevarla a aviso produce falsos positivos que erosionan la confianza en todos los demás avisos.

---

## Miembros con cuerpo de expresión

La flecha `=>` reemplaza un bloque cuando el cuerpo es una única expresión. El IL resultante es el mismo.

```csharp
// Aclara: la propiedad es una proyección directa.
public bool EstaVencida => _reloj.GetUtcNow() > Periodo.Fin;

// Aclara: el método es una delegación de una línea.
public Task<Sala?> ObtenerAsync(Guid id, CancellationToken ct)
    => _repositorio.BuscarPorIdAsync(id, ct);
```

Donde deja de aclarar es cuando la expresión única no es simple. Una única expresión puede contener tres operadores ternarios anidados y seguir siendo una única expresión; el lenguaje lo permite y el resultado es ilegible.

```csharp
// Comprime de más: es una expresión, y no se puede leer.
public string Estado => EstaCancelada ? "Cancelada"
    : EstaConfirmada ? (EstaVencida ? "Finalizada" : "Confirmada")
    : "Pendiente";
```

**Esta guía recomienda** el cuerpo de expresión para propiedades de solo lectura que proyectan estado, para métodos que delegan, y para constructores primarios triviales; y el bloque en cuanto la expresión requiera leerse dos veces. El criterio operativo: si el cuerpo de expresión ocupa más de una línea física, probablemente convenga un bloque.

Un caso aparte son los **constructores** con cuerpo de expresión. Un constructor con una sola asignación se escribe así sin problema, pero en cuanto haya validación de argumentos —que es lo habitual— el bloque es la única opción razonable.

```ini
[*.cs]
csharp_style_expression_bodied_properties = true:suggestion
csharp_style_expression_bodied_methods = when_on_single_line:suggestion
csharp_style_expression_bodied_constructors = false:suggestion
```

---

## Tipos de referencia anulables

Esto no es estilo. Es corrección, y merece tratarse en otra categoría que todo lo anterior.

Activar `<Nullable>enable</Nullable>` cambia el significado de cada declaración de tipo de referencia del proyecto. `Sala` pasa a significar «una sala, nunca nula» y `Sala?` pasa a significar «puede ser nula». El compilador verifica el flujo y emite avisos donde el contrato se rompe: una asignación de nulo a un no-anulable, una desreferencia de un anulable sin comprobación, un campo no inicializado en el constructor.

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

### Impacto sobre las firmas

El efecto más importante no está en el cuerpo de los métodos sino en las firmas, porque ahí el anotado se vuelve documentación verificada.

```csharp
// Sin anulables: la firma no dice nada. Hay que leer el cuerpo o adivinar.
Task<Sala> BuscarPorIdAsync(Guid id, CancellationToken ct);

// Con anulables: la firma dice que puede no encontrarla, y el compilador
// obliga al llamador a contemplarlo.
Task<Sala?> BuscarPorIdAsync(Guid id, CancellationToken ct);

// Y esta otra dice que si no la encuentra, lanza.
Task<Sala> ObtenerPorIdAsync(Guid id, CancellationToken ct);
```

Las tres firmas describen operaciones distintas, y sin anulables las tres se escriben igual. Esa es la ganancia: una parte del contrato que antes vivía en un comentario XML —o en ningún lado— pasa a estar en el tipo y a verificarse en compilación.

### El operador `!` y por qué conviene vigilarlo

El operador de perdón de nulo, `!`, le dice al compilador «confía en mí, esto no es nulo». Existe porque el análisis de flujo no es perfecto y a veces el desarrollador sabe algo que el compilador no puede deducir. También es la vía por la que se desactiva la verificación en silencio.

```csharp
// Legítimo: el patrón ya garantizó que no es nulo, pero el análisis de flujo
// no lo propaga a través del método auxiliar.
var sala = ObtenerSalaValidada(id)!;

// Sospechoso: se está silenciando un aviso en lugar de resolverlo.
var nombre = reserva.Sala!.Nombre!;
```

**Esta guía recomienda** tratar cada `!` como una excepción que merece un comentario de una línea explicando por qué el compilador se equivoca. Un archivo con quince `!` no tiene los anulables activados: tiene los avisos apagados con más pasos.

### En `ESC-3`

Activar anulables sobre un código existente produce cientos de avisos de golpe y por eso se pospone indefinidamente. La estrategia que funciona es la gradual: `<Nullable>enable</Nullable>` en los proyectos nuevos y en los de dominio, y para el resto, activación proyecto por proyecto, cada uno en su commit dedicado. La directiva `#nullable enable` a nivel de archivo permite una granularidad aún más fina cuando el proyecto es grande.

En `CTX-3` no es opcional. Una biblioteca sin anotaciones de nulabilidad obliga a todos sus consumidores a adivinar, y en un ecosistema donde el resto de los paquetes sí anota, la falta se nota de inmediato.

---

## `record`, `class` y `struct`

Tres formas de declarar un tipo, con semánticas de igualdad distintas. La elección tiene consecuencias observables.

| | `class` | `record` (`record class`) | `struct` | `record struct` |
|---|---|---|---|---|
| Ubicación | Montón | Montón | Pila o en línea | Pila o en línea |
| Igualdad predeterminada | Por referencia | **Por valor** | Por valor (con costo) | **Por valor** |
| Mutabilidad idiomática | Mutable | Inmutable | Preferentemente inmutable | Preferentemente inmutable |
| `with` | No | Sí | No | Sí |
| Uso típico | Entidades, servicios | DTO, mensajes, objetos de valor | Valores pequeños | Valores pequeños comparables |

La regla práctica: **si dos instancias con el mismo contenido deben considerarse la misma cosa, es un `record`**. Si dos instancias con el mismo contenido son cosas distintas porque tienen identidad propia, es una `class`.

```csharp
// Objeto de valor: dos rangos con los mismos extremos son el mismo rango.
public readonly record struct RangoHorario(DateTimeOffset Inicio, DateTimeOffset Fin)
{
    public TimeSpan Duracion => Fin - Inicio;

    public bool SeSolapaCon(RangoHorario otro) => Inicio < otro.Fin && otro.Inicio < Fin;
}

// Contrato de entrada: es un mensaje, no tiene identidad.
public sealed record CrearReservaComando(
    Guid SalaId,
    RangoHorario Periodo,
    IReadOnlyList<string> Asistentes);

// Entidad: dos reservas con los mismos datos son reservas distintas.
public sealed class Reserva
{
    public Guid Id { get; private init; }
    public EstadoReserva Estado { get; private set; }
    // ...
}
```

Hay una trampa conocida en la igualdad por valor de los `record`: se genera miembro a miembro, y si un miembro es una colección, la comparación es **por referencia de la colección**, no por su contenido. `CrearReservaComando` con dos listas de asistentes idénticas pero distintas instancias no es igual a sí mismo. Es la clase de detalle que hace que estas decisiones no sean estilo.

Sobre `struct`, el criterio de las Framework Design Guidelines (`N-01`..`N-04`, con la salvedad de que fueron escritas para bibliotecas y datan de 2008) sigue vigente en lo esencial: tipos de valor pequeños, lógicamente representables como un valor único, inmutables, y que no se pasen por valor con frecuencia si son grandes. Fuera de ese perfil, `class`.

---

## Expresiones `switch` y coincidencia de patrones

La expresión `switch` reemplaza a la sentencia cuando lo que se hace es **producir un valor** en función de una forma. La diferencia con la sentencia no es cosmética: la expresión es exhaustiva por construcción, y el compilador avisa si falta un caso.

```csharp
// Sentencia: se ejecutan efectos.
switch (reserva.Estado)
{
    case EstadoReserva.Pendiente:
        _notificador.RecordarConfirmacion(reserva);
        break;
    case EstadoReserva.Confirmada:
        _notificador.EnviarInvitaciones(reserva);
        break;
}

// Expresión: se produce un valor, y la falta de un caso es un aviso.
var mensaje = reserva.Estado switch
{
    EstadoReserva.Pendiente   => "Pendiente de confirmación",
    EstadoReserva.Confirmada  => "Confirmada",
    EstadoReserva.Cancelada   => "Cancelada",
    _ => throw new ArgumentOutOfRangeException(nameof(reserva))
};
```

La coincidencia de patrones se vuelve valiosa cuando la condición combina tipo, valor y propiedad. Comparado con una cadena de `if` anidados, el patrón hace visible la estructura de casos:

```csharp
public static Resultado Clasificar(Solicitud solicitud) => solicitud switch
{
    { Asistentes: 0 } => Resultado.Rechazada("Sin asistentes"),
    { Periodo.Duracion.TotalMinutes: < 15 } => Resultado.Rechazada("Muy corta"),
    { Sala.Aforo: var aforo, Asistentes: var n } when n > aforo
        => Resultado.Rechazada("Excede el aforo"),
    _ => Resultado.Aceptada()
};
```

El límite: un `switch` de veinte brazos con guardas anidadas es una tabla de decisión disfrazada, y probablemente el dominio pida un tipo que la represente.

### `required` e inicializadores de objeto

`required` obliga al llamador a asignar la propiedad en el inicializador, y el compilador lo verifica. Cubre el hueco entre el constructor —que garantiza pero obliga a un parámetro posicional— y el inicializador de objeto, que es legible pero no garantizaba nada.

```csharp
public sealed class ConfiguracionReservas
{
    public required TimeSpan DuracionMinima { get; init; }
    public required TimeSpan DuracionMaxima { get; init; }
    public int MaximoAsistentes { get; init; } = 50;
}

// No compila si falta DuracionMaxima.
var configuracion = new ConfiguracionReservas
{
    DuracionMinima = TimeSpan.FromMinutes(15),
    DuracionMaxima = TimeSpan.FromHours(4)
};
```

Es de las incorporaciones recientes con mejor relación entre costo y beneficio, especialmente para tipos de configuración y de opciones, donde el constructor con ocho parámetros posicionales era la alternativa.

---

## `async` y `await`

### El sufijo `Async`

Convención de facto (`F-04`), establecida por el Task-based Asynchronous Pattern (`N-15`): los métodos que devuelven `Task` o `ValueTask` llevan sufijo `Async`. No es una norma del lenguaje y el compilador no la verifica; es lo que hace todo el ecosistema, incluida la biblioteca base.

```csharp
Task<Reserva> ConfirmarAsync(Guid reservaId, CancellationToken cancelacion);
```

La excepción admitida son los manejadores de eventos y los métodos de prueba, donde el sufijo no aporta. La convención tiene una crítica razonable —el tipo de retorno ya dice que es asíncrono— y sobrevive de todos modos porque en una lista de IntelliSense el sufijo se ve antes que el tipo.

### `ConfigureAwait`

Es el punto donde más desinformación circula, porque la respuesta correcta **cambió** y mucha documentación quedó del lado viejo.

En ASP.NET clásico (.NET Framework) existía un `SynchronizationContext` de petición. Una continuación `await` volvía a ese contexto, y una espera bloqueante sobre una tarea que a su vez esperaba volver al contexto producía interbloqueo. `ConfigureAwait(false)` era la mitigación, y por eso se convirtió en consejo universal.

**ASP.NET Core no instala `SynchronizationContext`.** Sin contexto al que volver, `ConfigureAwait(false)` no tiene efecto: la continuación se programa en el pool de hilos de todos modos. Escribirlo en cada `await` de una aplicación ASP.NET Core agrega ruido sin comprar nada.

Donde sigue importando es en `CTX-3`. Una biblioteca no sabe quién la va a hospedar, y puede ser consumida desde una aplicación WPF, WinForms o .NET MAUI, que sí instalan contexto. Ahí `ConfigureAwait(false)` en cada `await` interno es la práctica correcta, porque evita imponerle al consumidor el costo de volver al hilo de interfaz en cada punto de espera de la biblioteca.

| Contexto | `ConfigureAwait(false)` | Motivo |
|----------|-------------------------|--------|
| `CTX-1` Blazor Server / ASP.NET Core | Innecesario | Sin `SynchronizationContext` |
| `CTX-2` API o servicio .NET Core | Innecesario | Sin `SynchronizationContext` |
| `CTX-3` Biblioteca | **Recomendado** | El anfitrión es desconocido |
| WPF / WinForms / MAUI | Necesario fuera del código de interfaz | Hay contexto de interfaz |

### `async void`

Un método `async void` no devuelve nada a lo que esperar, y por lo tanto una excepción que escape de su cuerpo no se captura en ninguna tarea: se propaga al `SynchronizationContext` o, en su ausencia, termina el proceso. No hay forma de esperarlo, ni de saber cuándo terminó, ni de probarlo con comodidad.

La única excepción legítima son los manejadores de eventos, cuya firma lo exige. Fuera de ahí, `async Task`.

```csharp
// Defecto: si ProcesarAsync lanza, el proceso cae y el llamador no se entera.
public async void ProcesarEnSegundoPlano(Reserva reserva)
{
    await _notificador.EnviarAsync(reserva);
}

// Correcto: el llamador puede esperar y capturar.
public async Task ProcesarEnSegundoPlanoAsync(Reserva reserva)
{
    await _notificador.EnviarAsync(reserva);
}
```

El SDK no trae una regla propia que lo detecte; hacerlo requiere un paquete de analizadores adicional, `Microsoft.VisualStudio.Threading.Analyzers`, cuya regla `VSTHRD100` cubre el caso y ofrece corrección automática a `Task` (`N-16`). **Esta guía recomienda** incorporarlo y elevar esa regla a `error`, porque el modo de fallo es severo y el falso positivo, prácticamente nulo. Sin ese paquete, la regla depende de la revisión humana, que es exactamente lo que [`TEM-AUTO`](Automatizacion-del-Estilo.md) trata de evitar.

### Cancelación

Un método asíncrono que hace trabajo de entrada/salida acepta `CancellationToken` y lo propaga. Es convención más que norma, y su ausencia se paga en `CTX-2` cuando una petición cancelada por el cliente sigue consumiendo una conexión de base de datos hasta terminar. El parámetro va último, y **esta guía recomienda** no darle valor predeterminado en código de aplicación: el predeterminado convierte en invisible el caso en que alguien olvidó propagarlo.

---

## Sentencias `using` con declaración simplificada

La forma de declaración ata la vida del recurso al ámbito que la contiene, sin nivel de anidamiento adicional.

```csharp
// Forma clásica: un nivel de indentación por recurso.
public async Task<int> ContarAsync(CancellationToken ct)
{
    using (var conexion = _fabrica.Crear())
    {
        using (var comando = conexion.CrearComando())
        {
            return await comando.EjecutarEscalarAsync(ct);
        }
    }
}

// Declaración simplificada: se libera al salir del método, en orden inverso.
public async Task<int> ContarAsync(CancellationToken ct)
{
    using var conexion = _fabrica.Crear();
    using var comando = conexion.CrearComando();
    return await comando.EjecutarEscalarAsync(ct);
}
```

La segunda forma es preferible en la mayoría de los casos y `N-06` la favorece. El caso donde conviene la clásica es cuando el recurso debe liberarse **antes** del final del método —porque después hay trabajo que no lo necesita y la liberación temprana importa—; ahí el bloque explícito delimita la vida con precisión y la declaración simplificada no puede hacerlo.

---

## Namespaces con ámbito de archivo

La declaración con punto y coma en lugar de llaves elimina un nivel de indentación de todo el archivo.

```csharp
namespace Reservas.Aplicacion.Disponibilidad;

public sealed class ConsultaDisponibilidad
{
    // ...
}
```

El tratamiento completo —organización de espacios de nombres, correspondencia con la estructura de carpetas, nombre del ensamblado— está en [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md). Acá alcanza con registrar la opción de estilo y que la plantilla predeterminada del SDK ya la genera así:

```ini
[*.cs]
csharp_style_namespace_declarations = file_scoped:warning
```

---

## Sentencias de nivel superior

`Program.cs` puede escribirse sin clase ni método `Main`: el compilador genera una clase `Program` con un `Main` que contiene el código escrito en el archivo. Es lo que producen las plantillas de proyecto desde .NET 6.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IRepositorioSalas, RepositorioSalasEfCore>();

var app = builder.Build();
app.MapGet("/salas/disponibles", async (
    [AsParameters] ConsultaSalas consulta,
    ConsultaDisponibilidad servicio,
    CancellationToken ct) =>
{
    var salas = await servicio.EjecutarAsync(consulta.APeriodo(), ct);
    return Results.Ok(salas);
});

app.Run();
```

Lo que habilitan es un archivo de arranque sin ceremonia, donde la configuración del anfitrión se lee de arriba abajo. Lo que cuestan es que la clase generada es **`internal`**, y eso rompe un escenario concreto y frecuente: las pruebas de integración con `WebApplicationFactory<TEntryPoint>` necesitan referenciar `Program` como parámetro de tipo, y no pueden hacerlo si es interna al ensamblado del servicio.

La solución idiomática es declarar la clase parcial pública en la última línea del archivo. El compilador combina esa declaración con la que genera, y el tipo pasa a ser accesible desde el proyecto de pruebas:

```csharp
// Expone la clase Program para los tests de integración (WebApplicationFactory).
public partial class Program;
```

Es un patrón estándar del ecosistema, y el comentario importa tanto como la declaración: sin él, la última línea del archivo de arranque parece una clase vacía que alguien olvidó borrar.

La alternativa a la clase parcial pública es `[assembly: InternalsVisibleTo("...")]`, que expone todo lo interno del ensamblado al proyecto de pruebas en lugar de un único tipo. **Esta guía recomienda** la clase parcial: la superficie que abre es mínima y explícita, y el comentario de una línea explica por qué existe una declaración que de otro modo parece inútil.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

Es el único escenario donde estas decisiones son gratis. `<Nullable>enable</Nullable>` desde el primer proyecto no cuesta nada y después cuesta semanas; lo mismo vale para el criterio de `var` y para el uso de `record` en los contratos de entrada.

La decisión de mayor consecuencia y menor costo, en orden: anulables activados, `async void` como error, `record` para todo lo que sea mensaje o valor. Las tres se toman en una tarde del primer sprint y ahorran trabajo durante toda la vida del sistema.

### `ESC-2` — Evolución estructural

Aplica de forma indirecta. Cuando se extrae un módulo a su propio proyecto, ese proyecto nuevo es una oportunidad para nacer con anulables activados aunque el resto de la solución no los tenga: la configuración es por proyecto, y la frontera de la extracción es el momento natural para cambiarla.

Lo que sí conviene revisar en una extracción es `ConfigureAwait`. Un código que era de aplicación y pasa a ser biblioteca compartida cambia de contexto: lo que era innecesario pasa a ser recomendado.

### `ESC-3` — Normalización de código existente

Estas reglas se normalizan mucho peor que el formato, y conviene declararlo. `dotnet format` puede convertir todos los namespaces a ámbito de archivo y todos los `using` a declaración simplificada sin riesgo. No puede decidir dónde `var` es apropiado, ni convertir una `class` en `record`, ni resolver los avisos de nulabilidad, porque cada uno de esos cambios requiere entender qué hace el código.

El orden que esta guía recomienda para un `ESC-3` de este documento: primero las reglas que la herramienta aplica sola y verifica sola —namespaces, `using` simplificados, orden de modificadores—; después `async void`, que es acotado y de alto valor; y por último los anulables, proyecto por proyecto, con un commit dedicado a cada uno. Intentar los tres grupos a la vez produce un cambio irrevisable.

### `ESC-4` — Evaluación de código ajeno

Las señales útiles no son las preferencias sino las ausencias. Un proyecto sin `<Nullable>enable</Nullable>` en 2026 indica o bien un código anterior a .NET 6 que nunca se actualizó, o bien un equipo que no priorizó una mejora barata. Un `async void` fuera de un manejador de eventos es un defecto latente, no una preferencia de estilo, y se señala como tal. Un archivo con muchos `!` indica que los anulables están activados y silenciados.

Lo que no corresponde señalar: el criterio de `var`, la elección de cuerpos de expresión y la presencia de `ConfigureAwait(false)` innecesario en una aplicación ASP.NET Core. Los tres son preferencia o inercia, y ninguno afecta al comportamiento.

### Variación por contexto

**`CTX-1`.** Blazor introduce sus propias particularidades: los manejadores de eventos de componente admiten `async Task` en la mayoría de los casos —a diferencia de los eventos clásicos de .NET, que fuerzan `void`—, y `EventCallback` está diseñado para eso. Es un buen lugar para evitar `async void` incluso donde otros marcos lo obligan.

**`CTX-2`.** La propagación de `CancellationToken` desde el endpoint hasta el acceso a datos es donde este documento tiene su efecto más medible. Un servicio que no propaga el token sigue trabajando para clientes que ya se fueron.

**`CTX-3`.** Cambia el peso de casi todo: anulables dejan de ser recomendables para ser obligatorios, `ConfigureAwait(false)` pasa a ser necesario, y la elección entre `class` y `record` deja de ser interna porque el `record` público expone `with`, la igualdad por valor y un `ToString` generado, todo lo cual es contrato.

**`CTX-4`.** Los `record` son la forma natural de los contratos de mensaje entre servicios, con la advertencia de la igualdad por valor de las colecciones. Y hay una trampa de versionado: agregar una propiedad a un `record` posicional público cambia el constructor primario, y eso es un cambio ruptor aunque no lo parezca.

---

## Ejemplos concretos

### Un caso de uso completo

Sistema de reserva de salas, ejemplo sintético, con las convenciones de este documento aplicadas.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reservas.Aplicacion.Reservas;

public sealed record ConfirmarReservaComando(Guid ReservaId, string ConfirmadoPor);

public sealed class ConfirmarReservaManejador
{
    private readonly IRepositorioReservas _reservas;
    private readonly TimeProvider _reloj;

    public ConfirmarReservaManejador(IRepositorioReservas reservas, TimeProvider reloj)
    {
        _reservas = reservas;
        _reloj = reloj;
    }

    public async Task<ResultadoReserva> EjecutarAsync(
        ConfirmarReservaComando comando,
        CancellationToken cancelacion)
    {
        // Tipo anulable en la firma del repositorio: el compilador obliga a
        // contemplar el caso de no encontrada.
        Reserva? reserva = await _reservas.BuscarPorIdAsync(comando.ReservaId, cancelacion);

        if (reserva is null)
        {
            return ResultadoReserva.NoEncontrada(comando.ReservaId);
        }

        // Expresión switch: exhaustiva, y el compilador avisa si se agrega un
        // estado nuevo al enum sin contemplarlo acá.
        return reserva.Estado switch
        {
            EstadoReserva.Confirmada => ResultadoReserva.YaConfirmada(reserva),
            EstadoReserva.Cancelada => ResultadoReserva.Cancelada(reserva),
            EstadoReserva.Pendiente when reserva.Periodo.Fin <= _reloj.GetUtcNow()
                => ResultadoReserva.Vencida(reserva),
            EstadoReserva.Pendiente => await ConfirmarAsync(reserva, comando, cancelacion),
            _ => throw new ArgumentOutOfRangeException(nameof(comando))
        };
    }

    private async Task<ResultadoReserva> ConfirmarAsync(
        Reserva reserva,
        ConfirmarReservaComando comando,
        CancellationToken cancelacion)
    {
        reserva.Confirmar(comando.ConfirmadoPor, _reloj.GetUtcNow());
        await _reservas.GuardarAsync(reserva, cancelacion);
        return ResultadoReserva.Confirmada(reserva);
    }
}
```

Lo que demuestra: el `Reserva?` explícito en lugar de `var` porque la nulabilidad es el punto de la línea; el `switch` como expresión porque produce un valor; el `CancellationToken` propagado hasta el repositorio; el comando como `record` porque es un mensaje sin identidad; el sufijo `Async` en todo lo que devuelve `Task`.

### El cierre del archivo de arranque

El mismo `Program.cs` del servicio de reservas —sentencias de nivel superior, endpoints Minimal API, lógica delegada a la capa de aplicación— termina con la declaración que expone el tipo generado a las pruebas:

```csharp
// Expone la clase Program para los tests de integración (WebApplicationFactory).
public partial class Program;
```

Dos líneas que resumen la mitad de este documento. La convención resuelve un problema concreto —la clase generada por las sentencias de nivel superior es `internal`— y el comentario aporta exactamente lo que [`TEM-FORMATO`](Formato-y-Llaves.md) pide de un comentario: explica **por qué** existe una línea que sin explicación parecería inútil. Un comentario que dijera «declara la clase Program como parcial y pública» no habría aportado nada.

---

## Preguntas guía

1. ¿Esta decisión cambia lo que el programa hace, o solo cómo se ve? Si cambia lo que hace, no se resuelve con «lo importante es ser uniforme».
2. ¿Los tipos de referencia anulables están activados en este proyecto? Si no, ¿cuál es el plan y cuál el motivo?
3. ¿Cuántos `!` hay en este archivo, y cada uno tiene una razón escrita?
4. ¿Este tipo tiene identidad propia o es un valor? La respuesta decide entre `class` y `record`.
5. ¿Hay algún `async void` que no sea un manejador de eventos?
6. ¿Estoy en `CTX-3`? Si sí, `ConfigureAwait(false)` es necesario; si no, probablemente sobre.
7. ¿El `CancellationToken` de la firma llega hasta la operación de entrada/salida, o se pierde en el camino?
8. Si este `Program.cs` usa sentencias de nivel superior, ¿está expuesto a las pruebas de integración?
9. ¿El criterio de `var` está decidido, o cada archivo del repositorio expresa la preferencia de quien lo escribió?

---

## Criterios de calidad

### Cómo se distingue una aplicación buena de una pobre

**Buena.** Las decisiones con consecuencia semántica están tomadas y verificadas por el compilador: anulables activados, `async void` como error, cancelación propagada. Las decisiones sin consecuencia están declaradas en `.editorconfig` con severidad baja y nadie las discute. La distinción entre ambos grupos es visible en el propio archivo de configuración, porque las severidades la reflejan.

**Pobre.** Todo con la misma severidad, de modo que el aviso de un `async void` y el de un `var` mal usado compiten por la misma atención y ambos se ignoran. O el caso inverso: anulables desactivados y una discusión de tres semanas sobre cuerpos de expresión.

La prueba: **¿alguna de las reglas de estilo de este repositorio previene un defecto de producción, y está configurada con más severidad que las que no?** Si todas están igual, no se hizo la distinción.

### Antipatrones

**El anulable silenciado.** `<Nullable>enable</Nullable>` activado y cuarenta `!` repartidos por el proyecto. Se pagó el costo de activarlo y no se cobró el beneficio.

**El `ConfigureAwait` de cargo.** `.ConfigureAwait(false)` en cada `await` de una aplicación ASP.NET Core, copiado de documentación de .NET Framework. No causa daño y agrega ruido en cada línea; el problema real es que revela que nadie entendió por qué existe, y ese mismo desconocimiento hace que falte en la biblioteca donde sí hacía falta.

**El `async void` fuera de un manejador.** Excepciones que derriban el proceso sin dejar rastro en el registro del llamador. No es preferencia de estilo: es un defecto.

**El `record` mutable.** Un `record` con propiedades `set` públicas. Conserva la igualdad por valor y pierde la garantía que la hace segura: el objeto puede cambiar después de haberse usado como clave de un diccionario, y el diccionario deja de encontrarlo.

**El `switch` como tabla de decisión.** Veinte brazos con guardas anidadas en una expresión. Lo que el código pide es un tipo del dominio que represente la regla, no más sintaxis.

**La cancelación decorativa.** Un `CancellationToken` en la firma que el cuerpo nunca propaga. Es peor que no tenerlo, porque el llamador cree que puede cancelar.

**El `Program` sin exponer.** Sentencias de nivel superior sin `public partial class Program;`, y como consecuencia, pruebas de integración escritas contra el ensamblado de otra manera —o, más frecuentemente, pruebas de integración que no se escribieron.

**La adopción por novedad.** Reescribir código funcionando para usar la construcción sintáctica de la última versión del lenguaje. El cambio no aporta y consume revisión.
