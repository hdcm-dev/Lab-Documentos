---
doc_id: ANEXO-REFERENCIAS
doc_type: anexo
title: Referencias y fuentes
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [MARCO-CONVENCIONES]
---

# Referencias y fuentes — `ANEXO-REFERENCIAS`

## Resumen ejecutivo

Toda afirmación normativa de la guía se apoya en una entrada de este anexo, clasificada según el nivel de autoridad que fija [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md). La clasificación es el instrumento central: en documentación de arquitectura circula mucha prescripción sin fuente, y una plantilla popular —arc42, C4— se cita a menudo como si fuera una norma. Este anexo separa lo que un estándar ISO, IEEE u OMG establece de lo que un marco propone y de lo que es criterio propio.

Cada fila registra la designación exacta, el estado formal y la fecha de verificación. Todas las fuentes se verificaron el **2026-07-21**. Una advertencia de método vale para todo el bloque normativo ISO: **iso.org bloquea la descarga automática (HTTP 403)**, de modo que las normas ISO se verificaron contra los **PDF de muestra oficiales** (portada, prólogo, alcance e índice), que son públicos; el texto normativo completo de cada cláusula está tras el muro de pago y no se transcribe. Lo que no pudo verificarse de primera mano se aísla en la última sección y no debe citarse como hecho.

---

## Los cinco niveles de identificador

`MARCO-CONVENCIONES` fija cuatro niveles de autoridad; este anexo los proyecta sobre cinco prefijos, porque una plantilla con autor y una guía corporativa cumplen funciones distintas aunque ninguna sea normativa.

| Prefijo | Qué agrupa | Fuerza de la cita |
|---------|------------|-------------------|
| `N-xx` | **Normativo.** Normas ISO/IEC/IEEE, RFC del IETF, especificaciones de la OMG, documentación oficial de Microsoft Learn | Es el estándar. Se cita designación y cláusula/sección |
| `G-xx` | **Marco o guía de organización.** arc42, C4, TOGAF, la plantilla SAD de RUP | Vale para quien lo adopta, no universalmente. Se nombra siempre el marco |
| `O-xx` | **Obras de referencia.** Artículos de autor identificable, papers fundacionales | Origen verificable de un concepto, no fuente de autoridad |
| `F-xx` | **Convención de facto.** Protocolos y prácticas sin especificación vigente que las imponga, drafts del IETF | No obliga. Requiere señalar la evidencia que la sostiene |
| `P-xx` | **Evidencia de plataformas y herramientas.** Documentación de herramientas reales | Prueba de qué hace una herramienta, no de qué corresponde hacer |

La confusión entre `N` y `G` es la que más daño hace en este tema. Que arc42 tenga una sección 7 de despliegue no convierte «tener una sección de despliegue» en un requisito normativo; lo convierte en lo que arc42 propone. El requisito normativo —que una descripción de arquitectura identifique a sus *stakeholders*, sus *concerns* y las vistas que los atienden— está en `N-01`, y arc42 es una forma de cumplirlo.

---

## 1. Fuentes normativas — `N-xx`

### 1.1 Descripción y evaluación de arquitectura — ISO/IEC/IEEE serie 420xx

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-01 | ISO/IEC/IEEE 42010:2022 | Software, systems and enterprise — Architecture description | **2.ª edición, 2022-11**; vigente. Cancela y reemplaza 42010:2011 | `https://www.iso.org/standard/74393.html` | 2026-07-21 |
| N-02 | ISO/IEC/IEEE 42020:2019 | Software, systems and enterprise — Architecture processes | 1.ª edición, 2019-07; vigente | `https://www.iso.org/standard/68982.html` | 2026-07-21 |
| N-03 | ISO/IEC/IEEE 42030:2019 | Software, systems and enterprise — Architecture evaluation framework | 1.ª edición, 2019-07; vigente | `https://www.iso.org/standard/73436.html` | 2026-07-21 |

`N-01` es la norma central de esta guía. Especifica **requisitos para la estructura y expresión de una descripción de arquitectura (AD)**, y distingue la arquitectura de una entidad de la descripción que la expresa. Su cláusula 6 enumera lo que una AD debe contener: identificación y visión general de la AD, identificación de *stakeholders*, sus perspectivas, sus *concerns*, los *aspects*, los *architecture viewpoints* incluidos, las *architecture views* incluidas, sus componentes, el registro de *correspondences* entre elementos, y el registro de decisiones de arquitectura con su *rationale*. La norma **no prescribe procesos, métodos, modelos ni notaciones**: fija qué debe estar, no cómo dibujarlo. Su linaje es IEEE 1471-2000 → ISO/IEC/IEEE 42010:2011 → **42010:2022**, y el cambio de vocabulario de 2022 —«system of interest» pasó a «entity of interest», «architecture framework» a «architecture description framework»— se hizo para alinear con `N-02` y `N-03`.

`N-02` especifica un conjunto de procesos de arquitectura —gobierno, gestión, conceptualización, evaluación y elaboración—. `N-03` define un marco genérico de **evaluación** de arquitectura, estructurado en tres niveles: síntesis de la evaluación, valoración de valor y análisis arquitectónico; da respaldo normativo a `ESC-4`, la evaluación de una solución ajena.

**Advertencia de citación.** El número de procesos y actividades de `N-02` que circula en la literatura secundaria («seis procesos, 37 actividades») no se verificó contra la norma; lo verificado son los cinco tipos de proceso citados arriba. La sucesión IEEE 1471-2000 → 42010:2011 consta en fuentes secundarias de IEEE, no en el prólogo de `N-01`, que solo nombra la edición 2011 que reemplaza.

### 1.2 Calidad del producto y requisitos — SQuaRE e ingeniería de requisitos

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-04 | ISO/IEC 25010:2023 | SQuaRE — Product quality model | **2.ª edición, 2023-11**; vigente. Revisa 25010:2011 | `https://www.iso.org/standard/78176.html` | 2026-07-21 |
| N-05 | ISO/IEC 25002:2024 | SQuaRE — Quality model overview and usage | 1.ª edición, 2024; vigente | `https://www.iso.org/standard/78175.html` | 2026-07-21 |
| N-06 | ISO/IEC/IEEE 29148:2018 | Systems and software engineering — Life cycle processes — Requirements engineering | 2.ª edición, 2018; vigente. Reemplaza IEEE 830-1998 | `https://www.iso.org/standard/72089.html` | 2026-07-21 |

`N-04` es el catálogo de referencia de los **requisitos no funcionales**: define el modelo de calidad de producto con **nueve características** (la edición 2011 tenía ocho). La lista verificada contra el prólogo de la 2.ª edición es: *Functional Suitability, Performance Efficiency, Compatibility, Interaction Capability, Reliability, Security, Maintainability, Flexibility, Safety*. Los cambios respecto de 2011, transcritos verbatim del prólogo, son sustantivos y desactualizan casi todo el material secundario en circulación:

> «Safety has been added as a quality characteristic… Usability and portability have been replaced with interaction capability and flexibility respectively… Inclusivity and self-descriptiveness, resistance, and scalability have been added as subcharacteristics of interaction capability, security, and flexibility respectively… User interface aesthetics and maturity have been replaced with user engagement and faultlessness respectively… Accessibility has been split into inclusivity and user assistance.»

Se citan textuales porque el error más común sobre calidad es usar la lista de 2011 —«usabilidad» y «portabilidad» entre las ocho— cuando la vigente renombró ambas y agregó *Safety*. El detalle de las subcaracterísticas se desarrolla en [`TEM-RNF`](../40-Requisitos/Requisitos-No-Funcionales.md).

`N-05` es el paraguas que explica cómo se usan los modelos de calidad SQuaRE; en la reestructuración de 2023–2024, el modelo de calidad de producto quedó en `N-04`, la visión general se movió a `N-05` y el modelo de calidad en uso se movió a ISO/IEC 25019.

`N-06` es la norma de **ingeniería de requisitos**. Define tres plantillas de especificación —StRS (de *stakeholders*), SyRS (de sistema) y SRS (de software)—, las nueve características de un requisito individual bien escrito —*necessary, appropriate, unambiguous, complete, singular, feasible, verifiable, correct, conforming*— y la estructura de contenido de una SRS, que separa las **Functions** (requisitos funcionales) de las *performance requirements*, *usability requirements*, *design constraints* y *software system attributes* (los no funcionales). Reemplaza a la clásica IEEE 830-1998, que sigue citándose por inercia y está superada.

**Advertencia de citación.** Son verbatim del prólogo: las cinco subcaracterísticas de *Safety* —*operational constraint, risk identification, fail safe, hazard warning, safe integration*—, las subcaracterísticas **agregadas** —*inclusivity* y *self-descriptiveness* en Interaction Capability, *resistance* en Security, *scalability* en Flexibility— y los renombres —*maturity* a *faultlessness*, *user interface aesthetics* a *user engagement*—. Las subcaracterísticas de las características que *no* cambiaron se reconstruyeron desde la base de 2011 (el prólogo declara que solo se les dieron nombres y definiciones más precisos). La numeración exacta de cláusulas de `N-06` no se leyó línea por línea: la estructura de la SRS se verificó contra una plantilla conforme a la norma, no contra el texto pago.

### 1.3 Notación de despliegue — OMG UML

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-07 | OMG UML 2.5.1 (`formal/2017-12-05`) | Unified Modeling Language | Especificación formal, versión 2.5.1, diciembre 2017; vigente | `https://www.omg.org/spec/UML/2.5.1/` | 2026-07-21 |

`N-07` define la notación estándar de la **vista de despliegue**: el diagrama de despliegue modela el despliegue de *artifacts* (piezas físicas desplegables) sobre *nodes*, distinguiendo *device nodes* (recursos de cómputo físicos) de *execution environment nodes* (contenedores de software: un sistema operativo, un servidor de aplicaciones). Una *Deployment* asocia un artefacto con el nodo donde se despliega, y los nodos se unen por *communication paths*. Es la referencia normativa para dibujar dónde corre cada componente.

**Advertencia de citación.** La designación, versión y alcance son verbatim de la página oficial de la OMG; la semántica de nodos y artefactos de arriba es una **paráfrasis** de la cláusula 19 («Deployments») de la especificación, no una cita textual.

### 1.4 Transferencia de archivos — IETF

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-08 | RFC 959 | File Transfer Protocol (FTP) | **Internet Standard — STD 9** (octubre 1985) | `https://www.rfc-editor.org/rfc/rfc959` | 2026-07-21 |

`N-08` es FTP, una de las dos opciones de servidor de archivos del sistema de ejemplo. Es un estándar de Internet de 1985; su objetivo declarado es promover el compartir archivos y transferir datos de forma fiable y eficiente. La otra opción del ejemplo —el protocolo de subida reanudable tus— no es una norma y figura en `F-01`.

### 1.5 .NET y ASP.NET Core — documentación oficial de Microsoft

Todas las páginas de ASP.NET Core se consultaron forzando el moniker `aspnetcore-10.0`. La advertencia es operativa: el selector de versión de Microsoft Learn puede servir contenido de otra versión, de modo que una consulta sin `?view=aspnetcore-10.0` no es reproducible.

| ID | Documento | Qué fija | URL | Verificado |
|----|-----------|----------|-----|------------|
| N-09 | .NET application publishing overview | FDD vs SCD, single-file, ReadyToRun, Native AOT; comandos `dotnet publish` | `https://learn.microsoft.com/en-us/dotnet/core/deploying/` | 2026-07-21 |
| N-10 | Host ASP.NET Core on Windows with IIS / Web server implementations | Kestrel solo o tras proxy inverso (IIS, Nginx); qué offload hace el proxy | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/` | 2026-07-21 |
| N-11 | Host ASP.NET Core in a Windows Service | Alojar una web app como servicio de Windows sin IIS; arranque tras reinicio | `https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service` | 2026-07-21 |
| N-12 | Worker Services in .NET | Plantilla `dotnet new worker`, `BackgroundService`, `Microsoft.Extensions.Hosting` | `https://learn.microsoft.com/en-us/dotnet/core/extensions/workers` | 2026-07-21 |
| N-13 | Create a Windows Service using BackgroundService / `UseWindowsService` | `UseWindowsService()` y `UseSystemd()`; lifetime, content root, logging | `https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service` | 2026-07-21 |
| N-14 | Host ASP.NET Core on Linux with Nginx | Kestrel tras Nginx; unidad systemd para arranque y supervisión | `https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx` | 2026-07-21 |
| N-15 | Containerize a .NET app with dotnet publish | `dotnet publish -t:PublishContainer` produce imagen OCI sin Docker | `https://learn.microsoft.com/en-us/dotnet/core/containers/sdk-publish` | 2026-07-21 |
| N-16 | MSIX packaging overview / Deploy a WPF app | Empaquetado MSIX y ClickOnce para escritorio; actualización automática | `https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/packaging/` | 2026-07-21 |
| N-17 | Publish a .NET MAUI app for Windows | App empaquetada (MSIX) o desempaquetada | `https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/overview` | 2026-07-21 |

Dos precisiones de alcance. `N-09` establece que el despliegue **dependiente del framework** (FDD) es el modo por defecto y exige que el host tenga instalado el runtime de .NET, mientras que el **autocontenido** (SCD) incluye el runtime y no lo exige en el host; `single-file`, `ReadyToRun` y `Native AOT` son variantes que se combinan con esos dos modos. `N-15` establece que el SDK de .NET crea imágenes de contenedor **sin Docker**; Docker o Podman solo hacen falta para *ejecutar* la imagen. La afirmación sobre imágenes base *chiseled*/*distroless* no está en `N-15` y no debe atribuírsele: pertenece a la página de imágenes base de .NET y no se verificó en esta revisión.

### 1.6 Proveedor de PostgreSQL para .NET

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| N-18 | Npgsql | .NET data provider for PostgreSQL; proveedor EF Core `Npgsql.EntityFrameworkCore.PostgreSQL` | Proyecto de código abierto, activo; proveedor EF Core 10.0.x alineado con .NET 10 | `https://www.npgsql.org/` | 2026-07-21 |

`N-18` es el proveedor de facto de PostgreSQL para .NET, y su documentación oficial es autoritativa sobre ese hecho. No es una norma ni documentación de Microsoft; se clasifica aquí por ser la fuente oficial del proveedor, y se cita para nombrar la tecnología de persistencia del sistema de ejemplo, no como prescripción.

---

## 2. Marcos y guías de organización — `G-xx`

Plantillas y marcos con autor u organización identificable. Ninguno es normativo; cada uno propone una forma concreta de organizar una descripción de arquitectura.

| ID | Marco | Autor / organización | Versión | Licencia | URL | Verificado |
|----|-------|----------------------|---------|----------|-----|------------|
| G-01 | arc42 | Gernot Starke y Peter Hruschka | Versión 9 | **CC BY-SA 4.0** | `https://arc42.org` | 2026-07-21 |
| G-02 | Modelo C4 | Simon Brown | Sin número de versión (spec viva) | **CC BY 4.0** | `https://c4model.com` | 2026-07-21 |
| G-03 | TOGAF Standard — definición de *Solution Architecture* | The Open Group | TOGAF 9.2 (2018), vigente en ediciones posteriores | Propietaria | `https://pubs.opengroup.org/architecture/togaf9-doc/arch/chap03.html` | 2026-07-21 |
| G-04 | Software Architecture Document (SAD) — plantilla RUP | Rational Software / IBM | Convención de RUP | Propietaria | — (documentación de producto RUP) | 2026-07-21 |

`G-01` **arc42** propone una plantilla de **doce secciones**, verificadas verbatim contra arc42.org: *1 Introduction & Goals, 2 Constraints, 3 Context & Scope, 4 Solution Strategy, 5 Building Block View, 6 Runtime View, 7 Deployment View, 8 Crosscutting Concepts, 9 Architectural Decisions, 10 Quality Requirements, 11 Risks & Technical Debt, 12 Glossary*. El despliegue es la sección 7 y los requisitos de calidad la 10. Es una plantilla, no una norma: adoptarla es sensato, pero su autoridad es la de sus autores.

`G-02` **C4** propone cuatro niveles de diagrama con zoom jerárquico: *System Context, Container, Component, Code*. Su aporte más citado es la definición de **container** —«una aplicación o un almacén de datos… algo que necesita estar en ejecución para que el sistema funcione»—, una unidad ejecutable o de almacenamiento por separado, explícitamente distinta de un contenedor Docker. Sus diagramas suplementarios son *System Landscape, Dynamic* y *Deployment*; este último ubica instancias de containers dentro de *deployment nodes* (infraestructura física, virtual, contenedorizada o un entorno de ejecución). C4 es **independiente de notación y de herramienta**: se puede dibujar en Mermaid, PlantUML o Structurizr.

`G-03` **TOGAF** aporta la única definición de *Solution Architecture* que esta guía cita como fuente: «una descripción de una operación de negocio discreta y focalizada y de cómo IS/IT la soporta… típicamente aplica a un solo proyecto o entrega, ayudando a traducir requisitos en una visión de solución». Fuera de TOGAF, el término no tiene definición normativa única. **Advertencia de acceso:** la página de definiciones de TOGAF ahora exige inicio de sesión SSO de The Open Group y ya no es de descarga anónima; la definición se cita desde el PDF de TOGAF 9.2.

`G-04` **SAD de RUP** es la plantilla que dio forma al documento de arquitectura tal como se lo conoce, organizada según las vistas de `O-01`. Su estructura típica —Introducción, objetivos y restricciones, vista de casos de uso, vista lógica, de proceso, de despliegue, de implementación, de datos, tamaño y rendimiento, calidad— es una **convención que varía** por versión y organización, no una especificación fija. No tiene una URL primaria canónica por ser documentación de producto.

---

## 3. Obras de referencia — `O-xx`

| ID | Autor | Título | Publicación | URL | Verificado |
|----|-------|--------|-------------|-----|------------|
| O-01 | Philippe Kruchten | Architectural Blueprints — The «4+1» View Model of Software Architecture | IEEE Software, vol. 12, n.º 6, noviembre 1995, pp. 42–50 | `https://ieeexplore.ieee.org/document/469759` | 2026-07-21 |

`O-01` es el artículo fundacional del **modelo 4+1**, la base conceptual de la plantilla SAD y el origen de la idea de describir una arquitectura por vistas que atienden concerns distintos. Sus cinco vistas: **lógica** (decomposición orientada a objetos; requisitos funcionales), **de proceso** (concurrencia, rendimiento, disponibilidad), **de desarrollo** (organización de módulos), **física** (mapeo del software al hardware: *«Mapping the Software to Hardware»* — es la vista que corresponde al despliegue) y **escenarios** (el «+1», casos de uso que validan e ilustran las otras cuatro). Es un artículo peer-reviewed, no un estándar: origen verificable de un concepto, no fuente de autoridad normativa.

**Advertencia de citación.** El rango de páginas 42–50 proviene de bases de datos de citación, no de una lectura del PDF original del artículo.

---

## 4. Convenciones de facto — `F-xx`

| ID | Designación | Título | Estado | URL | Verificado |
|----|-------------|--------|--------|-----|------------|
| F-01 | tus 1.0.0 | Resumable Upload Protocol | Protocolo abierto estable, publicado 2016-03-25 | `https://tus.io/protocols/resumable-upload` | 2026-07-21 |
| F-02 | draft-ietf-httpbis-resumable-upload | Resumable Uploads for HTTP | **Internet-Draft del IETF, en curso; no es RFC** | `https://datatracker.ietf.org/doc/draft-ietf-httpbis-resumable-upload/` | 2026-07-21 |

`F-01` es la otra opción de servidor de archivos del sistema de ejemplo: un protocolo abierto para **subidas reanudables sobre HTTP**, relevante porque el sistema de audiencias sube videos grandes que no pueden reiniciarse desde cero si el enlace se corta. Su núcleo obligatorio: `HEAD` para conocer el offset actual, `PATCH` para subir datos desde ese offset, cabeceras `Upload-Offset` y `Tus-Resumable`. `F-02` es el intento de estandarizar ese mecanismo en el IETF; se cita como draft en curso, **nunca como RFC**, y su número de revisión debe reverificarse al citarlo porque los drafts incrementan.

---

## 5. Evidencia de plataformas y herramientas — `P-xx`

| ID | Herramienta | Qué acredita | Estado | URL | Verificado |
|----|-------------|--------------|--------|-----|------------|
| P-01 | Mermaid — sintaxis C4 | Soporte C4 nativo de Mermaid marcado **experimental** por su propia documentación | Documentación oficial | `https://mermaid.js.org/syntax/c4.html` | 2026-07-21 |

`P-01` respalda la decisión de convenciones de dibujar los diagramas C4 con `flowchart` y `subgraph` en lugar de la sintaxis `C4Context`: la propia documentación de Mermaid declara que su soporte C4 es experimental y que «la sintaxis y las propiedades pueden cambiar en versiones futuras».

---

## 6. Linaje y supersesiones

Registro de qué reemplaza a qué, para detectar citas a documentos superados.

| Documento vigente | Reemplaza a | Nota |
|---|---|---|
| `N-01` 42010:2022 | 42010:2011 → IEEE 1471-2000 | El prólogo de 2022 solo nombra la edición 2011; el eslabón 1471 es de fuente secundaria |
| `N-04` 25010:2023 | 25010:2011 | La edición 2011 tenía 8 características; el modelo de calidad en uso se movió a ISO/IEC 25019 |
| `N-05` 25002:2024 | parte de 25010:2011 | La visión general de los modelos SQuaRE se separó a esta norma |
| `N-06` 29148:2018 | IEEE 830-1998, IEEE 1233-1998, IEEE 1362-1998 | La SRS clásica de IEEE 830 sigue citándose por inercia y está superada |

---

## 7. Fuentes no verificadas de primera mano

Se listan para que no se citen como hechos hasta verificarlas.

- **Cláusulas normativas completas de las normas ISO** (`N-01` a `N-06`): solo se verificaron portada, prólogo, alcance e índice de los PDF de muestra; el texto normativo interno está tras el muro de pago.
- **Cláusula 19 de `N-07`** (UML Deployments): la semántica de nodos y artefactos se parafraseó del índice y el alcance, no del texto de la cláusula.
- **Estructura exacta de secciones de `G-04`** (SAD de RUP): es una convención que varía por versión; no hay especificación primaria que la fije.
- **Recuento de procesos y actividades de `N-02`**: la cifra «seis procesos, 37 actividades» es de literatura secundaria.
- **Imágenes base *chiseled*/*distroless* de .NET**: no están en `N-15`; requieren verificar la página de imágenes base antes de citarse.
- **Definición de «arquitectura de despliegue»** como término con fuente única: no existe; es uso corriente de la industria.
