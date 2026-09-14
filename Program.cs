using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Aqui você pode registrar dependências futuras, se necessário (ex: HttpClient, serviços customizados)
    })
    .Build();

host.Run();