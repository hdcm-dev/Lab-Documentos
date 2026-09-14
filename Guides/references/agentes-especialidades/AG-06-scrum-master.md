# AG-06 — Scrum Master / Agile Coach

**Carpeta SDD:** `docs/06_backlog-tecnico/`  
**Perfil Profesional:** Gestión de backlog, metodología ágil

---

## 1. ¿Qué es esta especialidad?

El **Scrum Master** (en su faceta de Agile Coach orientada al backlog) es el profesional que facilita la gestión del producto desde la perspectiva ágil. Asegura que el backlog esté priorizado, que las user stories sean claras, estimables y verificables, y que exista trazabilidad entre las necesidades de negocio, los casos de uso y las tareas técnicas.

En el contexto SDD, este rol produce y mantiene el **Product Backlog**, el **Backlog Técnico** y la **Definition of Ready (DoR)**. Su trabajo conecta la capa de requisitos funcionales (CU) con el trabajo que se planifica en sprints.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el nivel de madurez ágil y el alcance organizacional:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Gestión de Product Backlog** | Construir, priorizar y mantener el backlog con user stories estimables y verificables | Todo equipo Scrum/Kanban que gestione trabajo priorizado | Product Backlog ordenado, user stories INVEST, épicas, priorización MoSCoW, Definition of Ready |
| **Coaching Ágil (Agile Coaching)** | Formar al equipo en mindset ágil, facilitar la adopción de prácticas y detectar anti-patrones | Organizaciones en transición ágil o equipos con baja madurez en prácticas | Evaluaciones de madurez ágil, workshops de formación, retrospectivas de proceso, guías de anti-patrones |
| **Facilitación de Equipos** | Guiar dinámicas grupales, resolver conflictos, promover colaboración y auto-organización | Equipos multidisciplinarios donde la comunicación es clave | Workshops de team building, acuerdos de trabajo, técnicas de facilitación (Liberating Structures, Open Space), conflict resolution |

> **En este proyecto** se aplica **Gestión de Product Backlog** porque el enfoque es construir y mantener el backlog con trazabilidad US→CU, priorización MoSCoW y Definition of Ready.

#### Ejemplos temáticos por disciplina

**Coaching Ágil** *(aplica en transformación organizacional)*
- *Ejemplo:* Una empresa de 200 personas migra de waterfall a Scrum. El Agile Coach evalúa la madurez actual de los 6 equipos, diseña un plan de adopción progresivo (mes 1: daily + backlog; mes 3: sprints completos; mes 6: retrospectivas efectivas), facilita workshops sobre estimación y Definition of Done, y detecta anti-patrones frecuentes ("el Sprint 0 eterno", "la retro sin acción").

**Facilitación de Equipos** *(aplica como skill transversal)*
- *Ejemplo:* Un equipo de 10 personas tiene conflictos recurrentes entre frontend y backend por la definición de contratos de API. El facilitador diseña un workshop de "Contract-First Design" usando Liberating Structures (1-2-4-All), donde cada par propone un contrato, se discuten en grupos y se converge en un acuerdo documentado. Establece un acuerdo de equipo para API reviews antes de implementar.

### Diferencia con AG-07 (Gestión de Proyectos Ágiles)

- El **AG-06 (Backlog)** se enfoca en *qué* se construye: priorización, user stories, épicas, estimación
- El **AG-07 (Sprints)** se enfoca en *cuándo* y *cómo* se planifica: sprint plans, velocity, ceremonias

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Construcción del Product Backlog | Crear y mantener la lista priorizada de user stories | `product-backlog_v1.0.md` |
| Mantenimiento del Backlog Técnico | Registrar deuda técnica y tareas de infraestructura | `backlog-tecnico_v1.0.md` |
| Definición de DoR | Establecer criterios que una historia debe cumplir antes de entrar a un sprint | `definition-of-ready_v1.0.md` |
| Refinamiento del backlog | Sesiones de refinamiento para descomponer, estimar y clarificar historias | Backlog actualizado |
| Trazabilidad US→CU→BT | Verificar que toda user story traza a CU y, si aplica, a tareas técnicas | Matriz de trazabilidad |
| Priorización MoSCoW | Clasificar historias en Must/Should/Could/Won't | Columna de prioridad en backlog |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Product Backlog | Scrum Guide 2020 §Product Backlog | Input para sprint planning: qué se compromete |
| User Stories | Format: Como [rol] Quiero [acción] Para [beneficio] | Unidad de trabajo para estimación y planificación |
| Definition of Ready (DoR) | Scrum practices / Kanban filters | Filtro de entrada para sprint planning |
| Priorización MoSCoW | MoSCoW method (DSDM Atern) | Priorización basada en valor de negocio |
| Backlog Técnico | Technical debt management practices | Visibilización de deuda técnica ante el PO |
| Épicas | SAFe / large user story decomposition | Agrupación de US relacionadas para roadmap |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| User stories INVEST | Cada US es Independiente, Negociable, Valiosa, Estimable, Small, Testeable | Checklist INVEST por US |
| Formato estándar | Todas las US siguen "Como [rol] Quiero [acción] Para [beneficio]" | 0 US sin formato |
| Trazabilidad completa | Cada US traza a ≥1 CU | 0 US huérfanas |
| Estimación presente | Todas las US priorizadas tienen story points | 0 US Must/Should sin estimación |
| Criterios de aceptación | Cada US tiene al menos 2 criterios de aceptación | 0 US sin criterios |
| DoR aplicable | El DoR no es excesivamente burocrático (≤ 8 criterios) | ≤ 8 ítems en DoR |
| Backlog técnico visible | La deuda técnica está registrada y priorizada | 0 items de deuda ocultos |

---

## 5. Preguntas guía para el análisis

### 5.1 User Stories

> **P1:** ¿La user story describe valor para alguien?
>
> *Ejemplo práctico:*
> - ✅ "Como desarrollador .NET, quiero renderizar un documento a ESC/POS, para poder imprimir tickets en impresoras térmicas"
> - ❌ "Implementar EscPosRenderer" (es una tarea técnica, no una US)

> **P2:** ¿La user story es lo suficientemente pequeña para un sprint?
>
> *Ejemplo práctico:* "Renderizar a ESC/POS" es una épica. Descomponer en: US-01 "Renderizar texto simple", US-02 "Renderizar con alineación", US-03 "Renderizar con estilos (bold, underline)"

> **P3:** ¿Los criterios de aceptación son verificables?
>
> *Ejemplo práctico:*
> - ✅ "Given un documento con texto bold, When se renderiza a ESC/POS, Then los bytes contienen `ESC ! 0x08`"
> - ❌ "El renderizado debe funcionar correctamente"

### 5.2 Priorización

> **P4:** ¿La priorización MoSCoW refleja el valor de negocio real?
>
> *Ejemplo práctico:*
> | US | Priorización | Justificación |
> |---|---|---|
> | Parser DSL | **Must** | Sin parser no hay motor — es prerequisito de todo |
> | Renderizar PDF | **Won't** (v1.0) | Caso de uso secundario, no bloquea la release alfa |
> | Preview UI | **Should** | Importante para DX pero no bloquea impresión |

> **P5:** ¿Hay user stories que se infieren de los CU pero no están en el backlog?
>
> *Ejemplo práctico:* "CU-30, CU-31, CU-32 (manejo de errores) → ¿hay US de error handling en el backlog? Si no, agregarlas."

### 5.3 Definition of Ready

> **P6:** ¿El DoR es práctico o burocrático?
>
> *Ejemplo práctico:*
> - ✅ DoR con 6 criterios: US escrita, criterios de aceptación, estimada, sin dependencias bloqueantes, CU trazado, datos de ejemplo disponibles
> - ❌ DoR con 15 criterios: incluye "diagrama de secuencia completo" y "revisión por 3 stakeholders" → demasiado pesado

---

## 6. Plantilla base

### 6.1 Product Backlog

```markdown
# Product Backlog

**Proyecto:** [Nombre]  
**Versión:** 1.0  
**Última actualización:** [YYYY-MM-DD]

---

## Épicas

| ID | Épica | Descripción | Sprints estimados |
|----|-------|-------------|-------------------|
| E-01 | [Nombre épica] | [Descripción breve] | S01-S02 |

## Backlog priorizado

| ID | User Story | Épica | Prioridad MoSCoW | SP | CU relacionados | Sprint |
|----|------------|-------|-------------------|----|-----------------|--------|
| US-01 | Como [rol], quiero [acción], para [beneficio] | E-01 | Must | 5 | CU-01, CU-02 | S01 |
| US-02 | Como [rol], quiero [acción], para [beneficio] | E-01 | Must | 3 | CU-03 | S01 |
| US-03 | Como [rol], quiero [acción], para [beneficio] | E-02 | Should | 8 | CU-06 | S03 |

## Resumen por prioridad

| Prioridad | Cantidad | Story Points |
|-----------|----------|--------------|
| Must | [n] | [sp] |
| Should | [n] | [sp] |
| Could | [n] | [sp] |
| Won't (v1.0) | [n] | — |
```

### 6.2 Definition of Ready

```markdown
# Definition of Ready (DoR)

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

Una User Story está lista para entrar a un Sprint cuando cumple TODOS estos criterios:

## Criterios

| # | Criterio | Verificación |
|---|----------|--------------|
| 1 | Escrita en formato "Como/Quiero/Para" | Revisión de texto |
| 2 | Tiene ≥ 2 criterios de aceptación en formato BDD | Revisión de criterios |
| 3 | Estimada en Story Points por el equipo | Consenso en planning poker |
| 4 | Sin dependencias bloqueantes no resueltas | Revisión de dependencias |
| 5 | Traza a ≥ 1 Caso de Uso (CU-XX) | Verificación en matriz |
| 6 | Datos de ejemplo disponibles para testing | Revisión de fixtures |

## Notas

- Si una US no cumple el DoR, regresa al backlog para refinamiento
- El PO es responsable de resolver dudas funcionales antes del sprint planning
- El equipo puede proponer excepciones documentadas al DoR
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| US como tarea técnica | "Implementar Parser" no tiene valor para el usuario | Reformular: "Como dev, quiero interpretar DSL, para generar documentos" |
| Épicas sin descomponer | Una épica de 40 SP en un sprint de 15 | Descomponer en US de 3-8 SP |
| Sin MoSCoW | Todo es "Must" → no hay priorización real | Forzar distribución: 60% Must, 20% Should, 20% Could |
| DoR excesivo | 15 criterios → nada nunca está "ready" | Máximo 6-8 criterios prácticos |
| Backlog técnico oculto | La deuda técnica no está visible para el PO | Registrar como ítems priorizables en el backlog |
| Sin trazabilidad US→CU | No se puede validar completitud del sistema | Columna "CU relacionados" obligatoria en cada US |
