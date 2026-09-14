---
doc_id: ANEXO-GLOSARIO
doc_type: anexo
title: Glosario
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [MARCO-CONVENCIONES, ANEXO-REFERENCIAS, TEM-QUE-ES, TEM-ESTANDARES, TEM-RNF]
---

# Glosario — `ANEXO-GLOSARIO`

## Resumen ejecutivo

Registro del vocabulario canónico de la guía: un término, una definición, y los sinónimos o formas en inglés que se registran como alias. Cuando dos documentos nombran lo mismo, usan el término de esta tabla; cuando el término tiene una designación establecida en inglés, se conserva en inglés y se apunta acá su equivalente español, según fija [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md#idioma). La columna de fuente remite a [`ANEXO-REFERENCIAS`](Referencias.md) cuando el término tiene definición normativa; su ausencia indica que es uso de la guía.

---

## Términos del informe y su marco

| Término canónico | Definición | Alias / inglés | Fuente |
|---|---|---|---|
| Informe de solución | Documento transversal que describe una solución de software en términos de arquitectura, despliegue y resolución de requisitos, para un lector que quiere comprender el enfoque general | Solution description, solution overview | Uso de la guía |
| Descripción de arquitectura | Producto de trabajo que expresa la arquitectura de una entidad, distinto de la arquitectura misma | Architecture description (AD) | `N-01` 42010 |
| Arquitectura | Conceptos y propiedades fundamentales de un sistema en su entorno, encarnados en sus elementos, relaciones y principios de diseño | Architecture | `N-01` 42010 |
| Solución (arquitectura de solución) | Descripción de una operación de negocio focalizada y de cómo IS/IT la soporta; típicamente un proyecto o entrega | Solution architecture | `G-03` TOGAF |
| Stakeholder | Individuo, grupo u organización con un interés en el sistema | Interesado, parte interesada | `N-01` 42010 |
| Concern | Interés de un stakeholder relativo al sistema —lo que le preocupa o le importa | Incumbencia | `N-01` 42010 |
| Viewpoint (punto de vista) | Convención sobre cómo construir y usar una vista para atender ciertos concerns | Architecture viewpoint | `N-01` 42010 |
| View (vista) | Expresión de la arquitectura desde la perspectiva de concerns específicos | Architecture view | `N-01` 42010 |
| Modelo 4+1 | Modelo que describe la arquitectura por cinco vistas: lógica, de proceso, de desarrollo, física y escenarios | 4+1 view model | `O-01` Kruchten |
| Vista de despliegue | Vista que mapea el software al hardware: qué corre en qué nodo | Deployment view, physical view | `O-01`, `G-01` §7 |
| Nodo | Recurso donde se despliega y ejecuta un artefacto; puede ser un dispositivo o un entorno de ejecución | Node (device / execution environment) | `N-07` UML |
| Artefacto | Pieza física y desplegable de información —un ejecutable, un paquete— que se despliega sobre un nodo | Artifact | `N-07` UML |
| Container (C4) | Aplicación o almacén de datos que necesita estar en ejecución para que el sistema funcione; unidad desplegable por separado | Container | `G-02` C4 |
| Decisión de arquitectura | Elección estructural registrada con su justificación (rationale) y sus alternativas | Architecture decision | `N-01` 42010 |
| Rationale | Justificación de una decisión de arquitectura: por qué se eligió, contra qué | Justificación | `N-01` 42010 |
| ADR | Registro breve y autónomo de una decisión de arquitectura, con contexto, decisión y consecuencias | Architecture Decision Record | `G-04`, guía hermana |
| Trazabilidad | Correspondencia explícita entre un requisito y el componente o mecanismo que lo resuelve | Traceability | `N-01`, `N-06` |

---

## Requisitos y calidad

| Término canónico | Definición | Alias / inglés | Fuente |
|---|---|---|---|
| Requisito funcional | Requisito que especifica qué debe hacer el sistema: una función, un comportamiento, un servicio | Functional requirement (RF) | `N-06` 29148 |
| Requisito no funcional | Requisito sobre cuán bien el sistema hace algo, o una restricción sobre él: rendimiento, fiabilidad, seguridad | Non-functional requirement (RNF, NFR) | `N-06` 29148 |
| Atributo de calidad | Propiedad medible que un requisito no funcional persigue; su catálogo de referencia es el modelo de calidad de producto | Quality attribute | `N-04` 25010:2023 |
| Modelo de calidad de producto | Modelo de nueve características de calidad y sus subcaracterísticas | Product quality model | `N-04` 25010:2023 |
| Interaction Capability | Característica de calidad de `N-04` que reemplazó a «Usability» en la edición 2023 | Usabilidad (obsoleto) | `N-04` 25010:2023 |
| Flexibility | Característica de calidad de `N-04` que reemplazó a «Portability» en 2023; incluye la nueva subcaracterística scalability | Portabilidad (obsoleto) | `N-04` 25010:2023 |
| Safety | Característica de calidad agregada en `N-04`:2023; subcaracterísticas: operational constraint, risk identification, fail safe, hazard warning, safe integration | Seguridad de operación | `N-04` 25010:2023 |
| Recoverability | Subcaracterística de Reliability: capacidad de recuperar el estado tras una interrupción | Recuperabilidad | `N-04` 25010:2023 |
| SRS | Especificación de requisitos de software; separa funciones de atributos del sistema | Software Requirements Specification | `N-06` 29148 |

---

## Despliegue y .NET

| Término canónico | Definición | Alias / inglés | Fuente |
|---|---|---|---|
| Despliegue | Proceso de llevar una versión del software a un entorno donde se ejecuta | Deployment | Uso de la guía |
| Entorno | Configuración de infraestructura donde corre el sistema: desarrollo, prueba, producción | Environment | Uso de la guía |
| Operación degradada | Comportamiento del sistema cuando parte de él está incomunicada o caída, conservando función | Degraded / offline operation, offline-first | Uso de la guía |
| Despliegue dependiente del framework | Publicación que no incluye el runtime de .NET; el host debe tenerlo instalado | Framework-dependent deployment (FDD) | `N-09` |
| Despliegue autocontenido | Publicación que incluye el runtime de .NET; el host no lo necesita preinstalado | Self-contained deployment (SCD) | `N-09` |
| Worker Service | Servicio de fondo de .NET basado en `BackgroundService`, alojable como servicio de Windows o systemd | Worker Service | `N-12`, `N-13` |
| Proxy inverso | Servidor que se antepone a Kestrel y hace offload de TLS, caché o contenido estático | Reverse proxy | `N-10` |
| MSIX | Formato moderno de empaquetado de aplicaciones de escritorio de Windows, con actualización automática | MSIX | `N-16` |
| tus | Protocolo abierto de subida reanudable sobre HTTP; permite retomar una subida interrumpida sin perder lo avanzado | Resumable upload protocol | `F-01` |
| FTP | Protocolo de transferencia de archivos; estándar de Internet de 1985 | File Transfer Protocol | `N-08` |

---

## Nota sobre términos que la industria confunde

Tres pares merecen cuidado porque su uso corriente induce error.

**«Arquitectura» y «diseño»** no son sinónimos: la arquitectura fija las decisiones estructurales caras de revertir; el diseño baja al detalle de cada componente. El informe describe la primera y referencia la segunda.

**«Estándar» y «plantilla»** se confunden a diario en este tema. ISO/IEC/IEEE 42010 es un estándar; arc42 y C4 son plantillas y marcos con autor. La distinción y por qué importa está en [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md#los-cuatro-niveles-de-autoridad).

**«Usabilidad» y «portabilidad»** siguen citándose como características de calidad, pero la edición 2023 de `N-04` las renombró a *Interaction Capability* y *Flexibility*. Citar las de 2011 delata material desactualizado.
