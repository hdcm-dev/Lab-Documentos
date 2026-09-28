# Guía de Documentación UX/UI
**Documento:** metodologia-uix-ux.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-04-25
**Autor:** Referencia metodológica SDD

---

## Índice

1. [Qué se documenta en SA-03 en la práctica real](#1-qué-se-documenta-en-sa-03-en-la-práctica-real)
2. [Qué inputs necesitás del cliente para producir SA-03](#2-qué-inputs-necesitás-del-cliente-para-producir-sa-03)
3. [Cómo estructurar documentos normalizados según la industria](#3-cómo-estructurar-documentos-normalizados-según-la-industria)
4. [Cómo articular esto en la cadena](#4-cómo-articular-esto-en-la-cadena)

---

## 1. Qué se documenta en SA-03 en la práctica real

Los tres artefactos que define el `rules.md` son correctos pero escuetos. En la práctica, cada uno cubre esto:

### `flujos-de-usuario_v1.0.md`

No es un diagrama — es la narrativa de cómo cada actor recorre el sistema. Cubre:

- **Happy path:** el camino sin errores, paso a paso
- **Caminos alternativos:** variaciones válidas del flujo principal
- **Error paths:** qué pasa cuando algo falla (validación, timeout, permiso denegado)
- **Puntos de decisión:** dónde el sistema bifurca según el estado o el rol
- **Transiciones entre pantallas:** qué acción dispara cada navegación

Un flujo de usuario bien documentado te permite responder: *"Si el usuario hace X estando en estado Y, qué ve?"* sin ambigüedad.

### `wireframes-descripcion_v1.0.md`

Es una especificación textual de cada pantalla. Por cada vista documenta:

- **Título y propósito** de la pantalla
- **Elementos UI presentes** (formularios, tablas, botones, mensajes)
- **Jerarquía visual** (qué es lo primero que el usuario debe ver)
- **Acciones disponibles** y qué dispara cada una
- **Estados de la pantalla:** vacío, cargando, con datos, con error
- **Mensajes de validación** y su ubicación exacta
- **Comportamiento responsivo** si aplica

### `guia-experiencia_v1.0.md`

Es el contrato de coherencia del sistema. Cubre:

- **Principios UX** del producto (los 3-5 valores que guían cada decisión de diseño)
- **Patrones de interacción** reutilizables (cómo funciona siempre un modal, una tabla paginada, un formulario)
- **Guía de estilo básica** (tipografía, colores, iconografía si ya están definidos)
- **Tono y voz** del sistema (cómo habla el sistema al usuario en mensajes de error, confirmaciones, onboarding)
- **Accesibilidad** mínima requerida

---

## 2. Qué inputs necesitás del cliente para producir SA-03

SA-03 tiene dos fuentes de input: los **CUs de SA-02** (que ya existen cuando llega tu turno) y lo que el cliente aportó en el intake. Este es el mapa completo:

### Inputs que vienen de SA-02 (ya procesados)

- Actores del sistema y sus roles
- Flujos funcionales (precondiciones, postcondiciones, pasos del CU)
- Reglas de negocio que afectan la UI (RN-XX)
- Criterios de aceptación Given/When/Then

### Inputs que necesitás directamente del cliente

| Tipo de input | Cómo lo obtenés | Dónde va |
|---|---|---|
| Capturas del sistema actual | Screenshots en la conversación inicial | `PROJECT-BRIEF Sección 2` |
| Referencias visuales de competidores | Links o imágenes que el cliente trae | `PROJECT-BRIEF Sección 2` |
| Público objetivo final | Descripción del usuario real | `PROJECT-README Sección 4` |
| Dispositivos principales de uso | Web, mobile, tablet, kiosko | `PROJECT-README Sección 7` |
| Preferencias de identidad visual | Logo, colores de marca, tipografía | Captura o descripción |
| Flujos críticos del negocio | "Lo más importante que tiene que hacer el sistema" | Conversación |
| Puntos de dolor del sistema actual | "Lo que más les molesta hoy" | `PROJECT-README Sección 3` |
| Restricciones de accesibilidad | WCAG, usuarios con discapacidades | `PROJECT-BRIEF Sección 7` |

### Preguntas concretas para hacer al cliente antes de SA-03

```
1. ¿Tenés capturas del sistema actual o de sistemas similares que te gusten?
2. ¿Cuál es el dispositivo principal desde el que los usuarios van a operar?
3. ¿Hay una identidad visual existente (logo, colores, tipografía)?
4. ¿Cuáles son los 2-3 flujos que más van a usar los usuarios todos los días?
5. ¿Hay alguna pantalla del sistema actual que definitivamente querés mejorar?
```

---

## 3. Cómo estructurar documentos normalizados según la industria

La industria UX tiene estándares establecidos. Estos son los formatos que mapean directamente a los tres artefactos de SA-03:

---

### Documento 1 — Flujos de usuario (estándar Nielsen Norman Group)

```markdown
# Flujos de Usuario — [Nombre del sistema]

## Actor: [nombre del actor]

### FU-01 — [Nombre del flujo]

**Trigger:** [qué evento inicia este flujo]
**Pantalla de inicio:** [nombre de la pantalla]
**Pantalla de fin:** [nombre de la pantalla]
**Precondiciones:** [estado requerido antes de iniciar]

#### Happy Path

| Paso | Actor hace | Sistema responde | Pantalla resultante |
|---|---|---|---|
| 1 | Ingresa email y contraseña | Valida formato | Login |
| 2 | Hace click en "Ingresar" | Verifica credenciales | Dashboard |
| 3 | ... | ... | ... |

#### Caminos alternativos

**CA-01 — [descripción]**
En el paso [N]: [qué pasa diferente] → [resultado]

#### Error Paths

**EP-01 — Credenciales inválidas**
En el paso 2: el sistema muestra mensaje "Usuario o contraseña incorrectos".
El usuario permanece en la pantalla Login. El campo de contraseña se limpia.
Después de 5 intentos: se bloquea la cuenta por 15 minutos.

**EP-02 — Timeout de sesión**
[descripción]

#### Diagrama del flujo (textual)

[Login]
  → credenciales válidas → [Dashboard]
  → credenciales inválidas → [Login + mensaje de error]
  → primer login → [Pantalla de bienvenida]
  → olvidé contraseña → [FU-02 Recupero de contraseña]

---

**Notas de diseño:**
- [Observaciones que el diseñador debe considerar]
- [Restricciones de UX relevantes]
```

---

### Documento 2 — Wireframes descriptivos (estándar de especificación funcional de UI)

```markdown
# Wireframes — Descripción de Pantallas

## WF-01 — [Nombre de la pantalla]

**Ruta de acceso:** [cómo llega el usuario aquí]
**Actor(es) que la usan:** [roles]
**CU relacionado:** [CU-XX]

### Propósito
[Una oración: qué tarea completa el usuario en esta pantalla]

### Layout general
[Descripción de la estructura: header / sidebar / contenido principal / footer]

### Componentes de la pantalla

#### Header / Navegación
- [elemento]: [descripción y comportamiento]
- [elemento]: [descripción y comportamiento]

#### Área principal
- [elemento]: [descripción y comportamiento]

#### Formularios (si aplica)

| Campo | Tipo | Obligatorio | Validación | Mensaje de error |
|---|---|---|---|---|
| Email | text input | Sí | formato email | "Ingresá un email válido" |
| Contraseña | password | Sí | mínimo 8 caracteres | "La contraseña debe tener al menos 8 caracteres" |

#### Acciones disponibles

| Acción | Tipo | Resultado | Condición |
|---|---|---|---|
| Guardar | Botón primario | Lleva a WF-02 | Formulario válido |
| Cancelar | Botón secundario | Vuelve a WF-01 | Siempre |
| Eliminar | Botón destructivo | Modal de confirmación | Solo admin |

### Estados de la pantalla

#### Estado vacío
[Qué ve el usuario si no hay datos aún]
Mensaje sugerido: "[texto del mensaje]"
CTA sugerido: "[texto del botón de acción principal]"

#### Estado de carga
[Qué muestra el sistema mientras procesa]
Skeleton / spinner / progress bar: [cuál y dónde]

#### Estado con datos
[Descripción del estado normal con información]

#### Estado de error
[Qué muestra si hay un error del sistema]
Mensaje sugerido: "[texto]"
Acción disponible: [reintentar / volver / contactar soporte]

### Notas para el desarrollador
- [Comportamiento específico que no queda claro en la descripción]
- [Restricción técnica que afecta el diseño]

### Referencia a captura de pantalla
[SS-XX del PROJECT-BRIEF — qué se mantiene, qué cambia]
```

---

### Documento 3 — Guía de experiencia (estándar de Design System lite)

```markdown
# Guía de Experiencia — [Nombre del sistema]

## 1. Principios de diseño

> Los principios son los criterios de desempate cuando hay dos opciones válidas.

| Principio | Descripción | Implica | No implica |
|---|---|---|---|
| Claridad sobre brevedad | El sistema prefiere ser claro antes que corto | Etiquetas descriptivas, mensajes de error explicativos | Textos innecesariamente largos |
| El sistema trabaja, no el usuario | Reducir fricción en tareas frecuentes | Defaults inteligentes, guardado automático | Eliminar confirmaciones en acciones destructivas |
| [Principio] | [descripción] | [qué incluye] | [qué no incluye] |

## 2. Patrones de interacción

### Formularios
- Validación: [inline / al submit / combinada]
- Cuándo mostrar errores: [al perder foco / al submit / en tiempo real]
- Posición de mensajes de error: [debajo del campo / en alert global]
- Campos obligatorios: [marcados con asterisco / con indicador "Opcional"]

### Tablas y listados
- Paginación: [infinita / por páginas / "cargar más"]
- Filas por página default: [N]
- Ordenamiento: [columnas que permiten orden / dirección default]
- Selección múltiple: [sí/no, cómo se implementa]
- Fila vacía: [texto estándar + CTA]

### Modales y confirmaciones
- Cuándo usar modal: [acciones destructivas, formularios secundarios]
- Cuándo NO usar modal: [flujos largos, navegación entre secciones]
- Estructura del modal: [título / cuerpo / acciones]
- Confirmación de acciones destructivas: [texto estándar de confirmación]

### Notificaciones y feedback
- Toast / snackbar: [para qué acciones, duración, posición]
- Alert inline: [para qué casos]
- Empty states: [cuándo y qué incluyen]
- Loading states: [skeleton vs spinner, cuándo usar cada uno]

## 3. Tono y voz

| Situación | Tono | Ejemplo ✓ | Ejemplo ✗ |
|---|---|---|---|
| Error del usuario | Directo, sin culpa | "El email ingresado no es válido" | "Error: campo incorrecto" |
| Error del sistema | Empático, con alternativa | "No pudimos guardar los cambios. Intentá de nuevo o contactá soporte." | "Error 500" |
| Acción exitosa | Confirmatorio, breve | "Cambios guardados" | "La operación se completó exitosamente" |
| Estado vacío | Invitacional | "Todavía no tenés proyectos. Creá el primero." | "No hay registros" |
| Confirmación destructiva | Claro sobre consecuencias | "¿Eliminar este proyecto? Esta acción no se puede deshacer." | "¿Estás seguro?" |

## 4. Accesibilidad mínima requerida

- Nivel objetivo: [WCAG 2.1 AA / A / AAA]
- Contraste mínimo: [4.5:1 para texto normal, 3:1 para texto grande]
- Navegación por teclado: [qué flujos críticos deben ser navegables sin mouse]
- Screen readers: [atributos aria requeridos en componentes clave]
- Tamaño mínimo de tap target (mobile): [44x44px recomendado]

## 5. Identidad visual de referencia

| Elemento | Valor / Referencia | Notas |
|---|---|---|
| Colores de marca | [hex / referencia] | [restricciones de uso] |
| Tipografía | [fuente / sistema] | [pesos disponibles] |
| Iconografía | [biblioteca / estilo] | [tamaños estándar] |
| Logo | [referencia al archivo] | [variantes y uso] |
```

---

## 4. Cómo articular esto en la cadena

El flujo de información concreto es este:

```
Conversación con cliente
  → capturas + preferencias + flujos críticos
       ↓ harvest
  PROJECT-BRIEF Sección 2 (screenshots SS-XX)
  PROJECT-README Sección 4 (actores y dispositivos)
       ↓ SA-02 procesa
  CU-XX con flujos, RN-XX con restricciones
       ↓ SA-03 lee todo
  FU-XX (flujos de usuario normalizados)
  WF-XX (wireframes textuales)
  Guía de experiencia
       ↓ SA-05 lee
  Arquitectura técnica (decisiones de API basadas en los flujos)
```

> **Punto crítico:** SA-05 no puede diseñar una API correctamente sin SA-03.
> Si el wireframe dice "el usuario ve los resultados mientras tipea", SA-05 sabe
> que necesita un endpoint con debounce. Esa dependencia es explícita en el
> grafo del orquestador.

---

**Fin del documento — SA-03_guia-documentacion-ux-ui_v1.0.md**
