---
doc_id: DEMO-AYR-01
doc_type: ficha
title: RN-DEMO-ASUNTO, largo máximo del asunto de un aviso
description: Ficha ficticia de la regla que limita el asunto de un aviso, con las anclas muestra, valida y contiene.
status: borrador
origin: ia-generated
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-02
audience: [analista, desarrollo]
traces: [RD-31]
---

# RN-DEMO-ASUNTO — Largo máximo del asunto de un aviso

**Versión:** 1.0

**Contenido ficticio.** La regla y su límite son inventados para la prueba extendida.

## 1. Ficha

| Campo | Contenido |
| --- | --- |
| Id | `RN-DEMO-ASUNTO` (asignado por esta prueba) |
| Enunciado atómico | El asunto de un aviso es obligatorio y tiene como máximo 80 caracteres |
| Límite | 80 |
| Dueño | `ReglaAsunto.Maximo`, en `Audiencias/Formulario.cs` |
| Prueba | Dado un asunto de 120 caracteres, cuando se valida el formulario, entonces el campo muestra el error de la capa `validacion` |

## 2. Anclas hacia el producto

La traza va de la ficha al microtexto. Las anclas no llevan SHA porque el código está en este mismo árbol; las
resuelve la consola de la prueba (control `anclas` extendido, E-5), no `docs-guard`.

| Ancla | Destino | Contiene |
| --- | --- | --- |
| valida | `AvisoForm.Asunto` → `StringLength` | 80 |
| muestra | `Microtexto.resx` → `Asunto.Pista` | 80 |
| muestra | `Microtexto.resx` → `Asunto.Validacion` | 80 |
