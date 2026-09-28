

# Actividad 4 - Implementando la Arquitectura

  > **Objetivo**
  > Llevar el modelo de dominio a una solución .NET en capas donde **la separación la hace cumplir el
  > compilador**, y medir qué se ganó.

  **Qué entra y qué sale**

  | | |
  | --- | --- |
  | **Entra** | el modelo de dominio de la actividad 3, y la lista de reglas sin dueño |
  | **Sale** | la solución compilando, con cada separación verificada por un comando, y la medición final |

  > ⚠ **Ésta es la única actividad donde el juez no sos vos.** En las tres anteriores había que
  > revisar a mano si el trabajo estaba bien. Acá, cuando una dependencia está mal, **el compilador
  > lo dice**. Por eso cada hito lleva su comando: si el comando no falla cuando tiene que fallar, el
  > hito no está hecho.

---

## Introducción a la arquitectura en capas

### La regla de dependencia

  > **Las dependencias del código apuntan sólo hacia adentro.** Nada de un círculo interior puede
  > nombrar algo de un círculo exterior.

  La operación que la vuelve real no es el diagrama: es **el proyecto**. Mientras todo viva en un
  solo proyecto, la regla es disciplina; cuando cada capa es un proyecto, **el compilador la hace
  cumplir**.

  Ésa es la razón de fondo por la que esta actividad separa proyectos, y no es prolijidad.

### Por qué aparece `Application`

  En la actividad 3 quedó una lista de **reglas sin dueño**: las que ningún objeto del dominio puede
  decidir, porque hay que mirar todas las instancias o algo de afuera.

  Esas reglas tienen que vivir en algún lado. Hoy, en la versión de escritorio, viven en el
  formulario —el único lugar que ve todo—. El problema es que **desde el otro lado de una API no hay
  formulario**.

  > `Application` no aparece porque el modelo sea pobre. Aparece porque **hay reglas que no caben en
  > ningún objeto y hoy viven en el único lugar que los ve a todos.**

### Lo que viaja y lo que no

  | Cruza HTTP sin costo | No cruza |
  | --- | --- |
  | Un cálculo que **no pregunta por el tipo** | Una decisión escrita con `is` o con un `switch` sobre el tipo |
  | Un dato | Un objeto con comportamiento |

  **El polimorfismo no viaja.** Del otro lado llega un discriminador y hay que reconstruir. Eso no es
  una limitación del framework: es la naturaleza de un contrato entre dos procesos.

### Cómo se mide si sirvió

  No con opiniones. Con un cambio concreto hecho dos veces:

  > **Agregar un tipo nuevo**, en la versión vieja y en la nueva, y **contar cuántos lugares exige
  > tocar el compilador**.

  Lo que se cuenta no son archivos: son **puntos que el compilador obliga**. Un punto que se puede
  olvidar sin que el programa deje de compilar es deuda, no trabajo.

---

## Los diez hitos

  Cada uno tiene un error típico que **debe** aparecer, y un comando que lo revela. Un hito sin señal
  observable no está hecho: está declarado.

| Hito | Qué se hace | El error que debe aparecer | Cómo se verifica |
| --- | --- | --- | --- |
| **A0** | Medir la línea de base: dónde vive hoy cada regla | leer el modelo, verlo con comportamiento y concluir que está todo bien sin abrir el formulario | `grep` de las decisiones por tipo y de las reglas que el formulario verifica |
| **A1** | Copiar el modelo **sin tocar una línea** y exponer las consultas | empezar «mejorando» antes de medir | el informe por HTTP da **los mismos números** que el de escritorio |
| **A2** | Del índice al identificador | exponer rutas posicionales y que anden en la demo | dos peticiones con un alta en el medio: con índices, la segunda devuelve otra cosa |
| **A3** | **Nace `Application`** con las reglas sin dueño | reponerlas en el controlador: el mismo error con otro traje | las pruebas pasan **sin HTTP y sin base** |
| **A4** | `Domain` como proyecto aparte | el `using` sin la referencia | `dotnet build` → **`CS0234`**; `Domain` sin referencias salientes |
| **A5** | El cierre sin preguntar por el tipo, y el contrato que transporta subtipos | reproducir la cadena de `is` en el handler o en el serializador | `grep ' is '` sobre `src/` → **vacío** |
| **A6** | Del `double` al Value Object | cambiar el tipo y no la fábrica: quedan dos caminos de creación | una suma con decimales da **distinto** que antes |
| **A7** | Persistir la jerarquía | agregar el ORM a `Domain` para poner atributos | `sha256sum` de `Domain.dll` antes y después: **idéntico** |
| **A8** | `Contracts` y el cliente | referenciar `Domain` desde el cliente «para reusar» | el cliente referencia **sólo `Contracts`** |
| **A9** | **El tipo nuevo, en los dos mundos** | declarar la mejora sin contar | la tabla de conteo, y el defecto latente de la versión vieja **no** se reproduce |

  > **A0 y A9 son mediciones y enmarcan a las demás.** La actividad empieza contando y termina
  > contando; lo del medio es lo que explica la diferencia.

---

## Consigna

### Paso 0. Medir antes de tocar (A0)

  Reproducí a mano la tabla de «dónde vive hoy cada regla». No la copies de la actividad 1: **hacela
  de nuevo mirando el código**. Las dos tienen que coincidir; si no coinciden, una de las dos está
  mal y hay que averiguar cuál.

  Y contá los puntos de toque del cambio de A9 **antes** de empezar, sobre la versión vieja.

### Paso 1. Trasplantar sin mejorar (A1)

  El modelo pasa tal cual. **La tentación de arreglarlo es el error del hito**: si lo mejorás ahora,
  perdés la comparación.

### Paso 2 a 7. Los hitos A2 a A7

  De a uno, y **en orden**. Cada uno termina cuando su comando da lo que tiene que dar — incluido
  cuando lo que tiene que dar es un error de compilación.

  > Los hitos **A5 y A7** tocan la jerarquía: cómo se publica y cómo se persiste. Son los que más
  > ayuda del apunte necesitan, y los que más revelan si la decisión de la actividad 3 era correcta.

### Paso 8. El cliente (A8)

  El cliente se conecta **sólo por el contrato**. Si en algún momento te hace falta referenciar el
  dominio, pará: **eso es el hallazgo**, no un obstáculo. Anotá qué necesitabas y por qué el contrato
  no te lo daba.

### Paso 9. Contar (A9)

  El mismo cambio, en los dos mundos, con la tabla:

  ```
  |                                   | Escritorio | En capas |
  | archivos tocados                  |            |          |
  | puntos que el compilador EXIGE    |            |          |
  | puntos que se pueden olvidar      |            |          |
  | defectos latentes que se activan  |            |          |
  ```

  **La fila que importa es la tercera.** Un punto que se puede olvidar sin que el programa deje de
  compilar es donde va a aparecer el error dentro de seis meses.

---

## Criterios de aceptación

  - [ ] Cada hito tiene su **comando corrido y su salida registrada**, no una afirmación.
  - [ ] Los hitos cuyo resultado es un **error de compilación** muestran el error.
  - [ ] `Domain` no tiene **ninguna** referencia saliente.
  - [ ] Ningún cliente referencia `Domain`, `Application` ni la infraestructura.
  - [ ] Las reglas de `Application` se prueban **sin HTTP y sin base de datos**.
  - [ ] La tabla de A9 está **completa**, incluida la fila de los puntos que se pueden olvidar.
  - [ ] Lo que no se hizo está **declarado**, no omitido.

## Lo que esta actividad le devuelve a las anteriores

  - **Reglas que no se podían implementar como estaban**: vuelven a la 1, reformuladas.
  - **Casos de uso que en el código resultaron dos**: vuelven a la 2.
  - **Ubicaciones que el compilador rechazó**: vuelven a la 3. Ésas son las más valiosas, porque son
    las únicas del taller donde **alguien que no opina** dijo que estaban mal.
