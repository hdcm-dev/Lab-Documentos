using FrameworkConcept.Core.Contracts.Encuestas;
using Microsoft.EntityFrameworkCore;

namespace FrameworkConcept.Shell.Data;

/// <summary>
/// <see cref="IDb"/> sobre EF Core SQLite (P5). Un DbContext nuevo por operación (EF Core no es seguro entre hilos) y
/// un semáforo para que el puente, el sincronizador y el actualizador no escriban a la vez sobre el mismo archivo.
/// </summary>
public sealed class DbSqlite : IDb
{
    private readonly DbContextOptions<ContextoDelCascaron> _opciones;
    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private readonly string _ruta;

    public DbSqlite(string rutaDelArchivo)
    {
        _ruta = rutaDelArchivo;
        _opciones = new DbContextOptionsBuilder<ContextoDelCascaron>()
            .UseSqlite($"Data Source={rutaDelArchivo}")
            .Options;
    }

    private static long Ahora => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public Task AbrirAsync(CancellationToken ct = default) => ConCerrojo(async db =>
    {
        // P5 / C-1 — sobre fijo: se crea entero la primera vez; después sólo se comprueba el número.
        await db.Database.OpenConnectionAsync(ct);
        var version = await LeerVersionDeEsquemaAsync(db, ct);

        if (version == 0)
        {
            await db.Database.EnsureCreatedAsync(ct);
            await db.Database.ExecuteSqlRawAsync($"PRAGMA user_version = {EsquemaDelCascaron.Version}", ct);
            Bitacora.Log($"db: creada {_ruta} con esquema {EsquemaDelCascaron.Version}");
        }
        else if (version != EsquemaDelCascaron.Version)
        {
            throw new EsquemaIncompatibleException(version, EsquemaDelCascaron.Version);
        }
        else
        {
            Bitacora.Log($"db: esquema {version} comprobado");
        }
        return 0;
    }, ct);

    private static async Task<int> LeerVersionDeEsquemaAsync(ContextoDelCascaron db, CancellationToken ct)
    {
        await using var comando = db.Database.GetDbConnection().CreateCommand();
        comando.CommandText = "PRAGMA user_version";
        return Convert.ToInt32(await comando.ExecuteScalarAsync(ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    // --- Documentos ---

    public Task<FilaDeDocumento?> ObtenerDocumentoAsync(string coleccion, string id, CancellationToken ct = default) =>
        ConCerrojo(db => db.Documentos.AsNoTracking().FirstOrDefaultAsync(d => d.Coleccion == coleccion && d.Id == id, ct), ct);

    public Task<IReadOnlyList<FilaDeDocumento>> ListarDocumentosAsync(string coleccion, CancellationToken ct = default) =>
        ConCerrojo<IReadOnlyList<FilaDeDocumento>>(async db =>
            await db.Documentos.AsNoTracking().Where(d => d.Coleccion == coleccion).OrderBy(d => d.Id).ToListAsync(ct), ct);

    public Task<int> GuardarDocumentoAsync(string coleccion, string id, int versionDeForma, string json, int? versionEsperada, CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            var fila = await db.Documentos.FirstOrDefaultAsync(d => d.Coleccion == coleccion && d.Id == id, ct);
            var actual = fila?.Version ?? 0;
            if (versionEsperada is int esperada && esperada != actual)
                throw new ConflictoDeVersionException(esperada, actual);

            if (fila is null)
            {
                fila = new FilaDeDocumento { Coleccion = coleccion, Id = id };
                db.Documentos.Add(fila);
            }
            fila.VersionDeForma = versionDeForma;
            fila.Json = json;
            fila.Version = actual + 1;
            fila.ActualizadoEn = Ahora;
            await db.SaveChangesAsync(ct);
            return fila.Version;
        }, ct);

    public Task<bool> BorrarDocumentoAsync(string coleccion, string id, CancellationToken ct = default) =>
        ConCerrojo(async db => await db.Documentos.Where(d => d.Coleccion == coleccion && d.Id == id).ExecuteDeleteAsync(ct) > 0, ct);

    public Task<bool> GuardarYEncolarAsync(FilaDeDocumento documento, FilaDeOutbox fila, CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            if (await db.Outbox.AnyAsync(f => f.ClaveIdempotencia == fila.ClaveIdempotencia, ct))
                return false;

            var existente = await db.Documentos.FirstOrDefaultAsync(d => d.Coleccion == documento.Coleccion && d.Id == documento.Id, ct);
            if (existente is null)
            {
                documento.Version = 1;
                documento.ActualizadoEn = Ahora;
                db.Documentos.Add(documento);
            }
            else
            {
                existente.Json = documento.Json;
                existente.VersionDeForma = documento.VersionDeForma;
                existente.Version++;
                existente.ActualizadoEn = Ahora;
            }

            fila.CreadaEn = Ahora;
            fila.ProximoIntentoEn = Ahora;
            db.Outbox.Add(fila);

            // Un solo SaveChanges = una sola transacción de SQLite: el documento y la fila quedan juntos (P5).
            await db.SaveChangesAsync(ct);
            return true;
        }, ct);

    // --- Outbox ---

    public Task<IReadOnlyList<FilaDeOutbox>> ListarOutboxSinEnviarAsync(CancellationToken ct = default) =>
        ConCerrojo<IReadOnlyList<FilaDeOutbox>>(async db =>
            await db.Outbox.AsNoTracking().Where(f => f.Estado != EstadoDeFila.Enviada).OrderBy(f => f.Id).ToListAsync(ct), ct);

    public Task ActualizarFilaAsync(FilaDeOutbox fila, CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            db.Outbox.Update(fila);
            await db.SaveChangesAsync(ct);
            return 0;
        }, ct);

    public Task<(int Pendientes, int Rechazadas)> ContarOutboxAsync(CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            var pendientes = await db.Outbox.CountAsync(f => f.Estado == EstadoDeFila.Pendiente, ct);
            var rechazadas = await db.Outbox.CountAsync(f => f.Estado == EstadoDeFila.Rechazada || f.Estado == EstadoDeFila.Error, ct);
            return (pendientes, rechazadas);
        }, ct);

    public Task<(int Pendientes, int Rechazadas)> ContarOutboxDeAsync(string? dueno, CancellationToken ct = default) =>
        dueno is null
            ? ContarOutboxAsync(ct)
            : ConCerrojo(async db =>
            {
                var delDueno = db.Outbox.Where(f => f.Dueno == dueno);
                var pendientes = await delDueno.CountAsync(f => f.Estado == EstadoDeFila.Pendiente, ct);
                var rechazadas = await delDueno.CountAsync(f => f.Estado == EstadoDeFila.Rechazada || f.Estado == EstadoDeFila.Error, ct);
                return (pendientes, rechazadas);
            }, ct);

    // --- Definiciones ---

    public Task GuardarDefinicionAsync(DefinicionEncuesta definicion, string json, CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            // Una versión publicada no cambia: si ya está guardada, se deja como estaba (P13.9).
            if (!await db.Definiciones.AnyAsync(d => d.EncuestaId == definicion.EncuestaId && d.Version == definicion.Version, ct))
            {
                db.Definiciones.Add(new FilaDeDefinicion
                {
                    EncuestaId = definicion.EncuestaId,
                    Version = definicion.Version,
                    Json = json,
                    GuardadaEn = Ahora
                });
                await db.SaveChangesAsync(ct);
            }
            return 0;
        }, ct);

    public Task<FilaDeDefinicion?> ObtenerDefinicionAsync(string encuestaId, int version, CancellationToken ct = default) =>
        ConCerrojo(db => db.Definiciones.AsNoTracking().FirstOrDefaultAsync(d => d.EncuestaId == encuestaId && d.Version == version, ct), ct);

    public Task<IReadOnlyList<FilaDeDefinicion>> ListarDefinicionesAsync(string? encuestaId = null, CancellationToken ct = default) =>
        ConCerrojo<IReadOnlyList<FilaDeDefinicion>>(async db =>
            await db.Definiciones.AsNoTracking()
                .Where(d => encuestaId == null || d.EncuestaId == encuestaId)
                .OrderBy(d => d.EncuestaId).ThenBy(d => d.Version)
                .ToListAsync(ct), ct);

    // --- Versión de contenido y configuración ---

    public Task AnotarVersionDeContenidoAsync(string version, string evento, CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            db.VersionesDeContenido.Add(new FilaDeVersionDeContenido { Version = version, Evento = evento, En = Ahora });
            await db.SaveChangesAsync(ct);
            return 0;
        }, ct);

    public Task<IReadOnlyList<FilaDeVersionDeContenido>> UltimosEventosDeContenidoAsync(int cantidad, CancellationToken ct = default) =>
        ConCerrojo<IReadOnlyList<FilaDeVersionDeContenido>>(async db =>
            await db.VersionesDeContenido.AsNoTracking().OrderByDescending(v => v.Id).Take(cantidad).ToListAsync(ct), ct);

    public Task<string?> LeerConfiguracionAsync(string clave, CancellationToken ct = default) =>
        ConCerrojo(async db => (await db.Configuracion.AsNoTracking().FirstOrDefaultAsync(c => c.Clave == clave, ct))?.Valor, ct);

    public Task EscribirConfiguracionAsync(string clave, string valor, CancellationToken ct = default) =>
        ConCerrojo(async db =>
        {
            var fila = await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == clave, ct);
            if (fila is null) db.Configuracion.Add(new FilaDeConfiguracion { Clave = clave, Valor = valor });
            else fila.Valor = valor;
            await db.SaveChangesAsync(ct);
            return 0;
        }, ct);

    private async Task<T> ConCerrojo<T>(Func<ContextoDelCascaron, Task<T>> operacion, CancellationToken ct)
    {
        await _cerrojo.WaitAsync(ct);
        try
        {
            await using var db = new ContextoDelCascaron(_opciones);
            return await operacion(db);
        }
        finally
        {
            _cerrojo.Release();
        }
    }
}
