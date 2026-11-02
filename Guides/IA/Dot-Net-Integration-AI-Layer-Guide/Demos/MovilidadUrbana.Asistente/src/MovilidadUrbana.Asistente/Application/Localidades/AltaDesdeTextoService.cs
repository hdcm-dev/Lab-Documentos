// Nuevo. Delta: Agregado por la IA — caso de uso que orquesta extractor y reglas (PD-3; plan §A.4 punto 6, §F.7).
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Application.Localidades;

/// <summary>Resultado de interpretar un pedido de alta. No guarda: la persona confirma antes.</summary>
/// <param name="Interpretado"><c>false</c>: el modelo no devolvió nada utilizable; el camino es el formulario.</param>
/// <param name="Propuesta">Lo leído, ya normalizado por el adaptador.</param>
/// <param name="Faltantes">Campo → requisito que la UI le muestra a la persona para que complete el dato.</param>
public sealed record PropuestaDeAlta(bool Interpretado, LocalidadPropuesta? Propuesta, IReadOnlyDictionary<string, string> Faltantes)
{
    public bool ListaParaConfirmar => Interpretado && Faltantes.Count == 0;
}

/// <summary>
/// «Agregá Bella Vista, en Corrientes…»: el extractor propone y las reglas del dominio deciden. El
/// caso de uso no llama al modelo ni conoce el prompt; sólo conoce el puerto.
/// </summary>
public sealed class AltaDesdeTextoService(IExtractorDeLocalidad extractor)
{
    public async Task<PropuestaDeAlta> ProponerAsync(string texto, CancellationToken cancellationToken = default)
    {
        var propuesta = await extractor.ExtraerAsync(texto, cancellationToken);
        if (propuesta is null)
        {
            return new PropuestaDeAlta(false, null, new Dictionary<string, string>
            {
                ["texto"] = "No pude interpretar el pedido: completá el formulario de alta."
            });
        }

        // Las mismas reglas que SaveAsync; el texto de cada faltante es el requisito de LocalidadPolicy,
        // así la UI pide el dato con la política vigente y no con una frase escrita a mano.
        var faltantes = new Dictionary<string, string>();
        if (!LocalidadRules.NombreValido(propuesta.Nombre)) faltantes["nombre"] = LocalidadPolicy.RequisitoDelNombre;
        if (!LocalidadRules.ProvinciaValida(propuesta.Provincia)) faltantes["provincia"] = LocalidadPolicy.RequisitoDeLaProvincia;
        if (!LocalidadRules.CodigoPostalValido(propuesta.CodigoPostal)) faltantes["codigoPostal"] = LocalidadPolicy.RequisitoDelCodigoPostal;
        if (!LocalidadRules.HabitantesValidos(propuesta.Habitantes)) faltantes["habitantes"] = LocalidadPolicy.RequisitoDeLosHabitantes;

        return new PropuestaDeAlta(true, propuesta, faltantes);
    }
}
