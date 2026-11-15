# Capa de IA integrada en .NET: la consola de chat como interfaz

**Alias:** AI-Layer-Chat-Console-Interface
**Naturaleza:** propio
**Tema:** Cómo se diseña y se construye la interfaz de la consola de chat de un asistente de producto: anatomía y componentes, estados visibles, sugerencias y destinos con acción, gráficos, interacción en escritorio y teléfono (lateral, hoja inferior, pantalla completa, teclado virtual, capas), accesibilidad (teclado, foco, ARIA, listbox), historial de conversaciones y dónde vive cada dato, aviso de cambios sin guardar, voto persistido sin texto, persistencia y observabilidad de conversaciones según la práctica de la industria, y una variante multi-participante para debates entre personas y agentes. Destilado de tres consolas construidas (PushDispatch, Geometría y Movilidad Urbana) como casos, no como norma
**Consumidor:** un agente IA (o una persona) que diseña, construye o revisa la interfaz de una consola de chat sobre un producto; quien revisa un PR que toca su anatomía, sus estados, su historial, su accesibilidad o su registro de conversaciones
**Condicion-de-carga:** al diseñar o construir la superficie de la consola, después de tener la especificación de K1 y el marco del turno de K5; al tocar su historial, su teclado, su versión de teléfono o la captura de conversaciones; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** AI-Layer-Integration-Designing-Specification (K1) para los requisitos y AI-Layer-Conversational-Design (K5) para el contenido de cada turno: este conocimiento no redefine qué se contesta, sólo cómo se muestra y se opera
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` 1.2.0 (libro de esta carpeta; las remisiones `[Guía §x]` usan sus anclas `cN-k`); `Conversational-Design-Framework-Guide.md` 1.6.0 (Guía DC); [K1](knowledge-AI-Layer-Integration-Designing-Specification.md) (requisitos); [K2](knowledge-AI-Layer-Console-Assistant-Building.md) (motor de la consola y reglas del panel §16); [K3](knowledge-AI-Layer-Verification-And-Acceptance.md) (capturas y aceptación); [K5](knowledge-AI-Layer-Conversational-Design.md) (formas F1–F8 y marco del turno); [K7](knowledge-AI-Layer-Testing-Strategy.md) (bUnit de estados y E2E)
**Versión:** 1.0
**Estado:** Borrador (pasa a Vigente cuando K3 acepte la interfaz aplicada sobre un segundo sistema con historial, teclado y teléfono probados; la variante multi-participante de §13 es propuesta rotulada, sin construir)
**Fecha:** 2026-11-15

---

## Índice

- [0. Propósito y mapa de carga](#k8-0)
- [1. Cuándo usarlo y cuándo no](#k8-1)
- [2. Reglas que no se negocian](#k8-2)
- [3. Anatomía y componentes](#k8-3)
- [4. Estados visibles](#k8-4)
- [5. Sugerencias, destinos con acción y gráficos](#k8-5)
- [6. Escritorio y teléfono: superficies, teclado virtual y capas](#k8-6)
- [7. Accesibilidad: teclado, foco y ARIA](#k8-7)
- [8. Historial: dónde vive cada dato, límites y borrado](#k8-8)
- [9. Aviso de cambios sin guardar](#k8-9)
- [10. Voto persistido sin texto](#k8-10)
- [11. Persistencia y observabilidad de conversaciones](#k8-11)
- [12. Procedimiento paso a paso](#k8-12)
- [13. Variante multi-participante para debates (propuesta rotulada)](#k8-13)
- [14. Checklist binario de completitud](#k8-14)
- [15. Antipatrones con su control](#k8-15)
- [16. Casos](#k8-16)
- [17. Límites](#k8-17)
- [Glosario](#k8-glosario)
- [Historial](#k8-historial)

<a id="k8-0"></a>
## 0. Propósito y mapa de carga

Este conocimiento dice **cómo se ve y cómo se opera** la consola de chat de un asistente de producto, para que un
agente que nunca vio las consolas de origen la construya en otro sistema sin tener que inventarla. **Qué**
contesta cada turno y con qué forma lo decide [K5](knowledge-AI-Layer-Conversational-Design.md); **cómo se
calcula** la respuesta (enrutado, corpus, destinos, votos, despliegue) lo dice
[K2](knowledge-AI-Layer-Console-Assistant-Building.md); **cómo se demuestra** que la interfaz funciona,
[K3](knowledge-AI-Layer-Verification-And-Acceptance.md) y [K7](knowledge-AI-Layer-Testing-Strategy.md). Acá
está la superficie: los componentes, sus estados, el teclado, el teléfono, el historial y el registro de las
conversaciones.

Es **general**: vale para un panel de control, una aplicación de estudio o un sitio de servicio. Los tres casos
de §16 muestran decisiones distintas sobre la misma regla; ninguna es la norma.

No repite lo que ya está escrito en otro lado; lo cita:

| Tema | Dónde está | Qué se toma de ahí |
|---|---|---|
| Formas de respuesta F1–F8, negativa útil, aclaración | [K5 §3](knowledge-AI-Layer-Conversational-Design.md#k5-3), [K5 §M](knowledge-AI-Layer-Conversational-Design.md#k5-m) | el contenido de cada turno; la interfaz lo dibuja |
| Chip de destino: verbo y objeto, abre la acción | [K5 §6](knowledge-AI-Layer-Conversational-Design.md#k5-6) | cómo se rotula y cuándo se ofrece |
| Sugerencias por pantalla y rol, voz | [K5 §7](knowledge-AI-Layer-Conversational-Design.md#k5-7) | qué sugerencias existen |
| Montaje único, estado en la URL, estilos enlazados | [K2 §16 E-1 a E-3](knowledge-AI-Layer-Console-Assistant-Building.md#k2-16) | cómo entra la consola al layout |
| Visualizaciones y su regla | [K2 §7](knowledge-AI-Layer-Console-Assistant-Building.md#k2-7), [K5 §4](knowledge-AI-Layer-Conversational-Design.md#k5-4) | qué gráfico; acá, cómo se dibuja |
| Votos sin texto | [K2 §8](knowledge-AI-Layer-Console-Assistant-Building.md#k2-8) | la tabla; acá, el gesto |
| IA apagada a la vista | [K2 §10](knowledge-AI-Layer-Console-Assistant-Building.md#k2-10) | el motivo; acá, cómo se muestra |
| Historial y streaming | [Guía §6.2](../Dot-Net-Integration-AI-Layer-Guide.md#c6-2) | el historial que viaja al modelo |
| Datos personales | [Guía §12.4](../Dot-Net-Integration-AI-Layer-Guide.md#c12-4) | minimización; acá, su excepción acotada (§11) |
| Pruebas de estados y E2E a 1280 y 390 | [K7 §4](knowledge-AI-Layer-Testing-Strategy.md#k7-4), [K7 §8](knowledge-AI-Layer-Testing-Strategy.md#k7-8) | cómo se prueba cada estado |
| Capturas lado a lado | [K3 §6](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-6-capturas) | la evidencia de la interfaz |

**Mapa de carga.** Cargá §0–§3 siempre; después sólo la sección del tema que tocás.

| Situación | Secciones |
|---|---|
| Arrancar la interfaz de una consola nueva | §0–§4, §12, §14 |
| Agregar o cambiar un estado (cargando, error, apagada) | §2, §4 |
| Tocar las sugerencias, el chip de destino o un gráfico | §5 y K5 §6–§7 |
| Hacer la versión de teléfono o arreglar capas | §6 |
| Arreglar el teclado, el foco o el lector de pantalla | §7 |
| Agregar historial de conversaciones | §8, §11 |
| La persona pierde lo que escribió al navegar | §9 |
| Agregar el voto | §10 y K2 §8 |
| Registrar o auditar conversaciones | §11 |
| Diseñar una sala de debate con varias personas y agentes | §13 (propuesta) |
| Revisar un PR de la interfaz | §14, §15 |

**Criterio de parada general.** Si para dibujar algo hace falta decidir qué contesta el asistente, no lo decidas
acá: es de K5. Si hace falta guardar texto de la conversación más allá de lo que §11 admite, es una decisión de
la especificación (K1) firmada por quien responde por la seguridad.

<a id="k8-1"></a>
## 1. Cuándo usarlo y cuándo no

**Usalo** cuando el asistente se muestra como una conversación (uno o varios turnos con pregunta y respuesta) dentro
de un producto: un panel lateral, una hoja, una página propia o una tarjeta en una pantalla.

**No lo uses** para:

- decidir la forma de cada respuesta ni si un turno aclara (K5);
- el pipeline del servidor (K2);
- un asistente sin interfaz propia (por ejemplo, expuesto sólo como servidor MCP a otro asistente): ahí rige el
  contrato de K1 §6.1 y no hay superficie que diseñar;
- un formulario que no conversa (una búsqueda con resultados): si no hay turnos, no es una consola.

Una tarjeta de **un solo turno** en una pantalla (como la de Geometría) también es una consola: le aplican §2–§5 y
§7; §8 y §9 no aplican mientras no haya varios turnos ni historial.

<a id="k8-2"></a>
## 2. Reglas que no se negocian

| Id | Regla (imperativo) | Porqué | Prueba |
|---|---|---|---|
| <a id="k8-r1"></a>CI-1 | **Un solo estado de conversación, varias superficies.** Si la consola se muestra como lateral, hoja, página o tarjeta, todas leen el mismo estado del hilo; cambiar de superficie no crea otra conversación ni navega | en PushDispatch, expandir el lateral al centro sin navegar fue la corrección de una refutación: navegar perdía el hilo y la pantalla de fondo | bUnit: abrir lateral, expandir, volver; el hilo y el id son los mismos |
| <a id="k8-r2"></a>CI-2 | **Cada estado visible existe y se distingue:** los diez estados de la tabla de [§4](#k8-4), única lista que rige (redactando sólo si hay streaming) | un estado que no existe se resuelve en silencio (un spinner eterno, una lista vacía) y la persona no sabe qué pasó | bUnit por estado de §4 ([K7 R4.1](knowledge-AI-Layer-Testing-Strategy.md#k7-4)) |
| <a id="k8-r3"></a>CI-3 | **La interfaz no inventa contenido ni rutas:** dibuja lo que el servidor devolvió; un enlace sólo se acepta si es relativo y viene del catálogo; el texto se arma con nodos, nunca como HTML crudo | una ruta o una etiqueta dentro del texto del modelo es la vía de inyección de la salida (K2 R5) | bUnit con un texto que trae `<a>` y una ruta absoluta → se muestran como texto y no hay destino |
| <a id="k8-r4"></a>CI-4 | **Todo se opera con teclado y se anuncia:** foco a un lugar definido después de cada acción, región viva para lo que llega, controles nativos con nombre | la consola es un diálogo que se actualiza solo; sin región viva, el lector de pantalla no se entera | E2E con teclado sólo, y la tabla de foco de §7 como casos |
| <a id="k8-r5"></a>CI-5 | **La consola no tapa la salida del producto:** el menú de sesión y la navegación principal siempre quedan alcanzables | en PushDispatch el panel abierto tapaba el menú de sesión: para cerrar sesión había que cerrar el chat ([Guía DC §14.3.1](../Conversational-Design-Framework-Guide.md#c14-3-1)) | E2E a 1280 y 390 con la consola abierta: el menú de sesión se abre y se usa |
| <a id="k8-r6"></a>CI-6 | **El almacenamiento de la pestaña guarda ids, nunca texto** (lo tecleado que vive sólo en el campo, §8, no cuenta). El texto de la conversación, si se guarda, vive en el servidor y se filtra por la persona; una conversación ajena es indistinguible de una inexistente | el almacenamiento del navegador es legible por cualquier script de la página y sobrevive al cierre de sesión en equipos compartidos | inspección del almacenamiento de la pestaña tras una conversación: sólo ids; pedir un id ajeno → 404 igual que uno inexistente |
| <a id="k8-r7"></a>CI-7 | **Historial para la persona ≠ trazas para el operador.** Son dos almacenes con dos lectores, dos retenciones y dos políticas; la traza del operador no lleva texto salvo captura encendida a propósito (§11) | mezclar los dos es lo que lleva a guardar todo «por si acaso» o a no poder dar historial | la especificación tiene una fila por almacén con lector, retención y contenido; la traza por omisión no tiene pregunta ni respuesta |
| <a id="k8-r8"></a>CI-8 | **Nada de lo provisional queda.** Lo que se muestra mientras se redacta (texto parcial) se reemplaza por la versión validada o por la plantilla; detener o cerrar cancela | un texto a medias que queda visible es una respuesta que nadie validó ([K2 R1](knowledge-AI-Layer-Console-Assistant-Building.md): el modelo sólo redacta sobre lo ya calculado y validado) | E2E: detener a mitad → queda la plantilla con su aviso, no el parcial |

<a id="k8-3"></a>
## 3. Anatomía y componentes

Una consola tiene seis piezas. Cada una es un componente con una responsabilidad; el nombre es libre, la
responsabilidad no.

| Pieza | Qué hace | Qué no hace |
|---|---|---|
| **Cabecera** | título; acciones como íconos con nombre accesible: nueva conversación, historial, cambiar de superficie (expandir o llevar al costado), cerrar; insignia de alcance si el asistente sólo lee («Solo lectura: consulta y lleva; no modifica nada») | no lleva texto de ayuda largo ni la pantalla actual (eso es el chip) |
| **Chip de contexto** | dice sobre qué pantalla o entidad se pregunta («Mensajes · Tienda»); se puede quitar y reponer («Usar esta pantalla») | nunca muestra filas, filtros ni ids; si el servidor no validó el contexto, no se muestra |
| **Lista del diálogo** | turnos en orden cronológico, lo nuevo abajo pegado al compositor; separadores de navegación («Fuiste a …»); turno pendiente | no reordena ni agrupa por tema |
| **Tarjeta de respuesta** | un turno: pregunta (burbuja) y respuesta con, en este orden, lo que bloquea, el contenido de la forma (K5 §3), datos (cifras, gráfico, tabla), acciones, procedencia plegada («Cómo se calculó», «Fuentes»), pie con copiar, voto, hora y rótulo de origen | no elige la forma; no muestra el texto provisional como definitivo (CI-8) |
| **Sugerencias** | píldoras que disparan una intención concreta, por pantalla y rol (K5 §7.4) | no se repiten encima del compositor si el estado vacío ya las muestra |
| **Compositor** | campo multilínea con etiqueta, contador o máximo, ayuda «Enter envía · Shift+Enter, otra línea», botón Enviar; chip de contexto dentro del borde | no tiene controles muertos: lo que no se puede usar (por ejemplo, redactar con IA si está apagada) no se dibuja |

**Reglas.**

1. **El rótulo de origen va en cada respuesta** («Respuesta fija», «Según la guía «X»», «Redactado con IA a partir
   de la ayuda: verificá»). *Porqué:* las tres consolas lo tienen; sin él la persona no sabe cuánto confiar.
   *Prueba:* cada forma F1–F8 en bUnit trae su rótulo.
2. **La forma de la respuesta queda en el DOM** como atributos de datos estables por forma; los nombres son libres
   (en los ejemplos, `data-forma` y `data-intencion`; PushDispatch usa `data-tarjeta`, `data-origen`,
   `data-bloqueo`). En una respuesta compuesta, cada parte lleva los suyos. *Porqué:* las E2E y las capturas se
   atan a la forma, no al texto. *Prueba:* selector por forma en la E2E.
3. **El formato del texto es un subconjunto cerrado** (negrita y código; listas si hacen falta) armado con nodos.
   *Porqué:* los asteriscos crudos se vieron en Movilidad Urbana ([K5-AP-7](knowledge-AI-Layer-Conversational-Design.md#k5-ap-7)).
   *Prueba:* captura sin `**` ni `#`.
4. **La procedencia se pliega, no se esconde:** herramienta, método, período, hora de lectura y fuentes en un
   `<details>`. *Porqué:* la persona experta la quiere; la novata no la necesita para leer. *Prueba:* cada F1 y F4
   trae su bloque plegado.
5. **Lo que bloquea va primero.** Si la respuesta detecta algo que impide la tarea (una falla de configuración), lo
   dice antes que el resto y con la acción de corregir como principal. *Prueba:* caso de batería con bloqueo → el
   primer elemento de la respuesta es el bloqueo.
6. **Los textos fijos de la interfaz son configurables** ([K2 §16 E-4](knowledge-AI-Layer-Console-Assistant-Building.md#k2-16)).

<a id="k8-3-1"></a>
### 3.1 Cómo se dibuja cada forma

Qué forma toca lo decide K5 §3; esta tabla dice cómo se ve y con qué rótulo de origen. Los rótulos son textos
fijos configurables; el valor esperado de la prueba de la regla 1 es la columna «Rótulo».

| Forma | Dibujo | Rótulo |
|---|---|---|
| F1 Cifra | cifra grande con su nota de métrica debajo; procedencia plegada; «Ir a» si hay destino | «Calculado con tus datos» |
| F2 Total con desglose | como F1, más una píldora «Ver por {entidad}» que dispara el desglose sin el modelo | «Calculado con tus datos» |
| F3 Aclaración | una línea («¿Cuál de estas?»), 2 a 4 píldoras con rótulo humano y su cifra distintiva; «y k más» es una píldora que abre la lista completa en la misma tarjeta | — (no es respuesta) |
| F4 Pasos desde la ayuda | lista numerada, cada paso con su cita; «Fuentes» plegado; «Ir a» | «Redactado con IA a partir de la ayuda: verificá» o, sin modelo, «Según la guía «X»» |
| F5 Abstención | texto fijo y dos salidas como píldoras («Probá con otras palabras», «Abrir la ayuda») | «Respuesta fija» |
| F6 Negativa útil | qué no y qué sí en dos líneas; la ficha del concepto como `<details>` cerrado («Qué es {concepto}»); píldoras de lo que sí | «Respuesta fija» |
| F7 Afuera | texto fijo y 2 o 3 píldoras de ejemplo | «Respuesta fija» |
| F7 derivación | texto fijo «Esto lo responde {quién}» con el canal (enlace externo del catálogo o teléfono) como acción principal; sin píldoras | «Respuesta fija» |
| F7 derivación urgente | banda de alerta (`role="alert"`, ícono y texto, no sólo color) al tope de la tarjeta con el canal de urgencia como primera acción; sin sugerencias, sin voto, sin copiar; se anuncia completa | «Respuesta fija» |
| F8 IA apagada | aviso con el motivo en la cabecera de la tarjeta y respuestas deterministas debajo | «Respuesta fija» |
| Compuesta | una tarjeta con una sección por intención, separadas por un divisor y cada una con su encabezado (la pregunta parcial) y su rótulo; un solo pie con voto | el de cada parte |
| Confirmación | ver regla 7 | — |

7. **Lo irreversible no se confirma en la consola.** El asistente no actúa (K2 §15): un destino a una acción
   irreversible (cancelar un turno, borrar) abre la pantalla de esa acción, y la confirmación explícita que pide K5
   §M.3 paso 5 es el diálogo de confirmación del producto en esa pantalla. La consola no dibuja «¿Confirmás?».
   *Porqué:* una confirmación en la consola sería una escritura sin construir. *Prueba:* bUnit: ningún componente
   de la consola tiene un control que escriba; E2E: el destino de cancelar llega al diálogo del producto.

<a id="k8-4"></a>
## 4. Estados visibles

| Estado | Qué se ve | Qué se anuncia | Qué no puede pasar |
|---|---|---|---|
| **Vacío** | encabezado con la pregunta que el asistente sabe contestar («¿Qué querés saber de tus aplicaciones?»), una bajada con el alcance y 3 o 4 sugerencias como tarjetas | nada (el encabezado tiene foco programático de respaldo) | un campo solo sin pista de qué preguntar |
| **Pensando** | la burbuja de la pregunta ya está; debajo, «Buscando…» con animación que respeta `prefers-reduced-motion`; el campo se vacía antes de esperar; se puede teclear, pero Enviar queda en `aria-disabled` hasta que llega la respuesta (no hay cola); a los 5 s sin respuesta, «Sigue buscando…»; al corte de espera del servidor (K2 §10.2) pasa a Error | «Buscando en la ayuda…» por región viva; el compositor con `aria-busy` | perder lo tecleado si la respuesta falla |
| **Redactando (si hay streaming)** | texto provisional marcado como tal y oculto al lector, con **Detener** arriba del texto (para que el botón no se corra); aviso de lentitud a los 5 s sin primer fragmento; tras Detener queda la plantilla con el aviso «Detuviste la redacción; esta es la respuesta fija.» | el aviso de lentitud y el final, no cada fragmento | que quede el parcial (CI-8) |
| **Respuesta** | la tarjeta de §3 | un resumen de una línea; si hay bloqueo, el bloqueo primero | anunciar el texto entero |
| **Error** | «No pudimos responder ahora. Tu texto quedó en el campo.»; sin destino ni voto | el mismo texto | ofrecer un destino que ninguna validación respaldó |
| **Sin permiso** | sobre una **entidad** ajena: igual que «no hay datos» (403 y 404 se ven igual) y el chip se quita; sobre una **acción** que el rol no tiene: la negativa útil F6 (K5 CD-3) | el mismo texto | insinuar que la entidad existe |
| **Aclaración** | opciones como píldoras con rótulos humanos (nunca identificadores técnicos); al elegir, la burbuja muestra la elección; teclear el rótulo también elige | «Elegí una opción» | aclarar lo que no cambia la respuesta (K5 §M.4) |
| **IA apagada** | la consola funciona con las respuestas deterministas; sólo desaparecen los controles que necesitan el modelo, y la cabecera dice el motivo exacto sin valores (K2 §10) | el motivo una vez | apagado en silencio o con un control muerto |
| **Consola apagada** | no hay rastro: ni botón ni panel | — | un botón que abre un panel vacío |
| **Conversación no disponible** | «Esta conversación ya no está disponible.» con «Empezar una nueva» | el mismo texto | un hilo vacío sin explicación |

*Prueba:* una prueba bUnit por fila ([K7 R4.1](knowledge-AI-Layer-Testing-Strategy.md#k7-4)) y una captura por
fila a 1280 y 390 en la entrega (K3 §6).

**Cuándo hay streaming.** Por omisión **no**: la respuesta se muestra entera después de la validación del servidor
(K2 §3, paso 7), y el estado «Redactando» no existe. Se admite sólo si la especificación lo pide con motivo
(respuestas largas con espera medida mayor a 5 s) y **nunca** en un contexto sensible (K1 CTX-SENS): ahí un texto
provisional no validado no se muestra. Con streaming, el texto provisional se reemplaza por el validado o por la
plantilla (CI-8).

**Compositor.** Enviar queda en `aria-disabled` con el campo vacío o sólo espacios, mientras se espera una respuesta
y si el texto supera el máximo. El campo no corta lo tecleado: el contador pasa a «{n} de más» y Enviar no actúa
hasta recortarlo.

<a id="k8-5"></a>
## 5. Sugerencias, destinos con acción y gráficos

**Sugerencias.** Cada píldora dispara una intención concreta que no necesita el modelo, filtrada por pantalla y rol
(K5 §7.4). En el estado vacío son las tarjetas; después de una respuesta, los seguimientos van en la fila de
píldoras de esa respuesta, no en una barra fija.

**Destinos con acción.** El contenido del chip es de K5 §6 (verbo y objeto, abre la acción, permiso de hacer,
pertinencia, rótulo igual al de la página). La interfaz agrega:

1. **Una sola píldora principal «Ir a …» por respuesta**; las acciones con parámetros (abrir el alta, aplicar un
   filtro) van como tarjeta de acción con ícono, título, una línea y **Abrir**. *Prueba:* bUnit con dos destinos →
   uno principal.
2. **Al navegar, la conversación acompaña** o se cierra con una forma clara de volver: en escritorio el lateral
   sigue abierto con un separador «Fuiste a {pantalla}»; en teléfono el panel se cierra y queda «Volver a la
   conversación (n)». *Porqué:* las dos decisiones aparecen en los casos (PushDispatch acompaña, Movilidad cierra);
   lo que no se admite es perder el hilo. *Prueba:* E2E «Ir a» → en la pantalla destino, la conversación se reabre
   con el mismo id.
3. **Ctrl, Cmd o Shift abren otra pestaña** con la misma conversación por parámetro de URL; el clic nativo no se
   intercepta en ese caso. *Prueba:* E2E con modificador → pestaña nueva con el id en la URL.
4. **La pantalla destino dice que vino del asistente** (una ficha «desde el asistente» con el filtro aplicado) y
   descarta con aviso un filtro que no vale ([K2 §16 E-2](knowledge-AI-Layer-Console-Assistant-Building.md#k2-16)).

**Gráficos.** Qué gráfico corresponde lo decide el código (K5 §4, K2 §7); cómo se dibuja:

1. **Escala honesta:** base cero; si se recorta, una nota visible («la escala no arranca en cero») y una marca de
   corte. *Prueba:* caso con valores cercanos → nota presente.
2. **El máximo se marca con texto, no sólo con color** («máx.»).
3. **Tamaño fijo legible** (texto de 12 px como mínimo); si no entra, desplazamiento horizontal del gráfico, nunca
   letra más chica.
4. **La lista de valores existe en texto** (oculta a la vista, enlazada por `aria-labelledby`) y la tabla acompaña
   plegada («Ver datos»). *Prueba:* el lector de pantalla obtiene los valores sin el SVG.
5. **Pie de fuente con hora de cálculo** («calculadas hace 3 min»).

<a id="k8-6"></a>
## 6. Escritorio y teléfono: superficies, teclado virtual y capas

| Ancho | Superficie recomendada | Detalle |
|---|---|---|
| ≥ 992 px | **Lateral** de 360 a 400 px | empuja el contenido o lo cubre; si cubre, el contenido de fondo sigue usable y el menú de sesión gana (CI-5) |
| 576–991 px | **Hoja inferior** | alto inicial cercano a 60 % de la ventana dinámica (`dvh`), máximo 85 %; radios arriba; se cierra con su botón y con Escape |
| < 576 px | **Pantalla completa** | ocupa la ventana; respeta `env(safe-area-inset-bottom)`; al navegar por un destino se cierra y deja «Volver a la conversación» |
| cualquiera | **Página propia** | la misma conversación en el centro, con «Volver a {pantalla}»; la pantalla de fondo queda montada e inerte (`inert`), no se navega |

**Reglas.**

1. **Medí con la ventana dinámica (`dvh`), no con `vh`, y tratá el teclado aparte.** *Porqué:* `dvh` evita el
   salto por las barras del navegador, pero no sigue al teclado virtual: en iOS Safari el teclado no cambia la
   ventana de layout, y en Chrome para Android tampoco por omisión desde la versión 108. Para el teclado se usa
   `visualViewport` (alto visible) o `interactive-widget=resizes-content` en el meta viewport, y se prueba en el
   dispositivo. *Prueba:* E2E a 390 con el campo enfocado: el
   compositor es visible (en emulación) y captura en un teléfono real en la entrega.
2. **Las capas son tokens, no números sueltos:** un token por capa (contenido, encabezado, menú de sesión, consola,
   diálogo modal, aviso) con un orden escrito; el menú de sesión y los diálogos del producto quedan por encima de
   la consola. *Porqué:* en PushDispatch el menú de sesión quedó debajo del panel porque vivía dentro de un
   encabezado con su propio contexto de apilamiento; el arreglo con tokens quedó planificado pero no estaba en la
   rama principal al relevarla. *Prueba:* E2E con la consola abierta → el menú de sesión responde al clic.
**Hoja inferior.** Es un diálogo no modal como el lateral: el fondo sigue visible pero no se desplaza mientras el
dedo está en la hoja (`overscroll-behavior: contain`); no se arrastra para redimensionar ni para cerrar (se cierra
con su botón o con Escape); su cabecera deja libre el menú de sesión (CI-5). *Prueba:* E2E y captura **a 768 px**
además de 1280 y 390 cuando la consola tiene hoja inferior: la hoja se abre, el fondo no se desplaza y el menú de
sesión responde.

3. **El historial en el teléfono es su propia hoja o una vista dentro del panel**, no un menú flotante que se
   salga de la pantalla. *Prueba:* captura a 390 con el historial abierto.
4. **Áreas de toque de 44 px como mínimo** en píldoras, «Ir a» e ítems del historial por debajo de 576 px.
5. **Al expandir a página, el fondo queda montado e inerte** (no navegar): volver no recarga la pantalla. *Prueba:*
   bUnit o E2E: expandir y volver conservan el estado de la pantalla de fondo.

<a id="k8-7"></a>
## 7. Accesibilidad: teclado, foco y ARIA

**Roles y nombres.**

- El panel lateral es `role="dialog"` **no modal** con nombre (el título); en pantalla completa puede ser modal
  (`aria-modal="true"`) con el foco contenido. La página propia no es diálogo.
- El botón que abre la consola lleva `aria-expanded` y `aria-controls`.
- La lista del diálogo es una `<section>` con encabezado (puede estar oculto) y una lista ordenada; cada respuesta
  es un `<article>` nombrado por su pregunta, con `aria-busy` mientras se arma.
- **Una región viva por superficie** (`role="status"`, `aria-live="polite"`), oculta, que anuncia el resumen de lo
  que llegó; nunca el texto entero ni cada fragmento del streaming.
- Botón Enviar deshabilitado con `aria-disabled` (se anuncia y no actúa) en lugar de `disabled` (desaparece del
  recorrido de tabulación).
- Votos con `aria-pressed`; «Copiar» cambia su nombre a «Copiado», y si no hay portapapeles no se dibuja.
- Grupos de píldoras con `role="group"` y nombre («Sugerencias», «Probá con», «Opciones para aclarar»).

**Teclado.**

| Tecla | Dónde | Efecto |
|---|---|---|
| Enter | compositor | envía (respetando la composición de caracteres: `isComposing`) |
| Shift+Enter | compositor | nueva línea |
| Escape | panel o hoja | cierra, salvo que haya un desplegable abierto (el historial), que se cierra primero |
| ↑ ↓ Inicio Fin | historial | mueve la opción activa |
| Enter | historial | retoma la conversación |
| Supr | historial | borra la conversación activa (con anuncio) |
| Escape | página propia | vuelve a la superficie anterior, como «Volver a {pantalla}» |
| Escape | fuera de la consola (lateral abierto) | no actúa sobre la consola |

**El historial es un listbox** con `aria-activedescendant` (el foco queda en la lista y la opción activa se
anuncia), siguiendo el patrón *Listbox* de la guía de prácticas de ARIA del W3C
(<https://www.w3.org/WAI/ARIA/apg/patterns/listbox/>, consultado 2026-11-15). Escape cierra el historial y detiene
la propagación para no cerrar también el panel.

**Borrar en el teléfono.** Sin teclado físico, el historial se muestra como una lista de botones (no listbox):
cada ítem es un botón que retoma y, a su lado, un botón «Borrar {título}» de 44 px. Borrar una conversación no
pide confirmación y deja «Conversación borrada · Deshacer» durante 10 s, anunciado; «Borrar todo» pide
confirmación con un diálogo («Se borran {n} conversaciones. No se puede deshacer.»). Las mismas reglas valen para
Supr en escritorio. *Prueba:* E2E a 390: borrar y deshacer; borrar todo con su diálogo.

**Foco por acción.** El foco nunca cae a `body`.

| Acción | Adónde va el foco |
|---|---|
| Abrir la consola | el compositor (o el encabezado del vacío si no hay compositor) |
| Expandir a página | el compositor; el encabezado de la página queda de respaldo |
| Volver al costado | el título del lateral (el botón que se tocó deja de existir) |
| Cerrar | el botón que la abrió, después del render; si no existe, el contenido principal |
| Llega una respuesta | no se mueve; se anuncia (en una tarjeta de un solo turno, ir a la respuesta es válido) |
| Error | se queda en el campo, con el texto repuesto |
| Cerrar el historial | su botón |
| «Ir a» en el teléfono | el encabezado de la pantalla destino |

**Foco visible:** anillo de 2 px con contraste ≥ 3:1 en todos los controles de la consola. **Contraste de texto**
≥ 4,5:1, también en la burbuja de la pregunta.

*Prueba:* E2E con sólo teclado que recorre la tabla de foco; una auditoría automática de accesibilidad sobre cada
estado de §4 sin errores; lectura con un lector de pantalla real en la entrega (conducta, no tasa).

<a id="k8-7-1"></a>
**Cómo se acepta la accesibilidad.** K3 lo recibe así, como parte del informe de la interfaz:

| Control | Herramienta | Aprueba si |
|---|---|---|
| Auditoría automática | axe-core desde la E2E de Playwright (`@axe-core/playwright`), reglas WCAG 2.1 A y AA, en cada estado de §4 y en 1280, 768 (si hay hoja) y 390 | cero violaciones de impacto *serious* o *critical*; las *moderate* se listan con su motivo |
| Teclado | la E2E sólo con teclado de la tabla de foco | ningún paso deja el foco en `body` |
| Lector de pantalla | NVDA con Firefox o Chrome en escritorio, y VoiceOver con Safari en el teléfono | el guion de seis pasos (abrir, preguntar, oír el resumen, elegir una aclaración, abrir el historial y retomar, cerrar y volver al botón) da lo esperado en cada paso |
| Teléfono real | un Android y un iPhone | compositor visible con el teclado abierto y menú de sesión alcanzable |

La evidencia es una tabla en el informe: paso, lo que se oyó o vio, lo esperado y sí/no, con la versión de cada
herramienta. Un «no» impide aceptar.

<a id="k8-8"></a>
## 8. Historial: dónde vive cada dato, límites y borrado

| Dato | Dónde vive | Por qué |
|---|---|---|
| Texto de los turnos | **servidor**, por (persona, conversación); **efímero** (en memoria, con vencimiento; se borra al cerrar sesión) o **durable** (en una tabla, con firma, §11.6: sobrevive al cierre de sesión, sigue a la persona entre dispositivos y se borra por retención o a pedido) | se filtra por persona |
| Id de la conversación actual | **pestaña** (almacenamiento de sesión, cifrado o firmado si el marco lo permite) y, para compartir entre pestañas, la URL (`?consola={id}`); el id es aleatorio de 128 bits o más, no adivinable, y en la URL no da acceso por sí solo: sin la sesión de su dueña da 404 | sobrevive a la navegación sin guardar texto en el navegador (CI-6) |
| Superficie abierta (cerrado, lateral, página) | pestaña | comodidad; al recargar puede arrancar cerrada |
| Lo tecleado sin enviar | **sólo el campo** del navegador; el servidor no lo pisa salvo cuando cambia desde afuera (vaciar al enviar, reponer tras error, nueva conversación) | un render atrasado del servidor borraba lo que la persona seguía tecleando |

**Reglas.**

1. **Límites escritos:** vencimiento por inactividad (en PushDispatch, 2 h sin uso y 8 h como máximo desde que se
   creó), máximo de conversaciones por persona
   (5) y de turnos por conversación (30), largo del título (60 caracteres, la primera pregunta recortada). Son
   puntos de partida medidos en un caso, no constantes. Al llegar al máximo de turnos, el compositor dice «Esta
   conversación llegó a su límite» con «Empezar una nueva»; al superar el máximo de conversaciones se descarta la
   más vieja sin aviso, y la lista lo dice en su leyenda. *Prueba:* unitarias de cada límite.
2. **Lo ajeno no existe:** pedir una conversación de otra persona devuelve lo mismo que una inexistente. *Prueba:*
   404 igual en los dos casos.
3. **Al cambiar de usuario o cerrar sesión se olvida todo** lo de la pestaña y el servidor descarta lo de esa
   persona si el almacén es efímero.
4. **Si la conversación venció**, el turno nuevo se guarda en una nueva y la pestaña la adopta sin error visible.
5. **Prioridad al iniciar:** primero el id de la URL, después el de la pestaña.
6. **Nueva conversación:** la actual queda en el historial; se repone el chip de contexto; el foco va al campo.
7. **Borrar** una conversación y «Borrar todo» existen y se anuncian (§7, «Borrar en el teléfono»); la lista dice
   cuánto dura lo guardado según el almacén (en el caso efímero: «Las conversaciones se guardan hasta 2 h sin uso y
   se borran al cerrar sesión»; en uno durable: «Las conversaciones se guardan {retención} y podés borrarlas») y tiene su vacío («No hay otras
   conversaciones»). *Porqué:* la persona tiene que saber qué se guarda para confiar.
8. **Cada ítem del historial** muestra la primera pregunta recortada, la hora y el contexto (la pantalla o
   entidad). Con datos sensibles (K1 CTX-SENS), ni el título ni el chip muestran la pregunta ni el nombre de una
   persona: el título es la intención del catálogo y la hora («Turnos · 10:42»), y el chip dice la pantalla.
9. **Lo que viaja al modelo es otra cosa:** cuántos turnos previos se envían como contexto lo decide el servidor
   ([Guía §6.2](../Dot-Net-Integration-AI-Layer-Guide.md#c6-2)); mostrar 30 turnos no obliga a mandarlos.
10. **Retomar una conversación vieja** muestra sus turnos tal como fueron, con una línea «Conversación del
    {fecha}: las cifras son de ese momento»; los chips de destino viejos se dibujan inactivos; el chip de contexto
    es el de la pantalla actual.

<a id="k8-9"></a>
## 9. Aviso de cambios sin guardar

Un destino de la consola navega. Si la persona estaba editando algo largo en la pantalla de fondo, o tenía un
mensaje a medias en el compositor, navegar lo pierde.

1. **Aplicá el aviso a los «editores largos»**: texto multilínea, ocho campos o más, o algo que rehacer lleva más de
   un minuto. Un campo cuenta como pendiente cuando recibió `input` y no volvió al valor que tenía al enfocarlo.
   *Porqué:* avisar en todo formulario entrena a ignorar el aviso. *Prueba:* lista escrita de pantallas cubiertas y
   bUnit o E2E por cada una.
2. **Con algo pendiente, el aviso del navegador al recargar** (`beforeunload`) y **un diálogo propio al navegar**
   dentro de la aplicación: «Si salís ahora, se pierde lo que escribiste y no guardaste. ¿Salir igual?», con
   «Seguir editando» como opción principal y «Salir sin guardar» como secundaria de peligro (contorno, contraste
   ≥ 4,5:1).
3. **El chip de destino respeta el aviso**: navegar desde la consola pasa por la misma guarda que cualquier enlace.
   *Prueba:* E2E: editar, tocar «Ir a» → aparece el diálogo.
4. **El compositor de la consola es un editor**: si la persona escribió y no envió, decidí y escribí una de dos
   cosas: o el borrador se conserva en la memoria de la página mientras no se recargue (nunca en el almacenamiento
   de la pestaña ni en el servidor, CI-6), o el compositor entra en la guarda. Con datos sensibles, la guarda. *Porqué:* en el caso de origen el compositor no estaba cubierto y lo tecleado sin enviar se
   perdía al recargar sin aviso; es coherente con no guardar borradores, pero tiene que ser una decisión escrita.
5. **Al expirar la sesión** lo editado se pierde; si se quiere evitar, el autoguardado es otra decisión con su
   propia especificación.

<a id="k8-10"></a>
## 10. Voto persistido sin texto

El almacén y la métrica son de [K2 §8](knowledge-AI-Layer-Console-Assistant-Building.md#k2-8); la interfaz:

1. **Útil / No útil** en el pie de cada respuesta votable; después de «No útil», los motivos cerrados (K5 §7.6) como
   píldoras; luego «Gracias por tu voto». *Prueba:* bUnit del recorrido completo.
2. **No se vota un error** ni una respuesta interrumpida.
3. **La interfaz dice qué se guarda:** «Tu voto quedó registrado, sin tu pregunta ni tu respuesta.» *Porqué:* así lo
   dice Movilidad Urbana y es lo que permite votar sin miedo.
4. **El voto se persiste en una tabla, no en el log.** Un voto que sólo va a un registro de texto no se mide
   ([K2-AP-6](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-6)); en el caso de origen el gesto existía y
   el voto sólo iba al log, que es justo el antipatrón.
5. **El voto es definitivo.** La fila de K2 §8 no guarda quién ni el turno, así que no se cambia ni se retira:
   después de votar, los botones quedan con `aria-pressed="true"` en el elegido y `aria-disabled` en ambos. Al
   retomar desde el historial, el estado del voto sale del estado de la conversación en el servidor (que sí conoce
   el turno), nunca de la tabla de votos. *Prueba:* bUnit: votar dos veces deja una fila; retomar muestra el voto.

<a id="k8-11"></a>
## 11. Persistencia y observabilidad de conversaciones

La práctica de la industria separa dos cosas que la consola suele confundir:

| | Historial para la persona | Trazas para el operador |
|---|---|---|
| Para qué | retomar, releer, borrar | medir, depurar, auditar |
| Quién lee | la persona dueña, sólo ella | quien opera el sistema, con permiso propio |
| Contenido | el texto de sus turnos | atributos cerrados: intención, forma, anillo, herramienta, modelo, tokens, latencia, resultado, respaldo, correlación; **sin texto por omisión** |
| Retención | la que la persona ve escrita (§8.7) | acotada y escrita; con contenido, la más corta (§11.5) |
| Borrado | la persona lo borra | por vencimiento; a pedido de la persona si la traza tuviera contenido |

**Reglas.**

<a id="k8-11-1"></a>
1. **Instrumentá con las convenciones semánticas de IA generativa de OpenTelemetry** (atributos `gen_ai.*`:
   operación, proveedor, modelo pedido y respondido, tokens de entrada y salida, motivo de fin)
   (<https://opentelemetry.io/docs/specs/semconv/gen-ai/>, estado *Development*, consultado 2026-11-15). Esas
   convenciones tratan la **captura del contenido** (mensajes de entrada y salida) como **optativa y apagada por
   omisión**, porque puede llevar datos personales. *Porqué:* un esquema común deja cambiar de herramienta de
   observación sin reescribir. *Prueba:* un span de una consulta trae los atributos y no trae el texto.
2. **Identidad de la persona en el span, no en las métricas** (las métricas se agregan; un id por persona explota
   la cardinalidad y expone a la persona). Es un **atributo propio**, fuera de `gen_ai.*`, igual que los de la
   consola: usá un espacio de nombres del sistema (por ejemplo `app.consola.intencion`, `.forma`, `.anillo`,
   `.respaldo`, `.resultado`) con valores cerrados del catálogo. La identidad va como **seudónimo**: un id opaco
   derivado con una clave del operador (HMAC), nunca nombre ni correo. Con datos sensibles, la intención va con su
   id genérico del catálogo y sin la entidad. Las trazas las lee sólo el rol operador, y su acceso queda
   registrado.
3. **La captura de contenido se enciende sólo en desarrollo o en pruebas con datos sembrados.** Rige
   [K1 REQ-IA-09](knowledge-AI-Layer-Integration-Designing-Specification.md): fuera de desarrollo los spans no
   traen mensajes. Capturar en producción para una investigación es un cambio de la especificación (REQ-IA-09)
   firmado por quien responde por la seguridad, no una opción de esta interfaz. *Prueba:* la clave que la enciende
   está en la plantilla de configuración con valor apagado y atada al entorno (en PushDispatch, a
   `IsDevelopment()`).
4. **Enmascará antes de guardar**, no al leer: correos, teléfonos, documentos, claves y tokens se reemplazan por
   marcas en el colector o en el decorador del cliente. *Prueba:* una pregunta sembrada con un correo y una clave
   → la traza guardada no los tiene.
5. **Retención acotada y automática** (criterio de este conocimiento: entre 30 y 90 días para trazas con contenido; la de los atributos cerrados
   puede ser mayor si no identifican a la persona). *Prueba:* tarea de purga con su prueba sobre datos vencidos.
6. **El historial durable de la persona es una excepción explícita** a la minimización de
   [Guía §12.4](../Dot-Net-Integration-AI-Layer-Guide.md#c12-4) y no toca la regla del voto: el voto sigue sin
   texto (K2 §8); el historial es otro almacén, con otro lector (la persona), su retención escrita y su borrado. Si
   la especificación no lo pide y no lo firma quien responde por la seguridad, el historial queda efímero (en
   memoria, con vencimiento). *Prueba:* la especificación tiene la fila del almacén con firma; sin firma, no hay
   tabla de conversaciones. El historial durable trae además su retención (criterio de este conocimiento: 90 días
   sin uso), la exportación a pedido y el borrado al dar de baja a la persona.
7. **Las herramientas de observación de aplicaciones con modelos** (por ejemplo Langfuse, LangSmith
   <https://docs.langchain.com/langsmith>, consultado 2026-11-15, o Phoenix) sirven de referencia para trazas por
   turno, evaluaciones y conjuntos de datos; su uso no cambia las reglas 1 a 6: el contenido sigue apagado por
   omisión y enmascarado si se enciende. *Porqué:* esas herramientas reciben lo que se les manda; la política es
   del sistema, no de la herramienta.
8. **El consumo es una traza más, sin texto:** una fila por llamada con función, proveedor, modelo, tokens, costo
   estimado, latencia y resultado; una falla del registro no corta la respuesta
   ([K1 §6.1](knowledge-AI-Layer-Integration-Designing-Specification.md#k1-6-1)).
<a id="k8-11-9"></a>
9. **El texto libre de la propia persona** (lo que escribe en el compositor) se trata como dato sensible cuando el
   dominio lo es (K1 CTX-SENS, F-8): el historial queda efímero salvo firma; antes de viajar al modelo, la pregunta
   y los turnos previos pasan por la misma proyección y enmascarado que los datos de terceros (F-8), y nunca llega a
   las trazas. *Prueba:* pregunta sembrada con un síntoma, un documento y un correo → el modelo recibe las marcas,
   la traza no tiene texto y, sin firma, no hay fila durable.

<a id="k8-12"></a>
## 12. Procedimiento paso a paso

Cada paso termina con su artefacto; sin artefacto el paso no está hecho.

1. **Leé K1, K5 y K2 §16.** Artefacto: lista de formas F1–F8 que la consola tiene que dibujar, roles y superficies
   del producto. Parada: si falta la especificación, volvé a K1.
2. **Elegí las superficies por ancho** (§6). Artefacto: tabla ancho → superficie, con la decisión de acompañar o
   cerrar al navegar.
3. **Maqueta con los estados de §4** a 1280 y 390 (y 768 si hay hoja inferior), en claro y oscuro. Artefacto: una captura por estado.
4. **Componentes de §3** con un solo estado compartido (CI-1). Artefacto: bUnit de montaje único y de cambio de
   superficie.
5. **Estados** (§4) con su prueba por estado (K7 §4).
6. **Teclado, foco y ARIA** (§7). Artefacto: E2E sólo con teclado y auditoría automática sin errores.
7. **Capas como tokens** (§6.2). Artefacto: E2E del menú de sesión con la consola abierta.
8. **Historial** (§8) si hay varios turnos. Artefacto: tabla «dato → dónde vive», unitarias de límites y 404 de
   lo ajeno.
9. **Guarda de cambios sin guardar** (§9). Artefacto: lista de editores largos y decisión escrita sobre el
   compositor.
10. **Voto** (§10) sobre la tabla de K2 §8.
11. **Trazas** (§11). Artefacto: span de ejemplo sin texto; clave de captura apagada en la plantilla; prueba de
    enmascarado y de purga.
12. **Entregá a K3:** capturas por estado a 1280 y 390 en claro y oscuro y el sitio real por rol. No declares
    cerrado antes del informe.

**Criterios de parada.** Si un estado no se puede provocar en una prueba, no está construido: volvé al paso 5. Si
el historial necesita texto durable y no hay firma, quedate en el historial efímero.

<a id="k8-13"></a>
## 13. Variante multi-participante para debates (propuesta rotulada)

> **Propuesta, no probada.** Ninguno de los tres casos la construyó. Se escribe como punto de partida para una
> plataforma de debates donde conversan varias personas y varios agentes; cada regla trae su prueba para que el
> primer sistema que la construya la confirme o la corrija. Hasta entonces no se cita como norma.

En una consola de una persona, el turno es «pregunta → respuesta». En una sala hay **varios autores** y la
respuesta no es obligatoria.

| Pieza | Propuesta | Prueba |
|---|---|---|
| **Turno con autor** | cada mensaje lleva autor (persona o agente), rol en la sala y hora; un agente se distingue a simple vista y en el lector (insignia «agente» y nombre accesible) | bUnit: un mensaje de agente se anuncia como tal |
| **Quién responde** | un agente sólo habla si lo **mencionan** (`@agente`), si le toca por un turno asignado por la moderación o si una regla escrita lo dispara; nunca porque «hay silencio» | prueba de la regla de disparo: sin mención ni turno, ningún agente publica |
| **Hilos** | una respuesta puede abrir un hilo bajo un mensaje; la sala muestra el último mensaje del hilo y su conteo | E2E: responder en hilo no mueve la sala |
| **Menciones** | autocompletado accesible (listbox, §7) sólo con participantes de la sala | bUnit: no se puede mencionar a quien no está |
| **Moderación** | un rol modera: fija el tema, da la palabra, oculta un mensaje con motivo visible y cierra la sala | prueba por rol: un participante no modera |
| **Frontera por agente** | cada agente tiene su frontera (K1 §3.1) y su rótulo de origen; un agente no responde por otro | caso de batería por agente |
| **Registro de deliberación** | el acta de la sala es otro almacén: quién dijo qué, qué se decidió y con qué votos; su retención y su lector se escriben como en §11, con aviso a los participantes al entrar | la sala muestra el aviso; el acta tiene retención escrita |
| **Votos de la sala** | si la sala decide, el voto es por participante y visible; distinto del voto de calidad de §10, que sigue sin texto | prueba: un voto de calidad no aparece en el acta |
| **Accesibilidad** | región viva que anuncia autor y resumen, con un límite de frecuencia (una sala activa no puede anunciar cada mensaje) | E2E con lector: ráfaga de mensajes → un anuncio resumido |

Lo que sigue igual: los destinos salen del catálogo y la captura de contenido para el operador sigue apagada fuera
de desarrollo (§11.3).

<a id="k8-13-1"></a>
### 13.1 Decisiones de la propuesta

Para que la propuesta se pueda construir sin inventar, estas decisiones quedan escritas. Siguen siendo propuesta:
el primer sistema que las construya las confirma o las corrige.

1. **Formas de sala.** Un agente debatiente no responde con F1–F8: publica una de cinco formas de sala, elegidas
   por código según lo que pidió la mención o la regla de disparo. Las formas son **S1 posición** (tesis y
   fundamentos, con fuentes citadas si las usa), **S2 réplica** (cita el mensaje al que responde), **S3 resumen**
   (de un hilo o de la sala, con enlaces a los mensajes resumidos), **S4 pregunta a la sala** y **S5 dato**. S5 es
   una F1 o una F4 de K5, con su anclaje y su validación. Cada mensaje de agente lleva el rótulo «Opinión de
   {agente}, con IA» (S1–S4) o el de su forma K5 (S5). Lo que no es S5 no afirma cifras: el validador de K2 R3 lo
   rechaza. *Prueba:* bUnit por forma; una S1 con un número sin herramienta no se publica.
2. **Alcance del agente.** El agente lee con el **alcance mínimo común** de los participantes presentes: sólo lo
   que todos pueden ver. Nunca usa el de quien lo menciona, porque lo que publica lo leen todos. *Prueba:* sala con
   dos roles; el agente mencionado por el de más permiso no muestra lo que el otro no ve.
3. **Qué viaja al modelo.** Los mensajes de otros participantes son **texto de terceros no confiable** (K1 §10,
   F-8): viajan delimitados como datos, nunca como instrucciones. Viajan sólo los visibles y no ocultos del hilo en
   curso, hasta los últimos 10 (punto de partida), con el mismo enmascarado de §11.9. *Prueba:* un mensaje con una
   instrucción inyectada no cambia la forma ni el destino del agente.
4. **Disparo y topes.** Una regla de disparo se escribe `cuando {evento de sala} → {agente} publica {forma}`
   (por ejemplo, «cuando la moderación cierra un hilo → el relator publica S3»). Un agente **no menciona** a otro
   agente. Hay un tope de 3 mensajes de agente por cada mensaje humano en la sala, un presupuesto de consumo por
   sala (§11.8) y un límite de tasa por agente. Al llegar a un tope, el agente queda «en pausa» a la vista.
   *Prueba:* dos agentes que se mencionarían no producen un bucle; el tope corta.
5. **Pertenencia.** Las salas son privadas por invitación de la moderación. Quien entra tarde ve el historial
   completo, con el aviso del acta (fila «Registro de deliberación»). Quien sale deja de ver la sala, y sus mensajes
   quedan con su nombre. «Lo ajeno no existe» (CI-6) se aplica a la **sala**: una sala a la que no pertenecés da
   404. *Prueba:* un no miembro pide la sala → 404; un miembro que salió → 404.
6. **Anatomía mínima.** Cabecera de sala (tema, lista de participantes con presencia y con «agente» rotulado,
   acciones de moderación), lista de mensajes con autor, hora y forma, panel de hilo como lateral dentro de la
   sala, y compositor con menciones (listbox, §7) y «Pedir la palabra». En la sala no hay «escribiendo…» de agentes:
   un agente que redacta se ve como un mensaje pendiente con su nombre. *Estados:* los de §4 más sala cerrada (sólo
   lectura), sin la palabra (compositor en `aria-disabled` con el motivo), expulsado (vista de 404), mensaje
   oculto («Oculto por la moderación: {motivo}», visible a su autor y a la moderación) y agente en pausa o apagado.
   *Superficies:* página propia en todo ancho, con el hilo como hoja inferior por debajo de 992 px. *Teclado:* el de
   §7, y además Alt+↑/↓ entre mensajes y Escape para cerrar el hilo.
7. **Acta.** La leen los participantes y la moderación. Su retención va escrita en la sala y se avisa al entrar.
   A pedido de su autor, un mensaje se reemplaza por «Retirado por su autor» en la sala y en el acta. «Qué se
   decidió» lo escribe la moderación; un agente sólo puede proponer un borrador S3, que la moderación confirma o
   edita antes de que entre al acta. El acta es otro almacén con su fila en la especificación (K1 C-29). *Prueba:*
   un S3 de agente no llega al acta sin la confirmación de la moderación.
8. **Pruebas.** La E2E de sala usa **dos contextos de navegador** de Playwright en la misma prueba (dos
   participantes) sobre el transporte real en tiempo real. Comprueba: llegada al otro en orden, ráfaga con un solo
   anuncio, mensaje oculto, y salir que da 404. La batería por agente sigue K3 con el mismo umbral que la consola
   para S5, y con la rúbrica de K5 §M.6 para S1–S4.

<a id="k8-14"></a>
## 14. Checklist binario de completitud

Cada ítem es sí/no; un «no» impide cerrar.

- [ ] Las superficies comparten un solo estado de conversación y cambiar de superficie no navega (CI-1).
- [ ] Cada estado de §4 tiene prueba bUnit y captura a 1280 y 390 en claro y oscuro (CI-2).
- [ ] Un texto con `<a>` o con una ruta absoluta se muestra como texto y no produce destino (CI-3).
- [ ] La E2E sólo con teclado recorre la tabla de foco de §7 sin caer a `body` (CI-4).
- [ ] Con la consola abierta, el menú de sesión se abre y se usa a 1280 y 390 (CI-5).
- [ ] El almacenamiento de la pestaña tiene sólo ids; un id ajeno da lo mismo que uno inexistente (CI-6).
- [ ] La especificación tiene una fila por almacén (historial, trazas, votos, consumo y, si hay sala, acta) con lector, retención y contenido (CI-7; K1 C-29).
- [ ] Detener o cerrar durante la redacción deja la plantilla, no el parcial (CI-8).
- [ ] Cada respuesta trae el rótulo de origen de la tabla de §3.1 y su forma en el DOM (§3).
- [ ] La auditoría automática, el guion del lector de pantalla y el teléfono real están en el informe con la tabla de §7.1.
- [ ] Si hay hoja inferior, hay E2E y captura a 768 px (§6).
- [ ] El gráfico tiene escala honesta, máximo con texto y valores en texto (§5).
- [ ] Las capas son tokens con orden escrito (§6.2).
- [ ] Con el teclado virtual abierto, el compositor queda visible en un teléfono real (§6.1).
- [ ] El historial es un listbox con ↑ ↓ Inicio Fin, Enter, Supr y Escape (§7).
- [ ] Los límites del historial están escritos y probados, y la lista dice cuánto se guarda (§8).
- [ ] Hay lista de editores largos con guarda y decisión escrita sobre el compositor (§9).
- [ ] El voto va a una tabla y la interfaz dice que se guarda sin texto (§10).
- [ ] Un span de una consulta trae `gen_ai.*` y no trae texto; la captura está apagada en la plantilla (§11).
- [ ] Hay prueba de enmascarado y de purga si la captura se puede encender (§11).
- [ ] K3 entregó informe aceptado con las capturas por estado y el sitio real por rol.

<a id="k8-15"></a>
## 15. Antipatrones con su control

La tabla índice de los antipatrones de la integración está en
[K4 §8](knowledge-AI-Layer-Integration-Working-Method.md#k4-8-antipatrones); acá viven los de la interfaz.

| Id | Antipatrón | Qué costó en los casos | Control |
|---|---|---|---|
| <a id="k8-ap-1"></a>K8-AP-1 | La consola tapa el menú de sesión | en PushDispatch, para cerrar sesión había que cerrar el chat | CI-5; tokens de capas (§6.2) |
| <a id="k8-ap-2"></a>K8-AP-2 | Expandir navega a otra página | se perdía la pantalla de fondo y, sin cuidado, el hilo | CI-1; fondo inerte (§6.5) |
| <a id="k8-ap-3"></a>K8-AP-3 | El servidor reescribe el campo en cada render | lo que la persona seguía tecleando se borraba | §8: el campo vive en el navegador; el servidor sólo lo escribe cuando cambia desde afuera |
| <a id="k8-ap-4"></a>K8-AP-4 | Texto de la conversación en el almacenamiento del navegador | legible por cualquier script y persistente en equipos compartidos | CI-6 |
| <a id="k8-ap-5"></a>K8-AP-5 | Capturar todo el contenido en las trazas «por si acaso» | — (control preventivo; la práctica de la industria lo apaga por omisión) | CI-7, §11.3–§11.5 |
| <a id="k8-ap-6"></a>K8-AP-6 | Control muerto con la IA apagada | un botón que no hace nada parece un error | §3 compositor; §4 IA apagada |
| <a id="k8-ap-7"></a>K8-AP-7 | Panel sin Escape ni manejo del foco | en un caso, abrir no movía el foco ni había Escape para cerrar | §7, tabla de foco |
| <a id="k8-ap-8"></a>K8-AP-8 | `vh` en el teléfono, o creer que `dvh` sigue al teclado | — (control preventivo; los casos usaron `dvh` y `env(safe-area-inset-bottom)` desde el origen) | §6.1 |
| <a id="k8-ap-9"></a>K8-AP-9 | Gesto de voto que sólo va al log | no se pudo medir nada | §10.4, K2-AP-6 |
| <a id="k8-ap-10"></a>K8-AP-10 | Navegar desde un chip pierde lo editado sin aviso | lo tecleado en el compositor se perdía al recargar | §9 |
| <a id="k8-ap-11"></a>K8-AP-11 | Aclaración con identificadores técnicos en las píldoras | la persona no reconoce las opciones | §4 aclaración; [K5-AP-13](knowledge-AI-Layer-Conversational-Design.md#k5-ap-13) |

<a id="k8-16"></a>
## 16. Casos

**PushDispatch (caso de origen, panel con roles).** Una conversación con tres superficies (portada —la tarjeta de la pantalla de inicio—, lateral y
página `/asistente`) sobre un mismo estado; expandir no navega y la pantalla de fondo queda inerte. Cabecera de
cuatro íconos, chip de contexto validado por el servidor, compositor de 500 caracteres con ayuda de teclado y sin
controles muertos, respuesta con bloqueo primero y procedencia plegada, gráfico con escala honesta, historial como
listbox con borrar, lateral a ≥ 992 px, hoja inferior entre 576 y 991 px y pantalla completa debajo. El texto vive
en el servidor en memoria (2 h sin uso y 8 h como máximo, 5 conversaciones, 30 turnos); la pestaña guarda sólo el id cifrado. Hay
guarda de cambios sin guardar en siete editores largos, pero no en el compositor. El voto existía como gesto y
sólo iba al log; el consumo se registra por llamada sin texto con atributos `gen_ai.*`, y el contenido en trazas se enciende
sólo en Development (el interruptor atado a `IsDevelopment()`, REQ-IA-09), que es el ejemplo real de §11.3. Faltaban, al relevarlo,
los tokens de capas (el menú de sesión quedaba debajo del panel), el historial durable, el voto en tabla y la
consola del raíz montada.

**Movilidad Urbana (demo del libro, sin roles).** Panel lateral fijo montado una vez en el layout, burbujas,
insignia de origen, chip de contexto quitable, aclaración con píldoras, gráfico por regla con tabla, votos con
motivo y la frase «tu voto quedó registrado, sin tu pregunta ni tu respuesta». En el teléfono, superposición a
pantalla completa (no hoja). Historial en memoria del circuito, sin lista de conversaciones. Al abrir no movía el
foco ni tenía Escape (K8-AP-7).

**Geometría (caso aplicado, una tarjeta de un turno).** La consola es una tarjeta dentro de dos pantallas, un
formulario de página entera sin guion propio: una sola respuesta, sin burbujas ni historial. Al llegar la
respuesta o el error el foco va a ellos, que en una tarjeta de un turno es lo correcto (§7). La negativa trae la
ficha plegada; las compuestas se muestran parte por parte. Sin voto ni sugerencias iniciales (quedaron como
propuesta). Muestra que §8–§10 son optativos cuando la superficie es de un turno.

<a id="k8-17"></a>
## 17. Límites

- Se destiló de **tres** consolas; sólo una tiene historial, hoja inferior y guarda de cambios. Las cifras (cortes
  de 992 y 576 px, 400 px, 62 dvh, 2 h y 8 h, 5 conversaciones, 30 turnos, 500 caracteres, 5 s, 10 s) son puntos de
  partida medidos allí o elegidos acá, no constantes.
- La persistencia durable no la construyó ningún caso. La captura de contenido existe en PushDispatch sólo como
  interruptor atado al entorno de desarrollo, sin enmascarado ni purga.
- §13 y su §13.1 son propuesta rotulada: nada de ella está probado.
- **Huecos conocidos, sin decidir todavía** (se resuelven con un criterio razonable y se anotan en la
  especificación del sistema):
  - el atajo de teclado para abrir la consola y el recorrido de tabulación dentro de la tarjeta;
  - cómo se anuncia «Volver a la conversación (n)» y qué cuenta n;
  - el umbral de «editor largo» (§9.1) no está medido, y no se dice cómo convive el diálogo con la pantalla completa;
  - el orden de capas es una lista sin caso construido (§6.2);
  - no hay reglas de tema: tokens de color, contraste en oscuro, gráfico en oscuro;
  - con render interactivo del servidor, no hay estado de conexión perdida, ni regla del historial en memoria con
    varias instancias o tras un reinicio;
  - la consola para una persona sin sesión (sitio público) y el alcance «por cuenta de otra persona»;
  - el orden de las sugerencias con pocos eventos y quién genera los seguimientos (K2 §8.6 y K5 §7);
  - la huella del voto es distinta en K2 §8.2 y en K1 REQ-IA-30, y hay que unificarlas;
  - en la sala: interfaz de cola de palabra, apelación, rol observador, tipo de votación y quórum, profundidad de
    hilos, edición de mensajes, orden ante llegadas simultáneas, notificación al mencionado, límite numérico de la
    región viva, y voto y copiar en mensajes de personas;
  - el glosario no unifica todavía los nombres de los controles en todos los conocimientos (píldora, pastilla,
    chip, tarjeta de acción).
- Las convenciones GenAI de OpenTelemetry están en estado *Development* y pueden cambiar nombres de atributos.
- No cubre voz (entrada o salida hablada) ni adjuntos de archivos.
- Si el repo anfitrión tiene su propio sistema de diseño (tokens, componentes, capas), rige el anfitrión y este
  conocimiento sirve de control.

<a id="k8-glosario"></a>
## Glosario

- **Superficie:** forma en que se muestra la consola (tarjeta, lateral, hoja inferior, pantalla completa, página).
- **Hoja inferior:** panel que sube desde abajo y ocupa parte del alto de la ventana.
- **Chip de contexto:** marca quitable que dice sobre qué pantalla o entidad se pregunta.
- **Rótulo de origen:** línea que dice cómo se hizo la respuesta (fija, según la guía, redactada con IA).
- **Región viva:** zona oculta que el lector de pantalla anuncia cuando cambia.
- **Listbox:** patrón de lista de opciones operable con flechas, con una opción activa.
- **Editor largo:** pantalla cuyo contenido cuesta rehacer; recibe la guarda de cambios sin guardar.
- **Captura de contenido:** guardar el texto de preguntas y respuestas en las trazas del operador.
- **Acta de deliberación:** registro de una sala multi-participante (§13).
- **Turno:** en la consola de una persona, el par pregunta y respuesta; en una sala (§13), un mensaje de un autor.
  «Turno asignado por la moderación» se dice «la palabra».
- **Píldora:** botón compacto que dispara una intención o una opción (sugerencia, seguimiento, aclaración). En K2 y
  K7 se la llama «pastilla».
- **Chip:** el chip de contexto (§3) o el chip de destino de K5 §6 («Ir a …»). **Tarjeta de acción:** un destino
  con parámetros, con ícono, título y «Abrir» (§5).
- **Anillo y forma:** de K5 (anillos de la frontera de K1 §3.1; formas F1–F8 y variantes, §3.1).
- **Procedencia:** bloque plegado con herramienta, método, período, hora y fuentes (§3.4).
- **Respaldo:** la respuesta determinista que se da cuando el modelo no está o falla (K2 §10).
- **Portada:** la tarjeta de la consola en la pantalla de inicio, una superficie de tipo tarjeta.
- **Raíz** y **circuito:** el modo raíz de K2 §9 y el circuito de render interactivo del servidor de K2 §16 E-1.

<a id="k8-historial"></a>
## Historial

| Versión | Fecha | Cambio |
|---|---|---|
| 1.0 | 2026-11-15 | Creación, como Borrador, a partir de las consolas de PushDispatch (expediente de la capa de IA), Geometría y Movilidad Urbana: anatomía, estados, destinos y gráficos, escritorio y teléfono, accesibilidad, historial, guarda de cambios, voto, persistencia y observabilidad con captura apagada fuera de desarrollo (alineada con K1 REQ-IA-09), y la variante multi-participante como propuesta rotulada con sus decisiones (§13.1). Antes de publicarse se cerraron los huecos bloqueantes de una prueba a ciegas (dibujo por forma §3.1, confirmación, streaming, hoja a 768 px, borrar en el teléfono, aceptación de accesibilidad §7.1, historial durable, voto definitivo, seudónimo, texto libre sensible); los menores quedan en §17. K1, K2, K3, K4, K5 y K7 remiten acá. |
