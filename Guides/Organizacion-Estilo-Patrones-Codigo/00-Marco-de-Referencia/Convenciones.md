---
doc_id: MARCO-CONVENCIONES
doc_type: marco-de-referencia
title: Convenciones de la guía
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-19
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Convenciones de la guía — `MARCO-CONVENCIONES`

## Resumen ejecutivo

Todos los documentos comparten estructura, vocabulario e identificadores, para que el lector aprenda una sola forma de leer y para que un agente pueda recorrerlos por parseo. Este documento fija esas convenciones. No enseña sobre organización de código: enseña cómo está escrita esta guía en particular.

Hay una tensión que conviene declarar de entrada. La guía trata sobre convenciones de código y a la vez tiene sus propias convenciones documentales; son planos distintos y no deben confundirse. Cuando un documento dice «PascalCase», habla del código que se estudia, no de esta guía.

---

## Identificadores

Los enlaces entre documentos citan el identificador además de la ruta: los archivos se mueven, los IDs no.

| Prefijo | Aplica a | Ejemplo |
|---------|----------|---------|
| `ESC-` | Escenarios del marco | `ESC-1` sistema nuevo |
| `CTX-` | Contextos del marco | `CTX-3` biblioteca reutilizable |
| `ACT-` | Actores del marco | `ACT-01` arquitecto |
| `FAM-` | Familias temáticas | `FAM-NOM` nomenclatura |
| `TEM-` | Documentos temáticos | `TEM-CAPAS` modelos de capas |
| `ANEXO-` | Anexos | `ANEXO-GLOSARIO` |

Dentro de los ejemplos se usan los prefijos habituales de la industria cuando corresponde: `ADR-` decisión de arquitectura, `RN-` regla de negocio, `CU-` caso de uso.

---

## Frontmatter obligatorio

Todo documento abre con este bloque YAML. Los campos no aplicables se omiten; no se dejan vacíos.

```yaml
---
doc_id: TEM-XXX                      # marcador de posición; usar el ID real
doc_type: tema | marco-de-referencia | familia | mapa | anexo | indice
title: <título del documento>
status: vigente | borrador | obsoleto
origin: human | ia-assisted | ia-generated
confidence: alta | media | baja      # obligatorio si origin != human
owner: <actor responsable>
last_review: AAAA-MM-DD
audience: [humano, agente]
traces: [IDs de documentos relacionados]
---
```

`origin` y `confidence` no son decorativos. Son el mecanismo por el cual un lector distingue lo verificado de lo inferido, y la condición bajo la cual un agente puede participar de la producción documental sin degradar la confiabilidad del conjunto.

---

## Estructura de un documento temático

La fija el Profile `Study-Guide-Documentation` y se respeta sin variaciones:

1. **Resumen ejecutivo** — qué es, para qué sirve, a quién le sirve. Prosa, no viñetas.
2. **Definición** — qué es, qué problema resuelve, qué **no** es y con qué se lo confunde.
3. **Aplicación por escenario** — las cuatro entradas `ESC-1` a `ESC-4`, y qué cambia según `CTX-1` a `CTX-4`.
4. **Ejemplos concretos** — código y estructuras reales, no descripciones abstractas.
5. **Preguntas guía** — las que hay que poder responder para decidir o evaluar.
6. **Criterios de calidad** — cómo se distingue una aplicación buena de una pobre, con los antipatrones más frecuentes.
7. **Anexo** — plantilla o lista de verificación, cuando el tema lo admite.

Cuando un tema no aplica en un escenario, la fila se conserva y se explica por qué no aplica. Omitirla deja al lector sin saber si es que no aplica o si es que nadie lo pensó.

### Variante de catálogo

Un documento cuyo cuerpo es una enumeración —el caso de [`TEM-ANTI`](../40-Nomenclatura/Antipatrones-de-Nombrado.md), con sus doce antipatrones— distribuye los ejemplos dentro de cada entrada en lugar de agruparlos en una sección propia. Es la única desviación admitida de la estructura, y existe porque concentrar los ejemplos obligaría a repetir cada antipatrón dos veces. Las otras cinco secciones se mantienen sin cambios.

---

## Los tres niveles de autoridad

Esta es la convención más importante de la guía y la que la distingue de la mayoría del material disponible sobre el tema. Toda afirmación normativa se clasifica en uno de tres niveles, y el nivel se hace explícito en el texto:

| Nivel | Qué significa | Cómo se marca |
|-------|---------------|---------------|
| **Normativo** | Publicado por Microsoft como especificación o guía oficial | Se cita la página de Microsoft Learn o la documentación del SDK |
| **Convención de facto** | Práctica dominante del ecosistema, sin especificación que la imponga | Se declara como convención y se indica su origen |
| **Criterio propio** | Recomendación de esta guía, discutible | Se declara con la fórmula «esta guía recomienda» |

La confusión entre estos niveles es endémica en la literatura sobre .NET. Se presenta como «el estándar de Microsoft» material que es opinión de la comunidad —Clean Architecture es el caso más notorio— y eso impide que el lector evalúe si le conviene. Ver [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md) para la lista completa de fuentes con su nivel.

Un matiz que conviene retener: las páginas de *Framework Design Guidelines* en Microsoft Learn son la reimpresión de la segunda edición del libro (2008) y llevan un aviso de que parte de la información puede estar desactualizada. Siguen siendo normativas para el diseño de bibliotecas, y esta guía las cita como tales, señalando dónde la práctica actual difiere.

---

## Vocabulario y ejemplos

Las tecnologías de referencia son .NET 10 y C#, con ASP.NET Core y Blazor en render *interactive server* para los ejemplos de `CTX-1`.

El dominio recurrente es un **sistema de reserva de salas**, el mismo que usa la guía hermana de documentación técnica. Se eligió por ser lo bastante simple para entenderse en un párrafo y lo bastante rico como para exhibir reglas de negocio, concurrencia e integración. Reutilizarlo permite comparar cómo la misma funcionalidad se organiza bajo modelos distintos.

Los ejemplos de ese dominio son **sintéticos** y se declaran como tales. La guía no toma casos de repositorios privados: un ejemplo particular ilustra a lo sumo una decisión de un equipo, y confundirlo con la práctica del ecosistema es el mismo error de atribución que la guía intenta desarmar.

Cuando hace falta evidencia de lo que el ecosistema efectivamente hace —por oposición a lo que se le atribuye—, la guía recurre a los **repositorios de referencia de Microsoft** registrados en [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md): `dotnet/runtime`, `dotnet/aspnetcore` y `dotnet/efcore`. Son públicos, inspeccionables y lo bastante distintos entre sí como para exhibir dónde hay acuerdo y dónde no.

---

## Trazabilidad y fuentes

Las fuentes se citan por su designación exacta y solo cuando lo que se les atribuye es verificable. Las URL de documentación oficial se registran en [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md) con su fecha de consulta, y los documentos temáticos remiten ahí en lugar de repetirlas.

No se transcriben extractos extensos de material con licencia restrictiva: se cita la obra, el capítulo y la idea.

---

## Estilo

Prosa técnica y formal en las zonas narrativas. Las listas se reservan para enumeraciones reales —pasos, campos, opciones—; una relación de causa y efecto se narra en un párrafo. Las tablas resumen y comparan, sin celdas de texto largo. Los diagramas van en Mermaid.

Ninguna sección abre reformulando su propio título. Los cierres aportan algo o no existen.

---

## No duplicar

Un concepto se desarrolla en un solo documento; el resto lo referencia por enlace relativo. La regla tiene una consecuencia práctica visible en la estructura de la guía: las convenciones de capitalización se explican una vez en [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md) y los demás documentos las citan, aunque eso obligue al lector a saltar entre archivos.
