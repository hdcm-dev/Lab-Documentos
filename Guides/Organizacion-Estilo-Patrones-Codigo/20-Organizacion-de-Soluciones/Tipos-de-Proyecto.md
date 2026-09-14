---
doc_id: TEM-SDK
doc_type: tema
title: Tipos de proyecto
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-SOL, TEM-SLN, TEM-BUILD, TEM-CVP, MARCO-ESCENARIOS, MARCO-CONTEXTOS, ANEXO-REFERENCIAS]
---

# Tipos de proyecto — `TEM-SDK`

## Resumen ejecutivo

Cuando alguien dice «creé un proyecto de tipo API web», describe una plantilla de Visual Studio. Lo que efectivamente distingue un proyecto de otro en el `.csproj` es una sola cosa: el valor del atributo `Sdk` en el elemento raíz. Ese atributo determina qué archivos se incluyen automáticamente en la compilación, qué destinos de MSBuild están disponibles, qué paquetes se referencian de forma implícita y qué hace `dotnet publish`.

El catálogo de SDK de proyecto es normativo: Microsoft lo publica y lo mantiene (`N-10`). No admite las ambigüedades de otros temas de esta guía. Lo que sí admite criterio es la elección, y ahí la pregunta útil no es «qué tipo de proyecto es esto» sino «qué comportamiento de build necesito que el SDK me dé por defecto».

Le sirve a `ACT-02` todos los días —cada proyecto nuevo es una elección de SDK— y a `ACT-01` en `ESC-1`, cuando decide si una funcionalidad merece un proyecto de biblioteca propio o es una carpeta dentro de otro.

---

## Definición

### Qué es

El SDK de proyecto es un conjunto de destinos y propiedades de MSBuild que se importa automáticamente al principio y al final de la evaluación del `.csproj`. Se declara en el atributo `Sdk` del elemento raíz:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
```

Esa línea es equivalente a dos importaciones explícitas —una al comienzo y otra al final del archivo— y es lo que permite que un `.csproj` moderno ocupe diez líneas mientras uno del formato anterior ocupaba doscientas. Todo lo que no está escrito lo aporta el SDK.

### Qué problema resuelve

El del `.csproj` como inventario. En el formato anterior a los SDK, cada archivo `.cs` del proyecto figuraba enumerado en el `.csproj`, y agregar un archivo desde fuera del IDE dejaba el proyecto inconsistente. Los SDK invierten la lógica: se incluye todo lo que corresponde al tipo de proyecto y se declara solo lo excluido. Un `.csproj` de estilo SDK describe intenciones, no contenidos.

El segundo problema es el de los valores predeterminados razonables. Un proyecto web necesita el servidor Kestrel, la generación del host, la publicación de `wwwroot` y el modelo de configuración por entorno. Nada de eso se declara: `Microsoft.NET.Sdk.Web` lo trae.

### Qué NO es, y con qué se confunde

**No es el marco de destino.** `TargetFramework` dice para qué versión de .NET se compila; el SDK dice qué clase de proyecto es. Un proyecto `Microsoft.NET.Sdk.Web` puede tener `net10.0` o `net8.0` y sigue siendo web.

**No es la plantilla.** `dotnet new webapi` y `dotnet new blazor` producen proyectos con el mismo SDK y contenido distinto. La plantilla es un punto de partida que se consume una vez; el SDK gobierna cada compilación.

**No es el tipo de salida.** `OutputType` distingue biblioteca de ejecutable, y es independiente del SDK. Un `Microsoft.NET.Sdk` con `OutputType=Exe` es una aplicación de consola perfectamente válida, y es la forma habitual de declarar un arnés de benchmarks.

**No determina la arquitectura.** Elegir `Microsoft.NET.Sdk.Worker` no impone ninguna organización interna. Lo que aporta es el andamiaje del host y de la inyección de dependencias; qué se pone adentro lo decide [`FAM-INT`](../30-Organizacion-Interna/README.md).

---

## El catálogo oficial

Los SDK que Microsoft publica y mantiene (`N-10`). Los tres primeros cubren la gran mayoría de los proyectos que un equipo escribe.

| SDK | Para qué | Qué agrega sobre el anterior |
|-----|----------|------------------------------|
| `Microsoft.NET.Sdk` | Bibliotecas de clases, consola, proyectos de prueba, arneses | El SDK base: compilación de C#, inclusión implícita de `**/*.cs`, restauración de NuGet, empaquetado |
| `Microsoft.NET.Sdk.Web` | Aplicaciones ASP.NET Core, APIs, Blazor Server, Blazor Web App | Framework compartido de ASP.NET Core, publicación de `wwwroot`, perfiles de lanzamiento, destinos de publicación web |
| `Microsoft.NET.Sdk.Razor` | Bibliotecas de componentes Razor (RCL) y componentes compartidos | Compilación de `.razor` y `.cshtml`, empaquetado de activos estáticos de biblioteca |
| `Microsoft.NET.Sdk.BlazorWebAssembly` | Aplicaciones Blazor WebAssembly independientes | Publicación como activos estáticos, recorte del ensamblado, compilación anticipada opcional |
| `Microsoft.NET.Sdk.Worker` | Servicios de fondo y procesos de larga duración | Andamiaje del host genérico, `BackgroundService`, integración con el gestor de servicios del sistema |

Dos SDK adicionales que no son de propósito general y que conviene conocer porque aparecen en soluciones reales:

`Aspire.AppHost.Sdk` para el proyecto de orquestación de .NET Aspire, que declara y compone los recursos de una solución distribuida. Es específico de `CTX-4` y no reemplaza a los anteriores: se usa junto con ellos, en un proyecto dedicado.

`MSTest.Sdk` para proyectos de prueba con MSTest, que preconfigura el corredor y las referencias del framework de pruebas y ahorra declarar los paquetes uno por uno. Es una alternativa a usar `Microsoft.NET.Sdk` con referencias explícitas, no una obligación; los proyectos con xUnit o NUnit siguen el camino explícito.

Un detalle de composición que suele sorprender: `Microsoft.NET.Sdk.Web` ya incluye lo que aporta `Microsoft.NET.Sdk.Razor`. Un proyecto web no necesita declarar el SDK de Razor para compilar componentes; el SDK de Razor existe para las bibliotecas de componentes, que no son aplicaciones web.

---

## Anatomía del `.csproj` de estilo SDK

Las propiedades que aparecen en casi todos los proyectos, y qué hace cada una.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

</Project>
```

**`TargetFramework`.** El marco de destino. Determina qué API están disponibles y qué versión del compilador de C# se usa de forma predeterminada.

**`Nullable`.** Activa el análisis de referencias nulas. Con `enable`, el compilador distingue `string` de `string?` y avisa cuando una referencia posiblemente nula se desreferencia. Es la propiedad de mayor impacto sobre la calidad del código de toda esta lista, y también la más costosa de activar sobre código existente: en `ESC-3` un proyecto grande puede producir cientos de avisos el primer día.

**`ImplicitUsings`.** Inyecta un conjunto de directivas `using` según el SDK —`System`, `System.Linq` y compañía en el SDK base; `Microsoft.AspNetCore.Builder` y otras en el SDK web—. Reduce el ruido de cabecera de cada archivo. Su contrapartida es que un lector no ve de dónde viene un tipo sin conocer el conjunto implícito de ese SDK.

**`IsPackable`.** Controla si `dotnet pack` produce un paquete NuGet. Conviene ponerlo en `false` explícitamente en todo lo que no es un paquete —pruebas, benchmarks, aplicaciones— aunque el valor predeterminado ya sea razonable, porque hace explícita la intención.

**`OutputType`.** `Library` de forma predeterminada, `Exe` para producir un ejecutable. El SDK web y el de worker lo ponen en `Exe` solos.

**`TargetFrameworks`** —en plural— compila el mismo código para varios marcos de destino a la vez:

```xml
<TargetFrameworks>net10.0;net8.0;netstandard2.0</TargetFrameworks>
```

Es casi exclusivo de `CTX-3`. Una biblioteca publicada en NuGet que quiere ser consumible desde aplicaciones que aún no migraron necesita multi-destino; una aplicación interna no lo necesita nunca, porque controla en qué framework corre. El costo es real: el código que difiere entre frameworks requiere compilación condicional con `#if`, la matriz de pruebas se multiplica, y cada marco de destino adicional alarga el build de todos.

---

## Qué elegir en cada contexto

**`CTX-1` — Web o cliente interactivo.** `Microsoft.NET.Sdk.Web` para la aplicación. Si hay componentes que se comparten con otra aplicación, una biblioteca `Microsoft.NET.Sdk.Razor`; si no los hay, los componentes viven en el proyecto web. Blazor WebAssembly independiente usa `Microsoft.NET.Sdk.BlazorWebAssembly`, pero la variante habitual hoy —Blazor Web App con modos de render mixtos— es un proyecto web que puede tener un proyecto cliente asociado.

**`CTX-2` — Servicio o API.** `Microsoft.NET.Sdk.Web` si expone HTTP, `Microsoft.NET.Sdk.Worker` si no lo hace. La frontera se cruza más seguido de lo que parece: un worker que necesita exponer un endpoint de estado de salud para el orquestador termina siendo un proyecto web con un `BackgroundService` adentro, y esa es la forma correcta de resolverlo, no dos procesos.

**`CTX-3` — Biblioteca reutilizable.** `Microsoft.NET.Sdk`, con `IsPackable=true` y los metadatos de paquete completos. Es el único contexto donde `TargetFrameworks` en plural se justifica de entrada, y donde conviene incorporar análisis de compatibilidad de API al build.

**`CTX-4` — Solución distribuida.** Cada servicio elige por su cuenta según `CTX-1` o `CTX-2`, más un proyecto de contratos compartidos —`Microsoft.NET.Sdk`, sin dependencias de infraestructura— y, si se usa Aspire, un proyecto de orquestación con `Aspire.AppHost.Sdk`.

---

## Bibliotecas de clases: cuándo sí y cuándo es ceremonia

Un proyecto de biblioteca se justifica cuando **algo lo consume desde afuera de la solución**, o cuando **hace falta que el compilador impida una dependencia**. Fuera de esos dos casos, lo que se busca casi siempre se consigue con una carpeta.

El primer criterio es objetivo: si el código se publica como paquete NuGet, se comparte con otra solución del mismo repositorio o lo consumen varios servicios de una solución distribuida, es una biblioteca y no hay discusión.

El segundo requiere honestidad. «Quiero que el dominio no dependa de la infraestructura» es una intención; separar en proyectos la convierte en un error de compilación. Pero la pregunta previa es si esa dependencia se estaba produciendo de verdad, o si el equipo la evitaba sin esfuerzo. Extraer un proyecto para prevenir un problema que no ocurre es agregar un `.csproj`, un conjunto de paquetes que sincronizar y un nodo de grafo a cambio de nada. El desarrollo completo del compromiso está en [`TEM-CVP`](../30-Organizacion-Interna/Carpetas-o-Proyectos.md).

Lo que un proyecto de biblioteca cuesta en régimen, y que se subestima al crearlo: sus versiones de paquete hay que mantenerlas alineadas con las del resto —problema que [`TEM-BUILD`](Build-Compartido.md) resuelve—, su superficie `public` se vuelve accesible desde toda la solución aunque el autor pensara que era interna, y cada compilación paga su costo de resolución de referencias.

**Bibliotecas de componentes Razor.** Cuando lo compartido es interfaz —componentes, hojas de estilo, activos estáticos— el proyecto usa `Microsoft.NET.Sdk.Razor`. La RCL empaqueta los activos estáticos de forma que las aplicaciones consumidoras los sirven sin copiarlos, lo que resuelve el problema práctico de compartir un diseño entre dos aplicaciones sin duplicar `wwwroot`. Se justifica cuando hay dos consumidores reales; con uno solo es una carpeta `Components/` en el proyecto web.

---

## Proyectos de prueba

Un proyecto de prueba es un `Microsoft.NET.Sdk` común con tres particularidades: `IsPackable=false`, referencias al framework de pruebas y al corredor, y una referencia de proyecto al código que prueba.

La convención de nombre es el nombre completo del proyecto probado más un sufijo: `MiEmpresa.Reservas.Dominio.Tests`. No está especificada por Microsoft; es práctica dominante del ecosistema. Su beneficio es de ordenamiento: en una lista alfabética, cada proyecto de prueba queda inmediatamente después del que prueba.

La separación por nivel de prueba en proyectos distintos responde a diferencias operativas concretas, no a purismo taxonómico. Cada nivel tiene requisitos de entorno y tiempos incompatibles con los demás:

| Nivel | Sufijo habitual | Qué necesita | Por qué se separa |
|-------|-----------------|--------------|-------------------|
| Unitarias | `.Tests` | Nada externo | Corren en segundos; son la compuerta de cada commit |
| Integración | `.IntegrationTests` | Base de datos, contenedores | Minutos; requieren infraestructura que no siempre está |
| Extremo a extremo | `.E2E` | Navegador, host levantado | Frágiles y lentas; corren en un job propio |
| Rendimiento | `.Benchmarks` | Compilación en Release | No afirman nada; miden. No participan de cobertura |

Mezclar niveles en un proyecto obliga a la compuerta más rápida a esperar por la más lenta, que es exactamente lo que la separación evita.

---

## Ejemplos concretos

### Caso sintético — dos SDK en la misma solución

Reserva de salas, `CTX-1` en `ESC-1`: un único proyecto web con panel Blazor *interactive server* y persistencia con EF Core, más su proyecto de pruebas unitarias. Ejemplo sintético.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.9">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.9" />
  </ItemGroup>

</Project>
```

Cuatro observaciones sobre este archivo. Un solo proyecto contiene el panel Blazor, los servicios de aplicación y el dominio de reservas: `Microsoft.NET.Sdk.Web` alcanza para compilar `.razor` sin declarar el SDK de Razor, según la composición de SDK que documenta `N-10`. `PrivateAssets=all` en las herramientas de diseño de EF Core evita que el paquete fluya como dependencia transitiva al consumidor del ensamblado, que es la forma correcta de declarar una dependencia de tiempo de diseño. `InvariantGlobalization` es una propiedad del comportamiento en ejecución, no del SDK, y ejemplifica cómo el `.csproj` mezcla ambos planos sin distinguirlos visualmente: nada en el archivo separa lo que elige el tipo de proyecto de lo que configura el runtime. Y las versiones están escritas literalmente acá, en lugar de en un lugar central: el hueco que trata [`TEM-BUILD`](Build-Compartido.md).

El proyecto de pruebas unitarias:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\MiEmpresa.Reservas.Web\MiEmpresa.Reservas.Web.csproj" />
  </ItemGroup>

</Project>
```

El elemento `<Using Include="Xunit" />` agrega una directiva implícita propia al conjunto que aporta `ImplicitUsings`. Es el mecanismo correcto para evitar repetir `using Xunit;` en cada archivo de prueba sin recurrir a un archivo de `GlobalUsings.cs`.

Cuando la solución crece hasta incorporar los otros dos niveles de prueba, dos propiedades los distinguen del anterior. El arnés de benchmarks declara `OutputType=Exe` con `IsTestProject=false` —es una aplicación de consola que mide, no un proyecto que el corredor de pruebas deba descubrir—; el proyecto de extremo a extremo declara `IsTestProject=true` y suele quedar fuera del `.slnx` por razones de alcance de compuerta que se explican en [`TEM-SLN`](Estructura-de-Solucion.md).

### Caso sintético — biblioteca publicada

Reserva de salas, `CTX-3`: un cliente HTTP del servicio de reservas empaquetado para consumo interno de la organización.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net10.0;net8.0</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <IsPackable>true</IsPackable>
    <PackageId>MiEmpresa.Reservas.Cliente</PackageId>
    <Description>Cliente HTTP tipado del servicio de reserva de salas.</Description>
    <Authors>MiEmpresa</Authors>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <RepositoryUrl>https://ejemplo.invalid/miempresa/reservas</RepositoryUrl>

    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <EnablePackageValidation>true</EnablePackageValidation>
  </PropertyGroup>

  <!-- PackageReadmeFile nombra el archivo; este ItemGroup es el que lo mete
       dentro del paquete. Sin él, dotnet pack falla con NU5039. -->
  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

Ejemplo sintético. Lo que lo distingue de una biblioteca interna son las dos últimas propiedades. `GenerateDocumentationFile` produce el XML de documentación que el IDE del consumidor muestra al escribir, y activa además los avisos por miembros públicos sin comentar. `EnablePackageValidation` compara la superficie pública contra la versión anterior del paquete y falla el build ante un cambio ruptor, que es la única forma de que la política de versionado semántico de `ACT-06` se verifique sola en lugar de depender de que alguien la recuerde.

El multi-destino `net10.0;net8.0` responde a una situación concreta: consumidores que aún no migraron. Si todos los consumidores están en `net10.0`, sobra, y cada marco de destino adicional multiplica el tiempo de build y la matriz de pruebas.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

La elección de SDK es casi mecánica una vez fijado el contexto, y la tabla de la sección anterior la resuelve. La decisión con contenido real es otra: cuántos proyectos de biblioteca crear, y la respuesta por defecto de esta guía es ninguno hasta que haya un consumidor externo o una dependencia que impedir.

Lo que sí conviene fijar el primer día son las propiedades comunes —`TargetFramework`, `Nullable`, `ImplicitUsings`— y fijarlas en `Directory.Build.props` en lugar de en cada `.csproj`. Activar `Nullable` desde el arranque es barato; activarlo en `ESC-3` sobre código existente cuesta semanas.

### `ESC-2` — Evolución estructural

Dos movimientos típicos. El primero es extraer una biblioteca de un proyecto que creció: se justifica cuando aparece el segundo consumidor, no cuando el proyecto se siente grande. El segundo es cambiar de SDK, que ocurre menos de lo que se teme; el caso realista es un worker que necesita exponer HTTP y pasa a `Microsoft.NET.Sdk.Web`, y el cambio es de una línea más las propiedades que el SDK nuevo trae por defecto.

Agregar multi-destino a una biblioteca interna que pasó a publicarse es el tercer caso, y es el más caro: obliga a revisar todo uso de API que no exista en el marco de destino más viejo.

### `ESC-3` — Normalización de código existente

Los repositorios que arrastran historia tienen dos patologías frecuentes. La primera es un `.csproj` de formato antiguo con archivos enumerados; migrar a estilo SDK reduce el archivo en un orden de magnitud y elimina la clase entera de defectos por archivo no incluido. La segunda es propiedades comunes repetidas en cada `.csproj`, que se resuelve moviéndolas a `Directory.Build.props`.

`Nullable` merece tratamiento aparte porque es la normalización de mayor impacto y la que más disciplina exige. Activarlo de golpe en un proyecto grande produce cientos de avisos que nadie va a leer. La alternativa practicable es activarlo con severidad de aviso, no de error, y bajar el número por archivo o por carpeta a lo largo de varios commits dedicados; la regla se sube a error cuando el número llega a cero, y no antes.

### `ESC-4` — Evaluación de código ajeno

Lo que se puede leer de un `.csproj` sin conocer nada del equipo: si el SDK corresponde a lo que el proyecto hace, si `Nullable` está activo, si las versiones de paquete están declaradas acá o vienen de un lugar central, si los proyectos de prueba tienen `IsPackable=false`, si un proyecto de biblioteca tiene metadatos de paquete completos o los tiene a medias.

La señal más informativa es la dispersión. Cinco `.csproj` que declaran el mismo `TargetFramework` cinco veces indican que no hay `Directory.Build.props`, y eso predice con bastante fiabilidad que tampoco habrá gestión centralizada de paquetes.

---

## Preguntas guía

1. ¿El SDK de este proyecto corresponde a lo que el proyecto hace, o es el que traía la plantilla que copié?
2. ¿Este proyecto de biblioteca tiene un consumidor fuera de la solución, o impide una dependencia concreta? Si no es ninguna de las dos, ¿por qué no es una carpeta?
3. ¿Qué propiedades de este `.csproj` están repetidas en otros? ¿Por qué no están en `Directory.Build.props`?
4. ¿`Nullable` está activo en todos los proyectos o hay islas?
5. Si esto es multi-destino, ¿quién consume el marco de destino más viejo y hasta cuándo?
6. En `CTX-3`: ¿los metadatos de paquete están completos y hay validación de compatibilidad de API en el build?
7. ¿Los niveles de prueba están en proyectos separados, o la compuerta rápida espera por la lenta?
8. ¿Este proyecto declara `IsPackable` de forma explícita, o depende de que el valor predeterminado siga siendo el conveniente?

---

## Criterios de calidad

Un conjunto de proyectos bien tipado se reconoce porque cada `.csproj` es corto y lo que contiene es específico de ese proyecto. Todo lo que se repite en más de dos proyectos vive en `Directory.Build.props`; lo que queda en el `.csproj` es el SDK, las referencias de paquete propias y las referencias de proyecto.

Las propiedades concretas: el SDK corresponde a lo que el proyecto hace; `Nullable` está activo en todos o en ninguno, sin islas; los proyectos de prueba declaran `IsPackable=false`; los nombres siguen una sola convención; y un proyecto de biblioteca existe porque algo lo consume, no porque el diagrama tenía una caja más.

### Antipatrones

**El SDK sobredimensionado.** Una biblioteca de clases declarada con `Microsoft.NET.Sdk.Web` porque «total, funciona». Arrastra el framework compartido de ASP.NET Core a un proyecto que no lo usa, cambia el comportamiento de publicación y agrega `using` implícitos que confunden. Compila, y por eso sobrevive.

**La biblioteca de una clase.** Un proyecto con un único tipo, sin consumidor externo y sin ninguna dependencia que impedir. Cuesta un `.csproj`, un conjunto de paquetes que sincronizar y una entrada en la solución; da una carpeta con nombre en mayúsculas.

**El `.csproj` copiado.** Un proyecto nuevo creado duplicando otro, con propiedades heredadas que no aplican —`OutputType=Exe` en una biblioteca, referencias al corredor de pruebas en un proyecto de producto—. Se detecta leyendo el archivo, que es algo que nadie hace después de crearlo.

**El multi-destino sin consumidor.** `TargetFrameworks` con tres marcos de destino en una aplicación interna que corre en uno solo. Triplica el tiempo de build y la matriz de pruebas para comprar una compatibilidad que nadie usa.

**Las pruebas empaquetables.** Un proyecto de prueba sin `IsPackable=false` que termina publicado en la fuente de paquetes de la organización porque la canalización ejecuta `dotnet pack` sobre la solución. Ocurre menos desde que los valores predeterminados mejoraron, y cuando ocurre es difícil de revertir: un paquete publicado no se despublica.

**La isla de nulabilidad.** `Nullable` activo en cuatro proyectos y ausente en el quinto, que es justamente el que más lo necesita. Produce falsa confianza: el análisis se corta en el límite del proyecto sin que nada lo señale.
