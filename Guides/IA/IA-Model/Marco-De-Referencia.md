---
doc_id: IAM-M1
doc_type: marco-de-referencia
title: Marco de referencia — escenarios, contextos, actores y glosario de la guía de modelos de IA
status: borrador
origin: ia-generated
confidence: media
owner: ACT-06
last_review: 2026-11-07
audience: [humano, agente]
traces: [IAM-M0, IAM-M2, IAM-M3]
---

# Marco de referencia — escenarios, contextos, actores y glosario

## Índice

1. [Propósito y cómo usar este marco](#1-propósito-y-cómo-usar-este-marco)
2. [Catálogo de escenarios (ESC)](#2-catálogo-de-escenarios-esc)
3. [Catálogo de contextos (CTX)](#3-catálogo-de-contextos-ctx)
4. [Catálogo de actores (ACT)](#4-catálogo-de-actores-act)
5. [Glosario bilingüe](#5-glosario-bilingüe)
6. [Normalización de grafías y convención de idioma](#6-normalización-de-grafías-y-convención-de-idioma)
7. [Referencias](#7-referencias)

---

## 1. Propósito y cómo usar este marco

Este documento fija el vocabulario común de la guía de estudio sobre modelos de IA: los catálogos
de **escenarios** (qué quiere lograr el lector), **contextos** (en qué entorno material lo hace) y
**actores** (quién decide qué), el **glosario bilingüe** con la forma canónica única de cada
término, y la **tabla de normalización de grafías**. Todos los documentos temáticos (`01-…` a
`11-…`) usan estos identificadores y estas formas canónicas; ninguno los redefine.

Los catálogos no son contenido original de este documento: transcriben, fieles, los catálogos
aprobados en el veredicto del caso que originó esta guía (expediente 0001 de la guía, folio 011
§3). Las marcas entre paréntesis del tipo «fila N del lote» o los identificadores de hallazgo
(`FP-06`, `H-EDI-03`, …) referencian decisiones de ese expediente: son la trazabilidad de cada
elección, no requisitos para el lector. Quien estudia la guía puede ignorarlas; quien la mantiene
(ACT-06) las necesita para saber por qué cada cosa es como es.

Cómo se usan los catálogos en los documentos temáticos: la sección «Aplicación por escenario» de
cada documento recorre los ESC que le competen y marca qué cambia por CTX; los ejemplos operables
declaran en qué CTX corren; los laboratorios y decisiones se narran desde el actor que decide
(ACT). Las dos rutas de lectura de la guía (formativa y de consulta) no son escenarios del dominio:
viven en la raíz (`IA-Model-Guide.md`) y en el documento de cohesión (`Cohesion-Global.md`)
(decisión H-EDI-03 del caso).

---

## 2. Catálogo de escenarios (ESC)

Un escenario es un objetivo del lector frente a los modelos: qué quiere lograr, no dónde ni con
qué. Catálogo aprobado (folio 011 §3 del caso):

| ID | Escenario |
| --- | --- |
| ESC-01 | Entender y elegir un modelo para una tarea (ficha/informe técnico, licencia, hardware) |
| ESC-02 | Montar y servir un modelo localmente |
| ESC-03 | Preguntarle a un corpus propio: indexar y responder con fuentes (RAG) |
| ESC-04 | Adaptar un modelo a un dominio (ajuste fino, con la decisión prompt/RAG/ajuste antes) |
| ESC-05 | Evaluar, comparar y decidir con evidencia |
| ESC-06 | Trabajar con más de una modalidad (imagen, audio) |
| ESC-07 | Integrar el modelo en una aplicación — referencia a la guía 16 del repositorio (integración .NET), no se duplica |

«Primer contacto» y «consulta puntual de referencia», propuestos en la ronda editorial del caso, no
son escenarios del dominio sino las dos rutas de lectura: viven en `Cohesion-Global.md` y en
`IA-Model-Guide.md` (decisión H-EDI-03).

---

## 3. Catálogo de contextos (CTX)

Un contexto es el entorno material donde el escenario ocurre: hardware, red y restricciones de
operación. Catálogo aprobado (folio 011 §3 del caso):

| ID | Contexto |
| --- | --- |
| CTX-01 | Equipo local sólo CPU (8–16 GB RAM): piso del lector; todo ejemplo base corre acá (default de la fila 1 del lote del caso) |
| CTX-02 | Equipo con GPU de consumo |
| CTX-03 | Servidor compartido en red local (el banco de ensayo del caso; `OLLAMA_HOST` parametrizado) |
| CTX-04 | Modelo consumido por API de terceros (sin pesos propios) |
| CTX-05 | Datos propios o sensibles: operación sin salida a internet |
| CTX-06 | Contenedor / CI: reproducibilidad, modelos como artefactos versionados |

El default de hardware de toda la guía es CTX-01 (CPU x86-64, 16 GB de RAM): cada ejemplo operable
se declara reproducible en CTX-01 o marca explícitamente el contexto que exige. Los ejemplos que
pegan contra un servidor de inferencia usan `http://localhost:11434` o la variable `$OLLAMA_HOST`;
la guía no publica direcciones de ningún host real.

---

## 4. Catálogo de actores (ACT)

Un actor se define por lo que **decide**, no por su cargo. Catálogo aprobado (folio 011 §3 del
caso):

| ID | Actor |
| --- | --- |
| ACT-01 | Estudiante-lector: sin conocimiento previo de IA, con terminal y programación básica (supuesto declarado; fila 3 del lote del caso); en ESC-04 asume el rol de ajustador bajo el protocolo de `Convenciones.md` |
| ACT-02 | Desarrollador integrador: consume la API del motor; no decide qué modelo corre |
| ACT-03 | Operador del banco / infraestructura: decide recursos, versiones del motor y modelos disponibles |
| ACT-04 | Evaluador: diseña y corre los evals; decide el veredicto comparativo con evidencia |
| ACT-05 | Curador de datos: decide qué entra al corpus/índice, su troceo, licencias y procedencia |
| ACT-06 | Mantenedor de la guía: decide vigencia, `last_review` y la capa volátil |

Descartes con motivo (también decisión del caso): «docente/tutor» y «agente de IA lector» no
deciden sobre el dominio — el agente lector es audiencia del RuleSet (doble audiencia), no actor;
«entrenador/ajustador» se funde en ACT-01 actuando en ESC-04 (en una guía de estudio quien ajusta
es el lector practicando); «responsable de datos» y «curador del corpus» se funden en ACT-05
(misma pregunta: qué dato entra y con qué derecho).

---

## 5. Glosario bilingüe

### 5.1 Reglas del glosario

Estas reglas salen de las decisiones EM-09, FP-06, FP-07 y H-EDI-08 del caso:

1. **Forma canónica única.** Cada término tiene una sola forma canónica en toda la guía: el término
   inglés de industria o su traducción elegida — una sola de las dos, nunca ambas alternándose.
   La cursiva es obligatoria en dos lugares exactos: la **primera aparición** del término en cada
   documento (que puede acompañarse con la otra lengua entre paréntesis) y las **definiciones**
   (las filas de esta tabla y la definición normativa del documento dueño). En el resto de la
   prosa la grafía va sin marca tipográfica obligatoria, pero única: nunca dos formas alternándose
   («dataset» y «data set», «chunk» y «fragmento») en la misma pieza. El código, las salidas
   transcriptas y los campos de API quedan exentos (§6.2). *Condición de revisión* (decisión
   R2-EDI-01 del expediente): si una ronda posterior mide confusión real de lectores entre
   formas, esta regla se endurece a cursiva en toda aparición; endurecer es aditivo.
2. **Definición en español, con fuente.** Toda entrada lleva definición en español y fuente
   verificable (autor-año; la referencia completa en §7). Las definiciones normativas extensas
   viven una sola vez, en el documento dueño del término; el glosario da la definición de una
   línea y reenvía.
3. **Regla de corte.** Entra al glosario el término que algún núcleo (documento 01–11) usa. Un
   término sin núcleo que lo use señala o un hueco del temario o un descarte, y se decide, no se
   acumula (decisión FP-07).
4. **Glosario inicial, no cerrado.** Esta tabla arranca con los términos que las comisiones del
   caso declararon imprescindibles (informes 005–009 del expediente). Los redactores temáticos
   agregan entradas conforme a las reglas 1–3; el mantenedor (ACT-06) consolida.
5. **Siglas.** Toda sigla se expande en su primera aparición en cada documento (decisión ARQIA-03).

### 5.2 Tabla de términos

Columnas: forma canónica (la que se escribe en la guía) · equivalente en la otra lengua ·
definición de una línea · fuente · documento dueño (donde vive la definición normativa; «docs» es
relativo a esta carpeta).

| Forma canónica | Equivalente | Definición breve | Fuente | Dueño |
| --- | --- | --- | --- | --- |
| *token* | — (sin traducción asentada) | Unidad mínima en que el modelo segmenta el texto; no coincide con la palabra | Sennrich et al. (2016) | 01 |
| tokenizador | *tokenizer* | Componente que convierte texto en tokens y viceversa, con un vocabulario fijo | Sennrich et al. (2016) | 01 |
| vocabulario | *vocabulary* | Conjunto cerrado de tokens que un modelo conoce | Sennrich et al. (2016) | 01 |
| BPE | *Byte Pair Encoding* (codificación por pares de bytes) | Algoritmo de tokenización dominante: fusiona de forma iterativa los pares más frecuentes del corpus hasta un vocabulario prefijado | Sennrich et al. (2016) | 01 |
| subpalabra | *subword* | Fragmento de palabra en que el tokenizador parte lo poco frecuente; la unidad del vocabulario BPE | Sennrich et al. (2016) | 01 |
| SentencePiece | — (nombre propio de la herramienta) | Tokenizador de subpalabras que no depende de espacios como separador | Kudo y Richardson (2018) | 01 |
| *embedding* | — (sin traducción asentada) | Vector numérico denso que representa un texto (token, frase o documento) en un espacio donde la cercanía refleja afinidad semántica | Reimers y Gurevych (2019) | 01 |
| similitud coseno | *cosine similarity* | Medida de cercanía entre dos embeddings por el ángulo entre vectores | Reimers y Gurevych (2019) | 01 |
| ventana de contexto | *context window* | Cantidad máxima de tokens que el modelo procesa en una pasada (entrada + salida) | Vaswani et al. (2017) | 01 |
| *transformer* | — (nombre propio de la arquitectura) | Arquitectura de red basada en atención que domina el parque de modelos vigente | Vaswani et al. (2017) | 02 |
| atención | *attention* | Mecanismo por el que cada token pondera su relación con los demás de la ventana | Vaswani et al. (2017) | 02 |
| *decoder-only* | — | Variante del transformer que sólo genera texto token a token; la de los modelos de lenguaje actuales | Brown et al. (2020) | 02 |
| autoregresivo | *autoregressive* | Que predice el token siguiente condicionado a todos los anteriores | Brown et al. (2020) | 02 |
| máscara causal | *causal mask* | Restricción que anula la atención hacia posiciones futuras; lo que hace autoregresivo al decoder-only | Brown et al. (2020) | 02 |
| codificación de posición | *positional encoding* | Información de orden que se suma o aplica a las representaciones para que la atención distinga posiciones; el estándar de facto actual es RoPE (rotatoria) | Vaswani et al. (2017); Su et al. (2024) | 02 |
| GQA / MQA | *grouped-query / multi-query attention* | Variantes de atención que reducen las cabezas K/V compartidas para achicar la caché KV | Shazeer (2019); Ainslie et al. (2023) | 02 |
| MoE (mezcla de expertos) | *Mixture of Experts* | Variante que activa sólo una fracción de los parámetros por token | Fedus et al. (2022) | 02 |
| parámetro | *parameter / weight* | Número ajustable del modelo; su cantidad (en miles de millones, «B») mide el tamaño | Brown et al. (2020) | 02 |
| modelo base | *base model* | Modelo sólo preentrenado: completa texto, no conversa | Ouyang et al. (2022) | 03 |
| modelo *instruct* | *instruct/chat model* | Modelo base alineado para seguir instrucciones y dialogar | Ouyang et al. (2022) | 03 |
| *prompt* | — (sin traducción asentada) | Entrada textual con la que se instruye al modelo; *prompting*: la práctica de diseñarla | Brown et al. (2020) | 03 |
| *prompt* de sistema | *system prompt* | Zona del prompt que fija quién es el asistente y las reglas fijas que rigen toda la conversación (el «rol de sistema» de las tres zonas) | Brown et al. (2020) | 03 |
| *few-shot* | — | Técnica de prompting que incluye ejemplos resueltos en la entrada | Brown et al. (2020) | 03 |
| muestreo | *sampling* | Elección probabilística del token siguiente; la temperatura regula su aleatoriedad | Brown et al. (2020) | 03 |
| alucinación | *hallucination* | Salida fluida pero sin sustento en los datos ni en el contexto provisto | Ji et al. (2023) | 03 |
| corpus | *corpus* | Conjunto de texto crudo del que un modelo aprende; se mide en tokens y su composición explica lo que el modelo sabe e ignora | Gao et al. (2020); Penedo et al. (2024) | 04 |
| *dataset* | conjunto de datos | Colección estructurada de ejemplos para entrenar o evaluar | Lee et al. (2022) | 04 |
| contaminación | *contamination* | Presencia de datos de evaluación dentro del corpus de entrenamiento; invalida la métrica | Lee et al. (2022) | 04 |
| deduplicación | *deduplication* | Limpieza que elimina repeticiones del corpus; mejora el modelo resultante | Lee et al. (2022) | 04 |
| procedencia | *provenance* | Cadena documentada que va de cada dato a su origen: de dónde salió, con qué licencia, por qué transformaciones pasó | Gebru et al. (2021) | 04 |
| JSONL | *JSON Lines* | Formato de dataset: un objeto JSON válido por línea, en UTF-8; la *lingua franca* del ajuste fino | JSON Lines (s. f.) | 04 |
| *datasheet* | — (sin traducción asentada) | Documento que declara origen, composición, licencias y usos previstos de un dataset; el simétrico de la *model card* | Gebru et al. (2021) | 04 |
| preentrenamiento | *pre-training* | Entrenamiento inicial a gran escala con objetivo autoregresivo sobre corpus masivo | Brown et al. (2020) | 05 |
| pérdida | *loss* | Función que mide el error del modelo durante el entrenamiento; entrenar es minimizarla | Bengio et al. (2003) | 05 |
| perplejidad | *perplexity* | Métrica derivada de la pérdida: cuán «sorprendido» queda el modelo ante un texto; menor es mejor | Jelinek et al. (1977) | 05 |
| *checkpoint* | — (sin traducción asentada) | Fotografía de los parámetros del modelo en un punto del entrenamiento | Hoffmann et al. (2022) | 05 |
| leyes de escala | *scaling laws* | Relaciones empíricas entre cómputo, datos, parámetros y calidad resultante | Kaplan et al. (2020) | 05 |
| ajuste fino | *fine-tuning* | Entrenamiento posterior de un modelo ya preentrenado sobre datos de una tarea o dominio | Hu et al. (2022) | 06 |
| SFT (ajuste fino supervisado) | *supervised fine-tuning* | Ajuste fino con pares instrucción→respuesta | Ouyang et al. (2022) | 06 |
| LoRA | *Low-Rank Adaptation* | Ajuste fino eficiente que entrena matrices chicas adosadas, sin tocar los pesos originales | Hu et al. (2022) | 06 |
| QLoRA | — | LoRA sobre un modelo cuantizado; permite ajustar en hardware modesto | Dettmers et al. (2023) | 06 |
| *safetensors* | — (formato de archivo) | Formato en que Hugging Face distribuye los pesos sin cuantizar; el insumo de la conversión a GGUF | Hugging Face (s. f.) | 06 |
| RLHF | *Reinforcement Learning from Human Feedback* | Alineación por refuerzo guiado por preferencias humanas | Ouyang et al. (2022) | 06 |
| modelo de recompensa | *reward model* | Modelo auxiliar que puntúa respuestas según preferencia humana; guía el RLHF | Ouyang et al. (2022) | 06 |
| DPO | *Direct Preference Optimization* | Alineación por preferencias sin modelo de recompensa explícito | Rafailov et al. (2023) | 06 |
| destilación | *distillation* | Entrenar un modelo chico para imitar la salida de uno grande | Hinton et al. (2015) | 11 |
| *benchmark* | — (sin traducción asentada) | Batería estandarizada de pruebas para comparar modelos | Hendrycks et al. (2021) | 07 |
| LLM-juez | *LLM-as-a-judge* | Uso de un modelo como evaluador de las salidas de otro; exige contraste con juicio humano | Zheng et al. (2023) | 07 |
| *eval* | evaluación propia | Batería de pruebas diseñada por el propio equipo para su tarea, con protocolo reproducible | Chiang et al. (2024) | 07 |
| inferencia | *inference* | Ejecutar un modelo ya entrenado para obtener salidas | Qwen Team (2025) | 08 |
| motor de inferencia | *inference engine / runtime* | Software que carga los pesos y sirve el modelo (llama.cpp, Ollama, vLLM) | ggml-org (s. f.-b); Ollama (s. f.); Kwon et al. (2023) | 08 |
| cuantización | *quantization* | Reducción de la precisión numérica de los pesos para bajar memoria y acelerar, con pérdida medible | Dettmers et al. (2023) | 08 |
| GGUF | — (formato de archivo) | Formato de pesos cuantizados del ecosistema llama.cpp; el que consume Ollama | ggml-org (s. f.-a) | 08 |
| caché KV | *KV cache* | Memoria de claves/valores de atención ya calculados; domina el costo en ventanas largas | Vaswani et al. (2017); Pope et al. (2023) | 08 |
| tokens por segundo | *tokens/s* | Métrica de servicio: velocidad de generación de un despliegue concreto | Ollama (s. f.) | 08 |
| base vectorial | *vector database* | Almacén que indexa embeddings para búsqueda de vecinos a escala | Malkov y Yashunin (2020) | 09 |
| ANN (vecinos aproximados) | *approximate nearest neighbor* | Búsqueda de vecinos que sacrifica exactitud por velocidad | Malkov y Yashunin (2020) | 09 |
| HNSW | *Hierarchical Navigable Small World* | Índice de grafos jerárquicos, el estándar de facto de los motores vectoriales | Malkov y Yashunin (2020) | 09 |
| *recall* | — (en recuperación ANN) | Fracción de los resultados verdaderos que la búsqueda aproximada efectivamente devuelve | Malkov y Yashunin (2020) | 09 |
| RAG | *Retrieval-Augmented Generation* | Pipeline que recupera pasajes de un corpus propio y los inyecta al prompt antes de generar | Lewis et al. (2020) | 09 |
| *chunk* | fragmento | Trozo de documento indexado como unidad en la base vectorial; trocear: *chunking* | Lewis et al. (2020) | 09 |
| *top-k* | — | Cantidad de pasajes recuperados que se inyectan al contexto | Lewis et al. (2020) | 09 |
| multimodalidad | *multimodality* | Capacidad de un modelo de operar sobre más de un tipo de dato (texto, imagen, audio) | Radford et al. (2021) | 10 |
| VLM (modelo de visión y lenguaje) | *vision-language model* | Modelo que acepta imágenes y texto en la misma conversación | Bai et al. (2025) | 10 |
| encoder de visión | *vision encoder* | Componente que corta la imagen en parches y produce un vector por parche; típicamente un ViT (*Vision Transformer*) | Dosovitskiy et al. (2021) | 10 |
| espectrograma | *spectrogram* | Representación del audio como imagen de tiempo contra frecuencia; la entrada del camino ASR | Radford et al. (2023) | 10 |
| ASR (reconocimiento automático del habla) | *Automatic Speech Recognition* | Transcripción de audio a texto, token a token, a partir del espectrograma | Radford et al. (2023) | 10 |
| códec neuronal | *neural codec* | Compresor aprendido que convierte la onda de audio en tokens discretos que un modelo de lenguaje puede generar | Défossez et al. (2022) | 10 |
| difusión | *diffusion* | Familia generativa de imágenes por eliminación progresiva de ruido; no es un transformer autoregresivo | Rombach et al. (2022) | 10 |
| pesos abiertos | *open weights* | Modelo cuyos pesos se descargan y ejecutan localmente, con la licencia que sea; **no** implica *open source* | OSI (2024) | 11 |
| *open source* (aplicado a modelos) | código abierto | Según la OSAID 1.0, exige pesos con licencia aprobada, código del pipeline **e** información suficientemente detallada sobre los datos de entrenamiento; la mayoría del parque «abierto» no califica | OSI (2024) | 11 |
| *model card* | ficha de modelo | Documento estandarizado que declara usos previstos, datos, métricas y límites de un modelo | Mitchell et al. (2019) | 11 |

La distinción pesos abiertos / open source es decisión FP-06 del caso: la guía usa «pesos
abiertos» como término preciso para el parque de modelos descargables, y reserva *open source*
para lo que cumple la definición de la OSI; el documento `11-Panorama-Y-Ecosistema-Abierto.md`
desarrolla la distinción por familia y licencia.

---

## 6. Normalización de grafías y convención de idioma

### 6.1 Tabla de normalización de grafías

El pedido que originó esta guía contiene erratas que son literales en el expediente (el testimonio
no se corrige) pero que **no deben filtrarse** a títulos, nombres de archivo ni prosa de la guía
(decisión H-EDI-08 del caso). Forma normalizada obligatoria:

| Grafía vista (testimonio u otras fuentes) | Forma normalizada en la guía | Nota |
| --- | --- | --- |
| «qween», «qwen» (en prosa) | Qwen | Nombre propio de la familia de modelos; mayúscula inicial. En identificadores técnicos rige la etiqueta literal del motor (p. ej. `qwen3-coder:30b`) |
| «open sorce» | *open source* | Y sólo cuando aplica según la OSAID 1.0; si se habla del parque descargable, el término preciso es «pesos abiertos» (§5.2) |
| «borrado» (por el estado editorial) | borrador | Estado `status: borrador` del frontmatter de la casa |
| «a aquien a un estudiante» | a quien no conoce del tema, como un estudiante | Lectura asentada en el caso (decisión FP-02): el supuesto de lector es ACT-01 |
| «destokenizar», «de-tokenizar» | des-tokenizar | Verbo acuñado por la guía para la conversión inversa tokens → texto (doc 01 §1.2); forma única con guion. Es grafía asentada acá, no entrada del glosario |
| «no determinismo» | no-determinismo | Sustantivo con guion, forma única en la guía (la que usa el doc 03); como predicado se escribe «no es determinista» |

La frase trunca del pedido («…modelos open sorce con el modelo.») quedó registrada en el caso como
ilegible y se trata como descriptiva, duplicada por la frase siguiente (decisión ARQIA-08 y fila 5
del lote del caso): ningún documento de la guía le asigna función inventada.

### 6.2 Convención de idioma

Rige la convención ya vigente en la casa: **patrón en inglés, dominio en español**. Aplicada a esta
guía:

- La prosa, las definiciones y las explicaciones van en español.
- Los términos técnicos de industria sin traducción asentada van en inglés, en cursiva, con su
  forma canónica del glosario (§5.2): *token*, *embedding*, *transformer*, *prompt*, *benchmark*,
  *checkpoint*, *chunk*…
- Donde la traducción está asentada y el glosario la eligió como canónica, se usa la traducción:
  ajuste fino, cuantización, perplejidad, ventana de contexto, pérdida, pesos abiertos.
- Los términos de las APIs y sus campos JSON se escriben literales y no se traducen (`model`,
  `eval_count`, `temperature`, nombres de endpoints como `/api/tags`).
- Las siglas (RAG, LoRA, DPO, RLHF, MoE, VLM, ANN, HNSW, SFT) se expanden en su primera aparición
  en cada documento y después se usan solas.

---

## 7. Referencias

Citado autor-año (APA 7.ª edición) conforme a `Convenciones.md`. Consultas web del caso:
2026-11-07.

- Ainslie, J., Lee-Thorp, J., de Jong, M., Zemlyanskiy, Y., Lebrón, F., y Sanghai, S. (2023). GQA: Training generalized multi-query transformer models from multi-head checkpoints. *EMNLP*. arXiv:2305.13245
- Bai, S., et al. (2025). *Qwen2.5-VL Technical Report*. arXiv:2502.13923. https://arxiv.org/abs/2502.13923
- Bengio, Y., Ducharme, R., Vincent, P., y Jauvin, C. (2003). A neural probabilistic language model. *Journal of Machine Learning Research, 3*, 1137–1155.
- Brown, T., et al. (2020). Language models are few-shot learners. *NeurIPS 33*. arXiv:2005.14165
- Chiang, W.-L., et al. (2024). Chatbot Arena: An open platform for evaluating LLMs by human preference. arXiv:2403.04132
- Défossez, A., Copet, J., Synnaeve, G., y Adi, Y. (2022). High fidelity neural audio compression. arXiv:2210.13438
- Dettmers, T., Pagnoni, A., Holtzman, A., y Zettlemoyer, L. (2023). QLoRA: Efficient finetuning of quantized LLMs. arXiv:2305.14314
- Dosovitskiy, A., et al. (2021). An image is worth 16x16 words: Transformers for image recognition at scale. *ICLR*. arXiv:2010.11929
- Fedus, W., Zoph, B., y Shazeer, N. (2022). Switch Transformers: Scaling to trillion parameter models with simple and efficient sparsity. *JMLR, 23*
- Gao, L., et al. (2020). The Pile: An 800GB dataset of diverse text for language modeling. arXiv:2101.00027
- Gebru, T., et al. (2021). Datasheets for datasets. *Communications of the ACM, 64*(12). arXiv:1803.09010
- ggml-org. (s. f.-a). *GGUF file format specification*. GitHub. https://github.com/ggml-org/ggml/blob/master/docs/gguf.md
- ggml-org. (s. f.-b). *llama.cpp: LLM inference in C/C++*. GitHub. https://github.com/ggml-org/llama.cpp
- Hendrycks, D., et al. (2021). Measuring massive multitask language understanding. *ICLR*. arXiv:2009.03300
- Hinton, G., Vinyals, O., y Dean, J. (2015). Distilling the knowledge in a neural network. arXiv:1503.02531
- Hoffmann, J., et al. (2022). Training compute-optimal large language models. arXiv:2203.15556
- Hu, E. J., et al. (2022). LoRA: Low-rank adaptation of large language models. *ICLR*. arXiv:2106.09685
- Hugging Face. (s. f.). *Safetensors — documentación oficial*. https://huggingface.co/docs/safetensors (consultada el 2026-11-07)
- Jelinek, F., Mercer, R. L., Bahl, L. R., y Baker, J. K. (1977). Perplexity—a measure of the difficulty of speech recognition tasks. *Journal of the Acoustical Society of America, 62*(S1).
- Ji, Z., et al. (2023). Survey of hallucination in natural language generation. *ACM Computing Surveys, 55*(12). https://doi.org/10.1145/3571730
- JSON Lines. (s. f.). *JSON Lines format specification*. https://jsonlines.org (consultada el 2026-11-07)
- Kaplan, J., et al. (2020). Scaling laws for neural language models. arXiv:2001.08361
- Kudo, T., y Richardson, J. (2018). SentencePiece: A simple and language independent subword tokenizer and detokenizer for neural text processing. *EMNLP (System Demonstrations)*. arXiv:1808.06226
- Kwon, W., Li, Z., Zhuang, S., Sheng, Y., Zheng, L., Yu, C. H., Gonzalez, J. E., Zhang, H., y Stoica, I. (2023). Efficient memory management for large language model serving with PagedAttention. *SOSP '23*. arXiv:2309.06180
- Lee, K., et al. (2022). Deduplicating training data makes language models better. *ACL*. arXiv:2107.06499
- Lewis, P., et al. (2020). Retrieval-augmented generation for knowledge-intensive NLP tasks. *NeurIPS 33*. arXiv:2005.11401
- Malkov, Y. A., y Yashunin, D. A. (2020). Efficient and robust approximate nearest neighbor search using Hierarchical Navigable Small World graphs. *IEEE TPAMI, 42*(4)
- Mitchell, M., et al. (2019). Model cards for model reporting. *FAT\**. arXiv:1810.03993
- Ollama. (s. f.). *Documentación oficial — referencia de la API*. https://docs.ollama.com (consultada el 2026-11-07)
- Open Source Initiative. (2024). *The Open Source AI Definition 1.0*. https://opensource.org/ai/open-source-ai-definition
- Ouyang, L., et al. (2022). Training language models to follow instructions with human feedback. *NeurIPS 35*. arXiv:2203.02155
- Penedo, G., et al. (2024). The FineWeb datasets: Decanting the web for the finest text data at scale. arXiv:2406.17557
- Pope, R., Douglas, S., Chowdhery, A., Devlin, J., Bradbury, J., Heek, J., Xiao, K., Agrawal, S., y Dean, J. (2023). Efficiently scaling transformer inference. *MLSys*. arXiv:2211.05102
- Qwen Team. (2025). *Qwen3 Technical Report*. arXiv:2505.09388
- Radford, A., et al. (2021). Learning transferable visual models from natural language supervision. *ICML*. arXiv:2103.00020
- Radford, A., Kim, J. W., Xu, T., Brockman, G., McLeavey, C., y Sutskever, I. (2023). Robust speech recognition via large-scale weak supervision. *ICML*. arXiv:2212.04356
- Rafailov, R., et al. (2023). Direct preference optimization: Your language model is secretly a reward model. *NeurIPS 36*. arXiv:2305.18290
- Reimers, N., y Gurevych, I. (2019). Sentence-BERT: Sentence embeddings using Siamese BERT-networks. *EMNLP*. arXiv:1908.10084
- Rombach, R., et al. (2022). High-resolution image synthesis with latent diffusion models. *CVPR*. arXiv:2112.10752
- Sennrich, R., Haddow, B., y Birch, A. (2016). Neural machine translation of rare words with subword units. *ACL*. arXiv:1508.07909
- Shazeer, N. (2019). Fast transformer decoding: One write-head is all you need. arXiv:1911.02150
- Su, J., Ahmed, M., Lu, Y., Pan, S., Bo, W., y Liu, Y. (2024). RoFormer: Enhanced transformer with rotary position embedding. *Neurocomputing, 568*. arXiv:2104.09864
- Vaswani, A., et al. (2017). Attention is all you need. *NeurIPS 30*. arXiv:1706.03762
- Zheng, L., et al. (2023). Judging LLM-as-a-judge with MT-Bench and Chatbot Arena. *NeurIPS 36*. arXiv:2306.05685
