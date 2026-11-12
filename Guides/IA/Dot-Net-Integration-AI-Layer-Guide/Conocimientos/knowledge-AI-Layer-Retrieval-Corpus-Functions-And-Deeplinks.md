# Capa de IA integrada en .NET: cómo se redacta el corpus de ayuda para recuperación, se crean las funciones del modelo y se resuelven los deeplinks

**Alias:** AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks
**Naturaleza:** propio
**Tema:** Las tres piezas de contenido y de acción que la consola usa en cada turno: (A) la ayuda escrita para que un recuperador la encuentre (páginas por tarea, frontmatter `preguntas:`/`destinos:`/`status:`, sólo lo vigente se sirve, fichas por pantalla, sinónimos, troceo por párrafo, búsqueda híbrida filtrada por rol, recall@3 medido y los errores de ranking que costaron); (B) las funciones que el modelo puede llamar (cuándo elige el código y cuándo el modelo, sólo lectura, cerradas sobre la sesión, argumentos validados, tope de llamadas, nombres desconocidos rechazados, validador de cifras, registro con `AIFunctionFactory`); (C) los destinos y deeplinks (catálogo tipado con conformidad contra las `@page` y sus políticas, destino por intención y por cuenta, revalidación en el servidor, enlace a la acción exacta, nunca una ruta del texto). Destilado de la consola de PushDispatch, diseñada desde cero
**Consumidor:** un agente IA (o una persona) que escribe o cambia una página de ayuda, agrega una función al modelo o un destino al catálogo; quien revisa ese PR
**Condicion-de-carga:** al escribir o cambiar una página de ayuda, una función o un destino; después de K2 §0–§3; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** AI-Layer-Console-Assistant-Building (K2): el pipeline de K2 manda y este conocimiento detalla sus pasos 5 y 8; AI-Layer-Integration-Designing-Specification (K1): los requisitos `REQ-IA-nn` no se redefinen acá
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` 1.2.0 (§7.1, §7.6, §8.2–§8.7, §9.2–§9.5, §12.2–§12.4, §14.5, §14.10; remisiones `[Guía §x]` con anclas `cN-k`); [K3](knowledge-AI-Layer-Verification-And-Acceptance.md); [K4](knowledge-AI-Layer-Integration-Working-Method.md); [K5](knowledge-AI-Layer-Conversational-Design.md) (marco del turno y formas); `Conversational-Design-Framework-Guide.md` 1.4.0, por [K5 §M](knowledge-AI-Layer-Conversational-Design.md#k5-m); [K7](knowledge-AI-Layer-Testing-Strategy.md)
**Versión:** 1.7
**Estado:** Borrador (pasa a Vigente cuando K7 lo acepte aplicado sobre un segundo sistema, igual que K2)
**Fecha:** 2026-11-12

---

## Índice

- [0. Propósito y mapa de carga](#k6-0)
- [1. Cuándo usarlo y cuándo no](#k6-1)
- [2. Reglas que no se negocian](#k6-2)
- Parte A — Corpus para recuperación
  - [3. Una página por tarea, una intención por sección](#k6-3)
  - [4. El frontmatter: `preguntas:`, `destinos:`, roles y estado](#k6-4)
  - [5. Fichas por pantalla, glosario y sinónimos](#k6-5)
  - [6. Ingesta: troceo, búsqueda híbrida, filtro por rol, precalentado](#k6-6)
  - [7. Medir la recuperación: recall@3 por rol](#k6-7)
  - [8. Errores reales de ranking, su arreglo y su prueba](#k6-8)
- Parte B — Funciones (function calling)
  - [9. Quién elige: el código o el modelo](#k6-9)
  - [10. Diseño de una función](#k6-10)
  - [11. Registro con `AIFunctionFactory`, tope y nombres desconocidos](#k6-11)
  - [12. El validador de cifras](#k6-12)
  - [13. Ejemplo real extractado](#k6-13)
- Parte C — Destinos y deeplinks
  - [14. Catálogo tipado y su conformidad](#k6-14)
  - [15. Destino por intención y por cuenta elegida](#k6-15)
  - [16. Revalidación en el servidor y ver ≠ hacer](#k6-16)
  - [17. Enlace a la acción exacta](#k6-17)
  - [18. Nunca una ruta del texto](#k6-18)
- [19. Procedimiento paso a paso](#k6-19)
- [20. Criterios de parada](#k6-20)
- [21. Checklist binario](#k6-21)
- [22. Antipatrones con su control](#k6-22)
- [23. Casos](#k6-23)
- [24. Límites](#k6-24)
- [Glosario](#k6-glosario)
- [Historial](#k6-historial)

<a id="k6-0"></a>
## 0. Propósito y mapa de carga

K2 dice **en qué orden** corre un turno de la consola; este conocimiento dice **cómo se fabrican** las tres
cosas que ese turno consume: el texto que se recupera (paso 5, anillo 2), la función que trae la cifra (paso 5,
anillo 1) y el destino al que se lleva (paso 8). Son las piezas que más se tocan después de la primera versión y
donde PushDispatch acumuló más defectos sutiles: ninguno rompía una prueba de unidad y todos se veían en una
respuesta equivocada. **Cómo se prueban** en el build lo dice K7; **qué evidencia** cierra una fase, K3.

No repite lo que ya está escrito en otro lado; lo cita:

| Tema | Dónde está | Qué se toma de ahí |
|---|---|---|
| Pipeline del turno, R1–R7, compuerta de cobertura | [K2 §2–§4](knowledge-AI-Layer-Console-Assistant-Building.md#k2-2) | el orden y las garantías que acá se implementan |
| Revalidación, ver ≠ hacer, permiso por instancia | [K2 §5](knowledge-AI-Layer-Console-Assistant-Building.md#k2-5) reglas 2, 3 y 6 | la norma; acá sólo se remite |
| Formas de respuesta (abstención, negativa útil, chip) | [K5 §3](knowledge-AI-Layer-Conversational-Design.md#k5-3) | qué se le muestra a la persona cuando una pieza falta |
| Escalones de contexto y cuándo saltar a RAG | [Guía §7.6](../Dot-Net-Integration-AI-Layer-Guide.md#c7-6), [Guía §14.5](../Dot-Net-Integration-AI-Layer-Guide.md#c14-5) | si esto tiene que ser RAG o no |
| Ingesta, atributos, filtros, medición | [Guía §8.2–§8.7](../Dot-Net-Integration-AI-Layer-Guide.md#c8-2) | el criterio general del RAG |
| Herramientas, sólo lectura, sesión, caso de uso | [Guía §9.2–§9.5](../Dot-Net-Integration-AI-Layer-Guide.md#c9-2) | el criterio general de las funciones |
| Inyección y agencia excesiva | [Guía §12.2](../Dot-Net-Integration-AI-Layer-Guide.md#c12-2), [Guía §12.3](../Dot-Net-Integration-AI-Layer-Guide.md#c12-3) | por qué la ruta y la app nunca salen del texto |
| Pruebas de cada regla de este conocimiento | [K7](knowledge-AI-Layer-Testing-Strategy.md) | cómo se escriben y dónde corren |

**Mapa de carga.** Cargá §0–§2 siempre; después sólo lo del tema que tocás.

| Situación | Secciones |
|---|---|
| Escribir o reescribir una página de ayuda | §3–§7 |
| La consola trae la página equivocada o ninguna | §7, §8 |
| Agregar o cambiar una función del modelo | §9–§13 |
| Agregar o cambiar un destino o un deeplink | §14–§18 |
| Revisar un PR de ayuda, función o destino | §21, §22 |

**Criterio de parada general.** Si la pieza que vas a agregar no está en la especificación de K1 (una función
nueva, un destino con una acción de escritura, un rol que no tiene fila), no la agregues: volvé a K1.

<a id="k6-1"></a>
## 1. Cuándo usarlo y cuándo no

**Usalo** cuando la consola ya existe o se está construyendo con K2 y hay que:
- escribir ayuda que un recuperador va a trocear y rankear (no sólo leer una persona);
- exponer una lectura de datos del producto como función del modelo o del enrutador;
- llevar a la persona a una pantalla, o abrir una acción dentro de ella.

**No lo uses** para:
- **un dato exacto por RAG.** Una cifra, un estado o un id se consulta por función; la ayuda explica *cómo*, no
  *cuánto* [Guía §7.1](../Dot-Net-Integration-AI-Layer-Guide.md#c7-1). *Prueba:* ninguna página del corpus
  trae una cifra de datos vivos (revisión del PR de la página);
- **un corpus chico y estable** que entra entero en el contexto: el escalón 1 (contexto completo con prefijo
  estable) es más simple y no necesita recall [Guía §7.6](../Dot-Net-Integration-AI-Layer-Guide.md#c7-6);
- **funciones que escriben.** Este conocimiento cubre el escalón 0 de agencia; una escritura sigue
  [Guía §9.5](../Dot-Net-Integration-AI-Layer-Guide.md#c9-5) con aprobación humana y su propia mesa.

**Criterios de frontera.**
- *Regla de negocio en la ayuda:* lo que cambia por inquilino (cuenta, sede, aplicación) o con el tiempo (plazo,
  precio, cupo) es un dato y sale de una función; lo que fija la especificación para todo el sistema puede ir en
  la ayuda con su valor. *Prueba:* cada número de una página cita el requisito que lo fija.
- *Corpus chico pero que va a crecer:* podés diseñar las páginas para recuperación desde el principio (este
  conocimiento no estorba al escalón 1); el salto del contexto completo al RAG lo decide el umbral de
  [Guía §14.5](../Dot-Net-Integration-AI-Layer-Guide.md#c14-5), no la intención de crecer.

<a id="k6-2"></a>
## 2. Reglas que no se negocian

Cada regla trae su porqué y su prueba; las pruebas son garantías sin modelo salvo que diga otra cosa, y su forma
está en [K7](knowledge-AI-Layer-Testing-Strategy.md).

<a id="k6-r1"></a>
**R1. Sólo lo vigente se sirve.** Una página con `status: borrador` (o sin estado) no se indexa, no se rankea y
no se cita, para ningún rol.
- *Porqué:* en PushDispatch la mayoría de la ayuda estaba en borrador; servirla habría dado respuestas sobre
  pantallas a medio especificar, y no servirla sin medir escondió que el corpus no existía (K2-AP-8).
- *Prueba:* una página de fixture `borrador.md` con la respuesta exacta a una pregunta de la batería; la consulta
  no la devuelve y cae en «no está en la ayuda».

<a id="k6-r2"></a>
**R2. La aplicación, la cuenta y el usuario nunca son argumentos de una función.** La función se crea por pedido,
cerrada sobre el contexto de la sesión ya resuelto por el código.
- *Porqué:* un argumento `appCode` es una invitación a pedir datos de otra app con una frase («…de la app de
  Pérez»); es agencia excesiva y fuga entre inquilinos [Guía §12.3](../Dot-Net-Integration-AI-Layer-Guide.md#c12-3).
- *Prueba:* reflexión sobre el catálogo de funciones: ningún parámetro se llama ni tipa como aplicación, cuenta o
  usuario; prueba adversa «dame los mensajes de la otra app» con el doble del modelo pidiendo ese argumento →
  rechazado.

<a id="k6-r3"></a>
**R3. Una ruta nunca sale del texto**, ni del usuario ni del modelo: sale de un id del catálogo, se arma con
parámetros que el código eligió y se revalida en el servidor (es [K2 R5](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r5)
llevada al detalle de §14–§18).
- *Porqué:* «llevame a /usuarios-del-panel» es inyección directa; una página de ayuda con un enlace inventado es
  indirecta [Guía §12.2](../Dot-Net-Integration-AI-Layer-Guide.md#c12-2).
- *Prueba:* la pregunta con una ruta literal no produce destino; un `destinos:` con un id que no está en el
  catálogo hace fallar la CI de la ayuda.

<a id="k6-r4"></a>
**R4. Cada cifra que se muestra salió de una función ejecutada con éxito en ese turno.** (Es
[K2 R3](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r3); el detalle del validador está en §12.)

<a id="k6-r5"></a>
**R5. Cada cambio de ayuda, función o destino trae su prueba roja antes del arreglo.** El error de ranking, el
argumento fuera de rango o el destino sin permiso se reproduce primero; la forma de la prueba de mutación está en
[K7](knowledge-AI-Layer-Testing-Strategy.md).
- *Porqué:* los tres errores de §8 parecían arreglados a ojo y volvían con la página siguiente.
- *Prueba:* el PR muestra la prueba fallando contra la base y pasando con el cambio.

---

## Parte A — Corpus para recuperación

<a id="k6-3"></a>
## 3. Una página por tarea, una intención por sección

Una página de ayuda escrita para personas se lee de arriba abajo; una escrita para recuperación se lee **de a
trozos sueltos**, sin el resto. Escribí para ese lector.

1. **Una página por tarea o por pantalla**, titulada con la tarea en el verbo que usa la persona: «Alta de una
   aplicación», no «Aplicaciones». *Porqué:* el título es el lugar de más peso después de `preguntas:` (§6);
   un título temático compite con todas las preguntas del tema (§8.2). *Prueba:* lista de títulos del corpus
   revisada en el PR: ninguno es sólo un sustantivo de área.
2. **Cada `##` responde una sola intención** («Dar de alta», «Administradores y desarrolladores», «Archivar»).
   *Porqué:* en PushDispatch el bloque del raíz decía en el mismo párrafo cómo dar de alta y cómo asignar
   administradores; el trozo que leía el modelo juntaba las dos y la respuesta a «¿Cómo doy de alta una
   aplicación?» mezclaba la asignación. La asignación pasó a su propia sección. *Prueba:* la pregunta de la tarea
   recupera un trozo que no contiene la otra intención (en PushDispatch,
   `Raiz_el_alta_no_mezcla_la_asignacion_de_administradores`, roja con la guía anterior).
3. **Nada importante en la introducción.** El párrafo antes del primer `##` es contexto («Cada aplicación tiene su
   propia audiencia…»), nunca los pasos. *Porqué:* una página sin `##` hacía que la plantilla de respaldo
   contestara con la introducción (actuación 018 del expediente de la capa). *Prueba:* las páginas con pasos
   tienen al menos un `##`; chequeo de CI de la ayuda.
4. **Cada sección se entiende sola:** repetí el objeto («el alta de la aplicación»), no «esto» ni «lo anterior».
   *Porqué:* el trozo viaja sin su vecino. *Prueba:* lectura del trozo aislado en el PR.
5. **Los pasos nombran el control visible** («botón **Nueva aplicación**») y en negrita. *Porqué:* la negrita es
   lugar fuerte para el léxico y el modelo copia el rótulo correcto. *Prueba:* cada paso con acción tiene un
   rótulo en negrita que existe en la pantalla (lo confirma la captura de K3).
6. **Variantes por rol dentro de la página** con bloques `::: roles <rol>` … `:::`, quitados en el servidor
   antes de trocear para quien no tiene el rol. *Porqué:* una página por rol duplicaba texto y desalineaba; el
   bloque se filtra antes que nada llegue al ranking (R4 de K2). *Prueba:* el administrador de aplicación no
   recupera el texto del bloque del raíz (en PushDispatch, `VariantesPorRol` y su par en las pruebas de ayuda).
   *Gramática del bloque:* la línea de apertura es `::: roles ` seguida de uno o más roles separados por espacio
   o coma (`::: roles admin-raiz, admin-app`); alcanza con tener **uno** de ellos. El bloque cierra con `:::` sola
   en su línea, no se anida, y se resuelve antes de trocear: un `##` dentro del bloque cuenta como sección para
   §6.2 sólo para quien conserva el bloque. *Prueba:* fixture con un bloque de dos roles: cada rol lo lee y un
   tercero no.

<a id="k6-4"></a>
## 4. El frontmatter: `preguntas:`, `destinos:`, roles y estado

El frontmatter es el contrato entre la página y la consola. Validalo con un esquema en la CI de la ayuda.

| Campo | Qué lleva | Regla | Prueba |
|---|---|---|---|
| `title` | la tarea | §3.1 | esquema |
| `contexto` | la pantalla (`panel/aplicaciones`) | una por página; debe existir como ruta | chequeo contra las `@page` |
| `status` | `borrador`, `vigente` o `retirado` | sólo `vigente` se sirve (R1). En PushDispatch una página **sin** `status:` cuenta como vigente, porque las páginas heredadas no lo declaraban; en un corpus nuevo, hacé el campo obligatorio en el esquema | fixture `borrador.md` |
| `delivery` (audiencia) | el canal de lectura de la página (en PushDispatch una clave `LEC-nn`) | sin `delivery`, la página **no entra al catálogo**: no se sirve a nadie | fixture `sin-audiencia.md` |
| `roles` | lista de roles que pueden leer la página entera (`roles: [admin-raiz]`) | opcional; **sin `roles`, la lee todo usuario que pasa la política del panel**, por eso la omisión se revisa en el PR; filtro antes del ranking | fixture `por-rol.md` |
| `preguntas:` | 2 a 5 preguntas reales con las palabras de la persona | nunca una pregunta que también conteste otra página (§8.1) | recall@3 (§7) |
| `destinos:` | ids del catálogo, del más general al de la acción | cada id existe en el catálogo (R3) | CI: destinos válidos |

**Reglas de redacción del frontmatter.**
- Escribí `preguntas:` **copiadas de cómo pregunta la gente** (batería, votos, soporte), no reformuladas en
  lenguaje del producto. *Porqué:* pesan más que el título (§6); una pregunta inventada por el redactor sólo
  atrapa preguntas del redactor. *Prueba:* cada pregunta del frontmatter es también un caso del conjunto de
  medición de §7 con esa página como esperada.
- Declará en `destinos:` **todos** los destinos de la página, incluida la acción (`aplicaciones.lista`,
  `aplicaciones.nueva`). El orden es el de respaldo; la intención lo reordena (§15). *Prueba:* CI de destinos.
- No pongas `status: vigente` para que una prueba pase: si la prueba necesita una página que el corpus todavía
  no tiene, usá una **página sintética dentro de la prueba** (PushDispatch lo hizo con la guía de organizaciones,
  que estaba en otra rama). *Porqué:* marcar vigente lo que no está revisado publica texto a medio hacer.

Extracto del frontmatter de una página real de PushDispatch (docs de la ayuda del panel; cuerpo omitido):

```yaml
---
title: Alta de una aplicación
contexto: panel/aplicaciones
nivel: producto
delivery: LEC-02
destinos:
  - aplicaciones.lista
  - aplicaciones.nueva
---
```

Esa página heredada no declara `status:` (vale vigente) ni `roles:` (la lee todo el panel; sus variantes van en
bloques `::: roles`). Un frontmatter completo con todos los campos que tienen regla (NO VERIFICADO: sintético, de
un sistema de turnos inventado):

```yaml
---
title: Cancelar un turno
contexto: panel/turnos
delivery: LEC-01
status: vigente
roles: [recepcion, admin]
preguntas:
  - ¿Cómo cancelo un turno?
  - ¿Puedo dar de baja el turno de mañana?
destinos:
  - turnos.lista
  - turnos.cancelar
---
```

<a id="k6-5"></a>
## 5. Fichas por pantalla, glosario y sinónimos

- **Una ficha por `@page`** que el rol ve, aunque sea corta: qué muestra la pantalla, qué se puede hacer y a
  dónde lleva. *Porqué:* la cobertura de K2 §4 se mide por pantalla; al revisar la consola de PushDispatch apenas
  7 de 38 pantallas tenían ayuda y se diagnosticó como búsqueda lo que era falta de documento
  ([K2-AP-8](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-8)). *Prueba:* la compuerta de cobertura de
  [K2 §4](knowledge-AI-Layer-Console-Assistant-Building.md#k2-4-compuerta).
- **Ficha y tareas.** La ficha de una pantalla es la página cuyo `contexto` es esa pantalla. Una tarea con sus
  propias `preguntas:` va en una página aparte con el **mismo** `contexto`; una tarea corta es una sección `##` de
  la ficha. *Porqué:* la cobertura cuenta pantallas por `contexto`, y la recuperación compite por página: una
  tarea con preguntas propias dentro de una ficha larga pierde contra el resto de la ficha (§8.3). *Prueba:*
  cada `contexto` del catálogo tiene al menos una página; ninguna página tiene más de una sección con
  `preguntas:` propias.
- **Glosario de dominio en una página propia**, con cada término y sus formas («push», «notificación»,
  «mensaje»). Lo que es vocabulario no se explica de nuevo en cada página.
- **Sinónimos en el código del léxico, mínimos y por raíz**, no en el texto: «crear» = «alta» = «nueva»,
  «notificación» = «push», «permiso» = «rol» = «acceso». *Porqué:* en PushDispatch «¿cómo pruebo una
  notificación antes de enviarla?» no llegaba a «Probar, guardar y enviar» porque «pruebo» y «prueba» no
  compartían raíz. *Prueba:* cada sinónimo agregado trae el caso de medición que lo motivó.
  *Cómo se calcula la raíz:* minúsculas, sin tildes, sin palabras vacías y recortada a las primeras **seis**
  letras («notificación» → `notifi`); el diccionario de sinónimos mapea raíces a un término canónico
  (`creden` → `#credencial`). Lo que no está en el diccionario es su propia raíz.
- **Cuidado con los sinónimos que disparan intenciones.** «agregar», «agrego», «registrar», «registro» pasaban
  por alta aunque se hablara del «registro de cambios»; ahora cuentan como alta sólo si lo que se agrega es una
  aplicación, organización o cuenta (crítica del PR del alta por enlace). *Prueba:* par de casos «agregar una
  aplicación» → alta, «ver el registro de cambios» → no alta.

<a id="k6-6"></a>
## 6. Ingesta: troceo, búsqueda híbrida, filtro por rol, precalentado

La definición de corpus y su compuerta de cobertura viven en [K2 §4](knowledge-AI-Layer-Console-Assistant-Building.md#k2-4);
el criterio general, en [Guía §8.2–§8.4](../Dot-Net-Integration-AI-Layer-Guide.md#c8-2). Acá, cómo se hizo y
por qué.

1. **Resolvé el texto por rol antes de trocear** (quitá los bloques `::: roles` ajenos). *Porqué:* trocear
   primero deja trozos con texto de otro rol que el filtro de atributos ya no ve. *Prueba:* fixture
   `variantes.md`: el rol sin el bloque no recupera ninguna de sus palabras.
2. **Troceá por `##` y, dentro, por párrafos de hasta 500 caracteres**, cada trozo con el rótulo
   `página › sección`; un párrafo que termina en «:» va con lo que sigue; las tablas pasan a ítems con las mismas
   palabras. *Porqué:* antes se cortaba a los 1200 caracteres y se perdía el final de las secciones largas; con
   trozos de 500 y recorte por párrafos la batería de redacción pasó de 21/33 a 27/30 ×3. *Prueba:* sección de
   1300 caracteres → tres trozos, ninguno empieza a mitad de oración.
3. **El embedding se calcula sobre rótulo + texto**, con los prefijos de consulta y de documento que pida el
   modelo de embeddings. *Prueba:* la consulta que nombra sólo la sección recupera el trozo aunque el párrafo no
   la repita.
4. **Búsqueda híbrida léxica + vectorial**, fusionada por rango recíproco (en PushDispatch RRF con k = 60), con
   **umbral** de coseno y regla de evidencia léxica (§8). Sin modelo de embeddings, sólo léxico. *Porqué:* el
   léxico gana en nombres de pantalla y códigos; el vectorial en paráfrasis. *Prueba:* degradación sin
   embeddings → la consola responde con léxico y no se cae.
   *Calibrá el umbral de coseno* con el conjunto de §7: partí del valor medido en PushDispatch (0,62 con su
   modelo de embeddings, configurable), barré el umbral sobre el conjunto y quedate con el **más bajo que da 100 %
   de «ninguna»** en las no respondibles sin bajar recall@3 del umbral. Cambiar el modelo de embeddings obliga a
   recalibrar.
5. **Pesos del léxico por lugar**, puntos de partida medidos en PushDispatch: `preguntas:` 4, título 3,
   encabezado o negrita 2, cuerpo 1, más 2 por cada par de palabras seguidas de la pregunta que aparece igual en
   la página. *Porqué:* sin peso por lugar, una mención al pasar valía lo mismo que el título. *Límite:* esos
   pesos causaron los errores de §8.1 y §8.2; no los copies sin la regla de evidencia.
6. **Filtrá por rol y por alcance antes del top-k** (es [K2 R4](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r4)).
   Sin aplicación elegida, las páginas y bloques de roles de aplicación no entran aunque la persona tenga ese rol
   en alguna app. *Porqué:* era una fuga entre aplicaciones. *Prueba:* administrador de la app A, sin chip →
   ningún trozo del bloque de administrador de aplicación.
7. **Top 1 a 3 al modelo, con tope de tokens** (en PushDispatch 800 estimados); si un trozo no entra, quedan sus
   párrafos con más términos de la pregunta, en su orden. Sin evidencia → la plantilla de abstención **sin llamar
   al modelo** ([K2 R2](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r2)).
8. **Precalentá** los embeddings al arrancar, con caché por hash de (modelo de embeddings + texto resuelto por
   rol), sobre las variantes extremas de cada página (sin roles y con todos). *Prueba:* segundo arranque sin
   cambios no recalcula; cambiar el modelo de embeddings invalida todo.

Snippet de la idea del paso 6 (NO VERIFICADO: ilustra el orden, no compila tal cual):

```csharp
// El filtro va ANTES del ranking: k resultados del rol, nunca k globales recortados.
var candidatos = corpus.Where(t => t.Vigente && politica.PuedeLeer(usuario, t) && t.EnAlcance(alcance));
var top = Fusionar(Lexico(candidatos, pregunta), Vectorial(candidatos, pregunta), k: 60)
          .Where(r => r.TieneEvidencia).Take(3);
```

<a id="k6-7"></a>
## 7. Medir la recuperación: recall@3 por rol

Medí antes de mirar respuestas [Guía §8.5](../Dot-Net-Integration-AI-Layer-Guide.md#c8-5); la batería de
respuestas es de K3.

1. **Conjunto de medición** `pregunta → página (o sección) esperada`, **por rol**, con: todas las
   `preguntas:` del frontmatter; las preguntas reales de la batería y de los votos negativos; y **no
   respondibles** (lo que el corpus no cubre, como «¿qué es un titular?» cuando no hay guía de eso) con
   esperada «ninguna». *Porqué:* sin no respondibles, un recuperador que siempre devuelve algo saca recall alto.
2. **Métricas:** hit@1, hit@3 (recall@3 con un solo relevante) y rango recíproco; para las no respondibles, la
   tasa de «ninguna». Contra una **línea base** (léxico solo) para saber si el vectorial suma.
3. **Umbral:** el que fije K1; sin umbral escrito, recall@3 ≥ 0,9 por rol y 100 % de «ninguna» en las no
   respondibles. *Porqué:* una no respondible que trae página es una respuesta inventada esperando el modelo.
4. **Corre en la CI de la ayuda** sin modelo de redacción: con el léxico siempre, con embeddings cuando el
   entorno los tiene (se informa cuál corrió; nunca un `Skip` silencioso, ver K7).
5. **Cada página nueva suma sus casos** y la medición se repite completa: una página nueva puede robar preguntas
   de otra (§8.2). *Prueba:* el PR de una página muestra el recall de todas, no sólo de la nueva.

La forma del cálculo está en el ejemplo `06-Embeddings-Y-Busqueda-Semantica` de la guía (`Metricas`) (hit@1, hit@3, rango recíproco y recall@3
de un mismo rango).

<a id="k6-8"></a>
## 8. Errores reales de ranking, su arreglo y su prueba

Los tres pasaron todas las pruebas de unidad y se vieron en respuestas. Cada uno queda como caso permanente del
conjunto de §7.

<a id="k6-8-0"></a>
**8.0. Qué palabra es común y cuál distintiva.** Los arreglos de §8.1 y §8.2 dependen de esta distinción, así que
tiene que ser un dato, no un juicio del momento.
- *Cómo está en PushDispatch:* una **lista cerrada a mano en el código del léxico** (`Comunes` en
  `BuscadorDeAyudaLexico`), sobre raíces ya normalizadas: los canónicos de verbos genéricos del dominio
  (`#alta`, `#baja`), el objeto principal del producto (`#app`) y palabras de interfaz («panel», «pantalla»,
  «ver», «usar», «datos», «paso»). Toda raíz que no está en la lista ni en las palabras vacías es **distintiva**.
- *Cómo se arma en otro sistema:* candidatas = raíces que aparecen en el título o en `preguntas:` de **más de una
  página** del corpus vigente (se listan con un script sobre el corpus); entra a la lista la candidata que tiene
  un caso del conjunto de §7 que falla sin ella. Ninguna palabra entra sin su caso.
- *Prueba:* la lista vive en un solo lugar del código; cada entrada tiene un caso de medición, y el conjunto de
  §7 se repite completo al cambiarla.

<a id="k6-8-1"></a>
**8.1. Una pregunta del frontmatter decidía sola la página.**
- *Qué pasó:* con `preguntas:` pesando 4, una página cuya pregunta compartía una palabra común con la consulta
  ganaba aunque el resto de la consulta no estuviera en la página: «alta de una organización» devolvía «Alta de
  una aplicación» por la palabra «alta».
- *Arreglo:* **evidencia, no coincidencia**. Toda palabra **distintiva** de la consulta tiene que estar en la
  página, y alguna distintiva en un lugar fuerte (título, encabezado, negrita o `preguntas:`); las palabras
  **comunes** («alta», «aplicación») suman pero solas no son evidencia. Sin evidencia, nada: la consola dice que
  no está en la ayuda.
- *Prueba:* `alta de una organización` → nunca la guía de aplicaciones (en PushDispatch,
  `N01_alta_de_organizacion_nunca_devuelve_la_guia_de_aplicaciones`, con una guía sintética de organizaciones).

<a id="k6-8-2"></a>
**8.2. Títulos que comparten palabras desviaban el ranking.**
- *Qué pasó:* «Segmentos de una aplicación» atrapaba «¿Cómo doy de alta una aplicación?»: el título compartía
  «aplicación» con la pregunta y la página del alta, sin `##` ni pregunta propia, quedaba abajo.
- *Arreglo:* si la consulta sólo tiene palabras comunes, **todas** tienen que estar en el título o en
  `preguntas:` de la página; el lugar fuerte lo da una palabra distintiva («alta» en el título «Alta de una
  aplicación» no avala «segmento»); y cada página declara su tarea en el título (§3.1) y en `preguntas:`.
- *Prueba:* par de casos `¿Cómo doy de alta una aplicación?` → alta, `¿Qué es un segmento?` → segmentos, los dos
  con la otra página presente en el corpus.

<a id="k6-8-3"></a>
**8.3. Una sección poco pertinente hundía la página (o la representaba).**
- *Qué pasó:* la página correcta se puntuaba por su texto entero o por su primer párrafo; una introducción o una
  sección lateral sin las palabras de la consulta bajaba la página, y cuando ganaba, la plantilla de respaldo
  devolvía la introducción («Cada aplicación tiene su propia audiencia…») en lugar de los pasos.
- *Arreglo:* rankear y extraer **por sección**: la página vale lo que su sección más pertinente; para la
  plantilla y para el contexto del modelo, la sección cuyo encabezado nombra la acción de la pregunta y, a
  igualdad, una sección antes que la introducción.
- *Prueba:* sobre la ayuda real, las preguntas de alta, enviar, programar y archivar devuelven la sección de la
  acción y nunca la introducción, con y sin modelo (en PushDispatch, `PlantillaDeRespaldoTests`).

**Diagnosticá con datos.** La batería vuelca, por pregunta, los trozos recuperados con su puntaje; así se vio que
la mayoría de los descartes no eran del control sino abstenciones correctas del modelo. Sin ese volcado, cada
error de ranking parece un error de redacción.

---

## Parte B — Funciones (function calling)

<a id="k6-9"></a>
## 9. Quién elige: el código o el modelo

| Elige el **código** (enrutador) | Elige el **modelo** (invocación de funciones) |
|---|---|
| la intención está resuelta y es del anillo 1 (una cifra conocida: suscriptores, envíos, tasa) | la pregunta pide combinar varias lecturas que no se pueden prever en una tabla |
| el modelo disponible es chico o lento al primer token | el modelo elige bien herramienta y argumentos en la batería ×3 |
| hay que garantizar alcance y destino | el alcance ya está fijado por la sesión y no es argumento (R2) |

**Regla:** empezá por el código ([K2 R1](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r1)); pasá una
intención al modelo sólo si la batería ×3 de K3 lo justifica con el mismo modelo que la app.
- *Qué se construye primero y contra qué se compara:* primero el ruteo por código y las funciones (que sirven a
  los dos caminos, ver abajo); después, la consulta con modelo detrás de la misma bandera. Se corren **los mismos
  casos** de la intención ×3 por los dos caminos; la intención pasa al modelo sólo si su mínimo de las tres
  corridas **iguala o supera** al del código, con cero cifras sin origen y sin peor latencia en caliente que la
  que fija K1. Si no, queda en el código y el resultado se registra.
- *Porqué:* en PushDispatch el llamado a herramientas con un modelo chico elegía mal herramienta y alcance; el
  ruteo determinista se prueba por tabla y no varía entre corridas. La consulta con modelo quedó para lo que el
  enrutador no cubre, con las mismas funciones.
- *Prueba:* cada intención tiene escrito quién elige; las del código tienen su fila en la tabla del enrutador y
  el doble del modelo verifica que el pedido **no** lleva definiciones de herramientas; las del modelo tienen sus
  casos en la batería.
- *Límite del ruteo por código:* el enrutador es determinista pero no completo frente al lenguaje abierto. Su
  tabla se escribe en los dos sentidos y se somete a la sonda de [K7 R6.7–R6.9](knowledge-AI-Layer-Testing-Strategy.md#k7-6-7);
  lo que la sonda no seca se declara como límite ([K3 R2.9](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-2-9)).
  Si las fallas persisten en un registro (en Geometría, los planteos cotidianos), el candidato es un clasificador
  por embeddings entrenado con la sonda acumulada, medido con los mismos casos, no otra lista de palabras.

Las mismas funciones sirven a los dos caminos: el enrutador las llama directo y la consulta con modelo las
ofrece. Así una función se prueba una vez [Guía §9.4](../Dot-Net-Integration-AI-Layer-Guide.md#c9-4).

<a id="k6-10"></a>
## 10. Diseño de una función

1. **Sólo lectura.** Marcala con un atributo propio (en PushDispatch `HerramientaDeSoloLectura`) que la
   reflexión usa para armar el catálogo. *Prueba:* ninguna función del catálogo llama a un caso de uso que
   escribe (revisión + prueba de que el doble de la fuente sólo expone lecturas).
2. **Cerrada sobre el contexto de la sesión** (R2): la clase recibe en el constructor la fuente y un
   `ContextoDeConsulta` (aplicación, cuenta, rol, correlación) que resolvió el código; se crea **por pedido**.
   *Prueba:* reflexión: ningún parámetro de aplicación o cuenta.
3. **Argumentos cerrados y validados antes de tocar la fuente:** enumerados con `Enum.IsDefined`, ids `> 0`,
   página dentro de `1..N`, ventanas de tiempo («últimos 7 días») como enumerado y no como texto. Un argumento fuera de rango
   devuelve `argumento_invalido` **sin llamar al servicio**. *Porqué:* el modelo inventa valores plausibles
   («Enviados», página 40). *Prueba:* por cada argumento, un caso fuera de rango → `argumento_invalido` y cero
   llamadas a la fuente.
   *Fechas y horas concretas* («el sábado 15», «a las 19»), que PushDispatch no necesitó: tipalas (`DateOnly`,
   `TimeOnly`, nunca `string`) y validalas contra un **rango relativo al reloj del servidor** (un `TimeProvider`
   inyectado; por ejemplo de hoy a hoy + 30 días) y contra la grilla del dominio (horas en punto, si así es la
   agenda). El modelo nunca aporta «hoy»: lo pone el contexto de la sesión. *Prueba:* fecha fuera del rango, en
   el pasado u hora fuera de grilla → `argumento_invalido` y cero llamadas, con el reloj fijado en la prueba.
4. **Resultado proyectado mínimo:** sólo cifras y estados; **nunca texto escrito por terceros** (título,
   cuerpo, botones, nombres de segmento, creador). *Porqué:* el texto libre es el vector de inyección indirecta
   y un dato personal [Guía §12.4](../Dot-Net-Integration-AI-Layer-Guide.md#c12-4). *Prueba:* doble de la fuente
   con un canario en esos campos → el canario no llega al pedido al modelo.
5. **Errores como dato:** `no_encontrado`, `argumento_invalido`, `no_disponible`; **nunca el mensaje de la
   excepción** (puede traer la dirección del servidor o un fragmento del pedido). *Prueba:* la fuente lanza →
   la función devuelve `no_disponible` y el texto de la excepción no aparece en el resultado ni en el registro.
6. **Descripción con las unidades y los valores válidos**, en el idioma de la persona: qué mide, cómo se calcula
   una tasa («aperturas / entregadas»), valores del enumerado y rango de página. *Porqué:* el modelo elige por la
   descripción. *Prueba:* revisión del PR: cada argumento tiene su rango en la descripción.
7. **Paginado con tope fijo** (en PushDispatch 20 por página, hasta la página 5). *Porqué:* acota el contexto y
   el costo de una pregunta abierta.

<a id="k6-11"></a>
## 11. Registro con `AIFunctionFactory`, tope y nombres desconocidos

Con Microsoft.Extensions.AI (criterio en [Guía §9.2](../Dot-Net-Integration-AI-Layer-Guide.md#c9-2); el ejemplo
`08-Primera-Herramienta-Solo-Lectura` de la guía es la versión mínima):

1. **Armá el catálogo por reflexión** sobre los métodos con el atributo, ordenados por nombre, y creá cada
   `AIFunction` con `AIFunctionFactory.Create(metodo, instancia, opciones)` dando **nombre y descripción
   explícitos** (no los infiera del método). *Porqué:* el nombre es contrato con el prompt y con las pruebas; el
   ejemplo 14 de la guía muestra que `AIFunctionFactory` no lee las anotaciones de otro marco.
2. **Conversación nueva por llamada:** prompt de sistema fijo + la pregunta, sin historial. *Prueba:* dos
   consultas seguidas con el doble → el segundo pedido no contiene nada del primero.
3. **Tope de llamadas por turno** en dos lugares: `MaximumIterationsPerRequest` de la invocación de funciones y
   un contador propio que envuelve cada función (`DelegatingAIFunction`) y devuelve `limite_de_llamadas` al
   pasarse. Si el turno termina pidiendo otra función, el resultado es «degradado por límite», no el texto.
   *Prueba:* doble que pide funciones en bucle → corta en el tope y degrada.
4. **Sin invocación concurrente ni errores detallados** (`AllowConcurrentInvocation = false`,
   `IncludeDetailedErrors = false`). *Porqué:* el orden de las lecturas importa para el validador y el detalle
   del error vuelve al modelo.
5. **Nombre desconocido → rechazo del turno**, no reintento: si alguna llamada de la respuesta nombra una función
   fuera del catálogo, el resultado es `herramienta_desconocida`. *Prueba:* doble que pide `borrar_mensaje` →
   rechazado y nada se muestra.
6. **Tiempo máximo y cupo:** el turno se cancela al tiempo máximo y degrada con el enlace al informe; si el
   servidor de inferencia atiende de a una, una segunda consulta degrada con «ocupado» en vez de esperar.
7. **Registro sin texto:** motivo, conteo de llamadas y correlación; ni pregunta ni respuesta
   [Guía §14.10](../Dot-Net-Integration-AI-Layer-Guide.md#c14-10). *Prueba:* registro capturado de un turno no
   contiene la pregunta.

<a id="k6-12"></a>
## 12. El validador de cifras

Implementa [K2 R3](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r3): nada se muestra sin pasar por él.

1. **Fuentes válidas de un número:** los resultados de las funciones del turno, la pregunta misma (si dijo «el
   mensaje 1042», 1042 puede aparecer) y los argumentos **de llamadas exitosas**.
2. **Una llamada fallida no respalda su argumento.** *Porqué:* una auditoría de seguridad de PushDispatch mostró
   que una llamada con `id = 99999` que devolvía `no_encontrado` dejaba pasar «hubo 99999». *Prueba:* doble que
   llama con un id inexistente y redacta ese número → rechazado.
3. **Sin origen → texto fijo** (`cifra_ausente`), nunca el texto con la cifra tachada. *Prueba:* respuesta doble
   con un número ajeno → reemplazada.
4. **El razonamiento del modelo no se muestra ni se valida:** sólo el texto de la respuesta. *Prueba:* respuesta
   doble con contenido de razonamiento → no aparece.
5. **Formatos:** normalizá miles, decimales y porcentajes antes de comparar (12.345, 12345, 12,3 %); una tasa
   calculada por el modelo cuenta como cifra sin origen salvo que la función la devuelva. *Porqué:* las tasas se
   calculan en la función con su fórmula escrita en la descripción.
6. **Horas, fechas e ids también son cifras con origen.** «19:00», «15/11» y «cancha 3» se validan igual que un
   total: tienen que salir de un resultado, de la pregunta o de un argumento de llamada exitosa. Normalizá antes
   de comparar: horas a `HH:mm` («19 h», «19:00», «7 de la tarde» → `19:00`), fechas a ISO (`2026-11-15`, con el
   año del reloj del servidor si falta) e ids a su número sin ceros a la izquierda. *Porqué:* en un dominio con
   agenda, una hora inventada hace tanto daño como una cifra inventada. *Límite:* PushDispatch valida cantidades,
   porcentajes e ids; la normalización de horas y fechas es una extensión no medida allí. *Prueba:* respuesta
   doble con una hora que ningún resultado trae → `cifra_ausente`.
7. **Cada cifra va atada a su categoría y a su orden.** Que el número exista entre los resultados no alcanza:
   el validador compara pares `(categoría, cifra)` y los ordinales («la más común», «la segunda») contra el orden
   que calculó el código; si el texto atribuye una cifra a otra categoría, o cambia el orden, es `cifra_ausente`.
   La alternativa equivalente es exigir que el texto conserve, palabra por palabra, los pares de la plantilla.
   *Porqué:* en [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor) el modelo escribió «Trabajo y Estudio, cada una con 4 respuestas» cuando Trabajo tenía 5;
   el 4 existía (era Estudio) y el validador por número suelto lo dejó pasar bajo el rótulo «Redactado con IA».
   Bajar el umbral no lo arregla. *Prueba:* doble que intercambia las cifras de dos categorías, y doble que
   dice «la segunda» de la tercera → rechazados en los dos casos; la batería incluye un fixture con categorías de
   cifras distintas pero cercanas.

<a id="k6-12-8"></a>
8. **Lo que el validador de cifras no ve.** Una afirmación falsa sin números pasa intacta: en Movilidad Urbana
   salió una paráfrasis que invertía el sentido de una nota y una restricción inventada («sólo en Corrientes»).
   Por eso, además de las cifras, el texto redactado se contrasta contra la plantilla en **negaciones,
   restricciones y entidades**: cada «no», «sólo», «nunca», «excepto» y cada nombre propio del dominio tiene que
   estar en la plantilla o en un resultado; si no, va la plantilla. *Límite:* una lista cerrada de entidades del
   dominio frena las conocidas, no las inventadas; el control correcto es «todo nombre propio del texto está en
   la fuente», no «ningún nombre de la lista falta». *Prueba:* doble que invierte una nota y doble que agrega
   «sólo en <localidad>» → rechazados.
9. **Normalizá el formato de las citas antes de comparar.** El atajo «idénticas salvo citas» no vio «100 %[1]»
   sin espacio. Normalizá el espaciado (o exigí un espacio antes de «[») y tratá una respuesta que difiere de la
   plantilla sólo en citas como plantilla, no como redactada. *Prueba:* doble con «[1]» pegado → corregido o
   rechazado.
10. **El validador informa su tasa de rechazo** por forma y por corrida (cuántas redacciones descartó y por qué
   regla). *Porqué:* en Movilidad Urbana un caso a temperatura 0 incumplía la regla de citas y otro traía una
   insinuación falsa, y «0 cifras inventadas» lo escondía. *Prueba:* el registro de la batería tiene la columna.

<a id="k6-13"></a>
## 13. Ejemplo real extractado

Extracto de PushDispatch (`HerramientasDeConsulta`, en el proyecto de servicios de la consulta; recortado a una
función):

```csharp
public sealed class HerramientasDeConsulta(IFuenteDeConsulta fuente, ContextoDeConsulta contexto)
{
    public const int TamanioDePagina = 20, PaginaMaxima = 5;

    [HerramientaDeSoloLectura("mensajes_recientes",
        "Mensajes repartidos de la aplicación, del más nuevo al más viejo, 20 por página. estado: Todos, Programado, Enviando, Enviado o Cancelado. pagina: de 1 a 5.")]
    public Task<RespuestaDeHerramienta<PaginaDeMensajes>> MensajesRecientesAsync(EstadoFiltro estado = EstadoFiltro.Todos, int pagina = 1)
    {
        if (!Enum.IsDefined(estado) || pagina is < 1 or > PaginaMaxima)
            return Task.FromResult(RespuestaDeHerramienta<PaginaDeMensajes>.ArgumentoInvalido);
        var filtro = estado == EstadoFiltro.Todos ? null : estado.ToString();
        return Proteger(async () => RespuestaDeHerramienta<PaginaDeMensajes>.Con(
            await fuente.MensajesAsync(contexto.AppId, filtro, pagina, TamanioDePagina)));
    }
}
```

Qué mirar: la aplicación sale de `contexto`, no de la firma (R2); el rango se valida antes de la fuente (§10.3);
`Proteger` convierte cualquier excepción en `no_disponible` (§10.5).

Extracto de PushDispatch (`ConsultaConModelo`, adaptador sobre `IChatClient`; recortado):

```csharp
_modelo = new ChatClientBuilder(modelo)
    .UseFunctionInvocation(logs, f =>
    {
        f.MaximumIterationsPerRequest = opciones.MaxLlamadas;
        f.AllowConcurrentInvocation = false;
        f.IncludeDetailedErrors = false;
    })
    .Build();
// …por pedido:
var herramientas = new HerramientasDeConsulta(_fuente, contexto);
Tools = [.. MetodosDelCatalogo().Select(m => (AITool)conversacion.Envolver(Crear(m, herramientas)))];
// …al recibir la respuesta:
if (llamadas.Any(c => !NombresDelCatalogo.Contains(c.Name)))
    return Fin(ResultadoDeConsulta.Rechazado("herramienta_desconocida", contexto), conversacion, contexto);
var veredicto = ValidadorDeCifras.Validar(texto, conversacion.Resultados, pregunta, conversacion.Argumentos);
```

Qué mirar: el catálogo se crea por pedido sobre el contexto (§11.1), el tope está en la invocación y en el
envoltorio contador (§11.3), el nombre desconocido rechaza el turno (§11.5) y el validador recibe sólo los
argumentos de llamadas exitosas (§12.2).

---

## Parte C — Destinos y deeplinks

<a id="k6-14"></a>
## 14. Catálogo tipado y su conformidad

La norma está en [K2 §5](knowledge-AI-Layer-Console-Assistant-Building.md#k2-5); acá, la forma.

1. **Un registro tipado por destino** (en PushDispatch `DefinicionDeDestino`): `Id` estable y versionado
   (`aplicacion.suscripciones`), `Rotulo` («Ir a {Rotulo}»), `Plantilla` de ruta relativa sin «/» inicial (para
   que funcione bajo un `PathBase`) con `{appCode}` o `{CuentaSlug}`, `Politica` (**la misma** del `[Authorize]`
   de la página), `Alcance` (global, cuenta, aplicación), `Claves` (cómo la nombra la gente), `Filtros` (lista
   blanca de parámetros de consulta) y `Consulta` fija opcional (`alta=1`). Los ids, como constantes; el
   enumerado de pantallas que había antes se reemplazó sin compatibilidad.
   - **Una sola política por destino: la de ver.** `Politica` es la del `[Authorize]` de la página, o sea el
     permiso de **ver**. El permiso de **hacer** la acción que abre `Consulta` no es un campo del registro: lo
     decide el criterio único de §16 (`PuedeCrear` en PushDispatch), por instancia cuando corresponde. Separar
     `PoliticaDeVer` y `PoliticaDeHacer` en el registro es una propuesta válida para otro sistema, no la práctica
     de PushDispatch; si la adoptás, la conformidad de §14.3 compara las dos.
   - **`Alcance` y la «cuenta» son del dominio.** Los valores (global, cuenta, aplicación) son los de
     PushDispatch; en otro sistema son su unidad de inquilino (sede, club, usuario). El principio no cambia: el
     código elige la unidad entre las de la persona, nunca el modelo ni la URL (§15.2).
   - **Id de instancia** (abrir *esa* reserva, *ese* mensaje): PushDispatch no lo tiene en el registro. Propuesta
     (NO VERIFICADO): un campo `Instancia` con el nombre del parámetro (`reserva`), cuyo valor lo pone **el
     código** desde el resultado de una función exitosa del turno (nunca del texto), validado como id `> 0`, y
     que la página revalida por instancia con el criterio de K2 §5.6: ajena o inexistente → «no encontrado».
2. **Validá el catálogo al arrancar:** plantilla insegura, id duplicado o ruta que no corresponde a una `@page`
   cortan el arranque.
3. **Conformidad por reflexión, en la CI** (cada una con su par rojo/verde):
   - cada `Plantilla` coincide con una `@page` real;
   - cada `Politica` es la del `[Authorize]` de esa página;
   - cada destino con `Consulta` (acción) tiene su criterio de hacer de §16, y la prueba por rol confirma que
     botón, deeplink y destino responden lo mismo;
   - cada `Filtro` y cada clave de `Consulta` se lee en la página con `[SupplyParameterFromQuery]`;
   - cada id de `destinos:` de la ayuda vigente existe en el catálogo.
   *Porqué:* que el catálogo nombre un parámetro que la página no lee, o una política más laxa, pasa todas las
   pruebas de unidad y termina en una pantalla sin filtro o en un 403. En PushDispatch los destinos de la ayuda
   real se validaron contra el catálogo en un PR propio, y la conformidad de filtros llegó con la apertura de
   Auditoría filtrada desde el asistente.
4. **Lo que registra otro módulo se registra igual que en su `Program.cs`**, y la conformidad corre sobre el
   catálogo completo. *Porqué:* en PushDispatch las cuentas y organizaciones llegaban de otro expediente.

<a id="k6-15"></a>
## 15. Destino por intención y por cuenta elegida

1. **La intención reordena, no inventa:** de los `destinos:` que la página declara, va primero el que corresponde
   a la intención (alta/crear/nueva → `*.nueva`; ver/listar/mis → `*.lista`; invitar/miembros → miembros de la
   cuenta). Sin intención clara, o si la página no declara ese destino, rige el orden de la página. Cada id sigue
   pasando por el catálogo y el permiso. *Porqué:* antes se ofrecía siempre el primero de `destinos:` y «¿cómo
   doy de alta una aplicación?» llevaba a la lista. *Prueba:* tabla `pregunta + destinos declarados → orden`.
2. **La cuenta del destino la elige el código**, siempre entre las cuentas de la persona, en este orden: la
   elegida en la aclaración, la dueña de la app del chip, la única donde puede abrir la pantalla. Si quedan
   varias y la cuenta cambia la respuesta ([K5 §M.4](knowledge-AI-Layer-Conversational-Design.md#k5-m4)), aclaración con el nombre humano de cada cuenta;
   si sólo cambia el enlace, el chip usa la cuenta activa (o abre un selector en la pantalla) y lo dice.
   *Prueba:* con dos cuentas: con chip va a la de la app; sin chip y con cifra que cambia, aclaración; pregunta conceptual, sin aclaración; elegida, esa; ajena, mal formada
   o con otras mayúsculas, ignorada; una sola posible, no se pregunta; la cuenta del chip sin permiso no deriva a
   otra cuenta.
3. **La cuenta elegida viaja por el servidor**, no por la URL armada en el cliente: llega a la autorización
   (`PuedeAbrirAsync(usuario, destino, app, cuentaSlug)`) y a `{CuentaSlug}`. Nada de la pregunta ni del modelo
   entra en la ruta.

<a id="k6-16"></a>
## 16. Revalidación en el servidor y ver ≠ hacer

Rigen las reglas 2, 3 y 6 de [K2 §5](knowledge-AI-Layer-Console-Assistant-Building.md#k2-5) (revalidar en el
turno, el permiso de la acción y no el de la página, permiso por instancia). Lo que este conocimiento agrega:

- **Un solo lugar decide si se puede hacer la acción**, y lo usan los tres: el botón visible, la apertura por
  deeplink y la autorización del destino. *Porqué:* en PushDispatch el botón «Nueva aplicación» se mostraba a
  todos, pero el alta del servidor sólo la permitía el raíz: el administrador de aplicación recibía 403 al
  confirmar. Se centralizó en un `PuedeCrear` que usan botón, `?alta=1` y destino. *Prueba:* para cada rol, las
  tres respuestas coinciden.
- **Sin permiso de la acción, no se ofrece la pantalla de al lado:** la consola dice quién puede, sin enlace («En
  {cuenta} crean aplicaciones el owner o el titular»). Es la negativa útil de
  [K5](knowledge-AI-Layer-Conversational-Design.md#k5-3), y evita [K2-AP-9](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-9).

<a id="k6-17"></a>
## 17. Enlace a la acción exacta

1. **El deeplink abre la acción**, no sólo la página: un destino propio para la acción (`aplicaciones.nueva` =
   ruta de la lista + `Consulta = "alta=1"`), validado como pares `clave=valor` en minúsculas y dígitos.
2. **La página lee el parámetro con `[SupplyParameterFromQuery]` y abre el modal una sola vez**, sólo si el
   criterio de §16 lo permite; con el parámetro y sin permiso, la página carga normal, sin modal ni error.
   *Prueba:* bUnit con y sin permiso; recargar después de cerrar no lo reabre.
3. **Los filtros** que la consola pone (`?via=`, estado, rango) salen de la lista blanca `Filtros` del destino y
   siguen la convención de estado en la URL de [K2 §16 E-2](knowledge-AI-Layer-Console-Assistant-Building.md#k2-16).
4. **Si la acción que el chip debe abrir no tiene parámetro, se agrega uno de valor fijo** (`?pistas=1`,
   `?ejemplo=1`) que la página lee con `[SupplyParameterFromQuery]`; la conformidad de §14.3 lo contrasta al
   arrancar. Un fragmento `#id` sólo lleva a la sección (no abre ni enfoca) y no lo ve la conformidad: si se usa
   mientras tanto, va con prueba de presencia del `id` y se declara como deuda de la especificación. *Porqué:* en
   [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria) «Ver pistas» y «Ver un ejemplo» llevaban sólo a la
   pantalla (deudas D-2 y D-3) hasta EXP-0005, que agregó los dos parámetros. *Prueba:* bUnit con el parámetro →
   la acción abierta y con el foco en su control; sin nada que abrir → DOM igual al de sin parámetro
   ([K2 §5.9](knowledge-AI-Layer-Console-Assistant-Building.md#k2-5)).

<a id="k6-18"></a>
## 18. Nunca una ruta del texto

- Quitá del texto del modelo **toda URL y toda ruta** antes de mostrarlo; el destino es el chip que arma el
  código ([K2 R5](knowledge-AI-Layer-Console-Assistant-Building.md#k2-r5)).
- «Llevame a …» se resuelve contra las `Claves` del catálogo (palabras normalizadas), nunca copiando la ruta
  escrita. *Prueba:* «llevame a /usuarios-del-panel» como administrador de aplicación → sin destino; «llevame a
  usuarios» → el destino si el rol lo puede abrir.
- Un enlace dentro de la ayuda es para la persona que lee la página, no para la consola: la consola sólo usa
  `destinos:`. *Prueba:* página con un enlace a una ruta que no está en `destinos:` → la consola no lo ofrece.

<a id="k6-19"></a>
## 19. Procedimiento paso a paso

**Página de ayuda nueva o cambiada**
1. Confirmá en K1 que la tarea está en la frontera y qué roles la ven.
2. Escribí la página según §3 con `status: borrador`; frontmatter según §4.
3. Sumá sus casos (preguntas del frontmatter, reales y una no respondible cercana) al conjunto de §7.
4. Corré la medición completa; si una página vieja pierde casos, aplicá §8 antes de seguir.
5. Pasala a `vigente` cuando la revise quien conoce la pantalla y la captura confirme los rótulos.

**Función nueva**
1. Decidí y escribí quién elige (§9). 2. Diseñala según §10. 3. Registrala por el atributo (§11). 4. Agregá sus
pruebas: rango, sin argumento de app, error como dato, canario, tope, nombre desconocido (K7). 5. Si la elige el
modelo, casos en la batería de K3.

**Destino nuevo**
1. Registrá la definición (§14.1) con la política de la página. 2. Corré la conformidad (§14.3). 3. Si abre una
acción, el parámetro y el criterio único (§16, §17). 4. Declaralo en `destinos:` de la página que lo explica.
5. Casos de intención y de cuenta (§15).

<a id="k6-20"></a>
## 20. Criterios de parada

Pará y volvé a K1 (o a la mesa de K4) si:
- la página necesita un dato vivo para tener sentido (es una función, no ayuda);
- la función necesita la aplicación o la cuenta como argumento para servir (falta resolver el alcance en el
  código, K2 §3 paso 2);
- la función tendría que escribir;
- el destino no tiene una `@page` con política, o su acción no tiene un criterio único de permiso;
- el recall@3 de un rol cae debajo del umbral y el arreglo de §8 no alcanza: es falta de corpus, no de ranking
  ([K2-AP-8](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-8)).

<a id="k6-21"></a>
## 21. Checklist binario

Cada ítem es sí/no; un «no» impide cerrar el PR.

- [ ] Ninguna página con `status: borrador` aparece en una consulta (R1).
- [ ] Cada página con pasos tiene `##` por intención y nada esencial en la introducción (§3).
- [ ] El frontmatter pasa el esquema y cada id de `destinos:` existe en el catálogo (§4, §14).
- [ ] Cada pregunta de `preguntas:` es un caso de medición con su página esperada (§4, §7).
- [ ] Los bloques de rol se quitan antes de trocear (§3.6, §6.1).
- [ ] Trozos ≤ 500 caracteres por párrafo con rótulo página › sección (§6.2).
- [ ] El filtro por rol y alcance va antes del top-k (§6.6).
- [ ] Sin evidencia, la plantilla responde sin llamar al modelo (§6.7).
- [ ] Recall@3 por rol y tasa de «ninguna» en no respondibles medidos sobre el corpus completo (§7).
- [ ] Los casos de §8.1, §8.2 y §8.3 están en el conjunto y pasan (§8).
- [ ] Cada intención tiene escrito quién elige la función (§9).
- [ ] Ninguna función recibe aplicación, cuenta ni usuario (R2).
- [ ] Cada argumento fuera de rango devuelve `argumento_invalido` sin llamar a la fuente (§10.3).
- [ ] Ningún texto de terceros ni mensaje de excepción sale de una función (§10.4, §10.5).
- [ ] Nombre y descripción explícitos en `AIFunctionFactory`; tope en invocación y en el contador (§11).
- [ ] Un nombre de función desconocido rechaza el turno (§11.5).
- [ ] El validador no acepta argumentos de llamadas fallidas (§12.2).
- [ ] La lista de palabras comunes vive en un solo lugar y cada entrada tiene su caso de medición (§8.0).
- [ ] Cada página declara `delivery`; la omisión de `roles` está revisada en el PR (§4).
- [ ] Fechas, horas e ids de la respuesta tienen origen, si el dominio los usa (§12.6).
- [ ] La conformidad catálogo ↔ `@page` ↔ política ↔ `[SupplyParameterFromQuery]` pasa (§14.3).
- [ ] El destino se ordena por intención y la cuenta la elige el código entre las de la persona (§15).
- [ ] Botón, deeplink y destino usan el mismo criterio de permiso (§16).
- [ ] El deeplink de la acción abre el modal sólo con permiso y una sola vez (§17).
- [ ] Ninguna ruta del texto del usuario, del modelo o de un enlace de la ayuda produce destino (§18).

<a id="k6-22"></a>
## 22. Antipatrones con su control

| Id | Antipatrón | Qué costó en PushDispatch | Control |
|---|---|---|---|
| <a id="k6-ap-1"></a>K6-AP-1 | `preguntas:` del frontmatter como atajo que decide la página | «alta de una organización» → guía de aplicaciones | §8.1 evidencia distintiva |
| <a id="k6-ap-2"></a>K6-AP-2 | Títulos temáticos que comparten palabras comunes | «Segmentos de una aplicación» atrapaba el alta | §3.1 título de tarea y §8.2 |
| <a id="k6-ap-3"></a>K6-AP-3 | Puntuar o extraer la página entera o su primer párrafo | la plantilla contestaba con la introducción | §8.3 por sección |
| <a id="k6-ap-4"></a>K6-AP-4 | Dos intenciones en un mismo bloque | el alta del raíz mezclaba la asignación de administradores | §3.2 una intención por `##` |
| <a id="k6-ap-5"></a>K6-AP-5 | Sinónimo que dispara una intención por cualquier objeto | «registro de cambios» ofrecía un alta | §5 sinónimos condicionados y su par de casos |
| <a id="k6-ap-6"></a>K6-AP-6 | Marcar vigente para que una prueba pase | — (control preventivo) | §4 página sintética dentro de la prueba |
| <a id="k6-ap-7"></a>K6-AP-7 | La aplicación como argumento de la función | — (control preventivo; agencia excesiva) | R2 y su prueba por reflexión |
| <a id="k6-ap-8"></a>K6-AP-8 | El argumento de una llamada fallida respalda una cifra | «hubo 99999» con un id inexistente | §12.2 |
| <a id="k6-ap-9"></a>K6-AP-9 | El botón, el deeplink y el servidor deciden el permiso por separado | 403 al confirmar el alta | §16 criterio único |
| <a id="k6-ap-10"></a>K6-AP-10 | Ofrecer siempre el primer destino de la página | el alta llevaba a la lista | §15.1 destino por intención |
| <a id="k6-ap-11"></a>K6-AP-11 | Validar sólo números | Movilidad Urbana: nota invertida y restricción inventada sin cifras | §12.8 |
| <a id="k6-ap-12"></a>K6-AP-12 | «Idénticas salvo citas» sin normalizar el formato | «100 %[1]» pasó | §12.9 |

Los antipatrones de construcción que este conocimiento detalla siguen en K2:
[K2-AP-7](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-7) (respuesta como puntero, palabras sueltas),
[K2-AP-8](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-8) (falta de corpus diagnosticada como búsqueda)
y [K2-AP-9](knowledge-AI-Layer-Console-Assistant-Building.md#k2-ap-9) (destino que termina en 403).

<a id="k6-23"></a>
## 23. Casos

**PushDispatch (caso de origen, diseñado desde cero).** La ayuda del panel pasó a fuente recuperable con
`destinos:` y `preguntas:` en el frontmatter (PR #465); el catálogo tipado de destinos reemplazó al enumerado de
pantallas (PR #466) y los destinos de la ayuda real se validaron contra él (PR #467); Auditoría se abre filtrada
desde el asistente con la conformidad de filtros (PR #473); el destino por intención y la cuenta elegida
llegaron en el PR #480, y el alta por enlace (`?alta=1` con un criterio único de permiso) en el PR siguiente. La
redacción anclada sobre ese corpus fue de 19/20 en una corrida a 21/33 ×3, a 27/30 ×3 con trozos de 500
caracteres y recorte por párrafos, y se aceptó con 36/36 en cada una de tres corridas (K3). La consulta con
modelo usa las cuatro funciones de lectura de `HerramientasDeConsulta` (métricas, mensajes recientes, informe y
serie de un mensaje) con tope de llamadas, rechazo de nombres desconocidos y validador de cifras.

**Movilidad Urbana (demo del libro).** [Guía §15](../Dot-Net-Integration-AI-Layer-Guide.md#c15). Las funciones de
sólo lectura sobre localidades cerradas sobre la sesión del visitante son §10 en limpio (la sesión aísla, el
argumento no la nombra); la localidad con instrucciones en el nombre
[Guía §12.7](../Dot-Net-Integration-AI-Layer-Guide.md#c12-7) es el canario de §10.4. No tiene panel con roles ni
catálogo de pantallas: la Parte C no aplica tal cual.

**Geometría (caso aplicado del libro; el repo [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria) no tiene todavía la ayuda Docs as Code ni variantes `::: roles`:
lo que sigue describe el libro).** [Guía §16](../Dot-Net-Integration-AI-Layer-Guide.md#c16). Para conceptos del dominio
la unidad es la **ficha de concepto**: `contexto` opcional y cobertura por términos del dominio en lugar de por `@page`;
el validador de cifras de §12 vale para las pistas, y en las fichas la regla es ninguna cifra (K1 F-9). La ayuda se
genera como Docs as Code [Guía §16.8](../Dot-Net-Integration-AI-Layer-Guide.md#c16-8): el esquema del
frontmatter, los destinos válidos y el recall de §7 son chequeos de CI. La «función» que da la cifra es el
validador de la figura: decide qué está mal y cuánto, y el modelo sólo redacta la pista; la Parte B se aplica con
el validador como única fuente de números (§12). Dos roles, alumno y docente, con variantes por rol en la misma
página (§3.6).

<a id="k6-24"></a>
## 24. Límites

- Se destiló de **un** sistema; 500 caracteres, RRF k = 60, pesos 4/3/2/1, top 3, 800 tokens, 20 por página y 5
  páginas son puntos de partida medidos allí, no constantes: cada sistema los vuelve a medir con §7 y K3.
- Cubre lectura y navegación (escalón 0); las funciones que escriben siguen
  [Guía §9.5](../Dot-Net-Integration-AI-Layer-Guide.md#c9-5).
- El léxico con raíces de seis letras y sinónimos a mano funciona en español para un dominio chico; con otro
  idioma o un vocabulario grande, medí antes de copiarlo.
- La caché de embeddings de PushDispatch es en memoria (se recalcula al reiniciar); una persistente es una
  decisión pendiente, no una regla.
- No fija proveedor ni modelo de embeddings ni de redacción.
- Fechas y horas como argumento (§10.3), horas y fechas en el validador (§12.6) y el id de instancia del destino
  (§14.1) son extensiones para dominios con agenda o instancias, no medidas en PushDispatch: se aceptan con su
  prueba en el sistema que las adopte.

<a id="k6-glosario"></a>
## Glosario

- **Corpus vigente:** las páginas con `status: vigente`; lo único que se indexa.
- **Destino:** entrada del catálogo `id → plantilla, política, alcance, filtros, consulta fija`.
- **Evidencia léxica:** toda palabra distintiva de la consulta presente en la página y alguna en un lugar fuerte.
- **Lugar fuerte:** título, encabezado, negrita o `preguntas:`.
- **No respondible:** pregunta del conjunto de medición cuya respuesta esperada es «ninguna página».
- **Palabra común:** raíz de la lista cerrada de §8.0 («alta», «aplicación»): suma pero no es evidencia sola. Toda otra raíz que no es vacía es distintiva.
- **Recall@3:** fracción de preguntas cuya página esperada aparece entre las tres primeras.
- **K1–K5, K7:** conocimientos hermanos de esta carpeta.

<a id="k6-historial"></a>
## Historial

| Versión | Fecha | Cambio |
|---|---|---|
| 1.7 | 2026-11-12 | §15.2 remite a [K5 §M](knowledge-AI-Layer-Conversational-Design.md#k5-m): la cuenta del destino se pregunta sólo si cambia la respuesta; si sólo cambia el enlace, cuenta activa dicha. Estado aclarado: Borrador en cabecera, README e historial. |
| 1.6 | 2026-11-12 | Banco de diálogo sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): §17.4 acción sin parámetro → parámetro de valor fijo; el fragmento `#id` como deuda declarada. |
| 1.5 | 2026-11-11 | Lecciones de [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): §9 límite del ruteo por código, sonda y clasificador por embeddings como candidato. |
| 1.4 | 2026-11-11 | Rondas 2 y 3 del banco Movilidad Urbana: §12.8 afirmaciones sin números y entidades no cerradas; §12.9 formato de citas; §12.10 tasa de rechazo; K6-AP-11 y K6-AP-12. |
| 1.3 | 2026-11-11 | Banco integral sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor) (Movilidad Urbana): §12.7 cifra atada a su categoría y a su orden. |
| 1.2 | 2026-11-11 | Banco de traslado sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria): caso Geometría rotulado como del libro; ficha de concepto con `contexto` opcional y cobertura por términos. |
| 1.1 | 2026-11-11 | Tras la prueba a ciegas (sistema de reservas sintético, 24 huecos): gramática de `::: roles`; `delivery` como audiencia y qué pasa sin `roles` o sin `status`; frontmatter completo de ejemplo; ficha frente a página por tarea; cálculo de la raíz; calibración del umbral de coseno; §8.0 criterio de palabra común; qué se compara antes de pasar una intención al modelo; fechas y horas como argumento y como cifra con origen; una sola política (ver) y el hacer por el criterio de §16; `Alcance` del dominio; propuesta de id de instancia; criterios de frontera de la ayuda. |
| 1.0 | 2026-11-11 | Creación, destilada del corpus de ayuda, las funciones de consulta y el catálogo de destinos de la consola de PushDispatch (expediente de la capa de IA, PR #465 a #480 y el alta por enlace); detalla los pasos 5 y 8 del pipeline de K2. |
