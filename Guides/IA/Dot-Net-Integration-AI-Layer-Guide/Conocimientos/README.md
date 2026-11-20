# Conocimientos de la capa de IA en .NET: mudados

Los ocho conocimientos que vivían en esta carpeta se mudaron el 2026-11-19 a la base de conocimientos de la
organización (repositorio privado), con versión 2.0. Acá no queda copia. Los libros siguen
acá: [`Dot-Net-Integration-AI-Layer-Guide.md`](../Dot-Net-Integration-AI-Layer-Guide.md) y
[`Conversational-Design-Framework-Guide.md`](../Conversational-Design-Framework-Guide.md).

| Id | Alias en la base | Para qué sirve |
| --- | --- | --- |
| K1 | `AI-Layer-Integration-Designing-Specification` | Decidir si hace falta IA y especificarla: arquitectura, ADR, `REQ-IA-nn` con prueba, riesgos |
| K2 | `AI-Layer-Console-Assistant-Building` | Construir la consola en un panel Blazor: el código elige, el modelo redacta |
| K3 | `AI-Layer-Verification-And-Acceptance` | Aceptar con evidencia: batería ×3, crítico, capturas, sitio real por rol, latencia |
| K4 | `AI-Layer-Integration-Working-Method` | Forma de trabajo: fases, mesa con refutador, plan en PRs, registro de frentes |
| K5 | `AI-Layer-Conversational-Design` | Marco normativo del turno y formas de respuesta |
| K6 | `AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks` | Corpus para recuperación, funciones y destinos |
| K7 | `AI-Layer-Testing-Strategy` | La suite de pruebas completa y las compuertas de CI |
| K8 | `AI-Layer-Chat-Console-Interface` | La consola de chat como interfaz |

- **Cómo se citan hoy.** Por alias y sección: «`<Alias>` §N (base de conocimientos)».
- **Cómo se leen las citas anteriores.** Una cita `K<n> §x`, `K<n> Rx.y` o `K<n>-AP-n`, o una por alias y versión
  1.x, se lee contra el commit `8791fb1` de este repositorio, que tiene la última versión publicada acá. Los
  identificadores de regla y de anti-patrón (`R2.6b`, `CD-8`, `K5-AP-8`…) no cambiaron. Las secciones se
  renumeraron así (nivel 2; un `N.k` del origen pasa a `S.M.k` con el mismo `k`):

| Id | Sección en `8791fb1` → sección en la base |
| --- | --- |
| K1 | 0→0 · 1→1.1 · 2→3.1 · 3→2.1 · 4→2.2 · 5→2.3 · 6→2.4 · 7→5.1 · 8→4 · 9→3.2 · 10→3.3 · 11→3.4 · 12→6 · 12.1→7 · 13→1.2 y §9 · 14→0 «Queda afuera» · Glosario→0.1 |
| K2 | 0→0 · 1→1.1 · 2→3.1 · 3→2.1 · 4→3.2 · 5→3.3 · 6→3.4 · 7→3.5 · 8→3.6 · 9→3.7 · 10→3.8 · 11→3.9 · 12→6 · 13→7 · 14→1.2 · 15→0 «Queda afuera» · 16→2.2 · Glosario→0.1 · Historial→10 |
| K3 | 0→0 · 1…12→3.1…3.12 · 13→6 · 14→7 · 15→5.1 · 16→1.2 · 17→0 «Queda afuera» · Glosario→0.1 |
| K4 | 0→0 · 1…7→3.1…3.7 · 8→7 · 9→3.8 · 10→3.9 · 11→6 · 12→1.2 · 13→0 «Queda afuera» · 14→3.10 · Glosario→0.1 · Control de cambios→10 |
| K5 | 0→0 · 1→1.1 · M→2.1 (M.1…M.6→2.1.1…2.1.6) · 2→3.1 · 3→2.2 · 4→3.2 · 5→3.3 · 6→3.4 · 7→3.5 · 8→3.6 · 9→6 · 10→7 · 11→1.2 · 12→0 «Queda afuera» · Glosario→0.1 · Historial→10 |
| K6 | 0→0 · 1→1.1 · 2→3.1 · Parte A→3.2 (3…8→3.2.1…3.2.6) · Parte B→3.3 (9…12→3.3.1…3.3.4) · 13→5.1 · Parte C→3.4 (14…18→3.4.1…3.4.5) · 19→3.5 · 20→3.6 · 21→6 · 22→7 · 23→1.2 · 24→0 «Queda afuera» · Glosario→0.1 · Historial→10 |
| K7 | 0→0 · 1→3.1 · 2→2.1 · 3…12→3.2…3.11 · 13→7.1 · 14→3.12 · 15→3.13 · 16→6 · 17→7.2 · 18→1.2 · 19→0 «Queda afuera» · Glosario→0.1 · Historial→10 |
| K8 | 0→0 · 1→1.1 · 2→3.1 · 3→2.1 · 4→2.2 · 5…13→3.2…3.10 · 14→6 · 15→7 · 16→1.2 · 17→0 «Queda afuera» · Glosario→0.1 · Historial→10 |

- **Acceso.** La base es privada. Se pide a quien mantiene este repositorio, con un issue acá.
