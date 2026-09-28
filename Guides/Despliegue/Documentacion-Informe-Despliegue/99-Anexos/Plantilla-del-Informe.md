---
doc_id: ANEXO-PLANTILLA
doc_type: anexo
title: Plantilla comentada del informe de solución
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [TEM-ESTRUCTURA, TEM-QUE-ES, TEM-COMPONENTES, TEM-TOPOLOGIA, TEM-RF, TEM-RNF, TEM-DECISIONES, MARCO-CONVENCIONES, ANEXO-REFERENCIAS]
---

# Plantilla comentada del informe de solución — `ANEXO-PLANTILLA`

## Resumen ejecutivo

Este anexo es el **modelo formal de documento** que la guía propone: la estructura completa de un informe de solución, sección por sección, con qué va en cada una, qué preguntarse antes de escribirla y una frase de referencia sobre el sistema de audiencias. Es criterio propio de la guía —declarado como tal— construido sobre tres fuentes: los elementos que `N-01` ISO/IEC/IEEE 42010:2022 exige en una descripción de arquitectura, la plantilla de doce secciones de `G-01` arc42, y la organización por vistas de `O-01`/`G-04`. El fundamento de por qué este orden se explica en [`TEM-ESTRUCTURA`](../50-Redaccion/Estructura-del-Documento.md); acá está la plantilla lista para copiar y completar.

No todas las secciones aplican a todo informe. La regla es la misma que rige toda la guía: si una sección no aplica, se conserva el título y se explica por qué no aplica, en una línea. Omitirla deja al lector sin saber si el tema no venía al caso o si nadie lo pensó.

---

## Cómo usar esta plantilla

Se copia la estructura, se completa cada sección respondiendo sus preguntas guía y se borran los comentarios. El nivel de cada sección lo fija el destinatario: el **resumen ejecutivo** se escribe para quien decide (`ACT-06`) y debe entenderse sin leer el resto; el **cuerpo** para el solicitante técnico (`ACT-03`); los **anexos** para quien despliega y opera (`ACT-04`). Esa estratificación es la que permite que un mismo documento sirva a tres lectores con profundidad distinta sin escribir tres documentos.

El orden de abajo sirve tal cual a `ESC-2` (solución construida). Para `ESC-1` se agrega, dentro de cada sección, la marca de estado —decidido / propuesto / por confirmar—; para `ESC-3` se desdobla la arquitectura y el despliegue en «estado actual» y «estado objetivo». Esas variaciones se detallan en [`TEM-ESTRUCTURA`](../50-Redaccion/Estructura-del-Documento.md).

---

## Frontmatter del informe

Todo informe abre con un bloque de control. No es adorno: es lo que permite saber qué versión se está leyendo y contra qué sistema se verificó.

```yaml
---
titulo: Informe de solución — <nombre del sistema>
version: 1.0
fecha: AAAA-MM-DD
estado: borrador | en revisión | aprobado
autor: <ACT-01>
destinatario: <rol y organización — ver Actores.md>
escenario: ESC-?            # ver Escenarios.md
contexto: CTX-?             # ver Contextos.md
sistema_verificado_contra: <entorno y fecha, en ESC-2/ESC-4>
alcance:
  incluye: [arquitectura, despliegue, requisitos_funcionales, requisitos_no_funcionales]
  excluye: []              # lo que deliberadamente queda fuera, y por qué
---
```

---

## 1. Resumen ejecutivo

**Propósito.** Que `ACT-06` entienda, en una página, qué es la solución, cómo está resuelta a grandes rasgos y qué se le pide decidir. Es lo único que muchos lectores van a leer; se escribe último y se lee primero.

**Contenido.** Qué problema resuelve el sistema, el enfoque en una o dos frases, los tres o cuatro rasgos arquitectónicos que lo definen, y el estado —propuesto, en producción, en evolución—. Sin jerga, sin siglas sin expandir, sin diagramas.

**Preguntas guía.** ¿Se entiende sin leer el resto? ¿Un lector no técnico sabe, al terminarlo, qué tiene que decidir? ¿Estoy resumiendo o estoy adelantando el cuerpo?

**Frase de referencia (sintética).**

> El sistema gestiona la grabación de audiencias en salas distribuidas. Cada sala opera de forma autónoma: puede iniciar y grabar una audiencia aunque el centro de datos esté incomunicado, y sincroniza los videos y su estado cuando la conexión vuelve. La solución está en producción en 12 salas. Este informe describe su arquitectura, su despliegue y cómo resuelve los requisitos de continuidad, para evaluar su incorporación a la sede norte.

---

## 2. Introducción y alcance

**Propósito.** Fijar de qué trata el informe y de qué no, para quién, y con qué vocabulario. Evita el malentendido de base.

**Contenido.** Propósito del documento y decisión que habilita; destinatario y su nivel; qué incluye y qué deja explícitamente afuera —y por qué—; referencias a documentos relacionados (el [SAD](../../Documentacion-Tecnica/30-Arquitectura/SAD.md), el [SRS](../../Documentacion-Tecnica/20-Analisis/SRS.md) completos, si existen); y un glosario mínimo de los términos que el resto usa. Un informe que enlaza al SRS no repite los requisitos: los referencia.

**Preguntas guía.** ¿El lector sabe qué NO va a encontrar acá? ¿Las siglas que uso después están definidas? ¿Estoy prometiendo un alcance que el cuerpo no cumple?

---

## 3. Contexto y objetivos

**Propósito.** Ubicar la solución en su entorno: qué la rodea, quiénes son sus interesados y qué le preocupa a cada uno. Es donde `N-01` 42010 pide identificar *stakeholders* y *concerns*.

**Contenido.** El problema de negocio; los actores humanos y los sistemas externos con los que la solución interactúa (un diagrama de contexto C4 encaja acá); los *stakeholders* y su *concern* dominante —el operador quiere no perder una grabación, el administrativo quiere reproducir cualquier audiencia, el área de sistemas quiere instalar sin visitar cada sala—; y los objetivos y restricciones que condicionan la arquitectura.

**Preguntas guía.** ¿Nombré a cada interesado y lo que le importa? ¿El diagrama de contexto muestra los límites del sistema, o se mete adentro? ¿Las restricciones son reales o las inventé para justificar el diseño?

**Frase de referencia (sintética).**

> Tres interesados condicionan la arquitectura. El operador de sala necesita que una grabación nunca se pierda por una caída de red, lo que empuja hacia autonomía local. El área administrativa necesita ver cualquier audiencia desde el centro, lo que exige sincronización. El área de sistemas necesita instalar y actualizar sin desplazarse a cada sala, lo que condiciona el mecanismo de distribución.

---

## 4. Visión general de la arquitectura

**Propósito.** Dar el modelo mental de la solución antes del detalle: el estilo, la estrategia, la forma general. Corresponde a la sección 4 «Solution Strategy» de `G-01` arc42.

**Contenido.** El estilo arquitectónico y por qué (cliente-servidor, distribuido en el borde, capas); la estrategia que resuelve los *concerns* del punto 3 —«autonomía local con sincronización eventual»—; y un diagrama de contenedores (nivel *Container* de `G-02` C4) que muestre las piezas grandes y cómo se comunican. Es la sección que un lector apurado lee después del resumen.

**Preguntas guía.** ¿Se entiende la forma general sin haber leído el detalle? ¿El estilo elegido responde a un *concern*, o es el que salió por defecto? ¿El diagrama tiene el nivel de zoom correcto —piezas grandes, no clases?

---

## 5. Vista de componentes

**Propósito.** Describir las piezas de la solución, sus responsabilidades, sus relaciones y sus límites. Lo desarrolla [`TEM-COMPONENTES`](../20-Arquitectura/Vista-de-Componentes.md).

**Contenido.** Por cada componente: qué hace, de qué es responsable, con qué habla y qué no le corresponde. Una tabla componente → responsabilidad → depende de es más legible que párrafos. Se describen componentes y responsabilidades, **no la estructura de carpetas del repositorio**.

**Preguntas guía.** ¿Cada componente tiene una responsabilidad enunciable en una frase? ¿Mostré los límites —qué no hace cada uno? ¿Esto describe el sistema o el árbol de directorios?

**Frase de referencia (sintética).**

| Componente | Responsabilidad | Depende de |
|---|---|---|
| Programa de escritorio | El operador inicia, controla y cierra la audiencia | Servicio en segundo plano (local), backend |
| Servicio en segundo plano | Graba las cámaras y sube los videos; sobrevive al cierre del escritorio | Servidor de archivos, backend |
| Backend | Concentra el estado de las audiencias y lo expone al frontend | PostgreSQL |
| Frontend administrativo | Visualización y reproducción para el área administrativa | Backend, servidor de archivos |

---

## 6. Vistas y comportamiento

**Propósito.** Mostrar la solución desde más de un ángulo y narrar sus flujos importantes. Ninguna arquitectura no trivial se entiende con un solo diagrama; `O-01` 4+1 y `G-02` C4 lo sistematizan. Lo desarrolla [`TEM-VISTAS`](../20-Arquitectura/Vistas-y-Diagramas.md).

**Contenido.** Las vistas que el sistema necesita —rara vez todas—: componentes (ya en el punto 5), y uno o dos flujos dinámicos contados de punta a punta con un `sequenceDiagram`. El flujo elegido debe ser uno que revele la arquitectura, no el trivial.

**Preguntas guía.** ¿El flujo que elegí muestra algo que el diagrama estático no? ¿Cada vista responde a un *concern* distinto, o repito la misma información con otro dibujo?

**Frase de referencia (sintética).** El flujo que conviene narrar en el sistema de audiencias es la subida diferida: qué pasa desde que el operador cierra la audiencia hasta que el video queda disponible en el centro, incluyendo el caso en que la red se cae a mitad de subida. Se desarrolla en [`TEM-OPERACION`](../30-Despliegue/Operacion-y-Resiliencia.md).

---

## 7. Vista de despliegue

**Propósito.** Decir dónde corre cada cosa y cómo llega ahí. Es la sección que da nombre a la guía y la más subestimada en `CTX-3`. La notación estándar es el diagrama de despliegue de `N-07` UML; la organización, el *deployment diagram* de `G-02` C4. La desarrollan [`TEM-TOPOLOGIA`](../30-Despliegue/Topologia-y-Entornos.md) y [`TEM-DISTRIBUCION`](../30-Despliegue/Distribucion-e-Instalacion.md).

**Contenido.** La topología —qué nodo corre qué componente, qué protocolo y qué puerto los une—; los entornos —desarrollo, prueba, producción— y en qué difieren; y cómo se distribuye e instala cada componente, que en `CTX-3` es un procedimiento repetido por terminal, no una nota. Si existe una [Deployment Guide](../../Documentacion-Tecnica/50-Operativa/Deployment-Guide.md) o [Installation Guide](../../Documentacion-Tecnica/50-Operativa/Installation-Guide.md), el informe las referencia y resume, no las copia.

**Preguntas guía.** ¿Un responsable de despliegue puede, con esto, saber qué instalar y dónde? ¿Declaré el modelo de publicación —dependiente del framework o autocontenido? ¿La instalación por terminal está descrita como proceso o escondida?

---

## 8. Operación y resiliencia

**Propósito.** Explicar cómo se comporta la solución cuando algo falla. En un sistema con operación degradada, es la sección más valiosa. La desarrolla [`TEM-OPERACION`](../30-Despliegue/Operacion-y-Resiliencia.md).

**Contenido.** Qué hace el sistema cuando un nodo o un enlace cae; qué estado se conserva localmente; cómo se recupera y cómo se reconcilia al volver la conexión. En el sistema de audiencias, los tres comportamientos que lo definen —operar con el centro caído, recuperar el estado tras la caída del escritorio, seguir subiendo en segundo plano al cerrar—. Se cruza con los requisitos no funcionales del punto 10.

**Preguntas guía.** ¿Describí qué pasa en el peor caso, no solo en el feliz? ¿El comportamiento degradado está declarado como diseño o parece un accidente afortunado? ¿La reconciliación de estado está explicada?

---

## 9. Resolución de requisitos funcionales

**Propósito.** Mostrar que la solución hace lo que debe, trazando cada requisito significativo hasta el mecanismo que lo resuelve. Lo desarrolla [`TEM-RF`](../40-Requisitos/Requisitos-Funcionales.md). El informe no re-lista el [SRS](../../Documentacion-Tecnica/20-Analisis/SRS.md): traza los requisitos que importan.

**Contenido.** Una tabla de trazabilidad requisito → componente/mecanismo → evidencia, para los requisitos funcionales significativos, con identificadores `RF-`.

**Preguntas guía.** ¿Cada requisito significativo apunta a un mecanismo concreto? ¿Los requisitos vienen de quien los definió (`ACT-05`) o los inventé para que calzaran? ¿Distinguí «implementado» de «previsto»?

---

## 10. Resolución de requisitos no funcionales

**Propósito.** Mostrar cómo la solución alcanza sus atributos de calidad, con métricas. Es donde una arquitectura se gana o se pierde. La referencia es `N-04` ISO/IEC 25010:2023 con sus nueve características; lo desarrolla [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md).

**Contenido.** Por cada atributo de calidad relevante: el requisito expresado de forma **medible**, cómo lo resuelve la arquitectura, y cómo se mide. Identificadores `RNF-`. Una intención sin métrica —«diseñado para escalar»— no es un requisito no funcional; es una aspiración.

**Preguntas guía.** ¿Cada RNF tiene un número o un criterio verificable? ¿Nombré la característica de `N-04` a la que corresponde? ¿Afirmo disponibilidad sin decir cómo la mido?

**Frase de referencia (sintética).**

| RNF | Característica `N-04` | Cómo lo resuelve | Cómo se mide |
|---|---|---|---|
| `RNF-01` La sala graba con el centro caído | Reliability — fault tolerance | Grabación y estado en almacenamiento local del servicio en segundo plano | Prueba de corte de red: 0 grabaciones perdidas |
| `RNF-02` Recuperar la audiencia tras caída del escritorio | Reliability — recoverability | Estado persistido localmente; el escritorio lo relee al reiniciar | Tiempo de recuperación < 10 s, medido |
| `RNF-03` Subir en segundo plano sin bloquear | Performance Efficiency — capacity | Cola de subida en el servicio, independiente de la sesión del escritorio | Nueva audiencia iniciable con subidas en curso |

---

## 11. Decisiones de arquitectura y trade-offs

**Propósito.** Registrar por qué la solución es como es, con las alternativas descartadas y su costo. `N-01` 42010 cuenta la decisión y su *rationale* entre lo que una descripción de arquitectura debe registrar. Lo desarrolla [`TEM-DECISIONES`](../20-Arquitectura/Decisiones-de-Arquitectura.md); para decisiones grandes con vida propia, se referencia un [ADR](../../Documentacion-Tecnica/30-Arquitectura/ADR.md) en lugar de inflar el informe.

**Contenido.** Por cada decisión estructural: contexto, decisión, alternativas consideradas, consecuencias —las buenas y las malas—. Una decisión sin alternativas registradas no se distingue de un reflejo.

**Preguntas guía.** ¿Registré lo que descarté y por qué? ¿Las consecuencias incluyen los costos, o solo los beneficios? ¿Esta decisión merece un ADR propio?

**Frase de referencia (sintética).**

> **Decisión.** Grabar en un servicio en segundo plano por terminal, no en el backend. **Contexto.** Las cámaras se conectan a la terminal y la audiencia debe grabarse con el centro caído. **Alternativas.** Enviar el video en vivo al backend (descartada: depende del enlace, que es lo que puede fallar). **Consecuencias.** Autonomía total de la sala, al costo de instalar y mantener software en cada terminal, tratado en [`TEM-DISTRIBUCION`](../30-Despliegue/Distribucion-e-Instalacion.md).

---

## 12. Riesgos y pendientes

**Propósito.** Declarar lo que puede salir mal y lo que todavía no está resuelto. Corresponde a la sección 11 «Risks & Technical Debt» de `G-01` arc42. Un informe honesto tiene esta sección; uno de venta, no.

**Contenido.** Riesgos técnicos conocidos con su mitigación; deuda técnica asumida; y, en `ESC-1`, los supuestos que hay que confirmar antes de construir. En `ESC-4`, acá van las preguntas que el informe evaluado no respondió.

**Preguntas guía.** ¿Escondí algún riesgo por incomodidad? ¿Los pendientes tienen dueño y criterio de cierre, o son una lista de deseos?

---

## 13. Anexos del informe

**Propósito.** Guardar el material de consulta que interrumpiría la lectura del cuerpo.

**Contenido.** Glosario completo; referencias a normas y documentos; diagramas fuente en su forma editable; y las plantillas de configuración o los procedimientos detallados que `ACT-04` necesita pero `ACT-03` no. Los diagramas van como código —Mermaid— para que sean regenerables.

---

## Lista de verificación de la plantilla

Antes de entregar el informe, se recorre esta lista. La completa está en [`ANEXO-CHECK`](Lista-de-Verificacion.md).

```yaml
resumen_ejecutivo_se_entiende_solo: si | no
alcance_declara_lo_excluido: si | no
stakeholders_y_concerns_nombrados: si | no
diagramas_coinciden_con_el_texto: si | no
despliegue_describe_instalacion_por_terminal: si | no | na   # obligatorio en CTX-3
cada_RF_significativo_trazado_a_mecanismo: si | no
cada_RNF_tiene_metrica: si | no
decisiones_registran_alternativas_y_costo: si | no
afirmaciones_normativas_citan_fuente: si | no
seccion_de_riesgos_presente: si | no
divergencias_diseno_realidad_declaradas: si | no | na        # obligatorio en ESC-2
```

El campo `cada_RNF_tiene_metrica` es el que más informes reprueba. Un requisito no funcional sin métrica no se puede verificar, y lo que no se puede verificar no se puede afirmar que la solución lo cumple.
