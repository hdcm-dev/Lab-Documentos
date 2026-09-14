# Metodología de Prompting Colaborativo Claude ↔ Copilot para SDD

**Documento:** metodologia-sdd-prompting_v1.0.md  
**Versión:** 1.0  
**Fecha:** 2026-04-06  
**Autor:** Equipo de Desarrollo — UTN FRP TUP Aplicada 2025  
**Estado:** Activo  
**Proyecto de referencia:** Motor DSL de Generación de Documentos

---

## 1. Propósito

Este documento describe una metodología reproducible de **prompting colaborativo** entre dos agentes de IA — **Claude** (Anthropic) y **GitHub Copilot** — para construir y mantener documentación de software bajo el enfoque **Specification-Driven Development (SDD)**.

La metodología fue desarrollada y validada durante la construcción del Motor DSL de Generación de Documentos, un proyecto .NET/MAUI con 12 secciones documentales, 32 casos de uso, 185+ tests automatizados y publicación NuGet, donde toda la documentación fue generada, refinada y verificada mediante esta cadena de prompting.

La meta es establecer un **patrón de trabajo reproducible** que cualquier equipo pueda aplicar en otros proyectos de software, independientemente del stack tecnológico.

---

## 2. Fundamento: ¿Por qué dos agentes?

La metodología se basa en una separación deliberada de responsabilidades entre dos agentes con capacidades complementarias:

**Claude** actúa como **arquitecto de prompts y estratega documental**. Opera en un proyecto persistente con knowledge base, donde acumula contexto del proyecto a lo largo de múltiples sesiones. Su rol es pensar, decidir, validar y generar los prompts que ejecutará Copilot. Claude no modifica archivos del repositorio directamente — diseña las instrucciones.

**Copilot** actúa como **ejecutor con acceso al filesystem**. Opera dentro del IDE (VS Code, terminal) con acceso directo al código fuente, los archivos markdown y la capacidad de crear, editar y compilar. Copilot ejecuta los prompts que Claude diseña, pero no decide qué hacer — sigue instrucciones precisas.

Esta separación evita dos problemas comunes del desarrollo asistido por IA: que un solo agente tome decisiones de arquitectura sin contexto suficiente (Copilot trabajando solo), o que el agente estratégico no pueda verificar el resultado contra el código real (Claude trabajando solo).

---

## 3. Roles y Especialidades de los Agentes

### 3.1 Claude como Agente Estratégico

Claude opera en modo **proyecto** dentro de claude.ai, lo que le permite mantener un knowledge base persistente con los documentos del repositorio. Sus funciones son:

**Contextualización del proyecto.** Al inicio, se cargan los documentos markdown de referencia (`/docs/`) como knowledge del proyecto. Claude los analiza, identifica la estructura, detecta gaps y propone mejoras. El equipo humano aporta la visión, las restricciones de negocio y las decisiones de diseño durante esta fase conversacional.

**Generación de prompts especializados.** Claude produce prompts con estructura precisa para Copilot, incluyendo: qué archivos leer antes de actuar, qué crear y en qué orden, qué restricciones respetar, qué verificar al terminar, y qué NO hacer. Cada prompt tiene un perfil profesional implícito (arquitecto, QA, DevOps, etc.) que determina el tono y los criterios de evaluación.

**Evaluación de resultados.** Cuando Copilot genera documentación o código, el equipo comparte la salida con Claude para evaluación. Claude verifica coherencia contra la documentación existente, detecta inconsistencias y genera el siguiente prompt correctivo o de avance.

**Orquestación de sub-agentes.** Para tareas de refinamiento integral, Claude genera un prompt maestro que define 12 sub-agentes especializados, cada uno con un perfil profesional concreto, un scope de archivos y criterios de evaluación específicos.

### 3.2 Copilot como Agente Ejecutor

Copilot opera dentro del entorno de desarrollo (VS Code, terminal, GitHub Copilot Chat) con acceso al filesystem del proyecto. Sus funciones son:

**Lectura dirigida de documentación.** Cada prompt de Claude le indica exactamente qué archivos leer y en qué orden. Copilot no decide qué es relevante — sigue la secuencia prescrita. Esto evita que ignore documentos clave o que "invente" contexto.

**Generación de artefactos.** Crea archivos markdown, código C#, configuraciones YAML, templates JSON y cualquier artefacto que el prompt especifique. Siempre uno a la vez, esperando confirmación entre cada uno.

**Verificación técnica.** Ejecuta comandos de validación (`dotnet test`, `dotnet build`, `Get-Content`) para confirmar que lo generado compila, pasa tests y es sintácticamente correcto.

**Reporte de estado.** Comparte la salida de cada paso con el equipo humano, que a su vez la comparte con Claude para evaluación.

### 3.3 El Equipo Humano como Orquestador

El equipo humano no es pasivo — es el **puente decisional** entre ambos agentes:

- Aporta la visión de negocio, las restricciones reales y las decisiones de arquitectura a Claude
- Copia los prompts generados por Claude y los ejecuta en Copilot
- Evalúa las salidas de Copilot con criterio profesional antes de compartirlas con Claude
- Aprueba, rechaza o modifica los artefactos generados
- Decide cuándo avanzar al siguiente paso de la cadena

---

## 4. El Ciclo de 6 Fases

La metodología se estructura en un ciclo de 6 fases que se repiten para cada nueva funcionalidad, sprint o proyecto:

### Fase 1 — Contextualización en Claude (Proyecto)

**Qué se hace:** Se crea un proyecto en claude.ai y se cargan los documentos markdown de referencia como knowledge base. Estos documentos pueden ser de un proyecto existente (como plantilla) o documentación inicial creada manualmente.

**Prompt típico del humano a Claude:**

```
Tengo este proyecto en [URL del repositorio]. 
La documentación está en /docs/ con esta estructura:
[pegar árbol de carpetas]

Quiero usar esta estructura como plantilla para un nuevo proyecto 
que consiste en [descripción del nuevo proyecto].

Revisá la estructura documental y decime:
1. Qué secciones aplican directamente
2. Qué secciones necesitan adaptación
3. Qué secciones faltan para este nuevo contexto
```

**Lo que Claude produce:** Un análisis de la estructura documental, gaps identificados y una propuesta de adaptación. Claude NO genera documentación todavía — primero entiende.

**Patrón de caracterización del prompt:**

| Atributo | Valor |
|---|---|
| Dirección | Humano → Claude |
| Tipo | Contextualización / Análisis |
| Especialidad implícita | Arquitecto de Soluciones (AG-ROOT) |
| Input | Documentos de referencia + descripción del nuevo proyecto |
| Output esperado | Análisis estructural, gaps, propuesta |
| Restricción clave | NO generar artefactos, solo analizar |

---

### Fase 2 — Definición de la Idea (Conversacional)

**Qué se hace:** En un chat dentro del proyecto Claude, el equipo humano plantea la idea del nuevo proyecto. Se establece un diálogo iterativo donde Claude cuestiona, propone alternativas, evalúa decisiones de arquitectura y el humano aporta ejemplos prácticos, capturas de pantalla y restricciones reales.

**Prompts típicos del humano a Claude:**

```
La librería NO debería ser responsable de obtener el perfil 
de la impresora desde una API. Eso es responsabilidad del cliente.
La generación de PDF tampoco debería ser parte de la librería 
si genera dependencias con terceros.
¿Cómo promptearías estos casos de uso para que Copilot tome 
en cuenta estas consideraciones?
```

```
Tengo el self-test completo de la impresora 58HB6. 
[pegar datos del self-test]
¿Podrías mejorar el modelo de datos en la documentación 
e incorporar el codepage soportado?
```

**Lo que Claude produce:** Decisiones de diseño documentadas, análisis de trade-offs, y el contexto necesario para la Fase 3. Claude internaliza las restricciones del proyecto.

**Patrón de caracterización del prompt:**

| Atributo | Valor |
|---|---|
| Dirección | Humano ↔ Claude (bidireccional) |
| Tipo | Deliberación / Decisión de arquitectura |
| Especialidad implícita | Variable (Product Manager, Arquitecto, Analista según el tema) |
| Input | Ideas, restricciones, ejemplos prácticos, datos reales |
| Output esperado | Decisiones validadas, trade-offs explícitos |
| Restricción clave | NO generar prompts para Copilot todavía |

---

### Fase 3 — Generación de Prompt Especializado (Claude → Copilot)

**Qué se hace:** Con la idea definida y las decisiones tomadas, se le pide a Claude que genere un prompt especializado para que Copilot reconstruya la documentación del nuevo proyecto, usando la estructura de `/docs/` como plantilla.

**Prompt típico del humano a Claude:**

```
Establecida la idea, generá un prompt especializado para que 
Copilot reconstruya la documentación en el nuevo proyecto local, 
basándose en la estructura y nomenclatura de los documentos 
markdown existentes, tomando estos documentos como ejemplo 
bajo el planteo de la nueva idea.
```

**Lo que Claude produce:** Un prompt estructurado y detallado para Copilot. Ejemplo real del proyecto:

```
Vamos a crear el Developer Guide completo de la librería.
Esta documentación está orientada al desarrollador que consume 
la librería, no al que la construye.

IMPORTANTE: NO modifiques ningún archivo existente.
Creá SOLO archivos nuevos en docs/10_developer_guide/

Leé primero estos archivos para entender el estado actual:
- docs/05_arquitectura_tecnica/guia-uso-libreria_v1.0.md
- docs/05_arquitectura_tecnica/contratos-del-motor_v1.0.md
- src/MotorDsl.Core/Models/DocumentNode.cs
- src/MotorDsl.Core/Models/DeviceProfile.cs
[...]

Creá estos archivos en orden, UNO A LA VEZ esperando 
confirmación entre cada uno:

ARCHIVO 1 — docs/10_developer_guide/README.md
[especificación detallada del contenido]

ARCHIVO 2 — docs/10_developer_guide/conceptos-fundamentales.md
[especificación detallada del contenido]
[...]
```

**Patrón de caracterización del prompt generado:**

| Atributo | Valor |
|---|---|
| Dirección | Claude genera → Humano ejecuta en Copilot |
| Tipo | Prompt de generación de artefactos |
| Estructura | Lectura previa → Restricciones → Creación secuencial → Verificación |
| Especialidad del prompt | Mapea al agente SDD correspondiente (AG-00 a AG-11) |
| Guardrails | NO modificar archivos existentes, uno a la vez, esperar confirmación |
| Verificación | `Get-Content` después de cada archivo |

---

### Fase 4 — Ejecución y Revisión Humana

**Qué se hace:** El equipo humano copia el prompt de Claude y lo ejecuta en Copilot. Copilot genera los artefactos. El equipo los revisa, sugiere cambios y los aplica directamente sobre los archivos generados.

**Flujo:**

```
Humano copia prompt de Claude
  → Lo pega en Copilot Chat
    → Copilot genera ARCHIVO 1
      → Humano revisa con Get-Content
        → Humano aprueba o corrige
          → Copilot genera ARCHIVO 2
            → [repite hasta completar]
```

**Criterios de revisión humana:**

- ¿El contenido es coherente con las decisiones tomadas en Fase 2?
- ¿La nomenclatura sigue la convención del proyecto (kebab-case, sufijo `_v1.0.md`)?
- ¿Los datos técnicos son correctos (versiones, nombres de interfaces, paths)?
- ¿Los links internos apuntan a archivos que existen?
- ¿El tono y nivel de detalle son apropiados para la audiencia?

---

### Fase 5 — Prompt de Coherencia (Claude → Copilot)

**Qué se hace:** Después de que Copilot genera la documentación y el equipo la revisa, se le pide a Claude que genere un prompt de refinamiento para que Copilot evalúe y corrija la coherencia de todos los documentos.

**Prompt típico del humano a Claude:**

```
Copilot ya generó toda la documentación. 
Generá un prompt especializado para que Copilot evalúe y corrija 
la coherencia de todos los datos en los diferentes documentos.
```

**Lo que Claude produce:** Un prompt maestro con sub-agentes especializados. Ejemplo real:

```
Sos un agente orquestador responsable de coordinar el refinamiento 
integral de la documentación SDD del proyecto.

La documentación está en la carpeta docs/ organizada en 12 secciones 
temáticas (00 a 11) más un README.md raíz.

Tu trabajo: lanzar 12 sub-agentes especializados en paralelo, 
cada uno con un perfil profesional concreto, y consolidar sus 
hallazgos en un informe único.

Reglas generales para TODOS los sub-agentes:
1. NO modificar ningún archivo. Solo analizar y reportar.
2. Idioma: Español.
3. Formato de salida: Markdown estructurado.
[...]

SA-01 — Arquitecto de Soluciones Senior
Scope: docs/README.md
Misión: Verificar que el README funciona como punto de entrada 
efectivo al SDD.
[criterios específicos]

SA-02 — Product Manager / Analista de Negocio
Scope: docs/00_contexto/*
Misión: Evaluar la solidez estratégica del contexto del producto.
[criterios específicos]
[... hasta SA-12]

Consolidación final:
1. Dashboard de Salud del SDD (tabla de puntuaciones)
2. Mapa de Trazabilidad SDD (eslabones rotos)
3. Top 10 Hallazgos Críticos
4. Plan de Acción Priorizado
5. Dependencias entre Hallazgos
```

**Patrón de caracterización:**

| Atributo | Valor |
|---|---|
| Dirección | Claude genera → Humano ejecuta en Copilot |
| Tipo | Prompt de evaluación / refinamiento |
| Estructura | Orquestador + N sub-agentes + consolidación |
| Especialidad | Cada sub-agente tiene un perfil profesional concreto |
| Restricción clave | Solo analizar, NO modificar |
| Output | Informe con hallazgos, propuestas y scoring |

---

### Fase 6 — Ejecución del Refinamiento y Retroalimentación

**Qué se hace:** El humano ejecuta el prompt de coherencia en Copilot. La salida se comparte con Claude para que genere el siguiente prompt (correcciones, nuevo sprint, nueva funcionalidad). El ciclo se repite.

**Flujo de retroalimentación:**

```
Copilot ejecuta prompt de coherencia
  → Genera informe de hallazgos
    → Humano comparte informe con Claude
      → Claude evalúa y genera:
        a) Prompt de corrección (si hay errores)
        b) Prompt del siguiente sprint (si todo está bien)
        c) Prompt de nueva funcionalidad
          → Vuelve a Fase 3 o Fase 4
```

---

## 5. Patrones de Prompt Recurrentes

A lo largo del proyecto se identificaron patrones de prompt que se repiten en diferentes contextos. Cada patrón tiene una estructura reconocible y un propósito definido.

### 5.1 Patrón "Lectura Antes de Acción"

Todo prompt para Copilot comienza con una lista explícita de archivos a leer antes de escribir cualquier cosa. Este es el patrón más importante porque evita que Copilot invente contexto.

```
Antes de escribir código, leé en orden:
1. docs/07_plan-sprint/plan-iteracion_sprint-XX_v1.0.md
2. docs/05_arquitectura_tecnica/contratos-del-motor_v1.0.md
3. docs/02_especificacion_funcional/casos-de-uso/CU-XX-*.md

Respondeme SOLO con:
- [lista de cosas que debe confirmar haber entendido]

NO escribas código todavía. Solo confirmame que entendiste.
```

### 5.2 Patrón "Uno a la Vez con Confirmación"

Para generación de múltiples artefactos, se exige creación secuencial con confirmación humana entre cada uno:

```
Creá estos archivos en orden, UNO A LA VEZ esperando 
confirmación entre cada uno:

ARCHIVO 1 — [path y especificación]
ARCHIVO 2 — [path y especificación]
[...]

Al terminar cada archivo, mostrá con:
Get-Content [path del archivo]
```

### 5.3 Patrón "Diagnóstico Antes de Plan"

Antes de planificar un sprint o una nueva funcionalidad, se ejecuta un diagnóstico del estado actual:

```
Antes de planificar Sprint XX, leé estos documentos en orden:
[lista de archivos]

Luego respondeme SOLO esto, sin código:
1. Qué ítems del backlog están completados
2. Qué ítems quedan pendientes ordenados por prioridad
3. Tu recomendación de qué entra en el sprint y por qué
4. Qué CUs se habilitarían al terminar

No planifiques todavía. Solo el diagnóstico.
```

### 5.4 Patrón "Corrección de Desvío"

Cuando Copilot pierde contexto (por compactación de conversación o error), se usa un prompt de reorientación:

```
Los archivos v2.0 de los CUs ya existen y ya fueron commiteados.
No los toques.

Verificá con:
git log --oneline -3

Ahora leé SOLO estos documentos:
[lista de archivos relevantes]

Respondeme SOLO con:
[lo que necesita confirmar]

NO escribas código. Esperá mi confirmación.
```

### 5.5 Patrón "Decisión de Arquitectura Embebida"

Cuando hay decisiones de diseño que Copilot debe respetar, se incluyen explícitamente en el prompt:

```
DECISIÓN 1 — CU-21, CU-24, CU-25, CU-26:
La librería NO es responsable de obtener plantillas, datos ni 
perfiles desde APIs externas. Eso es responsabilidad del cliente.

La librería SÍ debe:
- Definir interfaces que el cliente puede implementar
- Aceptar esas interfaces opcionalmente

DECISIÓN 2 — CU-09 (PDF):
Solo implementar si es posible sin dependencias de terceros.
```

### 5.6 Patrón "Verificación y Deploy"

Todo prompt de implementación termina con comandos de verificación:

```
Al terminar:
dotnet test PrintThermalDriver.sln --verbosity normal | Select-String "total|correcto|errores"
dotnet build -t:Run -f net10.0-android 2>&1 | Select-Object -Last 5
```

### 5.7 Patrón "Coherencia pre-cambio"

Antes de ejecutar cambios que afectan múltiples documentos, se verifica coherencia primero:

```
Antes de mandar estos prompts, revisaste que el apartado devops 
sea consistente con estos que estamos por hacer?
```

Claude entonces verifica contra el knowledge base y genera un prompt de actualización previa si detecta inconsistencias.

---

## 6. Tabla de Especialidades y Disciplinas (Agentes SDD)

Cada sección de la documentación tiene un agente responsable con un perfil profesional concreto. Este mapeo es fundamental porque los prompts generados por Claude heredan el perfil del agente correspondiente.

| ID | Rol / Especialidad | Disciplina | Sección que produce | Artefactos clave |
|---|---|---|---|---|
| AG-ROOT | Arquitecto de Soluciones Senior | Gestión integral / Gobernanza | `docs/README.md` | Árbol documental, coherencia cross-documental |
| AG-00 | Product Manager | Estrategia de producto | `00_contexto/` | Visión, alcance, roadmap, acuerdo de equipo |
| AG-01 | Analista de Negocio Senior | Ingeniería de requisitos de negocio | `01_necesidades_negocio/` | Necesidades de negocio (NB-XX), criterios de éxito |
| AG-02 | Analista Funcional / Ing. de Requisitos | Especificación funcional | `02_especificacion_funcional/` | Casos de uso (CU-XX), reglas de negocio (RN-XX), definición DSL |
| AG-03 | Especialista en Developer Experience (DX) | Diseño de experiencia de API | `03_ux-ui/` | Modos de uso, wireframes, representaciones de salida |
| AG-04 | Ingeniero de Prompts / Especialista IA | Ingeniería de prompts / LLMs | `04_prompts_ai/` | Prompts estructurados, esquemas JSON, guardrails |
| AG-05 | Arquitecto de Software Senior | Arquitectura de software | `05_arquitectura_tecnica/` | ADRs, contratos, pipeline, diagramas, extensibilidad |
| AG-06 | Scrum Master / Agile Coach (Backlog) | Gestión ágil de backlog | `06_backlog-tecnico/` | Product Backlog (US-XX), Backlog Técnico (BT-XX), DoR |
| AG-07 | Scrum Master / Gestión de Proyectos Ágiles | Planificación y seguimiento ágil | `07_plan-sprint/` | Sprint Plans (×8), review/retro templates, velocidad |
| AG-08 | Ingeniero QA / SDET Senior | Aseguramiento de calidad | `08_calidad_y_pruebas/` | Estrategia testing, DoD, matriz cobertura, quality gates |
| AG-09 | Ingeniero DevOps Senior | CI/CD e infraestructura | `09_devops/` | Pipeline CI/CD, SemVer, deploy, publicación NuGet |
| AG-10 | Technical Writer | Documentación técnica | `10_developer_guide/` | Guías de integración, referencia DSL, tutoriales |
| AG-11 | Developer Advocate / Ing. de Aplicación | Evangelización y ejemplos | `11_examples/` | 3 apps de referencia ejecutables, assets |

### Flujo de trazabilidad entre disciplinas

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

### Mapeo Agente → Documentación (Entradas y Salidas)

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
| `08_calidad_y_pruebas` | AG-08 | CU-XX + criterios aceptación de AG-02 | Quality gates → AG-09, DoD para todos |
| `09_devops` | AG-09 | DoD de AG-08, versionado de AG-00 | Pipeline automatizado build/test/publish |
| `10_developer_guide` | AG-10 | DSL spec (AG-02), contratos API (AG-05) | Documentación consumible para desarrolladores |
| `11_examples` | AG-11 | Guía dev (AG-10), arquitectura (AG-05) | Código de referencia, feedback → AG-10 |

---

## 7. Resumen por Sección de `/docs/`

| # | Sección | Propósito |
|---|---------|-----------|
| 00 | `00_contexto/` | Contexto estratégico. Visión del producto, alcance, compatibilidad de plataformas, roadmap y acuerdo de equipo. Define el *por qué* del proyecto. |
| 01 | `01_necesidades_negocio/` | Necesidades de negocio. Identifica 6 problemas (NB-01 a NB-06): desacoplamiento datos/diseño/dispositivo, estandarización, evolución de templates sin recompilar, soporte multiformato, versionado y reducción de costos. |
| 02 | `02_especificacion_funcional/` | Especificación funcional. Traduce las necesidades en casos de uso (CU-01 a CU-32), reglas de negocio (RN-XX), modelo de datos conceptual y la gramática del DSL (JSON). |
| 03 | `03_ux-ui/` | Experiencia de desarrollador (DX). Diseño de la experiencia de uso del motor: modos de impresión (ESC/POS), vista previa (MAUI) y debug (texto). Wireframes de documentos de salida. |
| 04 | `04_prompts_ai/` | Prompts para IA. Prompts estructurados para clasificación de documentos DSL mediante LLMs. Incluye esquemas JSON, few-shot examples y guardrails. |
| 05 | `05_arquitectura_tecnica/` | Arquitectura técnica. Pipeline de transformación (DSL → Parser → AST → Evaluador → Layout → Renderer), ADRs, contratos/interfaces, extensibilidad, modelo lógico de datos y ejecución. |
| 06 | `06_backlog-tecnico/` | Backlog del producto. 31 User Stories (US-01 a US-31) priorizadas con MoSCoW, backlog técnico (BT-XX), Definition of Ready. |
| 07 | `07_plan-sprint/` | Planificación de sprints. 8 planes de iteración (2 semanas c/u), templates de review/retrospectiva, tracking de velocidad y burndown. |
| 08 | `08_calidad_y_pruebas/` | Calidad y pruebas. Estrategia de testing (pirámide: 70% unit, 20% integración, 5% snapshot, 5% E2E), Definition of Done, matriz de cobertura, quality gates. |
| 09 | `09_devops/` | DevOps / CI-CD. Pipeline CI/CD (build → test → quality gates → pack → publish NuGet), estrategia SemVer 2.0, entornos de deploy, guía de publicación. |
| 10 | `10_developer_guide/` | Guía para consumidores. Documentación externa: quick start, formato DSL, perfiles de impresora, integración MAUI, API REST, templates de ejemplo. |
| 11 | `11_examples/` | Aplicaciones de referencia. 3 ejemplos progresivos ejecutables: ticket simple (básico), acta de infracción (intermedio) y consumo via NuGet (avanzado). |

---

## 8. Ejemplo Práctico Completo: Del Concepto al Sprint

A continuación se muestra el flujo completo para una funcionalidad real del proyecto — la creación del Developer Guide (`docs/10_developer_guide/`).

### Paso 1 — El humano plantea la necesidad a Claude

```
Lo que estaría faltando es hacer una documentación clara y precisa 
que defina el fundamento de la librería, cuáles son sus funcionalidades 
principales, el formato de los perfiles de impresoras, y el formato 
del template de documentos, con ejemplos que incluyan QR, códigos 
de barras, logos y formatos de texto para un ticket.

Esto no sé si es parte de algún documento de la estructura o del 
sprint concretamente. Determiná cómo sería esta fase de generación 
de documentación dentro del árbol de documentos.
Dame el prompteo para Copilot.
```

### Paso 2 — Claude analiza y propone ubicación

Claude identifica que esta documentación corresponde al rol AG-10 (Technical Writer), no al equipo interno, y propone la carpeta `docs/10_developer_guide/` con una estructura de archivos específica.

### Paso 3 — Claude genera el prompt para Copilot

```
Vamos a crear el Developer Guide completo de la librería.
Esta documentación está orientada al desarrollador que consume 
la librería, no al que la construye.

IMPORTANTE: NO modifiques ningún archivo existente.
Creá SOLO archivos nuevos en docs/10_developer_guide/

Leé primero estos archivos para entender el estado actual:
- docs/05_arquitectura_tecnica/guia-uso-libreria_v1.0.md
- docs/05_arquitectura_tecnica/contratos-del-motor_v1.0.md
- src/MotorDsl.Core/Models/DocumentNode.cs
- src/MotorDsl.Core/Models/DeviceProfile.cs
[...]

Creá estos archivos en orden, UNO A LA VEZ esperando 
confirmación entre cada uno:

ARCHIVO 1 — docs/10_developer_guide/README.md
ARCHIVO 2 — docs/10_developer_guide/conceptos-fundamentales.md
ARCHIVO 3 — docs/10_developer_guide/formato-dsl-templates.md
ARCHIVO 4 — docs/10_developer_guide/formato-perfiles-impresora.md
ARCHIVO 5 — docs/10_developer_guide/guia-integracion-maui.md
```

### Paso 4 — Copilot ejecuta, el humano revisa cada archivo

Copilot genera cada archivo. El humano ejecuta `Get-Content` para revisar y comparte la salida con Claude. Claude detecta typos (`parra` → `para`, `IDDeviceProfileProvider` con doble D) y los señala para corrección.

### Paso 5 — Coherencia post-generación

Antes de continuar con más funcionalidades, el humano pregunta a Claude:

```
Antes de mandar estos prompts, revisaste que el apartado devops 
sea consistente con estos que estamos por hacer?
```

Claude detecta 3 inconsistencias y genera un prompt correctivo para actualizar `pipeline-ci-cd_v1.0.md` antes de avanzar.

---

## 9. Anti-patrones Identificados

| Anti-patrón | Problema | Solución |
|---|---|---|
| Mega-prompt inicial | "Leé todo el repo y construí la solución" — el modelo prioriza lo que quiere | Prompts en capas: contexto → scaffolding → CU por CU |
| Aprobación sin revisión | Decir "sí, seguí" sin verificar — Copilot toma decisiones no deseadas | Siempre pedir confirmación explícita antes de generar código |
| Copilot sin lectura previa | Copilot inventa contexto porque no leyó los docs | Toda instrucción empieza con "Leé primero estos archivos" |
| Sesiones demasiado largas | Copilot compacta la conversación y pierde contexto | Máximo 2-3 CUs por sesión, usar prompts de reorientación |
| Claude sin feedback | Claude genera prompts sin ver los resultados de Copilot | Siempre compartir salida de Copilot con Claude |
| Add() en interfaces | Copilot mezcla responsabilidades (lectura + escritura en interfaces de proveedor) | Claude revisa el diseño antes de aprobar y corrige |
| Prompt sin verificación final | No se confirma que el resultado compila o pasa tests | Todo prompt termina con `dotnet test` o `dotnet build` |

---

## 10. Guía de Adaptación a Nuevos Proyectos

Para aplicar esta metodología a un proyecto diferente:

**Paso 1 — Preparar la estructura de referencia.** Tomar la estructura de `/docs/` (secciones 00 a 11) y evaluar cuáles aplican al nuevo proyecto. No todos los proyectos necesitan las 12 secciones — un microservicio puede prescindir de `03_ux-ui/` y `04_prompts_ai/`, mientras que una aplicación con IA necesitará expandir `04_prompts_ai/`.

**Paso 2 — Crear el proyecto en Claude.** Cargar los documentos de referencia como knowledge base y comenzar la Fase 1 (contextualización) y Fase 2 (definición de la idea).

**Paso 3 — Mapear agentes a la realidad del equipo.** No es necesario tener 13 personas — una persona puede cubrir múltiples roles. Lo importante es que cada sección tiene un perfil profesional que define el criterio de calidad.

**Paso 4 — Ejecutar el ciclo de 6 fases.** Empezar con las secciones de mayor impacto (generalmente 00 → 01 → 02 → 05) y avanzar hacia las de menor riesgo (10 → 11).

**Paso 5 — Establecer cadencia.** Ejecutar el prompt de coherencia (Fase 5) cada 2-3 sprints o cuando se completa una sección documental completa.

---

## 11. Diagrama del Flujo Completo

```
┌─────────────────────────────────────────────────────────────────┐
│                    CICLO DE PROMPTING SDD                       │
│                                                                 │
│  ┌──────────┐    Fase 1: Contextualización                      │
│  │  CLAUDE   │◄── Humano carga docs de referencia               │
│  │ (proyecto)│    Claude analiza estructura                      │
│  └────┬─────┘                                                   │
│       │                                                         │
│       ▼         Fase 2: Definición de la idea                   │
│  ┌──────────┐                                                   │
│  │  CLAUDE   │◄──► Humano aporta visión, restricciones,         │
│  │  (chat)   │     ejemplos prácticos, decisiones               │
│  └────┬─────┘                                                   │
│       │                                                         │
│       ▼         Fase 3: Generación de prompt                    │
│  ┌──────────┐                                                   │
│  │  CLAUDE   │──── Genera prompt especializado ──────┐          │
│  └──────────┘                                        │          │
│                                                      ▼          │
│                 Fase 4: Ejecución + Revisión    ┌──────────┐    │
│                 Humano revisa y aprueba     ◄───│  COPILOT  │   │
│                 cada artefacto                  │  (IDE)    │   │
│                      │                          └──────────┘    │
│                      │ comparte salida                          │
│                      ▼                                          │
│  ┌──────────┐   Fase 5: Coherencia                              │
│  │  CLAUDE   │──── Genera prompt de refinamiento ──────┐        │
│  └────┬─────┘                                          │        │
│       │                                                ▼        │
│       │         Fase 6: Retroalimentación         ┌──────────┐  │
│       │         Copilot ejecuta refinamiento       │  COPILOT  │ │
│       │                    │                       └─────┬────┘  │
│       │                    │ comparte informe             │      │
│       │◄───────────────────┘                              │      │
│       │                                                   │      │
│       └───────── Nuevo ciclo (sprint/funcionalidad) ──────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 12. Cadena de Trazabilidad SDD

La metodología garantiza trazabilidad vertical completa a través de toda la documentación:

```
Visión (00_contexto)
  → Necesidades de Negocio (NB-XX) (01_necesidades)
    → Casos de Uso (CU-XX) (02_especificacion)
      → Reglas de Negocio (RN-XX) (02_especificacion)
        → Decisiones de Arquitectura (ADR) (05_arquitectura)
          → User Stories (US-XX) (06_backlog)
            → Backlog Técnico (BT-XX) (06_backlog)
              → Sprint Plan (TK-XX) (07_plan-sprint)
                → Test Cases (08_calidad)
                  → Pipeline CI/CD (09_devops)
```

Cada prompt generado por Claude incluye la referencia a los documentos upstream que fundamentan la tarea, asegurando que Copilot no genere artefactos desconectados de la cadena.

---

## 13. Control de Cambios

| Versión | Fecha | Descripción |
|---------|-------|-------------|
| 1.0 | 2026-04-06 | Versión inicial — metodología completa con 6 fases, 7 patrones de prompt, tabla de 13 agentes, ejemplo práctico del Motor DSL |

---

**Fin del documento**
