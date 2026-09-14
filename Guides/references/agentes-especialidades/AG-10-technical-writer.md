# AG-10 — Technical Writer / Developer Advocate

**Carpeta SDD:** `docs/10_developer_guide/`  
**Perfil Profesional:** Documentación técnica orientada al consumidor

---

## 1. ¿Qué es esta especialidad?

El **Technical Writer** (o **Developer Advocate** en su faceta de documentación) es el profesional que crea documentación técnica orientada al **consumidor** de la librería. Su audiencia no es el equipo que construye el motor, sino el desarrollador externo que quiere integrarlo en su aplicación.

La diferencia clave con el resto de los roles es la perspectiva: mientras el Arquitecto documenta *cómo está construido* el motor, el Technical Writer documenta *cómo usarlo*. Escribe guías de integración, tutoriales paso a paso, referencia de API, troubleshooting y FAQs.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según la audiencia y el tipo de documentación:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Documentación Técnica de Software (Developer Docs)** | Crear guías, tutoriales y referencia para DESARROLLADORES que integran una librería/API/SDK | Librerías, SDKs, motores, frameworks consumidos por otros devs | Developer guide, quickstart, tutoriales Diataxis, troubleshooting, changelog |
| **Documentación de Producto (End-User Docs)** | Crear manuales, help y knowledge base para USUARIOS FINALES no-técnicos | Aplicaciones con usuarios finales (SaaS, apps móviles, desktop) | Manuales de usuario, tooltips, help center, knowledge base, videos tutoriales, onboarding guides |
| **Documentación de API (API Documentation)** | Generar referencia completa y precisa de endpoints, parámetros, respuestas y errores de APIs | APIs públicas (REST, GraphQL, gRPC) consumidas por terceros | OpenAPI/Swagger spec, API reference autogenerada (DocFX, Redoc), Postman collections, sandbox interactivo |

> **En este proyecto** se aplica **Documentación Técnica de Software** porque la audiencia son desarrolladores .NET que quieren integrar el motor DSL en sus aplicaciones, no usuarios finales.

#### Ejemplos temáticos por disciplina

**Documentación de Producto (End-User)** *(aplica en apps con usuario final)*
- *Ejemplo:* Un SaaS de gestión de proyectos (tipo Jira/Trello) necesita un Help Center. El technical writer crea artículos categorizados (Getting Started, Projects, Boards, Reports), escribe guías paso a paso con screenshots anotados ("Cómo crear tu primer proyecto"), diseña un flujo de onboarding in-app con tooltips progresivos, y mantiene una knowledge base searchable con FAQ y respuestas a tickets de soporte recurrentes.

**Documentación de API** *(aplica en APIs públicas)*
- *Ejemplo:* Una pasarela de pagos (tipo Mercado Pago/Stripe) publica su API REST para integradores. El API writer mantiene la spec OpenAPI 3.0 como source of truth, genera la referencia interactiva con Redoc (cada endpoint con sandbox "Try it"), crea Postman Collections con ejemplos para los 5 flujos principales (crear pago, consultar estado, reembolso, webhook, suscripción), y documenta cada error code con cause + resolution + curl de ejemplo.

### El "test del desarrollador nuevo"

El criterio fundamental de esta especialidad: ¿puede un desarrollador que nunca vio el proyecto hacer funcionar el motor siguiendo solo esta guía, sin ayuda externa?

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Conceptos fundamentales | Explicar el modelo mental del motor (DSL, pipeline, renderers) | `conceptos-fundamentales.md` |
| Formato de templates DSL | Documentar la gramática DSL con ejemplos copiables | `formato-dsl-templates.md` |
| Formato de perfiles de impresora | Documentar la configuración de DeviceProfile | `formato-perfiles-impresora.md` |
| Guía de integración MAUI | Tutorial paso a paso para integrar el motor en una app MAUI | `guia-integracion-maui.md` |
| Integración API REST | Documentar endpoints si hay un servicio API | `integracion-api-rest.md` |
| Perfiles de impresoras reales | Catálogo de configuraciones para impresoras comunes | `perfiles-impresoras-reales.md` |
| Soporte de imágenes | Documentar cómo incluir logos/imágenes en impresión térmica | `soporte-imagenes-termicas.md` |
| Templates de ejemplo | Proveer archivos JSON de plantilla copiables | `ejemplos-templates/*.json` |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Guía de inicio rápido (Quickstart) | Google Developer Documentation Style Guide | Primera experiencia del consumidor |
| Referencia de API | XML docs / DocFX / OpenAPI spec | Documentación autogenerada desde código |
| Tutorial paso a paso | Diataxis framework (Tutorial quadrant) | Aprendizaje guiado |
| Guía How-to | Diataxis framework (How-to quadrant) | Resolver problemas específicos |
| Troubleshooting / FAQ | Soporte de primer nivel | Reducción de issues/preguntas repetitivas |
| Templates de ejemplo | Copy-paste-ready patterns | Aceleración de onboarding |

### El framework Diataxis

Los 4 tipos de documentación técnica:

| Tipo | Propósito | Orientación |
|------|-----------|-------------|
| Tutorial | Enseñar haciendo | Aprendizaje |
| How-to guide | Resolver un problema | Objetivo concreto |
| Reference | Describir la maquinaria | Información |
| Explanation | Dar contexto/razonamiento | Comprensión |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Copiable | Los snippets de código compilan al hacer copy-paste | 0 snippets rotos |
| Progresión lógica | Va de simple a complejo (conceptos → formato → integración → avanzado) | Revisión de tabla de contenidos |
| Prerequisitos claros | Se indican: versión de .NET, paquetes NuGet, herramientas | Sección de prerequisitos presente |
| Auto-contenida | No requiere leer la documentación interna del equipo | Test del "dev nuevo" |
| Cross-links | Links a arquitectura para quien quiera profundizar | ≥ 3 links a `05_arquitectura_tecnica/` |
| Troubleshooting | Errores comunes con solución | Sección de troubleshooting presente |
| Templates JSON válidos | Los archivos .json de ejemplo son válidos sintácticamente | `jsonlint` pasa |

---

## 5. Preguntas guía para el análisis

### 5.1 Onboarding

> **P1:** ¿Un desarrollador puede hacer funcionar el motor en 15 minutos?
>
> *Ejemplo práctico:*
> ```
> 1. Crear proyecto: dotnet new console -n MiApp
> 2. Instalar paquete: dotnet add package MotorDsl.Engine
> 3. Registrar servicios: services.AddMotorDslEngine()
> 4. Crear template DSL (copiar del ejemplo)
> 5. Ejecutar: engine.Render(template, data, profile)
> 6. Ver resultado en consola
> ```

> **P2:** ¿Los prerequisitos están completos y actualizados?
>
> *Ejemplo práctico:*
> - .NET 10 SDK
> - Visual Studio 2022 17.x o VS Code + C# Dev Kit
> - Paquete NuGet: `MotorDsl.Engine` v0.3.0+
> - (Opcional) Impresora Bluetooth ESC/POS para tests de impresión

### 5.2 Contenido

> **P3:** ¿Hay progresión de complejidad?
>
> *Ejemplo práctico:*
> | Orden | Documento | Complejidad | Prerequisito |
> |-------|-----------|-------------|--------------|
> | 1 | conceptos-fundamentales | 🟢 Básico | Ninguno |
> | 2 | formato-dsl-templates | 🟢 Básico | conceptos |
> | 3 | guia-integracion-maui | 🟡 Medio | formato-dsl |
> | 4 | soporte-imagenes-termicas | 🔴 Avanzado | integración |

> **P4:** ¿Los ejemplos de código usan la API real del motor (no pseudo-código)?
>
> *Ejemplo práctico:*
> ```csharp
> // ✅ API real
> var engine = serviceProvider.GetRequiredService<IDocumentEngine>();
> var result = engine.Render(template, data, profile);
> Console.WriteLine(result.ToText());
> 
> // ❌ Pseudo-código
> var resultado = motor.Procesar(plantilla);
> ```

> **P5:** ¿Hay una sección de troubleshooting?
>
> *Ejemplo práctico:*
> | Síntoma | Causa probable | Solución |
> |---------|---------------|----------|
> | `DslParseException` en template válido | Encoding UTF-8 con BOM | Guardar archivo como UTF-8 sin BOM |
> | Resultado vacío al renderizar | Datos no matchean variables del template | Verificar nombres exactos en `data` |
> | Bluetooth timeout | Impresora en modo sleep | Despertar impresora antes de enviar |

---

## 6. Plantilla base

### 6.1 Guía de desarrollo (Developer Guide README)

```markdown
# Developer Guide

**Proyecto:** [Nombre]  
**Audiencia:** Desarrolladores que integran el motor en sus aplicaciones

---

## Prerequisitos

- [.NET version]
- [IDE recomendado]
- [Paquete NuGet]

## Guía de lectura

| Orden | Documento | Nivel |
|-------|-----------|-------|
| 1 | [Conceptos fundamentales](conceptos-fundamentales.md) | Básico |
| 2 | [Formato DSL](formato-dsl-templates.md) | Básico |
| 3 | [Integración MAUI](guia-integracion-maui.md) | Medio |
| 4 | [Perfiles de impresoras](perfiles-impresoras-reales.md) | Medio |
| 5 | [Soporte de imágenes](soporte-imagenes-termicas.md) | Avanzado |

## Inicio rápido

`` `csharp
// 1. Registrar servicios
services.AddMotorDslEngine();

// 2. Preparar datos
var template = "document { text \"Hola mundo\" }";
var data = new Dictionary<string, object>();
var profile = DeviceProfile.Default58mm;

// 3. Renderizar
var engine = provider.GetRequiredService<IDocumentEngine>();
var result = engine.Render(template, data, profile);

// 4. Usar resultado
Console.WriteLine(result.ToText());
`` `

## Troubleshooting

| Síntoma | Causa | Solución |
|---------|-------|----------|
| [Síntoma 1] | [Causa] | [Solución] |
```

### 6.2 Documento conceptual

```markdown
# [Título del concepto]

**Nivel:** [Básico / Medio / Avanzado]  
**Prerequisitos:** [Documentos que se deben leer antes]

---

## ¿Qué es [concepto]?

[Explicación clara en 2-3 párrafos. Sin jerga innecesaria.]

## ¿Para qué sirve?

[Casos de uso concretos]

## Diagrama

`` `text
[Diagrama ASCII o Mermaid que ilustre el concepto]
`` `

## Ejemplo práctico

`` `csharp
[Código que demuestra el concepto en acción]
`` `

## Relación con otros conceptos

- **[Concepto X]:** [Cómo se relaciona]
- **Profundizar:** ver [documento de arquitectura](../05_arquitectura_tecnica/...)
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Documentar para el equipo | El dev externo no entiende la jerga interna | Escribir asumiendo 0 contexto previo |
| Pseudo-código | `motor.procesar(plantilla)` → no es la API real | Usar la interfaz real: `engine.Render(template, data, profile)` |
| Sin prerequisitos | El dev no sabe qué instalar | Sección de prerequisitos al inicio |
| Solo texto, sin código | Mucha explicación, ningún ejemplo ejecutable | ≥ 1 snippet por concepto |
| Sin troubleshooting | El dev se queda trabado sin saber qué hacer | Tabla síntoma/causa/solución con problemas reales |
| Docs desactualizados | La API cambió pero la guía no | Verificar snippets en CI (compile check) |
