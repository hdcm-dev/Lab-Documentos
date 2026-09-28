# Guía de Estudio — Cómo arrancar un proyecto con Scrum

**Documento:** guia-de-estudio.md  
**Versión:** 1.0  
**Fecha:** 2026-04-15  
**Contexto:** Guía práctica de estudio basada en ejemplos concretos para organizar un proyecto usando Scrum desde cero

---

## Índice

- [1. Punto de partida — ¿Cuándo estás listo para organizar el Scrum?](#1--punto-de-partida--cuándo-estás-listo-para-organizar-el-scrum)
- [2. Lo primero que debés construir: el Product Backlog](#2--lo-primero-que-debés-construir-el-product-backlog)
  - [Paso 1 — Formalizar la Visión del producto](#paso-1--formalizar-la-visión-del-producto)
  - [Paso 2 — Identificar las Épicas](#paso-2--identificar-las-épicas)
  - [Paso 3 — Construir el Product Backlog (User Stories + MoSCoW)](#paso-3--construir-el-product-backlog-user-stories--moscow)
  - [Paso 4 — Definir el DoR y el DoD](#paso-4--definir-el-dor-y-el-dod)
- [3. Ejemplos completos en tres contextos distintos](#3--ejemplos-completos-en-tres-contextos-distintos)
  - [Ejemplo A — Sistema de turnos para una clínica](#ejemplo-a--sistema-de-turnos-para-una-clínica)
  - [Ejemplo B — Sistema de inventario para ferretería](#ejemplo-b--sistema-de-inventario-para-ferretería)
  - [Ejemplo C — Plataforma de cursos online](#ejemplo-c--plataforma-de-cursos-online)
- [4. Cómo agrupar el backlog en épicas](#4--cómo-agrupar-el-backlog-en-épicas)
  - [Vista de mapeo: US por épica (clínica)](#vista-de-mapeo-us-por-épica-clínica)
  - [Vista consolidada por prioridad](#vista-consolidada-por-prioridad)
- [5. Criterios para definir una épica](#5--criterios-para-definir-una-épica)
- [6. El patrón que se repite](#6--el-patrón-que-se-repite)

---

## 1 — Punto de partida — ¿Cuándo estás listo para organizar el Scrum?

Antes de nombrar roles, crear tableros o escribir user stories, necesitás tener cubiertos estos insumos. No necesitan ser documentos formales — alcanza con que estén discutidos y acordados con el cliente:

| Insumo | Descripción | Equivale a... |
|---|---|---|
| **La idea y qué quiere el cliente** | Visión general del problema y del producto esperado | AG-00: Visión del producto |
| **Qué da y qué pide el sistema** | Entradas, salidas, flujos principales — los requerimientos | AG-01: Necesidades de negocio |
| **Cómo lo quiere el cliente** | Interfaces posibles: formularios, JSONs de ejemplo, capturas, reportes | AG-02/AG-03: Especificación funcional y UX |

Cuando tenés estos tres bloques discutidos, **el siguiente paso es organizar el Scrum**. Y lo primero que se construye es el **Product Backlog**.

---

## 2 — Lo primero que debés construir: el Product Backlog

El Product Backlog es la lista priorizada de todo lo que el sistema debe hacer. Es la única fuente de verdad para el equipo de desarrollo. Sin él no hay Sprint Planning, no hay velocity y no hay compromisos posibles.

Se construye en cuatro pasos secuenciales. Cada paso depende del anterior.

---

### Paso 1 — Formalizar la Visión del producto

La visión es una frase única que ancla todas las decisiones de priorización. Sin ella, cualquier feature parece igual de importante.

**Template:**
```
"Para [quién], que [necesita],
el sistema [nombre del producto] es [qué tipo de sistema]
que [beneficio clave],
a diferencia de [alternativa actual que el cliente usa hoy]."
```

**Criterio de calidad:** si la visión se puede aplicar a cualquier producto, es demasiado vaga. Debe ser específica al problema real del cliente.

---

### Paso 2 — Identificar las Épicas

Una **épica** es una capacidad funcional completa del sistema que es demasiado grande para completarse en un solo sprint. Se descompone en múltiples User Stories.

**Cantidad típica al inicio:** entre 4 y 8 épicas para un MVP.

**Cómo identificarlas:** tomá los requerimientos del cliente y preguntate: *¿qué grandes cosas debe poder hacer este sistema como unidad de valor completa?* Cada respuesta es una épica candidata.

```
Requerimientos del cliente
        ↓
¿Qué puede hacer el sistema como unidad completa?
        ↓
      Épicas
```

---

### Paso 3 — Construir el Product Backlog (User Stories + MoSCoW)

Cada épica se descompone en **User Stories** (US). Cada US describe una funcionalidad desde la perspectiva del usuario.

**Formato de User Story:**
```
Como [rol/actor],
quiero [acción concreta],
para [beneficio o razón].
```

Una vez escritas, se clasifican con **MoSCoW**:

| Prioridad | Significado | Criterio práctico |
|---|---|---|
| **Must Have** | Imprescindible para el MVP | Sin esto el sistema no cumple su propósito |
| **Should Have** | Importante pero no bloqueante | El MVP funciona sin esto, pero sería incompleto |
| **Could Have** | Deseable si hay capacidad | Agrega valor pero puede esperar a la v2 |
| **Won't Have v1** | Fuera de alcance por ahora | Documentado para no olvidar, pero no se construye |

> **Regla práctica:** si todo es Must Have, no hay priorización real. Forzate a que el Must Have sea el mínimo con el que el cliente ya resuelve su problema.

---

### Paso 4 — Definir el DoR y el DoD

Antes del primer sprint, el equipo debe acordar dos definiciones que gobiernan el flujo de trabajo:

**DoR — Definition of Ready**  
*¿Cuándo una User Story está lista para entrar a un sprint?*

Si una historia no cumple el DoR, no entra al Sprint Planning. Esto evita comprometer trabajo que no está lo suficientemente definido.

Criterios típicos de DoR:
- [ ] Tiene criterios de aceptación escritos (al menos 3 escenarios)
- [ ] Fue estimada en Story Points por el equipo
- [ ] Las dependencias técnicas están identificadas
- [ ] Los wireframes o diseños necesarios están disponibles
- [ ] El PO la aprobó como prioridad del sprint

**DoD — Definition of Done**  
*¿Cuándo una User Story está realmente terminada?*

Si una historia no cumple el DoD, no se puede marcar como cerrada. Esto evita la deuda técnica silenciosa.

Criterios típicos de DoD:
- [ ] El código fue revisado y aprobado en PR
- [ ] Tiene tests unitarios con cobertura ≥ 70%
- [ ] Pasa todos los tests en CI (build verde)
- [ ] Fue probada manualmente contra sus criterios de aceptación
- [ ] El PO la aceptó en la Sprint Review

> **Diferencia clave:**  
> DoR = condiciones para *empezar* a construir.  
> DoD = condiciones para *terminar* de construir.

---

## 3 — Ejemplos completos en tres contextos distintos

---

### Ejemplo A — Sistema de turnos para una clínica

#### Visión
> "Para **pacientes** que **necesitan sacar turno médico sin llamar por teléfono**, el sistema **TurnoYa** es una **aplicación web** que **permite reservar, cancelar y reprogramar turnos en tiempo real**, a diferencia de **llamar al consultorio en horario de atención y esperar en línea**."

#### Épicas

| ID | Épica | Capacidad del sistema |
|---|---|---|
| EP-01 | Registro y autenticación de pacientes | El sistema puede identificar quién entra |
| EP-02 | Búsqueda y reserva de turnos | El sistema puede gestionar reservas de tiempo |
| EP-03 | Gestión de agenda del profesional | El sistema puede gestionar la disponibilidad del médico |
| EP-04 | Notificaciones y recordatorios | El sistema puede comunicarse proactivamente |
| EP-05 | Historial de turnos | El sistema puede mostrar el pasado de un paciente |

#### Product Backlog

| ID | User Story | Épica | Prioridad |
|---|---|---|---|
| US-01 | Como paciente, quiero registrarme con email y contraseña, para poder acceder al sistema | EP-01 | Must Have |
| US-02 | Como paciente, quiero buscar turnos disponibles por especialidad y fecha, para elegir el que me conviene | EP-02 | Must Have |
| US-03 | Como paciente, quiero reservar un turno, para confirmar mi cita sin llamar | EP-02 | Must Have |
| US-04 | Como paciente, quiero cancelar un turno, para liberar el horario si no puedo asistir | EP-02 | Must Have |
| US-05 | Como médico, quiero ver mi agenda del día, para saber qué pacientes tengo | EP-03 | Must Have |
| US-06 | Como paciente, quiero recibir un recordatorio por email 24hs antes, para no olvidar el turno | EP-04 | Should Have |
| US-07 | Como paciente, quiero reprogramar un turno sin cancelarlo, para no perder mi lugar en la agenda | EP-02 | Should Have |
| US-08 | Como paciente, quiero ver mi historial de turnos anteriores, para recordar cuándo fui | EP-05 | Could Have |
| US-09 | Como médico, quiero bloquear franjas de mi agenda, para marcar vacaciones o reuniones | EP-03 | Could Have |
| US-10 | Como paciente, quiero iniciar sesión con Google, para no crear otra contraseña | EP-01 | Won't Have v1 |

#### DoR

Una US entra al sprint cuando:
- [ ] Tiene criterios de aceptación escritos (al menos 3)
- [ ] Está estimada en Story Points por el equipo
- [ ] Las dependencias técnicas están identificadas
- [ ] Los wireframes o mockups necesarios están disponibles
- [ ] El PO la aprobó como prioridad del sprint

> **Ejemplo ilustrativo de criterios de aceptación — US-03 (Reservar un turno):**
>
> | # | Escenario | Resultado esperado |
> |---|---|---|
> | 1 | Paciente selecciona un turno disponible | Se confirma la reserva y se muestra comprobante con fecha, hora y profesional |
> | 2 | Otro paciente ya tomó ese turno (concurrencia) | El sistema muestra error y sugiere el próximo horario disponible |
> | 3 | Paciente ya tiene un turno en la misma fecha y especialidad | El sistema no permite la doble reserva y muestra aviso |

#### DoD

Una US se considera terminada cuando:
- [ ] El código fue revisado y aprobado en PR
- [ ] Tiene tests unitarios con cobertura ≥ 70%
- [ ] Pasa todos los tests en CI
- [ ] Fue probada manualmente contra sus criterios de aceptación
- [ ] El PO la aceptó en la Sprint Review

> **Ejemplo ilustrativo de validación manual — US-03 (Reservar un turno):**
>
> - [ ] Se reservó un turno disponible → aparece en "Mis turnos" del paciente y en la agenda del médico
> - [ ] Se intentó reservar un turno ya ocupado → se mostró error sin generar reserva duplicada
> - [ ] Se verificó que el turno reservado descuenta disponibilidad en la búsqueda

#### Criterios de aceptación por User Story

> *Los criterios de aceptación se elaboran durante el refinamiento, antes de que la US entre al sprint. Acá se muestran los de las US Must Have como referencia.*

##### US-01 — Registrarse con email y contraseña

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Paciente se registra con email válido y contraseña segura | Se crea la cuenta y se redirige al inicio con sesión activa |
| 2 | Paciente intenta registrarse con un email ya registrado | El sistema muestra error indicando que la cuenta ya existe |
| 3 | Paciente envía formulario con contraseña menor a 8 caracteres | El sistema rechaza y muestra regla de contraseña |
| 4 | Paciente deja el campo email vacío | El sistema muestra validación de campo obligatorio |

##### US-02 — Buscar turnos disponibles por especialidad y fecha

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Paciente busca por especialidad y fecha con turnos disponibles | Se muestra lista de horarios disponibles con nombre del profesional |
| 2 | Paciente busca por especialidad y fecha sin turnos disponibles | Se muestra mensaje "No hay turnos disponibles" y se sugiere otra fecha |
| 3 | Paciente busca sin seleccionar especialidad | El sistema exige seleccionar al menos la especialidad |

##### US-03 — Reservar un turno

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Paciente selecciona un turno disponible | Se confirma la reserva y se muestra comprobante con fecha, hora y profesional |
| 2 | Otro paciente ya tomó ese turno (concurrencia) | El sistema muestra error y sugiere el próximo horario disponible |
| 3 | Paciente ya tiene un turno en la misma fecha y especialidad | El sistema no permite la doble reserva y muestra aviso |

##### US-04 — Cancelar un turno

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Paciente cancela un turno con más de 24hs de anticipación | Se libera el horario y se confirma la cancelación |
| 2 | Paciente cancela un turno con menos de 2hs de anticipación | El sistema advierte pero permite cancelar |
| 3 | Paciente intenta cancelar un turno que ya pasó | El sistema no permite la operación e informa que el turno ya fue |

##### US-05 — Ver agenda del día (médico)

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Médico consulta su agenda con turnos programados | Se muestra lista ordenada por hora con nombre del paciente y especialidad |
| 2 | Médico consulta su agenda un día sin turnos | Se muestra mensaje "No tenés turnos programados para hoy" |
| 3 | Un paciente cancela un turno mientras el médico ve la agenda | La agenda refleja la cancelación al refrescar |

---

### Ejemplo B — Sistema de inventario para ferretería

#### Visión
> "Para **dueños de ferreterías** que **necesitan controlar el stock sin planillas Excel**, el sistema **StockFácil** es una **aplicación de escritorio** que **permite gestionar entradas, salidas y alertas de stock mínimo en tiempo real**, a diferencia de **actualizar tablas manuales con riesgo de errores y desincronización**."

#### Épicas

| ID | Épica | Capacidad del sistema |
|---|---|---|
| EP-01 | Gestión del catálogo de productos | El sistema puede mantener el registro de productos |
| EP-02 | Movimientos de stock (entradas y salidas) | El sistema puede registrar el flujo de mercadería |
| EP-03 | Alertas y reportes | El sistema puede informar el estado del inventario |
| EP-04 | Gestión de proveedores | El sistema puede relacionar productos con sus fuentes de reposición |
| EP-05 | Usuarios y permisos | El sistema puede controlar quién hace qué |

#### Product Backlog

| ID | User Story | Épica | Prioridad |
|---|---|---|---|
| US-01 | Como encargado, quiero agregar un producto con nombre, código y precio, para tenerlo en el inventario | EP-01 | Must Have |
| US-02 | Como encargado, quiero registrar una entrada de stock, para actualizar la cantidad disponible | EP-02 | Must Have |
| US-03 | Como vendedor, quiero registrar una salida de stock, para descontar unidades al vender | EP-02 | Must Have |
| US-04 | Como dueño, quiero ver el stock actual de todos los productos, para saber qué tengo en el depósito | EP-01 | Must Have |
| US-05 | Como dueño, quiero recibir una alerta cuando un producto llega al stock mínimo, para generar el pedido a tiempo | EP-03 | Must Have |
| US-06 | Como dueño, quiero ver un reporte de movimientos por fecha, para auditar entradas y salidas | EP-03 | Should Have |
| US-07 | Como encargado, quiero cargar un proveedor con sus datos de contacto, para saber a quién llamar cuando necesito reponer | EP-04 | Should Have |
| US-08 | Como dueño, quiero exportar el inventario a Excel, para compartirlo con el contador | EP-03 | Could Have |
| US-09 | Como dueño, quiero asignar roles (vendedor / encargado), para limitar quién puede modificar precios | EP-05 | Could Have |
| US-10 | Como vendedor, quiero escanear código de barras para registrar salidas, para no tipear el código | EP-02 | Won't Have v1 |

#### DoR

- [ ] Tiene criterios de aceptación escritos
- [ ] Está estimada en Story Points
- [ ] El modelo de datos que afecta está identificado
- [ ] No tiene dependencias bloqueantes sin resolver
- [ ] El PO la aprobó como prioridad del sprint

> **Ejemplo ilustrativo de criterios de aceptación — US-02 (Registrar entrada de stock):**
>
> | # | Escenario | Resultado esperado |
> |---|---|---|
> | 1 | Encargado registra entrada con cantidad válida para un producto existente | El stock se actualiza sumando las unidades ingresadas |
> | 2 | Encargado intenta registrar entrada para un código de producto inexistente | El sistema muestra error y sugiere dar de alta el producto primero |
> | 3 | Encargado ingresa cantidad negativa o cero | El sistema rechaza la operación y muestra validación |

#### DoD

- [ ] Código revisado en PR por al menos 1 persona
- [ ] Tests unitarios con cobertura ≥ 70% en la lógica de negocio
- [ ] Integración probada contra la base de datos real (no mock)
- [ ] Validado manualmente contra criterios de aceptación
- [ ] PO aceptó la funcionalidad en Sprint Review

> **Ejemplo ilustrativo de validación manual — US-05 (Alerta de stock mínimo):**
>
> - [ ] Se redujo el stock hasta el umbral mínimo configurado → se disparó la alerta
> - [ ] Se verificó que con stock por encima del mínimo → no se genera alerta
> - [ ] Se probó un producto sin stock mínimo configurado → no genera alerta ni error

#### Criterios de aceptación por User Story

> *Los criterios de aceptación se elaboran durante el refinamiento, antes de que la US entre al sprint. Acá se muestran los de las US Must Have como referencia.*

##### US-01 — Agregar producto al inventario

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Encargado carga producto con nombre, código y precio válidos | El producto queda registrado y visible en el catálogo |
| 2 | Encargado intenta cargar un producto con un código que ya existe | El sistema rechaza e informa que el código está duplicado |
| 3 | Encargado deja el campo precio vacío o en cero | El sistema muestra validación y no permite guardar |

##### US-02 — Registrar entrada de stock

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Encargado registra entrada con cantidad válida para producto existente | El stock se actualiza sumando las unidades ingresadas |
| 2 | Encargado intenta registrar entrada para código de producto inexistente | El sistema muestra error y sugiere dar de alta el producto primero |
| 3 | Encargado ingresa cantidad negativa o cero | El sistema rechaza la operación y muestra validación |

##### US-03 — Registrar salida de stock

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Vendedor registra salida con cantidad menor o igual al stock disponible | El stock se descuenta y se registra el movimiento |
| 2 | Vendedor intenta registrar salida mayor al stock disponible | El sistema rechaza e informa el stock actual |
| 3 | Vendedor registra salida que deja el stock en el umbral mínimo | Se descuenta y se dispara la alerta de stock mínimo |

##### US-04 — Ver stock actual de todos los productos

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Dueño consulta el inventario con productos cargados | Se muestra tabla con nombre, código, stock actual y precio |
| 2 | El inventario está vacío (sin productos) | Se muestra mensaje indicando que no hay productos registrados |
| 3 | Dueño filtra por nombre o código | La lista se reduce mostrando solo los productos que coinciden |

##### US-05 — Alerta de stock mínimo

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Stock de un producto llega al umbral mínimo configurado | Se genera alerta visible en el panel del dueño |
| 2 | Stock está por encima del mínimo | No se genera alerta |
| 3 | Producto no tiene stock mínimo configurado | No genera alerta ni error |

---

### Ejemplo C — Plataforma de cursos online

#### Visión
> "Para **instructores independientes** que **necesitan vender cursos sin pagar comisiones a plataformas como Udemy**, el sistema **CursoPropio** es una **plataforma SaaS** que **permite publicar, vender y gestionar cursos con su propia marca**, a diferencia de **publicar en plataformas que se quedan con el 30-50% de cada venta y no ceden el control sobre los estudiantes**."

#### Épicas

| ID | Épica | Capacidad del sistema |
|---|---|---|
| EP-01 | Registro y perfiles (instructores y alumnos) | El sistema puede identificar y diferenciar actores |
| EP-02 | Creación y publicación de cursos | El sistema puede alojar y organizar contenidos |
| EP-03 | Inscripción y pagos | El sistema puede habilitar el acceso previo cobro |
| EP-04 | Reproducción de contenidos | El sistema puede entregar el aprendizaje |
| EP-05 | Progreso, certificados y evaluaciones | El sistema puede medir y acreditar el avance |
| EP-06 | Panel de analytics para el instructor | El sistema puede informar el desempeño comercial |

#### Product Backlog

| ID | User Story | Épica | Prioridad |
|---|---|---|---|
| US-01 | Como instructor, quiero registrarme y crear mi perfil, para tener mi espacio en la plataforma | EP-01 | Must Have |
| US-02 | Como instructor, quiero crear un curso con título, descripción y precio, para publicarlo | EP-02 | Must Have |
| US-03 | Como instructor, quiero subir videos y materiales por sección, para organizar el contenido | EP-02 | Must Have |
| US-04 | Como alumno, quiero registrarme e inscribirme a un curso, para acceder al contenido | EP-01 | Must Have |
| US-05 | Como alumno, quiero ver el contenido del curso que compré, para aprender | EP-04 | Must Have |
| US-06 | Como instructor, quiero gestionar el pago de inscripciones, para cobrar por mis cursos | EP-03 | Must Have |
| US-07 | Como alumno, quiero marcar lecciones como completadas, para llevar mi progreso | EP-05 | Should Have |
| US-08 | Como alumno, quiero recibir un certificado al completar el curso, para acreditar lo que aprendí | EP-05 | Should Have |
| US-09 | Como instructor, quiero ver cuántos alumnos tengo y cuánto recaudé, para evaluar el desempeño | EP-06 | Should Have |
| US-10 | Como alumno, quiero recibir notificaciones cuando hay nuevo contenido, para saber que hay actualizaciones | EP-04 | Could Have |
| US-11 | Como instructor, quiero aplicar descuentos o cupones de pago, para hacer promociones | EP-03 | Could Have |
| US-12 | Como alumno, quiero ver los cursos en modo offline descargados, para estudiar sin internet | EP-04 | Won't Have v1 |

#### DoR

- [ ] Tiene criterios de aceptación con al menos 3 escenarios (happy path + 2 edge cases)
- [ ] Estimada en Story Points (Planning Poker completado)
- [ ] Dependencias de integración (pagos, video hosting) identificadas
- [ ] Diseño de pantalla disponible para las US con UI
- [ ] No hay ambigüedad en el alcance: el PO aprobó los límites de la historia

> **Ejemplo ilustrativo de criterios de aceptación — US-06 (Gestionar pago de inscripciones):**
>
> | # | Escenario | Resultado esperado |
> |---|---|---|
> | 1 | Alumno completa el pago exitosamente | Se habilita el acceso al curso y se envía comprobante por email |
> | 2 | El pago es rechazado por la pasarela | El alumno ve un mensaje de error claro y no se le otorga acceso |
> | 3 | Alumno intenta inscribirse a un curso que ya compró | El sistema informa que ya tiene acceso y no genera cobro duplicado |

#### DoD

- [ ] PR aprobado por al menos 2 reviewers
- [ ] Tests unitarios ≥ 70% cobertura + tests de integración para flujos críticos (pagos, acceso)
- [ ] CI/CD verde (build + test + linter)
- [ ] Probado en los 3 browsers objetivo (Chrome, Firefox, Safari)
- [ ] PO aceptó en Sprint Review
- [ ] Documentación de API actualizada si aplica

> **Ejemplo ilustrativo de validación manual — US-02 (Crear un curso):**
>
> - [ ] Se creó un curso con todos los campos válidos → queda en estado borrador, visible solo para el instructor
> - [ ] Se intentó crear un curso sin título o sin precio → el sistema mostró validaciones y no permitió guardar
> - [ ] Se verificó que el curso creado no aparece en el catálogo público hasta ser publicado explícitamente

#### Criterios de aceptación por User Story

> *Los criterios de aceptación se elaboran durante el refinamiento, antes de que la US entre al sprint. Acá se muestran los de las US Must Have como referencia.*

##### US-01 — Registrarse como instructor

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Instructor se registra con datos válidos | Se crea perfil y accede a su panel de gestión |
| 2 | Instructor intenta registrarse con email ya existente | El sistema informa que la cuenta ya existe |
| 3 | Instructor deja campos obligatorios vacíos (nombre, email) | El sistema muestra validaciones y no permite el registro |

##### US-02 — Crear un curso con título, descripción y precio

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Instructor crea curso con todos los campos válidos | El curso queda en estado borrador, visible solo para el instructor |
| 2 | Instructor intenta crear curso sin título o sin precio | El sistema muestra validaciones y no permite guardar |
| 3 | Instructor crea el curso | El curso no aparece en el catálogo público hasta ser publicado explícitamente |

##### US-03 — Subir videos y materiales por sección

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Instructor sube un video en formato válido (mp4, webm) | El video se almacena y queda asociado a la sección correspondiente |
| 2 | Instructor intenta subir un archivo que excede el tamaño máximo | El sistema rechaza e informa el límite permitido |
| 3 | Instructor sube material a un curso que no le pertenece | El sistema deniega la operación |

##### US-04 — Registrarse como alumno e inscribirse a un curso

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Alumno se registra con datos válidos y se inscribe a un curso gratuito | Se crea cuenta y se habilita acceso al curso inmediatamente |
| 2 | Alumno intenta inscribirse a un curso pago sin completar el pago | No se otorga acceso; se redirige al flujo de pago |
| 3 | Alumno intenta registrarse con email ya existente | El sistema informa que la cuenta ya existe |

##### US-05 — Ver contenido del curso comprado

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Alumno accede a un curso en el que está inscripto | Se muestra el contenido organizado por secciones y lecciones |
| 2 | Alumno intenta acceder a un curso en el que no está inscripto | El sistema deniega el acceso y muestra opción de inscripción |
| 3 | Instructor agrega nueva lección al curso | El alumno ve el contenido actualizado en su próxima visita |

##### US-06 — Gestionar pago de inscripciones

| # | Escenario | Resultado esperado |
|---|---|---|
| 1 | Alumno completa el pago exitosamente | Se habilita acceso al curso y se envía comprobante por email |
| 2 | El pago es rechazado por la pasarela | El alumno ve mensaje de error claro y no se otorga acceso |
| 3 | Alumno intenta inscribirse a un curso que ya compró | El sistema informa que ya tiene acceso y no genera cobro duplicado |

---

## 4 — Cómo agrupar el backlog en épicas

Una vez que tenés las US escritas, el agrupamiento en épicas no es arbitrario. Acá se muestra el mapeo completo del Ejemplo A para que quede claro el criterio:

### Vista de mapeo: US por épica (clínica)

#### EP-01 — Registro y autenticación de pacientes
- US-01 — Registrarse con email y contraseña *(Must Have)*
- US-10 — Iniciar sesión con Google *(Won't Have v1)*

#### EP-02 — Búsqueda y reserva de turnos
- US-02 — Buscar turnos disponibles *(Must Have)*
- US-03 — Reservar un turno *(Must Have)*
- US-04 — Cancelar un turno *(Must Have)*
- US-07 — Reprogramar un turno *(Should Have)*

#### EP-03 — Gestión de agenda del profesional
- US-05 — Ver agenda del día *(Must Have)*
- US-09 — Bloquear franjas de agenda *(Could Have)*

#### EP-04 — Notificaciones y recordatorios
- US-06 — Recordatorio por email 24hs antes *(Should Have)*

#### EP-05 — Historial de turnos
- US-08 — Ver historial de turnos anteriores *(Could Have)*

### Vista consolidada por prioridad

| Épica | Must Have | Should Have | Could Have | Won't Have v1 |
|---|---|---|---|---|
| EP-01 Autenticación | US-01 | — | — | US-10 |
| EP-02 Reserva de turnos | US-02, US-03, US-04 | US-07 | — | — |
| EP-03 Agenda médico | US-05 | — | US-09 | — |
| EP-04 Notificaciones | — | US-06 | — | — |
| EP-05 Historial | — | — | US-08 | — |

> **El MVP mínimo** de este sistema son las US Must Have de EP-01 y EP-02 (US-01 a US-05): con esas 5 historias el sistema ya resuelve el problema central — un paciente puede registrarse y reservar un turno sin llamar por teléfono.

---

## 5 — Criterios para definir una épica

La orientación al usuario es una consecuencia, no el criterio principal. El criterio real es la **capacidad funcional completa**.

### El criterio principal: capacidad funcional completa

Una épica responde a la pregunta:  
> **¿Qué puede hacer el sistema como unidad de valor completa e independiente?**

Cada respuesta es una épica candidata.

### Los tres criterios formales

**1. Independencia funcional**  
Si la capacidad se puede construir y desplegar sin afectar a otra, son épicas distintas.  
→ EP-04 (notificaciones) se puede implementar o no sin afectar EP-02 (reservas). Son independientes.

**2. Tamaño**  
Cada épica debe ser demasiado grande para completarse en un solo sprint. Si no lo es, es directamente una US.

**3. Cohesión interna**  
Las US dentro de una épica comparten:
- Las mismas entidades de datos
- Las mismas pantallas o flujos
- El mismo contexto de negocio

### Cuándo el usuario SÍ define la épica

En sistemas con roles muy distintos, es válido separar épicas por actor **si sus flujos son completamente independientes**:

```
EP-03 Agenda del médico    →  solo el médico opera aquí
EP-02 Reserva de turnos    →  solo el paciente opera aquí
```

Pero si dos actores operan sobre la **misma** capacidad (ej: el médico aprueba el turno que el paciente reservó), eso va en la misma épica porque comparten la misma capacidad funcional.

**Regla práctica:**
```
Primero identificá la capacidad → después fijate quién la usa
No al revés.
```

---

## 6 — El patrón que se repite

Independientemente del dominio (clínica, ferretería, cursos online), el patrón es siempre el mismo:

| Elemento | Pregunta que responde |
|---|---|
| **Visión** | ¿Por qué existe este sistema y para quién? |
| **Épicas** | ¿Qué grandes capacidades tiene el sistema? |
| **Must Have** | ¿Sin qué no hay MVP? |
| **Should Have** | ¿Qué lo hace completo pero no es bloqueante? |
| **Could Have** | ¿Qué agrega valor pero puede esperar a la v2? |
| **Won't Have v1** | ¿Qué está descartado por ahora (pero documentado)? |
| **DoR** | ¿Cuándo una historia está lista para construirse? |
| **DoD** | ¿Cuándo una historia está realmente terminada? |

Cuando estos ocho elementos están definidos, el equipo tiene todo lo necesario para ejecutar el primer Sprint Planning sin ambigüedad.

---

## Control de Cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-04-15 | Versión inicial — guía de estudio basada en ejemplos de arranque de proyectos Scrum |
| 1.1 | 2026-04-15 | Se agregan ejemplos ilustrativos de criterios de aceptación en DoR/DoD y fichas de criterios de aceptación por US Must Have en los tres ejemplos |

---

**Fin del documento**
