using System.Globalization;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;

namespace FrameworkConcept.Tools;

/// <summary>
/// «retirar»: anota un retiro en el estado de publicación. Surte efecto cuando se firma de nuevo el índice.
///
///   retirar --contenido 1.1.0
///     P2 — La versión pasa a la lista firmada «retiradas». El teléfono que la tenga instalada vuelve a previous en
///     el próximo arranque en frío (nunca a mitad de una captura). Es la forma de corregir hacia atrás sin
///     rebajar la versión: no existe el «rollback remoto».
///     P13.10 — Retirar una versión de CONTENIDO no rechaza las respuestas tomadas con ella.
///
///   retirar --definicion encuesta-movilidad@1 --retiro-el 2026-12-01
///     P13.5 — Un retiro de definición se anuncia en el índice firmado con su fecha, con 30 días de aviso como
///     mínimo. Hasta la fecha el receptor la acepta; desde la fecha, lo que llegue con ella va a carta muerta.
/// </summary>
internal static class Retiros
{
    public static int Retirar(Argumentos a)
    {
        var carpeta = CarpetaDelCanal.Desde(a);
        var estado = carpeta.LeerEstado();

        if (a.Opcion("contenido") is { } texto)
        {
            var version = VersionSemantica.Parse(texto);
            var publicada = Indexador.LeerPaquetes(carpeta).Any(p => p.Manifiesto.Version.CompareTo(version) == 0);
            if (!publicada)
                throw new PublicacionRechazada($"no hay un paquete {version} en {carpeta.Canal}/puente-{carpeta.PuenteMayor}.");
            if (!estado.Retiradas.Any(r => r.CompareTo(version) == 0)) estado.Retiradas.Add(version);

            carpeta.GuardarEstado(estado);
            carpeta.Anotar("retirar-contenido", new { version = version.ToString() });
            Console.WriteLine($"retirar: contenido {version} anotado como retirado. Sigue: «firmar».");
            return 0;
        }

        if (a.Opcion("definicion") is { } referencia)
        {
            var partes = referencia.Split('@');
            if (partes.Length != 2 || !int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
                throw new ErrorDeUso("--definicion se escribe encuestaId@version, por ejemplo encuesta-movilidad@1");
            if (!File.Exists(carpeta.RutaDeLaDefinicion(partes[0], numero)))
                throw new PublicacionRechazada($"{partes[0]} v{numero} no está publicada en {carpeta.Canal}/puente-{carpeta.PuenteMayor}.");

            var anunciadaEl = Indexador.LeerAhora(a);
            var retiroEl = DateTimeOffset.Parse(a.Requerida("retiro-el"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            var retiro = new DefinicionRetirada(partes[0], numero, anunciadaEl, retiroEl);
            if (!retiro.RespetaElPlazoDeAviso)
                throw new PublicacionRechazada($"P13.5: el retiro tiene que anunciarse con {ParametrosDeProducto.PlazoDeAvisoDeRetiro.TotalDays} días de aviso como mínimo (desde {anunciadaEl:yyyy-MM-dd}, el {anunciadaEl + ParametrosDeProducto.PlazoDeAvisoDeRetiro:yyyy-MM-dd} o después).");

            estado.DefinicionesRetiradas.RemoveAll(r => r.EncuestaId == retiro.EncuestaId && r.Version == retiro.Version);
            estado.DefinicionesRetiradas.Add(retiro);
            carpeta.GuardarEstado(estado);
            carpeta.Anotar("retirar-definicion", retiro);
            Console.WriteLine($"retirar: {retiro.EncuestaId} v{retiro.Version} se retira el {retiro.RetiroEl:yyyy-MM-dd} (anunciado el {retiro.AnunciadaEl:yyyy-MM-dd}). Sigue: «firmar».");
            return 0;
        }

        throw new ErrorDeUso("retirar necesita --contenido X.Y.Z o --definicion encuestaId@N --retiro-el AAAA-MM-DD");
    }
}
