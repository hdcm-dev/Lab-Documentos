---
doc_id: ANEXO-GLOSARIO
doc_type: anexo
title: Glosario
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [ANEXO-REFERENCIAS, MARCO-CONVENCIONES, TEM-TOPO, TEM-MODELOS]
---

# Glosario — `ANEXO-GLOSARIO`

## Resumen ejecutivo

Los términos que la guía usa, con su definición y el documento donde se desarrollan. Cada entrada indica su **nivel de autoridad** según [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md): si es terminología normativa de Microsoft, convención del ecosistema o concepto de la literatura.

Los sinónimos aparecen como alias remitiendo al término canónico. Varios conceptos de este dominio circulan con dos o tres nombres, y esa dispersión es en sí misma una fuente de malentendidos.

---

## A

**Adaptador** · *Adapter* — En arquitectura hexagonal (`O-05`), el componente del borde que traduce entre el mundo exterior y un puerto del dominio. Un controlador HTTP y un repositorio de EF Core son adaptadores. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

**Allman** — Estilo de llaves donde la llave de apertura va en línea propia alineada con la sentencia. Es la convención de C# (`N-06`, `F-06`). Nombrado por Eric Allman. → [`TEM-FORMATO`](../50-Estilo-de-Codificacion/Formato-y-Llaves.md)

**Analizador** · *Analyzer* — Componente que inspecciona el código en compilación y emite diagnósticos. Los del SDK se activan con `EnableNETAnalyzers`. → [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)

**Anclaje transitivo** · *Transitive pinning* — Mecanismo de la gestión centralizada de paquetes que fuerza la versión de una dependencia que llega indirectamente (`N-08`). → [`TEM-BUILD`](../20-Organizacion-de-Soluciones/Build-Compartido.md)

**Application Part** — Unidad de ensamblado desde la que ASP.NET Core descubre controllers, vistas y componentes de aplicación. El descubrimiento por defecto del `ApplicationPartManager` ya incluye el ensamblado de entrada **y sus ensamblados dependientes**, directos o transitivos (`N-17`): los controllers de un proyecto referenciado se registran solos. `AddApplicationPart` es para los casos en que ese descubrimiento no alcanza —carga dinámica, ensamblados no referenciados, arneses de prueba que alteran `applicationName`—. Normativo. → [`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md)


**Aviso** · *warning* — Diagnóstico que el compilador o un analizador emite sin impedir la compilación. Su severidad se declara en `.editorconfig` y puede elevarse a error. La guía usa «aviso» y no *warning*. → [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)
---

## B

**Bounded Context** · *Contexto delimitado* — De DDD (`O-03`). Frontera dentro de la cual un modelo y su vocabulario son consistentes. Es el criterio de corte más sólido para decidir una partición en servicios. → [`TEM-PART`](../10-Arquitectura-de-Servicios/Criterios-de-Particion.md)

---

## C

**camelCase** · *lower camel case* — Primera palabra en minúscula, las siguientes con inicial mayúscula: `nombreDeSala`. En .NET aplica a parámetros y variables locales (`N-02`). → [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md)

**Capa** · *Layer* — Agrupación horizontal del código por responsabilidad técnica: presentación, aplicación, dominio, infraestructura. Es una división **lógica**, interna al código. Ver *Tier* para la distinción que la traducción al español borra. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

**Clean Architecture** — Modelo de organización en capas concéntricas con la regla de dependencia hacia el centro (`O-04`, Martin 2017). **No es un estándar de Microsoft**: Microsoft lo describe en `N-12` sin prescribirlo. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

**Contrato** — La forma de los datos que cruzan un borde entre partes que evolucionan por separado, típicamente el borde HTTP. Se materializa en tipos planos y serializables —los DTO— que viven donde el consumidor pueda alcanzarlos, y su ciclo de cambio es el de la versión de la API, no el del negocio. Un proyecto de contratos no referencia el dominio: si lo hace, sus clientes quedan acoplados a lo que el contrato existía para aislar. → [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md), [`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md)

**CPM** → ver *Gestión centralizada de paquetes*.

**CQRS** · *Command Query Responsibility Segregation* — Separación del modelo de lectura del de escritura. No implica event sourcing ni bases de datos separadas; esa es la confusión más frecuente sobre el término. Sin fuente normativa: el acrónimo se atribuye a Greg Young y deriva del principio *Command-Query Separation* de Meyer. → [`TEM-DATOS`](../60-Patrones-de-Codigo/Patrones-de-Acceso-a-Datos.md)

**Convención de facto** — Práctica dominante del ecosistema que ninguna especificación impone. Nivel de autoridad intermedio en esta guía. → [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md)


**Cambio ruptor** · *breaking change* — Modificación que obliga a los consumidores a cambiar su código o que rompe su compilación. Es el criterio que separa `CTX-3` de los demás contextos: en una biblioteca publicada, renombrar un tipo público lo es. → [`MARCO-CONTEXTOS`](../00-Marco-de-Referencia/Contextos.md)

**Compuerta** · *gate* — Verificación que impide avanzar si no se cumple. Las hay de integración continua, de cobertura y de compilación. Quién decide su severidad y dónde se evalúa es de `ACT-05`. → [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)
---

## D

**`Directory.Build.props`** — Archivo de MSBuild con propiedades heredadas por todos los proyectos bajo su directorio (`N-09`). Normativo. → [`TEM-BUILD`](../20-Organizacion-de-Soluciones/Build-Compartido.md)

**`Directory.Packages.props`** — Archivo donde se declaran centralmente las versiones de los paquetes NuGet (`N-08`). Normativo. → [`TEM-BUILD`](../20-Organizacion-de-Soluciones/Build-Compartido.md)

**DTO** · *Data Transfer Object* — Tipo plano y sin comportamiento cuyo único fin es transportar datos a través de una frontera. Distinto de una entidad de dominio y de un modelo de presentación, aunque describan el mismo concepto: lo que los separa es quién los cambia. Dónde vive y cuándo hace falta, en [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md); su papel en el acceso a datos, en [`TEM-DATOS`](../60-Patrones-de-Codigo/Patrones-de-Acceso-a-Datos.md).

---

## E

**`.editorconfig`** — Archivo de configuración de reglas de estilo y nomenclatura, interpretado por el compilador y las herramientas (`N-07`). Es el mecanismo por el cual una convención deja de depender de la memoria de las personas. → [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)

**`EnforceCodeStyleInBuild`** — Propiedad que hace que las reglas de estilo `IDExxxx` se verifiquen durante la compilación. Sin `.editorconfig` solo fuerza los valores por defecto del SDK. → [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)

**Espacio de nombres** · *Namespace* — Agrupación lógica de tipos. Su convención de nombre es normativa (`N-01`): `<Empresa>.<Producto>[.<Funcionalidad>]`. → [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md)


**Endpoint** — Punto de entrada HTTP de una aplicación, identificado por ruta y verbo. La guía conserva el anglicismo por falta de una traducción asentada. → [`TEM-ENDP`](../60-Patrones-de-Codigo/Patrones-de-Endpoint.md)

**Ensamblado** · *assembly* — Unidad de compilación y despliegue de .NET: el `.dll` o `.exe` que produce un proyecto. Es lo que nombra `AssemblyName`, y lo que el compilador toma como frontera real de visibilidad —`internal` alcanza hasta acá, no hasta el espacio de nombres—. → [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md)
---

## F

**Framework Design Guidelines** — Guías de diseño de Microsoft (`O-01`, `N-01` a `N-04`). Escritas **para bibliotecas**, no para aplicaciones; las páginas de Learn reproducen la 2.ª edición de 2008 con aviso de posible desactualización. → [`FAM-NOM`](../40-Nomenclatura/README.md)

**Front Controller** — Patrón de `O-02` (Fowler): un punto de entrada único que despacha las peticiones. Es el ancestro del modelo de controllers de MVC. → [`TEM-ENDP`](../60-Patrones-de-Codigo/Patrones-de-Endpoint.md)

---

## G

**Gestión centralizada de paquetes** · *Central Package Management*, *CPM* — Ver `Directory.Packages.props`.

**`global.json`** — Archivo que fija la versión del SDK de .NET para el repositorio (`N-13`). Normativo. → [`TEM-BUILD`](../20-Organizacion-de-Soluciones/Build-Compartido.md)

**Gran bola de barro** · *Big Ball of Mud* — Sistema sin estructura discernible. Es ortogonal al modelo de despliegue: existe tanto en monolitos como en microservicios. → [`TEM-MONO`](../10-Arquitectura-de-Servicios/Monolito.md)

---

## H

**Hexagonal** · *Ports and Adapters* — Modelo donde el dominio define puertos y el exterior provee adaptadores (`O-05`, Cockburn 2005). No es estándar de Microsoft. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

**Húngara, notación** — Convención de prefijar el identificador con su tipo (`strNombre`, `iContador`). `N-03` la prohíbe explícitamente en .NET. Origen: Charles Simonyi. → [`TEM-ANTI`](../40-Nomenclatura/Antipatrones-de-Nombrado.md)

---

## K

**K&R** — Estilo de llaves con la de apertura en la misma línea de la sentencia. Nombrado por Kernighan y Ritchie. Es la convención de Java y JavaScript, no la de C#. → [`TEM-FORMATO`](../50-Estilo-de-Codificacion/Formato-y-Llaves.md)

**kebab-case** — Palabras en minúscula separadas por guiones: `reservar-sala`. No se usa en identificadores de C#; sí en rutas HTTP. → [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md)

---

## L

**Lenguaje ubicuo** · *Ubiquitous language* — De DDD (`O-03`). El vocabulario compartido entre el negocio y el código: si el negocio dice «cancelar», el método se llama `Cancelar`. → [`TEM-NOMB`](../40-Nomenclatura/Nombrado-de-Tipos-y-Miembros.md)

---

## M

**Minimal API** · *route handler*, *minimal hosting model* — Modelo de ASP.NET Core (desde .NET 6) donde los endpoints se registran como delegados con `MapGet`, `MapPost` y equivalentes. → [`TEM-ENDP`](../60-Patrones-de-Codigo/Patrones-de-Endpoint.md)

**Modelo de presentación** · *ViewModel*, *Vm* — Estructura que la interfaz liga a sus controles, con los datos ya formateados y las decisiones ya tomadas: la vista muestra `PuedeCancelarse`, no evalúa la política. Vive junto al componente que lo usa, porque cambia cuando cambia la pantalla. → [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md)

**Monolito** — Sistema que se despliega como una unidad. **No** significa código desordenado; esa equivalencia es el malentendido central del tema. → [`TEM-MONO`](../10-Arquitectura-de-Servicios/Monolito.md)

**Monolito distribuido** — Varias unidades desplegables que no pueden desplegarse ni fallar de forma independiente, típicamente por compartir base de datos. Acumula los costos de ambos modelos sin los beneficios de ninguno. Término de uso corriente, sin fuente normativa. → [`TEM-MICRO`](../10-Arquitectura-de-Servicios/Microservicios.md)

**Monolito modular** — Una unidad desplegable con módulos de límites explícitos. Sin definición canónica única; esta guía fija la suya. → [`TEM-MODU`](../10-Arquitectura-de-Servicios/Monolito-Modular.md)

**MSBuild SDK** — El valor del atributo `Sdk` de un `.csproj`, que determina qué destinos y propiedades trae el proyecto. Es lo que define técnicamente un «tipo de proyecto» (`N-10`). → [`TEM-SDK`](../20-Organizacion-de-Soluciones/Tipos-de-Proyecto.md)


**Módulo** — Agrupación de código con una superficie pública declarada y un límite que algo hace cumplir. Se distingue de una carpeta en que el límite es verificable, y de un servicio en que no implica despliegue propio. → [`TEM-MODU`](../10-Arquitectura-de-Servicios/Monolito-Modular.md)
---

## N

**N capas** · *N-Layer*, *Layered Architecture* — Organización horizontal clásica (`O-02`, `N-12`). El número de capas varía entre autores. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

**Namespace con ámbito de archivo** · *file-scoped namespace* — Declaración `namespace Foo;` sin llaves, desde C# 10. Reduce un nivel de indentación. → [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md)

**Normativo** — Nivel de autoridad de lo publicado por Microsoft como especificación o guía oficial. → [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md)


**Normalización** — Llevar código existente a una convención sin cambiar su comportamiento. Es el escenario `ESC-3`, y su rasgo definitorio es que cambia la superficie y no la arquitectura: una reorganización que además mueve responsabilidades es `ESC-2` disfrazado. → [`MARCO-ESCENARIOS`](../00-Marco-de-Referencia/Escenarios.md)
---

## O

**Onion Architecture** — Capas concéntricas con el dominio en el centro (`O-06`, Palermo 2008). No es estándar de Microsoft. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

---

## P

**PascalCase** · *upper camel case* — Todas las palabras con inicial mayúscula: `ReservaDeSala`. En .NET aplica a tipos, miembros públicos, espacios de nombres y **también a las constantes**, a diferencia de otros lenguajes (`N-02`). → [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md)

**Plano estructural / plano de dominio** — Los dos vocabularios que conviven en los nombres de un sistema. El estructural nombra la arquitectura —`Domain`, `Application`, `Contracts`, `Repository`— y proviene de una literatura escrita en inglés; el del dominio nombra el negocio —`Reserva`, `Aforo`, `PoliticaCancelacion`— y su valor está en coincidir con las palabras de quien define las reglas. Se deciden por separado, y la combinación más frecuente es estructura en inglés con dominio en el idioma del negocio. Lo que falla es mezclar **dentro** de un plano. Criterio propio de esta guía. → [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md)

**Puerto** · *Port* — En arquitectura hexagonal, la interfaz que el dominio define y que el exterior implementa. → [`TEM-CAPAS`](../30-Organizacion-Interna/Modelos-de-Capas.md)

---

## R

**RCL** · *Razor Class Library* — Biblioteca que empaqueta componentes Razor y activos estáticos. SDK `Microsoft.NET.Sdk.Razor`. → [`TEM-SDK`](../20-Organizacion-de-Soluciones/Tipos-de-Proyecto.md)

**Repository** · *Repositorio* — Patrón de `O-02` que abstrae el acceso a una colección de objetos de dominio. Su necesidad sobre EF Core es genuinamente discutida. → [`TEM-DATOS`](../60-Patrones-de-Codigo/Patrones-de-Acceso-a-Datos.md)

**`RootNamespace`** — Propiedad de MSBuild que fija el espacio de nombres base. Distinta de `AssemblyName`, aunque coincidan por defecto (`F-05`). → [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md)

---

## S

**Screaming Architecture** — Idea de que la estructura de carpetas debería revelar qué hace el sistema, no qué framework usa. Atribuida a Robert C. Martin; divulgada en su blog, no en `O-04`. → [`TEM-SLICE`](../30-Organizacion-Interna/Vertical-Slice.md)

**Severidad** — Nivel asignado a una regla en `.editorconfig`: `none`, `silent`, `suggestion`, `warning`, `error`. Decisión de `ACT-05`. → [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)

**`.slnx`** — Formato de archivo de solución en XML. Desde .NET 10, `dotnet new sln` lo crea por defecto; el soporte de CLI existe desde el SDK 9.0.200 (`N-11`). → [`TEM-SLN`](../20-Organizacion-de-Soluciones/Estructura-de-Solucion.md)

**`s_`, `t_`** — Prefijos de campos estáticos y `[ThreadStatic]`. Provienen del estilo de `dotnet/runtime` (`F-03`, `O-08`), **no** de las Framework Design Guidelines. Error de atribución frecuente. → [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md)

**Solución** — Agrupador de proyectos para las herramientas. **No** es una unidad de despliegue ni de arquitectura. → [`TEM-SLN`](../20-Organizacion-de-Soluciones/Estructura-de-Solucion.md)

**Strangler Fig** — Estrategia de migración incremental que envuelve el sistema existente y le va quitando responsabilidades. Atribuida a Martin Fowler. → [`TEM-PART`](../10-Arquitectura-de-Servicios/Criterios-de-Particion.md)

---

## T

**TAP** · *Task-based Asynchronous Pattern* — Patrón asincrónico de .NET que **establece** la convención vigente del sufijo `Async` (`N-15`, `F-04`). El sufijo es anterior al TAP: ya lo exigía el *Event-based Asynchronous Pattern*. → [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md)

**Tier** — Unidad de despliegue físico: un proceso, una máquina, un contenedor. El español traduce tanto *layer* como *tier* por «capa», y esa colisión es la que hace difícil leer la literatura en inglés sobre el tema.

La distinción es la misma que separa las dos familias de esta guía: *layer* es organización del código ([`FAM-INT`](../30-Organizacion-Interna/README.md)), *tier* es organización del despliegue ([`FAM-SRV`](../10-Arquitectura-de-Servicios/README.md)). Un sistema de tres capas lógicas desplegado como un solo proceso tiene tres *layers* y un *tier*, y eso no es una contradicción sino la disposición más común. Esta guía dice «capa» solo por *layer*, y para *tier* usa «unidad desplegable».

**Topología de solución** — El conjunto de proyectos de una solución, la dirección de las referencias entre ellos y su reparto en unidades desplegables. Esta guía cataloga cinco —`T1` proyecto único, `T2` capas en proyectos, `T3` web y API en un proceso, `T4` dos procesos, `T5` contratos con varios clientes—. La cantidad de proyectos y la de procesos son ejes independientes: la confusión entre ambos es el error más caro del tema. Criterio propio de esta guía. → [`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md)

---

## U

**Unit of Work** — Patrón de `O-02` que agrupa operaciones en una transacción. El `DbContext` de EF Core ya lo implementa. → [`TEM-DATOS`](../60-Patrones-de-Codigo/Patrones-de-Acceso-a-Datos.md)


**Unidad desplegable** — Lo que se despliega y se ejecuta como un todo: un proceso, un contenedor, un servicio. Es el eje sobre el que se define monolito, monolito distribuido y microservicio, y el que la guía mantiene separado de la organización del código. Ver también *Tier*. → [`FAM-SRV`](../10-Arquitectura-de-Servicios/README.md)
---

## V

**Vertical Slice** — Nombre del modelo de organización por funcionalidad en lugar de por capa técnica (`O-07`, Bogard 2018). No es estándar de Microsoft. Se conserva en inglés porque es la designación del autor; la unidad concreta que produce se llama **corte vertical**, en español, y es la forma que la guía usa en prosa. → [`TEM-SLICE`](../30-Organizacion-Interna/Vertical-Slice.md)

---

## Términos que se confunden entre sí

| Par | Distinción |
|-----|-----------|
| *Layer* / *Tier* | Capa lógica de código / unidad de despliegue físico |
| Solución / Sistema | Agrupador de herramienta / conjunto que se despliega |
| Proyecto / Proceso | `.csproj` que se compila / unidad que se ejecuta. Ejes independientes: N proyectos pueden correr en uno, y uno puede correr en N instancias |
| Contrato / Entidad de dominio | Lo que cruza el borde, lo cambia una versión de la API / el concepto con sus invariantes, lo cambia una regla de negocio |
| DTO / Modelo de presentación | Cruza una frontera de red / lo liga la interfaz. Distinta frontera, distinto ciclo de cambio |
| Plano estructural / Plano de dominio | Vocabulario de la arquitectura / vocabulario del negocio. Cada uno decide su idioma por separado |
| Monolito / Código desordenado | Modelo de despliegue / calidad interna. Ortogonales |
| CQRS / Event Sourcing | Separar lectura de escritura / almacenar eventos. Independientes |
| Repository / `DbSet` | Patrón / implementación que ya lo cumple |
| `RootNamespace` / `AssemblyName` | Espacio de nombres base / nombre del ensamblado |
| RFC / ADR | Instrumento para decidir / registro de lo decidido |
| Normativo / Convención de facto | Microsoft lo especifica / el ecosistema lo hace |
