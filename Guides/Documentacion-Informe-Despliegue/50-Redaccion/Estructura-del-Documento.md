---
doc_id: TEM-ESTRUCTURA
doc_type: tema
title: Estructura del documento
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [TEM-CRITERIO, TEM-ERRORES, FAM-NAT, TEM-QUE-ES, TEM-ESTANDARES, TEM-AUDIENCIA, ANEXO-PLANTILLA, ANEXO-CHECK, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, MARCO-CONVENCIONES, ANEXO-REFERENCIAS]
---

# Estructura del documento — `TEM-ESTRUCTURA`

## Resumen ejecutivo

El pedido que origina esta guía nombra su objeto sin ambigüedad: «contar con un documento que describa la solución… para comprender mejor el enfoque general». Ese lector necesita un **modelo formal de documento** —un orden de secciones estable, sabido de antemano, que le diga dónde va a encontrar cada cosa— y ese modelo no existe como norma. Hay una norma que dice qué debe contener una descripción de arquitectura (`N-01` ISO/IEC/IEEE 42010:2022), hay plantillas excelentes que proponen un orden concreto (arc42, el SAD de RUP), pero ninguna está pensada para un informe único y transversal que cruza arquitectura, despliegue y requisitos para un solo lector que pide el enfoque general.

Este documento propone ese orden. Es una **síntesis de criterio propio**: toma los elementos que `N-01` exige, la secuencia de doce secciones que arc42 propone (`G-01`) y la organización por vistas del SAD (`G-04`), y las adapta a un informe que no es ninguno de esos tres. Cada sección del modelo se mapea hacia atrás a su respaldo, de modo que la propuesta es discutible pero no arbitraria: se puede señalar de dónde sale cada decisión. El resultado se estratifica por actor —resumen para el decisor `ACT-06`, cuerpo para el solicitante técnico `ACT-03`, anexos para el responsable de despliegue `ACT-04`— porque un mismo documento sirve a lectores que leen partes distintas.

El modelo es un punto de partida, no una camisa de fuerza. El escenario mueve el orden: `ESC-3` obliga a una sección de estado actual frente al objetivo que `ESC-2` no necesita, y `ESC-1` reemplaza los hechos por una sección de supuestos y pendientes. La plantilla llena de este modelo vive en [`ANEXO-PLANTILLA`](../99-Anexos/Plantilla-del-Informe.md); aquí se justifica su forma.

---

## Definición

### Qué es

Un **modelo formal de documento** es un contrato de estructura: la lista ordenada de secciones que el informe va a tener, qué contiene cada una y en qué orden aparecen. Su valor no está en ninguna sección en particular sino en que sea **estable y sabido**: un lector que ya leyó un informe con este modelo sabe, en el siguiente, que la vista de despliegue está después de la de arquitectura y antes del cruce de requisitos, y va directo a lo que le interesa sin recorrer el documento entero. La estructura predecible es lo que `Rule-Dual-Audience` pide para la cara agente —encabezados y anclas parseables— y lo que la cara humana agradece como mapa.

El modelo que este documento propone es **transversal**: una sola secuencia que integra las tres familias de contenido en un documento, no tres documentos yuxtapuestos. Esa integración es el aporte, porque las plantillas disponibles resuelven una vista a la vez.

### Qué problema resuelve

El de la página en blanco y el del índice heredado. Sin un modelo, el redactor arranca de cero cada informe o copia el índice del último proyecto sin preguntarse si sirve a este. El primer camino produce documentos que no se parecen entre sí y que el lector tiene que aprender a leer cada vez; el segundo produce el error que [`TEM-ERRORES`](Errores-Frecuentes.md) cataloga como copiar arc42 tal cual: doce secciones para un monolito que llena la mitad con relleno. Un modelo pensado y adaptable evita ambos.

### Qué NO es, y con qué se lo confunde

**No es la plantilla arc42.** arc42 (`G-01`) es una de las fuentes de este modelo, no el modelo. Sus doce secciones —*Introduction & Goals, Constraints, Context & Scope, Solution Strategy, Building Block View, Runtime View, Deployment View, Crosscutting Concepts, Architectural Decisions, Quality Requirements, Risks & Technical Debt, Glossary*— están pensadas para documentar la arquitectura de un sistema, no para un informe de enfoque general que además cruza despliegue y requisitos para un lector externo. Adoptarlas verbatim es tratar una plantilla como si fuera obligatoria, el error de autoridad que [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md#los-cuatro-niveles-de-autoridad) advierte.

**No es un SAD.** El SAD de RUP (`G-04`) documenta la arquitectura de forma exhaustiva por vistas de `O-01`. El informe de solución es una síntesis orientada a una decisión, no la biblioteca completa; [`TEM-QUE-ES`](../10-Naturaleza-del-Informe/Que-es-y-que-no-es.md) traza esa frontera. El modelo de este documento referencia al SAD (`DOC-SAD`) donde el detalle vive, en lugar de reproducirlo.

**No es un requisito normativo.** `N-01` fija *qué* debe contener una descripción de arquitectura, no *en qué orden* ni *bajo qué títulos*. El orden es criterio; lo normativo es que los elementos estén. Confundir ambas cosas lleva a defender un índice como si fuera obligatorio cuando solo es conveniente.

---

## El modelo propuesto

Diez secciones, agrupadas en tres estratos por actor. La columna «respalda» nombra de dónde sale cada sección: `N-01` cuando es un elemento que la norma exige, un marco cuando es una forma concreta que ese marco propone, o «criterio propio» cuando es decisión de esta guía. Es la trazabilidad que vuelve discutible —y no arbitraria— la propuesta.

| # | Sección del informe | Qué contiene | Qué marco la respalda | A qué actor sirve |
|---|---|---|---|---|
| **Resumen — para el decisor** ||||
| 1 | Resumen ejecutivo | Qué es la solución, qué decisión habilita el informe, los tres o cuatro hechos que sostienen esa decisión. Una página, sin jerga, se entiende sola | Criterio propio; `N-01` no lo exige, arc42 no lo tiene | `ACT-06` decisor |
| **Cuerpo — para el solicitante técnico** ||||
| 2 | Contexto y alcance | Qué problema resuelve el sistema, quién lo usa, qué queda dentro y qué fuera del informe | arc42 §1+§3; `N-01` *stakeholders* y *concerns* | `ACT-03` |
| 3 | Requisitos que gobiernan | Los funcionales clave y los no funcionales con métrica, que son el criterio contra el que se juzga la arquitectura | `N-06` 29148; `N-04` 25010; arc42 §10 | `ACT-03`, `ACT-05` |
| 4 | Estrategia de solución | Las decisiones estructurantes: estilo arquitectónico, tecnologías, cómo se atacan los requisitos difíciles | arc42 §4 | `ACT-03` |
| 5 | Vista de arquitectura | Componentes, responsabilidades y relaciones; qué hace cada pieza y con qué habla | arc42 §5; C4 *Container*/*Component* (`G-02`); `N-01` *views*; `O-01` vista lógica | `ACT-03` |
| 6 | Vista de despliegue | Dónde corre cada componente, sobre qué nodos, con qué protocolos; cómo se instala y actualiza | arc42 §7; `O-01` vista física; `N-07` UML *deployment* | `ACT-04` |
| 7 | Resolución de requisitos | El cruce explícito requisito ↔ mecanismo: cómo la solución satisface cada RNF, narrado de punta a punta | arc42 §6+§10; `N-01` *correspondences* | `ACT-03`, `ACT-08` |
| 8 | Decisiones y trade-offs | Las decisiones de arquitectura con sus alternativas y su costo, resumidas; el detalle en los ADR | arc42 §9; `N-01` *architecture decisions* + *rationale* | `ACT-03` |
| 9 | Riesgos y pendientes | Lo que puede salir mal, la deuda conocida, lo que falta validar | arc42 §11 | `ACT-06`, `ACT-03` |
| **Anexos — para el detalle** ||||
| 10 | Anexos | Glosario, trazabilidad completa requisito↔mecanismo, diagramas de detalle, procedimientos de instalación | arc42 §12; [`ANEXO-PLANTILLA`](../99-Anexos/Plantilla-del-Informe.md) | `ACT-04`, `ACT-08` |

Tres decisiones de este orden merecen justificación, porque no son las de arc42.

**Los requisitos van antes que la arquitectura (sección 3 antes que la 5).** arc42 pone los *Quality Requirements* en la sección 10, casi al final. Esta guía los adelanta porque el informe de solución existe para mostrar *cómo la arquitectura resuelve los requisitos*, y no se puede juzgar una solución contra un criterio que todavía no se enunció. El lector `ACT-03` necesita saber qué se le pedía al sistema antes de evaluar si la arquitectura lo logra. Es criterio propio y se declara como tal.

**La resolución de requisitos es una sección propia (la 7), no una fila de una tabla.** Es el corazón del informe —lo que `N-01` llama *correspondences* entre requisitos y elementos de arquitectura— y el lugar donde el sistema de audiencias se gana o se pierde: los tres comportamientos que lo definen (operar con el centro caído, recuperar ante caída del escritorio, subir en diferido) se narran aquí de punta a punta. Enterrar ese cruce en una matriz de trazabilidad lo vuelve ilegible; la matriz completa va al anexo, la narración va al cuerpo.

**Arquitectura y despliegue son secciones separadas (5 y 6).** Se confunden todo el tiempo, y son cosas distintas: la arquitectura dice *qué* componentes hay y de qué responden; el despliegue dice *dónde* corren. `O-01` las separa en vista lógica y vista física por la misma razón. Fundirlas produce el antipatrón de describir la topología creyendo que se describió la arquitectura, que [`TEM-ERRORES`](Errores-Frecuentes.md) desarrolla.

### Trazabilidad a `N-01`

El modelo no inventa qué debe contener el informe: reordena lo que `N-01` ya exige. La cláusula 6 de la norma enumera los elementos de una descripción de arquitectura, y cada uno tiene su lugar en el modelo.

| Elemento que `N-01` exige | Sección del modelo que lo aloja |
|---|---|
| Identificación y visión general de la descripción | 1 Resumen ejecutivo, 2 Contexto |
| *Stakeholders* y sus *concerns* | 2 Contexto y alcance |
| *Architecture views* y *viewpoints* | 5 Arquitectura, 6 Despliegue |
| *Correspondences* entre elementos | 7 Resolución de requisitos |
| *Architecture decisions* con su *rationale* | 8 Decisiones y trade-offs |

Lo que `N-01` **no** dice —el orden, los títulos, que exista un resumen ejecutivo, que los requisitos vayan antes que la arquitectura— es criterio propio de esta guía, y aparece declarado como tal. La norma fija el contenido obligatorio; la forma es defendible pero no impuesta.

---

## Aplicación por escenario

El modelo es el mismo, pero el escenario mueve el orden y agrega o quita secciones. Un informe que usa el índice fijo para los cuatro escenarios sirve bien a uno y mal a los otros tres.

### `ESC-1` — Solución en diseño

El sistema no existe, y casi todo lo que el informe afirma es intención, no hecho. El modelo cambia en dos puntos. Las secciones 5 a 7 se escriben en modo propuesto —«se prevé», «la topología planificada»— y no en presente de indicativo. Y aparece una sección nueva, **supuestos y pendientes**, que `ESC-2` no necesita: la lista explícita de lo que está decidido, lo que está propuesto y lo que falta confirmar. Sin esa sección, el lector no puede distinguir la certeza de la hipótesis, que es la trampa que [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md) señala para este escenario. La sección 9 de riesgos se vuelve central, no accesoria.

### `ESC-2` — Solución construida

Es el escenario para el que el modelo está calibrado por defecto. El orden se respeta tal cual, y cada sección describe lo que hay, no lo que se diseñó. La única precaución de estructura es reservar en la sección 5 o en la 9 un lugar para las **divergencias conocidas entre diseño y realidad** —el atajo que nadie documentó, la instancia única que el diseño preveía escalar—, porque ocultarlas produce el informe elegante e inútil que describe el sistema que no es.

### `ESC-3` — Solución en evolución o migración

El cambio de estructura es el mayor de los cuatro. El informe deja de describir un sistema y pasa a describir una transición, y eso exige una sección que los otros escenarios no tienen: **estado actual frente al objetivo**, atributo por atributo. La vista de despliegue de la sección 6 se vuelve doble —la topología actual y la topología objetivo, con el camino entre ambas— y la sección 8 de decisiones tiene que costear el viaje, no solo describir el destino. Un modelo de `ESC-3` que omite el estado de partida deja sin respuesta la única pregunta del lector: ¿conviene migrar?

### `ESC-4` — Evaluación de una solución ajena

Aquí el modelo se usa al revés: como rúbrica de lectura, no como molde de escritura. Cada sección se convierte en una pregunta sobre el informe ajeno —¿tiene resumen que se entienda solo?, ¿cruza requisitos con mecanismos o solo los enumera?, ¿la vista de despliegue existe o falta?— y la habilidad es **detectar la sección ausente**. Cuando no hay informe previo, el modelo da el orden en que se levanta uno as-built desde el sistema, con el nivel de confianza declarado sección por sección.

### Qué cambia según el contexto

| Contexto | Ajuste de estructura | Por qué |
|---|---|---|
| `CTX-1` monolito | Sección 6 breve; foco en 5 y 7 | El despliegue es un host y una base; forzarlo a llenar páginas produce relleno |
| `CTX-2` cliente-servidor | Sección 6 gana los contratos entre nodos | Aparece qué protocolo une qué, dónde termina TLS |
| `CTX-3` borde distribuido | Sección 6 se desdobla: topología **y** procedimiento de instalación por terminal; sección 7 domina el informe | El despliegue es un proceso repetido; la operación degradada es el corazón del sistema |
| `CTX-4` multiservicio | Sección 5 necesita niveles de zoom (C4); jerarquía obligatoria | No cabe todo en un diagrama; el riesgo es aplanar y no distinguir lo central |

El sistema de audiencias es `CTX-3`, y por eso su informe dedica a las secciones 6 y 7 la mayor parte del espacio: la instalación por sala y los tres comportamientos de resiliencia son lo que el lector técnico más quiere entender. Un informe de audiencias que resuelve el despliegue en tres párrafos está mal calibrado, y la calibración es una decisión de estructura antes que de redacción.

---

## Frases de referencia

Pares de «así no / así sí» sobre la estructura del documento. Los fragmentos son **sintéticos** y pertenecen al informe del sistema de audiencias. Contrastan una decisión de estructura que falla con la que la corrige.

### El índice heredado frente al índice adaptado

**Así no** — índice copiado de arc42 para un monolito, con secciones que se llenan de relleno:

```
6. Vista de despliegue
   6.1 Diagrama de infraestructura
   6.2 Motivación
   6.3 Mapeo de building blocks a infraestructura
   6.4 Requisitos de calidad relacionados con el despliegue
```

**Así sí** — la misma sección, adaptada a `CTX-3`, donde el despliegue sí da para desarrollar:

```
6. Vista de despliegue
   6.1 Topología: qué corre en cada terminal y en el centro
   6.2 Instalación y actualización por terminal (MSIX / servicio de Windows)
   6.3 Comportamiento del sistema con el enlace al centro caído
```

La estructura sigue al contenido del contexto, no a la inversa. En un monolito, esas mismas cuatro subsecciones de arc42 serían media página inflada; en el borde distribuido, tres subsecciones distintas no alcanzan.

### El resumen que remite frente al resumen que se sostiene

**Así no** — un resumen ejecutivo que obliga a leer el cuerpo:

> «El sistema implementa una arquitectura resiliente según se detalla en la sección 5. La estrategia de despliegue se describe en la sección 6. Los requisitos no funcionales se abordan en la sección 7.»

**Así sí** — un resumen que un decisor entiende sin pasar de página:

> «La solución permite grabar audiencias en cada sala aunque el centro de datos esté caído: la grabación es local y se sincroniza después. Se pide aprobar el despliegue en las doce salas de la primera sede, con un costo de instalación de una jornada por sala.»

El resumen sirve a `ACT-06`, que no lee el resto. Un resumen que es un índice comentado no habilita ninguna decisión.

### Los requisitos al final frente a los requisitos como criterio

**Así no** — la arquitectura primero, los requisitos como anexo:

```
4. Arquitectura de la solución
5. Vista de despliegue
6. Componentes
...
9. Anexo B: requisitos no funcionales
```

**Así sí** — los requisitos antes de la arquitectura que los atiende:

```
3. Requisitos que gobiernan la solución
4. Estrategia de solución
5. Vista de arquitectura
7. Cómo la solución resuelve los requisitos
```

Un informe que muestra la arquitectura antes de enunciar contra qué se la juzga le pide al lector que evalúe una respuesta sin conocer la pregunta.

### La arquitectura y el despliegue fundidos frente a separados

**Así no** — una sola sección que mezcla qué componentes hay con dónde corren:

> «La arquitectura consta de un servicio backend en el servidor central, un frontend Blazor también en el centro, y un programa de escritorio instalado en cada terminal que se comunica por HTTP local con un Worker Service.»

**Así sí** — la arquitectura dice de qué responde cada pieza; el despliegue dice dónde vive:

> Sección 5: «El Worker Service es responsable de capturar de las cámaras, grabar localmente y subir los videos; el programa de escritorio orquesta la audiencia y consulta ese estado.»
>
> Sección 6: «El Worker corre como servicio de Windows en cada terminal; el backend, en el servidor central junto con PostgreSQL.»

Fundir ambas cosas hace que el lector no sepa si le están contando responsabilidades o infraestructura, y suele terminar sin entender ninguna de las dos.

---

## Preguntas guía

- ¿Mi índice sale de pensar qué necesita este lector, o de copiar el del último proyecto?
- ¿Cada sección de mi modelo puedo trazarla a `N-01`, a un marco nombrado o a criterio propio declarado? ¿O hay secciones que están «porque sí»?
- ¿Los requisitos aparecen antes de la arquitectura que los resuelve?
- ¿La arquitectura y el despliegue son secciones distintas, o las fundí?
- Si estoy en `ESC-3`, ¿tengo una sección de estado actual frente al objetivo? Si estoy en `ESC-1`, ¿una de supuestos y pendientes?
- ¿El peso de cada sección corresponde al contexto, o estoy rellenando la vista de despliegue de un monolito y subestimando la de un sistema en el borde?
- ¿El resumen ejecutivo se entiende sin leer el resto, o remite a las demás secciones?

---

## Criterios de calidad

### Estructura buena

El lector encuentra cada cosa donde el modelo dice que está, y las secciones tienen el peso que el contexto justifica: en el informe de audiencias, la vista de despliegue y la resolución de requisitos ocupan más que el resto, porque ahí está lo interesante. Cada sección se puede trazar a su respaldo, y las decisiones de orden que se apartan de arc42 —requisitos antes que arquitectura, resolución de requisitos como sección propia— están declaradas como criterio, no presentadas como obligatorias. El escenario se refleja en la estructura: un informe de `ESC-3` tiene su sección de estado actual, uno de `ESC-1` su lista de pendientes.

### Estructura pobre y antipatrones

**Índice heredado.** El índice del último proyecto aplicado a este sin preguntarse si sirve. Se detecta por las secciones vacías o rellenas: una vista de despliegue de dos páginas para un monolito, una sección de riesgos con una viñeta genérica.

**Plantilla citada como norma.** Doce secciones de arc42 defendidas como si fueran obligatorias. arc42 es un marco (`G-01`), no una norma; su autoridad es la de sus autores, y adaptarlo es legítimo. [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md#los-cuatro-niveles-de-autoridad) llama a esto el error de citación más frecuente del tema.

**Todo con el mismo peso.** Cada sección con la misma extensión, sin que la estructura refleje dónde está lo central. Es el riesgo de `CTX-4` que [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md) nombra: un informe plano donde no se distingue lo importante de lo accesorio.

**Resolución de requisitos enterrada.** El cruce requisito↔mecanismo reducido a una matriz en el anexo, sin narración en el cuerpo. La matriz es para `ACT-08`; la narración es para `ACT-03`, y sin ella el informe no explica cómo la solución logra lo que promete.

**Sin estratificar.** Un informe de nivel técnico uniforme que obliga al decisor a leer treinta páginas para saber si aprobar, o uno diluido hasta que el solicitante técnico no encuentra qué evaluar. La estratificación resumen/cuerpo/anexos existe para servir a los tres actores a la vez.

---

## Anexo — Lista de verificación de estructura

Se recorre antes de dar por cerrado el índice del informe. La plantilla llena de este modelo está en [`ANEXO-PLANTILLA`](../99-Anexos/Plantilla-del-Informe.md); la verificación integral, en [`ANEXO-CHECK`](../99-Anexos/Lista-de-Verificacion.md).

```yaml
modelo_de_documento:
  escenario: ESC-?
  contexto_dominante: CTX-?
  estratos_presentes:
    resumen_para_decisor: si | no          # sección 1, se entiende sola
    cuerpo_para_solicitante: si | no        # secciones 2-9
    anexos_para_detalle: si | no            # sección 10
  secciones:
    - id: 1
      nombre: "Resumen ejecutivo"
      presente: si | no
      peso: bajo | medio | alto
      respaldo: criterio_propio
    # ... una entrada por sección del modelo
  ajustes_por_escenario:
    esc1_supuestos_y_pendientes: si | no | na
    esc3_estado_actual_vs_objetivo: si | no | na
    esc2_divergencias_declaradas: si | no | na
  calibracion_por_contexto:
    peso_vista_despliegue_corresponde_al_contexto: si | no
    peso_resolucion_requisitos_corresponde_al_contexto: si | no
  trazabilidad:
    cada_seccion_mapeada_a_respaldo: si | no
    decisiones_propias_declaradas_como_tales: si | no
```

El campo `respaldo` es el que vuelve auditable el índice. Una sección que no se puede trazar ni a `N-01`, ni a un marco, ni a criterio propio declarado, o sobra o está mal ubicada; el ejercicio de llenarlo obliga a preguntarse por qué está cada sección, que es exactamente la pregunta que separa un modelo pensado de un índice heredado.
