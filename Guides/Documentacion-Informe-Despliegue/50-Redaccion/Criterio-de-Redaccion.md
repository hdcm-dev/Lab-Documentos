---
doc_id: TEM-CRITERIO
doc_type: tema
title: Criterio de redacción
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [TEM-ESTRUCTURA, TEM-ERRORES, FAM-NAT, TEM-AUDIENCIA, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, MARCO-CONVENCIONES, ANEXO-REFERENCIAS]
---

# Criterio de redacción — `TEM-CRITERIO`

## Resumen ejecutivo

Un informe puede tener la arquitectura correcta y ser ilegible. La distancia entre un contenido técnico exacto y un documento que un lector entiende no la cubre una plantilla: la cubre el criterio del que escribe, ejercido frase por frase. Este documento forma ese criterio. No enseña qué poner en cada sección —eso lo hace la [estructura del documento](Estructura-del-Documento.md)— sino cómo decidir, al escribir cada una, entre la versión que suena bien y la que sirve.

El criterio se apoya en dos pilares. El primero es una **voz**: técnica, formal, de ritmo variado, que distingue lo que el sistema hace de lo que se pretendía que hiciera, y que no gasta palabras en relleno. La fija `Rule-Narrative-Voice` y esta guía la sostiene en cada documento, incluido este. El segundo es una **disciplina**: escribir para el lector y no para uno mismo. El redactor conoce el sistema tan bien que olvida cuánto contexto le falta a `ACT-03`, y de ese olvido nacen casi todos los informes que su autor entiende y nadie más. [`MARCO-ACTORES`](../00-Marco-de-Referencia/Actores.md#el-error-de-actor-más-común) lo llama el error de actor más común; corregirlo es el núcleo de este documento.

La cuarta sección adopta la variante de `FAM-RED`: en lugar de un fragmento de informe, un conjunto de **frases de referencia** —pares de «así no / así sí»— que muestran el criterio aplicado a la frase. Son el destilado práctico de todo lo anterior.

---

## Definición

### Qué es

El **criterio de redacción** es el juicio que decide, ante cada frase, entre formulaciones que dicen lo mismo pero no valen lo mismo. «El sistema está diseñado para escalar» y «corre en una instancia y escala por su diseño sin estado, aunque no se ejercita en producción» describen la misma realidad; solo la segunda es un hecho verificable, y elegirla es criterio, no gramática. La redacción con criterio no es corrección ortográfica ni pulido de estilo: es la serie de decisiones que determinan si el lector termina sabiendo cómo funciona el sistema o solo con la impresión de que funciona bien.

Ese juicio se aprende preguntándose, en cada sección, lo correcto. No hay una regla que se aplique mecánicamente; hay preguntas que, hechas a tiempo, evitan las formulaciones que fallan.

### El criterio, sección por sección

Cada sección del informe tiene una pregunta que la gobierna. Si el redactor se la hace antes de escribir, la sección sale orientada; si no, sale como le salga.

| Sección del informe | Qué debo preguntarme al redactarla |
|---|---|
| Resumen ejecutivo | ¿Un decisor que solo lee esto sabe qué se le pide aprobar y por qué? ¿Se entiende sin el resto? |
| Contexto y alcance | ¿Qué da por sabido `ACT-03` que en realidad no sabe? ¿Dije qué queda fuera y por qué? |
| Requisitos que gobiernan | ¿Cada requisito viene de `ACT-05` o lo inventé para que la arquitectura calzara? ¿Cada RNF tiene métrica? |
| Estrategia de solución | ¿Estoy explicando por qué esta forma, o solo describiendo cuál? |
| Vista de arquitectura | ¿Estoy contando responsabilidades y relaciones, o el árbol de carpetas del repositorio? |
| Vista de despliegue | ¿El peso corresponde al contexto? ¿Un `ACT-04` podría instalarlo con lo que escribí? |
| Resolución de requisitos | ¿Estoy mostrando el mecanismo que satisface el requisito, o repitiendo el requisito con otras palabras? |
| Decisiones y trade-offs | ¿Registré la alternativa que descarté y su costo, o presento la decisión como inevitable? |
| Riesgos y pendientes | ¿Distingo lo decidido de lo supuesto y lo pendiente, o lo escribí todo en presente de indicativo? |

La pregunta de la vista de arquitectura y la de resolución de requisitos son las que más se olvidan, y las que más informes hunden. La primera separa describir la arquitectura de describir el repositorio; la segunda separa explicar la solución de enunciar el problema dos veces.

### Qué problema resuelve

El del informe técnicamente correcto e inútil. Existe un tipo de documento que no tiene ningún error de contenido —los componentes son esos, la topología es esa, los requisitos están todos— y que sin embargo no le sirve a nadie, porque está escrito para quien ya sabe. El criterio de redacción es lo que convierte ese material correcto en un documento que informa a quien no estaba. No agrega hechos: los ordena, los jerarquiza y los formula para el lector que los va a usar.

### Qué NO es, y con qué se lo confunde

**No es corrección de estilo.** El redactor técnico `ACT-07` cuida la forma —consistencia, legibilidad, que los diagramas digan lo que el texto dice— y su trabajo es necesario, pero el criterio de redacción es anterior y más profundo: decide qué se afirma y con qué grado de certeza. Un informe puede estar impecablemente maquetado y afirmar intenciones como hechos; ese es un problema de criterio, no de estilo.

**No es una plantilla de frases.** No hay un catálogo de oraciones para copiar. Las frases de referencia de este documento son ejemplos del criterio en acción, no fórmulas: enseñan a distinguir hecho de intención, no la frase exacta que hay que escribir.

**No es escribir simple.** Escribir para `ACT-03` no es diluir el contenido técnico hasta que cualquiera lo entienda. `ACT-03` tiene criterio técnico; lo que no tiene es el contexto que el autor da por sabido. El error simétrico —escribir todo el informe al nivel del decisor `ACT-06`— vacía el documento de lo que el solicitante técnico necesita evaluar. El criterio no es bajar el nivel: es estratificarlo.

---

## La voz

`Rule-Narrative-Voice` fija cómo suena un informe escrito con criterio, y esta guía la sigue en cada documento. Cuatro rasgos la definen, y los cuatro se ejercen en las zonas de prosa —resúmenes, racional, narración de flujos—, no en las tablas ni en los diagramas, que se mantienen rígidos a propósito.

**Hechos, no intenciones.** Es el rasgo central en este tema. Un informe describe un sistema, y un sistema hace cosas, no las pretende. «Está diseñado para», «busca garantizar», «tiene como objetivo» son marcas de intención que en un informe de `ESC-2` casi siempre esconden que el comportamiento no se verificó. La formulación honesta afirma lo que se puede confrontar con el sistema en ejecución, y donde no hay certeza, la declara: «se prevé», «no se ejercita en producción», «por confirmar».

**Ritmo variado.** Frases cortas junto a frases largas. Un texto donde todas las oraciones tienen el mismo largo se lee como generado en serie, y un informe que se lee así pierde la autoridad de haber sido pensado por alguien. La variación no es adorno: es la marca de que hubo un autor decidiendo dónde detenerse.

**Sin relleno.** «Cabe destacar», «es importante señalar», «en resumen», «como se puede observar» no aportan información y delatan un texto que se estira para parecer completo. Si un párrafo se puede borrar sin perder nada, sobra. La prosa densa respeta el tiempo del lector, que es escaso y suele estar decidiendo algo.

**Prosa donde va prosa.** Una relación de causa y efecto —por qué el Worker sube en diferido, qué pasa si el enlace se corta— se narra en un párrafo conectado, no se fragmenta en viñetas sueltas. Las listas se reservan para enumeraciones reales. Convertir en viñetas lo que es un razonamiento rompe el hilo que el lector necesita seguir.

### El tiempo verbal como herramienta

La distinción entre hecho e intención tiene un instrumento concreto y mecánico: el tiempo verbal. En un informe de `ESC-2` sobre un sistema que existe, el **presente de indicativo** afirma lo que el sistema hace y se puede confrontar —«el Worker graba en disco local»—. El **condicional y las perífrasis de posibilidad** califican lo que podría hacer y no se probó —«podría escalar horizontalmente»—. Y las **marcas de previsión** —«se prevé», «está planificado»— señalan lo que todavía no ocurre y pertenece a `ESC-1`.

El error de redacción más frecuente en `ESC-2` es usar el presente para lo que no es presente: «el sistema escala» cuando el sistema nunca escaló. La regla operativa es simple y se puede aplicar sin criterio artístico: si una afirmación en presente no se puede verificar señalando el sistema en ejecución, o es falsa o va en condicional. Escribir «escala» exige haber visto escalar; si no se vio, se escribe «puede escalar por su diseño, no ejercitado». El tiempo verbal deja de ser una decisión gramatical y se vuelve una declaración sobre el estado de la evidencia.

---

## Escribir para el lector

El error de actor más común es escribir para uno mismo, y su antídoto es un ejercicio, no una regla: releer cada sección preguntándose qué sabe `ACT-03` que le permita seguirla. El autor construyó el sistema; conoce el nombre interno de cada componente, la sigla de cada módulo, la razón por la que una decisión que parece rara era la única sensata. Nada de eso lo sabe el lector, y el informe que da por sabido ese contexto es un informe que solo su autor entiende.

Los síntomas son reconocibles. Siglas sin expandir en su primer uso. Componentes nombrados por su nombre de repositorio —`AudienceSvc.Worker`— en lugar de por su responsabilidad —«el servicio que graba y sube»—. Decisiones presentadas como obvias porque para el autor lo son. Un diagrama que el autor lee de un vistazo porque lo dibujó, y que el lector no sabe por dónde empezar a mirar.

La corrección no es escribir más simple sino escribir situado. `ACT-03` puede seguir un diagrama de despliegue y una discusión de trade-offs; lo que no puede es adivinar el contexto que el autor no puso. Estratificar el nivel —resumen para el decisor, cuerpo para el solicitante técnico, anexos para el responsable de despliegue— es la forma estructural de servir a cada lector en su sección, y [`TEM-ESTRUCTURA`](Estructura-del-Documento.md) la implementa. El criterio de redacción es la forma frase a frase: en cada oración, saber a quién se le habla.

Un auditor `ACT-08` recorre el informe con una pregunta permanente —«¿cómo sé que esto es así?»— y el redactor con criterio se la anticipa. Cada afirmación que no se puede respaldar señalando el sistema, la fuente o la decisión registrada es una afirmación que el informe pide creer sin dar con qué. Escribir previendo esa pregunta es lo que separa un informe que se sostiene de uno que se afirma.

---

## Aplicación por escenario

### `ESC-1` — Solución en diseño

El criterio dominante es marcar el estado de cada afirmación. En un sistema que no existe, todo es intención, y el informe honesto no la disfraza de hecho: distingue lo decidido de lo propuesto y de lo pendiente de validar, y no le teme a «se prevé». La trampa es el futuro perfecto —describir la solución como si ya funcionara—, y el antídoto es de redacción pura: no usar el presente de indicativo para lo que todavía no ocurre. «El backend escala horizontalmente» es falso si el backend no existe; «se prevé un backend sin estado que permita escalar horizontalmente» es exacto.

### `ESC-2` — Solución construida

El criterio dominante es que cada afirmación sobreviva a la confrontación con el sistema. Es el escenario del pedido que abre la guía y el que más castiga la redacción por intención. La frase «el sistema está diseñado para escalar» no describe el sistema: describe una aspiración del diseño. La frase útil —«el backend corre en una única instancia; el escalado horizontal es posible por su diseño sin estado pero no se ejercita en producción»— es un hecho verificable y, además, más informativa. `MARCO-ESCENARIOS` lo señala como la trampa característica de `ESC-2`, y es una trampa de redacción antes que de contenido: el sistema real está ahí para confrontarse, y el redactor eligió describir el otro.

### `ESC-3` — Solución en evolución o migración

El criterio dominante es no vender el destino sin costear el viaje. La redacción de una migración tiende al entusiasmo —la arquitectura objetivo es evidentemente superior— y ese entusiasmo produce marketing, no informe. El criterio es escribir el costo con el mismo cuidado que el beneficio: qué cuesta migrar, qué riesgo se corre, cómo se ve el sistema durante la convivencia. Y una precaución de tono: quien construyó el sistema actual suele estar entre los lectores, de modo que describir el estado de partida con desprecio es a la vez injusto y contraproducente.

### `ESC-4` — Evaluación de una solución ajena

El criterio se invierte: no se redacta, se lee con criterio, y lo que se escribe es el juicio. La disciplina central es separar lo que el informe ajeno demuestra de lo que solo afirma, y registrar las preguntas que no responde. Cuando toca redactar —el informe de evaluación—, el criterio es no puntuar la forma: un documento extenso y bien maquetado puede describir una solución pobre. El juicio se escribe sobre el fondo, con las evidencias que lo sostienen y la lista explícita de lo que no pudo verificarse.

### Qué cambia según el contexto

| Contexto | Riesgo de redacción dominante | Corrección de criterio |
|---|---|---|
| `CTX-1` monolito | Describir la estructura de carpetas como si fuera la arquitectura | Escribir responsabilidades y relaciones, no el árbol del repositorio |
| `CTX-2` cliente-servidor | Olvidar los contratos entre nodos | Narrar qué protocolo une qué y qué pasa si el enlace cae |
| `CTX-3` borde distribuido | Subestimar el despliegue; enterrar la operación degradada | Dar a la instalación y a la resiliencia el espacio que el lector busca |
| `CTX-4` multiservicio | Describir cada servicio con igual detalle | Jerarquizar: lo central se desarrolla, lo accesorio se referencia |

El sistema de audiencias, por ser `CTX-3`, concentra su exigencia de redacción en la sección de resolución de requisitos: los tres comportamientos que lo definen —operar con el centro caído, recuperar ante la caída del escritorio, subir en diferido— hay que narrarlos de punta a punta, con un caso concreto, porque son la parte que el lector técnico más quiere entender y la que peor se explica con una tabla.

---

## Frases de referencia

Pares de «así no / así sí». Los fragmentos son **sintéticos** y pertenecen al informe del sistema de audiencias. Cada par aísla una decisión de criterio.

### Intención frente a hecho

**Así no:** «El sistema está diseñado para escalar horizontalmente y garantizar alta disponibilidad.»

**Así sí:** «El backend corre en una única instancia. Su diseño sin estado permite escalar horizontalmente, pero esa configuración no se ejercita en producción.»

La primera afirma dos intenciones que nadie puede confrontar; la segunda afirma un hecho y califica una posibilidad. La segunda además es más útil: le dice al lector exactamente qué está y qué no está probado.

### Vago frente a medible

**Así no:** «La grabación se recupera rápidamente ante una caída del programa de escritorio.»

**Así sí:** «Ante la caída del programa de escritorio, el operador retoma la audiencia en curso desde el último estado persistido; la grabación en el Worker no se interrumpe porque corre en un proceso separado.»

«Rápidamente» no se puede verificar ni cumplir. La versión precisa nombra el mecanismo —proceso separado, estado persistido— y con eso el lector entiende por qué la recuperación es posible, no solo que se afirma.

### Carpeta frente a responsabilidad

**Así no:** «La solución se organiza en los proyectos `Audiencias.Core`, `Audiencias.Infrastructure`, `Audiencias.Worker` y `Audiencias.Web`.»

**Así sí:** «El servicio en segundo plano captura de las cámaras, graba localmente y sube los videos al servidor de archivos; el frontend administrativo consulta esas grabaciones. Cada uno es un componente desplegable por separado.»

El árbol de proyectos es la estructura del repositorio, no la arquitectura. El lector `ACT-03` quiere saber de qué responde cada pieza y con qué habla, no cómo se llaman las carpetas.

### Autoridad sin fuente frente a autoridad nombrada

**Así no:** «La comunicación entre terminal y centro usa REST porque es el estándar de la industria.»

**Así sí:** «La comunicación entre terminal y centro usa una API HTTP con recursos por URI y JSON. Es criterio del equipo, no una exigencia normativa; se eligió por su interoperabilidad y por el soporte de .NET.»

«Es el estándar» invoca una autoridad que no existe: REST es un estilo, no una norma. Nombrar el nivel de autoridad —criterio propio, en este caso— es lo que [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md#los-cuatro-niveles-de-autoridad) exige y lo que distingue una afirmación defendible de un argumento de autoridad hueco.

### Decisión inevitable frente a decisión con alternativa

**Así no:** «Se eligió PostgreSQL como base de datos.»

**Así sí:** «Se eligió PostgreSQL frente a SQL Server por su costo de licenciamiento y su soporte maduro en .NET vía Npgsql; el costo es un ecosistema de herramientas menos integrado con el stack de Microsoft.»

Una decisión sin alternativa registrada parece inevitable, y las decisiones inevitables no se pueden auditar. La versión con el trade-off explícito es lo que `N-01` llama una decisión con su *rationale*.

### Relleno frente a densidad

**Así no:** «Es importante señalar que la arquitectura del sistema ha sido cuidadosamente diseñada teniendo en cuenta los requisitos no funcionales más relevantes del dominio.»

**Así sí:** «Tres requisitos no funcionales gobiernan la arquitectura: operar con el centro caído, recuperar ante la caída del escritorio y subir en diferido. Cada uno se resuelve con un mecanismo distinto, descrito abajo.»

La primera frase no dice nada que el lector pueda usar. La segunda nombra los tres requisitos y anuncia dónde se resuelven; ocupa un renglón menos y transmite información en lugar de la impresión de haberla transmitido.

---

## Preguntas guía

- Cada afirmación que escribo, ¿es un hecho que puedo confrontar con el sistema, o una intención disfrazada de presente?
- ¿Qué da por sabido este párrafo que `ACT-03` en realidad no sabe?
- Si un auditor preguntara «¿cómo sé que esto es así?», ¿la respuesta está en el texto?
- ¿Estoy describiendo la arquitectura o el árbol de carpetas del repositorio?
- ¿Esta sección le habla al decisor, al solicitante técnico o al responsable de despliegue? ¿El nivel corresponde?
- ¿Cada RNF que menciono tiene una métrica, o lo dejé en un adjetivo —«rápido», «escalable», «robusto»?
- Si borro este párrafo, ¿se pierde información? Si no, ¿por qué está?

---

## Criterios de calidad

### Redacción buena

Cada afirmación se puede respaldar señalando el sistema, la fuente o la decisión registrada, y el texto deja claro cuál de las tres cosas la sostiene. Los hechos se distinguen de las intenciones sin que el lector tenga que adivinarlo: lo verificado va en presente, lo previsto va marcado. El nivel se estratifica —el resumen lo entiende un decisor, el cuerpo lo evalúa un técnico, el anexo lo usa un responsable de despliegue— y ninguna sección da por sabido el contexto que el lector no tiene. La voz es uniforme, de ritmo variado y sin relleno; se percibe un autor que decidió qué decir, no una plantilla rellenada.

### Redacción pobre y antipatrones

**Intención como hecho.** «Diseñado para», «busca garantizar», «tiene como objetivo» en un informe de `ESC-2`, donde el sistema real está disponible para confrontarse. Es el antipatrón central del tema y [`TEM-ERRORES`](Errores-Frecuentes.md) lo desarrolla.

**Adjetivo sin métrica.** «Rápido», «escalable», «robusto», «seguro» sin un número ni un mecanismo detrás. Un RNF sin métrica no se puede verificar ni cumplir, y decirlo con un adjetivo es no decirlo.

**Escribir para uno mismo.** Siglas sin expandir, nombres de repositorio, decisiones presentadas como obvias. El informe que su autor entiende y `ACT-03` no. `MARCO-ACTORES` lo nombra como el error de actor más común.

**Relleno.** Muletillas y párrafos que se pueden borrar sin pérdida. Estiran el documento y le restan la densidad que respeta el tiempo del lector.

**Autoridad sin fuente.** «Es el estándar», «REST lo exige», «la arquitectura limpia obliga a», sin nombrar de dónde sale la prescripción. Confunde criterio propio o plantilla con norma, el error que la guía combate en todos sus documentos.

---

## Anexo — Lista de verificación de redacción

Se recorre sobre el borrador antes de entregar, sección por sección. Complementa la [lista de verificación](../99-Anexos/Lista-de-Verificacion.md) integral con el foco puesto en la prosa.

```yaml
revision_de_redaccion:
  hechos_vs_intenciones:
    afirmaciones_en_presente_confrontables_con_el_sistema: si | no | parcial
    lo_previsto_esta_marcado_como_previsto: si | no | na    # se prevé, por confirmar
    frases_diseñado_para_o_busca_garantizar: 0 | n         # objetivo: 0 en ESC-2
  para_el_lector:
    siglas_expandidas_en_primer_uso: si | no
    componentes_por_responsabilidad_no_por_carpeta: si | no
    contexto_que_ACT03_no_tiene_esta_dado: si | no
    nivel_estratificado_resumen_cuerpo_anexos: si | no
  autoridad:
    cada_prescripcion_declara_su_nivel: si | no             # normativo/marco/facto/propio
    ninguna_afirmacion_dice_es_el_estandar_sin_fuente: si | no
  metrica:
    cada_RNF_tiene_metrica_o_mecanismo: si | no             # no solo un adjetivo
  voz:
    longitud_de_frase_varia: si | no
    sin_muletillas_de_relleno: si | no
    razonamientos_en_prosa_no_en_viñetas_sueltas: si | no
    ningun_parrafo_borrable_sin_perdida: si | no
```

El campo `frases_diseñado_para_o_busca_garantizar` es el más revelador: contar cuántas veces el borrador dice «diseñado para» o «busca garantizar» en un informe de `ESC-2` mide directamente cuánta intención se coló como hecho. El objetivo es cero, y cada aparición es una frase que hay que reescribir señalando lo que el sistema efectivamente hace.
