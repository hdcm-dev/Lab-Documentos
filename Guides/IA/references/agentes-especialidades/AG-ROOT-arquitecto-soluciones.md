# AG-ROOT — Arquitecto de Soluciones Senior

**Carpeta SDD:** `docs/README.md`  
**Perfil Profesional:** Visión integral del sistema, comunicación técnica ejecutiva

---

## 1. ¿Qué es esta especialidad?

El **Arquitecto de Soluciones Senior** es el profesional responsable de garantizar la **coherencia integral** de un proyecto de software desde una perspectiva sistémica. No se enfoca en un componente aislado sino en cómo todas las partes del sistema (código, documentación, procesos, infraestructura) se alinean para satisfacer los objetivos del negocio.

En el contexto de documentación SDD, este rol actúa como **integrador transversal**: verifica que el README principal del proyecto sea un punto de entrada efectivo, que los documentos no se contradigan entre sí, y que un desarrollador nuevo pueda entender el proyecto rápidamente.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según el nivel de alcance y el tipo de organización:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **Arquitectura de Soluciones** | Coherencia integral de UN proyecto/producto (docs, código, infra, procesos) | Siempre que un proyecto tenga múltiples capas que coordinar | README, árbol documental, mapa de trazabilidad cross-doc |
| **Arquitectura Empresarial** | Alineación IT ↔ estrategia de negocio a nivel ORGANIZACIONAL (múltiples sistemas) | Organizaciones con múltiples proyectos que deben interoperar | TOGAF ADM, EA repository, mapas de capacidad, landscape de aplicaciones |
| **Arquitectura de Integración** | Flujos de datos y comunicación ENTRE sistemas (APIs, eventos, ETL) | Ecosistemas con múltiples servicios, legacy + nuevo, multi-vendor | Diagramas de integración, contratos entre sistemas, mapping de datos, API gateway config |

> **En este proyecto** se aplica **Arquitectura de Soluciones** porque el producto es un proyecto único (Motor DSL) con múltiples capas documentales (SDD) que requieren coherencia integral.

#### Ejemplos temáticos por disciplina

**Arquitectura Empresarial** *(aplica en otra escala)*
- *Ejemplo:* Un banco necesita alinear 50 sistemas (core banking, CRM, canales digitales, legacy COBOL) con la estrategia de transformación digital. El arquitecto empresarial define el landscape de aplicaciones, identifica redundancias y planifica la migración usando TOGAF.

**Arquitectura de Integración** *(aplica en ecosistemas multi-sistema)*
- *Ejemplo:* Una empresa de logística necesita conectar su ERP (SAP), su WMS, la API de la transportadora y un portal web de tracking. El arquitecto de integración diseña los flujos event-driven (Kafka), define contratos OpenAPI entre servicios y establece el patrón de retry/fallback.

### Diferencia con el Arquitecto de Software (AG-05)

- El **Arquitecto de Soluciones** mira la solución completa (negocio + técnica + procesos + infraestructura)
- El **Arquitecto de Software** se enfoca en el diseño técnico interno (componentes, patrones, interfaces)

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Diseño de estructura documental | Definir la organización de carpetas y la jerarquía de documentos | Árbol de carpetas con descripciones |
| Validación de coherencia cross-documental | Verificar que la información no se contradice entre documentos | Informe de inconsistencias |
| Narrativa técnica ejecutiva | Redactar el README que presenta el proyecto completo | README.md del proyecto |
| Onboarding de desarrolladores | Diseñar la ruta de lectura para nuevos integrantes | Guía de primer viaje |
| Validación de links internos | Verificar que todos los links apuntan a archivos existentes | Checklist de links |
| Alineación estratégica-técnica | Asegurar que la documentación técnica refleja la visión de producto | Reporte de alineación |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Documento README del proyecto | Convención GitHub/GitLab README | Punto de entrada para onboarding, CI/CD badges, links a documentación |
| Árbol de estructura documental | IEEE 1016 (Software Design Description) | Marco organizativo que estructura toda la documentación SDD |
| Mapa de trazabilidad cross-documental | IEEE 830 (Trazabilidad de requisitos) | Valida completitud vertical del proyecto |
| Guía de navegación | Práctica interna | Reduce tiempo de onboarding, mejora productividad del equipo |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Tiempo de comprensión | Un desarrollador nuevo entiende el proyecto leyendo el README | ≤ 5 minutos de lectura |
| Links funcionales | Todos los links internos apuntan a archivos existentes | 0 links rotos |
| Coherencia con disco | El árbol de carpetas descrito coincide con la estructura real | Diff entre doc y `tree` |
| Cobertura de carpetas | Todas las carpetas del proyecto están documentadas | 100% de carpetas cubiertas |
| Narrativa no-técnica | Un stakeholder no-técnico entiende el propósito del proyecto | Revisión por par no-técnico |

---

## 5. Preguntas guía para el análisis

Estas preguntas deben responderse al construir o revisar la documentación:

### 5.1 Estructura y navegación

> **P1:** ¿El README describe todas las carpetas que existen en el disco?
> 
> *Ejemplo práctico:* Si el proyecto tiene `docs/09_devops/` pero el README no la menciona, hay un gap. Ejecutar `tree docs/` y comparar con el README.

> **P2:** ¿Un desarrollador que llega por primera vez sabe por dónde empezar a leer?
>
> *Ejemplo práctico:* El README debe tener una sección "Primer viaje del desarrollador" con 5-6 pasos numerados que guíen la lectura secuencial.

### 5.2 Links y referencias

> **P3:** ¿Todos los links internos apuntan a archivos que existen?
>
> *Ejemplo práctico:* Si el README dice `ver [arquitectura](docs/05_arquitectura_tecnica/arquitectura-solucion_v1.0.md)` pero el archivo fue renombrado, el link está roto.

> **P4:** ¿Los paths son links clickeables o solo texto plano?
>
> *Ejemplo práctico:* ❌ `Revisar docs/05_arquitectura_tecnica/` → ✅ `Revisar [arquitectura](docs/05_arquitectura_tecnica/)`

### 5.3 Coherencia

> **P5:** ¿La visión del producto descrita en el README coincide con `docs/00_contexto/vision-producto_v1.0.md`?
>
> *Ejemplo práctico:* Si el README dice "motor de impresión" pero la visión dice "motor de generación de documentos multiplataforma", hay inconsistencia.

> **P6:** ¿El roadmap del README está alineado con el roadmap detallado?
>
> *Ejemplo práctico:* El README no debe inventar su propio roadmap. Debe linkear al archivo `roadmap-producto_v1.0.md`.

---

## 6. Plantilla base

```markdown
# [Nombre del Proyecto]

**Descripción corta:** [Una línea que explica qué es y para qué sirve]  
**Stack:** [Tecnologías principales]  
**Estado:** [En desarrollo / Pre-release / Producción]

---

## ¿Qué es [Nombre]?

[2-3 párrafos explicando el propósito, la audiencia y el valor del proyecto. 
Debe ser comprensible para alguien sin contexto previo.]

---

## Estructura del Repositorio

```text
/
├── docs/                   # Documentación SDD completa
│   ├── 00_contexto/        # Visión, alcance, roadmap
│   ├── 01_necesidades/     # Necesidades de negocio
│   ├── ...                 # [Describir cada carpeta]
├── src/                    # Código fuente
├── tests/                  # Tests automatizados
└── devs/                   # Material de desarrollo interno
`` `

---

## Documentación

| Carpeta | Contenido | Lectura |
|---------|-----------|---------|
| [00_contexto](docs/00_contexto/) | Visión, alcance, roadmap | 1° |
| [01_necesidades](docs/01_necesidades_negocio/) | Necesidades de negocio | 2° |
| ... | ... | ... |

---

## Primer Viaje del Desarrollador

1. Leer la [visión del producto](docs/00_contexto/vision-producto_v1.0.md)
2. Entender las [necesidades de negocio](docs/01_necesidades_negocio/necesidades-negocio_v1.0.md)
3. Revisar la [arquitectura](docs/05_arquitectura_tecnica/arquitectura-solucion_v1.0.md)
4. Consultar la [guía de integración](docs/10_developer_guide/)
5. Ejecutar un [ejemplo](docs/11_examples/)

---

## Definición de Terminado

Aplica la [Definition of Done canónica](docs/08_calidad_y_pruebas/definition-of-done_v1.0.md).

---

## Roadmap

Ver [roadmap detallado](docs/00_contexto/roadmap-producto_v1.0.md).
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| README como wiki | Demasiada información, nadie lee | README conciso + links a docs detallados |
| Links como texto plano | No se pueden clickear, difícil navegar | Usar links Markdown `[texto](ruta)` |
| Árbol de carpetas desactualizado | Confunde al lector, genera desconfianza | Sincronizar con `tree` del disco |
| Roadmap duplicado | README y docs/00_contexto dicen diferente | Un solo roadmap canónico, README linkea |
| Sin ruta de lectura | El desarrollador nuevo se pierde | Sección "Primer viaje" con 5 pasos |
