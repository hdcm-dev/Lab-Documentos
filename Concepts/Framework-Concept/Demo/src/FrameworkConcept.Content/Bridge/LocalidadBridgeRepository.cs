using System.Text.Json;
using FrameworkConcept.Core.Application.Abstractions;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Domain.Entities;

namespace FrameworkConcept.Content.Bridge;

/// <summary>
/// El repositorio de localidades del caso base, ahora sobre los documentos del puente.
///
/// P5 — El contenido no crea tablas: guarda cada localidad como un documento JSON en la colección
/// «localidades» de la base del cascarón. Así, una versión nueva del contenido nunca necesita migrar la base
/// ni un release del cascarón. El servicio de aplicación (<c>LocalidadService</c>) y sus reglas son los del
/// caso base, sin cambios: sólo cambió dónde se guarda.
/// </summary>
public sealed class LocalidadBridgeRepository(BridgeClient puente) : ILocalidadRepository
{
    private const string Coleccion = "localidades";

    /// <summary>Forma del documento. Si un día cambia, sube y el lector tolera las dos (P5, lector tolerante).</summary>
    private const int VersionDeForma = 1;

    // Las dos localidades que siembra el caso base (SessionSeeder), con una marca para no volver a
    // sembrarlas si la persona las borra todas.
    private const string ColeccionDeSesion = "sesion";
    private const string MarcaDeSiembra = "localidades-sembradas";

    private static readonly LocalidadDocumento[] LocalidadesIniciales =
    [
        new(1, "Corrientes", "Corrientes", "3400", 346334),
        new(2, "Resistencia", "Chaco", "3500", 291720)
    ];

    // D-04 — La versión de cada documento leído. Se manda como versionEsperada al modificar: si otro lo
    // cambió entremedio, el cascarón contesta ConflictoDeVersion en lugar de pisar el cambio.
    private readonly Dictionary<int, int> _versiones = [];

    public async Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await SembrarSiHaceFaltaAsync();
        var documentos = await puente.ListDocumentsAsync(Coleccion);
        foreach (var documento in documentos) _versiones[int.Parse(documento.Id)] = documento.Version;
        return [.. documentos.Select(d => ALocalidad(d.Json)).OrderBy(l => l.Id)];
    }

    public async Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var documento = await puente.GetDocumentAsync(Coleccion, id.ToString());
        if (documento is null) return null;
        _versiones[id] = documento.Version;
        return ALocalidad(documento.Json);
    }

    public async Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default)
    {
        var existentes = await puente.ListDocumentsAsync(Coleccion);
        localidad.Id = existentes.Count == 0 ? 1 : existentes.Max(d => int.Parse(d.Id)) + 1;
        _versiones[localidad.Id] = await puente.PutDocumentAsync(Coleccion, localidad.Id.ToString(), VersionDeForma, ADocumento(localidad));
        return localidad.Id;
    }

    public async Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default)
    {
        int? esperada = _versiones.TryGetValue(localidad.Id, out var version) ? version : null;
        _versiones[localidad.Id] = await puente.PutDocumentAsync(Coleccion, localidad.Id.ToString(), VersionDeForma, ADocumento(localidad), esperada);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await puente.DeleteDocumentAsync(Coleccion, id.ToString());
        _versiones.Remove(id);
    }

    private async Task SembrarSiHaceFaltaAsync()
    {
        if (await puente.GetDocumentAsync(ColeccionDeSesion, MarcaDeSiembra) is not null) return;

        foreach (var inicial in LocalidadesIniciales)
            await puente.PutDocumentAsync(Coleccion, inicial.Id.ToString(), VersionDeForma, inicial);
        await puente.PutDocumentAsync(ColeccionDeSesion, MarcaDeSiembra, 1, new { sembradaEn = DateTimeOffset.UtcNow });
    }

    /// <summary>Lo que se guarda: sólo los datos, sin la sesión (la base ya es del dispositivo).</summary>
    private sealed record LocalidadDocumento(int Id, string Nombre, string Provincia, string CodigoPostal, int Habitantes);

    private static LocalidadDocumento ADocumento(Localidad l) => new(l.Id, l.Nombre, l.Provincia, l.CodigoPostal, l.Habitantes);

    private static Localidad ALocalidad(JsonElement json)
    {
        var d = json.Deserialize<LocalidadDocumento>(JsonDelContrato.Opciones)
                ?? throw new JsonException("Documento de localidad vacío.");
        return new Localidad { Id = d.Id, Nombre = d.Nombre, Provincia = d.Provincia, CodigoPostal = d.CodigoPostal, Habitantes = d.Habitantes };
    }
}
