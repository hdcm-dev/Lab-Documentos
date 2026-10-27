namespace FrameworkConcept.Shell.Bridge;

/// <summary>
/// P3 — El ÚNICO objeto que el contenido puede invocar, registrado con <c>HybridWebView.SetInvokeJavaScriptTarget</c>.
/// Tiene un solo método, <see cref="Call"/>: el contenido lo llama desde JavaScript con
/// <c>window.HybridWebView.InvokeDotNet('Call', [sobreJson])</c>, donde sobreJson es el TEXTO de un sobre JSON-RPC 2.0
/// con «v» (ver Core/Contracts/Puente/Sobre.cs). Devuelve el texto del sobre de respuesta.
///
/// Agregar una operación no agrega un método acá: agrega una fila a la TablaDeMetodos (el contrato) y un caso en
/// <see cref="DespachadorDelPuente"/>. Así la superficie C# expuesta al contenido no cambia nunca.
/// La llamada viaja como POST al mismo origen (/__hwvInvokeDotNet): sólo la página local la puede hacer, porque el
/// servidor de contenido bloquea todo otro origen.
/// </summary>
public sealed class PuenteDelCascaron(DespachadorDelPuente despachador)
{
    public Task<string> Call(string sobre) => despachador.AtenderAsync(sobre);
}
