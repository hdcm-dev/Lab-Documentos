// Nuevo: lo que «pide» y «responde» el guion del modo --fake. La herramienta que pide es la
// McpClientTool montada, así que su ejecución es un pedido MCP real al servidor en loopback.
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Infrastructure.Mcp;

namespace MovilidadUrbana.Asistente;

public static class GuionDelEjemplo
{
    public static IEnumerable<PasoDelGuion> Pasos(IEnumerable<Entrada> entradas)
    {
        foreach (var e in entradas)
        {
            yield return PasoDelGuion.Herramienta($"{e.Id}-1", LecturasMcp.Listar);
            yield return PasoDelGuion.Texto("Tenés 2 localidades cargadas: Corrientes (Corrientes) y Resistencia (Chaco).");
        }
    }
}
