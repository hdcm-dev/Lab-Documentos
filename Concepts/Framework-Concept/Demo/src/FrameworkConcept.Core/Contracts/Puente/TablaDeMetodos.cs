namespace FrameworkConcept.Core.Contracts.Puente;

/// <summary>
/// La tabla de métodos del puente, versión 1 del contrato (1.1: red combinada, política de acciones y sesión,
/// folio 038; ver DISENO-RED-Y-AUTENTICACION.md §6).
///
/// P3 — Clasificación de un cambio (hueco de VPI §2.6 para este objeto):
///   método nuevo, entrada nueva del catálogo, campo opcional nuevo en una respuesta → MENOR;
///   método quitado, parámetro obligatorio nuevo, cambio de tipo o de forma, cambio del transporte → MAYOR.
/// Cualquier cambio acá exige subir <see cref="ContratoDelPuente.Version"/> y regenerar contrato.json;
/// la instantánea de la compuerta lo detecta si alguien se olvida.
/// </summary>
public static class TablaDeMetodos
{
    private static CampoDelSobre C(string nombre, string tipo, bool opcional = false) => new(nombre, tipo, opcional);

    public static IReadOnlyList<MetodoDelPuente> Metodos { get; } =
    [
        // --- Arranque (P2, P3) ---
        new("bridge.handshake",
            "Lo primero que llama el contenido: versión del puente, catálogo con la versión de cada entrada y, sólo para diagnóstico, la huella de contrato.json.",
            [],
            [C("versionPuente", "string"), C("catalogo", "EntradaVersionada[]"), C("huellaContrato", "string")]),

        new("bridge.ready",
            "«Listo(version)»: el contenido arrancó bien. Borra la marca «pendiente»; sin esta llamada, el próximo arranque vuelve a previous.",
            [C("version", "string")],
            [C("aceptado", "bool")]),

        // --- Documentos del contenido (P5: el contenido no hace DDL; guarda documentos JSON) ---
        new("documents.get",
            "Lee un documento por colección e id.",
            [C("coleccion", "string"), C("id", "string")],
            [C("documento", "Documento", opcional: true)]),

        new("documents.list",
            "Lista los documentos de una colección.",
            [C("coleccion", "string")],
            [C("documentos", "Documento[]")]),

        new("documents.put",
            "Crea o reemplaza un documento. Con versionEsperada, falla con ConflictoDeVersion si otro lo cambió.",
            [C("coleccion", "string"), C("id", "string"), C("versionDeForma", "int"), C("json", "json"), C("versionEsperada", "int", opcional: true)],
            [C("version", "int")]),

        new("documents.delete",
            "Borra un documento.",
            [C("coleccion", "string"), C("id", "string")],
            [C("borrado", "bool")]),

        // --- Encuestas (P1, P13) ---
        new("encuestas.getDefinicion",
            "La definición vigente de una encuesta, para EMPEZAR una nueva. La que está en curso sigue con la suya (P13.2).",
            [C("encuestaId", "string")],
            [C("definicion", "DefinicionEncuesta")]),

        new("encuestas.enqueueRespuesta",
            "Guarda la respuesta y su fila del outbox en la misma transacción (P5). Idempotente por claveIdempotencia.",
            [C("envio", "EnvioDeRespuesta")],
            [C("claveIdempotencia", "string"), C("encolada", "bool")]),

        new("outbox.status",
            "Cuántas filas esperan envío y cuántas quedaron en carta muerta.",
            [],
            [C("pendientes", "int"), C("rechazadas", "int")]),

        // --- Capacidades del dispositivo: sólo por el puente (P12) ---
        new("device.info",
            "Fabricante, modelo, plataforma y versión del sistema.",
            [],
            [C("fabricante", "string"), C("modelo", "string"), C("plataforma", "string"), C("versionSistema", "string")],
            Capacidad: "device.info"),

        new("network.status",
            "Si hay red en este momento. Desde 1.1: «estado» es el estado combinado del borrador §6 (sinRed, redSinServidor, enLinea, enLineaMedida) y «motivo» el texto para mostrar.",
            [],
            [C("conectado", "bool"), C("estado", "EstadoDeRed", opcional: true), C("motivo", "string", opcional: true)],
            Capacidad: "network.status"),

        // --- Política de acciones según la red (borrador §7; puente 1.1). La evalúa el cascarón. ---
        new("actions.policy",
            "Todas las acciones declaradas, evaluadas ahora con la red y la sesión: clase, si están permitidas, si quedan pendientes y por qué.",
            [],
            [C("acciones", "EvaluacionDeAccion[]")]),

        new("actions.check",
            "Una acción por id, evaluada ahora.",
            [C("accion", "string")],
            [C("evaluacion", "EvaluacionDeAccion")]),

        new("outbox.sendNow",
            "«Enviar ahora»: una pasada del sincronizador ya. Acción outbox.enviarAhora (SoloEnLinea): sin servidor, AccionBloqueada.",
            [],
            [C("enviadas", "int"), C("pendientes", "int"), C("rechazadas", "int")]),

        // --- Sesión (P4; puente 1.1). El refresh token nunca cruza el puente (P3): sólo SesionInformada. ---
        new("auth.status",
            "Modo (dispositivo u oidc), estado de la sesión, usuario y si el desbloqueo local se ofrece.",
            [],
            [C("sesion", "SesionInformada")],
            Capacidad: "auth.session"),

        new("auth.signIn",
            "Primer ingreso o reingreso: OIDC code + PKCE en el navegador del sistema (RFC 8252). Acción sesion.ingresar (SoloEnLinea). Espera a que el navegador vuelva.",
            [],
            [C("sesion", "SesionInformada")],
            Capacidad: "auth.session"),

        new("auth.enableUnlock",
            "«Usar la seguridad del teléfono»: sella el refresh token con una clave del Keystore atada a la autenticación del dispositivo (API ≥ 30, D-03 = T1).",
            [],
            [C("sesion", "SesionInformada")],
            Capacidad: "auth.session"),

        new("auth.unlock",
            "Desbloqueo local: BiometricPrompt (huella, rostro, PIN o patrón) abre el refresh token y renueva la sesión en segundo plano.",
            [],
            [C("sesion", "SesionInformada")],
            Capacidad: "auth.session"),

        new("auth.signOut",
            "Cierra la sesión: borra tokens y la clave del Keystore. NO borra filas pendientes: quedan con su dueño (P4).",
            [],
            [C("sesion", "SesionInformada"), C("pendientesConservados", "int")],
            Capacidad: "auth.session")
    ];

    public static IReadOnlyList<EventoDelPuente> Eventos { get; } =
    [
        new("network.changed", "Cambió la conectividad. Desde 1.1 lleva el estado combinado y su motivo (opcionales).",
            [C("conectado", "bool"), C("estado", "EstadoDeRed", opcional: true), C("motivo", "string", opcional: true)]),
        new("outbox.changed", "Cambió la cantidad de filas del outbox.", [C("pendientes", "int"), C("rechazadas", "int")]),
        new("actions.changed", "Cambió la evaluación de alguna acción (por la red o por la sesión). Puente 1.1.",
            [C("acciones", "EvaluacionDeAccion[]")]),
        new("auth.changed", "Cambió la sesión. Puente 1.1.", [C("sesion", "SesionInformada")])
    ];

    public static MetodoDelPuente? Buscar(string nombre) =>
        Metodos.FirstOrDefault(m => string.Equals(m.Nombre, nombre, StringComparison.Ordinal));
}
