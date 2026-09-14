---
doc_id: TEM-DECISIONES
doc_type: tema
title: Decisiones de arquitectura
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [FAM-ARQ, TEM-COMPONENTES, TEM-VISTAS, TEM-RNF, TEM-OPERACION, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-CONVENCIONES, ANEXO-REFERENCIAS]
---

# Decisiones de arquitectura — `TEM-DECISIONES`

## Resumen ejecutivo

Una arquitectura es el resultado de un puñado de decisiones que podrían haber sido otras. Grabar en el borde y no en el centro, guardar los videos en un servidor de archivos y no en la base, operar aunque el centro esté caído: cada una descartó una alternativa razonable y cargó con un costo a cambio de una propiedad. Un informe que muestra el resultado —los componentes, las vistas— y calla esas decisiones obliga al lector a aceptar la arquitectura como un hecho de la naturaleza. Un informe que las declara, con su alternativa y su costo, convierte el sistema en algo **explicable**: `ACT-03` puede estar en desacuerdo con conocimiento de causa, que es exactamente para lo que pidió el informe.

La distinción que gobierna esta sección la tomó prestada la guía de su documento sobre REST y vale igual aquí: **una restricción relajada con su razón registrada es una decisión de arquitectura; sin razón registrada es una omisión.** El campo que separa una cosa de la otra no es la decisión —siempre hubo una— sino el registro. La mayoría de las malas secciones de arquitectura no tomaron malas decisiones; tomaron buenas y no las contaron, con lo que el lector no puede distinguir lo pensado de lo accidental.

Este documento enseña a **narrar decisiones dentro del informe** —un ADR embebido, compacto— y a saber cuándo eso alcanza y cuándo hay que remitir al catálogo completo de [`DOC-ADR`](../../Documentacion-Tecnica/30-Arquitectura/ADR.md) de la guía hermana. Se apoya en `N-01`, que nombra la decisión de arquitectura y su *rationale* como cosas que una descripción debe registrar.

---

## Definición

### Qué es una decisión de arquitectura en el informe

Una **decisión de arquitectura** es una elección entre alternativas que afecta la estructura, las propiedades o la evolución del sistema, y que cuesta caro revertir. No toda elección lo es: el nombre de una variable no es una decisión de arquitectura; grabar en el borde en lugar del centro, sí, porque define la topología, el despliegue y la resiliencia del sistema entero.

`N-01` (ISO/IEC/IEEE 42010:2022) le da estatus normativo. Su cláusula 6 enumera, entre lo que una descripción de arquitectura debe registrar, **las decisiones de arquitectura con su *rationale*** —el fundamento que las sostiene—. La norma trata la decisión y su razón como un par inseparable: registrar la decisión sin la razón incumple tanto como no registrarla. Esa exigencia es la que este documento traduce a la escala de un informe.

### ADR embebido y ADR externo

La guía hermana trata el **ADR** —Architecture Decision Record— como documento propio: una decisión por ficha, con contexto, opciones, decisión y consecuencias, versionada junto al código. Un sistema maduro tiene decenas. Un informe transversal no puede ni debe reproducirlas todas; su lector quiere el enfoque general, no el archivo histórico.

La regla de esta guía —**criterio propio, declarado como tal**— es la del recorte por relevancia: el informe **embebe las tres a cinco decisiones que dan forma a la solución entera** —las que, cambiadas, harían otro sistema— en un formato compacto, y **referencia el catálogo completo** en [`DOC-ADR`](../../Documentacion-Tecnica/30-Arquitectura/ADR.md) para el resto. Un informe que embebe treinta decisiones se convirtió en el catálogo que debía referenciar; uno que no embebe ninguna dejó al lector con una arquitectura sin explicar y un enlace a un documento que quizá no tenga a mano.

### El formato compacto

Un ADR embebido cabe en cuatro campos. Más que eso es un ADR externo mal ubicado; menos, es una afirmación sin fundamento.

- **Contexto** — la fuerza que empuja la decisión, casi siempre un requisito no funcional. Sin contexto, la decisión parece arbitraria.
- **Decisión** — qué se eligió, en una frase.
- **Alternativas** — qué otra cosa razonable se descartó. Es el campo que más se omite y el que más informa: una decisión sin alternativa declarada parece la única opción posible, y casi nunca lo es.
- **Consecuencias** — qué se ganó y qué se pagó. El costo declarado es lo que vuelve creíble el beneficio; una decisión que solo lista ventajas es propaganda.

### El trade-off, no la preferencia

Lo que distingue una decisión de arquitectura de una preferencia es que la decisión **cede algo**. Elegir grabar en el borde no es gratis: se paga con el despliegue por terminal. Si una elección no cuesta nada —si la alternativa es peor en todo—, no era una decisión difícil y probablemente no merece espacio en el informe; lo que merece espacio es la elección donde ambas opciones tenían algo a favor y se optó por una sabiendo lo que se resignaba. Ese punto donde dos propiedades deseables no pueden tenerse a la vez es el *trade-off*, y narrarlo es la parte más valiosa de la sección.

La consecuencia práctica para la redacción: una decisión cuyo relato no menciona qué se resignó está incompleta, no porque falte prosa sino porque falta el análisis. El lector técnico lee esta sección justamente para encontrar los trade-offs —son donde una arquitectura se juega—, y un informe que los suaviza hasta que todo parece haber sido la opción obvia le está ocultando lo único que vino a buscar.

### Qué problema resuelve

**Convierte el sistema en explicable.** El lector que ve una decisión con su alternativa y su costo puede reconstruir el razonamiento y disentir sobre bases concretas. El que ve solo el resultado tiene que confiar o rechazar en bloque.

**Preserva el porqué antes de que se olvide.** `MARCO-ESCENARIOS` lo anota para `ESC-1`: las decisiones, una vez tomadas, «se olvidan y quedan pareciendo inevitables». Registrarlas en el momento es la diferencia entre un sistema que sabe por qué es como es y uno donde nadie recuerda por qué no se hizo de la otra forma —y por lo tanto nadie puede evaluar si conviene cambiarlo.

### Qué NO es

**No es una lista de tecnologías.** «Usamos .NET 10, PostgreSQL y Blazor» no es una decisión de arquitectura; es un inventario. La decisión sería «persistimos en PostgreSQL y no en la base documental que el equipo conocía, porque el modelo es relacional y las consultas de auditoría lo exigen», con su alternativa y su costo.

**No es el catálogo completo de ADR.** Ese vive en [`DOC-ADR`](../../Documentacion-Tecnica/30-Arquitectura/ADR.md). El informe embebe las decisiones estructurales y remite al catálogo; reproducirlo entero es duplicar, que `MARCO-CONVENCIONES` prohíbe.

**No es una justificación a posteriori.** Fabricar razones nobles para decisiones que se tomaron por inercia o por lo que el framework traía por defecto es peor que no declararlas: induce a creer que hubo un análisis que no existió. Si una decisión se heredó sin evaluar, lo honesto en `ESC-2` es decir que se heredó.

---

## Aplicación por escenario

### `ESC-1` — Solución en diseño

Las decisiones están **vivas**: se están tomando mientras se escribe, y el informe es el lugar donde quedan registradas por primera vez. Es el escenario donde esta sección más rinde, porque capturar la alternativa y el costo *en el momento* de decidir es gratis, y reconstruirlos después es caro o imposible. `MARCO-ESCENARIOS` lo marca como el momento de registrar las decisiones con sus alternativas, «porque después se olvidan».

El riesgo es presentar la decisión como cerrada cuando todavía es propuesta. Una decisión de `ESC-1` honesta declara su estado —firme, tentativa, pendiente de una validación— igual que lo hace un componente propuesto en [`TEM-COMPONENTES`](Vista-de-Componentes.md).

### `ESC-2` — Solución construida

Las decisiones se **reconstruyen**, porque casi nunca se registraron cuando se tomaron. El trabajo es arqueológico: `ACT-01` y `ACT-02` recuperan por qué el sistema quedó así, y buena parte de esas razones vive solo en la memoria de quien las tomó. Aquí aparece el caso incómodo: el atajo no diseñado de [`TEM-COMPONENTES`](Vista-de-Componentes.md) —el programa de escritorio leyendo directo de PostgreSQL— es también una decisión, tomada bajo presión y nunca registrada. En `ESC-2` esa decisión se documenta *como lo que fue*: una solución de compromiso ante un problema de carga, con su costo de acoplamiento, no una elección de diseño elegante.

### `ESC-3` — Solución en evolución o migración

**La migración es la decisión.** Todo el informe de `ESC-3` gira alrededor de una decisión mayor —reemplazar FTP por tus, mudar el despliegue, rehacer un componente— y su trabajo es justificarla contra el estado actual. Aquí el formato compacto se expande: el contexto es el atributo de calidad que la solución actual no alcanza, la alternativa incluye *no migrar*, y las consecuencias incluyen el costo y el riesgo del viaje, no solo las virtudes del destino. `MARCO-ESCENARIOS` advierte contra «vender el destino sin costear el viaje»; el campo de consecuencias es donde se costea.

### `ESC-4` — Evaluación de una solución ajena

Se **juzgan** las decisiones ajenas, y la rúbrica es doble: si la decisión resuelve lo que dice, y si el informe permite confiar en esa afirmación. El evaluador busca las alternativas que no se declararon —una decisión presentada como única opción casi siempre esconde una que se descartó sin decirlo— y los costos que no se reconocieron. Una decisión con su alternativa y su costo sobre la mesa es evaluable; una afirmada como inevitable exige reconstruir el razonamiento desde afuera, y ahí `N-03` (el marco de evaluación de arquitectura) da el respaldo normativo a la tarea.

### Qué cambia según el contexto

| Contexto | Decisiones que dominan el informe | Qué se juega |
|---|---|---|
| `CTX-1` Monolito | Pocas y sobre todo internas | Estilo de capas, elección de persistencia |
| `CTX-2` Cliente-servidor | El contrato y dónde termina el TLS | Acoplamiento entre nodos, seguridad del canal |
| `CTX-3` Borde distribuido | Las de resiliencia y distribución | Operación degradada, recuperación, subida diferida |
| `CTX-4` Multiservicio | La descomposición en servicios | Consistencia, resiliencia entre servicios |

En `CTX-3`, el del sistema de audiencias, las decisiones que definen el sistema son las de resiliencia, y son las que el lector técnico más quiere ver justificadas —son la parte interesante de la arquitectura, como anota `MARCO-CONTEXTOS`—. En `CTX-4`, `MARCO-CONTEXTOS` recomienda justificar la descomposición con una decisión registrada «porque es la que más consecuencias tiene»: un informe de multiservicio que no explica por qué se partió el sistema así deja sin fundamento su decisión más cara.

---

## Ejemplos concretos

Todos los ejemplos son **sintéticos** y pertenecen al sistema de gestión de audiencias (`CTX-3`) de [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md#el-sistema-de-ejemplo--gestión-de-audiencias). Muestran el formato compacto sobre las tres decisiones que dan forma a la solución.

### Decisión — grabar en el borde, no en el centro

> **Contexto.** La audiencia debe poder **iniciarse y grabarse aunque el backend y el frontend estén caídos** (`RNF`, ver [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md)). La conexión con el centro no está garantizada durante la sesión.
>
> **Decisión.** La captura de cámaras y la grabación las realiza un **servicio en segundo plano instalado en cada terminal**, que escribe el video localmente y lo sube después.
>
> **Alternativas.** Enviar la señal de video al backend y grabar de forma centralizada. Descartada: ataría la grabación a la disponibilidad del enlace y del centro, que es justo lo que el requisito prohíbe.
>
> **Consecuencias.** El sistema graba con el centro caído. A cambio, hay software que instalar y actualizar en cada terminal —el despliegue deja de ser un acto y pasa a ser un proceso, ver [`TEM-OPERACION`](../30-Despliegue/Operacion-y-Resiliencia.md)— y almacenamiento local que gestionar.

El costo declarado —instalación y actualización por terminal— es lo que conecta esta decisión con el peso alto de la vista de despliegue en `CTX-3`. Una decisión que solo dijera «graba con el centro caído» escondería que esa propiedad se paga en el despliegue.

### Decisión — videos en el servidor de archivos, metadatos en PostgreSQL

> **Contexto.** Los videos de audiencia son archivos binarios grandes; el estado y los metadatos de la audiencia son datos estructurados que el frontend consulta y la auditoría recorre.
>
> **Decisión.** Los videos se almacenan en un **servidor de archivos** (FTP, `N-08`, o tus, `F-01`); PostgreSQL guarda **solo el estado y los metadatos**.
>
> **Alternativas.** Guardar el video como binario dentro de PostgreSQL. Descartada: inflaría la base, degradaría las consultas y complicaría el respaldo, sin ganar nada que el servidor de archivos no dé.
>
> **Consecuencias.** La base se mantiene chica y rápida y el video puede subirse con un protocolo reanudable. A cambio, hay **dos almacenes que mantener consistentes**: la integridad entre el metadato y su archivo es responsabilidad de la aplicación, no del motor, y un video sin su fila —o al revés— es un estado que el sistema tiene que poder detectar.

### Decisión — operación offline-first

> **Contexto.** Los componentes del borde deben seguir operando cuando la conexión con el centro no está, y **recuperar el estado de la audiencia ante la caída del programa de escritorio**.
>
> **Decisión.** El borde opera de forma **autónoma** y sincroniza con el centro de forma **eventual**: el estado de la audiencia se persiste localmente y se reconcilia con el backend cuando hay enlace.
>
> **Alternativas.** Exigir conectividad y bloquear la operación cuando el centro no responde. Descartada: convierte una caída del centro en una caída del servicio en cada sala, que es inaceptable para el dominio.
>
> **Consecuencias.** La sala funciona con el centro caído y sobrevive a un reinicio del escritorio. A cambio, se paga **consistencia eventual**: hay lógica de reconciliación, ventanas en las que el centro y el borde discrepan, y un estado local que recuperar tras un fallo. Es la decisión que más comportamiento agrega y la que [`TEM-OPERACION`](../30-Despliegue/Operacion-y-Resiliencia.md) desarrolla.

### Una decisión de migración — el formato compacto expandido

En `ESC-3` la decisión se agranda, porque hay que costear el viaje. La misma plantilla sirve, pero el contexto es un atributo que la solución actual no alcanza y las consecuencias incluyen el costo de migrar.

> **Contexto.** El servidor de archivos actual es FTP (`N-08`). Cuando el enlace de una terminal se corta a mitad de una subida de video, la transferencia se reinicia desde cero; con archivos grandes, una sala con enlace inestable acumula subidas fallidas. El atributo que falla es la **fiabilidad** de la subida bajo enlace intermitente.
>
> **Decisión.** Reemplazar FTP por un servidor con protocolo de subida reanudable tus (`F-01`), que permite retomar desde el último byte confirmado con `HEAD` y `PATCH`.
>
> **Alternativas.** (a) *No migrar* y mitigar con reintentos completos —descartada: no resuelve el reinicio desde cero—. (b) Un protocolo de subida propio sobre HTTP —descartada: reinventa `F-01` sin su ecosistema—.
>
> **Consecuencias.** Las subidas sobreviven al corte de enlace. A cambio: hay que **desplegar el nuevo servidor y convivir** con el FTP hasta migrar todas las terminales, el servicio en segundo plano de cada terminal debe actualizarse para hablar el nuevo protocolo, y durante la transición coexisten dos mecanismos de subida —el costo y el riesgo del viaje, no solo la virtud del destino.

La diferencia con las tres decisiones anteriores es el peso del campo de consecuencias: en una decisión de diseño el costo es una propiedad permanente; en una de migración, buena parte del costo es transitorio —la convivencia— y omitirlo es la trampa que `MARCO-ESCENARIOS` asigna a `ESC-3`.

### Cómo se cierra la sección en el informe

Después de las tres decisiones embebidas, el informe remite al resto sin reproducirlo:

> Las decisiones anteriores dan forma a la solución. El resto —elección de ORM, política de reintentos de subida, formato de los identificadores— está registrado en el catálogo de decisiones [`DOC-ADR`](../../Documentacion-Tecnica/30-Arquitectura/ADR.md), que este informe no reproduce.

Esa frase es la que evita que la sección se convierta en el catálogo: declara que hay más, dice dónde está, y no lo copia.

---

## Preguntas guía

- ¿Cuáles son las tres a cinco decisiones que, cambiadas, harían otro sistema? ¿Están todas en el informe?
- Para cada una, ¿declaré la alternativa que descarté, o la presenté como la única opción posible?
- ¿Declaré el costo, o solo las ventajas? Una decisión sin costo declarado no es creíble.
- ¿El contexto de cada decisión es un requisito real, o la inventé para que la arquitectura calzara?
- En `ESC-2`, ¿estoy reconstruyendo la razón real o fabricando una noble a posteriori?
- ¿Embebí las decisiones estructurales y referencié el resto, o copié el catálogo entero —o no puse ninguna?

---

## Criterios de calidad

### Sección buena

Las decisiones estructurales están, cada una con contexto, alternativa y costo. El contexto es un requisito verificable, no una racionalización. Las alternativas descartadas son razonables —descartar un hombre de paja no informa a nadie—. El costo está declarado con la misma franqueza que el beneficio. El informe embebe lo que da forma a la solución y remite al [`DOC-ADR`](../../Documentacion-Tecnica/30-Arquitectura/ADR.md) para el resto, sin duplicar ni omitir.

La prueba es que un lector técnico pueda leer una decisión y decir «no lo habría hecho así, y por estas razones» —o «tiene sentido»— sin necesitar más información. Si para disentir tiene que adivinar qué alternativas se consideraron, la decisión está mal contada.

### Sección pobre y antipatrones

**La decisión sin alternativa.** «Elegimos grabar en el borde» sin decir contra qué. Presenta una elección como un hecho inevitable y le quita al lector la posibilidad de evaluarla. Es el antipatrón más común y el más fácil de corregir: agregar la línea de la alternativa descartada.

**El costo escondido.** Enumerar solo ventajas. Toda decisión de arquitectura tiene un costo —si no lo tuviera no habría sido una decisión—; ocultarlo convierte la sección en material de venta y `MARCO-ESCENARIOS` lo señala como la trampa de `ESC-3`.

**La racionalización a posteriori.** Inventar un análisis que no ocurrió para una decisión que se tomó por defecto. Es más dañino que declarar «se heredó del framework», porque induce a confiar en un rigor inexistente.

**El catálogo embebido.** Volcar los treinta ADR del sistema en el informe. Deja de ser un informe y pasa a ser el archivo; el lector que quería el enfoque general se ahoga.

**El inventario de tecnologías disfrazado.** Listar el stack y llamarlo decisiones de arquitectura. La tecnología es la parte menos interesante; la decisión es por qué esa y no la alternativa.

**La omisión de la decisión incómoda.** No documentar el atajo de `ESC-2` porque no es elegante. Ocultarlo produce el informe que `MARCO-ESCENARIOS` describe como «elegante e inútil»: describe un sistema que no es el que corre.

---

## Anexo — Plantilla comentada de decisión embebida

Se completa por cada decisión estructural que entra al informe. Las que no son estructurales van al catálogo [`DOC-ADR`](../../Documentacion-Tecnica/30-Arquitectura/ADR.md), no aquí.

```yaml
decisiones_embebidas:
  - id: ""                         # referencia el ADR del catálogo si existe
    titulo: ""                     # p. ej. "grabar en el borde, no en el centro"
    contexto: ""                   # la fuerza que empuja; casi siempre un RNF
    requisito_que_la_motiva: ""    # ID del RNF; ver TEM-RNF. Si no hay, sospechar
    decision: ""                   # qué se eligió, una frase
    alternativas:
      - opcion: ""                 # la alternativa razonable descartada
        por_que_no: ""             # el motivo del descarte
    consecuencias:
      se_gana: ""                  # la propiedad obtenida
      se_paga: ""                  # el costo; si está vacío, la decisión no está entendida
    estado: firme | tentativa | heredada | pendiente_validar   # relevante en ESC-1
    modo: decidida | reconstruida  # ESC-1 decide; ESC-2 reconstruye
remision_al_catalogo:
  hay_mas_decisiones: si | no
  documento: DOC-ADR
  # el informe declara que hay más, dice dónde, y no las copia
```

Dos campos hacen el trabajo. `se_paga` vacío es la señal de que la decisión no se entendió: toda decisión de arquitectura cuesta algo, y quien no puede nombrar el costo no terminó de analizarla. `requisito_que_la_motiva` vacío es la señal de que puede ser un capricho documentado: una decisión de arquitectura que no responde a ningún requisito suele ser sobrediseño, el riesgo que `MARCO-ESCENARIOS` asigna a `ESC-1`.
