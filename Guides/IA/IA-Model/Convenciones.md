---
doc_id: IAM-M3
doc_type: marco-de-referencia
title: Convenciones de la guía de modelos de IA
status: borrador
origin: ia-generated
confidence: alta
owner: ACT-06
last_review: 2026-11-07
audience: [humano, agente]
traces: [IAM-M0, IAM-M1, IAM-M2]
---

# Convenciones de la guía

Este documento concentra el aparato editorial completo del cuerpo `Guides/IA/IA-Model/`: identidad
documental, citado, ejemplos operables, protocolo de cifras, profundidad, vigencia y la plantilla
del documento temático. La regla vive acá una sola vez; cada documento temático la cumple
**citando este documento**, nunca repitiéndola (expediente 0001, folio 011 §4 y veredicto RF-02).
Cuando un documento temático necesite apartarse de una convención, lo declara al abrir, con motivo:
un apartamiento silencioso se trata como defecto editorial.

## Índice

1. [Identidad documental](#1-identidad-documental)
2. [Citado y referencias](#2-citado-y-referencias)
3. [Patrón de ejemplo operable](#3-patrón-de-ejemplo-operable)
4. [Bloque «protocolo»: toda cifra con su corrida](#4-bloque-protocolo-toda-cifra-con-su-corrida)
5. [Contrato de profundidad](#5-contrato-de-profundidad)
6. [Política de vigencia y capa volátil](#6-política-de-vigencia-y-capa-volátil)
7. [Plantilla del documento temático](#7-plantilla-del-documento-temático)
8. [Referencias de este documento](#8-referencias-de-este-documento)

---

## 1. Identidad documental

### 1.1 Frontmatter

Todo documento del cuerpo abre con el bloque de frontmatter normado de la casa (ia-db del
repositorio, índice `10_Convenciones-y-Glosario.md` §1.1):

```yaml
---
doc_id: IAM-NN                 # ver §1.2
doc_type: tema | marco-de-referencia | mapa | anexo | indice
title: <título del documento>
status: vigente | borrador | obsoleto
origin: human | ia-assisted | ia-generated
confidence: alta | media | baja        # obligatorio si origin != human
owner: ACT-NN                  # sólo el ID del actor, del catálogo de M1
last_review: AAAA-MM-DD
audience: [humano, agente]
traces: [doc_id]               # semántica fijada abajo
---
```

Dos campos llevan forma fijada, porque son la interfaz de consulta mecánica del cuerpo
(expediente 0001, ronda 2):

- **`owner`**: sólo el ID del actor responsable, del catálogo de
  [`Marco-De-Referencia.md`](Marco-De-Referencia.md) (`ACT-06`, `ACT-05`, …). El rótulo del rol se
  lee en ese catálogo, no se repite acá: así `grep '^owner:'` devuelve una sola forma en las
  quince piezas.
- **`traces`**: en un documento temático lista **los prerrequisitos que el grafo de
  [`Cohesion-Global.md`](Cohesion-Global.md) declara, más el siguiente de la ruta formativa**, y
  nada más. Las piezas estructurales (IAM-M0 a IAM-M3) no se listan: son apoyo universal y se
  cargan siempre. En las piezas estructurales, `traces` lista sus pares estructurales (y, en el
  índice IAM-M0, el cuerpo completo que indexa).

Los campos no aplicables se omiten, no se dejan vacíos. No hay campo `version`: la vigencia se lee
en `last_review` y en la política de §6. Los documentos con ejemplos operables agregan un campo
extra `entorno_validado` (ver §6.2), siguiendo el uso ya relevado en las guías con laboratorio de
la casa.

### 1.2 Identificadores

El prefijo del cuerpo es `IAM-` (verificado libre de colisiones en el expediente, hallazgo
ARQIA-06). Asignación:

| doc_id | Documento |
| --- | --- |
| `IAM-M0` | `IA-Model-Guide.md` — raíz, portada e itinerarios |
| `IAM-M1` | `Marco-De-Referencia.md` — escenarios, contextos, actores, glosario |
| `IAM-M2` | `Cohesion-Global.md` — grafo de dependencias y fronteras |
| `IAM-M3` | `Convenciones.md` — este documento |
| `IAM-01` … `IAM-11` | Documentos temáticos, con el número de su núcleo |

Los enlaces internos del cuerpo son **relativos** y, en prosa, nombran el `doc_id` o el título:
los archivos pueden moverse, los identificadores no. Nada en el cuerpo referencia rutas absolutas
de ningún equipo: el banco de ensayo se escribe `http://localhost:11434` o `$OLLAMA_HOST` (§3.3).

### 1.3 Jerarquía y grafías

Encabezados sin saltos de nivel (`##` dentro de `#`, `###` dentro de `##`). La terminología técnica
usa la forma canónica que fija el glosario de [`Marco-De-Referencia.md`](Marco-De-Referencia.md):
término en inglés cuando esa es la forma canónica (transformer, embedding, fine-tuning), definición
en español. Rigen las grafías normalizadas de su tabla — en particular **Qwen**, **open source** y
**borrador**, que el pedido original trae con erratas (expediente 0001, hallazgo H-EDI-08); la
grafía errada no sale del testimonio del expediente.

---

## 2. Citado y referencias

### 2.1 Sistema

Citado **autor-año, estilo APA 7.ª edición** (American Psychological Association, 2020) en prosa:
`(Vaswani et al., 2017)`. Cada documento cierra con su propia sección **Referencias**, con la lista
completa de lo que ese documento cita: el lector de un documento suelto no debe salir del archivo
para resolver una cita. La elección es decisión del caso (expediente 0001, hallazgo H-EDI-04): el
sistema autor-año le dice algo al principiante sin obligarlo a saltar a la lista, y convertirlo a
numérico después es mecánico si alguna vez hiciera falta.

### 2.2 Qué exige cada tipo de fuente

| Tipo de fuente | La cita exige | Ejemplo de entrada en Referencias |
| --- | --- | --- |
| Paper académico | Autor(es), año, título, identificador perdurable (arXiv o DOI) | Vaswani, A., et al. (2017). Attention is all you need. arXiv:1706.03762 |
| Informe técnico de un equipo de modelos | Equipo como autor colectivo, año, título, identificador | Qwen Team. (2025). *Qwen3 Technical Report*. arXiv:2505.09388 |
| Documentación oficial de herramienta | Proyecto, URL y **fecha de consulta** | Ollama. *Documentación oficial*. https://docs.ollama.com (consultada el AAAA-MM-DD) |
| Norma o estándar | Organismo y número | ISO 690:2021 |
| Decisión del caso | Expediente y folio | Expediente 0001, folio 011 §4 |

Reglas de borde: el informe técnico de un equipo se cita por su autor colectivo **aunque el PDF
liste autores nominales** — para arXiv:2505.09388 la forma citable del cuerpo es «Qwen Team
(2025)», no la nómina de la portada (expediente 0001, ronda 2). Si el apartado exacto de una
fuente no es seguro, se cita **sin apartado** antes que inventarlo. Un blog de divulgación no respalda una afirmación técnica: se busca la fuente
primaria (paper, informe técnico, documentación oficial) o la afirmación se degrada a criterio
propio.

### 2.3 Hecho, inferencia y criterio propio

Toda afirmación de industria o academia lleva su cita. Lo que es criterio del autor se declara como
tal en la prosa («criterio de esta guía: …»), nunca disfrazado de hecho; `origin` y `confidence`
del frontmatter sostienen la misma distinción a nivel documento. Lo no verificado se marca
(«no verificado», «requiere verificación») en lugar de afirmarse — es restricción dura del contrato
de entrada del expediente, no una preferencia de estilo.

---

## 3. Patrón de ejemplo operable

### 3.1 Operable o ilustrativo, nunca ambiguo

Cada ejemplo del cuerpo es una de dos cosas, declarada al abrir:

- **Operable**: el lector lo corre y obtiene la salida esperada. Exige todo lo de §3.2.
- **Ilustrativo** (marcado `**[ilustrativo]**` en su título): se lee, no se corre; muestra forma o
  magnitud de algo que el entorno de referencia no puede reproducir (preentrenamiento a escala,
  generación de imagen según el alcance vigente). No promete salida.

Cada laboratorio operable existe **una sola vez**, en el documento dueño que asigna el mapa de
laboratorios (expediente 0001, folio 011 §4, pregunta 4); los demás documentos lo citan por enlace
relativo, no lo duplican. Tope: 2 a 3 laboratorios operables por documento temático.

### 3.2 Qué exige un ejemplo operable

1. **Contexto declarado**: en qué CTX del marco corre. El piso del cuerpo es **CTX-01: CPU x86-64,
   8–16 GB de RAM, sin GPU** — todo ejemplo base reproduce ahí, o marca explícitamente el contexto
   mayor que exige (default de la fila 1 del lote del expediente).
2. **Versiones fijadas**: el modelo con su etiqueta exacta y su digest (`qwen3:4b` a secas no
   alcanza si el registro puede moverla; `ollama show` da el digest), la herramienta con su número.
   Nunca «la última»: regla de la casa.
3. **Comandos transcriptos completos**, copiables, en bloques de código con lenguaje declarado.
4. **Salida esperada capturada** de una corrida real, recortada a lo relevante. La salida de un
   modelo generativo no es determinista: se captura una corrida real y se anota qué parte es
   estable (formato, orden de magnitud) y qué parte varía entre corridas.
5. **Qué hacer si difiere**: distinguir divergencia por entorno, por versión o por azar del
   muestreo, con reenvío al documento que enseña ese diagnóstico.
6. **Fecha y entorno de validación** en el frontmatter del documento (`entorno_validado`, §6.2).

### 3.3 El banco de ensayo y el paso 0

Los ejemplos de inferencia corren contra un servidor Ollama. El cuerpo lo escribe siempre
parametrizado — `$OLLAMA_HOST`, con `http://localhost:11434` como valor por defecto —, nunca con la
dirección de un equipo concreto. Todo laboratorio que pegue contra el banco empieza con el **paso 0
de verificación**, que se enseña una sola vez en `08-Inferencia-Local.md` y los demás citan
(expediente 0001, hallazgo H-IR-03):

```bash
curl -s "${OLLAMA_HOST:-http://localhost:11434}/api/tags" >/dev/null \
  && echo "banco accesible" || echo "banco NO accesible: revisá OLLAMA_HOST"
```

El par Ollama/Qwen es el **medio** de los ejemplos, no el alcance de la guía: es decisión del
contrato del expediente con origen en sus MEDIOS, asentada como agregado propuesto pendiente de
ratificación (fila 4 del lote del expediente). Los documentos forman criterio sobre el espacio de
motores y modelos abiertos; ejercitan sobre el banco.

---

## 4. Bloque «protocolo»: toda cifra con su corrida

Ninguna cifra medida entra al cuerpo suelta. Toda métrica que el texto afirme como resultado propio
(tokens/s, perplexity, exactitud de un eval, memoria consumida, tamaño de un índice) lleva al lado
su bloque «protocolo» (expediente 0001, hallazgo EM-03), colapsable para no romper la lectura:

```markdown
<details>
<summary>Protocolo de la cifra</summary>

- **Cifra**: 14,2 tokens/s (generación)
- **Modelo**: qwen3:4b — digest `sha256:…` (salida de `ollama show`)
- **Motor**: ollama 0.x.y
- **Parámetros**: temperature 0.7, top_p 0.9, num_ctx 8192 (los no listados, en su default)
- **Semilla**: 42 (o «sin semilla: muestreo libre», declarado)
- **Entrada / dataset**: prompt transcripto arriba, o dataset con nombre, versión y licencia
- **Contexto de hardware**: CTX-01 (CPU x86-64, 16 GB) — o el que corresponda
- **Fecha de la corrida**: AAAA-MM-DD
</details>
```

Campos no aplicables se omiten declarándolo («sin dataset: entrada única»). Una cifra **ajena** no
lleva protocolo propio: lleva su cita (§2), y si es una cifra de un modelo del ecosistema, se toma
de la tabla de modelos de referencia de `11-Panorama-Y-Ecosistema-Abierto.md`, único lugar donde
esas cifras viven (§6.3). Si una cifra no puede sostener ni protocolo ni cita, no se publica.

Cuando un defecto alcanza a una cifra ya verificada, rige un criterio único (expediente 0001,
ronda 2, hallazgo R2-REF-07): el defecto de **contenido** se corrige y el laboratorio se recorre;
el defecto de **forma** se asienta como errata junto al bloque, con la condición de recorrerse en
la próxima corrida completa del laboratorio.

---

## 5. Contrato de profundidad

El cuerpo inicia a un estudiante sin conocimiento previo de IA (ACT-01: terminal y programación
básica, supuesto declarado del expediente) y a la vez sirve de referencia. Para que once redactores
no resuelvan esa tensión de once maneras, rige el mismo contrato en todo documento temático
(expediente 0001, hallazgo EM-04):

1. **Intuición primero.** Cada concepto entra por una explicación en lenguaje natural, con analogía
   o ejemplo concreto, que se sostiene sola: el lector que pare ahí se lleva una idea correcta
   aunque incompleta.
2. **Fórmula de una línea.** Donde la precisión lo pida, la formalización cabe en una línea con sus
   símbolos nombrados en prosa. Softmax, similitud coseno o la loss autoregresiva entran así.
3. **Sin cálculo diferencial en el hilo.** Derivaciones, gradientes y demostraciones no van en el
   cuerpo del texto. Si un documento las necesita, van a su anexo, marcadas como lectura optativa,
   o se reemplazan por la cita al paper (§2).
4. **Profundidad al servicio de una decisión.** Una sección que no deja al lector en condiciones de
   decidir o hacer algo que antes no podía, sobra o está en el documento equivocado (criterio
   «formativo, no enciclopédico» del Profile del framework).

Extensión orientativa del documento temático: **350–700 líneas**. Un documento que la supere de
forma sostenida es candidato a partirse por el eje que declara el grafo de
[`Cohesion-Global.md`](Cohesion-Global.md), no a comprimir su prosa hasta volverla críptica.

---

## 6. Política de vigencia y capa volátil

### 6.1 Capa estable y capa volátil

El dominio se renueva en ciclos de meses; la guía lo absorbe separando dos capas (expediente 0001,
hallazgos ARQIA-02 y H-EDI-06):

| Capa | Qué contiene | Dónde vive | Cómo envejece |
| --- | --- | --- | --- |
| **Estable** | Conceptos, mecánica, criterios: tokenización, atención, cuantización, cómo se lee un benchmark | Documentos 01–10 y los estructurales | Lenta; se revisa por calendario |
| **Volátil** | Familias y modelos concretos, cifras comparativas, licencias vigentes, tabla de modelos de referencia | **Sólo** `11-Panorama-Y-Ecosistema-Abierto.md`, deliberadamente fechado | Rápida; se revisa por evento |

Los documentos estables **no citan cifras ni rankings de modelos concretos** salvo vía la tabla de
referencia del documento 11 (§6.3). Así el envejecimiento del catálogo invalida un documento, no
once.

Queda **exenta** de esa regla la cifra histórica o arquitectónica con cita primaria fechada
(expediente 0001, ronda 2, hallazgo R2-FUN-01): no envejece con el catálogo sino con su fuente, y
su cita (§2) la sostiene donde esté. La cifra del parque vigente, en cambio, entra **sólo** vía la
tabla del documento 11. Condición de revisión de la exención: si una cifra histórica pasa a usarse
comparativamente contra el parque vigente, deja de ser historia y vuelve a la tabla.

### 6.2 Vigencia declarada

- `last_review` del frontmatter es la fecha de la última revisión deliberada, no del último commit.
- Todo documento con ejemplos operables lleva `entorno_validado` con la foto mínima que permite
  juzgar si sigue valiendo sin correr nada: motor y versión, modelo y etiqueta, contexto (CTX) y
  fecha de la última corrida completa de sus laboratorios.
- **Disparadores de revisión** (actualizan `last_review` tras revisar, no antes): release mayor de
  la familia de modelos usada en los ejemplos; cambio de versión del motor del banco; un laboratorio
  reportado como no reproducible; y, para el documento 11, seis meses de calendario sin evento.
- **Compuerta de compilación mermaid** (expediente 0001, ronda 2, hallazgo R2-REF-10): todo
  diagrama mermaid del cuerpo compila antes de promover el documento que lo contiene, y se vuelve
  a compilar tras cada edición que lo toque. Un diagrama que no compila bloquea la promoción.
- Un documento que se reemplaza pasa a `status: obsoleto` con el `doc_id` de su reemplazo en
  `traces`: obsolescencia declarada, nunca silenciosa.

El responsable de este ciclo es el mantenedor de la guía (ACT-06 del marco de referencia).

### 6.3 La tabla de modelos de referencia

`11-Panorama-Y-Ecosistema-Abierto.md` mantiene la única tabla de modelos de referencia del cuerpo:
etiqueta exacta, tamaño, licencia, contexto, fuente de cada dato y fecha. Toda mención comparativa
de un modelo concreto en cualquier otro documento enlaza a esa tabla (expediente 0001, hallazgos
FP-04 y FP-05). Es la pieza más perecedera del cuerpo y por eso la más vigilada.

---

## 7. Plantilla del documento temático

Estructura obligatoria de `IAM-01` a `IAM-11` (expediente 0001, folio 011 §4; las seis secciones
del Profile del framework, con el bloque didáctico absorbiendo «Preguntas guía»). Los comentarios
`<!-- -->` guían al redactor y no sobreviven en el documento final.

```markdown
---
doc_id: IAM-NN
doc_type: tema
title: <título>
status: borrador
origin: ia-generated
confidence: <alta|media|baja>
owner: <ID del actor, ACT-NN (§1.1)>
last_review: AAAA-MM-DD
audience: [humano, agente]
traces: [<prerrequisitos del grafo y siguiente de la ruta, §1.1>]
entorno_validado: <motor+versión, modelo+etiqueta, CTX, fecha>   # sólo si hay labs operables
---

# NN. <Título>

<!-- Envoltura: una frase de qué resuelve este documento; luego la navegación. -->

**Antes de leer**: <prerrequisitos del grafo de Cohesion-Global.md, con enlaces relativos>.
**Anterior**: <doc NN-1 de la ruta> · **Siguiente**: <doc NN+1 de la ruta>.
Convenciones del cuerpo: [Convenciones.md](Convenciones.md) (citado §2, ejemplos §3, cifras §4).

## Índice
<!-- Enlaces a las secciones de este documento. -->

## 1. Definición
<!-- Qué es, qué problema resuelve, qué no es. Intuición primero (contrato §5).
     Cada definición entrega su icono (imagen mental) y su ancla (a qué conocido se ata). -->

## 2. Aplicación por escenario
<!-- Qué aporta este tema en cada ESC del marco que lo cruza, y qué cambia por CTX.
     Un tema que no cruza ningún escenario está mal delimitado. -->

## 3. Ejemplos concretos
<!-- 2–3 laboratorios operables de los que este documento es dueño (patrón §3), con paso 0
     citado y protocolo §4 en cada cifra. Labs ajenos: enlace, no copia.
     Ejemplos que no corren acá: marcados [ilustrativo]. -->

## 4. Bloque didáctico
<!-- Por cada pregunta guía, las cuatro piezas, juntas:
     **Pregunta** — la que forma criterio, no la que verifica memoria.
     **Criterio que forma** — explícito: qué va a poder decidir el lector.
     **Respuesta explicativa** — se sostiene sola.
     **Reenvío** — enlace a la sección o ejemplo de este documento que la realimenta. -->

## 5. Criterios de calidad
<!-- Cómo se distingue una versión buena de una pobre del artefacto o decisión del tema. -->

## 6. Anexo
<!-- Plantilla o lista de verificación comentada, cuando el tema la pida; si no, se omite
     la sección entera (no se deja vacía). -->

## Referencias
<!-- Lista completa de lo citado en este documento (§2.2). -->
```

Dos reglas de cierre sobre la plantilla: el documento es **autocontenido** en el sentido operativo
que define [`Cohesion-Global.md`](Cohesion-Global.md) — seguible sin abrir otro documento, con
reenvíos para profundizar — y **no delta**: ninguna versión exige haber leído una anterior. Los
diagramas van como código mermaid donde un diagrama diga más que la prosa; la integración de
modelos en aplicaciones .NET se referencia a la guía 16 del repositorio, no se repite.

---

## 8. Referencias de este documento

- American Psychological Association (2020). *Publication Manual of the American Psychological
  Association* (7.ª ed.). APA.
- ISO 690:2021. *Information and documentation — Guidelines for bibliographic references and
  citations to information resources*.
- Vaswani, A. et al. (2017). «Attention Is All You Need». arXiv:1706.03762. (Usada como patrón de
  cita; su contenido se trata en el documento 02.)
- Qwen Team (2025). «Qwen3 Technical Report». arXiv:2505.09388. (Usada como patrón de cita de
  informe técnico.)
- Expediente 0001 «Guía de estudio IA-Model» (2026): folio 002 (contrato de entrada), folio 009
  (informe de la comisión de edición) y folio 011 (veredicto del jurado) — origen de cada decisión
  editorial marcada «expediente 0001» en este documento.
- ia-db de Lab-Documentos, índice `10_Convenciones-y-Glosario.md` — bloque de frontmatter normado
  y patrón de las guías estructuradas de la casa.
- Profile *Study-Guide-Documentation* y Rules *Markdown*, *Documentation*, *Evidences*,
  *Dual-Audience* y *Narrative-Voice* del PromptFramework — pliego editorial que este documento
  instancia.
