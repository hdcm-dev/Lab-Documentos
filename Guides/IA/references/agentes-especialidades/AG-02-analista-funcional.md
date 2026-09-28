# AG-02 — Analista Funcional / Ingeniero de Requisitos

**Carpeta SDD:** `docs/02_especificacion_funcional/`  
**Perfil Profesional:** Especificación formal, modelado de datos

---

## 1. ¿Qué es esta especialidad?

El **Analista Funcional** (también llamado **Ingeniero de Requisitos**) es el profesional que transforma las necesidades de negocio en especificaciones funcionales precisas y verificables. Define *qué debe hacer el sistema* (no cómo), usando formalismos como casos de uso, reglas de negocio, modelos de datos y criterios de aceptación.

En el contexto SDD, este rol produce la capa más detallada de requisitos: los **casos de uso (CU)** individuales, las **reglas de negocio (RN)**, la **definición del DSL**, el **modelo de datos conceptual** y la **matriz de trazabilidad** que conecta todo hacia arriba (NB) y hacia abajo (arquitectura, tests).

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según la naturaleza del sistema y la profundidad del análisis:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Análisis Funcional de Software** | Especificar *qué debe hacer el sistema* con formalismos (CU, RN, flujos, criterios BDD) | Todo proyecto de software que requiera especificación verificable | Casos de uso (CU), reglas de negocio (RN), criterios BDD, matriz de trazabilidad |
| **Modelado de Dominio (Domain Modeling)** | Capturar los conceptos, entidades y relaciones del dominio de negocio con DDD/OO | Dominios complejos con lógica de negocio rica (banca, logística, salud) | Ubiquitous Language, Bounded Contexts, Aggregates, diagramas de dominio, Event Storming |
| **Análisis de Datos (Data Analysis/Engineering)** | Definir esquemas de datos, flujos ETL, governance y linaje de datos | Proyectos data-intensive, BI/analytics, data lakes, migraciones | ERD, diccionarios de datos, data lineage, esquemas de transformación, data quality rules |

> **En este proyecto** se aplica **Análisis Funcional de Software** porque el motor DSL requiere especificación formal de cada caso de uso (pipeline, nodos, renderers) con trazabilidad hacia las necesidades de negocio.

#### Ejemplos temáticos por disciplina

**Modelado de Dominio** *(aplica en dominios complejos con DDD)*
- *Ejemplo:* Una plataforma de seguros necesita modelar pólizas con múltiples coberturas, beneficiarios, cláusulas condicionales y renovaciones. El analista aplica Event Storming para descubrir los domain events (`PolicyIssued`, `ClaimFiled`, `CoverageActivated`), define Bounded Contexts (Underwriting, Claims, Billing) y establece el Ubiquitous Language que comparten negocio y desarrollo.

**Análisis de Datos** *(aplica en proyectos data-intensive)*
- *Ejemplo:* Una empresa de retail quiere unificar datos de ventas de 200 sucursales con 3 sistemas POS distintos. El analista de datos define el esquema destino normalizado, las reglas de transformación ETL (mapeo de SKUs, conversión de monedas), las data quality rules (no nulls en `transaction_id`, formato ISO de fechas) y el linaje de datos desde cada fuente hasta el data warehouse.

### Marco de referencia profesional

Se alinea con el cuerpo de conocimiento **IREB CPRE** (Certified Professional for Requirements Engineering), particularmente en elicitación, documentación, validación y gestión de requisitos.

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Especificación de casos de uso | Documentar cada funcionalidad con actores, flujos, precondiciones y postcondiciones | Archivos CU-XX individuales |
| Definición de reglas de negocio | Formalizar restricciones y políticas del dominio | Archivos RN-XX individuales |
| Definición del DSL | Especificar la gramática y los elementos del lenguaje de plantillas | `definicion-dsl_v1.0.md` |
| Modelado de datos conceptual | Definir las entidades y relaciones del dominio | Archivos en `modelo-datos/` |
| Criterios de aceptación BDD | Escribir criterios en formato Given/When/Then | Sección dentro de cada CU |
| Matriz de trazabilidad | Mapear NB → CU → RN → componentes | Tabla en `especificacion-funcional_v1.0.md` |
| Gestión de versiones de CU | Marcar CU deprecados, mantener v1.0/v2.0 | Nomenclatura en archivos |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Casos de uso | UML Use Case Diagrams / Alistair Cockburn format | Input para desarrollo (sprint backlog), testing (test cases) y arquitectura (contratos) |
| Reglas de negocio | SBVR (Semantics of Business Vocabulary and Rules) | Input para validaciones en código y criterios de aceptación |
| Modelo de datos conceptual | UML Class Diagram / ER Diagram | Input para el modelo de datos lógico del arquitecto |
| Criterios de aceptación | BDD (Behavior-Driven Development) — Given/When/Then | Input directo para test cases automatizados |
| Especificación DSL | EBNF (Extended Backus-Naur Form) / JSON Schema | Input para el parser y el validador del motor |
| Matriz de trazabilidad | IEEE 830 §3.5 / IREB traceability | Valida completitud bidireccional del proyecto |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Estructura uniforme | Todos los CU tienen la misma estructura interna | Checklist de secciones por CU |
| Criterios BDD | Los criterios de aceptación usan Given/When/Then | 100% de CU con al menos 2 criterios BDD |
| CU deprecados marcados | Si existe v2.0, v1.0 tiene nota de deprecación | 0 CU v1.0 sin nota cuando existe v2.0 |
| Trazabilidad completa | La matriz cubre TODOS los CU existentes | 0 CU sin fila en la matriz |
| RN verificables | Cada RN es comprobable por el sistema | 0 RN ambiguas o subjetivas |
| Nomenclatura consistente | `CU-XX-nombre-kebab_v1.0.md` | 0 excepciones |
| Sin contaminación | No hay documentos de otros proyectos | 0 archivos ajenos |

---

## 5. Preguntas guía para el análisis

### 5.1 Casos de uso

> **P1:** ¿Quién es el actor principal de este caso de uso?
>
> *Ejemplo práctico:* "CU-13 (Cargar plantilla DSL) → Actor: Sistema Cliente (la aplicación .NET MAUI que consume el motor)."

> **P2:** ¿Cuál es el flujo principal paso a paso?
>
> *Ejemplo práctico:*
> 1. El Sistema Cliente envía el identificador de plantilla al Motor DSL
> 2. El Motor busca la plantilla en el proveedor registrado
> 3. Si la plantilla existe, la retorna como string DSL
> 4. Si no existe, lanza `TemplateNotFoundException`

> **P3:** ¿Qué flujos alternativos y de error existen?
>
> *Ejemplo práctico:* "FA-01: Plantilla no encontrada → El motor retorna error con código `TEMPLATE_NOT_FOUND` y mensaje descriptivo."

> **P4:** ¿Los criterios de aceptación son verificables automáticamente?
>
> *Ejemplo práctico:*
> - **Given** una plantilla válida registrada con ID "ticket-venta"
> - **When** el Sistema Cliente solicita cargar "ticket-venta"
> - **Then** el motor retorna el contenido DSL completo sin errores

### 5.2 Reglas de negocio

> **P5:** ¿La regla es una restricción del dominio o una decisión de implementación?
>
> *Ejemplo práctico:* ✅ "RN-01: Toda plantilla debe cumplir el esquema DSL definido" (restricción del dominio) vs ❌ "Usar JSON para el parser" (decisión de implementación → va en ADR)

> **P6:** ¿La regla es verificable automáticamente?
>
> *Ejemplo práctico:* "RN-03: La estructura del documento debe ser jerárquica" → verificable: el validador rechaza documentos con nodos sin padre."

### 5.3 Trazabilidad

> **P7:** ¿Cada CU traza a una necesidad de negocio?
>
> *Ejemplo práctico:* "CU-06 (Renderizar ESC/POS) → NB-04 (Multi-dispositivo) + RN-03 (Jerarquía) + RN-06 (Perfil condiciona salida)"

> **P8:** ¿Hay CU que no aparecen en la matriz de trazabilidad?
>
> *Ejemplo práctico:* "CU-30, CU-31, CU-32 (manejo de errores) faltaban en la matriz original → agregarlos con NB-06"

### 5.4 DSL

> **P9:** ¿Los elementos DSL documentados coinciden con los nodos implementados en los CU?
>
> *Ejemplo práctico:* "La definición DSL documenta `TextNode`, `ConditionalNode`, `LoopNode`. ¿`TableNode` (usado en Sprint 05) ya está en la definición?"

---

## 6. Plantilla base

### 6.1 Caso de uso individual

```markdown
# CU-XX — [Nombre del caso de uso]

**Proyecto:** [Nombre]  
**Documento:** CU-XX-nombre-kebab_v1.0.md  
**Versión:** 1.0  
**Fecha:** [YYYY-MM-DD]  
**Estado:** [Activo / Deprecado → ver v2.0]

---

## 1. Descripción

[Breve descripción del caso de uso en 1-2 oraciones.]

## 2. Actores

| Actor | Tipo | Descripción |
|-------|------|-------------|
| [Actor 1] | Principal | [Qué hace] |
| [Actor 2] | Secundario | [Qué hace] |

## 3. Precondiciones

- [Condición que debe cumplirse antes de ejecutar el CU]

## 4. Postcondiciones

- [Estado del sistema después de la ejecución exitosa]

## 5. Flujo Principal

| Paso | Actor | Acción |
|------|-------|--------|
| 1 | [Actor] | [Acción] |
| 2 | Sistema | [Respuesta] |
| 3 | ... | ... |

## 6. Flujos Alternativos

### FA-01 — [Nombre del flujo alternativo]

| Paso | Condición | Acción |
|------|-----------|--------|
| 2a | [Condición de desvío] | [Acción alternativa] |

## 7. Flujos de Error

### FE-01 — [Nombre del error]

| Paso | Error | Respuesta del sistema |
|------|-------|-----------------------|
| 2b | [Error] | [Código + mensaje] |

## 8. Reglas de Negocio Aplicables

- RN-XX: [Descripción]

## 9. Criterios de Aceptación

### CA-01
- **Given** [contexto]
- **When** [acción]
- **Then** [resultado esperado]

### CA-02
- **Given** [contexto]
- **When** [acción]
- **Then** [resultado esperado]

## 10. Trazabilidad

| Elemento | Referencia |
|----------|------------|
| Necesidad de negocio | NB-XX |
| Reglas de negocio | RN-XX, RN-YY |
| Componente arquitectónico | [Componente] |
| Sprint | Sprint XX |

## 11. Historial

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | [YYYY-MM-DD] | Versión inicial |
```

### 6.2 Regla de negocio

```markdown
# RN-XX — [Nombre de la regla]

**Versión:** 1.0  
**Tipo:** [Restricción / Derivación / Inferencia]

---

## Enunciado

[Enunciado formal de la regla]

## Justificación

[¿Por qué existe esta regla?]

## Aplicabilidad

- Aplica a: [CU-XX, CU-YY]
- Validada por: [Componente del sistema]

## Ejemplos

| Escenario | Input | Resultado | ¿Cumple RN? |
|-----------|-------|-----------|--------------|
| Caso válido | [Input] | [Output] | ✅ |
| Caso inválido | [Input] | [Rechazo] | ❌ |
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| CU sin escenarios de error | Solo flujo feliz → el sistema no maneja excepciones | Agregar al menos 1 flujo de error por CU |
| Criterios narrativos | "El sistema debe funcionar correctamente" no se puede automatizar | Formato Given/When/Then con valores concretos |
| Matriz incompleta | CU nuevos no se agregan a la matriz | Regla: todo PR que agregue CU debe actualizar la matriz |
| RN como implementación | "Usar JSON Schema" es decisión técnica, no regla de negocio | Separar en ADR (arquitectura) vs RN (negocio) |
| CU v1.0 sin deprecación | Existe v2.0 pero v1.0 no dice nada → confusión | Header: "Estado: Deprecado → ver CU-XX_v2.0.md" |
| Documento ajeno | `proceso-gestion-reclamo.md` de otro proyecto | Revisar que cada archivo pertenece al dominio |
