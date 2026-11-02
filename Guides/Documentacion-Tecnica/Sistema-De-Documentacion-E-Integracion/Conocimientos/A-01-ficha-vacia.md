---
doc_id: ANX-SDEI-FICHA-VACIA
doc_type: anexo
title: Plantilla A-01 — ficha de diseño documental vacía
description: "Esqueleto vacío de la ficha A-01 que se entrega al aplicador del conocimiento de especificación."
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-02
audience: [humano, agente, desarrollo, analista, po]
traces: [ANX-SDEI-ESPECIFICACION]
---

# Ficha de diseño documental — {proyecto}

Si la ficha va al repo, lleva antes el frontmatter de §5.2 con `doc_type: ficha` y su `doc_id` (prefijo `FIC` u otro
libre, §5.6); este título no repite el ID, y los demás artefactos trazan a ese `doc_id`.

Numeración fija, la misma que usa la carta del aplicador: 1 Inventario, 2 Decisiones, 3 Catálogo activo, 4 Decisiones
de clase H para una persona, 5 Audiencias y entrega, 6 Artefactos A-02 a A-13, 7 Lo que el conocimiento no resolvió,
8 Verificación. No se agregan ni se renumeran secciones. Con envoltorio de comandos (`{envoltorio}`), cada comando de
§1 y §8 se copia tal como corrió, con el envoltorio (§3.7).

| Campo | Valor |
| --- | --- |
| Proyecto y repositorios leídos, con SHA (sin `.git`: «SHA: pendiente» y huella del árbol con `LC_ALL=C`, comando 11, §2.4) | |
| Carpetas excluidas o sin lectura (`{excluidas}`, nombres de carpeta a cualquier profundidad, separados por espacios) | |
| Fecha (salida de `date -u +%F`, comando 10) | |
| Quién aplica (el rol, no un nombre) | |
| Conocimiento aplicado (alias, versión, huella) | |

## 1. Inventario (paso 1)

Comandos corridos y salida recortada, o ruta al archivo con la salida. Un renglón por patrón de §3.1, aunque dé 0; el comando se copia
del bloque numerado de §3.1, no de su tabla.

| Patrón de §3.1 | Comando | Cuenta | Salida o ruta a la salida |
| --- | --- | --- | --- |
| ID declarado más de una vez | | | |
| Versión declarada dos veces | | | |
| Estado «planificado» o equivalente (y la proporción planificada, con ruta:línea) | | | |
| Traza de la interfaz a una unidad que no le corresponde | | | |
| Puntero a un documento obsoleto o ausente (sin CHANGELOG: «archivo ausente», cuenta 0 y su rc) | | | |
| Advertencia del índice del proyecto (cuenta y lista del comando 6; cada una pasa a reserva, con su estado) | | | |
| Espacios de IDs en uso, con su origen y sus choques con los Id de esta ficha (D-07) | | | |
| Plataformas que compila cada máquina (D-09) | | | |
| Prefijos nuevos libres (comando 9, uno por prefijo, `AUD` incluido) | | | |
| ¿El producto filtra por alcance? («sólo lo suyo» o inventario de alcances, con ruta:línea; o búsqueda negativa) | | | |
| Organización del runner: medida, o archivo que la nombra (evidencia secundaria declarada) | | | |

## 2. Decisiones

| Id | Pregunta | Valor | Evidencia (ruta:línea en el SHA leído, o «pendiente: qué falta») | Clase (P o H) | Si es H: opciones y default para la persona |
| --- | --- | --- | --- | --- | --- |
| D-01 | ¿Escenario y contextos? | | | | |
| D-01 · sitio | ¿Hay sitio publicado? | | | | |
| D-01 · marco | ¿Hay marco de especificación? | | | | |
| D-01 · topología | ¿Mismo repo, hermano o dedicado? | | | | |
| D-01 · organización | ¿Dentro o fuera de la organización del runner (medido)? | | | | |
| D-02 | ¿Qué clases de pieza se activan? | | | | |
| D-03 | ¿Hay sitio publicado? | | | | |
| D-04 | ¿Hay marco de especificación con registro de IDs? | | | | |
| D-05 | ¿Dónde vive cada pieza? | | | | |
| D-06 | ¿Contra qué ancla vale cada pieza? | | | | |
| D-07 | ¿Qué prefijos e IDs? | | | | |
| D-08 | ¿Qué esquema de frontmatter? | | | | |
| D-09 | ¿Cómo corre la compuerta? | | | | |
| D-10 | ¿Qué controles bloquean y cuáles avisan, en qué carpeta? | | | | |
| D-11 | ¿Quién decide qué? | | | | |
| D-12 | ¿Qué canales hay para agentes? | | | | |
| D-13 | ¿Cómo es la ayuda al usuario final? | | | | |
| D-14 | ¿Qué es privado y cómo se nombra? | | | | |
| D-15 | ¿Hay un cuerpo formativo? | | | | |
| D-16 | ¿Idioma y estilo? | | | | |
| D-17 | ¿Cómo es el carril programado? | | | | |
| D-18 | ¿Audiencias y entrega? | | | | |

## 3. Catálogo activo

| Clase | Activa (sí o no) | Alta o baja frente al escenario, con motivo | Dónde vive (todas las ubicaciones) | Ancla | Evidencia |
| --- | --- | --- | --- | --- | --- |
| CL-01 | | | | | |
| CL-02 | | | | | |
| CL-03 | | | | | |
| CL-04 | | | | | |
| CL-05 | | | | | |
| CL-06 | | | | | |
| CL-07 | | | | | |
| CL-08 | | | | | |
| CL-09 | | | | | |
| CL-10 | | | | | |
| CL-11 | | | | | |
| CL-12 | | | | | |
| CL-13 | | | | | |
| CL-14 | | | | | |

## 4. Decisiones de clase H para una persona

Las filas H de §2 (D-03, D-09, D-11, D-12 y las que su Clase vuelva H) más las acciones H de la tabla P/H del
proyecto que la adopción toca y la excepción que la adopción necesite agregar; nada más. **Obligatorias:** una fila por
cada API documentable (documento OpenAPI o grupo de endpoints) y una por cada punto de D-18 sin decidir (audiencia sin
acceso propio probado, `incluye:` sin evidencia, nivel sin punto de cumplimiento). La que el proyecto ya registra como
decidida por una persona va «decidida por {actor}», con su ruta:línea, conserva sus opciones y no se reabre; si cita un
acta que el árbol no tiene, «decidida por {actor} — citada, no versionada», con la ruta:línea de la cita. Las abiertas
llevan el default rotulado «propuesto».

| Id | Estado (abierta, «decidida por {actor}» con ruta:línea, o «decidida por {actor} — citada, no versionada») | Opciones | Default propuesto | Quién decide |
| --- | --- | --- | --- | --- |
| | | | | |

## 5. Audiencias y entrega (D-18)

Perfil (mínimo, o mínimo más extensiones) y extensiones activadas, cada una con la fila de §1 que la registra. Si D-01
no da roles: «sin audiencias declaradas». Se llena con las cinco reglas de §3.2.1, en orden; una audiencia con un punto
sin decidir lleva «pendiente» en ese punto, ningún nivel, y su fila en §4.

| Audiencia (fila de A-12) | Acceso propio (ruta:línea que la autentica) | Incluye (búsqueda de todas las políticas de la menor y ruta:línea de cada una, todas con la mayor; o «plana» con el punto que falta) | Nivel de acceso (`publico`, `producto`, `restringido`, `interno`) | Punto de aplicación y de cumplimiento (repositorio, build, servidor; ruta:línea) | Alcance (`por-aplicacion` con ruta:línea, o no activado) | Par de entrega (200 con contenido / distinto de 200 sin contenido) o «pendiente» |
| --- | --- | --- | --- | --- | --- | --- |
| | | | | | | |

Descartados (homónimos, agente, valores de `audience:` que sólo orientan), cada uno con su ruta:línea y motivo:

## 6. Artefactos A-02 a A-13

| Id | Producido (sí o no) | Ruta | Motivo si no se produce | Propuesta sujeta a (D-nn), si depende de una H abierta | «Sin probar», si falta la compuerta |
| --- | --- | --- | --- | --- | --- |
| A-02 | | | | | |
| A-03 | | | | | |
| A-04 | | | | | |
| A-05 | | | | | |
| A-06 | | | | | |
| A-07 | | | | | |
| A-08 | | | | | |
| A-09 | | | | | |
| A-10 | | | | | |
| A-11 | | | | | |
| A-12 | | | | | |
| A-13 | | | | | |

## 7. Lo que el conocimiento no resolvió

| Sección o decisión | Cita textual de lo que no resuelve | Qué quedó sin hacer |
| --- | --- | --- |
| | | |

## 8. Verificación (§6)

| Comprobación | Comando | Resultado (salida textual) |
| --- | --- | --- |
| Compuerta sobre los artefactos, con A-02 y A-03 (copia de §5.7 y su huella) | | |
| Par rojo/verde plantado, con la compuerta de D-09 | | |
| Autoprueba de la compuerta de origen (sin red ni caché: «pendiente», con el comando, §5.7) | | |
| Chequeo de citas ruta:línea (§3.5; cada cita repite el archivo, sin `:n` sueltas) | | |
| Muestra de cinco citas releídas contra lo que afirman (§3.5) | | |
| Barrido por `{lista-privada}` (§3.6; sin la lista, comando 12b y «lista: pendiente») | | |
