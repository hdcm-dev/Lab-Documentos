---
doc_id: TEM-ENDP
doc_type: tema
title: Patrones de endpoint
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-PAT, TEM-DATOS, TEM-SLICE, TEM-CVP, TEM-NOMB, TEM-TOPO, MARCO-ESCENARIOS, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Patrones de endpoint — `TEM-ENDP`

## Resumen ejecutivo

ASP.NET Core admite dos formas de declarar la superficie HTTP de una aplicación, y ambas están soportadas de forma permanente por Microsoft. Una registra delegados contra rutas —`app.MapPost("/api/reservas", …)`—; la otra declara clases decoradas con atributos que el framework descubre por reflexión. La elección entre ellas no es de gusto: cambia dónde vive el código de cada endpoint, cómo se inyectan las dependencias, qué mecanismo intercepta las peticiones y cómo se comporta la estructura cuando la cantidad de endpoints crece de cuatro a cuarenta.

El documento describe ambos patrones, los compara sobre dimensiones verificables y desarma la creencia más extendida a su alrededor: que las Minimal APIs son «para prototipos» y que un sistema serio migra a controllers cuando madura. Existe un paso intermedio —`MapGroup` con extracción a métodos de extensión— que resuelve la mayoría de los problemas que se atribuyen al crecimiento y que rara vez aparece en la discusión.

Le sirve a `ACT-01` cuando fija la estructura de un servicio nuevo, a `ACT-02` cuando decide dónde poner un endpoint más, y a `ACT-04` cuando evalúa si un `Program.cs` de cuatrocientas líneas es un problema real o una molestia estética.

---

## Definición

Un **patrón de endpoint** es la forma en que una aplicación asocia una ruta HTTP y un verbo con el código que los atiende, y la unidad de organización que resulta de esa asociación.

### Route handlers — Minimal APIs

Introducido en .NET 6 junto con el *minimal hosting model*. La ruta se registra imperativamente sobre la instancia de `WebApplication` mediante `MapGet`, `MapPost`, `MapPut`, `MapDelete` o `MapMethods`, y el segundo argumento es un delegado: una lambda, un método local `static`, un grupo de métodos o cualquier cosa invocable. El framework infiere de la firma del delegado de dónde sale cada parámetro —ruta, cadena de consulta, cuerpo, contenedor de servicios— y de qué se devuelve cómo se serializa la respuesta.

Se lo encuentra nombrado de varias maneras según la fuente: *Minimal APIs*, *minimal hosting model*, *route handlers*, o *endpoint routing con lambdas*. Las tres primeras no son sinónimos exactos —el minimal hosting model es el `Program.cs` de instrucciones de nivel superior sin `Startup`, y se puede usar con controllers— pero en la práctica se emplean de forma intercambiable.

### Controllers — MVC y Web API

El patrón anterior en el tiempo y el que hereda del ecosistema web previo. Una clase, por convención con sufijo `Controller` y derivada de `ControllerBase`, agrupa métodos públicos —*acciones*— que el framework descubre por reflexión al arrancar. La ruta se declara con atributos: `[Route]` sobre la clase, `[HttpGet]`/`[HttpPost]` sobre cada acción. `[ApiController]` agrega un conjunto de comportamientos automáticos: inferencia del origen de los parámetros, respuesta `400` automática ante estado de modelo inválido, y requisito de ruteo por atributo.

El linaje es explícito. Un controller de ASP.NET Core es un descendiente directo del **Front Controller** de `O-02`: un punto único de entrada que recibe todas las peticiones, resuelve a qué manejador corresponde cada una y delega. La *Page Controller* y la *Front Controller* de Fowler describen las dos alternativas que ya se discutían en 2002; ASP.NET Core resolvió por la segunda.

### Qué NO es y con qué se confunde

**No es una decisión de arquitectura.** Ni las Minimal APIs ni los controllers dicen nada sobre las capas, la dirección de las dependencias o el modelo de dominio. Un endpoint es una cáscara de transporte; lo que ocurre detrás lo determina [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md). Confundir ambos planos produce la variante más común del error: un `Program.cs` con lógica de negocio en las lambdas y la conclusión de que «las Minimal APIs no escalan», cuando lo que no escala es tener el dominio adentro del handler.

**Minimal no significa provisional.** La palabra «minimal» califica la ceremonia de declaración, no la capacidad ni el ámbito de uso. Un route handler acepta filtros, validación, autorización, versionado y generación de OpenAPI. Microsoft documenta ambos modelos como opciones vigentes y no marca ninguno como obsoleto ni como transitorio.

**No es lo mismo que el estilo del `Program.cs`.** Las instrucciones de nivel superior sin clase `Startup` son el minimal hosting model, y se pueden combinar con `AddControllers()` sin usar un solo `MapGet`. Los dos ejes son independientes y con frecuencia se los trata como uno.

---

## La comparación

| Dimensión | Route handlers (Minimal APIs) | Controllers (MVC / Web API) |
|-----------|-------------------------------|-----------------------------|
| Unidad de código | Un delegado registrado contra una ruta | Un método público de una clase |
| Descubrimiento | Explícito: existe porque hay una línea que lo registra | Por convención y reflexión al arrancar |
| Dónde se ve la superficie completa | En el código de registro, leyendo de arriba abajo | Repartida entre las clases; hace falta una herramienta para verla junta |
| Inyección de dependencias | Por parámetro del handler, por endpoint | Por constructor, compartida por todas las acciones de la clase |
| Interceptores | `AddEndpointFilter` / `IEndpointFilter`, por endpoint o por grupo | `IActionFilter`, `IAsyncActionFilter` y el resto del *filter pipeline*, por acción, clase o global |
| Model binding | Inferido de la firma; personalizable con `TryParse`/`BindAsync` | El *model binder* de MVC, con `[FromBody]`, `[FromQuery]`, proveedores de valores |
| Validación | No hay validación por anotaciones integrada de forma automática como en `[ApiController]`; se resuelve con filtros o bibliotecas | `[ApiController]` responde `400` automáticamente ante estado de modelo inválido |
| Sobrecarga por petición | Menor: no hay activación de controller ni canalización de filtros de MVC | Mayor, aunque en la enorme mayoría de las aplicaciones es irrelevante frente al acceso a datos |
| Convenciones automáticas | Pocas; casi todo es explícito | Muchas; hacen falta menos líneas pero hay que conocerlas |
| Cómo escala con la cantidad de endpoints | Requiere una disciplina activa de agrupación y extracción; sin ella, un archivo crece | La estructura ya impone una unidad de archivo por grupo de rutas |

La última fila concentra el argumento real. Los controllers traen gratis una organización que las Minimal APIs exigen decidir. Esa es una ventaja genuina para quien no quiere tomar la decisión, y una desventaja para quien quiere agrupar por funcionalidad en lugar de por recurso, porque el molde del controller empuja hacia el segundo eje.

Sobre la sobrecarga conviene ser preciso. Las Minimal APIs evitan la activación del controller y la canalización de filtros de MVC, y eso se mide en escenarios de carga sintética. En una aplicación que consulta una base de datos por petición, la diferencia queda sepultada bajo la latencia de la consulta. Elegir Minimal APIs por rendimiento es una razón defendible en un servicio de muy alto tráfico y prácticamente irrelevante en el resto.

---

## `MapGroup` — el paso intermedio que falta en la discusión

Cuando un `Program.cs` con Minimal APIs crece, el reflejo habitual es plantear la migración a controllers. Es un salto innecesario, porque entre «todo en `Program.cs`» y «una clase por recurso con atributos» hay dos mecanismos intermedios que resuelven casi todo lo que motiva la migración.

El primero es **`MapGroup`**, disponible desde .NET 7. Devuelve un `RouteGroupBuilder` que comparte prefijo de ruta y metadatos con todos los endpoints que se registren sobre él. La autorización, los filtros, las etiquetas de OpenAPI y las convenciones se aplican una vez al grupo en lugar de repetirse por endpoint.

```csharp
// Sintético. El prefijo y la autorización se declaran una vez para el grupo entero.
var reservas = app.MapGroup("/api/reservas")
    .RequireAuthorization()
    .WithTags("Reservas");

reservas.MapGet("/", ListarReservasAsync);
reservas.MapGet("/{id:guid}", ObtenerReservaAsync);
reservas.MapPost("/", CrearReservaAsync);
reservas.MapDelete("/{id:guid}", CancelarReservaAsync);
```

El segundo es la **extracción a métodos de extensión**. Cada grupo de endpoints se muda a su propio archivo, y `Program.cs` conserva una línea por grupo.

```csharp
// Sintético. Un archivo por área funcional; Program.cs queda como índice de la superficie HTTP.
namespace MiEmpresa.Reservas.Web.Endpoints;

public static class EndpointsReservas
{
    public static IEndpointRouteBuilder MapearEndpointsReservas(this IEndpointRouteBuilder rutas)
    {
        var grupo = rutas.MapGroup("/api/reservas")
            .RequireAuthorization()
            .WithTags("Reservas");

        grupo.MapPost("/", CrearAsync);
        grupo.MapDelete("/{id:guid}", CancelarAsync);

        return rutas;
    }

    private static async Task<IResult> CrearAsync(
        SolicitudReserva solicitud,
        ServicioReservas servicio,
        CancellationToken ct)
    {
        var resultado = await servicio.ReservarAsync(
            solicitud.SalaId, solicitud.Periodo, solicitud.Solicitante, ct);

        return resultado.EsExitoso
            ? Results.Created($"/api/reservas/{resultado.ReservaId}", resultado.ReservaId)
            : Results.Conflict(resultado.Motivo);
    }

    private static async Task<IResult> CancelarAsync(Guid id, ServicioReservas servicio, CancellationToken ct)
        => await servicio.CancelarAsync(id, ct) ? Results.NoContent() : Results.NotFound();
}
```

```csharp
// Program.cs conserva una línea por área.
app.MapearEndpointsReservas();
app.MapearEndpointsSalas();
app.MapearEndpointsAuditoria();
```

Lo que queda después de aplicar ambos mecanismos es un archivo por área funcional, con un método privado por endpoint y las dependencias declaradas en la firma. Es, estructuralmente, muy parecido a un controller —una unidad de archivo, un método por operación— con dos diferencias: el registro es explícito en lugar de inferido por reflexión, y la agrupación puede seguir cualquier eje, incluido el corte vertical de [`TEM-SLICE`](../30-Organizacion-Interna/Vertical-Slice.md), sin que el molde del framework empuje hacia el recurso.

El método de extensión es también la costura por la que la superficie HTTP se reparte entre proyectos cuando la solución deja de ser uno solo: cada proyecto expone su `Mapear…` y el anfitrión los invoca en orden. Ese caso —cuándo conviene ese reparto y qué arrastra— lo trata [`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md); acá alcanza con notar que el mecanismo es el mismo en un proyecto y en diez.

Esta guía recomienda agotar `MapGroup` y los métodos de extensión antes de considerar una migración a controllers. La migración cuesta tiempo, altera las firmas de todo el código de transporte y no resuelve ningún problema que estos dos mecanismos dejen abierto.

---

## Paralelo con otros ecosistemas

El lector que viene de otro entorno ya conoce estos dos patrones con otros nombres, y ubicarlos ahorra explicación.

El patrón de controllers es el de **Spring** en Java —`@RestController` sobre la clase, `@GetMapping` y `@PostMapping` sobre los métodos—, el de **Laravel** en PHP, el de **Rails** en Ruby y el de **NestJS** en TypeScript. El parecido con ASP.NET Core MVC es estrecho en los cuatro casos: clase que agrupa acciones, ruteo declarado por atributo o anotación, descubrimiento por convención, inyección por constructor.

El estilo de route handlers es el de **Express** en Node, **Flask** y **FastAPI** en Python, y **Javalin** en Java. Se registra una función contra un método y una ruta, y la función es la unidad.

La precisión que importa, y que se pierde con frecuencia, es que **la división no corre por lenguaje sino por diseño de framework**. Java tiene Javalin, con registro funcional de rutas, y Spring WebFlux ofrece un modelo de *functional routing* además del basado en anotaciones. Node tiene NestJS, que trae controllers con decoradores sobre el mismo Express que popularizó el estilo opuesto. Un mismo lenguaje aloja ambos patrones sin dificultad; lo que decide es qué eligió el framework.

El linaje sintáctico del estilo de route handlers se remonta a **Sinatra** (Ruby, 2007), que popularizó la forma `verbo('/ruta') { bloque }`. De ahí pasó a Express, y el parecido con Flask, FastAPI y las Minimal APIs es reconocible a simple vista. Estas atribuciones son conocimiento corriente del ecosistema y no se apoyan en ninguna fuente de la tabla de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md); se presentan como tales y no como afirmación verificada en fuente primaria.

---

## Aplicación por escenario

**`ESC-1` — Sistema nuevo.** El escenario donde la decisión es libre y barata. Esta guía recomienda arrancar con route handlers agrupados por `MapGroup` desde el primer endpoint, no desde el décimo: el costo de crear el grupo cuando hay un solo endpoint es una línea, y el de agrupar cuarenta endpoints después es una tarde. En `CTX-2` la superficie es toda la aplicación y conviene una carpeta `Endpoints/` con un archivo por área desde el principio. En `CTX-1` la superficie HTTP suele ser marginal —unos pocos endpoints que la interfaz necesita— y `Program.cs` alcanza. En `CTX-3` no aplica: una biblioteca no expone rutas HTTP, salvo que su producto sea precisamente un conjunto de endpoints, en cuyo caso el método de extensión `Mapear…` es el mecanismo de publicación natural y pasa a formar parte de la superficie pública con todo lo que eso implica según [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md).

**`ESC-2` — Evolución estructural.** El síntoma que trae a un equipo a este documento es casi siempre el mismo: `Program.cs` creció y nadie encuentra nada. El diagnóstico correcto exige separar dos causas que se confunden. Si el archivo es largo porque hay muchos registros de endpoints, el remedio es agrupar y extraer. Si es largo porque las lambdas contienen lógica de negocio, el remedio no está en este documento sino en [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md), y migrar a controllers solo mudaría el problema a otro archivo. En `CTX-4` aparece una consideración adicional: cuando varios servicios exponen superficies parecidas, la consistencia entre ellos pesa más que la elección concreta, y conviene fijarla por escrito.

**`ESC-3` — Normalización de código existente.** Es un caso donde el orden de las tareas importa mucho. Reagrupar endpoints bajo `MapGroup` y extraerlos a métodos de extensión es un cambio mecánico y verificable con las pruebas existentes, siempre que las rutas resultantes sean idénticas. Convertir controllers a route handlers o al revés no lo es: cambian el model binding, el comportamiento de validación automática y los filtros, y cada uno de esos cambios puede alterar el contrato observable. Esta guía recomienda no convertir el estilo de endpoint durante una normalización. Es una migración funcional con riesgo, no una normalización de estilo, y se planifica como tal.

**`ESC-4` — Evaluación de código ajeno.** Lo que se juzga no es el patrón elegido sino tres cosas independientes de él: si la superficie HTTP es enumerable —si alguien puede responder «qué endpoints tiene esto» sin ejecutarlo—, si el código de transporte contiene lógica de negocio, y si la aplicación es consistente consigo misma. Un sistema con controllers y route handlers mezclados sin criterio declarado es peor que cualquiera de los dos aplicado de forma uniforme. Señalar «esto debería ser controllers» sin más es introducir una preferencia, no verificar lo acordado, y `MARCO-ACTORES` es explícito sobre el límite del rol `ACT-04` en ese punto.

---

## Ejemplos concretos

El sistema de reserva de salas —ejemplo sintético— es una aplicación Blazor con render *interactive server*, y aun así expone cuatro endpoints de autenticación como Minimal APIs directamente en `Program.cs`:

```text
POST /api/auth/configuracion-inicial
POST /api/auth/ingresar
POST /api/auth/salir
POST /api/auth/cambiar-contrasena     .RequireAuthorization()
```

La razón por la que existen es la parte instructiva, y no tiene nada de particular de este ejemplo: es una consecuencia del modelo de render. Un componente interactivo no puede establecer una cookie de autenticación. Para cuando ese componente ejecuta su código, la respuesta HTTP ya se envió y las cabeceras ya salieron; el componente vive del otro lado de una conexión de tiempo real, no de la respuesta que podría llevar el `Set-Cookie`. Iniciar sesión requiere entonces una petición HTTP común, y eso es lo que provee un endpoint.

El formulario de las páginas `/configuracion-inicial` e `/ingresar` postea a estos endpoints, cuya única función es validar credenciales, emitir o limpiar la cookie y redirigir de vuelta a una ruta de la aplicación. Todo el resto de la interfaz —buscar salas libres, confirmar una reserva, cancelarla— pasa por componentes interactivos y no toca HTTP explícito en ningún momento. De ahí que la superficie sea de cuatro rutas y no vaya a crecer con el sistema.

La estructura del handler refleja exactamente ese alcance:

```csharp
// Estructura del handler, resumida. La lógica no está acá.
app.MapPost("/api/auth/ingresar", async (
    HttpContext ctx,
    IAntiforgery antiforgery,
    ServicioAutenticacion servicioAutenticacion) =>
{
    if (!await EsRequestValidaAsync(antiforgery, ctx))
    {
        return Results.Redirect("/ingresar?error=ANTIFORGERY");
    }

    var formulario = await ctx.Request.ReadFormAsync();
    var clave = ClaveIntentos(formulario["usuario"].ToString(), ctx);
    var resultado = await servicioAutenticacion.AutenticarAsync(
        formulario["usuario"].ToString(), formulario["contrasena"].ToString(), clave);

    // … traducción del resultado a una redirección con código de error …
});
```

Tres rasgos merecen atención. Las dependencias —`HttpContext`, `IAntiforgery`, `ServicioAutenticacion`— llegan como parámetros del handler y no por constructor, lo que hace visible en la firma exactamente qué necesita este endpoint y no el vecino. La lógica vive en `ServicioAutenticacion`, de la capa de Aplicación: el handler lee el formulario, delega, y traduce el resultado a una redirección; es una cáscara delgada de transporte. Y las funciones auxiliares `EsRequestValidaAsync` y `ClaveIntentos` están declaradas como funciones locales `static` al final del archivo, un mecanismo de C# que permite compartir código entre handlers sin crear una clase y sin capturar estado del ámbito exterior.

`.RequireAuthorization()` aparece sobre un único endpoint —el cambio de contraseña—, aplicado al final de la cadena de registro. Los otros tres son por definición anónimos: quien no tiene sesión es exactamente el que necesita `ingresar`.

**Por qué Minimal API es la elección correcta acá.** La superficie HTTP no es el producto. El producto es la interfaz de reservas; estos cuatro endpoints existen únicamente para sortear la limitación del modelo de render que se describió arriba. Un `AuthController` con cuatro acciones agregaría un archivo, una clase, un constructor con tres dependencias inyectadas —dos de las cuales solo usan algunas acciones— y un conjunto de convenciones de MVC en una aplicación que por lo demás no usa MVC en absoluto. El costo de mantenimiento sería permanente y el beneficio, ninguno.

**Cuál sería la señal para cambiar.** No la cantidad de líneas. Las señales son otras: que aparezcan endpoints de otras áreas —informes de ocupación, integración con el calendario corporativo— y `Program.cs` empiece a mezclar dominios; que los cuatro endpoints necesiten compartir más comportamiento transversal que el que hoy resuelven dos funciones locales; o que el proyecto pase a exponer una API para consumo externo, momento en que la generación de OpenAPI y el versionado pasan a ser requisitos. En los tres casos el primer movimiento sigue siendo `MapGroup("/api/auth")` con las convenciones comunes y extracción a un `EndpointsAutenticacion.MapearEndpointsAutenticacion(app)`, no controllers.

Un detalle del mismo archivo ilustra el valor del registro explícito. Los endpoints de sembrado para pruebas de extremo a extremo se montan dentro de un condicional sobre el entorno, de modo que fuera de `E2E` no existen —no están deshabilitados, no existen como superficie de ataque—:

```csharp
if (app.Environment.IsEnvironment("E2E"))
{
    MapearEndpointsSembradoE2E(app);
}
```

Con descubrimiento por reflexión, conseguir el mismo efecto exige convenciones de aplicación o filtros de ensamblado. Acá es un `if`.

---

## Preguntas guía

1. ¿Alguien puede enumerar todos los endpoints de este sistema leyendo el código, sin ejecutarlo ni consultar una herramienta externa?
2. ¿Los handlers deciden algo de negocio, o traducen entre HTTP y una operación que vive en otra capa?
3. Si el archivo de endpoints creció, ¿creció por cantidad de registros o por cantidad de lógica adentro de cada uno? Son dos problemas con remedios distintos.
4. ¿Las convenciones transversales —autorización, prefijo, etiquetas, manejo de errores— están declaradas una vez por grupo, o repetidas endpoint por endpoint?
5. Si conviven ambos patrones, ¿existe una regla escrita sobre qué se atiende con cada uno?
6. ¿La razón para preferir un patrón está registrada en alguna parte, o se está por repetir la discusión en cada incorporación al equipo?
7. Antes de plantear una migración a controllers: ¿se agotaron `MapGroup` y la extracción a métodos de extensión, y qué problema concreto quedó sin resolver?

---

## Criterios de calidad

Un buen uso de cualquiera de los dos patrones comparte tres propiedades: la superficie HTTP completa es enumerable leyendo el código; cada endpoint traduce entre HTTP y una operación de aplicación sin decidir nada de negocio; y las convenciones transversales —autorización, versionado, manejo de errores, etiquetas de documentación— se declaran una vez por grupo y no se repiten por endpoint.

Los antipatrones que aparecen con nombre propio:

**`Program.cs` monolítico.** Todos los endpoints de todas las áreas registrados en línea en el archivo de arranque, sin agrupación. Es el que da mala fama a las Minimal APIs, y no es un defecto del patrón sino la ausencia de la disciplina que el patrón exige. El remedio no es migrar; es `MapGroup`.

**Handler con lógica de negocio.** La lambda —o la acción del controller— consulta la base de datos, aplica reglas, decide. Convierte el código de transporte en el sitio donde vive el dominio, y hace que la única forma de probar una regla de negocio sea levantar un servidor HTTP.

**Controller anémico de traspaso.** El extremo opuesto y también un problema: una clase por recurso cuyas acciones no hacen más que llamar a un método de servicio con los mismos parámetros y devolver `Ok(resultado)`. Es ceremonia sin función. Si el controller no aporta traducción, validación ni negociación, el mismo trabajo lo hace un route handler de dos líneas.

**Mezcla sin criterio.** Controllers y route handlers conviviendo sin una regla declarada sobre qué va en cada uno. Coexistir es legítimo —hay razones para atender un área con cada mecanismo— pero la regla tiene que existir y estar escrita. Sin ella, la primera pregunta de cualquiera que llegue nuevo es dónde poner el siguiente endpoint, y no hay respuesta.

**Entidad de dominio como contrato HTTP.** Devolver directamente la entidad de EF Core desde el endpoint. Ata el esquema de la base al contrato público y expone campos que nadie decidió exponer; el desarrollo está en [`TEM-DATOS`](Patrones-de-Acceso-a-Datos.md).

**Convención de C# filtrada al contrato.** Rutas en PascalCase, propiedades JSON con la capitalización del tipo de C#. Los nombres del contrato externo siguen la convención de su medio, según advierte [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) para `CTX-2`.
