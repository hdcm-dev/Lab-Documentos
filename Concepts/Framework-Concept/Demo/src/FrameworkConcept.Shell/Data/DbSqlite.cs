using FrameworkConcept.Core.Contracts.Encuestas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FrameworkConcept.Shell.Data;

/// <summary>
/// <see cref="IDb"/> sobre EF Core SQLite (P5). Un DbContext nuevo por operación (EF Core no es seguro entre hilos) y
/// un semáforo para que el puente, el sincronizador y el actualizador no escriban a la vez sobre el mismo archivo.
///
/// Sin código dinámico (iOS): el modelo y las consultas salen compilados de antemano en Data/Compilado/
/// (scripts/modelo-compilado.sh, con «dotnet ef dbcontext optimize --precompile-queries --nativeaot»). El generador de
/// EF 10.0.11 (experimental) sólo precompila una consulta si todo lo que nombra son variables LOCALES: un parámetro del
/// método o de una lambda, sea el contexto, un valor o el CancellationToken, la hace fallar. Por eso cada método abre
/// el contexto en una local y copia antes los valores que usa la consulta («var (enColeccion, corte) = (coleccion, ct)»).
/// Los interceptores apuntan a línea y columna de cada consulta: cualquier cambio en este archivo, aunque sea un
/// comentario, exige regenerar, y si no se regenera el cascarón no compila (README.md, «La base del cascarón sin
/// código dinámico»).
/// Crear la base tampoco puede pasar por EnsureCreated: arma el SQL con el modelo de diseño, que sin código dinámico EF
/// no carga («Design-time DbContext operations are not supported when publishing with NativeAOT», visto en el Release
/// del simulador el 2026-11-02). El mismo script sale de «dotnet ef dbcontext script» a Data/Compilado/ y se ejecuta tal cual.
/// </summary>
public sealed class DbSqlite : IDb
{
    private readonly DbContextOptions<ContextoDelCascaron> _opciones;
    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private readonly string _ruta;
    private readonly string? _sqlDeCreacion;

    /// <param name="modeloCompilado">El modelo compilado (ContextoDelCascaronModel.Instance). La app lo pasa siempre;
    /// el proyecto de diseño (tools/FrameworkConcept.Shell.Diseno) no, porque es el que lo genera.</param>
    /// <param name="sqlDeCreacion">El script que crea la base (SqlDeCreacion.Script), con la misma regla: sin él se usa
    /// EnsureCreated, que sólo anda con código dinámico.</param>
    public DbSqlite(string rutaDelArchivo, IModel? modeloCompilado = null, string? sqlDeCreacion = null)
    {
        _ruta = rutaDelArchivo;
        _sqlDeCreacion = sqlDeCreacion;
        var opciones = new DbContextOptionsBuilder<ContextoDelCascaron>().UseSqlite($"Data Source={rutaDelArchivo}");
        if (modeloCompilado is not null)
            opciones.UseModel(modeloCompilado);
        _opciones = opciones.Options;
    }

    private static long Ahora => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public async Task AbrirAsync(CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();

        // P5 / C-1 — sobre fijo: se crea entero la primera vez; después sólo se comprueba el número.
        await db.Database.OpenConnectionAsync(ct);
        var version = await LeerVersionDeEsquemaAsync(db, ct);

        if (version == 0)
        {
            // Script y número en UNA transacción (en SQLite el DDL es transaccional): si la app se corta a mitad, no
            // quedan tablas con user_version en 0, que harían fallar el script en cada arranque («already exists»).
            await using var transaccion = await db.Database.BeginTransactionAsync(ct);
            if (_sqlDeCreacion is null)
            {
                await db.Database.EnsureCreatedAsync(ct);
            }
            else
            {
                await using var crear = db.Database.GetDbConnection().CreateCommand();
                crear.Transaction = transaccion.GetDbTransaction();
                crear.CommandText = _sqlDeCreacion;
                await crear.ExecuteNonQueryAsync(ct);
            }
            await db.Database.ExecuteSqlRawAsync($"PRAGMA user_version = {EsquemaDelCascaron.Version}", ct);
            await transaccion.CommitAsync(ct);
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
    }

    private static async Task<int> LeerVersionDeEsquemaAsync(ContextoDelCascaron db, CancellationToken ct)
    {
        await using var comando = db.Database.GetDbConnection().CreateCommand();
        comando.CommandText = "PRAGMA user_version";
        return Convert.ToInt32(await comando.ExecuteScalarAsync(ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    // --- Documentos ---

    public async Task<FilaDeDocumento?> ObtenerDocumentoAsync(string coleccion, string id, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (enColeccion, conId, corte) = (coleccion, id, ct);
        return await db.Documentos.AsNoTracking().FirstOrDefaultAsync(d => d.Coleccion == enColeccion && d.Id == conId, corte);
    }

    public async Task<IReadOnlyList<FilaDeDocumento>> ListarDocumentosAsync(string coleccion, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (enColeccion, corte) = (coleccion, ct);
        return await db.Documentos.AsNoTracking().Where(d => d.Coleccion == enColeccion).OrderBy(d => d.Id).ToListAsync(corte);
    }

    public async Task<int> GuardarDocumentoAsync(string coleccion, string id, int versionDeForma, string json, int? versionEsperada, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (enColeccion, conId, corte) = (coleccion, id, ct);
        var fila = await db.Documentos.FirstOrDefaultAsync(d => d.Coleccion == enColeccion && d.Id == conId, corte);
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
    }

    public async Task<bool> BorrarDocumentoAsync(string coleccion, string id, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (enColeccion, conId, corte) = (coleccion, id, ct);
        return await db.Documentos.Where(d => d.Coleccion == enColeccion && d.Id == conId).ExecuteDeleteAsync(corte) > 0;
    }

    public async Task<bool> GuardarYEncolarAsync(FilaDeDocumento documento, FilaDeOutbox fila, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (clave, enColeccion, conId, corte) = (fila.ClaveIdempotencia, documento.Coleccion, documento.Id, ct);
        if (await db.Outbox.AnyAsync(f => f.ClaveIdempotencia == clave, corte))
            return false;

        var existente = await db.Documentos.FirstOrDefaultAsync(d => d.Coleccion == enColeccion && d.Id == conId, corte);
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
    }

    // --- Outbox ---

    public async Task<IReadOnlyList<FilaDeOutbox>> ListarOutboxSinEnviarAsync(CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var corte = ct;
        return await db.Outbox.AsNoTracking().Where(f => f.Estado != EstadoDeFila.Enviada).OrderBy(f => f.Id).ToListAsync(corte);
    }

    public async Task ActualizarFilaAsync(FilaDeOutbox fila, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        db.Outbox.Update(fila);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(int Pendientes, int Rechazadas)> ContarOutboxAsync(CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var corte = ct;
        var pendientes = await db.Outbox.CountAsync(f => f.Estado == EstadoDeFila.Pendiente, corte);
        var rechazadas = await db.Outbox.CountAsync(f => f.Estado == EstadoDeFila.Rechazada || f.Estado == EstadoDeFila.Error, corte);
        return (pendientes, rechazadas);
    }

    public async Task<(int Pendientes, int Rechazadas)> ContarOutboxDeAsync(string? dueno, CancellationToken ct = default)
    {
        if (dueno is null)
            return await ContarOutboxAsync(ct);

        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (delDueno, corte) = (dueno, ct);
        var pendientes = await db.Outbox.CountAsync(f => f.Dueno == delDueno && f.Estado == EstadoDeFila.Pendiente, corte);
        var rechazadas = await db.Outbox.CountAsync(f => f.Dueno == delDueno && (f.Estado == EstadoDeFila.Rechazada || f.Estado == EstadoDeFila.Error), corte);
        return (pendientes, rechazadas);
    }

    // --- Definiciones ---

    public async Task GuardarDefinicionAsync(DefinicionEncuesta definicion, string json, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (deEncuesta, enVersion, corte) = (definicion.EncuestaId, definicion.Version, ct);

        // Una versión publicada no cambia: si ya está guardada, se deja como estaba (P13.9).
        if (!await db.Definiciones.AnyAsync(d => d.EncuestaId == deEncuesta && d.Version == enVersion, corte))
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
    }

    public async Task<FilaDeDefinicion?> ObtenerDefinicionAsync(string encuestaId, int version, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (deEncuesta, enVersion, corte) = (encuestaId, version, ct);
        return await db.Definiciones.AsNoTracking().FirstOrDefaultAsync(d => d.EncuestaId == deEncuesta && d.Version == enVersion, corte);
    }

    public async Task<IReadOnlyList<FilaDeDefinicion>> ListarDefinicionesAsync(string? encuestaId = null, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (deEncuesta, corte) = (encuestaId, ct);
        return await db.Definiciones.AsNoTracking()
            .Where(d => deEncuesta == null || d.EncuestaId == deEncuesta)
            .OrderBy(d => d.EncuestaId).ThenBy(d => d.Version)
            .ToListAsync(corte);
    }

    // --- Versión de contenido y configuración ---

    public async Task AnotarVersionDeContenidoAsync(string version, string evento, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        db.VersionesDeContenido.Add(new FilaDeVersionDeContenido { Version = version, Evento = evento, En = Ahora });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FilaDeVersionDeContenido>> UltimosEventosDeContenidoAsync(int cantidad, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (tope, corte) = (cantidad, ct);
        return await db.VersionesDeContenido.AsNoTracking().OrderByDescending(v => v.Id).Take(tope).ToListAsync(corte);
    }

    public async Task<string?> LeerConfiguracionAsync(string clave, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (conClave, corte) = (clave, ct);
        return (await db.Configuracion.AsNoTracking().FirstOrDefaultAsync(c => c.Clave == conClave, corte))?.Valor;
    }

    public async Task EscribirConfiguracionAsync(string clave, string valor, CancellationToken ct = default)
    {
        using var turno = await TomarTurnoAsync(ct);
        await using var db = NuevoContexto();
        var (conClave, corte) = (clave, ct);
        var fila = await db.Configuracion.FirstOrDefaultAsync(c => c.Clave == conClave, corte);
        if (fila is null) db.Configuracion.Add(new FilaDeConfiguracion { Clave = clave, Valor = valor });
        else fila.Valor = valor;
        await db.SaveChangesAsync(ct);
    }

    private ContextoDelCascaron NuevoContexto() => new(_opciones);

    /// <summary>Espera el semáforo y lo devuelve al salir del <c>using</c>: una sola operación sobre el archivo por vez.</summary>
    private async Task<Turno> TomarTurnoAsync(CancellationToken ct)
    {
        await _cerrojo.WaitAsync(ct);
        return new Turno(_cerrojo);
    }

    private readonly struct Turno(SemaphoreSlim cerrojo) : IDisposable
    {
        public void Dispose() => cerrojo.Release();
    }
}
