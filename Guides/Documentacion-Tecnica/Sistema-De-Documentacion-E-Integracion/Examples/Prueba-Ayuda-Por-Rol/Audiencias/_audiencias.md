---
doc_id: AUD-DEMO-AYR
doc_type: registro-audiencias
title: Registro de audiencias de la prueba extendida de ayuda por rol
description: Audiencias ficticias de la prueba extendida, con rol, tipo de alcance, superficie, nivel de acceso y jerarquía.
status: borrador
origin: ia-generated
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-02
audience: [analista, desarrollo]
traces: [TEM-SDEI-05]
---

# Registro de audiencias de la prueba extendida

**Contenido ficticio.** Las audiencias, las aplicaciones `app-a` y `app-b` y todo lo que se entrega con ellas son
inventados para la prueba: no describen ningún producto real.

## 1. Repositorio

Visibilidad del repositorio: `publico`

Un repositorio público sólo puede contener fuentes de nivel `publico` o `producto` (control `nivel-vs-repo`).

## 2. Audiencias

La columna «Incluye» declara la jerarquía: quien está en la fila recibe también lo de las audiencias que incluye. El
build la expande. Cambiar la jerarquía es clase H.

| Audiencia | Rol | Alcance | Superficie | Nivel | Incluye |
| --- | --- | --- | --- | --- | --- |
| admin-raiz | RootAdmin | generico | panel | producto | admin-app |
| admin-app | AppAdmin | por-aplicacion | panel | producto | usuario |
| usuario | User | por-aplicacion | panel, paquete | publico | — |
| integrador | Integrator | generico | portal | restringido | — |
| equipo | — | generico | ninguna | interno | — |

`integrador` y `equipo` no tienen páginas en este repositorio: su nivel exige fuente en un repositorio privado. Lo que
recibe el integrador en la prueba es un documento OpenAPI generado desde puntos finales ficticios, no una fuente
escrita.
