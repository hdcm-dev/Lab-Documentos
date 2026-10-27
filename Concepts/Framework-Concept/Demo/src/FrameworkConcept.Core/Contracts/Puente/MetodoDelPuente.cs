namespace FrameworkConcept.Core.Contracts.Puente;

/// <summary>
/// Una fila de la tabla de métodos: nombre, parámetros y forma de la respuesta. Es lo que la instantánea
/// del contrato compara (P3). Los tipos se escriben como texto («string», «int», «bool», «json»,
/// «string[]», o el nombre de un tipo del contrato) porque el contrato lo leen dos lenguajes: C# y JavaScript.
/// </summary>
/// <param name="Nombre">«area.operacion». El área en inglés si es del framework; en español si es del dominio.</param>
/// <param name="Descripcion">Para quien lee la tabla. No entra en la instantánea.</param>
/// <param name="Parametros">Parámetros con nombre.</param>
/// <param name="Respuesta">Campos del objeto «result».</param>
/// <param name="Capacidad">Id de la entrada del catálogo que el método usa, si toca el dispositivo (P12).</param>
public sealed record MetodoDelPuente(
    string Nombre,
    string Descripcion,
    IReadOnlyList<CampoDelSobre> Parametros,
    IReadOnlyList<CampoDelSobre> Respuesta,
    string? Capacidad = null);

/// <summary>Un campo de parámetros o de respuesta. Opcional = puede faltar.</summary>
public sealed record CampoDelSobre(string Nombre, string Tipo, bool Opcional = false);

/// <summary>Un evento que el cascarón le envía al contenido (sin respuesta).</summary>
public sealed record EventoDelPuente(string Nombre, string Descripcion, IReadOnlyList<CampoDelSobre> Parametros);
