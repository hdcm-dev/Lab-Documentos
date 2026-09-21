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
