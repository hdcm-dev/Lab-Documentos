# 🤖 Agentes de Refinamiento de Documentación

**Proyecto:** Motor DSL de Generación de Documentos  
**Documento:** agentes-refinamiento-docs.md  
**Versión:** 1.0  
**Fecha:** 2026-04-03  
**Propósito:** Definir perfiles de agentes especializados y prompt maestro para el refinamiento sistemático de la documentación del proyecto.

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

---

# 2. Detalle de Competencias y Criterios de Refinamiento por Agente

## AG-ROOT — Arquitecto de Soluciones Senior

**Archivos a revisar:**
- `docs/README.md`

**Criterios de refinamiento:**
- Verificar que todos los links internos apuntan a archivos existentes
- Validar que el árbol de carpetas refleja la estructura real del disco
- Asegurar que la narrativa es comprensible para un desarrollador nuevo en 5 minutos
- Verificar coherencia entre lo descrito en el README y lo que existe en cada subcarpeta
- Evaluar si el roadmap de alto nivel está alineado con `00_contexto/roadmap-producto_v1.0.md`
- Confirmar que la sección Definition of Done referencia al archivo canónico

**Entregable esperado:** Lista de inconsistencias cross-documentales y propuestas de corrección.

---

## AG-00 — Product Manager / Analista de Negocio

**Archivos a revisar:**
- `docs/00_contexto/vision-producto_v1.0.md`
- `docs/00_contexto/alcance-proyecto_v1.0.md`
- `docs/00_contexto/compatibilidad-plataformas_v1.0.md`
- `docs/00_contexto/acuerdo-equipo_v1.0.md`
- `docs/00_contexto/roadmap-producto_v1.0.md`

**Criterios de refinamiento:**
- Verificar que la visión de producto está claramente articulada y diferenciada
- Evaluar si el alcance tiene criterios de exclusión explícitos y justificados
- Validar que el roadmap conecta fases → épicas → sprints → releases de forma trazable
- Confirmar que el acuerdo de equipo tiene los campos pendientes identificados como acción
- Evaluar si las métricas de éxito son medibles (SMART)
- Revisar coherencia entre alcance y visión de producto

**Entregable esperado:** Gaps de alineación estratégica y propuestas de mejora en claridad/medibilidad.

---

## AG-01 — Analista de Negocio Senior

**Archivos a revisar:**
- `docs/01_necesidades_negocio/necesidades-negocio_v1.0.md`
- `docs/01_necesidades_negocio/necesidades-de-negocio/NB-01-Desacople.v1.0.md`
- `docs/01_necesidades_negocio/necesidades-de-negocio/NB-02 Estandarización.v1.0.md`
- `docs/01_necesidades_negocio/necesidades-de-negocio/NB-03-Cambios-sin-codigo.v1.0.md`
- `docs/01_necesidades_negocio/necesidades-de-negocio/NB-04-Multi-dispositivo.v1.0.md`
- `docs/01_necesidades_negocio/necesidades-de-negocio/NB-05-Reduccion-de-costos.v1.0.md`
- `docs/01_necesidades_negocio/necesidades-de-negocio/NB-06-Reutilizacion.v1.0.md`

**Criterios de refinamiento:**
- Verificar que cada NB tiene: problema claro, impacto medible, criterios de éxito
- Evaluar la trazabilidad NB → CU (cada necesidad debe trazar a al menos un caso de uso)
- Identificar necesidades de negocio implícitas no documentadas
- Revisar consistencia entre el resumen (necesidades-negocio_v1.0.md) y los archivos individuales
- Señalar inconsistencias de nomenclatura en nombres de archivo (espacios, acentos, separadores)
- Evaluar si los stakeholders están identificados con nombre/rol o son genéricos

**Entregable esperado:** Mapa de trazabilidad NB→CU completo, gaps identificados, propuestas de renombrado.

---

## AG-02 — Analista Funcional / Ingeniero de Requisitos

**Archivos a revisar:**
- `docs/02_especificacion_funcional/especificacion-funcional_v1.0.md`
- `docs/02_especificacion_funcional/definicion-dsl_v1.0.md`
- `docs/02_especificacion_funcional/casos-de-uso/CU-*.md` (42 archivos)
- `docs/02_especificacion_funcional/reglas-de-negocio/RN-*.md` (6 archivos)
- `docs/02_especificacion_funcional/modelo-datos/`
- `docs/02_especificacion_funcional/procesos/`

**Criterios de refinamiento:**
- Verificar que cada CU sigue la misma estructura interna (actores, precondiciones, flujo principal, flujos alternativos, postcondiciones, criterios de validación)
- Validar que los criterios de aceptación están en formato Given/When/Then
- Confirmar que los CU deprecados (v1.0 con v2.0 disponible) están marcados
- Verificar que la matriz de trazabilidad cubre TODOS los CU existentes (incluidos CU-30/31/32)
- Evaluar completitud de las reglas de negocio RN-01 a RN-06
- Identificar CU con nomenclatura inconsistente (CU-04, CU-23 tienen formato distinto)
- Verificar que el documento de procesos no contenga artefactos de otros proyectos
- Evaluar la definición DSL contra los nodos realmente implementados en los casos de uso

**Entregable esperado:** Checklist de conformidad por CU, gaps de trazabilidad, propuestas de estandarización.

---

## AG-03 — Especialista en Developer Experience (DX)

**Archivos a revisar:**
- `docs/03_ux-ui/experiencia-de-uso-del-motor_v1.0.md`
- `docs/03_ux-ui/representacion-documento-escpos_v1.0.md`
- `docs/03_ux-ui/representacion-vista-previa-ui_v1.0.md`
- `docs/03_ux-ui/wireframes-documentos_v1.0.md`

**Criterios de refinamiento:**
- Evaluar si los flujos de uso del motor son comprensibles para un desarrollador sin contexto previo
- Verificar que los modos de uso (Printing, Preview, Debug) están claramente diferenciados
- Validar que los parámetros de configuración están documentados con tipos, valores por defecto y ejemplos
- Evaluar si los wireframes representan adecuadamente las salidas del motor
- Verificar consistencia entre las representaciones documentadas y los renderizadores implementados
- Proponer mejoras en la ergonomía de la documentación (diagramas de flujo, snippets de código)

**Entregable esperado:** Evaluación de usabilidad de la documentación, propuestas de diagramas/ejemplos adicionales.

---

## AG-04 — Ingeniero de Prompts / AI Specialist

**Archivos a revisar:**
- `docs/04_prompts_ai/prompt-clasificacion-documento_v1.0.md`

**Criterios de refinamiento:**
- Evaluar la claridad y especificidad del system prompt
- Verificar que los guardrails son suficientes para evitar alucinaciones
- Validar que los ejemplos cubren los casos edge del clasificador
- Evaluar si el formato de salida JSON es parseable y completo
- Proponer métricas de evaluación del prompt (accuracy, recall)
- Identificar si faltan prompts para otros aspectos del motor (validación DSL, generación de templates, debugging)
- Evaluar versionado del prompt y estrategia de iteración

**Entregable esperado:** Evaluación del prompt con scoring, propuestas de mejora, sugerencia de prompts adicionales.

---

## AG-05 — Arquitecto de Software Senior

**Archivos a revisar:**
- `docs/05_arquitectura_tecnica/arquitectura-solucion_v1.0.md`
- `docs/05_arquitectura_tecnica/decisiones-arquitectura_v1.0.md`
- `docs/05_arquitectura_tecnica/contratos-del-motor_v1.0.md`
- `docs/05_arquitectura_tecnica/extensibilidad-motor_v1.0.md`
- `docs/05_arquitectura_tecnica/flujo-ejecucion-motor_v1.0.md`
- `docs/05_arquitectura_tecnica/guia-uso-libreria_v1.0.md`
- `docs/05_arquitectura_tecnica/modelo-datos-logico_v1.0.md`
- `docs/05_arquitectura_tecnica/modelo-logico-de-ejecucion_v1.0.md`
- `docs/05_arquitectura_tecnica/README.md`

**Criterios de refinamiento:**
- Verificar que los ADRs siguen un formato consistente (Contexto, Decisión, Consecuencias, Alternativas)
- Evaluar si los contratos/interfaces están completos (todas las interfaces públicas documentadas)
- Validar que el flujo de ejecución es consistente con los casos de uso
- Verificar que el modelo de extensibilidad documenta puntos de extensión concretos con ejemplos
- Evaluar si faltan diagramas (C4 Context, Container, Component)
- Validar que el modelo de datos lógico es coherente con el modelo conceptual en `02_especificacion_funcional/`
- Identificar decisiones arquitectónicas implícitas no documentadas como ADR
- Evaluar acoplamiento entre componentes documentados

**Entregable esperado:** Evaluación de completitud arquitectónica, ADRs faltantes, propuestas de diagramas C4.

---

## AG-06 — Scrum Master / Agile Coach

**Archivos a revisar:**
- `docs/06_backlog-tecnico/backlog-tecnico_v1.0.md`
- `docs/06_backlog-tecnico/product-backlog_v1.0.md`
- `docs/06_backlog-tecnico/definition-of-ready_v1.0.md`

**Criterios de refinamiento:**
- Verificar que el Product Backlog tiene user stories en formato estándar (Como/Quiero/Para)
- Validar la priorización MoSCoW: ¿la clasificación refleja el valor de negocio real?
- Evaluar si cada user story traza a un CU y a tareas técnicas (BT-XXX)
- Verificar que el backlog técnico tiene criterios de aceptación verificables
- Validar que el DoR es aplicable y no excesivamente burocrático
- Evaluar si las épicas están correctamente delimitadas (ni muy grandes ni muy pequeñas)
- Verificar coherencia de estimaciones entre Product Backlog y sprint plans
- Identificar user stories faltantes que se infieren de los CU pero no están en el backlog

**Entregable esperado:** Análisis de salud del backlog, trazabilidad US→CU→BT, recomendaciones de refinamiento.

---

## AG-07 — Scrum Master / Gestión de Proyectos Ágiles

**Archivos a revisar:**
- `docs/07_plan-sprint/plan-iteracion_sprint-01_v1.0.md` hasta `sprint-08_v1.0.md`
- `docs/07_plan-sprint/template-sprint-review_v1.0.md`
- `docs/07_plan-sprint/template-sprint-retrospectiva_v1.0.md`
- `docs/07_plan-sprint/velocidad-equipo_v1.0.md`

**Criterios de refinamiento:**
- Verificar que cada sprint plan tiene un Sprint Goal formal de una sola frase orientada a valor
- Evaluar si los sprints tienen scope creep (demasiados puntos vs capacidad)
- Validar que los criterios de terminado referencian al DoD canónico (no los redefinen)
- Verificar consistencia de formato entre los 8 sprint plans
- Evaluar si los riesgos identificados tienen plan de mitigación
- Validar que las historias comprometidas suman coherentemente con los puntos reportados
- Verificar que los templates de Review y Retro son prácticos y completos
- Evaluar si la tabla de velocidad tiene instrucciones claras de actualización
- Proponer formato de burndown compatible con las herramientas del equipo

**Entregable esperado:** Análisis de consistencia inter-sprint, propuestas de mejora en planificación, checklist de ceremonias.

---

## AG-08 — Ingeniero QA / SDET Senior

**Archivos a revisar:**
- `docs/08_calidad_y_pruebas/estrategia-calidad-motor_v1.0.md`
- `docs/08_calidad_y_pruebas/estrategia-testing-motor_v1.0.md`
- `docs/08_calidad_y_pruebas/casos-prueba-referenciales_v1.0.md`
- `docs/08_calidad_y_pruebas/criterios-validacion-motor_v1.0.md`
- `docs/08_calidad_y_pruebas/guia-testing-extensibilidad_v1.0.md`
- `docs/08_calidad_y_pruebas/matriz-cobertura-pruebas_v1.0.md`
- `docs/08_calidad_y_pruebas/plan-pruebas_v1.0.md`
- `docs/08_calidad_y_pruebas/definition-of-done_v1.0.md`

**Criterios de refinamiento:**
- Verificar que la estrategia de testing cubre todos los niveles (unit, integration, snapshot, regression, e2e)
- Evaluar si los casos de prueba referenciales cubren los escenarios críticos de cada componente
- Validar que la matriz de cobertura tiene datos actualizados (no todo "Pendiente")
- Verificar que el DoD es alcanzable y medible (criterios objetivos, no subjetivos)
- Evaluar si la guía de testing de extensibilidad cubre cómo testear nuevos renderizadores y plugins
- Validar que el plan de pruebas referencia correctamente a los demás documentos de calidad
- Verificar que los quality gates del CI/CD están alineados con los criterios de calidad documentados
- Proponer test cases faltantes para edge cases del DSL (templates vacíos, datos nulos, nodos anidados profundos)

**Entregable esperado:** Gap analysis de cobertura de pruebas, test cases faltantes, propuestas de quality gates.

---

## AG-09 — Ingeniero DevOps Senior

**Archivos a revisar:**
- `docs/09_devops/pipeline-ci-cd_v1.0.md`
- `docs/09_devops/estrategia-versionado_v1.0.md`
- `docs/09_devops/entornos-deploy_v1.0.md`
- `docs/09_devops/guia-publicacion-nuget_v1.0.md`

**Criterios de refinamiento:**
- Verificar que el pipeline CI/CD cubre: build, test, quality analysis, packaging, publishing
- Evaluar si los triggers están correctamente definidos (PR, push, tags)
- Validar que los quality gates son coherentes con `08_calidad_y_pruebas/`
- Verificar que la estrategia de versionado SemVer tiene reglas claras para breaking changes
- Evaluar si los entornos de deploy están correctamente segmentados (dev, staging, prod)
- Validar que la guía de publicación NuGet es reproducible paso a paso
- Verificar manejo de secretos y variables de entorno
- Evaluar la estrategia de rollback: ¿es práctica y documentada?
- Verificar si hay monitoring/alerting post-deploy documentado
- Evaluar si los workflow files referenciados (ci.yml, cd-*.yml) están documentados a nivel de steps

**Entregable esperado:** Evaluación de madurez DevOps, gaps de automatización, propuestas de hardening.

---

## AG-10 — Technical Writer / Developer Advocate

**Archivos a revisar:**
- `docs/10_developer_guide/README.md`
- `docs/10_developer_guide/conceptos-fundamentales.md`
- `docs/10_developer_guide/formato-dsl-templates.md`
- `docs/10_developer_guide/formato-perfiles-impresora.md`
- `docs/10_developer_guide/guia-integracion-maui.md`
- `docs/10_developer_guide/integracion-api-rest.md`
- `docs/10_developer_guide/perfiles-impresoras-reales.md`
- `docs/10_developer_guide/soporte-imagenes-termicas.md`
- `docs/10_developer_guide/ejemplos-templates/*.json`

**Criterios de refinamiento:**
- Evaluar si un desarrollador nuevo puede integrar la librería siguiendo solo esta guía (test del "developer nuevo")
- Verificar que todos los snippets de código compilan y son copy-pasteable
- Validar que los prerequisitos están completos y actualizados (.NET version, paquetes NuGet)
- Evaluar si hay progresión lógica de complejidad (conceptos → formato → integración → avanzado)
- Verificar que los templates JSON de ejemplo son válidos y representativos
- Evaluar si falta una sección de troubleshooting / errores comunes
- Verificar que los archivos tienen sufijo de versión consistente
- Validar que hay cross-links a `05_arquitectura_tecnica/` para quien quiera profundizar
- Evaluar si la integración API REST está documentada con endpoints, payloads y responses

**Entregable esperado:** Evaluación de usabilidad de la guía, gaps de onboarding, propuestas de mejora didáctica.

---

## AG-11 — Developer Advocate / Ingeniero de Aplicación

**Archivos a revisar:**
- `docs/11_examples/README.md`
- `docs/11_examples/ejemplo-01-simple.md`
- `docs/11_examples/ejemplo-02-multa.md`
- `docs/11_examples/ejemplo-03-multaapp-nuget.md`
- `docs/11_examples/imagenes_logos/*`

**Criterios de refinamiento:**
- Verificar que cada ejemplo es reproducible (pasos claros, prerequisitos, resultado esperado)
- Evaluar la progresión de complejidad: básico → avanzado → producción
- Validar que el README tiene una tabla comparativa clara de features por ejemplo
- Verificar que las imágenes/logos base64 son válidas y usadas en los templates
- Evaluar si los ejemplos cubren todos los renderizadores (ESC/POS, texto, UI, PDF)
- Verificar cross-links a `10_developer_guide/` para contexto conceptual
- Evaluar si falta un ejemplo de extensibilidad (crear un renderizador custom)
- Proponer mejoras en la presentación (screenshots de output, diffs entre ejemplos)

**Entregable esperado:** Evaluación de didáctica, gaps de cobertura de ejemplos, propuestas de nuevos ejemplos.

---

# 3. Prompt Maestro para Orquestación de Sub-Agentes

El siguiente prompt está diseñado para ser ejecutado por un agente orquestador que lance sub-agentes especializados en paralelo. Cada sub-agente recibe su contexto, archivos y criterios específicos.

---

## Prompt de Orquestación

```text
Sos un agente orquestador responsable de coordinar el refinamiento integral de la 
documentación del proyecto Motor DSL de Generación de Documentos.

La documentación está en la carpeta `docs/` y contiene 13 subcarpetas temáticas 
más un README.md raíz. Tu trabajo es lanzar 12 sub-agentes especializados en 
paralelo, cada uno enfocado en su área de competencia.

## Instrucciones generales para TODOS los sub-agentes:

1. **NO modificar ningún archivo**. Solo analizar y reportar.
2. Cada sub-agente debe producir un informe estructurado con:
   - **Hallazgos** (tabla: ID, Severidad [Alta/Media/Baja], Archivo, Descripción)
   - **Propuestas de mejora** (tabla: ID, Archivo, Cambio propuesto, Justificación)
   - **Evaluación general** (puntuación 1-10 de completitud, coherencia y calidad)
3. Idioma del reporte: Español
4. Formato de salida: Markdown
5. Verificar coherencia de nomenclatura de archivos (kebab-case, sufijo _v1.0.md)
6. Verificar que los links internos entre documentos son válidos
7. Verificar que la información no contradice otros documentos del proyecto

## Sub-agentes a lanzar:

### Sub-agente 1: Arquitecto de Soluciones Senior
- **Scope:** `docs/README.md`
- **Rol:** Revisás como un Arquitecto de Soluciones Senior con 15+ años de experiencia 
  en proyectos .NET enterprise. Tu foco es la coherencia integral del repositorio.
- **Tarea:** Verificar que el README.md es el punto de entrada efectivo al proyecto. 
  Validar todos los links, el árbol de carpetas, la narrativa técnica y la alineación 
  con los documentos internos. Evaluar si un desarrollador nuevo entiende el proyecto 
  en 5 minutos de lectura.

### Sub-agente 2: Product Manager
- **Scope:** `docs/00_contexto/` (todos los archivos)
- **Rol:** Revisás como un Product Manager con experiencia en productos de infraestructura 
  técnica. Tu foco es la claridad estratégica.
- **Tarea:** Evaluar visión de producto, alcance, roadmap y acuerdo de equipo. Verificar 
  que las métricas de éxito son SMART, que el roadmap traza a épicas/sprints, y que 
  el alcance tiene exclusiones justificadas.

### Sub-agente 3: Analista de Negocio Senior
- **Scope:** `docs/01_necesidades_negocio/` (todos los archivos incluyendo subcarpeta)
- **Rol:** Revisás como un Analista de Negocio Senior certificado CBAP. Tu foco es la 
  completitud y trazabilidad de las necesidades.
- **Tarea:** Verificar que cada NB tiene problema, impacto y criterios de éxito. Validar 
  trazabilidad NB→CU. Identificar inconsistencias en nomenclatura de archivos. 
  Evaluar si los stakeholders están identificados concretamente.

### Sub-agente 4: Analista Funcional / Ingeniero de Requisitos
- **Scope:** `docs/02_especificacion_funcional/` (todos los archivos y subcarpetas)
- **Rol:** Revisás como un Ingeniero de Requisitos con certificación IREB. Tu foco es 
  la completitud y consistencia de la especificación funcional.
- **Tarea:** Verificar estructura uniforme de los 42 CU. Validar que criterios de 
  aceptación usan Given/When/Then. Confirmar que CU deprecados están marcados. 
  Verificar matriz de trazabilidad completa. Evaluar definición DSL contra nodos en CU. 
  Verificar que no hay documentos de otros proyectos.

### Sub-agente 5: Especialista en Developer Experience
- **Scope:** `docs/03_ux-ui/` (todos los archivos)
- **Rol:** Revisás como un Especialista en DX con experiencia en diseño de APIs y SDKs. 
  Tu foco es la usabilidad del motor desde la perspectiva del desarrollador consumidor.
- **Tarea:** Evaluar claridad de flujos de uso, diferenciación de modos (Printing, 
  Preview, Debug), documentación de parámetros con tipos y defaults, y calidad 
  de wireframes.

### Sub-agente 6: Ingeniero de Prompts
- **Scope:** `docs/04_prompts_ai/` (todos los archivos)
- **Rol:** Revisás como un Ingeniero de Prompts con experiencia en producción de sistemas 
  LLM. Tu foco es la calidad y robustez del prompt.
- **Tarea:** Evaluar claridad del system prompt, guardrails, cobertura de ejemplos, 
  parseabilidad del output JSON. Proponer métricas de evaluación y prompts adicionales 
  para otros aspectos del motor.

### Sub-agente 7: Arquitecto de Software Senior
- **Scope:** `docs/05_arquitectura_tecnica/` (todos los archivos)
- **Rol:** Revisás como un Arquitecto de Software Senior especializado en .NET y 
  arquitecturas modulares. Tu foco es la solidez técnica de las decisiones.
- **Tarea:** Verificar formato de ADRs, completitud de contratos, consistencia del flujo 
  de ejecución con CU, puntos de extensión documentados con ejemplos, presencia de 
  diagramas C4, coherencia del modelo de datos con el modelo conceptual.

### Sub-agente 8: Scrum Master / Agile Coach
- **Scope:** `docs/06_backlog-tecnico/` (todos los archivos)
- **Rol:** Revisás como un Scrum Master certificado PSM-II y Agile Coach. Tu foco es 
  la salud del backlog y la práctica ágil.
- **Tarea:** Verificar formato de user stories, validez de priorización MoSCoW, 
  trazabilidad US→CU→BT, aplicabilidad del DoR, delimitación de épicas, coherencia 
  de estimaciones con sprint plans.

### Sub-agente 9: Scrum Master / Gestión de Proyectos
- **Scope:** `docs/07_plan-sprint/` (todos los archivos)
- **Rol:** Revisás como un Scrum Master con experiencia en gestión de proyectos ágiles 
  de 6+ sprints. Tu foco es la consistencia y utilidad de la planificación.
- **Tarea:** Verificar Sprint Goals formales, consistencia de formato entre 8 sprints, 
  que el DoD no se redefine en cada sprint, coherencia de puntos comprometidos, 
  practicidad de templates de Review/Retro, instrucciones de velocidad.

### Sub-agente 10: Ingeniero QA / SDET Senior
- **Scope:** `docs/08_calidad_y_pruebas/` (todos los archivos)
- **Rol:** Revisás como un SDET Senior con experiencia en test automation frameworks 
  para .NET. Tu foco es la cobertura y ejecutabilidad de la estrategia de calidad.
- **Tarea:** Verificar cobertura de niveles de testing, actualización de matriz de 
  cobertura, alcanzabilidad del DoD, completitud de casos de prueba, alineación de 
  quality gates con CI/CD, test cases para edge cases del DSL.

### Sub-agente 11: Ingeniero DevOps Senior
- **Scope:** `docs/09_devops/` (todos los archivos)
- **Rol:** Revisás como un Ingeniero DevOps Senior con experiencia en CI/CD para 
  proyectos .NET y publicación NuGet. Tu foco es la automatización y confiabilidad.
- **Tarea:** Verificar completitud del pipeline, triggers, quality gates coherentes con 
  QA docs, versionado SemVer claro, reproducibilidad de publicación NuGet, manejo 
  de secretos, estrategia de rollback, monitoring post-deploy.

### Sub-agente 12: Technical Writer / Developer Advocate
- **Scope:** `docs/10_developer_guide/` y `docs/11_examples/` (todos los archivos)
- **Rol:** Revisás como un Technical Writer senior con experiencia en documentación 
  de SDKs .NET. Tu foco es la experiencia del desarrollador consumidor.
- **Tarea:** Evaluar si la guía permite integración autónoma, verificar que snippets 
  compilan, validar progresión de complejidad, evaluar templates JSON, verificar 
  reproducibilidad de ejemplos, identificar gaps de troubleshooting, evaluar 
  cross-links entre guía y ejemplos.

## Consolidación final:

Después de que todos los sub-agentes reporten, consolidar un informe único con:

1. **Dashboard de salud documental** — Puntuación por área (tabla con AG-ID, Área, 
   Completitud, Coherencia, Calidad, Puntuación global)
2. **Top 10 hallazgos críticos** — Los más impactantes ordenados por severidad
3. **Plan de acción priorizado** — Acciones concretas ordenadas por impacto/esfuerzo
4. **Mapa de dependencias entre hallazgos** — Qué correcciones deben hacerse primero
```

---

# 4. Instrucciones de Uso

## Ejecución del refinamiento

1. **Copiar el prompt de la Sección 3** y ejecutarlo en un agente con capacidad de lanzar sub-agentes (GitHub Copilot CLI, Claude, GPT con tools, etc.)

2. **Asegurar acceso a la carpeta `docs/`** — Los sub-agentes necesitan leer todos los archivos markdown del proyecto.

3. **Revisar los informes** — Cada sub-agente genera un informe independiente. El orquestador los consolida.

4. **Priorizar acciones** — Usar el plan de acción priorizado del informe consolidado para ejecutar las correcciones en orden.

5. **Iterar** — Después de aplicar correcciones, re-ejecutar el prompt para verificar que los hallazgos fueron resueltos.

## Ejecución parcial

Si se desea refinar solo una sección, se puede extraer el prompt del sub-agente específico de la Sección 2 y ejecutarlo individualmente.

**Ejemplo:** Para refinar solo DevOps:

```text
Actuá como un Ingeniero DevOps Senior con experiencia en CI/CD para proyectos 
.NET y publicación NuGet.

Revisá todos los archivos en docs/09_devops/ y generá un informe con:
- Hallazgos (tabla: ID, Severidad, Archivo, Descripción)
- Propuestas de mejora (tabla: ID, Archivo, Cambio, Justificación)
- Evaluación general (1-10)

[Insertar criterios específicos del AG-09 de la Sección 2]
```

---

# 5. Control de Cambios

| Versión | Fecha      | Descripción                                                    |
| ------- | ---------- | -------------------------------------------------------------- |
| 1.0     | 2026-04-03 | Versión inicial — 12 agentes, prompt maestro, instrucciones    |

---

**Fin del documento**
