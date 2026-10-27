---
doc_id: DEMO-MU-12
doc_type: ficha
title: RN-LOC-02, localidad única por nombre y provincia
description: Ficha de la regla de unicidad de una localidad por nombre y provincia en MovilidadUrbana.MAUI, con sus anclas al código fijado.
status: borrador
origin: ia-generated
confidence: alta
owner: Lab-Documentos
last_review: 2026-10-27
audience: [analista, desarrollo, agente]
traces: [RD-04]
---

# RN-LOC-02 — Localidad única por nombre y provincia

**Versión:** 1.0

## Índice

1. [Ficha](#1-ficha)
2. [Anclas verificables](#2-anclas-verificables)

## 1. Ficha

Misma forma y mismo apartamiento que [RN-LOC-01](RN-LOC-01.md#1-ficha): el ancla es `ruta#Símbolo@SHA`.

| Campo | Contenido |
| --- | --- |
| Id | `RN-LOC-02` (provisional, asignado por esta demo) |
| Enunciado atómico | En una misma sesión no puede haber dos localidades con el mismo nombre en la misma provincia; el nombre se compara sin distinguir mayúsculas y sin los espacios de los extremos |
| Ancla | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.MismaLocalidad@8fb4c3a` y las de §2 |
| Letra | «Entre 3 y 60 caracteres, único por provincia.» y, al fallar, «Ya existe una localidad con ese nombre en la provincia.» |
| Intención | Evitar duplicados que la persona no distinguiría en la lista. Respaldo: el comentario de `MismaLocalidad` |
| ¿Divergen? | No |
| Especie | Restricción |
| Momento | Precondition del alta y de la modificación |
| Quién la verifica hoy | `LocalidadService.SaveAsync`, antes de guardar |
| Dueño | `LocalidadRules`, en el dominio de MovilidadUrbana.MAUI |
| Depende de | `RN-LOC-01` (el nombre que se compara ya es válido) |
| Grado de certeza | Código |
| Prueba | Dadas «Goya» en Corrientes, cuando se da de alta «goya » en Corrientes, entonces el servicio devuelve el error de duplicado en `nombre` |
| Propuesta | No corresponde: no hay divergencia |

La base no tiene índice único por nombre y provincia: la regla vive sólo en el servicio. No es una divergencia (la
letra no promete nada sobre la base), pero es un dato para quien lea el código.

## 2. Anclas verificables

| Rol | Ancla | Afirma |
| --- | --- | --- |
| declara | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.MismaLocalidad@8fb4c3a` | contiene `OrdinalIgnoreCase` |
| muestra | `src/MovilidadUrbana.MAUI/Application/Localidades/LocalidadPolicy.cs#LocalidadPolicy.RequisitoDelNombre@8fb4c3a` | contiene `único por provincia` |
| verifica | `src/MovilidadUrbana.MAUI/Application/Localidades/LocalidadService.cs#LocalidadService.SaveAsync@8fb4c3a` | contiene `LocalidadRules.MismaLocalidad` |
| verifica | `src/MovilidadUrbana.MAUI/Application/Localidades/LocalidadService.cs#LocalidadService.SaveAsync@8fb4c3a` | contiene `Ya existe una localidad con ese nombre en la provincia.` |
