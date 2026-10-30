using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;
using FrameworkConcept.Core.Update;
using FrameworkConcept.Shell.Actualizacion;
using FrameworkConcept.Shell.Catalogo;
using FrameworkConcept.Shell.Data;
using FrameworkConcept.Shell.Sync;

namespace FrameworkConcept.Shell.Hosting;

/// <summary>
/// La preparación del arranque en frío, en orden (sale de ArranquePage para que el arranque de siempre y E04 corran el
/// mismo método):
///   1. abrir la base y comprobar su número de esquema (C-1: si no coincide, no arranca);
///   2. arrancar el monitor de red, restaurar la sesión (folio 038) y la sincronización del outbox (es del cascarón:
///      corre pase lo que pase con el contenido, P5);
///   3. preparar el arranque con el actualizador, sin red (P2): rollback por marca, retiradas, activación de lo
///      verificado en staging/, semilla en el primer arranque, WebView ≥ 94 (C-3);
///   4. abrir la puerta del arranque: desde acá el puente responde el saludo del contenido (E04).
/// </summary>
public sealed class PreparacionDelArranque(
    IDb db,
    ProveedorDelActualizador proveedor,
    ContenidoActivo contenido,
    SincronizadorDelOutbox sincronizador,
    IMonitorDeRed red,
    ISesionDelUsuario sesion,
    ServidorDeContenido servidor,
    PuertaDelArranque puerta)
{
    public async Task<ResultadoDeArranque> PrepararAsync()
    {
        try
        {
#if FC_E04_V2
            return await PrepararSuperpuestoAsync();
#else
            await db.AbrirAsync();
            red.Iniciar();                 // borrador §6 — antes que el sincronizador, que lee su estado
            await sesion.IniciarAsync();   // P4 — SinCuenta, Bloqueada o Vencida; no toca la red
            sincronizador.Iniciar();

            foreach (var diferencia in ImplementacionesDelCatalogo.Diferencias())
                Bitacora.Log("catálogo: " + diferencia);

            var actualizador = await proveedor.ObtenerAsync();
            var resultado = await actualizador.PrepararArranqueAsync();

            // Una sola vez por proceso: no hay activación en caliente (P2).
            contenido.Fijar(resultado.Version, resultado.Directorio, resultado.Notas);
            foreach (var nota in resultado.Notas) Bitacora.Log("actualizador: " + nota);
            Bitacora.Log($"arranque: {resultado.Estado} {resultado.Version}");
#if FC_E04
            // E04 — Sin versión especulada el servidor está esperando la raíz: se fija ANTES de abrir la puerta, para que
            // el pedido retenido encuentre la versión decidida. Si había una especulada distinta, la reemplaza la página.
            if (resultado.Estado == EstadoDeArranque.ContenidoListo && servidor.Raiz is null)
                servidor.Raiz = resultado.Directorio;
#else
            _ = servidor;   // sin E04 la raíz la fija la pantalla de contenido, como siempre
#endif
            puerta.Abrir(resultado);
            if (resultado.Version is { } version)
                await db.AnotarVersionDeContenidoAsync(version.ToString(), "arranque");
            return resultado;
#endif
        }
        catch (Exception ex)
        {
            puerta.Fallar(ex);
            throw;
        }
    }

#if FC_E04_V2
    /// <summary>
    /// E04 V2 — El mismo arranque, superpuesto: las claves (lectura del APK) en paralelo con abrir la base; la sesión en
    /// paralelo con el actualizador; la puerta se abre apenas decide el actualizador; el sincronizador, el catálogo, las
    /// notas y la anotación de la versión van después, fuera del camino crítico. C-1 sigue antes del saludo.
    /// </summary>
    private async Task<ResultadoDeArranque> PrepararSuperpuestoAsync()
    {
        var claves = proveedor.ObtenerAsync();
        await db.AbrirAsync();             // C-1 — si el esquema no coincide, no arranca (ni se responde el saludo)
        red.Iniciar();                     // barato; network.status lo pide antes del primer render
        var restauracion = Task.Run(sesion.IniciarAsync);   // P4 — en paralelo con el actualizador

        var resultado = await (await claves).PrepararArranqueAsync();
        contenido.Fijar(resultado.Version, resultado.Directorio, resultado.Notas);
        Bitacora.Log($"arranque: {resultado.Estado} {resultado.Version}");
        if (resultado.Estado == EstadoDeArranque.ContenidoListo && servidor.Raiz is null)
            servidor.Raiz = resultado.Directorio;
        puerta.Abrir(resultado);           // el saludo se libera ACÁ

        _ = Task.Run(async () =>
        {
            try
            {
                await restauracion;
                sincronizador.Iniciar();
                puerta.SegundoPlanoTerminado();
                Bitacora.Log("arranque: segundo plano listo (sesión, sincronizador)");
            }
            catch (Exception ex)
            {
                Bitacora.Error("arranque: segundo plano", ex);
                puerta.SegundoPlanoFallo(ex);   // los métodos que la esperan responden ErrorInterno
                return;
            }
            try
            {
                foreach (var diferencia in ImplementacionesDelCatalogo.Diferencias())
                    Bitacora.Log("catálogo: " + diferencia);
                foreach (var nota in resultado.Notas) Bitacora.Log("actualizador: " + nota);
                if (resultado.Version is { } version)
                    await db.AnotarVersionDeContenidoAsync(version.ToString(), "arranque");
            }
            catch (Exception ex)
            {
                Bitacora.Error("arranque: anotaciones", ex);
            }
        });
        return resultado;
    }
#endif
}
