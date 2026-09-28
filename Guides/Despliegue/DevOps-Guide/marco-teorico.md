# Marco Teórico DevOps — Release Engineering de Librerías .NET

**Archivo:** marco-teorico.md
**Versión:** 1.0
**Fecha:** 2026-04-25
**Autor / Owner:** Arquitectura / DevOps — Release Engineering
**Estado:** Aprobado
**Sección:** /devs/devops (material metodológico, no producto)

---

## Disclaimer

Este documento es **marco metodológico**. Reside en `/devs/devops/` por convención del repositorio: separa material teórico/didáctico (`/devs`) de documentación operativa viva del producto (`/docs`). No reemplaza a `/docs/09_devops/` ni se invoca durante la operación diaria de release. Su propósito es proveer la fundamentación industrial 2024-2026 sobre la que descansan las decisiones D1–D7 del Motor DSL y servir como base de comparación cross-domain para equipos que adopten el template fuera del nicho de librerías .NET.

El producto operativo de Release Engineering del Motor DSL es la sección `/docs/09_devops/`. Este documento la **referencia** y **fundamenta**, pero no la duplica.

---

## Cómo leer este documento

| Perfil | Entrada recomendada | Lectura mínima |
|---|---|---|
| Docente que adopta el template | §1 → §2 → §11 → §13 | Marco conceptual + comparativa cross-domain + bibliografía. |
| Alumno que sigue la cátedra | §1 → §3 → §4 → §5 → §6 → §7 → §12 | Métricas, versionado, branching, pipeline y mapeo a las decisiones D1–D7. |
| Practicante de industria | §2 → §7 → §8 → §9 → §11 → §12 | DevOps + pipeline + supply chain + release engineering + comparativa industrial. |

Las secciones §8 (supply chain), §10 (GitOps como anti-aplicación) y §11 (seis contextos) son las de mayor valor diferencial industrial. §12 consolida el caso Motor DSL como aplicación.

---

# 1. Propósito, alcance y posicionamiento

## 1.1. Qué es este documento (y qué no es)

**Es:**

- Marco de referencia industrial 2024-2026 para release engineering de librerías .NET distribuidas como paquetes NuGet.
- Síntesis de especificaciones normativas vigentes (SemVer 2.0.0, Conventional Commits 1.0.0, Keep a Changelog 1.1.0, SLSA v1.0, NIST SSDF SP 800-218, OWASP SCVS v1, CycloneDX 1.6).
- Comparativa cross-domain de seis contextos de industria con el Motor DSL como séptima columna.
- Fundamentación de las decisiones arquitectónicas D1–D5 y D7 listadas en `/docs/09_devops/README.md`.

**No es:**

- Documento operativo. Para ejecutar un release, leer `/docs/09_devops/estrategia-versionado_v1.0.md` §15 y `/docs/09_devops/guia-publicacion-nuget_v1.0.md` §6.
- Guía de onboarding del producto. Para eso existe `/docs/10_developer_guide/` y los README de cada sección operativa.
- Catálogo exhaustivo de tooling. Se citan herramientas que aplican al caso o que ilustran un patrón; no se cubre todo el ecosistema.
- Documento estable. La superficie regulatoria (EU CRA, NIST SSDF, SLSA) está en evolución activa; se versiona con bump MAJOR si la teoría subyacente cambia, no si solo cambia la operativa del Motor DSL.

## 1.2. Caso de estudio: Motor DSL en una línea

Librería .NET 10 LTS de generación de documentos compuesta por 4 paquetes publicables (`MotorDsl.Core`, `MotorDsl.Parser`, `MotorDsl.Rendering`, `MotorDsl.Extensions`), distribuida como artefacto NuGet vía GitHub Packages, versionada con SemVer 2.0.0 + Conventional Commits + MinVer 6.0.0, ramificada con GitHub Flow estricto, validada por una sample app E2E (`MotorDsl.MultaApp.Nuget`) y un pipeline de 15 stages en GitHub Actions distribuido en 5 workflows.

## 1.3. Audiencia y convenciones

El documento asume familiaridad con Git, .NET y CI/CD básico. No explica `git rebase` ni `dotnet build`. Sí explica cuándo bumpear MAJOR y por qué SLSA L2 es el mínimo viable para una librería OSS .NET en 2026.

Convenciones tipográficas:

- `código inline` para nombres de comandos, archivos, identificadores y comandos shell.
- **negrita** para términos al introducirlos por primera vez y para énfasis operativo.
- *cursiva* solo para títulos de obras citadas.
- Citas externas con URL en angle brackets `<https://...>`.
- Bloque "**Aplicación al Motor DSL:**" donde el tema general aterriza en el caso.
- Bloque "**No aplica al caso:**" donde la teoría es relevante para el lector industrial pero no para el Motor DSL (típicamente en §10).
- Anti-patterns marcados explícitamente con la etiqueta "**Anti-pattern:**".

Términos técnicos canónicos en inglés (Pull Request, branch, commit, signing, yanking, attestation, provenance) **no se traducen**. La castellanización degrada la precisión semántica.

## 1.4. Lectura cruzada

Este documento opera junto a:

- `/docs/09_devops/README.md` — índice operativo de la sección DevOps, glosario de 24 términos, RACI, decisiones D1–D5+D7.
- `/docs/09_devops/pipeline-ci-cd_v1.0.md` — pipeline de 15 stages.
- `/docs/09_devops/estrategia-versionado_v1.0.md` — autoridad operativa para versionar.
- `/docs/09_devops/entornos-deploy_v1.0.md` — modelo feeds + consumers downstream.
- `/docs/09_devops/guia-publicacion-nuget_v1.0.md` — operativa NuGet + metadata.
- `/docs/00_contexto/acuerdo-equipo_v1.0.md` — acuerdo de trabajo del equipo.
- `/docs/06_backlog-tecnico/definition-of-ready_v1.0.md` — DoR.
- `/docs/08_calidad_y_pruebas/definition-of-done_v1.0.md` — DoD.
- `/devs/devops/plan-mejoras-devops_v1.0.md` — meta-doc del plan ejecutado para llegar al estado actual.
- `/devs/metodologia-sdd/` — fundamentos del enfoque SDD asistido por IA.

Cuando este documento referencia un archivo del repositorio, lo hace por path relativo desde la raíz del repo. Cuando referencia un anchor interno (`§N`), aplica al propio archivo `/docs/09_devops/<doc>.md` referido.

---

# 2. DevOps: definición operativa y pilares

## 2.1. Definición operativa

DevOps es la disciplina que reduce el tiempo entre la decisión de cambiar el software y el momento en que ese cambio está en manos del usuario, manteniendo o mejorando la confiabilidad. No es un rol, no es un equipo y no es una herramienta. Es un conjunto de prácticas culturales, técnicas y de medición que se evalúan por outcomes — DORA metrics — y no por outputs (cantidad de deploys, líneas de YAML, número de stages).

La distinción operativa relevante en 2024-2026 es la separación entre **DevOps** (entrega), **SRE** (confiabilidad operativa post-deploy), **Platform Engineering** (productividad de los equipos de producto vía plataformas internas) y **DevSecOps** (seguridad integrada al ciclo). En una organización chica (≤30 personas) las cuatro convergen en uno o dos roles. En una organización grande (≥500) son funciones distintas con SLAs propios.

Para el Motor DSL aplica DevOps + DevSecOps con énfasis en supply chain. SRE no aplica como disciplina autónoma porque la librería no tiene runtime propio (ver §2.5). Platform Engineering aplica como consumidor: el repo usa GitHub como plataforma; no construye una propia.

## 2.2. CALMS — Culture, Automation, Lean, Measurement, Sharing

CALMS es el acrónimo introducido por Damon Edwards y John Willis (2010, posteriormente popularizado por Jez Humble) para los cinco pilares de la cultura DevOps. Sigue siendo la rúbrica más usada en madurez organizacional 2024-2026.

| Pilar | Definición | Indicadores observables | Evaluación 0-4 |
|---|---|---|---|
| **Culture** | Responsabilidad compartida entre desarrollo y operaciones; ausencia de silos. | Postmortems blameless. Rotación dev↔ops. Pulls coautoreados. | 0: Silos opacos. 4: Generative (Westrum). |
| **Automation** | Eliminación sistemática del trabajo manual repetible. | % de pipelines automáticos. Provisioning IaC. Tests automáticos pre-merge. | 0: Despliegues manuales. 4: GitOps + IaC + tests E2E. |
| **Lean** | Optimización del flujo end-to-end; reducir batch size, WIP, lead time. | Lead time. WIP limit. Frecuencia de release. | 0: Releases trimestrales con batch grande. 4: On-demand, batch unitario. |
| **Measurement** | Decisión por datos, no por opinión. | DORA metrics. SLI/SLO. Telemetría de producto. | 0: Sin métricas. 4: DORA Elite + SLOs explícitos. |
| **Sharing** | Conocimiento como bien común; documentación viva; comunidad de práctica. | RFCs / ADRs. Postmortems públicos. Internal blog. | 0: Conocimiento tribal. 4: Documentación versionada y descubrible. |

**Aplicación al Motor DSL:**

| Pilar | Estado actual | Evidencia | Brecha |
|---|---|---|---|
| Culture | 3/4 | RACI consolidado en `/docs/09_devops/README.md`; PR obligatorio + reviewer; squash merge. | Postmortems formales aún no instituidos (releases bajo tutela del Tech Lead). |
| Automation | 3/4 | 5 workflows GitHub Actions; pipeline 15 stages; `dotnet pack` + push automatizado; Dependabot. | Análisis automático de breaking changes (`PublicApiAnalyzers`) planeado para v2.x. |
| Lean | 3/4 | GitHub Flow + squash merge → batch unitario por feature; PRs cortos por convención. | Cadencia de release todavía cuasi-manual (release manager dispara tag). |
| Measurement | 3/4 | DORA metrics definidas en `/docs/09_devops/pipeline-ci-cd_v1.0.md` §10.2. | Dashboard externo (Datadog/Grafana) fuera de alcance v1.x. |
| Sharing | 4/4 | `/docs/` versionado; CHANGELOG; release notes; este `marco-teorico.md`; `plan-mejoras-devops_v1.0.md`. | Ninguna identificada. |

Score agregado: 16/20. Brechas conocidas y planificadas; ninguna bloqueante.

## 2.3. Three Ways

Gene Kim formalizó en *The Phoenix Project* (2013) y *The DevOps Handbook* (2016) los **Three Ways** que sostienen la cultura DevOps. La formulación 2024-2026 es:

| Way | Principio | Indicador medible |
|---|---|---|
| **First Way — Flow** | Maximizar el flujo de trabajo del lado izquierdo (desarrollo) al derecho (operaciones / cliente). | Lead Time for Changes; WIP; batch size. |
| **Second Way — Feedback** | Amplificar y acortar los ciclos de feedback en cada etapa. | Time-to-detect; time-to-restore; cobertura de tests; canary fail rate. |
| **Third Way — Continuous Learning** | Cultura de experimentación, aprendizaje y mejora continua; tolerancia al fallo controlado. | Frecuencia de postmortems blameless; chaos engineering days; ADRs publicados. |

**Aplicación al Motor DSL:**

- **Flow.** El squash merge garantiza un commit por feature en `main` (batch unitario). Cada feature dispara `ci.yml` (matriz `ubuntu-latest` + `windows-latest`); cada tag dispara `cd-nuget.yml`. WIP limit implícito por la disciplina de PR cortos del equipo.
- **Feedback.** El pipeline detecta build break o test failure en minutos, no días. Dependabot alerta sobre CVEs en dependencias antes de que un release los herede. La sample app `MotorDsl.MultaApp.Nuget` aporta feedback E2E post-publicación: si no compila, el release está roto aunque los tests internos hayan pasado.
- **Continuous Learning.** El `plan-mejoras-devops_v1.0.md` es la traza del aprendizaje aplicado: hallazgos del audit, priorización P0/P1/P2, ejecución por subagentes especializados. La auditoría externa al inicio (con un reviewer sin contexto previo) es la versión documental del chaos engineering: encontrar fallas que el equipo no ve por familiaridad.

## 2.4. Cultura: tipología Westrum

Ron Westrum (2004) clasificó las culturas organizacionales según cómo procesan la información en tres tipos. *Accelerate* (Forsgren, Humble, Kim, 2018) demostró correlación estadísticamente significativa entre cultura Generative y performance DORA Elite.

| Tipo | Tratamiento de información | Cooperación | Mensajeros con malas noticias | Responsabilidad ante fallos |
|---|---|---|---|---|
| **Pathological** | Suprimida | Baja | Castigados | Asignación de culpa (scapegoating) |
| **Bureaucratic** | Ignorada / compartimentada | Modesta | Tolerados | Responsabilidad limitada al rol |
| **Generative** | Activamente buscada | Alta | Entrenados / recompensados | Responsabilidad compartida; postmortems blameless |

**Aplicación al Motor DSL:** el repo opera en modo Generative observable. Indicadores: el plan de mejoras (`plan-mejoras-devops_v1.0.md`) se generó a partir de una auditoría externa que reportó **defectos bloqueantes (P0)** sin filtros — incluyendo corrupción de archivo, triple contradicción de feed y licencia GPL contradiciendo MIT. El equipo no rechazó los hallazgos: los priorizó, asignó subagentes especializados y ejecutó. Bureaucratic habría tratado los hallazgos como "fuera de alcance del sprint actual"; Pathological habría reasignado la culpa al auditor.

## 2.5. SRE como disciplina hermana

Site Reliability Engineering (SRE) es la disciplina ingenieril de Google (Treynor, 2003) para operar servicios con confiabilidad cuantificada. Su contribución conceptual al canon DevOps:

| Concepto | Definición | Aplicación clásica (servicios) |
|---|---|---|
| **SLI** (Service Level Indicator) | Métrica de un atributo de servicio (latencia p99, error rate, throughput). | "Latencia p99 del endpoint `/checkout`". |
| **SLO** (Service Level Objective) | Target sobre un SLI medido en una ventana temporal. | "p99 ≤ 200 ms en cualquier ventana de 30 días, 99.9% del tiempo". |
| **SLA** (Service Level Agreement) | Compromiso contractual con el cliente, usualmente con penalidad económica. | "p99 ≤ 250 ms o crédito del 5%". |
| **Error budget** | Tolerancia al fallo derivada del SLO (1 - SLO). | SLO 99.9% → error budget 0.1% = 43.2 min/mes. |
| **Burn rate alerting** | Alerta proactiva cuando el consumo del error budget excede una velocidad sostenible. | "Burning 14.4× el budget en la última hora → alerta". |
| **Toil** | Trabajo manual, repetitivo, sin valor duradero. SRE lo cuantifica y limita (≤50% del tiempo). | Reset manual de réplicas tras failover. |

**Adaptación SRE para librerías:** la formulación clásica asume que existe un servicio observable en runtime. Una librería no tiene proceso propio (ver `/docs/09_devops/entornos-deploy_v1.0.md` §3.1). La adaptación industrial 2024-2026 reformula los SLIs para librerías OSS:

| SLI clásico (servicio) | SLI equivalente (librería) | Cómo se mide |
|---|---|---|
| Latencia p99 | Time-to-first-byte de `dotnet restore` | Métricas de feed (GitHub Packages). |
| Error rate runtime | Crash rate del consumer atribuible a la lib | Telemetría del consumer (Crashlytics, App Center). |
| Disponibilidad | Disponibilidad del feed | SLA del proveedor (GitHub). |
| Tiempo de recuperación | Time-to-PATCH ante CVE confirmado | GitHub Issues `created_at` → release `published_at`. |
| Frescura del catálogo | Frecuencia de release | DORA Deployment Frequency. |

El **error budget** para una librería se reformula como tolerancia a regresiones binarias: cuántas versiones por trimestre pueden requerir un PATCH dentro de las 24 h post-publicación (DORA Change Failure Rate aplicado a release). El Motor DSL adopta implícitamente un target Elite (≤5%) por la disciplina de gates pre-merge.

## 2.6. Platform Engineering en una línea

Platform Engineering es la disciplina de construir Internal Developer Platforms (IDPs) que reducen carga cognitiva del equipo de producto y aceleran su flujo. Referencia canónica 2024: CNCF Platform Engineering Maturity Model (2024); herramienta dominante: Backstage (CNCF Incubating, originada en Spotify 2016, donada 2020).

**Aplicación al Motor DSL:** el repo es **consumidor** de una plataforma (GitHub: identity, CI runners, package registry, security advisories, secret scanning). No construye una plataforma propia; el costo de hacerlo no se justifica para un proyecto académico-industrial. La decisión D1 (GitHub Packages como único feed) es coherente con esta postura: minimizar superficie operativa apoyándose en la plataforma del proveedor.

---

# 3. Métricas: DORA y más allá

## 3.1. Las cuatro métricas DORA

DORA (DevOps Research and Assessment), originada en el grupo de Nicole Forsgren, Jez Humble y Gene Kim (2014), absorbida por Google Cloud (2018) y rebautizada periódicamente desde 2014, publica el *State of DevOps Report* anual desde 2014. Las cuatro métricas core operacionalizan el rendimiento de entrega de software.

| Métrica | Definición operativa | Elite | High | Medium | Low |
|---|---|---|---|---|---|
| **Lead Time for Changes** | Tiempo entre el primer commit en un branch y la disponibilidad de ese cambio en producción. | <1 hora | 1 día - 1 semana | 1 semana - 1 mes | >1 mes |
| **Deployment Frequency** | Frecuencia con la que la organización despliega cambios exitosos a producción. | On-demand (varias veces por día) | 1/día - 1/semana | 1/semana - 1/mes | <1/mes |
| **Change Failure Rate** | Porcentaje de despliegues que requieren remediación (rollback, hotfix, fix-forward). | 0–5% | ~10% | ~16% | ~64% |
| **Failed Deployment Recovery Time** (ex-MTTR) | Tiempo para restaurar el servicio tras un deploy fallido. | <1 hora | <1 día | <1 semana | >1 semana |

Distribución 2023 (último año con dataset estable): Elite 18%, High 31%, Medium 33%, Low 17%. La denominación "MTTR" fue retirada explícitamente en *Accelerate State of DevOps Report 2023* por confusión con MTTR de incident management; la métrica se renombró **Failed Deployment Recovery Time** para acotar el alcance al post-deploy.

Citas: <https://dora.dev/research/>, <https://cloud.google.com/devops/state-of-devops/>.

## 3.2. Reliability y Operational Performance

A partir de 2021, DORA agregó **Reliability** como quinta métrica (cumplimiento del SLO o equivalente operativo) para evitar que organizaciones optimicen velocidad a costa de estabilidad. En 2023 introdujo **Operational Performance** como constructo compuesto que combina Reliability con disponibilidad y satisfacción operativa.

La interpretación industrial 2024-2026: las cuatro métricas core son **necesarias pero no suficientes**. Una organización que despliega 50 veces por día con Change Failure Rate 4% pero cuyo SLO de disponibilidad está al 95% (objetivo Elite ≥99.9% para servicios críticos) **no es Elite**. Reliability rompe el simplismo de "más rápido = mejor".

## 3.3. Las 24 capabilities

*Accelerate* identifica 24 **capabilities** organizadas en 5 categorías que predicen estadísticamente performance DORA Elite. Las categorías son:

1. **Continuous Delivery** (8 capabilities): version control, deployment automation, CI, trunk-based development, test automation, test data management, shift left on security, continuous delivery.
2. **Architecture** (2): loosely-coupled architecture, empowered teams.
3. **Product & Process** (4): customer feedback, value stream visibility, working in small batches, team experimentation.
4. **Lean Management & Monitoring** (5): change approval (lightweight), monitoring, proactive notification, WIP limits, visualizing work.
5. **Cultural** (5): Westrum culture, supportive leadership, transformational leadership, job satisfaction, learning culture.

La trayectoria Low → Elite no es lineal: un equipo Low que optimiza solo "más deploys por día" sin las capabilities estructurales (test automation, trunk-based, monitoring) entra en zona de **alta velocidad + alta tasa de fallos**, peor que el Low original. La progresión típica es: capabilities culturales y de proceso → automation → frecuencia → mantenimiento de Reliability.

## 3.4. DORA aplicado a una librería

La reinterpretación de DORA para una librería NuGet OSS está formalizada en `/docs/09_devops/pipeline-ci-cd_v1.0.md` §10.2. Resumen normativo:

| Métrica DORA | Reinterpretación para librería | Fuente de medición |
|---|---|---|
| Lead Time for Changes | Tiempo entre primer commit en branch feature y merge a `main`. | GitHub API: `PR.commits[0].author.date` → `PR.merged_at`. |
| Deployment Frequency | Cantidad de tags `v*` publicados por mes (proxy de releases estables). | GitHub Releases API. |
| Change Failure Rate | % de releases que requieren PATCH dentro de 24 h post-publicación. | Comparación temporal entre tags. |
| Failed Deployment Recovery Time | Tiempo entre apertura de issue de regresión y release del PATCH. | GitHub Issues `created_at` → release `published_at`. |
| Reliability (adaptado) | % de versiones publicadas que no fueron yankeadas. | GitHub Packages + advisories. |

**Aplicación al Motor DSL:** el target operativo es **High en Lead Time y Deployment Frequency** y **Elite en Change Failure Rate y Recovery Time**. La asimetría es deliberada: una librería con audiencia industrial puede tolerar releases semanales (no on-demand), pero **no** tolera CFR alto porque cada PATCH defectuoso fuerza yanking o pérdida de confianza del consumer downstream.

## 3.5. Anti-patterns de medición

**Anti-pattern: vanity metrics.** Reportar "número de deploys por mes" sin Change Failure Rate. Una organización que pasa de 10 deploys/mes con 5% CFR a 100 deploys/mes con 30% CFR ha empeorado, aunque la métrica primaria suba 10×.

**Anti-pattern: gaming.** Splittear PRs grandes en muchos PRs triviales para inflar Deployment Frequency. Detectable comparando Lead Time (cae artificialmente) con WIP (sube).

**Anti-pattern: medir individuos.** DORA mide equipos, no personas. Aplicar DORA a evaluación individual produce optimización local destructiva (cherry-picking de tareas fáciles, evitar cambios riesgosos pero necesarios).

**Anti-pattern: medir lo que es fácil de medir, ignorando lo importante.** Lead Time es trivial de extraer del API; Reliability requiere telemetría de consumers. Reportar solo Lead Time produce falsos positivos de "Elite".

**Anti-pattern: targets de quartile sin contexto.** Decir "queremos ser Elite" sin entender que Elite en un banco con compliance regulatorio implica trade-offs distintos que Elite en un SaaS B2C. Los benchmarks DORA son referencia, no objetivo absoluto.

---

# 4. Versionado: SemVer, CalVer, ZeroVer

## 4.1. SemVer 2.0.0

Semantic Versioning 2.0.0 (Tom Preston-Werner, 2013, estable) es la especificación normativa adoptada por el ecosistema NuGet, npm, Cargo, Maven (con extensiones), Go modules y la mayoría de OSS donde la API pública es contrato.

**Formato:**

```text
MAJOR.MINOR.PATCH[-PRERELEASE][+BUILDMETADATA]
```

**11 reglas formales** (resumen normativo de <https://semver.org/spec/v2.0.0.html>):

1. Software con API pública debe declarar versión.
2. Formato `X.Y.Z` con `X`, `Y`, `Z` enteros no negativos sin ceros a la izquierda.
3. Una vez publicada, una versión **no se modifica**. Cambios requieren nueva versión.
4. `0.y.z` indica desarrollo inicial; cualquier cosa puede cambiar; API pública no debe considerarse estable.
5. `1.0.0` define la API pública. La forma en que se incrementa la versión a partir de ahí depende de la API y de cómo cambia.
6. PATCH `Z` (`x.y.Z | x > 0`) bumpea ante bug fixes retrocompatibles.
7. MINOR `Y` (`x.Y.z | x > 0`) bumpea ante nueva funcionalidad retrocompatible o ante deprecación de funcionalidad. Reset PATCH.
8. MAJOR `X` (`X.y.z | X > 0`) bumpea ante cualquier cambio incompatible. Reset MINOR y PATCH.
9. PRERELEASE: identificadores ASCII alfanuméricos + hyphen separados por `.`. Identificadores numéricos sin ceros a la izquierda. Tiene precedencia menor que la versión sin pre-release.
10. BUILDMETADATA: identificadores ASCII alfanuméricos + hyphen separados por `.`. Se ignora al determinar precedencia.
11. Precedencia: comparar MAJOR, MINOR, PATCH numéricamente. Si iguales, versión sin pre-release tiene mayor precedencia. Si ambas tienen pre-release, comparar identificadores izq→der: numéricos comparados numéricamente, alfanuméricos lexicográficamente; campos con menos identificadores tienen menor precedencia si todos los identificadores precedentes son iguales.

**Tabla de precedencia (verbatim spec §11):**

```text
1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta < 1.0.0-beta <
1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0
```

`1.0.0+20130313144700` y `1.0.0+exp.sha.5114f85` son **iguales en precedencia** (build metadata ignorado).

**BNF simplificado del formato:**

```bnf
<valid semver>     ::= <version core> | <version core> "-" <pre-release> |
                       <version core> "+" <build> | <version core> "-" <pre-release> "+" <build>
<version core>     ::= <major> "." <minor> "." <patch>
<major>            ::= <numeric identifier>
<minor>            ::= <numeric identifier>
<patch>            ::= <numeric identifier>
<pre-release>      ::= <dot-separated pre-release identifiers>
<build>            ::= <dot-separated build identifiers>
```

**Anti-patterns SemVer comunes:**

- **Anti-pattern: bumpear MINOR ante breaking change.** Viola Regla 8. Frecuente cuando el equipo confunde "feature nueva" con "feature retrocompatible".
- **Anti-pattern: usar BUILDMETADATA para distinguir preview vs estable** (`1.0.0+preview` vs `1.0.0`). El metadato se ignora en precedencia; ambos son la misma versión a efectos del resolver. Lo correcto es PRERELEASE: `1.0.0-preview.1` vs `1.0.0`.
- **Anti-pattern: ZeroVer como excusa permanente.** Mantener `0.x.y` indefinidamente para evitar disciplina SemVer. Ver §4.3.
- **Anti-pattern: re-publicación de versión existente.** Viola Regla 3. Algunos feeds (NuGet.org, GitHub Packages) lo previenen por diseño; otros (npm con `--force`) no, pero la práctica está prohibida en cualquier organización seria.
- **Anti-pattern: bump de PATCH ante cambio de comportamiento documentado.** "Es solo un fix" no aplica si el comportamiento previo era documentado y consumidores dependían de él. Es MAJOR.

**Aplicación al Motor DSL:** SemVer 2.0.0 adoptado sin modificaciones. Detalle en `/docs/09_devops/estrategia-versionado_v1.0.md` §3. La librería parte directamente desde `1.0.0` (no ZeroVer); las dos superficies públicas (API .NET + DSL JSON) se versionan conjuntamente con disciplina estricta documentada en `estrategia-versionado_v1.0.md` §4.

## 4.2. CalVer

Calendar Versioning (Mahmoud Hashemi, 2016, <https://calver.org>) es el esquema alternativo que codifica fecha en la versión. No reemplaza a SemVer; es apropiado en contextos donde la noción de "API pública estable" no encaja: distribuciones, productos con cadencia temporal forzada, tooling con compromiso de soporte por fecha.

**Esquemas comunes:**

| Esquema | Formato | Ejemplo |
|---|---|---|
| `YYYY.0M.0D` | Año + mes + día con padding | `2026.04.25` |
| `YY.0M` | Año corto + mes (LTS-style) | `26.04` |
| `YYYY.MINOR.MICRO` | Año + minor + micro (estilo IntelliJ) | `2026.1.0` |
| `YY.MM.MICRO` | Año + mes + micro (estilo Black) | `26.4.0` |

**Adopción industrial verificada 2024-2026:**

| Producto | Esquema | Uso |
|---|---|---|
| Ubuntu LTS | `YY.0M` | 24.04 LTS, 26.04 LTS. |
| JetBrains (IntelliJ, PyCharm, Rider) | `YYYY.MINOR.MICRO` | 2024.1, 2024.3, 2025.1. |
| pip | `YY.0` | 24.0, 24.2. |
| Black (Python formatter) | `YY.MM.MICRO` | 24.3.0. |
| yt-dlp | `YYYY.0M.0D` | 2024.04.09. |
| Stripe API | `YYYY-MM-DD` | versiones de API por fecha de release. |

**Decision guide CalVer vs SemVer:**

- ¿La librería tiene API pública con contrato estable y consumers pinneando versiones? → **SemVer**.
- ¿El producto tiene compromiso de soporte por fecha calendario (LTS) o cadencia temporal forzada (release mensual obligatorio)? → **CalVer**.
- ¿La librería es interna (consumers controlados, sin diamond dependency)? → cualquiera; en la práctica SemVer por costumbre.
- ¿Híbrido (CalVer público + SemVer interno)? → ver Stripe API: CalVer en endpoint, SemVer en SDKs.

**Aplicación al Motor DSL:** SemVer. La librería tiene API pública contractual con consumers pinneando versiones (Diamond dependency es riesgo real). CalVer no aplica.

## 4.3. ZeroVer — anti-pattern documentado

ZeroVer (<https://0ver.org>) es el "esquema" satírico que documenta el anti-pattern de mantener `0.x.y` indefinidamente. La página categoriza proyectos famosos que llevan años en `0.x` (Kubernetes en 1.0 tras 7 años; muchos siguen en 0.x permanente).

El problema operativo: bajo SemVer Regla 4, `0.x.y` declara que **cualquier cambio puede ser breaking** y libera al maintainer de la disciplina de bumpear MAJOR. En la práctica esto produce dos patologías:

1. **Consumers ignoran la advertencia** y pinean a `^0.5.0` esperando estabilidad. Cuando el maintainer libera `0.6.0` con breaking changes, los consumers rompen sin aviso.
2. **El maintainer evita `1.0.0` indefinidamente** para no comprometerse a estabilidad. La librería madura técnicamente pero nunca formaliza el contrato.

**Anti-pattern: ZeroVer como evasión.** El Motor DSL lo evita explícitamente partiendo desde `1.0.0` con disciplina SemVer plena (`/docs/09_devops/estrategia-versionado_v1.0.md` §3.2).

## 4.4. Auto-versioning tooling

Tabla comparativa de las herramientas de auto-versioning relevantes en 2024-2026:

| Tool | Stack | Mecanismo | Estado | Adopción |
|---|---|---|---|---|
| **MinVer** | .NET | Git tag-based (height + tag prefix) | Estable, mantenido por Adam Ralph | NuGet OSS, .NET Foundation; **caso Motor DSL**. |
| **Nerdbank.GitVersioning** (NBGV) | .NET | `version.json` versionado + commit height | Estable, Microsoft | Microsoft enterprise; libs internas. |
| **GitVersion** | .NET, multi | Branch + commit message inference | Estable, legacy enterprise | GitFlow legacy. |
| **semantic-release** | JS / Node | Conventional Commits + plugins | Estable, lider npm | npm OSS dominante. |
| **release-please** | Multi (Go binary) | Conventional Commits + release PRs | Estable, Google | Go, Node, Python OSS. |
| **changesets** | JS / pnpm | Markdown changesets manuales | Estable, lider monorepo | pnpm/Turborepo monorepos. |
| **git-cliff** | Multi (Rust binary) | Conventional Commits → CHANGELOG | Estable, mantenido | Rust + multilenguaje. |

**Comparativa por trade-off:**

| Eje | MinVer | NBGV | semantic-release | release-please |
|---|---|---|---|---|
| Necesita servidor | No | No | No | No |
| Necesita archivo en repo | No (solo tag) | Sí (`version.json`) | No (config opcional) | Sí (`release-please-config.json`) |
| Determinismo | Alto (función de tag + height) | Alto | Medio (depende de plugins) | Alto |
| Curva de aprendizaje | Baja | Media | Alta | Media |
| Soporte multi-package | Vía `Directory.Build.props` | Sí | Vía monorepo plugin | Sí (path-based) |
| Mantenedor | Adam Ralph (community) | Microsoft | Pioneers / community | Google |

**Aplicación al Motor DSL:** MinVer 6.0.0 (configuración en `src/Directory.Build.props`, detalle en `/docs/09_devops/estrategia-versionado_v1.0.md` §8.6). La elección se justifica por: (a) configuración mínima, (b) determinismo "misma rev → misma versión", (c) sin commits de bump ni archivos versionados, (d) compatibilidad nativa con `dotnet pack` sin pasos extra.

## 4.5. Keep a Changelog 1.1.0

Keep a Changelog (Olivier Lacan, 2014, versión actual 1.1.0) es la especificación de formato de CHANGELOG dominante en OSS. URL normativa: <https://keepachangelog.com/en/1.1.0/>.

**Seis categorías obligatorias por release:**

| Categoría | Uso |
|---|---|
| **Added** | Nueva funcionalidad. Mapea a `feat:` (Conventional Commits). |
| **Changed** | Cambio en funcionalidad existente, no breaking salvo cuando lo es. Mapea a `feat!:` o `BREAKING CHANGE:`. |
| **Deprecated** | Funcionalidad marcada para futura remoción. |
| **Removed** | Funcionalidad eliminada. |
| **Fixed** | Bug fix. Mapea a `fix:`. |
| **Security** | Fix de vulnerabilidad. Especialmente sensible: comunicar siempre. |

**Anti-patterns CHANGELOG:**

- **Anti-pattern: dump de commits.** Pegar `git log --oneline` no es CHANGELOG. El CHANGELOG es para humanos consumers; los commits son para developers contribuidores.
- **Anti-pattern: ausencia de fechas.** Cada versión debe llevar fecha ISO. Sin fecha, no se puede correlacionar un advisory CVE con la versión publicada.
- **Anti-pattern: editar versiones publicadas.** Una vez liberada, la sección `[X.Y.Z]` es inmutable. Correcciones se hacen en la sección de la siguiente versión.
- **Anti-pattern: sección `Security` ausente cuando hubo CVE.** Esconder fixes de seguridad por miedo reputacional. Lo correcto es lo opuesto: explicitar el CVE, severidad, vector y mitigación.

**Tooling automático:**

- **release-drafter** (GitHub Action) genera draft de release notes desde PR labels.
- **git-cliff** genera CHANGELOG completo desde Conventional Commits.
- **release-please** mantiene el CHANGELOG vía PRs automáticos.

**Aplicación al Motor DSL:** Keep a Changelog 1.1.0 adoptado. Ejemplo y reglas en `/docs/09_devops/estrategia-versionado_v1.0.md` §9.2. Generación automática planeada para v2.x del pipeline (`pipeline-ci-cd_v1.0.md` §13).

## 4.6. Inmutabilidad y yanking

**Inmutabilidad** es la propiedad de que una versión publicada no puede modificarse. Es prerequisito de SemVer Regla 3 y de la integridad operativa de cualquier feed serio. GitHub Packages, NuGet.org, Maven Central y crates.io la garantizan por diseño: rechazan re-push de la misma versión.

**Yanking** es la eliminación de una versión publicada del feed. Distinto de **unlisting** (semántica NuGet.org: versión oculta pero instalable; el lockfile que la pinea sigue funcionando). En GitHub Packages no existe unlisting; solo hay delete destructivo que rompe lockfiles.

**Política industrial 2024-2026 para yanking:**

| Severidad CVE | Acción |
|---|---|
| CVSS ≥ 9.0 (crítico) | Yanking procede si hay versión sustituta publicada simultáneamente. |
| CVSS 7.0–8.9 con vector remoto sin auth (alto) | Yanking procede. |
| CVSS 7.0–8.9 sin vector remoto | Publicar PATCH; no yankear. |
| CVSS <7.0 | Publicar PATCH; sección `Security` en CHANGELOG; no yankear. |

**Aplicación al Motor DSL:** threshold yanking **CVSS ≥ 9.0** o alto con vector remoto sin auth, con aprobación dual Tech Lead + Security y versión sustituta publicada antes del delete. Detalle: `/docs/09_devops/estrategia-versionado_v1.0.md` §13.

---

# 5. Conventional Commits y trazabilidad

## 5.1. Especificación 1.0.0

Conventional Commits 1.0.0 (Angular Team, formalizado 2017, estable) es la especificación de formato de mensaje de commit dominante en OSS 2024-2026. URL normativa: <https://www.conventionalcommits.org/en/v1.0.0/>.

**BNF simplificado:**

```bnf
<commit>     ::= <type> [ "(" <scope> ")" ] [ "!" ] ":" <description>
                 [ <body> ]
                 [ <footers> ]
<type>       ::= "feat" | "fix" | "docs" | "style" | "refactor" |
                 "perf" | "test" | "build" | "ci" | "chore" | "revert"
<scope>      ::= identifier
<description>::= one-line summary, imperative mood, ≤72 chars
<footer>     ::= "BREAKING CHANGE:" <text> | <token> ":" <text> | <token> "#" <text>
```

**11 tipos canónicos:**

| Tipo | Uso | Bump SemVer |
|---|---|---|
| `feat` | Nueva funcionalidad. | MINOR |
| `fix` | Bug fix retrocompatible. | PATCH |
| `perf` | Mejora de performance sin cambio de comportamiento. | PATCH |
| `refactor` | Refactor interno sin impacto público. | PATCH |
| `docs` | Solo documentación. | none / PATCH |
| `style` | Formato sin cambio funcional. | none / PATCH |
| `test` | Solo tests. | none / PATCH |
| `build` | Sistema de build, dependencias dev. | none / PATCH |
| `ci` | Pipeline CI/CD. | none / PATCH |
| `chore` | Tareas misceláneas. | none / PATCH |
| `revert` | Revertir un commit previo. | depende del revertido |

**Marcado de breaking change** (dos formas equivalentes):

- `feat!:` o `fix!:` — el `!` después del tipo/scope.
- Footer `BREAKING CHANGE:` con descripción.

Ambos disparan bump MAJOR. El footer prevalece como fuente de verdad cuando hay ambigüedad.

**Mapeo SemVer canónico:**

| Commit | Bump |
|---|---|
| `feat:` | MINOR |
| `fix:` | PATCH |
| `feat!:` o footer `BREAKING CHANGE:` | MAJOR |
| `perf:`, `refactor:` | PATCH |
| `docs:`, `style:`, `test:`, `ci:`, `build:`, `chore:` | none (en la práctica PATCH si se libera) |

## 5.2. Tooling

| Herramienta | Rol | Stack |
|---|---|---|
| **commitlint** | Lint de mensajes de commit en pre-commit hook o CI. | Node. |
| **commitizen** | CLI interactivo que guía la escritura del commit. | Multi (Node, Python). |
| **husky** | Manager de Git hooks en proyectos Node/JS. | Node. |
| **lefthook** | Manager de Git hooks multi-stack. | Go binary. |
| **wagoid/commitlint-github-action** | GitHub Action que ejecuta commitlint en PRs. | GitHub Actions. |
| **conventional-changelog** | Generador de CHANGELOG desde commits. | Node. |
| **git-cliff** | Generador de CHANGELOG en Rust, multi-stack. | Rust binary. |

## 5.3. Anti-patterns

- **Anti-pattern: scope inventado por commit.** Cada PR usa un scope distinto, perdiendo la utilidad de filtrado posterior. Mitigación: tabla de scopes válidos en el repo (Motor DSL: `/docs/09_devops/estrategia-versionado_v1.0.md` §8.4).
- **Anti-pattern: `chore` como cajón de sastre.** Usar `chore` para todo lo que no se quiere clasificar. Pierde el mapeo SemVer. Mitigación: rechazar en review.
- **Anti-pattern: descripción no imperativa.** "Added support for X" en lugar de "add support for X". Inconsistente con el resto de los commits del repo y con el changelog.
- **Anti-pattern: breaking change sin footer.** Usar `feat:` (sin `!`) para algo breaking porque "es solo un cambio menor". Rompe el bump SemVer automático.
- **Anti-pattern: 50 commits intermedios en un branch que termina en squash sin mensaje convencional.** Si el squash final no respeta el formato, el bump no se calcula. Mitigación: el reviewer valida el título del PR antes de aprobar el merge.

## 5.4. Trazabilidad commit → versión → changelog → tag

La cadena de trazabilidad de un cambio en un proyecto que adopta Conventional Commits + SemVer + Keep a Changelog + auto-versioning es:

```text
PR feature/parser-trim
  ├── commits intermedios
  └── squash merge → commit en main
        "feat(parser): support trim() in expression evaluator"
              │
              ▼
        Conventional Commits parser
              │
              ▼
        Bump SemVer = MINOR (feat:)
              │
              ▼
        Tag firmado v1.5.0
              │
              ▼
        MinVer detecta tag → versión 1.5.0
              │
              ▼
        Pipeline pack + push
              │
              ▼
        4 paquetes publicados (Motor DSL: Core, Parser, Rendering, Extensions 1.5.0)
              │
              ▼
        CHANGELOG.md sección [1.5.0] - 2026-04-25
              │
              ▼
        GitHub Release v1.5.0 con notes
```

**Inmutabilidad transversal:**

| Artefacto | Inmutabilidad |
|---|---|
| Squash commit en `main` | Inmutable (linear history + protected branch). |
| Tag Git firmado | Inmutable (protected tags). |
| Paquete `.nupkg` | Inmutable (feed rechaza re-push). |
| Entrada CHANGELOG `[X.Y.Z]` | Inmutable post-release. |
| GitHub Release | Editable (notes), pero el tag asociado no. |

**Aplicación al Motor DSL:** la cadena está formalizada en `/docs/09_devops/estrategia-versionado_v1.0.md` §9.1. El RACI de quién valida cada eslabón está en `/docs/09_devops/README.md` §RACI.

---

# 6. Estrategias de branching

## 6.1. GitHub Flow

GitHub Flow (Scott Chacon, GitHub, 2011) es la estrategia minimalista: una sola rama persistente (`main` siempre liberable), feature branches efímeras, PR obligatorio, deploy continuo desde `main`. URL: <https://docs.github.com/en/get-started/using-github/github-flow>.

| Atributo | Valor |
|---|---|
| Branches persistentes | 1 (`main`) |
| Branches efímeras | `feature/*` (vida hasta el merge) |
| Releases | Tag sobre `main` |
| Hotfixes | Otra `feature/fix-*` con commit `fix:` |
| Requisito CI | Tests verdes pre-merge |
| Adecuado para | Web SaaS, librerías OSS modernas, equipos pequeños-medianos. |
| Adecuado para LTS multi-version | Mediano (con cherry-pick disciplinado). |

## 6.2. GitFlow

GitFlow (Vincent Driessen, 2010) es la estrategia clásica con dos branches persistentes (`main` + `develop`) y branches de soporte (`feature/*`, `release/*`, `hotfix/*`).

| Atributo | Valor |
|---|---|
| Branches persistentes | 2 (`main`, `develop`) |
| Branches efímeras | `feature/*`, `release/*`, `hotfix/*` |
| Releases | Branch `release/*` → merge a `main` + `develop` + tag |
| Hotfixes | `hotfix/*` desde `main` → merge a `main` + `develop` |
| Adecuado para | Productos con releases versionados, soporte multi-version, equipos grandes. |
| Adecuado para CI/CD continuo | Bajo (la doble integración duplica trabajo). |

**Disclaimer del propio autor (Vincent Driessen, 2020, addendum a la nota original; verbatim, citar literalmente cuando se documente la decisión de no adoptar GitFlow):**

> "If your team is doing continuous delivery of software, I would suggest to adopt a much simpler workflow (like GitHub flow) instead of trying to shoehorn git-flow into your team."
>
> "If, however, you are building software that is explicitly versioned, or if you need to support multiple versions of your software in the wild, then git-flow may still be as good of a fit to your team."

Cita: <https://nvie.com/posts/a-successful-git-branching-model/>.

## 6.3. Trunk-Based Development

Trunk-Based Development (Paul Hammant et al., formalizado 2017, <https://trunkbaseddevelopment.com>) es la estrategia de los equipos de hyperscale. Una sola rama (`main` o `trunk`), commits directos o branches de muy corta vida (≤1 día), feature flags **mandatorios** para desacoplar deploy de release.

| Atributo | Valor |
|---|---|
| Branches persistentes | 1 |
| Branches efímeras | Opcional, ≤1 día |
| Feature flags | Mandatorios |
| Releases | Por flag flip (no por merge ni por tag) |
| Adecuado para | Web SaaS hyperscale (Google, Meta, Netflix), monorepos, microservicios. |
| Inadecuado para | Librerías versionadas (no hay forma de "flag flip" un consumer downstream). |

## 6.4. Release Flow (Microsoft)

Release Flow (Edward Thomson et al., Microsoft DevOps blog, 2018) es una variante derivada de TBD adaptada a productos con cadencia de release definida y necesidad ocasional de hotfix multi-version. Una `main`, `release/*` efímeras por release, cherry-pick para hotfixes.

| Atributo | Valor |
|---|---|
| Branches persistentes | 1 (`main`) |
| Branches efímeras | `release/<version>` |
| Hotfixes | Cherry-pick desde `main` a `release/*` afectada |
| Adecuado para | Productos enterprise con releases periódicos (Office, VS Code). |

## 6.5. OneFlow

OneFlow (Adam Ruka, 2015, <https://www.endoflineblog.com/oneflow-a-git-branching-model-and-workflow>) es una variante simplificada de GitFlow: una sola rama persistente, releases por tag, hotfix branches efímeras.

| Atributo | Valor |
|---|---|
| Branches persistentes | 1 (`main` o `master`) |
| Branches efímeras | `feature/*`, `hotfix/*`, `release/*` opcional |
| Releases | Tag sobre `main` |
| Adecuado para | Equipos que quieren los beneficios de GitFlow sin la complejidad. |

## 6.6. Decision tree

```text
¿Releases versionados con consumers downstream pinneando versión?
├─ Sí → ¿Soporte multi-version simultáneo (LTS y mainline)?
│       ├─ Sí → GitFlow o Release Flow
│       └─ No → GitHub Flow estricto
└─ No (deploy continuo, no hay "versión" expuesta a consumers)
        ├─ Equipo > 100 con monorepo → Trunk-Based Development + feature flags
        └─ Equipo < 100, deploy continuo → GitHub Flow o TBD
```

**Comparativa exhaustiva:**

| Criterio | GitHub Flow | GitFlow | TBD | Release Flow | OneFlow |
|---|---|---|---|---|---|
| Complejidad | Baja | Alta | Baja | Media | Media |
| Branches persistentes | 1 | 2 | 1 | 1 + efímeras | 1 |
| Hotfixes | `feature/fix-*` | `hotfix/*` | Feature flag flip | Cherry-pick a `release/*` | `hotfix/*` |
| Velocidad release | Alta | Baja | **Máxima** | Alta | Media |
| Equipos < 10 | Excelente | Sobreingeniería | Bueno con feature flags | Sobreingeniería | Bueno |
| Equipos > 500 | Difícil | Difícil | **Mandatorio en hyperscale** | Excelente | Difícil |
| Releases LTS multi-version | Bueno con disciplina | **Excelente** | Requiere disciplina alta | Excelente | Bueno |
| CI/CD continuo | Excelente | Regular (doble merge) | **Excelente** | Excelente | Bueno |
| Requisito feature flags | Recomendado | No | **Mandatorio** | Recomendado | No |

## 6.7. Caso de deuda documental: contradicción en `/devs/devops/guia-flujo-trabajo-versionado.md`

El archivo `/devs/devops/guia-flujo-trabajo-versionado.md` (Sección 1) declara dos branches protegidas, `main` y `homologacion`, con flujo `issue → homologacion → main`. Este modelo es incompatible con la decisión D3 (GitHub Flow estricto, una sola rama persistente) confirmada en `/docs/09_devops/estrategia-versionado_v1.0.md` §7. Se documenta la contradicción aquí; su resolución (actualización del archivo `/devs` o nota explícita de obsolescencia) es deuda documental conocida, planificada en una iteración futura del plan de mejoras (`plan-mejoras-devops_v1.0.md`).

**Aplicación al Motor DSL:** GitHub Flow estricto. Detalle operativo y reglas de protección de `main` en `/docs/09_devops/estrategia-versionado_v1.0.md` §7.4.

---

# 7. Pipeline CI/CD

## 7.1. Definiciones

| Término | Definición operativa |
|---|---|
| **CI** (Continuous Integration) | Práctica de integrar cambios a un trunk con frecuencia ejecutando build, tests y análisis automático en cada PR. |
| **CD — Continuous Delivery** | Cada cambio integrado pasa por un pipeline que produce un artefacto desplegable; el deploy final puede requerir aprobación manual. |
| **CD — Continuous Deployment** | Cada cambio integrado se despliega automáticamente a producción si pasa el pipeline. Subset de Continuous Delivery sin gate manual. |
| **CT** (Continuous Testing) | Práctica de ejecutar tests automáticos en cada etapa del pipeline (no solo al final). |

La distinción Delivery / Deployment es relevante para librerías: el **Delivery** del Motor DSL es automático (tag → publicación al feed). No hay **Deployment** en el sentido de servicio runtime; el deploy lo hace el consumer en su pipeline.

## 7.2. Anatomía de un pipeline

| Elemento | Definición |
|---|---|
| **Trigger** | Evento que dispara el pipeline (push, PR, tag, schedule, manual). |
| **Stage** | Agrupación lógica de pasos (build, test, pack, sign, publish). |
| **Step / Job** | Unidad ejecutable atómica. |
| **Gate** | Condición que debe cumplirse para avanzar al siguiente stage (cobertura ≥70%, tests verdes, sin CVE crítico). |
| **Artifact** | Archivo producido por un stage y consumido por otro (típicamente persistido en el runner host). |
| **Cache** | Almacén compartido entre runs para acelerar restore/build. |
| **Secret** | Credencial inyectada en runtime, nunca commitada. |
| **Environment** | Concepto de GitHub Actions: contexto con secrets y reglas de protección (revisores requeridos, branch policies). |

## 7.3. Pipeline reference: 15 stages del Motor DSL

El pipeline del Motor DSL está formalizado en `/docs/09_devops/pipeline-ci-cd_v1.0.md` §5. Tabla de stages (síntesis):

| # | Stage | Trigger | Gate | Artefacto |
|---|---|---|---|---|
| 1 | Checkout | Todos | Repo accesible | Working tree |
| 2 | Build | Todos | Compilación sin errores | DLLs |
| 3 | Tests | Todos | Cobertura ≥70%, sin fallas | Coverage report |
| 4 | Lint / Format | PR + push | `dotnet format --verify-no-changes` | — |
| 5 | Análisis estático | PR + push | Roslyn analyzers, `TreatWarningsAsErrors=true` | — |
| 6 | SCA | PR + push | Sin CVE High/Critical | Vuln report |
| 7 | SBOM | Tag `v*` | SBOM generado | `sbom.json` (CycloneDX) |
| 8 | Pack | Tag `v*` | `.nupkg` válido | `.nupkg` + `.snupkg` |
| 9 | Firma | Tag `v*` | `dotnet nuget sign` exitoso | `.nupkg` firmado |
| 10 | Publish | Tag `v*` | Push exitoso al feed | Versión publicada |
| 11 | Source Link y símbolos | Tag `v*` | `.snupkg` publicado | `.snupkg` |
| 12 | Reproducibilidad | Tag `v*` | Build determinista | `Deterministic=true` config |
| 13 | Caché | Todos | Hit rate aceptable | Cache key |
| 14 | CD Android | Push `main` + tag | APK generado | APK artifact |
| 15 | CD iOS | Push `main` (paths) | `.app.zip` generado | `.app.zip` artifact |

Pipeline declarado en 5 workflows GitHub Actions (`ci.yml`, `cd-nuget.yml`, `cd-android.yml`, `cd-ios-sampleapp.yml`, `cd-ios-multaapp.yml`).

## 7.4. Gates: DoR y DoD

**Definition of Ready** (DoR) se aplica antes de aceptar trabajo en el sprint: la PBI tiene criterios de aceptación, dependencias resueltas, estimación. Es gate humano, no automatizado. Detalle: `/docs/06_backlog-tecnico/definition-of-ready_v1.0.md`.

**Definition of Done** (DoD) se aplica en el merge: cobertura ≥70%, tests verdes, no warnings de analyzers, PR aprobado, CHANGELOG actualizado. La mayoría de los criterios DoD del Motor DSL están automatizados en el pipeline (`/docs/08_calidad_y_pruebas/definition-of-done_v1.0.md` §2). El pipeline es la **materialización ejecutable del DoD**.

| Criterio DoD | Automatización |
|---|---|
| Compila sin warnings | Roslyn analyzers + `TreatWarningsAsErrors` (stage 5). |
| Cobertura ≥70% | Stage 3 (test). |
| Tests verdes | Stage 3. |
| Convenciones de naming | Stage 4 (lint) + stage 5 (analyzers). |
| Sin CVE High/Critical en deps | Stage 6 (SCA). |
| Code review aprobado | GitHub branch protection (no es stage; es config del repo). |
| CHANGELOG actualizado | Validación manual por reviewer (no automatizado todavía). |

## 7.5. Workflows multiplataforma

**Matriz OS** en CI captura issues específicos de plataforma (path separator, line endings, file casing) antes de que un consumer los descubra.

| OS | Cuándo aplica | Cuándo no aplica |
|---|---|---|
| `ubuntu-latest` | Build y tests por defecto. Más rápido y barato. | — |
| `windows-latest` | Tests con PR (matriz). | Push a `main`, tag (innecesario duplicar). |
| `macos-15` | Builds iOS (mandatorio: Apple Toolchain solo en macOS). | Builds .NET puros (waste). |

**Runners hosted vs self-hosted:**

| Tipo | Ventaja | Desventaja | Cuándo elegir |
|---|---|---|---|
| Hosted (GitHub-managed) | Cero mantenimiento, escalable | Costo por minuto en repos privados/grandes | Default para OSS y proyectos pequeños-medianos. |
| Self-hosted | Sin costo por minuto, hardware específico (GPU, ARM real) | Mantenimiento, security hardening, supply chain risk | Repos con build cost alto, hardware especial, compliance. |

**Aplicación al Motor DSL:** hosted runners. Matriz `ubuntu-latest` + `windows-latest` solo en PR; `ubuntu-latest` en push y tag para minimizar tiempo y costo. iOS forzosamente en `macos-15` con Xcode 26.0.

## 7.6. Determinismo y caché

**Determinismo** garantiza que la misma rev de Git produce el mismo binario bit-a-bit. Configuración en .NET:

```xml
<Deterministic>true</Deterministic>
<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
```

Estas dos propiedades, combinadas con `global.json` que pinea la versión del SDK, eliminan paths absolutos del PDB, normalizan timestamps y estabilizan ordenamientos no determinísticos. Son prerequisito de **SLSA L3** (provenance no forjable) y mitigan ataques tipo SolarWinds (build pipeline tampering produciría binarios distintos al baseline determinista).

**Caché** reduce el tiempo de `dotnet restore` y otros pasos repetidos. Patrón canónico GitHub Actions:

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json', '**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

**Anti-pattern: caché sin lockfile.** Cachear `~/.nuget/packages` sin que el repo tenga `packages.lock.json` produce caché no determinista (la misma key puede contener versiones flotantes resueltas de forma distinta).

**Aplicación al Motor DSL:** `Deterministic=true`, `ContinuousIntegrationBuild=true`, `global.json` con `rollForward: latestFeature`, caché NuGet basada en hash de lockfile + csproj. Detalle: `/docs/09_devops/pipeline-ci-cd_v1.0.md` §5.12 y §5.13.

---

# 8. Supply chain security

## 8.1. Resumen ejecutivo: 6 controles no-negociables 2024-2026

Para cualquier librería OSS distribuida vía un package registry público en 2024-2026, los controles mínimos no-negociables son:

| # | Control | Estándar / Tooling | Ya cumplido por Motor DSL |
|---|---|---|---|
| 1 | Build determinista | `Deterministic=true`, `ContinuousIntegrationBuild=true`, `global.json` pinned | Sí |
| 2 | SBOM por release | CycloneDX 1.6 + `Microsoft.Sbom.Tool` | Sí (desde pipeline v1.3) |
| 3 | SCA con gate bloqueante | Dependabot + `dotnet list package --vulnerable` | Sí (gate High/Critical) |
| 4 | Firma de paquete | `dotnet nuget sign` con cert privado o Sigstore keyless | Sí (`dotnet nuget sign`) |
| 5 | Provenance attestation | SLSA L2 vía `actions/attest-build-provenance@v2` | Parcial (SLSA L2 alcanzable, no formalmente atestado todavía) |
| 6 | Vulnerability response policy | `SECURITY.md` + GitHub Security Advisories | Sí (advisories habilitados) |

Quien no cumple los 6 en 2026 está fuera del estado del arte y aceptablemente expuesto a SolarWinds, Log4Shell, xz-utils, typosquatting y CRA non-compliance.

## 8.2. SLSA v1.0 — Build levels

SLSA (Supply-chain Levels for Software Artifacts, originalmente "Salsa") es el framework estándar 2024-2026 para medir madurez de supply chain. URL normativa: <https://slsa.dev/spec/v1.0/>.

**Build levels (v1.0):**

| Nivel | Requisito | Cómo cumplirlo en GitHub Actions |
|---|---|---|
| **L1** | Build documentado y reproducible "razonablemente". | Cualquier CI con configuración versionada. |
| **L2** | Provenance firmada por hosted build platform. | GitHub Actions + `actions/attest-build-provenance@v2`. |
| **L3** | Build platform aislada, no forjable; provenance no falsificable. | SLSA GitHub Generator (reusable workflow firmado). |

**Provenance** es metadata estructurada que documenta cómo se produjo el artefacto: source repo, commit, builder identity, build steps, materials. Predicate type estándar: `https://slsa.dev/provenance/v1`.

**VSA** (Verification Summary Attestation) documenta que un artefacto fue verificado contra una política. Predicate: `https://slsa.dev/verification_summary/v1`. Útil para downstream que recibe el artefacto sin acceso al builder.

**In-toto** (<https://in-toto.io/>) es el framework de attestations sobre el que SLSA se construye. Las attestations SLSA son in-toto envelopes con predicates SLSA-específicos.

**Cómo cumplir L2 con GitHub Actions** (patrón canónico 2024-2026):

```yaml
permissions:
  id-token: write
  contents: read
  attestations: write

steps:
  - uses: actions/checkout@v4
  - uses: actions/setup-dotnet@v4
  - run: dotnet pack -c Release -o artifacts
  - uses: actions/attest-build-provenance@v2
    with:
      subject-path: 'artifacts/*.nupkg'
```

El step `attest-build-provenance` produce un envelope in-toto firmado vía Sigstore (Fulcio + Rekor) y lo asocia al artefacto. Verificación por consumers downstream con `gh attestation verify`.

**Aplicación al Motor DSL:** SLSA L2 alcanzable con la configuración actual (build determinista + GitHub-hosted runners). Adopción formal de attestations queda en roadmap como mejora incremental sin breaking change.

## 8.3. SBOM

Software Bill of Materials (SBOM) es el inventario formal de componentes y dependencias del paquete. Es prerequisito legal y operativo en 2024-2026 (US EO 14028, NIST SSDF PS.3, EU CRA).

**Estándares dominantes:**

| Estándar | Autoridad | Versión actual | Casos de uso |
|---|---|---|---|
| **CycloneDX** | OWASP / Ecma TC54 (2024) | 1.6 | DevSecOps pipelines, vulnerability scanning, machine-readable. |
| **SPDX** | Linux Foundation; ISO/IEC 5962:2021 | 2.3 / 3.0 | License compliance, government procurement, formato académico-legal. |

**Tooling .NET para generación de SBOM:**

| Herramienta | Formato | Fuente |
|---|---|---|
| **Microsoft.Sbom.Tool** | SPDX + CycloneDX | <https://github.com/microsoft/sbom-tool> |
| **CycloneDX/cyclonedx-dotnet** | CycloneDX | <https://github.com/CycloneDX/cyclonedx-dotnet> |
| **syft** (Anchore) | CycloneDX + SPDX | <https://github.com/anchore/syft> |

**Marco regulatorio que exige SBOM:**

- **US Executive Order 14028** (Biden, May 2021): "Improving the Nation's Cybersecurity". Sec 4(e)(vii) exige SBOM en software entregado al gobierno federal.
- **NTIA Minimum Elements for an SBOM** (2021): los 7 mínimos (supplier, component name, version, dependency, author, timestamp, unique identifier).
- **OMB M-22-18** y **M-23-16**: detalla cumplimiento federal de EO 14028.
- **EU CRA** (Reg. 2024/2847): exige SBOM como obligación legal para productos con componentes digitales (ver §8.9).

**Aplicación al Motor DSL:** SBOM CycloneDX 1.5+ generado por release con `Microsoft.Sbom.Tool`, adjuntado como asset al GitHub Release del tag y persistido como artifact con retención 90 días. Detalle: `/docs/09_devops/pipeline-ci-cd_v1.0.md` §5.7.

## 8.4. NIST SSDF SP 800-218

NIST Secure Software Development Framework (SSDF), publicado como Special Publication 800-218 (febrero 2022), es el framework base de prácticas de seguridad en el SDLC adoptado por agencias federales US y referenciado en EO 14028. URL: <https://csrc.nist.gov/Projects/ssdf>.

**Cuatro grupos de prácticas:**

| Grupo | Significado | Foco |
|---|---|---|
| **PO** | Prepare the Organization | Políticas, roles, capacitación. |
| **PS** | Protect the Software | Integridad del código y de los releases. |
| **PW** | Produce Well-Secured Software | Diseño y desarrollo seguro. |
| **RV** | Respond to Vulnerabilities | Identificación, análisis y remediación. |

**Prácticas críticas** (relevantes para una librería NuGet):

| Práctica | Descripción | Aplicación al Motor DSL |
|---|---|---|
| **PS.1** | Protect All Forms of Code from Unauthorized Access | Branch protection, secret scanning, restricted push. |
| **PS.2** | Verify Software Release Integrity | `dotnet nuget sign` (cert) o Sigstore attestation. |
| **PS.3** | Archive and Protect Each Software Release | SBOM + provenance + GitHub Releases inmutables. |
| **PW.4** | Reuse Existing, Well-Secured Software | Dependabot + lockfile + auditoría. |
| **PW.6** | Configure Compilation/Build for Security | `Deterministic=true`, `ContinuousIntegrationBuild=true`, `TreatWarningsAsErrors`. |
| **PW.7** | Review and/or Analyze Code | Roslyn analyzers, CodeQL (planeado). |
| **RV.1–3** | Vulnerability Response | Dependabot, `SECURITY.md`, GitHub Security Advisories. |

**Mapeo PS.3 ↔ Motor DSL:** la combinación SBOM + tag firmado + GitHub Release inmutable + `.nupkg` inmutable en GitHub Packages cumple los 4 requisitos formales de PS.3 (archivado, integridad, accesibilidad, no-modificación post-release).

## 8.5. OWASP SCVS v1

OWASP Software Component Verification Standard v1 (<https://owasp.org/www-project-software-component-verification-standard/>) es el estándar OWASP para verificación de componentes de software.

**Niveles de assurance:**

| Nivel | Significado |
|---|---|
| L1 | Mínimo. Aceptable para librerías de bajo riesgo. |
| L2 | Recomendado para librerías OSS con audiencia industrial. |
| L3 | Crítico. Sistemas con compliance regulatorio estricto. |

**6 familias de controles V1–V6:**

| Familia | Foco |
|---|---|
| **V1** Inventory | Inventario completo de componentes. |
| **V2** Software Bill of Materials | SBOM machine-readable por release. |
| **V3** Build Environment | Build environment seguro, reproducible. |
| **V4** Package Management | Gestión de paquetes y registries. |
| **V5** Component Analysis | SCA, vulnerability scanning. |
| **V6** Pedigree and Provenance | Trazabilidad de origen. |

**Aplicación al Motor DSL:** L2 alcanzable. V1 (inventory) y V2 (SBOM) cubiertos por el SBOM CycloneDX. V3 (build environment) por determinismo + GitHub Actions hosted. V4 (package management) por GitHub Packages + lockfile. V5 (component analysis) por Dependabot + `dotnet list package --vulnerable`. V6 (provenance) cubierto al activar attestations SLSA.

## 8.6. OpenSSF Scorecards

OpenSSF Scorecards (<https://securityscorecards.dev/>) es el sistema de scoring open source de la Open Source Security Foundation que evalúa repositorios OSS contra 18 checks. Action: `ossf/scorecard-action@v2`.

**18 checks:**

| Check | Qué evalúa |
|---|---|
| Branch-Protection | Reglas de protección de branches críticos. |
| CI-Tests | Tests automáticos en CI. |
| CII-Best-Practices | Badge CII Best Practices. |
| Code-Review | Revisión de código obligatoria pre-merge. |
| Contributors | Diversidad de contribuidores (mitigación takeover). |
| Dangerous-Workflow | Patterns peligrosos en GitHub Actions. |
| Dependency-Update-Tool | Dependabot/Renovate habilitado. |
| Fuzzing | Fuzzing automático (OSS-Fuzz, libFuzzer). |
| License | Archivo LICENSE presente y válido. |
| Maintained | Actividad de mantenimiento reciente. |
| Packaging | Publicación a registries oficiales. |
| Pinned-Dependencies | Dependencias pinned por SHA en CI. |
| SAST | SAST tooling configurado (CodeQL, Semgrep). |
| Security-Policy | `SECURITY.md` presente. |
| Signed-Releases | Releases firmados criptográficamente. |
| Token-Permissions | Permissions mínimos en workflows. |
| Vulnerabilities | Sin vulnerabilidades abiertas críticas. |
| Webhooks | Webhooks revisados (low-priority). |

Score agregado 0–10. Threshold industrial 2024-2026: ≥7 para librerías con audiencia industrial.

**Aplicación al Motor DSL:** la mayoría de los checks aprobables con la configuración actual (Branch-Protection, CI-Tests, Code-Review, License MIT, Pinned-Dependencies vía lockfile, Security-Policy, Signed-Releases vía `dotnet nuget sign`, Token-Permissions). Fuzzing y SAST formal pendientes.

## 8.7. Sigstore

Sigstore (<https://sigstore.dev/>) es el proyecto OSS (Linux Foundation, 2021, GA 2022) que provee firma criptográfica con keyless signing vía OIDC. Componentes:

| Componente | Rol |
|---|---|
| **Cosign** | CLI de firma/verificación de containers, blobs y attestations. |
| **Fulcio** | Certificate Authority que emite certs efímeros (~10 min) bound a una identidad OIDC. |
| **Rekor** | Transparency log público de attestations firmadas. Append-only, auditable. |
| **Gitsign** | Firma commits Git con OIDC en lugar de GPG. |

**Modelo keyless OIDC vs `dotnet nuget sign`:**

| Aspecto | `dotnet nuget sign` (cert tradicional) | Sigstore keyless (Cosign) |
|---|---|---|
| Identidad | Cert PFX privado del proyecto | OIDC token (GitHub Actions, Google, Microsoft) |
| Custodia de clave | Maintainer responsable | No hay clave persistente; cert efímero |
| Rotación | Manual | Automática (cada firma) |
| Verificación | `dotnet nuget verify` + cert raíz | `cosign verify` + Rekor lookup |
| Costo operativo | Gestión cert anual + secret rotation | Solo OIDC (gratis) |
| Adopción .NET | Estándar oficial | Emergente (vía GitHub Artifact Attestations) |

**GitHub Artifact Attestations** (2024) usa Sigstore internamente. Vía `actions/attest-build-provenance@v2`, los artefactos producidos en Actions reciben firma keyless gratuita verificable con `gh attestation verify`. Es la ruta de adopción Sigstore para repos GitHub sin migrar tooling.

**Aplicación al Motor DSL:** `dotnet nuget sign` con cert PFX (estándar NuGet). Sigstore complementario vía Artifact Attestations es upgrade futuro sin breaking change.

## 8.8. Threat modeling — incidentes paradigmáticos

Cuatro incidentes de supply chain de la última década definen el modelo de amenazas vigente 2024-2026.

| Incidente | Año | Vector | Mitigación |
|---|---|---|---|
| **SolarWinds Orion** | 2020 | Build pipeline tampering: atacante inyectó código en el build server, los binarios firmados oficialmente contenían backdoor. | SLSA L2/L3 (provenance no forjable) + build determinista. |
| **Log4Shell** (CVE-2021-44228) | 2021 | Vulnerabilidad crítica RCE en dependencia transitiva ubicua (Log4j 2.x). Tiempo medio de respuesta crítico. | SBOM (impact analysis acelerado) + Dependabot + advisories. |
| **xz-utils** (CVE-2024-3094) | Marzo 2024 | Maintainer takeover gradual: actor "Jia Tan" obtuvo trust como co-maintainer durante 2 años, inyectó binary backdoor en tarballs. Detectado por accidente (microbenchmark de SSH). | Scorecards (Binary-Artifacts, Dangerous-Workflow, Contributors) + reproducible builds + auditoría humana de cambios sospechosos. |
| **PyPI typosquatting** | Recurrente | Paquetes con nombre similar a populares (`requets`, `python-pillow` vs `Pillow`). | Lockfiles + allowlists + verificación manual del primer pin. |

**Matriz vector ↔ control:**

| Vector | SBOM | SLSA L2 | Sigstore | Scorecards | Lockfile | Dependabot |
|---|---|---|---|---|---|---|
| Build pipeline tampering | Parcial | **Mitiga** | **Mitiga** | Mitiga | — | — |
| Dependencia con CVE crítica | **Mitiga (response time)** | — | — | — | Mitiga (pin) | **Mitiga (alerta)** |
| Maintainer takeover | — | — | Mitiga (firma identity) | **Mitiga (Contributors, Binary-Artifacts)** | Mitiga (pin a SHA) | — |
| Typosquatting | Mitiga (audit) | — | Mitiga (verify identity) | — | **Mitiga (pin a versión válida)** | — |

Citas: <https://www.cisa.gov/news-events/cybersecurity-advisories/aa20-352a>, <https://nvd.nist.gov/vuln/detail/CVE-2021-44228>, <https://nvd.nist.gov/vuln/detail/CVE-2024-3094>.

## 8.9. EU CRA (Reglamento 2024/2847)

EU Cyber Resilience Act, formalmente Reglamento (UE) 2024/2847, es la regulación europea de ciberseguridad para productos con elementos digitales.

| Hito | Fecha |
|---|---|
| Publicación DOUE | 20 noviembre 2024 |
| Entrada en vigor | 11 diciembre 2024 |
| Reporte de incidentes (obligación) | 11 septiembre 2026 |
| Aplicación general | 11 diciembre 2027 |

**Categorías de productos:**

| Categoría | Severidad | Régimen |
|---|---|---|
| Default | Bajo riesgo | Self-assessment del fabricante. |
| Important Class I | Riesgo medio | Self-assessment + estándares armonizados. |
| Important Class II | Riesgo medio-alto | Third-party assessment. |
| Critical | Alto | Certificación obligatoria. |

**Obligaciones (resumen):**

- SBOM por producto.
- Vulnerability handling con disclosure coordinado.
- Security updates ≥5 años (o vida del producto).
- CE marking.
- Reporte de incidentes a ENISA en plazos definidos.

**Open Source Steward** (figura introducida explícitamente en CRA): foundations o entidades que coordinan el desarrollo de OSS pero no lo comercializan directamente. Tienen obligaciones reducidas respecto del fabricante comercial. Recital 18 establece exención específica para OSS individual no comercial.

**Safe harbors:** OSS distribuido sin contrapartida monetaria y fuera de actividad comercial cae fuera del scope. La interpretación de "actividad comercial" sigue siendo ambigua y se aclarará en guidance de la Comisión durante 2025-2027.

Cita: <https://eur-lex.europa.eu/eli/reg/2024/2847/oj>.

**Aplicación al Motor DSL:** la librería se distribuye en modalidad OSS no comercial (licencia MIT, sin contraparte económica). En la interpretación más conservadora, cae bajo Open Source Steward o exención Recital 18. Independientemente del scope formal, el cumplimiento operativo (SBOM, vulnerability handling, security advisories) está alineado con CRA por el ya descrito stack de supply chain.

## 8.10. Mapeo aplicado al Motor DSL

Tabla síntesis de controles y su materialización:

| Control | Estándar | Implementación Motor DSL | Documento autoritativo |
|---|---|---|---|
| Build determinista | SLSA L1 prereq, NIST SSDF PW.6 | `Deterministic=true`, `ContinuousIntegrationBuild=true`, `global.json` | `pipeline-ci-cd_v1.0.md` §5.12 |
| SBOM por release | NIST SSDF PS.3, OWASP SCVS V2 | `Microsoft.Sbom.Tool` CycloneDX, asset al Release | `pipeline-ci-cd_v1.0.md` §5.7 |
| SCA con gate bloqueante | OWASP SCVS V5, NIST SSDF PW.4 | Dependabot + `dotnet list package --vulnerable`, gate High/Critical | `pipeline-ci-cd_v1.0.md` §5.6 |
| Análisis estático | NIST SSDF PW.7 | Roslyn analyzers + `TreatWarningsAsErrors` | `pipeline-ci-cd_v1.0.md` §5.5 |
| Firma de paquete | NIST SSDF PS.2, SLSA L2 | `dotnet nuget sign` con cert PFX | `pipeline-ci-cd_v1.0.md` §5.9 |
| Provenance attestation | SLSA L2 | `actions/attest-build-provenance@v2` (planeado) | Roadmap |
| Yanking policy | NIST SSDF RV.3 | CVSS ≥9.0 + dual approval Tech Lead + Security | `estrategia-versionado_v1.0.md` §13 |
| Vulnerability response | NIST SSDF RV.1–3 | Dependabot, `SECURITY.md`, GHSA | `/docs/09_devops/README.md` |
| Lockfile NuGet | OWASP SCVS V4 | `RestorePackagesWithLockFile=true` + `packages.lock.json` | `pipeline-ci-cd_v1.0.md` §5.13 |

**Workflow YAML reference** (patrón canónico para release con attestation, simplificado):

```yaml
name: CD NuGet
on:
  push:
    tags: ['v*']

permissions:
  contents: read
  packages: write
  id-token: write
  attestations: write

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # MinVer requires full history
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build -c Release --no-restore
      - run: dotnet test --no-build -c Release
      - run: dotnet pack -c Release --no-build -o artifacts
      # SBOM + sign + attestation
      - name: Generate SBOM
        run: |
          dotnet tool install --global Microsoft.Sbom.DotNetTool
          # ...generar sbom.json
      - name: Sign packages
        env:
          NUGET_CERT_PFX_BASE64: ${{ secrets.NUGET_CERT_PFX_BASE64 }}
          NUGET_CERT_PASSWORD: ${{ secrets.NUGET_CERT_PASSWORD }}
        run: |
          echo "$NUGET_CERT_PFX_BASE64" | base64 -d > $RUNNER_TEMP/cert.pfx
          dotnet nuget sign artifacts/*.nupkg \
            --certificate-path $RUNNER_TEMP/cert.pfx \
            --certificate-password "$NUGET_CERT_PASSWORD" \
            --timestamper http://timestamp.digicert.com
      - uses: actions/attest-build-provenance@v2
        with:
          subject-path: 'artifacts/*.nupkg'
      - name: Push to GitHub Packages
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          dotnet nuget push artifacts/*.nupkg \
            --api-key $GITHUB_TOKEN \
            --source https://nuget.pkg.github.com/<owner>/index.json
```

---

# 9. Release engineering: del commit al consumidor

## 9.1. Modelo de feeds: Preview / Stable

El modelo de distribución del Motor DSL distingue dos canales sobre **un único feed físico** (GitHub Packages):

| Canal | Identificación | Audiencia | SLA |
|---|---|---|---|
| **Preview** | Versiones con sufijo `-alpha.N`, `-beta.N`, `-rc.N` | Integradores tempranos, sample app E2E, dogfooding interno | Ninguno; el path correctivo es subir a la siguiente preview o al estable. |
| **Stable** | Versiones sin sufijo (`1.2.3`) | Consumers productivos, default cuando el consumer no habilita prerelease | Última MAJOR: features + fixes + security; penúltima MAJOR: solo security 6 meses. |

Detalle: `/docs/09_devops/entornos-deploy_v1.0.md` §4.

**Decisión clave:** un único feed físico. La distinción Preview vs Stable es por sufijo SemVer y por la flag `prerelease=true` que NuGet propaga. No se duplica el feed por canal porque GitHub Packages distingue pre-releases nativamente y duplicar agregaría overhead operativo sin beneficio.

## 9.2. Procedimiento de release (5 pasos)

| Paso | Acción | Responsable |
|---|---|---|
| 1 | Cierre de `[Unreleased]` → `[X.Y.Z] - YYYY-MM-DD` en CHANGELOG | Release manager |
| 2 | Verificar CI verde post-merge en `main` | Release manager |
| 3 | Crear tag firmado `git tag -s vX.Y.Z` y push | Release manager |
| 4 | Pipeline auto-dispara: pack + sign + SBOM + push al feed | DevOps (automatizado) |
| 5 | `gh release create vX.Y.Z --notes-from-tag --verify-tag` | Release manager |

Detalle paso a paso: `/docs/09_devops/estrategia-versionado_v1.0.md` §15.

## 9.3. Política de breaking changes — paquete monolítico vs granular

**Estrategias de empaquetado:**

| Estrategia | Ejemplo industrial | Trade-off |
|---|---|---|
| **Monolítica** | `Newtonsoft.Json` (un solo paquete) | Simple operativamente; consumer instala todo. |
| **Granular** | `Microsoft.Extensions.DependencyInjection` (.Abstractions + impl) | Consumer instala solo lo que necesita; versionado independiente posible. |

**Aplicación al Motor DSL:** estrategia granular con 4 paquetes (`MotorDsl.Core`, `.Parser`, `.Rendering`, `.Extensions`). El consumer instala únicamente `MotorDsl.Extensions`, que trae los otros 3 transitivamente. Detalle: `/docs/09_devops/guia-publicacion-nuget_v1.0.md` §3.

**Política aplicada:** los 4 paquetes se publican **simultáneamente con la misma versión**. No existe un release donde solo 2 de los 4 estén publicados. Esto sacrifica versionado independiente por simplicidad operativa: cuando uno bumpea, los 4 bumpean. Es el costo aceptado por mantener consistencia transitiva.

## 9.4. Política de deprecación (3 fases)

Detalle normativo: `/docs/09_devops/estrategia-versionado_v1.0.md` §10.2 y §11.

| Fase | Acción | Período |
|---|---|---|
| 1 — Aviso | Marcar API con `[Obsolete("...", false)]`. Sigue compilando, emite warning. CHANGELOG sección `Deprecated`. | Mínimo **2 versiones MINOR** posteriores al anuncio **o 6 meses calendario**, lo que ocurra **después**. |
| 2 — Escalada a error | Marcar con `[Obsolete("...", true)]`. Compila falla. Solo en la última MINOR antes de la MAJOR que remueve. | Una sola MINOR. |
| 3 — Removido | API eliminada. Bump MAJOR. Entrada en CHANGELOG sección `Removed`. | Permanente. |

**Anti-pattern: skip de fase 2.** Saltar de aviso a removido sin pasar por escalada `error: true` quita al consumer la oportunidad de migración progresiva. Prohibido en el Motor DSL.

## 9.5. Yanking y soporte multi-version

**Yanking** detallado en §4.6 de este documento y `/docs/09_devops/estrategia-versionado_v1.0.md` §13.

**Soporte multi-version** (ventana N+1):

| Banda | Política Motor DSL |
|---|---|
| Última MAJOR (`vN.x.x`) | Features (MINOR) + fixes (PATCH) + security. Soporte completo. |
| Penúltima MAJOR (`v(N-1).x.x`) | Solo security fixes durante **6 meses** desde el release de `vN.0.0`. Sin backport de features. |
| Anteriores (`v(N-2)` y previas) | Sin soporte. Consumer debe migrar. |

**Soporte intra-MAJOR:** dentro de `vN`, solo la última MINOR liberada recibe PATCH proactivos. Las MINOR anteriores reciben PATCH **únicamente** ante CVE crítico (CVSS ≥7.0) **y** mitigación trivial no disponible vía upgrade.

**Aplicación al Motor DSL:** ventana N+1 con security-only en N-1 durante 6 meses. La política minimiza fragmentación del soporte sin abandonar consumers que aún no migraron.

---

# 10. GitOps y por qué no aplica al Motor DSL

## 10.1. GitOps: definición + 4 principios OpenGitOps

GitOps (acuñado por Weaveworks, 2017) es el paradigma operativo en el que Git es la fuente única de verdad para infraestructura y configuración, y un controlador automatizado reconcilia el estado real contra el declarado en el repo. La fundación OpenGitOps (CNCF Sandbox, 2022) formalizó los **4 principios**:

| # | Principio | Significado operativo |
|---|---|---|
| 1 | **Declarative** | El sistema gestionado se describe declarativamente. |
| 2 | **Versioned and Immutable** | El estado deseado vive en un sistema versionado e inmutable (Git). |
| 3 | **Pulled Automatically** | Los agentes pull automáticamente el estado del repo. |
| 4 | **Continuously Reconciled** | Los agentes reconcilian continuamente el estado real con el deseado. |

Cita: <https://opengitops.dev/>.

## 10.2. Stack canónico

| Componente | Rol | Estado CNCF |
|---|---|---|
| **ArgoCD** | Continuous deployment declarativo Kubernetes-native | Graduated 2022 |
| **Flux CD** | Reconciliador GitOps multi-tenant | Graduated 2022 |
| **Helm** | Package manager Kubernetes | Graduated 2020 |
| **Kustomize** | Template-less customization YAML | Built-in kubectl |
| **Cosign** | Firma de OCI images / charts | Sigstore Graduated |

## 10.3. Por qué no aplica a librerías

**No aplica al caso Motor DSL.** Razones operativas:

1. **GitOps gestiona estado de runtime.** Un cluster Kubernetes, un set de servicios, una flota de nodos. La librería no tiene runtime propio; no hay "estado deseado del Motor DSL en producción" que reconciliar.
2. **El consumer aporta el runtime.** El estado deseado relevante es del consumer (qué versión consumir, en qué entorno desplegar). El consumer puede usar GitOps si su contexto lo amerita (ver §11.5), pero esa decisión está fuera del alcance del repo de la librería.
3. **El feed ya es inmutable y versionado.** GitHub Packages provee inmutabilidad y versionado por diseño. Un controlador GitOps que "reconcilie" el estado del feed sería redundante: ya es declarativo (`PackageReference` en el `.csproj` del consumer) y ya es continuously enforced (NuGet rechaza re-publicación).

**Conclusión normativa:** intentar forzar GitOps sobre release engineering de librerías es **anti-pattern conceptual** análogo al anti-pattern de "deploy de la librería como si fuera servicio" documentado en `/docs/09_devops/entornos-deploy_v1.0.md` §3.1.

## 10.4. Cuándo sí aplica

| Contexto | Cuándo GitOps aplica |
|---|---|
| Microservicios sobre Kubernetes | Sí. Es el caso canónico (ver §11.5). |
| Plataformas internas (IDP) | Sí. Backstage + ArgoCD es stack común. |
| Despliegue de configuración SaaS (feature flags, secrets) | Sí parcial. Tools tipo Atlas, Terraform Cloud. |
| Web SaaS con deploy continuo | Sí, si el deploy target es Kubernetes. |
| Librerías | **No.** |
| Mobile apps a stores | **No** (los stores no soportan reconciliación continua). |
| Aplicaciones de escritorio / paquetes de instalación | **No.** |

---

# 11. Seis contextos de industria: comparativa cross-domain

## 11.1. Marco de comparación

Diez ejes para comparar contextos:

| # | Eje | Pregunta |
|---|---|---|
| 1 | Versionado | ¿Qué esquema (SemVer, CalVer, triple, otros)? |
| 2 | Branching | ¿Qué estrategia (GitHub Flow, GitFlow, TBD, Release Flow)? |
| 3 | Cadencia | ¿Cuántos deploys/releases por unidad de tiempo? |
| 4 | Distribución | ¿Cómo llega al consumer (deploy interno, feed, store, registry)? |
| 5 | DORA dominante | ¿Qué métrica DORA es la más sensible? |
| 6 | Supply chain | ¿Qué nivel de madurez (SLSA L1/L2/L3, SCA, SBOM)? |
| 7 | Feature flags | ¿Mandatorios, recomendados, irrelevantes? |
| 8 | Rollback | ¿Cómo se revierte un cambio en producción? |
| 9 | Observability | ¿Qué se mide (SLI runtime, descargas de feed, crash rate)? |
| 10 | Compliance regulatorio | ¿Hay regulación específica del dominio? |

## 11.2. C1 — Web SaaS gran escala

**Casos**: Netflix (Spinnaker, Chaos Engineering), Spotify (Backstage, squad model), Stripe (API versioning por fecha), Shopify (~15 min e2e + canary 5%/10 min), GitHub (deploy continuo del propio GitHub).

**Características:**

| Eje | Valor |
|---|---|
| Versionado | CalVer interno (deploys), SemVer en SDKs públicos (Stripe SDK), API versioning por fecha (Stripe API). |
| Branching | TBD + feature flags (Netflix, Shopify). |
| Cadencia | On-demand, miles de deploys/día. |
| Distribución | Despliegue interno (K8s, Spinnaker). |
| DORA dominante | Deployment Frequency. |
| Supply chain | Alta (SLSA L2/L3 alcanzado, SBOM, sign). |
| Feature flags | Mandatorios. |
| Rollback | Flag flip < 1 min; canary detect → revert auto. |
| Observability | SLI runtime + business metrics + telemetry product-grade. |
| Compliance | SOC2, PCI (Stripe, Shopify), GDPR, CRA. |

Tooling distintivo: **Spinnaker** (Netflix, deploy multicloud), **Backstage** (Spotify, internal developer portal, CNCF Incubating).

## 11.3. C2 — OSS .NET pública

**Casos:** `Microsoft.Extensions.*`, `Newtonsoft.Json`, `Serilog`, `FluentValidation`, `MediatR`, `AutoMapper`, `Polly`. **El Motor DSL pertenece a este contexto.**

**Características:**

| Eje | Valor |
|---|---|
| Versionado | SemVer 2.0.0 estricto. |
| Branching | GitHub Flow o Release Flow (Microsoft.Extensions). |
| Cadencia | Semanal a mensual; algunos mensual a trimestral. |
| Distribución | nuget.org (mainstream) o GitHub Packages (org-restricted). |
| DORA dominante | Lead Time + Change Failure Rate. |
| Supply chain | Media-alta. SLSA L2 alcanzable; SBOM emergente; sign con `dotnet nuget sign` o Sigstore. |
| Feature flags | No relevantes (no hay runtime propio). |
| Rollback | Publicar nueva PATCH; yankear solo en CVE crítico. |
| Observability | Métricas de feed (downloads, adopción por versión). |
| Compliance | EU CRA en interpretación OSS Steward; licencia MIT/Apache. |

## 11.4. C3 — Mobile dual-store

**Casos:** Slack, Uber, WhatsApp, Notion, Airbnb. Distribución obligatoria a Apple App Store y Google Play.

**Características:**

| Eje | Valor |
|---|---|
| Versionado | Triple (versión user-facing + build numbers iOS/Android + versión backend). |
| Branching | Release Flow / GitFlow (release branches por versión cuasi-mensual). |
| Cadencia | 1-2 semanas (limitada por app review de Apple). |
| Distribución | App Store (iOS), Google Play (Android). |
| DORA dominante | Change Failure Rate (rollback caro: nueva versión + review). |
| Supply chain | Forzada por stores (firma, attestations Play Integrity). |
| Feature flags | Mandatorios (server-side flags + remote config para revertir sin store). |
| Rollback | Staged rollout (1% → 10% → 100%); halt rollout si crash spike. |
| Observability | Crash-free users ≥99.85%, ANR <0.47% (Android vitals targets industriales). |
| Compliance | CCPA, GDPR, store policies. |

Tooling distintivo: **Fastlane** (estándar de facto para automation iOS/Android).

## 11.5. C4 — Microservicios + GitOps

**Casos:** plataformas modernas CNCF-native; productos empresariales sobre K8s; SaaS B2B con compliance estricto.

**Características:**

| Eje | Valor |
|---|---|
| Versionado | SemVer + tag inmutable de OCI image (sha256 digest). |
| Branching | TBD + config repo separado (GitOps pattern). |
| Cadencia | Continua (reconciliación automática). |
| Distribución | OCI registry (Docker Hub, GHCR, Harbor, ECR). |
| DORA dominante | MTTR + reconciliation lag. |
| Supply chain | **Máxima.** SLSA L3 declarado; Cosign keyless OIDC; SBOM mandatorio. |
| Feature flags | Recomendados. |
| Rollback | Reconciliación a tag previo (git revert en config repo). |
| Observability | OpenTelemetry; Prometheus + Grafana; SLOs cuantificados. |
| Compliance | Variable; SOC2, ISO 27001, FedRAMP, PCI según vertical. |

Tooling distintivo: **ArgoCD** y **Flux** (ambos CNCF Graduated 2022), **Cosign keyless OIDC**, **OpenTelemetry**.

## 11.6. C5 — Data Engineering

**Casos:** dbt Cloud, Apache Airflow, Apache Iceberg, Databricks, Snowflake.

**Características:**

| Eje | Valor |
|---|---|
| Versionado | SemVer + git SHA del modelo. |
| Branching | GitHub Flow (dbt Cloud) o feature branches por modelo. |
| Cadencia | Diaria a horaria (jobs incrementales). |
| Distribución | dbt Cloud, Airflow scheduler, Iceberg catalog. |
| DORA dominante | Freshness SLO (datos al día) + correctness. |
| Supply chain | Emergente. SBOM no estándar todavía; lineage como sustituto. |
| Feature flags | Relevantes en dual-write durante migration. |
| Rollback | Re-run con código previo o revert de schema. |
| Observability | Slim CI con schema PR-único; data quality tests; lineage viewer. |
| Compliance | Variable; HIPAA en healthcare; PCI en finance; GDPR. |

Tooling distintivo: **slim CI con schema PR-único** (dbt), **Iceberg V3** (2025) con row lineage permanent IDs.

## 11.7. C6 — ML Model Versioning

**Casos:** MLflow, DVC, Hugging Face Hub, Weights & Biases, ClearML.

**Características:**

| Eje | Valor |
|---|---|
| Versionado | **Triple** (model + data + code) interrelacionado. |
| Branching | Notebooks + registry stages (Staging → Production en MLflow). |
| Cadencia | Asíncrona: train pipeline (semanal/mensual) vs serve pipeline (diaria/horaria). |
| Distribución | Model Registry (MLflow, HF Hub). |
| DORA dominante | Drift detection + p99 inference latency. |
| Supply chain | **Gap crítico.** Modelos no se firman estándarmente; pickle deserialization es RCE; pre-trained checkpoints poco auditados. |
| Feature flags | A/B testing por modelo (champion/challenger). |
| Rollback | Promote previo champion. |
| Observability | Drift, fairness, latency; ground truth lag. |
| Compliance | EU AI Act, GDPR (datos de entrenamiento). |

**Riesgos supply chain identificados (no exhaustivo):**

- **PoisonGPT** (2023): demostración de inyección de backdoors en LLMs por modificación pesos sin firma.
- **GGUF poisoned templates**: chat templates maliciosos en formatos GGUF cargados por defecto.
- **Pickle deserialization**: formato `.pkl` ejecuta código arbitrario al cargar; preferir **safetensors**.

**MLflow 3.0** (2025) introduce **GenAI registry** que comienza a abordar firma y trazabilidad; aún emergente.

## 11.8. Tabla maestra comparativa

| Eje | C1 SaaS | C2 OSS .NET | C3 Mobile | C4 Micros+GitOps | C5 Data | C6 ML | **Motor DSL** |
|---|---|---|---|---|---|---|---|
| Versionado | CalVer interno | SemVer estricto | Triple | SemVer + tag inmutable | SemVer + git SHA | Triple (model+data+code) | **SemVer 2.0.0** |
| Branching | TBD + flags | GitHub Flow / Release Flow | Release Flow / GitFlow | TBD + config repo | GitHub Flow | Notebooks + registry stages | **GitHub Flow estricto** |
| Cadencia | On-demand miles/día | Semanal a mensual | 1-2 semanas | Continua | Diaria a horaria | Async train vs serve | **Semanal a mensual** |
| Distribución | Despliegue interno | nuget.org | Play / App Store | OCI registry | dbt Cloud / catalog | Model Registry | **GitHub Packages** |
| DORA dominante | Deployment Freq | Lead Time | Change Failure Rate | MTTR + reconciliation lag | Freshness SLO | Drift + p99 | **Lead Time + Change Failure Rate** |
| Supply chain | Alta (SLSA L3) | Media-alta | Forzada por stores | **Máxima** (Cosign+SLSA L3) | Emergente | **Gap crítico** | **L2 alcanzable** |
| Feature flags | **Mandatorios** | N/A | **Mandatorios** | Recomendados | A/B en migración | Champion/challenger | N/A |
| Rollback | Flag flip <1 min | Nueva PATCH | Staged rollout halt | Revert config repo | Re-run código previo | Promote previo champion | **Nueva PATCH** |
| Observability | SLI runtime + business | Feed metrics | Crash-free + ANR | OpenTelemetry + SLOs | Freshness + lineage | Drift + fairness | **Feed metrics + DORA** |
| Compliance | SOC2/PCI/GDPR | EU CRA (OSS Steward) | Store policies + GDPR | Variable + ISO 27001 | HIPAA/PCI según vertical | EU AI Act + GDPR | **EU CRA en OSS Steward** |

## 11.9. Síntesis: invariantes y variables

**Invariantes (cumplen todos los contextos en 2024-2026):**

- **Inmutabilidad** del artefacto/release/deploy una vez producido.
- **Versionado** con identificador único trazable a un commit.
- **Tests automáticos** como gate pre-promoción.
- **SBOM** (en distinto grado de madurez según contexto, pero presente en todos los contextos serios).
- **CI/CD** con triggers automatizados.

**Variables principales:**

| Variable | Determinada por |
|---|---|
| Esquema de versionado (SemVer / CalVer / triple) | Si hay API pública contractual con consumers pinneando vs si hay cadencia temporal forzada vs si hay artefactos compuestos (modelo + datos). |
| Estrategia de branching | Tamaño del equipo, cadencia objetivo, necesidad de soporte multi-version. |
| Distribución | Naturaleza del artefacto (servicio runtime vs paquete inerte vs imagen OCI vs binario en store). |
| Necesidad de feature flags | Si el deploy y la habilitación de la feature pueden desacoplarse (sí en SaaS y mobile; no en librería pinneada por consumer). |
| Severidad de Change Failure Rate | Costo del rollback en el dominio (caro en mobile, barato en SaaS, intermedio en librería). |

El Motor DSL como C2 tiene rollback intermedio (publicar PATCH, no yankear) y CFR objetivo Elite por la sensibilidad reputacional típica de OSS .NET con audiencia industrial.

---

# 12. Síntesis: las decisiones D1–D7 como caso de aplicación

Cada decisión arquitectónica del Motor DSL se fundamenta en las secciones precedentes. La presentación es **decisión → teoría → alternativa rechazada → implicancia operativa**.

## 12.1. D1 — GitHub Packages como único feed

**Teoría detrás:** §2.6 (Platform Engineering como consumidor), §9.1 (modelo feeds + consumers), §10 (anti-pattern de "deploy de librería").

**Alternativas rechazadas:**

| Alternativa | Razón de rechazo |
|---|---|
| NuGet.org único | Conflicto de naming pública con paquetes existentes; superficie operativa adicional (cuenta separada, API key rotación). |
| Multi-feed (NuGet.org + GitHub Packages) | Doble publicación + sincronización de versiones; falla parcial deja consumers en estado inconsistente. |
| Feed propio (BaGet, ProGet) | Mantenimiento + costo + redundancia con GitHub Packages que ya provee la primitiva. |

**Implicancia operativa:** un único `dotnet nuget push --source https://nuget.pkg.github.com/<owner>/index.json` por release. Autenticación vía `GITHUB_TOKEN` (pipeline) o PAT con `read:packages` (consumer). Detalle: `/docs/09_devops/guia-publicacion-nuget_v1.0.md` §6.5.

## 12.2. D2 — Licencia MIT

**Teoría detrás:** §8.9 (CRA, exenciones OSS), §11.3 (C2 contexto OSS .NET con licencias permisivas dominantes en NuGet).

**Alternativas rechazadas:**

| Alternativa | Razón de rechazo |
|---|---|
| GPL v2 / v3 | Restrictiva para reutilización industrial; excluye consumo desde productos comerciales propietarios. (Inicialmente el `LICENSE` raíz contenía GPL v2 — corregido por el plan de mejoras, ver `plan-mejoras-devops_v1.0.md` §A1.) |
| Apache 2.0 | Equivalente funcional pero adiciona patent grant explícita; preferido en proyectos enterprise. Para el caso académico-industrial MIT es más simple. |
| Propietaria | Incompatible con redistribución vía package registry público y con audiencia académica. |

**Implicancia operativa:** declarar `<PackageLicenseExpression>MIT</PackageLicenseExpression>` en `Directory.Build.props`; archivo `LICENSE` raíz alineado; ningún componente con licencia incompatible (validable vía SBOM + license scanning).

## 12.3. D3 — GitHub Flow estricto

**Teoría detrás:** §6.1 (GitHub Flow), §6.2 (disclaimer Driessen 2020 sobre GitFlow para CD), §6.6 (decision tree).

**Alternativas rechazadas:**

| Alternativa | Razón de rechazo |
|---|---|
| GitFlow (`develop` + `release/*` + `hotfix/*`) | Sobreingeniería para equipo pequeño con CI/CD continuo; el propio Driessen lo desaconseja en este contexto. |
| TBD puro sin feature branches | Inadecuado cuando el equipo no opera con feature flags (la librería no tiene runtime para flag). |
| `main` + `homologacion` (modelo descripto en `/devs/devops/guia-flujo-trabajo-versionado.md`) | Contradice el modelo de feeds + consumers downstream: la "homologación" en una librería se realiza en el canal Preview del feed, no en una rama del repo. Documento `/devs/` con deuda documental conocida (§6.7). |

**Implicancia operativa:** una rama `main` siempre liberable; feature branches efímeras; squash merge obligatorio; PR + reviewer + CI verde como precondición. Detalle: `/docs/09_devops/estrategia-versionado_v1.0.md` §7.

## 12.4. D4 — TFM único `net10.0`

**Teoría detrás:** §4.1 (SemVer Regla 8: cualquier cambio incompatible es MAJOR; cambio de TFM rompe consumers), §11.3 (C2 OSS .NET donde multi-target es común pero costoso).

**Alternativas rechazadas:**

| Alternativa | Razón de rechazo |
|---|---|
| Multi-target `net10.0;net8.0;netstandard2.1` | Polyfills extensos, matriz de tests N×TFM (185 → ≥370), análisis condicional `#if`, releases acoplados a regresiones por TFM. Costo desproporcionado para audiencia académica-industrial moderna. |
| `netstandard2.1` único | Pierde features de .NET 10 que la librería usa activamente (static abstract members, generic math, `UnsafeAccessor`). |
| `net8.0` LTS único | Bloquea adopción de features nuevos; ciclo LTS desacoplado del propio. |

**Implicancia operativa:** consumers atados a .NET 10 (audiencia inicial reducida; trade-off aceptado). Cualquier cambio futuro de TFM (`net10.0` → `net11.0`) es **siempre breaking** y dispara MAJOR sin discusión caso por caso. Detalle: `/docs/09_devops/estrategia-versionado_v1.0.md` §14.

## 12.5. D5 — Conventional Commits + MinVer

**Teoría detrás:** §5 (Conventional Commits y trazabilidad), §4.4 (auto-versioning tooling, comparativa MinVer / NBGV / etc.).

**Alternativas rechazadas:**

| Alternativa | Razón de rechazo |
|---|---|
| Versionado manual en `.csproj` | Bumps olvidables; no determinista; commits de bump contaminan historia. |
| Nerdbank.GitVersioning | Mayor complejidad (`version.json` por rama), lock-in con `nbgv` CLI. Solo justificable si el equipo necesita pipelines multi-versión simultáneos, que no es el caso. |
| GitVersion | Diseñado para GitFlow; configuración pesada; legacy. |
| semantic-release (npm-centric) | Stack desalineado con .NET. |

**Implicancia operativa:** cada squash merge respeta `<tipo>(<scope>)!: <descripción>`; el reviewer valida el título del PR antes de aprobar; MinVer calcula la versión desde el tag durante `dotnet pack`; commits de bump no existen. Detalle: `/docs/09_devops/estrategia-versionado_v1.0.md` §8.6.

## 12.6. D7 — Tono industrial

**Teoría detrás:** convención editorial transversal; soporta legibilidad operativa para release manager, mantenedores y consumers; descarta ambigüedad pedagógica que erosiona la utilidad como contrato.

**Alternativas rechazadas:**

| Alternativa | Razón de rechazo |
|---|---|
| Documentación con TODOs didácticos ("ejercicio para el alumno") | Confunde al practicante de industria que busca operativa. |
| Documentación narrativa larga | Costo de lectura excesivo para release engineering; los documentos operan como referencia. |
| Documentación sin referencias cruzadas explícitas | Desincronización inevitable entre artefactos. |

**Implicancia operativa:** sintaxis declarativa, voz activa, tablas para datos cuantitativos, citas inline con URL angle-bracketed, bibliografía agrupada temáticamente. Material didáctico (este `marco-teorico.md`, `plan-mejoras-devops_v1.0.md`) reside en `/devs/`, no en `/docs/`; los documentos operativos de `/docs/` permanecen densos y accionables.

## 12.7. RACI consolidado del caso

Síntesis del RACI; detalle exhaustivo en `/docs/09_devops/README.md` §RACI y `/docs/09_devops/estrategia-versionado_v1.0.md` §16.

| Actividad | R | A | C | I |
|---|---|---|---|---|
| Decidir tipo de bump (Conventional Commits del PR) | Autor del PR | Reviewer | — | Release manager |
| Aprobar PR con breaking change | Reviewer | Tech Lead | Autores impactados | Equipo, integradores |
| Cierre de `[Unreleased]` → `[X.Y.Z]` | Release manager | Tech Lead | Autores de PRs incluidos | Equipo |
| Crear tag firmado | Release manager | Tech Lead | — | DevOps, equipo |
| Disparar pipeline de publicación | Release manager (push tag) | — | — | DevOps |
| Yanking por CVE | Release manager | Tech Lead **+** Security | DevOps | Consumers afectados |
| Decidir extender ventana de soporte | Tech Lead | Producto | DevOps | Equipo |

## 12.8. Métricas y thresholds del caso

| Métrica / threshold | Valor | Fuente normativa |
|---|---|---|
| Cobertura de tests mínima | ≥70% | `/docs/08_calidad_y_pruebas/definition-of-done_v1.0.md` §2.2 |
| Threshold yanking | CVSS ≥9.0 (o alto con vector remoto sin auth) | `/docs/09_devops/estrategia-versionado_v1.0.md` §13 |
| Período de aviso de deprecación | ≥2 MINOR **y** ≥6 meses | `/docs/09_devops/estrategia-versionado_v1.0.md` §10.2 |
| Ventana de soporte penúltima MAJOR | 6 meses security-only | `/docs/09_devops/estrategia-versionado_v1.0.md` §12 |
| RC mínimo en Preview pre-promoción | 7 días | `/docs/09_devops/entornos-deploy_v1.0.md` §4.3 |
| Threshold SCA bloqueante | High / Critical | `/docs/09_devops/pipeline-ci-cd_v1.0.md` §5.6 |
| DORA Lead Time target | High (1d-1sem) | `/docs/09_devops/pipeline-ci-cd_v1.0.md` §10.2 |
| DORA Change Failure Rate target | Elite (0–5%) | `/docs/09_devops/pipeline-ci-cd_v1.0.md` §10.2 |
| Pipeline stages | 15 | `/docs/09_devops/pipeline-ci-cd_v1.0.md` §5 |
| Workflows GitHub Actions | 5 | `/docs/09_devops/README.md` |
| Paquetes publicables | 4 (Core, Parser, Rendering, Extensions) | `/docs/09_devops/guia-publicacion-nuget_v1.0.md` §3 |
| TFM | `net10.0` único | `/docs/09_devops/estrategia-versionado_v1.0.md` §14 |

---

# 13. Bibliografía consolidada

## 13.1. Especificaciones normativas

- **Semantic Versioning 2.0.0** (Tom Preston-Werner, 2013) — <https://semver.org/spec/v2.0.0.html>
- **Conventional Commits 1.0.0** (Angular Team, 2017) — <https://www.conventionalcommits.org/en/v1.0.0/>
- **Keep a Changelog 1.1.0** (Olivier Lacan, 2014) — <https://keepachangelog.com/en/1.1.0/>
- **CalVer** (Mahmoud Hashemi, 2016) — <https://calver.org/>
- **ZeroVer** (sátira de anti-pattern) — <https://0ver.org/>
- **CycloneDX 1.6** (OWASP / Ecma TC54, 2024) — <https://cyclonedx.org/>
- **SPDX 2.3 / 3.0** (Linux Foundation; ISO/IEC 5962:2021) — <https://spdx.dev/>

## 13.2. Frameworks de supply chain

- **SLSA v1.0** (OpenSSF) — <https://slsa.dev/spec/v1.0/>, <https://slsa.dev/spec/v1.0/levels>
- **NIST SSDF SP 800-218** (NIST, 2022) — <https://csrc.nist.gov/Projects/ssdf>
- **OWASP SCVS v1** (OWASP) — <https://owasp.org/www-project-software-component-verification-standard/>
- **OpenSSF Scorecards** — <https://securityscorecards.dev/>
- **Sigstore** (Linux Foundation, 2021 / GA 2022) — <https://sigstore.dev/>
- **in-toto** — <https://in-toto.io/>
- **NTIA Minimum Elements for an SBOM** (NTIA, 2021) — <https://www.ntia.gov/page/software-bill-materials>
- **US Executive Order 14028** (Biden, mayo 2021) — <https://www.whitehouse.gov/briefing-room/presidential-actions/2021/05/12/executive-order-on-improving-the-nations-cybersecurity/>
- **OMB M-22-18** y **M-23-16** (compliance EO 14028) — <https://www.whitehouse.gov/omb/management/ofcio/>

## 13.3. Marcos DevOps y SRE

- **DORA — State of DevOps Research** — <https://dora.dev/research/>, <https://cloud.google.com/devops/state-of-devops/>
- **Forsgren, Humble, Kim — *Accelerate*** (IT Revolution, 2018)
- **Kim, Humble, Debois, Willis — *The DevOps Handbook*** (IT Revolution, 2016)
- **Beyer, Jones, Petoff, Murphy — *Site Reliability Engineering*** (O'Reilly, 2016) — <https://sre.google/books/>
- **Westrum, Ron — "A Typology of Organisational Cultures"** (Quality and Safety in Health Care, 2004)
- **Edwards & Willis — CALMS framework** (2010)
- **CNCF Platform Engineering Maturity Model** (2024) — <https://tag-app-delivery.cncf.io/whitepapers/platform-eng-maturity-model/>
- **OpenGitOps Principles** (CNCF Sandbox, 2022) — <https://opengitops.dev/>

## 13.4. Branching y versionado: fuentes primarias

- **Driessen, Vincent — "A successful Git branching model"** (2010, addendum 2020) — <https://nvie.com/posts/a-successful-git-branching-model/>
- **GitHub Flow** — <https://docs.github.com/en/get-started/using-github/github-flow>
- **Hammant, Paul et al. — Trunk-Based Development** — <https://trunkbaseddevelopment.com/>
- **Thomson, Edward — Microsoft Release Flow** — <https://learn.microsoft.com/en-us/devops/develop/how-microsoft-develops-devops>
- **Ruka, Adam — OneFlow** — <https://www.endoflineblog.com/oneflow-a-git-branching-model-and-workflow>

## 13.5. Tooling normativo

- **MinVer** (Adam Ralph) — <https://github.com/adamralph/minver>
- **Nerdbank.GitVersioning** (Microsoft) — <https://github.com/dotnet/Nerdbank.GitVersioning>
- **GitVersion** — <https://gitversion.net/>
- **semantic-release** — <https://semantic-release.gitbook.io/>
- **release-please** (Google) — <https://github.com/googleapis/release-please>
- **changesets** — <https://github.com/changesets/changesets>
- **git-cliff** — <https://git-cliff.org/>
- **commitlint** — <https://commitlint.js.org/>
- **commitizen** — <https://commitizen-tools.github.io/commitizen/>
- **husky** — <https://typicode.github.io/husky/>
- **lefthook** — <https://github.com/evilmartians/lefthook>
- **Microsoft.Sbom.Tool** — <https://github.com/microsoft/sbom-tool>
- **CycloneDX/cyclonedx-dotnet** — <https://github.com/CycloneDX/cyclonedx-dotnet>
- **syft** (Anchore) — <https://github.com/anchore/syft>
- **Cosign** — <https://docs.sigstore.dev/cosign/>
- **GitHub Artifact Attestations** — <https://docs.github.com/en/actions/security-guides/using-artifact-attestations-to-establish-provenance-for-builds>

## 13.6. Plataforma .NET

- **.NET 10 — What's new** — <https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview>
- **NuGet best practices** — <https://learn.microsoft.com/en-us/nuget/concepts/package-authoring-best-practices>
- **NuGet versioning (prerelease)** — <https://learn.microsoft.com/en-us/nuget/concepts/package-versioning>
- **GitHub Packages — NuGet registry** — <https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry>
- **GitHub Security Advisories** — <https://docs.github.com/en/code-security/security-advisories>
- **Dependabot** — <https://docs.github.com/en/code-security/dependabot>
- **Source Link** — <https://github.com/dotnet/sourcelink>

## 13.7. Casos de industria citados

- **Netflix Spinnaker** — <https://spinnaker.io/>
- **Spotify Backstage** (CNCF Incubating) — <https://backstage.io/>
- **Stripe API versioning** — <https://stripe.com/docs/api/versioning>
- **Shopify deployment cadence** — <https://shopify.engineering/>
- **GitHub Actions deployment of GitHub** — <https://github.blog/engineering/>
- **Microsoft.Extensions packages** — <https://github.com/dotnet/runtime>
- **Newtonsoft.Json** — <https://github.com/JamesNK/Newtonsoft.Json>
- **Serilog** — <https://serilog.net/>
- **Fastlane** (Mobile CD estándar de facto) — <https://fastlane.tools/>
- **ArgoCD** (CNCF Graduated 2022) — <https://argo-cd.readthedocs.io/>
- **Flux CD** (CNCF Graduated 2022) — <https://fluxcd.io/>
- **dbt Cloud** — <https://www.getdbt.com/>
- **Apache Airflow** — <https://airflow.apache.org/>
- **Apache Iceberg** — <https://iceberg.apache.org/>
- **MLflow** — <https://mlflow.org/>
- **DVC** — <https://dvc.org/>
- **Hugging Face Hub** — <https://huggingface.co/docs/hub/>

### Incidentes de supply chain

- **CISA Advisory AA20-352A — SolarWinds Orion** — <https://www.cisa.gov/news-events/cybersecurity-advisories/aa20-352a>
- **CVE-2021-44228 — Log4Shell** — <https://nvd.nist.gov/vuln/detail/CVE-2021-44228>
- **CVE-2024-3094 — xz-utils** — <https://nvd.nist.gov/vuln/detail/CVE-2024-3094>
- **PoisonGPT (demostración)** — <https://blog.mithrilsecurity.io/poisongpt-how-we-hid-a-lobotomized-llm-on-hugging-face-to-spread-fake-news/>

## 13.8. Estándares emergentes

- **EU CRA (Reg. 2024/2847)** — <https://eur-lex.europa.eu/eli/reg/2024/2847/oj>
- **EU AI Act (Reg. 2024/1689)** — <https://eur-lex.europa.eu/eli/reg/2024/1689/oj>
- **ISO/IEC 5962:2021 (SPDX)** — <https://www.iso.org/standard/81870.html>
- **Iceberg V3 Row Lineage** (2025) — <https://iceberg.apache.org/spec/>
- **MLflow 3.0 GenAI Registry** (2025) — <https://mlflow.org/docs/latest/genai/>
- **safetensors** (Hugging Face) — <https://github.com/huggingface/safetensors>
- **OpenSSF Best Practices Badge** — <https://www.bestpractices.dev/>

---

## Control de cambios

| Versión | Fecha | Autor | Cambios |
|---|---|---|---|
| 1.0 | 2026-04-25 | Arquitectura / DevOps — Release Engineering | Versión inicial. Marco teórico DevOps + supply chain + comparativa cross-domain con el Motor DSL como caso de estudio integrador. Cubre CALMS, Three Ways, DORA + 4 capabilities, SemVer 2.0.0, CalVer, Conventional Commits, GitHub Flow vs GitFlow vs TBD vs Release Flow vs OneFlow, pipeline 15 stages, supply chain (SLSA v1.0, SBOM CycloneDX/SPDX, NIST SSDF SP 800-218, OWASP SCVS v1, OpenSSF Scorecards, Sigstore, EU CRA), release engineering (feeds Preview/Stable, deprecación 3-fases, yanking CVSS≥9.0), GitOps como anti-aplicación al caso, 6 contextos cross-domain (SaaS / OSS .NET / Mobile / Microservicios+GitOps / Data / ML) con tabla maestra y Motor DSL como séptima columna, decisiones D1–D5+D7 fundamentadas, bibliografía agrupada temáticamente. Documenta deuda documental conocida en `/devs/devops/guia-flujo-trabajo-versionado.md` (§6.7). |

---

**Fin del documento**
