---
doc_id: ANEXO-PENDIENTES
doc_type: anexo
title: Pendientes y huecos registrados
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Documentación técnica
last_review: 2026-07-18
audience: [humano, agente]
traces: [ANEXO-REVISION, GUIA-INDICE, MAPA-CONCEPTUAL]
---

# Pendientes y huecos registrados

## Resumen ejecutivo

Backlog de la guía: lo que se decidió no cubrir en la primera versión, lo que quedó repartido sin fuente única y lo que necesita contraste contra fuente antes de sostenerse en un contexto formal. Cada entrada tiene identificador estable, ubicación propuesta y la razón por la que quedó afuera, para que retomar el trabajo no exija repetir el diagnóstico.

Ninguna entrada bloquea el uso de la guía. Están ordenadas por el criterio con el que conviene atacarlas: primero lo que ya está escrito pero disperso, después lo que falta, al final lo que solo requiere verificación.

El diagnóstico que las produjo está en el [informe de consistencia](Revision-de-Consistencia.md).

---

## Prioridad 1 — Contenido disperso sin fuente única

Estos temas ya están tratados en la guía, repartidos entre dos o tres documentos. El trabajo no es escribirlos sino consolidarlos y dejar que el resto los referencie. Es el tipo de deuda que crece sola: cada documento nuevo que toque el tema agrega una versión más.

| ID | Hueco | Dónde está hoy | Ubicación propuesta |
|----|-------|----------------|---------------------|
| `HUE-01` | Métricas de ingeniería y de entrega (DORA, métricas de flujo) | Repartido entre [Developer Guide](../60-Desarrollo/Developer-Guide.md), [Test Plan](../60-Desarrollo/Test-Plan.md) y [Kanban](../80-Metodos-Agiles/Kanban.md) | Transversal nuevo en `95-Transversales/` |
| `HUE-02` | Gobierno documental: propiedad, ciclo de revisión, medición de utilidad y retiro de documentos obsoletos | Tratado parcialmente desde el [manual de usuario](../70-Usuarios/User-Manual.md) y el [manifiesto ágil](../80-Metodos-Agiles/Manifiesto-y-Documentacion.md) | Transversal nuevo en `95-Transversales/` |

`HUE-02` es el más relevante de los dos. La guía enseña a producir documentación y a evaluarla, pero no a mantenerla viva, y la deuda documental que el propio manifiesto describe nace justamente ahí.

---

## Prioridad 2 — Artefactos sin dueño asignado

Piezas que la guía menciona como necesarias pero que caen entre dos familias y no recibieron identificador.

| ID | Hueco | Por qué quedó sin dueño | Ubicación propuesta |
|----|-------|------------------------|---------------------|
| `HUE-03` | Guía de transición para migraciones: el mapa «esto que hacía así, ahora se hace asá» | Tratada como artefacto necesario dentro del manual de usuario, sin `doc_id` propio | `70-Usuarios/` o `60-Desarrollo/` |
| `HUE-04` | Catálogo de errores de API traducido a mensajes de interfaz | Cae entre el contrato de [diseño](../40-Diseno/API-Specification.md) y la microcopy de [UX](../95-Transversales/UX-UI-y-Flujo-de-Usuario.md) | `95-Transversales/` o `70-Usuarios/` |

`HUE-03` es el que más rinde: es documentación característica de `ESC-2`, el escenario que la guía identifica como el de mayor riesgo documental, y hoy no tiene ficha propia.

---

## Prioridad 3 — Temas no cubiertos

Quedaron fuera por decisión de alcance, no por olvido. Cada uno tiene su razón registrada.

| ID | Hueco | Razón de la exclusión | Ubicación propuesta |
|----|-------|----------------------|---------------------|
| `HUE-05` | Arquitecturas orientadas a eventos y CQRS | La serie las roza al tratar mensajería en [microservicios](../90-Modelos-de-Arquitectura/Microservicios.md), sin desarrollarlas como modelo propio | `90-Modelos-de-Arquitectura/` |
| `HUE-06` | Estimación y previsibilidad contractual | Es método de gestión, no artefacto documental; la [comparativa de métodos](../80-Metodos-Agiles/Comparativa-y-Criterios.md) lo declara fuera de alcance | `80-Metodos-Agiles/` |
| `HUE-07` | Modelo Kano, story mapping y técnicas de priorización | Mismo criterio: son método, no artefacto | `80-Metodos-Agiles/` |

`HUE-05` es el candidato más fuerte a incorporarse: a diferencia de los otros dos, sí es un modelo de arquitectura con exigencias documentales propias —contratos de eventos, versionado de esquemas, documentación de la consistencia eventual— y la serie quedaría más completa con él.

---

## Prioridad 4 — Verificación contra fuente

No son huecos de contenido sino de respaldo. La guía se sostiene sin resolverlos; un uso formal, ante auditoría o certificación, no.

| ID | Qué verificar | Estado |
|----|--------------|--------|
| `VER-01` | La revisión 2023 de **ISO/IEC 25010** reorganizó parte de la taxonomía de atributos de calidad. La [comparativa de modelos](../90-Modelos-de-Arquitectura/Comparativa-y-Criterios.md) se apoya deliberadamente en las características cuya interpretación no cambia entre versiones | No verificado contra la norma vigente |
| `VER-02` | De las normas **ISO/IEC/IEEE** citadas —29148, 42010, 25010, 29119, 26511, 26514, 26515— se cita designación, sección e idea, nunca texto, por licencia | Ninguna afirmación depende de cita literal; sostener un argumento ante un auditor exige ir a la fuente |

---

## Prioridad 5 — Convenciones menores sin fijar

Detectadas al revisar los ejemplos. No afectan la corrección de la guía.

| ID | Cuestión | Estado actual |
|----|----------|---------------|
| `CNV-01` | Encabezado de la sección de cambios sin publicar en un changelog: español (`[Sin publicar]`) o el `[Unreleased]` de Keep a Changelog | Los ejemplos usan `[Sin publicar]`; la guía no fija criterio |
| `CNV-02` | Los ejemplos cruzados de [Release Notes](../60-Desarrollo/Release-Notes.md) y [Change Log](../60-Desarrollo/Change-Log.md) describen la misma versión 1.3.1 sin que la entrada de origen de un defecto aparezca en ambos | Sin contradicción; el lector que cruce ambos ejemplos no encuentra la traza completa |

---

## Cómo usar este registro

Al retomar la guía, la secuencia con mejor rendimiento es consolidar antes de agregar: `HUE-01` y `HUE-02` eliminan duplicación existente, mientras que `HUE-05` agrega superficie nueva que después habrá que mantener.

Toda entrada que se resuelva se elimina de este documento y se registra en el CHANGELOG del repositorio, no acá. Un backlog que acumula entradas resueltas deja de leerse.
