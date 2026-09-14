---
doc_id: TEM-AUTH
doc_type: tema
title: Autenticación y autorización
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-SEG, TEM-PROT, TEM-RESIL, TEM-STATUS, TEM-ERR, TEM-OPENAPI, TEM-URI, MARCO-ACTORES, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Autenticación y autorización — `TEM-AUTH`

## Resumen ejecutivo

Dos preguntas distintas se resuelven con mecanismos distintos y se confunden constantemente porque llegan juntas en la misma cabecera. **Quién sos** lo responde la autenticación; **qué podés hacer** lo responde la autorización. Un token válido prueba lo primero y no dice casi nada sobre lo segundo: que el sistema sepa con certeza que la petición viene de Marina no determina si Marina puede cancelar la reserva `r-8842`.

Este documento recorre los mecanismos de autenticación con criterios de elección por contexto —`CTX-1` y `CTX-2` piden cosas opuestas—, desarrolla JWT con el detalle que su ubicuidad justifica, y dedica su parte central a lo que menos cubierto está: la autorización a nivel de instancia. Roles y políticas resuelven «¿puede este usuario cancelar reservas?»; ningún framework resuelve solo «¿es esta reserva suya?», porque la respuesta está en el dominio y no en el token.

El destinatario es `ACT-07`, que decide, y `ACT-01`, que tiene que diseñar la superficie sabiendo qué va a pedir `ACT-07`. La matriz de [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md) asigna esta decisión a `ACT-07` con **D** y a `ACT-01` con **C**; leerla al revés produce el fallo que ese documento describe como el más caro, la intervención tardía.

---

## Definición

### Qué es cada cosa

**Autenticación** es el proceso por el cual una petición queda asociada a una identidad verificada. Su salida es un sujeto —un usuario, un servicio, una aplicación— y un conjunto de afirmaciones sobre él. En ASP.NET Core esa salida se materializa en un `ClaimsPrincipal` disponible en `HttpContext.User`.

**Autorización** es la decisión de permitir o denegar una operación concreta sobre un recurso concreto, dada una identidad ya establecida. Su entrada incluye la identidad, pero también el recurso, la operación y con frecuencia el estado del dominio.

La distinción tiene una consecuencia protocolar directa, que [`TEM-STATUS`](../30-Semantica-HTTP/Codigos-de-Estado.md) desarrolla: `401 Unauthorized` corresponde a un fallo de autenticación —no sé quién sos, o la credencial que trajiste no sirve— y `403 Forbidden` a un fallo de autorización —sé quién sos y no podés—. El nombre del primero es históricamente desafortunado y explica buena parte de la confusión.

### Qué problema resuelven

El de la confianza distribuida. HTTP es un protocolo sin estado, cada petición llega sola y el servidor no puede asumir nada sobre quién la envió. Todo mecanismo de autenticación resuelve la misma pregunta: cómo transportar una prueba de identidad en una petición individual, de forma que el servidor pueda verificarla sin haber visto las anteriores.

### Qué no son

**No son cifrado del transporte.** TLS protege la petición en tránsito y no dice nada sobre quién la envió. Un token Bearer viaja en claro dentro del canal cifrado: sin TLS es una credencial regalada. `N-16` §2.1 lo trata como precondición y no como opción.

**No son gestión de identidades.** Registrar usuarios, recuperar contraseñas y administrar el ciclo de vida de una cuenta son problemas de un sistema de identidad, no de la API que consume sus tokens. Confundirlos lleva a APIs de recursos que exponen endpoints de gestión de credenciales sin haberlo decidido.

**La autorización no es validación de negocio.** Que una reserva no pueda cancelarse a menos de veinticuatro horas del inicio es una regla de dominio y su incumplimiento produce `409` o `422`, no `403`. La frontera es real y se cruza seguido en ambas direcciones: implementar reglas de negocio como políticas de autorización oscurece el dominio, y tratar un fallo de permisos como error de validación filtra información.

**Un JWT no es una sesión.** Es la confusión más costosa del tema y merece sección propia más abajo.

---

## Los mecanismos y cuándo corresponde cada uno

### API key

Una cadena opaca, emitida por el productor, que el cliente presenta en cada petición. Identifica a la **aplicación** llamante, no al usuario final, y esa es a la vez su virtud y su límite.

Su fuerza es la simplicidad: no requiere infraestructura de identidad y un integrador puede empezar a llamar en minutos, lo que en `CTX-1` es un argumento real de adopción. Sus debilidades son estructurales. No expira por sí sola, no acota permisos salvo que el productor construya ese modelo por su cuenta, y una vez filtrada sirve indefinidamente hasta que alguien la rote. Su transporte correcto es una cabecera propia; ponerla en la query string la deposita en cada log de cada intermediario del camino.

Esta guía recomienda API key solo para acceso de servicio a servicio con permisos gruesos, con rotación prevista desde el diseño, y nunca como sustituto de identidad de usuario.

### Autenticación Basic

Usuario y contraseña codificados en base64 en la cabecera `Authorization`. La codificación no es cifrado: la credencial viaja recuperable, en cada petición, y por lo tanto se expone en cada punto donde esa petición se registre o se inspeccione.

Su único uso defendible hoy es el de un entorno controlado con una credencial de servicio dedicada. Para identidad de usuario final en cualquier contexto, esta guía la desaconseja.

### Bearer token

`N-16` (RFC 6750) define el esquema: `Authorization: Bearer <token>`. Su propiedad definitoria está en el nombre —*bearer*, portador—: **quien lo tiene, puede usarlo**. El servidor no verifica que quien presenta el token sea aquel a quien se le emitió; verifica que el token sea válido. De ahí que la vida corta y el canal cifrado sean parte del mecanismo y no precauciones opcionales.

`N-16` §3 define además los parámetros del *challenge* y el mapeo de errores, que conviene respetar porque los clientes bien construidos lo leen:

| `error` | Código HTTP | Situación |
|---|---|---|
| `invalid_request` | `400` | La petición está mal formada |
| `invalid_token` | `401` | El token expiró, es inválido o su firma no verifica |
| `insufficient_scope` | `403` | El token es válido pero no alcanza para la operación |

Un detalle de `N-16` que se ignora casi siempre: si la petición **no trae credenciales**, el servidor *SHOULD NOT* incluir código de error en el *challenge*. Distinguir «no mandaste nada» de «lo que mandaste está mal» es información gratuita para quien sondea.

### OAuth 2.0

`N-15` (RFC 6749) no es un mecanismo de autenticación sino un **marco de delegación de autorización**. Resuelve un problema específico: permitir que una aplicación acceda a recursos en nombre de un usuario sin que ese usuario le entregue su contraseña. El token Bearer de `N-16` es el vehículo habitual del resultado.

`N-15` está vigente y actualizado por RFC 8252, RFC 8996 y RFC 9700. Define cuatro *grants*, de los cuales dos siguen siendo recomendables:

```mermaid
sequenceDiagram
    participant U as Usuario (dueño del recurso)
    participant C as Cliente (app de reservas)
    participant AS as Servidor de autorización
    participant API as API de salas

    U->>C: quiere reservar
    C->>U: redirección al servidor de autorización
    U->>AS: se autentica y consiente
    AS->>C: código de autorización
    C->>AS: código + client_id + code_verifier
    AS->>C: access token (+ refresh token)
    C->>API: GET /v1/reservas · Authorization: Bearer …
    API->>C: 200 OK
```

**Authorization Code** es el flujo del diagrama y el que corresponde cuando hay un usuario presente que debe autenticarse ante un tercero. **Client Credentials** elimina al usuario del cuadro: la aplicación se autentica a sí misma con sus propias credenciales, y es el flujo de `CTX-2`, servicio contra servicio.

Los otros dos están en retirada. **Implicit** y **Resource Owner Password Credentials** siguen figurando en el texto de `N-15` porque un RFC no se reescribe, pero `F-03` —el borrador de OAuth 2.1, `draft-ietf-oauth-v2-1-15`, del 2 de marzo de 2026— los **elimina**, y hace **PKCE obligatorio**: *«Clients MUST use `code_challenge` and `code_verifier` and authorization servers MUST enforce their use»*. Ese documento consolida `N-15`, `N-16` y las mejores prácticas posteriores, y agrega la coincidencia exacta de *redirect URI* salvo en *loopback*.

La advertencia de nivel de autoridad es necesaria: **OAuth 2.1 no es un RFC**. `F-03` es un Internet-Draft activo, sin *area director* responsable ni *shepherd* asignados, con envío al IESG previsto para diciembre de 2026. Diseñar siguiéndolo es prudente y esta guía lo recomienda; citarlo como norma vigente es incorrecto.

### mTLS

TLS mutuo: además de que el cliente verifique el certificado del servidor, el servidor verifica el del cliente. La identidad queda establecida en el nivel del transporte, antes de que exista una petición HTTP.

Su fuerza es que la credencial no viaja en la aplicación y no puede filtrarse por un log ni por un error verboso. Su costo es operativo y no es menor: emisión, distribución, rotación y revocación de certificados para cada cliente. Encaja en `CTX-2` con un número acotado de servicios y en `CTX-4` cuando la contraparte lo exige; es impracticable en `CTX-1`, donde los consumidores son desconocidos y numerosos.

### Criterio de elección por contexto

| | `CTX-1` Pública | `CTX-2` Interna | `CTX-3` App propia | `CTX-4` Integración |
|---|---|---|---|---|
| **Mecanismo típico** | OAuth 2.0 con Authorization Code; API key para acceso de servicio | Client Credentials o mTLS | OAuth 2.0 con Authorization Code y PKCE | Lo que imponga el proveedor |
| **Identidad de** | Usuario final y aplicación | Servicio | Usuario final | Ambos lados, según el contrato ajeno |
| **Vida del token** | Corta, con renovación | Corta, renovación automática del cliente | Corta; el móvil complica la renovación | No se decide de este lado |
| **Dónde vive la credencial** | En el cliente del integrador | En configuración del servicio | Almacenamiento seguro del dispositivo o del navegador | En la configuración propia |
| **Riesgo dominante** | Credencial filtrada que nadie rota | Confianza implícita en la red | Token en un cliente que no controlamos | Rotación impuesta sin aviso |

Dos casos merecen precisión adicional. En `CTX-3` con Blazor en render *interactive server*, el componente **se ejecuta en el servidor** y la llamada a la API es servidor contra servidor: la credencial nunca baja al navegador y el caso se comporta como `CTX-2`, según establece [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md). La misma aplicación en render WebAssembly vuelve a ser un cliente desplegado en un dispositivo ajeno, y todo lo que guarde es inspeccionable por su usuario.

Y en `CTX-2` conviene resistir la tentación de la red confiable. «Está detrás del firewall» es una afirmación sobre la topología de hoy, no sobre la del año que viene, y ninguna API sobrevive intacta a la primera vez que alguien expone un servicio interno «temporalmente».

---

## JWT

### Qué es

`N-17` (RFC 7519) define un formato compacto y firmado para transportar afirmaciones —*claims*— entre partes. Tres segmentos separados por puntos y codificados en base64url: cabecera, cuerpo y firma. Los dos primeros son **legibles por cualquiera**; lo que la firma garantiza es integridad y origen, no confidencialidad. Poner en un JWT algo que no debería ver el portador es un error frecuente y difícil de revertir una vez que hay clientes emitidos.

Los *claims* registrados por `N-17` §4.1 son siete: `iss` (emisor), `sub` (sujeto), `aud` (audiencia), `exp` (expiración), `nbf` (no antes de), `iat` (emitido en) y `jti` (identificador del token). El RFC base los declara **todos opcionales**; las aplicaciones y los perfiles son los que los vuelven obligatorios.

### El perfil de `N-18`

`N-18` (RFC 9068) es el perfil de JWT para *access tokens* de OAuth 2.0, y resuelve un problema práctico: `N-15` nunca definió el formato del token, de modo que cada servidor de autorización eligió el suyo. El perfil fija tres cosas que conviene conocer:

- El `typ` de la cabecera es **`at+jwt`**, con media type `application/at+jwt`. Sirve para que un token de acceso no pueda confundirse con un token de identidad, una confusión que ha causado problemas reales.
- Los *claims* requeridos son `iss`, `exp`, `aud`, `sub`, `client_id`, `iat` y `jti`. La diferencia con `N-17` es que acá no son opcionales.
- El token **MUST** ir firmado, el algoritmo `none` está prohibido y el soporte de RS256 es obligatorio.

### Validación

Un JWT recibido no es un JWT confiable. La validación mínima comprende cuatro comprobaciones, y omitir cualquiera de ellas deja la puerta abierta:

1. **Firma.** Con la clave del emisor, obtenida de su documento de metadatos y no incrustada en el código.
2. **Emisor** (`iss`). Que el token venga de quien esperamos.
3. **Audiencia** (`aud`). Que el token haya sido emitido **para esta API**. Sin esta comprobación, un token válido para otro servicio del mismo emisor abre este.
4. **Expiración** (`exp`, y `nbf` si está presente).

`N-44` establece la posición de Microsoft sobre cómo configurarlas, y es contraintuitiva: *«Explicitly defining the parameters is not required… You should use the default values if possible.»* La documentación **desaconseja** reescribir a mano lo que ya viene por defecto, porque cada parámetro que se fija explícitamente es un parámetro que puede fijarse mal.

### Por qué un JWT no se revoca

Un JWT firmado es autocontenido: la API lo valida verificando la firma y los *claims*, sin consultar a nadie. Esa es exactamente la propiedad que lo hace escalable y la que hace que **no exista forma de invalidarlo antes de su `exp`** dentro del mecanismo. Un usuario dado de baja, un permiso revocado o un token robado siguen funcionando hasta que el reloj los alcance.

Las salidas conocidas trasladan el costo a otro lado. Una **vida corta** —minutos, no horas— acota la ventana, con el precio de renovaciones frecuentes; es la respuesta estándar y la que esta guía recomienda como punto de partida. Una **lista de revocación** consultada en cada petición devuelve el estado al sistema y con él la dependencia que el JWT venía a evitar; es aceptable si se limita a los casos que lo justifican, y `jti` existe precisamente para eso. **Introspección** del token contra el servidor de autorización en cada petición produce un token de referencia con sintaxis de JWT, lo cual es una decisión legítima siempre que se tome sabiendo lo que se está pagando.

Lo que no es una salida es suponer que borrar el token del cliente lo invalida. El cliente que colabora ya no lo usa; el que no colabora, sí.

### Renovación

El *refresh token* es una credencial de vida larga que solo sirve para obtener nuevos *access tokens*, y por eso su tratamiento tiene que ser más estricto que el del token que renueva. En `CTX-3` con clientes móviles el problema se agrava: una aplicación instalada que estuvo semanas sin abrirse necesita renovar antes de la primera llamada útil, y esa renovación puede fallar por razones que el usuario no entiende.

El diseño del lado de la API es sencillo de enunciar: responder `401` con un *challenge* claro cuando el token expiró, para que el cliente sepa que debe renovar y reintentar en lugar de mostrarle un error al usuario. La política de reintento correspondiente la trata [`TEM-RESIL`](Resiliencia-y-Reintentos.md).

---

## Autorización

### Por roles

El modelo más antiguo y el más fácil de implementar: el token trae un *claim* de rol y el endpoint exige uno determinado. Funciona mientras los roles sean pocos y estables.

Su límite aparece rápido. Los roles tienden a multiplicarse hasta que nadie sabe qué implica cada uno, y las excepciones —«los administradores de sede pueden cancelar cualquier reserva, salvo las de las salas de dirección»— no tienen dónde expresarse salvo creando un rol más.

### Por políticas

Una política nombra un requisito y separa la decisión del endpoint. `N-46` documenta el modelo de ASP.NET Core, y su semántica de combinación es la parte que más se malinterpreta: **varios *requirements* dentro de una política se combinan con AND; varios *handlers* para un mismo *requirement* se combinan con OR**. Un *handler* que tiene éxito basta para satisfacer su *requirement*, aunque otro haya fallado.

Dos detalles de `N-46` con consecuencias prácticas: `InvokeHandlersAfterFailure` vale `true` por defecto, de modo que los *handlers* se ejecutan aunque uno ya haya fallado, y *«handlers can execute in any order»* — no hay que escribir *handlers* que dependan del orden.

La ventaja de diseño es de mantenimiento: la política `PuedeCancelarReservas` se define una vez y el día que su criterio cambia se cambia en un lugar. La ventaja de contrato es que el nombre de la política es documentación.

### Por recurso, y el problema de la instancia

Acá está el hueco que ningún framework llena. Una política responde preguntas sobre el sujeto: qué rol tiene, qué *scope* trae, qué edad declara. La pregunta «¿esta reserva es tuya?» **no se puede responder mirando el token**, porque la respuesta depende de un dato que está en la base de datos: quién es el titular de `r-8842`.

El sistema de reserva de salas lo muestra con nitidez. `GET /v1/reservas/r-8842` puede estar perfectamente autenticado, el usuario puede tener el rol correcto y el *scope* correcto, y aun así no tener nada que ver con esa reserva. La verificación necesariamente ocurre **después** de cargar el recurso, dentro del código de la operación, y por lo tanto es responsabilidad de `ACT-02` en cada endpoint. Es exactamente el tipo de responsabilidad distribuida que se olvida en algún lugar.

Tres consecuencias de diseño se siguen de esto:

**Hay que buscarla explícitamente al revisar.** Un endpoint que carga por identificador y devuelve sin comprobar titularidad pasa cualquier prueba del camino feliz. Solo aparece si `ACT-04` prueba con el identificador de otro usuario, y eso solo ocurre si alguien lo pidió.

**El modelo de recursos puede reducir la superficie.** Exponer `/v1/mis-reservas` en lugar de `/v1/reservas/{id}` traslada la restricción a la consulta: la colección se filtra por el sujeto del token y no hay identificador ajeno que pedir. No elimina el problema —el detalle sigue necesitando la comprobación— pero elimina la clase entera de fallos en el listado. La decisión pertenece a [`TEM-JERARQ`](../20-Diseno-de-Recursos/Jerarquias-y-Relaciones.md) y tiene este costado de seguridad.

**El código de respuesta ante el fallo es una decisión, no un dato.** Y es la tensión con `ACT-03` que desarrolla [`TEM-PROT`](Proteccion-de-la-Superficie.md): responder `403` es informativo y confirma que `r-8842` existe; responder `404` no confirma nada y deja a un consumidor legítimo sin saber si se equivocó de identificador o de cuenta.

---

## Aplicación por escenario

### `ESC-1` — API nueva

El mecanismo de autenticación es una de las decisiones que [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) marca como caras de revertir, y con razón: cambiarlo después obliga a todos los consumidores a modificar su cliente al mismo tiempo, que es la definición práctica de un cambio rompiente.

Lo que se decide ahora: el mecanismo, la forma del token, la granularidad de los *scopes* y —la que más se posterga— si el modelo de autorización va a necesitar decisiones por instancia. Si la respuesta es sí, y en un sistema de reservas lo es, conviene fijar desde el primer endpoint el patrón con el que se comprueba, para que sea uniforme y revisable.

La trampa es el sobrediseño: montar un servidor de autorización propio, cinco *scopes* por recurso y autorización basada en atributos para una API con un consumidor conocido resuelve problemas que todavía no existen. Lo que **no** se puede postergar es exigir autenticación desde el principio; una API que nació abierta y se cierra después rompe a todos sus consumidores el mismo día.

### `ESC-2` — Exposición o migración

El sistema previo ya tiene un modelo de permisos, y casi nunca es el que conviene exponer. Se repite acá la tensión general del escenario: el modelo interno empuja hacia una API que lo refleja. Una tabla `USR_PERM` con veintitrés banderas booleanas heredadas de 2011 no debe convertirse en veintitrés *scopes*.

El trabajo consiste en definir el modelo de autorización del contrato público desde las operaciones que la API ofrece, y traducir. El costo de traducción es real y conviene declararlo, porque desde afuera parece trabajo redundante.

Un riesgo específico y frecuente: los sistemas heredados que resuelven la autorización **en la interfaz de usuario** —el botón no se dibuja si no corresponde— y por lo tanto tienen la lógica en la capa equivocada. Al exponer una API esa protección desaparece por completo, porque no hay botón.

### `ESC-3` — Evolución en producción

Cambiar el mecanismo de autenticación de una API con consumidores es de las operaciones más caras que existen, porque no admite despliegue parcial del lado del consumidor: o el cliente manda la credencial nueva o no entra.

Lo que sí se puede evolucionar con cuidado es el modelo de autorización, y ahí hay una asimetría que conviene tener presente: **afinar permisos rompe**. Agregar un *scope* requerido a un endpoint existente rompe a todo consumidor cuyo token no lo tenga, aunque desde el lado del productor se perciba como «corregir un permiso que faltaba». La corrección es correcta y el cambio es rompiente; ambas cosas a la vez.

Corregir un fallo de autorización a nivel de instancia descubierto en producción es el caso donde `ACT-07` y `ACT-06` chocan de frente: el veto de seguridad no admite ventana de deprecación, y el consumidor que dependía del fallo —a veces sin saberlo— se rompe sin aviso. Es una de las pocas situaciones donde esta guía recomienda romper sin período de gracia.

### `ESC-4` — Evaluación de una API ajena

En `ESC-4a`, con acceso al código o a la especificación, el trabajo es sistemático: recorrer cada operación y verificar que declare su requisito de seguridad, y contrastar contra el código, porque la divergencia entre lo declarado y lo implementado es el hallazgo más frecuente del escenario. Un `securityScheme` declarado globalmente en OpenAPI no prueba que todos los endpoints lo apliquen.

En `ESC-4b`, solo desde afuera, se observa qué mecanismo pide la API, qué devuelve ante una petición sin credenciales —`401` con `WWW-Authenticate` bien formado, o `403`, o un `200` con cuerpo vacío—, si el token es un JWT legible y qué *claims* trae. Ese último punto no requiere ninguna técnica especial: un JWT es base64url y su cuerpo se lee. Lo que se obtiene es una hipótesis del modelo de seguridad, que debe registrarse como tal.

El límite que enuncia [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) aplica con particular fuerza acá: probar el control de acceso de una API ajena sin autorización explícita no es evaluación.

### Qué cambia por contexto

En **`CTX-1`** el mecanismo es parte del producto: hay que documentarlo, ofrecer autogestión de credenciales y una política de rotación publicada. Los errores de autenticación son la primera experiencia de un integrador nuevo y su claridad determina cuánto soporte se consume.

En **`CTX-2`** el flujo natural es Client Credentials o mTLS, y el riesgo dominante no es el mecanismo sino su ausencia: la confianza implícita en la red interna. Conviene además propagar la identidad del usuario original a través de la cadena de servicios, porque de lo contrario la autorización por instancia se pierde en el primer salto.

En **`CTX-3`** la pregunta que gobierna todo es dónde vive la credencial. Blazor en *interactive server* la mantiene en el servidor; WebAssembly y MAUI la depositan en un dispositivo ajeno, y ahí ninguna credencial de larga vida es segura. `N-49` documenta un cambio de .NET 10 muy pertinente: las peticiones no autenticadas a *endpoints* de API protegidos por cookies devuelven `401` y `403` **en lugar de redirigir** a una página de login, lo que resuelve el viejo problema de las APIs que respondían `302` a un cliente que esperaba JSON.

En **`CTX-4`** no se elige: se implementa lo que el proveedor soporte y se aísla. Esa capa de aislamiento es la que permite reemplazar al proveedor sin que su modelo de identidad circule por todo el sistema.

---

## Ejemplos concretos

Los ejemplos son **sintéticos** y corresponden al dominio de reserva de salas.

### Petición autenticada y sus tres fallos

```http
GET /v1/reservas/r-8842 HTTP/1.1
Host: api.salas.ejemplo.com
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9…
Accept: application/json
```

Sin credencial, o con una que no valida, corresponde `401` con el *challenge* de `N-16`:

```http
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer realm="api.salas", error="invalid_token", error_description="The access token expired"
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/token-invalido",
  "title": "Credencial inválida o expirada",
  "status": 401
}
```

Con token válido pero *scope* insuficiente, `403` y el error `insufficient_scope`:

```http
HTTP/1.1 403 Forbidden
WWW-Authenticate: Bearer realm="api.salas", error="insufficient_scope", scope="reservas:escribir"
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/permiso-insuficiente",
  "title": "El token no habilita esta operación",
  "status": 403
}
```

Con token válido, *scope* correcto y una reserva que pertenece a otro usuario, la respuesta es una **decisión de diseño** y no un dato del protocolo. `404` es la opción que no confirma la existencia del recurso:

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://api.salas.ejemplo.com/problemas/reserva-no-encontrada",
  "title": "Reserva no encontrada",
  "status": 404
}
```

El criterio para elegir entre `403` y `404` lo desarrolla [`TEM-PROT`](Proteccion-de-la-Superficie.md); el formato del cuerpo, [`TEM-ERR`](../40-Contratos-y-Representaciones/Manejo-de-Errores.md).

### Configuración de JWT bearer en ASP.NET Core

Paquete `Microsoft.AspNetCore.Authentication.JwtBearer`; `AddJwtBearer` vive en `Microsoft.Extensions.DependencyInjection.JwtBearerExtensions`. Verificado sobre `N-44` con moniker `aspnetcore-10.0`.

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        jwtOptions.Authority = "https://identidad.ejemplo.com";
        jwtOptions.Audience  = "https://api.salas.ejemplo.com";
    });
```

Dos líneas de configuración bastan, y esa brevedad es deliberada. `N-44` señala que las validaciones obligatorias —firma, emisor, audiencia y expiración— quedan activas por defecto, y recomienda **no** redefinir `TokenValidationParameters` salvo que haya una razón concreta. Los parámetros existen (`ValidateIssuer`, `ValidateAudience`, `ValidateIssuerSigningKey`, `ValidAudiences`, `ValidIssuers`, más `MetadataAddress` y `MapInboundClaims`) y están documentados; el punto es que fijarlos a mano introduce la posibilidad de fijarlos mal.

Para desarrollo local, `dotnet user-jwts` (`N-45`) emite tokens firmados sin necesidad de un servidor de identidad. Sus subcomandos son `create`, `list`, `print`, `key`, `clear` y `remove`; `create` acepta `--scope`, `--role`, `--claim nombre=valor` y `--valid-for`, con expiración por defecto de seis meses. Escribe `Authentication:Schemes:Bearer:ValidAudiences` y `:ValidIssuer` en `appsettings.Development.json`, de modo que `AddJwtBearer()` sin argumentos toma esa configuración.

### Políticas y su aplicación

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PuedeGestionarReservas", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "reservas:escribir"));

    options.AddPolicy("AdministraSede", policy =>
        policy.Requirements.Add(new AdministraSedeRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, AdministraSedeHandler>();
```

Los *helpers* documentados en `N-46` son `RequireClaim`, `RequireRole`, `RequireAuthenticatedUser` y `RequireAssertion`. `IAuthorizationRequirement` es *«a marker interface with no methods»*: el *requirement* transporta datos y el `AuthorizationHandler<TRequirement>` decide, señalizando con `context.Succeed(requirement)` o `context.Fail()`.

Aplicado sobre un grupo de rutas:

```csharp
var reservas = app.MapGroup("/v1/reservas")
                  .RequireAuthorization("PuedeGestionarReservas");

reservas.MapGet("/{id}", ObtenerReserva);
reservas.MapDelete("/{id}", CancelarReserva);
```

También admite política declarada en línea —`.RequireAuthorization(p => p.RequireClaim("scope", "reservas:leer"))`— y, para exigir autenticación en toda la aplicación, `AddAuthorizationBuilder()` con `SetFallbackPolicy`. Esta guía recomienda esa última forma: **exigir por defecto y eximir explícitamente** convierte el olvido en un endpoint inaccesible, que es un fallo ruidoso, en lugar de un endpoint abierto, que es un fallo silencioso.

> El uso de `RequireAuthorization` sobre un `MapGroup` no está respaldado por un ejemplo completo en la documentación consultada: `N-25` menciona `RequireAuthorization` como método aplicable a grupos y los *convention builders* componen, pero no se obtuvo un fragmento oficial de esa forma exacta. Se marca por la convención de trazabilidad de [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md).

El orden del *middleware* no es negociable. `N-48` lo fija: *«`UseCors`, `UseAuthentication`, and `UseAuthorization` must appear in the order shown.»*

```csharp
app.UseRouting();
app.UseRateLimiter();
app.UseCors("IntegradoresConocidos");
app.UseAuthentication();
app.UseAuthorization();
```

### Autorización por instancia

Ninguna política resuelve esto, y por eso el código es explícito:

```csharp
static async Task<Results<Ok<ReservaDto>, NotFound>> ObtenerReserva(
    string id,
    ClaimsPrincipal usuario,
    IReservasRepository repositorio,
    CancellationToken ct)
{
    var reserva = await repositorio.ObtenerAsync(id, ct);

    // La comprobación de titularidad ocurre DESPUÉS de cargar el recurso:
    // depende del estado del dominio, no de los claims del token.
    if (reserva is null || reserva.TitularId != usuario.FindFirstValue(ClaimTypes.NameIdentifier))
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(ReservaDto.Desde(reserva));
}
```

La condición unifica deliberadamente «no existe» y «no es tuya» en una sola rama, de modo que la respuesta sea indistinguible. Escribirlas separadas es igualmente válido siempre que ambas devuelvan lo mismo; unificarlas evita que una futura modificación las separe por descuido.

### Declaración de la seguridad en OpenAPI

El contrato de seguridad forma parte del contrato de la API, y una operación cuya autenticación no está declarada es una operación mal especificada. `N-19` (OAS 3.2.0) define los `securitySchemes` dentro de `components` y su aplicación mediante `security`, globalmente o por operación.

```yaml
components:
  securitySchemes:
    oauth2Salas:
      type: oauth2
      flows:
        authorizationCode:
          authorizationUrl: https://identidad.ejemplo.com/authorize
          tokenUrl: https://identidad.ejemplo.com/token
          scopes:
            reservas:leer: Consultar reservas propias
            reservas:escribir: Crear y cancelar reservas
    bearerJwt:
      type: http
      scheme: bearer
      bearerFormat: JWT

security:
  - oauth2Salas: [reservas:leer]

paths:
  /v1/reservas/{id}:
    delete:
      summary: Cancela una reserva
      security:
        - oauth2Salas: [reservas:escribir]
      responses:
        "204": { description: Reserva cancelada }
        "401": { description: Credencial ausente, inválida o expirada }
        "403": { description: El token no habilita la operación }
        "404": { description: La reserva no existe o no pertenece al solicitante }
```

Tres decisiones del ejemplo merecen ser leídas con atención. El bloque `security` global fija el requisito por defecto y cada operación lo estrecha cuando necesita más; una operación con `security: []` lo elimina, y esa es la única forma correcta de declarar un endpoint público. Los `401`, `403` y `404` están **declarados como respuestas**, porque `ACT-03` necesita saber que existen para construir su cliente. Y la descripción del `404` es deliberadamente ambigua entre las dos causas: la especificación tampoco debe deshacer lo que el diseño de la respuesta protege.

La generación de este documento desde ASP.NET Core con `AddOpenApi` y los *transformers* la trata [`TEM-OPENAPI`](../60-Especificacion-y-Documentacion/Especificacion-OpenAPI.md); .NET 10 emite OpenAPI 3.1 por defecto, según `N-32`.

---

## Preguntas guía

- ¿Qué puede hacer un token robado de esta API, y por cuánto tiempo? Es la primera pregunta de `ACT-07` según [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md), y su respuesta se mide en minutos o en meses.
- ¿Validamos la audiencia (`aud`), o aceptamos cualquier token bien firmado por nuestro emisor?
- ¿Qué pasa hoy si damos de baja a un usuario? ¿Cuánto tiempo sigue funcionando su token?
- Para cada endpoint que recibe un identificador de recurso, ¿dónde está la comprobación de que ese recurso le corresponde al solicitante? ¿Puedo señalar la línea?
- ¿Un endpoint nuevo queda protegido por omisión o desprotegido por omisión?
- ¿La especificación OpenAPI declara el requisito de seguridad de cada operación, y coincide con lo que el código exige?
- Si mañana hay que cambiar de proveedor de identidad, ¿cuánto código propio se toca?
- ¿Los mensajes de error distinguen «no existe» de «no es tuyo»? ¿Fue una decisión o quedó así?

---

## Criterios de calidad

Una aplicación buena se reconoce en que la autenticación es uniforme —un mecanismo, no cuatro conviviendo por sedimentación—, en que el estado por defecto de un endpoint nuevo es protegido, en que la autorización por instancia se comprueba de forma consistente y verificable, y en que la especificación declara lo que el código hace.

Una aplicación pobre suele tener autenticación impecable y autorización artesanal: el proveedor de identidad está bien integrado, los tokens se validan como corresponde, y hay tres endpoints donde nadie comprobó de quién es la reserva.

### Antipatrones

**Autenticar y no autorizar.** Verificar el token y asumir que quien está autenticado puede hacer lo que pide. Es el fallo más común y el más silencioso, porque toda prueba del camino feliz lo atraviesa sin ruido.

**El JWT tratado como sesión revocable.** Emitir tokens de vida larga suponiendo que dar de baja al usuario los invalida. No los invalida; la única variable que controla la ventana de exposición es `exp`.

**Guardar en el token lo que no debe verse.** El cuerpo de un JWT es legible por cualquiera que lo tenga. Datos personales, identificadores internos o detalles de configuración depositados ahí quedan expuestos al portador y a todo lo que registre esa cabecera.

**Omitir la validación de audiencia.** Aceptar cualquier token firmado por el emisor convierte a todas las APIs que comparten emisor en una sola superficie: un token emitido para el servicio de facturación abre el de reservas.

**Autorizar en el cliente.** Ocultar el botón de cancelar y no comprobar el permiso en el servidor. En una aplicación web era una protección débil; frente a una API es ninguna, porque no hay botón que ocultar.

**Roles que crecen sin gobierno.** Cuando aparece `AdminSedeSalvoDireccion`, el modelo de roles ya se rompió y lo que hacía falta era una política.

**La credencial en la query string.** `GET /v1/reservas?api_key=…` deposita la credencial en los logs del servidor, del *proxy*, del balanceador y en el `Referer` de cualquier navegación posterior. Las credenciales van en cabeceras.

**Reinventar el token.** Diseñar un formato propio con un cifrado propio. `N-17` y `N-18` existen, están implementados en todas las plataformas y fueron revisados por gente cuyo trabajo es encontrarles fallos.

**Autenticación diferida a `ESC-3`.** Publicar sin autenticación con la intención de agregarla después. Agregarla después rompe a todos los consumidores el mismo día y sin ventana posible, porque una ventana de deprecación con la API abierta es una API abierta durante la ventana.

---

## Anexo — Checklist de revisión de seguridad de una API

Se recorre antes de publicar y se vuelve a recorrer ante cualquier cambio en la superficie. Las tres primeras secciones corresponden a este documento; las restantes remiten a [`TEM-PROT`](Proteccion-de-la-Superficie.md) y [`TEM-RESIL`](Resiliencia-y-Reintentos.md), y se reproducen acá para que la lista sea utilizable de una sola pasada. `ACT-07` es quien la firma.

```yaml
autenticacion:
  - mecanismo_declarado: ""              # OAuth 2.0 AuthCode | ClientCredentials | mTLS | API key
  - mismo_mecanismo_en_toda_la_superficie: si | no
  - tls_obligatorio_sin_excepciones: si | no
  - credenciales_solo_en_cabeceras: si | no      # nunca en query string
  - vida_del_access_token: ""            # en minutos
  - renovacion_definida_y_documentada: si | no
  - endpoints_publicos_declarados: []    # lista explícita; vacío es una respuesta válida

validacion_del_token:
  - firma_verificada: si | no
  - emisor_validado: si | no
  - audiencia_validada: si | no          # el que más se omite
  - expiracion_validada: si | no
  - algoritmo_none_rechazado: si | no
  - claves_obtenidas_de_metadatos: si | no       # no incrustadas en el código
  - defaults_del_framework_conservados: si | no  # N-44 lo recomienda

autorizacion:
  - modelo: roles | politicas | mixto
  - estado_por_defecto_de_un_endpoint_nuevo: protegido | abierto   # debe ser "protegido"
  - endpoints_que_reciben_id_de_recurso: []
  - todos_comprueban_titularidad: si | no
  - respuesta_ante_recurso_ajeno: "403" | "404"  # decisión declarada, no accidental
  - permisos_comprobados_del_lado_servidor: si | no

filtracion_de_informacion:
  - errores_sin_traza_de_excepcion: si | no
  - errores_sin_nombres_de_tablas_ni_de_tipos_internos: si | no
  - errores_sin_versiones_de_framework_ni_de_bibliotecas: si | no
  - respuestas_indistinguibles_entre_no_existe_y_no_es_tuyo: si | no
  - identificadores_expuestos: secuenciales | opacos

limites_y_superficie:
  - rate_limiting_activo: si | no
  - codigo_de_rechazo: "429" | "503"     # el default del framework es 503; debe corregirse
  - retry_after_emitido: si | no         # no es automático
  - limites_documentados_para_el_consumidor: si | no
  - cors_con_origenes_explicitos: si | no        # nunca comodín con credenciales
  - validacion_de_entrada_antes_del_dominio: si | no

especificacion:
  - securitySchemes_declarados_en_openapi: si | no
  - cada_operacion_declara_su_requisito: si | no
  - 401_403_404_declarados_como_respuestas: si | no
  - especificacion_contrastada_contra_el_codigo: si | no

firma:
  actor: ACT-07
  fecha: ""
  hallazgos_abiertos: []
  vetos_ejercidos: []
```

El campo `endpoints_que_reciben_id_de_recurso` es el que más trabajo ahorra y el que más se omite. Enumerarlos convierte la autorización por instancia —que de otro modo es una preocupación difusa que cada desarrollador recuerda o no— en una lista finita que se puede recorrer.
