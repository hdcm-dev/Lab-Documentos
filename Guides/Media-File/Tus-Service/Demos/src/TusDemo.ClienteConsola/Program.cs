using System.Net.Http.Json;
using System.Security.Cryptography;
using TusDemo.ClienteConsola;

// Uso: TusDemo.ClienteConsola <url-base> <clave> <archivo> [trozos-antes-del-corte]
// Sube el archivo con tus, corta la conexión después de N trozos, retoma desde el offset del servidor,
// registra la fila y descarga el archivo para comparar su huella SHA-256 con la del original.
if (args.Length < 3)
{
    Console.Error.WriteLine("Uso: TusDemo.ClienteConsola <url-base> <clave> <archivo> [trozos-antes-del-corte]");
    return 2;
}

var baseUrl = new Uri(args[0].EndsWith('/') ? args[0] : args[0] + "/");
var clave = args[1];
var ruta = args[2];
var trozosAntesDelCorte = args.Length > 3 ? int.Parse(args[3]) : 2;
const int trozo = 1024 * 1024;

using var http = new HttpClient { BaseAddress = baseUrl, Timeout = TimeSpan.FromMinutes(10) };
http.DefaultRequestHeaders.Add("X-Clave-Demo", clave);
var cliente = new ClienteTus(http);

await using var origen = File.OpenRead(ruta);
var largo = origen.Length;
var suba = await cliente.CrearAsync(new Uri(baseUrl, "archivos"), largo, Path.GetFileName(ruta), "text/plain", default);
var idSuba = suba.Segments[^1];
Console.WriteLine($"creada: {suba} ({largo} bytes)");

// Primer intento: se "corta la red" después de N trozos.
using var corte = new CancellationTokenSource();
var enviados = 0;
try
{
    await cliente.EnviarAsync(suba, origen, largo, trozo, corte.Token, _ =>
    {
        if (++enviados >= trozosAntesDelCorte) corte.Cancel();
    });
}
catch (OperationCanceledException)
{
    Console.WriteLine($"corte simulado después de {enviados} trozos");
}

var avance = await cliente.AvanceAsync(suba, default);
Console.WriteLine($"HEAD después del corte: Upload-Offset={avance} de {largo}");

// Segundo intento: retoma desde lo que el servidor confirma.
await cliente.EnviarAsync(suba, origen, largo, trozo, default);
Console.WriteLine($"HEAD al terminar: Upload-Offset={await cliente.AvanceAsync(suba, default)}");

// La fila se registra recién ahora. Dos veces, para mostrar que no se duplica.
var r1 = await http.PostAsJsonAsync("api/archivos", new { idSuba, descripcion = "desde la consola" });
var r2 = await http.PostAsJsonAsync("api/archivos", new { idSuba, descripcion = "desde la consola" });
Console.WriteLine($"registro: {(int)r1.StatusCode} y repetido: {(int)r2.StatusCode}");
Console.WriteLine($"fila: {await r1.Content.ReadAsStringAsync()}");

// Descarga y comparación de huellas.
var bajado = await http.GetByteArrayAsync($"descargas/{idSuba}");
origen.Seek(0, SeekOrigin.Begin);
var h1 = Convert.ToHexString(await SHA256.HashDataAsync(origen));
var h2 = Convert.ToHexString(SHA256.HashData(bajado));
Console.WriteLine($"sha256 original={h1}");
Console.WriteLine($"sha256 bajado  ={h2}");
Console.WriteLine(h1 == h2 ? "RESULTADO: OK, mismo contenido" : "RESULTADO: FALLA, contenido distinto");
return h1 == h2 && avance > 0 && avance < largo ? 0 : 1;
