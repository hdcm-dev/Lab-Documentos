---
doc_id: TEM-HEADERS
doc_type: tema
title: Cabeceras y negociación de contenido
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-HTTP, TEM-METODOS, TEM-STATUS, TEM-CACHE, TEM-IDEM, TEM-HATEOAS, TEM-PAG, TEM-VERS, TEM-PROT, TEM-SERIAL, MARCO-ESCENARIOS, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Cabeceras y negociación de contenido — `TEM-HEADERS`

## Resumen ejecutivo

Las cabeceras son el canal por el que viaja todo lo que no es ni la identificación del recurso ni su representación: qué formato se prefiere, en qué idioma, dónde quedó lo que se creó, adónde seguir, cuánto esperar antes de reintentar. Son también la parte del protocolo que más se improvisa, porque agregar una cabecera propia no requiere permiso de nadie y funciona de inmediato.

`N-01` §12 define la negociación de contenido y `N-10` (RFC 8288) define la cabecera `Link` y su registro de relaciones, que administra IANA (`N-11`). Fuera de eso hay un territorio amplio de cabeceras de aplicación sin especificación que las respalde, y este documento se ocupa de que la frontera quede visible.

Una advertencia de método vale desde el principio: la negociación de contenido es una de las funciones más completas de HTTP y una de las menos usadas en APIs reales. La mayoría de las APIs sirve JSON y solo JSON, y en ese caso implementar negociación completa es ceremonia. Lo que sí conviene siempre es no romperla: declarar `Content-Type` con precisión, respetar `Accept` cuando se puede, y emitir `Vary` cuando la respuesta depende de una cabecera de petición.

---

## Definición

Una cabecera HTTP es un par nombre-valor que acompaña a una petición o a una respuesta y transporta metadatos sobre el mensaje, sobre la representación que lleva o sobre el recurso al que se refiere. `N-01` distingue varias familias según a qué se refieran, y la distinción importa para saber qué se cachea, qué se reenvía y qué puede alterar un intermediario.

**Negociación de contenido** es el mecanismo por el cual cliente y servidor acuerdan qué representación de un recurso se transfiere, entre varias disponibles. `N-01` §12 la llama proactiva cuando el cliente expresa preferencias —`Accept`, `Accept-Encoding`, `Accept-Language`— y el servidor elige.

### Qué no es

La negociación de contenido no es un mecanismo de versionado de API, aunque se la use para eso. Versionar por media type —`Accept: application/vnd.salas.v2+json`— es técnicamente posible y es la opción que las guías más «puristas» prescriben; `G-05` (Zalando) la exige en su regla 114 y prohíbe la versión en URL en la 115. La evidencia de plataformas es demoledora en la dirección contraria: **ninguna** de las verificadas —Stripe, GitHub, Shopify, Twilio, Azure— la usa. Es la opción más recomendada con cero adopción en el conjunto medido, y `G-06` (GOV.UK) justifica su rechazo por el riesgo de que *proxies* y cortafuegos bloqueen media types no reconocidos. El tema completo es de [`TEM-VERS`](../50-Evolucion-y-Versionado/Estrategias-de-Versionado.md).

Tampoco es el lugar donde poner datos de negocio. Una cabecera propia que transporta el identificador del inquilino o el rol del usuario funciona y es invisible en la documentación generada, en los ejemplos y en las pruebas de contrato. Lo que pertenece al recurso va en el recurso.

---

## Negociación de contenido

### `Accept` y `Content-Type`

`Content-Type` declara el media type de la representación que **este mensaje lleva**; aplica tanto a peticiones como a respuestas. `Accept` declara los media types que el cliente **está dispuesto a recibir**, con preferencias relativas expresadas por el parámetro de calidad `q`.

```http
GET /v1/salas/SEDE-NORTE-201 HTTP/1.1
Host: api.salas.ejemplo.com
Accept: application/json;q=1.0, application/xml;q=0.5
```

```http
HTTP/1.1 200 OK
Content-Type: application/json
Vary: Accept
ETag: "b91d40"

{ "codigo": "SEDE-NORTE-201", "nombre": "Sala Norte 201", "capacidad": 12 }
```

Cuando ninguna representación disponible satisface el `Accept`, corresponde `406 Not Acceptable` (`N-01` §15.5.7). Cuando el `Content-Type` de la petición no está soportado por el recurso, corresponde `415 Unsupported Media Type` (§15.5.16). Confundir ambos es frecuente y da mensajes desconcertantes: `406` habla de lo que el cliente quiere recibir, `415` de lo que el cliente envió.

En una API que solo sirve JSON hay una decisión de criterio: qué hacer con un `Accept: application/xml`. Responder `406` es correcto según la norma. Responder JSON de todos modos es lo que hace la mayoría de las implementaciones y lo que hace ASP.NET Core por defecto —`ReturnHttpNotAcceptable` viene en `false`—. Esta guía recomienda activar el rechazo en `CTX-1`, donde el silencio produce integradores que deserializan mal sin enterarse, y considera indistinto el ajuste en `CTX-2`.

En `PATCH`, el `Content-Type` deja de ser un detalle: distingue `application/json-patch+json` de `N-06` de `application/merge-patch+json` de `N-07`, que piden operaciones distintas sobre el mismo recurso. `N-05` prevé la cabecera de respuesta `Accept-Patch`, que *SHOULD* aparecer en la respuesta a `OPTIONS` de un recurso que admita `PATCH`, precisamente para que el cliente no tenga que adivinar.

### `Accept-Language`

Expresa el idioma preferido para la respuesta. En una API su alcance útil es acotado y conviene tenerlo claro: **los datos del dominio no se traducen; los textos destinados a personas sí**. Un nombre de sala es un dato y viaja como está; el `title` y el `detail` de un cuerpo de error de `N-04` son textos y pueden localizarse —`N-04` contempla explícitamente la localización como la única razón por la que `title` puede variar entre ocurrencias del mismo tipo de problema—.

```http
POST /v1/reservas HTTP/1.1
Host: api.salas.ejemplo.com
Accept-Language: es-AR, es;q=0.9, en;q=0.5
Content-Type: application/json

{ "salaId": "a3f1", "desde": "2026-08-03T11:00:00Z", "hasta": "2026-08-03T09:00:00Z" }
```

```http
HTTP/1.1 422 Unprocessable Content
Content-Type: application/problem+json
Content-Language: es-AR
Vary: Accept-Language

{ "type": "https://api.salas.ejemplo.com/problemas/validacion",
  "title": "La petición no supera la validación", "status": 422,
  "detail": "El fin de la reserva debe ser posterior al inicio." }
```

Localizar los mensajes de error tiene un efecto secundario que hay que anticipar: si el cliente ramifica sobre el texto, la localización lo rompe. Es la razón por la que el patrón de dos niveles de [`TEM-STATUS`](Codigos-de-Estado.md) existe —un `code` de cadena estable para la máquina, un texto para la persona— y por la que `N-04` dice que `detail` no debe parsearse.

### `Vary`

Declara qué cabeceras de la petición influyeron en la selección de la representación. Es la pieza que hace correcta la negociación en presencia de cachés: sin `Vary`, una caché compartida puede servirle a un cliente la representación negociada para otro.

Es la cabecera más olvidada de esta familia y su ausencia produce defectos difíciles de reproducir, porque solo se manifiestan con una caché intermedia de por medio. La regla es mecánica: **si la respuesta depende de una cabecera de petición, esa cabecera va en `Vary`**. Y tiene un caso especialmente peligroso: una respuesta que depende de `Authorization` y se marca cacheable sin `Vary` puede terminar servida al usuario equivocado, riesgo que desarrolla [`TEM-CACHE`](Cache-y-Peticiones-Condicionales.md).

---

## Cabeceras de representación y de recurso

`ETag` y `Last-Modified` son los dos validadores que define `N-01` §8.8. Aparecen acá porque son cabeceras, y se desarrollan en [`TEM-CACHE`](Cache-y-Peticiones-Condicionales.md) —donde sirven para revalidar— y en [`TEM-IDEM`](Idempotencia-y-Concurrencia.md) —donde sirven para detectar escrituras concurrentes—.

`Content-Length`, `Content-Encoding` y `Content-Language` describen la representación transferida. La primera la gestiona el servidor; la segunda depende de la compresión negociada con `Accept-Encoding`; la tercera acompaña a la localización descrita arriba.

### `Location`

Identifica un recurso relacionado con la respuesta. `N-01` §10.2.2 la asocia a `201 Created` —donde apunta al recurso creado— y a las redirecciones `3xx` —donde apunta al destino—.

```http
HTTP/1.1 201 Created
Location: /v1/reservas/9c2b
Content-Type: application/json

{ "id": "9c2b", "estado": "confirmada" }
```

Su tercer uso, menos difundido y más valioso, es el de las operaciones asíncronas: un `202 Accepted` con `Location` apuntando al recurso de seguimiento es lo que convierte al `202` en una respuesta accionable en lugar de una promesa vacía.

Hay una decisión de forma que conviene tomar de entrada y sostener: URI absoluta o relativa. `G-01` (Azure) exige URL absolutas en su campo `nextLink`, incluyendo el parámetro de versión, con el argumento de que el cliente no debe reconstruir nada. Es prescripción de organización, no norma. Lo que sí es defecto es alternar entre ambas formas dentro de la misma API.

### `Retry-After`

Indica cuánto conviene esperar antes de reintentar. La define `N-01` §10.2.3 —**sección no consultada individualmente en la verificación de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md)**; quien necesite apoyarse en su redacción exacta debe abrir el RFC—. Acompaña naturalmente a `503 Service Unavailable` y a `429 Too Many Requests`, este último definido por `N-03` y no por `N-01`.

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 30
```

Admite dos formas: segundos como entero, o una fecha HTTP. La primera es la habitual en APIs y la que menos problemas de reloj produce.

Su ausencia tiene una consecuencia previsible y mala. Un cliente que recibe `429` sin `Retry-After` reintenta según su propia política, que en la mayoría de las bibliotecas es de inmediato o con un retroceso corto, y agrava la condición que el límite quería aliviar. Emitir el código sin la cabecera es comunicar la mitad del mensaje.

---

## `Link` y las relaciones de enlace

`N-10` (RFC 8288, que obsoleta RFC 5988) define la cabecera `Link` como forma de expresar relaciones tipadas entre recursos en el canal de cabeceras, sin tocar el cuerpo.

```
Link: <target-uri>; rel="relation-type"; param=...
```

El atributo `rel` es obligatorio; `N-10` define además `anchor` —que cambia el contexto del enlace—, `title`, `type`, `hreflang` y `media`. Varios enlaces se separan por coma o se emiten en cabeceras `Link` sucesivas.

```http
GET /v1/salas/a3f1/reservas?desde=2026-08-01&limite=20 HTTP/1.1
Host: api.salas.ejemplo.com
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json
Link: </v1/salas/a3f1/reservas?desde=2026-08-01&limite=20>; rel="self",
      </v1/salas/a3f1/reservas?desde=2026-08-01&limite=20&cursor=eyJ0Ijoi>; rel="next",
      </v1/salas/a3f1/reservas?desde=2026-08-01&limite=20>; rel="first"

{ "datos": [ … ] }
```

### El registro IANA de relaciones

Hay una precisión que se equivoca constantemente y que la verificación de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md) dejó establecida: **`N-10` define la cabecera y el registro, no los nombres de relación**. Los nombres viven en el registro que administra IANA (`N-11`), y cada uno tiene su propia fuente.

| Relación | Qué significa | Fuente registrada |
|---|---|---|
| `self` | Identificador del contexto del enlace | RFC 4287 |
| `next` | Siguiente de una serie | Especificación HTML |
| `prev` | Anterior de una serie | Especificación HTML |
| `previous` | Sinónimo de `prev` | HTML 4.01 |
| `first` | Recurso más precedente de la serie | RFC 8288 |
| `last` | Recurso más siguiente de la serie | RFC 8288 |
| `related` | Recurso relacionado | RFC 4287 |
| `describedby` | Recurso que describe al contexto | POWDER |
| `collection` | Colección a la que pertenece el contexto | RFC 6573 |
| `item` | Miembro de la colección | RFC 6573 |

De las relaciones de paginación, solo `first` y `last` provienen de `N-10`; `self`, `next`, `prev` y `related` vienen de RFC 4287 y de HTML. Decir «`rel="next"` según RFC 8288» es inexacto; lo correcto es citar `N-11` para la relación y `N-10` para la cabecera. Nótese además que `prev` y `previous` están **ambos** registrados, siendo el segundo sinónimo del primero: elegir uno y no mezclarlos evita clientes que solo reconocen una de las dos formas.

Una relación no registrada se expresa con un URI en lugar de un nombre corto, y eso permite usar relaciones propias sin colisionar con el registro.

### Cabecera o cuerpo

`N-10` es la vía estándar y la práctica está dividida de forma llamativa. GitHub emite la cabecera `link` de `N-10` para paginación (`P-06`). Azure (`G-01`), JSON:API (`F-04`) y Stripe (`P-03`) ponen la navegación **en el cuerpo**: `nextLink`, un objeto `links`, y un campo `url` con `has_more` respectivamente. La prescripción «usá `Link`» no describe lo que hace la mayoría del ecosistema.

El argumento a favor del cuerpo es práctico: muchos clientes de alto nivel exponen el JSON deserializado y no las cabeceras, y obligar al consumidor a leer una cabecera para paginar agrega fricción. El argumento a favor de la cabecera es que la navegación es metadato del recurso, no parte de su representación, y que así el mismo mecanismo sirve para cualquier media type. Ambos son legítimos; lo que no lo es es hacer las dos cosas de manera inconsistente. La decisión de paginación completa es de [`TEM-PAG`](../40-Contratos-y-Representaciones/Colecciones-y-Paginacion.md), y la discusión sobre qué sobrevivió de hipermedia es de [`TEM-HATEOAS`](../10-Fundamentos-REST/Hipermedia.md).

Un detalle práctico verificado en `G-01` que rinde más de lo que aparenta: **omitir el enlace a la página siguiente en la última página en lugar de enviarlo con valor nulo**. Le ahorra al cliente una petición y una rama de código.

---

## Cabeceras personalizadas y el prefijo `X-`

Una API puede definir cabeceras propias, y las razones legítimas son pocas y reconocibles: correlación de peticiones para diagnóstico, versión solicitada, clave de idempotencia, y poco más.

El prefijo `X-` fue durante años la forma convencional de marcar una cabecera como no estandarizada, y cayó en desuso. **Esta guía no puede citar una fuente verificada para esa deprecación**: el documento del IETF que la formaliza no figura en [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md) y no se verificó en la sesión que produjo estas fichas. Lo que sí está documentado es el argumento y su evidencia práctica. El argumento: una cabecera experimental que se vuelve permanente queda con un prefijo que miente, y si después se estandariza hay que sostener las dos formas para siempre. La evidencia: los campos `RateLimit` de `F-02` atraviesan exactamente ese problema —el ecosistema usa `X-RateLimit-Limit`, `X-RateLimit-Remaining` y `X-RateLimit-Reset`, drafts anteriores usaron los mismos nombres sin prefijo, y el draft vigente **no usa ninguno de los dos conjuntos**: define solo `RateLimit-Policy` y `RateLimit` con parámetros de una letra—.

La conducta de las plataformas verificadas es mixta y honesta al respecto: GitHub versiona con `X-GitHub-Api-Version` (`P-05`), Stripe con `Stripe-Version` sin prefijo (`P-01`), Azure exige `x-ms-error-code` (`G-01`). Ninguna forma es normativa.

Esta guía recomienda **no usar el prefijo `X-`** en cabeceras nuevas, usar un prefijo de organización cuando haya riesgo de colisión —`Salas-Request-Id` antes que `Request-Id`—, y documentar cada cabecera propia en la especificación OpenAPI con el mismo rigor que un campo del cuerpo. La cabecera que no está en la especificación es la que el consumidor descubre por accidente y de la que después depende sin garantía, que es el modo de fallar de `ACT-03` según [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md).

Dos cabeceras propias merecen mención por su difusión. `Idempotency-Key` es convención de facto impuesta por Stripe (`P-04`) sobre un draft IETF **expirado** (`F-01`), y la trata [`TEM-IDEM`](Idempotencia-y-Concurrencia.md). Los campos `RateLimit` de `F-02` son un Internet-Draft activo con expiración el 2026-11-24; su uso es razonable y su presentación como estándar no lo es.

---

## Aplicación por escenario

### `ESC-1` — API nueva

Casi todo lo de este documento se puede postergar sin penalización, y conviene saber cuál es la excepción. `Location` en `201` y `202`, `Allow` en `405`, `WWW-Authenticate` en `401` y `Retry-After` en `429` y `503` son cabeceras que el propio código de estado exige o presupone: emitir el código sin ellas es publicar un contrato incompleto, y agregarlas después no rompe pero tampoco llega a los clientes ya escritos.

Lo que sí se puede diferir es la negociación de contenido completa, la localización y los enlaces `Link`. La trampa que enuncia [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) para este escenario aplica de lleno: negociar tres formatos y cuatro idiomas en una API cuyo único consumidor pide JSON en español resuelve un problema inexistente.

### `ESC-2` — Exposición o migración

El sistema previo suele traer sus propias cabeceras, a veces con datos de negocio adentro, y la migración es la oportunidad de decidir cuáles sobreviven. La pregunta a hacerse por cada una es si lo que transporta pertenece al recurso —y entonces va al cuerpo— o es metadato del mensaje.

Aparece también el caso inverso: sistemas que devolvían todo por el cuerpo y ahora pueden aprovechar `Location`, `ETag` y `Link`. Migrar sin usarlas produce una API que es la anterior con sintaxis HTTP.

### `ESC-3` — Evolución en producción

Agregar una cabecera de respuesta es de los cambios más seguros que existen: un cliente que no la conoce la ignora. Quitarla o cambiar su semántica rompe a quien dependía de ella, y el problema es que esa dependencia suele no estar documentada porque la cabecera tampoco lo estaba.

Empezar a exigir una cabecera de petición es rompiente sin excepción. Hacer obligatorio `Idempotency-Key` en un endpoint que ya está en producción deja fuera a todos los clientes actuales, y la vía compatible es aceptarla, medir su adopción y exigirla recién en una versión nueva.

### `ESC-4` — Evaluación de una API ajena

Las cabeceras son la fuente de información más rica en `ESC-4b`, y por eso [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) las nombra explícitamente en la técnica de ese escenario. `Server` y las cabeceras propias delatan el marco de trabajo; `Cache-Control` y `ETag` revelan la estrategia de caché; la presencia de `Link` indica si hay navegación; una cabecera de límite de uso indica que hay *rate limiting* aunque no esté documentado; `Vary` dice qué negociación existe de verdad.

En `ESC-4a` el contraste típico es que la especificación OpenAPI no declara las cabeceras que el código emite. Es la divergencia menos vigilada de todas, porque los generadores de especificación desde anotaciones rara vez capturan cabeceras escritas a mano en el cuerpo de un método.

### Qué cambia según el contexto

| Contexto | Qué cambia respecto de las cabeceras |
|---|---|
| `CTX-1` pública | Toda cabecera emitida es contrato: alguien va a depender de ella. Conviene documentarlas todas y emitir las que los códigos exigen. La negociación de idioma en los mensajes de error tiene valor real acá y casi en ningún otro lado |
| `CTX-2` interna | Se agrega la propagación de contexto de traza, que es la preocupación propia del contexto según [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md). Es cabecera de infraestructura y debe atravesar todos los saltos sin que la lógica la toque |
| `CTX-3` backend de app propia | El cliente propio suele leer solo el cuerpo. Poner navegación únicamente en `Link` obliga a que el cliente lea cabeceras, y con clientes MAUI o Blazor eso agrega fricción real; es el caso donde la navegación en el cuerpo se justifica mejor |
| `CTX-4` integración | Las cabeceras las impone el proveedor: su nombre de clave de idempotencia, su formato de límite de uso, su versión. El trabajo es traducirlas en la capa de aislamiento y no dejarlas circular por el dominio propio |

---

## Ejemplos concretos

Sintéticos, del sistema de reserva de salas.

### Negociación completa en una respuesta

```http
GET /v1/sedes/norte/salas?limite=20 HTTP/1.1
Host: api.salas.ejemplo.com
Accept: application/json
Accept-Language: es-AR
Authorization: Bearer eyJhbGciOi…
```

```http
HTTP/1.1 200 OK
Content-Type: application/json
Content-Language: es-AR
Vary: Accept, Accept-Language, Authorization
Cache-Control: private, max-age=60
ETag: "7a10f2"
Link: </v1/sedes/norte/salas?limite=20&cursor=eyJ0Ijoi>; rel="next"

{ "datos": [ { "codigo": "SEDE-NORTE-201", "nombre": "Sala Norte 201", "capacidad": 12 } ] }
```

El `Vary` incluye `Authorization` porque el listado depende de los permisos del solicitante, y el `Cache-Control: private` impide que una caché compartida la almacene. Las dos cosas juntas son la protección mínima de una respuesta autenticada; el desarrollo es de [`TEM-CACHE`](Cache-y-Peticiones-Condicionales.md).

### Formato de parche no soportado

```http
PATCH /v1/salas/SEDE-NORTE-201 HTTP/1.1
Host: api.salas.ejemplo.com
Content-Type: application/json-patch+json

[ { "op": "replace", "path": "/capacidad", "value": 14 } ]
```

```http
HTTP/1.1 415 Unsupported Media Type
Accept-Patch: application/merge-patch+json
```

### Negociación proactiva en ASP.NET Core

```csharp
builder.Services.AddControllers(options =>
{
    // Sin esto, un Accept no satisfacible recibe JSON igual en lugar de 406.
    options.ReturnHttpNotAcceptable = true;
});
```

### Emisión de `Link` según `N-10`

```csharp
public static class LinkHeader
{
    // Construye la cabecera Link de N-10. Los nombres de relación provienen
    // del registro IANA (N-11): self y next no son de RFC 8288 sino de RFC 4287/HTML.
    public static void Escribir(HttpResponse respuesta, string self, string? siguiente, string? primero)
    {
        var partes = new List<string> { $"<{self}>; rel=\"self\"" };

        if (primero is not null)
            partes.Add($"<{primero}>; rel=\"first\"");

        // Se omite la relación next en la última página en lugar de emitirla vacía.
        if (siguiente is not null)
            partes.Add($"<{siguiente}>; rel=\"next\"");

        respuesta.Headers.Link = string.Join(", ", partes);
    }
}
```

### `Vary` y `Retry-After` desde un endpoint

```csharp
app.MapGet("/v1/sedes/{sedeId}/salas", async (
    string sedeId, ISalaService svc, HttpResponse resp, CancellationToken ct) =>
{
    resp.Headers.Vary = "Accept, Accept-Language, Authorization";
    resp.Headers.CacheControl = "private, max-age=60";
    var salas = await svc.ListarAsync(sedeId, ct);
    return Results.Ok(salas);
});

// 429 con Retry-After. El código lo define N-03 (RFC 6585), no N-01.
app.Use(async (ctx, next) =>
{
    if (LimitadorDeUso.Excedido(ctx, out var esperaSegundos))
    {
        ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.Response.Headers.RetryAfter = esperaSegundos.ToString();
        return;
    }
    await next(ctx);
});
```

### Cabecera propia declarada en OpenAPI

```csharp
app.MapPost("/v1/reservas", async (
    [FromHeader(Name = "Idempotency-Key")] string? claveIdempotencia,
    NuevaReserva cuerpo, IReservaService svc, CancellationToken ct) =>
{
    // Idempotency-Key es convención de facto (F-01 expiró; la sostiene P-04),
    // no un estándar IETF. Se documenta como tal en la especificación.
    var creada = await svc.CrearAsync(cuerpo, claveIdempotencia, ct);
    return Results.Created($"/v1/reservas/{creada.Id}", creada);
})
.WithOpenApi(op =>
{
    op.Parameters[0].Description =
        "Clave de idempotencia. Convención de facto, no estándar IETF: el draft expiró en 2025.";
    return op;
});
```

---

## Preguntas guía

- ¿Cada respuesta cuyo contenido depende de una cabecera de petición emite `Vary` con esa cabecera?
- ¿Qué hace mi API con un `Accept` que no puede satisfacer? ¿Es la conducta que decidí o la que trae el marco de trabajo por defecto?
- ¿Mis `201` traen `Location`? ¿Mis `202` traen a dónde consultar el resultado?
- ¿Emito `Retry-After` en todos los `429` y `503`? Si no, ¿qué espero que haga el cliente?
- ¿Cuántas cabeceras propias emite mi API, y cuántas de esas están en la especificación OpenAPI?
- Si uso `Link`, ¿los nombres de relación están en el registro `N-11` o los inventé?
- ¿Alguna cabecera propia transporta datos que deberían estar en el recurso?

---

## Criterios de calidad

Se reconoce una aplicación buena cuando las cabeceras que los códigos de estado exigen están siempre presentes, `Vary` acompaña a toda respuesta negociada, y el conjunto de cabeceras propias es pequeño, documentado y sin prefijo `X-`.

### Antipatrones

**`Vary` ausente en respuestas negociadas.** El defecto más silencioso del documento. No se manifiesta en desarrollo ni en pruebas, solo con una caché compartida de por medio, y entonces se manifiesta como un cliente que recibe contenido de otro.

**Datos de negocio en cabeceras propias.** El identificador del inquilino, el rol, el estado de la entidad. No aparecen en los ejemplos, no se validan con el esquema, no los ve nadie que lea la especificación, y crean un contrato paralelo no documentado.

**Cabeceras no declaradas en la especificación.** Toda cabecera que la API emite y OpenAPI no declara es una dependencia que algún consumidor va a adquirir sin garantía. Es la forma en que `ACT-03` se acopla a lo no garantizado y descubre la ruptura en producción.

**`X-` en cabeceras nuevas.** Marca como experimental algo que va a ser permanente, y obliga a sostener dos nombres si alguna vez se normaliza. El historial de los campos `RateLimit` es el caso testigo.

**`429` sin `Retry-After`.** Se trata en [`TEM-STATUS`](Codigos-de-Estado.md) y reaparece acá porque el defecto es de cabecera, no de código: el cliente reintenta de inmediato y empeora la condición.

**Presentar `Idempotency-Key` o los campos `RateLimit` como estándar.** El primero se apoya en un draft **expirado** (`F-01`); los segundos, en un draft activo que **cambió los nombres** respecto de todo lo que la industria usa (`F-02`). Usarlos está bien; el argumento de autoridad no.

**Mezclar `prev` y `previous`.** Ambos están registrados en `N-11` y el segundo es sinónimo del primero. Alternarlos dentro de la misma API produce clientes que reconocen la mitad de los enlaces.

**Enlaces de navegación duplicados e inconsistentes.** Emitir `Link` y además un objeto `links` en el cuerpo que no coincide. Cada uno por separado es defendible; los dos a la vez, divergiendo, no.

---

## Anexo — Inventario de cabeceras

Se completa una vez por API. La columna de fuente es la que obliga a distinguir lo normativo de lo convencional.

```yaml
api: ""

cabeceras_de_respuesta_normativas:
  - nombre: Location
    fuente: "N-01 §10.2.2"
    se_emite_en: [201, 202, 3xx]
    verificado: si | no
  - nombre: Allow
    fuente: "N-01 §15.5.6"
    se_emite_en: [405]
    verificado: si | no
  - nombre: WWW-Authenticate
    fuente: "N-01 §15.5.2"
    se_emite_en: [401]
    verificado: si | no
  - nombre: Retry-After
    fuente: "N-01 §10.2.3 (sección no verificada individualmente)"
    se_emite_en: [429, 503]
    verificado: si | no
  - nombre: Vary
    fuente: "N-01 §12"
    se_emite_en: "toda respuesta negociada"
    verificado: si | no
  - nombre: Accept-Patch
    fuente: "N-05"
    se_emite_en: [OPTIONS, 415]
    verificado: si | no
  - nombre: Link
    fuente: "N-10 (cabecera) + N-11 (nombres de relación)"
    relaciones_usadas: []
    todas_registradas_en_N11: si | no

cabeceras_propias:
  - nombre: ""
    proposito: ""
    nivel: "convencion-de-facto | criterio-propio"
    evidencia: ""                 # p. ej. "P-04" para Idempotency-Key
    prefijo_X: si | no            # "si" es deuda
    en_openapi: si | no
    transporta_datos_de_negocio: si | no   # "si" es defecto

negociacion:
  media_types_servidos: []
  rechaza_accept_no_satisfacible: si | no
  idiomas_soportados: []
  se_localizan: "solo mensajes de error | también datos"   # "también datos" exige justificación
```

El campo `todas_registradas_en_N11` es el que más veces se responde mal, porque la creencia extendida es que las relaciones las define `N-10`. Las define el registro, y cada una tiene su fuente propia.
