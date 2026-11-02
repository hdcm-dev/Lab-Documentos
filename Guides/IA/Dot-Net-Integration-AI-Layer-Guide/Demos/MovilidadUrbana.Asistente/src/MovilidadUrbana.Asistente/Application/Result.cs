// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Application/Result.cs @ 8fb4c3a
// Delta: Modificado a causa de la IA — Id de lo creado en Success (H2-015); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
namespace MovilidadUrbana.Asistente.Application;

/// <summary>
/// Resultado de un caso de uso: si salió bien, el aviso a mostrar y los errores por campo.
/// Las claves de <see cref="Errors"/> son los nombres de campo que la pantalla conoce
/// (`nombre`, `provincia`, …), para que la vista solo tenga que ubicarlos.
/// </summary>
public sealed record Result(bool IsSuccess, string Message, IReadOnlyDictionary<string, string> Errors)
{
    private static readonly IReadOnlyDictionary<string, string> NoErrors =
        new Dictionary<string, string>();

    /// <summary>Id de lo que el caso de uso creó, si creó algo (nuevo: H2-015, para que una herramienta informe lo creado).</summary>
    public int? Id { get; init; }

    public static Result Success(string mensaje) => new(true, mensaje, NoErrors);

    public static Result Success(string mensaje, int id) => new(true, mensaje, NoErrors) { Id = id };

    public static Result Invalid(IReadOnlyDictionary<string, string> errors) =>
        new(false, "Revise los campos marcados en rojo.", errors);

    public static Result Invalid(string campo, string mensaje) =>
        Invalid(new Dictionary<string, string> { [campo] = mensaje });
}
