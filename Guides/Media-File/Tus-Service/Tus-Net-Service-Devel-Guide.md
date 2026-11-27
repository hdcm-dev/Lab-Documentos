# Guía de desarrollo: un servicio de subas reanudables con tus en .NET 10

> **Para quién es.** Para quien programa en C# y nunca armó un servicio de archivos. No hace falta saber nada de
> tus: la guía lo explica desde cero, con definiciones, y cada pieza de código que muestra sale de la demo que
> acompaña a esta guía (carpeta [`Demos/`](Demos/)), que compila y se probó.
>
> **Qué vas a poder hacer al terminar.** Explicar qué es una suba reanudable y cómo funciona el protocolo tus;
> armar un servicio ASP.NET Core que recibe archivos con tus, los guarda y los sirve con autenticación; escribir un
> cliente de navegador y uno de .NET que retoman una suba cortada; publicar el servicio en un contenedor; y saber
> qué falta para llevarlo a producción.

## Índice

- [1. Conceptos](#1-conceptos)
  - [1.1 El problema: subir archivos grandes por una red que se corta](#11-el-problema-subir-archivos-grandes-por-una-red-que-se-corta)
  - [1.2 Qué es tus](#12-qué-es-tus)
  - [1.3 Glosario](#13-glosario)
  - [1.4 El protocolo, pedido por pedido](#14-el-protocolo-pedido-por-pedido)
  - [1.5 Lo que tus no hace](#15-lo-que-tus-no-hace)
- [2. La demo](#2-la-demo)
  - [2.1 Qué hace](#21-qué-hace)
  - [2.2 Cómo está organizada](#22-cómo-está-organizada)
  - [2.3 Cómo correrla](#23-cómo-correrla)
- [3. El servidor, pieza por pieza](#3-el-servidor-pieza-por-pieza)
  - [3.1 El arranque: `Program.cs`](#31-el-arranque-programcs)
  - [3.2 La configuración](#32-la-configuración)
  - [3.3 El almacén: `TusDiskStore`](#33-el-almacén-tusdiskstore)
  - [3.4 El endpoint tus y sus eventos](#34-el-endpoint-tus-y-sus-eventos)
  - [3.5 La autenticación de la demo](#35-la-autenticación-de-la-demo)
  - [3.6 Consultar el estado de una suba](#36-consultar-el-estado-de-una-suba)
  - [3.7 La fila de archivo y el orden de las operaciones](#37-la-fila-de-archivo-y-el-orden-de-las-operaciones)
  - [3.8 La API con controllers y Scalar](#38-la-api-con-controllers-y-scalar)
- [4. Los clientes](#4-los-clientes)
  - [4.1 El cliente de navegador](#41-el-cliente-de-navegador)
  - [4.2 La página Blazor Interactive Server](#42-la-página-blazor-interactive-server)
  - [4.3 El cliente .NET](#43-el-cliente-net)
- [5. El contenedor](#5-el-contenedor)
  - [5.1 El `Dockerfile`](#51-el-dockerfile)
  - [5.2 `compose.yaml` y el volumen](#52-composeyaml-y-el-volumen)
- [6. Cómo se verificó la demo](#6-cómo-se-verificó-la-demo)
- [7. Errores comunes](#7-errores-comunes)
- [8. Del ejemplo a producción](#8-del-ejemplo-a-producción)
- [9. Referencias](#9-referencias)

---

## 1. Conceptos

### 1.1 El problema: subir archivos grandes por una red que se corta

Imaginá un inspector que recorre una ruta y saca fotos y videos con el teléfono. La señal va y viene. Si la
aplicación sube cada archivo con un único pedido HTTP —la forma habitual de un formulario con un `<input
type="file">`—, un corte a mitad de camino **pierde todo lo enviado**: el pedido falla y hay que empezar de nuevo
desde el primer byte. Con un video de 200 MB y una señal que se corta cada pocos minutos, el archivo nunca llega.

Lo que hace falta es una **suba reanudable**: el archivo viaja por partes, el servidor recuerda cuánto recibió, y
después de un corte el cliente pregunta «¿hasta dónde llegó?» y sigue desde ahí.

### 1.2 Qué es tus

**tus** es un protocolo abierto para subas reanudables sobre HTTP. No es una biblioteca ni un producto: es una
especificación que dice qué pedidos manda el cliente, con qué cabeceras, y qué responde el servidor. Su versión
estable es la **1.0.0**. Como es un estándar, cualquier cliente que lo hable (una biblioteca de JavaScript, una de
.NET, una aplicación móvil) puede subir a cualquier servidor que lo hable.

En .NET el servidor se arma con la biblioteca **tusdotnet**, que agrega a ASP.NET Core un endpoint que entiende el
protocolo. La demo usa la versión **2.12.0**, fijada en el proyecto.

### 1.3 Glosario

| Término | Qué significa en esta guía |
| --- | --- |
| **Suba** | Un archivo que se está subiendo o que ya se subió con tus. Tiene un identificador (id) y una URL propia |
| **Offset** | Cuántos bytes de la suba recibió el servidor hasta ahora. Una suba está **completa** cuando el offset alcanza el largo total |
| **Trozo** | Una parte del archivo que viaja en un solo pedido. La demo usa trozos de 5 MB en el navegador y de 1 MB en la consola |
| **Metadatos** | Datos que acompañan a la suba sin ser el archivo: en la demo, el nombre original (`filename`) y el tipo (`filetype`) |
| **Base64** | Una forma de escribir bytes (o texto con acentos) usando sólo letras, dígitos, `+`, `/` y `=`. tus la usa para los valores de los metadatos |
| **Almacén** | El lugar donde el servidor guarda las subas y su estado. En la demo, una carpeta del disco (`TusDiskStore`) |
| **Endpoint** | Una dirección del servidor que atiende pedidos, por ejemplo `/archivos` |
| **Fila de archivo** | El registro que el producto guarda de un archivo ya subido: su nombre, su tipo, su tamaño y **la ruta relativa** para leerlo (`descargas/<id>`), nunca el nombre del servidor |
| **Idempotente** | Una operación que se puede repetir sin cambiar el resultado. Registrar dos veces la misma suba deja una sola fila |
| **Autenticación** | Saber quién hace el pedido. En la demo, una clave compartida en la cabecera `X-Clave-Demo` |
| **CORS** | La regla del navegador que impide que una página de un origen (dominio y puerto) llame a otro, salvo que el servidor lo permita |
| **Preflight** | El pedido `OPTIONS` que el navegador manda **antes** de un pedido entre orígenes, sin credenciales, para preguntar si está permitido |
| **Controller** | Una clase de ASP.NET Core que agrupa los endpoints de una API. Cada método público con un atributo como `[HttpGet]` es un endpoint |
| **OpenAPI** | Un documento en JSON que describe una API: rutas, parámetros y respuestas. ASP.NET Core lo genera solo |
| **Scalar** | Una página web que lee el documento OpenAPI y deja probar la API desde el navegador |
| **Blazor Interactive Server** | Un modo de Blazor en el que los componentes corren en el servidor; el navegador recibe sólo los cambios de la pantalla |
| **Circuito** | La conexión permanente entre el navegador y el servidor en Blazor Interactive Server. Viaja sobre **SignalR**, la biblioteca de .NET para mensajes en tiempo real |
| **Interop de JavaScript** | El mecanismo con el que un componente Blazor llama a una función de JavaScript, y JavaScript llama de vuelta a un método de .NET |
| **Contenedor** | Un proceso aislado que corre con su propio sistema de archivos, armado desde una **imagen** |
| **Volumen** | Un espacio de disco que Docker mantiene fuera del contenedor: sobrevive a borrar el contenedor |

### 1.4 El protocolo, pedido por pedido

Una suba con tus usa cuatro pedidos HTTP. Todos llevan la cabecera `Tus-Resumable: 1.0.0`, salvo `OPTIONS`.

| Pedido | Para qué | Cabeceras que manda el cliente | Respuesta correcta |
| --- | --- | --- | --- |
| `OPTIONS /archivos` | Preguntar qué versión y qué extensiones soporta el servidor | — | `204` con `Tus-Version`, `Tus-Extension` y `Tus-Max-Size` |
| `POST /archivos` | **Crear** una suba vacía | `Upload-Length` (largo total en bytes) y `Upload-Metadata` | `201 Created` con `Location: /archivos/<id>` |
| `PATCH /archivos/<id>` | **Enviar** un trozo | `Upload-Offset` (desde qué byte) y `Content-Type: application/offset+octet-stream` | `204` con el nuevo `Upload-Offset` |
| `HEAD /archivos/<id>` | **Preguntar** cuánto llegó | — | `200` con `Upload-Offset` |

Las respuestas de error que importan:

- `409 Conflict` en un `PATCH`: el `Upload-Offset` que mandó el cliente no coincide con lo que tiene el servidor.
  El cliente pregunta con `HEAD` y sigue desde ahí.
- `413` en un `POST`: el archivo pasa el tamaño máximo del servidor.
- `404` en un `HEAD` o `PATCH`: la suba no existe (por ejemplo, porque venció).

La cabecera `Upload-Metadata` es una lista de pares separados por coma. En cada par van la clave, un espacio y el
valor en Base64:

```text
Upload-Metadata: filename Zm90by5qcGc=,filetype aW1hZ2UvanBlZw==
```

(`Zm90by5qcGc=` es `foto.jpg` y `aW1hZ2UvanBlZw==` es `image/jpeg`.)

El recorrido completo de una suba que se corta y se retoma:

```mermaid
sequenceDiagram
    participant C as Cliente
    participant S as Servicio tus
    participant A as Almacén (disco)
    C->>S: POST /archivos (Upload-Length 10 MB, metadatos)
    S->>A: crea la suba vacía
    S-->>C: 201 Location /archivos/abc
    C->>S: PATCH /archivos/abc (Upload-Offset 0, 5 MB)
    S->>A: agrega 5 MB
    S-->>C: 204 Upload-Offset 5 MB
    Note over C,S: se corta la red
    C->>S: HEAD /archivos/abc
    S-->>C: 200 Upload-Offset 5 MB
    C->>S: PATCH /archivos/abc (Upload-Offset 5 MB, 5 MB)
    S->>A: agrega 5 MB, la suba queda completa
    S-->>C: 204 Upload-Offset 10 MB
```

*Diagrama: una suba de 10 MB en dos trozos, con un corte en el medio.*

### 1.5 Lo que tus no hace

Dos cosas que vas a necesitar y que el protocolo **no** resuelve:

1. **La descarga.** tus define cómo subir, no cómo bajar. El servicio de la demo agrega su propio endpoint de
   lectura, `GET /descargas/<id>` (sección 3.8).
2. **Saber quién sube.** tus no dice nada de autenticación. La demo la agrega con un esquema mínimo (sección 3.5).

---

## 2. La demo

### 2.1 Qué hace

Es una sola aplicación ASP.NET Core .NET 10 que junta tres cosas en un proceso:

- el **endpoint tus** en `/archivos`, que recibe las subas;
- una **API con controllers** en `/api/archivos` (registrar y listar archivos) y `/descargas/<id>` (bajar uno),
  documentada con OpenAPI y navegable con Scalar en `/scalar`;
- una **página Blazor Interactive Server** en `/` para elegir un archivo, subirlo con barra de avance, pausarlo,
  retomarlo y ver la lista de archivos registrados, que se actualiza sola en todas las pestañas abiertas.

Además trae un **cliente de consola** que sube un archivo, simula un corte de red, retoma, registra la fila y
compara la huella del archivo bajado con la del original.

### 2.2 Cómo está organizada

```text
Demos/
├── TusDemo.slnx                        solución
├── Dockerfile                          imagen del servicio
├── compose.yaml                        servicio + volumen
├── scripts/probar.sh                   prueba automática (sección 6)
└── src/
    ├── TusDemo.Web/                    el servicio
    │   ├── Program.cs                  arranque y orden del pipeline
    │   ├── appsettings.json            configuración
    │   ├── Archivos/                   lógica de archivos
    │   │   ├── OpcionesArchivos.cs     la sección "Archivos" de la configuración
    │   │   ├── ConfiguracionTus.cs     el endpoint tus y sus eventos
    │   │   ├── EstadoDeSuba.cs         consulta al almacén: ¿existe?, ¿está completa?
    │   │   ├── ServicioDeArchivos.cs   registrar la fila después de la suba
    │   │   ├── RegistroDeArchivos.cs   la "tabla" de archivos (un JSON)
    │   │   ├── ArchivoRegistrado.cs    la fila y el pedido de registro
    │   │   └── AvisoDeArchivos.cs      aviso en el proceso cuando cambia la lista
    │   ├── Seguridad/ClaveDemoHandler.cs   autenticación por cabecera
    │   ├── Controllers/                ArchivosController y DescargasController
    │   ├── Components/                 App, Routes, Layout y Pages/Home.razor
    │   └── wwwroot/js/tus-cliente.js   cliente tus del navegador
    └── TusDemo.ClienteConsola/         cliente .NET (ClienteTus.cs y Program.cs)
```

### 2.3 Cómo correrla

**Con el SDK de .NET 10 instalado:**

```bash
cd Demos/src/TusDemo.Web
dotnet run
```

Abrí `http://localhost:5080`. La página trae la clave de la demo desde `appsettings.json`; para la API, usala en la
cabecera `X-Clave-Demo`. La interfaz de Scalar está en `http://localhost:5080/scalar`.

**Con Docker** (no hace falta el SDK):

```bash
cd Demos
docker compose up --build
```

El servicio queda en `http://localhost:8090`.

**El cliente de consola**, con el servicio andando:

```bash
cd Demos/src/TusDemo.ClienteConsola
dotnet run -- http://localhost:5080 clave-de-demo-cambiar <ruta-de-un-archivo.txt> 2
```

El último argumento es cuántos trozos manda antes de simular el corte.

**Versiones fijas.** Paquetes: `tusdotnet` 2.12.0, `Microsoft.AspNetCore.OpenApi` 10.0.15 y `Scalar.AspNetCore`
3.3.0. Imágenes: `mcr.microsoft.com/dotnet/sdk:10.0.404` para compilar y `mcr.microsoft.com/dotnet/aspnet:10.0.15`
para correr. Se fijan para que la demo compile igual mañana: una etiqueta como `10.0` cambia de contenido con cada
parche.

---

## 3. El servidor, pieza por pieza

### 3.1 El arranque: `Program.cs`

`Program.cs` hace dos cosas: **registra servicios** (qué objetos existen y cómo se crean) y **arma el pipeline**
(por qué pasos atraviesa cada pedido HTTP, en orden).

```csharp
// --- Configuración y servicios de archivos ---
builder.Services.Configure<OpcionesArchivos>(builder.Configuration.GetSection(OpcionesArchivos.Seccion));
builder.Services.AddSingleton(sp =>
{
    var raiz = sp.GetRequiredService<IOptions<OpcionesArchivos>>().Value.Raiz;
    Directory.CreateDirectory(raiz);
    return new TusDiskStore(raiz);          // una sola instancia para tus y para las descargas
});
builder.Services.AddSingleton<RegistroDeArchivos>();
builder.Services.AddSingleton<AvisoDeArchivos>();
builder.Services.AddSingleton<ServicioDeArchivos>();
```

- `Configure<OpcionesArchivos>` liga la sección `"Archivos"` de la configuración a la clase `OpcionesArchivos`
  (sección 3.2). Cualquier clase que pida `IOptions<OpcionesArchivos>` recibe esos valores.
- `AddSingleton` registra un objeto **único** para toda la aplicación. El almacén tiene que ser uno solo: el endpoint
  tus escribe en él y el controller de descargas lee de él. Dos instancias sobre la misma carpeta podrían pisarse.

```csharp
// --- Autenticación de la demo (cabecera X-Clave-Demo) ---
builder.Services.AddAuthentication(ClaveDemoHandler.Esquema)
    .AddScheme<AuthenticationSchemeOptions, ClaveDemoHandler>(ClaveDemoHandler.Esquema, null);
builder.Services.AddAuthorization();

// --- API con controllers y documento OpenAPI ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- Interfaz Blazor Interactive Server ---
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
```

- `AddAuthentication(ClaveDemoHandler.Esquema)` dice cuál es el esquema **por defecto**: con eso, cada pedido pasa
  por el `ClaveDemoHandler` y, si trae la clave, queda con un usuario autenticado (`HttpContext.User`).
- `AddControllers` habilita las clases controller; `AddOpenApi` genera el documento OpenAPI de esas rutas.
- `AddRazorComponents().AddInteractiveServerComponents()` habilita Blazor con el modo interactivo de servidor.

Después viene el pipeline. **El orden importa**: cada línea es un paso, y un pedido los atraviesa de arriba abajo.

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapOpenApi();                    // /openapi/v1.json
app.MapScalarApiReference();         // interfaz de Scalar sobre ese documento
app.MapControllers();                // /api/archivos y /descargas
app.MapTus("/archivos", ConfiguracionTus.CrearAsync);   // endpoint del protocolo tus
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
```

- `UseAuthentication` va **antes** de todo lo que necesita saber quién pide. `UseAntiforgery`, que protege los
  formularios de Blazor contra pedidos falsificados, va **después** de la autenticación y la autorización.
- Las líneas `Map…` declaran endpoints. `MapTus("/archivos", …)` es la extensión de tusdotnet: recibe la ruta y una
  función que arma la configuración de tus para cada pedido (sección 3.4).

### 3.2 La configuración

Los valores que cambian según dónde corre el servicio están en `appsettings.json`:

```json
"Archivos": {
  "Raiz": "datos/tus",
  "Registro": "datos/registro.json",
  "TopeBytes": 524288000,
  "Extensiones": [ ".jpg", ".jpeg", ".png", ".heic", ".mp4", ".mov", ".m4a", ".aac", ".pdf", ".txt" ],
  "HorasDeVencimiento": 72,
  "TrozoBytes": 5242880,
  "ClaveDemo": "clave-de-demo-cambiar"
}
```

| Valor | Para qué |
| --- | --- |
| `Raiz` | Carpeta del almacén de tus |
| `Registro` | Archivo JSON que hace de tabla de archivos |
| `TopeBytes` | Tamaño máximo de una suba (500 MB). Más grande recibe `413` |
| `Extensiones` | Extensiones aceptadas. Otra recibe `400` |
| `HorasDeVencimiento` | Horas sin actividad después de las cuales una suba incompleta vence |
| `TrozoBytes` | Tamaño de trozo que usa el cliente del navegador |
| `ClaveDemo` | Clave compartida de la demo (sección 3.5) |

Cada valor se puede reemplazar con una **variable de entorno**, escribiendo la ruta con doble guion bajo:
`Archivos__Raiz=/data/tus`. Así lo hace el contenedor (sección 5.1) sin tocar el archivo.

La clase que recibe esos valores es una clase común con propiedades y valores por omisión:

```csharp
public sealed class OpcionesArchivos
{
    public const string Seccion = "Archivos";

    /// <summary>Carpeta donde el almacén de tus guarda los archivos y su estado.</summary>
    public string Raiz { get; set; } = "datos/tus";
```

### 3.3 El almacén: `TusDiskStore`

`TusDiskStore` es el almacén en disco que trae tusdotnet. Se crea con la carpeta donde guardar:

```csharp
return new TusDiskStore(raiz);          // una sola instancia para tus y para las descargas
```

Por cada suba guarda el contenido y unos archivos chicos con su estado (largo declarado, metadatos, vencimiento).
El **nombre en disco es el id** que genera el propio almacén —por defecto, un GUID sin guiones, como
`5be63826212b4f10abf054965ff13de8`—, nunca el nombre que manda el usuario. Eso evita choques entre dos archivos que
se llaman igual y que alguien arme una ruta con `../`.

Además de guardar, el almacén responde preguntas: `FileExistAsync` (¿existe?), `GetUploadLengthAsync` (¿cuál es el
largo declarado?), `GetUploadOffsetAsync` (¿cuánto llegó?) y `GetFileAsync` (dame el archivo, con su contenido y
sus metadatos). La demo las usa en la sección 3.6.

### 3.4 El endpoint tus y sus eventos

`ConfiguracionTus.CrearAsync` arma la configuración del endpoint. tusdotnet la pide en cada pedido, así que puede
leer los servicios registrados en `Program.cs`:

```csharp
var configuracion = new DefaultTusConfiguration
{
    Store = almacen,
    MaxAllowedUploadSizeInBytesLong = opciones.TopeBytes,
    Expiration = new SlidingExpiration(TimeSpan.FromHours(opciones.HorasDeVencimiento)),
```

- `Store` es el almacén de la sección 3.3.
- `MaxAllowedUploadSizeInBytesLong` es el tope. El servidor lo anuncia en `Tus-Max-Size` y rechaza con `413` la
  suba más grande.
- `Expiration` con `SlidingExpiration` hace que una suba incompleta venza tras ese tiempo **sin actividad**: cada
  trozo que llega renueva el plazo. (La otra opción, `AbsoluteExpiration`, cuenta desde que se creó.)

Lo interesante son los **eventos**: funciones que tusdotnet llama en momentos fijos del pedido, para que el
producto decida.

**`OnAuthorizeAsync`: ¿quién pide?** Se llama primero, en todo pedido tus.

```csharp
OnAuthorizeAsync = ctx =>
{
    if (ctx.Intent == IntentType.GetOptions) return Task.CompletedTask;
    if (ctx.HttpContext.User.Identity?.IsAuthenticated != true)
        ctx.FailRequest(HttpStatusCode.Unauthorized);
    return Task.CompletedTask;
},
```

- `ctx.Intent` dice qué intenta el pedido: crear (`CreateFile`), escribir (`WriteFile`), consultar
  (`GetFileInfo`), borrar (`DeleteFile`), concatenar (`ConcatenateFiles`) o descubrir el servidor (`GetOptions`).
- **`OPTIONS` queda libre.** El navegador lo manda sin credenciales, para descubrir el servidor y como preflight de
  CORS. Si se le exige la clave, recibe `401` y el cliente no puede ni empezar. Esta línea salió de la prueba de la
  demo: la primera versión no la tenía y el `OPTIONS` respondía `401` (sección 7).
- `ctx.FailRequest(HttpStatusCode.Unauthorized)` corta el pedido con ese código.

**`OnBeforeCreateAsync`: ¿se puede crear esta suba?** Se llama antes de crear, con los metadatos ya leídos.

```csharp
OnBeforeCreateAsync = ctx =>
{
    if (!ctx.Metadata.ContainsKey("filename") || !ctx.Metadata.ContainsKey("filetype"))
    {
        ctx.FailRequest("Faltan los metadatos filename y filetype");
        return Task.CompletedTask;
    }
    var nombre = ctx.Metadata["filename"].GetString(Encoding.UTF8);
    var extension = Path.GetExtension(nombre);
    if (!extensiones.Contains(extension))
        ctx.FailRequest($"Extensión no permitida: '{extension}'");
    return Task.CompletedTask;
},
```

- `ctx.Metadata` es un diccionario con los pares de `Upload-Metadata`, ya decodificados de Base64 a bytes;
  `GetString(Encoding.UTF8)` los convierte a texto.
- `FailRequest` con un mensaje responde `400 Bad Request` con ese texto.

**`OnFileCompleteAsync`: la suba terminó.** Se llama **una sola vez**, cuando llega el último byte.

```csharp
OnFileCompleteAsync = async ctx =>
{
    var archivo = await ctx.GetFileAsync();
    log.LogInformation("Suba completa {Id}", archivo.Id);
    // Acá se encolaría un antivirus o la generación de miniaturas.
}
```

Mientras este evento corre, el cliente está esperando la respuesta de su último trozo. Por eso un proceso lento
(un antivirus, un video que se recodifica) no se hace acá adentro: se **encola** y se hace después.

### 3.5 La autenticación de la demo

Para no depender de un proveedor de identidad, la demo usa el esquema más simple posible: el pedido trae una
cabecera `X-Clave-Demo` con una clave compartida. **Sirve para aprender dónde se engancha la autenticación; no es un
esquema de producción** (sección 8).

Un esquema de autenticación en ASP.NET Core es una clase que hereda de `AuthenticationHandler` y responde una
pregunta: ¿este pedido es de alguien?

```csharp
protected override Task<AuthenticateResult> HandleAuthenticateAsync()
{
    if (!Request.Headers.TryGetValue(Cabecera, out var valor))
        return Task.FromResult(AuthenticateResult.NoResult());

    var esperada = Encoding.UTF8.GetBytes(archivos.Value.ClaveDemo);
    var recibida = Encoding.UTF8.GetBytes(valor.ToString());
    if (esperada.Length == 0 || !CryptographicOperations.FixedTimeEquals(esperada, recibida))
        return Task.FromResult(AuthenticateResult.Fail("Clave inválida"));

    var identidad = new ClaimsIdentity([new Claim(ClaimTypes.Name, "demo")], Esquema);
    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identidad), Esquema);
    return Task.FromResult(AuthenticateResult.Success(ticket));
}
```

- `NoResult()`: el pedido no trae credenciales. No es un error; simplemente no hay usuario.
- `Fail(...)`: trajo credenciales equivocadas.
- `Success(ticket)`: el pedido queda con un usuario llamado `demo`.
- `CryptographicOperations.FixedTimeEquals` compara las dos claves en un tiempo que no depende de dónde difieren.
  Una comparación común (`==`) termina antes cuanto antes difiere, y un atacante podría medir esos tiempos para
  adivinar la clave de a un carácter.

Con eso, el evento `OnAuthorizeAsync` de tus y el atributo `[Authorize]` de los controllers preguntan lo mismo:
`User.Identity.IsAuthenticated`.

### 3.6 Consultar el estado de una suba

Antes de registrar o de servir un archivo hay que saber si existe y si está completo. La demo lo junta en un método
de extensión sobre el almacén:

```csharp
var noExiste = new EstadoDeSuba(false, false, 0, "", "");
// El id llega del cliente: sólo se aceptan letras y dígitos (el almacén genera GUID sin guiones).
// Así un id como "../otra-carpeta" nunca llega a armar una ruta en disco.
if (string.IsNullOrWhiteSpace(id) || !id.All(char.IsAsciiLetterOrDigit))
    return noExiste;
if (!await almacen.FileExistAsync(id, ct))
    return noExiste;

long? largo = await almacen.GetUploadLengthAsync(id, ct);
long recibido = await almacen.GetUploadOffsetAsync(id, ct);
```

- **Validar el id primero.** El id viene en la URL o en el cuerpo del pedido, así que lo escribe el cliente. Si
  tuviera `../`, podría apuntar a otra carpeta. Aceptar sólo letras y dígitos cierra esa puerta antes de tocar el
  disco.
- **Completa** quiere decir `recibido == largo`. Una suba a medias existe, pero no se registra ni se sirve.

Un **método de extensión** (`this TusDiskStore almacen` en el primer parámetro) se escribe en otra clase pero se
llama como si fuera del objeto: `almacen.ConsultarAsync(id, ct)`.

### 3.7 La fila de archivo y el orden de las operaciones

El almacén guarda bytes. El producto necesita además **su propio registro** de cada archivo: a qué pertenece, quién
lo subió, con qué descripción. Ese registro es la **fila de archivo**:

```csharp
public sealed record ArchivoRegistrado(
    long Id,
    string Documento,
    string Nombre,
    string Tipo,
    long Bytes,
    string Descripcion,
    DateTimeOffset FechaAlta,
    string UsuarioAlta);
```

`Documento` guarda **la ruta relativa** para leer el archivo, `descargas/<id>`, y **nunca** la URL completa con el
nombre del servidor. Si mañana el servicio se muda de servidor, las filas siguen valiendo: el cliente arma la URL
juntando la dirección actual del servicio con `Documento`.

**El orden es la decisión central:** primero la suba completa, **después** la fila.

```csharp
var estado = await almacen.ConsultarAsync(idSuba, ct);
if (!estado.Existe) return (ResultadoDeRegistro.NoExiste, null);
if (!estado.Completa) return (ResultadoDeRegistro.Incompleto, null);

var (fila, creada) = registro.RegistrarSiFalta(
    documento: $"descargas/{idSuba}",
```

¿Por qué en ese orden? Si la fila se creara al empezar la suba y el cliente nunca terminara, quedaría una fila
apuntando a un archivo a medias: la pantalla mostraría un archivo que no se puede abrir. Al revés, si la suba
termina y el registro falla, queda un archivo sin fila; el cliente **reintenta el registro** y, como el registro es
**idempotente**, el reintento no duplica:

```csharp
var existente = _filas.FirstOrDefault(f => f.Documento == documento);
if (existente is not null) return (existente, false);
```

En la demo la «tabla» es un archivo JSON protegido con un candado (`Lock`) para que dos pedidos simultáneos no lo
escriban a la vez. En un producto real es una tabla de la base de datos con un **índice único** sobre `Documento`,
que da la misma garantía.

Cuando se crea una fila nueva, el servicio avisa:

```csharp
if (!creada) return (ResultadoDeRegistro.YaExistia, fila);
await aviso.AvisarAsync();
```

`AvisoDeArchivos` es un evento de C# dentro del proceso. Cada página Blazor abierta se suscribe y se redibuja
(sección 4.2).

### 3.8 La API con controllers y Scalar

La API sigue el estilo de **controllers**: una clase por recurso, con atributos que dicen la ruta y el método HTTP.

```csharp
[ApiController]
[Route("api/archivos")]
[Authorize]
[Produces("application/json")]
public sealed class ArchivosController(RegistroDeArchivos registro, ServicioDeArchivos servicio) : ControllerBase
```

- `[ApiController]` activa comportamientos de API: valida el cuerpo del pedido y responde `400` si no se puede leer.
- `[Route("api/archivos")]` es la ruta base de todos sus métodos.
- `[Authorize]` exige un usuario autenticado en todos sus métodos; sin clave, la respuesta es `401`.
- Los parámetros del constructor (`registro`, `servicio`) los entrega la **inyección de dependencias**: ASP.NET
  Core busca lo registrado en `Program.cs`.

El método que registra traduce cada resultado del servicio a un código HTTP:

```csharp
return resultado switch
{
    ResultadoDeRegistro.Creado => CreatedAtAction(nameof(Obtener), new { id = fila!.Id }, fila),
    ResultadoDeRegistro.YaExistia => Ok(fila),
    ResultadoDeRegistro.NoExiste => NotFound($"No existe la suba {pedido.IdSuba}"),
    _ => Conflict($"La suba {pedido.IdSuba} todavía no terminó")
};
```

| Resultado | Código | Significado |
| --- | --- | --- |
| `Creado` | `201 Created` | Fila nueva, con la cabecera `Location` apuntando a `GET /api/archivos/<id>` |
| `YaExistia` | `200 OK` | Reintento: devuelve la fila que ya estaba |
| `NoExiste` | `404 Not Found` | No hay suba con ese id |
| `Incompleto` | `409 Conflict` | La suba existe pero no terminó |

La **descarga** vive en otro controller, en `/descargas`, porque `/archivos/<id>` ya lo atiende tus con `HEAD`,
`PATCH` y `DELETE`, y mezclar un `GET` propio en la misma ruta confunde a los dos:

```csharp
var estado = await almacen.ConsultarAsync(id, ct);
if (!estado.Existe) return NotFound();
if (!estado.Completa) return Conflict("La suba todavía no terminó");

var archivo = await almacen.GetFileAsync(id, ct);
var contenido = await archivo.GetContentAsync(ct);
return File(contenido, estado.Tipo, estado.Nombre);
```

`File(contenido, tipo, nombre)` devuelve el contenido con su `Content-Type` y con el nombre original para que el
navegador lo guarde así.

**OpenAPI y Scalar.** Con `AddOpenApi` y `MapOpenApi`, ASP.NET Core publica la descripción de la API en
`/openapi/v1.json`, armada a partir de los atributos (`[HttpPost]`, `[ProducesResponseType]`) y de los comentarios.
`MapScalarApiReference` publica en `/scalar` una página que lee ese documento y permite probar cada endpoint. Para
probar desde Scalar, agregá la cabecera `X-Clave-Demo` en el pedido.

---

## 4. Los clientes

### 4.1 El cliente de navegador

La demo trae un cliente tus **escrito a mano** en `wwwroot/js/tus-cliente.js`, de unas cien líneas, para que se vea
el protocolo completo. En un producto conviene la biblioteca `tus-js-client` (sección 8).

**Recordar la suba para retomarla.** El cliente arma una huella del archivo (nombre, tamaño y fecha de
modificación) y guarda la URL de la suba en el `localStorage` del navegador, que sobrevive a cerrar la pestaña:

```javascript
function huella(archivo) {
  return `tus::${archivo.name}::${archivo.size}::${archivo.lastModified}`;
}
```

**Paso 1: ¿hay una suba anterior?** Si la hay, pregunta con `HEAD` cuánto llegó. Si el servidor ya no la tiene
(venció), la olvida y crea una nueva.

```javascript
if (url) {
  const avance = await consultarAvance(url, cabeceras);
  if (avance === null) { localStorage.removeItem(llave); url = null; }
  else offset = avance;
}
```

**Paso 2: crear la suba.** Manda el largo y los metadatos en Base64, y guarda la URL **antes** de enviar el primer
trozo. Si la guardara después y la red se cortara justo en el primer trozo, no sabría qué suba retomar.

```javascript
const r = await fetch(endpoint, {
  method: "POST",
  headers: {
    ...cabeceras,
    "Upload-Length": String(archivo.size),
    "Upload-Metadata": `filename ${base64(archivo.name)},filetype ${base64(archivo.type || "application/octet-stream")}`,
  },
});
if (r.status !== 201) throw new Error(`Crear: ${r.status} ${await r.text()}`);
url = new URL(r.headers.get("Location"), endpoint).href; // Location puede ser relativa
localStorage.setItem(llave, url); // guardar ANTES del primer trozo
```

`new URL(relativa, base)` resuelve la cabecera `Location` contra la dirección del endpoint: tusdotnet la devuelve
relativa (`/archivos/<id>`).

**Paso 3: enviar trozo por trozo.** `archivo.slice(desde, hasta)` toma una parte del archivo sin leerlo entero en
memoria. Después de cada `PATCH`, el offset nuevo es **el que confirma el servidor**, no el que calcula el cliente:

```javascript
while (offset < archivo.size) {
  const pedazo = archivo.slice(offset, offset + trozo);
  const r = await fetch(url, {
    method: "PATCH",
    headers: { ...cabeceras, "Upload-Offset": String(offset), "Content-Type": "application/offset+octet-stream" },
    body: pedazo,
    signal: controlador.signal,
  });
  if (r.status === 409) { // el servidor tiene otro offset: preguntar y seguir desde ahí
    offset = await consultarAvance(url, cabeceras);
    continue;
  }
  if (r.status !== 204) throw new Error(`Enviar: ${r.status} ${await r.text()}`);
  offset = Number(r.headers.get("Upload-Offset"));
  await dotnet.invokeMethodAsync("Avance", offset, archivo.size);
}
```

**Pausar.** `controlador` es un `AbortController`: llamar a `abort()` corta el `fetch` en curso. Pausar y que se
corte la red son, para el cliente, lo mismo: la próxima vez retoma con `HEAD`.

**Por qué la descarga no es un enlace.** El endpoint de descarga exige la cabecera `X-Clave-Demo`, y un enlace
`<a href>` no puede mandar cabeceras. El cliente pide el archivo con `fetch`, lo convierte en un objeto en memoria
(`blob`) y lo entrega al navegador como descarga.

### 4.2 La página Blazor Interactive Server

`Components/Pages/Home.razor` es la pantalla. Con `@rendermode InteractiveServer`, el componente **corre en el
servidor**: sus variables viven allá, y el navegador recibe sólo los cambios de la pantalla por el circuito
(SignalR).

Eso tiene una consecuencia importante: **el archivo elegido está en el navegador, no en el servidor**. Si se usara
el componente `InputFile` de Blazor, los bytes viajarían por el circuito al servidor, que es justo lo que tus
evita. Por eso la página usa un `<input type="file">` común y le pasa la referencia al JavaScript, que sube
directo al endpoint tus.

**Cargar el módulo de JavaScript.** En el primer dibujo, la página importa el módulo y crea una referencia a sí
misma para que JavaScript pueda llamarla:

```csharp
protected override async Task OnAfterRenderAsync(bool primeraVez)
{
    if (!primeraVez) return;
    modulo = await JS.InvokeAsync<IJSObjectReference>("import", "./js/tus-cliente.js");
    yo = DotNetObjectReference.Create(this);
}
```

**Arrancar y volver.** Una llamada de .NET a JavaScript tiene un tiempo máximo de espera; una suba de un video
puede durar mucho más. Por eso la función `subir` del módulo **arranca la suba y vuelve enseguida**, y el avance y el
final llegan después, cuando JavaScript llama a métodos marcados con `[JSInvokable]`:

```javascript
export function subir(selector, opciones, dotnet) {
  const archivo = selector.files[0];
  if (!archivo) {
    dotnet.invokeMethodAsync("Fallo", "Error", "Elegí un archivo primero.");
    return;
  }
  trabajar(archivo, opciones, dotnet)
    .then((id) => dotnet.invokeMethodAsync("Terminado", id))
    .catch((e) => dotnet.invokeMethodAsync("Fallo", e.name ?? "Error", e.message ?? String(e)));
}
```

```csharp
[JSInvokable]
public async Task Terminado(string idSuba)
{
    // La suba terminó: recién ahora se inserta la fila.
    var (resultado, fila) = await Servicio.RegistrarAsync(idSuba, descripcion, "demo-web", CancellationToken.None);
```

`Terminado` llama al mismo `ServicioDeArchivos` que usa el controller: las dos puertas (la página y la API) siguen
el mismo orden.

**`InvokeAsync(StateHasChanged)`.** Los métodos `[JSInvokable]` y el aviso de cambios llegan desde afuera del ciclo
normal del componente. `StateHasChanged` pide redibujar, e `InvokeAsync` lo hace en el contexto del componente,
donde es seguro.

**La lista que se actualiza sola.** Al iniciar, la página se suscribe al aviso; al cerrarse, se desuscribe:

```csharp
protected override void OnInitialized()
{
    archivos = Registro.Todos();
    Aviso.Cambio += AlCambiarLaListaAsync;
}
```

```csharp
private async Task AlCambiarLaListaAsync()
{
    archivos = Registro.Todos();
    await InvokeAsync(StateHasChanged);
}
```

Cuando cualquier cliente registra un archivo —la página en otra pestaña, la API desde Scalar o el cliente de
consola—, `AvisoDeArchivos` llama a todas las páginas suscriptas, cada una relee la lista y el cambio viaja a su
navegador por el circuito. No hace falta un hub de SignalR propio: el circuito de Blazor ya es una conexión SignalR
abierta con cada navegador.

**Desuscribirse siempre.** `DisposeAsync` saca la suscripción. Si no, el servicio (que es único y vive toda la
aplicación) seguiría guardando una referencia a cada página cerrada: una pérdida de memoria.

### 4.3 El cliente .NET

`TusDemo.ClienteConsola` muestra el mismo protocolo desde .NET, con `HttpClient`. Es el esqueleto que usaría una
aplicación móvil (.NET MAUI).

```csharp
public async Task<Uri> CrearAsync(Uri endpoint, long largo, string nombre, string tipo, CancellationToken ct)
{
    using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
    req.Headers.Add("Tus-Resumable", Version);
    req.Headers.Add("Upload-Length", largo.ToString());
    req.Headers.Add("Upload-Metadata", $"filename {B64(nombre)},filetype {B64(tipo)}");
    using var res = await http.SendAsync(req, ct);
    if (res.StatusCode != HttpStatusCode.Created)
        throw new HttpRequestException($"Crear: {(int)res.StatusCode} {await res.Content.ReadAsStringAsync(ct)}");
    return new Uri(endpoint, res.Headers.Location!); // Location puede ser relativa
}
```

`EnviarAsync` empieza siempre preguntando el avance con `HEAD`, así sirve igual para la primera vez y para retomar:

```csharp
var offset = await AvanceAsync(suba, ct);
var buffer = new byte[trozo];
while (offset < largo)
{
    origen.Seek(offset, SeekOrigin.Begin);
```

`origen.Seek(offset, …)` mueve la lectura del archivo local al byte desde el que hay que seguir.

**El corte simulado.** El programa de consola cancela el envío después de dos trozos con un
`CancellationTokenSource`, que es la forma de .NET de pedir que una operación en curso se detenga:

```csharp
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
```

Después pregunta el avance, retoma, registra la fila dos veces (para mostrar que no se duplica), baja el archivo y
compara las huellas SHA-256. En una aplicación móvil real, la URL de la suba se guarda en la base local **después
de crearla y antes del primer trozo**, para retomar aunque la aplicación se cierre.

---

## 5. El contenedor

### 5.1 El `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0.404 AS build
WORKDIR /src
COPY src/TusDemo.Web/TusDemo.Web.csproj src/TusDemo.Web/
RUN dotnet restore src/TusDemo.Web/TusDemo.Web.csproj
COPY src/TusDemo.Web/ src/TusDemo.Web/
RUN dotnet publish src/TusDemo.Web/TusDemo.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.15
WORKDIR /app
COPY --from=build /app .
# La carpeta de datos es del usuario sin privilegios "app" (uid 1654) que traen las imágenes de .NET.
RUN mkdir -p /data && chown "$APP_UID" /data
USER $APP_UID
ENV Archivos__Raiz=/data/tus \
    Archivos__Registro=/data/registro.json
EXPOSE 8080
ENTRYPOINT ["dotnet", "TusDemo.Web.dll"]
```

- **Dos etapas.** La primera (`build`) tiene el SDK y compila; la segunda tiene sólo el runtime de ASP.NET Core y
  copia lo compilado. La imagen final no lleva el compilador: pesa menos y expone menos.
- **Primero el `.csproj`, después el código.** Docker guarda cada paso en una capa. Si sólo cambia el código y no
  el `.csproj`, el `dotnet restore` se reutiliza y no se bajan los paquetes de nuevo.
- **Usuario sin privilegios.** Las imágenes de .NET traen un usuario `app` con uid 1654, en la variable `APP_UID`.
  `USER $APP_UID` hace que el proceso no corra como root. Antes, todavía como root, se crea `/data` y se le da a ese
  usuario.
- **Puerto 8080.** Desde .NET 8, las imágenes de ASP.NET Core escuchan en el 8080 y no en el 80, que es un puerto
  que sólo root puede abrir.
- **Configuración por variables de entorno.** `Archivos__Raiz=/data/tus` reemplaza el valor de `appsettings.json`
  (sección 3.2).

### 5.2 `compose.yaml` y el volumen

```yaml
services:
  tus-demo:
    build: .
    image: tus-demo:local
    ports:
      - "8090:8080"
    volumes:
      - tus-datos:/data          # volumen nombrado: sobrevive a borrar el contenedor
    environment:
      Archivos__ClaveDemo: "clave-de-demo-cambiar"
volumes:
  tus-datos:
```

- `ports: "8090:8080"` publica el 8080 del contenedor en el 8090 de tu máquina.
- `volumes: tus-datos:/data` monta un **volumen nombrado** en `/data`. Los archivos subidos quedan en el volumen, no
  en el contenedor: si lo borrás y lo creás de nuevo, siguen ahí. Un volumen nombrado nuevo toma el dueño de la
  carpeta `/data` de la imagen, por eso el usuario `app` puede escribir.
- Si en lugar del volumen montás una carpeta de tu máquina (`./datos:/data`), esa carpeta tiene que pertenecer al
  uid 1654, o el servicio no va a poder escribir.

---

## 6. Cómo se verificó la demo

La demo se compiló y se probó el **2026-11-27**, en contenedores efímeros sobre un host Linux x86_64, con la imagen
`mcr.microsoft.com/dotnet/sdk:10.0.404`. La compilación terminó con 0 advertencias y 0 errores.

El script [`scripts/probar.sh`](Demos/scripts/probar.sh) compila, levanta el servicio y recorre estos casos. Todos
dieron el resultado esperado:

| Caso | Esperado | Obtenido |
| --- | --- | --- |
| `OPTIONS /archivos` | `Tus-Version` con `1.0.0` | `1.0.0` |
| `POST` sin clave | `401` | `401` |
| `POST` sin metadatos | `400` | `400` |
| `POST` con extensión `.exe` | `400` | `400` |
| `POST` de 30 MB con tope de 20 MB | `413` | `413` |
| `POST` válido | `201` con `Location` | `/archivos/5be63826212b4f10abf054965ff13de8` |
| `PATCH` con offset equivocado | `409` | `409` |
| `PATCH` del primer trozo | `204` | `204` |
| `HEAD` después de 5 bytes | `Upload-Offset: 5` | `5` |
| `GET /descargas/<id>` de una suba incompleta | `409` | `409` |
| Registrar una suba incompleta | `409` | `409` |
| `GET /descargas/<id>` sin clave | `401` | `401` |
| `GET /descargas/<id>` completa | el contenido | `abcdefghij` |
| Registrar la suba completa | `201` | `201` |
| Registrar otra vez | `200`, sin fila nueva | `200` |
| `Documento` de la fila | `descargas/<id>` | `descargas/5be63826212b4f10abf054965ff13de8` |
| `GET /descargas/..%2Fregistro` | `404` | `404` |
| Registrar una suba que no existe | `404` | `404` |
| `/openapi/v1.json` y `/scalar` | `200` | `200` y `200` (Scalar redirige primero) |
| Página Blazor | carga | carga |

Y el cliente .NET, con un archivo de 6.754.388 bytes en trozos de 1 MB:

```text
corte simulado después de 2 trozos
HEAD después del corte: Upload-Offset=2097152 de 6754388
HEAD al terminar: Upload-Offset=6754388
registro: 201 y repetido: 200
sha256 original=A76D6BF240BDC0BEA3F95115A13B3331C1394333300D1A77AE7721D766551918
sha256 bajado  =A76D6BF240BDC0BEA3F95115A13B3331C1394333300D1A77AE7721D766551918
RESULTADO: OK, mismo contenido
```

La imagen del `Dockerfile` también se construyó y se probó:

| Caso | Obtenido |
| --- | --- |
| Usuario de la imagen | `1654` |
| `id -u` del proceso en el contenedor | `1654` |
| Subir `hola!`, borrar el contenedor, crearlo de nuevo con el mismo volumen y leer | `hola!` |

La página Blazor se comprobó por su respuesta HTTP; la interacción en el navegador (elegir, pausar, retomar) no se
automatizó y conviene probarla a mano siguiendo la sección 2.3.

---

## 7. Errores comunes

| Error | Qué pasa | Cómo se evita |
| --- | --- | --- |
| Exigir credenciales en `OPTIONS` | El preflight del navegador recibe `401` y la suba nunca empieza. Le pasó a la primera versión de la demo | En `OnAuthorizeAsync`, dejar pasar `IntentType.GetOptions` (sección 3.4) |
| Correr el `.dll` desde otra carpeta | La aplicación toma como raíz de contenido la carpeta actual, no encuentra `appsettings.json` y arranca con los valores por omisión (en la demo, sin extensiones permitidas: rechazaba todo). Le pasó a la primera prueba | Correr desde la carpeta del proyecto, con `dotnet run`, o fijar la raíz de contenido |
| Crear la fila al empezar la suba | Si el cliente no termina, queda una fila que apunta a un archivo a medias | Registrar después de la suba completa (sección 3.7) |
| Guardar la URL completa en `Documento` | Mudar el servicio obliga a reescribir la base | Guardar `descargas/<id>` |
| Armar la ruta en disco con el nombre del usuario | Choques entre nombres iguales y riesgo de `../` | Usar el id del almacén y validar el id que llega |
| Servir sin comprobar que la suba terminó | Se entrega un archivo truncado como si estuviera bien | Comparar offset y largo antes de servir (sección 3.6) |
| Confiar en `filetype` | Lo declara el cliente: un ejecutable puede llegar como `image/jpeg` | Verificar el contenido en un proceso posterior a `OnFileCompleteAsync` |
| Trozos sin tope detrás de un proxy | Kestrel acepta por defecto hasta 30.000.000 bytes por pedido y los proxys tienen su propio límite: un trozo más grande recibe `413` o se corta la conexión | Trozos menores que el límite más chico del camino |
| Trabajo lento en `OnFileCompleteAsync` | El cliente espera la respuesta de su último trozo y, si el proceso falla, no se puede reintentar | Encolar y procesar después |
| Llamar a JavaScript y esperar a que la suba termine | Una suba larga pasa el tiempo máximo de la interop y la llamada falla | Arrancar y volver; avisar con `[JSInvokable]` (sección 4.2) |
| No desuscribirse del aviso | Cada página cerrada queda en memoria | Desuscribir en `DisposeAsync` |
| Montar una carpeta del host sin ajustar el dueño | El usuario `app` no puede escribir en `/data` | Volumen nombrado, o carpeta del uid 1654 |

---

## 8. Del ejemplo a producción

La demo es deliberadamente chica. Lo que cambia en un producto:

1. **Autenticación real.** Reemplazar `ClaveDemoHandler` por el esquema del producto (JWT, cookies). La clave nunca
   viaja a la página, como sí hace la demo.
2. **Base de datos.** Reemplazar el JSON por una tabla con índice único sobre `Documento`.
3. **Varias instancias.** Si corren varias copias del servicio, todas tienen que ver el mismo almacén; si no, una
   suba creada en una no existe en la otra.
4. **Limpieza de vencidas.** Las subas incompletas vencen, pero alguien tiene que borrarlas. `TusDiskStore`
   implementa `ITusExpirationStore`; el producto programa una tarea periódica que las borre.
5. **Biblioteca de cliente.** En el navegador, `tus-js-client` agrega reintentos automáticos (`retryDelays`) y
   retoma con `findPreviousUploads` y `resumeFromPreviousUpload`. No fija trozos por defecto: hay que darle
   `chunkSize` si hay un proxy con límite.
6. **CORS.** Si la página y el servicio están en orígenes distintos, el servicio tiene que exponer las cabeceras de
   tus para que el navegador pueda leer `Location` y `Upload-Offset`. tusdotnet trae
   `CorsHelper.GetExposedHeaders()` para eso.
7. **Verificación del contenido.** Antivirus y tipo real del archivo, encolados desde `OnFileCompleteAsync`.
8. **Imágenes fijadas por digest.** En producción, las imágenes base se fijan por su huella (`@sha256:…`), no sólo
   por la etiqueta.

La base de conocimientos del equipo tiene este mismo diseño como conocimiento reutilizable:
`Media-File-Service-TUS` (en `IA.Knowledge.Base`), del que la demo es la prueba.

---

## 9. Referencias

Consultadas el 2026-11-27.

- tus, «Resumable upload protocol 1.0.0», en el repositorio de la especificación:
  <https://github.com/tus/tus-resumable-upload-protocol> (consultado el 2026-11-27)
- tusdotnet, repositorio y wiki (configuración, eventos, CORS, Kestrel): <https://github.com/tusdotnet/tusdotnet> (consultado el 2026-11-27)
- tusdotnet en NuGet: <https://www.nuget.org/packages/tusdotnet> (consultado el 2026-11-27)
- tus-js-client: <https://github.com/tus/tus-js-client> (consultado el 2026-11-27)
- Scalar.AspNetCore en NuGet: <https://www.nuget.org/packages/Scalar.AspNetCore> (consultado el 2026-11-27)
- Microsoft Learn, puerto 8080 en las imágenes de ASP.NET Core:
  <https://learn.microsoft.com/dotnet/core/compatibility/containers/8.0/aspnet-port> (consultado el 2026-11-27)
- Microsoft Learn, opciones de Kestrel (`MaxRequestBodySize`):
  <https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/options> (consultado el 2026-11-27)
- .NET Blog, contenedores sin root: <https://devblogs.microsoft.com/dotnet/securing-containers-with-rootless/> (consultado el 2026-11-27)
