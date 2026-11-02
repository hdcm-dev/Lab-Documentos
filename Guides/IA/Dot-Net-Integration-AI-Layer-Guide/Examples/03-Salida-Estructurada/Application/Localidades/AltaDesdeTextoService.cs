// Nuevo (plan §F.7; ADR-08). Caso de uso «alta desde texto»: pide la propuesta al extractor, la
// normaliza contra Catalogos y deja que DECIDAN LAS REGLAS del dominio. La escritura pasa siempre
// por LocalidadService.SaveAsync, que vuelve a validar: no hay camino de escritura sin las reglas.
using System.Globalization;
using System.Text;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Application.Localidades;

public enum EstadoDeLaPropuesta
{
    /// <summary>Pasa las reglas: se muestra y la persona confirma antes de guardar.</summary>
    Lista,
    /// <summary>Falta o es inválido algún dato: la pantalla lo pide.</summary>
    FaltanDatos,
    /// <summary>El modelo no devolvió el tipo pedido: camino sin IA (el formulario de siempre).</summary>
    SinPropuesta
}

/// <param name="Errores">Campo → mensaje, con las mismas claves que LocalidadService (nombre, provincia, codigoPostal, habitantes).</param>
public sealed record DecisionDeAlta(
    EstadoDeLaPropuesta Estado,
    LocalidadModel Modelo,
    IReadOnlyDictionary<string, string> Errores,
    IReadOnlyList<string> Descartados)
{
    /// <summary>Forma corta y estable de la decisión; la recalcula el oráculo desde el registro.</summary>
    public string Codigo => Estado switch
    {
        EstadoDeLaPropuesta.Lista => "lista",
        EstadoDeLaPropuesta.SinPropuesta => "sin-propuesta",
        _ => "faltan:" + string.Join(",", Errores.Keys.Order(StringComparer.Ordinal))
    };
}

public sealed class AltaDesdeTextoService(IExtractorDeLocalidad extractor, LocalidadService localidades)
{
    public async Task<DecisionDeAlta> ProponerAsync(string texto, CancellationToken cancellationToken = default) =>
        Decidir(await extractor.ExtraerAsync(texto, cancellationToken));

    /// <summary>Lo que deciden las reglas sobre una extracción ya hecha (público para las pruebas y la consola).</summary>
    public static DecisionDeAlta Decidir(ExtraccionDeLocalidad extraccion)
    {
        if (extraccion.Propuesta is not { } p)
        {
            return new DecisionDeAlta(EstadoDeLaPropuesta.SinPropuesta, new LocalidadModel(),
                new Dictionary<string, string>(), extraccion.Descartados);
        }

        var modelo = new LocalidadModel
        {
            Nombre = (p.Nombre ?? string.Empty).Trim(),
            Provincia = NormalizarProvincia(p.Provincia),
            CodigoPostal = (p.CodigoPostal ?? string.Empty).Trim(),
            Habitantes = p.Habitantes
        };
        var errores = Validar(modelo);
        return new DecisionDeAlta(errores.Count == 0 ? EstadoDeLaPropuesta.Lista : EstadoDeLaPropuesta.FaltanDatos,
            modelo, errores, extraccion.Descartados);
    }

    /// <summary>
    /// La persona confirmó (y, si faltaba, completó) los datos. Lo que ella escribe reemplaza a lo
    /// propuesto. Guarda SaveAsync, que aplica otra vez las reglas.
    /// </summary>
    public Task<Result> ConfirmarAsync(DecisionDeAlta decision, int? habitantesConfirmados = null,
        string? codigoPostalConfirmado = null, CancellationToken cancellationToken = default)
    {
        if (decision.Estado == EstadoDeLaPropuesta.SinPropuesta)
        {
            return Task.FromResult(Result.Invalid("nombre", "No hay propuesta: completá el formulario."));
        }
        var modelo = new LocalidadModel
        {
            Nombre = decision.Modelo.Nombre,
            Provincia = decision.Modelo.Provincia,
            CodigoPostal = codigoPostalConfirmado ?? decision.Modelo.CodigoPostal,
            Habitantes = habitantesConfirmados ?? decision.Modelo.Habitantes
        };
        return localidades.SaveAsync(modelo, cancellationToken);
    }

    /// <summary>
    /// «corrientes» o «Corrientes » pasan a la clave exacta del catálogo; lo que no coincide (p. ej.
    /// «Corrientes Capital») queda como vino y lo rechaza LocalidadRules.ProvinciaValida.
    /// </summary>
    public static string NormalizarProvincia(string? provincia)
    {
        var limpia = (provincia ?? string.Empty).Trim();
        var clave = Plegar(limpia);
        return Catalogos.Provincias.FirstOrDefault(p => Plegar(p) == clave) ?? limpia;
    }

    /// <summary>Minúsculas y sin tildes (la misma regla la reimplementa el oráculo en jq).</summary>
    public static string Plegar(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    // Mismos mensajes y claves que LocalidadService.Validate (privado en el caso base): la pantalla
    // muestra el aviso antes de que la persona confirme. El del nombre sigue al de _Comun desde el
    // endurecimiento de LocalidadRules (REC-17): «entre 3 y 60».
    private static Dictionary<string, string> Validar(LocalidadModel m)
    {
        var errores = new Dictionary<string, string>();
        if (!LocalidadRules.NombreValido(m.Nombre))
            errores["nombre"] = $"El nombre debe tener entre {LocalidadRules.LargoMinimoDelNombre} y {LocalidadRules.LargoMaximoDelNombre} caracteres.";
        if (!LocalidadRules.ProvinciaValida(m.Provincia))
            errores["provincia"] = string.IsNullOrWhiteSpace(m.Provincia) ? "Seleccione una provincia." : "La provincia no está en el catálogo.";
        if (!LocalidadRules.CodigoPostalValido(m.CodigoPostal))
            errores["codigoPostal"] = $"El código postal debe tener {LocalidadRules.DigitosDelCodigoPostal} dígitos.";
        if (!LocalidadRules.HabitantesValidos(m.Habitantes))
            errores["habitantes"] = "Ingrese una cantidad de habitantes mayor a cero.";
        return errores;
    }
}
