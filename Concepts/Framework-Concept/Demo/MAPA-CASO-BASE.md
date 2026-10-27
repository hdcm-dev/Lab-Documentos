# Mapa del caso base

La demo no parte de cero: toma el dominio, las reglas y las pruebas de la app **MovilidadUrbana.MAUI** del
laboratorio Lab-E2E.WebBlazor (P11: «se conservan dominio, reglas y pruebas de ViewModels, copiadas y no
referidas, con mapa archivo por archivo»). Este archivo dice, para cada archivo copiado, de dónde sale y qué
se le cambió.

**Origen.** Repositorio `LAB/Lab-E2E.WebBlazor/Lab-E2E.WebBlazor` (rutas desde la raíz del workspace), commit
`8fb4c3a`, sin cambios locales en las carpetas copiadas (`git status --short` vacío el 2026-10-25). La columna
«blob» es `git rev-parse --short=8 8fb4c3a:<ruta>`: con ella se puede recuperar el original exacto.

**Por qué copiar y no referenciar.** No compartir proyectos entre apps es una regla del laboratorio, y la demo vive
en otro repositorio (relevamiento R1 §14.5, punto 4). Además el proyecto original es `net10.0-android`: no se puede
referenciar desde `net10.0` sin el workload de Android.

**Qué cambia siempre.** El espacio de nombres: `MovilidadUrbana.MAUI.` → `FrameworkConcept.Core.`. Nada más, salvo
donde la tabla dice «adaptado».

## Dominio y aplicación → `src/FrameworkConcept.Core/`

Origen: `src/MovilidadUrbana.MAUI/` en el commit `8fb4c3a`. Destino: la misma ruta relativa bajo
`src/FrameworkConcept.Core/`.

| Archivo | Blob | Cómo | Por qué |
| --- | --- | --- | --- |
| `Domain/Catalogos.cs` | `3764586e` | Tal cual | Provincias, medios, frecuencias y motivos. La definición v1 repite sus claves y etiquetas; una prueba lo comprueba |
| `Domain/Entities/Localidad.cs` | `8e232f7e` | Tal cual | — |
| `Domain/Entities/Session.cs` | `5a5e953b` | Tal cual | La usará la sesión por dispositivo del cascarón (P4, demo sin login) |
| `Domain/Entities/RespuestaDeEncuesta.cs` | `dc1a0205` | **Adaptado** | Suma `EncuestaId` y `VersionDefinicion`: cada respuesta guarda con qué definición se tomó (P13.2) |
| `Domain/Rules/EncuestaRules.cs` | `93331c50` | Tal cual | Rangos de la encuesta. La definición v1 los repite como datos; una prueba lo comprueba |
| `Domain/Rules/LocalidadRules.cs` | `da4f294f` | Tal cual | Incluye el `[GeneratedRegex]` del código postal |
| `Application/Result.cs` | `d3cbc5d1` | Tal cual | — |
| `Application/DependencyInjection.cs` | `bf444508` | **Adaptado** (sólo comentario) | Decía «la web y la API»; en la demo lo invoca el contenido Blazor |
| `Application/Abstractions/ISessionContext.cs` | `06b7c815` | **Adaptado** (sólo comentario) | Decía «depende de cookies y de HTTP»; en la demo la implementa el cascarón |
| `Application/Abstractions/ILocalidadRepository.cs` | `33e35e7a` | Tal cual | Costura para que el contenido no conozca el motor: se implementará llamando al puente (R1 §14.2) |
| `Application/Abstractions/IEncuestaRepository.cs` | `6eebb2b6` | Tal cual | Ídem |
| `Application/Localidades/LocalidadModel.cs` | `253750bb` | Tal cual | — |
| `Application/Localidades/LocalidadService.cs` | `05839def` | Tal cual | — |
| `Application/Localidades/LocalidadPolicy.cs` | `1653ecc1` | Tal cual | — |
| `Application/Encuestas/EncuestaModel.cs` | `80c05460` | **Adaptado** | Suma `EncuestaId` y `VersionDefinicion`, que se fijan al empezar a cargar y no cambian aunque llegue otra versión (P13.2) |
| `Application/Encuestas/EncuestaService.cs` | `07b41abb` | **Adaptado** | `RegistrarAsync` copia la definición a la respuesta y rechaza una encuesta sin definición (P13.2). `ValidarPaso` no cambia |
| `Application/Encuestas/EncuestaPolicy.cs` | `8925e85b` | Tal cual | Los requisitos de la definición v1 son estos textos; una prueba lo comprueba |
| `Application/Encuestas/ResumenDeEncuesta.cs` | `696275e4` | Tal cual | — |

Archivos **nuevos** de la demo en las mismas capas (no salen del caso base):

| Archivo | Qué es |
| --- | --- |
| `Application/Definiciones/DefinicionValidator.cs` | Valida un paso de una encuesta escrita como datos. Con la v1 da los mismos errores que `EncuestaService.ValidarPaso` (probado) |

## Lo que no se copió

| Del caso base | Por qué no | Adónde va en la demo |
| --- | --- | --- |
| `Infrastructure/` (EF Core sobre SQLite, `SessionSeeder`, repositorios) | La base es del cascarón (P5), con esquema fijo C-1 (folio 023) | `src/FrameworkConcept.Shell/Data/` (tramo A2). Las dos localidades sembradas (`SessionSeeder.cs`, blob `7ea1a0c3`) se repiten en el doble de prueba de `tests/FrameworkConcept.Core.Tests/TestEnvironment.cs` |
| `Presentation/` (ViewModels con CommunityToolkit.Mvvm) | Las pantallas pasan de XAML a Razor (P1); el filtro de la lista, los estados de lista y la navegación son de la vista | Páginas Razor de `src/FrameworkConcept.Content/` (tramo A2) |
| `Pages/`, `Controls/`, `Services/`, `MauiProgram.cs` | Controles nativos y correcciones de Android que en un WebView no aplican (R1 §14.4) | — |
| `tests/MovilidadUrbana.MAUI.UITests` (Appium) | Localiza `resource-id` nativos; el contenido es DOM dentro de un WebView | — (R1 §14.4, NO VERIFICADO cómo se prueba la UI Razor en el WebView) |

## Pruebas → `tests/FrameworkConcept.Core.Tests/`

Origen: `tests/MovilidadUrbana.MAUI.Tests/` en el commit `8fb4c3a` (18 casos sobre ViewModels, R1 §12.1). Las pruebas
de ViewModels no se pueden copiar tal cual, porque los ViewModels no viajan; se **adaptaron** a los servicios de
aplicación, que son los que deciden las reglas: mismos datos de entrada y mismos mensajes.

| Origen | Blob | Destino | Cómo |
| --- | --- | --- | --- |
| `TestEnvironment.cs` | `77f7071b` | `TestEnvironment.cs` | **Adaptado:** repositorios en memoria en lugar de EF Core sobre un SQLite temporal; mismas dos localidades sembradas; misma cultura `es-AR` |
| `EncuestaViewModelTests.cs` (7 casos) | `1310b64b` | `EncuestaServiceTests.cs` | **Adaptado:** paso 1 inválido, paso 2 corregido, alternar medios, registrar con resumen («Colectivo, Bicicleta», «12,5 km») y paso 3 fuera de rango. Salen los casos que sólo eran de la vista (etiqueta «Paso 1 de 3», volver atrás, «cargar otra») y entran dos de P13.2 |
| `LocalidadEditorViewModelTests.cs` (6 casos) | `54a77d88` | `LocalidadServiceTests.cs` | **Adaptado:** errores por campo, corregir un campo, alta, modificación y baja. Sale la confirmación cancelada (era del diálogo); entra el rechazo de duplicados |
| `LocalidadesViewModelTests.cs` (5 casos) | `f03c35c9` | `LocalidadServiceTests.cs` | **Adaptado:** sólo «arranca con las sembradas». El filtro, el orden por nombre y los estados de la lista son de la vista: van con las páginas Razor |
| — | — | `ReglasTests.cs` | **Nuevo:** bordes de `LocalidadRules` y `EncuestaRules` (R1 §3.3) |
| — | — | `DefinicionTests.cs` | **Nuevo:** la definición v1 coincide con el dominio; el validador de definiciones equivale a `ValidarPaso`; v2 es evolución compatible (P13.8) |
| — | — | `ContratoTests.cs` | **Nuevo:** versiones SemVer, rangos con techo, forma del índice, plazo de aviso de retiro |
