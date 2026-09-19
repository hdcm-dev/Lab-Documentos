#!/usr/bin/env bash
# Laboratorio de la guía de arquitectura .NET (L00..L24).
# Se ejecuta dentro de mcr.microsoft.com/dotnet/sdk:10.0 con esta carpeta montada en /lab:
#   docker run --rm --user "$(id -u):$(id -g)" -e IMAGEN=... -e DIGEST=... \
#     -v "$PWD":/lab -w /lab mcr.microsoft.com/dotnet/sdk:10.0 bash lab.sh
# Produce: capturas/Lnn-*.txt (salida literal con encabezado), aserciones.log y MyProject/ (código final sin bin/obj).
set -u
export HOME=/tmp/home DOTNET_CLI_HOME=/tmp/home DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p "$HOME"
LAB=/lab
CAP=$LAB/capturas
W=/tmp/work            # la solución se construye aquí y se copia a $LAB/src al final
SLN=$W/MyProject
API=http://127.0.0.1:5180
rm -rf "$CAP" "$W" "$LAB/MyProject"; mkdir -p "$CAP" "$W"
: > "$LAB/aserciones.log"

# cap ID nombre comando... : ejecuta el comando en bash, guarda salida literal con encabezado.
cap() {
  local id=$1 name=$2; shift 2
  local f="$CAP/$id-$name.txt"
  {
    echo "# $id — $name"
    echo "# fecha: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
    echo "# imagen: ${IMAGEN:-desconocida}"
    echo "# digest: ${DIGEST:-desconocido}"
    echo "# sdk: $(dotnet --version)"
    echo "# directorio: $(pwd)"
    echo "# comando: $*"
    echo "# ---"
  } > "$f"
  bash -c "$*" >> "$f" 2>&1
  local rc=$?
  printf "\n# --- código de salida: %s\n" "$rc" >> "$f"
  LAST=$f; LASTRC=$rc
}
# ok ID descripción patrón [archivo] : aserción sobre lo que no cambia entre equipos.
# body ARCHIVO: solo la salida del comando (sin el encabezado, que repite el comando y daría falsos positivos).
body() { awk 'f{print} /^# ---$/{f=1}' "$1" | grep -v '^# --- código de salida'; }
ok() {
  local id=$1 desc=$2 pat=$3 f=${4:-$LAST}
  if body "$f" | grep -Eq -- "$pat"; then echo "PASS $id $desc" >> "$LAB/aserciones.log"
  else echo "FAIL $id $desc (patrón: $pat)" >> "$LAB/aserciones.log"; fi
}
nok() {  # la salida NO debe contener el patrón
  local id=$1 desc=$2 pat=$3 f=${4:-$LAST}
  if body "$f" | grep -Eq -- "$pat"; then echo "FAIL $id $desc (no debía aparecer: $pat)" >> "$LAB/aserciones.log"
  else echo "PASS $id $desc" >> "$LAB/aserciones.log"; fi
}
rc_is() { if [ "$LASTRC" = "$2" ]; then echo "PASS $1 código de salida $2" >> "$LAB/aserciones.log"; else echo "FAIL $1 código de salida $LASTRC (esperado $2)" >> "$LAB/aserciones.log"; fi; }
rc_not0() { if [ "$LASTRC" != "0" ]; then echo "PASS $1 código de salida distinto de 0 ($LASTRC)" >> "$LAB/aserciones.log"; else echo "FAIL $1 código de salida 0 (se esperaba error)" >> "$LAB/aserciones.log"; fi; }

start_api() {  # receta DR-20
  if [ "$(curl -s -o /dev/null -w '%{http_code}' $API/api/productos)" != "000" ]; then
    echo "FAIL start_api($1): el puerto ya estaba ocupado por otra instancia" >> "$LAB/aserciones.log"
  fi
  API_NAME=$1
  ( cd "$SLN/src/Backend/MyProject.WebAPI" && ASPNETCORE_ENVIRONMENT=Development setsid dotnet run --no-launch-profile --urls $API > "$W/api-$1.log" 2>&1 & echo $! > "$W/api.pid" )
  for i in $(seq 1 90); do
    c=$(curl -s -o /dev/null -w '%{http_code}' $API/api/productos) ; [ "$c" != "000" ] && break; sleep 1
  done
}
stop_api() {
  kill -TERM -"$(cat "$W/api.pid")" 2>/dev/null
  pkill -TERM -f 'MyProject.WebAPI' 2>/dev/null
  for i in $(seq 1 30); do
    [ "$(curl -s -o /dev/null -w '%{http_code}' $API/api/productos)" = "000" ] && break; sleep 1
  done
  mkdir -p "$CAP/api-logs"; cp "$W/api-$API_NAME.log" "$CAP/api-logs/" 2>/dev/null
}

# ---------------------------------------------------------------- L00 entorno
cd "$W"
cap L00 entorno 'dotnet --info | head -20; echo; curl --version | head -1'
ok L00 "SDK 10 presente" '^ Version: +10\.'

# ---------------------------------------------------------------- L01 solución
mkdir -p "$SLN"; cd "$SLN"
cap L01 nueva-solucion 'dotnet new sln -n MyProject && dotnet new globaljson --sdk-version "$(dotnet --version)" --roll-forward latestFeature && ls'
ok L01 "se crea MyProject.slnx" 'MyProject\.slnx'
ok L01 "se crea global.json" 'global\.json'

# ---------------------------------------------------------------- L02 dos proyectos y una referencia
cap L02 dos-proyectos 'dotnet new classlib -n MyProject.Domain -o src/Backend/MyProject.Domain && dotnet new classlib -n MyProject.Infrastructure -o src/Backend/MyProject.Infrastructure && rm src/Backend/*/Class1.cs && dotnet sln add src/Backend/MyProject.Domain src/Backend/MyProject.Infrastructure && dotnet add src/Backend/MyProject.Infrastructure reference src/Backend/MyProject.Domain && dotnet list src/Backend/MyProject.Infrastructure reference && dotnet build'
ok L02 "referencia listada" 'MyProject\.Domain\.csproj'
ok L02 "compila" 'Build succeeded'
# Aserción provocada: debe fallar (demuestra que el mecanismo detecta).
if body "$LAST" | grep -Eq 'CS9999'; then echo "PASS L02-provocada (no debía pasar)" >> "$LAB/aserciones.log"; else echo "FAIL L02-provocada patrón CS9999 inexistente — fallo provocado a propósito" >> "$LAB/aserciones.log"; fi

# ---------------------------------------------------------------- L03 using sin referencia
mkdir -p src/Backend/MyProject.Infrastructure/Persistence
cat > src/Backend/MyProject.Infrastructure/Persistence/ConexionSql.cs <<'CS'
namespace MyProject.Infrastructure.Persistence;

public class ConexionSql
{
    public void Ejecutar(string sql) { }
}
CS
cat > src/Backend/MyProject.Domain/Producto.cs <<'CS'
using MyProject.Infrastructure.Persistence;

namespace MyProject.Domain;

public class Producto
{
    public string Nombre { get; set; } = "";

    public void Guardar(ConexionSql conexion) =>
        conexion.Ejecutar($"INSERT INTO Productos VALUES ('{Nombre}')");
}
CS
cap L03 using-sin-referencia 'dotnet build src/Backend/MyProject.Domain'
ok L03 "CS0234: el espacio de nombres no existe para Domain" 'error CS0234'
ok L03 "CS0246: el tipo no se encuentra" 'error CS0246'
rc_not0 L03

# ---------------------------------------------------------------- L04 ciclo
cap L04 ciclo-agregar 'dotnet add src/Backend/MyProject.Domain reference src/Backend/MyProject.Infrastructure'
ok L04 "el comando acepta la referencia inversa" 'added to the project'
rc_is L04 0
cap L04 ciclo-compilar 'dotnet build'
ok L04 "MSB4006 al compilar" 'error MSB4006'
rc_not0 L04

# ---------------------------------------------------------------- L05 deshacer
cap L05 quitar-con-carpeta 'dotnet remove src/Backend/MyProject.Domain reference src/Backend/MyProject.Infrastructure; echo "código: $?"; grep -n ProjectReference src/Backend/MyProject.Domain/MyProject.Domain.csproj'
ok L05 "con la carpeta no encuentra la referencia" 'could not be found'
rc_is L05 0
ok L05 "y la referencia sigue en el csproj" 'ProjectReference Include'
cap L05 quitar-con-csproj 'dotnet remove src/Backend/MyProject.Domain reference src/Backend/MyProject.Infrastructure/MyProject.Infrastructure.csproj && rm src/Backend/MyProject.Domain/Producto.cs src/Backend/MyProject.Infrastructure/Persistence/ConexionSql.cs && dotnet build'
ok L05 "con el csproj la quita" 'removed'
ok L05 "vuelve a compilar" 'Build succeeded'

# ---------------------------------------------------------------- L06 / L07 todo junto vs separado
mkdir -p "$W/TodoJunto"; cd "$W/TodoJunto"
dotnet new classlib -n TodoJunto -o . >/dev/null 2>&1; rm -f Class1.cs
cat > ConexionSql.cs <<'CS'
namespace TodoJunto;

public class ConexionSql
{
    public void Ejecutar(string sql) { }
}
CS
cat > Producto.cs <<'CS'
namespace TodoJunto;

public class Producto
{
    public string Nombre { get; set; } = "";

    public void Guardar(ConexionSql conexion) =>
        conexion.Ejecutar($"INSERT INTO Productos VALUES ('{Nombre}')");
}
CS
cap L06 todo-junto 'dotnet build'
ok L06 "un solo proyecto: la mezcla compila" 'Build succeeded'
cd "$SLN"
cp "$W/TodoJunto/Producto.cs" src/Backend/MyProject.Domain/Producto.cs
sed -i 's/namespace TodoJunto;/using MyProject.Infrastructure.Persistence;\n\nnamespace MyProject.Domain;/' src/Backend/MyProject.Domain/Producto.cs
mkdir -p src/Backend/MyProject.Infrastructure/Persistence
sed 's/namespace TodoJunto;/namespace MyProject.Infrastructure.Persistence;/' "$W/TodoJunto/ConexionSql.cs" > src/Backend/MyProject.Infrastructure/Persistence/ConexionSql.cs
cap L07 separado 'cat src/Backend/MyProject.Domain/Producto.cs; echo; dotnet build src/Backend/MyProject.Domain'
ok L07 "separado: el mismo uso no compila" 'error CS0234'
rc_not0 L07
rm src/Backend/MyProject.Domain/Producto.cs src/Backend/MyProject.Infrastructure/Persistence/ConexionSql.cs

# ---------------------------------------------------------------- L08 esqueleto del backend
cap L08 esqueleto 'dotnet new classlib -n MyProject.Application -o src/Backend/MyProject.Application && rm src/Backend/MyProject.Application/Class1.cs && dotnet new webapi --use-controllers --no-https -n MyProject.WebAPI -o src/Backend/MyProject.WebAPI && rm -f src/Backend/MyProject.WebAPI/WeatherForecast.cs src/Backend/MyProject.WebAPI/Controllers/WeatherForecastController.cs src/Backend/MyProject.WebAPI/MyProject.WebAPI.http && dotnet sln add src/Backend/MyProject.Application src/Backend/MyProject.WebAPI && dotnet add src/Backend/MyProject.Application reference src/Backend/MyProject.Domain && dotnet add src/Backend/MyProject.Infrastructure reference src/Backend/MyProject.Application && dotnet add src/Backend/MyProject.WebAPI reference src/Backend/MyProject.Application src/Backend/MyProject.Infrastructure && for p in Domain Application Infrastructure WebAPI; do echo "== MyProject.$p"; dotnet list src/Backend/MyProject.$p reference; done && dotnet build'
ok L08 "Domain sin referencias" 'There are no Project to Project references'
ok L08 "compila" 'Build succeeded'
ok L08 "cero advertencias" '0 Warning\(s\)'

# ---------------------------------------------------------------- L09 Domain
D=src/Backend/MyProject.Domain
mkdir -p $D/Common $D/Productos
cat > $D/Common/DomainException.cs <<'CS'
namespace MyProject.Domain.Common;

/// <summary>Se lanza cuando una operación violaría una regla del negocio.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
CS
cat > $D/Common/Dinero.cs <<'CS'
namespace MyProject.Domain.Common;

/// <summary>Value object: dos Dinero con el mismo monto y moneda son iguales.</summary>
public record Dinero(decimal Monto, string Moneda)
{
    public Dinero Sumar(Dinero otro)
    {
        if (Moneda != otro.Moneda)
            throw new DomainException("No se pueden sumar montos de monedas distintas.");
        return this with { Monto = Monto + otro.Monto };
    }
}
CS
cat > $D/Productos/Producto.cs <<'CS'
using MyProject.Domain.Common;

namespace MyProject.Domain.Productos;

public class Producto
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; }

    // Constructor privado: la única forma de crear un Producto es Create.
    private Producto() { }

    public static Producto Create(string nombre, decimal precio)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");
        if (precio <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        return new Producto { Id = Guid.NewGuid(), Nombre = nombre, Precio = precio, Activo = true };
    }

    public void Desactivar() => Activo = false;
}
CS
cat > $D/Productos/IProductoRepository.cs <<'CS'
namespace MyProject.Domain.Productos;

/// <summary>Lo que el dominio necesita para guardar y recuperar productos. La implementación vive en Infrastructure.</summary>
public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Producto producto, CancellationToken ct = default);
}
CS
cap L09 domain 'dotnet build src/Backend/MyProject.Domain'
ok L09 "Domain compila solo" 'Build succeeded'

# ---------------------------------------------------------------- L10 setter privado
mkdir -p src/Backend/MyProject.Application/Productos
cat > src/Backend/MyProject.Application/Productos/Intento.cs <<'CS'
using MyProject.Domain.Productos;

namespace MyProject.Application.Productos;

public static class Intento
{
    public static void BajarPrecio()
    {
        var producto = Producto.Create("Mate", 3500m);
        producto.Precio = -1m;
    }
}
CS
cap L10 setter-privado 'dotnet build src/Backend/MyProject.Application'
ok L10 "la asignación desde otro proyecto no compila" 'error CS0(200|272)'
rc_not0 L10
rm src/Backend/MyProject.Application/Productos/Intento.cs

# ---------------------------------------------------------------- L11 Application + tests
A=src/Backend/MyProject.Application/Productos
mkdir -p $A/Commands/CrearProducto $A/Queries/ObtenerProductos $A/Queries/ObtenerProductoPorId
cat > $A/ProductoDto.cs <<'CS'
using MyProject.Domain.Productos;

namespace MyProject.Application.Productos;

/// <summary>Modelo de lectura que devuelven los casos de uso. Mapeo manual desde la entidad.</summary>
public record ProductoDto(Guid Id, string Nombre, decimal Precio, bool Activo)
{
    public static ProductoDto From(Producto producto) =>
        new(producto.Id, producto.Nombre, producto.Precio, producto.Activo);
}
CS
cat > $A/Commands/CrearProducto/CrearProductoCommand.cs <<'CS'
namespace MyProject.Application.Productos.Commands.CrearProducto;

public record CrearProductoCommand(string Nombre, decimal Precio);
CS
cat > $A/Commands/CrearProducto/CrearProductoHandler.cs <<'CS'
using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Commands.CrearProducto;

public class CrearProductoHandler
{
    private readonly IProductoRepository _repository;

    public CrearProductoHandler(IProductoRepository repository) => _repository = repository;

    public async Task<Guid> Handle(CrearProductoCommand command, CancellationToken ct = default)
    {
        var producto = Producto.Create(command.Nombre, command.Precio); // la regla vive en el dominio
        await _repository.AddAsync(producto, ct);
        return producto.Id;
    }
}
CS
cat > $A/Queries/ObtenerProductos/ObtenerProductosQuery.cs <<'CS'
namespace MyProject.Application.Productos.Queries.ObtenerProductos;

public record ObtenerProductosQuery;
CS
cat > $A/Queries/ObtenerProductos/ObtenerProductosHandler.cs <<'CS'
using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Queries.ObtenerProductos;

public class ObtenerProductosHandler
{
    private readonly IProductoRepository _repository;

    public ObtenerProductosHandler(IProductoRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ProductoDto>> Handle(ObtenerProductosQuery query, CancellationToken ct = default)
    {
        var productos = await _repository.GetAllAsync(ct);
        return productos.Select(ProductoDto.From).ToList();
    }
}
CS
cat > $A/Queries/ObtenerProductoPorId/ObtenerProductoPorIdQuery.cs <<'CS'
namespace MyProject.Application.Productos.Queries.ObtenerProductoPorId;

public record ObtenerProductoPorIdQuery(Guid Id);
CS
cat > $A/Queries/ObtenerProductoPorId/ObtenerProductoPorIdHandler.cs <<'CS'
using MyProject.Domain.Productos;

namespace MyProject.Application.Productos.Queries.ObtenerProductoPorId;

public class ObtenerProductoPorIdHandler
{
    private readonly IProductoRepository _repository;

    public ObtenerProductoPorIdHandler(IProductoRepository repository) => _repository = repository;

    public async Task<ProductoDto?> Handle(ObtenerProductoPorIdQuery query, CancellationToken ct = default)
    {
        var producto = await _repository.GetByIdAsync(query.Id, ct);
        return producto is null ? null : ProductoDto.From(producto);
    }
}
CS
dotnet new xunit -n MyProject.Domain.Tests -o tests/Backend/MyProject.Domain.Tests >/dev/null 2>&1
dotnet new xunit -n MyProject.Application.Tests -o tests/Backend/MyProject.Application.Tests >/dev/null 2>&1
rm -f tests/Backend/*/UnitTest1.cs
cat > tests/Backend/MyProject.Domain.Tests/ProductoTests.cs <<'CS'
using MyProject.Domain.Common;
using MyProject.Domain.Productos;

namespace MyProject.Domain.Tests;

public class ProductoTests
{
    [Fact]
    public void Create_con_precio_negativo_lanza_DomainException() =>
        Assert.Throws<DomainException>(() => Producto.Create("Mate", -5m));

    [Fact]
    public void Create_con_datos_validos_devuelve_un_producto_activo()
    {
        var producto = Producto.Create("Mate", 3500m);

        Assert.True(producto.Activo);
        Assert.Equal(3500m, producto.Precio);
    }

    [Fact]
    public void Dos_Dinero_con_los_mismos_datos_son_iguales() =>
        Assert.Equal(new Dinero(10m, "ARS"), new Dinero(10m, "ARS"));
}
CS
cat > tests/Backend/MyProject.Application.Tests/FakeProductoRepository.cs <<'CS'
using MyProject.Domain.Productos;

namespace MyProject.Application.Tests;

/// <summary>Doble de prueba: guarda en una lista. Vive en el proyecto de tests, no en Infrastructure.</summary>
public class FakeProductoRepository : IProductoRepository
{
    public List<Producto> Guardados { get; } = new();

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Guardados.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Producto>>(Guardados);

    public Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        Guardados.Add(producto);
        return Task.CompletedTask;
    }
}
CS
cat > tests/Backend/MyProject.Application.Tests/CrearProductoHandlerTests.cs <<'CS'
using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Domain.Common;

namespace MyProject.Application.Tests;

public class CrearProductoHandlerTests
{
    [Fact]
    public async Task Handle_guarda_el_producto_y_devuelve_su_id()
    {
        var repository = new FakeProductoRepository();
        var handler = new CrearProductoHandler(repository);

        var id = await handler.Handle(new CrearProductoCommand("Yerba 1 kg", 4500m));

        Assert.Single(repository.Guardados);
        Assert.Equal(id, repository.Guardados[0].Id);
    }

    [Fact]
    public async Task Handle_con_precio_negativo_no_guarda_nada()
    {
        var repository = new FakeProductoRepository();
        var handler = new CrearProductoHandler(repository);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new CrearProductoCommand("Yerba 1 kg", -5m)));
        Assert.Empty(repository.Guardados);
    }
}
CS
cap L11 tests 'dotnet sln add tests/Backend/MyProject.Domain.Tests tests/Backend/MyProject.Application.Tests && dotnet add tests/Backend/MyProject.Domain.Tests reference src/Backend/MyProject.Domain && dotnet add tests/Backend/MyProject.Application.Tests reference src/Backend/MyProject.Application && dotnet test'
ok L11 "todas las pruebas pasan" '(Passed!|succeeded)'
nok L11 "sin pruebas falladas" 'Failed!|[1-9][0-9]* failed'
rc_is L11 0

# ---------------------------------------------------------------- L12 regresión detectada
cp $D/Productos/Producto.cs "$W/Producto.cs.bak"
sed -i '/if (precio <= 0)/,+1d' $D/Productos/Producto.cs
cap L12 regresion 'grep -n "precio" src/Backend/MyProject.Domain/Productos/Producto.cs; echo; dotnet test'
ok L12 "falla la prueba de Domain que depende de la regla" 'Failed MyProject\.Domain\.Tests\.ProductoTests\.Create_con_precio_negativo_lanza_DomainException'
ok L12 "falla la prueba de Application que depende de la regla" 'Failed MyProject\.Application\.Tests\.CrearProductoHandlerTests\.Handle_con_precio_negativo_no_guarda_nada'
ok L12 "en Domain falla exactamente una de tres" 'Failed! +- Failed: +1, Passed: +2, Skipped: +0, Total: +3'
ok L12 "en Application falla exactamente una de dos" 'Failed! +- Failed: +1, Passed: +1, Skipped: +0, Total: +2'
rc_not0 L12
cp "$W/Producto.cs.bak" $D/Productos/Producto.cs

# ---------------------------------------------------------------- L13 API con repositorio en memoria
I=src/Backend/MyProject.Infrastructure
mkdir -p $I/Persistence
cat > $I/Persistence/InMemoryProductoRepository.cs <<'CS'
using System.Collections.Concurrent;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence;

/// <summary>Implementación de laboratorio: los datos viven mientras el proceso está en marcha.</summary>
public class InMemoryProductoRepository : IProductoRepository
{
    private readonly ConcurrentDictionary<Guid, Producto> _productos = new();

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_productos.TryGetValue(id, out var producto) ? producto : null);

    public Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Producto>>(_productos.Values.ToList());

    public Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        _productos[producto.Id] = producto;
        return Task.CompletedTask;
    }
}
CS
WA=src/Backend/MyProject.WebAPI
mkdir -p $WA/Contracts $WA/Controllers
cat > $WA/Contracts/CrearProductoRequest.cs <<'CS'
namespace MyProject.Contracts;

public record CrearProductoRequest(string Nombre, decimal Precio);
CS
cat > $WA/Contracts/ProductoResponse.cs <<'CS'
namespace MyProject.Contracts;

public record ProductoResponse(Guid Id, string Nombre, decimal Precio);
CS
cat > $WA/Controllers/ProductosController.cs <<'CS'
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Productos;
using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Application.Productos.Queries.ObtenerProductoPorId;
using MyProject.Application.Productos.Queries.ObtenerProductos;
using MyProject.Contracts;

namespace MyProject.WebAPI.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> GetAll(
        [FromServices] ObtenerProductosHandler handler, CancellationToken ct)
    {
        var productos = await handler.Handle(new ObtenerProductosQuery(), ct);
        return Ok(productos.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductoResponse>> GetById(
        Guid id, [FromServices] ObtenerProductoPorIdHandler handler, CancellationToken ct)
    {
        var producto = await handler.Handle(new ObtenerProductoPorIdQuery(id), ct);
        return producto is null ? NotFound() : Ok(ToResponse(producto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CrearProductoRequest request, [FromServices] CrearProductoHandler handler, CancellationToken ct)
    {
        var id = await handler.Handle(new CrearProductoCommand(request.Nombre, request.Precio), ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // El contrato expone solo lo que promete: Activo no viaja.
    private static ProductoResponse ToResponse(ProductoDto producto) =>
        new(producto.Id, producto.Nombre, producto.Precio);
}
CS
cat > $WA/Program.cs <<'CS'
using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Application.Productos.Queries.ObtenerProductoPorId;
using MyProject.Application.Productos.Queries.ObtenerProductos;
using MyProject.Domain.Productos;
using MyProject.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Composition root: el único lugar que une interfaces con implementaciones.
builder.Services.AddSingleton<IProductoRepository, InMemoryProductoRepository>();
builder.Services.AddScoped<CrearProductoHandler>();
builder.Services.AddScoped<ObtenerProductosHandler>();
builder.Services.AddScoped<ObtenerProductoPorIdHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
CS
cap L13-build compilar-api 'dotnet build'
ok L13 "la solución compila" 'Build succeeded'
ok L13 "cero advertencias" '0 Warning\(s\)'
start_api memoria
cap L13 arranque "cat $W/api-memoria.log"
ok L13 "escucha en el puerto fijado" 'Now listening on: http://127.0.0.1:5180'
ok L13 "entorno Development" 'Hosting environment: Development'

# ---------------------------------------------------------------- L14 / L15 HTTP
cap L14 post "curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Yerba 1 kg\",\"precio\":4500}'"
ok L14 "201 Created" 'HTTP/1.1 201'
ok L14 "Location apunta a GetById" '^Location: http://127.0.0.1:5180/api/productos/[0-9a-f-]{36}'
LOC=$(grep -i '^Location:' "$LAST" | awk '{print $2}' | tr -d '\r')
cap L15 get "curl -s -i $API/api/productos; echo; echo; curl -s -i $LOC; echo; echo; curl -s $API/openapi/v1.json | head -c 400; echo; echo; curl -s -i $API/api/productos/00000000-0000-0000-0000-000000000000 | head -1"
ok L15 "200 OK" 'HTTP/1.1 200'
ok L15 "JSON con nombre" '\"nombre\":\"Yerba 1 kg\"'
nok L15 "Activo no viaja en el contrato" '\"activo\"'
ok L15 "documento OpenAPI" '\"openapi\"'
ok L15 "404 para un id inexistente" 'HTTP/1.1 404'
stop_api

# ---------------------------------------------------------------- L16 EF Core + SQLite
sha256sum $D/bin/Debug/net10.0/MyProject.Domain.dll | cut -d' ' -f1 > "$W/domain-antes.sha"
stat -c '%y' $D/bin/Debug/net10.0/MyProject.Domain.dll > "$W/domain-antes.mtime"
cap L16-paquete agregar-efcore 'dotnet add src/Backend/MyProject.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite && grep -n PackageReference src/Backend/MyProject.Infrastructure/MyProject.Infrastructure.csproj'
ok L16 "paquete EF Core Sqlite 10" 'Microsoft.EntityFrameworkCore.Sqlite" Version="10\.'
mkdir -p $I/Persistence/Configurations $I/Persistence/Repositories
cat > $I/Persistence/AppDbContext.cs <<'CS'
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
CS
cat > $I/Persistence/Configurations/ProductoConfiguration.cs <<'CS'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence.Configurations;

/// <summary>Mapeo desde afuera: Producto no sabe que existe una tabla.</summary>
public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Precio).HasPrecision(18, 2);
    }
}
CS
cat > $I/Persistence/Repositories/ProductoRepository.cs <<'CS'
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Productos;

namespace MyProject.Infrastructure.Persistence.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _db;

    public ProductoRepository(AppDbContext db) => _db = db;

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Producto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Productos.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Producto producto, CancellationToken ct = default)
    {
        _db.Productos.Add(producto);        // marca la entidad para insertar
        await _db.SaveChangesAsync(ct);     // confirma la escritura en la base
    }
}
CS
cat > $I/DependencyInjection.cs <<'CS'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Domain.Productos;
using MyProject.Infrastructure.Persistence;
using MyProject.Infrastructure.Persistence.Repositories;

namespace MyProject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IProductoRepository, ProductoRepository>();
        return services;
    }

    /// <summary>Crea la base si no existe. En un proyecto real se usan migraciones.</summary>
    public static void EnsureDatabaseCreated(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }
}
CS
cp $WA/Program.cs "$W/Program.memoria.cs"
cat > $WA/Program.cs <<'CS'
using MyProject.Application.Productos.Commands.CrearProducto;
using MyProject.Application.Productos.Queries.ObtenerProductoPorId;
using MyProject.Application.Productos.Queries.ObtenerProductos;
using MyProject.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Composition root: cambió la implementación del repositorio; Domain y Application no se tocaron.
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MyProject") ?? "Data Source=myproject.db");
builder.Services.AddScoped<CrearProductoHandler>();
builder.Services.AddScoped<ObtenerProductosHandler>();
builder.Services.AddScoped<ObtenerProductoPorIdHandler>();

var app = builder.Build();

app.Services.EnsureDatabaseCreated();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
CS
rm $I/Persistence/InMemoryProductoRepository.cs
cap L16 compilar-con-efcore 'dotnet build -v n 2>&1 | grep -E "Skipping target \"CoreCompile\"|CoreCompile:|Done Building Project .*MyProject\.(Domain|Application|Infrastructure|WebAPI)\.csproj|Build succeeded|Warning\(s\)|Error\(s\)" ; echo; echo "sha256 Domain.dll antes : $(cat /tmp/work/domain-antes.sha)"; echo "sha256 Domain.dll despues: $(sha256sum src/Backend/MyProject.Domain/bin/Debug/net10.0/MyProject.Domain.dll | cut -d" " -f1)"; echo "fecha Domain.dll antes : $(cat /tmp/work/domain-antes.mtime)"; echo "fecha Domain.dll despues: $(stat -c %y src/Backend/MyProject.Domain/bin/Debug/net10.0/MyProject.Domain.dll)"'
ok L16 "compila con EF Core" 'Build succeeded'
A1=$(cat "$W/domain-antes.sha"); A2=$(sha256sum $D/bin/Debug/net10.0/MyProject.Domain.dll | cut -d' ' -f1)
if [ "$A1" = "$A2" ]; then echo "PASS L16 Domain.dll idéntico antes y después" >> "$LAB/aserciones.log"; else echo "FAIL L16 Domain.dll cambió" >> "$LAB/aserciones.log"; fi

# ---------------------------------------------------------------- L17 mismo contrato, otra base
rm -f $WA/myproject.db*
start_api sqlite
cap L17 mismo-contrato "curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Yerba 1 kg\",\"precio\":4500}' | head -1; curl -s -i $API/api/productos; echo; echo; ls $WA/*.db; echo; grep -rn 'Infrastructure' $WA --include=*.cs"
ok L17 "200 con el producto persistido" '\"nombre\":\"Yerba 1 kg\"'
ok L17 "se creó la base SQLite" 'myproject\.db'
N=$(grep -rln 'Infrastructure' $WA --include=*.cs | wc -l)
if [ "$N" = "1" ]; then echo "PASS L17 un solo archivo .cs de WebAPI nombra Infrastructure (Program.cs)" >> "$LAB/aserciones.log"; else echo "FAIL L17 $N archivos nombran Infrastructure" >> "$LAB/aserciones.log"; fi
stop_api

# ---------------------------------------------------------------- L18 sin SaveChanges
R=$I/Persistence/Repositories/ProductoRepository.cs
cp $R "$W/ProductoRepository.cs.bak"
sed -i 's#^        await _db.SaveChangesAsync(ct);.*#        await Task.CompletedTask;           // (L18) falta confirmar la escritura#' $R
rm -f $WA/myproject.db*
dotnet build >/dev/null 2>&1
start_api sin-save
cap L18 sin-savechanges "grep -n 'Add(\|SaveChanges\|CompletedTask' $R; echo; curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Yerba 1 kg\",\"precio\":4500}' | head -1; curl -s -i $API/api/productos"
ok L18 "el POST responde 201" 'HTTP/1.1 201'
ok L18 "pero la lista queda vacía" '^\[\]$'
stop_api
cp "$W/ProductoRepository.cs.bak" $R
rm -f $WA/myproject.db*
dotnet build >/dev/null 2>&1
start_api con-save
cap L18-corregido con-savechanges "grep -n 'Add(\|SaveChanges' $R; echo; curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Yerba 1 kg\",\"precio\":4500}' | head -1; curl -s $API/api/productos"
ok L18 "con SaveChangesAsync el producto aparece" '\"nombre\":\"Yerba 1 kg\"'

# ---------------------------------------------------------------- L19 / L20 errores
cap L19 json-mal-formado "curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Mate\",\"precio\":}'"
ok L19 "400 por forma" 'HTTP/1.1 400'
ok L19 "ValidationProblemDetails con errors" '\"errors\"'
cap L20 regla-sin-traducir "curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Mate\",\"precio\":-5}' | head -12"
ok L20 "500: la regla rechaza pero nadie traduce" 'HTTP/1.1 500'
ok L20 "la causa es DomainException" 'DomainException'
stop_api

# ---------------------------------------------------------------- L21 traducción en el borde
mkdir -p $WA/ExceptionHandlers
cat > $WA/ExceptionHandlers/DomainExceptionHandler.cs <<'CS'
using Microsoft.AspNetCore.Diagnostics;
using MyProject.Domain.Common;

namespace MyProject.WebAPI.ExceptionHandlers;

/// <summary>Traduce una regla de negocio incumplida a una respuesta HTTP 400 con ProblemDetails.</summary>
public class DomainExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public DomainExceptionHandler(IProblemDetailsService problemDetails) => _problemDetails = problemDetails;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException)
            return false; // otras excepciones siguen su curso (500)

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Regla de negocio incumplida",
                Detail = exception.Message,
            },
        });
    }
}
CS
sed -i 's#^builder.Services.AddOpenApi();#builder.Services.AddOpenApi();\nbuilder.Services.AddProblemDetails();\nbuilder.Services.AddExceptionHandler<DomainExceptionHandler>();#' $WA/Program.cs
sed -i 's#^using MyProject.Infrastructure;#using MyProject.Infrastructure;\nusing MyProject.WebAPI.ExceptionHandlers;#' $WA/Program.cs
sed -i 's#^app.Services.EnsureDatabaseCreated();#app.Services.EnsureDatabaseCreated();\n\napp.UseExceptionHandler();#' $WA/Program.cs
rm -f $WA/myproject.db*
cap L21-build compilar-manejador 'dotnet build src/Backend/MyProject.WebAPI'
ok L21 "compila con el manejador" 'Build succeeded'
start_api manejador
cap L21 regla-traducida "curl -s -i -X POST $API/api/productos -H 'Content-Type: application/json' -d '{\"nombre\":\"Mate\",\"precio\":-5}'"
ok L21 "400" 'HTTP/1.1 400'
ok L21 "ProblemDetails con título propio" 'Regla de negocio incumplida'
ok L21 "content-type problem+json" 'application/problem\+json'

# ---------------------------------------------------------------- L22 nace Contracts y el primer cliente
cap L22-contracts nace-contracts "dotnet new classlib -n MyProject.Contracts -o src/Contracts/MyProject.Contracts && rm src/Contracts/MyProject.Contracts/Class1.cs && mv $WA/Contracts/*.cs src/Contracts/MyProject.Contracts/ && rmdir $WA/Contracts && dotnet sln add src/Contracts/MyProject.Contracts && dotnet add $WA reference src/Contracts/MyProject.Contracts && dotnet new console -n MyProject.ConsoleClient -o src/Clients/MyProject.ConsoleClient && dotnet sln add src/Clients/MyProject.ConsoleClient && dotnet add src/Clients/MyProject.ConsoleClient reference src/Contracts/MyProject.Contracts && dotnet list src/Clients/MyProject.ConsoleClient reference"
ok L22 "el cliente referencia solo Contracts" 'MyProject\.Contracts\.csproj'
nok L22 "el cliente no referencia Domain ni Application" 'MyProject\.(Domain|Application|Infrastructure)\.csproj'
cat > src/Clients/MyProject.ConsoleClient/Program.cs <<'CS'
using System.Net.Http.Json;
using MyProject.Contracts;

// Cliente remoto: conoce el contrato HTTP y nada más.
var baseUrl = args.Length > 0 ? args[0] : "http://127.0.0.1:5180";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

var respuesta = await http.PostAsJsonAsync("api/productos", new CrearProductoRequest("Mate de calabaza", 12000m));
Console.WriteLine($"POST api/productos -> {(int)respuesta.StatusCode} {respuesta.Headers.Location}");

var productos = await http.GetFromJsonAsync<List<ProductoResponse>>("api/productos") ?? new();
foreach (var p in productos)
    Console.WriteLine($"{p.Id}  {p.Nombre,-20} {p.Precio,10}");
CS
stop_api
cap L22-build compilar-solucion 'dotnet build'
ok L22 "la solución compila con Contracts extraído" 'Build succeeded'
ok L22 "cero advertencias" '0 Warning\(s\)'
rm -f $WA/myproject.db*
start_api cliente
cap L22 cliente-consola "dotnet run --project src/Clients/MyProject.ConsoleClient -- $API"
ok L22 "el cliente crea y lista" 'POST api/productos -> 201'
ok L22 "lista el producto" 'Mate de calabaza'
stop_api

# ---------------------------------------------------------------- L23 estructura
cap L23 estructura 'dotnet sln list; echo; find src tests -maxdepth 2 -type d | sort; echo; cat MyProject.slnx'
ok L23 "Contracts en src/Contracts" 'src/Contracts/MyProject.Contracts'
ok L23 "cliente en src/Clients" 'src/Clients/MyProject.ConsoleClient'

# ---------------------------------------------------------------- L24 evaluar una dependencia
mkdir -p "$W/EvaluarDependencia"; cd "$W/EvaluarDependencia"
dotnet new console -n EvaluarDependencia -o . >/dev/null 2>&1
cap L24 evaluar-dependencia 'dotnet add package AutoMapper --version 14.0.0 | tail -3; echo; dotnet list package --vulnerable; echo; for v in automapper/14.0.0 automapper/15.0.0 mediatr/12.5.0 mediatr/13.0.0; do echo "== $v"; curl -s https://api.nuget.org/v3-flatcontainer/$v/$(basename $(dirname $v)).nuspec | grep -o "<license[^>]*>[^<]*</license>"; done'
ok L24 "vulnerabilidad informada" '(High|Alta)'
lic() { body "$LAST" | grep -A1 -F "== $1" | tail -1; }   # línea <license …> que sigue al encabezado
if lic automapper/14.0.0 | grep -q 'expression">MIT<';        then echo "PASS L24 AutoMapper 14.0.0 es MIT" >> "$LAB/aserciones.log"; else echo "FAIL L24 AutoMapper 14.0.0 no muestra MIT" >> "$LAB/aserciones.log"; fi
if lic automapper/15.0.0 | grep -q 'type="file"';              then echo "PASS L24 AutoMapper 15.0.0 declara licencia por archivo (no MIT)" >> "$LAB/aserciones.log"; else echo "FAIL L24 AutoMapper 15.0.0 no muestra licencia por archivo" >> "$LAB/aserciones.log"; fi
if lic mediatr/12.5.0    | grep -q 'expression">Apache-2.0<'; then echo "PASS L24 MediatR 12.5.0 es Apache-2.0" >> "$LAB/aserciones.log"; else echo "FAIL L24 MediatR 12.5.0 no muestra Apache-2.0" >> "$LAB/aserciones.log"; fi

# ---------------------------------------------------------------- cierre
cd "$SLN"
mkdir -p "$LAB/MyProject"
tar --exclude=bin --exclude=obj --exclude='*.db*' -cf - . | tar -xf - -C "$LAB/MyProject"
echo "---" >> "$LAB/aserciones.log"
echo "PASS: $(grep -c '^PASS' "$LAB/aserciones.log")  FAIL: $(grep -c '^FAIL' "$LAB/aserciones.log")" >> "$LAB/aserciones.log"
cat "$LAB/aserciones.log"
