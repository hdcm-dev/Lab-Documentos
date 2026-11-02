---
doc_id: FC-ADR-0001
doc_type: proceso
title: ADR-0001 — Contenedor nativo en lugar de «cascarón»
description: Decisión de nombrar «contenedor nativo» (native shell) a lo que la jerga del proyecto llamaba «cascarón», con su regla de uso y su alcance.
status: vigente
origin: ia-generated
confidence: media
owner: Lab-Documentos
last_review: 2026-11-02
audience: [humano, agente]
traces: [FC-DECISIONES, FC-GUIA-LIBRO, FC-ANEXO-99, GUIA-PORTADA]
---

# ADR-0001 — Contenedor nativo en lugar de «cascarón»

**Estado:** aceptado · **Fecha:** 2026-11-02

## 1. Contexto

«Cascarón» nació como palabra coloquial del PO para nombrar la app .NET MAUI que se instala desde la tienda y
aloja el contenido. De ahí pasó al libro, al Conocimiento, a la demo y a los nombres de archivos y de código.
Ninguna fuente pública define el término; en cambio, *hybrid app* y *native wrapper* tienen respaldo
bibliográfico, y *native shell* ya se usaba en el libro y en el glosario del Anexo A.

La mesa de expertos del 2026-11-01 decidió el término y su regla de uso. El acta está en
Lab-Documentos.Documentacion, PROMPTs/Guides/16-IA-Layer/OUTPUTs/Bitacoras/Prompt-02/Terminologia/Mesa-Acta.md,
junto con el relevamiento de fuentes (`Termino-Cascaron.md`) y la respuesta de FC. La decisión quedó aprobada
5/5, sin vetos, y pide a FC un ADR propio con la decisión y el riesgo del *Shell*.

## 2. Decisión

| Español | Inglés | Uso |
|---|---|---|
| **contenedor nativo** | *native shell* (sinónimo: *native wrapper*) | La app MAUI que se instala desde la tienda: aloja el WebView, el puente nativo y el verificador de firma, y no trae las pantallas de negocio. |
| **app híbrida con contenido OTA** | *hybrid app with OTA content* | El modelo completo. No es Blazor Hybrid, que compila las pantallas dentro del binario. |
| **contenido web** | *web content* | Las pantallas de negocio en Blazor WASM que corren en el WebView. |
| **paquete de actualización OTA** | *OTA update bundle* | La unidad que se distribuye: se descarga, se verifica la firma y se sirve desde el disco. |
| cascarón (coloq.) | — | Sólo remisión al término principal. |

Se descartan «App Shell model» (falso amigo: en Google designa la cáscara de una app web), «contenido web
remoto» (el contenido se sirve desde el disco del teléfono) y «sitio empaquetado».

**Regla de uso.**

1. Primera mención en cada documento: «contenedor nativo (*native shell*; en la jerga del proyecto,
   "cascarón")» para la app, y «contenido web, distribuido como paquete de actualización OTA firmado» para el
   contenido.
2. Después, siempre «contenedor nativo», nunca «contenedor» solo. «Cascarón» no vuelve a aparecer en prosa.
3. Glosario (Anexo A del libro): «contenedor nativo (*native shell*; sin. *native wrapper*)» con la nota «no
   confundir con `Microsoft.Maui.Controls.Shell`», y «cascarón (coloq.) → véase contenedor nativo».

## 3. Alcance

**Se renombra la prosa de:**

- `MAUI-Framework-Developer-Guide.md` (el libro).
- `Conocimientos/Knowledge-App-Hibrida-OTA-Offline-First.md` y `Conocimientos/README.md`.
- `README.md` de Framework-Concept.
- Los `.md` de `Demo/`: `README.md`, `DISENO-RED-Y-AUTENTICACION.md`, `MAPA-CASO-BASE.md` y `etapas/README.md`.

**No se toca, y por qué:**

| Qué | Razón |
|---|---|
| Los capítulos fuente de la guía anterior, de `00-Marco.md` a `08-Cohesion-Global.md`, y `Developer-Guide.md` | El libro los cita literalmente y por número de línea; reescribirlos rompe esas citas. |
| `99-Anexos.md`, salvo la mención «App Shell model» de su glosario | Mismo motivo que los capítulos fuente; el acta pide quitar el falso amigo y sólo eso. |
| `Framework-APP.md` | Es el borrador original del PO: se conserva como documento de origen. |
| El código (`.cs`, `.razor`, `.js`, `.csproj`, `.props`, `.json`, `.patch`, `.sh`) | El acta excluye el código; renombrarlo es otra decisión, con pruebas y versionado propios. |
| Nombres de archivos, carpetas e identificadores (`02-Cascaron-Puente-Y-Capacidades.md`, `PuenteDelCascaron`, `version-cascaron.props`, `error-de-compilacion-en-el-cascaron.patch`, `FrameworkConcept.Shell`…) | Cambiarlos rompe enlaces, citas por ruta, el build y la CI. |
| Citas literales (texto entre «…» que transcribe una fuente, bloques que copian código o comentarios) | Una cita transcribe la fuente tal como está. |
| Bitácoras | Son registro histórico. |
| Dos evidencias selladas de DocsAsCode del expediente 0003 que copian «cascarón» | Están selladas y son territorio de DocsAsCode; copian el comentario de `01-Actualizacion-Sin-Tienda.md:235`, que tampoco se toca. |

El renombre de la prosa va en un PR aparte, revisado a mano, después de que fusionen los PR de las mesas
DocsAsCode en curso. Antes de ese PR se quita «App Shell model» del `README.md`, del libro y de `99-Anexos.md`.

## 4. Consecuencias

- El lector nuevo encuentra un término con respaldo bibliográfico, y el coloquial queda como aclaración en la
  primera mención y en el glosario.
- Conviven el término nuevo en la prosa y «cascarón» en nombres de archivos, identificadores, código, citas y
  capítulos fuente. Es deliberado: la primera mención y la remisión del glosario sirven de puente.
- El nombre no cambia lo que exigen las tiendas: Apple 2.5.2, 4.7 y 4.7.2 y la regla de Google sobre código
  interpretado rigen igual. La 4.7.2 afecta al puente y se agregó al libro y al Conocimiento.

## 5. Riesgo

`FrameworkConcept.Shell` no se renombra, y *native shell* puede confundirse con `Microsoft.Maui.Controls.Shell`,
el componente de navegación de .NET MAUI. Son cosas distintas: el contenedor nativo es la app entera; el *Shell*
de MAUI es una plantilla de navegación que esta demo no usa para las pantallas de negocio.

**Mitigación:** la entrada del glosario lleva la nota «no confundir con `Microsoft.Maui.Controls.Shell`». No se
renombra código por este riesgo.
