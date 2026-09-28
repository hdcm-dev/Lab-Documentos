---
doc_id: TEM-OPERACION
doc_type: tema
title: Operación y resiliencia
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [FAM-DESP, TEM-TOPOLOGIA, TEM-DISTRIBUCION, TEM-RNF, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Operación y resiliencia — `TEM-OPERACION`

## Resumen ejecutivo

La tercera pregunta del despliegue es la que separa un informe memorable de uno correcto: *¿qué hace el sistema cuando algo falla?* En un `CTX-3` como el sistema de audiencias, la respuesta no es un anexo de contingencia sino el corazón de la arquitectura, porque el software corre en el borde y tiene que seguir funcionando cuando el centro no está. Los tres comportamientos que definen el sistema —iniciar y grabar una audiencia aunque el backend y el frontend estén caídos, recuperar el estado y continuar tras la caída del programa de escritorio, y seguir subiendo los videos en segundo plano después de cerrar la audiencia— son exactamente lo que un lector técnico quiere entender, y lo que un informe descuidado reduce a un «el sistema es tolerante a fallos» que no dice nada.

Este documento no enseña a *construir* esos mecanismos ni a *operar* el sistema: enseña cómo el **informe los narra**. La operación cotidiana, los procedimientos de recuperación y los runbooks viven en la [Operations Guide](../../Documentacion-Tecnica/50-Operativa/Operations-Guide.md) (`DOC-OPERACION`) y el [Runbook](../../Documentacion-Tecnica/50-Operativa/Runbook.md) (`DOC-RUNBOOK`) de la guía hermana, y el informe remite a ellos. Lo que aquí se trata es cómo describir una conducta resiliente de modo que se pueda evaluar: qué la dispara, qué estado persiste localmente y cómo se reconcilia cuando el enlace vuelve.

La resiliencia tiene dos caras en el informe y este documento cubre una. Como **atributo de calidad** —fiabilidad, en el vocabulario de `N-04`— la trata [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md), que dice qué se exige y cómo se mide. Como **conducta de despliegue** —qué hace el sistema, observable en ejecución— la trata este tema. El informe las describe una vez de cada lado, sin contradecirse.

---

## Definición

### Qué es

La **operación** es el comportamiento del sistema en régimen normal: qué procesos corren, qué hacen, cómo se los observa. La **resiliencia** es su comportamiento cuando algo se aparta de lo normal: un enlace que se cae, un proceso que muere, un host que se reinicia. Describir la resiliencia en un informe es narrar, para cada modo de fallo relevante, tres cosas: **qué lo dispara**, **qué estado sobrevive localmente** al fallo, y **cómo se reconcilia** ese estado con el resto del sistema cuando la condición normal se restablece.

Tres patrones cubren la mayoría de los comportamientos resilientes de un sistema en el borde, y los tres aparecen en el ejemplo de audiencias:

| Patrón | Qué resuelve | En audiencias |
|---|---|---|
| **Operación degradada** (*offline-first*) | Seguir funcionando con capacidades reducidas cuando un componente remoto no está | Iniciar y grabar con el backend y el frontend caídos |
| **Recuperabilidad** (*recoverability*) | Restaurar el estado tras la caída de un proceso local y continuar | Recuperar la audiencia tras la caída del escritorio |
| **Cola de trabajo diferido** (*deferred work queue*) | Desacoplar una tarea lenta del flujo interactivo y ejecutarla en segundo plano | Subir los videos después de cerrar la audiencia |

Ninguno es exclusivo de audiencias; son formas generales que el informe nombra para que el lector reconozca de qué mecanismo se trata en lugar de leer una descripción a medida cada vez.

### Qué problema resuelve

**Convertir una promesa en una conducta evaluable.** «Tolerante a fallos» es una etiqueta; «al perder el enlace con el centro, el escritorio sigue grabando contra el estado local y sincroniza al reconectar» es un mecanismo que un `ACT-04` puede verificar cortando el cable. La descripción de resiliencia da al lector algo contra qué probar, y esa es la diferencia entre un informe que inspira confianza y uno que la pide.

**Anticipar la pregunta del auditor.** `ACT-08` pregunta siempre lo mismo: «¿cómo sé que esto es así?». Para la resiliencia, la respuesta es describir el disparador, el estado local y la reconciliación con suficiente precisión como para que el auditor diseñe la prueba que lo confirma. Un informe que afirma resiliencia sin describir el mecanismo no sobrevive a esa pregunta.

**Dimensionar el riesgo real de la operación.** Saber qué se pierde y qué se conserva ante cada fallo permite decidir cuánto invertir en prevenirlo. Si la caída del centro no interrumpe una grabación pero sí retrasa la publicación, el riesgo se ubica donde está y no donde el miedo lo pondría.

### Qué NO es, y con qué se lo confunde

**No es el runbook.** El informe describe *qué* hace el sistema ante un fallo y *por qué* su diseño lo permite; el procedimiento que un operador ejecuta cuando el fallo ocurre —comandos, verificaciones, escalamiento— es `DOC-RUNBOOK`. Confundirlos llena el informe de pasos operativos y lo desactualiza a cada cambio de procedimiento.

**No es el plan de recuperación ante desastres.** La resiliencia que trata este tema es la del funcionamiento cotidiano —el enlace que se cae varias veces por día, el proceso que muere—, no la catástrofe del centro de datos entero. El DR es un documento aparte; mezclarlo diluye lo que el lector de un `CTX-3` necesita, que es la resiliencia ordinaria del borde.

**No es la lista de requisitos no funcionales.** [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md) enuncia el requisito —«disponibilidad de grabación del 99,9 % con el centro caído»— y su métrica; este tema describe el mecanismo que lo cumple. El requisito dice qué se exige; la operación dice cómo se logra. Un informe que solo lista requisitos sin describir mecanismos afirma sin demostrar.

**No es «alta disponibilidad» sin más.** Replicar el backend en tres instancias mejora la disponibilidad del centro, pero no resuelve nada de lo que define a `CTX-3`: la audiencia tiene que grabarse aunque el centro entero —replicado o no— sea inalcanzable desde la sala. La resiliencia del borde no se compra con redundancia en el centro.

---

## Cómo se describe cada uno de los tres comportamientos

El informe describe cada mecanismo con la misma estructura de tres partes —disparador, estado local, reconciliación—, lo que permite compararlos y verifica que ninguno quede a medias.

### Operación degradada: grabar con el centro caído

**Disparador.** El enlace entre la terminal y el centro se pierde, o el backend no responde. El escritorio y el Worker lo detectan y pasan a modo degradado sin interrumpir lo que estén haciendo.

**Estado local.** La audiencia se inicia y se conduce contra un **estado local** en la terminal —un archivo o una base local— que no depende del centro. El Worker sigue capturando de las cámaras y escribiendo el video en disco local. La grabación no sabe ni le importa si el backend está vivo.

**Reconciliación.** Cuando el enlace vuelve, el escritorio envía al backend el estado de la audiencia y el Worker envía los metadatos acumulados; el centro los integra. La reconciliación es **eventual**: el centro converge al estado de la terminal cuando puede, no en el instante en que el hecho ocurre. El informe debe decir qué gana el conflicto si el centro y la terminal divergen —en audiencias, la terminal, porque es la fuente de verdad de lo que pasó en la sala—.

### Recuperabilidad: continuar tras la caída del escritorio

**Disparador.** El programa de escritorio se cierra de forma anómala —cuelgue, cierre accidental, reinicio de la terminal— en mitad de una audiencia.

**Estado local.** Porque la audiencia se conduce contra el estado local persistido y no contra la memoria del proceso, ese estado sobrevive a la muerte del escritorio. El Worker, que corre como servicio de Windows independiente (`N-13`), puede seguir grabando aunque el escritorio no esté.

**Reconciliación.** Al reabrirse, el escritorio lee el estado local, detecta que hay una audiencia en curso y **ofrece continuarla** en lugar de empezar de cero. La reconciliación aquí es entre el proceso reiniciado y el estado que dejó; el informe describe qué se recupera (la audiencia y su punto) y qué no (lo que solo vivía en memoria).

### Cola de trabajo diferido: subir los videos al cerrar

**Disparador.** El operador cierra la audiencia. La subida de los videos —archivos grandes, enlace posiblemente inestable— no debe bloquear el cierre ni impedir iniciar la siguiente audiencia.

**Estado local.** Los videos quedan en disco local y se **encolan** para subida. La cola persiste localmente, de modo que sobrevive a un reinicio de la terminal: una subida a medias no se pierde ni se reinicia desde cero si el proceso se corta.

**Reconciliación.** El Worker consume la cola en segundo plano y sube cada video al servidor de archivos, reintentando ante fallos de red. Cuando una subida termina, informa al backend que el video está disponible. El operador, mientras tanto, ya inició la audiencia siguiente. Aquí es donde la elección de protocolo de subida importa, y el informe lo explica.

---

## Transferencia de archivos: FTP frente a tus

El canal de subida de videos admite dos opciones, y la diferencia entre ellas es exactamente la propiedad que un `CTX-3` necesita.

**FTP** (`N-08`) es un estándar de Internet de 1985 —STD 9—, cuyo objetivo declarado es transferir archivos de forma fiable y eficiente. Es simple, universal y suficiente cuando el enlace es estable. Su límite aparece con archivos grandes sobre enlaces inestables: una transferencia interrumpida a mitad de camino, por defecto, se reinicia desde cero.

**tus** (`F-01`) es un protocolo abierto de **subida reanudable sobre HTTP**, estable desde 2016. Su núcleo obligatorio: un `HEAD` para conocer el *offset* actual de una subida en curso, un `PATCH` para enviar datos desde ese *offset*, y las cabeceras `Upload-Offset` y `Tus-Resumable` que coordinan el proceso. La propiedad que aporta es la reanudación: si el enlace se corta al 80 % de un video de dos horas, la subida retoma desde ese 80 % en lugar de empezar de nuevo. Para el sistema de audiencias —videos grandes, enlaces del borde posiblemente flojos— esa propiedad no es un lujo: es lo que hace viable la cola de trabajo diferido.

Existe además un intento de estandarizar el mecanismo en el IETF, `draft-ietf-httpbis-resumable-upload` (`F-02`). Es un **Internet-Draft en curso, no un RFC**, y el informe lo cita como tal; su número de revisión debe reverificarse al citarlo porque los drafts incrementan. Presentarlo como estándar sería un error de nivel de autoridad de los que `MARCO-CONVENCIONES` advierte.

El criterio de redacción que esta guía recomienda: el informe no elige el protocolo por moda sino por la propiedad que el sistema necesita, y lo dice. «Se usa tus porque los videos son grandes y el enlace del borde se corta, y una subida no reanudable reiniciaría desde cero» es una justificación evaluable; «se usa tus porque es moderno» no lo es.

---

## El flujo de subida diferida, de punta a punta

El siguiente diagrama es **sintético** y narra la subida diferida desde que el operador cierra la audiencia hasta que el backend marca el video como disponible, incluyendo un corte de enlace y su reanudación con tus (`F-01`).

```mermaid
sequenceDiagram
    actor Op as Operador
    participant Desk as Escritorio
    participant Wrk as Worker (servicio)
    participant Q as Cola local (persistente)
    participant FS as Servidor de archivos (tus)
    participant Api as Backend (centro)

    Op->>Desk: Cierra la audiencia
    Desk->>Wrk: Señala fin de grabación
    Wrk->>Q: Encola el video para subida
    Desk-->>Op: Audiencia cerrada (puede iniciar la siguiente)

    Note over Wrk,FS: En segundo plano, sin bloquear al operador
    Wrk->>FS: PATCH · sube desde offset 0
    FS-->>Wrk: Upload-Offset creciente
    Note over Wrk,FS: Se corta el enlace al 80 %
    Wrk->>Q: Marca subida incompleta (offset guardado)

    Note over Wrk,FS: El enlace vuelve; el Worker reintenta
    Wrk->>FS: HEAD · consulta offset actual
    FS-->>Wrk: Upload-Offset = 80 %
    Wrk->>FS: PATCH · reanuda desde 80 %
    FS-->>Wrk: 204 · subida completa
    Wrk->>Q: Marca el ítem como subido
    Wrk->>Api: Notifica: video disponible
    Api-->>Wrk: Registrado
```

Dos detalles del diagrama son los que un informe de `CTX-3` debe hacer visibles. El primero es la respuesta temprana al operador —«puede iniciar la siguiente»— antes de que la subida empiece: es la propiedad que define el requisito, y el diagrama la ubica donde ocurre. El segundo es el par `HEAD`/`PATCH` tras el corte: la reanudación desde el *offset* guardado, que es la razón concreta por la que se eligió tus sobre FTP, y no una decoración del flujo.

---

## Aplicación por escenario

### `ESC-1` — Resiliencia prevista

Se describe cómo se *piensa* resolver cada fallo, marcando qué está decidido y qué es hipótesis. Es el escenario donde la resiliencia se gana o se pierde en el papel, porque los tres mecanismos —operación degradada, recuperabilidad, cola diferida— condicionan la arquitectura entera y rehacerlos después es carísimo. La trampa de `ESC-1` aquí es describir una resiliencia de nivel bancario para un sistema de dos salas: el informe debe dimensionar el mecanismo al riesgo real.

### `ESC-2` — Resiliencia real

Se describe cómo el sistema *efectivamente* se comporta ante fallos, confrontable con el sistema en ejecución. Es donde se paga la honestidad: si el informe dice «recupera ante la caída del escritorio» pero en producción la recuperación pierde los últimos dos minutos de audiencia, eso se declara. Un informe de `ESC-2` que describe la resiliencia idealizada en lugar de la observada engaña justo en el punto que más importa.

### `ESC-3` — Resiliencia en transición

Cuando el mecanismo cambia —migrar la subida de FTP a tus para ganar reanudación, por ejemplo— el informe describe el comportamiento actual, el objetivo y la convivencia. `MARCO-ESCENARIOS` lo advierte: una migración se justifica casi siempre por un atributo de calidad que la solución actual no alcanza, y en resiliencia ese atributo suele ser fiabilidad de la subida o tiempo de recuperación. El informe costea el viaje, no solo vende el destino.

### `ESC-4` — Resiliencia observada

Se juzga la resiliencia de un sistema ajeno, y el trabajo es detectar qué mecanismo falta o está solo afirmado. Un informe que promete operación offline sin describir dónde persiste el estado local ni cómo reconcilia tiene un hueco. Donde no hay informe, se prueba: cortar el enlace y observar si la grabación sigue es la verificación directa, y su resultado se registra distinguiendo lo observado de lo inferido.

### Qué cambia según el contexto

| Contexto | Peso del tema | Qué describir | Riesgo |
|---|---|---|---|
| `CTX-1` Monolito | Bajo | Reinicio del proceso, respaldo de la base | Inventar resiliencia distribuida que el sistema no tiene |
| `CTX-2` Cliente-servidor | Medio | Qué hace el cliente si el backend no responde; reintentos | Suponer que el enlace nunca falla |
| `CTX-3` Borde distribuido | **Máximo** | Operación degradada, recuperabilidad y trabajo diferido, cada uno con disparador, estado local y reconciliación | Reducirlo a «tolerante a fallos»; es el peor error del tema |
| `CTX-4` Multiservicio | Alto | Resiliencia entre servicios: reintentos, circuit breakers, consistencia eventual | Confundir redundancia del centro con resiliencia del borde |

El caso de `CTX-3` es el que da sentido a todo el documento. `MARCO-CONTEXTOS` lo dice: los mecanismos de recuperación ante fallos locales, las colas de trabajo diferido y la sincronización eventual son la parte más interesante de la arquitectura y la que el lector técnico más quiere entender. Un informe de `CTX-3` que trata la resiliencia como un párrafo de cierre invirtió el peso al revés.

---

## Ejemplos concretos

Los fragmentos son **sintéticos** y pertenecen al informe del sistema de audiencias.

### Fragmento — operación degradada, descrita como conducta

> **Grabación con el centro inalcanzable.** La audiencia se inicia y se conduce contra un estado local en la terminal, independiente del backend. Al detectar que el centro no responde, el escritorio pasa a modo degradado: deshabilita las funciones que requieren el centro —consultar el histórico, publicar— y conserva las que no —iniciar, grabar, pausar, cerrar—. El Worker sigue capturando de las cámaras y escribiendo el video en disco local sin interrupción. Cuando el enlace se restablece, el escritorio envía el estado de la audiencia y el Worker los metadatos acumulados; el centro los integra y, ante divergencia, prevalece el estado de la terminal por ser la fuente de verdad de lo ocurrido en la sala. La reconciliación es eventual: el centro converge cuando el enlace lo permite, no en el instante del hecho.

El fragmento describe disparador, estado local y reconciliación, y nombra la regla de resolución de conflictos. Un `ACT-04` puede diseñar la prueba —cortar el enlace, grabar, reconectar, verificar convergencia— porque el texto le dice qué esperar.

### Fragmento — la nota que cruza a requisitos

> Los tres comportamientos de esta sección satisfacen los requisitos no funcionales `RNF-01` (disponibilidad de grabación con el centro caído), `RNF-02` (recuperación ante caída del escritorio) y `RNF-03` (subida diferida no bloqueante), cuya métrica y umbral se definen en [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md). Esta sección describe el mecanismo; aquel documento fija qué se exige y cómo se mide.

El cruce explícito es criterio de redacción de esta guía: la resiliencia aparece dos veces en el informe —como conducta y como atributo de calidad— y el texto debe enlazar ambas para que el auditor recorra la trazabilidad sin reconstruirla.

---

## Preguntas guía

- Para cada modo de fallo relevante, ¿describí qué lo dispara, qué estado sobrevive localmente y cómo se reconcilia al volver a la normalidad?
- ¿Dije qué gana el conflicto cuando el estado local y el del centro divergen?
- ¿La subida diferida responde al operador antes de completarse, y el informe ubica esa respuesta donde ocurre?
- ¿Justifiqué la elección de protocolo de archivos por la propiedad que el sistema necesita, y cité tus como `F-01` y el draft como `F-02` (no como RFC)?
- ¿Describí la resiliencia real (`ESC-2`) o la idealizada? ¿Un corte de enlace confirmaría lo que afirmo?
- ¿Enlacé cada mecanismo con su requisito no funcional en `TEM-RNF`, en lugar de describir dos veces sin conectar?
- ¿Remití la operación cotidiana y los procedimientos a `DOC-OPERACION` y `DOC-RUNBOOK` en vez de meterlos en el informe?

---

## Criterios de calidad

### Resiliencia bien narrada

Cada comportamiento resiliente se describe con su disparador, su estado local y su reconciliación, en términos que permiten diseñar una prueba. La regla de resolución de conflictos está enunciada. La elección de protocolo de archivos se justifica por la propiedad que el sistema necesita, con el nivel de autoridad correcto —tus como convención de facto `F-01`, el draft del IETF como `F-02`—. Los mecanismos se cruzan con los requisitos no funcionales que satisfacen, y lo operativo se remite a los documentos operativos. El lector termina sabiendo qué se pierde y qué se conserva ante cada fallo.

### Resiliencia mal narrada y antipatrones

**«Tolerante a fallos» y nada más.** Afirmar resiliencia sin describir el mecanismo. Es el peor antipatrón del tema en `CTX-3`, porque promete lo más importante y no lo demuestra; el auditor no tiene contra qué probar.

**El mecanismo a medias.** Describir el disparador y el estado local pero callar la reconciliación —qué pasa cuando el enlace vuelve—. La reconciliación es donde viven los problemas reales (conflictos, orden, duplicados), y omitirla esconde justo lo difícil.

**Confundir redundancia con resiliencia del borde.** Presentar tres réplicas del backend como respuesta a «¿qué pasa si el centro está caído desde la sala?». La redundancia del centro no ayuda si el enlace a la sala no existe.

**El protocolo por moda.** «Usamos tus porque es moderno» en lugar de «porque los videos son grandes y el enlace se corta, y una subida no reanudable reiniciaría desde cero». El primero no es una justificación; el segundo sí.

**El draft citado como estándar.** Presentar `F-02` como si fuera un RFC. Es un Internet-Draft en curso, y confundir el nivel de autoridad es el error que `MARCO-CONVENCIONES` advierte.

**El runbook dentro del informe.** Llenar la sección con comandos y pasos de recuperación que pertenecen a `DOC-RUNBOOK`. El informe describe la conducta y su razón; el procedimiento se referencia.

**La resiliencia idealizada en `ESC-2`.** Describir cómo *debería* recuperarse el sistema en lugar de cómo se recupera. Si la recuperación pierde los últimos minutos, se declara; ocultarlo engaña en el punto más sensible.

---

## Anexo — Ficha de comportamiento resiliente

Se completa una entrada por modo de fallo relevante. Las tres columnas centrales —disparador, estado local, reconciliación— son obligatorias: una vacía es un mecanismo descrito a medias.

```yaml
resiliencia:
  escenario: ESC-?
  contexto: CTX-?
  comportamientos:
    - nombre: ""
      patron: operacion_degradada | recuperabilidad | cola_diferida | otro
      disparador: ""                  # qué evento activa el comportamiento
      estado_local:
        que_persiste: ""              # qué sobrevive al fallo, y dónde
        soporte: archivo | base_local | cola_persistente
      reconciliacion:
        cuando: ""                    # al reconectar, al reabrir el proceso...
        resolucion_de_conflicto: ""   # qué gana si el estado local y el central divergen
        es_eventual: si | no
      requisito_no_funcional: ""       # RNF-xx que satisface; ver TEM-RNF
      verificacion: ""                 # cómo se prueba (p. ej. cortar el enlace)
  transferencia_de_archivos:
    protocolo: ftp | tus
    fuente: N-08 | F-01
    reanudable: si | no
    justificacion: ""                  # la propiedad que el sistema necesita
  remisiones:
    operacion_cotidiana: DOC-OPERACION
    procedimientos_de_fallo: DOC-RUNBOOK
  afirmaciones_no_verificadas: []
```
