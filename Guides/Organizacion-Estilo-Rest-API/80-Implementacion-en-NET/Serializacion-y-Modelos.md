---
doc_id: TEM-SERIAL
doc_type: tema
title: Serialización y modelos
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-NET, TEM-MINIMAL, TEM-PROYECTO, TEM-VALID, TEM-CAMPOS, TEM-PATCH, TEM-ERR, TEM-BREAK, TEM-OPENAPI, MARCO-ESCENARIOS, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Serialización y modelos — `TEM-SERIAL`

## Resumen ejecutivo

El JSON que sale de una API ASP.NET Core está gobernado por decisiones que en su mayoría nadie tomó. Los nombres van en `camelCase` porque el framework lo impone —no System.Text.Json, que deja los nombres sin cambios (`N-38`)—; los enums van como número salvo que alguien pida lo contrario; la deserialización acepta números entre comillas y no distingue mayúsculas al hacer coincidir nombres; y si la aplicación mezcla controllers con Minimal APIs, **hay dos superficies de configuración distintas y configurar una no afecta a la otra**.

Cada uno de esos comportamientos es contrato. El consumidor no ve la clase C#: ve el JSON. Un `JsonSerializerOptions` mal ubicado cambia el contrato de la mitad de la API sin producir un solo error de compilación.

La segunda mitad del documento trata la otra fuente de contrato accidental: exponer la entidad de persistencia en lugar de un tipo pensado para el cable. Cuando eso ocurre, cada campo que se agrega al modelo interno aparece en la respuesta, y cada renombre de una propiedad es un cambio rompiente publicado sin que nadie lo haya decidido.

Le sirve a `ACT-01` cuando fija las opciones globales, a `ACT-02` cuando decide qué devuelve un endpoint, y a `ACT-03` cuando descubre que un campo cambió de forma sin aviso.

---

## Definición

La **serialización** es la traducción entre los tipos C# de la aplicación y la representación JSON que viaja por HTTP. En ASP.NET Core la hace **System.Text.Json**, y las decisiones que la gobiernan son de tres clases: las globales, en `JsonSerializerOptions`; las declarativas, en atributos sobre los tipos; y las estructurales, en la elección de qué tipo se serializa.

Los **modelos de contrato** —lo que la comunidad llama DTOs— son los tipos que existen para ser serializados. Su razón de ser es que **el contrato publicado tenga un lugar donde vivir que no sea la entidad de dominio**, de modo que cambiar el modelo interno y cambiar el contrato sean dos acciones distintas.

### Qué NO es

**No es la nomenclatura de campos.** Qué nombre corresponde ponerle a un campo, si `fechaInicio` o `inicio`, si en español o en inglés, si `camelCase` o `snake_case`, lo decide [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md). Este documento trata cómo se hace efectiva esa decisión en .NET y qué hace el framework cuando nadie decidió.

**No es el mapeo entre capas en general.** El mapeo entre el modelo de dominio y el de persistencia es asunto de la guía hermana de código. Acá interesa solo el borde: la traducción entre lo interno y lo publicado.

**No es validación.** Que un campo llegue con el tipo correcto lo resuelve el deserializador; que el valor tenga sentido lo resuelve [`TEM-VALID`](Validacion.md). La frontera se cruza seguido: un `null` en un campo no anulable es un problema de deserialización, y una fecha de fin anterior a la de inicio es un problema de validación.

**No es la generación del esquema OpenAPI.** Cómo se documenta el modelo, cómo se reutilizan los esquemas con `$ref` y cómo se personalizan lo trata [`TEM-OPENAPI`](../60-Especificacion-y-Documentacion/OpenAPI.md).

---

## Lo que ASP.NET Core decide por uno

### `camelCase` viene de ASP.NET Core, no de System.Text.Json

La distinción es real y está verificada de los dos lados.

**System.Text.Json puro**, sin ASP.NET Core (`N-38`):

> *«By default, property names and dictionary keys are unchanged in the JSON output, including case. Enum values are represented as numbers.»*

**ASP.NET Core MVC** (`N-37`): *«The default formatting is camelCase.»*

**Minimal APIs** (`N-26`): *«By default, Minimal API apps use `Web defaults` options during JSON serialization and deserialization.»*

La consecuencia práctica aparece cuando alguien serializa a mano —dentro de un servicio, para un mensaje de cola, en una prueba— usando `JsonSerializer.Serialize(...)` sin opciones y obtiene `PascalCase`, distinto de lo que la API devuelve por el mismo objeto. No es un bug: son dos configuraciones distintas, y una de ellas no está configurada.

Volver a `PascalCase` se hace poniendo la política en `null`, no fijando una política de `PascalCase`, que no existe:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = null);
```

**Hay una excepción documentada.** `ProblemDetails` es **siempre** `camelCase`, aunque la aplicación fije `PascalCase` (`N-37`):

> *«A `ProblemDetails` response is always camelCase, even when the app sets the format to PascalCase. `ProblemDetails` follows RFC 7807, which specifies lowercase.»*

Vale registrar que la documentación vuelve a decir RFC 7807 acá. Es coherente con el resto: ASP.NET Core **no** implementa RFC 9457, el issue de adopción sigue abierto en Backlog (`N-63`), y la posición correcta de la guía es citar 7807 cuando se describe lo que el framework hace y 9457 cuando se describe la especificación vigente. Se detalla en [`TEM-ERR`](../40-Contratos-y-Representaciones/Manejo-de-Errores.md).

### `JsonSerializerDefaults.Web` implica cuatro cosas, no una

Es el hallazgo que más contratos explica. `N-39`, verbatim, sobre el miembro `Web = 1`:

> *«This member implies that:*
> - *Integers must be encoded as small as possible.*
> - *Property names are treated as case-insensitive.*
> - *"camelCase" name formatting should be employed.*
> - *Quoted numbers (JSON strings for number properties) are allowed.»*

Las dos que casi nadie conoce son la segunda y la cuarta, y ambas afectan al contrato de entrada:

**El matching de nombres es insensible a mayúsculas al deserializar.** La API acepta `{"salaId": "..."}`, `{"SalaId": "..."}` y `{"SALAID": "..."}` indistintamente. Un consumidor puede estar mandando el nombre equivocado desde hace un año y funcionando. El día que alguien apriete la configuración —o migre a un servicio que no la tenga— ese consumidor se rompe, y la causa va a parecer inexplicable.

**Se aceptan números entre comillas.** `{"capacidad": "12"}` se deserializa igual que `{"capacidad": 12}`. Es tolerancia deliberada hacia clientes de JavaScript, y significa que la especificación publicada —que declara `integer`— es más estricta que lo que la API acepta.

Ambas son formas de laxitud que **no están declaradas en el documento OpenAPI**. En `CTX-1` eso importa: el contrato efectivo es más amplio que el publicado, y endurecerlo después rompe a quien se apoyó en la laxitud, que es exactamente el caso que [`TEM-BREAK`](../50-Evolucion-y-Versionado/Compatibilidad-y-Cambios-Rompientes.md) señala como el cambio rompiente que nadie ve venir.

.NET 10 agregó el miembro `Strict = 2`, que va en la dirección opuesta: no permite propiedades JSON no mapeadas ni duplicadas, respeta anotaciones de nulabilidad y respeta parámetros de constructor requeridos.

### Los dos tipos `JsonOptions` son distintos

Confusión frecuentísima, y la de consecuencias más silenciosas. Son dos tipos diferentes, en ensamblados diferentes, con **nombres de propiedad diferentes** (`N-61`).

| | Controllers / MVC | Minimal APIs / `HttpContext` |
|---|---|---|
| Tipo | `Microsoft.AspNetCore.Mvc.JsonOptions` | `Microsoft.AspNetCore.Http.Json.JsonOptions` |
| Ensamblado | `Microsoft.AspNetCore.Mvc.Core.dll` | `Microsoft.AspNetCore.Http.Extensions.dll` |
| Se configura con | `AddControllers().AddJsonOptions(...)` | `builder.Services.ConfigureHttpJsonOptions(...)` |
| Propiedad de opciones | `.JsonSerializerOptions` | `.SerializerOptions` |

**Configurar `AddControllers().AddJsonOptions(...)` no afecta a los endpoints Minimal API, y viceversa.** En una aplicación mixta hay que configurar las dos, con los mismos valores, o la API devuelve dos contratos distintos según el endpoint. La trampa adicional está en el nombre de la propiedad: quien copia un fragmento de un lado al otro se encuentra con que `SerializerOptions` no existe en un tipo y `JsonSerializerOptions` no existe en el otro, lo cual al menos falla en compilación. Lo que no falla en compilación es configurar solo una de las dos.

### Los enums son números por defecto

Tanto en System.Text.Json puro como en ASP.NET Core. `JsonSerializerDefaults.Web` **no** cambia esto. Los enums como string son siempre opt-in.

Es una de las decisiones de contrato más consecuentes y de las que menos se deciden conscientemente. Un `EstadoDeReserva.Confirmada` serializado como `2` obliga al consumidor a mantener una tabla de equivalencias que la especificación no siempre publica, y convierte cualquier reordenamiento del enum —agregar un valor en el medio, borrar uno obsoleto— en un cambio rompiente invisible. Esta guía recomienda **enums como string en toda API que cruce un límite de despliegue**, es decir en `CTX-1`, `CTX-3` y `CTX-4`.

Los tipos verificados (`N-40`):

- `JsonStringEnumConverter` — no genérico.
- `JsonStringEnumConverter<TEnum>` — genérico, **desde .NET 8**. La documentación es explícita: *«Only `JsonStringEnumConverter<TEnum>` is supported by the Native AOT runtime.»* Es la variante compatible con source generation y AOT.
- `[JsonSourceGenerationOptions(UseStringEnumConverter = true)]` — para aplicarlo globalmente con source generation.
- `JsonStringEnumMemberName` — **nuevo en .NET 9**, no en .NET 10. Permite fijar el nombre del miembro en el JSON con independencia del identificador C#.

---

## Source generation

El generador de código de System.Text.Json produce en compilación lo que la serialización por reflexión resolvería en ejecución. Sus dos motivos son el arranque más rápido y la compatibilidad con recorte y AOT (`N-40`).

```csharp
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(WeatherForecast))]
internal partial class SourceGenerationContext : JsonSerializerContext { }
```

Los modos son `JsonSourceGenerationMode.Metadata` y `.Serialization` —el *fast path*—; por defecto se generan ambos.

La conexión con ASP.NET Core se hace con **`TypeInfoResolverChain.Add`**, que es lo que usa la documentación:

```csharp
services.AddControllers().AddJsonOptions(
    static options =>
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(MyJsonContext.Default));
```

`JsonSerializerOptions.AddContext<T>()` **no aparece en ninguna página actual de la documentación**. Existió históricamente y sigue circulando en material viejo; lo vigente es `TypeInfoResolver` y `TypeInfoResolverChain`.

Para AOT y recorte, la propiedad relevante es `<JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>`. La documentación aclara: *«If you don't specify this property and `PublishTrimmed` is enabled, reflection-based serialization is automatically disabled.»*

La decisión de adoptarlo o no es de rendimiento y de destino de publicación, no de contrato: **el JSON que produce es el mismo**. Lo que sí cambia es que el conjunto de tipos serializables pasa a ser explícito, y un tipo olvidado falla en ejecución en lugar de resolverse por reflexión.

---

## Nulos, fechas y cultura

**Nulos.** El comportamiento por defecto es emitir la propiedad con valor `null`. `JsonIgnoreCondition.WhenWritingNull` la omite. La elección es de contrato y hay que tomarla una vez para toda la API: un consumidor que distingue «campo ausente» de «campo con valor nulo» —el caso de las actualizaciones parciales, en [`TEM-PATCH`](../40-Contratos-y-Representaciones/Actualizaciones-Parciales.md)— necesita que la política sea estable y esté documentada. Omitir nulos ahorra bytes y elimina información que a veces se necesita.

**Fechas.** System.Text.Json serializa `DateTimeOffset` en formato ISO 8601 extendido. Esta guía recomienda `DateTimeOffset` sobre `DateTime` en todo campo de contrato, porque `DateTime` no lleva desplazamiento y obliga al consumidor a adivinar la zona. En el dominio de reserva de salas la diferencia no es teórica: una sede en otro huso horario convierte «reserva de 14:00» en una pregunta sin respuesta.

Para fechas sin hora —la fecha de una reserva de jornada completa— `DateOnly` es el tipo adecuado y se serializa como `"2026-08-15"`. La plantilla de .NET 10 lo usa en su propio sample (`N-66`).

**Cultura.** La serialización de números y fechas de System.Text.Json no depende de la cultura del hilo: el formato JSON es invariante por especificación. Donde sí aparece la cultura es en el *model binding* de valores de ruta y de cadena de consulta, en el `TryParse` de tipos propios —cuya firma recibe un `IFormatProvider`— y en cualquier conversión hecha a mano. La regla es que **el borde HTTP se procesa siempre en cultura invariante**, y la cultura del usuario se aplica solo al presentar, del lado del cliente.

---

## DTOs frente a entidades

### Por qué no se expone la entidad

Cinco razones, en orden de gravedad:

**El contrato queda a merced del modelo interno.** Agregar una propiedad a la entidad la publica. Renombrarla es un cambio rompiente. Ninguna de las dos acciones se percibe como cambio de contrato en el momento de hacerla, que es precisamente el problema: la revisión de código ve una refactorización.

**Se filtran datos que no corresponde publicar.** El hash de la contraseña del usuario, la nota interna del administrador sobre la sala, el identificador del sistema heredado. Es el punto donde `ACT-07` tiene poder de veto y donde su intervención llega tarde con más frecuencia.

**Aparecen ciclos y cargas involuntarias.** Una entidad con propiedades de navegación —`Reserva.Sala.Reservas`— produce recursión o, con carga diferida, un número impredecible de consultas durante la serialización, en un punto del pipeline donde el `DbContext` puede ya no estar vivo.

**El esquema OpenAPI hereda la forma de la persistencia.** Claves foráneas, campos de auditoría, discriminadores de herencia. El consumidor recibe un modelo relacional en lugar de un recurso.

**Se pierde el lugar donde declarar el contrato.** Sin un tipo propio, no hay dónde poner los atributos de validación, la política de nulos ni los nombres del cable sin contaminar la entidad.

### Records

Los `record` son el tipo natural para un DTO: inmutables por construcción, con igualdad estructural que simplifica las aserciones en pruebas, y con sintaxis posicional breve. **Están soportados por la validación nativa de .NET 10** (`N-35`), que aplica los atributos declarados sobre los parámetros posicionales:

```csharp
public record Product(
    [Required] string Name,
    [Range(1, 1000)] int Quantity);
```

Hay un matiz de contrato que conviene tener presente y que no es de .NET sino de diseño: la sintaxis posicional hace que **el orden de los parámetros importe en el código y no en el JSON**. Reordenarlos no cambia el JSON pero sí rompe a todo el código C# que construya el tipo posicionalmente, incluidas las pruebas. No es un cambio rompiente de la API; sí lo es del ensamblado.

### El mapeo

Tres opciones, y la elección tiene menos peso del que se le suele dar.

**A mano**, con un método de extensión o un `static` factory. Explícito, sin dependencias, verificable leyendo. El costo es repetitivo y crece con la cantidad de tipos.

**Con un mapeador por source generation.** Genera el código en compilación, de modo que un campo no mapeado es un error de compilación. Es **(c) convención de comunidad**; ninguno de estos paquetes está verificado en la ficha de evidencia de esta guía y por eso no se nombra ninguno.

**Proyectando en la consulta.** Un `Select` que construye el DTO directamente desde la consulta a la base evita materializar la entidad y suele ser lo más eficiente. Tiene la ventaja adicional, poco mencionada, de que **imposibilita el error de devolver la entidad**: nunca existió una instancia de la entidad.

Esta guía recomienda mapeo a mano o proyección en la consulta, y reserva el mapeador automático para cuando la cantidad de tipos lo justifique. El motivo es de contrato: el mapeo a mano es el lugar donde alguien decide, campo por campo, qué se publica. Un mapeador que copia por convención de nombres publica lo que la entidad tenga, que es el problema que los DTOs venían a resolver.

---

## Aplicación por escenario

### `ESC-1` — API nueva

Las opciones de serialización son de las decisiones que hay que tomar el primer día porque después cuestan. Cuatro, concretamente: la política de nombres —o su confirmación explícita—, los enums como string, la política de nulos, y `DateTimeOffset` frente a `DateTime`.

La cuarta se subestima siempre. Cambiar un campo de `"2026-08-15T14:00:00"` a `"2026-08-15T14:00:00-03:00"` es un cambio rompiente para cualquier cliente que parsee con formato fijo, y descubrirlo después de publicar significa arrastrar el campo sin desplazamiento hasta la próxima versión mayor.

La quinta decisión, la de tener DTOs separados, casi no se percibe como decisión en `ESC-1` porque en ese momento la entidad y el contrato son idénticos y el mapeo parece ceremonia pura. Es exactamente la trampa declarada del escenario: el costo aparece cuando ya hay consumidores.

### `ESC-2` — Exposición o migración

Es el escenario donde los DTOs dejan de ser opcionales. El modelo interno existente empuja hacia una representación que lo refleja, y sin una capa de contrato ese empuje llega hasta el JSON: campos `TB_RESERVA_CAB_ID`, banderas `char(1)` con valores `'S'` y `'N'`, fechas como enteros `AAAAMMDD`.

En la variante de migración desde otra plataforma hay una tensión específica: **conservar la forma exacta del JSON anterior**. Cuando el objetivo declarado es que los clientes existentes no se enteren, la forma del JSON es un requisito y no una decisión, y eso puede obligar a `PascalCase`, a enums numéricos o a nombres que nadie elegiría. Es legítimo, y conviene declararlo en el proyecto como deuda con fecha de revisión, no como convención.

El costo de traducción hay que declararlo explícitamente ante quien financia. Desde afuera, mapear parece trabajo redundante.

### `ESC-3` — Evolución en producción

Cambiar opciones globales de serialización en producción es **un cambio rompiente de toda la superficie a la vez**, y es la operación más peligrosa de este documento. Pasar de enums numéricos a enums string cambia cada respuesta que contenga un enum. Cambiar la política de nulos altera la forma de todos los objetos. Ninguna de las dos cosas produce un error del lado del servidor.

Lo que **sí** se puede hacer sin romper:

- Agregar un campo nuevo a una respuesta. Un consumidor que deserializa con esquema estricto se rompe igual, y ese es el matiz de `TEM-BREAK` que corresponde consultar.
- Agregar un campo **opcional** a una petición.
- Aplicar un cambio de serialización **por tipo** con atributos, en lugar de globalmente. Es la vía para migrar de a poco.

Lo que **no** se puede hacer sin coordinar: cambiar la política de nombres, cambiar la representación de un enum, cambiar el tipo de un campo, agregar un valor a un enum que los clientes deserializan estrictamente, o endurecer la laxitud de `JsonSerializerDefaults.Web` —quitar la aceptación de números entre comillas rompe a quien los estaba mandando—.

Adoptar `JsonSerializerOptions.Strict` de .NET 10 en una API existente cae de lleno en esta categoría. Es una mejora genuina y es rompiente.

### `ESC-4` — Evaluación de una API ajena

**`ESC-4a`**, con acceso al código: se busca primero si hay DTOs o si los endpoints devuelven entidades. Si devuelven entidades, la conclusión operativa es que **la especificación no es confiable como contrato**, porque cualquier cambio en el modelo interno la altera sin que nadie lo revise.

Segundo, se verifica si la aplicación es mixta y si las dos superficies de `JsonOptions` están configuradas. Es una fuente frecuente de inconsistencias reportadas como bugs intermitentes.

**`ESC-4b`**, desde afuera: la serialización es de lo poco que sí se observa con precisión. Se determinan por observación la convención de nombres, la representación de los enums, si los nulos se emiten u omiten, el formato de las fechas y si llevan desplazamiento. Y se puede probar la laxitud de entrada: mandar un número entre comillas, mandar un nombre de campo con la capitalización cambiada, mandar un campo de más. Las tres respuestas caracterizan la configuración efectiva mejor que cualquier documentación.

El resultado es una hipótesis del contrato y hay que registrarlo como tal. Que la API acepte números entre comillas hoy no significa que esté declarado ni garantizado.

### Qué cambia según el contexto

| Contexto | Qué pesa en la serialización |
|---|---|
| `CTX-1` pública | Toda decisión de forma es permanente. Los enums como string y `DateTimeOffset` dejan de ser preferencia. La laxitud no declarada de `JsonSerializerDefaults.Web` es una promesa implícita que después no se puede retirar. |
| `CTX-2` interna | Los defaults alcanzan casi siempre, porque un cambio se coordina. La disciplina que igual conviene sostener es la de DTOs: sin ellos, los servicios se acoplan al modelo de persistencia ajeno. |
| `CTX-3` app propia | Con Blazor interactive server el consumo es servidor a servidor y se comporta como `CTX-2`. Con MAUI hay clientes instalados que no se actualizan: rige la disciplina de `CTX-1`. Un enum numérico en una API consumida por MAUI es una bomba de tiempo. |
| `CTX-4` integración | La forma la impone el proveedor. El trabajo de serialización es de traducción, y el criterio es que ningún tipo del proveedor cruce el límite de la capa de aislamiento. |

---

## Ejemplos concretos

Sintéticos, del dominio de reserva de salas. Tipos y métodos verificados en `N-37` a `N-41` y `N-61`.

### Configuración de las dos superficies en una aplicación mixta

```csharp
// Sintético. Aplicación con controllers Y Minimal APIs: hay que configurar AMBAS,
// con los mismos valores, o la API devuelve dos contratos distintos (N-61).
var builder = WebApplication.CreateBuilder(args);

// Superficie de Minimal APIs — nótese: .SerializerOptions
builder.Services.ConfigureHttpJsonOptions(opciones =>
{
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter<EstadoDeReserva>());
    opciones.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Superficie de MVC — nótese: .JsonSerializerOptions
builder.Services.AddControllers().AddJsonOptions(opciones =>
{
    opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<EstadoDeReserva>());
    opciones.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

### Un enum como string, declarado en el tipo

Aplicarlo por atributo en lugar de globalmente permite migrar de a un tipo por vez, que es la única forma de hacerlo en `ESC-3` sin romper todo junto.

```csharp
// Sintético. El converter genérico es el compatible con AOT y source generation (N-40).
[JsonConverter(typeof(JsonStringEnumConverter<EstadoDeReserva>))]
public enum EstadoDeReserva
{
    Pendiente,
    Confirmada,
    Cancelada,
    [JsonStringEnumMemberName("no_asistio")] NoAsistio   // JsonStringEnumMemberName: .NET 9+
}
```

Nótese que `JsonStringEnumMemberName` llegó en **.NET 9**, no en .NET 10. En .NET 8 el mismo efecto requiere un converter propio.

### El contrato como tipo separado

```csharp
// Sintético. Contratos/ReservaDetalle.cs — esto ES el contrato público.
// Cualquier cambio en este archivo es un cambio de contrato y se revisa como tal.
namespace Salas.Api.Contratos;

public sealed record ReservaDetalle(
    Guid Id,
    Guid SalaId,
    string NombreSala,
    string CodigoSede,
    DateTimeOffset Inicio,          // con desplazamiento: las sedes están en husos distintos
    DateTimeOffset Fin,
    EstadoDeReserva Estado,
    string SolicitanteNombre,
    int AsistentesPrevistos,
    DateTimeOffset CreadaEn);
```

```csharp
// Sintético. La entidad — NO se serializa nunca. Nótese qué contiene de más.
namespace Salas.Api.Dominio;

public sealed class Reserva
{
    public Guid Id { get; init; }
    public Guid SalaId { get; init; }
    public Sala Sala { get; init; } = null!;             // navegación: produciría un ciclo
    public Guid SolicitanteId { get; init; }
    public Usuario Solicitante { get; init; } = null!;
    public DateTimeOffset Inicio { get; set; }
    public DateTimeOffset Fin { get; set; }
    public EstadoDeReserva Estado { get; set; }
    public int AsistentesPrevistos { get; set; }
    public string? NotaInternaDeAdministracion { get; set; }   // no se publica
    public string? IdSistemaHeredado { get; set; }             // no se publica
    public byte[] RowVersion { get; set; } = [];               // no se publica
    public DateTimeOffset CreadaEn { get; init; }
    public DateTimeOffset? ModificadaEn { get; set; }
}
```

Tres campos de la entidad no aparecen en el contrato, y ninguno de los tres se habría podido excluir sin un tipo separado. `NotaInternaDeAdministracion` es el caso donde `ACT-07` interviene.

### Proyección en la consulta

```csharp
// Sintético. El DTO se construye en la consulta: la entidad nunca se materializa,
// y por lo tanto es imposible devolverla por error.
private static async Task<Results<Ok<ReservaDetalle>, NotFound>> ObtenerAsync(
    Guid id, ContextoDeSalas db, CancellationToken ct)
{
    var reserva = await db.Reservas
        .Where(r => r.Id == id)
        .Select(r => new ReservaDetalle(
            r.Id,
            r.SalaId,
            r.Sala.Nombre,
            r.Sala.Sede.Codigo,
            r.Inicio,
            r.Fin,
            r.Estado,
            r.Solicitante.NombreCompleto,
            r.AsistentesPrevistos,
            r.CreadaEn))
        .FirstOrDefaultAsync(ct);

    return reserva is null ? TypedResults.NotFound() : TypedResults.Ok(reserva);
}
```

### Mapeo a mano, cuando la entidad ya está materializada

```csharp
// Sintético. El mapeo explícito es el lugar donde alguien decide, campo por campo,
// qué se publica. Un mapeador por convención publicaría lo que la entidad tenga.
namespace Salas.Api.Contratos;

internal static class MapeoDeReservas
{
    public static ReservaDetalle ADetalle(this Dominio.Reserva reserva) => new(
        Id:                  reserva.Id,
        SalaId:              reserva.SalaId,
        NombreSala:          reserva.Sala.Nombre,
        CodigoSede:          reserva.Sala.Sede.Codigo,
        Inicio:              reserva.Inicio,
        Fin:                 reserva.Fin,
        Estado:              reserva.Estado,
        SolicitanteNombre:   reserva.Solicitante.NombreCompleto,
        AsistentesPrevistos: reserva.AsistentesPrevistos,
        CreadaEn:            reserva.CreadaEn);
}
```

### Source generation con los tipos de contrato

```csharp
// Sintético. Solo los tipos del contrato entran al contexto: es la lista explícita
// de lo que la API serializa (N-40).
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ReservaDetalle))]
[JsonSerializable(typeof(ReservaResumen))]
[JsonSerializable(typeof(PaginaDe<ReservaResumen>))]
[JsonSerializable(typeof(SalaDetalle))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal sealed partial class ContextoJsonDeSalas : JsonSerializerContext { }
```

```csharp
// Registro. TypeInfoResolverChain, no AddContext<T>() — que ya no figura en la doc.
builder.Services.ConfigureHttpJsonOptions(opciones =>
    opciones.SerializerOptions.TypeInfoResolverChain.Insert(0, ContextoJsonDeSalas.Default));
```

### La laxitud de `Web` en el cable

Las tres peticiones siguientes producen el mismo resultado con la configuración por defecto, y solo la primera está declarada en la especificación:

```http
POST /v1/reservas HTTP/1.1
Content-Type: application/json

{ "salaId": "a3f1…", "asistentesPrevistos": 12 }
```

```http
POST /v1/reservas HTTP/1.1
Content-Type: application/json

{ "SalaId": "a3f1…", "AsistentesPrevistos": 12 }
```

```http
POST /v1/reservas HTTP/1.1
Content-Type: application/json

{ "salaid": "a3f1…", "asistentesPrevistos": "12" }
```

Las dos últimas funcionan por las implicaciones de `JsonSerializerDefaults.Web` que casi nadie conoce (`N-39`). El contrato efectivo es más amplio que el publicado.

---

## Preguntas guía

- Los nombres del JSON de mi API, ¿los decidí o los puso el framework?
- Si mi aplicación tiene controllers y Minimal APIs, ¿configuré las dos superficies de `JsonOptions`?
- ¿Mis enums viajan como número o como string, y qué pasa cuando agregue un valor?
- ¿Alguna respuesta de mi API serializa una entidad de persistencia? ¿Cómo lo verifico automáticamente?
- ¿Las fechas que devuelvo llevan desplazamiento horario, y qué hace el cliente si no lo llevan?
- ¿Qué acepta mi API que la especificación no declara? ¿Probé mandarle un número entre comillas?
- Si mañana quisiera adoptar `JsonSerializerOptions.Strict`, ¿a quién rompo?

---

## Criterios de calidad

Se reconoce una serialización bien tratada en que **la forma del JSON es una decisión escrita en algún lado**, y en que existe un conjunto de tipos que son el contrato y otro que es el modelo interno, sin superposición.

Tres señales concretas: cambiar una propiedad de una entidad no cambia ninguna respuesta; el documento OpenAPI describe lo que la API efectivamente acepta, incluida la laxitud; y ningún tipo con propiedades de navegación llega al serializador.

### Antipatrones

**Devolver la entidad de persistencia.** El de peores consecuencias y el más difícil de revertir, porque para cuando se detecta ya hay consumidores apoyados en campos que nunca se pensaron como públicos.

**Configurar una sola de las dos superficies de `JsonOptions`.** Produce una API con dos contratos. Se manifiesta como bug intermitente reportado por el consumidor y cuesta días de localizar.

**Enums numéricos en una API con clientes instalados.** Convierte cualquier reordenamiento del enum en un cambio rompiente silencioso, y obliga al consumidor a mantener una tabla de equivalencias que la especificación rara vez publica.

**`DateTime` sin desplazamiento en un dominio con varias sedes.** Traslada al consumidor una ambigüedad que el servidor podía resolver, y produce reservas con una hora de diferencia dos veces por año.

**Confiar en la laxitud de `JsonSerializerDefaults.Web` sin declararla.** El contrato efectivo se vuelve más amplio que el publicado, y endurecerlo después rompe a quien se apoyó en él.

**Serializar a mano con `JsonSerializer.Serialize(objeto)` sin opciones y esperar el mismo resultado que la API.** Sin opciones se obtiene `PascalCase`; la API devuelve `camelCase`. Es el origen de pruebas que pasan contra un JSON que la API nunca produjo.

**Usar `AddContext<T>()`.** No figura en la documentación actual. Lo vigente es `TypeInfoResolverChain`.

**Poner atributos de serialización sobre la entidad de dominio.** Es la señal de que el DTO no existe y de que alguien intentó arreglarlo desde el lado equivocado. Un `[JsonIgnore]` sobre una entidad es un contrato escondido en la capa de persistencia.

**Cambiar opciones globales en producción como si fuera una mejora interna.** Es un cambio rompiente de toda la superficie a la vez.

---

## Anexo — Ficha de contrato de serialización

Se completa una vez por API y se revisa ante cualquier cambio de opciones globales. Su función es que las decisiones de forma dejen de ser implícitas.

```yaml
version_dotnet: "10.0"
superficies_configuradas:
  minimal_apis: si | no | no_aplica      # ConfigureHttpJsonOptions → .SerializerOptions
  mvc:          si | no | no_aplica      # AddJsonOptions → .JsonSerializerOptions
  alineadas:    si | no | no_aplica      # obligatorio si ambas existen

nombres:
  politica: camelCase | PascalCase | snake_case | otra
  origen: "default de ASP.NET Core (JsonSerializerDefaults.Web)" | "decisión explícita"
  matching_insensible_a_mayusculas: si | no      # implicación de Web (N-39)
  declarado_en_openapi: si | no

enums:
  representacion: numero | string
  converter: "JsonStringEnumConverter<T>" | "JsonStringEnumConverter" | ninguno
  politica_ante_valor_nuevo: ""                  # ver TEM-BREAK

nulos:
  emitidos: si | no
  condicion: "WhenWritingNull" | "Never" | otra
  distingue_ausente_de_nulo: si | no             # relevante para PATCH

fechas:
  tipo_en_contrato: DateTimeOffset | DateTime | DateOnly | mixto
  lleva_desplazamiento: si | no
  formato: "ISO 8601"

entrada:
  acepta_numeros_entre_comillas: si | no         # implicación de Web (N-39)
  acepta_propiedades_no_mapeadas: si | no
  preset_strict_adoptado: si | no                # .NET 10; adoptarlo es rompiente

modelos:
  dtos_separados_de_entidades: si | no
  estrategia_de_mapeo: manual | proyeccion | generador
  verificacion_de_que_no_se_expone_entidad: ""   # prueba, analizador o revisión

source_generation:
  habilitada: si | no
  contexto: ""
  registro: "TypeInfoResolverChain"              # AddContext<T>() está obsoleto
```

Los dos campos que más aportan son `matching_insensible_a_mayusculas` y `acepta_numeros_entre_comillas`, porque describen contrato que la API cumple y la especificación no declara. Un `sí` en cualquiera de los dos es una promesa implícita que ya se está haciendo.
