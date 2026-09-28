---
doc_id: TEM-NS
doc_type: tema
title: Espacios de nombres
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-INT, TEM-CAPAS, TEM-SLICE, TEM-CVP, TEM-CAPS, TEM-SLN, TEM-TOPO, TEM-MODELOS, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Espacios de nombres — `TEM-NS`

## Resumen ejecutivo

El espacio de nombres es la única parte de la organización interna que Microsoft especifica de forma normativa. `N-01` fija el patrón `<Empresa>.<Producto|Tecnología>[.<Funcionalidad>][.<Subespacio>]`, y esa es una diferencia importante respecto de todo lo demás que trata esta familia: los modelos de capas y el corte vertical son opinión bien fundada, la estructura del espacio de nombres tiene fuente oficial.

Su función es doble y conviene distinguirla. Hacia adentro organiza el código y previene colisiones de nombres. Hacia afuera —en `CTX-3`— forma parte del contrato publicado: renombrarlo rompe la compilación de todo consumidor, con el mismo efecto que renombrar un tipo público.

La mayor parte de las decisiones se toman una vez y se heredan del SDK sin que nadie las note, porque el generador de plantillas produce espacios de nombres correctos por defecto. Este documento importa cuando esa herencia se rompe: al mover archivos entre carpetas, al partir un proyecto en dos, al reorganizar de capas a cortes verticales. Le sirve a `ACT-02` en el trabajo diario y a `ACT-06`, para quien cada segmento del espacio de nombres es un compromiso de largo plazo.

---

## Definición

### Qué es

Un mecanismo del lenguaje que agrupa tipos bajo un nombre jerárquico y calificado, de modo que dos tipos con el mismo nombre simple puedan coexistir. `Reservas.Dominio.Reserva` y `Reservas.Infraestructura.Persistencia.Reserva` son tipos distintos y ninguno de los dos necesita renombrarse.

El espacio de nombres es una construcción **exclusiva del código fuente**. No sobrevive con esa forma al ensamblado —los metadatos guardan el nombre completo del tipo, no una jerarquía real— y no tiene ninguna relación con la accesibilidad: declarar un tipo dentro de un espacio de nombres no lo restringe a los tipos de ese espacio de nombres. Quien busque un mecanismo de encapsulamiento debe usar `internal`, no la jerarquía de nombres.

### Qué problema resuelve

**La colisión de identificadores.** Es su función original y el motivo por el que el mecanismo existe. Sin espacios de nombres, todo tipo del sistema y de todas sus dependencias competiría por un único listado plano de identificadores.

**La navegación.** Un nombre completo bien construido dice de dónde viene el tipo antes de abrirlo. `Reservas.Funcionalidades.CancelarReserva.CancelarReservaHandler` ubica el archivo sin buscarlo.

**La identificación del origen en `CTX-3`.** El segmento de empresa distingue el paquete de una organización del de otra, y es lo que evita que dos bibliotecas independientes publiquen tipos indistinguibles.

### Qué NO es, y con qué se lo confunde

**No es el nombre del ensamblado.** Coinciden por defecto y son propiedades distintas (`F-05`). El detalle se desarrolla más abajo porque la confusión tiene consecuencias reales.

**No es un límite de acceso.** Vale repetirlo porque el error aparece en revisiones: un tipo `public` en `Reservas.Infraestructura` es accesible desde `Reservas.Dominio` sin ninguna restricción. La dirección de dependencia la hacen cumplir los proyectos o los analizadores ([`TEM-CVP`](Carpetas-o-Proyectos.md)), nunca el espacio de nombres.

**No es una carpeta.** El SDK deriva uno del otro por convención, no por regla del lenguaje. Es perfectamente legal declarar `namespace Cualquiera;` en un archivo alojado en `Dominio/Reservas/`, y eso compila.

**No es el modelo de arquitectura.** Que existan `Application`, `Domain` e `Infrastructure` como segmentos no significa que las dependencias vayan en la dirección correcta. Es el equivalente nominal de las «capas de mentira» que registra [`TEM-CAPAS`](Modelos-de-Capas.md).

---

## La convención normativa

`N-01` especifica el patrón:

```text
<Empresa>.<Producto|Tecnología>[.<Funcionalidad>][.<Subespacio>]
```

Los segmentos van en PascalCase, con la capitalización que fija `N-02` y que esta guía desarrolla una sola vez en [`TEM-CAPS`](../40-Nomenclatura/Capitalizacion.md). Las reglas que acompañan al patrón en `N-01` y que más se incumplen: usar nombres de producto estables y no marcas de campaña que caducan; no usar como segmento un nombre que coincida con el de un tipo del mismo espacio, porque genera ambigüedades que el compilador resuelve de forma sorprendente; y pluralizar cuando el segmento agrupa un conjunto de elementos semánticamente equivalentes —`System.Collections`, no `System.Collection`.

Conviene recordar el alcance. `N-01` pertenece a las *Framework Design Guidelines*, escritas para el diseño de bibliotecas y no de aplicaciones, y reimpresas de la edición de 2008 con aviso de posible desactualización. En `CTX-3` se aplican literalmente. En una aplicación interna, el segmento de empresa aporta poco —nadie va a colisionar con ella— y la práctica corriente lo omite, arrancando directamente por el producto. Esta guía considera legítima esa omisión y recomienda declararla en las convenciones del equipo en lugar de dejarla implícita, para que no conviva con proyectos que sí lo incluyen.

Ejemplos válidos bajo el patrón:

```text
MiEmpresa.Reservas.Dominio.Salas        // empresa + producto + funcionalidad + subespacio
MiEmpresa.Reservas.Infraestructura      // empresa + producto + funcionalidad
Reservas.Funcionalidades.ReservarSala   // aplicación interna, sin segmento de empresa
```

---

## Relación con la estructura de carpetas

El SDK compone el espacio de nombres predeterminado de un archivo concatenando `RootNamespace` con su ruta relativa dentro del proyecto, reemplazando los separadores por puntos. Un archivo en `Dominio/Reservas/Reserva.cs`, dentro de un proyecto cuyo `RootNamespace` es `Reservas`, obtiene `Reservas.Dominio.Reservas` como espacio de nombres sugerido.

Ese comportamiento se manifiesta en dos lugares: al crear un archivo desde el IDE o con `dotnet new`, y en los analizadores de estilo, que señalan la discrepancia cuando el espacio de nombres declarado no coincide con la carpeta. No es una regla del lenguaje —el compilador acepta cualquier espacio de nombres en cualquier archivo— pero la convención es lo bastante fuerte como para que todo el instrumental la asuma.

La consecuencia práctica: **mover un archivo entre carpetas sin ajustar su espacio de nombres deja el proyecto en un estado inconsistente que el compilador no señala**. Es el modo más frecuente de degradar esta propiedad y aparece siempre en las reorganizaciones de `ESC-2`. Los IDE ofrecen la operación combinada —mover y renombrar el espacio de nombres— y esta guía recomienda usarla en lugar de arrastrar archivos en el explorador de archivos del sistema operativo.

### `RootNamespace` y `AssemblyName`

Son dos propiedades de MSBuild con propósitos distintos que por defecto toman el mismo valor: el nombre del archivo `.csproj`. De ahí que la mayoría de los proyectos nunca las declare y que la mayoría de los desarrolladores las trate como una sola cosa (`F-05`).

`AssemblyName` determina el nombre del archivo producido —`Reservas.Dominio.dll`— y por lo tanto el identificador del ensamblado en tiempo de ejecución, en las referencias y en el paquete NuGet. `RootNamespace` no afecta al artefacto compilado en absoluto: solo alimenta la generación del espacio de nombres predeterminado en archivos nuevos.

```xml
<PropertyGroup>
  <AssemblyName>MiEmpresa.Reservas.Dominio</AssemblyName>
  <RootNamespace>MiEmpresa.Reservas.Dominio</RootNamespace>
</PropertyGroup>
```

Divergen de forma legítima en dos casos. Uno es el proyecto cuyo nombre de archivo lleva un sufijo que no debe aparecer en el código —`Reservas.Dominio.Pruebas.csproj` que produce un ensamblado con ese nombre pero declara los tipos bajo `Reservas.Dominio.Pruebas` o bajo el espacio de nombres del código que prueba—. El otro es la renombración de un proyecto que ya se publicó: se cambia `AssemblyName` y se conserva `RootNamespace`, o al revés, según qué compromiso se quiera preservar. En `CTX-3` esa asimetría exige cuidado, porque el consumidor referencia el ensamblado por su nombre y escribe `using` con el espacio de nombres, y los dos cambios rompen cosas distintas.

---

## Espacios de nombres en varios proyectos

Todo lo anterior describe un proyecto. En cuanto la solución tiene varios ([`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md)), aparece un hecho que se pasa por alto porque el resultado *parece* una jerarquía: cada `.csproj` tiene su propio `RootNamespace`, derivado por defecto de su propio nombre de archivo, y no existe ningún mecanismo que los relacione entre sí. Una solución de siete proyectos tiene siete raíces independientes. Que todas empiecen por `MiEmpresa.Reservas` es una coincidencia sostenida por la convención de nombrar los proyectos con prefijo común, no una propiedad del lenguaje ni de MSBuild.

### Qué aporta el prefijo compartido y qué no

Aporta dos cosas, ambas reales. Agrupa los proyectos en el explorador de soluciones y en cualquier listado ordenado alfabéticamente, de modo que las siete raíces aparecen juntas. E identifica el producto: un tipo cuyo nombre completo empieza por `MiEmpresa.Reservas` se ubica sin averiguar de qué ensamblado salió, que es exactamente la función que `N-01` asigna a los segmentos de empresa y producto.

No aporta ninguna relación de dependencia. `MiEmpresa.Reservas.Web` no puede usar un tipo de `MiEmpresa.Reservas.Domain` por compartir el prefijo: lo puede usar si y solo si hay un `ProjectReference` que lo habilite. El prefijo tampoco impide nada. Un proyecto que declara sus tipos bajo `Otra.Cosa.Distinta` y está referenciado se consume con normalidad.

### La jerarquía de nombres no es el grafo de referencias

Son dos estructuras que se dibujan igual y significan cosas distintas. `MiEmpresa.Reservas.Contracts` se lee como si colgara de `MiEmpresa.Reservas`, y `MiEmpresa.Reservas.Domain` también, con lo cual la lectura natural sugiere que son hermanos de un mismo padre y que algo los vincula. No los vincula nada. En `T5` de [`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md), `Contracts` es precisamente el proyecto que **no** referencia `Domain`, y esa ausencia es la regla que sostiene la topología entera: si la referencia existiera, el cliente móvil se llevaría las entidades del dominio.

Es la misma propiedad que este documento ya enuncia para un proyecto único —el espacio de nombres no es un límite de acceso— vista desde el otro lado. Ahí el problema era creer que la jerarquía restringe; acá es creer que expresa. Un nombre completo dice dónde está declarado el tipo. No dice quién puede verlo, ni quién depende de quién. Para eso están el grafo de `ProjectReference` y los analizadores ([`TEM-CVP`](Carpetas-o-Proyectos.md)).

Una consecuencia práctica de `ESC-4`: leer los espacios de nombres de una solución ajena no permite reconstruir su grafo de dependencias. Hay que leer los `.csproj`.

### Colisión de nombres simples entre proyectos

El caso concreto aparece en cuanto hay contratos. `MiEmpresa.Reservas.Domain.Reservas` declara `Reserva`; `MiEmpresa.Reservas.Contracts.Reservas` declara `ReservaResponse` y `CrearReservaRequest`. Mientras los nombres simples difieran, un archivo importa ambos espacios de nombres y no pasa nada.

La colisión aparece cuando el mismo nombre simple existe en los dos, que es lo que ocurre si los contratos se llaman `Reserva` a secas. El compilador da error de ambigüedad en cada uso, y hay dos formas de resolverlo. Una es calificar el tipo completo en el punto de uso, que resuelve el error y ensucia cada línea. La otra es el alias de using, que nombra el espacio de nombres una vez por archivo:

```csharp
using DominioReservas = MiEmpresa.Reservas.Domain.Reservas;
using ContratosReservas = MiEmpresa.Reservas.Contracts.Reservas;

// El nombre simple queda inutilizable; cada uso lleva su prefijo.
ContratosReservas.Reserva respuesta = Mapear(entidad: (DominioReservas.Reserva)origen);
```

Ambas soluciones funcionan y ninguna es buena. El costo no está en las teclas: está en que todo lector debe resolver mentalmente a qué apunta cada alias, y los alias los elige cada archivo por su cuenta, de modo que el mismo espacio de nombres aparece con tres nombres distintos en tres archivos.

Por eso [`TEM-MODELOS`](Modelos-y-Contratos.md) recomienda sufijos que distingan el plano —`Request`, `Response`, `Vm`, y el dominio sin sufijo—. La recomendación se justifica ahí por lo que el sufijo comunica; acá se ve su efecto colateral, que es de la misma importancia: con `Reserva` y `ReservaResponse` la ambigüedad no se resuelve, **no llega a existir**. Los alias son la reparación de un problema que la nomenclatura evita.

### El archivo de mapeo

Hay un lugar donde ambos espacios de nombres conviven por diseño, y es el único: el mapeo entre representaciones. Un método que recibe una entidad de dominio y devuelve un DTO necesita ver los dos tipos, y ese archivo importa los dos espacios de nombres con naturalidad.

```csharp
// MiEmpresa.Reservas.Api/Reservas/MapeoReservas.cs
using MiEmpresa.Reservas.Contracts.Reservas;
using MiEmpresa.Reservas.Domain.Reservas;

namespace MiEmpresa.Reservas.Api.Reservas;

internal static class MapeoReservas
{
    public static ReservaResponse ADto(this Reserva reserva, string nombreSala) => /* ... */;
}
```

La lectura inversa sirve como diagnóstico. Si los `using` del dominio y los de contratos aparecen juntos en archivos que no son de mapeo —en un componente de la interfaz, en un repositorio, en el propio dominio—, el DTO se filtró fuera del borde o la entidad cruzó hacia adentro, que son los dos antipatrones centrales de [`TEM-MODELOS`](Modelos-y-Contratos.md).

### Las siete raíces de `T5`

La topología `T5` de [`TEM-TOPO`](../20-Organizacion-de-Soluciones/Topologias-de-Solucion.md) enfrentada a sus espacios de nombres, en la variante de plano estructural en inglés y dominio en español. Ejemplo sintético.

```text
PROYECTO (.csproj)                      RAÍZ Y SUBESPACIOS TÍPICOS
──────────────────────────────────      ──────────────────────────────────────────
MiEmpresa.Reservas.Domain               MiEmpresa.Reservas.Domain
                                        MiEmpresa.Reservas.Domain.Reservas
                                        MiEmpresa.Reservas.Domain.Salas

MiEmpresa.Reservas.Application          MiEmpresa.Reservas.Application.Reservas
                                        MiEmpresa.Reservas.Application.Ports

MiEmpresa.Reservas.Infrastructure       MiEmpresa.Reservas.Infrastructure.Persistence
                                        MiEmpresa.Reservas.Infrastructure.Notificaciones

MiEmpresa.Reservas.Contracts            MiEmpresa.Reservas.Contracts.Reservas
                                        MiEmpresa.Reservas.Contracts.Salas

MiEmpresa.Reservas.Api                  MiEmpresa.Reservas.Api.Reservas

MiEmpresa.Reservas.Web                  MiEmpresa.Reservas.Web
                                        MiEmpresa.Reservas.Web.Components.Reservas

MiEmpresa.Reservas.Movil                MiEmpresa.Reservas.Movil.Vistas
```

Siete raíces, siete `RootNamespace` que nadie declaró porque cada uno se derivó del nombre de su `.csproj`. El prefijo `MiEmpresa.Reservas` se repite siete veces sin que ningún archivo lo diga.

Dos detalles del cuadro. `Contracts` aparece al mismo nivel que `Domain` y no debajo, lo que refleja que son proyectos hermanos sin referencia entre ellos; el nombre no lo hubiera dicho. Y `Movil` declara sus propios subespacios de interfaz mientras consume los tipos de `Contracts` tal como llegan, sin redeclararlos: el contrato es un ensamblado referenciado, no un espacio de nombres que cada cliente reconstruye.

---

## Espacios de nombres con ámbito de archivo

Desde C# 10 la declaración admite una forma sin llaves que aplica al archivo completo. Sustituye a la forma con bloque en el caso —abrumadoramente mayoritario— de un solo espacio de nombres por archivo, y elimina un nivel de indentación de todo el contenido.

```csharp
// Forma con bloque: todo el archivo indentado un nivel de más
namespace Reservas.Dominio.Salas
{
    public sealed class Sala
    {
        public required string Nombre { get; init; }
    }
}
```

```csharp
// Ámbito de archivo: C# 10 en adelante
namespace Reservas.Dominio.Salas;

public sealed class Sala
{
    public required string Nombre { get; init; }
}
```

`N-06` recomienda la forma con ámbito de archivo en las convenciones actuales de C#, y las plantillas del SDK la generan por defecto en proyectos con versión de lenguaje suficiente. Esta guía recomienda adoptarla de forma uniforme y hacerla cumplir con la regla de estilo correspondiente en `.editorconfig`, porque es una de las pocas normalizaciones de `ESC-3` que una herramienta aplica sola, verifica sola y no cambia ningún comportamiento: el diff es grande pero mecánicamente seguro, y se resuelve en un commit dedicado con su entrada en `.git-blame-ignore-revs`.

La forma con bloque sigue siendo necesaria cuando un archivo declara tipos en más de un espacio de nombres. Ese caso es raro y casi siempre indica que el archivo debería partirse.

---

## `global using` e `ImplicitUsings`

`ImplicitUsings` es una propiedad del proyecto que, cuando está activada, hace que el SDK inserte un conjunto de directivas `global using` correspondientes al tipo de proyecto. Un proyecto web obtiene `System`, `System.Linq`, `Microsoft.AspNetCore.Builder` y una decena más sin que aparezcan en ningún archivo del repositorio.

```xml
<PropertyGroup>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

Un `global using` explícito extiende el mecanismo a espacios de nombres propios y aplica a todos los archivos del proyecto.

```csharp
// Usings.cs — un único archivo, en la raíz del proyecto
global using Reservas.Dominio;
global using Reservas.Dominio.Salas;
```

El beneficio es visible: desaparecen quince líneas repetidas al principio de cada archivo. El riesgo lo es menos, y conviene enunciarlo con precisión. **Un tipo que aparece sin `using` visible oculta de dónde viene.** Quien lee un archivo no puede determinar por inspección local si `Sala` es del dominio propio o de una biblioteca de terceros, y quien evalúa código ajeno pierde el indicador más rápido de las dependencias de un archivo. En un sistema donde la dirección de dependencia se verifica leyendo los `using` —que es exactamente el caso de la opción de carpetas de [`TEM-CVP`](Carpetas-o-Proyectos.md)—, un `global using` de infraestructura anula el método de verificación.

Esta guía recomienda: mantener `ImplicitUsings` activado, porque su contenido es conocido y está documentado; declarar los `global using` propios en un único archivo dedicado y con nombre reconocible, nunca dispersos; y **no incluir nunca espacios de nombres de infraestructura entre ellos**. Un `global using Microsoft.EntityFrameworkCore;` en un proyecto que separa capas por carpetas hace que el acceso a datos esté disponible en todas ellas sin dejar rastro en ningún archivo.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

Se decide una vez y se hereda: el nombre del proyecto determina `RootNamespace`, y a partir de ahí las carpetas hacen el resto. Las dos decisiones que sí requieren criterio son si se incluye el segmento de empresa y qué segmento representa la organización interna —`Dominio`, `Aplicacion`, `Infraestructura` si el sistema se organiza por capas; `Funcionalidades` si se organiza por cortes.

Esta guía recomienda fijar el idioma el primer día, con la precisión que aporta [`TEM-MODELOS`](Modelos-y-Contratos.md): un solo idioma **por plano**, no un solo idioma. El plano estructural —`Domain`, `Application`, `Ports`— y el del dominio —`Salas`, `Reservas`, `PoliticaCancelacion`— se deciden por criterios distintos y pueden resolverse en idiomas distintos. Lo que no admite variación es la mezcla dentro de un mismo plano: `Reservas.Dominio.Salas` junto a `Reservas.Application.Handlers` obliga a recordar en qué idioma está cada parte, y ese costo no se paga una vez sino en cada búsqueda.

### `ESC-2` — Evolución estructural

Es el escenario donde la propiedad se rompe. Toda reorganización mueve archivos, y todo archivo movido arrastra un espacio de nombres que ya no corresponde a su ubicación. Cuando la reorganización es de capas a cortes verticales, el efecto es masivo: los segmentos `Servicios` y `Repositorios` desaparecen del árbol de carpetas y sobreviven en los espacios de nombres durante meses.

La operación de mover con actualización del espacio de nombres que ofrecen los IDE resuelve el caso, y esta guía recomienda además activar la regla de estilo que exige coincidencia entre espacio de nombres y carpeta, en el mismo commit de la reorganización, para que la deriva no se reintroduzca. Es la aplicación literal del criterio de `ESC-3`: normalizar sin activar la regla garantiza repetir el trabajo.

Al partir un proyecto en dos ([`TEM-CVP`](Carpetas-o-Proyectos.md)) hay una decisión adicional. Si el espacio de nombres de los tipos extraídos cambia, todo consumidor necesita ajustar sus `using`; si se conserva, se obtiene un ensamblado nuevo cuyo `RootNamespace` no coincide con su `AssemblyName`. En una aplicación conviene cambiarlo —el ajuste es mecánico y lo hace el compilador señalando cada punto—. En `CTX-3` conservarlo es lo correcto, porque el cambio sería ruptor para consumidores que no se controlan.

### `ESC-3` — Normalización de código existente

Es el documento de la familia que mejor se normaliza. Tres operaciones aplican, y las tres las hace una herramienta: convertir a espacios de nombres con ámbito de archivo, alinearlos con las carpetas, y ordenar y depurar las directivas `using`.

Se aplican en commits dedicados sin ningún cambio funcional, se registran en `.git-blame-ignore-revs` y se acompañan de la activación de la regla correspondiente en `.editorconfig`. El detalle del procedimiento está en [`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md).

Una advertencia sobre el orden. Alinear los espacios de nombres con las carpetas **cambia el nombre completo de los tipos**, y eso no es cosmético: rompe la reflexión por nombre, la deserialización que persiste nombres calificados, y las configuraciones que referencian tipos como cadena de texto. Antes de ejecutar la normalización conviene buscar esos usos. Es el único caso de esta guía donde una operación de estilo puede alterar el comportamiento en ejecución.

### `ESC-4` — Evaluación de código ajeno

Los espacios de nombres son un instrumento de diagnóstico barato porque se leen sin entender el dominio. Tres lecturas rinden.

La primera es de coherencia entre el espacio de nombres declarado y la carpeta donde vive el archivo. La discrepancia sistemática indica reorganizaciones a medio hacer y suele venir acompañada de otras inconsistencias.

La segunda es de coherencia entre el espacio de nombres y el modelo declarado. Si el README describe cortes verticales y los espacios de nombres dicen `Servicios` y `Repositorios`, el modelo vigente es el que dicen los espacios de nombres.

La tercera aplica a `CTX-3` y es la más consecuente: si el paquete publica tipos bajo espacios de nombres que no llevan el segmento de la organización, o bajo nombres genéricos, hay riesgo de colisión con el consumidor, y ese es un motivo legítimo para observar una biblioteca antes de adoptarla.

---

## Ejemplos concretos

**La misma jerarquía, todo en español.** El cuadro de `T5` de más arriba usa la variante de plano estructural en inglés; este ejemplo cambia deliberadamente a la variante íntegra en español —`Dominio`, `Aplicacion`, `Puertos`, `Infraestructura`— para mostrar que ambas son coherentes mientras no se mezclen. El criterio que gobierna la elección es de [`TEM-MODELOS`](Modelos-y-Contratos.md).

La correspondencia entre carpeta y espacio de nombres se ve mejor con los dos árboles enfrentados. El ejemplo es sintético: la aplicación de reserva de salas que la guía usa como hilo conductor, organizada por capas, con `AssemblyName` y `RootNamespace` derivados del nombre del proyecto.

```text
src/MiEmpresa.Reservas.Web/
├── MiEmpresa.Reservas.Web.csproj
├── Program.cs
├── Dominio/
│   ├── Salas/
│   │   ├── Sala.cs
│   │   └── DisponibilidadSala.cs
│   └── Reservas/
│       ├── Reserva.cs
│       ├── RangoHorario.cs
│       └── PoliticaCancelacion.cs
├── Aplicacion/
│   ├── ServicioReservas.cs
│   └── Puertos/
│       └── IRepositorioReservas.cs
├── Infraestructura/
│   ├── Persistencia/
│   │   ├── ReservasDbContext.cs
│   │   └── Entidades/
│   └── Notificaciones/
└── Components/
```

Cada archivo declara el espacio de nombres que su ruta predice, con sintaxis de ámbito de archivo:

```text
MiEmpresa.Reservas.Web
MiEmpresa.Reservas.Web.Dominio.Salas
MiEmpresa.Reservas.Web.Dominio.Reservas
MiEmpresa.Reservas.Web.Aplicacion
MiEmpresa.Reservas.Web.Aplicacion.Puertos
MiEmpresa.Reservas.Web.Infraestructura.Persistencia
MiEmpresa.Reservas.Web.Infraestructura.Persistencia.Entidades
MiEmpresa.Reservas.Web.Infraestructura.Notificaciones
```

El mapeo con `N-01` es directo: `MiEmpresa` es el segmento de empresa, `Reservas` el producto, `Web` la funcionalidad —el frontal, por oposición a un `MiEmpresa.Reservas.Importador` que compartiría el dominio—, y de ahí en adelante los subespacios. Nótese `Salas` y `Reservas` en plural, como pide la regla de pluralización de `N-01` para segmentos que agrupan elementos equivalentes, frente a los tipos `Sala` y `Reserva` en singular. La misma aplicación como sistema interno omitiría el primer segmento y arrancaría en `Reservas.Web`, con la salvedad de que esa omisión conviene declararla en las convenciones del equipo.

Un rasgo más del ejemplo que no se lee en el árbol: el `.csproj` activa `ImplicitUsings` sin declarar ningún `global using` propio: se aprovecha lo documentado del SDK y no se oculta ninguna dependencia del proyecto.

Conviene notar por último que `Aplicacion.Puertos` describe un rol arquitectónico —los puertos de `O-05`— y no una funcionalidad. Es coherente con un sistema organizado por capas, y sería un antipatrón en uno organizado por cortes verticales, donde el segmento equivalente sería `Funcionalidades.ReservarSala`.

---

## Preguntas guía

1. ¿La ruta de cada archivo predice su espacio de nombres? Un muestreo de diez archivos en carpetas distintas lo responde.
2. ¿El espacio de nombres refleja cómo está organizado el sistema hoy, o cómo lo estuvo antes de la última reorganización?
3. ¿`RootNamespace` y `AssemblyName` coinciden? Si no, ¿la divergencia está registrada y es deliberada?
4. ¿Todo el código usa espacios de nombres con ámbito de archivo, y lo verifica una regla o depende de la costumbre?
5. ¿Qué `global using` propios existen, dónde están declarados, y hay alguno de infraestructura?
6. En `CTX-3`: ¿algún espacio de nombres publicado contiene un nombre de marca, de versión o de iniciativa que vaya a caducar antes que el paquete?
7. Antes de alinear los espacios de nombres con las carpetas en `ESC-3`: ¿hay reflexión, deserialización o configuración que dependa de nombres de tipo calificados?
8. ¿Los segmentos de cada plano están en un solo idioma, y la decisión de cada plano está escrita?
9. En una solución de varios proyectos: ¿hay archivos fuera del mapeo que importen a la vez el espacio de nombres del dominio y el de contratos?
10. ¿Algún archivo necesita alias de using para desambiguar tipos propios? Si lo necesita, ¿el problema es la organización o son los nombres?

---

## Criterios de calidad

Un sistema con espacios de nombres sanos tiene una propiedad simple de verificar: la ruta del archivo predice su espacio de nombres y el espacio de nombres predice la ruta, sin excepciones. A partir de ahí, tres condiciones más. Los segmentos siguen `N-01` en su estructura y `N-02` en su capitalización. Cada plano de vocabulario está en un solo idioma, según el criterio de [`TEM-MODELOS`](Modelos-y-Contratos.md). Y el espacio de nombres refleja el eje de organización real del sistema, no el de una reorganización anterior que quedó a medias.

Antipatrones nombrados:

**Espacio de nombres huérfano de carpeta.** El archivo vive en `Funcionalidades/ReservarSala/` y declara `namespace Reservas.Servicios;`. Sobrevivió a una reorganización. El coste no es estético: la navegación por nombre completo deja de funcionar y el desarrollador siguiente lo replica por imitación.

**Un espacio de nombres por clase.** Cada tipo en su propio segmento, con jerarquías de seis niveles que contienen un archivo cada una. Multiplica los `using` sin agrupar nada, y suele ser el residuo de generar la estructura desde una herramienta.

**Capa técnica sobre organización funcional.** El sistema se organiza por cortes verticales y los espacios de nombres conservan `Servicios`, `Repositorios` y `Modelos`. El nombre completo del tipo contradice la estructura de carpetas, y quien llega no sabe cuál de las dos es la vigente.

**Colisión con el framework.** Un tipo propio llamado `System`, `Task`, `List` o `Environment`, o un segmento de espacio de nombres con esos nombres. Compila, y produce errores de resolución cuya causa es difícil de ver: dentro de un espacio de nombres `MiEmpresa.System`, la referencia `System.String` puede resolverse contra el espacio de nombres propio en lugar de contra el del framework. `N-01` desaconseja que un segmento coincida con el nombre de un tipo del mismo espacio, y `N-03` advierte sobre los identificadores que chocan con palabras clave o con nombres del framework; ambas apuntan a este caso.

**Segmento de marca caduca.** `MiEmpresa.ProyectoFenix.Reservas`, donde «Fénix» fue el nombre de la iniciativa de 2019. En `CTX-3` es permanente: el espacio de nombres publicado no se cambia sin ruptura, y la marca sobrevive a la iniciativa que la originó.

**`global using` de infraestructura.** `global using Microsoft.EntityFrameworkCore;` en un proyecto que separa capas por carpetas. Anula la única verificación disponible de la dirección de dependencia, y no deja rastro en ningún archivo del dominio.

**Idiomas mezclados dentro de un plano.** `Reservas.Domain.Salas` junto a `Reservas.Aplicacion.Handlers`: dos segmentos estructurales en idiomas distintos. Cada búsqueda requiere probar dos veces. El caso distinto —`Domain` estructural con `Salas` del negocio— no es este antipatrón sino la combinación que [`TEM-MODELOS`](Modelos-y-Contratos.md) declara legítima.

**El alias de using como solución permanente.** Un archivo abre con `using DominioReservas = ...;` y `using ContratosReservas = ...;` porque dos tipos propios comparten nombre simple, y el alias se replica por copia en veinte archivos con nombres distintos en cada uno. El alias resuelve la ambigüedad y deja el diagnóstico sin atender: los nombres no distinguen el plano al que pertenece cada tipo, que es lo que los sufijos de [`TEM-MODELOS`](Modelos-y-Contratos.md) existen para evitar. Su uso legítimo es el conflicto con un tipo de terceros, que no se puede renombrar.

**El prefijo compartido leído como jerarquía.** Alguien concluye que `MiEmpresa.Reservas.Contracts` «está bajo» `MiEmpresa.Reservas` y por lo tanto puede ver el dominio, o que la jerarquía de nombres documenta el grafo de referencias. Son raíces independientes que comparten un prefijo por convención de nombrado de proyectos. La revisión de dependencias se hace sobre los `.csproj`.
