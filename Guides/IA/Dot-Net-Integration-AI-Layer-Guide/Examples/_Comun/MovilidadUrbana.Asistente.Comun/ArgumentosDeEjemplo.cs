// Nuevo (plan §E.1, «Contrato por ejemplo»; H2-018). Toda consola de Examples/ acepta los mismos
// argumentos, para que tools/verificar.sh las corra igual.
using System.Globalization;

namespace MovilidadUrbana.Asistente.Comun;

/// <param name="Fake">--fake: cliente guionado y embeddings estables; sin red.</param>
/// <param name="Verificar">--verificar: corre las entradas, escribe R-LLM y sale 0 sólo si se cumplen todos los invariantes de código.</param>
/// <param name="PlazoSegundos">--plazo-s N: al vencer, se termina la llamada en curso y las entradas que faltan quedan «excluida» con motivo «plazo».</param>
public sealed record ArgumentosDeEjemplo(bool Fake, bool Verificar, int? PlazoSegundos, IReadOnlyList<string> Resto)
{
    public static ArgumentosDeEjemplo Leer(string[] args)
    {
        var fake = false;
        var verificar = false;
        int? plazo = null;
        var resto = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--fake": fake = true; break;
                case "--verificar": verificar = true; break;
                case "--plazo-s" when i + 1 < args.Length &&
                                      int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var s):
                    plazo = s; i++; break;
                default: resto.Add(args[i]); break;
            }
        }
        return new ArgumentosDeEjemplo(fake, verificar, plazo, resto);
    }

    public Plazo CrearPlazo() => new(PlazoSegundos is int s ? DateTimeOffset.UtcNow.AddSeconds(s) : null);
}

/// <summary>Plazo de la suite (lo calcula el medidor de verificar.sh con el tope restante).</summary>
public sealed record Plazo(DateTimeOffset? VenceEn)
{
    public bool Vencido => VenceEn is { } v && DateTimeOffset.UtcNow >= v;
}
