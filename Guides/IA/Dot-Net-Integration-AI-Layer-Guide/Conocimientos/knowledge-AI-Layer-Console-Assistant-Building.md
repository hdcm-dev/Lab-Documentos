# Capa de IA integrada en .NET: cómo se construye el asistente de consola sobre un panel Blazor

**Alias:** AI-Layer-Console-Assistant-Building
**Naturaleza:** propio
**Tema:** Cómo se construye un asistente de consola dentro de un panel de control Blazor Interactive Server: pipeline determinista (alcance, rol, herramienta, contexto, redacción, validación de cifras, destino) donde el código elige y el modelo sólo redacta; corpus de ayuda filtrado por rol con su cobertura como compuerta; deeplinks del catálogo revalidados por permiso; elección entre candidatas; votos con motivo cerrado persistidos sin texto; rol raíz explícito; despliegue con un solo modelo cargado y aviso si la IA queda apagada. Destilado de la construcción de la consola de PushDispatch, diseñada desde cero
**Consumidor:** un agente IA (o una persona) que implementa la consola de IA en un panel existente o nuevo, ya especificada con K1; quien revisa un PR que toca su enrutado, su corpus, sus destinos, sus votos o su despliegue
**Condicion-de-carga:** al implementar la consola o tocar su enrutado, corpus, destinos, votos o despliegue; después de tener la especificación de K1; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** AI-Layer-Integration-Designing-Specification (K1): la especificación manda; este conocimiento no redefine requisitos, los implementa
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` 1.2.0 (libro de esta carpeta; las remisiones `[Guía §x]` usan esa numeración y sus anclas `cN-k`); [K3](knowledge-AI-Layer-Verification-And-Acceptance.md); [K4](knowledge-AI-Layer-Integration-Working-Method.md); [K5](knowledge-AI-Layer-Conversational-Design.md) (borrador provisorio); [K6](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md) (detalle de corpus, funciones y destinos); [K7](knowledge-AI-Layer-Testing-Strategy.md) (cómo se escriben las pruebas que este conocimiento pide); guía de diseño conversacional (en redacción: sus remisiones se agregan al publicarse). Opcionales, si el repo anfitrión los tiene (lo que este conocimiento necesita de ellos está resumido en §16): Template-Blazor-Interactive-Server-Panel-De-Control, Bundle-JS, Parametros-De-Sistema, Release-Y-Despliegue-Versionado
**Versión:** 1.2
**Estado:** Borrador (pasa a Vigente cuando K3 lo acepte aplicado sobre un segundo sistema)
**Fecha:** 2026-11-11

---

## Índice

- [0. Propósito y mapa de carga](#k2-0)
- [1. Cuándo usarlo y cuándo no](#k2-1)
- [2. Reglas que no se negocian](#k2-2)
- [3. El pipeline de un turno](#k2-3)
- [4. Corpus de ayuda: existencia, cobertura y filtro por rol](#k2-4)
- [5. Destinos y deeplinks revalidados](#k2-5)
- [6. Elegir entre candidatas](#k2-6)
- [7. Visualizaciones](#k2-7)
- [8. Votos persistidos sin texto](#k2-8)
- [9. Rol raíz explícito](#k2-9)
- [10. Despliegue: un modelo cargado y la IA apagada a la vista](#k2-10)
- [11. Procedimiento paso a paso](#k2-11)
- [12. Checklist binario de completitud](#k2-12)
- [13. Antipatrones con su control](#k2-13)
- [14. Casos](#k2-14)
- [15. Límites](#k2-15)
- [16. Reglas del panel que este conocimiento necesita (resumen autocontenido)](#k2-16)
- [Glosario](#k2-glosario)
- [Historial](#k2-historial)

<a id="k2-0"></a>
## 0. Propósito y mapa de carga

Este conocimiento dice **cómo se construye** la consola que K1 especificó, para que un agente que nunca vio
PushDispatch la levante en otro panel y el resultado no se distinga del que hizo ese equipo: mismas decisiones,
mismas pruebas, mismos controles contra los errores que a ese equipo le costaron. **Qué** contesta el asistente
y **con qué forma** lo decide K5 (y la guía de diseño conversacional); **cómo se demuestra** que funciona, K3.
Acá está la implementación de ambos.

No repite lo que ya está escrito en otro lado; lo cita:

| Tema | Dónde está | Qué se toma de ahí |
|---|---|---|
| Requisitos `REQ-IA-nn`, niveles de agencia, OWASP | K1 [§9](knowledge-AI-Layer-Integration-Designing-Specification.md#k1-9), [§10](knowledge-AI-Layer-Integration-Designing-Specification.md#k1-10) | lo que se implementa y su prueba |
| Página, render mode, estado en URL, bUnit | [§16 E-1, E-2](#k2-16) | la consola es un componente del panel; no se rehace la estructura |
| Hoja de estilos y módulo JS de la consola | [§16 E-3](#k2-16) | cómo se linkea y cómo se comprueba (ver [K2-AP-1](#k2-ap-1)) |
| Endpoint, tiempos, umbrales y textos configurables | [§16 E-4](#k2-16) | dónde vive cada parámetro que no es secreto |
| Versión, etiqueta, despliegue, verificación post-release | [§16 E-5](#k2-16) | el ciclo; acá sólo lo propio de la IA |
| Formas de respuesta, negativa útil, candidatas, gráficos | [K5](knowledge-AI-Layer-Conversational-Design.md) | qué forma toma cada respuesta (F1–F8) |
| Frontera de competencia, anillos, señales sin texto | [Guía §3.7](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7) | criterio de cada rama del pipeline |

**Mapa de carga.** Cargá §0–§3 siempre; después sólo la sección del tema que tocás.

| Situación | Secciones |
|---|---|
| Arrancar la consola en un panel | §0–§3, §11, §12 |
| Tocar el enrutado o el alcance | §2, §3 |
| Sumar o revisar la ayuda (RAG) | §2, §4 y [K6 Parte A](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-3) |
| Tocar un destino o un deeplink | §2, §5 y [K6 Parte C](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-14) |
| Agregar o cambiar una función del modelo | §2, §3 y [K6 Parte B](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-9) |
| Escribir las pruebas de lo que tocaste | [K7](knowledge-AI-Layer-Testing-Strategy.md#k7-0) |
| La consola no encuentra la entidad que nombran | §6 |
| Agregar un gráfico | §7 |
| Votos, señales, métricas de la frontera | §8 |
| Agregar o cambiar un rol | §9 |
| Desplegar o cambiar de modelo | §10, §13 |
| Revisar un PR de la consola | §12, §13 |

**Criterio de parada general.** Si para avanzar hace falta decidir algo que K1 no especifica (una herramienta
nueva, un anillo distinto, un rol nuevo), no lo decidas acá: volvé a K1 y actualizá la especificación primero.

<a id="k2-1"></a>
## 1. Cuándo usarlo y cuándo no

**Usalo** cuando:
- el producto tiene un panel Blazor Interactive Server con roles y la especificación de K1 está cerrada (nivel
  de agencia, herramientas de lectura, anillos, requisitos con prueba nombrada);
- la consola responde sobre **datos propios del producto** (cifras por herramienta) y **sobre cómo usarlo**
  (ayuda por RAG), y lleva a una pantalla.

**No lo uses** cuando:
- no pasó por K1: sin especificación, este procedimiento produce una demo, no una capa;
- la consola tiene que **escribir** (escalón 1 o más con aprobación): este conocimiento cubre lectura y
  navegación; la escritura sigue [Guía §9.5](../Dot-Net-Integration-AI-Layer-Guide.md#c9-5) y requiere su propia mesa;
- el asistente no vive en un panel con sesión (un chat público): faltan rol y permiso, que son la base de §4 y §5.

**Roles de personas externas** (clientes finales, pacientes, ciudadanos con portal y sesión propia): quedan
**fuera** de este procedimiento salvo que la especificación de K1 los trate como un rol más, con su propia fila en
la matriz de §9, su corpus, sus destinos y sus casos de batería. *Porqué:* el panel de PushDispatch sólo tenía
roles internos; un rol externo cambia qué datos son ajenos y qué es Afuera. *Prueba:* si hay rol externo, la
especificación tiene su fila en la matriz de roles y K3 lo cubre en una celda de la batería.

<a id="k2-2"></a>
## 2. Reglas que no se negocian

Cada regla trae su porqué y la prueba que la vigila. La prueba es de tipo *garantía* (sin servidor de modelos,
en cada build) salvo que diga otra cosa [K1 §9].

<a id="k2-r1"></a>
**R1. El código elige herramienta, alcance y destino; el modelo sólo redacta sobre lo ya calculado.**
- *Porqué:* con un modelo chico el llamado a herramientas elegía mal herramienta y alcance, y era lento al
  primer token; el ruteo determinista se prueba con casos y no varía entre corridas. La frontera es decisión del
  código [Guía §3.7.4](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-4), las cifras salen del carril aparte
  [Guía §7.1](../Dot-Net-Integration-AI-Layer-Guide.md#c7-1) y la salida del modelo es una propuesta
  [Guía §14.9](../Dot-Net-Integration-AI-Layer-Guide.md#c14-9).
- *Prueba:* tabla de casos `pregunta → (herramienta, alcance, destino)` sobre el enrutador puro, sin modelo; el
  doble de `IChatClient` [Guía §11.2](../Dot-Net-Integration-AI-Layer-Guide.md#c11-2) verifica que el mensaje al
  modelo no incluye definiciones de herramientas.

<a id="k2-r2"></a>
**R2. «No está en la ayuda» antes que inventar.**
- *Porqué:* una abstención orienta; un dato inventado destruye la confianza y en PushDispatch pasó una batería
  al 95 % con uno adentro [K3]. Es una de las tres señales de la frontera [Guía §3.7.5](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-5).
- *Prueba:* con recuperación vacía o bajo umbral, la respuesta es la plantilla fija y no se llama al modelo.
- «No está en la ayuda» nombra la plantilla, no un texto literal: cada proyecto fija el suyo (en PushDispatch,
  «No encontré esto en la ayuda…» sin resultados y «No lo encontré en la ayuda» cuando el modelo se abstiene)
  [K5 §2](knowledge-AI-Layer-Conversational-Design.md#k5-2).

<a id="k2-r3"></a>
**R3. Toda cifra de la respuesta tiene origen en una herramienta del turno; si no, texto fijo.**
- *Porqué:* garantía por corrida, no estadística.
- *Prueba:* validador de cifras sobre la salida con una respuesta doble que mete un número ajeno → se reemplaza.

<a id="k2-r4"></a>
**R4. El filtro por rol va antes del top-k, nunca después.**
- *Porqué:* filtrar después deja huecos (k resultados de los que se descartan casi todos) y expone por el ranking
  la existencia de contenido ajeno [Guía §8.4](../Dot-Net-Integration-AI-Layer-Guide.md#c8-4).
- *Prueba:* corpus con fragmentos de dos roles; consulta del rol A con k=3 devuelve 3 fragmentos del rol A.

<a id="k2-r5"></a>
**R5. El modelo nunca arma una ruta: el destino sale del catálogo por id y se revalida por permiso en el servidor.**
- *Porqué:* una ruta escrita por el modelo es una inyección esperando pasar («llevame a /usuarios»)
  [Guía §12.2](../Dot-Net-Integration-AI-Layer-Guide.md#c12-2).
- *Prueba:* las URLs libres del texto se eliminan; la pregunta inyectada no ofrece destino; un destino sin
  permiso para el rol no se ofrece.

<a id="k2-r6"></a>
**R6. Ninguna clave, endpoint ni nombre de modelo en registros, evidencias, respuestas ni repo.**
- *Porqué:* el repo y las evidencias circulan; lo que se imprime una vez se filtra [Guía §12.6](../Dot-Net-Integration-AI-Layer-Guide.md#c12-6),
  [Guía §13.5](../Dot-Net-Integration-AI-Layer-Guide.md#c13-5).
- *Prueba:* búsqueda de secretos sobre el diff y sobre la carpeta de evidencias antes de cada commit; el motivo
  de habilitación (§10) nombra la **clave** que falta, nunca su valor.

<a id="k2-r7"></a>
**R7. Toda señal que la UI emite tiene almacenamiento y lector definidos antes de implementarse.**
- *Porqué:* en PushDispatch el voto se diseñó y sólo terminaba en el log: se perdía (§8).
- *Prueba:* prueba de integración que vota y lee la fila persistida.

<a id="k2-3"></a>
## 3. El pipeline de un turno

Ocho pasos, en este orden. Los pasos 1–5 y 7–8 son código puro y se prueban sin modelo; el paso 6 es el único
que habla con el modelo. Corresponde al pipeline transversal de [Guía §6.4](../Dot-Net-Integration-AI-Layer-Guide.md#c6-4)
y al ADR-09 [Guía §14.10](../Dot-Net-Integration-AI-Layer-Guide.md#c14-10).

| # | Paso | Entrada → salida | Si falla |
|---|---|---|---|
| 1 | **Contexto de pantalla** | pantalla actual + chip quitable → pista de tema | se ignora la pista |
| 2 | **Alcance** | pregunta + sesión → cuenta / aplicación / global, o candidatas | §6: elegir entre candidatas; nunca «¿sobre qué aplicación?» si la pregunta admite una respuesta global |
| 3 | **Rol** | sesión → rol efectivo (incluido el raíz, §9) | sin rol, sin consola: error explícito |
| 4 | **Anillo e intención** | pregunta → anillo 1/2/3/afuera e intención [Guía §3.7.2](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-2) | entre dos anillos: una pregunta cerrada con opciones |
| 5 | **Herramienta o recuperación** | anillo 1 → herramienta de lectura con el alcance ya fijado; anillo 2 → RAG filtrado por rol (§4) | sin datos o bajo umbral → R2 |
| 6 | **Redacción** | cifras + fragmentos + plantilla → texto | sin modelo → plantilla de respaldo por pertinencia (§10) |
| 7 | **Validación** | texto → texto con cifras verificadas (R3), sin URLs libres (R5) | texto fijo |
| 8 | **Destino** | intención + rol → id del catálogo revalidado (§5) | sin destino; nunca uno que termine en 403 |

**Reglas del pipeline.**
- Resolvé el **alcance antes de responder** (paso 2). *Porqué:* «¿Cuántos suscriptores tengo?» contestaba
  «¿Sobre qué aplicación?» y la falla no era de herramientas sino de alcance. *Prueba:* para cada pregunta de
  cantidad, un caso con 0, 1 y varias aplicaciones; si la persona **no nombró** ninguna, con varias la respuesta
es el total con desglose ofrecido; si **nombró** una y coincide con varias, es la aclaración de §6
([K5 §5](knowledge-AI-Layer-Conversational-Design.md#k5-5)).
- Implementá el enrutador como **función pura** (sin DI, sin I/O). *Porqué:* se prueba por tabla y lo leen los
  críticos. *Prueba:* el proyecto de pruebas lo instancia sin host.
- **La ayuda es contexto, no respuesta.** No devuelvas el fragmento ni un puntero a la guía; pasalo al modelo
  para redactar. *Porqué:* la primera versión respondía con un enlace a la guía y emparejaba por palabras sueltas
  («alta»). *Prueba:* la respuesta nunca es igual a un fragmento ni consiste sólo en un enlace.
- La **plantilla del texto vive en el código** (voseo, sin primera persona del asistente, cita por oración); el
  criterio de esas formas es de [K5 §3](knowledge-AI-Layer-Conversational-Design.md#k5-3). *Prueba:* casos de
  conducta medidos por K3, cada uno con su forma esperada F1–F8.

<a id="k2-4"></a>
## 4. Corpus de ayuda: existencia, cobertura y filtro por rol

**Definición.** El *corpus de ayuda* es el conjunto de páginas de ayuda, troceadas e indexadas, que el RAG puede
recuperar. Su *cobertura* es la fracción de pantallas del panel (`@page`) con al menos una página de ayuda
publicada (no borrador) para cada rol que las ve.

<a id="k2-4-compuerta"></a>
**Regla: la cobertura es una compuerta previa al RAG.** No implementes ni ajustes recuperación hasta medir la
cobertura. *Porqué:* en PushDispatch se diagnosticó como fallo de búsqueda lo que era falta de corpus: no había
ninguna página para un rol entero, la mayoría estaba en borrador y al revisar la consola apenas unas 7 de 38
pantallas tenían ayuda. Aunque hubiera RAG, no había documento que recuperar. *Prueba:* script que cruza las
rutas `@page` con el índice de ayuda por rol y falla por debajo del umbral que fije K1 (sin umbral escrito, 100 %
de las pantallas del anillo 1).

**Ingesta** [Guía §8.2](../Dot-Net-Integration-AI-Layer-Guide.md#c8-2):
1. Troceá por encabezado `##` y, dentro, por párrafos de ≤ 500 caracteres. *Porqué:* trozos largos diluyen el
   embedding; la crítica de una ronda lo pidió y subió la batería.
2. Cada trozo lleva `rol`, `pantalla`, `id de destino` y `estado` como atributos filtrables
   [Guía §8.3](../Dot-Net-Integration-AI-Layer-Guide.md#c8-3). Los borradores no se indexan.
3. Búsqueda **híbrida** léxica + vectorial con **umbral de rechazo real** (que dispare R2), filtrada por rol
   antes del top-k (R4).
4. **Precalentá** los embeddings al arrancar con caché por hash de (modelo de embeddings + texto). *Porqué:* el
   primer pedido no paga la ingesta; cambiar de modelo invalida la caché sola. *Prueba:* segundo arranque sin
   cambios no recalcula ningún embedding.

**Medí la recuperación antes que la respuesta** [Guía §8.5](../Dot-Net-Integration-AI-Layer-Guide.md#c8-5): un
conjunto de preguntas con su trozo esperado y su recall@k, por rol. La batería de respuestas es de K3.

**Detalle en K6.** Cómo se redacta cada página para que se recupere (secciones por tarea, frontmatter, fichas,
sinónimos), la ingesta completa, la medición de recall@3 y los errores de ranking que costaron están en
[K6 §3–§8](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-3); esta sección queda como resumen y
no cambia de sentido.

**Docs as Code.** Si el producto genera la ayuda desde el repo, la cobertura es un chequeo de CI
[Guía §16.8](../Dot-Net-Integration-AI-Layer-Guide.md#c16-8).

<a id="k2-5"></a>
## 5. Destinos y deeplinks revalidados

**Definición.** Un *destino* es una entrada del catálogo `id → ruta, parámetros, permiso para ver, permiso para
hacer`. Un *deeplink* es la ruta resuelta de un destino, con los parámetros que abren la acción.

**Reglas.**
1. **Por id del catálogo, nunca texto del modelo** (R5). Al arrancar, validá el catálogo: una plantilla insegura
   o una ruta inexistente corta el arranque. *Prueba:* catálogo con una ruta que no corresponde a ningún `@page`
   → el host no arranca.
2. **Revalidá el permiso en el servidor en el turno**, no al armar el catálogo. *Porqué:* el permiso depende de
   la sesión y puede cambiar. *Prueba:* mismo destino para dos roles → sólo lo recibe el que tiene permiso.
3. **Distinguí ver de hacer.** Si la intención es una acción, el destino exige el permiso de la acción (el POST),
   no el de la página. *Porqué:* PushDispatch ofrecía «Ir a Credenciales» a quien podía ver la página pero no
   cargar la credencial: un destino que termina en 403 es una negativa disfrazada. *Prueba:* rol con permiso de
   lectura y sin permiso de escritura → la intención «cargar» no ofrece el destino y la respuesta usa la negativa
   útil ([K5 F6](knowledge-AI-Layer-Conversational-Design.md#k5-3)).
4. **El deeplink abre la acción**, no sólo la página (por ejemplo `?alta=1` que abre el modal de alta).
   *Porqué:* responder y llevar; un modal sin parámetro obliga a buscarlo. *Prueba:* bUnit sobre la página con el
   parámetro → el modal está abierto. Los filtros de la URL siguen la convención de [§16 E-2](#k2-16).
5. **Conformidad catálogo ↔ páginas por reflexión.** *Porqué:* que el catálogo nombre un parámetro que la página
   no lee pasa todas las pruebas de unidad. *Prueba:* para cada destino, sus parámetros existen como
   `[SupplyParameterFromQuery]` en la página de su ruta.
6. **Si el permiso de hacer depende de la instancia** (sólo lo propio, sólo lo de su sede o cuenta), el catálogo
   declara el permiso por rol y **el caso de uso de la acción lo revalida para esa instancia** antes de ofrecer el
   destino; el deeplink lleva el id de la instancia y la página lo vuelve a comprobar al abrir. *Porqué:* el
   permiso por rol dice «puede cancelar», no «puede cancelar este»; ofrecer la acción sobre algo ajeno es un 403
   disfrazado (y revela que existe). *Prueba:* mismo rol, dos instancias (propia y ajena) → sólo la propia recibe
   el destino de la acción; abrir el deeplink de la ajena a mano → «no encontrado», no 403.

**Detalle en K6.** El catálogo tipado, el destino por intención y por cuenta, la revalidación y el enlace a la
acción exacta se desarrollan en [K6 §14–§18](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-14);
las funciones que alimentan el paso de contexto, en [K6 §9–§13](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-9).
Cada *Prueba:* de esta sección se escribe según [K7](knowledge-AI-Layer-Testing-Strategy.md#k7-5).

<a id="k2-6"></a>
## 6. Elegir entre candidatas

**Cuándo.** El paso 2 encuentra más de una entidad que coincide con lo nombrado (apps de nombre parecido, la
misma app en dos cuentas) o ninguna exacta pero sí parecidas.

**Lógica (la forma del diálogo es de [K5 §5](knowledge-AI-Layer-Conversational-Design.md#k5-5) y de [Guía §3.7.2](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-2), «cuando el pedido cae entre dos…»):**
1. Implementá un **resolutor puro** `nombre + entidades visibles para el rol → exacta | candidatas | ninguna`.
2. Con candidatas, mostrá **de 2 a 4**, ordenadas por parecido, **con su cifra** relevante y «y k más» si sobran.
3. Si la entidad existe en **varias cuentas** visibles, la aclaración es entre cuentas antes que entre apps.
4. Sólo entidades visibles para el rol: nunca se revela por la aclaración que existe algo ajeno.
5. «No encuentro esa aplicación» sólo con cero candidatas, y con el destino a la lista.

*Porqué:* «No encuentro esa aplicación» sin candidatas era un callejón. *Prueba:* tabla de casos del resolutor
(exacta, 2 parecidas, 6 parecidas → 4 + «y 2 más», misma en dos cuentas, ajena al rol → ninguna).

<a id="k2-7"></a>
## 7. Visualizaciones

El criterio (torta sólo con 2 a 6 partes de un todo, si no barras; nota de métrica siempre) es de
[K5 §4](knowledge-AI-Layer-Conversational-Design.md#k5-4). La implementación:
- Agregá en el servidor: **las 5 mayores + «Otras (k)»**, y comprobá que la suma es **exactamente** el total que
  la herramienta devolvió. *Prueba:* propiedad sobre listas aleatorias: suma de porciones = total.
- Acompañá la cifra con la **nota de qué métrica es** (por ejemplo, que los suscriptores son dispositivos y no
  personas); nunca sumes métricas que no se suman (activos mensuales de varias apps).
- El gráfico es SVG inline del sistema de diseño, con texto alternativo que repite las cifras; no una librería
  externa por un solo gráfico ([§16 E-3](#k2-16)).

<a id="k2-8"></a>
## 8. Votos persistidos sin texto

**Definición.** Un *voto* es la señal útil / no útil sobre un turno, con un **motivo cerrado** que indica dónde
arreglar («No es lo que pregunté» → intención; «Falta información» → corpus; «Dato incorrecto» → herramienta o
validador) [Guía §3.7.5](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-5), [Guía §13.9](../Dot-Net-Integration-AI-Layer-Guide.md#c13-9).

**Reglas.**
1. **Persistí en una tabla**, no sólo en el log (R7). *Porqué:* el voto que sólo se registraba en el log se
   perdía; la mesa lo reconoció en su autocrítica.
2. **Guardá la huella del turno, nunca la pregunta ni quién**: intención, anillo, herramienta, destino, motivo,
   fecha, versión. *Porqué:* no hay enmascarador confiable de texto libre; lo que no se guarda no se filtra
   [Guía §12.4](../Dot-Net-Integration-AI-Layer-Guide.md#c12-4). *Prueba:* el esquema de la tabla no tiene
   columnas de texto libre ni de usuario.
3. **Ningún voto cambia el comportamiento solo.** El cambio lo decide una persona con umbral de repetición
   [Guía §3.7.5](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-5). *Prueba:* no hay lector del voto en el pipeline.
4. Registrá con la misma huella las otras dos señales: «No está en la ayuda» y la clasificación «afuera».
5. La tabla se crea por migración versionada del producto, no a mano.

<a id="k2-9"></a>
## 9. Rol raíz explícito

**Regla.** Decidí y escribí, para **cada** rol incluido el raíz (superadministrador), si tiene consola, y si la
tiene, qué adaptador, qué sugerencias y qué destinos. *Porqué:* en PushDispatch el raíz quedó excluido en el
layout, en Inicio, en los adaptadores, en el buscador y en el controlador, y las sugerencias le ofrecían
«¿Cómo creo una aplicación?» aunque no podía; lo reclamó el responsable del producto. El raíz no puede ser un
callejón ni un comodín que todo lo ve sin reglas.

**Prueba:** matriz rol × pantalla; por cada rol, una prueba que abre la consola, pregunta una sugerencia propia
y obtiene una respuesta no vacía con un destino permitido. Toda sugerencia de un rol pasa por la revalidación de §5.
La matriz va en la especificación (K1 §3, actores) y la prueba en el CI.

<a id="k2-10"></a>
## 10. Despliegue: un modelo cargado y la IA apagada a la vista

Lo general del ciclo está resumido en [§16 E-5](#k2-16) y en [Guía §13](../Dot-Net-Integration-AI-Layer-Guide.md#c13);
acá lo propio de la consola.

1. **Endpoint, modelo de redacción, modelo de embeddings y tiempos por variable de entorno**, con **un solo
   nombre de clave por dato** documentado en un solo lugar [Guía §13.5](../Dot-Net-Integration-AI-Layer-Guide.md#c13-5),
   [Guía §13.6](../Dot-Net-Integration-AI-Layer-Guide.md#c13-6). *Porqué:* el código exigía `IA__Modelo` y el
   despliegue configuró otra clave parecida; la IA quedó apagada en silencio. *Prueba:* prueba que lee las
   claves que el código exige y las compara con la plantilla de variables del despliegue.
2. **Habilitación con motivo exacto.** Al arrancar, la capa calcula `Habilitada | FaltaEndpoint | FaltaModelo |
   FaltaModeloDeEmbeddings | SinRespuesta…`, lo **registra una vez** (nombrando la clave, nunca el valor, R6) y lo
   **expone** en el health check y en la consola («La ayuda con IA está apagada: falta configurar X»).
   *Prueba:* arranque sin la variable → health con el motivo y la consola muestra el aviso.
3. **Sin modelo, la consola sigue útil:** plantilla de respaldo **por pertinencia** (el fragmento de ayuda y el
   destino que correspondan), no un texto de introducción genérico. *Prueba:* sin modelo, una pregunta del
   anillo 1 devuelve su cifra y su destino.
4. **Un modelo cargado a la vez.** Si el servidor de inferencia alterna redacción y embeddings, cada cambio
   recarga: en PushDispatch, entre unos 3 y 7,5 s por turno, porque el embedding de cada pregunta desalojaba al
   modelo de redacción. Diseñá para eso: embeddings precalentados (§4), consultas de embeddings agrupadas
   antes de la redacción, y medición que pague el cambio como la app (K3). *Prueba:* traza de un turno frío con
   el tiempo de recarga separado.
5. **Verificación post-despliegue en el sitio real con sesión por rol** (K3): la demo no mostró ni la IA apagada
   ni la plantilla equivocada; el sitio real sí.
6. **Estilos y módulo JS de la consola linkeados** según [§16 E-3](#k2-16) y comprobados en la app servida
   ([K2-AP-1](#k2-ap-1)).

<a id="k2-11"></a>
## 11. Procedimiento paso a paso

Cada paso termina con su artefacto; sin artefacto el paso no está hecho.

1. **Leé K1 y la especificación del producto.** Artefacto: lista de herramientas, anillos, roles, requisitos con
   prueba nombrada. Parada: si falta alguno, volvé a K1.
2. **Medí la cobertura del corpus** (§4). Artefacto: tabla pantalla × rol con ayuda sí/no. Parada: por debajo
   del umbral, escribir ayuda es el trabajo, no el RAG.
3. **Escribí el catálogo de destinos** con permisos de ver y de hacer (§5) y su validación de arranque.
4. **Escribí el enrutador puro y el resolutor de candidatas** (§3, §6) con sus tablas de casos.
5. **Escribí herramientas de lectura** sobre casos de uso existentes [Guía §9.4](../Dot-Net-Integration-AI-Layer-Guide.md#c9-4),
   con el alcance como parámetro obligatorio.
6. **Ingesta y recuperación** (§4) con filtro por rol previo y umbral; medí recall.
7. **Redacción, validador de cifras, eliminación de URLs** (R2, R3, R5) con doble de `IChatClient`.
8. **Componente de consola** en el panel según [§16 E-1](#k2-16): chip de contexto, sugerencias por
   pantalla y rol, votos (§8), visualización (§7). Linkeá estilos ([§16 E-3](#k2-16)).
9. **Matriz de roles con el raíz** (§9) y su prueba.
10. **Configuración y habilitación** (§10) con [§16 E-4](#k2-16) para lo que no es secreto.
11. **Entregá a K3**: batería real ×3, crítico, capturas a 1280 y 390 en claro y oscuro (cuatro pares por
    pantalla, K3 R6.1), sitio real por rol. No declares cerrado
    antes del informe de K3.
12. **PR con conformación automática** ([K4 §14](knowledge-AI-Layer-Integration-Working-Method.md#k4-14-externas)) y búsqueda de secretos (R6).

<a id="k2-12"></a>
## 12. Checklist binario de completitud

Cada ítem es sí/no; un «no» impide cerrar.

- [ ] Hay tabla de casos del enrutador `pregunta → (herramienta, alcance, destino)` que corre sin modelo (R1).
- [ ] El mensaje al modelo no lleva definiciones de herramientas (R1).
- [ ] Con recuperación vacía la respuesta es «No está en la ayuda» sin llamar al modelo (R2).
- [ ] El validador reemplaza una cifra sin origen (R3).
- [ ] El filtro por rol se aplica antes del top-k (R4).
- [ ] Las URLs libres del texto se eliminan y una ruta inyectada no produce destino (R5).
- [ ] La búsqueda de secretos sobre diff y evidencias da cero (R6).
- [ ] Las preguntas de cantidad con varias apps devuelven total con desglose ofrecido (§3).
- [ ] La cobertura del corpus por rol está medida y sobre el umbral (§4).
- [ ] Los borradores no se indexan y el segundo arranque no recalcula embeddings (§4).
- [ ] El catálogo inválido corta el arranque (§5).
- [ ] Un rol sin permiso de hacer no recibe el destino de la acción (§5).
- [ ] El deeplink de alta abre el modal (§5).
- [ ] La conformidad catálogo ↔ `[SupplyParameterFromQuery]` pasa (§5).
- [ ] El resolutor muestra 2–4 candidatas con cifra y «y k más», y nunca entidades ajenas al rol (§6).
- [ ] Las porciones de un gráfico suman el total exacto y llevan nota de métrica (§7).
- [ ] El voto queda en una tabla sin texto ni usuario, creada por migración (§8).
- [ ] Cada rol, incluido el raíz, tiene prueba de consola con respuesta no vacía (§9).
- [ ] Las claves que el código exige coinciden con la plantilla del despliegue (§10).
- [ ] Sin la variable del modelo, health y consola muestran el motivo exacto (§10).
- [ ] Sin modelo, una pregunta del anillo 1 devuelve cifra y destino (§10).
- [ ] La hoja de estilos de la consola se carga en la app servida (computed style verificado) (K2-AP-1).
- [ ] Si hay permiso de hacer por instancia, la instancia ajena no recibe el destino (§5.6).
- [ ] Si hay roles externos, tienen su fila en la matriz y su celda en la batería (§1).
- [ ] K3 entregó informe aceptado con batería ×3, capturas a 1280 y 390 en claro y oscuro y sitio real por rol.

<a id="k2-13"></a>
## 13. Antipatrones con su control

La tabla índice de todos los antipatrones de la integración está en K4 §8; acá viven los que se previenen al
construir.

| Id | Antipatrón | Qué costó en PushDispatch | Control |
|---|---|---|---|
| <a id="k2-ap-1"></a>K2-AP-1 | Estilos de la consola sin linkear o en una rama sin PR | la consola se veía bien en la demo y «a medias» en la app | captura de la **app servida**, no de la demo, y verificación del estilo calculado de un elemento de la consola antes de declarar (§16 E-3, K3) |
| <a id="k2-ap-2"></a>K2-AP-2 | Consola del raíz olvidada | el raíz quedó sin consola en cinco puntos; reclamo del responsable | §9: matriz rol × pantalla con prueba por rol |
| <a id="k2-ap-3"></a>K2-AP-3 | Variable del modelo faltante: IA apagada en silencio | una clave con nombre parecido dejó la consola sin modelo en el sitio real | §10.1–2: una clave por dato, prueba de claves, motivo exacto visible |
| <a id="k2-ap-4"></a>K2-AP-4 | Medir con el modelo ya cargado | latencia optimista; la batería no pagaba el cambio de modelo que paga la app | §10.4 y K3: medir frío y caliente por separado en las condiciones de la app |
| <a id="k2-ap-5"></a>K2-AP-5 | Claves impresas en un registro | — (control preventivo) | R6: el motivo nombra la clave, nunca el valor; nunca volcar la configuración entera; búsqueda de secretos antes del commit |
| <a id="k2-ap-6"></a>K2-AP-6 | Diseñar el gesto sin el destino del dato | votos que se perdían en el log | R7 y §8 |
| <a id="k2-ap-7"></a>K2-AP-7 | La respuesta es un puntero a la guía; búsqueda por palabras sueltas | respuestas que no contestaban y emparejaban por «alta» | §3 «la ayuda es contexto» y umbral real de rechazo (§4) |
| <a id="k2-ap-8"></a>K2-AP-8 | Diagnosticar como búsqueda lo que es falta de corpus | se ajustó recuperación sin documentos que recuperar | compuerta de cobertura (§4) |
| <a id="k2-ap-9"></a>K2-AP-9 | Destino que termina en 403 | «Ir a Credenciales» a quien no podía cargar | §5.3 ver ≠ hacer |
| <a id="k2-ap-10"></a>K2-AP-10 | Preguntar el alcance cuando hay respuesta global | «¿Sobre qué aplicación?» ante «¿cuántos suscriptores tengo?» | §3 alcance antes de responder |

Un 90 % reportado sin repetir la medición, y fases cerradas sin evidencia, son antipatrones de verificación:
están en K3.

<a id="k2-14"></a>
## 14. Casos

**PushDispatch (caso de origen, diseñado desde cero).** Servicio de notificaciones push con panel Blazor de
varios roles. La consola nació como fichas de búsqueda sobre la ayuda; tras leer un producto de referencia del
mercado y tabular la brecha, se rehízo con este pipeline: enrutado puro, alcance antes de responder, total con
desglose, aclaración entre candidatas y entre cuentas, deeplink que abre el alta, torta de las 5 mayores +
«Otras», votos con motivo cerrado persistidos sin texto, raíz con consola propia, habilitación con motivo
exacto. La redacción anclada dio 19/20 en una sola corrida (con un dato inventado que sólo detectó la lectura
humana); repetida ×3 cayó a 21/33 (64 %); con trozos de 500 caracteres, recorte por párrafos y un control de
anclaje más estricto llegó a 27/30 ×3, y se aceptó con 36/36 en cada una de tres corridas y 81/81 casos sin
falla (K3 R2.3). La verificación en el sitio real por rol, que encontró la IA apagada por una
clave mal nombrada y una plantilla de respaldo que devolvía la introducción.

**Movilidad Urbana (demo del libro).** Chat sobre localidades y recorridos [Guía §15](../Dot-Net-Integration-AI-Layer-Guide.md#c15).
Aplicación: el recorrido PD-1…PD-6 [Guía §15.3](../Dot-Net-Integration-AI-Layer-Guide.md#c15-3) es el enrutado
del §3; la localidad con instrucciones en el nombre [Guía §12.7](../Dot-Net-Integration-AI-Layer-Guide.md#c12-7)
es la prueba de R5; el dato exacto por el carril aparte es R3. No tiene panel con roles: §5 y §9 no aplican
tal cual (lo que haría falta en producción está en [Guía §15.6](../Dot-Net-Integration-AI-Layer-Guide.md#c15-6)).

**Geometría (caso aplicado del libro).** [Guía §16](../Dot-Net-Integration-AI-Layer-Guide.md#c16). La ayuda se
genera como Docs as Code [Guía §16.8](../Dot-Net-Integration-AI-Layer-Guide.md#c16-8): la cobertura del §4 es
un chequeo de CI y no una tarea manual. R1 se ve en limpio: el validador decide qué está mal y cuánto, y el
modelo sólo redacta pistas graduadas para el alumno o el borrador del comentario del docente; al modelo viajan
la figura, el campo y el sentido de la diferencia, no el trabajo del alumno (minimización, como §8). La matriz
del §9 tiene dos filas, alumno y docente; sin modelo, rige la plantilla del docente (§10.3).

<a id="k2-15"></a>
## 15. Límites

- Cubre lectura y navegación (escalón 0 de agencia); la escritura con aprobación no está construida acá
  [Guía §9.5](../Dot-Net-Integration-AI-Layer-Guide.md#c9-5).
- Se destiló de **un** sistema; las cifras (≤ 500 caracteres, 2–4 candidatas, 5 + «Otras», 3 a 7,5 s de recarga) son
  puntos de partida medidos allí, no constantes: cada sistema las vuelve a medir con K3.
- No fija proveedor ni modelo; asume un servidor de inferencia que puede tener un solo modelo residente.
- Las formas de respuesta están en [K5](knowledge-AI-Layer-Conversational-Design.md), que es borrador provisorio hasta que se publique la guía de diseño conversacional;
  si esa guía contradice algo de §3, §6 o §7, rige la guía y se actualiza este conocimiento.
- No reemplaza la mesa: cómo se organiza el trabajo (especialistas, refutador, rondas, coordinación) está en K4.


<a id="k2-16"></a>
## 16. Reglas del panel que este conocimiento necesita (resumen autocontenido)

Estas reglas vienen de conocimientos del workspace donde se hizo PushDispatch que **no están en este repositorio**.
Se resumen acá lo justo para que la consola se construya sin cargarlos; si el repo anfitrión tiene sus propias
normas para lo mismo, rigen las del anfitrión y estas sirven de control.

| Id | Regla (imperativo) | Porqué | Prueba |
|---|---|---|---|
| <a id="k2-e1"></a>E-1 | La consola es **un componente del panel** con el mismo modo de render interactivo del servidor que las páginas que la alojan; se monta una sola vez en el layout y recibe la pantalla actual como parámetro | dos montajes del mismo botón aparecieron cuando dos sesiones lo implementaron por separado; una consola con otro render mode pierde el estado del circuito | prueba bUnit del layout: un solo componente de consola, que recibe la ruta actual |
| <a id="k2-e2"></a>E-2 | **El estado de una pantalla que se puede compartir va en la URL**: cada filtro es un parámetro de consulta con nombre estable leído con `[SupplyParameterFromQuery]`; un filtro aplicado se ve como chip quitable; un valor inválido se ignora sin romper la página. El deeplink del catálogo usa esta misma convención | el deeplink sólo puede abrir lo que la URL sabe expresar; en PushDispatch se capturó cada estado (con filtro, chip, chip quitado, valor inválido) a 1280 y 390 | conformidad catálogo ↔ página (§5.5) y bUnit con un valor inválido → la página carga sin filtro y sin error |
| <a id="k2-e3"></a>E-3 | **La hoja de estilos y el módulo JS de la consola se enlazan desde lo que el layout ya carga** (la hoja del proyecto, o el `<Proyecto>.styles.css` si se usan estilos aislados `.razor.css`), con versión en la URL; nada de CSS ni JS sólo en la maqueta | en PushDispatch la consola se veía bien en la demo y a medias en la app porque la hoja no estaba enlazada (K2-AP-1) | en la app servida, el pedido de la hoja devuelve 200 y un script de navegador lee el estilo calculado de un elemento de la consola y lo compara con el esperado |
| <a id="k2-e4"></a>E-4 | **Los parámetros que no son secretos** (tiempos, umbrales, textos fijos, tamaño del top-k) viven en un solo lugar de configuración, con un nombre de clave por dato documentado; **los secretos** sólo por variable de entorno o almacén de secretos, nunca en el repo | una clave con dos nombres dejó la IA apagada en silencio (K2-AP-3) | prueba que compara las claves que el código lee con la plantilla documentada; búsqueda de secretos sobre el diff |
| <a id="k2-e5"></a>E-5 | **El despliegue sale de una versión etiquetada** y se verifica después en el sitio real: health con el motivo de habilitación y recorrido por rol (K3 §7); volver atrás es redesplegar la etiqueta anterior | la demo no mostró la IA apagada ni la plantilla equivocada; el sitio real sí | la actuación de despliegue cita la etiqueta, el health leído y el recorrido por rol |

<a id="k2-glosario"></a>
## Glosario

- **Alcance:** cuenta, aplicación o global sobre la que se calcula una cifra.
- **Anillo:** zona de la frontera de competencia [Guía §3.7.2](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-2).
- **Catálogo de destinos:** tabla `id → ruta, parámetros, permiso de ver, permiso de hacer`.
- **Cobertura del corpus:** fracción de pantallas con ayuda publicada por rol.
- **Huella del turno:** intención, anillo, herramienta, destino, motivo, fecha y versión; sin texto ni usuario.
- **Motivo de habilitación:** estado calculado al arrancar que dice por qué la IA está o no disponible.
- **Resolutor de candidatas:** función pura que devuelve la entidad exacta, candidatas o ninguna.
- **K1, K3, K4, K5:** conocimientos hermanos de esta carpeta (especificación, verificación, forma de trabajo,
  diseño conversacional).

<a id="k2-historial"></a>
## Historial

| Versión | Fecha | Cambio |
|---|---|---|
| 1.2 | 2026-11-11 | Remisiones mínimas a K6 (§4, §5, mapa de carga) y a K7 (pruebas); nada de 1.1 cambió de sentido. |
| 1.1 | 2026-11-11 | Tras la prueba a ciegas: §16 resume las reglas del panel tomadas de conocimientos externos (ya no son obligatorios); K5 existe y se enlaza; ids K2-AP-n; roles externos (§1); permiso de hacer por instancia (§5.6); total frente a aclaración (§3); cifras de batería y de recarga corregidas; capturas en claro y oscuro; anclas estables hacia K1. |
| 1.0 | 2026-11-11 | Creación, destilada de la consola de PushDispatch (expediente de la capa de IA) y coordinada con la mesa de la guía de diseño conversacional. |
