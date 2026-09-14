# 🤖 Agentes de Refinamiento de Documentación

**Proyecto:** Motor DSL de Generación de Documentos  
**Documento:** tabla-caracterizacion-agentes.md 
**Versión:** 1.0  
**Fecha:** 2026-04-03  
**Propósito:** Definir perfiles de agentes especializados.

---


# 1. Tabla de Caracterización de Agentes

Cada carpeta de la estructura `docs/` es asignada a un agente con el perfil profesional más adecuado para evaluar y refinar su contenido.

| ID | Carpeta | Agente Especializado | Perfil Profesional | Competencias Clave |
|----|---------|---------------------|--------------------|--------------------|
| AG-ROOT | `docs/README.md` | Arquitecto de Soluciones Senior | Visión integral del sistema, comunicación técnica ejecutiva | Coherencia cross-documental, narrativa técnica, estructura de repositorio, onboarding |
| AG-00 | `docs/00_contexto/` | Product Manager / Analista de Negocio | Estrategia de producto, alineación de stakeholders | Visión de producto, alcance, roadmap, acuerdos de equipo, gestión de expectativas |
| AG-01 | `docs/01_necesidades_negocio/` | Analista de Negocio Senior | Elicitación de requisitos, modelado de negocio | Necesidades de negocio, trazabilidad vertical, análisis de stakeholders, métricas de valor |
| AG-02 | `docs/02_especificacion_funcional/` | Analista Funcional / Ingeniero de Requisitos | Especificación formal, modelado de datos | Casos de uso, reglas de negocio, modelo conceptual, DSL, criterios de aceptación BDD |
| AG-03 | `docs/03_ux-ui/` | Especialista en Developer Experience (DX) | Experiencia de uso de APIs y librerías | Flujos de uso del motor, representaciones de salida, wireframes, ergonomía del API |
| AG-04 | `docs/04_prompts_ai/` | Ingeniero de Prompts / AI Specialist | Diseño de prompts, ingeniería de LLMs | Clasificación de documentos, prompts estructurados, guardrails, evaluación de outputs |
| AG-05 | `docs/05_arquitectura_tecnica/` | Arquitecto de Software Senior | Diseño de sistemas, patrones, decisiones técnicas | ADRs, contratos/interfaces, modelos de ejecución, extensibilidad, diagramas C4 |
| AG-06 | `docs/06_backlog-tecnico/` | Scrum Master / Agile Coach | Gestión de backlog, metodología ágil | Product Backlog, DoR, priorización MoSCoW, user stories, estimación, trazabilidad |
| AG-07 | `docs/07_plan-sprint/` | Scrum Master / Gestión de Proyectos Ágiles | Planificación de sprints, métricas ágiles | Sprint planning, velocity, reviews, retrospectivas, burndown, capacidad del equipo |
| AG-08 | `docs/08_calidad_y_pruebas/` | Ingeniero QA / SDET Senior | Estrategia de calidad, automatización de pruebas | Testing strategy, cobertura, DoD, casos de prueba, matrices de validación, quality gates |
| AG-09 | `docs/09_devops/` | Ingeniero DevOps Senior | CI/CD, infraestructura, automatización de releases | Pipelines, versionado SemVer, entornos de deploy, publicación NuGet, rollback, secretos |
| AG-10 | `docs/10_developer_guide/` | Technical Writer / Developer Advocate | Documentación técnica orientada al consumidor | Guías de integración, tutoriales paso a paso, ejemplos de código, troubleshooting |
| AG-11 | `docs/11_examples/` | Developer Advocate / Ingeniero de Aplicación | Desarrollo de ejemplos didácticos, aplicaciones de referencia | Ejemplos funcionales completos, progresión de complejidad, reproducibilidad, buenas prácticas |