// Nuevo (ejemplo 11; plan §F.4). Las herramientas que monta el CÓDIGO para el agente: una lectura
// (listar_localidades: id, nombre y provincia, H-011) y una escritura destructiva
// (eliminar_localidad(nombre, provincia) sobre GetByNombreYProvinciaAsync + DeleteAsync, H3-027),
// envuelta en ApprovalRequiredAIFunction. El modelo nunca da un id.
//
// La baja no confía en los argumentos que trae la llamada: busca qué id aprobó la persona para ESA
// llamada (por su CallId) y borra sólo si la localidad resuelta es ésa. Así, lo borrado es lo
// mostrado en el pedido aunque la base cambie entre las dos peticiones.
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Application.Herramientas;

/// <summary>Lo que devuelve la lectura: id, nombre y provincia (como LocalidadDto del caso base).</summary>
public sealed record LocalidadListada(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("provincia")] string Provincia);

/// <summary>El <see cref="Result" /> del caso de uso, con nombres estables para el modelo y el registro.</summary>
/// <param name="Id">En una baja con éxito, el id borrado (el registro lo cruza con el pedido mostrado).</param>
public sealed record ResultadoDeHerramienta(
    [property: JsonPropertyName("exito")] bool Exito,
    [property: JsonPropertyName("mensaje")] string Mensaje,
    [property: JsonPropertyName("errores")] IReadOnlyDictionary<string, string> Errores,
    [property: JsonPropertyName("id")] int? Id)
{
    public static ResultadoDeHerramienta De(Result r, int? id = null) => new(r.IsSuccess, r.Message, r.Errors, id ?? r.Id);
}

/// <summary>Una ejecución real de la baja (después de la aprobación, o el intento sin ella).</summary>
public sealed record Ejecucion(string CallId, string Herramienta, IReadOnlyDictionary<string, object?> Argumentos, ResultadoDeHerramienta Resultado);

/// <summary>Qué id aprobó la persona para una llamada del modelo. Lo implementa el servidor de aprobaciones.</summary>
public interface IBajasAprobadas
{
    int? IdAprobado(string callId);
}

public sealed class HerramientasDelAgente(LocalidadService localidades, IBajasAprobadas aprobadas)
{
    public const string Listar = "listar_localidades";
    public const string Eliminar = "eliminar_localidad";

    /// <summary>Bajas que de verdad llegaron al código (con o sin éxito).</summary>
    public List<Ejecucion> Ejecuciones { get; } = [];

    /// <summary>
    /// La lectura devolvió campos libres (Nombre) que la app no escribió: desde acá el pedido queda
    /// marcado (política de origen, regla 2, §F.4).
    /// </summary>
    public bool LeyoTextoNoConfiable { get; private set; }

    /// <summary>Las herramientas montadas: el código las fija; la baja es aprobable.</summary>
    public IList<AITool> Crear() =>
    [
        AIFunctionFactory.Create(ListarAsync, Listar,
            "Lista las localidades de la sesión de la persona: id, nombre y provincia."),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(EliminarAsync, Eliminar,
            "Da de baja una localidad del ABM, buscándola por nombre y provincia. Si la persona no dijo la provincia, dejala vacía."))
    ];

    public async Task<IReadOnlyList<LocalidadListada>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var todas = await localidades.GetAllAsync(cancellationToken);
        LeyoTextoNoConfiable = true;
        return [.. todas.Select(l => new LocalidadListada(l.Id, l.Nombre, l.Provincia))];
    }

    public async Task<ResultadoDeHerramienta> EliminarAsync(
        [Description("Nombre de la localidad")] string nombre,
        [Description("Provincia de la localidad; vacía si la persona no la dijo")] string provincia = "",
        CancellationToken cancellationToken = default)
    {
        var callId = FunctionInvokingChatClient.CurrentContext?.CallContent.CallId;
        var argumentos = new Dictionary<string, object?> { ["nombre"] = nombre, ["provincia"] = provincia };
        ResultadoDeHerramienta resultado;
        if (callId is null || aprobadas.IdAprobado(callId) is not int idAprobado)
        {
            // No pasa por el bucle (FunctionInvokingChatClient no ejecuta una aprobable sin respuesta),
            // pero la herramienta no lo da por hecho: sin aprobación registrada, no borra.
            resultado = ResultadoDeHerramienta.De(Result.Invalid("aprobacion", "Esta baja no tiene la aprobación de la persona."));
        }
        else
        {
            // Se resuelve otra vez al ejecutar: entre el pedido y la aprobación la base pudo cambiar.
            var r = await localidades.GetByNombreYProvinciaAsync(nombre, provincia, cancellationToken);
            resultado = r.Localidad switch
            {
                null => ResultadoDeHerramienta.De(r.Error!),
                { Id: var id } when id != idAprobado => ResultadoDeHerramienta.De(
                    Result.Invalid("nombre", "La localidad no es la que se mostró en el pedido de aprobación.")),
                var l => ResultadoDeHerramienta.De(await localidades.DeleteAsync(l.Id, cancellationToken), l.Id)
            };
        }
        Ejecuciones.Add(new Ejecucion(callId ?? "(fuera del bucle)", Eliminar, argumentos, resultado));
        return resultado;
    }
}
