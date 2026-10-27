using FrameworkConcept.Core.Update;

namespace FrameworkConcept.Shell.Actualizacion;

/// <summary>
/// P2 — «Cuándo» se consulta el canal. La consulta corre SIEMPRE en segundo plano (N = 0: nunca se espera al abrir) y
/// nunca activa nada: lo que baja queda en staging/ y se activa en el próximo arranque en frío.
///
/// Disparadores de la demo: arranque en frío (después de mostrar el contenido) y reanudación (volver desde segundo
/// plano; el actualizador la omite si no pasaron 15 minutos desde la última exitosa). El tercero, «login exitoso», no
/// se ejecuta: la demo no tiene login (P4).
/// </summary>
public sealed class ConsultasDelCanal(ProveedorDelActualizador proveedor)
{
    private int _enCurso;

    /// <summary>El resultado de la última consulta y cuándo terminó, para la pantalla de diagnóstico.</summary>
    public (ResultadoDeConsulta Resultado, DateTimeOffset Cuando)? Ultima { get; private set; }

    /// <summary>Avisa cuando termina una consulta (la pantalla «sin contenido sano» la usa para reintentar el arranque).</summary>
    public event Action<ResultadoDeConsulta>? Terminada;

    /// <summary>Lanza la consulta y vuelve enseguida. Si ya hay una en curso, no lanza otra.</summary>
    public void ConsultarEnSegundoPlano(DisparadorDeConsulta disparador)
    {
        if (Interlocked.CompareExchange(ref _enCurso, 1, 0) != 0)
        {
            Bitacora.Log($"consulta ({disparador}): ya hay una en curso");
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var actualizador = await proveedor.ObtenerAsync();
                var resultado = await actualizador.ConsultarAsync(disparador);
                Ultima = (resultado, DateTimeOffset.Now);
                Bitacora.Log($"consulta ({disparador}): {resultado.Estado} {resultado.Motivo} — {resultado.Detalle}");
                Terminada?.Invoke(resultado);
            }
            catch (Exception ex)
            {
                Bitacora.Error($"consulta ({disparador})", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _enCurso, 0);
            }
        });
    }
}
