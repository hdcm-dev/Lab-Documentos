---
doc_id: ANEXO-CHECK
doc_type: anexo
title: Listas de verificación
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-19
audience: [humano, agente]
traces: [MARCO-ESCENARIOS, MARCO-CONTEXTOS, ANEXO-PLANTILLAS, ANEXO-REFERENCIAS]
---

# Listas de verificación — `ANEXO-CHECK`

## Resumen ejecutivo

Cinco listas operativas, una por escenario del marco más una de evaluación de biblioteca. Se usan sobre un repositorio concreto, no se leen.

Cada ítem está formulado para que la respuesta sea verificable —un archivo existe o no, una dependencia apunta en una dirección o en otra— en lugar de depender de apreciación. Los ítems marcados con **⚠** son los que en la experiencia de esta guía se omiten con más frecuencia y cuestan más caro después.

---

## Lista 1 — Arranque de un repositorio nuevo (`ESC-1`)

### Andamiaje

- [ ] Existe `global.json` con la versión del SDK fijada y una política de `rollForward` explícita
- [ ] Existe `Directory.Build.props` con `TargetFramework`, `Nullable` e `ImplicitUsings` en un solo lugar
- [ ] **⚠** Existe `Directory.Packages.props` con `ManagePackageVersionsCentrally=true` — omitirlo obliga a editar cada `.csproj` en cada actualización
- [ ] **⚠** Existe `.editorconfig` con `root = true` — sin él, `EnforceCodeStyleInBuild` solo aplica los valores por defecto del SDK
- [ ] Existe `.gitattributes` con normalización de fin de línea
- [ ] `EnableNETAnalyzers` está activado y `AnalysisMode` elegido conscientemente
- [ ] La canalización de integración continua usa `-warnaserror`; el build local no

### Estructura

- [ ] El código productivo vive bajo `src/`, la infraestructura de build bajo `eng/` y los tests separados del código productivo — las tres piezas con evidencia unánime (`F-01`, `F-07`)
- [ ] La ubicación de los tests (raíz o anidada por componente) y el plural (`test`/`tests`) están **decididos**, no heredados por inercia, y se sostienen sin alternar
- [ ] Cada proyecto declara el SDK correcto para lo que es (`N-10`)
- [ ] Los proyectos de prueba tienen `IsPackable=false`
- [ ] **⚠** Todo proyecto en disco está en la solución, o su ausencia es deliberada y está documentada
- [ ] El nombre de cada proyecto permite anticipar qué contiene
- [ ] `RootNamespace` es coherente con el nombre del proyecto

### Decisiones registradas

- [ ] **⚠** El modelo de despliegue está decidido y registrado en un ADR
- [ ] La decisión de carpetas o proyectos para las capas está registrada, con su criterio
- [ ] El idioma del código —español, inglés o el reparto entre capas— está decidido explícitamente
- [ ] La convención de nombrado de pruebas está fijada

---

## Lista 2 — Evaluar si conviene partir (`ESC-2`)

Antes de reorganizar, responder. Si la mayoría de las respuestas de la primera sección es «no», la reorganización probablemente no se justifica.

### Motivación

- [ ] Hay un síntoma **medible**, no una incomodidad estética
- [ ] Está identificado qué indicador debería moverse si el cambio funciona
- [ ] **⚠** El síntoma apunta al remedio correcto: un build lento no se arregla con microservicios; un cambio que toca cinco carpetas apunta a la organización interna, no al despliegue
- [ ] Se estimó el costo del cambio y se comparó con el costo de convivir con el problema

### Viabilidad de una partición en servicios

- [ ] Los datos de la parte a extraer se pueden separar **sin** transacciones distribuidas
- [ ] La parte a extraer tiene una razón de cambio distinta del resto
- [ ] Hay un perfil de escalado, de disponibilidad o de equipo que lo justifique
- [ ] El límite propuesto corresponde a un *bounded context* real y no a una capa técnica
- [ ] **⚠** Tras la partición, cada servicio se puede desplegar sin coordinar con los otros — si no, el resultado es un monolito distribuido
- [ ] Existe capacidad operativa para observabilidad distribuida y versionado de contratos

### Alternativas descartadas

- [ ] Se evaluó modularizar sin partir, y se registró por qué no alcanza
- [ ] Se evaluó extraer de forma incremental antes que reescribir

---

## Lista 3 — Normalizar código existente (`ESC-3`)

### Preparación

- [ ] **⚠** La regla queda **activada** en `.editorconfig` en el mismo commit que la normalización — sin esto, el trabajo se repite en seis meses
- [ ] La severidad elegida es coherente con la política del repositorio
- [ ] Las ramas abiertas están identificadas y sus responsables avisados del conflicto que viene

### Ejecución

- [ ] Los commits de normalización **no contienen ningún cambio funcional**
- [ ] Primero lo mecánico y verificable por herramienta (formato, `using`, modificadores); después lo que requiere criterio (renombres, movimientos)
- [ ] Cada tipo de cambio va en su propio commit, no todos mezclados
- [ ] **⚠** Los hashes se registran en `.git-blame-ignore-revs` y el repositorio está configurado con `blame.ignoreRevsFile`
- [ ] Las pruebas pasan antes y después, sin cambios en ellas

### Verificación

- [ ] El build reproduce el mismo resultado funcional
- [ ] No quedaron supresiones (`#pragma warning disable`, `[SuppressMessage]`) agregadas para esquivar la regla en lugar de cumplirla

---

## Lista 4 — Evaluar código ajeno (`ESC-4`)

### Estructura

- [ ] La estructura declarada en la documentación coincide con la real en disco
- [ ] **⚠** Las dependencias entre capas van en una sola dirección; no hay ciclos
- [ ] Si hay capas en proyectos separados, el `ProjectReference` hace cumplir la dirección
- [ ] Si hay capas en carpetas, existe algún mecanismo que las haga cumplir, o se asume conscientemente que depende de la revisión
- [ ] El proyecto de dominio no referencia paquetes de infraestructura
- [ ] Los espacios de nombres coinciden con las carpetas

### Convenciones

- [ ] Las convenciones están **automatizadas** y no dependen de que alguien las recuerde
- [ ] Hay un solo lugar donde se declaran las versiones de paquetes
- [ ] El estilo es uniforme — **⚠** la uniformidad pesa más que la elección concreta; tres estilos excelentes en distintas partes son peor que uno mediocre aplicado en todas
- [ ] Los nombres usan un vocabulario único: no conviven `Obtener`, `Traer` y `Buscar` para la misma operación
- [ ] No hay clases `Manager`, `Helper` ni `Utils` que oculten responsabilidades no identificadas

### Juicio

- [ ] **⚠** Lo que se señala es una inconsistencia real, no una convención distinta de la propia
- [ ] Se buscó si la desviación está justificada en un ADR o comentario antes de señalarla
- [ ] Las observaciones separan hecho de interpretación
- [ ] Lo que excede lo acordado se propone, no se exige

---

## Lista 5 — Publicar o mantener una biblioteca (`CTX-3`)

Aplica solo a `CTX-3`, y su rigor es mayor porque los errores son ruptores.

### Superficie pública

- [ ] Todo tipo y miembro `public` está ahí a propósito; lo demás es `internal`
- [ ] **⚠** Los nombres siguen `N-01` a `N-04` de forma literal — acá sí aplican en su sentido original
- [ ] Los acrónimos de tres o más letras van en PascalCase (`XmlDocument`, no `XMLDocument`)
- [ ] Las interfaces llevan prefijo `I` (`N-01`); los métodos asincrónicos, sufijo `Async` (`F-04`, `N-15` — convención del TAP, no de las Framework Design Guidelines)
- [ ] No hay campos públicos: son propiedades
- [ ] El espacio de nombres sigue `<Empresa>.<Producto>[.<Funcionalidad>]`

### Contrato y versionado

- [ ] `GenerateDocumentationFile` está activado y los miembros públicos están documentados
- [ ] La versión sigue versionado semántico y un cambio ruptor implica versión mayor
- [ ] Los miembros a retirar están marcados `[Obsolete]` con el reemplazo indicado en el mensaje
- [ ] **⚠** Un miembro obsoleto sobrevive al menos una versión menor antes de eliminarse
- [ ] `TreatWarningsAsErrors` está activado: un aviso de API mal formada se publica

### Empaquetado

- [ ] `PackageId`, `Description`, `Authors` y licencia están completos
- [ ] Los paquetes de tiempo de diseño llevan `PrivateAssets=all` y no se filtran al consumidor
- [ ] Si hay multi-target, cada destino compila y las diferencias de API están resueltas

---

## Cómo usar estas listas en una revisión automatizada

Los ítems están redactados para poder verificarse por inspección de archivos, lo que los hace aptos para un agente. Un recorrido mínimo sobre un repositorio .NET:

```bash
# Andamiaje presente
ls -1 global.json Directory.Build.props Directory.Packages.props .editorconfig 2>&1

# Proyectos en disco vs en la solución
find . -name "*.csproj" | sort
grep -o 'Path="[^"]*"' *.slnx 2>/dev/null | sort

# Versiones de paquete declaradas fuera del archivo central
grep -rn 'PackageReference.*Version=' --include=*.csproj .

# Dirección de las dependencias entre proyectos
grep -rn 'ProjectReference' --include=*.csproj .
```

La última consulta es la que más rinde en una evaluación: el grafo de `ProjectReference` revela la arquitectura real, que no siempre coincide con la declarada.
