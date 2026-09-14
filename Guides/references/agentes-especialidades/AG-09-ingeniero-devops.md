# AG-09 — Ingeniero DevOps Senior

**Carpeta SDD:** `docs/09_devops/`  
**Perfil Profesional:** CI/CD, infraestructura, automatización de releases

---

## 1. ¿Qué es esta especialidad?

El **Ingeniero DevOps** es el profesional que diseña y mantiene la infraestructura de automatización que permite al equipo construir, probar, empaquetar y publicar software de manera confiable y repetible. Combina conocimientos de desarrollo, operaciones, seguridad y gestión de configuración.

En el contexto de este proyecto (una librería .NET publicada como paquete NuGet), el Ingeniero DevOps diseña el **pipeline CI/CD**, define la **estrategia de versionado SemVer**, configura los **entornos de deploy**, documenta el proceso de **publicación NuGet** y establece los mecanismos de **rollback** y **monitoreo**.

### Disciplinas que abarca

Esta especialidad engloba varias disciplinas profesionales según la infraestructura y el nivel de madurez operativo:

| Disciplina | Enfoque | Cuándo aplica | Artefactos típicos |
|---|---|---|---|
| **CI/CD Engineering** | Diseñar y mantener pipelines de build, test, empaquetado y publicación automatizados | Todo proyecto con versionado, tests y entregas automatizables | Pipeline YAML (GitHub Actions, Azure DevOps), quality gates, artefactos de build, scripts de release |
| **Infrastructure as Code (IaC)** | Provisionar y gestionar infraestructura cloud de forma declarativa y reproducible | Proyectos desplegados en cloud (AWS, Azure, GCP) con entornos múltiples | Terraform/Bicep/Pulumi scripts, módulos IaC, state management, environment configs, cost monitoring |
| **Site Reliability Engineering (SRE)** | Garantizar confiabilidad, observabilidad y disponibilidad de sistemas en producción | Sistemas con SLOs (uptime, latencia), usuarios en producción, necesidad de on-call | SLIs/SLOs/SLAs, dashboards de observabilidad (Grafana, Datadog), runbooks, incident response, post-mortems, error budgets |
| **DevSecOps** | Integrar seguridad en el pipeline de desarrollo (shift-left security) | Sistemas con requisitos de compliance, datos sensibles, supply chain | SAST/DAST en CI, dependency scanning, SBOM, container scanning, security policies, audit logs |

> **En este proyecto** se aplica **CI/CD Engineering** porque el producto es una librería NuGet con un ciclo de vida digital (build → test → pack → publish) pero sin infraestructura cloud ni servidores en producción.

#### Ejemplos temáticos por disciplina

**Infrastructure as Code** *(aplica en proyectos cloud)*
- *Ejemplo:* Un SaaS de analytics desplegado en AWS necesita 3 entornos (dev, staging, prod) idénticos. El ingeniero IaC define toda la infra en Terraform: VPC, subnets, ECS Fargate clusters, RDS PostgreSQL, S3 buckets, CloudFront CDN. Cada PR de infraestructura pasa por `terraform plan` en CI, y el `terraform apply` a producción requiere aprobación manual y genera un drift report.

**SRE (Site Reliability Engineering)** *(aplica en sistemas con usuarios en producción)*
- *Ejemplo:* Una plataforma de e-commerce con 99.9% SLO de disponibilidad configura observabilidad completa: métricas (Prometheus → Grafana), logs (ELK stack), traces (OpenTelemetry → Jaeger). Define SLIs (latencia p99, error rate, throughput), error budgets (0.1% downtime = 43.8 min/mes), runbooks para incidentes comunes (DB failover, cache invalidation), y post-mortems blameless para cada incidente >5min.

**DevSecOps** *(aplica en sistemas con compliance/seguridad)*
- *Ejemplo:* Una fintech regulada por PCI-DSS integra seguridad en el pipeline: Semgrep SAST en cada PR, OWASP ZAP DAST en staging, Dependabot + Snyk para dependency scanning, generación automática de SBOM (Software Bill of Materials), y Trivy para container image scanning. Cada release require un security sign-off y genera un audit trail de todas las dependencias y vulnerabilidades remediadas.

### ¿DevOps en una librería?

Sí. Aunque no hay servidores que deployar, una librería NuGet tiene su propio ciclo de vida:
- **CI:** Build automático, ejecución de tests, análisis de calidad en cada PR
- **CD:** Empaquetado NuGet, publicación a feed privado/público, gestión de versiones
- **Ops:** Monitoreo de descargas, manejo de breaking changes, deprecación de versiones

---

## 2. Tareas principales

| Tarea | Descripción | Entregable |
|-------|-------------|------------|
| Diseño de pipeline CI/CD | Definir stages, triggers, quality gates y artefactos | `pipeline-ci-cd_v1.0.md` |
| Estrategia de versionado | Definir reglas SemVer para patch/minor/major/pre-release | `estrategia-versionado_v1.0.md` |
| Configuración de entornos | Definir feeds NuGet, environments y variables | `entornos-deploy_v1.0.md` |
| Guía de publicación NuGet | Documentar proceso paso a paso de publicación | `guia-publicacion-nuget_v1.0.md` |
| Manejo de secretos | Configurar vault/variables protegidas para API keys | Sección en pipeline doc |
| Estrategia de rollback | Definir cómo revertir una publicación problemática | Sección en pipeline doc |

---

## 3. Especificaciones normalizadas que produce

| Especificación | Estándar/Referencia | Integración en el ciclo |
|---|---|---|
| Pipeline CI/CD | GitHub Actions / Azure DevOps YAML conventions | Automatización del build-test-publish cycle |
| Versionado SemVer | Semantic Versioning 2.0.0 (semver.org) | Comunicación de compatibilidad a consumidores del paquete |
| Entornos de deploy | NuGet feed management (nuget.org, GitHub Packages) | Publicación de artefactos empaquetados |
| Gestión de secretos | OWASP Secret Management / GitHub Secrets | Protección de API keys y tokens de publicación |
| Changelog | Keep a Changelog convention (keepachangelog.com) | Comunicación de cambios a usuarios del paquete |
| Policy de breaking changes | SemVer major bump policy | Protección de consumidores contra cambios incompatibles |

---

## 4. Criterios de calidad

| Criterio | Descripción | Cómo se mide |
|----------|-------------|--------------|
| Pipeline reproducible | Cualquier dev puede entender y ejecutar el pipeline localmente | Documentación + `dotnet build && dotnet test` funciona local |
| Triggers definidos | Cada pipeline tiene trigger explícito (PR, push, tag) | 0 pipelines sin trigger |
| Quality gates coherentes | Los gates del CI coinciden con el DoD y la estrategia de QA | Diff entre quality gates y DoD |
| SemVer clara | Reglas explícitas para cuándo bump patch/minor/major | Tabla de decisión documentada |
| Publicación reproducible | El proceso NuGet se puede ejecutar sin conocimiento tribal | Guía paso a paso verificada |
| Secretos protegidos | No hay API keys en código ni en logs | 0 secretos expuestos |
| Rollback documentado | Existe proceso para revertir una publicación | Instrucciones paso a paso |

---

## 5. Preguntas guía para el análisis

### 5.1 Pipeline CI/CD

> **P1:** ¿Qué stages tiene el pipeline y qué hace cada uno?
>
> *Ejemplo práctico:*
> ```
> CI Pipeline (en cada PR):
>   1. Restore → dotnet restore
>   2. Build → dotnet build --configuration Release
>   3. Test → dotnet test --collect:"XPlat Code Coverage"
>   4. Quality → verificar cobertura ≥ 80%, 0 warnings nuevos
>   5. Pack → dotnet pack (solo en main)
> 
> CD Pipeline (en tag v*):
>   1. Build release
>   2. Test full suite
>   3. Pack NuGet
>   4. Publish → nuget push al feed
> ```

> **P2:** ¿Qué pasa si un quality gate falla?
>
> *Ejemplo práctico:* "Si cobertura < 80% → build rojo → PR no se puede mergear. Si hay tests fallando → build rojo. Si hay vulnerability en dependencia → warning (no bloquea en MVP, bloquea en GA)."

### 5.2 Versionado

> **P3:** ¿Cuándo se hace un bump de versión y de qué tipo?
>
> *Ejemplo práctico:*
> | Cambio | Tipo de bump | Ejemplo |
> |--------|-------------|---------|
> | Bug fix sin cambio de API | Patch | 0.3.0 → 0.3.1 |
> | Nueva feature backward-compatible | Minor | 0.3.0 → 0.4.0 |
> | Breaking change en API pública | Major | 0.x → 1.0.0 |
> | Pre-release | Pre-release suffix | 0.3.0-alpha.1 |

> **P4:** ¿Cómo se comunican los breaking changes?
>
> *Ejemplo práctico:* "En el CHANGELOG.md con sección `### BREAKING CHANGES` + guía de migración. En el README del paquete NuGet. En el PR description."

### 5.3 Entornos y publicación

> **P5:** ¿Dónde se publica el paquete y quién tiene acceso?
>
> *Ejemplo práctico:*
> | Entorno | Feed | Acceso | Cuándo |
> |---------|------|--------|--------|
> | Dev | GitHub Packages (privado) | Equipo | Cada merge a main |
> | Staging | NuGet.org (unlisted) | Testers | Pre-release tags |
> | Prod | NuGet.org (listed) | Público | Release tags |

> **P6:** ¿Los secretos están protegidos adecuadamente?
>
> *Ejemplo práctico:* "NUGET_API_KEY guardada en GitHub Secrets, no en código. Pipeline config referencia `${{ secrets.NUGET_API_KEY }}`. La key rota cada 90 días."

### 5.4 Rollback

> **P7:** ¿Si publicamos un paquete con un bug crítico, cómo revertimos?
>
> *Ejemplo práctico:*
> 1. Delist la versión afectada: `nuget delete MotorDsl 0.3.1 -Source nuget.org`
> 2. Publicar versión fix como patch: 0.3.2
> 3. Comunicar por changelog y GitHub release notes
> 4. Si es breaking: major bump + guía de migración

---

## 6. Plantilla base

### 6.1 Pipeline CI/CD

```markdown
# Pipeline CI/CD

**Proyecto:** [Nombre]  
**Versión:** 1.0  
**Plataforma:** [GitHub Actions / Azure DevOps / etc.]

---

## 1. Visión general

`` `
PR → CI (build + test + quality) → Merge → CD (pack + publish)
`` `

## 2. CI Pipeline

**Trigger:** Pull Request a `main`  
**Archivo:** `.github/workflows/ci.yml`

| Stage | Comando | Quality Gate |
|-------|---------|-------------|
| Restore | `dotnet restore` | — |
| Build | `dotnet build -c Release` | 0 errores de compilación |
| Test | `dotnet test --collect:"XPlat Code Coverage"` | 0 tests fallidos |
| Quality | Check coverage report | Cobertura ≥ [X]% |

## 3. CD Pipeline

**Trigger:** Push de tag `v*` a `main`  
**Archivo:** `.github/workflows/cd-release.yml`

| Stage | Comando | Condición |
|-------|---------|-----------|
| Build | `dotnet build -c Release` | — |
| Test | `dotnet test` | 0 tests fallidos |
| Pack | `dotnet pack -c Release` | — |
| Publish | `dotnet nuget push` | Tag válido SemVer |

## 4. Secretos requeridos

| Secreto | Uso | Rotación |
|---------|-----|----------|
| `NUGET_API_KEY` | Publicación a NuGet.org | Cada 90 días |

## 5. Rollback

[Instrucciones paso a paso para revertir una publicación]
```

### 6.2 Estrategia de versionado

```markdown
# Estrategia de Versionado

**Proyecto:** [Nombre]  
**Estándar:** Semantic Versioning 2.0.0

---

## Formato

`MAJOR.MINOR.PATCH[-pre-release]`

## Reglas de bump

| Cambio | Bump | Ejemplo |
|--------|------|---------|
| Bug fix, no cambia API | Patch | 0.3.0 → 0.3.1 |
| Feature nueva, backward-compatible | Minor | 0.3.0 → 0.4.0 |
| Breaking change | Major | 0.x → 1.0.0 |
| Pre-release | Suffix | 0.3.0-alpha.1 |

## Reglas de pre-release

- `alpha`: Funcionalidad incompleta, API puede cambiar
- `beta`: Funcionalidad completa, API estable pero no probada en producción
- `rc`: Release candidate, solo bug fixes antes de GA

## Breaking changes policy

1. Documentar en CHANGELOG bajo `### BREAKING CHANGES`
2. Proveer guía de migración
3. Deprecar API anterior con `[Obsolete]` al menos 1 minor version antes
```

---

## 7. Anti-patrones comunes

| Anti-patrón | Problema | Solución |
|---|---|---|
| Pipeline sin quality gates | Tests pasan pero cobertura baja → bugs escapan | Agregar gate de cobertura mínima |
| Secretos en código | API key en `appsettings.json` → exposición | GitHub Secrets / Azure Key Vault |
| Versionado manual | Olvidos, inconsistencias, versiones saltadas | Automatizar con GitVersion o tags |
| Sin rollback | Paquete roto publicado → consumidores bloqueados | Documentar proceso de delist + patch |
| Pipeline no reproducible | "En mi máquina funciona" | `dotnet build && dotnet test` debe funcionar local |
| Changelog ausente | Usuarios del paquete no saben qué cambió | Actualizar changelog en cada PR |
