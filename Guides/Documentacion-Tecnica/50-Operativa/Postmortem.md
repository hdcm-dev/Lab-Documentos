---
doc_id: DOC-POSTMORTEM
doc_type: tema
title: Postmortem / Incident Report
status: vigente
origin: ia-assisted
confidence: alta
owner: ACT-06 DevOps / SRE
last_review: 2026-07-18
audience: [humano, agente]
traces: [FAM-OPE, DOC-RUNBOOK, DOC-OPERACION, DOC-DEPLOY, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES]
---

# Postmortem / Incident Report — `DOC-POSTMORTEM`

## 1. Resumen ejecutivo

Un postmortem es el análisis escrito de un incidente ya resuelto: qué pasó, en qué orden, por qué el sistema permitió que pasara, y qué se va a cambiar para que la próxima vez no pase o duela menos. Se escribe después de la restauración, en frío, y su destinatario no es quien vivió el incidente sino quien no estaba.

Es el único documento de la familia operativa que **no se ejecuta**. Su valor no se mide por su calidad narrativa sino por las acciones correctivas que produjo y que efectivamente se completaron. Un postmortem impecable cuyas acciones siguen abiertas dieciocho meses después no cerró el ciclo: produjo un documento.

La condición que lo hace posible es la cultura **sin culpa** —*blameless*—, formalizada en el **Google SRE Book**. No es una cortesía ni un gesto de amabilidad organizacional: es un requisito técnico. Un análisis que busca responsables obtiene relatos defensivos, y sobre relatos defensivos no se puede reconstruir una línea de tiempo fiable. Se renuncia a asignar culpa porque asignarla destruye la información que se necesita.

En el marco de **ITIL 4**, el postmortem pertenece a la gestión de problemas: el incidente ya se cerró con la restauración del servicio: lo que sigue es eliminar la causa para que el incidente no vuelva.

---

## 2. Definición

### Qué es

El registro estructurado de un incidente con impacto, compuesto por: resumen e impacto cuantificado, línea de tiempo con marcas horarias, análisis de causa y factores contribuyentes, qué funcionó y qué no durante la respuesta, y acciones correctivas con dueño y fecha comprometida.

El término «postmortem» es el de uso extendido en ingeniería de fiabilidad; «Incident Report» es su equivalente en contextos de gestión de servicios y suele denotar una versión más formal, a veces dirigida al cliente. Se documenta el mismo hecho, con distinto nivel de detalle técnico y distinto destinatario, y ambas versiones deben derivar del mismo análisis: mantener dos relatos separados es cómo se producen las contradicciones que después alguien nota.

### Qué problema resuelve

Convierte un incidente en conocimiento organizacional. Sin él, lo aprendido queda en la cabeza de quien estaba de guardia y se pierde cuando esa persona rota, cambia de equipo o se va. La misma falla vuelve a ocurrir en nueve meses y el equipo la diagnostica desde cero, a veces sin recordar que ya la había visto.

Resuelve además un problema de priorización. Las tareas de fiabilidad compiten permanentemente con las funcionalidades, y suelen perder porque su beneficio es hipotético mientras el de una funcionalidad es inmediato. Un postmortem con impacto cuantificado —cuántos usuarios, cuánto tiempo, qué se dejó de hacer— convierte esa comparación en una decisión con datos.

### Qué NO es

**No es un informe de estado durante el incidente.** Mientras el sistema está caído se comunica en tiempo real, con otro formato y otra audiencia. El postmortem se escribe después, cuando se conoce el desenlace.

**No es un runbook.** El [Runbook](Runbook.md) se ejecuta bajo presión, el postmortem se lee en calma. La relación entre ambos es de producción: un postmortem bien hecho **crea o corrige un runbook**, y ese es uno de sus resultados esperables. Si el incidente reveló que no existía procedimiento, escribirlo es una acción correctiva; si el procedimiento existía pero llevó por el camino equivocado, corregirlo también.

**No es una evaluación de desempeño.** El postmortem describe lo que un sistema —técnico y organizativo— permitió que ocurriera. Cuando el documento nombra personas como causa, dejó de ser un postmortem y pasó a ser otra cosa, generalmente una que garantiza que el siguiente análisis sea incompleto.

**No es un ejercicio obligatorio para todo incidente.** Un umbral escrito evita dos degeneraciones simétricas: analizar todo, hasta que el equipo lo vive como burocracia y los postmortems se llenan a las apuradas; o no analizar casi nada, hasta que solo los desastres se examinan y se pierde el aprendizaje barato de los incidentes menores. El umbral se fija explícitamente —por ejemplo: impacto sobre usuarios superior a cinco minutos, causa no identificada, o segunda ocurrencia en treinta días— y se revisa.

---

## 3. Aplicación por escenario

| Escenario | ¿Aplica? | Naturaleza | Qué se produce |
|-----------|----------|-----------|----------------|
| `ESC-1` Desarrollo nuevo | Sí, desde el primer incidente en producción | Emergente | El corpus se construye incidente a incidente; no hay nada que escribir por anticipado |
| `ESC-2` Migración | Sí, con valor doble | Doble | Los postmortems del origen son requisitos implícitos del destino |
| `ESC-3` Evaluación con código | Sí, como fuente antes que como producto | Descriptiva | Se leen los existentes; su ausencia es en sí misma un hallazgo |
| `ESC-4` Evaluación externa | **Parcialmente** | Observacional | Solo los que el proveedor publique; no se produce ninguno propio |

### `ESC-1` — Desarrollo de software nuevo

No hay postmortems antes del primer incidente, y ese hecho trivial tiene una consecuencia que sí conviene decidir temprano: **el umbral y el proceso se acuerdan antes de necesitarlos**. Discutir si un incidente merece análisis mientras el equipo está agotado por haberlo resuelto es la peor manera de tomar esa decisión, y termina sistemáticamente en que no se escribe.

Lo que corresponde establecer en `ESC-1` es el acuerdo: cuándo se escribe, quién lo escribe —quien estuvo de guardia, no el más experimentado—, en cuánto tiempo, quién lo revisa y dónde se publica. Y el compromiso más difícil de sostener: que **las acciones correctivas entran al backlog con la misma prioridad que las funcionalidades**. Sin ese compromiso, el postmortem se convierte en un ritual sin consecuencias, y el equipo lo detecta rápido.

### `ESC-2` — Migración a otro lenguaje o plataforma

Los postmortems del sistema origen tienen un uso que no es obvio y que suele desaprovecharse: son la **especificación de los fallos que el destino no debe repetir**. Un incidente causado por un índice faltante, por un bloqueo bajo concurrencia o por un límite de configuración documenta una restricción real del dominio que el nuevo sistema va a encontrar también.

La lectura sistemática del historial de incidentes del origen, antes de diseñar el destino, es una de las actividades de mayor retorno de una migración y una de las menos practicadas. Produce una lista de escenarios de carga y de fallo que ninguna sesión de requisitos habría generado, porque son escenarios que nadie imaginó: ocurrieron.

Durante el corte conviene bajar el umbral de análisis. Los incidentes de las primeras semanas son los más informativos y los más urgentes de entender, porque revelan las diferencias entre origen y destino que las pruebas de paridad no capturaron.

### `ESC-3` — Evaluación de software existente con acceso al código

Acá el postmortem es sobre todo una **fuente**, y de las mejores. El corpus de incidentes de un sistema cuenta cómo se rompe realmente, que es información que ningún diagrama de arquitectura contiene. Diez postmortems del último año dicen más sobre la fragilidad de un sistema que su SAD completo.

Qué buscar al leerlos. Los **patrones de repetición**: la misma causa en varios incidentes indica que las acciones correctivas anteriores no se completaron o no atacaban el problema. La **distribución por componente**: la concentración de incidentes señala dónde está la deuda técnica que importa, que no siempre es la que el equipo declara. El **tiempo de detección frente al tiempo de resolución**: si detectar toma más que resolver, el problema es de observabilidad y no de arquitectura. Y el **estado de las acciones correctivas**: una alta proporción de acciones abiertas de más de seis meses indica que el proceso existe formalmente y no funciona.

La ausencia de postmortems en un sistema de varios años con usuarios reales es un hallazgo, y se enuncia con cuidado porque admite dos lecturas: o los incidentes no se analizan, o el análisis vive en canales de chat y correos, sin estructura ni seguimiento. Ambas cosas se verifican preguntando por el último incidente grande y viendo qué queda escrito de él.

### `ESC-4` — Evaluación de un producto solo desde afuera

Aplica de forma parcial y en una sola dirección: se leen los que el proveedor publique, no se produce ninguno.

Cuando existen, son de las evidencias más valiosas de todo `ESC-4`, y por una razón que excede a su contenido técnico. Un proveedor que publica postmortems detallados, con línea de tiempo, causa y acciones correctivas, está exhibiendo su proceso de ingeniería y aceptando el costo reputacional de hacerlo. Uno que solo publica «se resolvió un problema de conectividad» también está diciendo algo, en la otra dirección. La confianza de esta lectura es alta para lo que el documento afirma y baja para lo que se infiere de su ausencia.

Lo que se puede afirmar leyendo postmortems públicos: qué componentes tiene el proveedor, cómo se relacionan, qué tan rápido detecta, cómo comunica y qué considera aceptable. Lo que **no** se puede: que la ausencia de postmortems publicados signifique ausencia de incidentes. Solo significa ausencia de publicación.

---

## 4. Cultura sin culpa

### Por qué no es opcional

La organización que busca responsables obtiene, de forma predecible, tres efectos que degradan el análisis. Los relatos se vuelven defensivos y la línea de tiempo pierde precisión justo en los tramos que más importan. Los incidentes menores dejan de reportarse, con lo cual la organización pierde las señales tempranas y solo se entera de los desastres. Y quien opera desarrolla aversión a actuar, que en la práctica alarga cada incidente: ante la duda, no se toca nada y se escala, aunque la acción correcta fuera evidente.

La formulación del **Google SRE Book** es útil porque no apela a la generosidad: parte de que las personas competentes y bien intencionadas toman decisiones razonables con la información disponible en ese momento. Si el resultado fue malo, la pregunta productiva no es por qué esa persona se equivocó sino **qué hacía que esa decisión pareciera correcta**. Ahí está la causa que se puede corregir.

### Cómo se escribe sin culpa

Es una disciplina de redacción y se verifica leyendo. Se describen roles y no personas: «el ingeniero de guardia», no «Martín». Se describen acciones y no intenciones: «se ejecutó el script de migración» y no «se decidió apresuradamente ejecutar». Se evitan los adverbios de juicio —«incorrectamente», «erróneamente», «sin verificar»— que en un informe técnico solo agregan reproche.

El contraste hace evidente la diferencia. La versión con culpa: *«El desarrollador desplegó sin probar la migración, causando la caída.»* La versión sin culpa: *«La migración se desplegó a producción sin haberse ejecutado en un entorno con volumen comparable. El pipeline no exige esa verificación y el entorno de staging tiene 3.000 filas contra los 4,1 millones de producción, de modo que ejecutarla ahí no habría revelado el problema.»*

La segunda versión es más larga y contiene tres acciones correctivas posibles que la primera no contiene. La primera contiene un culpable y ninguna acción. La diferencia no es de tono: es de contenido.

Hay un límite que conviene explicitar para que la práctica no se malinterprete. Sin culpa no significa sin responsabilidad: las acciones correctivas tienen dueño con nombre, y no completarlas tiene consecuencias. Lo que se abandona es la atribución de culpa por el incidente, no la responsabilidad por su remedio.

---

## 5. Análisis de causa

### Los cinco porqués y sus límites

La técnica consiste en preguntar «por qué» de forma encadenada hasta alcanzar una causa sobre la que se pueda actuar. Aplicada al ejemplo de la sección 6:

1. ¿Por qué se degradó el servicio? Porque el pool de conexiones se agotó.
2. ¿Por qué se agotó? Porque una consulta de disponibilidad pasó de 40 ms a 4 s y retenía cada conexión cien veces más tiempo.
3. ¿Por qué pasó a 4 s? Porque el plan de ejecución cambió tras una migración que agregó una columna a `Reserva` sin actualizar estadísticas.
4. ¿Por qué el cambio de plan no se detectó antes de producción? Porque staging tiene 3.000 filas y el optimizador elige el mismo plan con cualquier índice a ese volumen.
5. ¿Por qué staging tiene ese volumen? Porque la carga de datos representativos se descartó por costo de almacenamiento en 2024 y la decisión no se revisó.

La cadena llega a algo accionable, que es la prueba de que se aplicó bien. Detenerse en el nivel 2 —«se agotó el pool»— habría producido como acción correctiva «aumentar el tamaño del pool», que trata el síntoma y garantiza la reincidencia.

Los límites de la técnica son tres y conviene tenerlos presentes, porque su aplicación mecánica produce análisis pobres con apariencia de rigor.

**Sugiere una única cadena causal.** Los incidentes reales tienen varias causas que se combinan; los cinco porqués producen una línea, y esa línea suele ser la que el analista tenía en la cabeza al empezar. Distintas personas aplicando la técnica al mismo incidente llegan a causas distintas, todas defendibles.

**Se detiene donde el analista decide detenerse.** No hay criterio objetivo para saber cuándo se llegó a la causa raíz. Se puede seguir preguntando hasta llegar a la cultura de la organización o al presupuesto del año pasado, y a partir de cierto punto las respuestas dejan de ser accionables sin volverse falsas.

**Confunde secuencia con causalidad.** Que A haya precedido a B no lo convierte en su causa. En sistemas complejos coexisten condiciones latentes que no causan nada por separado y provocan un fallo al alinearse.

Por eso los cinco porqués se usan como herramienta de exploración y no como método de conclusión. El resultado se complementa siempre con factores contribuyentes.

### Factores contribuyentes

Un factor contribuyente es una condición que no causó el incidente pero que lo hizo posible, lo agravó o retrasó su resolución. Enumerarlos evita el reduccionismo de la causa única y produce, en la práctica, las acciones correctivas de mayor retorno: casi nunca se puede eliminar la causa raíz completa, y casi siempre se pueden eliminar varios factores contribuyentes.

Cuatro categorías cubren la mayoría de los casos. Los factores **técnicos**: falta de un índice, ausencia de un límite de tiempo, un valor por defecto inadecuado. Los de **detección**: no había alerta, el umbral era demasiado alto, la alerta llegó a un canal que nadie mira de noche. Los de **respuesta**: no existía runbook, el runbook llevaba a un diagnóstico equivocado, la persona de guardia no tenía permisos para ejecutar la mitigación. Y los **organizativos**: la decisión que creó la condición se tomó sin registrar sus consecuencias, o el trabajo de fiabilidad se pospuso tres trimestres seguidos.

La categoría de respuesta es la que más rápido produce mejoras medibles. Reducir el tiempo de detección de veinte minutos a dos suele ser más barato y más efectivo que eliminar la causa técnica, y beneficia además a incidentes futuros de causa distinta.

---

## 6. Ejemplo completo — `PM-2026-05-03`

Postmortem ilustrativo con datos sintéticos, escrito tal como debería publicarse. Corresponde al incidente que el runbook [`RB-012`](Runbook.md) mitiga.

---

> # `PM-2026-05-03` — Degradación del sistema de reservas por saturación del pool de conexiones
>
> **Estado:** cerrado · **Severidad:** Alta · **Autor:** ingeniería de guardia
> **Revisado por:** `ACT-06` responsable de plataforma, `ACT-03` arquitectura
> **Fecha del incidente:** 2026-05-03 · **Publicado:** 2026-05-06
>
> ## 1. Resumen
>
> Entre las 09:14 y las 10:02 del 3 de mayo, el sistema de reserva de salas presentó desconexiones reiteradas del circuito y fallos en la confirmación de reservas. La causa inmediata fue el agotamiento del pool de conexiones a SQL Server, provocado por la degradación del plan de ejecución de la consulta de disponibilidad tras una migración de esquema desplegada la noche anterior. El servicio se restauró actualizando las estadísticas de la tabla `Reserva`.
>
> ## 2. Impacto
>
> | Dimensión | Valor |
> |-----------|-------|
> | Duración del impacto | 48 min (09:14 – 10:02) |
> | Usuarios afectados | ~340 de 1.200 activos en la franja |
> | Reservas fallidas | 87 intentos, 61 usuarios distintos |
> | Reservas perdidas sin reintento | 12, confirmadas por consulta posterior |
> | Consumo de presupuesto de error | 48 min sobre 216 min mensuales de `SLO-01` (22 %) |
> | Impacto en `SLI-03` (estabilidad de circuito) | 91,2 % en la ventana de 7 días, contra un objetivo del 99 % |
>
> No hubo pérdida ni corrupción de datos. Las 12 reservas no completadas se identificaron por auditoría y se notificó a los usuarios el mismo día.
>
> ## 3. Línea de tiempo
>
> Horas en UTC-3. Se registran las acciones, no las intenciones.
>
> ```mermaid
> timeline
>     title Incidente PM-2026-05-03
>     section Condición previa
>         02:14 · Migración desplegada : Se aplica 20260502_AddReservaRecurrenciaId. Sin impacto inmediato (tráfico nulo)
>         07:30 · Tráfico inicial : Consulta de disponibilidad en 180 ms. Elevado pero por debajo del umbral
>     section Degradación
>         09:14 · Inicio del impacto : Consulta de disponibilidad supera 3 s. Pool al 78 %
>         09:17 · ALT-013 se dispara : Reconexión de circuitos > 5 %. Notificación sin llamada
>         09:26 · Primer reporte de usuario : Mesa de ayuda recibe la primera consulta
>         09:31 · ALT-012 se dispara : Pool > 80 %. Notificación con llamada
>     section Respuesta
>         09:33 · Guardia toma el incidente : Se abre canal y se aplica RB-012
>         09:36 · Evidencia capturada : sys.dm_exec_requests exportado
>         09:41 · D1 negativo, D2 positivo : Consulta de disponibilidad en 34 peticiones activas, 4.100 ms de promedio
>         09:44 · Se aplica 5.2 : UPDATE STATISTICS dbo.Reserva WITH FULLSCAN
>         09:52 · Estadísticas completadas : Consulta baja a 52 ms. Pool desciende
>     section Restauración
>         10:02 · Verificación completa : Pool 31 %, reconexiones 0,4 %, reserva de prueba OK
>         10:05 · Incidente cerrado : Alertas silenciadas tras verificar
> ```
>
> Dos intervalos merecen atención por separado. Entre las 09:14 y las 09:33 transcurrieron **19 minutos hasta que alguien tomó el incidente**: `ALT-013` no genera llamada telefónica y llegó a un canal sin lectura activa. Entre las 09:33 y las 10:02 transcurrieron **29 minutos de respuesta**, dentro del objetivo del runbook. El tiempo de detección superó al de resolución, y ese desequilibrio orienta las acciones correctivas.
>
> ## 4. Análisis de causa
>
> **Causa inmediata.** La consulta de disponibilidad de salas pasó de 40 ms a 4.100 ms, reteniendo cada conexión del pool cien veces más tiempo. Con el tráfico habitual de la mañana, el pool de 100 conexiones por instancia se agotó en aproximadamente 90 segundos.
>
> **Cadena causal.**
>
> 1. El servicio se degradó porque el pool de conexiones se agotó.
> 2. El pool se agotó porque la consulta de disponibilidad retenía las conexiones cien veces más tiempo.
> 3. La consulta se degradó porque el optimizador eligió un recorrido completo de índice en lugar de la búsqueda habitual, tras la migración que agregó `RecurrenciaId` a `Reserva`.
> 4. El plan cambió sin detección porque las estadísticas de la tabla no se actualizaron tras la migración y el plan almacenado se invalidó con la modificación del esquema.
> 5. El problema no se detectó en staging porque `dbo.Reserva` tiene allí 3.000 filas contra 4,1 millones en producción, y a ese volumen el optimizador elige el mismo plan con cualquier estrategia.
>
> La cadena se detiene en el punto 5 porque ahí hay una condición sobre la que se puede actuar. Siguiendo un nivel más se llegaría a la decisión de 2024 de no mantener datos representativos en staging por costo de almacenamiento, que es información contextual válida —y que se registra— pero cuya revisión excede al alcance de este análisis.
>
> **Factores contribuyentes.**
>
> | Categoría | Factor | Acción asociada |
> |-----------|--------|-----------------|
> | Técnico | La migración no incluye actualización de estadísticas de las tablas afectadas | `AC-1` |
> | Técnico | No hay límite de tiempo por comando en el `DbContext`; la consulta esperaba indefinidamente | `AC-2` |
> | Detección | `ALT-013` no genera llamada telefónica pese a ser precursor confiable de degradación total | `AC-3` |
> | Detección | No existe alerta sobre variación del tiempo de ejecución de las consultas principales | `AC-4` |
> | Respuesta | El runbook `RB-012` no contemplaba «migración reciente» como causa candidata; se llegó a la correcta por D2, no por D3 | `AC-5` |
> | Organizativo | Staging con volumen de datos tres órdenes de magnitud menor que producción | `AC-6` |
>
> ## 5. Qué funcionó y qué no
>
> **Funcionó.** El runbook `RB-012` llevó a la causa correcta en 8 minutos desde que se tomó el incidente. La captura de evidencia previa a cualquier acción permitió este análisis sin esperar una reincidencia. La mitigación no requirió reiniciar ni desplegar, de modo que no se cortaron circuitos adicionales.
>
> **No funcionó.** La detección tardó 19 minutos y el primer aviso efectivo fue una llamada de usuario, no una alerta. La severidad de `ALT-013` estaba subestimada. El runbook no ofrecía la relación entre migración reciente y degradación de plan, que es el patrón exacto de este incidente. La comunicación a usuarios se hizo a las 09:48, 34 minutos después del inicio del impacto y solo tras la insistencia de la mesa de ayuda.
>
> **Suerte.** El incidente ocurrió un martes a las 09:14, con equipo completo disponible. El mismo fallo un sábado por la noche habría tenido un tiempo de detección considerablemente mayor, porque `ALT-013` habría llegado al mismo canal sin lectura. Registrar la suerte evita concluir que la respuesta fue mejor de lo que el proceso garantiza.
>
> ## 6. Acciones correctivas
>
> | ID | Acción | Tipo | Dueño | Fecha | Estado |
> |----|--------|------|-------|-------|--------|
> | `AC-1` | Incorporar `UPDATE STATISTICS` de las tablas afectadas al procedimiento de migración, como paso del pipeline | Prevención | Plataforma | 2026-05-15 | Completada |
> | `AC-2` | Fijar `CommandTimeout = 10s` en el `DbContext` de consultas de lectura | Mitigación | Desarrollo | 2026-05-20 | Completada |
> | `AC-3` | Elevar `ALT-013` a severidad alta con llamada telefónica | Detección | Plataforma | 2026-05-08 | Completada |
> | `AC-4` | Alerta sobre desviación del p95 por consulta respecto de la línea base de 7 días | Detección | Plataforma | 2026-06-30 | En curso |
> | `AC-5` | Agregar «¿migración en las últimas 24 h?» al diagnóstico de `RB-012` | Respuesta | Guardia | 2026-05-10 | Completada |
> | `AC-6` | Carga mensual de datos anonimizados de producción en staging, con volumen del 25 % | Prevención | Plataforma | 2026-07-31 | En curso |
> | `AC-7` | Definir criterio de comunicación a usuarios por severidad y automatizar el primer aviso | Respuesta | Producto | 2026-06-15 | Completada |
>
> Cada acción tiene dueño nominal y fecha comprometida. `AC-6` es la única que ataca la causa del punto 5 de la cadena; las demás reducen probabilidad, detección o impacto. Esa proporción es deliberada y habitual: la causa raíz suele ser la más cara de eliminar, y el conjunto de acciones baratas sobre factores contribuyentes rinde más en el corto plazo.
>
> ## 7. Lecciones
>
> Una migración de esquema aparentemente inocua —agregar una columna nullable— alteró el comportamiento del optimizador y produjo una degradación de dos órdenes de magnitud. La clasificación de migraciones por riesgo del [Deployment Guide](Deployment-Guide.md) considera este cambio compatible hacia atrás y lo es a nivel de contrato de datos, pero no a nivel de rendimiento. **Compatibilidad de esquema y estabilidad de plan de ejecución son propiedades distintas** y hasta ahora se trataban como una sola.
>
> El tiempo de detección superó al de resolución. Cuando eso ocurre, invertir en observabilidad rinde más que invertir en robustez, y esa es la orientación que este postmortem deja para el trimestre.

---

## 7. Preguntas guía

- ¿El documento nombra personas? Si es así, ¿qué se pierde al reemplazarlas por roles?
- ¿La línea de tiempo distingue lo observado de lo reconstruido después?
- ¿Cuánto tardó la detección comparado con la resolución? ¿Cuál de los dos hay que atacar?
- ¿La cadena causal llega a algo accionable, o se detiene en un síntoma?
- ¿Se enumeraron factores contribuyentes, o el análisis se conformó con una causa única?
- ¿Qué papel jugó la suerte, y qué habría pasado sin ella?
- ¿Cada acción correctiva tiene dueño nominal y fecha? ¿Alguna es «revisar» o «mejorar» sin criterio de terminación?
- ¿Alguna acción correctiva modifica un runbook, un umbral o un paso de verificación? Si ninguna lo hace, ¿por qué?
- ¿Qué proporción de las acciones de los postmortems del último año está completada?
- ¿Este incidente ya había ocurrido antes? ¿Qué pasó con las acciones de aquella vez?

---

## 8. Criterios de calidad y antipatrones

### Criterios de calidad

El criterio dominante es de resultado: **la proporción de acciones correctivas completadas dentro de su fecha comprometida**. Es medible sobre el conjunto de postmortems del último año y es el único indicador que distingue un proceso vivo de un ritual. Por debajo de la mitad, el equipo está escribiendo documentos que nadie usa, y conviene decirlo antes de que el propio equipo lo concluya en silencio.

Los criterios de contenido: redacción sin culpa verificable —ningún nombre propio como causa, ningún adverbio de juicio—; línea de tiempo con marcas horarias y fuente de cada dato; impacto cuantificado en unidades que el negocio entienda; causa inmediata y cadena causal separadas; factores contribuyentes en las cuatro categorías; sección explícita de qué funcionó y qué no, con el papel de la suerte; acciones con dueño nominal, fecha y tipo; y publicación con acceso amplio, porque un postmortem que solo lee el equipo que lo escribió no cumple su función.

Un criterio de plazo: se publica dentro de los cinco días hábiles. Más allá de eso, los detalles se pierden, las personas rotan y el documento se escribe con lo que quedó en el canal de chat.

### Antipatrones

**El postmortem con culpa disimulada.** No nombra a nadie pero la redacción reparte responsabilidades: «el procedimiento no se siguió», «no se verificó antes de desplegar». El equipo lo lee y entiende perfectamente a quién se refiere, con lo cual se pagan todos los costos de la atribución de culpa sin ninguno de los beneficios de la franqueza.

**La causa raíz única.** Un incidente, una causa, una acción. Los sistemas complejos no fallan así, y el análisis reduccionista produce acciones correctivas que no evitan la reincidencia porque atacaron una condición entre varias.

**Acción correctiva sin dueño ni fecha.** «Mejorar el monitoreo.» Nadie la toma, nadie la reclama, y aparece idéntica en el postmortem del incidente siguiente.

**Acción correctiva que es un proceso.** «Ser más cuidadosos al desplegar.» No es accionable, no es verificable y traslada al comportamiento humano un problema que el sistema debería resolver. La versión útil de esa intención es siempre un control automático o una verificación obligatoria.

**El postmortem sin impacto cuantificado.** «Hubo una degradación del servicio.» Sin números no se puede priorizar nada, y las acciones correctivas pierden todas las discusiones de backlog contra funcionalidades que sí traen números.

**La línea de tiempo reconstruida sin fuentes.** Escrita de memoria tres semanas después, con horas aproximadas. Los intervalos de detección y respuesta, que son los datos que orientan las mejoras, quedan inservibles.

**El postmortem que no toca ningún runbook.** Si el análisis fue bueno, algo tiene que cambiar en los procedimientos: un diagnóstico nuevo, un umbral distinto, una verificación agregada. Cuando ninguna acción modifica documentación operativa, o el incidente era trivial o el análisis se quedó en la superficie.

**El archivo de postmortems.** Se escriben, se publican, no se leen nunca más y no se revisan patrones entre ellos. La revisión trimestral del conjunto —qué causas se repiten, qué acciones siguen abiertas— es la que convierte los documentos en aprendizaje.

**Analizar todo.** Sin umbral, el equipo escribe postmortems de incidentes de dos minutos, se cansa, y la calidad de todos cae al mismo nivel.

---

## 9. Anexo — Plantilla comentada

```markdown
---
doc_id: PM-AAAA-MM-DD
doc_type: postmortem
title: <Qué falló, en términos de impacto sobre el usuario>
status: borrador | en revisión | cerrado
origin: human
owner: <quien estuvo de guardia — no el más experimentado del equipo>
severity: crítica | alta | media
incident_date: AAAA-MM-DD
published: AAAA-MM-DD
impact_minutes: <minutos de impacto sobre usuarios>
related_runbooks: [RB-xxx]
audience: [humano, agente]
---

# PM-AAAA-MM-DD — <Título>

## 1. Resumen
<!-- Cinco líneas: qué pasó, cuánto duró, qué lo causó, cómo se restauró.
     Debe ser legible por alguien que no conoce el sistema. Es lo único que
     la mayoría de los lectores va a leer. -->

## 2. Impacto
<!-- Tabla con números: duración, usuarios afectados, operaciones fallidas,
     consumo de error budget. Si hubo pérdida o corrupción de datos, va acá
     y en primer lugar. "Degradación del servicio" no es un impacto. -->

## 3. Línea de tiempo
<!-- Diagrama Mermaid (timeline) más tabla si hace falta detalle.
     Marcar por separado: condición previa, inicio del impacto, detección,
     toma del incidente, acciones, restauración, cierre.
     Los dos intervalos que orientan las mejoras son
     inicio→detección e inicio-de-respuesta→restauración: hacerlos visibles.
     Distinguir lo registrado de lo reconstruido después. -->

## 4. Análisis de causa
### 4.1 Causa inmediata
### 4.2 Cadena causal
<!-- Cinco porqués como exploración, no como conclusión. Declarar dónde se
     detuvo y por qué; si se decidió no seguir, registrar lo que quedó fuera. -->
### 4.3 Factores contribuyentes
<!-- Tabla por categoría: técnico | detección | respuesta | organizativo.
     Cada factor enlaza a la acción correctiva que lo atiende. Un factor sin
     acción es una decisión de no actuar y conviene que sea explícita. -->

## 5. Qué funcionó, qué no, y qué fue suerte
<!-- Las tres cosas. La tercera es la que más se omite y la que evita
     concluir que el proceso es mejor de lo que garantiza. -->

## 6. Acciones correctivas
<!-- Tabla: ID | acción | tipo (prevención/detección/mitigación/respuesta) |
     dueño NOMINAL | fecha | estado.
     Nada de "revisar" ni "mejorar": cada acción necesita criterio de
     terminación verificable. Entran al backlog con prioridad, no como deseo. -->

## 7. Lecciones
<!-- Lo generalizable más allá de este incidente. Qué creencia sobre el
     sistema resultó falsa. Es la sección que hace útil el documento para
     quien no vivió el incidente. -->

## 8. Anexos
<!-- Evidencia: consultas capturadas, gráficos, enlaces al canal del
     incidente y al ticket. Con procedencia y marca de tiempo. -->
```

Los campos `impact_minutes` y `related_runbooks` del frontmatter existen para que el conjunto de postmortems sea consultable como corpus y no solo legible de a uno. Con ellos, la revisión trimestral —qué causas se repiten, qué runbooks concentran incidentes, cuánto impacto acumulado hubo por componente— se resuelve por parseo en lugar de por lectura, que es la diferencia entre hacerla y postergarla.
