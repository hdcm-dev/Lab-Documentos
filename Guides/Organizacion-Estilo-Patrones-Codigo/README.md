---
doc_id: GUIA-INDICE
doc_type: indice
title: Guía de estudio — Organización, estilo y patrones de código en .NET
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [MAPA-CONCEPTUAL, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, MARCO-CONVENCIONES]
---

# Guía de estudio — Organización, estilo y patrones de código en .NET

## Resumen ejecutivo

Cuerpo documental formativo sobre cómo se organiza y se escribe código .NET: desde cuántas unidades desplegables tiene un sistema hasta si una constante va en `PascalCase`. Está pensada para leerse de corrido una vez y consultarse muchas después.

El recorrido va de lo general a lo particular, y de las decisiones caras de revertir a las baratas. Primero el **marco de referencia**, que fija cuatro escenarios, cuatro contextos y seis actores, y se convierte en el vocabulario común de toda la guía. Después el **mapa conceptual**, con tres tablas de entrada que responden «estoy acá, qué aplico». Después las **seis familias temáticas**, ordenadas por costo de reversión. Al final, los **anexos**: plantillas listas para copiar, listas de verificación, glosario y el registro de fuentes.

Hay un criterio que atraviesa toda la guía y que la distingue del material corriente sobre el tema: **cada afirmación normativa declara su nivel de autoridad**. Lo que Microsoft especifica se cita con su fuente; lo que es convención del ecosistema se declara como tal; lo que es criterio de esta guía se marca con la fórmula «esta guía recomienda». La confusión entre esos tres niveles es endémica —Clean Architecture se presenta como estándar de Microsoft con enorme frecuencia, y no lo es— y le impide al lector evaluar si algo le conviene o solo es popular.

---

## Cómo usar esta guía

Tres entradas según el motivo de la consulta.

**Estudiar el tema completo.** El orden de los directorios es el orden de lectura. La [ruta sugerida](01-Mapa-Conceptual/Mapa-Conceptual.md#ruta-de-lectura-sugerida) la desglosa en cinco tramos, cada uno cerrando con algo que se puede decidir.

**Resolver algo concreto.** Ir directo a la [tabla de entrada por decisión](01-Mapa-Conceptual/Mapa-Conceptual.md#tabla-de-entrada-por-decisión-concreta) del mapa conceptual: unas treinta preguntas frecuentes con el documento que las trata. No hace falta el resto.

**Evaluar código ajeno.** [Listas de verificación](99-Anexos/Listas-de-Verificacion.md), y dentro de cada documento la sección de criterios de calidad, que es donde vive la distinción entre lo aceptable y lo que solo lo parece.

---

## La distinción que conviene tener antes de empezar

**Cómo se despliega un sistema y cómo se organiza su código son decisiones independientes.** Un monolito puede tener el código impecablemente modularizado; un conjunto de microservicios puede ser un desastre interno. Buena parte de la literatura mezcla ambos ejes, y de ahí sale la creencia de que partir en servicios «ordena» un sistema. No lo hace: un sistema desordenado que se parte en servicios se convierte en un sistema desordenado con latencia de red y fallas parciales.

La guía separa esos dos planos desde su estructura: [`FAM-SRV`](10-Arquitectura-de-Servicios/README.md) trata el despliegue, [`FAM-INT`](30-Organizacion-Interna/README.md) trata el código, y se leen por separado.

---

## Tabla de contenido

### Marco de referencia

El vocabulario común. Nada del resto de la guía se entiende sin esto.

| Documento | ID | Qué fija |
|-----------|----|----------|
| [Escenarios](00-Marco-de-Referencia/Escenarios.md) | `MARCO-ESCENARIOS` | Las cuatro situaciones: sistema nuevo, evolución estructural, normalización, evaluación |
| [Contextos](00-Marco-de-Referencia/Contextos.md) | `MARCO-CONTEXTOS` | Los cuatro entornos: web y cliente, servicio y API, biblioteca reutilizable, solución distribuida |
| [Actores](00-Marco-de-Referencia/Actores.md) | `MARCO-ACTORES` | Los seis roles, su autoridad y la matriz de responsabilidad |
| [Convenciones](00-Marco-de-Referencia/Convenciones.md) | `MARCO-CONVENCIONES` | Identificadores, frontmatter, estructura y los tres niveles de autoridad |

### Mapa conceptual

| Documento | ID | Qué resuelve |
|-----------|----|--------------|
| [Mapa conceptual](01-Mapa-Conceptual/Mapa-Conceptual.md) | `MAPA-CONCEPTUAL` | Tres tablas de entrada —por escenario, por contexto y por decisión— más los cruces escenario × familia y actor × familia |

### Familia 1 — Arquitectura de servicios · ¿Cuántas unidades desplegables?

[Índice de la familia](10-Arquitectura-de-Servicios/README.md) · `FAM-SRV` · *La decisión más cara de revertir*

| Documento | ID | Propósito |
|-----------|----|-----------|
| [Monolito](10-Arquitectura-de-Servicios/Monolito.md) | `TEM-MONO` | Una unidad desplegable como decisión legítima por defecto, no como fracaso |
| [Monolito modular](10-Arquitectura-de-Servicios/Monolito-Modular.md) | `TEM-MODU` | Módulos con límites explícitos dentro de una unidad desplegable |
| [Microservicios](10-Arquitectura-de-Servicios/Microservicios.md) | `TEM-MICRO` | Qué problemas resuelve de verdad, cuáles no, y el monolito distribuido |
| [Criterios de partición](10-Arquitectura-de-Servicios/Criterios-de-Particion.md) | `TEM-PART` | Cuándo partir y cuándo no, con criterios verificables |

### Familia 2 — Organización de soluciones · ¿Cuántos proyectos y cómo se agrupan?

[Índice de la familia](20-Organizacion-de-Soluciones/README.md) · `FAM-SOL`

| Documento | ID | Propósito |
|-----------|----|-----------|
| [Topologías de solución](20-Organizacion-de-Soluciones/Topologias-de-Solucion.md) | `TEM-TOPO` | Las cinco disposiciones de proyectos, cuándo aparece el proyecto de contratos y por qué proyectos y procesos son ejes independientes |
| [Estructura de solución](20-Organizacion-de-Soluciones/Estructura-de-Solucion.md) | `TEM-SLN` | Anatomía del repositorio, `.sln` frente a `.slnx`, la convención `src/tests` |
| [Tipos de proyecto](20-Organizacion-de-Soluciones/Tipos-de-Proyecto.md) | `TEM-SDK` | El atributo `Sdk` como definición real de un tipo de proyecto |
| [Build compartido](20-Organizacion-de-Soluciones/Build-Compartido.md) | `TEM-BUILD` | `Directory.Build.props`, versiones centralizadas, `global.json` |

### Familia 3 — Organización interna · ¿Cómo se reparte el código adentro?

[Índice de la familia](30-Organizacion-Interna/README.md) · `FAM-INT`

| Documento | ID | Propósito |
|-----------|----|-----------|
| [Modelos de capas](30-Organizacion-Interna/Modelos-de-Capas.md) | `TEM-CAPAS` | N capas, Hexagonal, Onion y Clean: origen, diferencias reales y por qué ninguna es estándar de Microsoft |
| [Vertical Slice](30-Organizacion-Interna/Vertical-Slice.md) | `TEM-SLICE` | Organizar por funcionalidad en lugar de por capa técnica |
| [Carpetas o proyectos](30-Organizacion-Interna/Carpetas-o-Proyectos.md) | `TEM-CVP` | La decisión práctica más frecuente, y la única diferencia que importa |
| [Espacios de nombres](30-Organizacion-Interna/Espacios-de-Nombres.md) | `TEM-NS` | Convención, relación con las carpetas, `RootNamespace`, `global using` |
| [Modelos, DTOs y contratos](30-Organizacion-Interna/Modelos-y-Contratos.md) | `TEM-MODELOS` | Las cuatro representaciones de un concepto, dónde vive cada una y la decisión de idioma por planos |

### Familia 4 — Nomenclatura · ¿Cómo se llaman las cosas?

[Índice de la familia](40-Nomenclatura/README.md) · `FAM-NOM` · *La familia con mayor densidad normativa*

| Documento | ID | Propósito |
|-----------|----|-----------|
| [Capitalización](40-Nomenclatura/Capitalizacion.md) | `TEM-CAPS` | PascalCase, camelCase y compañía: cómo se llama cada convención y dónde aplica |
| [Nombrado de tipos y miembros](40-Nomenclatura/Nombrado-de-Tipos-y-Miembros.md) | `TEM-NOMB` | Qué palabras elegir, sufijos convencionales, tests, español o inglés |
| [Antipatrones de nombrado](40-Nomenclatura/Antipatrones-de-Nombrado.md) | `TEM-ANTI` | Notación húngara, `Manager`, `Helper`, abreviaturas y nombres que mienten |

### Familia 5 — Estilo de codificación · ¿Cómo se dispone el texto?

[Índice de la familia](50-Estilo-de-Codificacion/README.md) · `FAM-EST` · *Máxima discusión, mínimo valor: automatizar y no debatir*

| Documento | ID | Propósito |
|-----------|----|-----------|
| [Formato y llaves](50-Estilo-de-Codificacion/Formato-y-Llaves.md) | `TEM-FORMATO` | Allman, K&R, 1TBS: los estilos con nombre propio y cuál usa C# |
| [Convenciones de lenguaje](50-Estilo-de-Codificacion/Convenciones-de-Lenguaje.md) | `TEM-LENG` | `var`, `record`, anulables, `async`, sentencias de nivel superior |
| [Automatización del estilo](50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md) | `TEM-AUTO` | `.editorconfig`, analizadores, severidades y cómo normalizar sin romper `git blame` |

### Familia 6 — Patrones de código · ¿Qué formas estructurales se usan?

[Índice de la familia](60-Patrones-de-Codigo/README.md) · `FAM-PAT`

| Documento | ID | Propósito |
|-----------|----|-----------|
| [Patrones de endpoint](60-Patrones-de-Codigo/Patrones-de-Endpoint.md) | `TEM-ENDP` | Minimal APIs frente a controllers, `MapGroup`, y el paralelo con otros ecosistemas |
| [Patrones de acceso a datos](60-Patrones-de-Codigo/Patrones-de-Acceso-a-Datos.md) | `TEM-DATOS` | Repository sobre EF Core, Unit of Work, CQRS, DTO y migraciones |

### Anexos

No se leen de corrido: se usan.

| Documento | ID | Cuándo se usa |
|-----------|----|---------------|
| [Plantillas comentadas](99-Anexos/Plantillas.md) | `ANEXO-PLANTILLAS` | Al arrancar un repositorio: los diez archivos que se escriben una vez |
| [Listas de verificación](99-Anexos/Listas-de-Verificacion.md) | `ANEXO-CHECK` | Al revisar, evaluar o normalizar |
| [Glosario](99-Anexos/Glosario.md) | `ANEXO-GLOSARIO` | Ante un término, un alias o un par que se confunde |
| [Referencias](99-Anexos/Referencias.md) | `ANEXO-REFERENCIAS` | Para verificar el respaldo de cualquier afirmación normativa |

---

## Alcance y límites

**Qué cubre.** La organización de soluciones y proyectos .NET, la partición en servicios, la organización interna del código, la nomenclatura, el estilo de codificación y los patrones estructurales que impactan en cómo se dispone el código.

**Qué no cubre.** No es un catálogo general de patrones de diseño —para eso están las obras de `ANEXO-REFERENCIAS`—, ni un manual de C#, ni una guía de pruebas, ni documentación de despliegue. Los cruces con esos temas se enlazan sin desarrollarse.

**Tecnologías de referencia.** .NET 10 y C#, con ASP.NET Core y Blazor en render *interactive server* para los ejemplos de aplicación web. Las convenciones son en su mayoría independientes de la versión; donde no lo son, se indica desde qué versión aplican.

**Dominio de los ejemplos.** Un sistema de reserva de salas, el mismo de la [guía hermana de documentación técnica](../Documentacion-Tecnica/README.md), para poder comparar el mismo problema desde artefactos distintos. Los ejemplos son sintéticos y se declaran como tales.

**Evidencia de la práctica real.** Donde hace falta mostrar qué hace el ecosistema y no solo qué prescribe la documentación, la guía inspecciona tres repositorios públicos de Microsoft —`dotnet/runtime`, `dotnet/aspnetcore` y `dotnet/efcore`—, elegidos porque difieren entre sí lo suficiente como para revelar dónde hay convención y dónde solo hay costumbre. Esa inspección corrigió más de una afirmación que la guía daba por sentada.

---

## Sobre la evidencia

Las fuentes están registradas en [`ANEXO-REFERENCIAS`](99-Anexos/Referencias.md) con su URL y fecha de verificación, clasificadas en tres niveles: normativo de Microsoft (`N-xx`), convención de facto (`F-xx`) y obras de referencia (`O-xx`). Los documentos citan por ese identificador en lugar de repetir la URL.

Al revisar la guía, toda afirmación normativa debería poder rastrearse hasta una fila de ese anexo. Si no puede, o falta la fuente o es criterio propio mal etiquetado.
