

# Actividad 1 - Deduciendo Reglas de negocio - RN

  > **Objetivo**
  > Leer un documento de requerimientos escrito en prosa y extraer de él las reglas de negocio,
  > enunciadas de forma que se puedan verificar.

  **Qué entra y qué sale**

  | | |
  | --- | --- |
  | **Entra** | un párrafo de un documento de requerimientos, en prosa |
  | **Sale** | tres tablas —términos, actos, reglas— y una lista de preguntas para el autor |

  **Lo que esta actividad NO hace todavía**: no decide clases, no decide capas, no escribe código.
  Eso es diseño y viene en las actividades 3 y 4. Acá sólo se describe el problema.

> ### ⟨M⟩ Qué agregó la mesa, y cómo distinguirlo de lo tuyo
>
> **Este cuaderno es del autor.** Lo que lleva la marca **⟨M⟩** lo escribió la *mesa de cierre de la
> actividad 1* (2026-10-03; acta en
> `Lab-Documentos.Documentacion/PROMPTs/Guides/06-Crear-Dot-NET-Arquitectura-Guide/OUTPUTs/Mesa/07-Acta-Cierre-Actividad-1.md`).
> Todo lo que **no** la lleva es trabajo del autor y **no se reescribió**.
>
> | Marca | Qué significa |
> | --- | --- |
> | **⟨M⟩** | bloque, fila, ficha o celda **agregada** por la mesa |
> | **⟨M corrige: …⟩** | celda del autor que la mesa **modificó**, con el texto anterior dicho ahí mismo |
>
> Los rótulos son los de `Knowledge-Deduccion-De-Reglas-De-Negocio.md` §3.2 y se respetan al pie de
> la letra: **`Letra` sólo admite cita textual**; **`Intención`** admite reconstrucción, siempre con
> el razonamiento y las anclas a la vista; **`Propuesta`** lleva **frase de residuo** y nunca es lo
> que la fuente dice.
>
> **Lo que la mesa deliberadamente NO hizo:** no contestó ninguna de las seis preguntas del cierre
> —son del autor—, no corrigió el enunciado, no avanzó al párrafo II, y no tocó los pasos 1 y 2
> salvo las dos etiquetas que la tarea `T-1` obligó a cambiar.

---

## Introducción a reglas de negocios

### Qué es una regla de negocio

  Hay dos definiciones y se complementan: una dice **de dónde viene**, la otra **qué clase de cosa es**.

  > **De dónde viene.** Condición o procedimiento que el negocio impone **por sus propias razones** y
  > que valdría **aunque no hubiera software** que lo ejecute.

  > **Qué clase de cosa es.** Una afirmación que **define o restringe** algún aspecto del negocio: o
  > bien establece su estructura, o bien controla su comportamiento.

  La segunda ya trae adentro la clasificación que se usa más abajo: *define* → derivación,
  *restringe* → restricción, *establece estructura* → estructura.

### Cómo se reconoce: la prueba del papel

  La operación concreta, y es una sola:

  > **Si el negocio se llevara en un cuaderno, ¿esto seguiría valiendo?**

  - Si **sí** → es regla de negocio.
  - Si su razón es **la base de datos, la pantalla o el protocolo** → no lo es.

  **Dos que sí lo son** — sobreviven al cuaderno:

  | Condición | Por qué pasa la prueba |
  | --- | --- |
  | La superficie de un campo no puede ser cero | Un capataz tampoco anotaría a mano un campo de cero hectáreas: no existe |
  | El identificador de un campo no se repite dentro de la estancia | Dos campos con el mismo nombre en el cuaderno ya serían un problema |

  **Dos que no lo son** — son reales y hay que cumplirlas, pero las impone la tecnología:

  | Condición | Por qué no pasa la prueba |
  | --- | --- |
  | Los campos se muestran de a veinte por página | En papel no hay paginado. La impone **la pantalla** |
  | La superficie llega como número y no como texto | En el cuaderno se escribe «50 ha» y se entiende. La impone **el transporte** |

  > **Lo que decide no es el texto de la condición: es su razón.**
  >
  > La misma frase cae de los dos lados según por qué existe. *«El nombre admite hasta 40
  > caracteres»* es **regla de negocio** si el motivo es que eso es lo que entra en el cartel de la
  > tranquera, y es **restricción técnica** si el motivo es el ancho de la columna.

  Y la que no pasa la prueba **no se descarta: se reubica.** Hay que cumplirla igual; lo que cambia es
  dónde vive, y eso se decide en las actividades 3 y 4.

### Las cuatro preguntas, en orden

  Por cada candidata, y en este orden:

  | # | Pregunta | Qué decide |
  | --- | --- | --- |
  | 1 | ¿Seguiría valiendo **en papel**? | si es del negocio o de la tecnología |
  | 2 | ¿**Describe, prohíbe o calcula**? | la *especie* |
  | 3 | ¿Vale **siempre**, o **antes de un acto**? | el *momento* |
  | 4 | ¿**Quién tiene los datos** para decidirla? | el *dueño* |

  La pregunta 3 tiene una prueba que no falla: **¿vale en el instante en que el objeto nace?** Si el
  objeto nace sin cumplirla, no es invariante.

  La pregunta 4 admite **«ninguno»** como respuesta, y escribirlo es un hallazgo, no una falla: esas
  son las reglas que no caben en ningún objeto y terminan fuera del modelo.

> ⚠ **Ojo con una confusión frecuente.** «Las reglas de negocio viven en las entidades» **no es la
> definición**: es una afirmación sobre *dónde vive una parte de ellas*, y además no siempre es
> cierta. Dónde vive cada regla se decide en la actividad 3, con la pregunta 4. Acá no se decide.

  **La que sí vive en la entidad**

  > «La superficie de una parcela no puede superar la superficie sin parcelar del campo.»

  `Campo` tiene todo lo que hace falta: su superficie total y sus parcelas. **Se decide mirándose a sí
  mismo**, y por eso la regla vive ahí.

  **La que no vive en ninguna**

  > «Una parcela no puede estar asignada a dos actividades de la estancia.»

  Para decidirla hay que ver **todas las actividades**. `Parcela` no las conoce; `Actividad` conoce
  las suyas pero no las de las demás. **Ningún objeto tiene los datos**, y la regla es tan del negocio
  como la anterior.

  Y hay evidencia de que esto pasa de verdad, no en teoría: en la versión de escritorio ya resuelta de
  esta misma estancia, esa regla **terminó en el formulario** —`FormPrincipal.cs`, línea 337—, que es
  el único lugar que ve todo. No fue descuido: **no había dónde ponerla.**

  > Por eso la pregunta 4 admite «ninguno». Si se arranca creyendo que toda regla vive en una entidad,
  > a ésta se le inventa un dueño, y el hallazgo se pierde.

### La fórmula

  La forma general de una regla de negocio:

  ```
  [momento] · sujeto · verbo · exigencia
  ```

  Ejemplo:
  > **«Al conformarse, la estancia tiene al menos un campo.»**

  **Sujeto explícito, presente del indicativo, vocabulario del negocio.** El momento entra en la oración cuando la regla está atada a un acto; si vale siempre, no se escribe.

  **Una forma por especie**

  Las cuatro especies no se enuncian igual, y ahí es donde se equivoca todo el mundo.

  | Especie | Plantilla | Ejemplo |
  |---|---|---|
  | **Restricción** | *No se puede ‹acto› si ‹condición›* · o *‹sujeto› no puede ‹predicado›* | «No se puede cerrar una actividad sin parcelas asignadas.» |
  | **Derivación** | *‹resultado› se calcula como ‹fórmula›* | «La producción de una actividad agrícola se calcula como toneladas × precio de la tonelada.» |
  | **Estructura** | *‹todo› tiene ‹cardinalidad› ‹parte›* · o *‹A› es un ‹B›* | «Todo casco tiene un Administrador a cargo.» |
  | **Alcance** | *‹acto› queda fuera del alcance* | «Administrar el casco queda fuera del alcance.» |

  Si no sabés cuál usar, la pregunta es una: **¿podés contar la escena del rechazo?** Si sí, restricción. Si no porque calcula, derivación. Si no porque rechaza una composición, estructura.

  - Las dos pruebas, antes de darla por escrita

    **1. «Es falso que…»** — ponelo adelante. Si la frase se entiende, es una regla.

    - *«Es falso que la estancia tenga exactamente un casco»* → se entiende ✅
    - *«Es falso que estancia una casco»* → no se entiende ❌

    **2. «¿Cómo la violo?»** — describí el intento concreto que el sistema rechaza. Si no podés describirlo, **no es restricción**: es otra especie, o no es regla.

  - Lo que no puede aparecer

    | ❌ | Por qué |
    |---|---|
    | **Telegráfico** — «campo al menos uno» | no tiene valor de verdad |
    | **Vocabulario de implementación** — «el campo `Identificador` no puede ser `null`» | eso es diseño; la regla dice «todo campo tiene identificador» |
    | **Dos reglas en una** — «el identificador es único y la superficie no es cero» | son dos, se parten en el «y» |
    | **Palabras no verificables** — «coherente», «adecuado», «razonable» | no se puede escribir la prueba |
    | **Pasiva sin agente** — «se verifica que…» | ¿quién verifica? Si no se sabe, va `⟦?⟧`, no se tapa |
    | **Condiciones que el autor no puso** | la intención completa la gramática, no el negocio |

  - Dónde vive

    El enunciado va en la columna **`Intención`**. La `Letra` lleva la cita textual, que puede estar rota. Por eso son dos columnas.

    ```
    RN-nn
      Letra:            «‹textual, entre comillas›»
      Intención:        ‹la oración completa, con sujeto›
      Especie:          restricción / derivación / estructura / alcance
      Momento:          Invariant / Precondition de ‹acto› / completitud / ninguno
      Quién verifica:   ‹acto›  ·  o  nadie
      Dueño:            ‹objeto›  ·  o  ninguno en el dominio
      De dónde:         ‹ancla›
    ```

---

## Extraer del parrafo siguiente las reglas de negocio

  ```
  Una estancia agropecuaria tiene un nombre y está compuesta por al menos un campo en su momento de conformación. En el mismo acto se establece un casco central a cargo de un Administrador y varios puestos rurales, de los cuales cada uno se puede conocer el nombre del encargado. Tanto el casco como el puesto son instalaciones y viviendas donde se aloja el dueño y personal de la estancia. La administración del casco y de los puestos se contempla en un futuro análisis del sistema. 
  ```

### Paso 0. Numerá las oraciones

  (01) Una estancia agropecuaria tiene un nombre y está compuesta por al menos un campo en su momento de conformación. 

  (02) En el mismo acto se establece un casco central a cargo de un Administrador y varios puestos rurales, de los cuales cada uno se puede conocer el nombre del encargado. 

  (03) Tanto el casco como el puesto son instalaciones y viviendas donde se aloja el dueño y personal de la estancia. 

  (04) La administración del casco y de los puestos se contempla en un futuro análisis del sistema. 

### Paso 1. Corta cada oración en afirmaciones

> Se corta en cada <<y>>, en cada coma, y en cada <<de los cuales/donde tanto... como>>

> Es decir que podría separarse en dos o mas proposiones que podrian constituir oraciones independientes y que pueden afirmarse en cada una si es verdadera o falsa

> ### ⟨M⟩ Aviso: el enunciado de arriba produce falsos cortes — y el corte que hiciste no lo siguió
>
> **La mesa no reescribe este paso**: el autor reservó su revisión. Pero lo deja marcado, porque
> todo lo que viene después cuelga de los números que este paso produjo, y un error de corte no se
> arregla más abajo.
>
> **El criterio correcto** está en `02-Criterio-De-Segmentacion.md` §1.3: *se corta donde hay una
> **predicación propia** capaz de sostener por sí sola un juicio de cumplimiento*. Y la prueba
> operativa es la del punto (§1.4, `P-1`): reemplazá la unión por un punto y leé los dos pedazos en
> voz alta — **¿hay un verbo de cada lado?** Si uno de los dos no se sostiene aislado, no se corta:
> se registra como **calificador** de la predicación que modifica.
>
> **Las tres veces que la regla escrita arriba habría cortado mal en este mismo párrafo:**
>
> | Lugar | Qué manda la regla escrita | Qué corresponde | Por qué |
> | --- | --- | --- | --- |
> | el `y` de **(02)**: «un casco central **y** varios puestos rurales» | cortar | **no corta** | coordina dos objetos del **mismo** verbo `se establece`. Un solo predicado → falso corte `FC-1` (`02` §6.1) |
> | el **«tanto… como»** de **(03)** | cortar | **no corta** | sujeto múltiple de un solo predicado `son` → falso corte `FC-10` |
> | la **coma** de **(02)**, antes de «de los cuales» | cortar | **sí corta** | pero no por ser coma: porque del otro lado hay predicado propio, `se puede conocer`, y aislado se sostiene restituyendo el antecedente ⟦los puestos rurales⟧ |
>
> Dos de las tres son falsos cortes y la tercera es un acierto por la razón equivocada. **El `y` de
> (01) sí corta**, también por predicado —`tiene` / `está compuesta`— y no por ser `y`. La
> diferencia entre el `y` de (01) y el de (02) es exactamente la que la regla escrita no puede ver.
>
> **Y lo producido está bien.** Los cinco fragmentos de abajo son los que el criterio por
> predicación da: **5 afirmaciones contra 7 verbos en forma personal** —`tiene`, `está compuesta`,
> `se establece`, `se puede conocer`, `son`, `se aloja`, `se contempla`—, dentro de la cota de
> atomización (`02` `P11`, condición 5). **El defecto está en la regla enunciada, no en su
> resultado**, y por eso la mesa avisa en vez de rehacer: el aviso es lo que hace que el error
> enseñe; sin él, enseña el error.

- En **(01)**:

  01.a [ Una estancia agropecuaria tiene un nombre]  

  `y`

  01.b [ está compuesta por al menos un campo en su momento de conformación ]  

- En **(02)**: 

  02.a  [ En el mismo acto se establece un casco central a cargo de un Administrador y varios puestos rurales]
  
  `, de los cuales`

  02.b [ cada uno se puede conocer el nombre del encargado ] 

- En **(03)**:

  03.a [ Tanto el casco como el puesto son instalaciones y viviendas donde se aloja el dueño y personal de la estancia.  ]

- En **(04)**:

  04.a [ La administración del casco y de los puestos se contempla en un futuro análisis del sistema. ]


### Paso 2. Etiquetar cada fragmento con una letra, sin pensar.

  | Letra | Qué es |
  | --- | --- |
  | T | término - nombre una cosa del negocio |
  | A | atributo - algo que una cosa tiene |
  | V | acto - algo que alguien hace |
  | C | cardinalidad - cuántos |
  | M | momento - <<en el acto de>>, <<luego>>, <<el cierre>> |
  | R | candidata a regla |
  | E |  alcance que **el autor declara** fuera — «se contempla en un futuro análisis» - **lo tuyo** - alcance que **el analista difiere**, revisable, con la condición de reapertura declarada |
  | ? | no sé |

  > Regla: El acto se registra una sola vez, en el fragmento donde se lo nombra. Las menciones posteriores son calificadores de momento con referencia.

- En **01.a**

  > Una estancia agropecuaria tiene un nombre  

  ```
  01.a.1 [ "estancia"          T ]
  01.a.2 [ "tiene un nombre"   A ]
  ```

- En **01.b**   

  > está compuesta por al menos un campo en su momento de conformación 

  ```
  01.b.1 [ "estancia"          T ] <- sujeto tácito o implicito
  01.b.2 [ "compuesta por al menos un campo"   R+C ]
  01.b.3 [ "campo"   T ]
  01.b.4 [ "en su momento de conformación"  M ]
  01.b.5 [ "conformación"  V ]
  ```

> Notas:
> Sujeto tácito, o sujeto disinencial o sujeto nulo. El español es una lengua de sujeto nulo: el verbo ya carga la persona y el número, así que repetir el sujeto suena redundante.

- En **02.a**

  `En el mismo acto se establece un casco central a cargo de un Administrador y varios puestos rurales`

  ```
  02.a.1 [ "En el mismo acto"                M° ]  ref 01.b.5
  02.a.2 [ "se establece"                    V  ]  efecto de la conformación · agente ⟦?⟧
  02.a.3 [ "un casco central"                T+C]  C: exactamente uno
  02.a.4 [ "central"                       R/A  ]  Q1 · ⟨C-restr⟩ sobre ⟨instalaciones⟩ ⟨M corrige: A⟩
  02.a.5 [ "a cargo de un Administrador"     R(estructura) ]
  02.a.6 [ "Administrador"                   T  ]
  02.a.7 [ "varios puestos rurales"          T+C]  C(dudoso): ¿cuántos es «varios»?
  02.a.8 [ "rurales"                      R/A  ]  Q1 · el otro polo del eje ⟨ubicación⟩ ⟨M corrige: A⟩
  ```

  > **⟨M⟩ Las dos etiquetas de arriba son lo único que la mesa tocó del paso 2**, y la tarea lo
  > obligó: con `A` sola, `central` y `rural` iban a la tabla de atributos y la afirmación que
  > producen no llegaba a ninguna tabla de reglas. El porqué está en el bloque del paso 3.

- En **02.b**

  `de los cuales cada uno se puede conocer el nombre del encargado"
       antecedente: ⟦los puestos rurales⟧`

  ```
  02.b.1 [ "cada uno"                        C  ]  distributivo: vale para TODOS
  02.b.2 [ "se puede conocer"                V  ]  impersonal → «el sistema registra» · agente ⟦?⟧
  02.b.3 [ "el nombre del encargado"         A  ]  atributo del encargado
  02.b.4 [ "encargado"                       T  ]
  02.b.5 [ (implícito) cada puesto tiene un encargado    R(estructura) ]
  ```

- En **03.a**

  > Tanto el casco como el puesto son instalaciones y viviendas donde se aloja el dueño y personal de la estancia. 

  ```
  03.a.1 [ "casco"                 T  ]    
  03.a.2 [ "puesto"                T  ]    
  03.a.3 [ "instalaciones"         T  ]
  03.a.4 [ "son instalaciones y viviendas"     R(estructura)  ]
  03.a.5 [ "y viviendas"                         T  ]  cara 2: habitable
  03.a.6 [ "donde se aloja el dueño y personal"             T  ]  desarrolla la cara 2
                                                                 → Designa de `vivienda`,
                                                                   no abre fila nueva

  ```

  > Nota: Que dos cosas compartan una categoría en el lenguaje del negocio no obliga a que compartan una clase en el modelo.
  > Una jerarquía se justifica cuando cada hoja responde distinto la misma pregunta, y el negocio le puso nombre a cada una.
  

- En **04.a**

  > La administración del casco y de los puestos se contempla en un futuro análisis del sistema. 

  ```
  04.a.1 [ "La administración"                      V  ]  nominalización → el acto «administrar»
  04.a.2 [ "del casco"                              T° ]  segunda mención
  04.a.3 [ "de los puestos"                         T° ]  segunda mención
  04.a.4 [ "se contempla"                          ⟦?⟧ ]  agente oculto: ¿quién lo contempla?
  04.a.5 [ "en un futuro análisis del sistema"      E  ]  alcance: el acto queda FUERA
  ```

### ⟨M⟩ Paso 2-bis. Las hojas del referente

  **Qué es y por qué aparece recién ahora.** El instrumento es nuevo: sale del replanteo del método
  (acta `06-Acta-Replanteo-Del-Modelo.md` §4). La idea entera cabe en una oración:

  > **El sentido no se acumula en el fragmento: se acumula en el referente** —la cosa del negocio de
  > la que el párrafo habla—. Ningún fragmento contiene la cosa entera, porque el autor la describe
  > **por caras**: cada oración aporta una dimensión distinta de la misma cosa.

  **Cómo se usa.** Se abre **una hoja por cada sustantivo del dominio** que el párrafo nombra, y cada
  fragmento **deposita** lo que dice en un casillero de la hoja, con su número al lado. Los
  casilleros son siete: `género` · `eje` · `miembros` · `momento` · `agente` · `alcance` ·
  `definición`, más `pendientes` para lo que no cerró. **No es un paso más: es la misma pasada,
  anotando en dos lugares en vez de uno.** Cuando un casillero recibe un **segundo** depósito, se
  contrasta: coinciden → uno confirma al otro; uno discrimina al otro → fija la lectura; no cierran
  → es hallazgo y baja por la escalera `E1/E2/E3`. **Un depósito que no cierra no se tira: queda
  pendiente en la hoja.**

  Va numerado `2-bis` y no `1` porque en el método las hojas se **abren** en el paso 1 y se **llenan**
  mientras se etiqueta. Acá se escriben después porque el paso 2 ya estaba hecho.

> **⟨M⟩ Advertencia honesta, y hay que dejarla escrita.** El párrafo I **no es una corrida a ciegas**:
> sus resultados ya se conocían antes de abrir la primera hoja —están en las tablas de abajo, que el
> autor produjo sin el instrumento—. Por lo tanto **esto es re-derivación, no prueba**. Que las hojas
> reproduzcan lo ya encontrado demuestra que el instrumento **alcanza** para lo conocido; **no
> demuestra que encuentre lo que nadie encontró**. Ésa es la deuda `IR-1` del acta `06` §7, y **la
> prueba pendiente es el párrafo II**, que todavía no se leyó. Hasta entonces, estas siete hojas valen
> como **ejemplo trabajado** y no como evidencia de rendimiento.

#### Hoja — `instalación`

  La hoja que más rinde del párrafo, y la única que cierra.

  | Casillero | Depósitos |
  | --- | --- |
  | **género** | ← `03.a.4` «tanto el casco como el puesto **son instalaciones**» · ← `03.a.5` «**y viviendas**» |
  | **eje** | ← `02.a.4` «un casco **central**» ⟂ `02.a.8` «varios puestos **rurales**» — eje nombrable: **ubicación**. Exhaustividad **no declarada** |
  | **miembros** | ← `02.a.3` «**un** casco» · ← `02.a.7` «**varios** puestos» |
  | **momento** | ← `01.b.4` «en su **momento de conformación**» · ← `02.a.1` «en el **mismo acto** se establece» |
  | **agente** | ← `02.a.5` «a cargo de un **Administrador**» *(casco)* · ← `02.b.3` «el nombre del **encargado**» *(puesto)* |
  | **alcance** | ← `04.a.5` «la administración del casco y de los puestos **se contempla en un futuro análisis**» |
  | **definición** | ← `03.a.6` «donde **se aloja el dueño y personal** de la estancia» — desarrolla la cara *vivienda* |
  | **pendientes** | ninguno sin causa. El agente de `02.a.2` «se establece» queda `⟦?⟧`: es **hallazgo**, no pendiente, y se cobra en la tabla de actos |

  **Tres cosas que esta hoja produce sola, y que ninguna oración dice:**

  | Qué | De qué casilleros sale |
  | --- | --- |
  | `instalación` y `vivienda` **no son dos términos: son dos caras de un referente** | **género**, dos depósitos en la misma hoja → **una** entrada de glosario, no dos |
  | **`AD-01`**: la instalación central es el casco; las periféricas, los puestos | **género** × **eje**: dos casilleros llenos que ninguna oración dijo junta |
  | La **asimetría** entre los dos miembros: del casco se nombra el **cargo**, del puesto el **nombre del titular** | **agente**, dos depósitos de distinta naturaleza. Es la pregunta 3 del cierre, y alimenta la 6 |

  > **Por qué `casco` y `puesto` no abren hoja propia, declarado con su causa.** Son los **miembros**
  > de esta hoja: todo lo que el párrafo predica de ellos lo predica del grupo —«tanto el casco como
  > el puesto…»— y lo que se predica del grupo se deposita en cada miembro. Abrir dos hojas más
  > duplicaría los mismos depósitos y volvería invisible el eje, que es lo único que las une. **Es
  > revisable**: si el párrafo II le predica a uno algo que no es común al otro, se parten.

#### Hoja — `estancia`

  | Casillero | Depósitos |
  | --- | --- |
  | **género** | ← `01.a.1` «una estancia **agropecuaria**»: establecimiento agropecuario |
  | **eje** | *(vacío: no hay otro establecimiento con el que contrastar)* |
  | **miembros** | ← `01.b.2` «al menos **un** campo» · ← `02.a.3` «**un** casco» · ← `02.a.7` «**varios** puestos» |
  | **momento** | ← `01.b.4` «en su momento de **conformación**» · ← `02.a.1` «en el **mismo acto**» — **dos depósitos, un solo antecedente posible**: el casco y los puestos nacen en el mismo acto que la estancia |
  | **agente** | `⟦?⟧` — ningún fragmento dice **quién** conforma |
  | **alcance** | *(vacío)* |
  | **definición** | *(vacío: el párrafo no la da)* |
  | **pendientes** | «varios» sin cota (`02.a.7`) → pregunta 4 · ¿`dueño` y `personal` integran la estancia o sólo se alojan en ella? (`03.a.6`) → preguntas 1 y 2 |

  > El casillero **momento** de esta hoja es el que vuelve barata una afirmación que costaba trabajo:
  > «el casco y los puestos se establecen en el acto de conformación». Dos depósitos en el mismo
  > casillero, y el segundo —«en el **mismo** acto»— no tiene otro antecedente que el primero.

#### Hoja — `campo`

  | Casillero | Depósitos |
  | --- | --- |
  | **género** | *(vacío)* |
  | **eje** | *(vacío)* |
  | **miembros** | ← `01.b.2` «al menos **un**» — cardinalidad respecto de la estancia, no miembros propios |
  | **momento** | ← `01.b.4` nace en la conformación |
  | **agente** · **alcance** | *(vacíos)* |
  | **definición** | *(vacía)* — **y es el pendiente central de la hoja** |
  | **pendientes** | la hoja queda **abierta con causa declarada**: el párrafo I nombra el campo y no dice qué es. El párrafo II la llena —superficie, identificador—, y por eso no se eleva como pregunta |

  > **Una hoja casi vacía es un resultado, no una falla.** Dice exactamente qué le falta al párrafo I
  > y dónde hay que ir a buscarlo.

#### Hojas — `Administrador` y `encargado`

  | Casillero | `Administrador` | `encargado` |
  | --- | --- | --- |
  | **género** | `⟦?⟧` — ni persona, ni rol, ni cargo: el texto no lo dice | `⟦?⟧` — ídem |
  | **eje** | *(vacío)* — **pendiente**: ¿se opone a `encargado`, o son el mismo rol en dos instalaciones? | *(vacío)* — el mismo pendiente, del otro lado |
  | **miembros** | ← `02.a.5` «**un** Administrador» *(por casco)* | ← `02.b.1` «**cada uno**» → **uno por puesto** |
  | **momento** | ← `02.a.1` queda a cargo en el mismo acto | **vacío** — **y éste es el hallazgo**: ver `RN-06` en el paso 4 |
  | **agente** | *(vacío)* | *(vacío)* |
  | **alcance** | ← `04.a.5` administrar el casco queda afuera | ← `04.a.5` administrar el puesto queda afuera |
  | **definición** | ← `02.a.5` quien está «a cargo de» el casco | ← `02.b.5` ⟦implícito⟧ quien está a cargo de un puesto |
  | **pendientes** | del Administrador **no** se declara el nombre, y del encargado **sí** (`02.b.3`) → pregunta 6 | un atributo declarado **conocible** (`02.b.3`) y el casillero **momento** vacío → escalera `E2` |

  > **Las dos hojas juntas muestran lo que ninguna muestra sola**: los mismos casilleros llenos y
  > vacíos **al revés**. Del casco se sabe que hay un Administrador y no cómo se llama; del puesto se
  > sabe que se puede saber cómo se llama el encargado y nada más. Esa asimetría es un hecho del
  > texto, y la mesa **no decide** si es deliberada: son las preguntas 3 y 6.

#### Hojas — `dueño` y `personal`

  | Casillero | Depósitos (las dos hojas, idénticas) |
  | --- | --- |
  | **género** · **eje** · **miembros** · **momento** · **agente** · **alcance** | *(todos vacíos)* |
  | **definición** | *(vacía)* |
  | **único depósito** | ← `03.a.6` «donde **se aloja el dueño y personal** de la estancia» — una relación de alojamiento con la instalación, y nada más |
  | **pendientes** | **un solo depósito y ningún casillero de identidad.** Con esto no se puede decidir si son cosas del modelo o sólo la explicación de para qué sirve una vivienda → preguntas 1 y 2 |

  > **Por qué estas dos hojas valen la pena aunque estén vacías.** Son la evidencia, junta y a la
  > vista, de que el párrafo nombra dos cosas del negocio y no dice **nada** de ninguna. Dispersa en
  > dos celdas de la tabla de términos, esa constatación no se ve; como dos hojas en blanco, salta.

---

### Paso 3. separando las cosas de diferente naturaleza

#### Tabla 1 — Términos

| Término | Designa | Unidad | Ámbito | De dónde |
|---|---|---|---|---|
| estancia agropecuaria | el establecimiento | — | el sistema | 01.a |
| campo | **⟦?⟧ no se define acá** ⟨M corrige: «no se define en este párrafo» — mismo sentido, fórmula uniformada con las dos de abajo⟩ | — | estancia | 01.b.3 |
| instalación | construcción de la estancia | — | estancia | 03.a.3 |
| vivienda | instalación **que se habita**: aloja al dueño y al personal | — | estancia | 03.a.5 + 03.a.6 |
| casco | la instalación **central** | — | estancia | 02.a.3 |
| puesto | instalación **rural** | — | estancia | 02.a.7 |
| Administrador | quien está a cargo del casco | — | casco | 02.a.6 |
| encargado | quien está a cargo de un puesto | — | puesto | 02.b.4 |
| dueño | **⟦?⟧ no se define acá** ⟨M corrige: «nunca se define»⟩ | — | ⟦?⟧ | 03.a.6 |
| personal | **⟦?⟧ no se define acá** ⟨M corrige: «nunca se define»⟩ | — | ⟦?⟧ | 03.a.6 |

> **⟨M⟩ Por qué «nunca» se cambió por «acá».** Vos vas **párrafo por párrafo** y estás en el
> primero: *«nunca se define»* es una afirmación sobre **el documento entero**, y para sostenerla
> habría que haber leído los otros tres párrafos y la lista de casos de uso. No están leídos. Lo
> que la información disponible sostiene es *«no se define acá»*, que es **provisorio**: si el
> párrafo II define el término, la celda se tacha sin que nadie se haya equivocado; con «nunca», en
> cambio, queda un error escrito.
>
> **Es la misma disciplina que gobierna todo el cuaderno** —no afirmar más de lo que la evidencia a
> mano sostiene—, aplicada contra uno mismo y no contra el autor. El caso análogo, para que se vea
> que no es puntillismo: la mesa de expertos tiene una regla que dice que **una afirmación de que
> algo *no está* sólo se admite con la búsqueda exhibida**, porque «una cita muestra dónde está una
> palabra, no dónde no está».
>
> Las tres celdas quedan **abiertas, no resueltas**: `dueño` y `personal` son además las dos hojas
> en blanco del paso 2-bis, y van a las preguntas 1 y 2 del cierre.

**Atributos**

| Atributo | Designa | Unidad | Ámbito | De dónde |
|---|---|---|---|---|
| nombre | cómo se llama la estancia | — | estancia | 01.a.2 |
| central | polo ⟨centro⟩ del eje **ubicación** de las instalaciones ⟨M corrige: «ocupa el centro; polo de `rural`»⟩ | — | **instalación** ⟨M corrige: casco⟩ | 02.a.4 |
| rural | polo ⟨periferia⟩ del mismo eje ⟨M corrige: «no central; polo de `central`»⟩ | — | **instalación** ⟨M corrige: puesto⟩ | 02.a.8 |
| nombre del encargado | se **registra** | — | encargado | 02.b.3 |

> ### ⟨M⟩ `central` y `rural` no eran sólo atributos — el caso testigo del criterio
>
> **Qué decía esta tabla y por qué estaba mal.** Los dos figuraban como atributos y la etiqueta del
> paso 2 era `A`. El error tiene nombre: se juzgó la **unicidad sobre el núcleo** —«no hay otro
> casco, luego `central` no recorta nada»— cuando **el conjunto contra el que un modificador recorta
> no es la extensión de su sustantivo**. Buscar «otro casco» es buscar en el lugar equivocado. Es el
> único resultado del replanteo del método que **invalida algo ya producido** (acta `06` §9.2), y es
> la razón por la que se escribe largo acá y no en una nota al pie.
>
> **Las dos preguntas, que son dos y no una** (`02-Criterio-De-Segmentacion.md` §4.3):
>
> | | Pregunta | Cómo se contesta sobre este párrafo | Resultado |
> | --- | --- | --- | --- |
> | 1 | **¿restringe?** → *¿hay eje depositado en la hoja?* | Sí. `02.a.4` «central» ⟂ `02.a.8` «rurales» cumplen las cuatro condiciones: **C1** los dos núcleos coordinados bajo un mismo `se establece`; **C2** los dos adjetivos pospuestos; **C3** adjetivo contra adjetivo; **C4** eje nombrable en una palabra — *«se distinguen por su **ubicación**»* | **Sí**, y recorta sobre el **género** de la hoja —*instalaciones*, depositado por `03.a.4`—, **no** sobre cascos |
> | 2 | **¿informa?** → *¿es un depósito nuevo?* | Para contestar que **no** hay que **señalar el depósito anterior** donde ya estuviera dicho cuál de las instalaciones es la central. No hay ninguno. Y la regla antifraude es explícita: **si no podés señalarlo, informa** | **Sí** |
>
> **Sí / sí → `Q1`.** Y `Q1` tiene **letra `R`** —`R/A` cuando la propiedad es además un dato de la
> cosa, que es exactamente este caso— y **doble destino**: la afirmación va a la **tabla de reglas**,
> y la propiedad queda en el **glosario como atributo**.
>
> Por eso las dos filas de arriba **se conservan** —son el destino «glosario» de `Q1`, y lo único
> que cambió es su `Designa` y su `Ámbito`, que eran de casco y de puesto y son de *instalación*— y
> lo que se agrega es el destino que faltaba: **la fila en la tabla 3**.
>
> **Y es una sola fila, no dos.** «El casco es la instalación central» y «los puestos son las
> periféricas» son las dos mitades de la misma afirmación: partirlas dejaría dos reglas que se
> incumplen con **el mismo contraejemplo**, que es lo que la condición 4 del criterio de corte
> prohíbe. La fila es **`AD-01`**, y está abajo.
>
> **El criterio, para llevarse:** *el conjunto de contraste se **mira** en el casillero «género» de
> la hoja del referente; no se **busca** entre los hermanos del núcleo.* Y su contracara: **«no
> restringe» no quiere decir «no importa»** — un modificador que no recorta igual predica algo, y
> ése es el cuadrante `Q3`.

#### Tabla 2 — Actos

| Acto | Quién lo hace | Qué recibe | Qué cambia | De dónde |
|---|---|---|---|---|
| **conformación** | ⟦?⟧ | ⟦?⟧ | existe la estancia · con nombre · con ≥1 campo · con **un** casco a cargo de un Administrador · con **varios** puestos, cada uno con encargado | 01.b.5 · 01.b.4 · 02.a.1 · 02.a.2 |
| administrar el casco | — | — | **fuera de alcance** | 04.a |
| administrar el puesto | — | — | **fuera de alcance** | 04.a |
| ⟨M⟩ **designar al encargado de un puesto** · **⟦Propuesta⟧** | ⟦?⟧ | el puesto, y el nombre de su encargado | el puesto pasa de no tener nombre de encargado a tenerlo | **ninguna**: el enunciado **no lo pide**. Es la premisa que falta — ver `RN-06` en el paso 4 |

> **⟨M⟩ La última fila no es un acto del enunciado.** Lleva rótulo `⟦Propuesta⟧` y columna «de
> dónde» vacía a propósito: no hay ancla porque no hay texto. Está en la tabla y no escondida en una
> nota porque un hueco que no se ve no se discute, y **se borra el día que el autor conteste la
> pregunta 5**, de una u otra manera.

> `se puede conocer` (02.b.2) **no abre fila**. No es algo que alguien hace en un momento: es la afirmación de que el dato está disponible. Es atributo, no acto.

#### Tabla 3 — Reglas

| Id | Letra | Intención | Especie | Momento | Quién verifica | De dónde |
|---|---|---|---|---|---|---|
| RN-01 | «está compuesta por al menos un campo» | la estancia se conforma con ≥1 campo | estructura | Precondition de `conformación` | la conformación | 01.b.2+01.b.4 |
| RN-02 | «se establece un casco central» | la conformación establece **un** casco | estructura | Precondition de `conformación` | la conformación | 02.a.2-3+02.a.1 |
| RN-03 | «a cargo de un Administrador» | todo casco tiene un Administrador | estructura | ⟦?⟧ | **nadie** | 02.a.5 |
| RN-04 | «y varios puestos rurales» | la conformación establece varios puestos | estructura | Precondition de `conformación` | la conformación | 02.a.7+02.a.1 |
| RN-05 | — *(no hay cita)* ⟨M corrige: «⟦implícita⟧»⟩ · rótulo **`Intención`** | **cada** puesto tiene un encargado | estructura | ⟦?⟧ | **nadie** | 02.b.1 + 02.b.3 + 02.b.5 ⟨M⟩ |
| RN-06 | «se puede conocer el nombre del encargado» | de cada encargado se registra el nombre | estructura | ⟦?⟧ | **nadie** | 02.b.3 |
| RN-07 | «son instalaciones y viviendas» | casco y puesto son instalaciones habitadas | estructura | — | — | 03.a.4 |
| AL-01 | «se contempla en un futuro análisis del sistema» | administrar casco y puestos queda afuera | alcance | — | — | 04.a |
| ⟨M⟩ **AD-01** | — *(ninguna oración la dice entera)* · rótulo **`Intención`** | la instalación **central** de la estancia es el **casco**; las **periféricas** son los **puestos** | estructura | Invariant | **nadie** | 02.a.4 **+** 02.a.8 **+** 03.a.4 |

> **⟨M⟩ Dos filas sin `Letra`, y la columna vacía es el dato.** `RN-05` y `AD-01` no tienen cita
> porque **no hay texto que citar**: la primera está implícita en el párrafo, la segunda está
> repartida entre tres fragmentos. La regla es la misma para las dos y no admite excepción: *una
> regla sin cita textual lleva rótulo `Intención` y declara de qué fragmentos se deduce*. **Lo que
> nunca se hace es rellenar `Letra` con una paráfrasis** — ahí es exactamente donde la propuesta se
> cuela disfrazada de fuente. El razonamiento de cada una está en su ficha del paso 4.
>
> **`RN-06` tiene letra y le falta otra cosa:** un acto que la verifique. Está diagnosticada en el
> paso 4 como **`E2`, incompletitud** —no contradicción—, con la propuesta que le corresponde.


---

### Paso 4. La regla del orden

  > Nota: Una regla es una oración de la que se puede decir «esto es falso».
  > La forma: **‹quién› + ‹qué se exige›**, en palabras del negocio, con sujeto explícito y en presente.

  | ❌ | ✅ |
  |---|---|
  | `estancia una casco` | **La estancia tiene exactamente un casco.** |
  | `campo al menos uno` | **La estancia, al conformarse, tiene al menos un campo.** |

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

> **⟨M⟩ Las tres fichas de arriba son del autor y quedan como están.** Les faltan cinco de los siete
> campos —`Especie`, `Momento`, `Quién verifica`, `Dueño`, `De dónde`—; están en la tabla 3 y el
> autor los va a volcar él. Las fichas que siguen son las que faltaban, **completas**, con la
> plantilla que este mismo cuaderno fija: `[momento] · sujeto · verbo · exigencia`, **sujeto
> explícito, presente del indicativo, vocabulario del negocio**.

#### ⟨M⟩ Las fichas que faltaban

```
⟨M⟩ RN-04
  Letra:            «y varios puestos rurales»
  Intención:        La estancia, al conformarse, tiene varios puestos.
  Especie:          estructura
  Momento:          Precondition de `conformación`
  Quién verifica:   la conformación
  Dueño:            estancia
  De dónde:         02.a.7 + 02.a.1
  ⚠ «varios» no tiene cota: ver pregunta 4. Mientras no se conteste,
    la exigencia es contable sólo si «varios» ≥ 2.

⟨M⟩ RN-05
  Letra:            — no hay cita: la afirmación no está escrita en ninguna parte.
                    Rótulo `Intención`, nunca `Letra`.
  Intención:        Todo puesto tiene un encargado a cargo.
                    De qué se deduce, fragmento por fragmento:
                      02.b.1  «cada uno»            distributivo sobre ⟦los puestos rurales⟧:
                                                    lo que sigue vale para TODOS, no para alguno
                      02.b.3  «el nombre del
                               encargado»           el artículo definido presupone que hay
                                                    un encargado, y uno por puesto
                      02.b.5  ⟦implícito⟧           la atadura que los dos anteriores sostienen
  Especie:          estructura
  Momento:          ⟦?⟧ — no se puede decir si vale en el instante en que el puesto nace
  Quién verifica:   nadie
  Dueño:            puesto
  De dónde:         02.b.1 + 02.b.3 + 02.b.5

⟨M⟩ RN-06
  Letra:            «se puede conocer el nombre del encargado»
  Intención:        De cada encargado de puesto se registra el nombre.
  Especie:          estructura
  Momento:          condición de completitud — NO es Invariant: el puesto nace sin el nombre
                    (prueba del instante de nacimiento, y el CU-1 la confirma)
  Quién verifica:   nadie   ← acá está el hueco. Ver el diagnóstico de abajo
  Dueño:            puesto — es quien tiene el dato del encargado en el momento de decidirlo
  De dónde:         02.b.3 (+ 02.b.2 por el «se puede conocer»)

⟨M⟩ RN-07
  Letra:            «Tanto el casco como el puesto son instalaciones y viviendas»
  Intención:        El casco y el puesto son instalaciones, y se habitan.
  Especie:          estructura
  Momento:          Invariant — vale en el instante en que cada uno nace
  Quién verifica:   nadie, y no hay acto que pueda violarlo
  Dueño:            instalación
  De dónde:         03.a.4 (+ 03.a.5, 03.a.6)
  ⚠ No son dos reglas: «instalación» y «vivienda» son dos CARAS del mismo referente
    —dos depósitos en el casillero «género» de la misma hoja—, no dos términos.
    Una entrada de glosario, una regla.
  ⚠ ⟨M⟩ La tabla 3 dejó «—» en Momento y en Quién verifica. La ficha los completa:
    «—» no es un valor válido en esos dos campos; «nadie» sí, y es un hallazgo.

⟨M⟩ AL-01
  Letra:            «La administración del casco y de los puestos se contempla en un futuro
                     análisis del sistema»
  Intención:        Administrar el casco y administrar los puestos quedan fuera del alcance.
  Especie:          alcance
  Momento:          — no hay estado prohibido: hay función no pedida
  Quién verifica:   — nadie, y no corresponde que nadie lo haga
  Dueño:            ninguno en el dominio — termina en la frontera del sistema
  De dónde:         04.a.5 (+ 04.a.1, 04.a.2, 04.a.3)

⟨M⟩ AD-01   — afirmación distribuida
  Letra:            — ninguna oración la dice entera. Rótulo `Intención`, nunca `Letra`,
                    y grado de certeza `inferencia`.
  Intención:        La instalación central de la estancia es el casco; las periféricas son
                    los puestos.
                    Las tres premisas, todas letra del texto:
                      (a) 02.a.4  «un casco central»            ubica un miembro en el eje
                      (b) 02.a.8  «varios puestos rurales»      ubica el otro, y ES el eje
                      (c) 03.a.4  «tanto el casco como el
                                   puesto son instalaciones»    crea el conjunto del que habla
                    (c) crea retroactivamente el género; (a) y (b) depositan el eje que ese
                    género admite. La conclusión es un paso deductivo de quien analiza:
                    la gramática suministra las premisas, no la conclusión.
  Especie:          estructura
  Momento:          Invariant
  Quién verifica:   nadie
  Dueño:            instalación
  De dónde:         02.a.4 + 02.a.8 + 03.a.4
```

> **⟨M⟩ La prueba de admisión de `AD-01`: si se suprime cualquiera de las tres premisas, la
> afirmación se cae.** Se borra una por vez y se mira si la conclusión sobrevive. Si sobrevive a
> alguno de los borrados, esa premisa no era premisa —y entonces la conclusión venía de otro lado,
> probablemente de lo que uno sabe de estancias—.
>
> | Se suprime | Qué queda | ¿Sobrevive la afirmación? |
> | --- | --- | --- |
> | **`02.a.4`** «central» | el casco sin la propiedad por la que sería el central | **No.** No queda quién sea la instalación central: la conclusión ni siquiera se puede formular |
> | **`02.a.8`** «rurales» | los puestos sin el polo opuesto | **No.** Sin segundo polo no hay eje; sin eje, «central» deja de recortar y se cae además la segunda mitad —«las periféricas son los puestos»— |
> | **`03.a.4`** «son instalaciones» | dos cosas con adjetivos y ningún conjunto común | **No.** No queda el conjunto del que la afirmación habla. Es la premisa que crea el género |
>
> **Las tres son necesarias → la supresión se cumple.** Y la otra prueba, la que descarta lo que el
> autor podría desdecir sin contradecirse: la **negación** —*«la instalación central de la estancia
> no es el casco»*— puesta en el párrafo **contradice de frente** a `02.a.4` leída junto con
> `03.a.4`. El autor no podría agregarla sin corregirse. Con tres anclas citables, en el mismo
> párrafo, y las dos pruebas cumplidas, **`AD-01` entra**.
>
> **Por qué se discutió tanto y por qué hacía falta la fila.** Ni `02.a` la dice —no menciona
> instalaciones— ni `03.a` la dice —no menciona ni centralidad ni periferia—. Está **entre** las dos,
> y un procedimiento que etiqueta fragmento por fragmento **no tiene ningún paso en el que pueda
> aparecer**. Aparece en la hoja del referente, y en un solo movimiento: *dos casilleros llenos que
> ninguna oración dijo junta*.

#### ⟨M⟩ `RN-06` pide un acto que no existe — diagnóstico `E2`, incompletitud

> **La escalera se baja en orden y está prohibido saltear.** Un `E2` reportado como `E3` manda a
> descartar una de las dos afirmaciones, y con el autor ausente eso significa **tirar una afirmación
> verdadera del negocio sin dejar rastro**.
>
> | Peldaño | Veredicto |
> | --- | --- |
> | **`E1` — ambigüedad de expresión** | **Descartada.** Ninguna lectura de «se puede conocer el nombre del encargado» disuelve el problema: lea lo que lea, el dato existe y ningún acto lo carga |
> | **`E2` — incompletitud** | **Aplicada.** El caso que satisface a las tres piezas a la vez se construye sin esfuerzo: *el puesto nace sin nombre de encargado, y el nombre se carga después, en un acto que el enunciado no escribió.* **Muchos mundos** cumplen y el texto no elige |
> | **`E3` — contradicción** | **No se llega.** Hay mundo que las satisface, así que no hay contradicción. Decir que la hay sería el error caro |
>
> **Los tres lugares que hacen necesario el acto, y hay que nombrar los tres:**
>
> | # | Pieza | Qué aporta |
> | --- | --- | --- |
> | 1 | **El párrafo I**, `02.b.3`: «de los cuales cada uno **se puede conocer el nombre del encargado**» | el dato **existe** y el negocio lo declara **conocible** |
> | 2 | **El CU-1** de la fuente —«Alta de la estancia»—: el casco y los puestos se crean **«sin designar los nombres de los encargados de ambas entidades»** | en el **único acto que los crea**, el dato **no se carga**, y el enunciado lo dice con todas las letras |
> | 3 | **La oración (04)**, `04.a.5` → `AL-01`: la administración del casco y de los puestos queda fuera | el acto que naturalmente lo cargaría está **declarado fuera de alcance** |
>
> Las tres juntas dejan **un atributo declarado conocible sin ningún momento en que alguien lo
> escriba**. Eso es exactamente una *intención sin momento de verificación*, y la salida que le
> corresponde es `Propuesta`, **nombrando la premisa que falta**.
>
> **⟦Propuesta⟧ El acto faltante**
>
> ```
> ⟨M⟩ ACTO PROPUESTO — designar al encargado de un puesto
>   Enunciado:     Al designar al encargado de un puesto, el puesto registra su nombre.
>   Quién lo hace: ⟦?⟧ — el enunciado no nombra agente para ningún acto de este párrafo
>   Qué recibe:    el puesto, y el nombre de su encargado
>   Qué cambia:    el puesto pasa de no tener nombre de encargado a tenerlo
>   Qué resuelve:  le da momento a RN-06, que hoy no verifica nadie
> ```
>
> **Frase de residuo —obligatoria, y no es una formalidad—:** *este acto **no está en el enunciado**.
> No es lo que la fuente dice: es la premisa que falta para que `02.b.3` y el CU-1 puedan ser
> verdaderos a la vez. Si el autor lo rechaza, lo que se cae no es la propuesta: se cae `RN-06`, que
> quedaría como un dato que el negocio declara conocible y que nunca nadie carga.*
>
> **Lo que la mesa NO hizo acá, dicho explícito:** no dio el acto por pedido —la fila de la tabla 2
> lleva su rótulo y su «de dónde» vacío—, no lo nombró como caso de uso, y **no decidió cuándo se
> carga el nombre**: ésa es la pregunta 5, y es del autor.

---

## ⟨M⟩ Cierre de la actividad — párrafo I

### Recuento

```text
fragmentos del paso 2:          31   (en 5 afirmaciones del paso 1, sobre 4 oraciones)
reglas:                          9
  por especie:                   estructura 8 · alcance 1 · restricción 0 · derivación 0
reglas no escritas:              2   RN-05 (implícita en el párrafo)
                                     AD-01 (distribuida entre tres anclas)
huecos — regla que no verifica
  ningún acto:                   3   RN-03 · RN-05 · RN-06
  de ellos, con propuesta
  de acto:                       1   RN-06
huecos de agente ⟦?⟧:            3   02.a.2 «se establece» · 02.b.2 «se puede conocer»
                                     · la columna «quién lo hace» de `conformación`
                                     (04.a.4 «se contempla» NO se cobra: su destino es `E`)
?  levantados → sobrevividos:    ? 0/31 → 0/31 → 0/31
términos huérfanos:              2   `dueño` y `personal`: ninguna regla los usa
hojas del referente:             7 abiertas · 1 cerrada (instalación)
                                     · 6 con pendientes declarados con causa
preguntas para el autor:         6
```

**Cómo se lee el `0/31`, y cómo no.** Ningún fragmento quedó con la letra `?` **sola**, así que la
compuerta no tuvo sobre qué correr. Eso **no** quiere decir que el párrafo no tenga dudas: las dudas
de este párrafo **no son de etiqueta, son de negocio**, y por eso salieron por donde el método las
manda —como `⟦?⟧`, que es hallazgo, y como pendientes en las hojas—. Y hay una lectura menos cómoda
que conviene dejar escrita: **una corrida que levanta cero `?` también puede ser una corrida que no
los levantó.** La medida honesta de cuánto tiene este párrafo sin resolver no es el `0/31`: son los
**tres huecos de agente, los tres huecos de verificación y las seis preguntas**.

**Las cuatro especies, y lo que su reparto dice.** Ocho de nueve son **estructura** y ninguna es
restricción ni derivación. No es casualidad ni es un defecto del análisis: el párrafo I **compone la
estancia** —dice de qué está hecha y qué nace con qué— y no prohíbe nada ni calcula nada. Las
restricciones y las derivaciones de este enunciado viven en los párrafos que siguen. Si el reparto
hubiera dado ocho restricciones, habría que sospechar del análisis.

### Preguntas para el autor

**Las seis salen en un solo lote, al cierre, y ninguna está contestada.** Cada una pasó la compuerta:
ningún criterio en mano la resuelve, ningún otro depósito de la misma hoja la resuelve, y la
respuesta **cambia lo que se construye**. Cada una va con las dos lecturas en pugna y la consecuencia
de cada una, que es la única forma en que una pregunta así se contesta en quince segundos.

**P-1. ¿Quién conforma la estancia?** El acto está nombrado —`conformación`, `01.b.5`— y no tiene
agente: `se establece` y el resto del párrafo son impersonales.
· *(a)* Lo hace **un actor** —alguien da de alta la estancia—: el acto tiene dueño, la columna «quién
lo hace» se llena y la conformación es un caso de uso con precondiciones verificables.
· *(b)* **No lo hace nadie del sistema**: la estancia ya existe cuando el sistema arranca y
«conformación» nombra sólo el instante en que los datos quedan cargados.
**Consecuencia:** con (a) hay un caso de uso de alta y las cuatro reglas de conformación —`RN-01`,
`RN-02`, `RN-04` y `RN-05`— son **precondiciones que alguien verifica**; con (b) no hay acto que las
verifique y las cuatro pasan a ser invariantes que nadie hace cumplir.

**P-2. ¿`dueño` y `personal` son cosas del modelo?** El párrafo los nombra una sola vez, en `03.a.6`,
y no dice nada más de ninguno de los dos.
· *(a)* **Son cosas del negocio**: dos términos con ficha propia, y hay que decir qué se guarda de
ellos y cómo se relacionan con la estancia.
· *(b)* **Son la explicación de para qué sirve una vivienda**: `03.a.6` se queda como el «designa» de
*vivienda* y no abre fila.
**Consecuencia:** con (a) aparecen dos hojas que el párrafo I deja en blanco y que hay que llenar
desde otro lado; con (b) la expresión «el dueño y personal **de la estancia**» queda sin referente en
el modelo, y hay que aceptar que el enunciado nombra gente que el sistema no conoce.

**P-3. ¿`Administrador` y `encargado` son el mismo rol?** Uno está a cargo del casco y el otro de un
puesto, y el texto los nombra distinto.
· *(a)* **Mismo rol, dos nombres** según la instalación: una sola cosa en el glosario, con nombre
preferente y tabla de equivalencias.
· *(b)* **Dos roles distintos**: dos términos, y hay que decir en qué se diferencian **además** de la
instalación donde están.
**Consecuencia:** con (a) la asimetría del párrafo —del casco se nombra el cargo, del puesto el
nombre del titular— es sólo redacción, y el `nombre` vale para los dos; con (b) la asimetría es un
hecho del negocio y hay que explicar por qué del Administrador no se registra nada.

**P-4. ¿Cuántos son «varios» puestos?** `02.a.7`, y la cardinalidad quedó marcada como dudosa desde
el paso 2.
· *(a)* **«Varios» es indeterminado, incluido cero**: una estancia se puede conformar sin ningún
puesto.
· *(b)* **«Varios» es al menos dos**: una estancia con un solo puesto se rechaza en la conformación.
**Consecuencia:** con (a) `RN-04` no es exigible y es sólo una descripción de lo típico —`C(describe)`,
no `R`—; con (b) `RN-04` es una precondición **contable** y hay una validación concreta en el alta.
Es la diferencia entre que exista o no exista un rechazo que alguien tiene que programar.

**P-5. ¿Cuándo se carga el nombre del encargado?** Es la pregunta que abre el hueco de `RN-06`.
· *(a)* **En la conformación**: entonces el CU-1 está mal donde dice «sin designar los nombres de los
encargados de ambas entidades», y el acto propuesto sobra.
· *(b)* **Después, en un acto propio**: entonces hace falta el acto, y hay que decir **quién** lo
ejecuta, dado que administrar el puesto está declarado fuera de alcance (`AL-01`).
**Consecuencia:** con (a) se corrige el caso de uso y el hueco desaparece sin agregar nada; con (b) o
se abre un caso de uso nuevo, o se corre la frontera de `AL-01`. **No hay tercera opción en la que el
dato se cargue solo**, y ésa es la razón por la que la pregunta no se puede dejar pasar.

**P-6. ¿Del `Administrador` se registra el nombre?** Del encargado el texto dice que se puede conocer
(`02.b.3`); del Administrador no dice nada (`02.a.5`).
· *(a)* **Sí, simétrico al encargado**: `nombre del Administrador` es un atributo más y `RN-06` vale
para las dos instalaciones.
· *(b)* **No**: la asimetría es deliberada y del casco sólo se sabe que hay un Administrador a cargo.
**Consecuencia:** con (a) el hueco de `RN-06` **se duplica** —tampoco hay acto que cargue ese nombre—
y la propuesta del paso 4 tiene que cubrir los dos casos; con (b) hay que poder explicar por qué el
casco, que es **la instalación central**, guarda menos datos que un puesto.

### Cláusula de no proactividad

Este cuaderno **no modifica el enunciado**. Las seis preguntas de arriba **quedan abiertas**: son del
autor, y la mesa no eligió entre las lecturas de ninguna.

Lo único agregado que **no está en la fuente** son dos cosas, las dos rotuladas:

| Qué | Rótulo | Qué lo marca |
| --- | --- | --- |
| El **acto propuesto** «designar al encargado de un puesto» | `⟦Propuesta⟧` | su frase de residuo, y la columna «de dónde» vacía en la tabla 2 |
| La **afirmación distribuida** `AD-01` | `Intención` | columna `Letra` vacía, grado de certeza `inferencia`, y sus tres anclas a la vista |

Todo lo demás con marca **⟨M⟩** es **reclasificación o completamiento** de lo que el autor ya había
producido, siempre con su ancla. Y lo que no se hizo, dicho para que nadie lo busque: **no se avanzó
al párrafo II**, no se tocaron los pasos 1 y 2 salvo las dos etiquetas que `central` y `rural`
obligaron, y el enunciado del paso 1 **no se reescribió** — se le puso el aviso.


