---
doc_id: ANX-SDEI-GLOSARIO
doc_type: anexo
title: Anexo — glosario del sistema documentador
description: Términos y siglas del cuerpo con su definición, sus alias y dónde se ancla cada uno; un término, un significado.
status: borrador
origin: ia-assisted
confidence: alta
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-27
audience: [humano, agente, estudiante, docente, desarrollo, analista, po]
traces: [MARCO-SDEI, ANX-SDEI-ESQUEMA]
---

# Anexo — glosario

Un término, un significado. Cuando la industria usa otra palabra para lo mismo, figura como alias; el texto del cuerpo
usa siempre el término de la primera columna. La columna «Dónde se ancla» dice en qué documento se define con detalle
o qué control lo verifica.

## Índice

1. [Siglas de los controles](#1-siglas-de-los-controles)
2. [Términos](#2-términos)

## 1. Siglas de los controles

Hay cuatro familias de siglas parecidas. No se mezclan.

| Sigla | Qué nombra | Ejemplo |
| --- | --- | --- |
| `PR-1` a `PR-7` | Grupos de controles del carril PR de `docs-guard` | PR-5 es `traces-resolve` |
| `AI-1` a `AI-5` | Chequeos anti-inyección sobre el texto | AI-2 detecta caracteres invisibles |
| `AK-1` a `AK-7` | Controles anti-inyección del sistema (excepciones con huella, lista de dominios, fixture permanente) | AK-7 es el fixture que tiene que dar rojo en cada corrida |
| `G1` a `G4` | Guarda de merge de la conformación automática del pull request | G3: todos los controles de la cabeza en éxito |
| `CQ-nn`, `PE-nn`, `VER-…` | Criterios de calidad, patrones de ejemplos e ítems de verificación | Ver sus anexos |
| `CL-01` a `CL-14` | Clases de pieza documental | CL-06 es la ayuda al usuario final |

## 2. Términos

| Término | Alias | Definición | Dónde se ancla |
| --- | --- | --- | --- |
| Ancla de versión | — | El dato que dice contra qué estado del código vale una pieza: el commit en el mismo repositorio, un SHA fijado (`base_commit`, `source.lock`) entre repositorios, o una etiqueta | [Marco](../00-Marco.md#1-definiciones); carril programado (atraso) |
| Agente de IA | Agente | Programa basado en un modelo de lenguaje que lee y escribe en el repositorio. Propone trabajo de clase P | [Marco, actores](../00-Marco.md#4-actores) |
| `AGENTS.md` | Archivo de contexto | Archivo en la raíz de un repositorio que orienta a los agentes: dónde está el README del cuerpo, las convenciones y los anexos. Es clase H | CL-03 en el [Mapa](../01-Mapa.md#4-por-artefacto) |
| Canal derivado | — | Salida generada para agentes a partir del sitio publicado: una página `.md` por página, `llms.txt`. Sólo existe si hay sitio | CL-14; `CTX-SDEI-1` |
| Carril PR | — | Conjunto de controles que corre en cada pull request y bloquea | `docs-guard`, grupos PR-1 a PR-7 |
| Carril programado | — | Corrida semanal que sólo avisa: enlaces externos, atraso de anclas, frescura. Abre o actualiza un issue | `docs-enlaces-semanal` |
| Cita entre repositorios | — | Forma `Repo@ancla:corpus_id/doc_id` con enlace permanente al SHA. Dentro del cuerpo, en cambio, se usan enlaces relativos | [Convenciones](../00-Marco.md#5-convenciones) |
| Clase de intervención | D/P/H | D: la produce un generador determinista. P: la propone un agente o una persona y entra por la compuerta. H: sólo una persona | [Cómo decidir](Como-Decidir.md#6-quién-decide) |
| Clase de pieza | `CL-nn` | Categoría cerrada del catálogo de catorce clases: fija dónde vive la pieza, su ancla, su control y su tema | [Mapa](../01-Mapa.md#4-por-artefacto) |
| Compuerta | Gate | Control que corre en la CI y termina con un código de salida: 0 en orden, 1 bloquea | `docs-guard` |
| Conformación automática del pull request | CPA | El agente abre el pull request, publica el informe con el enlace, espera la guarda G1–G4 y fusiona con commit de merge. Vale sólo con la delegación asentada por escrito | [Cómo decidir](Como-Decidir.md#6-quién-decide) |
| Contexto | — | Variante que cruza los escenarios y cambia una respuesta concreta | [Marco](../00-Marco.md#3-contextos) |
| Copia fijada | Vendor | Copia de un conocimiento compartido, con su versión y su SHA-256 en `BASE-FIJADA.md`. Si el archivo cambia, la compuerta da rojo | CL-11; PR-7 |
| Cuerpo documental | Corpus | Conjunto de documentos de una guía con un README que declara `corpus_id` y `version` | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#3-readme-del-cuerpo-y-páginas-de-ayuda) |
| Deriva | Desactualización | Distancia entre lo que una pieza afirma y lo que el código hace | `traces-resolve` (PR); carril programado (aviso) |
| *Docs as code* | Documentación como código | Escribir la documentación en texto plano, versionarla en Git, revisarla por pull request y verificarla en la CI, con las mismas herramientas que el código | `TEM-01` |
| `docs-guard` | — | Workflow reutilizable de Lab-Documentos que corre los grupos PR-1 a PR-7 con un agregador estricto | `.github/workflows/docs-guard.yml` |
| Escenario | Tipo de sistema | Tipo de sistema que se documenta; el cuerpo tiene cuatro | [Marco](../00-Marco.md#2-escenarios) |
| Escala anclada | — | Escala de 1 a 5 donde cada punto tiene un observable | [Cómo decidir](Como-Decidir.md#3-la-escala-anclada) |
| Excepción con huella | — | Línea `sha256 ruta control` que exceptúa un archivo de un control mientras el archivo no cambie | `tools/docs-guard/excepciones.txt` |
| Frontmatter | Metadatos | Bloque YAML al principio del archivo | [Esquema](Esquema-Frontmatter-Y-Prefijos.md) |
| Ia-db | Índice interno | Índice para agentes en el repositorio hermano `.Documentacion`, anclado con `base_commit`. Es un índice, no un marco SDD | CL-08 |
| Integración | — | Dos acepciones que no se mezclan: en el ciclo del código, pull request más CI; en el producto, la ayuda embebida y el canal para agentes | `TEM-02` |
| Línea de base | — | Cantidad de hallazgos de un control sobre lo que vigila antes de empezar a bloquear. Un control bloquea sólo con línea de base 0 | [Criterios de calidad](Criterios-De-Calidad.md) |
| P-LECT | Prueba de lectura | Un lector sin contexto recorre una ruta y anota las dudas que el texto no responde; el umbral es cero dudas sin respuesta | `CQ-04`; `PE-09` |
| Par rojo/verde | Prueba negativa | Un fixture con el defecto (el control da rojo) y otro sin él (da verde) | `tools/docs-guard/fixtures/` |
| Pieza documental | — | Unidad que se escribe, se versiona y se valida por separado: un caso de uso, un `AGENTS.md`, una página de ayuda, el `openapi.json` | [Mapa](../01-Mapa.md#4-por-artefacto) |
| Registro de requerimientos documentales | RRD | Tabla con una fila por requerimiento: audiencia, tarea, pieza, criterio Dado/Cuando/Entonces y decisión de una persona | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#6-registro-de-requerimientos-documentales) |
| Reserva | — | Condición que devuelve el merge a una persona: un control falló, la cabeza cambió, o el agente declaró una duda | G1–G4 |
| SDD | Desarrollo guiado por especificación | Marco de trabajo donde la especificación (necesidades, casos de uso, reglas, decisiones) es la fuente de la que sale el código | `CTX-SDEI-2`; `TEM-07` |
| SDD-B | — | Alias del servicio de notificaciones push de repositorio privado que se usa como ejemplo del escenario `ESC-SDEI-4` | [Marco](../00-Marco.md#esc-sdei-4--servicio-con-paquetes) |
| Sistema documentador | — | Conjunto de piezas, fuentes de verdad, repositorios, anclas, compuertas, canales y actores que producen y mantienen la documentación de un sistema | [README](../README.md#1-qué-es-esta-guía) |
| `source.lock` | — | Archivo que fija contra qué SHA de otro repositorio vale una demostración | `CTX-SDEI-3`; PR-5 |
| Traza | `traces` | Dependencia declarada de un documento hacia un identificador; tiene que resolver contra el registro | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#4-trazas) |

Volver al [Mapa](../01-Mapa.md#6-anexos).
