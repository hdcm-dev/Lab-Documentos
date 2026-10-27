using System.Security.Cryptography;
using System.Text.Json.Nodes;
using FrameworkConcept.Core.Contracts;
using FrameworkConcept.Core.Contracts.Canal;

namespace FrameworkConcept.Tools;

/// <summary>
/// «verificar» hace, del lado de quien publica, las mismas comprobaciones que el teléfono hace antes de usar
/// algo del canal, en el mismo orden (P2): firma → clave → expires → canal y mayor → size y sha256 de cada entrada.
/// Sirve para ver que lo publicado es aceptable sin instalar nada, y para ver el rechazo de «firma-invalida».
///
/// «firma-invalida» altera indice.json sin firmarlo de nuevo (le alarga la vigencia un año, como haría quien
/// quisiera congelar a los teléfonos en un contenido viejo). El teléfono tiene que rechazarlo y seguir con lo
/// instalado. Para volver al estado sano: «firmar».
/// </summary>
internal static class Verificador
{
    public static int Verificar(Argumentos a)
    {
        var carpeta = CarpetaDelCanal.Desde(a);
        var ahora = Indexador.LeerAhora(a);
        var keyIdEsperado = a.Opcion("key-id", ClavesDeDemostracion.KeyIdPorDefecto);
        using var publica = ClavesDeDemostracion.CargarPublica(a.Opcion("publica", Path.Combine("claves", ClavesDeDemostracion.Publica)));

        var bytes = File.ReadAllBytes(carpeta.Indice);
        var firma = JsonDelContrato.Leer<FirmaDelIndice>(File.ReadAllText(carpeta.Firma));

        if (firma.KeyId != keyIdEsperado) return Rechazo("ClaveDesconocida", $"la firma es de «{firma.KeyId}», se esperaba «{keyIdEsperado}»");
        if (firma.Algoritmo != FirmaDelIndice.Es256) return Rechazo("FirmaInvalida", $"algoritmo «{firma.Algoritmo}»");
        if (!publica.VerifyData(bytes, Convert.FromBase64String(firma.Firma), HashAlgorithmName.SHA256))
            return Rechazo("FirmaInvalida", "la firma no corresponde a los bytes de indice.json");

        // Sólo después de verificar la firma se interpreta el contenido del índice.
        var indice = JsonDelContrato.Leer<IndiceDelCanal>(System.Text.Encoding.UTF8.GetString(bytes));
        if (indice.KeyId != firma.KeyId) return Rechazo("ClaveDesconocida", "el keyId del índice y el de la firma difieren");
        if (indice.Formato != IndiceDelCanal.FormatoActual) return Rechazo("FormatoDesconocido", $"formato {indice.Formato}");
        if (indice.Expires <= ahora) return Rechazo("IndiceVencido", $"venció el {indice.Expires:yyyy-MM-dd HH:mm}Z");
        if (indice.Canal != carpeta.Canal) return Rechazo("OtroCanal", $"el índice es de «{indice.Canal}»");
        if (indice.PuenteMayor != carpeta.PuenteMayor) return Rechazo("OtroMayorDelPuente", $"el índice es del puente {indice.PuenteMayor}");

        var rutas = indice.Entradas.Contenido.Select(e => (e.Ruta, e.Size, e.Sha256))
            .Concat(indice.Entradas.Definiciones.Select(d => (d.Ruta, d.Size, d.Sha256)));
        foreach (var (ruta, size, sha) in rutas)
        {
            var archivo = Path.Combine(carpeta.Ruta, ruta);
            if (!File.Exists(archivo)) return Rechazo("PaqueteInvalido", $"{ruta} no existe");
            if (new FileInfo(archivo).Length != size) return Rechazo("TamanoDistinto", ruta);
            if (CarpetaDelCanal.Sha256DeArchivo(archivo) != sha) return Rechazo("HashDistinto", ruta);
        }

        Console.WriteLine($"verificar: OK — {carpeta.Canal}/puente-{carpeta.PuenteMayor}, firmado por {firma.KeyId}, vence el {indice.Expires:yyyy-MM-dd}, " +
                          $"{indice.Entradas.Contenido.Count} contenido(s) y {indice.Entradas.Definiciones.Count} definición(es) con size y sha256 correctos.");
        return 0;
    }

    private static int Rechazo(string motivo, string detalle)
    {
        // Los nombres coinciden con MotivoDeRechazo del actualizador (Core/Update).
        Console.WriteLine($"verificar: RECHAZADO — {motivo}: {detalle}");
        return 1;
    }

    public static int FirmaInvalida(Argumentos a)
    {
        var carpeta = CarpetaDelCanal.Desde(a);
        var json = JsonNode.Parse(File.ReadAllText(carpeta.Indice))!.AsObject();
        var expires = DateTimeOffset.Parse(json["expires"]!.GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture);
        json["expires"] = expires.AddYears(1);
        CarpetaDelCanal.EscribirAtomico(carpeta.Indice, json.ToJsonString(JsonDelContrato.Opciones) + "\n");
        carpeta.Anotar("firma-invalida", new { nota = "indice.json alterado sin volver a firmar (demo de rechazo)" });

        Console.WriteLine($"firma-invalida: {carpeta.Indice} alterado (expires + 1 año) y la firma de al lado quedó como estaba.");
        Console.WriteLine("  El teléfono tiene que rechazarlo (FirmaInvalida) y seguir con lo instalado. Para volver: «firmar».");
        return 0;
    }
}
