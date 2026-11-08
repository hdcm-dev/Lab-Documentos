---
doc_id: IAM-M0
doc_type: indice
title: Guía de estudio de modelos de IA — portada e itinerarios de lectura
status: borrador
origin: ia-generated
confidence: alta
owner: ACT-06
last_review: 2026-11-07
audience: [humano, agente]
traces: [IAM-M1, IAM-M2, IAM-M3, IAM-01, IAM-02, IAM-03, IAM-04, IAM-05, IAM-06, IAM-07, IAM-08, IAM-09, IAM-10, IAM-11]
---

# Guía de estudio de modelos de IA

Esta guía enseña qué es un modelo de lenguaje y cómo se trabaja con él de punta a punta: cómo
representa el texto, qué máquina lo mueve, de dónde salen sus datos y su entrenamiento, cómo se lo
monta y sirve en una computadora propia, cómo se lo evalúa con evidencia, cómo se le conecta un
corpus propio (RAG, *Retrieval-Augmented Generation*) y cómo se lee el parque de modelos de pesos
abiertos para elegir uno. No es un tutorial de una herramienta: forma **criterio**, y cada criterio
se practica con laboratorios operables que corren en hardware común.

El cuerpo son quince documentos bajo `Guides/IA/IA-Model/`: esta raíz, tres piezas de marco y once
documentos temáticos, uno por núcleo. Cada documento temático es **autocontenido** (se sigue sin
abrir otro, con sus prerrequisitos declarados al abrir) y comparte la misma estructura interna:
definición, aplicación por escenario, ejemplos concretos, bloque didáctico, criterios de calidad y
anexo. La estructura, el temario y las reglas editoriales son decisiones asentadas del caso que
originó la guía (expediente 0001, folio 011 §2 y §4).

## Índice

1. [Audiencia y supuestos](#1-audiencia-y-supuestos)
2. [Tabla de contenido](#2-tabla-de-contenido)
3. [Ruta de lectura formativa](#3-ruta-de-lectura-formativa)
4. [Ruta de consulta: estoy acá, qué leo](#4-ruta-de-consulta-estoy-acá-qué-leo)
5. [Cómo leer esta guía](#5-cómo-leer-esta-guía)
6. [Referencias](#6-referencias)

---

## 1. Audiencia y supuestos

**Para quién es.** Para el estudiante-lector (ACT-01 del
[marco de referencia](Marco-De-Referencia.md)): una persona **sin conocimiento previo de IA**, con
dos únicos supuestos declarados — manejo básico de la terminal y de algún lenguaje de programación
(decisión FP-02 del caso; supuesto de la fila 3 del lote). Ninguna pieza asume más que eso; la que
excepcionalmente exija más, lo marca al abrir. La guía tiene además **doble audiencia**: está
escrita para que la aproveche igual un lector humano y un agente de IA que la cargue como contexto
(`audience: [humano, agente]` en cada frontmatter).

**Con qué equipo.** El piso es CTX-01: una computadora sólo CPU x86-64 con 8–16 GB de RAM. Todo
ejemplo base corre ahí o declara en qué contexto corre. Los laboratorios que piden un motor de
inferencia usan un banco de ensayo accesible por red — en los textos, `http://localhost:11434` o
`$OLLAMA_HOST` —, que el lector puede reemplazar por su propia instalación local siguiendo el
documento [08](08-Inferencia-Local.md). Los catálogos completos de escenarios (ESC), contextos
(CTX) y actores (ACT) viven en [`Marco-De-Referencia.md`](Marco-De-Referencia.md) y todos los
documentos los usan sin redefinirlos.

**Qué promete al cerrar.** Haber montado y servido un modelo local, haberle hablado por API con el
muestreo bajo control, haber ajustado uno chico con datos propios, haberlo medido con un eval
propio con protocolo, haber indexado un corpus propio y preguntado con fuentes, y poder leer una
ficha de modelo (*model card*) y elegir con criterio.

## 2. Tabla de contenido

Una línea por pieza; los enlaces son relativos a esta carpeta.

| # | Pieza | Qué lleva |
| --- | --- | --- |
| M0 | `IA-Model-Guide.md` — este documento | Portada, tabla de contenido y los dos itinerarios de lectura (formativo y de consulta) |
| M1 | [`Marco-De-Referencia.md`](Marco-De-Referencia.md) | Catálogos ESC/CTX/ACT, glosario bilingüe con forma canónica única por término y tabla de normalización de grafías |
| M2 | [`Cohesion-Global.md`](Cohesion-Global.md) | Grafo de dependencias entre los once núcleos, definición operativa de autocontenido/no-delta, fronteras entre núcleos y lecturas asentadas del pedido |
| M3 | [`Convenciones.md`](Convenciones.md) | Aparato editorial completo: identidad documental, citado APA 7.ª, patrón de ejemplo operable, bloque «protocolo» por cifra, contrato de profundidad, política de vigencia y plantilla del documento temático |
| 01 | [`01-Representacion-Del-Lenguaje.md`](01-Representacion-Del-Lenguaje.md) | Del texto a tokens, vocabularios y embeddings; similitud y ventana de contexto |
| 02 | [`02-Arquitectura-Transformer.md`](02-Arquitectura-Transformer.md) | Atención, *decoder-only*, codificación de posición, MoE: la mecánica común del parque vigente |
| 03 | [`03-Del-Modelo-Base-Al-Asistente.md`](03-Del-Modelo-Base-Al-Asistente.md) | Base/*instruct*, alineación como taxonomía, muestreo, *prompting*, modos de razonamiento, alucinación |
| 04 | [`04-Datos-Y-Corpus.md`](04-Datos-Y-Corpus.md) | Formatos de datasets, limpieza/deduplicación/contaminación, licencias y procedencia de pesos y datos |
| 05 | [`05-Preentrenamiento-Y-Leyes-De-Escala.md`](05-Preentrenamiento-Y-Leyes-De-Escala.md) | Objetivo autoregresivo, pérdida/perplejidad, *checkpoints*, economía del preentrenamiento; demo juguete en CPU |
| 06 | [`06-Ajuste-Fino-Y-Alineacion.md`](06-Ajuste-Fino-Y-Alineacion.md) | SFT, LoRA/QLoRA, DPO/RLHF; la cadena Hugging Face (PEFT/TRL) → GGUF → motor local hasta servir el resultado |
| 07 | [`07-Evaluacion-Y-Metricas.md`](07-Evaluacion-Y-Metricas.md) | Perplejidad, métricas de tarea, *benchmarks* y su contaminación, LLM-juez, evals propios con protocolo |
| 08 | [`08-Inferencia-Local.md`](08-Inferencia-Local.md) | Motores (llama.cpp/Ollama/vLLM), GGUF, cuantización, memoria, métricas de servicio; montar, descargar y servir — y el paso 0 de verificación del banco que todos citan |
| 09 | [`09-Bases-Vectoriales-Y-RAG.md`](09-Bases-Vectoriales-Y-RAG.md) | Embeddings aplicados, índices ANN/HNSW, motores vectoriales y el pipeline RAG con su diagnóstico por etapas |
| 10 | [`10-Multimodalidad-Y-Generacion-No-Textual.md`](10-Multimodalidad-Y-Generacion-No-Textual.md) | VLM, audio y difusión a nivel conceptual: qué cambia y qué no respecto del *transformer* |
| 11 | [`11-Panorama-Y-Ecosistema-Abierto.md`](11-Panorama-Y-Ecosistema-Abierto.md) | Familias, licencias y pesos abiertos, *model cards*, tabla de modelos de referencia, criterio de selección — la única pieza deliberadamente fechada (capa volátil) |

## 3. Ruta de lectura formativa

Es el itinerario de quien empieza de cero y quiere formarse: **un solo orden, derivado del grafo de
dependencias** de [`Cohesion-Global.md`](Cohesion-Global.md) — cada documento usa sólo lo que los
anteriores ya dieron por leído, con una excepción razonada que se explica abajo.

> M0 (esta portada) → [M1](Marco-De-Referencia.md) →
> [01](01-Representacion-Del-Lenguaje.md) → [02](02-Arquitectura-Transformer.md) →
> [03](03-Del-Modelo-Base-Al-Asistente.md) → [08](08-Inferencia-Local.md) →
> [04](04-Datos-Y-Corpus.md) → [05](05-Preentrenamiento-Y-Leyes-De-Escala.md) →
> [06](06-Ajuste-Fino-Y-Alineacion.md) → [07](07-Evaluacion-Y-Metricas.md) →
> [09](09-Bases-Vectoriales-Y-RAG.md) → [10](10-Multimodalidad-Y-Generacion-No-Textual.md) →
> [11](11-Panorama-Y-Ecosistema-Abierto.md)

Cómo leerla:

1. **M1 primero, como mapa, no como lección.** Alcanza con recorrer los catálogos ESC/CTX/ACT una
   vez para reconocer los identificadores; al glosario se vuelve cuando un término lo pida.
2. **[`Convenciones.md`](Convenciones.md) (M3) no está en la fila**: es el reglamento editorial.
   Se consulta la primera vez que un documento remite a él — típicamente al llegar al primer
   laboratorio (patrón de ejemplo operable, §3) o a la primera cifra con «protocolo» (§4).
3. **El 08 va temprano a propósito.** Sus prerrequisitos son 02 y 03, así que puede leerse apenas
   termina el 03 — y conviene: desde ahí el lector tiene un modelo servido y verificado (el
   «paso 0» del banco), y los laboratorios de 04 a 11 pegan contra algo que él mismo montó. Es la
   única alteración del orden numérico, y la declara el grafo, no el capricho.
4. **El tramo 04 → 05 → 06 → 07 es el ciclo de vida del modelo**: qué datos entran, cómo se
   preentrena, cómo se ajusta y cómo se mide si el ajuste sirvió. Se lee en bloque.
5. **El 11 cierra siempre.** Depende de casi todos (05–10) y es la capa volátil: su fotografía del
   ecosistema caduca por diseño, y leerla al final es leerla con todo el criterio ya formado.
6. Cada documento trae su envoltura con **«Antes de leer»** y los enlaces **Anterior/Siguiente**
   de esta misma ruta: perderse cuesta un clic. Quien quiera ver las dependencias completas, con
   las fronteras entre núcleos, va al grafo de [`Cohesion-Global.md`](Cohesion-Global.md).

## 4. Ruta de consulta: estoy acá, qué leo

Es el itinerario de referencia: la guía como obra de consulta, para quien llega con una necesidad
puntual y no quiere recorrer la fila entera. Cada fila dice dónde entrar y qué prerrequisito
mínimo conviene tener fresco (el documento de entrada lo declara igualmente al abrir).

| Estoy acá | Qué leo | Con qué apoyo |
| --- | --- | --- |
| Tengo que **elegir un modelo** para una tarea (ESC-01) | [11](11-Panorama-Y-Ecosistema-Abierto.md): tabla de referencia, licencias, *model cards* y criterio de selección | [08](08-Inferencia-Local.md) §memoria para saber si entra en mi equipo; [07](07-Evaluacion-Y-Metricas.md) para decidir con evidencia y no por *benchmark* ajeno |
| Quiero **montar y servir un modelo local** (ESC-02) | [08](08-Inferencia-Local.md): motores, GGUF, cuantización, montar/descargar/servir y el paso 0 del banco | [02](02-Arquitectura-Transformer.md) y [03](03-Del-Modelo-Base-Al-Asistente.md) si parámetros, contexto o base/*instruct* no dicen nada todavía |
| El modelo responde, pero **mal o distinto cada vez** | [03](03-Del-Modelo-Base-Al-Asistente.md): muestreo (temperatura, semilla), *prompting*, alucinación | [07](07-Evaluacion-Y-Metricas.md) si «mal» necesita medirse en vez de intuirse |
| Quiero **preguntarle a mis propios documentos** con fuentes (ESC-03) | [09](09-Bases-Vectoriales-Y-RAG.md): embeddings aplicados, índice vectorial, pipeline RAG y su diagnóstico por etapas | [01](01-Representacion-Del-Lenguaje.md) §embeddings y [08](08-Inferencia-Local.md) para el motor que genera |
| Quiero **adaptar un modelo a mi dominio** (ESC-04) | [06](06-Ajuste-Fino-Y-Alineacion.md) — que abre con la decisión previa *prompt / RAG / ajuste fino*, para no entrenar lo que un prompt resolvía | [04](04-Datos-Y-Corpus.md) para armar el dataset con derecho y sin contaminación; [07](07-Evaluacion-Y-Metricas.md) para probar que mejoró |
| Tengo que **evaluar o comparar modelos con evidencia** (ESC-05) | [07](07-Evaluacion-Y-Metricas.md): métricas, *benchmarks* y su caducidad, LLM-juez, eval propio con protocolo | [03](03-Del-Modelo-Base-Al-Asistente.md) §muestreo: sin temperatura y semilla fijadas no hay corridas comparables |
| Quiero trabajar con **imagen o audio** (ESC-06) | [10](10-Multimodalidad-Y-Generacion-No-Textual.md): VLM, audio, difusión — qué cambia respecto del *transformer* | [02](02-Arquitectura-Transformer.md) si la atención todavía es una caja negra |
| Quiero **integrar el modelo en una aplicación** (ESC-07) | La guía de integración del repositorio: [`Dot-Net-Integration-AI-Layer-Guide`](../Dot-Net-Integration-AI-Layer-Guide/Dot-Net-Integration-AI-Layer-Guide.md) — esta guía no la duplica | [08](08-Inferencia-Local.md) §API y métricas de servicio, para hablar con el motor desde código |
| **No entiendo un término** o veo dos grafías del mismo | [M1](Marco-De-Referencia.md) §5 glosario bilingüe y §6 normalización de grafías | — |
| **Una cifra de la guía no coincide** con lo que veo hoy en la web | [11](11-Panorama-Y-Ecosistema-Abierto.md) (capa volátil: ganó la web) y [M3](Convenciones.md) §6 política de vigencia | El frontmatter `last_review` del documento dice de cuándo es la foto |
| Quiero **reproducir un laboratorio** y algo no da igual | [M3](Convenciones.md) §3 (patrón de ejemplo operable) y §4 (bloque «protocolo»): ahí está qué debía quedar fijado para que diera igual | [08](08-Inferencia-Local.md) paso 0, si la duda es si el banco responde |
| Quiero saber **por qué la guía está organizada así** o dónde termina un núcleo y empieza otro | [M2](Cohesion-Global.md): grafo, fronteras y lecturas asentadas del pedido | [M3](Convenciones.md) §7, plantilla que todos los temáticos cumplen |

## 5. Cómo leer esta guía

- **Los documentos son autocontenidos y no-delta** (definición operativa en
  [`Cohesion-Global.md`](Cohesion-Global.md)): cada uno se sigue de punta a punta sin abrir otro —
  los prerrequisitos se declaran al abrir, no se descubren a mitad de página — y ninguno exige
  haber leído una versión anterior de sí mismo.
- **Los laboratorios existen una sola vez.** Cada ejemplo operable tiene un documento dueño; los
  demás lo citan por enlace. Si un documento remite a un laboratorio ajeno, ése es el camino, no
  una omisión.
- **Lo operable y lo ilustrativo están marcados.** Un ejemplo operable trae entorno, versiones
  fijadas y salida esperada ([M3](Convenciones.md) §3); lo que el entorno de referencia no
  reproduce va marcado «ilustrativo». Toda cifra medida trae su bloque «protocolo»
  ([M3](Convenciones.md) §4).
- **La vigencia se lee en `last_review`.** La mecánica (01–10) envejece lento; el panorama (11) es
  la capa volátil y caduca por diseño ([M3](Convenciones.md) §6).
- **Para agentes de IA**: el frontmatter de cada pieza (`doc_id`, `doc_type`, `traces`,
  `audience`) es la interfaz de carga; el grafo de [`Cohesion-Global.md`](Cohesion-Global.md)
  dice qué cargar antes de qué, y esta portada, por dónde entrar según la tarea (§4).

## 6. Referencias

- Expediente 0001 de la guía, folio 011 — veredicto del jurado: §2 temario consolidado y ruta de
  lectura, §3 marco de referencia aprobado, §4 estructura documental aprobada. (Decisión del caso;
  se cita según [`Convenciones.md`](Convenciones.md) §2.2.)
- Las referencias de industria y academia viven en la sección «Referencias» de cada documento
  temático; esta portada no introduce afirmaciones técnicas propias.
