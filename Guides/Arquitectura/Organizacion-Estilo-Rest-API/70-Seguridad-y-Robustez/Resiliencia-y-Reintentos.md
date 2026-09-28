---
doc_id: TEM-RESIL
doc_type: tema
title: Resiliencia y reintentos
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-SEG, TEM-AUTH, TEM-PROT, TEM-IDEM, TEM-STATUS, TEM-HEADERS, TEM-ERR, TEM-CONSUMO, MARCO-ACTORES, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Resiliencia y reintentos — `TEM-RESIL`

## Resumen ejecutivo

Los otros dos documentos de esta familia miran la API desde adentro. Este la mira desde afuera, que es donde vive `ACT-03` y donde `CTX-4` pone a la mitad de los equipos: la pasarela de pagos, el ERP corporativo, el servicio de facturación del organismo fiscal son APIs que no se diseñan, se consumen, y cuyo comportamiento ante el fallo es un dato y no una decisión.

La pregunta central es engañosamente simple. La API no respondió: ¿reintento? La respuesta correcta no depende de la política de reintentos sino de una propiedad de la operación que [`TEM-IDEM`](../30-Semantica-HTTP/Idempotencia-y-Concurrencia.md) define: si repetirla produce el mismo efecto que ejecutarla una vez. Reintentar un `GET` es gratis; reintentar el `POST` que confirma una reserva puede producir dos reservas y un cliente enojado. Y el caso que hace difícil el problema no es el error limpio sino el *timeout*, donde el consumidor **no sabe** si la operación se ejecutó.

El documento cierra invirtiendo el punto de vista, porque la resiliencia del consumidor depende de lo que el productor le diga. Un `500` genérico ante un conflicto de dominio no le permite a nadie decidir bien; un `429` con `Retry-After` sí. Los códigos de estado que emite una API son la interfaz con la que sus consumidores construyen sus políticas de fallo, y por eso son una decisión de diseño de contrato y no un detalle de implementación.

---

## Definición

### Qué es

**Resiliencia** es la capacidad de un sistema de seguir prestando servicio, quizá degradado, cuando una de sus dependencias falla. En el consumo de APIs se materializa en un conjunto acotado de estrategias: reintentos, *backoff*, *circuit breaker*, *timeouts* y degradación.

### Qué problema resuelve

Que la red falla y los servicios remotos también. En un sistema con una sola dependencia externa el problema es visible; en `CTX-2`, con una red de llamadas entre servicios, deja de ser evidente cuál falló y aparece el modo de fallo que da nombre al tema: la **falla en cascada**, donde un servicio degradado consume los recursos de quienes lo llaman —que quedan esperando— hasta tirarlos también.

### Qué no es

**No es alta disponibilidad.** Redundar instancias reduce la probabilidad de que una dependencia falle; la resiliencia trata qué hacer cuando falla igual.

**No es reintentar más veces.** Es el malentendido operativamente más caro del tema. Reintentar agresivamente contra un servicio degradado agrega carga justo cuando menos puede absorberla, y convierte una falla transitoria en una sostenida. Buena parte de las estrategias de este documento existen para **reintentar menos y mejor**, no más.

**No es un sustituto de la idempotencia.** Ninguna política de reintentos vuelve segura una operación que no lo es. La propiedad está en la operación.

**No reemplaza el manejo de errores.** Cuando se agotaron los reintentos hay que decidir qué se le dice al usuario, y esa es una decisión de producto que `ACT-05` y `ACT-06` tienen que haber tomado antes.

---

## El problema desde el lado del consumidor

Cuando una petición no devuelve la respuesta esperada, el consumidor está en uno de tres estados, y son muy distintos entre sí.

**Sabe que no se ejecutó.** La conexión fue rechazada, la resolución de nombres falló, o el servidor respondió `503` antes de tocar el dominio. Reintentar es seguro con independencia del método.

**Sabe que se ejecutó y falló.** El servidor respondió `409` o `422`: la petición llegó, se procesó y el resultado es un rechazo con significado. Reintentar lo mismo no cambia nada.

**No sabe.** El *timeout* expiró, o la conexión se cortó después de enviar. Es el caso interesante y el que gobierna el diseño: **el silencio no distingue entre «no llegó» y «se ejecutó y se perdió la respuesta»**. Una reserva pudo haberse creado. Reintentar puede duplicarla; no reintentar puede dejar al usuario creyendo que no reservó cuando sí lo hizo.

De ese tercer estado no se sale con más información sobre lo ocurrido, porque no la hay. Se sale haciendo que la repetición sea inofensiva, y eso es idempotencia.

`ACT-03` es quien enfrenta esto, y [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md) registra entre las preguntas que debe poder responder una que apunta directo al tema: *¿qué hace mi cliente cuando la API responde `429` o `503`?* Que sean dos códigos distintos en la misma pregunta no es casual: uno pide esperar y el otro reintentar, y confundirlos —cosa que el default de `N-43` favorece, como desarrolla [`TEM-PROT`](Proteccion-de-la-Superficie.md)— produce clientes que se comportan al revés.

---

## Reintentos y cuándo son seguros

### La dependencia de la idempotencia

`N-01` §9.2.2 define idempotentes a los métodos safe más `PUT` y `DELETE`. `POST` y `PATCH` no lo son. La regla operativa que se sigue de ahí es directa:

| Método | Idempotente según `N-01` | Reintento automático |
|---|---|---|
| `GET`, `HEAD`, `OPTIONS` | Sí | Seguro |
| `PUT` | Sí | Seguro |
| `DELETE` | Sí | Seguro |
| `POST` | No | Solo con clave de idempotencia |
| `PATCH` | No | Solo con clave de idempotencia o `If-Match` |

`N-51` refleja exactamente esta distinción en el ecosistema .NET: el *pipeline* estándar de resiliencia ofrece `options.Retry.DisableForUnsafeHttpMethods()` y `options.Retry.DisableFor(HttpMethod.Post, HttpMethod.Delete)` precisamente porque la política por defecto no puede saber si el `POST` de esta aplicación es seguro de repetir.

La salida para `POST` es la clave de idempotencia, y su estatus normativo merece precisión. `F-01` —`draft-ietf-httpapi-idempotency-key-header`, revisión -07 del 2025-10-15— **expiró sin llegar a RFC**. Lo que existe es una convención de facto impuesta por Stripe (`P-04`) y adoptada por el ecosistema de pagos: la cabecera es perfectamente utilizable, y lo que no es utilizable es el argumento de autoridad. `P-04` documenta además la conducta que da forma al mecanismo: Stripe compara *endpoint* y parámetros además de la clave, y responde `409 Conflict` cuando una misma clave se reutiliza con una petición distinta. El desarrollo completo está en [`TEM-IDEM`](../30-Semantica-HTTP/Idempotencia-y-Concurrencia.md).

### Qué se reintenta

No todo fallo merece reintento, y la distinción es entre lo que puede resolverse solo y lo que no. `N-51` documenta que el *pipeline* estándar de .NET reintenta ante HTTP `≥500`, `408` y `429`, más `HttpRequestException` y `TimeoutRejectedException`.

Los `4xx` restantes no se reintentan porque describen un problema de la petición: repetir un `400` produce otro `400`. Dos excepciones merecen nota. El `401` admite **un** reintento, pero solo después de renovar el token, y por lo tanto no es un reintento de la política general sino un paso del flujo de autenticación de [`TEM-AUTH`](Autenticacion-y-Autorizacion.md). Y el `429` se reintenta, pero **respetando `Retry-After`**: es el único caso donde el servidor dijo explícitamente cuánto esperar y desoírlo es garantía de volver a chocar.

---

## Backoff exponencial y jitter

Reintentar de inmediato es la peor opción disponible: agrega carga al servicio en el momento exacto en que menos puede absorberla. El *backoff* exponencial espacia los intentos multiplicando el intervalo —un segundo, dos, cuatro, ocho— de modo que el consumidor cede terreno a medida que la evidencia de degradación se acumula.

El *jitter* resuelve un problema de segundo orden que solo aparece a escala y que sorprende cuando se descubre. Si mil clientes fallan simultáneamente por una caída de dos segundos y todos aplican el mismo *backoff* determinista, los mil reintentan **a la vez**, y otra vez al doble del intervalo. El servicio recibe olas sincronizadas justo mientras intenta recuperarse. Agregar una componente aleatoria al intervalo dispersa esas olas.

`N-51` documenta que el *pipeline* estándar de .NET trae *jitter* activado por defecto —`jitter: true`— con tres reintentos y retardo base de dos segundos. Es un buen default y conviene no desactivarlo sin una razón.

`Retry-After` tiene precedencia sobre cualquier cálculo propio. Si el servidor indicó cuánto esperar, esperar menos es ignorar la única información autorizada que hay sobre el estado del otro lado. La cabecera está definida en `N-01` §10.2.3 y la trata [`TEM-HEADERS`](../30-Semantica-HTTP/Cabeceras-y-Negociacion.md); admite tanto un número de segundos como una fecha HTTP, y un cliente robusto acepta ambas formas.

> `N-01` §10.2.3 no se consultó individualmente en la verificación de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md); se marca según la convención de trazabilidad.

---

## Circuit breaker

Los reintentos suponen que la falla es transitoria. Cuando deja de serlo, seguir intentando desperdicia recursos del consumidor y prolonga la agonía del servicio caído. El *circuit breaker* introduce la decisión de **dejar de intentar por un rato**.

```mermaid
stateDiagram-v2
    [*] --> Cerrado
    Cerrado --> Abierto: la proporción de fallos<br/>supera el umbral<br/>en la ventana de muestreo
    Abierto --> Semiabierto: transcurrió<br/>la duración de corte
    Semiabierto --> Cerrado: la petición de prueba<br/>tuvo éxito
    Semiabierto --> Abierto: la petición de prueba<br/>volvió a fallar

    note right of Cerrado
        Las peticiones pasan.
        Se cuentan los fallos.
    end note
    note right of Abierto
        Las peticiones fallan de inmediato,
        sin tocar la red.
    end note
    note right of Semiabierto
        Pasa una petición de prueba.
        El resto sigue rechazándose.
    end note
```

El valor del estado **abierto** es doble y ambas mitades importan. Hacia afuera, deja de golpear a un servicio que no está en condiciones. Hacia adentro, el consumidor **falla rápido**: en lugar de acumular peticiones esperando un *timeout* de treinta segundos, responde de inmediato y libera los recursos que de otro modo se agotarían. Esa segunda mitad es la que contiene la falla en cascada de `CTX-2`.

Los defaults del *pipeline* estándar de .NET, según `N-51`, son proporción de fallos del 10 %, umbral mínimo de cien peticiones, ventana de muestreo de treinta segundos y corte de cinco segundos. El umbral mínimo merece atención: con menos de cien peticiones en la ventana el interruptor no abre, lo cual evita que un servicio de bajo tráfico se corte por dos fallos aislados.

La granularidad correcta es **por dependencia**, no por aplicación. Que la pasarela de pagos esté caída no debe impedir consultar la disponibilidad de las salas. `N-51` documenta que el *pipeline* de *hedging* mantiene un conjunto de interruptores por autoridad de URL mediante `SelectPipelineByAuthority`.

---

## Timeouts

Un *timeout* ausente no significa esperar mucho: significa **esperar indefinidamente**, y es la forma más común de agotar los recursos de un consumidor. Cada petición colgada retiene una conexión y un hilo lógico; con suficientes, la aplicación deja de responder por una causa que ocurrió en otro sistema.

`N-51` distingue dos niveles en el *pipeline* estándar, y la distinción es la parte útil:

- **Timeout total** (30 s por defecto): la operación completa, incluidos todos los reintentos. Es lo que percibe quien llamó.
- **Timeout por intento** (10 s por defecto): cada petición individual.

Sin el segundo, un solo intento lento consume todo el presupuesto y no queda margen para reintentar. Sin el primero, tres intentos de diez segundos con *backoff* producen una espera que ningún usuario tolera. Ambos se derivan hacia atrás desde lo único que es un requisito real: cuánto está dispuesto a esperar quien inició la operación.

Un detalle de implementación que confunde a quien lo encuentra por primera vez: **Polly v8 lanza `TimeoutRejectedException`, no `System.TimeoutException`**. Un `catch (TimeoutException)` no captura los *timeouts* del *pipeline* de resiliencia.

---

## Degradación

La pregunta final de toda esta cadena es qué se hace cuando la dependencia sigue sin responder. Hay tres respuestas y elegir entre ellas es una decisión de producto, no técnica.

**Fallar.** Correcto cuando la operación no tiene sentido sin la dependencia. Si la pasarela de pagos no responde, no se confirma la seña. Lo que hay que cuidar es que el fallo sea comprensible y que no deje al sistema en un estado ambiguo.

**Degradar.** Servir un resultado menos bueno pero utilizable: la disponibilidad de las salas desde una caché de cinco minutos, con la antigüedad declarada. Depende de que alguien haya decidido que ese resultado es aceptable, y esa decisión no le corresponde a quien escribe el cliente HTTP.

**Diferir.** Aceptar la operación, registrarla y completarla cuando la dependencia vuelva, respondiendo `202 Accepted` con una forma de consultar el progreso. Es la respuesta más robusta y la más cara: introduce estado, reconciliación y un modo de fallo nuevo, el de la operación que nunca se completa.

Lo que no es una opción es que la decisión quede implícita en un bloque `catch`. Una degradación no diseñada es un comportamiento que nadie especificó y que aparece por primera vez durante un incidente.

---

## Desde el lado del productor

La resiliencia del consumidor depende por completo de la calidad de la información que recibe. Un productor que devuelve códigos precisos le está entregando a `ACT-03` el material con el que decide; uno que devuelve `500` ante todo lo obliga a adivinar, y adivinar en esta materia significa reintentar cosas que no debía o rendirse ante cosas que se resolvían solas.

| Código | Qué le dice al consumidor | Conducta que habilita |
|---|---|---|
| `408` | La petición tardó demasiado en llegar | Reintentar |
| `409` | Conflicto de estado del dominio | No reintentar; resolver y reenviar |
| `422` | La petición es sintácticamente válida y no se puede procesar | No reintentar |
| `429` | Superaste el límite; esperá lo que dice `Retry-After` | Esperar y reintentar |
| `500` | Algo falló del lado del servidor, sin más detalle | Reintentar con cautela |
| `502`, `504` | Falló un intermediario o una dependencia | Reintentar |
| `503` | El servidor no está disponible ahora | Reintentar, idealmente con `Retry-After` |

El significado normativo de cada uno lo fija [`TEM-STATUS`](../30-Semantica-HTTP/Codigos-de-Estado.md); acá interesa la conducta que induce. De la tabla se siguen tres obligaciones del productor.

**Distinguir `429` de `503`.** Es la fila donde el ecosistema .NET pone una trampa. `N-43` documenta que el rate limiter nativo rechaza con `503` por defecto: el consumidor lee «servidor caído», reintenta y vuelve a chocar contra el límite. Corregirlo a `429` es una línea y la desarrolla [`TEM-PROT`](Proteccion-de-la-Superficie.md).

**Emitir `Retry-After` siempre que se sepa la respuesta.** Es la única forma de que el consumidor espere lo correcto en lugar de estimarlo. Vale tanto para `429` como para `503` planificado.

**No devolver `500` ante condiciones conocidas.** El solapamiento de reservas es `409`; el `500` genérico convierte una condición de negocio prevista en un fallo indeterminado que el consumidor va a reintentar sin sentido. Es además el caso donde más información se filtra, según [`TEM-PROT`](Proteccion-de-la-Superficie.md).

Hay una cuarta obligación, menos evidente: **soportar la clave de idempotencia en las operaciones que la necesiten**. Un productor que no la acepta le está negando al consumidor la posibilidad de reintentar con seguridad, y con eso lo condena a elegir entre duplicar o perder.

---

## Aplicación por escenario

### `ESC-1` — API nueva

Como productor, lo que se decide es qué códigos emite cada camino de fallo —incluidos los que nadie probó, que es la pregunta que [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md) le asigna a `ACT-02`— y qué operaciones aceptan clave de idempotencia. Ambas cosas son baratas ahora y caras después: agregar soporte de idempotencia a un `POST` publicado es fácil, pero los consumidores que ya construyeron su cliente sin ella no la van a adoptar.

Como consumidor de las dependencias de la API nueva, el momento es igual de bueno, porque el *pipeline* de resiliencia se configura una vez en el registro del cliente HTTP y después se hereda.

La trampa es la de siempre en este escenario, con una forma específica: montar *hedging*, interruptores por dependencia y degradación con caché para una API que llama a un solo servicio interno.

### `ESC-2` — Exposición o migración

El sistema previo puede tener reintentos escritos a mano y dispersos, o —más frecuente— no tener ninguno porque las llamadas eran locales. Al convertir una llamada en proceso en una llamada de red aparece un modo de fallo que antes no existía, y esa transformación es exactamente lo que hace el escenario.

El riesgo específico: una operación que era transaccional dentro del sistema deja de serlo al cruzar la red. Lo que era una transacción de base de datos pasa a ser dos llamadas que pueden fallar por separado, y ahí la idempotencia deja de ser teoría.

### `ESC-3` — Evolución en producción

Cambiar el código de estado que devuelve una condición conocida **rompe** a los consumidores que ramifican sobre él, aunque el nuevo código sea el correcto. Corregir un `500` a `409` es una mejora del contrato y un cambio rompiente al mismo tiempo, y se trata como tal según [`TEM-BREAK`](../50-Evolucion-y-Versionado/Compatibilidad-y-Cambios-Rompientes.md).

Del lado del consumidor, el escenario tiene un cambio típico: los *timeouts* configurados hace dos años dejaron de corresponder al comportamiento actual de la dependencia. Los valores de resiliencia envejecen y nadie los revisa, porque no fallan de forma visible: producen esperas largas o cortes prematuros que se atribuyen a otra cosa.

### `ESC-4` — Evaluación de una API ajena

Es el escenario donde este documento es más directamente aplicable, porque coincide con `CTX-4`. Lo que hay que determinar antes de integrarse: qué códigos emite ante cada fallo, si emite `Retry-After`, si acepta clave de idempotencia en las operaciones no idempotentes, qué límites tiene publicados y qué garantías de disponibilidad ofrece por escrito.

En `ESC-4a` la especificación responde parte y hay que contrastarla con el código, porque los caminos de fallo son justamente los que más divergen entre lo declarado y lo implementado. En `ESC-4b` se observa: qué llega ante una petición mal formada, qué cabeceras acompañan un rechazo por límite, si `Retry-After` está presente. Un `503` sin `Retry-After` ante volumen moderado sugiere un limitador sin configurar del otro lado, y esa hipótesis cambia la política de reintentos que conviene escribir.

El límite ético aplica: caracterizar el comportamiento de una API ajena bajo carga requiere autorización explícita.

### Qué cambia por contexto

En **`CTX-1`**, como productor, las garantías se publican y se cumplen; los consumidores construyen su resiliencia contra lo documentado y descubren la diferencia en producción. Como consumidor de una API pública ajena, no hay canal para negociar: se diseña contra el peor comportamiento observado.

En **`CTX-2`** aparece la falla en cascada como riesgo dominante, y el *circuit breaker* pasa de ser prolijidad a ser necesario. [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) registra la resiliencia ante fallas parciales como preocupación propia de este contexto, junto con la observabilidad distribuida: sin propagación de contexto de traza, determinar cuál de los seis servicios de la cadena falló es adivinanza.

En **`CTX-3`** la red es hostil por naturaleza. Una aplicación MAUI en un teléfono pierde conectividad de forma rutinaria, y el *timeout* que es razonable en un centro de datos es absurdo en una conexión móvil. Blazor en *interactive server* está del otro lado de esa frontera: la llamada sale del servidor y se comporta como `CTX-2`. El detalle de consumo desde ambos clientes lo trata [`TEM-CONSUMO`](../80-Implementacion-en-NET/Consumo-desde-Blazor-y-MAUI.md).

En **`CTX-4`** este documento **es** el trabajo. [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) lo enuncia sin rodeos: las preguntas se desplazan a qué garantías reales ofrece el otro lado, qué pasa cuando no responde y cómo se reintenta sin duplicar operaciones, *«la idempotencia deja de ser teoría y se vuelve el problema central»*. La preocupación propia del contexto es el aislamiento, y la resiliencia vive en esa capa de aislamiento: si el *circuit breaker* de la pasarela de pagos está disperso por el dominio, cambiar de pasarela deja de ser una decisión comercial.

---

## Ejemplos concretos

Sintéticos, del dominio de reserva de salas.

### El productor comunica bien un rechazo por límite

```http
POST /v1/reservas HTTP/1.1
Host: api.salas.ejemplo.com
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9…
Idempotency-Key: 8f3c1e94-2a77-4b10-9d51-6e2c0b4a7f31
Content-Type: application/json

{ "salaId": "s-12", "desde": "2026-08-04T10:00:00Z", "hasta": "2026-08-04T11:30:00Z" }
```

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 12
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/limite-excedido",
  "title": "Límite de peticiones alcanzado",
  "status": 429,
  "correlacion": "5d80-1c44"
}
```

El consumidor tiene todo lo que necesita: el código dice que espere, `Retry-After` dice cuánto, y `Idempotency-Key` garantiza que el reintento no duplique la reserva. Compárese con lo que produce un limitador sin configurar:

```http
HTTP/1.1 503 Service Unavailable
Content-Type: text/plain

Too many requests.
```

El mismo hecho, comunicado de forma que induce la conducta equivocada: sin `Retry-After`, con un código que dice «el servidor está caído» y un cuerpo que dice otra cosa y que nadie va a parsear.

### El caso ambiguo

```http
POST /v1/reservas HTTP/1.1
Idempotency-Key: 8f3c1e94-2a77-4b10-9d51-6e2c0b4a7f31
…
```

No hay respuesta: el *timeout* del cliente expira a los diez segundos. La reserva pudo haberse creado. Con la misma clave, el reintento es seguro y el productor devuelve el resultado de la operación original:

```http
HTTP/1.1 201 Created
Location: /v1/reservas/r-9013
Content-Type: application/json

{ "id": "r-9013", "salaId": "s-12", "estado": "confirmada" }
```

Sin `Idempotency-Key` no hay decisión correcta, solo dos incorrectas: reintentar y arriesgar la reserva duplicada, o no reintentar y arriesgar que el usuario crea que no reservó. Que la cabecera sea convención de facto sostenida por `P-04` y no norma vigente —`F-01` expiró— no le quita utilidad; se la presenta como lo que es.

### Cliente resiliente en .NET

Paquete `Microsoft.Extensions.Http.Resilience`, construido sobre `Microsoft.Extensions.Resilience` y Polly, encadenado sobre `IHttpClientBuilder` (`N-51`, `N-52`).

```csharp
builder.Services
    .AddHttpClient<PasarelaDePagosClient>(cliente =>
    {
        cliente.BaseAddress = new Uri("https://pagos.proveedor.ejemplo.com/");
        cliente.Timeout = Timeout.InfiniteTimeSpan;   // el pipeline gobierna los tiempos
    })
    .AddStandardResilienceHandler(opciones =>
    {
        opciones.TotalRequestTimeout.Timeout   = TimeSpan.FromSeconds(20);
        opciones.AttemptTimeout.Timeout        = TimeSpan.FromSeconds(6);
        opciones.Retry.MaxRetryAttempts        = 3;

        // El cobro de la seña no es idempotente del lado del proveedor:
        // el reintento automático de POST se desactiva y se maneja explícitamente.
        opciones.Retry.DisableFor(HttpMethod.Post);
    });
```

Las cinco estrategias del *pipeline* estándar, en orden de exterior a interior según `N-51`, con sus defaults:

| Orden | Estrategia | Default |
|---|---|---|
| 1 | Rate limiter | Cola `0`, permisos `1_000` |
| 2 | Timeout total | 30 s |
| 3 | Retry | 3 intentos, exponencial, *jitter* activado, retardo base 2 s |
| 4 | Circuit breaker | Proporción de fallos 10 %, mínimo 100 peticiones, muestreo 30 s, corte 5 s |
| 5 | Timeout por intento | 10 s |

`N-51` es normativo sobre un punto que se viola con facilidad: *«you should only add one resilience handler and avoid stacking handlers.»* Apilar dos manejadores multiplica los reintentos —tres del interno por tres del externo son nueve peticiones— y produce exactamente la amplificación de carga que el mecanismo venía a evitar. `RemoveAllResilienceHandlers()` existe para el caso en que haya que empezar de cero.

Sobre el `HttpClient` subyacente, `N-50` documenta lo esencial de `IHttpClientFactory`: vida del *handler* de dos minutos por defecto, ajustable con `SetHandlerLifetime`, y la razón de existir del componente —*«Creating more handlers than necessary can result in socket exhaustion and connection delays. Some handlers also keep connections open indefinitely, which can prevent the handler from reacting to DNS changes.»*—. Dos advertencias documentadas y poco conocidas: si la aplicación necesita cookies, `N-50` **recomienda evitar** `IHttpClientFactory`, porque el agrupamiento de *handlers* comparte los `CookieContainer`; y capturar un cliente tipado en un *singleton* anula la reacción a cambios de DNS, que es justamente uno de los propósitos del componente.

Nota de estado necesaria: **`Microsoft.Extensions.Http.Polly` está deprecado**, y `N-52` remite a `Microsoft.Extensions.Resilience` o `Microsoft.Extensions.Http.Resilience`. El matiz es que sigue publicándose y recibiendo *servicing*: un proyecto que lo use no se rompe, pero está sobre un camino cerrado.

### Reintento explícito con clave de idempotencia

Para el `POST` que se excluyó de la política automática:

```csharp
public async Task<ResultadoCobro> CobrarSenaAsync(
    Guid reservaId, decimal monto, CancellationToken ct)
{
    // La clave se deriva de la operación de negocio, no se genera por intento:
    // un reintento debe presentar la MISMA clave que el intento original.
    var clave = DeterministicKey.Para(reservaId, monto);

    for (var intento = 1; intento <= 3; intento++)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, "cobros")
        {
            Content = JsonContent.Create(new { reservaId, monto })
        };
        peticion.Headers.Add("Idempotency-Key", clave.ToString());

        try
        {
            var respuesta = await _http.SendAsync(peticion, ct);

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await EsperarSegunRetryAfter(respuesta, intento, ct);
                continue;
            }

            if ((int)respuesta.StatusCode >= 500)
            {
                await EsperarConBackoffYJitter(intento, ct);
                continue;
            }

            return await LeerResultado(respuesta, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // No se sabe si la operación se ejecutó. El reintento es seguro
            // únicamente porque la clave de idempotencia es la misma.
            if (intento == 3) throw;
            await EsperarConBackoffYJitter(intento, ct);
        }
    }

    throw new CobroNoConfirmadoException(reservaId);
}
```

El punto del ejemplo es el comentario sobre la clave. Generarla por intento —lo que ocurre si se la construye con `Guid.NewGuid()` dentro del bucle— destruye por completo el mecanismo: cada reintento se presenta como una operación nueva y el proveedor cobra tantas veces como se intente. Es un error frecuente y silencioso, porque el código funciona perfectamente mientras no haya fallos.

La excepción final tampoco es decorativa. `CobroNoConfirmadoException` deja registrado el estado que corresponde: no «falló el cobro», sino «no sabemos si el cobro ocurrió», que es un estado del dominio que alguien tiene que reconciliar.

---

## Preguntas guía

- Para cada operación que consumimos, ¿es idempotente? Si no lo es, ¿podemos reintentarla con seguridad, y con qué mecanismo?
- Cuando una petición se corta por *timeout*, ¿nuestro código distingue «no se ejecutó» de «no sabemos»? ¿Y qué hace en el segundo caso?
- ¿La clave de idempotencia se genera una vez por operación de negocio o una vez por intento?
- ¿Tenemos *timeout* en todas las llamadas salientes? ¿Y de dónde salieron esos números?
- ¿Qué pasa hoy si una dependencia deja de responder por diez minutos? ¿Nuestra aplicación se degrada o se cae?
- ¿Hay más de un manejador de resiliencia apilado sobre el mismo cliente? (`N-51` lo desaconseja.)
- Como productor: ¿cada camino de fallo devuelve un código que le permita al consumidor decidir? ¿Cuántos devuelven `500` por omisión?
- ¿Emitimos `Retry-After` cuando sabemos la respuesta?
- ¿Nuestro rate limiter dice `429` o `503`? Es la misma pregunta de [`TEM-PROT`](Proteccion-de-la-Superficie.md), vista desde el consumidor.
- ¿Alguien probó el comportamiento de nuestro cliente con la dependencia apagada, o solo con ella funcionando?

---

## Criterios de calidad

Una aplicación buena tiene un solo manejador de resiliencia por cliente, con valores derivados de lo que el usuario final está dispuesto a esperar; reintenta solo lo que es seguro reintentar y lo declara explícitamente; respeta `Retry-After`; tiene un interruptor por dependencia; y decidió de antemano qué hace cuando se agota el presupuesto. Del lado productor, emite códigos precisos, acompaña los rechazos por límite con `Retry-After` y acepta clave de idempotencia donde corresponde.

Una aplicación pobre suele tener la configuración correcta y ninguna evidencia de que funcione, porque el camino que ejercita la resiliencia es justamente el que nadie prueba. La forma más común del defecto no es la ausencia de política sino la política nunca ejecutada.

### Antipatrones

**Reintentar lo que no es idempotente.** Un `POST` en la política general de reintentos, sin clave de idempotencia. Produce reservas duplicadas y cobros repetidos, y solo se manifiesta bajo la condición que nadie reproduce en desarrollo.

**La clave de idempotencia generada por intento.** Anula el mecanismo conservando toda su apariencia.

**Reintentar sin *backoff*.** Tres intentos inmediatos contra un servicio degradado son tres veces la carga en el peor momento.

**El *backoff* sin *jitter* a escala.** Sincroniza a todos los clientes en olas que impiden la recuperación. `N-51` lo trae activado por defecto; desactivarlo requiere una razón.

**Ignorar `Retry-After`.** Calcular la propia espera cuando el servidor dijo cuánto esperar, y volver a chocar contra el límite.

**Manejadores de resiliencia apilados.** Tres reintentos por tres reintentos son nueve peticiones. `N-51` lo desaconseja de forma explícita.

**La llamada sin *timeout*.** No espera mucho: espera para siempre, y agota los recursos del consumidor por una causa remota.

**Capturar `TimeoutException` esperando los *timeouts* del *pipeline*.** Polly v8 lanza `TimeoutRejectedException`. El bloque no captura nada y el fallo se propaga por un camino que nadie previó.

**El `catch` que se traga la excepción y devuelve una lista vacía.** Es degradación no diseñada: el usuario ve «no hay salas disponibles» cuando lo cierto es «no pudimos consultar». Son dos hechos distintos y el sistema acaba de mentir.

**Como productor, el `500` genérico.** Convierte condiciones de negocio previstas en fallos indeterminados que el consumidor va a reintentar en vano, y es donde más información se filtra.

**El *pipeline* configurado y nunca ejercitado.** Sin una prueba que apague la dependencia, lo que existe es la creencia de tener resiliencia. Es el antipatrón que produce los otros.

---

## Anexo — Checklist de revisión de seguridad de una API

La lista completa está en el anexo de [`TEM-AUTH`](Autenticacion-y-Autorizacion.md), que es la que `ACT-07` firma. Esta la amplía para la robustez, y tiene dos mitades porque hay dos roles: se completa una por cada dependencia consumida y una sola vez para la API producida.

```yaml
# Se completa una vez por cada dependencia externa consumida.
consumo:
  dependencia: ""
  contexto: CTX-2 | CTX-3 | CTX-4
  operaciones_no_idempotentes: []
  clave_de_idempotencia:
    soportada_por_el_proveedor: si | no | desconocido
    derivada_de_la_operacion_no_del_intento: si | no
  reintentos:
    metodos_excluidos: []                  # POST y PATCH salvo prueba en contrario
    codigos_que_reintentan: []             # típicamente 408, 429, >=500
    backoff: exponencial | fijo | ninguno
    jitter_activo: si | no
    retry_after_respetado: si | no
  timeouts:
    por_intento: ""
    total: ""
    derivados_de_la_espera_tolerable_del_usuario: si | no
  circuit_breaker:
    presente: si | no
    granularidad: por-dependencia | global
  degradacion:
    comportamiento_al_agotar_reintentos: fallar | degradar | diferir
    decidido_por_producto: si | no         # no puede quedar implícito en un catch
    estado_ambiguo_registrado_para_reconciliacion: si | no
  implementacion_net:
    un_solo_resilience_handler: si | no     # N-51
    paquete: "Microsoft.Extensions.Http.Resilience" | "Microsoft.Extensions.Http.Polly (deprecado)"
    cliente_no_capturado_en_singleton: si | no
  verificacion:
    probado_con_la_dependencia_apagada: si | no
    probado_con_la_dependencia_lenta: si | no
    probado_contra_respuestas_429: si | no

# Se completa una sola vez, para la API que se produce.
produccion:
  - cada_camino_de_fallo_tiene_codigo_asignado: si | no
  - condiciones_de_negocio_conocidas_no_devuelven_500: si | no
  - rechazo_por_limite_devuelve_429: si | no          # el default del framework es 503
  - retry_after_emitido_en_429_y_en_503_planificado: si | no
  - clave_de_idempotencia_aceptada_donde_corresponde: si | no
  - reutilizacion_de_clave_con_peticion_distinta_devuelve_409: si | no
  - codigos_de_fallo_declarados_en_openapi: si | no
  - garantias_de_disponibilidad_documentadas: si | no  # obligatorio en CTX-1
```

Las tres líneas de `verificacion` son las que separan una política de resiliencia de la creencia de tenerla. Ninguna de las anteriores se sostiene sin ellas, porque el camino que ejercitan es el único que en operación normal no se recorre nunca.
