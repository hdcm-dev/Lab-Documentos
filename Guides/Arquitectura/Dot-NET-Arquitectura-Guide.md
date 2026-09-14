# Arquitectura del Proyecto .NET — Guía Completa

> Fecha: Abril 2026  
> Stack: .NET 9, Blazor, MAUI Hybrid, ASP.NET Core Web API, EF Core

---

## Índice

1. [Glosario de Términos](#1-glosario-de-términos)
2. [Visión General de la Solución](#2-visión-general-de-la-solución)
3. [Estructura de Proyectos en la Solución](#3-estructura-de-proyectos-en-la-solución)
4. [Proyecto: Domain](#4-proyecto-domain)
5. [Proyecto: Application](#5-proyecto-application)
6. [Proyecto: Infrastructure](#6-proyecto-infrastructure)
7. [Proyecto: WebAPI](#7-proyecto-webapi)
8. [Proyecto: WebFront (Blazor)](#8-proyecto-webfront-blazor)
9. [Proyecto: Shared.UI (Razor Class Library)](#9-proyecto-sharedui-razor-class-library)
10. [Proyecto: Desktop (MAUI Blazor Hybrid)](#10-proyecto-desktop-maui-blazor-hybrid)
11. [Comunicación Front → API: Capa de Servicios](#11-comunicación-front--api-capa-de-servicios)
12. [Diagrama Final Completo](#12-diagrama-final-completo)
13. [Reglas de Dependencia entre Proyectos](#13-reglas-de-dependencia-entre-proyectos)

---

## 1. Glosario de Términos

Antes de profundizar en la estructura, es fundamental definir con precisión cada término, ya que en la industria muchos se usan de forma ambigua.

---

### Entidad de Dominio (Domain Entity)

Clase C# que representa un concepto central del negocio. Contiene **identidad propia** (un `Id`) y **comportamiento** (métodos que modelan reglas del negocio). No depende de ninguna tecnología.

```csharp
// MyProject.Domain/Entities/Producto.cs
public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; }

    // El constructor es privado para controlar cómo se crea una instancia
    private Producto() { }

    public static Producto Crear(string nombre, decimal precio)
    {
        if (precio <= 0) throw new DomainException("El precio debe ser mayor a cero.");
        return new Producto { Id = Guid.NewGuid(), Nombre = nombre, Precio = precio, Activo = true };
    }

    public void Desactivar() => Activo = false;
}
```

> **Regla clave**: una entidad de dominio nunca tiene atributos de EF Core, referencias a `DbContext`, ni propiedades de navegación pensadas para la base de datos. Es C# puro.

---

### Value Object

Objeto que representa un concepto del dominio pero **sin identidad propia**. Dos Value Objects con los mismos datos son considerados iguales. No tienen `Id`.

```csharp
// MyProject.Domain/ValueObjects/Dinero.cs
public record Dinero(decimal Monto, string Moneda)
{
    public static Dinero Cero(string moneda) => new(0, moneda);
    public Dinero Sumar(Dinero otro)
    {
        if (Moneda != otro.Moneda) throw new DomainException("No se pueden sumar monedas distintas.");
        return new Dinero(Monto + otro.Monto, Moneda);
    }
}
```

Ejemplos: `Dinero`, `Direccion`, `Email`, `RangoFecha`.

---

### DTO (Data Transfer Object)

Objeto plano (sin comportamiento) cuyo único propósito es **transportar datos entre capas o entre sistemas**. No tiene lógica de negocio. Existen dos subtipos clave:

- **Request DTO**: datos que entran a la aplicación (desde la UI o la API).
- **Response DTO**: datos que salen hacia el consumidor.

```csharp
// MyProject.Application/Products/DTOs/ProductoResponseDto.cs
public record ProductoResponseDto(Guid Id, string Nombre, decimal Precio, bool Activo);

// MyProject.Application/Products/DTOs/CrearProductoRequestDto.cs
public record CrearProductoRequestDto(string Nombre, decimal Precio);
```

> **Distinción importante**: los DTOs no son entidades de dominio. Se mapean a/desde entidades usando AutoMapper o mapeo manual. Esto protege al dominio de cambios en los contratos de la API.

---

### ViewModel

Similar al DTO pero orientado a la **capa de presentación (UI)**. Modela exactamente lo que una vista necesita mostrar, que puede diferir de la estructura del dominio.

```csharp
// MyProject.WebFront/Models/ProductoListaViewModel.cs
public class ProductoListaViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string PrecioFormateado { get; set; }   // "$1.500,00"
    public string EstadoBadge { get; set; }        // "Activo" / "Inactivo"
    public string CssClass { get; set; }           // "badge-green" / "badge-red"
}
```

> En proyectos Blazor, el ViewModel también puede ser el modelo de un formulario con validaciones de UI (`[Required]`, `[MaxLength]`).

---

### Modelo de Persistencia

Clase diseñada específicamente para el **mapeo con la base de datos** (tablas, columnas, índices). Solo existe si las entidades de dominio no se pueden mapear directamente con EF Core.

```csharp
// MyProject.Infrastructure/Persistence/Models/ProductoDbModel.cs
// (Solo necesario si hay conflicto entre el modelo de dominio y el esquema de BD)
public class ProductoDbModel
{
    public Guid prod_id { get; set; }
    public string prod_nombre { get; set; }
    public decimal prod_precio { get; set; }
}
```

> **Recomendación**: en la mayoría de proyectos nuevos con EF Core Code-First, **no se necesita** un modelo de persistencia separado. EF Core mapea directamente las entidades de dominio mediante configuraciones Fluent API. Solo se justifica si se trabaja con una base de datos legada con esquema fijo.

---

### Use Case / Caso de Uso

Representa **una intención del usuario** o del sistema. Es la unidad mínima de lógica de aplicación. Orquesta el dominio para cumplir un objetivo de negocio.

En CQRS se implementa como un `Command` (escribe) o una `Query` (lee).

---

### CQRS (Command Query Responsibility Segregation)

Patrón que separa las operaciones de **escritura** (Commands) de las de **lectura** (Queries). Cada una tiene su propio modelo.

- **Command**: acción que cambia estado. Ejemplo: `CrearProductoCommand`.
- **Query**: consulta que devuelve datos sin modificar nada. Ejemplo: `ObtenerProductosQuery`.

No implica dos bases de datos (eso es CQRS avanzado). En la mayoría de proyectos: misma BD, misma ORM, lógica separada.

---

### MediatR

Librería .NET que implementa el patrón **Mediator**. Desacopla quien envía una solicitud de quien la maneja. El Controller (o componente Blazor) envía un Command/Query a través de MediatR, y el Handler correspondiente lo procesa.

```
Controller → IMediator.Send(command) → Handler → Repositorio → DB
```

Beneficio: el Controller no conoce la implementación del Handler. Solo conoce el contrato (el Command/Query).

---

### Repository Pattern

Abstracción que aísla la lógica de acceso a datos. Expone métodos de dominio (`ObtenerPorId`, `Guardar`, `Eliminar`) sin revelar cómo se implementa la persistencia.

- La **interfaz** vive en `Application` (el dominio la define).
- La **implementación** vive en `Infrastructure` (EF Core la implementa).

---

### Razor Class Library (RCL)

Proyecto especial de .NET que puede contener componentes Blazor (`.razor`), páginas, CSS, JavaScript y otros recursos. Puede ser **referenciado tanto por un proyecto Blazor Web como por un proyecto MAUI**, permitiendo compartir UI entre plataformas.

---

### Clean Architecture

Estilo arquitectónico propuesto por Robert C. Martin que organiza el sistema en capas concéntricas donde la **regla de dependencia** establece que el código solo puede apuntar hacia adentro (hacia el dominio). Las capas externas (infraestructura, UI) dependen de las internas (aplicación, dominio), nunca al revés.

---

## 2. Visión General de la Solución

```
┌─────────────────────────────────────────────────────────────────────┐
│                          MyProject.sln                              │
│                                                                     │
│   PRESENTACIÓN                                                      │
│   ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐  │
│   │  WebFront        │  │    Desktop       │  │     WebAPI       │  │
│   │  (Blazor WASM    │  │  (MAUI Hybrid)   │  │  (ASP.NET Core)  │  │
│   │  o Server)       │  │                  │  │                  │  │
│   └────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘  │
│            │                     │                      │            │
│            └──────────┬──────────┘                      │            │
│                       ↓                                 ↓            │
│   ┌───────────────────────────┐   ┌─────────────────────────────┐  │
│   │       Shared.UI           │   │        Application           │  │
│   │  (Razor Class Library)    │   │  (Use Cases, DTOs, Contratos)│  │
│   └───────────────────────────┘   └──────────────┬──────────────┘  │
│                                                   ↓                 │
│                                   ┌─────────────────────────────┐  │
│                                   │           Domain             │  │
│                                   │  (Entidades, Value Objects,  │  │
│                                   │   Reglas de negocio puras)   │  │
│                                   └──────────────┬──────────────┘  │
│                                                  ↑                  │
│                                   ┌──────────────────────────────┐ │
│                                   │        Infrastructure         │ │
│                                   │  (EF Core, Repos, Servicios  │ │
│                                   │   externos, Email, Storage)  │ │
│                                   └──────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

**WebFront** y **Desktop** se comunican con **WebAPI** exclusivamente vía HTTP/REST. Nunca referencian directamente los proyectos de dominio o infraestructura de la API.

---

## 3. Estructura de Proyectos en la Solución

```
MyProject.sln
│
├── src/
│   │
│   ├── core/
│   │   ├── MyProject.Domain/
│   │   └── MyProject.Application/
│   │
│   ├── infrastructure/
│   │   └── MyProject.Infrastructure/
│   │
│   ├── presentation/
│   │   ├── MyProject.WebAPI/
│   │   ├── MyProject.WebFront/
│   │   ├── MyProject.Desktop/
│   │   └── MyProject.Shared.UI/
│   │
│   └── shared/
│       └── MyProject.Contracts/          ← (Opcional) Contratos compartidos entre API y clientes
│
└── tests/
    ├── MyProject.Domain.Tests/
    ├── MyProject.Application.Tests/
    └── MyProject.Integration.Tests/
```

### Nota sobre `MyProject.Contracts` (opcional pero valioso)

Si la WebAPI y los clientes (WebFront, Desktop) comparten los mismos DTOs de request/response, estos pueden vivir en un proyecto separado `Contracts` referenciado por ambos lados. Evita duplicar las definiciones de DTOs.

---

## 4. Proyecto: Domain

**Tipo de proyecto**: Class Library (.NET)  
**Dependencias externas**: ninguna (cero NuGet packages de infraestructura)

```
MyProject.Domain/
│
├── Entities/
│   ├── Producto.cs
│   ├── Cliente.cs
│   └── Pedido.cs
│
├── ValueObjects/
│   ├── Dinero.cs
│   ├── Direccion.cs
│   └── Email.cs
│
├── Enums/
│   └── EstadoPedido.cs
│
├── Exceptions/
│   └── DomainException.cs
│
├── Events/                              ← Domain Events (avanzado, para notificar cambios)
│   └── ProductoCreadoEvent.cs
│
└── Interfaces/                          ← Interfaces de repositorios que define el dominio
    ├── IProductoRepository.cs
    └── IClienteRepository.cs
```

**Qué NO entra aquí**: DTOs, ViewModels, atributos de EF Core, referencias a HttpClient, nada de System.Web.

---

## 5. Proyecto: Application

**Tipo de proyecto**: Class Library (.NET)  
**Dependencias**: `MyProject.Domain`, `MediatR`, `FluentValidation`, `AutoMapper`

Este proyecto organiza los casos de uso. La estructura recomendada es **por Feature** (Vertical Slice dentro de la capa):

```
MyProject.Application/
│
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs        ← Pipeline MediatR: valida antes de ejecutar
│   │   └── LoggingBehavior.cs           ← Pipeline MediatR: loguea cada operación
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   └── ValidationException.cs
│   └── Mappings/
│       └── MappingProfile.cs            ← Perfiles AutoMapper globales
│
├── Features/
│   │
│   ├── Productos/
│   │   ├── Commands/
│   │   │   ├── CrearProducto/
│   │   │   │   ├── CrearProductoCommand.cs      ← record con los datos de entrada
│   │   │   │   ├── CrearProductoHandler.cs      ← lógica del caso de uso
│   │   │   │   └── CrearProductoValidator.cs    ← reglas de validación (FluentValidation)
│   │   │   └── EliminarProducto/
│   │   │       ├── EliminarProductoCommand.cs
│   │   │       └── EliminarProductoHandler.cs
│   │   │
│   │   ├── Queries/
│   │   │   ├── ObtenerProductos/
│   │   │   │   ├── ObtenerProductosQuery.cs
│   │   │   │   ├── ObtenerProductosHandler.cs
│   │   │   │   └── ProductoDto.cs               ← DTO de respuesta de esta query
│   │   │   └── ObtenerProductoPorId/
│   │   │       ├── ObtenerProductoPorIdQuery.cs
│   │   │       └── ObtenerProductoPorIdHandler.cs
│   │   │
│   │   └── DTOs/
│   │       └── ProductoResponseDto.cs           ← DTO compartido entre queries/commands
│   │
│   └── Clientes/
│       ├── Commands/...
│       └── Queries/...
│
└── Interfaces/
    ├── ICurrentUserService.cs           ← Contrato para obtener el usuario autenticado actual
    └── IEmailService.cs                 ← Contrato para envío de emails (implementado en Infra)
```

**Ejemplo de Command + Handler:**

```csharp
// CrearProductoCommand.cs
public record CrearProductoCommand(string Nombre, decimal Precio) : IRequest<Guid>;

// CrearProductoValidator.cs
public class CrearProductoValidator : AbstractValidator<CrearProductoCommand>
{
    public CrearProductoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Precio).GreaterThan(0);
    }
}

// CrearProductoHandler.cs
public class CrearProductoHandler : IRequestHandler<CrearProductoCommand, Guid>
{
    private readonly IProductoRepository _repo;

    public CrearProductoHandler(IProductoRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CrearProductoCommand request, CancellationToken ct)
    {
        var producto = Producto.Crear(request.Nombre, request.Precio);
        await _repo.AgregarAsync(producto, ct);
        return producto.Id;
    }
}
```

---

## 6. Proyecto: Infrastructure

**Tipo de proyecto**: Class Library (.NET)  
**Dependencias**: `MyProject.Application`, `MyProject.Domain`, `EF Core`, `SendGrid/MailKit`, etc.

```
MyProject.Infrastructure/
│
├── Persistence/
│   ├── AppDbContext.cs                  ← DbContext de EF Core
│   ├── Configurations/                  ← Configuraciones Fluent API por entidad
│   │   ├── ProductoConfiguration.cs
│   │   └── ClienteConfiguration.cs
│   ├── Repositories/                    ← Implementaciones concretas de las interfaces del dominio
│   │   ├── ProductoRepository.cs
│   │   └── ClienteRepository.cs
│   └── Migrations/                      ← Generado automáticamente por EF Core
│
├── Services/
│   ├── EmailService.cs                  ← Implementa IEmailService de Application
│   └── StorageService.cs               ← Implementa IStorageService de Application
│
└── DependencyInjection.cs               ← Extension method para registrar todo en el DI container
```

**Configuración Fluent API** (evita contaminar el dominio con atributos de EF Core):

```csharp
// Configurations/ProductoConfiguration.cs
public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Precio).HasColumnType("decimal(18,2)");
    }
}
```

---

## 7. Proyecto: WebAPI

**Tipo de proyecto**: ASP.NET Core Web API  
**Dependencias**: `MyProject.Application`, `MyProject.Infrastructure`

El WebAPI es una **capa delgada de entrada**. Los controllers no contienen lógica: solo reciben peticiones HTTP, las convierten en Commands/Queries, y devuelven el resultado.

```
MyProject.WebAPI/
│
├── Controllers/
│   ├── ProductosController.cs
│   └── ClientesController.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs   ← Captura excepciones y devuelve ProblemDetails
│   └── RequestLoggingMiddleware.cs
│
├── Filters/                             ← (Opcional) Filtros de autorización custom
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs  ← Registro de servicios, Swagger, CORS, etc.
│
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

**Ejemplo de Controller delgado:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken ct)
    {
        var resultado = await _mediator.Send(new ObtenerProductosQuery(), ct);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProductoCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, null);
    }
}
```

> El Controller no valida, no accede a repositorios, no mapea entidades. Solo delega a MediatR.

---

## 8. Proyecto: WebFront (Blazor)

**Tipo de proyecto**: Blazor WebAssembly (standalone) o Blazor Server  
**Dependencias**: `MyProject.Shared.UI`, `Refit` (o HttpClient)

Esta es la aplicación web de cara al usuario. Se conecta a la WebAPI exclusivamente vía HTTP. Es completamente independiente: si la WebAPI cae, el front puede seguir abierto (con mensajes de error apropiados).

### Capas internas del WebFront

```
MyProject.WebFront/
│
├── Pages/                               ← Páginas de la aplicación (rutas)
│   ├── Productos/
│   │   ├── Index.razor                  ← Lista de productos
│   │   ├── Detalle.razor               ← Detalle de un producto
│   │   └── Formulario.razor            ← Crear / Editar
│   └── Clientes/
│       └── Index.razor
│
├── Layout/                              ← Estructura visual (NavMenu, MainLayout)
│   ├── MainLayout.razor
│   └── NavMenu.razor
│
├── Services/                            ← CAPA DE SERVICIOS: conexión con la WebAPI
│   ├── Interfaces/
│   │   ├── IProductoApiService.cs
│   │   └── IClienteApiService.cs
│   └── Implementations/
│       ├── ProductoApiService.cs
│       └── ClienteApiService.cs
│
├── ViewModels/                          ← Modelos de formularios y de vista
│   ├── ProductoFormViewModel.cs
│   └── ProductoListaItemViewModel.cs
│
├── Mappers/                             ← Conversión DTO ↔ ViewModel
│   └── ProductoMapper.cs
│
├── Shared/                              ← Componentes compartidos SOLO dentro del front
│   ├── LoadingSpinner.razor
│   └── ConfirmDialog.razor
│
├── wwwroot/                             ← Assets estáticos (CSS, imágenes)
│
├── appsettings.json                     ← URL base de la API, configuración
└── Program.cs                           ← Registro de servicios, HttpClient, Refit
```

### La capa de Services en el WebFront

Esta es la capa más importante del front. Define cómo se comunica con la WebAPI.

**Principio**: las páginas y componentes Blazor nunca llaman a `HttpClient` directamente. Siempre usan un servicio inyectado vía interfaz. Esto permite testear los componentes sin hacer llamadas HTTP reales.

```csharp
// Services/Interfaces/IProductoApiService.cs
public interface IProductoApiService
{
    Task<List<ProductoResponseDto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<ProductoResponseDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CrearAsync(CrearProductoRequestDto request, CancellationToken ct = default);
    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
```

**Opción A: Implementación manual con HttpClient** (simple, sin dependencias extra):

```csharp
// Services/Implementations/ProductoApiService.cs
public class ProductoApiService : IProductoApiService
{
    private readonly HttpClient _http;

    public ProductoApiService(HttpClient http) => _http = http;

    public async Task<List<ProductoResponseDto>> ObtenerTodosAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<ProductoResponseDto>>("api/productos", ct)
               ?? new List<ProductoResponseDto>();
    }

    public async Task<Guid> CrearAsync(CrearProductoRequestDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/productos", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
    }
}
```

**Opción B: Refit** (recomendado para proyectos de tamaño medio/grande):

Refit genera la implementación automáticamente a partir de una interfaz decorada con atributos HTTP. Elimina todo el boilerplate.

```csharp
// Services/IProductoApiService.cs (con Refit, la interfaz ES la implementación)
public interface IProductoApiService
{
    [Get("/api/productos")]
    Task<List<ProductoResponseDto>> ObtenerTodosAsync(CancellationToken ct = default);

    [Get("/api/productos/{id}")]
    Task<ProductoResponseDto> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/productos")]
    Task<ApiResponse<Guid>> CrearAsync([Body] CrearProductoRequestDto request, CancellationToken ct = default);

    [Delete("/api/productos/{id}")]
    Task<IApiResponse> EliminarAsync(Guid id, CancellationToken ct = default);
}
```

Registro en `Program.cs`:

```csharp
// Program.cs
builder.Services
    .AddRefitClient<IProductoApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["ApiBase"]!));
```

> Con Refit: no hay clase de implementación. Refit la genera en tiempo de compilación. La interfaz es el contrato y la implementación al mismo tiempo.

### Comparativa de opciones para la capa de servicios del front

| Criterio | HttpClient Manual | Refit |
|---|---|---|
| Código a escribir | Mucho (serialización, URLs, errores) | Mínimo (solo la interfaz) |
| Curva de aprendizaje | Baja | Baja |
| Manejo de errores | Manual | Con `ApiResponse<T>` automático |
| Soporte para auth headers | Manual | Mediante `DelegatingHandler` |
| Testabilidad | Mockear la interfaz | Mockear la interfaz |
| Recomendado para | Proyectos pequeños o con pocos endpoints | Proyectos con múltiples endpoints |

### Flujo completo dentro del WebFront

```
Usuario interactúa con la UI
        ↓
[Componente Blazor (.razor)]
        ↓
[ViewModel / Estado del componente]
        ↓
[IProductoApiService] (inyectado, la página no sabe si es Refit, HttpClient, o mock)
        ↓
[HTTP GET/POST/PUT/DELETE]
        ↓
[WebAPI REST]
        ↓
[Respuesta DTO]
        ↓
[Mapper: DTO → ViewModel]
        ↓
[La vista se actualiza con StateHasChanged()]
```

### Ejemplo de una página Blazor con este patrón

```razor
@* Pages/Productos/Index.razor *@
@page "/productos"
@inject IProductoApiService ProductoService

<h3>Productos</h3>

@if (_cargando)
{
    <LoadingSpinner />
}
else
{
    <ProductosTabla Productos="_productos" OnEliminar="EliminarProducto" />
}

@code {
    private List<ProductoListaItemViewModel> _productos = new();
    private bool _cargando = true;

    protected override async Task OnInitializedAsync()
    {
        var dtos = await ProductoService.ObtenerTodosAsync();
        _productos = dtos.Select(ProductoMapper.ToViewModel).ToList();
        _cargando = false;
    }

    private async Task EliminarProducto(Guid id)
    {
        await ProductoService.EliminarAsync(id);
        _productos.RemoveAll(p => p.Id == id);
    }
}
```

> El componente no sabe nada de HTTP. Solo conoce su ViewModel y el servicio. Es testeable y reemplazable.

---

## 9. Proyecto: Shared.UI (Razor Class Library)

**Tipo de proyecto**: Razor Class Library (RCL)  
**Dependencias**: mínimas (solo las necesarias para los componentes)

Contiene los componentes Blazor que son **idénticos** entre WebFront y Desktop. No contiene páginas con rutas específicas de una sola plataforma.

```
MyProject.Shared.UI/
│
├── Components/
│   ├── Buttons/
│   │   ├── PrimaryButton.razor
│   │   └── DangerButton.razor
│   ├── Tables/
│   │   ├── DataTable.razor
│   │   └── DataTableColumn.razor
│   ├── Forms/
│   │   ├── InputText.razor
│   │   └── SelectField.razor
│   └── Feedback/
│       ├── AlertBanner.razor
│       ├── LoadingSpinner.razor
│       └── ConfirmDialog.razor
│
├── wwwroot/
│   └── css/
│       └── shared.css                   ← Estilos compartidos entre plataformas
│
└── _Imports.razor                       ← Using globales para todos los componentes
```

**Qué NO entra aquí**:
- Páginas con `@page` de una sola plataforma.
- Llamadas a servicios de la API (eso es responsabilidad de cada app).
- Lógica de negocio.

---

## 10. Proyecto: Desktop (MAUI Blazor Hybrid)

**Tipo de proyecto**: .NET MAUI  
**Dependencias**: `MyProject.Shared.UI`, `Refit` (o HttpClient)

### La distinción crítica: Hybrid vs WebView remoto

```
❌ MAL: MAUI apunta con WebView a https://mi-web.com
   El desktop es un navegador empaquetado. No es una app de escritorio real.

✓ BIEN: MAUI Blazor Hybrid consume la misma Razor Class Library que el WebFront.
   Los componentes se renderizan nativamente dentro de la app. Funciona offline.
```

### Estructura interna del Desktop

```
MyProject.Desktop/
│
├── Pages/                               ← Páginas específicas del desktop (si difieren del web)
│   └── Inicio.razor
│
├── Services/                            ← Misma capa de servicios que el WebFront
│   ├── Interfaces/
│   │   └── IProductoApiService.cs       ← Puede reutilizarse de un proyecto Contracts
│   └── Implementations/
│       └── ProductoApiService.cs        ← Idéntica implementación o específica para desktop
│
├── Platforms/                           ← Código específico por plataforma (Windows, Mac, Android)
│   ├── Windows/
│   ├── MacCatalyst/
│   └── Android/
│
├── wwwroot/
│
├── MainPage.xaml                        ← Contiene el BlazorWebView
├── MauiProgram.cs                       ← Registro de servicios, configuración de MAUI
└── appsettings.json
```

### Cómo el Desktop comparte componentes con el WebFront

```xml
<!-- MyProject.Desktop.csproj -->
<ItemGroup>
  <ProjectReference Include="..\MyProject.Shared.UI\MyProject.Shared.UI.csproj" />
</ItemGroup>
```

```xml
<!-- MainPage.xaml -->
<BlazorWebView HostPage="wwwroot/index.html">
    <BlazorWebView.RootComponents>
        <RootComponent Selector="#app" ComponentType="{x:Type local:Routes}" />
    </BlazorWebView.RootComponents>
</BlazorWebView>
```

Los componentes de `Shared.UI` se usan exactamente igual que en el WebFront. El mismo `<DataTable>`, el mismo `<ConfirmDialog>`.

---

## 11. Comunicación Front → API: Capa de Servicios

### ¿Dónde vive la capa de servicios?

Cada aplicación cliente (WebFront y Desktop) tiene su **propia capa de servicios**. No comparten la implementación de los servicios HTTP porque:

1. Puede haber diferencias en el manejo de autenticación por plataforma.
2. El Desktop puede tener servicios adicionales (acceso a archivos locales, notificaciones nativas).
3. Mantiene independencia entre proyectos.

Sin embargo, las **interfaces** pueden compartirse en el proyecto opcional `MyProject.Contracts`.

### Manejo de autenticación en la capa de servicios

Tanto en WebFront como en Desktop, se usa un `DelegatingHandler` que inyecta automáticamente el token JWT en cada petición:

```csharp
// Services/AuthorizationMessageHandler.cs
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;

    public AuthorizationMessageHandler(ITokenService tokenService) => _tokenService = tokenService;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
```

Registro con Refit:

```csharp
builder.Services
    .AddTransient<AuthorizationMessageHandler>()
    .AddRefitClient<IProductoApiService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
```

---

## 12. Diagrama Final Completo

```
USUARIO WEB              USUARIO DESKTOP
     │                        │
     ▼                        ▼
┌──────────────┐     ┌─────────────────┐
│  WebFront    │     │    Desktop      │
│  (Blazor)    │     │ (MAUI Hybrid)   │
│              │     │                 │
│  ┌─────────┐ │     │  ┌───────────┐  │
│  │  Pages  │ │     │  │  Pages    │  │
│  └────┬────┘ │     │  └─────┬─────┘  │
│       │      │     │        │        │
│  ┌────▼────┐ │     │  ┌─────▼─────┐  │
│  │Services │ │     │  │ Services  │  │
│  │(Refit)  │ │     │  │ (Refit)   │  │
│  └────┬────┘ │     │  └─────┬─────┘  │
└───────│──────┘     └────────│────────┘
        │                     │
        └──────────┬──────────┘
                   │  HTTP/REST
                   ▼
        ┌─────────────────────┐
        │       WebAPI        │
        │   (ASP.NET Core)    │
        │                     │
        │  ┌───────────────┐  │
        │  │  Controllers  │  │  ← Thin layer
        │  └───────┬───────┘  │
        │          │ MediatR   │
        │  ┌───────▼───────┐  │
        │  │  Application  │  │  ← Commands / Queries / Handlers
        │  │  Use Cases    │  │
        │  └───────┬───────┘  │
        │          │          │
        │  ┌───────▼───────┐  │
        │  │    Domain     │  │  ← Entidades, reglas de negocio
        │  └───────┬───────┘  │
        │          │          │
        │  ┌───────▼───────┐  │
        │  │Infrastructure │  │  ← EF Core, repositorios, servicios externos
        │  └───────┬───────┘  │
        └──────────│──────────┘
                   │
                   ▼
             ┌───────────┐
             │ Base de   │
             │   Datos   │
             └───────────┘

        ┌───────────────────────────┐
        │        Shared.UI          │  ← Componentes Blazor reutilizables
        │    (Razor Class Library)  │     consumidos por WebFront y Desktop
        └───────────────────────────┘
```

---

## 13. Reglas de Dependencia entre Proyectos

Esta tabla define qué proyecto puede referenciar a cuál. Si va en contra de estas reglas, la arquitectura se degrada.

| Proyecto | Puede referenciar | NO puede referenciar |
|---|---|---|
| `Domain` | Nadie | Application, Infrastructure, WebAPI, WebFront, Desktop |
| `Application` | Domain | Infrastructure, WebAPI, WebFront, Desktop |
| `Infrastructure` | Application, Domain | WebAPI, WebFront, Desktop |
| `WebAPI` | Application, Infrastructure | WebFront, Desktop, Shared.UI |
| `WebFront` | Shared.UI, Contracts | Domain, Application, Infrastructure, WebAPI |
| `Desktop` | Shared.UI, Contracts | Domain, Application, Infrastructure, WebAPI |
| `Shared.UI` | (mínimo) | Domain, Application, Infrastructure, WebAPI |
| `Contracts` | (nada o mínimo) | Domain, Application, Infrastructure |

> **La violación más común**: que el WebFront o el Desktop referencien directamente `Application` o `Domain` para "ahorrar un viaje HTTP". Esto acopla la UI al backend y arruina la independencia de despliegue.

---

## Resumen de Decisiones Tecnológicas

| Capa | Decisión | Tecnología |
|---|---|---|
| Dominio | Entidades puras en C# | .NET 9 Class Library |
| Casos de uso | CQRS | MediatR 12 |
| Validaciones | Pipeline automático | FluentValidation |
| Mapeo | DTO ↔ Entidad | AutoMapper o Mapeo manual (records) |
| Persistencia | ORM Code-First | EF Core 9 |
| API | REST con documentación | ASP.NET Core + Scalar/Swagger |
| Front web | SPA con .NET | Blazor WebAssembly o Server |
| Componentes compartidos | Reutilización multiplataforma | Razor Class Library |
| Desktop | App nativa con UI web | MAUI Blazor Hybrid |
| Cliente HTTP | Sin boilerplate | Refit |
| Autenticación | JWT + Bearer | ASP.NET Core Identity |
