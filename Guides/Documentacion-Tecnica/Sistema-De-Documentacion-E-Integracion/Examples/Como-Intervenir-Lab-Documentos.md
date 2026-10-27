---
doc_id: GP-SDEI-INTERVENIR-LD
doc_type: proceso
title: Cómo intervenir en Lab-Documentos
description: Instancia de una página de la plantilla Cómo intervenir para este repositorio, con sus rutas, su compuerta y quién decide.
status: borrador
origin: ia-assisted
confidence: alta
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-27
audience: [humano, agente, desarrollo, docente]
traces: [GP-SDEI-INTERVENIR, IDX-SDEI@2.0]
---

# Cómo intervenir en Lab-Documentos

Instancia de la plantilla [Cómo intervenir](Como-Intervenir.md) para este repositorio. Los pasos, con sus bloques de
entradas, salidas y validaciones, están en la plantilla; esta página da los datos de Lab-Documentos.

| Campo | Lab-Documentos |
| --- | --- |
| Escenario | `ESC-SDEI-1` Corpus de guías. Clases activas: CL-02, CL-03, CL-09, CL-10, CL-11, CL-12 |
| Dónde vive cada pieza | Guías en `Guides/` y `Concepts/`; este cuerpo en `Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/`; ejemplos en su `Examples/`; controles en `tools/docs-guard/`; expedientes e índices en el repositorio hermano Lab-Documentos.Documentacion |
| Guion de validación | `scripts/ci-local.sh todo <carpeta>` desde la raíz del repositorio. Sin `<carpeta>`, verifica este cuerpo. Requiere bash, curl, Python 3 y Node.js |
| Compuerta en la CI | `.github/workflows/docs-guard.yml` en cada pull request, en el runner `[self-hosted, i7infra-dev]`; `docs-enlaces-semanal.yml` como carril programado |
| Quién decide | Hay delegación del merge asentada en el [CHANGELOG](../../../../CHANGELOG.md): D y P sin reserva los fusiona el agente por la guarda. Lo de clase H (pasar a `vigente`, etiquetar, cambiar un workflow) lo decide la persona responsable del producto |
| Excepciones | Ninguna respecto de la plantilla |

**Las guías prácticas de los ejemplos.** Cada ejemplo de `Examples/` tiene su propia guía práctica; para intervenirlo
se sigue la plantilla con la carpeta del ejemplo como `<carpeta>` del paso 3.

| Ejemplo | Guía práctica | Qué agrega al paso 3 |
| --- | --- | --- |
| Mini demo MovilidadUrbana | [Tutorial](MovilidadUrbana-Mini-Demo/README.md) | Su `verificar.sh` y el workflow `mini-demo-movilidad.yml` comprueban contrato y anclas contra el SHA de `source.lock` |
| Ayuda por rol | [Lanzar el ejemplo](Lanzar-Example.md) | Levantarlo en su devcontainer y abrir cada rol |
