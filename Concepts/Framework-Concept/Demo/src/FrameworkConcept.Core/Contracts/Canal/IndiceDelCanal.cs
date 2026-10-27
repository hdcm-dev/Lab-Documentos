namespace FrameworkConcept.Core.Contracts.Canal;

/// <summary>
/// El índice del canal: la lista firmada de lo que se puede bajar (P2).
///
/// Vive en una ruta fija, <c>/{canal}/puente-{mayor}/indice.json</c> (ver <see cref="RutasDelCanal"/>): hay un
/// índice por canal (dev, beta, prod) y por mayor del puente, y cada cascarón lee el de su mayor.
/// Se firma byte a byte; la firma va al lado, en <c>indice.json.sig</c> (<see cref="FirmaDelIndice"/>).
///
/// En el glosario de la guía (P10) es el «índice del canal». NO es el «manifiesto del paquete de contenido»
/// (<see cref="Contenido.ManifiestoContenido"/>, que viaja dentro del zip) ni el «manifiesto de la app»
/// (AndroidManifest.xml / Info.plist, que sólo cambia por tienda).
/// </summary>
/// <param name="Formato">Versión de la forma de este archivo. Hoy, 1.</param>
/// <param name="Canal">dev, beta o prod. El cliente rechaza el índice de otro canal.</param>
/// <param name="PuenteMayor">Mayor del puente al que sirve. El cliente rechaza el de otro mayor.</param>
/// <param name="KeyId">Clave con la que se firmó. El cliente acepta sólo claves conocidas y no revocadas.</param>
/// <param name="Emitido">Cuándo se generó.</param>
/// <param name="Expires">Desde cuándo deja de valer. P2: emitido + 30 días; se renueva cada 7.</param>
/// <param name="Entradas">Lo que se puede bajar: contenido (código) y definiciones (datos).</param>
/// <param name="Retiradas">P2 — Versiones de contenido retiradas: el próximo arranque en frío las trata como fallidas.</param>
/// <param name="DefinicionesRetiradas">P13.5 — Retiros de definiciones, anunciados con su fecha.</param>
/// <param name="CascaronMinimoSoportado">P2 — Política de soporte (no una declaración de compatibilidad).</param>
public sealed record IndiceDelCanal(
    int Formato,
    string Canal,
    int PuenteMayor,
    string KeyId,
    DateTimeOffset Emitido,
    DateTimeOffset Expires,
    EntradasDelIndice Entradas,
    IReadOnlyList<VersionSemantica> Retiradas,
    IReadOnlyList<DefinicionRetirada> DefinicionesRetiradas,
    VersionSemantica? CascaronMinimoSoportado = null)
{
    public const int FormatoActual = 1;
}

/// <summary>P1, P2 — Dos clases de entradas en el mismo índice, aceptables por separado.</summary>
public sealed record EntradasDelIndice(
    IReadOnlyList<EntradaDeContenido> Contenido,
    IReadOnlyList<EntradaDeDefinicion> Definiciones);

/// <summary>Un paquete de contenido (zip plano, sin .br ni .gz: condición C-4 de E01).</summary>
/// <param name="Version">SemVer con +commit y sin pre-release (P7).</param>
/// <param name="Ruta">Relativa a la carpeta del índice.</param>
/// <param name="Size">Bytes del zip.</param>
/// <param name="Sha256">Hash del zip, en hex minúscula.</param>
/// <param name="Mandatory">P2 — Obligatoria: se activa en el próximo arranque en frío, igual que las demás, y la guía lo dice.</param>
public sealed record EntradaDeContenido(
    VersionSemantica Version,
    string Ruta,
    long Size,
    string Sha256,
    bool Mandatory = false);

/// <summary>Una versión de una definición de encuesta (datos, no código).</summary>
/// <param name="CompatibleConContenido">P1 — «compatible-con-contenido»: qué contenidos saben dibujarla.</param>
public sealed record EntradaDeDefinicion(
    string EncuestaId,
    int Version,
    string Ruta,
    long Size,
    string Sha256,
    RangoDeVersion CompatibleConContenido);

/// <summary>
/// P13.5 — Retirar una versión de definición es un acto explícito, anunciado en el índice firmado con
/// <see cref="ParametrosDeProducto.PlazoDeAvisoDeRetiro"/> de aviso. Una respuesta de una versión retirada no
/// se borra: queda en carta muerta, visible.
/// </summary>
public sealed record DefinicionRetirada(string EncuestaId, int Version, DateTimeOffset AnunciadaEl, DateTimeOffset RetiroEl)
{
    /// <summary>¿Respeta el plazo de aviso?</summary>
    public bool RespetaElPlazoDeAviso => RetiroEl - AnunciadaEl >= ParametrosDeProducto.PlazoDeAvisoDeRetiro;
}
