---
doc_id: ANEXO-REFERENCIAS
doc_type: anexo
title: Referencias y fuentes
status: vigente
origin: ia-assisted
confidence: alta
owner: "Guía de estudio — Organización y estilo de REST API en .NET"
last_review: 2026-07-20
audience: [humano, agente]
traces: [MARCO-CONVENCIONES]
---

# Referencias y fuentes — `ANEXO-REFERENCIAS`

## Resumen ejecutivo

Toda afirmación normativa de la guía se apoya en una entrada de este anexo, clasificada según el nivel de autoridad que fija [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md). La clasificación es el instrumento central de la guía: en diseño de APIs REST circula más prescripción sin fuente que en cualquier otro tema de esta serie, y una parte considerable de lo que se enuncia como «regla REST» no aparece en ninguna especificación, o aparece en un documento que otro reemplazó hace años.

Cada fila registra la designación exacta del documento, su estado formal y la fecha en que se verificó. Todas las URL de este anexo se verificaron el **2026-07-20** contra la fuente primaria. Lo que no pudo verificarse de primera mano está aislado en la sección final y no debe citarse.

---

## Los cinco niveles de identificador

`MARCO-CONVENCIONES` fija cuatro niveles de autoridad para las afirmaciones; este anexo los proyecta sobre cinco prefijos de identificador, porque la evidencia de plataformas cumple una función distinta de la de una guía corporativa aunque ambas sean no normativas.

| Prefijo | Qué agrupa | Fuerza de la cita |
|---------|------------|-------------------|
| `N-xx` | **Normativo.** RFC del IETF, OpenAPI Specification, estándares OASIS, documentación oficial de Microsoft Learn | Es el estándar. Se cita designación y sección exacta |
| `G-xx` | **Guía de organización.** Prescripciones de Microsoft, Google, Zalando, GOV.UK, adidas | Vale para quien adopta esa guía, no universalmente. Se nombra siempre la organización |
| `F-xx` | **Convención de facto.** Prácticas sin especificación vigente que las imponga, drafts IETF, specs comunitarias | No obliga. Requiere señalar la evidencia `P-xx` que la sostiene |
| `O-xx` | **Obras de referencia.** Disertaciones, artículos de autor identificable, papers, documentación conceptual | Origen verificable de un concepto, no fuente de autoridad |
| `P-xx` | **Evidencia de plataformas.** Documentación pública de APIs reales | Prueba de qué se hace, jamás de qué corresponde hacer |

La confusión entre `G` y `F` es la que más daño hace. Que Google prescriba `page_size` y `page_token` en `G-04` no convierte esos nombres en convención del ecosistema; los convierte en la convención de Google. Y la confusión entre `P` y cualquiera de los otros niveles produce el argumento circular más frecuente del tema: «Stripe lo hace así» describe una decisión de producto de una empresa, no una norma.

---

## 1. Fuentes normativas — `N-xx`

Especificaciones publicadas. El estado indicado es el que figura en el RFC Editor, en el sitio de la especificación o en el registro de OASIS.

### 1.1 Semántica y caché HTTP

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-01 | RFC 9110 | HTTP Semantics | **Internet Standard — STD 97** (junio 2022) | `https://www.rfc-editor.org/info/rfc9110` | 2026-07-20 |
| N-02 | RFC 9111 | HTTP Caching | **Internet Standard — STD 98** (junio 2022) | `https://www.rfc-editor.org/info/rfc9111` | 2026-07-20 |
| N-03 | RFC 6585 | Additional HTTP Status Codes | Proposed Standard (abril 2012), vigente | `https://www.rfc-editor.org/info/rfc6585` | 2026-07-20 |
| N-04 | RFC 9457 | Problem Details for HTTP APIs | Proposed Standard (julio 2023), obsoleta RFC 7807 | `https://www.rfc-editor.org/info/rfc9457` | 2026-07-20 |
| N-81 | RFC 6648 | Deprecating the "X-" Prefix and Similar Constructs in Application Protocols | **Best Current Practice — BCP 178** (junio 2012) | `https://www.rfc-editor.org/info/rfc6648` | 2026-07-20 |
| N-82 | RFC 7725 | An HTTP Status Code to Report Legal Obstacles | Proposed Standard (febrero 2016) | `https://www.rfc-editor.org/info/rfc7725` | 2026-07-20 |

`N-01` obsoleta RFC 2818, RFC 7231, RFC 7232, RFC 7233, RFC 7235, RFC 7538, RFC 7615, RFC 7694 y parcialmente RFC 7230. `N-02` obsoleta RFC 7234. Ambas concentran lo que antes estaba disperso en la serie 723x, y son la referencia única para métodos, códigos de estado, negociación de contenido, peticiones condicionales y caché.

`N-81` es la fuente normativa que respalda no usar cabeceras con prefijo `X-` en diseños nuevos. Su abstract es explícito: *«while in theory the "X-" convention was a good way to avoid collisions … in practice the benefits have been outweighed by the costs associated with the leakage of unstandardized parameters into the standards space»*. Su estado es **BCP 178**, no Informational, lo que le da más peso del que suele atribuírsele. Autores: P. Saint-Andre, D. Crocker y M. Nottingham.

`N-82` define el **451 Unavailable For Legal Reasons** y sugiere acompañarlo de una explicación en el cuerpo y de una cabecera `Link` con relación `blocked-by`. Antes figuraba entre las fuentes no verificadas de este anexo; se verificó de primera mano el 2026-07-20.

**Numeración de subsecciones de códigos de estado en `N-01`.** Para citar con la misma precisión que ya se usa con 3xx y 4xx:

| Código | Sección de `N-01` | Código | Sección de `N-01` |
|--------|-------------------|--------|-------------------|
| 200 OK | §15.3.1 | 500 Internal Server Error | §15.6.1 |
| 201 Created | §15.3.2 | 501 Not Implemented | §15.6.2 |
| 202 Accepted | §15.3.3 | 502 Bad Gateway | §15.6.3 |
| 203 Non-Authoritative Information | §15.3.4 | 503 Service Unavailable | §15.6.4 |
| 204 No Content | §15.3.5 | 504 Gateway Timeout | §15.6.5 |
| 205 Reset Content | §15.3.6 | 505 HTTP Version Not Supported | §15.6.6 |
| 206 Partial Content | §15.3.7 | | |

`N-01` **§10.2.3 es `Retry-After`**, verificada individualmente. Su ABNF es `Retry-After = HTTP-date / delay-seconds` y el texto la asocia a **503**, a las respuestas de **redirección 3xx** y a **429**.

### 1.2 Modificación parcial

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-05 | RFC 5789 | PATCH Method for HTTP | Proposed Standard (marzo 2010) | `https://www.rfc-editor.org/rfc/rfc5789.html` | 2026-07-20 |
| N-06 | RFC 6902 | JavaScript Object Notation (JSON) Patch | Proposed Standard (abril 2013) | `https://www.rfc-editor.org/rfc/rfc6902.html` | 2026-07-20 |
| N-07 | RFC 7396 | JSON Merge Patch | Proposed Standard (octubre 2014) | `https://www.rfc-editor.org/rfc/rfc7396.html` | 2026-07-20 |

### 1.3 URIs y enlaces

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-08 | RFC 3986 | Uniform Resource Identifier (URI): Generic Syntax | **Internet Standard — STD 66** (enero 2005) | `https://www.rfc-editor.org/rfc/rfc3986.html` | 2026-07-20 |
| N-09 | RFC 6570 | URI Template | Proposed Standard (marzo 2012) | `https://www.rfc-editor.org/info/rfc6570` | 2026-07-20 |
| N-10 | RFC 8288 | Web Linking | Proposed Standard (octubre 2017), obsoleta RFC 5988 | `https://www.rfc-editor.org/info/rfc8288` | 2026-07-20 |
| N-11 | IANA Link Relations Registry | Link Relation Types | Registro activo | `https://www.iana.org/assignments/link-relations/link-relations.xhtml` | 2026-07-20 |

`N-10` define la cabecera `Link` y el registro, no los nombres de relación. `self`, `next`, `prev` y `related` provienen de RFC 4287 y de la especificación HTML; solo `first` y `last` se registran con referencia a RFC 8288. Para relaciones concretas se cita `N-11`, no `N-10`.

### 1.4 Ciclo de vida y evolución

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-12 | RFC 9745 | The Deprecation HTTP Response Header Field | Proposed Standard (marzo 2025) | `https://www.rfc-editor.org/rfc/rfc9745.html` | 2026-07-20 |
| N-13 | RFC 8594 | The Sunset HTTP Header Field | **Informational** (mayo 2019) | `https://www.rfc-editor.org/rfc/rfc8594.html` | 2026-07-20 |
| N-14 | RFC 9413 | Maintaining Robust Protocols | Informational, stream IAB (junio 2023) | `https://www.rfc-editor.org/rfc/rfc9413.html` | 2026-07-20 |

### 1.5 Autenticación y autorización

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-15 | RFC 6749 | The OAuth 2.0 Authorization Framework | Proposed Standard (octubre 2012), actualizado por RFC 8252, 8996 y 9700 | `https://www.rfc-editor.org/info/rfc6749` | 2026-07-20 |
| N-16 | RFC 6750 | The OAuth 2.0 Authorization Framework: Bearer Token Usage | Proposed Standard (octubre 2012) | `https://www.rfc-editor.org/info/rfc6750` | 2026-07-20 |
| N-17 | RFC 7519 | JSON Web Token (JWT) | Proposed Standard (mayo 2015) | `https://www.rfc-editor.org/info/rfc7519` | 2026-07-20 |
| N-18 | RFC 9068 | JSON Web Token (JWT) Profile for OAuth 2.0 Access Tokens | Proposed Standard (2021) | `https://www.rfc-editor.org/info/rfc9068` | 2026-07-20 |

### 1.6 Descripción de contratos

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-19 | OAS 3.2.0 | OpenAPI Specification | Publicada 2025-09-19; última estable a 2026-07 | `https://spec.openapis.org/oas/v3.2.0.html` | 2026-07-20 |
| N-20 | Arazzo 1.1.0 | Arazzo Specification | Publicada 2026-05-17 | `https://spec.openapis.org/arazzo/latest.html` | 2026-07-20 |
| N-21 | OData v4.01 Part 1 | OData Version 4.01 Part 1: Protocol | **OASIS Standard** | `https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html` | 2026-07-20 |
| N-22 | GraphQL Specification, September 2025 edition | GraphQL | Edición vigente; primera desde October 2021 | `https://spec.graphql.org/September2025/` | 2026-07-20, texto leído |

`N-22` quedó **verificado sobre el texto de la especificación** en esta revisión, lo que corrige el registro anterior. El HTTP 403 que impidió la lectura era una limitación de la herramienta de fetch, no del sitio: `spec.graphql.org` responde 200 a una petición con User-Agent de navegador, y el fuente Markdown está además en el repositorio `graphql/graphql-spec`. Se leyeron la introducción, la Section 2 «Language», la Section 3 «Type System» y la Section 7 «Response». La licencia declarada es OWFa 1.0.

La gobernanza consta en la propia introducción de la especificación y no solo en `P-09`: la spec es *«a deliverable of the GraphQL Specification Project, established in 2019 with the Joint Development Foundation»*, con la GraphQL Foundation formada en 2019 como punto focal neutral, y las contribuciones gestionadas por el GraphQL Working Group bajo el Technical Steering Committee.

Una precisión de alcance que evita la cita incorrecta más frecuente sobre GraphQL: **`N-22` es agnóstica del transporte y no menciona HTTP en su Section 7**. Lo que fija ahí es la forma del cuerpo —que un *execution result* pueda contener `data` y `errors` a la vez—, no ningún código de estado. Todo lo relativo a HTTP pertenece a `F-12`, que es un draft.

### 1.7 .NET y ASP.NET Core — documentación oficial de Microsoft

Todas las páginas se consultaron con el moniker `aspnetcore-10.0` forzado en la URL. La advertencia es operativa y necesaria: el selector de versión de Microsoft Learn puede servir contenido de otra versión, de modo que una consulta sin `?view=aspnetcore-10.0` no es reproducible y no cuenta como verificación.

| ID | Documento | Qué fija | URL | Verificado |
|----|-----------|----------|-----|------------|
| N-23 | .NET and .NET Core Support Policy | Cadencia LTS/STS y fechas de fin de soporte | `https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core` | 2026-07-20 |
| N-24 | Create web APIs with ASP.NET Core | Minimal APIs recomendadas para proyectos nuevos; lista oficial de casos que piden controllers | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis` | 2026-07-20 |
| N-25 | Route handlers in Minimal API apps — Route groups | `MapGroup`, `RouteGroupBuilder`, anidamiento y orden de filtros | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers` | 2026-07-20 |
| N-26 | How to create responses in Minimal API apps | `TypedResults` preferido sobre `Results`; `Results<T1,Tn>`; defaults JSON de Minimal APIs | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses` | 2026-07-20 |
| N-27 | Filters in Minimal API apps | `IEndpointFilter`, orden FIFO/FILO, los filtros no se resuelven desde DI | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters` | 2026-07-20 |
| N-28 | Handle errors in ASP.NET Core web APIs | `AddProblemDetails`, `IProblemDetailsService`, `ProblemDetailsFactory`, `ClientErrorMapping` | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api` | 2026-07-20 |
| N-29 | Handle errors in ASP.NET Core | `IExceptionHandler`, `UseExceptionHandler`, `UseStatusCodePages`, `SuppressDiagnosticsCallback` | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling` | 2026-07-20 |
| N-30 | `ProblemDetails` Class | Propiedades, ensamblado y convertidor JSON del tipo | `https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails` | 2026-07-20 |
| N-31 | `IProblemDetailsService` Interface | `WriteAsync`/`TryWriteAsync`; media types soportados por el writer por defecto | `https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iproblemdetailsservice` | 2026-07-20 |
| N-32 | Generate OpenAPI documents | `AddOpenApi`/`MapOpenApi`, ruta `/openapi/{documentName}.json`, versión de OAS emitida, YAML, build time | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi` | 2026-07-20 |
| N-33 | Use the generated OpenAPI documents | ASP.NET Core no incluye UI; Swagger UI, Scalar y ReDoc solo en Development | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents` | 2026-07-20 |
| N-34 | Customize OpenAPI documents | Los tres transformers, su orden de ejecución y la política de `$ref` | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi` | 2026-07-20 |
| N-35 | Validation in ASP.NET Core | `Microsoft.Extensions.Validation`, `AddValidation()`, source generator, opt-outs y limitaciones | `https://learn.microsoft.com/en-us/aspnet/core/validation/overview` | 2026-07-20 |
| N-36 | Create web APIs with ASP.NET Core — `[ApiController]` | 400 automático por `ModelState`, `SuppressModelStateInvalidFilter` | `https://learn.microsoft.com/en-us/aspnet/core/web-api/` | 2026-07-20 |
| N-37 | Format response data in ASP.NET Core Web API | camelCase como formato por defecto de MVC; exención de `ProblemDetails` | `https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting` | 2026-07-20 |
| N-38 | How to customize property names and values with System.Text.Json | Sin ASP.NET Core los nombres quedan sin cambios, incluida la capitalización | `https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties` | 2026-07-20 |
| N-39 | `JsonSerializerDefaults` Enum | Las cuatro implicaciones de `Web`; miembro `Strict` | `https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializerdefaults` | 2026-07-20 |
| N-40 | How to use source generation in System.Text.Json | `JsonSerializerContext`, modos de generación, `TypeInfoResolverChain`, AOT/trimming | `https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation` | 2026-07-20 |
| N-41 | What's new in .NET 10 — Libraries | `AllowDuplicateProperties`, preset `Strict`, `PipeReader`, `ReferenceHandler` en source gen | `https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/libraries` | 2026-07-20 |
| N-42 | Rate limiting middleware in ASP.NET Core | Los cuatro algoritmos, `OnRejected`, orden respecto de `UseRouting` | `https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit` | 2026-07-20 |
| N-43 | `RateLimiterOptions.RejectionStatusCode` Property | El código de rechazo por defecto es 503 | `https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.ratelimiting.ratelimiteroptions.rejectionstatuscode` | 2026-07-20 |
| N-44 | Configure JWT bearer authentication in ASP.NET Core | `AddJwtBearer`, `TokenValidationParameters`, preferencia por los defaults | `https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication` | 2026-07-20 |
| N-45 | Generate tokens with `dotnet user-jwts` | Comandos, opciones, ubicación del almacén y claves de configuración escritas | `https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn` | 2026-07-20 |
| N-46 | Policy-based authorization in ASP.NET Core | Requirements con AND, handlers con OR, `InvokeHandlersAfterFailure` | `https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies` | 2026-07-20 |
| N-47 | Enable CORS in ASP.NET Core | Políticas, `RequireCors`, ubicación obligatoria entre `UseRouting` y los endpoints | `https://learn.microsoft.com/en-us/aspnet/core/security/cors` | 2026-07-20 |
| N-48 | ASP.NET Core Middleware | Orden obligatorio de `UseCors`, `UseAuthentication`, `UseAuthorization` y `UseResponseCaching` | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/` | 2026-07-20 |
| N-49 | What's new in ASP.NET Core 10.0 | 401/403 en vez de redirect en endpoints de API; `IApiEndpointMetadata`; passkeys; métricas | `https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0` | 2026-07-20 |
| N-50 | `IHttpClientFactory` in .NET | Los cuatro patrones de consumo, vida del handler de 2 minutos, restricciones de cookies y singletons | `https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory` | 2026-07-20 |
| N-51 | Build resilient HTTP apps | `AddStandardResilienceHandler`, las cinco estrategias y sus defaults, hedging | `https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience` | 2026-07-20 |
| N-52 | Introduction to resilience in .NET | Relación con Polly; deprecación de `Microsoft.Extensions.Http.Polly` | `https://learn.microsoft.com/en-us/dotnet/core/resilience/` | 2026-07-20 |
| N-53 | Call a web API from an ASP.NET Core Blazor app | Server-side sin `HttpClient` registrado; WebAssembly sobre Fetch API y CORS; trampa de prerendering | `https://learn.microsoft.com/en-us/aspnet/core/blazor/call-web-api` | 2026-07-20 |
| N-54 | Consume a local web service from a .NET MAUI app | `10.0.2.2` en Android, `localhost` en el simulador iOS, cleartext y ATS, certificado de desarrollo | `https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services` | 2026-07-20 |
| N-55 | Integration tests in ASP.NET Core | `WebApplicationFactory`, `TestServer`, `ConfigureTestServices`, orden de callbacks | `https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests` | 2026-07-20 |
| N-56 | `WebApplicationFactory<TEntryPoint>` Class | Miembros nuevos de .NET 10: `UseKestrel`, `StartServer`; `CreateServer` obsoleto | `https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1` | 2026-07-20 |
| N-57 | Use `.http` files in Visual Studio 2022 | Sintaxis, entornos, variables de request, y qué features son exclusivas de la extensión de VS Code | `https://learn.microsoft.com/en-us/aspnet/core/test/http-files` | 2026-07-20 |
| N-58 | `dotnet new` SDK templates reference | Opciones de `webapi` y `webapiaot`; `--use-minimal-apis` y `--use-controllers` | `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-sdk-templates` | 2026-07-20 |
| N-59 | Kiota overview | Generador oficial de clientes desde OpenAPI; qué distribuciones son oficiales y cuáles no | `https://learn.microsoft.com/en-us/openapi/kiota/overview` | 2026-07-20 |
| N-60 | API design best practices (Azure Architecture Center) | Los cinco enfoques de versionado; única guía de primera parte de Microsoft sobre el tema | `https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design` | 2026-07-20 |
| N-61 | `ConfigureHttpJsonOptions` Method | El `JsonOptions` de Minimal APIs y su propiedad `SerializerOptions` | `https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.httpjsonserviceextensions.configurehttpjsonoptions` | 2026-07-20 |
| N-83 | ASP.NET Core OpenAPI XML documentation comment support | El source generator de comentarios XML, `GenerateDocumentationFile`, tags soportados, `AdditionalFiles` y cómo desactivarlo | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/openapi-comments?view=aspnetcore-10.0` | 2026-07-20 |

Dos precisiones sobre el alcance de estas fuentes. `N-60` es la única guía de primera parte de Microsoft sobre versionado de APIs, y opera **en el nivel de diseño**: enumera cinco enfoques —sin versionado, URI, query string, header y media type— sin prescribir uno. ASP.NET Core no incluye versionado de APIs; la implementación es de terceros (`F-09`). Y `N-24` no contiene ninguna tabla comparativa entre Minimal APIs y controllers: su contenido es prosa y viñetas, de modo que cualquier «matriz de capacidades» que circule es material de comunidad, no documentación oficial.

### 1.8 Anuncios y seguimiento en repositorios de Microsoft

Estas entradas son **anuncios oficiales o seguimiento de trabajo del equipo de ASP.NET Core, no especificaciones**. Se citan para fechar una decisión o para acreditar la ausencia de una capacidad, nunca como prescripción. Un issue abierto es evidencia de que algo no está hecho; no es una promesa de que vaya a hacerse.

| ID | Fuente | Qué acredita | Estado | URL | Verificado |
|----|--------|--------------|--------|-----|------------|
| N-62 | `dotnet/aspnetcore` issue 54599 | Anuncio de la remoción de `Swashbuckle.AspNetCore` de la plantilla web API y de la extensión de `Microsoft.AspNetCore.OpenApi` | **Cerrado**; era un anuncio | `https://github.com/dotnet/aspnetcore/issues/54599` | 2026-07-20 |
| N-63 | `dotnet/aspnetcore` issue 52414 | *Support Latest ProblemDetails RFC (July 2023, RFC 9457)*: ASP.NET Core no adoptó formalmente RFC 9457 | **Abierto**, milestone Backlog, sin ramas ni PR | `https://github.com/dotnet/aspnetcore/issues/52414` | 2026-07-20 |
| N-64 | `dotnet/aspnetcore` PR 61463 | Introducción de `IOpenApiDocumentProvider` en .NET 10 | Integrado | `https://github.com/dotnet/aspnetcore/pull/61463` | 2026-07-20 |
| N-65 | `dotnet/aspnetcore` issue 67033 | Los nullable value types declarados como parámetros de Minimal API no se validan | Limitación documentada | `https://github.com/dotnet/aspnetcore/issues/67033` | 2026-07-20 |
| N-66 | Plantilla `WebApi-CSharp`, rama `release/10.0` | Contenido real de `dotnet new webapi` en .NET 10, leído en el código fuente y no en la documentación | Código fuente | `https://github.com/dotnet/aspnetcore/tree/release/10.0/src/ProjectTemplates/Web.ProjectTemplates/content/WebApi-CSharp` | 2026-07-20 |
| N-67 | .NET 11 Preview 6 (.NET Blog) | Señales de .NET 11 relevantes para APIs: OpenAPI 3.2 por defecto, validación asíncrona, protección CSRF automática | Preview, publicado 2026-07-14 | `https://devblogs.microsoft.com/dotnet/dotnet-11-preview-6/` | 2026-07-20 |
| N-68 | «API versioning in .NET 10 applications» (.NET Blog) | Patrón `NewVersionedApi(...).MapGroup(...).HasApiVersion(...)` y `WithDocumentPerVersion()` | Publicado 2026-04-28; autor invitado MVP en canal oficial | `https://devblogs.microsoft.com/dotnet/api-versioning-in-dotnet-10-applications/` | 2026-07-20 |
| N-69 | `dotnet/core` — release notes 11.0 | Calendario de previews de .NET 11 | Activo | `https://github.com/dotnet/core/blob/main/release-notes/11.0/README.md` | 2026-07-20 |

`N-66` merece un comentario de método. El contenido de la plantilla se verificó leyendo los archivos del repositorio, no la documentación que los describe. Es la única forma de responder con certeza qué genera `dotnet new webapi`, y es lo que permite afirmar que la plantilla de .NET 10 no incluye ninguna interfaz de usuario de OpenAPI. El `.csproj` es la excepción: no está en el repositorio porque se produce al construir el paquete de plantillas, y figura entre las fuentes no verificadas.

### 1.9 Versiones de .NET y diferencias de comportamiento

La versión de referencia para producción a esta fecha es **.NET 10 (LTS)**. .NET 8 y .NET 9 terminan soporte el mismo día, el 2026-11-10, a cuatro meses de la fecha de esta revisión, de modo que todo proyecto nuevo apunta a .NET 10. .NET 11 es STS, está en Preview 6 desde el 2026-07-14 y tiene GA prevista para noviembre de 2026: se cita como contraste, jamás como base de una recomendación de producción.

| Versión | Release | Tipo | Fin de soporte | Último patch verificado |
|---------|---------|------|----------------|-------------------------|
| .NET 10 | 2025-11-11 | **LTS** | 2028-11-10 | 10.0.10 (2026-07-14) |
| .NET 9 | 2024-11-12 | STS | 2026-11-10 | 9.0.18 (2026-07-14) |
| .NET 8 | 2023-11-14 | LTS | 2026-11-10 | 8.0.29 (2026-07-14) |
| .NET 11 | — | STS | — | Preview 6 (2026-07-14) |

Los documentos temáticos deben anclar a versión toda afirmación que aparezca en esta tabla, porque el comportamiento por defecto cambia entre releases y una afirmación sin versión envejece mal.

| Comportamiento | .NET 8 | .NET 9 | .NET 10 | .NET 11 (preview) | Fuente |
|----------------|--------|--------|---------|-------------------|--------|
| Versión de OpenAPI generada | — | 3.0 | **3.1** | 3.2 | `N-32`, `N-67` |
| Soporte OpenAPI integrado (`Microsoft.AspNetCore.OpenApi`) | No | Sí | Sí | Sí | `N-32` |
| Swashbuckle en la plantilla `webapi` | Sí | **Removido** | No | No | `N-62`, `N-66` |
| `--use-controllers` disponible en `dotnet new webapi` | Sí | Sí | Sí | Sí | `N-58` |
| Validación nativa de Minimal APIs (`Microsoft.Extensions.Validation`) | No | No | **Sí, síncrona** | Asíncrona | `N-35`, `N-67` |
| `IExceptionHandler` que devuelve `true` suprime logs y métricas | No | No | **Sí** | Sí | `N-29` |
| `WebApplicationFactory` con `UseKestrel`/`StartServer` e `IAsyncDisposable` | No | No | **Sí** | Sí | `N-56` |
| `JsonStringEnumMemberName` | No | **Sí** | Sí | Sí | `N-40` |
| `JsonSerializerOptions.Strict` y `AllowDuplicateProperties` | No | No | **Sí** | Sí | `N-41` |
| Endpoints de API protegidos por cookies devuelven 401/403 en vez de redirect | No | No | **Sí** | Sí | `N-49` |

### 1.10 Versiones de HTTP

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-70 | RFC 9113 | HTTP/2 | Proposed Standard (junio 2022); obsoleta RFC 7540 y RFC 8740 | `https://www.rfc-editor.org/rfc/rfc9113.html` | 2026-07-20 |
| N-71 | RFC 9114 | HTTP/3 | Proposed Standard (junio 2022) | `https://www.rfc-editor.org/rfc/rfc9114.html` | 2026-07-20 |

Estas dos entradas dan respaldo normativo a lo que hasta ahora la guía sostenía solo con `O-06`, que es documentación de MDN. `N-70` describe la multiplexación en su abstract —*«allowing multiple concurrent exchanges on the same connection»*— y la precisa en la introducción: *«Multiplexing of requests is achieved by having each HTTP request/response exchange associated with its own stream»*. `N-71` aporta el matiz que más se pierde, en su §1.1: bajo HTTP/2 *«a lost or reordered packet causes all active transactions to experience a stall regardless of whether that transaction was directly impacted by the lost packet»*, y HTTP/3 lo resuelve delegando en QUIC, que provee *«reliability at the stream level and congestion control across the entire connection»*.

Ninguno de los dos RFC dice nada sobre el costo de servidor por petición. Toda afirmación sobre esa dimensión es criterio propio y debe marcarse como tal.

### 1.11 Estilos de integración distintos de REST

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-72 | SOAP 1.2 Part 1 (Second Edition) | Messaging Framework | **W3C Recommendation**, 2007-04-27 | `https://www.w3.org/TR/soap12-part1/` | 2026-07-20 |
| N-73 | WSDL 2.0 Part 1 | Web Services Description Language Version 2.0 Part 1: Core Language | **W3C Recommendation**, 2007-06-26 | `https://www.w3.org/TR/wsdl20/` | 2026-07-20 |
| N-74 | gRPC — Core concepts | Core concepts, architecture and lifecycle | Documentación oficial del proyecto | `https://grpc.io/docs/what-is-grpc/core-concepts/` | 2026-07-20 |
| N-75 | `grpc/grpc` — PROTOCOL-HTTP2.md | gRPC over HTTP/2 | Especificación de protocolo, rama `master` | `https://github.com/grpc/grpc/blob/master/doc/PROTOCOL-HTTP2.md` | 2026-07-20 |
| N-76 | gRPC — Status codes | Status codes and their use in gRPC | Documentación oficial del proyecto | `https://grpc.io/docs/guides/status-codes/` | 2026-07-20 |
| N-77 | `grpc/grpc` — PROTOCOL-WEB.md | gRPC-Web | Especificación de protocolo, rama `master` | `https://github.com/grpc/grpc/blob/master/doc/PROTOCOL-WEB.md` | 2026-07-20 |
| N-78 | Compare gRPC services with HTTP APIs | Microsoft Learn, moniker `aspnetcore-10.0` | `ms.date` 2019-12-05; refresh de contenido 2024-07-31 | `https://learn.microsoft.com/en-us/aspnet/core/grpc/comparison` | 2026-07-20 |
| N-79 | .NET Framework technologies unavailable on .NET 6+ | Microsoft Learn | Vigente | `https://learn.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable` | 2026-07-20 |
| N-80 | IANA Media Types Registry — `application` | Media Types | Registro activo | `https://www.iana.org/assignments/media-types/application.csv` | 2026-07-20 |

Cuatro precisiones que estas entradas permiten hacer y que el material corriente falla.

**El estado de SOAP no es el que se supone.** `N-72` no lleva banner de obsoleto: es una Recommendation formalmente activa, descrita como *«a stable document [that] may be used as reference material»*. Lo que sí terminó es su mantenimiento: el **XML Protocol Working Group cerró el 2009-07-10**, de modo que hace unos diecisiete años que ningún grupo tiene mandato para revisarla. La formulación correcta es «vigente pero sin grupo que la mantenga», no «obsoleta» ni «retirada». La distinción respecto de WSDL importa: `N-73` es Recommendation, mientras **WSDL 1.1 es una W3C Note** (`F-13`) que en su propia sección de estado declara *«Publication of this Note by W3C indicates no endorsement by W3C»*. La versión que la industria adoptó es la que no tiene estatus.

**Los códigos de gRPC no son códigos HTTP, y la fuente para decirlo no es la obvia.** `N-76` enumera los diecisiete códigos, de `OK` (0) a `UNAUTHENTICATED` (16), pero **no contiene ninguna frase que los contraste con los códigos HTTP**. La evidencia primaria de que son capas separadas está en `N-75`, que establece que la respuesta gRPC usa HTTP 200 con independencia del resultado del RPC y transporta el resultado real en un *trailer* `grpc-status`, obligatorio incluso cuando el código es `OK`. Para ese punto se cita `N-75`, no `N-76`.

**La razón de existir de gRPC-Web está en la especificación, no en el README.** `N-77` es explícito —*«Due to browser limitation, the Web client library implements a different protocol than the native gRPC protocol»*— con el objetivo declarado de desacoplarse del framing de HTTP/2, *«which is not, and will never be, directly exposed by browsers»*. `N-78` lo corrobora desde Microsoft: *«browsers do not allow a caller to require that HTTP/2 be used, or provide access to underlying HTTP/2 frames»*.

**`N-78` tiene dos matices de uso.** Su fecha de contenido es vieja, de modo que es una página estable y no una revisión reciente; y menciona **gRPC JSON transcoding** (.NET 7 en adelante) como segunda vía de compatibilidad con navegador junto a gRPC-Web, algo que los resúmenes de esa página suelen omitir.

---

## 2. Guías de organización — `G-xx`

Prescripciones de una organización concreta. Ninguna es normativa fuera de su ámbito, y entre ellas se contradicen en casi todas las decisiones de estilo.

| ID | Guía | Organización | Versión o fecha | Estado | URL | Verificado |
|----|------|--------------|-----------------|--------|-----|------------|
| G-01 | Azure REST API Guidelines | Microsoft (Azure) | Documento fechado 2025-03-28 | Activo | `https://github.com/microsoft/api-guidelines/blob/vNext/azure/Guidelines.md` | 2026-07-20 |
| G-02 | Microsoft Graph REST API Guidelines | Microsoft (Graph) | Sin fecha en el documento | Activo | `https://github.com/microsoft/api-guidelines/blob/vNext/graph/GuidelinesGraph.md` | 2026-07-20 |
| G-03 | Microsoft REST API Guidelines (documento original) | Microsoft | — | **Deprecado**; remoción declarada 2024-07-01 | `https://github.com/microsoft/api-guidelines/blob/vNext/graph/Guidelines-deprecated.md` | 2026-07-20 |
| G-04 | API Improvement Proposals (AIP) | Google | Corpus vivo; changelog con cambios hasta enero 2025 | Activo | `https://google.aip.dev` | 2026-07-20 |
| G-05 | RESTful API Guidelines | Zalando SE | Sin fecha ni versión en la página | Activo | `https://opensource.zalando.com/restful-api-guidelines` | 2026-07-20 |
| G-06 | API technical and data standards | UK Government Digital Service | Actualizada 2024-07-19 | Activo | `https://www.gov.uk/guidance/gds-api-technical-and-data-standards` | 2026-07-20 |
| G-07 | adidas API Guidelines | adidas | Revisión indicada: febrero 2025; 730 commits | Activo | `https://github.com/adidas/api-guidelines` | 2026-07-20, solo estado |
| G-08 | HTTP API Design Guide | Heroku / interagent | 132 commits, sin actividad reciente | Activo pero **inactivo de facto** | `https://github.com/interagent/http-api-design` | 2026-07-20 |

### AIPs de `G-04` verificadas individualmente

El corpus de Google se cita como `G-04` seguido del número de AIP y, cuando corresponde, de la regla: «`G-04` AIP-158». Las verificadas de primera mano son AIP-121 (diseño orientado a recursos), AIP-122 (nombres de recursos), AIP-131 a AIP-135 (los cinco métodos estándar), AIP-140 (nombres de campos), AIP-136 (métodos personalizados), AIP-140 (nombres de campos), AIP-158 (paginación), AIP-160 (filtrado) y AIP-185 (versionado). Toda otra AIP citada en un documento temático debe verificarse antes de usarse como respaldo.

De **AIP-136 «Custom methods»** se verificó el título exacto y las tres reglas que la guía necesita citar. La condición de uso: *«Custom methods **should** only be used for functionality that can not be easily expressed via standard methods; prefer standard methods if possible.»* El método HTTP: *«The HTTP method **must** be `GET` or `POST`»*. Y la sintaxis, que es lo que suele citarse mal: *«The HTTP URI **must** use a `:` character followed by the custom verb (`:archive` in the above example), and the verb in the URI **must** match the verb in the name of the RPC.»* El ejemplo literal del documento es `post: "/v1/{name=publishers/*/books/*}:archive"`. Se confirmó también la numeración de los cinco métodos estándar: AIP-131 *Get*, AIP-132 *List*, AIP-133 *Create*, AIP-134 *Update*, AIP-135 *Delete*.

### Precisiones sobre las guías de Microsoft

`G-03` es el documento que internet cita de forma ubicua como «Microsoft REST API Guidelines» y ya no es normativo. Lleva el aviso de estar *being deprecated and merged with the Microsoft Graph REST API Guidelines, with a removal date of July 1, 2024*. Se conserva un identificador propio precisamente para poder señalar que una cita apunta ahí.

El repositorio `microsoft/api-guidelines` sí sigue activo —949 commits en la rama `vNext`, 39 pull requests abiertos, 23,3k estrellas, sin aviso de archivo—, pero se bifurcó en dos documentos vivos y mutuamente divergentes, `G-01` y `G-02`. Su README los describe como *companion documents* sin declarar cuál prevalece. Esa divergencia es interna y sustantiva: `G-01` usa `skip`/`top` sin prefijo y versión por fecha en query, `G-02` usa `$skip`/`$top` de OData y versión en el path. Citar «Microsoft prescribe X» sin decir cuál de las dos guías es, en general, incorrecto.

### Precisión sobre `G-07`

De adidas se verificó la existencia, la actividad del repositorio, el uso de keywords RFC 2119 y la presencia de tooling de enforcement (`adidas-spectral.yaml`). Sus prescripciones REST concretas —casing, versionado, paginación, formato de error— **no se verificaron** y figuran en la sección final.

---

## 3. Convenciones de facto — `F-xx`

Prácticas y documentos sin especificación vigente que las imponga. Se citan como convención, nunca como norma, y siempre acompañadas de la evidencia `P-xx` que las sostiene.

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| F-01 | draft-ietf-httpapi-idempotency-key-header-07 | The Idempotency-Key HTTP Header Field | **Internet-Draft EXPIRADO** (rev -07, 2025-10-15); nunca fue RFC | `https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/` | 2026-07-20 |
| F-02 | draft-ietf-httpapi-ratelimit-headers-11 | RateLimit header fields for HTTP | **Internet-Draft activo** (2026-05-23), expira 2026-11-24 | `https://datatracker.ietf.org/doc/html/draft-ietf-httpapi-ratelimit-headers` | 2026-07-20 |
| F-03 | draft-ietf-oauth-v2-1-15 | The OAuth 2.1 Authorization Framework | **Internet-Draft activo** (2026-03-02); no es RFC | `https://datatracker.ietf.org/doc/draft-ietf-oauth-v2-1/` | 2026-07-20 |
| F-04 | JSON:API v1.1 | JSON:API — A specification for building APIs in JSON | Estable; finalizada 2022-09-30 | `https://jsonapi.org/format/` | 2026-07-20 |
| F-05 | OData v4.02 | OData Version 4.02 | **Committee Specification Draft 02**; no es OASIS Standard | `https://docs.oasis-open.org/odata/` | 2026-07-20 |
| F-06 | OpenAPI 4.0 «Moonwalk» | — | **En diseño; no publicada.** Sin tooling de producción | `https://github.com/OAI/sig-moonwalk` | 2026-07-20 |
| F-12 | GraphQL over HTTP | GraphQL over HTTP Specification | **Stage 2: Draft**; no aprobada | `https://github.com/graphql/graphql-over-http` | 2026-07-20 |
| F-13 | WSDL 1.1 | Web Services Description Language 1.1 | **W3C Note** (2001-03-15); sin estatus de estándar | `https://www.w3.org/TR/2001/NOTE-wsdl-20010315` | 2026-07-20 |
| F-14 | AsyncAPI 3.1.0 | AsyncAPI Specification | Vigente, publicada 2026-01-31; Linux Foundation | `https://www.asyncapi.com/docs/reference/specification/latest` | 2026-07-20 |
| F-15 | CloudEvents v1.0.2 | CloudEvents Specification | Estable desde 2022-02-06; CNCF **graduated** (2024-01-25) | `https://github.com/cloudevents/spec` | 2026-07-20 |
| F-16 | Standard Webhooks 1.0.0 | Standard Webhooks | Iniciativa comunitaria; **ningún organismo de estándares** | `https://www.standardwebhooks.com` | 2026-07-20 |
| F-17 | draft-kelly-json-hal-11 | JSON Hypertext Application Language (HAL) | **Internet-Draft EXPIRADO** el 2024-04-21; nunca fue RFC | `https://datatracker.ietf.org/doc/draft-kelly-json-hal/` | 2026-07-20 |
| F-18 | Siren | Siren: a hypermedia specification for representing entities | Repo personal; contenido congelado el 2017-12-07 | `https://github.com/kevinswiber/siren` | 2026-07-20 |
| F-19 | Ion (hipermedia) | The Ion Hypermedia Type | **Nunca enviado al IETF**; muerto desde 2018-05-29 | `https://ionspec.org/` | 2026-07-20 |
| F-20 | Hydra Core Vocabulary | Hydra Core Vocabulary | **W3C Community Group Report**, no es estándar W3C; **CG cerrado el 2026-05-21** | `https://www.w3.org/groups/cg/hydra/` | 2026-07-20 |
| F-21 | tRPC | tRPC — End-to-end typesafe APIs | **11.18.0**, publicada 2026-06-17; proyecto activo | `https://trpc.io/docs` | 2026-07-20 |

`F-04` es una especificación comunitaria completa y estable, no un draft, pero ninguna organización de estándares la respalda y su media type `application/vnd.api+json` es lo único registrado ante IANA. Se clasifica aquí por su fuerza normativa, no por su calidad.

`F-12` merece una advertencia porque de él depende la afirmación sobre errores de GraphQL que más circula. Su propio README declara: *«This spec is in the draft stage of development, and can change before reaching `Accepted` stage.»* Y su contenido **contradice la creencia habitual**: para una respuesta con `data` y `errors` a la vez no recomienda `200` sino un código propio, *«If the GraphQL response contains both the {data} entry (even if it is {null}) and the {errors} entry, then the server SHOULD reply with a `294` status code»*. La sección «Partial success» reconoce que ese código lo inventó el proyecto: *«Since we are defining the code ourselves, rather than the IETF, we only recommend its usage alongside the `application/graphql-response+json` media type.»* El `200` ubicuo proviene de la ruta de compatibilidad que `F-12` define para el *legacy client*, esto es, el cliente que solo entiende `application/json`. Quien afirme «la especificación de GraphQL manda responder 200 con errores en el cuerpo» está citando mal dos documentos a la vez.

`F-16` se autodescribe como *«an open source and community-driven set of tools and guidelines»* y su encuadre es abiertamente aspiracional: *«We believe "Standard Webhooks" can do for webhooks what JWT did for API authentication.»* Lo respalda un comité técnico con Zapier, Twilio, ngrok, Supabase, Svix y Kong, lo cual no es un organismo de normalización. Dos trampas de citación verificadas: los tags de release del repositorio (v1.0.0 a v1.0.2) corresponden a las **bibliotecas cliente y no a la especificación** —la nota de la v1.0.0 lo dice—, y las adopciones que el proyecto se atribuye figuran entre las fuentes no verificadas de este anexo.

### Los cuatro formatos de hipermedia abandonados

`F-17` a `F-20` se registran para poder afirmar su estado, no para recomendarlos. Ninguno fue adoptado jamás por un working group del IETF ni por un Working Group del W3C: toda su huella institucional es un Community Group cerrado y un Internet-Draft individual expirado.

El registro IANA (`N-80`) se consultó descargando el CSV completo de la rama `application` y buscando sobre el archivo. El resultado es exacto: están registrados `application/vnd.hal+json`, `application/vnd.hal+xml` (a nombre de Mike Kelly) y `application/vnd.siren+json` (a nombre de Kevin Swiber), los tres en el **vendor tree**, que es la categoría más débil del registro porque no exige revisión ni organismo detrás. No hay ninguna coincidencia para `hal+json`, `hal+xml` ni `ion+json`.

De ahí sale el dato más útil de los cuatro: **HAL emite en la práctica `application/hal+json`, que no está registrado**, mientras el tipo registrado es `application/vnd.hal+json`. Lleva más de una década sirviendo un media type sin registrar, y la sección 10 de `F-17` dice íntegramente *«No IANA actions required»*, o sea que el draft nunca intentó registrarlo. En Siren, en cambio, especificación y registro coinciden. Ambas plantillas de registro citan RFC 4627, obsoleto desde 2014, y no se tocan desde 2011 y 2012 respectivamente.

Sobre `F-20` conviene ser literal, porque es el que más se cita como si fuera estándar del W3C. Su propia sección de estado dice: *«This specification was published by the Hydra W3C Community Group. It is not a W3C Standard nor is it on the W3C Standards Track.»* El W3C confirma la regla general de que *«Community Groups do not create W3C standards»*. El documento se renderiza con marca de agua «UNOFFICIAL DRAFT», no tiene media type propio —se consume como JSON-LD vía `application/ld+json`— y el grupo que lo amparaba cerró el 2026-05-21 tras un archivo de noticias que contiene dos entradas, de 2013 y 2014.

Y una distinción obligatoria: **`F-19` no tiene ninguna relación con Amazon Ion**, que es un formato de serialización jerárquico y autodescriptivo de Amazon, superconjunto de JSON, con codificaciones texto y binaria, destinado a serializar datos y no a expresar afordances hipermedia. Comparten la palabra «Ion» y nada más. `F-19`, pese a estar formateado como Internet-Draft y referirse a un «Ion Working Group», **nunca se envió al IETF**: las consultas a la API del datatracker por `ionwg`, `ion-hypermedia`, `hazlewood` e `ion+json` devuelven todas cero resultados, y ese «working group» es un nombre dentro de un documento.

### Paquetes del ecosistema .NET que no son de Microsoft

Todo lo que sigue son productos de terceros o de comunidad. Su presencia en la documentación de Microsoft Learn los hace opciones documentadas, no prescripciones, y ninguno forma parte del framework compartido.

| ID | Paquete | Mantenedor | Versión y fecha | Estado | URL | Verificado |
|----|---------|------------|-----------------|--------|-----|------------|
| F-07 | `Swashbuckle.AspNetCore` | `domaindrivendev` | **10.2.3**, 2026-06-22; 1.100 M de descargas | **Activo**, no deprecado; soporta `net10.0` | `https://www.nuget.org/packages/Swashbuckle.AspNetCore/` | 2026-07-20 |
| F-08 | `Scalar.AspNetCore` | Scalar (terceros) | **2.16.15**, 2026-07-16; 33,5 M de descargas | **Activo**; MIT; targets `net8.0`, `net9.0` y `net10.0`; documentado por Microsoft en `N-33` como opción de UI | `https://www.nuget.org/packages/Scalar.AspNetCore` | 2026-07-20 |
| F-09 | Familia `Asp.Versioning` | `API-Versioning-Team`, bajo tutela de la .NET Foundation | **10.0.0**, 2026-04-21, MIT, target `net10.0` | **Activo**; 40 releases | `https://github.com/dotnet/aspnet-api-versioning` | 2026-07-20 |
| F-10 | `Microsoft.AspNetCore.Mvc.Versioning` | Mismo proyecto, nombre anterior | 5.1.0, 2023-05-23; 270,6 M de descargas | **Deprecado en NuGet** desde agosto de 2022; solo correcciones críticas | `https://github.com/dotnet/aspnet-api-versioning/discussions/807` | 2026-07-20 |
| F-11 | `FluentValidation` | Jeremy Skinner | **12.1.1**, 2025-12-03; 987,1 M de descargas | **Activo**, licencia **Apache-2.0** sin cambios | `https://github.com/FluentValidation/FluentValidation` | 2026-07-20 |
| F-22 | `Grpc.AspNetCore` | Proyecto gRPC, con participación de Microsoft | **2.80.0**, 2026-04-30; 105 versiones | **Activo** | `https://www.nuget.org/packages/Grpc.AspNetCore` | 2026-07-20 |
| F-23 | `CoreWCF` | .NET Foundation | **1.9.1**, 2026-06-16, MIT | **Activo**; port del lado servidor de WCF | `https://github.com/CoreWCF/CoreWCF` | 2026-07-20 |

`F-23` resuelve la pregunta de si WCF existe en .NET moderno, y la respuesta tiene dos partes. `N-79` lista WCF entre las tecnologías de .NET Framework **no disponibles** en .NET 6+, con esta nota literal: *«Windows Communication Foundation (WCF) server can be used in .NET 6+ by using the CoreWCF NuGet packages.»* CoreWCF se describe a sí mismo como *«a port of the service side of Windows Communication Foundation (WCF) to .NET Core»* y *«supported by the .NET Foundation»*. Microsoft anunció que *«Microsoft Product Support will be available for CoreWCF customers»* y lo encuadra como vía de modernización para aplicaciones WCF existentes, recomendando para proyectos nuevos *«consider more modern alternatives to SOAP such as gRPC»*. El encuadre correcto es entonces: proyecto de la .NET Foundation con contribución y soporte comercial de Microsoft, **no un producto propiedad de Microsoft**. El README no reclama respaldo de Microsoft; eso proviene solo del blog.
| F-24 | `NSwag.AspNetCore` | Rico Suter (`RicoSuter`) | **14.7.1**, 2026-04-20; 130,7 M de descargas | **Activo**; sin aviso de archivo ni de deprecación. Targets `net8.0`, `netstandard2.0` y `net462`; **no declara un TFM `net10.0` propio**, se consume desde net10.0 por compatibilidad computada | `https://www.nuget.org/packages/NSwag.AspNetCore` | 2026-07-20 |

Los paquetes concretos de `F-09` verificados en nuget.org, todos en 10.0.0 y publicados el 2026-04-21, son `Asp.Versioning.Http`, `Asp.Versioning.Mvc`, `Asp.Versioning.Mvc.ApiExplorer` y `Asp.Versioning.Http.Client`. La cadena de dependencias va de `Asp.Versioning.Mvc` a `Asp.Versioning.Http` y de ahí a `Asp.Versioning.Abstractions`. Existen además `Asp.Versioning.OpenApi`, `.OData`, `.OData.ApiExplorer` y `.WebApi`.

De `F-08` ya se verificaron versión, fecha, licencia y targets en nuget.org, además de su mención en `N-33`: es la fila que más rápido envejece de esta tabla, porque el proyecto publica varias versiones por mes. `F-11` está clasificado como convención de facto y no como guía porque **FluentValidation no aparece recomendado en ninguna guía de primera parte de Microsoft**: las opciones documentadas son `Microsoft.Extensions.Validation` (`N-35`) y los endpoint filters (`N-27`).

---

## 4. Obras de referencia — `O-xx`

Origen verificable de conceptos que la guía usa. Ninguna es normativa.

| ID | Obra | Autoría y datos | Aporta | URL | Verificado |
|----|------|-----------------|--------|-----|------------|
| O-01 | *Architectural Styles and the Design of Network-based Software Architectures*, cap. 5 | Fielding, R. T. — disertación doctoral, UC Irvine, 2000 | Definición del estilo REST y sus seis restricciones | `https://ics.uci.edu/~fielding/pubs/dissertation/rest_arch_style.htm` | 2026-07-20 |
| O-02 | «REST APIs must be hypertext-driven» | Fielding, R. T. — entrada de blog, 2008-10-20 | La condición de hipertexto; control del servidor sobre su namespace | `https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven` | 2026-07-20 |
| O-03 | «Richardson Maturity Model» | Fowler, M. — artículo, 2010-03-18, sobre la charla de Leonard Richardson en QCon | Los cuatro niveles 0–3 de madurez | `https://martinfowler.com/articles/richardsonMaturityModel.html` | 2026-07-20 |
| O-04 | «An Analysis of Public REST Web Service APIs» | Neumann, A.; Laranjeiro, N.; Bernardino, J. — IEEE Transactions on Services Computing 14(4):957–970 | Evidencia cuantitativa: 4,2 % de 500 APIs públicas cumplen HATEOAS | `https://eden.dei.uc.pt/~cnl/selected-research/2018-tsc-rest.pdf` | 2026-07-20, indirecto |
| O-05 | «We need tool support for keyset pagination» («No Offset») | Winand, M. — publicado 2014-08-06, actualizado 2023-09-08 | Costo de `OFFSET`, *drifting results*, paginación por keyset | `https://use-the-index-luke.com/no-offset` | 2026-07-20 |
| O-06 | Evolution of HTTP | MDN Web Docs | Multiplexado de HTTP/2; eliminación del bloqueo de cabecera de línea de transporte en HTTP/3 | `https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/Evolution_of_HTTP` | 2026-07-20 |
| O-07 | Página de proyecto de gRPC | Cloud Native Computing Foundation | Nivel de madurez de gRPC en la CNCF | `https://www.cncf.io/projects/grpc/` | 2026-07-20 |

`O-07` acredita un dato que se enuncia mal con frecuencia: *«gRPC was accepted to CNCF on February 16, 2017 at the Incubating maturity level.»* gRPC sigue en **incubating**, no en graduated, y `N-78` lo confirma desde Microsoft —*«CNCF considers gRPC an incubating project»*—. El contraste con `F-15` es instructivo: CloudEvents, mucho menos conocido, sí alcanzó el nivel graduated el 2024-01-25. El nivel de madurez en una fundación no mide adopción.

Dos precisiones sobre `O-01` y `O-02` que la guía sostiene de forma literal. En la disertación las restricciones son seis —cliente-servidor, sin estado, caché, interfaz uniforme, sistema por capas y código bajo demanda—, y la última se declara explícitamente opcional; la fórmula «cinco más una» es correcta y la de «seis obligatorias» no. La frase famosa *«if the engine of application state … is not being driven by hypertext, then it cannot be a REST API. Period»* pertenece a `O-02`, no a `O-01`; la disertación dice *hypermedia as the engine of application state*. Y en `O-02` Fielding **no discute el versionado en URI**: la objeción al `/v1/` es una derivación del principio de que el cliente no debe construir URIs, no una condena textual.

Sobre `O-04`: se verificaron autores, publicación, metodología y la cifra del 4,2 %, pero no se abrió el PDF completo. El conjunto de datos es de alrededor de 2018; es la mejor evidencia cuantitativa disponible y no es actual.

---

## 5. Evidencia de plataformas — `P-xx`

Documentación pública de APIs en producción. Prueba qué hace la industria, jamás qué corresponde hacer.

| ID | Plataforma y documento | Qué evidencia | URL | Verificado |
|----|------------------------|---------------|-----|------------|
| P-01 | Stripe — API versioning | Versionado por fecha con nombre de release (`2026-06-24.dahlia`), header `Stripe-Version`, cuenta pinneada | `https://docs.stripe.com/api/versioning` | 2026-07-20 |
| P-02 | Stripe — API v2 overview | El namespace `/v2` convive con `/v1`; no es versión sucesora | `https://docs.stripe.com/api-v2-overview` | 2026-07-20 |
| P-03 | Stripe — Pagination | Cursor con `limit`, `starting_after`, `ending_before`, `has_more` | `https://docs.stripe.com/api/pagination` | 2026-07-20 |
| P-04 | Stripe — Errors | Cuatro tipos de error, `402 Request Failed`, `409` por reutilización de clave de idempotencia | `https://docs.stripe.com/api/errors` | 2026-07-20 |
| P-05 | GitHub — API versions | Header `X-GitHub-Api-Version`, default `2022-11-28`, soporte de 24 meses | `https://docs.github.com/en/rest/about-the-rest-api/api-versions` | 2026-07-20 |
| P-06 | GitHub — Pagination | `page`/`per_page` y cursores por endpoint; header `link` de RFC 8288 | `https://docs.github.com/en/rest/using-the-rest-api/using-pagination-in-the-rest-api` | 2026-07-20 |
| P-07 | Shopify — API versioning | Fecha en la URL (`2026-04`), release trimestral, ≥12 meses de soporte y ≥9 de solapamiento | `https://shopify.dev/docs/api/usage/versioning` | 2026-07-20 |
| P-08 | Twilio — Requests to Twilio | Base URL `https://api.twilio.com/2010-04-01`: fecha congelada hace ~16 años | `https://www.twilio.com/docs/usage/requests-to-twilio` | 2026-07-20 |
| P-09 | GraphQL — Governance | Gobernanza bajo Linux Foundation / Joint Development Foundation desde 2019; TSC aprueba releases | `https://graphql.org/community/contribute/governance/` | 2026-07-20 |

`P-04` sostiene `F-01`: la conducta de Stripe ante una clave de idempotencia reutilizada —comparar endpoint y parámetros, no solo la clave— coincide con el *idempotency fingerprint* que describía el draft expirado. La práctica precedió a la especificación y le sobrevivió.

---

## 6. Documentos obsoletos que se siguen citando

Es el error de citación más frecuente del tema y el más fácil de detectar: basta mirar el número. Un documento sobre APIs REST que cite cualquiera de estas cuatro fuentes está desactualizado en ese punto, con independencia de su fecha de publicación.

| Documento citado | Estado real | Reemplazado por | Consecuencia de citarlo |
|------------------|-------------|-----------------|-------------------------|
| **RFC 2616** — Hypertext Transfer Protocol HTTP/1.1 | Obsoleto desde 2014 | La serie 723x, a su vez reemplazada por `N-01` y `N-02` | Dos generaciones de retraso. Los nombres de códigos, la definición de idempotencia y el modelo de caché cambiaron |
| **RFC 7231** — HTTP/1.1: Semantics and Content | Obsoleto desde junio de 2022 | `N-01` (RFC 9110) | Cita de semántica HTTP a un documento retirado. Renombres perdidos: 413 pasó de *Request Entity Too Large* a **Content Too Large**, 414 de *Request-URI Too Long* a **URI Too Long**, 422 de *Unprocessable Entity* a **Unprocessable Content** |
| **RFC 7807** — Problem Details for HTTP APIs | Obsoleto desde julio de 2023 | `N-04` (RFC 9457) | Se pierde el registro *HTTP Problem Types* y la guía sobre múltiples problemas y URIs `type` no dereferenciables. Nótese que `G-05` regla 176 sigue citando 7807 |
| **Microsoft REST API Guidelines** (documento monolítico) | Deprecado, con fecha de remoción declarada 2024-07-01 | `G-01` para Azure y `G-02` para Graph, que **se contradicen entre sí** | La cita apunta a un documento que ya no se mantiene y oculta que Microsoft hoy no tiene una postura única |

El ecosistema .NET tiene dos análogos exactos, en forma de paquetes en lugar de documentos. **`Microsoft.AspNetCore.Mvc.Versioning`** (`F-10`) está formalmente deprecado en NuGet desde agosto de 2022: el proyecto se renombró en su versión 6.0 y continúa como `Asp.Versioning` (`F-09`). Su última versión es 5.1.0, de mayo de 2023, y acumula 270,6 millones de descargas, lo que explica cuánto material sigue enseñándolo. Y **`Microsoft.Extensions.Http.Polly`** está deprecado según `N-52`, que remite a `Microsoft.Extensions.Resilience` o `Microsoft.Extensions.Http.Resilience`; el matiz es que sigue publicándose y recibiendo servicing, no está retirado ni sin listar, de modo que un proyecto que lo use no se rompe pero sí está sobre un camino cerrado.

Un quinto caso merece mención aunque no admita reemplazo. **PayPal API Standards** se cita con frecuencia como guía viva: `github.com/paypal/api-standards` devuelve HTTP 404 y el repositorio fue retirado. Circulan forks y archivos de terceros que no son fuentes oficiales. PayPal mantiene `paypal/paypal-rest-api-specifications`, que contiene archivos de especificación y no una guía de estilo. Ninguna cita de «PayPal API Standards» es verificable a 2026-07-20.

---

## 7. Advertencias de estado

Los casos siguientes son aquellos en que el estado formal de un documento contradice lo que la mayoría del material sobre APIs da por sentado. Cada uno produce una cita incorrecta que suena plausible.

**429 Too Many Requests no está en RFC 9110.** El índice de `N-01` se revisó explícitamente: 428, 429, 431 y 451 no figuran entre sus secciones. Los tres primeros los define `N-03` (RFC 6585, Proposed Standard, abril de 2012, vigente y no obsoleto) y el cuarto `N-82` (RFC 7725, Proposed Standard, febrero de 2016). La atribución «429 según RFC 9110» es incorrecta y aparece constantemente, probablemente porque 9110 consolidó tantos otros códigos que se asume exhaustividad. Para el código se cita `N-03`; para `Retry-After`, `N-01`.

**Los campos RateLimit son un Internet-Draft y no se llaman como se cree.** `F-02` define exactamente dos campos, ambos Structured Field Lists y ambos prohibidos en trailers: `RateLimit-Policy`, con parámetros `q`, `qu`, `w` y `pk`, y `RateLimit`, con parámetros `r`, `t` y `pk`. Ni los populares `X-RateLimit-Limit` / `X-RateLimit-Remaining` / `X-RateLimit-Reset` ni los `RateLimit-Limit` / `RateLimit-Remaining` / `RateLimit-Reset` de revisiones anteriores del propio draft corresponden al documento actual. Una guía que prescriba los nombres con prefijo `X-` está prescribiendo una convención de facto sin decirlo; una que prescriba los nombres largos está citando un draft superado. El documento vigente expira el 2026-11-24 y puede volver a cambiar.

**El draft de Idempotency-Key expiró sin llegar a RFC.** `F-01` alcanzó la revisión -07 el 2025-10-15 y su estado en el datatracker es *Expired*; no tiene número de RFC asignado. La frase «seguí el draft IETF de Idempotency-Key» cita un documento muerto. Lo que existe es una convención de facto impuesta por Stripe (`P-04`) y adoptada por el ecosistema de pagos, y como tal debe presentarse. La cabecera es perfectamente utilizable; lo que no es utilizable es el argumento de autoridad.

**Sunset y Deprecation no tienen el mismo peso.** `N-13` (RFC 8594, mayo de 2019) es **Informational**: no es standards track. `N-12` (RFC 9745, marzo de 2025) sí es Proposed Standard. La intuición corriente los trata como pareja simétrica y no lo son. Tampoco comparten formato: `Deprecation` es un Structured Field de tipo Date, expresado como `@` seguido de tiempo Unix —`Deprecation: @1688169599`—, mientras `Sunset` usa un HTTP-date clásico —`Sunset: Sat, 31 Dec 2018 23:59:59 GMT`—. Y `N-12` dejó de ser draft: citarlo como `draft-dalal-deprecation-header`, como se hizo durante años, ya es incorrecto.

**La adopción de `Deprecation` en el formato que prescribe `N-12` no está acreditada.** La búsqueda de plataformas grandes que emitan la cabecera arrojó **un solo caso documentado, y no cumple el RFC**. GitHub declara en `P-05` que emite *«Deprecation — The date when the API version will be closing down, formatted as an HTTP date per RFC 7231»*, con ejemplo `Wed, 27 Nov 2019 14:34:29 GMT`, junto a `Sunset` *«Follows RFC 8594»* y un `410 Gone` posterior. Es decir: usa el **nombre** de la cabecera con el **formato HTTP-date de la etapa de draft**, no el Structured Field de tipo Date (`Deprecation: @1688169599`) que `N-12` exige, y cita un RFC obsoleto para el formato. La conclusión operativa para los documentos temáticos es doble: existe evidencia de que la cabecera se usa, y **no hay evidencia de una implementación conforme a RFC 9745**. Las citas a Stripe y GitHub que circulan en material secundario se refieren a ventanas de deprecación de dos años, no al formato de la cabecera, y no sirven como respaldo.

**OData 4.02 no es un estándar aprobado.** El estándar OASIS vigente es la versión 4.01 (`N-21`). La 4.02 (`F-05`) es Committee Specification Draft 02, en revisión pública desde abril de 2024 y sin aprobación a julio de 2026. Se la cita habitualmente como «la versión actual de OData» porque es la más alta publicada, lo cual confunde el número más nuevo con el documento con autoridad.

**Ninguna especificación de GraphQL prescribe responder `200` con `errors` en el cuerpo.** Es la atribución falsa más extendida sobre GraphQL y requiere separar dos documentos. `N-22` es agnóstica del transporte: su Section 7 fija que un *execution result* puede contener `data` y `errors` a la vez, y no menciona HTTP en ningún punto. La parte HTTP vive en `F-12`, que es un **draft** y que para la respuesta parcial recomienda `294`, un código que el propio proyecto reconoce haber inventado. El `200` proviene de la ruta de compatibilidad que `F-12` define para el *legacy client*. Lo correcto es presentar el error en cuerpo como comportamiento normativo de `N-22` y el `200` como convención de facto del ecosistema.

**Hydra no es un estándar del W3C, y desde mayo de 2026 ni siquiera tiene grupo.** `F-20` es un Community Group Report, y su propia sección de estado dice *«It is not a W3C Standard nor is it on the W3C Standards Track»*. El grupo cerró el 2026-05-21. Citar «Hydra, la especificación del W3C» es incorrecto en el término que importa.

**El media type de HAL que todo el mundo emite no está registrado.** El registro consultado en `N-80` contiene `application/vnd.hal+json` en el vendor tree; `application/hal+json`, que es lo que emiten la especificación y las implementaciones reales, no aparece. `F-17` expiró como Internet-Draft el 2024-04-21 y su sección de IANA dice *«No IANA actions required»*.

**gRPC está en incubating, no en graduated.** `O-07` es explícito y `N-78` lo corrobora. La confusión es frecuente porque su adopción es mucho mayor que la de varios proyectos graduated, entre ellos `F-15`.

**OpenAPI 4.0 «Moonwalk» no existe como especificación publicada.** `F-06` sigue en fase de diseño en el SIG correspondiente, sin fecha de release y sin soporte de tooling en producción; el propio SIG recomienda usar la serie 3.x. La versión estable a citar es `N-19`, OAS 3.2.0, publicada el 2025-09-19.

### Advertencias de estado en el ecosistema .NET

**«Removido de las plantillas» no significa «deprecado»: el caso de Swashbuckle.** Microsoft anunció en `N-62` la remoción de `Swashbuckle.AspNetCore` de la plantilla web API, y la razón declarada en 2024 fue de mantenimiento: *«The project is no longer actively maintained by its community owner … there is not an official release for .NET 8»*. Esa premisa ya no describe la realidad. `F-07` publicó su versión 10.2.3 el 2026-06-22, soporta `net10.0` explícitamente, acumula 1.100 millones de descargas y **no está marcado como deprecado en NuGet**. La formulación correcta es «Swashbuckle ya no viene en las plantillas»; la formulación «Swashbuckle está muerto» es falsa a esta fecha. Es el caso donde la distancia entre la decisión de producto y el estado del proyecto es mayor, y donde más material desactualizado circula.

**ASP.NET Core no implementa RFC 9457, y su propia documentación es inconsistente al respecto.** Bajo el mismo moniker `aspnetcore-10.0` se citan tres RFC distintos en tres lugares: `N-28` enlaza a **RFC 7807** en su sección compartida y en la pestaña de Minimal APIs, la pestaña de controllers del mismo artículo enlaza a **RFC 9457**, y la referencia de API de `N-30` describe el tipo como basado en **RFC 9110**, sin mencionar 9457 en ninguna parte de la página. El estado upstream lo resuelve: `N-63` sigue **abierto** en milestone Backlog, sin ramas ni pull requests asociados. La afirmación «ASP.NET Core implementa RFC 9457» no está respaldada por la documentación. En la práctica el impacto sobre el formato de cable es escaso, porque `N-04` obsoleta a 7807 sin romper el JSON, pero la cita debe ser precisa.

**El rate limiter nativo rechaza con 503, no con 429.** `N-43` es literal: *«Defaults to `Status503ServiceUnavailable`»*. El 429 aparece en `N-42` únicamente como opt-in explícito mediante `RejectionStatusCode`. La consecuencia práctica es severa: un rate limiter dejado en su configuración por defecto le dice al cliente que el servidor está caído en lugar de pedirle que baje la frecuencia, con lo que las políticas de reintento del consumidor se comportan al revés de lo esperado. Tampoco hay cabecera `Retry-After` automática: hay que leer `MetadataName.RetryAfter` del lease dentro de `OnRejected` y escribirla a mano, y esa metadata no está presente en todos los limitadores.

**Minimal APIs es el enfoque oficialmente recomendado, no la opción para demos.** `N-24` es explícito —*«For new projects, we recommend using Minimal APIs»*— y sus propios títulos de sección son normativos: *Minimal APIs - Recommended for new projects* frente a *Controller-based APIs - Alternative approach*. `N-58` lo confirma del lado de las herramientas: `dotnet new webapi` sin opciones genera un proyecto de Minimal APIs. La creencia habitual, que Minimal APIs sirve para ejemplos y los controllers para aplicaciones serias, es la inversa de la posición documentada. La lista oficial de razones para elegir controllers es corta y concreta: extensibilidad de model binding, validación avanzada vía `IModelValidator`, application parts y el application model, y soporte de OData.

**La validación nativa de .NET 10 llegó a Minimal APIs y Blazor, y no a MVC.** `N-35` lo dice sin ambigüedad: *«It's not supported by default in MVC»*. Esto invierte la otra mitad de la creencia anterior, la de que los controllers tienen la validación buena y Minimal APIs no tiene nada. Conviene además no repetir la afirmación de que `Microsoft.Extensions.Validation` shippeó como experimental en .NET 10: aparece solo en blogs de terceros y la documentación oficial la contradice.

**El camelCase de las APIs .NET es un default de ASP.NET Core, no de System.Text.Json.** `N-38` establece que, sin ASP.NET Core, los nombres de propiedad y las claves de diccionario quedan sin cambios, incluida la capitalización. El camelCase proviene de `JsonSerializerDefaults.Web`, que según `N-39` implica cuatro cosas y no una: enteros codificados lo más pequeño posible, nombres tratados sin distinguir mayúsculas al deserializar, formato camelCase, y aceptación de números entre comillas. Las dos del medio suelen ignorarse y ambas afectan al contrato. Hay una excepción documentada en `N-37`: `ProblemDetails` es siempre camelCase aunque la aplicación fije PascalCase.

**`Asp.Versioning` vive bajo `github.com/dotnet` y no es un producto de Microsoft.** Es probablemente el malentendido más extendido sobre este paquete. Su mantenedor lo declara textualmente en `F-10`: *«I am not, nor have ever been, a part of the ASP.NET team»*, y el proyecto *«has never had any type of official company sponsorship or funding»*. El README indica tutela de la .NET Foundation, que no es soporte de producto de Microsoft. La razón declarada del renombrado desde `Microsoft.AspNetCore.Mvc.Versioning` fue precisamente que el nombre anterior sugería propiedad de Microsoft. El proyecto está activamente desarrollado y soporta .NET 10; lo que no tiene es respaldo corporativo.

**La API de `F-09` para declarar versiones en Minimal APIs es `NewVersionedApi()`, no `NewApiVersionSet()`.** El quick start oficial del wiki del proyecto muestra `var people = app.NewVersionedApi();` seguido de `people.MapGet( "/people", … ).HasApiVersion( 1.0 );`, y las release notes registran que **`MapApiGroup` pasó a llamarse `NewVersionedApi`** por recomendación del equipo de ASP.NET. Coincide con `N-68`. El matiz que evita una corrección excesiva: `NewApiVersionSet()` y `WithApiVersionSet()` **no fueron eliminados** —`WithApiVersionSet` conserva desde 7.0.0 la firma `TBuilder WithApiVersionSet<TBuilder>(TBuilder, ApiVersionSet)`—, de modo que el material de comunidad que los usa no está roto, está escrito en el estilo anterior. La versión 10.0.0 sí introdujo breaking changes propios, derivados del soporte de *extension members* de C#: varios extension methods pasaron a extension properties.

**NSwag no está abandonado.** `F-24` publicó `NSwag.AspNetCore` 14.7.1 el 2026-04-20, el repositorio no tiene aviso de archivo ni de búsqueda de mantenedores, y acumula 130,7 millones de descargas. La precisión que sí corresponde hacer es sobre los targets: el paquete declara `net8.0`, `netstandard2.0` y `net462`, y **no** un TFM `net10.0` propio; se consume desde .NET 10 por compatibilidad computada, no por soporte declarado. Es una situación distinta de la de `F-07`, que sí declara `net10.0` explícitamente.

**El source generator de comentarios XML para OpenAPI existe y no exige configuración.** `N-83` lo dice literalmente: *«The XML documentation feature is implemented as a source generator»*, con la clase `XmlCommentGenerator`, y sobre su habilitación: basta `<GenerateDocumentationFile>true</GenerateDocumentationFile>` en el `.csproj` más `AddOpenApi()`, porque *«No configuration is needed, the source generator handles the rest»*. El procesamiento ocurre en tiempo de compilación y no tiene costo en runtime. Los tags soportados son `<summary>`, `<remarks>`, `<param>`, `<returns>`, `<response>`, `<example>`, `<deprecated>` e `<inheritdoc>`. La limitación real documentada **no** es la que suele repetirse: el generador deja de intervenir cuando el `documentName` de `AddOpenApi` no es una cadena literal —`AddOpenApi(documentName)` con una variable no obtiene soporte XML—. La restricción de que el handler de Minimal API deba ser un método nombrado y no un lambda **no figura en la página**, aunque todos sus ejemplos usan métodos estáticos nombrados; sigue entre las fuentes no verificadas.

**`RequireAuthorization()` sobre un `MapGroup(...)` está documentado con ejemplo.** `N-25`, sección *Route groups*: *«Use this method to customize entire groups of endpoints with a single call to methods like `RequireAuthorization` and `WithMetadata` that add endpoint metadata»*, con el ejemplo `app.MapGroup("/private/todos").MapTodosApi().WithTags("Private").AddEndpointFilterFactory(QueryPrivateTodos).RequireAuthorization();` y la consecuencia declarada de que ese grupo *«require authentication»*. La misma página fija la semántica que importa al enseñarlo: *«Adding filters or metadata to a group results in the same behavior as adding them individually to each endpoint»*.

**Las plantillas de .NET 10 no traen ninguna interfaz de usuario de OpenAPI.** Ni Swagger UI ni Scalar. `N-33` es normativo al respecto —*«Interactive UIs such as Swagger UI or Scalar are not included by default and must be added separately»*— y advierte con énfasis propio que esas interfaces deben habilitarse solo en entornos de desarrollo. La lectura de `N-66` lo confirma en el código: la plantilla llama a `AddOpenApi()` y a `MapOpenApi()` dentro de `IsDevelopment()`, sin UI alguna, y su `launchSettings.json` trae `launchBrowser: false` justamente porque ya no hay nada que abrir. La idea de que .NET 10 reemplazó Swagger por Scalar en las plantillas es incorrecta en ambos términos.

**El rumor sobre la licencia de FluentValidation es falso.** `F-11` mantiene licencia Apache-2.0 sin cambios y su repositorio está activo. El cambio a licencia restrictiva corresponde a **Fluent Assertions**, un proyecto distinto, tras una alianza con Xceed en enero de 2025. El riesgo real de FluentValidation es de *bus factor* —un único mantenedor, en su tiempo libre, según el propio README—, no de licencia.

**Trampa de migración a .NET 10.** Según `N-29`, el comportamiento por defecto pasó a suprimir la emisión de diagnósticos —logs y métricas— para las excepciones manejadas, es decir cuando `TryHandleAsync` devuelve `true`. Un `IExceptionHandler` que funcionaba en .NET 9 deja de emitir logs de excepciones silenciosamente al actualizar. Se restablece con `ExceptionHandlerOptions.SuppressDiagnosticsCallback`.

Una advertencia adicional que no es de estado sino de contenido, y que conviene retener por su capacidad de generar citas falsas: **la palabra *idempotent* no aparece en RFC 7396** (`N-07`), verificado sobre el texto completo. Cualquier afirmación sobre la idempotencia de JSON Merge Patch puede ser cierta y no puede atribuirse a ese RFC. De manera análoga, `N-08` (RFC 3986) no declara explícitamente que el path sea sensible a mayúsculas; trata el scheme y el host como insensibles y no se pronuncia sobre el resto. La regla «usá minúsculas en las rutas» es una convención de diseño, no un mandato de RFC 3986.

---

## 8. Fuentes no verificadas

Lo que sigue se consultó y **no pudo confirmarse de primera mano**. Ningún documento de esta guía debe citar estas entradas como respaldo de una afirmación normativa. Se registran para evitar que un revisor posterior repita el intento creyéndolo pendiente, y para que quede constancia de qué se buscó.

### 8.1 Especificaciones no consultadas o confirmadas solo de forma indirecta

| Ítem | Situación |
|------|-----------|
| RFC 6901 — JSON Pointer | No consultado. Referenciado por `N-06` para el formato de `path` y `from` |
| RFC 9651 — Structured Field Values for HTTP | No consultado directamente; solo referenciado por `N-12` y `F-02` |
| RFC 9700, RFC 8252, RFC 8996 | Solo se verificó que actualizan a `N-15`. Títulos y contenidos exactos sin confirmar |
| Overlay Specification (OpenAPI Initiative) | Versión y fecha no obtenidas |
| Estado y fecha de JSON:API v1.2 | Mencionada como próxima, sin detalles |
| Designación formal de retiro de SOAP por el W3C | Se verificó el **cierre del XML Protocol Working Group** el 2009-07-10, pero no se pudo consultar el listado de TR obsoletos. No afirmar «el W3C la retiró» ni «el W3C nunca la retiró» |
| Lado cliente de WCF en .NET moderno (`System.ServiceModel.*`) | Solo se confirmó el port del lado servidor (`F-23`). El estado de las bibliotecas cliente no se verificó |
| Que tRPC exija un monorepo | Ninguna página oficial lo declara como requisito; el quickstart usa directorios `client/` y `server/` separados. Lo verificado es el mecanismo de inferencia de tipos, no la topología del repositorio |
| Adopciones que se atribuye Standard Webhooks (`F-16`) | OpenAI, Google, Brex, Twilio y Etsy figuran en la página del propio proyecto y **no se contrastaron** contra la documentación de esas empresas |
| Fechas de release de tRPC en GitHub | La lectura de la página de releases devolvió fechas de 2024 por mala interpretación de los timestamps relativos. Se usaron las del registry de npm, que son las autoritativas |

Tres entradas que figuraban acá **salieron de esta sección** en la revisión del 2026-07-20 porque se verificaron de primera mano: el texto de la especificación GraphQL (`N-22`), las fuentes primarias sobre gRPC y tRPC (`N-74` a `N-78`, `F-21`, `F-22`, `O-07`) y las especificaciones de HAL, Siren, Ion e Hydra (`F-17` a `F-20`). En los tres casos la verificación cambió el contenido de lo que la guía podía afirmar, y no solo su nivel de confianza.

### 8.2 Guías con datos incompletos

| Ítem | Situación |
|------|-----------|
| Prescripciones REST concretas de adidas (`G-07`) | No expuestas en la página consultada; requieren leer el GitBook |
| Fecha o versión de las guías de Zalando (`G-05`) | No figura en la página |
| Fecha o versión del documento de Graph (`G-02`) | Sin fecha de publicación en el documento |
| Declaración de canonicidad entre `G-01` y `G-02` | El README de `microsoft/api-guidelines` no la contiene |
| Casing JSON exacto prescrito por Google (`G-04`) | AIP-140 normaliza `lower_snake_case` solo en protobuf y menciona el mapeo a JSON sin especificar la convención |
| Texto de la Azure Breaking Change Policy | Referenciada por `G-01`, no enumerada en el documento consultado |
| Guía de diseño de APIs pública y oficial de Stripe | No se encontró ninguna. La disciplina de `snake_case` de Stripe proviene de su referencia y de análisis de terceros, no de un documento normativo publicado |
| Año exacto de la charla de Leonard Richardson en QCon | El artículo de Fowler (`O-03`) referencia la charla sin año |

### 8.3 Prácticas de plataformas no confirmadas

| Ítem | Situación |
|------|-----------|
| Definición formal de *breaking change* de GitHub | La URL correspondiente devuelve 404 |
| Parámetros de paginación de Atlassian Jira Cloud REST v3 | Contenido truncado; `startAt`, `maxResults`, `total`, `isLast` sin confirmar |
| Políticas de deprecación publicadas de Twilio y Shopify más allá de la ventana de soporte | No obtenidas |
| Emisión del header `Deprecation` **en el formato de `N-12`** por alguna plataforma grande | **Sin evidencia de adopción encontrada.** El único caso hallado es GitHub (`P-05`), que emite una cabecera `Deprecation` con HTTP-date citando RFC 7231, no el Structured Field de RFC 9745. Ver la advertencia correspondiente en la sección 7 |
| Implementaciones de `Idempotency-Key` fuera de Stripe | Twilio, Shopify, Adyen y PayPal no verificadas; tampoco si coinciden en nombre de cabecera o código de estado |
| Alguna plataforma grande sirviendo `application/problem+json` | No verificada. Ni Stripe ni Azure lo hacen: ambas usan formato propio |
| Modelo de errores de validación de GitHub, Twilio, Shopify y Atlassian | No verificado |
| Que alguna especificación de GraphQL prescriba `200` con `errors` en el body | **Verificado como falso.** `N-22` no menciona HTTP; `F-12`, que es un draft, recomienda `294` para el caso parcial y solo contempla `200` en su ruta de compatibilidad con clientes que únicamente entienden `application/json`. El `200` es convención de facto del ecosistema |
| MDN sobre *domain sharding* y concatenación como antipatrones | `O-06` no contiene esas afirmaciones. La atribución, muy repetida, no está respaldada |

### 8.4 .NET y ASP.NET Core

| Ítem | Situación |
|------|-----------|
| Restricción de que el handler de Minimal API deba ser un método nombrado y no un lambda para que el generador de comentarios XML lo procese | **No figura en `N-83`.** La página no la enuncia, aunque todos sus ejemplos usan métodos estáticos nombrados. No afirmarla como documentada |
| Versión y estado en nuget.org de `Refit` y `Refit.HttpClientFactory` | Solo se verificó que `N-50` los menciona como biblioteca de terceros |
| Contenido literal del `.csproj` de la plantilla `webapi` | No está en el repositorio; se produce al construir el paquete de plantillas. `ImplicitUsings`, `Nullable` y `TargetFramework` están fuertemente inferidos de los archivos fuente, no leídos |
| Un source generator de .NET 10 que haga público el `Program` de top-level statements para `WebApplicationFactory` | Afirmado por fuentes secundarias, **no confirmado**. Las release notes no tienen sección de testing y el artículo no menciona el mecanismo. Seguir enseñando `public partial class Program { }` |
| `Microsoft.AspNetCore.Testing` como paquete de pruebas nuevo | Sin evidencia. El paquete soportado sigue siendo `Microsoft.AspNetCore.Mvc.Testing` |
| Advertencias `RequiresUnreferencedCode` de `Microsoft.Extensions.Validation` | No verificadas |
| Que `Microsoft.Extensions.Validation` shippee como experimental en .NET 10 | Aparece solo en blogs de terceros y `N-35` lo contradice. **No repetir la afirmación** |
| `JsonSerializerOptions.AddContext<T>()` | No aparece en ninguna página actual. Existió históricamente; usar `TypeInfoResolver` o `TypeInfoResolverChain` |
| Cambios de .NET 10 en los defaults de JwtBearer, migración de `Microsoft.IdentityModel` v8, endpoints de token integrados | No figuran en `N-49` |
| Que Blazor Server no pueda acceder a cookies o `localStorage` del navegador de la misma forma | `N-53` no lo dice. Lo que sí documenta es que `IHttpClientFactory` crea los `DelegatingHandler` en un scope de DI distinto del circuito de Blazor |
| `NSUrlSessionHandler` y `AndroidMessageHandler` | `N-54` no los menciona; solo usa `HttpClientHandler`. Existen, pero están documentados en otra página |
| Integración de Kiota con MSBuild u `OpenApiReference` | `N-59` documenta solo CLI, Docker, dotnet tool, extensión de VS Code y GitHub Action |
| Que el default de `dotnet new webapi` haya pasado a Minimal APIs **en .NET 8** | `N-58` ancla `--use-controllers` a «.NET 8 SDK», que es la afirmación oficial más fuerte disponible, pero no contiene una frase que fije la versión del cambio de default |
| Matriz comparativa oficial entre Minimal APIs y controllers | No existe. `N-24` es prosa y viñetas. El renderizado de vistas **no** figura en su lista de carencias, y el binding de formularios con `IFormFile` fue **eliminado** de esa lista a partir del moniker 7.0 |

---

## 9. Cómo citar en esta guía

Los documentos temáticos citan por identificador y sección exacta, no por URL. Se escribe «`N-01` §9.2.2» y no «el estándar HTTP» ni el enlace al RFC. La razón es doble: las URL cambian y los documentos se obsoletan entre sí, de modo que la designación exacta es lo único que permite a un lector detectar que una afirmación se apoya en un documento retirado. Toda la sección 6 de este anexo existe porque esa detección, en este tema, hace falta a menudo.

La forma de la cita varía con el nivel. Una fuente `N-xx` se cita por identificador y sección. Una `G-xx` se cita nombrando la organización, porque el lector debe poder decidir si esa organización le aplica: «`G-04` AIP-158 prescribe `page_token` opaco». Una `F-xx` se declara como convención y se acompaña de la evidencia que la sostiene: «convención de facto, sostenida por `P-04`». Una `P-xx` se presenta siempre como descripción de lo que una plataforma hace, nunca como prescripción. Y cuando una afirmación no tiene respaldo en ninguna entrada de este anexo, se declara como criterio propio con la fórmula «esta guía recomienda».

Las afirmaciones sobre .NET llevan además un ancla de versión. La documentación de Microsoft Learn se sirve por moniker y su comportamiento por defecto cambia entre releases, de modo que «ASP.NET Core genera OpenAPI 3.1» solo es cierto acompañado de «desde .NET 10». Cuando una afirmación aparece en la tabla de la sección 1.9, el documento temático indica la versión desde la cual aplica; cuando no aparece, se asume .NET 10, que es la versión de referencia de esta guía. Las capacidades de .NET 11 se citan siempre marcadas como preview y nunca sostienen una recomendación.

Al revisar la guía, toda afirmación normativa debería poder rastrearse hasta una fila de este anexo. Si no puede, o falta la fuente o es criterio propio mal etiquetado.
