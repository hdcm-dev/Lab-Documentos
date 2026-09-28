---
doc_id: TEM-ERRORES
doc_type: tema
title: Errores frecuentes de redacción
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [TEM-ESTRUCTURA, TEM-CRITERIO, FAM-NAT, TEM-RNF, TEM-VISTAS, TEM-OPERACION, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, MARCO-CONVENCIONES, ANEXO-REFERENCIAS]
---

# Errores frecuentes de redacción — `TEM-ERRORES`

## Resumen ejecutivo

Los errores que arruinan un informe de solución no suelen ser de contenido técnico. El autor conoce el sistema; los componentes que nombra existen, la topología es la que es. Lo que falla es cómo se comunica: se describe la estructura de carpetas creyendo que se describió la arquitectura, se afirma una intención con el tono de un hecho, se vende una migración sin costear el viaje, se atribuye a «el estándar» lo que es criterio propio. Son patrones, se repiten entre autores y proyectos, y una vez nombrados se detectan y se corrigen.

Este documento los cataloga. Para cada uno registra tres cosas: el **síntoma** —cómo se reconoce en un borrador—, **por qué daña** —qué le cuesta al lector— y **cómo se corrige** —la reescritura que lo resuelve—. No es una lista de gustos de estilo: cada error tiene una consecuencia concreta sobre la decisión que el informe debía habilitar. El catálogo es la contracara del [criterio de redacción](Criterio-de-Redaccion.md): donde aquel forma el juicio para escribir bien, este reúne lo que hay que revisar para no entregar mal.

La cuarta sección adopta la variante de `FAM-RED`, con frases de referencia de «así no / así sí». Los ejemplos pertenecen al sistema de audiencias y son sintéticos.

---

## Definición

### Qué es un antipatrón de redacción

Un **antipatrón de redacción** es una forma de escribir que parece correcta, se comete con frecuencia y produce un daño previsible en el lector. Se distingue de un error de contenido: no es que la arquitectura esté mal, es que está mal contada. Un informe puede tener cero errores técnicos y estar lleno de antipatrones de redacción, y ese informe no sirve, porque el lector no llega al contenido correcto que hay debajo.

Se detectan por su síntoma —una marca reconocible en el texto— y se corrigen con una reescritura que ataca la causa, no la superficie. Nombrarlos es la mitad del trabajo: un redactor que sabe que «diseñado para» suele esconder una intención sin verificar, lo caza en su propio borrador.

### El catálogo

Nueve antipatrones, ordenados de más frecuente a más específico. Los tres primeros aparecen en casi todo informe; los últimos dependen del contexto o del escenario.

| # | Antipatrón | Síntoma | Escenario / contexto donde más aparece |
|---|---|---|---|
| 1 | Carpetas en vez de arquitectura | Árbol de proyectos como vista de arquitectura | `CTX-1` monolito |
| 2 | Intención en vez de realidad | «Diseñado para», «busca garantizar» en presente | `ESC-2` construida |
| 3 | Vender el destino sin costear el viaje | La arquitectura objetivo como evidentemente superior, sin costo | `ESC-3` evolución |
| 4 | Autoridad sin fuente | «Es el estándar», «REST lo exige», «la arquitectura obliga a» | Todos |
| 5 | Muro de detalle sin jerarquía | Cada componente con igual peso, sin distinguir lo central | `CTX-4` multiservicio |
| 6 | Despliegue subestimado | Tres párrafos de despliegue para un sistema en el borde | `CTX-3` borde distribuido |
| 7 | Resumen que no se entiende solo | El resumen remite a las demás secciones | Todos, ante `ACT-06` |
| 8 | Diagrama que no coincide con el texto | El diagrama muestra un componente que la prosa no menciona | Todos |
| 9 | RNF sin métrica | «Rápido», «escalable», «seguro» sin número ni mecanismo | Todos, agudo en `CTX-3` |

Lo que sigue desarrolla cada uno con su daño y su corrección.

---

## Los antipatrones, uno por uno

### 1 · Describir la estructura de carpetas en vez de la arquitectura

**Síntoma.** La vista de arquitectura es el árbol de proyectos del repositorio: `Core`, `Infrastructure`, `Application`, `Web`. Los nombres son de solución de Visual Studio, no de responsabilidades del sistema.

**Por qué daña.** El lector `ACT-03` quiere saber de qué responde cada componente y con qué habla, no cómo el autor organizó su código. El árbol de carpetas es una decisión de implementación que no le dice si el sistema separa bien sus responsabilidades ni cómo fluye una operación. `MARCO-CONTEXTOS` lo señala como el riesgo característico de `CTX-1`, donde la arquitectura lógica es lo único que da para desarrollar y tienta reemplazarla por lo más fácil de listar.

**Cómo se corrige.** Escribir componentes por responsabilidad y relación. En lugar de «el proyecto `Audiencias.Worker`», «el servicio en segundo plano, responsable de capturar de las cámaras, grabar y subir». El nombre del proyecto puede ir entre paréntesis para quien busque en el código, pero la unidad de descripción es la responsabilidad, no el directorio. La [vista de componentes](../20-Arquitectura/Vista-de-Componentes.md) desarrolla cómo se hace.

### 2 · Describir la intención en vez de la realidad

**Síntoma.** «El sistema está diseñado para escalar», «busca garantizar la disponibilidad», «tiene como objetivo procesar en tiempo real», en un informe de un sistema que ya existe.

**Por qué daña.** En `ESC-2` el sistema real está disponible para confrontarse, y una afirmación de intención elige describir el diseño en lugar del sistema. Peor: suele esconder que el comportamiento no se verificó. «Diseñado para escalar» es compatible con un backend que nunca escaló y no se sabe si escalaría. El lector cree estar leyendo una capacidad y está leyendo una aspiración. `MARCO-ESCENARIOS` lo nombra como la trampa central de `ESC-2`.

**Cómo se corrige.** Afirmar lo que el sistema hace, y calificar lo que no se probó. «Corre en una instancia; el diseño sin estado permite escalar, pero no se ejercita en producción» reemplaza una intención por un hecho más una posibilidad honesta. La regla operativa: buscar cada «diseñado para» y «busca» del borrador y reescribirlo señalando el sistema real.

### 3 · Vender el destino sin costear el viaje

**Síntoma.** En un informe de migración, la arquitectura objetivo se describe como evidentemente superior, con sus ventajas detalladas, y el costo, el riesgo y el período de convivencia están ausentes o despachados en una línea.

**Por qué daña.** El lector de `ESC-3` no pregunta «¿es buena esta arquitectura?» sino «¿es mejor que la que tengo y vale lo que cuesta migrar?». Un informe que omite el costo no responde esa pregunta: es una pieza de marketing con forma de informe técnico. Y la contracara —describir el estado actual con desprecio— es injusta y peligrosa, porque quien lo construyó suele estar entre los lectores.

**Cómo se corrige.** Costear el viaje con el mismo cuidado que se describe el destino: qué cuesta migrar, qué riesgo se corre, cómo se ve el sistema durante la convivencia de lo viejo y lo nuevo. Comparar estado actual y objetivo atributo por atributo, con respeto por lo que ya funciona. La vista de despliegue se vuelve doble, actual y objetivo, como pide [`TEM-ESTRUCTURA`](Estructura-del-Documento.md) para este escenario.

### 4 · Atribuir a «el estándar» lo que es criterio propio o plantilla

**Síntoma.** «Usamos REST porque es el estándar», «la arquitectura limpia obliga a esta separación», «según arc42 el informe debe tener estas doce secciones». Una prescripción presentada como norma cuando es criterio propio, convención de facto o una plantilla con autor.

**Por qué daña.** Invoca una autoridad que no existe, y con eso cierra la discusión que debería estar abierta. REST es un estilo, no una norma; arc42 es una plantilla (`G-01`), no un estándar; la «arquitectura limpia» es criterio. Presentarlos como obligatorios impide que el lector evalúe si la decisión conviene, y desprestigia al autor ante un lector que conoce la diferencia. [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md#los-cuatro-niveles-de-autoridad) llama a la confusión entre norma y marco el error de citación más frecuente del tema.

**Cómo se corrige.** Nombrar el nivel de autoridad. Lo normativo se cita por su identificador —`N-01` ISO/IEC/IEEE 42010:2022— con la cláusula; un marco se nombra como lo que es —«arc42 propone», «el modelo C4»—; una convención de facto se declara con su evidencia; el criterio propio se declara con la fórmula «esta guía recomienda» o «es criterio del equipo». Una decisión con su nivel de autoridad nombrado es defendible; sin nombrarlo, es un argumento de autoridad hueco.

### 5 · Muro de detalle sin jerarquía

**Síntoma.** Cada componente descrito con la misma extensión y el mismo nivel de detalle, sin que el texto distinga lo central de lo accesorio. El informe es plano: se puede leer entero y no saber qué importa más.

**Por qué daña.** El lector no tiene tiempo infinito y necesita que el informe le señale dónde mirar. Un documento donde el servicio crítico y el detalle marginal ocupan lo mismo obliga al lector a hacer la jerarquización que el autor no hizo, y la mayoría no la hace: se pierde. `MARCO-CONTEXTOS` lo identifica como el riesgo de `CTX-4`, donde la cantidad de piezas tienta a describirlas todas por igual.

**Cómo se corrige.** Jerarquizar. Desarrollar en el cuerpo lo central —en el sistema de audiencias, los tres comportamientos de resiliencia— y referenciar el resto al detalle que vive en otros documentos. Usar niveles de zoom cuando el sistema los pide: la [vista de diagramas](../20-Arquitectura/Vistas-y-Diagramas.md) trata cómo el modelo C4 organiza un sistema con muchas piezas sin aplanarlo. Un buen informe se lee y deja claro qué es lo importante en los primeros párrafos de cada sección.

### 6 · Despliegue subestimado

**Síntoma.** Un sistema con componentes en decenas de terminales, y una vista de despliegue de tres párrafos que dice «se instala en cada terminal» sin describir cómo. La operación degradada —qué hace el sistema cuando el centro está caído— aparece en una nota al pie, si aparece.

**Por qué daña.** En `CTX-3` el despliegue no es un acto sino un proceso repetido y distribuido, y la operación offline es un requisito no funcional de primer orden, no un detalle. Un informe que los subestima omite exactamente lo que el lector técnico más quiere entender y lo que el responsable de despliegue `ACT-04` necesita para hacer su trabajo. `MARCO-CONTEXTOS` es explícito: un informe de `CTX-3` que dedica tres párrafos al despliegue está mal calibrado.

**Cómo se corrige.** Dar a la instalación y a la resiliencia el espacio que el contexto justifica. Describir la instalación por terminal como un procedimiento —qué se instala, cómo se actualiza, qué falla si una versión no coincide— y narrar la operación degradada de punta a punta. [`TEM-OPERACION`](../30-Despliegue/Operacion-y-Resiliencia.md) desarrolla los tres comportamientos del sistema de audiencias, que son el corazón de su informe.

### 7 · Resumen ejecutivo que no se entiende solo

**Síntoma.** El resumen remite a las demás secciones —«según se detalla en la sección 5»— o repite el índice del documento en prosa, sin decir qué es la solución ni qué decisión habilita el informe.

**Por qué daña.** El decisor `ACT-06` no lee el documento entero: lee el resumen y confía en que el resto lo respalda. Un resumen que obliga a pasar de página para entender algo falló en servir al único lector que depende de él. `MARCO-ACTORES` lo dice sin rodeos: un informe que obliga al decisor a leer treinta páginas para saber si aprobar falló en servirle.

**Cómo se corrige.** Escribir el resumen para que se sostenga solo: qué es la solución, qué decisión habilita, los pocos hechos que sostienen esa decisión, sin jerga y sin remisiones. La prueba es leerlo aislado del resto y preguntarse si un decisor sabría qué se le pide. Si necesita el cuerpo para entenderse, no es un resumen.

### 8 · Diagramas que no coinciden con el texto

**Síntoma.** El diagrama de despliegue muestra un componente —una caché, un balanceador— que la prosa no menciona, o el texto describe un flujo que el diagrama no dibuja. Las dos caras cuentan historias distintas.

**Por qué daña.** El lector no sabe cuál creer, y la contradicción le resta confianza a todo el informe. `Rule-Dual-Audience` es explícita: las dos caras describen el mismo hecho, y ante divergencia hay que corregir, nunca mantener versiones paralelas. Un diagrama que no coincide con el texto suele ser un diagrama que quedó de una versión anterior y nadie actualizó, y delata que el informe no se revisó como un todo.

**Cómo se corrige.** Verificar que cada componente del diagrama aparezca en la prosa y viceversa, y que el flujo narrado sea el flujo dibujado. La narrativa complementa al diagrama, no lo repite ni lo contradice: el texto cuenta qué pasa desde que ocurre un evento, el diagrama muestra la estructura sobre la que pasa. Cuando cambia uno, se revisa el otro.

### 9 · Requisitos no funcionales sin métrica

**Síntoma.** «El sistema es rápido», «escalable», «seguro», «robusto». Adjetivos sin número, sin condición de medición y sin el mecanismo que los sostiene.

**Por qué daña.** Un RNF sin métrica no se puede verificar ni cumplir: «rápido» no dice cuánto, «disponible» no dice bajo qué medida. El lector no puede juzgar si la solución lo logra, y el auditor `ACT-08` no tiene contra qué contrastar. En `CTX-3` el daño es mayor, porque los RNF son el corazón del sistema: «opera con el centro caído» sin decir por cuánto tiempo ni con qué límites es una promesa vacía.

**Cómo se corrige.** Dar a cada RNF una métrica o, cuando la métrica no está fijada, el mecanismo que lo atiende y su límite conocido. «Opera con el centro caído» se vuelve «la grabación continúa localmente con el centro caído; los metadatos se encolan y se sincronizan al restablecerse el enlace; el límite es la capacidad de disco local de la terminal». [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md) desarrolla cómo se cuantifican contra las nueve características de `N-04` ISO/IEC 25010:2023.

---

## Aplicación por escenario

Cada escenario tiene un antipatrón que lo acecha más que los demás, y conocerlo permite revisar el borrador con la lupa correcta.

### `ESC-1` — Solución en diseño

El antipatrón dominante es una variante del número 2: describir en presente lo que todavía no existe. Como todo es intención, el riesgo no es confundir intención con realidad —no hay realidad aún— sino no marcar los estados: no distinguir lo decidido de lo propuesto y de lo pendiente. Un informe de `ESC-1` que escribe todo en presente de indicativo induce a decidir sobre una certeza que no existe.

### `ESC-2` — Solución construida

Domina el antipatrón 2 en su forma pura: intención en vez de realidad, con el sistema real disponible para confrontarse. Es el escenario del pedido que abre la guía y el que más castiga esta falla. El antipatrón 7 —resumen que no se entiende solo— también es frecuente aquí, porque el autor, inmerso en el sistema, escribe el resumen para alguien que ya lo conoce.

### `ESC-3` — Solución en evolución o migración

Domina el antipatrón 3: vender el destino sin costear el viaje. Es casi definitorio del escenario, porque la propuesta de migración tiende naturalmente al entusiasmo. Le sigue el olvido del estado de partida, que deja sin respuesta la pregunta sobre si conviene migrar.

### `ESC-4` — Evaluación de una solución ajena

Aquí los antipatrones no se cometen: se detectan. La habilidad de `ESC-4` es reconocer en el informe ajeno la carpeta descrita como arquitectura, la intención afirmada como hecho, el RNF sin métrica, la sección de despliegue ausente. El antipatrón propio del evaluador es puntuar la forma sobre el fondo: premiar un informe extenso y bien maquetado que describe una solución pobre. Este catálogo funciona, en `ESC-4`, como rúbrica de lectura.

### Qué cambia según el contexto

| Contexto | Antipatrón que más acecha |
|---|---|
| `CTX-1` monolito | 1 · Carpetas en vez de arquitectura |
| `CTX-2` cliente-servidor | Olvidar los contratos entre nodos (variante de 5) |
| `CTX-3` borde distribuido | 6 · Despliegue subestimado; 9 · RNF sin métrica |
| `CTX-4` multiservicio | 5 · Muro de detalle sin jerarquía |

---

## Frases de referencia

Pares de «así no / así sí». Fragmentos **sintéticos** del informe del sistema de audiencias, cada uno ilustrando la corrección de un antipatrón.

### Muro de detalle frente a jerarquía (antipatrón 5)

**Así no** — cada componente con igual peso, sin señalar qué importa:

> «El backend expone endpoints de audiencias, de usuarios, de configuración y de reportes. El frontend tiene páginas de listado, de detalle, de administración y de reproducción. El Worker gestiona cámaras, grabación, colas y subida.»

**Así sí** — lo central primero, el resto referenciado:

> «El Worker es el componente crítico: graba localmente y sube en diferido, de modo que la audiencia sobrevive a la caída del centro. El backend y el frontend administran y consultan esas grabaciones; su detalle de endpoints está en la [especificación de API](../../Documentacion-Tecnica/40-Diseno/API-Specification.md).»

El primero obliga al lector a adivinar qué importa; el segundo se lo dice y descarga el detalle donde vive.

### Despliegue subestimado frente a descrito (antipatrón 6)

**Así no:** «El programa de escritorio y el servicio se instalan en cada terminal.»

**Así sí:** «En cada terminal se instalan dos artefactos: el programa de escritorio (paquete MSIX, con actualización automática) y el servicio en segundo plano (servicio de Windows que arranca con el equipo). La instalación es una jornada por sala; una versión desalineada entre ambos se detecta al iniciar y bloquea la grabación hasta actualizar.»

En `CTX-3` el despliegue es el contenido, no una nota. La versión corta omite justo lo que `ACT-04` necesita.

### RNF sin métrica frente a cuantificado (antipatrón 9)

**Así no:** «El sistema es resiliente y opera aunque falle la conexión.»

**Así sí:** «Con el enlace al centro caído, la grabación continúa en la terminal sin interrupción; los metadatos se encolan localmente y se sincronizan al restablecerse el enlace. El límite es el disco local: con la capacidad actual, unas 40 horas de grabación por terminal antes de requerir sincronización.»

«Resiliente» no se puede verificar. La versión cuantificada nombra el mecanismo, el comportamiento y el límite, que es lo que el auditor va a contrastar.

### Resumen que remite frente a resumen que se sostiene (antipatrón 7)

**Así no:** «Este informe describe la arquitectura (sección 4), el despliegue (sección 5) y los requisitos (sección 6) del sistema de gestión de audiencias.»

**Así sí:** «El sistema graba audiencias en cada sala de forma autónoma: aunque el centro de datos esté caído, la grabación local continúa y se sincroniza después. Se solicita aprobar el despliegue en las doce salas de la primera sede.»

El primero es un índice en prosa; el segundo le dice al decisor qué hace el sistema y qué se le pide, sin abrir el cuerpo.

---

## Preguntas guía

- ¿Mi vista de arquitectura describe responsabilidades o el árbol de carpetas?
- ¿Cuántas veces digo «diseñado para» o «busca garantizar» en un informe de un sistema que ya existe?
- Si es una migración, ¿costeé el viaje o solo vendí el destino?
- ¿Cada prescripción declara si es norma, marco, convención o criterio propio?
- ¿El informe deja claro qué es lo central, o todo pesa igual?
- En `CTX-3`, ¿el despliegue y la resiliencia tienen el espacio que el lector busca, o los subestimé?
- ¿El resumen se entiende leído solo?
- ¿Cada componente del diagrama está en el texto, y cada RNF tiene métrica o mecanismo?

---

## Criterios de calidad

### Revisión buena

El borrador se recorre contra el catálogo antes de entregar, y cada antipatrón detectado se corrige atacando la causa, no maquillando el síntoma: la carpeta se reescribe como responsabilidad, la intención como hecho, el adjetivo como métrica. Las dos caras del informe coinciden —diagrama y prosa cuentan la misma historia— y el peso de cada sección refleja lo que importa. Un informe revisado así se lee y deja claro, en cada sección, qué es lo central y con qué autoridad se afirma cada cosa.

### Revisión pobre y antipatrones de la propia revisión

**Corregir el síntoma y no la causa.** Cambiar «rápido» por «muy rápido» no agrega métrica; sacar un «diseñado para» y dejar la intención en otra forma no la convierte en hecho. La corrección real reescribe la afirmación, no la reformula.

**Revisar por secciones y no como un todo.** Los antipatrones 8 —diagrama contra texto— y 7 —resumen que no se sostiene— solo se detectan leyendo el informe completo y confrontando sus partes. Una revisión sección por sección los deja pasar.

**Confundir extensión con calidad.** Agregar párrafos para que el informe «se vea completo» introduce relleno y muro de detalle, dos antipatrones, en nombre de la exhaustividad. Un informe denso y jerárquico sirve más que uno largo y plano.

---

## Anexo — Lista de verificación de antipatrones

Se recorre sobre el borrador terminado. Cada fila es un antipatrón del catálogo; la respuesta esperada es «no» en la columna de presencia. Complementa la [lista de verificación](../99-Anexos/Lista-de-Verificacion.md) integral.

```yaml
revision_de_antipatrones:
  contenido_mal_contado:
    carpetas_en_vez_de_arquitectura: presente | ausente        # CTX-1
    intencion_en_vez_de_realidad: presente | ausente           # ESC-2; contar "diseñado para"
    vender_destino_sin_costear_viaje: presente | ausente | na  # ESC-3
    autoridad_sin_fuente: presente | ausente                   # "es el estándar"
  jerarquia_y_calibracion:
    muro_de_detalle_sin_jerarquia: presente | ausente          # CTX-4
    despliegue_subestimado: presente | ausente | na            # CTX-3
  para_el_lector:
    resumen_no_se_entiende_solo: presente | ausente            # ACT-06
    diagrama_no_coincide_con_texto: presente | ausente
    rnf_sin_metrica: presente | ausente
  contadores:
    frases_diseñado_para_o_busca: 0 | n                        # objetivo 0 en ESC-2
    rnf_expresados_solo_con_adjetivo: 0 | n                    # objetivo 0
    componentes_en_diagrama_no_mencionados_en_texto: 0 | n     # objetivo 0
```

Los tres contadores del final son la parte más útil del anexo: convierten en un número lo que de otro modo queda en apreciación. Un borrador con cinco «diseñado para» y tres RNF que son solo adjetivos tiene ocho reescrituras pendientes, y el conteo las vuelve visibles antes de que el lector las encuentre.
