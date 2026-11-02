---
doc_id: ANX-SDEI-GLOSARIO
doc_type: anexo
title: Anexo — glosario del sistema documentador
description: Términos y siglas del cuerpo con su definición, sus alias y dónde se ancla cada uno; un término, un significado.
status: borrador
origin: ia-assisted
confidence: alta
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-02
audience: [humano, agente, estudiante, docente, desarrollo, analista, po]
traces: [MARCO-SDEI, ANX-SDEI-ESQUEMA]
---

# Anexo — glosario

Un término, un significado. Cuando la industria usa otra palabra para lo mismo, figura como alias; el texto del cuerpo
usa siempre el término de la primera columna. La columna «Dónde se ancla» dice en qué documento se define con detalle
o qué control lo verifica.

## Índice

1. [Siglas de los controles](#1-siglas-de-los-controles)
2. [Términos](#2-términos)

## 1. Siglas de los controles

Hay seis familias de siglas parecidas. No se mezclan.

| Sigla | Qué nombra | Ejemplo |
| --- | --- | --- |
| `PR-1` a `PR-7` | Grupos de controles del carril PR de `docs-guard` | PR-5 es `traces-resolve` |
| `AI-1` a `AI-5` | Chequeos anti-inyección sobre el texto | AI-2 detecta caracteres invisibles |
| `AK-1`, `AK-3`, `AK-4`, `AK-5` y `AK-7` | Controles anti-inyección del sistema (excepciones con huella, lista de dominios, fixture permanente) | AK-7 es el fixture que tiene que dar rojo en cada corrida |
| `G1` a `G4` | Guarda de merge de la conformación automática del pull request | G3: todos los controles de la cabeza en éxito |
| `CQ-nn`, `PE-nn`, `VER-…` | Criterios de calidad, patrones de ejemplos e ítems de verificación | Ver sus anexos |
| `CL-01` a `CL-14` | Clases de pieza documental | CL-06 es la ayuda al usuario final |

## 2. Términos

| Término | Alias | Definición | Dónde se ancla |
| --- | --- | --- | --- |
| Actor | — | Rol que interviene en el sistema documentador, con lo que le toca decidir y lo que no: `ACT-SDEI-1` a `ACT-SDEI-6`. No confundir con el rol de un producto | [Marco, actores](../00-Marco.md#4-actores) |
| Agente de IA | Agente | Programa basado en un modelo de lenguaje que lee y escribe en el repositorio. Propone trabajo de clase P | [Marco, actores](../00-Marco.md#4-actores) |
| `AGENTS.md` | Archivo de contexto | Archivo en la raíz de un repositorio que orienta a los agentes: dónde está el README del cuerpo, las convenciones y los anexos. Es clase H | CL-03 en el [Mapa](../01-Mapa.md#4-por-artefacto) |
| Ancla de versión | — | El dato que dice contra qué estado del código vale una pieza: el commit en el mismo repositorio, un SHA fijado (`base_commit`, `source.lock`) entre repositorios, o una etiqueta | [Marco](../00-Marco.md#1-definiciones); carril programado (atraso) |
| Atraso | — | Cantidad de commits que el SHA fijado (`base_commit`, `sha` de `source.lock`) está detrás de la rama principal del repositorio que fija; un entero ≥ 0. Avisa, no bloquea | Control 3 (`atraso`) de `tools/docs-guard/semanal.sh`; carril programado |
| Audiencia | — | Conjunto de lectores a los que se dirige una pieza, definido por sus necesidades y sus tareas. Orienta qué se escribe y cómo; **no da ni quita acceso**: eso lo hace la regla de entrega. No confundir con lector (persona o agente), rol (perfil del producto), persona (arquetipo de UX) ni actor (`ACT-SDEI-n`). «Audiencia primaria o secundaria» va como nota, no como campo. Adaptada de ISO/IEC/IEEE 26514:2022 §3.1.7, **sin verificar** contra la edición publicada | `audience` en el [Esquema](Esquema-Frontmatter-Y-Prefijos.md#2-los-diez-campos-más-description); [Marco, §2.5](../00-Marco.md#25-qué-cambia-entre-escenarios) (audiencias típicas) |
| Canal derivado | — | Salida generada para agentes a partir del sitio publicado: una página `.md` por página, `llms.txt`. Sólo existe si hay sitio | CL-14; `CTX-SDEI-1` |
| Capa | Capa de microtexto | Cada nivel del microtexto de un campo, de un vocabulario cerrado: `etiqueta · pista · info · ejemplo · validacion` | Ver microtexto |
| Carril PR | — | Conjunto de controles que corre en cada pull request y bloquea | `docs-guard`, grupos PR-1 a PR-7 |
| Carril programado | — | Corrida semanal que sólo avisa: enlaces externos, atraso de anclas, frescura. Abre o actualiza un issue | `docs-enlaces-semanal` |
| Cita entre repositorios | — | Forma `Repo@ancla:corpus_id/doc_id` con enlace permanente al SHA. Dentro del cuerpo, en cambio, se usan enlaces relativos | [Convenciones](../00-Marco.md#5-convenciones) |
| Clase de intervención | D/P/H | D: la produce un generador determinista. P: la propone un agente o una persona y entra por la compuerta. H: sólo una persona | [Cómo decidir](Como-Decidir.md#6-quién-decide) |
| Clase de pieza | `CL-nn` | Categoría cerrada del catálogo de catorce clases: fija dónde vive la pieza, su ancla, su control y su tema | [Mapa](../01-Mapa.md#4-por-artefacto) |
| Comportamiento | — | Lo que el código hace (en la mini demo, el campo admite 80 caracteres). Con la letra y la intención, ubica una divergencia | `TEM-01`; ficha `RN-LOC-01` de la mini demo |
| Compuerta | Gate | El carril PR: el conjunto de controles que corre en cada pull request y bloquea. En este cuerpo, `docs-guard`, con los grupos PR-1 a PR-7 más `autoprueba`. Un control suelto no es una compuerta | `docs-guard` |
| Compuerta de escalón | — | Hito de cierre de un escalón: la revisión que lo da por cumplido y deja pasar al siguiente | `TEM-07` (escalón) |
| Conformación automática del pull request | CPA | El agente abre el pull request, publica el informe con el enlace, espera la guarda G1–G4 y fusiona con commit de merge. Vale sólo con la delegación asentada por escrito | [Cómo decidir](Como-Decidir.md#6-quién-decide) |
| Contexto | — | Variante que cruza los escenarios y cambia una respuesta concreta. No confundir con el identificador de contexto (`contexto:`) | [Marco](../00-Marco.md#3-contextos) |
| Control | Check | Una comprobación de la CI que termina con un código de salida: 0 (en orden o con avisos) o 1 (bloquea), según la regla de bloqueo | `tools/docs-guard/dg.py`; [Criterios de calidad](Criterios-De-Calidad.md#2-qué-automatiza-la-compuerta) |
| Copia fijada | Vendor | Copia de un conocimiento compartido, con su versión y su SHA-256 en `BASE-FIJADA.md`. Si el archivo cambia, la compuerta da rojo | CL-11; PR-7 |
| Cuerpo documental | Corpus | Conjunto de documentos de una guía con un README que declara `corpus_id` y `version` | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#3-readme-del-cuerpo-y-páginas-de-ayuda) |
| Deriva | Desactualización | Distancia entre lo que una pieza afirma y lo que el código hace; instrumento `traces-resolve`. No es el atraso del SHA fijado | `traces-resolve` (PR); A1 de la [mini demo](../Examples/MovilidadUrbana-Mini-Demo/README.md) (aviso) |
| Deriva de copia fijada | — | Copia fijada cuya huella SHA-256 ya no coincide con la que declara `BASE-FIJADA.md`. Es alias del control 4 (`deriva`) del carril programado hasta que el expediente de herramienta lo renombre | Control 4 de `tools/docs-guard/semanal.sh:15`; PR-7 |
| Dimensiones de entrega | — | Cuatro, ortogonales: **rol**, **tipo de alcance** (`generico` o `por-aplicacion`; los valores viven en el producto), **superficie** (panel, portal, app móvil, servicio, SDK) y **nivel de acceso**. Perfil mínimo: roles planos con jerarquía declarada, nivel y la regla `nivel-vs-repo`; lo demás se activa sólo si el inventario lo registra | [Marco, §2.5](../00-Marco.md#25-qué-cambia-entre-escenarios) (superficies); registro de audiencias |
| Divergencia | — | Diferencia entre la letra, la intención y el comportamiento. **Leve**: no cambia qué se acepta, qué se rechaza ni qué se guarda. **De fondo**: cambia alguna de esas tres cosas, como el 60/80 de la mini demo. La decide una persona (`ACT-SDEI-2`), acto de clase H | [Marco, actores](../00-Marco.md#4-actores); tutorial de la mini demo |
| Doble audiencia | — | Nombre anterior del tema de personas y agentes como lectores (`TEM-04`), que pasa a «Lectura por agentes». Queda sólo como remisión: el agente es lector, no audiencia | Ver lector; `TEM-04` |
| *Docs as code* | Documentación como código | Escribir la documentación en texto plano, versionarla en Git, revisarla por pull request y verificarla en la CI, con las mismas herramientas que el código | `TEM-01` |
| `docs-guard` | — | Workflow reutilizable de Lab-Documentos que corre los grupos PR-1 a PR-7 con un agregador estricto | `.github/workflows/docs-guard.yml` |
| Escala anclada | — | Escala de 1 a 5 donde cada punto tiene un observable | [Cómo decidir](Como-Decidir.md#3-la-escala-anclada) |
| Escalón | — | Etapa de prueba del sistema documentador con su propia compuerta de escalón: 1, esta guía y la mini demo; 2, la piloto en Lab-Geometria; 3, el SDD-B | [`TEM-07`](../TEM-07-Marco-SDD.md#definiciones) |
| Escenario | Tipo de sistema | Tipo de sistema que se documenta; el cuerpo tiene cuatro | [Marco](../00-Marco.md#2-escenarios) |
| Excepción con huella | — | Línea `sha256 ruta control` que exceptúa un archivo de un control mientras el archivo no cambie | `tools/docs-guard/excepciones.txt` |
| Ficha | — | Página corta que declara una regla o un caso de uso con su identificador, su versión y dónde se aplica en el código, con la forma `ruta#Símbolo@SHA` | [Marco](../00-Marco.md#1-definiciones) |
| Frontmatter | Metadatos | Bloque YAML al principio del archivo | [Esquema](Esquema-Frontmatter-Y-Prefijos.md) |
| Fuente de verdad | — | El lugar del que sale lo que una pieza afirma; si la pieza y la fuente discrepan, manda la fuente | [`TEM-01`](../TEM-01-Sistema-Documentador.md#qué-es) |
| Guarda de merge | G1–G4 | Comprobación que hace un agente antes de fusionar su propio pull request: abierto, sin conflictos, con todos los controles en verde y sin reserva. Vale sólo con la delegación asentada por escrito | [Marco](../00-Marco.md#1-definiciones) |
| Ia-db | Índice interno | Índice para agentes en el repositorio hermano `.Documentacion`, anclado con `base_commit`. Es un índice, no un marco SDD | CL-08 |
| Identificador de contexto | `contexto:` | `contexto: <app>/<id>` en el frontmatter de una página de ayuda: la pantalla o el campo que la pide. El mapa pantalla → página se deriva de esos campos. No confundir con contexto (`CTX-SDEI-n`) | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#3-readme-del-cuerpo-y-páginas-de-ayuda); control `contexto-resuelve` (avisa, sin implementar) |
| Integración | — | Dos acepciones que no se mezclan: en el ciclo del código, pull request más CI; en el producto, la ayuda embebida y el canal para agentes | `TEM-02` |
| Intención | — | Para qué existe una regla, lo que el producto debía hacer, y qué la respalda en el código | `TEM-01`; ficha `RN-LOC-01` de la mini demo |
| Jerarquía | `incluye:` | Orden entre roles que se declara una vez con `incluye:` en el registro de audiencias y que el build expande. Un enlace de un nivel bajo a uno alto es un cruce. Cambiarla es clase H | Registro de audiencias; control `cruce-de-audiencia` (avisa, sin implementar) |
| Lector | — | Persona o agente. Es dimensión de **canal**, no de acceso: el agente hereda la audiencia y el alcance de su usuario y nunca tiene acceso propio | `TEM-04` |
| Letra | — | Lo que la app promete a quien la usa, textual: el texto de ayuda bajo el campo y los mensajes («Entre 3 y 60 caracteres») | `TEM-01`; ficha `RN-LOC-01` de la mini demo |
| Línea de base | — | Cantidad de hallazgos de un control sobre lo que vigila antes de empezar a bloquear. Un control bloquea sólo con línea de base 0 | [Criterios de calidad](Criterios-De-Calidad.md) |
| Microtexto | — | Texto corto de un campo, en capas: `etiqueta · pista · info · ejemplo · validacion`. Desarrollo decide su clave y su ubicación; la letra la escribe una persona y los números se interpolan desde la regla | [Marco, actores](../00-Marco.md#4-actores) |
| Nivel de acceso | — | Vocabulario cerrado: `publico`, `producto` (con sesión en el producto), `restringido` (por invitación o credencial) e `interno`. Lo que viaja en un paquete instalable es sólo `publico` | [Marco, §2.5](../00-Marco.md#25-qué-cambia-entre-escenarios) (confidencialidad); control `nivel-vs-repo` (avisa, sin implementar) |
| P-LECT | Prueba de lectura | Un lector sin contexto recorre una ruta y anota las dudas que el texto no responde; el umbral es cero dudas sin respuesta | `CQ-04`; `PE-09` |
| Par rojo/verde | Prueba negativa | Un fixture con el defecto (el control da rojo) y otro sin él (da verde) | `tools/docs-guard/fixtures/` |
| Pieza documental | — | Unidad que se escribe, se versiona y se valida por separado: un caso de uso, un `AGENTS.md`, una página de ayuda, el `openapi.json` | [Mapa](../01-Mapa.md#4-por-artefacto) |
| Punto de aplicación | — | Lugar donde se hace cumplir el nivel de acceso: **repositorio** (visibilidad de la fuente), **build** (qué se empaqueta) y **servidor** (qué se entrega a cada token) | Nivel de acceso |
| Punto de uso | — | Canal de `CL-06` que lleva la ayuda a la pantalla o al campo donde se la necesita. Es canal, no clase nueva | [Marco, §2.5](../00-Marco.md#25-qué-cambia-entre-escenarios) (canales de ayuda); [Mapa](../01-Mapa.md#4-por-artefacto) |
| Registro de audiencias | `AUD-` | Pieza versionada (`doc_type: registro-audiencias`) con una fila por audiencia y sus dimensiones; es la fuente. La matriz audiencia × página es **derivada**, no se escribe a mano | [Registro de prefijos](Esquema-Frontmatter-Y-Prefijos.md#5-registro-de-prefijos) |
| Registro de requerimientos documentales | RRD | Tabla con una fila por requerimiento: audiencia, tarea, pieza, criterio Dado/Cuando/Entonces y decisión de una persona | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#6-registro-de-requerimientos-documentales) |
| Regla de entrega | `delivery:` | Qué audiencia recibe una pieza, por qué canal, con qué nivel de acceso y en qué punto se hace cumplir. Es la que protege; en la página se declara con `delivery:`, cerrado al registro de audiencias | `delivery` en el [Esquema](Esquema-Frontmatter-Y-Prefijos.md#2-los-diez-campos-más-description); controles `audiencia-vocabulario` y `delivery-clave-cercana` (avisan, sin implementar) |
| Repositorio hermano | `.Documentacion` | Repositorio que acompaña a uno de código con el mismo nombre y el sufijo `.Documentacion`, y guarda índices, expedientes y bitácoras | [Marco](../00-Marco.md#1-definiciones) |
| Reserva | — | Condición que devuelve el merge a una persona: un control falló, la cabeza cambió, o el agente declaró una duda | G1–G4 |
| Rol | Perfil | Perfil del producto con permisos distintos (alumno, administrador). No es un escenario: dentro de `ESC-SDEI-3`, decide qué página se sirve, por la regla de entrega. Es una de las dimensiones de entrega; ver audiencia. No confundir con actor | [`TEM-05`](../TEM-05-Ayuda-Al-Usuario-Final.md#definición) |
| Runner | — | Máquina que ejecuta los trabajos de la CI. Se elige por etiqueta, que fija el caso por plataforma; la etiqueta no es una dirección | [Marco](../00-Marco.md#1-definiciones) |
| SDD | Desarrollo guiado por especificación | Marco de trabajo donde la especificación (necesidades, casos de uso, reglas, decisiones) es la fuente de la que sale el código | `CTX-SDEI-2`; `TEM-07` |
| SDD-B | — | Alias del servicio de notificaciones push de repositorio privado que se usa como ejemplo del escenario `ESC-SDEI-4` | [Marco](../00-Marco.md#esc-sdei-4--servicio-con-paquetes) |
| Sistema documentador | — | Conjunto de piezas, fuentes de verdad, repositorios, anclas, compuertas, canales y actores que producen y mantienen la documentación de un sistema | [README](../README.md#1-qué-es-esta-guía) |
| `source.lock` | — | Archivo que fija contra qué SHA de otro repositorio vale una demostración | `CTX-SDEI-3`; PR-5 |
| Traza | `traces` | Dependencia declarada de un documento hacia un identificador; tiene que resolver contra el registro | [Esquema](Esquema-Frontmatter-Y-Prefijos.md#4-trazas) |
| Unidad | Unidad de entrega | Parte del producto que se construye y se entrega por separado, con su propia especificación y sus casos de uso; una pantalla pertenece a la unidad del front que la dibuja | [`TEM-07`](../TEM-07-Marco-SDD.md#definiciones); `TEM-05` |

Volver al [Mapa](../01-Mapa.md#6-anexos).
