using FrameworkConcept.Core.Contracts;

namespace FrameworkConcept.Core.Update;

/// <summary>
/// El actualizador del cascarón (P2). Es «puro»: no toca archivos, red ni reloj directamente, sino por
/// <see cref="IAlmacenDeContenido"/>, <see cref="IClienteDeCanal"/>, <see cref="IReloj"/> y
/// <see cref="IVerificadorDeFirma"/>. Por eso se prueba sin teléfono (FrameworkConcept.Update.Tests).
///
/// Dos momentos, nunca mezclados:
///   1. <see cref="PrepararArranqueAsync"/> — en el arranque en frío, ANTES de mostrar. No usa la red.
///      Rollback por marca, retiradas, activación de lo ya preparado en staging/.
///   2. <see cref="ConsultarAsync"/> — en segundo plano, después de mostrar (N = 0: no se espera al abrir).
///      Baja y verifica; deja lo nuevo en staging/. NO activa: no hay activación en caliente.
/// </summary>
public interface IActualizador
{
    /// <summary>
    /// Arranque en frío, antes de mostrar. En orden:
    ///   — si encuentra la marca «pendiente» y la versión ya empezó a cargar en
    ///     <see cref="Actualizador.CargasSinListoParaFallar"/> arranques sin llamar a Listo, vuelve a previous y anota
    ///     esa versión como fallida; si no, la deja pendiente y la vuelve a intentar;
    ///   — si current figura en <c>retiradas</c> del último índice válido, la trata como fallida;
    ///   — si hay una versión verificada en staging/ y compatible, la activa: versions/vN inmutable y
    ///     puntero.json reemplazado con un solo rename; pone la marca «pendiente»;
    ///   — si no queda versión sana, devuelve <see cref="EstadoDeArranque.SinContenidoSano"/> (no vuelve a la semilla).
    /// </summary>
    Task<ResultadoDeArranque> PrepararArranqueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el índice y baja lo nuevo. Orden de verificación (P2): firma y clave no revocada →
    /// <c>expires</c> → canal y mayor → versión estrictamente mayor → tamaño y sha256 → descomprimir en
    /// staging/ → predicado de compatibilidad (P3). Las definiciones nuevas se guardan en
    /// <see cref="IAlmacenDeDefiniciones"/>; se usan al empezar la próxima encuesta.
    /// </summary>
    Task<ResultadoDeConsulta> ConsultarAsync(DisparadorDeConsulta disparador, CancellationToken cancellationToken = default);

    /// <summary>«Listo(version)»: el contenido arrancó bien; se borra la marca «pendiente».</summary>
    Task ConfirmarListoAsync(VersionSemantica version, CancellationToken cancellationToken = default);

    /// <summary>
    /// El WebView empezó a cargar esta versión (pidió <c>_framework/dotnet.js</c>). Si es la pendiente, suma un
    /// arranque con la carga empezada; sólo esos cuentan para el rollback (ver <see cref="Actualizador.CargasSinListoParaFallar"/>).
    /// </summary>
    Task AnotarCargaIniciadaAsync(VersionSemantica version, CancellationToken cancellationToken = default);
}
