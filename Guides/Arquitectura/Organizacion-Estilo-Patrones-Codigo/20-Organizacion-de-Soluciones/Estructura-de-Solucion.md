---
doc_id: TEM-SLN
doc_type: tema
title: Estructura de solución
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-SOL, TEM-SDK, TEM-BUILD, TEM-NS, TEM-CVP, FAM-SRV, MARCO-ESCENARIOS, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Estructura de solución — `TEM-SLN`

## Resumen ejecutivo

Un repositorio .NET tiene una forma reconocible: un archivo de solución en la raíz, una carpeta `src/` con los proyectos del producto, los tests en algún lugar separado de ellos, y un puñado de archivos de configuración compartida. Esa forma se repite en miles de repositorios y casi nadie la eligió: se heredó. Conocer de dónde viene cada pieza permite saber cuáles son negociables y cuáles no —y la respuesta sorprende, porque una de las piezas que se citan como convención firme no lo es.

La distinción que ordena todo el documento es entre lo que el compilador necesita y lo que las personas necesitan. Los proyectos son reales: producen ensamblados, tienen dependencias verificadas por el compilador, definen espacios de nombres. El archivo de solución es un artefacto de herramienta: agrupa proyectos para que un IDE los abra juntos y para que `dotnet build` los compile de una pasada. Confundir ambos planos produce el error más frecuente de esta familia, que es leer el número de proyectos de una solución como si dijera algo sobre la arquitectura del sistema.

Le sirve principalmente a `ACT-01`, que decide la composición en `ESC-1` y la revisa en `ESC-2`, y a `ACT-04` cuando evalúa un repositorio ajeno en `ESC-4` y necesita saber qué es convención y qué es decisión.

---

## Definición

### Qué es

La estructura de solución es la disposición física y lógica de un repositorio .NET: qué proyectos contiene, cómo se agrupan en carpetas de disco, cuáles están declarados en el archivo de solución y cómo se llaman.

Tiene dos planos que no coinciden necesariamente. El plano de disco lo determinan las carpetas reales y las rutas de los `.csproj`. El plano lógico lo determina el archivo de solución, que declara qué proyectos forman parte del conjunto y en qué carpetas virtuales se muestran. Ninguno de los dos planos es más verdadero que el otro; lo que importa es si se corresponden, porque cada desalineación es una trampa para quien llegue después.

### Qué problema resuelve

El de operar sobre varios proyectos a la vez. Sin archivo de solución, cada `dotnet build`, cada `dotnet test` y cada apertura de IDE apunta a un `.csproj` individual. Con archivo de solución, un solo comando en la raíz alcanza a todo el conjunto y la canalización de integración continua se escribe una vez en lugar de enumerarse proyecto por proyecto.

El segundo problema que resuelve es de navegación. Un repositorio con veinte proyectos sin agrupación es una lista plana donde cuesta distinguir producto de prueba de herramienta. Las carpetas de solución dan esa agrupación sin obligar a mover archivos en disco.

### Qué NO es, y con qué se confunde

**No es una unidad de despliegue.** Una solución puede compilar a un único ejecutable, a siete contenedores independientes o a ninguno de los dos si solo contiene bibliotecas. Lo que se despliega son los artefactos publicados de determinados proyectos, y qué proyectos son eso lo decide la topología de servicios, no el `.sln`. La discusión sobre unidades desplegables está en [`FAM-SRV`](../10-Arquitectura-de-Servicios/README.md).

**No es una decisión de arquitectura.** Agregar un proyecto a una solución no crea un límite arquitectónico; lo que crea un límite es la dirección de las referencias entre proyectos, y eso se puede lograr también con carpetas y analizadores dentro de un único proyecto ([`TEM-CVP`](../30-Organizacion-Interna/Carpetas-o-Proyectos.md)). Un sistema con cuatro capas declaradas en carpetas dentro de un único `.csproj` y un sistema con las mismas cuatro capas en cuatro proyectos pueden tener la misma arquitectura; lo que cambia entre ambos es quién verifica las reglas de dependencia, el compilador o la disciplina.

**No es el repositorio.** Un repositorio puede contener varias soluciones, y una solución puede referenciar proyectos que viven fuera de su carpeta. La ecuación «un repositorio, una solución» es lo habitual, no lo obligatorio.

**No es un contrato de compilación.** Un proyecto que existe en disco pero no figura en el `.slnx` compila perfectamente si alguien lo invoca por ruta. La solución determina qué compila `dotnet build` en la raíz, no qué es compilable.

---

## El archivo de solución: `.sln` y `.slnx`

### El formato clásico

El `.sln` es un archivo de texto con una sintaxis propia —no XML, no JSON— nacida en Visual Studio y estable durante dos décadas. Declara cada proyecto con un GUID de tipo y un GUID de identidad, las carpetas de solución como si fueran proyectos de un tipo especial, y una sección de configuraciones que asigna a cada proyecto una combinación de configuración y plataforma para cada configuración de la solución.

Su defecto práctico es el conflicto de fusión. Los GUID no aportan información legible, la sección de configuraciones crece de forma cuadrática con proyectos por configuraciones, y dos ramas que agregan un proyecto cada una producen un conflicto que hay que resolver a mano sin poder leer qué significa cada línea.

### El formato XML

`.slnx` es el formato XML de archivo de solución. Es legible, no usa GUID para identificar proyectos y omite la sección de configuraciones cuando los valores predeterminados alcanzan, que es la mayoría de los casos. Un conflicto de fusión en un `.slnx` se resuelve leyendo.

Tres datos verificables sobre su estado, todos de `N-11`:

- El soporte del formato en la CLI de .NET existe **desde el SDK 9.0.200**.
- Existe el comando **`dotnet sln migrate`**, que produce un `.slnx` a partir de un `.sln` existente.
- En **.NET 10, `dotnet new sln` crea un `.slnx` de forma predeterminada**.

Que el comando de creación de plantilla haya cambiado su formato predeterminado es la señal más fuerte disponible sobre la dirección del ecosistema: el formato nuevo dejó de ser opcional para convertirse en el camino por omisión. Sobre la disponibilidad en versiones concretas de Visual Studio, esta guía no afirma nada: el dato circula en material de divulgación y no se verificó en fuente primaria.

Migrar es barato y reversible —el `.sln` original queda en el historial de Git— pero no es gratis para el equipo: toda herramienta de la cadena que lea el archivo de solución tiene que soportarlo. Esta guía recomienda verificar primero el soporte en la canalización de integración continua y en cualquier herramienta de análisis que consuma el archivo de solución, y recién después ejecutar la migración.

---

## Disposición de carpetas

### `src/`, `eng/` y la carpeta de tests

Acá conviven una convención sólida y otra que se cita como si lo fuera y no lo es. Separarlas es el aporte principal de esta sección.

Microsoft **usa** la disposición en material oficial y **no la prescribe** en ninguna especificación (`F-01`). El tutorial de organización y prueba de proyectos con la CLI usa `/src` y `/test`, y lo enmarca explícitamente como recomendación con alternativa: reconoce que hay desarrolladores que prefieren mantener ambos proyectos en la misma carpeta. La documentación de personalización del build por carpeta (`N-09`) escribe *«Suppose you have this standard solution structure»* con `\src` y `\test`, pero como supuesto de ejemplo para explicar el encadenamiento de `Directory.Build.props`, no como prescripción.

La práctica es más elocuente que la documentación, y desmiente la versión difundida. Los tres repositorios de referencia de Microsoft, consultados el 2026-07-19 sobre rama `main`:

| | `src/` | `eng/` | Ubicación de los tests | Singular o plural |
|---|:---:|:---:|---|---|
| `dotnet/runtime` | sí | sí | Anidados por componente: `src/libraries/System.Text.Json/tests/` | `tests` |
| `dotnet/aspnetcore` | sí | sí | Anidados por componente: `src/Mvc/test/` | `test` |
| `dotnet/efcore` | sí | sí | En la raíz, hermana de `src/` | `test` |

**Lo que los tres respaldan**: el código productivo vive bajo `src/`, la infraestructura de build vive bajo `eng/` (`F-07`), y los tests están separados del código productivo. Esas tres afirmaciones son convención de facto con evidencia unánime.

**Lo que ninguna de las tres respalda**: que la carpeta de tests viva en la raíz —dos de tres la anidan por componente— ni que se escriba en plural —dos de tres usan singular—. La fórmula «`src/` + `tests/` en la raíz» circula como convención heredada y no lo es. Es una decisión de equipo perfectamente razonable, y conviene tomarla sabiendo que se está decidiendo y no aplicando un estándar.

El criterio que separa ambas opciones es el tamaño y el acoplamiento. Anidar los tests junto al componente que prueban, como hacen `runtime` y `aspnetcore`, mantiene la distancia corta entre código y prueba en repositorios con decenas de componentes independientes; el costo es que la raíz no muestra dónde están las pruebas y que un `Directory.Build.props` para todos los proyectos de test necesita otro mecanismo de alcance. Ponerlos en la raíz, como hace `efcore`, permite exactamente lo contrario: un `Directory.Build.props` colocado en esa carpeta alcanza a todos los proyectos de prueba y a ninguno de producto, sin enumerarlos, y quien clona el repositorio distingue producto de verificación en el primer `ls`. Esta guía recomienda la disposición en la raíz para repositorios de un solo producto —que son la mayoría de los casos de `CTX-1` y `CTX-2`— y reserva el anidamiento para repositorios con muchos componentes de ciclo de vida propio.

Sobre el plural no hay nada que decidir con argumentos: elegir uno y aplicarlo en todo el repositorio vale más que cuál se elija.

### Las otras carpetas de convención

Cinco nombres aparecen con suficiente frecuencia como para ser reconocibles sin explicación. Ninguno está especificado.

`perf/` para proyectos de medición de rendimiento, típicamente arneses de BenchmarkDotNet. Se separa de `tests/` porque un benchmark no es una prueba: no afirma nada, tarda minutos u horas y no participa de las compuertas de cobertura.

`samples/` para aplicaciones de ejemplo que consumen la biblioteca del repositorio. Es propio de `CTX-3` y casi no aparece fuera de ese contexto. Su función real es doble: documenta el uso previsto y funciona como prueba de que la API pública es usable, que es algo que las pruebas unitarias internas no verifican porque acceden desde adentro.

`docs/` para documentación versionada junto al código, incluida la carpeta `docs/adr/` de registros de decisión de arquitectura.

`eng/`, `build/` y `tools/` para lo que sostiene la construcción: scripts de canalización, plantillas de MSBuild importadas, utilidades de generación. De los tres, `eng/` es el único con respaldo fuerte —los tres repositorios de referencia lo usan (`F-07`)—; la separación entre `build/` y `tools/` no es estable en el ecosistema y no vale la pena discutirla. Lo que vale es que ese material no esté suelto en la raíz.

### Carpetas de solución y carpetas de disco

Un `.slnx` declara carpetas de solución que son puramente visuales, y un proyecto puede aparecer en cualquiera de ellas con independencia de dónde esté en disco. Nada obliga a que coincidan.

Esa libertad es útil en un caso y dañina en el resto. Es útil para agrupar en la vista del IDE archivos sueltos de la raíz —`Directory.Build.props`, `global.json`, `.editorconfig`— bajo una carpeta virtual `Solution Items`, que no tiene ninguna contrapartida en disco y no debería tenerla. Es dañina en cuanto se aplica a proyectos: cuando un proyecto que vive en `src/Facturacion/` aparece en la solución bajo una carpeta `Ventas`, todo desarrollador que busque el archivo desde el explorador de archivos lo busca donde no está, y toda ruta relativa escrita en un script apunta a un lugar distinto del que el IDE muestra.

Esta guía recomienda que las carpetas de solución que contienen proyectos repliquen exactamente la jerarquía de disco, y que las carpetas virtuales sin correlato se reserven para archivos que están sueltos en la raíz por obligación de MSBuild.

---

## Nombrado de proyectos

El nombre del proyecto no es una etiqueta: por comportamiento predeterminado del SDK determina el nombre del ensamblado y el espacio de nombres raíz (`F-05`). `MiEmpresa.Reservas.Application.csproj` produce `MiEmpresa.Reservas.Application.dll` y hace que las clases nuevas nazcan en el espacio de nombres `MiEmpresa.Reservas.Application`. Renombrar un proyecto cambia las tres cosas a la vez, y en `CTX-3` cambia además el identificador del paquete NuGet, que es un cambio ruptor para todo consumidor.

La convención dominante es la segmentación por puntos con un prefijo común de producto u organización: `MiEmpresa.Reservas.Domain`, `MiEmpresa.Reservas.Web`. El prefijo evita colisiones de ensamblado cuando el binario convive con los de otros orígenes, y en `CTX-3` es además lo que reserva el espacio de nombres del paquete.

Dos observaciones sobre segmentos. El segmento final debería nombrar la responsabilidad, no la tecnología: `MiEmpresa.Reservas.Notificaciones` sobrevive a un cambio de proveedor de correo, `MiEmpresa.Reservas.SendGrid` no. Y el segmento de prueba se agrega al nombre completo del proyecto probado —`MiEmpresa.Reservas.Domain.Tests`— de modo que la correspondencia sea evidente en una lista ordenada alfabéticamente.

Las reglas de composición de espacios de nombres, incluida la relación entre el espacio de nombres raíz y la jerarquía de carpetas, se desarrollan en [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md).

---

## Proyectos fuera de la solución

Un proyecto que existe en disco y no está declarado en el archivo de solución sigue siendo un proyecto válido. Compila si se lo invoca por ruta, hereda `Directory.Build.props` como cualquier otro y puede referenciar proyectos de la solución. Lo único que pierde es la inclusión en las operaciones que operan sobre la solución completa.

El motivo habitual para dejar un proyecto afuera es de alcance de compuerta: que `dotnet test` sobre la solución —que suele ser la compuerta de pruebas unitarias con cobertura— no arrastre proyectos cuyos requisitos son incompatibles con ella. Los dos candidatos típicos son las pruebas de extremo a extremo, que necesitan binarios de navegador instalados y un host levantado, y los arneses de benchmark, que son aplicaciones de consola que no afirman nada y solo miden con optimizaciones habilitadas. Ninguno de los dos debería correr en cada commit, y ninguno de los dos aporta cobertura.

**El compromiso.** A favor: la compuerta de la solución queda rápida y con un alcance nítido, y nadie tiene que recordar excluir proyectos con filtros en cada invocación. En contra: `dotnet build` en la raíz **no compila esos proyectos**, de modo que un cambio ruptor en el servicio puede romperlos sin que nadie lo note hasta que alguien los invoca. Es una compuerta de compilación que se pierde.

**Qué hay que garantizar para elegirlo conscientemente.** Que la canalización compile o ejecute esos proyectos en algún job, aunque sea aparte; sin eso, la exclusión no es una decisión de alcance sino un agujero de verificación. Que la exclusión esté documentada en el propio `.csproj`, porque es el único lugar donde la va a leer quien se pregunte por qué su proyecto no aparece en el IDE. Y que la alternativa se haya evaluado: `dotnet test --filter` o la propiedad `IsTestProject` permiten en muchos casos mantener el proyecto dentro de la solución y excluirlo solo de la ejecución.

---

## Monorepo o repositorio por servicio

Cuando hay más de una unidad desplegable aparece una decisión que precede a todo lo anterior: si viven en el mismo repositorio o en repositorios separados. La elección afecta la estructura de solución de forma directa —un monorepo suele tener una solución por servicio más una que abarca todo, o ninguna que abarque todo— pero la decisión no se toma con criterios de estructura sino de despliegue, versionado y organización de equipos. Se trata en [`FAM-SRV`](../10-Arquitectura-de-Servicios/README.md) y en [`TEM-PART`](../10-Arquitectura-de-Servicios/Criterios-de-Particion.md).

Lo único que corresponde señalar acá es una consecuencia estructural que suele descubrirse tarde. En un monorepo, `Directory.Build.props` y `Directory.Packages.props` en la raíz alcanzan a todos los servicios, lo que fuerza una versión única de cada paquete en todo el repositorio. Eso es una ventaja de consistencia y una restricción de autonomía a la vez, y quien elige monorepo la está eligiendo sin saberlo si no leyó [`TEM-BUILD`](Build-Compartido.md).

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

La decisión concreta es cuántos proyectos crear el primer día, y la respuesta de esta guía es: los menos que permitan avanzar. Un proyecto de producto y uno de pruebas cubren la mayoría de los arranques, y las capas se declaran en carpetas hasta que aparezca una razón para separarlas.

Lo que sí conviene fijar el primer día, porque después cuesta: el prefijo de nombrado de proyectos, la disposición de carpetas —`src/` para el producto y una carpeta de tests en la raíz, con el plural decidido de una vez— y el formato de solución. Crear el `.slnx` desde el arranque en .NET 10 no requiere ninguna acción especial, ya que es el predeterminado de `dotnet new sln` (`N-11`).

Por contexto. En `CTX-3` el nombre del proyecto es contrato desde el primer commit, porque será el identificador del paquete; conviene verificar disponibilidad del prefijo antes de escribir código. En `CTX-4` la decisión de monorepo se toma acá y es de las caras de revertir. En `CTX-1` y `CTX-2` la estructura mínima alcanza casi siempre.

### `ESC-2` — Evolución estructural

El disparador legítimo para partir un proyecto en dos es una dependencia que hay que impedir y que hoy nada impide. Si `MiEmpresa.Reservas.Domain` no debe conocer EF Core y todo vive en un solo proyecto, extraerlo convierte una regla que depende de disciplina en un error de compilación.

Los disparadores ilegítimos son dos y se repiten. «El proyecto tiene muchos archivos» se resuelve con carpetas. «El build es lento» empeora casi siempre al agregar proyectos, porque cada proyecto adicional agrega su propia sobrecarga de restauración y resolución de referencias.

Agregar un proyecto es barato en el momento y caro en régimen: cada uno agrega un `.csproj` que mantener, una entrada en la solución y un nodo más en el grafo de dependencias que alguien tendrá que entender.

### `ESC-3` — Normalización de código existente

Es el escenario donde las intervenciones de este documento son más seguras, porque casi ninguna toca código. Migrar de `.sln` a `.slnx` con `dotnet sln migrate`, mover proyectos a `src/` y `tests/`, alinear carpetas de solución con carpetas de disco: todo eso cambia rutas y archivos de configuración, no comportamiento.

La única precaución seria es la que aplica a toda normalización estructural. Mover un proyecto de carpeta rompe toda ruta relativa que lo mencione: referencias de proyecto, scripts de canalización, `Dockerfile`, rutas en la configuración de cobertura. Conviene hacerlo en un commit dedicado, buscar el nombre de la carpeta vieja en todo el repositorio antes de dar por cerrado el cambio, y registrar el commit en `.git-blame-ignore-revs` si arrastró reformateos.

### `ESC-4` — Evaluación de código ajeno

Cinco observaciones se pueden hacer sin conocer el contexto del equipo. ¿Coinciden las carpetas de solución con las de disco? ¿Hay `.csproj` en disco que no están en la solución, y está documentada la razón? ¿El nombre de cada proyecto permite anticipar qué contiene? ¿La cantidad de proyectos guarda relación con la cantidad de límites reales, o hay proyectos con tres clases? ¿La disposición de carpetas es uniforme, o conviven `src/`, `source/` y proyectos en la raíz?

La última es la que más pesa. Una estructura discutible aplicada de forma uniforme se aprende una vez; tres estructuras conviviendo obligan a aprender el caso particular de cada proyecto.

---

## Ejemplos concretos

### Evidencia — qué hacen los repositorios de referencia

Dos observaciones de la inspección del 2026-07-19 sobre rama `main` valen para este documento.

La primera es sobre el formato de solución. `dotnet/aspnetcore` y `dotnet/efcore` ya migraron a `.slnx` —sus archivos son `AspNetCore.slnx` y `EFCore.slnx`—, lo que agrega un dato de práctica a lo que `N-11` documenta como capacidad de la herramienta. Que dos repositorios de ese tamaño hayan migrado dice algo que la documentación no dice: el formato aguanta soluciones grandes con cadenas de herramientas complejas, que es exactamente la objeción que suele frenar la migración en un repositorio chico.

La segunda es sobre la disposición de carpetas y ya se desarrolló más arriba: `src/` y `eng/` en los tres, tests separados del código productivo en los tres, y desacuerdo completo sobre dónde ponerlos y cómo escribirlos.

### Caso sintético — la solución mínima en formato XML

Reserva de salas, `ESC-1` en `CTX-1`. Ejemplo sintético.

```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/MiEmpresa.Reservas.Web/MiEmpresa.Reservas.Web.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/MiEmpresa.Reservas.Web.Tests/MiEmpresa.Reservas.Web.Tests.csproj" />
  </Folder>
</Solution>
```

Nueve líneas para lo que en formato clásico habría requerido bloques `Project` con dos GUID cada uno, una sección `GlobalSection(SolutionConfigurationPlatforms)` y cuatro líneas de `ProjectConfigurationPlatforms` por proyecto. Las carpetas de solución `/src/` y `/tests/` coinciden con las de disco, que es lo que este documento recomienda.

### Caso sintético — reserva de salas, tres estructuras

La misma funcionalidad bajo tres composiciones. Ninguna es correcta en abstracto.

**Mínima**, para `ESC-1` en `CTX-1`. Dos proyectos, capas en carpetas.

```text
Reservas/
├── Reservas.slnx
├── Directory.Build.props
├── src/MiEmpresa.Reservas.Web/         (Microsoft.NET.Sdk.Web)
│   ├── Domain/  Application/  Infrastructure/  Components/
└── tests/MiEmpresa.Reservas.Web.Tests/ (Microsoft.NET.Sdk)
```

**Con dominio extraído**, para `ESC-2` cuando hace falta impedir por compilación que el dominio conozca la persistencia:

```text
Reservas/
├── src/MiEmpresa.Reservas.Domain/          (sin referencias a infraestructura)
├── src/MiEmpresa.Reservas.Application/     → Domain
├── src/MiEmpresa.Reservas.Infrastructure/  → Domain, Application (EF Core acá)
├── src/MiEmpresa.Reservas.Web/             → todas
└── tests/MiEmpresa.Reservas.Domain.Tests/  → Domain
```

Lo que compra la separación es exactamente una cosa: que un `using Microsoft.EntityFrameworkCore` en el dominio no compile. Si esa garantía no hace falta, los cuatro proyectos son cuatro `.csproj` que mantener a cambio de nada.

**Distribuida**, para `CTX-4`, con carpetas de solución que agrupan por servicio:

```text
Reservas/
├── src/MiEmpresa.Reservas.Servicio/        → publica su contrato
├── src/MiEmpresa.Notificaciones.Servicio/  → consume el contrato, no el proyecto
├── src/MiEmpresa.Reservas.Contracts/       (CTX-3 interno: es contrato compartido)
├── src/MiEmpresa.Reservas.AppHost/         (Aspire.AppHost.Sdk, orquesta el conjunto)
└── tests/…
```

`MiEmpresa.Reservas.Contracts` es la única referencia de proyecto legítima entre los dos servicios. Cualquier otra convierte la solución distribuida en un monolito con más pasos, y el compilador no lo va a impedir: la referencia compila.

---

## Preguntas guía

1. ¿Cuántos proyectos tiene esta solución y cuántos límites de dependencia hacía falta impedir? Si el segundo número es menor, hay proyectos que no compran nada.
2. ¿Este proyecto existe para impedir una dependencia concreta, o para reflejar una capa del diagrama?
3. ¿Las carpetas que muestra el IDE coinciden con las de disco?
4. ¿Hay `.csproj` en disco que no están en la solución? ¿Está documentado por qué, y hay algún job que los compile?
5. Si renombro este proyecto, ¿qué más cambia? En `CTX-3`, ¿el identificador del paquete es un contrato ya publicado?
6. ¿El nombre del proyecto sobrevive a un cambio de proveedor de infraestructura?
7. Si migro a `.slnx`, ¿toda la cadena de herramientas lo soporta —canalización, análisis, generación de artefactos?
8. En `ESC-4`: ¿lo que me molesta de esta estructura es una inconsistencia real, o es que uso otra convención?

---

## Criterios de calidad

Una estructura de solución bien resuelta se reconoce porque alguien que clona el repositorio encuentra lo que busca sin preguntar. El nombre de cada proyecto anticipa su contenido, la ubicación en disco coincide con la que muestra el IDE, y la cantidad de proyectos se explica por límites que hacen falta y no por simetría.

Las propiedades concretas: la raíz tiene pocas entradas y todas legibles; hay una sola convención de disposición aplicada a todo el repositorio; todo proyecto en disco está en la solución o su exclusión está documentada y compensada en la canalización; el grafo de referencias entre proyectos no tiene ciclos y se puede dibujar de memoria.

### Antipatrones

**La solución-espejo.** Un proyecto por capa porque el diagrama tiene cuatro capas, sin que ninguna dependencia necesite impedirse. Produce cuatro `.csproj`, cuatro conjuntos de paquetes que sincronizar y un build más lento, a cambio de una separación que las carpetas ya daban. Se detecta cuando un proyecto tiene menos de diez archivos y ninguna regla de dependencia que justifique su existencia.

**La solución fantasma.** El `.sln` declara proyectos que ya no existen en disco, o carpetas de solución que apuntan a rutas muertas. Es el resultado típico de mover archivos sin actualizar la solución. Rompe la apertura en el IDE y no rompe el build, que es lo que hace que sobreviva tanto tiempo.

**El proyecto huérfano.** Un `.csproj` en disco, fuera de la solución, sin comentario que explique por qué y sin ningún job de canalización que lo compile. Se pudre en silencio: cuando alguien lo necesita, hace dos años que no compila. Lo que separa este antipatrón de una exclusión legítima no es la exclusión sino sus dos acompañantes: el comentario en el propio `.csproj` y el job de canalización que lo compila aparte.

**La carpeta virtual mentirosa.** Carpetas de solución que agrupan proyectos con una jerarquía distinta de la de disco. Cada búsqueda en el explorador de archivos falla y cada script escrito desde la vista del IDE apunta mal.

**La raíz saturada.** Quince proyectos en la raíz del repositorio, sin `src/` ni `tests/`. No rompe nada y hace que cada `Directory.Build.props` tenga que discriminar por nombre de proyecto lo que la jerarquía habría discriminado sola.

**El nombre tecnológico.** `MiEmpresa.Reservas.SqlServer`, `MiEmpresa.Reservas.SendGrid`, `MiEmpresa.Reservas.RabbitMQ`. Cada cambio de proveedor deja un nombre que miente, y renombrar arrastra ensamblado y espacio de nombres.
