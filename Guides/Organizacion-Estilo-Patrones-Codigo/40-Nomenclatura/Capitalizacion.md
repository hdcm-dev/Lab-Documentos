---
doc_id: TEM-CAPS
doc_type: tema
title: Capitalización
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-NOM, TEM-NOMB, TEM-ANTI, TEM-AUTO, TEM-NS, TEM-MODELOS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS, ANEXO-PLANTILLAS]
---

# Capitalización — `TEM-CAPS`

## Resumen ejecutivo

C# distingue mayúsculas de minúsculas, y .NET aprovecha esa propiedad para codificar información en la forma de cada identificador. `Reserva` es un tipo, `reserva` una variable local, `_reserva` un campo privado de instancia: tres cosas distintas que se distinguen sin abrir ninguna declaración. Ese es todo el valor de la convención, y explica por qué conviene aplicarla incluso donde no hay contrato que proteger.

Microsoft especifica el esquema en `N-02` para la superficie pública de bibliotecas y en `N-05` para los identificadores del lenguaje. Entre ambas cubren casi todo, y lo que dejan afuera —prefijos de campos privados, sufijos de métodos asíncronos— lo cubre el ecosistema con convenciones de facto muy estables (`F-02`, `F-03`, `F-04`) que esta guía distingue con cuidado, porque atribuirlas a Microsoft es un error corriente.

El documento nombra cada técnica con su denominación habitual —PascalCase, camelCase, `SCREAMING_SNAKE_CASE`, snake_case, kebab-case— porque en una discusión de equipo el nombre es lo que permite acordar en una línea en lugar de en un párrafo. Y porque tres de esas cinco no se usan en identificadores C# pero sí aparecen en el ecosistema .NET, en rutas HTTP, en JSON y en el esquema de la base.

---

## Definición

### Qué es

Una convención de capitalización es una regla que asigna, a cada **categoría** de identificador, una forma de escribirlo cuando el identificador se compone de varias palabras. Las categorías son las del lenguaje: tipo, método, propiedad, parámetro, variable local, campo, constante, espacio de nombres, parámetro genérico.

Las cinco técnicas relevantes, con su nombre:

| Técnica | Nombre alterno | Forma | Ejemplo |
|---------|----------------|-------|---------|
| **PascalCase** | upper camel case | Cada palabra en mayúscula inicial, sin separadores | `ReservaDeSala` |
| **camelCase** | lower camel case, dromedary case | Igual, salvo la primera palabra en minúscula | `reservaDeSala` |
| **SCREAMING_SNAKE_CASE** | `UPPER_CASE`, macro case, constant case | Todo mayúscula, palabras separadas por guion bajo | `RESERVA_DE_SALA` |
| **snake_case** | — | Todo minúscula, palabras separadas por guion bajo | `reserva_de_sala` |
| **kebab-case** | dash case, spinal case | Todo minúscula, palabras separadas por guion medio | `reserva-de-sala` |

En identificadores de C#, .NET usa **solo las dos primeras**. Las otras tres aparecen en el ecosistema, pero fuera del código C#.

### Qué problema resuelve

Reduce el costo de leer código ajeno a cero en una dimensión concreta. Sin convención, saber si `total` es un parámetro, un campo o una propiedad exige buscar su declaración; con convención, la forma lo dice. En un archivo de doscientas líneas esa diferencia es menor. Multiplicada por todas las lecturas de todo el código de un sistema durante años, no lo es.

El segundo problema que resuelve es más específico de .NET y explica por qué el esquema es como es. La plataforma admite lenguajes que **no** distinguen mayúsculas de minúsculas —Visual Basic es el caso vivo—. Si un tipo expusiera una propiedad `Total` y un campo público `total`, un consumidor en VB no podría distinguirlos. `N-02` recomienda por eso que la superficie pública no dependa de la diferencia de caja para desambiguar dos miembros, y el esquema PascalCase para todo lo público hace que la situación no llegue a plantearse.

### Qué NO es, y con qué se lo confunde

**No es el nombrado.** Capitalización responde a «cómo se escribe»; la elección de las palabras es [`TEM-NOMB`](Nombrado-de-Tipos-y-Miembros.md). `ProcesarDatos1` está impecablemente capitalizado y es un mal nombre.

**No es el estilo de formato.** Llaves, sangría, espacios y saltos de línea son [`FAM-EST`](../50-Estilo-de-Codificacion/README.md). Se automatizan con el mismo archivo pero no son la misma cosa.

**No es una regla del compilador.** C# compila `class reserva_de_sala` sin una queja. Todo lo de este documento es convención verificada por analizadores, nunca por el compilador, y esa es exactamente la razón por la que hace falta configurar los analizadores ([`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)).

**No es transferible desde otros lenguajes.** Quien viene de Java, C o Python trae un esquema para constantes que en .NET no rige, y es el punto donde más se equivoca. Tiene sección propia más abajo.

---

## La tabla normativa — elemento por elemento

`N-02` fija la capitalización de la superficie pública; `N-05` extiende el criterio a los identificadores internos del lenguaje. La síntesis:

| Elemento | Convención | Ejemplo | Nivel |
|----------|-----------|---------|-------|
| Espacio de nombres | PascalCase | `Reservas.Dominio.Salas` | Normativo `N-02` |
| Ensamblado / proyecto | PascalCase, igual al espacio de nombres raíz | `Reservas.Dominio` | `N-02` + `F-05` |
| Clase, struct, record | PascalCase | `ReservaDeSala` | Normativo `N-02` |
| Interfaz | PascalCase con prefijo `I` | `IRepositorioSalas` | Normativo `N-01` |
| Delegado | PascalCase | `ManejadorDeConflicto` | Normativo `N-02` |
| Enumeración | PascalCase | `EstadoReserva` | Normativo `N-02` |
| Valor de enumeración | PascalCase | `EstadoReserva.Confirmada` | Normativo `N-02` |
| Método | PascalCase | `CancelarReserva` | Normativo `N-02` |
| Método asíncrono | PascalCase + sufijo `Async` | `CancelarReservaAsync` | `F-04` |
| Propiedad | PascalCase | `FechaInicio` | Normativo `N-02` |
| Evento | PascalCase | `ReservaConfirmada` | Normativo `N-02` |
| Campo público o protegido | PascalCase | `MinValue` | Normativo `N-02` |
| **Constante (`const`)** | **PascalCase** | `DuracionMaximaHoras` | Normativo `N-02` |
| `static readonly` | PascalCase | `Empty` | Normativo `N-02` |
| Campo privado de instancia | `_camelCase` | `_repositorio` | Convención `F-02`, documentada en `N-05` |
| Campo privado estático | `s_camelCase` | `s_cache` | Convención `F-03` — estilo `dotnet/runtime` |
| Campo `[ThreadStatic]` | `t_camelCase` | `t_contexto` | Práctica de `dotnet/runtime` documentada solo en prosa; a diferencia de `s_`, no está declarada como regla en su `.editorconfig` |
| Parámetro | camelCase | `salaId` | Normativo `N-02` |
| Variable local | camelCase | `reservaPendiente` | `N-05` |
| Constante local | camelCase | `const int maximo = 8;` | `N-05` |
| Parámetro genérico | PascalCase con prefijo `T` | `TResultado` | Normativo `N-01` |
| Atributo (tipo) | PascalCase + sufijo `Attribute` | `ValidarDisponibilidadAttribute` | Normativo `N-01` |
| Excepción (tipo) | PascalCase + sufijo `Exception` | `SalaNoDisponibleException` | Normativo `N-01` |

Un detalle sobre la fila de constantes locales. `N-05` las trata como variables locales, en camelCase, mientras que las constantes que son **campos** van en PascalCase por `N-02`. Es la única inconsistencia aparente del esquema y tiene explicación: `N-02` gobierna lo que se ve desde afuera del tipo, y una constante local no se ve desde ningún lado.

---

## Las constantes: la sorpresa para quien llega de otro lenguaje

**En .NET las constantes son PascalCase.** No `SCREAMING_SNAKE_CASE`. `N-02` no hace ninguna excepción para `const`: un campo constante es un campo, y los campos públicos van en PascalCase.

```csharp
// Correcto en .NET
public const int DuracionMaximaHoras = 8;
public const string PrefijoCodigoReserva = "RES-";
public static readonly TimeSpan AnticipacionMinima = TimeSpan.FromMinutes(15);

// Incorrecto en .NET, aunque sea lo normal en Java, C o Python
public const int DURACION_MAXIMA_HORAS = 8;
public const string PREFIJO_CODIGO_RESERVA = "RES-";
```

La biblioteca base lo confirma en cada tipo que uno abra: `int.MaxValue`, `string.Empty`, `Math.PI`, `TimeSpan.Zero`. Ninguno grita.

El origen de la confusión es histórico y vale conocerlo, porque desactiva la discusión. En C, `#define MAX 100` es una directiva del preprocesador que sustituye texto antes de compilar; la mayúscula servía para advertir al lector que ese identificador no era una variable ni respetaba el ámbito, y que un uso descuidado podía producir sustituciones inesperadas. Era una señal de peligro, no una señal de constancia. Java heredó la forma sin heredar el preprocesador, y de ahí pasó a buena parte de la industria. En .NET, `const` es una construcción del lenguaje con tipo, ámbito y verificación del compilador: no hay peligro del que advertir, y la convención no lo señala.

Quien insista igual tiene un argumento legítimo —la constancia es información útil al leer— y una respuesta práctica: en .NET esa información la aporta el IDE, que colorea las constantes de forma distinta, y el costo de desviarse es que todo el código propio se lea distinto de todo el código del framework que lo rodea.

---

## Las técnicas que .NET no usa en C#, y dónde sí aparecen

Que `snake_case` y `kebab-case` no se usen en identificadores no significa que no aparezcan. Un sistema .NET real las produce todos los días en sus fronteras, y confundir el plano del código con el plano del contrato es de los errores más caros de esta familia, porque lo que se publica en una API queda atado a sus consumidores.

**kebab-case en rutas HTTP.** Es la convención dominante de la web para segmentos de URL, y no hay razón para exportar PascalCase a una ruta.

```csharp
// La ruta sigue la convención de su medio; el método, la de C#.
app.MapGet("/api/salas-disponibles/{salaId:guid}", ObtenerSalasDisponiblesAsync);

// Filtra la convención de C# hacia un plano que no le corresponde.
app.MapGet("/api/SalasDisponibles/{SalaId:guid}", ObtenerSalasDisponiblesAsync);
```

**camelCase en JSON.** `System.Text.Json` en ASP.NET Core aplica por defecto `JsonNamingPolicy.CamelCase` a los nombres de propiedad, de modo que una propiedad `FechaInicio` se serializa como `fechaInicio` sin que nadie lo configure. La política se puede cambiar, y ahí está el riesgo: quien la desactiva para «que coincida con el modelo» publica un contrato en PascalCase que después nadie quiere tocar.

```csharp
public sealed record ReservaDto(Guid SalaId, DateTimeOffset FechaInicio);
// Se serializa como: { "salaId": "...", "fechaInicio": "..." }
```

**snake_case en serializaciones y en la base.** Aparece en dos lugares distintos. En integraciones con sistemas que lo exigen —muchas APIs de origen Python o Ruby, y los estándares OAuth y OpenID Connect, con sus `access_token`, `refresh_token`, `client_id`— la solución es una política de nombres explícita, no renombrar las propiedades del modelo:

```csharp
// .NET 8 incorporó JsonNamingPolicy.SnakeCaseLower.
var opciones = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};
```

Y aparece en el esquema de base de datos cuando el motor es PostgreSQL, donde snake_case es la convención del ecosistema porque los identificadores sin comillas se pliegan a minúscula. Con EF Core se resuelve en la configuración del modelo, sin contaminar las entidades:

```csharp
modelBuilder.Entity<Reserva>().ToTable("reserva")
    .Property(r => r.FechaInicio).HasColumnName("fecha_inicio");
```

**`SCREAMING_SNAKE_CASE` en variables de entorno.** Es la convención de facto de POSIX y la que usa la configuración de .NET: `ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT`. El doble guion bajo `__` funciona además como separador de sección en el proveedor de configuración por variables de entorno, de modo que `ConnectionStrings__Reservas` se lee como `ConnectionStrings:Reservas`.

La regla que gobierna los cuatro casos: **cada plano sigue la convención de su medio, y la traducción entre planos es explícita y vive en un solo lugar.** Un tipo C# en PascalCase, serializado a camelCase por una política, expuesto en una ruta kebab-case y persistido en columnas snake_case es correcto en los cuatro planos a la vez.

---

## Acrónimos y siglas

`N-02` fija una regla que casi nadie recuerda y que se viola con frecuencia, incluso en código que por lo demás es impecable. Depende del **largo del acrónimo**:

**Dos letras: ambas en mayúscula.** `IO`, `UI`, `DB`, `IP`, `OK`.

```csharp
public class IOStream { }
public UIElement ElementoRaiz { get; }
```

**Tres letras o más: PascalCase, solo la primera en mayúscula.** El acrónimo se trata como una palabra común.

```csharp
// Correcto
public class XmlDocument { }
public class HtmlSanitizer { }
public HttpClient Cliente { get; }
public string ObtenerUrlCanonica() => ...;

// Incorrecto, aunque se ve todo el tiempo
public class XMLDocument { }
public class HTMLSanitizer { }
public HTTPClient Cliente { get; }
public string ObtenerURLCanonica() => ...;
```

La biblioteca base es consistente con su propia regla y sirve de verificación: `XmlWriter`, `HtmlEncoder`, `HttpContext`, `JsonSerializer`, `UriBuilder`, `SqlConnection`. Frente a `System.IO`, de dos letras y por eso en mayúscula. La única razón por la que la regla parece arbitraria es que uno la aprende leyendo casos sueltos en lugar de leyendo el criterio.

**En camelCase, un acrónimo de dos letras al principio va todo en minúscula.** No hay forma de escribir `IOStream` en camelCase respetando ambas mayúsculas sin producir un identificador ilegible:

```csharp
public void Procesar(Stream ioStream, XmlReader xmlReader, Uri uriBase) { }
```

**`Id` es un caso aparte.** No es un acrónimo sino la abreviatura de *identifier*, y por eso se escribe `Id` y no `ID`. La biblioteca base y ASP.NET Core lo tratan así de manera uniforme —`ManagedThreadId`, `TraceIdentifier`, `UserId`— y esta guía recomienda seguir esa práctica: `SalaId`, `ReservaId`, `IdUsuario`. Es observación de la práctica del ecosistema, no una regla explícita de `N-02`.

**Un acrónimo propio de la organización se trata igual.** Si el negocio llama `SGR` al sistema de gestión de reservas, el tipo es `SgrCliente`, no `SGRCliente`. Es incómodo la primera semana y consistente para siempre.

---

## Prefijos y sufijos, con su nivel de autoridad

Esta sección existe porque la atribución equivocada es endémica. Cuatro prefijos y un sufijo conviven en cualquier repositorio .NET moderno, y provienen de cuatro fuentes distintas con cuatro pesos distintos.

### `I` en interfaces — normativo (`N-01`)

`N-01` prescribe que los nombres de interfaz lleven el prefijo `I`, y es la única concesión que las Framework Design Guidelines hacen a la codificación de tipo en el nombre —lo mismo que prohíben bajo el rótulo de notación húngara en `N-03`—. La inconsistencia es conocida, está reconocida por los propios autores y no cambia nada: la convención está tan asentada en toda la plataforma que desviarse solo genera fricción.

```csharp
public interface IRepositorioSalas { }
public interface IServicioNotificaciones { }
```

### `T` en parámetros genéricos — normativo (`N-01`)

Un parámetro genérico único se llama `T`. Cuando hay más de uno, o cuando el papel del parámetro merece explicarse, se usa `T` seguido de un nombre descriptivo en PascalCase.

```csharp
public interface IRepositorio<T> { }
public interface ICache<TClave, TValor> { }
public delegate TResultado Proyeccion<TOrigen, TResultado>(TOrigen origen);
public interface IManejador<TComando, TRespuesta> { }
```

Los nombres canónicos de la biblioteca base son `T`, `TKey`, `TValue`, `TResult`, `TSource`, `TElement`. Un equipo que nombra su dominio en español enfrenta acá la primera fricción concreta de esa decisión, tratada en [`TEM-NOMB`](Nombrado-de-Tipos-y-Miembros.md).

Lo que `N-01` sí desaconseja es el parámetro genérico de una sola letra cuando el tipo tiene varios: `IDictionary<K, V>` obliga a recordar cuál es cuál, `IDictionary<TKey, TValue>` no.

### `_` en campos privados de instancia — convención de facto (`F-02`)

El prefijo de guion bajo con camelCase para campos privados de instancia es la práctica dominante del ecosistema y está documentado en `N-05`. **No** está en las Framework Design Guidelines, y por una razón estructural: `N-01` a `N-04` gobiernan la superficie pública de una biblioteca, y un campo privado no forma parte de ella.

```csharp
public sealed class ServicioReservas
{
    private readonly IRepositorioSalas _repositorio;
    private readonly ILogger<ServicioReservas> _registro;

    public ServicioReservas(IRepositorioSalas repositorio, ILogger<ServicioReservas> registro)
    {
        _repositorio = repositorio;
        _registro = registro;
    }
}
```

Lo que compra el prefijo es que desaparezca la ambigüedad entre campo y parámetro en el cuerpo del constructor, sin recurrir a `this.`. Ambas soluciones son legítimas; lo que no es legítimo es que convivan en el mismo repositorio.

### `s_` y `t_` — estilo de `dotnet/runtime` (`F-03`), no de Microsoft como norma general

**Este es el error de atribución más frecuente de toda la familia.** Los prefijos `s_` para campos privados estáticos y `t_` para campos marcados `[ThreadStatic]` provienen del repositorio `dotnet/runtime` (`O-08`). No están en `N-01` a `N-04`, no son el comportamiento predeterminado de Visual Studio, y la documentación de C# lo aclara de forma literal.

Los dos prefijos no tienen el mismo respaldo, y la asimetría conviene declararla en lugar de suponerla. `s_` está declarado como regla `dotnet_naming_rule` en el `.editorconfig` del repositorio: es verificable y se aplica sola. `t_` aparece únicamente en la prosa de sus guías de codificación; no se encontró una regla que lo exprese. Un equipo que adopte `t_` está adoptando una costumbre escrita, no una regla que alguna herramienta vaya a hacer cumplir.

```csharp
// Estilo de dotnet/runtime — legítimo si el equipo lo adopta,
// pero no es "la norma de Microsoft".
private static readonly HttpClient s_cliente = new();

[ThreadStatic]
private static Contexto? t_contextoActual;
```

Adoptarlo es una decisión razonable, sobre todo en código de bajo nivel donde importa ver de un vistazo que un campo es compartido entre hilos. Presentarlo como norma de .NET en una revisión de código, no. La distinción no es pedantería: cambia quién tiene que justificar la decisión. Si es norma de Microsoft, quien se desvía debe explicarse; si es estilo de un repositorio, quien lo impone debe explicarse.

Esta guía recomienda adoptarlo solo cuando el código maneja estado estático mutable con frecuencia. En una aplicación de línea de negocio típica, donde los campos estáticos son escasos y casi siempre `readonly`, agrega una regla más sin resolver ningún problema observado.

### Sufijo `Async` — convención del Task-based Asynchronous Pattern (`F-04`)

El sufijo `Async` en métodos que devuelven `Task`, `Task<T>` o `ValueTask<T>` lo establece el *Task-based Asynchronous Pattern* (`N-15`), no las guías de nombrado. El sufijo es anterior al TAP —ya lo exigía el *Event-based Asynchronous Pattern*—, de modo que lo correcto es decir que el TAP fija la convención vigente, no que la origina. `N-15` exceptúa además a los combinadores: `WhenAll` y `WhenAny` no lo llevan. Su función original era permitir que un tipo expusiera la versión síncrona y la asíncrona de la misma operación sin colisión de nombres.

```csharp
public interface IReservasRepository
{
    Task<Reserva> ObtenerReservaAsync(Guid reservaId, CancellationToken ct);
}
```

Hay una excepción practicada por el propio ASP.NET Core que conviene conocer antes de que alguien la señale como error: los métodos de acción de un controlador **no** llevan el sufijo, porque forma parte del nombre de la acción en el enrutamiento. ASP.NET Core lo recorta automáticamente por medio de `SuppressAsyncSuffixInActionNames`, activo por defecto desde .NET Core 3.0.

Los métodos de prueba tampoco lo llevan por costumbre, aunque devuelvan `Task`; nadie va a invocarlos desde código.

---

## Comparación con otros ecosistemas

Sirve a quien llega de afuera y explica de dónde vienen los desvíos que aparecen en un equipo mixto. La columna de .NET es la de este documento; las otras dos recogen la convención dominante de cada comunidad.

| Elemento | .NET / C# | Java | Python (PEP 8) |
|----------|-----------|------|----------------|
| Paquete / espacio de nombres | `Reservas.Dominio` PascalCase | `com.empresa.reservas` minúscula | `reservas.dominio` minúscula |
| Clase | `ReservaDeSala` | `ReservaDeSala` | `ReservaDeSala` |
| Interfaz | `IRepositorio` con prefijo | `Repositorio` sin prefijo | no existe como construcción |
| Método / función | `CancelarReserva` PascalCase | `cancelarReserva` camelCase | `cancelar_reserva` snake_case |
| Propiedad | `FechaInicio` PascalCase | `getFechaInicio()` | `fecha_inicio` |
| Parámetro y variable local | `salaId` camelCase | `salaId` camelCase | `sala_id` snake_case |
| **Constante** | **`DuracionMaxima` PascalCase** | `DURACION_MAXIMA` | `DURACION_MAXIMA` |
| Campo privado | `_repositorio` | `repositorio` | `_repositorio` (uno) o `__x` (dos) |
| Valor de enumeración | `EstadoReserva.Confirmada` | `EstadoReserva.CONFIRMADA` | `EstadoReserva.CONFIRMADA` |

Tres filas concentran casi todos los errores de quien migra. Los métodos —Java escribe `cancelarReserva`, .NET escribe `CancelarReserva`— son el desvío más visible y el más fácil de corregir. Las constantes y los valores de enumeración son el desvío más persistente, porque quien lo trae está convencido de que `SCREAMING_SNAKE_CASE` es lo correcto y no lo consulta.

El prefijo `_` de Python merece una aclaración, porque parece el mismo que el de .NET y no lo es. En Python significa «no forma parte de la API pública», una señal social sin efecto en el intérprete; en .NET no significa nada sobre visibilidad —eso lo dice `private`— y es solo desambiguación léxica.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

La capitalización se fija el primer día en el `.editorconfig` y no se vuelve a discutir. Es la decisión más barata de toda la guía: no tiene alternativas serias, está especificada, y postergarla solo consigue que las primeras cincuenta clases queden desparejas.

Lo que conviene resolver explícitamente en `ESC-1` es únicamente lo que la norma deja abierto, y es poco: si se adopta `s_`/`t_` o no, si se usa `_` o `this.` para desambiguar campos, y en qué idioma se nombra cada plano —esta última no es capitalización pero se decide en el mismo momento y está en [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md)—. Tres decisiones, quince minutos, y con severidad `warning` desde el primer commit.

### `ESC-2` — Evolución estructural

Prácticamente no aplica. Una reorganización de proyectos o una extracción de módulo no cambia la capitalización de nada; los identificadores viajan con su nombre intacto.

Hay una excepción y viene de la partición. Cuando un módulo se extrae a un proyecto propio, tipos que eran `internal` pasan a ser `public`, y ahí sí cambia el régimen: pasan a estar bajo `N-01` a `N-04` en sentido literal, con el rigor de `CTX-3`. Un tipo `internal` con un acrónimo mal capitalizado era una molestia; el mismo tipo `public` en un paquete es un compromiso.

### `ESC-3` — Normalización de código existente

Es el escenario donde este documento se convierte en trabajo. Un repositorio con capitalización inconsistente tiene decenas o cientos de identificadores por corregir, y el orden importa.

Empezar por lo que no cruza la frontera del tipo: campos privados, variables locales, parámetros. Un renombre de campo privado no puede romper a nadie fuera de su archivo, el refactor de renombrado del IDE lo resuelve mecánicamente, y el diff es revisable. Después los tipos y miembros `internal`, que como mucho afectan al ensamblado. Al final, y con criterio distinto, los `public`: en `CTX-1` y `CTX-2` se renombran sin más; en `CTX-3` **no se renombran**, se marca el miembro con `[Obsolete]` y se publica el nombre correcto en paralelo, porque el consumidor no controla su calendario.

Dos prácticas que definen si el trabajo sirve o se repite. Los commits de renombrado no llevan ningún cambio funcional y se registran en `.git-blame-ignore-revs`, de modo que el historial siga siendo legible. Y la regla queda activada en el `.editorconfig` en el mismo commit: normalizar sin activar la regla es normalizar dos veces.

### `ESC-4` — Evaluación de código ajeno

Lo evaluable de la capitalización no es la elección —está especificada, hay poco que discutir— sino **si la convención está automatizada**. Un repositorio con reglas `dotnet_naming_rule` en su `.editorconfig` y severidad distinta de `none` va a mantenerse consistente. Uno que depende de que el revisor lo note no va a mantenerse, aunque hoy esté impecable.

Tres señales ordenan la evaluación rápido. Un `SCREAMING_SNAKE_CASE` en constantes indica que el equipo trajo la convención de otro lenguaje y probablemente traiga otras. Acrónimos de tres letras en mayúscula sostenida —`XMLParser`, `HTTPCliente`— indican que se siguió la intuición y no la fuente. Y prefijos de campo mezclados, `_x` en unos archivos y `m_x` o nada en otros, indican que hay más de una generación de código conviviendo sin que nadie lo haya unificado.

### Qué cambia según el contexto

**`CTX-1`.** Aparecen artefactos que no son C# y tienen convención propia. El componente Blazor sigue PascalCase y su nombre de archivo debe coincidir exactamente con el del tipo generado —`ListaDeSalas.razor` produce la clase `ListaDeSalas`—. Las clases CSS del proyecto no siguen la convención de C# sino la de su medio, habitualmente kebab-case.

**`CTX-2`.** El peso se desplaza del código al contrato. Rutas en kebab-case, propiedades JSON en camelCase, cabeceras HTTP en su forma canónica. El error caro de este contexto no es capitalizar mal una variable sino publicar en PascalCase un contrato que después tiene consumidores.

**`CTX-3`.** El único contexto donde `N-02` aplica con todo su rigor y donde equivocarse cuesta una versión mayor. La regla operativa: todo lo que no sea explícitamente parte de la API es `internal`, para achicar la superficie sobre la que hay que ser riguroso.

**`CTX-4`.** Se suma un plano nuevo: los nombres de los mensajes y de sus propiedades, que son contrato entre servicios y que ningún compilador verifica. Conviene fijar su convención de forma tan explícita como la de C#, porque acá no hay red de seguridad.

---

## Ejemplos concretos

### Un tipo con todas las categorías

```csharp
namespace Reservas.Dominio.Salas;                      // PascalCase

public interface IPoliticaCancelacion { }               // prefijo I, normativo N-01

public enum EstadoReserva                               // PascalCase
{
    Pendiente,                                          // valores en PascalCase, no PENDIENTE
    Confirmada,
    Cancelada
}

public sealed class ServicioReservas<TResultado>        // parámetro genérico con prefijo T
{
    public const int DuracionMaximaHoras = 8;           // PascalCase, no DURACION_MAXIMA_HORAS
    public static readonly TimeSpan AnticipacionMinima  // PascalCase
        = TimeSpan.FromMinutes(15);

    private readonly IRepositorioSalas _repositorio;    // _camelCase — F-02
    private static readonly HttpClient s_cliente = new();  // s_camelCase — F-03, opcional

    public event EventHandler<ReservaEventArgs>? ReservaConfirmada;  // PascalCase

    public Guid SalaId { get; init; }                   // "Id", no "ID"
    public DateTimeOffset FechaInicio { get; init; }    // sustantivo, PascalCase

    public async Task<TResultado> ConfirmarAsync(       // PascalCase + Async (F-04)
        Guid salaId,                                    // parámetro en camelCase
        DateTimeOffset fechaInicio,
        CancellationToken cancellationToken)
    {
        const int reintentosMaximos = 3;                // constante local en camelCase
        var reservasPendientes = await _repositorio     // variable local en camelCase
            .ObtenerPendientesAsync(salaId, cancellationToken);
        ...
    }
}
```

Lo que demuestra: doce categorías de identificador en un solo tipo, cada una con su convención, y las tres que provienen de convención de facto marcadas como tales. Un lector que abra este archivo sabe qué es cada cosa sin leer una sola declaración completa.

### El mismo dato en cuatro planos

```csharp
// C#: PascalCase
public sealed record ReservaDto(Guid SalaId, DateTimeOffset FechaInicio, bool EsRecurrente);
```

```json
// JSON: camelCase por la política predeterminada de ASP.NET Core
{ "salaId": "8f3e...", "fechaInicio": "2026-08-01T09:00:00-03:00", "esRecurrente": false }
```

```text
GET /api/salas/8f3e.../reservas-recurrentes    ← ruta en kebab-case
```

```sql
-- Columnas en snake_case si el motor es PostgreSQL
select sala_id, fecha_inicio, es_recurrente from reserva;
```

Los cuatro son correctos simultáneamente. Cada plano sigue la convención de su medio y la traducción entre planos está declarada en un solo lugar: la política de serialización, la plantilla de ruta y la configuración del modelo de EF Core.

### La regla que lo automatiza

Fragmento de `.editorconfig` que convierte la convención en verificación. El archivo completo, con estas mismas reglas y el resto de la configuración de estilo, está en [`ANEXO-PLANTILLAS`](../99-Anexos/Plantillas.md); el fragmento va acá para que se vea que la regla existe.

```ini
[*.cs]
# Campos privados de instancia: _camelCase  (F-02)
dotnet_naming_rule.campos_privados_con_guion_bajo.symbols  = campo_privado
dotnet_naming_rule.campos_privados_con_guion_bajo.style    = prefijo_guion_bajo
dotnet_naming_rule.campos_privados_con_guion_bajo.severity = warning

dotnet_naming_symbols.campo_privado.applicable_kinds           = field
dotnet_naming_symbols.campo_privado.applicable_accessibilities = private
dotnet_naming_symbols.campo_privado.required_modifiers         =

dotnet_naming_style.prefijo_guion_bajo.required_prefix   = _
dotnet_naming_style.prefijo_guion_bajo.capitalization    = camel_case
```

Toda regla de este documento admite una expresión equivalente. La pregunta de `ACT-03` frente a cada convención que quiera fijar es la misma: ¿esto lo verifica una herramienta, o depende de que alguien lo recuerde?

### La configuración que parece verificar y no verifica

`EnforceCodeStyleInBuild` en `true` sin `.editorconfig` en el repositorio deja la verificación activada sin reglas que aplicar: la capitalización sigue dependiendo de la disciplina de quien escribe, y el día que alguien la rompa nada avisa. El desarrollo de ese antipatrón y de cómo detectarlo está en [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md).

Lo que corresponde retener acá es el contraste con los repositorios de referencia: los tres tienen `.editorconfig` en la raíz, y el de `dotnet/runtime` declara reglas `dotnet_naming_rule` explícitas para `s_` en estáticos privados y `_` en campos privados (`F-03`, `O-08`). Una convención de capitalización sin archivo que la exprese es una convención declarada, no vigente.

---

## Preguntas guía

1. ¿Puedo determinar la categoría de cada identificador de este archivo por su forma, sin buscar su declaración?
2. Las constantes de este proyecto, ¿están en PascalCase o alguien trajo `SCREAMING_SNAKE_CASE` de otro lenguaje?
3. Los acrónimos de tres letras o más, ¿están en PascalCase según `N-02`, o quedaron en mayúscula sostenida?
4. Los prefijos que usa este equipo, ¿son los que documenta `N-05` y `F-02`, o son los de `dotnet/runtime` (`F-03`)? ¿El equipo sabe cuál es cuál?
5. ¿Hay una regla `dotnet_naming_rule` que verifique esto, y con qué severidad? Si no la hay, ¿por qué se espera que la convención sobreviva?
6. ¿La convención de C# se filtró hacia algún plano que tiene la suya —rutas, JSON, mensajes, columnas—?
7. Si esto es `CTX-3`: ¿algún nombre público quedaría mal si se publicara hoy, sabiendo que corregirlo después exige una versión mayor?
8. ¿Convive más de una convención en el repositorio? ¿Es residuo histórico o desacuerdo vigente?

---

## Criterios de calidad

Una aplicación buena se reconoce porque **la forma del identificador permite anticipar su categoría sin verificarlo**. Si al leer un método hay que buscar declaraciones para saber si `total` es campo, parámetro o local, la convención no está cumpliendo su función aunque cada identificador por separado esté bien escrito.

**Está automatizada.** Es el criterio que más predice el futuro. Una convención verificada por `.editorconfig` con severidad `warning` o superior sigue vigente en dos años; una convención acordada en una reunión, no.

**Es única en todo el repositorio.** Un repositorio con dos convenciones excelentes conviviendo es peor que uno con una convención mediocre aplicada de forma uniforme. La uniformidad es lo que permite leer sin pensar.

**Los planos no se contaminan entre sí.** Ni PascalCase filtrado hacia una ruta HTTP, ni kebab-case intentando entrar en un identificador C#.

**Los prefijos declaran su origen.** El equipo que usa `s_` sabe que lo tomó de `dotnet/runtime` (`F-03`) y no de Microsoft como norma general. Esto parece irrelevante hasta que alguien nuevo lo cuestiona y la discusión se resuelve por autoridad inventada.

### Antipatrones nombrados

Los específicos de capitalización. Los de elección de palabras están en [`TEM-ANTI`](Antipatrones-de-Nombrado.md).

**Constantes gritadas.** `public const int DURACION_MAXIMA = 8;`. Convención importada de C, Java o Python. En .NET va `DuracionMaxima`.

**Acrónimo sostenido.** `XMLParser`, `HTTPCliente`, `JSONSerializador`. Tres letras o más van en PascalCase por `N-02`: `XmlParser`, `HttpCliente`, `JsonSerializador`.

**Prefijo de miembro heredado.** `m_reserva` de C++/MFC, o `_Reserva` con la caja equivocada. La convención de `F-02` es `_camelCase`, y una sola forma por repositorio.

**Caja importada de otro lenguaje.** Métodos en camelCase —`cancelarReserva`— de quien viene de Java, o miembros en snake_case de quien viene de Python. Es la señal más rápida de que un `.editorconfig` no está haciendo su trabajo.

**Colisión por caja.** Un tipo que expone la propiedad `Total` y el campo público `total`. Compila en C# y es inaccesible desde lenguajes que no distinguen mayúsculas; `N-02` desaconseja depender de la caja para desambiguar miembros públicos.

**PascalCase filtrado al contrato.** `/api/SalasDisponibles` o un JSON con `{ "FechaInicio": ... }`. Corregible el primer día, atado a consumidores el segundo.

**Prefijo `s_` presentado como norma de Microsoft.** No es un error de código sino de atribución, y produce discusiones que no se pueden cerrar porque la autoridad invocada no existe (`F-03`).
