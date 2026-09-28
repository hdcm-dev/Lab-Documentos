---
doc_id: ANEXO-REVISION
doc_type: anexo
title: Revisión de consistencia
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Documentación técnica
last_review: 2026-07-18
audience: [humano, agente]
traces: [GUIA-INDICE, MAPA-CONCEPTUAL, MARCO-CONVENCIONES]
---

# Revisión de consistencia

## Resumen ejecutivo

Registro de la revisión de la guía completa contra el mapa conceptual: qué se verificó, qué defectos aparecieron, cuáles se corrigieron y cuáles quedan abiertos con su razón. Existe para que la próxima persona que retome el trabajo no repita el diagnóstico, y para que las decisiones que se tomaron por criterio y no por norma queden visibles.

La guía se produjo con diez líneas de trabajo en paralelo, una por familia o serie, todas contra el mismo marco de referencia. Ese método rinde en velocidad y en profundidad por tema, y paga en dos monedas: identificadores divergentes en las fronteras entre familias, y decisiones que cada línea da por resueltas en otra. Los defectos encontrados son casi todos de esa naturaleza.

---

## Qué se verificó

| Comprobación | Método | Resultado |
|--------------|--------|-----------|
| Frontmatter presente en todo documento | Inspección de la primera línea de los 57 archivos | Sin faltantes |
| Las cuatro entradas `ESC-1`..`ESC-4` en cada documento temático | Recuento de identificadores por archivo | Sin faltantes; ningún documento omite un escenario que no aplica, lo declara |
| Sección de anexo con plantilla en cada documento temático | Búsqueda de encabezado | Sin faltantes |
| Diagramas Mermaid | Búsqueda de bloques por archivo | Cuatro documentos sin diagrama; corregido |
| Enlaces relativos entre documentos | Resolución de cada destino contra el sistema de archivos | Dos rotos; corregidos |
| Identificadores `FAM-`, `DOC-`, `MET-`, `ARQ-` | Contraste de todos los usos contra la lista canónica | Once variantes divergentes; unificadas |
| Cobertura de la tabla del catálogo | Cruce fila por fila contra los documentos producidos | 28 de 28 |
| Artefactos de agrupación práctica sin fila propia | Verificación de que existen como sección y no como documento | 9 de 9 |

---

## Defectos corregidos

### Identificadores divergentes en las fronteras entre familias

Cada línea de trabajo dedujo los identificadores de las familias vecinas antes de que existieran, y once usos quedaron fuera de la nomenclatura canónica. La familia de desarrollo aparecía como `FAM-DES`, `FAM-OPS`, `FAM-QA` y `FAM-PRUEBAS` según quién la nombrara; la serie de métodos ágiles, como `FAM-METODOS`; el Test Plan, como `DOC-TEST-PLAN` y `DOC-TESTPLAN` en documentos distintos.

Se unificó contra la lista de [Convenciones](../00-Marco-de-Referencia/Convenciones.md), con estas equivalencias: `FAM-DES`, `FAM-QA` y `FAM-PRUEBAS` a `FAM-DEV`; `FAM-OPS` y `FAM-OPERATIVA` a `FAM-OPE`; `FAM-SEC` a `FAM-ARQ`; `FAM-METODOS` a `MET-INDICE`; `DOC-TEST-PLAN` a `DOC-TESTPLAN`; `DOC-TEST-CASES` a `DOC-TESTCASES`; `DOC-MODELO-DATOS` a `DOC-DATOS`; `DOC-DEV-GUIDE` a `DOC-DEVGUIDE`.

El defecto ilustra el argumento que la propia guía sostiene en `CTX-3`: los identificadores estables no son burocracia, son la condición para que un cuerpo documental escrito por varias manos se lea como uno solo.

### Enlaces rotos

El documento de UX enlazaba a `20-Analisis/Casos-de-Uso.md`, que por diseño no existe: los casos de uso son una sección interna del SRS, no un documento propio. Se redirigió el enlace al ancla correspondiente dentro del SRS. El mapa conceptual enlazaba al README de la guía antes de que se escribiera; el README existe.

### Estados de pantalla: cuatro contra seis

[Contextos](../00-Marco-de-Referencia/Contextos.md) fijaba cuatro estados mínimos por pantalla y el [documento de UX](../95-Transversales/UX-UI-y-Flujo-de-Usuario.md) exigía seis, sin que ninguno de los dos declarara la relación. Ocho documentos citaban una cifra u otra según de cuál hubieran heredado.

Se resolvió por jerarquía en lugar de por unificación: cuatro es el piso exigible a cualquier documento de la guía, y seis es el mínimo cuando la pieza especificada es la interfaz misma, con dos estados adicionales propios de Blazor *interactive server*. El marco ahora declara esa extensión y remite al documento de UX, que ya la registraba como criterio propio.

### Glosario canónico frente a glosarios de plantilla

Seis documentos contienen una sección de glosario, lo que a primera vista contradice la regla de fuente única. La inspección mostró que se trata de dos cosas distintas: el glosario de la guía, que define los términos del oficio, y los glosarios que viven dentro de las plantillas y los ejemplos, cuyo alcance es el vocabulario del sistema documentado. El [Glosario](Glosario.md) ahora explicita la distinción y sugiere el modelo de dominio como sede del glosario de dominio de un proyecto real.

### Documentos sin diagrama

Cuatro documentos temáticos no tenían ninguno: Release Notes, Change Log, API Specification y Operations Guide. Se agregó a cada uno un diagrama con contenido propio, coherente con lo que el texto ya afirmaba.

---

## Decisiones de criterio propio

Las siguientes no derivan de ninguna norma y se declaran acá para que puedan discutirse.

**El catálogo tiene 28 filas, no 27.** El enunciado del encargo hablaba de veintisiete tipos, de Vision Document a RFC. El recuento de la tabla del catálogo da veintiocho. Se produjo un documento por fila.

**Test Plan, Test Cases, Release Notes y Change Log se ubican en la familia de desarrollo.** El catálogo no los asigna a ninguna de sus siete agrupaciones prácticas. Se los ubicó donde su dueño natural trabaja, y la decisión queda declarada en el índice de esa familia. Un equipo con función de calidad separada podría preferir una familia de pruebas propia.

**Postmortem y Administrator Guide se ubican en la familia operativa; RFC, Threat Model y arquitectura de seguridad, en la de arquitectura; Integration Guide y API Specification, en la de diseño.** Mismo criterio: el dueño del artefacto.

**El criterio de paridad de `ESC-2` se desarrolla completo en el Test Plan.** Lo reclamaban tres documentos —el BRD como decisión de negocio, la tabla de equivalencias del SAD y el Test Plan como verificación—. Se eligió el Test Plan porque es donde el criterio se vuelve ejecutable; los otros dos lo referencian.

**El actor `ACT-10`, agente de IA, se incorpora al marco.** El encargo enumeraba QA, arquitecto, analista, desarrollador y product owner «entre otros». Se agregaron DevOps/SRE, seguridad, UX, technical writer y agente de IA, este último porque la guía incluye un documento sobre generación de código asistida y no tenía sentido tratar la práctica sin ubicar al actor que la ejecuta.

**El dominio recurrente de los ejemplos es un sistema de reserva de salas.** El catálogo de referencia usaba un caso propio, HomeHub, que el encargo excluye explícitamente por ser ajeno al marco conceptual. Se necesitaba un dominio único para que el mismo problema pudiera compararse desde artefactos distintos.

---

## Huecos abiertos

La revisión dejó siete temas sin cubrir, dos artefactos sin dueño asignado, dos verificaciones de fuente pendientes y dos convenciones menores sin fijar. Ninguno bloquea el uso de la guía.

El registro completo, con identificador estable, razón de exclusión y ubicación propuesta para cada uno, está en [Pendientes](Pendientes.md). Se mantiene como documento separado porque es un backlog vivo —las entradas se eliminan al resolverse— mientras que este informe es el acta de una revisión concreta y no debería cambiar.

Los dos de mayor rendimiento, para quien retome: consolidar el gobierno documental (`HUE-02`), porque la guía enseña a producir documentación y a evaluarla pero no a mantenerla viva; y escribir la guía de transición para migraciones (`HUE-03`), que es documentación característica del escenario que la propia guía identifica como el de mayor riesgo.

---

## Cómo mantener la guía consistente

Las comprobaciones de la primera sección son mecánicas y conviene repetirlas después de cualquier cambio de volumen. Tres bastan para detectar la mayoría de las regresiones: que todo enlace relativo resuelva, que todo identificador usado exista en la lista canónica de Convenciones, y que todo documento temático conserve sus cuatro entradas de escenario.

El defecto que ninguna comprobación mecánica detecta es el que más daño hace: dos documentos que afirman cosas distintas sobre el mismo tema sin contradecirse de forma evidente, como ocurrió con los estados de pantalla. Aparece en las fronteras entre familias, y la única manera de encontrarlo es leer las dos secciones enfrentadas.
