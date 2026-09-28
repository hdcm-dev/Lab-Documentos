---
doc_id: TEM-NOMB
doc_type: tema
title: Nombrado de tipos y miembros
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-NOM, TEM-CAPS, TEM-ANTI, TEM-NS, TEM-MODELOS, TEM-SLICE, TEM-AUTO, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Nombrado de tipos y miembros — `TEM-NOMB`

## Resumen ejecutivo

Un nombre es la interfaz más consultada de cualquier elemento de código. Se lee cientos de veces por cada vez que se lee su implementación, y en la mayoría de esas lecturas el nombre es lo único que se lee. De ahí que la elección de palabras tenga un rendimiento desproporcionado respecto de su costo: treinta segundos de pensar el nombre correcto ahorran horas repartidas entre todos los que después van a leerlo.

Microsoft especifica más de lo que suele recordarse. `N-03` fija que los nombres deben ser legibles antes que breves, prohíbe las abreviaturas y las contracciones, y descarta explícitamente la notación húngara. `N-04` asigna categoría gramatical a cada clase de miembro: los tipos son sustantivos, los métodos son verbos, las propiedades son sustantivos o adjetivos, los eventos son verbos conjugados en el tiempo que corresponde a su momento.

Lo que no está especificado —y ocupa la segunda mitad de este documento— es todo lo que un equipo hispanohablante decide de verdad: qué sufijos convencionales adopta, cómo nombra sus tests, y cómo se lleva el lenguaje ubicuo del negocio a identificadores que conviven con el vocabulario del framework. La decisión de idioma en sí no se toma acá: la fija [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md) plano por plano, y este documento trabaja sobre ella.

---

## Definición

### Qué es

La elección de las palabras que componen un identificador y de su categoría gramatical, de modo que quien lo lee pueda anticipar qué representa y qué hace sin abrir su implementación.

Descansa sobre una hipótesis que conviene explicitar porque a veces se discute: **un nombre es una afirmación verificable**. `ObtenerReserva` afirma que devuelve una reserva y que no cambia nada; si además persiste algo, el nombre es falso. Un nombre falso es peor que un nombre vago, porque el vago obliga a leer el código y el falso convence de que no hace falta.

### Qué problema resuelve

**El costo de reconstruir la intención.** El código dice qué hace la máquina; el nombre dice qué se pretendía. Cuando ambos coinciden, un lector puede razonar sobre el sistema a nivel de nombres y bajar al detalle solo donde le hace falta. Cuando no coinciden, cada lectura obliga a verificar todo desde cero.

**La distancia entre el negocio y el código.** Cuando el analista dice «cancelar una reserva» y el código dice `ReservaService.Delete()`, cada conversación entre ambos mundos pasa por una traducción que alguien tiene que sostener de memoria. El *lenguaje ubicuo* de Evans (`O-03`) es la práctica que elimina esa traducción, y su forma concreta en el código es esta: los nombres son los del negocio.

**La búsqueda.** Un vocabulario único hace que `grep Cancelar` encuentre todo lo relacionado con cancelaciones. Un vocabulario donde conviven `Cancelar`, `Anular`, `Rechazar` y `Baja` para la misma operación hace que no encuentre nada completo, y que quien busca no sepa que le falta algo.

### Qué NO es, y con qué se lo confunde

**No es capitalización.** Cómo se escribe `CancelarReserva` es [`TEM-CAPS`](Capitalizacion.md). Este documento trata por qué se llama así y no `ProcesarSolicitud2`.

**No es documentación.** Un buen nombre reduce la necesidad de comentarios pero no reemplaza a la documentación XML de una API pública, que explica precondiciones, excepciones y contrato. Lo que sí reemplaza es el comentario que traduce un nombre malo: si un método necesita `// obtiene la reserva y la marca como vista`, el nombre debería decirlo.

**No es una prohibición de renombrar.** El buen nombre rara vez aparece al primer intento; aparece cuando se entendió el problema, que suele ser después de escribir el código. Renombrar al terminar es parte del trabajo, con una advertencia de contexto: en `CTX-1` y `CTX-2` es gratis, en `CTX-3` no.

---

## Reglas normativas

### `N-03` — Convenciones generales

**Legibilidad antes que brevedad.** `N-03` prescribe elegir nombres legibles y afirma que la claridad vale más que la longitud. Un nombre de veinte caracteres que se entiende gana a uno de ocho que hay que descifrar.

```csharp
// Cumple
public Reserva ObtenerReservaConfirmadaPorCodigo(string codigo) => _repositorio.Buscar(codigo);

// No cumple: abrevia y no se entiende sin contexto
public Reserva GetRsvConfPorCod(string cod) => _repositorio.Buscar(cod);
```

**Sin abreviaturas ni contracciones.** `N-03` es explícita: no se usan abreviaturas ni contracciones como parte de los nombres de identificador. `usuario`, no `usr`. `contador`, no `cnt`. `administrador`, no `admin` ni `mgr`. La única excepción sensata son las siglas ya asentadas como palabras del dominio técnico —`Http`, `Xml`, `Uri`, `Id`— que se capitalizan según la regla de acrónimos de [`TEM-CAPS`](Capitalizacion.md).

**Sin notación húngara.** `N-03` lo dice con la fórmula «DO NOT use Hungarian notation»: el tipo no se codifica en el nombre. `strNombre`, `iContador`, `arrSalas` quedan descartados. El desarrollo del antipatrón, con su origen y con la distinción que casi nunca se explica, está en [`TEM-ANTI`](Antipatrones-de-Nombrado.md).

**Sin identificadores que colisionen con palabras clave de otros lenguajes .NET.** Es una recomendación de compatibilidad que solo importa de verdad en `CTX-3`, y que hoy tiene menos peso que en 2008.

### `N-04` — Categoría gramatical por clase de miembro

| Elemento | Categoría gramatical | Ejemplo | Contraejemplo |
|----------|---------------------|---------|---------------|
| Clase, struct, record | Sustantivo o frase nominal | `Reserva`, `PoliticaCancelacion` | `Reservar`, `GestionarSalas` |
| Interfaz | Sustantivo, frase nominal o adjetivo de capacidad | `IRepositorioSalas`, `IDisposable` | `IHacerCosas` |
| Método | Verbo o frase verbal | `Cancelar`, `CalcularCosto` | `Cancelacion`, `Costo` |
| Propiedad | Sustantivo, frase nominal o adjetivo | `FechaInicio`, `Duracion` | `ObtenerFechaInicio` |
| Propiedad booleana | Prefijo `Is`, `Can`, `Has` — o su forma en español | `EstaConfirmada`, `PuedeCancelarse` | `Confirmacion`, `Cancelable2` |
| Evento | Verbo, en gerundio o participio | `Cerrando` / `Cerrada` | `EventoDeCierre` |
| Enumeración | Sustantivo singular; plural solo con `[Flags]` | `EstadoReserva`, `[Flags] PermisosSala` | `[Flags] PermisoSala` |
| Excepción | Frase nominal + sufijo `Exception` | `SalaNoDisponibleException` | `ErrorDeSala` |

**La regla de tipos.** Una clase representa una cosa, y las cosas se nombran con sustantivos. Cuando el sustantivo no aparece, la señal suele ser que el tipo no representa una cosa sino que agrupa procedimientos sueltos, y eso es un problema de diseño antes que de nombre: `GestionarSalas` casi siempre debería ser una función de otro tipo o un tipo con una responsabilidad identificable.

**La regla de métodos.** Un método hace algo, y lo que se hace se nombra con verbo. `Reserva.Cancelacion()` describe un concepto donde debería describir una acción.

**La regla de propiedades y el error que produce.** Una propiedad es un dato, no una operación, y por eso se nombra como se nombra un dato. El error corriente es traer de Java el par `getX/setX`:

```csharp
// Java trasladado a C#
public DateTimeOffset ObtenerFechaInicio() => _fechaInicio;
public void EstablecerFechaInicio(DateTimeOffset valor) => _fechaInicio = valor;

// C#
public DateTimeOffset FechaInicio { get; private set; }
```

**Booleanas.** `N-04` recomienda el prefijo `Is`, `Can` o `Has`. Trasladado al español, la construcción equivalente es `Esta`, `Es`, `Puede`, `Tiene`: `EstaConfirmada`, `EsRecurrente`, `PuedeCancelarse`, `TieneAsistentes`. Lo que la regla evita es la propiedad booleana nombrada como sustantivo —`Confirmacion`— que obliga a leer el tipo para saber si devuelve un dato o un sí/no.

**Eventos y sus dos tiempos.** El par gerundio/participio codifica el momento respecto de la acción, y es una convención con consecuencia funcional: el evento en gerundio ocurre **antes** y suele ser cancelable; el de participio ocurre **después** y ya no admite intervención.

```csharp
public event EventHandler<CancelacionEventArgs>? Cancelando;  // antes; se puede cancelar
public event EventHandler<ReservaEventArgs>? Cancelada;       // después; es un hecho
```

**Enumeraciones.** Singular si los valores son excluyentes, plural si el tipo lleva `[Flags]` y admite combinación. El plural comunica que un valor puede contener varios.

```csharp
public enum EstadoReserva { Pendiente, Confirmada, Cancelada }   // singular

[Flags]
public enum PermisosSala                                         // plural
{
    Ninguno   = 0,
    Ver       = 1,
    Reservar  = 2,
    Aprobar   = 4,
    Administrar = Ver | Reservar | Aprobar
}
```

### Interfaces: sustantivo o adjetivo de capacidad

`N-01` reconoce dos formas y la elección no es estética.

**Sustantivo** cuando la interfaz representa **lo que algo es**: un papel, un contrato de colaboración. `IRepositorioSalas`, `IServicioNotificaciones`, `IPoliticaCancelacion`. Es la forma habitual en el código de aplicación.

**Adjetivo de capacidad, terminado en `-able`,** cuando la interfaz representa **lo que algo puede hacer** y se agrega a tipos de naturalezas distintas: `IDisposable`, `IComparable`, `IEnumerable`, `ICloneable`. Es la forma habitual en las bibliotecas base y la que corresponde cuando el tipo que implementa no queda definido por esa capacidad —una `Reserva` que es `IComparable` no es «una comparable», es una reserva que además se compara—.

En español el sufijo `-able` funciona igual y no hace falta forzar el inglés: `ICancelable`, `IAuditable`, `INotificable`.

---

## Sufijos convencionales y qué comunican

Un sufijo es la forma más económica de comunicar un papel: `RepositorioSalas` dice de qué se trata la clase antes de abrirla. Pero no todos tienen el mismo peso, y la distinción entre los normativos y los convencionales importa porque solo unos pocos son obligatorios.

### Normativos — derivan de tipos base del framework

`N-01` los prescribe: un tipo que hereda de estas bases **debe** llevar el sufijo, y ningún tipo que no herede de ellas debe llevarlo.

| Sufijo | Se aplica a | Ejemplo |
|--------|-------------|---------|
| `Exception` | Todo tipo que deriva de `System.Exception` | `SalaNoDisponibleException` |
| `Attribute` | Todo tipo que deriva de `System.Attribute` | `ValidarDisponibilidadAttribute` |
| `EventArgs` | Todo tipo que deriva de `System.EventArgs` | `ReservaCanceladaEventArgs` |
| `Collection` | Implementaciones de `IEnumerable`/`ICollection` | `ReservaCollection` |
| `Dictionary` | Implementaciones de `IDictionary` | `SalaPorCodigoDictionary` |
| `Stream` | Tipos que derivan de `System.IO.Stream` | `ReservaExportStream` |

La regla es bidireccional, y la segunda mitad se olvida. Una clase llamada `ReservaCollection` que no implementa `IEnumerable` miente sobre lo que es, y el lector que la trata como enumerable descubre el error al compilar o —peor— al leer.

### Convencionales — comunican un papel arquitectónico

Ninguno es normativo. Son vocabulario compartido del ecosistema, con orígenes distintos: algunos vienen de patrones catalogados (`O-02`), otros del propio ASP.NET Core, otros del uso.

| Sufijo | Qué comunica | Origen |
|--------|--------------|--------|
| `Service` / `Servicio` | Operación de aplicación sin estado propio | Uso extendido; sentido variable |
| `Repository` / `Repositorio` | Acceso a persistencia con interfaz de colección | Patrón Repository, `O-02` |
| `Factory` / `Fabrica` | Construcción de objetos con lógica de decisión | Patrón Factory |
| `Options` | Configuración enlazada, patrón de opciones | ASP.NET Core |
| `Handler` / `Manejador` | Atiende un comando, consulta o evento | Uso extendido |
| `Controller` | Punto de entrada HTTP en MVC | Convención de enrutamiento de ASP.NET Core |
| `Middleware` | Componente de la canalización HTTP | ASP.NET Core |
| `Component` | Componente de interfaz | Uso; en Blazor no se acostumbra |
| `Validator` / `Validador` | Verifica invariantes de un objeto | Uso extendido |
| `Builder` / `Constructor` | Construcción incremental | Patrón Builder |

`Controller` merece una nota aparte: en ASP.NET Core MVC no es solo convención sino que participa del **descubrimiento por convención** del enrutamiento. Un tipo que hereda de `ControllerBase` sin ese sufijo funciona, pero el nombre de la ruta generada por `[controller]` deja de coincidir con lo esperado.

`Service` es el sufijo más gastado del ecosistema y el que menos comunica. Antes de usarlo conviene comprobar si hay un nombre que diga qué hace: `CalculadorDeCosto` dice más que `CostoService`. El caso degenerado —una clase que hace todo lo relacionado con un sustantivo— tiene tratamiento propio en [`TEM-ANTI`](Antipatrones-de-Nombrado.md).

---

## Nombrado de tests

Los tests siguen convenciones propias, y hay un motivo por el que la regla de `N-03` sobre guiones bajos se relaja acá. Un método de prueba **no lo invoca nadie desde código**: no es superficie de API, no participa de ningún contrato, y su nombre tiene un único consumidor, que es el informe del corredor de pruebas cuando algo falla. Optimizarlo para legibilidad en ese informe, en lugar de para consistencia con el resto del código, es una decisión defendible que el ecosistema tomó hace tiempo.

Tres estilos conviven, todos legítimos.

**`Metodo_Escenario_ResultadoEsperado`.** El más difundido en .NET, atribuido a Roy Osherove. Tres segmentos separados por guion bajo.

```csharp
[Fact]
public void Cancelar_ReservaYaIniciada_LanzaInvalidOperationException() { }

[Fact]
public void Confirmar_SalaOcupadaEnElIntervalo_DevuelveConflicto() { }
```

**`Should_X_When_Y`.** Formulación en prosa desde la perspectiva del comportamiento esperado, de origen BDD.

```csharp
[Fact]
public void Should_RechazarLaReserva_When_LaSalaEstaOcupada() { }
```

**Prosa con guiones bajos.** Una oración completa, sin estructura fija.

```csharp
[Fact]
public void Una_reserva_ya_iniciada_no_se_puede_cancelar() { }
```

La comparación práctica: el primero ordena alfabéticamente por método bajo prueba, lo que agrupa bien en el ejecutor; el tercero se lee sin esfuerzo en el informe de fallos y es el que mejor tolera el español. Cuál se elige importa menos que elegir uno: un proyecto de pruebas con los tres estilos mezclados pierde la propiedad que los hace útiles, que es poder leer la lista de tests como una especificación.

**Es decisión de `ACT-03`**, se registra junto con las demás convenciones del equipo, y a diferencia de casi todo lo de esta familia **no es automatizable**: ningún analizador verifica que un nombre de test describa el escenario. Depende de la revisión, y por eso conviene que la convención sea simple.

Los nombres de las **clases** de prueba sí siguen la convención normal de C#, sin guiones bajos, y la práctica dominante es reflejar el tipo bajo prueba: `ServicioReservasTests` o `ServicioReservasDeberia`.

---

## Nombrado en el dominio: el lenguaje ubicuo

Evans introduce en `O-03` el **lenguaje ubicuo**: un vocabulario único, construido entre el equipo técnico y los expertos del negocio, que se usa en las conversaciones, en los documentos y —esto es lo que aterriza en el código— en los nombres de los tipos y los métodos. No es un glosario que se escribe al principio y se archiva: es la propiedad de que no exista traducción entre lo que se dice y lo que se programa.

La prueba operativa es simple y sirve en cualquier revisión. **Tomar un nombre del código y decirlo en voz alta frente a alguien del negocio.** Si hay que explicarlo, el nombre está mal elegido.

```csharp
// El negocio dice "cancelar una reserva", "confirmar", "reprogramar".
public sealed class Reserva
{
    public void Confirmar(DateTimeOffset momento) { /* ... */ }
    public void Cancelar(MotivoCancelacion motivo) { /* ... */ }
    public void Reprogramar(RangoHorario nuevoPeriodo) { /* ... */ }
}

// El negocio no dice "eliminar una reserva" ni "actualizar el estado".
public sealed class ReservaService
{
    public void Delete(Guid id) { /* ... */ }
    public void UpdateStatus(Guid id, int status) { /* ... */ }
}
```

La diferencia entre ambos bloques no es de estilo. `Delete` y `Cancelar` describen operaciones distintas: una reserva cancelada sigue existiendo, conserva su historia, puede aparecer en un informe y quizá dispare una notificación. Cuando el método se llama `Delete`, esa diferencia deja de estar en el código y pasa a vivir en la cabeza de quien lo escribió. El día que alguien implemente el borrado real, no va a haber ningún nombre libre para distinguirlo.

Hay un límite que conviene marcar. El lenguaje ubicuo rige **dentro de un contexto delimitado**, y el mismo término puede significar cosas distintas en dos contextos —«reserva» en el módulo de agenda y «reserva» en el de facturación—. Cuando eso ocurre, la respuesta no es unificar el vocabulario a la fuerza sino separar los espacios de nombres, que es asunto de [`TEM-NS`](../30-Organizacion-Interna/Espacios-de-Nombres.md).

---

## Español o inglés

El criterio lo fija [`TEM-MODELOS`](../30-Organizacion-Interna/Modelos-y-Contratos.md), que es el dueño del tema y lo plantea como corresponde: no es una decisión de idioma sino dos, una por cada **plano**. El plano estructural nombra la arquitectura —los segmentos de espacio de nombres y los sufijos de rol— y el plano del dominio nombra el negocio. Se decide cada uno por separado, se registra, y la combinación más frecuente es estructura en inglés con dominio en el idioma del negocio. Acá no se repite ese desarrollo; lo que sigue es lo que el nombrado de tipos y miembros agrega una vez tomada la decisión.

**El vocabulario del framework no entra en la decisión.** Hay nombres que ninguna política elige: los que el compilador o el framework imponen. `Program`, `Dispose`, `MapPost`, `ConfigureServices`, `OnInitializedAsync`, `CancellationToken` conservan su forma porque son un miembro heredado, una convención de descubrimiento o una firma que hay que respetar. Es la tensión concreta que el lenguaje ubicuo tiene en el código: `O-03` pide que los nombres sean los del negocio, y una parte de la superficie no es nombrable por el equipo. La respuesta no es traducir lo intraducible ni renunciar al vocabulario del negocio, sino que la frontera entre ambos sea visible y esté en pocos lugares —típicamente el borde donde el framework invoca al código propio—.

**Los sufijos de rol siguen al plano estructural, no al del dominio.** `Service`, `Repository`, `Handler`, `Controller`, `Factory`, `Options` comunican un papel arquitectónico, y por eso se escriben en el idioma que se haya elegido para la estructura, aunque el sustantivo que los acompaña venga del negocio. Con estructura en inglés se escribe `ReservasService` e `IReservasRepository`; con estructura en español, `ServicioReservas` e `IRepositorioReservas`. Lo que no corresponde es tratarlos como parte del dominio y traducirlos uno por uno según qué palabra suene mejor.

Dos sufijos quedan fuera de la elección incluso con estructura en español, porque son normativos y derivan de un tipo base: `Exception` y `EventArgs` se escriben así siempre —`SalaNoDisponibleException`, `ReservaCanceladaEventArgs`—. Lo mismo vale para `Async` (`N-15`, `F-04`), que marca la forma del método y no es una palabra del vocabulario.

**Lo que no funciona es mezclar dentro del mismo plano.** Una clase `ReservaRepository` con métodos `ObtenerPorId` y `SaveChanges`, o un `ServicioReservas` con una propiedad `StartDate`, obliga a decidir el idioma en cada línea. Es el peor de los resultados posibles, porque acumula la fricción de ambos sin la coherencia de ninguno.

En `CTX-3` hay una consideración adicional que sí es propia del nombrado: la superficie pública de una biblioteca publicada más allá de la organización es contrato con gente que quizá no hable español, y eso pesa sobre la elección del plano estructural que `TEM-MODELOS` describe.

### El esquema aplicado — ejemplo sintético

Este ejemplo ilustra la variante con **el plano estructural en español**, que `TEM-MODELOS` admite como igualmente coherente; con estructura en inglés los mismos tipos serían `ReservasService` e `IReservasRepository`. El dominio y los tipos de aplicación e infraestructura están en español —`Sala`, `Reserva`, `ServicioReservas`, `IRepositorioReservas`, y las carpetas `Dominio/`, `Aplicacion/`, `Infraestructura/`—, mientras que la superficie que toca el framework está en inglés porque no puede estar de otra manera: `Program`, `MapPost`, la carpeta `Components/` que Blazor espera.

```csharp
// Program.cs — la superficie de framework está en inglés
app.MapPost("/api/reservas", async (
    SolicitudReserva solicitud,               // tipo propio, español
    ServicioReservas servicio,                // tipo propio, español
    CancellationToken cancellationToken) =>   // parámetro del framework, inglés
{
    var resultado = await servicio.ReservarAsync(solicitud, cancellationToken);
    return resultado.EsExitoso ? Results.Ok(resultado) : Results.Conflict(resultado.Motivo);
});
```

`MapPost`, `Results` y `CancellationToken` no admiten traducción; `SolicitudReserva`, `ServicioReservas` y `EsExitoso` son decisión del equipo. La línea donde ambos se encuentran es visible y está en un solo lugar, que es lo que hace que el esquema funcione. Un `ServicioReservas` con un método `Book()` sería el fracaso del mismo esquema: la mezcla pasa a estar dentro del mismo plano en vez de en su borde, y a partir de ahí cada miembro nuevo vuelve a abrir la discusión.

Un detalle que el ejemplo también exhibe: la ruta HTTP está en español, y si tuviera más de una palabra iría en kebab-case —`/api/reservas/salas-disponibles`— y no en el PascalCase del tipo que la sirve. Es coherente con el resto —el contrato lo consumen clientes de la misma organización— y con la regla de que cada plano sigue la convención de su medio ([`TEM-CAPS`](Capitalizacion.md)).

---

## Nombrado de componentes Blazor — `CTX-1`

Un componente `.razor` no es un archivo cualquiera: el compilador genera a partir de él una clase parcial cuyo nombre es el del archivo. **El nombre de archivo y el nombre del tipo deben coincidir exactamente**, y esa correspondencia no es estilística sino un requisito de la generación.

```text
Components/
  Pages/
    ListaDeSalas.razor            → clase ListaDeSalas
    ListaDeSalas.razor.cs         → partial class ListaDeSalas  (code-behind)
    ListaDeSalas.razor.css        → estilos con ámbito del componente
    DetalleReserva.razor          → clase DetalleReserva
  Shared/
    SelectorDeFranja.razor        → clase SelectorDeFranja
```

Consecuencias prácticas. Un componente en `ListaDeSalas.razor` se usa en otro componente como `<ListaDeSalas />`, de modo que el nombre de archivo termina siendo el nombre de la etiqueta y conviene que lea bien como tal. El archivo de código subyacente lleva el nombre completo con `.razor.cs`, no `ListaDeSalas.cs`, para que las herramientas lo agrupen. Y un archivo en kebab-case —`lista-de-salas.razor`— produce un tipo inválido: la convención de la web no entra acá, porque del otro lado del archivo hay una clase C#.

El sufijo `Component` no se acostumbra en Blazor. `ListaDeSalas` es preferible a `ListaDeSalasComponent`, que repite en el nombre lo que la extensión ya dice.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

Se decide la política y se registra: idioma del dominio, sufijos convencionales adoptados y su significado, y estilo de nombrado de tests. Tres decisiones, media hora, y la salida es un párrafo en el documento de convenciones del equipo.

Hay un error característico de este escenario: fijar el vocabulario del dominio antes de conocer el dominio. Los nombres de la primera semana son hipótesis, y conviene tratarlas como tales —revisarlas cuando el modelo se estabiliza, mientras renombrar sigue siendo barato porque nadie depende de nada—. Lo que sí conviene fijar el primer día es la política, que es distinta del vocabulario.

### `ESC-2` — Evolución estructural

Cada extracción de módulo es una oportunidad de corregir vocabulario, y también un riesgo. La oportunidad: al mover código a un proyecto propio se lo lee entero, y es cuando se detecta que tres clases usan tres verbos distintos para lo mismo. El riesgo: mezclar el renombrado con el movimiento produce un diff donde no se distingue qué se movió de qué se cambió, y nadie lo revisa de verdad.

La práctica que esta guía recomienda es separarlo en dos commits: primero mover sin tocar nombres, después renombrar. En ese orden, porque la herramienta de renombrado funciona mejor cuando el código ya está donde va a quedar.

Cuando la extracción convierte tipos `internal` en `public`, cambia el régimen: pasan a estar bajo el rigor de `CTX-3` y conviene revisar sus nombres con ese criterio antes de publicarlos, no después.

### `ESC-3` — Normalización de código existente

Es el escenario más caro de esta familia, porque renombrar exige criterio y no se puede delegar en una herramienta. Ningún analizador sabe que `ProcesarDatos` debería llamarse `ConfirmarReserva`.

La secuencia que esta guía recomienda empieza por levantar el inventario de sinónimos: buscar todos los verbos que se usan para las operaciones principales y encontrar que hay `Obtener`, `Traer`, `Buscar`, `Consultar` y `Get` conviviendo. Ese inventario es en sí mismo un entregable útil, incluso antes de tocar nada. Después se fija el vocabulario canónico —una palabra por operación— y recién ahí se renombra, de a un verbo por vez.

El límite duro sigue siendo el contexto. En `CTX-3` un renombre público no se hace: se marca el miembro anterior con `[Obsolete]` indicando el reemplazo, se publica el nuevo, y el viejo se retira en la próxima versión mayor.

```csharp
[Obsolete("Use CancelarAsync. Se retira en la versión 3.0.")]
public Task EliminarAsync(Guid reservaId, CancellationToken ct)
    => CancelarAsync(reservaId, MotivoCancelacion.Solicitud, ct);
```

### `ESC-4` — Evaluación de código ajeno

El nombrado es lo más evaluable de esta guía sin conocer el contexto, porque las inconsistencias se ven leyendo y no requieren saber por qué se decidió nada.

Cuatro verificaciones ordenan una evaluación rápida. Si hay más de un verbo para la misma operación, hay un vocabulario sin dueño. Si los nombres del código no se parecen a los términos que usa el negocio, hay una traducción permanente que alguien está sosteniendo. Si conviven dos idiomas dentro del mismo plano, no hubo decisión. Y si aparece algún nombre que miente —un `Obtener` que persiste, un `Validar` que además notifica— eso se señala sin consultar a nadie, porque es un defecto en sentido estricto y no una convención distinta de la propia.

Lo que **no** corresponde señalar en `ESC-4` es la elección de idioma. Un revisor que objeta el español en un repositorio consistentemente en español está imponiendo su preferencia, que es lo que `ACT-04` tiene explícitamente vedado.

### Qué cambia según el contexto

**`CTX-1`.** Se suma la correspondencia entre archivo `.razor` y tipo generado, y el nombre del componente pasa a ser también el nombre de la etiqueta de marcado.

**`CTX-2`.** El nombrado del contrato HTTP es tan importante como el del código y sigue otras reglas: rutas en kebab-case, sustantivos en plural para las colecciones, verbos expresados por el método HTTP en lugar de por la ruta. `POST /reservas/{id}/cancelaciones` en lugar de `POST /reservas/cancelarReserva`.

**`CTX-3`.** Todo nombre público es permanente en la práctica. Es el contexto donde `N-01` a `N-04` aplican en su sentido literal y donde conviene pasar cada nombre público por una revisión explícita antes de la primera publicación, porque después el costo se traslada a terceros.

**`CTX-4`.** Aparece el nombrado de los mensajes, que es contrato entre servicios sin verificación de compilador. La convención dominante nombra los eventos de integración en pasado —`ReservaConfirmada`, `SalaDadaDeBaja`— porque un evento comunica un hecho consumado, mientras que los comandos van en imperativo —`ConfirmarReserva`—. La distinción tiene consecuencia: un evento no se rechaza, un comando sí.

---

## Ejemplos concretos

### El mismo tipo, mal y bien nombrado

```csharp
// Antes: nombres que no permiten anticipar nada
public class ReservaManager
{
    public bool Process(ReservaData d, int t)
    {
        var tmp = _repo.GetAll().Where(x => x.SId == d.SId);
        return tmp.Any();
    }
    public List<ReservaData> GetData(int i, bool f) => _repo.GetAll();
    public void DoUpdate(ReservaData d) => _repo.Save(d);
}
```

```csharp
// Después: cada nombre es una afirmación verificable
public sealed class ServicioReservas
{
    public Task<ResultadoReserva> ConfirmarAsync(
        SolicitudReserva solicitud,
        CancellationToken cancellationToken) => _confirmacion.EjecutarAsync(solicitud, cancellationToken);

    public Task<IReadOnlyList<Reserva>> ObtenerPorSalaAsync(
        Guid salaId,
        bool incluirCanceladas,
        CancellationToken cancellationToken) => _repositorio.ObtenerPorSalaAsync(salaId, incluirCanceladas, cancellationToken);

    public Task ReprogramarAsync(
        Guid reservaId,
        RangoHorario nuevoPeriodo,
        CancellationToken cancellationToken) => _reprogramacion.EjecutarAsync(reservaId, nuevoPeriodo, cancellationToken);
}
```

Lo que cambió, y por qué en cada caso. `Manager` desapareció porque no describía ninguna responsabilidad (`TEM-ANTI`). `Process` se convirtió en tres métodos con verbos distintos, porque hacía tres cosas y el nombre genérico lo ocultaba. El parámetro `int t` —presumiblemente un tipo o un umbral— se volvió explícito. `GetData` y `DoUpdate` adoptaron el verbo del negocio. Y el sufijo `Async` apareció donde corresponde por `F-04`.

### Un tipo de dominio completo

```csharp
namespace MiEmpresa.Reservas.Dominio.Reservas;

/// <summary>Reserva de una sala en un rango horario, con su ciclo de vida.</summary>
public sealed class Reserva
{
    public Guid Id { get; }
    public Guid SalaId { get; }
    public RangoHorario Periodo { get; private set; }
    public string Solicitante { get; private set; }
    public EstadoReserva Estado { get; private set; }

    // Booleanas con la construcción equivalente a Is/Can/Has
    public bool EstaConfirmada => Estado is EstadoReserva.Confirmada;
    public bool PuedeCancelarse => Estado is not EstadoReserva.Cancelada
                                   && Periodo.Inicio > DateTimeOffset.UtcNow;
    public bool TieneAsistentes => _asistentes.Count > 0;

    // Eventos: gerundio antes, participio después
    public event EventHandler<CancelacionEventArgs>? Cancelando;
    public event EventHandler<ReservaEventArgs>? Cancelada;

    // Métodos: verbos del negocio, no operaciones CRUD
    public void Confirmar(DateTimeOffset momento) { /* ... */ }
    public void Cancelar(MotivoCancelacion motivo) { /* ... */ }
    public void Reprogramar(RangoHorario nuevoPeriodo) { /* ... */ }
}
```

Lo que se espera de este código: que alguien del negocio, sin saber C#, reconozca en la lista de métodos las operaciones que describiría al explicar el sistema. Esa es la prueba del lenguaje ubicuo (`O-03`) y es la única verificación real de que los nombres están bien elegidos.

### Los cuatro planos de un mismo concepto

```csharp
// Dominio — español, lenguaje ubicuo
public sealed class Reserva { public void Cancelar(MotivoCancelacion motivo) { /* ... */ } }

// Aplicación — español, verbo del negocio
public sealed class ServicioReservas
{
    public Task<ResultadoReserva> CancelarAsync(Guid reservaId, MotivoCancelacion motivo, CancellationToken ct)
        => _cancelacion.EjecutarAsync(reservaId, motivo, ct);
}

// Endpoint — el verbo lo expresa el método HTTP, la ruta en kebab-case
app.MapPost("/api/reservas/{reservaId:guid}/cancelaciones", ...);

// Componente Blazor — archivo y tipo coinciden
// Components/Pages/CancelarReserva.razor → clase CancelarReserva
```

Un mismo concepto atraviesa cuatro planos y cada uno respeta su convención sin contaminar a los demás. El vocabulario —«cancelar»— es el mismo en los cuatro, y esa continuidad es lo que permite seguir una funcionalidad de punta a punta buscando una sola palabra.

---

## Preguntas guía

1. Este nombre, ¿lo diría alguien del negocio? Si tengo que explicarlo, ¿por qué?
2. ¿El nombre afirma algo que el código no hace, o deja de afirmar algo que sí hace?
3. ¿Cuántos verbos distintos usa este sistema para la misma operación? ¿Cuál es el canónico y está escrito en algún lado?
4. Los tipos, ¿son sustantivos? Los métodos, ¿verbos? ¿Hay algún tipo cuyo nombre sea una acción, y qué indica eso sobre su diseño?
5. ¿El idioma del dominio fue una decisión o un accidente? ¿Es consistente dentro de cada plano?
6. ¿Los sufijos que usa este código comunican algo, o son relleno? ¿`Service` está diciendo algo que un nombre específico diría mejor?
7. Si esto es `CTX-3`: ¿podría defender cada nombre público dentro de cinco años, sabiendo que cambiarlo exige una versión mayor?
8. ¿El estilo de nombrado de tests está decidido y es uno solo?
9. Si esto es `CTX-1`: ¿el nombre de cada archivo `.razor` coincide exactamente con el del tipo, y lee bien como etiqueta de marcado?
10. ¿Revisé estos nombres después de terminar el código, o quedaron los de la primera versión?

---

## Criterios de calidad

Una aplicación buena se reconoce por una prueba: **alguien que conoce el negocio y no el código puede leer los nombres de los tipos y métodos de una capa de dominio y reconocer de qué se trata el sistema.** Si esa lectura exige traducción, el vocabulario está desalineado por más correctas que sean las reglas de capitalización.

**Un verbo por operación.** El vocabulario es único en todo el sistema. Si hay `Obtener` en un lado y `Traer` en otro para lo mismo, no hay vocabulario sino acumulación.

**Los nombres no mienten.** Lo que el nombre afirma es lo que el código hace. Un `Obtener` no escribe, un `Validar` no notifica, un `Calcular` no persiste.

**La categoría gramatical corresponde.** Tipos con sustantivos, métodos con verbos, propiedades con sustantivos o adjetivos, eventos con verbos conjugados (`N-04`).

**Los sufijos normativos son exactos en las dos direcciones.** Todo lo que deriva de `Exception` lo lleva; nada que no derive de `Exception` lo lleva.

**El idioma es una decisión registrada, no un accidente.** Y es consistente dentro de cada plano.

**Los nombres se revisaron después de escribir el código.** El buen nombre casi nunca es el primero; aparece cuando se entendió el problema.

### Antipatrones nombrados

Cada uno con su desarrollo, ejemplo malo y corrección en [`TEM-ANTI`](Antipatrones-de-Nombrado.md):

**Nombres genéricos vacíos** — `SalaManager`, `ReservaHelper`, `Utils`, `DataProcessor`. La ausencia de un nombre específico casi siempre señala que no se identificó la responsabilidad.

**Nombres que mienten** — un `ObtenerSala` que además persiste; un `Get` que muta estado.

**Abreviaturas y contracciones** — `usr`, `cnt`, `tmp`, `mgr`, `proc`. Prohibidas por `N-03`.

**Inconsistencia de sinónimos** — `Obtener`, `Traer`, `Buscar` y `Consultar` para la misma operación en clases distintas.

**Notación húngara** — `strNombre`, `iContador`. Prohibida explícitamente por `N-03`.

**Sufijo `Impl`** — `ServicioReservasImpl` como única implementación de `IServicioReservas`, que suele indicar que la interfaz no hacía falta.

**Idiomas mezclados dentro de un plano** — `ReservaRepository.ObtenerPorId()`. Ninguna de las dos opciones y la fricción de ambas.

**CRUD sobre un dominio que no es CRUD** — `Delete` donde el negocio dice «cancelar». Es el antipatrón que más silenciosamente destruye el lenguaje ubicuo, porque cada método por separado parece razonable.
