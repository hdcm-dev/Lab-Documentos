---
doc_id: ANEXO-99
doc_type: anexo
title: Anexos — glosario, fuentes, trazabilidad a folios, mapa guía↔demo y pendientes
status: borrador
origin: ia-generated
confidence: media
owner: Lab-Documentos
last_review: 2026-10-26
audience: [humano, agente]
traces: [GUIA-INDICE, MARCO-REFERENCIA, TEM-01, TEM-02, TEM-03, TEM-04, TEM-05, TEM-06, TEM-07, TEM-08]
---

# Anexos — glosario, fuentes, trazabilidad a folios, mapa guía↔demo y pendientes

## Índice

1. [Glosario](#1-glosario)
2. [Equivalencias de nombres del modelo](#2-equivalencias-de-nombres-del-modelo)
3. [Fuentes](#3-fuentes)
4. [Trazabilidad a folios](#4-trazabilidad-a-folios)
5. [Mapa guía ↔ demo](#5-mapa-guía--demo)
6. [Pendientes y NO VERIFICADO](#6-pendientes-y-no-verificado)
7. [Lista de verificación de la guía](#7-lista-de-verificación-de-la-guía)
8. [Insumos fuera del conjunto y apartamientos declarados](#8-insumos-fuera-del-conjunto-y-apartamientos-declarados)
9. [Revisión de consistencia](#9-revisión-de-consistencia)

## 1. Glosario

Glosario de **orientación**: cada fila da el texto de la definición, sin operación ni escena. La operación y la escena
de los cinco términos centrales están en [`00-Marco` §2](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/00-Marco.md#2-vocabulario-mínimo);
las del resto, en el tema que cita cada fila.

| Término | Definición | Qué NO es | Alias | Fuente |
| --- | --- | --- | --- | --- |
| **Blazor Hybrid** (de Microsoft) | Componentes Razor compilados dentro de la app MAUI y mostrados con `BlazorWebView` | No se puede bajar: es código de la app | `BlazorWebView` | P0 premisas, folio 035 |
| **Blazor WebAssembly en `HybridWebView`** | La forma de este framework: páginas Razor publicadas como Blazor WASM sin AOT, bajadas y servidas localmente por `HybridWebView` | No es Blazor Hybrid de Microsoft | «pantallas Blazor WebAssembly descargables», «A+datos» | P1 |
| **Cascarón** | App MAUI instalada por tienda: XAML propio, actualizador, servidor de contenido, puente, catálogo, base | No contiene pantallas de negocio | *shell* | P3 |
| **Contenido** | Paquete firmado con las pantallas de negocio | No incluye permisos ni código nativo | paquete | P1 |
| **Definición** | Encuesta como datos JSON con `encuestaId` y `version` | No es código; no requiere contenido nuevo | definición de encuesta | P1, P13 |
| **Canal** | Índice firmado por canal (`dev`, `beta`, `prod`) y mayor del puente | No es una versión | — | P2, P7 |
| **Índice del canal** | `indice.json` + `indice.json.sig`: entradas `contenido` y `definicion`, `expires`, `retiradas`, `keyId` | No es el manifiesto del paquete | índice firmado | P2 |
| **Manifiesto del paquete de contenido** | Lo que el contenido declara: `compatibleConPuente`, `capabilities {requires, optional}`, motor mínimo | No es el AndroidManifest | `declaracion-del-contenido.json` | P3, P12 |
| **Manifiesto de la app** | AndroidManifest / Info.plist: permisos y claves de uso; sólo cambia por tienda | No lo cambia un contenido | — | P12 |
| **Puente** | `Call(sobre)` JSON-RPC 2.0 + eventos, con `bridge.handshake` y `Listo(version)` | No son interfaces C# compartidas | *bridge* | P3 |
| **Contrato del puente** | Tabla de métodos + catálogo + transporte, fijado en `contrato.json` | No es la huella (dato de diagnóstico) | — | P3 |
| **Catálogo de capacidades** | Lista constante del cascarón, una entrada por capacidad con versión propia | No es la lista de permisos | — | P12 |
| **Predicado de compatibilidad** | Puente en rango ∧ `requires` en catálogo con rango ∧ motor ≥ mínimo | No es `minShellVersion` | — | P3 |
| **Semilla** | Contenido y definición embebidos en el APK, firmados igual | No es destino de rollback | *seed* | P2 |
| **Staging** | Carpeta donde queda una versión bajada y verificada hasta el próximo arranque en frío | No es la versión activa | — | P2 |
| **Rollback por marca** | Vuelta a `previous` cuando la versión activada no llama `Listo` en 2 cargas | No es rollback remoto | — | P2 |
| **Retirada** | Versión listada en el índice firmado para no usarse | No es borrarla del servidor | — | P2, P13 |
| **Outbox** | Tabla del cascarón con las acciones pendientes de envío, con dueño e idempotencia | No se borra al cerrar sesión | bandeja de salida | P5 |
| **Carta muerta** | Respuesta rechazada que queda visible al usuario | No se descarta | *dead letter* | P5, P13 |
| **Compuerta** | Pruebas que un PR debe pasar; se resumen en `fw-ok` | No es la cobertura | *gate* | P8 |
| **`fw-ok`** | Job agregador estricto, único check requerido de `main` | No es un job de pruebas | — | P6 |
| **Runner** | Máquina que corre el workflow; acá `[self-hosted, i7infra-dev]` (`i7infra-runner-dev`) | No tiene `docker.sock` ni KVM | — | P6, R2 |
| **Rojo provocado** | Cambio mínimo que debe hacer fallar una suite | No es un error real | — | P8 |
| **TPR** | Tiempo al primer render del contenido: marca `primer-render` desde el inicio del proceso | No es TTFD | — | P1 |
| **Estado de red combinado** | `EnLinea`, `EnLineaMedida`, `RedSinServidor`, `SinRed` | No es sólo `Connectivity` | — | Folio 038 |
| **Clase de acción** | `SoloLocal`, `Encolable`, `SoloEnLinea` (y `SoloWiFi`) | No la decide el contenido | — | Folio 038 |
| **Desbloqueo local** | Entrada con la seguridad del teléfono que abre el refresh token sellado | No es el login | — | P4 |
| **Arranque en frío** | Abrir la app con su proceso cerrado (`am force-stop` y volver a abrir); es cuando se activa lo que quedó en staging | No es volver a la app desde las recientes | — | P2; `guion.sh` L329 |
| **PO** | El dueño del producto: pidió la guía y decide lo que la mesa le eleva | No es un miembro de la mesa | *product owner* | [Folio 001](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/001-presentacion-pedido-del-po.md) |
| **Folio** | Cada actuación numerada del expediente 0001 (informes, providencias, resoluciones) | No es una sección de la guía | actuación | §4 |
| **P1…P13** | Las trece decisiones de la mesa evaluadora, en la tabla del folio 037 | No son prioridades | decisiones | [Folio 037](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/037-resolucion-plan-de-aplicacion-y-cierre-de-la-mesa.md) |
| **E01, E02, E03** | Los tres ensayos que corrieron la técnica: WASM descargado en WebView, integración de la demo, red y autenticación | No son pruebas automáticas | ensayos | §4 |
| **r01…r18** | Rojos provocados de la compuerta, numerados en E02 `rojos-compuerta.md` | No son fallas abiertas | — | [`rojos-compuerta.md`](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E02-Demo-Integracion/rojos-compuerta.md) |
| **M-12, M-13** | Dos cambios del código postal a cinco dígitos: M-12 cambia sólo la constante y queda en verde; M-13 cambia el patrón de `\d{4}` a `\d{5}` y deja `Core.Tests` en rojo, que es el rojo con que se prueba la protección de `main` | No son versiones | — | [`etapas/README.md`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/etapas/README.md) |
| **PKCE** | `code_verifier` + `code_challenge` S256 que protege el canje del `code` | No es un secreto de cliente | — | RFC 7636 |

## 2. Equivalencias de nombres del modelo

| Nombre en la guía | En el expediente | Estado |
| --- | --- | --- |
| Pantallas Blazor WebAssembly descargables más encuestas como datos | «A+datos» (folio 021), contrato «híbrido (C)», I0 «C» | **Elegido** (P1) |
| Renderizador compilado en el cascarón + encuestas como datos | «C-min» | Camino de reserva si el costo no convence o el TPR supera el umbral |
| Sólo interfaz por datos | «B pura» | Descartada |
| Sólo WASM, encuestas compiladas | «A pura» | Descartada |
| Sitio remoto en vivo | «D» | Descartada (no opera sin red) |

## 3. Fuentes

Sólo las leídas por la mesa o por esta guía. Fecha = consulta.

| Id | Fuente | Fecha |
| --- | --- | --- |
| F-01 | Apple, *App Review Guidelines* (2.5.2, 4.7): https://developer.apple.com/app-store/review/guidelines/ | 2026-10-25 |
| F-02 | Apple, *Developer Program License Agreement* §3.3.1: https://developer.apple.com/support/terms/apple-developer-program-license-agreement/ | 2026-10-25 |
| F-03 | Google, *Device and Network Abuse*: https://support.google.com/googleplay/android-developer/answer/9888379?hl=en | 2026-10-25 |
| F-04 | Microsoft, *HybridWebView – .NET MAUI*: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-10.0 | 2026-10-25 |
| F-05 | Microsoft, *Blazor WebAssembly build tools and AOT*: https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-build-tools-and-aot?view=aspnetcore-10.0 | 2026-10-25 |
| F-06 | IETF, *RFC 8252*: https://www.rfc-editor.org/rfc/rfc8252 | 2026-10-25 |
| F-07 | IETF, *RFC 7636*: https://www.rfc-editor.org/rfc/rfc7636 (citada a través de N2) | 2026-10-25 |
| F-08 | GitHub, *GitHub flow*: https://docs.github.com/en/get-started/using-github/github-flow | 2026-10-25 |
| F-09 | *Trunk Based Development*: https://trunkbaseddevelopment.com/ | 2026-10-25 |
| F-10 | GitHub, *About protected branches*: https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches | 2026-10-25 |
| F-11 | GitHub, *Troubleshooting required status checks*: https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/collaborating-on-repositories-with-code-quality-features/troubleshooting-required-status-checks | 2026-10-25 |
| F-12 | GitHub, *Secure use reference*: https://docs.github.com/en/actions/reference/security/secure-use | 2026-10-25 |
| F-13 | *Semantic Versioning 2.0.0*: https://semver.org/ | 2026-10-25 |
| F-14 | Vocke, *The Practical Test Pyramid*: https://martinfowler.com/articles/practical-test-pyramid.html | 2026-10-25 |
| F-15 | Luo y otros, *An empirical analysis of flaky tests* (FSE 2014): https://doi.org/10.1145/2635868.2635920 | 2026-10-25 |
| L-01 | Borrador del PO, `Framework-APP.md` (insumo fuera del conjunto, §8; no se edita) | 2026-10-26 |
| L-02 | Prompt `Crear-Framework-Concept.md` | 2026-10-26 |
| L-03 | Dossiers N1–N5, relevamientos R1–R4, investigación I0–I7 (`OUTPUTs/Bitacoras/`) | 2026-10-26 |
| L-04 | Ensayos E01, E02, E03 (`OUTPUTs/Ensayos/`) | 2026-10-26 |
| L-05 | Demo, árbol de trabajo del 2026-10-26 (la carpeta `Demo/` todavía no tiene commit en el repositorio) | 2026-10-26 |
| C-01 | `scripts/ci-local.sh --hasta 4` sobre una copia de la demo: Core 211/211, Update 102/102, Gate 52/52, `rc=0` | 2026-10-26 |
| C-02 | Ídem con `etapas/M-13-codigo-postal-cinco-digitos.patch`: Core.Tests 6 con error de 211, `rc=1` | 2026-10-26 |
| C-03 | Ídem con `etapas/aviso-promovido-a-error.patch`: `error CS0219`, `rc=1` | 2026-10-26 |
| C-04 | `scripts/guion.sh humo` sobre la copia: «humo: 47 OK, 0 MAL» | 2026-10-26 |

Las rutas locales: prompt y bitácoras bajo
[`/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/`](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/Crear-Framework-Concept.md);
la demo bajo [`/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/README.md).

## 4. Trazabilidad a folios

Expediente 0001, carpeta
[`actuaciones/`](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/037-resolucion-plan-de-aplicacion-y-cierre-de-la-mesa.md).

| Decisión | Folio | Documento de la guía |
| --- | --- | --- |
| P1 Modelo de contenido | 035, 036 | `TEM-01` |
| P2 Canal de actualización | 035, 006 (forzado) | `TEM-01` |
| P3 Puente y compatibilidad | 035 | `TEM-02` |
| P4 Entrada del usuario | 035, 038 (entra a la demo) | `TEM-07` |
| P5 Datos locales | 035, 023 (C-1) | `TEM-02` |
| P6 Procedimiento de PR | 035, 023 (proteger `main`), 007 (delegación del merge) | `TEM-03`, `TEM-06` |
| P7 Líneas de versión | 035, 023 (atribución) | `TEM-03`, `TEM-06` |
| P8 Compuerta | 035 | `TEM-04` |
| P9 iOS | 035, 023 | `TEM-02`, `TEM-03`, Developer-Guide |
| P10 Estructura de la guía | 035 | Este conjunto (ver `TEM-08` §6) |
| P11 Alcance de la demo | 035 | `TEM-05`, Developer-Guide |
| P12 Capacidades | 035, 004 (señal del PO desde una app híbrida de referencia de la organización) | `TEM-02` |
| P13 Encuestas entre versiones | 035, 005 | `TEM-02` |
| Plan A1–A5 y cierre de la mesa | 037 | Developer-Guide, «Estado» |
| Red y autenticación en la demo | 038 | `TEM-07` |

Enlaces a los folios y ensayos citados:

| Cita | Documento |
| --- | --- |
| Folio 004 | [Señales del PO: actualización al loguear y manifiesto](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/004-providencia-senales-del-po-actualizacion-al-loguear-y-manifiesto.md) |
| Folio 005 | [Escenario de la encuesta](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/005-providencia-escenario-de-la-encuesta.md) |
| Folio 006 | [Actualización forzada al iniciar](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/006-providencia-actualizacion-forzada-al-iniciar.md) |
| Folio 007 | [Delegación del merge](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/007-providencia-delegacion-del-merge.md) |
| Folio 021 | [Veredicto del jurado, ronda 1](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/021-informe-jurado-veredicto.md) |
| Folio 023 | [Respuesta del PO al lote de la ronda 1](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/023-providencia-respuesta-del-po-al-lote-de-la-ronda-1.md) |
| Folio 035 | [Veredicto del jurado, ronda 2](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/035-informe-r2-jurado-veredicto.md) |
| Folio 036 | [Respuesta del PO al lote de la ronda 2](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/036-providencia-respuesta-del-po-al-lote-de-la-ronda-2.md) |
| Folio 037 | [Resolución: plan de aplicación y cierre de la mesa](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/037-resolucion-plan-de-aplicacion-y-cierre-de-la-mesa.md) |
| Folio 038 | [Autenticación del SO y manejo de red en la demo](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/038-providencia-autenticacion-del-so-y-manejo-de-red-en-la-demo.md) |
| E01 | [INFORME](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E01-WASM-Descargado-En-WebView/INFORME.md) |
| E02 | [INFORME](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E02-Demo-Integracion/INFORME.md), [correcciones](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E02-Demo-Integracion/correcciones.md), [rojos de la compuerta](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E02-Demo-Integracion/rojos-compuerta.md), [rojos del actualizador](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E02-Demo-Integracion/rojos-actualizador.md) |
| E03 | [INFORME](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E03-Red-Y-Autenticacion/INFORME.md), [correcciones](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E03-Red-Y-Autenticacion/correcciones.md) |

Los folios y ensayos viven en el repositorio de documentación del laboratorio, no en el de la guía: desde el
repositorio publicado de la guía, estos enlaces se resuelven contra el espacio de trabajo completo.

## 5. Mapa guía ↔ demo

| Tema | Archivos de la demo |
| --- | --- |
| Actualizador y canal | `src/FrameworkConcept.Core/Update/`, `Contracts/Canal/`, `src/FrameworkConcept.Shell/Actualizacion/` |
| Servidor de contenido | `src/FrameworkConcept.Shell/Hosting/ServidorDeContenido.cs` |
| Puente | `Contracts/Puente/`, `Contracts/contrato.json`, `Shell/Bridge/`, `Content/wwwroot/js/puente.js` |
| Capacidades | `Contracts/Puente/CatalogoDeCapacidades.cs`, `Content/declaracion-del-contenido.json` |
| Encuestas | `definiciones/encuesta-movilidad/`, `Content/Survey/`, `Core/Application/Definiciones/DefinicionValidator.cs` |
| Base y outbox | `Shell/Data/`, `Shell/Sync/` |
| Red y acciones | `Core/Red/`, `Shell/Red/`, `Content/Red/` |
| Sesión | `Core/Auth/`, `Shell/Auth/`, `Content/Auth/`, `infra/keycloak/` |
| Versionado | `versionado/`, `Contracts/VersionSemantica.cs`, `Contracts/RangoDeVersion.cs` |
| Compuerta | `tests/`, `scripts/ci-local.sh`, `etapas/`, workflow en `.github/workflows/` de la raíz del repositorio |
| Valores de producto | `Contracts/ParametrosDeProducto.cs` |

## 6. Pendientes y NO VERIFICADO

| Pendiente | Motivo | Fuente |
| --- | --- | --- |
| **TPR en el moto g42** en la condición declarada (máximo de 5 corridas ≤ 5 000 ms) — NO VERIFICADO | El g42 quedó reservado para otra sesión; el intento anterior midió 8,2–8,7 s y la corrección sacó ~0,7 s; el estimado (~7,5 s) seguiría fuera | E02 §4.3, §5 H-A, §7 |
| **Apartamiento H-1**: una huella nueva no invalida la clave | Decisión de mesa: se mantiene P4 (PIN o patrón), avisado al PO | E03 §5 H-1 |
| **iOS** — NO VERIFICADO | Sin Mac, sin cuenta de Apple Developer, sin job `macos-*` (folio 023) | P9 |
| `.wasm` ante Apple y ante Play — NO VERIFICADO | Ninguna política nombra WebAssembly | P1, N1 §4.1 |
| Corrida del workflow con `run_id` en `i7infra-runner-dev` — NO VERIFICADO | Tramo A5 | Folio 037 |
| Protección de `main` y su prueba con M-13 — pendiente | Tramo A5, después de un `fw-ok` verde | Folio 037, P6.1 |
| Operación sin red en el teléfono de medida (criterio 3 de P11) — NO VERIFICADO | Sólo emulador | E02 §7, P0 premisas |
| `guion.sh demo` como un solo comando — NO VERIFICADO | Se corrieron sus pasos uno por uno | E02 §7 |
| Sobre no ASCII en el dispositivo tras el arreglo — NO VERIFICADO | La encuesta de E02 fue ASCII | E02 §7 |
| Reenvío inverso de `adb` como camino de red — NO VERIFICADO | Con servidor adb remoto el reenvío llega a otra máquina | README de la demo |
| `EnLineaMedida` en dispositivo; R0 sin cuenta en el emulador — NO VERIFICADOS | El emulador no tiene datos móviles; R0 no se corrió | E03 §7 |
| Consulta del canal disparada por el ingreso (`ESC-6`) — NO VERIFICADO | Ningún registro de E03 la muestra | Este anexo |
| Actualización obligatoria (`ESC-7`) — no ejecutada | La demo no publica un `mandatory` | Este anexo |
| `Permissions-Policy` en `HybridWebView` — NO VERIFICADO | Sin ensayo | P12, folio 037 |
| Aceptación de versiones viejas en ODK Central — NO VERIFICADO | Opinión autorizada, no doc oficial | P13 |
| Recorrido desde un clon limpio siguiendo sólo la guía — NO VERIFICADO | Esta guía corrió la compuerta sobre una copia del árbol de trabajo, no sobre un clon | P10 |
| Comentario de `CofreDeSesion.cs` L128 contradice H-1 | Hallado al escribir esta guía | `TEM-07` §8 |
| Registro en la ia-db de `Lab-Documentos.Documentacion` (OB-IDX-02) — pendiente | Se hace con la entrega por PR | Tramo A5, folio 037 |
| Fragmentos citados sin commit — pendiente | `Demo/` no tiene commit; cada `*Fuente:*` fija la versión con el `sha256` del archivo hasta que la entrega por PR dé un commit | Tramo A5 |
| Paso 0 de la compuerta en un clon donde `Demo/` no está en `origin/main` — NO VERIFICADO | Esta guía corrió sobre una copia fuera de git | `Developer-Guide` §6, ejercicio 2 |
| `Framework-APP.md` nombra un repositorio privado — decisión del PO | Es su borrador y no se edita; la guía no lo enlaza (§8) | Bitácora 03 |
| Valores provisorios (vigencia 30 d, renovación 7 d, reanudación 15 min, aviso de retiro 30 d) | Criterio del jurado, confirmados por el PO en el folio 036 como valores de producto | `ParametrosDeProducto.cs` |

## 7. Lista de verificación de la guía

Marcada el 2026-10-26 con la revisión de §9.

- [x] Cada documento temático tiene Resumen ejecutivo, Definición, Aplicación por escenario, Ejemplos concretos,
  Preguntas guía, Criterios de calidad y Anexo (`TEM-06` con la variante declarada en §8).
- [x] Cada escenario `ESC-1`…`ESC-7` aparece en la tabla de aplicación de cada tema, o se dice por qué no aplica.
- [x] Cada fragmento de código de la demo lleva su línea `*Fuente: … L<a>–L<b>*` con el `sha256` del archivo.
- [x] Cada NO VERIFICADO lleva su motivo.
- [x] Ningún enlace es relativo al repositorio; todos parten de la raíz del espacio de trabajo.
- [x] Ninguna clave privada ni nombre de repositorio privado en los documentos del conjunto (`Framework-APP.md` queda
  fuera, §8). Los nombres de runner y emulador se mantienen: están en el workflow publicado.
- [x] `Framework-APP.md` no se editó.
- [x] Un único índice: `Developer-Guide.md` (P10); `README.md` es portada.

## 8. Insumos fuera del conjunto y apartamientos declarados

| Id | Qué | Declaración |
| --- | --- | --- |
| X-1 | `Framework-APP.md` | Borrador del PO en la misma carpeta. **No forma parte del conjunto**: no tiene frontmatter, tiene un enlace a un archivo que no existe y nombra un repositorio privado. No se edita (es del PO) ni se enlaza desde la guía; la decisión de dejarlo, renombrar lo privado o sacarlo es del PO |
| X-2 | Variante de `TEM-06` | Agrega §8 «Procedimientos» después de los siete apartados comunes; Observaciones y Referencias pasan a §9 y §10 |
| X-3 | Estructura plana | El marco y los anexos son un archivo cada uno (`00-Marco.md`, `99-Anexos.md`), no carpetas `00-Marco-de-Referencia/` y `99-Anexos/` (OB-EST-03, OB-EST-07, OB-LAB-01, OB-LAB-02). P10 no exige carpetas y el conjunto es chico |
| X-4 | Enlaces desde la raíz del espacio de trabajo | OB-MD-08 pide rutas relativas; rige la regla del despacho de enlazar desde la raíz, porque los clones viven en rutas distintas. Precedencia: la regla del despacho sobre OB-MD-08 |
| X-5 | Glosario sin operación ni escena | Rotulado como orientación (§1); operación y escena en `00-Marco` §2 y en los temas |

## 9. Revisión de consistencia

Revisión del 2026-10-26, con tres revisores: obligaciones normativas (R3 y el perfil de guía de estudio), un lector sin
contexto que corrió los ejercicios 1 a 7 sobre una copia de la demo, y un crítico de coherencia. El cuadro completo
hallazgo → acción está en la
[bitácora 03](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Bitacoras/03-Verificacion-De-La-Guia.md). Resultado:

| Revisión | P0 | P1 | P2 | Estado |
| --- | --- | --- | --- | --- |
| Obligaciones (R3, perfil) | 0 | 3 | 6 | Aplicados; ia-db pasa a pendiente del tramo A5 (§6) |
| Lector sin contexto | 0 | 1 | 4 | Aplicados en `Developer-Guide` §5.3 y §6 |
| Coherencia y veracidad | 0 | 1 | 3 | Aplicados; los nombres de runner y emulador se mantienen (§7) |
