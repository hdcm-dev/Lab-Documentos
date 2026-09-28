# Guía de Flujo de Trabajo, Versionado y Convenciones de Desarrollo

**Audiencia:** Equipo de desarrollo .NET  
**Última actualización:** Abril 2026  
**Propósito:** Documento técnico de referencia interna — autocontenido para onboarding

---

## Tabla de Contenidos

1. [Flujo de Trabajo con GitHub](#sección-1-flujo-de-trabajo-con-github)
2. [Versionado Semántico con Número de Build](#sección-2-versionado-semántico-con-número-de-build)
3. [Ramas de Mantenimiento (Bifurcación)](#sección-3-ramas-de-mantenimiento-bifurcación)
4. [Convención de Commits](#sección-4-convención-de-commits-conventional-commits)
5. [Diccionario Completo de Commits para .NET](#sección-5-diccionario-completo-de-commits-para-desarrollo-net)
6. [Cheat Sheet](#cheat-sheet-resumen-de-una-página)

---

## SECCIÓN 1: Flujo de Trabajo con GitHub

### 1.1 Ramas permanentes

El repositorio tiene **dos ramas protegidas** que nunca se eliminan:

| Rama | Propósito | Entorno | Quién mergea |
|------|-----------|---------|--------------|
| `main` | Código en **producción** | Producción | Solo vía PR desde `homologacion` |
| `homologacion` | Código en **staging/QA** | Homologación | Solo vía PR desde ramas de issue |

> **Regla absoluta:** Nadie commitea ni pushea directamente a `main` ni a `homologacion`. Todo cambio entra por Pull Request.

### 1.2 Ciclo de trabajo del desarrollador

1. Se crea un **issue en GitHub** describiendo la tarea, bug o mejora.
2. El desarrollador crea una rama **desde `homologacion`** con la nomenclatura:

   ```
   {número_issue}-homologacion-{siglas_dev}
   ```

   **Ejemplo:** `42-homologacion-JPC`

3. El desarrollador trabaja en su rama, commiteando según la [convención de commits](#sección-4-convención-de-commits-conventional-commits).
4. Al terminar, abre un **Pull Request hacia `homologacion`**.
5. El PR se revisa (code review), se aprueba y se mergea a `homologacion`.
6. La rama del issue (`42-homologacion-JPC`) **se elimina** después del merge.
7. Se ejecutan pruebas integrales en el entorno de homologación.
8. Cuando `homologacion` se estabiliza y pasa QA, se abre un **PR de `homologacion` → `main`**.
9. Al mergear a `main`, se crea un **tag de versión** (ver [Sección 2](#sección-2-versionado-semántico-con-número-de-build)) y opcionalmente un **GitHub Release** con changelog autogenerado.

### 1.3 Comandos git del ciclo completo

```bash
# 1. Posicionarse en homologacion actualizada
git checkout homologacion
git pull origin homologacion

# 2. Crear rama del issue
git checkout -b 42-homologacion-JPC

# 3. Trabajar y commitear
git add .
git commit -m "feat(api): agregar endpoint de consulta de saldo"

# 4. Pushear la rama
git push origin 42-homologacion-JPC

# 5. Abrir PR en GitHub (web o CLI)
gh pr create --base homologacion --title "feat(api): agregar endpoint de consulta de saldo" --body "Closes #42"

# 6. Después del merge, limpiar la rama local
git checkout homologacion
git pull origin homologacion
git branch -d 42-homologacion-JPC
```

### 1.4 Diagrama de flujo del ciclo completo

```
  ┌──────────────────────────────────────────────────────────────────────┐
  │                        FLUJO DE TRABAJO                             │
  └──────────────────────────────────────────────────────────────────────┘

  ① ISSUE                ② RAMA                  ③ DESARROLLO
  ┌─────────────┐       ┌──────────────────┐     ┌──────────────────┐
  │  Se crea    │──────▶│  Se crea rama    │────▶│  Commits en      │
  │  issue #42  │       │  desde           │     │  rama del issue  │
  │  en GitHub  │       │  homologacion    │     │                  │
  └─────────────┘       │                  │     │  42-homologacion │
                        │  42-homologacion │     │  -JPC            │
                        │  -JPC            │     └────────┬─────────┘
                        └──────────────────┘              │
                                                          ▼
  ⑥ PRODUCCIÓN           ⑤ QA/TESTING            ④ PULL REQUEST
  ┌─────────────────┐   ┌──────────────────┐     ┌──────────────────┐
  │  PR:            │   │  Pruebas         │     │  PR hacia        │
  │  homologacion   │◀──│  integrales en   │◀────│  homologacion    │
  │  → main         │   │  entorno de      │     │                  │
  │                 │   │  homologación    │     │  Code review     │
  │  Tag: v1.2.0.5  │   │                  │     │  + Aprobación    │
  │  GitHub Release │   │  ¿Pasa QA?       │     │  + Merge         │
  │                 │   │  ───────────     │     │                  │
  │                 │   │  SÍ → paso ⑥     │     │  Se elimina la   │
  │                 │   │  NO → volver ③   │     │  rama del issue  │
  └─────────────────┘   └──────────────────┘     └──────────────────┘

  Flujo lineal:  Issue → Rama → Commits → PR a homologacion → QA → PR a main → Tag
```

### 1.5 Reglas del flujo

| ✅ Permitido | ❌ Prohibido |
|-------------|-------------|
| Crear ramas desde `homologacion` | Push directo a `main` |
| Mergear a `homologacion` vía PR aprobado | Push directo a `homologacion` |
| Mergear `homologacion` → `main` vía PR cuando QA pasa | Mergear ramas de issue directo a `main` |
| Eliminar ramas de issue después del merge | Dejar ramas de issue vivas después del merge |
| Crear tags en `main` después de mergear | Mergear a `main` sin haber pasado por homologación |
| Force-push en ramas de issue propias (rebase) | Force-push a `main` o `homologacion` |
| Mergear PRs con al menos 1 aprobación | Mergear PRs propios sin revisión (self-merge) |

---

## SECCIÓN 2: Versionado Semántico con Número de Build

### 2.1 Formato de versión

```
MAJOR.MINOR.PATCH.BUILD
```

| Componente | Qué representa | Cuándo sube | Ejemplo de cambio |
|------------|---------------|-------------|-------------------|
| **MAJOR** | Cambios que **rompen compatibilidad** (breaking changes) | Reestructuración de API, cambio de esquema de BD que rompe mapeos, migración de framework | `1.x.x.x` → `2.0.0.x` |
| **MINOR** | **Funcionalidad nueva** sin romper lo existente | Nuevo endpoint, nuevo módulo, nueva tabla en BD | `1.0.x.x` → `1.1.0.x` |
| **PATCH** | **Correcciones de bugs** sin agregar funcionalidad | Fix de cálculo, corrección de query, fix de validación | `1.0.0.x` → `1.0.1.x` |
| **BUILD** | Número **incremental por compilación/deploy** a producción | Cada vez que se mergea a `main` y se deploya | Siempre sube |

### 2.2 Reglas del BUILD

- El BUILD **siempre incrementa** con cada merge a `main` que genera un deploy.
- **Nunca se resetea** (convención del equipo). Esto permite identificar exactamente qué build está corriendo en cualquier entorno.
- El BUILD es un contador global del proyecto, no relativo al MAJOR/MINOR/PATCH.

### 2.3 Progresión de versiones — Ejemplo

| Versión | Qué pasó | PRs incluidos | BUILD |
|---------|----------|---------------|-------|
| `v1.0.0.1` | Release inicial | — | 1 |
| `v1.0.1.2` | Fix de bug en cálculo de totales | fix(service) | 2 |
| `v1.0.2.3` | Fix de query de reportes | fix(db) | 3 |
| `v1.1.0.4` | Nuevo endpoint de consulta de saldo | feat(api) + fix(auth) | 4 |
| `v1.1.1.5` | Corrección de validación en formulario | fix(ui) | 5 |
| `v1.2.0.6` | Nuevo módulo de notificaciones por email | feat(mail), feat(worker) | 6 |
| `v2.0.0.7` | Migración de API v1 a v2 (breaking) | feat!(api), feat!(dto) | 7 |
| `v2.0.1.8` | Fix de encoding en nuevo módulo de mail | fix(mail) | 8 |
| `v2.1.0.9` | Nueva integración con servicio externo | feat(integration) | 9 |

### 2.4 Regla de decisión al versionar

Cuando `homologacion` se mergea a `main`, se revisan **todos los PRs acumulados** desde la última versión:

```
¿Hay al menos un commit feat! o BREAKING CHANGE?
  └── SÍ → MAJOR sube, MINOR y PATCH se resetean a 0

¿Hay al menos un commit feat (sin !)?
  └── SÍ → MINOR sube, PATCH se resetea a 0

¿Solo hay fix, refactor, chore, docs, etc.?
  └── SÍ → PATCH sube

En TODOS los casos: BUILD incrementa en +1
```

> **Siempre gana el de mayor peso.** Si hay un `feat!` y tres `feat` y diez `fix`, la versión sube como MAJOR.

### 2.5 Comandos git para tags

```bash
# Crear tag anotado con el formato de versión
git tag -a v1.2.0.6 -m "v1.2.0.6 - Módulo de notificaciones por email"

# Pushear el tag al remoto
git push origin v1.2.0.6

# Pushear todos los tags pendientes
git push origin --tags

# Ver todos los tags ordenados por versión
git tag --sort=-v:refname

# Crear un GitHub Release desde CLI (requiere gh instalado)
gh release create v1.2.0.6 --title "v1.2.0.6" --generate-notes
```

---

## SECCIÓN 3: Ramas de Mantenimiento (Bifurcación)

### 3.1 ¿Cuándo se necesita una rama de mantenimiento?

Cuando un cliente o entorno está corriendo una **versión anterior** y necesita un parche **sin recibir los cambios de versiones superiores**.

> **Regla:** Las ramas de mantenimiento se crean **solo cuando surge la necesidad**, nunca de forma preventiva.

**Ejemplo concreto:** Producción está en `v2.1.0.9`, pero un cliente legacy sigue en `v1.0.1.3` y reporta un bug crítico. No podemos darle la `v2.x` porque requiere una migración. Necesitamos parchear la `v1.0.x`.

### 3.2 Nomenclatura

```
maintenance/v{MAJOR}.{MINOR}.x
```

**Ejemplo:** `maintenance/v1.0.x`

### 3.3 Ciclo de vida de una rama de mantenimiento

1. **Crear** la rama desde el tag exacto de la versión a parchear:

   ```bash
   git checkout -b maintenance/v1.0.x v1.0.1.3
   git push origin maintenance/v1.0.x
   ```

2. El desarrollador crea **su rama de trabajo** desde la rama de mantenimiento:

   ```
   {número_issue}-maintenance-{siglas_dev}
   ```

   ```bash
   git checkout maintenance/v1.0.x
   git pull origin maintenance/v1.0.x
   git checkout -b 87-maintenance-JPC
   ```

3. Trabaja, commitea y abre un **PR hacia `maintenance/v1.0.x`**.

4. Al mergear, se crea un **nuevo tag** con PATCH incrementado y BUILD global:

   ```bash
   git tag -a v1.0.2.10 -m "v1.0.2.10 - Fix crítico de cálculo para cliente legacy"
   git push origin v1.0.2.10
   ```

5. Si el fix **también aplica a la versión actual**, se hace cherry-pick hacia `homologacion`:

   ```bash
   git checkout homologacion
   git cherry-pick <hash_del_commit>
   # Resolver conflictos si los hay, luego:
   git push origin homologacion
   ```

6. La rama de mantenimiento **se elimina** cuando el cliente/entorno migra a una versión superior.

### 3.4 Diagrama de árbol de ramas

```
  Tag: v1.0.1.3                    Tag: v1.0.2.10
       │                                │
       ▼                                ▼
  ─────●────────────────────────────────●──────── maintenance/v1.0.x
       │                                │
       │          (cherry-pick si aplica)│
       │                                │
  ═════╪════════════════════════════════╪═══════════════════════════ tiempo
       │                                │
       │  Tag:     Tag:     Tag:        │  Tag:
       │  v2.0.0.7 v2.0.1.8 v2.1.0.9   │  v2.1.1.11
       │    │        │        │         │    │
       ▼    ▼        ▼        ▼         ▼    ▼
  ─────●────●────────●────────●─────────◆────●──── main
       │                                ▲
       │                                │
  ─────●────●────●───●────●───●────●────●──────── homologacion
            │    │   │    │   │    │    │
            ▼    ▼   ▼    ▼   ▼    ▼    │
           ramas de issues del          │
           equipo de desarrollo    merge a main
                                   cuando QA pasa

  Leyenda:
  ● = commit/merge
  ◆ = cherry-pick aplicado
  ═ = línea de tiempo
```

### 3.5 Reglas de ramas de mantenimiento

| Regla | Detalle |
|-------|---------|
| **Creación bajo demanda** | Solo se crean cuando un cliente/entorno necesita un parche en una versión anterior |
| **Una por línea MAJOR.MINOR** | `maintenance/v1.0.x`, `maintenance/v1.1.x`, etc. No se crean duplicadas |
| **Tag obligatorio** | Todo merge a una rama de mantenimiento genera un tag versionado |
| **Cherry-pick cuando aplica** | Si el fix es relevante para la versión actual, se porta vía cherry-pick |
| **Eliminación al migrar** | Cuando el cliente migra a la versión actual, se elimina la rama |
| **Nunca preventivas** | No se crean "por las dudas" al momento del release |

---

## SECCIÓN 4: Convención de Commits (Conventional Commits)

### 4.1 Formato del mensaje de commit

```
<prefijo>(<scope>): <descripción corta>

[cuerpo opcional — explicación detallada del cambio]

[footer opcional — referencias a issues, breaking changes]
```

### 4.2 Prefijos disponibles

| Prefijo | Cuándo usar | Impacto en versión |
|---------|-------------|-------------------|
| `feat` | Funcionalidad nueva | MINOR |
| `feat!` | Funcionalidad con breaking change (el `!` lo marca) | MAJOR |
| `fix` | Corrección de bug | PATCH |
| `refactor` | Reestructuración sin cambiar comportamiento | Ninguno |
| `perf` | Mejora de rendimiento | Ninguno |
| `test` | Agregar o corregir tests | Ninguno |
| `chore` | Tareas de mantenimiento (dependencias, config) | Ninguno |
| `ci` | Cambios en pipeline CI/CD | Ninguno |
| `build` | Cambios en sistema de build o Docker | Ninguno |
| `docs` | Documentación | Ninguno |
| `style` | Formato de código sin cambiar lógica | Ninguno |

### 4.3 Scopes recomendados para .NET

| Scope | Descripción | Ejemplo de uso |
|-------|-------------|----------------|
| `api` | Controllers, endpoints, middlewares HTTP | `feat(api): agregar endpoint GET /clientes/{id}` |
| `service` | Capa de servicios / lógica de aplicación | `fix(service): corregir cálculo de descuento` |
| `repo` | Capa de repositorios / acceso a datos | `refactor(repo): extraer repositorio genérico` |
| `domain` | Entidades de dominio, value objects, enums | `feat(domain): agregar entidad Factura` |
| `dto` | Data Transfer Objects, ViewModels, contratos | `feat!(dto): reestructurar ClienteResponse` |
| `db` | Migraciones, scripts SQL, esquema de BD | `feat(db): crear tabla AuditoriaAccesos` |
| `auth` | Autenticación, autorización, roles, permisos | `fix(auth): corregir validación de token expirado` |
| `config` | Archivos de configuración, appsettings, variables de entorno | `chore(config): agregar connection string de staging` |
| `ui` | Componentes Blazor/Razor, vistas, layouts | `feat(ui): agregar componente de paginación` |
| `test` | Tests unitarios, de integración, fixtures | `test(test): agregar tests para FacturaService` |
| `infra` | Infraestructura, IoC, DI, startup | `refactor(infra): migrar a minimal API hosting` |
| `nuget` | Paquetes NuGet, dependencias externas | `chore(nuget): actualizar Serilog a v4.0` |
| `docs` | Documentación técnica, README, Swagger | `docs(docs): documentar flujo de autenticación` |
| `perf` | Optimizaciones de rendimiento, caching, queries | `perf(perf): optimizar consulta de listado de productos` |
| `signal` | Hubs de SignalR, conexiones real-time | `feat(signal): agregar hub de notificaciones en tiempo real` |
| `worker` | Background services, jobs, Hangfire, hosted services | `feat(worker): agregar job de limpieza de sesiones` |
| `cache` | Redis, MemoryCache, estrategias de invalidación | `fix(cache): corregir invalidación de caché de catálogo` |
| `log` | Logging, Serilog, Application Insights, trazas | `chore(log): configurar enricher de correlación` |
| `mail` | Envío de emails, templates, SMTP | `feat(mail): agregar template de bienvenida` |
| `report` | Reportes, exports, generación de PDFs/Excel | `feat(report): agregar reporte de ventas mensual` |
| `integration` | Integraciones con servicios externos, APIs de terceros | `feat(integration): integrar API de AFIP para facturación` |

### 4.4 Reglas del commit message

| Regla | Ejemplo correcto | Ejemplo incorrecto |
|-------|------------------|--------------------|
| Descripción en **minúsculas** | `agregar endpoint de consulta` | `Agregar Endpoint de Consulta` |
| **Sin punto final** | `corregir cálculo de IVA` | `corregir cálculo de IVA.` |
| Verbo en **imperativo** | `agregar`, `corregir`, `eliminar` | `se agrega`, `agregado`, `agregando` |
| Scope **opcional** pero recomendado | `feat(api): agregar endpoint` | `feat: agregar endpoint` (válido pero menos claro) |
| Si un refactor cambia firma pública → **es `feat!`** | `feat!(service): cambiar firma de CalcularTotal` | `refactor(service): cambiar firma de CalcularTotal` |
| `BREAKING CHANGE` en footer como alternativa al `!` | Ver ejemplo en [5.9](#59-ejemplos-completos-de-commits) | — |
| `Closes #N` en footer para cerrar issues | `Closes #42` | `Cierra el issue 42` |

---

## SECCIÓN 5: Diccionario Completo de Commits para Desarrollo .NET

### 5.1 Código .NET — Features (`feat`)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `feat` | `api` | Nuevo endpoint | MINOR | `feat(api): agregar endpoint GET /api/v1/clientes/{id}/saldo` |
| `feat` | `service` | Nueva lógica de negocio | MINOR | `feat(service): implementar cálculo de descuento por volumen` |
| `feat` | `domain` | Nueva entidad de dominio | MINOR | `feat(domain): agregar entidad OrdenDeCompra con estados` |
| `feat` | `ui` | Nuevo componente UI (Blazor/Razor) | MINOR | `feat(ui): agregar componente DataGrid con ordenamiento` |
| `feat` | `report` | Nuevo reporte | MINOR | `feat(report): agregar reporte de ventas mensual en PDF` |
| `feat` | `auth` | Nuevo rol/permiso | MINOR | `feat(auth): agregar rol Auditor con permisos de solo lectura` |
| `feat` | `worker` | Nuevo job/worker | MINOR | `feat(worker): agregar job de sincronización nocturna de stock` |
| `feat` | `signal` | Funcionalidad real-time | MINOR | `feat(signal): agregar hub de notificaciones de pedidos en tiempo real` |
| `feat` | `integration` | Nueva integración externa | MINOR | `feat(integration): integrar API de MercadoPago para cobros` |
| `feat` | `mail` | Nueva notificación por email | MINOR | `feat(mail): agregar notificación de orden confirmada` |

### 5.2 Código .NET — Breaking Changes (`feat!`)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `feat!` | `api` | Migración de versión de API | MAJOR | `feat!(api): migrar endpoints de /api/v1/ a /api/v2/` |
| `feat!` | `domain` | Cambio de modelo de dominio | MAJOR | `feat!(domain): separar entidad Cliente en ClientePersona y ClienteEmpresa` |
| `feat!` | `dto` | Cambio de contratos DTO | MAJOR | `feat!(dto): reestructurar PedidoResponse eliminando campos legacy` |
| `feat!` | `auth` | Migración de esquema de autenticación | MAJOR | `feat!(auth): migrar autenticación de cookies a JWT Bearer` |
| `feat!` | `config` | Reestructuración de configuración | MAJOR | `feat!(config): reestructurar appsettings separando por módulo` |
| `feat!` | `service` | Cambio de firma de servicio público | MAJOR | `feat!(service): cambiar firma de IFacturacionService.Calcular()` |

### 5.3 Código .NET — Fixes (`fix`)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `fix` | `api` | Corrección de status code | PATCH | `fix(api): retornar 404 en lugar de 500 cuando cliente no existe` |
| `fix` | `service` | Corrección de lógica de negocio | PATCH | `fix(service): corregir cálculo de IVA para productos exentos` |
| `fix` | `repo` | Corrección de query/mapeo | PATCH | `fix(repo): corregir mapeo de fecha en consulta de pedidos` |
| `fix` | `auth` | Corrección de acceso/auth | PATCH | `fix(auth): corregir validación de token expirado que permitía acceso` |
| `fix` | `ui` | Corrección de UI | PATCH | `fix(ui): corregir desbordamiento de tabla en resoluciones móviles` |
| `fix` | `mail` | Corrección de encoding en email | PATCH | `fix(mail): corregir encoding UTF-8 en template de factura` |
| `fix` | `worker` | Corrección de timeout en job | PATCH | `fix(worker): aumentar timeout de job de sincronización a 30 min` |
| `fix` | `cache` | Corrección de invalidación de caché | PATCH | `fix(cache): corregir invalidación de caché al actualizar producto` |
| `fix` | `integration` | Corrección de integración externa | PATCH | `fix(integration): corregir parseo de respuesta XML de AFIP` |
| `fix` | `config` | Corrección de configuración | PATCH | `fix(config): corregir connection string de producción con timeout` |

### 5.4 Código .NET — Refactor, Perf, Test

#### Refactor

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `refactor` | `service` | Extraer método privado | Ninguno | `refactor(service): extraer validación de stock a método privado` |
| `refactor` | `repo` | Unificar repositorios | Ninguno | `refactor(repo): extraer repositorio genérico base` |
| `refactor` | `infra` | Reorganizar startup | Ninguno | `refactor(infra): separar registro de servicios por módulo` |
| `refactor` | `domain` | Aplicar value object | Ninguno | `refactor(domain): reemplazar string Email por value object Email` |
| `refactor` | `api` | Simplificar controller | Ninguno | `refactor(api): mover validación de request a FluentValidation` |

#### Perf

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `perf` | `repo` | Optimizar query | Ninguno | `perf(repo): agregar paginación server-side en listado de productos` |
| `perf` | `cache` | Implementar caché | Ninguno | `perf(cache): agregar caché distribuido para catálogo de productos` |
| `perf` | `api` | Reducir payload | Ninguno | `perf(api): implementar response compression con Brotli` |
| `perf` | `db` | Optimizar índex | Ninguno | `perf(db): agregar índice compuesto en Pedidos(ClienteId, Fecha)` |
| `perf` | `service` | Reducir allocations | Ninguno | `perf(service): usar Span<T> en parseo de códigos de barra` |

#### Test

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `test` | `service` | Tests de lógica de negocio | Ninguno | `test(service): agregar tests para cálculo de descuento por volumen` |
| `test` | `api` | Tests de integración | Ninguno | `test(api): agregar integration tests para endpoint de clientes` |
| `test` | `repo` | Tests de repositorio | Ninguno | `test(repo): agregar tests con InMemoryDatabase para PedidoRepo` |
| `test` | `auth` | Tests de autorización | Ninguno | `test(auth): agregar tests de política de permisos por rol` |
| `test` | `domain` | Tests de entidad | Ninguno | `test(domain): agregar tests de invariantes de OrdenDeCompra` |

### 5.5 Código .NET — Chore, CI, Build, Docs, Style

#### Chore

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `chore` | `nuget` | Actualizar paquete | Ninguno | `chore(nuget): actualizar Serilog de v3.1 a v4.0` |
| `chore` | `config` | Cambiar configuración | Ninguno | `chore(config): agregar variables de entorno para staging` |
| `chore` | `log` | Configurar logging | Ninguno | `chore(log): configurar Serilog con sink a Seq` |
| `chore` | `infra` | Limpiar código muerto | Ninguno | `chore(infra): eliminar middleware de logging legacy` |

#### CI

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `ci` | `infra` | Pipeline CI/CD | Ninguno | `ci(infra): agregar workflow de GitHub Actions para build y test` |
| `ci` | `infra` | Configurar deploy | Ninguno | `ci(infra): agregar step de deploy a Azure App Service` |
| `ci` | `test` | Agregar coverage | Ninguno | `ci(test): agregar reporte de cobertura con Coverlet` |

#### Build

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `build` | `infra` | Docker | Ninguno | `build(infra): agregar Dockerfile multi-stage para API` |
| `build` | `config` | Target framework | Ninguno | `build(config): migrar target framework de net7.0 a net8.0` |
| `build` | `infra` | Docker Compose | Ninguno | `build(infra): agregar docker-compose con SQL Server y Redis` |

#### Docs

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `docs` | `api` | Swagger/OpenAPI | Ninguno | `docs(api): agregar descripciones a endpoints de Swagger` |
| `docs` | `docs` | README | Ninguno | `docs(docs): actualizar README con instrucciones de setup local` |
| `docs` | `config` | Documentar variables | Ninguno | `docs(config): documentar variables de entorno requeridas` |

#### Style

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `style` | `api` | Formato de código | Ninguno | `style(api): aplicar formato de código con dotnet format` |
| `style` | `service` | Ordenar usings | Ninguno | `style(service): ordenar usings y eliminar imports no usados` |
| `style` | `domain` | Consistencia de naming | Ninguno | `style(domain): renombrar propiedades a PascalCase` |

### 5.6 Base de Datos — Cambios Estructurales (DDL)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `feat` | `db` | Crear tabla nueva | MINOR | `feat(db): crear tabla AuditoriaAccesos con campos usuario, accion, fecha` |
| `feat` | `db` | Agregar columna nullable | MINOR | `feat(db): agregar columna Observaciones (nullable) a tabla Pedidos` |
| `feat` | `db` | Crear vista | MINOR | `feat(db): crear vista vw_ResumenVentasMensual` |
| `feat` | `db` | Crear stored procedure | MINOR | `feat(db): crear SP sp_ObtenerTopClientesPorPeriodo` |
| `feat` | `db` | Crear función | MINOR | `feat(db): crear función fn_CalcularEdad para cálculo normalizado` |
| `feat` | `db` | Agregar trigger | MINOR | `feat(db): agregar trigger de auditoría en tabla Productos` |
| `feat` | `db` | Agregar constraint CHECK/UNIQUE | MINOR | `feat(db): agregar constraint UNIQUE en Clientes(CUIT)` |
| `feat` | `db` | Crear índice | MINOR | `feat(db): crear índice IX_Pedidos_ClienteId_Fecha en Pedidos` |

### 5.7 Base de Datos — Breaking Changes

> **⚠️ Regla fundamental:** Toda operación que pueda romper queries existentes, mapeos de EF Core, o reportes es **BREAKING CHANGE**. Ante la duda, es MAJOR.

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `feat!` | `db` | Eliminar tabla | MAJOR | `feat!(db): eliminar tabla ProductosLegacy` |
| `feat!` | `db` | Eliminar columna con datos | MAJOR | `feat!(db): eliminar columna CodigoAnterior de tabla Clientes` |
| `feat!` | `db` | Renombrar tabla | MAJOR | `feat!(db): renombrar tabla Pedidos a OrdenesDeCompra` |
| `feat!` | `db` | Renombrar columna | MAJOR | `feat!(db): renombrar columna Nombre a RazonSocial en Clientes` |
| `feat!` | `db` | Cambiar tipo de dato de columna | MAJOR | `feat!(db): cambiar Precio de decimal(10,2) a decimal(18,4)` |
| `feat!` | `db` | Agregar columna NOT NULL sin default | MAJOR | `feat!(db): agregar columna TipoCliente NOT NULL a Clientes` |
| `feat!` | `db` | Cambiar PK o FK existente | MAJOR | `feat!(db): cambiar PK de Productos de int a GUID` |
| `feat!` | `db` | Particionar/dividir tabla | MAJOR | `feat!(db): dividir tabla Direcciones en DireccionesEnvio y DireccionesFacturacion` |
| `feat!` | `db` | Cambiar estructura de vista crítica | MAJOR | `feat!(db): reestructurar vista vw_DashboardVentas eliminando columnas` |
| `feat!` | `db` | Cambiar firma de SP público | MAJOR | `feat!(db): cambiar parámetros de sp_GenerarFactura` |

### 5.8 Base de Datos — Correcciones y Datos

#### Correcciones (`fix` → PATCH)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `fix` | `db` | Corregir migración fallida | PATCH | `fix(db): corregir migración 20260401 que fallaba en índice duplicado` |
| `fix` | `db` | Corregir constraint | PATCH | `fix(db): corregir constraint CHECK de rango de fechas en Contratos` |
| `fix` | `db` | Corregir vista | PATCH | `fix(db): corregir JOIN en vw_ResumenVentasMensual que duplicaba filas` |
| `fix` | `db` | Corregir stored procedure | PATCH | `fix(db): corregir filtro de fechas en sp_ObtenerVentasPorPeriodo` |
| `fix` | `db` | Corregir datos inconsistentes | PATCH | `fix(db): corregir registros huérfanos en tabla DetallePedido` |

#### Datos y mantenimiento (`chore` → Ninguno)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `chore` | `db` | Seed data / datos iniciales | Ninguno | `chore(db): agregar seed data de provincias argentinas` |
| `chore` | `db` | Migración de datos | Ninguno | `chore(db): migrar datos de tabla legacy a nueva estructura` |

#### Optimizaciones (`perf` → Ninguno)

| Prefijo | Scope | Descripción | Impacto | Ejemplo de commit |
|---------|-------|-------------|---------|-------------------|
| `perf` | `db` | Optimizar índices | Ninguno | `perf(db): agregar índice filtrado en Pedidos para estado Activo` |
| `perf` | `db` | Optimizar stored procedure | Ninguno | `perf(db): optimizar sp_ReporteVentas reemplazando cursor por CTE` |

### 5.9 Ejemplos Completos de Commits

#### Ejemplo 1 — MINOR (nueva funcionalidad)

```
feat(api): agregar endpoint de consulta de saldo de cliente

Se agrega el endpoint GET /api/v1/clientes/{id}/saldo que retorna
el saldo actual del cliente calculado desde sus movimientos.

Incluye:
- Controller ClientesSaldoController
- Servicio SaldoService con cálculo desde MovimientosRepository
- DTO SaldoClienteResponse con campos saldo, moneda, fechaCorte

Closes #42
```

#### Ejemplo 2 — MAJOR (breaking change con `!`)

```
feat!(auth): migrar autenticación de cookies a JWT Bearer

Se reemplaza el esquema de autenticación basado en cookies por
JWT Bearer tokens. Esto rompe todos los clientes que usan cookies
para autenticarse.

Cambios:
- Eliminar CookieAuthenticationHandler
- Agregar JwtBearerHandler con validación RS256
- Nuevos endpoints POST /auth/login y POST /auth/refresh
- Eliminar endpoint GET /auth/session (ya no aplica)

BREAKING CHANGE: los clientes deben migrar de cookies a JWT.
El endpoint /auth/session fue eliminado.

Closes #98
```

#### Ejemplo 3 — PATCH (corrección de bug)

```
fix(service): corregir cálculo de IVA para productos exentos

Los productos marcados como exentos de IVA estaban recibiendo
el cálculo estándar del 21%. Se corrige la condición para
verificar el flag ExentoIVA antes de aplicar el porcentaje.

Closes #103
```

#### Ejemplo 4 — Ninguno (refactor sin impacto en versión)

```
refactor(repo): extraer repositorio genérico base

Se extrae la lógica común de CRUD a una clase RepositoryBase<T>
para reducir duplicación entre los repositorios existentes.

No hay cambios en firmas públicas ni en comportamiento.
Los tests existentes siguen pasando sin modificaciones.
```

#### Ejemplo 5 — MAJOR (breaking change con footer)

```
feat(db): reestructurar tabla Clientes separando direcciones

Se normaliza la tabla Clientes extrayendo los campos de dirección
a una nueva tabla Direcciones con relación 1:N.

Cambios en BD:
- Crear tabla Direcciones (Id, ClienteId, Calle, Numero, CP, etc.)
- Migrar datos existentes de Clientes a Direcciones
- Eliminar columnas Calle, Numero, CP, Localidad de Clientes
- Actualizar FK y mapeos de EF Core

BREAKING CHANGE: los mapeos de EF Core y queries que accedan
a campos de dirección directamente desde Clientes van a fallar.
Actualizar todas las referencias a usar la nueva tabla Direcciones.

Closes #115
```

---

## Cheat Sheet — Resumen de Una Página

### Prefijos y su impacto en versión

| Prefijo | Impacto | ¿Cuándo? |
|---------|---------|----------|
| `feat!` | **MAJOR** | Cambio que rompe compatibilidad |
| `feat` | **MINOR** | Funcionalidad nueva sin romper nada |
| `fix` | **PATCH** | Corrección de bug |
| `refactor` / `perf` / `test` / `chore` / `ci` / `build` / `docs` / `style` | **Ninguno** | Cambios internos sin impacto funcional |

### Formato de versión

```
MAJOR.MINOR.PATCH.BUILD     Ejemplo: v2.1.3.15
```

### Regla de decisión rápida

```
¿Hay feat! o BREAKING CHANGE? → MAJOR (MINOR y PATCH se resetean)
¿Hay feat?                    → MINOR (PATCH se resetea)
¿Solo fix?                    → PATCH
BUILD siempre incrementa (+1)
```

### Nomenclatura de ramas

| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Issue normal | `{nro}-homologacion-{siglas}` | `42-homologacion-JPC` |
| Mantenimiento | `{nro}-maintenance-{siglas}` | `87-maintenance-JPC` |
| Rama de mantenimiento | `maintenance/v{MAJOR}.{MINOR}.x` | `maintenance/v1.0.x` |

### Flujo resumido

```
Issue → Rama → Commits → PR a homologacion → QA → PR a main → Tag → Release
```

### Scopes rápidos

| Scope | Capa |
|-------|------|
| `api` | Controllers / HTTP |
| `service` | Lógica de aplicación |
| `repo` | Acceso a datos |
| `domain` | Modelo de dominio |
| `dto` | Contratos / ViewModels |
| `db` | Base de datos / SQL |
| `auth` | Autenticación / Autorización |
| `config` | Configuración |
| `ui` | Frontend / Blazor / Razor |
| `test` | Tests |
| `infra` | Infraestructura / DI / Startup |
| `nuget` | Dependencias NuGet |
| `signal` | SignalR / Real-time |
| `worker` | Background jobs |
| `cache` | Caché |
| `log` | Logging |
| `mail` | Email |
| `report` | Reportes / Exports |
| `integration` | APIs externas |

### Formato de commit

```
<prefijo>(<scope>): <descripción en imperativo, minúsculas, sin punto>

[cuerpo opcional]

[BREAKING CHANGE: descripción]
[Closes #N]
```

### Reglas que no se negocian

1. **Nunca** push directo a `main` ni a `homologacion`
2. **Nunca** mergear a `main` sin pasar por homologación
3. **Nunca** mergear un PR sin al menos 1 aprobación
4. **Siempre** crear tag al mergear a `main`
5. **Siempre** eliminar la rama del issue después del merge
6. **Siempre** referenciar el issue en el commit (`Closes #N`)
