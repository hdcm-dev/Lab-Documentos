---
doc_id: MARCO-ACTORES
doc_type: marco-de-referencia
title: Actores del dominio
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-CONVENCIONES, MAPA-CONCEPTUAL]
---

# Actores del dominio — `MARCO-ACTORES`

## Resumen ejecutivo

Un informe de solución tiene siempre dos actores que no pueden confundirse: quien lo escribe y quien lo va a usar para decidir algo. La mayoría de los informes malos nacen de escribir para el primero —el autor documenta lo que le costó construir— en lugar de para el segundo —el lector necesita entender para decidir—. Este documento nombra los roles que intervienen alrededor del informe, con su alcance y lo que cada uno decide, para que el redactor sepa a quién le está hablando en cada sección.

Los actores se referencian por su identificador (`ACT-01` … `ACT-08`) en el resto de la guía, sobre todo en la columna «actor que conduce» de las tablas por escenario. No son cargos de un organigrama: son papeles. Una misma persona suele encarnar varios —en un equipo chico, el arquitecto escribe, despliega y presenta—, y un mismo papel puede repartirse entre varias personas.

---

## Los ocho actores

### `ACT-01` — Arquitecto de la solución

El autor principal del informe y el responsable de que la descripción represente el sistema real. Decide la estructura del documento, qué vistas se incluyen y con qué profundidad, y arbitra entre lo que sería lindo contar y lo que el lector necesita. En `ESC-1` propone la arquitectura; en `ESC-2` la describe tal como quedó; en `ESC-3` justifica el cambio. Es quien más cerca está de cometer el error central: escribir para lucir la solución en lugar de para explicarla.

Se corresponde con el arquitecto de software de la [guía hermana](../../Documentacion-Tecnica/00-Marco-de-Referencia/Actores.md) (`ACT-03` allí), que es el dueño del SAD, el HLD y los ADR. Aquí su producto no es cada uno de esos documentos sino el informe que los sintetiza.

### `ACT-02` — Desarrollador o líder técnico

Aporta el detalle que el arquitecto no tiene fresco: cómo se comunican realmente dos componentes, qué atajo se tomó bajo presión, qué biblioteca resolvió un problema. Es la fuente que evita que el informe describa el diseño en lugar del sistema. En `ESC-2` su aporte es decisivo, porque es quien sabe dónde la realidad se apartó del plano. No suele escribir el informe, pero lo revisa buscando lo que no coincide con lo que él construyó.

### `ACT-03` — Solicitante técnico

Quien pidió el informe y va a leerlo para decidir: el arquitecto del cliente que evalúa integrarse, el evaluador de una due diligence, el responsable técnico de un organismo que recibe el sistema. Es el destinatario por defecto del pedido que abre esta guía. Tiene criterio técnico pero no conoce el sistema, y esa combinación define el nivel del informe: puede seguir un diagrama de despliegue y una discusión de trade-offs, pero no comparte el contexto que el autor da por sabido. Escribir para `ACT-03` es el caso de referencia de toda la guía.

### `ACT-04` — Responsable de despliegue y operación

DevOps, SRE o administrador de sistemas: consume la vista de despliegue y la operativa, y a menudo las aporta. Le importa dónde corre cada cosa, qué hay que instalar, qué se rompe cuando un nodo cae y cómo se recupera. En `CTX-3` es un lector central, porque la instalación por terminal y la operación degradada son su territorio. Se corresponde con el DevOps/SRE de la guía hermana (`ACT-06` allí), dueño de la Deployment Guide, la Operations Guide y el Runbook, que esta guía referencia en lugar de reescribir.

### `ACT-05` — Analista de requisitos o product owner

La fuente de los requisitos funcionales y no funcionales, y quien puede confirmar si la solución efectivamente los resuelve. El informe cruza arquitectura contra requisitos, y ese cruce solo es honesto si los requisitos vienen de quien los definió, no inventados por el autor para que la arquitectura calce. En `ESC-4` es quien tiene el criterio para decir si lo entregado corresponde a lo pedido.

### `ACT-06` — Patrocinador o decisor

Lee el informe para decidir sobre presupuesto, aprobación o continuidad, y no siempre tiene profundidad técnica. No lee el documento entero: lee el resumen ejecutivo y las conclusiones, y confía en que el resto lo respalda. Su existencia es la razón por la que el informe abre con un resumen que se entiende solo y por la que las decisiones y sus costos se enuncian sin jerga. Un informe que obliga al decisor a leer treinta páginas para saber si aprobar falló en servirle.

### `ACT-07` — Redactor técnico

Cuida la forma: consistencia de vocabulario, estructura, legibilidad, que los diagramas digan lo que el texto dice. No aporta contenido técnico pero evita que el informe se lea como escrito por tres personas distintas —que suele ser el caso—. En equipos sin este rol, la función recae en `ACT-01`, y se nota cuando no se ejerce. Se corresponde con el technical writer de la guía hermana.

### `ACT-08` — Auditor o responsable de calidad

Evalúa que la solución descrita resuelve los requisitos y que el informe lo demuestra, no solo lo afirma. En `ESC-4` es el actor que conduce: recorre la trazabilidad entre requisitos y mecanismos, verifica lo verificable y registra lo que queda como afirmación sin respaldo. Su pregunta permanente —«¿cómo sé que esto es así?»— es la que el redactor debería anticiparse a responder en cada sección.

---

## Matriz de responsabilidad sobre el informe

Quién produce, consume, aporta o aprueba cada parte del informe. `P` produce, `C` consume, `A` aporta contenido, `V` aprueba o valida.

| Sección del informe | `ACT-01` Arq. | `ACT-02` Dev | `ACT-03` Solicit. | `ACT-04` Despl. | `ACT-05` Req. | `ACT-06` Decisor | `ACT-08` Audit. |
|---|---|---|---|---|---|---|---|
| Resumen ejecutivo | P | — | C | — | — | C | C |
| Arquitectura | P | A | C | — | — | — | V |
| Despliegue | P | A | C | P·C | — | — | V |
| Requisitos funcionales | P | A | C | — | A·V | — | V |
| Requisitos no funcionales | P | A | C | A | A·V | C | V |
| Decisiones y trade-offs | P | A | C | A | — | C | V |

El redactor técnico (`ACT-07`) opera sobre todas las filas mejorando la forma, por eso no tiene columna propia. La matriz deja ver un patrón: `ACT-01` produce casi todo, pero casi nada sin un `A` de otro. Un informe donde la columna de aportes está vacía es un informe que el arquitecto escribió de memoria, y de memoria se describe el diseño, no el sistema.

---

## El error de actor más común

Escribir para uno mismo. El autor conoce el sistema tan bien que olvida cuánto contexto le falta al lector, y produce un informe que él entiende perfectamente y `ACT-03` no. Los síntomas son reconocibles: siglas sin expandir, componentes nombrados por su nombre interno de repositorio, decisiones presentadas como obvias porque para el autor lo son. El antídoto es leer cada sección preguntándose qué sabe `ACT-03` que le permita seguirla, y ese ejercicio pertenece al [criterio de redacción](../50-Redaccion/Criterio-de-Redaccion.md).

El error simétrico, menos frecuente pero más costoso ante un decisor: escribir para `ACT-06` todo el informe, diluyendo el contenido técnico hasta que `ACT-03` no encuentra nada que evaluar. La solución no es elegir una audiencia sino **estratificar**: resumen ejecutivo para el decisor, cuerpo para el solicitante técnico, anexos para el responsable de despliegue. La [estructura del documento](../50-Redaccion/Estructura-del-Documento.md) está pensada para eso.

---

## Preguntas guía

- ¿Para quién estoy escribiendo esta sección: el decisor, el solicitante técnico o el responsable de despliegue?
- ¿El resumen ejecutivo se entiende sin leer el resto? ¿Lo probé con alguien que no conoce el sistema?
- ¿Cada requisito que cruzo con la arquitectura viene de `ACT-05`, o lo inventé para que calzara?
- ¿La columna de aportes de mi informe está vacía? ¿Lo escribí de memoria?
- ¿Un auditor podría preguntar «¿cómo sé que esto es así?» y encontrar la respuesta en el texto?

---

## Anexo — Ficha de audiencia

Se completa antes de redactar y determina el nivel de cada sección.

```yaml
autor_principal: ACT-01
aportes:
  - actor: ACT-??
    seccion: ""
    aporta: ""
destinatarios:
  - actor: ACT-??
    lee: [resumen | cuerpo | anexos]
    decide: ""
    conoce_el_sistema: si | no | parcial
nivel_por_seccion:
  resumen_ejecutivo: no_tecnico
  cuerpo: tecnico
  anexos: tecnico_detallado
validacion:
  requisitos_confirmados_por: ACT-05
  trazabilidad_revisada_por: ACT-08
```
