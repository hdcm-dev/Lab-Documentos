// Nuevo (REC-17, pruebas primero): dos huecos del dominio heredado con un valor que propone el modelo.
// «٣٤٥٠» son dígitos arábigo-índicos (\d de .NET los acepta) y un nombre de 61 caracteres pasa si el
// largo máximo declarado no se aplica. Las dos propuestas llegan como salida estructurada del cliente
// guionado y tienen que rechazarse sin que el repositorio escriba nada. Sin red ni base.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Domain.Entities;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Comun.Pruebas.Cableado;

[TestFixture, Category("Cableado")]
public sealed class ReglasDeLocalidadTests
{
    private sealed record PropuestaDelModelo(string Nombre, string Provincia, string CodigoPostal, int? Habitantes);

    [TestCase("codigoPostal", """{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"٣٤٥٠","habitantes":90000}""",
        TestName = "Localidad_CodigoPostalConDigitosNoAscii_PropuestoPorElModelo_SeRechazaSinEscribir")]
    [TestCase("nombre", """{"nombre":"NOMBRE_DE_61","provincia":"Corrientes","codigoPostal":"3450","habitantes":90000}""",
        TestName = "Localidad_NombreDe61Caracteres_PropuestoPorElModelo_SeRechazaSinEscribir")]
    public async Task PropuestaDelModeloFueraDeLasReglas_SeRechazaSinEscribir(string campo, string json)
    {
        var modelo = new GuionChatClient(json.Replace("NOMBRE_DE_61", new string('a', 61)));
        var propuesta = (await modelo.GetResponseAsync<PropuestaDelModelo>("Alta: Goya, Corrientes, 3450, 90000 habitantes.")).Result;
        var repositorio = new RepositorioQueCuentaEscrituras();

        var resultado = await new LocalidadService(repositorio).SaveAsync(new LocalidadModel
        {
            Nombre = propuesta.Nombre,
            Provincia = propuesta.Provincia,
            CodigoPostal = propuesta.CodigoPostal,
            Habitantes = propuesta.Habitantes
        });

        Assert.Multiple(() =>
        {
            Assert.That(resultado.IsSuccess, Is.False);
            Assert.That(resultado.Errors.Keys, Is.EquivalentTo(new[] { campo }));
            Assert.That(repositorio.Escrituras, Is.Zero, "rechazado antes de escribir");
        });
    }

    [Test]
    public void Reglas_LosLimitesDeclarados_SeAplican()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LocalidadRules.CodigoPostalValido("3450"), Is.True);
            Assert.That(LocalidadRules.CodigoPostalValido("٣٤٥٠"), Is.False);
            Assert.That(LocalidadRules.NombreValido(new string('a', LocalidadRules.LargoMaximoDelNombre)), Is.True);
            Assert.That(LocalidadRules.NombreValido(new string('a', LocalidadRules.LargoMaximoDelNombre + 1)), Is.False);
            Assert.That(LocalidadRules.NombreValido("Go"), Is.False);
        });
    }

    private sealed class RepositorioQueCuentaEscrituras : ILocalidadRepository
    {
        public int Escrituras { get; private set; }

        public Task<IReadOnlyList<Localidad>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Localidad>>([]);

        public Task<Localidad?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Localidad?>(null);

        public Task<int> AddAsync(Localidad localidad, CancellationToken cancellationToken = default)
        {
            Escrituras++;
            return Task.FromResult(1);
        }

        public Task UpdateAsync(Localidad localidad, CancellationToken cancellationToken = default)
        {
            Escrituras++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            Escrituras++;
            return Task.CompletedTask;
        }
    }
}
