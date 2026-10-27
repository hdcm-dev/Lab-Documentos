using FrameworkConcept.Core.Auth;

namespace FrameworkConcept.Core.Red;

/// <summary>Borrador §7 — Cómo se comporta una acción del negocio según la red.</summary>
public enum ClaseDeAccion
{
    /// <summary>Se ejecuta siempre (con o sin red).</summary>
    SoloLocal,

    /// <summary>Se guarda en el outbox y sale cuando haya red y sesión válida (P4, P5).</summary>
    Encolable,

    /// <summary>Sin servidor se bloquea con motivo: botón deshabilitado + por qué.</summary>
    SoloEnLinea,

    /// <summary>Opcional del borrador: sólo con WiFi. Ninguna acción de la demo la usa; la lógica pura la contempla.</summary>
    SoloWiFi
}

/// <summary>
/// Una acción del negocio declarada con su clase y los métodos del puente que la ejecutan. El cascarón evalúa la
/// acción (DISENO §4): el contenido deshabilita el botón, y el despachador del puente rechaza el método si la
/// acción no está permitida (CodigosDeError.AccionBloqueada).
/// </summary>
/// <param name="Id">Estable; dominio en español («localidades.guardar»).</param>
/// <param name="Clase">Borrador §7.</param>
/// <param name="MetodosDelPuente">Qué filas de la tabla de métodos la ejecutan.</param>
/// <param name="Descripcion">Para quien lee la tabla y para el diagnóstico.</param>
public sealed record AccionDeclarada(string Id, ClaseDeAccion Clase, IReadOnlyList<string> MetodosDelPuente, string Descripcion);

/// <summary>
/// La política de la demo: una CONSTANTE del cascarón, como el catálogo (P12). El borrador §7 deja abierto que
/// venga en el manifiesto del contenido; la demo no lo hace (DISENO §4.2).
/// </summary>
public static class PoliticaDeAcciones
{
    /// <summary>La acción de ingresar con la cuenta: tiene reglas propias en el evaluador (DISENO §4.3).</summary>
    public const string Ingresar = "sesion.ingresar";

    public static IReadOnlyList<AccionDeclarada> Acciones { get; } =
    [
        new("localidades.guardar", ClaseDeAccion.SoloLocal, ["documents.put", "documents.delete"],
            "Alta, modificación y baja de localidades: documentos del teléfono."),
        new("encuesta.enviar", ClaseDeAccion.Encolable, ["encuestas.enqueueRespuesta"],
            "Registrar una encuesta: se guarda con su fila del outbox y sale cuando haya red y sesión (P5)."),
        new("outbox.enviarAhora", ClaseDeAccion.SoloEnLinea, ["outbox.sendNow"],
            "«Enviar ahora»: fuerza una pasada del sincronizador. Sin servidor no tiene sentido: se bloquea con motivo."),
        new(Ingresar, ClaseDeAccion.SoloEnLinea, ["auth.signIn"],
            "Ingresar con la cuenta (OIDC en el navegador del sistema): necesita llegar al emisor.")
    ];

    public static AccionDeclarada? Buscar(string id) =>
        Acciones.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.Ordinal));

    /// <summary>La acción que ejecuta un método del puente, o null si el método no es una acción del negocio.</summary>
    public static AccionDeclarada? DelMetodo(string metodo) =>
        Acciones.FirstOrDefault(a => a.MetodosDelPuente.Contains(metodo, StringComparer.Ordinal));
}

/// <summary>
/// Resultado de evaluar una acción ahora. Es la forma de «EvaluacionDeAccion» en el puente (actions.policy,
/// actions.check, evento actions.changed).
/// </summary>
/// <param name="Accion">Id de la acción.</param>
/// <param name="Clase">Su clase.</param>
/// <param name="Permitida">false = botón deshabilitado y el puente rechaza el método.</param>
/// <param name="QuedaPendiente">Encolable sin servidor o sin sesión válida: se acepta y queda en el outbox.</param>
/// <param name="Motivo">Por qué no está permitida, o por qué queda pendiente. Texto para la persona.</param>
public sealed record EvaluacionDeAccion(string Accion, ClaseDeAccion Clase, bool Permitida, bool QuedaPendiente, string? Motivo = null);

/// <summary>
/// Evalúa una acción con el estado de red y de sesión del momento (borrador §7 y P4). Función pura; las reglas
/// están en DISENO §4.3.
/// </summary>
public static class EvaluadorDeAcciones
{
    public static EvaluacionDeAccion Evaluar(AccionDeclarada accion, EstadoDeRed red, SesionInformada sesion)
    {
        var dispositivo = sesion.Modo == ModoDeSesion.Dispositivo;
        // Entrada abierta: se puede usar la app (local). Sesión válida: lo que se envía sale con identidad (P4).
        var entradaAbierta = dispositivo || sesion.Estado is EstadoDeSesion.Activa or EstadoDeSesion.Vencida or EstadoDeSesion.Revocada;
        var sesionValida = dispositivo || sesion.Estado == EstadoDeSesion.Activa;
        var conServidor = red is EstadoDeRed.EnLinea or EstadoDeRed.EnLineaMedida;

        EvaluacionDeAccion Permitida() => new(accion.Id, accion.Clase, true, false);
        EvaluacionDeAccion Pendiente(string motivo) => new(accion.Id, accion.Clase, true, true, motivo);
        EvaluacionDeAccion Bloqueada(string motivo) => new(accion.Id, accion.Clase, false, false, motivo);

        // Ingresar es la puerta de la entrada: sus reglas son al revés (sólo sin sesión válida).
        if (accion.Id == PoliticaDeAcciones.Ingresar)
        {
            if (dispositivo) return Bloqueada("Esta app no usa cuenta");
            if (sesion.Estado == EstadoDeSesion.Activa) return Bloqueada("Ya ingresaste");
            if (!entradaAbierta && sesion.Estado != EstadoDeSesion.SinCuenta) return Bloqueada(MotivoSinEntrada(sesion.Estado));
            return conServidor ? Permitida() : Bloqueada(MotivoSinServidor(red));
        }

        // Sin entrada abierta no se hace nada, sea cual sea la clase.
        if (!entradaAbierta) return Bloqueada(MotivoSinEntrada(sesion.Estado));

        switch (accion.Clase)
        {
            case ClaseDeAccion.SoloLocal:
                return Permitida();

            case ClaseDeAccion.Encolable:
                // P5 — Nunca se bloquea: se guarda con su fila del outbox y sale cuando haya servidor y sesión.
                if (red == EstadoDeRed.SinRed) return Pendiente("Sin conexión: se guarda y se envía cuando vuelva");
                if (red == EstadoDeRed.RedSinServidor) return Pendiente("El servidor no responde: se guarda y se envía cuando responda");
                if (!sesionValida) return Pendiente("Se guarda; se envía cuando vuelvas a ingresar");
                return Permitida();

            default:   // SoloEnLinea y SoloWiFi
                if (!conServidor) return Bloqueada(MotivoSinServidor(red));
                if (!sesionValida) return Bloqueada("Tu sesión venció: ingresá de nuevo");
                if (accion.Clase == ClaseDeAccion.SoloWiFi && red == EstadoDeRed.EnLineaMedida) return Bloqueada("Sólo con WiFi");
                return Permitida();
        }
    }

    private static string MotivoSinServidor(EstadoDeRed red) =>
        red == EstadoDeRed.SinRed ? "Sin conexión" : "El servidor no responde";

    private static string MotivoSinEntrada(EstadoDeSesion estado) => estado switch
    {
        EstadoDeSesion.Bloqueada => "Desbloqueá la app",
        EstadoDeSesion.Ingresando => "Completá el ingreso en el navegador",
        _ => "Ingresá con tu cuenta"
    };
}
