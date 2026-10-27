using FrameworkConcept.Core.Auth;
using FrameworkConcept.Core.Red;

namespace FrameworkConcept.Core.Tests.Red;

/// <summary>
/// Borrador §7 y P4 — El evaluador de acciones (DISENO §4.3): matriz clase × estado de red × sesión, y las reglas
/// especiales de «sesion.ingresar».
/// </summary>
[TestFixture]
public class EvaluadorDeAccionesTests
{
    private static readonly EstadoDeRed[] Redes = Enum.GetValues<EstadoDeRed>();

    // Las sesiones de la matriz de §10.1, con nombre para que el mensaje de una falla diga cuál fue.
    private static readonly (string Nombre, SesionInformada Sesion)[] Sesiones =
    [
        ("Dispositivo", SesionInformada.PorDispositivo),
        ("SinCuenta", Oidc(EstadoDeSesion.SinCuenta)),
        ("Ingresando", Oidc(EstadoDeSesion.Ingresando)),
        ("Bloqueada", Oidc(EstadoDeSesion.Bloqueada)),
        ("Activa", Oidc(EstadoDeSesion.Activa)),
        ("Vencida", Oidc(EstadoDeSesion.Vencida)),
        ("Revocada", Oidc(EstadoDeSesion.Revocada))
    ];

    private static SesionInformada Oidc(EstadoDeSesion estado) =>
        new(ModoDeSesion.Oidc, estado, estado == EstadoDeSesion.SinCuenta ? null : "encuestador", DesbloqueoLocal.Disponible);

    private static AccionDeclarada DeClase(ClaseDeAccion clase) => new("prueba." + clase, clase, ["prueba"], "acción de prueba");

    private static bool EntradaAbierta(SesionInformada s) =>
        s.Modo == ModoDeSesion.Dispositivo || s.Estado is EstadoDeSesion.Activa or EstadoDeSesion.Vencida or EstadoDeSesion.Revocada;

    private static bool SesionValida(SesionInformada s) => s.Modo == ModoDeSesion.Dispositivo || s.Estado == EstadoDeSesion.Activa;

    private static bool ConServidor(EstadoDeRed r) => r is EstadoDeRed.EnLinea or EstadoDeRed.EnLineaMedida;

    private static IEnumerable<(AccionDeclarada Accion, EstadoDeRed Red, string NombreDeSesion, SesionInformada Sesion)> Matriz() =>
        from accion in PoliticaDeAcciones.Acciones.Concat(Enum.GetValues<ClaseDeAccion>().Select(DeClase))
        from red in Redes
        from s in Sesiones
        select (accion, red, s.Nombre, s.Sesion);

    // --- Propiedades de toda la matriz -----------------------------------------------------------------------

    [Test]
    [Description("§7 «botón deshabilitado + motivo»: toda acción no permitida dice por qué")]
    public void TodaAccionNoPermitidaLlevaMotivo()
    {
        foreach (var (accion, red, nombre, sesion) in Matriz())
        {
            var e = EvaluadorDeAcciones.Evaluar(accion, red, sesion);
            if (!e.Permitida) Assert.That(e.Motivo, Is.Not.Null.And.Not.Empty, $"{accion.Id} / {red} / {nombre}");
        }
    }

    [Test]
    [Description("Queda pendiente sólo una Encolable permitida; y una pendiente dice por qué")]
    public void SoloLasEncolablesQuedanPendientes()
    {
        foreach (var (accion, red, nombre, sesion) in Matriz())
        {
            var e = EvaluadorDeAcciones.Evaluar(accion, red, sesion);
            if (!e.QuedaPendiente) continue;
            Assert.That(accion.Clase, Is.EqualTo(ClaseDeAccion.Encolable), $"{accion.Id} / {red} / {nombre}");
            Assert.That(e.Permitida, Is.True, $"{accion.Id} / {red} / {nombre}");
            Assert.That(e.Motivo, Is.Not.Null.And.Not.Empty, $"{accion.Id} / {red} / {nombre}");
        }
    }

    [Test]
    [Description("La evaluación repite el id y la clase de la acción (el contenido la muestra tal cual llega)")]
    public void LaEvaluacionNombraALaAccion()
    {
        foreach (var (accion, red, _, sesion) in Matriz())
        {
            var e = EvaluadorDeAcciones.Evaluar(accion, red, sesion);
            Assert.That((e.Accion, e.Clase), Is.EqualTo((accion.Id, accion.Clase)));
        }
    }

    [Test]
    [Description("Permitida y sin pendiente: no hay nada que explicar")]
    public void PermitidaSinPendienteNoLlevaMotivo()
    {
        foreach (var (accion, red, nombre, sesion) in Matriz())
        {
            var e = EvaluadorDeAcciones.Evaluar(accion, red, sesion);
            if (e.Permitida && !e.QuedaPendiente) Assert.That(e.Motivo, Is.Null, $"{accion.Id} / {red} / {nombre}");
        }
    }

    // --- Una regla por clase (DISENO §4.3) ----------------------------------------------------------------

    [Test]
    public void SoloLocalDependeSoloDeLaEntrada()
    {
        foreach (var red in Redes)
            foreach (var (nombre, sesion) in Sesiones)
            {
                var e = EvaluadorDeAcciones.Evaluar(DeClase(ClaseDeAccion.SoloLocal), red, sesion);
                Assert.That((e.Permitida, e.QuedaPendiente), Is.EqualTo((EntradaAbierta(sesion), false)), $"{red} / {nombre}");
            }
    }

    [Test]
    [Description("P4, P5 — Con la entrada abierta, una Encolable NUNCA se bloquea: se guarda y queda pendiente si hace falta")]
    public void EncolableNuncaSeBloqueaConLaEntradaAbierta()
    {
        foreach (var red in Redes)
            foreach (var (nombre, sesion) in Sesiones)
            {
                var e = EvaluadorDeAcciones.Evaluar(DeClase(ClaseDeAccion.Encolable), red, sesion);
                Assert.That(e.Permitida, Is.EqualTo(EntradaAbierta(sesion)), $"{red} / {nombre}");
                if (e.Permitida)
                    Assert.That(e.QuedaPendiente, Is.EqualTo(!ConServidor(red) || !SesionValida(sesion)), $"{red} / {nombre}");
            }
    }

    [Test]
    public void SoloEnLineaPideEntradaServidorYSesionValida()
    {
        foreach (var red in Redes)
            foreach (var (nombre, sesion) in Sesiones)
            {
                var e = EvaluadorDeAcciones.Evaluar(DeClase(ClaseDeAccion.SoloEnLinea), red, sesion);
                Assert.That(e.Permitida, Is.EqualTo(EntradaAbierta(sesion) && ConServidor(red) && SesionValida(sesion)), $"{red} / {nombre}");
                Assert.That(e.QuedaPendiente, Is.False);
            }
    }

    [Test]
    public void SoloWiFiEsSoloEnLineaSinDatosMoviles()
    {
        foreach (var red in Redes)
            foreach (var (nombre, sesion) in Sesiones)
            {
                var e = EvaluadorDeAcciones.Evaluar(DeClase(ClaseDeAccion.SoloWiFi), red, sesion);
                Assert.That(e.Permitida, Is.EqualTo(EntradaAbierta(sesion) && red == EstadoDeRed.EnLinea && SesionValida(sesion)), $"{red} / {nombre}");
            }
        var medida = EvaluadorDeAcciones.Evaluar(DeClase(ClaseDeAccion.SoloWiFi), EstadoDeRed.EnLineaMedida, SesionInformada.PorDispositivo);
        Assert.That(medida.Motivo, Does.Contain("WiFi"));
    }

    // --- Motivos que ve la persona (los textos de DISENO §4.3) ----------------------------------------------

    [TestCase(EstadoDeRed.SinRed, "Sin conexión")]
    [TestCase(EstadoDeRed.RedSinServidor, "El servidor no responde")]
    public void EnviarAhoraSinServidorDicePorQue(EstadoDeRed red, string motivo)
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar("outbox.enviarAhora")!, red, SesionInformada.PorDispositivo);
        Assert.That(e.Permitida, Is.False);
        Assert.That(e.Motivo, Does.StartWith(motivo));
    }

    [TestCase(EstadoDeRed.SinRed, "Sin conexión")]
    [TestCase(EstadoDeRed.RedSinServidor, "El servidor no responde")]
    public void EncuestaSinServidorQuedaPendienteConMotivo(EstadoDeRed red, string motivo)
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar("encuesta.enviar")!, red, SesionInformada.PorDispositivo);
        Assert.That((e.Permitida, e.QuedaPendiente), Is.EqualTo((true, true)));
        Assert.That(e.Motivo, Does.StartWith(motivo));
    }

    [Test]
    [Description("P4 — Sesión vencida con red: la encuesta se guarda y espera al reingreso")]
    public void EncuestaConSesionVencidaEsperaAlReingreso()
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar("encuesta.enviar")!, EstadoDeRed.EnLinea, Oidc(EstadoDeSesion.Vencida));
        Assert.That((e.Permitida, e.QuedaPendiente), Is.EqualTo((true, true)));
        Assert.That(e.Motivo, Does.Contain("ingresar"));
    }

    [TestCase(EstadoDeSesion.SinCuenta, "Ingresá con tu cuenta")]
    [TestCase(EstadoDeSesion.Bloqueada, "Desbloqueá la app")]
    [TestCase(EstadoDeSesion.Ingresando, "Completá el ingreso en el navegador")]
    public void SinEntradaNadaSeHaceYSeDicePorQue(EstadoDeSesion estado, string motivo)
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar("localidades.guardar")!, EstadoDeRed.EnLinea, Oidc(estado));
        Assert.That(e.Permitida, Is.False);
        Assert.That(e.Motivo, Is.EqualTo(motivo));
    }

    [Test]
    public void EnviarAhoraConSesionVencidaPideReingreso()
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar("outbox.enviarAhora")!, EstadoDeRed.EnLinea, Oidc(EstadoDeSesion.Vencida));
        Assert.That(e.Permitida, Is.False);
        Assert.That(e.Motivo, Does.Contain("ingresá de nuevo"));
    }

    // --- sesion.ingresar (DISENO §4.3, último párrafo) ------------------------------------------------------

    [TestCase(EstadoDeSesion.SinCuenta)]
    [TestCase(EstadoDeSesion.Vencida)]
    [TestCase(EstadoDeSesion.Revocada)]
    public void IngresarSePermiteSinSesionValidaYConServidor(EstadoDeSesion estado)
    {
        var ingresar = PoliticaDeAcciones.Buscar(PoliticaDeAcciones.Ingresar)!;
        Assert.That(EvaluadorDeAcciones.Evaluar(ingresar, EstadoDeRed.EnLinea, Oidc(estado)).Permitida, Is.True);
        Assert.That(EvaluadorDeAcciones.Evaluar(ingresar, EstadoDeRed.EnLineaMedida, Oidc(estado)).Permitida, Is.True);

        var sinRed = EvaluadorDeAcciones.Evaluar(ingresar, EstadoDeRed.SinRed, Oidc(estado));
        Assert.That((sinRed.Permitida, sinRed.Motivo), Is.EqualTo((false, "Sin conexión")));
        var sinServidor = EvaluadorDeAcciones.Evaluar(ingresar, EstadoDeRed.RedSinServidor, Oidc(estado));
        Assert.That((sinServidor.Permitida, sinServidor.Motivo), Is.EqualTo((false, "El servidor no responde")));
    }

    [Test]
    public void IngresarEnModoDispositivoNoSeOfrece()
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar(PoliticaDeAcciones.Ingresar)!, EstadoDeRed.EnLinea, SesionInformada.PorDispositivo);
        Assert.That((e.Permitida, e.Motivo), Is.EqualTo((false, "Esta app no usa cuenta")));
    }

    [Test]
    public void IngresarConSesionActivaNoHaceFalta()
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar(PoliticaDeAcciones.Ingresar)!, EstadoDeRed.EnLinea, Oidc(EstadoDeSesion.Activa));
        Assert.That((e.Permitida, e.Motivo), Is.EqualTo((false, "Ya ingresaste")));
    }

    [TestCase(EstadoDeSesion.Bloqueada)]
    [TestCase(EstadoDeSesion.Ingresando)]
    public void IngresarBloqueadaOIngresandoNoSePermite(EstadoDeSesion estado)
    {
        var e = EvaluadorDeAcciones.Evaluar(PoliticaDeAcciones.Buscar(PoliticaDeAcciones.Ingresar)!, EstadoDeRed.EnLinea, Oidc(estado));
        Assert.That(e.Permitida, Is.False);
        Assert.That(e.Motivo, Is.Not.Null.And.Not.Empty);
    }
}
