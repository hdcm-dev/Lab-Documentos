# AG-00 — Product Manager / Analista de Negocio

**Carpeta SDD:** `docs/00_contexto/`  
**Perfil Profesional:** Estrategia de producto, alineación de stakeholders

---

## 1. ¿Qué es esta especialidad?

El **Product Manager (PM)** es el profesional que define el *qué* y el *por qué* de un producto de software. Se sitúa en la intersección entre negocio, tecnología y experiencia de usuario. Su responsabilidad es articular una visión clara del producto, delimitar su alcance, planificar la evolución (roadmap) y establecer acuerdos de equipo.

En el contexto SDD, el PM produce los documentos fundacionales que dan dirección al resto del proyecto. Todo lo que se construye (especificaciones, arquitectura, código, pruebas) debe poder trazarse hacia atrás hasta los documentos de contexto.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el tipo de producto y organización:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Gestión de Producto Digital** | Estrategia de productos de software (SaaS, apps, librerías, APIs) | Productos tech consumidos por usuarios o desarrolladores | Visión de producto, roadmap Now-Next-Later, métricas de adopción, Lean Canvas |
| **Gestión de Producto Físico/Industrial** | Estrategia de productos tangibles (manufactura, retail, hardware) | Productos con cadena de producción, inventario y distribución | BOM (Bill of Materials), pricing strategy, market fit analysis, lifecycle management |
| **Product Owner Ágil** | Gestión operativa del backlog y priorización sprint a sprint | Equipos Scrum/Kanban donde el PO es el responsable del valor | Product Backlog ordenado, aceptación de incrementos, stakeholder management day-to-day |

> **En este proyecto** se aplica **Gestión de Producto Digital** porque el producto es una librería .NET (paquete NuGet) consumida por desarrolladores, con ciclo de vida digital (versionado SemVer, distribución via feed).

#### Ejemplos temáticos por disciplina

**Gestión de Producto Físico/Industrial** *(aplica en productos tangibles)*
- *Ejemplo:* Una empresa de electrónica lanza un lector de códigos QR portátil. El PM define el BOM, negocia con proveedores de sensores, planifica la certificación FCC/CE, y crea el roadmap con hitos de fabricación (prototipo → piloto → producción en masa).

**Product Owner Ágil** *(aplica como rol operativo)*
- *Ejemplo:* En un equipo Scrum de 7 personas que construye un e-commerce, el PO asiste a todas las ceremonias, prioriza el backlog en cada planning, acepta o rechaza historias en la review, y traduce el feedback de los stakeholders en refinamiento de user stories.

### ¿Cómo se diferencia del Analista de Negocio (AG-01)?

- El **PM** define la estrategia del producto: qué construir, para quién, y en qué orden
- El **Analista de Negocio** profundiza en las necesidades específicas: qué problemas resolver y cómo validar que se resolvieron

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Definición de visión de producto | Articular propósito, audiencia, propuesta de valor y diferenciación | `vision-producto_v1.0.md` |
| Delimitación de alcance | Definir qué está dentro y fuera del proyecto, con justificaciones | `alcance-proyecto_v1.0.md` |
| Planificación de roadmap | Mapear la evolución del producto en fases, épicas y releases | `roadmap-producto_v1.0.md` |
| Análisis de compatibilidad | Identificar plataformas, entornos y restricciones de runtime | `compatibilidad-plataformas_v1.0.md` |
| Acuerdos de equipo | Formalizar normas de trabajo, herramientas, ceremonias | `acuerdo-equipo_v1.0.md` |
| Gestión de stakeholders | Identificar y alinear a las partes interesadas | Mapa de stakeholders |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Visión de producto | Lean Canvas / Product Vision Board | Base para priorización de backlog y roadmap |
| Alcance del proyecto | IEEE 830 §3 (Scope) / PMBOK §5 | Límite para planificación de sprints y estimación |
| Roadmap de producto | Roadmap Now-Next-Later / Timeline | Input para backlog épico → sprint → release |
| Compatibilidad de plataformas | .NET Target Framework documentation | Input para decisiones de arquitectura y CI/CD |
| Acuerdo de equipo (Working Agreement) | Scrum Guide / Team Charter | Normas operativas para todas las ceremonias ágiles |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Claridad de la visión | La visión se entiende sin ambigüedades en una lectura | Revisión por 3 stakeholders distintos |
| Métricas SMART | Las métricas de éxito son Específicas, Medibles, Alcanzables, Relevantes y Temporales | Checklist SMART por cada métrica |
| Exclusiones justificadas | Cada ítem fuera del alcance tiene una razón explícita | 100% de exclusiones con justificación |
| Trazabilidad roadmap→sprints | Cada fase del roadmap traza a épicas y sprints | Mapa de trazabilidad completo |
| Stakeholders con nombre/rol | No hay stakeholders genéricos ("el equipo", "los usuarios") | Cada stakeholder tiene nombre o rol concreto |
| Acuerdo ejecutable | El acuerdo de equipo tiene acciones concretas, no aspiraciones | Cada punto es verificable |

---

## 5. Preguntas guía para el análisis

### 5.1 Visión de producto

> **P1:** ¿Se puede explicar el producto en una oración?
>
> *Ejemplo práctico:* "Motor DSL que transforma plantillas declarativas + datos dinámicos en documentos renderizados para impresión térmica, vista previa UI y texto plano."

> **P2:** ¿Quiénes son los usuarios del producto y qué problema les resuelve?
>
> *Ejemplo práctico:* "Desarrolladores .NET que necesitan generar tickets, facturas o recibos sin acoplar la lógica de presentación al código de la aplicación."

> **P3:** ¿Qué alternativas existen y por qué esta solución es mejor?
>
> *Ejemplo práctico:* "Crystal Reports (obsoleto, licencia costosa), iTextSharp (PDF only, no ESC/POS), strings hardcodeados (no mantenible). Nuestro motor es declarativo, multi-render y extensible."

### 5.2 Alcance

> **P4:** ¿Qué está explícitamente fuera del alcance y por qué?
>
> *Ejemplo práctico:* "Renderizado PDF queda fuera → porque la prioridad es ESC/POS para el caso de uso principal (POS retail). Se incluirá en v2.0 post-validación."

> **P5:** ¿Los criterios de éxito son medibles?
>
> *Ejemplo práctico:* ❌ "El motor debe ser rápido" → ✅ "Parser DSL < 100ms para plantillas < 10KB en hardware de referencia"

### 5.3 Roadmap

> **P6:** ¿El roadmap conecta fases → épicas → sprints → releases?
>
> *Ejemplo práctico:* "Fase 1: Core Engine → Épica: Parser + Evaluator → Sprint 01-02 → Release 0.1.0-alpha"

> **P7:** ¿Cada release tiene criterios de salida (exit criteria)?
>
> *Ejemplo práctico:* "Release 0.3.0-beta: 100% de CU del pipeline básico implementados, ≥80% cobertura de tests, guía de integración publicada."

### 5.4 Acuerdos de equipo

> **P8:** ¿El acuerdo de equipo tiene campos pendientes identificados?
>
> *Ejemplo práctico:* "Horario de Daily: [Pendiente de definir en Sprint 01]" → debe ser visible como acción, no dejarse en blanco.

---

## 6. Plantilla base

### 6.1 Visión de producto

```markdown
# Visión de Producto

**Proyecto:** [Nombre]  
**Versión:** 1.0  
**Fecha:** [YYYY-MM-DD]

---

## 1. Propósito

[¿Qué problema resuelve este producto?]

## 2. Audiencia

| Segmento | Descripción | Necesidad principal |
|----------|-------------|---------------------|
| [Segmento 1] | [Quiénes son] | [Qué necesitan] |

## 3. Propuesta de valor

[¿Por qué este producto y no las alternativas?]

## 4. Métricas de éxito

| Métrica | Target | Plazo |
|---------|--------|-------|
| [Métrica 1] | [Valor medible] | [Fecha o sprint] |

## 5. Lo que NO es este producto

- [Exclusión explícita 1]: [Justificación]
- [Exclusión explícita 2]: [Justificación]
```

### 6.2 Alcance del proyecto

```markdown
# Alcance del Proyecto

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

## 1. Dentro del alcance

| Funcionalidad | Épica | Sprint estimado |
|---------------|-------|-----------------|
| [Funcionalidad 1] | [Épica] | [Sprint XX] |

## 2. Fuera del alcance

| Funcionalidad excluida | Justificación | Versión futura |
|------------------------|---------------|----------------|
| [Funcionalidad X] | [Razón] | [v2.0 / Backlog] |

## 3. Supuestos

- [Supuesto 1]

## 4. Restricciones

- [Restricción 1]
```

### 6.3 Roadmap

```markdown
# Roadmap de Producto

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

## Visión por fases

| Fase | Objetivo | Épicas | Sprints | Release |
|------|----------|--------|---------|---------|
| 1 — MVP | [Objetivo] | [E-01, E-02] | S01-S03 | v0.1.0-alpha |
| 2 — Beta | [Objetivo] | [E-03, E-04] | S04-S06 | v0.3.0-beta |
| 3 — GA | [Objetivo] | [E-05, E-06] | S07-S08 | v1.0.0 |

## Criterios de salida por release

### v0.1.0-alpha
- [ ] [Criterio 1]
- [ ] [Criterio 2]
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Visión genérica | "Hacer un buen producto" no guía decisiones | Articular propuesta de valor diferenciada |
| Métricas no medibles | "Rendimiento aceptable" no se puede validar | Definir targets numéricos con plazo |
| Alcance abierto | Sin exclusiones → scope creep inevitable | Listar exclusiones con justificación |
| Roadmap sin releases | Fases sin exit criteria → nunca se sabe si se llegó | Cada fase con release y criterios de salida |
| Acuerdo aspiracional | "Vamos a comunicarnos bien" → no verificable | Cada ítem como regla concreta verificable |
