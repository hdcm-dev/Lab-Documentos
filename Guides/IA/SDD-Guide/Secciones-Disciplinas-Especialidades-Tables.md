# Resumen de la Documentación (`/docs/`) y Disciplinas del Proyecto

---

## Resumen por Sección

| # | Sección | Propósito |
|---|---------|-----------|
| **00** | `00_contexto/` | **Contexto estratégico.** Visión del producto, alcance, compatibilidad de plataformas, roadmap y acuerdo de equipo. Define el *por qué* del proyecto. |
| **01** | `01_necesidades_negocio/` | **Necesidades de negocio.** Identifica 6 problemas de negocio (NB-01 a NB-06): desacoplamiento datos/diseño/dispositivo, estandarización, evolución de templates sin recompilar, soporte multiformato, versionado y reducción de costos. |
| **02** | `02_especificacion_funcional/` | **Especificación funcional.** Traduce las necesidades en casos de uso (CU-01 a CU-32), reglas de negocio (RN-XX), modelo de datos conceptual y la gramática del DSL (JSON). |
| **03** | `03_ux-ui/` | **Experiencia de desarrollador (DX).** Diseño de la experiencia de uso del motor: modos de impresión (ESC/POS), vista previa (MAUI) y debug (texto). Wireframes de documentos de salida. |
| **04** | `04_prompts_ai/` | **Prompts para IA.** Prompts estructurados para clasificación de documentos DSL mediante LLMs. Incluye esquemas JSON, few-shot examples y guardrails. |
| **05** | `05_arquitectura_tecnica/` | **Arquitectura técnica.** Pipeline de transformación (DSL → Parser → AST → Evaluador → Layout → Renderer), ADRs, contratos/interfaces, extensibilidad, modelo lógico de datos y ejecución. |
| **06** | `06_backlog-tecnico/` | **Backlog del producto.** 31 User Stories (US-01 a US-31) priorizadas con MoSCoW, backlog técnico (BT-XX), Definition of Ready. |
| **07** | `07_plan-sprint/` | **Planificación de sprints.** 8 planes de iteración (2 semanas c/u), templates de review/retrospectiva, tracking de velocidad y burndown. |
| **08** | `08_calidad_y_pruebas/` | **Calidad y pruebas.** Estrategia de testing (pirámide: 70% unit, 20% integración, 5% snapshot, 5% E2E), Definition of Done, matriz de cobertura, quality gates. |
| **09** | `09_devops/` | **DevOps / CI-CD.** Pipeline CI/CD (build → test → quality gates → pack → publish NuGet), estrategia SemVer 2.0, entornos de deploy, guía de publicación. |
| **10** | `10_developer_guide/` | **Guía para consumidores.** Documentación externa: quick start, formato DSL, perfiles de impresora, integración MAUI, API REST, templates de ejemplo. |
| **11** | `11_examples/` | **Aplicaciones de referencia.** 3 ejemplos progresivos ejecutables: ticket simple (básico), acta de infracción (intermedio) y consumo via NuGet (avanzado). |

---

## Tabla de Especialidades y Disciplinas

| ID | Rol / Especialidad | Disciplina | Sección que produce | Artefactos clave |
|---|---|---|---|---|
| **AG-ROOT** | Arquitecto de Soluciones Senior | Gestión integral / Gobernanza | `docs/README.md` | Árbol documental, coherencia cross-documental |
| **AG-00** | Product Manager | Estrategia de producto | `00_contexto/` | Visión, alcance, roadmap, acuerdo de equipo |
| **AG-01** | Analista de Negocio Senior | Ingeniería de requisitos de negocio | `01_necesidades_negocio/` | Necesidades de negocio (NB-XX), criterios de éxito |
| **AG-02** | Analista Funcional / Ing. de Requisitos | Especificación funcional | `02_especificacion_funcional/` | Casos de uso (CU-XX), reglas de negocio (RN-XX), definición DSL |
| **AG-03** | Especialista en Developer Experience (DX) | Diseño de experiencia de API | `03_ux-ui/` | Modos de uso, wireframes, representaciones de salida |
| **AG-04** | Ingeniero de Prompts / Especialista IA | Ingeniería de prompts / LLMs | `04_prompts_ai/` | Prompts estructurados, esquemas JSON, guardrails |
| **AG-05** | Arquitecto de Software Senior | Arquitectura de software | `05_arquitectura_tecnica/` | ADRs, contratos, pipeline, diagramas, extensibilidad |
| **AG-06** | Scrum Master / Agile Coach (Backlog) | Gestión ágil de backlog | `06_backlog-tecnico/` | Product Backlog (US-XX), Backlog Técnico (BT-XX), DoR |
| **AG-07** | Scrum Master / Gestión de Proyectos Ágiles | Planificación y seguimiento ágil | `07_plan-sprint/` | Sprint Plans (×8), review/retro templates, velocidad |
| **AG-08** | Ingeniero QA / SDET Senior | Aseguramiento de calidad | `08_calidad_y_pruebas/` | Estrategia testing, DoD, matriz cobertura, quality gates |
| **AG-09** | Ingeniero DevOps Senior | CI/CD e infraestructura | `09_devops/` | Pipeline CI/CD, SemVer, deploy, publicación NuGet |
| **AG-10** | Technical Writer | Documentación técnica | `10_developer_guide/` | Guías de integración, referencia DSL, tutoriales |
| **AG-11** | Developer Advocate / Ing. de Aplicación | Evangelización y ejemplos | `11_examples/` | 3 apps de referencia ejecutables, assets |

---

## Flujo de Trazabilidad entre Disciplinas

```
AG-00 (Visión/Alcance)
  → AG-01 (Necesidades de negocio)
    → AG-02 (Especificación funcional + DSL)
      → AG-03 (Experiencia de uso)
      → AG-04 (Prompts IA)
      → AG-05 (Arquitectura técnica)
      → AG-06 (Backlog priorizado)
        → AG-07 (Sprint Plans)
          → AG-08 (Calidad y pruebas)
          → AG-09 (CI/CD y deploy)
            → AG-10 (Guía para desarrolladores)
              → AG-11 (Ejemplos ejecutables)
```

**13 roles/disciplinas** cubren el ciclo completo: desde la estrategia de producto hasta las aplicaciones de referencia, siguiendo el enfoque **SDD (Specification-Driven Development)** donde cada capa documental alimenta a la siguiente y cada sección tiene un responsable claro.

---

## Mapeo Agente → Documentación (Entradas y Salidas)

| Sección Docs | Agente | Entrada upstream | Salida downstream |
|---|---|---|---|
| `00_contexto` | AG-00 | Investigación de mercado, stakeholders | Alcance y visión → AG-01 |
| `01_necesidades_negocio` | AG-01 | Contexto de negocio (AG-00) | User stories, épicas → AG-06 |
| `02_especificacion_funcional` | AG-02 | Necesidades (NB-XX) de AG-01 | Restricciones arquitectura → AG-05 |
| `03_ux-ui` | AG-03 | Casos de uso (CU-XX) de AG-02 | Wireframes → AG-05 (diseño API), AG-10 (docs) |
| `04_prompts_ai` | AG-04 | Specs funcionales de AG-02 | Prompts operacionales para features con LLM |
| `05_arquitectura_tecnica` | AG-05 | Specs + DX de AG-02 + AG-03 | Blueprint de implementación para devs |
| `06_backlog-tecnico` | AG-06 | CU-XX de AG-02, priorización de AG-00 | Sprint backlog → AG-07 |
| `07_plan-sprint` | AG-07 | Backlog priorizado de AG-06 | Roadmap de ejecución, compromiso del equipo |
| `08_calidad_y_pruebas` | AG-08 | CU-XX + criterios aceptación de AG-02 | Quality gates → AG-09 (CI/CD), DoD para todos |
| `09_devops` | AG-09 | DoD de AG-08, versionado de AG-00 | Pipeline automatizado build/test/publish |
| `10_developer_guide` | AG-10 | DSL spec (AG-02), contratos API (AG-05) | Documentación consumible para desarrolladores |
| `11_examples` | AG-11 | Guía dev (AG-10), arquitectura (AG-05) | Código de referencia, feedback → AG-10 |
