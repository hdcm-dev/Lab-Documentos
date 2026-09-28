# 📚 Guía de Especialidades para Documentación SDD

**Proyecto:** Motor DSL de Generación de Documentos  
**Propósito:** Material de referencia sobre cada especialidad involucrada en el ciclo de desarrollo SDD.  
**Fecha:** 2026-04-04  
**Basado en:** [Tabla de Caracterización de Agentes](../agentes-refinamiento-docs.md)

---

## ¿Qué es esto?

Esta carpeta contiene una guía detallada por cada especialidad profesional que participa en la construcción y refinamiento de la documentación de un proyecto de software bajo el enfoque **Specification-Driven Development (SDD)**.

Cada documento incluye:

- **Definición** de la especialidad y su rol en el ciclo de desarrollo
- **Tareas principales** que desempeña
- **Especificaciones normalizadas** que produce o consume
- **Criterios de calidad** para evaluar sus entregables
- **Preguntas guía** con ejemplos prácticos para el proceso de análisis
- **Plantilla base** reutilizable para generar la documentación

---

## Índice de Especialidades

| ID | Especialidad | Archivo | Carpeta SDD que gestiona |
|----|---|---|---|
| AG-ROOT | Arquitecto de Soluciones Senior | [AG-ROOT-arquitecto-soluciones.md](AG-ROOT-arquitecto-soluciones.md) | `docs/README.md` |
| AG-00 | Product Manager / Analista de Negocio | [AG-00-product-manager.md](AG-00-product-manager.md) | `docs/00_contexto/` |
| AG-01 | Analista de Negocio Senior | [AG-01-analista-negocio.md](AG-01-analista-negocio.md) | `docs/01_necesidades_negocio/` |
| AG-02 | Analista Funcional / Ingeniero de Requisitos | [AG-02-analista-funcional.md](AG-02-analista-funcional.md) | `docs/02_especificacion_funcional/` |
| AG-03 | Especialista en Developer Experience (DX) | [AG-03-especialista-dx.md](AG-03-especialista-dx.md) | `docs/03_ux-ui/` |
| AG-04 | Ingeniero de Prompts / AI Specialist | [AG-04-ingeniero-prompts.md](AG-04-ingeniero-prompts.md) | `docs/04_prompts_ai/` |
| AG-05 | Arquitecto de Software Senior | [AG-05-arquitecto-software.md](AG-05-arquitecto-software.md) | `docs/05_arquitectura_tecnica/` |
| AG-06 | Scrum Master / Agile Coach | [AG-06-scrum-master.md](AG-06-scrum-master.md) | `docs/06_backlog-tecnico/` |
| AG-07 | Scrum Master / Gestión de Proyectos Ágiles | [AG-07-gestion-proyectos-agiles.md](AG-07-gestion-proyectos-agiles.md) | `docs/07_plan-sprint/` |
| AG-08 | Ingeniero QA / SDET Senior | [AG-08-ingeniero-qa.md](AG-08-ingeniero-qa.md) | `docs/08_calidad_y_pruebas/` |
| AG-09 | Ingeniero DevOps Senior | [AG-09-ingeniero-devops.md](AG-09-ingeniero-devops.md) | `docs/09_devops/` |
| AG-10 | Technical Writer / Developer Advocate | [AG-10-technical-writer.md](AG-10-technical-writer.md) | `docs/10_developer_guide/` |
| AG-11 | Developer Advocate / Ingeniero de Aplicación | [AG-11-developer-advocate.md](AG-11-developer-advocate.md) | `docs/11_examples/` |

---

## Flujo de lectura recomendado

```
AG-00 (Contexto estratégico)
  ↓
AG-01 (Necesidades de negocio)
  ↓
AG-02 (Especificación funcional)
  ↓
AG-03 (Experiencia de uso) + AG-04 (Prompts AI)
  ↓
AG-05 (Arquitectura técnica)
  ↓
AG-06 (Backlog) → AG-07 (Sprint planning)
  ↓
AG-08 (Calidad y pruebas)
  ↓
AG-09 (DevOps) → AG-10 (Guía de desarrollo) → AG-11 (Ejemplos)
  ↓
AG-ROOT (Integración y coherencia global)
```

---

## Cómo usar este material

1. **Antes de escribir documentación:** leer la guía de la especialidad correspondiente para entender qué se espera, qué preguntas responder y qué formato utilizar.
2. **Durante la escritura:** usar la plantilla base como punto de partida y responder las preguntas guía.
3. **Al revisar:** aplicar los criterios de calidad como checklist de validación.
4. **Al refinar:** ejecutar los sub-agentes del [prompt maestro](../agentes-refinamiento-docs.md) para identificar gaps.
