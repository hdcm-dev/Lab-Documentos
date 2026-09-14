---
doc_id: ANEXO-REFERENCIAS
doc_type: anexo
title: Referencias y niveles de autoridad
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [MARCO-CONVENCIONES, ANEXO-GLOSARIO]
---

# Referencias y niveles de autoridad — `ANEXO-REFERENCIAS`

## Resumen ejecutivo

Toda afirmación normativa de la guía se apoya en una de estas fuentes, clasificada según el nivel de autoridad que fija [`MARCO-CONVENCIONES`](../00-Marco-de-Referencia/Convenciones.md): normativo, convención de facto o criterio propio. La clasificación importa porque buena parte del material disponible sobre organización de código .NET presenta como oficial lo que es opinión, y el lector que no distingue ambas cosas no puede evaluar si le conviene.

Las URL se verificaron el **2026-07-19**. Cada entrada indica su estado de verificación.

---

## 1. Fuentes normativas — Microsoft

Documentación oficial. Lo que estas fuentes prescriben es el estándar de .NET en sentido estricto.

| # | Fuente | Qué fija | URL | Estado |
|---|--------|----------|-----|--------|
| N-01 | Framework Design Guidelines — Naming Guidelines | Nombres de tipos, miembros, espacios de nombres | `https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines` | Verificado |
| N-02 | Capitalization Conventions | PascalCasing y camelCasing, y dónde aplica cada uno | `https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions` | Verificado |
| N-03 | General Naming Conventions | Legibilidad, abreviaturas, prohibición de notación húngara | `https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions` | Verificado |
| N-04 | Names of Type Members | Métodos como verbos, propiedades como sustantivos, campos | `https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members` | Verificado |
| N-05 | Identifier names — rules and conventions | Reglas del lenguaje y convenciones de identificadores en C# | `https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names` | Verificado |
| N-06 | C# Coding Conventions | Estilo de C#: llaves Allman, `var`, orden de miembros | `https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions` | Verificado |
| N-07 | .NET code style rule options | Reglas de estilo configurables por `.editorconfig` | `https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options` | Verificado |
| N-08 | Central Package Management | `Directory.Packages.props`, `PackageVersion`, anclaje transitivo | `https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management` | Verificado |
| N-09 | Customize the build by folder | `Directory.Build.props` y `.targets`, orden de importación | `https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory` | Verificado |
| N-10 | .NET project SDKs — overview | Catálogo oficial de SDKs de proyecto | `https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview` | Verificado |
| N-11 | `dotnet sln` | Formato `.slnx`, migración y soporte de CLI | `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln` | Verificado |
| N-12 | Common web application architectures | Monolito, N capas, Clean Architecture, microservicios | `https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures` | Verificado |
| N-13 | `global.json` overview | Selección del SDK: `version`, `rollForward`, `allowPrerelease`, `sdk.paths` | `https://learn.microsoft.com/en-us/dotnet/core/tools/global-json` | Verificado |
| N-14 | `nuget.config` File Reference | Orígenes de paquetes, credenciales, `packageSourceMapping` | `https://learn.microsoft.com/en-us/nuget/reference/nuget-config-file` | Verificado |
| N-15 | Task-based Asynchronous Pattern (TAP) | Convención del sufijo `Async` en métodos que devuelven tipos esperables | `https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap` | Verificado |
| N-16 | Analizador `VSTHRD100` — Avoid `async void` methods | Detección de `async void`; requiere el paquete `Microsoft.VisualStudio.Threading.Analyzers` | `https://microsoft.github.io/vs-threading/analyzers/VSTHRD100.html` | Verificado |
| N-17 | Application Parts en ASP.NET Core | Descubrimiento de controllers alojados en otros ensamblados; `AddApplicationPart` | `https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts` | Verificado |
| N-18 | Minimal APIs — overview | Registro de endpoints por método de extensión sobre `IEndpointRouteBuilder`; `MapGroup` | `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis` | Verificado |
| N-19 | .NET MAUI — estructura de proyecto | El proyecto MAUI usa `Microsoft.NET.Sdk` con `UseMaui` y `SingleProject`, no un SDK propio | `https://learn.microsoft.com/en-us/dotnet/maui/migration/multi-project-to-multi-project-with-template` | Verificado |
| N-20 | Blazor Web App con OIDC | Muestra `AddAdditionalAssemblies` para componentes enrutables en otro ensamblado | `https://learn.microsoft.com/en-us/aspnet/core/blazor/security/blazor-web-app-with-oidc` | Verificado |

### Precisiones sobre las fuentes normativas

**N-01 a N-04 son de 2008.** Las páginas de Framework Design Guidelines en Microsoft Learn reproducen la segunda edición del libro y llevan un aviso explícito de que parte del contenido puede estar desactualizado. Siguen siendo la referencia normativa para diseño de bibliotecas (`CTX-3`); esta guía señala en cada caso dónde la práctica actual difiere.

**N-01 a N-04 fueron escritas para bibliotecas, no para aplicaciones.** Es la precisión que más se pierde. El propio título de la obra —*Framework* Design Guidelines— delimita el alcance. Aplicar su rigor completo a una aplicación interna agrega ceremonia sin comprar la compatibilidad que justifica esas reglas.

**N-12 es documentación de arquitectura, no especificación.** Microsoft publica ahí una descripción de arquitecturas frecuentes con recomendaciones; describe Clean Architecture sin prescribirla como estándar.

---

## 2. Convenciones de facto

Prácticas dominantes del ecosistema que ninguna especificación impone.

| # | Convención | Origen verificable | Estado |
|---|-----------|--------------------|--------|
| F-01 | Código productivo bajo `src/`, tests separados del código productivo | Usada por Microsoft en tutoriales y en sus tres repositorios de referencia, **sin especificación normativa** | Verificado, con matiz |
| F-02 | Prefijo `_` en campos privados de instancia | `https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names` | Verificado |
| F-03 | Prefijos `s_` en campos estáticos privados y `t_` en `[ThreadStatic]` | `dotnet/runtime`, **no** las Framework Design Guidelines. `s_` está declarado como regla en su `.editorconfig`; `t_` solo en la prosa de sus guías | Verificado, con matiz |
| F-04 | Sufijo `Async` en métodos asíncronos | Task-based Asynchronous Pattern (`N-15`) | Verificado, con matiz |
| F-05 | Nombre de ensamblado igual al espacio de nombres raíz | Comportamiento por defecto del SDK | Verificado |
| F-06 | El estilo Allman aplicado sin excepción en el código de la plataforma | `.editorconfig` de `dotnet/runtime` (`O-08`), que **reafirma** la regla normativa de `N-06` | Verificado |
| F-07 | Carpeta `eng/` para infraestructura de build | Presente en `dotnet/runtime`, `dotnet/aspnetcore` y `dotnet/efcore` | Verificado |

### Repositorios de referencia

Tres repositorios de Microsoft se usan en esta guía como evidencia de lo que el ecosistema efectivamente hace, por oposición a lo que se le atribuye. Consultados el 2026-07-19, rama `main`:

| Repositorio | URL |
|-------------|-----|
| `dotnet/runtime` | `https://github.com/dotnet/runtime` |
| `dotnet/aspnetcore` | `https://github.com/dotnet/aspnetcore` |
| `dotnet/efcore` | `https://github.com/dotnet/efcore` |

### Precisión sobre F-01 — la convención `src` es sólida; la de `tests` no

Es el punto donde más se afirma de más, en dos niveles. Primero, respecto de la documentación:

- El tutorial *Organizing and testing projects with the .NET CLI* usa `/src` y `/test` y lo enmarca como recomendación con alternativa explícita: *«This tutorial recommends that you place the application project and test project in separate folders. Some developers prefer to keep these projects in the same folder.»* (`https://learn.microsoft.com/en-us/dotnet/core/tutorials/testing-with-cli`). Usa `test` en singular.
- N-09 escribe *«Suppose you have this standard solution structure»* con `\src` y `\test`, pero como supuesto de ejemplo para explicar el encadenamiento de `Directory.Build.props`, no como prescripción.

Segundo, y más importante, respecto de la práctica. La inspección de los tres repositorios de referencia desmiente que exista una convención única:

| | `src/` | Ubicación de los tests | Singular o plural |
|---|:---:|---|---|
| `dotnet/runtime` | sí | Anidados por componente: `src/libraries/System.Text.Json/tests/` | `tests` |
| `dotnet/aspnetcore` | sí | Anidados por componente: `src/Mvc/test/` | `test` |
| `dotnet/efcore` | sí | En la raíz, hermana de `src/` | `test` |

**Lo que sí está respaldado por los tres**: el código productivo vive bajo `src/`, la infraestructura de build bajo `eng/`, los tests están separados del código productivo, y las propiedades comunes se centralizan en `Directory.Build.props` en la raíz. Los tres tienen además `Directory.Build.targets`, `global.json`, `NuGet.config` y `.editorconfig` en la raíz.

**Lo que no está respaldado**: que la carpeta de tests viva en la raíz —dos de tres la anidan por componente— y que se escriba en plural —dos de tres usan singular—. La fórmula «`src/` + `tests/` en la raíz» es una decisión de equipo legítima, no una convención heredada, y presentarla como lo segundo es inexacto.

### Precisión sobre el alcance de F-01 en gestión de paquetes

Solo `dotnet/efcore` adopta la gestión centralizada de paquetes (`N-08`): su `Directory.Packages.props` declara `ManagePackageVersionsCentrally` en `true`. `dotnet/runtime` y `dotnet/aspnetcore` no tienen ese archivo y resuelven lo mismo mediante el sistema Arcade, con las versiones en `eng/Versions.props`.

Afirmar que la gestión centralizada es «lo que hacen los repositorios de Microsoft» sería inexacto. Lo correcto es que es una práctica moderna que uno de los tres adoptó, mientras los otros dos usan un mecanismo propio anterior. La recomendación de esta guía a favor de `N-08` se apoya en el mecanismo y sus beneficios, no en un argumento de autoridad por adopción.

### Precisión sobre N-17 — los controllers de otro ensamblado sí se descubren solos

Circula la creencia de que un controller alojado en un ensamblado distinto del de arranque exige registrar un *application part*. `N-17` muestra que no es así: el `ApplicationPartManager` incluye por defecto *«the app's assembly and dependent assemblies»*, de modo que una referencia de proyecto —directa o transitiva— basta para que el descubrimiento funcione.

`AddApplicationPart` hace falta cuando alguna de las tres condiciones que enumera `N-17` no se cumple: que `applicationName` apunte al ensamblado raíz de descubrimiento, que ese ensamblado raíz referencie las partes, y que referencie el Web SDK. Los casos reales son la carga dinámica de complementos, los ensamblados no referenciados en compilación y los arneses de prueba que alteran `applicationName`.

El análogo en Blazor sí es explícito: los componentes **enrutables** definidos en otro ensamblado requieren `AddAdditionalAssemblies` sobre `MapRazorComponents`, como muestra `N-20`. La asimetría entre ambos mecanismos es la que conviene retener.

### Precisión sobre F-04 — el sufijo `Async` no nace en el TAP

`N-15` documenta la convención en su sección *Naming, parameters, and return types*, y esa es la fuente citable. Pero la misma página deja ver que el sufijo es **anterior al TAP**: ya lo exigía el *Event-based Asynchronous Pattern*, y el TAP define una regla de desempate para cuando ambos conviven —si una clase ya expone un `GetAsync` del EAP, el método TAP se llama `GetTaskAsync`—.

Formulación correcta: el TAP **establece** la convención vigente del sufijo `Async` para métodos que devuelven tipos esperables. Es incorrecto decir que el sufijo «se origina» en el TAP. `N-15` documenta además una excepción: los combinadores (`WhenAll`, `WhenAny`) no lo llevan.

### Precisión sobre F-06 — Allman es normativo, no convención

La entrada figura en esta sección por su valor como evidencia, no porque el estilo sea opcional. `N-06` especifica Allman para C#, de modo que la regla es **normativa**; lo que aporta `F-06` es que el código de la plataforma la aplica sin excepción, con la regla declarada en su `.editorconfig`.

La distinción importa para una discusión de equipo: quien proponga K&R en C# no está eligiendo entre dos convenciones admitidas, está apartándose de `N-06`. Es legítimo hacerlo, y conviene saber que se está haciendo.

### Precisión sobre F-03 — el prefijo `s_`

Se atribuye con frecuencia a las guidelines de Microsoft. No están ahí: provienen del estilo interno de `dotnet/runtime`. La documentación de C# lo aclara de forma literal al señalar que no es el comportamiento predeterminado de Visual Studio ni parte de las Framework Design Guidelines. Adoptarlo es legítimo; atribuirlo a una norma general de .NET, no.

---

## 3. Obras de referencia

Libros y artículos citados. Ninguno es normativo para .NET; son el origen verificable de conceptos que la guía usa.

| # | Obra | Autoría y datos | Aporta |
|---|------|-----------------|--------|
| O-01 | *Framework Design Guidelines* | Cwalina, K.; Abrams, B. — 2.ª ed., Addison-Wesley, 2008. 3.ª ed.: Cwalina, Barton y Abrams, 2020 | Origen de N-01 a N-04 |
| O-02 | *Patterns of Enterprise Application Architecture* | Fowler, M. — Addison-Wesley, 2002 | Layered Architecture, Repository, Front Controller |
| O-03 | *Domain-Driven Design* | Evans, E. — Addison-Wesley, 2003 | Bounded Context, lenguaje ubicuo |
| O-04 | *Clean Architecture* | Martin, R. C. — Prentice Hall, 2017 | Regla de dependencia, límites |
| O-05 | Hexagonal Architecture (Ports and Adapters) | Cockburn, A. — 2005-09-04 · `https://alistair.cockburn.us/hexagonal-architecture/` | Puertos y adaptadores |
| O-06 | The Onion Architecture, part 1 | Palermo, J. — 2008-07-29 · `https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/` | Capas concéntricas |
| O-07 | Vertical Slice Architecture | Bogard, J. — 2018-04-19 · `https://www.jimmybogard.com/vertical-slice-architecture/` | Corte por funcionalidad |
| O-08 | C# Coding Style de `dotnet/runtime` | `https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md` | Origen de F-03 y F-06 |

**Nota sobre O-05.** La fecha corresponde al artículo publicado en el sitio de Cockburn. El autor ha mencionado que la idea es anterior; esa atribución no se verificó y la guía no la afirma.

**Nota sobre O-02.** El ISBN no se confirmó en fuente primaria; se cita autor, título, editorial y año.

---

## 4. Conceptos sin fuente normativa

Términos de uso corriente cuyo origen es difuso o cuya definición varía entre autores. La guía los usa señalando esta condición.

| Concepto | Situación |
|----------|-----------|
| Monolito modular | Uso extendido, sin definición canónica única. La guía fija la suya en [`TEM-MODU`](../10-Arquitectura-de-Servicios/Monolito-Modular.md) |
| Monolito distribuido | Término peyorativo de uso común, sin fuente normativa. Se usa como diagnóstico, no como categoría formal |
| N capas / N-Layer | Descrito en O-02 y en N-12, con variantes en el número de capas |
| Screaming Architecture | Atribuido a Robert C. Martin; concepto divulgado en su blog, no en O-04 |
| CQRS | Acrónimo atribuido a Greg Young, derivado del principio *Command-Query Separation* de Bertrand Meyer. Sin especificación normativa |
| Specification | Patrón descrito por Evans (O-03) y por Fowler en material posterior a O-02. Sin fuente primaria única verificada; se menciona sin atribuir |
| Monolith first | Posición atribuida a Martin Fowler. Artículo no verificado en fuente primaria; se cita por autoría, sin URL |
| Strangler Fig | Estrategia de migración incremental atribuida a Martin Fowler. Mismo criterio que la anterior |
| Ley de Conway | Melvin Conway, «How Do Committees Invent?», 1968. Cita no verificada en fuente primaria; se atribuye con esta reserva |

---

## 5. Cómo citar en esta guía

Los documentos temáticos referencian por el identificador de esta tabla —`N-02`, `F-01`, `O-06`— en lugar de repetir la URL. Cuando una afirmación no tiene respaldo en ninguna entrada, se declara como criterio propio con la fórmula «esta guía recomienda».

Al revisar la guía, toda afirmación normativa debería poder rastrearse hasta una fila de este anexo. Si no puede, o bien falta la fuente o bien la afirmación es criterio propio mal etiquetado.
