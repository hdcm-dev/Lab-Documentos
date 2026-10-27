using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;
using FrameworkConcept.Core.Contracts.Contenido;
using FrameworkConcept.Core.Contracts.Encuestas;

namespace FrameworkConcept.Tools;

/// <summary>
/// «firmar»: arma el índice del canal a partir de lo que hay en la carpeta y lo firma (P2).
///
///   1. Lee los paquetes (paquetes/*.zip, con su manifiesto adentro) y las definiciones (definiciones/*/vN.json).
///   2. Calcula size y sha256 de cada archivo: el teléfono los compara antes de usar nada.
///   3. Agrega lo que decidió quien publica: retiradas, obligatorias, retiros de definiciones.
///   4. Fija emitido = ahora y expires = ahora + 30 días (P2; se renueva cada 7: en la demo, el guion).
///   5. Firma los BYTES de indice.json con ECDSA P-256 + SHA-256 y escribe indice.json.sig al lado.
///
/// Firmar otra vez no cambia nada de lo publicado: sólo renueva la vigencia y recoge lo nuevo.
/// </summary>
internal static class Indexador
{
    public sealed record PaqueteLeido(string Ruta, ManifiestoContenido Manifiesto);

    public static int Firmar(Argumentos a)
    {
        var carpeta = CarpetaDelCanal.Desde(a);
        var keyId = a.Opcion("key-id", ClavesDeDemostracion.KeyIdPorDefecto);
        var ahora = LeerAhora(a);
        using var clave = ClavesDeDemostracion.CargarPrivada(a.Opcion("clave", Path.Combine("claves", ClavesDeDemostracion.Privada)));

        var estado = carpeta.LeerEstado();
        if (a.Opcion("obligatoria") is { } obligatoria)
        {
            // P2 — «mandatory» se activa con la regla general: en el próximo arranque en frío, ya verificada.
            estado.Obligatorias.Add(VersionSemantica.Parse(obligatoria));
        }
        if (a.Opcion("cascaron-minimo") is { } minimo)
            estado.CascaronMinimoSoportado = VersionSemantica.Parse(minimo);
        carpeta.GuardarEstado(estado);

        var indice = Armar(carpeta, keyId, ahora, estado);
        var bytes = Bytes(indice);
        var firma = new FirmaDelIndice(keyId, FirmaDelIndice.Es256, Convert.ToBase64String(clave.SignData(bytes, HashAlgorithmName.SHA256)));

        // Autoverificación: si la clave pública derivada no valida lo recién firmado, no se escribe nada.
        if (!clave.VerifyData(bytes, Convert.FromBase64String(firma.Firma), HashAlgorithmName.SHA256))
            throw new InvalidOperationException("la firma recién hecha no verifica");

        CarpetaDelCanal.EscribirAtomico(carpeta.Indice, bytes);
        CarpetaDelCanal.EscribirAtomico(carpeta.Firma, JsonDelContrato.Escribir(firma) + "\n");
        carpeta.Anotar("firmar", new { keyId, emitido = indice.Emitido, expires = indice.Expires, sha256 = CarpetaDelCanal.Sha256(bytes) });

        Console.WriteLine($"firmar: {carpeta.Indice} ({bytes.Length} B) firmado con {keyId}");
        Console.WriteLine($"  emitido {indice.Emitido:yyyy-MM-dd HH:mm}Z · expires {indice.Expires:yyyy-MM-dd HH:mm}Z");
        Console.WriteLine($"  contenido: [{string.Join(", ", indice.Entradas.Contenido.Select(e => e.Version + (e.Mandatory ? " (obligatoria)" : "")))}]");
        Console.WriteLine($"  definiciones: [{string.Join(", ", indice.Entradas.Definiciones.Select(d => $"{d.EncuestaId} v{d.Version}"))}]");
        Console.WriteLine($"  retiradas: [{string.Join(", ", indice.Retiradas)}] · definiciones retiradas: [{string.Join(", ", indice.DefinicionesRetiradas.Select(d => $"{d.EncuestaId} v{d.Version} el {d.RetiroEl:yyyy-MM-dd}"))}]");
        return 0;
    }

    public static IndiceDelCanal Armar(CarpetaDelCanal carpeta, string keyId, DateTimeOffset ahora, EstadoDePublicacion estado)
    {
        var contenido = LeerPaquetes(carpeta)
            // P2 — Una versión retirada sigue en el servidor (retención, atribución) pero el índice no la ofrece.
            .Where(p => !estado.Retiradas.Any(r => r.CompareTo(p.Manifiesto.Version) == 0))
            .Select(p => new EntradaDeContenido(
                p.Manifiesto.Version,
                carpeta.Relativa(p.Ruta),
                new FileInfo(p.Ruta).Length,
                CarpetaDelCanal.Sha256DeArchivo(p.Ruta),
                Mandatory: estado.Obligatorias.Any(o => o.CompareTo(p.Manifiesto.Version) == 0)))
            .OrderBy(e => e.Version)
            .ToList();

        var definiciones = LeerDefiniciones(carpeta)
            // P13.5 — Pasada la fecha de retiro, la definición deja de ofrecerse para encuestas nuevas.
            .Where(d => !estado.DefinicionesRetiradas.Any(r => r.EncuestaId == d.Definicion.EncuestaId
                                                              && r.Version == d.Definicion.Version
                                                              && r.RetiroEl <= ahora))
            .Select(d => new EntradaDeDefinicion(
                d.Definicion.EncuestaId,
                d.Definicion.Version,
                carpeta.Relativa(d.Ruta),
                new FileInfo(d.Ruta).Length,
                CarpetaDelCanal.Sha256DeArchivo(d.Ruta),
                d.Definicion.CompatibleConContenido))
            .OrderBy(d => d.EncuestaId, StringComparer.Ordinal).ThenBy(d => d.Version)
            .ToList();

        return new IndiceDelCanal(
            IndiceDelCanal.FormatoActual,
            carpeta.Canal,
            carpeta.PuenteMayor,
            keyId,
            ahora,
            ahora + ParametrosDeProducto.VigenciaDelIndice,
            new EntradasDelIndice(contenido, definiciones),
            [.. estado.Retiradas.OrderBy(v => v)],
            [.. estado.DefinicionesRetiradas],
            estado.CascaronMinimoSoportado);
    }

    /// <summary>Los bytes exactos que se firman. Lo que se firma es esto, no el objeto.</summary>
    public static byte[] Bytes(IndiceDelCanal indice) => Encoding.UTF8.GetBytes(JsonDelContrato.Escribir(indice) + "\n");

    public static IReadOnlyList<PaqueteLeido> LeerPaquetes(CarpetaDelCanal carpeta)
    {
        if (!Directory.Exists(carpeta.Paquetes)) return [];
        var paquetes = new List<PaqueteLeido>();
        foreach (var ruta in Directory.EnumerateFiles(carpeta.Paquetes, "*.zip").Order(StringComparer.Ordinal))
            paquetes.Add(new PaqueteLeido(ruta, LeerManifiesto(ruta)));
        return paquetes;
    }

    public static ManifiestoContenido LeerManifiesto(string zip)
    {
        using var archivo = ZipFile.OpenRead(zip);
        var entrada = archivo.GetEntry(ManifiestoContenido.NombreDeArchivo)
            ?? throw new PublicacionRechazada($"{zip} no tiene {ManifiestoContenido.NombreDeArchivo}");
        using var lector = new StreamReader(entrada.Open(), Encoding.UTF8);
        return JsonDelContrato.Leer<ManifiestoContenido>(lector.ReadToEnd());
    }

    public sealed record DefinicionLeida(string Ruta, DefinicionEncuesta Definicion);

    public static IReadOnlyList<DefinicionLeida> LeerDefiniciones(CarpetaDelCanal carpeta)
    {
        if (!Directory.Exists(carpeta.Definiciones)) return [];
        return [.. Directory.EnumerateFiles(carpeta.Definiciones, "v*.json", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(ruta => new DefinicionLeida(ruta, DefinicionEncuesta.Leer(File.ReadAllText(ruta))))];
    }

    /// <summary>--ahora (ISO 8601) fija el reloj: sirve para probar un índice vencido o un retiro ya cumplido.</summary>
    public static DateTimeOffset LeerAhora(Argumentos a) =>
        a.Opcion("ahora") is { } texto
            ? DateTimeOffset.Parse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime()
            : SinFraccion(DateTimeOffset.UtcNow);

    // Segundos enteros: el índice se lee mejor y nada depende de los milisegundos.
    private static DateTimeOffset SinFraccion(DateTimeOffset t) =>
        new(t.Ticks - (t.Ticks % TimeSpan.TicksPerSecond), TimeSpan.Zero);
}
