---
doc_id: TEM-ARQ
doc_type: tema
title: Arquitectura y REST
status: vigente
origin: ia-assisted
confidence: media
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-TRA, TEM-DX, TEM-SDD, TEM-REST, TEM-RECURSOS, TEM-JERARQ, TEM-PAG, TEM-VERS, TEM-RESIL, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Arquitectura y REST — `TEM-ARQ`

## Resumen ejecutivo

Una API REST no flota. Está sostenida por una arquitectura concreta —un monolito, un conjunto de servicios, una aplicación organizada en capas— y esa arquitectura condiciona qué puede ofrecer el contrato, qué cuesta cambiarlo y qué hay que documentar para que alguien lo entienda. La relación se suele leer en una sola dirección, como si la arquitectura fuera una decisión previa que la API se limita a exponer. Corre en las dos: la frontera de un servicio se materializa en una superficie HTTP, y una superficie HTTP mal cortada produce una frontera de servicio que después nadie puede mover.

Este documento recorre cinco modelos —cliente-servidor, modelo de capas, monolítico, hexagonal y microservicios— desde una única pregunta: qué le hace cada uno a la API, y qué le exige la API a cada uno en materia de documentación. Cierra con el patrón *Backend for Frontend*, que es el caso donde la decisión arquitectónica y la decisión de contrato son literalmente la misma decisión.

El tratamiento general de esos modelos vive en otra parte del repositorio y no se repite acá. La guía de documentación técnica los desarrolla en [`ARQ-INDICE`](../../Documentacion-Tecnica/90-Modelos-de-Arquitectura/README.md) con un documento por modelo; la guía de organización de código desarrolla el eje de las unidades desplegables en [`FAM-SRV`](../../Organizacion-Estilo-Patrones-Codigo/10-Arquitectura-de-Servicios/README.md) y el de la organización interna en [`TEM-CAPAS`](../../Organizacion-Estilo-Patrones-Codigo/30-Organizacion-Interna/Modelos-de-Capas.md). Lo que sigue es la lectura que ninguna de las dos hace: la del protocolo.

---

## Definición

### Qué es

La relación entre el modelo de arquitectura de un sistema y el diseño de su interfaz HTTP: qué decisiones de contrato quedan determinadas por la arquitectura, cuáles quedan libres, y cuáles —las más interesantes— determinan la arquitectura en lugar de derivarse de ella.

Hay una asimetría que ordena todo el documento. La arquitectura fija **el espacio de lo posible**: un monolito no puede ofrecer disponibilidad diferenciada por recurso, y un conjunto de servicios con datos propios no puede garantizar atomicidad entre dos de ellos. Dentro de ese espacio, el diseño de la API es libre, y esa libertad es lo que hace que dos sistemas con arquitecturas idénticas tengan contratos que no se parecen en nada.

### Qué problema resuelve

**Que la API refleje el organigrama en lugar del dominio.** Es el síntoma más frecuente y el más difícil de argumentar en una reunión. Cuando cuatro equipos publican cuatro superficies HTTP con cuatro formatos de error, cuatro criterios de paginación y cuatro convenciones de nomenclatura, el consumidor recibe cuatro APIs aunque el producto sea uno. La arquitectura no lo causó; lo permitió, y no haber decidido nada en el nivel de la superficie lo consumó.

**Que la frontera de servicio se descubra tarde.** Partir un sistema es una decisión que la guía hermana trata con criterios y umbrales propios en [`TEM-PART`](../../Organizacion-Estilo-Patrones-Codigo/10-Arquitectura-de-Servicios/Criterios-de-Particion.md). Lo que agrega la perspectiva del contrato es un indicador temprano y barato: si dos recursos de la misma API se leen y se escriben siempre juntos, en la misma transacción, la línea que los separa no es una frontera de servicio, por más que el diagrama la dibuje.

### Qué **no** es

**No es una elección entre arquitecturas.** Este documento no dice cuándo conviene partir un sistema; eso lo dicen los cinco criterios de [`TEM-PART`](../../Organizacion-Estilo-Patrones-Codigo/10-Arquitectura-de-Servicios/Criterios-de-Particion.md), con sus umbrales declarados como criterio propio de aquella guía. Acá se asume el modelo dado y se pregunta qué se sigue para la API.

**No es la organización interna del código.** Que la solución tenga cuatro proyectos, que el dominio no referencie a Entity Framework o que los *handlers* estén en carpetas por *feature* son decisiones que no llegan al cable. Un consumidor no puede distinguir por observación una API implementada con arquitectura hexagonal de una implementada con todo en un archivo. La distinción importa —la primera envejece mejor— pero no es una propiedad del contrato.

**No es sinónimo de «arquitectura de microservicios».** La asociación entre REST y microservicios es histórica y no conceptual. La restricción cliente-servidor de `O-01` §5.1.2 describe una arquitectura de dos partes, y buena parte de las APIs REST del mundo viven dentro de un único despliegue.

**Y con qué se lo confunde.** Con el modelo de despliegue. Que una API se sirva desde tres réplicas detrás de un balanceador, o desde una función sin servidor, o desde un contenedor por región, es una decisión de operación que la restricción de estado de `O-01` §5.1.3 habilita y que el contrato no debería revelar. Cuando la revela —porque el consumidor tiene que mandar la petición al nodo que atendió la anterior— la restricción se está violando, y eso sí es un problema de arquitectura visible desde afuera.

---

## Cliente-servidor: la restricción que ya es una arquitectura

Es la primera restricción que `O-01` §5.1.2 agrega sobre el estilo nulo, y es un modelo de arquitectura completo antes de ser otra cosa. Lo que prohíbe es que la interfaz de usuario y el almacenamiento de datos vivan en el mismo componente; lo que compra es que ambos evolucionen y escalen por separado. [`TEM-REST`](../10-Fundamentos-REST/Que-es-REST.md) lo desarrolla junto con las otras cinco restricciones y ahí se cita la tabla completa.

Lo que corresponde retener acá es que **todos los modelos posteriores presuponen esta separación y ninguno la deroga**. Un monolito con API HTTP es cliente-servidor; una malla de cuarenta servicios es cliente-servidor cuarenta veces, con cada servicio ocupando ambos roles según la llamada. La restricción no distingue entre esos casos porque opera en un nivel más básico: dice que hay una frontera y que la frontera es el contrato.

La consecuencia que más se pierde es la de la **separación de intereses**. La disertación justifica la restricción diciendo que permite que los componentes evolucionen de forma independiente, lo cual solo es cierto si el contrato es más estable que las dos partes que conecta. Un contrato que cambia cada vez que cambia el cliente, o cada vez que cambia la base de datos, no está separando nada: está transmitiendo. La cita de `O-02` sobre que los servidores deben conservar la libertad de controlar su propio espacio de nombres apunta al mismo problema desde el otro lado.

---

## El modelo de capas: dónde vive el contrato

Acá hay una homonimia que produce confusiones reales y conviene desarmarla antes de seguir.

**El «sistema por capas» de `O-01` §5.1.6** es una restricción sobre lo que un componente puede ver: cada uno habla con la capa adyacente y no sabe si hay algo más allá. Lo que compra son los intermediarios —balanceadores, cachés compartidas, pasarelas—, que pueden interponerse sin que ninguna de las dos puntas se entere. Es una propiedad de la red, no del código.

**El «modelo de capas» como arquitectura de aplicación** —presentación, aplicación, dominio, infraestructura— es otra cosa: una restricción sobre las dependencias de compilación dentro de un despliegue. Su tratamiento canónico está en [`ARQ-CAPAS`](../../Documentacion-Tecnica/90-Modelos-de-Arquitectura/Modelo-de-Capas.md) y en [`TEM-CAPAS`](../../Organizacion-Estilo-Patrones-Codigo/30-Organizacion-Interna/Modelos-de-Capas.md), que además argumenta que hexagonal, *onion* y *clean* dicen lo mismo con vocabularios distintos.

Con la distinción hecha, la pregunta de este documento es dónde vive el contrato en el segundo sentido. Esta guía recomienda una respuesta que suena obvia y se incumple constantemente: **el contrato es la capa más externa y no tiene por debajo ninguna dependencia que lo defina**. En concreto, los tipos que se serializan a JSON pertenecen a la frontera, no al dominio.

El incumplimiento típico se ve en dos formas. La primera es exponer directamente la entidad de persistencia: `Reserva`, con sus propiedades de navegación, su clave técnica y su marca de fila de concurrencia, se devuelve tal cual, y a partir de ese momento cualquier cambio en el modelo de datos es un cambio rompiente para el consumidor. La segunda es la inversa y menos evidente: hacer que el dominio conozca los tipos de la API, porque «así no hay que mapear». El costo del mapeo es real y hay que declararlo —lo hace [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) al describir `ESC-2`— pero es el precio de que el contrato público y la base de datos puedan evolucionar a ritmos distintos.

De la lectura de la capa también sale un criterio de nomenclatura que [`TEM-CAMPOS`](../40-Contratos-y-Representaciones/Formato-y-Nomenclatura-de-Campos.md) desarrolla: cuando el nombre de un campo JSON coincide exactamente con el nombre de una columna, incluida su abreviatura y su prefijo, lo que se está observando no es una coincidencia feliz sino la ausencia de una capa.

---

## Monolítico: una API interna dentro de un solo despliegue

Una aplicación de un solo despliegue puede tener una API HTTP interna, y con frecuencia la tiene. La pregunta es cuándo eso aporta y cuándo es ceremonia.

**Cuándo tiene sentido.** El caso claro es `CTX-3`: la aplicación tiene un cliente que se despliega por separado —una aplicación Blazor WebAssembly, una MAUI, una vista MVC que llama por AJAX— y la API es la frontera real entre dos artefactos que viajan por caminos distintos. Ahí la API no es una decisión interna: es la única forma de que el cliente hable con el servidor, y todo el aparato de esta guía aplica sin descuento.

El segundo caso es el de la API como **frontera preparada**. Un monolito modular cuyos módulos se comunican por llamadas en proceso puede exponer, además, una superficie HTTP para el módulo que tiene más probabilidad de separarse. La ventaja no es técnica sino documental: obliga a escribir el contrato antes de necesitarlo, y el contrato escrito es lo que después permite decidir si la separación es viable. Esta guía recomienda esa práctica con una advertencia: sirve para uno o dos módulos identificados, no para todos, y no reemplaza los criterios de [`TEM-PART`](../../Organizacion-Estilo-Patrones-Codigo/10-Arquitectura-de-Servicios/Criterios-de-Particion.md).

**Cuándo es ceremonia.** Cuando el consumidor de la API es el mismo proceso que la sirve. Un componente Blazor en render *interactive server* que llama por `HttpClient` a un endpoint de su propia aplicación paga serialización, deserialización, una vuelta por la pila de red y la pérdida del contexto de excepción, a cambio de nada: el código del componente se ejecuta en el servidor y podría invocar el servicio de aplicación directamente. [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) señala exactamente ese caso al describir `CTX-3` y explicar que ese render se comporta como `CTX-2`. La distinción no es cosmética: cambia dónde viven las credenciales y qué latencia se paga.

El síntoma de la ceremonia es fácil de reconocer. Si nadie fuera del despliegue puede llamar al endpoint, si no hay especificación publicada, si el «cliente» es una clase del mismo ensamblado y si un cambio en el contrato se despliega junto con el consumidor, entonces lo que hay es un método con sintaxis HTTP.

---

## Hexagonal: la API REST como adaptador primario

En el vocabulario de puertos y adaptadores, la API REST es un **adaptador primario**, también llamado conductor: un mecanismo por el cual el mundo exterior invoca al núcleo de la aplicación. Está del mismo lado que la interfaz de línea de comandos, que el consumidor de mensajes y que el trabajo programado, y del lado opuesto al repositorio o al cliente de la pasarela de pagos, que son adaptadores secundarios.

El tratamiento general del modelo está en [`ARQ-HEX`](../../Documentacion-Tecnica/90-Modelos-de-Arquitectura/Hexagonal.md) y, con más profundidad para .NET, en [`TEM-CAPAS`](../../Organizacion-Estilo-Patrones-Codigo/30-Organizacion-Interna/Modelos-de-Capas.md). Lo que agrega la lectura desde el protocolo son tres consecuencias.

**Primera: el modelo de recursos no es el modelo de dominio.** Si la API es un adaptador, entonces traduce, y traducir significa que las dos orillas tienen vocabularios distintos. Un agregado del dominio puede exponerse como tres recursos, y tres entidades pueden exponerse como uno solo. La correspondencia uno a uno entre clase de dominio y recurso HTTP es un caso particular, no la norma, y [`TEM-RECURSOS`](../20-Diseno-de-Recursos/Modelado-de-Recursos.md) desarrolla el criterio de modelado que reemplaza esa correspondencia automática.

**Segunda: si un cambio en el dominio obliga a un cambio en el contrato, el adaptador no está adaptando.** Es la prueba operativa que esta guía recomienda usar, porque es verificable con el historial: renombrar una propiedad de una entidad no debería producir un cambio rompiente en la API. Cuando lo produce, hay un serializador apuntando directo al dominio, y toda la ventaja del modelo se perdió en el único lugar donde importaba.

**Tercera: hay varios adaptadores primarios y comparten el núcleo.** Es lo que hace que la operación «cancelar una reserva» tenga que existir como caso de uso invocable, y no como el cuerpo de un método de controlador. Un endpoint `DELETE /reservas/{id}` y un consumidor de mensajes que procesa cancelaciones en lote deben ejecutar la misma regla de las veinticuatro horas de antelación; si cada uno la implementa por su lado, hay dos verdades y una va a divergir. Es también el argumento que justifica que las operaciones no CRUD tengan tratamiento propio en [`TEM-ACCIONES`](../20-Diseno-de-Recursos/Operaciones-No-CRUD.md): son casos de uso del núcleo que hay que expresar en una interfaz uniforme, no accidentes del diseño.

La tentación simétrica también existe y se ve poco. Cuando el adaptador primario se vuelve tan grueso que contiene reglas de negocio —validaciones de dominio escritas en el filtro del endpoint, decisiones de estado tomadas en el controlador— el núcleo deja de ser invocable desde otro adaptador sin duplicar esas reglas.

---

## Microservicios: la API como frontera de servicio

Cuando un sistema se parte en unidades con despliegue independiente y datos propios, cada unidad publica una superficie y esa superficie **es** la frontera. Todo lo que no está en el contrato es privado por construcción, y todo lo que está en el contrato es un compromiso con otro equipo.

### Granularidad de servicios frente a granularidad de recursos

Es la relación que ninguna de las dos guías hermanas trata, y la fuente de un error de razonamiento frecuente: suponer que las dos granularidades tienen que coincidir. No coinciden, y confundirlas produce los dos extremos.

Un servicio expone normalmente **varios recursos**. El servicio de reservas del dominio de ejemplo expone reservas, sus confirmaciones y sus cancelaciones; que sean tres recursos no significa que deban ser tres servicios. La granularidad del recurso responde a «qué cosa tiene identidad y ciclo de vida propios», una pregunta de modelado que trata [`TEM-RECURSOS`](../20-Diseno-de-Recursos/Modelado-de-Recursos.md); la granularidad del servicio responde a «qué cosa conviene desplegar y escalar por separado», una pregunta de operación que tratan los cinco criterios de [`TEM-PART`](../../Organizacion-Estilo-Patrones-Codigo/10-Arquitectura-de-Servicios/Criterios-de-Particion.md).

De la independencia entre ambas se sigue una regla que esta guía sí recomienda en sentido fuerte: **un recurso no se parte entre dos servicios**. Si `GET /reservas/{id}` necesita consultar a dos servicios para armar su respuesta, y ambos son dueños de campos de la misma representación, el corte pasó por el medio de una entidad. El síntoma clínico es que agregar un campo a la reserva requiere coordinar dos despliegues; el diagnóstico es que no hay dos servicios sino uno mal repartido.

El extremo opuesto —un servicio por recurso— produce lo que la guía hermana llama nanoservicio, y en la superficie se reconoce porque el consumidor necesita orquestar cinco llamadas para completar una operación que el dominio considera una sola.

### El monolito distribuido, visto desde el cable

El diagnóstico canónico está en [`TEM-MICRO`](../../Organizacion-Estilo-Patrones-Codigo/10-Arquitectura-de-Servicios/Microservicios.md), con sus dos síntomas —base de datos compartida y despliegues coordinados—. Desde la API se observan tres señales adicionales que no requieren acceso al código y que por eso sirven en `ESC-4b`:

Cadenas sincrónicas profundas. Una petición que atraviesa cuatro servicios antes de responder tiene la disponibilidad del producto de las cuatro y la latencia de la suma. [`TEM-RESIL`](../70-Seguridad-y-Robustez/Resiliencia-y-Reintentos.md) trata qué hacer con eso; acá interesa que es un indicio de que la partición no aisló fallas, que era una de las cosas que prometía comprar.

Contratos que cambian juntos. Si las notas de versión de tres servicios tienen siempre la misma fecha y describen el mismo cambio, los tres se despliegan juntos aunque se compilen aparte.

Tipos de dominio compartidos por la superficie. Cuando el mismo objeto JSON —con los mismos campos, incluidos los que un servicio no usa— aparece en las respuestas de varios servicios, es probable que haya una biblioteca de contratos común que todos referencian. Esa biblioteca es cómoda y es el mecanismo por el cual un cambio se propaga a todos a la vez.

### Qué cambia en el contrato

En un sistema partido, el consumidor de la mayoría de las APIs es otro servicio, es decir `CTX-2`. Eso relaja algunas cosas y endurece otras. Se relaja el versionado formal, porque el despliegue se puede coordinar; se relaja la necesidad de un portal de documentación, porque el consumidor es identificable. Se endurece, en cambio, todo lo que tiene que ver con la falla parcial: los códigos de estado dejan de ser un detalle estético y pasan a gobernar la política de reintento del llamante, y la distinción entre `503` y `429` —que trata [`TEM-STATUS`](../30-Semantica-HTTP/Codigos-de-Estado.md)— determina si el cliente espera o desiste. La observabilidad distribuida, que [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) señala como preocupación propia de `CTX-2`, se apoya en la propagación de contexto por cabecera y por lo tanto es también una decisión de contrato.

---

## El patrón *Backend for Frontend*

Un BFF es una API diseñada para un cliente concreto en lugar de para la generalidad del dominio: agrega en una respuesta lo que la pantalla necesita, con la forma en que la pantalla lo necesita, evitando que el cliente haga cinco llamadas y componga.

Es la aplicación directa de `CTX-3`, y [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) ya enuncia su condición de legitimidad: **es válido mientras el cliente sea uno**. Deja de serlo cuando aparece el segundo con necesidades distintas y el mismo backend empieza a acumular endpoints a medida para cada pantalla de cada aplicación. En ese punto hay dos salidas honestas —dos BFF separados, cada uno con su dueño, o una API de dominio con un mecanismo de selección de campos como el que trata [`TEM-FILTRO`](../40-Contratos-y-Representaciones/Filtrado-Orden-y-Seleccion.md)— y una salida deshonesta, que es seguir agregando y llamarlo API general.

Ninguna de las dos guías hermanas desarrolla el patrón: la de código lo roza al preguntarse cómo compone la interfaz datos de varios servicios, sin nombrarlo. Lo que esta guía agrega son tres precisiones de contrato.

**El BFF es un consumidor, no solo un productor.** Hacia arriba sirve a una aplicación (`CTX-3`); hacia abajo consume APIs de dominio (`CTX-2`) o de terceros (`CTX-4`). Esa doble condición lo convierte en el lugar natural donde traducir errores ajenos al formato propio, tarea que [`TEM-ERR`](../40-Contratos-y-Representaciones/Manejo-de-Errores.md) desarrolla, y donde absorber la resiliencia para que el cliente no tenga que implementarla.

**El riesgo dominante es que el contrato se modele según la pantalla.** El endpoint `/pantallaReservas`, que devuelve exactamente los campos que esa vista dibuja, convierte cualquier rediseño visual en un cambio de backend. La recomendación de esta guía es que un BFF agregue y filtre, pero nombre recursos del dominio: `GET /reservas?incluir=sala,solicitante` es agregación; `GET /pantallaReservas` es acoplamiento a la interfaz.

**Un BFF que sirve a un cliente instalado hereda las restricciones de `CTX-1`.** Una aplicación MAUI en el teléfono de un usuario no se actualiza cuando el backend se despliega. La cercanía organizativa —mismo equipo, mismo repositorio— engaña sobre la libertad real de cambio, y es el error que [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) marca como origen de la mayoría de las rupturas evitables.

---

## Qué documentación exige cada modelo

Cada modelo compra propiedades a cambio de artefactos que hay que producir y mantener. Los de la columna derecha no son opcionales en el sentido de que, sin ellos, la propiedad que el modelo prometía no se obtiene.

| Modelo | Qué compra en la API | Artefactos que exige del lado del contrato |
|---|---|---|
| Cliente-servidor | Evolución independiente de las dos puntas | Especificación del contrato; declaración de qué es estable y qué no |
| Modelo de capas | Que el contrato sobreviva a cambios internos | Mapa recurso ↔ modelo interno; catálogo de tipos de la frontera |
| Monolítico | Simplicidad operativa, atomicidad local | Especificación mínima y colección ejecutable; casi nada más si es `CTX-2` |
| Hexagonal | Que el dominio no se filtre | Inventario de casos de uso y su correspondencia con operaciones HTTP |
| Microservicios | Despliegue y escalado independientes | Una especificación por servicio, catálogo de servicios, política de versionado y compatibilidad, contrato de propagación de traza |
| BFF | Menos llamadas por pantalla | Dueño declarado del BFF; inventario de las APIs que consume; mapa pantalla ↔ endpoint |

La fila de microservicios concentra el costo, y es el punto donde la decisión arquitectónica se paga en trabajo documental sostenido. Un catálogo de servicios desactualizado es peor que no tenerlo, porque produce integraciones contra contratos que ya no existen. La correspondencia con la guía hermana de documentación técnica es directa: el contrato de cada servicio es un [`DOC-API`](../../Documentacion-Tecnica/40-Diseno/API-Specification.md) y las decisiones de partición se registran como ADR.

---

## Aplicación por escenario

### `ESC-1` — API nueva

La arquitectura se está decidiendo al mismo tiempo que el contrato, y esa simultaneidad es una ventaja que casi nunca se aprovecha. Lo que conviene hacer acá es escribir el modelo de recursos **antes** de fijar la partición en servicios, porque el modelo de recursos revela qué cosas se leen y se escriben juntas, que es precisamente el dato que necesita el primer criterio de partición.

La trampa del escenario en esta materia es partir de entrada. Un sistema nuevo cuyos límites de dominio nadie conoce todavía, partido en seis servicios el primer mes, produce fronteras arbitrarias que después son caras de mover porque cada una ya es un contrato. Esta guía recomienda una API interna dentro de un despliegue único, con módulos de límites explícitos, y la partición como decisión posterior con evidencia.

### `ESC-2` — Exposición o migración

Es el escenario donde el modelo de capas gobierna todo. Hay un sistema interno con su propia estructura y hay que ponerle una superficie encima; la tensión que describe [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) —el modelo interno empuja hacia una API que lo refleja— es exactamente el problema del adaptador que no adapta.

La lectura hexagonal aporta acá un instrumento concreto: tratar la API nueva como un adaptador primario sobre un núcleo que todavía no existe, e ir extrayendo ese núcleo detrás de la superficie. El artefacto de salida que el escenario exige, el mapa explícito entre cada recurso y lo que lo respalda internamente, es literalmente la documentación del adaptador.

### `ESC-3` — Evolución en producción

La arquitectura está dada y el contrato tiene consumidores. Lo que cambia acá es que **la arquitectura ya no se puede modificar sin pasar por el contrato**: extraer un servicio, unificar dos, mover un recurso de un despliegue a otro son operaciones que el consumidor no debería percibir, y que solo son invisibles si el contrato no expone la estructura interna.

Es donde se cobra la deuda de las decisiones de `ESC-1`. Una API cuyas rutas revelan el servicio que atiende cada una —`/servicio-reservas/v1/reservas`— convirtió su topología en parte del contrato público, y no puede reorganizarla sin romper. La recomendación de esta guía es que el nombre del servicio no aparezca nunca en la URI pública; si hace falta enrutar, eso es trabajo de la pasarela, que es un intermediario en el sentido de `O-01` §5.1.6 y por lo tanto debe ser invisible.

### `ESC-4` — Evaluación de una API ajena

En `ESC-4a`, con la especificación o el código a la vista, la evaluación arquitectónica es directa: se contrasta el modelo de recursos con la estructura interna y se busca la filtración. El hallazgo típico no es que la arquitectura sea mala sino que el contrato la revela.

En `ESC-4b` hay que inferir, y conviene registrar lo inferido como hipótesis. Las señales útiles son las tres del apartado de monolito distribuido, más las cabeceras, que a menudo delatan la pasarela y el marco de trabajo, y la forma de los identificadores, que sugiere si hay una base de datos o varias. Nada de esto es concluyente y todo debe declararse con su nivel de confianza, según prescribe el escenario.

### Qué cambia según el contexto

| | `CTX-1` Pública | `CTX-2` Interna | `CTX-3` App propia | `CTX-4` Integración |
|---|---|---|---|---|
| **Qué revela la arquitectura** | Nada; la topología es privada | Se acepta que se note; el consumidor coordina | Nada hacia el cliente instalado | La ajena se padece; se aísla |
| **Modelo dominante** | Cualquiera, con pasarela por delante | Microservicios o modular | Monolítico o BFF | Adaptador secundario propio |
| **Artefacto crítico** | Especificación publicada y estable | Catálogo de servicios y trazas | Mapa pantalla ↔ endpoint | Capa de aislamiento del proveedor |
| **Riesgo propio** | Cristalizar la estructura interna en URIs | Monolito distribuido | Contrato modelado según la pantalla | Que el modelo del proveedor circule por dentro |

El cruce más instructivo es `CTX-4` con hexagonal, porque invierte los roles: al consumir una API ajena, el cliente HTTP que se escribe es un **adaptador secundario**, y el riesgo que [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) declara dominante en ese contexto —que los tipos del proveedor circulen por todo el sistema— es exactamente el fallo de no haber puesto un puerto.

---

## Ejemplos concretos

Los ejemplos son sintéticos, del dominio de reserva de salas, y se declaran como tales.

### El corte que parte un recurso

Un sistema partido en un servicio de salas y un servicio de reservas. La representación de una reserva incluye el nombre y la capacidad de la sala, porque el consumidor los necesita para mostrar la lista:

```http
GET /v1/reservas/8f3c HTTP/1.1
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": "8f3c",
  "desde": "2026-08-14T10:00:00Z",
  "hasta": "2026-08-14T11:30:00Z",
  "estado": "confirmada",
  "sala": { "id": "a3f1", "nombre": "Sarmiento", "capacidad": 12 }
}
```

Si `nombre` y `capacidad` son propiedad del servicio de salas y `desde`, `hasta` y `estado` del de reservas, cada `GET` implica una llamada interna, y agregar un campo a la sala requiere tocar los dos. Hay dos salidas correctas y una incorrecta.

La primera correcta es **enlazar en lugar de embeber**: la reserva devuelve el identificador de la sala y el consumidor resuelve el detalle si lo necesita. Cuesta una llamada más y hace explícito el límite.

La segunda es **duplicar deliberadamente lo estable**: el servicio de reservas guarda una copia del nombre de la sala al momento de reservar, y la devuelve como dato histórico. No es una desnormalización perezosa, es una decisión de dominio —la reserva del año pasado se hizo sobre la sala que se llamaba así entonces— y conviene documentarla como tal, porque un lector la va a leer como inconsistencia.

La incorrecta es dejar la llamada sincrónica y no declararla. La API funciona, el corte queda oculto, y la disponibilidad real del endpoint es el producto de dos que nadie midió.

### La URI que congela la topología

```http
GET /api/servicio-reservas/v1/reservas?desde=2026-08-01 HTTP/1.1
```

Funciona y es cómodo mientras la pasarela enruta por el primer segmento. El día en que reservas y salas se unifican —porque los cinco criterios de partición dejaron de sostener el corte— hay que elegir entre romper a los consumidores o mantener para siempre un segmento que nombra un servicio que ya no existe. La alternativa no tiene costo en `ESC-1`:

```http
GET /v1/reservas?desde=2026-08-01 HTTP/1.1
```

### El adaptador que no adapta, en C#

```csharp
// Fuga: la entidad de persistencia es el contrato.
app.MapGet("/v1/reservas/{id}", async (string id, ReservasDbContext db) =>
    await db.Reservas.Include(r => r.Sala).FirstOrDefaultAsync(r => r.Id == id)
        is { } reserva ? Results.Ok(reserva) : Results.NotFound());
```

Todo cambio de esquema es un cambio de contrato: renombrar una propiedad rompe, agregar una navegación agrega campos que nadie decidió publicar, y la marca de concurrencia viaja al cliente sin que nadie lo haya querido. La versión con frontera explícita separa las dos evoluciones:

```csharp
// El adaptador traduce. El tipo de la frontera es una decisión de contrato.
app.MapGet("/v1/reservas/{id}", async (string id, IConsultarReserva consulta) =>
    await consulta.PorIdAsync(id) is { } reserva
        ? TypedResults.Ok(ReservaResponse.Desde(reserva))
        : Results.NotFound());
```

`ReservaResponse` existe para poder cambiar de un lado sin cambiar del otro; ese es todo su propósito y es suficiente. La discusión sobre cómo se organizan estos tipos en la solución corresponde a [`TEM-MODELOS`](../../Organizacion-Estilo-Patrones-Codigo/30-Organizacion-Interna/Modelos-y-Contratos.md), y la de si el endpoint se escribe como Minimal API o como controller, a [`TEM-MINIMAL`](../80-Implementacion-en-NET/Minimal-APIs-y-Controllers.md).

### BFF frente a API de dominio

La pantalla de agenda diaria necesita, para cada sala de una sede, las reservas del día y si el usuario puede reservarla. Contra una API de dominio son tres llamadas y una composición en el cliente. Contra un BFF:

```http
GET /bff/agenda?sede=centro&fecha=2026-08-14 HTTP/1.1
```

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "fecha": "2026-08-14",
  "salas": [
    { "id": "a3f1", "nombre": "Sarmiento", "capacidad": 12,
      "puedeReservar": true,
      "reservas": [ { "id": "8f3c", "desde": "10:00", "hasta": "11:30" } ] }
  ]
}
```

Es una decisión razonable en `CTX-3` y tiene dos costos que conviene tener escritos. El campo `puedeReservar` es una decisión de autorización precalculada para una pantalla: si mañana la regla cambia, cambia el backend. Y las horas sin fecha ni zona son un formato para dibujar, no para procesar; un segundo consumidor que quiera calcular con ellas va a pedir el formato completo, y ahí empieza la acumulación que vuelve ilegítimo al patrón.

---

## Preguntas guía

- ¿Qué decisiones de mi contrato están determinadas por la arquitectura y cuáles las tomé yo sin advertirlo?
- Si renombro una propiedad de una entidad de dominio, ¿se rompe algún consumidor? ¿Por qué?
- ¿Hay algún recurso cuya representación necesite datos de más de un servicio? ¿Quién es el dueño de cada campo?
- ¿Mis URIs públicas nombran servicios, equipos o despliegues? ¿Qué pasa cuando alguno de los tres cambie?
- ¿La API interna que estoy publicando tiene algún consumidor fuera de este proceso?
- Si esto es un BFF, ¿quién es su dueño, cuántos clientes tiene hoy y qué hago cuando aparezca el segundo?
- ¿Cuántos saltos sincrónicos hay entre la petición del usuario y la respuesta, y qué disponibilidad compuesta implica?

---

## Criterios de calidad

### Aplicación buena

El contrato no revela la topología: ni el nombre del servicio, ni la tecnología, ni el número de despliegues aparecen en la superficie. Cada recurso tiene un único dueño y ese dueño es un servicio. Existe un mapa entre los recursos publicados y lo que los respalda internamente, y está al día porque se revisa cuando se agrega un endpoint. Las operaciones que el dominio considera una sola se resuelven en una llamada, aunque por dentro toquen a tres componentes. El modelo elegido está registrado con su fecha y su motivo, de modo que dentro de dos años se pueda saber si el motivo sigue vigente.

### Antipatrones

**La API que es el diagrama.** Cada caja del diagrama de arquitectura se convirtió en un segmento de URI. El consumidor aprende el organigrama de la empresa para poder llamar a un endpoint, y cualquier reorganización rompe.

**El recurso repartido.** Dos servicios son dueños de campos de la misma representación. Se reconoce porque agregar un campo requiere coordinar dos despliegues, exactamente lo que la partición prometía evitar.

**El HTTP en proceso.** Un componente llama por red a un endpoint de su propia aplicación, en el mismo proceso. Paga serialización y latencia por una frontera que no existe.

**La entidad publicada.** El tipo de persistencia se serializa directo. Es cómodo el primer mes y convierte cada migración de esquema en un cambio rompiente.

**El BFF que se volvió API general.** Empezó sirviendo a una aplicación y hoy tiene endpoints de tres. Ninguno se puede cambiar sin averiguar quién más lo usa, y nadie lo sabe.

**La partición sin contrato.** Se extrajo un servicio y su superficie se documentó después, o no se documentó. El resultado es una frontera que existe en el despliegue y no en el conocimiento del equipo.

**El pseudoservicio de lectura.** Un servicio que no es dueño de ningún dato y solo compone llamadas a otros. A veces es un BFF legítimo y hay que llamarlo así; cuando no lo es, agrega un salto, una falla posible y ningún límite.

---

## Anexo — Ficha de ubicación arquitectónica de una API

Se completa junto con la ficha de [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) al iniciar el trabajo sobre una API, y se revisa cuando se agrega o se retira un servicio.

```yaml
api: ""                          # nombre de la superficie, no del servicio
modelo_arquitectonico: monolitico | modular | microservicios | bff
es_bff: si | no
  cliente_unico: ""              # obligatorio si es_bff = si
  que_pasa_con_el_segundo: ""    # decisión escrita, no intención

recursos:
  - nombre: ""
    dueno: ""                    # el servicio dueño; debe ser exactamente uno
    campos_de_otro_dueno: []     # si tiene alguno, el corte pasa por el medio
    respaldado_por: ""           # tabla, agregado o sistema interno

topologia_visible_en_el_contrato:
  nombres_de_servicio_en_uri: si | no
  cabeceras_que_revelan_infraestructura: []

dependencias_salientes:
  - api: ""
    contexto: CTX-2 | CTX-4
    aislada_tras_puerto: si | no
    saltos_hasta_responder: 0

artefactos:
  especificacion_publicada: ""   # ruta o URL
  mapa_recurso_a_interno: ""
  adr_de_particion: ""           # si hubo decisión de partir o de no partir
```

Los tres campos que más información aportan son `campos_de_otro_dueno`, porque cualquier entrada no vacía indica un recurso repartido; `nombres_de_servicio_en_uri`, porque un `si` es deuda que se paga en la primera reorganización; y `aislada_tras_puerto`, porque un `no` en `CTX-4` significa que el proveedor es hoy irreemplazable.
