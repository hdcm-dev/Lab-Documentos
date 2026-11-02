// Nuevo (ejemplo 17-Observabilidad-Y-Pipeline). Contrato de ejemplo: --fake, --verificar, --plazo-s.
using MovilidadUrbana.Asistente;
using MovilidadUrbana.Asistente.Comun;
using MovilidadUrbana.Asistente.Comun.AI;

var argumentos = ArgumentosDeEjemplo.Leer(args);
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

var resultado = await Corrida.CorrerAsync(argumentos, config, Console.Out);
foreach (var falla in resultado.InvariantesQueFallan)
{
    Console.Error.WriteLine($"invariante: {falla}");
}
return argumentos.Verificar && resultado.InvariantesQueFallan.Count > 0 ? 1 : 0;
