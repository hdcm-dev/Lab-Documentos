using System.Text.Json;

namespace TusDemo.Web.Archivos;

/// <summary>
/// Tabla de archivos de la demo, guardada en un JSON. En un producto real es una tabla de la base
/// con un índice único sobre Documento; acá el mismo efecto lo da la búsqueda por Documento
/// dentro de un candado.
/// </summary>
public sealed class RegistroDeArchivos
{
    private readonly string _ruta;
    private readonly Lock _candado = new();
    private readonly List<ArchivoRegistrado> _filas;

    public RegistroDeArchivos(Microsoft.Extensions.Options.IOptions<OpcionesArchivos> opciones)
    {
        _ruta = opciones.Value.Registro;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_ruta))!);
        _filas = File.Exists(_ruta)
            ? JsonSerializer.Deserialize<List<ArchivoRegistrado>>(File.ReadAllText(_ruta)) ?? []
            : [];
    }

    public IReadOnlyList<ArchivoRegistrado> Todos()
    {
        lock (_candado) return _filas.OrderByDescending(f => f.Id).ToList();
    }

    public ArchivoRegistrado? Buscar(long id)
    {
        lock (_candado) return _filas.FirstOrDefault(f => f.Id == id);
    }

    /// <summary>
    /// Inserta la fila si no existe una con el mismo Documento; si existe, la devuelve.
    /// Así un reintento del cliente no duplica (inserción idempotente).
    /// </summary>
    public (ArchivoRegistrado Fila, bool Creada) RegistrarSiFalta(
        string documento, string nombre, string tipo, long bytes, string descripcion, string usuario)
    {
        lock (_candado)
        {
            var existente = _filas.FirstOrDefault(f => f.Documento == documento);
            if (existente is not null) return (existente, false);

            var fila = new ArchivoRegistrado(
                Id: _filas.Count == 0 ? 1 : _filas.Max(f => f.Id) + 1,
                Documento: documento,
                Nombre: nombre,
                Tipo: tipo,
                Bytes: bytes,
                Descripcion: descripcion,
                FechaAlta: DateTimeOffset.UtcNow,
                UsuarioAlta: usuario);
            // Escritura atómica: primero la lista nueva a un temporal y después se reemplaza el
            // archivo con un Move. Si algo falla antes, ni el disco ni la memoria cambian.
            var nuevas = new List<ArchivoRegistrado>(_filas) { fila };
            var json = JsonSerializer.Serialize(nuevas);
            var temporal = _ruta + ".tmp";
            File.WriteAllText(temporal, json);
            File.Move(temporal, _ruta, overwrite: true);
            _filas.Add(fila);
            return (fila, true);
        }
    }
}
