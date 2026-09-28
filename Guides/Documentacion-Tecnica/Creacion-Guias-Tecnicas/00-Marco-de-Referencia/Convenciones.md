---
doc_id: MARCO-CONVENCIONES
doc_type: marco-de-referencia
title: Convenciones de la guía
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Documentación técnica
last_review: 2026-07-18
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES]
---

# Convenciones de la guía

## Resumen ejecutivo

Todos los documentos de esta guía comparten estructura, vocabulario e identificadores para que el lector aprenda una sola forma de leer y para que un agente pueda recorrerlos por parseo. Este documento fija esas convenciones. Es la referencia interna de la guía: no enseña documentación técnica, enseña cómo está escrita esta guía en particular.

---

## Identificadores

Los enlaces entre documentos apuntan al identificador, no a la ruta: los archivos se mueven, los IDs no.

| Prefijo | Aplica a | Ejemplo |
|---------|----------|---------|
| `ESC-` | Escenarios del marco | `ESC-2` migración |
| `CTX-` | Contextos del marco | `CTX-1` web |
| `ACT-` | Actores del marco | `ACT-03` arquitecto |
| `DOC-` | Documentos temáticos por tipo de documentación | `DOC-SAD` |
| `FAM-` | Familias documentales | `FAM-ARQ` |
| `MET-` | Métodos ágiles | `MET-SCRUM` |
| `ARQ-` | Modelos de arquitectura | `ARQ-HEX` |

Dentro de los ejemplos que la guía usa para ilustrar, se emplean los prefijos habituales de la industria: `RF-` requisito funcional, `RNF-` requisito no funcional, `RN-` regla de negocio, `CU-` caso de uso, `ADR-` decisión de arquitectura, `TC-` caso de prueba, `RSK-` riesgo.

---

## Frontmatter obligatorio

Todo documento abre con este bloque YAML. Los campos no aplicables se omiten; no se dejan vacíos.

```yaml
---
doc_id: DOC-SAD
doc_type: tema | marco-de-referencia | familia | mapa | anexo | indice
title: Software Architecture Document
status: vigente | borrador | obsoleto
origin: human | ia-assisted | ia-generated
confidence: alta | media | baja      # obligatorio si origin != human
owner: <actor responsable>
last_review: AAAA-MM-DD
audience: [humano, agente]
traces: [IDs de documentos relacionados]
---
```

`origin` y `confidence` no son decorativos: son el mecanismo por el cual un lector distingue lo verificado de lo inferido, y la condición bajo la cual un agente de IA puede participar de la producción documental sin degradar la confiabilidad del conjunto.

---

## Estructura de un documento temático

La fija el Profile `Study-Guide-Documentation` y se respeta sin variaciones:

1. **Resumen ejecutivo** — qué es, para qué sirve, a quién le sirve. Prosa, no viñetas.
2. **Definición** — qué es, qué problema resuelve, qué **no** es y con qué se lo confunde.
3. **Aplicación por escenario** — las cuatro entradas `ESC-1` a `ESC-4`, y qué cambia según `CTX-1`, `CTX-2` y `CTX-3`.
4. **Ejemplos concretos** — casos con datos, no descripciones abstractas.
5. **Preguntas guía** — las que hay que poder responder para producir o evaluar el artefacto.
6. **Criterios de calidad** — cómo se distingue una versión buena de una pobre, y los antipatrones más comunes.
7. **Anexo** — plantilla comentada, con la pregunta que guía cada campo.

Cuando un artefacto no aplica en un escenario, la fila se conserva y se explica por qué no aplica. Omitirla deja al lector sin saber si es que no aplica o si es que nadie lo pensó.

---

## Vocabulario y ejemplos

Las tecnologías de referencia para los ejemplos son .NET y C#, Blazor con render mode *interactive server*, ASP.NET MVC y .NET MAUI con patrón MVVM. Se usan como vocabulario ilustrativo; la guía trata sobre documentación, no sobre esas tecnologías.

El dominio recurrente de los ejemplos es un **sistema de reserva de salas**, elegido por ser lo bastante simple para entenderse en un párrafo y lo bastante rico para exhibir reglas de negocio, concurrencia, integración y roles. Reutilizarlo entre documentos permite comparar cómo el mismo problema se describe desde artefactos distintos.

Los datos de los ejemplos son sintéticos y realistas. Cuando un ejemplo sea ilustrativo y no provenga de un sistema real, se marca como tal.

---

## Trazabilidad y fuentes

Las referencias a estándares se citan por su designación exacta —ISO/IEC/IEEE 29148:2018, ISO/IEC/IEEE 42010, ISO/IEC 25010, arc42, C4 Model, RFC 2119, Semantic Versioning 2.0.0, Keep a Changelog, OpenAPI 3.1, WCAG 2.2, OWASP ASVS, STRIDE, Scrum Guide 2020, entre otros— y solo cuando lo que se les atribuye es verificable. Lo que es criterio propio de esta guía se declara como tal.

No se anexan extractos extensos de normas sujetas a licencia restrictiva; se cita la designación, la sección y la idea, y se remite a la fuente.

---

## Estilo

Prosa técnica y formal en las zonas narrativas: resúmenes, definiciones, análisis y racional. Las listas se reservan para enumeraciones reales —pasos, campos, opciones—; una relación de causa y efecto se narra en un párrafo. Las tablas resumen y comparan, sin celdas de texto largo. Los diagramas van en Mermaid.

Ninguna sección abre reformulando su propio título. Los cierres aportan algo o no existen.

---

## No duplicar

Un concepto se desarrolla en un solo documento; el resto lo referencia por enlace relativo. Si al escribir aparece la necesidad de explicar algo que ya está explicado en otro lado, se enlaza. Si no está explicado en ningún lado y no corresponde al documento en curso, se anota como hueco para el cierre de la guía.
