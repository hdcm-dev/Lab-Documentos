# Para retomar — sesión del 2026-09-26

> **Qué es esto**: el refresco corto, para esta misma sesión. Si la sesión murió y hay que empezar
> de cero, el documento es [`Contexto.md`](Contexto.md), que es el largo.
>
> Leer esto lleva dos minutos y alcanza para seguir donde quedamos.

---

## Dónde quedamos

El PO está haciendo la **Actividad 1**: deducir las reglas de negocio del **párrafo I** del enunciado
de La Estancia, paso a paso, con lápiz, en su cuaderno
[`Actividad-1-Deduciendo-Reglas-Negocios.md`](Actividad-1-Deduciendo-Reglas-Negocios.md).

**El párrafo I está fragmentado y etiquetado completo** (oraciones 01 a 04), y las tres tablas del
paso 3 están armadas. El paso 4 se explicó y quedaron tres deudas.

**El PO se tomó una pausa. Retoma mañana.**

---

## Las tres deudas del párrafo I — son mías, no de él

Lo más corto para arrancar:

1. **`AD-01` no está en la tabla 3.** La afirmación distribuida del PO —*«la instalación central de la
   estancia es el casco; las periféricas son los puestos»*— se discutió largo y **nunca se registró**.
   Le corresponde fila propia, rótulo `Intención`, y las tres anclas que la producen: `02.a.4` +
   `02.a.8` + `03.a.4`.
2. **`RN-05` no tiene `Letra`.** Está marcada ⟦implícita⟧; le falta el rótulo `Intención` y decir de
   qué fragmentos se deduce.
3. **`RN-06` pide un acto que no existe.** Diagnóstico E2 (incompletitud). Corresponde redactar el
   acto faltante bajo rótulo `Propuesta` —algo como «designar el encargado de un puesto»— nombrando
   los tres lugares que lo hacen necesario.

---

## El hallazgo más fuerte del párrafo, por si hace falta recordarlo

**RN-06 declara un dato que ningún acto del sistema puede cargar nunca.**

| Momento | ¿Se puede asignar el nombre del encargado? |
| --- | --- |
| conformación | **no** — el CU-1 dice «sin designar los nombres de los encargados de ambas entidades» |
| administración | **no existe** — declarada fuera de alcance en la oración 4 |

Necesitó cruzar tres lugares: el párrafo I, el CU-1 y la oración de alcance. Y apareció **leyendo el
documento original**, no la transcripción: la transcripción había condensado el CU-1 en una fila de
tabla y perdido la cláusula. Ya se repuso, con su nota.

Segundo hallazgo, del mismo tipo: **las tres reglas sin verificador (RN-03, RN-05, RN-06) son
exactamente las tres que hablan de roles**, y su verificación viviría en la administración que la
oración 4 declara fuera. El agujero no es descuido: es consecuencia de AL-01.

---

## El cuestionario abierto (6 de 7 sobrevivieron la compuerta)

| # | Pregunta |
| --- | --- |
| P-2 | ¿Quién conforma la estancia? |
| P-3 | ¿`dueño` y `personal` son términos del modelo o son prosa? |
| P-4 | ¿`Administrador` y `encargado` son el mismo rol con dos nombres? — **I-02** |
| P-5 | ¿Cuántos son «varios» puestos? — `C(dudoso)` |
| P-6 | **¿Cuándo se carga el nombre del encargado?** — la importante |
| P-7 | ¿Del Administrador también se registra el nombre? — asimetría de detalle |

Murió una sola: *«¿qué es un campo?»*, porque el párrafo siguiente lo define. Esa es la compuerta B-8
funcionando.

---

## Corrección pendiente de aplicar a las tablas

En la tabla 1 se anotó *«nunca se define»* para `dueño` y `personal`. **Está mal para el estado del
PO**: él va párrafo por párrafo y todavía no puede saberlo. Corresponde `no se define acá`
—provisorio— y recién al cerrar el último párrafo se convierte en «nunca» o se llena. Lo mismo con
`campo`.

---

## Lo que está corriendo en segundo plano

**La mesa está deliberando sobre el replanteo del modelo** (acta esperada en
`.../OUTPUTs/Mesa/06-Acta-Replanteo-Del-Modelo.md`). Se le pasaron las seis tesis del PO. Si decide
que el modelo cambia, **pueden cambiar nombres de etiquetas, no los hallazgos**.

Quedan además **37 parches redactados y sin aplicar** en `04-Afirmacion-Distribuida-Y-Modificadores.md`:
20 a `02`, 14 a `03` y 3 al `Knowledge`. **No aplicarlos hasta que la mesa se expida**, porque el
encargo incluye decidir cuáles sobreviven.

---

## Siguiente paso natural

Cerrar las tres deudas de arriba —son cortas— y después seguir con el **párrafo II** del enunciado,
donde aparecen las primeras restricciones de verdad y la segunda inconsistencia: `administración`
con dos sentidos, acto y ámbito.

El temario de las cuatro actividades quedó en [`Temas.md`](Temas.md).
