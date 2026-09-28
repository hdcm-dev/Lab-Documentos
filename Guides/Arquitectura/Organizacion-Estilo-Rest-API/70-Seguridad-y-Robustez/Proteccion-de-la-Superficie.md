---
doc_id: TEM-PROT
doc_type: tema
title: Protección de la superficie
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-SEG, TEM-AUTH, TEM-RESIL, TEM-STATUS, TEM-HEADERS, TEM-ERR, TEM-VALID, TEM-URI, MARCO-ACTORES, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Protección de la superficie — `TEM-PROT`

## Resumen ejecutivo

Una API autenticada sigue expuesta. Quien tiene credenciales válidas puede consumir más de lo que corresponde, mandar entradas que el sistema no esperaba, o aprender del sistema observando en qué se diferencian sus respuestas de error. Este documento trata los mecanismos con los que se acota esa superficie y, sobre todo, los criterios para elegir entre ellos, porque tres de los cuatro que se tratan acá se aplican mal con frecuencia por razones distintas.

El rate limiting se aplica mal por un default: el limitador nativo de ASP.NET Core rechaza con `503` y no con `429`. CORS se aplica mal por un malentendido de fondo: se lo trata como defensa del servidor cuando es una restricción del navegador. Y las respuestas de error se diseñan mal por una tensión legítima entre dos actores, `ACT-03`, que necesita saber qué pasó, y `ACT-07`, que sabe que cada distinción adicional describe el sistema desde afuera.

Ese último punto es el que [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md) señala como la intervención más específica y menos advertida de `ACT-07`: un `404` que distingue «la reserva no existe» de «la reserva existe pero no es tuya» le confirma a quien pregunta la existencia del recurso. La distinción es gratuita de implementar y no tiene vuelta atrás una vez publicada.

---

## Definición

### Qué abarca

Cuatro mecanismos que comparten un propósito: acotar lo que un llamante puede hacer, mandar o aprender.

**Rate limiting** acota el volumen. **CORS** determina qué páginas web pueden llamar a la API desde un navegador. **Validación de entrada** determina qué datos cruzan la frontera del sistema. **Diseño de las respuestas de error** determina qué se aprende observando desde afuera.

### Qué resuelven

El rate limiting protege la disponibilidad: sin él, un consumidor mal programado —que es un caso mucho más frecuente que el malicioso— degrada el servicio para todos los demás. La validación protege la integridad y es la última frontera antes del dominio. El diseño de errores protege lo que en `CTX-1` no se puede recuperar: la información que ya se publicó.

### Qué no son

**El rate limiting no es control de acceso.** Limita cuánto, no qué. Un límite de mil peticiones por hora sobre un endpoint que no debería ser accesible sigue permitiendo mil peticiones que no deberían ocurrir.

**El rate limiting no es facturación.** Cuando el límite expresa un plan comercial —el caso habitual en `CTX-1`— sigue habiendo dos cosas distintas: la cuota del contrato y la protección técnica. Suelen implementarse con el mismo mecanismo y responden a decisiones de actores distintos, `ACT-06` y `ACT-07`, que la matriz de responsabilidad registra por separado.

**CORS no es una defensa del servidor.** Merece desarrollo propio más abajo, porque el malentendido es casi universal.

**La validación de entrada no es la validación de negocio.** Que el campo `desde` sea una fecha válida se comprueba en la frontera; que la sala esté libre en esa fecha es una regla de dominio. La primera devuelve `400`, la segunda `409` o `422`.

**El diseño de errores no es la ocultación de errores.** Devolver `500` sin cuerpo ante todo es seguro y es inútil: `ACT-03` no puede construir un cliente contra una API que no distingue sus fallos.

---

## Rate limiting

### Para qué sirve realmente

La justificación que se enuncia primero —frenar abuso deliberado— rara vez es la que más rinde. Lo que un límite de uso detiene todos los días es el consumidor con un bucle mal escrito, el reintento agresivo sin *backoff* que convierte una falla transitoria en una caída sostenida, y el proceso *batch* que alguien lanzó a las tres de la mañana sin avisar. La protección frente a un actor decidido es real pero parcial: quien quiere saturar un servicio no se detiene ante un `429`.

Un segundo propósito, menos citado y a menudo más valioso, es **informativo**. Un límite bien comunicado le dice al consumidor cuál es el uso previsto de la API antes de que descubra el límite chocándose con él, y eso convierte una fuente de fricción en documentación.

### Los algoritmos disponibles en ASP.NET Core

`N-42` documenta cuatro limitadores en el espacio de nombres `Microsoft.AspNetCore.RateLimiting`, con sus tipos de opciones en `System.Threading.RateLimiting`. Están en el *framework* compartido desde .NET 7 y no requieren paquete adicional.

| Limitador | Método de registro | Qué limita | Dónde encaja |
|---|---|---|---|
| Ventana fija | `AddFixedWindowLimiter` | N peticiones por ventana de tiempo | Límite simple y predecible; el más fácil de documentar |
| Ventana deslizante | `AddSlidingWindowLimiter` | Ídem, sin el salto de la frontera de ventana | Cuando el pico al reiniciar la ventana es un problema |
| *Token bucket* | `AddTokenBucketLimiter` | Consumo con recarga continua | Cuando conviene tolerar ráfagas cortas |
| Concurrencia | `AddConcurrencyLimiter` | Peticiones simultáneas | Protección de un recurso escaso; no acota el total |

`N-42` marca la diferencia del cuarto: *«The fixed, sliding, and token limiters all limit the maximum number of requests in a time period. The concurrency limiter limits only the number of concurrent requests and doesn't cap the number of requests in a time period.»* Un limitador de concurrencia no impide que un cliente haga un millón de peticiones; impide que las haga a la vez.

La ventana fija es la elección por defecto que esta guía recomienda, y la razón es de contrato antes que técnica: es el único de los cuatro que se explica al integrador en una frase.

### El default de `503` y por qué corregirlo

`N-43` es literal: *«Gets or sets the default status code to set on the response when a request is rejected. Defaults to `Status503ServiceUnavailable`.»* El `429` aparece en `N-42` únicamente como opt-in explícito.

La consecuencia no es cosmética. `503 Service Unavailable` comunica que el servidor no está en condiciones de atender; `429 Too Many Requests` —definido por `N-03`, RFC 6585, **no** por `N-01`, como precisa [`TEM-STATUS`](../30-Semantica-HTTP/Codigos-de-Estado.md)— comunica que el cliente debe bajar la frecuencia. Son mensajes distintos y producen conductas opuestas del otro lado.

Un cliente bien construido reacciona a `503` reintentando, porque la indisponibilidad es transitoria por definición y reintentar es lo correcto. Un cliente que reintenta contra un límite de uso lo vuelve a alcanzar de inmediato. Y como el *pipeline* de resiliencia estándar de .NET reintenta ante `429` **y** ante `5xx` —`N-51`—, la corrección tampoco desactiva el reintento; lo que hace es habilitar la lectura de `Retry-After` y darle al consumidor la información con la que espaciar. Un rate limiter dejado en su configuración por defecto le dice al consumidor que el servidor está caído, y con esa premisa toda la política de reintentos del otro lado se comporta al revés de lo previsto.

La corrección es una línea. Que haga falta escribirla es el punto.

`Retry-After` tampoco es automática. Hay que leer la metadata del *lease* dentro de `OnRejected` y escribirla a mano, y `N-42` advierte que `MetadataName.RetryAfter` **no está presente en todos los limitadores**: los basados en ventana la proveen, el de concurrencia no tiene con qué calcularla. Un código que la asuma presente emite respuestas sin ella en silencio.

Sobre el orden del *middleware*, `N-42` es normativo: *«`UseRateLimiter` must be called after `UseRouting` when rate limiting endpoint specific APIs are used… When calling only global limiters, `UseRateLimiter` can be called before `UseRouting`.»*

### Cómo comunicar los límites al cliente

Acá el nivel de autoridad cambia y conviene decirlo antes que nada: **no hay estándar**. Es la zona del tema donde más se cita como norma algo que no lo es.

`F-02` —`draft-ietf-httpapi-ratelimit-headers-11`, del 23 de mayo de 2026— es un **Internet-Draft activo** del grupo de trabajo HTTPAPI, no un RFC, y expira el 24 de noviembre de 2026. Define exactamente **dos** campos, ambos *Structured Field Lists* y ambos prohibidos en *trailers*:

| Campo | Parámetros | Significado |
|---|---|---|
| `RateLimit-Policy` | `q` (cuota, requerido), `qu` (unidad: `requests`, `content-bytes`, `concurrent-requests`), `w` (ventana en segundos), `pk` (clave de partición) | La política vigente |
| `RateLimit` | `r` (cuota restante, requerido), `t` (segundos hasta el reinicio), `pk` | El estado actual del llamante |

Los parámetros son de **una sola letra**. Esto sorprende a casi todo el mundo, y explica la confusión más extendida del tema: ni los populares `X-RateLimit-Limit` / `X-RateLimit-Remaining` / `X-RateLimit-Reset`, ni los `RateLimit-Limit` / `RateLimit-Remaining` / `RateLimit-Reset` de revisiones anteriores **del propio draft**, corresponden al documento actual.

De ahí se sigue un criterio de citación que esta guía sostiene con firmeza. Una guía que prescribe los nombres con prefijo `X-` está prescribiendo una convención de facto sin declararlo. Una que prescribe los nombres largos está citando un draft superado. Y el documento vigente expira dentro de meses y puede volver a cambiar.

Esta guía recomienda, para `CTX-1`, comunicar los límites de forma explícita y documentada, elegir el juego de cabeceras con conocimiento de su estatus, y **documentar la elección**. Los `X-RateLimit-*` son la opción más interoperable en la práctica porque es la que los clientes existentes ya entienden; adoptarlos es legítimo mientras no se los presente como estándar. Lo que sí es normativo y no debe faltar es `Retry-After`, definida en `N-01` §10.2.3 y tratada en [`TEM-HEADERS`](../30-Semantica-HTTP/Cabeceras-y-Negociacion.md).

> `N-01` §10.2.3 no se consultó individualmente en la verificación de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md). Un lector que necesite apoyarse en su texto exacto para una decisión de contrato debe abrir el RFC.

### Cómo se particiona

Un límite necesita una clave: se cuenta por algo. Por token o cuenta es lo correcto cuando hay identidad, y es la razón por la que este documento va después de [`TEM-AUTH`](Autenticacion-y-Autorizacion.md). Por dirección IP es lo único disponible antes de autenticar, y es una aproximación pobre —muchos usuarios legítimos comparten salida— aunque necesaria para proteger el propio *endpoint* de autenticación, que por definición no puede exigir un token.

Una consecuencia de diseño que se descubre tarde: **el límite por IP sobre el endpoint de login castiga al usuario legítimo de una oficina compartida**. Particionar ese caso por cuenta además de por IP evita convertir una protección en una interrupción de servicio.

---

## CORS

### Qué es

*Cross-Origin Resource Sharing* es un mecanismo por el cual un servidor le indica al **navegador** que páginas de otros orígenes pueden leer sus respuestas. El navegador aplica por defecto la política del mismo origen: el código JavaScript de `https://app.ejemplo.com` no puede leer la respuesta de `https://api.salas.ejemplo.com` salvo que esta última lo autorice con cabeceras.

`N-47` documenta la implementación en ASP.NET Core y `N-48` fija su posición en el *pipeline*: `UseCors` antes de `UseAuthentication`, y —esto se olvida— antes de `UseResponseCaching`, *«to add CORS headers on every request, including cached responses»*. Además, con enrutamiento por *endpoints*, `N-47` es explícito en que el *middleware* **debe** ejecutarse entre `UseRouting` y los *endpoints*.

### Qué protege y qué no

Lo que CORS protege es al **usuario del navegador**, no a la API. Impide que una página maliciosa que el usuario visitó lea respuestas de otro origen aprovechando las credenciales que ese usuario ya tiene en el navegador.

Lo que CORS **no** hace, y esta es la parte que se malinterpreta de forma casi universal:

**No impide que se llame a la API.** Salvo en las peticiones que disparan *preflight*, la petición llega al servidor y se ejecuta; lo que el navegador bloquea es que el JavaScript de la página **lea la respuesta**. Un efecto secundario ya ocurrió.

**No aplica fuera del navegador.** `curl`, un cliente HTTP de un servicio, una aplicación MAUI o un script no implementan la política del mismo origen porque no tienen origen. Ninguna configuración de CORS los afecta en lo más mínimo.

**No sustituye a la autenticación.** Una API sin control de acceso con CORS restrictivo está abierta: solo está incómoda para el JavaScript de otros sitios.

De ahí el criterio: **CORS es una decisión de compatibilidad con clientes de navegador, no un control de seguridad del servidor**. Se configura para habilitar a los clientes legítimos, y la protección de la API viene de [`TEM-AUTH`](Autenticacion-y-Autorizacion.md).

Dicho eso, hay una configuración que sí importa y es la combinación de comodín de origen con credenciales. Permitir cualquier origen y a la vez permitir el envío de credenciales anula la protección que CORS le da al usuario. El caso es exactamente aquel en el que el mecanismo hace su trabajo, y conviene no desactivarlo por conveniencia de desarrollo y olvidarse.

Un caso frecuente de `CTX-3` merece mención. `N-53` documenta que en Blazor WebAssembly el `HttpClient` está *«implemented using the browser's Fetch API and is subject to its limitations, including enforcement of the same-origin policy»*, y que esas aplicaciones a menudo no pueden llamar a APIs de otros orígenes. Las mitigaciones documentadas son un *backend* propio que haga de intermediario desde código de servidor, o un servicio de proxy. `SetBrowserRequestMode(BrowserRequestMode.NoCors)` está señalado explícitamente como **no** siendo una solución.

---

## Validación de entrada como frontera de seguridad

La mecánica de la validación en ASP.NET Core —`Microsoft.Extensions.Validation`, `AddValidation()`, el *source generator* de .NET 10, `[ApiController]` y sus `400` automáticos— la desarrolla [`TEM-VALID`](../80-Implementacion-en-NET/Validacion.md) sobre `N-35`. Acá interesa otra pregunta: por qué la validación es una decisión de seguridad y no solo de calidad.

Todo dato que entra al sistema y no se validó es un dato que el resto del código va a tratar como si fuera correcto. La frontera de validación es el punto donde eso deja de ser cierto, y su valor está en que sea **una sola** y esté **antes del dominio**. Una API que valida en tres lugares distintos tiene tres oportunidades de que uno quede desactualizado.

Tres decisiones concretas del contrato tienen carga de seguridad:

**Qué se hace con los campos de más.** Aceptar y descartar es lo tolerante; rechazar es lo estricto. `N-41` documenta que .NET 10 agregó `JsonSerializerOptions.AllowDuplicateProperties`, y la justificación citada es explícita: *«This can lead to unexpected results and security vulnerabilities.»* El preset `Strict` de .NET 10 rechaza propiedades no mapeadas y duplicadas. La decisión pertenece al contrato y la registra [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md), pero conviene saber que el permisivo es el default.

**Los límites de tamaño.** Longitud de cadenas, cantidad de elementos de un arreglo, profundidad de anidamiento, tamaño del cuerpo. Sin ellos, la validación de forma pasa y el consumo de recursos no está acotado.

**Qué dice el mensaje de error.** Un mensaje de validación que devuelve el valor recibido puede terminar reflejando en la respuesta lo que el llamante mandó, y esa respuesta va a algún lado. Enunciar la regla incumplida —«`desde` debe ser una fecha ISO 8601»— es informativo y no arrastra la entrada.

Hay una limitación documentada que conviene conocer porque produce un hueco silencioso: `N-65` registra que **los tipos de valor anulables declarados como parámetros de Minimal API no se validan**. Un `int?` como parámetro pasa sin comprobar.

Y una precisión de fondo sobre la actitud general. El principio de robustez de Postel —«sé conservador en lo que emitís, liberal en lo que aceptás»— está **cuestionado por el IAB** en `N-14` (RFC 9413): *«When official specifications fail to be updated, then deployed implementations — including their quirks — often become a substitute standard.»* Ese documento propone en cambio la *intolerancia virtuosa*, rechazar las violaciones para dar retroalimentación temprana. Aceptar lo que no se especificó no es generosidad: es aceptar un contrato que nadie escribió.

---

## Filtración de información por las respuestas de error

### El problema

Dos peticiones idénticas salvo por el identificador. Una devuelve `403 Forbidden` y la otra `404 Not Found`. La diferencia acaba de responder una pregunta que nadie hizo explícitamente: el primer recurso existe.

Es el ejemplo que [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md) usa para caracterizar la intervención más específica de `ACT-07`, y su forma general vale más que el caso: **cualquier diferencia observable entre dos respuestas transmite información**. El código de estado es la diferencia más visible, pero no la única: la presencia o ausencia de un campo, la redacción del mensaje, el tamaño del cuerpo y el tiempo de respuesta también lo son.

### La tensión con `ACT-03`

La tensión es real y la guía no la disuelve. `ACT-03` construye un cliente y necesita distinguir casos: si la reserva no existe, el flujo es uno; si existe y no le corresponde, es otro. Un `404` uniforme lo deja adivinando y multiplica el soporte que `ACT-06` va a tener que atender. La matriz de responsabilidad de [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md) registra esta fila —formato de errores— como una de las dos que más conflicto generan, con `ACT-03` y `ACT-07` tirando en direcciones opuestas.

Lo que la guía ofrece es un criterio para resolverla caso por caso, que depende de dos variables: **cuán sensible es la existencia del recurso** y **cuán adivinable es su identificador**.

| Situación | Respuesta recomendada | Por qué |
|---|---|---|
| Recurso de otro usuario, identificador opaco, `CTX-1` | `404` uniforme | La existencia es información y el identificador no se enumera |
| Recurso de otro usuario, identificador secuencial | `404` uniforme, y **cambiar el identificador** | Con identificadores secuenciales el `403` es una enumeración asistida |
| Recurso de la propia organización, `CTX-2` | `403` con detalle | El llamante ya está dentro del perímetro de confianza; la claridad rinde más |
| Falta de permiso sobre una operación, no sobre una instancia | `403` con el permiso requerido | No revela nada sobre el recurso y le dice a `ACT-03` qué pedir |
| Recurso que el usuario podría legítimamente conocer | `403` con detalle | Negar su existencia confunde a un usuario legítimo sin proteger nada |

La última fila importa porque el criterio se aplica en exceso con facilidad. Un `404` uniforme sobre un recurso cuya existencia el usuario ya conoce por otro camino no protege nada y produce un cliente desconcertado.

La regla que sí es incondicional: **lo que se elija debe ser uniforme**. Un sistema que devuelve `404` en el detalle y `403` en la cancelación de la misma reserva ya respondió la pregunta.

### Qué no debe aparecer nunca en un error

La lista es corta y no admite excepciones por entorno, porque los entornos se confunden:

**La traza de la excepción.** Expone la estructura interna, los nombres de los tipos, la organización del código y las versiones de las bibliotecas. `N-29` documenta que en .NET 10 el comportamiento por defecto pasó a **suprimir** la emisión de diagnósticos para las excepciones manejadas cuando `TryHandleAsync` devuelve `true`, lo cual es una trampa de migración en el sentido opuesto: los logs propios dejan de emitirse en silencio. Se restablece con `ExceptionHandlerOptions.SuppressDiagnosticsCallback`. Suprimir la traza **en la respuesta** y conservarla **en el log** son dos decisiones independientes y ambas necesarias.

**Los detalles del almacenamiento.** Nombres de tablas o columnas, fragmentos de consulta, mensajes del motor de base de datos. Un mensaje que dice qué restricción se violó describe el esquema.

**Las versiones.** De la plataforma, del *framework*, de las bibliotecas. `ESC-4b` enseña por qué: las cabeceras y los cuerpos de error son justamente donde se lee qué corre del otro lado.

**Las rutas del sistema de archivos y los nombres de máquina.** `N-28` documenta que `CustomizeProblemDetails` permite agregar extensiones, y el propio ejemplo de la documentación agrega `Environment.MachineName` como `nodeId`. Es útil para diagnóstico interno y no corresponde en `CTX-1`.

**Cualquier dato de otro usuario.** Incluido el que aparece por descuido al explicar un conflicto: «esa sala ya fue reservada por marina.acosta@…» resuelve el problema del usuario y publica un dato ajeno.

Lo que sí debe aparecer: un identificador de correlación. Permite que `ACT-03` reporte un problema concreto y que el productor lo encuentre en sus logs, sin que el cuerpo de la respuesta transporte nada del diagnóstico. Es la respuesta operativa a la tensión de esta sección, y la forma del cuerpo la fija [`TEM-ERR`](../40-Contratos-y-Representaciones/Manejo-de-Errores.md) con `N-04`.

### Exposición de identificadores

Un identificador secuencial en la URI publica dos cosas sin que nadie lo haya decidido. La primera es el volumen: `/v1/reservas/4192` informa que hubo unas cuatro mil doscientas reservas, y dos observaciones separadas por un mes dan la tasa. En `CTX-1` eso puede ser información comercialmente sensible. La segunda es más seria: convierte cualquier fallo de autorización por instancia en un problema sistemático, porque el identificador siguiente se conoce sin buscarlo.

Un identificador opaco —un UUID, un identificador aleatorio con suficiente entropía— no publica ninguna de las dos. Y conviene ser preciso sobre su alcance: **un identificador opaco no es un control de acceso**. No autoriza nada; lo que hace es que el fallo de autorización requiera conocer el identificador en lugar de contar hasta él. La comprobación de titularidad de [`TEM-AUTH`](Autenticacion-y-Autorizacion.md) sigue siendo obligatoria.

La decisión pertenece a [`TEM-URI`](../20-Diseno-de-Recursos/Nomenclatura-de-URIs.md) y tiene este costado. Su momento es `ESC-1`: cambiar la forma de los identificadores de una API publicada es rompiente sin matices. Esta guía recomienda identificadores opacos en el contrato público de `CTX-1` y `CTX-3`, con independencia de lo que use la base de datos internamente; en `CTX-2` el costo de la opacidad —la dificultad de leer un log a ojo— puede superar al beneficio.

---

## Aplicación por escenario

### `ESC-1` — API nueva

Dos de estas decisiones son caras de revertir y dos no. La **forma de los identificadores** y el **criterio de respuesta ante recursos ajenos** hay que tomarlos ahora: el primero es rompiente después, y el segundo no se puede deshacer porque la información que ya se publicó no se recupera. El **rate limiting** y **CORS** se pueden agregar cuando haga falta, con la salvedad de que un límite introducido sobre consumidores que ya se acostumbraron a no tenerlo se percibe como una degradación del servicio.

La trampa del escenario es la del límite fijado sin evidencia. Sin tráfico real no hay base para elegir el número, y un límite arbitrario o bien no protege o bien corta usos legítimos. Es preferible instrumentar primero y medir, con el límite configurado y muy holgado, que adivinar.

Lo que sí conviene desde el primer *endpoint* es la uniformidad del criterio de errores, porque es lo que después no se puede corregir en bloque.

### `ESC-2` — Exposición o migración

El sistema previo probablemente no tenga rate limiting, porque su superficie no estaba expuesta, y sí tenga mensajes de error verbosos, porque estaban destinados a operadores internos. Ambas cosas hay que resolverlas antes de exponer, no después.

El riesgo específico del escenario es que los identificadores internos se filtren al contrato: la clave primaria autoincremental de `TB_RESERVA_CAB` es el identificador más a mano y el peor candidato. Traducirlo cuesta y ese costo se declara, como todo el resto del costo de traducción del escenario.

Un caso frecuente y desagradable: sistemas heredados que devuelven el mensaje del motor de base de datos como mensaje de error de usuario. Al exponerlos por una API ese mensaje pasa a ser público.

### `ESC-3` — Evolución en producción

**Introducir un límite donde no lo había rompe a quien lo excedía sin saberlo.** El camino ordenado tiene tres pasos: medir el consumo real por consumidor, comunicar el límite con antelación a quienes lo excederían, y recién entonces aplicarlo. Publicar las cabeceras informativas antes de aplicar el rechazo permite que los consumidores se vean venir el problema.

**Endurecer una validación rompe.** [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) lo enumera entre las trampas del escenario: quien dependía de la laxitud deja de funcionar.

**Restringir un error que antes era informativo no rompe** en sentido técnico, pero degrada al consumidor que ramificaba sobre esa distinción. Cuando el motivo es de seguridad, se hace igual; conviene ofrecer a la vez la alternativa —un `code` estable en el cuerpo que distinga lo que se puede distinguir sin filtrar—.

### `ESC-4` — Evaluación de una API ajena

En `ESC-4a` se verifica que exista rate limiting y con qué código rechaza, que la configuración de CORS no combine comodín con credenciales, que la validación esté en la frontera y no dispersa, y que los manejadores de error no devuelvan trazas. La divergencia típica del escenario aparece acá con nitidez: la especificación declara `429` y el código emite `503` porque nadie tocó el default.

En `ESC-4b` las cabeceras son la fuente principal, y [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) lo señala: *«a menudo revelan el framework, la estrategia de caché y la de rate limiting»*. Un `503` ante volumen moderado, sin `Retry-After`, es un indicio bastante claro de un limitador nativo de ASP.NET Core sin configurar. La forma de los identificadores se observa en cualquier respuesta.

El límite ético rige con particular fuerza en esta sección. Caracterizar el comportamiento de una API ajena bajo carga, o probar sistemáticamente sus respuestas de error, requiere autorización explícita y no es distinguible desde el otro lado de lo que no es evaluación.

### Qué cambia por contexto

En **`CTX-1`** las cuatro decisiones están en su punto de máxima exigencia. El rate limiting es necesario y debe estar **publicado**: [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) lo registra así en su tabla comparativa. Los identificadores opacos son la opción por defecto y el criterio de errores es el más restrictivo, porque el consumidor no puede leer el código para entender qué pasó y tampoco se puede confiar en él.

En **`CTX-2`** el límite existe por protección y no por negocio, y su valor es contener la falla en cascada: un servicio que reintenta agresivamente contra otro degradado lo termina de tirar. La conversación sobre errores se relaja: dentro del perímetro, la claridad diagnóstica rinde más que la opacidad, siempre que los datos personales sigan protegidos.

En **`CTX-3`** el rate limiting es poco relevante según [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md), y CORS pasa al primer plano porque el consumidor es un navegador. La distinción entre Blazor *interactive server* —que llama desde el servidor y no atraviesa CORS— y WebAssembly o MAUI —que sí, o que no tienen origen en absoluto— determina qué hay que configurar.

En **`CTX-4`** el rol se invierte: el límite se padece en lugar de aplicarse. Lo que hay que diseñar es la reacción, y eso es [`TEM-RESIL`](Resiliencia-y-Reintentos.md).

---

## Ejemplos concretos

Sintéticos, del dominio de reserva de salas.

### Respuesta de rechazo por límite, bien formada

```http
GET /v1/salas/s-12/disponibilidad?desde=2026-08-01 HTTP/1.1
Host: api.salas.ejemplo.com
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9…
```

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 37
X-RateLimit-Limit: 600
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1785312037
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/limite-excedido",
  "title": "Límite de peticiones alcanzado",
  "status": 429,
  "detail": "Se permiten 600 peticiones por minuto. Reintente en 37 segundos.",
  "correlacion": "9f2c-4a11"
}
```

Las cabeceras `X-RateLimit-*` de este ejemplo son **convención de facto**, no estándar, y se muestran porque son las que los clientes existentes entienden. `Retry-After` sí es normativa (`N-01` §10.2.3). La forma que prescribe `F-02` es otra y su estatus es Internet-Draft:

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 37
RateLimit-Policy: "porminuto";q=600;qu="requests";w=60
RateLimit: "porminuto";r=0;t=37
```

Los parámetros de una letra —`q`, `qu`, `w`, `r`, `t`— no son una abreviatura del ejemplo: son los nombres que define el draft.

### El default del framework, corregido

```csharp
using System.Globalization;
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(opciones =>
{
    // Sin esta línea el rechazo es 503 Service Unavailable (N-43),
    // que le dice al consumidor que el servidor está caído.
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opciones.AddFixedWindowLimiter(policyName: "porminuto", limitador =>
    {
        limitador.PermitLimit = 600;
        limitador.Window = TimeSpan.FromMinutes(1);
        limitador.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limitador.QueueLimit = 0;
    });

    // Retry-After tampoco es automática: hay que leerla del lease.
    opciones.OnRejected = async (contexto, ct) =>
    {
        if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var esperar))
        {
            contexto.HttpContext.Response.Headers.RetryAfter =
                ((int)esperar.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        await contexto.HttpContext.Response.WriteAsync("Límite de peticiones alcanzado.", ct);
    };
});

var app = builder.Build();

app.UseRouting();
app.UseRateLimiter();        // N-42: después de UseRouting si hay políticas por endpoint
app.UseCors("ClientesPropios");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/v1/salas/{id}/disponibilidad", ConsultarDisponibilidad)
   .RequireRateLimiting("porminuto");
```

Dos observaciones sobre el fragmento. `MetadataName.RetryAfter` está dentro de un `TryGetMetadata` porque `N-42` advierte que no todos los limitadores la proveen; el limitador de concurrencia no tiene con qué calcularla. Y no hay asignación de `StatusCode` dentro de `OnRejected` porque `RejectionStatusCode` ya se fijó: `N-43` precisa que el código se establece **antes** de invocar `OnRejected`, de modo que lo que se asigne ahí gana sobre el default.

Sobre la aplicación por atributos, `N-42` documenta `[EnableRateLimiting("politica")]` y `[DisableRateLimiting]`, aplicables a controlador, método de acción o Razor Page pero **no** a *page handlers*, y que `[DisableRateLimiting]` gana sobre todo, incluido `RequireRateLimiting`. Es un mecanismo de exención potente y conviene que su uso sea revisable.

### CORS con orígenes explícitos

```csharp
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("ClientesPropios", politica =>
        politica.WithOrigins("https://app.salas.ejemplo.com",
                             "https://admin.salas.ejemplo.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
});
```

`AllowCredentials()` obliga a enumerar los orígenes: es exactamente la combinación que no debe hacerse con comodín. Y conviene recordar el alcance real de esta configuración: no protege la API de un cliente que no sea un navegador, porque ese cliente no tiene origen que declarar.

### Dos respuestas indistinguibles

La reserva `r-8842` existe y pertenece a otro usuario; `r-0000` no existe. Ambas peticiones vienen del mismo token válido:

```http
GET /v1/reservas/r-8842 HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9…
```

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/reserva-no-encontrada",
  "title": "Reserva no encontrada",
  "status": 404,
  "correlacion": "3b71-9d02"
}
```

```http
GET /v1/reservas/r-0000 HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9…
```

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/reserva-no-encontrada",
  "title": "Reserva no encontrada",
  "status": 404,
  "correlacion": "3b71-9d03"
}
```

Idénticas salvo el identificador de correlación, que es lo que permite a `ACT-03` reportar un caso concreto y al productor distinguirlos en sus logs. Toda la información diagnóstica vive del lado del servidor.

### Un error que filtra, y su corrección

Lo que no debe salir:

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/problem+json
Server: Kestrel

{
  "title": "An error occurred",
  "status": 500,
  "detail": "Microsoft.Data.SqlClient.SqlException: Violation of UNIQUE KEY constraint 'UQ_TB_RESERVA_CAB_SALA_FRANJA'. Cannot insert duplicate key in object 'dbo.TB_RESERVA_CAB'.\n   at Salas.Infra.Persistencia.ReservaRepository.GuardarAsync(Reserva r) in C:\\build\\src\\Salas.Infra\\Persistencia\\ReservaRepository.cs:line 214",
  "nodeId": "SALAS-PRD-03"
}
```

Cuatro filtraciones en un cuerpo: el motor y el esquema de la base de datos, la organización interna del código, la ruta de compilación y el nombre de la máquina. Además el `500` es incorrecto: un solapamiento de reservas es un conflicto de dominio conocido y su código es `409`.

```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/sala-ocupada",
  "title": "La sala no está disponible en la franja solicitada",
  "status": 409,
  "detail": "Existe otra reserva para la sala s-12 entre las 10:00 y las 11:30 del 2026-08-04.",
  "correlacion": "7c14-2f80"
}
```

El `detail` es informativo y no revela nada que el solicitante no pueda deducir consultando la disponibilidad de la sala, que es una operación que ya tiene permitida. Ese es el criterio: **es publicable lo que el llamante podría obtener por una vía legítima**.

---

## Preguntas guía

- ¿Nuestro rate limiter devuelve `429` o `503`? ¿Alguien lo verificó contra el servicio corriendo, o se asumió?
- ¿Emitimos `Retry-After`? ¿Y el limitador que usamos provee la metadata para calcularla?
- ¿Con qué cabeceras comunicamos los límites, y sabemos cuál es su estatus normativo?
- ¿Por qué clave está particionado el límite? ¿Qué le pasa a una oficina entera detrás de una sola salida a internet?
- ¿Alguien en el equipo cree que CORS protege la API? ¿Qué pasaría si un cliente llamara con `curl`?
- ¿Nuestra política de CORS combina comodín de origen con credenciales?
- ¿Dónde está la frontera de validación, y es una sola?
- Ante un recurso ajeno, ¿devolvemos `403` o `404`? ¿Es uniforme en todos los endpoints del recurso?
- ¿Qué se aprende de nuestro sistema leyendo cien respuestas de error distintas?
- ¿Nuestros identificadores públicos permiten estimar volumen o adivinar el siguiente?
- Si un `IExceptionHandler` devuelve `true`, ¿seguimos registrando la excepción en el log? (`N-29`, cambio de .NET 10.)

---

## Criterios de calidad

Una aplicación buena tiene límites que alguien eligió a partir de tráfico medido, comunicados con cabeceras cuyo estatus el equipo conoce, y un `429` con `Retry-After` verificado contra el servicio en ejecución. Su política de CORS enumera orígenes. Su validación está en un solo lugar y antes del dominio. Sus errores son uniformes, informativos hasta donde puede y opacos donde debe, y llevan un identificador de correlación.

Una aplicación pobre suele tener todos los mecanismos presentes y ninguno verificado: el rate limiter configurado que rechaza con `503`, la política de CORS con comodín heredada de una sesión de depuración, la validación duplicada en dos capas que ya divergieron, y errores que en producción devuelven lo mismo que en desarrollo porque nadie miró.

### Antipatrones

**El limitador dejado en su default.** Rechaza con `503`, sin `Retry-After`, y con eso instruye a todos sus consumidores a comportarse como si el servidor estuviera caído. Es el antipatrón que da nombre a la mitad de este documento y su corrección cuesta una línea.

**Las cabeceras de rate limiting presentadas como estándar.** Prescribir `X-RateLimit-*` citando «el estándar de rate limiting» o los nombres largos citando `F-02` sin advertir que corresponden a una revisión superada del draft.

**CORS como control de seguridad.** Configurar orígenes y considerar la API protegida. Un cliente sin navegador ignora la configuración por completo.

**El comodín de CORS con credenciales.** Habitualmente introducido para desarmar un problema de desarrollo y nunca revertido.

**El error verboso en producción.** Trazas de excepción, mensajes del motor de base de datos y rutas de compilación en el cuerpo de la respuesta. La variante moderna es la contraria y la introduce .NET 10: suprimir el log **y** la respuesta, con lo cual el incidente no queda registrado en ninguna parte (`N-29`).

**El `403` explícito sobre identificadores secuenciales.** La combinación es peor que cualquiera de sus dos partes: el código confirma la existencia y el identificador dice cuál probar después.

**El `404` uniforme aplicado sin criterio.** Aplicado también a recursos cuya existencia el usuario ya conoce, produce un cliente desconcertado sin proteger nada. La opacidad tiene costo y hay que gastarla donde rinde.

**La validación en la capa equivocada.** Validar en el manejador de un endpoint y no en otro, o validar solamente en el cliente. Un dato no validado que llega al dominio ya es un dato en el que el resto del código confía.

**El límite fijado sin medir.** Un número elegido por intuición o bien no protege o bien corta usos legítimos, y en ambos casos nadie se entera hasta que hay un incidente.

---

## Anexo — Checklist de revisión de seguridad de una API

La lista completa, que abarca las tres áreas de la familia, está en el anexo de [`TEM-AUTH`](Autenticacion-y-Autorizacion.md) y es la que `ACT-07` firma. Lo que sigue amplía las secciones que corresponden a este documento, para revisar la superficie por separado.

```yaml
rate_limiting:
  - activo: si | no
  - algoritmo: ventana-fija | ventana-deslizante | token-bucket | concurrencia
  - clave_de_particion: token | cuenta | ip | mixta
  - limite_derivado_de_trafico_medido: si | no
  - codigo_de_rechazo_verificado_en_ejecucion: "429" | "503"   # el default es 503
  - retry_after_emitido: si | no
  - metadata_retry_after_disponible_en_este_limitador: si | no
  - cabeceras_informativas: "X-RateLimit-*" | "RateLimit (F-02)" | ninguna
  - estatus_de_esas_cabeceras_declarado_en_la_doc: si | no
  - limite_publicado_para_el_consumidor: si | no               # obligatorio en CTX-1
  - endpoint_de_autenticacion_limitado_aparte: si | no
  - orden_del_middleware_verificado: si | no                   # UseRateLimiter tras UseRouting

cors:
  - politica_con_origenes_explicitos: si | no
  - comodin_combinado_con_credenciales: si | no                # debe ser "no"
  - politicas_de_desarrollo_ausentes_en_produccion: si | no
  - orden_verificado: si | no                                  # UseCors antes de UseAuthentication
  - equipo_entiende_que_no_protege_a_clientes_sin_navegador: si | no

validacion:
  - frontera_unica_y_previa_al_dominio: si | no
  - campos_no_reconocidos: rechazados | descartados            # decisión declarada
  - propiedades_duplicadas_rechazadas: si | no                 # AllowDuplicateProperties, .NET 10
  - limites_de_tamano_definidos: si | no                       # longitud, cardinalidad, profundidad, cuerpo
  - parametros_nullable_de_minimal_api_revisados: si | no      # N-65: no se validan
  - mensajes_de_error_sin_reflejar_la_entrada: si | no

respuestas_de_error:
  - criterio_403_vs_404_declarado: si | no
  - criterio_uniforme_en_todas_las_operaciones_del_recurso: si | no
  - sin_traza_de_excepcion: si | no
  - sin_detalles_del_almacenamiento: si | no
  - sin_versiones_de_plataforma_ni_bibliotecas: si | no
  - sin_rutas_de_compilacion_ni_nombres_de_maquina: si | no
  - sin_datos_de_otros_usuarios: si | no
  - identificador_de_correlacion_presente: si | no
  - excepciones_manejadas_siguen_registrandose_en_log: si | no # N-29, cambio de .NET 10

identificadores:
  - forma: secuencial | opaco
  - permite_estimar_volumen: si | no
  - permite_adivinar_el_siguiente: si | no
  - comprobacion_de_titularidad_presente_igualmente: si | no   # la opacidad no autoriza
```

El campo `codigo_de_rechazo_verificado_en_ejecucion` está redactado así a propósito. Leer la configuración no alcanza: la única comprobación que vale es alcanzar el límite contra el servicio corriendo y mirar qué llega.
