---
doc_id: ANX-905
doc_type: ficha
title: Ficha de prueba
description: Documento de prueba de docs-guard; sólo existe para el par rojo/verde de un control.
status: borrador
origin: ia-generated
confidence: alta
owner: docs-guard
last_review: 2026-10-27
audience: [humano, agente]
traces: [ANX-905]
---

# Ficha de prueba

| Rol | Ancla | Afirma |
| --- | --- | --- |
| declara | `src/Reglas.cs#Reglas.Maximo@0123456` | = `60` |
| verifica | `src/Reglas.cs#Reglas.Valido@0123456` | no contiene `Maximo` |
| aplica | `src/Pagina.xaml#Campo.MaxLength@0123456` | = `80` |
