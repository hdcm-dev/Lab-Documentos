# AG-01 — Analista de Negocio Senior

**Carpeta SDD:** `docs/01_necesidades_negocio/`  
**Perfil Profesional:** Elicitación de requisitos, modelado de negocio

---

## 1. ¿Qué es esta especialidad?

El **Analista de Negocio (BA – Business Analyst)** es el profesional que traduce los problemas y oportunidades del negocio en requisitos estructurados que el equipo técnico puede implementar. Su trabajo es entender *qué necesita el negocio*, *por qué lo necesita* y *cómo se validará que la necesidad fue satisfecha*.

En el contexto SDD, el Analista de Negocio produce los documentos de **necesidades de negocio (NB)** que constituyen el nivel más alto de requisitos. Cada NB debe ser trazable hacia abajo hasta casos de uso (CU) y tareas técnicas.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según la profundidad del análisis y el contexto organizacional:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Análisis de Negocio (Business Analysis)** | Traducir problemas del negocio en requisitos estructurados para el equipo técnico | Siempre que exista una brecha entre lo que el negocio necesita y lo que el sistema hace | Documento de necesidades (NB), criterios de éxito, mapa de stakeholders, matriz NB→CU |
| **Ingeniería de Requisitos (Requirements Engineering)** | Elicitar, documentar, validar y gestionar requisitos con formalismo ingenieril | Proyectos regulados, safety-critical, con contratos formales (defensa, salud, aviación) | SRS (IEEE 830), modelo de requisitos DOORS, trazabilidad formal, análisis de impacto |
| **Análisis de Procesos de Negocio (BPA/BPM)** | Modelar, optimizar y automatizar procesos organizacionales end-to-end | Transformación digital, reingeniería de procesos, implementaciones ERP/CRM | Diagramas BPMN 2.0, mapas de procesos AS-IS/TO-BE, métricas de proceso (cycle time, throughput) |

> **En este proyecto** se aplica **Análisis de Negocio** porque el objetivo es identificar los dolores del domain (impresión de documentos acoplada al código) y traducirlos en necesidades trazables hacia casos de uso.

#### Ejemplos temáticos por disciplina

**Ingeniería de Requisitos** *(aplica en proyectos regulados)*
- *Ejemplo:* Para un sistema de piloto automático de aviones, el ingeniero de requisitos documenta cada requisito funcional en IBM DOORS con atributos de criticidad (DAL A-E según DO-178C), verificabilidad y trazabilidad bidireccional hacia tests de certificación. Cada cambio de requisito pasa por un Change Control Board formal.

**Análisis de Procesos de Negocio** *(aplica en transformación organizacional)*
- *Ejemplo:* Un hospital quiere reducir el tiempo de admisión de pacientes. El analista BPM modela el proceso AS-IS en BPMN 2.0, identifica cuellos de botella (3 firmas en papel, búsqueda manual de historia clínica), diseña el proceso TO-BE (firma digital + HCE integrada) y establece KPIs de mejora (cycle time de 45min → 12min).

### Marco de referencia profesional

Esta especialidad se alinea con el cuerpo de conocimiento **BABOK v3** (Business Analysis Body of Knowledge) del IIBA (International Institute of Business Analysis), particularmente en las áreas de Elicitación, Análisis de Requisitos y Trazabilidad.

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Elicitación de necesidades | Identificar qué problemas tiene el negocio que el software debe resolver | Documento de necesidades individual (NB-XX) |
| Definición de criterios de éxito | Establecer métricas que validen que la necesidad fue resuelta | Tabla de criterios con métrica y target |
| Análisis de stakeholders | Identificar quién tiene la necesidad, quién la implementa, quién se beneficia | Mapa de stakeholders por NB |
| Trazabilidad vertical | Conectar cada NB con los CU que la satisfacen | Matriz NB → CU |
| Consolidación de necesidades | Agrupar y priorizar las necesidades en un documento resumen | `necesidades-negocio_v1.0.md` |
| Detección de dependencias | Identificar qué necesidades son prerequisito de otras | Grafo de dependencias |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Necesidades de negocio (Business Needs) | BABOK v3 §5.1 / IEEE 830 §2 | Input para derivar casos de uso y user stories |
| Criterios de éxito | BABOK v3 §6.3 (Solution Evaluation) | Input para criterios de aceptación en QA |
| Mapa de stakeholders | BABOK v3 §2.2 (Stakeholder Analysis) | Input para priorización de backlog |
| Matriz de trazabilidad NB→CU | IEEE 830 §3.5 (Traceability) | Valida completitud: toda necesidad tiene implementación |
| Análisis de impacto | BABOK v3 §8.5 (Impact Analysis) | Input para estimación de esfuerzo y riesgo |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Problema explícito | Cada NB articula un dolor concreto, no una solución | Revisión de texto: ¿describe un problema o una feature? |
| Criterios medibles | Cada criterio de éxito tiene métrica numérica y target | 100% de criterios con número |
| Trazabilidad completa | Cada NB traza a ≥1 CU | 0 NB huérfanos |
| Stakeholders concretos | Propietario, implementador y beneficiario identificados | 3 roles por NB |
| Dependencias documentadas | Si NB-03 depende de NB-01, está escrito | 0 dependencias implícitas |
| Nomenclatura consistente | Archivos siguen patrón `NB-XX-nombre-kebab.v1.0.md` | 0 excepciones |

---

## 5. Preguntas guía para el análisis

### 5.1 Identificación del problema

> **P1:** ¿Cuál es el dolor actual que motiva esta necesidad?
>
> *Ejemplo práctico:* "Cada cambio en el formato de un ticket de venta requiere modificar código fuente C#, recompilar y re-deployar. Esto implica ~4 horas de trabajo del desarrollador por cada cambio de formato."

> **P2:** ¿Qué pasa si NO se resuelve este problema?
>
> *Ejemplo práctico:* "Los operadores de puntos de venta seguirán usando tickets con formato incorrecto, generando reclamos de clientes y errores en auditorías fiscales."

### 5.2 Criterios de éxito

> **P3:** ¿Cómo sabemos que la necesidad fue satisfecha?
>
> *Ejemplo práctico:*
> | Criterio | Métrica | Target |
> |----------|---------|--------|
> | Tiempo de cambio de formato | Horas de desarrollo | < 0.5h (vs 4h actual) |
> | Errores por cambio | Incidentes post-deploy | 0 |

> **P4:** ¿Quién valida que el criterio se cumplió?
>
> *Ejemplo práctico:* "El Product Owner valida con el operador de POS que el ticket se imprime correctamente después de un cambio de plantilla DSL sin recompilación."

### 5.3 Stakeholders

> **P5:** ¿Quién tiene el problema, quién lo resuelve y quién se beneficia?
>
> *Ejemplo práctico:*
> - **Propietario:** Product Owner (define prioridad)
> - **Implementador:** Equipo de desarrollo .NET
> - **Beneficiario:** Operadores de POS + área de marketing (diseño de tickets)

### 5.4 Trazabilidad

> **P6:** ¿Qué casos de uso satisfacen esta necesidad?
>
> *Ejemplo práctico:* "NB-01 (Desacoplar lógica de presentación) → CU-01 (Interpretar plantilla DSL), CU-02 (Resolver datos), CU-03 (Construir modelo interno), CU-05 (Generar representación abstracta)"

> **P7:** ¿Existen necesidades implícitas no documentadas?
>
> *Ejemplo práctico:* "Si necesitamos imprimir en múltiples dispositivos (NB-04), implícitamente necesitamos poder probar la salida sin hardware real → ¿hay un NB para debugging/preview?"

---

## 6. Plantilla base

### 6.1 Necesidad de negocio individual

```markdown
# NB-XX — [Nombre descriptivo]

**Proyecto:** [Nombre del Proyecto]  
**Documento:** NB-XX-nombre-kebab.v1.0.md  
**Versión:** 1.0  
**Fecha:** [YYYY-MM-DD]

---

## 1. Descripción

[Descripción breve de la necesidad en 2-3 oraciones.]

## 2. Problema Específico

[¿Cuál es el dolor concreto que motiva esta necesidad? 
Situación actual → impacto → consecuencia si no se resuelve.]

## 3. Criterios de Éxito

| Criterio | Métrica | Target |
|----------|---------|--------|
| [Criterio 1] | [Unidad de medida] | [Valor objetivo] |
| [Criterio 2] | [Unidad de medida] | [Valor objetivo] |

## 4. Stakeholders

| Rol | Descripción |
|-----|-------------|
| **Propietario** | [Quién decide la prioridad] |
| **Implementador** | [Quién lo construye] |
| **Beneficiario** | [Quién se beneficia] |

## 5. Trazabilidad a Casos de Uso

| Caso de Uso | Descripción | Archivo |
|-------------|-------------|---------|
| CU-XX | [Nombre del CU] | [link al archivo](../../02_especificacion_funcional/casos-de-uso/CU-XX-nombre_v1.0.md) |

## 6. Dependencias

- **Depende de:** [NB-YY, si aplica]
- **Es prerequisito de:** [NB-ZZ, si aplica]

## 7. Historial de versiones

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | [YYYY-MM-DD] | Versión inicial |
```

### 6.2 Documento resumen de necesidades

```markdown
# Necesidades de Negocio — Resumen

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

## Necesidades identificadas

| ID | Necesidad | Prioridad | CU relacionados | Estado |
|----|-----------|-----------|-----------------|--------|
| NB-01 | [Nombre] | Alta | CU-01, CU-02 | Documentada |
| NB-02 | [Nombre] | Alta | CU-13, CU-14 | Documentada |

## Mapa de dependencias

`` `
NB-01 (Desacople)
  ↓
NB-02 (Estandarización) → NB-03 (Cambios sin código)
  ↓
NB-04 (Multi-dispositivo)
`` `

## Gaps detectados

- [Gap 1]: [Descripción del gap y propuesta]
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Necesidad como solución | "Necesitamos un parser JSON" describe solución, no problema | Reformular como dolor: "Los cambios de formato requieren recompilación" |
| Criterios subjetivos | "El sistema debe ser fácil de usar" no se puede validar | Métrica concreta: "Configurar una plantilla en < 15 min" |
| Stakeholders genéricos | "El equipo" no tiene accountabilidad | Nombre o rol concreto: "PO: Juan Pérez" |
| NB sin trazabilidad | Necesidad documentada pero sin casos de uso | Cada NB debe trazar a ≥1 CU |
| Nomenclatura inconsistente | `NB-02 Estandarización.v1.0.md` con espacios y acentos | Kebab-case sin acentos: `NB-02-Estandarizacion.v1.0.md` |
