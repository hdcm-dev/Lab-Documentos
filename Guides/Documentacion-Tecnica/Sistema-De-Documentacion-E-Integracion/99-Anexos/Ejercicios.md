---
doc_id: ANX-SDEI-EJERCICIOS
doc_type: anexo
title: Anexo — ejercicios de transferencia
description: Ejercicios EJR-SDEI-nn que comprueban los objetivos de aprendizaje del README, con el caso resuelto antes y las claves al final.
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-27
audience: [humano, estudiante, docente]
traces: [IDX-SDEI, MARCO-SDEI, MAPA-SDEI, ANX-SDEI-ESQUEMA, ANX-SDEI-DECIDIR]
---

# Anexo — ejercicios

Cada ejercicio comprueba un objetivo de aprendizaje del [README](../README.md#2-objetivos-de-aprendizaje). Siguen el
patrón «ejemplo trabajado, después ejercicio» (`PE-02`): el caso resuelto está en el documento que se indica, y el
ejercicio cambia el sistema y mantiene la estructura. Las claves están al final, para mirarlas después de intentar.

## Índice

1. [Cómo se usan](#1-cómo-se-usan)
2. [Ejercicios](#2-ejercicios)
3. [Claves](#3-claves)

## 1. Cómo se usan

| Ejercicio | Objetivo | Ejemplo trabajado previo | Tiempo estimado |
| --- | --- | --- | --- |
| `EJR-SDEI-01` | `OBJ-SDEI-01` | [Marco, §6](../00-Marco.md#6-un-caso-que-recorre-el-marco) | 15 min |
| `EJR-SDEI-02` | `OBJ-SDEI-02` | [Esquema, §7](Esquema-Frontmatter-Y-Prefijos.md#7-un-ejemplo-completo-y-sus-errores) | 15 min |
| `EJR-SDEI-03` | `OBJ-SDEI-03` | [Cómo decidir, §5](Como-Decidir.md#5-una-matriz-trabajada) | 30 min |
| `EJR-SDEI-04` | `OBJ-SDEI-04` | [Mapa, §2 y §3](../01-Mapa.md#2-por-escenario) | 10 min |
| `EJR-SDEI-05` | `OBJ-SDEI-05` | [Esquema, §6](Esquema-Frontmatter-Y-Prefijos.md#6-registro-de-requerimientos-documentales) | 15 min |

Los tiempos son **[criterio propio]**; sirven para planificar una clase, no para evaluar.

## 2. Ejercicios

### EJR-SDEI-01 — Ubicar un sistema

Una cátedra arma una aplicación web donde los alumnos entregan trabajos y los docentes los corrigen; cada rol ve
pantallas distintas. El equipo usa un marco SDD con casos de uso numerados. No hay sitio de documentación publicado.

1. ¿En qué escenario del Marco está?
2. ¿Qué dos contextos fijan las respuestas en este caso, y en qué rama de cada uno cae?
3. Nombrá tres clases de pieza que se activan y una que queda latente, con el motivo.

### EJR-SDEI-02 — Escribir un frontmatter

Escribí el frontmatter de una página nueva del cuerpo: un anexo con plantillas de registro de requerimientos, que
redactó un agente y que todavía no revisó nadie. Después respondé: si borrás la línea `confidence`, ¿qué grupo de la
compuerta da rojo y con qué regla?

### EJR-SDEI-03 — Decidir con la escala

Un equipo duda entre dos formas de publicar la ayuda al usuario de una app sin roles:

- **X**: las páginas de ayuda en Markdown dentro del repositorio del código, servidas por la propia app.
- **Y**: un sitio aparte, generado con un generador de sitios estáticos, en un repositorio dedicado.

1. Puntuá X e Y en K1 a K7 con la escala anclada, citando el observable de cada celda.
2. Calculá los totales con los pesos del anexo.
3. ¿Alguna cae por un filtro? ¿La decisión es firme?

### EJR-SDEI-04 — Encontrar qué aplicar

En MovilidadUrbana se renombró el método que valida el largo del comentario. Una página de ayuda de la demostración
traza a la ficha de esa regla, y la demostración fija el código en un `source.lock`. Usando sólo el Mapa:

1. ¿Por qué tabla entrás y qué fila aplicás?
2. ¿Qué control detecta el cambio, y bloquea o avisa? ¿Por qué?
3. ¿Quién actualiza la ficha y quién decide si el número de la regla sigue siendo el correcto?

### EJR-SDEI-05 — Redactar un requerimiento documental

Un docente pide que la guía explique cómo se corre la compuerta en local. Redactá la fila del registro de
requerimientos documentales: audiencia, tarea, pieza, criterio Dado/Cuando/Entonces y decisión. ¿Quién la acepta?

## 3. Claves

**EJR-SDEI-01.** Escenario `ESC-SDEI-3` (panel con roles). Contextos: `CTX-SDEI-2` en la rama «con SDD» (los
requerimientos documentales viven con la especificación y la ayuda traza a los casos de uso) y `CTX-SDEI-1` en la rama
«sin sitio». Se activan, entre otras, CL-01 (especificación), CL-06 (ayuda al usuario final) y CL-08 (índice para
agentes); CL-14 (canales derivados) queda latente porque no hay sitio.

**EJR-SDEI-02.** Una respuesta válida [Fragmento ilustrativo: una de varias formas correctas]:

```yaml
---
doc_id: ANX-SDEI-PLANTILLA-RRD
doc_type: anexo
title: Anexo — plantillas del registro de requerimientos documentales
description: Plantillas comentadas para escribir filas del registro de requerimientos documentales con criterio Dado/Cuando/Entonces.
status: borrador
origin: ia-generated
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-27
audience: [humano, agente, analista]
traces: [ANX-SDEI-ESQUEMA]
---
```

Sin `confidence`, da rojo el grupo PR-2 (esquema) con la regla `esquema-cuerpo`, porque `origin` no es `human`.

**EJR-SDEI-03.** No hay una única respuesta: se evalúa que cada celda cite su observable. Una lectura posible:
X agrega poco (K1 alto), sirve a la persona usuaria y al agente desde la misma fuente (K5 alto) y la deriva se ve en
el pull request del código (K7 = 5); Y suma una herramienta y un repositorio (K1 = 2) y un ancla entre repositorios
(K7 = 4, aviso programado). Si X gana por 0,30 o más con pesos iguales, la decisión es firme. Ninguna cae por un filtro
si ambas corren en el runner del caso.

**EJR-SDEI-04.** Se entra por §2, fila `ESC-SDEI-2`, y se ajusta por §3, fila `CTX-SDEI-3` (la pieza vive en otro
repositorio que el código). Contra el SHA fijado en `source.lock`, `traces-resolve` sigue en verde: el cambio no
entró ahí. El carril programado, que compara contra la rama principal del código, **avisa**, porque la deriva no la
introdujo el pull request de la documentación (no cumple la segunda condición de la regla de bloqueo). Un agente
propone actualizar la ficha y el `source.lock`; si el número de la regla es correcto lo decide una persona (analista).

**EJR-SDEI-05.** Una fila válida:

| ID | Audiencia | Tarea | Pieza | Criterio | Decisión |
| --- | --- | --- | --- | --- | --- |
| RD-07 | estudiante | correr la compuerta en local antes de abrir un pull request | CL-10, tema `TEM-06` | Dado un clon del repositorio, cuando el estudiante sigue la sección, entonces `scripts/ci-local.sh` termina con el mismo veredicto que la CI | pendiente |

La acepta, rechaza o difiere una persona (la analista o el PO): es la fase de evaluación del ciclo, de clase H.

Volver al [Mapa](../01-Mapa.md#6-anexos).
