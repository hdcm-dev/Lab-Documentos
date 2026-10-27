namespace FrameworkConcept.Core.Contracts.Puente;

/// <summary>Respuesta de <c>bridge.handshake</c> (P3, P12): con esto el contenido decide qué funciones opcionales muestra.</summary>
public sealed record RespuestaDeHandshake(
    VersionSemantica VersionPuente,
    IReadOnlyList<EntradaVersionada> Catalogo,
    string HuellaContrato);

/// <summary>Parámetros de <c>bridge.ready</c>, el «Listo(version)» del dictamen (P2).</summary>
public sealed record PedidoListo(VersionSemantica Version);

/// <summary>Un documento JSON del contenido (P5): colección, id, versión de forma y el JSON.</summary>
public sealed record Documento(string Coleccion, string Id, int VersionDeForma, int Version, System.Text.Json.JsonElement Json);
