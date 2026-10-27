---
doc_id: DEMO-MU-911
doc_type: ficha
title: RN-LOC-01, largo del nombre de la localidad
description: Prueba negativa real de docs-guard; versión 1.0 sin corregir de la ficha RN-LOC-01 de la mini demo MovilidadUrbana.
status: borrador
origin: ia-generated
confidence: alta
owner: Lab-Documentos
last_review: 2026-10-27
audience: [analista, desarrollo, agente]
traces: [DEMO-MU-911]
---

# RN-LOC-01 — Largo del nombre de la localidad

> Fixture permanente de docs-guard (expediente 0002, E1-U3): la ficha tal como se derivó de la letra de la pantalla,
> antes de compararla con el código. Contra el SHA de `source.lock` **tiene que dar rojo** (defecto real 60/80).

**Versión:** 1.0

## Índice

1. [Ficha](#1-ficha)
2. [Anclas verificables](#2-anclas-verificables)

## 1. Ficha

Forma de la ficha por regla de *Deducción de reglas de negocio* (§5.1), con un apartamiento declarado: el campo
**Ancla** usa `ruta#Símbolo@SHA` en lugar de `archivo:línea`, para que un renombre o un corrimiento de líneas no
rompa la traza sin que nadie lo vea. Las rutas son relativas a la raíz del repositorio del código y el SHA es el de
[`source.lock`](source.lock).

| Campo | Contenido |
| --- | --- |
| Id | `RN-LOC-01` (provisional: MovilidadUrbana no tiene registro de reglas; el ID lo asigna esta demo) |
| Enunciado atómico | El nombre de una localidad tiene entre 3 y 60 caracteres, sin contar los espacios de los extremos |
| Ancla | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.LargoMaximoDelNombre@8fb4c3a` y las seis de §2 |
| Letra | «Entre 3 y 60 caracteres, único por provincia.» (texto de ayuda bajo el campo Nombre) |
| Intención | Que el nombre entre en las vistas y en la base sin cortarse. Respaldos: (a) la constante `LargoMaximoDelNombre = 60`; (b) la columna `Nombre` se declara con `HasMaxLength(60)` |
| ¿Divergen? | No |
| Especie | Restricción |
| Momento | Precondition del alta y de la modificación de una localidad |
| Quién la verifica hoy | `LocalidadRules.NombreValido` (mínimo y máximo) |
| Dueño | `LocalidadRules`, en el dominio de MovilidadUrbana.MAUI |
| Depende de | Ninguna |
| Grado de certeza | Código (leído en el SHA fijado; las anclas de §2 lo comprueban en cada corrida) |
| Prueba | Dado un nombre de 61 caracteres, cuando se guarda la localidad, entonces el servicio devuelve un error en `nombre` |
| Propuesta | Que `NombreValido` compare también contra `LargoMaximoDelNombre` y que el campo use `MaxLength="60"`. Es una propuesta para el dueño del código: la decide una persona, porque elegir entre 60 y 80 es una divergencia entre letra e intención. Residuo: mientras no se decida, la ayuda dice lo que la app hace |

## 2. Anclas verificables

Cada fila la comprueba `traces-resolve` contra el árbol del SHA de `source.lock`. La columna **Afirma** dice qué tiene
que ser cierto en el código: `= v` (el valor del símbolo), `contiene` o `no contiene` (el texto de su declaración).
Si una fila no se cumple, la compuerta da rojo.

| Rol | Ancla | Afirma |
| --- | --- | --- |
| declara | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.LargoMaximoDelNombre@8fb4c3a` | = `60` |
| declara | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.LargoMinimoDelNombre@8fb4c3a` | = `3` |
| muestra | `src/MovilidadUrbana.MAUI/Application/Localidades/LocalidadPolicy.cs#LocalidadPolicy.RequisitoDelNombre@8fb4c3a` | contiene `LargoMaximoDelNombre` |
| verifica | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.NombreValido@8fb4c3a` | contiene `LargoMaximoDelNombre` |
| aplica | `src/MovilidadUrbana.MAUI/Pages/LocalidadEditorPage.xaml#NombreEntry.MaxLength@8fb4c3a` | = `60` |
| persiste | `src/MovilidadUrbana.MAUI/Infrastructure/Persistence/AppDbContext.cs#AppDbContext.OnModelCreating@8fb4c3a` | contiene `l.Nombre).HasMaxLength(60)` |
