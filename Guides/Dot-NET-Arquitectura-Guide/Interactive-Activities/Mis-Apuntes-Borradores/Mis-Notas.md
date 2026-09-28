Perfecto, arrancaste por el párrafo uno. No te lo toco ni te lo resuelvo. Acá van los criterios.

# Cómo leer el párrafo

## 1. Tres pasadas, no una

El error más caro es empezar por las reglas. Van últimas.

| Pasada | Qué buscás | Por qué va antes |
|---|---|---|
| **1ª — Términos** | todo sustantivo del dominio: qué designa, en qué unidad | Las reglas se escriben *con* términos. Si el término es ambiguo, la regla hereda la ambigüedad |
| **2ª — Actos** | todo verbo que sea algo que alguien **hace** | Sin actos no tenés dónde poner el «cuándo» de cada regla, ni cómo detectar que falta un acto |
| **3ª — Reglas** | recién ahora | — |

En la primera pasada nuestra nos salteamos la 1ª y la 2ª, y **seis de trece errores fueron de término, no de regla**.

## 2. De cada frase salen ocho cosas, no una

Esto es lo que te preguntabas: qué mirar además de los RN.

| # | Qué extraer | Cómo se reconoce |
|---|---|---|
| 1 | **Términos** | sustantivos del negocio |
| 2 | **Atributos** | «tiene un…», «se puede conocer el…» |
| 3 | **Actos** | verbos de acción: conformar, establecer, agregar, cerrar |
| 4 | **Cardinalidades** | «al menos», «varios», «cada uno», «un» |
| 5 | **Calificadores temporales** | «en su momento de», «en el mismo acto», «luego», «al cierre» |
| 6 | **Reglas** | lo que prohíbe, calcula, compone o delimita |
| 7 | **Alcance declarado** | «se contempla en un futuro análisis» |
| 8 | **Silencios** | lo que *no* dice y debería |

Los números **5 y 8** son los que casi nadie anota y son los que más rinden. El 5 te dice si la regla es invariante o precondición — muchas veces el texto ya lo trae y uno lo pisa. El 8 es donde viven los huecos.

## 3. Cómo cortar una regla

**Una regla = una afirmación que puede ser verdadera o falsa.** Si tiene un «y», casi siempre son dos.

Y una prueba de corte que sirve mucho: **¿podés describir un intento que el sistema rechaza?**

- Sí → es una **restricción**
- No, porque calcula algo → es una **derivación** (nunca se «viola»)
- No, porque dice qué queda afuera → es **alcance** (no es regla)
- Rechaza una *composición*, no una operación → es **estructura**

Mezclar las cuatro en una sola numeración fue el defecto 1 de mi lista.

## 4. Las cuatro preguntas para clasificar

Por cada regla, en este orden:

1. **¿Sobrevive en papel?** Si la razón es la pantalla, la base o el protocolo, no es regla de negocio.
2. **¿Vale siempre, o antes de un acto?** → La prueba que nunca falla: **¿vale en el instante en que el objeto nace?** Si el objeto nace sin cumplirla, no es invariante.
3. **¿Quién la verifica hoy?** Contestá aunque la respuesta sea **«nadie»**. Ese «nadie» es un hallazgo, no un hueco de tu tabla.
4. **¿Quién tiene los datos para decidirla?** Si ningún objeto los tiene a mano, escribilo. Esas son las que después terminan fuera del modelo.

## 5. Las trampas de este texto

Con ejemplos que ya conocés:

| Trampa | Qué hacer |
|---|---|
| **Palabra no verificable** — «coherente», «adecuado», «razonable» | **Recuperá la intención, no propongas un reemplazo.** Es el error que cometí |
| **Un concepto con varios nombres** | anotá los dos y elegí el preferente, no los unifiques en la fuente |
| **Un nombre con dos sentidos** | numerá los sentidos; decí cuál usa cada regla |
| **Silencio de alcance** | si declara fuera una operación y calla sobre otra parecida, eso es un silencio, **no** una decisión |
| **Invariante por accidente** | vale siempre solo porque falta la operación que lo rompería |

## 6. Anotá en tres columnas, no en una

Es lo único no negociable del método:

| Letra | Intención | Propuesta |
|---|---|---|
| lo que dice, **textual y entre comillas** | qué tuvo que haber querido decir, con el porqué | lo que vos harías |

Para trabajo a mano con lápiz, con estos campos alcanza: **id · enunciado atómico · dónde lo dice · letra · intención · especie · momento · quién verifica · prueba**. La ficha de trece campos es para un agente, no para vos.

Y **nunca escribas en la columna Letra algo que no esté entre comillas en el texto.** Ahí es donde se cuela la propuesta disfrazada de fuente.

## 7. Lo que todavía NO hagas

- **No decidas dónde vive en el código.** Eso es diseño, viene después.
- **No corrijas el enunciado.** Anotalo como divergencia.
- **No descartes una regla por vaga.** Si existe, dice algo *más* que su vecina.

---

Un aviso sobre el párrafo que elegiste: **tiene un calificador temporal en la primera oración** que decide si una de sus reglas es invariante o precondición. Está escrito, no hay que inferirlo. Si lo encontrás, ya vas bien.

Dale, arrancá. Cuando tengas la primera tanda me la traés y te la reviso contra el texto.

---

Buena pregunta, porque «unidad» ahí significa **cinco cosas distintas** y las cinco rompen reglas si las salteás.

## Los cinco sentidos

| # | Sentido | La pregunta que hacés | Ejemplo del dominio |
|---|---|---|---|
| 1 | **Unidad de medida** | ¿en qué se mide? | superficie → hectáreas; producción → dinero; peso → kilos |
| 2 | **Qué cuenta como uno** | ¿cuál es el individuo? | «cabezas»: ¿las vacas madres son cabezas? ¿los terneros destetados? |
| 3 | **Unidad de identidad** | ¿qué hace que dos menciones sean la misma cosa? | una parcela se identifica por **el par campo + identificador**, no por el identificador solo |
| 4 | **Unidad temporal** | ¿qué tipo de cosa es el tiempo acá? | «período»: ¿un año? ¿una campaña? ¿un rango de fechas? |
| 5 | **Ámbito** | ¿dentro de qué contexto vale? | «único dentro de la estancia» contra «único dentro del propio campo» |

El 1 es el que todos anotan. Los otros cuatro son los que rinden.

## Por qué paga: lo que la columna de unidades te va a demostrar sola

Mirá qué pasa cuando le ponés unidad al rendimiento de cada tipo:

| Tipo | Unidad del rendimiento |
|---|---|
| Agrícola | toneladas **/ hectárea** |
| Cría | terneros **/ cien vacas madres** |
| Recría | kilos **/ cabeza** |
| Invernada | kilos ganados **/ cabeza** |

Cuatro unidades distintas. **Conclusión que cae sola: los rendimientos no se pueden sumar ni promediar.** No es una opinión de diseño, es aritmética.

Y el contraste, que es la otra mitad:

| Tipo | Unidad de la producción |
|---|---|
| los cuatro | **dinero** |

Misma unidad → **sí se pueden sumar**. Por eso el enunciado pide «producción total» y **no** pide «rendimiento total». El texto ya lo sabía; la columna de unidades te lo hace visible.

Ese es el tipo de hallazgo que una columna aparentemente burocrática te regala.

## Dos trampas que trae la unidad

**1. Toda unidad con denominador esconde una pregunta: ¿y si el denominador es cero?**

`terneros / vacas madres` con cero vacas madres no da cero: **no da nada**. Si el código devuelve `0`, ese cero *significa* «no sé» pero *se lee* «rindió pésimo», y después se promedia con los demás como si fuera un dato. Anotá la unidad y la pregunta aparece sola.

**2. Un porcentaje no es una unidad: es la ausencia de unidad.**

«Productividad general (porcentaje)» es hectáreas sobre hectáreas — se cancelan. Está bien. Pero eso te obliga a escribir **qué sobre qué**, y ahí es donde se ve si numerador y denominador miden lo mismo. Si el denominador incluye tierra que el numerador nunca podría contar, el indicador tiene techo y nadie lo sabe.

## El tip práctico

**Escribí la unidad siempre, incluso cuando no hay.**

```
| Término        | Designa                        | Unidad        |
| nombre         | cómo se llama el establecimiento | —           |
| superficie     | extensión de tierra             | hectáreas     |
| período        | ?                               | ?             |
```

Poner `—` cuesta nada y te obliga a mirar cada término. Y cuando te salga un `?` en la columna —como te va a pasar con «período»— ése es el hallazgo: **un término que el enunciado usa como si fuera obvio y nunca define**.

Regla corta para llevarte: **un número sin unidad no es un dato, es un rumor.**

---

## Qué es

**La unidad es el «de qué» de una cantidad.** Dice qué significa *uno*.

`500` no es un dato. `500 hectáreas` sí. La unidad es lo que convierte un número en un hecho.

Y sirve para una sola cosa, pero es decisiva: **decirte qué podés sumar y qué podés comparar.** Solo se suman cosas de la misma unidad. Si dos números tienen distinta unidad, sumarlos es un error aunque la cuenta dé.

## Cómo la ves en el texto

Cinco señales, en orden de qué tan fácil es verlas:

**1. Está entre paréntesis.** El autor te la regala.
> «una superficie determinada **(hectáreas)**» · «la producción total **(dinero)**» · «la productividad general **(porcentaje)**»

**2. Está adentro del nombre.**
> «**cantidad de cabezas**» · «precio de la **tonelada**» · «**kilos** logrados»

El sustantivo ya trae la unidad pegada: *toneladas*, *kilos*, *cabezas*, *terneros*.

**3. Aparece después de «por» o «cada».** Esas dos palabras **fabrican** unidades compuestas.
> «toneladas **por** hectárea» · «kilos **por** cabeza» · «terneros **cada cien** vacas madres»

Donde leas «por» o «cada», hay una división y hay un denominador. Marcalo.

**4. La delatan los verbos de comparar y sumar.** Ésta es la mejor.
> «la **suma** de hectáreas» · «**supera** la superficie» · «debe ser **igual** a»

Cada vez que el texto suma o compara dos cosas, **te está diciendo que esas dos cosas tienen la misma unidad**. Si al anotarlas te salen distintas, encontraste un error.

**5. La pregunta que decide.** Poné `¿cuántos qué?` después del término:

| Término | ¿Cuántos qué? | Unidad |
|---|---|---|
| superficie | cuántas **hectáreas** | ha |
| cabezas | cuántos **animales** | animal |
| nombre | *¿cuántos nombre?* — no tiene sentido | **—** |

Si la pregunta no tiene sentido, no hay unidad. Ponés `—` y seguís.

## El procedimiento con lápiz

Sobre el párrafo, tres pasadas de marcador:

1. **Subrayá** todo lo que podría responderse con un número.
2. **Encerrá** los paréntesis, los «por» y los «cada».
3. Para cada subrayado, escribí al margen: `¿cuántos qué?` → la respuesta.

Lo que quede sin respuesta es tu hallazgo. Va a pasarte con **«período»**: el enunciado lo usa cuatro veces con cuatro nombres y nunca dice si es un año, una campaña o un rango de fechas. No tiene unidad declarada — y sin embargo se compara y se ordena.

## Cuándo no hay unidad, y está bien

`nombre`, `descripción`, `identificador`. No son cantidades, son etiquetas. Anotá `—` y listo.

Pero ojo con una: **`identificador` no tiene unidad, pero tiene ámbito** — «único dentro de la estancia» o «único dentro del campo». Ese es el sentido 5 del mensaje anterior, y en la tabla va en su propia columna. No lo metas en la de unidad.

> -----------


**No arranques por una tabla.** Una tabla sola es la ensalada otra vez, ordenada en columnas. Primero se corta, después se clasifica, y al final se tabula.

## Paso 0 — Numerá las oraciones

Tu párrafo tiene cuatro. Ponéles `O1` a `O4`. Ya tenés anclas: de ahora en más todo lo que encuentres cita dónde estaba.

## Paso 1 — Cortá cada oración en afirmaciones

Acá se despeja la ensalada. Cortá en cada **«y»**, en cada coma que une dos cosas que podrían vivir solas, y en cada «de los cuales / donde / tanto… como».

Una afirmación = algo que puede ser verdadero o falso por sí solo.

Numerá `O1.a`, `O1.b`, `O1.c`…

> Te vas a sorprender: cuatro oraciones dan **entre diez y quince** afirmaciones.

## Paso 2 — Etiquetá cada fragmento con una letra, sin pensar

Una sola pasada, rápida, mecánica. **No decidas nada todavía**, solo etiquetá:

| Letra | Qué es |
|---|---|
| **T** | término — nombra una cosa del negocio |
| **A** | atributo — algo que una cosa tiene |
| **V** | acto — algo que alguien hace |
| **C** | cardinalidad — cuántos |
| **M** | momento — «en el acto de», «luego», «al cierre» |
| **R** | candidata a regla |
| **E** | alcance — lo que queda afuera |
| **?** | no sé |

Con otro dominio, para que veas la mecánica:

```
"Una biblioteca tiene un nombre y está compuesta por al menos
 una sala en el momento de su apertura."

O1.a  "biblioteca"                          T
O1.b  "tiene un nombre"                     A
O1.c  "compuesta por al menos una sala"     R + C
O1.d  "sala"                                T
O1.e  "en el momento de su apertura"        M   ← califica a O1.c
O1.f  "apertura"                            V   ← ¡hay un acto acá!
```

Fijate lo que pasó: de una oración que «dice una cosa» salieron seis, **dos actos y un momento que nadie hubiera anotado**. Y ese `M` de O1.e es el que decide si O1.c es invariante o precondición.

**Los `?` no los resuelvas.** Son oro: son las preguntas para el autor.

## Paso 3 — Recién ahora, tres tablas. No una.

Separadas, porque son cosas de distinta naturaleza y mezclarlas es volver al principio:

**Tabla 1 — Términos** *(las T y las A)*
```
| Término | Designa | Unidad | Ámbito | De dónde |
```

**Tabla 2 — Actos** *(las V y las M)*
```
| Acto | Quién lo hace | Qué recibe | Qué cambia | De dónde |
```

**Tabla 3 — Reglas** *(las R, las C y las E)*
```
| Id | Letra (textual) | Intención | Especie | Momento | Quién verifica | De dónde |
```

## Paso 4 — La regla del orden

Llenalas **en ese orden**, y no vuelvas atrás hasta terminar cada una:

1. Términos primero. Si al escribir una regla te falta un término, **volvé a la tabla 1 y agregalo** — no lo definas dentro de la regla.
2. Actos segundo. Sin actos no tenés dónde poner la columna «Momento» de la tabla 3.
3. Reglas último.

## El consejo que más te va a servir

**No busques que la primera pasada quede bien.** El paso 2 es sucio a propósito: etiquetar rápido y mal es mejor que etiquetar despacio y bien, porque lo que importa es que **ningún fragmento quede sin etiqueta**. Los errores de etiqueta se arreglan solos al armar las tablas; los fragmentos que no anotaste, no.

---

Arrancá por el **paso 1** nomás: cortá las cuatro oraciones en fragmentos y numeralos. Traémelos así, crudos, sin clasificar. Sobre eso te digo si el corte está bien, que es lo que decide todo lo demás.


>

Se llama **sujeto tácito** — también *elíptico*, *omitido* o *implícito*. Si querés ser preciso sobre el mecanismo: **sujeto desinencial**, porque quien lo identifica es la desinencia del verbo. En lingüística moderna vas a encontrarlo como *sujeto nulo*.

En tu 01.b el caso tiene nombre propio: **elipsis del sujeto por correferencia**. Se omite porque está coordinado con 01.a y comparten sujeto.

## Por qué pasa

El español es una **lengua de sujeto nulo**: el verbo ya carga la persona y el número, así que repetir el sujeto suena redundante. No es una omisión descuidada del autor, es la forma normal de la lengua. Por eso te lo vas a encontrar en casi todos los enunciados.

## Cómo lo recuperás, mecánicamente

Dos marcas, y con las dos alcanza:

1. **La desinencia del verbo** → `está` te da tercera persona del singular.
2. **La concordancia del participio o adjetivo** → `compuest**a**`, femenino singular.

Femenino singular, tercera persona → en el contexto solo hay un candidato: **«una estancia agropecuaria»**.

Esa segunda marca es la que más rinde y la que menos se usa. Cuando tengas dudas de a quién se refiere un fragmento, **mirá el género del participio**: te descarta candidatos sin que tengas que interpretar nada.

## La consecuencia para tu tabla

Acá está lo importante, y es una trampa fina:

> **Restituís el sujeto, pero marcando que lo restituiste.**

Porque la columna `Letra` es **textual**. Si escribís «Una estancia agropecuaria está compuesta por…» entre comillas, estás poniendo en la columna de la letra algo que el texto **no dice en ese lugar**. Es exactamente la contaminación que todo el método trata de evitar, en su versión más inocente.

La notación que te propongo:

```
01.b  Letra:       "está compuesta por al menos un campo en su momento de conformación"
      Sujeto:      ⟨una estancia agropecuaria⟩  — tácito, restituido por concordancia (01.a)
```

Los corchetes angulares —o lo que uses— dicen: *esto lo puse yo, y lo puedo justificar*. Sin eso, en la tercera relectura ya no vas a saber qué era del autor y qué tuyo.

## Tu corte está bien

El «y» de 01 coordina **dos predicados** —*tiene* / *está compuesta*—, no dos sustantivos. Corta. ✅

Dos cosas para el paso 2, ya que estás etiquetando:

- **«en su momento de conformación»** no es afirmación aparte: es **calificador de momento** (`M`) de 01.b. Lo dejaste adentro, que es correcto — pero etiquetalo aparte, porque es el que decide si 01.b es invariante o precondición.
- Ahí adentro hay además un sustantivo que esconde un verbo: **«conformación»**. Eso es un **acto** (`V`), y va a la tabla 2. Si lo dejás como sustantivo se te va a términos y perdés el acto.

## No lo confundas con su primo

**Sujeto tácito** ≠ **elipsis verbal**. El primero omite el sujeto y es inofensivo. El segundo omite el **verbo** —«el casco a cargo de un administrador y los puestos ⟨a cargo⟩ de un encargado»— y produce **falsas uniones**: parece una afirmación y son dos.

En tu oración 02 hay algo de esa familia. Cuando llegues, frená ahí.

>

Frenaste en el lugar correcto: esa oración trae **tres cosas nuevas** que no aparecían en la primera. Vamos por partes.

## Primero, un bug de numeración

Etiquetaste el fragmento como `01.b.5`, pero pertenece a `02.a`. Y `01.b.5` ya existe: es «conformación». Si se repite el id, perdés el anclaje — que es lo único que hace verificable todo lo demás.

Va como **`02.a.1`**, y la referencia apunta a `01.b.5`.

## Lo que pasa con «En el mismo acto»

Tu instinto fue correcto —pusiste `ref: 01.b.5`— pero la etiqueta no: **no es `V`, es `M`**.

«En el mismo acto» **no crea un acto: señala uno que ya existe**. Es una *anáfora*: una expresión que no tiene contenido propio y toma el del antecedente. Si la etiquetás `V`, en la tabla de actos te van a aparecer dos actos donde hay uno solo.

La regla, que te va a servir para todo el enunciado:

> **El acto se registra una sola vez, en el fragmento donde se lo nombra. Las menciones posteriores son calificadores de momento con referencia.**

Señales de anáfora a vigilar: *el mismo*, *dicho*, *ese*, *tal*, *ahí*, *entonces*, *luego*.

## Y acá está el hallazgo bueno

Si «se establece el casco» ocurre **dentro** del acto de conformación, entonces `establecer` **no es un acto aparte**: es un **efecto** de `conformación`.

Y eso reorganiza lo que llevás hecho. No tenés dos oraciones con dos actos. Tenés **un acto con varios efectos**:

```
ACTO: conformación de la estancia          (01.b.5)
  efecto 1 — la estancia queda con un nombre           (01.a)
  efecto 2 — se incorpora al menos un campo            (01.b.2)
  efecto 3 — se establece un casco central             (02.a)
  efecto 4 — se establecen varios puestos rurales      (02.b, viene)
```

Por eso el autor escribió «en el mismo acto»: **te estaba avisando que no abrieras un acto nuevo.**

En la tabla 2, `conformación` es **una fila** con la columna «qué cambia» acumulando los cuatro efectos. Ése es el modo de resolver la anáfora: no se duplica el acto, se acumulan los efectos.

## La fragmentación que te propongo

```
02.a.1 [ "En el mismo acto"                M  ]  ref 01.b.5 — anáfora, no acto nuevo
02.a.2 [ "se establece"                    V  ]  efecto de 01.b.5. Agente oculto por «se» → ¿quién?
02.a.3 [ "un casco central"                T+C]  C: exactamente uno
02.a.4 [ "central"                         ?  ]  ¿distingue de otro casco, o es decorativo?
02.a.5 [ "a cargo de un Administrador"     R  ]  todo casco tiene un administrador a cargo
02.a.6 [ "Administrador"                   T  ]  rol
```

Tres comentarios sobre eso:

**`02.a.2` la dejo en `V` a propósito**, aunque acabo de decirte que es un efecto. El paso 2 es sucio por diseño: etiquetás rápido, y la distinción acto/efecto se resuelve **al armar la tabla 2**, no ahora. Si te ponés a decidirlo fragmento por fragmento no terminás más.

**El «se» de «se establece» oculta al agente.** ¿Quién establece el casco? El texto no lo dice. Cuando llenes la columna «quién lo hace» te va a quedar vacía — y eso **no es un descuido tuyo, es un hallazgo**. Anotalo como `?`.

**`02.a.5` no corta** —«a cargo de un Administrador» no tiene verbo, modifica a «casco»— pero **sí contiene una afirmación**: todo casco tiene alguien a cargo. Por eso va `R` aunque no sea oración independiente. Es el caso del calificador que aporta un hecho.

## Dos cosas para anotar como `?`

**«central»** — si hay exactamente un casco, ¿de qué lo distingue? O es decorativo, o hay cascos no centrales que el enunciado no menciona. No lo resuelvas: es pregunta para el autor.

**La mayúscula de «Administrador»** — el autor la escribió con mayúscula, y dos líneas después escribe «encargado» con minúscula. Eso suele significar que trata uno como **tipo** y el otro como dato suelto. Cuando llegues a 02.b vas a tener que decidir si son **el mismo concepto con dos nombres** o dos roles distintos. Es la inconsistencia I-02 del catálogo, y va a ser tu primera de verdad.

---

Seguí con `02.b`. Ahí está la relativa explicativa —«de los cuales cada uno se puede conocer…»— que **sí corta**, pero con restitución del antecedente. Traémela y la vemos.

>

Confirmado, y es peor de lo que pensaba.

## La respuesta a tu pregunta

«Central» es un **adjetivo explicativo** (*no restrictivo*). El subtipo clásico se llama **epíteto**: el adjetivo que expresa una cualidad inherente del sustantivo y **no recorta el conjunto de referentes**.

La prueba que lo decide, y es mecánica:

> **Un adjetivo restringe solo si existe un conjunto de contraste del cual recortar.**

- «un casco central» → artículo indefinido, singular, referente nuevo, **no hay otros cascos en el discurso**. No hay de dónde recortar → **explicativo**.
- Tu contraejemplo, «dos cascos, y uno central» → **construiste el conjunto de contraste**. Ahí sí recorta.

Y la prueba de la supresión lo confirma sola: sacá «central» y el referente es idéntico, porque hay uno solo. Si al sacarlo el referente no cambia, el adjetivo no restringe.

Así que la pregunta no iba al autor: **se contestaba con el determinante**.

## Qué especialidad lo hubiera resuelto

Ninguna nueva. **La gramática — y ya estaba en el equipo.**

Y ahí está lo feo: el criterio de segmentación tiene, en su §4.3, una **jerarquía de seis indicios** cuyo indicio número uno es *«determinante del antecedente»* y cuya prueba de respaldo es la supresión. Es exactamente la herramienta que resuelve «central». La tengo escrita, en el archivo, desde ayer.

Lo que falló fue la generalización: **esa jerarquía está escrita solo para relativas.** Grepeé el documento y `adjetivo especificativo`, `adjetivo explicativo`, `epíteto` y `modificador` no aparecen ni una vez.

La distinción restrictivo / no restrictivo **no es una propiedad de las relativas: es una propiedad de todo modificador** — adjetivos, relativas, complementos preposicionales. El gramático la desarrolló para el caso que le pedí, y ni él ni yo la subimos un nivel. Yo apliqué la herramienta donde decía la etiqueta y no donde servía.

## Y hay una lección más incómoda

Levanté un `?` que podía contestar con un criterio que tenía en la mano.

Eso no es prudencia: **es pasividad disfrazada de rigor.** Y es caro de un modo específico — si inundo al autor con preguntas que se contestaban solas, las tres que de verdad necesitan su decisión se pierden en el montón.

Falta una compuerta que no existe en ninguno de los dos conocimientos:

> **Antes de elevar un `?`, pasarlo por los criterios disponibles. Un `?` que un criterio en mano resuelve no es una pregunta para el autor: es un criterio no aplicado.**

Es el reverso exacto de lo que veníamos hablando. Discutimos mucho el agente que decide de más; éste es el agente que **pregunta de más**, y se disfraza de cuidadoso.

## Qué corrijo

Tres cosas, y son de distinto tamaño:

1. **En tu tabla**: `02.a.4` deja de ser `?`. Queda `A` —atributo explicativo de `casco`— con la nota de que no restringe.
2. **En el criterio de segmentación**: generalizar §4.3 de «relativas» a «todo modificador», y agregar la fila del adjetivo explicativo con la prueba del conjunto de contraste.
3. **En el conocimiento de deducción**: la compuerta del `?`, como obligación de búsqueda. Va con las otras siete.

Las tres las puedo hacer ahora. ¿Arranco, o preferís seguir con `02.b` y las dejo anotadas para después?


>

Diste con algo real, y tiene nombre en lógica: **la cópula «ser» encubre operaciones distintas**, y el español las escribe todas igual. Es un problema conocido —la distinción entre el «es» de identidad, el de predicación y el de inclusión de clase es doctrina estándar desde Frege y Russell— pero acá no es filosofía: **cada sentido rutea a una tabla distinta.**

> Lo digo sin comillas ni cita: no coteje ninguna fuente. Tomalo como criterio, no como autoridad.

## Los cuatro «es» que te vas a encontrar

| Sentido | Forma | Qué hace | Dónde termina |
|---|---|---|---|
| **Definición** | «X es ‹descripción›» | dice **qué es** X | **Tabla 1**, columna *Designa* |
| **Pertenencia** | «X es un Y» — X individuo | dice que X **está en** la categoría Y | **Tabla 3**, `R(estructura)` |
| **Inclusión** | «los X son Y» — X tipo | dice que el tipo X **está contenido** en Y | **Tabla 3**, `R(estructura)` |
| **Atribución** | «X es ‹adjetivo›» | le cuelga una **cualidad** | **Tabla 1**, como `A` |

Y en los cuatro casos, **el sustantivo que aparece a la derecha es un término nuevo** que hay que dar de alta. Por eso decías bien: son dos cosas a la vez. El «es» **introduce** un concepto y **afirma** algo sobre él, en el mismo acto.

## Cómo los distinguís

Prueba de sustitución. Reemplazá «es» y fijate cuál aguanta:

| Reemplazo que funciona | Sentido |
|---|---|
| «se define como», «significa» | **definición** → glosario |
| «es un tipo de», «pertenece a la categoría» | **pertenencia o inclusión** → `R(estructura)` |
| «tiene la cualidad de» | **atribución** → `A` |
| «es el mismo que» | **identidad** → dos nombres para una cosa, ojo: es la inconsistencia I-02 |

El último es el que más rinde para cazar problemas: si «es» se puede leer como identidad, **encontraste un concepto con dos nombres**.

## Aplicado a tu oración

*«Tanto el casco como el puesto son instalaciones»*

Sustituís: «el casco **es un tipo de** instalación» → aguanta. Es inclusión. `R(estructura)`. ✅

Pero mirá esto, que es la parte fina y te toca de lleno:

| | Qué es gramaticalmente | Qué es ontológicamente |
|---|---|---|
| «el casco» | singular con artículo definido | **un individuo** — hay exactamente uno |
| «el puesto» | singular con artículo definido | **un tipo** — hay varios puestos |

Misma forma, distinta naturaleza. El casco **pertenece** a la categoría; el puesto **está incluido** en ella como subtipo.

> El autor los puso en paralelo gramatical, pero **no son paralelos en el modelo**: uno es instancia única y el otro es clase con muchas instancias.

Eso no es una objeción al texto —en castellano se dice así y se entiende— pero si trasladás el paralelismo al modelo te queda `Casco` y `Puesto` como dos clases hermanas, cuando en realidad hay **una clase con varias instancias y una cosa que hay una sola**. Es material para cuando decidas el diseño.

## Por qué esto importa más de lo que parece

Es el mecanismo detrás de la confusión que el catálogo llama la más cara, `A` contra `R`. Fijate:

- *«el casco es central»* → atribución → `A` → **no genera validación**
- *«el casco es una instalación»* → inclusión → `R(estructura)` → **sí es afirmación del modelo**

Misma palabra, mismo lugar en la oración, **destinos opuestos**. Y lo único que los separa es qué hay a la derecha del verbo: un adjetivo o un sustantivo.

Ahí tenés una regla de tres palabras para la hoja de bolsillo:

> **«es» + adjetivo → `A`. «es» + sustantivo → `R(estructura)` + `T`.**

---

Esto me parece que se gana un lugar en el catálogo, en la ficha de `R` y en el desempate `A`/`R`. Lo anoto para el próximo lote de parches, salvo que quieras que lo mande ahora.

>

Le pasé los dos defectos, y el segundo con una advertencia que me parece la parte delicada del encargo.

## Mi opinión, mientras tanto

Sobre tu síntesis —*la instalación central es el casco, las periféricas son los puestos*—: **la compro como hallazgo, y la quiero rotulada.**

Es verdadera, se deriva de dos oraciones y el negocio la reconocería. Pero **no está escrita en ninguna parte**, y ahí está el riesgo: una pasada que busca «lo que el texto sugiere» es la puerta de entrada a atribuirle al autor cosas que no dijo. Es el modo de falla opuesto al literalismo que veníamos corrigiendo, y no quiero cambiar un defecto por el otro.

Por eso le pedí explícitamente que defina **con qué rótulo sale el producto de esa pasada y qué evidencia hay que exhibir**. Mi posición, que veremos si sostiene: sale como `Intención`, nunca como `Letra`, y **tiene que nombrar las dos anclas que la producen**. Tu caso las tiene —oración 2 y oración 3—, y eso es lo que la separa de una ocurrencia.

La prueba que propongo: **si no podés señalar los dos fragmentos que, combinados, la producen, no es una afirmación distribuida: es una conjetura tuya.** Y va con el rótulo que corresponde.

## Y una cosa que quiero dejar dicha sobre el método

Lo que encontraste no es un error de aplicación, es **una limitación de diseño**. Cortar y atomizar es correcto y necesario —sin eso no hay anclaje ni verificación— pero tiene un costo que nadie había declarado: **lo que vive entre fragmentos se pierde, y se pierde en silencio**.

No hay síntoma. No queda un hueco visible. El catálogo cierra, las cuentas dan, y la afirmación nunca existió. Es el mismo patrón que la pasividad de la que hablamos: el entregable está impecable y le falta algo que nadie puede notar que falta.

Si la pasada de recomposición sobrevive al análisis del gramático, va a ser el segundo aparato que agregamos por esa razón — y el primero que corrige un defecto del procedimiento y no del que lo ejecuta.

---

Seguí con el párrafo. Cuando llegue el dictamen te aviso, y ahí vemos si tu «instalación central» entra al catálogo como la primera afirmación distribuida documentada.

>
Lo que decís reencuadra mi error mejor que mi propio diagnóstico, y me obliga a corregir algo del catálogo.

## La redundancia no es ruido: es corrección de errores

Yo había clasificado como **inconsistencia** —I-07, «regla que es copia débil de otra»— el hecho de que dos reglas digan casi lo mismo. Bajo tu lectura eso está mal planteado. **La reafirmación es el mecanismo del autor, no su descuido.**

El lenguaje natural es redundante a propósito: es lo que permite que el sentido sobreviva al ruido. Cuando el autor dice algo dos veces con palabras distintas, no está siendo torpe: está poniendo **bits de paridad**. Y ahí está tu punto fino, que es el que me parece el mejor:

> **Un concepto dicho una sola vez no se puede verificar. Dicho dos veces, se puede triangular. Donde las dos formulaciones no cierran, ahí está la ambigüedad.**

O sea: **la redundancia es lo que vuelve *detectable* la ambigüedad.** Sin reafirmación, una lectura equivocada pasa sin dejar rastro. Con reafirmación, produce una contradicción — y la contradicción es el detector.

## Y eso explica mi error con RN-08 mejor que «literalismo»

«Coherente» no estaba solo. Sobre esa misma idea el enunciado tiene **tres formulaciones**: el «no supera» del CU-7, el «coherente» del párrafo, y el denominador de la productividad general. Eran tres mediciones del mismo punto.

Yo tomé la más débil, la declaré defectuosa y **propuse reemplazarla**. Destruí la redundancia en vez de usarla para triangular. Si hubiera cruzado las tres, la intención salía sola — que es exactamente lo que después hizo el analista: la sostuvo con tres evidencias del propio texto, sin necesitar tu aclaración.

**El error no fue ser literal. Fue tratar la repetición como defecto en lugar de como instrumento.**

## Lo que proponés es otro algoritmo, y conviene decirlo así

| | Lo que hace hoy el método | Lo que proponés |
|---|---|---|
| Por fragmento | **una** lectura, decidida localmente | **una gama** de lecturas posibles |
| Cómo se resuelve | clasificando cada uno por su cuenta | **sintonizando** el conjunto: la asignación que hace consistentes a todas |
| Qué produce el desacuerdo | nada, no se nota | **el punto de ambigüedad, localizado** |

No es un paso más: es **propagación de restricciones** en vez de clasificación local. Y tenés razón en que la segmentación sola no alcanza —ni en texto ni en imagen—, porque la segmentación supone que cada parte se entiende sola, y justamente lo que decís es que no.

## Dónde te pondría un freno

Una cosa es la idea y otra el procedimiento. Si cada fragmento abre tres lecturas y hay quince fragmentos, son catorce millones de combinaciones. **No se puede sintonizar por enumeración.**

Necesita dos controles, y sin ellos esto es hermoso e inaplicable:

1. **No se abren lecturas múltiples en todos lados.** Solo donde hay una señal de ambigüedad: una palabra vaga, un término que aparece dos veces, un adjetivo con contraste posible.
2. **La propagación es local y en cadena**, no global. Se resuelve el fragmento más restringido primero —el que admite una sola lectura— y esa decisión recorta las lecturas de sus vecinos. Como un sudoku: se empieza por la casilla forzada.

Y un tercero, que es de honestidad: **no toda repetición es funcional.** Algunas son estilo. Distinguir la reafirmación que triangula de la que solo adorna es parte del problema, no algo dado.

## Qué dicen los especialistas

El gramático sigue corriendo — le mandé el contraste, el paralelismo, la separación entre restringir e informar, y la afirmación distribuida. **Esto no se lo mandé, porque lo dijiste después**, y es lo más grande de todo: no es un indicio más, es un cambio de algoritmo.

Se lo agrego ahora.

>

Lo que decís reencuadra mi error mejor que mi propio diagnóstico, y me obliga a corregir algo del catálogo.

## La redundancia no es ruido: es corrección de errores

Yo había clasificado como **inconsistencia** —I-07, «regla que es copia débil de otra»— el hecho de que dos reglas digan casi lo mismo. Bajo tu lectura eso está mal planteado. **La reafirmación es el mecanismo del autor, no su descuido.**

El lenguaje natural es redundante a propósito: es lo que permite que el sentido sobreviva al ruido. Cuando el autor dice algo dos veces con palabras distintas, no está siendo torpe: está poniendo **bits de paridad**. Y ahí está tu punto fino, que es el que me parece el mejor:

> **Un concepto dicho una sola vez no se puede verificar. Dicho dos veces, se puede triangular. Donde las dos formulaciones no cierran, ahí está la ambigüedad.**

O sea: **la redundancia es lo que vuelve *detectable* la ambigüedad.** Sin reafirmación, una lectura equivocada pasa sin dejar rastro. Con reafirmación, produce una contradicción — y la contradicción es el detector.

## Y eso explica mi error con RN-08 mejor que «literalismo»

«Coherente» no estaba solo. Sobre esa misma idea el enunciado tiene **tres formulaciones**: el «no supera» del CU-7, el «coherente» del párrafo, y el denominador de la productividad general. Eran tres mediciones del mismo punto.

Yo tomé la más débil, la declaré defectuosa y **propuse reemplazarla**. Destruí la redundancia en vez de usarla para triangular. Si hubiera cruzado las tres, la intención salía sola — que es exactamente lo que después hizo el analista: la sostuvo con tres evidencias del propio texto, sin necesitar tu aclaración.

**El error no fue ser literal. Fue tratar la repetición como defecto en lugar de como instrumento.**

## Lo que proponés es otro algoritmo, y conviene decirlo así

| | Lo que hace hoy el método | Lo que proponés |
|---|---|---|
| Por fragmento | **una** lectura, decidida localmente | **una gama** de lecturas posibles |
| Cómo se resuelve | clasificando cada uno por su cuenta | **sintonizando** el conjunto: la asignación que hace consistentes a todas |
| Qué produce el desacuerdo | nada, no se nota | **el punto de ambigüedad, localizado** |

No es un paso más: es **propagación de restricciones** en vez de clasificación local. Y tenés razón en que la segmentación sola no alcanza —ni en texto ni en imagen—, porque la segmentación supone que cada parte se entiende sola, y justamente lo que decís es que no.

## Dónde te pondría un freno

Una cosa es la idea y otra el procedimiento. Si cada fragmento abre tres lecturas y hay quince fragmentos, son catorce millones de combinaciones. **No se puede sintonizar por enumeración.**

Necesita dos controles, y sin ellos esto es hermoso e inaplicable:

1. **No se abren lecturas múltiples en todos lados.** Solo donde hay una señal de ambigüedad: una palabra vaga, un término que aparece dos veces, un adjetivo con contraste posible.
2. **La propagación es local y en cadena**, no global. Se resuelve el fragmento más restringido primero —el que admite una sola lectura— y esa decisión recorta las lecturas de sus vecinos. Como un sudoku: se empieza por la casilla forzada.

Y un tercero, que es de honestidad: **no toda repetición es funcional.** Algunas son estilo. Distinguir la reafirmación que triangula de la que solo adorna es parte del problema, no algo dado.

## Qué dicen los especialistas

El gramático sigue corriendo — le mandé el contraste, el paralelismo, la separación entre restringir e informar, y la afirmación distribuida. **Esto no se lo mandé, porque lo dijiste después**, y es lo más grande de todo: no es un indicio más, es un cambio de algoritmo.

Se lo agrego ahora.


---

>

Sí. Y creo que se puede nombrar el axioma que falla.

## La cuenta de los parches

En esta sesión le agregamos diez: la triple lectura, las obligaciones de búsqueda, la compuerta del `?`, las marcas, la nota `R(estructura)`, la generalización a todo modificador, la pasada de recomposición, las lecturas múltiples, la sintonización, y ahora los tres diagnósticos.

Pero **cinco de esos diez son el mismo parche con distinto disfraz**:

| Parche | Lo que en realidad dice |
|---|---|
| Afirmación distribuida | el sentido vive **entre** fragmentos |
| Redundancia y triangulación | el sentido lo fijan **varios** fragmentos juntos |
| Sintonización | las lecturas son **interdependientes** |
| Premisa faltante | el conjunto **subdetermina** |
| Conjunto de contraste mal buscado | el ámbito relevante **no es el fragmento** |

Los cinco dicen lo mismo:

> **El fragmento atómico no es la unidad de sentido.**

Y ése es exactamente el supuesto del modelo: *un texto es un conjunto de fragmentos independientes, cada uno con sentido determinado, que se clasifican localmente*. Cada parche fue una corrección a ese supuesto. Por eso no paran de aparecer: no estamos tapando agujeros distintos, estamos tapando el mismo agujero desde diez ángulos.

## Qué modelo se ajusta a los datos

Si el fragmento no es la unidad de sentido, hay que preguntarse **para qué sirve entonces la segmentación**. Y la respuesta que los datos sostienen es: **sirve para anclar, no para significar**.

El anclaje fue lo mejor que hicimos —es lo que dejó ver que RN-13 chocaba con el CU-11— y no depende de que el fragmento signifique solo. Es indexación.

El modelo replanteado tendría cuatro movimientos en vez de tres:

| | Qué es | Qué produce |
|---|---|---|
| **Indexar** | cortar y numerar, mecánico y rápido | direcciones, no interpretaciones |
| **Vincular** | qué fragmentos se tocan: mismo término, mismo acto, mismo referente | la red |
| **Resolver** | propagar restricciones hasta donde converja | lecturas determinadas, y los puntos donde no converge |
| **Emitir** | términos, actos y reglas caen de la red resuelta | las tres tablas |

Bajo ese modelo, los cinco parches **dejan de ser parches**: la afirmación distribuida es una arista y no un nodo; la redundancia es sobredeterminación, que es justamente lo que permite detectar error; la premisa faltante es un subgrafo subdeterminado; el `?` es un nodo que no resuelve. Ninguno necesita una regla propia: son consecuencias.

## Dónde te pondría el límite

**El replanteo tiene que preservar el lápiz.** Lo mejor del procedimiento actual es que es mecánico y rápido —cinco segundos por fragmento— y una red de restricciones no se resuelve así.

Creo que la salida es que las dos cosas conviven en **capas distintas**, y que buena parte del enredo actual viene de tenerlas mezcladas en un solo procedimiento lineal:

- **Capa mecánica**: indexar y etiquetar. Rápida, local, por fragmento. Es lo que ya funciona.
- **Capa relacional**: vincular y resolver. Lenta, global, **una vez por párrafo, no por fragmento**.

Lo que veníamos haciendo por acumulación era meter operaciones relacionales dentro del bucle mecánico. Por eso cada una se sentía como una excepción.

## La prueba de si el replanteo sirve

Ésta me parece la parte importante, porque un replanteo también puede ser un parche más grande:

> **Después del replanteo, cada uno de los diez parches tiene que caer como consecuencia del modelo, o desaparecer. Si siguen haciendo falta como excepciones, el replanteo falló.**

Es verificable y es duro. Y de paso nos dice cuáles de los diez eran realmente estructurales y cuáles eran oficio circunstancial.

## Qué haría ahora

**Dejar que el gramático termine.** No porque su trabajo vaya a sobrevivir tal cual, sino porque es el mejor insumo que vamos a tener: está analizando justamente los casos que rompen el modelo viejo. Su dictamen es la evidencia del replanteo, no su competencia.

Y después convocar la mesa, pero no para dictaminar otro parche: **para decidir si se replantea el modelo**, con la prueba de arriba como criterio de aceptación.

Si te parece bien, cuando entregue armo la convocatoria con eso.


---

>

El argumento se sostiene, y es mejor que el mío.

> **La corrección no es abrir lecturas: es dejar de tirarlas.** No hace falta un paso de búsqueda nuevo. Hace falta guardar lo que el procedimiento ya descartaba.

## Cambio mi posición sobre el replanteo

Yo estaba por endosarte una re-arquitectura en cuatro movimientos. El gramático muestra que **el modelo ya produce la información que le falta, y la tira en cada paso**: los `?`, y —esto no estaba marcado en ningún lado— **cada decisión tomada por un indicio derrotable**, como la puntuación del autor o la consistencia interna.

O sea: en cada fragmento donde el método dudó y resolvió por un indicio débil, sabía que estaba dudando. Y lo olvidaba.

Y encaja con algo que ya teníamos escrito sin verlo:

> La sintonización es **la segunda compuerta del `?`**. La primera pregunta *¿hay un criterio que lo resuelva?* La segunda, *¿hay otra parte del texto que lo resuelva?* Recién lo que sobrevive a las dos es pregunta para el autor.

## Apliqué mi propia prueba de aceptación

Dije que el replanteo servía si los cinco parches repetidos caían como consecuencia. Los pasé:

| Parche | ¿Cae solo? |
|---|---|
| Afirmación distribuida | ✅ P13, dirigida por señales |
| Redundancia y triangulación | ✅ T-2, es el «cómo» que a I-06 le faltaba |
| Sintonización | ✅ propagación sobre la lista que ya se produce |
| Premisa faltante | ✅ escalón E2 de la escalera |
| Conjunto de contraste | ✅ los cuatro cuadrantes |

**Los cinco caen, y `P0–P11` no cambian.** La prueba pasa sin re-arquitectura.

Así que tu diagnóstico era correcto y mi remedio era caro. La formulación que me queda:

> **El modelo no era pobre: era con pérdida.** Producía la información correcta y descartaba parte en cada paso.

Que no es lo mismo, y tiene consecuencias distintas: un modelo pobre se reemplaza; uno con pérdida se tapona.

## Y sobre «central» — vos tenías razón y yo seguía medio equivocado

Tercera posición sobre el mismo adjetivo, y es la tuya:

| | Quién | Veredicto |
|---|---|---|
| 1 | yo, primero | no restringe, decorativo ❌ |
| 2 | yo, después de tu objeción | no restringe pero informa ❌ |
| 3 | el gramático | **sí restringe**: el conjunto es `instalaciones`, no `cascos`. «Nunca fue epíteto» |

Yo corregí el alcance a medias: acepté que había contraste pero seguí mirando el conjunto de los cascos. Vos ya lo habías dicho bien —*«la instalación central de la unidad productiva es el casco»*—. El conjunto siempre fueron las instalaciones.

## Lo que queda sobre la mesa

**37 parches pendientes**: 20 al criterio de segmentación (6 son P0), 14 al catálogo (5 P0), y **3 al conocimiento**, entre ellos la reformulación de I-07 que anticipábamos — el hallazgo es el desacuerdo, no la duplicación.

Y un problema que ya no puedo no decir: **el documento nuevo son 2053 líneas.** Entre los cuatro de `Conocimientos/` vamos por unas 5100. El aparato está creciendo más rápido que la capacidad de usarlo, y eso es una forma de la misma falla que venimos corrigiendo.

Antes de aplicar nada, mi propuesta: que la mesa dictamine **qué de estos 37 parches es estructural y qué es oficio circunstancial**, con tu criterio como vara —si un parche no cae como consecuencia del modelo, es candidato a no entrar—. Aplicar los 37 sin ese filtro es exactamente hacer lo que vos señalaste.

¿Convoco la mesa con ese encargo?

---

>acoto esto, a modo de estrategia , sino entendi mal, `La primera es de costo. Quince fragmentos con tres lecturas cada uno son catorce millones de combinaciones.`  una persona real no hace combinaciones de todo con todo, va agrupando conceptos y contrastando , apartando aquello que no cuadra, y luego buscanod lo que no cuadra con aquello que hace referencia para ver si cierra el concepto - sino a esa persona le lleva la vida y otras mas,- y es la diferencia estre el concepto de que ser inteligente y lo que es mecanico - lo que diferencia entre humano y maquina - y lo que busca la inteligencia artificial de copiar de la inteligencia natural separandose de lo mecanico - y de ahi que lo mecanico sea determinista y la inteligencia natural no sea determinista, pero no quite que la inteligencia natural sea exacto dentro de la información que tiene , si conjuga bien la información que tiene o que tan bien lo haga es que tanto se va a acercar al objetivo propuestos - de no hacerlo, caemos en el concepto de que tan bueno es el modelo abstracto en soportar los datos al que se va someter, si el modelo es adecuado comprenderá ese conjunto de datos, si los datos son mas diversos de lo que se penso el modelo, el modelo hará aguas por varios lados y tendrá puntos de conflictos



---

Me hacés notar un error mío y es de fondo: **yo mecanicé tu propuesta y después objeté mi mecanización.** Vos nunca dijiste «enumerá todas las combinaciones». Los catorce millones los inventé yo al traducir «sintonizar» a «producto cartesiano», y después usé esa traducción como objeción. Es un hombre de paja, y encima propio.

## Lo que describiste es un procedimiento, y no explota

Cuatro movimientos:

| | Qué hace la persona |
|---|---|
| **Agrupar** | junta lo que habla de lo mismo |
| **Contrastar** | compara dentro del grupo, no contra todo |
| **Apartar** | lo que no cuadra se **aparta**, no se descarta |
| **Cerrar** | toma lo apartado y **sigue sus referencias** a ver si cierra |

No explota porque **la búsqueda va por referencia, no por combinación**. El texto mismo dice qué se toca con qué: el término que se repite, el pronombre que apunta, el hiperónimo que aparece después. Se recorren esos hilos, que son pocos, en vez de todos los pares, que son millones.

Y el cuarto movimiento es el que yo no tenía y es el que hace el trabajo: **agarrar el residuo y perseguirlo hasta que cierre o hasta que se pruebe que le falta algo.** Eso es exactamente tu «instalación central».

## Dónde te pondría una precisión

Coincido en que lo mecánico es determinista y la inteligencia natural no, y me parece muy buena tu salvedad: **no determinista no quiere decir impreciso**. Es exacta dentro de la información que tiene, y lo que varía es qué tan bien conjuga lo que tiene.

Pero hay algo que sí cambia las cosas para lo que estamos escribiendo, y es que **este procedimiento tiene dos ejecutores distintos**:

| | Una persona | Un agente |
|---|---|---|
| Agrupar y contrastar | lo hace sin que se lo digan | si no está escrito, **enumera o inventa** |
| Apartar lo que no cuadra | lo hace y lo recuerda | lo descarta y lo olvida |
| Perseguir el residuo | lo hace si le interesa | no lo hace nunca |

Por eso hay que escribirlo. **No para que la persona aprenda a pensar —ya sabe— sino porque el agente no lo hace solo, y porque escribirlo es lo que vuelve auditable lo que la persona hizo bien.**

Ésa es la diferencia que a mí me importa acá, y es más chica que la de humano contra máquina: no estamos mecanizando la inteligencia, estamos **declarando sus pasos para poder verificarlos**. Vos podés llegar a «la instalación central es el casco» sin decir cómo; el documento tiene que poder decir de qué dos fragmentos salió.

Y de ahí sale el criterio de exactitud que me parece el bueno, y es tuyo:

> **Exacto dentro de la información que tiene.** Nunca afirmar más de lo que el texto sostiene, y conjugar bien lo que sostiene.

Eso ya está en el aparato sin que lo hubiéramos nombrado así: los rótulos son precisamente el mecanismo para no afirmar de más, y la triangulación es el mecanismo para conjugar bien.

## Y tu cierre sigue siendo el diagnóstico vigente

> si los datos son más diversos de lo que pensó el modelo, el modelo hará aguas por varios lados

Es donde estamos. El gramático mostró que el remedio es más barato de lo que yo creía —el modelo ya produce lo que le falta y lo tira— pero eso no contradice tu diagnóstico: **lo tira porque fue diseñado suponiendo que cada fragmento se entendía solo.** El desperdicio es la consecuencia del supuesto.

## Lo que cambia en cómo sigo

Iba a convocar la mesa para que filtrara los 37 parches. Con lo que acabás de decir, ése es el encargo equivocado — es contar parches otra vez.

El encargo que corresponde es:

> **¿El procedimiento describe lo que una persona competente realmente hace, o describe una mecanización de eso?** Donde describa la mecanización, reescribirlo. Y los 37 parches se juzgan contra eso: el que sobrevive es el que declara un paso real; el que desaparece es el que parchaba un paso inventado.

Con tu criterio de adecuación como vara: no cuántos parches, sino **cuántos deja de necesitar el modelo**.

¿Voy con esa convocatoria?

---

>```
Y ése es exactamente el supuesto del modelo: un texto es un conjunto de fragmentos independientes, cada uno con sentido determinado, que se clasifican localmente. Cada parche fue una corrección a ese supuesto. Por eso no paran de aparecer: no estamos tapando agujeros distintos, estamos tapando el mismo agujero desde diez ángulos.
```
> creo que capturaste una tecnica mas que el autor de la estancia persigue ,    y el tiene como concpeto - describir algo viendolo desde diferentes vistas o con diferentes visiones - cada cara que describe de la cosa le da mejor completitud a la descripción, le da mas finura al modelo

---

Buena inversión, y me deja mal parado en una frase que escribí como reproche.

Yo dije «tapando el mismo agujero desde diez ángulos» como diagnóstico de defecto. Y describir una cosa desde varias caras es exactamente lo que el autor hace a propósito, y le sale bien.

## La diferencia no está en la estructura

Los dos casos tienen la misma forma: **un objeto, varias vistas parciales, y el todo aparece recién al componerlas.**

Lo que los separa es otra cosa:

| | El autor con la estancia | Yo con los parches |
|---|---|---|
| ¿Hay un objeto detrás? | sí | sí |
| **¿Está nombrado?** | **sí** — la estancia | **no** — el supuesto falso |
| Las vistas son | caras deliberadas | síntomas dispersos |

> **Varias vistas son una técnica cuando el objeto está nombrado, y un síntoma cuando no.**

Y funciona en los dos sentidos: si te encontrás describiendo algo desde muchos ángulos, o ya sabés qué es —y estás haciendo oficio— o no lo sabés, y esos ángulos te están avisando que hay un objeto sin nombre.

## La analogía que me parece exacta

Es el dibujo técnico. Planta, alzado y corte: **ninguna vista contiene la pieza, y las tres juntas la determinan.** No son redundantes —cada una trae lo que las otras no— pero **se solapan lo suficiente como para poder contrastarse**.

Ese solapamiento es tu redundancia como corrección de error, dicho en geometría. Si la planta y el alzado no cierran en una cota, hay un error, y lo detectás **porque se solapan**.

## Tus tres observaciones son una sola

Mirá cómo se encadenan:

| | Lo que dijiste |
|---|---|
| 1 | la redundancia es corrección de errores |
| 2 | el contraste construye el concepto |
| 3 | cada cara descrita le da más finura al modelo |

Las tres dicen: **un concepto queda determinado por la intersección de descripciones parciales, no por ninguna sola.**

Y eso es el mismo axioma que falla, visto desde el lado del que escribe en vez del que lee:

> **El autor distribuye. El lector tiene que intersecar.**

Nuestro método leía como si el autor no hubiera distribuido nada. Por eso hacía aguas: no era un método malo, era **un método que suponía un autor que no existe**.

## Qué me llevo como operación

«Cara» pasa a ser un objeto de primera clase del análisis, y con eso se unifican tres cosas que teníamos sueltas:

| Pregunta | Qué resuelve |
|---|---|
| ¿Qué fragmentos son caras del mismo objeto? | tu **agrupar** del mensaje anterior, ahora con nombre |
| ¿Cuántas caras tiene este objeto en el texto? | **completitud**: una sola cara = objeto subdeterminado, candidato a hueco |
| ¿Las caras cierran entre sí? | **discordancia**: donde no cierran, ahí está la ambigüedad |

Tres búsquedas que teníamos separadas, una sola operación.

Con una salvedad, que es la contracara de lo de antes: **no toda repetición es una cara.** Una cara **agrega una dimensión**; un eco repite la misma. Si dos formulaciones traen la misma información, no tenés dos vistas: tenés una vista y su copia. La prueba es la del dibujo: *¿esta vista muestra algo que la otra no puede mostrar?*

---

Dejo la convocatoria de la mesa preparada y en pausa — te la ofrecí dos veces y me parece que el hilo que estás tirando rinde más que despachar. La levanto cuando digas, y ahora entra con un elemento más: **nombrar el objeto**, que según tu propia distinción es lo que convierte diez síntomas en una técnica.

---

>

Convocada, con tus seis tesis numeradas y separadas para que las trate una por una.

## Lo que le mandé, y por qué así

Tus aportes van **identificados como tuyos**, y eso no es formalidad: sos el autor del enunciado que se está analizando, así que tu lectura tiene otro peso probatorio que la de un subagente. La mesa tiene que saberlo para no tratarlas como una opinión más.

También le mandé **el contra-argumento del gramático en su mejor versión**, no debilitado. Si lo que dice es cierto —que el método ya produce lo que le falta y solo hay que dejar de tirarlo— entonces tu diagnóstico es correcto pero el remedio es mucho más barato que un replanteo. Quiero que eso lo dirima la mesa y no yo, porque **ya me equivoqué dos veces en la misma discusión**: primero mecanizando tu propuesta y después endosando una re-arquitectura antes de leer el dictamen.

Y tu criterio de aceptación va como condición innegociable:

> Después de la decisión, cada uno de los diez parches tiene que caer como consecuencia del modelo **o desaparecer**. Si siguen haciendo falta como excepciones, la decisión fue la equivocada.

Con la exigencia de verificarlo **parche por parche**, no en bloque — que es donde estas cosas se escapan.

## Dos cuestiones que agregué por tu cuenta

**Nombrar el objeto.** Por tu distinción: diez vistas de lo mismo avisan que hay algo sin nombre. Si la mesa no lo nombra, el aparato va a seguir creciendo por acumulación.

**El volumen.** Son ~5100 líneas en `Conocimientos/` para un método cuyo paso central se ejecuta en cinco segundos por fragmento. Le di permiso explícito para decidir que **documentos enteros sobran**. Un aparato que no se puede sostener no se va a usar, y eso es exactamente la falla que el método existe para corregir — estaríamos haciendo, en la escala del método, lo mismo que le criticamos al enunciado.

## Y queda como norma

Anoté en memoria que **lo que razonamos acá se les pasa a la mesa y a los especialistas sin que lo pidas**, con las tesis separadas y numeradas, y diciendo cuándo el aporte es tuyo. Las mejores piezas del método salieron de esta conversación, no de los subagentes; si se quedan en el chat, ellos siguen trabajando con un marco que vos ya superaste.

---

Mientras delibera, el párrafo te sigue esperando en `03.b`. O si preferís, cuando vuelva el acta la miramos juntos y después seguís — la decisión sobre el modelo puede cambiar las etiquetas que uses de acá en adelante.

---

> mientras la mesa y los especialistas trabjaan con todo lo que le pasaste y van haciendo sus tareas, - sigamos con lo que estabamos en un principio,

> bueno yo habia separado la frase dejando que `el casco y el puersto son instalaciones` y en otra frase que son `viviendas...`

> llegamos a que tanto el caso y el puerto no son mas que instalaciones -

> al unir la frase con que  y son `viviendas`     podríamos englobar todo en instalaciones de igual manera 

> el tema que lo que se afirma despues : `aloja el dueño y personal de la estancia` - como decis sobre el glosario, es parte solo del glosario, enriquese al glosario  dandole un contexto - pero no aporta al modelo informatico en sí - a no ser que mas adelante surjan mas descriptivas sobre capacidades de alojamiento o requerimientos de planificación en mantenimiento de acuerdo a sus capacidades  -ahora , por lo que tenemos en el parrafo no surge mas - bajo esta mirada de definición de alcance preeliminar hasta que no aparezca algun otro parrafo que nos sugiera mas - que etiquetado darias ?

>

Tu razonamiento de alcance es correcto, pero antes del etiquetado te marco dos cosas del análisis.

## 1. «No son más que instalaciones» dice de más

El texto dice que **son** instalaciones. No dice que sean *sólo* eso. En el mismo párrafo el casco además es **central** y está **a cargo de un Administrador**, y el puesto tiene **encargado**.

Si «no son más que instalaciones» entra al registro, borrás esas caras. Es una diferencia chica al escribirla y grande tres semanas después.

## 2. «Viviendas» no es un eco de «instalaciones»

Acá te aplico tu propia prueba. ¿Agrega una dimensión que la otra no puede mostrar?

| | Qué aporta |
|---|---|
| **instalación** | es infraestructura construida |
| **vivienda** | **se habita** |

Sí agrega. Entonces **es una cara, no un eco**, y englobarlas pierde justamente la dimensión que la oración desarrolla después.

Y mirá lo que aparece cuando las separás: *«viviendas»* y *«donde se aloja el dueño y personal»* **son la misma cara dicha dos veces**. El relativo no agrega una dimensión nueva: **desarrolla la que «vivienda» ya había anunciado**. Es la triangulación en vivo — el autor dice *habitable* y después dice *quién la habita*.

Así que hay dos caras, no una: **construida** y **habitada**. Y la segunda viene reforzada.

## El etiquetado

El fragmento contesta *¿para qué es una instalación/vivienda?* Eso es **la definición del término**, no un dato de casco ni de puesto. Su destino es la Tabla 1, columna *Designa*.

```
03.a.5 [ "y viviendas"                                    T  ]  cara 2: habitable
03.a.6 [ "donde se aloja el dueño y personal"             T  ]  desarrolla la cara 2
                                                                 → Designa de `vivienda`,
                                                                   no abre fila nueva
```

Va a `T` porque su destino es el glosario. No es `A`: un atributo es un dato con valor, y esto no es un campo, es qué significa la palabra.

## Y acá hay un hueco del sistema de etiquetas, que es tuyo

Vos decidiste algo importante: *«no aporta al modelo informático, a no ser que más adelante surjan capacidades de alojamiento o requerimientos de mantenimiento»*.

**Eso es una decisión de alcance, y las decisiones de alcance se escriben.** Si no queda registrada, dentro de tres semanas nadie sabe si se analizó y se apartó, o si nunca se vio.

Pero fijate que no tenés dónde ponerla:

| | Qué es |
|---|---|
| **`E`** | alcance que **el autor declara** fuera — «se contempla en un futuro análisis» |
| **lo tuyo** | alcance que **el analista difiere**, revisable, con la condición de reapertura declarada |

Son cosas distintas. La primera es una decisión del dueño del problema; la segunda es nuestra y **se revisa sola** si aparece el dato que la dispara. El catálogo tiene la primera y no tiene la segunda.

Es un hallazgo del banco de pruebas, y va a la mesa con el resto. Mientras tanto, anotalo así, que es lo que importa:

```
DECISIÓN DE ALCANCE (analista, revisable)
  03.a.6  «donde se aloja el dueño y personal»
  No se modela: enriquece el glosario, no genera dato ni regla.
  SE REABRE SI: aparece capacidad de alojamiento, ocupación,
                o mantenimiento por capacidad.
```

Lo que la vuelve seria es la línea **«se reabre si»**: sin eso es una omisión con buena prosa; con eso es una decisión con disparador.

---
> claro, porque seguido a esa oración, sigue esta `La administración del casco y de los puestos se contempla en un futuro análisis del sistema. ` que es la cuarta que tengo que analizar en fragmentos,  y la descompsisión en fragmentos como seria?


---


Ahí se cierra la técnica que venías viendo: **la oración 3 agrupa y la 4 despacha al grupo entero de una sola vez.** Por eso el autor necesitaba la 3.

## La fragmentación

```
04.a  "La administración del casco y de los puestos se contempla
       en un futuro análisis del sistema."

04.a.1 [ "La administración"                      V  ]  nominalización → el acto «administrar»
04.a.2 [ "del casco"                              T° ]  segunda mención
04.a.3 [ "de los puestos"                         T° ]  segunda mención
04.a.4 [ "se contempla"                          ⟦?⟧ ]  agente oculto: ¿quién lo contempla?
04.a.5 [ "en un futuro análisis del sistema"      E  ]  alcance: el acto queda FUERA
```

## Por qué el «y» no corta

Coordina **dos complementos de un mismo sustantivo** —«la administración *del casco* y *de los puestos*»— bajo un solo predicado. Una predicación, un corte.

Pero ojo: **la declaración se distribuye sobre dos objetos.** No cortás el fragmento, pero cuando lo lleves a la tabla registrás **dos filas fuera de alcance**, no una. Si mañana entra la administración del casco y la del puesto sigue afuera, lo vas a agradecer.

## La nominalización, que acá es la pieza principal

`administración` es un sustantivo que esconde el verbo **administrar**. Si lo leés como sustantivo se te va al glosario y **perdés el acto** — que es justamente lo que la oración está declarando fuera.

Y el registro correcto no es omitirlo:

| Acto | Quién lo hace | Estado |
|---|---|---|
| administrar el casco | — | **fuera de alcance**, declarado por el autor |
| administrar el puesto | — | **fuera de alcance**, declarado por el autor |

Registrar el acto ausente **con su motivo** es mejor que no registrarlo. Si no está, la falta parece un descuido tuyo; si está, es una decisión del autor con su cita.

Y de paso te habilita la búsqueda de silencios: con los actos fuera de alcance anotados, el que **no** está declarado ni adentro ni afuera salta por contraste. En este enunciado hay uno así esperándote —la baja de actividades— pero eso es más adelante.

## Una trampa que quiero que veas

`04.a.5` parece un `M`. Contesta «¿cuándo?»: en un futuro análisis.

**No es `M`.** Fijate el plano:

| Fragmento | De qué tiempo habla |
|---|---|
| «en su momento de conformación» (01.b) | **del negocio** — un acto de la estancia |
| «en un futuro análisis del sistema» (04.a) | **del proyecto** — un acto de quien construye el software |

Si etiquetás `M`, el «análisis del sistema» se te cuela a la tabla de actos como si fuera un acto del dominio. Y no lo es: la estancia no hace análisis de sistemas.

Por eso va `E` y no `M`. La regla corta: **`M` es para el tiempo del negocio. El tiempo del proyecto no entra en las tablas del dominio.**

## Y un aviso para cuando sigas

`administración` te va a volver a aparecer, y con otro sentido. En el párrafo siguiente del enunciado está «la administración de los campos **consiste** en el alta» —acto— y también «único dentro del **contexto administrativo** de la estancia» —ámbito, no acto—.

Mismo término, dos sentidos. Es I-01, y va a ser tu segunda inconsistencia de verdad.

---

Buena sospecha, y el método para resolverla es **el mismo test que salvó al «y»**.

## ¿Hay un verbo de cada lado?

| | Izquierda | Derecha | ¿Corta? |
|---|---|---|---|
| **«y»** de 02.a | `se establece un casco…` | `varios puestos rurales` ← **sin verbo** | **No** |
| **«de los cuales»** | `…varios puestos rurales` | `se puede conocer el nombre…` ← **verbo** | **Sí** |

Del lado derecho del relativo hay predicación: *se puede conocer*. Por eso corta, y por la misma razón que el «y» no cortaba.

Tu sospecha estaba metodológicamente bien —aplicaste la desconfianza al caso siguiente, que es lo correcto— pero el test da lo contrario, y da lo contrario por un motivo que se puede señalar con el dedo.

## `02` completa

```
02.a  "En el mismo acto se establece un casco central a cargo de un
       Administrador y varios puestos rurales"

  02.a.1 [ "En el mismo acto"                M° ]  ref 01.b.5
  02.a.2 [ "se establece"                    V  ]  efecto de la conformación · agente ⟦?⟧
  02.a.3 [ "un casco central"                T+C]  C: exactamente uno
  02.a.4 [ "central"                         A  ]  restringe dentro de ⟨instalaciones⟩
  02.a.5 [ "a cargo de un Administrador"     R(estructura) ]
  02.a.6 [ "Administrador"                   T  ]
  02.a.7 [ "varios puestos rurales"          T+C]  C(dudoso): ¿cuántos es «varios»?
  02.a.8 [ "rurales"                         A  ]  el otro polo del contraste con «central»

02.b  "de los cuales cada uno se puede conocer el nombre del encargado"
       antecedente: ⟦los puestos rurales⟧

  02.b.1 [ "cada uno"                        C  ]  distributivo: vale para TODOS
  02.b.2 [ "se puede conocer"                V  ]  impersonal → «el sistema registra» · agente ⟦?⟧
  02.b.3 [ "el nombre del encargado"         A  ]  atributo del encargado
  02.b.4 [ "encargado"                       T  ]
  02.b.5 [ (implícito) cada puesto tiene un encargado    R(estructura) ]
```

## Y ahora el hallazgo, que salta al ponerlos uno al lado del otro

| | Qué dice el texto |
|---|---|
| casco | «a cargo de **un Administrador**» — la relación, y nada más |
| puesto | «se puede conocer **el nombre** del encargado» — la relación **y el dato** |

**El autor no dice lo mismo de los dos.** Del casco sabemos que tiene Administrador; **no dice que se registre su nombre**. Del puesto dice las dos cosas.

Eso es una **asimetría de detalle**, y es una de las señales que el gramático listó. Abre una pregunta legítima:

> ¿Del Administrador del casco también se registra el nombre, o solo del encargado del puesto?

No la contestes vos. El paralelismo sugiere que sí, pero *sugerir* no es *decir*, y acá es exactamente donde se cuela la sobreinterpretación. Va al cuestionario.

## Dos cosas más

**`cada uno` es cardinalidad, y es la buena.** «Varios» es vago —no sabés cuántos—; «cada uno» es preciso —sabés que vale para **todos**—. Son dos preguntas distintas y el autor contestó la segunda. Tu `C(dudoso)` sobre «varios» sigue en pie.

**`rurales` es el otro polo.** Cuando etiquetes `02.a.8`, acordate de que no es un adjetivo suelto: es la mitad del contraste que hace que «central» signifique algo. Los dos juntos son los que producen tu «la instalación central es el casco».

----


Ahora sí, con el párrafo completo.

# Paso 3 — Las tres tablas

## Tabla 1 — Términos

| Término | Designa | Unidad | Ámbito | De dónde |
|---|---|---|---|---|
| estancia agropecuaria | el establecimiento | — | el sistema | 01.a |
| campo | **⟦?⟧ no se define en este párrafo** | — | estancia | 01.b.3 |
| instalación | construcción de la estancia | — | estancia | 03.a.3 |
| vivienda | instalación **que se habita**: aloja al dueño y al personal | — | estancia | 03.a.5 + 03.a.6 |
| casco | la instalación **central** | — | estancia | 02.a.3 |
| puesto | instalación **rural** | — | estancia | 02.a.7 |
| Administrador | quien está a cargo del casco | — | casco | 02.a.6 |
| encargado | quien está a cargo de un puesto | — | puesto | 02.b.4 |
| dueño | **⟦?⟧ nunca se define** | — | ⟦?⟧ | 03.a.6 |
| personal | **⟦?⟧ nunca se define** | — | ⟦?⟧ | 03.a.6 |

**Atributos**

| Atributo | Designa | Unidad | Ámbito | De dónde |
|---|---|---|---|---|
| nombre | cómo se llama la estancia | — | estancia | 01.a.2 |
| central | ocupa el centro; polo de `rural` | — | casco | 02.a.4 |
| rural | no central; polo de `central` | — | puesto | 02.a.8 |
| nombre del encargado | se **registra** | — | encargado | 02.b.3 |

## Tabla 2 — Actos

| Acto | Quién lo hace | Qué recibe | Qué cambia | De dónde |
|---|---|---|---|---|
| **conformación** | ⟦?⟧ | ⟦?⟧ | existe la estancia · con nombre · con ≥1 campo · con **un** casco a cargo de un Administrador · con **varios** puestos, cada uno con encargado | 01.b.5 · 01.b.4 · 02.a.1 · 02.a.2 |
| administrar el casco | — | — | **fuera de alcance** | 04.a |
| administrar el puesto | — | — | **fuera de alcance** | 04.a |

> `se puede conocer` (02.b.2) **no abre fila**. No es algo que alguien hace en un momento: es la afirmación de que el dato está disponible. Es atributo, no acto.

## Tabla 3 — Reglas

| Id | Letra | Intención | Especie | Momento | Quién verifica | De dónde |
|---|---|---|---|---|---|---|
| RN-01 | «está compuesta por al menos un campo» | la estancia se conforma con ≥1 campo | estructura | Precondition de `conformación` | la conformación | 01.b.2+01.b.4 |
| RN-02 | «se establece un casco central» | la conformación establece **un** casco | estructura | Precondition de `conformación` | la conformación | 02.a.2-3+02.a.1 |
| RN-03 | «a cargo de un Administrador» | todo casco tiene un Administrador | estructura | ⟦?⟧ | **nadie** | 02.a.5 |
| RN-04 | «y varios puestos rurales» | la conformación establece varios puestos | estructura | Precondition de `conformación` | la conformación | 02.a.7+02.a.1 |
| RN-05 | ⟦implícita⟧ | **cada** puesto tiene un encargado | estructura | ⟦?⟧ | **nadie** | 02.b.1+02.b.5 |
| RN-06 | «se puede conocer el nombre del encargado» | de cada encargado se registra el nombre | estructura | ⟦?⟧ | **nadie** | 02.b.3 |
| RN-07 | «son instalaciones y viviendas» | casco y puesto son instalaciones habitadas | estructura | — | — | 03.a.4 |
| AL-01 | «se contempla en un futuro análisis del sistema» | administrar casco y puestos queda afuera | alcance | — | — | 04.a |

---

# Lo que cayó solo

**1. Siete reglas, y ninguna prohíbe nada.**

Todas `estructura`. Este párrafo **define de qué está hecha una estancia**; no rechaza ni un intento. Las restricciones empiezan en el párrafo siguiente, con los identificadores únicos y las superficies.

**2. Tres reglas sin nadie que las verifique — y no es casualidad.**

RN-03, RN-05 y RN-06 son **exactamente las tres que hablan de roles**: el Administrador, el encargado, y el nombre del encargado.

Y mirá qué dice la oración 4: la administración del casco y de los puestos **queda fuera de alcance**.

> Las tres reglas sin verificador son las tres cuya verificación viviría en lo que el autor declaró afuera. **El agujero no es un descuido: es la consecuencia de AL-01.**

Eso no lo dice ninguna oración. Sale de cruzar la 2 con la 4 — es otra afirmación distribuida, y ésta explica los huecos en vez de agregar contenido.

**3. Y acá el hallazgo fuerte, que necesitó tres lugares**

La oración 2 dice que del encargado **se puede conocer el nombre**. Pero el caso de uso 1 dice que el casco y el puesto se crean **«sin designar los nombres de los encargados de ambas entidades»**. Y administrarlos está fuera de alcance.

| Momento | ¿Se puede asignar el nombre? |
|---|---|
| conformación | **no** — el CU-1 lo excluye expresamente |
| administración | **no existe** — AL-01 |

> **RN-06 declara un dato que ningún acto del sistema puede llegar a cargar nunca.**

No es contradicción: es **incompletitud**. Falta el acto. Y por la escalera, el diagnóstico correcto es E2, no E3 — no hay que descartar RN-06, hay que nombrar el acto que falta.

**4. Una asimetría que queda abierta.**

Del encargado se registra el nombre. Del Administrador, el texto **no lo dice**. Puede ser deliberado o puede ser que el autor lo dio por obvio.

**5. Ninguna unidad, en catorce filas.**

Este párrafo no tiene magnitudes. Nada que sumar, nada que comparar.

---

# El cuestionario

| # | Pregunta | Compuerta |
|---|---|---|
| 1 | ¿Qué es un campo? | ❌ **muerta** — el párrafo siguiente lo define |
| 2 | ¿Quién conforma la estancia? | ✅ |
| 3 | ¿`dueño` y `personal` son del modelo o son prosa? | ✅ |
| 4 | ¿`Administrador` y `encargado` son el mismo rol con dos nombres? | ✅ **I-02** |
| 5 | ¿Cuántos son «varios» puestos? | ✅ `C(dudoso)` |
| 6 | **¿Cuándo se carga el nombre del encargado, si la conformación lo excluye y la administración está afuera?** | ✅ **la más importante — pide un acto que no existe** |
| 7 | ¿Del Administrador también se registra el nombre? | ✅ asimetría |

**Seis de siete sobreviven.** Y una decisión de alcance registrada: `03.a.6`, con su disparador de reapertura.

---

Una cosa más, y es sobre mí. El hallazgo 3 apareció **cruzando con el documento original**, no con mi transcripción: yo había condensado el CU-1 en una fila y perdí la cláusula «sin designar los nombres». Ya la repuse, con la nota de por qué.

Es la prueba de uno de los criterios de aceptación que habíamos escrito sin terminar de creerle: **anclar a la fuente primaria, nunca a una tabla intermedia.**

---

# Reglas

No. `estancia una casco` no es una regla: es un apunte telegráfico. Una regla es **una oración de la que se puede decir «esto es falso»**.

## La forma

> **‹quién› + ‹qué se exige›**, en palabras del negocio, con sujeto explícito y en presente.

| ❌ | ✅ |
|---|---|
| `estancia una casco` | **La estancia tiene exactamente un casco.** |
| `campo al menos uno` | **La estancia, al conformarse, tiene al menos un campo.** |

La prueba para chequear lo que escribiste: **poné «es falso que…» adelante.** Si la frase resultante se entiende, es una regla. Si no se entiende, todavía no escribiste una.

- *«Es falso que la estancia tenga exactamente un casco»* → se entiende. ✅
- *«Es falso que estancia una casco»* → no se entiende. ❌

## Dónde va cada cosa

La regla no es un texto suelto: **es una fila**. Y la oración que escribís va en la columna `Intención`, no en `Letra`:

| Columna | Qué lleva |
|---|---|
| `Letra` | la **cita textual**, tal cual, entre comillas. Puede ser un fragmento sin sujeto |
| `Intención` | **tu oración completa**, con sujeto, que se sostiene sola |

Por eso son dos columnas: la letra puede estar rota y la intención no.

## Tus tres primeras, escritas

```
RN-01
  Letra:     «está compuesta por al menos un campo»
  Intención: La estancia, al conformarse, tiene al menos un campo.

RN-02
  Letra:     «se establece un casco central»
  Intención: La estancia tiene exactamente un casco.

RN-03
  Letra:     «a cargo de un Administrador»
  Intención: Todo casco tiene un Administrador a cargo.
```

Mirá las tres `Letra`: ninguna tiene sujeto. «Está compuesta» — ¿quién? «Se establece» — ¿qué? «A cargo de» — ¿de qué?

Y las tres `Intención` sí lo tienen. **Ese es el trabajo del paso 4**: no copiar el fragmento, sino escribir la oración que el fragmento quería decir, completa y verificable.

## Y no inventes lo que no está

`RN-02` dice «exactamente un casco» porque el texto dice *«un casco»* en singular y nunca menciona otro. Si el texto hubiera dicho «al menos un casco», la intención sería otra.

**La intención completa la gramática, no el negocio.** Repone el sujeto que la elipsis se llevó; no agrega condiciones que el autor no puso.