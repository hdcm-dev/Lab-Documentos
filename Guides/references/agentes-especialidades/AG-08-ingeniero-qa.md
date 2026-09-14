# AG-08 — Ingeniero QA / SDET Senior

**Carpeta SDD:** `docs/08_calidad_y_pruebas/`  
**Perfil Profesional:** Estrategia de calidad, automatización de pruebas

---

## 1. ¿Qué es esta especialidad?

El **Ingeniero QA / SDET (Software Development Engineer in Test)** es el profesional que diseña, implementa y mantiene la estrategia de calidad del software. No solo ejecuta pruebas: define *qué probar*, *cómo probarlo*, *cuándo probarlo* y *qué tan cubierto está el sistema*. Trabaja con automatización de tests, matrices de cobertura, quality gates y criterios de aceptación.

En el contexto SDD, este rol produce los documentos que definen cómo se valida que el software cumple con las especificaciones: la **estrategia de testing**, los **casos de prueba**, la **Definition of Done (DoD)**, la **matriz de cobertura** y los **criterios de validación** del motor.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el nivel de automatización y el tipo de validación:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Estrategia y Diseño de Testing (QA)** | Definir *qué probar*, el enfoque por niveles (pirámide), quality gates y criterios de aceptación | Todo proyecto de software que necesite validación sistemática | Estrategia de testing, plan de pruebas, DoD, pirámide de tests, quality gates en CI/CD |
| **Automatización de Pruebas (SDET)** | Implementar frameworks de tests automatizados, fixtures, CI integration y snapshot testing | Proyectos con CI/CD donde los tests deben ejecutarse automáticamente | Test frameworks, test fixtures, snapshot baselines, coverage reports, CI test stages |
| **Testing de Performance y Seguridad** | Validar requisitos no-funcionales: carga, estrés, latencia, vulnerabilidades, penetration testing | Sistemas con SLAs de rendimiento, sistemas expuestos a internet, regulaciones de seguridad | Load test scripts (k6, JMeter), benchmarks, OWASP ZAP reports, threat modeling, chaos engineering |

> **En este proyecto** se aplica **Estrategia de Testing (QA) + Automatización (SDET)** porque el motor DSL requiere tanto diseño de estrategia (pirámide, DoD, coverage matrix) como automatización (xUnit, snapshot testing de renderers, CI quality gates).

#### Ejemplos temáticos por disciplina

**Testing de Performance** *(aplica en sistemas con SLAs estrictos)*
- *Ejemplo:* Una plataforma de pagos online necesita garantizar <200ms de latencia p99 y 5000 TPS sostenidos. El ingeniero de performance diseña load tests con k6 que simulan 10K usuarios concurrentes, ejecuta tests de estrés hasta encontrar el punto de quiebre (7200 TPS → timeouts), configura benchmarks automatizados en CI que fallan si la latencia p99 excede el SLA, y diseña chaos tests con Litmus para validar resiliencia ante caída de un nodo de base de datos.

**Testing de Seguridad** *(aplica en sistemas expuestos)*
- *Ejemplo:* Un portal de salud que maneja datos de pacientes (HIPAA) necesita validación de seguridad. El SDET configura OWASP ZAP en el pipeline CI para escaneo automático de vulnerabilidades en cada PR, ejecuta penetration testing manual trimestralmente (SQLi, XSS, IDOR), mantiene un threat model actualizado (STRIDE), y define quality gates que bloquean el deploy si hay vulnerabilidades críticas o altas sin remediar.

### SDET vs QA Manual

- **SDET:** Escribe tests automatizados (unit, integration, snapshot, e2e), diseña frameworks de test, define quality gates para CI/CD
- **QA Manual:** Ejecuta tests exploratorios, pruebas de aceptación con el PO, validación visual

Este rol combina ambas facetas: diseña la estrategia y especifica los tests que se automatizarán.

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Estrategia de calidad | Definir niveles de testing, herramientas y enfoque por componente | `estrategia-calidad-motor_v1.0.md` |
| Estrategia de testing | Detallar tipos de test por capa (unit, integration, snapshot, e2e) | `estrategia-testing-motor_v1.0.md` |
| Casos de prueba referenciales | Diseñar test cases representativos para cada componente | `casos-prueba-referenciales_v1.0.md` |
| Criterios de validación | Definir qué se valida en cada etapa del pipeline | `criterios-validacion-motor_v1.0.md` |
| Definition of Done | Establecer criterios objetivos de terminado | `definition-of-done_v1.0.md` |
| Matriz de cobertura | Mapear CU/componentes vs tipos de test vs estado | `matriz-cobertura-pruebas_v1.0.md` |
| Plan de pruebas | Planificar la ejecución de pruebas por sprint/release | `plan-pruebas_v1.0.md` |
| Guía de testing de extensibilidad | Documentar cómo testear plugins/renderers nuevos | `guia-testing-extensibilidad_v1.0.md` |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Estrategia de testing | ISTQB Test Strategy / IEEE 829 | Define el enfoque de QA para todo el proyecto |
| Casos de prueba | IEEE 829 §Test Case Specification | Input para implementación de tests automatizados |
| Definition of Done (DoD) | Scrum Guide 2020 §DoD | Criterio de terminado para todo el equipo (refs en sprint plans) |
| Matriz de cobertura | ISTQB Test Coverage / Traceability Matrix | Dashboard de completitud de testing |
| Plan de pruebas | IEEE 829 §Test Plan | Planificación de ejecución de pruebas |
| Pirámide de tests | Martin Fowler Test Pyramid | Distribución de esfuerzo: muchos unit, menos integration, pocos e2e |
| Quality gates | CI/CD quality gating practices | Criterios de aprobación en pipeline CI/CD |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Pirámide respetada | Distribución: ~70% unit, ~20% integration, ~10% e2e/snapshot | % por tipo de test |
| Cobertura mínima | Componentes críticos tienen ≥ 80% de cobertura | Reporte de cobertura |
| DoD objetiva | Todos los criterios del DoD son verificables mecánicamente | 0 criterios subjetivos |
| SLAs definidos | Performance tiene targets numéricos (latencia, throughput) | Tabla de SLAs |
| Edge cases cubiertos | Templates vacíos, datos nulos, nodos anidados profundos están testeados | ≥ 3 edge cases por componente |
| Matrix actualizada | La matriz refleja el estado real de los tests | 0 estados "Pendiente" en componentes implementados |
| Quality gates en CI | El pipeline falla si no se cumplen los criterios | Build rojo = gate no pasado |

---

## 5. Preguntas guía para el análisis

### 5.1 Estrategia de testing

> **P1:** ¿Qué niveles de testing se usan y qué cubre cada uno?
>
> *Ejemplo práctico:*
> | Nivel | Qué testea | Herramienta | Cantidad esperada |
> |-------|-----------|-------------|-------------------|
> | Unit | Funciones aisladas de cada componente | xUnit + FluentAssertions | ~70% del total |
> | Integration | Pipeline completo DSL→Render | xUnit + fixtures | ~20% del total |
> | Snapshot | Output de renderers vs referencia guardada | Verify (snapshot testing) | ~5% del total |
> | E2E | App MAUI ejecuta pipeline y muestra resultado | Manual / UI test | ~5% del total |

> **P2:** ¿Cómo se testea el renderizado ESC/POS sin hardware?
>
> *Ejemplo práctico:* "Se valida el byte[] generado contra bytes esperados: `Assert.Equal(expected, result.Data)`. No se necesita impresora física."

### 5.2 Casos de prueba

> **P3:** ¿Los test cases cubren el happy path Y los edge cases?
>
> *Ejemplo práctico para el Parser:*
> | Test | Input | Expected | Tipo |
> |------|-------|----------|------|
> | DSL simple | `document { text "Hola" }` | AST con 1 TextNode | Happy path |
> | DSL vacío | `document { }` | AST con 0 children | Edge case |
> | DSL inválido | `{ broken` | `DslParseException` | Error case |
> | DSL con nesting profundo (10 niveles) | `document { group { group { ... } } }` | AST válido | Stress case |

> **P4:** ¿Los criterios de aceptación del CU tienen test case correspondiente?
>
> *Ejemplo práctico:* "CU-06 CA-01: 'Given un documento válido, When se renderiza a ESC/POS, Then los bytes comienzan con ESC @' → Test: `EscPosRenderer_ValidDocument_StartsWithInit()`"

### 5.3 Definition of Done

> **P5:** ¿Los criterios del DoD son objetivos y verificables?
>
> *Ejemplo práctico:*
> - ❌ "El rendimiento es aceptable" → subjetivo
> - ✅ "Parser < 100ms para plantillas < 10KB" → objectivo, medible, automatizable

> **P6:** ¿El DoD incluye SLAs de performance concretos?
>
> *Ejemplo práctico:*
> | Componente | Métrica | SLA |
> |-----------|---------|-----|
> | Parser | Tiempo de parsing | < 100ms para plantillas < 10KB |
> | LayoutEngine | Tiempo de layout | < 50ms para < 1000 nodos |
> | EscPosRenderer | Tiempo de render | < 500ms para documentos < 5KB |
> | End-to-end | DSL → byte[] | < 500ms en dispositivo móvil de referencia |

### 5.4 Quality gates

> **P7:** ¿El pipeline CI/CD falla si no se cumplen los criterios de calidad?
>
> *Ejemplo práctico:* "Quality gate: build falla si cobertura < 80% en core, o si algún test falla, o si hay warnings de análisis estático nuevos."

---

## 6. Plantilla base

### 6.1 Definition of Done

```markdown
# Definition of Done (DoD)

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

Este documento es la fuente canónica de la DoD del proyecto. Los sprint plans 
deben referenciar este documento, no redefinir sus criterios.

## 1. Criterios de código

- [ ] El código compila sin errores ni warnings nuevos
- [ ] Está integrado en la solution principal
- [ ] Cumple con las convenciones de estilo del proyecto

## 2. Criterios de testing

- [ ] Tiene tests unitarios con cobertura ≥ 80% para código nuevo
- [ ] Tests de integración pasan para flujos afectados
- [ ] No rompe tests existentes (regresión)

## 3. Criterios de calidad

- [ ] No existen defectos con etiqueta 'blocker' o 'release-blocker' abiertos
- [ ] Performance cumple SLAs:
  - Parser: < [X]ms / < [X]KB
  - Render: < [X]ms / < [X]KB
  - End-to-end: < [X]ms en dispositivo de referencia

## 4. Criterios de documentación

- [ ] Decisiones técnicas relevantes documentadas
- [ ] API pública documentada con tipos y ejemplos

## 5. Vigencia

Este documento reemplaza cualquier DoD incluida en sprint plans individuales.
```

### 6.2 Caso de prueba

```markdown
# Caso de Prueba — [Nombre del test]

**Componente:** [Parser / Evaluator / Renderer / etc.]  
**Tipo:** [Unit / Integration / Snapshot / E2E]  
**CU relacionado:** CU-XX

---

## Precondiciones

- [Estado inicial requerido]

## Input

`` `json
{
  "template": "[DSL input]",
  "data": { "key": "value" },
  "profile": { "target": "escpos", "width": 32 }
}
`` `

## Output esperado

`` `
[Resultado esperado: AST, byte[], string, etc.]
`` `

## Verificaciones

| # | Qué se verifica | Cómo | Resultado esperado |
|---|-----------------|------|-------------------|
| 1 | [Propiedad] | [Assert/Check] | [Valor] |

## Edge cases

| Variante | Input | Expected |
|----------|-------|----------|
| [Edge case 1] | [Input modificado] | [Resultado] |
```

### 6.3 Matriz de cobertura

```markdown
# Matriz de Cobertura de Pruebas

**Última actualización:** [YYYY-MM-DD]

---

| Componente | CU cubiertos | Unit | Integration | Snapshot | E2E | Cobertura % | Estado |
|-----------|-------------|------|-------------|----------|-----|-------------|--------|
| Parser | CU-01, CU-14 | ✅ 15 | ✅ 3 | ❌ | ❌ | 85% | Completo |
| Evaluator | CU-15-17 | ✅ 20 | ✅ 5 | ❌ | ❌ | 82% | Completo |
| LayoutEngine | CU-04 | ✅ 8 | ✅ 2 | ❌ | ❌ | 75% | En progreso |
| EscPosRenderer | CU-06 | ✅ 12 | ✅ 3 | ✅ 5 | ❌ | 90% | Completo |
| TextRenderer | CU-07 | ✅ 6 | ✅ 2 | ✅ 3 | ❌ | 80% | Completo |
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Solo happy path | Los edge cases no están testeados → bugs en producción | ≥ 3 edge cases por componente |
| DoD subjetiva | "Rendimiento aceptable" cambia según quién la evalúa | SLAs numéricos concretos |
| DoD redefinida por sprint | Cada sprint tiene su propia versión → inconsistencia | Un solo documento canónico |
| Matriz desactualizada | Dice "Pendiente" pero ya hay 50 tests implementados | Actualizar después de cada sprint |
| Tests sin assert | Test que ejecuta código pero no verifica nada | Cada test debe tener ≥ 1 assertion |
| Coverage como meta | 100% coverage pero tests triviales | Priorizar tests con valor: edge cases > getters |
