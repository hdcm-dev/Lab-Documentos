# Marco Teórico de UX/UI/DX

**Documento:** UX-UI-Guide.md
**Versión:** 1.0
**Estado:** Aprobado
**Fecha:** 2026-04-25
**Autor:** Equipo de documentación técnica
**Audiencia:** profesionales y estudiantes universitarios avanzados (UTN)
**Idioma:** español
**Norma bibliográfica:** APA 7

---

## Índice

1. [Introducción y encuadre](#1-introducción-y-encuadre)
2. [Fundamentos conceptuales](#2-fundamentos-conceptuales)
3. [Modelos de proceso de diseño](#3-modelos-de-proceso-de-diseño)
4. [Principios, heurísticas y leyes](#4-principios-heurísticas-y-leyes)
5. [Estándares y normativa](#5-estándares-y-normativa)
6. [Sistemas de diseño](#6-sistemas-de-diseño)
7. [Investigación, evaluación y métricas](#7-investigación-evaluación-y-métricas)
8. [UX en plataformas web](#8-ux-en-plataformas-web)
9. [UX en plataformas móviles](#9-ux-en-plataformas-móviles)
10. [Developer Experience en REST APIs](#10-developer-experience-en-rest-apis)
11. [Caso aplicado: motor DSL multi-target](#11-caso-aplicado-motor-dsl-multi-target)
12. [Síntesis y checklist de aplicación](#12-síntesis-y-checklist-de-aplicación)
- [Apéndice A — Glosario](#apéndice-a--glosario)
- [Apéndice B — Bibliografía](#apéndice-b--bibliografía)
- [Apéndice C — Índice de cuadros](#apéndice-c--índice-de-cuadros)

---

# 1. Introducción y encuadre

## 1.1 Propósito

Este marco teórico consolida las nociones, modelos, normas y métricas que sustentan la práctica profesional de UX/UI y de Developer Experience (DX). Se redacta con un doble objetivo: ofrecer a estudiantes universitarios avanzados de UTN un mapa conceptual riguroso y, simultáneamente, servir de guía operativa a profesionales que diseñan, documentan o auditan productos digitales.

El documento no pretende ser exhaustivo en cada autor, sino articular un cuerpo coherente que permita decidir con criterio: cuándo aplicar Lean UX y cuándo Design Sprint, cuándo basta WCAG A y cuándo se exige AAA, cuándo conviene versionar una API por URL y cuándo por header. Se evita la mera enumeración: cada concepto se acompaña de un ejemplo, y cada bloque de un cuadro comparativo y un caso real.

## 1.2 Alcance: UX/UI/DX como continuo

Este marco trata UX, UI y DX como variantes de una misma disciplina (el diseño de interacción humano-sistema), no como áreas estancas. UI es la capa sensorial y operativa de un sistema; UX es la experiencia integral resultante; DX es UX cuando el "humano" es un desarrollador y la "interfaz" es código, contratos, mensajes de error y documentación. Las heurísticas, las leyes psicológicas y los estándares aplican a las tres con sus instanciaciones particulares.

## 1.3 Cómo leer este documento

El orden de capítulos respeta dependencia conceptual estricta: ningún término se usa antes de definirse. Se sugieren tres rutas de lectura:

- **Ruta diseñador**: §2 → §3 → §4 → §5 → §6 → §7 → §8 → §9.
- **Ruta desarrollador / integrador de APIs**: §2 → §4 (heurísticas) → §5 (WCAG, ARIA si toca front) → §10 → §11.
- **Ruta auditor académico**: §1 → §2 → §5 → §7 → §12 + apéndices.

El documento puede usarse como obra de consulta puntual mediante el índice y los apéndices.

## 1.4 Preguntas guía de redacción

Las siguientes preguntas funcionan como *advance organizer* (Ausubel, 1960): conviene formularlas explícitamente al iniciar cualquier documentación o proyecto UX/UI/DX. La sección 12.2 retoma este conjunto como checklist verificativo.

1. **¿Quién es la persona objetivo?** ¿Cliente final, integrador, operador interno? ¿Hay más de una persona? ¿Cuáles son sus capacidades, limitaciones, contextos de uso?
2. **¿Qué nivel de fidelidad requiere este documento?** ¿Wireframe lo-fi, hi-fi, especificación funcional, contrato técnico? La fidelidad influye en lo que se documenta y lo que se omite deliberadamente.
3. **¿Qué estándares de calidad y normativa aplican?** ¿WCAG A/AA/AAA, ISO 9241-210, leyes locales de accesibilidad, RFCs HTTP, semver?
4. **¿Es UX de usuario final o DX de desarrollador?** Las heurísticas se reformulan según el caso (ver §10.1).
5. **¿Hay restricciones de hardware, conectividad o accesibilidad?** ¿Pantalla pequeña, conexión variable, restricciones de input táctil, encoding limitado, latencia, asistivos?
6. **¿Qué criterios de aceptación verificables tiene este artefacto?** Fidelidad declarada, métricas medibles, tolerancias formales (ver §11 para un ejemplo concreto).
7. **¿Cuál es la persona objetivo de la propia documentación?** Distinción crítica entre tutorial, guía, referencia y explicación (Diátaxis, §10.7).
8. **¿Qué decisiones quedan congeladas y cuáles abiertas?** Las decisiones abiertas se documentan como tales para evitar interpretaciones implícitas en código.
9. **¿Cómo evolucionará esta documentación?** ¿Versionado, deprecation policy, historial de cambios?
10. **¿Qué evidencia (research, datos, normativa, citas) sostiene cada afirmación?** Sin evidencia, una recomendación es opinión.

---

# 2. Fundamentos conceptuales

Esta sección define el vocabulario nuclear. Las definiciones siguen, cuando es posible, fuentes canónicas (autores fundadores o normas ISO). Cada definición lleva un *diferenciador*: qué la distingue de un término vecino con el que tiende a confundirse.

## 2.1 HCI — Human-Computer Interaction

**Definición.** "Disciplina que estudia el diseño, la evaluación y la implementación de sistemas computacionales interactivos para uso humano y los fenómenos que los rodean" (Hewett et al., 1992, *ACM SIGCHI Curricula for Human-Computer Interaction*). Su libro fundacional es *The Psychology of Human-Computer Interaction* (Card, Moran & Newell, 1983).

**Diferenciador.** HCI es el campo académico; UX es su aplicación profesional/industrial. Un investigador de HCI publica papers en CHI; un practitioner de UX entrega artefactos de proyecto.

**Ejemplo.** Un estudio de eye-tracking sobre patrones de lectura en formularios web (HCI académico) cuyas conclusiones se aplican a rediseñar el checkout de un e-commerce (práctica UX).

## 2.2 Usabilidad

**Definición.** "Grado en que un sistema, producto o servicio puede ser usado por usuarios específicos para alcanzar objetivos específicos con eficacia, eficiencia y satisfacción en un contexto de uso especificado" (ISO 9241-11:2018). Operacionalización clásica: cinco atributos de Nielsen (1993, *Usability Engineering*) — aprendibilidad, eficiencia, memorabilidad, errores, satisfacción.

**Diferenciador.** La usabilidad es una *cualidad medible* de un sistema; no es sinónimo de UX. Un sistema puede ser altamente usable y producir mala UX (porque genera ansiedad, ofrece un producto deficiente o explota dark patterns).

**Ejemplo.** Un sitio de aerolínea que permite comprar pasajes en 3 clics (alta usabilidad), pero cuya tarifa final triplica la inicial por cargos ocultos (UX negativa).

## 2.3 UX — User Experience

**Definición.** "Percepciones y respuestas de una persona que resultan del uso o uso anticipado de un sistema, producto o servicio" (ISO 9241-210:2019). El término fue acuñado por Don Norman en Apple (ca. 1993) y desarrollado en *The Design of Everyday Things* (Norman, 1988/2013).

**Diferenciador.** UX abarca pre-uso (expectativas, marketing), uso (interacción) y post-uso (recuerdo, recomendación). No se limita a la pantalla.

**Ejemplo.** La UX de comprar un iPhone incluye la página web, el unboxing, el tutorial inicial y el soporte tres meses después. La UI es solo una parte.

## 2.4 UI — User Interface

**Definición.** "El medio por el cual el usuario y un sistema computacional interactúan, en particular mediante dispositivos de entrada y software" (Galitz, 2007, *The Essential Guide to User Interface Design*).

**Diferenciador.** La UI es la capa sensorial-operativa concreta (botones, formularios, gestos); la UX es el resultado integral percibido. Una buena UI no garantiza buena UX, y viceversa.

**Ejemplo.** Una calculadora con UI minimalista pero con tipografía ilegible para mayores tiene buena geometría visual y mala UX.

## 2.5 IxD — Interaction Design

**Definición.** "Diseño del comportamiento de productos y sistemas interactivos con los que un usuario interactúa" (Cooper, Reimann & Cronin, 2014, *About Face*, 4ª ed.). Disciplina formalizada por Bill Moggridge y Bill Verplank (ca. 1984).

**Diferenciador.** IxD se ocupa del *comportamiento dinámico* (estados, transiciones, feedback temporal), no de la composición visual estática. UI sin IxD es una pantalla muerta.

**Ejemplo.** El comportamiento de "deshacer envío" en Gmail durante 30 segundos tras el clic — es una decisión de IxD, no de UI.

## 2.6 Arquitectura de Información

**Definición.** "El arte y la ciencia de organizar y rotular sitios web, intranets, comunidades en línea y software para favorecer la usabilidad y *findability*" (Rosenfeld, Morville & Arango, 2015, *Information Architecture for the Web and Beyond*, 4ª ed.).

**Diferenciador.** AI se ocupa de estructuras (taxonomías, navegación, etiquetado), no de píxeles. Es la "esqueleto-de-conceptos" antes que el "esqueleto-de-pantalla".

**Ejemplo.** Decidir si un sitio universitario organiza su contenido por "Carreras → Materias" o por "Año de cursada → Materias" es una decisión de AI con consecuencias drásticas para la usabilidad.

## 2.7 Accesibilidad

**Definición.** "Usabilidad de un producto, servicio, entorno o instalación por personas con el rango más amplio de capacidades" (ISO 9241-11:2018, def. 3.1.3). En la web, normalizada por WCAG 2.2 (W3C, 2023).

**Diferenciador.** Accesibilidad no es sinónimo de usabilidad. La accesibilidad se centra explícitamente en personas con discapacidades (visual, auditiva, motriz, cognitiva) y en el principio de diseño universal.

**Ejemplo.** Un botón que solo cambia de color al recibir foco es usable para personas videntes, pero inaccesible para usuarios de lector de pantalla. Agregar un `aria-pressed="true"` y un cambio visible de borde lo hace accesible.

## 2.8 Service Design

**Definición.** "Actividad de planificar y organizar personas, infraestructura, comunicación y componentes materiales de un servicio para mejorar su calidad y la interacción entre proveedor y cliente" (Stickdorn, Hormess, Lawrence & Schneider, 2018, *This Is Service Design Doing*).

**Diferenciador.** Service Design integra *frontstage* (UX visible al cliente) y *backstage* (procesos internos, sistemas, personas no visibles) mediante service blueprints. Zoom-out de la UX puntual a la experiencia total del servicio.

**Ejemplo.** La UX de un cajero automático (frontstage: pantalla, teclado, expulsión de billetes) se sostiene en procesos backstage (carga del cajero, conexión con central, mantenimiento) que no son visibles al usuario pero condicionan la experiencia.

## 2.9 DX — Developer Experience

**Definición.** "La experiencia que tienen los desarrolladores al usar herramientas, APIs, SDKs, documentación y procesos para construir software" (Fagerholm & Münch, 2012, *Developer Experience: Concept and Definition*, ICSSP).

**Diferenciador.** DX es UX especializada cuando el usuario es desarrollador. Las "interfaces" críticas dejan de ser pantallas y pasan a ser APIs, mensajes de error, tiempos de build, claridad de la documentación y predictibilidad del SDK.

**Ejemplo.** La frase fundacional de Heroku — "git push heroku master" — es una decisión de DX: comprime *deploy a producción* en un comando que el desarrollador ya conoce.

## 2.10 Cuadro 1 — Las nueve disciplinas comparadas

| Disciplina | Foco | Output típico | Audiencia | Norma de referencia |
|---|---|---|---|---|
| **HCI** | Conocimiento académico de la interacción | Papers, métodos | Investigadores | ACM SIGCHI |
| **Usabilidad** | Cualidad medible del sistema | Tests, reportes, métricas | Diseñadores, QA | ISO 9241-11:2018 |
| **UX** | Experiencia percibida integral | Journeys, prototipos, métricas SUS/NPS | Usuarios finales | ISO 9241-210:2019 |
| **UI** | Capa sensorial-operativa | Mockups, hi-fi, design system | Usuarios finales | DS de plataforma (Material/HIG) |
| **IxD** | Comportamiento dinámico | Specs de estados, microinteracciones | Usuarios finales | — |
| **AI (IA)** | Estructura conceptual | Sitemap, taxonomía, card-sort | Usuarios finales | — |
| **Accesibilidad** | Usabilidad universal | Audits, ARIA, alt text | Usuarios con discapacidad | WCAG 2.2 |
| **Service Design** | Servicio extremo-a-extremo | Service blueprint, journey amplio | Cliente + organización | ISO 20282 (parcial) |
| **DX** | Experiencia del desarrollador | API, docs, SDK, mensajes de error | Desarrolladores | RFCs HTTP, OpenAPI |

---

# 3. Modelos de proceso de diseño

Después de definir el vocabulario (§2), corresponde describir cómo se aplica en la práctica. Esta sección presenta cinco modelos de proceso. No son recetas excluyentes: en proyectos reales se hibridan (Lean UX dentro de un Doble Diamante, Design Sprint para una fase específica de Design Thinking). El **Cuadro 2** los compara y la §3.7 ofrece criterios de selección.

## 3.1 Garrett — Los 5 planos de la UX

Jesse James Garrett, en *The Elements of User Experience* (2002, 2ª ed. 2011), articula la UX como cinco planos que van de lo abstracto a lo concreto. Estos planos *no son fases secuenciales*: idealmente se desarrollan en paralelo, pero las decisiones de un plano constriñen las del siguiente.

| Plano | Pregunta que responde | Output | Ejemplo (checkout de tickets) |
|---|---|---|---|
| **Strategy** | ¿Por qué existe el producto? | Necesidades del usuario + objetivos de negocio | Reducir abandono del 68% al 40%; usuario quiere comprar antes de sold-out |
| **Scope** | ¿Qué hace el producto? | Requerimientos funcionales + de contenido | One-tap pay, hold de 8 min, guardado de tarjeta |
| **Structure** | ¿Cómo se organiza? | Diseño de interacción + AI | Flujo de 3 pasos: selección → datos → pago |
| **Skeleton** | ¿Cómo se presenta cada pantalla? | Wireframes, navegación, diseño de información | Precio total siempre visible, CTA fijo |
| **Surface** | ¿Cómo se ve? | Diseño visual sensorial | Tipografía, paleta, microinteracciones del countdown |

**Fortaleza.** Vocabulario común para equipos heterogéneos. **Limitación.** Aunque Garrett aclara que las capas se desarrollan en paralelo, su ilustración apilada induce a leerlo como cascada. **Cuándo usarlo.** Documentar y planificar productos digitales orientados a contenido o tareas.

## 3.2 Doble Diamante

Propuesto por el Design Council del Reino Unido (2005) y refinado en 2019 como *Framework for Innovation*. Articula el proceso en cuatro fases con alternancia entre pensamiento divergente y convergente.

```text
   Discover  ──>  Define  ──>  Develop  ──>  Deliver
   (divergir)    (converger)   (divergir)    (converger)
   ◇ Exploración  ◆ Síntesis    ◇ Generación   ◆ Lanzamiento
```

- **Discover.** Investigación etnográfica, entrevistas, observación. Pregunta: ¿qué problema importa?
- **Define.** Síntesis en un *problem statement*. Pregunta: ¿cuál de todos los problemas atacamos?
- **Develop.** Ideación, prototipado, divergencia de soluciones. Pregunta: ¿qué soluciones probamos?
- **Deliver.** Convergencia, testeo, iteración, lanzamiento. Pregunta: ¿qué solución funciona?

**Principio nuclear.** "Diseñar lo correcto" (primer diamante) antes de "diseñarlo correctamente" (segundo diamante).

**Ejemplo aplicado.** Para el checkout: *Discover* — entrevistar 12 compradores, analizar drop-off por paso. *Define* — formular "How might we ayudar al fan a comprar antes que se agote sin perder confianza". *Develop* — divergir 6 conceptos (express, wizard, single-page). *Deliver* — implementar single-page con autocompletado.

**Fortaleza.** Pedagógico, comunica a stakeholders no-diseñadores. **Limitación.** Induce a leerlo como cascada cuando en realidad las fases se solapan e iteran.

## 3.3 Design Thinking

Tim Brown popularizó el término desde IDEO (Brown, 2009, *Change by Design*). La Stanford d.school lo formaliza en cinco modos no estrictamente secuenciales (Stanford d.school, 2010, *An Introduction to Design Thinking Process Guide*).

1. **Empathize.** Observación, entrevistas, *shadowing*.
2. **Define.** *POV statements* ("Lucía, fan ansiosa, necesita confirmar su compra en menos de 90s porque teme perder el lugar"); preguntas *How Might We*.
3. **Ideate.** Brainstorming, *crazy 8s*, *worst possible idea*.
4. **Prototype.** Baja fidelidad rápida, mejor a mano.
5. **Test.** Feedback con usuarios reales.

**Fortaleza.** Democratiza la innovación más allá del equipo de diseño. **Limitación.** Críticos como Iskander (HBR, 2018) y Vinsel (2018) lo acusan de "innovation theatre" cuando se aplica sin rigor metodológico — talleres con post-its sin investigación previa ni testeo posterior. **Cuándo usarlo.** Problemas ambiguos centrados en humanos, fase de exploración inicial de producto.

## 3.4 Lean UX

Jeff Gothelf y Josh Seiden (2013/2021, *Lean UX*, 3ª ed., O'Reilly) integran el ciclo Build-Measure-Learn de Lean Startup (Ries, 2011) con prácticas UX.

**Núcleo.**
- **Hipótesis.** Plantilla canónica: "Creemos que [resultado] se logrará si [usuario] alcanza [beneficio] con [feature]".
- **MVP.** Experimento mínimo para validar o refutar la hipótesis. No necesariamente un producto.
- **Outcomes over outputs.** Medir cambios de comportamiento del usuario, no entregables del equipo.

**Ejemplo aplicado.** "Creemos que **eliminar el campo CVV en compras recurrentes** para usuarios con tarjeta guardada **aumentará la conversión del checkout en 8%**. Sabremos que estamos en lo correcto cuando veamos **conversion rate ≥ +5% en A/B test sobre 10.000 sesiones durante 14 días**".

**Fortaleza.** Integra UX con Agile/Scrum sin ceremonia adicional. **Limitación.** Riesgo de superficialidad si no se hace research; el "MVP" se confunde con "primera versión apurada". **Cuándo usarlo.** Contextos de incertidumbre alta y ciclos cortos de release.

## 3.5 Design Sprint

Jake Knapp, John Zeratsky y Braden Kowitz (2016, *Sprint*, Google Ventures) proponen una agenda canónica de cinco días para validar una idea de alto riesgo.

| Día | Foco | Actividades |
|---|---|---|
| **Lunes — Map** | Entender el problema | Mapa del usuario, alineación de stakeholders, target a abordar |
| **Martes — Sketch** | Ideación individual | Lightning demos, *crazy 8s*, soluciones detalladas |
| **Miércoles — Decide** | Convergencia | Heatmap voting, *storyboard* de 12 frames |
| **Jueves — Prototype** | Construir prototipo de fachada | Figma clickable, no producción real |
| **Viernes — Test** | Validar | 5 entrevistas con usuarios reales |

**Aplicación al checkout.** *Lun*: mapear journey y elegir target ("ingreso de pago"). *Mar*: lightning demos de Eventbrite, Ticketmaster, DICE (app europea de tickets musicales). *Mié*: voting; storyboard del flujo express. *Jue*: Figma clickable. *Vie*: 5 entrevistas; 4 de 5 completaron en menos de 60s, 1 falló por validación de DNI.

**Fortaleza.** Forzar una decisión validada en una semana; alinea stakeholders por presencia obligada. **Limitación.** Solo testea deseabilidad, no escala técnica ni viabilidad de negocio; requiere stakeholder con poder de decisión presente. **Cuándo usarlo.** *Kickoff* de feature crítica, riesgo alto, ventana de tiempo cerrada.

## 3.6 Cuadro 2 — Modelos de proceso comparados

| Modelo | Duración típica | Output principal | Fase fuerte | Cuándo conviene |
|---|---|---|---|---|
| **Garrett** | Continuo (no es proceso, es taxonomía) | Vocabulario común para el equipo | Documentar coherencia entre planos | Productos digitales con AI compleja |
| **Doble Diamante** | Semanas a meses | Solución validada con usuarios | Discover + Define (encontrar el problema correcto) | Problemas mal definidos al inicio |
| **Design Thinking** | Talleres + iteración (semanas) | Concepto innovador | Empathize | Búsqueda de oportunidades, exploración |
| **Lean UX** | Sprints de 1-2 semanas | Hipótesis validadas | MVP + medición | Contextos Agile, incertidumbre, métricas claras |
| **Design Sprint** | 5 días fijos | Prototipo testeado | Decide + Prototype + Test | Decisión de alto riesgo, ventana cerrada |

## 3.7 Criterios de selección

- **Si el problema está mal definido y hay tiempo:** Doble Diamante o Design Thinking.
- **Si hay urgencia y stakeholders dispersos:** Design Sprint.
- **Si el producto está en producción y hay métricas:** Lean UX.
- **Si se necesita un mapa para coordinar especialistas:** Garrett como columna vertebral, hibridado con cualquiera de los anteriores.

En la práctica industrial argentina, la combinación más frecuente en startups y empresas medianas es Garrett (como vocabulario) + Lean UX (como ritmo iterativo) con Sprints insertados ante decisiones de alto riesgo.

---

# 4. Principios, heurísticas y leyes

Las heurísticas son reglas prácticas derivadas de la experiencia profesional acumulada. Las leyes son hallazgos empíricos de psicología cognitiva con validez estadística distinta. Ambas se aplican durante "Structure" y "Skeleton" (Garrett) y especialmente en la fase "Test" del Doble Diamante.

## 4.1 Las 10 heurísticas de Nielsen

Jakob Nielsen formuló sus heurísticas en 1990 y las publicó en 1994 (Nielsen, 1994, *Heuristic Evaluation*); el Nielsen Norman Group las revisó en 2020 manteniendo su esencia. Son la herramienta de inspección heurística más citada y enseñada del mundo.

1. **Visibilidad del estado del sistema.** El sistema debe informar al usuario qué está sucediendo. *Ejemplo.* Barra de progreso al subir un archivo.
2. **Correspondencia entre el sistema y el mundo real.** Usar lenguaje, conceptos y convenciones familiares al usuario. *Ejemplo.* Ícono de papelera para eliminar.
3. **Control y libertad del usuario.** "Salidas de emergencia" sin penalización. *Ejemplo.* "Deshacer envío" en Gmail.
4. **Consistencia y estándares.** No hacer que el usuario adivine si distintas palabras o acciones significan lo mismo. *Ejemplo.* El logo en la esquina superior izquierda lleva siempre al home.
5. **Prevención de errores.** Mejor que un buen mensaje de error es prevenir que ocurra. *Ejemplo.* Confirmar antes de borrar definitivamente; deshabilitar el botón "Pagar" hasta que el formulario sea válido.
6. **Reconocer antes que recordar.** Minimizar la carga de memoria. *Ejemplo.* Autocompletado en búsqueda.
7. **Flexibilidad y eficiencia de uso.** Aceleradores invisibles para usuarios expertos. *Ejemplo.* Atajos de teclado, command palettes.
8. **Diseño estético y minimalista.** Cada elemento extra compite por la atención. *Ejemplo.* Ocultar opciones avanzadas detrás de un toggle.
9. **Ayudar a reconocer, diagnosticar y recuperarse de errores.** Mensajes en lenguaje plano, indicar el problema y sugerir solución. *Ejemplo.* "La contraseña debe tener al menos 8 caracteres" en lugar de "Error 403".
10. **Ayuda y documentación.** Aunque el ideal sea no necesitarla, la ayuda debe existir, ser buscable y orientada a la tarea. *Ejemplo.* Tooltips contextuales, FAQ buscable.

### Aplicación al checkout — antes/después

| Heurística | Antes | Después |
|---|---|---|
| Visibilidad del estado | Spinner anónimo al pagar | Stepper "1 Datos > **2 Pago** > 3 Confirmación" + microcopy "Validando con tu banco… (~5s)" |
| Prevención de errores | Acepta cualquier número | Detecta marca al tipear, valida Luhn en cliente, formatea espacios cada 4 dígitos, deshabilita "Pagar" hasta validez |
| Recuperación de errores | "Error 402" | "Tu banco rechazó la tarjeta por fondos insuficientes. Probá con otra tarjeta o transferencia →" |

## 4.2 Las 8 reglas de oro de Shneiderman

Ben Shneiderman (1986/2017, *Designing the User Interface*, 6ª ed., con Plaisant et al.) propuso ocho reglas paralelas a Nielsen pero anteriores en formulación.

1. Buscar la consistencia.
2. Permitir atajos a usuarios frecuentes.
3. Ofrecer feedback informativo.
4. Diseñar diálogos que produzcan cierre (inicio, medio, fin).
5. Prevenir errores y ofrecer manejo simple.
6. Permitir reversión de acciones (*undo*).
7. Soportar locus de control interno (usuario como iniciador).
8. Reducir la carga de memoria a corto plazo.

## 4.3 Leyes psicológicas y de interacción

A diferencia de las heurísticas (normativas, profesionales), las leyes son hallazgos empíricos.

### 4.3.1 Ley de Fitts

**Enunciado.** El tiempo para alcanzar un *target* es función logarítmica de la distancia dividida por el ancho del *target* (Fitts, 1954, *Journal of Experimental Psychology*):

```text
T = a + b · log₂(D/W + 1)
```

**Implicación.** *Targets* táctiles ≥ 44 × 44 pt (Apple HIG) o 48 × 48 dp (Material Design); *targets* críticos cerca de la zona natural del cursor o pulgar.

**Ejemplo aplicado.** En el checkout, el botón "Pagar" pasa de 88×28 px alineado a la derecha (requiere scroll y precisión) a *full-width*, 56 px de alto, *sticky* en el bottom — área tappeable de 360×56 px en mobile.

### 4.3.2 Ley de Hick-Hyman

**Enunciado.** El tiempo de decisión crece logarítmicamente con el número de opciones equiprobables (Hick, 1952; Hyman, 1953):

```text
RT = a + b · log₂(n + 1)
```

**Implicación.** *Progressive disclosure*; agrupar y jerarquizar opciones.

**Ejemplo aplicado.** Selector de método de pago: pasa de 11 opciones planas (Visa, Master, Amex, Mercado Pago, Modo, transferencia, crypto, Apple Pay, Google Pay, PayPal, efectivo) a **3 grupos** ("Tarjeta", "Billetera digital", "Otros") + sugerencia del último método usado. La decisión cae de log₂(11) ≈ 3,5 a log₂(3) ≈ 1,6 niveles.

### 4.3.3 Miller's Law / 7±2

**Enunciado.** La memoria de trabajo retiene 7±2 *chunks* (Miller, 1956, *Psychological Review*).

**Controversia.** Cowan (2001) revisó el límite a 4±1 con metodologías más rigurosas. Miller hablaba de *capacidad inmediata*, no de límite UI. Usar el "7" como dogma de cantidad de ítems en menús es una sobre-aplicación.

**Implicación.** *Chunkear* (agrupar) información — número telefónico en bloques (`+54 9 11 1234-5678` en lugar de `5491112345678`).

### 4.3.4 Ley de Tesler / Conservación de la complejidad

**Enunciado.** Toda aplicación tiene una complejidad inherente irreducible; la pregunta es quién la asume, el sistema o el usuario (Larry Tesler, Xerox PARC, ca. 1984).

**Implicación.** El diseñador debe absorber la complejidad por defecto.

**Ejemplo.** Autodetección de país por IP en formulario de pago: la complejidad no desaparece, pero la asume el sistema (no el usuario que tipea su país).

### 4.3.5 Ley de Jakob

**Enunciado.** "Los usuarios pasan la mayor parte del tiempo en *otros* sitios, por lo tanto prefieren que el tuyo funcione como esos" (Nielsen Norman Group, 2000).

**Implicación.** La innovación visual gratuita penaliza usabilidad; respetar convenciones de plataforma.

**Ejemplo.** Carrito de compras en la esquina superior derecha; logo en la superior izquierda como link al home.

### 4.3.6 Postel's Law (Principio de robustez)

**Enunciado.** "Sé conservador en lo que envías, liberal en lo que aceptás" (Postel, 1980, RFC 761).

**Implicación.** Tolerancia de input; normalización del lado servidor; reduce fricción y errores.

**Ejemplo.** Aceptar `+54 9 11 1234-5678`, `541112345678` y `11-1234-5678` en un mismo input de teléfono y normalizar internamente.

> **Bisagra hacia DX (§10).** Postel se formuló originalmente para protocolos de red, no UX. Su migración al diseño de interfaz humana es un caso típico de cómo principios de DX y UX comparten sustrato.

## 4.4 Cuadro 3 — Nielsen vs Shneiderman

| Tema | Nielsen (10) | Shneiderman (8) | ¿Solapan? |
|---|---|---|---|
| Consistencia | ✓ #4 | ✓ #1 | Sí |
| Aceleradores | ✓ #7 | ✓ #2 | Sí |
| Feedback | ✓ #1 | ✓ #3 | Sí |
| Cierre / fin de tarea | — | ✓ #4 | Solo Shneiderman |
| Prevención de errores | ✓ #5 | ✓ #5 | Sí |
| Undo / reversión | ✓ #3 | ✓ #6 | Sí |
| Locus de control | — | ✓ #7 | Solo Shneiderman |
| Memoria | ✓ #6 | ✓ #8 | Sí |
| Match con mundo real | ✓ #2 | — | Solo Nielsen |
| Estética minimalista | ✓ #8 | — | Solo Nielsen |
| Recuperación de errores | ✓ #9 | — | Solo Nielsen |
| Ayuda y documentación | ✓ #10 | — | Solo Nielsen |

**Lectura.** Nielsen y Shneiderman comparten ~6 principios. Nielsen es más rico en aspectos perceptuales (estética, mundo real); Shneiderman enfatiza el ciclo cognitivo de la tarea (cierre, locus de control). En la práctica se complementan.

---

# 5. Estándares y normativa

Las heurísticas son recomendaciones; los estándares son normas con validez legal o industrial. Esta sección presenta las normas vigentes que estructuran la práctica profesional. Se incluyen año y versión: las definiciones cambian.

## 5.1 ISO 9241-210:2019 — Diseño centrado en humanos

Reemplaza ISO 13407:1999. Prescribe seis principios para sistemas interactivos:

1. El diseño se basa en comprensión explícita de usuarios, tareas y entornos.
2. Los usuarios participan a lo largo de todo el diseño y desarrollo.
3. El diseño se conduce y refina por evaluación centrada en el usuario.
4. El proceso es iterativo.
5. El diseño aborda toda la experiencia de usuario.
6. El equipo de diseño incluye habilidades y perspectivas multidisciplinarias.

Etapas iterativas: planificar → entender contexto de uso → especificar requisitos → producir soluciones de diseño → evaluar contra requisitos → (volver al inicio si no se cumplen).

**Implicación práctica.** Si una organización declara cumplimiento de ISO 9241-210, debe poder demostrar que sus usuarios participaron iterativamente — no solo que los "consultaron" al inicio.

## 5.2 ISO 9241-11:2018 — Usabilidad operacional

Define usabilidad mediante tres dimensiones medibles:

- **Eficacia (effectiveness).** Precisión y completitud con que los usuarios alcanzan objetivos especificados. *Métrica típica:* task success rate.
- **Eficiencia (efficiency).** Recursos consumidos (tiempo, esfuerzo cognitivo, costos) en relación con la eficacia. *Métrica típica:* time on task.
- **Satisfacción (satisfaction).** Respuestas físicas, cognitivas y emocionales del usuario. *Métrica típica:* SUS, CSAT.

Cambio respecto a ISO 9241-11:1998: la edición 2018 incluye explícitamente accesibilidad y experiencia de usuario como conceptos relacionados, no como categorías separadas.

## 5.3 WCAG 2.2 — Web Content Accessibility Guidelines

Recomendación oficial del W3C publicada el 5 de octubre de 2023 (W3C, 2023). Constituye el estándar de facto en accesibilidad web y es referencia legal en múltiples jurisdicciones (EU EN 301 549, Section 508 de EE. UU., Ley 26.653 de Argentina por referencia normativa).

### 5.3.1 Los 4 principios POUR

| Principio | Definición | Ejemplo de criterio |
|---|---|---|
| **Perceivable** | La información debe poder percibirse | 1.1.1 Texto alternativo en imágenes |
| **Operable** | Los componentes deben poder operarse | 2.1.1 Toda funcionalidad accesible por teclado |
| **Understandable** | Información y operación comprensibles | 3.3.1 Errores identificados |
| **Robust** | Compatible con tecnologías asistivas actuales y futuras | 4.1.2 Name, Role, Value |

### 5.3.2 Niveles de conformidad

- **Nivel A.** Mínimo legal en muchas jurisdicciones; remoción de barreras críticas.
- **Nivel AA.** Estándar de facto. Es lo que exige Section 508 (EE. UU.), EN 301 549 (UE), Ley 26.653 (Argentina). En el caso argentino, la Resolución 1106/2017 de la Agencia de Acceso a la Información Pública (AAIP) tomó WCAG como referencia normativa para los sitios web del Estado.
- **Nivel AAA.** Máximo. No exigible para todo el contenido (W3C aclara explícitamente que ciertos criterios AAA no se pueden cumplir en todo tipo de contenido).

### 5.3.3 Cambios respecto a 2.1 (2018)

WCAG 2.2 agrega 9 criterios nuevos y elimina uno (4.1.1 *Parsing*, considerado obsoleto por el avance de los parsers HTML5):

- **2.4.11 Focus Not Obscured (Minimum)** — AA. El elemento con foco no debe quedar totalmente oculto.
- **2.4.12 Focus Not Obscured (Enhanced)** — AAA.
- **2.4.13 Focus Appearance** — AAA. Especifica grosor y contraste mínimo del indicador de foco.
- **2.5.7 Dragging Movements** — AA. Toda funcionalidad que requiera arrastre debe tener alternativa con un solo puntero (tap simple).
- **2.5.8 Target Size (Minimum)** — AA. *Targets* ≥ 24 × 24 CSS px.
- **3.2.6 Consistent Help** — A. Mecanismos de ayuda en posición consistente.
- **3.3.7 Redundant Entry** — A. No solicitar dos veces la misma información en el mismo proceso.
- **3.3.8 Accessible Authentication (Minimum)** — AA. Autenticación sin pruebas cognitivas no asistibles (CAPTCHAs visuales, recordar contraseñas, etc.).
- **3.3.9 Accessible Authentication (Enhanced)** — AAA.

### 5.3.4 Cuadro 4 — WCAG niveles A/AA/AAA aplicados a un formulario de pago

| Aspecto | Nivel A (mínimo) | Nivel AA (estándar legal) | Nivel AAA (máximo) |
|---|---|---|---|
| **Contraste de texto** | No requerido específicamente | ≥ 4,5:1 (texto normal), 3:1 (texto grande, componentes UI) | ≥ 7:1 (texto normal) |
| **Labels** | `<label for>` asociado a cada input | + `aria-describedby` para mensajes de error | + ayuda contextual permanente disponible |
| **Color** | No depender solo de color para indicar error (ícono + texto) | + cumple contraste de borde 3:1 | — |
| **Teclado** | Toda funcionalidad operable | + foco visible (2.4.7); foco no oculto (2.4.11) | + apariencia de foco con grosor mínimo (2.4.13) |
| **Tiempo** | Pausar/extender tiempos limitados | — | Sin límites de tiempo o usuario los desactiva |
| **Lenguaje** | Atributo `lang` en raíz | — | Lenguaje en nivel de lectura secundario bajo |
| **Autenticación** | — | Sin pruebas cognitivas no asistibles (3.3.8) | Sin pruebas cognitivas alternativas a objeto/dispositivo (3.3.9) |
| **Errores** | Identificados textualmente | + sugerencia de corrección | + prevención antes de envío irreversible |

## 5.4 WAI-ARIA 1.2 — Accessible Rich Internet Applications

Recomendación W3C del 6 de junio de 2023. Provee semántica a controles dinámicos cuando HTML nativo no alcanza. Define tres familias:

- **Roles.** `role="dialog"`, `role="tab"`, `role="tabpanel"`, `role="tablist"`.
- **Estados.** Cambian con la interacción. `aria-expanded`, `aria-checked`, `aria-disabled`, `aria-pressed`.
- **Propiedades.** Se establecen al inicio. `aria-label`, `aria-labelledby`, `aria-describedby`, `aria-controls`.

**Primera regla de ARIA** (W3C, *ARIA Authoring Practices*): "no uses ARIA si podés usar un elemento HTML nativo con la semántica requerida". Un `<button>` ya es accesible; envolver un `<div role="button" tabindex="0">` agrega complejidad sin valor.

## 5.5 Documentación: IEEE 1063 e ISO/IEC 26514

- **IEEE 1063-2001** (reafirmada 2007). *Software User Documentation.* Especifica estructura mínima: identificación, completitud, exactitud, navegación, presentación. Aplica a manuales tradicionales y ayuda online.
- **ISO/IEC 26514:2022.** *Designers and developers of user documentation.* Reemplaza la edición 2008. Define el proceso de diseño y desarrollo de documentación: análisis de audiencia y tareas, arquitectura de información, estilo, estructura, gestión de versiones, accesibilidad de la propia documentación.

Complementan con: ISO/IEC/IEEE 26511 (gestión), 26512 (adquisición/suministro), 26513 (testers), 26515 (Agile).

**Implicación.** La documentación es un artefacto sujeto a estándares. En un proyecto que declara cumplimiento de ISO/IEC 26514, la documentación misma debe ser auditable: trazabilidad de versiones, glosario, índice, accesibilidad WCAG aplicada al doc final si es web.

---

# 6. Sistemas de diseño

Un *Design System* materializa de forma opinionada las heurísticas (§4) y los estándares (§5) en un cuerpo reusable de tokens, componentes y patrones. Se compone típicamente de cuatro capas:

- **Tokens.** Variables semánticas (color, tipografía, espaciado, motion) — desacoplan decisiones de implementación.
- **Componentes.** Botones, inputs, modales, etc., con sus variantes y estados.
- **Patrones.** Combinaciones recurrentes (formulario de login, tabla con filtros, *empty state*).
- **Guidelines.** Voz, tono, accesibilidad, *do/don't*.

## 6.1 Atomic Design — Frost

Brad Frost (2016, *Atomic Design*, self-published) propone una taxonomía de cinco niveles:

1. **Atoms.** Elementos básicos indivisibles (label, input, button, icon).
2. **Molecules.** Agrupaciones simples y funcionales (search field = label + input + button).
3. **Organisms.** Secciones complejas (header con nav + search + user menu; tarjeta de evento completa).
4. **Templates.** Layouts con placeholders.
5. **Pages.** Templates con contenido real.

**Crítica.** La frontera molécula/organismo es subjetiva; equipos discuten interminablemente. Frost mismo reconoce que la utilidad del modelo está en el vocabulario, no en la clasificación estricta.

## 6.2 ITCSS — Inverted Triangle CSS

Harry Roberts (2014, *csswizardry.com*) propone organizar CSS en triángulo invertido por especificidad creciente:

```text
     /-----------------\
    /     Settings      \    (variables, tokens)
    \-------------------/
     \-----------------/
      \     Tools      /    (mixins, funciones)
       \-------------/
        \-----------/
         \ Generic /     (resets, normalize)
          \-------/
           \-----/
            \   /          (Elements, Objects, Components, Utilities…)
             \/
```

Capas, de menor a mayor especificidad y de más genérico a más explícito:

1. **Settings.** Variables (Sass, CSS custom properties).
2. **Tools.** Mixins, funciones.
3. **Generic.** Resets, normalize.
4. **Elements.** Estilos de tags HTML puros (`h1`, `p`).
5. **Objects.** Patrones estructurales sin cosmética (`.o-container`).
6. **Components.** UI específica (`.c-card`, `.c-event-card__title`).
7. **Utilities / Trumps.** Helpers, `!important` controlado.

Compatible con BEM y Atomic Design; mitiga la guerra de especificidad CSS.

### Ejemplo comparativo Atomic vs ITCSS — tarjeta de evento

**Atomic Design** (composición lógica):
- *Atoms*: `Image`, `Heading`, `Text`, `Badge`, `Button`, `Icon`.
- *Molecules*: `EventMeta` (icono+fecha+lugar), `PriceTag` (símbolo+monto+desde), `EventTitle` (título+badge).
- *Organism*: `EventCard` = imagen + EventTitle + EventMeta + PriceTag + Button("Comprar").

**ITCSS** (organización CSS por capa):
- *Settings*: `$radius-card`, `$color-brand`.
- *Tools*: `@mixin truncate`, `@mixin focus-ring`.
- *Generic*: reset.
- *Elements*: estilos de `h3`, `img`, `button`.
- *Objects*: `.o-card` (padding, layout grid genérico).
- *Components*: `.c-event-card`, `.c-event-card__title`, `.c-event-card__price`.
- *Utilities*: `.u-hidden@mobile`.

Atomic Design clasifica *qué es* el componente; ITCSS clasifica *cómo se escriben sus estilos*. Son ortogonales y se combinan.

## 6.3 Tour de Design Systems de referencia

### Material Design 3 — Google (2021–presente)

Tercera generación; introduce *Material You*. Pilares:

- **Tokens.** `md.sys.color.primary`, `md.sys.typescale.body-large`. Tres niveles: *reference*, *system*, *component*.
- **Dynamic Color.** Paleta generada desde el wallpaper del usuario vía algoritmo HCT (Hue-Chroma-Tone), Android 12+.
- **Motion.** Curvas tokenizadas (`emphasized`, `standard`); duraciones canónicas; transiciones compartidas entre pantallas.
- **Componentes.** ~35 componentes (`NavigationBar`, `FAB`, `Snackbar`, `BottomSheet`).

Implementación oficial: Jetpack Compose y Material Components for Android.

### Apple Human Interface Guidelines (HIG)

Apple Inc., desde 1987, revisiones continuas. Principios actuales (desde iOS 7, 2013):

- **Clarity.** Tipografía legible (San Francisco), iconografía precisa, jerarquía visual nítida.
- **Deference.** La UI cede protagonismo al contenido; transparencias, blur.
- **Depth.** Capas visuales y transiciones realistas.

Targets táctiles 44 × 44 pt; tipografía dinámica (Dynamic Type); soporte VoiceOver. Más prescriptivo que Material en cuanto a fidelidad nativa: Apple desaconseja explícitamente que apps de iOS adopten convenciones de Android (y viceversa).

### Carbon — IBM (2017–presente)

Sistema open-source para productos enterprise. Principios:

- **Open.** Gobernanza pública, contribuciones externas.
- **Modular.** Tokens y temas (`white`, `g10`, `g90`, `g100`).
- **Inclusivo.** Accesibilidad WCAG AA por defecto en componentes; auditorías documentadas. (Carbon documenta históricamente conformidad con 2.1; las migraciones formales a 2.2, publicada en octubre de 2023, están en curso a la fecha de redacción de este marco.)
- **Neutralidad.** Estética sobria, baja personalidad, pensada para densidad informativa B2B.

### Polaris — Shopify (2017–presente)

Sistema para administradores de comercio. Principios declarados:

- *Build for merchants.*
- *Be consistent, but not uniform.*
- *Designed to be customized.*
- *Empowering.*

Foco fuerte en *content guidelines* (voz, tono, microcopy) y en componentes para data-heavy admin UIs.

## 6.4 Cuadro 5 — Material 3 vs HIG vs Carbon vs Polaris

| Atributo | Material 3 | Apple HIG | Carbon (IBM) | Polaris (Shopify) |
|---|---|---|---|---|
| **Audiencia primaria** | Apps Android, web cross-platform | iOS / iPadOS / macOS / visionOS | Productos B2B enterprise | Admins de comercio |
| **Filosofía** | Personalización dinámica (Material You) | Convención estricta de plataforma | Densidad y neutralidad | Contenido ante todo |
| **Color** | Tokens HCT + dynamic color desde wallpaper | Tint color de app + System Colors semánticos | Temas g10/g90/g100 fijos | Tokens semánticos |
| **Tipografía** | Roboto / Roboto Flex | San Francisco (SF Pro) | IBM Plex Sans | Inter |
| **Forma del botón** | Stadium (full radius) | Radius redondeado (~8-12 según control y versión iOS) | Recto (radius 0) | Radius 8 |
| **Target táctil** | 48 × 48 dp | 44 × 44 pt | 48 × 48 px (size lg) | 44 × 44 px |
| **Licencia** | Apache 2.0 | Propietario | Apache 2.0 | MIT |
| **Accesibilidad** | WCAG AA en componentes (2.1 documentada; 2.2 en curso) | Soporte VoiceOver nativo | WCAG AA documentado (2.1) | WCAG AA (2.1) |
| **Ejemplo botón "Comprar"** | `[Comprar ticket]` stadium primary | `[Comprar]` borderedProminent SF Pro | `[Comprar ticket]` recto Plex Sans | `[Comprar ahora]` Inter pill |

**Lectura.** Material apunta a flexibilidad y expresividad; HIG a fidelidad de plataforma; Carbon a densidad enterprise; Polaris a microcopy y admin densos.

---

# 7. Investigación, evaluación y métricas

Esta sección cierra la mitad transversal del documento. La investigación de usuario alimenta los modelos de proceso (§3); las métricas operacionalizan las heurísticas (§4) y las normas (§5).

## 7.1 Métodos cualitativos

### 7.1.1 Personas

Alan Cooper (1998, *The Inmates Are Running the Asylum*; refinado en *About Face*, 2014) propone "arquetipos de usuario derivados de investigación, no del marketing". Una persona contiene: nombre y foto, contexto demográfico relevante, objetivos (primarios y secundarios), comportamientos, frustraciones, escenario.

**Diferenciación clave.** Las personas se construyen desde investigación cualitativa; las *marketing personas* se construyen desde segmentación de mercado. Norman (2008) advierte sobre el uso de personas sin datos: "personas ficticias hostiles a la innovación".

### 7.1.2 User Journey y Customer Journey Map

- **User journey.** Mapa de pasos que un usuario realiza para completar una tarea con un producto digital específico.
- **Customer journey map.** Mapa más amplio de la relación cliente-marca a través de múltiples *touchpoints* y canales (Kalbach, 2020, *Mapping Experiences*, 2ª ed.).

Componentes canónicos: actor, escenario, fases, acciones, *thoughts*, *emotions* (curva), *touchpoints*, *pain points*, *opportunities*.

### 7.1.3 Service Blueprint

Distinto del journey, el *service blueprint* (Shostack, 1984, *How to Design a Service*) añade *frontstage* / *backstage* y procesos de soporte. Permite ver cómo la UX visible al cliente depende de procesos invisibles. Conecta directamente con Service Design (§2.8).

### 7.1.4 Jobs-To-Be-Done (JTBD)

Clayton Christensen (2003, *The Innovator's Solution*; 2016, *Competing Against Luck*) y Anthony Ulwick (2005, *What Customers Want*) proponen: "la gente no compra productos; los contrata para hacer un *job*". Plantilla canónica:

> "Cuando [situación], quiero [motivación], para [resultado esperado]".

Distingue *jobs* funcionales, emocionales y sociales.

**Controversia.** Dos escuelas conviven:
- **Christensen / Moesta.** *Switch interview* cualitativa.
- **Ulwick.** *Outcome-Driven Innovation*, ~150 *outcome statements* priorizados estadísticamente.

### Personas vs JTBD aplicadas al mismo usuario

**Persona estilo Cooper.**
> **Lucía, 27 años**, diseñadora freelance, vive en CABA. Foto: mujer sonriendo con auriculares.
> *Contexto*: usa el celular en el subte, plan de datos limitado, tarjeta de débito Visa.
> *Objetivos*: ir a 1 recital por mes con amigas, no perderse preventas.
> *Frustraciones*: filas virtuales que la expulsan, formularios largos, sitios que no andan en mobile.

**JTBD estilo Christensen.**
> Cuando se anuncia un recital de un artista que me gusta y mis amigas quieren ir, quiero asegurar mis tickets en menos de dos minutos sin perder mi lugar en la fila virtual, para poder coordinar con el grupo y no quedarme afuera del plan social.

**Diferencia clave.** Persona enfatiza *quién* es el usuario; JTBD enfatiza *qué progreso busca*. JTBD es agnóstico de demografía: el mismo *job* puede tenerlo un jubilado. Útil para definir features sin sesgo demográfico.

## 7.2 Métodos cuantitativos y mixtos

### 7.2.1 Card sorting

Donna Spencer (2009, *Card Sorting: Designing Usable Categories*, Rosenfeld Media). Participantes agrupan tarjetas con conceptos.

- **Open.** El usuario nombra los grupos.
- **Closed.** Grupos predefinidos.
- **Hybrid.** Mixto.

Insumo principal para arquitectura de información y taxonomías. Herramientas: OptimalSort, UserZoom.

### 7.2.2 A/B testing

Experimento controlado entre dos variantes (A control, B tratamiento) con asignación aleatoria; se evalúa significancia estadística sobre una métrica primaria. *Multivariate testing* (MVT) para múltiples variables.

**Pitfall.** *Peeking* (mirar resultados antes del tamaño muestral predefinido) infla falsos positivos (Kohavi, Tang & Xu, 2020, *Trustworthy Online Controlled Experiments*).

### 7.2.3 Eye-tracking y heatmaps

- **Eye-tracking.** Registra fijaciones y sacadas (Duchowski, 2017, *Eye Tracking Methodology*, 3ª ed.). Outputs: *gaze plots*, AOIs (areas of interest), *time to first fixation*. Patrones reportados por NN/g: F-pattern (texto), Z-pattern (landings simples), *layer-cake* (con headings claros).
- **Heatmaps.** Visualización agregada de comportamiento: *click maps*, *scroll maps*, *attention maps*, *move maps*. Herramientas: Hotjar, Microsoft Clarity. Limitación: no explican el porqué; complementar con sesiones cualitativas.

## 7.3 Métricas estandarizadas

### 7.3.1 SUS — System Usability Scale

John Brooke (1996). Diez ítems en escala Likert 1-5, alternancia positivo/negativo. *Score* 0-100. Benchmarks empíricos (Sauro & Lewis, 2016, *Quantifying the User Experience*, 2ª ed.):

- Media empírica ≈ 68.
- ≥ 80,3 → percentil 90 ("A").
- < 51 → "F".

### 7.3.2 NPS — Net Promoter Score

Reichheld (HBR, 2003). Una pregunta: "¿Qué tan probable es que recomiendes X a un amigo o colega?" en escala 0-10.

```text
NPS = % Promotores (9-10) − % Detractores (0-6)
```

**Controversia.** Schneider et al. (2008) cuestionan su validez predictiva. Útil como tracking longitudinal, no como diagnóstico puntual.

### 7.3.3 CES — Customer Effort Score

Dixon, Freeman & Toman (HBR, 2010). "¿Qué tan fácil te resultó resolver tu problema?" en escala 1-5 o 1-7. Los autores sostienen que es mejor predictor de lealtad que la satisfacción declarativa.

### 7.3.4 Métricas de tarea

- **Task success rate.** Porcentaje de usuarios que completan correctamente una tarea predefinida. *Binary* (sí/no) o *levels* (completo, parcial, fallido). Métrica primaria de eficacia (Tullis & Albert, 2013, *Measuring the User Experience*, 2ª ed.).
- **Time on task.** Tiempo de completación, generalmente reportado como mediana y desvío geométrico (distribución log-normal). Útil para eficiencia comparativa entre versiones; sin contexto cualitativo puede engañar (rapidez por confusión).
- **Error rate.** Errores por tarea o por usuario.

## 7.4 Cuadro 6 — Métricas UX comparadas

| Métrica | Qué mide | Escala | Cuándo aplicar | Benchmark |
|---|---|---|---|---|
| **SUS** | Usabilidad percibida | 0-100 | Tras una sesión de uso | Media ≈ 68; ≥80,3 = A |
| **NPS** | Probabilidad de recomendación | -100 a +100 | Tracking longitudinal | Industria-dependiente |
| **CES** | Esfuerzo percibido | 1-5 o 1-7 | Tras tarea concreta (post-soporte) | Bajo = mejor |
| **Task success rate** | Eficacia | 0-100% | Test de usabilidad | ≥ 80% es bueno |
| **Time on task** | Eficiencia | Segundos (mediana) | Comparativo entre versiones | Comparativo, no absoluto |
| **Error rate** | Calidad de la interacción | Errores / tarea | Test de usabilidad | Comparativo |

---

# 8. UX en plataformas web

A partir de aquí, el documento se especializa por plataforma. Web va antes que mobile por dependencia conceptual: *Responsive Web Design* (RWD) y *Mobile First* nacen en el contexto web, y *Mobile First* se define explícitamente *contra* desktop.

## 8.1 Paradigmas de respuesta

### 8.1.1 Responsive Web Design

Acuñado por Ethan Marcotte en *A List Apart* (2010) y consolidado en *Responsive Web Design* (Marcotte, 2011, A Book Apart). Tres pilares técnicos:

- **Fluid grid.** Layouts en unidades relativas (porcentajes, `fr`, `em`, `rem`). Fórmula original: `target / context = result`.
- **Flexible images.** `max-width: 100%; height: auto`. Hoy: `srcset`, `sizes`, `<picture>`.
- **Media queries.** CSS3. Hoy también `prefers-color-scheme`, `prefers-reduced-motion`, `prefers-contrast`.

### 8.1.2 Mobile First vs Desktop First

Luke Wroblewski (2011, *Mobile First*) propone diseñar primero para la restricción mayor (pantallas pequeñas, conexiones limitadas, input táctil) y enriquecer hacia arriba. Argumentos:

- Disciplina la jerarquía de contenidos (lo esencial primero).
- Alinea con el tráfico global (>60% mobile desde 2017, StatCounter).
- En CSS se traduce en *media queries* `min-width`, no `max-width`.

**Desktop First** parte del layout amplio y adapta hacia abajo. Suele producir interfaces sobrecargadas en mobile y cascadas de *overrides*.

### 8.1.3 Progressive Enhancement

Steve Champeon y Nick Finck (2003, SXSW). Se construye sobre HTML semántico funcional, se añade CSS para presentación y JS para interactividad opcional. Garantiza accesibilidad universal y resiliencia ante fallos. Contraparte: *graceful degradation*, partir del óptimo y proveer fallbacks.

El consenso de NN/g y MDN favorece *Progressive Enhancement* por robustez, accesibilidad y SEO.

## 8.2 Performance: Core Web Vitals

Métricas oficiales de Google (web.dev, 2020+) que ponderan el ranking de búsqueda desde la actualización *Page Experience* (2021).

| Métrica | Mide | Bueno | Necesita mejora | Pobre |
|---|---|---|---|---|
| **LCP** (Largest Contentful Paint) | Carga del elemento principal visible | < 2,5 s | 2,5-4 s | > 4 s |
| **INP** (Interaction to Next Paint, reemplaza FID en marzo 2024) | Latencia total de interacciones | < 200 ms | 200-500 ms | > 500 ms |
| **CLS** (Cumulative Layout Shift) | Estabilidad visual | < 0,1 | 0,1-0,25 | > 0,25 |

**Optimizaciones.** Para LCP: preload de hero image, fonts con `font-display: swap`, CDN, AVIF/WebP. Para INP: dividir tareas largas (`scheduler.yield()`), evitar long tasks (>50 ms), debouncing, web workers. Para CLS: reservar espacio (`width`/`height` o `aspect-ratio`), evitar inserción tardía.

## 8.3 Arquitectura de información aplicada

### 8.3.1 Estructuras de navegación

- **Sitemap.** Representación jerárquica del contenido.
- **Navegación primaria.** Secciones de nivel superior, persistente en navbar.
- **Navegación secundaria.** Subsecciones contextuales.
- **Breadcrumbs.** Ruta jerárquica que reduce carga cognitiva (NN/g, 2007). Tipos: *location-based*, *attribute-based*, *path-based*.

### 8.3.2 LATCH — Wurman

Richard Saul Wurman (1989, *Information Anxiety*) propone cinco esquemas universales:

- **L**ocation — geografía.
- **A**lphabet — directorios.
- **T**ime — feeds, timelines.
- **C**ategory — taxonomías.
- **H**ierarchy — rankings.

### 8.3.3 Patrones URL

- **Jerárquicas.** `/categoria/subcategoria/producto`.
- **Planas.** `/producto-x`.
- **Slugs semánticos.** Legibles, kebab-case, sin parámetros opacos. Mejoran SEO, *shareability* y accesibilidad cognitiva.

## 8.4 Patrones UI web

- **Hero.** Bloque superior de gran impacto con propuesta de valor y CTA primario.
- **Navbar.** Navegación global; persistente o sticky.
- **Footer.** Navegación secundaria, legales, sitemap.
- **Sidebar.** Navegación contextual, filtros.
- **Modal.** Foco bloqueante para tareas críticas.
- **Drawer.** Panel lateral deslizable.

### 8.4.1 Forms

- *Progressive disclosure.* Exponer solo lo necesario; revelar campos avanzados bajo demanda.
- *Inline validation.* Feedback al perder foco (Wroblewski, 2008/2009).
- *Smart defaults.* Valores precargados según contexto.

### 8.4.2 Data tables

- Sorting con indicador visual.
- Filtros facetados, persistentes en URL para *shareability*.
- Paginación vs scroll infinito: paginación favorece tareas analíticas; scroll infinito conviene para feeds. NN/g (Loranger, 2014) desaconseja scroll infinito en e-commerce por pérdida de footer y orientación.

### 8.4.3 Dashboards

- Jerarquía visual: KPIs primarios en F-pattern superior; detalles bajo demanda.
- *Visual Information Seeking Mantra* (Shneiderman, 1996): "*overview first, zoom and filter, details on demand*".

## 8.5 Arquitectura CSS

Atomic Design e ITCSS ya tratados en §6. Notas adicionales para web:

- **BEM.** *Block-Element-Modifier* (Yandex, 2009). Convención `.block__element--modifier`. Evita anidamientos profundos.
- **Tailwind.** Velocidad de iteración a costo de HTML verboso y acoplamiento markup-estilo. Adopción explosiva en proyectos modernos.
- **CSS-in-JS** (styled-components, Emotion). Dinamismo basado en props, costo runtime. Impacto negativo en INP si se hidrata mal.
- **Design tokens.** Variables semánticas formalizadas por el W3C *Design Tokens Community Group*.

## 8.6 Accesibilidad web

### 8.6.1 Semantic HTML5

Uso correcto de `<header>`, `<nav>`, `<main>`, `<article>`, `<aside>`, `<section>`, `<footer>`. Un `<div>` no transmite rol; un `<button>` sí.

### 8.6.2 ARIA en práctica

Roles, estados y propiedades de §5.4 aplicados:

- Toggle expandible: `aria-expanded`, `aria-controls`.
- Tabs: `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-selected`.
- Modal: `role="dialog"`, `aria-modal="true"`, `aria-labelledby`, focus trap.

### 8.6.3 WCAG aplicado

- Contraste mínimo **4,5:1** para texto normal, **3:1** para texto grande / componentes UI (1.4.3, 1.4.11).
- *Focus visible* nunca debe eliminarse sin reemplazo (2.4.7, 2.4.11).
- *Skip links* iniciales ("Saltar al contenido principal").
- Navegación completa por teclado, sin trampas de foco.
- *Form labels* explícitos (`<label for>` o `aria-labelledby`).
- *Heading hierarchy* sin saltos: `h1 → h2 → h3`.

### 8.6.4 Pruebas

- **Automatizadas.** axe-core (Deque), Lighthouse (Google), Pa11y, WAVE. Cubren ~30-40% de issues (Deque, 2021).
- **Manuales con lectores de pantalla.** NVDA + Firefox/Chrome (gratuito), JAWS (comercial), VoiceOver, TalkBack.
- **Solo teclado.** Tab/Shift+Tab/Enter/Space/Esc/flechas.
- **Pruebas con usuarios** con discapacidad — el patrón oro.

## 8.7 Forms: validación inline vs on-submit

### Inline validation (recomendada por Wroblewski, 2009)

- Email: al hacer *blur*, si falta `@` muestra debajo "Falta el @ en tu email" en rojo + ícono.
- Tarjeta: valida Luhn al tipear el dígito 16; si falla, borde naranja + "Revisá los últimos dígitos" (tono asistente, no acusatorio).
- Éxito: check verde en el borde derecho cuando el campo es válido.
- Tono: orientativo, en tiempo real, no bloquea.

### On-submit

- Usuario tipea todo, presiona "Pagar".
- Aparecen 3 errores juntos al tope + foco automático al primero: "No pudimos procesar tu pago. Revisá: email inválido, tarjeta vencida, CVV incompleto".
- Tono: correctivo, *post-mortem*.
- Riesgo: frustración acumulada; ventaja: sin "ruido" mientras tipea.

### 8.7.1 Cuadro 7 — Inline vs on-submit

| Aspecto | Inline | On-submit | Híbrido (recomendado) |
|---|---|---|---|
| Cuándo se valida | Al perder foco (`onBlur`) | Al hacer submit | Inline + sumario al submit |
| Carga cognitiva | Distribuida | Concentrada al final | Distribuida |
| Frustración | Baja si se hace bien | Alta si hay muchos errores | Mínima |
| Mejor para | Forms con validaciones complejas independientes | Forms cortos, validaciones cruzadas | La mayoría de los casos |
| Riesgo | Acoso si se valida `onChange` (no `onBlur`) | Frustración por descubrir errores tarde | Sobrecargo de implementación |

### Otras buenas prácticas

- **Single-column** estándar (Penzo, *UXmatters*, 2006; Baymard Institute). Multi-column solo para campos relacionados breves (ciudad/CP, mes/año tarjeta).
- **Autocomplete HTML5.** `autocomplete="email|tel|street-address|cc-number|one-time-code"`.
- **Required vs optional.** Marcar lo que sea minoría: si la mayoría son obligatorios, marcar "(opcional)"; si son minoría, marcar `*` (Enders, 2014; NN/g).
- **Mensajes de error.** Posición debajo del campo; asociación programática (`aria-describedby`); ícono + texto (no solo color); tono específico y accionable.

## 8.8 Microinteracciones

Dan Saffer (2013, *Microinteractions: Designing with Details*, O'Reilly) define un modelo de cuatro partes:

1. **Trigger.** Qué la inicia (tap, hover, evento del sistema).
2. **Rules.** Qué ocurre.
3. **Feedback.** Cómo se comunica al usuario.
4. **Loops & modes.** Meta-reglas y persistencia.

Ejemplos canónicos: toggle de like, validación inline, *pull-to-refresh*, *autosave* indicator. Refuerzan affordance, dan placer y comunican estado del sistema.

## 8.9 Casos reales

### 8.9.1 Stripe Dashboard

Referente de dashboards densos y elegantes. Combina jerarquía tipográfica fuerte, micro-tipografía, tablas filtrables con URL state, atajos de teclado (`?` para help, `Cmd+K` para command palette), modo oscuro nativo y documentación contextual. *Power users* y simplicidad coexisten.

### 8.9.2 GitHub — navegación de repos

Tabs persistentes (Code, Issues, Pull Requests, Actions), breadcrumbs en árbol de archivos, búsqueda con sintaxis avanzada (`is:open author:@me`), command palette (2023), keyboard shortcuts globales. Modelo mental jerárquico claro: org → repo → branch → path.

### 8.9.3 Notion — bloques editables

Cada elemento (párrafo, heading, toggle, *database row*) es un *block* manipulable. Slash command (`/`) como interfaz universal. Drag handle persistente. Demuestra cómo abstracciones uniformes habilitan flexibilidad sin sacrificar consistencia.

### 8.9.4 Airbnb — search-first UX

Hero centrado en búsqueda (destino, fechas, huéspedes) como tarea primaria. Grid de tarjetas con imágenes prioritarias, mapa sincronizado, filtros progresivos. Refuerza la heurística #2 de Nielsen — *match between system and the real world*.

### 8.9.5 Google Search — simplicidad radical

Logo + input + dos botones. Caso canónico de "menos es más" (Maeda, 2006, *The Laws of Simplicity*, MIT Press). Toda la complejidad reside en el back-end y se expone progresivamente.

## 8.10 Antipatrones web

- **Carruseles automáticos.** Estudios de NN/g (Nielsen, 2013) y Erik Runyon (Notre Dame, 2013): solo el primer slide recibe atención significativa (~89% de los clicks). Auto-rotation interrumpe lectura y rompe accesibilidad. Si se usan: control manual, pausa al hover/focus, indicadores claros, máximo 3-5 slides.
- **Splash screens.** Vestigio del software de escritorio. Dañan LCP y SEO; en web no aportan valor.
- **"Click here" como link text.** Anti-patrón clásico de accesibilidad. Lectores de pantalla permiten listar enlaces; "click here" descontextualizado es inutilizable. WCAG 2.4.4 *Link Purpose*. Correcto: "Descargá el [reporte anual 2025]".
- **Modales bloqueantes en mobile web.** Cubren la pantalla, dificultan cierre (X minúsculo), atrapan scroll, rompen el botón back. Google penaliza *intrusive interstitials* en mobile (2017+).
- **Pop-ups de cookies mal diseñados.** Dark patterns sancionados por CNIL francesa (2021-2024): "Aceptar todo" prominente vs "Rechazar" oculto; pre-marcado de checkboxes (ilegal en GDPR — *Planet49*, TJUE C-673/17, 2019). Buenas prácticas: paridad visual entre aceptar y rechazar, granularidad por categoría, persistencia, lenguaje claro.

---

# 9. UX en plataformas móviles

Mobile especializa lo visto en §8 con restricciones particulares de hardware y contexto. Se apoya en los Design Systems Material y HIG (§6) y en las leyes ergonómicas (§4.3).

## 9.1 Mobile First revisitado

Wroblewski (2011) ya tratado en §8.1.2. En mobile como plataforma nativa, el principio se extiende:

- **Pantalla pequeña.** Superficie ~5-7"; viewport 360-430 px CSS de ancho. Reduce densidad de información en ~80% respecto a desktop.
- **Conexión variable.** Redes 3G/4G/5G/Wi-Fi inestables; el diseño debe tolerar latencia y fallos (offline-first, retries, caching).
- **Atención fragmentada.** El usuario móvil se interrumpe cada 1-2 minutos (Oulasvirta et al., 2005, *CHI 2005*). Las tareas deben completarse en sesiones cortas.
- **Batería.** Animaciones costosas, geolocalización continua y polling drenan batería; impacto directo en abandono.
- **Sensores y biometría.** GPS, giroscopio, acelerómetro, cámara, NFC, Face ID, huella — habilitan UX contextual.
- **Notificaciones.** Canal asíncrono crítico; mal usadas generan fatiga y desinstalación (Pielot, Church & de Oliveira, 2014, *MobileHCI*).

## 9.2 Tipos de aplicaciones móviles

| Tipo | Tecnología | Acceso a APIs nativas | Performance | Distribución |
|---|---|---|---|---|
| **Nativa** | Swift/Kotlin | Total | Máxima | App Store / Play Store |
| **Híbrida (WebView)** | HTML/CSS/JS + Cordova/Ionic | Vía plugins | Media-baja | Stores |
| **Cross-platform compilada** | Flutter (Dart), React Native, .NET MAUI | Casi total | Alta | Stores |
| **PWA** | HTML/CSS/JS + Service Worker + Manifest | Limitado | Media | Web (instalable) |

## 9.3 Material 3 vs HIG en mobile

Cuadro detallado complementario al Cuadro 5 (§6.4):

| Aspecto | Material Design 3 | Apple HIG |
|---|---|---|
| **Navegación principal** | Bottom Navigation Bar (3-5 destinos) | Tab Bar (2-5 destinos), inferior |
| **Back navigation** | Botón back del sistema (gesture o barra) + back en TopAppBar | Sin back de sistema; back contextual en NavigationBar superior + edge-swipe |
| **Modales** | `ModalBottomSheet`, `Dialog`; full-screen dialogs en flujos largos | `Sheet` (form/page), `Alert`, `ActionSheet` |
| **Tipografía** | Roboto / Roboto Flex; escala display/headline/title/body/label | San Francisco; Dynamic Type con 12 tamaños |
| **Gestos** | Swipe-to-dismiss, pull-to-refresh, edge-swipe back (Android 10+) | Edge-swipe back, swipe-down, long-press contextual |
| **FAB** | Floating Action Button para acción primaria | No equivalente; acción primaria en NavigationBar o toolbar |

## 9.4 Ergonomía táctil

### 9.4.1 Fitts aplicado

Ya tratado en §4.3.1. En pantallas táctiles: **W** = ancho del *target*; **D** = distancia desde la posición natural del pulgar, no desde el centro de la pantalla.

### 9.4.2 Thumb zones — Hoober

Steven Hoober (2011, *Designing Mobile Interfaces*, O'Reilly; 2013, *UXmatters*) observó 1.333 usuarios y mapeó zonas de alcance del pulgar:

- **Natural (verde).** Tercio inferior central — alcance cómodo a una mano.
- **Stretch (amarillo).** Zona media — requiere extensión.
- **Hard to reach (rojo).** Esquinas superiores opuestas al pulgar — incómodas, riesgo de caída del dispositivo.

Hoober (2013) reportó que ~49% del uso es a una mano, ~36% con sostén a dos manos pero pulgar único, ~15% con dos pulgares. **Diseñar asumiendo uso dominante a una mano** salvo en tareas de input intensivo.

### 9.4.3 Tamaños mínimos comparados

| Estándar | Mínimo | Ámbito |
|---|---|---|
| **Apple HIG** | 44 × 44 pt | iOS nativo |
| **Material Design** | 48 × 48 dp | Android nativo |
| **WCAG 2.2 (2.5.8)** | 24 × 24 CSS px (AA) | Web mínimo |
| **WCAG 2.5.5** | 44 × 44 CSS px (AAA) | Web reforzado |

## 9.5 Patrones de navegación móvil

### 9.5.1 Cuadro 8 — Patrones de navegación

| Patrón | Descripción | Ideal cuando | Evitar cuando |
|---|---|---|---|
| **Bottom navigation / Tab bar** | 3-5 destinos persistentes | Secciones paralelas, frecuencia alta de cambio | >5 secciones, jerarquía profunda |
| **Drawer (hamburger)** | Oculta navegación tras icono ≡ | Muchos destinos secundarios, app utility | Destinos primarios frecuentes |
| **Tabs internos** | TabRow / segmented control | Vistas alternas de un mismo destino | Navegación primaria |
| **Modal stacks** | Flujos lineales encapsulados | Tarea acotada con inicio y fin | Navegación libre o exploratoria |
| **Gestos** | Edge-swipe back, swipe-down dismiss | Navegación complementaria | Único medio de descubrir acción |

### 9.5.2 Aplicación a un app de tickets

App con 5 secciones (Home, Eventos, Mis tickets, Notificaciones, Perfil):

- **Bottom navigation.** *Default ideal*. 5 íconos fijos abajo (recomendado por Material e iOS). Máxima descubribilidad.
- **Drawer.** Penaliza descubribilidad: estudios reportan caída de engagement de hasta 30% (Pernice & Budiu, NN/g, 2014). Time, Facebook y otros lo abandonaron entre 2014-2016. *Mal fit acá.*
- **Tab superior.** Si las secciones son vistas alternas del mismo contenido (ej. "Próximos / Pasados" dentro de "Mis tickets"). No para navegación primaria.
- **Gesture-based.** App de uso muy frecuente con power users (Tinder, Instagram Stories). *Comprador casual no descubre los gestos.*

**Recomendación.** Bottom navigation para las 5 primarias; tab superior dentro de "Mis tickets" para Próximos/Pasados.

### 9.5.3 Anti-patrones de navegación

- **Hamburger en pantalla principal con destinos primarios.** Reduce engagement ~30%.
- **Navegación inconsistente entre stacks.** Que el back vuelva a destinos distintos según ruta confunde el modelo mental.
- **Bottom navigation con scroll-to-hide agresivo.** Oculta navegación cuando el usuario más la necesita.

## 9.6 Inputs móviles

### 9.6.1 Teclados contextuales

- **Android.** `android:inputType="textEmailAddress|number|phone|textUri|textPassword"`.
- **iOS.** `UIKeyboardType` (`.emailAddress`, `.numberPad`, `.decimalPad`, `.phonePad`, `.URL`); `textContentType` para autocompletado semántico.
- **Web/PWA.** `inputmode="numeric|decimal|tel|email|url|search"` y `type="email|tel|url"`.

Mostrar el teclado correcto reduce errores y acelera input — un campo de email sin `@` accesible es una de las fricciones más reportadas (Baymard Institute, 2022, *Mobile Forms*).

### 9.6.2 Autocompletado y biometría

- **Autofill.** `autocomplete="cc-number|email|tel|street-address"` (HTML), `textContentType` (iOS), `autofillHints` (Android).
- **Biometría.**
  - iOS: `LocalAuthentication` (Touch ID 2013, Face ID 2017).
  - Android: `BiometricPrompt` (AndroidX Biometric, Android 9+).
  - Web: WebAuthn / Passkeys (FIDO Alliance + W3C, 2019; Passkeys 2022).

## 9.7 Performance móvil

- **App launch time.** Apple recomienda < 400 ms cold launch (WWDC 2019).
- **Core Web Vitals** aplicables a webviews y PWAs.
- **Skeleton screens vs spinners.** Bill Holman (2013) y Facebook (2017): skeletons reducen percepción de espera frente a spinners.
- **Optimistic UI updates.** Linear, Superhuman, mensajeros: actualizar la UI asumiendo éxito y reconciliar al recibir respuesta. Reduce latencia percibida casi a cero. Requiere estrategia de rollback ante error.
- **Offline-first.** Service Workers (web). En nativo: SQLite, Room, Core Data, Realm. *Conflict resolution*: CRDTs, last-write-wins, sincronización con versionado.

## 9.8 Accesibilidad móvil

### 9.8.1 Lectores de pantalla

- **VoiceOver (iOS, 2009).** `accessibilityLabel`, `accessibilityHint`, `accessibilityTraits`. Navegación por swipe horizontal entre elementos; doble tap para activar; *rotor* para navegación por encabezados, links, formularios.
- **TalkBack (Android).** `contentDescription`, `stateDescription`. Explora por swipe; gestos de navegación configurables.

### 9.8.2 Dynamic Type / Font scaling

- **iOS.** 12 escalas (xSmall a AccessibilityXXXL); usar text styles (`.body`, `.headline`).
- **Android.** `sp` escala con preferencia del usuario; Android 14 admite escalado no lineal hasta 200%.
- Layouts deben tolerar texto al 200%+ sin truncar ni solapar (WCAG 1.4.4 *Resize Text*).

### 9.8.3 WCAG 2.2 aplicado a móvil

Especialmente relevantes:

- **2.5.8 Target Size (Minimum)** — AA: 24 × 24 CSS px mínimo.
- **2.5.7 Dragging Movements** — AA: alternativa con un solo puntero (tap simple). Crítico para sliders, reordenamientos, signature pads.
- **2.4.11 Focus Not Obscured (Minimum)** — AA: el elemento con foco no debe quedar oculto por contenido superpuesto (teclados, snackbars, modales pegajosos).

## 9.9 Notificaciones

### 9.9.1 Tipos

- **Push.** APNs (iOS), FCM (Android). Aún con app cerrada.
- **In-app.** Banners o toasts visibles solo dentro de la app activa.
- **Locales.** Programadas en el dispositivo sin servidor.

### 9.9.2 Permisos

- **iOS 12+.** Permiso explícito vía `UNUserNotificationCenter.requestAuthorization`.
- **Android 13+ (2022).** Permiso `POST_NOTIFICATIONS` requerido en runtime.

**Práctica recomendada.** *No solicitar permiso en el primer launch.* Esperar a un momento de valor demostrado. Apps que piden on-launch obtienen ~40% de aceptación; las que esperan, >70% (Localytics, 2018).

### 9.9.3 Diseño

- Relevancia: cada notificación debe accionar valor concreto.
- Frecuencia: límite por usuario y categoría (Android *notification channels* desde 8.0).
- Deep links: cada notificación abre la pantalla específica, no la home.
- Tono: título <40 caracteres, cuerpo <120; identidad clara del remitente.

## 9.10 Casos reales

### 9.10.1 Instagram — gestos

Doble tap para like (2011), swipe horizontal entre Stories, swipe vertical para descartar, pinch para zoom. Cada gesto tiene **fallback visible** (botón corazón sigue presente). Lección: gestos como aceleradores, nunca como única vía.

### 9.10.2 Uber — onboarding minimalista

Tres pantallas: teléfono, código SMS, datos básicos posteriores. *Just-in-time onboarding* difiere campos no críticos (foto, dirección de casa) hasta que se necesiten. Conversión a primer viaje >85% (Uber Engineering, 2017).

### 9.10.3 Spotify — modo offline

Sincronización selectiva (descargar playlists/álbumes) con caché transparente. Indicador "Available offline" visible y predictivo. Resuelve el contexto móvil clásico: subte, avión, datos limitados.

### 9.10.4 Mercado Pago — biometría y biprotección

Face ID/huella para login y autorización; respaldo por PIN (defensa en profundidad). Confirmación con doble factor en transferencias; resúmenes pre-confirmación con monto destacado. Anti-patrón evitado: nunca pedir credencial en notificación o deep link externo (anti-phishing).

### 9.10.5 WhatsApp — voice messages

*Press-and-hold to record*, slide-up to lock, slide-left to cancel. Innovación de input que reconoce que escribir en móvil es costoso. >7.000 millones de mensajes de voz diarios (Meta, 2022). Lección: cuando la fricción del input estándar es alta, una alternativa modal puede ser transformadora.

## 9.11 Antipatrones móviles

- **Modal sobre modal.** Stacks de diálogos sin mapeo mental claro; back ambiguo. Solución: máquina de estados clara, un modal a la vez o *full-screen takeover*.
- **Tipografía no escalable.** Tamaños fijos en `px`/`pt` que ignoran Dynamic Type y *font scaling*. Excluye usuarios con baja visión y viola WCAG 1.4.4.
- **Gestos invisibles.** Funcionalidad accesible solo por gestos no señalizados. Solución: combinar con icono o long-press visible; respetar 2.5.7 *Dragging Movements*.
- **Auto-play de video con audio.** Apple bloquea autoplay con audio en Safari iOS desde 2017; buena práctica: muted-by-default.
- **Tap targets pegados.** Targets contiguos sin separación generan *mistaps*; respetar 8 dp/pt mínimo entre targets.
- **Splash screens largas como branding.** Cada segundo superior a ~1s erosiona retención (Localytics, 2019).
- **Notificaciones genéricas y frecuentes.** Causa #1 de desinstalación (Adjust, Localytics).

---

# 10. Developer Experience en REST APIs

DX (§2.9) se trata aquí como UX especializada: el "usuario" es un desarrollador y la "interfaz" es código, contratos, mensajes de error y documentación. Las heurísticas de §4 se reformulan; muchas leyes (Postel, Jakob) aplican literalmente. Esta sección cubre métricas, estándares, modelos de error, patrones y documentación como interfaz.

## 10.1 DX como UX especializada

DX hereda principios UX y los traduce:

| UX clásica | DX |
|---|---|
| Visibilidad del estado (Nielsen #1) | Logs claros, request IDs, status codes correctos |
| Recuperación de errores (Nielsen #9) | Mensajes de error con `code`, `hint`, `documentation_url` |
| Match con el mundo real (Nielsen #2) | Naming consistente con el dominio (`customer.id`, no `usrIdInt`) |
| Consistencia (Nielsen #4) | snake_case o camelCase — uno solo; convenciones de paginación uniformes |
| Ley de Jakob | "El developer ya conoció Stripe; tu API que se parece le cuesta menos" |
| Postel | Aceptar variantes de input; serializar siempre canónicamente |

## 10.2 Métricas de DX

### 10.2.1 TTFC / Time to Hello World

**Time to First Call (TTFC)** — minutos desde landing → primera llamada exitosa. **Time to Hello World (TTHW)** — variante extendida que incluye hasta el primer resultado significativo. Stripe lo redujo a < 3 min con `curl` preconfigurado y test keys visibles. Twilio (Lawson, 2021, *Ask Your Developer*) sostiene que cada minuto adicional cuesta adopción exponencial.

### 10.2.2 DORA y SPACE

**DORA** (Forsgren, Humble & Kim, 2018, *Accelerate*). Cuatro métricas reinterpretadas para el consumidor de tu API:

- *Deployment Frequency* del cliente que integra.
- *Lead Time for Changes.*
- *Change Failure Rate* — ¿cuánto rompen los cambios tuyos?
- *Mean Time to Recovery* — ¿cuán rápido se recupera el cliente?

**SPACE** (Forsgren, Storey, Maddila, Zimmermann, Houck & Butler, 2021, *ACM Queue*). Cinco dimensiones para evitar reduccionismo:

- **S**atisfaction and well-being (encuestas, eNPS).
- **P**erformance (calidad del output).
- **A**ctivity (commits, PRs, deploys — usar con cuidado, no como vanity).
- **C**ommunication and collaboration (review latency).
- **E**fficiency and flow (interrupciones).

Regla: medir al menos tres dimensiones y al menos una perceptual.

### 10.2.3 Otras métricas DX

- **Time to First Successful Integration (TTFSI).** Sign-up → call productivo. Tracking 24h, 7d, 30d.
- **Error rate por endpoint.** Distribución 4xx/5xx; un 4xx alto puntual indica mala doc o mal diseño del payload, no mal cliente.
- **Onboarding funnel.** sign-up → email verified → API key creada → primera call test → primera call productiva → primer webhook. Cada paso con drop-off mide fricción.
- **Documentation search-to-success.** % de búsquedas que terminan en código copiado.
- **Support ticket rate / 1.000 req.** Baja → self-service alto → buena DX.

## 10.3 Estándares y especificaciones

| Estándar | Año / Versión | Rol |
|---|---|---|
| **RFC 9110** | Fielding, Nottingham & Reschke, 2022 | *HTTP Semantics*. Reemplaza 7230-7235. Lectura obligada. |
| **OpenAPI 3.1** | OAI, 2021 | Contrato machine-readable. Habilita SDKs autogenerados, mocks, validación. |
| **JSON:API** | v1.1, 2022 | Convención fuerte: `data`, `included`, `relationships`, `links`. |
| **HAL** | Kelly, 2012 | Hipermedia liviana con `_links` y `_embedded`. |
| **HATEOAS** | Fielding, 2000 (tesis doctoral) | Restricción REST original. |
| **RFC 7807** | Nottingham & Wilde, 2016 | *Problem Details for HTTP APIs*. Formato canónico de error. |
| **RFC 9457** | Nottingham, Wilde & Sanchez, 2023 | Reemplaza 7807. Aclara extensiones y `errors[]`. |
| **SemVer 2.0.0** | Preston-Werner, semver.org | MAJOR.MINOR.PATCH. Contrato de compatibilidad. |

## 10.4 Modelos de error en la industria

### 10.4.1 Mismo error ("tarjeta declinada por fondos") expresado en cinco estilos

**Estilo malo (anti-patrón).**
```http
HTTP/1.1 200 OK
{ "ok": false, "msg": "error" }
```
Status 200 oculta el fallo a proxies/monitoring; mensaje no accionable; sin código estable; sin request_id; sin link a doc.

**Estilo Stripe.**
```http
HTTP/1.1 402 Payment Required
{
  "error": {
    "type": "card_error",
    "code": "card_declined",
    "decline_code": "insufficient_funds",
    "message": "Your card has insufficient funds.",
    "doc_url": "https://stripe.com/docs/error-codes/card-declined",
    "request_id": "req_8f2k1aZ"
  }
}
```

**Estilo GitHub.**
```http
HTTP/1.1 422 Unprocessable Entity
{
  "message": "Validation Failed",
  "errors": [
    { "resource": "Payment", "field": "card", "code": "insufficient_funds" }
  ],
  "documentation_url": "https://docs.example.com/v3/payments/#errors"
}
```

**Estilo Twilio.**
```http
HTTP/1.1 400 Bad Request
{
  "code": 30007,
  "message": "Card declined: insufficient funds",
  "more_info": "https://www.example.com/docs/errors/30007",
  "status": 400
}
```

**Estilo RFC 9457 puro.**
```http
HTTP/1.1 402 Payment Required
Content-Type: application/problem+json
{
  "type": "https://example.com/probs/insufficient-funds",
  "title": "Insufficient funds",
  "status": 402,
  "detail": "The card ending in 4242 was declined due to insufficient funds.",
  "instance": "/orders/9931/payments/77"
}
```

### 10.4.2 Cuadro 9 — Modelos de error comparados

| Aspecto | Stripe | GitHub | Twilio | RFC 9457 puro |
|---|---|---|---|---|
| **Granularidad por campo** | Sí (`param`) | Sí (`errors[]`) | No | Vía extensión |
| **Código estable** | `code` string | `code` string | `code` numérico | `type` URI |
| **Link a documentación** | `doc_url` | `documentation_url` | `more_info` | `type` URI |
| **Request ID para debug** | `request_id` | Header | Header | Vía extensión |
| **Status HTTP correcto** | Sí | Sí | Sí | Sí |
| **Adopción industrial** | Muy alta | Alta | Alta | Estándar formal |

### 10.4.3 Códigos HTTP correctos

- **4xx.** El cliente puede arreglarlo (mal request, sin auth, sin permisos, recurso no existe).
- **5xx.** El servidor falló; el cliente solo puede reintentar.
- **400 Bad Request.** Payload sintácticamente inválido (JSON roto, tipo incorrecto).
- **422 Unprocessable Content** (RFC 9110). Sintácticamente válido pero semánticamente inválido (email correcto pero ya existe; fecha de fin anterior a la de inicio).
- **409 Conflict.** Estado del recurso impide la operación (intentar pagar una invoice ya pagada).
- **429 Too Many Requests.** Rate limit; debe acompañarse de `Retry-After`.
- **503 Service Unavailable.** Caída temporal; con `Retry-After`.

**Anti-patrón frecuente.** Devolver `200 OK` con `{"error": "..."}` en el body. Rompe toda capa intermedia (proxies, monitoring, retries automáticos de SDKs).

## 10.5 Patrones de DX

### 10.5.1 Idempotency keys

Header `Idempotency-Key: <uuid>`. El cliente puede reintentar con seguridad operaciones no idempotentes (POST). El servidor cachea la respuesta 24 h. Stripe lo documenta en `https://stripe.com/docs/api/idempotent_requests`. Esencial para pagos, transferencias, cualquier acción con efecto secundario monetario.

### 10.5.2 Expandable objects

`?expand[]=customer&expand[]=invoice.subscription`. El cliente decide la profundidad del payload, evita N+1 calls. Stripe lo introdujo; Salesforce y otros lo adoptaron.

### 10.5.3 Paginación cursor-based vs offset

- **Offset** (`?page=3&size=20`). Vulnerable a inserciones concurrentes (un nuevo registro entre páginas duplica un ítem en la página siguiente).
- **Cursor-based** (`?starting_after=evt_abc&limit=20`, con `has_more` en respuesta). Inmune a inserciones concurrentes. Adoptado por Stripe, GitHub, Slack.

### 10.5.4 Conditional requests

Headers `If-None-Match` (con `ETag`) y `If-Modified-Since`. Respuestas `304 Not Modified` no consumen rate limit. GitHub las usa intensivamente.

### 10.5.5 Rate-limit headers

- `X-RateLimit-Limit`
- `X-RateLimit-Remaining`
- `X-RateLimit-Reset`
- `X-RateLimit-Used`
- `X-RateLimit-Resource`

Permiten al cliente autoregularse antes de recibir 429.

### 10.5.6 Webhooks

- Firma HMAC en header `X-Twilio-Signature` o equivalente, para verificar autenticidad.
- Reintentos con backoff exponencial documentado.
- Respuesta esperada `2xx` en < 15s; cualquier otra cosa = retry.
- Webhook debugger en consola con replay (Twilio, 2021).

### 10.5.7 Block Kit y 12-factor

- **Slack Block Kit.** En vez de exponer 30 endpoints de UI, Slack expone un *JSON DSL declarativo* para componer mensajes interactivos. El developer describe la UI, no la dibuja. Builder visual exporta JSON listo. Reduce TTHW de mensajes ricos de horas a minutos.
- **12 Factor App** (Wiggins, 2011, *12factor.net*). Codebase, dependencies, config, backing services, build/release/run, processes, port binding, concurrency, disposability, dev/prod parity, logs, admin processes. No es estándar formal pero estructura el contrato implícito de DX entre PaaS y desarrollador.

## 10.6 Versionado: URL path vs header

### Aplicación al mismo endpoint `GET /events`

**URL versioning.**
```
GET https://api.example.com/v1/events?city=BA
GET https://api.example.com/v2/events?city=BA
```
- *Pros*: cacheable, debuggable en logs, descubrible en navegador.
- *Contras*: rompe el principio de "una URL = un recurso".

**Header versioning (media type).**
```
GET /events?city=BA
Accept: application/vnd.example.v2+json
```

**Header versioning (date-based, estilo Stripe/GitHub).**
```
GET /events?city=BA
API-Version: 2024-04-10
```
- *Pros*: URL estable; granularidad fina por fecha.
- *Contras*: invisible en logs estándar; clientes deben configurarlo explícitamente; cache por `Vary: Accept` o `Vary: API-Version`.

### 10.6.1 Cuadro 10 — Versionado

| Estrategia | Caching | Discoverability | Descubrible en navegador | Granularidad | Adopción |
|---|---|---|---|---|---|
| **URL path** (`/v1/`) | Simple | Alta | Sí | MAJOR | GitHub, Twilio, mayoría |
| **Media type** (`Accept: vnd.x.v2+json`) | Vía `Vary` | Media | No | MAJOR/MINOR | APIs hipermedia avanzadas |
| **Date-header** (`API-Version: 2024-04-10`) | Vía `Vary` | Media | No | Granularidad fina | Stripe, GitHub GraphQL, Azure |
| **Query** (`?v=2`) | Variable | Media | Sí | MAJOR | Discouraged (rompe semántica REST) |

**Regla práctica.** URL para APIs públicas con rotación lenta de breaking changes; header con fecha para SaaS con cambios frecuentes y muchos clientes legacy. Stripe lo usa así desde 2011.

## 10.7 Documentación como interfaz: Diátaxis

Daniele Procida (2017+, *diataxis.fr*) propone cuatro modos ortogonales de documentación, según el eje "estudio vs trabajo" y "práctico vs teórico":

| | Práctico | Teórico |
|---|---|---|
| **Estudio (aprender)** | **Tutorials** — guían paso a paso, garantizan éxito | **Explanation** — explican el porqué, contexto, decisiones de diseño |
| **Trabajo (resolver)** | **How-To Guides** — recetas para problemas reales | **Reference** — descripción precisa, exhaustiva, neutral (OpenAPI) |

Confundir cuadrantes es el error #1: poner explicaciones en el reference, o convertir un tutorial en how-to. Cada cuadrante tiene tono y estructura propios.

### Estructura efectiva de SDK docs

1. **Quickstart** (5 min, copy-paste, end-to-end).
2. **Conceptos** (modelo mental: ¿qué es un Customer, una Charge?).
3. **Guías por caso de uso** (cobrar una suscripción, manejar webhooks).
4. **Reference autogenerada desde OpenAPI**.
5. **Changelog y migration guides** versionados.
6. **Recipes / cookbook** con código real.

### Sandboxes y API explorers

- **Postman.** Colecciones compartibles, mock servers, monitors.
- **Bruno.** Open source, file-based, Git-friendly.
- **Insomnia.** Foco en GraphQL y gRPC.
- **Swagger UI / Redoc / Scalar** embebidos en la doc.
- **Stripe Shell** y **GitHub API explorer.** REPL en el navegador con auth ya resuelta. Reducen TTHW casi a cero.

### Conexión con §5.5

ISO/IEC 26514:2022 e IEEE 1063-2001 cubren documentación de software como artefacto formal. Diátaxis es un meta-marco que estructura *cómo* organizar la documentación dentro de esos estándares.

---

# 11. Caso aplicado: motor DSL multi-target

Este caso documenta un producto real del repositorio donde reside este marco teórico: un motor que toma plantillas DSL (JSON), datos y perfiles de dispositivo y produce salidas en múltiples formatos (ESC/POS, vista previa UI, texto plano, PDF). El caso es interesante porque combina **UX de usuario final indirecta** (operador de caja que ve el ticket impreso), **UI de diseñador de plantillas** (define el JSON), **DX de desarrollador integrador** (consume el motor desde una app .NET MAUI) y **accesibilidad WCAG** (en el preview).

## 11.1 Caracterización del producto

El motor expone cuatro renderizadores sobre una **representación abstracta** común producida por un motor de layout. Tres usuarios coexisten sin solaparse:

- Persona primaria: *Diego, dev MAUI senior* (DX).
- Persona secundaria: *Carla, líder funcional / analista* (UI/UX del diseñador de plantillas).
- Persona terciaria: *Mateo, cajero* (UX indirecta vía output físico).

## 11.2 Mapa de usuarios

### 11.2.1 Developer integrador (DX — referencia §10)

Diego integra el motor en una app .NET MAUI que imprime tickets desde una térmica Bluetooth. La métrica clave es **TTFP (Time To First Print)**: tiempo desde "instalé el NuGet" hasta "salió un ticket correcto en hardware real". Objetivo declarado: < 60 minutos. *TTFP es a este caso lo que TTFC/TTHW (§10.2.1) es a una API SaaS pública.*

**Aplicación de §10.4.** Los errores del motor siguen el patrón canónico: `EngineDiagnostic { Code, Severity, Message, Hint, Halts, Context }`. Códigos como `DSL-VALIDATION-002`, `RENDER-ENCODING-002` son estables entre versiones menores; los tests asertan contra `Code`, no contra `Message` (i18n-able).

**Proceso de diseño aplicado.** El equipo del motor opera con un modelo de **Lean UX** (§3.4) sobre la columna vertebral de **Garrett** (§3.1): hipótesis explícitas en cada release ("creemos que `EngineDiagnostic.Code` estable reducirá tickets de soporte en X%"), iteración corta y métricas de adopción al consumidor.

### 11.2.2 Diseñador de plantillas (UX/UI — referencia §6, §8)

Carla diseña tickets, recibos y comandas modificando JSON. La métrica clave es **iteraciones a plantilla válida**: cantidad de intentos hasta que el motor acepta la plantilla. Objetivo: ≤ 3.

**Aplicación de §4.1 (Nielsen #9).** Los mensajes de error del motor incluyen ubicación precisa (path JSON, línea), código estable y *hint* accionable. No solo dicen "Plantilla inválida": dicen "DSL-VALIDATION-003: el nodo `text` en `body[2]` requiere `content` o `dataBinding`".

### 11.2.3 Operador de caja (UX indirecta vía output físico)

Mateo dispara la impresión desde la app y consume el ticket impreso. **No interactúa con el motor**, pero la calidad del ticket (legibilidad, ancho correcto, encoding fiel a los datos) es su UX.

**Aplicación de §4.3.4 (Tesler).** El motor absorbe la complejidad del encoding ESC/POS por defecto: Mateo nunca ve un `?` en lugar de una `ñ` si el perfil del dispositivo soporta CP858 con transliteración configurada.

## 11.3 Renderizado multi-target

El pipeline canónico del motor es:

```text
Plantilla DSL + Datos + Perfil
        ↓
Modelo interno
        ↓
Motor de Layout       ← lógica compartida entre los tres renderers
        ↓
Representación abstracta
        ↓
Renderizador (ESC/POS | UI Preview | Texto plano | PDF)
        ↓
Salida final
```

Los pasos previos al renderer son **idénticos** entre los tres renderizadores activos (ESC/POS, UI, texto plano). Esto es lo que permite la tolerancia formal definida en §11.5.

**Aplicación de §6.** El motor de layout es el "core" del sistema; los renderizadores son "componentes" en el sentido de Atomic Design. La estructura interna del motor se asemeja a ITCSS: del más genérico (parser DSL) al más específico (renderer ESC/POS con sus comandos hex `1B 40`, `1B 61 01`, etc.).

## 11.4 Accesibilidad WCAG 2.2 AA en el preview

El preview UI vive dentro de una app MAUI accesible. Aplicaciones explícitas:

- **Contraste 4,5:1.** Default texto negro `#000000` sobre blanco `#FFFFFF` (ratio 21:1).
- **Navegación por teclado.** Toda funcionalidad del preview accesible sin mouse.
- **Atajos.** `Ctrl + +/-/0` para zoom, `Ctrl + W` para fit-to-width, `F5` refresh, `Ctrl + I` toggle indicadores.
- **Lectores de pantalla.** Área del preview anuncia rol como `document` o `region` con etiqueta semántica.
- **WCAG 2.5.8 Target Size** se aplica a los controles del preview (zoom, indicadores), no al ticket simulado en sí (cuyo "target táctil" es N/A — es una imagen).

## 11.5 Tolerancia formal preview ↔ output físico

Caso de aplicación de la heurística de Nielsen #1 (visibilidad del estado del sistema) y de la ley de Postel (#4.3.6) reformuladas:

| Aspecto | Tolerancia | Verificación |
|---|---|---|
| Caracteres por línea (texto) | **±0** | Diff exacto contra renderer de texto plano |
| Anchos de columna en tablas | **±0** | Mismo algoritmo de columnado que ESC/POS |
| Wrapping | **±0** | Misma lógica greedy word-wrap |
| Altura vertical (líneas) | **±0** para texto puro | Conteo de saltos de línea idéntico |
| Altura vertical con QR/imagen | **±1 línea** | Estimación basada en módulo QR / aspect ratio |

**Test de fidelidad** (snapshot en CI):

```text
text_debug = engine.Render(plantilla, datos, perfil, formato=TEXTO_PLANO)
preview     = engine.Render(plantilla, datos, perfil, formato=PREVIEW_UI)
assert ExtractTextSkeleton(preview) == text_debug
```

Es decir: el "esqueleto textual" del preview debe ser **byte-exacto** al output del renderer de texto plano para el mismo input.

**Decisión documentada explícitamente.** El preview *no* es WYSIWYG estricto: la fuente visual difiere del firmware térmico. La tolerancia se acota formalmente, no se deja como buena voluntad.

## 11.6 Lecciones del caso

1. **UX no convencional ≠ UX inexistente.** Aunque el "usuario final" no tipea en una pantalla, sus criterios (legibilidad del ticket, fidelidad del encoding) son criterios UX de pleno derecho.
2. **DX como UX especializada se valida en el caso.** Los desarrolladores integradores (Diego) requieren las mismas atenciones: errores accionables, predictibilidad, documentación.
3. **La accesibilidad se aplica donde hay interfaz humana.** El preview UI es la única superficie con interacción humana directa, y allí WCAG aplica plenamente.
4. **Tolerancias formales > buenas intenciones.** Documentar "no garantiza fidelidad absoluta" sin tolerancia numérica deja el contrato abierto. Documentar "±0 caracteres por línea, ±1 línea para QR/imagen" lo cierra.
5. **El motor de layout compartido entre renderers es un pattern de Atomic Design en grande:** lógica reutilizada en múltiples salidas, exactamente como un átomo se reutiliza en moléculas y organismos.

---

# 12. Síntesis y checklist de aplicación

## 12.1 Tabla maestra: pilar × proceso × norma × métrica

| Pilar | Proceso aplicable | Norma de referencia | Métrica primaria |
|---|---|---|---|
| **UX usuario final** | Garrett + Doble Diamante | ISO 9241-210, WCAG 2.2 AA | SUS, task success rate |
| **UI** | Skeleton/Surface (Garrett) + Design System | Material 3 / HIG / Carbon / Polaris | Task success, time on task |
| **IxD** | Lean UX + microinteracciones | Nielsen #1, Shneiderman #3 | Error rate, time on task |
| **AI** | Card sorting (Spencer) + LATCH | ISO 9241-210 | Findability, task success |
| **Accesibilidad** | HCD (Human-Centered Design, ISO 9241-210) + WCAG audit | WCAG 2.2 AA, ISO 9241-11 | Conformidad WCAG, axe score |
| **Service Design** | Service blueprint | ISO 20282 (parcial) | NPS, CSAT, churn |
| **DX** | Lean UX + Diátaxis | RFC 9457, OpenAPI 3.1, semver | TTFC, error rate, support tickets |

## 12.2 Checklist verificativo de redacción/diseño

Tomado de §1.4 y reformulado como verificación operativa al cierre de cualquier proyecto/documento UX/UI/DX:

- [ ] **Personas identificadas.** Cada persona tiene nombre, rol, contexto, objetivos, frustraciones. Si hay más de una, su jerarquía está clara.
- [ ] **Niveles de fidelidad declarados.** El documento dice qué es (lo-fi, hi-fi, contrato) y qué no es.
- [ ] **Estándares aplicables citados con versión y año.** WCAG 2.2 (no solo "WCAG"), ISO 9241-210:2019, RFC 9457, etc.
- [ ] **UX vs DX clarificado.** El artefacto declara explícitamente si su usuario es final o desarrollador, y reformula heurísticas en consecuencia (§10.1).
- [ ] **Restricciones de hardware, conectividad y accesibilidad documentadas.** Pantalla, conexión, encoding, latencia, asistivos.
- [ ] **Métricas primarias definidas.** SUS, NPS, task success, TTFC, según corresponda.
- [ ] **Modelo de errores canónico.** Si hay API o software, los errores tienen `code`, `severity`, `hint`, `halts`, `context`.
- [ ] **Tolerancias formales declaradas.** Donde haya simulación o aproximación (preview vs físico), la tolerancia está numéricamente acotada.
- [ ] **Accesibilidad mínima.** WCAG nivel especificado (A/AA/AAA), criterios cumplidos enumerados.
- [ ] **Heurísticas de Nielsen aplicadas.** Especialmente #1 (estado), #5 (prevención), #9 (recuperación).
- [ ] **Decisiones congeladas vs abiertas explicitadas.** Las decisiones aún en discusión están marcadas como tales, no quedan implícitas en el código o en el siguiente artefacto.
- [ ] **Documentación organizada por Diátaxis** (si aplica). Tutoriales, how-to, reference y explanation no se mezclan.
- [ ] **Glosario y bibliografía.** Términos canónicos sin solapamiento; citas con autor, año y versión.
- [ ] **Versionado y deprecation policy.** Si hay contrato (API, doc), el versionado es semver; las deprecaciones se anuncian con tiempo.
- [ ] **Casos reales referenciados.** Al menos uno cuando sea pertinente (Stripe, GitHub, Material, HIG, Mercado Pago, etc.).
- [ ] **Antipatrones enumerados.** Lo que el equipo decidió *no* hacer, documentado para no rehacerse en cada iteración.

---

# Apéndice A — Glosario

Términos canónicos del marco teórico, ordenados alfabéticamente. Cuando un término colisiona con uso vernáculo del repositorio (motor DSL), se aclara la diferencia.

- **Accesibilidad.** Usabilidad por personas con el rango más amplio de capacidades (ISO 9241-11:2018). En la web, normada por WCAG 2.2.
- **AI / Arquitectura de Información.** Disciplina de organizar y rotular contenido para usabilidad y findability (Rosenfeld, Morville & Arango, 2015).
- **ARIA.** Accessible Rich Internet Applications (W3C, 1.2 desde 2023). Roles, estados y propiedades para enriquecer accesibilidad.
- **Atomic Design.** Metodología de composición en cinco niveles: atoms, molecules, organisms, templates, pages (Frost, 2016).
- **Card sorting.** Método de IA donde participantes agrupan tarjetas (Spencer, 2009).
- **CES.** Customer Effort Score (Dixon, Freeman & Toman, 2010).
- **CLS.** Cumulative Layout Shift; métrica Core Web Vitals (Google, 2020).
- **Customer Journey Map.** Mapa de la relación cliente-marca a través de touchpoints (Kalbach, 2020).
- **Design System.** Cuerpo opinionado de tokens, componentes, patrones y guidelines.
- **Design Sprint.** Proceso de 5 días para validar una idea (Knapp, Zeratsky & Kowitz, 2016).
- **Design Thinking.** Proceso en 5 modos (Empathize, Define, Ideate, Prototype, Test) (IDEO, Stanford d.school).
- **Diátaxis.** Marco para organización de documentación en cuatro cuadrantes (Procida, 2017+).
- **Doble Diamante.** Modelo de proceso del Design Council UK (2005/2019).
- **DX.** Developer Experience. UX cuando el usuario es desarrollador (Fagerholm & Münch, 2012).
- **ETag.** Header HTTP para conditional requests (RFC 9110).
- **Fitts (ley).** T = a + b · log₂(D/W + 1) (Fitts, 1954).
- **Garrett (5 capas).** Strategy → Scope → Structure → Skeleton → Surface (Garrett, 2002/2011).
- **HATEOAS.** Hypermedia As The Engine Of Application State (Fielding, 2000).
- **HCI.** Human-Computer Interaction; campo académico.
- **Hick-Hyman (ley).** RT = a + b · log₂(n+1).
- **HIG.** Human Interface Guidelines (Apple).
- **INP.** Interaction to Next Paint; métrica Core Web Vitals (reemplazó FID en marzo 2024).
- **ITCSS.** Inverted Triangle CSS (Roberts, 2014).
- **IxD.** Interaction Design (Cooper et al.).
- **Jakob (ley).** Los usuarios pasan más tiempo en otros sitios; prefieren convención (NN/g, 2000).
- **JTBD.** Jobs-To-Be-Done (Christensen, Ulwick).
- **LATCH.** Location, Alphabet, Time, Category, Hierarchy (Wurman, 1989).
- **LCP.** Largest Contentful Paint; métrica Core Web Vitals.
- **Lean UX.** Build-Measure-Learn aplicado a UX (Gothelf & Seiden, 2013/2021).
- **Material Design 3.** Sistema de diseño de Google (2021+).
- **Microinteracción.** Saffer (2013): trigger + rules + feedback + loops/modes.
- **Miller (7±2).** Capacidad de memoria de trabajo (Miller, 1956); revisión Cowan 4±1 (2001).
- **NPS.** Net Promoter Score (Reichheld, 2003).
- **OpenAPI 3.1.** Especificación machine-readable de APIs HTTP.
- **Persona.** Arquetipo de usuario derivado de investigación (Cooper, 1998/2014).
- **Postel (ley).** Conservador en lo que envías, liberal en lo que aceptás (RFC 761, 1980).
- **Problem Details.** Formato canónico de error HTTP (RFC 7807 → 9457).
- **Progressive Enhancement.** Construir desde HTML semántico hacia arriba (Champeon & Finck, 2003).
- **POUR.** Perceivable, Operable, Understandable, Robust (WCAG).
- **RWD.** Responsive Web Design (Marcotte, 2010/2011).
- **Service Blueprint.** Mapa con frontstage/backstage (Shostack, 1984).
- **SemVer.** Semantic Versioning (semver.org).
- **Service Design.** Diseño de servicio extremo-a-extremo (Stickdorn et al., 2018).
- **Shneiderman (8 reglas).** Reglas de oro de la UI (Shneiderman, 1986).
- **SPACE framework.** Forsgren et al. (2021): Satisfaction, Performance, Activity, Communication, Efficiency.
- **SUS.** System Usability Scale (Brooke, 1996).
- **Tesler (ley).** Conservación de la complejidad.
- **UI.** User Interface; capa sensorial-operativa (Galitz, 2007).
- **Usabilidad.** Eficacia + eficiencia + satisfacción (ISO 9241-11:2018).
- **UX.** User Experience (ISO 9241-210:2019; Norman, 1988/2013).
- **WCAG 2.2.** Web Content Accessibility Guidelines (W3C, 2023).

---

# Apéndice B — Bibliografía

Formato APA 7. Agrupada por bloque temático para facilitar el trabajo de quien necesite profundizar un área.

## B.1 Fundamentos UX/UI/HCI

- Card, S. K., Moran, T. P., & Newell, A. (1983). *The psychology of human-computer interaction*. Lawrence Erlbaum Associates.
- Cooper, A. (1998). *The inmates are running the asylum: Why high-tech products drive us crazy and how to restore the sanity*. Sams.
- Cooper, A., Reimann, R., & Cronin, D. (2014). *About face: The essentials of interaction design* (4th ed.). Wiley.
- Galitz, W. O. (2007). *The essential guide to user interface design* (3rd ed.). Wiley.
- Hewett, T., et al. (1992). *ACM SIGCHI curricula for human-computer interaction*. Association for Computing Machinery.
- Norman, D. A. (1988/2013). *The design of everyday things* (Revised and expanded ed.). Basic Books.
- Rosenfeld, L., Morville, P., & Arango, J. (2015). *Information architecture for the web and beyond* (4th ed.). O'Reilly Media.
- Stickdorn, M., Hormess, M. E., Lawrence, A., & Schneider, J. (2018). *This is service design doing*. O'Reilly Media.

## B.2 Modelos de proceso

- Brown, T. (2009). *Change by design: How design thinking transforms organizations and inspires innovation*. HarperBusiness.
- Design Council UK. (2005/2019). *The double diamond: A universally accepted depiction of the design process*. https://www.designcouncil.org.uk/
- Garrett, J. J. (2011). *The elements of user experience: User-centered design for the web and beyond* (2nd ed.). New Riders.
- Gothelf, J., & Seiden, J. (2021). *Lean UX: Creating great products with agile teams* (3rd ed.). O'Reilly Media.
- Knapp, J., Zeratsky, J., & Kowitz, B. (2016). *Sprint: How to solve big problems and test new ideas in just five days*. Simon & Schuster.
- Ries, E. (2011). *The lean startup*. Crown Business.

## B.3 Heurísticas, leyes y métricas

- Brooke, J. (1996). SUS: A "quick and dirty" usability scale. En P. W. Jordan, B. Thomas, B. A. Weerdmeester, & I. L. McClelland (Eds.), *Usability evaluation in industry* (pp. 189-194). Taylor & Francis.
- Cowan, N. (2001). The magical number 4 in short-term memory: A reconsideration of mental storage capacity. *Behavioral and Brain Sciences, 24*(1), 87-114.
- Dixon, M., Freeman, K., & Toman, N. (2010). Stop trying to delight your customers. *Harvard Business Review, 88*(7-8), 116-122.
- Duchowski, A. T. (2017). *Eye tracking methodology: Theory and practice* (3rd ed.). Springer.
- Fitts, P. M. (1954). The information capacity of the human motor system in controlling the amplitude of movement. *Journal of Experimental Psychology, 47*(6), 381-391.
- Hick, W. E. (1952). On the rate of gain of information. *Quarterly Journal of Experimental Psychology, 4*(1), 11-26.
- Hyman, R. (1953). Stimulus information as a determinant of reaction time. *Journal of Experimental Psychology, 45*(3), 188-196.
- Kohavi, R., Tang, D., & Xu, Y. (2020). *Trustworthy online controlled experiments: A practical guide to A/B testing*. Cambridge University Press.
- Miller, G. A. (1956). The magical number seven, plus or minus two: Some limits on our capacity for processing information. *Psychological Review, 63*(2), 81-97.
- Nielsen, J. (1993). *Usability engineering*. Academic Press.
- Nielsen, J. (1994). Heuristic evaluation. En J. Nielsen & R. L. Mack (Eds.), *Usability inspection methods*. Wiley.
- Reichheld, F. F. (2003). The one number you need to grow. *Harvard Business Review, 81*(12), 46-54.
- Sauro, J., & Lewis, J. R. (2016). *Quantifying the user experience: Practical statistics for user research* (2nd ed.). Morgan Kaufmann.
- Shneiderman, B., Plaisant, C., Cohen, M., Jacobs, S., Elmqvist, N., & Diakopoulos, N. (2017). *Designing the user interface: Strategies for effective human-computer interaction* (6th ed.). Pearson.
- Tullis, T., & Albert, B. (2013). *Measuring the user experience: Collecting, analyzing, and presenting usability metrics* (2nd ed.). Morgan Kaufmann.

## B.4 Investigación de usuario

- Christensen, C. M., Hall, T., Dillon, K., & Duncan, D. S. (2016). *Competing against luck: The story of innovation and customer choice*. HarperBusiness.
- Kalbach, J. (2020). *Mapping experiences: A complete guide to creating value through journeys, blueprints, and diagrams* (2nd ed.). O'Reilly Media.
- Spencer, D. (2009). *Card sorting: Designing usable categories*. Rosenfeld Media.
- Ulwick, A. W. (2005). *What customers want: Using outcome-driven innovation to create breakthrough products and services*. McGraw-Hill.

## B.5 Estándares y normas

- International Organization for Standardization. (2018). *Ergonomics of human-system interaction — Part 11: Usability: Definitions and concepts* (ISO 9241-11:2018).
- International Organization for Standardization. (2019). *Ergonomics of human-system interaction — Part 210: Human-centred design for interactive systems* (ISO 9241-210:2019).
- IEEE. (2001/2007). *IEEE Standard for Software User Documentation* (IEEE 1063-2001).
- ISO/IEC. (2022). *Systems and software engineering — Developing information for users (developers and designers)* (ISO/IEC 26514:2022).
- World Wide Web Consortium. (2023, October 5). *Web Content Accessibility Guidelines (WCAG) 2.2*. https://www.w3.org/TR/WCAG22/
- World Wide Web Consortium. (2023, June 6). *Accessible Rich Internet Applications (WAI-ARIA) 1.2*. https://www.w3.org/TR/wai-aria-1.2/

## B.6 DX, REST APIs y especificaciones

- Fagerholm, F., & Münch, J. (2012). Developer experience: Concept and definition. *International Conference on Software and System Process (ICSSP)*. IEEE.
- Fielding, R. T. (2000). *Architectural styles and the design of network-based software architectures* (Doctoral dissertation, University of California, Irvine).
- Fielding, R. T., Nottingham, M., & Reschke, J. (2022, June). *HTTP semantics* (RFC 9110). Internet Engineering Task Force.
- Forsgren, N., Humble, J., & Kim, G. (2018). *Accelerate: The science of lean software and DevOps*. IT Revolution Press.
- Forsgren, N., Storey, M.-A., Maddila, C., Zimmermann, T., Houck, B., & Butler, J. (2021). The SPACE of developer productivity. *ACM Queue, 19*(1).
- Lawson, J. (2021). *Ask your developer: How to harness the power of software developers and win in the 21st century*. Harper Business.
- Nottingham, M., & Wilde, E. (2016). *Problem details for HTTP APIs* (RFC 7807). Internet Engineering Task Force.
- Nottingham, M., Wilde, E., & Sanchez, S. (2023). *Problem details for HTTP APIs* (RFC 9457). Internet Engineering Task Force.
- OpenAPI Initiative. (2021). *OpenAPI Specification 3.1.0*. https://spec.openapis.org/oas/v3.1.0
- Postel, J. (1980). *DOD standard transmission control protocol* (RFC 761). Internet Engineering Task Force.
- Preston-Werner, T. *Semantic Versioning 2.0.0*. https://semver.org
- Procida, D. (2017+). *Diátaxis*. https://diataxis.fr
- Wiggins, A. (2011). *The twelve-factor app*. https://12factor.net

## B.7 Web

- Champeon, S., & Finck, N. (2003, March). *Inclusive web design for the future*. SXSW Interactive.
- Marcotte, E. (2010, May 25). Responsive web design. *A List Apart, 306*. https://alistapart.com/article/responsive-web-design/
- Marcotte, E. (2011). *Responsive web design*. A Book Apart.
- Wroblewski, L. (2008). *Web form design: Filling in the blanks*. Rosenfeld Media.
- Wroblewski, L. (2011). *Mobile first*. A Book Apart.

## B.8 Mobile

- Apple Inc. *Human interface guidelines*. https://developer.apple.com/design/human-interface-guidelines/
- Google. *Material Design 3*. https://m3.material.io
- Hoober, S. (2013, September 2). How do users really hold mobile devices? *UXmatters*.
- Hoober, S., & Berkman, E. (2011). *Designing mobile interfaces*. O'Reilly Media.
- Oulasvirta, A., Tamminen, S., Roto, V., & Kuorelahti, J. (2005). Interaction in 4-second bursts: The fragmented nature of attentional resources in mobile HCI. *Proceedings of the SIGCHI Conference on Human Factors in Computing Systems*.
- Pernice, K., & Budiu, R. (2014). *Hamburger menus and hidden navigation hurt UX metrics*. Nielsen Norman Group.
- Pielot, M., Church, K., & de Oliveira, R. (2014). An in-situ study of mobile phone notifications. *Proceedings of MobileHCI 2014*.

## B.9 Sistemas de diseño y CSS

- Frost, B. (2016). *Atomic design*. https://atomicdesign.bradfrost.com/
- IBM. *Carbon design system*. https://carbondesignsystem.com
- Roberts, H. (2014). *ITCSS: Inverted triangle CSS*. csswizardry.com
- Saffer, D. (2013). *Microinteractions: Designing with details*. O'Reilly Media.
- Shopify. *Polaris*. https://polaris.shopify.com

## B.10 Otros

- Maeda, J. (2006). *The laws of simplicity*. MIT Press.
- Norman, D. A. (2008). Personas — what should they include? *jnd.org*. https://jnd.org
- Shostack, G. L. (1984, January-February). How to design a service. *European Journal of Marketing, 16*(1), 49-63.
- Wurman, R. S. (1989). *Information anxiety*. Doubleday.
- Ausubel, D. P. (1960). The use of advance organizers in the learning and retention of meaningful verbal material. *Journal of Educational Psychology, 51*(5), 267-272.

---

# Apéndice C — Índice de cuadros

| Nº | Cuadro | Sección |
|---|---|---|
| 1 | Las nueve disciplinas comparadas | §2.10 |
| 2 | Modelos de proceso comparados | §3.6 |
| 3 | Nielsen vs Shneiderman | §4.4 |
| 4 | WCAG niveles A/AA/AAA aplicados | §5.3.4 |
| 5 | Material 3 vs HIG vs Carbon vs Polaris | §6.4 |
| 6 | Métricas UX comparadas | §7.4 |
| 7 | Inline vs on-submit (con híbrido) | §8.7.1 |
| 8 | Patrones de navegación móvil | §9.5.1 |
| 9 | Modelos de error API comparados | §10.4.2 |
| 10 | Versionado API: URL vs header | §10.6.1 |

---

**Fin del documento — marco-teorico-ux-ui_v1.0.md**
