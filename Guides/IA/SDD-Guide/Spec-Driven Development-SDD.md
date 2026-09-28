# Spec-Driven Development (SDD)

SDD es un enfoque de desarrollo en el que se escribe primero una especificación formal antes de generar o escribir código. En el contexto de desarrollo asistido por IA, la especificación actúa como el contrato que guía al modelo para producir implementaciones coherentes y verificables.

---

## Los Tres Niveles de SDD

### Nivel 1 — Spec-First *(el más común)*

> "La spec guía la implementación. Una vez completada, se descarta."

El flujo es: escribir la spec → Claude la implementa → descartar el documento.

**Cuándo usarlo:**
- Tareas de una sola sesión
- Proyectos personales sin coordinación de equipo
- Prototipos y experimentos
- Correcciones puntuales de bugs

**Ventaja:** Sin carga de mantenimiento.  
**Desventaja:** En seis meses, no habrá documentación disponible si se necesitan modificaciones.

---

### Nivel 2 — Spec-Anchored *(estándar para equipos)*

Tanto la especificación como el código se mantienen como **artefactos vivos**. Los cambios a la spec **preceden** a los cambios de código.

**Cuándo usarlo:**
- Proyectos con múltiples contribuyentes
- Sistemas que requieren documentación de cumplimiento (compliance)
- Productos con horizonte de mantenimiento largo (6+ meses)
- Features que sufrirán iteraciones múltiples

**Requisito crítico:** disciplina para actualizar la spec antes de tocar el código. Sin esa disciplina, spec y código divergen y el artefacto pierde valor.

---

### Nivel 3 — Spec-as-Source *(experimental)*

La especificación **es** el artefacto principal; el código se regenera bajo demanda a partir de ella.

**Desafío fundamental:** inconsistencia determinística.  
> "Identical specifications do not produce identical code."  
Dos generaciones con la misma spec producen implementaciones sintácticamente distintas, lo que complica el debugging y el control de versiones.

**Cuándo tiene sentido:**
- Código altamente repetitivo (operaciones CRUD)
- Código desechable (scripts de migración, scaffolding)
- Entornos con cobertura de tests robusta que validen el comportamiento sin importar la forma del código

---

## Marco de Decisión

| Situación | Nivel recomendado |
|---|---|
| Trabajo individual, sesión única | Spec-First (1) |
| Equipo de 2+ personas | Spec-Anchored (2) |
| Sistema con mantenimiento 6+ meses | Spec-Anchored (2) |
| Código repetitivo / desechable con tests | Spec-as-Source (3) |
| Experimento personal con buena cobertura | Spec-as-Source (3) |

**Recomendación general:** empezar con Spec-First y migrar a Spec-Anchored cuando el proyecto involucre equipo o tenga un horizonte de mantenimiento largo. Reservar Spec-as-Source para contextos controlados con tests que garanticen el comportamiento esperado.

---

## Conceptos clave

- **Spec como contrato:** la especificación define el *qué* y el *por qué*; el código define el *cómo*.
- **Artefacto vivo:** una spec que no se actualiza junto con el código se convierte en documentación desactualizada y pierde su valor como fuente de verdad.
- **Inconsistencia determinística:** los modelos de lenguaje no producen código idéntico ante entradas idénticas; esto es relevante cuando se plantea regenerar código desde specs.

---

## Referencias

- [Three Levels of SDD — Panaversity Agent Factory](https://agentfactory.panaversity.org/docs/General-Agents-Foundations/spec-driven-development/three-levels-of-sdd)
