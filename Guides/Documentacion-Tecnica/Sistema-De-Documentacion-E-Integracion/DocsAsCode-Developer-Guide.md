---
doc_id: DOC-SDEI-LIBRO
doc_type: guia-de-estudio
title: DocsAsCode Developer Guide — guía de estudio del sistema de documentación e integración
description: Libro del cuerpo SDEI; explica cómo diseñar, verificar y mantener la documentación de un sistema tratada como código.
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-07
audience: [humano, estudiante, docente, desarrollo]
traces: [IDX-SDEI, MARCO-SDEI, MAPA-SDEI, TEM-SDEI-01, TEM-SDEI-02, TEM-SDEI-03, TEM-SDEI-04, TEM-SDEI-05, TEM-SDEI-06, TEM-SDEI-07]
---

# DocsAsCode Developer Guide

El libro se lee en diez archivos: esta raíz, con el índice general y el glosario-índice (anexo A), y una pieza
por Parte (la V en dos) más una con los anexos B a J. Cada pieza lleva al pie y a la cabeza la navegación
«anterior · índice · siguiente».

[Empezar por los preliminares →](DocsAsCode-Developer-Guide-Parte-0.md)

## Índice general

- [Preliminares](DocsAsCode-Developer-Guide-Parte-0.md#preliminares)
  - [Portada y colofón](DocsAsCode-Developer-Guide-Parte-0.md#portada-y-colofón)
- [Prólogo](DocsAsCode-Developer-Guide-Parte-0.md#prólogo)
  - [P. Cómo usar este libro](DocsAsCode-Developer-Guide-Parte-0.md#p-cómo-usar-este-libro)
- [Parte I. Fundamentos](DocsAsCode-Developer-Guide-Parte-I.md#parte-i-fundamentos)
  - [1. Fundamentos: la documentación tratada como código](DocsAsCode-Developer-Guide-Parte-I.md#1-fundamentos-la-documentación-tratada-como-código)
  - [2. Fundamentos didácticos y de evidencia](DocsAsCode-Developer-Guide-Parte-I.md#2-fundamentos-didácticos-y-de-evidencia)
  - [3. Marco de referencia](DocsAsCode-Developer-Guide-Parte-I.md#3-marco-de-referencia)
- [Parte II. Diseñar el sistema documentador](DocsAsCode-Developer-Guide-Parte-II.md#parte-ii-diseñar-el-sistema-documentador)
  - [4. Qué es un sistema documentador](DocsAsCode-Developer-Guide-Parte-II.md#4-qué-es-un-sistema-documentador)
  - [5. El ciclo documental](DocsAsCode-Developer-Guide-Parte-II.md#5-el-ciclo-documental)
  - [6. Topología de repositorios y versionado](DocsAsCode-Developer-Guide-Parte-II.md#6-topología-de-repositorios-y-versionado)
- [Parte III. Audiencias y canales](DocsAsCode-Developer-Guide-Parte-III.md#parte-iii-audiencias-y-canales)
  - [7. Audiencias y lectura por agentes](DocsAsCode-Developer-Guide-Parte-III.md#7-audiencias-y-lectura-por-agentes)
  - [8. Ayuda al usuario final](DocsAsCode-Developer-Guide-Parte-III.md#8-ayuda-al-usuario-final)
- [Parte IV. Verificar e integrar](DocsAsCode-Developer-Guide-Parte-IV.md#parte-iv-verificar-e-integrar)
  - [9. Compuertas de calidad y CI](DocsAsCode-Developer-Guide-Parte-IV.md#9-compuertas-de-calidad-y-ci)
  - [10. Integración con un marco SDD](DocsAsCode-Developer-Guide-Parte-IV.md#10-integración-con-un-marco-sdd)
- [Parte V. Práctica](DocsAsCode-Developer-Guide-Parte-V-a.md#parte-v-práctica)
  - [11. Estoy acá: qué aplico](DocsAsCode-Developer-Guide-Parte-V-a.md#11-estoy-acá-qué-aplico)
  - [12. Cómo intervenir: el proceso en cinco pasos](DocsAsCode-Developer-Guide-Parte-V-a.md#12-cómo-intervenir-el-proceso-en-cinco-pasos)
  - [13. Caso de estudio: la mini demo MovilidadUrbana](DocsAsCode-Developer-Guide-Parte-V-a.md#13-caso-de-estudio-la-mini-demo-movilidadurbana)
  - [14. Caso de estudio: ayuda por rol](DocsAsCode-Developer-Guide-Parte-V-a.md#14-caso-de-estudio-ayuda-por-rol)
  - [15. Decidir entre alternativas](DocsAsCode-Developer-Guide-Parte-V-b.md#15-decidir-entre-alternativas)
  - [16. Ejercicios](DocsAsCode-Developer-Guide-Parte-V-b.md#16-ejercicios)
  - [17. Mantener el libro, el cuerpo y el conocimiento](DocsAsCode-Developer-Guide-Parte-V-b.md#17-mantener-el-libro-el-cuerpo-y-el-conocimiento)
- [Parte VI. Industria](DocsAsCode-Developer-Guide-Parte-VI.md#parte-vi-industria)
  - [18. El sistema documentador en la industria](DocsAsCode-Developer-Guide-Parte-VI.md#18-el-sistema-documentador-en-la-industria)
- [Anexos](#anexos)
  - [A. Glosario-índice](#a-glosario-índice)
  - [B. Referencias](DocsAsCode-Developer-Guide-Anexos.md#b-referencias)
  - [C. Esquema de frontmatter y prefijos](DocsAsCode-Developer-Guide-Anexos.md#c-esquema-de-frontmatter-y-prefijos)
  - [D. Criterios de calidad](DocsAsCode-Developer-Guide-Anexos.md#d-criterios-de-calidad)
  - [E. Lista de verificación](DocsAsCode-Developer-Guide-Anexos.md#e-lista-de-verificación)
  - [F. Patrones de ejemplos](DocsAsCode-Developer-Guide-Anexos.md#f-patrones-de-ejemplos)
  - [G. Plantillas](DocsAsCode-Developer-Guide-Anexos.md#g-plantillas)
  - [H. Claves de los ejercicios](DocsAsCode-Developer-Guide-Anexos.md#h-claves-de-los-ejercicios)
  - [I. Correspondencia](DocsAsCode-Developer-Guide-Anexos.md#i-correspondencia)
  - [J. Marcas de estado y ensayos fechados](DocsAsCode-Developer-Guide-Anexos.md#j-marcas-de-estado-y-ensayos-fechados)

## Anexos

### A. Glosario-índice

*Forma: referencia.*

Cada término del libro, con su definición, su equivalente en inglés, su sigla y la sección donde se define. Hace de índice analítico. Orden alfabético de la *Ortografía*: la ñ va después de la n.

| Término | Definición | En inglés | Sigla | Sección |
| --- | --- | --- | --- | --- |
| A1 | Control del workflow de la mini demo que compara contrato y anclas contra la rama principal del código y sólo avisa; corre cuando corre el workflow, no está programado | — | A1 | 4.4.2 |
| Actor | Rol que interviene en el sistema documentador, con lo que decide y lo que no | — | ACT-SDEI-n | 3.1 |
| Adopción pública | Proyecto público que materializa una práctica en una ruta de su repositorio, con un commit dentro de la ventana de 24 meses; prueba presencia y actividad, no efectividad | Public adoption | — | 18.2 |
| `AGENTS.md` | Archivo con las instrucciones para un agente de código; puntero, no canal ni copia | — | `CL-03` | 7.5 |
| Agregador estricto | Último paso del job docs-guard: mira el resultado de cada grupo y da rojo si alguno falló | Strict aggregator | — | 9.1.2 |
| Ancla (de una ficha) | ruta#Símbolo@SHA: dónde se declara, se muestra, se verifica o se aplica una regla, con una afirmación que la compuerta comprueba (mini demo README:39). Distinto de «Ancla de versión» | Anchor | — | 13.1 |
| Ancla de versión | Dato que dice contra qué estado del código vale una pieza: commit, SHA fijado, etiqueta o versión del cuerpo | — | — | 1.3.2 |
| Anti-inyección | Controles que buscan instrucciones escondidas para un agente en el texto | — | AI-1 a AI-5 | 1.9 |
| Artefacto del build | Pieza que no se versiona como prosa porque la emite el build del código | build artifact | — | 6.1.1 |
| Atraso | Commits que el SHA fijado está detrás de la rama principal; entero ≥ 0; avisa | — | — | 1.3.4 |
| Audiencia | Conjunto de lectores a los que se dirige una pieza, definido por sus necesidades y sus tareas. Orienta qué se escribe y cómo; no da ni quita acceso | audience | — | 7.1 |
| Ayuda al usuario final | Pieza que explica a la persona usuaria cómo lograr una tarea con el producto, en su lenguaje y anclada a la regla o caso de uso | End-user help; user documentation | CL-06 | 8.1 |
| Canal | La forma en que un lector recibe la fuente | channel | — | 7.5 |
| Canal derivado | Lo que el build de un sitio genera de la fuente | — | `CL-14` | 7.5 |
| Capa | Cada nivel del microtexto de un campo, de un vocabulario cerrado | — | — | 8.4 |
| Capas A y B (lecciones de decisión) | A: conocimiento compartido (CL-11) que registra y destila lecciones, clase P; B: línea en la norma de cada orquestador que manda a consultar A, clase H. Criterio propio en borrador hasta el escalón 2 | — | — | 10.2.3 |
| Carril PR | Los controles que corren en cada pull request y bloquean el merge si dan rojo; grupos PR-1 a PR-7 | PR lane | — | 9.1.2 |
| Carril programado | Corrida semanal de la CI que sólo avisa con un issue | scheduled lane | — | 1.7 |
| Catálogo de ayuda | En la prueba de ayuda por rol, la lista de páginas con su campo roles que se consulta con el rol de la sesión; no es el catálogo de clases de pieza | Help catalog | — | 14.1 |
| Catálogo de clases | Lista cerrada de las catorce clases de pieza (CL-01 a CL-14) con dónde vive, ancla, control e intervención de cada una; una pieza que no encaja abre una clase por expediente | piece class catalog [criterio propio] | — | 4.2.2 |
| Categoría (IA.SDD) | Cada una de las doce carpetas numeradas de la especificación generada por IA.SDD (01 necesidades … 11 documentación de entrega) | — | — | 10.1.4 |
| Ciclo documental | Secuencia de fases por la que pasa una pieza (1a captura, 1b evaluación, 2 análisis, 3 diseño, 4 implementación, 5 integración, 5m control de deriva), con entrada, salida, quién decide y control | documentation lifecycle | — | 5.1.1 |
| Clara (metodología) | La que un lector sin contexto recorre sin dudas que el texto no responda; se mide con P-LECT, umbral cero dudas | clear | — | 5.1.6 |
| Clase de intervención | Clasifica el acto de producir o cambiar una pieza: D generador, P propone y entra por la compuerta, H decide una persona | — | D/P/H | 1.6 |
| Clase de pieza | Categoría cerrada del catálogo de catorce que fija dónde vive la pieza, su ancla, su control y quién interviene | — | CL-nn | 1.3.1 |
| Cláusula | Regla de la metodología escrita para verificarse: identificador, fase, sujeto, obliga, origen, criterio Dado/Cuando/Entonces, instrumento y nivel | clause | — | 5.1.5 |
| Co-cambio | Corrección de fondo hecha en el libro que se aplica también en la fuente, en el mismo pull request (P-01) | — | — | P.5 |
| Colofón | Página que dice sobre qué SHA de Lab-Documentos se editó cada capítulo, para medir la distancia con las fuentes | Colophon | — | Colofón |
| Comportamiento | Lo que el código hace | — | — | 1.3.4 |
| Composición por unión | Un sistema con rasgos de dos escenarios activa la unión de sus clases, sin agregar fases | — | — | 3.2.6 |
| Compuerta | El carril PR: el conjunto de controles que corre en cada pull request | gate | — | 1.7 |
| Compuerta de escalón | Hito que cierra un escalón del sistema documentador; no es un control ni el carril PR | — | — | 1.7 |
| Conocimiento de especificación | Pieza que destila del cuerpo lo que hace falta para diseñar el sistema documentador de otro proyecto: decisiones, artefactos y criterios de aceptación; consultivo, neutral respecto de un marco de especificación, con versión propia (Conocimientos/knowledge-DocsAsCode-Designing-Specification.md §0, §1, §8) | Specification knowledge | ANX-SDEI-ESPECIFICACION | 17.4 |
| Contexto | Variante que cruza los escenarios y cambia una respuesta concreta | — | CTX-SDEI-n | 3.1 |
| Control | Una comprobación de la CI que termina en verde o en rojo; bloquea o avisa según la regla de bloqueo | check | — | 1.7 |
| Control de deriva (fase 5m) | Parte de la fase 5 que devuelve una pieza al ciclo: a 1a si cambió lo que se necesita, a 4 si cambió lo que dice la fuente; no es una sexta fase | drift control | 5m | 5.1.1 |
| Criterio (de decisión) | Dimensión por la que se compara toda alternativa de una pregunta de diseño; se nombra K1 a K7 | Criterion | K | 15.1 |
| Criterio propio | Lo que no sale de una fuente; se rotula y se separa de los hechos | — | — | 2.2.2 |
| Cruce de rol | Enlace de una página de ayuda a otra que alguno de sus roles lectores no puede ver | Role crossing | — | 14.2 |
| Cuerpo documental | Documentos de una guía bajo un README que declara corpus_id y version | corpus | — | 3.1 |
| Cuerpo heredado | Las guías del repositorio escritas antes del contrato de frontmatter, fuera de un cuerpo nuevo: ahí los mismos defectos avisan | Legacy corpus | — | 9.1.2 |
| Cuerpo nuevo | Carpeta con un README.md cuyo frontmatter tiene corpus_id; dentro de ella, todo archivo que no sea un README tiene que tener frontmatter | — | — | C (fuente: Esquema-Frontmatter-Y-Prefijos.md:36-39; el anexo A la define) |
| Cuerpos de la categoría 11 | Documentación de entrega de IA.SDD por rol de intervención: integrador, mantenedor, operador | — | — | 10.1.4 |
| Dato de proveedor | Cifra que un proveedor comercial publica sobre su propia base de clientes; sirve como tendencia, no como medición independiente | Vendor data | — | 18.5.1 |
| Dato fechado | Cifra de una fuente viva que se publica con «al AAAA-MM-DD» y su método; el argumento no se apoya en ella sino en el patrón | Dated figure | — | 18.1.3 |
| Decisión con revisión | Margen menor o ganador inestable: se toma la alternativa reversible y se escribe una condición de revisión comprobable | — | CON REVISIÓN | 15.1 |
| Decisión de arquitectura | Registro corto con contexto, decisión, estado y consecuencias; se numera y no se borra | architecture decision record | ADR | 1.8 |
| Decisión firme | Margen de 0,30 o más, y el ganador no cambia al mover ±10 % cualquier peso; con varios evaluadores, además ventaja de tres o más puntos de celda | — | FIRME | 15.1; 15.4 |
| Delegación escrita | Acto H por el que la persona responsable delega una decisión H, por escrito, en un lugar legible del repositorio y con alcance acotado; la responsabilidad sigue siendo suya | written delegation | — | 5.3.1 |
| Deriva | Distancia entre lo que una pieza afirma y lo que el código hace; instrumento traces-resolve | drift | — | 1.3.4 |
| Deriva de copia fijada | Copia fijada cuya huella SHA-256 ya no coincide; alias del control 4 del carril programado | — | — | 1.3.4 |
| Derivar (una página) | Escribir la página a partir de la especificación, reescrita en la voz del usuario; no es generarla ni tiene que ver con la deriva | Derive | — | 8.1 |
| Desarrollo guiado por especificación | Marco donde la especificación se escribe antes que el código y es su referente | spec-driven development | SDD | 1.8 |
| Diátaxis | Marco que separa la documentación en tutorial, guía práctica, referencia y explicación | Diátaxis | — | 1.4 |
| Dimensiones de entrega | Rol, tipo de alcance, superficie y nivel de acceso; cuatro y ortogonales | — | — | 7.2 |
| Divergencia | Diferencia entre letra, intención y comportamiento; leve o de fondo; la decide una persona (ACT-SDEI-2) | — | — | 1.3.4 |
| Doble audiencia | Nombre anterior del tema de personas y agentes como lectores. Véase **lector** | — | — | 7.5 |
| DocsAsCode | El método: tratar la documentación como código, del que este libro es la guía | Docs as code (nombre de la obra) | — | Portada |
| Documentación como código | Escribir la documentación en texto plano, versionarla en Git, revisarla por pull request y verificarla en la CI, con las mismas herramientas que el código | docs as code | — | 1.2 |
| Edición de lectura | Forma de este libro: reúne y ordena los documentos del cuerpo sin reemplazarlos; si discrepan, manda la fuente (P-01) | Reading edition | — | P.5 |
| Escala anclada | Escala de 1 a 5 en la que cada punto tiene un observable; sin observable, la celda queda «s/d» | Anchored rating scale | — | 15.1 |
| Escalón | Etapa de prueba del sistema documentador con su compuerta de escalón: 1, el cuerpo SDEI y la mini demo; 2, la prueba piloto en Lab-Geometria; 3, `{servicio-privado}` | — | — | 10.1.4 |
| Escenario | Tipo de sistema que se documenta; el cuerpo tiene cuatro | — | ESC-SDEI-n | 3.1 |
| Especificación | Documentos del SDD que dicen qué tiene que hacer el sistema y por qué: NB-, CU-, RN-, ADR- | specification | — | 1.8 |
| Estado de verificación | Marca al final de una referencia: V verificada en fuente primaria o registro oficial; P parcial | — | V / P | B |
| Estado del documento | Valor de status en el frontmatter: borrador, vigente u obsoleto; pasar a vigente es un acto H | document status | — | 5.1.4 |
| Ficha | Página corta que declara una regla o un caso de uso con su ID, su versión y su ancla ruta#Símbolo@SHA | — | — | 3.1 |
| Filtro duro | Restricción que excluye una alternativa antes de puntuar; no se compensa con puntaje | Hard filter | — | 15.1 |
| Filtro único | La misma regla de entrega, en el mismo punto, para todos los canales, con el token del usuario | — | — | 7.5 |
| Forma (Diátaxis) | Rótulo de apertura de cada capítulo: tutorial, guía práctica, referencia o explicación; orienta y no es norma | Documentation type (Diátaxis) | — | P.3 |
| Frontmatter | Bloque YAML entre dos líneas `---` al principio del archivo; la compuerta entiende un subconjunto: clave: valor, clave: [a, b] y listas con - elemento | front matter | — | C (fuente: Esquema-Frontmatter-Y-Prefijos.md:36-39; el anexo A la define) |
| Fuente de verdad | Lugar del que sale lo que una pieza afirma; si discrepan, manda la fuente | source of truth | — | 1.3.1 |
| Fuente única | El archivo Markdown con frontmatter del que se deriva todo canal | single source | — | 7.5 |
| Grados de la especificación | Según Böckeler (2025): la que guía una tarea y se descarta, la que se conserva para mantener la funcionalidad, la que es la única fuente que se edita | Spec-first, spec-anchored, spec-as-source | — | 10.1.1 |
| Guarda de merge | Comprobación que hace un agente antes de fusionar su propio pull request, válida sólo con delegación asentada | merge guard | G1–G4 | 1.6 |
| Hoja de integración con un SDD | Sección de la instancia de «Cómo intervenir» de un proyecto, con diez campos que fijan marco, ancla, declaración del ID, prefijos, carpetas excluidas, clases citadas y agregadas, RRD, runner y norma que no se toca | — | — | 10.6 |
| Identificador de contexto | `contexto: <app>/<id>` en el frontmatter de una página de ayuda; el mapa pantalla → página se deriva | context ID | — | 8.4 |
| Índice interno | Índices versionados en el repositorio `.Documentacion`, anclados a `base_commit` | — | `CL-08` | 7.5 |
| Instancia (de «Cómo intervenir») | Página de un proyecto que completa la plantilla del proceso con sus rutas, su compuerta y quién decide; traza a la plantilla y a la versión del cuerpo | — | — | 12.1 |
| Instancia de «Cómo intervenir» | Página de un proyecto que completa la plantilla del proceso con sus datos; doc_type proceso | — | — | G.8 |
| Integración | En el ciclo del código, pull request más CI (fase 5); en el producto, la ayuda dentro de la aplicación y los canales para agentes | integration | — | 5.1.6 |
| Intención | Para qué existe una regla y qué la respalda en el código | — | — | 1.3.4 |
| Intervención | Cualquier cambio a una pieza documental de un proyecto: agregar, corregir, mover o retirar | — | — | 12.1 |
| Inventario del sistema documentador | Lista de las piezas de un proyecto, una fila por pieza con clase, fuente, ancla, control, intervención y quién decide; se levanta por comando | documentation system inventory [criterio propio] | — | 4.4.1 |
| Inyección indirecta | Texto escondido en una fuente que intenta que un agente haga algo que nadie pidió | indirect prompt injection | — | 7.5 |
| Ítem de verificación | Pregunta de sí o no, con ID estable, que quien revisa responde antes de pasar un documento a vigente | checklist item | VER | E |
| Jerarquía | Orden entre roles que se declara una vez con `incluye:` y que el build expande; cambiarla es clase H | — | — | 7.2 |
| Jerarquía de estilo | Tres niveles para estilo y lengua: convenciones del cuerpo, RAE y ASALE, Spanish (Neutral) Style Guide de Microsoft; manda el primero (P-05) | Style precedence | M3 | P.4 |
| Lección de decisión | Registro de una decisión que un agente resolvió sin la persona responsable, con nivel (N1, N2) o elevación (N3), regla, evidencia y resultado; nunca de clase H | Decision lesson | — | 10.1.4 |
| Lector | Persona o agente. Es dimensión de canal, no de acceso: el agente hereda la audiencia y el alcance de su usuario | reader | — | 7.1 |
| Letra | Lo que la app promete a quien la usa, textual | — | — | 1.3.4 |
| Línea de base | Hallazgos de un control sobre lo que vigila, medidos antes de bloquear | baseline | — | 1.7 |
| Marca de estado | Rótulo en negrita del estado de un dato: NO VERIFICADO, HISTÓRICO (fecha), ESPECULACIÓN; listadas en el anexo J | Status label | — | P.4 |
| Marca de procedencia | Rótulo que dice de dónde sale un fragmento: Compilado, Salida registrada, Fragmento ilustrativo | Provenance label | — | P.4 |
| Marco de especificación del proyecto | Forma neutral con que el conocimiento nombra el marco de especificación de un proyecto, sin sigla (P-24) | — | — | 17.4.2 |
| Margen | Diferencia de puntaje ponderado entre la primera y la segunda alternativa admitidas | Margin | — | 15.1 |
| Método de conteo | El comando exacto con que se obtuvo una cifra; se declara antes del número porque dos métodos correctos dan cifras distintas sobre el mismo archivo | Counting method | — | 18.1.1 |
| Microtexto | Texto corto de un campo, en capas: `etiqueta`, `pista`, `info`, `ejemplo`, `validacion` | microcopy | — | 8.4 |
| Nivel de acceso | Vocabulario cerrado: `publico`, `producto`, `restringido`, `interno` | access level | — | 7.3 |
| Orquestador | Agente o persona que conduce las fases del SDD, despacha el trabajo y decide el paso de fase | Orchestrator | — | 10.1.4 |
| Par rojo/verde | Dos fixtures por control, con y sin el defecto; prueba negativa de la compuerta | red/green pair | — | 1.7 |
| Patrón de ejemplo | Regla citable para escribir ejemplos y material de consulta | example pattern | PE | F.1 |
| Peso | Fracción de importancia de un criterio; los pesos suman 1 y se fijan antes de puntuar | Weight | — | 15.1 |
| Pieza derivada | Pieza que sale de otra por un paso mecánico (el contrato OpenAPI del build) o que depende de otra por una traza | Derived artifact | — | 12.1 |
| Pieza documental | Unidad que se escribe, se versiona y se valida por separado | documentation artifact | — | 1.3.1 |
| Prefijo | Parte de un identificador antes del primer guion (TEM en TEM-01); sale de un registro cerrado | prefix | — | C.4 |
| Prefijo externo | Prefijo del registro cerrado (CU, RN, ADR, NB, RF…) para IDs que declara el sistema que se documenta; sin source.lock ni --fuente, la traza que no resuelve sólo avisa | External prefix | — | 10.1.4 |
| Prefijo interno | Prefijo de un ID que se declara en este repositorio; si la traza no resuelve, bloquea | — | — | C.4 |
| Propuesta | Especificación pública sin organismo de normalización detrás (como llms.txt); no obliga y el cuerpo la activa sólo en su contexto | Proposal | — | 18.4 |
| Proyecto destino | Repositorio que aplica un marco SDD y guarda su especificación generada; las trazas resuelven contra él, no contra el marco | Target project | — | 10.1.4 |
| Prueba de lectura | Un lector sin contexto recorre el texto y anota las dudas que no responde; umbral cero | — | P-LECT | 2.4.3 |
| Prueba negativa permanente | Pieza que tiene que seguir dando rojo en cada corrida para probar que el control muerde; en la demo, la ficha RN-LOC-01 1.0 (control C4; RN-LOC-01.md:76-79) | Permanent negative test | — | 13.2 |
| Prueba pareada | 200 con contenido al autorizado; distinto de 200 y sin contenido al que no lo está | — | — | 7.3 |
| Punto de aplicación | Lugar donde se hace cumplir el nivel: repositorio, build o servidor | enforcement point | — | 7.3 |
| Punto de uso | Canal de `CL-06` que lleva la ayuda a la pantalla o al campo donde se la necesita | point of use | — | 8.4 |
| Registro de audiencias | Pieza versionada con prefijo `AUD-`, una fila por audiencia; es la fuente, y la matriz audiencia × página se deriva | audience register | AUD | 7.1 |
| Registro de identificadores | Lista que la compuerta arma sola con los doc_id y los encabezados que empiezan con un ID, contra la que resuelve las trazas | ID registry | — | 1.3.3 |
| Registro de IDs | Registro único por repositorio que la compuerta arma con los doc_id y los encabezados que empiezan con un ID; contra él resuelve traces-resolve | ID registry [criterio propio] | — | 6.1.3 |
| Registro de IDs del SDD | Lista de identificadores que la especificación declara, cada uno una vez y con su versión (encabezado que empieza con el ID y línea **Versión:**) | ID registry | — | 10.1.4 |
| Registro de requerimientos documentales | Tabla con una fila por requerimiento documental: audiencia, tarea, pieza, criterio Dado/Cuando/Entonces y decisión de una persona | documentation requirements register [criterio propio] | RRD | 5.1.3 |
| Regla de bloqueo | Las cinco condiciones que un control cumple para bloquear; si falla una, avisa | — | — | 1.7 |
| Regla de entrega | Qué audiencia recibe una pieza, por qué canal, con qué nivel de acceso y en qué punto se hace cumplir. Es la que protege | delivery rule | — | 7.1 |
| Relato de migración | Texto en que un equipo cuenta por qué adoptó una práctica; complementa la adopción, que sólo dice que existe | Migration story | — | 18.2 |
| Repositorio dedicado | Repositorio cuyo producto es la documentación | dedicated repository | — | 6.1.1 |
| Repositorio hermano | Repositorio .Documentacion que acompaña a uno de código con el mismo nombre | — | — | 3.1 |
| Requerimiento documental | Lo que una audiencia necesita lograr con la documentación, como fila del RRD con criterio Dado/Cuando/Entonces | — | RRD (el registro) | 3.1 |
| Reserva | Duda declarada o condición que devuelve el merge a una persona | — | — | 1.6 |
| Reverificación | Repetir cada afirmación de un caso ajeno contra la respuesta actual de la fuente, con un comando por afirmación y fecha nueva (P-07) | Re-verification | — | 18.1.1 |
| Rol | Perfil del producto con permisos distintos; una dimensión de la regla de entrega, no un escenario | role | — | 7.1 |
| Runner | Máquina que ejecuta los trabajos de la CI; se elige por etiqueta | runner | — | 3.1 |
| Runner efímero | Runner donde cada job arranca de cero, salvo una caché de herramientas | Ephemeral runner | — | 9.1.3 |
| Ruta de lectura | Recorrido del libro según el lector: aprender, intervenir, agente, escribo la ayuda de un producto, de corrido | Reading path | — | P.3 |
| SDEI | Este cuerpo documental: Sistema de documentación e integración, la carpeta de Lab-Documentos de la que sale el libro | — | SDEI | Portada |
| SHA fijado | Ancla de una pieza que vive en otro repositorio que el código: el SHA completo escrito en source.lock o como base_commit | pinned commit | — | 6.1.2 |
| Sistema documentador | Lo que el método construye: piezas, fuentes de verdad, repositorios, anclas de versión, compuertas, canales y actores que producen y mantienen la documentación de un sistema | Documentation system | — | P.1 |
| source.lock | Archivo que fija el repositorio y el SHA del código que se documenta desde otro repositorio; todo lo que la pieza afirma se comprueba contra ese commit (mini demo README:37) | — | — | 13.1 |
| Supuesto (en un pull request) | Caso que el tutorial no cubre, resuelto por analogía y anotado en una sección «Supuestos» del PR para que lo decida quien revisa (mini demo README:349-356) | Assumption | — | 13.4.6 |
| Topología | Dónde vive una pieza respecto del código que describe: mismo repositorio, repositorio hermano, repositorio dedicado o artefacto del build | repository topology | — | 6.1.1 |
| Traza | Declaración en el frontmatter de que un documento depende de un identificador, con o sin versión | trace | traces | 1.3.3 |
| Traza con versión | traces: [ID@MAYOR.MENOR]: qué regla explica la página y en qué versión se leyó; una subida deja la traza desfasada | Versioned trace | — | 8.1 |
| Traza de nivel 1 | Página → ficha: el ID existe y la versión coincide (mini demo README:41) | — | — | 13.1 |
| Traza de nivel 2 | Ficha → código: cada ancla resuelve en el SHA y cumple su afirmación (mini demo README:42) | — | — | 13.1 |
| Variante completa (página de ayuda) | Página de ayuda con doc_id y los diez campos más description; la compuerta la valida como documento del cuerpo y no mira roles | — | — | G.9.1 |
| Variante de rol (página de ayuda) | Página de ayuda sin doc_id, con title, description y roles o translation_of, que la compuerta valida con el esquema de ayuda | — | — | G.9.2 |
| Versión congelada | Copia de un documento en una versión anterior, guardada para la historia (carpetas _legacy/); repite el ID del vigente | Frozen version | — | 10.1.4 |
| Versión del cuerpo | Ancla de una guía de estudio: version del README del cuerpo en SemVer y una etiqueta anotada por versión | corpus version [criterio propio] | — | 6.1.2 |
| Visibilidad y acceso | Ocultar en la navegación es visibilidad; no entregar es acceso. Sólo lo segundo protege | — | — | 7.3 |
| Vocabulario cerrado | Lista fija de valores admitidos para un campo; un valor fuera de la lista pone la compuerta en rojo | controlled vocabulary | — | C (fuente: Esquema-Frontmatter-Y-Prefijos.md:37; el anexo A la define) |

*Tabla A.1. Glosario-índice.*
