---
doc_id: MARCO-ACTORES
doc_type: marco-de-referencia
title: Actores
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-19
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, MAPA-CONCEPTUAL]
---

# Actores — `MARCO-ACTORES`

## Resumen ejecutivo

Seis roles intervienen en las decisiones que trata esta guía, y cada uno tiene un alcance distinto de autoridad. La distinción que más conflictos evita es la que separa **quién fija una convención** de **quién la aplica** y de **quién la hace cumplir automáticamente**: cuando esos tres se confunden, las convenciones se discuten en cada revisión de código en lugar de discutirse una vez.

En equipos pequeños una misma persona ejerce varios roles. El marco sigue siendo útil porque lo que importa no es la asignación de personas sino la separación de decisiones: incluso quien decide solo se beneficia de saber si está decidiendo como arquitecto o aplicando como desarrollador.

---

## Los seis actores

| ID | Actor | Decide | No decide |
|----|-------|--------|-----------|
| `ACT-01` | Arquitecto de software | Partición en servicios, estructura de solución, modelo de capas | Detalles de formato y nombrado |
| `ACT-02` | Desarrollador | Organización interna de su módulo, nombres concretos | Convenciones globales |
| `ACT-03` | Responsable técnico | El conjunto de convenciones del equipo y sus excepciones | La arquitectura del sistema |
| `ACT-04` | Revisor de código | Si un cambio cumple lo acordado | Cambiar lo acordado durante la revisión |
| `ACT-05` | Ingeniero DevOps | Cómo se automatiza y se hace cumplir la convención | Cuál es la convención |
| `ACT-06` | Mantenedor de biblioteca | Superficie pública, versionado, política de cambios ruptores | Organización interna de los consumidores |

---

## `ACT-01` — Arquitecto de software

Responde por las decisiones caras de revertir: cuántas unidades desplegables tiene el sistema, cómo se reparten las responsabilidades entre ellas, qué proyectos componen cada solución y en qué dirección fluyen las dependencias.

Su intervención se concentra en `ESC-1` y `ESC-2`. En `ESC-3` no tiene mucho que aportar —la normalización de estilo no es una decisión arquitectónica— y en `ESC-4` participa solo cuando la evaluación alcanza la estructura.

El criterio que define un buen ejercicio del rol es la **contención**: la mayor parte del valor que aporta un arquitecto está en las estructuras que decide *no* introducir todavía. Cada capa, cada proyecto y cada servicio adicional tiene un costo permanente de mantenimiento que se paga aunque el beneficio previsto nunca se materialice.

Registra sus decisiones en ADR. Una partición de servicios sin registro es una decisión que en dos años nadie puede evaluar, porque las restricciones que la motivaron ya no están a la vista.

---

## `ACT-02` — Desarrollador

Escribe el código y toma cientos de decisiones pequeñas por día: cómo se llama esta variable, en qué carpeta va esta clase, si este método merece extraerse.

Su autoridad es local y real. Dentro del módulo que le toca, la organización de archivos y los nombres concretos son suyos, siempre que respeten las convenciones globales. Lo que no le corresponde es cambiar unilateralmente una convención del equipo, aunque tenga razón: eso se propone a `ACT-03`, y la razón es que el costo de la inconsistencia supera casi siempre al beneficio de la mejora puntual.

En `ESC-3` es quien ejecuta la normalización. En `ESC-4` es quien recibe la observación, y conviene explicitar algo: una observación sobre un nombre no es una observación sobre la persona, y tratarla como tal es la fuente más común de fricción improductiva en las revisiones.

---

## `ACT-03` — Responsable técnico

Fija el conjunto de convenciones del equipo, resuelve las discusiones que no se resuelven solas y autoriza las excepciones.

Es el rol que esta guía considera peor cubierto en la práctica. Muchos equipos no lo tienen asignado, y el resultado es reconocible: las convenciones se negocian en cada *pull request*, la misma discusión sobre `var` o sobre el orden de los `using` vuelve cada dos meses, y el código termina exhibiendo tantos estilos como personas pasaron por él.

Su producto concreto es el archivo `.editorconfig` del repositorio y el documento que registra las decisiones que no se pueden expresar ahí. Toda regla que fije debería poder responder a la pregunta: ¿esto lo verifica una herramienta o depende de que un humano lo recuerde? Si depende de un humano, el rendimiento es bajo y conviene descartar la regla o encontrar cómo automatizarla ([`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)).

---

## `ACT-04` — Revisor de código

Evalúa cambios ajenos antes de que entren. Es el ejercicio típico de `ESC-4` en su forma cotidiana.

La regla que ordena el rol: **el revisor verifica el cumplimiento de lo acordado, no impone su preferencia**. Una revisión que introduce criterios nuevos convierte cada *pull request* en una negociación y desincentiva los cambios pequeños, que son justamente los que conviene fomentar.

Cuando el revisor detecta que la convención acordada está mal, el camino es aprobar el cambio si cumple lo vigente y llevar la objeción a `ACT-03` por separado. Bloquear un cambio correcto para discutir la norma que lo rige mezcla dos conversaciones con tiempos distintos.

Lo que sí le corresponde señalar sin consultar a nadie: inconsistencias internas, dependencias que violan la dirección establecida, y nombres que no describen lo que la cosa hace.

---

## `ACT-05` — Ingeniero DevOps

Convierte las convenciones en verificaciones automáticas: configura los analizadores, el `Directory.Build.props`, la gestión centralizada de paquetes y las compuertas de la canalización de integración continua.

La distinción con `ACT-03` es de naturaleza, no de jerarquía: `ACT-03` decide qué regla rige, `ACT-05` decide si se verifica en el build local, en el *commit* o en la canalización, y con qué severidad. Esa segunda decisión tiene consecuencias prácticas fuertes. Una regla que rompe el build local castiga la iteración; la misma regla como error solo en integración continua permite trabajar y bloquea la entrada.

El reparto asimétrico es el que esta guía recomienda: los avisos no se tratan como error en el build local —donde el desarrollador necesita compilar código a medio escribir— y la canalización fuerza `dotnet build -warnaserror`, de modo que un aviso nuevo no llega a integrarse. La misma regla, dos severidades según dónde se evalúa. El detalle de cómo se declara está en [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md).

---

## `ACT-06` — Mantenedor de biblioteca

Aparece solo en `CTX-3`. Responde por la superficie pública de un paquete o biblioteca compartida y por lo que le pasa a los consumidores cuando esa superficie cambia.

Es el único actor para el que las **Framework Design Guidelines** son de aplicación literal, y el único que enfrenta decisiones irreversibles en materia de nomenclatura: un tipo público mal nombrado se arrastra durante toda la vida del paquete, porque corregirlo obliga a una versión mayor y a que todos los consumidores editen su código.

Su disciplina característica es el versionado semántico y la política de obsolescencia: marcar con `[Obsolete]`, mantener el miembro anterior durante al menos una versión menor, y documentar el reemplazo en el propio atributo.

---

## Matriz de responsabilidad

Las decisiones que trata la guía, con el actor que las toma. `R` responsable, `C` consultado, `I` informado.

| Decisión | `ACT-01` | `ACT-02` | `ACT-03` | `ACT-04` | `ACT-05` | `ACT-06` |
|----------|:--------:|:--------:|:--------:|:--------:|:--------:|:--------:|
| Modelo de despliegue (monolito / distribuido) | **R** | C | C | – | C | – |
| Estructura de la solución y proyectos | **R** | C | C | – | C | C |
| Modelo de capas y dirección de dependencias | **R** | C | C | I | – | – |
| Organización de carpetas dentro de un módulo | C | **R** | I | C | – | – |
| Convenciones de nomenclatura del equipo | C | C | **R** | I | I | C |
| Estilo de formato y `.editorconfig` | – | C | **R** | I | C | – |
| Superficie pública de una biblioteca | C | C | C | C | – | **R** |
| Severidad de las reglas y compuertas de la canalización de integración continua | – | I | C | I | **R** | – |
| Aprobación de un cambio concreto | – | I | – | **R** | – | – |

---

## Preguntas guía

1. En la decisión que tengo enfrente, ¿qué rol estoy ejerciendo y tengo autoridad para tomarla?
2. ¿Hay alguien que ejerza `ACT-03` en este equipo? Si no, ¿dónde se están resolviendo las discusiones de convención —o no se están resolviendo?
3. ¿La observación que voy a hacer en esta revisión verifica lo acordado o introduce un criterio nuevo?
4. ¿Esta regla la verifica una herramienta o depende de que alguien la recuerde?
