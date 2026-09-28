---
doc_id: DOC-RUNBOOK
doc_type: tema
title: Runbook
status: vigente
origin: ia-assisted
confidence: alta
owner: ACT-06 DevOps / SRE
last_review: 2026-07-18
audience: [humano, agente]
traces: [FAM-OPE, DOC-OPERACION, DOC-DEPLOY, DOC-POSTMORTEM, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES]
---

# Runbook — `DOC-RUNBOOK`

## 1. Resumen ejecutivo

Un runbook es el procedimiento para un síntoma concreto: qué hacer, en qué orden, cuando una señal determinada indica que el sistema dejó de comportarse como debe. Se escribe para ser ejecutado bajo presión por alguien que no lo escribió, a una hora en la que nadie razona bien, y esa condición de uso determina cada decisión de forma del documento.

Su valor no está en contener conocimiento sino en **hacerlo ejecutable sin comprenderlo por completo**. Quien está de guardia a las tres de la mañana no necesita entender por qué el pool de conexiones se satura; necesita restaurar el servicio y dejar evidencia suficiente para que mañana alguien lo entienda. Comprender es el trabajo del [Postmortem](Postmortem.md); restaurar es el de este documento. La distinción viene de **ITIL 4**, que separa la gestión de incidentes —cuyo objetivo es la restauración del servicio— de la gestión de problemas —cuyo objetivo es eliminar la causa—, y es la que evita el error más común de la guardia: diagnosticar a fondo mientras el sistema sigue caído.

La forma sigue a la función. Pasos numerados, imperativos, con resultado esperado y verificación; ninguna prosa explicativa entre pasos; comandos copiables sin edición; puntos de decisión explícitos; criterio de escalamiento con umbral de tiempo. Todo lo que sea contexto va al final, en una sección que el lector apurado se saltea sin perder nada ejecutable.

---

## 2. Definición

### Qué es

Un documento breve, disparado por un síntoma observable, que contiene: el disparador, la evaluación de impacto, el diagnóstico ordenado por probabilidad, la mitigación, la verificación de que el servicio se restauró, el criterio de escalamiento y el registro de evidencia para el análisis posterior.

Su encabezado no es un tema sino una condición: «`ALT-012` — Saturación del pool de conexiones a SQL Server». Se encuentra por el nombre de la alerta que lo referencia, no navegando un índice, y esa es la diferencia estructural con el resto de la documentación operativa.

### Qué problema resuelve

Reduce el tiempo de restauración y su varianza. Sin runbook, el tiempo de resolución depende de quién esté de guardia: la persona con tres años en el sistema tarda diez minutos y la que entró el mes pasado tarda dos horas o llama a la primera. Con runbook, ambas tardan quince minutos, y esa uniformidad es lo que hace sostenible una rotación de guardia.

Resuelve además un problema de calidad de la decisión bajo presión. Alguien despertado a las tres de la mañana toma malas decisiones no por falta de conocimiento sino por el estado en que las toma; un procedimiento escrito en frío traslada la decisión al momento en que se pensaba con claridad.

### Qué NO es

**No es el Operations Guide.** Ésta es la confusión que más runbooks arruina. El [Operations Guide](Operations-Guide.md) describe el régimen normal —qué significa la métrica, por qué el umbral es 80 %, qué se hace todos los martes— y se navega por índice, en calma. El runbook describe el régimen degradado, se dispara por una alerta y se ejecuta con urgencia. Cuando el contenido del primero se filtra en el segundo, aparece el defecto característico: un runbook de nueve páginas donde los tres comandos que hacen falta están enterrados en explicaciones.

La regla operativa que separa ambos: **si el lector puede saltearlo sin dejar de resolver el incidente, no va en el runbook**. Va en la sección de contexto del final, o va al Operations Guide.

**No es un diagnóstico de causa raíz.** El runbook mitiga. Reiniciar un servicio para restaurar el servicio es una acción legítima y frecuentemente la correcta, aunque no explique nada. Lo que el runbook sí debe garantizar es que **antes de reiniciar se capture la evidencia** que el análisis posterior va a necesitar, porque un reinicio destruye el estado que explicaba el fallo. Ese paso —volcado de memoria, captura de consultas activas, exportación de métricas— es lo que hace posible el [Postmortem](Postmortem.md), y su omisión condena a esperar la próxima ocurrencia.

**No es la guía de despliegue.** El despliegue es una operación planificada, con su propio procedimiento y su propio rollback en el [Deployment Guide](Deployment-Guide.md). Cuando un despliegue sale mal y deja el sistema en un estado imprevisto, ahí sí se entra en régimen degradado y corresponde un runbook.

**No es un script.** Cuando el procedimiento entero puede automatizarse, debe automatizarse: un runbook cuyos pasos son siempre los mismos y cuyas decisiones son siempre las mismas es código pendiente de escribir. Los runbooks que sobreviven son los que contienen **decisiones humanas** —evaluar impacto, elegir entre mitigaciones con costos distintos, decidir si se escala— y esas decisiones se documentan con sus criterios, no con su resultado.

---

## 3. Aplicación por escenario

| Escenario | ¿Aplica? | Naturaleza | Qué se produce |
|-----------|----------|-----------|----------------|
| `ESC-1` Desarrollo nuevo | Sí, pero solo después de operar | Emergente | Nacen de los incidentes reales; los escritos antes son conjeturas |
| `ESC-2` Migración | Sí, y con vigencia doble | Doble | Runbooks del origen todavía válidos + los del destino, sin equivalencia automática |
| `ESC-3` Evaluación con código | Sí, reconstructiva | Descriptiva | Se reconstruyen desde alertas, historial de incidentes y conocimiento tácito del equipo |
| `ESC-4` Evaluación externa | **No aplica** | — | No se opera un sistema al que solo se accede como usuario |

### `ESC-1` — Desarrollo de software nuevo

Es el único artefacto de la familia que **no conviene escribir por anticipado**, y la razón es empírica: un sistema falla de maneras que su diseño no anticipa. Un runbook escrito antes del primer incidente documenta el fallo que el arquitecto imaginó, que rara vez es el que ocurre.

Lo que sí corresponde en `ESC-1` es preparar las condiciones para que los runbooks puedan nacer: instrumentación suficiente para diagnosticar, alertas definidas con sus umbrales, y la disciplina de que **todo incidente con impacto produzca un postmortem, y todo postmortem produzca o corrija un runbook**. Ese ciclo, sostenido durante seis meses de operación, produce un conjunto de runbooks que ninguna sesión de diseño habría generado.

La excepción legítima son los procedimientos de recuperación previsibles y de consecuencia grave —restauración desde respaldo, conmutación a la región secundaria, revocación de credenciales comprometidas—, que sí se escriben antes porque su primera ejecución no puede ser improvisada. Y se ensayan, porque un runbook de restauración no probado tiene la misma fiabilidad que un respaldo no restaurado.

En `CTX-1` los runbooks giran alrededor de la sesión y la interactividad; con Blazor Server, la degradación del circuito es la familia de incidentes más característica. En `CTX-2` giran alrededor de colas, reintentos, envenenamiento de mensajes y dependencias externas caídas. En `CTX-3` aparece un tipo propio: el incidente cuyo síntoma se manifiesta en una capa y cuya causa está en otra, que es exactamente el caso del ejemplo desarrollado más abajo.

### `ESC-2` — Migración a otro lenguaje o plataforma

Durante la coexistencia hacen falta los runbooks de ambos sistemas, con una advertencia que hay que escribir en letra grande: **los runbooks del origen no se traducen al destino**. Un procedimiento que resolvía un bloqueo en el sistema viejo puede ser inaplicable o directamente dañino en el nuevo, aunque el síntoma se llame igual.

El plan de corte necesita además su propio runbook —qué hacer si el corte falla a la mitad, cómo revertir el enrutamiento, qué hacer con las transacciones en vuelo— y ese documento se ensaya antes del corte real. Es la única ocasión en que un runbook se escribe, se prueba y ojalá nunca se use.

### `ESC-3` — Evaluación de software existente con acceso al código

Reconstruir runbooks es de las tareas más reveladoras de `ESC-3`, porque son el artefacto que menos se escribe y más se sabe: el conocimiento existe, distribuido en la cabeza de dos o tres personas.

Las fuentes, y qué aporta cada una. El **historial de incidentes** —tickets, canales de chat, alertas disparadas— dice qué falla realmente y con qué frecuencia; los diez incidentes más repetidos del último año son los diez runbooks que faltan. Las **reglas de alerta** dicen qué se considera anómalo, y las que no apuntan a ningún procedimiento son huecos identificados sin esfuerzo. Los **scripts sueltos** en las máquinas de operación o en carpetas del repositorio son runbooks a medio escribir: alguien automatizó un paso y no documentó cuándo ejecutarlo. Las **conversaciones de incidentes en el canal del equipo** son la fuente más rica y la peor conservada; un incidente resuelto en un hilo de cuarenta mensajes contiene el runbook completo, desordenado.

El hallazgo típico se enuncia sin rodeos: el sistema se opera por conocimiento tácito de dos personas, y esa concentración es un riesgo operativo de primer orden que se documenta como tal, con nombre —factor de autobús— y con la estimación de qué pasaría durante una guardia sin ellas.

### `ESC-4` — Evaluación de un producto solo desde afuera

**No aplica.** Un runbook requiere capacidad de actuar sobre el sistema: reiniciar procesos, consultar telemetría, modificar configuración, escalar recursos. En `ESC-4` no se dispone de ninguna de esas capacidades, y el único procedimiento posible ante una caída del producto evaluado es esperar o abrir un ticket con el proveedor, que no constituye documentación operativa propia.

Lo observable pertenece a otro análisis: el tiempo que el proveedor tarda en reconocer un incidente en su página de estado, la granularidad con la que informa y si publica el análisis posterior. Son indicios sobre la madurez operativa ajena, se registran con confianza explícita y no son un runbook.

---

## 4. Anatomía de un runbook

Ocho bloques, en este orden, y el orden importa porque refleja la secuencia de decisión de quien lo ejecuta.

| Bloque | Contesta | Extensión razonable |
|--------|----------|--------------------|
| Cabecera | ¿Es éste el runbook que necesito? | 5 líneas |
| Impacto | ¿Cuán grave es y a quién afecta? | 3 líneas |
| Mitigación inmediata | ¿Puedo restaurar ya, sin entender? | 1 a 3 pasos |
| Diagnóstico | ¿Cuál de las causas conocidas es? | 3 a 5 comprobaciones ordenadas por probabilidad |
| Acción por causa | ¿Qué hago según lo que encontré? | 1 bloque por causa |
| Verificación | ¿Se restauró de verdad? | 3 a 5 comprobaciones |
| Escalamiento | ¿Cuándo dejo de intentar y llamo? | Criterio con umbral de tiempo |
| Contexto | ¿Por qué pasa esto? | Al final; salteable |

La decisión de forma más importante es que la **mitigación va antes que el diagnóstico**. Es contraintuitivo para quien viene de la ingeniería —primero entender, después actuar— y es correcto en operación: si existe una acción que restaura el servicio sin riesgo de destruir evidencia, se ejecuta primero y se diagnostica con el sistema funcionando. Cuando la mitigación sí destruye evidencia, el runbook lo dice y ordena capturarla antes.

---

## 5. Ejemplo completo — `RB-012`

El runbook que sigue es ilustrativo, con datos sintéticos, y está escrito tal como debería estar en producción: sin explicaciones intercaladas, con comandos copiables y con puntos de decisión explícitos. Corresponde al sistema de reserva de salas en `CTX-3`, con Blazor Server *interactive server*, ASP.NET Core 8, EF Core y SQL Server 2022.

El incidente elegido es el más característico de esa combinación y el que peor se diagnostica sin procedimiento: la saturación del pool de conexiones degrada las llamadas a base de datos, lo cual bloquea el hilo de renderizado del circuito, y el usuario percibe una aplicación que «se desconecta sola». El síntoma se manifiesta en la interfaz y la causa está en la persistencia.

---

> ## `RB-012` — Saturación del pool de conexiones a SQL Server
>
> **Disparador:** `ALT-012` (conexiones activas > 80 % del máximo durante 3 min) o `ALT-013` (tasa de reconexión de circuitos > 5 % durante 10 min).
> **Severidad:** Alta. **Tiempo objetivo de restauración:** 20 min.
> **Runbooks relacionados:** `RB-003` (latencia alta sin saturación), `RB-021` (restauración de base de datos).
> **Última ejecución real:** 2026-05-03 · **Última revisión:** 2026-06-19
>
> ### 1. Impacto
>
> Los usuarios ven el cartel «Intentando reconectar» y pierden el trabajo no guardado del formulario de reserva. Las confirmaciones de reserva fallan con `500` o expiran. El impacto crece de forma no lineal: al agotarse el pool, la degradación pasa de parcial a total en pocos minutos.
>
> ### 2. Mitigación inmediata
>
> Ejecute esta sección **antes** de diagnosticar. No destruye evidencia.
>
> 1. **Verifique** el estado del pool en el tablero *Reservas — Persistencia*, panel **Pool de conexiones**.
>    *Resultado esperado:* un valor entre 0 y 100. Anótelo.
>
> 2. **Si el valor es ≥ 95 %**, capture la evidencia antes de cualquier otra acción:
>    ```sql
>    -- Guardar la salida completa. Se pierde al reiniciar.
>    SELECT TOP 50 r.session_id, r.status, r.wait_type, r.wait_time,
>           r.blocking_session_id, r.total_elapsed_time,
>           SUBSTRING(t.text, 1, 400) AS consulta
>    FROM sys.dm_exec_requests r
>    CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
>    WHERE r.session_id > 50
>    ORDER BY r.total_elapsed_time DESC;
>    ```
>    *Resultado esperado:* una tabla con las peticiones activas. Adjúntela al canal del incidente.
>
> 3. **Continúe** al diagnóstico. No reinicie todavía: el reinicio es una acción del paso 5 y solo para causas concretas.
>
> ### 3. Diagnóstico
>
> Ejecute las comprobaciones en orden. Deténgase en la primera que dé positivo.
>
> **D1 — ¿Hay bloqueo en la base de datos?**
> ```sql
> SELECT blocking_session_id, COUNT(*) AS bloqueadas
> FROM sys.dm_exec_requests
> WHERE blocking_session_id <> 0
> GROUP BY blocking_session_id
> ORDER BY bloqueadas DESC;
> ```
> *Positivo si:* alguna fila muestra más de 5 sesiones bloqueadas. → **Causa A**, paso 5.1.
>
> **D2 — ¿Hay una consulta lenta dominante?**
> ```sql
> SELECT TOP 5 SUBSTRING(t.text, 1, 200) AS consulta,
>        COUNT(*) AS ejecuciones, AVG(r.total_elapsed_time) AS ms_promedio
> FROM sys.dm_exec_requests r
> CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
> WHERE r.session_id > 50
> GROUP BY SUBSTRING(t.text, 1, 200)
> ORDER BY ejecuciones DESC;
> ```
> *Positivo si:* una misma consulta aparece en más de 10 peticiones activas con promedio superior a 1000 ms. → **Causa B**, paso 5.2.
>
> **D3 — ¿Hubo un despliegue en las últimas 2 horas?**
> Consulte el canal `#despliegues` o el historial del pipeline.
> *Positivo si:* hay un despliegue posterior al inicio de la degradación. → **Causa C**, paso 5.3.
>
> **D4 — ¿Hay fuga de conexiones?**
> Compare, en el tablero, `pool_conexiones_activas` contra `peticiones_por_segundo` de las últimas 6 horas.
> *Positivo si:* las conexiones crecen de forma monótona mientras el tráfico se mantiene estable o baja. → **Causa D**, paso 5.4.
>
> **D5 — ¿Es carga legítima?**
> *Positivo si:* `peticiones_por_segundo` supera en más del 50 % el máximo de los últimos 30 días y ninguna de las anteriores dio positivo. → **Causa E**, paso 5.5.
>
> Si ninguna da positivo, vaya directamente al paso 6, **Escalamiento**.
>
> ### 4. Árbol de decisión
>
> ```mermaid
> flowchart TD
>     A["ALT-012 / ALT-013 activa"] --> B["Capturar peticiones activas<br/>(paso 2.2)"]
>     B --> D1{"D1 · ¿Sesiones<br/>bloqueadas > 5?"}
>     D1 -->|Sí| CA["<b>Causa A</b><br/>Bloqueo en base<br/>→ 5.1 Terminar bloqueador"]
>     D1 -->|No| D2{"D2 · ¿Consulta lenta<br/>dominante?"}
>     D2 -->|Sí| CB["<b>Causa B</b><br/>Consulta degradada<br/>→ 5.2 Estadísticas / plan"]
>     D2 -->|No| D3{"D3 · ¿Despliegue<br/>< 2 h?"}
>     D3 -->|Sí| CC["<b>Causa C</b><br/>Regresión de versión<br/>→ 5.3 Rollback"]
>     D3 -->|No| D4{"D4 · ¿Conexiones crecen<br/>con tráfico estable?"}
>     D4 -->|Sí| CD["<b>Causa D</b><br/>Fuga de conexiones<br/>→ 5.4 Reinicio escalonado"]
>     D4 -->|No| D5{"D5 · ¿Tráfico > 150 %<br/>del máximo mensual?"}
>     D5 -->|Sí| CE["<b>Causa E</b><br/>Carga legítima<br/>→ 5.5 Escalar"]
>     D5 -->|No| ESC["<b>Sin causa identificada</b><br/>→ 6 Escalar a ACT-06 guardia 2"]
>
>     CA --> V["7 · Verificación"]
>     CB --> V
>     CC --> V
>     CD --> V
>     CE --> V
>     V --> OK{"¿Pool < 50 % y<br/>reconexiones < 1 %?"}
>     OK -->|Sí| FIN["Cerrar incidente<br/>→ Postmortem si hubo impacto"]
>     OK -->|No, tras 20 min| ESC
> ```
>
> ### 5. Acción según la causa
>
> **5.1 — Causa A: bloqueo en la base de datos**
>
> 1. **Identifique** la sesión bloqueadora con el `blocking_session_id` de D1.
> 2. **Inspeccione** qué está ejecutando:
>    ```sql
>    DBCC INPUTBUFFER(<session_id>);
>    ```
>    *Resultado esperado:* el texto de la sentencia. Anótelo: es la evidencia principal del postmortem.
> 3. **Si es una consulta de la aplicación**, termínela:
>    ```sql
>    KILL <session_id>;
>    ```
>    *Resultado esperado:* las sesiones bloqueadas se liberan en menos de 30 s y el pool baja.
>    *Si es un proceso de mantenimiento programado* (reindexación, respaldo), **no lo termine**: espere hasta 10 min y vaya al paso 6 si no cede.
> 4. **Vaya** a la verificación, paso 7.
>
> **5.2 — Causa B: consulta degradada**
>
> 1. **Actualice** las estadísticas de la tabla implicada:
>    ```sql
>    UPDATE STATISTICS dbo.Reserva WITH FULLSCAN;
>    ```
>    *Resultado esperado:* completa en menos de 2 min; el promedio de la consulta baja en los 5 min siguientes.
> 2. **Si no mejora**, elimine el plan almacenado de esa consulta:
>    ```sql
>    DBCC FREEPROCCACHE(<plan_handle>);
>    ```
>    *Resultado esperado:* la siguiente ejecución compila un plan nuevo.
> 3. **Si tampoco mejora**, vaya al paso 6. No intente optimizar la consulta durante el incidente.
> 4. **Vaya** a la verificación, paso 7.
>
> **5.3 — Causa C: regresión introducida por un despliegue**
>
> 1. **Ejecute el rollback** de solo código según el [Deployment Guide](Deployment-Guide.md#8-ejemplos-concretos), paso 7 del procedimiento.
>    ```bash
>    gh workflow run deploy-app.yml -f environment=prod -f digest=<digest anterior>
>    ```
>    *Resultado esperado:* 4 de 4 réplicas con la versión anterior en menos de 8 min.
> 2. **No** investigue qué cambió en el despliegue durante el incidente. Eso es trabajo del postmortem.
> 3. **Vaya** a la verificación, paso 7.
>
> **5.4 — Causa D: fuga de conexiones**
>
> El reinicio libera las conexiones y **destruye la evidencia**. Confirme que capturó la salida del paso 2.2 antes de continuar.
>
> 1. **Reinicie las réplicas de a una**, esperando a que cada una pase a `Ready` antes de la siguiente:
>    ```bash
>    kubectl rollout restart deployment/reservas-web -n prod
>    kubectl rollout status deployment/reservas-web -n prod --timeout=300s
>    ```
>    *Resultado esperado:* las conexiones caen a menos del 30 % a medida que cada réplica se recicla.
>    *Nota:* cada réplica reiniciada corta sus circuitos Blazor. Los usuarios verán el cartel de reconexión y recuperarán la sesión en pocos segundos; el trabajo no guardado del formulario se pierde. Es aceptable frente a la degradación total.
> 2. **Si las conexiones vuelven a crecer** en menos de 30 min, la fuga es de la versión en ejecución: aplique 5.3 (rollback) aunque no haya habido despliegue reciente, volviendo a la última versión estable conocida.
> 3. **Vaya** a la verificación, paso 7.
>
> **5.5 — Causa E: carga legítima**
>
> 1. **Aumente** el número de réplicas:
>    ```bash
>    kubectl scale deployment/reservas-web -n prod --replicas=6
>    ```
>    *Resultado esperado:* 6 réplicas en `Ready` en menos de 3 min.
>    *Atención:* cada réplica abre su propio pool. Verifique que `6 × MaxPoolSize` no supere el límite de conexiones del servidor SQL (actual: 800). Con `MaxPoolSize=100`, el límite se alcanza en 8 réplicas.
> 2. **Si el límite del servidor es la restricción**, no escale más y vaya al paso 6.
> 3. **Registre** el evento para la revisión de capacidad del [Operations Guide](Operations-Guide.md), sección 6.
> 4. **Vaya** a la verificación, paso 7.
>
> ### 6. Escalamiento
>
> Escale **de inmediato** si se cumple cualquiera de estas condiciones:
>
> - Han pasado 20 minutos desde el disparo de la alerta sin restauración.
> - El diagnóstico no identificó ninguna causa conocida.
> - La acción aplicada no produjo el resultado esperado y no hay una alternativa en este runbook.
> - Aparece corrupción de datos, pérdida de datos o cualquier señal de compromiso de seguridad.
>
> **A quién:** guardia de segundo nivel `ACT-06`, por llamada telefónica. Si no responde en 10 min, al responsable técnico del producto.
> **Qué entregar al escalar:** identificador del incidente, hora de inicio, comprobaciones D1 a D5 con su resultado, acciones ya ejecutadas con su hora, y la captura del paso 2.2.
>
> Escalar no es fracasar. Un incidente escalado a los 20 minutos con evidencia ordenada se resuelve más rápido que uno escalado a los 90 después de tres intentos sin registrar.
>
> ### 7. Verificación
>
> El incidente **no** está resuelto hasta que las cuatro comprobaciones den positivo:
>
> 1. `pool_conexiones_activas` por debajo del 50 % durante 5 minutos continuos.
> 2. Tasa de reconexión de circuitos por debajo del 1 %.
> 3. p95 de `POST /reservas` por debajo de 500 ms.
> 4. Una reserva de prueba creada y cancelada desde la cuenta `qa.deploy@interna` completa sin error.
>
> La cuarta comprobación existe porque las tres primeras son métricas agregadas y pueden verse normales con el sistema roto para el usuario. Ejercitar el flujo real es lo único que confirma la restauración.
>
> ### 8. Cierre
>
> 1. **Registre** en el incidente: hora de detección, causa identificada, acción aplicada, hora de restauración.
> 2. **Silencie** las alertas solo después de verificar; nunca antes.
> 3. **Abra el [Postmortem](Postmortem.md)** si hubo más de 5 minutos de impacto sobre usuarios, si la causa no se identificó, o si es la segunda ocurrencia en 30 días.
>
> ### 9. Contexto — no necesario para ejecutar
>
> El pool de conexiones de ADO.NET tiene por defecto `Max Pool Size=100` por cadena de conexión y por proceso. Cuando se agota, las peticiones nuevas esperan hasta `Connect Timeout` —15 segundos en esta configuración— y luego fallan con `InvalidOperationException: Timeout expired`.
>
> En Blazor Server el efecto es más severo que en una aplicación web tradicional, y ésa es la particularidad que hace confuso el diagnóstico. El circuito procesa los eventos del usuario en un hilo del `SynchronizationContext` del renderizador; una llamada a base de datos que espera 15 segundos bloquea el procesamiento de ese circuito por completo. El cliente, que no recibe respuesta a sus mensajes de SignalR, concluye que perdió la conexión e inicia la reconexión. De ahí el síntoma reportado —«se desconecta sola»— cuya causa está dos capas más abajo, en la persistencia, y no en la red ni en el WebSocket, que es donde todo el mundo mira primero.
>
> Las causas D (fuga) y B (consulta degradada) son las que más se repiten en este sistema. La fuga proviene históricamente de un `DbContext` resuelto fuera del ámbito de la petición y no liberado; el análisis correspondiente está en el postmortem `PM-2026-03`.

---

## 6. Preguntas guía

- ¿Alguien que nunca vio este sistema puede ejecutar este runbook y saber si funcionó?
- ¿La mitigación está antes del diagnóstico? Si no, ¿hay una razón escrita?
- ¿Qué evidencia se destruye en cada paso, y está capturada antes?
- ¿Cada comando se puede copiar y ejecutar sin editar nada que no esté señalado?
- ¿El criterio de escalamiento tiene un umbral de tiempo, o dice «si no se resuelve»?
- ¿Cuándo se ejecutó este runbook por última vez? ¿Funcionó tal como está escrito?
- ¿La verificación ejercita el flujo real del usuario, o solo mira métricas agregadas?
- ¿Este runbook debería ser un script? ¿Qué decisión humana justifica que siga siendo un documento?
- ¿Hay alguna alerta sin runbook? ¿Hay algún runbook sin alerta que lo dispare?

---

## 7. Criterios de calidad y antipatrones

### Criterios de calidad

El criterio dominante es de campo: **se ejecutó con éxito por alguien distinto de su autor, y hay registro de esa ejecución con fecha**. Un runbook nunca ejecutado es una hipótesis sobre un incidente que todavía no ocurrió.

Los criterios de forma: cabe en una pantalla y media hasta la sección de verificación; empieza por el síntoma y no por el tema; la mitigación precede al diagnóstico; los comandos son copiables sin edición y los parámetros a sustituir están marcados; cada paso declara su resultado esperado; el diagnóstico está ordenado por probabilidad y no por elegancia; existe un punto de escalamiento con umbral temporal; la verificación ejercita el camino real del usuario; y el contexto está al final, salteable.

Un criterio de conjunto, verificable de forma mecánica: **correspondencia biunívoca entre alertas y runbooks**. Cada alerta del [Operations Guide](Operations-Guide.md) apunta a uno, y cada runbook se dispara por al menos una condición identificable. Los huecos de esa correspondencia son la lista de tareas del trimestre.

Y un criterio de vigencia: la fecha de última ejecución real en la cabecera. Un runbook con dos años sin ejecutar describe un sistema que probablemente ya no existe.

### Antipatrones

**El runbook-ensayo.** Nueve páginas donde los tres comandos útiles están enterrados en explicación. Quien lo abre a las tres de la mañana lo cierra y llama a alguien, que es exactamente lo que el documento debía evitar.

**Diagnóstico antes que mitigación.** Media hora de investigación con el sistema caído, cuando un reinicio lo habría restaurado en dos minutos y el análisis podía hacerse al día siguiente sobre la evidencia capturada.

**Mitigación que destruye evidencia sin advertirlo.** «Reinicie el servicio» como primer paso, sin capturar el estado. El servicio se restaura, la causa se pierde, y el incidente se repite la semana siguiente con el mismo desenlace.

**Escalamiento sin umbral.** «Si no se resuelve, escale.» Nadie quiere ser quien no pudo, y la escalada llega noventa minutos tarde. Un umbral escrito traslada la decisión al momento en que se pensaba en frío.

**Comandos con marcadores no señalados.** `kubectl delete pod <pod>` sin decir cómo obtener `<pod>`. Quien no conoce el sistema se detiene ahí, o improvisa.

**Runbook heredado sin verificar.** Se copia de otro sistema o de una versión anterior. Los comandos son plausibles, apuntan a recursos que ya no existen, y eso se descubre durante el incidente.

**Verificación por métrica agregada.** Las gráficas se ven bien y el sistema sigue roto para los usuarios. Ejercitar el flujo real es la única verificación que no miente.

**Runbook para lo que debería ser un script.** Cinco pasos idénticos, sin decisiones, ejecutados cuarenta veces al año. Es automatización pendiente disfrazada de documentación.

**El runbook que nadie encuentra.** Existe, está bien escrito, y la alerta no lo referencia. El enlace desde la notificación al procedimiento es parte del runbook, no un detalle de configuración.

---

## 8. Anexo — Plantilla comentada

```markdown
---
doc_id: RB-<nnn>
doc_type: runbook
title: <Síntoma observable, no nombre del componente>
status: vigente
origin: human
owner: <ACT-06 responsable>
severity: crítica | alta | media
triggers: [ALT-xxx, ALT-yyy]        # las alertas que llevan hasta acá
last_review: AAAA-MM-DD
last_executed: AAAA-MM-DD           # ejecución REAL, no revisión de escritorio
mttr_objetivo: <minutos>
audience: [humano, agente]
---

# RB-<nnn> — <Síntoma>

**Disparador:** <alerta o síntoma reportado, textual>
**Severidad:** <nivel> · **Tiempo objetivo de restauración:** <minutos>
**Relacionados:** <otros runbooks que podrían ser el correcto en su lugar>

## 1. Impacto
<!-- Qué percibe el usuario y cuántos están afectados. Dos o tres líneas.
     Determina la urgencia y si corresponde comunicar. -->

## 2. Mitigación inmediata
<!-- Lo que restaura el servicio SIN entender la causa. Va primero.
     Si alguna acción destruye evidencia, ordenar su captura antes y decir
     explícitamente qué se pierde. -->

## 3. Diagnóstico
<!-- Comprobaciones ordenadas por PROBABILIDAD, no por profundidad técnica.
     Cada una: comando copiable + criterio de positivo + a qué causa lleva.
     "Deténgase en la primera que dé positivo." -->

## 4. Árbol de decisión
<!-- Mermaid. Obligatorio cuando hay más de tres causas posibles: bajo
     presión, un diagrama se recorre más rápido que una lista de condiciones. -->

## 5. Acción según la causa
<!-- Un bloque por causa. Pasos numerados, imperativos, con resultado
     esperado. Señalar los parámetros a sustituir y cómo obtenerlos.
     Incluir el efecto colateral de cada acción (p. ej., circuitos cortados). -->

## 6. Escalamiento
<!-- Condiciones explícitas, al menos una con UMBRAL DE TIEMPO.
     A quién, por qué medio, y qué entregar al escalar. -->

## 7. Verificación
<!-- Comprobaciones que confirman la restauración. Al menos una debe
     ejercitar el flujo real del usuario, no solo métricas. -->

## 8. Cierre
<!-- Qué registrar, cuándo silenciar alertas (siempre después de verificar),
     y el criterio para abrir postmortem. -->

## 9. Contexto — no necesario para ejecutar
<!-- Por qué ocurre, qué se aprendió de ocurrencias anteriores, enlaces a
     los postmortems relacionados. Todo lo salteable vive acá y en ningún
     otro sitio del documento. -->
```

El campo `last_executed` del frontmatter distingue la ejecución real de la revisión de escritorio, y esa distinción es deliberada: un runbook revisado se lee y se aprueba, un runbook ejecutado se descubre incorrecto. Solo la segunda cosa da fe de que funciona.
