// Nuevo (ejemplo 09; plan §F.4). Dos casos de uso del ABM expuestos como herramientas de ESCRITURA:
// agregar_localidad sobre LocalidadService.SaveAsync y eliminar_localidad(nombre, provincia) sobre
// GetByNombreYProvinciaAsync + DeleteAsync (H3-027: el modelo nunca da un id). Las dos van envueltas en
// ApprovalRequiredAIFunction: FunctionInvokingChatClient no las ejecuta sin una respuesta de aprobación.
// Lo que el caso de uso devuelve (Result) vuelve al modelo tal cual, como ResultadoDeHerramienta.
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application.Herramientas;

/// <summary>El <see cref="Result" /> del caso de uso, con nombres estables para el modelo y el registro.</summary>
public sealed record ResultadoDeHerramienta(
    [property: JsonPropertyName("exito")] bool Exito,
    [property: JsonPropertyName("mensaje")] string Mensaje,
    [property: JsonPropertyName("errores")] IReadOnlyDictionary<string, string> Errores,
    [property: JsonPropertyName("id")] int? Id)
{
    public static ResultadoDeHerramienta De(Result r) => new(r.IsSuccess, r.Message, r.Errors, r.Id);
}

/// <summary>Una ejecución real de una herramienta de escritura (después de la aprobación).</summary>
public sealed record Ejecucion(string CallId, string Herramienta, IReadOnlyDictionary<string, object?> Argumentos, ResultadoDeHerramienta Resultado);

public sealed class HerramientasDeLocalidades(LocalidadService localidades)
{
    public const string Agregar = "agregar_localidad";
    public const string Eliminar = "eliminar_localidad";

    /// <summary>Lo que de verdad se ejecutó, con el id de la llamada del modelo (para cruzarlo con las aprobaciones).</summary>
    public List<Ejecucion> Ejecuciones { get; } = [];

    /// <summary>Las herramientas montadas: las dos aprobables. No hay otra forma de armarlas en este ejemplo.</summary>
    public IList<AITool> Crear() =>
    [
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(AgregarAsync, Agregar,
            "Da de alta una localidad en el ABM. Pasá los datos tal como los dio la persona, sin corregirlos.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(EliminarAsync, Eliminar,
            "Da de baja una localidad del ABM, buscándola por nombre y provincia."))
    ];

    public async Task<ResultadoDeHerramienta> AgregarAsync(
        [Description("Nombre de la localidad")] string nombre,
        [Description("Provincia, tal como la dijo la persona")] string provincia,
        [Description("Código postal, tal como lo dijo la persona")] string codigoPostal,
        [Description("Cantidad de habitantes")] int habitantes,
        CancellationToken cancellationToken = default)
    {
        var model = new LocalidadModel { Nombre = nombre, Provincia = provincia, CodigoPostal = codigoPostal, Habitantes = habitantes };
        var resultado = ResultadoDeHerramienta.De(await localidades.SaveAsync(model, cancellationToken));
        Anotar(Agregar, new Dictionary<string, object?>
        {
            ["nombre"] = nombre, ["provincia"] = provincia, ["codigoPostal"] = codigoPostal, ["habitantes"] = habitantes
        }, resultado);
        return resultado;
    }

    public async Task<ResultadoDeHerramienta> EliminarAsync(
        [Description("Nombre de la localidad")] string nombre,
        [Description("Provincia de la localidad")] string provincia,
        CancellationToken cancellationToken = default)
    {
        // Se resuelve otra vez al ejecutar: entre el pedido y la aprobación la base pudo cambiar.
        var r = await localidades.GetByNombreYProvinciaAsync(nombre, provincia, cancellationToken);
        var resultado = ResultadoDeHerramienta.De(r.Localidad is { } l
            ? await localidades.DeleteAsync(l.Id, cancellationToken)
            : r.Error!);
        Anotar(Eliminar, new Dictionary<string, object?> { ["nombre"] = nombre, ["provincia"] = provincia }, resultado);
        return resultado;
    }

    private void Anotar(string herramienta, Dictionary<string, object?> argumentos, ResultadoDeHerramienta resultado) =>
        Ejecuciones.Add(new Ejecucion(FunctionInvokingChatClient.CurrentContext?.CallContent.CallId ?? "(fuera del bucle)", herramienta, argumentos, resultado));
}
