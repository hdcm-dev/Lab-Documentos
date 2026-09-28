# AG-03 — Especialista en Developer Experience (DX)

**Carpeta SDD:** `docs/03_ux-ui/`  
**Perfil Profesional:** Experiencia de uso de APIs y librerías

---

## 1. ¿Qué es esta especialidad?

El **Especialista en Developer Experience (DX)** es el profesional que diseña y evalúa la experiencia de uso de una librería, SDK o motor desde la perspectiva del desarrollador que lo consume. A diferencia del diseñador UX/UI clásico (enfocado en usuarios finales), el especialista DX se enfoca en la **ergonomía del API**, la claridad de los flujos de integración, la calidad de los mensajes de error y la predictibilidad del comportamiento del sistema.

En el contexto de este proyecto, el Especialista DX documenta cómo se *siente* usar el motor DSL: qué modos de uso existen (impresión, preview, debug), cómo se configuran, qué parámetros aceptan y qué salida producen. También diseña los wireframes que representan las salidas del motor.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales que se complementan según el tipo de producto:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **UX (User Experience)** | Investigar y diseñar la experiencia completa del usuario final (personas, journeys, usabilidad) | Aplicaciones con usuarios finales no-técnicos (apps móviles, web, desktop) | Personas, User Journey Maps, tests de usabilidad, Information Architecture, wireflows |
| **UI (User Interface Design)** | Diseñar la capa visual e interactiva de las pantallas (layout, colores, tipografía, componentes) | Productos con interfaz gráfica que el usuario ve y toca | Design System, mockups en Figma/Sketch, prototipos interactivos, guías de estilo, iconografía |
| **DX (Developer Experience)** | Diseñar y evaluar la experiencia de uso de APIs, SDKs, CLIs y librerías desde la perspectiva del desarrollador que las consume | Librerías, SDKs, motores, APIs, herramientas de línea de comando | Documentación de API, tablas de parámetros, wireframes de outputs, mensajes de error accionables, snippets de integración |

```
UX (experiencia general)
├── UI (interfaces visuales — usuario final)
│   ├── Sketches y wireframes de pantallas
│   ├── Flujos de navegación
│   └── Design System (tokens, componentes)
└── DX (experiencia del desarrollador)
    ├── Ergonomía del API/SDK
    ├── Wireframes de salidas del motor
    └── Mensajes de error y diagnóstico
```

> **En este proyecto** se aplica **DX (Developer Experience)** porque el producto es una librería .NET que consumen otros desarrolladores, no una aplicación con pantallas de usuario final. Los wireframes representan salidas del motor (tickets ESC/POS, vistas previas), no pantallas de una app.

#### Ejemplos temáticos por disciplina

**UX (User Experience)** *(aplica en productos con usuario final)*
- *Ejemplo:* Una app bancaria móvil necesita rediseñar el flujo de transferencias. El UX researcher hace tests de usabilidad con 12 usuarios reales, construye un Journey Map que revela 3 puntos de fricción (confirmación confusa, falta de feedback visual, timeout sin aviso), y propone un flujo simplificado de 3 pasos con feedback progresivo.

**UI (User Interface Design)** *(aplica en productos con interfaz gráfica)*
- *Ejemplo:* Un SaaS de gestión de proyectos necesita un Design System. El UI designer define tokens de diseño (colores, spacing, tipografía), crea la librería de componentes en Figma (buttons, cards, modals, tables), diseña los estados de cada componente (default, hover, active, disabled, error) y documenta las guías de accesibilidad (WCAG 2.1 AA).

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Documentación de modos de uso | Describir cada modo del motor (Printing, Preview, Debug) con configuración y output | `experiencia-de-uso-del-motor_v1.0.md` |
| Representación de salidas ESC/POS | Documentar cómo se ve un ticket impreso, con coordenadas y elementos | `representacion-documento-escpos_v1.0.md` |
| Representación de vista previa UI | Documentar el modelo visual de preview en pantalla | `representacion-vista-previa-ui_v1.0.md` |
| Wireframes de documentos | Diseñar esquemas visuales de las salidas posibles | `wireframes-documentos_v1.0.md` |
| Evaluación de ergonomía del API | Revisar si la interfaz pública es intuitiva, consistente y predecible | Informe de DX |
| Documentación de parámetros | Listar cada parámetro con tipo, default, rango y ejemplo | Tablas en cada documento |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Documentación de modos de uso | API Style Guide (Microsoft, Google) | Input para guía de integración y developer guide |
| Wireframes de salida | Low-fidelity wireframes / ASCII art | Input para tests de snapshot y validación visual |
| Representación ESC/POS | ESC/POS Command Reference (Epson) | Input para el renderizador ESC/POS y tests de bytes |
| Representación UI | .NET MAUI visual tree conventions | Input para el componente MauiDocumentPreview |
| Tabla de parámetros | OpenAPI-style parameter docs | Input para contratos del motor y guía de uso |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Modos diferenciados | Cada modo (Printing, Preview, Debug) tiene entrada/salida distintas claramente documentadas | 0 ambigüedades entre modos |
| Parámetros completos | Cada parámetro tiene tipo, default, rango válido y ejemplo | 100% de parámetros documentados |
| Wireframes representativos | Los wireframes reflejan la salida real del motor | Comparación wireframe vs output real |
| Snippets ejecutables | Los ejemplos de código compilan y producen el resultado descrito | 0 snippets rotos |
| Mensajes de error claros | Los mensajes de error documentados son accionables (dicen qué hacer) | 0 mensajes genéricos |
| Consistencia con renderizadores | Lo documentado coincide con lo que producen los renderers | 0 diferencias |

---

## 5. Preguntas guía para el análisis

### 5.1 Modos de uso

> **P1:** ¿Cuántos modos de uso tiene el motor y en qué se diferencian?
>
> *Ejemplo práctico:*
> | Modo | Propósito | Salida | Requiere hardware |
> |------|-----------|--------|-------------------|
> | Printing | Imprimir en papel térmico | `byte[]` ESC/POS | Sí (impresora BT) |
> | Preview | Ver en pantalla | `View` MAUI | No |
> | Debug | Inspeccionar en consola | `string` texto plano | No |

> **P2:** ¿Cómo selecciona el desarrollador el modo de uso?
>
> *Ejemplo práctico:* "Mediante el `DeviceProfile.Target`: `escpos`, `ui`, `text`. El motor selecciona automáticamente el renderer."

### 5.2 Parámetros y configuración

> **P3:** ¿Qué parámetros acepta el motor y cuáles son opcionales?
>
> *Ejemplo práctico:*
> | Parámetro | Tipo | Requerido | Default | Ejemplo |
> |-----------|------|-----------|---------|---------|
> | `template` | `string` (DSL) | Sí | — | `"document { text ... }"` |
> | `data` | `Dictionary<string, object>` | Sí | — | `{ "cliente": { "nombre": "Juan" } }` |
> | `profile` | `DeviceProfile` | No | `Default58mm` | `new DeviceProfile { Width = 32 }` |

### 5.3 Salidas y representaciones

> **P4:** ¿Cómo se ve un ticket ESC/POS generado por el motor?
>
> *Ejemplo práctico:*
> ```
> ================================
>         MI TIENDA S.A.
>        Av. Corrientes 1234
> ================================
> Producto          Cant.   Precio
> --------------------------------
> Café Americano       2   $450.00
> Medialunas           3   $600.00
> --------------------------------
> TOTAL:                  $1050.00
> ================================
> ```

> **P5:** ¿Qué diferencias hay entre la salida de cada renderer para el mismo documento?
>
> *Ejemplo práctico:* "ESC/POS usa comandos binarios para bold (`ESC ! n`). TextRenderer usa asteriscos `**texto**`. UIRenderer usa `FontAttributes.Bold`."

### 5.4 Errores y diagnóstico

> **P6:** ¿Qué mensajes de error ve el desarrollador y qué debe hacer?
>
> *Ejemplo práctico:*
> | Código | Mensaje | Acción sugerida |
> |--------|---------|-----------------|
> | `DSL_PARSE_ERROR` | "Unexpected token 'xyz' at line 12" | Verificar sintaxis DSL en línea 12 |
> | `DATA_MISSING` | "Field 'cliente.nombre' not found in data" | Agregar el campo al diccionario de datos |
> | `PROFILE_INVALID` | "Width must be > 0" | Revisar configuración del DeviceProfile |

---

## 6. Plantilla base

### 6.1 Documento de experiencia de uso

```markdown
# Experiencia de Uso del Motor

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

## 1. Modos de uso

### 1.1 [Modo 1: Printing]

**Propósito:** [Para qué se usa]  
**Salida:** [Tipo de output]  
**Requiere hardware:** [Sí/No]

**Configuración mínima:**

`` `csharp
var result = engine.Render(template, data, new DeviceProfile
{
    Target = "escpos",
    PaperWidth = 58
});
`` `

**Output esperado:** [Descripción del output]

### 1.2 [Modo 2: Preview]

[Misma estructura]

---

## 2. Parámetros del motor

| Parámetro | Tipo | Requerido | Default | Rango válido | Ejemplo |
|-----------|------|-----------|---------|--------------|---------|
| [param] | [tipo] | [Sí/No] | [valor] | [rango] | [ejemplo] |

---

## 3. Mensajes de error

| Código | Categoría | Mensaje | Acción del desarrollador |
|--------|-----------|---------|--------------------------|
| [código] | [Parse/Data/Render] | [Mensaje] | [Qué hacer] |

---

## 4. Flujo típico de integración

`` `
1. Instalar paquete NuGet
2. Registrar servicios: services.AddMotorDslEngine()
3. Inyectar IDocumentEngine
4. Preparar template + datos + perfil
5. Llamar engine.Render(...)
6. Procesar resultado (enviar a impresora / mostrar en UI)
`` `
```

### 6.2 Wireframe de documento

```markdown
# Wireframe — [Nombre del documento]

**Tipo de salida:** [ESC/POS / UI / Texto]  
**Ancho:** [32 / 48 / variable] caracteres

---

## Representación visual

`` `
[Dibujo ASCII del documento con todos los elementos:
  encabezado, líneas, tablas, footer, etc.]
`` `

## Elementos usados

| Elemento | Tipo DSL | Datos | Posición |
|----------|----------|-------|----------|
| Logo | `ImageNode` | Base64 | Header |
| Nombre tienda | `TextNode` | `empresa.nombre` | Header, centrado |
| Línea items | `LoopNode` > `TextNode` | `items[]` | Body |
| Total | `TextNode` | `totales.total` | Footer, bold |
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Documentar solo el flujo feliz | El desarrollador no sabe qué hacer cuando falla | Documentar cada error posible con acción sugerida |
| Parámetros sin default | El desarrollador no sabe qué valor poner | Definir defaults sensatos y documentarlos |
| Wireframes desactualizados | El wireframe muestra algo distinto al output real | Generar wireframes desde el motor (test de snapshot) |
| Mezclar DX con UX | Documentar pantallas de la app en lugar de la API | Enfocarse en la interfaz programática del motor |
| Sin snippets de código | El desarrollador tiene que adivinar cómo se usa | Mínimo 1 snippet ejecutable por modo de uso |
