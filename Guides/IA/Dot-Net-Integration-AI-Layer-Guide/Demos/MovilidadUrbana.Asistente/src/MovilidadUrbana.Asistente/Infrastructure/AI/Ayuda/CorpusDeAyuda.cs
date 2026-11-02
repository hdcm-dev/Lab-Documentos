// Nuevo. Delta: Agregado por la IA — ingesta de los 12 literales de *Policy (plan §F.6, H2-027).
using System.Reflection;
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Localidades;

namespace MovilidadUrbana.Asistente.Infrastructure.AI.Ayuda;

/// <summary>Un fragmento del corpus: id calificado por clase, texto según la plantilla y su fuente.</summary>
public sealed record FragmentoDelCorpus(string Id, string Texto);

/// <summary>
/// El corpus de la ayuda son las propiedades <c>public static string</c> de <see cref="LocalidadPolicy" />
/// (4) y <see cref="EncuestaPolicy" /> (8), leídas por reflexión al arrancar: si cambia una regla,
/// cambia el requisito y cambia la ayuda (N6 P3). Un fragmento por propiedad, con id calificado por
/// clase porque <c>RequisitoDelNombre</c> existe en las dos.
///
/// Plantilla <c>plantilla.v1</c>: «{tema}, {campo}: {requisito}». Los rótulos de tema y campo son
/// ESCRITOS PARA EL EJEMPLO; el requisito es el valor vivo de la política. (La plantilla es la que
/// usará 06; si 06 la cambia, se versiona como plantilla.v2 y se rehacen los embeddings.)
/// </summary>
public static class CorpusDeAyuda
{
    public const string Plantilla = "plantilla.v1";

    private static readonly Dictionary<string, string> Campos = new()
    {
        ["LocalidadPolicy.RequisitoDelNombre"] = "nombre de la localidad",
        ["LocalidadPolicy.RequisitoDeLaProvincia"] = "provincia",
        ["LocalidadPolicy.RequisitoDelCodigoPostal"] = "código postal",
        ["LocalidadPolicy.RequisitoDeLosHabitantes"] = "cantidad de habitantes",
        ["EncuestaPolicy.RequisitoDelNombre"] = "nombre de quien responde",
        ["EncuestaPolicy.RequisitoDeLaEdad"] = "edad",
        ["EncuestaPolicy.RequisitoDeLaLocalidad"] = "localidad",
        ["EncuestaPolicy.RequisitoDeLosMedios"] = "medios de transporte",
        ["EncuestaPolicy.RequisitoDeLaFrecuencia"] = "frecuencia de uso",
        ["EncuestaPolicy.RequisitoDeLaDistancia"] = "distancia recorrida",
        ["EncuestaPolicy.RequisitoDeLosMinutos"] = "tiempo de viaje",
        ["EncuestaPolicy.RequisitoDelMotivo"] = "motivo del viaje"
    };

    public static IReadOnlyList<FragmentoDelCorpus> Fragmentos()
    {
        var fragmentos = new List<FragmentoDelCorpus>();
        foreach (var (clase, tema) in new[] { (typeof(LocalidadPolicy), "Alta de una localidad"), (typeof(EncuestaPolicy), "Encuesta de transporte") })
        {
            foreach (var propiedad in clase.GetProperties(BindingFlags.Public | BindingFlags.Static)
                         .Where(p => p.PropertyType == typeof(string))
                         .OrderBy(p => p.MetadataToken))
            {
                var id = $"{clase.Name}.{propiedad.Name}";
                var campo = Campos.TryGetValue(id, out var c)
                    ? c
                    : throw new InvalidOperationException($"La política tiene un requisito nuevo sin rótulo en el corpus: {id}.");
                fragmentos.Add(new FragmentoDelCorpus(id, $"{tema}, {campo}: {(string)propiedad.GetValue(null)!}"));
            }
        }
        return fragmentos;
    }
}
