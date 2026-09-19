# Changelog

Cambios relevantes de las guías del repositorio. El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## 2026-09-19

### Guides/Arquitectura/Dot-NET-Arquitectura-Guide.md — v2.0.2

#### Cambiado

- El laboratorio pasa de `Guides/Arquitectura/Dot-NET-Arquitectura-Lab/` a `Guides/Arquitectura/Examples/Dot-NET-Arquitectura-Lab/`; la guía actualiza sus cinco referencias (convenciones de §0, L00 y anexo A). `lab.sh` no depende de su ubicación y las capturas quedan como se registraron.
- Terminología: los términos de la literatura se escriben en inglés y sin traducir, en cursiva la primera vez (*Entity*, *Value Object*, *Business Rule*, *Invariant*, *Precondition*, *Factory Method*, *Use Case*, *Repository*, *Unit of Work*, *Aggregate*…). Se aplica en §3.1 y §4.1 (definiciones), en los títulos de §3.2, §3.5, §4.9, §5.2, §5.4 y §5.5 (con sus anclas y remisiones), en el resto de la prosa técnica, en el diagrama 6 y en el glosario, cuyas entradas pasan a ser el término en inglés con su equivalente en castellano.
- §0.4: «el precio no puede ser cero ni negativo» se presenta como decisión de esta tienda para el laboratorio («un producto se da de alta con su precio»), y anuncia la variante de §3.7. El laboratorio no se regenera: L09–L21 siguen valiendo.
- §3.2: la prueba del papel se corrige (el cuaderno del depósito sí anota mercadería sin precio; lo que el mostrador no hace es venderla); se agrega la pregunta «¿vale siempre o solo antes de una acción?» (ahora son cuatro) y la fila «no se vende un producto sin precio». §3.1 separa los ejemplos de *Business Rule* e *Invariant* y define *Precondition* y *Postcondition*.
- §4: se renumera por las secciones nuevas (§4.2→§4.5, §4.3→§4.6, §4.4→§4.8, §4.5→§4.9, §4.6→§4.11), y §3.6→§3.8 y §9.6→§9.7, con sus remisiones en el texto, §7.1, anexo A y glosario.
- §0.5: las marcas *Salida registrada* y **[Compilado]** admiten las variantes (`Variantes/…`, `Variantes/capturas/Vnn-….txt`). Índice, §9.5 y anexo A remiten a lo nuevo.

#### Añadido

- §3.2 «¿Cómo se reconoce una regla de negocio?»: definición (Martin, 2017, cap. 20), tres preguntas en orden (¿vale en papel?, ¿vale llegue por donde llegue el dato?, ¿el error se entiende en términos del negocio?), tabla de condiciones que lo son y que no lo son con su lugar, y la distinción con las reglas específicas de la aplicación de Martin (2012), que la guía trata como pasos del caso de uso. Se suma la definición a §3.1, la entrada al glosario y la referencia Martin (2017); la invariante pasa a definirse como una regla de negocio sobre un solo objeto. §3.2–§3.5 se renumeran a §3.3–§3.6 (con sus remisiones en §7.1, anexo A y glosario).
- §2.2 «Los tres conceptos en un programa de consola»: regla de dependencia, inversión e inyección en tres pasos (la regla nombra el detalle → la regla declara la interfaz → el contenedor hace los `new`), con un diagrama que separa la dependencia de código de la llamada en ejecución y una tabla de qué responde cada concepto y cómo puede darse sin los otros. El código está en `Examples/Dot-NET-Arquitectura-Lab/Conceptos-DI/` (Paso0–Paso2; compilados con SDK 10.0.400, cero advertencias). §2.2–§2.7 se renumeran a §2.3–§2.8 con sus remisiones; la marca «Fragmento ilustrativo» de §0.5 admite programas aparte.
- §3.6 «¿Dónde vive una *Business Rule* en un programa orientado a objetos?»: los tres tipos del Business Rules Group (2000) —*Structural Assertion*, *Action Assertion*, *Derivation*—, el objeto y el *Message* (Booch, 1994; Kay, 1998), *Design by Contract* (Meyer, 1988; Eiffel Software, s. f.; Evans, 2015), *Responsibility* (Wirfs-Brock, 2006) y una tabla de síntesis tipo de regla → dónde vive, con ejemplos de la tienda.
- §3.7 «¿Vale siempre o solo antes de una acción? El producto sin precio»: la objeción del producto que llega sin manifiesto de precios; *Invariant* frente a *Precondition* y la validación contextual (Fowler, 2005b); variante compilada `Precio decimal?` + `AsignarPrecio` + `PrecioDeVenta` en `Examples/Dot-NET-Arquitectura-Lab/Variantes/ProductoSinPrecio/`, con su salida V01.
- §4.2 «¿Cómo se pasa de un *Use Case* a un Command y un Handler?»: el método de servicio `VentasService.Vender` partido en firma (Command) y cuerpo (Handler), la frase «tengo un producto y quiero venderlo» descompuesta en piezas y el orden en que conviene pensarlas.
- §4.3 «¿Cómo se reconoce un Command y un Handler?»: señales de cada uno, prueba rápida, con qué se confunden (Request DTO, servicio, método de la *Entity*) y la analogía de la orden de trabajo.
- §4.4 «¿Cómo se reconoce un *Repository*?»: definición (Fowler, 2002; Evans, 2015), cinco preguntas de criterio con su respuesta (objetos y no tablas, uno por *Aggregate Root*, dónde vive, sin reglas, cuándo hace falta con EF Core) y con qué se confunde.
- §4.7 «¿De dónde vienen los nombres Command y Query?»: *Command-Query Separation* (Meyer, 1988, vía Fowler, 2005a), el patrón Command (Gamma et al., 1994) y *Command Message* (Hohpe y Woolf, 2003); remite a §4.8 (CQRS) sin repetirlo.
- §4.10 «Un *Use Case* con dos *Entities*: RegistrarPedido»: `Producto` y `Pedido` como *Entities*, `Pedido` como *Aggregate Root* con `ItemPedido` (que copia el precio del día), «vender» como *Use Case* (Martin, 2012), enunciado completo al estilo de Cockburn (2001) con extensiones enlazadas al código, y `RegistrarPedidoHandler` con `IUnitOfWork` y una sola confirmación. Código compilado en `Examples/Dot-NET-Arquitectura-Lab/Variantes/Pedidos/` sobre el `Producto` de la variante sin precio, con su salida V02.
- §7.4 «¿Cómo se traduce el vocabulario de siempre al de esta guía?»: tabla de equivalencias (DTO → Command/Query y Request/Response, método del servicio → Handler, clase plana de `Models/` → *Entity*, *Value Object* sin cambios, `DbContext` → *Repository*), los tres papeles que cumple una clase plana mientras no hay reglas y por qué chocan cuando aparecen, y la distinción entre las dos particiones: la del transporte, que sí crea una clase nueva, y la de la persistencia, que se resuelve sacando las anotaciones del ORM a la Fluent API (§5.4) y solo crea un *Persistence Model* con esquema ajeno (§7.2 g). §7.4 y §7.5 se renumeran a §7.5 y §7.6, con sus remisiones en §7.3, §9.1 y el anexo B; §4.3 y §5.4 remiten a la sección nueva.
- §4.10: `IUnitOfWork` declara `Task<int> SaveChangesAsync(CancellationToken ct = default)`, la firma de `DbContext.SaveChangesAsync`, de modo que `AppDbContext` la implementa sin adaptador; la prosa lo dice así. El código de la variante y la captura V02 se regeneraron: la salida es idéntica.
- §9.6 «La escalera en código: del dato a los *Use Cases*»: un ABM de personas en siete pasos, cada uno con su señal, la pieza que agrega, el delta de código (fragmento ilustrativo) y lo que dice Fowler (*Transaction Script*, *Service Layer*, *Anemic Domain Model*, *Repository* y *Unit of Work* dentro del `DbContext`), con remisiones a §3, §4, §5 y §6 en lugar de repetirlos.
- `Examples/Dot-NET-Arquitectura-Lab/Variantes/`: `ProductoSinPrecio/` (Domain + Demo), `Pedidos/` (Domain + Application + Demo), `global.json` (10.0.400, `latestFeature`), `variantes.sh` y `capturas/` V01–V02 (compilados con SDK 10.0.400 en `mcr.microsoft.com/dotnet/sdk:10.0`, cero advertencias). No forman parte de `MyProject/` ni de `lab.sh`; el anexo A las lista aparte.
- Anexo E: Booch (1994), Business Rules Group (2000), Cockburn (2001), Eiffel Software (s. f.), Evans (2003, 2015), Fowler (2003, 2005a, 2005b), Gamma et al. (1994), Hohpe y Woolf (2003), Kay (1998), Meyer (1988) y Wirfs-Brock (2006). Anexo D: 18 entradas nuevas (*Action Assertion*, *Aggregate*, *Aggregate Root*, *Anemic Domain Model*, *Command Message*, *Command-Query Separation*, *Derivation*, *Design by Contract*, *Domain Model*, *Extension*, *Message*, *Precondition*, *Postcondition*, *Primary Actor*, *Responsibility*, *Service Layer*, *Structural Assertion*, validación contextual).

## 2026-09-18 (reedición)

### Guides/Arquitectura/Dot-NET-Arquitectura-Guide.md — v2.0.1

#### Cambiado

- Reedición íntegra como guía de estudio y de consulta de criterios (1425 líneas): §0 cómo usarla, problema conductor y escenarios E-A..E-D; Parte I fundamentos (solución, proyecto, referencia; regla de dependencia); Parte II construcción capa por capa (Domain, Application, Infrastructure y WebAPI, clientes); Parte III criterio (§7 «Cada objeto responde una pregunta» con las siete respuestas explicativas, §8 estructura física y nombres, §9 del problema a la estructura); anexos A–E (hoja de ruta del laboratorio, lista de verificación, versiones y licencias fechadas, glosario, referencias autor-fecha).
- Cada capítulo sigue el patrón definiciones → decisiones como pregunta con respuesta en una línea → práctica → cierre «¿cuándo no?».
- Toda salida mostrada proviene de una ejecución registrada; todo bloque de C# está rotulado [Compilado] o [Fragmento ilustrativo]; las afirmaciones volátiles llevan fuente y fecha de consulta.
- Base tecnológica: .NET 10 LTS, EF Core 10, `.slnx`, plantilla `webapi` sin Swashbuckle. MediatR y AutoMapper salen del ejemplo (handlers propios y mapeo manual) y pasan a §9.4 como caso de evaluación de dependencias (licencia dual desde 13.0/15.0; vulnerabilidad alta en AutoMapper 14.0.0).
- Estructura física `src/Backend`, `src/Clients`, `src/Contracts`; `Contracts` nace con el primer cliente .NET; el controller traduce entre contrato y mensajes (el Command deja de ser el cuerpo HTTP); `IProductoRepository` en Domain; regla de referencias del front condicionada por escenario.
- Correcciones técnicas sobre la versión anterior: `CreatedAtAction` apunta a una acción existente; `Add` + `SaveChangesAsync` en el repositorio; Blazor Hybrid descrito según la documentación (nativo + Web View, sin WebAssembly); configuración de WASM en `wwwroot` rotulada como visible al usuario; sin afirmar que Identity emita JWT.

#### Añadido

- `Guides/Arquitectura/Dot-NET-Arquitectura-Lab/`: `lab.sh` (25 pasos L00–L24 en `mcr.microsoft.com/dotnet/sdk:10.0`), `capturas/` (salidas literales con encabezado de fecha, imagen, digest y SDK), `aserciones.log` (74 PASS, 1 FAIL provocado a propósito) y `MyProject/` (la solución final que compila con cero advertencias).

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
