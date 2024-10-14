using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using System.Text.Json;
//using System.Text.Json.Serialization;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

host.Run();
