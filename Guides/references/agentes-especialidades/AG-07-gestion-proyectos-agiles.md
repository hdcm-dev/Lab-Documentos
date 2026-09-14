# AG-07 — Scrum Master / Gestión de Proyectos Ágiles

**Carpeta SDD:** `docs/07_plan-sprint/`  
**Perfil Profesional:** Planificación de sprints, métricas ágiles

---

## 1. ¿Qué es esta especialidad?

El **Gestor de Proyectos Ágiles** (Scrum Master en su faceta de planificación) es el profesional que facilita la ejecución del proyecto sprint a sprint. Se encarga de que cada iteración tenga un objetivo claro, un scope razonable, métricas de seguimiento y ceremonias bien definidas (planning, review, retrospectiva).

En el contexto SDD, este rol produce los **planes de iteración** para cada sprint, los **templates de ceremonias** (review, retro) y la **tabla de velocidad** del equipo. Su trabajo asegura que el backlog priorizado (AG-06) se traduzca en planes ejecutables con commitment realista.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el framework ágil y la escala del proyecto:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Gestión de Sprints (Scrum)** | Planificar, ejecutar y cerrar iteraciones timeboxed con ceremonias definidas | Equipos que usan Scrum con sprints de 1-4 semanas | Sprint Plans, Sprint Goals, Review/Retro templates, velocity charts, burndown |
| **Gestión de Flujo (Kanban)** | Optimizar el flujo continuo de trabajo con límites WIP y métricas de lead time | Equipos con demanda continua (soporte, ops, mantenimiento) o en entrega continua | Tablero Kanban, WIP limits, lead time / cycle time charts, cumulative flow diagram, políticas explícitas |
| **Gestión de Programas Ágiles (SAFe/LeSS)** | Coordinar múltiples equipos ágiles que trabajan en el mismo producto o portfolio | Organizaciones con 3+ equipos dependientes que necesitan sincronización | PI Planning, Program Board, ART sync, inter-team dependencies, Release Train |

> **En este proyecto** se aplica **Gestión de Sprints (Scrum)** porque el equipo trabaja con iteraciones de duración fija, ceremonias Scrum (planning, review, retro) y tracking de velocity.

#### Ejemplos temáticos por disciplina

**Gestión de Flujo (Kanban)** *(aplica en equipos con demanda continua)*
- *Ejemplo:* Un equipo de soporte de producción gestiona bugs, patches y requests de clientes. Usan un tablero Kanban con columnas (Triage → Análisis → Desarrollo → QA → Deploy) con WIP limits (máximo 3 en Desarrollo, 2 en QA). Miden lead time promedio (actualmente 4.2 días), cycle time (2.1 días) y usan el Cumulative Flow Diagram para detectar cuellos de botella (QA se acumula → necesitan más capacidad de testing).

**Gestión de Programas Ágiles** *(aplica en organizaciones multi-equipo)*
- *Ejemplo:* Una fintech tiene 5 equipos (Pagos, Onboarding, Compliance, Core Banking, Mobile) que trabajan sobre la misma plataforma. Usan SAFe con PI Planning trimestral donde los 5 equipos sincronizan objetivos, identifican dependencias inter-equipo en el Program Board, y definen el Release Train para la siguiente iteración. El RTE (Release Train Engineer) facilita las sincronizaciones semanales y gestiona riesgos cross-team.

### Diferencia con AG-06 (Backlog)

- El **AG-06** gestiona el *qué*: product backlog, priorización, user stories
- El **AG-07** gestiona el *cuándo* y *cómo*: sprint plans, velocity, ceremonias, métricas

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Sprint Planning | Crear el plan de cada sprint con objetivo, historias comprometidas y capacidad | `plan-iteracion_sprint-XX_v1.0.md` |
| Definición de Sprint Goal | Articular el objetivo del sprint en una frase orientada a valor | Sección en cada sprint plan |
| Facilitación de Review | Guiar la demostración del incremento al PO/stakeholders | `template-sprint-review_v1.0.md` |
| Facilitación de Retrospectiva | Guiar la reflexión de mejora continua del equipo | `template-sprint-retrospectiva_v1.0.md` |
| Tracking de velocidad | Registrar y analizar la velocidad del equipo por sprint | `velocidad-equipo_v1.0.md` |
| Gestión de riesgos del sprint | Identificar impedimentos y riesgos con plan de mitigación | Sección en cada sprint plan |
| Consistencia inter-sprint | Asegurar que todos los sprints siguen el mismo formato | Revisión de formato |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Sprint Plan / Sprint Backlog | Scrum Guide 2020 §Sprint Backlog | Compromiso del equipo para el sprint actual |
| Sprint Goal | Scrum Guide 2020 §Sprint Goal | Dirección del sprint, criterio de coherencia |
| Sprint Review | Scrum Guide 2020 §Sprint Review | Feedback del PO, validación del incremento |
| Sprint Retrospectiva | Scrum Guide 2020 §Sprint Retrospective | Mejora continua del proceso |
| Velocity chart | Scrum metrics / Evidence-Based Management | Proyección de capacidad para sprints futuros |
| Burndown chart | Scrum metrics | Tracking diario del progreso durante el sprint |
| Definition of Done (referencia) | Scrum Guide 2020 §Definition of Done | Cada sprint referencia al DoD canónico |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Sprint Goal claro | 1 frase orientada a valor, no lista de tareas | Longitud ≤ 2 oraciones, sin bullet points |
| Formato consistente | Todos los sprint plans tienen la misma estructura | Diff de estructura entre sprint 01 y sprint N |
| Capacidad realista | Los SP comprometidos no exceden la velocidad promedio | SP comprometidos ≤ velocity promedio |
| DoD por referencia | El sprint no redefine el DoD, referencia al canónico | Link a `definition-of-done_v1.0.md` |
| Riesgos con mitigación | Cada riesgo identificado tiene un plan de acción | 0 riesgos sin mitigación |
| Templates prácticos | Review y Retro templates son ejecutables (no teóricos) | Feedback del equipo post-ceremonia |
| Velocity actualizada | La tabla de velocidad tiene datos de sprints completados | 0 sprints completados sin registro |

---

## 5. Preguntas guía para el análisis

### 5.1 Sprint Planning

> **P1:** ¿El Sprint Goal es una frase de valor o una lista de tareas?
>
> *Ejemplo práctico:*
> - ✅ "Al final del sprint, el motor puede procesar una plantilla DSL simple y generar un AST válido"
> - ❌ "Implementar parser, crear nodos, escribir tests del AST"

> **P2:** ¿La capacidad del sprint es realista?
>
> *Ejemplo práctico:* "Velocity promedio: 18 SP. Sprint 05 compromete 25 SP → riesgo de no cumplir. Reducir a 20 SP o justificar capacidad extra."

> **P3:** ¿Las historias comprometidas suman coherentemente con los puntos declarados?
>
> *Ejemplo práctico:* "Sprint dice 'Capacidad: 20 SP'. Las historias listadas suman 15 SP. ¿Faltan historias o la capacidad está mal declarada?"

### 5.2 Consistencia inter-sprint

> **P4:** ¿Todos los sprint plans tienen la misma estructura?
>
> *Ejemplo práctico:* "Sprint 01 tiene: Objetivo, Historias, DoD, Plan de pruebas, Riesgos. Sprint 04 no tiene sección de Riesgos → inconsistencia."

> **P5:** ¿El DoD se redefine en cada sprint o referencia al canónico?
>
> *Ejemplo práctico:*
> - ✅ "Aplica la [DoD canónica](../08_calidad_y_pruebas/definition-of-done_v1.0.md). Criterios específicos de este sprint: [lista]"
> - ❌ "Una tarea se considera terminada cuando: el código compila, tiene tests, ..." (redefinición)

### 5.3 Velocity y métricas

> **P6:** ¿La tabla de velocidad tiene instrucciones de actualización?
>
> *Ejemplo práctico:*
> | Sprint | SP Comprometidos | SP Completados | Velocity | Notas |
> |--------|-----------------|----------------|----------|-------|
> | S01 | 15 | 13 | 13 | Sprint inaugural, curva de aprendizaje |
> | S02 | 18 | 18 | 18 | — |
> | **Promedio** | — | — | **15.5** | — |

> **P7:** ¿Los riesgos del sprint tienen plan de mitigación?
>
> *Ejemplo práctico:*
> | Riesgo | Probabilidad | Impacto | Mitigación |
> |--------|-------------|---------|------------|
> | Integración BT no probada | Alta | Alto | Spike técnico en día 1, fallback a mock |
> | Ausencia de miembro | Media | Medio | Par programming para no generar silos |

---

## 6. Plantilla base

### 6.1 Plan de iteración (Sprint Plan)

```markdown
# Plan de Iteración — Sprint XX

**Proyecto:** [Nombre]  
**Sprint:** XX  
**Fecha inicio:** [YYYY-MM-DD]  
**Fecha fin:** [YYYY-MM-DD]  
**Capacidad:** [XX] Story Points

---

## 🎯 Sprint Goal

[Una frase orientada a valor que describe qué se logra al final del sprint.]

---

## 📋 Historias comprometidas

| ID | User Story | SP | CU relacionados | Responsable |
|----|------------|-----|-----------------|-------------|
| US-XX | [Como/Quiero/Para] | [N] | CU-XX | [Nombre] |

**Total SP comprometidos:** [Suma]

---

## ✅ Criterios de terminado (Definition of Done)

Aplica la [Definition of Done canónica](../08_calidad_y_pruebas/definition-of-done_v1.0.md).

### Criterios específicos de este sprint

- [Criterio 1 específico del sprint]
- [Criterio 2 específico del sprint]

---

## 🧪 Plan de pruebas del sprint

### Pruebas unitarias
- [Test 1]
- [Test 2]

### Pruebas de integración
- [Test 1]

---

## ⚠️ Riesgos y mitigación

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| [Riesgo 1] | [Alta/Media/Baja] | [Alto/Medio/Bajo] | [Plan de acción] |

---

## 📝 Notas del sprint

[Observaciones, decisiones tomadas durante el sprint, deuda técnica generada.]
```

### 6.2 Template de Sprint Review

```markdown
# Sprint Review — Sprint XX

**Fecha:** [YYYY-MM-DD]  
**Asistentes:** [Nombres]

---

## 1. Sprint Goal

**Objetivo:** [Copiar del sprint plan]  
**¿Se cumplió?** [Sí / Parcialmente / No]

## 2. Incremento demostrado

| US | Demostración | Feedback PO | Estado |
|----|-------------|-------------|--------|
| US-XX | [Qué se mostró] | [Comentarios] | ✅ Aceptada / ❌ Rechazada |

## 3. Métricas

| Métrica | Valor |
|---------|-------|
| SP comprometidos | [N] |
| SP completados | [N] |
| Velocity | [N] |
| Tests totales | [N] |
| Cobertura | [%] |

## 4. Feedback y acciones

| Feedback | Acción | Responsable | Sprint target |
|----------|--------|-------------|---------------|
| [Feedback 1] | [Acción] | [Nombre] | S-XX |
```

### 6.3 Template de Retrospectiva

```markdown
# Sprint Retrospectiva — Sprint XX

**Fecha:** [YYYY-MM-DD]  
**Facilitador:** [Nombre]  
**Formato:** [Start-Stop-Continue / Mad-Sad-Glad / 4Ls / etc.]

---

## ✅ Qué salió bien (seguir haciendo)

- [Ítem 1]

## ❌ Qué salió mal (dejar de hacer)

- [Ítem 1]

## 💡 Qué podemos mejorar (empezar a hacer)

- [Ítem 1]

## 🎯 Acciones de mejora

| Acción | Responsable | Sprint target | Estado |
|--------|-------------|---------------|--------|
| [Acción concreta] | [Nombre] | S-XX | Pendiente |
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Sprint Goal = lista de tareas | "Implementar A, B, C" no da dirección ni prioridad | Reformular como frase de valor: "El motor procesa DSL end-to-end" |
| DoD redefinido por sprint | Cada sprint inventa su DoD → inconsistencia | Referenciar DoD canónico + agregar criterios específicos |
| Overcommitment | Más SP que la velocity promedio | Regla: nunca comprometer > 110% de velocity promedio |
| Sin riesgos | No se identifican riesgos → no hay plan B | Mínimo 2 riesgos por sprint con mitigación |
| Retro sin acciones | Se habla mucho pero no se define nada concreto | Cada retro debe producir ≥ 1 acción con responsable |
| Velocity no registrada | No se sabe cuánto puede el equipo | Actualizar tabla después de cada sprint review |
