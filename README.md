---
doc_id: LAB-DOCUMENTOS-README
doc_type: indice
title: Lab-Documentos
status: vigente
origin: ia-generated
confidence: alta
owner: ACT-09 Technical Writer
last_review: 2026-10-27
audience: [humano, agente]
traces: [GUIAS-INDICE, DOC-SDT-0001, GUIA-INDICE]
---

# Lab-Documentos

Repositorio de guías de estudio y conceptos para el desarrollo de software: arquitectura, documentación técnica,
metodologías, despliegue, pruebas e IA. Todo está escrito en Markdown y se lee directamente en GitHub.

## Índice

1. [Ruta de lectura mínima](#ruta-de-lectura-mínima)
2. [Estructura](#estructura)
3. [Compuerta de documentación](#compuerta-de-documentación)

## Ruta de lectura mínima

1. [Guías](Guides/README.md): el índice por categoría.
2. [Guía de estudio — Documentación técnica](Guides/Documentacion-Tecnica/Creacion-Guias-Tecnicas/README.md): las
   familias de documentos, los actores y las convenciones de frontmatter que usa el resto del corpus.
3. [Sistema documentador técnico](Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/Sistema-Documentador-Tecnico-Guide.md):
   de Markdown al portal publicado, con una prueba de concepto de ayuda por rol.

## Estructura

| Carpeta | Contenido |
| --- | --- |
| [`Guides/`](Guides/README.md) | Guías de estudio, agrupadas por categoría |
| [`Concepts/`](Concepts/) | Material conceptual de apoyo |
| [`scripts/`](scripts/) | Verificación local de la documentación |
| [`tools/docs-guard/`](tools/docs-guard/README.md) | Controles de la compuerta de documentación y del carril programado |
| [`CHANGELOG.md`](CHANGELOG.md) | Cambios del repositorio y delegaciones vigentes |
| [`AGENTS.md`](AGENTS.md) | Puntero para agentes de código: ruta de lectura, validación y qué no pueden cambiar solos |

## Compuerta de documentación

Cada pull request pasa por `docs-guard`: enlaces internos y anclas, esquema de frontmatter, anti-inyección, estructura
y mermaid, trazas, contrato OpenAPI y piezas fijadas, más la autoprueba de cada control
([`tools/docs-guard/`](tools/docs-guard/README.md)). Una vez por semana, `docs-enlaces-semanal` revisa los enlaces entre
repositorios y externos y deja el informe en un issue. Para correr la compuerta en local:

```bash
scripts/ci-local.sh
```
