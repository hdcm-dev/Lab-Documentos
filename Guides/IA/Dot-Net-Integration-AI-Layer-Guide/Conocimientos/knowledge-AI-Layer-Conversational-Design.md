# Capa de IA integrada en .NET: diseño conversacional del asistente (marco del turno y formas de respuesta)

**Alias:** AI-Layer-Conversational-Design
**Naturaleza:** propio
**Tema:** Marco normativo del turno (principios T-1 a T-8, definiciones, árbol, regla de aclaración, preguntas compuestas y rúbrica R1–R14) y qué forma toma cada respuesta del asistente de un producto y quién la elige: catálogo cerrado de formas (cifra con nota de métrica, total con desglose ofrecido, aclaración entre candidatas, pasos con cita y destino, abstención, negativa útil, afuera, IA apagada), torta o barras por regla, deeplink como chip que abre la acción, voz del asistente y sugerencias por rol. Destilado de lo medido en la consola de PushDispatch, diseñada desde cero, y alineado con la guía de la capa de IA
**Consumidor:** un agente IA (o una persona) que especifica, construye o verifica las respuestas de un asistente de producto; el crítico que lee las respuestas en la lectura humana
**Condicion-de-carga:** al decidir si un turno aclara, al juzgar un diseño o un cambio con la rúbrica; al escribir una plantilla de respuesta, una negativa, una aclaración o un gráfico del asistente; al armar la columna de conducta de una batería; al leer respuestas como crítico; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** —
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` 1.2.0 (libro de esta carpeta; las remisiones `[Guía §x]` usan sus anclas `cN-k`); [K1](knowledge-AI-Layer-Integration-Designing-Specification.md) (frontera y requisitos); [K2](knowledge-AI-Layer-Console-Assistant-Building.md) (implementación); [K3](knowledge-AI-Layer-Verification-And-Acceptance.md) (medición); [K4](knowledge-AI-Layer-Integration-Working-Method.md) (forma de trabajo); `Conversational-Design-Framework-Guide.md` 1.5.0 (Guía DC: libro de diseño conversacional; las remisiones `[Guía DC §x]` usan sus anclas; si algo choca, rige el libro)
**Versión:** 1.1
**Estado:** Vigente (formas medidas en un sistema; el marco del turno de §M es criterio del libro, con umbrales no calibrados)
**Fecha:** 2026-11-12

---

## Índice

- [0. Propósito y mapa de carga](#k5-0)
- [1. Cuándo usarlo y cuándo no](#k5-1)
- [M. Marco normativo del turno](#k5-m)
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
- [12. Límites](#k5-12)
- [Glosario](#k5-glosario)
- [Historial](#k5-historial)

---

<a id="k5-0"></a>
## 0. Propósito y mapa de carga

K1 dice **qué** puede contestar el asistente (la frontera de competencia, en anillos); K2 dice **cómo se
construye**; K3, **cómo se mide**. Este conocimiento dice **con qué forma** contesta: qué ve una persona inexperta
en cada caso, también cuando no hay respuesta. La forma la **elige el código** por regla; el modelo, si interviene,
sólo redacta dentro de ella [Guía §3.7.4](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-4).

**Coordinación con los libros.** El libro para personas es la guía de diseño conversacional (Guía DC); §M destila
su base «Criterio y juicio» y las formas se tomaron de lo medido en PushDispatch. Si algo choca con la Guía DC o
con la Guía 16, **rige el libro** y se corrige esta.

| Situación | Cargá |
|---|---|
| Decidir si un turno aclara, partir una pregunta compuesta | §M.3–§M.5 |
| Juzgar un diseño o anunciar una mejora | §M.6 |
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

<a id="k5-m"></a>
## M. Marco normativo del turno

Esta sección es la norma del turno para toda la carpeta: K1, K2 y K6 remiten acá y no la repiten. Destila la base
«Criterio y juicio» del libro ([Guía DC §B](../Conversational-Design-Framework-Guide.md#cj)); si algo choca, rige
el libro. Lo que sigue en §2–§12 (formas, datos, candidatas, chip, voz) aplica esta norma.

<a id="k5-m1"></a>
### M.1 Principios del turno

| Id | Imperativo | Porqué | Prueba |
|---|---|---|---|
| T-1 | Respondé la pregunta que se hizo, primero la respuesta | máxima de relación de Grice ([Guía DC REF-022](../Conversational-Design-Framework-Guide.md#ref-022)) | R3: captura del primer turno |
| T-2 | Preguntá sólo lo que cambia la respuesta y no se puede inferir | máxima de cantidad; cada repregunta cuesta un turno | R4: tabla de casos del enrutador, sin modelo |
| T-3 | El código decide lo verificable, también si el turno necesita dato de alcance | es P-3 llevado al turno | el contrato tiene `NecesitaDatoDeAlcance` |
| T-4 | Nunca un callejón: todo turno cierra con respuesta, destino, sugerencia o persona | un turno sin salida obliga a adivinar | R7: turno sin chip ni sugerencia = falla |
| T-5 | Si la pregunta implica hacer algo, llevá con un chip validado por rol (§6) | es la diferencia con un buscador | R6 |
| T-6 | La forma la decide la naturaleza del dato (CD-7) | la misma pregunta sale igual entre corridas | R10 |
| T-7 | Honestidad: no prometer (CD-6), abstenerse fijo (CD-2), decir el supuesto si se infirió | la persona ve sobre qué objeto se le responde | R8, R9 |
| T-8 | La mejora se juzga en lo que el usuario lee en pantalla | lo interno sin captura distinta nadie lo percibe | R13, R14 |

**Precedencia.** Los principios de proceso del libro ganan (no prometer ni inventar, después el código elige,
después la frontera; [Guía DC §2.5](../Conversational-Design-Framework-Guide.md#c2-decision)); entre los del
turno, T-7 > T-2 > T-1 > T-5 > T-6. La confirmación explícita de una acción irreversible **no es una
aclaración** y T-2 no la limita.

<a id="k5-m2"></a>
### M.2 Definiciones

- **Prueba de anillo** (ordenada, gana el primer paso que se cumple): (0) ¿haría por la persona el trabajo que el
  producto le exige a ella, o no tiene relación con el producto? → Afuera; (1) ¿depende de datos de su alcance o
  lleva a hacer algo? → 1; (2) ¿concepto con la misma respuesta para cualquiera? → 2; (3) ¿lo resuelve otro? → 3.
  El anillo **no** dice si hay que pedir un dato (K1 §3.1 fija la frontera; esta prueba la opera).
- **Alcance:** el recorte de datos de la respuesta (PushDispatch: cuenta › organización › aplicación, jerarquía
  NO VERIFICADA en el código; Movilidad Urbana: localidad › encuesta o período; Geometría: el ejercicio abierto).
- **Cambia la respuesta:** dos valores dan texto, cifra o destino de contenido distinto. Si sólo cambia el id del
  enlace, no cambia: el chip usa el alcance activo o abre un selector.
- **Respuesta global útil:** verdadera y no engañosa: total sólo si es sumable y se dice que es total; desglose si
  hay ≤ 5 candidatas; un concepto. Cifras de encuesta de localidades distintas no se suman (K5-AP-13).
- **Contrato de la intención:** `Id`, `Anillo`, ejemplos y sinónimos, `NecesitaDatoDeAlcance` (sí/no y nivel),
  `Parámetros` (cada uno: `cambiaLaRespuesta`, `inferibleDe`, `default`), mecanismo, forma F1–F8, destino con rol
  mínimo, riesgo (leer/actuar/irreversible), casos de batería. Vive en el catálogo de K2 §3 / K6 §9.
- **Aclaración:** turno que pide un parámetro antes de responder (F3); legítima sólo bajo M.4.
- **Respuesta por cercanía:** ante intención no reconocida o búsqueda vacía, lo más cercano visible, diciendo el
  supuesto («Si preguntabas X, …»); también pasa el paso (0).

<a id="k5-m3"></a>
### M.3 Árbol del turno

```text
0 Partir el mensaje en intenciones (M.5); para cada una:
1 Intención en el catálogo → anillo del contrato; si no, prueba de anillo sobre el texto
2 Frontera: Afuera → F7 · anillo 3 → derivar · 1/2 → seguir (sin catálogo: cercanía, que pasa el paso 0)
3 ¿NecesitaDatoDeAlcance? no → 6 (no se aclara nada) · sí → 4
4 Por parámetro faltante (candidatas ya filtradas por rol): ¿cambia? no → default
  · ¿se infiere? sí → inferir y decir el supuesto · ¿global útil? sí → F2 con desglose · si no → F3
5 Rol y riesgo: destino permitido (si no, F6); irreversible → confirmación explícita
6 Mecanismo: función | recuperación con cita | deeplink | combinación
7 Forma por naturaleza del dato (§3, §4) + chip si hay acción (§6)
8 Componer un solo turno con salida (T-4)
```

*Prueba:* tabla de casos `mensaje + sesión → nodo de salida y forma`, sin modelo, con al menos un caso por nodo.

<a id="k5-m4"></a>
### M.4 Regla de aclaración

**Aclarás sólo si se cumplen las cuatro:** (a) la intención necesita dato de alcance; (b) el parámetro faltante
cambia la respuesta; (c) no se infiere; (d) no hay respuesta global útil. Una pregunta conceptual («qué es», «para
qué sirve», «dónde se crea», «cómo se hace») **no pide alcance**; excepción: si el *cómo* difiere por alcance (una
función habilitada sólo en algunas cuentas) se cumple (b) y se da la respuesta común más la diferencia.

*Porqué:* en PushDispatch, «decime qué es una organización y dónde la creo», de un usuario con dos cuentas,
recibió «¿En qué cuenta?» con chips de identificadores técnicos: el código usaba «varias cuentas» como disparador
de F3 sin mirar el contrato (conducta observada en el sitio real). *Prueba* (tabla del enrutador, sin modelo):

| Caso | Salida esperada |
|---|---|
| conceptual con 2 cuentas | F4 sin F3 |
| «cuántas apps tengo» con 2 cuentas | F2 con desglose por cuenta, sin F3 |
| «la app de pedidos» con 2 coincidencias | F3 con cifra distintiva |
| 2 localidades candidatas, cifra de encuesta | desglose por localidad; homónimas con provincia; nunca suma |
| acción irreversible con cuenta inferible | confirmación explícita (no cuenta como F3) |

<a id="k5-m5"></a>
### M.5 Preguntas compuestas

1. Detectá la coordinación por código («y», «además», dos interrogativos); el modelo, si se usa, sólo segmenta y
   se valida contra el catálogo.
2. Resolvé cada intención por el árbol; componé concepto antes que acción, un turno, una salida.
3. Si una parte exige F3 legítima, respondé la otra igual. Si una parte es Afuera, respondé la permitida y la otra
   con F7; nunca rechaces el mensaje entero.

*Prueba:* un caso compuesto por intención frecuente y uno con una «y» que no separa («push de bienvenida y
despedida»; la detección no está probada en español coloquial, NO VERIFICADO). Falla si se responde una sola parte.

<a id="k5-m6"></a>
### M.6 Rúbrica de juicio

Se puntúa 0–2 con la evidencia exigida; sin evidencia, 0. De tasa (R1, R3, R5–R11): 2 = ≥ 9/10 de la batería ×3
en el sitio real (K3); 1 = ≥ 7/10. Tolerancia cero (R4, R12): 2 = 0 fallas; 1 = 1 o 2. Detalle y planilla en
[Guía DC Anexo I](../Conversational-Design-Framework-Guide.md#anexo-i).

| Id | Criterio | Evidencia |
|---|---|---|
| R1 | Intenciones, incluidas compuestas | batería con compuestos; log de intención sin texto |
| R2 | Anillo correcto | ≥ 30 casos etiquetados por alguien ajeno; 2 = ≥ 90 % de acuerdo, 1 = ≥ 75 % |
| R3 | Responde lo preguntado en el primer turno | capturas del sitio real |
| R4 | Aclara sólo bajo M.4 | tabla de M.4; aclaraciones innecesarias = 0 |
| R5 | Anclaje: cifra de función, concepto con cita | validadores de K6 §12 y K3 |
| R6 | Chip válido para el rol | captura + E2E sin 403 (K7 §8) |
| R7 | Nunca callejón | control automático |
| R8 | Abstención fija y rechazo con salida | casos fuera del corpus y Afuera |
| R9 | Dice el supuesto cuando infiere | casos con alcance inferido |
| R10 | Forma por naturaleza del dato, sin suma silenciosa | tabla `(n, tipo) → forma` |
| R11 | Candidatas con nombre humano y dato distintivo | captura de F3 sin identificadores técnicos |
| R12 | Nada ajeno al rol; acceso a la consola según la **matriz de acceso escrita** | pruebas por rol (K3 §7); rol con consola en la matriz que no la ve = defecto; sin matriz = 0 |
| R13 | Mejora percibida | ≥ 10 pares antes/después a ciegas, juez ajeno; 2 = mejor ≥ 70 % y peor ≤ 10 %; 1 = mejor > peor |
| R14 | Mejora interna trazable | cada cambio lista los casos cuya captura cambia; 2 = todos, 1 = la mayoría |

**Umbral:** ningún 0 en R3, R4, R7, R12 y total ≥ 22/28. R14 sin R13 es trabajo útil que **no** se anuncia como
«mejoró la conversación» (K5-AP-14). Los umbrales son criterio del libro y no están calibrados.

<a id="k5-2"></a>
## 2. Reglas que no se negocian

| Id | Regla (imperativo) | Porqué | Prueba |
|---|---|---|---|
| <a id="k5-r1"></a>CD-1 | Toda respuesta cae en **una** forma del catálogo de §3, y esa forma la elige el código | si la forma la elige el modelo, una misma pregunta sale distinta entre corridas y la batería no la puede fijar | tabla de casos `pregunta → forma` sobre el enrutador, sin modelo |
| <a id="k5-r2"></a>CD-2 | **Abstenerse antes que inventar:** sin evidencia, una plantilla fija de abstención que ofrece una salida | en PushDispatch una batería pasó con 95 % y una cifra inventada adentro; la abstención orienta y la invención rompe la confianza | recuperación vacía → plantilla fija, sin llamada de redacción (K1 REQ-IA-33) |
| <a id="k5-r3"></a>CD-3 | **La negativa es útil:** dice qué no se puede, qué sí y a dónde ir; nunca lleva a una pantalla que va a rechazar a la persona | un destino que termina en 403 es una negativa disfrazada; pasó en PushDispatch con una credencial | rol sin permiso de hacer → negativa útil sin destino de la acción |
| <a id="k5-r4"></a>CD-4 | **Toda cifra lleva su nota de métrica** (qué se contó, período, filtro) | «suscriptores» eran dispositivos, no personas; sin la nota, la cifra se lee mal | en la batería, cada respuesta con número tiene nota |
| <a id="k5-r5"></a>CD-5 | **Ante un nombre que coincide con varias cosas, y sólo si la regla de §M.4 pide aclarar, se aclara con candidatas y su cifra; nunca se elige ni se suma en silencio** | la persona tiene que ver sobre qué objeto se le responde | resolutor con dos coincidencias → aclaración, sin cifra agregada |
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
| F3 | **Aclaración** | nombre dado con 2 o más coincidencias visibles **y** se cumple §M.4 | 2 a 4 candidatas con una cifra distintiva y «y k más» | revelar una candidata ajena al rol | resolutor por tabla (K2 §6) |
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
5. **La forma es un parámetro del contrato.** La herramienta declara la naturaleza de la serie (`ParteDeUnTodo`, `EnElTiempo`, `Ranking`, `Comparacion`); el clasificador detecta el modificador pedido («en torta», «en barras», «en tabla», «sólo el número»); el código cruza ambos: compatible → se honra; incompatible → una línea que lo dice y la forma por omisión («Una torta no muestra la evolución; te lo dejo en barras»). La tabla acompaña plegada («Ver datos»), no como segundo gráfico. *Porqué:* en el caso, el pedido «en torta» recibía barras y tabla porque nada entendía la forma [Guía DC §6.3](../Conversational-Design-Framework-Guide.md#c6-3-pedido). *Prueba:* matriz naturaleza × pedido ([K7 R5.5](knowledge-AI-Layer-Testing-Strategy.md#k7-5-5)).
6. **La negativa de una función trae lo más cercano**, por prefijo o subcadena sin acentos y sólo entre lo que el usuario ve: «No exactamente; tenés *Acme Sur*», con su rol y «Abrir». «No encontré esto en la ayuda» sólo se admite cuando la intención es de ayuda. *Prueba:* una cuenta ajena con el mismo prefijo no aparece.

<a id="k5-5"></a>
## 5. Candidatas: aclarar o totalizar

Esta es la distinción que un agente nuevo suele confundir:

| La persona… | Forma | Ejemplo |
|---|---|---|
| no nombró ninguna entidad | **F2** total con desglose ofrecido | «¿cuántos suscriptores tengo?» |
| nombró una y hay una sola coincidencia visible | **F1** cifra, y la respuesta dice sobre qué objeto es | «la app Tienda» |
| nombró una y hay varias coincidencias visibles | **F3** aclaración con cifra distintiva | «la app de prueba» (dos parecidas) |
| nombró una que existe en varias cuentas a las que pertenece | **F3** entre cuentas sólo si la cuenta cambia la respuesta (§M.4); si no, se responde sin preguntar | la misma app en dos cuentas |
| hizo una pregunta conceptual («qué es», «dónde se crea») y tiene varias cuentas | **F4** sin aclaración; el chip usa la cuenta activa y lo dice | «qué es una organización y dónde la creo» |
| nombró una que no existe para su rol | «No encuentro esa aplicación» + destino a la lista | nunca insinuar que existe en otra cuenta |

*Porqué:* sumar en silencio cuando se nombró algo responde otra pregunta; preguntar cuando no se nombró nada
traba una respuesta que existe. *Prueba:* las seis filas son casos de la batería. La cifra distintiva sale de una
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
por criterio de §M.6 que se puntúa en la lectura (R3, R4, R9, R10, R11): forma correcta; nota de métrica presente; ninguna cifra sin origen; cita por oración en F4; destino
permitido y que abre la acción; voz única; sin primera persona de acción. *Porqué:* la tasa automática dio 95 %
con una cifra inventada adentro que sólo encontró la lectura humana. *Prueba:* la tabla de lectura tiene esas
columnas y cada forma F1–F8 tiene al menos un caso.

<a id="k5-9"></a>
## 9. Checklist binario

- [ ] Cada respuesta del asistente cae en una forma F1–F8 elegida por el código (CD-1).
- [ ] La abstención es una plantilla fija con salida, sin llamada de redacción (CD-2).
- [ ] Ningún destino lleva a una pantalla que rechace a la persona; la negativa dice qué sí (CD-3).
- [ ] Toda cifra lleva nota de métrica (CD-4).
- [ ] Las seis filas de §5 son casos de la batería y pasan.
- [ ] Cada intención del catálogo tiene `NecesitaDatoDeAlcance`, y la tabla de §M.4 pasa sin modelo.
- [ ] La batería tiene casos compuestos (§M.5).
- [ ] Ningún cambio se anuncia como mejora sin R13 y R14 de §M.6.
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
| <a id="k5-ap-13"></a>K5-AP-13 | Aclaración irrelevante: «varias cuentas → ¿En qué cuenta?» ante una pregunta conceptual, con chips de identificadores técnicos | la conversación quedó trabada en un dato que no cambiaba la respuesta | §M.4 y R4, R11 |
| <a id="k5-ap-14"></a>K5-AP-14 | Mejora invisible: se anuncia «mejoró» y la captura de la batería es igual | el dueño del producto no vio mejora | R13, R14 (§M.6) |
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
## 12. Límites

- Está medido en **un** sistema. Las cifras (2 a 4 candidatas, 2 a 6 porciones, 5 + «Otras») son puntos de
  partida: cada sistema las vuelve a medir con K3.
- El método por etapas, el glosario y cuándo mover o abandonar la frontera están en la Guía DC (§7–§16); este
  conocimiento no los repite.
- Los umbrales de §M.6 (90/75 %, 70/10 %, 10 pares, 22 de 28) son criterio del libro, no medidos; la jerarquía de
  alcance de PushDispatch no está verificada en el código.
- No decide la frontera ni el tratamiento de dominios de riesgo profesional: eso es de K1 §3.1.
- Es un repo público: no se nombra el producto de referencia ni se publican sus capturas.

<a id="k5-glosario"></a>
## Glosario

- **Alcance, cambia la respuesta, respuesta global útil, contrato, aclaración, cercanía:** ver §M.2.
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
| 1.1 | 2026-11-12 | Tras las mesas de proveedores, consumo y MCP y de la ronda del dueño del producto sobre la consola de PushDispatch: §4.5 forma como parámetro del contrato (naturaleza × pedido); §4.6 negativa por cercanía y «en la ayuda» sólo para intenciones de ayuda. |
| 1.0 | 2026-11-12 | Pasa a Vigente con la Guía DC 1.4.0: §M marco normativo del turno (T-1 a T-8 con precedencia, prueba de anillo, alcance, contrato con `NecesitaDatoDeAlcance`, árbol, regla de aclaración de cuatro condiciones, compuestas, rúbrica R1–R14); CD-5, F3 y §5 condicionados a §M.4; K5-AP-13 y K5-AP-14. K1, K2 y K6 remiten acá. |
| 0.13 | 2026-11-12 | Banco de diálogo sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): F6 trae la ficha (no un botón); §6.5 pertinencia por estado; §6.6 rótulo del chip igual al de la acción y según el rol; §6.7 negativa con contenido; §7.5 procedimiento en la voz. |
| 0.12 | 2026-11-11 | Lecciones de [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): K5-AP-12 enrutador por listas de palabras. |
| 0.11 | 2026-11-11 | Rondas 2 y 3 del banco Movilidad Urbana sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor): CD-8 redactar sólo donde aporta con compuerta «no peor que la fija» a ciegas; control de registro (§7.1); K5-AP-8 a K5-AP-11. |
| 0.10 | 2026-11-11 | Banco integral sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor): K5-AP-7 marcas de formato crudas. |
| 0.9 | 2026-11-11 | Creación como borrador provisorio, a pedido de la prueba a ciegas (K2 y K4 lo citaban sin que existiera). Reglas medidas en PushDispatch; en espera de la guía de diseño conversacional. |
