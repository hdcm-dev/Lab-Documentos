# Capa de IA integrada en .NET: cómo se verifica y se acepta antes de declararla cerrada

**Alias:** AI-Layer-Verification-And-Acceptance
**Naturaleza:** propio
**Tema:** Cómo se demuestra que una capa de IA funciona antes de declarar cerrada una fase: batería real contra el modelo repetida tres veces con umbral de 90 %, medida en frío y pagando el cambio de modelo como lo paga la aplicación; crítico con nota de 8 o más y rondas de ajuste, verificación independiente y crítica; lectura humana; capturas lado a lado a 1280 y 390; sitio real con sesión por rol; cifras contra SQL; latencia; criterios de parada; informe que separa lo MEDIDO de lo que queda como INFORME. Destilado de la integración de IA de PushDispatch y alineado con la guía de la capa de IA
**Consumidor:** un agente IA verificador o crítico de una mesa de integración de IA; el gestor que decide cerrar una fase; una persona que revisa una entrega de la capa
**Condicion-de-carga:** antes de declarar cerrada cualquier fase de la capa de IA; al armar una batería, un criterio de aceptación o el informe de una fase; al revisar un informe ajeno; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** —
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` (libro de esta carpeta, edición 1.2.0; las remisiones `[Guía §x]` usan esa numeración y sus anclas `#cN-M`); [K1](knowledge-AI-Layer-Integration-Designing-Specification.md) (los `REQ-IA-nn` son lo que este conocimiento mide); [K2](knowledge-AI-Layer-Console-Assistant-Building.md) (construcción de la consola); [K4](knowledge-AI-Layer-Integration-Working-Method.md) (forma de trabajo de la mesa, tabla índice de antipatrones y resumen del registro de frentes y del ciclo del PR en su §14); [K5](knowledge-AI-Layer-Conversational-Design.md) (qué forma debe tener cada respuesta: lo que la lectura humana juzga); [K7](knowledge-AI-Layer-Testing-Strategy.md) (cómo se escribe la suite y los scripts que producen esta evidencia); [K6](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md) (recall@3 del corpus, funciones y destinos que se miden). Opcionales, no hacen falta para aplicar este conocimiento: Knowledge-Razonamiento-Del-Gestor y Knowledge-Conformacion-Pull-Request-Automatico, si el repo anfitrión los tiene
**Versión:** 1.8
**Estado:** Vigente
**Fecha:** 2026-11-12

**Cambios en 1.8** (mesas de proveedores, consumo y MCP y de la ronda del dueño del producto sobre la consola de PushDispatch): R2.11 la batería ×3 es la compuerta para habilitar un modelo; R2.12 aceptación de consumo, cuotas y servidor MCP con cifras contra la fuente.

**Cambios en 1.7** (banco de diálogo sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria)): el id repetido R2.6 se separa (la batería antes de la firma pasa a R2.2b; la caída del servidor sigue como R2.6); R2.6b límite de tasa = no medido; R10.2b p95 del redactor con modelo frente al corte.

**Cambios en 1.6** (lecciones de [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria), enrutador del asistente de conceptos): R2.9 límite declarado del componente heurístico, casos conocidos en el informe y garantía en la configuración por omisión; K3-AP-14.

**Cambios en 1.5** (rondas 2 y 3 del banco Movilidad Urbana sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor)): R2.8 los controles de anclaje no miden calidad; compuerta «no peor que la fija» a ciegas, ejercida, y tasa de rechazo del validador en el informe; K3-AP-12 y K3-AP-13.

**Cambios en 1.4** (banco integral sobre [Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor)): R8.4 mismo redondeo en código y SQL, con medios exactos en el fixture.

**Cambios en 1.3** (banco de traslado sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria); nada de 1.2 cambió de sentido): R2.2a lectura experta cuando el oráculo no es decidible por código; R2.6 batería antes de la firma del corpus; R4.6 juez dueño distinto del crítico; caso Geometría corregido contra el repo.

**Cambios en 1.2** (nada de 1.1 cambió de sentido): remisiones mínimas a K7 para escribir la suite, los scripts de batería y de capturas y las compuertas de CI (mapa de carga, §2, §9), y a K6 para la medición del corpus.

**Cambios en 1.1** (tras la prueba a ciegas; nada de 1.0 cambió de sentido): R2.1a tamaño mínimo M ≥ 20 y forma esperada por caso; R2.3 cuenta la secuencia real de la batería (95 % en una corrida → 64 % ×3 → 90 % ×3 → 36/36 ×3); R3.1 y R3.3a, recarga del modelo único y configuración del servidor junto a cada latencia; R4.1, roles como subagentes distintos; R9.4, datos personales en las evidencias; R10.2a, presupuesto de latencia derivado del uso; ids K3-AP-n con ancla; enlaces a K1, K2, K4 y K5.

---

## Índice

- [0. Propósito y mapa de carga](#k3-0-proposito)
- [1. Principio: evidencia antes que «cerrado»](#k3-1-principio)
- [2. Batería real contra el modelo, ×3, umbral 90 %](#k3-2-bateria)
- [3. Condiciones de medición: en frío y pagando el cambio de modelo](#k3-3-condiciones)
- [4. Crítico con nota y rondas ajuste → verificación → crítica](#k3-4-critico)
- [5. Lectura humana de las respuestas](#k3-5-lectura-humana)
- [6. Capturas lado a lado a 1280 y 390](#k3-6-capturas)
- [7. Sitio real con sesión por rol](#k3-7-sitio-real)
- [8. Cifras contra la fuente SQL](#k3-8-cifras-sql)
- [9. Pruebas de conformidad sin modelo](#k3-9-conformidad)
- [10. Latencia](#k3-10-latencia)
- [11. Criterios de parada](#k3-11-parada)
- [12. Procedimiento paso a paso](#k3-12-procedimiento)
- [13. Checklist binario de completitud](#k3-13-checklist)
- [14. Antipatrones con su control](#k3-14-antipatrones)
- [15. Plantilla del informe: MEDIDO e INFORME](#k3-15-informe)
- [16. Casos](#k3-16-casos)
- [17. Límites de este conocimiento](#k3-17-limites)
- [Glosario](#k3-glosario)

---

<a id="k3-0-proposito"></a>
## 0. Propósito y mapa de carga

Este conocimiento dice **qué medir, cómo y con qué umbral** para que una fase de la capa de IA pueda declararse
cerrada, y **cómo escribir el informe** de modo que otra persona o agente pueda repetir cada cifra. No enseña a
construir la capa (eso es AI-Layer-Console-Assistant-Building) ni a especificarla (eso es
[AI-Layer-Integration-Designing-Specification](knowledge-AI-Layer-Integration-Designing-Specification.md)).

La base teórica está en el libro: qué puede afirmar una prueba sobre un sistema no determinista
([Guía §11.1](../Dot-Net-Integration-AI-Layer-Guide.md#c11-1)), cómo se hace una corrida contra el modelo
([Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4)), el LLM juez
([Guía §11.6](../Dot-Net-Integration-AI-Layer-Guide.md#c11-6)), el conjunto de oro
([Guía §11.7](../Dot-Net-Integration-AI-Layer-Guide.md#c11-7)), costo y latencia
([Guía §13.3](../Dot-Net-Integration-AI-Layer-Guide.md#c13-3)) y el paso a producción
([Guía §13.8](../Dot-Net-Integration-AI-Layer-Guide.md#c13-8)). Este conocimiento **no contradice** esas
secciones: las vuelve operativas y les suma la práctica que hizo funcionar la integración de PushDispatch
(batería ×3, crítico con umbral, capturas, sitio real por rol, cifras contra SQL), que el libro todavía no trae.

### Mapa de carga

| Situación del agente | Cargar |
|---|---|
| Voy a declarar cerrada una fase | §1, §11, §13, §15 |
| Tengo que armar o correr la batería | §2, §3, §10 + [Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4), [§11.7](../Dot-Net-Integration-AI-Layer-Guide.md#c11-7) |
| Soy el crítico de una ronda | §4, §5, §14 |
| Cambió la pantalla de la consola | §6, §7 |
| Hay que desplegar o se desplegó | §7, §10, §11 + [Guía §13.5](../Dot-Net-Integration-AI-Layer-Guide.md#c13-5) |
| La respuesta muestra cifras | §8 |
| Reviso un informe ajeno | §14, §15 |
| Todo el ciclo de una fase | §12 (procedimiento) y luego §13 |
| Tengo que escribir las pruebas o los scripts que producen la evidencia | [K7](knowledge-AI-Layer-Testing-Strategy.md#k7-0) |

---

<a id="k3-1-principio"></a>
## 1. Principio: evidencia antes que «cerrado»

**R1.1 — No declares cerrada una fase sin un artefacto de evidencia por cada criterio de salida.**
*Por qué:* en PushDispatch la revisión del plan encontró que «ninguna fase cerrada estaba completa de verdad»:
se habían dado por cumplidas sin medir. *Prueba:* cada ítem del §13 apunta a un archivo (registro de batería,
captura, salida de prueba, consulta SQL) que existe y se puede abrir.

**R1.2 — Separá lo que el código garantiza de lo que el modelo hace.** Lo primero se prueba sin modelo en
cada build; lo segundo, con corridas registradas ([Guía §11.1](../Dot-Net-Integration-AI-Layer-Guide.md#c11-1)).
*Por qué:* mezclar las dos cosas mide dos cosas a la vez. *Prueba:* las pruebas de cableado corren sin servidor
de modelos y pasan; las corridas con modelo están en otra categoría.

**R1.3 — Lo que no mediste se escribe como INFORME, nunca como cumplido.** *Por qué:* el estado declarado
sin medir fue el error más caro (ver §14, K3-AP-1). *Prueba:* el informe tiene las dos secciones del §15 y
ninguna afirmación de la sección MEDIDO carece de comando y artefacto.

**R1.4 — Quien ajusta no se evalúa a sí mismo.** *Por qué:* el autor ve lo que quiso hacer, no lo que hizo.
*Prueba:* el registro de la ronda nombra a un verificador y a un crítico distintos del que ajustó (§4).

---

<a id="k3-2-bateria"></a>
## 2. Batería real contra el modelo, ×3, umbral 90 %

Una **batería** es un conjunto congelado de M preguntas distintas que se pasan por el sistema real (consola,
ruteo, herramientas, RAG y modelo), cada una con un **oráculo fijado antes de correr**. Es una corrida contra el
modelo en el sentido de [Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4), aplicada a la consola
de un panel.

**R2.1 — Armá la batería por rol y por anillo de competencia.** Cada rol del sistema tiene preguntas; cada
anillo de la frontera ([Guía §3.7.2](../Dot-Net-Integration-AI-Layer-Guide.md#c3-7-2)) tiene preguntas; hay
preguntas no respondibles cercanas y lejanas ([Guía §11.7](../Dot-Net-Integration-AI-Layer-Guide.md#c11-7)),
preguntas que exigen aclaración (dos candidatas de nombre parecido, varias cuentas) y al menos una inyección
(«llevame a /usuarios») que no debe ofrecer destino. *Por qué:* una batería sólo de preguntas felices pasa con
un sistema que inventa. *Prueba:* la tabla de la batería tiene columnas rol, anillo, tipo (respondible,
no respondible, aclaración, inyección) y ninguna celda rol × tipo está vacía sin motivo escrito.

**R2.1a — Tamaño mínimo: M ≥ 20 preguntas distintas y al menos una por celda rol × tipo.** Si las celdas
exigen más de 20, manda la cobertura de celdas. *Por qué:* con M = 10 el 90 % admite una sola falla y no
distingue nada; con M = 20 admite dos, y PushDispatch aceptó con baterías de ese orden (20 en la primera, 12
redacciones ancladas más 15 deterministas por corrida al final). *Prueba:* la tabla de la batería tiene ≥ 20
filas y la cuenta por celda no tiene ceros sin motivo escrito. Cada caso lleva además su **forma esperada**
(F1–F8 de [K5 §3](knowledge-AI-Layer-Conversational-Design.md#k5-3)).

**R2.2 — Fijá el oráculo antes de correr y que lo decida código.** Para cada pregunta: qué herramienta debe
elegirse, qué alcance, qué cifra (con su fuente), qué destino o ninguno, o «No está en la ayuda». *Por qué:*
un oráculo escrito después se acomoda al resultado. *Prueba:* el archivo de la batería tiene fecha y SHA
anteriores a la primera corrida, y el veredicto se puede recalcular leyendo sólo el registro
([Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4)).

**R2.2a — Si una condición no es decidible por código, agregá la lectura experta al veredicto.** «No dicta la
operación» o «no hace el ejercicio por el alumno» no se decide con expresiones regulares. *Por qué:* en Geometría
el oráculo por código daba verde y 24 de 30 respuestas dictaban la operación en palabras. *Prueba:* el archivo de
datos de la batería tiene una columna de lectura experta y el veredicto la cuenta.

<a id="k3-2-2b"></a>
**R2.2b — Batería antes de la firma del corpus** (hasta 1.6 se citaba «R2.6», id repetido con la caída del servidor). Si el corpus sólo se sirve `vigente` y la firma es del dueño, la
batería previa corre con una copia de ensayo declarada en el informe, fuera del repo; nunca se marca `vigente` en
el repo para medir. *Prueba:* el informe nombra la copia de ensayo y el repo no tiene `vigente` sin firma.

**R2.3 — Corré la batería completa tres veces y reportá las tres.** Cada corrida da k/M; el informe lleva
las tres, el mínimo y el máximo. *Por qué:* una corrida sola no distingue la varianza del modelo de una mejora
real. En PushDispatch la redacción anclada dio **19/20 (95 %) en una sola corrida**; la crítica exigió ×3 y, al
repetir, la tasa real resultó **7/11 y después 21/33 (64 %)**. Las causas, medidas volcando por pregunta los
fragmentos recuperados y el pedido, fueron cuatro: el troceo cortaba las secciones a los 1200 caracteres y
perdía el final; el recorte del pedido se quedaba con el principio del fragmento; el modelo se abstenía teniendo
el fragmento correcto; y el control descartaba oraciones fieles (cita al trozo equivocado, polaridad cortada por
«;»). Con trozos de 500 caracteres por párrafo, recorte por párrafos de la pregunta y un control **más estricto,
no más laxo**, llegó a **27/30 (90 %) ×3**, y la aceptación final fue **36/36 en cada una de tres corridas, con
81 de 81 casos sin falla**. El 95 % de una corrida escondía un 64 %.
*Prueba:* hay tres registros con la misma composición (modelo, versión, corpus, SHA del código, batería) y el
informe los cita.

**R2.4 — No sumes las tres corridas como si fueran 3M casos.** La unidad de muestra es la pregunta distinta;
repetir mide **estabilidad**, no conducta ([Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4) y su
antipatrón en [Guía §11.12](../Dot-Net-Integration-AI-Layer-Guide.md#c11-calidad)). *Por qué:* 57/60 no es más
evidencia que 19/20 tres veces; y una sola corrida de 19/20 puede esconder un 64 % (R2.3). *Prueba:* el informe dice «k₁/M, k₂/M, k₃/M» y nunca «Σk/3M».

**R2.5 — Umbral de aceptación: cada una de las tres corridas ≥ 90 %, y ninguna pregunta con dato inventado.**
*Por qué:* el umbral sobre el mínimo (no sobre el promedio) exige estabilidad; el dato inventado es el error
caro y no se compensa con aciertos. *Prueba:* min(k₁, k₂, k₃)/M ≥ 0,90 y la lectura humana (§5) no marca
ninguna cifra sin origen. Con M chico, informá además el IC95 que pide el libro (con M ≤ 12 toda tasa es
descriptiva): el 90 % es el umbral del proyecto para seguir, no una afirmación sobre la población.

<a id="k3-2-6"></a>
**R2.6 — Caída del servidor = no medido.** Si el servidor de modelos no es el esperado o deja de responder, lo
que falta queda «no medido»; no se completa con otra estación ni cuenta como falla
([Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4)). *Prueba:* el registro trae la versión del
servidor y del modelo verificadas antes del primer pedido.

<a id="k3-2-6b"></a>
**R2.6b — El límite de tasa del servidor tampoco es una falla.** Si la instancia limita pedidos por minuto, las
corridas se espacian para no cobrarlo y todo rechazo por límite (429) cuenta como no medido; una corrida con
rechazos no entra en el ×3. *Porqué:* en Lab-Geometria tres corridas seguidas contra una instancia limitada a 60
pedidos por minuto mezclaron 429 con fallas reales; espaciadas 65 s, desaparecieron. *Prueba:* el registro trae
pausa y conteo de 429 por corrida ([K7 R9.12](knowledge-AI-Layer-Testing-Strategy.md#k7-9-12)).

**R2.7 — Una pregunta fallida se arregla en el código o en el corpus, nunca en la batería.** *Por qué:*
reescribir la pregunta hasta que pase vacía la medición. *Prueba:* el diff de la batería entre rondas sólo
agrega casos (con motivo) o crea una `v2` con fecha; no cambia oráculos de casos fallidos.

---

**Implementación en K7.** El script de la batería ×3, el umbral, las inestables y el registro por corrida se
arman según [K7 §9](knowledge-AI-Layer-Testing-Strategy.md#k7-9); la latencia, según [K7 §10](knowledge-AI-Layer-Testing-Strategy.md#k7-10).

**R2.8 — Pasar los controles de cifras y citas no es pasar la calidad.** En Movilidad Urbana la redacción
pasó todos los controles de anclaje y aun así salió peor que la plantilla fija en 6 de 13 pares a ciegas. Para
aceptar una forma redactada se exige, además, la compuerta «la redactada no es peor que la fija» leída a ciegas
(K5 CD-8), con pares efectivamente juzgados (K7 R9.8), y el informe declara la tasa de rechazo del validador
(K6 §12.10). Sin pares juzgados, la compuerta queda **NO EJECUTADO** y la fase no se entrega. *Prueba:* el
informe MEDIDO tiene «pares juzgados», «peores a ciegas» y «rechazos del validador» por forma.

<a id="k3-2-9"></a>
**R2.9 — El límite de un componente heurístico se declara, y la garantía descansa en la configuración por
omisión.** Si un componente decide con reglas que no cubren el lenguaje abierto (un enrutador, un filtro), el
informe dice que es heurístico, cuántos casos conocidos quedan sin resolver ([K7 R6.9](knowledge-AI-Layer-Testing-Strategy.md#k7-6-9))
y de qué tipo, y la fase se acepta **sólo** con la configuración por omisión que no depende de él (respuesta
fija desde la ficha, sin modelo). Encender el camino que sí depende de él (redacción con modelo) exige una
condición escrita de reapertura: que la sonda acumulada deje de encontrar fallas en el registro que hoy falla, o
un clasificador nuevo entrenado con esa sonda. *Porqué:* en Geometría la sonda secó el vocabulario del dominio,
pero la ronda de control encontró 16 fallas en 54 planteos cotidianos; declararlo evitó dar por garantizado lo que
sólo estaba probado en un registro. *Prueba:* el informe tiene «límite declarado» con el conteo de casos
conocidos, la configuración aceptada y la condición de reapertura.

<a id="k3-2-11"></a>
**R2.11 La batería es la compuerta de habilitación de un modelo.** Un modelo nuevo de redacción, de cualquier proveedor, se carga y se prueba su conexión, pero no se habilita ni se ofrece al usuario sin la batería ×3 aprobada con ese modelo, registrada con fecha y versión de la batería. Habilitar sin aprobación devuelve un conflicto. Cambiar la versión de un SDK de proveedor también corre la batería. *Porqué:* la conducta medida está anclada al modelo con el que se midió; otro modelo es otro sistema [Guía DC §17.3.8](../Conversational-Design-Framework-Guide.md#c17-3-8). *Prueba:* habilitar sin aprobada → 409.

**R2.12 Consumo y MCP se aceptan con cifras contra la fuente.** N consultas de la batería dejan N filas de consumo con tokens > 0 y costo coherente con la tabla de precios; el tablero da las mismas cifras que una consulta SQL directa (R8); las cuotas se aceptan al 80 % (aviso) y al 100 % (corte o respaldo según política). El servidor MCP se acepta conectado desde un asistente real con un usuario de prueba que ve sólo sus cuentas, y con los casos sin token (401 con metadatos), token vencido y sin alcance (403).

<a id="k3-3-condiciones"></a>
## 3. Condiciones de medición: en frío y pagando el cambio de modelo

**R3.1 — Medí en las condiciones de la aplicación, no en las del banco.** Si el servidor tiene un solo
modelo residente, la aplicación paga cada alternancia entre el modelo de redacción y el de embeddings; la
batería también tiene que pagarla ([Guía §13.4](../Dot-Net-Integration-AI-Layer-Guide.md#c13-4)). *Por qué:*
en PushDispatch la batería corría con el modelo ya cargado y daba latencias optimistas; con un modelo
residente, el modelo de embeddings desalojaba al de redacción y cada turno lo volvía a cargar (entre unos 3 y
7,5 s por pedido, medidos aparte); por eso el caliente no salía más rápido que el frío. El arreglo fue que «la batería
pague el cambio de modelo como la app». *Prueba:* el registro de cada pregunta muestra la llamada de
embeddings y la de redacción en el mismo orden que la consola, sin precarga artificial entre ellas.

**R3.2 — Separá frío de caliente.** Medí el primer pedido tras arrancar (o tras la descarga por inactividad)
aparte de los siguientes ([Guía §13.3](../Dot-Net-Integration-AI-Layer-Guide.md#c13-3)). *Prueba:* el informe
tiene dos filas de latencia, frío y caliente, cada una con su p50 y p95.

**R3.3 — No compares series de condiciones distintas.** Un cambio de hardware, de modelo, de servidor o de
corpus abre una serie nueva. *Por qué:* en PushDispatch el servidor ganó aceleración a mitad del ciclo y las
latencias de las primeras rondas dejaron de ser comparables con las siguientes. *Prueba:* cada serie del
informe declara su composición y una nota explícita marca el corte.

**R3.3a — Anotá la configuración del servidor junto a cada cifra de latencia:** cuántos modelos tiene en
memoria a la vez y el hardware (con o sin aceleración), sin nombrar el equipo ni el modelo. *Por qué:* en
PushDispatch un reporte de latencia no fue reproducible: sus cifras eran de un servidor anterior, de un solo
modelo en memoria y sin aceleración; con el servidor nuevo, de dos modelos cargados, el primer token pasó de
unos 12,6 s a unos 2,9 s. El reporte se corrigió declarando la configuración. *Prueba:* cada fila de latencia
del informe tiene las columnas «modelos en memoria» y «aceleración»; dos filas con valores distintos no se
comparan ni se restan.

**R3.4 — Registrá la composición en cada corrida:** versión del modelo (por variable, no en el repo),
versión del servidor, SHA del código, SHA del corpus, SHA de la batería, fecha. *Prueba:* las tres corridas
del §2 coinciden en los cinco campos; si no, no son ×3.

---

<a id="k3-4-critico"></a>
## 4. Crítico con nota y rondas ajuste → verificación → crítica

**R4.1 — Cada ronda tiene tres roles distintos: el ajustador, el verificador independiente y el crítico.**
El ajustador cambia código o corpus; el verificador corre la batería y las pruebas sin conocer la intención del
cambio; el crítico lee respuestas, capturas y diff y pone **una nota de 0 a 10 con sus motivos**. *Por qué:* en
PushDispatch la calidad subió ronda a ronda (r2 a r8) porque cada crítica dejaba hallazgos concretos que la
ronda siguiente atacaba. *Prueba:* cada ronda deja tres archivos (ajuste, verificación, crítica) con autores
distintos. **Con un solo agente al mando**, los tres roles son **subagentes distintos** lanzados con contexto
propio: el verificador recibe sólo el SHA y la batería, no la intención del cambio; el crítico, la evidencia. Lo
que no vale es que el mismo contexto ajuste y verifique (detalle en
[K4 §2.2](knowledge-AI-Layer-Integration-Working-Method.md#k4-2-2-roles)).

**R4.2 — Umbral del crítico: nota ≥ 8, con la batería ≥ 90 % en las tres corridas.** Las dos condiciones a la
vez. *Por qué:* la batería mide lo que el oráculo sabe preguntar; el crítico ve lo que el oráculo no ve
(primera persona del asistente, voseo roto, cita que no respalda la oración, burbuja que no se lee como
conversación). *Prueba:* la crítica de la última ronda tiene nota ≥ 8 y cita el registro de la batería de
esa misma ronda.

**R4.3 — La crítica enumera hallazgos accionables, no impresiones.** Cada hallazgo: qué pregunta o pantalla,
qué se vio, qué se esperaba, dónde se arregla (código, corpus, plantilla). *Prueba:* cada hallazgo de la ronda
n aparece como resuelto, diferido con motivo o refutado en la ronda n+1.

**R4.4 — Si el crítico es un modelo, calibralo antes de creerle y no uses el de la misma familia que
redacta como única validación** ([Guía §11.6](../Dot-Net-Integration-AI-Layer-Guide.md#c11-6)). *Prueba:* hay
un registro de calibración o la crítica es de un agente distinto que razona sobre la evidencia y además pasa
por la lectura humana del §5.

**R4.6 — El crítico itera; acepta el juez dueño.** Donde el dueño del dominio fija su propio umbral (en Geometría,
puntuación docente ≥ 4 y mayor que la plantilla), la nota del crítico o de la mesa no lo sustituye. *Prueba:* el
informe separa la nota del crítico de la del juez dueño y el estado «aceptado» cita la del juez.

**R4.5 — Diferí con nombre.** Lo que una ronda decide no arreglar entra en una lista de diferidos (`D-n`) con
motivo y condición de reapertura. *Prueba:* el informe final lista los diferidos y ninguno está sin motivo.

---

<a id="k3-5-lectura-humana"></a>
## 5. Lectura humana de las respuestas

**R5.1 — Leé las respuestas una por una, además del veredicto automático.** *Por qué:* en PushDispatch la
batería automática pasó con 95 % y la lectura honesta encontró **una cifra inventada en unas dieciséis
respuestas mostradas**; el oráculo no la detectaba. *Prueba:* el informe tiene una tabla «leídas / con
problema» y cada problema enlaza la línea del registro.

**R5.2 — Qué se busca al leer:** toda cifra tiene origen en una herramienta; toda oración informativa tiene
su cita; «No está en la ayuda» aparece donde debe en lugar de un dato inventado; el asistente no habla en
primera persona («Armaré», «Pondré»); el destino ofrecido existe y el rol lo puede usar; la aclaración entre
candidatas muestra 2 a 4 opciones con sus cifras; la forma (F1–F8) es la esperada. Los criterios de forma los
fija [K5 §8](knowledge-AI-Layer-Conversational-Design.md#k5-8). *Prueba:* la tabla de lectura tiene una columna por criterio.

**R5.3 — Un dato inventado detiene la aceptación aunque la tasa pase.** *Prueba:* si la tabla del §5.1 tiene
una fila «inventado», el estado del informe no puede ser «aceptado» (ver §11).

---

<a id="k3-6-capturas"></a>
## 6. Capturas lado a lado a 1280 y 390

**R6.1 — Capturá la consola de la aplicación servida, a 1280 y a 390 px de ancho, en claro y oscuro, junto a
la pantalla vecina del mismo panel.** *Por qué:* el crítico juzga «parece la misma aplicación» sólo si ve las
dos al lado; y un estilo que existe en el repositorio pero no se carga no aparece en ninguna prueba unitaria.
*Prueba:* cuatro pares de capturas por pantalla tocada (1280 claro y oscuro, 390 claro y oscuro).

**R6.2 — Medí, no mires solamente:** `document.documentElement.scrollWidth == window.innerWidth` (sin
desborde horizontal), desplazamiento de diseño acumulado 0 al recargar, y el estilo computado del componente
de la consola distinto del valor por defecto del navegador. *Por qué:* el último chequeo detecta la hoja de
estilos que no quedó enlazada (ver §14, K3-AP-2). *Prueba:* un script de navegador automatizado imprime las tres
cifras por ancho y el informe las copia.

**R6.3 — La captura es de la app real, no de la demo ni de la maqueta.** *Prueba:* la URL de la captura es
la del sitio desplegado o la de la app levantada desde el SHA del PR, y figura en el informe.

---

<a id="k3-7-sitio-real"></a>
## 7. Sitio real con sesión por rol

**R7.1 — Después de desplegar, entrá al sitio real con una sesión de cada rol y hacé al menos una pregunta
por rol.** *Por qué:* en PushDispatch la verificación en el sitio real encontró dos defectos que la demo no
mostraba: la IA apagada en silencio por una variable con otro nombre, y la plantilla de respaldo que devolvía
la introducción en vez de la respuesta pertinente. *Prueba:* hay un registro por rol con la pregunta, la
respuesta y la captura, tomado del sitio desplegado.

**R7.2 — Incluí el rol con más privilegios.** *Por qué:* el raíz es el rol que todos asumen cubierto y el que
quedó sin consola (ver §14, K3-AP-3). *Prueba:* la matriz rol × pantalla tiene la fila del raíz con respuesta no
vacía.

**R7.3 — Comprobá el estado de la IA antes de preguntar.** La aplicación expone el motivo exacto de
habilitación (habilitada, falta endpoint, falta modelo…) sin dirección ni modelo; el verificador lo lee
primero. *Por qué:* sin eso, «la consola responde con plantillas» parece funcionamiento normal. *Prueba:* el
informe copia el motivo leído y es «habilitada».

**R7.4 — Probá ver y hacer por separado.** Para cada destino ofrecido, que el rol pueda abrir la página y,
si el destino promete una acción, que pueda ejecutarla. *Por qué:* un destino que termina en 403 es una
negativa disfrazada. *Prueba:* por destino, dos marcas: abre / ejecuta.

**R7.5 — Las credenciales de las sesiones salen del entorno del verificador y no aparecen en ningún
registro, captura ni informe.** *Prueba:* §14, K3-AP-5.

---

<a id="k3-8-cifras-sql"></a>
## 8. Cifras contra la fuente SQL

**R8.1 — Toda cifra que muestre la consola se compara contra una consulta SQL directa, escrita aparte de la
herramienta que la calcula.** *Por qué:* repetir la misma lectura no prueba nada; una segunda derivación sí.
En PushDispatch los totales por cuenta y por aplicación se contrastaron así antes de aceptar la fase.
*Prueba:* por cifra, el informe tiene la consulta, su resultado y la respuesta de la consola, y coinciden.

**R8.2 — Contrastá también las agregaciones derivadas:** «Otras (k)» de una torta suma exactamente el total
menos las mayores; un total con desglose suma sus partes; la métrica rotulada es la que se calculó (por
ejemplo, dispositivos, no personas). *Prueba:* una fila por agregación con la suma verificada.

**R8.3 — El validador de cifras del código es garantía; el contraste SQL es aceptación.** El libro pide que
cada número tenga origen en una herramienta ([Guía §11.1](../Dot-Net-Integration-AI-Layer-Guide.md#c11-1));
el contraste comprueba que la herramienta calcula bien. *Prueba:* las dos existen.

**R8.4 — El código y la consulta redondean igual, y el fixture tiene medios exactos.** .NET `Math.Round(x, 1)`
redondea al par (25,25 → 25,2); SQLite y la mayoría de los motores, alejándose del cero (25,3). Fijá
`MidpointRounding.AwayFromZero` (o el modo que use la base) en cada redondeo que se muestre. *Por qué:* en
[Lab-E2E.WebBlazor](https://github.com/hdcm-dev/Lab-E2E.WebBlazor) los minutos de colectivo [10, 8, 36, 47] promedian 25,25: la pantalla decía 25,2 y la consulta 25,3, y
la prueba de contraste no lo vio porque su fixture no tenía promedios en la mitad. *Prueba:* el fixture del
contraste trae al menos un promedio y un porcentaje que caen exactamente en la mitad.

---

<a id="k3-9-conformidad"></a>
## 9. Pruebas de conformidad sin modelo

**R9.1 — Atá con pruebas lo que tiende a divergir:** el catálogo de destinos contra las rutas y los
parámetros de consulta que las páginas aceptan (por reflexión sobre los atributos de la página); el catálogo
contra los permisos; el corpus contra las pantallas (cobertura). *Por qué:* catálogo y páginas cambian en PRs
distintos. *Prueba:* las pruebas corren en cada build sin modelo y fallan si se agrega una ruta sin entrada o
un filtro que la página no lee.

**R9.2 — Probá las garantías del ruteo con un cliente guionado:** alcance resuelto antes de responder, filtro
por rol previo a la búsqueda, URLs del texto del modelo eliminadas, destino revalidado por permiso
([Guía §11.2](../Dot-Net-Integration-AI-Layer-Guide.md#c11-2);
listas por actor en [Guía Anexo B](../Dot-Net-Integration-AI-Layer-Guide.md#anexo-b)). *Prueba:* una prueba negativa por garantía.

**R9.3 — Vocabulario neutro en las pruebas.** Viven en el repositorio: no nombran clientes, organizaciones
ni datos reales. *Prueba:* grep de nombres de organización sobre `tests/` da cero.

**R9.4 — Las evidencias no llevan datos personales.** Capturas del sitio real, registros de batería e informes
se toman con cuentas y datos de prueba; si una captura del sitio real muestra personas reales (nombres, correos,
teléfonos, documentos), se tapa antes de commitear o no se publica. *Por qué:* el control de secretos cubre
claves y direcciones, no personas; las capturas del sitio real con sesión por rol muestran lo que el rol ve.
*Prueba:* grep de patrones de correo, teléfono y documento sobre los textos de la evidencia da cero, y una
persona (o un subagente distinto del autor) revisa cada captura con la lista de §13.

---

**Implementación en K7.** Cómo se escriben estas pruebas, las adversas, la mutación y las compuertas de CI está
en [K7 §5–§7 y §12](knowledge-AI-Layer-Testing-Strategy.md#k7-5); la conformidad del catálogo, en
[K6 §14](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-14).

<a id="k3-10-latencia"></a>
## 10. Latencia

**R10.1 — Informá latencia de la consola de punta a punta (pregunta → primera palabra y → respuesta
completa), en frío y en caliente, con p50 y p95** ([Guía §13.3](../Dot-Net-Integration-AI-Layer-Guide.md#c13-3)).
*Prueba:* la tabla del informe tiene las cuatro cifras y el número de muestras.

**R10.2 — Fijá el presupuesto antes de medir** (por ejemplo, p95 caliente por debajo del tiempo que la
especificación tolera) y estimá la capacidad con la cola del servidor: si atiende de a uno, la capacidad por
hora sale del tiempo de servicio, no del deseo. *Prueba:* el presupuesto figura en la especificación
(`REQ-IA-nn`) con fecha anterior a la medición.

**R10.2a — Sin presupuesto en la especificación, derivalo del uso:** el tiempo que la persona espera hoy en la
pantalla sin asistente (buscar en la ayuda, filtrar) es el techo del p95 caliente; si es atención con cola (un
mostrador), el techo es menor que el tiempo entre llegadas. *Prueba:* el `REQ-IA-nn` de latencia cita de dónde
salió la cifra.

**R10.2b — El p95 con modelo se mide contra el corte, no sólo contra el presupuesto.** Si el servicio corta al
redactor a los *t* segundos (y cae a la respuesta fija), el informe da el p95 del redactor **con** modelo frente a
*t* y cuántas respuestas cayeron por tiempo. Medir sólo la configuración sin modelo (≤ 1 s) esconde que el
redactor vive pegado al corte. *Porqué:* en Lab-Geometria el redactor dio p95 de 7–8 s contra un corte de 8 s con
servidor compartido: 16 respuestas «fuera de tiempo». *Prueba:* la tabla del §10 tiene la fila «con modelo, p95
frente al corte» y el conteo de cortes.

**R10.3 — La latencia sale del registro de la batería en las condiciones del §3, no de una corrida aparte con
el modelo caliente.** *Prueba:* las cifras del §10 enlazan a los mismos registros del §2.

---

<a id="k3-11-parada"></a>
## 11. Criterios de parada

**Aceptar la fase** cuando, a la vez:

1. las tres corridas de la batería ≥ 90 % y ningún dato inventado en la lectura humana;
2. la última crítica tiene nota ≥ 8 y sus hallazgos están resueltos o diferidos con nombre;
3. las capturas a 1280 y 390 no muestran desborde, el desplazamiento al recargar es 0 y los estilos se
   aplican;
4. el sitio real responde a cada rol, raíz incluido, con la IA «habilitada»;
5. las cifras coinciden con SQL;
6. la latencia cumple el presupuesto en frío y en caliente;
7. el checklist del §13 está completo y el informe separa MEDIDO de INFORME.

**Seguir con otra ronda** si falla alguno de 1–3 y la ronda anterior mejoró algo medible.

**Escalar a la mesa** (no seguir ajustando) si: dos rondas seguidas no mejoran ni la tasa mínima ni la nota;
el fallo es de corpus (no hay documento que recuperar) y no de búsqueda; el fallo pide cambiar la frontera
de competencia o un requisito; o el arreglo contradice un ADR. *Por qué:* en PushDispatch un fallo que parecía
de búsqueda era falta de corpus, y uno que parecía de herramientas era de alcance; seguir ajustando el prompt
no los habría resuelto.

**No medido** (ni aceptado ni rechazado) si el servidor de modelos no estuvo disponible o cambió de
composición entre corridas: se repite cuando vuelve, sin completar con otra estación.

---

<a id="k3-12-procedimiento"></a>
## 12. Procedimiento paso a paso

1. Leé los `REQ-IA-nn` de la especificación y anotá cuáles se miden en esta fase.
2. Congelá la batería (§2): tabla rol × anillo × tipo, oráculos en código, SHA y fecha. Si la batería ya
   existe, sólo agregás casos con motivo.
3. Fijá presupuesto de latencia y umbrales (§2.5, §4.2, §10.2) por escrito antes de correr.
4. Corré las pruebas sin modelo (§9). Si fallan, no sigas.
5. Verificá la composición (servidor, modelo, versiones) y corré la batería tres veces en las condiciones del
   §3. Guardá los tres registros.
6. Leé las respuestas (§5) y llená la tabla de lectura.
7. Contrastá las cifras contra SQL (§8).
8. Levantá la app desde el SHA del PR y tomá las capturas con sus medidas (§6).
9. Pasá la evidencia al crítico; recibí la nota y los hallazgos (§4).
10. Aplicá el criterio de parada (§11): aceptar, otra ronda o escalar.
11. Al desplegar: leé el motivo de habilitación y recorré el sitio real con una sesión por rol (§7).
12. Revisá registros, capturas e informe con el control de secretos (§14, K3-AP-5).
13. Escribí el informe con la plantilla del §15 y subilo con el PR que lleva la evidencia.
14. Registrá el estado de la fase en el registro de frentes con el comando que lo mide.

---

<a id="k3-13-checklist"></a>
## 13. Checklist binario de completitud

Cada ítem se tacha sólo con un artefacto que existe.

- [ ] Pruebas sin modelo pasan y están separadas de las corridas con modelo.
- [ ] La batería tiene casos por cada rol, cada anillo, no respondibles, aclaración e inyección.
- [ ] Los oráculos tienen fecha y SHA anteriores a la primera corrida.
- [ ] Hay tres registros de batería con la misma composición.
- [ ] Las tres corridas ≥ 90 % y el informe da las tres cifras, mínimo y máximo.
- [ ] La batería pagó el cambio de modelo como la aplicación y se midió en frío.
- [ ] La tabla de lectura humana existe y no tiene datos inventados.
- [ ] La última crítica tiene nota ≥ 8, autor distinto del ajustador, y sus hallazgos están cerrados o
      diferidos con nombre.
- [ ] Hay capturas a 1280 y 390, claro y oscuro, junto a la pantalla vecina, de la app real.
- [ ] La batería tiene M ≥ 20 preguntas distintas, ninguna celda rol × tipo vacía sin motivo y la forma esperada de cada caso (R2.1a).
- [ ] Cada cifra de latencia declara modelos en memoria y aceleración del servidor (R3.3a).
- [ ] Ninguna evidencia muestra datos de personas reales (R9.4).
- [ ] Sin desborde horizontal, desplazamiento 0 al recargar, estilo de la consola aplicado.
- [ ] El motivo de habilitación leído en el sitio real es «habilitada».
- [ ] Hay una pregunta respondida por rol en el sitio real, raíz incluido.
- [ ] Cada destino ofrecido abre y, si promete acción, la ejecuta con ese rol.
- [ ] Cada cifra mostrada coincide con una consulta SQL independiente.
- [ ] Latencia p50 y p95, frío y caliente, dentro del presupuesto escrito antes.
- [ ] Grep de secretos sobre registros, capturas e informe da cero.
- [ ] El informe separa MEDIDO de INFORME y cada cifra medida trae comando y artefacto.

---

<a id="k3-14-antipatrones"></a>
## 14. Antipatrones con su control

| # | Antipatrón | Qué costó en PushDispatch | Control |
|---|---|---|---|
| <a id="k3-ap-1"></a>K3-AP-1 | Fase dada por cumplida sin evidencia | La revisión del plan concluyó que ninguna fase cerrada estaba completa | Checklist del §13 con artefacto por ítem; R1.1 |
| <a id="k3-ap-2"></a>K3-AP-2 | Estilos sin enlazar o en una rama sin PR | La consola se veía bien en la maqueta y no en la app; el arreglo quedó a medias | Captura de la app real y estilo computado del componente (R6.2, R6.3) |
| <a id="k3-ap-3"></a>K3-AP-3 | Consola del raíz olvidada | El rol raíz quedó excluido en varias capas y el dueño del producto lo reclamó | Matriz rol × pantalla con prueba por rol (R7.2) |
| <a id="k3-ap-4"></a>K3-AP-4 | Variable del modelo faltante en el despliegue | Se configuró la clave con otro nombre y la IA quedó apagada en silencio | Motivo exacto de habilitación leído en el sitio real (R7.3); nombre único de la clave en la especificación |
| <a id="k3-ap-5"></a>K3-AP-5 | Claves impresas en un registro | Riesgo de publicar secretos junto con la evidencia | Nunca volcar la configuración entera; el motivo nombra la clave que falta, no su valor; grep de secretos antes de commitear (ítem del §13) |
| <a id="k3-ap-6"></a>K3-AP-6 | Un 90 % reportado sin repetir | La crítica obligó a rehacer como «resultado ×3» | R2.3 y R2.4 |
| <a id="k3-ap-7"></a>K3-AP-7 | Medir con el modelo ya cargado | Latencias optimistas que la app no tenía | R3.1, R3.2 |
| <a id="k3-ap-8"></a>K3-AP-8 | Comparar series de condiciones distintas | Rondas antes y después de un cambio de hardware parecían una mejora | R3.3 |
| <a id="k3-ap-9"></a>K3-AP-9 | Creer la tasa automática sin leer | 95 % con una cifra inventada adentro | R5.1, R5.3 |
| <a id="k3-ap-10"></a>K3-AP-10 | Ajustar la batería para que pase | Mediría el oráculo, no el sistema | R2.7 |
| <a id="k3-ap-11"></a>K3-AP-11 | Confiar en la demo | La demo no mostraba la IA apagada ni la plantilla equivocada | R7.1 |
| <a id="k3-ap-12"></a>K3-AP-12 | Tomar el anclaje como calidad | Movilidad Urbana: 6 de 13 pares peores con 0 cifras inventadas | R2.8 |
| <a id="k3-ap-13"></a>K3-AP-13 | «Peores 0» sin que el juez actúe | ronda 3 no aprobada: la compuerta nunca se ejerció | R2.8; K7 R9.8 |
| <a id="k3-ap-14"></a>K3-AP-14 | Aceptar un enrutador heurístico como garantía | Geometría: 16 de 54 planteos cotidianos fallaban tras una sonda que ya había secado | R2.9; K7 R6.8 |

La tabla índice de todos los antipatrones del conjunto está en AI-Layer-Integration-Working-Method; acá viven
los que esta fase previene.

---

<a id="k3-15-informe"></a>
## 15. Plantilla del informe: MEDIDO e INFORME

```markdown
# Verificación de la fase <n> · <fecha> · SHA <sha>

## Composición
Modelo de redacción y de embeddings: por variable (no se nombran) · versión del servidor · SHA corpus ·
SHA batería · SHA código

## MEDIDO
| Qué | Cifra | Comando | Artefacto |
|---|---|---|---|
| Batería corrida 1/2/3 | k₁/M, k₂/M, k₃/M (mín, máx; IC95 si M chico) | … | registros/… |
| Lectura humana | leídas N, con problema p (inventadas i) | — | lectura.md |
| Crítica | nota x/10, hallazgos h (resueltos, diferidos D-n) | — | critica-rN.md |
| Capturas 1280/390 | scrollWidth==innerWidth, desplazamiento 0, estilo aplicado | script | capturas/… |
| Sitio real por rol | rol → respuesta no vacía, motivo «habilitada» | … | sitio/… |
| Cifras vs SQL | n de n coinciden | consulta | cifras.md |
| Latencia | frío p50/p95, caliente p50/p95, n | … | registros/… |

## INFORME (no medido en esta fase)
- <afirmación> — por qué no se midió — qué la mediría

## Diferidos
- D-n — motivo — condición de reapertura

## Estado
aceptado | otra ronda | escalado a mesa | no medido — según §11
```

---

<a id="k3-16-casos"></a>
## 16. Casos

**PushDispatch (producto diseñado desde cero; caso de origen).** Consola de IA dentro del panel de control.
La redacción anclada dio 19/20 (95 %) en una sola corrida; repetida ×3 por exigencia de la crítica cayó a
21/33 (64 %); con los arreglos de troceo, recorte, abstención y control llegó a 27/30 ×3 y se aceptó con 36/36
en cada una de tres corridas y 81/81 casos sin falla (R2.3); las rondas de ajuste,
verificación y crítica fueron de r2 a r8; la lectura humana encontró una cifra inventada que el automático
dejaba pasar (diferida con nombre y corregida después con el validador de cifras); las capturas a 1280 y 390
se midieron con un navegador automatizado; las cifras de la consola se contrastaron con SQL; el sitio real,
recorrido con sesión por rol, encontró la IA apagada por un nombre de variable distinto y la plantilla de
respaldo equivocada; la batería pasó a pagar el cambio de modelo como la app.

**Movilidad Urbana (demo del libro).** Muestra la base de este conocimiento: cableado sin red, corridas con
registro y oráculo recalculable, conjunto de oro con no respondibles
([Guía §11.9](../Dot-Net-Integration-AI-Layer-Guide.md#c11-ejemplos), [Guía §15](../Dot-Net-Integration-AI-Layer-Guide.md#c15)).
No tiene panel ni roles: aplicar §6 y §7 exige un sistema con pantalla y sesiones.

**Geometría (laboratorio didáctico, [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria)).** Sirve para ensayar §2, §5 y §8 a escala chica. La cifra se calcula
sin modelo, pero lo que hace fallar a Geometría (dictar la operación) **no** es decidible por código: el veredicto
lleva lectura docente (R2.2a) y la aceptación es del juez dueño (R4.6). El despliegue es manual del dueño, así que
la fase cierra en una instancia propia y el sitio real queda como INFORME hasta el encendido (§7). El enrutador
del asistente de conceptos se aceptó con su límite declarado y la respuesta desde la ficha por omisión; la
redacción con modelo queda apagada hasta cubrir los planteos cotidianos (R2.9; expediente `SDD/Expedientes/0004-Asistente-De-Conceptos`, actuación 003 §6–§7; PR #266 y #267).

---

<a id="k3-17-limites"></a>
## 17. Límites de este conocimiento

- El 90 % y la nota 8 son umbrales del proyecto para seguir, probados en un caso; no son una afirmación
  estadística sobre la población de preguntas. El tratamiento estadístico (IC95, comparación pareada) es el
  del libro ([Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4)).
- No cubre evaluación con bibliotecas de evaluación ni LLM juez calibrado en detalle: ver
  [Guía §11.5](../Dot-Net-Integration-AI-Layer-Guide.md#c11-5) y [§11.6](../Dot-Net-Integration-AI-Layer-Guide.md#c11-6).
- No cubre telemetría de producción ni realimentación continua: ver
  [Guía §13.1](../Dot-Net-Integration-AI-Layer-Guide.md#c13-1) y [§13.9](../Dot-Net-Integration-AI-Layer-Guide.md#c13-9).
- No define qué es una buena respuesta en forma y tono: lo define [K5](knowledge-AI-Layer-Conversational-Design.md).
- Probado en un solo sistema con panel; pasa a «probado» de verdad cuando se aplique a un segundo.

---

<a id="k3-glosario"></a>
## Glosario

- **Batería:** conjunto congelado de M preguntas distintas con oráculo fijado antes de correr, pasado por el
  sistema real.
- **×3:** tres corridas completas de la batería con la misma composición; mide estabilidad.
- **Composición:** modelo, servidor, código, corpus y batería de una corrida, con sus versiones.
- **En frío:** primer pedido tras arrancar o tras la descarga del modelo por inactividad.
- **Pagar el cambio de modelo:** medir con la misma alternancia entre redacción y embeddings que hace la app.
- **Crítico:** agente o persona distinta del ajustador que pone nota con hallazgos accionables.
- **Sitio real:** la aplicación desplegada, no la demo ni la maqueta.
- **Motivo de habilitación:** estado exacto de la IA que la app expone sin dirección ni modelo.
- **MEDIDO / INFORME:** lo que tiene comando y artefacto / lo que se afirma sin haberlo medido en la fase.
- **Diferido (D-n):** hallazgo que se decide no arreglar ahora, con motivo y condición de reapertura.
