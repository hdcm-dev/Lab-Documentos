# AG-11 — Developer Advocate / Ingeniero de Aplicación

**Carpeta SDD:** `docs/11_examples/`  
**Perfil Profesional:** Desarrollo de ejemplos didácticos, aplicaciones de referencia

---

## 1. ¿Qué es esta especialidad?

El **Developer Advocate** (en su faceta de creación de ejemplos) es el profesional que construye aplicaciones de referencia completas y funcionales que demuestran cómo usar el producto en escenarios reales. A diferencia del Technical Writer (AG-10) que documenta conceptos y guías, el Developer Advocate produce **código ejecutable** que el consumidor puede clonar, ejecutar y adaptar.

En el contexto SDD, este rol produce los **ejemplos progresivos** que van desde un "hola mundo" hasta una aplicación completa de producción. Cada ejemplo es un proyecto funcional con su propia documentación, prerequisitos, instrucciones de ejecución y resultado esperado.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el alcance de la relación con la comunidad de desarrolladores:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Sample Engineering** | Construir aplicaciones de referencia completas y funcionales que demuestren el uso del producto | Todo producto técnico que necesite ejemplos ejecutables para onboarding | Proyectos de ejemplo progresivos, fixtures, datasets de prueba, scripts de setup, README por ejemplo |
| **Developer Relations (DevRel)** | Construir y nutrir la comunidad de desarrolladores alrededor del producto: evangelismo, feedback loops, presencia en ecosistema | Productos con base de desarrolladores externos (OSS, APIs públicas, plataformas) | Blog posts técnicos, talks en conferencias, workshops, community forums, feedback channels, developer surveys |
| **Developer Education** | Diseñar experiencias de aprendizaje estructuradas (cursos, workshops, certificaciones) | Productos complejos que requieren capacitación formal para adopción | Currículum de cursos, labs hands-on, exámenes de certificación, learning paths, plataforma de e-learning |

> **En este proyecto** se aplica **Sample Engineering** porque la prioridad es construir ejemplos ejecutables progresivos que demuestren cómo usar el motor DSL, no gestión de comunidad ni cursos formales.

#### Ejemplos temáticos por disciplina

**Developer Relations (DevRel)** *(aplica en productos con comunidad)*
- *Ejemplo:* Una plataforma cloud (tipo Vercel/Supabase) necesita crecer su comunidad de desarrolladores. El DevRel publica blog posts técnicos semanales ("Cómo deployar una app Next.js en 2 minutos"), da talks en 4 conferencias al año (JSConf, Netlify Conf), gestiona un Discord de 5K desarrolladores con channels temáticos, ejecuta un developer survey trimestral para priorizar features, y crea un "Champion Program" para power users que escriben contenido y dan feedback temprano.

**Developer Education** *(aplica en productos complejos)*
- *Ejemplo:* Una plataforma de data engineering (tipo Databricks/Snowflake) necesita capacitar a data engineers. El educator diseña un learning path de 3 niveles (Fundamentals → Advanced → Expert), crea labs hands-on en un sandbox pre-configurado donde el alumno ejecuta queries reales, diseña un examen de certificación con 60 preguntas prácticas (80% para aprobar), y publica workshops grabados en la plataforma de e-learning con quizzes intercalados.

### Diferencia con AG-10 (Technical Writer)

- **AG-10:** Escribe guías y documentación conceptual con snippets intercalados
- **AG-11:** Produce proyectos de ejemplo completos que se pueden ejecutar de principio a fin

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Ejemplo básico | Proyecto mínimo que demuestra el pipeline DSL→Render | `ejemplo-01-simple.md` |
| Ejemplo intermedio | Proyecto con lógica condicional, loops y datos reales | `ejemplo-02-multa.md` |
| Ejemplo avanzado/producción | Proyecto completo con integración NuGet, MAUI y Bluetooth | `ejemplo-03-multaapp-nuget.md` |
| Imágenes y assets | Proveer logos y assets necesarios para los ejemplos | `imagenes_logos/` |
| Tabla comparativa de ejemplos | Resumen de features cubiertas por cada ejemplo | Tabla en `README.md` |
| Verificación de reproducibilidad | Confirmar que los ejemplos se ejecutan en un entorno limpio | Checklist de verificación |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Aplicación de referencia | Reference Application pattern (Microsoft docs) | Demostración de integración real del motor |
| README de ejemplo | GitHub example repo conventions | Instrucciones de ejecución por ejemplo |
| Progresión de complejidad | Diataxis tutorials (learning-oriented) | Onboarding progresivo del consumidor |
| Assets reproducibles | Base64-encoded images, JSON fixtures | Autocontenidos: se ejecutan sin dependencias externas |
| Checklist de reproduciblidad | "Works on a clean machine" principle | Validación de que el ejemplo funciona en un entorno nuevo |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Reproducible | Se ejecuta en una máquina limpia con las instrucciones dadas | Test en entorno clean |
| Progresión clara | Cada ejemplo añade complejidad respecto al anterior | Tabla de features incremental |
| Resultado visible | Cada ejemplo produce un resultado que se puede ver/verificar | Output documentado con screenshot o snapshot |
| Autocontenido | No requiere servicios externos (APIs, DBs) para ejecutarse | 0 dependencias externas |
| Cross-link a guía | Cada ejemplo referencia la guía de desarrollo para contexto | ≥ 1 link a `10_developer_guide/` |
| Cobertura de renderers | Al menos 1 ejemplo por renderer (ESC/POS, texto, UI) | Tabla de cobertura |
| Assets válidos | Las imágenes Base64 son válidas y usables | Decodificación exitosa |

---

## 5. Preguntas guía para el análisis

### 5.1 Estructura de ejemplos

> **P1:** ¿Los ejemplos tienen progresión de complejidad?
>
> *Ejemplo práctico:*
> | Ejemplo | Complejidad | Features nuevas respecto al anterior |
> |---------|-------------|--------------------------------------|
> | 01 - Simple | 🟢 Básico | DSL básico, TextRenderer, consola |
> | 02 - Multa | 🟡 Medio | Datos dinámicos, condicionales, loops, ESC/POS |
> | 03 - MultaApp NuGet | 🔴 Avanzado | Integración NuGet, MAUI, Bluetooth, multi-renderer |

> **P2:** ¿Cada ejemplo tiene resultado esperado documentado?
>
> *Ejemplo práctico:*
> ```
> ## Resultado esperado (Ejemplo 01)
> 
> Al ejecutar el programa, la consola muestra:
> 
> ================================
>          MI TIENDA
> ================================
> Producto: Café
> Precio: $450.00
> ================================
> ```

### 5.2 Reproducibilidad

> **P3:** ¿Se puede ejecutar el ejemplo en 5 pasos o menos?
>
> *Ejemplo práctico:*
> ```
> 1. git clone [repo] && cd ejemplo-01
> 2. dotnet restore
> 3. dotnet run
> → Se imprime el ticket en consola
> ```

> **P4:** ¿Los prerequisitos están claros?
>
> *Ejemplo práctico:*
> ```
> ## Prerequisitos
> - .NET 10 SDK
> - (Ejemplo 03 solamente) Impresora Bluetooth ESC/POS
> - (Ejemplo 03 solamente) Visual Studio con workload MAUI
> ```

> **P5:** ¿Funciona sin dependencias externas?
>
> *Ejemplo práctico:* "El ejemplo 01 usa datos hardcodeados y TextRenderer → funciona sin internet, sin impresora, sin APIs. El ejemplo 03 puede funcionar con mock de impresora para testing."

### 5.3 Cobertura

> **P6:** ¿Los ejemplos cubren todos los renderers disponibles?
>
> *Ejemplo práctico:*
> | Renderer | Ejemplo que lo cubre | Estado |
> |----------|---------------------|--------|
> | TextRenderer | Ejemplo 01 | ✅ |
> | EscPosRenderer | Ejemplo 02 | ✅ |
> | UIRenderer (MAUI) | Ejemplo 03 | ✅ |
> | PdfRenderer | — | ❌ Pendiente (futuro) |

> **P7:** ¿Falta un ejemplo de extensibilidad?
>
> *Ejemplo práctico:* "Ejemplo 04 (propuesto): crear un renderer custom (HtmlRenderer) e integrarlo con `AddRenderer<HtmlRenderer>()`. Demuestra el punto de extensión más importante del motor."

### 5.4 Assets

> **P8:** ¿Las imágenes/logos Base64 incluidas son válidas?
>
> *Ejemplo práctico:* "Decodificar el Base64 del logo → debe producir una imagen PNG/BMP válida. Si el Base64 está truncado, la impresión del logo falla silenciosamente."

---

## 6. Plantilla base

### 6.1 README de ejemplos

```markdown
# Ejemplos

**Proyecto:** [Nombre]
**Propósito:** Aplicaciones de referencia progresivas para aprender a usar el motor.

---

## Tabla de ejemplos

| # | Nombre | Complejidad | Renderer | Features | Prerequisitos |
|---|--------|-------------|----------|----------|---------------|
| 01 | [Simple](ejemplo-01-simple.md) | 🟢 | Text | DSL básico, datos estáticos | .NET 10 |
| 02 | [Intermedio](ejemplo-02-multa.md) | 🟡 | ESC/POS | Datos dinámicos, condicionales | .NET 10 |
| 03 | [Producción](ejemplo-03-multaapp-nuget.md) | 🔴 | Multi | NuGet, MAUI, Bluetooth | .NET 10, VS + MAUI |

## Convenciones

- Cada ejemplo es autocontenido (no depende de los otros)
- Cada ejemplo incluye el resultado esperado
- Los assets (logos, datos) están embebidos para reproducibilidad
```

### 6.2 Documento de ejemplo individual

```markdown
# Ejemplo XX — [Nombre descriptivo]

**Complejidad:** [🟢 Básico / 🟡 Medio / 🔴 Avanzado]  
**Renderer:** [Text / ESC/POS / UI / Multi]  
**Prerequisitos:** [Lista]

---

## 1. Objetivo

[¿Qué demuestra este ejemplo? ¿Qué aprende el desarrollador?]

## 2. Features demostradas

| Feature | CU relacionado | Descripción |
|---------|---------------|-------------|
| [Feature 1] | CU-XX | [Descripción breve] |

## 3. Prerequisitos

- [Herramienta/SDK 1]
- [Herramienta/SDK 2]

## 4. Instrucciones paso a paso

### 4.1 Crear proyecto

`` `bash
dotnet new console -n [NombreEjemplo]
cd [NombreEjemplo]
dotnet add package MotorDsl.Engine
`` `

### 4.2 Crear template DSL

`` `json
{
  "type": "document",
  "children": [
    { "type": "text", "content": "Hola mundo", "align": "center" }
  ]
}
`` `

### 4.3 Escribir código

`` `csharp
using MotorDsl.Engine;

var services = new ServiceCollection();
services.AddMotorDslEngine();
var provider = services.BuildServiceProvider();

var engine = provider.GetRequiredService<IDocumentEngine>();
var result = engine.Render(template, data, DeviceProfile.Default58mm);

Console.WriteLine(result.ToText());
`` `

### 4.4 Ejecutar

`` `bash
dotnet run
`` `

## 5. Resultado esperado

`` `
[Output exacto que se verá en consola/pantalla]
`` `

## 6. Variaciones sugeridas

| Variación | Qué cambiar | Resultado |
|-----------|-------------|-----------|
| Cambiar ancho | `Width = 48` | Ticket más ancho |
| Agregar bold | `"bold": true` en el nodo | Texto resaltado |

## 7. Profundizar

- [Conceptos fundamentales](../10_developer_guide/conceptos-fundamentales.md)
- [Formato DSL completo](../10_developer_guide/formato-dsl-templates.md)
- [Arquitectura del motor](../05_arquitectura_tecnica/arquitectura-solucion_v1.0.md)
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Ejemplo no ejecutable | El código tiene errores o faltan dependencias | Verificar en entorno limpio antes de publicar |
| Sin resultado esperado | El dev no sabe si el ejemplo funcionó correctamente | Incluir output exacto esperado |
| Sin progresión | Todos los ejemplos tienen la misma complejidad | Diseñar escalera: básico → medio → avanzado |
| Dependencias externas | Requiere API o impresora que el dev no tiene | Usar mocks o datos embebidos |
| Sin cross-links | El ejemplo existe aislado de la documentación | Linkear a guía y arquitectura |
| Ejemplo obsoleto | El API cambió pero el ejemplo no | Compilar ejemplos en CI |
| Solo un renderer | Todos los ejemplos usan TextRenderer | Cubrir al menos ESC/POS + texto + UI |
