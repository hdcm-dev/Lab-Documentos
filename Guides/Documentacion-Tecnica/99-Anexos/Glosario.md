---
doc_id: ANEXO-GLOSARIO
doc_type: anexo
title: Glosario
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Documentación técnica
last_review: 2026-07-18
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, MARCO-CONVENCIONES, MAPA-CONCEPTUAL]
---

# Glosario

## Resumen ejecutivo

Término canónico, definición breve y alias registrados. Cuando dos documentos de la guía usan palabras distintas para lo mismo, el conflicto se resuelve acá: prevalece el término canónico y el otro queda registrado como alias. Los sinónimos que circulan en la industria se listan aunque esta guía no los use, porque el lector se los va a encontrar.

Este es el glosario **de la guía**, y su alcance son los términos del oficio. No debe confundirse con los glosarios que aparecen dentro del SAD, del PRD, del modelo de dominio, del Developer Guide, del manual de usuario y de la guía del administrador: aquellos pertenecen a las plantillas y a los ejemplos, y su alcance es el vocabulario **del sistema documentado**, que en un proyecto real es distinto en cada caso. Un producto tiene un solo glosario de dominio, y vive donde el equipo decida; esta guía sugiere el modelo de dominio como sede natural, porque es donde el lenguaje del negocio se fija por primera vez.

---

## Términos del marco de referencia

| Término | Definición | Alias |
|---------|-----------|-------|
| **Escenario** | Situación de partida que determina si la documentación decide, describe o infiere. Cuatro en esta guía (`ESC-1` a `ESC-4`). | Situación de proyecto |
| **Contexto** | Tipo de software sobre el que se documenta: web, backend o fullstack (`CTX-1` a `CTX-3`). | Entorno, vertical técnica |
| **Actor** | Rol con autoridad y alcance definidos sobre ciertos documentos (`ACT-01` a `ACT-10`). | Rol, interesado, *stakeholder* |
| **Familia documental** | Agrupación de artefactos que responden a la misma pregunta. Siete en esta guía. | Categoría documental |
| **Artefacto** | Documento concreto y nombrado, con propósito, dueño y audiencia propios. | Entregable documental |
| **Traza** | Vínculo explícito entre dos elementos identificados, que permite seguir un requisito hasta su verificación. | Trazabilidad, *traceability link* |
| **Criterio de paridad** | Definición operativa de «hace lo mismo» entre sistema origen y destino en una migración (`ESC-2`). | Equivalencia funcional |
| **Doble audiencia** | Propiedad de un documento que sirve a la vez a una persona que comprende y a un agente que extrae datos. | — |

---

## Artefactos documentales

| Término | Definición | Alias |
|---------|-----------|-------|
| **Vision Document** | Qué es el producto, qué problema resuelve, alcance y público. Visión de negocio. | Documento de visión |
| **BRD** | Business Requirements Document: necesidades del negocio que justifican el proyecto. | Requisitos de negocio |
| **PRD** | Product Requirements Document: funcionalidades desde la perspectiva del usuario y del negocio. | Especificación de producto |
| **SRS** | Software Requirements Specification: requisitos funcionales y no funcionales detallados. | ERS, especificación de requisitos |
| **Caso de uso** | Descripción de la interacción entre un actor y el sistema para lograr un objetivo, con flujo principal y alternativos. | *Use case* |
| **Regla de negocio** | Restricción o política del dominio que el sistema debe respetar, independiente de la interfaz que la exponga. | *Business rule* |
| **Modelo de dominio** | Entidades, conceptos y relaciones del negocio, en el lenguaje del negocio. | Modelo conceptual |
| **SAD** | Software Architecture Document: arquitectura completa, componentes, relaciones y principios. | Documento de arquitectura |
| **HLD** | High Level Design: módulos principales e interacción entre ellos, sin detalle de implementación. | Diseño de alto nivel |
| **LLD** | Low Level Design: clases, interfaces, tablas, algoritmos y detalles de implementación. | Diseño detallado |
| **ADR** | Architecture Decision Record: una decisión, sus alternativas y su justificación. | Registro de decisión |
| **RFC** | Propuesta abierta a comentarios antes de implementar un cambio. En esta guía, RFC interna de equipo. | Propuesta de diseño |
| **Modelo de datos** | Estructura física o lógica de los datos: entidades, atributos, relaciones, claves e índices. | Esquema, DER |
| **API Specification** | Contrato de una interfaz consumida por programas: recursos, operaciones, esquemas y errores. | Contrato de API |
| **Integration Guide** | Cómo integrar sistemas externos con la plataforma. | Guía de integración |
| **Arquitectura de seguridad** | Autenticación, autorización, cifrado, auditoría y controles del sistema. | — |
| **Threat Model** | Análisis de amenazas, riesgos y mitigaciones sobre un sistema o flujo. | Modelado de amenazas |
| **Test Plan** | Estrategia de pruebas, niveles, alcance y criterios de aceptación. | Plan de pruebas |
| **Test Case** | Caso concreto con precondiciones, pasos y resultado esperado. | Caso de prueba |
| **Installation Guide** | Instalación desde cero en un entorno. | Guía de instalación |
| **Deployment Guide** | Despliegue de una versión en los distintos entornos. | Guía de despliegue |
| **Operations Guide** | Operación diaria: monitoreo, mantenimiento, respaldo y recuperación. | Manual de operación |
| **Administrator Guide** | Configuración, usuarios, permisos y mantenimiento, para administradores del sistema. | Manual del administrador |
| **Runbook** | Procedimiento operativo para resolver un incidente o ejecutar una tarea repetitiva. | Procedimiento operativo, *playbook* |
| **Disaster Recovery** | Plan de continuidad y recuperación ante pérdida de servicio o de datos. | DR, plan de contingencia |
| **Postmortem** | Análisis de un incidente ocurrido: línea de tiempo, causas y acciones correctivas. | *Incident report*, análisis post-incidente |
| **Developer Guide** | Estructura del proyecto, convenciones, arquitectura y flujo de trabajo, para quien programa. | Guía del desarrollador |
| **Coding Standards** | Convenciones de escritura de código acordadas y, en lo posible, automatizadas. | Estándares de codificación |
| **Git Workflow** | Política de ramas, integración y revisión adoptada por el equipo. | Flujo de trabajo de ramas |
| **CI/CD** | Integración y entrega continuas: automatización de construcción, verificación y publicación. | *Pipeline* |
| **User Manual** | Manual orientado a la persona que usa el sistema. | Manual del usuario |
| **Tutorial** | Recorrido guiado orientado al aprendizaje, con un resultado garantizado. | — |
| **Guía rápida** | Referencia breve para ejecutar una tarea conocida. | *Quick start*, *cheat sheet* |
| **FAQ** | Respuestas a preguntas frecuentes, ordenadas por la pregunta y no por la funcionalidad. | Preguntas frecuentes |
| **Release Notes** | Qué cambió en una versión, redactado para quien usa el producto. | Notas de versión |
| **Change Log** | Historial cronológico de cambios, redactado para quien desarrolla o integra. | Registro de cambios |
| **Roadmap** | Evolución prevista del producto, en horizontes y no en fechas exactas. | Hoja de ruta |

---

## Métodos, modelos y prácticas

| Término | Definición | Alias |
|---------|-----------|-------|
| **Scrum** | Marco de trabajo con iteraciones de duración fija, roles y artefactos definidos. | — |
| **Kanban** | Sistema de flujo continuo con límites de trabajo en curso y políticas explícitas. | — |
| **Business Model Canvas** | Lienzo de nueve bloques que describe cómo un negocio crea y captura valor. | BMC, lienzo de modelo de negocio |
| **Lean Canvas** | Variante del anterior orientada a productos nuevos y validación de hipótesis. | — |
| **Definition of Done** | Criterios que un incremento debe cumplir para considerarse terminado, incluidos los documentales. | DoD |
| **Monolito** | Sistema desplegado como una unidad única. Su variante ordenada es el monolito modular. | — |
| **Modelo de capas** | Organización en niveles con dependencia dirigida: presentación, aplicación, dominio, infraestructura. | Arquitectura en capas, *n-tier* |
| **Cliente-servidor** | Estilo en el que un componente solicita y otro provee, con la frontera de red entre ambos. | — |
| **Arquitectura hexagonal** | Dominio aislado del exterior mediante puertos y adaptadores. | Puertos y adaptadores |
| **Microservicios** | Servicios desplegables de forma independiente, con datos propios y comunicación por red. | — |
| **Spec-Driven Development** | Práctica en la que la especificación es la fuente de verdad de la que se deriva el código. | SDD |
| **Diátaxis** | Marco que distingue tutorial, guía práctica, referencia y explicación como géneros documentales. | — |
| **Deuda documental** | Distancia acumulada entre el sistema real y lo que la documentación afirma. | — |

---

## Convenciones de identificación usadas en los ejemplos

| Prefijo | Elemento |
|---------|----------|
| `RF-` | Requisito funcional |
| `RNF-` | Requisito no funcional |
| `RN-` | Regla de negocio |
| `CU-` | Caso de uso |
| `ADR-` | Decisión de arquitectura |
| `TC-` | Caso de prueba |
| `RSK-` | Riesgo |
| `FLU-` | Flujo de usuario |

Los prefijos del propio aparato de la guía —`ESC-`, `CTX-`, `ACT-`, `DOC-`, `FAM-`, `MET-`, `ARQ-`— están definidos en [Convenciones](../00-Marco-de-Referencia/Convenciones.md).

---

## Términos que esta guía evita deliberadamente

**«Documentación»** a secas, cuando se puede nombrar el artefacto. Pedir «documentación» de un sistema es una solicitud sin criterio de terminación; pedir un SAD reconstruido y un modelo de datos verificado contra el esquema, no.

**«Documentar el código»**, cuando se quiere decir otra cosa. Los comentarios y la documentación de API generada desde el código son un género; el cuerpo documental de un sistema es otro, y confundirlos produce equipos que creen estar documentados porque tienen XML comments.

**«Arquitectura»** para referirse a la estructura de carpetas. Es el error más frecuente de `ESC-3`.
