---
doc_id: TEM-ANTI
doc_type: tema
title: Antipatrones de nombrado
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-NOM, TEM-CAPS, TEM-NOMB, TEM-AUTO, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Antipatrones de nombrado — `TEM-ANTI`

## Resumen ejecutivo

Un antipatrón con nombre es un antipatrón discutible. «Este nombre no me gusta» abre una negociación de gustos donde gana quien tiene más energía; «esto es notación húngara, que `N-03` prohíbe explícitamente» cierra la conversación en una línea y sin que nadie se sienta juzgado. Ese es el propósito de este documento: dar vocabulario para las revisiones de código, de modo que lo que se discuta sea el defecto y no la persona.

Los doce antipatrones que siguen se ordenan de más a menos verificable. Los primeros están prohibidos por escrito en `N-03` y no admiten discusión de fondo. Los del medio son herencia de otros lenguajes y otras épocas, desaconsejados por consenso amplio. Los últimos —el nombre genérico vacío, el sinónimo inconsistente— no violan ninguna norma y sin embargo hacen más daño acumulado que todos los anteriores, porque no se detectan leyendo un archivo sino leyendo diez.

Cada entrada trae el defecto, por qué importa, y un par de bloques con la versión mala y su corrección. Sirve a `ACT-04` durante la revisión, a `ACT-02` como filtro antes de proponerla, y a `ACT-03` como catálogo de lo que conviene automatizar.

---

## Definición

### Qué es

Un patrón de nombrado que se repite lo suficiente como para tener nombre propio y que degrada la legibilidad o la corrección del código. Se distingue del simple error porque es **sistemático**: no es un nombre desafortunado sino una manera de nombrar que se aplica de forma consistente y produce daño de forma consistente.

### Qué problema resuelve tenerlos catalogados

Convierte la revisión de código en una verificación en lugar de una negociación. `ACT-04` tiene explícitamente vedado imponer su preferencia durante una revisión; lo que sí le corresponde es señalar el incumplimiento de lo acordado y los defectos objetivos. Un catálogo de antipatrones con nombre y con nivel de autoridad traza esa frontera: lo que está en `N-03` se señala sin más, lo que es criterio del equipo se señala si el equipo lo adoptó, y lo demás no se señala.

### Qué NO es

**No es una lista de prohibiciones absolutas.** Varios de estos antipatrones tienen usos legítimos en contextos acotados —una variable `i` en un bucle de tres líneas, un `Base` que sí es una clase base—. El antipatrón es el uso sistemático fuera de esos casos.

**No es un sustituto del diseño.** Varios de estos nombres son síntomas y no enfermedades. `SalaManager` no se arregla renombrando a `SalaService`; se arregla identificando qué hace realmente esa clase, que casi siempre resulta ser más de una cosa.

---

## 1. Notación húngara

**Nivel: prohibido por `N-03`,** con la fórmula literal «DO NOT use Hungarian notation».

### Qué es y de dónde viene

Consiste en prefijar el identificador con una abreviatura que codifica información sobre él. La creó **Charles Simonyi**, húngaro, durante su trabajo en Xerox PARC en los años setenta, y la llevó a Microsoft, donde se convirtió en el estilo dominante de las APIs de Windows y de las bibliotecas de C++ de esa época. `lpszName`, `hwndMain`, `dwFlags` son notación húngara, y quien haya visto la firma de una función de la API de Win32 la reconoce.

### El matiz que casi nunca se explica

Hay **dos notaciones húngaras**, y la que se prohíbe no es la que Simonyi propuso.

**Húngara de sistemas** —la que se prohíbe—. El prefijo codifica el **tipo de dato**: `str` para cadena, `i` para entero, `arr` para arreglo, `b` para booleano. `strNombre`, `iContador`, `arrSalas`, `bEstaActiva`.

**Húngara de aplicaciones** —la intención original—. El prefijo codifica el **significado o la clase de uso** del valor, información que el tipo no captura. En el ejemplo clásico, `usName` para una cadena de origen no confiable y `sName` para una ya saneada: ambas son `string`, y el prefijo distingue lo que el compilador no puede distinguir. Bajo esa convención, `strOutput = strInput` se ve mal a simple vista, que es exactamente lo que la notación buscaba.

La distinción está bien documentada en la literatura del oficio —la difundió Joel Spolsky en *Making Wrong Code Look Wrong* (2005)— y no figura en [`ANEXO-REFERENCIAS`](../99-Anexos/Referencias.md); se cita acá como contexto histórico verificable y no como fuente normativa.

### Por qué la de sistemas es dañina

Repite información que el compilador ya tiene y el IDE ya muestra al pasar el cursor, con dos agravantes. Si el tipo cambia y el nombre no —de `int` a `long`, de `string` a un tipo propio— el prefijo pasa a mentir, y mentir es peor que no decir nada. Y con genéricos, tipos anónimos, `var` y tuplas, la mitad de los valores de un archivo moderno de C# no tiene un prefijo obvio que ponerles.

La húngara de aplicaciones sigue teniendo una intuición valiosa; lo que cambió es la forma de aplicarla. Hoy la distinción entre una cadena confiable y una que no lo es se expresa con **tipos**, no con prefijos, y el compilador la verifica en lugar de dejarla en manos del lector.

```csharp
// Húngara de sistemas — prohibida por N-03
string strNombreSala;
int iCantidadAsistentes;
bool bEstaConfirmada;
List<Reserva> lstReservas;
```

```csharp
// Corrección: el tipo ya está en la declaración
string nombreSala;
int cantidadAsistentes;
bool estaConfirmada;
List<Reserva> reservas;
```

```csharp
// La intuición de la húngara de aplicaciones, resuelta con tipos
public sealed record HtmlSaneado(string Valor);

// El compilador impide pasar una cadena cruda donde se espera contenido saneado.
public void Publicar(HtmlSaneado contenido) { ... }
```

**La excepción con nombre.** El prefijo `I` en interfaces (`N-01`) es técnicamente notación húngara y está prescrito por la misma obra que la prohíbe. La contradicción es conocida y no cambia nada en la práctica: la convención está asentada en toda la plataforma.

---

## 2. Prefijo `C` en clases

**Nivel: desaconsejado; herencia de C++/MFC.**

`CReserva`, `CSalaManager`, `CDocument`. Viene de las bibliotecas MFC de Microsoft, donde el prefijo distinguía las clases de la biblioteca de los tipos primitivos de C y de los `struct` de la API de Windows, en una época en que un archivo mezclaba los tres.

En C# no distingue nada. Todo es un tipo, el IDE los colorea distinto, y el prefijo solo consigue que la lista alfabética de tipos de un ensamblado agrupe todo bajo la letra C.

```csharp
public class CReserva { }
public class CRepositorioSalas { }
```

```csharp
public class Reserva { }
public class RepositorioSalas { }
```

---

## 3. Prefijo `m_` en miembros

**Nivel: desaconsejado; la convención de facto en .NET es `_` (`F-02`).**

`m_reserva`, `m_contador`. El mismo origen que el anterior: en C++ el prefijo distinguía los miembros de la clase de las variables locales, y era útil porque en C++ no había una forma corta de referirse al miembro. En C# la hay —`this.`— y además existe una convención dominante que hace lo mismo con un carácter en lugar de dos.

Sus parientes son `s_` para estático y `g_` para global. El primero **sí** se usa en .NET, pero con otro origen y otro alcance: es estilo de `dotnet/runtime` (`F-03`) y no del linaje MFC. El segundo no tiene equivalente porque C# no tiene variables globales.

```csharp
public class ServicioReservas
{
    private readonly IRepositorioSalas m_repositorio;
    private int m_contadorIntentos;
}
```

```csharp
public class ServicioReservas
{
    private readonly IRepositorioSalas _repositorio;
    private int _contadorIntentos;
}
```

Lo que hace daño no es `m_` en sí sino la mezcla. Un repositorio con `m_` en el código viejo, `_` en el nuevo y nada en el del medio obliga a que cada lector recuerde tres convenciones, y es un hallazgo típico de `ESC-4`.

---

## 4. Abreviaturas y contracciones

**Nivel: prohibido por `N-03`,** que descarta el uso de abreviaturas y contracciones como parte de los nombres de identificador.

`usr`, `cnt`, `tmp`, `mgr`, `proc`, `calc`, `qty`, `desc`, `num`, `msg`, `btn`, `idx`. Se justifican con el argumento de que ahorran escritura, que dejó de valer cuando los editores completaron nombres automáticamente, y que nunca valió mucho: un identificador se escribe una vez y se lee cientos.

Tres daños concretos. La abreviatura es ambigua —`desc` es *descripción* o *descendente*, `proc` es *procesar* o *procedimiento*—. Es individual: cada quien abrevia distinto, y el repositorio termina con `cant`, `cnt` y `qty` para lo mismo. Y rompe la búsqueda, porque `grep cantidad` no encuentra `cnt`.

```csharp
public class RsvMgr
{
    public void ProcRsv(RsvData d, int qty, string desc) { }
    public int GetCntBySala(Guid sId) => ...;
    private readonly Dictionary<Guid, RsvData> _tmpCache = new();
}
```

```csharp
public class ServicioReservas
{
    public void ProcesarReserva(SolicitudReserva solicitud, int cantidadAsistentes, string descripcion) { }
    public int ContarPorSala(Guid salaId) => ...;
    private readonly Dictionary<Guid, Reserva> _reservasEnCurso = new();
}
```

**Lo que sí se acepta.** Las siglas ya asentadas como término técnico —`Http`, `Xml`, `Json`, `Uri`, `Sql`, `Db`— y que se capitalizan según la regla de acrónimos de [`TEM-CAPS`](Capitalizacion.md). También `Id`, que es abreviatura de *identifier* pero está universalmente asentada en la biblioteca base. La prueba práctica: si la abreviatura aparece en la documentación oficial de .NET como parte de un nombre de tipo, es vocabulario; si la inventó el equipo, es abreviatura.

---

## 5. Nombres genéricos vacíos

**Nivel: criterio propio; esta guía recomienda tratarlos como señal de diseño incompleto.**

`Manager`, `Helper`, `Utils`, `Utilities`, `Data`, `Info`, `Common`, `Shared`, `Processor`, `Handler` sin más, `Base`, `Misc`. No violan ninguna norma y son los que más daño acumulan, porque no se detectan leyendo una clase sino observando cómo crece.

### Por qué `SalaManager` es un problema

Un nombre describe una responsabilidad. `Manager` no describe ninguna: gestionar es lo que hace cualquier cosa que hace algo. Cuando una clase se llama `SalaManager`, lo que ocurrió es que nadie pudo decir qué hace, y el nombre registra esa incapacidad en lugar de resolverla.

El daño es que el nombre **no ofrece resistencia**. Frente a un método nuevo relacionado con salas, la pregunta «¿va en `SalaManager`?» tiene siempre la misma respuesta, porque todo lo relacionado con salas cabe en algo que las gestiona. Un año después la clase tiene mil doscientas líneas, cuatro dependencias inyectadas y tres razones distintas para cambiar. Un nombre específico —`ValidadorDisponibilidadSala`— rechaza el método que no le corresponde, y ese rechazo es lo que mantiene el diseño en su lugar.

`Utils` y `Helper` producen la variante estática del mismo problema: una clase que acumula funciones sueltas sin criterio, que nadie puede probar en aislamiento y que termina siendo dependencia de medio sistema.

### Qué hacer en su lugar

**Nombrar la responsabilidad.** Si la clase valida, es un validador; si calcula, es un calculador; si traduce entre representaciones, es un traductor o un asignador.

**Si no se puede nombrar en pocas palabras, son varias clases.** La dificultad para encontrar el nombre es el diagnóstico, no el obstáculo. Una clase que necesita una conjunción para describirse —«gestiona salas **y** valida disponibilidad»— son dos.

**Si son funciones sueltas sobre un tipo, van como métodos de extensión** en una clase nombrada por lo que extiende: `ReservaExtensions`, no `ReservaHelper`.

```csharp
// Un año después de haberse llamado "Manager"
public class SalaManager
{
    public bool ValidarDisponibilidad(Guid salaId, RangoHorario periodo) { /* ... */ }
    public decimal CalcularCosto(Guid salaId, TimeSpan duracion) { /* ... */ }
    public void EnviarNotificacion(Guid salaId, string mensaje) { /* ... */ }
    public SalaDto MapearADto(Sala sala) { /* ... */ }
    public List<Sala> BuscarPorAforo(int minimo) { /* ... */ }
}
```

```csharp
// Cada responsabilidad con su nombre; cada nombre rechaza lo que no le toca
public sealed class VerificadorDisponibilidad
{
    public bool EstaDisponible(Guid salaId, RangoHorario periodo) => _agenda.SinSolapamiento(salaId, periodo);
}

public sealed class TarifadorDeSalas
{
    public decimal CalcularCosto(Guid salaId, TimeSpan duracion) => _tarifa.Por(salaId) * (decimal)duracion.TotalHours;
}

public sealed class NotificadorDeReservas
{
    public Task NotificarAsync(Guid salaId, string mensaje, CancellationToken ct)
        => _correo.EnviarAsync(salaId, mensaje, ct);
}

public static class SalaExtensions
{
    public static SalaDto ADto(this Sala sala) => new(sala.Id, sala.Nombre, sala.Aforo);
}
```

**Dos excepciones legítimas.** `Handler` es específico cuando nombra el manejador de algo concreto —`ConfirmarReservaHandler`—; el vacío es `Handler` a secas. Y `Common`/`Shared` como nombre de **proyecto** en una solución es corriente, aunque conviene desconfiar: un proyecto llamado así tiende a acumular todo lo que nadie supo dónde poner, y el problema es idéntico al de la clase.

---

## 6. Nombres que mienten

**Nivel: defecto objetivo; se señala en revisión sin consultar a nadie.**

Un nombre es una afirmación. `ObtenerSala` afirma dos cosas: que devuelve una sala, y —por la convención universal de que un `Get` es una consulta— que no cambia nada observable. Cuando el método hace algo más, la afirmación es falsa.

```csharp
// Miente: obtiene, y además persiste y notifica
public Sala ObtenerSala(Guid salaId)
{
    var sala = _repositorio.Buscar(salaId);
    sala.UltimoAcceso = DateTimeOffset.UtcNow;
    _repositorio.GuardarCambios();              // efecto no anunciado
    _auditoria.Registrar(salaId);               // efecto no anunciado
    return sala;
}
```

```csharp
// Cada nombre afirma lo que hace
public Sala ObtenerSala(Guid salaId) => _repositorio.Buscar(salaId);

public void RegistrarAcceso(Guid salaId)
{
    _repositorio.ActualizarUltimoAcceso(salaId, DateTimeOffset.UtcNow);
    _auditoria.Registrar(salaId);
}
```

Es el antipatrón más peligroso de los doce por una razón: **es indetectable leyendo el punto de llamada**. Los otros once se ven en el código que los contiene; este se ve solo abriendo la implementación, que es exactamente lo que un buen nombre debería ahorrar. Quien llame a `ObtenerSala` dentro de un bucle está produciendo una escritura por iteración sin sospecharlo.

Las variantes que conviene reconocer: el `Get` que muta —el caso anterior—; el `Validar` que además corrige el objeto en lugar de solo verificarlo; el `Calcular` que persiste el resultado; el `Cancelar` que borra en lugar de cambiar el estado; y el booleano invertido, `EstaDeshabilitada` que devuelve `true` cuando está habilitada.

Un caso especial que aparece a menudo en `CTX-2`: el método asíncrono sin sufijo `Async`, o el sufijo `Async` en un método que devuelve un valor sincrónicamente. El sufijo es una afirmación sobre el modelo de ejecución (`F-04`) y también puede ser falsa.

---

## 7. Números mágicos y nombres de un carácter

**Nivel: criterio propio, con consenso amplio.**

Un literal sin nombre en medio del código obliga a reconstruir su significado desde el contexto, y cuando el mismo valor aparece en tres lugares, nadie puede saber si son el mismo concepto o una coincidencia.

```csharp
if (duracion.TotalHours > 8) throw new InvalidOperationException();
if (asistentes.Count > 8) EnviarAlerta();
await Task.Delay(500);
if (r.E == 3) { }
```

```csharp
public const int DuracionMaximaHoras = 8;
public const int AforoSalaEstandar = 8;
private static readonly TimeSpan EsperaEntreReintentos = TimeSpan.FromMilliseconds(500);

if (duracion.TotalHours > DuracionMaximaHoras) throw new ReservaDemasiadoLargaException();
if (asistentes.Count > AforoSalaEstandar) EnviarAlerta();
await Task.Delay(EsperaEntreReintentos);
if (reserva.Estado is EstadoReserva.Cancelada) { }
```

Los dos ochos del ejemplo malo son valores iguales de conceptos distintos. Con nombres, el día que la capacidad estándar pase a diez, nadie va a cambiar de paso la duración máxima.

Sobre los nombres de un carácter: `i`, `j`, `x` son aceptables en bucles cortos donde la declaración y el uso caben en la misma pantalla, y `e` para el argumento de un `catch` está tan asentado que cambiarlo genera más ruido del que quita. Fuera de eso —un `d` que vive treinta líneas, un `r` que es parámetro de un método público— el ahorro no existe y el costo se paga en cada lectura. `x` y `t` en expresiones lambda cortas también son legítimos; en una lambda de diez líneas, no.

---

## 8. Sufijo `Impl`

**Nivel: criterio propio; esta guía recomienda tratarlo como señal, no como error.**

`ServicioReservasImpl`, `RepositorioSalasImpl`. Es herencia de Java, donde la convención `IFoo`/`FooImpl` circuló durante años.

El defecto no es estético. `Impl` no dice **qué** implementación es, y solo hace falta distinguirla cuando hay más de una. Cuando hay una sola, lo que el nombre está registrando es que se creó una interfaz sin necesidad —«por si acaso», o porque un marco de pruebas viejo exigía interfaces para simular—, y el sufijo es la cicatriz de esa decisión.

```csharp
public interface IServicioReservas { }
public class ServicioReservasImpl : IServicioReservas { }   // única implementación
```

```csharp
// Si hay una sola implementación: probablemente no hace falta la interfaz.
public sealed class ServicioReservas { }

// Si hay varias: cada nombre dice qué la distingue.
public interface IRepositorioSalas { }
public sealed class RepositorioSalasEfCore : IRepositorioSalas { }
public sealed class RepositorioSalasEnMemoria : IRepositorioSalas { }
```

La segunda forma tiene un beneficio adicional que se nota al leer la configuración de inyección de dependencias: `AddScoped<IRepositorioSalas, RepositorioSalasEfCore>()` dice qué se está eligiendo, mientras que `AddScoped<IRepositorioSalas, RepositorioSalasImpl>()` no dice nada.

---

## 9. Prefijo `Base` mal usado

**Nivel: criterio propio.**

`Base` es correcto cuando el tipo **es** una clase base abstracta destinada a heredarse, y la biblioteca base lo usa así: `ControllerBase`, `DbContext` no, pero `TextWriter` y `Stream` cumplen el papel sin el prefijo. Nótese que la convención de .NET lo pone como **sufijo**, no como prefijo.

Se usa mal en dos formas. La primera es `BaseReserva` para un tipo que nadie hereda y que en realidad es un caso concreto; el prefijo anuncia una jerarquía inexistente. La segunda es más sutil: `BaseServicio` con métodos protegidos que las subclases usan como si fueran una biblioteca de funciones. Eso no es herencia sino reutilización disfrazada, y produce jerarquías donde la clase base acumula todo lo común de subclases que no tienen nada que ver entre sí.

```csharp
public class BaseServicio
{
    protected ILogger Logger { get; }
    protected void RegistrarError(Exception ex) { }
    protected string FormatearFecha(DateTimeOffset f) { }   // ¿qué hace esto acá?
}
public class ServicioReservas : BaseServicio { }
public class ServicioNotificaciones : BaseServicio { }
```

```csharp
// La utilidad compartida se inyecta o se extiende; no se hereda.
public sealed class ServicioReservas
{
    private readonly ILogger<ServicioReservas> _registro;
    public ServicioReservas(ILogger<ServicioReservas> registro) => _registro = registro;
}

// Y si sí hay una jerarquía real, el sufijo va al final, como en la BCL.
public abstract class ManejadorDeComandoBase<TComando> { }
```

---

## 10. Inconsistencia de sinónimos

**Nivel: criterio propio, coherente con el espíritu de `N-03`.**

Es el antipatrón que peor se detecta y más se paga. Cada nombre por separado es correcto; el defecto está en el conjunto, y solo aparece cuando alguien lee varias clases seguidas.

```csharp
public interface IRepositorioSalas          { Sala         ObtenerPorId(Guid id); }
public interface IRepositorioReservas       { Reserva      TraerPorId(Guid id); }
public interface IRepositorioUsuarios       { Usuario      BuscarPorId(Guid id); }
public interface IRepositorioNotificaciones { Notificacion ConsultarPorId(Guid id); }
public interface IRepositorioAuditoria      { Registro     GetById(Guid id); }
```

```csharp
// Un verbo por operación, en todo el sistema
public interface IRepositorioSalas          { Sala         ObtenerPorId(Guid id); }
public interface IRepositorioReservas       { Reserva      ObtenerPorId(Guid id); }
public interface IRepositorioUsuarios       { Usuario      ObtenerPorId(Guid id); }
public interface IRepositorioNotificaciones { Notificacion ObtenerPorId(Guid id); }
public interface IRepositorioAuditoria      { Registro     ObtenerPorId(Guid id); }
```

Los cinco costos son concretos. La búsqueda por texto deja de encontrar todo. La autocompletar deja de ayudar, porque hay que recordar qué verbo usa cada clase. Un desarrollador nuevo asume que verbos distintos significan operaciones distintas y busca la diferencia hasta convencerse de que no existe. La interfaz común se vuelve imposible sin renombrar. Y aparecen métodos duplicados, porque quien busca `Obtener` en una clase que usa `Traer` concluye que no existe y lo escribe otra vez.

El vocabulario canónico se decide una vez, se escribe, y se verifica en revisión. Un conjunto mínimo alcanza: `Obtener` para el acceso por clave, `Buscar` para consulta con criterios que puede no devolver nada, `Listar` para colecciones, `Crear`, `Actualizar`, `Cancelar`, `Confirmar`. Que sean esos exactamente importa menos que sean siempre los mismos.

**Diagnóstico rápido para `ESC-4`.** Buscar en el repositorio los prefijos `Obtener`, `Traer`, `Buscar`, `Consultar`, `Recuperar`, `Get`, `Fetch`, `Retrieve` y contar cuántos aparecen. Si son más de dos, no hay vocabulario acordado.

---

## 11. Enum en plural sin `[Flags]`, y en singular con `[Flags]`

**Nivel: `N-01` fija el criterio.**

El número gramatical del nombre de una enumeración comunica si sus valores se combinan. En plural, el lector espera poder acumular varios; en singular, espera exclusión mutua. Cuando el número contradice al atributo, el nombre engaña.

```csharp
// Plural sin Flags: sugiere combinación donde no la hay
public enum EstadosReserva { Pendiente, Confirmada, Cancelada }

// Singular con Flags: oculta que los valores se combinan
[Flags]
public enum PermisoSala { Ninguno = 0, Ver = 1, Reservar = 2, Aprobar = 4 }
```

```csharp
// Singular: los valores se excluyen
public enum EstadoReserva { Pendiente, Confirmada, Cancelada }

// Plural: los valores se combinan, y los valores son potencias de dos
[Flags]
public enum PermisosSala
{
    Ninguno     = 0,
    Ver         = 1,
    Reservar    = 2,
    Aprobar     = 4,
    Administrar = Ver | Reservar | Aprobar
}
```

Dos errores que acompañan al del número. Una enumeración `[Flags]` cuyos valores no son potencias de dos no funciona como conjunto de banderas, y el compilador no avisa. Y una enumeración sin un valor cero nombrado —`Ninguno`, `Desconocido`— deja que el valor predeterminado de un campo no inicializado sea un estado que nadie eligió; `N-01` recomienda que el cero tenga nombre y sea un estado válido.

---

## 12. CRUD sobre un dominio que no es CRUD

**Nivel: criterio propio, fundado en el lenguaje ubicuo (`O-03`).**

Cada método por separado parece razonable, y ese es el motivo por el que ningún revisor lo detiene. El daño solo se ve al comparar la lista de métodos con lo que dice el negocio.

```csharp
public class ReservaService
{
    public void Create(ReservaDto dto) { /* ... */ }
    public void Update(Guid id, ReservaDto dto) { /* ... */ }
    public void Delete(Guid id) { /* ... */ }
    public void UpdateStatus(Guid id, int status) { /* ... */ }
}
```

```csharp
public sealed class ServicioReservas
{
    public Task<Reserva> SolicitarAsync(SolicitudReserva solicitud, CancellationToken ct)
        => _solicitud.EjecutarAsync(solicitud, ct);

    public Task ConfirmarAsync(Guid reservaId, CancellationToken ct)
        => _confirmacion.EjecutarAsync(reservaId, ct);

    public Task CancelarAsync(Guid reservaId, MotivoCancelacion motivo, CancellationToken ct)
        => _cancelacion.EjecutarAsync(reservaId, motivo, ct);

    public Task ReprogramarAsync(Guid reservaId, RangoHorario nuevoPeriodo, CancellationToken ct)
        => _reprogramacion.EjecutarAsync(reservaId, nuevoPeriodo, ct);
}
```

`Delete` y `Cancelar` no son la misma operación. Una reserva cancelada existe, conserva su motivo y su momento, puede aparecer en un informe de uso y suele disparar una notificación al solicitante; una reserva borrada no está. Cuando el método se llama `Delete`, esa diferencia deja de estar escrita y pasa a vivir en la implementación, donde solo la encuentra quien la busque.

`UpdateStatus(id, 3)` agrega un defecto propio: la transición de estado desaparece como concepto. El sistema pasa a permitir cualquier estado desde cualquier otro, y las reglas que dicen que una reserva cancelada no se confirma tienen que vivir en el llamador o en ningún lado.

No todo dominio es así. Un catálogo de tipos de sala probablemente sea CRUD de verdad, y ahí `Crear`, `Actualizar` y `Eliminar` son los verbos correctos porque son los que usa el negocio. El antipatrón es aplicar el vocabulario CRUD **por defecto**, sin haber preguntado qué verbos usa quien conoce el problema.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

Casi nada de este catálogo aparece en `ESC-1`, salvo lo que alguien traiga de su historia previa: un desarrollador con años de C++ trae `m_`, uno con años de Java trae `Impl` y `SCREAMING_SNAKE_CASE`. Se resuelve con un `.editorconfig` desde el primer commit y con una lectura conjunta de este documento en la primera semana, que cuesta media hora.

El único antipatrón que se genera nativamente en `ESC-1` es el nombre genérico vacío, y por una razón estructural: al principio nadie conoce el dominio lo suficiente como para nombrar responsabilidades con precisión, y `SalaManager` es una salida cómoda. La contramedida no es prohibirlo sino programar una revisión de nombres cuando el modelo se estabiliza, mientras renombrar sigue siendo gratis.

### `ESC-2` — Evolución estructural

El antipatrón característico es el nombre genérico vacío que **creció**. Una clase que se llamó `Helper` cuando tenía dos métodos y hoy tiene treinta es a la vez un problema de nombre y de diseño, y la extracción de módulos es el momento natural para partirla, porque de todos modos hay que decidir dónde va cada pedazo.

El segundo es el sinónimo inconsistente entre módulos que crecieron por separado. Se detecta al unificarlos y es trabajo mecánico si se hace de a un verbo por commit.

### `ESC-3` — Normalización de código existente

El escenario propio de este documento. El orden importa y no es obvio.

Primero lo que una herramienta hace sola: prefijos de campo, capitalización, notación húngara en variables locales. Son cambios de ámbito acotado que el refactor del IDE resuelve y que no pueden romper nada fuera de su archivo.

Después el vocabulario. Se levanta el inventario de sinónimos, se fija el canónico y se renombra de a un verbo por commit. Cada commit es revisable porque hace una sola cosa.

Al final los nombres que exigen entender el código: los genéricos vacíos, los que mienten, los CRUD sobre dominio que no es CRUD. Estos no son renombrados sino rediseños, y conviene tratarlos como tales —con su propio análisis y sus propias pruebas— en lugar de colarlos en un commit de normalización.

Dos condiciones sin las cuales el trabajo se repite: los commits de renombrado no llevan cambio funcional y se registran en `.git-blame-ignore-revs`, y la regla correspondiente queda activada en el `.editorconfig` en el mismo commit ([`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)).

### `ESC-4` — Evaluación de código ajeno

Este documento es la lista de verificación del escenario. Los antipatrones se detectan leyendo, sin necesidad de conocer las restricciones bajo las que se decidió nada, y esa es su utilidad: son de los pocos hallazgos que `ACT-04` puede afirmar sin riesgo de estar señalando una decisión justificada que no ve.

Una advertencia igual. Un repositorio de quince años con `m_` en todas partes no tiene un problema de nomenclatura sino una convención de otra época aplicada de forma consistente, y la consistencia vale más que la actualidad. Lo que sí se señala es la **mezcla**: `m_` en unos archivos y `_` en otros indica que nadie decidió, y esa es la observación útil.

Un orden de recorrido que rinde: contar cuántos verbos distintos se usan para la misma operación (antipatrón 10), buscar tipos cuyo nombre termine en `Manager`, `Helper` o `Utils` y mirar su cantidad de líneas (antipatrón 5), y abrir tres métodos que empiecen con `Obtener` o `Get` para ver si alguno escribe (antipatrón 6). Con eso alcanza para saber si hay vocabulario acordado.

### Qué cambia según el contexto

**`CTX-1`.** Se agrega un antipatrón propio: el componente Blazor con nombre de archivo que no coincide con el tipo, o en kebab-case, que directamente no compila como se espera. Y el componente cuyo nombre no lee bien como etiqueta —`<DatosComponent />`—.

**`CTX-2`.** El nombre que miente cuesta más caro, porque el llamador puede ser otro equipo que solo ve la firma. Se suma el antipatrón de exportar vocabulario interno al contrato HTTP: una ruta `/api/reservaManager/doProcess` publica el mal nombre y lo vuelve difícil de corregir.

**`CTX-3`.** Todos los antipatrones cuestan una versión mayor. Es el contexto donde conviene una revisión explícita de nombres antes de la primera publicación, porque después la corrección la pagan los consumidores.

**`CTX-4`.** El sinónimo inconsistente se vuelve estructural: dos servicios que llaman `Reserva` y `Booking` a la misma entidad producen un contrato de mensajes que nadie puede leer de corrido, y ningún compilador va a avisarlo.

---

## Preguntas guía

1. ¿Puedo enumerar en una tarjeta todos los verbos que usa este sistema para sus operaciones? ¿Hay más de uno para la misma?
2. ¿Hay algún tipo llamado `Manager`, `Helper`, `Utils`, `Data` o `Processor`? ¿Cuántas líneas tiene y cuántas razones de cambio?
3. ¿Algún método hace algo que su nombre no anuncia? ¿Algún `Obtener` escribe?
4. ¿Conviven dos generaciones de convención de prefijos? ¿Es residuo histórico o desacuerdo vigente?
5. Este literal, ¿qué significa? ¿Aparece en otro lado con el mismo valor y otro significado?
6. Esta interfaz con sufijo `Impl` en su única implementación, ¿hace falta como interfaz?
7. Esta enumeración, ¿su número gramatical coincide con si lleva `[Flags]`? ¿Tiene un cero nombrado?
8. Los verbos de esta API, ¿son los del negocio o son CRUD por defecto? ¿Se lo pregunté a alguien que conozca el dominio?
9. De los antipatrones que encontré, ¿cuáles puede detectar una herramienta y cuáles dependen de que alguien los lea?
10. Si esto es `ESC-4`: lo que voy a señalar, ¿es un defecto objetivo o una convención distinta de la mía aplicada de forma consistente?

---

## Criterios de calidad

Cómo se distingue un código sano de uno con problema de nombrado, sin leer implementaciones:

**Los verbos son contables.** Se puede enumerar el vocabulario de operaciones del sistema en una tarjeta. Si hay ocho verbos para cuatro operaciones, no hay vocabulario.

**Ningún tipo se llama por lo que es en general.** Ni `Manager`, ni `Helper`, ni `Utils`, ni `Data`, ni `Processor` a secas.

**El nombre de cada método es una afirmación que el código cumple.** Un `Obtener` no escribe, un `Validar` no corrige, un `Async` es asíncrono.

**Hay una sola generación de convenciones.** Ni `m_` conviviendo con `_`, ni `Impl` conviviendo con nombres descriptivos.

**Lo automatizable está automatizado.** Los antipatrones 2, 3, 4 parcialmente, y 11 parcialmente admiten regla de analizador. Los demás dependen de la revisión, y por eso conviene que el catálogo sea corto y esté leído.

**Los literales tienen nombre.** Y dos literales de igual valor tienen nombres distintos si son conceptos distintos.

---

## Qué se automatiza y qué no

Reparto que le sirve a `ACT-03` para decidir dónde invertir y a `ACT-05` para configurarlo ([`TEM-AUTO`](../50-Estilo-de-Codificacion/Automatizacion-del-Estilo.md)).

| Antipatrón | ¿Automatizable? | Cómo |
|-----------|-----------------|------|
| 1 · Notación húngara | Parcialmente | Regla de nomenclatura con prefijos prohibidos; no detecta todos los casos |
| 2 · Prefijo `C` | Sí | `dotnet_naming_rule` sobre tipos, prefijo requerido vacío |
| 3 · Prefijo `m_` | Sí | `dotnet_naming_rule` sobre campos privados, `required_prefix = _` |
| 4 · Abreviaturas | No | Ninguna herramienta conoce el vocabulario del equipo |
| 5 · Nombres genéricos vacíos | No | Requiere juicio sobre la responsabilidad |
| 6 · Nombres que mienten | No | Requiere leer la implementación |
| 7 · Números mágicos | Parcialmente | Analizadores de terceros; con muchos falsos positivos |
| 8 · Sufijo `Impl` | Sí, si se decide | Regla de sufijo prohibido |
| 9 · `Base` mal usado | No | Requiere juicio sobre la jerarquía |
| 10 · Sinónimos inconsistentes | No | Requiere el vocabulario canónico, que no es dato de la herramienta |
| 11 · Enum plural/`[Flags]` | Parcialmente | `CA1027`, `CA1714`, `CA1717` cubren parte |
| 12 · CRUD sobre dominio | No | Requiere conocer el negocio |

La lectura que importa: **los cuatro antipatrones más dañinos —4, 5, 6, 10— son justamente los que ninguna herramienta detecta.** No hay forma de tercerizarlos: dependen de que alguien lea el código con el catálogo en la cabeza. Esa asimetría es la razón por la que este documento existe como texto y no como archivo de configuración.
