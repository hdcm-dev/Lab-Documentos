// Nuevo (ejemplo 10). Sin servidor de modelos: todas corren en la fase build.
using MovilidadUrbana.Asistente.Application.Encuestas;
using MovilidadUrbana.Asistente.Application.Workflows;
using MovilidadUrbana.Asistente.Domain;
using Microsoft.Agents.AI.Workflows;

namespace MovilidadUrbana.Asistente.Pruebas;

public sealed class WorkflowYFuncionTests
{
    private static IEnumerable<TestCaseData> Casos() =>
        Entradas.Leer().Entradas.Select(e => new TestCaseData(e).SetName($"Entrada_{e.Id}"));

    [TestCaseSource(nameof(Casos))]
    public async Task FuncionYWorkflow_DanLoMismo_YElWorkflowSeRehidrata(EntradaDeEncuesta entrada)
    {
        using var tope = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        var c = await Corrida.CompararAsync(entrada, TextWriter.Null, tope.Token);

        Assert.That(Corrida.Invariantes(entrada.Id, c), Is.Empty);
    }

    [Test]
    public async Task Workflow_EmiteUnCheckpointPorSuperstep()
    {
        var entrada = Entradas.Leer().Entradas.First(e => e.Id == "e02");
        await using var corrida = await InProcessExecution.RunStreamingAsync(
            EncuestaComoWorkflow.Construir(Corrida.CrearServicio()), new IniciarEncuesta(), CheckpointManager.CreateInMemory(), "prueba");

        var r = await ConductorDelWorkflow.ConducirAsync(corrida, new Persona(entrada.Respuestas));

        Assert.That(r.Resultado?.Estado, Is.EqualTo("completa[1,2,1]"));
        Assert.That(r.Supersteps, Is.GreaterThan(0));
        Assert.That(r.Checkpoints, Is.EqualTo(r.Supersteps));
    }

    [Test]
    public void Funcion_RechazaElMedioFueraDeCatalogo_ConElMismoValidarPaso()
    {
        var entrada = Entradas.Leer().Entradas.First(e => e.Id == "e02");

        var r = EncuestaComoFuncion.Completar(new Persona(entrada.Respuestas), Corrida.CrearServicio());

        Assert.That(r.Rechazos, Is.EqualTo(new[] { "2.medios" }));
        Assert.That(r.Datos, Does.Contain("|auto|"));
    }

    [Test]
    public void Funcion_SinRespuestas_QuedaIncompletaEnElPaso1()
    {
        var r = EncuestaComoFuncion.Completar(new Persona([]), Corrida.CrearServicio());

        Assert.That(r.Estado, Is.EqualTo("incompleta@1[0,0,0]"));
    }

    // Prueba de cableado del estímulo (plan §E.1, (i)): todo valor de catálogo de una respuesta no
    // negativa está en Catalogos; cada respuesta negativa declarada la rechaza ValidarPaso.
    [Test]
    public void Estimulo_ValoresDeCatalogo_YNegativosDeclarados()
    {
        var servicio = Corrida.CrearServicio();
        var archivo = Entradas.Leer();
        Assert.That(archivo.Rotulo, Is.EqualTo("escrito para el ejemplo"));

        foreach (var e in archivo.Entradas)
        {
            var modelo = new EncuestaModel();
            foreach (var r in e.Respuestas)
            {
                if (!r.Negativo)
                {
                    Assert.That(r.Medios ?? [], Is.SubsetOf(Catalogos.Medios.Select(m => m.Clave)), $"{e.Id} paso {r.Paso}");
                    if (r.Frecuencia is { } f) Assert.That(Catalogos.Frecuencias.Select(x => x.Clave), Does.Contain(f), e.Id);
                    if (r.Motivo is { } m) Assert.That(Catalogos.Motivos.Select(x => x.Clave), Does.Contain(m), e.Id);
                    if (r.Nombre is { } n) Assert.That(n, Does.Match(@"^Persona (0[1-9]|1[0-9]|20)$"), e.Id);
                }
                r.AplicarA(modelo);
                Assert.That(servicio.ValidarPaso(r.Paso, modelo), r.Negativo ? Is.Not.Empty : Is.Empty, $"{e.Id} paso {r.Paso}");
            }
        }
    }
}

public sealed class InvariantesTests
{
    // --verificar sale con 1 si algo de esto falla: la prueba comprueba que el chequeo no es de adorno.
    [Test]
    public void Invariantes_DetectanRehidratadoDistinto_YCheckpointsDeMenos()
    {
        var c = new ComparacionDeEntrada("completa[1,1,1]", "completa[1,1,1]", "completa[1,1,1]", "incompleta@3[1,1,0]",
            "d", "d", "d", Supersteps: 11, Checkpoints: 10, Cortado: true, PedidosTrasRehidratar: 1);

        var fallan = Corrida.Invariantes("x", c);

        Assert.That(fallan, Has.Some.Contains("rehidratado"));
        Assert.That(fallan, Has.Some.Contains("checkpoints"));
    }
}
