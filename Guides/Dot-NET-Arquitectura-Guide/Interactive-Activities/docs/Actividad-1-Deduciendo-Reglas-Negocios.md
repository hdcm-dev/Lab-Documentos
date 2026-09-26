

# Actividad 1 - Deduciendo Reglas de negocio - RN

  > Objetivo: 
  > Identificar y Abstraer las reglas de negocios.

  Repaso sobre el concepto de reglas de negocios

  > Son las reglas que viven en las entidades
  > ¿Es del negocio o de la tecnología?
  > ¿Describe, prohíbe o calcula?
  > ¿Vale siempre o antes de un acto?


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
  02.a.4 [ "central"                         A  ]  restringe dentro de ⟨instalaciones⟩
  02.a.5 [ "a cargo de un Administrador"     R(estructura) ]
  02.a.6 [ "Administrador"                   T  ]
  02.a.7 [ "varios puestos rurales"          T+C]  C(dudoso): ¿cuántos es «varios»?
  02.a.8 [ "rurales"                         A  ]  el otro polo del contraste con «central»
  ```

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

### Paso 3. separando las cosas de diferente naturaleza

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


