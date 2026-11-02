// Nuevo (ejemplo 16; plan §E.2 fila 16 y §F.4). Las herramientas que monta el CÓDIGO según la variante:
//   origen              → sólo listar_localidades (regla 1: sin herramientas de escritura montadas);
//   aprobacion          → listar_localidades + eliminar_localidad(nombre, provincia) aprobable y vigilada;
//   aprobacion+marcado  → lo mismo, y el resultado de la lectura va encerrado entre marcas (spotlighting
//                         por delimitación, con una marca nueva por pedido para que el texto no la pueda cerrar).
// listar_localidades devuelve id, nombre y provincia (H-011); el Nombre es un campo libre que cargó una
// persona, así que la lectura MARCA el pedido (regla 2). El modelo nunca da un id (H3-027).
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Application.Origen;

namespace MovilidadUrbana.Asistente.Application.Herramientas;

/// <summary>Lo que devuelve la lectura: id, nombre y provincia (como LocalidadDto del caso base).</summary>
public sealed record LocalidadListada(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("provincia")] string Provincia);

/// <summary>El <see cref="Result" /> del caso de uso, con nombres estables para el modelo y el registro.</summary>
public sealed record ResultadoDeHerramienta(
    [property: JsonPropertyName("exito")] bool Exito,
    [property: JsonPropertyName("mensaje")] string Mensaje,
    [property: JsonPropertyName("errores")] IReadOnlyDictionary<string, string> Errores,
    [property: JsonPropertyName("id")] int? Id)
{
    public static ResultadoDeHerramienta De(Result r, int? id = null) => new(r.IsSuccess, r.Message, r.Errors, id ?? r.Id);
}

/// <summary>Una baja que de verdad llegó al caso de uso (con o sin éxito).</summary>
public sealed record Ejecucion(string CallId, IReadOnlyDictionary<string, object?> Argumentos, ResultadoDeHerramienta Resultado);

/// <summary>Qué id resolvió y mostró el código para una llamada aprobada. Lo implementa el servidor de aprobaciones.</summary>
public interface IBajasAprobadas
{
    int? IdAprobado(string callId);
}

public sealed class HerramientasDeLaInyeccion
{
    public const string Listar = "listar_localidades";
    public const string Eliminar = "eliminar_localidad";

    private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly LocalidadService _localidades;
    private readonly IBajasAprobadas _bajas;
    private readonly IAprobaciones _aprobaciones;
    private readonly string _variante;

    public HerramientasDeLaInyeccion(LocalidadService localidades, Aprobacion.ServidorDeAprobaciones servidor, string variante, string? marca = null)
    {
        if (!Variantes.Valida(variante)) throw new ArgumentException($"Variante desconocida: {variante}", nameof(variante));
        _localidades = localidades;
        _bajas = servidor;
        _aprobaciones = servidor;
        _variante = variante;
        Marca = marca ?? $"datos-{Guid.NewGuid():n}"[..14];
    }

    /// <summary>El estado del pedido: lo marca la lectura y lo leen la escritura y el pedido de aprobación.</summary>
    public EstadoDelPedido Estado { get; } = new();

    /// <summary>La marca de esta conversación (sólo se usa en «aprobacion+marcado»).</summary>
    public string Marca { get; }

    /// <summary>Bajas que de verdad llegaron al caso de uso.</summary>
    public List<Ejecucion> Ejecuciones { get; } = [];

    /// <summary>La capa de la regla 2 sobre la baja (null en «origen», donde la baja no existe).</summary>
    public EscrituraVigilada? Vigilada { get; private set; }

    /// <summary>Las herramientas montadas para la variante. El código las fija; el modelo no elige.</summary>
    public IList<AITool> Crear()
    {
        var listar = AIFunctionFactory.Create(ListarAsync, Listar,
            "Lista las localidades cargadas en la sesión de la persona: id, nombre y provincia de cada una.");
        if (_variante == Variantes.Origen) return [listar];

        Vigilada = new EscrituraVigilada(CrearEliminar(), Estado, _aprobaciones);
        return [listar, new ApprovalRequiredAIFunction(Vigilada)];
    }

    /// <summary>
    /// Sólo para la prueba de la regla 2 con una escritura NO aprobable: la misma baja, vigilada pero sin
    /// ApprovalRequiredAIFunction. Con el pedido marcado, rechaza sin ejecutar.
    /// </summary>
    public IList<AITool> CrearConEscrituraNoAprobable()
    {
        Vigilada = new EscrituraVigilada(CrearEliminar(), Estado, _aprobaciones);
        return [AIFunctionFactory.Create(ListarAsync, Listar, "Lista las localidades cargadas en la sesión de la persona."), Vigilada];
    }

    private AIFunction CrearEliminar() => AIFunctionFactory.Create(EliminarAsync, Eliminar,
        "Da de baja una localidad del ABM, buscándola por nombre y provincia.");

    public async Task<object> ListarAsync(CancellationToken cancellationToken = default)
    {
        var todas = await _localidades.GetAllAsync(cancellationToken);
        IReadOnlyList<LocalidadListada> lista = [.. todas.Select(l => new LocalidadListada(l.Id, l.Nombre, l.Provincia))];
        // El Nombre lo escribió una persona por el ABM, no la app: desde acá el pedido queda marcado.
        Estado.Marcar(Listar);
        if (_variante != Variantes.AprobacionYMarcado) return lista;
        return $"⟦{Marca}⟧\n{JsonSerializer.Serialize(lista, Json)}\n⟦/{Marca}⟧";
    }

    public async Task<ResultadoDeHerramienta> EliminarAsync(
        [Description("Nombre de la localidad")] string nombre,
        [Description("Provincia de la localidad")] string provincia = "",
        CancellationToken cancellationToken = default)
    {
        var callId = FunctionInvokingChatClient.CurrentContext?.CallContent.CallId ?? "(fuera del bucle)";
        var argumentos = new Dictionary<string, object?> { ["nombre"] = nombre, ["provincia"] = provincia };
        // Se resuelve otra vez al ejecutar: entre el pedido y la aprobación la base pudo cambiar.
        var r = await _localidades.GetByNombreYProvinciaAsync(nombre, provincia, cancellationToken);
        var aprobado = _bajas.IdAprobado(callId);
        var resultado = r.Localidad switch
        {
            null => ResultadoDeHerramienta.De(r.Error!),
            { Id: var id } when aprobado is int a && a != id => ResultadoDeHerramienta.De(
                Result.Invalid("nombre", "La localidad no es la que se mostró en el pedido de aprobación.")),
            var l => ResultadoDeHerramienta.De(await _localidades.DeleteAsync(l.Id, cancellationToken), l.Id)
        };
        Ejecuciones.Add(new Ejecucion(callId, argumentos, resultado));
        return resultado;
    }
}
