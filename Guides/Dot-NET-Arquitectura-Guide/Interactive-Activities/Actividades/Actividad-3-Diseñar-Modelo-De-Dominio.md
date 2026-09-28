

# Actividad 3 - Diseñando el Modelo de Dominio

  > **Objetivo**
  > Convertir las reglas en objetos: decidir qué clases existen, qué sabe hacer cada una, y **dónde
  > vive cada regla**.

  **Qué entra y qué sale**

  | | |
  | --- | --- |
  | **Entra** | las tres tablas de la actividad 1 y los casos de uso de la actividad 2 |
  | **Sale** | el diagrama de clases y la **tabla de ubicación**: una fila por regla, diciendo en qué objeto vive |

  **Lo que esta actividad NO hace todavía**: no hay capas, no hay proyectos, no hay base de datos, no
  hay HTTP. Todo eso es la actividad 4.

  > ⚠ **Acá se cruza la frontera.** Las actividades 1 y 2 describían el problema. Ésta **decide una
  > solución**, y la decisión es orientada a objetos. Lo que venías haciendo servía igual para
  > cualquier paradigma; de acá en adelante, no.

---

## Introducción al modelo de dominio

### Dos modelos con el mismo nombre

  | | Qué es | Qué disciplina |
  | --- | --- | --- |
  | **Modelo conceptual** | qué conceptos existen y cómo se relacionan | análisis: describe |
  | **Modelo de diseño** | con invariantes, fábricas, agregados, visibilidad | diseño: decide |

  Esta actividad **empieza en el primero y termina en el segundo**. Conviene saber en cuál se está,
  porque las preguntas son distintas.

### Entity o Value Object: la prueba de los dos billetes

  > **¿Dos con los mismos datos son el mismo, o son dos?**

  | Respuesta | Qué es | Ejemplo |
  | --- | --- | --- |
  | Son **dos distintos** | **Entity**: tiene identidad propia, que sobrevive al cambio de sus datos | dos campos de 50 ha con distinto identificador |
  | Son **el mismo** | **Value Object**: es un valor; si cambia, es otro | 50 hectáreas son 50 hectáreas |

  Segunda prueba, por si la primera no alcanza: **¿tiene sentido preguntar "cuál de los dos"?** Si no
  lo tiene, es valor.

### Dónde vive cada regla: la pregunta 4, ahora en serio

  En la actividad 1 se preguntó **quién tiene los datos para decidirla**. Acá esa respuesta se
  convierte en una ubicación:

  | Quién tiene los datos | Dónde va la regla |
  | --- | --- |
  | **Un objeto, mirándose a sí mismo** | en ese objeto |
  | **Un objeto, mirando a los que contiene** | en el que los contiene —la raíz del agregado |
  | **Nadie: hay que mirar a otras instancias, o a algo de afuera** | **en ningún objeto del dominio** |

  La tercera fila no es un fracaso: **es el hallazgo principal de esta actividad.** Esas reglas son
  las que en la actividad 4 van a hacer aparecer una capa nueva. Anotalas aparte, con su nombre.

### Invariante o precondición, ahora en código

  Es la columna `Momento` de la actividad 1, traducida a dónde se escribe la guarda:

  | Momento | Dónde se verifica |
  | --- | --- |
  | **Invariant** | en la creación **y en cada método que toca el dato** |
  | **Precondition** de ‹acto› | **sólo** en el método de ese acto |
  | **Condición de completitud** | en el método que cierra el proceso — y si no hay ninguno, es el hueco de la actividad 2 |

  La prueba que ya usaste: **¿vale en el instante en que el objeto nace?** Si el objeto nace sin
  cumplirla, no es invariante, y ponerla en el constructor hace que el objeto no se pueda crear.

### Agregado y raíz

  > Un **agregado** es un grupo de objetos que se modifica como una unidad, con **una** entidad raíz
  > que es la única a la que se llega desde afuera.

  Cuándo hace falta: **cuando hay una regla que ningún objeto suelto alcanza a verificar**, y sí
  alcanza el que los contiene.

  La operación que lo hace real no es el diagrama: es **la visibilidad**. Si a las partes se llega
  sin pasar por la raíz, la raíz no tiene nada que hacer cumplir.

### Cuándo una jerarquía de tipos es la respuesta

  > **Cuando los subtipos se diferencian por comportamiento que el negocio nombra, y todo lo que vale
  > para la base sigue valiendo para cada hoja.**

  Tres preguntas, con una condición previa —que la diferencia sea de **comportamiento**, no de datos:

  1. **¿Puede un objeto cambiar de variante sin dejar de ser el mismo?** Si puede, es composición.
  2. **¿Cuántos ejes varían a la vez?** Uno admite jerarquía; dos la hacen explotar.
  3. **¿Todo lo que vale para la base vale para cada hoja?**

  Y el criterio que las cierra: **la jerarquía no se valida mirando las clases, sino los programas
  escritos contra la base.** Si hay un solo lugar donde el código que usa el tipo base tiene que
  preguntar de qué subtipo se trata, ahí está la refutación.

  ❌ Heredar **para compartir código** ata dos cosas que el negocio no ató.

### Fábricas

  Un constructor público no puede hacer dos cosas que acá hacen falta: **llevar el nombre del acto** y
  **negarse a crear**. Por eso las reglas de creación viven en un método con nombre —`Crear`,
  `Conformar`— y el constructor queda privado.

---

## La plantilla de la tabla de ubicación

  Es el entregable principal, más que el diagrama.

  ```
  | Regla | Enunciado | Vive en | Cómo se verifica | Visibilidad |
  | RN-01 | ...       | Estancia | Conformar(), guarda | público |
  | RN-07 | ...       | Campo    | CrearParcela(), guarda | internal |
  | RN-10 | ...       | NINGUNO  | —                | —        |
  ```

  **Ninguna fila puede quedar vacía.** `NINGUNO` es una respuesta válida y esperada.

---

## Consigna

### Paso 0. Separar conceptos de datos

  Sobre la tabla 1 de términos: por cada uno, la prueba de los dos billetes. Sale una lista de
  **entidades candidatas** y otra de **valores candidatos**.

  Los términos que no son ni una cosa ni la otra —los que resultaron prosa— se marcan y se dejan
  afuera, con su motivo.

### Paso 1. Ubicar cada regla

  Una por una, con la pregunta 4. Llená la tabla de ubicación **completa** antes de dibujar nada.

  El diagrama viene después y sale solo: si la tabla está bien, las clases y sus métodos ya están
  decididos.

### Paso 2. Las reglas sin dueño

  Juntá las que dieron `NINGUNO` y, por cada una, escribí **qué necesitaría para decidirse**: ver
  todas las instancias, consultar algo de afuera, conocer el tipo.

  > Esa lista es el insumo de la actividad 4. **No la resuelvas acá**, y sobre todo no inventes un
  > objeto que las contenga sólo para que la tabla quede llena.

### Paso 3. Decidir los agregados

  Buscá las reglas que un objeto solo no alcanza a verificar y su contenedor sí. Cada una propone una
  raíz.

  Por cada agregado, declarar: **quién es la raíz, qué hay adentro, y qué visibilidad tienen las
  partes**.

### Paso 4. Decidir las jerarquías

  Por cada familia de subtipos, las tres preguntas. Y la contraprueba: **¿hay algún recorrido que les
  pida lo mismo a todos?** Si no lo hay, la jerarquía no se gana el lugar todavía.

### Paso 5. El diagrama

  Recién ahora. Clases, atributos, métodos y relaciones, con la cardinalidad que salió de la
  actividad 1.

  **Cada método del diagrama tiene que aparecer en la columna «cómo se verifica» de alguna regla, o
  en el flujo de algún caso de uso.** Un método que no está en ninguna de las dos no tiene de dónde
  haber salido.

---

## Criterios de aceptación

  - [ ] La tabla de ubicación está **completa**: ninguna regla sin fila.
  - [ ] `NINGUNO` aparece al menos una vez, o está explicado por qué no.
  - [ ] Cada invariante se verifica **en la creación y en cada método que toca el dato**, no en uno solo.
  - [ ] Cada agregado declara **raíz, partes y visibilidad**.
  - [ ] Ninguna jerarquía existe sólo para compartir código.
  - [ ] Ningún método del diagrama es huérfano: sale de una regla o de un caso de uso.
  - [ ] Ningún nombre del diagrama contradice el glosario de la actividad 1.

## Lo que esta actividad le devuelve a las anteriores

  - **Actos sin dueño**: casos de uso cuyo flujo no encuentra a quién pedírselo. Vuelven a la 2.
  - **Términos que resultaron no ser nada**: vuelven a la 1, marcados.
  - **Reglas que eran dos**: la ubicación las separa cuando una mitad vive en un objeto y la otra no.
