---
doc_id: TEM-DATOS
doc_type: tema
title: Patrones de acceso a datos
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-PAT, TEM-ENDP, TEM-CAPAS, TEM-SLICE, TEM-CVP, TEM-MODELOS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Patrones de acceso a datos — `TEM-DATOS`

## Resumen ejecutivo

Ningún otro punto de la organización de una aplicación .NET genera tanta discusión como este. La pregunta «¿hace falta un repositorio sobre EF Core?» lleva más de una década sin acuerdo, tiene argumentos sólidos de los dos lados y admite respuestas distintas según el tamaño del sistema y la disciplina del equipo. Un documento que la resuelva por decreto sería más cómodo de leer y menos útil.

Lo que sí se puede fijar con precisión es el mapa. Qué son Repository y Unit of Work en su definición original, qué de eso ya provee EF Core, por qué el repositorio genérico casi siempre falla, qué separa CQRS de las cosas con las que se lo confunde, y qué efecto tiene sobre la consulta que la entidad de dominio, el DTO y el modelo de presentación sean o no el mismo tipo —cuántas de esas representaciones existen lo determina la topología, y eso se resuelve en [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md)—. Con ese mapa, la decisión sobre repositorios deja de ser una cuestión de bando y pasa a ser una consecuencia de decisiones anteriores sobre capas y sobre pruebas.

Le sirve a `ACT-01` cuando fija la dirección de dependencias entre aplicación e infraestructura, a `ACT-02` cuando escribe la consulta número cuarenta, y a `ACT-04` cuando evalúa si una capa de repositorios está comprando algo o solo trasladando llamadas.

---

## Definición

### Repository

Fowler lo define en `O-02` como un objeto que media entre el dominio y la capa de mapeo de datos, y que ofrece al dominio una interfaz **con aspecto de colección en memoria**. El código de dominio pide objetos a esa colección y no sabe si detrás hay una base de datos relacional, un archivo o una llamada remota. Esa es la idea completa, y conviene retenerla porque el uso corriente del término se ha alejado bastante: buena parte de lo que hoy se llama repositorio es una clase con métodos `GetById`, `Add` y `Save` que envuelve una tabla, lo cual está más cerca de un Table Data Gateway —también de `O-02`— que del patrón que se invoca.

El contexto histórico importa. `O-02` es de 2002, anterior a los ORM maduros del ecosistema .NET. El problema que Repository resolvía era que el acceso a datos era código explícito de SQL y mapeo, disperso por toda la aplicación, y hacía falta un lugar donde concentrarlo. Ese problema hoy lo resuelve el ORM.

### Unit of Work

También de `O-02`. Mantiene una lista de los objetos afectados por una transacción de negocio, coordina la escritura de los cambios y resuelve los problemas de concurrencia que surgen. En lugar de escribir en la base cada vez que algo cambia, el Unit of Work acumula y confirma todo junto.

### Qué NO es, y con qué se confunde

**Un `DbContext` de EF Core ya es un Unit of Work.** No metafóricamente: hace exactamente lo que describe `O-02`. Rastrea las entidades cargadas, acumula los cambios en el *change tracker* y los confirma juntos —dentro de una transacción— cuando se llama a `SaveChanges` o `SaveChangesAsync`. La documentación de EF Core lo describe en esos términos.

**Un `DbSet<T>` ya es un repositorio.** Implementa `IQueryable<T>`, ofrece `Add`, `Remove`, `Find` y una superficie de consulta con aspecto de colección en memoria. Es, con bastante literalidad, la interfaz que `O-02` describe.

De ahí la observación que ordena todo el resto del documento: escribir `IRepositorioReservas` sobre un `DbSet<Reserva>` es agregar una capa sobre un patrón que ya está implementado. Puede haber razones para hacerlo —las hay, y se desarrollan más abajo— pero «hace falta un repositorio porque EF Core no lo tiene» no es una de ellas.

**CQRS no es event sourcing.** Es la confusión más extendida del área y se trata en su propia sección.

**El patrón no depende de la nomenclatura.** Una clase llamada `RepositorioDeReservas` que expone `IQueryable<Reserva>` no es un repositorio en el sentido de `O-02`; es un `DbSet` con otro nombre. Una clase llamada `ConsultasDeDisponibilidad` con métodos que devuelven resultados materializados puede serlo. El nombre no decide.

---

## El debate sobre el repositorio, con los dos lados

Esta es la discusión genuina del área. Ambas posiciones tienen defensores competentes y ninguna es obviamente correcta.

### A favor de envolver EF Core en repositorios

**Aísla el dominio de la infraestructura.** Si la capa de aplicación depende de `IRepositorioReservas` y no de `DbContext`, el `PackageReference` de EF Core puede quedar confinado a un solo proyecto y la regla de dependencia de `O-04` se vuelve verificable por el compilador. Sin ese aislamiento, `Microsoft.EntityFrameworkCore` está disponible en todo el código y solo la disciplina impide usarlo.

**Sustitución en pruebas.** Una interfaz de repositorio con cinco métodos se implementa a mano en veinte líneas para una prueba unitaria. La alternativa —proveedor en memoria de EF Core, o SQLite en memoria— funciona pero acopla la prueba al comportamiento del proveedor, y el proveedor in-memory no reproduce la semántica relacional en varios puntos relevantes.

**Controla el vocabulario de consultas.** Un repositorio con `ObtenerReservasSolapadasAsync(salaId, periodo)` fija un conjunto acotado de consultas que la aplicación puede hacer. Eso permite razonar sobre cuántas consultas existen, revisarlas juntas, indexar la base en consecuencia y detectar cuando alguien está por agregar la número treinta.

**Concentra decisiones de rendimiento.** `AsNoTracking`, `Include`, `Split query`, límites de paginación: si viven en un solo lugar, se auditan en un solo lugar.

### En contra

**Es una abstracción con fuga.** El repositorio pretende ocultar la persistencia, pero el comportamiento de EF Core la atraviesa igual. El seguimiento de cambios, la carga diferida, el momento exacto en que se materializa una consulta y el hecho de que dos llamadas devuelvan la misma instancia rastreada son detalles del ORM que el consumidor termina necesitando conocer. Una abstracción que hay que entender para usarla no está abstrayendo.

**Pierde capacidades de EF Core.** Proyecciones con `Select` a un tipo específico, consultas compuestas, `ExecuteUpdateAsync` y `ExecuteDeleteAsync`, `Include` selectivo según el caso de uso: todo eso vive en `IQueryable` y se pierde en cuanto el repositorio devuelve una lista materializada. Recuperarlo por métodos exige un método por combinación, y las combinaciones crecen más rápido que los casos de uso.

**Genera código de traspaso.** En la mayoría de los sistemas, buena parte de los métodos del repositorio son una línea que llama al `DbSet` equivalente. Es código que hay que escribir, revisar, mantener y navegar, y que no toma ninguna decisión.

**La sustitución en pruebas es menos necesaria de lo que se supone.** Una prueba que verifica una consulta contra un doble de repositorio verifica el doble, no la consulta. Los errores reales de acceso a datos —una condición mal escrita, un `Include` faltante, una traducción a SQL que no ocurre— solo aparecen contra una base de datos real. Muchos equipos terminan con pruebas de integración sobre SQLite o contenedor de todas formas, y en ese momento el repositorio deja de comprar la sustitución que lo justificaba.

### Criterio de decisión

La pregunta útil no es «¿repositorio sí o no?» sino **qué está comprando la capa en este sistema concreto**. Esta guía recomienda decidir sobre tres hechos observables:

| Hecho | Empuja hacia |
|-------|--------------|
| La aplicación se organiza por puertos y adaptadores, con la infraestructura como detalle sustituible declarado | Repositorios explícitos, uno por agregado, con métodos del vocabulario del dominio |
| El dominio vive en un proyecto separado que no debe referenciar EF Core | Repositorios explícitos: es la única forma de que el compilador lo verifique |
| Las consultas son variadas, cambian por caso de uso y necesitan proyecciones específicas | `DbContext` directo, o CQRS con consultas de lectura sueltas |
| El sistema es pequeño, un solo proyecto, y las pruebas de datos ya corren contra una base real | `DbContext` directo; el repositorio sería traspaso |
| Hay reglas de acceso que deben aplicarse siempre —multi-inquilino, borrado lógico, auditoría— | Concentrarlas: repositorio, o filtros de consulta globales de EF Core, que resuelven el caso sin capa nueva |

La combinación intermedia es frecuente y perfectamente defendible: repositorios para las escrituras, donde el agregado y su consistencia importan, y consultas de lectura que usan el `DbContext` directamente con proyecciones a DTO. Es, de hecho, CQRS aplicado sin ceremonia.

---

## Repositorio genérico

`IRepositorio<T>` con `ObtenerPorIdAsync`, `ListarAsync`, `AgregarAsync`, `ActualizarAsync` y `EliminarAsync`, implementado una vez contra `DbSet<T>` y registrado por reflexión para todas las entidades. Aparece en incontables plantillas y esta guía lo considera un antipatrón en la enorme mayoría de los casos, por un argumento que se puede seguir sin apelar a autoridad.

Un repositorio genérico enfrenta una disyuntiva de la que no hay salida. O bien expone `IQueryable<T>` —o acepta expresiones `Expression<Func<T, bool>>` como parámetro—, y entonces el consumidor está escribiendo consultas de EF Core a través de una interfaz que dice ser agnóstica, con lo cual no aísla absolutamente nada y solo agrega una indirección. O bien no lo expone, y entonces cada consulta real necesita un método propio en una interfaz específica, con lo cual la parte genérica queda reducida a cuatro operaciones triviales y aparece un `IRepositorioReservas : IRepositorio<Reserva>` con ocho métodos más. En ese punto la base genérica no está aportando nada que el `DbSet` no diera.

Hay un segundo problema, menos visible y más grave. El repositorio genérico existe por entidad, y eso empuja a tratar cada tabla como una unidad de acceso independiente. El diseño que `O-03` propone es el opuesto: la unidad de consistencia es el agregado, y un repositorio corresponde a un agregado, no a una tabla. Un `IRepositorio<LineaDeReserva>` permite modificar una línea sin pasar por su reserva, y con eso se pierde la única garantía que el agregado ofrecía.

---

## Specification

Cuando los criterios de consulta se repiten entre casos de uso, el patrón Specification los encapsula en objetos componibles: un objeto que representa «reservas activas de esta sala», otro que representa «que se solapan con este período», y una regla de composición. El repositorio recibe la especificación y la aplica.

Resuelve un problema real —la proliferación de métodos en el repositorio— a cambio de introducir un lenguaje de consulta propio que hay que aprender, mantener y traducir a `IQueryable`. Esta guía recomienda considerarlo solo cuando el número de métodos de consulta ya es un problema medible y los criterios se repiten de verdad entre varios de ellos. Aplicado por anticipado, es la abstracción sobre la abstracción sobre el ORM.

---

## CQRS

*Command Query Responsibility Segregation* separa el modelo con el que se escribe del modelo con el que se lee. Las escrituras pasan por el dominio, con sus invariantes y su agregado; las lecturas van directo de la base a la forma que la pantalla necesita, sin cargar entidades ni rastrear cambios.

La motivación es que ambos lados tienen requisitos opuestos. Escribir exige consistencia y reglas; leer exige forma conveniente y velocidad. Un único modelo que sirva a ambos termina siendo malo para los dos: entidades con propiedades de navegación que existen solo para que una pantalla las muestre, o consultas que cargan un agregado completo para leer tres campos.

**Lo que CQRS no implica**, y es donde se acumulan las confusiones:

- **No implica event sourcing.** Son patrones distintos que se mencionan juntos porque combinan bien y porque fueron divulgados por autores cercanos. Se puede aplicar CQRS sobre una base relacional con un esquema convencional.
- **No implica dos bases de datos.** Puede haber dos, con proyecciones de lectura desnormalizadas y sincronización eventual, pero es una variante avanzada y no la definición. La forma más común es una sola base y dos maneras de acceder a ella.
- **No implica un bus de mensajes ni una biblioteca de mediación.** Que la implementación más difundida en .NET use un mediador es una elección de implementación. Un método `ObtenerDisponibilidadAsync` que proyecta directamente a DTO ya es el lado de lectura de CQRS.
- **No implica consistencia eventual.** Solo aparece si se eligen almacenes separados.

Aplicado en su forma simple, CQRS es la razón por la cual el debate sobre repositorios pierde buena parte de su carga: la mayoría de los métodos que hinchan un repositorio son consultas de lectura, y si esas consultas viven en su propio lado, el repositorio queda con las tres o cuatro operaciones de escritura que realmente corresponden al agregado.

La afinidad con [`TEM-SLICE`](../30-Organizacion-Interna/Vertical-Slice.md) es directa. Un corte vertical agrupa todo lo que una funcionalidad necesita; CQRS establece que la funcionalidad de leer y la de escribir son cortes distintos. Combinados, la unidad de organización pasa a ser la operación —`CrearReserva`, `ConsultarDisponibilidad`— con su modelo de entrada, su manejador y su modelo de salida en la misma carpeta. Ni Vertical Slice ni CQRS son estándares de Microsoft; el origen del primero es `O-07` y el segundo no tiene ninguna entrada normativa en [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md).

---

## Qué tipo sale de la consulta

La distinción entre las tres representaciones —entidad de dominio, DTO y modelo de presentación—, cuántas de ellas existen según la topología y dónde vive cada una se desarrolla en [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md); acá interesa solo su efecto sobre la consulta. En `T1` y `T2` introducir DTOs puede ser sobreingeniería, y esta sección no lo contradice: describe qué cambia en el acceso a datos cuando la representación de salida existe y cuando no.

El caso peor tiene una secuencia reconocible. Una entidad de EF Core se devuelve tal cual desde el endpoint. Lo primero que ocurre es que el serializador recorre sus propiedades de navegación y dispara consultas de carga diferida —una por propiedad, mientras se serializa— o falla con una referencia circular entre entidad y colección relacionada. Lo segundo, si se resuelve con configuración del serializador, es que la respuesta incluye columnas que nadie decidió publicar: marcas internas, claves foráneas, campos de auditoría. Lo tercero llega meses después, cuando una migración renombra una columna y rompe a los clientes, porque el esquema de la base y el contrato público habían pasado a ser lo mismo sin que nadie lo declarara.

La proyección directa evita las tres cosas y además reduce lo que se trae de la base:

```csharp
// Sintético. La consulta pide solo las columnas de la respuesta; el SELECT resultante también.
public async Task<IReadOnlyList<ReservaResumenResponse>> ListarPorSalaAsync(
    Guid salaId, CancellationToken ct)
{
    return await _contexto.Reservas
        .AsNoTracking()
        .Where(r => r.SalaId == salaId && r.Estado == EstadoReserva.Confirmada)
        .OrderBy(r => r.Periodo.Inicio)
        .Select(r => new ReservaResumenResponse(
            r.Id, r.Sala.Nombre, r.Periodo.Inicio, r.Periodo.Fin, r.Solicitante.Nombre))
        .ToListAsync(ct);
}
```

`AsNoTracking` corresponde acá porque nada de lo devuelto se va a modificar, y el `Select` hace innecesario cualquier `Include`: EF Core traduce la proyección a las columnas efectivamente pedidas.

---

## Migraciones

Las migraciones de EF Core son código C# generado y versionado que describe la evolución del esquema. Al ser código, forman parte de la organización del proyecto y merecen las mismas decisiones que el resto.

**Dónde viven.** En el proyecto que contiene el `DbContext`, en una carpeta `Migrations/` que la herramienta crea por convención. Cuando el `DbContext` está en un proyecto de infraestructura sin punto de entrada ejecutable, hace falta indicar el proyecto de arranque al generar (`--startup-project`), porque la herramienta necesita construir el host para resolver la configuración.

**Quién las genera.** Una persona, con `dotnet ef migrations add`, revisando el resultado antes de confirmarlo. Que la herramienta las genere no significa que no haya que leerlas: una migración puede proponer eliminar y recrear una columna donde bastaba renombrarla, con pérdida de datos que solo se ve leyendo el archivo. Las migraciones se revisan en el *pull request* como cualquier otro código.

**Cuándo se aplican.** Hay dos momentos posibles y el trade-off es real. Aplicarlas al arrancar la aplicación —`Database.Migrate()`— es simple, no necesita nada del entorno de despliegue y garantiza que el esquema y el binario están sincronizados. Aplicarlas como paso separado del despliegue, con un script SQL idempotente generado por `dotnet ef migrations script`, es más trabajo y da control sobre el momento, sobre los permisos y sobre el orden respecto de la puesta en línea de las instancias nuevas.

La migración al arranque tiene un límite que conviene conocer antes de cruzarlo: **con más de una instancia**, varias arrancan a la vez y compiten por aplicar la misma migración. Además, la aplicación necesita en producción permisos de modificación del esquema, lo cual muchas organizaciones no conceden. Para un despliegue de instancia única es una elección correcta y esta guía no la desaconseja; para un despliegue con réplicas o escalado automático, deja de serlo.

---

## Aplicación por escenario

**`ESC-1` — Sistema nuevo.** La decisión sobre repositorios se toma junto con la de capas y no antes. Si [`TEM-CVP`](../30-Organizacion-Interna/Carpetas-o-Proyectos.md) resolvió un proyecto de dominio separado que no referencia EF Core, los repositorios ya están decididos por consecuencia. Si todo vive en un proyecto con capas en carpetas, esta guía recomienda arrancar con `DbContext` directo desde la capa de aplicación y extraer puertos cuando aparezca una razón concreta: la extracción posterior es mecánica, y la capa introducida por si acaso rara vez se retira. En `CTX-3` casi nada de este documento aplica —una biblioteca no suele poseer una base de datos—, con la excepción de que si la biblioteca **es** el acceso a datos, su superficie pública es contrato y las Framework Design Guidelines pasan a regir en sentido literal.

**`ESC-2` — Evolución estructural.** El síntoma habitual es que las consultas se multiplicaron y nadie sabe cuántas hay ni cuáles son caras. Introducir repositorios en ese momento suele ser el remedio equivocado: mueve las consultas de lugar sin reducirlas. El remedio que corresponde es separar el lado de lectura —CQRS en su forma simple— y proyectar a DTO, con lo cual las consultas quedan enumerables y cada una declara qué columnas trae. El segundo síntoma frecuente es que el `DbContext` aparece referenciado desde la capa de presentación; ahí sí la introducción de puertos compra algo verificable.

**`ESC-3` — Normalización de código existente.** Se ordenan por costo creciente. Barato y verificable: agregar `AsNoTracking` a las consultas de solo lectura. Reemplazar la devolución de entidades por proyecciones a un tipo de respuesta propio **no** entra en esa categoría por defecto: cambia la forma serializada de la respuesta y por lo tanto el comportamiento en el borde, que es lo que [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md) clasifica como `ESC-2`. Solo cuenta como `ESC-3` cuando el consumidor está bajo control del mismo equipo y el contrato serializado se verifica antes y después del cambio. Caro y riesgoso: introducir o quitar una capa de repositorios en un sistema en producción. Esta guía recomienda no hacerlo como tarea de normalización; es un cambio de diseño y se justifica por un problema, no por uniformidad.

**`ESC-4` — Evaluación de código ajeno.** Lo que se juzga no es si hay repositorios sino si la capa que existe está comprando algo. Un `IRepositorio<T>` genérico que expone `IQueryable` es señalable sin necesidad de conocer la historia del sistema, porque no aísla nada por construcción. También lo son las entidades de EF Core devueltas desde endpoints y las migraciones aplicadas al arranque en un despliegue con réplicas. Lo que no se puede juzgar sin contexto es la elección misma: un equipo que decidió repositorios explícitos para poder sustituir la persistencia tomó una decisión legítima, y si está registrada en un ADR, el evaluador la verifica en lugar de discutirla.

---

## Ejemplos concretos

### Sintético — el mismo caso con y sin repositorio

Con puerto explícito, en un diseño donde la aplicación no conoce EF Core:

```csharp
// Aplicacion/Puertos/IRepositorioReservas.cs — vocabulario del dominio, sin IQueryable.
public interface IRepositorioReservas
{
    Task<Reserva?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteSolapamientoAsync(
        Guid salaId, RangoHorario periodo, CancellationToken ct = default);

    Task AgregarAsync(Reserva reserva, CancellationToken ct = default);
}
```

```csharp
// Infraestructura/Persistencia/RepositorioReservas.cs — el adaptador, único que ve EF Core.
internal sealed class RepositorioReservas(ReservasDbContext contexto) : IRepositorioReservas
{
    public Task<Reserva?> ObtenerAsync(Guid id, CancellationToken ct = default)
        => contexto.Reservas.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExisteSolapamientoAsync(
        Guid salaId, RangoHorario periodo, CancellationToken ct = default)
        => contexto.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.SalaId == salaId
                        && r.Estado == EstadoReserva.Confirmada
                        && r.Periodo.Inicio < periodo.Fin
                        && periodo.Inicio < r.Periodo.Fin, ct);

    public async Task AgregarAsync(Reserva reserva, CancellationToken ct = default)
        => await contexto.Reservas.AddAsync(reserva, ct);
}
```

Nótese que no hay `GuardarCambiosAsync` en la interfaz. La confirmación es responsabilidad del Unit of Work, que es el `DbContext`, y el servicio de aplicación es quien decide dónde termina la transacción. Un repositorio que confirma por su cuenta destruye la única propiedad que el Unit of Work aportaba: que dos escrituras del mismo caso de uso se confirmen juntas o no se confirmen.

Sin repositorio, el mismo caso de uso con el `DbContext` inyectado directamente:

```csharp
// La misma lógica, una capa menos. Legítimo si el proyecto no separa dominio de infraestructura.
public sealed class ServicioReservas(ReservasDbContext contexto)
{
    public async Task<ResultadoReserva> ReservarAsync(
        Guid salaId, RangoHorario periodo, string solicitante, CancellationToken ct)
    {
        if (periodo.Fin <= periodo.Inicio)
        {
            return ResultadoReserva.Rechazada("El período es inválido.");
        }

        var haySolapamiento = await contexto.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.SalaId == salaId
                        && r.Estado == EstadoReserva.Confirmada
                        && r.Periodo.Inicio < periodo.Fin
                        && periodo.Inicio < r.Periodo.Fin, ct);

        if (haySolapamiento)
        {
            return ResultadoReserva.Rechazada("La sala ya está reservada en ese período.");
        }

        var reserva = Reserva.Solicitar(salaId, periodo, solicitante);
        contexto.Reservas.Add(reserva);
        await contexto.SaveChangesAsync(ct);

        return ResultadoReserva.Confirmada(reserva.Id);
    }
}
```

La diferencia entre ambas versiones es una interfaz, una clase de implementación y un registro en el contenedor. Vale la pena cuando el proyecto de dominio no debe referenciar EF Core, y no vale la pena cuando todo compila en el mismo `.csproj` y nada impide de todas formas que alguien inyecte el `DbContext` donde no corresponde.

Cuando se elige aislar, aparece una consecuencia que conviene anticipar: la confirmación también tiene que cruzar el puerto. El servicio de aplicación ya no puede llamar a `SaveChangesAsync` porque no conoce el `DbContext`, así que la unidad de trabajo se expone como una operación más del puerto —`ConfirmarCambiosAsync` en el ejemplo de [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)— que el caso de uso invoca al cerrar. Lo que **no** se hace es confirmar dentro de cada método del repositorio: ese es el antipatrón que este documento nombra más abajo.

Una advertencia sobre la consulta de solapamiento, válida en las dos versiones: verificar y después escribir no es atómico. Dos peticiones concurrentes pueden pasar la verificación y crear reservas superpuestas. La garantía real la da una restricción en la base, un índice de exclusión o un token de concurrencia; el chequeo previo sirve para dar un mensaje de error decente, no para garantizar la invariante.

### Repositorios explícitos como puertos

Cuando el repositorio sí se justifica, la razón es el diseño en el que está inserto y no una preferencia general. En el ejemplo sintético de reserva de salas, las interfaces viven en `Aplicacion/Puertos/`, junto con otros puertos que no son de persistencia —`INotificador`, `ICalendarioCorporativo`; el tiempo lo provee `TimeProvider` de la BCL y no necesita puerto propio—, y esa vecindad declara la intención: son todos detalles sustituibles del mundo exterior, tratados de la misma manera.

`IRepositorioReservas` declara `AgregarAsync` y `ObtenerAsync`, y también métodos de consulta con nombre del dominio como `BuscarAsync(FiltroReservas, …)` y `MarcarCanceladaAsync(…)`. Ninguno expone `IQueryable`: el filtro se pasa como un tipo propio y la paginación se resuelve en la consulta a la base, no en memoria.

Ese último punto es lo que separa un repositorio útil de uno de traspaso. Un método que trae todo y pagina en memoria habría sido más corto de escribir y habría degradado con el volumen; el que existe traduce el filtro a la consulta y devuelve solo la página pedida más el total. La decisión de rendimiento quedó concentrada en un lugar auditable, que es exactamente el argumento a favor del patrón.

La relación con el modelo de capas es directa y se desarrolla en [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md): en un diseño de puertos y adaptadores al estilo `O-05` —que no es un estándar de Microsoft— el repositorio no es una capa opcional sino la definición misma del puerto de persistencia. Preguntarse si sobra equivale a preguntarse si sobra el diseño.

Hay un matiz que el mismo ejemplo obliga a señalar. Si todo esto vive en un único proyecto `Microsoft.NET.Sdk.Web` —que es la forma que tiene el ejemplo—, el `PackageReference` de EF Core alcanza también a `Aplicacion/` y a `Dominio/`. Que el puerto no tenga fugas depende hoy de la revisión y no del compilador, y ese es el costo aceptado de la decisión registrada en [`TEM-CVP`](../30-Organizacion-Interna/Carpetas-o-Proyectos.md).

### Migración al arranque y WAL

El mismo `Program.cs` aplica las migraciones al arrancar, dentro de un ámbito creado a mano, y a continuación habilita el modo WAL de SQLite:

```csharp
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ReservasDbContext>();
    contexto.Database.Migrate();

    var conexion = (SqliteConnection)contexto.Database.GetDbConnection();
    conexion.Open();
    using var comando = conexion.CreateCommand();
    comando.CommandText = "PRAGMA journal_mode=WAL;";
    comando.ExecuteNonQuery();
}
```

El ámbito explícito es necesario y no decorativo: el `DbContext` se registra con vida de ámbito, y en el arranque no hay ninguna petición HTTP que provea uno. Resolverlo desde el proveedor raíz produciría una instancia de larga vida y un error en tiempo de ejecución si la validación de ámbitos está activa.

El PRAGMA responde a que dos componentes del mismo proceso —la interfaz de reservas y el proceso que sincroniza el calendario— escriben sobre el mismo archivo SQLite; WAL permite que las lecturas no bloqueen a la escritura. Es una decisión de infraestructura que conviene dejar registrada en el código con su motivo y una referencia al ADR que la sustenta, y eso es lo que la vuelve evaluable por alguien que llegue después.

Sobre el trade-off de migrar al arranque, el caso es el favorable: una instancia única, despliegue en contenedor, base de datos embebida en un archivo. No hay competencia posible por aplicar la migración y no hay que otorgar permisos de esquema a nadie, porque no hay servidor de base de datos. La misma línea en un servicio con tres réplicas detrás de un balanceador sería un problema distinto, y la señal para cambiar de estrategia es exactamente esa: el día que haya más de una instancia arrancando a la vez, la aplicación de migraciones se muda al despliegue.

---

## Preguntas guía

1. ¿Qué compra exactamente la capa de repositorios de este sistema? Si la respuesta es «separar responsabilidades» sin más precisión, probablemente no compra nada.
2. ¿El dominio está en un proyecto que no referencia EF Core, y lo verifica el compilador o la disciplina?
3. ¿Quién decide cuándo se confirma una transacción, y ese punto es el mismo en todos los casos de uso?
4. ¿Se puede enumerar el conjunto de consultas que la aplicación hace contra la base, o hay que buscarlas por todo el código?
5. ¿Qué tipo cruza el límite hacia la interfaz de usuario o hacia el cliente HTTP: una entidad de EF Core o un tipo propio de ese límite?
6. Si se aplica CQRS: ¿está separado el modelo de lectura del de escritura, o se llama CQRS a haber puesto un mediador en el medio?
7. ¿Las migraciones se revisan antes de confirmarse, o se generan y se aceptan sin leerlas?
8. ¿Cuántas instancias de la aplicación arrancan a la vez, y la estrategia de aplicación de migraciones es compatible con ese número?

---

## Criterios de calidad

Una capa de acceso a datos bien organizada permite responder tres preguntas sin leer todo el código: cuántas consultas distintas existen y dónde están, qué tipo cruza cada límite del sistema, y quién decide cuándo se confirma una transacción.

Los antipatrones con nombre:

**Repositorio genérico que expone `IQueryable`.** Ya desarrollado. No aísla nada y agrega una indirección que hay que atravesar para entender cualquier consulta.

**Repositorio de traspaso.** Cada método es una línea que llama al `DbSet` homónimo. Existe para cumplir con un diagrama de capas, no para resolver un problema.

**Repositorio que confirma.** Cada método termina con `SaveChangesAsync`. Rompe el Unit of Work: dos escrituras del mismo caso de uso quedan en transacciones distintas, y la segunda puede fallar dejando la primera confirmada.

**Repositorio por tabla en lugar de por agregado.** Permite modificar una entidad hija sin pasar por su raíz, con lo cual las invariantes del agregado dejan de estar garantizadas.

**Entidad como contrato.** La entidad de EF Core viaja hasta la vista o hasta el cuerpo de la respuesta HTTP. Ata el esquema de la base al contrato público y publica campos que nadie decidió publicar.

**Consulta `N+1`.** Una consulta para la lista y una más por cada elemento, generalmente por acceder a una propiedad de navegación dentro de un bucle o durante la serialización. Se detecta mirando el registro de SQL de EF Core en desarrollo, no leyendo el código.

**Traer todo y filtrar en memoria.** `ToListAsync()` seguido de `Where` en LINQ to Objects. Funciona con doscientas filas y colapsa con doscientas mil. La variante más frecuente es la paginación aplicada después de materializar.

**Rastreo innecesario.** Consultas de solo lectura sin `AsNoTracking`. EF Core rastrea cada entidad devuelta y paga memoria y tiempo por una funcionalidad que no se va a usar.

**Migración pendiente no versionada.** El esquema de la base de desarrollo se modificó a mano y no hay migración que lo describa. El próximo despliegue en un entorno limpio falla, y nadie sabe por qué.
