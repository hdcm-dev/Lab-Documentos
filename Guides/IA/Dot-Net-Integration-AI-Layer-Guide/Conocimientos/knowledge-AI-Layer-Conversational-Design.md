# Capa de IA integrada en .NET: diseño conversacional del asistente (formas de respuesta)

**Alias:** AI-Layer-Conversational-Design
**Naturaleza:** propio
**Tema:** Qué forma toma cada respuesta del asistente de un producto y quién la elige: catálogo cerrado de formas (cifra con nota de métrica, total con desglose ofrecido, aclaración entre candidatas, pasos con cita y destino, abstención, negativa útil, afuera, IA apagada), torta o barras por regla, deeplink como chip que abre la acción, voz del asistente y sugerencias por rol. Destilado de lo medido en la consola de PushDispatch, diseñada desde cero, y alineado con la guía de la capa de IA
**Consumidor:** un agente IA (o una persona) que especifica, construye o verifica las respuestas de un asistente de producto; el crítico que lee las respuestas en la lectura humana
**Condicion-de-carga:** al escribir una plantilla de respuesta, una negativa, una aclaración o un gráfico del asistente; al armar la columna de conducta de una batería; al leer respuestas como crítico; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** —
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` 1.2.0 (libro de esta carpeta; las remisiones `[Guía §x]` usan sus anclas `cN-k`); [K1](knowledge-AI-Layer-Integration-Designing-Specification.md) (frontera y requisitos); [K2](knowledge-AI-Layer-Console-Assistant-Building.md) (implementación); [K3](knowledge-AI-Layer-Verification-And-Acceptance.md) (medición); [K4](knowledge-AI-Layer-Integration-Working-Method.md) (forma de trabajo); `Conversational-Design-Framework-Guide.md` (libro hermano en redacción: cuando se publique, este conocimiento remite a sus secciones y, si algo choca, rige el libro)
**Versión:** 0.13
**Estado:** Borrador provisorio (las reglas están medidas en un sistema; pasa a 1.0 cuando se publique la guía de diseño conversacional y se agreguen sus remisiones)
**Fecha:** 2026-11-12

---

## Índice

- [0. Propósito y mapa de carga](#k5-0)
- [1. Cuándo usarlo y cuándo no](#k5-1)
- [2. Reglas que no se negocian](#k5-2)
- [3. Catálogo de formas de respuesta](#k5-3)
- [4. Datos: cifra, desglose, torta o barras](#k5-4)
- [5. Candidatas: aclarar o totalizar](#k5-5)
- [6. Destino: el chip que abre la acción](#k5-6)
- [7. Voz y sugerencias](#k5-7)
- [8. Cómo se mide (lo que juzga la lectura humana)](#k5-8)
- [9. Checklist binario](#k5-9)
- [10. Antipatrones con su control](#k5-10)
- [11. Casos](#k5-11)
- [12. Límites y espera del libro](#k5-12)
- [Glosario](#k5-glosario)
- [Historial](#k5-historial)

---

<a id="k5-0"></a>
## 0. Propósito y mapa de carga

K1 dice **qué** puede contestar el asistente (la frontera de competencia, en anillos); K2 dice **cómo se
construye**; K3, **cómo se mide**. Este conocimiento dice **con qué forma** contesta: qué ve una persona inexperta
en cada caso, también cuando no hay respuesta. La forma la **elige el código** por regla; el modelo, si interviene,
sólo redacta dentro de ella [Guía §3.7.4](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-4).

**Coordinación con los libros.** Es un borrador provisorio: la guía de diseño conversacional, que es el libro
para personas sobre este tema, todavía no está publicada. Estas reglas se tomaron de lo medido en PushDispatch y
de la tabla de brecha con un producto de referencia del mercado, y ninguna contradice la Guía 16. Cuando el libro
se publique, cada regla va a remitir a su sección; si alguna choca con él, **rige el libro** y se corrige esta.

| Situación | Cargá |
|---|---|
| Escribir o revisar una plantilla de respuesta | §2, §3 |
| La respuesta lleva una cifra o un gráfico | §4 |
| Lo que nombró la persona coincide con varias cosas | §5 |
| La respuesta ofrece ir a una pantalla | §6 |
| Tono, persona gramatical, sugerencias | §7 |
| Armar la lectura humana o la columna de conducta de la batería | §8, y K3 §5 |
| Revisar un PR que toca textos del asistente | §9, §10 |

<a id="k5-1"></a>
## 1. Cuándo usarlo y cuándo no

**Usalo** en todo asistente o consola que conteste en lenguaje natural dentro de un producto con sesión y roles.

**No lo uses** para decidir si la pregunta se contesta (eso es la frontera de K1 §3.1) ni para elegir herramienta o
alcance (K2 §3). Tampoco cubre el texto de pantallas que no son del asistente.

<a id="k5-2"></a>
## 2. Reglas que no se negocian

| Id | Regla (imperativo) | Porqué | Prueba |
|---|---|---|---|
| <a id="k5-r1"></a>CD-1 | Toda respuesta cae en **una** forma del catálogo de §3, y esa forma la elige el código | si la forma la elige el modelo, una misma pregunta sale distinta entre corridas y la batería no la puede fijar | tabla de casos `pregunta → forma` sobre el enrutador, sin modelo |
| <a id="k5-r2"></a>CD-2 | **Abstenerse antes que inventar:** sin evidencia, una plantilla fija de abstención que ofrece una salida | en PushDispatch una batería pasó con 95 % y una cifra inventada adentro; la abstención orienta y la invención rompe la confianza | recuperación vacía → plantilla fija, sin llamada de redacción (K1 REQ-IA-33) |
| <a id="k5-r3"></a>CD-3 | **La negativa es útil:** dice qué no se puede, qué sí y a dónde ir; nunca lleva a una pantalla que va a rechazar a la persona | un destino que termina en 403 es una negativa disfrazada; pasó en PushDispatch con una credencial | rol sin permiso de hacer → negativa útil sin destino de la acción |
| <a id="k5-r4"></a>CD-4 | **Toda cifra lleva su nota de métrica** (qué se contó, período, filtro) | «suscriptores» eran dispositivos, no personas; sin la nota, la cifra se lee mal | en la batería, cada respuesta con número tiene nota |
| <a id="k5-r5"></a>CD-5 | **Ante un nombre que coincide con varias cosas, se aclara con candidatas y su cifra; nunca se elige ni se suma en silencio** | la persona tiene que ver sobre qué objeto se le responde | resolutor con dos coincidencias → aclaración, sin cifra agregada |
| <a id="k5-r6"></a>CD-6 | **El asistente no actúa ni dice que actúa:** explica y lleva; la acción la hace la persona en la pantalla | el anillo 1 es de lectura y navegación (K2 §1); «yo te lo creo» promete lo que el software no hace | control del texto: ninguna oración en primera persona de acción |
| <a id="k5-r8"></a>CD-8 | **Redactá con el modelo sólo donde aporta** (cifras con su contexto) y aceptá cada forma redactada sólo si **no es peor que su plantilla fija**, medido a ciegas | en el banco Movilidad Urbana de [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor) la redacción pasó todos los controles de cifras y citas y aun así salió peor que la plantilla en 6 de 13 pares a ciegas: el anclaje no mide calidad | compuerta a ciegas por forma (K3 R2.8); una respuesta que sólo suma citas a la plantilla no cuenta como redactada |
| <a id="k5-r7"></a>CD-7 | **La forma de los datos (torta, barras, número solo) la decide el código por regla**, no el modelo | el modelo no sabe cuántas categorías hay ni si suman un todo | tabla de casos `(n categorías, tipo) → forma` |

**Texto de la abstención.** La plantilla es fija y vive en el código; el texto exacto lo elige cada proyecto. En
este conocimiento se la nombra «No está en la ayuda». En PushDispatch el código usa dos: una para cuando la
búsqueda no trae nada («No encontré esto en la ayuda. Probá con otras palabras o abrí la Ayuda.») y otra para
cuando el modelo se abstiene con fragmentos («No lo encontré en la ayuda»). Lo que se exige es la conducta
(texto fijo, sin dato, con salida), no el texto.

<a id="k5-3"></a>
## 3. Catálogo de formas de respuesta

| # | Forma | Cuándo la elige el código | Qué lleva | Qué no puede pasar | Prueba |
|---|---|---|---|---|---|
| F1 | **Cifra** | anillo 1, una entidad resuelta | número de una herramienta + nota de métrica + destino si hay | número sin origen en una herramienta (K2 R3) | validador de cifras |
| F2 | **Total con desglose ofrecido** | pregunta de cantidad **sin** nombre y varias entidades visibles | total + nota + oferta de abrirlo («¿Lo abro por aplicación?») | preguntar «¿sobre qué aplicación?» cuando hay respuesta global | caso con 0, 1 y varias entidades |
| F3 | **Aclaración** | nombre dado con 2 o más coincidencias visibles | 2 a 4 candidatas con una cifra distintiva y «y k más» | revelar una candidata ajena al rol | resolutor por tabla (K2 §6) |
| F4 | **Pasos desde la ayuda** | anillo 1 o 2 con fragmentos sobre el umbral | redacción anclada, cita por oración, destino del catálogo | devolver el fragmento crudo o sólo un enlace (K2 §3) | lectura humana + control de anclaje |
| F5 | **Abstención** | sin fragmento sobre el umbral, o el modelo se abstiene | plantilla fija + salida (otras palabras, Ayuda) | un dato o un paso inventado | recuperación vacía |
| F6 | **Negativa útil** | la intención existe pero el rol no puede | qué no, qué sí, a quién pedirlo o a dónde ir; si nombra un concepto, **la ficha viaja en la misma respuesta** (plegada) | un destino que termina en 403; revelar qué tiene otro rol; remitir a un botón «Abrir la ficha» en lugar de traerla | rol sin permiso de hacer |
| F7 | **Afuera** | anillo «Afuera» de la frontera | texto fijo + 2 o 3 preguntas de ejemplo que sí contesta | una respuesta del modelo [Guía §3.7.2](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-2) | pregunta afuera → sin llamada al modelo |
| F8 | **IA apagada** | habilitación distinta de `Habilitada` (K2 §10) | aviso con el motivo («falta configurar X») + respaldo por pertinencia | un texto genérico de introducción | arranque sin la variable |

<a id="k5-4"></a>
## 4. Datos: cifra, desglose, torta o barras

1. **La primera respuesta es el agregado**, con su nota, y **ofrece** el desglose sin imponerlo. *Porqué:* la
   tabla de brecha con un producto de referencia del mercado mostró que ese es el patrón que una persona inexperta
   lee sin esfuerzo. *Prueba:* caso «¿cuántos X tengo?» con varias entidades → F2.
2. **Torta sólo para partes de un todo con 2 a 6 partes:** las mayores y una porción «Otras (k)». Con más de 6 o
   partes que no suman un todo, **barras ordenadas** de mayor a menor. Para evolución en el tiempo, **barras por
   período**. *Porqué:* las porciones finas no se distinguen y la torta no tiene eje de tiempo. *Prueba:* tabla
   `(n, tipo) → forma`; las porciones suman exactamente el total (K2 §7). En PushDispatch se usó «5 mayores +
   Otras»: es un punto de partida dentro de la regla, no una constante.
3. **Nunca sumes métricas que no se suman** (activos de varias apps, promedios). *Prueba:* la herramienta marca la
   métrica como no sumable y el código no arma F2 con ella.
4. **Dato parcial, se dice en la nota** («faltan datos de las últimas 2 horas»), no en un descargo genérico.

<a id="k5-5"></a>
## 5. Candidatas: aclarar o totalizar

Esta es la distinción que un agente nuevo suele confundir:

| La persona… | Forma | Ejemplo |
|---|---|---|
| no nombró ninguna entidad | **F2** total con desglose ofrecido | «¿cuántos suscriptores tengo?» |
| nombró una y hay una sola coincidencia visible | **F1** cifra, y la respuesta dice sobre qué objeto es | «la app Tienda» |
| nombró una y hay varias coincidencias visibles | **F3** aclaración con cifra distintiva | «la app de prueba» (dos parecidas) |
| nombró una que existe en varias cuentas a las que pertenece | **F3** entre **cuentas** primero | la misma app en dos cuentas |
| nombró una que no existe para su rol | «No encuentro esa aplicación» + destino a la lista | nunca insinuar que existe en otra cuenta |

*Porqué:* sumar en silencio cuando se nombró algo responde otra pregunta; preguntar cuando no se nombró nada
traba una respuesta que existe. *Prueba:* las cinco filas son casos de la batería. La cifra distintiva sale de una
herramienta y se filtra por permiso como cualquier otra.

<a id="k5-6"></a>
## 6. Destino: el chip que abre la acción

1. El destino se muestra como un **chip con verbo y objeto** («Abrir lista de apps», «Crear aplicación»), no como
   URL ni como texto del modelo. *Prueba:* el chip sale del catálogo (K2 §5) y su rótulo también.
2. El chip **abre la acción** (el modal de alta, el filtro aplicado), no sólo la pantalla. *Prueba:* bUnit con el
   parámetro → la acción está abierta (K2 §5.4).
3. Se ofrece **sólo si el permiso de hacer pasa** para esa sesión y, si depende de la instancia, para esa instancia
   (K2 §5.6). Si no pasa, la respuesta es F6.
4. Un solo destino principal por respuesta; si hay más, el principal primero.
5. Se ofrece **sólo si es pertinente para el estado de la instancia**, no sólo si el permiso pasa: sobre un
   trabajo ya enviado no se ofrece «Cargar un trabajo»; «Ver pistas» sólo si hay advertencias. *Prueba:* K2 §5.7.
6. **El rótulo del chip promete lo que la página muestra.** Usa las mismas palabras que la acción en la página
   destino (si la página dice «Ver un ejemplo», el chip no dice «Ver el ejemplo de formato»), y el posesivo según
   el rol («tu trabajo» al alumno, «el trabajo del alumno» al docente, K2 §5.8). *Porqué:* en la rebanada de
   [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria) EXP-0005 el chip abría la acción correcta pero con
   otro nombre, y la persona no reconoce que llegó. *Prueba:* por destino que abre una acción, el rótulo del chip
   contiene el rótulo visible del control que la página enfoca.
7. **La negativa trae el contenido, no un botón hacia él.** Si la respuesta F6 nombra un concepto, la ficha va
   plegada en la misma respuesta. *Porqué:* en Lab-Geometria (EXP-0004, H2) «[Abrir la ficha]» fue lo primero que
   se corrigió: una negativa que obliga a otro clic para lo que sí se puede saber pierde su utilidad. *Prueba:*
   caso de batería «pedir resolver» → la respuesta contiene el texto de la ficha.

<a id="k5-7"></a>
## 7. Voz y sugerencias

1. **Una sola persona gramatical, la del producto** (en PushDispatch, voseo y segunda persona). *Prueba:* control
   del texto en la batería (tuteo o voseo mezclado = falla). El control es de **registro**, no de pronombre suelto:
   «vuestra» es de la segunda del plural peninsular y no es voseo; en Movilidad Urbana se coló así. La lista de
   formas ajenas tiene su prueba de falsos positivos (palabras legítimas que no deben marcarse), K7 R9.10.
2. **El asistente no se presenta como quien actúa** (CD-6). *Prueba:* ninguna oración del tipo «yo creo», «te lo
   configuro».
3. **Explicá el término del producto la primera vez** cuando la persona usó un sinónimo
   [Guía §3.7.3](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-3).
4. **Las sugerencias son por pantalla y rol, y sólo de cosas que ese rol puede hacer.** *Porqué:* al rol raíz de
   PushDispatch se le sugería «¿Cómo creo una aplicación?» y no podía. *Prueba:* cada sugerencia pasa la
   revalidación de destino (K2 §9).
5. **Procedimiento en la voz = dictar.** En un producto didáctico, el imperativo de voseo con tilde («calculá»,
   «sumá», «tomá») y los marcadores de secuencia («primero», «tenés que») convierten la explicación en receta; el
   presente descriptivo («la suma de las áreas», «se toma la base») no. *Porqué:* en Lab-Geometria el filtro de
   salida tuvo que distinguir «sumá» de «suma» (H1). *Prueba:* lista de formas de procedimiento con corpus bueno
   que la contenga en forma descriptiva y cero marcas (K7 R9.11).
6. **Los motivos del voto son cerrados y dicen dónde arreglar** («No es lo que pregunté», «Falta información»,
   «Dato incorrecto») (K2 §8).

<a id="k5-8"></a>
## 8. Cómo se mide (lo que juzga la lectura humana)

Cada caso de la batería de K3 lleva la **forma esperada** (F1 a F8) y la lectura humana (K3 §5) tiene una columna
por criterio: forma correcta; nota de métrica presente; ninguna cifra sin origen; cita por oración en F4; destino
permitido y que abre la acción; voz única; sin primera persona de acción. *Porqué:* la tasa automática dio 95 %
con una cifra inventada adentro que sólo encontró la lectura humana. *Prueba:* la tabla de lectura tiene esas
columnas y cada forma F1–F8 tiene al menos un caso.

<a id="k5-9"></a>
## 9. Checklist binario

- [ ] Cada respuesta del asistente cae en una forma F1–F8 elegida por el código (CD-1).
- [ ] La abstención es una plantilla fija con salida, sin llamada de redacción (CD-2).
- [ ] Ningún destino lleva a una pantalla que rechace a la persona; la negativa dice qué sí (CD-3).
- [ ] Toda cifra lleva nota de métrica (CD-4).
- [ ] Las cinco filas de §5 son casos de la batería y pasan.
- [ ] La forma de los datos sale de la tabla `(n, tipo) → forma` y las porciones suman el total (§4).
- [ ] Los chips salen del catálogo, tienen verbo y objeto y abren la acción (§6).
- [ ] Voz única y ninguna primera persona de acción (§7).
- [ ] Las sugerencias de cada rol son realizables por ese rol (§7).
- [ ] Cada forma redactada pasó la compuerta «no peor que la fija» a ciegas (CD-8).
- [ ] El control de registro detecta formas ajenas («vuestra», tuteo) y tiene prueba de falsos positivos (§7).
- [ ] La lectura humana tiene una columna por criterio de §8 y cada forma tiene un caso.

<a id="k5-10"></a>
## 10. Antipatrones con su control

| Id | Antipatrón | Qué costó en PushDispatch | Control |
|---|---|---|---|
| <a id="k5-ap-1"></a>K5-AP-1 | Respuesta = puntero a la guía | respuestas que no contestaban | F4: redacción anclada; nunca sólo un enlace |
| <a id="k5-ap-2"></a>K5-AP-2 | «¿Sobre qué aplicación?» ante una pregunta global | traba una respuesta que existía | §5, fila 1 |
| <a id="k5-ap-3"></a>K5-AP-3 | «No encuentro esa aplicación» con candidatas parecidas | un callejón sin salida | §5, filas 3 y 5 |
| <a id="k5-ap-4"></a>K5-AP-4 | Cifra sin nota de métrica | dispositivos leídos como personas | CD-4 |
| <a id="k5-ap-5"></a>K5-AP-5 | Destino que termina en 403 | «Ir a Credenciales» a quien no podía cargar | CD-3, §6.3 |
| <a id="k5-ap-6"></a>K5-AP-6 | Sugerencia que el rol no puede hacer | el raíz recibía sugerencias ajenas | §7.4 |
| <a id="k5-ap-8"></a>K5-AP-8 | Aceptar la redacción porque pasó el anclaje | Movilidad Urbana: 6 de 13 pares peores que la plantilla con 0 cifras inventadas | CD-8 |
| <a id="k5-ap-9"></a>K5-AP-9 | Afirmación falsa sin números (paráfrasis que invierte una nota; restricción inventada «sólo en una localidad») | escapa al validador de cifras | [K6 §12.8](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-12-8) |
| <a id="k5-ap-10"></a>K5-AP-10 | Nota que habla de porcentajes donde sólo hay conteos («no suman 100 %») | persistió en la redactada y en la plantilla fija | la nota nombra la unidad que se muestra; caso de batería con conteos sin porcentaje |
| <a id="k5-ap-11"></a>K5-AP-11 | Mismo «[1]» repetido en cada oración cuando hay una sola fuente | la IA sólo agregaba citas: ~1 s contra 2 ms de la plantilla | una cita por respuesta con fuente única; si la redacción sólo suma citas, va la plantilla |
| <a id="k5-ap-12"></a>K5-AP-12 | Enrutador que elige la forma (o la negativa) con listas de palabras ampliadas a mano | en [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria) cada ampliación rechazó preguntas legítimas o dejó escapar pedidos | regla por función del elemento en la frase, probada en los dos sentidos ([K7 R6.7](knowledge-AI-Layer-Testing-Strategy.md#k7-6-7)) y con sonda ([K7 R6.8](knowledge-AI-Layer-Testing-Strategy.md#k7-6-8)) |
| <a id="k5-ap-7"></a>K5-AP-7 | Marcas de formato crudas en el panel (`**Encuesta**`) | en [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor) se veían los asteriscos en respuestas fijas y redactadas | el panel renderiza un subconjunto cerrado (negrita, listas) o lo quita; captura sin `**` ni `#` |

La tabla índice de todos los antipatrones de la integración está en
[K4 §8](knowledge-AI-Layer-Integration-Working-Method.md#k4-8-antipatrones).

<a id="k5-11"></a>
## 11. Casos

**PushDispatch (caso de origen).** La consola empezó con fichas de búsqueda que devolvían punteros a la ayuda. La
tabla de brecha con un producto de referencia del mercado aportó cinco patrones: total agregado con desglose
ofrecido, elección entre apps de nombre parecido con sus cifras, chip de destino («Abrir lista de apps»), torta de
las mayores más «Otras» y nota de qué métrica se muestra. Todos quedaron como formas de §3 con su caso en la
batería, que se aceptó con 36 de 36 redacciones ancladas en cada una de tres corridas (K3 §16).

**Movilidad Urbana (demo del libro).** [Guía §15](../Dot-Net-Integration-AI-Layer-Guide.md#c15): sin roles, F6 y
§7.4 no aplican; la localidad inexistente es F5 y la cifra de recorridos es F1 con nota.

<a id="k5-12"></a>
## 12. Límites y espera del libro

- Está medido en **un** sistema. Las cifras (2 a 4 candidatas, 2 a 6 porciones, 5 + «Otras») son puntos de
  partida: cada sistema las vuelve a medir con K3.
- **Queda en espera de la guía de diseño conversacional:** remisiones por sección, el método por etapas (de
  inventario de tareas a plan de medición), el glosario y los sinónimos como insumo de diseño, y cuándo mover o
  abandonar la frontera. Hasta que se publique, este conocimiento no los fija.
- No decide la frontera ni el tratamiento de dominios de riesgo profesional: eso es de K1 §3.1.
- Es un repo público: no se nombra el producto de referencia ni se publican sus capturas.

<a id="k5-glosario"></a>
## Glosario

- **Forma de respuesta:** una de F1–F8; la elige el código.
- **Desglose ofrecido:** el total va primero y se ofrece abrirlo por una dimensión.
- **Cifra distintiva:** número visible para el rol que permite elegir entre candidatas.
- **Nota de métrica:** línea que dice qué se contó, en qué período y con qué filtro.
- **Negativa útil:** respuesta que dice qué no se puede, qué sí y a dónde ir.
- **Chip de destino:** botón con verbo y objeto que abre la acción en la pantalla del catálogo.

<a id="k5-historial"></a>
## Historial

| Versión | Fecha | Cambio |
|---|---|---|
| 0.13 | 2026-11-12 | Banco de diálogo sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): F6 trae la ficha (no un botón); §6.5 pertinencia por estado; §6.6 rótulo del chip igual al de la acción y según el rol; §6.7 negativa con contenido; §7.5 procedimiento en la voz. |
| 0.12 | 2026-11-11 | Lecciones de [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): K5-AP-12 enrutador por listas de palabras. |
| 0.11 | 2026-11-11 | Rondas 2 y 3 del banco Movilidad Urbana sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor): CD-8 redactar sólo donde aporta con compuerta «no peor que la fija» a ciegas; control de registro (§7.1); K5-AP-8 a K5-AP-11. |
| 0.10 | 2026-11-11 | Banco integral sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor): K5-AP-7 marcas de formato crudas. |
| 0.9 | 2026-11-11 | Creación como borrador provisorio, a pedido de la prueba a ciegas (K2 y K4 lo citaban sin que existiera). Reglas medidas en PushDispatch; en espera de la guía de diseño conversacional. |
