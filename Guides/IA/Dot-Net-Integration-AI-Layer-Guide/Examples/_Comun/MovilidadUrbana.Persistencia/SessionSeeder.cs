// Procedencia: Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/Infrastructure/Persistence/SessionSeeder.cs @ 8fb4c3a
// Delta: Agregado por la IA — siembra de encuestas rotulada «escrito para el ejemplo» (H-062); Modificado por otro motivo — namespace raíz MovilidadUrbana.Asistente (H2-008)
using Microsoft.EntityFrameworkCore;
using MovilidadUrbana.Asistente.Application.Abstractions;
using MovilidadUrbana.Asistente.Domain;
using MovilidadUrbana.Asistente.Domain.Entities;
using MovilidadUrbana.Asistente.Domain.Rules;

namespace MovilidadUrbana.Asistente.Infrastructure.Persistence;

/// <summary>
/// Deja la sesión con su juego de datos inicial la primera vez que se la toca. La marca en la
/// tabla `Sesiones` evita volver a sembrar cuando la persona borró todas las localidades a mano.
/// </summary>
public sealed class SessionSeeder(IDbContextFactory<AppDbContext> factory, ISessionContext session)
{
    private static readonly (string Nombre, string Provincia, string CodigoPostal, int Habitantes)[] LocalidadesIniciales =
    [
        ("Corrientes", "Corrientes", "3400", 346334),
        ("Resistencia", "Chaco", "3500", 291720)
    ];

    private bool _alreadySeeded;

    public async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
    {
        if (_alreadySeeded) return;

        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        if (!await context.Sessions.AnyAsync(s => s.Id == session.Id, cancellationToken))
        {
            context.Sessions.Add(new Session { Id = session.Id, CreatedAt = DateTimeOffset.UtcNow });
            context.Localidades.AddRange(LocalidadesIniciales.Select(inicial => new Localidad
            {
                SessionId = session.Id,
                Nombre = inicial.Nombre,
                Provincia = inicial.Provincia,
                CodigoPostal = inicial.CodigoPostal,
                Habitantes = inicial.Habitantes
            }));
            context.Encuestas.AddRange(SiembraDeEncuestas.Para(session.Id));

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Otra petición de la misma sesión ganó la carrera insertando la marca: los datos ya están.
            }
        }

        _alreadySeeded = true;
    }
}

/// <summary>
/// Nuevo. ESCRITO PARA EL EJEMPLO (estímulo de prueba, plan §E.1 y §F.2; H-062): 20 respuestas por
/// sesión, con nombres «Persona 01…20», valores tomados sólo de <see cref="Catalogos" />, edades y
/// rangos dentro de <see cref="EncuestaRules" /> y la localidad de las dos sembradas del caso base.
/// Es determinista (función del índice), así que toda cifra que produzca es reproducible; junto a
/// cada cifra se declara «estímulo del mismo autor».
/// </summary>
public static class SiembraDeEncuestas
{
    public const int Cantidad = 20;

    public static IEnumerable<RespuestaDeEncuesta> Para(string sessionId)
    {
        var localidades = new[] { "Corrientes", "Resistencia" };
        for (var i = 0; i < Cantidad; i++)
        {
            var medios = new List<string> { Catalogos.Medios[i % Catalogos.Medios.Count].Clave };
            if (i % 3 == 0 && Catalogos.Medios[(i + 2) % Catalogos.Medios.Count].Clave is var otro && !medios.Contains(otro))
            {
                medios.Add(otro);
            }

            yield return new RespuestaDeEncuesta
            {
                SessionId = sessionId,
                Nombre = $"Persona {i + 1:00}",
                Edad = EncuestaRules.EdadMinima + (i * 7) % 60,
                Localidad = localidades[i % localidades.Length],
                // En el orden del catálogo, como RegistrarAsync.
                Medios = [.. Catalogos.Medios.Where(m => medios.Contains(m.Clave)).Select(m => m.Clave)],
                Frecuencia = Catalogos.Frecuencias[i % Catalogos.Frecuencias.Count].Clave,
                Distancia = 1 + (i * 3) % 25,
                Minutos = 5 + (i * 11) % 90,
                Motivo = Catalogos.Motivos[i % Catalogos.Motivos.Count].Clave,
                RegistradaEn = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero).AddHours(i)
            };
        }
    }
}
