---
doc_id: ANX-SDEI-VER
doc_type: anexo
title: Anexo — lista de verificación VER-* de una guía
description: Ítems VER-* que se responden con sí o no sobre una guía antes de pasarla a vigente, agrupados por lo que revisan y con su regla de origen.
status: borrador
origin: ia-assisted
confidence: alta
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-27
audience: [humano, agente, docente, analista, po]
traces: [ANX-SDEI-CQ, MARCO-SDEI]
---

# Anexo — lista de verificación

Cada ítem se responde con sí o no sobre el documento o el conjunto. Los identificadores `VER-…` son estables: una
revisión dice «no cumple `VER-G08`» y quien lee sabe qué falta. Esta lista recoge los ítems que se aplican a una guía
o a una demostración que se publica; la columna «Origen» nombra la regla del marco de prompts de la que sale cada uno
(Profile de guía de estudio y reglas *Evidences*, *Markdown*, *Documentation*, *Dual-Audience* y *Narrative-Voice*).

## Índice

1. [Cómo se usa](#1-cómo-se-usa)
2. [Evidencia y veracidad](#2-evidencia-y-veracidad)
3. [Estructura de la guía](#3-estructura-de-la-guía)
4. [Estructura de cada tema](#4-estructura-de-cada-tema)
5. [Markdown, índices y mermaid](#5-markdown-índices-y-mermaid)
6. [Contenido y organización](#6-contenido-y-organización)
7. [Cara humana](#7-cara-humana)
8. [Cara agente](#8-cara-agente)
9. [Voz narrativa](#9-voz-narrativa)
10. [Proceso](#10-proceso)

## 1. Cómo se usa

Antes de pasar un documento a `vigente`, quien revisa (no quien redactó) recorre las secciones que le aplican: un tema,
las secciones 2 a 9; un anexo, las secciones 2, 5, 6 y 8; el conjunto, además, la sección 3. Lo que la compuerta ya
verifica figura en la columna «Compuerta»; si el pull request está en verde, esos ítems se dan por cumplidos.

## 2. Evidencia y veracidad

| Id | Verificación | Compuerta | Origen |
| --- | --- | --- | --- |
| `VER-E01` | Ninguna información inventada | — | Evidences |
| `VER-E02` | Toda afirmación lleva evidencia verificable: `ruta:línea` o commit para el repositorio; referencia con fecha de consulta para fuentes externas | PR-7 `reflint` (formato) | Evidences |
| `VER-E03` | Lo no verificado está marcado («No verificado», «Requiere verificación manual», «Información no disponible») y dice por qué | — | Evidences |
| `VER-E04` | El criterio propio se declara como tal y se separa de los hechos | — | Profile; Evidences |
| `VER-E05` | Las conclusiones salen de la evidencia; no se omitió ni reinterpretó evidencia contraria | — | Evidences |
| `VER-E07` | Hay una sección de observaciones que distingue hechos de interpretaciones, cuando el documento las tiene | — | Evidences |
| `VER-E08` | Definiciones y prácticas anclan a estándares o fuentes de la industria | — | Profile |
| `VER-E09` | Las inconsistencias entre documentos se señalan explícitamente | — | Reglas generales |

## 3. Estructura de la guía

| Id | Verificación | Compuerta | Origen |
| --- | --- | --- | --- |
| `VER-G01` | Se diseñó el mapa antes de escribir piezas: mapa → ejes → un documento por tema → anexos | — | Profile |
| `VER-G02` | Los tres ejes (escenarios, contextos, actores) están definidos una sola vez, en documentos propios, y el resto los referencia | — | Profile |
| `VER-G03` | Ningún tema queda sin cruce con al menos un escenario o contexto | — | Profile |
| `VER-G04` | Hay mapa con tablas de entrada por escenario, por contexto y por artefacto | — | Profile |
| `VER-G05` | Hay un documento de métodos y marcos: alternativas, diferencias y criterios de elección | — | Profile |
| `VER-G06` | Hay anexos: plantillas comentadas, listas de verificación y glosario | — | Profile |
| `VER-G07` | El README tiene la tabla de contenido de todo lo generado y la ruta de lectura sugerida | — | Profile |
| `VER-G08` | Cada documento enlaza al que lo precede y al que lo continúa; ninguno queda huérfano del mapa | PR-1 (que los enlaces resuelvan) | Profile |
| `VER-G09` | Revisión final de la guía completa contra el mapa: huecos, solapamientos y contradicciones corregidos | — | Profile |
| `VER-G10` | Cada sección deja al lector en condiciones de decidir, no sólo de reconocer términos | — | Profile |

## 4. Estructura de cada tema

| Id | Verificación | Origen |
| --- | --- | --- |
| `VER-T01` | **Definición**: qué es, qué problema resuelve, qué no es | Profile |
| `VER-T02` | **Aplicación por escenario**, con lo que cambia según el contexto | Profile |
| `VER-T03` | **Ejemplos concretos** del dominio, no descripciones abstractas | Profile |
| `VER-T04` | **Preguntas guía** para producir o evaluar el artefacto | Profile; Dual-Audience |
| `VER-T05` | **Criterios de calidad**: cómo distinguir una versión buena de una pobre | Profile |
| `VER-T06` | **Anexo**: plantilla comentada con preguntas por campo | Profile |
| `VER-T07` | Los seis encabezados se llaman igual en todos los temas (anclas predecibles) | Profile; Dual-Audience |

## 5. Markdown, índices y mermaid

| Id | Verificación | Compuerta | Origen |
| --- | --- | --- | --- |
| `VER-M01` | Secciones jerárquicas, sin saltar niveles, sin títulos vacíos ni duplicados | PR-4 (MD001, MD024, MD025) | Markdown |
| `VER-M02` | Todo documento tiene índice | — | Markdown |
| `VER-M03` | Autocontenido y no delta: no remite a versiones previas ni a una conversación para entenderse | — | Markdown |
| `VER-M04` | Sin secciones vacías o decorativas | — | Markdown |
| `VER-M05` | Tablas para resumir o comparar, sin celdas de texto largo | PR-4 (MD056, columnas) | Markdown |
| `VER-M06` | Bloques de código con lenguaje indicado, mínimos y con contexto | — | Markdown |
| `VER-M07` | Diagramas en mermaid, sólo cuando aportan y sin redundancia | — | Markdown |
| `VER-M08` | Todo bloque mermaid es sintácticamente correcto | PR-4 (parser de mermaid) | Markdown |
| `VER-M09` | Los ejemplos reales salen de una ejecución; los ilustrativos están marcados | — | Markdown |
| `VER-M10` | Enlaces válidos; se referencia en lugar de duplicar | PR-1 | Markdown |
| `VER-M11` | Estilo técnico, didáctico y uniforme: el conjunto se lee como una única obra | — | Markdown |

## 6. Contenido y organización

| Id | Verificación | Origen |
| --- | --- | --- |
| `VER-D01` | Se documenta el estado real de los sistemas; lo no verificable está marcado | Documentation |
| `VER-D02` | Se explica (qué es, para qué, cómo funciona, cuándo, cómo se relaciona), no sólo se describe | Documentation |
| `VER-D03` | Las relaciones (dependencias, procesos, repositorios) están documentadas | Documentation |
| `VER-D04` | No se asume que el lector conoce el proyecto | Documentation |
| `VER-D05` | Los procedimientos traen objetivo, prerrequisitos, pasos, resultado esperado, validaciones y errores posibles | Documentation |
| `VER-D06` | La arquitectura documenta componentes, responsabilidades, relaciones, flujos y límites, no listas de archivos | Documentation |
| `VER-D07` | Cada dato vive en un solo documento; el resto lo referencia | Documentation |
| `VER-D08` | Documentos pequeños, de una sola responsabilidad | Documentation |
| `VER-D09` | No se eliminó ni sobrescribió contenido existente sin mandato explícito | Reglas generales |

## 7. Cara humana

| Id | Verificación | Origen |
| --- | --- | --- |
| `VER-H01` | Cada documento abre con un resumen breve: qué es, para qué, a quién sirve | Dual-Audience |
| `VER-H02` | Los flujos importantes se narran de punta a punta con un caso concreto; la narración complementa el diagrama | Dual-Audience |
| `VER-H03` | Cada término se define en su primer uso y está en el glosario | Dual-Audience |
| `VER-H04` | Las secciones densas cierran con preguntas guía | Dual-Audience |
| `VER-H05` | Todo ejemplo dice qué demuestra, sus precondiciones y el resultado esperado | Dual-Audience |

## 8. Cara agente

| Id | Verificación | Compuerta | Origen |
| --- | --- | --- | --- |
| `VER-A01` | Frontmatter con los diez campos (`doc_id`, `doc_type`, `title`, `status`, `origin`, `confidence` si `origin` no es `human`, `owner`, `last_review`, `audience`, `traces`) más `description` | PR-2 | Dual-Audience |
| `VER-A02` | IDs estables con prefijo; las trazas apuntan al ID, no a la ruta | PR-2 (prefijo); PR-5 | Dual-Audience |
| `VER-A03` | Documentos del mismo tipo comparten secciones y anclas | — | Dual-Audience |
| `VER-A04` | Diagramas y modelos como código; binarios sólo sin alternativa | — | Dual-Audience |
| `VER-A05` | Los documentos que definen un proceso que un agente ejecuta tienen bloques entradas, salidas y validaciones | — | Dual-Audience |
| `VER-A06` | Fragmentos con procedencia: ruta, rango y versión | — | Dual-Audience |
| `VER-A07` | Cara humana y cara agente dicen lo mismo; los sinónimos figuran como alias en el glosario | — | Dual-Audience |

## 9. Voz narrativa

| Id | Verificación | Origen |
| --- | --- | --- |
| `VER-V01` | La prosa responde al caso concreto, no a una plantilla rellenada | Narrative-Voice |
| `VER-V02` | Afirma con seguridad donde hay evidencia; señala la incertidumbre sin rodeos defensivos | Narrative-Voice |
| `VER-V03` | Largo de frase variado | Narrative-Voice |
| `VER-V04` | Causa → efecto → impacto en párrafos; listas sólo para enumeraciones reales | Narrative-Voice |
| `VER-V05` | Sin paralelismo forzado en listas | Narrative-Voice |
| `VER-V06` | Ninguna sección abre reformulando su título ni anunciando lo que va a tratar | Narrative-Voice |
| `VER-V07` | Sin muletillas («Es importante destacar», «Cabe destacar», «En resumen», «En conclusión») | Narrative-Voice |
| `VER-V08` | Sin conectores decorativos encadenados («Además», «Asimismo», «Por otro lado») | Narrative-Voice |
| `VER-V09` | Los cierres aportan información o recomendación; si un párrafo se borra sin perder nada, sobra | Narrative-Voice |
| `VER-V10` | Una sola voz en todo el conjunto | Narrative-Voice; Profile |
| `VER-V11` | Castellano rioplatense neutro y claro | Convención del cuerpo |

## 10. Proceso

| Id | Verificación | Origen |
| --- | --- | --- |
| `VER-W06` | Se reutilizó lo existente (guía anterior, ejemplos, conocimientos) antes de crear | Workflow |
| `VER-W09` | El workflow de CI corre en el runner con la etiqueta exacta `[self-hosted, i7infra-dev]` | Pedido del caso |
| `VER-I04` | Los documentos nuevos o los cambios de estructura quedan reflejados en el índice interno (ia-db) antes de cerrar el escalón | Indexing |
| `VER-I06` | No se duplica conocimiento entre índices | Indexing |

Volver al [Mapa](../01-Mapa.md#6-anexos).
