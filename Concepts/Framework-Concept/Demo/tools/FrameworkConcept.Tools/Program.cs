using FrameworkConcept.Core.Contracts.Puente;
using FrameworkConcept.Tools;

// Herramientas de quien PUBLICA (no corren en el teléfono). Se usan desde la raíz de la demo:
//   dotnet run --project tools/FrameworkConcept.Tools -- <comando> [--opcion valor ...]
// o, como hace scripts/guion.sh, con el .dll ya compilado. Ver Ayuda() al final.
try
{
    var comando = args.FirstOrDefault();
    var resto = args.Skip(1);
    return comando switch
    {
        "claves" => ClavesDeDemostracion.Generar(Argumentos.Leer(resto, "forzar")),
        "empaquetar" => Empaquetador.Empaquetar(Argumentos.Leer(resto)),
        "publicar-definicion" => PublicadorDeDefiniciones.Publicar(Argumentos.Leer(resto)),
        "firmar" => Indexador.Firmar(Argumentos.Leer(resto)),
        "retirar" => Retiros.Retirar(Argumentos.Leer(resto)),
        "firma-invalida" => Verificador.FirmaInvalida(Argumentos.Leer(resto)),
        "verificar" => Verificador.Verificar(Argumentos.Leer(resto)),
        "contrato" => EscribirContrato(resto.FirstOrDefault() ?? "src/FrameworkConcept.Core/Contracts/contrato.json"),
        "tpr" => Tpr(Argumentos.Leer(resto)),
        _ => Ayuda()
    };
}
catch (ErrorDeUso e)
{
    Console.Error.WriteLine($"uso: {e.Message}");
    return 2;
}
catch (PublicacionRechazada e)
{
    Console.Error.WriteLine($"RECHAZADO: {e.Message}");
    return 1;
}

// P3 — Regenera contrato.json a partir del código (tabla de métodos + catálogo + transporte).
// Se corre DESPUÉS de un cambio intencional del contrato y de subir ContratoDelPuente.Version;
// la prueba de instantánea de la compuerta falla si alguno de los dos falta.
static int EscribirContrato(string destino)
{
    File.WriteAllText(destino, ContratoDelPuente.SerializarInstantanea());
    Console.WriteLine($"contrato {ContratoDelPuente.Version} escrito en {destino}");
    return 0;
}

// P1 — TPR de un logcat de arranques en frío (LectorDeTpr.cs; lo usa «scripts/guion.sh tpr»).
static int Tpr(Argumentos a) =>
    LectorDeTpr.Comando(a.Requerida("logcat"), long.Parse(a.Opcion("umbral", "5000"), System.Globalization.CultureInfo.InvariantCulture));

static int Ayuda()
{
    Console.WriteLine("""
        FrameworkConcept.Tools — herramientas de publicación del canal (se corren desde la raíz de la demo).

        Opciones del canal, comunes a casi todos los comandos:
          --raiz salida/canal   --canal dev|beta|prod (dev)   --puente <mayor del puente> (el de este código)
          La carpeta del canal es {raiz}/{canal}/puente-{mayor}/ (P2).

          claves [--dir claves] [--forzar]
              Genera el par ECDSA P-256 de demostración: claves/privada-demo.pem (no se versiona) y
              claves/publica-demo.pem (se versiona; la embebe el cascarón). Sin --forzar no pisa una
              pública existente: un par nuevo obliga a rehacer la semilla (publicar, definicion, firmar, semilla).
          empaquetar --publicado <salida de dotnet publish> --version X.Y.Z [--commit abc1234] [--csproj ...]
                     [--declaracion src/FrameworkConcept.Content/declaracion-del-contenido.json]
                     [--puente-piso 1.0.0] [--requires id@X.Y.Z]... [--optional id@X.Y.Z]...
              Zip plano (sin .br/.gz) con manifiesto.json: versión, compatibleConPuente, capabilities, motor
              mínimo y sha256 de cada archivo. Rango y capabilities salen de la declaración del contenido
              (las opciones la reemplazan). Rechaza AOT, pre-release y versiones no mayores.
          publicar-definicion --definicion <vN.json> --cambia-datos si|no
              Copia la definición al canal si cumple P13 (versión mayor, evolución compatible).
          firmar [--clave claves/privada-demo.pem] [--key-id demo-2026] [--obligatoria X.Y.Z]
                 [--cascaron-minimo X.Y.Z] [--ahora AAAA-MM-DDTHH:MM:SSZ]
              Arma indice.json desde la carpeta (expires = ahora + 30 días) y escribe indice.json.sig.
          retirar --contenido X.Y.Z
          retirar --definicion encuestaId@N --retiro-el AAAA-MM-DD [--ahora ...]
              Anota el retiro (con 30 días de aviso para definiciones). Surte efecto al «firmar».
          firma-invalida
              Altera indice.json sin volver a firmarlo, para la demo de rechazo.
          verificar [--publica claves/publica-demo.pem] [--ahora ...]
              Hace las comprobaciones del teléfono sobre lo publicado: firma, expires, canal, size, sha256.
          contrato [destino]
              Regenera la instantánea del contrato del puente (contrato.json).
          tpr --logcat <archivo> [--umbral 5000]
              P1 — TPR por arranque en frío (inicio del proceso → marca primer-render) leído de un logcat
              (-v time o -v threadtime). Una corrida sin marca dice si el WebView llegó a pedir el contenido.
              Sale 0 si todas tienen marca y el máximo no pasa el umbral.
        """);
    return 2;
}
