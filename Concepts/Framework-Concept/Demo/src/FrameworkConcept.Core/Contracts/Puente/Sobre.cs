using System.Text.Json;

namespace FrameworkConcept.Core.Contracts.Puente;

// P3 — El puente tiene UN solo método nativo, Call(sobre). Lo que viaja es un «sobre» JSON-RPC 2.0
// (https://www.jsonrpc.org/specification) con un campo más, «v»: la versión del puente que el
// contenido espera. Así, agregar una operación no cambia la superficie C# del cascarón: agrega una fila
// a la TablaDeMetodos, que es lo que el contrato fija.

/// <summary>Pedido del contenido al cascarón.</summary>
/// <param name="Jsonrpc">Siempre «2.0».</param>
/// <param name="V">Versión SemVer del puente con la que se escribió el contenido.</param>
/// <param name="Id">Identificador del pedido; la respuesta lo repite. Un evento no lo lleva.</param>
/// <param name="Method">Nombre de la tabla de métodos, por ejemplo «bridge.handshake».</param>
/// <param name="Params">Parámetros con nombre (un objeto JSON), o nada.</param>
public sealed record SobrePedido(string Jsonrpc, string V, string Id, string Method, JsonElement? Params = null)
{
    public const string VersionJsonRpc = "2.0";
}

/// <summary>Respuesta del cascarón. Lleva <see cref="Result"/> o <see cref="Error"/>, nunca los dos.</summary>
public sealed record SobreRespuesta(string Jsonrpc, string V, string Id, JsonElement? Result = null, ErrorDelSobre? Error = null)
{
    public static SobreRespuesta Exito(string v, string id, JsonElement result) => new(SobrePedido.VersionJsonRpc, v, id, result);

    public static SobreRespuesta Fallo(string v, string id, int code, string message) =>
        new(SobrePedido.VersionJsonRpc, v, id, Error: new ErrorDelSobre(code, message));
}

/// <summary>El objeto «error» de JSON-RPC 2.0.</summary>
public sealed record ErrorDelSobre(int Code, string Message, JsonElement? Data = null);

/// <summary>
/// Aviso del cascarón al contenido, sin respuesta (una «notification» de JSON-RPC: no lleva id).
/// Lo envía el cascarón con <c>HybridWebView.InvokeJavaScriptAsync</c>.
/// </summary>
public sealed record SobreEvento(string Jsonrpc, string V, string Method, JsonElement? Params = null);

/// <summary>Códigos de error. Los negativos de -32700 a -32600 son los de JSON-RPC 2.0; los -3200x, propios.</summary>
public static class CodigosDeError
{
    public const int JsonInvalido = -32700;
    public const int PedidoInvalido = -32600;
    public const int MetodoInexistente = -32601;
    public const int ParametrosInvalidos = -32602;
    public const int ErrorInterno = -32603;

    /// <summary>El mayor de «v» no es el del puente del cascarón.</summary>
    public const int VersionDelPuenteIncompatible = -32001;

    /// <summary>La capacidad del catálogo que el método necesita no está en este cascarón.</summary>
    public const int CapacidadNoDisponible = -32002;

    /// <summary>El usuario negó el permiso que la capacidad necesita (P12: rationale y salida a ajustes).</summary>
    public const int PermisoDenegado = -32003;

    /// <summary>Documento cambiado por otro: la versión esperada no coincide.</summary>
    public const int ConflictoDeVersion = -32004;

    /// <summary>
    /// Borrador §7 — La acción que ejecuta el método no está permitida ahora (SoloEnLinea sin servidor, sesión
    /// bloqueada…). «message» lleva el motivo para mostrar; «data» lleva la EvaluacionDeAccion. (Puente 1.1.)
    /// </summary>
    public const int AccionBloqueada = -32005;

    /// <summary>P4 — La persona cerró el navegador del emisor o canceló el BiometricPrompt. No es un error del sistema. (Puente 1.1.)</summary>
    public const int AutenticacionCancelada = -32006;

    /// <summary>P4 — El emisor o el Keystore rechazaron la operación (invalid_grant, clave invalidada, sin bloqueo de pantalla). (Puente 1.1.)</summary>
    public const int AutenticacionFallida = -32007;
}
