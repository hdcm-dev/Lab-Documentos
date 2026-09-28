---
doc_id: MARCO-CONVENCIONES
doc_type: marco-de-referencia
title: Convenciones de la guía
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Convenciones de la guía — `MARCO-CONVENCIONES`

## Resumen ejecutivo

Todos los documentos de esta guía comparten estructura, vocabulario e identificadores, para que el lector aprenda una sola forma de leer y para que un agente pueda recorrerlos por parseo. Este documento fija esas convenciones. No enseña a escribir informes de solución: enseña cómo está escrita esta guía en particular, y cómo distingue lo que un estándar dice de lo que es criterio propio.

Conviene aclarar de entrada una distinción que atraviesa el material. La guía trata sobre cómo redactar un documento —el informe de solución— y a la vez es ella misma un conjunto de documentos con sus propias convenciones. Cuando un documento dice «el informe abre con un resumen ejecutivo» habla del informe que el lector va a escribir; las reglas de abajo describen cómo está escrita esta guía.

---

## Identificadores

Los enlaces entre documentos citan el identificador además de la ruta: los archivos se mueven, los IDs no.

| Prefijo | Aplica a | Ejemplo |
|---------|----------|---------|
| `ESC-` | Escenarios del marco | `ESC-2` solución construida |
| `CTX-` | Contextos arquitectónicos del marco | `CTX-3` borde distribuido |
| `ACT-` | Actores del marco | `ACT-03` solicitante técnico |
| `FAM-` | Familias temáticas | `FAM-DESP` despliegue |
| `TEM-` | Documentos temáticos | `TEM-RNF` requisitos no funcionales |
| `ANEXO-` | Anexos | `ANEXO-PLANTILLA` |

Las fuentes se citan por el identificador que les asigna [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md), no por su URL: `N-xx` para lo normativo, `G-xx` para guías de organización y marcos con autor, `O-xx` para obras de referencia, `F-xx` para convención de facto y `P-xx` para evidencia de plataformas reales.

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
owner: <responsable>
last_review: AAAA-MM-DD
audience: [humano, agente]
traces: [IDs de documentos relacionados]
---
```

`origin` y `confidence` no son decorativos: son el mecanismo por el cual un lector distingue lo verificado de lo inferido. Todo el cuerpo de esta guía es `ia-assisted`, y las afirmaciones normativas se anclan a `ANEXO-REFERENCIAS`, cuyas fuentes se verificaron contra el documento primario en la fecha que cada fila indica.

---

## Estructura de un documento temático

La fija el Profile `Study-Guide-Documentation` y se respeta sin variaciones:

1. **Resumen ejecutivo** — qué es, para qué sirve, a quién le sirve. Prosa, no viñetas.
2. **Definición** — qué es, qué problema resuelve, qué **no** es y con qué se lo confunde.
3. **Aplicación por escenario** — las cuatro entradas `ESC-1` a `ESC-4`, y qué cambia según `CTX-1` a `CTX-4`.
4. **Ejemplos concretos** — fragmentos de informe reales sobre el dominio de audiencias, no descripciones abstractas.
5. **Preguntas guía** — las que el redactor debe poder responder para producir o evaluar esa parte del informe.
6. **Criterios de calidad** — cómo se distingue una sección buena de una pobre, con los antipatrones más frecuentes.
7. **Anexo** — plantilla comentada o lista de verificación, cuando el tema lo admite.

Cuando un tema no aplica en un escenario, la fila se conserva y se explica por qué no aplica. Omitirla deja al lector sin saber si es que no aplica o si es que nadie lo pensó.

### Variante de redacción

Los documentos de la familia [`FAM-RED`](../50-Redaccion/README.md) tratan sobre cómo escribir, no sobre qué describir, y por eso su sección de ejemplos toma otra forma: en lugar de fragmentos de arquitectura, ofrecen **frases de referencia** —pares de «así no / así sí»— que forman criterio de redacción. Es la única desviación admitida respecto de la estructura anterior.

---

## Los cuatro niveles de autoridad

Es la convención más importante de la guía. En la documentación de arquitectura circula mucha prescripción sin fuente: se enuncian como reglas universales cosas que son la plantilla de una empresa, o el gusto de un autor, o la costumbre de un equipo. Toda afirmación normativa de esta guía se clasifica en uno de cuatro niveles, y el nivel se hace explícito en el texto.

| Nivel | Qué significa | Cómo se marca |
|-------|---------------|---------------|
| **Normativo** | Estándar publicado: norma ISO/IEC/IEEE, RFC del IETF, especificación de la OMG, documentación oficial de Microsoft | Se cita el identificador `N-xx` y la designación exacta |
| **Marco o guía de organización** | Plantilla o marco de un autor u organización —arc42, C4, TOGAF, el SAD de RUP— que vale para quien lo adopta, no universalmente | Se nombra el marco: «arc42 propone…», «el modelo C4…» |
| **Convención de facto** | Práctica dominante sin especificación que la imponga | Se declara como convención y se indica la evidencia que la sostiene |
| **Criterio propio** | Recomendación de esta guía, discutible | Se declara con la fórmula «esta guía recomienda» |

La distinción entre los dos niveles del medio es la que más se pierde. **arc42 no es un estándar**: es una plantilla excelente, con licencia Creative Commons, creada por dos autores; adoptarla es una buena decisión, pero «lo dice arc42» no tiene el peso de «lo dice ISO/IEC/IEEE 42010». La norma fija *qué requisitos* debe cumplir una descripción de arquitectura; arc42 y C4 ofrecen *una forma concreta* de cumplirlos, entre otras posibles. Confundir una plantilla popular con una norma es el error de citación más frecuente del tema.

Un caso merece advertencia explícita: **«solución» y «arquitectura de despliegue» no tienen una definición normativa única**. TOGAF define «arquitectura de solución», y esta guía cita esa definición cuando corresponde, pero fuera de ese marco el término es de uso común sin fuente única. La guía lo usa en su acepción práctica —la solución es el sistema propuesto o construido, con su arquitectura, su despliegue y su forma de resolver los requisitos— y lo declara.

---

## Vocabulario y ejemplos

Las tecnologías de referencia son .NET y C# con ASP.NET Core, Blazor en render *interactive server* para el frontend, aplicaciones de escritorio en WPF, WinForms o .NET MAUI, Worker Services para los procesos en segundo plano, y PostgreSQL vía Npgsql para la persistencia. La versión de referencia para producción es **.NET 10 (LTS)**. La versión concreta se indica donde el comportamiento depende de ella; la mayoría del criterio de redacción es independiente de la versión.

El dominio recurrente es el **sistema de gestión de audiencias** que [`MARCO-CONTEXTOS`](Contextos.md#el-sistema-de-ejemplo--gestión-de-audiencias) describe: un sistema distribuido en el borde (`CTX-3`) con programa de escritorio y servicio en segundo plano por terminal, backend central, frontend administrativo y servidor de archivos. Se eligió porque exhibe los tres comportamientos que un ejemplo simple no muestra —operación con el centro caído, recuperación ante caída del escritorio, subida diferida en segundo plano— y porque el pedido que origina la guía proviene de un sistema de esa forma.

Los ejemplos de ese dominio son **sintéticos** y se declaran como tales: los fragmentos de informe, los diagramas y las cifras ilustran cómo se redacta, no reproducen un documento real. Cuando la guía necesita respaldar una afirmación sobre estándares o sobre el comportamiento de .NET, recurre a las fuentes verificadas de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md), con la fecha en que se consultó cada una.

### Formato de los diagramas

Los diagramas van en **Mermaid**, preferido sobre ASCII o imágenes binarias, porque es diffeable y regenerable. Las vistas de arquitectura y despliegue usan `flowchart` o `graph`; los flujos de proceso, `sequenceDiagram`; las líneas de tiempo, `timeline`. La guía trata el modelo C4 como forma de organizar esos diagramas por nivel de zoom, no como una sintaxis: el soporte C4 nativo de Mermaid es experimental, de modo que los diagramas C4 se dibujan con `flowchart` y agrupación por `subgraph`, no con la sintaxis `C4Context`.

---

## Idioma

La prosa de la guía es española. Los términos técnicos con designación establecida en inglés se mantienen en inglés —*deployment*, *stakeholder*, *trade-off*, *runtime*, *rollback*— porque traducirlos entorpece la búsqueda y desalinea al lector respecto de la documentación que va a consultar. Se registran en [`ANEXO-GLOSARIO`](../99-Anexos/Glosario.md) con su equivalente español cuando existe.

---

## Trazabilidad y fuentes

Toda afirmación normativa debe poder rastrearse hasta una fila de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md). Si no puede, o falta la fuente o es criterio propio mal etiquetado.

Las normas se citan por designación exacta y año: «ISO/IEC 25010:2023», no «la norma de calidad», porque las ediciones se reemplazan y buena parte del material sobre calidad sigue citando la edición 2011 con sus ocho características, cuando la vigente tiene nueve. Citar el año permite detectar esa desactualización.

No se transcriben extractos extensos de material con licencia restrictiva —las normas ISO son de pago—: se cita la designación, la cláusula y la idea, nunca el texto normativo completo.

---

## No duplicar, y la relación con las guías hermanas

Un concepto se desarrolla en un solo documento; el resto lo referencia por enlace relativo. La regla tiene una consecuencia estructural fuerte en esta guía, por su relación con las hermanas de la serie.

La [guía de documentación técnica](../../Documentacion-Tecnica/README.md) ya trata **cada tipo de documento por separado**: el [SAD](../../Documentacion-Tecnica/30-Arquitectura/SAD.md), el [SRS](../../Documentacion-Tecnica/20-Analisis/SRS.md), la [Deployment Guide](../../Documentacion-Tecnica/50-Operativa/Deployment-Guide.md), los [ADR](../../Documentacion-Tecnica/30-Arquitectura/ADR.md). Esta guía **no los reescribe**. Su objeto es distinto: cómo se **compone** un único informe transversal que cruza arquitectura, despliegue y requisitos para un lector que quiere el enfoque general, y con qué criterio de redacción. Cuando un tema pertenece a la guía hermana, esta lo enlaza y agrega solo lo que cambia al integrarlo en un informe único. La frontera es deliberada y se declara en cada documento que la roza.

El [catálogo de tipos de documentación](#situación-en-el-catálogo-de-familias) —del que la guía toma la tabla y la agrupación en siete familias— sitúa este informe: se nutre de las familias de análisis (requisitos), arquitectura y operativa (despliegue), y las sintetiza. Ese cruce se detalla en [`TEM-QUE-ES`](../10-Naturaleza-del-Informe/Que-es-y-que-no-es.md) y en el [mapa conceptual](../01-Mapa-Conceptual/Mapa-Conceptual.md).

### Situación en el catálogo de familias

El catálogo de referencia agrupa la documentación técnica en siete familias, según la pregunta que cada una responde:

| Familia | Pregunta | Aporta a este informe |
|---|---|---|
| 1 · Visión | ¿Qué queremos construir? | Contexto y alcance |
| 2 · Análisis | ¿Qué debe hacer el sistema? | **Requisitos funcionales y no funcionales** |
| 3 · Arquitectura | ¿Cómo estará organizado? | **La vista de arquitectura** |
| 4 · Diseño | ¿Cómo se implementa cada componente? | Detalle referenciado, no incluido |
| 5 · Operativa | ¿Cómo se instala, mantiene y opera? | **La vista de despliegue** |
| 6 · Desarrollo | ¿Cómo trabajamos sobre el proyecto? | Fuera de alcance |
| 7 · Usuarios | ¿Cómo se usa el sistema? | Fuera de alcance |

El informe de solución vive en la intersección de las familias 2, 3 y 5. No es ninguno de sus documentos: es una síntesis orientada a un lector que pregunta «explíquenme el enfoque general», y toma de cada familia lo que sirve a esa pregunta. El catálogo incluye además un cierre sobre un caso particular ajeno a este marco, que esta guía no utiliza.

---

## Estilo

Prosa técnica y formal en las zonas narrativas. Las listas se reservan para enumeraciones reales —pasos, campos, opciones—; una relación de causa y efecto se narra en un párrafo. Las tablas resumen y comparan, sin celdas de texto largo. Los diagramas van en Mermaid.

Ninguna sección abre reformulando su propio título. Los cierres aportan algo o no existen. La voz es uniforme en toda la guía: se percibe una sola autoría, según [`Rule-Narrative-Voice`](../../../../IA.Prompts/PromptFramework/Rules/Rule-Narrative-Voice.md).
