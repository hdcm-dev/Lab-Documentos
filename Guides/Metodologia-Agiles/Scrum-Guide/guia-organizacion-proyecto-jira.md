# Guía Práctica: Cómo Organizar un Proyecto de Software con Jira

**Versión:** 1.0  
**Fecha:** 2026-04-14  
**Basado en:** `devs/metodos-agiles/metodologias-agiles-jira.md` — Repositorio Ejemplo_IA_SDD_Template  
**Propósito:** Guía paso a paso para dar de alta y organizar un proyecto de desarrollo de software usando Jira, con ejemplos en distintos ámbitos.

---

## Índice

- [Parte 1 — Elementos Típicos de Jira](#parte-1--elementos-típicos-de-jira)
  - [1.1 Conceptos base](#11-conceptos-base)
  - [1.2 Tipos de issue](#12-tipos-de-issue)
  - [1.3 Jerarquía del trabajo](#13-jerarquía-del-trabajo)
  - [1.4 Campos clave de cada issue](#14-campos-clave-de-cada-issue)
  - [1.5 Prioridades y mapeo a MoSCoW](#15-prioridades-y-mapeo-a-moscow)
  - [1.6 Estados y workflow](#16-estados-y-workflow)
  - [1.7 Métricas y reportes](#17-métricas-y-reportes)
  - [1.8 JQL — Jira Query Language](#18-jql--jira-query-language)
- [Parte 2 — Metodologías Propuestas por Jira](#parte-2--metodologías-propuestas-por-jira)
  - [2.1 Scrum](#21-scrum)
  - [2.2 Kanban](#22-kanban)
  - [2.3 Cuándo elegir cada una](#23-cuándo-elegir-cada-una)
  - [2.4 Técnicas complementarias de descomposición](#24-técnicas-complementarias-de-descomposición)
- [Parte 3 — Paso a Paso: Dar de Alta un Proyecto](#parte-3--paso-a-paso-dar-de-alta-un-proyecto)
  - [3.1 Crear el proyecto](#31-crear-el-proyecto)
  - [3.2 Configurar el board](#32-configurar-el-board)
  - [3.3 Definir épicas](#33-definir-épicas)
  - [3.4 Crear user stories](#34-crear-user-stories)
  - [3.5 Descomponer en tareas y sub-tareas](#35-descomponer-en-tareas-y-sub-tareas)
  - [3.6 Configurar releases y hitos](#36-configurar-releases-y-hitos)
  - [3.7 Configurar componentes](#37-configurar-componentes)
  - [3.8 Planificar y ejecutar sprints](#38-planificar-y-ejecutar-sprints)
  - [3.9 Flujo de trabajo diario](#39-flujo-de-trabajo-diario)
  - [3.10 Integraciones recomendadas](#310-integraciones-recomendadas)
- [Parte 4 — Ejemplos Completos en Distintos Ámbitos](#parte-4--ejemplos-completos-en-distintos-ámbitos)
  - [4.1 E-commerce (Web)](#41-e-commerce-web)
  - [4.2 App mobile (Delivery)](#42-app-mobile-delivery)
  - [4.3 SaaS B2B (CRM)](#43-saas-b2b-crm)
  - [4.4 Motor DSL (Librería/Framework)](#44-motor-dsl-libreríaframework)

---

## Parte 1 — Elementos Típicos de Jira

### 1.1 Conceptos base

Jira es una herramienta de gestión de proyectos desarrollada por Atlassian. Es la plataforma más usada en la industria del software para tracking de tareas, gestión de backlogs, planificación de sprints y seguimiento de bugs. Estos son los conceptos fundamentales que hay que entender antes de trabajar con la herramienta:

| Concepto | Definición |
|---|---|
| **Proyecto** | Contenedor de todo el trabajo. Tiene una clave única (ej: `SHOP`) que prefija todos los issues. |
| **Board** | Vista visual del trabajo. Puede ser Scrum (con sprints) o Kanban (flujo continuo). |
| **Backlog** | Vista de lista de todos los issues del proyecto, priorizados por orden. |
| **Sprint** | Iteración timeboxed en un proyecto Scrum. Se crea, se llena de issues, se inicia y se cierra. |
| **Issue** | Unidad de trabajo. Puede ser Epic, Story, Task, Sub-task o Bug. |
| **Workflow** | Flujo de estados que un issue recorre (ej: To Do → In Progress → Done). |
| **Component** | Categoría para agrupar issues por módulo o subsistema (ej: Frontend, API, Database). |
| **Fix Version** | Versión del producto donde se incluirá el issue (ej: v1.0.0). Equivale a un Release/Milestone. |

### 1.2 Tipos de issue

Jira maneja cinco tipos de issue estándar. Cada uno tiene un propósito específico y es importante no confundirlos:

**Epic:** Funcionalidad grande que abarca múltiples historias y puede extenderse a varios sprints. Se crea cuando hay un bloque de funcionalidades relacionadas que forman un objetivo de negocio coherente. Ejemplo: *"Checkout de pagos"* o *"Motor de rendering ESC/POS"*.

**Story (User Story):** Funcionalidad concreta que entrega valor al usuario y se puede completar en un solo sprint. Siempre se escribe con el formato `Como [rol], quiero [acción], para [beneficio]`. Debe estar vinculada a una Epic y tener criterios de aceptación. Ejemplo: *"Como comprador, quiero pagar con tarjeta de crédito, para completar mi compra sin efectivo"*.

**Task (Tarea técnica):** Trabajo que no tiene valor directo para el usuario final pero es necesario para el proyecto — infraestructura, configuración, spikes técnicos. Ejemplo: *"Configurar pipeline CI/CD en GitHub Actions"*.

**Sub-task:** Descomposición técnica de una Story o Task. Representa trabajo implementable asignable a una sola persona. Ejemplo: *"Crear migración para tabla `payment_transactions`"*.

**Bug:** Diferencia entre el comportamiento actual y el esperado. Debe incluir siempre pasos para reproducir, resultado esperado y resultado actual. Ejemplo: *"[Checkout] Doble cobro cuando el usuario hace doble clic en Pagar"*.

Una regla práctica para clasificar: si se puede demostrar al Product Owner en la Sprint Review y cabe en un sprint, es una Story. Si se puede demostrar pero no cabe en un sprint, es una Épica. Si no se puede demostrar porque no tiene valor visible, es una Tarea técnica.

### 1.3 Jerarquía del trabajo

El trabajo en un proyecto ágil se organiza en una jerarquía de niveles. Entender esta estructura es fundamental para mantener la trazabilidad entre los objetivos de negocio y el trabajo diario:

```
Iniciativa (Tema estratégico / OKR)
    └── Épica (funcionalidad grande, multi-sprint)
            └── User Story (valor entregable en un sprint)
                    └── Sub-tarea / Tarea técnica (trabajo implementable)
                            └── Bug (defecto encontrado)
```

| Nivel | Criterio para distinguirlo | Scope temporal |
|---|---|---|
| **Iniciativa** | Responde a un OKR o meta de negocio | Trimestral/anual |
| **Épica** | Tarda más de 1 sprint. Si tiene más de 8 historias, considerar dividirla | Multi-sprint (2-8 sprints) |
| **User Story** | Se puede demostrar en la Sprint Review, cabe en un sprint | 1 sprint |
| **Tarea técnica** | El PO no la entiende ni le importa. Siempre vinculada a una US | Horas a 2-3 días |
| **Sub-tarea** | Una sola persona la puede hacer en horas | Horas |
| **Bug** | Funcionaba y dejó de funcionar (si nunca funcionó así, es feature nueva) | Variable |

### 1.4 Campos clave de cada issue

Al crear un issue en Jira, estos son los campos que se deben completar:

| Campo | Descripción | ¿Obligatorio? |
|---|---|---|
| **Summary** | Título conciso del issue | Sí |
| **Description** | Detalle, formato US, criterios de aceptación | Sí |
| **Assignee** | Miembro del equipo responsable | Sí (durante el sprint) |
| **Reporter** | Quien creó el issue | Automático |
| **Priority** | Highest, High, Medium, Low, Lowest | Sí |
| **Labels** | Etiquetas libres para categorizar (ej: `tech-debt`, `refactor`) | Opcional |
| **Components** | Módulo del sistema (ej: Parser, Rendering, Core) | Recomendado |
| **Sprint** | Sprint al que está asignado | Sí (cuando se compromete) |
| **Story Points** | Estimación de complejidad relativa | Sí (para Stories) |
| **Fix Version** | Release donde se incluirá | Recomendado |
| **Epic Link** | Épica padre | Sí (para Stories y Tasks) |

### 1.5 Prioridades y mapeo a MoSCoW

Jira ofrece cinco niveles de prioridad que se pueden mapear directamente a la técnica de priorización MoSCoW:

| Prioridad Jira | Icono | Mapeo MoSCoW | Significado práctico |
|---|---|---|---|
| **Highest** | 🔴 | Must Have | Bloqueante. Sin esto, el release no sale. |
| **High** | 🟠 | Must Have | Crítico pero no bloqueante inmediato. |
| **Medium** | 🟡 | Should Have | Importante. Se incluye si hay capacidad. |
| **Low** | 🔵 | Could Have | Deseable. Se hace si sobra tiempo. |
| **Lowest** | ⚪ | Won't Have | No se hará en esta versión. Documentado para futuro. |

Una distribución saludable del backlog sería aproximadamente: 60% Must, 20% Should, 20% Could. Si todo es "Must", no hay priorización real.

### 1.6 Estados y workflow

El workflow define el flujo de estados que recorre un issue desde su creación hasta su cierre. Un workflow típico tiene esta estructura:

```
┌──────────┐    ┌─────────────┐    ┌───────────┐    ┌─────┐    ┌──────┐
│  TO DO   │───►│ IN PROGRESS │───►│ IN REVIEW │───►│ QA  │───►│ DONE │
└──────────┘    └─────────────┘    └───────────┘    └─────┘    └──────┘
                       │                  │              │
                       ◄──────────────────┘              │
                       │   (cambios solicitados)         │
                       ◄─────────────────────────────────┘
                             (bug encontrado en QA)
```

Personalizaciones recomendadas del workflow:

- Agregar estado **Blocked** para impedimentos.
- Agregar estado **Ready for QA** si QA es un equipo separado.
- Configurar transición automática cuando se crea un PR (→ In Review).
- Validaciones en transición: no permitir pasar a Done sin PR mergeado.

### 1.7 Métricas y reportes

Jira ofrece reportes integrados que permiten medir la salud del proyecto:

| Reporte | Qué mide | Para quién |
|---|---|---|
| **Velocity Chart** | Story points completados por sprint. Tendencia de capacidad del equipo. | SM, PO |
| **Burndown Chart** | Trabajo restante vs. tiempo durante un sprint. | Dev Team, SM |
| **Sprint Report** | Resumen de lo completado vs. comprometido al cerrar un sprint. | SM, PO, Stakeholders |
| **Cumulative Flow Diagram** | Issues acumulados por estado a lo largo del tiempo. Detecta cuellos de botella. | SM (Kanban) |
| **Control Chart** | Cycle time por issue. Identifica issues con tiempos anormales. | SM |

Los dashboards personalizados permiten combinar widgets como: Sprint Burndown, Velocity Chart, distribución por prioridad, bugs abiertos y carga por persona.

### 1.8 JQL — Jira Query Language

JQL es el lenguaje de consultas de Jira. Permite filtrar issues con precisión. Algunas consultas útiles:

| Necesidad | Consulta JQL |
|---|---|
| Issues del sprint actual | `sprint in openSprints()` |
| Bugs sin resolver | `type = Bug AND resolution = Unresolved` |
| Stories sin estimar | `type = Story AND "Story Points" is EMPTY` |
| Issues bloqueados | `status = Blocked OR labels = impediment` |
| Issues de una épica específica | `"Epic Link" = SHOP-E03` |
| Issues sin asignar en el sprint | `sprint in openSprints() AND assignee is EMPTY` |
| Deuda técnica pendiente | `labels = tech-debt AND resolution = Unresolved ORDER BY priority DESC` |
| Issues creados esta semana | `created >= startOfWeek()` |
| Alta prioridad sin sprint asignado | `priority in (Highest, High) AND sprint is EMPTY` |

---

## Parte 2 — Metodologías Propuestas por Jira

Jira soporta nativamente dos marcos de trabajo ágiles: Scrum y Kanban. Cada uno tiene su propio tipo de board y configuración.

### 2.1 Scrum

Scrum es un framework ágil para desarrollar productos complejos mediante iteraciones cortas llamadas sprints (típicamente de 2 semanas). Define tres pilares fundamentales: transparencia (el proceso y el trabajo son visibles para todos), inspección (los artefactos y el progreso se inspeccionan frecuentemente) y adaptación (cuando algo se desvía, se ajusta lo antes posible).

**Roles:** Product Owner (prioriza el backlog y maximiza el valor), Scrum Master (facilita el proceso y remueve impedimentos) y Development Team (equipo auto-organizado que construye el producto, idealmente 3-9 personas).

**Artefactos:** Product Backlog (lista priorizada de todo el trabajo pendiente), Sprint Backlog (issues seleccionados para el sprint actual) e Incremento (producto potencialmente entregable al final del sprint).

**Ceremonias:** Sprint Planning (planificar qué se hará en el sprint), Daily Scrum (sincronización de 15 minutos diarios), Sprint Review (demostración del incremento al PO y stakeholders), Sprint Retrospective (reflexión del equipo sobre el proceso) y Refinamiento (detallar y estimar historias futuras).

En Jira, un proyecto Scrum incluye automáticamente: vista de backlog, gestión de sprints, velocity chart y burndown chart.

### 2.2 Kanban

Kanban es un método de gestión visual basado en el flujo continuo de trabajo, sin iteraciones fijas. Se enfoca en visualizar el flujo, limitar el trabajo en progreso (WIP limits) y optimizar el tiempo de entrega (lead time y cycle time).

En Jira, un proyecto Kanban incluye: board con columnas configurables, WIP limits por columna, Cumulative Flow Diagram y Control Chart.

### 2.3 Cuándo elegir cada una

| Criterio | Scrum | Kanban |
|---|---|---|
| **Tipo de trabajo** | Desarrollo de features nuevas, productos con releases | Soporte, ops, mantenimiento, demanda continua |
| **Cadencia** | Sprints fijos (1-4 semanas) | Flujo continuo, sin iteraciones |
| **Planificación** | Sprint Planning al inicio de cada sprint | Bajo demanda, se agregan items cuando hay capacidad |
| **Roles** | PO, SM, Dev Team definidos | Sin roles prescriptivos |
| **Mejor para** | Equipos con iteraciones fijas y ceremonias | Equipos de soporte, ops o con demanda impredecible |
| **Tipo de proyecto Jira** | Scrum | Kanban |

También existen proyectos **Team-managed** (para equipos que quieren autonomía y configuración simple) y **Company-managed** (para organizaciones con governance centralizada).

### 2.4 Técnicas complementarias de descomposición

Scrum y Kanban definen marcos de trabajo, pero no especifican cómo descomponer el trabajo para que cada incremento entregue valor real. Estas técnicas llenan ese vacío:

| Técnica | En qué ayuda | Cuándo usarla |
|---|---|---|
| **Vertical Slicing** | Cortar funcionalidades end-to-end, atravesando todas las capas | Al crear user stories |
| **Walking Skeleton** | Validar la arquitectura con un path completo mínimo | Inicio del proyecto / MVP |
| **Story Mapping** | Organizar el backlog por flujo de usuario | Refinamiento, planificación de releases |
| **Example Mapping** | Clarificar historias con ejemplos concretos | Refinamiento pre-sprint |
| **Spike** | Investigar antes de comprometer (produce conocimiento, no código) | Cuando la estimación es imposible |
| **Impact Mapping** | Conectar entregas con objetivos de negocio | Inicio del proyecto, roadmap |

---

## Parte 3 — Paso a Paso: Dar de Alta un Proyecto

### 3.1 Crear el proyecto

**Pasos en Jira:**

1. Ir a **Projects → Create project**.
2. Seleccionar template: **Scrum** (para desarrollo iterativo) o **Kanban** (para flujo continuo).
3. Elegir tipo: **Team-managed** (autonomía rápida) o **Company-managed** (governance centralizada).
4. Definir el **nombre** del proyecto (descriptivo, ej: *"Plataforma E-commerce"*).
5. Definir la **clave** del proyecto — un prefijo corto de 2-5 letras que identificará todos los issues (ej: `SHOP`). Todos los issues quedarán como `SHOP-1`, `SHOP-2`, etc.
6. Confirmar la creación.

**Ejemplos de clave por ámbito:**

| Proyecto | Clave sugerida |
|---|---|
| Plataforma e-commerce | `SHOP` |
| App de delivery | `DLVR` |
| CRM para ventas | `CRM` |
| Motor DSL de documentos | `MDSL` |
| Sistema de gestión hospitalaria | `HOSP` |

### 3.2 Configurar el board

Una vez creado el proyecto, configurar el board:

1. Ir a **Board settings → Columns**.
2. Definir las columnas que representan el workflow del equipo. Un setup recomendado para empezar:

| Columna | Mapea al estado | Descripción |
|---|---|---|
| **To Do** | To Do | Issues listos para ser trabajados |
| **In Progress** | In Progress | El desarrollador está trabajando en el issue |
| **In Review** | In Review | PR creado, esperando code review |
| **QA** | QA | Testing manual o automatizado |
| **Done** | Done | Completado y aceptado |

3. Configurar **Swimlanes** (opcional): por épica, por asignado o por prioridad.
4. Activar **Quick Filters** para filtrar por tipo (solo bugs, solo mi trabajo).

### 3.3 Definir épicas

Las épicas representan las grandes áreas funcionales del proyecto. Son el primer nivel de organización del trabajo.

**Cómo crearlas:**

1. En el backlog, click en **Create Epic**.
2. Completar: nombre descriptivo de la capacidad, descripción con el objetivo, y un color para identificarla visualmente en el board.

**Reglas para buenas épicas:**

- El título indica la capacidad completa: *"Gestión de pagos"*, no *"Pagos"*.
- Incluir: objetivo de negocio, user stories hijas identificadas y criterio de éxito medible.
- Si una épica tiene más de 8-10 historias, considerar dividirla en dos.

**Ejemplo para un e-commerce:**

| Epic | Objetivo | Sprints estimados |
|---|---|---|
| SHOP-E01 — Catálogo de productos | Que los usuarios puedan explorar y buscar productos | 2-3 |
| SHOP-E02 — Carrito de compras | Que los usuarios puedan agregar productos y gestionar cantidades | 2 |
| SHOP-E03 — Checkout y pagos | Que los usuarios puedan completar su compra con tarjeta | 3-4 |
| SHOP-E04 — Gestión de pedidos | Que los compradores vean el estado de sus pedidos | 2 |
| SHOP-E05 — Panel de administración | Que los admins gestionen productos, stock y pedidos | 3 |

**Ejemplo para una app de delivery:**

| Epic | Objetivo | Sprints estimados |
|---|---|---|
| DLVR-E01 — Registro y onboarding | Que usuarios y repartidores puedan registrarse | 1-2 |
| DLVR-E02 — Menú y pedido | Que los usuarios puedan armar un pedido desde el menú | 2-3 |
| DLVR-E03 — Tracking en tiempo real | Que los usuarios vean la ubicación del repartidor | 2 |
| DLVR-E04 — Pagos y propinas | Que los usuarios paguen in-app y dejen propina | 2 |
| DLVR-E05 — Panel de restaurante | Que los restaurantes gestionen menú y pedidos entrantes | 2-3 |

### 3.4 Crear user stories

Las user stories son la unidad de planificación del sprint. Cada una representa una funcionalidad concreta que entrega valor al usuario.

**Formato estándar:**

```
Como [rol],
quiero [acción],
para [beneficio].
```

**Campos a completar:**

1. **Summary:** título conciso con formato US.
2. **Description:** detalle de la historia, contexto, restricciones.
3. **Acceptance Criteria:** en formato Given/When/Then (mínimo 2 criterios).
4. **Epic Link:** vincular a la épica correspondiente.
5. **Story Points:** estimar con Planning Poker (secuencia Fibonacci: 1, 2, 3, 5, 8, 13).
6. **Priority:** según MoSCoW.

**Criterios de aceptación — formato Given/When/Then:**

```
GIVEN  [contexto o precondición]
WHEN   [acción del usuario]
THEN   [resultado esperado]
```

**Criterios INVEST para validar una story:** cada story debe ser **I**ndependiente, **N**egociable, **V**aliosa, **E**stimable, **S**mall (pequeña) y **T**esteable.

**Ejemplo completo — E-commerce:**

```
Summary: SHOP-12 — Como comprador, quiero filtrar productos por categoría,
         para encontrar más rápido lo que busco

Epic Link: SHOP-E01 — Catálogo de productos
Priority: High
Story Points: 5

Acceptance Criteria:

  AC-1: GIVEN que estoy en la página de catálogo
        WHEN selecciono una categoría del menú lateral
        THEN solo se muestran productos de esa categoría
        AND el contador muestra la cantidad de resultados

  AC-2: GIVEN que filtré por una categoría
        WHEN hago clic en "Limpiar filtros"
        THEN se muestran todos los productos nuevamente

  AC-3: GIVEN que selecciono una categoría sin productos
        WHEN se carga la página
        THEN se muestra un mensaje "No hay productos en esta categoría"
```

**Ejemplo completo — App de delivery:**

```
Summary: DLVR-08 — Como usuario, quiero ver la ubicación del repartidor en un mapa,
         para saber cuánto falta para que llegue mi pedido

Epic Link: DLVR-E03 — Tracking en tiempo real
Priority: Highest
Story Points: 8

Acceptance Criteria:

  AC-1: GIVEN que mi pedido fue aceptado por un repartidor
        WHEN abro la pantalla de seguimiento
        THEN veo un mapa con la ubicación del repartidor actualizada cada 10 segundos

  AC-2: GIVEN que el repartidor llega al punto de entrega
        WHEN la distancia es menor a 100 metros
        THEN recibo una notificación push "Tu pedido está llegando"
```

**Ejemplo completo — SaaS CRM:**

```
Summary: CRM-15 — Como vendedor, quiero filtrar leads por industria y tamaño de empresa,
         para priorizar mi pipeline de ventas

Epic Link: CRM-E04 — Gestión de leads
Priority: Medium
Story Points: 5

Acceptance Criteria:

  AC-1: GIVEN que estoy en la vista de leads
        WHEN selecciono "Tecnología" en el filtro de industria
        THEN solo se muestran leads con industria = Tecnología

  AC-2: GIVEN que apliqué múltiples filtros
        WHEN hago clic en "Exportar"
        THEN se descarga un CSV con los leads filtrados
```

### 3.5 Descomponer en tareas y sub-tareas

Cada user story se descompone en tareas técnicas y sub-tareas que representan el trabajo implementable.

**Reglas de descomposición:**

- Tareas técnicas: verbo de acción en el título (*"Implementar"*, *"Configurar"*, *"Crear"*, *"Migrar"*).
- Siempre vinculadas a una user story (nunca huérfanas).
- Si una tarea tarda más de 2 días, descomponerla en sub-tareas.
- Las tareas se estiman en horas, no en story points.

**Ejemplo: descomposición de SHOP-12 (filtro por categoría):**

| Sub-task | Descripción | Estimación |
|---|---|---|
| SHOP-12-01 | Crear endpoint `GET /api/categories` | 4h |
| SHOP-12-02 | Implementar componente `CategoryFilter` en React | 6h |
| SHOP-12-03 | Agregar query param `?category=` al endpoint de productos | 3h |
| SHOP-12-04 | Escribir tests unitarios del filtro (backend + frontend) | 4h |
| SHOP-12-05 | Agregar índice a columna `category_id` en la tabla de productos | 1h |

**Ejemplo: descomposición de DLVR-08 (tracking en mapa):**

| Sub-task | Descripción | Estimación |
|---|---|---|
| DLVR-08-01 | Implementar conexión WebSocket para posición del repartidor | 8h |
| DLVR-08-02 | Integrar SDK de Google Maps en la pantalla de seguimiento | 6h |
| DLVR-08-03 | Crear servicio de geolocalización en la app del repartidor | 6h |
| DLVR-08-04 | Implementar notificación push por proximidad (< 100m) | 4h |
| DLVR-08-05 | Tests de integración del flujo WebSocket → mapa | 4h |

### 3.6 Configurar releases y hitos

Jira no tiene un concepto nativo de "Milestone" como GitHub. En su lugar, se usan **Fix Versions** (también llamadas Releases) para representar hitos del producto.

**Cómo configurarlos:**

1. Ir a **Project Settings → Releases** (o Versions).
2. Crear versiones alineadas con el roadmap del proyecto.
3. Para cada versión, definir: nombre (versionado semántico), fecha de inicio, fecha de release y descripción del objetivo.
4. Asignar cada issue a su Fix Version correspondiente desde el campo "Fix Version" del issue.
5. Asignación masiva: seleccionar múltiples issues → Bulk Change → Edit → Fix Version.

**Ejemplo de releases para un e-commerce:**

| Fix Version | Sprints | Objetivo | Épicas incluidas |
|---|---|---|---|
| v1.0.0 — MVP | Sprint 01-04 | Compra básica funcional | Catálogo, Carrito, Checkout |
| v1.1.0 — Mejoras UX | Sprint 05-06 | Filtros avanzados, wishlist | Catálogo (mejoras), UX |
| v2.0.0 — Marketplace | Sprint 07-10 | Multi-vendedor, reviews | Marketplace, Reviews |

**Release Hub en Jira:** desde Deployments → Releases se puede ver el planning (qué issues hay en cada versión), el tracking (porcentaje de issues completados) y el ship (marcar como Released cuando se despliega).

Al marcar una Fix Version como "Released", Jira genera automáticamente release notes agrupadas por tipo de issue.

### 3.7 Configurar componentes

Los componentes permiten agrupar issues por módulo o subsistema de la arquitectura. Facilitan el filtrado y la asignación de responsables por área.

**Cómo crearlos:**

1. Ir a **Project Settings → Components**.
2. Crear componentes que reflejen la arquitectura del sistema.
3. Opcionalmente, asignar un "Component Lead" responsable.

**Ejemplo para un e-commerce:**

| Componente | Descripción |
|---|---|
| Frontend-Web | Interfaz web React/Next.js |
| API | Backend REST/GraphQL |
| Database | Modelos, migraciones, queries |
| Payments | Integración con pasarelas de pago |
| Auth | Autenticación y autorización |
| Infra | CI/CD, cloud, monitoreo |

**Ejemplo para una app de delivery:**

| Componente | Descripción |
|---|---|
| App-Cliente | App mobile del usuario (React Native/Flutter) |
| App-Repartidor | App mobile del repartidor |
| API-Gateway | API central del backend |
| Geolocation | Servicios de geolocalización y rutas |
| Notifications | Push notifications, SMS, email |
| Panel-Admin | Dashboard web de administración |

### 3.8 Planificar y ejecutar sprints

**Sprint Planning (al inicio de cada sprint):**

1. Abrir la vista **Backlog** en Jira.
2. Revisar el Product Backlog priorizado con el Product Owner.
3. Verificar que las historias candidatas cumplen el **Definition of Ready** (criterios de aceptación escritos, estimación hecha, sin dependencias bloqueantes).
4. Arrastrar historias al sprint recién creado.
5. Estimar con Planning Poker (usar campo Story Points).
6. Definir el **Sprint Goal** (objetivo del sprint en una frase).
7. Verificar que los SP comprometidos no excedan la velocity promedio del equipo.
8. Iniciar el sprint con fecha de inicio y fin.

**Durante el sprint:**

- Actualizar estados: mover issues entre columnas al cambiar de estado.
- Registrar impedimentos: usar label `impediment` o estado `Blocked`.
- Agregar sub-tasks si surge trabajo adicional.
- Documentar decisiones en los comentarios del issue.

**Sprint Review (al cerrar):**

1. Filtrar issues del sprint con JQL: `sprint = "Sprint XX"`.
2. Para cada issue completado: demo al PO, registrar feedback.
3. Issues no completados: documentar razón, decidir si mover al siguiente sprint o devolver al backlog.
4. Registrar la velocity del sprint.

**Sprint Retrospective (después de la review):**

1. Formato Start-Stop-Continue u otro acordado.
2. Definir acciones de mejora concretas con responsable y fecha.
3. Crear issues para las acciones de mejora en el backlog del próximo sprint.
4. Revisar si las acciones del sprint anterior se cumplieron.

### 3.9 Flujo de trabajo diario

El flujo diario con Jira sigue esta secuencia:

```
┌──────────────────────────────────────────────────────────────────┐
│                    FLUJO DIARIO CON JIRA                         │
│                                                                  │
│  1. DAILY SCRUM (15 min, frente al Board)                        │
│     → ¿Qué moví ayer? ¿Qué moveré hoy? ¿Qué me bloquea?       │
│                                                                  │
│  2. DESARROLLO                                                   │
│     a. Tomar issue de TO DO → mover a IN PROGRESS                │
│     b. Crear branch: feature/SHOP-12-category-filter             │
│     c. Desarrollar + tests                                       │
│     d. Crear PR → mover issue a IN REVIEW                        │
│     e. Code review aprobado → mover a QA                         │
│     f. QA aprobado → mover a DONE                                │
│                                                                  │
│  3. ACTUALIZACIÓN CONTINUA                                       │
│     → El board debe reflejar la realidad en todo momento          │
│     → Si no está en el board, no existe                          │
└──────────────────────────────────────────────────────────────────┘
```

### 3.10 Integraciones recomendadas

**Jira + Git (GitHub/GitLab/Bitbucket):**

Incluir la clave del issue en los commits para vincularlos automáticamente:

```bash
git commit -m "SHOP-12 #comment Implementar filtro de categorías #time 3h"
```

Formato de ramas recomendado: `feature/SHOP-12-category-filter`, `bugfix/SHOP-BUG003-double-charge`, `hotfix/SHOP-BUG007-payment-crash`.

**Jira + CI/CD:** Configurar el pipeline para que Jira muestre en qué entorno está desplegado cada issue (dev, staging, prod) y el estado del build (pass/fail).

**Jira + Confluence:** Vincular páginas de documentación a issues. Crear páginas desde templates para Sprint Plan, Review y Retrospectiva.

**Jira + Slack/Teams:** Configurar notificaciones automáticas para issues asignados, issues completados, bugs de prioridad máxima y resúmenes de sprint.

**Automatizaciones útiles (Jira Automation):**

| Automatización | Trigger | Acción |
|---|---|---|
| Auto-assign | Issue movido a In Progress | Asignar al usuario que lo movió |
| Auto-transition | PR mergeado en GitHub | Mover issue a In Review o Done |
| Notificación de bloqueo | Issue marcado como Blocked | Mensaje al SM por Slack |
| Sub-tasks completadas | Todas las sub-tasks en Done | Transicionar la Story a In Review |
| SLA warning | Issue lleva > 3 días en In Progress | Notificación al assignee y SM |
| Scope alert | Issue agregado al sprint activo | Notificación al SM y PO |

---

## Parte 4 — Ejemplos Completos en Distintos Ámbitos

### 4.1 E-commerce (Web)

**Proyecto:** Tienda online de ropa — Clave: `SHOP`

**Iniciativa:** Aumentar la conversión del checkout en un 25% (OKR Q2)

| Nivel | ID | Descripción |
|---|---|---|
| Iniciativa | — | Aumentar conversión de checkout en 25% |
| Épica | SHOP-E03 | Checkout multi-paso con pasarela de pagos |
| Story | SHOP-20 | Como comprador, quiero guardar mi tarjeta para futuras compras, para no reingresarla cada vez |
| Tarea técnica | SHOP-20-01 | Integrar SDK de Stripe para tokenización de tarjetas |
| Sub-tarea | SHOP-20-02 | Crear migración para tabla `saved_cards` |
| Bug | SHOP-BUG005 | [Checkout] Doble cobro cuando el usuario hace doble clic en "Pagar" |

**Releases:**

| Versión | Objetivo | Épicas |
|---|---|---|
| v1.0.0 | Compra básica: catálogo, carrito, checkout con tarjeta | E01, E02, E03 |
| v1.1.0 | Wishlist, reviews de productos, notificaciones de stock | E06, E07 |
| v2.0.0 | Marketplace multi-vendedor | E08, E09, E10 |

**Componentes:** Frontend-Web, API, Database, Payments, Auth, Infra.

### 4.2 App mobile (Delivery)

**Proyecto:** App de delivery de comida — Clave: `DLVR`

**Iniciativa:** Reducir el tiempo de entrega promedio en 15 minutos

| Nivel | ID | Descripción |
|---|---|---|
| Iniciativa | — | Reducir tiempo de entrega promedio en 15 min |
| Épica | DLVR-E03 | Tracking en tiempo real de pedidos |
| Story | DLVR-08 | Como usuario, quiero ver la ubicación del repartidor en un mapa, para saber cuánto falta |
| Tarea técnica | DLVR-08-01 | Implementar conexión WebSocket para posición del repartidor |
| Sub-tarea | DLVR-08-02 | Integrar SDK de Google Maps en la pantalla de seguimiento |
| Bug | DLVR-BUG010 | [Mapa] La ruta no se actualiza al recalcular |

**Releases:**

| Versión | Objetivo | Épicas |
|---|---|---|
| v1.0.0 | Pedido básico: menú, carrito, pago, tracking simple | E01, E02, E03, E04 |
| v1.1.0 | Ruta optimizada, historial de pedidos, promociones | E05, E06 |
| v2.0.0 | Multi-restaurante, sistema de ratings | E07, E08 |

**Componentes:** App-Cliente, App-Repartidor, API-Gateway, Geolocation, Notifications, Panel-Admin.

### 4.3 SaaS B2B (CRM)

**Proyecto:** CRM para equipos de ventas — Clave: `CRM`

**Iniciativa:** Mejorar la captación de leads en un 30%

| Nivel | ID | Descripción |
|---|---|---|
| Iniciativa | — | Mejorar captación de leads en 30% |
| Épica | CRM-E04 | Formularios dinámicos de captura de leads |
| Story | CRM-15 | Como vendedor, quiero filtrar leads por industria, para priorizar mi pipeline |
| Tarea técnica | CRM-15-01 | Crear componente de filtro dinámico con React |
| Sub-tarea | CRM-15-02 | Agregar índice a columna `industry` en la BD |
| Bug | CRM-BUG008 | [Filtros] El filtro por fecha no respeta timezone del usuario |

**Releases:**

| Versión | Objetivo | Épicas |
|---|---|---|
| v1.0.0 | Gestión básica de contactos y leads | E01, E02, E03 |
| v1.1.0 | Formularios dinámicos, importación CSV, deduplicación | E04, E05 |
| v2.0.0 | Pipeline visual, forecasting, integración con email | E06, E07, E08 |

**Componentes:** Frontend-Dashboard, API, Database, Integrations, Auth, Reports.

### 4.4 Motor DSL (Librería/Framework)

**Proyecto:** Motor DSL de generación de documentos — Clave: `MDSL`

**Iniciativa:** Desacoplamiento datos/diseño — Necesidad de negocio NB-01

| Nivel | ID | Descripción |
|---|---|---|
| Iniciativa | NB-01 | Desacoplamiento datos/diseño |
| Épica | MDSL-E03 | Parser DSL — Convertir DSL → AST |
| Story | MDSL-05 | Como desarrollador, quiero resolver datos dinámicos en el AST, para vincular datos externos a plantillas |
| Tarea técnica | MDSL-BT020 | Implementar IDslParser |
| Sub-tarea | MDSL-BT021 | Definir gramática DSL base |
| Bug | MDSL-BUG001 | Parser no maneja correctamente caracteres Unicode en TextNode |

**Releases:**

| Versión | Objetivo | Épicas |
|---|---|---|
| v1.0.0 — MVP | Motor funcional DSL → AST → Render ESC/POS | E01-E07 (parcial) |
| v1.1.0 — Expansión | Multi-formato, extensibilidad, integración MAUI | E07 (completa), E08, E09 |
| v1.3.0 — Madurez | PDF, Bluetooth, performance, documentación | E10, E11 |

**Componentes:** Core, Parser, Rendering, Layout, Extensibility, Testing.

**Tabla de nomenclatura (mapeo SDD → Jira):**

| Concepto SDD | Equivalente Jira | Ejemplo |
|---|---|---|
| Fase del roadmap | Fix Version (Release) | v1.0.0 — MVP |
| Épica | Epic | ÉPICA 3 — Parser DSL |
| User Story | Story | US-05: Resolver datos dinámicos en el AST |
| Backlog Técnico | Sub-task o Task | BT-012: Implementar nodos básicos |
| Sprint Plan | Sprint | Sprint 01 (2 semanas, 48 SP) |
| Necesidad de Negocio | Initiative/Theme (o Label) | NB-01 |
| Definition of Ready | Checklist en Story | DoR checklist con 6-8 criterios |
| Definition of Done | Workflow transition conditions | Validaciones en transición a Done |

---

## Anti-patrones a evitar

Para cerrar, un resumen de los errores más comunes al organizar proyectos en Jira:

| Anti-patrón | Problema | Solución |
|---|---|---|
| **Todo es "Must"** | No hay priorización real | Forzar distribución: 60% Must, 20% Should, 20% Could |
| **Épicas sin descomponer** | Una épica de 40 SP en un sprint de 15 | Descomponer en stories de 3-8 SP |
| **Board fantasma** | El board no refleja la realidad | Actualizar en la daily. Si no está en el board, no existe |
| **Sprint elástico** | Se extiende "unos días más" para terminar | Fecha fija. Lo no completado vuelve al backlog |
| **Refinamiento inexistente** | Stories llegan al planning sin detallar ni estimar | Dedicar 10% del sprint a refinamiento |
| **Daily de 45 minutos** | Se convierte en reunión de estado detallado | Timebox de 15 min. Discusiones técnicas al "parking lot" |
| **Velocity como KPI individual** | Genera inflación de estimaciones y competencia tóxica | Velocity es métrica del equipo para planificación, nunca para evaluar individuos |
| **US como tarea técnica** | "Implementar Parser" no tiene valor para el usuario | Reformular con formato As/Want/So that |

---

> **Fuente:** Esta guía fue elaborada a partir del documento `devs/metodos-agiles/metodologias-agiles-jira.md` del repositorio **Ejemplo_IA_SDD_Template**. Para mayor profundidad sobre Scrum, Kanban, XP, métricas ágiles y técnicas de descomposición (Vertical Slicing, Walking Skeleton, Story Mapping, etc.), consultar el documento fuente completo.
