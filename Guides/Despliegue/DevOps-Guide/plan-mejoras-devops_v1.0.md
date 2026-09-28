# Plan de Mejoras DevOps — Registro Metodológico

**Archivo:** plan-mejoras-devops_v1.0.md
**Versión:** 1.0
**Fecha:** 2026-04-25
**Autor:** Equipo de Arquitectura / DevOps (asistido por agentes IA)
**Estado:** En ejecución (Fase 1 cerrada; Fase 2 en curso; Fase 3 pendiente)
**Sección:** /devs/devops (material metodológico, no producto)

---

## 1. Propósito de este documento

Este documento registra, en clave archivística, el plan de mejoras DevOps ejecutado sobre la sección `/docs/09_devops` del repositorio `Ejemplo_IA_SDD_Template`, junto con las acciones cross-repo asociadas (Bloque A). No es documentación operativa del producto: es la **traza metodológica** del proceso por el cual se llegó al estado actual de esos artefactos. Se conserva en `/devs/devops/` para preservar la separación entre material teórico/didáctico (`/devs`) y documentación viva del producto (`/docs`).

El registro cumple tres funciones. Primero, **rendición de cuentas**: deja constancia de qué se cambió, por qué, quién (qué rol especializado) lo cambió y bajo qué auditoría. Esto permite a un revisor externo (un alumno, un auditor, un nuevo miembro del equipo) reconstruir el proceso sin acceso a los logs originales de la sesión IA. Segundo, **reusabilidad metodológica**: el patrón "plan-then-confirm con subagentes especializados + audit independiente entre fases" es replicable en otras secciones del repositorio o en otros proyectos que adopten una metodología SDD asistida por IA; este documento es la plantilla de referencia. Tercero, **didáctica**: para alumnos y equipos que adopten el template, mostrar el proceso (no solo el producto) explicita los criterios de calidad que rigen un refactor documental industrial — qué se considera un defecto bloqueante, cómo se prioriza, cómo se asigna trabajo, cómo se audita.

Por convención de la metodología SDD aplicada en este repositorio, cada plan no-trivial ejecutado debe materializarse como documento en `/devs/devops/` (o la carpeta de proceso correspondiente) antes, durante o inmediatamente después de su ejecución. Este archivo cumple ese requisito para el plan de mejoras DevOps del 2026-04-25. Se redacta con Fase 1 cerrada y Fase 2 en ejecución; la Fase 3 (audit final cross-doc) se documentará retrospectivamente como anexo o como nueva versión del documento (`v1.1`) cuando se cierre.

**Audiencia primaria:** equipo de arquitectura/DevOps del proyecto, docentes y alumnos de la cátedra, futuros mantenedores del template. **Audiencia secundaria:** auditores externos que evalúen el proceso, otros equipos de la organización que adopten la metodología SDD asistida por IA.

**Lectura cruzada recomendada:** este documento opera junto a `/devs/metodologia-sdd/` (fundamentos del enfoque SDD) y `/devs/agentes-especialidades/` (catálogo de especialidades de subagentes). Aquellos definen el qué y el por qué del enfoque; este documento muestra un caso concreto de aplicación.

---

## 2. Contexto: la auditoría inicial

El plan se origina en una auditoría independiente sobre `/docs/09_devops` realizada el 2026-04-25 por un subagente con rol "Auditor especialista en documentación técnica + DevOps + estándares de la industria". El subagente operó sin contexto previo sobre las decisiones del proyecto y emitió un informe priorizado por severidad. Los hallazgos clave que dispararon el plan se sintetizan a continuación.

### 2.1. Defectos bloqueantes (P0)

- **Corrupción de contenido**: el archivo `estrategia-versionado_v1.0.md` contenía una copia textual de `entornos-deploy_v1.0.md`. La sección de versionado, pieza autoritaria del proceso de release, era inexistente como tal.
- **Triple contradicción de feed de paquetes**: el pipeline de CI/CD declaraba publicación a NuGet.org; el documento de entornos también declaraba NuGet.org y mencionaba paquetes `1.0.x`; la guía de publicación afirmaba que "GitHub Packages es el feed actual y NuGet.org se evaluará a futuro". Los tres documentos describían políticas mutuamente incompatibles.
- **Ausencia de `CHANGELOG.md` raíz**: el repositorio no contaba con un changelog en la raíz, requisito para alinearse con Keep a Changelog y para que herramientas downstream (release notes, dashboards) puedan consumirlo.

### 2.2. Defectos de industria (P1)

- **Ausencia de README de sección** en `/docs/09_devops/`, lo que dificultaba la navegación y la jerarquía informativa.
- **Pipeline incompleto frente a estándares de supply chain**: sin SCA (analisis de composición de software), sin SBOM (Software Bill of Materials), sin firma de paquetes, sin Source Link, sin matriz de SO en CI.
- **Modelo de entornos mal aplicado**: el doc de entornos describía "DEV/QA/STAGING/PROD", esquema válido para servicios desplegables pero anti-patrón cuando el artefacto es una librería NuGet.
- **Guía NuGet sin metadatos production-grade**: faltaban `PackageProjectUrl`, `RepositoryUrl`, `RepositoryType`, `PackageReadmeFile`, `PackageLicenseExpression`, `PackageIcon`, entre otros.

### 2.3. Inconsistencias cross-repo (Bloque A)

- **`LICENSE` raíz GPL v2** contradiciendo la licencia MIT declarada en la documentación. Esta inconsistencia es jurídicamente relevante: el archivo `LICENSE` raíz prevalece legalmente sobre cualquier mención en docs, por lo que el repositorio estaba de facto bajo GPL v2 hasta el cierre de A1.
- **`acuerdo-equipo_v1.0.md`** declaraba ramas `develop` y `hotfix/*`, contradiciendo el flujo GitHub Flow que el equipo había adoptado. Esto generaba confusión operativa: nuevos contribuidores leían el acuerdo y abrían PRs contra `develop` (rama inexistente).
- **`docs/11_examples/`** describía el consumo del paquete desde NuGet.org, contradiciendo la decisión confirmada de usar exclusivamente GitHub Packages. Un consumidor que siguiera el ejemplo no podría resolver el paquete.

### 2.4. Priorización aplicada a los hallazgos

El subagente auditor priorizó los hallazgos en tres niveles:

- **P0 (bloqueante):** impide cualquier release o introduce un riesgo legal/técnico inmediato. Se ataca primero, sin excepciones.
- **P1 (industria):** el documento existe pero está incompleto frente a estándares de industria razonables. Se ataca en segunda fase.
- **P2 (cross-repo / coherencia global):** afecta consistencia entre secciones; suele descubrirse al cruzar el alcance original con el resto del repositorio.

Este sistema de prioridades es el mismo que se utiliza en triage de issues de seguridad y de incidentes en producción, lo que facilita que un auditor externo lo entienda sin briefing.

---

## 3. Decisiones del proyecto

Antes de ejecutar el plan, el asistente principal recolectó del usuario un set de siete decisiones (D1–D7) que actuarían como invariantes para todos los subagentes. Estas decisiones eliminan ambigüedad y permiten que cada subagente especializado opere con un único marco de referencia.

| ID  | Decisión                          | Valor elegido                                       | Justificación breve                                                                                          |
|-----|-----------------------------------|-----------------------------------------------------|--------------------------------------------------------------------------------------------------------------|
| D1  | Modelo de feeds                   | Solo GitHub Packages                                | Simplifica gobernanza, evita doble publicación; alineado con que el repo es template académico/industrial.  |
| D2  | Licencia                          | MIT                                                 | Permisiva, máximo reuso, compatible con uso académico y comercial.                                          |
| D3  | Estrategia de branching           | GitHub Flow estricto (`main` + `feature/*`)         | Adecuado para una librería pequeña con releases continuos; evita complejidad de Git Flow.                   |
| D4  | Target Framework                  | `net10.0` único, sin multi-target                   | Reduce superficie de matriz CI; simplifica empaquetado y soporte.                                            |
| D5  | Versionado automático             | Conventional Commits + MinVer                       | Trazabilidad commit-a-versión sin scripts custom; SemVer 2.0.0 garantizado.                                  |
| D6  | Alcance del plan                  | Tres fases (P0 + P1 + audit cruzado)                | Permite check-ins entre fases y aísla riesgo de regresión.                                                  |
| D7  | Tono de la documentación          | Industrial / productivo (sin TODOs didácticos)      | El template debe ser usable en producción; los TODOs didácticos pueden vivir en `/devs`.                    |

Posteriormente el alcance se amplió a P2 / Bloque A (LICENSE, acuerdo-equipo, ejemplos 03, slug del repositorio), por decisión explícita del usuario al detectar que las inconsistencias cross-repo bloqueaban la coherencia global del template.

### 3.1. Justificaciones extendidas

**D1 — Solo GitHub Packages.** Mantener un único feed simplifica la gobernanza de auth (un solo `nuget.config`, un solo PAT/token), reduce la superficie de ataque (una sola lista de paquetes a vigilar por dependabot), elimina decisiones de "qué publicar dónde" por versión, y es coherente con que los consumidores primarios del template son académicos o equipos internos con acceso ya provisto al repo. Si en el futuro el paquete adquiere tracción externa, migrar a NuGet.org es una decisión incremental: D1 no la cierra, solo la difiere.

**D2 — MIT.** Es la licencia más permisiva de uso común, máximamente compatible con uso académico (sin restricciones de copyleft que afecten trabajos derivados de alumnos) y comercial (sin obligaciones de redistribución de código fuente). MIT también es la elección por defecto de la mayor parte del ecosistema .NET reciente, lo que evita fricción de incompatibilidad de licencias en dependencias.

**D3 — GitHub Flow estricto.** Para una librería de tamaño moderado con pocos contribuidores y release continuo, GitHub Flow ofrece la complejidad mínima necesaria: una sola rama protegida (`main`), branches efímeras `feature/*`, PR como única vía de merge. Git Flow (con `develop`, `release/*`, `hotfix/*`) introduce overhead que solo se justifica con releases coordinados y trenes de versión, escenario que no aplica aquí.

**D4 — `net10.0` único.** Multi-target (`net8.0;net9.0;net10.0`) tiene valor cuando la audiencia consumidora es amplia y heterogénea; en este template, la audiencia opera sobre la versión más reciente del runtime. Single-target reduce: tiempo de build (un solo TFM), tamaño del paquete (un solo target framework moniker), complejidad de directivas `#if NET10_0`, y matriz CI. Si el template gana adopción externa con audiencias en versiones anteriores, multi-target es una decisión de iteración futura.

**D5 — Conventional Commits + MinVer.** Esta combinación entrega versionado automático sin scripts custom: Conventional Commits da semántica al commit (`feat`, `fix`, `BREAKING CHANGE`), MinVer la traduce a SemVer leyendo tags Git. La alternativa (manual bump del número de versión) es propensa a error y rompe la trazabilidad commit-versión. La alternativa (NerdBank.GitVersioning, GitVersion) es más expresiva pero más compleja; MinVer fue suficiente.

**D6 — Tres fases.** F1 ataca P0, F2 ataca P1+Bloque A, F3 audita cross-doc. Esta partición permite check-ins en puntos naturales y limita el blast radius de una regresión. Una fase única hubiera concentrado el riesgo; cuatro o más fases hubieran fragmentado innecesariamente.

**D7 — Tono industrial.** El template debe ser usable directamente en producción por equipos sin contexto previo. Un TODO didáctico ("aquí el alumno debe completar...") en un documento de pipeline es obstrucción operativa. La didáctica vive en `/devs`; el producto en `/docs`. Esta separación (formalizada en la convención `/docs vs /devs` del repositorio) hace D7 enforcement automático: si un texto es didáctico, va a `/devs`; si es operativo, va a `/docs`.

---

## 4. Patrón metodológico aplicado

### 4.1. Plan-then-confirm con subagente especializado

El patrón opera en cuatro pasos:

1. **Auditoría inicial independiente** por un subagente sin contexto previo, que emite hallazgos priorizados.
2. **Plan explícito** del asistente principal: lista de tareas, asignación de subagente con rol especializado por tarea, entregable esperado, criterio de aceptación. Este plan se presenta al usuario.
3. **Confirmación corta** del usuario sobre el plan (decisiones D1–D7 + alcance + modo de ejecución).
4. **Ejecución en fases** con un subagente distinto por tarea, y **audit independiente entre fases** por otro subagente que no participó en la ejecución.

La especialización del subagente no es decorativa: cada rol trae implícitos los estándares de su disciplina (SemVer 2.0.0 para Release Engineer, SLSA/OWASP SCVS/NIST SSDF para Supply Chain, Keep a Changelog 1.1.0 para changelog), reduciendo la carga cognitiva del prompt y mejorando la calidad del entregable.

### 4.2. Modo de ejecución

El usuario eligió **(a) Check-in entre fases**: el asistente principal pausa después de cada audit de fase y espera confirmación explícita antes de iniciar la siguiente. La alternativa rechazada era **(b) Cascada autónoma**, donde el asistente ejecuta las tres fases sin interrupción y reporta al final. Se prefirió (a) porque:

- Permite al usuario revisar diffs reales antes de avanzar, reduciendo el costo de un rollback masivo.
- Habilita ajustes finos del plan en función de hallazgos de la fase previa (la auditoría F1 generó la observación B1 que se resolvió en línea antes de iniciar F2).
- Es coherente con el carácter didáctico del repositorio: cada check-in es un punto de aprendizaje observable.

### 4.3. Asignación de roles a subagentes

El criterio para asignar rol a tarea es **proximidad de dominio**: la tarea va al subagente cuya especialidad contiene el dominio del cambio con la mayor cobertura. Cuando una tarea cruza dos dominios (p. ej., reforzar el pipeline con controles de supply chain), el rol se compone (`DevOps/Platform Engineer + Supply Chain (SLSA, OWASP SCVS, NIST SSDF)`). Cuando la tarea es transversal (auditoría final), el rol explícita la independencia (`Auditor independiente cross-doc (sin contexto previo)`).

Reglas adicionales:

- **Ningún subagente audita su propia salida.** El auditor de cada fase es siempre distinto del ejecutor. Esta regla no es negociable y aplica también si el subagente ejecutor es objetivamente más competente: la independencia del auditor pesa más que su especialización.
- **El asistente principal no asume rol de subagente.** Su función es orquestación, confirmación y ediciones quirúrgicas menores (p. ej., el fix B1 de una sola línea). Cualquier tarea que requiera juicio especializado debe delegarse, incluso si parece trivial.
- **Los roles se nombran por especialidad reconocible en la industria**, no por jerarquía organizacional, para maximizar la transferencia de estándares implícitos. "Release Engineer + SemVer 2.0.0" es preferible a "Senior Engineer L5".
- **Los roles componen al menos dos disciplinas cuando la tarea es transversal**, separadas con `+`. La primera disciplina es la dominante; la segunda aporta el marco de calidad. Ejemplo: `Technical Writer + Information Architect` para un README — el Writer redacta, el Architect garantiza navegabilidad.
- **El número de subagentes por fase se mantiene bajo (≤ 8 por fase).** Pasar de ese umbral indica que la fase está mal recortada y debería dividirse, o que algunas tareas pueden agruparse bajo un solo subagente.

---

## 5. Plan ejecutado

### 5.1. Fase 1 — P0 bloqueantes

| Tarea                                          | Subagente (rol especializado)                                  | Entregable                                                                            | Estado    |
|------------------------------------------------|----------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------|
| F1.1 Reescribir `estrategia-versionado_v1.0.md`| Release Engineer senior + especialista SemVer 2.0.0            | Documento reescrito desde cero, 807 líneas, 18 secciones, autoritario sobre SemVer    | Completado|
| F1.2 Reconciliar feeds                         | NuGet Packaging Specialist senior                              | Ediciones quirúrgicas en `pipeline-ci-cd`, `entornos-deploy`, `guia-publicacion-nuget`| Completado|
| F1.3 Crear `CHANGELOG.md` raíz                 | Release Engineer senior + Keep a Changelog 1.1.0               | Nuevo `CHANGELOG.md` con 3 versiones históricas                                       | Completado|
| Audit F1                                       | Auditor independiente DevOps + estándares                      | Veredicto: APROBADO CON OBSERVACIONES                                                 | Completado|
| Fix B1                                         | Asistente principal (no subagente)                             | Removida línea "Multi-target frameworks" en `pipeline-ci-cd_v1.0.md` §13:355          | Completado|

**Notas de fase:**

- F1.1 reescribió el documento desde cero porque la corrupción detectada hacía inviable un parche incremental; además, era la oportunidad para consolidar el documento como **fuente única de verdad** sobre versionado (SemVer 2.0.0, Conventional Commits, MinVer, política de pre-release y de hotfix dentro de GitHub Flow). El documento incluye 18 secciones que cubren: principios SemVer, mapeo Conventional Commits → bump (feat→minor, fix→patch, BREAKING CHANGE→major), configuración MinVer (`MinVerTagPrefix`, `MinVerVerbosity`, `MinVerDefaultPreReleaseIdentifiers`), política de tags (`v<X.Y.Z>`), política de pre-release (`-alpha.<n>`, `-beta.<n>`, `-rc.<n>`), política de hotfix sin rama dedicada (PR directo a `main` desde `feature/hotfix-*`), criterios de bump por tipo de cambio, ejemplos completos de mensaje de commit conformes, anti-patrones explícitos, y matriz de decisión versión-vs-cambio.
- F1.2 fue deliberadamente quirúrgica para no introducir cambios fuera de alcance: solo se tocaron las cláusulas que mencionaban NuGet.org o describían el feed. El subagente NuGet Specialist documentó cada edición con justificación (qué línea cambió, qué dijo antes, qué dice ahora, por qué).
- F1.3 generó un changelog con tres versiones históricas reconstruidas a partir del log Git, alineado con Keep a Changelog 1.1.0 (secciones Added/Changed/Deprecated/Removed/Fixed/Security; encabezados con fecha ISO; enlaces de comparación al final del archivo en formato `[X.Y.Z]: https://github.com/<owner>/<repo>/compare/v<prev>...v<X.Y.Z>`).
- El audit F1 emitió la observación B1 ("multi-target framework" residual en pipeline §13), que el asistente principal resolvió con una edición de una línea sin invocar nuevo subagente, dado el alcance trivial. Se registró la observación y la resolución en este documento para preservar trazabilidad.

### 5.2. Fase 2 — P1 industria + Bloque A (cross-repo)

| Tarea                                  | Subagente (rol especializado)                                                   | Archivo objetivo                                       | Estado       |
|----------------------------------------|---------------------------------------------------------------------------------|--------------------------------------------------------|--------------|
| F2.1 Crear README de sección           | Technical Writer + Information Architect                                        | `docs/09_devops/README.md` (nuevo)                     | En ejecución |
| F2.2 Reforzar pipeline                 | DevOps/Platform Engineer + Supply Chain (SLSA, OWASP SCVS, NIST SSDF)           | `docs/09_devops/pipeline-ci-cd_v1.0.md`                | En ejecución |
| F2.3 Reescribir `entornos-deploy`      | Release Architect (distribución de librerías)                                   | `docs/09_devops/entornos-deploy_v1.0.md`               | En ejecución |
| F2.4 Completar guía NuGet              | NuGet Packaging Specialist senior                                               | `docs/09_devops/guia-publicacion-nuget_v1.0.md`        | En ejecución |
| A1 LICENSE → MIT                       | Open Source Compliance Officer + Legal-Tech                                     | `LICENSE` (raíz)                                       | En ejecución |
| A2 Fixear acuerdo-equipo               | Agile Coach + Technical Writer                                                  | `docs/00_contexto/acuerdo-equipo_v1.0.md`              | En ejecución |
| A3 Fixear ejemplos                     | Technical Writer + NuGet Specialist                                             | `docs/11_examples/README.md`, `ejemplo-03-multaapp-nuget.md` | En ejecución |
| Audit F2                               | Auditor independiente Supply Chain + DevOps                                     | (a ejecutar tras F2)                                   | Pendiente    |

**Notas de fase:**

- F2.1 produce el README de sección con función de mapa: lista cada documento de `/docs/09_devops/`, su propósito en una línea, su audiencia (developer, ops, release manager, auditor) y el orden de lectura sugerido (acuerdo-equipo → estrategia-versionado → pipeline → entornos → guía-publicación). El Information Architect garantiza que la navegación cumpla el principio de "menos clicks que decisiones": cada salto debe responder a una pregunta concreta del lector.
- F2.2 es la tarea de mayor superficie técnica de toda la fase: introduce SCA (p. ej., `dotnet list package --vulnerable` y `--deprecated`, integración con Dependabot o Renovate), SBOM (CycloneDX vía `CycloneDX/cyclonedx-dotnet-action` o equivalente, formato JSON, adjunto al release), firma de paquetes (con sigstore/cosign o certificado de organización via `dotnet nuget sign`), Source Link (`PublishRepositoryUrl`, `EmbedUntrackedSources`, `IncludeSymbols=true`, `SymbolPackageFormat=snupkg`), y matriz de SO en CI (Ubuntu/Windows/macOS para los jobs de test, con cache de paquetes NuGet por SO). El subagente trae el mapeo a controles de SLSA Build L1/L2 y a categorías de OWASP SCVS.
- F2.3 reformula el documento de entornos para una librería NuGet: en vez de DEV/QA/STAGING/PROD, describe **canales de distribución** (pre-release con sufijos `-alpha`/`-beta`/`-rc`, release estable, política de unlist en GitHub Packages, política de no-yank salvo CVE crítico), **ambientes de build** (CI matrix con triggers por evento: push a `main` → release estable; PR → build de validación sin publicación; tag → release con notas), y **consumidores** (developer local con auth via PAT, CI consumidor downstream con `GITHUB_TOKEN`). El Release Architect documenta la decisión de no usar staging porque para una librería el "staging" se modela con pre-release versions, no con un feed separado.
- F2.4 completa los metadatos de empaquetado: `PackageId`, `Version` (controlado por MinVer), `Authors`, `Company`, `Product`, `Description`, `Copyright`, `PackageTags`, `PackageProjectUrl`, `RepositoryUrl`, `RepositoryType=git`, `RepositoryBranch`, `RepositoryCommit`, `PackageReadmeFile=README.md` (con `<None Include="README.md" Pack="true" PackagePath="\"/>`), `PackageLicenseExpression=MIT`, `PackageIcon=icon.png`, `PackageReleaseNotes` (alimentado desde CHANGELOG), `MinClientVersion`. El subagente especifica el orden de las propiedades en el `.csproj` y referencia la documentación oficial de Microsoft Learn.
- A1, A2, A3 son ediciones cross-repo necesarias para coherencia global; se incluyen en F2 (no en una fase separada) porque comparten ventana de revisión y el costo marginal del check-in es bajo. A1 reemplaza el `LICENSE` GPL v2 por el texto canónico de MIT con copyright `(c) <año> <holder>`. A2 reescribe la sección de branching del acuerdo-equipo para alinear con GitHub Flow estricto (eliminando referencias a `develop` y `hotfix/*`). A3 actualiza los ejemplos para mostrar configuración de fuente NuGet apuntada a GitHub Packages, con `nuget.config` de ejemplo y pasos de autenticación.

### 5.3. Fase 3 — Audit final cross-doc independiente

| Tarea            | Subagente (rol especializado)                              | Estado    |
|------------------|------------------------------------------------------------|-----------|
| F3 Audit final   | Auditor independiente cross-doc (sin contexto previo)      | Pendiente |

**Criterio de aceptación de F3:** el auditor debe verificar (a) coherencia entre las 7 decisiones D1–D7 y los artefactos finales; (b) ausencia de contradicciones internas (todos los docs deben coincidir en feed, branching, target, licencia y versionado); (c) cumplimiento de estándares de industria reclamados (SemVer 2.0.0, Keep a Changelog 1.1.0, Conventional Commits, SLSA básico, GitHub Flow); (d) navegabilidad (links internos, README de sección operativo). El audit emite veredicto: APROBADO / APROBADO CON OBSERVACIONES / RECHAZADO + plan de remediación.

### 5.4. Estado de ejecución al momento de redactar este documento

| Fase | Estado al 2026-04-25                                                                                  |
|------|--------------------------------------------------------------------------------------------------------|
| F1   | **Cerrada.** F1.1, F1.2, F1.3 completados; audit F1 con veredicto APROBADO CON OBSERVACIONES; B1 resuelto. |
| F2   | **En ejecución.** Subagentes despachados en paralelo para F2.1–F2.4 + A1–A3; audit F2 a ejecutar tras cierre de tareas. |
| F3   | **Pendiente.** A iniciar tras confirmación del usuario sobre cierre de F2. Bloqueada hasta entonces.   |

Este documento se redacta con visibilidad limitada al estado de F2 dado que algunos subagentes ejecutan en paralelo. Cuando F2 cierre y F3 ejecute, se actualizará el control de cambios con una versión `1.1` que registre el resultado.

---

## 6. Auditorías por fase

| Fase | Auditor (rol)                                                         | Veredicto                  | Observaciones críticas                                                                                                          |
|------|-----------------------------------------------------------------------|----------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| F1   | Auditor independiente DevOps + estándares de la industria             | APROBADO CON OBSERVACIONES | B1: línea residual "Multi-target frameworks" en `pipeline-ci-cd_v1.0.md` §13:355. Resolución: edición en línea, sin subagente.  |
| F2   | Auditor independiente Supply Chain + DevOps                           | (pendiente)                | A ejecutar tras cierre de F2.1–F2.4 + A1–A3.                                                                                    |
| F3   | Auditor independiente cross-doc (sin contexto previo)                 | (pendiente)                | A ejecutar tras cierre de F2 y de su audit.                                                                                     |

**Política de re-trabajo:** una observación con clasificación crítica del auditor obliga a re-abrir la fase con el subagente ejecutor original (o uno equivalente si el original no está disponible) y re-auditar antes de avanzar. Una observación menor puede resolverse en línea por el asistente principal y registrarse como nota de fase.

**Clasificación de observaciones del auditor:**

- **Crítica (bloqueante):** contradice una invariante del proyecto (D1–D7), introduce una regresión, o deja un documento autoritario en estado inconsistente. Requiere re-trabajo formal con subagente ejecutor.
- **Mayor:** afecta la calidad del entregable pero no la corrección. Ejemplo: un README sin sección de "audiencia" definida. Se resuelve antes de avanzar a la siguiente fase, pero puede asignarse al asistente principal si el alcance es acotado.
- **Menor:** estilística, tipográfica o estructural sin impacto funcional. Se resuelve en línea o se difiere a una iteración posterior con registro explícito en este documento.

La observación B1 se clasificó como **menor** (línea residual sin impacto funcional, dado que el `.csproj` ya tenía un único TFM `net10.0` y la línea era documental). Por eso el asistente principal pudo resolverla sin invocar nuevo subagente.

---

## 7. Trazabilidad de cambios

| Archivo                                                | Tipo (nuevo/editado/reescrito)               | Fase            | Subagente                                                |
|--------------------------------------------------------|----------------------------------------------|-----------------|----------------------------------------------------------|
| `docs/09_devops/estrategia-versionado_v1.0.md`         | reescrito                                    | F1.1            | Release Engineer + SemVer 2.0.0                          |
| `docs/09_devops/pipeline-ci-cd_v1.0.md`                | editado (F1.2) + editado (F2.2)              | F1.2, F2.2      | NuGet Specialist; DevOps + Supply Chain                  |
| `docs/09_devops/entornos-deploy_v1.0.md`               | editado (F1.2) + reescrito (F2.3)            | F1.2, F2.3      | NuGet Specialist; Release Architect                      |
| `docs/09_devops/guia-publicacion-nuget_v1.0.md`        | editado (F1.2) + editado (F2.4)              | F1.2, F2.4      | NuGet Specialist; NuGet Specialist                       |
| `docs/09_devops/README.md`                             | nuevo                                        | F2.1            | Technical Writer + Information Architect                 |
| `CHANGELOG.md`                                         | nuevo                                        | F1.3            | Release Engineer + Keep a Changelog 1.1.0                |
| `LICENSE`                                              | reemplazado                                  | A1              | Open Source Compliance Officer + Legal-Tech              |
| `docs/00_contexto/acuerdo-equipo_v1.0.md`              | editado                                      | A2              | Agile Coach + Technical Writer                           |
| `docs/11_examples/README.md`                           | editado                                      | A3              | Technical Writer + NuGet Specialist                      |
| `docs/11_examples/ejemplo-03-multaapp-nuget.md`        | editado                                      | A3              | Technical Writer + NuGet Specialist                      |

**Convención de tipos:**
- *nuevo*: archivo creado por la tarea (no existía antes).
- *editado*: cambios quirúrgicos sobre archivo preexistente, sin alterar la estructura general.
- *reescrito*: contenido sustituido en su totalidad o casi totalidad, manteniendo nombre y ruta.
- *reemplazado*: contenido sustituido en su totalidad, manteniendo nombre y ruta (usado para artefactos no documentales como `LICENSE`).

**Política de preservación de nombres de archivo:** ningún archivo se renombra durante el plan. Esta regla protege links externos (referencias en clases, en presentaciones, en commits previos, en otros documentos del repositorio o forks). El cambio de versión del documento se refleja en el sufijo `_v<X.Y>.md` solo cuando es necesario; mientras la rama mayor de versión sea estable, se preserva el sufijo `_v1.0`.

**Trazabilidad commit-a-tarea:** cada tarea de la tabla anterior produce uno o varios commits con prefijo Conventional Commits que la identifican. Convención sugerida:
- F1.1 → `docs(versionado): rewrite estrategia-versionado from scratch`
- F1.2 → `docs(devops): reconcile feeds to GitHub Packages only`
- F1.3 → `docs: add root CHANGELOG.md`
- F2.1 → `docs(devops): add section README`
- F2.2 → `docs(pipeline): harden with SCA, SBOM, signing, source link, OS matrix`
- F2.3 → `docs(devops): rewrite entornos-deploy for library distribution model`
- F2.4 → `docs(nuget): complete production-grade package metadata`
- A1 → `chore: replace LICENSE with MIT`
- A2 → `docs(team): align acuerdo-equipo with GitHub Flow`
- A3 → `docs(examples): align consumption examples with GitHub Packages`

---

## 7.1. Métricas del plan

A modo de registro cuantitativo, se reportan las siguientes métricas observadas hasta el momento de redacción de este documento.

| Métrica                                                         | Valor                                              |
|-----------------------------------------------------------------|----------------------------------------------------|
| Cantidad de subagentes ejecutores distintos invocados (F1+F2)   | 9 (1 auditor inicial + 5 ejecutores F1/F2 + 3 Bloque A) |
| Cantidad de auditores independientes invocados                  | 1 (F1) + 1 pendiente (F2) + 1 pendiente (F3)       |
| Archivos nuevos creados                                         | 3 (`CHANGELOG.md`, `docs/09_devops/README.md`, este documento) |
| Archivos reescritos                                             | 2 (`estrategia-versionado_v1.0.md`, `entornos-deploy_v1.0.md`) |
| Archivos editados                                               | 5 (pipeline + guía-publicacion + acuerdo-equipo + ejemplos×2) |
| Archivos reemplazados                                           | 1 (`LICENSE`)                                      |
| Decisiones invariantes fijadas antes de ejecución               | 7 (D1–D7)                                          |
| Observaciones de audit registradas                              | 1 (B1, menor, resuelta en línea)                   |
| Riesgos residuales documentados                                 | 7 (RR1–RR7)                                        |
| Lecciones metodológicas capturadas                              | 12 (L1–L12)                                        |

Estas métricas no son SLAs ni KPIs vinculantes; son registro descriptivo del esfuerzo del plan, útil para calibrar planes futuros de complejidad similar.

## 8. Riesgos identificados y residuales

### 8.1. Riesgos mitigados

- **R1: Inconsistencia de feed.** Mitigado en F1.2 (reconciliación quirúrgica) y reforzado en F2 (README de sección + entornos reescritos describen un único feed).
- **R2: Documento autoritario corrupto (versionado).** Mitigado en F1.1 con reescritura completa y validación contra SemVer 2.0.0.
- **R3: Falta de changelog.** Mitigado en F1.3.
- **R4: Licencia contradictoria.** Mitigado en A1.
- **R5: Branching contradictorio en acuerdo-equipo.** Mitigado en A2.
- **R6: Ejemplos describiendo consumo desde NuGet.org.** Mitigado en A3.
- **R7: Pipeline débil en supply chain.** En mitigación en F2.2.

### 8.2. Riesgos residuales

- **RR1: Drift futuro entre `estrategia-versionado_v1.0.md` y la configuración real de MinVer en el `.csproj`.** No se introdujo aún un check automatizado que valide que el target framework, los prefijos de tag y la política de pre-release del documento coinciden con lo declarado en el proyecto. Mitigación propuesta: agregar un job de CI que parsee ambos y falle el build ante divergencia. Pendiente para una iteración futura. Indicador de detección: un release con número de versión inesperado o con TFM divergente del declarado en docs.
- **RR2: SBOM y firma sin política de retención.** F2.2 introduce SBOM y firma, pero la política de retención de artefactos (cuántas versiones, por cuánto tiempo) queda fuera de alcance; aplica más a un proyecto productivo que a un template. Mitigación: documentar la política como ítem futuro en el README de sección, sin imponer valores concretos.
- **RR3: Slug del repositorio.** Se mencionó como parte del Bloque A; pendiente confirmar si el cambio de slug (renombre del repo en GitHub) está dentro o fuera de alcance del plan actual. Decisión inicial: fuera de alcance, dado que el renombre rompe URLs externas y excede el costo del beneficio en el contexto de un template. Si se decide ejecutar, requerirá una fase dedicada con migración de redirects.
- **RR4: No hay validación independiente del CHANGELOG histórico.** Las tres versiones históricas del `CHANGELOG.md` se reconstruyeron desde el log Git, pero no se verificó cada entrada contra los commits de origen uno por uno; un sesgo de interpretación es posible. Mitigación: el audit F3 puede incluir una verificación spot-check de al menos una entrada por versión histórica.
- **RR5: Idioma mixto.** Algunos documentos tienen títulos o secciones en inglés mientras el cuerpo es en español. La política de idioma uniforme no fue parte de las decisiones D1–D7 y queda como deuda menor. Recomendación: definir la política en la próxima iteración como D8 (idioma de documentación = español, con permitidos términos técnicos en inglés cuando no haya equivalente preciso).
- **RR6: Subagentes ejecutando en paralelo sin contrato de no-interferencia.** F2.1–F2.4 + A1–A3 se despacharon en paralelo, lo que mejora el tiempo de ciclo pero introduce riesgo de conflictos de edición si dos subagentes tocan el mismo archivo. Mitigación aplicada: cada tarea tiene un archivo objetivo único; sin embargo, el patrón debería formalizarse con un "contrato de no-interferencia" en futuros planes (lista explícita de archivos por tarea, prohibido salirse).
- **RR7: La auditoría inicial podría haber omitido hallazgos.** Un solo subagente auditor, por especializado que sea, no garantiza cobertura. Para refactors mayores, sería preferible un audit redundante por dos subagentes con especialidades distintas (p. ej., uno enfocado en supply chain, otro en consistencia documental). Para este plan se usó un solo auditor inicial; se compensa con el audit F3 cross-doc.

---

## 9. Lecciones metodológicas

**L1. La auditoría inicial independiente paga su costo de inmediato.** Detectar la corrupción de `estrategia-versionado_v1.0.md` antes de iniciar el plan evitó iterar sobre un documento inválido. El subagente auditor sin contexto previo encontró en una pasada lo que un ejecutor con contexto pudo haber pasado por alto al asumir que el archivo era el correcto.

**L2. Confirmar feeds (y otras decisiones invariantes) antes de ejecutar es no-negociable.** Las tres contradicciones de feed que afectaban a tres documentos distintos no son detectables localmente: solo se ven cuando alguien lee los tres juntos. Un subagente especializado opera local; la coherencia global es responsabilidad del orquestador. La forma de garantizar coherencia es **fijar las invariantes (D1–D7) antes** y referenciarlas en cada prompt de subagente.

**L3. Centralizar reglas de versionado en un único documento autoritario reduce el costo marginal de cambios futuros.** Antes del refactor, las reglas de versionado estaban dispersas (parcialmente en pipeline, en entornos, en la guía NuGet). Tras F1.1, `estrategia-versionado_v1.0.md` es la fuente única; los demás documentos enlazan a sus secciones. Cualquier cambio futuro de política se hace en un solo lugar.

**L4. El audit independiente entre fases es barato y captura observaciones tipo B1.** El costo marginal de invocar un subagente auditor entre F1 y F2 fue bajo, y atrapó una línea residual ("multi-target") que un ejecutor enfocado en otro objetivo no hubiera revisado. La inversión se justifica.

**L5. La especialización del rol del subagente es una herramienta de calidad, no de cosmética.** Asignar la tarea de SBOM/firma/Source Link a un rol "DevOps + Supply Chain" trae implícitos los frameworks de referencia (SLSA, OWASP SCVS, NIST SSDF). Si la misma tarea se asignara a un rol genérico, el prompt tendría que cargar manualmente todos esos estándares y aún así correr el riesgo de omitir alguno.

**L6. El check-in entre fases tiene costo bajo y valor alto cuando hay incertidumbre.** En refactors documentales con riesgo de regresión, el modo (a) se prefiere a (b). La cascada autónoma se justifica solo cuando las invariantes son sólidas y el dominio es bien conocido para el subagente.

**L7. Las inconsistencias cross-repo no se descubren auditando una sola sección.** El Bloque A (LICENSE, acuerdo-equipo, ejemplos) emergió como ampliación de alcance porque la auditoría inicial sobre `/docs/09_devops` reveló contradicciones que solo eran visibles al cruzar con la raíz del repo y con `/docs/00_contexto` y `/docs/11_examples`. Un audit final cross-doc (F3) es la red de seguridad para este tipo de hallazgos.

**L8. Documentar el plan en `/devs/devops/` antes de cerrar la fase final tiene valor preventivo.** Si el plan se documenta solo al final, se corre el riesgo de racionalizar a posteriori. Documentarlo durante la ejecución (este archivo se crea con F2 en curso y F3 pendiente) preserva la honestidad metodológica: las observaciones se registran cuando ocurren, no cuando son convenientes.

**L9. La separación `/docs` vs `/devs` paga su costo en mantenimiento.** Tener una carpeta dedicada a material metodológico permite que documentos como este existan sin contaminar la documentación operativa. Un alumno o consumidor del template lee `/docs` para usarlo; un mantenedor o adoptor del patrón lee `/devs` para entender cómo evolucionó. La frontera reduce ruido para ambas audiencias.

**L10. La paralelización de subagentes en F2 mejora el tiempo de ciclo pero requiere disciplina de no-interferencia.** Despachar siete subagentes a la vez (F2.1–F2.4 + A1–A3) acorta significativamente la fase, pero solo es seguro porque cada uno tiene un archivo objetivo único. Si dos subagentes hubieran necesitado tocar `pipeline-ci-cd_v1.0.md`, la paralelización habría requerido coordinación explícita o serialización. La regla práctica: paralelizar solo cuando los archivos objetivo son disjuntos.

**L11. Los roles compuestos (`X + Y`) son más útiles que los roles simples cuando la tarea es transversal.** Un Technical Writer solo produce un README legible; un Technical Writer + Information Architect produce un README legible y navegable. La composición del rol en el prompt traslada implícitamente dos sets de estándares al subagente sin que el orquestador deba enumerarlos. Esta es una propiedad emergente útil del prompting con roles especializados.

**L12. Las decisiones invariantes deben tomarse antes, no durante.** Cuando una decisión emerge a mitad de fase (p. ej., "¿usamos NuGet.org o GitHub Packages?"), interrumpe a los subagentes y obliga a re-trabajo. Recolectar D1–D7 antes de despachar el primer subagente es cara fija que se amortiza con la primera decisión que hubiera causado re-trabajo.

---

## 10. Aplicabilidad como template SDD

El patrón aquí registrado — **plan documentado en `/devs/<dominio>/` + ejecución por subagentes especializados + audit independiente entre fases** — es replicable. Para aplicarlo a otra sección del repo o a otro proyecto, el flujo sugerido es:

1. **Auditoría inicial independiente** sobre la sección objetivo. Subagente sin contexto previo. Output: lista priorizada (P0/P1/P2) de hallazgos.
2. **Recolección de invariantes** con el usuario (equivalente a D1–D7 de este plan). Cada invariante elimina ambigüedad para todos los subagentes.
3. **Plan explícito** con asignación de subagente especializado por tarea, entregable y criterio de aceptación.
4. **Confirmación corta** con el usuario (alcance, modo de ejecución, invariantes).
5. **Ejecución en fases** con check-ins entre fase y fase si hay incertidumbre, o cascada autónoma si las invariantes son sólidas.
6. **Audit independiente** entre fases por subagente distinto del ejecutor.
7. **Audit final cross-doc** por subagente sin contexto previo, para capturar inconsistencias globales.
8. **Documentar el plan** como archivo en `/devs/<dominio>/plan-<tema>_v<X.Y>.md` siguiendo la estructura de este documento.

**Secciones del repositorio donde el patrón se aplicaría con beneficio inmediato:**

- `/docs/03_arquitectura/` — auditar coherencia entre documentos de arquitectura, ADRs y diagramas.
- `/docs/05_calidad/` — auditar coherencia entre estándares de testing, criterios DoD/DoR y configuración real del pipeline.
- `/docs/00_contexto/` — auditar coherencia entre acuerdo de equipo, definición de proyecto y stakeholders.

**Adaptaciones recomendadas:**

- Para secciones de menor superficie (un solo documento), el plan puede colapsar las fases F1 y F2 en una sola fase con dos audits.
- Para proyectos productivos (no template), el documento de plan debería incluir un campo "owner" por tarea (responsable humano además del subagente IA) y un campo "reviewer" (humano que aprueba el merge del PR).
- Para proyectos con alta rotación de personas, el documento de plan se vuelve crítico como onboarding: un nuevo miembro puede entender el porqué de las decisiones leyendo `/devs/<dominio>/plan-*.md`.
- Para proyectos con compliance regulatorio (ISO 27001, SOC 2, GDPR-related), el documento de plan puede mapearse directamente a evidencia de control: cada audit independiente cumple un requisito de "separación de funciones"; cada decisión D# cumple un requisito de "documentación de cambios".

**Anti-patrones a evitar al replicar el patrón:**

- **No mezclar ejecución y auditoría en el mismo subagente.** Aunque tentador por ahorro de costo, contamina el audit y elimina su valor de detección.
- **No omitir las invariantes (D#) por considerarlas "obvias".** Las contradicciones cross-doc son típicamente invisibles localmente; las invariantes existen para fijar el contexto global de cada subagente.
- **No saltar la documentación del plan al final.** Documentar a posteriori induce racionalización y omite hallazgos incómodos. Documentar durante (como este archivo) preserva la honestidad metodológica.
- **No invocar más subagentes de los necesarios.** Cada subagente es una unidad de coordinación; multiplicarlos sin causa fragmenta la responsabilidad y degrada la calidad del entregable.
- **No usar el patrón para tareas triviales.** Para una tipo en un README, el costo de plan + subagente + audit excede el costo del cambio. Reservar el patrón para refactors no-triviales (≥ 3 archivos afectados o ≥ 1 invariante en juego).

---

## 11. Referencias

- Auditoría inicial (output del subagente Auditor especialista en documentación técnica + DevOps + estándares de la industria — no archivo persistente, está en logs de sesión 2026-04-25).
- `docs/09_devops/estrategia-versionado_v1.0.md` — documento autoritario sobre versionado tras F1.1.
- `docs/09_devops/README.md` — README de sección tras F2.1.
- `CHANGELOG.md` — changelog raíz tras F1.3.
- `docs/09_devops/pipeline-ci-cd_v1.0.md` — pipeline tras F1.2 + F2.2.
- `docs/09_devops/entornos-deploy_v1.0.md` — modelo de distribución tras F1.2 + F2.3.
- `docs/09_devops/guia-publicacion-nuget_v1.0.md` — guía de publicación tras F1.2 + F2.4.
- `LICENSE` — licencia raíz tras A1.
- `docs/00_contexto/acuerdo-equipo_v1.0.md` — acuerdo tras A2.
- `docs/11_examples/README.md` y `docs/11_examples/ejemplo-03-multaapp-nuget.md` — ejemplos tras A3.
- Memoria del asistente: `feedback_workflow.md` (patrón "plan-then-confirm con subagente especializado").
- Estándares de industria invocados: SemVer 2.0.0 (`https://semver.org/spec/v2.0.0.html`); Keep a Changelog 1.1.0 (`https://keepachangelog.com/en/1.1.0/`); Conventional Commits 1.0.0 (`https://www.conventionalcommits.org/en/v1.0.0/`); SLSA (`https://slsa.dev/`); OWASP SCVS (`https://owasp.org/www-project-software-component-verification-standard/`); NIST SSDF SP 800-218 (`https://csrc.nist.gov/publications/detail/sp/800-218/final`); GitHub Flow (`https://docs.github.com/en/get-started/quickstart/github-flow`).
- Documentación oficial relevante: NuGet package metadata (`https://learn.microsoft.com/en-us/nuget/reference/nuspec`); MinVer (`https://github.com/adamralph/minver`); Source Link (`https://github.com/dotnet/sourcelink`); CycloneDX .NET (`https://github.com/CycloneDX/cyclonedx-dotnet`); GitHub Packages para NuGet (`https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry`).
- Convenciones internas del repositorio: `/devs/agentes-especialidades/` (catálogo de roles de subagentes); `/devs/metodologia-sdd/` (fundamentos de la metodología SDD asistida por IA); `/devs/refinamiento_docs/` (patrones de refinamiento documental).

---

## 11.1. Glosario operativo

- **Subagente:** instancia de modelo IA invocada por el asistente principal con un prompt que fija un rol especializado y un alcance acotado. Opera con su propio contexto, separado del asistente principal.
- **Asistente principal:** instancia de modelo IA que orquesta el plan, dialoga con el usuario, despacha subagentes y consolida resultados. No ejecuta tareas especializadas directamente cuando hay un subagente más apropiado.
- **Plan-then-confirm:** patrón en el que el plan se documenta y se confirma antes de la ejecución, con asignación explícita de roles a cada tarea.
- **Audit independiente:** revisión por un subagente que no participó en la ejecución de la fase auditada y que opera sin contexto de las decisiones de ejecución previas, salvo las invariantes formales (D1–D7).
- **Invariante:** decisión del proyecto que se mantiene constante durante todo el plan y que actúa como restricción para todos los subagentes (en este plan: D1–D7).
- **Bloque A:** conjunto de tareas cross-repo de coherencia global, despachadas en F2 junto con las tareas P1.
- **Cross-doc:** atributo de un audit que examina coherencia entre documentos, no solo dentro de un documento.
- **Drift:** divergencia progresiva entre dos artefactos que deberían mantenerse sincronizados (p. ej., documento autoritario vs. configuración real).
- **SLSA:** Supply-chain Levels for Software Artifacts. Marco de niveles de garantía sobre la cadena de suministro de software.
- **OWASP SCVS:** Software Component Verification Standard. Estándar OWASP para verificación de componentes de software.
- **NIST SSDF:** Secure Software Development Framework. Marco NIST (SP 800-218) para desarrollo seguro.
- **SBOM:** Software Bill of Materials. Inventario de componentes de un artefacto, formato CycloneDX o SPDX.
- **SCA:** Software Composition Analysis. Análisis de las dependencias de un proyecto para detectar vulnerabilidades conocidas y licencias incompatibles.
- **Source Link:** mecanismo de Microsoft para que los símbolos `.pdb` apunten a las URLs de código fuente del repositorio, habilitando debugging de paquetes NuGet.
- **MinVer:** herramienta .NET que calcula la versión SemVer del paquete a partir de los tags Git, sin necesidad de mantener un campo `<Version>` en el `.csproj`.
- **Conventional Commits:** convención de mensajes de commit con prefijos semánticos (`feat:`, `fix:`, `chore:`, etc.) y marcador `BREAKING CHANGE` para mayor.

## 12. Control de cambios

| Versión | Fecha       | Autor                                   | Cambios                                                                                            |
|---------|-------------|-----------------------------------------|----------------------------------------------------------------------------------------------------|
| 1.0     | 2026-04-25  | Equipo Arquitectura + agentes IA        | Versión inicial. Plan ejecutado para refactor `/docs/09_devops` + Bloque A (LICENSE, acuerdo-equipo, ejemplos). Fase 1 cerrada con observación B1 resuelta; Fase 2 en ejecución; Fase 3 pendiente. |

---

*Fin del documento.*
