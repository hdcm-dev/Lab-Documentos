// Nuevo. Delta: Agregado por la IA — herramientas como casos de uso, armadas por circuito (plan §F.3, §F.4; ADR-06, ADR-12).
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Domain;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

/// <summary>Lo que devuelve <c>listar_localidades</c>: id, nombre y provincia (H-011). El nombre es texto no confiable.</summary>
public sealed record LocalidadListada(int Id, string Nombre, string Provincia);

/// <summary>Lo que devuelve <c>contar_localidades</c>.</summary>
public sealed record ConteoDeLocalidades(string Provincia, int Cantidad);

/// <summary>
/// Cada herramienta llama a un caso de uso; ninguna recibe un repositorio (prueba de arquitectura,
/// regla 3). Se arman por circuito con <see cref="AIFunctionFactory" /> sobre métodos de esta
/// instancia, resuelta en el scope del circuito: la sesión sale del scope, nunca de un argumento,
/// y ninguna firma tiene parámetros que resuelva DI.
///
/// Las escrituras (<c>agregar_localidad</c>, <c>eliminar_localidad</c>) ejecutan lo que el código
/// resolvió y la persona aprobó (<see cref="EstadoDelPedido.Aprobada" />), no lo que el modelo mande
/// después: los argumentos del modelo sólo sirven para armar el pedido de aprobación.
/// </summary>
public sealed class HerramientasDelAsistente(
    LocalidadService localidades,
    ResultadosDeEncuestaService resultados,
    NormalizadorDeCatalogo normalizador,
    EstadoDelPedido estado)
{
    public const string Listar = "listar_localidades";
    public const string Contar = "contar_localidades";
    public const string Resumir = "resumir_encuesta";
    public const string Agregar = "agregar_localidad";
    public const string Eliminar = "eliminar_localidad";

    public static readonly IReadOnlySet<string> Escrituras = new HashSet<string> { Agregar, Eliminar };

    /// <summary>
    /// Las herramientas del pedido. <paramref name="conEscrituras" /> en <c>false</c> es la regla 1 de
    /// la política de origen: si el pedido trae contexto no confiable al armarse, no hay escrituras.
    /// <paramref name="escriturasAprobables" /> en <c>false</c> existe sólo para probar la regla 2
    /// (una escritura no aprobable después de texto no confiable se rechaza); la demo siempre las
    /// arma aprobables (toda escritura pide aprobación, H-012).
    /// </summary>
    public IList<AITool> Crear(bool conEscrituras = true, bool escriturasAprobables = true)
    {
        var herramientas = new List<AITool>
        {
            AIFunctionFactory.Create(ListarLocalidadesAsync, Listar,
                "Lista las localidades de la sesión con id, nombre y provincia."),
            AIFunctionFactory.Create(ContarLocalidadesAsync, Contar,
                $"Cuenta las localidades de la sesión en una provincia. Provincias: {string.Join(", ", Catalogos.Provincias)}."),
            AIFunctionFactory.Create(ResumirEncuestaAsync, Resumir,
                "Porcentajes de la encuesta de la sesión por medio de transporte, frecuencia y motivo. Sin datos de personas.")
        };
        if (!conEscrituras) return herramientas;

        AIFunction agregar = new GuardaDeOrigen(AIFunctionFactory.Create(AgregarLocalidadAsync, Agregar,
            "Da de alta una localidad en la sesión. La app le pide confirmación a la persona antes de guardar."), estado);
        AIFunction eliminar = new GuardaDeOrigen(AIFunctionFactory.Create(EliminarLocalidadAsync, Eliminar,
            "Da de baja una localidad de la sesión por nombre y, si hace falta, provincia. La app pide confirmación."), estado);
        herramientas.Add(escriturasAprobables ? new ApprovalRequiredAIFunction(agregar) : agregar);
        herramientas.Add(escriturasAprobables ? new ApprovalRequiredAIFunction(eliminar) : eliminar);
        return herramientas;
    }

    public async Task<IReadOnlyList<LocalidadListada>> ListarLocalidadesAsync(CancellationToken cancellationToken = default)
    {
        // Los nombres los escribió una persona, no la app: texto no confiable (H2-012).
        estado.MarcarNoConfiable();
        return (await localidades.GetAllAsync(cancellationToken)).Select(l => new LocalidadListada(l.Id, l.Nombre, l.Provincia)).ToList();
    }

    public async Task<ConteoDeLocalidades> ContarLocalidadesAsync(string provincia, CancellationToken cancellationToken = default)
    {
        var clave = normalizador.Provincia(provincia);
        if (clave is null || !Catalogos.Provincias.Contains(clave, StringComparer.Ordinal))
        {
            throw new ArgumentException($"«{provincia}» no es una provincia del catálogo: {string.Join(", ", Catalogos.Provincias)}.");
        }
        var cantidad = (await localidades.GetAllAsync(cancellationToken)).Count(l => l.Provincia == clave);
        return new ConteoDeLocalidades(clave, cantidad);
    }

    /// <summary>Agregados, nunca filas: el tipo de retorno no tiene Nombre ni Edad (T5; OWASP LLM02).</summary>
    public Task<ResultadosDeEncuesta> ResumirEncuestaAsync(CancellationToken cancellationToken = default) =>
        resultados.ResumirAsync(cancellationToken);

    public async Task<string> AgregarLocalidadAsync(
        string nombre, string provincia, string codigoPostal, int? habitantes, CancellationToken cancellationToken = default)
    {
        var modelo = estado.Aprobada(FunctionInvokingChatClient.CurrentContext?.CallContent.CallId) is AltaResuelta alta
            ? alta.Modelo
            : new LocalidadModel { Nombre = nombre, Provincia = normalizador.Provincia(provincia) ?? string.Empty, CodigoPostal = codigoPostal, Habitantes = habitantes };
        var resultado = await localidades.SaveAsync(modelo, cancellationToken);
        return resultado.IsSuccess
            ? $"{resultado.Message} (id {resultado.Id})"
            : $"No se guardó: {string.Join(" ", resultado.Errors.Values)}";
    }

    public async Task<string> EliminarLocalidadAsync(string nombre, string? provincia = null, CancellationToken cancellationToken = default)
    {
        int id;
        if (estado.Aprobada(FunctionInvokingChatClient.CurrentContext?.CallContent.CallId) is BajaResuelta baja)
        {
            id = baja.Id;
        }
        else
        {
            var resolucion = await localidades.GetByNombreYProvinciaAsync(nombre, normalizador.Provincia(provincia), cancellationToken);
            if (resolucion.Localidad is null) return $"No se eliminó: {string.Join(" ", resolucion.Error!.Errors.Values)}";
            id = resolucion.Localidad.Id;
        }
        var resultado = await localidades.DeleteAsync(id, cancellationToken);
        return resultado.IsSuccess ? resultado.Message : $"No se eliminó: {string.Join(" ", resultado.Errors.Values)}";
    }
}
