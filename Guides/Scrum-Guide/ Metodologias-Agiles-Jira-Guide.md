# Metodologías Ágiles y Gestión de Tareas con Jira

**Documento:**  Metodologias-Agiles-Jira-Guide.md 
**Versión:** 1.0  
**Fecha:** 2026-04-13  
**Autor:** Equipo de Desarrollo  
**Estado:** Activo  
**Contexto:** Guía de referencia para gestión ágil de proyectos de software

---

## Índice

### PARTE 1 — METODOLOGÍAS ÁGILES

- [Sección 1 — Propósito del documento](#sección-1--propósito-del-documento)
  - [1.1 Para qué sirve este documento](#11-para-qué-sirve-este-documento)
  - [1.2 A quién está dirigido](#12-a-quién-está-dirigido)
  - [1.3 Relación con la documentación SDD del proyecto](#13-relación-con-la-documentación-sdd-del-proyecto)
- [Sección 2 — Manifiesto Ágil y principios fundamentales](#sección-2--manifiesto-ágil-y-principios-fundamentales)
  - [2.1 Los 4 valores del Manifiesto Ágil](#21-los-4-valores-del-manifiesto-ágil)
  - [2.2 Los 12 principios ágiles](#22-los-12-principios-ágiles)
  - [2.3 Contexto histórico: por qué surgió](#23-contexto-histórico-por-qué-surgió)
  - [2.4 Tabla comparativa: Waterfall vs. Ágil](#24-tabla-comparativa-waterfall-vs-ágil)
- [Sección 3 — Scrum](#sección-3--scrum)
  - [3.1 Definición y origen](#31-definición-y-origen)
  - [3.2 Los 3 pilares](#32-los-3-pilares)
  - [3.3 Los 5 valores](#33-los-5-valores)
  - [3.4 Roles](#34-roles)
  - [3.5 Artefactos](#35-artefactos)
  - [3.6 Eventos / Ceremonias](#36-eventos--ceremonias)
  - [3.7 El Sprint](#37-el-sprint)
  - [3.8 Diagrama del ciclo Scrum](#38-diagrama-del-ciclo-scrum)
- [Sección 4 — Kanban](#sección-4--kanban)
  - [4.1 Definición y origen](#41-definición-y-origen)
  - [4.2 Principios de Kanban](#42-principios-de-kanban)
  - [4.3 Tablero Kanban](#43-tablero-kanban)
  - [4.4 Métricas Kanban](#44-métricas-kanban)
  - [4.5 Tabla comparativa: Scrum vs. Kanban](#45-tabla-comparativa-scrum-vs-kanban)
  - [4.6 Cuándo elegir Kanban sobre Scrum](#46-cuándo-elegir-kanban-sobre-scrum)
- [Sección 5 — Otras metodologías ágiles (resumen)](#sección-5--otras-metodologías-ágiles-resumen)
  - [5.1 XP (Extreme Programming)](#51-xp-extreme-programming)
  - [5.2 SAFe (Scaled Agile Framework)](#52-safe-scaled-agile-framework)
  - [5.3 LeSS (Large-Scale Scrum)](#53-less-large-scale-scrum)
  - [5.4 Scrumban](#54-scrumban)
  - [5.5 Tabla comparativa rápida](#55-tabla-comparativa-rápida)
- [Sección 6 — User Stories, Épicas y Tareas](#sección-6--user-stories-épicas-y-tareas)
  - [6.0 Introducción — Conceptos fundamentales y vocabulario](#60-introducción--conceptos-fundamentales-y-vocabulario)
  - [6.1 Formato estándar de User Story](#61-formato-estándar-de-user-story)
  - [6.2 Criterios INVEST](#62-criterios-invest)
  - [6.3 Criterios de aceptación: formato Given/When/Then](#63-criterios-de-aceptación-formato-givenwhenthen)
  - [6.4 Jerarquía: Iniciativa → Épica → User Story → Sub-tarea → Bug](#64-jerarquía-iniciativa--épica--user-story--sub-tarea--bug)
  - [6.5 Estimación](#65-estimación)
  - [6.6 Priorización](#66-priorización)
- [Sección 7 — Métricas ágiles](#sección-7--métricas-ágiles)
  - [7.1 Velocity](#71-velocity)
  - [7.2 Burndown Chart](#72-burndown-chart)
  - [7.3 Burnup Chart](#73-burnup-chart)
  - [7.4 Cumulative Flow Diagram (CFD)](#74-cumulative-flow-diagram-cfd)
  - [7.5 Lead Time y Cycle Time](#75-lead-time-y-cycle-time)
  - [7.6 Tabla resumen de métricas](#76-tabla-resumen-de-métricas)
- [Sección 8 — Anti-patrones ágiles](#sección-8--anti-patrones-ágiles)
  - [8.1 Anti-patrones del backlog y gestión](#81-anti-patrones-del-backlog-y-gestión)
  - [8.2 Anti-patrones de sprints y ceremonias](#82-anti-patrones-de-sprints-y-ceremonias)
  - [8.3 Anti-patrones adicionales comunes](#83-anti-patrones-adicionales-comunes)

### PARTE 2 — GESTIÓN DE TAREAS CON JIRA

- [Sección 9 — Introducción a Jira](#sección-9--introducción-a-jira)
  - [9.1 Qué es Jira](#91-qué-es-jira)
  - [9.2 Conceptos base](#92-conceptos-base)
  - [9.3 Tipos de proyecto](#93-tipos-de-proyecto)
  - [9.4 Cuándo usar Jira vs. alternativas](#94-cuándo-usar-jira-vs-alternativas)
- [Sección 10 — Nomenclatura y tipos de issue en Jira](#sección-10--nomenclatura-y-tipos-de-issue-en-jira)
  - [10.1 Tipos de issue estándar](#101-tipos-de-issue-estándar)
  - [10.2 Campos clave de cada issue](#102-campos-clave-de-cada-issue)
  - [10.3 Prioridades en Jira y mapeo a MoSCoW](#103-prioridades-en-jira-y-mapeo-a-moscow)
  - [10.4 Estados y workflow típico](#104-estados-y-workflow-típico)
  - [10.5 Tabla de nomenclatura sugerida](#105-tabla-de-nomenclatura-sugerida)
- [Sección 11 — Configuración de un proyecto Scrum en Jira](#sección-11--configuración-de-un-proyecto-scrum-en-jira)
  - [11.1 Crear proyecto Scrum: paso a paso](#111-crear-proyecto-scrum-paso-a-paso)
  - [11.2 Configurar Board](#112-configurar-board)
  - [11.3 Configurar Backlog](#113-configurar-backlog)
  - [11.4 Crear y gestionar Sprints](#114-crear-y-gestionar-sprints)
  - [11.5 Configurar Epics](#115-configurar-epics)
  - [11.6 Configurar Releases (Fix Versions)](#116-configurar-releases-fix-versions)
  - [11.7 Componentes](#117-componentes)
- [Sección 12 — Flujo de trabajo diario con Jira](#sección-12--flujo-de-trabajo-diario-con-jira)
  - [12.1 Sprint Planning en Jira](#121-sprint-planning-en-jira)
  - [12.2 Daily Scrum con Jira](#122-daily-scrum-con-jira)
  - [12.3 Durante el sprint](#123-durante-el-sprint)
  - [12.4 Sprint Review en Jira](#124-sprint-review-en-jira)
  - [12.5 Sprint Retrospective](#125-sprint-retrospective)
  - [12.6 Refinamiento en Jira](#126-refinamiento-en-jira)
  - [12.7 Diagrama del flujo diario con Jira](#127-diagrama-del-flujo-diario-con-jira)
- [Sección 13 — Reportes y métricas en Jira](#sección-13--reportes-y-métricas-en-jira)
  - [13.1 Velocity Chart](#131-velocity-chart)
  - [13.2 Burndown Chart](#132-burndown-chart)
  - [13.3 Sprint Report](#133-sprint-report)
  - [13.4 Cumulative Flow Diagram (CFD)](#134-cumulative-flow-diagram-cfd)
  - [13.5 Control Chart](#135-control-chart)
  - [13.6 Dashboards personalizados](#136-dashboards-personalizados)
  - [13.7 JQL (Jira Query Language)](#137-jql-jira-query-language)
- [Sección 14 — Hitos (Milestones) y Releases en Jira](#sección-14--hitos-milestones-y-releases-en-jira)
  - [14.1 Fix Versions como Milestones](#141-fix-versions-como-milestones)
  - [14.2 Release Hub](#142-release-hub)
  - [14.3 Mapeo al roadmap del proyecto](#143-mapeo-al-roadmap-del-proyecto)
  - [14.4 Cómo vincular issues a releases](#144-cómo-vincular-issues-a-releases)
  - [14.5 Release notes automáticas desde Jira](#145-release-notes-automáticas-desde-jira)
  - [14.6 Tabla de mapeo completa SDD → Jira](#146-tabla-de-mapeo-completa-sdd--jira)
- [Sección 15 — Integración Jira con herramientas del equipo](#sección-15--integración-jira-con-herramientas-del-equipo)
  - [15.1 Jira + Git (GitHub/GitLab/Bitbucket)](#151-jira--git-githubgitlabbitbucket)
  - [15.2 Jira + CI/CD](#152-jira--cicd)
  - [15.3 Jira + Confluence](#153-jira--confluence)
  - [15.4 Jira + Slack/Teams](#154-jira--slackteams)
  - [15.5 Automatizaciones útiles](#155-automatizaciones-útiles)

### TÉCNICAS COMPLEMENTARIAS

- [Sección 16 — Técnicas Complementarias de Descomposición y Planificación Ágil](#sección-16--técnicas-complementarias-de-descomposición-y-planificación-ágil)
  - [16.1 Introducción](#161--introducción)
  - [16.2 Vertical Slicing](#162--vertical-slicing)
  - [16.3 Walking Skeleton](#163--walking-skeleton)
  - [16.4 Thin Slice / Tracer Bullet](#164--thin-slice--tracer-bullet)
  - [16.5 Story Mapping (Jeff Patton)](#165--story-mapping-jeff-patton)
  - [16.6 Example Mapping](#166--example-mapping)
  - [16.7 Spike / Architectural Spike](#167--spike--architectural-spike)
  - [16.8 ATDD (Acceptance Test-Driven Development)](#168--atdd-acceptance-test-driven-development)
  - [16.9 Mob Programming](#169--mob-programming)
  - [16.10 Shape Up (Basecamp)](#1610--shape-up-basecamp)
  - [16.11 Impact Mapping (Gojko Adzic)](#1611--impact-mapping-gojko-adzic)
  - [16.12 Dual-Track Agile](#1612--dual-track-agile)
  - [16.13 Tabla resumen: Cuándo usar cada técnica](#1613--tabla-resumen-cuándo-usar-cada-técnica)
- [Sección 17 — Guía práctica para arrancar un proyecto ágil](#sección-17--guía-práctica-para-arrancar-un-proyecto-ágil)
  - [17.1 Preguntas de diagnóstico inicial](#171--preguntas-de-diagnóstico-inicial)
  - [17.2 Criterios para elegir la metodología](#172--criterios-para-elegir-la-metodología)
  - [17.3 Checklist de arranque — Sprint 0](#173--checklist-de-arranque--sprint-0)
  - [17.4 Orden recomendado para construir los artefactos](#174--orden-recomendado-para-construir-los-artefactos)
  - [17.5 Errores frecuentes al iniciar un proyecto ágil](#175--errores-frecuentes-al-iniciar-un-proyecto-ágil)

---

## PARTE 1 — METODOLOGÍAS ÁGILES

---

### Sección 1 — Propósito del documento

#### 1.1 Para qué sirve este documento

Este documento es una guía integral de referencia sobre metodologías ágiles y gestión de tareas con Jira. Consolida los conceptos fundamentales de Scrum, Kanban, XP y frameworks de escala, junto con la operativa práctica de Jira como herramienta de tracking.

El objetivo es que un desarrollador o estudiante pueda consultar este documento como fuente única para entender:

- Qué son las metodologías ágiles y por qué existen
- Cómo funcionan Scrum y Kanban en la práctica
- Cómo se estructuran user stories, épicas y tareas técnicas
- Cómo se gestiona un proyecto ágil en Jira
- Cómo se relaciona todo esto con la documentación SDD del proyecto

#### 1.2 A quién está dirigido

- Desarrolladores que se incorporan a equipos ágiles por primera vez
- Estudiantes de ingeniería de software que necesitan una referencia práctica
- Miembros de equipo que necesitan entender la gestión de proyectos más allá del código

#### 1.3 Relación con la documentación SDD del proyecto

Este proyecto sigue el enfoque **Specification-Driven Development (SDD)**, donde la documentación guía el desarrollo a través de una cadena de trazabilidad:

```
Visión del Producto          →  docs/00_contexto/vision-producto_v1.0.md
    ↓
Necesidades de Negocio       →  docs/01_necesidades_negocio/necesidades-negocio_v1.0.md
    ↓
Especificación Funcional     →  docs/02_especificacion_funcional/especificacion-funcional_v1.0.md
    ↓
Arquitectura Técnica         →  docs/05_arquitectura_tecnica/arquitectura-solucion_v1.0.md
    ↓
Product Backlog (US)         →  docs/06_backlog-tecnico/product-backlog_v1.0.md
    ↓
Backlog Técnico (BT)         →  docs/06_backlog-tecnico/backlog-tecnico_v1.0.md
    ↓
Plan de Sprint               →  docs/07_plan-sprint/plan-iteracion_sprint-XX_v1.0.md
    ↓
Testing y Calidad            →  docs/08_calidad_y_pruebas/definition-of-done_v1.0.md
    ↓
DevOps y Deploy              →  docs/09_devops/
```

Cada nivel de esta cadena alimenta al siguiente. Las metodologías ágiles (este documento) gobiernan **cómo** se ejecuta el trabajo desde el Product Backlog hacia abajo: cómo se priorizan las historias, cómo se planifican los sprints, cómo se mide el progreso y cómo se mejora continuamente.

---

### Sección 2 — Manifiesto Ágil y principios fundamentales

#### 2.1 Los 4 valores del Manifiesto Ágil

El [Manifiesto Ágil](https://agilemanifesto.org/) fue firmado en 2001 por 17 profesionales del software. Establece 4 valores:

| # | Valoramos más... | ...sobre... |
|---|---|---|
| 1 | **Individuos e interacciones** | Procesos y herramientas |
| 2 | **Software funcionando** | Documentación exhaustiva |
| 3 | **Colaboración con el cliente** | Negociación contractual |
| 4 | **Respuesta ante el cambio** | Seguir un plan |

> **Nota importante:** "Valoramos más" no significa que los elementos de la derecha no importen. Significa que, ante un conflicto, priorizamos los de la izquierda. En el proyecto Motor DSL, por ejemplo, tenemos documentación extensa (SDD), pero siempre al servicio del software funcionando — la documentación guía la construcción, no la reemplaza.

#### 2.2 Los 12 principios ágiles

| # | Principio | Explicación práctica |
|---|---|---|
| 1 | Satisfacer al cliente mediante entrega temprana y continua de software con valor | No esperar 6 meses para entregar. Entregar incrementos cada sprint. |
| 2 | Aceptar cambios en los requisitos, incluso tarde en el desarrollo | Los cambios son oportunidades, no amenazas. El backlog es dinámico. |
| 3 | Entregar software funcionando frecuentemente (semanas, no meses) | Sprints de 1-4 semanas. Cada sprint produce un incremento usable. |
| 4 | Negocio y desarrolladores deben trabajar juntos diariamente | El Product Owner no desaparece entre sprints. Está disponible. |
| 5 | Construir proyectos en torno a individuos motivados | Dar autonomía al equipo. Confiar en sus decisiones técnicas. |
| 6 | La comunicación cara a cara es la más eficiente | Dailys, reviews, retros. No reemplazar todo con emails y tickets. |
| 7 | El software funcionando es la medida principal de progreso | No medir progreso por documentos escritos sino por features entregadas. |
| 8 | Desarrollo sostenible: mantener un ritmo constante indefinidamente | No hacer crunches. Si el equipo hace 18 SP por sprint, no comprometer 30. |
| 9 | Atención continua a la excelencia técnica y buen diseño | Refactoring, testing, code reviews. La deuda técnica se gestiona, no se ignora. |
| 10 | Simplicidad: maximizar el trabajo no realizado | YAGNI (You Aren't Gonna Need It). No construir features "por si acaso". |
| 11 | Las mejores arquitecturas emergen de equipos auto-organizados | El equipo decide cómo implementar, no un arquitecto aislado. |
| 12 | Reflexión regular sobre cómo ser más efectivo | Las retrospectivas no son opcionales. El proceso mejora sprint a sprint. |

#### 2.3 Contexto histórico: por qué surgió

Antes del manifiesto ágil, el modelo dominante era **Waterfall** (cascada), popularizado por Winston Royce en 1970. En cascada, el proyecto avanza en fases secuenciales rígidas: Requisitos → Diseño → Implementación → Verificación → Mantenimiento. Cada fase debe completarse antes de iniciar la siguiente.

Los problemas de waterfall en software:

- El cliente no ve el producto hasta el final (meses o años después)
- Los cambios de requisitos son costosos y resisten
- Los bugs se descubren tarde, cuando corregirlos es caro
- El equipo no recibe feedback hasta que es demasiado tarde

El manifiesto ágil surgió como respuesta a estos problemas, proponiendo un enfoque iterativo e incremental.

#### 2.4 Tabla comparativa: Waterfall vs. Ágil

| Aspecto | Waterfall | Ágil |
|---|---|---|
| **Planificación** | Completa y detallada al inicio (Big Design Up Front) | Progresiva: planificación detallada solo para el sprint actual |
| **Entrega** | Una sola entrega al final del proyecto | Entregas incrementales cada 1-4 semanas |
| **Feedback** | Al final, cuando el producto está "terminado" | Continuo: cada sprint review genera feedback |
| **Gestión de cambios** | Resistidos; requieren proceso formal de change request | Bienvenidos; el backlog se reprioriza continuamente |
| **Documentación** | Exhaustiva y previa al código | Suficiente y al servicio del desarrollo (como en SDD) |
| **Riesgo** | Concentrado al final (integración y testing tardío) | Distribuido: cada sprint integra, prueba y entrega |
| **Roles** | Rígidos y jerárquicos (Project Manager decide todo) | Colaborativos y auto-organizados (PO, SM, Dev Team) |
| **Testing** | Fase separada al final | Continuo durante todo el desarrollo (TDD, CI) |
| **Éxito se mide por** | Cumplimiento del plan original | Valor entregado al cliente |

---

### Sección 3 — Scrum

#### 3.1 Definición y origen

**Scrum** es un framework ágil para desarrollar, entregar y mantener productos complejos. Fue formalizado por Ken Schwaber y Jeff Sutherland, y su referencia canónica es la [Scrum Guide 2020](https://scrumguides.org/).

Scrum no es una metodología prescriptiva — es un framework ligero que define roles, eventos y artefactos mínimos. El equipo decide cómo trabajar dentro de ese marco.

#### 3.2 Los 3 pilares

| Pilar | Definición | Ejemplo práctico |
|---|---|---|
| **Transparencia** | El proceso y el trabajo deben ser visibles para todos | El board de Jira está visible. La velocity se registra en `velocidad-equipo_v1.0.md`. Todos ven el estado real. |
| **Inspección** | Los artefactos y el progreso se inspeccionan frecuentemente | Cada daily se revisa el board. Cada review se inspecciona el incremento. Cada retro se inspecciona el proceso. |
| **Adaptación** | Cuando algo se desvía, se ajusta lo antes posible | Si en la daily se detecta un bloqueo, se actúa hoy, no al final del sprint. Si la velocity baja, se reduce el compromiso del próximo sprint. |

#### 3.3 Los 5 valores

| Valor | Significado | Anti-ejemplo |
|---|---|---|
| **Compromiso** | El equipo se compromete con los objetivos del sprint | No comprometerse a 40 SP cuando la velocity es 18 |
| **Coraje** | Tener la valentía de hacer lo correcto y trabajar en problemas difíciles | Decir "esta historia no cumple el DoR" aunque el PO presione |
| **Foco** | Concentrarse en el trabajo del sprint y los objetivos del equipo | No meter historias al sprint a mitad de camino sin justificación |
| **Apertura** | Ser transparente sobre el trabajo y los desafíos | Reportar impedimentos en la daily, no ocultarlos |
| **Respeto** | Respetar que cada persona es capaz y actúa con buena intención | No culpar al compañero si un spike no dio el resultado esperado |

#### 3.4 Roles

##### Tabla comparativa de roles

| Aspecto | Product Owner | Scrum Master | Development Team |
|---|---|---|---|
| **Responsabilidad** | Maximizar el valor del producto | Facilitar el proceso Scrum | Construir el incremento |
| **Gestiona** | Product Backlog, priorización | Impedimentos, ceremonias | Trabajo técnico del sprint |
| **Decide** | Qué se construye y en qué orden | Cómo se facilita el proceso | Cómo se implementa técnicamente |
| **NO hace** | No decide el cómo técnico | No asigna tareas ni gestiona personas | No decide prioridad de negocio |
| **Reporta a** | Stakeholders / Negocio | Nadie (es un líder servicial) | Se auto-organiza |
| **Anti-patrón** | PO ausente o proxy sin poder de decisión | SM que asigna tareas como Project Manager | Equipo que espera que le digan qué hacer |

##### Product Owner (PO)

**Responsabilidades:**
- Gestionar y priorizar el Product Backlog
- Definir los criterios de aceptación de cada historia
- Tomar decisiones de negocio sobre qué se construye
- Aceptar o rechazar el incremento en la Sprint Review
- Maximizar el valor del trabajo del equipo

**Qué NO hace:**
- No decide la arquitectura ni las decisiones técnicas
- No asigna tareas a personas específicas
- No cambia el scope del sprint a mitad de camino (sin negociación)

**Anti-patrones comunes:**
- *El PO proxy:* alguien que "representa" al PO pero no tiene poder de decisión real
- *El PO ausente:* aparece solo en el planning y desaparece hasta la review
- *El PO micromanager:* decide el cómo técnico además del qué

##### Scrum Master (SM)

**Responsabilidades:**
- Facilitar las ceremonias Scrum
- Remover impedimentos del equipo
- Proteger al equipo de interrupciones externas
- Coaching en prácticas ágiles
- Asegurar que se respeten los acuerdos (DoR, DoD, WIP limits)

**Qué NO hace:**
- No es un Project Manager — no asigna tareas ni controla horarios
- No es el jefe del equipo — es un líder servicial
- No decide qué se construye — eso es responsabilidad del PO

**Anti-patrones comunes:**
- *El SM secretario:* solo agenda reuniones y toma notas
- *El SM policía:* controla y vigila en vez de facilitar
- *El SM ausente:* no remueve impedimentos activamente

##### Development Team

**Responsabilidades:**
- Auto-organizarse para entregar el incremento
- Estimar las historias de usuario
- Descomponer historias en tareas técnicas
- Cumplir el Definition of Done
- Participar activamente en todas las ceremonias

**Qué NO hace:**
- No decide la prioridad del backlog — eso lo hace el PO
- No reporta a un líder técnico — se auto-organiza
- No trabaja en historias fuera del sprint sin acordarlo

**Anti-patrones comunes:**
- *El equipo silencioso:* no habla en las dailies ni en las retros
- *El héroe solitario:* una persona hace todo, el resto espera
- *El equipo fragmentado:* cada uno trabaja en silos sin comunicarse

#### 3.5 Artefactos

##### Product Backlog

**Definición:** Lista priorizada de todo lo que se necesita en el producto. Es el único origen de requisitos para el trabajo del equipo.

**Quién lo gestiona:** El Product Owner, con input del equipo y stakeholders.

**En este proyecto:**  
El Product Backlog está documentado en `../docs/06_backlog-tecnico/product-backlog_v1.0.md` con 31 User Stories clasificadas por prioridad MoSCoW:

| Prioridad | Descripción | Ejemplo del proyecto |
|---|---|---|
| Must Have | Imprescindible para el MVP | US-03: Parser DSL básico, US-09: Renderizar ESC/POS |
| Should Have | Importante pero no bloqueante | US-10: Renderizar texto plano, US-16: Adaptar al perfil del dispositivo |
| Could Have | Deseable si hay capacidad | US-27: Renderizar a PDF |
| Won't Have (v1.0) | Fuera de alcance para esta versión | Editor visual, scripting avanzado |

##### Sprint Backlog

**Definición:** Subconjunto del Product Backlog que el equipo se compromete a entregar durante el sprint actual, más el Sprint Goal y el plan para lograrlo.

**Quién lo gestiona:** El Development Team.

**En este proyecto:**  
Cada sprint tiene su plan documentado en `../docs/07_plan-sprint/plan-iteracion_sprint-XX_v1.0.md`. Por ejemplo, el Sprint 01 compromete US-01 a US-04 con un total de 48 SP y el objetivo: *"Implementar el MVP técnico del núcleo del motor, permitiendo crear la estructura base, definir el AST e implementar el parser DSL básico."*

##### Incremento

**Definición:** La suma de todos los ítems del Product Backlog completados durante el sprint, más los incrementos de sprints anteriores. Cada incremento debe ser usable y cumplir el Definition of Done.

**Quién lo valida:** El Product Owner durante el Sprint Review.

**En este proyecto:**
- El Definition of Ready está en `../docs/06_backlog-tecnico/definition-of-ready_v1.0.md`
- El Definition of Done está en `../docs/08_calidad_y_pruebas/definition-of-done_v1.0.md`

El DoD incluye criterios de código (compila, sin warnings, PR aprobado), testing (unitarios, integración, cobertura ≥ 70%, CI verde), calidad (SLAs de performance) y documentación.

#### 3.6 Eventos / Ceremonias

| Evento | Propósito | Timebox | Participantes | Entradas | Salidas |
|---|---|---|---|---|---|
| **Sprint Planning** | Planificar qué se va a construir en el sprint y cómo | 2-4 horas (sprint de 2 semanas) | PO, SM, Dev Team | Product Backlog priorizado, velocity histórica, DoR | Sprint Backlog, Sprint Goal |
| **Daily Scrum** | Sincronizar al equipo y detectar impedimentos | 15 minutos | Dev Team (SM facilita) | Estado del sprint board | Impedimentos identificados, plan del día |
| **Sprint Review** | Inspeccionar el incremento y obtener feedback | 1-2 horas | PO, SM, Dev Team, Stakeholders | Incremento completado | Feedback, ajustes al backlog |
| **Sprint Retrospective** | Reflexionar sobre el proceso y definir mejoras | 1-1.5 horas | SM, Dev Team (PO opcional) | Observaciones del sprint | Acciones de mejora con responsable |
| **Refinamiento (Grooming)** | Detallar, estimar y descomponer historias futuras | 1-2 horas (no es evento formal de Scrum) | PO, Dev Team, SM | Historias candidatas del backlog | Historias refinadas con DoR cumplido |

**Templates del proyecto para ceremonias:**
- Sprint Review → `../docs/07_plan-sprint/template-sprint-review_v1.0.md`
- Sprint Retrospectiva → `../docs/07_plan-sprint/template-sprint-retrospectiva_v1.0.md`

#### 3.7 El Sprint

**Definición:** Un período fijo de tiempo (timebox) durante el cual se crea un incremento de producto "Done", usable y potencialmente entregable.

**Duración típica:** 1 a 4 semanas. En este proyecto se usan sprints de **2 semanas**.

**Qué pasa durante un sprint:**

1. **Sprint Planning** (día 1): Se define el Sprint Goal, se seleccionan historias del backlog y se descomponen en tareas
2. **Desarrollo diario** (días 1-10): El equipo trabaja en las historias. Cada día hay una Daily Scrum de 15 minutos
3. **Refinamiento** (durante el sprint): Se detallan historias para el próximo sprint
4. **Sprint Review** (último día): Se demuestra el incremento al PO y stakeholders
5. **Sprint Retrospective** (después de la review): El equipo reflexiona sobre el proceso

**Reglas del sprint:**
- El Sprint Goal no cambia a mitad del sprint
- No se agregan historias al sprint sin negociación (el PO puede negociar cambio de scope)
- Si el Sprint Goal se vuelve obsoleto, el PO puede cancelar el sprint (caso extremo)
- La calidad no se reduce — el DoD se respeta siempre

#### 3.8 Diagrama del ciclo Scrum

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          CICLO SCRUM                                    │
│                                                                         │
│   ┌──────────────┐                                                      │
│   │   PRODUCT     │◄──── Repriorización continua                        │
│   │   BACKLOG     │      (basada en feedback                            │
│   │               │       de Reviews y Retros)                          │
│   │  ┌─────────┐  │                                                     │
│   │  │ US-01   │  │    ┌──────────────────────────────────────────┐     │
│   │  │ US-02   │──┼───►│         SPRINT (2 semanas)                │     │
│   │  │ US-03   │  │    │                                          │     │
│   │  │ US-04   │  │    │  Sprint    Daily     Desarrollo          │     │
│   │  │ ...     │  │    │  Planning  Scrum     continuo            │     │
│   │  └─────────┘  │    │  (Día 1)  (15 min)  (Días 1-10)         │     │
│   └──────────────┘    │     │        │            │               │     │
│                        │     ▼        ▼            ▼               │     │
│                        │  ┌─────────────────────────────┐         │     │
│                        │  │    Sprint     Sprint Goal    │         │     │
│                        │  │    Backlog    + Tareas       │         │     │
│                        │  └─────────────────────────────┘         │     │
│                        │                    │                      │     │
│                        │                    ▼                      │     │
│                        │           ┌──────────────┐               │     │
│                        │           │ INCREMENTO   │               │     │
│                        │           │ (Done, usable │               │     │
│                        │           │  y entregable)│               │     │
│                        │           └──────┬───────┘               │     │
│                        └─────────────────┼────────────────────────┘     │
│                                          │                              │
│                                          ▼                              │
│                        ┌──────────────────────────────────┐             │
│                        │  Sprint Review                    │             │
│                        │  → Demo al PO y Stakeholders      │             │
│                        │  → Feedback sobre el incremento   │             │
│                        └──────────────┬───────────────────┘             │
│                                       │                                 │
│                                       ▼                                 │
│                        ┌──────────────────────────────────┐             │
│                        │  Sprint Retrospective             │             │
│                        │  → ¿Qué salió bien?               │             │
│                        │  → ¿Qué salió mal?                │             │
│                        │  → Acciones de mejora              │             │
│                        └──────────────────────────────────┘             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### Sección 4 — Kanban

#### 4.1 Definición y origen

**Kanban** (看板, "tablero visual" en japonés) es un método de gestión del flujo de trabajo que se originó en el sistema de producción de Toyota en la década de 1940. Fue adaptado al desarrollo de software por David J. Anderson en 2007.

A diferencia de Scrum, Kanban no tiene sprints, roles obligatorios ni ceremonias fijas. Se enfoca en **visualizar el flujo de trabajo** y **optimizarlo continuamente**.

#### 4.2 Principios de Kanban

| # | Principio | Explicación |
|---|---|---|
| 1 | **Visualizar el trabajo** | Todo el trabajo en progreso debe ser visible en un tablero. Si no está en el board, no existe. |
| 2 | **Limitar el Work In Progress (WIP)** | Definir límites máximos de ítems por columna. Menos WIP = más foco = menos context switching. |
| 3 | **Gestionar el flujo** | Observar y optimizar cómo fluyen los ítems a través del tablero. Detectar cuellos de botella. |
| 4 | **Hacer políticas explícitas** | Documentar las reglas: qué significa cada columna, cuándo se mueve un ítem, quién puede hacerlo. |
| 5 | **Implementar feedback loops** | Reuniones regulares para inspeccionar el sistema (standups, service delivery reviews). |
| 6 | **Mejorar colaborativamente, evolucionar experimentalmente** | Cambios incrementales basados en datos, no revoluciones. |

#### 4.3 Tablero Kanban

Un tablero Kanban típico con WIP limits:

```
┌──────────────┬──────────────┬──────────────┬──────────────┬──────────────┐
│   BACKLOG    │ IN PROGRESS  │  IN REVIEW   │     QA       │    DONE      │
│   (sin WIP)  │   WIP: 3     │   WIP: 2     │   WIP: 2     │  (sin WIP)   │
├──────────────┼──────────────┼──────────────┼──────────────┼──────────────┤
│  ┌────────┐  │  ┌────────┐  │  ┌────────┐  │  ┌────────┐  │  ┌────────┐  │
│  │ Item 7 │  │  │ Item 4 │  │  │ Item 2 │  │  │ Item 1 │  │  │ Item A │  │
│  ├────────┤  │  ├────────┤  │  └────────┘  │  └────────┘  │  ├────────┤  │
│  │ Item 8 │  │  │ Item 5 │  │              │              │  │ Item B │  │
│  ├────────┤  │  ├────────┤  │              │              │  ├────────┤  │
│  │ Item 9 │  │  │ Item 6 │  │              │              │  │ Item C │  │
│  ├────────┤  │  └────────┘  │              │              │  └────────┘  │
│  │ Item 10│  │              │              │              │              │
│  └────────┘  │              │              │              │              │
└──────────────┴──────────────┴──────────────┴──────────────┴──────────────┘
```

**Políticas de columna (ejemplo):**
- **Backlog:** Cualquier ítem priorizado por el PO o líder técnico
- **In Progress:** El desarrollador se asigna el ítem. Máximo 1 ítem por persona
- **In Review:** PR creado y asignado a reviewer. Máximo 24h en esta columna
- **QA:** Testing manual o automatizado. Si falla, regresa a In Progress
- **Done:** Cumple el Definition of Done. Desplegado o listo para deploy

#### 4.4 Métricas Kanban

| Métrica | Definición | Para qué sirve |
|---|---|---|
| **Lead Time** | Tiempo total desde que se solicita un ítem hasta que se entrega | Mide el tiempo de respuesta al cliente |
| **Cycle Time** | Tiempo desde que se empieza a trabajar un ítem hasta que se termina | Mide la eficiencia del equipo |
| **Throughput** | Cantidad de ítems completados por unidad de tiempo | Mide la capacidad de entrega |
| **Cumulative Flow Diagram (CFD)** | Gráfico de área que muestra ítems en cada estado a lo largo del tiempo | Detecta cuellos de botella y tendencias |

**Relación entre métricas:**
- Si Lead Time >> Cycle Time → los ítems esperan mucho antes de empezar (problema de priorización o capacidad)
- Si throughput baja → verificar WIP limits y cuellos de botella en el CFD
- Si una banda del CFD se ensancha → cuello de botella en esa columna

#### 4.5 Tabla comparativa: Scrum vs. Kanban

| Aspecto | Scrum | Kanban |
|---|---|---|
| **Cadencia** | Sprints fijos (1-4 semanas) | Flujo continuo, sin iteraciones fijas |
| **Roles** | PO, SM, Dev Team (obligatorios) | Sin roles prescriptos |
| **Ceremonias** | Planning, Daily, Review, Retro | Opcionales (standups, service reviews) |
| **Planificación** | Sprint Planning al inicio de cada sprint | Just-in-time: se toma el siguiente ítem del backlog |
| **Compromiso** | Sprint Backlog comprometido | Sin compromiso fijo; se gestiona por WIP limits |
| **Métricas** | Velocity, Burndown | Lead Time, Cycle Time, Throughput, CFD |
| **Cambios** | No se cambia el Sprint Backlog (salvo negociación) | Los ítems pueden cambiar en cualquier momento |
| **Estimación** | Story Points (Planning Poker) | Opcional; se puede medir por throughput |
| **Cuándo usar** | Proyectos con entregas regulares, equipos estables | Soporte, ops, trabajo con demanda variable |

#### 4.6 Cuándo elegir Kanban sobre Scrum

- **Equipos de soporte/ops** con demanda continua e impredecible
- **Mantenimiento de software** donde los bugs llegan todo el tiempo
- **Equipos con flujo continuo** que no se benefician de iteraciones fijas
- **Equipos en transición** que quieren empezar con algo más simple que Scrum
- **Trabajo de DevOps/SRE** donde el timebox de sprint no tiene sentido

---

### Sección 5 — Otras metodologías ágiles (resumen)

#### 5.1 XP (Extreme Programming)

**Creador:** Kent Beck (1996). XP es la metodología ágil más prescriptiva en prácticas técnicas.

**Prácticas clave:**

| Práctica | Descripción |
|---|---|
| **Pair Programming** | Dos programadores trabajan juntos en la misma estación. Uno escribe código, el otro revisa en tiempo real. |
| **TDD (Test-Driven Development)** | Escribir el test antes que el código. Ciclo: Red → Green → Refactor. |
| **Continuous Integration** | Integrar código al repositorio compartido múltiples veces al día. |
| **Refactoring** | Mejorar la estructura del código sin cambiar su comportamiento. Continuamente. |
| **Collective Ownership** | Todo el equipo es dueño de todo el código. Cualquiera puede modificar cualquier parte. |
| **Simple Design** | La solución más simple que funcione. No anticipar necesidades futuras (YAGNI). |
| **Releases pequeñas** | Entregas frecuentes y pequeñas en vez de releases grandes. |
| **Estándares de código** | Convenciones compartidas para que el código sea legible por todos. |

#### 5.2 SAFe (Scaled Agile Framework)

**Cuándo aplica:** Organizaciones con 3 o más equipos ágiles que necesitan coordinarse para entregar un producto o portfolio.

**Conceptos clave:**

| Concepto | Descripción |
|---|---|
| **PI Planning** | Program Increment Planning. Evento de 2 días donde todos los equipos sincronizan objetivos para las próximas 8-12 semanas. |
| **ART (Agile Release Train)** | Equipo de equipos (5-12 equipos) que trabajan en un mismo flujo de valor. |
| **Release Train** | Cadencia fija de entregas alineada con el PI. |
| **RTE (Release Train Engineer)** | El Scrum Master del ART — facilita la coordinación entre equipos. |

#### 5.3 LeSS (Large-Scale Scrum)

**Cuándo aplica:** Organizaciones que quieren escalar Scrum de forma minimalista (2-8 equipos trabajando en el mismo producto).

**Diferencia con SAFe:**
- LeSS es Scrum aplicado a escala, sin agregar capas de gestión
- SAFe agrega roles, ceremonias y artefactos adicionales
- LeSS tiene un solo Product Backlog y un solo PO para todos los equipos
- SAFe permite múltiples backlogs coordinados

#### 5.4 Scrumban

**Qué es:** Combinación de Scrum + Kanban. Mantiene la estructura de sprints de Scrum pero incorpora WIP limits y flujo continuo de Kanban.

**Cuándo usarlo:**
- Equipos Scrum que quieren mejorar su flujo sin abandonar sprints
- Equipos que necesitan más flexibilidad que Scrum puro pero más estructura que Kanban
- Transición gradual de Scrum a Kanban (o viceversa)

#### 5.5 Tabla comparativa rápida

| Aspecto | XP | Scrum | Kanban | SAFe | LeSS |
|---|---|---|---|---|---|
| **Escala** | 1 equipo | 1 equipo | 1 equipo | Multi-equipo (3-125+) | Multi-equipo (2-8) |
| **Foco** | Prácticas técnicas | Proceso y ceremonias | Flujo de trabajo | Coordinación organizacional | Scrum escalado minimalista |
| **Iteraciones** | 1-2 semanas | 1-4 semanas | Sin iteraciones | PI de 8-12 semanas | Sprints Scrum |
| **Roles** | Coach, Tracker, Customer | PO, SM, Dev Team | Sin roles fijos | Muchos (RTE, Product Mgmt, etc.) | PO, SM, equipos |
| **Prescripción** | Alta (prácticas técnicas) | Media (proceso) | Baja (principios) | Muy alta (framework completo) | Baja (Scrum + reglas) |
| **Ideal para** | Equipos técnicos maduros | Mayoría de equipos de desarrollo | Soporte, ops, mantenimiento | Grandes organizaciones | Organizaciones medianas |

---

### Sección 6 — User Stories, Épicas y Tareas

#### 6.0 Introducción — Conceptos fundamentales y vocabulario

Antes de entrar en formatos y técnicas, es necesario definir con precisión los términos que se usan cotidianamente en la gestión ágil. La confusión entre estos conceptos es una de las causas más frecuentes de backlogs desordenados, sprints mal planificados e historias que no se pueden cerrar.

##### Definiciones clave

| Término | Definición | Quién la escribe | Scope temporal | Ejemplo rápido |
|---|---|---|---|---|
| **Iniciativa** | Objetivo estratégico de alto nivel que agrupa múltiples épicas. Responde al "por qué" del trabajo. | Liderazgo / Gerencia de producto | Trimestral o anual | "Aumentar la conversión de ventas online en un 25%" |
| **Épica** | Funcionalidad grande que NO cabe en un solo sprint y se descompone en múltiples user stories. Representa una capacidad completa del producto. | Product Owner con input del equipo | Multi-sprint (2 a 8 sprints) | "Checkout de pagos" / "Motor de rendering ESC/POS" |
| **User Story (Historia de usuario)** | Descripción corta de una funcionalidad **desde la perspectiva del usuario** que entrega valor y se puede completar en un solo sprint. Es la unidad de planificación en Scrum. | Product Owner (redacta) + equipo (refina) | 1 sprint | "Como comprador, quiero pagar con tarjeta de crédito, para completar mi compra sin efectivo" |
| **Tarea técnica** | Trabajo de implementación necesario para completar una user story. No tiene valor de negocio por sí sola — solo tiene valor como parte de una historia. | Equipo de desarrollo | Horas a 2-3 días | "Implementar endpoint POST /api/payments" / "Escribir tests unitarios del parser" |
| **Sub-tarea** | División de una tarea en pasos más pequeños para tracking interno del equipo. Típicamente asignada a una sola persona. | Desarrollador individual | Horas | "Crear la migración de base de datos" / "Configurar el mock del servicio de pagos" |
| **Bug (Defecto)** | Comportamiento actual del sistema que difiere del comportamiento esperado definido en los criterios de aceptación. No es una feature nueva — es una corrección. | QA / cualquier miembro del equipo | Variable | "El parser falla con caracteres Unicode" / "El botón de pago no responde en Safari" |
| **Spike** | Timebox de investigación para reducir incertidumbre. No produce código productivo — produce conocimiento (ADR, PoC, informe). | Equipo de desarrollo | 1-3 días (timeboxed) | "Evaluar viabilidad de rendering PDF nativo en .NET" |

##### Reglas rápidas de redacción por tipo

**Para User Stories:**
- Siempre en formato: `Como [rol], quiero [acción], para [beneficio]`
- El **rol** es quién usa la funcionalidad (no "como desarrollador" si es una feature de usuario final)
- La **acción** es qué quiere hacer, en voz activa
- El **beneficio** es el valor de negocio — por qué importa
- Debe tener al menos 2 criterios de aceptación en formato Given/When/Then
- Si no cumple INVEST (ver 6.2), no está lista para el sprint

**Para Épicas:**
- Título descriptivo que indique la capacidad completa: "Gestión de pagos", no "Pagos"
- Incluir: objetivo, user stories hijas identificadas, criterio de éxito medible
- Si una épica tiene más de 8-10 historias, considerar dividirla en 2 épicas

**Para Tareas técnicas:**
- Título con verbo de acción: "Implementar", "Configurar", "Crear", "Migrar"
- Siempre vinculada a una user story (nunca huérfana)
- Estimación en horas (no en story points) — las tareas son trabajo concreto
- Si una tarea tarda más de 2 días, descomponerla en sub-tareas

**Para Bugs:**
- Formato: "[Componente] Descripción del comportamiento incorrecto"
- Incluir siempre: pasos para reproducir, comportamiento actual, comportamiento esperado
- Severidad (Blocker / Critical / Major / Minor / Trivial) y prioridad no son lo mismo

##### Ejemplos en contexto web (e-commerce)

| Tipo | Ejemplo en e-commerce | Ejemplo en app mobile (delivery) | Ejemplo en SaaS (CRM) |
|---|---|---|---|
| **Iniciativa** | Aumentar conversión de checkout en 25% | Reducir tiempo de entrega promedio en 15 min | Mejorar captación de leads en 30% |
| **Épica** | Checkout multi-paso con pasarela de pagos | Tracking en tiempo real de pedidos | Formularios dinámicos de captura |
| **User Story** | Como comprador, quiero guardar mi tarjeta para futuras compras, para no reingresarla cada vez | Como repartidor, quiero ver la ruta optimizada, para llegar más rápido | Como vendedor, quiero filtrar leads por industria, para priorizar mi pipeline |
| **Tarea técnica** | Integrar API de Stripe para tokenización de tarjetas | Implementar conexión con Google Maps Directions API | Crear componente de filtro dinámico con React |
| **Sub-tarea** | Crear migración para tabla `saved_cards` | Escribir tests de integración del servicio de rutas | Agregar índice a columna `industry` en la BD |
| **Bug** | [Checkout] Doble cobro cuando el usuario hace doble clic en "Pagar" | [Mapa] La ruta no se actualiza al recalcular | [Filtros] El filtro por fecha no respeta timezone del usuario |

---

#### 6.1 Formato estándar de User Story

> **Definición:** Una **User Story** (historia de usuario) es una descripción breve e informal de una funcionalidad descrita desde la perspectiva de la persona que la necesita. No es un requerimiento técnico — es una **promesa de conversación** entre el PO y el equipo sobre qué valor se va a entregar. El concepto fue introducido por Kent Beck en Extreme Programming (XP) y popularizado por Mike Cohn en *User Stories Applied* (2004).

El formato estándar de redacción es:

```
Como [rol], quiero [acción], para [beneficio]
```

**Ejemplo del proyecto:**
> Como desarrollador, quiero parsear templates DSL JSON, para alimentar el AST del motor  
> — US-05 del `../docs/06_backlog-tecnico/product-backlog_v1.0.md`

El formato establece:
- **Quién** necesita la funcionalidad (rol)
- **Qué** necesita hacer (acción)
- **Por qué** lo necesita (beneficio / valor de negocio)

**Ejemplos en contexto web:**

| Dominio | User Story |
|---|---|
| E-commerce | Como comprador, quiero filtrar productos por rango de precio, para encontrar artículos dentro de mi presupuesto |
| App mobile | Como repartidor, quiero escanear el código QR del paquete, para confirmar la entrega sin tipear datos |
| SaaS (CRM) | Como gerente de ventas, quiero ver un dashboard con el pipeline por etapa, para identificar cuellos de botella |
| Red social | Como usuario, quiero silenciar notificaciones de un grupo, para no recibir alertas irrelevantes |

**Anti-patrones de redacción (historias mal escritas):**

| ❌ Mal escrita | Problema | ✅ Corregida |
|---|---|---|
| "Implementar API REST de productos" | No hay rol ni beneficio — es una tarea técnica disfrazada de historia | "Como comprador, quiero buscar productos por nombre, para encontrar rápidamente lo que necesito" |
| "Como usuario, quiero que el sistema sea rápido" | No es testeable — ¿qué significa "rápido"? | "Como usuario, quiero que la búsqueda devuelva resultados en menos de 2 segundos, para no abandonar la página" |
| "Como PO, quiero que se use React" | El PO no decide tecnología — esto es una restricción técnica, no una historia | "Como usuario, quiero una interfaz responsiva que funcione en móvil y desktop, para acceder desde cualquier dispositivo" |
| "Como desarrollador, quiero refactorizar el módulo de pagos" | Refactoring es tarea técnica, no tiene valor de negocio directo | Registrar como tarea técnica vinculada a una US, no como US |

**¿Cuándo NO usar el formato "Como/quiero/para"?**
- **Tareas técnicas:** usar formato directo → "Configurar pipeline de CI/CD en GitHub Actions"
- **Spikes:** usar formato de pregunta → "¿Es viable renderizar PDF con la librería X?"
- **Bugs:** usar formato de reporte → "[Parser] Error al procesar caracteres Unicode en TextNode"
- **Tareas de infraestructura:** formato directo → "Crear base de datos PostgreSQL en el entorno de staging"

#### 6.2 Criterios INVEST

> **Definición:** **INVEST** es un acrónimo creado por Bill Wake (2003) que define los 6 atributos que toda User Story bien escrita debe cumplir. Si una historia no cumple alguno de estos criterios, probablemente necesita re-escribirse o descomponerse antes de entrar al sprint. Los criterios INVEST funcionan como un **checklist de calidad** durante el refinamiento del backlog.

Una buena User Story cumple los criterios INVEST:

| Criterio | Significado | Ejemplo ✅ | Ejemplo ❌ |
|---|---|---|---|
| **I**ndependiente | No depende de otras US para entregarse | US-03: Parser DSL puede entregarse sin rendering | "Implementar UI" que necesita backend primero |
| **N**egociable | Los detalles se negocian con el PO | El formato del DSL puede ajustarse durante el sprint | "Debe usar exactamente esta librería" |
| **V**aliosa | Aporta valor medible al usuario o negocio | "Renderizar ESC/POS para imprimir tickets" | "Refactorizar clase interna" (es tarea técnica) |
| **E**stimable | El equipo puede estimar su esfuerzo | "Parser DSL básico" → 13 SP | "Mejorar el sistema" → ¿cuánto es eso? |
| **S**mall | Lo suficientemente pequeña para un sprint | US-09: Renderizar ESC/POS (1 sprint) | "Implementar todo el motor" (épica entera) |
| **T**estable | Se puede verificar si está terminada | "Given DSL input, When parse, Then AST válido" | "El sistema debe ser rápido" |

#### 6.3 Criterios de aceptación: formato Given/When/Then

> **Definición:** Un **criterio de aceptación** es una condición que debe cumplirse para que una User Story se considere terminada. Define el **comportamiento esperado** del sistema en términos concretos y verificables. Sin criterios de aceptación, no hay forma objetiva de saber si la historia está completa.

> **Definición:** **BDD (Behavior-Driven Development)** es una práctica de desarrollo que extiende TDD para escribir tests en un lenguaje cercano al negocio. Fue creado por Dan North (2006). El formato estándar de BDD es **Given/When/Then**, también conocido como **Gherkin** (el lenguaje que usa la herramienta Cucumber para automatizar estos tests).

Los criterios de aceptación se escriben en formato BDD:

```
Given [precondición / contexto]     → el estado inicial del sistema
When  [acción que ejecuta el usuario o el sistema]  → el evento que ocurre
Then  [resultado esperado observable]   → lo que debe pasar como consecuencia
```

**Ejemplo del proyecto (US-03: Parser DSL básico):**

```
Given una plantilla DSL JSON válida con un TextNode
When  el parser procesa la plantilla
Then  se genera un AST con un DocumentNode raíz y un TextNode hijo

Given una plantilla DSL JSON con sintaxis inválida
When  el parser intenta procesarla
Then  se lanza una excepción descriptiva con la ubicación del error
```

**Ejemplos en contexto web:**

*E-commerce — Historia: "Como comprador, quiero agregar productos al carrito"*
```
Given que estoy viendo la página de detalle de un producto con stock disponible
When  hago clic en el botón "Agregar al carrito"
Then  el producto aparece en mi carrito con cantidad 1
And   el ícono del carrito muestra el total actualizado

Given que ya tengo el producto X en el carrito con cantidad 2
When  hago clic en "Agregar al carrito" para el mismo producto X
Then  la cantidad del producto X en el carrito se incrementa a 3
And   no se crea una línea duplicada
```

*App mobile — Historia: "Como repartidor, quiero confirmar la entrega"*
```
Given que llegué a la dirección del cliente y tengo el paquete
When  escaneo el código QR del paquete
Then  la entrega se marca como "completada" en el sistema
And   el cliente recibe una notificación push de confirmación
And   se registra la hora y ubicación GPS de la entrega
```

**¿Cuántos criterios de aceptación debería tener una historia?**
- **Mínimo 2:** un escenario de éxito (happy path) y uno de error/borde
- **Recomendado 3-5:** cubren los casos más relevantes sin sobrecargar
- **Máximo 7-8:** si tiene más, la historia probablemente es demasiado grande y debería descomponerse
- **Regla práctica:** si los criterios no caben en una tarjeta (o una pantalla), la historia es demasiado compleja

#### 6.4 Jerarquía: Iniciativa → Épica → User Story → Sub-tarea → Bug

Los ítems de trabajo en un proyecto ágil se organizan en una **jerarquía de descomposición** que va desde lo estratégico (por qué hacemos esto) hasta lo operativo (qué código escribo hoy). Entender esta jerarquía es fundamental para mantener la trazabilidad entre los objetivos de negocio y el trabajo diario.

**Definiciones formales de cada nivel:**

| Nivel | Definición | Criterio para distinguirlo |
|---|---|---|
| **Iniciativa** | Objetivo estratégico de alto nivel que agrupa épicas relacionadas. Define el "para qué" del esfuerzo. | Si responde a un OKR o meta de negocio → es una iniciativa |
| **Épica** | Cuerpo de trabajo grande que entrega una **capacidad completa** del producto. No cabe en un sprint. | Si tarda más de 1 sprint → es épica. Si tiene más de 8 historias → considerar dividirla |
| **User Story** | Funcionalidad concreta que entrega **valor al usuario** y se completa en 1 sprint. | Si se puede demostrar en la Sprint Review → es historia. Si no → es tarea o épica |
| **Tarea técnica** | Trabajo de implementación **sin valor de negocio directo**, necesario para completar una historia. | Si el PO no la entiende ni le importa → es tarea técnica. Siempre vinculada a una US |
| **Sub-tarea** | División de una tarea para asignación y tracking individual. | Si una sola persona la puede hacer en horas → es sub-tarea |
| **Bug** | Diferencia entre el comportamiento **actual** y el **esperado** (según criterios de aceptación). | Si funcionaba y dejó de funcionar → bug. Si nunca funcionó así → es nueva funcionalidad |

```
Iniciativa (Tema estratégico)
    └── Épica (funcionalidad grande, multi-sprint)
            └── User Story (valor entregable en un sprint)
                    └── Sub-tarea / Tarea técnica (trabajo implementable)
                            └── Bug (defecto encontrado)
```

**Ejemplo concreto del proyecto Motor DSL:**

| Nivel | ID | Descripción |
|---|---|---|
| **Iniciativa** | NB-01 | Desacoplamiento datos/diseño — Necesidad de negocio |
| **Épica** | ÉPICA 3 | Parser DSL — Convertir DSL → AST |
| **User Story** | US-05 | Como desarrollador, quiero resolver datos dinámicos en el AST, para vincular datos externos a plantillas |
| **Sub-tarea (BT)** | BT-020 | Implementar IDslParser |
| **Sub-tarea (BT)** | BT-021 | Definir gramática DSL base |
| **Sub-tarea (BT)** | BT-022 | Implementar tokenizer / lexer |
| **Bug** | BUG-001 | Parser no maneja correctamente caracteres Unicode en TextNode |

> **Referencia:** Las épicas y user stories están en `../docs/06_backlog-tecnico/product-backlog_v1.0.md`. Las sub-tareas técnicas (BT-XXX) en `../docs/06_backlog-tecnico/backlog-tecnico_v1.0.md`.

**Ejemplo en contexto web (e-commerce):**

| Nivel | Ejemplo |
|---|---|
| **Iniciativa** | Aumentar la conversión del checkout en un 25% (OKR Q2) |
| **Épica** | Checkout multi-paso con pasarela de pagos |
| **User Story** | Como comprador, quiero pagar con tarjeta de crédito, para completar mi compra sin efectivo |
| **Tarea técnica** | Integrar SDK de Stripe para tokenización de tarjetas |
| **Sub-tarea** | Crear migración para tabla `payment_transactions` |
| **Bug** | [Checkout] Doble cobro cuando el usuario hace doble clic en "Pagar" |

**Regla práctica para clasificar:** Si no sabés si algo es épica, historia o tarea, preguntate: *¿Se puede demostrar al PO en la Sprint Review?* Si la respuesta es sí y cabe en un sprint → es historia. Si la respuesta es sí pero no cabe en un sprint → es épica. Si la respuesta es no → es tarea técnica.

#### 6.5 Estimación

> **Definición:** **Estimación** en el contexto ágil es el proceso de asignar un valor relativo de esfuerzo, complejidad y riesgo a un ítem del backlog. No es una "promesa de entrega" — es una herramienta de **planificación** que ayuda al equipo a decidir cuánto trabajo cabe en un sprint. Las estimaciones ágiles son deliberadamente imprecisas: se busca estar "aproximadamente correcto" en lugar de "precisamente incorrecto".

> **Definición:** Los **Story Points** (puntos de historia) son una unidad abstracta de medida que combina tres factores: **complejidad** (qué tan difícil es), **esfuerzo** (cuánto trabajo implica) e **incertidumbre** (cuánto desconocemos). No se traducen directamente a horas — un ítem de 5 SP puede tardar 4 horas para un senior y 12 horas para un junior, pero el equipo asigna los mismos 5 SP.

> **Definición:** La **Velocity** (velocidad) es la cantidad promedio de Story Points que un equipo completa por sprint. Se calcula sobre los últimos 3-5 sprints. Se usa para predecir cuánto trabajo cabe en el próximo sprint. No es una métrica de productividad — es una métrica de **capacidad de planificación**.

##### Story Points vs. Horas

| Aspecto | Story Points | Horas |
|---|---|---|
| **Mide** | Complejidad relativa | Tiempo absoluto |
| **Ventaja** | Abstracto, no penaliza diferencias de skill | Fácil de entender para no-técnicos |
| **Desventaja** | Requiere calibración del equipo | Estimaciones inexactas, presión por cumplir |
| **Uso recomendado** | Planificación de sprints, velocity | Tracking personal, timeboxing de spikes |

##### Planning Poker

> **Definición:** **Planning Poker** (también llamado Scrum Poker) es una técnica de estimación por consenso creada por James Grenning (2002) y popularizada por Mike Cohn. Combina la opinión experta de cada miembro del equipo con la discusión grupal para llegar a estimaciones más precisas que las individuales. El uso de cartas evita el sesgo de anclaje (que un estimador influya en los demás).

Técnica de estimación grupal:
1. El PO presenta la historia
2. Cada miembro del equipo elige una carta con su estimación (escala Fibonacci)
3. Se revelan todas las cartas simultáneamente
4. Si hay diferencias grandes, los extremos explican su razonamiento
5. Se repite hasta converger

**Escala Fibonacci:** 1, 2, 3, 5, 8, 13, 21, ∞, ☕

| Puntos | Significado típico |
|---|---|
| 1-2 | Tarea trivial, cambio menor |
| 3-5 | Complejidad moderada, bien entendida |
| 8 | Complejidad alta, alguna incertidumbre |
| 13 | Muy complejo, considerar descomponer |
| 21+ | Demasiado grande, DEBE descomponerse |
| ∞ | No se puede estimar — falta información |
| ☕ | Necesitamos un break |

**Ejemplo del proyecto:** En el Sprint 01, US-03 (Parser DSL básico) fue estimada en 13 SP — la más compleja del sprint, reflejando la incertidumbre del diseño del tokenizer.

#### 6.6 Priorización

> **Definición:** **Priorización** es el acto de decidir el **orden** en que se implementan los ítems del backlog. Es responsabilidad principal del **Product Owner**, quien balancea valor de negocio, riesgo técnico, dependencias y feedback de stakeholders. Un backlog sin priorización explícita es una lista de deseos — no una herramienta de planificación.

Existen varias técnicas de priorización, cada una útil en contextos diferentes:

##### MoSCoW

> **Definición:** **MoSCoW** es un método de priorización creado por Dai Clegg (1994) en el contexto de DSDM (Dynamic Systems Development Method). Clasifica cada ítem del backlog en una de 4 categorías según su importancia para el release actual. El nombre es un acrónimo: **M**ust have, **S**hould have, **C**ould have, **W**on't have.

| Categoría | Significado | Distribución sugerida | Ejemplo del proyecto |
|---|---|---|---|
| **Must Have** | Imprescindible. Sin esto, el release no tiene sentido | ~60% del esfuerzo | US-03 (Parser DSL), US-09 (Render ESC/POS) |
| **Should Have** | Importante, pero el producto funciona sin esto | ~20% del esfuerzo | US-10 (Render texto plano), US-11 (Vista previa UI) |
| **Could Have** | Deseable si sobra capacidad | ~20% del esfuerzo | US-27 (Render PDF) |
| **Won't Have** | Explícitamente fuera de alcance para esta versión | 0% | Editor visual, scripting avanzado |

**Ejemplo MoSCoW en contexto web (e-commerce):**

| Categoría | Ejemplo e-commerce |
|---|---|
| **Must** | Carrito de compras, checkout, registro de usuario |
| **Should** | Favoritos, historial de compras, reviews de productos |
| **Could** | Recomendaciones personalizadas, comparador de productos |
| **Won't** | Marketplace multi-vendor, programa de lealtad (v2.0) |

##### WSJF (Weighted Shortest Job First)

> **Definición:** **WSJF** (Weighted Shortest Job First) es una técnica de priorización numérica del framework SAFe (Scaled Agile Framework). Calcula una puntuación que favorece ítems de **alto valor y bajo esfuerzo**, optimizando el retorno económico del trabajo. Es especialmente útil cuando hay muchos ítems compitiendo por recursos limitados.

Fórmula de priorización de SAFe:

$$WSJF = \frac{\text{Valor de negocio} + \text{Criticidad temporal} + \text{Reducción de riesgo}}{\text{Tamaño del trabajo}}$$

Se usa para comparar ítems del backlog: el ítem con mayor WSJF se hace primero.

##### Value vs. Effort

> **Definición:** La **Matriz Value vs. Effort** (Valor vs. Esfuerzo) es una herramienta visual de priorización rápida que clasifica los ítems del backlog en 4 cuadrantes según dos ejes: cuánto valor aportan al negocio y cuánto esfuerzo requieren. Es más simple que WSJF y útil para sesiones de priorización con stakeholders no técnicos.

Matriz 2x2 para priorización rápida:

```
                        VALOR ALTO
                            │
              ┌─────────────┼─────────────┐
              │  HACER       │  HACER       │
              │  DESPUÉS     │  PRIMERO     │
              │  (Big Bets)  │  (Quick Wins)│
  ESFUERZO ───┼─────────────┼─────────────┤─── ESFUERZO
    ALTO      │  NO HACER    │  HACER SI    │     BAJO
              │  (Money Pit) │  SOBRA TIEMPO│
              │              │  (Fill-Ins)  │
              └─────────────┼─────────────┘
                            │
                        VALOR BAJO
```

---

### Sección 7 — Métricas ágiles

#### 7.1 Velocity

**Qué es:** La cantidad de Story Points que el equipo completa por sprint. Es la métrica ágil más usada en Scrum.

**Cómo se calcula:** Suma de SP de todas las historias que cumplen el DoD al final del sprint.

**Cómo se usa para planificar:**
- Se toma el promedio de los **últimos 3 sprints** como referencia
- No se comprometen más SP que la velocity promedio (máximo 110%)
- Se usa para proyectar la fecha de finalización del release

**Referencia del proyecto:** `../docs/07_plan-sprint/velocidad-equipo_v1.0.md`

El proyecto registra velocity por sprint:

| Sprint | SP Comprometidos | SP Completados | Velocity |
|---|---|---|---|
| Sprint 01 | 48 | Pendiente | - |
| Sprint 02 | 76 | Pendiente | - |
| Sprint 03 | 77 | Pendiente | - |

> La velocity promedio se calcula con los últimos 3 sprints completados. Las variaciones significativas (>20%) deben analizarse en la retrospectiva.

#### 7.2 Burndown Chart

**Sprint Burndown:** Muestra los SP restantes del sprint a lo largo del tiempo.

```
SP restantes
    │
 40 │ ●
    │   ●
 30 │     ●                        Línea ideal
    │       ●  - - - - - - - - - -
 20 │         ●    ●
    │              ●               Línea real
 10 │                  ●
    │                    ●  ●
  0 │─────────────────────────●──
    └─────────────────────────────
     D1  D2  D3  D4  D5  D6  D7  D8  D9  D10
```

**Señales de alerta:**
- Línea real muy por encima de la ideal → riesgo de no completar el sprint
- Línea plana varios días → bloqueo o trabajo no descompuesto
- Caída súbita al final → historias NO se cierran incrementalmente

**Release Burndown:** Lo mismo pero a nivel de release. Muestra SP restantes del backlog completo, sprint a sprint.

#### 7.3 Burnup Chart

Inverso del burndown: muestra SP completados acumulados. Más útil cuando el scope cambia:

```
SP completados
    │                                    ─── Scope total
 200│                          ┌─────────
    │                   ┌──────┘
 150│            ┌──────┘
    │     ┌──────┘         ●
 100│  ┌──┘           ●
    │  │         ●                       ─── SP completados
 50 │  │    ●
    │  ●
  0 │──
    └─────────────────────────────
     S01  S02  S03  S04  S05  S06
```

Cuando la línea de scope sube, se agregó trabajo. Cuando las líneas convergen, se proyecta la fecha de finish.

#### 7.4 Cumulative Flow Diagram (CFD)

Muestra cuántos ítems hay en cada estado a lo largo del tiempo. Las bandas deben tener ancho estable. Si una banda se ensancha, hay un cuello de botella.

```
Ítems
    │
 50 │░░░░░░░░░░░░░░░░░░░░░░░░░░░  Done
    │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓    In QA
 40 │████████████████████████       In Review
    │▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒          In Progress
 30 │░░░░░░░░░░░░░░░░              Backlog
    │
 20 │
    │
 10 │
    │
  0 │──────────────────────────────
    └──────────────────────────────
     Semana 1    Semana 2    Semana 3
```

#### 7.5 Lead Time y Cycle Time

```
                Lead Time
    ◄───────────────────────────────────►
    │                                   │
    │         Cycle Time                │
    │    ◄──────────────────────►       │
    │    │                      │       │
────┼────┼──────────────────────┼───────┼────
  Creado │                      │     Entregado
         │                      │
     Se empieza            Se termina
     a trabajar            de trabajar
```

#### 7.6 Tabla resumen de métricas

| Métrica | Qué mide | Frecuencia | Quién la usa |
|---|---|---|---|
| **Velocity** | SP completados por sprint | Cada sprint | SM, PO (planning) |
| **Sprint Burndown** | SP restantes durante el sprint | Diario | Dev Team, SM |
| **Release Burndown** | SP restantes del release | Cada sprint | PO, Stakeholders |
| **Burnup** | SP completados acumulados vs. scope | Cada sprint | PO, Stakeholders |
| **CFD** | Distribución de ítems por estado en el tiempo | Continuo | SM (detectar cuellos de botella) |
| **Lead Time** | Tiempo desde solicitud hasta entrega | Continuo | PO, Stakeholders |
| **Cycle Time** | Tiempo desde inicio de trabajo hasta fin | Continuo | Dev Team, SM |
| **Throughput** | Ítems completados por unidad de tiempo | Semanal | SM, PO |
| **Sprint Goal Completion** | % de sprints con goal cumplido | Cada sprint | SM, Stakeholders |

---

### Sección 8 — Anti-patrones ágiles

#### 8.1 Anti-patrones del backlog y gestión

Fuente: `../devs/especialidades/AG-06-scrum-master.md`

| Anti-patrón | Problema | Solución |
|---|---|---|
| **US como tarea técnica** | "Implementar Parser" no tiene valor para el usuario | Reformular como: "Como dev, quiero interpretar DSL, para generar documentos" |
| **Épicas sin descomponer** | Una épica de 40 SP en un sprint de 15 | Descomponer en US de 3-8 SP |
| **Sin MoSCoW** | Todo es "Must" → no hay priorización real | Forzar distribución: 60% Must, 20% Should, 20% Could |
| **DoR excesivo** | 15 criterios → nada nunca está "ready" | Máximo 6-8 criterios prácticos |
| **Backlog técnico oculto** | La deuda técnica no está visible para el PO | Registrar como ítems priorizables en el backlog |
| **Sin trazabilidad US→CU** | No se puede validar completitud del sistema | Columna "CU relacionados" obligatoria en cada US |

#### 8.2 Anti-patrones de sprints y ceremonias

Fuente: `../devs/especialidades/AG-07-gestion-proyectos-agiles.md`

| Anti-patrón | Problema | Solución |
|---|---|---|
| **Sprint Goal = lista de tareas** | "Implementar A, B, C" no da dirección ni prioridad | Reformular como frase de valor: "El motor procesa DSL end-to-end" |
| **DoD redefinido por sprint** | Cada sprint inventa su DoD → inconsistencia | Referenciar DoD canónico + agregar criterios específicos |
| **Overcommitment** | Más SP que la velocity promedio | Regla: nunca comprometer > 110% de velocity promedio |
| **Sin riesgos** | No se identifican riesgos → no hay plan B | Mínimo 2 riesgos por sprint con mitigación |
| **Retro sin acciones** | Se habla mucho pero no se define nada concreto | Cada retro debe producir ≥ 1 acción con responsable |
| **Velocity no registrada** | No se sabe cuánto puede el equipo | Actualizar tabla después de cada sprint review |

#### 8.3 Anti-patrones adicionales comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| **El PO ausente** | El Product Owner aparece solo en el planning y desaparece hasta la review. Las dudas no se resuelven, el equipo asume o se bloquea. | El PO debe estar disponible durante el sprint. Mínimo: atender dudas en el día, asistir a dailies 2-3 veces por semana. |
| **La daily de 45 minutos** | La daily se convierte en reunión de estado detallado, discusión técnica o resolución de problemas. | Timebox estricto de 15 minutos. Solo 3 preguntas: qué hice, qué haré, qué me bloquea. Discusiones técnicas → "parking lot" después de la daily. |
| **Sprint 0 eterno** | El equipo pasa semanas o meses en "preparación" (infra, diseño, setup) sin entregar incremento de producto. | Sprint 0 máximo de 1 sprint. Entregar algo funcional desde el Sprint 1, aunque sea mínimo. Setup de infra se puede hacer en paralelo. |
| **La retro sin acción** | La retrospectiva se convierte en sesión de catarsis donde todos hablan pero nadie se compromete a mejorar. | Cada retro produce mínimo 1 acción concreta con: responsable, fecha y sprint target. Revisar acciones anteriores al inicio de cada retro. |
| **Velocity como KPI de performance individual** | Se usa la velocity para evaluar la productividad de personas individuales, generando inflación de estimaciones y competencia tóxica. | Velocity es una métrica del EQUIPO para planificación. Nunca se usa para evaluar individuos. Si alguien entrega menos SP, se analiza en la retro como equipo, no como performance review. |
| **Micromanagement disfrazado de Scrum** | Un "Scrum Master" o manager asigna tareas, controla horarios y decide quién hace qué. El equipo no se auto-organiza. | El SM facilita, no gestiona. El equipo decide cómo hacer el trabajo. El SM remueve impedimentos y protege al equipo de interrupciones externas. |
| **El sprint elástico** | El sprint se extiende "unos días más" para completar las historias comprometidas. | El sprint tiene fecha fija. Lo que no se completó vuelve al backlog y se reprioriza. Nunca se extiende el timebox. |
| **El refinamiento inexistente** | No se hace refinamiento del backlog. Las historias llegan al planning sin detallar, sin estimar y sin criterios de aceptación. | Dedicar 10% del sprint a refinamiento (ej: 1-2 horas semanales). Las historias deben cumplir el DoR antes del planning. |
| **El board fantasma** | El board de Jira/tablero no refleja la realidad. Los ítems no se actualizan, las columnas no representan el estado real. | Actualizar el board en la daily. Si no está en el board, no existe. El SM revisa el board diariamente. |

---

## PARTE 2 — GESTIÓN DE TAREAS CON JIRA

---

### Sección 9 — Introducción a Jira

#### 9.1 Qué es Jira

**Jira** es una herramienta de gestión de proyectos desarrollada por **Atlassian**. Es la plataforma más usada en la industria del software para tracking de tareas, gestión de backlogs, planificación de sprints y seguimiento de bugs.

Jira permite:
- Crear y gestionar proyectos ágiles (Scrum y Kanban)
- Definir backlogs con épicas, historias, tareas y bugs
- Planificar y ejecutar sprints
- Visualizar el trabajo en tableros (boards)
- Generar reportes y métricas (velocity, burndown, CFD)
- Integración con Git, CI/CD y herramientas de comunicación

#### 9.2 Conceptos base

| Concepto | Definición |
|---|---|
| **Proyecto** | Contenedor de todo el trabajo. Tiene una clave única (ej: MDSL) que prefija todos los issues. |
| **Board** | Vista visual del trabajo. Puede ser Scrum (con sprints) o Kanban (flujo continuo). |
| **Backlog** | Vista de lista de todos los issues del proyecto, priorizados por orden. |
| **Sprint** | Iteración timeboxed en un proyecto Scrum. Se crea, se llena de issues, se inicia y se cierra. |
| **Issue** | Unidad de trabajo. Puede ser Epic, Story, Task, Sub-task o Bug. |
| **Workflow** | Flujo de estados que un issue recorre (To Do → In Progress → Done). |
| **Component** | Categoría para agrupar issues por módulo o subsistema (ej: Parser, Rendering, Core). |
| **Fix Version** | Versión del producto donde se incluirá el issue (ej: v1.0.0). Equivale a un Release/Milestone. |

#### 9.3 Tipos de proyecto

| Tipo | Descripción | Cuándo usar |
|---|---|---|
| **Scrum** | Board con backlog, sprints, velocity, burndown | Equipos con iteraciones fijas y ceremonias Scrum |
| **Kanban** | Board con flujo continuo, WIP limits, CFD | Equipos de soporte, ops o con demanda continua |
| **Team-managed** | Proyecto simple, configurado por el equipo | Equipos pequeños que quieren autonomía rápida |
| **Company-managed** | Proyecto administrado por admins de Jira | Organizaciones con governance y configuraciones centralizadas |

#### 9.4 Cuándo usar Jira vs. alternativas

| Herramienta | Fortaleza | Cuándo elegirla |
|---|---|---|
| **Jira** | Completísima para Scrum/Kanban, reportes avanzados, JQL, integraciones enterprise | Equipos medianos/grandes, organizaciones con procesos ágiles maduros |
| **Azure DevOps** | Integración nativa con ecosistema Microsoft (.NET, Azure, VS) | Equipos .NET/Azure, organizaciones Microsoft |
| **Linear** | UX moderna, velocidad, developer-friendly | Startups, equipos técnicos que priorizan DX |
| **Trello** | Simplicidad, tableros Kanban, gratuito | Equipos pequeños, proyectos simples, personas no-técnicas |
| **GitHub Projects** | Integración directa con repos y PRs, gratuito | Proyectos open source, equipos que viven en GitHub |

---

### Sección 10 — Nomenclatura y tipos de issue en Jira

#### 10.1 Tipos de issue estándar

##### Epic

- **Definición:** Funcionalidad grande que abarca múltiples historias y puede extenderse a varios sprints
- **Cuándo crear una:** Cuando hay un bloque de funcionalidades relacionadas que forman un objetivo de negocio coherente
- **Nomenclatura sugerida:** Se usa la clave del proyecto + número auto-incremental. Agregar título descriptivo
- **Ejemplo:** `MDSL-E03 — Parser DSL` (equivale a ÉPICA 3 del proyecto)

##### Story (User Story)

- **Formato:** "Como [rol], quiero [acción], para [beneficio]"
- **Campos obligatorios:** Summary, Description (con formato US), Acceptance Criteria, Epic Link, Story Points, Priority
- **Link a épica:** Toda story debe estar vinculada a una Epic
- **Ejemplo:** `MDSL-05 — Como desarrollador, quiero resolver datos dinámicos en el AST, para vincular datos externos a plantillas`

##### Task

- **Cuándo usar task vs. story:** Usar Task para trabajo que no tiene valor directo para el usuario final pero es necesario (infraestructura, configuración, spikes técnicos)
- **Ejemplo:** `MDSL-TK01 — Crear solución .sln y proyectos (Core, Parser, etc.)`

##### Sub-task

- **Para descomposición técnica:** Se crean bajo una Story o Task para representar el trabajo implementable
- **Ejemplo:** `MDSL-BT020 — Implementar IDslParser` (sub-task de la Story US-05)

##### Bug

- **Campos específicos:**
  - Pasos para reproducir (Steps to Reproduce)
  - Resultado esperado (Expected Result)
  - Resultado actual (Actual Result)
  - Severidad: Critical / Major / Minor / Trivial
  - Entorno: SO, versión del runtime, dispositivo
- **Ejemplo:** `MDSL-BUG003 — Parser no detecta nodos duplicados en templates con más de 3 niveles de anidamiento`

#### 10.2 Campos clave de cada issue

| Campo | Descripción | Obligatorio |
|---|---|---|
| **Summary** | Título conciso del issue | Sí |
| **Description** | Detalle, formato US, criterios de aceptación | Sí |
| **Assignee** | Miembro del equipo responsable | Sí (durante el sprint) |
| **Reporter** | Quien creó el issue | Auto (creador) |
| **Priority** | Highest, High, Medium, Low, Lowest | Sí |
| **Labels** | Etiquetas libres para categorizar (ej: tech-debt, refactor) | Opcional |
| **Components** | Módulo del sistema (ej: Parser, Rendering, Core) | Recomendado |
| **Sprint** | Sprint al que está asignado | Sí (cuando se compromete) |
| **Story Points** | Estimación de complejidad | Sí (para Stories) |
| **Fix Version** | Release donde se incluirá | Recomendado |
| **Epic Link** | Épica padre | Sí (para Stories y Tasks) |

#### 10.3 Prioridades en Jira y mapeo a MoSCoW

| Prioridad Jira | Icono | Mapeo MoSCoW | Uso |
|---|---|---|---|
| **Highest** | 🔴 | Must Have | Bloqueante. Sin esto, el release no sale. |
| **High** | 🟠 | Must Have | Crítico pero no bloqueante inmediato. |
| **Medium** | 🟡 | Should Have | Importante. Se incluye si hay capacidad. |
| **Low** | 🔵 | Could Have | Deseable. Se hace si sobra tiempo. |
| **Lowest** | ⚪ | Won't Have | No se hará en esta versión. Documentado para futuro. |

#### 10.4 Estados y workflow típico

```
┌──────────┐    ┌─────────────┐    ┌───────────┐    ┌─────┐    ┌──────┐
│  TO DO   │───►│ IN PROGRESS │───►│ IN REVIEW │───►│ QA  │───►│ DONE │
└──────────┘    └─────────────┘    └───────────┘    └─────┘    └──────┘
                       │                  │              │
                       │                  │              │
                       ◄──────────────────┘              │
                       │     (cambios solicitados)       │
                       ◄─────────────────────────────────┘
                             (bug encontrado en QA)
```

**Personalización del workflow:**
- Agregar estado **Blocked** para impedimentos
- Agregar estado **Ready for QA** si QA es un equipo separado
- Agregar transición automática cuando se crea un PR (→ In Review)
- Validaciones en transición: ej. no pasar a Done sin PR mergeado

#### 10.5 Tabla de nomenclatura sugerida

| Tipo Jira | Nomenclatura SDD | Ejemplo | Descripción |
|---|---|---|---|
| Epic | ÉPICA-XX | ÉPICA-03 | Agrupación de funcionalidades relacionadas |
| Story | US-XXX | US-005 | User Story con valor para el usuario |
| Task | TK-XXX | TK-015 | Tarea técnica sin valor directo al usuario |
| Sub-task | BT-XXX | BT-012 | Descomposición técnica de una Story/Task |
| Bug | BUG-XXX | BUG-003 | Defecto encontrado en testing o producción |

---

### Sección 11 — Configuración de un proyecto Scrum en Jira

#### 11.1 Crear proyecto Scrum: paso a paso

1. **Ir a Projects → Create project**
2. **Seleccionar template:** Scrum
3. **Tipo:** Team-managed (para autonomía) o Company-managed (para governance)
4. **Nombre:** Motor DSL de Generación de Documentos
5. **Clave:** MDSL (esta clave prefica todos los issues: MDSL-1, MDSL-2, etc.)
6. **Acceso:** Restringido al equipo o abierto a la organización

#### 11.2 Configurar Board

**Columnas** (mapean al workflow):

| Columna | Mapea a estado | Descripción |
|---|---|---|
| TO DO | To Do | Issues listos para tomar (cumplen DoR) |
| IN PROGRESS | In Progress | Alguien está trabajando activamente |
| IN REVIEW | In Review | PR creado, esperando code review |
| QA | QA | Testing (manual o automatizado) |
| DONE | Done | Cumple Definition of Done |

**Swimlanes** (filas horizontales para agrupar):
- Por Epic: cada épica tiene su fila → visual por funcionalidad
- Por Assignee: cada persona tiene su fila → visual de carga
- Por Priority: separar Highest/High del resto

**Filtros rápidos** (quick filters útiles):
- Solo mis issues: `assignee = currentUser()`
- Bugs: `type = Bug`
- Sin estimar: `"Story Points" is EMPTY`
- Bloqueados: `status = Blocked OR labels = impediment`

#### 11.3 Configurar Backlog

- Activar la vista de Backlog en el Board settings
- Ordenar por prioridad (drag & drop o usando Priority field)
- Agrupar por Epic para visualización jerárquica
- Usar la sección "Backlog" (sin sprint) para ítems no planificados

#### 11.4 Crear y gestionar Sprints

1. **Crear Sprint:** En la vista Backlog, click en "Create Sprint"
2. **Nombrar:** Sprint 01, Sprint 02, etc.
3. **Agregar issues:** Arrastrar desde el Backlog al Sprint
4. **Iniciar Sprint:** Definir fechas de inicio y fin, Sprint Goal
5. **Durante el Sprint:** El Board muestra los issues del sprint activo
6. **Cerrar Sprint:** Al finalizar, Jira pregunta qué hacer con ítems no completados (mover al siguiente sprint o al backlog)

#### 11.5 Configurar Epics

- Crear Epics desde el Roadmap view o desde el Backlog
- Asignar color a cada Epic para identificación visual
- Vincular Stories a Epics usando el campo "Epic Link"
- Usar la vista **Timeline** (roadmap) para visualizar Epics en el tiempo

#### 11.6 Configurar Releases (Fix Versions)

1. Ir a **Project Settings → Releases** (o Versions)
2. Crear versiones alineadas con el roadmap:
   - v1.0.0 — MVP (Sprints 01-04)
   - v1.1.0 — Expansión (Sprints 05-06)
   - v1.3.0 — Madurez (Sprints 07-08)
3. Asignar cada issue a su Fix Version correspondiente

#### 11.7 Componentes

Crear componentes que mapeen a los módulos de la arquitectura:

| Componente | Descripción | Lead (opcional) |
|---|---|---|
| Core | Modelo AST, DocumentNode, nodos base | — |
| Parser | Tokenizer, lexer, IDslParser | — |
| Rendering | EscPosRenderer, TextRenderer, renderers | — |
| Layout | Motor de layout, posicionamiento | — |
| Extensibility | Sistema de plugins, DI | — |
| Testing | Tests unitarios, integración, fixtures | — |

---

### Sección 12 — Flujo de trabajo diario con Jira

#### 12.1 Sprint Planning en Jira

1. Abrir la vista **Backlog**
2. Revisar el Product Backlog priorizado con el PO
3. Verificar que las historias candidatas cumplen el **DoR** (checklist en cada issue)
4. Arrastrar historias al sprint recién creado
5. Estimar con Planning Poker (usar campo Story Points)
6. Definir el **Sprint Goal** en el campo del sprint
7. Verificar que los SP comprometidos no excedan la velocity promedio
8. Iniciar el sprint con fecha de inicio y fin

#### 12.2 Daily Scrum con Jira

- Abrir el **Board** del sprint activo
- Cada miembro revisa sus issues en el board:
  - ¿Qué moví ayer? (columna actual)
  - ¿Qué voy a mover hoy? (siguiente transición)
  - ¿Hay algo que me bloquea? (marcar como Blocked o agregar label `impediment`)
- El SM verifica que el board refleja la realidad
- Duración: 15 minutos máximo

#### 12.3 Durante el sprint

- **Actualizar estados:** Mover issues entre columnas al cambiar de estado
- **Registrar impedimentos:** Crear issue tipo Bug o agregar label `impediment`
- **Agregar sub-tasks:** Si surge trabajo adicional para una Story, agregar sub-tasks
- **Logging de trabajo:** Usar la función Log Work si se trackea tiempo
- **Comentarios:** Documentar decisiones y hallazgos en los comentarios del issue

#### 12.4 Sprint Review en Jira

1. Filtrar issues del sprint: `sprint = "Sprint XX"`
2. Revisar estados: ¿cuántos Done vs. no completados?
3. Para cada issue completado: demo al PO, registrar feedback
4. Para issues no completados: documentar razón, decidir si mover al siguiente sprint o al backlog
5. Registrar métricas en el template de Review (`../docs/07_plan-sprint/template-sprint-review_v1.0.md`)
6. Actualizar tabla de velocity (`../docs/07_plan-sprint/velocidad-equipo_v1.0.md`)

#### 12.5 Sprint Retrospective

1. Usar el template de Retro (`../docs/07_plan-sprint/template-sprint-retrospectiva_v1.0.md`)
2. Formato Start-Stop-Continue (u otro acordado)
3. Definir acciones de mejora concretas
4. **Crear issues para las acciones de mejora** en el backlog del próximo sprint
5. Revisar acciones del sprint anterior (sección 2.6 del template)

#### 12.6 Refinamiento en Jira

1. Abrir la vista **Backlog**, sección sin sprint
2. Revisar historias candidatas para los próximos 2 sprints
3. Detallar description y criterios de aceptación
4. Estimar Story Points (Planning Poker)
5. Verificar DoR (usar checklist del issue)
6. Si la historia es muy grande, descomponerla en varias Stories + Sub-tasks

#### 12.7 Diagrama del flujo diario con Jira

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FLUJO DIARIO CON JIRA                            │
│                                                                     │
│  ┌──────────────┐                                                   │
│  │  DAILY SCRUM │  ◄── 15 min, frente al Board                     │
│  │  (mañana)    │                                                   │
│  └──────┬───────┘                                                   │
│         │                                                           │
│         ▼                                                           │
│  ┌──────────────────────────────────────────────────────────┐       │
│  │  DESARROLLO                                              │       │
│  │                                                          │       │
│  │  1. Tomar issue de TO DO → mover a IN PROGRESS           │       │
│  │  2. Crear branch: {CLAVE-ISSUE}-descripcion-corta        │       │
│  │  3. Desarrollar + tests                                  │       │
│  │  4. Crear PR → mover a IN REVIEW                         │       │
│  │  5. Code review aprobado → mover a QA                    │       │
│  │  6. Tests pasan → mover a DONE                           │       │
│  └──────┬───────────────────────────────────────────────────┘       │
│         │                                                           │
│         ▼                                                           │
│  ┌──────────────┐                                                   │
│  │  ACTUALIZAR  │  ◄── Al finalizar el día o al completar un issue  │
│  │  BOARD       │                                                   │
│  │  + COMENTAR  │                                                   │
│  └──────────────┘                                                   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

### Sección 13 — Reportes y métricas en Jira

#### 13.1 Velocity Chart

**Qué muestra:** SP comprometidos vs. SP completados por sprint.

**Cómo leerlo:**
- Barras azules por encima de barras verdes → overcommitment
- Barras verdes consistentes → equipo predecible
- Barras verdes creciendo gradualmente → equipo madurando
- Barras verdes muy variables → estimación inconsistente

**Ubicación en Jira:** Board → Reports → Velocity Chart

#### 13.2 Burndown Chart

**Qué muestra:** SP restantes del sprint a lo largo del tiempo.

**Señales de alerta:**
- Línea plana → trabajo bloqueado o no descompuesto
- Caída súbita al final → historias cerradas en batch, no incrementalmente
- Línea real por encima de la guía → riesgo de no completar el sprint
- Línea sube → se agregó scope al sprint (anti-patrón)

**Ubicación en Jira:** Board → Reports → Burndown Chart

#### 13.3 Sprint Report

**Qué incluye:**
- Issues completados vs. no completados
- SP comprometidos vs. completados
- Issues agregados o removidos during sprint
- Resumen de status para la review

**Ubicación en Jira:** Board → Reports → Sprint Report

#### 13.4 Cumulative Flow Diagram (CFD)

**Qué detecta:**
- Cuello de botella en Code Review → banda de "In Review" se ensancha
- Falta de capacidad de QA → banda de "QA" crece
- Backlog creciendo más rápido que la entrega → divergencia

**Ubicación en Jira:** Board → Reports → Cumulative Flow Diagram (solo Kanban boards por defecto; en Scrum, usar plugin o JQL + dashboard)

#### 13.5 Control Chart

**Qué muestra:** Cycle time de cada issue individual a lo largo del tiempo.

**Uso:** Identificar issues con cycle time anormalmente alto para investigar la causa (bloqueados, mal estimados, dependencias).

**Ubicación en Jira:** Board → Reports → Control Chart

#### 13.6 Dashboards personalizados

Crear un dashboard con widgets útiles:

| Widget | Qué muestra | Para quién |
|---|---|---|
| Sprint Burndown | Progreso del sprint actual | Dev Team, SM |
| Velocity Chart | Tendencia de velocity | SM, PO |
| Pie Chart (por Priority) | Distribución de prioridades | PO |
| Pie Chart (por Status) | Issues por estado | SM |
| Filter Results (bugs abiertos) | Lista de bugs sin resolver | Dev Team, QA |
| Two-Dimensional Filter (Assignee × Status) | Carga por persona y estado | SM |
| Created vs. Resolved | Ratio de creación vs. resolución | PO, Stakeholders |

#### 13.7 JQL (Jira Query Language)

JQL es el lenguaje de consultas de Jira. Permite filtrar issues con precisión:

| Consulta | JQL |
|---|---|
| Issues del sprint actual | `sprint in openSprints()` |
| Bugs sin resolver | `type = Bug AND resolution = Unresolved` |
| Stories sin estimar | `type = Story AND "Story Points" is EMPTY` |
| Issues bloqueados | `status = Blocked OR labels = impediment` |
| Trabajo completado en el último sprint | `sprint in closedSprints() AND status = Done ORDER BY updated DESC` |
| Issues de una épica específica | `"Epic Link" = MDSL-E03` |
| Issues creados esta semana | `created >= startOfWeek()` |
| Issues sin asignar en el sprint | `sprint in openSprints() AND assignee is EMPTY` |
| Issues con alta prioridad sin sprint | `priority in (Highest, High) AND sprint is EMPTY` |
| Deuda técnica | `labels = tech-debt AND resolution = Unresolved ORDER BY priority DESC` |

---

### Sección 14 — Hitos (Milestones) y Releases en Jira

#### 14.1 Fix Versions como Milestones

Jira no tiene un concepto nativo de "Milestone" como GitHub. En su lugar, se usan **Fix Versions** (también llamadas Releases) para representar hitos del producto.

Cada Fix Version tiene:
- **Nombre:** versión semántica (ej: v1.0.0)
- **Start date:** fecha de inicio
- **Release date:** fecha de entrega planificada
- **Description:** objetivo del release
- **Status:** Unreleased, Released, Archived

#### 14.2 Release Hub

Jira ofrece una vista de Release Hub (Deployments → Releases) donde se puede:

- **Planning:** Ver qué issues están asignados a cada versión
- **Tracking:** Progreso de completitud (% de issues Done)
- **Ship:** Marcar la versión como Released cuando se despliega

#### 14.3 Mapeo al roadmap del proyecto

Referencia: `../docs/00_contexto/roadmap-producto_v1.0.md`

| Fase | Release Jira | Sprints | Objetivo | Épicas |
|---|---|---|---|---|
| **Fase 1 — MVP** | v1.0.0 | Sprint 01-04 | Motor funcional DSL → AST → Render ESC/POS | ÉPICA 1-7 (parcial) |
| **Fase 2 — Expansión** | v1.1.0 / v1.2.0 | Sprint 05-06 | Multi-formato, extensibilidad, integración MAUI | ÉPICA 7 (completa), 8, 9 |
| **Fase 3 — Madurez** | v1.3.0 | Sprint 07-08 | PDF, Bluetooth, performance, documentación | ÉPICA 10, 11 |
| **Futuro** | v2.0.0 | Por definir | Breaking changes, DSL v2, editor visual | Por definir |

#### 14.4 Cómo vincular issues a releases

1. Abrir el issue en Jira
2. Campo **Fix Version**: seleccionar la versión correspondiente
3. En bulk: seleccionar múltiples issues → Bulk Change → Edit → Fix Version

#### 14.5 Release notes automáticas desde Jira

Al marcar una Fix Version como "Released":
1. Ir a **Project → Releases → [versión]**
2. Click en **Release notes**
3. Jira genera automáticamente una lista de issues completados, agrupados por tipo

#### 14.6 Tabla de mapeo completa SDD → Jira

| Concepto SDD | Equivalente Jira | Ejemplo del proyecto |
|---|---|---|
| Fase del roadmap | Fix Version (Release) | v1.0.0 — MVP |
| Épica | Epic | ÉPICA 3 — Parser DSL |
| User Story | Story | US-05: Resolver datos dinámicos en el AST |
| Backlog Técnico | Sub-task o Task | BT-012: Implementar nodos básicos |
| Sprint Plan | Sprint | Sprint 01 (2 semanas, 48 SP) |
| Necesidad de Negocio | Initiative/Theme (o Label) | NB-01: Desacoplamiento datos/diseño |
| Caso de Uso | Linked Issue o Label | CU-05: Procesar template DSL |
| Definition of Ready | Checklist en Story (o campo custom) | DoR checklist con 8 criterios |
| Definition of Done | Workflow transition conditions | Validaciones en transición a Done |
| Review del sprint | Sprint Report + filtro del sprint | Sprint Report de Jira + template de review |
| Retrospectiva | Issues de mejora en siguiente sprint | Acciones AM-XX como Tasks del sprint siguiente |
| Velocity | Velocity Chart de Jira | Charts auto-generados + velocidad-equipo_v1.0.md |

---

### Sección 15 — Integración Jira con herramientas del equipo

#### 15.1 Jira + Git (GitHub/GitLab/Bitbucket)

**Smart Commits:** Al incluir la clave del issue en los mensajes de commit, Jira vincula automáticamente el commit al issue:

```
git commit -m "MDSL-05 #comment Implementar DataResolver básico #time 2h"
```

Acciones disponibles en smart commits:
- `#comment [texto]` → agrega un comentario al issue
- `#time [duración]` → registra tiempo de trabajo
- `#[transición]` → cambia el estado (ej: `#done`, `#in-review`)

**Branch naming con clave de issue:**

Formato recomendado: `{CLAVE-ISSUE}-descripcion-corta`

| Tipo | Formato de rama | Ejemplo |
|---|---|---|
| Feature | `feature/MDSL-05-data-resolver` | `feature/MDSL-05-data-resolver` |
| Bugfix | `bugfix/MDSL-BUG003-unicode-parser` | `bugfix/MDSL-BUG003-unicode-parser` |
| Hotfix | `hotfix/MDSL-BUG007-crash-escpos` | `hotfix/MDSL-BUG007-crash-escpos` |

> **Referencia:** Ver la guía de flujo de trabajo y versionado del proyecto en `../devs/versionado/guia-flujo-trabajo-versionado.md`

**Beneficios de la integración:**
- En Jira, cada issue muestra las branches, commits y PRs asociados
- En GitHub/GitLab, el PR muestra el issue de Jira vinculado
- Trazabilidad completa: issue → branch → commits → PR → merge → deploy

#### 15.2 Jira + CI/CD

**Deployment tracking:**
- Jira puede mostrar en qué entorno está desplegado un issue (dev, staging, prod)
- Se configura desde la integración con GitHub Actions, GitLab CI, Azure Pipelines, etc.

**Build info:**
- Jira muestra el estado del build (pass/fail) asociado a cada issue
- Si el CI falla, el issue no debería transicionar a Done

**Configuración típica:**
1. Instalar la app de Jira en GitHub/GitLab
2. Configurar el pipeline para enviar deployment info a Jira
3. En Jira: ver Deployments tab en cada issue

#### 15.3 Jira + Confluence

- Vincular páginas de Confluence a issues de Jira (documentación técnica, ADRs, guías)
- Crear páginas desde templates: Sprint Plan, Review, Retro
- Usar macros de Jira en Confluence para embeber filtros JQL y reportes

#### 15.4 Jira + Slack/Teams

**Notificaciones útiles:**
- Issue asignado a mí → notificación personal
- Issue movido a Done → notificación al canal del equipo
- Bug creado con prioridad Highest → notificación urgente
- Sprint iniciado/cerrado → resumen al canal

**Configuración:**
1. Instalar la app de Jira para Slack/Teams
2. Conectar el proyecto
3. Configurar reglas de notificación por canal

#### 15.5 Automatizaciones útiles

Jira Automation permite reglas sin código:

| Automatización | Trigger | Acción |
|---|---|---|
| **Auto-assign** | Issue movido a In Progress | Asignar al usuario que movió el issue |
| **Auto-transition** | PR mergeado en GitHub | Mover issue a In Review o Done |
| **Notificación de bloqueo** | Issue marcado como Blocked | Enviar mensaje al SM por Slack |
| **Cierre de sprint** | Sprint cerrado | Enviar resumen de velocity al canal del equipo |
| **Sub-task done** | Todas las sub-tasks de una Story en Done | Transicionar la Story a In Review |
| **SLA warning** | Issue lleva > 3 días en In Progress | Notificación al assignee y SM |
| **Sprint scope alert** | Issue agregado al sprint activo | Notificación al SM y PO |

---

## Sección 16 — Técnicas Complementarias de Descomposición y Planificación Ágil

### 16.1 — Introducción

Las metodologías ágiles como Scrum y Kanban definen **marcos de trabajo** — roles, ceremonias, artefactos, cadencias — pero no especifican **cómo descomponer el trabajo** para que cada incremento entregue valor real. Scrum dice "hacer sprints con historias priorizadas" pero no dice cómo transformar una funcionalidad grande en historias que atraviesen toda la arquitectura y sean demostrables en una Sprint Review.

Este vacío metodológico genera problemas recurrentes:

- Historias que son "capas horizontales" (toda la UI, toda la API) en lugar de funcionalidades completas
- Backlogs extensos sin una visión clara de cómo las historias se conectan con el flujo del usuario
- Criterios de aceptación vagos que generan retrabajo después del sprint
- Sprints de investigación sin output claro
- Equipos que no saben cómo estimar porque la incertidumbre técnica es demasiado alta

Las técnicas complementarias que se presentan en esta sección **llenan ese vacío**. No reemplazan a Scrum ni a Kanban — los complementan con prácticas concretas para cortar, organizar, refinar y validar el trabajo.

**Tabla resumen de técnicas:**

| Técnica | Categoría | En qué ayuda | Cuándo usarla |
|---|---|---|---|
| Vertical Slicing | Descomposición | Cortar funcionalidades end-to-end | Al crear user stories |
| Walking Skeleton | Arquitectura incremental | Validar la arquitectura con un path completo mínimo | Inicio del proyecto / MVP |
| Thin Slice / Tracer Bullet | Arquitectura incremental | Reducir riesgo técnico temprano | Sprint 1 o cuando hay incertidumbre arquitectónica |
| Story Mapping | Planificación visual | Organizar el backlog por flujo de usuario | Refinamiento, planificación de releases |
| Example Mapping | Refinamiento de historias | Clarificar historias con ejemplos concretos | Refinamiento pre-sprint |
| Spike / Architectural Spike | Reducción de incertidumbre | Investigar antes de comprometer | Cuando la estimación es imposible |
| ATDD | Calidad integrada | Definir criterios de aceptación ejecutables | Antes de codear cada historia |
| Mob Programming | Colaboración | Resolver problemas complejos en equipo | Onboarding, problemas cross-cutting |
| Shape Up | Framework alternativo | Ciclos largos con scope flexible | Equipos con poca ceremonia |
| Impact Mapping | Planificación estratégica | Conectar entregas con objetivos de negocio | Inicio del proyecto, planificación de roadmap |
| Dual-Track Agile | Discovery + Delivery | Separar investigación de construcción | Productos con alta incertidumbre de mercado |

---

### 16.2 — Vertical Slicing

#### Qué es

Una **rebanada vertical** (vertical slice) es una porción de funcionalidad que atraviesa **todas las capas de la arquitectura** — desde la interfaz/entrada hasta el almacenamiento/salida — y entrega una funcionalidad completa de punta a punta, aunque sea mínima. El concepto se contrapone al "horizontal slicing" donde cada sprint completa una capa entera (toda la UI, toda la API, toda la base de datos) sin entregar valor funcional hasta que todas las capas se integran.

#### Contraste con Horizontal Slicing

```
HORIZONTAL SLICING (anti-patrón):        VERTICAL SLICING (recomendado):
┌─────────────────────────┐              ┌───┬───┬───┬───────────┐
│      UI completa        │ Sprint 1     │ U │ U │ U │           │
├─────────────────────────┤              │ I │ I │ I │           │
│      API completa       │ Sprint 2     ├───┼───┼───┤    ...    │
├─────────────────────────┤              │ A │ A │ A │           │
│    Lógica completa      │ Sprint 3     │ P │ P │ P │           │
├─────────────────────────┤              │ I │ I │ I │           │
│     Datos completo      │ Sprint 4     ├───┼───┼───┤           │
└─────────────────────────┘              │ D │ D │ D │           │
                                         │ B │ B │ B │           │
Valor entregado: Sprint 4               └───┴───┴───┴───────────┘
                                         S1   S2   S3
                                         Valor entregado: Sprint 1
```

En el anti-patrón horizontal, el equipo trabaja 4 sprints antes de poder demostrar algo funcional. En el enfoque vertical, **cada sprint entrega una funcionalidad completa** (aunque limitada) que se puede demostrar en la Sprint Review.

#### Ejemplo concreto del proyecto Motor DSL

**HORIZONTAL (anti-patrón) — cómo NO se hizo:**

| Sprint | Qué se hubiera entregado | Valor para el usuario |
|---|---|---|
| Sprint 1 | Todo el parser (CU-01 a CU-03 completos) | ❌ Ninguno — no se puede ejecutar nada |
| Sprint 2 | Todo el evaluator (CU-05 a CU-08 completos) | ❌ Ninguno — no hay rendering |
| Sprint 3 | Todo el layout + rendering | ✅ Recién ahora se puede ver un resultado |

**VERTICAL (como se planificó) — enfoque correcto:**

| Sprint | Qué se entrega | Valor para el usuario |
|---|---|---|
| Sprint 01 | Parser básico → AST mínimo → estructura base funcional (US-01 + US-02 + US-03 + US-04) | ✅ Pipeline mínimo demostrable |
| Sprint 02 | Evaluator de variables → condicionales → loops → AST evaluado (US-05 a US-08) | ✅ Plantillas con datos dinámicos |
| Sprint 03 | Layout + first render ESC/POS (US-09 a US-12) | ✅ Documento imprimible |

El roadmap del proyecto aplica vertical slicing desde la Fase 1 (MVP): entrega un pipeline completo **DSL → AST → Evaluación → Render**, no "toda una capa". Cada sprint agrega profundidad al pipeline manteniendo la capacidad de generar output funcional.

> 📎 Referencia: `docs/00_contexto/roadmap-producto_v1.0.md` — Fase 1 (Sprints 01–04)

#### Reglas prácticas para hacer vertical slicing

1. **Identificar el flujo más simple** que atraviese todas las capas de la arquitectura
2. **Reducir el scope de cada capa** al mínimo viable para ese flujo (un solo tipo de nodo, un solo formato de salida)
3. **Cada slice debe ser demostrable** en una Sprint Review — si no se puede demostrar, es demasiado técnico
4. **Si un slice no tiene output visible**, re-cortar hasta que lo tenga
5. **Priorizar slices por riesgo** — los que validan decisiones arquitectónicas van primero

#### Ejemplo en contexto web — E-commerce (Checkout)

**HORIZONTAL (anti-patrón):**

| Sprint | Qué se entrega | Valor para el usuario |
|---|---|---|
| Sprint 1 | Toda la UI del checkout (formulario de dirección, resumen, pago) | ❌ Ninguno — no hay backend |
| Sprint 2 | Toda la API REST (endpoints de carrito, pagos, envío) | ❌ Ninguno — no hay integración con pasarela |
| Sprint 3 | Toda la integración (Stripe, cálculo de envío, emails) | ✅ Recién ahora funciona |

**VERTICAL (recomendado):**

| Sprint | Qué se entrega | Valor para el usuario |
|---|---|---|
| Sprint 1 | Comprar 1 producto con tarjeta de crédito (UI mínima → API → Stripe → confirmación) | ✅ Se puede comprar |
| Sprint 2 | Agregar cupón de descuento al checkout (campo UI → validación API → descuento aplicado) | ✅ Se pueden usar cupones |
| Sprint 3 | Pagar con MercadoPago como alternativa (botón UI → integración MP → confirmación) | ✅ Nuevo medio de pago |

```
Vertical Slicing en E-commerce:
┌─────────┬────────────┬─────────────┬───────────────┐
│ Slice 1 │  Slice 2   │  Slice 3    │               │
│ Pagar   │  Cupón de  │  Pagar con  │               │
│ con     │  descuento │  Mercado    │    ...más     │
│ tarjeta │            │  Pago       │    slices     │
│         │            │             │               │
│ UI+API  │  UI+API    │  UI+API     │               │
│ +Stripe │  +lógica   │  +integrac. │               │
│ +email  │  +BD       │  +webhook   │               │
└─────────┴────────────┴─────────────┴───────────────┘
  Sprint 1    Sprint 2     Sprint 3
  ✅ valor    ✅ valor     ✅ valor
```

---

### 16.3 — Walking Skeleton

#### Qué es

Un **walking skeleton** es el primer vertical slice del proyecto — una implementación mínima que recorre toda la arquitectura end-to-end. No tiene funcionalidad real completa, pero demuestra que **las capas se conectan** y que el pipeline funciona de punta a punta. El término fue acuñado por Alistair Cockburn.

El skeleton "camina" — es decir, se ejecuta — pero apenas hace algo útil. Su valor no está en la funcionalidad que entrega sino en la **validación arquitectónica** que provee.

#### Ejemplo concreto del proyecto Motor DSL

El Sprint 01 fue el walking skeleton del Motor DSL:

| Componente | Qué se implementó | Qué NO se implementó |
|---|---|---|
| Template DSL | Un JSON mínimo con un solo TextNode | Tablas, imágenes, condicionales |
| Parser | Deserialización básica JSON → nodo | Validación de schema, errores descriptivos |
| AST | DocumentNode con un hijo TextNode | Nodos complejos, anidamiento profundo |
| Estructura base | Solución .NET, proyecto, interfaces | Evaluator, Layout, Renderers |

```
Walking Skeleton (Sprint 01):
┌──────────┐    ┌─────────┐    ┌──────────┐    ┌──────────┐
│ Template │───►│ Parser  │───►│   AST    │───►│ Procesar │──► Output
│ JSON     │    │ (básico)│    │ (mínimo) │    │ plantilla│
└──────────┘    └─────────┘    └──────────┘    └──────────┘
     ▲                                              │
     │            Todas las capas conectadas         │
     └───────────── valor demostrable ◄──────────────┘
```

**Resultado:** Al final del Sprint 01, el equipo pudo ejecutar el motor con una plantilla DSL mínima y obtener un resultado procesado. La arquitectura estaba validada; los sprints siguientes agregaron profundidad sin cambiar la estructura fundamental.

> 📎 Referencia: `docs/07_plan-sprint/plan-iteracion_sprint-01_v1.0.md`

#### Diferencia con MVP

| Concepto | Walking Skeleton | MVP (Minimum Viable Product) |
|---|---|---|
| Objetivo | Validar la **arquitectura** | Validar el **producto** con usuarios reales |
| Audiencia | El equipo de desarrollo | Usuarios / stakeholders |
| Output | Pipeline funcional mínimo | Producto usable con valor de negocio |
| Cuándo | Sprint 1 | Fin de Fase 1 (Sprint 04 en este proyecto) |
| Criterio de éxito | "Las capas se conectan" | "El usuario puede resolver su problema" |

El walking skeleton es un paso **técnico** — un medio para llegar al MVP. El MVP es un paso de **negocio** — el primer producto que valida hipótesis de mercado.

#### Ejemplo en contexto web — Plataforma de cursos online (SaaS)

El walking skeleton de una plataforma de cursos online sería:

| Componente | Qué se implementa en el skeleton | Qué NO se implementa |
|---|---|---|
| Frontend | Página de login + 1 pantalla de catálogo con 1 curso hardcodeado | Diseño final, responsive, catálogo dinámico |
| API | Endpoint `/courses` que devuelve 1 curso fijo, endpoint `/enroll` | CRUD completo, búsqueda, filtros, paginación |
| Base de datos | Tabla `users` + tabla `enrollments` mínimas | Tabla de pagos, progreso, certificados |
| Auth | Login con email/password básico | OAuth, 2FA, recuperar contraseña |
| Video | Embed de 1 video de YouTube | Player propio, DRM, streaming adaptativo |

```
Walking Skeleton — Plataforma de cursos:
┌──────────┐    ┌─────────┐    ┌──────────┐    ┌──────────┐
│ Usuario  │───►│  Login  │───►│ Catálogo │───►│Inscribir │──► Confirmación
│ (email)  │    │ (básico)│    │ (1 curso)│    │ (mínimo) │
└──────────┘    └─────────┘    └──────────┘    └──────────┘
     ▲                                              │
     │         Flujo completo conectado              │
     └────────────── se puede probar ◄───────────────┘
```

**Resultado:** Al final del sprint, un usuario puede registrarse, ver un curso y hacer clic en "Inscribirse". No es usable para producción, pero demuestra que las capas se conectan (React → Express → PostgreSQL → response).

---

### 16.4 — Thin Slice / Tracer Bullet

#### Qué es

Un **tracer bullet** (bala trazadora) es un path de implementación que "ilumina" toda la cadena desde el input hasta el output. El término viene de *The Pragmatic Programmer* (Hunt & Thomas, 1999): así como una bala trazadora muestra al tirador la trayectoria real del disparo, un tracer bullet en software muestra si las decisiones tecnológicas y arquitectónicas funcionan de punta a punta **en condiciones reales** (no en un PoC aislado).

Se diferencia del walking skeleton en un matiz: el tracer bullet pone énfasis en **validar decisiones técnicas** específicas, no solo en conectar capas.

#### Cuándo usarlo

- Cuando hay **incertidumbre sobre si las tecnologías elegidas se integran** bien entre sí
- Cuando el equipo **no tiene experiencia previa** con el stack tecnológico
- Para **validar performance assumptions** antes de comprometer sprints enteros
- Cuando el cliente pregunta "¿realmente se puede hacer esto?" y necesitás una respuesta concreta

#### Ejemplo del proyecto Motor DSL

Los casos de uso CU-01 (Interpretar plantilla DSL) + CU-03 (Construir modelo interno) + CU-07 (Renderizar texto plano) juntos forman un tracer bullet que valida la pregunta técnica clave:

> *"¿Podemos ir de un JSON DSL a un output renderizado usando el pipeline de .NET con el modelo de AST propuesto?"*

```
Tracer Bullet del Motor DSL:

  ┌─────────────────────────────────────────────────────────────┐
  │                    PREGUNTA A VALIDAR:                      │
  │  "¿El pipeline DSL → Parser → AST → Renderer funciona?"    │
  └─────────────────────────────────────────────────────────────┘
                              │
  ┌──────────┐  ┌──────────┐  ▼  ┌──────────┐  ┌──────────┐
  │  CU-01   │─►│  CU-03   │───►│  CU-05   │─►│  CU-07   │
  │ Cargar   │  │ Construir│    │ Generar  │  │ Render   │
  │ template │  │ modelo   │    │ represent│  │ texto    │
  └──────────┘  └──────────┘    └──────────┘  └──────────┘
       │              │              │              │
       ▼              ▼              ▼              ▼
    JSON DSL    AST mínimo     Documento     Texto plano
    válido      funcional      abstracto     verificable
```

Si el tracer bullet falla (por ejemplo, si el parser no puede manejar la estructura DSL propuesta), se descubre en el Sprint 01 — no en el Sprint 04 cuando ya se invirtieron semanas de trabajo.

> 📎 Referencia: `docs/05_arquitectura_tecnica/arquitectura-solucion_v1.0.md` — Pipeline del motor

#### Ejemplo en contexto web — Fintech (Billetera virtual)

Una startup fintech quiere construir una billetera virtual. Antes de invertir 3 meses en la app completa, lanzan un tracer bullet para responder:

> *"¿Podemos mover dinero de cuenta A a cuenta B usando la API del banco X con nuestro stack Node.js + PostgreSQL?"*

```
Tracer Bullet — Billetera Virtual:

  ┌─────────────────────────────────────────────────────────────┐
  │                    PREGUNTA A VALIDAR:                      │
  │  "¿La API del banco X permite transferencias P2P desde      │
  │   nuestra app con el stack elegido (Node + PostgreSQL)?"    │
  └─────────────────────────────────────────────────────────────┘
                              │
  ┌──────────┐  ┌──────────┐  ▼  ┌──────────┐  ┌──────────┐
  │  UI      │─►│  API     │───►│  Banco   │─►│ Notific. │
  │  Botón   │  │  POST    │    │  API X   │  │  Email   │
  │ "Enviar" │  │ /transfer│    │ (sandbox)│  │ (básico) │
  └──────────┘  └──────────┘    └──────────┘  └──────────┘
       │              │              │              │
       ▼              ▼              ▼              ▼
   Formulario    Validación     Respuesta      Confirmación
   mínimo       básica         del banco      por email
```

**Elementos validados:** latencia de la API bancaria, formato de respuestas, manejo de errores del banco, flujo de idempotencia para evitar doble transferencia. Si el tracer bullet falla, se pivotea de banco antes de escribir la app.

---

### 16.5 — Story Mapping (Jeff Patton)

#### Qué es

**Story Mapping** es una técnica visual de organización del backlog en dos ejes:

- **Eje X (horizontal):** flujo del usuario — las actividades principales que el usuario realiza en secuencia
- **Eje Y (vertical):** profundidad de detalle — desde la funcionalidad mínima hasta las variantes más completas

El backbone (línea superior) representa las actividades principales. Debajo de cada actividad se apilan las historias de usuario, ordenadas de arriba (más prioritaria / mínima) hacia abajo (más completa / menos urgente). Se trazan líneas horizontales que definen los releases: todo lo que está sobre la primera línea es Release 1, lo que está entre la primera y la segunda línea es Release 2, etc.

#### Diagrama ASCII de un Story Map aplicado al proyecto

```
BACKBONE (actividades del pipeline del Motor DSL):
──────────────────────────────────────────────────────────────────────
 Crear       │  Cargar     │  Procesar   │  Renderizar  │ Imprimir
 Template    │  Datos      │  DSL / AST  │  Documento   │ / Output
──────────────────────────────────────────────────────────────────────
              │             │             │              │
RELEASE 1     │             │             │              │
(Fase 1 MVP): │             │             │              │
 Texto        │ JSON        │ TextNode    │ Texto plano  │ Consola
 simple       │ estático    │ básico      │ ESC/POS      │
─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─
RELEASE 2     │             │             │              │
(Fase 2):     │             │             │              │
 Tablas,      │ Variables   │ Condicional │ ESC/POS      │ Bluetooth
 imágenes     │ dinámicas   │ Loop        │ PDF          │
─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─
RELEASE 3     │             │             │              │
(Fase 3):     │             │             │              │
 DSL v2,      │ APIs        │ Extensible  │ Custom       │ Cloud
 visual       │ externas    │ plugins     │ renderers    │
```

#### Ejemplo del proyecto

Las épicas del roadmap se mapean directamente a un Story Map:

- **Backbone:** las actividades del pipeline — Crear Template → Parsear → Evaluar → Layout → Render → Output
- **Release 1 (Fase 1 — Sprints 01 a 04):** funcionalidad mínima por cada actividad — parser básico, AST con TextNode, rendering a texto plano y ESC/POS
- **Release 2 (Fase 2 — Sprints 05 a 06):** profundidad agregada — multi-formato, extensibilidad, integración .NET MAUI
- **Release 3 (Fase 3 — Sprints 07 a 08):** funcionalidades avanzadas — PDF, Bluetooth, documentación completa

> 📎 Referencia: `docs/00_contexto/roadmap-producto_v1.0.md` y `docs/06_backlog-tecnico/product-backlog_v1.0.md`

#### Cuándo usarlo

- **Al inicio del proyecto** para visualizar el scope completo y definir releases
- **En refinamientos** para identificar qué slices faltan en una actividad
- **Para comunicar el plan** al PO y stakeholders de forma visual — un Story Map es más intuitivo que una lista plana de user stories
- **Cuando el backlog crece** y se pierde la noción de cómo las historias se conectan entre sí

#### Ejemplo en contexto web — Story Map de E-commerce

```
BACKBONE (actividades del comprador):
──────────────────────────────────────────────────────────────────────
 Buscar       │ Ver         │ Agregar    │ Checkout    │  Recibir
 productos    │ detalle     │ al carrito │ y pagar     │  pedido
──────────────────────────────────────────────────────────────────────
              │             │            │             │
RELEASE 1     │             │            │             │
(MVP):        │             │            │             │
 Listado      │ Nombre,     │ Agregar 1  │ Tarjeta     │ Email de
 por categoría│ precio,     │ producto   │ de crédito  │ confirmación
              │ 1 foto      │            │ (Stripe)    │
─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─
RELEASE 2:    │             │            │             │
 Búsqueda     │ Reviews,    │ Cantidad,  │ Cupones,    │ Tracking
 por texto,   │ galería,    │ favoritos, │ MercadoPago,│ en tiempo
 filtros      │ relacionados│ comparar   │ 3 cuotas    │ real
─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─
RELEASE 3:    │             │            │             │
 IA recomend.,│ Video,      │ Wishlist   │ Wallet,     │ Devolución,
 autocomplet. │ AR preview  │ compartida │ crypto      │ reembolso
```

**Cómo se lee:** Todo lo que está arriba de la primera línea punteada es el MVP (Release 1). El equipo puede ver de un vistazo que necesita al menos 1 historia por cada columna del backbone para tener un producto usable. Si una columna del Release 1 está vacía, falta un slice.

---

### 16.6 — Example Mapping

#### Qué es

**Example Mapping** es una técnica de refinamiento de historias que usa tarjetas de 4 colores para clarificar qué significa exactamente una user story antes de comprometerla en un sprint:

- 🟡 **Amarillo:** la User Story que se está refinando
- 🔵 **Azul:** las Reglas de negocio que la historia debe cumplir
- 🟢 **Verde:** los Ejemplos concretos que ilustran cada regla (instancias específicas con datos reales)
- 🔴 **Rojo:** las Preguntas o incertidumbres que surgen durante la sesión

La sesión dura **25-30 minutos** por historia. Si hay más de 3 tarjetas rojas (preguntas sin resolver), la historia **no está lista** para el sprint — necesita más investigación (posiblemente un spike).

#### Ejemplo del proyecto — US-05: Evaluar variables simples en el AST

```
🟡 US-05: Como desarrollador, quiero evaluar variables simples en nodos
          del AST, para reemplazar placeholders con datos reales

🔵 Regla 1: El evaluator debe reemplazar variables {{variable}} con datos del contexto
  🟢 Ejemplo 1.1: {{nombre}} con datos {nombre: "Juan"} → "Juan"
  🟢 Ejemplo 1.2: {{total}} con datos {total: 1500.50} → "1500.50"
  🟢 Ejemplo 1.3: {{empresa.nombre}} (nested) con datos {empresa: {nombre: "ACME"}} → "ACME"

🔵 Regla 2: El evaluator debe manejar variables ausentes de forma configurable
  🟢 Ejemplo 2.1: {{telefono}} sin datos, modo estricto → error descriptivo
  🟢 Ejemplo 2.2: {{telefono}} sin datos, modo permisivo → mantener "{{telefono}}"

🔵 Regla 3: El evaluator debe procesar todos los TextNode del AST
  🟢 Ejemplo 3.1: AST con 3 TextNode, cada uno con variables → los 3 resueltos
  🟢 Ejemplo 3.2: AST con TextNode sin variables → sin cambios, sin error

🔴 Pregunta: ¿Se soportan filtros/pipes en variables? Ej: {{total | currency}}
🔴 Pregunta: ¿Hay límite de profundidad para nested properties?
🔴 Pregunta: ¿Qué formato de fecha se usa para variables de tipo DateTime?
```

**Resultado de la sesión:** Si las preguntas rojas se resuelven, la historia está lista (cumple el DoR). Si no, se agenda un spike o se escala al PO.

> 📎 Referencia: `docs/06_backlog-tecnico/definition-of-ready_v1.0.md` — criterios que Example Mapping ayuda a cumplir

#### Cuándo usarlo

- En la **sesión de refinamiento**, 1-2 sprints antes de comprometer la historia
- Cuando los **criterios de aceptación son vagos** o están expresados como "funcionar correctamente"
- Cuando hay **muchas reglas de negocio** que interactúan y se necesita desambiguar
- Cuando el **PO y el equipo** no están alineados sobre el scope exacto de una historia

#### Ejemplo en contexto web — E-commerce: "Aplicar cupón de descuento"

```
🟡 US: Como comprador, quiero aplicar un cupón de descuento en el checkout,
       para pagar menos por mi pedido

🔵 Regla 1: El cupón debe ser válido (existe, no expirado, no agotado)
  🟢 Ejemplo 1.1: Cupón "VERANO20" vigente → se aplica 20% de descuento al total
  🟢 Ejemplo 1.2: Cupón "INVIERNO10" expirado hace 2 días → error "Cupón expirado"
  🟢 Ejemplo 1.3: Cupón "UNICO50" ya usado por este usuario → error "Cupón ya utilizado"

🔵 Regla 2: El cupón tiene monto mínimo de compra
  🟢 Ejemplo 2.1: Cupón requiere mínimo $5000, carrito tiene $6000 → se aplica
  🟢 Ejemplo 2.2: Cupón requiere mínimo $5000, carrito tiene $3000 → error "Monto mínimo no alcanzado ($5000)"

🔵 Regla 3: Solo un cupón por pedido
  🟢 Ejemplo 3.1: Carrito sin cupón, aplico "VERANO20" → se aplica
  🟢 Ejemplo 3.2: Carrito ya tiene "VERANO20", aplico "ENVIOGRATIS" → reemplaza el anterior con confirmación

🔵 Regla 4: Tipos de descuento (porcentaje vs. monto fijo)
  🟢 Ejemplo 4.1: Cupón "20OFF" (20%) en carrito de $10.000 → descuento de $2.000
  🟢 Ejemplo 4.2: Cupón "1000MENOS" ($1.000 fijo) en carrito de $10.000 → descuento de $1.000
  🟢 Ejemplo 4.3: Cupón "90OFF" (90%) en carrito de $500 → descuento de $450 (no puede ser mayor al total)

🔴 Pregunta: ¿Los cupones aplican a productos individuales o al total del carrito?
🔴 Pregunta: ¿Se pueden combinar cupones con ofertas de temporada?
🔴 Pregunta: ¿El descuento se calcula antes o después del costo de envío?
```

**Resultado:** Esta sesión de 25 minutos reveló 3 preguntas que el PO no había considerado. Sin Example Mapping, estas ambigüedades se hubieran descubierto durante el desarrollo, generando retrabajo.

---

### 16.7 — Spike / Architectural Spike

#### Qué es

Un **spike** es un timebox de investigación (típicamente 1-3 días o un porcentaje del sprint) cuyo objetivo es **reducir incertidumbre** técnica o funcional antes de comprometer una historia de usuario. El término viene de Extreme Programming (XP).

A diferencia de una user story, un spike **no entrega funcionalidad** — entrega **conocimiento**: una decisión documentada (ADR), un Proof of Concept descartable, un informe con hallazgos.

#### Tipos de spike

| Tipo | Pregunta que responde | Output esperado |
|---|---|---|
| **Funcional** | ¿Se puede hacer X con la tecnología Y? | PoC descartable + informe |
| **Arquitectónico** | ¿Cómo se integra el componente A con B? | ADR (Architecture Decision Record) |
| **De performance** | ¿El approach elegido escala a N registros? | Benchmark + métricas |
| **De UX** | ¿Los usuarios entienden la interfaz propuesta? | Resultados de test de usabilidad |

#### Ejemplo del proyecto Motor DSL

Un spike relevante para el proyecto sería evaluar la viabilidad de renderizado PDF:

```
┌──────────────────────────────────────────────────────────────┐
│  SPIKE: Evaluar viabilidad de rendering PDF nativo           │
├──────────────────────────────────────────────────────────────┤
│  ID:        SP-01                                            │
│  Tipo:      Arquitectónico / Funcional                       │
│  Timebox:   2 días (16 horas)                                │
│  Pregunta:  ¿Se puede generar PDF desde .NET sin             │
│             dependencias de terceros costosas?                │
│  Output:    ADR con decisión Go/No-Go para CU-09             │
│  Criterio:  Go si existe librería open-source estable;       │
│             No-Go si requiere licencia comercial > USD 500    │
├──────────────────────────────────────────────────────────────┤
│  Resultado: Decisión documentada — PDF condicional           │
│             (se implementa solo si es viable sin terceros)    │
└──────────────────────────────────────────────────────────────┘
```

> 📎 Referencia: `docs/05_arquitectura_tecnica/decisiones-arquitectura_v1.0.md`

#### Formato de un spike en el backlog

| ID | Tipo | Descripción | Timebox | Output esperado |
|---|---|---|---|---|
| SP-01 | Spike Arquitectónico | Evaluar viabilidad PDF nativo en .NET | 2 días | ADR con decisión Go/No-Go |
| SP-02 | Spike de Performance | Benchmark de parsing para templates > 100 nodos | 1 día | Informe con métricas |
| SP-03 | Spike Funcional | Evaluar impresión Bluetooth desde .NET MAUI | 2 días | PoC + informe de compatibilidad |

#### Regla clave

Un spike **NO produce código productivo** — produce conocimiento. El código productivo se implementa en una user story separada que se crea después del spike, ya con la incertidumbre resuelta. Si un spike produce código que se sube a producción, no era un spike — era una historia mal clasificada.

#### Ejemplos de spikes en contexto web

| ID | Dominio | Tipo | Pregunta a responder | Timebox | Output esperado |
|---|---|---|---|---|---|
| SP-W01 | E-commerce | Funcional | ¿Stripe soporta pagos recurrentes con tarjetas argentinas sin 3D Secure? | 2 días | Informe de compatibilidad + PoC descartable |
| SP-W02 | SaaS | Performance | ¿Elasticsearch responde en < 200ms para búsquedas full-text sobre 500K documentos? | 1 día | Benchmark con métricas y gráficos |
| SP-W03 | App mobile | Arquitectónico | ¿Se puede implementar auth con OAuth2 + PKCE en React Native sin librería de terceros? | 2 días | ADR con decisión Go/No-Go + flujo documentado |
| SP-W04 | Fintech | De seguridad | ¿El approach de tokenización cumple con PCI DSS nivel 2 sin certificación adicional? | 3 días | Informe de compliance + checklist de requisitos |
| SP-W05 | Red social | Performance | ¿WebSockets escala a 10K conexiones concurrentes con nuestro tier de AWS? | 2 días | Resultados de load test + estimación de costos |

**Patrón común:** Cada spike tiene una **pregunta concreta** y un **output medible**. Si el spike no tiene pregunta clara, no es un spike — es una tarea de investigación sin foco.

---

### 16.8 — ATDD (Acceptance Test-Driven Development)

#### Qué es

**ATDD** (Acceptance Test-Driven Development) es una extensión del TDD donde los **criterios de aceptación** de la user story se escriben como **tests ejecutables ANTES de codear**. Los tests los co-escribe el trío: PO + Dev + QA (las "tres amigos").

La diferencia fundamental con TDD clásico:

| Aspecto | TDD | ATDD |
|---|---|---|
| Quién escribe los tests | El desarrollador | PO + Dev + QA (colaborativo) |
| Tipo de test | Unitario (técnico) | De aceptación (funcional) |
| Qué guía | El diseño del código | El comportamiento del sistema |
| Nivel de abstracción | Método / clase | Historia de usuario completa |
| Lenguaje | Código técnico | Given/When/Then (Gherkin) |
| Complementarios | Sí — ATDD no reemplaza TDD | Sí — TDD no reemplaza ATDD |

#### Flujo ATDD

```
  ┌─────────────────────────────┐
  │  1. PO + Dev + QA definen   │
  │     criterios de aceptación │
  │     en Given/When/Then      │
  └─────────────┬───────────────┘
                │
  ┌─────────────▼───────────────┐
  │  2. QA/Dev escriben test    │
  │     automatizado (ROJO)     │
  └─────────────┬───────────────┘
                │
  ┌─────────────▼───────────────┐
  │  3. Dev implementa el       │
  │     mínimo para pasar       │
  │     el test (VERDE)         │
  └─────────────┬───────────────┘
                │
  ┌─────────────▼───────────────┐
  │  4. Refactor sin romper     │
  │     el test de aceptación   │
  └─────────────┬───────────────┘
                │
  ┌─────────────▼───────────────┐
  │  5. Criterio de aceptación  │
  │     cumplido ✅              │
  └─────────────────────────────┘
```

#### Ejemplo del proyecto — US-03: Parser DSL básico

Criterio de aceptación de la US-03 expresado como test ATDD:

```
Feature: Parser DSL básico

  Scenario: Parsear un template con un solo TextNode
    Given un template JSON DSL válido con un nodo de tipo "text"
      """
      {
        "type": "document",
        "children": [
          { "type": "text", "content": "Hola mundo" }
        ]
      }
      """
    When el parser procesa el template
    Then el AST resultante contiene un DocumentNode raíz
    And el DocumentNode tiene exactamente 1 hijo de tipo TextNode
    And el TextNode tiene content igual a "Hola mundo"

  Scenario: Rechazar un template JSON malformado
    Given un template JSON DSL con sintaxis inválida
      """
      { "type": "document", "children": [ }
      """
    When el parser procesa el template
    Then se lanza un error de tipo ParseException
    And el mensaje de error indica la posición del error de sintaxis
```

La estrategia de testing del proyecto define una pirámide de 70% tests unitarios, 20% tests de integración y 10% tests end-to-end. ATDD opera principalmente en el nivel de **integración y E2E**, complementando los tests unitarios que escribe el desarrollador con TDD.

> 📎 Referencia: `docs/08_calidad_y_pruebas/estrategia-testing-motor_v1.0.md`

#### Ejemplo en contexto web — E-commerce: Aplicar cupón de descuento

```
Feature: Aplicar cupón de descuento en el checkout

  Scenario: Aplicar cupón porcentual válido
    Given que tengo un carrito con 2 productos por un total de $10.000
    And existe un cupón "VERANO20" vigente con 20% de descuento
    When ingreso el código "VERANO20" en el campo de cupón
    And hago clic en "Aplicar"
    Then el total del carrito se actualiza a $8.000
    And se muestra el mensaje "Cupón VERANO20 aplicado: -$2.000"
    And el botón "Aplicar" cambia a "Quitar cupón"

  Scenario: Rechazar cupón expirado
    Given que tengo un carrito con productos
    And el cupón "INVIERNO10" expiró el 2026-03-31
    When ingreso el código "INVIERNO10" y hago clic en "Aplicar"
    Then el total del carrito NO cambia
    And se muestra el error "Este cupón expiró el 31/03/2026"

  Scenario: Rechazar cupón con monto mínimo no alcanzado
    Given que tengo un carrito con un total de $3.000
    And el cupón "DESCUENTO50" requiere un mínimo de $5.000
    When ingreso el código "DESCUENTO50" y hago clic en "Aplicar"
    Then el total del carrito NO cambia
    And se muestra el error "Este cupón requiere una compra mínima de $5.000"

  Scenario: Reemplazar cupón existente
    Given que tengo el cupón "VERANO20" aplicado en mi carrito de $10.000
    When ingreso un nuevo código "ENVIOGRATIS" y hago clic en "Aplicar"
    Then se muestra un diálogo "Ya tenés un cupón aplicado. ¿Querés reemplazarlo?"
    When confirmo el reemplazo
    Then el cupón "VERANO20" se remueve
    And el cupón "ENVIOGRATIS" se aplica
    And el total se recalcula con el nuevo descuento
```

**Valor del ATDD en este caso:** Estos tests se escribieron en el refinamiento con el PO, el dev y el QA juntos. Cuando el desarrollador empieza a codear, ya tiene una especificación ejecutable. Cuando el QA testea, usa exactamente estos escenarios. No hay ambigüedad.

---

### 16.9 — Mob Programming

#### Qué es

**Mob Programming** (o "programming mob") es una práctica donde **todo el equipo** trabaja en el mismo problema, en la misma máquina, al mismo tiempo. Una persona actúa como **driver** (escribe código) y el resto son **navigators** (guían las decisiones). El rol de driver rota cada **10-15 minutos** usando un timer.

La idea central es que la comunicación más eficiente ocurre cuando todos trabajan en el mismo contexto al mismo tiempo, eliminando las demoras por handoffs, code reviews asíncronos y reuniones de alineación.

#### Cuándo usarlo

| Situación | ¿Mob Programming? | Justificación |
|---|---|---|
| Onboarding de nuevos miembros | ✅ Sí | El nuevo miembro aprende el codebase en contexto |
| Decisión de arquitectura cross-cutting | ✅ Sí | Todos deben entender y comprometerse con la decisión |
| Debugging de un problema complejo | ✅ Sí | Múltiples perspectivas aceleran el diagnóstico |
| Definición de interfaces compartidas | ✅ Sí | Evita retrabajo por contratos mal comunicados |
| Tarea rutinaria bien conocida | ❌ No | Una sola persona es más eficiente |
| Tareas paralelizables sin dependencias | ❌ No | Se desperdicia capacidad del equipo |
| Trabajo individual que requiere concentración | ❌ No | El mob puede ser disruptivo |

#### Ejemplo hipotético del proyecto Motor DSL

El equipo hace una sesión de mob programming de 2 horas para definir la interfaz `IDocumentRenderer`, porque todos los renderers del proyecto dependen de ella:

```
Sesión de Mob Programming: Definir IDocumentRenderer
──────────────────────────────────────────────────────
Participantes: 4 desarrolladores + 1 QA
Duración: 2 horas (rotación cada 15 min = 8 rotaciones)
Artefacto: Interfaz IDocumentRenderer + tests de contrato

Resultado:
  - Interfaz acordada por todo el equipo
  - 4 implementaciones planificadas: EscPosRenderer, TextRenderer,
    UiRenderer, PdfRenderer
  - Tests de contrato que toda implementación debe pasar
  - 0 ambigüedades → se evitó retrabajo posterior

Sin mob, este diseño hubiera requerido:
  - 1 dev diseña la interfaz (2h)
  - Code review (1h de espera + 30min de review)
  - Feedback → cambios → re-review (2h)
  - Stand-up para alinear al equipo (15min × 3 días)
  Total: ~8h distribuidas en 3 días vs. 2h concentradas
```

> 📎 Referencia: `docs/05_arquitectura_tecnica/contratos-del-motor_v1.0.md` — contratos e interfaces del motor

#### Ejemplo en contexto web — Startup SaaS: Definir API REST pública

Una startup SaaS está construyendo una plataforma de gestión de inventario. La API REST será consumida por 3 clientes: la app web (React), la app mobile (React Native) y partners que integran vía API. El equipo decide hacer mob programming para definir el contrato de la API:

```
Sesión de Mob Programming: Definir API REST pública v1
────────────────────────────────────────────────────────
Participantes: 2 devs backend + 1 dev frontend + 1 dev mobile + 1 QA
Duración: 3 horas (rotación cada 15 min = 12 rotaciones)
Artefactos: Especificación OpenAPI 3.0 + colección Postman + tests de contrato

Resultado:
  - 12 endpoints definidos con request/response schemas
  - Convenciones acordadas: paginación cursor-based, errores RFC 7807,
    versionado por header (Accept: application/vnd.inventory.v1+json)
  - El dev frontend validó que los payloads tienen la forma que necesita
  - El dev mobile identificó 2 campos que necesita y que el backend
    no había considerado
  - El QA generó la colección Postman con ejemplos
  - 0 sorpresas en integración → se evitaron 2-3 días de ida y vuelta

Sin mob, este diseño hubiera requerido:
  - Backend diseña API solo (4h)
  - Documenta en Swagger (2h)
  - Frontend revisa y pide cambios (1 día de espera + 2h de cambios)
  - Mobile revisa y pide más cambios (otro día de espera + 1h)
  Total: ~12h distribuidas en 3-4 días vs. 3h concentradas
```

**Por qué funciona:** La API es un contrato compartido. Si se diseña en aislamiento, los consumidores descubren problemas tarde. El mob asegura que todos los consumidores validan el contrato en tiempo real.

---

### 16.10 — Shape Up (Basecamp)

#### Qué es

**Shape Up** es un framework alternativo a Scrum creado por Basecamp (Ryan Singer, 2019). Se basa en ciclos de **6 semanas** (no sprints de 2) con un **appetite** (presupuesto de tiempo) fijo. El equipo tiene autonomía total sobre cómo entregar dentro del appetite — no hay daily standups, no hay sprint planning detallado, no hay burndown charts.

#### Conceptos clave

| Concepto | Descripción |
|---|---|
| **Appetite** | Cuánto tiempo estamos **dispuestos** a invertir en una funcionalidad. No es una estimación (cuánto tardará) sino una **decisión** (cuánto vale invertir). |
| **Pitch** | Propuesta de funcionalidad que incluye: problema, solución resumida, rabbit holes identificados y no-gos explícitos. |
| **Betting Table** | El liderazgo "apuesta" en qué pitches invertir en el próximo ciclo. No hay backlog permanente — los pitches que no se seleccionan se descartan. |
| **Cooldown** | 2 semanas entre ciclos para deuda técnica, exploración libre, bug fixing, capacitación. |
| **Hill Chart** | Visualización del progreso con una "colina": subiendo = descubriendo (fase de incertidumbre), bajando = ejecutando (fase de certeza). |

#### Tabla comparativa Shape Up vs. Scrum

| Aspecto | Scrum | Shape Up |
|---|---|---|
| Cadencia | 1-4 semanas (sprint) | 6 semanas (cycle) + 2 cooldown |
| Estimación | Story Points / Planning Poker | Appetite (presupuesto de tiempo) |
| Roles formales | PO, SM, Dev Team | Shapers, Builders |
| Backlog | Product Backlog mantenido y priorizado | No hay backlog permanente — pitches frescos cada ciclo |
| Ceremonias | 5 eventos formales (Planning, Daily, Review, Retro, Refinement) | Betting table + kickoff |
| Tracking | Burndown chart, velocity | Hill chart |
| Autonomía del equipo | Media (sprint scope fijo, daily obligatorio) | Alta (cómo entregar es decisión del equipo) |
| Scope del trabajo | User stories estimadas | Pitches con appetite y no-gos |
| Manejo de riesgos | Impediments → SM los remueve | Rabbit holes → identificados en el pitch |

#### Cuándo considerar Shape Up

- Equipos **maduros y autónomos** que sienten que Scrum tiene demasiada ceremonia
- Productos **estables** donde las funcionalidades son grandes y autocontenidas
- Equipos **pequeños** (2-3 personas) donde los roles formales de Scrum no tienen sentido
- Organizaciones que quieren **eliminar el backlog infinito** y forzar decisiones frescas cada ciclo

#### Cuándo NO usar Shape Up

- Equipos nuevos que necesitan la estructura de Scrum para aprender a ser ágiles
- Proyectos con requisitos regulatorios estrictos que necesitan trazabilidad detallada
- Equipos grandes (> 8 personas) donde la coordinación requiere ceremonias explícitas

#### Ejemplo en contexto web — Pitch de Shape Up para notificaciones push (SaaS)

```
┌─────────────────────────────────────────────────────────────┐
PITCH: Notificaciones push configurables
├─────────────────────────────────────────────────────────────┤
│ PROBLEMA:                                                   │
│ Los usuarios se quejan de recibir demasiados emails de       │
│ la plataforma. El 30% desactiva las notificaciones por       │
│ email, perdiendo alertas importantes (facturas, vencimientos)│
├─────────────────────────────────────────────────────────────┤
│ APPETITE: 6 semanas (1 ciclo)                                │
├─────────────────────────────────────────────────────────────┤
│ SOLUCIÓN:                                                    │
│ - Sistema de notificaciones push (browser + mobile)          │
│ - Panel de preferencias donde el usuario elige qué recibir   │
│ - 3 canales: push, email, in-app (el usuario elige)          │
│ - Templates de notificación editables por el admin            │
├─────────────────────────────────────────────────────────────┤
│ RABBIT HOLES (evitar):                                       │
│ - NO construir un editor visual de templates (demasiado)     │
│ - NO soportar SMS en este ciclo (integración costosa)        │
│ - NO hacer analíticas de apertura (complejidad de tracking)   │
├─────────────────────────────────────────────────────────────┤
│ NO-GOS (explícitamente fuera de scope):                       │
│ - Notificaciones transaccionales (facturas) → otro ciclo     │
│ - Integración con Slack/Teams → otro ciclo                    │
└─────────────────────────────────────────────────────────────┘
```

**Diferencia clave con Scrum:** En Scrum, este trabajo se descompondría en 15-20 user stories estimadas en Story Points, distribuidas en 3 sprints de 2 semanas, con daily standups y burndown charts. En Shape Up, el pitch define el **qué** y los **límites** (rabbit holes, no-gos), y el equipo de 2-3 personas decide el **cómo** con autonomía total durante 6 semanas.

---

### 16.11 — Impact Mapping (Gojko Adzic)

#### Qué es

**Impact Mapping** es una técnica de planificación estratégica que conecta los entregables técnicos con los objetivos de negocio a través de una cadena de 4 niveles: **Goal → Actors → Impacts → Deliverables**. Fue popularizada por Gojko Adzic en su libro *Impact Mapping* (2012).

La pregunta que responde es: **¿Por qué estamos construyendo esto?** Si un deliverable no se puede conectar con un goal de negocio a través de un actor y un impacto, probablemente no debería estar en el backlog.

#### Diagrama ASCII

```
                    ┌─────────────────────┐
                    │       GOAL          │
                    │ Reducir costos de   │
                    │ impresión en 40%    │
                    └─────────┬───────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
        ┌───────────┐  ┌───────────┐  ┌───────────┐
        │  ACTOR    │  │  ACTOR    │  │  ACTOR    │
        │ Desarrolla│  │ Operador  │  │ Gerente   │
        │ dor       │  │ de campo  │  │ de IT     │
        └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
              │               │               │
        ┌─────▼─────┐  ┌─────▼─────┐  ┌─────▼─────┐
        │  IMPACT   │  │  IMPACT   │  │  IMPACT   │
        │ Diseñar   │  │ Imprimir  │  │ Controlar │
        │ templates │  │ sin IT    │  │ versiones │
        │ sin código│  │           │  │           │
        └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
              │               │               │
        ┌─────▼─────┐  ┌─────▼─────┐  ┌─────▼─────┐
        │DELIVERABLE│  │DELIVERABLE│  │DELIVERABLE│
        │ Motor DSL │  │ App móvil │  │ Versionado│
        │ + editor  │  │ Bluetooth │  │ templates │
        └───────────┘  └───────────┘  └───────────┘
```

#### Ejemplo del proyecto — Impact Map desde las necesidades de negocio

Las necesidades de negocio del proyecto (NB-01 a NB-06) se mapean a un Impact Map:

| Nivel | Elemento | Conexión |
|---|---|---|
| **Goal** | Estandarizar y abaratar la generación de documentos impresos | NB-02, NB-05 |
| **Actor 1** | Desarrollador de aplicaciones | Consume la librería MotorDsl.Core |
| **Impact 1** | Diseñar templates sin hardcodear lógica de impresión | NB-01: desacoplar datos, diseño y dispositivo |
| **Deliverable 1** | Motor DSL con parser + AST + renderers | Épicas 1-7 del backlog técnico |
| **Actor 2** | Operador de campo | Usa la app final en dispositivos móviles |
| **Impact 2** | Imprimir documentos sin depender de IT | NB-04: adaptarse a múltiples dispositivos |
| **Deliverable 2** | Rendering ESC/POS + Bluetooth + perfiles de dispositivo | Épicas 7, 9 |
| **Actor 3** | Gerente de IT / Responsable técnico | Gestiona las aplicaciones de la organización |
| **Impact 3** | Reutilizar componentes entre proyectos | NB-06: habilitar reutilización |
| **Deliverable 3** | Librería NuGet empaquetada con API pública | Épica 8 (extensibilidad) |

> 📎 Referencia: `docs/01_necesidades_negocio/necesidades-negocio_v1.0.md` — NB-01 a NB-06

#### Cuándo usarlo

- **Al inicio del proyecto** para validar que los entregables técnicos se conectan con objetivos de negocio
- **En planificación de roadmap** para priorizar features por impacto real, no por complejidad técnica
- **Cuando el backlog crece** y se necesita decidir qué eliminar — si un deliverable no se conecta con un goal, es candidato a eliminación

#### Ejemplo en contexto web — Impact Map de E-commerce

```
                    ┌─────────────────────┐
                    │       GOAL          │
                    │ Aumentar conversión │
                    │ del checkout en 25% │
                    └─────────┬───────────┘
                              │
              ┌─────────────┼─────────────┐
              ▼               ▼               ▼
        ┌───────────┐  ┌───────────┐  ┌───────────┐
        │  ACTOR    │  │  ACTOR    │  │  ACTOR    │
        │ Comprador │  │ Vendedor/ │  │ Equipo    │
        │ frecuente │  │ Admin     │  │ marketing │
        └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
              │               │               │
        ┌─────▼─────┐  ┌─────▼─────┐  ┌─────▼─────┐
        │  IMPACT   │  │  IMPACT   │  │  IMPACT   │
        │ Completar │  │ Crear    │  │ Lanzar   │
        │ compra en │  │ ofertas  │  │ campañas │
        │ < 2 min   │  │ rápido   │  │ retarget │
        └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
              │               │               │
        ┌─────▼─────┐  ┌─────▼─────┐  ┌─────▼─────┐
        │DELIVERABLE│  │DELIVERABLE│  │DELIVERABLE│
        │ Checkout 1│  │ Panel de  │  │ Pixel de  │
        │ clic,     │  │ cupones y │  │ tracking +│
        │ tarjeta   │  │ descuentos│  │ integración│
        │ guardada  │  │ masivos   │  │ Meta Ads  │
        └───────────┘  └───────────┘  └───────────┘
```

**Cómo se usa en la práctica:** Si el equipo propone "Agregar chat en vivo al sitio", se valida contra el Impact Map: ¿qué actor impacta? ¿qué impacto genera? ¿se conecta con el goal de conversión? Si no se puede trazar la línea, la feature no debería estar en el backlog de este trimestre.

---

### 16.12 — Dual-Track Agile

#### Qué es

**Dual-Track Agile** separa el trabajo del equipo en dos tracks paralelos:

- **Discovery Track:** investigar **qué construir** — entrevistas con usuarios, prototipos, spikes, validación de hipótesis, análisis competitivo
- **Delivery Track:** construir **lo que ya se validó** — sprints normales con historias refinadas, código, testing, deployment

El Discovery Track alimenta al Delivery Track con **ideas validadas** que se transforman en user stories listas para el sprint. No todo lo que se investiga se construye — solo lo que pasa los criterios de validación.

#### Diagrama ASCII

```
DISCOVERY TRACK                       DELIVERY TRACK
(qué construir)                       (cómo construirlo)

┌──────────────┐                      ┌──────────────┐
│ Investigar   │                      │ Sprint N     │
│ necesidad    │──── ideas ─────►     │ US validadas │
│ del usuario  │   validadas          │              │
└──────────────┘                      └──────────────┘
┌──────────────┐                      ┌──────────────┐
│ Prototipar   │                      │ Sprint N+1   │
│ y testear    │──── specs ─────►     │ US refinadas │
│ con usuarios │   completas          │              │
└──────────────┘                      └──────────────┘
┌──────────────┐                      ┌──────────────┐
│ Analizar     │                      │ Sprint N+2   │
│ resultados   │──── ajustes ───►     │ US ajustadas │
│ y pivotar    │   de scope           │              │
└──────────────┘                      └──────────────┘

     ▲                                       │
     │           Feedback loop               │
     └───────────────────────────────────────┘
```

#### Cuándo usarlo

- **Productos nuevos** con alta incertidumbre de mercado — no se sabe si los usuarios necesitan lo que se planea construir
- Cuando el **PO necesita validar hipótesis** antes de invertir sprints de desarrollo
- Equipos con **capacidad de UX research** o Product Discovery dedicada
- Organizaciones que practican **Lean Startup** (Build-Measure-Learn)

#### Cuándo NO usarlo

- **Proyectos internos** donde los requisitos son claros y validados — como el Motor DSL, donde la necesidad de negocio ya está definida (NB-01 a NB-06)
- **Equipos pequeños** que no pueden dividirse en dos tracks sin perder capacidad de delivery
- Proyectos con **deadline fijo** donde el scope ya está comprometido y no hay margen para discovery

#### Ejemplo en contexto web — App de delivery: Discovery + Delivery en paralelo

Una app de delivery tiene dos equipos trabajando en paralelo:

```
DISCOVERY TRACK                          DELIVERY TRACK
(Product Designer + 1 Dev)               (3 Devs + 1 QA)

┌────────────────────────┐              ┌────────────────────────┐
│ Semana 1-2:            │              │ Sprint 5:              │
│ Entrevistar 10         │              │ Tracking en tiempo     │
│ repartidores:          │              │ real (ya validado en   │
│ "¿Necesitás chat con   │              │ discovery anterior)    │
│  el cliente?"          │              │                        │
└────────────┬───────────┘              └────────────────────────┘
             │
             ▼
┌────────────────────────┐
│ Semana 3-4:            │              ┌────────────────────────┐
│ Resultado: 8/10 dicen  │              │ Sprint 6:              │
│ que SÍ, pero solo para │──── specs ──►│ Chat con mensajes     │
│ mensajes predefinidos  │  validadas   │ predefinidos           │
│ (no chat libre).       │              │ (scope reducido por    │
│ Prototipo testeado ✅   │              │  discovery)            │
└────────────────────────┘              └────────────────────────┘
```

**Qué hubiera pasado sin Dual-Track:** El equipo hubiera construido un chat libre completo (Sprint 5 + 6), consumiendo 4 semanas. Con discovery, descubrieron que los repartidores solo necesitan **mensajes predefinidos** ("Estoy en la puerta", "No encuentro la dirección"), reduciendo el scope a 1 sprint y ahorrando 2 semanas de desarrollo innecesario.

---

### 16.13 — Tabla resumen: Cuándo usar cada técnica

La siguiente tabla cruza situaciones típicas de proyecto con las técnicas más recomendadas para cada una:

| Situación del proyecto | Técnicas recomendadas | Por qué |
|---|---|---|
| Inicio de proyecto, mucha incertidumbre arquitectónica | Walking Skeleton + Spike + Impact Mapping | Validar arquitectura y conectar con negocio antes de comprometer sprints |
| Backlog desordenado, scope poco claro | Story Mapping + Vertical Slicing | Visualizar el flujo completo y cortar en slices entregables |
| Historias vagas, muchas reglas de negocio | Example Mapping + ATDD | Clarificar con ejemplos concretos y tests ejecutables |
| Equipo nuevo, poca experiencia compartida | Mob Programming + Walking Skeleton | Construir conocimiento compartido mientras se valida la arquitectura |
| Producto maduro, funcionalidades grandes | Shape Up o Vertical Slicing | Ciclos largos con autonomía o cortes incrementales |
| Producto nuevo, incertidumbre de mercado | Dual-Track Agile + Impact Mapping | Validar hipótesis antes de construir |
| Sprint planning recurrente, refinamiento | Example Mapping + Story Mapping | Técnicas específicas para sesiones de refinamiento |
| Decisiones técnicas con alto riesgo | Spike + Tracer Bullet | Investigar y validar antes de comprometer |
| Deuda técnica acumulada | Shape Up (cooldown) + Vertical Slicing | Ciclos dedicados a deuda o slices que incluyan refactoring |

**Nota:** Estas técnicas no son mutuamente excluyentes. Un equipo maduro típicamente combina varias según la fase del proyecto y la naturaleza del trabajo. Lo importante es elegir conscientemente en lugar de aplicar Scrum "de libro" sin complementarlo con prácticas de descomposición y planificación.

---

## Sección 17 — Guía práctica para arrancar un proyecto ágil

Esta sección responde una pregunta que los documentos teóricos raramente abordan con claridad: **¿por dónde empiezo?** Conocer Scrum, Kanban o las técnicas de la Sección 16 no es suficiente si no existe un criterio claro para elegir qué aplicar, en qué orden y con qué checklist validar que el equipo está listo para ejecutar.

La guía está organizada en cinco pasos secuenciales. Se puede usar al inicio de cualquier proyecto nuevo o al re-arrancar un proyecto que perdió el rumbo.

---

### 17.1 — Preguntas de diagnóstico inicial

Antes de nombrar roles, crear tableros o escribir user stories, el equipo necesita responder estas preguntas. Las respuestas determinan qué metodología elegir y qué artefactos construir primero.

**Sobre el equipo:**

| Pregunta | Por qué importa |
|---|---|
| ¿Cuántas personas tiene el equipo? | Equipos < 10 personas: Scrum o Kanban. Equipos > 10: considerar SAFe o LeSS. |
| ¿Hay una persona que pueda actuar como Product Owner real (con poder de decisión)? | Sin PO real, Scrum no funciona. Si no existe, Kanban es más honesto. |
| ¿El equipo tiene experiencia previa en ágil? | Equipos nuevos en ágil necesitan más estructura (Scrum). Equipos maduros pueden usar Kanban o Scrumban. |
| ¿El equipo es estable o tiene rotación frecuente? | Equipos con rotación alta se benefician de Kanban (menos dependencia de velocity acumulada). |

**Sobre el trabajo:**

| Pregunta | Por qué importa |
|---|---|
| ¿Es un proyecto nuevo o mantenimiento de un sistema existente? | Proyectos nuevos → Scrum (sprints, backlog, releases). Mantenimiento/soporte → Kanban (flujo continuo). |
| ¿La demanda de trabajo es predecible (features planificadas) o variable (bugs, requests ad-hoc)? | Demanda predecible → Scrum. Demanda variable e impredecible → Kanban. |
| ¿Cuánta incertidumbre técnica existe al inicio? | Alta incertidumbre → Sprint 0 con Spikes y Walking Skeleton antes de comprometer sprints. |
| ¿El cliente/stakeholder puede participar regularmente en reviews? | Si no puede, las Sprint Reviews no agregan valor. Ajustar cadencia o canal de feedback. |

**Sobre el producto:**

| Pregunta | Por qué importa |
|---|---|
| ¿Existe una visión clara del producto? | Sin visión, no se puede priorizar el backlog. Definirla es el paso cero. |
| ¿Se conoce el alcance inicial aunque sea a grandes rasgos? | Permite construir épicas e identificar dependencias antes del primer sprint. |
| ¿Hay una fecha de entrega comprometida externamente? | Fecha fija + scope fijo = problema. En ágil se negocia scope, no fecha. |

---

### 17.2 — Criterios para elegir la metodología

Con las respuestas del diagnóstico, usar esta tabla para elegir la metodología base:

| Condición del proyecto | Metodología recomendada | Razón principal |
|---|---|---|
| Proyecto nuevo, equipo estable, PO disponible, sprints de 2 semanas | **Scrum** | Framework completo con ceremonias y artefactos claros para construir producto iterativamente |
| Mantenimiento, soporte o bugs continuos, demanda impredecible | **Kanban** | Flujo continuo con WIP limits es más efectivo que comprometer sprints con demanda variable |
| Quiero sprints pero también más flexibilidad de flujo | **Scrumban** | Combina timebox de Scrum con WIP limits de Kanban |
| Proyecto técnico con alta incertidumbre inicial, equipo maduro | **Scrum + Sprint 0 + XP** | Scrum como marco + prácticas XP (TDD, pair programming) + Sprint 0 de validación técnica |
| 3 o más equipos que deben coordinarse en el mismo producto | **SAFe o LeSS** | Sin coordinación formal, los equipos se bloquean entre sí. SAFe para organizaciones grandes, LeSS para 2-8 equipos |
| Equipo sin experiencia en ágil, quiere empezar simple | **Kanban** | Menos ceremonias y roles que aprender desde el día 1. Se puede evolucionar a Scrum después. |

> **Principio:** Empezar simple y agregar complejidad solo cuando el equipo la necesite. Un equipo nuevo haciendo Kanban bien supera a un equipo haciendo Scrum a medias.

---

### 17.3 — Checklist de arranque — Sprint 0

El **Sprint 0** no es un sprint de desarrollo — es un sprint de preparación. Su objetivo es dejar al equipo listo para ejecutar el Sprint 1 sin bloquearse en las primeras horas. No produce incremento de producto, produce las condiciones para producirlo.

**Checklist de artefactos mínimos:**

| Artefacto | ¿Listo? | Criterio de completitud |
|---|---|---|
| **Visión del producto** | ☐ | Una frase que responde: ¿para quién? ¿qué problema resuelve? ¿qué lo diferencia? |
| **Épicas identificadas** | ☐ | Al menos las épicas del MVP listadas y nombradas |
| **Product Backlog inicial** | ☐ | Al menos 2 sprints de trabajo en US escritas y priorizadas (MoSCoW) |
| **Definition of Ready (DoR)** | ☐ | Criterios acordados que una US debe cumplir para entrar al sprint |
| **Definition of Done (DoD)** | ☐ | Criterios acordados que una US debe cumplir para considerarse terminada |
| **Velocity base estimada** | ☐ | Story Points que el equipo estima poder completar en el primer sprint (si no hay histórico: usar planning poker con el equipo para las primeras US) |
| **Herramienta de tracking configurada** | ☐ | Board en Jira (u otra herramienta) con columnas y WIP limits definidos |
| **Ceremonias acordadas** | ☐ | Horario fijo para Daily, Planning, Review y Retro |
| **Repositorio y entorno base** | ☐ | Repo creado, ramas configuradas, CI/CD básico operativo |
| **Spike técnico completado (si aplica)** | ☐ | Si hay incertidumbre arquitectónica alta, el Spike resuelve antes del Sprint 1 |

**El Sprint 0 está completo cuando todos los ítems marcados. Si alguno falta, el Sprint 1 empezará con deuda de proceso que se pagará en los primeros días.**

---

### 17.4 — Orden recomendado para construir los artefactos

La cadena siguiente representa el orden lógico de construcción. Cada artefacto depende del anterior. Saltear un paso genera retrabajo.

```
┌────────────────────────────────────────────────────────────────────────────┐
│              CADENA DE ARTEFACTOS — ARRANQUE DE PROYECTO ÁGIL              │
│                                                                            │
│  1. VISIÓN DEL PRODUCTO                                                    │
│     "Para [usuario], que [necesidad], [nombre del producto] es             │
│      [categoría] que [beneficio], a diferencia de [alternativa]."          │
│     Fuente: docs/00_contexto/vision-producto_v1.0.md                       │
│     ↓                                                                      │
│  2. ÉPICAS                                                                 │
│     Capacidades grandes del producto (multi-sprint).                       │
│     Cada épica agrupa un conjunto de US relacionadas.                      │
│     ↓                                                                      │
│  3. STORY MAP                                                              │
│     Visualizar el flujo completo del usuario recorriendo las épicas.       │
│     Identificar el MVP (capa mínima horizontal del mapa).                  │
│     Técnica: User Story Mapping (Sección 16.5)                             │
│     ↓                                                                      │
│  4. USER STORIES PRIORIZADAS (MoSCoW)                                     │
│     Descomponer épicas en US. Clasificar Must / Should / Could / Won't.    │
│     Solo las Must Have van al primer bloque de sprints.                    │
│     Fuente: docs/06_backlog-tecnico/product-backlog_v1.0.md                │
│     ↓                                                                      │
│  5. DoR Y DoD DEFINIDOS                                                    │
│     Definition of Ready: qué debe tener una US para entrar al sprint.     │
│     Definition of Done: qué debe cumplir para considerarse terminada.      │
│     Fuente: docs/06_backlog-tecnico/definition-of-ready_v1.0.md            │
│             docs/08_calidad_y_pruebas/definition-of-done_v1.0.md           │
│     ↓                                                                      │
│  6. REFINAMIENTO INICIAL (Grooming)                                        │
│     Refinar las US del primer sprint: criterios de aceptación,             │
│     estimación en Story Points, descomposición en tareas técnicas.         │
│     Técnicas: Example Mapping (16.6), Planning Poker                       │
│     ↓                                                                      │
│  7. SPRINT 0 — VALIDACIÓN TÉCNICA                                          │
│     Walking Skeleton: arquitectura mínima de extremo a extremo.            │
│     Spikes: resolver incertidumbres técnicas críticas.                     │
│     Configurar entorno, CI/CD, repositorio, herramienta de tracking.       │
│     ↓                                                                      │
│  8. SPRINT 1 — PRIMER INCREMENTO                                           │
│     Sprint Planning → Ejecución → Review → Retro                          │
│     El equipo tiene todo lo necesario para ejecutar sin bloquearse         │
│     en preguntas de proceso o de arquitectura.                             │
└────────────────────────────────────────────────────────────────────────────┘
```

**Tiempo estimado para completar pasos 1-7 (Sprint 0):**

| Tamaño del proyecto | Duración del Sprint 0 |
|---|---|
| Proyecto pequeño (1-3 personas, scope acotado) | 2-3 días |
| Proyecto mediano (4-8 personas, scope moderado) | 1 semana |
| Proyecto grande (9+ personas, múltiples épicas) | 1-2 semanas (puede ser un sprint completo) |

---

### 17.5 — Errores frecuentes al iniciar un proyecto ágil

Estos son los errores más habituales en los primeros sprints. Cada uno tiene síntoma observable, causa raíz y corrección.

| Error | Síntoma observable | Causa raíz | Corrección |
|---|---|---|---|
| **Sin Definition of Done desde el día 1** | Las US se "terminan" pero nadie sabe si están realmente terminadas. La deuda técnica crece silenciosamente. | El equipo asumió que "terminado" era obvio. | Definir el DoD en el Sprint 0. Incluir: código revisado, tests verdes, docu actualizada, CI verde. |
| **Backlog vacío o insuficiente en el primer Sprint Planning** | El Sprint Planning dura 4 horas y no se llega a acuerdo sobre qué commitear. | No hubo refinamiento previo. Las US no cumplen el DoR. | Siempre entrar al Sprint Planning con las US del sprint refinadas y estimadas (DoR cumplido). |
| **PO proxy sin poder de decisión real** | Las preguntas de negocio quedan sin respuesta durante el sprint. El equipo toma decisiones a ciegas o se bloquea esperando aprobación. | Se asignó un "representante" del negocio que no puede decidir. | Escalar el problema: el ágil no funciona sin un PO con autoridad real. Si no es posible, documentar el riesgo explícitamente. |
| **Estimación sin referencia ni calibración** | El Sprint 1 se compromete con 60 SP y se completan 20. El equipo queda desmoralizado. | No existía velocity histórica ni se calibró con el equipo antes del primer sprint. | En el Sprint 0, usar Planning Poker sobre 3-5 US reales para establecer una escala de referencia. Comprometer conservadoramente el Sprint 1. |
| **Saltarse el Sprint 0** | El equipo arranca el Sprint 1 sin entorno configurado, sin DoD, sin backlog refinado y sin criterios de estimación. Los primeros 3 días se pierden en setup. | Urgencia por "empezar a desarrollar ya". | El Sprint 0 no es pérdida de tiempo — es inversión. Un Sprint 0 bien hecho ahorra 2-3 sprints de retrabajo. |
| **Demasiados ítems en el primer sprint (overcommitment)** | El sprint termina con 40% de las historias incompletas. El equipo siente que falló. | Optimismo en la estimación, sin data histórica. | Reducir el compromiso inicial en un 20-30% respecto a lo que el equipo cree que puede hacer. La velocity real se ajusta en sprints siguientes. |
| **No realizar la Retrospectiva en el Sprint 1** | Los problemas del primer sprint se repiten en el segundo y el tercero. | "No tenemos tiempo" o "fue el primer sprint, no hay mucho que decir". | La Retro del Sprint 1 es la más importante. Es cuando el equipo establece sus normas de trabajo. Nunca saltearla. |
| **Métricas desde el día 1 sin contexto** | El PO exige velocity alta en el primer sprint y presiona al equipo cuando no se cumple. | Confundir métricas de mejora (para el equipo) con métricas de control (para la gerencia). | Explicar que la velocity de los primeros 3 sprints es de calibración, no de producción. No usarla para evaluar al equipo. |

---

## Control de Cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-04-13 | Versión inicial — documento completo de referencia |
| 1.1 | 2026-04-13 | Agregada Sección 16 — Técnicas complementarias de descomposición y planificación ágil |
| 1.2 | 2026-04-13 | Ampliada Sección 6 — Definiciones, técnicas de redacción y ejemplos en contextos web |
| 1.3 | 2026-04-13 | Agregado índice completo de secciones y subsecciones en la cabecera |
| 1.4 | 2026-04-13 | Agregados ejemplos en contextos web y otros dominios a la Sección 16 |
| 1.5 | 2026-04-14 | Agregada Sección 17 — Guía práctica para arrancar un proyecto ágil |

---

**Fin del documento**
