using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyProject.Domain.Common;
using MyProject.Domain.Productos;
using MyProject.Infrastructure.Persistence;

// Cuatro escenarios, uno por captura. El argumento elige cuál corre.
switch (args is [var escenario, ..] ? escenario : "ciclo")
{
    case "ciclo": await Ciclo(); break;
    case "rechazos": Rechazos(); break;
    case "sin-ctor": SinCtor(); break;
    case "valores": Valores(); break;
    default: Console.WriteLine("Escenarios: ciclo | rechazos | sin-ctor | valores"); break;
}

// ---------------------------------------------------------------------------------------------------
// V05 — El ciclo completo con EF Core: alta por Factory Function, guardar, y releer en OTRO DbContext.
// ---------------------------------------------------------------------------------------------------
async Task Ciclo()
{
    await using var conexion = new SqliteConnection("Data Source=:memory:");
    await conexion.OpenAsync();
    var opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conexion).Options;

    Guid id;
    await using (var db = new AppDbContext(opciones))
    {
        await db.Database.EnsureCreatedAsync();

        var tipo = db.Model.FindEntityType(typeof(ProductoQueDevuelve))!;
        Console.WriteLine($"1. Constructor que EF Core eligió para materializar: {Firma(tipo)}");
        Console.WriteLine($"2. Propiedades mapeadas: {string.Join(", ", tipo.GetProperties().Select(p => p.Name))}");
        Console.WriteLine($"3. Precio: campo de respaldo = {tipo.FindProperty(nameof(ProductoQueDevuelve.Precio))!.FieldInfo?.Name}");

        // Alta por el único camino que existe desde afuera de la clase.
        var alta = ProductoQueDevuelve.Create("Mate", 3500m, "ARS");
        if (!alta.TryGetValue(out var producto, out var codigo))
        {
            Console.WriteLine($"   alta rechazada: {codigo}");
            return;
        }

        db.Add(producto);
        await db.SaveChangesAsync();
        id = producto.Id;
        Console.WriteLine($"4. Alta guardada: {producto.Nombre} {producto.Precio} {producto.Moneda} {producto.Estado}");
    }

    Console.WriteLine($"5. Veces que corrió el constructor privado hasta acá: {ProductoQueDevuelve.VecesQueCorrioElConstructorPrivado} (la del alta)");

    await using (var db = new AppDbContext(opciones))          // DbContext NUEVO: nada en caché
    {
        var releido = await db.Productos.AsNoTracking().SingleAsync(p => p.Id == id);
        Console.WriteLine($"6. Releído desde la base: {releido.Nombre} {releido.Precio:G29} {releido.Moneda} {releido.Estado}");
        Console.WriteLine($"7. Veces que corrió el constructor privado: {ProductoQueDevuelve.VecesQueCorrioElConstructorPrivado} (la del alta + la de la relectura)");
        Console.WriteLine($"8. Los setters siguen siendo privados y EF Core los escribió igual: Estado = {releido.Estado}");
    }
}

// ---------------------------------------------------------------------------------------------------
// V06 — Las dos estrategias de rechazo sobre el MISMO producto, y lo que cuesta cada una.
// ---------------------------------------------------------------------------------------------------
void Rechazos()
{
    Console.WriteLine("A. Las dos reglas del producto, rechazadas de las dos maneras");
    foreach (var (nombre, precio) in new (string, decimal)[] { ("", 3500m), ("Mate", 0m), ("Mate", 3500m) })
    {
        var caso = $"   Create(\"{nombre}\", {precio})";
        try
        {
            ProductoQueLanza.Create(nombre, precio);
            Console.WriteLine($"{caso} por excepción: aplicado, sin novedad");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"{caso} por excepción: DomainException: {ex.Message}");
        }

        var resultado = ProductoQueDevuelve.Create(nombre, precio, "ARS");
        Console.WriteLine($"{caso} por resultado: Aplicado = {resultado.Aplicado}, Codigo = {resultado.Codigo ?? "(ninguno)"}");
    }

    // La tercera regla la aporta el Value Object y sólo existe en la estrategia B (ver el escenario `valores`).
    var conMonedaInventada = ProductoQueDevuelve.Create("Mate", 3500m, "XYZ");
    Console.WriteLine($"   Create(\"Mate\", 3500, \"XYZ\") por resultado: Aplicado = {conMonedaInventada.Aplicado}, Codigo = {conMonedaInventada.Codigo}");

    Console.WriteLine();
    Console.WriteLine("B. Costo de un millón de rechazos (Release, mismo escenario, misma máquina)");
    const int n = 1_000_000;
    var porExcepcion = Medir(() => { try { ProductoQueLanza.Create("", 3500m); } catch (DomainException) { } }, n);
    var porResultado = Medir(() => { ProductoQueDevuelve.Create("", 3500m, "ARS"); }, n);
    Console.WriteLine($"   por excepción: {porExcepcion.ms,10:N1} ms   {porExcepcion.bytes / (double)n,7:N1} bytes por rechazo");
    Console.WriteLine($"   por resultado: {porResultado.ms,10:N1} ms   {porResultado.bytes / (double)n,7:N1} bytes por rechazo");
    Console.WriteLine($"   factor: {porExcepcion.ms / porResultado.ms:N0}x a favor del resultado");

    var exitoExcepcion = Medir(() => ProductoQueLanza.Create("Mate", 3500m), n);
    var exitoResultado = Medir(() => ProductoQueDevuelve.Create("Mate", 3500m, "ARS"), n);
    Console.WriteLine($"   un millón de altas VÁLIDAS: {exitoExcepcion.ms,10:N1} ms contra {exitoResultado.ms,10:N1} ms");
    Console.WriteLine("   (el mismo orden de magnitud: elegir resultado no acelera nada cuando no hay rechazos, y el camino B además construye el Value Object)");

    Console.WriteLine();
    Console.WriteLine("C. El resultado se puede ignorar; la excepción no");
    ProductoQueDevuelve.Create("", 3500m, "ARS");   // <- el rechazo se descarta y nadie avisa (ver V04)
    Console.WriteLine("   la línea de arriba descarta un rechazo y el programa siguió; la compilación de V04 no emitió un solo aviso.");
    Console.WriteLine("   la línea de abajo hace el mismo rechazo por excepción, sin try/catch:");
    ProductoQueLanza.Create("", 3500m);
    Console.WriteLine("   ESTA LÍNEA NO SE IMPRIME.");
}

// ---------------------------------------------------------------------------------------------------
// V07 — Lo que pasa sin el constructor privado sin parámetros, y con una propiedad sin setter.
// ---------------------------------------------------------------------------------------------------
void SinCtor()
{
    Console.WriteLine("A. Entity sin constructor sin parámetros y con nombres que no coinciden");
    try
    {
        using var db = Contexto<ProductoSinCtorPrivado>(new SqliteConnection("Data Source=:memory:"));
        db.Database.EnsureCreated();
        Console.WriteLine("   el modelo se construyó (no debería llegar acá)");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"   InvalidOperationException: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("B. La misma Entity con `{ get; }` en lugar de setter privado: compila, no avisa y la propiedad se va");
    using (var conexion = new SqliteConnection("Data Source=:memory:"))
    {
        conexion.Open();
        using var db = Contexto<ProductoConPropiedadSinSetter>(conexion);
        db.Database.EnsureCreated();
        var tipo = db.Model.FindEntityType(typeof(ProductoConPropiedadSinSetter))!;
        Console.WriteLine($"   propiedades mapeadas: {string.Join(", ", tipo.GetProperties().Select(p => p.Name))}");
        Console.WriteLine($"   `Nombre` está en el modelo: {tipo.FindProperty("Nombre") is not null}");
        Console.WriteLine("   tabla creada:");
        Console.WriteLine("   " + db.Database.GenerateCreateScript().Replace("\n", "\n   ").TrimEnd());
    }
}

// ---------------------------------------------------------------------------------------------------
// V08 — Conjunto cerrado: enum por convención, enum como texto, y el Value Object con regla.
// ---------------------------------------------------------------------------------------------------
void Valores()
{
    Console.WriteLine("A. El enum por convención: columna entera");
    using (var conexion = new SqliteConnection("Data Source=:memory:"))
    {
        conexion.Open();
        using var db = Contexto<ProductoQueLanza>(conexion);
        db.Database.EnsureCreated();
        Console.WriteLine($"   tipo de columna de Estado: {db.Model.FindEntityType(typeof(ProductoQueLanza))!.FindProperty("Estado")!.GetColumnType()}");
        db.Add(ProductoQueLanza.Create("Mate", 3500m));
        db.SaveChanges();
        Console.WriteLine($"   valor guardado en la base: '{Escalar(conexion, "SELECT Estado FROM Filas")}'");

        // Fila escrita por una versión futura del catálogo: el 9 no existe en este binario.
        db.Database.ExecuteSqlRaw("INSERT INTO Filas (Id, Nombre, Precio, Estado) VALUES ('11111111-1111-1111-1111-111111111111', 'Fantasma', '1.0', 9)");
        db.ChangeTracker.Clear();
        var fantasma = db.Filas.AsNoTracking().Single(p => p.Nombre == "Fantasma");
        Console.WriteLine($"   fila con Estado = 9 releída: Estado = {fantasma.Estado}, IsDefined = {Enum.IsDefined(fantasma.Estado)}, y NO lanzó");
    }

    Console.WriteLine();
    Console.WriteLine("B. El mismo enum con HasConversion<string>: columna de texto");
    using (var conexion = new SqliteConnection("Data Source=:memory:"))
    {
        conexion.Open();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conexion).Options);
        db.Database.EnsureCreated();
        Console.WriteLine($"   tipo de columna de Estado: {db.Model.FindEntityType(typeof(ProductoQueDevuelve))!.FindProperty("Estado")!.GetColumnType()}");
        db.Add(ProductoQueDevuelve.Create("Mate", 3500m, "ARS").Value!);
        db.SaveChanges();
        Console.WriteLine($"   valor guardado en la base: '{Escalar(conexion, "SELECT Estado FROM Productos")}'");

        db.Database.ExecuteSqlRaw("INSERT INTO Productos (Id, Nombre, Precio, Estado, Moneda) VALUES ('22222222-2222-2222-2222-222222222222', 'Fantasma', '1.0', 'Archivado', 'ARS')");
        db.ChangeTracker.Clear();
        try
        {
            var fantasma = db.Productos.AsNoTracking().Single(p => p.Nombre == "Fantasma");
            Console.WriteLine($"   fila con Estado = 'Archivado' releída: {fantasma.Estado}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   fila con Estado = 'Archivado': InvalidOperationException: {ex.Message}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("C. El enum, fuera de la base, tampoco está cerrado");
    var inventado = (EstadoProducto)99;
    Console.WriteLine($"   (EstadoProducto)99 compila y no lanza: ToString() = '{inventado}', IsDefined = {Enum.IsDefined(inventado)}");
    Console.WriteLine($"   JSON por defecto: {JsonSerializer.Serialize(new { Estado = EstadoProducto.Publicado })} (número, no nombre)");
    var conNombres = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
    Console.WriteLine($"   JSON con JsonStringEnumConverter: {JsonSerializer.Serialize(new { Estado = EstadoProducto.Publicado }, conNombres)}");
    Console.WriteLine($"   y deserializa un 99 sin quejarse: {JsonSerializer.Deserialize<EstadoProducto>("99")}");
    Console.WriteLine($"   por eso el switch necesita un brazo defensivo: {Describir(EstadoProducto.Publicado)} / {Describir(inventado)}");

    Console.WriteLine();
    Console.WriteLine("D. El Value Object con regla es el único que no admite un valor imposible");
    foreach (var codigo in new[] { "ars", "XYZ", "" })
    {
        var moneda = Moneda.Create(codigo);
        Console.WriteLine($"   Moneda.Create(\"{codigo}\") → Aplicado = {moneda.Aplicado}, {(moneda.Aplicado ? moneda.Value!.Codigo : moneda.Codigo)}");
    }
    var sinRegla = new MonedaSinRegla("XYZ");
    Console.WriteLine($"   new MonedaSinRegla(\"XYZ\") se construye sin quejas: {sinRegla}");
    Console.WriteLine($"   y `with` la cambia sin pasar por ninguna regla: {sinRegla with { Codigo = "@@@" }}");
    Console.WriteLine($"   constructores públicos: Moneda = {typeof(Moneda).GetConstructors().Length}, MonedaSinRegla = {typeof(MonedaSinRegla).GetConstructors().Length}");
    Console.WriteLine($"   igualdad por datos en las dos: {Moneda.Create("ARS").Value! == Moneda.Create("ars").Value!} / {new MonedaSinRegla("XYZ") == new MonedaSinRegla("XYZ")}");
}

// --------------------------------------------------- auxiliares de la demostración -----------------

static string Describir(EstadoProducto estado) => estado switch
{
    EstadoProducto.Borrador => "borrador",
    EstadoProducto.Publicado => "publicado",
    EstadoProducto.Retirado => "retirado",
    _ => $"valor fuera del catálogo ({(int)estado})"
};

static ContextoDeUnaEntidad<TEntity> Contexto<TEntity>(SqliteConnection conexion)
    where TEntity : class =>
    new(new DbContextOptionsBuilder<ContextoDeUnaEntidad<TEntity>>().UseSqlite(conexion).Options);

static object? Escalar(SqliteConnection conexion, string sql)
{
    using var comando = conexion.CreateCommand();
    comando.CommandText = sql;
    return comando.ExecuteScalar();
}

static string Firma(IEntityType tipo)
{
    var enlace = (ConstructorBinding?)tipo.ConstructorBinding;
    var ctor = enlace?.GetType().GetProperty("Constructor")?.GetValue(enlace) as ConstructorInfo;
    if (ctor is null)
        return "(ninguno)";
    var visibilidad = ctor.IsPrivate ? "private" : ctor.IsPublic ? "public" : "protected/internal";
    var parametros = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    return $"{visibilidad} {tipo.ClrType.Name}({parametros})";
}

static (double ms, long bytes) Medir(Action cuerpo, int n)
{
    for (var i = 0; i < 1_000; i++) cuerpo();                 // calentamiento: que el JIT ya haya compilado
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    var antes = GC.GetAllocatedBytesForCurrentThread();
    var reloj = Stopwatch.StartNew();
    for (var i = 0; i < n; i++) cuerpo();
    reloj.Stop();
    return (reloj.Elapsed.TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - antes);
}
