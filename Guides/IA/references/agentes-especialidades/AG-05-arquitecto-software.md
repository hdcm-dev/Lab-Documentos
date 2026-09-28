# AG-05 — Arquitecto de Software Senior

**Carpeta SDD:** `docs/05_arquitectura_tecnica/`  
**Perfil Profesional:** Diseño de sistemas, patrones, decisiones técnicas

---

## 1. ¿Qué es esta especialidad?

El **Arquitecto de Software** es el profesional que diseña la estructura interna del sistema: cómo se organizan los componentes, cómo se comunican, qué patrones utilizan y qué decisiones técnicas se toman (y por qué). Su trabajo traduce los requisitos funcionales y no-funcionales en un diseño técnico implementable.

En el contexto SDD, el Arquitecto de Software produce los documentos que guían la implementación: la **arquitectura de la solución** (componentes, pipeline, flujo), los **ADRs** (Architecture Decision Records), los **contratos** (interfaces públicas), el **modelo de extensibilidad** y los **diagramas C4**.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según la escala del sistema y el nivel de responsabilidad:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Arquitectura de Software** | Diseñar la estructura interna de UN sistema: componentes, capas, patrones, interfaces, pipeline | Todo proyecto de software que requiera diseño estructural | ADRs, diagramas C4, contratos de interfaces, modelo de extensibilidad, flujo de ejecución |
| **Diseño de Sistemas Distribuidos** | Diseñar sistemas que operan en múltiples nodos: microservicios, mensajería, consistencia, resiliencia | Sistemas cloud-native, plataformas de alta escala, arquitecturas event-driven | Diagramas de topología, contratos de API inter-servicio, esquemas de consistencia eventual, circuit breakers, partitioning strategy |
| **Liderazgo Técnico (Tech Lead)** | Guiar al equipo en decisiones de stack, estándares de código, code review y deuda técnica | Equipos de desarrollo donde alguien define dirección técnica además de diseñar | Coding standards, guías de contribución, tech radar del equipo, decisiones de build-vs-buy, mentoring técnico |

> **En este proyecto** se aplica **Arquitectura de Software** porque el motor DSL es un sistema monolítico modular (pipeline) con componentes internos bien definidos, sin distribución ni microservicios.

#### Ejemplos temáticos por disciplina

**Diseño de Sistemas Distribuidos** *(aplica en plataformas cloud-native)*
- *Ejemplo:* Una plataforma de streaming de video (tipo Netflix) necesita servir 10M de usuarios concurrentes. El arquitecto diseña microservicios independientes (Catalog, Streaming, Recommendations, Billing), define la comunicación async via Kafka para eventos de viewing, implementa circuit breakers con Polly entre servicios, establece la estrategia de consistencia eventual (SAGA pattern para suscripciones) y diseña el CDN geo-distribuido para latencia <100ms globalmente.

**Liderazgo Técnico** *(aplica como rol de guía del equipo)*
- *Ejemplo:* Un tech lead de un equipo de 8 desarrolladores define el coding standard (C# conventions, async/await patterns, naming), conduce las code reviews con focus en mantenibilidad, gestiona el tech radar del equipo (qué frameworks adoptar/evaluar/retirar), facilita las decisiones build-vs-buy (¿usar Hangfire o implementar job queue propio?) y hace mentoring técnico con los desarrolladores júnior.

### Diferencia con el Arquitecto de Soluciones (AG-ROOT)

- El **Arquitecto de Soluciones** mira el proyecto completo (docs + código + infraestructura + procesos)
- El **Arquitecto de Software** se enfoca en el diseño técnico interno (componentes, DI, interfaces, pipeline)

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Diseño de arquitectura | Definir componentes, capas y pipeline del motor | `arquitectura-solucion_v1.0.md` |
| Documentación de decisiones (ADR) | Registrar cada decisión técnica con contexto, alternativas y consecuencias | `decisiones-arquitectura_v1.0.md` |
| Definición de contratos | Especificar interfaces públicas del motor (inputs, outputs, tipos) | `contratos-del-motor_v1.0.md` |
| Modelo de extensibilidad | Documentar puntos de extensión (nuevos renderers, nodos, proveedores) | `extensibilidad-motor_v1.0.md` |
| Flujo de ejecución | Describir el pipeline paso a paso con transformaciones de datos | `flujo-ejecucion-motor_v1.0.md` |
| Modelo de datos lógico | Definir las estructuras de datos internas del motor | `modelo-datos-logico_v1.0.md` |
| Mapeo componente → proyecto | Vincular componentes lógicos a proyectos .NET concretos | Tabla en `arquitectura-solucion_v1.0.md` |
| Diagramas C4 | Crear diagramas Context, Container y Component | Diagramas en Mermaid o PlantUML |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Arquitectura de solución | IEEE 1016 (Software Design Description) / ISO 42010 | Guía de implementación para todo el equipo de desarrollo |
| ADR (Architecture Decision Records) | ADR format de Michael Nygard | Registro de trade-offs para mantenibilidad futura |
| Contratos/Interfaces | .NET Interface conventions / OpenAPI spec | Input para implementación de código y tests de contrato |
| Modelo C4 | Simon Brown C4 Model (Context, Container, Component, Code) | Comunicación visual de la arquitectura |
| Modelo de datos lógico | UML Class Diagram / ERD | Input para implementación de clases y DTOs |
| Guía de extensibilidad | Plugin/Extension Point patterns | Input para la guía del desarrollador y tests de extensibilidad |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| ADRs completos | Cada decisión tiene: Contexto, Decisión, Alternativas evaluadas, Consecuencias | Checklist de campos por ADR |
| Contratos completos | Todas las interfaces públicas documentadas con parámetros y retornos | 0 interfaces sin documentación |
| Consistencia CU → componentes | Los componentes documentados cubren todos los pasos del flujo en los CU | Mapa de trazabilidad CU → componente |
| Extensibilidad con ejemplos | Cada punto de extensión tiene un ejemplo de cómo agregar un nuevo elemento | ≥ 1 ejemplo por punto de extensión |
| Diagramas C4 presentes | Al menos Level 1 (Context) y Level 2 (Container) | 2+ diagramas |
| Mapeo a .NET | Cada componente lógico tiene un namespace y .csproj correspondiente | Tabla completa |
| Bajo acoplamiento | Las dependencias entre componentes son explícitas y unidireccionales | Grafo de dependencias sin ciclos |

---

## 5. Preguntas guía para el análisis

### 5.1 Estilo arquitectónico

> **P1:** ¿Qué estilo arquitectónico se usa y por qué?
>
> *Ejemplo práctico:* "Pipeline modular: porque el proceso de renderización es una cadena de transformaciones (DSL → AST → Modelo → Layout → Output) donde cada etapa es independiente y testeable."

> **P2:** ¿Cuáles son los componentes principales y qué responsabilidad tiene cada uno?
>
> *Ejemplo práctico:*
> | Componente | Responsabilidad (1 oración) |
> |---|---|
> | Parser DSL | Transforma texto DSL en árbol AST |
> | Evaluator | Resuelve variables, condiciones e iteraciones |
> | LayoutEngine | Calcula distribución espacial según perfil de dispositivo |
> | Renderer | Convierte modelo abstracto en salida específica (ESC/POS, texto, UI) |

### 5.2 Decisiones arquitectónicas

> **P3:** ¿Qué alternativas se evaluaron para cada decisión y por qué se eligió esta?
>
> *Ejemplo práctico:*
> | Decisión | Alternativa elegida | Alternativas descartadas | Motivo |
> |---|---|---|---|
> | Formato DSL | JSON declarativo | XML (verboso), YAML (indentación frágil), DSL custom (costo de parser) | JSON es universal, parseable nativamente en .NET, familiar para la mayoría de devs |

> **P4:** ¿Qué consecuencias tiene cada decisión?
>
> *Ejemplo práctico:* "Elegir JSON como DSL → ✅ Tooling existente, ✅ Validación con JSON Schema. ❌ Menos expresivo que un DSL custom, ❌ Verboso para documentos complejos."

### 5.3 Contratos

> **P5:** ¿Cuál es la interfaz pública principal del motor?
>
> *Ejemplo práctico:*
> ```csharp
> public interface IDocumentEngine
> {
>     RenderResult Render(string dslTemplate, Dictionary<string, object> data, DeviceProfile profile);
> }
> ```

> **P6:** ¿Los contratos son estables o están sujetos a cambios?
>
> *Ejemplo práctico:* "IDocumentEngine es estable (v1.0). IRenderer es extensible (se pueden agregar implementaciones sin cambiar la interfaz)."

### 5.4 Extensibilidad

> **P7:** ¿Cómo agrega un desarrollador un nuevo renderizador?
>
> *Ejemplo práctico:*
> ```csharp
> public class PdfRenderer : IRenderer
> {
>     public string Target => "pdf";
>     public RenderResult Render(LayoutedDocument document) { ... }
> }
> 
> // Registro
> services.AddMotorDslEngine()
>     .AddRenderer<PdfRenderer>();
> ```

---

## 6. Plantilla base

### 6.1 Arquitectura de solución

```markdown
# Arquitectura de Solución

**Proyecto:** [Nombre]  
**Versión:** 1.0

---

## 1. Propósito

[¿Qué documenta este archivo?]

## 2. Estilo arquitectónico

[Pipeline / Layers / Microkernel / etc. + justificación]

## 3. Diagrama de alto nivel

`` `text
[Diagrama ASCII o referencia a Mermaid]
`` `

## 4. Componentes principales

### 4.1 [Componente 1]

**Responsabilidad:** [1 oración]  
**Entradas:** [Tipos]  
**Salidas:** [Tipos]  
**Dependencias:** [Otros componentes]

### 4.2 [Componente 2]

[Misma estructura]

## 5. Mapeo Componentes → Proyectos .NET

| Componente | Namespace | Proyecto (.csproj) | Responsabilidad |
|---|---|---|---|
| [Comp 1] | [Namespace] | [Proyecto] | [1 oración] |

## 6. Flujo principal

`` `text
[Entrada] → [Comp 1] → [Comp 2] → ... → [Salida]
`` `

## 7. Decisiones técnicas clave

| Decisión | Elección | Motivo |
|----------|----------|--------|
| [Decisión 1] | [Opción elegida] | [Justificación] |
```

### 6.2 ADR (Architecture Decision Record)

```markdown
# ADR-XXX — [Título de la decisión]

**Estado:** [Propuesta / Aceptada / Deprecada / Reemplazada por ADR-YYY]  
**Fecha:** [YYYY-MM-DD]  
**Autores:** [Nombres]

---

## Contexto

[¿Qué problema se necesita resolver? ¿Qué restricciones existen?]

## Decisión

[¿Qué se decidió hacer?]

## Alternativas evaluadas

| Alternativa | Pros | Contras |
|-------------|------|---------|
| [Opción A] | [Ventajas] | [Desventajas] |
| [Opción B] | [Ventajas] | [Desventajas] |

## Consecuencias

### Positivas
- [Consecuencia positiva 1]

### Negativas
- [Consecuencia negativa 1]

### Riesgos
- [Riesgo 1 + mitigación]
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Decisiones implícitas | Se eligió JSON pero nadie sabe por qué | Documentar como ADR con contexto y alternativas |
| Arquitectura en la cabeza | Solo el arquitecto sabe cómo funciona | Diagramas C4 + mapeo a .csproj |
| Contratos incompletos | La interfaz está en código pero no documentada | Documentar cada método público con tipos y ejemplos |
| Sin puntos de extensión | El motor es monolítico → no se puede agregar renderers | Diseñar interfaces + registro DI |
| ASCII art ambiguo | Diagrama con flechas que no se entienden | Usar Mermaid para diagramas renderizables |
| Componente = clase | Confundir componente arquitectónico con una clase .NET | Componente = módulo con responsabilidad cohesiva |
