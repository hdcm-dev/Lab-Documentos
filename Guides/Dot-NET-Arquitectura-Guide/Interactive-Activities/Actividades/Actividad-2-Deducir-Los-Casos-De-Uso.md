

# Actividad 2 - Deduciendo los Casos de Uso - CU

  > **Objetivo**
  > Escribir los casos de uso del problema y cruzarlos contra las reglas de la actividad 1, para
  > descubrir qué reglas no tienen dónde vivir y qué actos faltan.

  **Qué entra y qué sale**

  | | |
  | --- | --- |
  | **Entra** | las tres tablas de la actividad 1 —términos, actos, reglas— y el mismo párrafo |
  | **Sale** | los casos de uso con precondiciones y postcondiciones, la matriz regla × caso de uso, y los huecos |

  **Lo que esta actividad NO hace todavía**: no decide clases ni capas. Sigue describiendo el
  problema, igual que la actividad 1.

  > ⚠ **Esto no es una etapa posterior a la 1: es su otra mitad.** Las reglas dicen qué actos tienen
  > que existir; los casos de uso dicen qué reglas se quedaron sin casa. Se va y se vuelve entre las
  > dos, y eso no es desprolijidad: es el método.

---

## Introducción a los casos de uso

### Qué es un caso de uso

  > Un caso de uso es **lo que alguien le pide al sistema, de punta a punta, y que termina dejándole
  > algo que le sirve.**

  Tres condiciones, y las tres tienen que darse:

  | | Condición | Si falla |
  | --- | --- | --- |
  | 1 | **Alguien** lo inicia —un actor, no el sistema solo | es un proceso interno, no un caso de uso |
  | 2 | Se completa **de una sentada** | es un procedimiento largo: se parte |
  | 3 | Al terminar, el que lo pidió **tiene algo** | es un paso, no un caso de uso |

### Cómo se reconoce: la prueba del mostrador

  La operación concreta:

  > **¿Alguien se acercaría al mostrador a pedir esto?** Y si se lo dan, **¿se va conforme?**

  - «Dar de alta un campo» → alguien lo pide, y se va con el campo dado de alta. ✅
  - «Validar el identificador» → nadie lo pide; es un paso de otro. ❌
  - «Administrar los campos» → es un menú, no un acto. Adentro hay varios casos de uso. ❌

  El error más común es escribir **pantallas** en vez de actos. Una pantalla puede contener tres
  casos de uso, y un caso de uso puede cruzar dos pantallas.

### De dónde salen las precondiciones

  **No se inventan: ya están escritas.** Son la columna `Momento` de la tabla 3 de la actividad 1.

  | En la actividad 1 | En el caso de uso |
  | --- | --- |
  | `Precondition de ‹acto›` | **precondición** de ese caso de uso |
  | `Invariant` | no es precondición de nadie: vale siempre |
  | `condición de completitud` | **postcondición** de algún acto — y si ninguno la establece, **es un hueco** |
  | `⟦?⟧` | todavía no se sabe: se arrastra como pregunta |

  Por eso la actividad 1 va primero: sin la columna `Momento`, las precondiciones se escriben de
  memoria.

### El actor, y el «se» que lo esconde

  Todo caso de uso tiene un actor. Cuando el documento está escrito en impersonal —«**se** establece»,
  «**se** contempla», «**se** puede conocer»— **el actor no está**, y eso hay que anotarlo, no
  completarlo.

  > Un actor que no aparece en el texto se escribe `⟦?⟧` y va al cuestionario. Poner «el usuario»
  > porque queda mejor es inventar.

---

## La plantilla de un caso de uso

  ```
  CU-nn  ‹Nombre en infinitivo: «Dar de alta un campo»›

    Actor:          ‹quién lo inicia›  ·  o  ⟦?⟧
    Disparador:     ‹qué lo hace empezar›
    Precondición:   ‹lo que tiene que ser cierto antes›  ·  de la columna Momento
    Flujo:          1. ...
                    2. ...
    Alternativas:   a.1 ‹qué pasa si falla›  → ‹qué regla lo rechaza›
    Postcondición:  ‹qué quedó cierto al terminar›
    Reglas que usa: RN-nn, RN-nn
    De dónde:       ‹ancla en el documento›
  ```

  **El nombre va en infinitivo y nombra el acto**, no la pantalla: «Asignar una parcela a una
  actividad», no «Pantalla de asignación».

---

## Consigna

### Paso 0. Recuperar los actos de la actividad 1

  La tabla 2 ya tiene actos. Copialos: **son los candidatos a caso de uso**, y cada uno viene con su
  ancla.

  Ojo: no todo acto de la tabla 2 es un caso de uso. Los que están **fuera de alcance** se listan
  igual, marcados, para que la ausencia sea visible y no parezca un olvido.

### Paso 1. Pasar cada acto por la prueba del mostrador

  Por cada acto, las tres condiciones. El resultado es una de tres:

  - **Es caso de uso** → se le escribe la ficha.
  - **Es un paso de otro** → se anota adentro del flujo del que lo contiene.
  - **Es un menú** → se parte en los casos de uso que agrupa.

### Paso 2. Escribir la ficha de cada caso de uso

  Con la plantilla de arriba. **Las precondiciones se copian de la columna `Momento`**, no se
  escriben de nuevo.

  Donde el actor no esté en el texto, va `⟦?⟧`.

### Paso 3. La matriz regla × caso de uso

  Una fila por regla, una columna por caso de uso. En cada cruce: **¿este caso de uso hace cumplir
  esta regla?**

  ```
  |        | CU-1 | CU-2 | CU-3 | ... |  ¿alguno?
  | RN-01  |  ✔   |      |      |     |    sí
  | RN-02  |      |      |      |     |    NO  ← hueco
  ```

  **La columna final es el producto de esta actividad.** Una regla sin ningún ✔ es una regla que
  nadie verifica.

### Paso 4. Diagnosticar cada hueco

  Por cada regla sin caso de uso, la escalera, **en orden y sin saltear**:

  | # | Diagnóstico | Cómo se resuelve |
  | --- | --- | --- |
  | 1 | **Ambigüedad**: hay una lectura que la hace caer en un caso de uso existente | leyendo bien |
  | 2 | **Incompletitud**: falta un caso de uso | **nombrando el que falta**, bajo rótulo `Propuesta` |
  | 3 | **Contradicción**: la regla afirma algo que ningún flujo permite | al autor |

  La prueba que separa 2 de 3: **¿puedo imaginar un sistema donde la regla y los casos de uso
  convivan?** Si puedo, falta información. Si no, hay conflicto.

  > **Prohibido saltar al 3.** Declarar contradicción lo que sólo estaba incompleto obliga a
  > descartar una regla que estaba bien.

---

## Criterios de aceptación

  - [ ] Cada caso de uso tiene **actor**, aunque sea `⟦?⟧`.
  - [ ] Ninguna precondición se escribió de memoria: todas vienen de la columna `Momento`.
  - [ ] La matriz está **completa**: todos los cruces visitados, no sólo los que dan ✔.
  - [ ] Cada regla sin caso de uso tiene **diagnóstico declarado** (1, 2 o 3) y dice por qué se
        descartaron los peldaños anteriores.
  - [ ] Cada acto declarado fuera de alcance está **listado y marcado**, no omitido.
  - [ ] Los nombres están en infinitivo y nombran actos, no pantallas.

## Lo que esta actividad le devuelve a la 1

  Al terminar, volvé a la tabla 3 y completá lo que esta actividad resolvió:

  - Las columnas `Momento` que estaban en `⟦?⟧` y ahora tienen acto.
  - La columna `Quién verifica`, que recién acá se puede llenar de verdad.
  - Las reglas que resultaron ser **dos**, porque el caso de uso las separó.

  **Eso es la ida y vuelta.** Si al cruzar no cambió nada en la actividad 1, o el documento era
  perfecto, o el cruce no se hizo.
