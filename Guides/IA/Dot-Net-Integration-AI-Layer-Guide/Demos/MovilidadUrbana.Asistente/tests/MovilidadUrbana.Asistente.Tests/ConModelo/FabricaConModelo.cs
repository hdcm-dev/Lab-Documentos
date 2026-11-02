// Nuevo. El host con el cliente real (por la capa única) y el grabador encima.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MovilidadUrbana.Asistente.Comun.AI;

namespace MovilidadUrbana.Asistente.Tests.ConModelo;

public sealed class FabricaConModelo(ConfiguracionDelModelo config, IDictionary<string, string?> ajustes) : ApiWebApplicationFactory(ajustes)
{
    public Grabador Grabador { get; } = new(FabricaDeClientes.CrearChatOllama(config.Endpoint, config.Modelo, config), config);

    protected override void ConfigurarCliente(IServiceCollection services) => services.AddSingleton<IChatClient>(Grabador);
}
