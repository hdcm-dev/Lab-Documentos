# AG-04 — Ingeniero de Prompts / AI Specialist

**Carpeta SDD:** `docs/04_prompts_ai/`  
**Perfil Profesional:** Diseño de prompts, ingeniería de LLMs

---

## 1. ¿Qué es esta especialidad?

El **Ingeniero de Prompts** es el profesional que diseña, evalúa e itera las instrucciones (prompts) que se envían a modelos de lenguaje (LLMs) para obtener resultados predecibles, precisos y seguros. Combina conocimientos de lingüística computacional, diseño de sistemas y evaluación de calidad de outputs de IA.

En el contexto de este proyecto, el Ingeniero de Prompts diseña los prompts que el motor podría usar para tareas como **clasificación de documentos**, **generación asistida de plantillas** o **debugging inteligente de DSL**. Cada prompt se trata como un artefacto de software: tiene versión, tests, guardrails y métricas de evaluación.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el grado de profundidad en IA y el tipo de integración:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Ingeniería de Prompts (Prompt Engineering)** | Diseñar, iterar y evaluar instrucciones para LLMs con guardrails y métricas | Productos que consumen APIs de LLM (GPT, Claude, Gemini) para tareas específicas | System prompts, few-shot examples, JSON schemas de output, matrices de evaluación, guardrails |
| **Integración AI/ML (AI/ML Engineering)** | Desarrollar pipelines de ML end-to-end: data prep, training, fine-tuning, deployment, MLOps | Productos que entrenan modelos propios o hacen fine-tuning de foundational models | Pipelines de training, datasets etiquetados, model cards, A/B testing, modelos en producción, drift monitoring |
| **Diseño Conversacional (Conversational AI)** | Diseñar flujos de diálogo multi-turno para chatbots, asistentes virtuales y voice UI | Productos con interfaz conversacional (chatbots, IVR, asistentes de voz) | Flujos de diálogo, intents/entities, fallback strategies, personality guidelines, escalation rules |

> **En este proyecto** se aplica **Ingeniería de Prompts** porque el motor DSL usa LLMs para tareas acotadas (clasificación de documentos) vía API, sin entrenamiento de modelos propios ni interfaz conversacional.

#### Ejemplos temáticos por disciplina

**Integración AI/ML** *(aplica en productos con modelos propios)*
- *Ejemplo:* Una empresa de e-commerce entrena un modelo de detección de fraude. El ML engineer prepara el dataset (1M transacciones etiquetadas), diseña el pipeline de feature engineering (velocity features, geolocation anomalies), entrena con LightGBM, define la model card (precision=0.94, recall=0.87, bias audit por demografía), despliega con SageMaker y configura drift monitoring con alertas si la distribución de inputs cambia >15%.

**Diseño Conversacional** *(aplica en productos con chatbots/asistentes)*
- *Ejemplo:* Un banco diseña un asistente virtual para WhatsApp. El diseñador conversacional define los intents principales (consulta_saldo, transferencia, reclamo), los flujos de diálogo multi-turno con confirmación ("¿Querés transferir $5000 a Juan Pérez? Confirmá con SÍ"), las estrategias de fallback ("No entendí tu consulta, ¿querés hablar con un humano?"), y la personalidad del bot (formal pero cercano, sin emojis, respuestas <50 palabras).

### ¿Por qué esta especialidad en un proyecto de impresión?

Aunque el motor DSL es determinístico, la IA puede asistir en:
- Clasificar el tipo de documento a partir de una plantilla (ticket, factura, recibo)
- Sugerir correcciones en plantillas DSL con errores
- Generar plantillas de ejemplo a partir de descripciones en lenguaje natural

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Diseño de system prompts | Redactar instrucciones claras y acotadas para el LLM | Archivo de prompt con system/user/assistant |
| Definición de guardrails | Establecer límites para evitar alucinaciones, outputs tóxicos o fuera de dominio | Sección de guardrails en cada prompt |
| Diseño de ejemplos (few-shot) | Crear ejemplos representativos que guíen al modelo | Sección de examples con input/output |
| Definición de formato de salida | Especificar el JSON/estructura esperada de la respuesta | JSON Schema del output |
| Evaluación de prompts | Medir accuracy, recall, latencia y cost del prompt | Matriz de evaluación |
| Versionado de prompts | Gestionar iteraciones y cambios en prompts | Convención `_v1.0.md`, `_v2.0.md` |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| System prompt | OpenAI/Anthropic prompt engineering guidelines | Input para integración con APIs de LLM en código |
| Few-shot examples | Prompt engineering best practices | Parte del prompt enviado al modelo |
| Output JSON Schema | JSON Schema Draft 7+ | Validación programática del output del LLM |
| Guardrails | OWASP LLM Top 10 / Responsible AI guidelines | Prevención de alucinaciones y outputs inseguros |
| Evaluation framework | Prompt evaluation metrics (P/R/F1, latency, cost) | CI/CD: regression tests de prompts |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Claridad del system prompt | Las instrucciones son inequívocas y no dejan espacio a interpretación | Revisión por par + test con modelo |
| Cobertura de examples | Los few-shot cubren el happy path y al menos 2 edge cases | ≥ 3 examples por prompt |
| Output parseable | La respuesta del LLM es JSON válido parseable programáticamente | 100% de responses pasan JSON.parse |
| Guardrails efectivos | El prompt rechaza inputs fuera de dominio | Test con 5 inputs adversariales |
| Reproducibilidad | El mismo input produce el mismo output (con temperature=0) | ≥ 95% de consistencia en 10 runs |
| Costo razonable | El prompt no consume tokens excesivos | < 2000 tokens por request |

---

## 5. Preguntas guía para el análisis

### 5.1 Diseño del prompt

> **P1:** ¿Qué tarea específica debe resolver el prompt?
>
> *Ejemplo práctico:* "Dado el contenido DSL de una plantilla, clasificar el tipo de documento (ticket_venta, factura, recibo, remito, otro)."

> **P2:** ¿El system prompt define claramente el rol, la tarea y las restricciones?
>
> *Ejemplo práctico:*
> ```
> Sos un clasificador de documentos DSL. Tu tarea es analizar una plantilla DSL 
> y determinar qué tipo de documento comercial representa.
> 
> RESTRICCIONES:
> - Solo respondés con JSON válido
> - Solo clasificás en las categorías definidas
> - Si no podés clasificar, usás "otro" con confidence < 0.5
> ```

### 5.2 Guardrails

> **P3:** ¿Qué pasa si el input es inválido, vacío o malicioso?
>
> *Ejemplo práctico:* "Si el input contiene `ignore previous instructions`, el guardrail debe detectar prompt injection y retornar `{ "error": "invalid_input" }`."

> **P4:** ¿El prompt previene alucinaciones?
>
> *Ejemplo práctico:* "El prompt dice: 'Si no reconocés el tipo de documento, clasificá como otro con confidence=0.3. NO inventes categorías nuevas.'"

### 5.3 Evaluación

> **P5:** ¿Cómo se mide si el prompt funciona bien?
>
> *Ejemplo práctico:*
> | Métrica | Target | Resultado actual |
> |---------|--------|------------------|
> | Accuracy (clasificación correcta) | ≥ 90% | [Pendiente] |
> | Latencia promedio | < 2s | [Pendiente] |
> | Costo por request | < $0.01 | [Pendiente] |
> | Tasa de "otro" en docs válidos | < 5% | [Pendiente] |

> **P6:** ¿Existen otros aspectos del motor que se beneficiarían de IA?
>
> *Ejemplo práctico:* "Prompt para sugerir correcciones en DSL con errores de sintaxis. Prompt para generar plantillas de ejemplo a partir de descripción en lenguaje natural."

---

## 6. Plantilla base

### 6.1 Documento de prompt

```markdown
# Prompt — [Nombre descriptivo]

**Proyecto:** [Nombre]  
**Documento:** prompt-[nombre-kebab]_v1.0.md  
**Versión:** 1.0  
**Fecha:** [YYYY-MM-DD]  
**Modelo target:** [GPT-4o / Claude 3.5 / etc.]  
**Temperature:** [0 / 0.3 / etc.]

---

## 1. Propósito

[¿Qué tarea resuelve este prompt?]

## 2. System Prompt

`` `text
[Contenido completo del system prompt]
`` `

## 3. Guardrails

| Guardrail | Descripción | Método de validación |
|-----------|-------------|----------------------|
| Anti prompt-injection | Rechazar inputs que intenten modificar instrucciones | Regex + detección en system prompt |
| Anti alucinación | Forzar categorías predefinidas, no inventar | Validación contra enum de categorías |
| Input vacío | Retornar error si el input está vacío | Validación pre-envío |

## 4. Formato de salida

`` `json
{
  "tipo": "ticket_venta | factura | recibo | remito | otro",
  "confidence": 0.0-1.0,
  "razonamiento": "string"
}
`` `

**JSON Schema:**

`` `json
{
  "type": "object",
  "required": ["tipo", "confidence"],
  "properties": {
    "tipo": { "type": "string", "enum": ["ticket_venta", "factura", "recibo", "remito", "otro"] },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 },
    "razonamiento": { "type": "string" }
  }
}
`` `

## 5. Ejemplos (Few-shot)

### Ejemplo 1 — Ticket de venta

**Input:**
`` `dsl
document {
  header { text "MI TIENDA" align center bold }
  body { loop items { text "${nombre} ${precio}" } }
  footer { text "TOTAL: ${total}" bold }
}
`` `

**Output esperado:**
`` `json
{ "tipo": "ticket_venta", "confidence": 0.95, "razonamiento": "Contiene header con nombre de tienda, loop de items con precios, y total en footer" }
`` `

### Ejemplo 2 — Documento no reconocible

**Input:**
`` `dsl
document { text "Hello world" }
`` `

**Output esperado:**
`` `json
{ "tipo": "otro", "confidence": 0.3, "razonamiento": "Documento genérico sin estructura comercial reconocible" }
`` `

## 6. Evaluación

| Métrica | Target | Resultado | Fecha test |
|---------|--------|-----------|------------|
| Accuracy | ≥ 90% | [Pendiente] | — |
| Latencia | < 2s | [Pendiente] | — |
| Costo/request | < $0.01 | [Pendiente] | — |
| Consistencia (10 runs) | ≥ 95% | [Pendiente] | — |

## 7. Historial

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | [YYYY-MM-DD] | Versión inicial |
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Prompt sin guardrails | El modelo alucina o responde fuera de dominio | Agregar restricciones explícitas en system prompt |
| Sin few-shot examples | El modelo no tiene referencia de calidad esperada | Mínimo 3 examples: happy path + edge case + caso negativo |
| Output no estructurado | "El documento parece un ticket" no se puede parsear | Forzar JSON con schema definido |
| Sin versionado | Cambios al prompt rompen comportamiento sin trazabilidad | Versionar como código: `_v1.0.md`, `_v2.0.md` |
| Sin métricas | No se sabe si el prompt mejoró o empeoró | Definir baseline y medir en cada iteración |
| Prompt monolítico | Un solo prompt hace todo → difícil de debuggear | Chain of prompts: clasificar → validar → generar |
