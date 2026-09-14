---
doc_id: DOC-API
doc_type: tema
title: API Specification
status: vigente
origin: ia-assisted
confidence: alta
owner: ACT-04 Desarrollador
last_review: 2026-07-18
audience: [humano, agente]
traces: [FAM-DIS, DOC-HLD, DOC-LLD, DOC-DATOS, DOC-INTEGRACION, DOC-DEVGUIDE, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES]
---

# API Specification — `DOC-API`

## 1. Resumen ejecutivo

La API Specification es el contrato que un sistema ofrece a otros programas: qué operaciones expone, con qué formas de entrada y salida, con qué errores, con qué garantías y bajo qué reglas de autenticación. Se escribe en un formato que una máquina puede validar —OpenAPI 3.1 para HTTP, ficheros `.proto` para gRPC, el esquema de tipos para GraphQL— porque su función es permitir que alguien construya un cliente correcto sin conversaciones.

Es el documento con mayor densidad de uso diario en `CTX-2`: se consulta varias veces por día, alimenta generadores de cliente, sostiene pruebas de contrato y aparece en la revisión de cada cambio que toca la superficie pública. Esa frecuencia impone una exigencia que otros documentos no tienen: una especificación desincronizada del servicio es peor que ninguna, porque los clientes construidos contra ella fallan en producción y su autor culpa al servidor.

---

## 2. Definición

### Qué es

Una descripción formal y completa de la superficie de interacción entre programas. Completa significa que cubre lo que un cliente necesita para funcionar en todos los casos, no solo en el feliz: cada respuesta posible con su código y su forma, cada error con su significado y su acción recomendada, cada restricción de tamaño y de frecuencia, la política de reintentos, el esquema de autenticación y su vencimiento, y las garantías que el servicio ofrece —idempotencia, consistencia, orden— que ninguna firma de método expresa.

Formal significa que existe como artefacto legible por máquina, versionado junto al código, del cual se generan clientes y contra el cual se validan las peticiones y las respuestas en las pruebas.

### Qué problema resuelve

El acoplamiento por conversación. Sin contrato explícito, cada integrador descubre el comportamiento probando, y lo que descubre incluye accidentes de la implementación actual que después se rompen: que el campo llega siempre en cierto orden, que el error de validación devuelve texto plano, que la lista nunca supera cien elementos. Todo eso se vuelve dependencia de hecho, y un cambio inocente rompe a un consumidor que nadie sabía que existía.

Resuelve también el problema inverso, que es el que limita la velocidad del equipo: sin contrato publicado, ningún cambio es seguro, y la respuesta racional del equipo es no cambiar nada. Una especificación con política de compatibilidad explícita libera al equipo a modificar lo que declaró modificable.

### Qué NO es

**No es la Integration Guide.** La distinción se malentiende con frecuencia y produce documentos que sirven a la mitad. La API Specification es de referencia: describe cada operación de forma exhaustiva y se consulta puntualmente, como un diccionario. La [Integration Guide](Integration-Guide.md) es narrativa: explica cómo se resuelve un caso completo de punta a punta, en qué orden se llaman las operaciones, cómo se obtienen credenciales, qué hacer ante un error transitorio, cómo se prueba antes de producción. Un integrador necesita las dos, y por razones distintas: la guía para arrancar, la especificación para no volver a preguntar.

**No es el Developer Guide.** El [Developer Guide](../60-Desarrollo/Developer-Guide.md) sirve a quien desarrolla *dentro* del sistema: cómo se levanta el entorno, qué convenciones se aplican, cómo se corre la batería de pruebas. La API Specification sirve a quien lo consume *desde afuera*. Cuando ambos se mezclan, el integrador externo lee instrucciones de cadena de compilación que no le sirven y no encuentra el catálogo de errores que buscaba.

**No es documentación generada sin curar.** La especificación que un generador produce por reflexión sobre los controladores describe formas y códigos, y omite todo lo semántico: qué significa el `409`, si la operación es idempotente, cuánto vale un token, qué pasa si el mismo cuerpo se envía dos veces. Ese material generado es el esqueleto correcto; sin anotación deliberada, es un catálogo de firmas.

**No es el LLD.** El [LLD](LLD.md) describe interfaces internas, modificables por el equipo cuando le convenga. La API Specification compromete compatibilidad hacia consumidores que no controla.

---

## 3. Aplicación por escenario

| Escenario | Naturaleza | Momento de escritura | Riesgo característico |
|-----------|-----------|---------------------|----------------------|
| `ESC-1` Desarrollo nuevo | Prescriptiva; puede ser diseño previo | Antes o junto con la implementación | Diseñar la API como espejo de las tablas |
| `ESC-2` Migración | Contrato de paridad entre origen y destino | Relevada del origen antes de diseñar el destino | Replicar en el destino las rarezas del origen |
| `ESC-3` Evaluación con código | Reconstruida desde controladores y rutas | Al inicio, porque delimita la superficie | Documentar lo que el código expone sin decir qué se usa |
| `ESC-4` Evaluación externa | Parcial, solo si el producto la publica | Si existe portal de desarrolladores | Inferir endpoints observando la aplicación web |

En `ESC-1` la API es el compromiso más caro de revertir de toda la familia, porque en cuanto hay un consumidor deja de ser propiedad del equipo. La decisión más consecuente es el **nivel de abstracción**: una API que refleja las tablas —`GET /reservas`, `GET /reserva_asistentes`— traslada al cliente la responsabilidad de reconstruir las operaciones del negocio y ata el contrato al esquema, con lo cual toda evolución de la base se vuelve un cambio incompatible. Una API que expone operaciones del dominio —`POST /reservas`, `POST /reservas/{id}/cancelacion`— deja libre el esquema y describe algo que el consumidor entiende.

En `ESC-2` la especificación del origen es el criterio de paridad más objetivo disponible, y conviene levantarla antes de tocar el destino, incluyendo el comportamiento no documentado que los clientes ya dependen: el campo obsoleto que un cliente sigue leyendo, el orden implícito de una lista, el código de error que alguien detecta por su texto. Lo que se decida no replicar se registra explícitamente, con su plan de comunicación a los consumidores afectados.

En `ESC-3` la reconstrucción parte de las rutas registradas y los tipos de retorno, pero el hallazgo relevante no es qué expone el sistema sino **qué se usa realmente**. Los registros del servidor o la puerta de enlace contestan esa pregunta en una tarde, y suele revelar que un tercio de la superficie no recibe tráfico desde hace dos años. Esa información vale más que la especificación completa, porque define qué se puede retirar.

En `ESC-4` lo legítimo es la especificación que el proveedor publica. Observar las peticiones que la aplicación web del producto hace a su propio backend produce información real, pero se trata de una interfaz interna sin compromiso de estabilidad, y documentarla como contrato induce a construir sobre algo que puede cambiar sin aviso. Se registra como observación fechada, con versión del producto, y con la advertencia explícita.

### Variación por contexto

En **`CTX-1`** la API se consume, no se define, y lo que el equipo de interfaz necesita documentar es distinto: qué latencia esperar para dimensionar los estados de carga, qué errores son recuperables por reintento y cuáles exigen intervención del usuario, qué campos pueden faltar. Un catálogo de errores traducido a mensajes de interfaz es un artefacto propio de este contexto.

En **`CTX-2`** la especificación es el documento central, y el criterio de suficiencia es el enunciado en los contextos del marco: un desarrollador externo debe poder integrarse leyendo solo la documentación, sin preguntar. Toda pregunta que un integrador hace por otro canal es un defecto de la especificación, y llevar registro de esas preguntas es la mejor fuente de mejora que existe.

En **`CTX-3`** aparece una decisión que los otros contextos no enfrentan: qué operaciones tienen endpoint y cuáles se invocan directamente. En Blazor Server el componente puede llamar a un servicio del lado servidor sin pasar por HTTP, y exponer además un endpoint duplica superficie y trabajo. El criterio razonable es que una operación tiene endpoint cuando existe un consumidor fuera del proceso —la aplicación MAUI, una integración, un proceso batch— y no cuando podría llegar a haberlo. Esa política se declara una vez, en el [HLD](../30-Arquitectura/HLD.md), y evita la discusión repetida en cada revisión.

---

## 4. Formato

### 4.1 OpenAPI 3.1 para HTTP

OpenAPI Specification 3.1 es el formato de referencia de esta guía para APIs HTTP. Su versión 3.1 alineó el modelo de esquemas con JSON Schema, lo que permite reutilizar los mismos esquemas para validar peticiones, generar clientes y validar documentos en otros contextos, y eliminó las divergencias que obligaban a mantener definiciones duplicadas.

La semántica de los métodos y de los códigos de estado no la define OpenAPI sino RFC 9110, que fija qué significa cada método, cuáles son seguros e idempotentes, y qué expresa cada clase de código de estado. Una API que devuelve `200` con un cuerpo de error, o que usa `POST` para una consulta sin efecto, contradice esa semántica aunque su documento OpenAPI sea válido: la herramienta valida forma, no sentido.

### 4.2 `POST /reservas` — fragmento de especificación

Fragmento real y autocontenido, ilustrativo del sistema de reservas. Muestra la operación con su clave de idempotencia y sus cuatro respuestas relevantes.

```yaml
openapi: 3.1.0
info:
  title: API de Reservas de Salas
  version: 1.4.0
  description: |
    Operaciones sobre reservas de salas. La versión mayor del contrato viaja en
    la ruta (`/v1`); esta versión de `info` identifica la publicación concreta
    de la especificación y sigue Semantic Versioning 2.0.0.
servers:
  - url: https://api.reservas.example.com/v1
    description: Producción
  - url: https://sandbox.api.reservas.example.com/v1
    description: Sandbox; datos sintéticos, se reinicia cada domingo

paths:
  /reservas:
    post:
      operationId: crearReserva
      summary: Crea una reserva para una sala en un intervalo dado
      description: |
        Operación **idempotente respecto de `Idempotency-Key`**. Un reintento con la
        misma clave y el mismo cuerpo devuelve `200` con la reserva ya creada, sin
        crear una segunda. La clave se conserva 24 horas; pasado ese plazo la misma
        clave se trata como nueva. Un reintento con la misma clave y distinto cuerpo
        devuelve `422`.
      tags: [Reservas]
      security:
        - oauth2: [reservas:escribir]
      parameters:
        - name: Idempotency-Key
          in: header
          required: true
          description: Identificador único de intento generado por el cliente.
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/SolicitudDeReserva'
            examples:
              reunionSemanal:
                summary: Reserva de una hora con dos asistentes
                value:
                  salaId: "8f14e45f-ceea-467a-9f0b-2c1a3d5e7b90"
                  inicio: "2026-08-03T10:00:00-03:00"
                  fin: "2026-08-03T11:00:00-03:00"
                  motivo: "Revisión de sprint"
                  asistentes:
                    - "b1c2d3e4-5f60-4a7b-8c9d-0e1f2a3b4c5d"
                    - "c2d3e4f5-6071-4b8c-9d0e-1f2a3b4c5d6e"
      responses:
        '201':
          description: Reserva creada.
          headers:
            Location:
              description: URI de la reserva creada.
              schema: { type: string, format: uri }
            ETag:
              description: Versión del recurso; usar en `If-Match` para modificarlo.
              schema: { type: string }
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Reserva'
        '200':
          description: |
            Reintento detectado por `Idempotency-Key`. Devuelve la reserva creada en
            el intento original. No es un error: es la respuesta correcta al reintento.
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Reserva'
        '409':
          description: |
            La sala está ocupada en todo o parte del intervalo pedido. El cuerpo
            incluye las reservas en conflicto y hasta tres intervalos alternativos
            cercanos. Reintentar sin cambiar el intervalo devolverá el mismo error.
          content:
            application/problem+json:
              schema:
                $ref: '#/components/schemas/ProblemaConflicto'
              example:
                type: "https://api.reservas.example.com/problemas/sala-ocupada"
                title: "La sala no está disponible en el intervalo solicitado"
                status: 409
                detail: "La sala Roble tiene una reserva confirmada de 10:30 a 11:30."
                instance: "/reservas"
                salaId: "8f14e45f-ceea-467a-9f0b-2c1a3d5e7b90"
                conflictos:
                  - reservaId: "3a7b9c1d-2e4f-4061-8a2b-5c7d9e0f1a2b"
                    inicio: "2026-08-03T10:30:00-03:00"
                    fin: "2026-08-03T11:30:00-03:00"
                alternativas:
                  - inicio: "2026-08-03T09:00:00-03:00"
                    fin: "2026-08-03T10:00:00-03:00"
                  - inicio: "2026-08-03T11:30:00-03:00"
                    fin: "2026-08-03T12:30:00-03:00"
        '422':
          description: |
            El cuerpo es sintácticamente válido pero viola una regla de negocio:
            intervalo invertido, fuera del horario de la sala, con más asistentes
            que la capacidad, o clave de idempotencia reutilizada con otro cuerpo.
          content:
            application/problem+json:
              schema:
                $ref: '#/components/schemas/ProblemaValidacion'
              example:
                type: "https://api.reservas.example.com/problemas/validacion"
                title: "La solicitud no cumple las reglas de reserva"
                status: 422
                detail: "Se detectaron 2 errores."
                instance: "/reservas"
                errores:
                  - campo: "fin"
                    regla: "RN-002"
                    mensaje: "El fin debe ser posterior al inicio."
                  - campo: "asistentes"
                    regla: "RN-011"
                    mensaje: "La sala Roble admite 6 asistentes; se enviaron 9."

components:
  securitySchemes:
    oauth2:
      type: oauth2
      flows:
        clientCredentials:
          tokenUrl: https://auth.example.com/oauth2/token
          scopes:
            reservas:leer: Consultar reservas y disponibilidad
            reservas:escribir: Crear, reprogramar y cancelar reservas
  schemas:
    SolicitudDeReserva:
      type: object
      required: [salaId, inicio, fin]
      additionalProperties: false
      properties:
        salaId: { type: string, format: uuid }
        inicio:
          type: string
          format: date-time
          description: Instante con desplazamiento explícito; se rechaza sin zona.
        fin: { type: string, format: date-time }
        motivo: { type: string, maxLength: 500 }
        asistentes:
          type: array
          maxItems: 50
          items: { type: string, format: uuid }
    Reserva:
      type: object
      required: [id, salaId, solicitanteId, inicio, fin, estado]
      properties:
        id: { type: string, format: uuid }
        salaId: { type: string, format: uuid }
        solicitanteId: { type: string, format: uuid }
        inicio: { type: string, format: date-time }
        fin: { type: string, format: date-time }
        estado:
          type: string
          enum: [borrador, pendienteAprobacion, confirmada, enCurso, finalizada, cancelada]
        motivo: { type: string }
    Problema:
      description: Problem Details conforme a RFC 9457.
      type: object
      required: [type, title, status]
      properties:
        type: { type: string, format: uri }
        title: { type: string }
        status: { type: integer }
        detail: { type: string }
        instance: { type: string }
    ProblemaConflicto:
      allOf:
        - $ref: '#/components/schemas/Problema'
        - type: object
          properties:
            salaId: { type: string, format: uuid }
            conflictos:
              type: array
              items:
                type: object
                properties:
                  reservaId: { type: string, format: uuid }
                  inicio: { type: string, format: date-time }
                  fin: { type: string, format: date-time }
            alternativas:
              type: array
              maxItems: 3
              items:
                type: object
                properties:
                  inicio: { type: string, format: date-time }
                  fin: { type: string, format: date-time }
    ProblemaValidacion:
      allOf:
        - $ref: '#/components/schemas/Problema'
        - type: object
          properties:
            errores:
              type: array
              items:
                type: object
                properties:
                  campo: { type: string }
                  regla: { type: string, description: "ID de regla de negocio (RN-)" }
                  mensaje: { type: string }
```

Lo que el fragmento demuestra, más allá de la sintaxis, son cuatro decisiones que un contrato completo debe tomar y que los contratos incompletos omiten. Que `201` y `200` significan cosas distintas en la misma operación, y que el `200` no es un error sino la respuesta correcta a un reintento. Que `409` y `422` no son intercambiables: el `409` indica un conflicto con el estado actual del recurso, que puede resolverse cambiando el intervalo o esperando; el `422` indica que la solicitud viola una regla y reintentarla idéntica nunca funcionará. Que el cuerpo de error incluye información accionable —las alternativas— en lugar de solo un mensaje. Y que cada error de validación referencia la regla de negocio por identificador, lo que cierra la traza vertical de `CTX-3` desde el mensaje que el usuario ve hasta el enunciado del SRS.

Las tres respuestas que el fragmento distingue solo se entienden en secuencia, porque dependen de un estado que la especificación obliga a mantener y que el esquema no muestra: el registro de claves de idempotencia con su ventana de 24 horas.

```mermaid
sequenceDiagram
    autonumber
    participant C as Cliente
    participant A as POST /reservas
    participant K as Registro de claves<br/>(24 h)
    participant R as Reservas

    C->>A: Idempotency-Key 9f3a… · sala Roble 10:00–11:00
    A->>K: ¿clave registrada?
    K-->>A: no
    A->>R: alta con verificación de solapamiento
    R-->>A: creada
    A->>K: registra clave, huella del cuerpo e id de reserva
    A-->>C: 201 · Location · ETag · Reserva

    Note over C,A: la respuesta se pierde en la red; el cliente reintenta

    C->>A: misma clave · mismo cuerpo
    A->>K: ¿clave registrada?
    K-->>A: sí, huella coincidente
    A-->>C: 200 · la reserva del intento original

    C->>A: clave nueva · sala Roble 10:00–11:00
    A->>K: ¿clave registrada?
    K-->>A: no
    A->>R: alta con verificación de solapamiento
    R-->>A: conflicto con reserva de 10:30 a 11:30
    A-->>C: 409 · problem+json · conflictos[] y alternativas[]
```

El orden importa en un solo punto y es el que se implementa mal: la clave se registra después de que el alta prospera y en la misma transacción. Registrarla antes deja al cliente con una clave quemada y sin reserva, y ese estado no tiene respuesta correcta en el contrato.

### 4.3 Problem Details — RFC 9457

Los cuerpos de error usan `application/problem+json` conforme a RFC 9457, que sucede a RFC 7807. El campo `type` es un URI que identifica el tipo de problema y debe resolver a documentación legible; es el campo que el cliente usa para discriminar programáticamente, no el `title`, que es texto para humanos y puede cambiar de redacción. `status` repite el código HTTP para que el problema sobreviva a intermediarios que lo transporten. Los miembros de extensión —`conflictos`, `alternativas`, `errores`— son la parte que hace útil al estándar: permiten información específica del dominio dentro de una envoltura común.

El error de diseño frecuente es tratar el catálogo de valores de `type` como implementación en lugar de contrato. Un cliente que ramifica según `type` depende de esos URIs tanto como de los nombres de campo, y cambiarlos rompe integraciones. Van versionados con el resto del contrato.

### 4.4 gRPC y GraphQL

REST con OpenAPI no es la única opción, y elegir por costumbre desperdicia oportunidades.

**gRPC**, con contratos en ficheros `.proto`, aventaja a REST cuando la comunicación es entre servicios propios, de alta frecuencia y baja latencia, o cuando hace falta transmisión bidireccional. El contrato es más estricto —el generador produce tipos en ambos extremos y el compilador detecta la incompatibilidad— y el formato binario reduce carga útil y costo de serialización. Se paga en accesibilidad: probar desde un navegador o desde una terminal es incómodo, atravesar intermediarios HTTP antiguos puede fallar, y los consumidores externos poco sofisticados no lo agradecen. En el sistema de reservas, la comunicación entre el servicio de reservas y el de disponibilidad —interna, frecuente, con carga útil pequeña— es candidata razonable; el endpoint público que consume la aplicación MAUI, no.

**GraphQL** conviene cuando muchos clientes distintos necesitan proyecciones distintas de un mismo grafo de datos y la alternativa es multiplicar endpoints específicos o transferir de más. Traslada al cliente la decisión de qué campos pedir, y traslada al servidor problemas que REST no tiene: consultas anidadas costosas que hay que acotar, caché mucho más difícil, análisis de costo de consulta como requisito y no como refinamiento. Para una API de reservas con dos clientes conocidos y operaciones que son comandos más que consultas, agrega complejidad sin resolver un problema que exista.

La elección no es excluyente y conviene documentarla como tal: REST hacia afuera, gRPC entre servicios internos, y ningún GraphQL mientras el número de proyecciones distintas no lo justifique. Lo que sí es excluyente es la disciplina: cualquiera de los tres exige contrato explícito, versionado y validado contra el servicio.

---

## 5. Contract-first o code-first

La pregunta operativa es cuál de los dos artefactos —la especificación o el código— es la fuente de verdad, y cuál se deriva del otro. Mantener ambos a mano no es una tercera opción: es la garantía de divergencia.

En **contract-first** se escribe primero el documento OpenAPI, se revisa con los consumidores, y del contrato se generan los contratos del servidor y los clientes. Conviene cuando hay consumidores externos que necesitan empezar antes de que el servicio exista, cuando varios equipos deben acordar antes de implementar, cuando la API es el producto, o cuando la especificación necesita aprobación formal. Su costo es fricción: cada cambio pasa por el documento, y los equipos con poca disciplina terminan modificando el código y actualizando el contrato después, que es exactamente lo que el enfoque pretendía impedir.

En **code-first** la especificación se genera desde el código anotado. Conviene cuando el equipo controla servicio y clientes, cuando la API evoluciona rápido en fase temprana, o cuando el consumidor es la propia interfaz del producto. Su ventaja decisiva es que la especificación no puede desincronizarse: si se genera en cada compilación, describe lo que el servicio hace. Su debilidad es que lo generado captura formas y códigos, y omite la semántica —idempotencia, garantías, significado de los errores— salvo que alguien la anote deliberadamente. Ese trabajo de anotación es la parte que los equipos omiten y la que hace útil al documento.

| Criterio | Contract-first | Code-first |
|----------|---------------|-----------|
| Consumidores externos que arrancan antes | Fuerte | Débil |
| Acuerdo entre equipos previo a implementar | Fuerte | Débil |
| Riesgo de desincronización | Alto si falta disciplina | Bajo si se genera en la compilación |
| Velocidad en fase temprana | Baja | Alta |
| Calidad semántica por defecto | Alta: se escribe pensando | Baja: hay que anotar a conciencia |

Sea cual sea el enfoque, la práctica que resuelve el problema de fondo es la misma: **validar la especificación contra el servicio en la integración continua**. Con contract-first, se verifica que las respuestas reales satisfagan los esquemas declarados; con code-first, se compara la especificación generada contra la publicada y se falla la compilación ante diferencias no declaradas. Sin esa verificación automatizada, los dos enfoques convergen al mismo destino en un año.

---

## 6. Versionado, compatibilidad, paginación y autenticación

### Versionado y compatibilidad

Los contratos siguen Semantic Versioning 2.0.0, con la salvedad de que solo la versión mayor viaja en la ruta —`/v1/reservas`— y las menores y de parche se publican sin ruptura. Poner la versión en la ruta no es la única opción; hacerlo por cabecera o por negociación de contenido es defendible y a veces más elegante. Lo que no es defendible es no decidirlo y descubrir en el segundo año que hay tres mecanismos conviviendo.

La distinción práctica que hay que dejar por escrito es qué cambio es compatible. Son compatibles hacia atrás: agregar un endpoint, agregar un campo opcional a una petición, agregar un campo a una respuesta, agregar un valor a un enumerado **de salida** siempre que se haya advertido que puede crecer, y relajar una validación. No lo son: quitar o renombrar un campo, volver obligatorio un campo opcional, endurecer una validación, cambiar un tipo, cambiar el código de estado de un caso existente, y agregar un valor a un enumerado que el cliente usa para ramificar de forma exhaustiva. Este último es el que más sorprende: si el contrato no advirtió que el enumerado puede crecer, agregar `enCurso` a los estados de reserva rompe a todo cliente que hiciera una discriminación cerrada.

La deprecación necesita procedimiento, no anuncio. Marcar la operación con `deprecated: true`, publicar la fecha de retiro y la alternativa, notificar a los consumidores identificados, medir el tráfico residual, y retirar solo cuando ese tráfico llegue a cero o cuando venza el plazo comprometido, lo que ocurra después. Retirar una versión sin haber medido el tráfico es cómo se descubre que un proceso batch de otra área dependía de ella.

### Paginación

Toda colección que pueda crecer sin techo se pagina desde el primer día, incluso cuando hoy tenga doce elementos. Convertir una respuesta no paginada en paginada es un cambio incompatible, y hacerlo cuando ya hay clientes cuesta una versión mayor.

La paginación por desplazamiento —`?pagina=3&tamanio=50`— es simple y permite saltar a una página arbitraria, pero degrada en conjuntos grandes y produce resultados inconsistentes cuando los datos cambian entre páginas: un elemento insertado desplaza a los demás y el cliente ve un registro dos veces o ninguna. La paginación por cursor —`?despuesDe=<opaco>&limite=50`— es estable ante inserciones y eficiente a cualquier profundidad, a cambio de no permitir saltos arbitrarios. Para el listado de reservas, que se ordena por fecha y se recorre de forma secuencial, el cursor es la elección correcta. El cursor se documenta como **opaco**: el cliente no debe interpretarlo ni construirlo, y decirlo explícitamente evita que alguien lo decodifique y quede atado a su formato interno.

Se documentan además el tamaño por defecto, el máximo, el comportamiento ante un tamaño mayor que el máximo —acotar en silencio o rechazar; ambas son válidas, la ambigüedad no— y si se devuelve el total, que en conjuntos grandes puede costar más que la propia página.

### Autenticación y autorización

El esquema se declara en `securitySchemes` y se aplica por operación. Lo que la declaración formal no dice y el documento debe agregar: cómo se obtienen las credenciales, cuánto vive el token, si existe refresco y cómo se rota una credencial comprometida; qué alcance protege cada operación y qué se devuelve cuando falta —`403` cuando el sujeto está autenticado pero no autorizado, `401` cuando el token falta o venció, distinción que los clientes usan para decidir si reintentan tras refrescar o si abortan—; y los límites de frecuencia con sus cabeceras y su código, que el cliente necesita para respetarlos en lugar de descubrirlos rebotando.

---

## 7. Preguntas guía

- ¿Un desarrollador externo podría integrarse leyendo solo esto, sin preguntar? ¿Qué preguntó el último que lo intentó?
- ¿Qué operaciones son idempotentes, respecto de qué clave, y por cuánto tiempo se recuerda esa clave?
- ¿Cada código de error dice qué debe hacer el cliente, o solo qué salió mal?
- ¿Qué cambios se declaran compatibles y cuáles obligan a versión mayor? ¿Está escrito o es criterio del que revisa?
- ¿La especificación se valida contra el servicio de forma automatizada? ¿Qué falla si divergen?
- ¿Esta colección puede crecer? Entonces, ¿por qué no está paginada?
- ¿Los valores de `type` de los problemas están versionados como parte del contrato?
- ¿Qué endpoints declarados no reciben tráfico desde hace un año?

---

## 8. Criterios de calidad y antipatrones

### Criterios

Una especificación de calidad es **completa en los errores**: cada operación enumera todas las respuestas que puede producir, con su significado y la acción esperada del cliente. Es **explícita en las garantías**: idempotencia, consistencia, orden y entrega están escritas y no supuestas. Es **verificable**: existe un mecanismo automatizado que la contrasta con el servicio. Es **estable en lo que prometió**: su política de compatibilidad está publicada y se respeta. Y es **ejecutable**: incluye ejemplos que funcionan copiados tal cual contra el sandbox, que es la prueba más honesta de que alguien la probó.

El indicador más útil para medir su calidad no está en el documento sino afuera: la cantidad de preguntas que los integradores hacen por otros canales. Una API con buena especificación genera preguntas sobre el negocio; una con mala especificación genera preguntas sobre el formato de un campo.

### Antipatrones

**La especificación generada sin curar.** Formas y códigos correctos, cero semántica. El lector sabe que existe `POST /reservas` y que devuelve `409`, y no sabe qué significa ni qué hacer al respecto.

**El error genérico.** Todo falla con `400` y un cuerpo `{ "error": "Ha ocurrido un error" }`. El cliente no puede distinguir un dato inválido de una sala ocupada, y su único comportamiento posible es mostrar un mensaje inútil.

**La API espejo del esquema.** Un endpoint por tabla, con los nombres de columna como campos. Ata el contrato a la base, obliga al cliente a orquestar operaciones del negocio, y convierte toda migración de esquema en un cambio incompatible.

**El versionado por costumbre.** `/v2` publicada por cada cambio, incluidos los compatibles, con tres versiones vivas que nadie retira. Multiplica el costo de mantenimiento y confunde al integrador, que no sabe cuál usar.

**La respuesta sin techo.** `GET /reservas` devuelve todas. Funciona el primer año y colapsa el día que alguien la llama sobre un edificio con cien mil filas.

**El `200` universal.** Éxito y error con el mismo código, distinguidos por un campo del cuerpo. Rompe todo intermediario que decida por código de estado —caché, reintentos, monitoreo— y contradice RFC 9110.

**La documentación en un lugar y el servicio en otro.** Un portal actualizado a mano tres versiones atrás. Los clientes construidos contra él fallan, y su autor tarda un día en descubrir que la culpa era del documento.

---

## 9. Anexo — Plantilla comentada

```markdown
---
doc_id: DOC-API
doc_type: tema
title: API Specification — <nombre de la API>
status: borrador | vigente | obsoleto
origin: human | ia-assisted | ia-generated
confidence: alta | media | baja
owner: ACT-04 <persona>
last_review: AAAA-MM-DD
audience: [humano, agente]
traces: [DOC-HLD, DOC-LLD, DOC-DATOS, DOC-INTEGRACION]
---

# API Specification — <nombre>

## 1. Alcance y audiencia
<!-- ¿Quién consume esta API: clientes propios, equipos internos, terceros?
     ¿Qué queda fuera del contrato y se reserva el equipo modificar sin aviso? -->

## 2. Formato y fuente de verdad
<!-- ¿OpenAPI 3.1, .proto, esquema GraphQL? ¿Dónde vive el archivo?
     ¿Contract-first o code-first? ¿Qué verificación automatizada impide divergir? -->

## 3. Modelo de recursos y operaciones
<!-- ¿Los recursos expresan conceptos del dominio o tablas?
     ¿Qué operación del negocio resuelve cada endpoint? -->

## 4. Convenciones transversales
<!-- Nombres, formatos de fecha, uso de nulos vs. ausencia de campo,
     unidades, codificación de identificadores.
     ¿Están escritas o cada endpoint hace lo suyo? -->

## 5. Autenticación y autorización
<!-- ¿Cómo se obtiene una credencial? ¿Cuánto vive? ¿Cómo se rota si se compromete?
     ¿Qué alcance protege qué operación? ¿Cuándo 401 y cuándo 403? -->

## 6. Errores
<!-- Catálogo de `type` de Problem Details (RFC 9457) con significado y acción esperada.
     ¿Cuál es transitorio y admite reintento? ¿Cuál nunca funcionará si se repite? -->

## 7. Garantías
<!-- Idempotencia: qué operaciones, con qué clave, cuánto se recuerda.
     Consistencia: ¿una lectura posterior a una escritura ve el cambio, siempre?
     Concurrencia: ¿ETag e If-Match? ¿Qué se devuelve ante conflicto de versión? -->

## 8. Paginación, filtrado y ordenamiento
<!-- ¿Desplazamiento o cursor, y por qué? Tamaño por defecto y máximo.
     ¿El cursor es opaco? ¿Qué pasa si el cliente pide más del máximo? -->

## 9. Límites de uso
<!-- Frecuencia, tamaño máximo de cuerpo, cardinalidad máxima de colecciones.
     ¿Qué código y qué cabeceras se devuelven al excederlos? -->

## 10. Versionado y compatibilidad
<!-- ¿Dónde viaja la versión? ¿Qué cambio es compatible y cuál no? Lista explícita.
     Procedimiento de deprecación: aviso, plazo, medición de tráfico, retiro. -->

## 11. Entornos
<!-- Producción y sandbox: URLs, diferencias de comportamiento, datos de prueba,
     credenciales, frecuencia de reinicio. -->

## 12. Especificación
<!-- Referencia al archivo OpenAPI/proto versionado, no una copia pegada.
     Los ejemplos, ¿se ejecutaron contra el sandbox antes de publicarse? -->
```

El apartado 7 es el que separa una especificación que sirve de una que solo describe. Las garantías son lo único del contrato que un generador nunca podrá inferir del código, y son exactamente lo que un integrador necesita saber para escribir un cliente que no duplique reservas.
