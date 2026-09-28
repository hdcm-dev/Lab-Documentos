---
doc_id: TEM-MONO
doc_type: tema
title: Monolito
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Organización, estilo y patrones de código
last_review: 2026-07-20
audience: [humano, agente]
traces: [FAM-SRV, TEM-MODU, TEM-MICRO, TEM-PART, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Monolito — `TEM-MONO`

## Resumen ejecutivo

Un monolito es un sistema que se despliega como una sola unidad. Toda su funcionalidad se compila junta, se publica junta y se ejecuta en un mismo proceso, con lo cual una llamada entre dos partes del sistema es una llamada de método y no un viaje por la red. Esa propiedad —aparentemente menor— es la que produce casi todas sus ventajas.

La palabra arrastra una connotación negativa que su definición no justifica. En la conversación corriente «monolito» significa a la vez «una unidad desplegable» y «código enredado que nadie entiende», y esas dos cosas son independientes: hay monolitos con el dominio aislado, dependencias en una sola dirección y módulos que no se conocen entre sí, y hay conjuntos de microservicios donde la lógica de negocio vive dentro de los controladores. La confusión importa porque produce decisiones caras: equipos que parten el despliegue para resolver un problema de organización de código, y que terminan con el mismo desorden repartido sobre la red.

Este documento sostiene que el monolito es la elección por defecto razonable para un sistema nuevo, y que apartarse de ella exige justificación. Le sirve a `ACT-01` cuando decide la estructura de despliegue en `ESC-1`, y a `ACT-03` cuando tiene que resistir la presión de partir un sistema que todavía no lo necesita.

---

## Definición

### Qué es

Un sistema desplegado como **una unidad**: un artefacto que se publica de una vez, se versiona de una vez y se ejecuta en un proceso. En .NET esto significa habitualmente un ejecutable producido por un proyecto de aplicación —`Microsoft.NET.Sdk.Web` para un sistema con HTTP, `Microsoft.NET.Sdk.Worker` para un servicio de fondo— junto con los ensamblados de los que depende.

La definición no dice nada sobre la cantidad de proyectos. Un monolito puede componerse de un único `.csproj` o de doce, con capas separadas en ensamblados distintos y referencias dirigidas entre ellos; sigue siendo un monolito mientras el resultado publicado sea un solo artefacto que arranca como un solo proceso. Tampoco dice nada sobre el tamaño: un sistema de ochocientas líneas y uno de cuatrocientos mil pueden ambos ser monolitos.

Microsoft lo describe en `N-12` como una de las arquitecturas frecuentes para aplicaciones web, junto con la organización en N capas y las variantes de arquitectura limpia. Conviene notar cómo lo presenta: no como una etapa previa a algo mejor, sino como una opción con su propio perfil de ventajas.

### Qué problema resuelve

**El costo de coordinación.** En un despliegue único no existe el problema de las versiones incompatibles entre partes del sistema, porque todas las partes se publican juntas. Un cambio que modifica la firma de un método y sus once llamadores entra en un solo *commit*, se verifica en un solo *build* y se publica en un solo despliegue.

**La consistencia de los datos.** Con un almacén único y un proceso único, una operación que toca tres agregados es una transacción de base de datos. Confirma o revierte. No hace falta una saga, ni compensaciones, ni un estado intermedio observable en el que la reserva existe pero la notificación todavía no se envió.

**La dificultad de encontrar dónde falla algo.** Un error tiene una pila de llamadas completa, desde el punto de entrada HTTP hasta la consulta a la base. Se depura poniendo un punto de interrupción y avanzando. En un sistema distribuido esa misma tarea requiere correlación de trazas entre procesos, que es una capacidad que hay que construir y operar.

### Qué NO es, y con qué se lo confunde

**Monolito no es código desordenado.** Es la confusión central del tema y la que produce las decisiones más caras. «Monolito» describe cómo se despliega el sistema; «código desordenado» describe cómo está escrito. Se combinan libremente, y la evidencia son los muchos monolitos con separación estricta entre dominio, aplicación e infraestructura, verificada por el compilador en cada *build*.

El origen de la confusión es histórico y comprensible: durante años, la ausencia de límites internos fue la práctica común dentro de los monolitos, y los sistemas grandes que envejecieron mal eran casi todos monolíticos porque casi todo era monolítico. Pero la correlación no es causa. Lo que degradó esos sistemas fue la falta de límites, y esa falta se reproduce igual repartida en servicios.

**Monolito no es «una capa».** Un sistema en tres capas dentro de un solo ejecutable es un monolito. La organización interna —capas, cortes verticales, módulos— es un eje distinto y se trata en [`FAM-INT`](../30-Organizacion-Interna/README.md).

**Monolito no es «un proyecto».** Un `.slnx` con nueve `.csproj` que produce un solo ejecutable es un monolito con nueve proyectos. La cantidad de proyectos es una decisión de compilación y de imposición de límites, no de despliegue ([`TEM-CVP`](../30-Organizacion-Interna/Carpetas-o-Proyectos.md)).

**Monolito no es una fase transitoria.** Muchos sistemas nacen monolíticos, crecen monolíticos y terminan su vida útil monolíticos sin que eso constituya deuda técnica. La suposición de que todo sistema debe eventualmente partirse no tiene respaldo: lo que determina la necesidad de partir son condiciones concretas —perfiles de escalado divergentes, equipos que se bloquean— que muchos sistemas nunca desarrollan.

**Monolito no es lo mismo que monolito distribuido.** El segundo tiene varias unidades desplegables que no pueden desplegarse de forma independiente. Es una categoría de fallo, no una variante del monolito ([`TEM-MICRO`](Microservicios.md)).

---

## Las ventajas reales, y por qué son reales

Cada una de estas ventajas se deriva de una propiedad técnica concreta y no de una preferencia.

**Transacciones ACID locales.** Con un solo almacén y un solo proceso, la atomicidad la provee el motor de base de datos. Confirmar una reserva, descontar el cupo de la sala y registrar la auditoría ocurre dentro de una transacción: o pasan las tres cosas o no pasa ninguna. En un sistema partido, esas tres escrituras viven en tres almacenes y la atomicidad hay que construirla con una saga y sus compensaciones, que es código que se escribe, se prueba y se opera.

**Refactorización atómica verificada por el compilador.** Renombrar un tipo, cambiar la firma de un método o mover una responsabilidad de un módulo a otro produce, si algo quedó mal, un error de compilación. El compilador recorre todo el sistema porque todo el sistema está en la misma compilación. Esa garantía desaparece en cuanto un límite se convierte en una llamada HTTP: el consumidor compila igual aunque el productor haya cambiado el contrato, y la falla aparece en producción.

**Un solo despliegue.** No hay orden de publicación que respetar, ni ventana de incompatibilidad entre versiones, ni la pregunta de qué pasa si el servicio A ya se actualizó y el B todavía no. Publicar es reemplazar un artefacto.

**Depuración de punta a punta.** Una petición se sigue desde el *endpoint* hasta el acceso a datos con el depurador del IDE. La observabilidad distribuida —trazas correlacionadas, propagación de contexto, agregación de registros— es una capacidad valiosa que un monolito simplemente no necesita para responder «¿por qué falló esta petición?».

**Latencia de llamada en proceso.** Invocar un método cuesta nanosegundos. Una llamada HTTP entre servicios del mismo centro de datos cuesta del orden de milisegundos, y además puede fallar por razones que no tienen nada que ver con la lógica: tiempo de espera agotado, conexión rechazada, respuesta parcial. Cada límite de red que se agrega es un modo de fallo nuevo que hay que manejar en el código y en la operación.

**Costo operativo bajo.** Una canalización, un artefacto, un conjunto de métricas, un lugar donde mirar los registros. La infraestructura que exige un sistema distribuido —descubrimiento de servicios, malla o pasarela, correlación de trazas, gestión de configuración por servicio— tiene un costo de construcción y de operación permanente que alguien paga.

---

## Cuándo deja de alcanzar

Ninguna de las ventajas anteriores desaparece con el tamaño; lo que aparece son costos que crecen y que en algún punto las superan. Los síntomas que importan son cinco, y conviene distinguirlos porque solo tres se resuelven partiendo el despliegue.

**Perfiles de escalado divergentes.** El módulo de consulta de disponibilidad recibe cuarenta veces más tráfico que el de administración de salas, y escalar el monolito replica ambos. Si el desperdicio de recursos es medible y significativo, hay un argumento para partir. Si no se midió, no hay argumento.

**Equipos que se bloquean entre sí.** Ocho personas trabajando sobre el mismo artefacto coordinan sin fricción. Sesenta personas en seis equipos que comparten una canalización y una ventana de despliegue se bloquean, y ese bloqueo tiene un costo que se puede medir en tiempo de espera antes de publicar.

**Requisitos de disponibilidad distintos.** Si la caída del módulo de reportes no debe poder tumbar el de reservas, y en un proceso único sí puede —una fuga de memoria en el generador de reportes agota el proceso entero—, hay un argumento de aislamiento de fallas.

**El *build* se volvió lento.** Este síntoma **no** se resuelve partiendo el despliegue. Se resuelve con menos proyectos, no con más, y con caché de compilación. Partir en servicios agrega procesos de *build* independientes pero también agrega el costo de coordinarlos.

**Un cambio pequeño obliga a tocar cinco carpetas.** Tampoco se resuelve partiendo. Es un síntoma de organización interna —típicamente, carpetas por capa técnica en un sistema que cambia por funcionalidad— y su remedio está en [`TEM-SLICE`](../30-Organizacion-Interna/Vertical-Slice.md).

La confusión entre los tres primeros y los dos últimos es la causa más frecuente de particiones que no mejoran ningún indicador. Los criterios completos, con su árbol de decisión, están en [`TEM-PART`](Criterios-de-Particion.md).

---

## «Monolith first» como posición defendible

**Martin Fowler** sostuvo la posición conocida como *monolith first*: para un sistema nuevo, conviene empezar con un monolito y partirlo después, cuando los límites del dominio se conozcan. La atribución al autor es firme; el artículo específico no se verificó en fuente primaria para esta guía y no se cita URL. La idea circula ampliamente y merece evaluarse por su argumento más que por su autoridad.

El argumento es este. Los límites entre servicios son caros de mover una vez establecidos, porque mover uno implica migrar datos entre almacenes. Y el momento en que se decide dónde ponerlos —el arranque del proyecto— es precisamente aquel en que menos se sabe del dominio. Empezar monolítico permite que los límites se descubran con el uso, cuando ya se ve qué cambia junto y qué cambia por separado, y recién entonces convertir los que resultaron estables en límites de despliegue.

La objeción seria a esta posición también existe: un monolito construido sin ninguna atención a los límites internos es muy difícil de partir después, y el argumento de «lo partimos cuando haga falta» se vuelve vacío. Esa objeción es válida y no invalida la posición, la precisa. **Esta guía recomienda** la formulación intermedia: empezar con una unidad desplegable, pero con límites internos explícitos desde el primer día, que es exactamente lo que describe [`TEM-MODU`](Monolito-Modular.md). El costo de mantener límites dentro de un monolito es bajo; el de no tenerlos cuando hace falta partir es alto.

---

## Aplicación por escenario

### `ESC-1` — Sistema nuevo

Es donde la decisión se toma, y donde el monolito es la respuesta por defecto. La pregunta operativa no es «¿monolito o microservicios?» sino «¿existe hoy una condición verificable que exija más de una unidad desplegable?». Si la respuesta requiere hipótesis sobre el futuro —«vamos a necesitar escalar», «el equipo va a crecer»—, no es una condición verificable y el monolito gana por defecto.

Lo que sí conviene decidir el primer día es la organización interna, porque es lo que determina si la partición futura será factible. Un monolito con su dirección de dependencia respetada —el dominio sin conocer la infraestructura, en carpetas o en proyectos según el criterio de [`TEM-CVP`](../30-Organizacion-Interna/Carpetas-o-Proyectos.md)— se parte con esfuerzo acotado; uno donde los componentes de UI consultan el `DbContext` directamente, no. Lo que decide la factibilidad de la partición futura es la dirección, no el mecanismo que la sostiene: fijar proyectos el primer día compra verificación por el compilador a cambio de rigidez, y `TEM-CVP` desaconseja pagarla antes de que la disciplina haya fallado.

### `ESC-2` — Evolución estructural

La decisión característica es cuándo **dejar** de ser monolito, y la respuesta correcta la mayoría de las veces es «todavía no». El criterio: hay un síntoma medido —no percibido— que la partición resolvería, y que ninguna intervención más barata resuelve.

El movimiento inverso también ocurre y merece registrarse: sistemas que se partieron sin motivo suficiente y que se reunifican. Es más caro que partir, por la divergencia de datos, y es la razón por la cual la partición merece un ADR con su disparador de revisión explícito.

### `ESC-3` — Normalización de código existente

Aplica de forma indirecta. La normalización de estilo y nomenclatura no cambia el modelo de despliegue, pero en un monolito tiene una propiedad que en un sistema distribuido no tiene: se hace de una vez sobre todo el código, con una sola configuración de analizadores y un solo `.editorconfig`. La misma normalización sobre siete repositorios de servicios requiere coordinar siete cambios y siete despliegues.

### `ESC-4` — Evaluación de código ajeno

La pregunta a formular no es «¿es un monolito?» sino «¿tiene límites internos y se hacen cumplir?». Un monolito con módulos verificados por el compilador es un sistema sano; uno sin ningún límite es un sistema con riesgo creciente, aunque hoy funcione.

La observación que **no** corresponde hacer es señalar el modelo monolítico como defecto en sí. Sin conocer el tamaño del equipo, el perfil de carga y los requisitos de disponibilidad, no hay base para juzgar la decisión de despliegue. Sí hay base para juzgar la consistencia interna, que es observable sin contexto.

### Qué cambia según el contexto

| Contexto | Qué cambia |
|---|---|
| `CTX-1` Web/cliente | El monolito es la forma dominante. Un proyecto `Microsoft.NET.Sdk.Web` con la UI y la lógica en carpetas separadas cubre la mayoría de los casos. El riesgo específico es que la frontera entre presentación y dominio se cruce sin que nada la vigile |
| `CTX-2` Servicio/API | Igual de aplicable. Un único proyecto con todos los *endpoints* es un monolito de servicio, y el contrato HTTP externo no obliga a nada respecto de la organización interna |
| `CTX-3` Biblioteca | No aplica en sentido estricto: una biblioteca no se despliega, se referencia. La distinción análoga es cuántos paquetes NuGet se publican, y ahí rige un criterio distinto —la granularidad la fija el consumidor, no el equipo |
| `CTX-4` Distribuida | Por definición no es monolito. La advertencia relevante es la del contexto: buena parte de lo que se declara `CTX-4` es un monolito repartido en procesos que comparten base de datos |

---

## Ejemplos concretos

### Un monolito con límites internos — sistema de reserva de salas

Ejemplo sintético. Un único proyecto ejecutable, con las capas en carpetas y el dominio sin dependencias hacia afuera.

```text
Reservas.slnx
├── src/
│   └── MiEmpresa.Reservas.Servicio/            ← Microsoft.NET.Sdk.Web (única unidad desplegable)
│       ├── Domain/
│       │   ├── Reserva.cs
│       │   ├── Sala.cs
│       │   └── PoliticaSolapamiento.cs
│       ├── Application/
│       │   ├── ConfirmarReserva.cs
│       │   └── ConsultarDisponibilidad.cs
│       ├── Infrastructure/
│       │   ├── ReservasDbContext.cs
│       │   └── Migrations/
│       ├── Components/                   ← Blazor interactive server
│       └── Program.cs
└── tests/
    └── MiEmpresa.Reservas.Servicio.Tests/
```

Lo que este arreglo demuestra es que la separación de responsabilidades no requiere separar el despliegue. `Domain/` no conoce `Infrastructure/`; `Components/` no toca el `DbContext`. Que esos límites se cumplan depende acá de la disciplina y la revisión, porque las carpetas no las hace cumplir el compilador: el paso siguiente, cuando la disciplina no alcanza, es convertirlas en proyectos ([`TEM-MODU`](Monolito-Modular.md)).

### Dónde se comprueba que la separación es real

La forma de carpetas no prueba nada por sí sola. Lo que la vuelve comprobable es el reparto en el borde HTTP: los *endpoints* de autenticación viven como Minimal APIs en `Program.cs` y la regla vive en `ServicioReservas`, dentro de la capa de aplicación. El punto de entrada resuelve el protocolo —lee el formulario, valida el antifalsificación, traduce el resultado a una redirección— y delega. Cuando esa delegación desaparece y el handler empieza a decidir, la separación existe en el árbol de archivos y no en el código.

La ubicación de `tests/` en la raíz, hermana de `src/`, es una decisión de equipo y no una convención heredada: de los tres repositorios de referencia de Microsoft, dos anidan los tests por componente (`F-01`).

### La transacción que un monolito hace gratis

```csharp
// Application/ConfirmarReserva.cs — ejemplo sintético
public async Task<ResultadoReserva> EjecutarAsync(
    SolicitudReserva solicitud,
    CancellationToken ct)
{
    await using var transaccion = await _db.Database.BeginTransactionAsync(ct);

    var sala = await _db.Salas.FindAsync([solicitud.SalaId], ct);
    if (sala is null) return ResultadoReserva.SalaInexistente();

    var reserva = sala.Reservar(solicitud.Periodo, solicitud.Solicitante);
    _db.Reservas.Add(reserva);
    _db.Auditoria.Add(EventoAuditoria.ReservaCreada(reserva));

    await _db.SaveChangesAsync(ct);   // una sola escritura atómica
    await transaccion.CommitAsync(ct);

    return ResultadoReserva.Confirmada(reserva.Id);
}
```

Tres escrituras que confirman o revierten juntas, con la garantía provista por el motor. El equivalente distribuido —reserva en un servicio, auditoría en otro— requiere una saga con su compensación, un estado intermedio observable y una decisión explícita sobre qué se hace si la compensación falla. La comparación no dice que el monolito sea mejor; dice cuál es el costo concreto de la alternativa, para que se lo pague a sabiendas.

---

## Preguntas guía

1. ¿Existe hoy una condición verificable que exija más de una unidad desplegable, o la justificación se apoya en una hipótesis sobre el futuro?
2. ¿El desorden que motiva la conversación es de despliegue o de organización del código? ¿Partir lo resolvería, o solo lo distribuiría?
3. Si mañana hubiera que partir este sistema, ¿alguien puede señalar por dónde se cortaría? ¿Las tablas de la base de datos se dejan repartir entre esos cortes?
4. ¿Los límites internos que el diagrama declara los verifica el compilador, un analizador, o solamente la revisión de código?
5. ¿Cuál de los cinco síntomas está presente, y está medido? ¿Se descartó una intervención más barata que resolviera lo mismo?
6. ¿Hay un ADR que registre por qué el sistema es monolítico y bajo qué condición se revisaría esa decisión?

---

## Criterios de calidad

Un monolito bien construido y uno mal construido se distinguen por propiedades observables, ninguna de las cuales tiene que ver con su tamaño.

**Las dependencias van en una sola dirección.** El dominio no conoce la infraestructura ni la presentación. Se verifica leyendo los `using` o, mejor, con referencias de proyecto dirigidas que hacen imposible el ciclo.

**Hay un lugar evidente donde va cada cosa.** Un desarrollador nuevo que agrega una regla de negocio sabe dónde ponerla sin preguntar. Si la respuesta es «depende de a quién le preguntes», el sistema no tiene límites, tiene costumbres.

**La partición futura es concebible.** Alguien puede señalar por dónde se cortaría el sistema si hiciera falta. Que nadie pueda responder esa pregunta es el indicador temprano de que los límites no existen.

**La decisión está registrada.** Un ADR que diga por qué es monolítico, qué condiciones lo cambiarían y qué se mide para detectarlas. Sin eso, la discusión vuelve cada seis meses.

### Antipatrones

**La gran bola de barro.** El monolito sin ningún límite interno: cualquier clase puede llamar a cualquier otra, la lógica de negocio aparece en controladores, en componentes de UI y en procedimientos almacenados. Es la imagen que la palabra «monolito» evoca y que su definición no implica. Se reconoce porque agregar una funcionalidad requiere leer código en lugares que nadie predijo.

**El monolito con capas ornamentales.** Existen las carpetas `Domain/`, `Application/` e `Infrastructure/`, y también existe un componente de UI que inyecta el `DbContext` y consulta directamente. Las capas están declaradas pero no se cumplen. Es peor que no tenerlas, porque el diagrama miente y la revisión se relaja creyendo que hay una garantía.

**El monolito como excusa.** «Es un monolito, así que da igual dónde ponga esto». Confunde el eje de despliegue con el de organización del código y produce la bola de barro por decisión consciente.

**La partición preventiva.** Partir el despliegue en `ESC-1` por una necesidad de escalado que nadie midió y que puede no llegar nunca. Paga hoy el costo completo de la distribución a cambio de una opción que quizá no se ejerza.

**El monolito indefendible.** El sistema que efectivamente cumple las condiciones para partirse —equipos bloqueados, escalado divergente medido, disponibilidad diferencial exigida— y no se parte porque «siempre fue así». La contención tiene límite, y este documento defiende el monolito como elección, no como inercia.
