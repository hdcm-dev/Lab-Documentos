namespace FrameworkConcept.Core.Contracts;

/// <summary>
/// Valores de producto que el PO confirmó (folio 036, ítem 2). Están juntos y con nombre para que la guía
/// los muestre como parámetros y para que nadie los escriba a mano en otro lado.
/// </summary>
public static class ParametrosDeProducto
{
    /// <summary>P1, P3 — Mayor mínimo del Android System WebView (condición C-3 de E01).</summary>
    public const int WebViewMinimo = 94;

    /// <summary>P1, P9 — Piso del cascarón iOS. Diseño sin ensayo, NO VERIFICADO.</summary>
    public const string IosMinimo = "16.4";

    /// <summary>P2 — Vigencia del índice firmado (<c>expires</c> = emitido + 30 días).</summary>
    public static readonly TimeSpan VigenciaDelIndice = TimeSpan.FromDays(30);

    /// <summary>P2 — Cada cuánto se renueva el índice (en la demo lo hace el guion).</summary>
    public static readonly TimeSpan RenovacionDelIndice = TimeSpan.FromDays(7);

    /// <summary>P2 — Al volver desde segundo plano, como mucho una consulta cada 15 minutos.</summary>
    public static readonly TimeSpan IntervaloMinimoAlReanudar = TimeSpan.FromMinutes(15);

    /// <summary>P13.5 — Plazo de aviso para retirar una versión de definición.</summary>
    public static readonly TimeSpan PlazoDeAvisoDeRetiro = TimeSpan.FromDays(30);

    /// <summary>P2 — El servidor retiene al menos 3 paquetes firmados por canal y por mayor del puente.</summary>
    public const int PaquetesRetenidosEnElServidor = 3;

    /// <summary>P2 — El teléfono guarda <c>current</c> y <c>previous</c>.</summary>
    public const int VersionesRetenidasEnElTelefono = 2;

    /// <summary>P2 — Cota del forzado: se activa en el primer arranque en frío desde que quedó verificado.</summary>
    public const int ArranquesHastaAplicarUnaObligatoria = 1;
}
