// Nuevo: ejemplo 03 · Salida estructurada (guía §5.3, OA-D2; plan §E.2 fila 03).
// Del texto libre a una LocalidadPropuesta tipada (GetResponseAsync<T> + TryGetResult), con campos
// anulables, una garantía del código contra la invención y las reglas del dominio que deciden.
//   dotnet run -- --fake [--verificar] [--extension | --cp34] [--plazo-s N]
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente;
using MovilidadUrbana.Asistente.Application.Localidades;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Domain.Rules;
using MovilidadUrbana.Asistente.Infrastructure.AI;
using MovilidadUrbana.Asistente.Infrastructure.Persistencia;

const string Ejemplo = "03-Salida-Estructurada";
const string Oraculo = "salida-estructurada";

var a = ArgumentosDeEjemplo.Leer(args);
var extension = a.Resto.Contains("--extension");
// REC-16: estímulo de una entrada con «CP 34» (para la fase con modelo; en --fake el guion inventa 3400).
var cp34 = a.Resto.Contains("--cp34");
if (extension && cp34)
{
    Console.Error.WriteLine("--extension y --cp34 eligen archivos de entradas distintos: usá uno solo.");
    return 2;
}
ConfiguracionDelModelo config;
try
{
    config = ConfiguracionDelModelo.DesdeEntorno().Validar();
}
catch (ConfiguracionInvalidaException e)
{
    Console.Error.WriteLine($"Configuración rechazada: {e.Message}");
    return 1;
}

var rutaEntradas = Path.Combine(AppContext.BaseDirectory,
    extension ? "entradas.ext.v1.json" : cp34 ? "entradas.cp34.v1.json" : "entradas.v1.json");
var entradas = ArchivoDeEntradas.Leer(rutaEntradas).Entradas;

// Cliente SIEMPRE por la capa única (FabricaDeClientes): razonamiento apagado, num_ctx, temperatura y semilla.
var guion = a.Fake ? GuionDelEjemplo.Para(entradas) : null;
string? version = null, digest = null;
if (!a.Fake)
{
    var predicado = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, PredicadoDeSalto.Chat);
    if (predicado.Saltar)
    {
        Console.Error.WriteLine($"No se corre contra el modelo: {predicado.Motivo}");
        return 1;
    }
    (version, digest) = (predicado.VersionDeOllama, predicado.Digest);
}
IChatClient cliente = a.Fake
    ? FabricaDeClientes.Envolver(guion!, config, Adaptador.OllamaSharp)
    : FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);

var extractor = new ExtractorDeLocalidadConModelo(cliente);
var repositorio = new RepositorioDeLocalidadesEnMemoria();
var alta = new AltaDesdeTextoService(extractor, new LocalidadService(repositorio));

using var reg = new EscritorDeRegistro(new ContextoDeCorrida
{
    Ejemplo = Ejemplo,
    Configuracion = config,
    Adaptador = Adaptador.OllamaSharp,
    Modelo = a.Fake ? "guion" : config.Modelo,
    Digest = digest,
    OllamaVersion = version,
    EntradasVersion = "v1",
    EntradasHash = Hashes.Sha256DeArchivo(rutaEntradas),
    PaquetesHash = Hashes.DePaquetes(AppContext.BaseDirectory) ?? Hashes.DePaquetes(),
    Maquina = a.Fake ? "guion" : Environment.GetEnvironmentVariable("RLLM_MAQUINA")
});

var plazo = a.CrearPlazo();
var todoBien = true;
var guardadasEsperadas = 0;
var conHabitantesCrudo = 0;
var jsonValidos = 0;
var cpFueraDelTexto = 0;

foreach (var entrada in entradas)
{
    if (plazo.Vencido)
    {
        reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = Oraculo, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
        Console.WriteLine($"[{entrada.Id}] excluida: plazo");
        continue;
    }

    string? estadoForzado = null;
    if (!a.Fake && config.Servidor == TipoDeServidor.Compartido &&
        await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo) == false)
    {
        estadoForzado = "frio";
    }

    var reloj = EscritorDeRegistro.Cronometro();
    var detalle = await extractor.ExtraerConDetalleAsync(entrada.Texto);
    var opcionesEfectivas = a.Fake ? guion!.Llamadas[^1].Opciones : EfectivasReales(detalle.Opciones, config);
    var linea = reg.Llamada(entrada.Id, 1, detalle.Enviados, opcionesEfectivas, detalle.Respuesta, reloj.Elapsed,
        paso: "extraer", estadoCacheForzado: estadoForzado);
    reg.Escribir(linea);

    var decision = AltaDesdeTextoService.Decidir(detalle.Extraccion);

    // Conducta del modelo (descriptiva): ¿propuso habitantes donde el pedido no los trae?
    if (detalle.JsonValido) jsonValidos++;
    var crudo = detalle.Cruda?.Habitantes is not null;
    if (!entrada.TraeHabitantes && crudo) conHabitantesCrudo++;
    // ¿Propuso un código postal que no está en el texto? (lo descarta GarantiaDelTexto; REC-16)
    if (detalle.Extraccion.Descartados.Contains("codigoPostal")) cpFueraDelTexto++;

    // La persona confirma lo que pasó las reglas (simulado: el ejemplo no tiene pantalla). Lo que no
    // pasó no se escribe: la pantalla pediría el dato que falta.
    if (decision.Estado == EstadoDeLaPropuesta.Lista)
    {
        var guardado = await alta.ConfirmarAsync(decision);
        if (guardado.IsSuccess) guardadasEsperadas++;
    }

    var invariantes = new List<string>();
    if (linea.Razonamiento != "None") invariantes.Add("razonamiento no apagado");
    if (linea.NumCtx is not > 0) invariantes.Add("sin num_ctx");
    if (decision.Estado == EstadoDeLaPropuesta.Lista)
    {
        var numeros = GarantiaDelTexto.Numeros(entrada.Texto);
        var m = decision.Modelo;
        if (!LocalidadRules.CodigoPostalValido(m.CodigoPostal) || !LocalidadRules.ProvinciaValida(m.Provincia) || !LocalidadRules.HabitantesValidos(m.Habitantes))
            invariantes.Add("propuesta «lista» que no pasa las reglas");
        if (!numeros.Contains(m.CodigoPostal) || !numeros.Contains(m.Habitantes!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            invariantes.Add("valor numérico que no está en la entrada");
    }
    if (a.Fake && GuionDelEjemplo.DecisionEsperada[entrada.Id] != decision.Codigo)
        invariantes.Add($"decisión {decision.Codigo}, se esperaba {GuionDelEjemplo.DecisionEsperada[entrada.Id]}");

    string resultado;
    string motivo;
    if (linea.TruncadoPosible == true)
    {
        (resultado, motivo) = (ResultadoDeEntrada.Excluida, "truncado_posible");
    }
    else
    {
        resultado = invariantes.Count == 0 ? ResultadoDeEntrada.Pasa : ResultadoDeEntrada.Falla;
        motivo = $"decision={decision.Codigo}" + (invariantes.Count == 0 ? string.Empty : "; " + string.Join("; ", invariantes));
        todoBien &= invariantes.Count == 0;
    }
    reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = entrada.Id, Oraculo = Oraculo, Resultado = resultado, Motivo = motivo });

    Console.WriteLine($"[{entrada.Id}] {entrada.Texto}");
    Console.WriteLine($"  modelo devolvió: {detalle.Respuesta.Text}");
    Console.WriteLine($"  TryGetResult: {(detalle.JsonValido ? "true" : "false")}" +
                      (detalle.Extraccion.Descartados.Count > 0 ? $"; descartado por no estar en el texto: {string.Join(", ", detalle.Extraccion.Descartados)}" : string.Empty));
    Console.WriteLine($"  decisión de las reglas: {decision.Codigo}" +
                      (decision.Errores.Count > 0 ? $" → {string.Join(" ", decision.Errores.Values)}" : string.Empty));
    Console.WriteLine($"  oráculo: {resultado}");
}

// Ninguna escritura sin las reglas: lo guardado es exactamente lo que pasó las reglas y se confirmó,
// y cada alta guardada vuelve a pasar LocalidadRules.
var escriturasOk = repositorio.Altas.Count == guardadasEsperadas &&
                   repositorio.Altas.All(l => LocalidadRules.CodigoPostalValido(l.CodigoPostal) &&
                                              LocalidadRules.ProvinciaValida(l.Provincia) &&
                                              LocalidadRules.HabitantesValidos(l.Habitantes));
if (!escriturasOk) Console.WriteLine("INVARIANTE ROTO: hay escrituras que no pasaron por las reglas");
todoBien &= escriturasOk;

var sinDato = entradas.Count(e => !e.TraeHabitantes);
Console.WriteLine($"altas guardadas: {repositorio.Altas.Count}");
Console.WriteLine($"conducta del modelo (descriptiva, sin umbral): JSON válido {jsonValidos}/{entradas.Count}; " +
                  $"propuso habitantes sin el dato {conHabitantesCrudo}/{sinDato}; " +
                  $"código postal que no está en el texto {cpFueraDelTexto}/{entradas.Count}" + (a.Fake ? " (guion, no es el modelo)" : string.Empty));
Console.WriteLine($"registro: {reg.RenglonesEscritos} líneas en {Path.GetFileName(reg.Ruta)}");
Console.WriteLine(todoBien ? "invariantes de código: se cumplen" : "invariantes de código: NO se cumplen");
return a.Verificar && !todoBien ? 1 : 0;

// Opciones con que salió la llamada real: las de la llamada más lo que agrega la capa única y el
// formato que pone GetResponseAsync<T>. No fija nada a mano: reproduce la capa para el registro.
static ChatOptions EfectivasReales(ChatOptions pedidas, ConfiguracionDelModelo config)
{
    var efectivas = pedidas.Clone();
    efectivas.ResponseFormat = ChatResponseFormat.ForJsonSchema<LocalidadPropuesta>();
    OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
    return efectivas;
}
