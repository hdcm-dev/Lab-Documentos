// Nuevo (ejemplo 12): los mensajes que viajan por las aristas del workflow. Son sólo datos.
namespace MovilidadUrbana.Asistente.Application.Workflows;

/// <summary>Arranque: lo que pidió la persona y qué medios tiene que nombrar el resumen.</summary>
public sealed record SolicitudDeResumen(string EntradaId, string Pedido, IReadOnlyList<string> MediosPedidos);

/// <summary>La solicitud más los datos de resumir_encuesta, ya serializados (el contexto del modelo).</summary>
public sealed record PedidoDeResumen(SolicitudDeResumen Solicitud, string Datos);

/// <summary>Un texto candidato. <paramref name="Vuelta" /> 0 es el primero; 1, el corregido.</summary>
public sealed record Borrador(PedidoDeResumen Pedido, string Texto, int Vuelta, string Autor);

/// <summary>Lo que el verificador manda de vuelta cuando la cuenta no coincide.</summary>
public sealed record PedidoDeCorreccion(PedidoDeResumen Pedido, string BorradorAnterior, IReadOnlyList<string> Errores, int Vuelta);

/// <summary>Salida del workflow: el último texto y lo que dijo la cuenta sobre él.</summary>
public sealed record ResumenVerificado(string EntradaId, string Texto, bool Correcto, IReadOnlyList<string> Errores, int Vueltas, string Autor);
