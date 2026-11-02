// Nuevo (REC-14, plan de recuperación B-05, fila 08): ciclo de vida de lo que arma las herramientas
// por sesión, en forma concurrente. Patrón traído del insumo (CicloDeVidaDelClienteTests.cs y
// ConcurrenciaTests.cs, ver Procedencia.md) y adaptado a la base: la persistencia de _Comun tal cual
// (SessionContext Scoped con su Guid provisorio, LocalidadRepository acotado a la sesión,
// SessionSeeder) y sólo cambia la vida con que se registra LocalidadService.
//
// Dos sesiones, A y B, en hilos distintos y sincronizadas con una barrera en cuatro fases. Cada
// herramienta resuelve LocalidadService desde AIFunctionArguments.Services, que es el proveedor del
// ámbito de su sesión (así lo hace el bucle de M.E.AI cuando se le pasan servicios).
//
// El ejemplo 08 sólo monta herramientas de LECTURA. Para que el cruce se vea, la prueba suma DOS
// herramientas de escritura que el ejemplo no registra (agregar_localidad_de_prueba y
// eliminar_localidad_de_prueba); viven sólo en este archivo y Garantias.SoloHerramientasDeLectura
// sigue comprobando que lo montado son las dos de lectura.
//
//   (a) Singleton + ValidateScopes = false → caso rojo: B ve y borra lo de A, sin error.
//   (b) Singleton + ValidateScopes = true  → caso rojo: no falla al construir el contenedor
//       (ValidateOnBuild queda en false, el valor por defecto); falla en CADA llamada a la herramienta.
//   (c) Scoped (un ámbito por sesión)      → aislado, con ValidateScopes en true y en false.
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistence;
using MovilidadUrbana.Asistente.Infrastructure.Sessions;
using NUnit.Framework;

namespace MovilidadUrbana.Asistente.Pruebas;

public class CicloDeVidaTests
{
    private const string Agregar = "agregar_localidad_de_prueba";
    private const string Eliminar = "eliminar_localidad_de_prueba";

    // Cuatro fases separadas por la barrera; null = esa sesión espera la fase sin pedir nada. B cuenta
    // recién después de que A escribió, así el resultado no depende de cómo se intercalen los hilos.
    private static readonly (string Herramienta, string Argumento)?[] PedidosDeA =
        [(Agregar, "Goya"), (HerramientasDeLectura.Contar, "Corrientes"), null, (HerramientasDeLectura.Contar, "Corrientes")];

    private static readonly (string Herramienta, string Argumento)?[] PedidosDeB =
        [null, (HerramientasDeLectura.Contar, "Corrientes"), (Eliminar, "Goya"), (HerramientasDeLectura.Contar, "Corrientes")];

    // (c) Scoped por sesión: aislado, valide o no los ámbitos.
    [TestCase(true)]
    [TestCase(false)]
    public async Task Scoped_PorSesion_BNoVeNiBorraLoDeA(bool validarScopes)
    {
        var (a, b) = await CorrerAsync(ServiceLifetime.Scoped, validarScopes);

        Assert.Multiple(() =>
        {
            Assert.That(Cruces(a, b), Is.Empty);
            Assert.That(a, Is.EqualTo(new[] { "agregada Goya", "cantidad 2", "-", "cantidad 2" }));
            Assert.That(b, Is.EqualTo(new[] { "-", "cantidad 1", "borradas 0", "cantidad 1" }));
        });
    }

    // (a) Caso rojo: el Singleton se resuelve con los servicios de la raíz, con el SessionContext de la
    // raíz y su Guid provisorio, que ninguna de las dos sesiones fijó: las dos conversaciones comparten
    // un único espacio de datos. La verificación de cruce de (c) lo detecta.
    [Test]
    public async Task Singleton_ValidateScopesFalse_CasoRojo_BVeYBorraLoDeA_SinError()
    {
        var (a, b) = await CorrerAsync(ServiceLifetime.Singleton, validarScopes: false);

        Assert.Multiple(() =>
        {
            Assert.That(Cruces(a, b), Is.Not.Empty, "la verificación de cruce detecta el defecto");
            Assert.That(b[1], Is.EqualTo("cantidad 2"), "B vio la localidad de A");
            Assert.That(b[2], Is.EqualTo("borradas 1"), "B borró la localidad de A");
            Assert.That(a[3], Is.EqualTo("cantidad 1"), "A perdió lo suyo");
            Assert.That(a.Concat(b), Has.None.StartsWith("EXC"), "sin error");
        });
    }

    // (b) Caso rojo: con ValidateScopes = true el contenedor se construye sin error y la falla aparece
    // en cada una de las seis llamadas a una herramienta, no al construir.
    [Test]
    public async Task Singleton_ValidateScopesTrue_CasoRojo_FallaEnCadaLlamadaALaHerramienta()
    {
        var (a, b) = await CorrerAsync(ServiceLifetime.Singleton, validarScopes: true);

        var llamadas = a.Concat(b).Where(x => x != "-").ToList();
        Assert.That(llamadas, Has.Count.EqualTo(6));
        Assert.That(llamadas, Has.All.EqualTo("EXC InvalidOperationException"));
    }

    private static List<string> Cruces(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        var cruces = new List<string>();
        if (b[1] != "cantidad 1") cruces.Add($"B vio «{b[1]}» (lo suyo es 1)");
        if (b[2] != "borradas 0") cruces.Add($"B borró algo de A: «{b[2]}»");
        if (a[3] != "cantidad 2") cruces.Add($"A perdió lo suyo: «{a[3]}»");
        return cruces;
    }

    private static async Task<(List<string> A, List<string> B)> CorrerAsync(ServiceLifetime vida, bool validarScopes)
    {
        var archivo = Path.Combine(Path.GetTempPath(), $"ej08-vida-{Guid.NewGuid():n}.db");
        var coleccion = new ServiceCollection().AddInfrastructure($"Data Source={archivo};Default Timeout=30");
        coleccion.Add(new ServiceDescriptor(typeof(LocalidadService), typeof(LocalidadService), vida));
        var raiz = coleccion.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = validarScopes });
        try
        {
            DatabaseInitializer.Initialize(raiz);
            var herramientas = Herramientas().ToDictionary(h => h.Name);
            using var barrera = new Barrier(2);

            List<string> Sesion(string id, (string Herramienta, string Argumento)?[] pedidos)
            {
                using var ambito = raiz.CreateScope();
                ambito.ServiceProvider.GetRequiredService<SessionContext>().Set(id);
                var vistos = new List<string>();
                foreach (var pedido in pedidos)
                {
                    barrera.SignalAndWait(TimeSpan.FromSeconds(10));
                    if (pedido is not { } p) { vistos.Add("-"); continue; }
                    var clave = p.Herramienta == HerramientasDeLectura.Contar ? "provincia" : "nombre";
                    try
                    {
                        var r = herramientas[p.Herramienta]
                            .InvokeAsync(new AIFunctionArguments { [clave] = p.Argumento, Services = ambito.ServiceProvider })
                            .AsTask().GetAwaiter().GetResult();
                        vistos.Add(Resumir(r));
                    }
                    catch (Exception e)
                    {
                        vistos.Add($"EXC {e.GetType().Name}");
                    }
                }
                barrera.SignalAndWait(TimeSpan.FromSeconds(10));
                return vistos;
            }

            var a = Task.Run(() => Sesion("ejemplo-08-vida-a", PedidosDeA));
            var b = Task.Run(() => Sesion("ejemplo-08-vida-b", PedidosDeB));
            var res = await Task.WhenAll(a, b);
            return (res[0], res[1]);
        }
        finally
        {
            await raiz.DisposeAsync();
            SqliteConnection.ClearAllPools();
            foreach (var sufijo in new[] { "", "-wal", "-shm" })
            {
                try { File.Delete(archivo + sufijo); } catch (IOException) { }
            }
        }
    }

    private static string Resumir(object? r)
    {
        var texto = r is System.Text.Json.JsonElement e
            ? (e.ValueKind == System.Text.Json.JsonValueKind.String ? e.GetString()! : e.GetRawText())
            : r?.ToString() ?? "null";
        return texto.StartsWith("cantidad ", StringComparison.Ordinal) || texto.StartsWith("agregada ", StringComparison.Ordinal) ||
               texto.StartsWith("rechazada ", StringComparison.Ordinal) || texto.StartsWith("borradas ", StringComparison.Ordinal)
            ? texto
            : $"otro {texto}";
    }

    // La de lectura es la de producción (HerramientasDeLectura.ContarLocalidadesAsync), armada con el
    // LocalidadService que resuelve el proveedor de la llamada; las dos de escritura son sólo de prueba.
    private static IEnumerable<AIFunction> Herramientas() =>
    [
        AIFunctionFactory.Create(async (string provincia, AIFunctionArguments argumentos, CancellationToken ct) =>
        {
            var conteo = await new HerramientasDeLectura(argumentos.Services!.GetRequiredService<LocalidadService>())
                .ContarLocalidadesAsync(provincia, ct);
            return conteo.Cantidad is int n ? $"cantidad {n}" : $"error {conteo.Error}";
        }, HerramientasDeLectura.Contar),
        AIFunctionFactory.Create(async (string nombre, AIFunctionArguments argumentos, CancellationToken ct) =>
        {
            var r = await argumentos.Services!.GetRequiredService<LocalidadService>().SaveAsync(
                new LocalidadModel { Nombre = nombre, Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 }, ct);
            return r.IsSuccess ? $"agregada {nombre}" : $"rechazada {nombre}";
        }, Agregar),
        AIFunctionFactory.Create(async (string nombre, AIFunctionArguments argumentos, CancellationToken ct) =>
        {
            var servicio = argumentos.Services!.GetRequiredService<LocalidadService>();
            var objetivo = (await servicio.GetAllAsync(ct)).Where(l => l.Nombre == nombre).ToList();
            foreach (var l in objetivo) await servicio.DeleteAsync(l.Id, ct);
            return $"borradas {objetivo.Count}";
        }, Eliminar),
    ];
}
