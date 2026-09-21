---
doc_id: GF-09
doc_type: guia-practica
title: Guía práctica de GitFlow — escenarios de un equipo de tres personas
status: vigente
origin: agente
confidence: alta
owner: Lab-GitFlow
last_review: 2026-08-23
audience: [desarrollo, qa, devops, po]
traces: [GF-06, GF-07, GF-08]
---

# Guía práctica de GitFlow

Los ocho documentos anteriores explican el modelo; este lo pone a correr. La práctica se hace sobre
el repositorio [`Lab-GitFlow`](https://github.com/hdcm-dev/Lab-GitFlow), con la aplicación de
`Lab-E2E.WebBlazor` como sistema bajo prueba, y está pensada para un equipo de **tres personas** que
rotan por los roles.

Los ocho escenarios están en un único documento autocontenido:
**[Guia-Practica-GitFlow.md](Guia-Practica-GitFlow.md)**. Este README es el índice: explica quién
hace qué, en qué orden se ejecutan y qué hace falta antes de empezar.

Cada escenario se puede hacer de a uno, pero rinde mucho más con las tres personas en simultáneo:
buena parte de lo que hay que aprender —esperar una revisión, descubrir que alguien mergeó antes,
decidir si una corrección entra a la release— solo aparece cuando hay más de una persona tocando el
repositorio.

## Los tres integrantes y su rotación

Para no atar los roles a las personas, la guía los nombra **I1**, **I2** e **I3**, y los rota por
escenario. La correspondencia con los actores de
[01 — Marco de referencia](../Estandares-Modelo-Ramas-Guide/Estandares-Modelo-Ramas.md#1-marco-de-referencia) es esta:

| Escenario | I1 | I2 | I3 |
|---|---|---|---|
| [01 Funcionalidad nueva](Guia-Practica-GitFlow.md#3-escenario-01--funcionalidad-nueva-e-01) | A-DEV | A-REV | A-QA |
| [02 Defecto con release abierta](Guia-Practica-GitFlow.md#4-escenario-02--defecto-con-release-abierta-e-02) | A-QA | A-DEV | A-REV + A-OPS |
| [03 Corte de release](Guia-Practica-GitFlow.md#5-escenario-03--corte-de-release-e-03-y-e-04) | A-OPS | A-PO | A-QA |
| [04 PR que rompe la regresión](Guia-Practica-GitFlow.md#6-escenario-04--pull-request-que-rompe-la-regresión-e-08) | A-DEV | A-REV | A-QA |
| [05 Emergencia en producción](Guia-Practica-GitFlow.md#7-escenario-05--emergencia-en-producción-e-05) | A-DEV | A-OPS | A-AUT |
| [06 Versión de demostración](Guia-Practica-GitFlow.md#8-escenario-06--versión-de-demostración-e-06) | A-OPS | A-PO | A-DEV |
| [07 Cierre y auditoría](Guia-Practica-GitFlow.md#9-escenario-07--cierre-y-auditoría) | los tres | | |

La rotación es deliberada: quien nunca cortó una release no entiende por qué el tamaño de los pull
requests le importa a otro.

## Cómo se usa

Cada escenario tiene la misma estructura, y conviene respetarla:

1. **Objetivo** — qué se aprende, en una línea.
2. **Precondición** — en qué estado tiene que estar el repositorio antes de empezar.
3. **Pasos** — los comandos y las acciones en GitHub, en orden.
4. **Qué observar** — lo que hay que mirar mientras corre; es la parte formativa.
5. **Errores frecuentes** — lo que suele salir mal y qué significa.
6. **Verificación** — cómo se comprueba que el escenario quedó bien resuelto.

**El orden de lectura no es el orden de ejecución.** La numeración agrupa por tema; las
precondiciones mandan. El único orden ejecutable es este, y es el que hay que seguir:

**00 → 01 → 03 → 02 → 04 → 05 → 06 → 07**

El 03 va antes que el 02 porque el 02 exige `release/1.0` con su candidata, y el único escenario que
la crea es el 03. El 05 exige además la versión `v1.0.0` liberada, que produce el paso 6 del 03.
Cada escenario deja el repositorio en el estado que el siguiente de **esta** secuencia necesita.

## Escenarios

| # | Escenario | Ejercita |
|---|---|---|
| [00](Guia-Practica-GitFlow.md#2-escenario-00--preparación) | Preparación | Repositorio, protección de rama, pipeline, `CODEOWNERS` |
| [01](Guia-Practica-GitFlow.md#3-escenario-01--funcionalidad-nueva-e-01) | Funcionalidad nueva | E-01: rama corta, pull request, revisión, squash merge |
| [02](Guia-Practica-GitFlow.md#4-escenario-02--defecto-con-release-abierta-e-02) | Defecto con release abierta | E-02: prueba que falla primero, cherry-pick, nueva candidata |
| [03](Guia-Practica-GitFlow.md#5-escenario-03--corte-de-release-e-03-y-e-04) | Corte de release y liberación | E-03 y E-04: corte retroactivo, candidata, criterios de admisión, autorización, tag de versión final y promoción |
| [04](Guia-Practica-GitFlow.md#6-escenario-04--pull-request-que-rompe-la-regresión-e-08) | Pull request que rompe la regresión | E-08: el control que motivó esta guía |
| [05](Guia-Practica-GitFlow.md#7-escenario-05--emergencia-en-producción-e-05) | Emergencia en producción | E-05: hotfix desde el tag y retorno obligatorio |
| [06](Guia-Practica-GitFlow.md#8-escenario-06--versión-de-demostración-e-06) | Versión de demostración | E-06: artefacto identificable y desechable |
| [07](Guia-Practica-GitFlow.md#9-escenario-07--cierre-y-auditoría) | Cierre y auditoría | Convergencia, higiene de ramas, retrospectiva |

## Qué hace falta

- Acceso de escritura al repositorio de práctica y permiso para configurar protección de rama.
- El runner autoalojado `i7infra-dev` disponible, o un runner alojado sustituyendo el `runs-on:` de
  los workflows.
- Docker en la máquina de cada integrante, para correr las pruebas sin instalar .NET ni Node.

Una advertencia sobre el tiempo: los escenarios 01 a 05 llevan una jornada de trabajo si se hacen
completos y con las esperas reales de revisión. Comprimirlos en dos horas es posible, pero se pierde
justamente lo que se quería practicar.
