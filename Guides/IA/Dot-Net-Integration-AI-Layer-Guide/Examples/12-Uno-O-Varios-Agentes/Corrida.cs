// Nuevo: la corrida del ejemplo 12, separada de Program.cs para que las pruebas la reusen. Corre los
// mismos 2 pedidos por las dos variantes (diseño pareado), deja una línea R-LLM por llamada al modelo y
// una por entrada y variante, informa el costo de cada variante y comprueba las garantías de código.
using Microsoft.Extensions.AI;
using MovilidadUrbana.Asistente.Application.Resumenes;
using MovilidadUrbana.Asistente.Application.Workflows;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;
using MovilidadUrbana.Asistente.Comun.Pruebas;
using MovilidadUrbana.Asistente.Comun.Registro;
using MovilidadUrbana.Asistente.Infrastructure;
using MovilidadUrbana.Asistente.Infrastructure.AI;

namespace MovilidadUrbana.Asistente;

/// <summary>Lo que costó una variante: llamadas al modelo, tokens informados y vueltas.</summary>
public sealed record CostoDeVariante(string Variante, int Llamadas, long TokensDeEntrada, long TokensDeSalida, int Vueltas, int Pasan, int Validas);

public sealed record ResultadoDeCorrida(IReadOnlyList<string> InvariantesQueFallan, IReadOnlyList<CostoDeVariante> Costos, string? RutaDelRegistro);

public static class Corrida
{
    public const string Ejemplo = "12-Uno-O-Varios-Agentes";

    public static readonly Variante[] Variantes = [Variante.UnAgente, Variante.DosAgentes];

    public static async Task<ResultadoDeCorrida> CorrerAsync(
        ArgumentosDeEjemplo a, ConfiguracionDelModelo config, TextWriter salida, CancellationToken cancellationToken = default)
    {
        var rutaEntradas = Path.Combine(AppContext.BaseDirectory, "entradas.v1.json");
        var entradas = ArchivoDeEntradas.Leer(rutaEntradas).Entradas;
        var invariantes = new List<string>();

        string? version = null, digest = null;
        if (!a.Fake)
        {
            var p = await PredicadoDeSalto.EvaluarAsync(config.Endpoint, config.Modelo, PredicadoDeSalto.Chat, cancellationToken);
            if (p.Saltar) return new ResultadoDeCorrida([$"no se corre contra el modelo: {p.Motivo}"], [], null);
            (version, digest) = (p.VersionDeOllama, p.Digest);
        }

        // Cliente: SIEMPRE por la capa única. Contra el modelo, uno solo para redactor y corrector. En
        // --fake, un guion por variante con un espía debajo de la capa (ve las opciones efectivas).
        var espias = new List<ClienteEspia>();
        IChatClient? real = a.Fake ? null : FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config);
        var capas = Variantes.ToDictionary(v => v, v =>
        {
            if (real is not null) return real;
            var espia = new ClienteEspia(new GuionChatClient(GuionDelEjemplo.Pasos(v, entradas), "Sin cambios."));
            espias.Add(espia);
            return FabricaDeClientes.Envolver(espia, config, Adaptador.OllamaSharp);
        });

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

        using var bd = BaseDeEjemplo.Crear();
        var fuente = new FuenteDeResultados(bd.Resultados);
        var resultados = await fuente.ResumirEncuestaAsync(cancellationToken);
        salida.WriteLine($"siembra rotulada «escrito para el ejemplo»: {resultados.Total} respuestas; " +
                         string.Join(", ", resultados.Medios.Select(m => $"{m.Clave} {ComparadorDePorcentajes.Formato(m.Porcentaje)} %")) +
                         " (estímulo del mismo autor)");

        var acumulado = Variantes.ToDictionary(v => v, _ => (Llamadas: 0, Entrada: 0L, Salida: 0L, Vueltas: 0, Pasan: 0, Validas: 0));
        var plazo = a.CrearPlazo();

        // Pareado: cada pedido corre por las dos variantes, una detrás de la otra.
        foreach (var entrada in entradas)
        {
            foreach (var variante in Variantes)
            {
                var nombre = CaminoDelResumen.Nombre(variante);
                var id = $"{entrada.Id}.{nombre}";
                if (plazo.Vencido)
                {
                    reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = id, Oraculo = OraculoDePorcentajes.Nombre, Resultado = ResultadoDeEntrada.Excluida, Motivo = "plazo" });
                    salida.WriteLine($"{id}: excluida (plazo)");
                    continue;
                }

                string? frio = null;
                if (!a.Fake && config.Servidor == TipoDeServidor.Compartido &&
                    await PredicadoDeSalto.ModeloCargadoAsync(config.Endpoint, config.Modelo, cancellationToken) == false)
                {
                    frio = "frio";
                }

                var grabadas = new List<LlamadaGrabada>();
                var capa = capas[variante];
                var workflow = CaminoDelResumen.Construir(variante, fuente,
                    new GrabadorDeLlamadas(capa, "redactor", grabadas),
                    variante == Variante.DosAgentes ? new GrabadorDeLlamadas(capa, "corrector", grabadas) : null);
                var corrida = await ConductorDelCamino.CorrerAsync(workflow, new SolicitudDeResumen(id, entrada.Texto, entrada.Medios), cancellationToken);

                var contexto = FuenteDeResultados.Contexto(resultados, entrada.Medios);
                var lineas = new List<RegistroDeLlamada>();
                for (var i = 0; i < grabadas.Count; i++)
                {
                    var g = grabadas[i];
                    var efectivas = g.Opciones?.Clone() ?? new ChatOptions();
                    OpcionesDelProveedor.Aplicar(efectivas, config, Adaptador.OllamaSharp);
                    var linea = reg.Llamada(id, i + 1, g.Mensajes, efectivas, g.Respuesta, g.Reloj,
                        paso: $"{nombre}/{g.Nodo}{(i > 0 ? "/vuelta" : "")}", herramientas: [], contexto: contexto,
                        estadoCacheForzado: i == 0 ? frio : null);
                    reg.Escribir(linea);
                    lineas.Add(linea);
                    if (linea.Razonamiento != "None" || linea.NumCtx is not > 0)
                    {
                        invariantes.Add($"{id}, llamada {i + 1}: no salió con razonamiento apagado y num_ctx explícito");
                    }
                }

                invariantes.AddRange(InvariantesDelCamino(id, variante, corrida, grabadas, resultados, entrada));

                var veredicto = OraculoDePorcentajes.Evaluar(lineas);
                reg.Escribir(new RegistroDeEntrada { Ejemplo = Ejemplo, EntradaId = id, Oraculo = OraculoDePorcentajes.Nombre, Resultado = veredicto.Resultado, Motivo = veredicto.Motivo });

                var c = acumulado[variante];
                acumulado[variante] = (c.Llamadas + lineas.Count, c.Entrada + lineas.Sum(l => l.PromptEvalCount ?? 0), c.Salida + lineas.Sum(l => l.EvalCount ?? 0),
                    c.Vueltas + (corrida.Salida?.Vueltas ?? 0),
                    c.Pasan + (veredicto.Resultado == ResultadoDeEntrada.Pasa ? 1 : 0),
                    c.Validas + (veredicto.Resultado == ResultadoDeEntrada.Excluida ? 0 : 1));

                salida.WriteLine($"{id}: «{entrada.Texto}»");
                salida.WriteLine($"  camino: {string.Join(" → ", corrida.Camino)}");
                foreach (var (l, g) in lineas.Zip(grabadas))
                {
                    salida.WriteLine($"  {g.Nodo} {(a.Fake ? "(guion)" : "(modelo)")}: {l.Salida}");
                }
                salida.WriteLine($"  verificador: {(corrida.Salida?.Correcto == true ? "coincide" : "no coincide: " + string.Join("; ", corrida.Salida?.Errores ?? ["sin salida"]))}");
                salida.WriteLine($"  oráculo {OraculoDePorcentajes.Nombre}: {veredicto.Resultado}{(veredicto.Motivo is null ? "" : $" ({veredicto.Motivo})")}");
            }
        }

        // Garantías de código que no dependen de ninguna entrada.
        invariantes.AddRange(Garantias.ResumirEncuestaSinDatosDePersona());
        invariantes.AddRange(Garantias.VerificadorSinModelo());
        if (a.Fake && espias.SelectMany(e => e.OpcionesVistas).Any(o => o?.Reasoning?.Effort != ReasoningEffort.None || OpcionesDelProveedor.NumCtxDe(o) is not > 0))
        {
            invariantes.Add("--fake: el cliente espía vio una llamada sin razonamiento apagado o sin num_ctx");
        }

        var costos = Variantes.Select(v => new CostoDeVariante(CaminoDelResumen.Nombre(v), acumulado[v].Llamadas, acumulado[v].Entrada,
            acumulado[v].Salida, acumulado[v].Vueltas, acumulado[v].Pasan, acumulado[v].Validas)).ToList();
        salida.WriteLine("costo por variante (los mismos pedidos; sin tasa: con 2 entradas ilustra el costo, no responde «¿uno o varios?»):");
        foreach (var c in costos)
        {
            salida.WriteLine($"  {c.Variante,-11} llamadas al modelo {c.Llamadas} · tokens de entrada {c.TokensDeEntrada} · de salida {c.TokensDeSalida} · vueltas {c.Vueltas} · oráculo {c.Pasan} de {c.Validas}");
        }
        salida.WriteLine($"registro: {reg.RenglonesEscritos} líneas en {Path.GetFileName(reg.Ruta)}");
        return new ResultadoDeCorrida(invariantes, costos, reg.Ruta);
    }

    /// <summary>
    /// Lo que el código fija y se comprueba en cada corrida: el camino es el de la variante, hay a lo
    /// sumo una vuelta, cada nodo con modelo llama una vez, el corrector sólo existe en «dos-agentes», la
    /// salida es la cuenta del verificador y al modelo no llega ningún dato de persona.
    /// </summary>
    public static IEnumerable<string> InvariantesDelCamino(
        string id, Variante variante, CorridaDelCamino corrida, IReadOnlyList<LlamadaGrabada> grabadas,
        Application.Encuestas.ResultadosDeEncuesta resultados, Entrada entrada)
    {
        if (corrida.Salida is not { } s)
        {
            yield return $"{id}: el workflow terminó sin salida";
            yield break;
        }
        if (s.Vueltas > CaminoDelResumen.MaximoDeVueltas) yield return $"{id}: {s.Vueltas} vueltas, más que el tope";
        var esperado = ConductorDelCamino.CaminoEsperado(variante, s.Vueltas);
        if (!corrida.Camino.SequenceEqual(esperado))
        {
            yield return $"{id}: camino {string.Join("→", corrida.Camino)}; el código fija {string.Join("→", esperado)}";
        }
        if (grabadas.Count != 1 + s.Vueltas) yield return $"{id}: {grabadas.Count} llamadas al modelo para {s.Vueltas} vueltas";
        var delCorrector = grabadas.Count(g => g.Nodo == "corrector");
        if (variante == Variante.UnAgente && delCorrector > 0) yield return $"{id}: «un-agente» llamó al corrector";
        if (variante == Variante.DosAgentes && delCorrector != s.Vueltas) yield return $"{id}: el corrector hizo {delCorrector} llamadas para {s.Vueltas} vueltas";
        if (grabadas.Count > 0 && grabadas[^1].Respuesta.Text != s.Texto) yield return $"{id}: la salida no es el último texto del modelo";
        var recalculo = ComparadorDePorcentajes.Verificar(s.Texto, entrada.Medios, resultados);
        if (recalculo.Correcto != s.Correcto) yield return $"{id}: el veredicto del verificador no es la cuenta";
        if (grabadas.SelectMany(g => g.Mensajes).Any(m => m.Text?.Contains("Persona ", StringComparison.Ordinal) == true))
        {
            yield return $"{id}: un nombre de persona llegó al modelo";
        }
    }
}
