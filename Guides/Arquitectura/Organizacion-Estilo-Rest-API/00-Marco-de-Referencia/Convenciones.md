---
doc_id: MARCO-CONVENCIONES
doc_type: marco-de-referencia
title: Convenciones de la guía
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización y estilo de REST API en .NET
last_review: 2026-07-20
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Convenciones de la guía — `MARCO-CONVENCIONES`

## Resumen ejecutivo

Todos los documentos de esta guía comparten estructura, vocabulario e identificadores, para que el lector aprenda una sola forma de leer y para que un agente pueda recorrerlos por parseo. Este documento fija esas convenciones. No enseña sobre diseño de APIs: enseña cómo está escrita esta guía en particular.

Conviene declarar de entrada una ambigüedad que atraviesa el material. La guía trata sobre convenciones de nomenclatura y a la vez tiene las suyas propias, documentales; son planos distintos. Cuando un documento dice «`camelCase`» habla del JSON que se diseña, no del nombre de los archivos de esta guía.

---

## Identificadores

Los enlaces entre documentos citan el identificador además de la ruta: los archivos se mueven, los IDs no.

| Prefijo | Aplica a | Ejemplo |
|---------|----------|---------|
| `ESC-` | Escenarios del marco | `ESC-1` API nueva |
| `CTX-` | Contextos del marco | `CTX-2` API interna entre servicios |
| `ACT-` | Actores del marco | `ACT-01` arquitecto de API |
| `FAM-` | Familias temáticas | `FAM-HTTP` semántica HTTP |
| `TEM-` | Documentos temáticos | `TEM-VERS` estrategias de versionado |
| `ANEXO-` | Anexos | `ANEXO-GLOSARIO` |

Las fuentes se citan por el identificador que les asigna [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md), no por su URL: `N-xx` para lo normativo, `F-xx` para convención de facto, `O-xx` para obras de referencia y `P-xx` para la evidencia de plataformas reales.

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
4. **Ejemplos concretos** — peticiones, respuestas y código reales, no descripciones abstractas.
5. **Preguntas guía** — las que hay que poder responder para decidir o evaluar.
6. **Criterios de calidad** — cómo se distingue una aplicación buena de una pobre, con los antipatrones más frecuentes.
7. **Anexo** — plantilla o lista de verificación, cuando el tema lo admite.

Cuando un tema no aplica en un escenario, la fila se conserva y se explica por qué no aplica. Omitirla deja al lector sin saber si es que no aplica o si es que nadie lo pensó.

### Variante de catálogo

Un documento cuyo cuerpo es una enumeración —el caso de [`TEM-STATUS`](../30-Semantica-HTTP/Codigos-de-Estado.md), con su recorrido código por código— distribuye los ejemplos dentro de cada entrada en lugar de agruparlos en una sección propia. Es la única desviación admitida, y existe porque concentrar los ejemplos obligaría a nombrar cada código dos veces.

---

## Los cuatro niveles de autoridad

Es la convención más importante de la guía. En ningún tema técnico circula tanta prescripción sin fuente como en el diseño de APIs REST: buena parte de lo que se enuncia como «regla REST» no aparece en ninguna especificación. Toda afirmación normativa de esta guía se clasifica en uno de cuatro niveles, y el nivel se hace explícito en el texto.

| Nivel | Qué significa | Cómo se marca |
|-------|---------------|---------------|
| **Normativo** | Especificación publicada: RFC del IETF, OpenAPI Specification, documentación oficial de Microsoft | Se cita el identificador `N-xx` y la sección exacta |
| **Guía de organización** | Prescripción de una organización concreta —Microsoft, Google, Zalando— que vale para quien la adopta, no universalmente | Se nombra la organización: «Google AIP-158 prescribe…» |
| **Convención de facto** | Práctica dominante sin especificación que la imponga | Se declara como convención y se indica la evidencia `P-xx` que la sostiene |
| **Criterio propio** | Recomendación de esta guía, discutible | Se declara con la fórmula «esta guía recomienda» |

La distinción entre los dos niveles del medio es la que más se pierde. Que Google prescriba `page_size` y `page_token` no convierte esos nombres en estándar; los convierte en la convención de Google, excelente dentro de su ecosistema y arbitraria fuera de él. Cuando dos guías se contradicen —y se contradicen a menudo— lo que corresponde no es buscar cuál «tiene razón», sino entender qué problema resolvía cada una.

Un caso merece advertencia explícita: **casi nada de lo que se llama «REST» en la industria es REST según Fielding**. La disertación define un estilo arquitectónico con restricciones precisas; el uso corriente designa cualquier API HTTP con URIs y JSON. Esta guía usa el término en su acepción corriente porque es la que el lector va a encontrar, y reserva [`TEM-REST`](../10-Fundamentos-REST/Que-es-REST.md) para explicar la diferencia.

---

## Vocabulario y ejemplos

Las tecnologías de referencia son .NET y C# con ASP.NET Core, y para los consumidores Blazor en render *interactive server*, ASP.NET Core MVC y .NET MAUI con patrón MVVM. La versión concreta del SDK a la que aplica cada afirmación se indica donde importa; la mayoría de las convenciones de diseño son independientes de la versión.

El dominio recurrente es un **sistema de reserva de salas**, el mismo que usan las guías hermanas de [documentación técnica](../../Documentacion-Tecnica/README.md) y de [organización y estilo de código](../../Organizacion-Estilo-Patrones-Codigo/README.md). Reutilizarlo permite comparar cómo el mismo problema se resuelve desde artefactos distintos. Sus entidades son salas, reservas, usuarios y sedes; tiene reglas de negocio no triviales —solapamiento de horarios, capacidad, cancelación con antelación— y por lo tanto exhibe los casos que ninguna API CRUD de ejemplo llega a mostrar.

Los ejemplos de ese dominio son **sintéticos** y se declaran como tales. Cuando hace falta evidencia de lo que la industria efectivamente hace —por oposición a lo que se le atribuye— la guía recurre a **APIs públicas documentadas** de plataformas grandes, registradas en [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md) con la fecha en que se consultó su documentación. Una API pública es evidencia razonable: está publicada, es inspeccionable y su diseño sobrevivió al contacto con consumidores reales.

### Formato de los ejemplos de protocolo

Las peticiones y respuestas se escriben en HTTP crudo, con las cabeceras relevantes y sin las accesorias:

```http
GET /v1/salas/a3f1/reservas?desde=2026-08-01&limite=20 HTTP/1.1
Host: api.ejemplo.com
Accept: application/json
```

```http
HTTP/1.1 200 OK
Content-Type: application/json
ETag: "8f3c1e"

{ "datos": [ … ], "siguiente": "…" }
```

Se muestran las cabeceras que el ejemplo demuestra. Incluir `User-Agent`, `Date` y `Content-Length` en cada bloque agrega ruido sin enseñar nada.

---

## Idioma

La prosa de la guía es española. Los términos técnicos con designación establecida en inglés se mantienen en inglés —*endpoint*, *payload*, *cursor*, *breaking change*— porque traducirlos entorpece la búsqueda y desalinea al lector respecto de la documentación que va a consultar. Se registran en [`ANEXO-GLOSARIO`](../99-Anexos/Glosario.md) con su equivalente español cuando existe.

Los identificadores de los ejemplos —rutas, campos JSON— van en español cuando el ejemplo ilustra el dominio de reserva de salas, y en inglés cuando reproduce una API real. La decisión de en qué idioma nombrar los recursos de una API propia es un tema de diseño, no una convención documental, y se trata en [`TEM-URI`](../20-Diseno-de-Recursos/Nomenclatura-de-URIs.md).

---

## Trazabilidad y fuentes

Toda afirmación normativa debe poder rastrearse hasta una fila de [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md). Si no puede, o falta la fuente o es criterio propio mal etiquetado.

Las especificaciones se citan por designación exacta y sección: «RFC 9110 §9.2.2», no «el estándar HTTP». Importa porque los RFC se obsoletan entre sí y buena parte del material sobre APIs sigue citando documentos reemplazados —RFC 2616 y RFC 7231 para HTTP, RFC 7807 para *problem details*—. Citar el número exacto permite detectar esa desactualización.

No se transcriben extractos extensos de material con licencia restrictiva: se cita la obra, el capítulo y la idea.

---

## Estilo

Prosa técnica y formal en las zonas narrativas. Las listas se reservan para enumeraciones reales —pasos, campos, opciones—; una relación de causa y efecto se narra en un párrafo. Las tablas resumen y comparan, sin celdas de texto largo. Los diagramas van en Mermaid.

Ninguna sección abre reformulando su propio título. Los cierres aportan algo o no existen.

---

## No duplicar

Un concepto se desarrolla en un solo documento; el resto lo referencia por enlace relativo. La regla tiene consecuencias visibles en la estructura: la semántica de cada método HTTP se explica una vez en [`TEM-METODOS`](../30-Semantica-HTTP/Metodos.md) y los documentos de diseño de recursos la citan, aunque eso obligue al lector a saltar entre archivos.

Hay una excepción deliberada respecto de la guía hermana de código. [`TEM-ENDP`](../../Organizacion-Estilo-Patrones-Codigo/60-Patrones-de-Codigo/Patrones-de-Endpoint.md) ya trata Minimal APIs frente a controllers desde la óptica de la organización del código. Esta guía vuelve sobre el tema en [`TEM-MINIMAL`](../80-Implementacion-en-NET/Minimal-APIs-y-Controllers.md) desde la óptica del contrato HTTP que cada opción permite expresar. Son preguntas distintas sobre el mismo mecanismo, y la guía es autocontenida por decisión: se puede leer sin la otra.
