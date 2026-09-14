# Caso de Estudio 1 — Impresión de tickets desde aplicación MAUI híbrida

**Documento:** caso-de-estudio-1.md  
**Versión:** 1.0  
**Fecha:** 2026-04-15  
**Contexto:** Análisis y corrección de User Stories aplicando los criterios de la guía de estudio de Scrum  
**Referencia:** [guia-de-estudio.md](guia-de-estudio.md)

---

## Índice

- [1. Descripción del caso](#1--descripción-del-caso)
- [2. Historias originales presentadas](#2--historias-originales-presentadas)
- [3. Análisis de problemas detectados](#3--análisis-de-problemas-detectados)
- [4. Reescritura corregida](#4--reescritura-corregida)
- [5. Resumen de correcciones](#5--resumen-de-correcciones)

---

## 1 — Descripción del caso

Una aplicación MAUI híbrida que, al navegar con un query parameter, toma la decisión de solicitar por REST API un JSON con datos para enviar a una impresora térmica de tickets desde el mismo MAUI.

**Flujo general:**
1. La app detecta un parámetro de navegación que indica impresión
2. Consulta los perfiles de impresora asociados al cliente
3. Obtiene el documento a imprimir desde un endpoint REST
4. Invoca la impresora térmica para generar el ticket

---

## 2 — Historias originales presentadas

### User Stories (tal como fueron redactadas)

| ID | User Story |
|---|---|
| US-01 | Como cliente MAUI, quiero consultar los perfiles asociados al cliente para luego usarlos en la invocación de la impresora |
| US-02 | Como cliente MAUI, quiero usar el queryparam Print=UrlEndpoint para conseguir por restapi consultando al urlendpoint el json con el documento para invocar a la impresora |
| US-03 | Como cliente MAUI, quiero con el perfil de impresora y el documento json a imprimir invocar a la impresora para imprimir o generar el ticket impreso |

### Escenarios presentados (sin asociar a US específica)

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Tengo varios perfiles de impresión | Se muestra un diálogo con las posibles impresoras a elegir y se da la opción de continuar con dicha elección |
| 2 | Al verificar la impresora, o fallo de impresora, o da error | Se muestra un diálogo que informe la causa del error y un botón para reintentar la impresión |

---

## 3 — Análisis de problemas detectados

### Problema 1 — El actor es un componente técnico, no un rol de usuario

"Como **cliente MAUI**" no es un rol/actor — es un componente técnico. La guía establece en el Paso 3:

> *Como [rol/actor], quiero [acción concreta], para [beneficio o razón].*

El actor debe ser **quien usa la aplicación**: un cajero, un operador, un vendedor, un empleado. La pregunta correcta es: *¿quién está parado frente a la pantalla queriendo imprimir un ticket?*

### Problema 2 — Las US mezclan implementación con necesidad del usuario

Las historias describen **cómo** se resuelve técnicamente (query params, REST API, JSON, endpoints) en lugar de **qué** necesita el usuario. Comparado con los ejemplos de la guía — ninguno menciona tecnología en el enunciado de la US:

> ❌ "quiero usar el queryparam Print=UrlEndpoint para conseguir por restapi..."  
> ✅ "quiero seleccionar un documento para imprimirlo como ticket"

Los detalles técnicos (query params, REST API, JSON) van en los **criterios de aceptación** o en notas técnicas de la US, no en el enunciado.

### Problema 3 — Los escenarios están incompletos y mezclados

La guía pide **al menos 3 escenarios por US** (happy path + edge cases). En la versión original hay 2 escenarios sueltos sin asociar a una US específica. Además falta el happy path completo y varios edge cases obvios.

### Problema 4 — Falta la estructura completa

Según la guía, cada US Must Have necesita sus propios criterios de aceptación con al menos 3 escenarios. Los escenarios presentados no están asociados a ninguna US en particular.

---

## 4 — Reescritura corregida

> Nota: se asume "operador" como actor. Debe ajustarse al rol real de quien opera la aplicación.

### User Stories corregidas

| ID | User Story | Prioridad |
|---|---|---|
| US-01 | Como operador, quiero ver los perfiles de impresora disponibles, para elegir con cuál imprimir | Must Have |
| US-02 | Como operador, quiero que al navegar con el parámetro de impresión el sistema obtenga el documento a imprimir, para no tener que cargar datos manualmente | Must Have |
| US-03 | Como operador, quiero imprimir el documento en la impresora seleccionada, para obtener el ticket impreso | Must Have |

### Criterios de aceptación — US-01 (Ver perfiles de impresora)

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Hay varios perfiles de impresora configurados | Se muestra un diálogo para elegir la impresora y confirmar la selección |
| 2 | Hay un solo perfil de impresora | Se selecciona automáticamente sin mostrar diálogo |
| 3 | No hay perfiles configurados | Se muestra error informando que no hay impresoras disponibles |

### Criterios de aceptación — US-02 (Obtener documento a imprimir)

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | El endpoint responde con un JSON válido | El documento queda listo para enviar a impresión |
| 2 | El endpoint no responde o devuelve error | Se muestra mensaje con la causa del error |
| 3 | El JSON recibido tiene formato inválido o está incompleto | Se informa el error de formato sin intentar imprimir |

### Criterios de aceptación — US-03 (Imprimir ticket)

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Impresora disponible y documento válido | Se imprime el ticket exitosamente y se confirma al usuario |
| 2 | La impresora no responde o falla | Se muestra diálogo con la causa del error y botón para reintentar |
| 3 | Se reintenta la impresión tras un fallo | Se vuelve a intentar la impresión con los mismos datos |

---

## 5 — Resumen de correcciones

| Problema | Sección de la guía que aplica |
|---|---|
| Actor técnico en vez de rol de usuario | Paso 3 — formato de User Story |
| Detalles de implementación en el enunciado | Paso 3 — "acción concreta" ≠ detalle técnico |
| Escenarios sin asociar a US específica | Paso 4 — DoR exige criterios por historia |
| Menos de 3 escenarios por historia | DoR — "al menos 3 escenarios" |
| Falta el happy path explícito | Ejemplo C DoR — "happy path + 2 edge cases" |

---

## Control de Cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-04-15 | Versión inicial — caso de estudio con análisis y corrección de US para impresión de tickets en MAUI |

---

**Fin del documento**
