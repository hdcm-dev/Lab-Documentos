---
doc_id: DEMO-MU-00
doc_type: guia-practica
title: Mini demo MovilidadUrbana
description: Tutorial de la mini demo, que documenta MovilidadUrbana.MAUI desde otro repositorio con trazas al código fijado por SHA y el contrato extraído en ejecución.
status: borrador
origin: ia-generated
confidence: alta
owner: Lab-Documentos
last_review: 2026-10-27
audience: [desarrollo, analista, estudiante, agente]
traces: [RD-01, RD-02, RD-03, RD-04]
---

# Mini demo MovilidadUrbana

Documentación de una app real (MovilidadUrbana.MAUI, del repositorio Lab-E2E.WebBlazor) escrita **desde otro
repositorio**, sin copiar el código y con una compuerta que da rojo cuando lo escrito deja de coincidir con lo que el
código hace. Es el ejemplo del escalón 1 del sistema documentador: audiencias sin roles, fuente generada del SHA,
ancla SHA entre repositorios con aviso de deriva y trazas de dos niveles.

## Índice

1. [Definiciones](#1-definiciones)
2. [Qué hay en esta carpeta](#2-qué-hay-en-esta-carpeta)
3. [Cómo funciona](#3-cómo-funciona)
4. [Correr la verificación](#4-correr-la-verificación)
5. [Tutorial: documentar una pantalla nueva](#5-tutorial-documentar-una-pantalla-nueva)
6. [Cuando el código cambia](#6-cuando-el-código-cambia)
7. [El caso real: RN-LOC-01 y el 60/80](#7-el-caso-real-rn-loc-01-y-el-6080)
8. [Límites de la demo](#8-límites-de-la-demo)

## 1. Definiciones

| Término | Significado |
| --- | --- |
| `source.lock` | Archivo que fija el repositorio y el SHA del código que se documenta. Todo lo que la demo afirma se comprueba contra ese commit |
| Ficha | Documento de una regla de negocio (`RN-LOC-nn`) con la forma de *Deducción de reglas de negocio* §5.1 y una tabla de anclas |
| Ancla | `ruta#Símbolo@SHA`: dónde se declara, se muestra, se verifica o se aplica una regla. Con una afirmación (`= 60`, `contiene …`) que la compuerta comprueba |
| Traza | En el frontmatter, `traces: [ID@MAYOR.MENOR]`: de qué requerimiento o regla, y de qué versión, sale lo que una página afirma |
| Traza de nivel 1 | Página → ficha: el ID existe y la versión coincide |
| Traza de nivel 2 | Ficha → código: cada ancla resuelve en el SHA y cumple su afirmación |
| RRD | Registro de requerimientos documentales: [`docs/_requisitos.md`](docs/_requisitos.md). Una fila `RD-nn` por cosa que la documentación tiene que lograr |
| CL-06, CL-07 | Clases de documento: ayuda al usuario final (CL-06) y técnica escrita (CL-07) |
| Deriva | Diferencia entre el SHA fijado y `main` del repositorio del código. Avisa; no bloquea |
| docs-guard | Compuerta documental de Lab-Documentos ([`tools/docs-guard/`](../../../../../tools/docs-guard/README.md)) |

## 2. Qué hay en esta carpeta

| Pieza | Clase | Para quién | Qué es |
| --- | --- | --- | --- |
| [`source.lock`](source.lock) | — | todos | Repositorio, SHA, rutas y proyecto de la API |
| [`verificar.sh`](verificar.sh) | — | desarrollo, CI | Baja el código, extrae el contrato y corre los controles |
| [`contracts/openapi.json`](contracts/openapi.json) | contrato | desarrollo | Contrato de la API extraído en ejecución del SHA |
| [`docs/_requisitos.md`](docs/_requisitos.md) | CL-02 (RRD) | analista, PO | Qué documentación hace falta y cómo se comprueba |
| [`docs/fichas/RN-LOC-01.md`](docs/fichas/RN-LOC-01.md) | anexo | analista | Largo del nombre (con divergencia 60/80) |
| [`docs/fichas/RN-LOC-02.md`](docs/fichas/RN-LOC-02.md) | anexo | analista | Localidad única por nombre y provincia |
| [`docs/ayuda/alta-de-localidad.md`](docs/ayuda/alta-de-localidad.md) | CL-06 | persona usuaria | Cómo dar de alta una localidad |
| [`docs/tecnica/arquitectura.md`](docs/tecnica/arquitectura.md) | CL-07 | desarrollo | Capas y dónde vive cada regla |
| [`docs/tecnica/compilar-y-primer-pedido.md`](docs/tecnica/compilar-y-primer-pedido.md) | CL-07 | desarrollo | Compilar la API, pedir el contrato, primer alta |

Fuera de esta carpeta, en la raíz de Lab-Documentos: el workflow `.github/workflows/mini-demo-movilidad.yml` y la
prueba negativa permanente `tools/docs-guard/fixtures/anclas/real-rn-loc-01/`.

## 3. Cómo funciona

```mermaid
flowchart LR
  subgraph LD["Lab-Documentos (esta carpeta)"]
    PAG["docs/ayuda, docs/tecnica<br/>traces: RD-nn, RN-LOC-nn@ver"] -->|nivel 1| FIC["docs/fichas/RN-LOC-nn<br/>anclas ruta#Símbolo@SHA"]
    LOCK["source.lock<br/>SHA 8fb4c3a"]
    CON["contracts/openapi.json"]
  end
  subgraph LE["Lab-E2E.WebBlazor (no se toca)"]
    SHA["commit 8fb4c3a"]
    MAIN["main"]
  end
  LOCK --> SHA
  FIC -->|nivel 2: bloquea| SHA
  CON -->|diff: bloquea| SHA
  FIC -.->|deriva: avisa| MAIN
  CON -.->|deriva: avisa| MAIN
```

La compuerta tiene dos partes:

| Dónde corre | Qué comprueba | Bloquea |
| --- | --- | --- |
| `docs-guard` (cada PR) | Enlaces, frontmatter, RRD, anti-inyección, estructura, trazas de nivel 1 | Sí |
| `mini-demo-movilidad` (cada PR que toca la demo o `tools/docs-guard/`) | C1 contrato = extraído del SHA; C2 un contrato con una operación plantada da rojo; C3 trazas de nivel 1 y 2 contra el SHA; C4 la ficha RN-LOC-01 sin corregir da rojo; A1 deriva contra `main` | C1 a C4 sí; A1 sólo avisa |

## 4. Correr la verificación

Requisitos: bash, git, curl, Python 3, el SDK de .NET 10 y Node.js (para docs-guard). Desde la raíz de Lab-Documentos:

```bash
DEMO=Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/Examples/MovilidadUrbana-Mini-Demo
scripts/ci-local.sh todo "$DEMO"     # docs-guard sobre la demo
"$DEMO/verificar.sh"                 # contrato, anclas y deriva contra el código
```

El final de una corrida en orden:

```text
== agregador mini demo
  C1  verde
  C2  verde
  C3  verde
  C4  verde
== mini demo: verde
```

Si un control da rojo, la línea anterior al agregador dice qué archivo y qué fila. Por ejemplo:
`docs/fichas/RN-LOC-01.md:62: ERROR ancla-valor-distinto: NombreEntry.MaxLength: la ficha afirma 60 y el código tiene 80`.

## 5. Tutorial: documentar una pantalla nueva

Ejemplo trabajado: documentar la pantalla **Encuesta** para la persona usuaria. Los pasos valen para cualquier
pantalla.

### 5.1 Agregar la fila al RRD

En [`docs/_requisitos.md`](docs/_requisitos.md), una fila nueva con el número siguiente y las mismas ocho columnas de
la tabla: `ID`, `Audiencia`, `Tarea`, `Pieza`, `Fuente`, `Criterio`, `Decisión` y `Decide y fecha`. La compuerta exige
seis (`ID`, `Audiencia`, `Tarea`, `Pieza`, `Criterio`, `Decisión`); las otras dos se completan igual. Reglas:

- **Criterio**: tiene que tener las palabras **Dado**, **Cuando** y **Entonces**, y nombrar el instrumento.
- **Pieza**: clase y enlace relativo al archivo, como en las filas existentes.
- **Fuente**: una regla con versión por cada ficha que la página va a trazar (§5.2).
- **Decisión**: `aceptado`, `rechazado`, `diferido` o `pendiente`. Quien escribe no decide: una fila nueva va como
  `pendiente`, y en `Decide y fecha` se pone quién la va a decidir, sin fecha (`PO, pendiente`). Cuando la persona
  decide, cambia la decisión y agrega la fecha.
- **Frontmatter del RRD**: su `traces` lista las reglas de las que salen las filas; se agregan las fichas nuevas con
  su versión.

```text
| RD-05 | persona usuaria | Completar el paso 1 de la encuesta | CL-06, [`docs/ayuda/encuesta.md`](ayuda/encuesta.md) | `RN-ENC-01@1.0`, `RN-ENC-02@1.0` | Dado el paso 1 de la encuesta, cuando la persona lee la ayuda, entonces cada número coincide con el de la ficha; instrumento: `traces-resolve` | pendiente | PO, pendiente |
```

### 5.2 Escribir la ficha de cada regla que la página nombre

Si la pantalla tiene reglas (un mínimo, un rango, un formato, una unicidad), **cada regla lleva su ficha** en
`docs/fichas/`, con un ID `RN-<ÁREA>-nn` nuevo (los asigna la demo; ver §8). Copiá
[`RN-LOC-02.md`](docs/fichas/RN-LOC-02.md) como modelo: frontmatter (`doc_type: ficha`, `traces: [RD-04]`, porque las
fichas son la pieza de RD-04), encabezado `# RN-XXX-nn — Nombre`, línea `**Versión:** 1.0`, la tabla de la ficha y la
tabla de anclas.

**Valores de la tabla de la ficha** (forma de *Deducción de reglas de negocio* §5.1; la lista es cerrada):

| Campo | Valores |
| --- | --- |
| ¿Divergen? | `No`, `Leve` o `De fondo`, y en qué. Hay divergencia sólo si el código hace otra cosa que lo que la pantalla o la regla prometen |
| Especie | restricción, derivación, alcance o estructura |
| Momento | Invariant, Precondition de ‹acto› (por ejemplo, «del alta»), completitud o ninguno |
| Grado de certeza | testimonio, letra, caso de uso, código, inferencia o conjetura |
| Propuesta | Obligatoria si hay divergencia de fondo; si no, «No corresponde» |

**Tabla de anclas.** Encabezado `| Rol | Ancla | Afirma |`. El código del SHA lo deja `verificar.sh fuente` en
`$TRABAJO/sha/`. `TRABAJO` es la variable de entorno, si está; si no, `$RUNNER_TEMP`, `$TMPDIR` o `/tmp`, más
`/mini-demo-movilidad`.

| Archivo | Símbolo | Ejemplo | Qué entiende la compuerta |
| --- | --- | --- | --- |
| `.cs` (incluye `.xaml.cs`) | `Clase.Miembro` | `src/MovilidadUrbana.MAUI/Domain/Rules/EncuestaRules.cs#EncuestaRules.EdadMinima@8fb4c3a` | Constantes, campos, propiedades y métodos, con cuerpo de expresión (`=>`) o de bloque, con o sin atributos (`[RelayCommand]`). No entiende parámetros de constructores primarios ni funciones locales |
| `.xaml` | `Elemento.Atributo`, con el elemento nombrado por `x:Name` o por `AutomationId` | `src/MovilidadUrbana.MAUI/Pages/EncuestaPage.xaml#edad.MaxLength@8fb4c3a` | Cualquier atributo; el valor se compara como texto (`` = `Numeric` `` también vale) |

La columna **Afirma** se escribe con el valor entre comillas invertidas: `` = `16` ``, `` contiene `texto` `` o
`` no contiene `texto` ``; vacía o `—` comprueba sólo que el símbolo existe. `contiene` mira la declaración completa,
cuerpo incluido. La columna **Rol** no la lee la compuerta; los roles de las fichas modelo (`declara`, `muestra`,
`verifica`, `aplica`, `persiste`) cubren los casos de la demo, y se puede usar otro si ninguno describe el lugar.

El SHA del ancla tiene que ser el de `source.lock` (alcanza con los primeros siete caracteres). Escribí en la ficha lo
que **el código hace**; si eso diverge de lo que la pantalla promete, marcá «¿Divergen?» como «De fondo» y dejá la
decisión en la propuesta. No la decidas vos (§7). Un dato que no es divergencia (por ejemplo, que la base no tiene
índice único) va en un párrafo debajo de la tabla, como en RN-LOC-02.

### 5.3 Escribir la página

Creá `docs/ayuda/encuesta.md` con este frontmatter; todos los campos son obligatorios. Los comentarios `#` son sólo
para este tutorial: en el archivo real se quitan.

```yaml
---
doc_id: DEMO-MU-22          # prefijo DEMO, siguiente número libre del grupo (ayuda 2x, técnica 3x, fichas 1x)
doc_type: guia-practica
title: Completar el paso 1 de la encuesta
description: Una oración de hasta 160 caracteres que diga qué resuelve la página.
status: borrador            # vigente lo decide una persona
origin: ia-generated        # human, ia-assisted o ia-generated
confidence: alta            # obligatorio si origin no es human: alta, media o baja
owner: Lab-Documentos
last_review: 2026-10-27
audience: [persona-usuaria] # humano, agente, estudiante, docente, desarrollo, qa, devops, po, analista, persona-usuaria
traces: [RD-05, RN-ENC-01@1.0, RN-ENC-02@1.0]
---
```

El cuerpo, como [`alta-de-localidad.md`](docs/ayuda/alta-de-localidad.md): un solo título `#`, índice, qué se carga,
pasos, qué pide cada campo y qué hacer cuando la app no acepta un dato. Si la pantalla no guarda (un paso intermedio),
esa sección se llama por lo que pasa («Si la app no te deja seguir»). Una página puede cubrir una parte de una
pantalla; el título y la tarea de la fila del RRD lo dicen. La ayuda trata de vos, aunque la app trate de usted.

**Qué tiene que estar en una ficha.** Cada límite o valor de una regla que la página nombra (mínimos, máximos, rangos,
formatos). Los datos de ejemplo («Goya», «3450») y los mensajes sin números no: un mensaje se cita textual y, si sale
de una regla, la ficha de esa regla ya lo ancla en su fila `verifica`.

### 5.4 Validar

```bash
scripts/ci-local.sh todo "$DEMO"
"$DEMO/verificar.sh"
```

Los dos tienen que terminar en verde. El final de `ci-local.sh`:

```text
== agregador docs-guard
  enlaces      verde
  esquema      verde
  ai           verde
  estructura   verde
  trazas       verde
  contrato     verde
  fijadas      verde
  autoprueba   verde
== docs-guard: verde
```

El de `verificar.sh` está en §4. Errores frecuentes:

| Mensaje | Causa | Arreglo |
| --- | --- | --- |
| `traza-no-resuelve: RD-05` | La fila no está en el RRD o el ID está mal escrito | Agregar la fila (§5.1) |
| `traza-version-desfasada` | La página traza `@1.0` y la ficha está en otra versión | Releer la página contra la ficha y actualizar la traza |
| `ancla-sin-simbolo` | El símbolo no existe en el SHA, o es una forma que la compuerta no entiende (§5.2) | Buscarlo en `$TRABAJO/sha/`; revisar mayúsculas y la clase |
| `ancla-valor-distinto` | El código tiene otro valor que el que afirma la ficha | Escribir lo que el código hace (§5.2) |
| `prefijo` | El `doc_id` no usa un prefijo registrado | Usar `DEMO-MU-nn` |
| `rrd-criterio` | Falta Dado, Cuando o Entonces en el criterio | Reescribir el criterio |

### 5.5 Proponer el cambio

En el mismo cambio, además de las piezas nuevas:

- la tabla de §2 de este README suma una fila por pieza nueva;
- [`CHANGELOG.md`](../../../../../CHANGELOG.md) de Lab-Documentos suma una línea en la entrada del día, bajo
  «Añadido».

Después, una rama (`demo-mu/<pantalla>`, por ejemplo `demo-mu/encuesta-paso-1`), un commit con un mensaje que diga qué
pantalla se documentó («Mini demo: ayuda del paso 1 de la encuesta») y un PR contra `main` de Lab-Documentos. En el PR
corren `docs-guard` y `mini-demo-movilidad` en el runner `[self-hosted, i7infra-dev]`; el PR se fusiona sólo con los
dos en verde. El cuerpo del PR copia el final de las dos salidas de §5.4.

## 6. Cuando el código cambia

Mientras `source.lock` no cambie, nada de lo que pase en el otro repositorio pone esta demo en rojo: sólo aparece el
aviso de deriva (A1) en el registro de la corrida. Para actualizar la demo a un código nuevo:

1. Cambiar `sha:` en `source.lock`.
2. Correr `verificar.sh`. Las anclas que ya no se cumplen dan rojo y dicen cuál.
3. Corregir cada ficha y subir su versión (`1.1` → `1.2`); `traces-resolve` marca entonces las páginas con traza
   desfasada, que se releen y se actualizan.
4. Si C1 da rojo, revisar el diff y reemplazar `contracts/openapi.json` por el extraído (queda en
   `$TRABAJO/sha/openapi.json`).

Todo eso va en un mismo PR.

## 7. El caso real: RN-LOC-01 y el 60/80

Al derivar la ayuda desde el código apareció un defecto: la pantalla promete «Entre 3 y 60 caracteres», el campo deja
escribir 80 y la app guarda nombres de más de 60. La primera versión de la ficha (1.0) copió la promesa de la pantalla
y la compuerta dio rojo en dos anclas. La 1.1 dice lo que el código hace y deja la decisión a una persona: elegir
entre 60 y 80 es una divergencia entre letra e intención, y no se resuelve documentando.

La regla que sale de este caso: **lo que la ayuda afirma se verifica contra el comportamiento; si diverge, se
documenta lo que hace y se avisa**. La ayuda al usuario lo dice en lenguaje llano ([§3 de la
ayuda](docs/ayuda/alta-de-localidad.md#3-qué-pide-cada-campo)) y la ficha 1.0 se conserva como prueba negativa
permanente.

## 8. Límites de la demo

| Límite | Por qué |
| --- | --- |
| No hay roles ni confidencialidad | La app móvil no tiene roles; la ayuda por rol se demostró aparte (Prueba-Ayuda-Por-Rol, en esta misma carpeta `Examples/`) |
| Los IDs `RN-LOC-nn` son provisionales | MovilidadUrbana no tiene registro de reglas; los asigna esta demo |
| Las páginas no se publican en un sitio | Se leen como `.md` en el repositorio |
| No se modifica el código | Si el repositorio del código abre un expediente que lo autorice, la documentación pasa a vivir junto al código |
