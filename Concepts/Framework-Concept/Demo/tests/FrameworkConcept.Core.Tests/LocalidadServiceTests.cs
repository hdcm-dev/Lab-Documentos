using FrameworkConcept.Core.Application.Localidades;

namespace FrameworkConcept.Core.Tests;

/// <summary>
/// Adaptado de LocalidadEditorViewModelTests y LocalidadesViewModelTests del caso base: los casos que
/// decide el servicio (validar, rechazar duplicados, alta, modificación, baja) se prueban contra él.
/// Lo que era sólo de la vista MAUI (filtro de la lista, navegación, confirmación) queda para las páginas
/// Razor del contenido.
/// </summary>
[TestFixture]
public class LocalidadServiceTests
{
    private TestEnvironment _entorno = default!;

    [SetUp] public void Create() => _entorno = new TestEnvironment();

    [Test]
    [Description("Arranca con las dos localidades sembradas")]
    public async Task ArrancaConLasSembradas()
    {
        var todas = await _entorno.Localidades.GetAllAsync();

        Assert.That(todas.Select(l => l.Nombre), Is.EqualTo(new[] { "Corrientes", "Resistencia" }));
    }

    [Test]
    [Description("Con campos inválidos devuelve un error por campo y no guarda")]
    public async Task GuardarInvalidoDevuelveErrores()
    {
        var resultado = await _entorno.Localidades.SaveAsync(new LocalidadModel { Nombre = "Go", CodigoPostal = "12" });

        Assert.That(resultado.IsSuccess, Is.False);
        Assert.That(resultado.Message, Is.EqualTo("Revise los campos marcados en rojo."));
        Assert.That(resultado.Errors.Keys, Is.EquivalentTo(new[] { "nombre", "provincia", "codigoPostal", "habitantes" }));
        Assert.That(await _entorno.Localidades.GetAllAsync(), Has.Count.EqualTo(2));
    }

    [Test]
    [Description("Corregir un campo saca su error, sin tocar los de los demás")]
    public async Task CorregirUnCampoSacaSuError()
    {
        var resultado = await _entorno.Localidades.SaveAsync(new LocalidadModel { Provincia = "Corrientes" });

        Assert.That(resultado.Errors.ContainsKey("provincia"), Is.False);
        Assert.That(resultado.Errors.ContainsKey("nombre"), Is.True);
    }

    [Test]
    [Description("Un alta válida avisa y queda guardada")]
    public async Task GuardarNuevaAvisa()
    {
        var resultado = await _entorno.Localidades.SaveAsync(
            new LocalidadModel { Nombre = "Goya", Provincia = "Corrientes", CodigoPostal = "3450", Habitantes = 90000 });

        Assert.That(resultado.IsSuccess, Is.True);
        Assert.That(resultado.Message, Is.EqualTo("Se agregó la localidad Goya."));
        Assert.That((await _entorno.Localidades.GetAllAsync()).Single(l => l.Nombre == "Goya").Habitantes, Is.EqualTo(90000));
    }

    [Test]
    [Description("No admite la misma localidad dos veces en la provincia, sin distinguir mayúsculas")]
    public async Task RechazaDuplicada()
    {
        var resultado = await _entorno.Localidades.SaveAsync(
            new LocalidadModel { Nombre = " corrientes ", Provincia = "Corrientes", CodigoPostal = "3400", Habitantes = 1 });

        Assert.That(resultado.Errors["nombre"], Is.EqualTo("Ya existe una localidad con ese nombre en la provincia."));
    }

    [Test]
    [Description("Modificar conserva el identificador y actualiza los datos")]
    public async Task ModificarActualiza()
    {
        var corrientes = (await _entorno.Localidades.GetAllAsync())[0];

        var resultado = await _entorno.Localidades.SaveAsync(new LocalidadModel
        {
            Id = corrientes.Id, Nombre = "Corrientes Capital", Provincia = corrientes.Provincia,
            CodigoPostal = corrientes.CodigoPostal, Habitantes = corrientes.Habitantes
        });

        Assert.That(resultado.Message, Is.EqualTo("Se actualizó la localidad Corrientes Capital."));
        var todas = await _entorno.Localidades.GetAllAsync();
        Assert.That(todas.Single(l => l.Id == corrientes.Id).Nombre, Is.EqualTo("Corrientes Capital"));
        Assert.That(todas, Has.Count.EqualTo(2));
    }

    [Test]
    [Description("La baja ocurre y avisa; una segunda baja dice que ya no existe")]
    public async Task EliminarBorra()
    {
        var corrientes = (await _entorno.Localidades.GetAllAsync())[0];

        var primera = await _entorno.Localidades.DeleteAsync(corrientes.Id);
        var segunda = await _entorno.Localidades.DeleteAsync(corrientes.Id);

        Assert.That(primera.Message, Does.StartWith("Se eliminó la localidad"));
        Assert.That(segunda.Errors["nombre"], Is.EqualTo("La localidad ya no existe."));
        Assert.That(await _entorno.Localidades.GetAllAsync(), Has.Count.EqualTo(1));
    }
}
