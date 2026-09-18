# Changelog

Cambios relevantes de las guías del repositorio. El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## 2026-09-18

### Guides/Arquitectura/Dot-NET-Arquitectura-Guide.md

#### Añadido

- Entrada **Clean Architecture** en el glosario: diagrama de anillos concéntricos, tabla de qué sabe e ignora cada anillo y nota de inversión de dependencias.
- Aclaración de que los anillos expresan **dependencia, no ubicación**: `Domain`, `Application` e `Infrastructure` son proyectos y espacios de nombres hermanos, y la cebolla se ve en sus referencias (`Infrastructure → Application → Domain`).

#### Cambiado

- Los diagramas ASCII (flujo de MediatR, visión general de la solución y otros) pasan a Mermaid.
- La interfaz del repositorio (`IProductoRepository`) vive en `Domain` en toda la guía; antes el glosario y la nota de inversión la ubicaban en `Application`, en contradicción con el árbol del proyecto `Domain`. Las interfaces de servicios técnicos (`IEmailService`, `ICurrentUserService`) quedan en `Application`.
- Convención de nombres: los términos de arquitectura, capas y patrones van en inglés y los conceptos del dominio del problema en español.
  - Anillos: `Entities (Domain)`, `Use Cases (Application)`, `Interface Adapters`, `Frameworks & Drivers`; grupos de la solución: `Presentation`, `Core`.
  - Operaciones estándar del patrón en inglés: repositorio `GetByIdAsync` / `AddAsync` / `RemoveAsync`; controller `GetAll` / `Create`; servicios de API del front `GetAllAsync` / `GetByIdAsync` / `CreateAsync` / `DeleteAsync`; factory `Producto.Create`.
  - Páginas de scaffold: `Details.razor`, `Form.razor`, `Home.razor`.
  - `ProductoListaViewModel` y `ProductoListaItemViewModel` se unifican en `ProductoListItemViewModel`, bajo `ViewModels/`.
  - Los casos de uso siguen en español porque expresan intenciones del usuario: `CrearProductoCommand`, `ObtenerProductosQuery`.

### Guides/references/arquitectura

#### Eliminado

- `guia-arquitectura-.net.md`, `guia-arquitectura-microservicios.md` y `guia-manejo-recursos.md`: eran copias idénticas de `Guides/Arquitectura/Dot-NET-Arquitectura-Guide.md`, `Microservicios-Guide.md` y `Manejo-Archivos-Guide.md`.
