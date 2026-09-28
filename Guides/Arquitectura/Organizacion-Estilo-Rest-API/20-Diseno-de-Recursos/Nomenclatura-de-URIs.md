---
doc_id: TEM-URI
doc_type: tema
title: Nomenclatura de URIs
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-REC, TEM-RECURSOS, TEM-JERARQ, TEM-ACCIONES, TEM-CAMPOS, TEM-FILTRO, TEM-PAG, TEM-VERS, TEM-HEADERS, TEM-METODOS, TEM-STATUS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Nomenclatura de URIs — `TEM-URI`

## Resumen ejecutivo

Es el tema sobre el que más se prescribe y menos se especifica. Existe una norma —`N-08`, RFC 3986, Internet Standard STD 66— y esa norma define la sintaxis de una URI sin decir una palabra sobre plurales, guiones, verbos ni idioma. Todo lo que un desarrollador recuerda como «las reglas de las URIs REST» proviene de guías corporativas que se contradicen entre sí de forma frontal, documentada y —esto es lo importante— justificada: cada organización eligió lo que eligió para resolver un problema que tenía.

El caso testigo es el casing. Zalando (`G-05` regla 129) obliga a `kebab-case` en los segmentos de ruta. Google (`G-04` AIP-122) obliga a `camelCase` en los identificadores de colección. Azure (`G-01`) admite los dos. Las tres son documentos vigentes de organizaciones serias, y no hay manera de cumplir las tres a la vez. Quien busque cuál es «la correcta» va a encontrar tres respuestas y ninguna fuente que las dirima, porque esa fuente no existe.

Este documento hace dos cosas. Establece qué es normativo de verdad —que es poco y conviene conocerlo con precisión— y, donde las guías chocan, muestra la contradicción y ofrece un criterio para elegir. Ese criterio, cuando es de esta guía, va marcado como tal.

---

## Definición

### Qué es

La convención que determina cómo se escribe la ruta que identifica a un recurso: qué palabras la componen, en qué número gramatical, con qué separación entre palabras, en qué idioma, con qué profundidad, y qué información va en la ruta y qué en la cadena de consulta.

### Qué problema resuelve

**La predictibilidad.** Una convención consistente permite que quien conoce tres endpoints pueda adivinar el cuarto. Ese es el único beneficio real y es enorme: reduce el tiempo de integración, reduce las consultas al soporte, y hace que la documentación sea confirmación en lugar de descubrimiento. Una API con convención impredecible obliga a leerla entera antes de usar cualquier parte.

**La verificabilidad.** Una convención escrita es la única forma de que `ACT-01` ejerza su autoridad sin revisar cada *pull request*. Un *linter* sobre la especificación OpenAPI puede rechazar `/ObtenerSalas` automáticamente; ningún proceso humano escala a la velocidad a la que se agregan endpoints.

**La estabilidad.** GOV.UK (`G-06`) recomienda explícitamente que los nombres de recursos sean persistentes entre versiones. Una URI es lo único de una API que el consumidor escribe literalmente en su código; cambiarla rompe siempre.

### Qué NO es, y con qué se lo confunde

**No es un problema de REST.** Fielding (`O-01`) no prescribe forma de URI en ningún punto de la disertación. La restricción de interfaz uniforme exige que los recursos estén identificados, no que se llamen de determinada manera. Más aún, en `O-02` sostiene que los servidores deben tener libertad para controlar su propio *namespace* y que el cliente no debería construir URIs sino seguirlas. Bajo esa lectura estricta, la nomenclatura de URIs es un asunto de conveniencia humana, no de arquitectura. La guía la trata igual, y con seriedad, porque en la práctica los clientes sí construyen URIs.

**No es el casing de los campos JSON.** Son dos decisiones separadas y hay al menos una organización mayor que deliberadamente las resuelve distinto: Zalando usa `kebab-case` en paths, `snake_case` en query y `snake_case` en el cuerpo. El casing de campos lo trata [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md).

**No es el versionado.** Dónde vive la versión —path, query, cabecera, media type— es una decisión de evolución con sus propias contradicciones entre guías, y la trata [`TEM-VERS`](../50-Evolucion-y-Versionado/Estrategias-de-Versionado.md). Acá solo se asume que si hay un segmento de versión, es el primero.

**No es una garantía de calidad.** Una API con URIs impecables puede devolver `200 OK` con errores adentro, no paginar y no versionar. La nomenclatura es lo primero que se ve y lo menos determinante de la experiencia de integración.

---

## Lo que sí es normativo

Poco, y conviene conocerlo con exactitud porque es lo único que no admite discusión.

### `N-08` — RFC 3986, sintaxis genérica

**Caracteres.** El RFC distingue dos conjuntos (§2.2 y §2.3):

| Conjunto | Caracteres |
|---|---|
| **unreserved** | `ALPHA` `DIGIT` `-` `.` `_` `~` |
| **reserved — gen-delims** | `:` `/` `?` `#` `[` `]` `@` |
| **reserved — sub-delims** | `!` `$` `&` `'` `(` `)` `*` `+` `,` `;` `=` |

La consecuencia directa es la que gobierna la decisión de casing: **de los separadores de palabra imaginables, solo `-`, `.`, `_` y `~` son *unreserved***. El guión y el guión bajo se pueden usar sin codificar; el espacio, el `+` y la coma no —el `+` y la coma son *sub-delims* y el espacio ni siquiera es un carácter válido de URI—. Un segmento con un espacio se codifica como `%20` y el resultado es ilegible, lo que explica por qué ninguna guía propone separar palabras con espacio aunque nadie lo prohíba explícitamente.

**Sensibilidad a mayúsculas.** Acá hay que ser preciso porque la afirmación que circula es incorrecta. `N-08` establece que el *scheme* (§3.1) es insensible a mayúsculas con forma canónica en minúscula, y que el componente *host* de la autoridad (§3.2.2) es insensible a mayúsculas. **Sobre el path, `N-08` no se pronuncia.** No dice que sea sensible ni que sea insensible; lo trata como dato que se compara tal cual salvo reglas específicas del esquema.

De modo que la regla «usá minúsculas en las rutas» **no es un mandato de RFC 3986**. Es una convención de diseño con una motivación práctica sólida: como el RFC no garantiza insensibilidad, dos servidores pueden tratar `/Salas` y `/salas` de manera distinta, y en la práctica lo hacen —Azure (`G-01`) declara explícitamente que trata los segmentos de path como *case-sensitive*—. Usar un solo casing elimina la ambigüedad. Pero la fuerza del argumento es de prudencia, no de norma, y la diferencia importa cuando alguien pregunta de dónde sale la regla.

Hay una consecuencia práctica que sí conviene fijar como decisión: si los segmentos se tratan como sensibles a mayúsculas, `/salas` y `/Salas` son recursos distintos, y una API que responde a los dos con lo mismo está exponiendo dos URIs para una cosa. Lo mismo vale, con más consecuencias, para los identificadores: un `id` que es un GUID en mayúsculas y otro en minúsculas son claves distintas en la ruta y la misma clave en la base.

### `N-09` — RFC 6570, plantillas de URI

Define la sintaxis para expresar familias de URIs con variables, en cuatro niveles. Lo relevante para la nomenclatura son los operadores de nivel 3: `/` para expansión de segmentos de path, `?` y `&` para parámetros de query, y `;` para *path parameters* —la forma `/salas;sede=norte`, técnicamente válida y prácticamente extinta fuera del ecosistema Java EE—.

Su uso concreto en una API es el de documentar rutas: `/v1/salas/{salaId}/reservas{?desde,hasta}` es una plantilla RFC 6570 y expresa sin ambigüedad qué es variable. OpenAPI usa una sintaxis emparentada pero no idéntica.

### `N-01` — RFC 9110, el límite de longitud

El estándar HTTP define **414 URI Too Long** (§15.5.15), renombrado respecto de RFC 7231 donde era *Request-URI Too Long*. La existencia del código es la única referencia normativa sobre longitud: el RFC no fija un máximo. Los límites reales los imponen servidores, proxies y navegadores, y varían.

---

## Las decisiones, una por una

### Sustantivo, no verbo

Es lo más parecido a un consenso que existe en la materia. La comparativa de guías registra explícitamente el acuerdo: recursos como sustantivos y verbos HTTP con su semántica estándar, lo que corresponde al nivel 2 del modelo de Richardson (`O-03`).

El fundamento no es estético. La acción ya está expresada en el método HTTP, y ponerla también en la ruta produce una redundancia que a veces contradice: `POST /obtenerSalas` declara en el método que modifica y en la ruta que consulta. La consecuencia es que se pierden las tres propiedades que HTTP regala gratis —caché sobre `GET`, idempotencia sobre `PUT` y `DELETE`, y semántica de códigos de estado— porque ninguna intermediaria puede razonar sobre un `POST` a una ruta arbitraria.

| En lugar de | Se escribe | Método |
|---|---|---|
| `POST /crearReserva` | `/reservas` | `POST` |
| `GET /obtenerSala?id=a3f1` | `/salas/a3f1` | `GET` |
| `POST /actualizarSala` | `/salas/a3f1` | `PATCH` |
| `POST /eliminarReserva` | `/reservas/8f21c3` | `DELETE` |
| `GET /listarSalasDeSede?sede=n1` | `/sedes/n1/salas` | `GET` |

Lo que este consenso **no** resuelve es qué hacer con las operaciones que no son ninguno de los cinco casos —cancelar, aprobar, enviar—, que es el problema más frecuente del diseño REST y tiene documento propio: [`TEM-ACCIONES`](Operaciones-No-CRUD.md).

### Plural o singular

**Contradicción C10, débil.** Google (`G-04` AIP-122), Zalando (`G-05` regla 134) y Microsoft Graph (`G-02`) exigen plural para las colecciones. GOV.UK (`G-06`) no prescribe: pide consistencia, singular o plural, y delega la elección en el equipo.

El argumento a favor del plural es la coherencia de lectura: `/salas/a3f1` se lee «el elemento a3f1 de la colección de salas», y esa lectura sostiene la distinción entre colección y elemento sin necesidad de explicarla. El argumento a favor del singular es la uniformidad de segmento: en `/sala/a3f1` el segmento `sala` es el mismo tipo en los dos usos.

En español aparece un problema que las guías en inglés no enfrentan, y conviene tenerlo presente porque es la fuente de las inconsistencias reales. Google contempla la excepción de las palabras cuyo plural coincide con el singular (`info`, `moose`); en español el caso equivalente es más común y menos elegante: `/analisis`, `/crisis`, `/estatus` son idénticos en ambos números, y `/pais` → `/paises` cambia la raíz. Ninguna guía tiene una respuesta para eso.

**Esta guía recomienda** el plural, por la coherencia de lectura y porque es lo que hacen tres de las cuatro guías mayores, con dos precisiones. Los recursos singleton van en singular, aceptando la inconsistencia deliberada con la regla —un `/configuraciones` que siempre devuelve una configuración miente sobre su cardinalidad—. Y cuando el plural español es idéntico al singular o irregular, se acepta y se documenta, en lugar de forzar un anglicismo o inventar una forma.

### Casing de los segmentos

**Contradicción C4, directa.** Es la más citada de este documento.

| Organización | Prescripción | Fuerza | Ejemplo |
|---|---|---|---|
| **Zalando** (`G-05` regla 129) | `kebab-case`, regex `^[a-z][a-z\-0-9]*$` | **MUST** | `/shipment-orders/{shipment-order-id}` |
| **Google** (`G-04` AIP-122) | `camelCase` en identificadores de colección, inicial minúscula, solo ASCII | **MUST** | `/userEvents` |
| **Microsoft Azure** (`G-01`) | `kebab-case` **preferido** o `camelCase` | Admite ambos | `/resource-groups` o `/resourceGroups` |
| **Microsoft Graph** (`G-02`) | No prescribe casing de segmento; sí plural de colecciones | — | `/addresses` |
| **GOV.UK** (`G-06`) | No prescribe; pide consistencia y persistencia | — | — |

Las tres primeras filas son incompatibles entre sí y ninguna es más autorizada que otra. Lo que sí se puede decir es **qué problema resolvía cada una**, que es el criterio que permite elegir:

**Zalando eligió `kebab-case` como parte de un sistema de tres casings por capa.** Sus reglas 118, 129 y 130 asignan deliberadamente `snake_case` a las propiedades JSON, `kebab-case` a los segmentos de path y `snake_case` a los parámetros de query. La separación es intencional: al mirar un identificador se sabe de qué capa es. Es una organización con muchos equipos publicando muchas APIs, y el valor de la regla está en su verificabilidad automática —hay una regex por regla— más que en la elegancia.

**Google eligió `camelCase` porque sus URIs derivan de definiciones protobuf.** AIP-140 fija `lower_snake_case` para los campos en protobuf, y el identificador de colección en la URI mapea desde el nombre del mensaje. El `camelCase` de las URIs es consistente con la generación de código y con el resto del ecosistema de herramientas de Google. Fuera de ese ecosistema, el argumento pierde casi toda su fuerza.

**Azure eligió no elegir**, con la consecuencia previsible: la superficie de Azure tiene ambos estilos y el consumidor no puede predecir cuál.

**Esta guía recomienda** `kebab-case` para los segmentos de ruta, por tres razones que valen fuera del ecosistema de cualquiera de las tres. El guión es un carácter *unreserved* de `N-08`, de modo que nunca requiere codificación. Elimina la interacción con el casing: como todo va en minúscula, la ambigüedad de `/salasDeReunion` frente a `/salasdereunion` frente a `/SalasDeReunion` no llega a plantearse. Y es legible en una URI escrita en minúsculas, que es como aparece en logs, en documentación y en barras de direcciones que a veces normalizan.

La recomendación pierde fuerza —y conviene decirlo— si la organización genera sus APIs desde protobuf o consume tooling de Google, en cuyo caso `camelCase` es la elección coherente. Lo que no es defendible en ningún marco es `snake_case` en el path: no lo prescribe ninguna de las guías mayores para esa capa, y el guión bajo tiene el problema práctico de desaparecer bajo el subrayado de los enlaces en algunos renderizadores.

Una nota sobre la interacción con el idioma, que ninguna guía trata porque todas están en inglés: en español los nombres compuestos son más frecuentes y más largos —`salas-de-reunion` frente a `rooms`—, de modo que la decisión de casing tiene más impacto visible que en inglés.

### Idioma

Ninguna guía de las verificadas se pronuncia. Las cinco están escritas en inglés y asumen inglés sin discutirlo. Es, por lo tanto, una decisión sin ninguna autoridad externa disponible, y el criterio que sigue es **enteramente propio**.

La decisión se toma por contexto y el eje que la gobierna es quién lee esas URIs.

| Contexto | Recomendación de esta guía | Razón |
|---|---|---|
| `CTX-1` pública, integradores internacionales | **Inglés** | El consumidor no habla español y las URIs son parte de la documentación |
| `CTX-1` pública, integradores del mismo país | **Español**, si el dominio es local | Un dominio regulado localmente pierde precisión al traducirse |
| `CTX-2` interna | **El idioma del dominio del equipo** | Coherencia con el código, con el analista y con la conversación cotidiana |
| `CTX-3` app propia | **El del código** | El consumidor es el mismo equipo |
| `CTX-4` integración | No aplica: lo fija el proveedor | — |

Los dos criterios que hacen la diferencia:

**No se traducen los términos del dominio que no tienen traducción exacta.** Si el negocio dice «sede», «legajo», «monotributo» o «prestación», traducirlos introduce una capa de interpretación que después nadie sabe deshacer. Un `/branches` que significa sede, y otro `/offices` que también significa sede en otro endpoint, es peor que `/sedes`.

**No se mezcla.** El peor resultado posible es el intermedio, y es el más frecuente: `/salas/{id}/bookings`, `/usuarios/{id}/permissions`. Cada mezcla obliga al consumidor a recordar cuál de los dos idiomas se usó en cada segmento, que es exactamente lo contrario de la predictibilidad que se buscaba. Si hay que romper la regla para un término sin traducción razonable, se rompe una vez, se documenta, y no se convierte en precedente.

Hay un tercer criterio que suele decidir en la práctica y que conviene enunciar: **el idioma de las URIs y el de los campos JSON deberían coincidir**. Una API con rutas en español y campos en inglés obliga a traducir mentalmente en cada petición. La decisión, por lo tanto, se toma una vez para toda la superficie, junto con [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md), y no endpoint por endpoint.

Los ejemplos de esta guía usan español porque el dominio de reserva de salas es sintético y local, según fija [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md). No es una recomendación implícita.

### Identificadores en la ruta

El segmento de identificador es el único cuyo contenido no elige el diseñador sino que viene de los datos, y por eso tiene reglas propias.

**Formato.** Google (`G-04` AIP-122) es la única guía verificada que lo especifica con precisión, y solo para los identificadores provistos por el usuario: *should* cumplir RFC-1034, regex `^[a-z]([a-z0-9-]{0,61}[a-z0-9])?$`, máximo 63 caracteres, y los caracteres no ASCII *should not* permitirse. Heroku (`G-08`) prescribe UUID en minúsculas con el formato 8-4-4-4-12 salvo muy buena razón en contra. GOV.UK (`G-06`) solo pide consistencia entre recursos similares.

**La decisión que importa no es el formato sino la enumerabilidad.** Un identificador secuencial —`/reservas/1`, `/reservas/2`— le dice al mundo cuántas reservas hay, a qué ritmo crecen, y le permite a cualquiera recorrerlas para descubrir cuáles existen. Que el control de acceso rechace las ajenas no elimina el problema: la diferencia entre `403` y `404` ya confirma la existencia, como advierte `ACT-07` en [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md). En `CTX-1` esto es materia de veto, no de preferencia.

**Esta guía recomienda** identificadores opacos y no secuenciales en la ruta para todo recurso expuesto en `CTX-1`, y en `CTX-2` o `CTX-3` cuando el recurso pertenece a un usuario. Un identificador secuencial es aceptable para catálogos públicos sin dueño —sedes, tipos de sala— donde la enumeración no revela nada.

**Los identificadores compuestos son una señal.** Cuando un recurso se identifica por dos valores —`/reservas/a3f1/2026-08-14`— casi siempre pasa una de dos cosas: o la clave primaria de la tabla se filtró a la URI, o falta un recurso intermedio. La resolución habitual es asignar un identificador propio.

**Los caracteres del identificador se codifican.** Un identificador que contenga `/`, `?`, `#` o cualquier *gen-delim* de `N-08` va percent-encoded, y en ese punto la URI deja de ser legible y empieza a ser frágil. Es un argumento adicional a favor de los identificadores opacos generados por el sistema: no contienen sorpresas.

### Longitud y profundidad

`N-01` define `414 URI Too Long` sin fijar un umbral, y en la práctica los límites los imponen los intermediarios. Ninguna de las guías verificadas prescribe un máximo de longitud ni de profundidad de anidamiento.

Lo que sí se puede decir con fundamento es que la profundidad tiene un costo de diseño independiente de cualquier límite técnico, y ese costo lo trata [`TEM-JERARQ`](Jerarquias-y-Relaciones.md): cada nivel de anidamiento es un compromiso sobre una relación que puede cambiar.

**Esta guía recomienda** dos niveles de colección como norma y tres como excepción justificada, y ninguna URI de más de unos pocos cientos de caracteres en la ruta. El criterio operativo no es el número: es que **la ruta se pueda leer en voz alta sin perder el hilo**. Si hay que releerla para saber de qué recurso se trata, sobra un nivel.

Un caso que produce URIs largas sin anidamiento es el de las listas de identificadores en la ruta. Va en query, y aun ahí tiene límite; cuando la lista es grande, la operación deja de ser un `GET` y pasa a ser otra cosa, lo que trata [`TEM-ACCIONES`](Operaciones-No-CRUD.md).

### Barra final

Ninguna guía verificada se pronuncia, y `N-08` sí tiene algo que decir aunque no lo enuncie como regla de estilo: `/salas` y `/salas/` son **URIs sintácticamente distintas**, porque el path es distinto. Que un servidor las trate igual es una decisión del servidor, no del estándar.

Las tres conductas posibles, con sus consecuencias:

| Conducta | Consecuencia |
|---|---|
| Responder lo mismo a ambas | Dos URIs para un recurso: se duplica la caché y se rompe la comparación de URIs |
| Redirigir con `301` a la forma canónica | Correcto y explícito; cuesta un salto de red por petición mal formada |
| Rechazar la no canónica con `404` | Correcto y hostil; el consumidor no entiende qué pasó |

**Esta guía recomienda** elegir la forma sin barra final como canónica, documentarlo, y redirigir con `301` desde la otra. La razón de preferir la redirección sobre la equivalencia silenciosa es que la equivalencia le enseña al consumidor que las dos formas son intercambiables, y en el momento en que un endpoint nuevo no las trate igual, el error va a ser incomprensible.

En ASP.NET Core el enrutamiento no distingue la barra final por defecto en la mayoría de las configuraciones, de modo que esta decisión requiere configurarla explícitamente si se quiere la conducta canónica.

### Extensiones de archivo

`/salas/a3f1.json` frente a `/salas/a3f1` con `Accept: application/json`.

La extensión en la ruta tiene un atractivo real: se puede pegar en un navegador y funciona, sin herramientas. Y tiene un costo conceptual concreto: **convierte el formato de representación en parte de la identidad del recurso**. `/salas/a3f1.json` y `/salas/a3f1.xml` son dos URIs, y por lo tanto dos recursos según la definición del propio protocolo, cuando en realidad son dos representaciones de uno. La negociación de contenido de `N-01` §12 existe exactamente para eso, con `Accept` en la petición y `Vary` en la respuesta para que las cachés no mezclen.

Ninguna de las guías verificadas prescribe extensiones. **Esta guía recomienda** no usarlas y negociar con `Accept`, con una excepción pragmática: cuando la API sirve descargas destinadas a abrirse en un navegador —un informe en PDF, una exportación en CSV—, la extensión comunica algo al usuario final y el argumento de la identidad pierde peso frente a la usabilidad. La negociación de contenido y el uso de `Vary` los desarrolla [`TEM-HEADERS`](../30-Semantica-HTTP/Cabeceras-y-Negociacion.md).

### Query string o segmento de ruta

La pregunta se puede reducir a una: **¿esto identifica al recurso o lo modifica?**

Lo que identifica va en la ruta. Lo que filtra, ordena, pagina, selecciona campos o ajusta la representación va en la query.

| Va en la ruta | Va en la query |
|---|---|
| `/salas/a3f1` — cuál sala | `?capacidadMinima=10` — cuáles de todas |
| `/sedes/n1/salas` — de qué sede | `?ordenar=nombre` — en qué orden |
| `/reservas/8f21c3/asistentes` — de qué reserva | `?limite=20&cursor=...` — qué porción |
| | `?campos=id,nombre` — con qué detalle |

La prueba práctica que resuelve los casos dudosos: **si se quita el elemento, ¿la URI sigue identificando algo?** Al quitar `?capacidadMinima=10` de `/salas`, queda `/salas`, que es un recurso perfectamente válido: la colección completa. Al quitar `a3f1` de `/salas/a3f1`, queda `/salas`, que también es válido pero es **otro recurso**. Eso indica que `a3f1` era identificatorio.

El caso que rompe la regla y hay que decidir por criterio es el filtro que devuelve siempre un elemento. `/salas?nombre=Belgrano` y `/salas/belgrano` pueden coexistir legítimamente si el nombre es un identificador alternativo estable. Si el nombre puede cambiar, solo la primera forma es correcta: un identificador que cambia no identifica.

Hay una consideración de caché que suele decidir en `CTX-1`: algunas cachés intermedias tratan de forma más conservadora las URIs con query string. No es una razón para meter filtros en la ruta —el costo de esa decisión es mucho mayor—, pero sí explica por qué la paginación por *path* aparece a veces en APIs de alto tráfico.

La sintaxis concreta de esos parámetros —`filter`, `$filter`, `filter[campo]`, y las tres convenciones mutuamente incompatibles que existen— es materia de [`TEM-FILTRO`](../40-Contratos-y-Representaciones/Filtrado-Orden-y-Seleccion.md), y su casing de [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md).

---

## Tabla de contradicciones y criterio de elección

El resumen operativo del documento. Cada fila declara quién prescribe qué y qué recomienda esta guía, con la razón.

| Decisión | Prescripciones en conflicto | Criterio para elegir | Recomendación de esta guía |
|---|---|---|---|
| **Casing de segmento** | Zalando `kebab-case` (MUST) · Google `camelCase` (MUST) · Azure ambos | ¿Se genera desde protobuf o se usa tooling de Google? | `kebab-case`. Carácter *unreserved*, sin interacción con el casing |
| **Plural de colecciones** | Google, Zalando y Graph: plural (MUST) · GOV.UK: consistencia, cualquiera | ¿Hay razón de dominio para el singular? | Plural, salvo singletons |
| **Idioma** | Ninguna guía se pronuncia | ¿Quién lee estas URIs? | Por contexto; nunca mezclado; igual que los campos JSON |
| **Formato de identificador** | Google: RFC-1034 (SHOULD) · Heroku: UUID · GOV.UK: consistencia | ¿El identificador lo provee el usuario o el sistema? | Opaco y no secuencial cuando el recurso tiene dueño |
| **Extensión de archivo** | Ninguna guía se pronuncia | ¿El destinatario es un programa o una persona con un navegador? | Sin extensión; `Accept`. Excepción para descargas |
| **Barra final** | Ninguna guía se pronuncia; `N-08` las distingue | — | Sin barra como canónica, `301` desde la otra |
| **Minúsculas en el path** | Convención universal; **no** es mandato de `N-08` | — | Minúsculas siempre, y saberlo como convención |

La forma de usar esta tabla en una organización real: elegir una fila por decisión, escribirla en el documento de convenciones de `ACT-01`, y hacerla verificable por una herramienta. Una convención que no se puede verificar automáticamente se erosiona endpoint por endpoint, y `ACT-02` es quien la erosiona sin querer y sin que nadie lo note.

---

## Aplicación por escenario

### `ESC-1` — API nueva

El único momento en que estas decisiones son gratis, y por eso el momento en que hay que tomarlas todas juntas y escribirlas. La secuencia recomendada: se elige casing, número, idioma y formato de identificador antes de definir el primer endpoint, se registran en un documento de convenciones con una regla verificable por cada decisión, y se configura el *linter* de OpenAPI en la integración continua en la misma semana.

El error específico de este escenario es tomar las decisiones implícitamente, endpoint por endpoint, con la intención de «normalizar después». No hay después: en el momento en que hay diez endpoints con tres criterios distintos, normalizar es rompiente y la API todavía ni se publicó.

La trampa opuesta también existe: escribir un documento de convenciones de cuarenta páginas que nadie lee. Lo que necesita `ESC-1` son siete decisiones escritas en una página, cada una con su regla automática.

### `ESC-2` — Exposición o migración

El escenario donde la nomenclatura arrastra más peso muerto. El sistema existente tiene nombres, y esos nombres son casi siempre inadecuados para una URI pública: prefijos de tabla, abreviaturas de un sistema anterior, sufijos de normalización, y con frecuencia una mezcla de idiomas acumulada por capas de mantenimiento.

La regla operativa es que **el nombre público se decide desde el vocabulario del negocio, no desde el nombre interno**, con el mismo procedimiento que [`TEM-RECURSOS`](Modelado-de-Recursos.md) aplica al modelo. El mapa de traducción de nombres es parte del artefacto de salida del escenario:

| URI pública | Origen interno | Nota |
|---|---|---|
| `/salas` | `TB_SALA` | — |
| `/salas-de-reunion` | — | Descartado: redundante, el dominio solo tiene salas de reunión |
| `/reservas` | `TB_RESERVA_CAB` | El sufijo `_CAB` no se expone |
| `/sedes` | `TB_SUCURSAL` | El negocio dice «sede»; «sucursal» es vocabulario de 2011 |

La última fila es la más instructiva y la que genera más resistencia interna: el nombre interno y el nombre del negocio ya divergían antes de que existiera la API. Exponer `sucursal` porque «así se llama en el sistema» perpetúa un vocabulario que el negocio abandonó.

En la variante de migración de protocolo hay un riesgo específico: conservar los nombres de las operaciones SOAP como segmentos de ruta produce `/EjecutarReservaSala`, que es la peor combinación posible —verbo, `PascalCase` y granularidad de operación—.

### `ESC-3` — Evolución en producción

Las URIs publicadas están congeladas. Lo que se decide es cómo se nombra lo nuevo, y hay una tensión que no tiene solución limpia: si la convención vigente es mala, aplicarla a lo nuevo perpetúa el problema, y no aplicarla produce una API con dos convenciones.

**Esta guía recomienda** mantener la convención existente aunque sea mala, y cambiarla solo en un límite de versión mayor, donde el consumidor ya sabe que todo puede cambiar. La razón es que la predictibilidad —el único beneficio real de una convención— se destruye por la inconsistencia, no por la elección concreta. Una API íntegramente en `camelCase` es mejor que una mitad `camelCase` y mitad `kebab-case`, aun si `kebab-case` era la elección correcta.

Renombrar una URI existente es rompiente y se trata como tal, con el mecanismo de [`TEM-DEPR`](../50-Evolucion-y-Versionado/Deprecacion-y-Retiro.md). El paliativo habitual es servir la ruta vieja con un `301` a la nueva, y tiene un límite conocido: un `301` sobre `POST` o `PUT` no es transparente para todos los clientes, porque la conducta al redirigir un método no seguro no es uniforme entre bibliotecas HTTP. Sirve para `GET`; para el resto hay que mantener las dos rutas.

### `ESC-4` — Evaluación de una API ajena

**`ESC-4a`.** La nomenclatura se audita sobre la especificación, y es la parte de una auditoría que se puede automatizar casi por completo. Las preguntas productivas: ¿hay más de un casing? ¿hay verbos en alguna ruta? ¿hay singulares y plurales mezclados para colecciones? ¿algún segmento delata una tabla? ¿los identificadores son secuenciales? La última tiene consecuencias de seguridad y conviene reportarla a `ACT-07`.

Un hallazgo frecuente y específico de esta variante: la especificación declara una convención que el código no cumple en los endpoints agregados más tarde. La divergencia entre especificación e implementación se manifiesta primero en la nomenclatura porque es lo que ningún test verifica.

**`ESC-4b`.** Se recolectan las rutas observadas, se las tabula por segmento y se busca la excepción. El valor de esta técnica no está en catalogar la convención sino en encontrar dónde se rompe: **los endpoints que no siguen la convención dominante suelen ser los más nuevos, los más urgentes o los que resolvieron algo que el modelo no preveía**, y son los que más sorpresas guardan para un integrador.

Lo que no se puede saber desde afuera: si la convención está escrita en algún lado, si hay verificación automática, y si las inconsistencias son deliberadas. Se registra como observación, no como juicio.

### Qué cambia según el contexto

| Contexto | Qué cambia en la nomenclatura |
|---|---|
| `CTX-1` pública | Cada URI es contrato indefinido. El idioma se decide por la audiencia real. Los identificadores no pueden ser enumerables: es materia de veto de `ACT-07`. La convención se publica junto con la API |
| `CTX-2` interna | Renombrar es posible coordinando el despliegue, y conviene hacerlo antes de que el nombre malo se propague. Lo que no cambia es la necesidad de convención escrita: el equipo que se conoce rota |
| `CTX-3` app propia | El idioma es el del equipo sin discusión. El riesgo propio es que las rutas adopten vocabulario de la interfaz —`/pantalla-reservas`— porque el mismo equipo escribe las dos puntas |
| `CTX-4` integración | La nomenclatura es un dato. El trabajo consiste en no dejar que los nombres del proveedor se propaguen: si la pasarela llama `merchant` a lo que el dominio llama «sede», la traducción ocurre en la capa de aislamiento y no más adentro |

---

## Ejemplos concretos

Ejemplos **sintéticos** del sistema de reserva de salas.

### Superficie completa, con la convención de esta guía

```http
GET    /v1/sedes                                   # colección
GET    /v1/sedes/n1                                # elemento
GET    /v1/sedes/n1/salas                          # subcolección
GET    /v1/salas/a3f1                              # el mismo recurso, acceso directo
GET    /v1/salas/a3f1/disponibilidad               # recurso calculado
GET    /v1/salas/a3f1/reservas?desde=2026-08-01    # subcolección con filtro
POST   /v1/reservas                                # creación
GET    /v1/reservas/8f21c3                         # elemento
PATCH  /v1/reservas/8f21c3                         # modificación parcial
DELETE /v1/reservas/8f21c3                         # baja
GET    /v1/usuarios/u-771/preferencias             # singleton
GET    /v1/salas-de-uso-restringido                # kebab-case con nombre compuesto
```

Todo en minúsculas, colecciones en plural, sin verbos, sin extensiones, sin barra final, identificadores opacos, dos niveles de anidamiento como máximo.

### La misma superficie con las tres convenciones en conflicto

```http
# Zalando (G-05 regla 129) — kebab-case
GET /v1/salas-de-uso-restringido/a3f1/solicitudes-de-reserva

# Google (G-04 AIP-122) — camelCase
GET /v1/salasDeUsoRestringido/a3f1/solicitudesDeReserva

# Azure (G-01) — admite las dos, y por eso conviven en la misma API
GET /v1/salas-de-uso-restringido/a3f1/solicitudesDeReserva
```

Las tres son válidas según su propia guía. La tercera es la única que ninguna guía recomienda y la que más se encuentra en la realidad, porque es lo que pasa cuando no hay convención escrita.

### Antes y después de una migración `ESC-2`

```http
# Antes: el modelo interno filtrado
POST /api/TB_RESERVA_CAB/Insertar
GET  /api/getSalasBySucursal?idSuc=1
GET  /api/ReservaDetalle.aspx?idCab=8821

# Después
POST /v1/reservas
GET  /v1/sedes/n1/salas
GET  /v1/reservas/8f21c3
```

Cinco problemas corregidos de una vez: nombres de tabla, verbos en la ruta, mezcla de idiomas, identificadores secuenciales y una extensión de tecnología en la URI. El último merece atención: `.aspx` en una ruta pública ata el contrato a la plataforma, y migrar de plataforma pasa a ser un cambio rompiente.

### Peticiones completas

```http
GET /v1/sedes/n1/salas?capacidad-minima=8&ordenar=nombre&limite=20 HTTP/1.1
Host: api.reservas.ejemplo.com
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json
Vary: Accept
ETag: "3b7f01"

{
  "datos": [
    { "id": "a3f1", "nombre": "Belgrano", "capacidad": 12 },
    { "id": "b7c2", "nombre": "Rivadavia", "capacidad": 20 }
  ],
  "siguiente": "/v1/sedes/n1/salas?capacidad-minima=8&ordenar=nombre&limite=20&cursor=eyJ..."
}
```

El casing del parámetro de query es una decisión aparte de la del segmento de ruta y no tiene por qué coincidir. Zalando, deliberadamente, usa `kebab-case` en el path y `snake_case` en la query. El ejemplo usa `kebab-case` en los dos por simplicidad; la decisión corresponde a [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md).

```http
GET /v1/Salas/A3F1 HTTP/1.1
```

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://api.reservas.ejemplo.com/problemas/ruta-no-encontrada",
  "title": "Recurso no encontrado",
  "status": 404,
  "detail": "Las rutas de esta API distinguen mayúsculas. ¿Quiso decir /v1/salas/a3f1?"
}
```

Responder `404` a una diferencia de casing es la conducta correcta —son URIs distintas— y es hostil sin una pista. El `detail` cuesta poco y ahorra una consulta de soporte. El formato de la respuesta es `N-04` (RFC 9457) y lo trata [`TEM-ERR`](../40-Contratos-y-Representaciones/Manejo-de-Errores.md).

### En C#: hacer la convención explícita en el código

ASP.NET Core no impone ningún casing de ruta. La convención se cumple sola si cada endpoint la escribe a mano, lo cual funciona hasta el endpoint número treinta.

```csharp
// Cumple la convención por disciplina de quien escribe: frágil a escala.
app.MapGet("/v1/salas-de-uso-restringido/{salaId}", ObtenerSala);
app.MapGet("/v1/salasDeUsoRestringido/{salaId}", ObtenerSala);   // nadie lo va a notar en revisión
```

Con controllers, la transformación de tokens de ruta permite derivar el segmento del nombre del controlador y hacer que la convención sea estructural en lugar de voluntaria:

```csharp
// Convierte "SalasDeUsoRestringido" en "salas-de-uso-restringido"
public sealed class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null) return null;
        var texto = value.ToString()!;
        return string.Concat(texto.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
    }
}
```

```csharp
builder.Services.AddControllers(opciones =>
{
    opciones.Conventions.Add(
        new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
});
```

```csharp
[ApiController]
[Route("v1/[controller]")]                 // el token se transforma a kebab-case
public sealed class SalasDeUsoRestringidoController : ControllerBase
{
    [HttpGet("{salaId}")]                  // → /v1/salas-de-uso-restringido/{salaId}
    public Task<IResult> Obtener(string salaId) => /* … */;
}
```

Lo que este mecanismo compra es que la convención deje de depender de que alguien la recuerde. Lo que **no** compra es la elección de las palabras: `SalasDeUsoRestringido` sigue siendo una decisión humana, y el transformador la convierte en `kebab-case` sea buena o mala.

Para Minimal APIs no hay un equivalente directo, y la verificación se traslada a la especificación generada. La forma robusta, y la que **esta guía recomienda** en cualquiera de los dos estilos, es una prueba que recorra el documento OpenAPI:

```csharp
[Fact]
public async Task Todas_las_rutas_cumplen_la_convencion()
{
    var documento = await ObtenerDocumentoOpenApiAsync();
    var patronSegmento = new Regex(@"^[a-z][a-z0-9\-]*$");

    var infractoras = documento.Paths.Keys
        .SelectMany(ruta => ruta.Split('/', StringSplitOptions.RemoveEmptyEntries))
        .Where(segmento => !segmento.StartsWith('{'))     // los parámetros no aplican
        .Where(segmento => !patronSegmento.IsMatch(segmento))
        .Distinct()
        .ToArray();

    Assert.Empty(infractoras);
}
```

La misma verificación se puede hacer fuera del código con un *linter* de OpenAPI en la integración continua, que es la forma en que `ACT-01` ejerce su autoridad sin convertirse en cuello de botella. La organización concreta de ese pipeline es materia de [`FAM-ESP`](../60-Especificacion-y-Documentacion/).

---

## Preguntas guía

- ¿La convención de mi API está escrita en algún lado, o vive en la cabeza de quien escribió los primeros endpoints?
- ¿Hay una verificación automática que rechace un endpoint que la viola? Si no, ¿cuánto tardaría en detectarse una violación?
- Si le muestro tres de mis rutas a alguien, ¿puede adivinar la cuarta?
- ¿Qué guía estoy siguiendo, y sé por qué esa organización decidió así?
- ¿Mis identificadores le dicen a un extraño cuántos recursos tengo?
- ¿El idioma de mis rutas coincide con el de mis campos JSON? ¿Y con el del negocio?
- Si mañana cambio de plataforma, ¿alguna URI mía lo delata?
- ¿Alguna de mis rutas necesita que la lea dos veces para saber de qué se trata?

---

## Criterios de calidad

### Señales de una nomenclatura sana

Existe un documento de convenciones de una página con una regla por decisión. Cada regla tiene una verificación automática en la integración continua. Toda la superficie usa un solo casing, un solo número gramatical para colecciones y un solo idioma. Ningún segmento contiene un verbo, una extensión de tecnología ni un nombre de tabla. Los identificadores de recursos con dueño no son enumerables. Y las excepciones a la convención están documentadas como excepciones, con su razón.

### Antipatrones

**El verbo en la ruta.** `POST /crearReserva`. El más frecuente y el más costoso: descarta caché, idempotencia y semántica de estado de un solo movimiento. Se detecta con una regla automática que busque infinitivos y gerundios en los segmentos.

**El casing mixto.** Dos o más casings en la misma superficie. Casi siempre es consecuencia de no tener convención escrita, no de un desacuerdo. Su costo se paga en cada integración: el consumidor no puede predecir y tiene que buscar cada ruta.

**Singulares y plurales mezclados.** `/sala/a3f1` junto a `/reservas/8f21c3`. Misma causa, mismo costo, y más difícil de detectar automáticamente porque requiere saber qué es colección.

**El idioma híbrido.** `/salas/{id}/bookings`. Obliga a recordar en qué idioma se nombró cada segmento. Es peor que cualquiera de los dos idiomas puros.

**El identificador secuencial en recurso con dueño.** `/reservas/1`. Revela volumen, ritmo y existencia, y habilita la enumeración. Es hallazgo de seguridad en `CTX-1`, no observación de estilo.

**La extensión de tecnología.** `.aspx`, `.php`, `.do` en la ruta. Ata el contrato a la plataforma; migrarla se convierte en cambio rompiente. La variante moderna es `/api/v1/...` donde `api` no aporta nada salvo cuando comparte host con una aplicación web, caso en que sí es legítimo.

**El anidamiento innecesario.** `/sedes/n1/salas/a3f1/reservas/8f21c3/asistentes/2`. Cada nivel compromete una relación. Lo trata [`TEM-JERARQ`](Jerarquias-y-Relaciones.md).

**Las dos formas para una cosa.** Servir `/salas` y `/Salas`, o `/salas` y `/salas/`, con la misma respuesta. Duplica entradas de caché, rompe la comparación de URIs y le enseña al consumidor una equivalencia que en algún endpoint no se va a cumplir.

**La convención documentada sin verificación.** El antipatrón organizativo, y el que produce todos los demás con el tiempo. Un documento de convenciones sin *linter* dura lo que dura la atención del revisor.

**La abreviatura de conveniencia.** `/res/{id}` por reservas, `/usr` por usuarios. Ahorra caracteres que nadie escribe a mano y cuesta una consulta a la documentación cada vez.

---

## Anexo — Plantilla de convenciones de URI

Se completa una vez por API o por organización, se publica junto con la especificación, y cada línea debe tener una regla automática asociada. Si una decisión no se puede verificar automáticamente, conviene revisar si está formulada con la precisión suficiente.

```yaml
convencion_uri:
  version: "1.0"
  alcance: ""                      # una API, un dominio funcional, toda la organización
  autoridad_base: propia | G-01 | G-04 | G-05 | G-06
  # si se adopta una guía externa, se listan las desviaciones abajo

  casing_segmento: kebab-case | camelCase | lowercase
    regex: "^[a-z][a-z0-9\\-]*$"
  numero_colecciones: plural | singular
    excepciones: [singletons]
  idioma: es | en
    justificacion: ""              # quién lee estas URIs
    terminos_sin_traducir: []      # los del dominio que se dejan como están

  identificadores:
    formato: opaco | uuid | slug | secuencial
    enumerables: si | no           # "si" requiere aprobación de ACT-07 en CTX-1
    regex: ""
    longitud_maxima: 0

  estructura:
    prefijo_version: "/v{n}" | ninguno
    profundidad_maxima_colecciones: 2
    barra_final: sin-barra | con-barra
    conducta_no_canonica: redirigir-301 | rechazar-404
    extensiones_de_archivo: no | solo-descargas

  reparto:
    en_ruta: [identificacion, jerarquia]
    en_query: [filtrado, orden, paginacion, seleccion-de-campos]

  verificacion:
    herramienta: ""                # linter de OpenAPI, prueba automatizada
    momento: pull-request | integracion-continua | ambos
    bloquea_merge: si | no

  desviaciones_aceptadas:
    - ruta: ""
      regla_violada: ""
      razon: ""
      fecha: ""
```

Las dos secciones que hacen la diferencia son `verificacion` y `desviaciones_aceptadas`.

Sin `verificacion`, el documento entero es una declaración de intenciones que se erosiona endpoint por endpoint sin que nadie lo advierta; el campo `bloquea_merge` es el que separa una convención real de una aspiración.

`desviaciones_aceptadas` cumple una función distinta y menos obvia: registrar las excepciones **con fecha y razón** evita que se conviertan en precedente. Una ruta que viola la convención y está registrada como excepción es un caso; tres rutas que la violan sin registro son una convención nueva que nadie decidió.
