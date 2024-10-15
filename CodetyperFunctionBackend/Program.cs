using CodetyperFunctionBackend.Repositories;
using CodetyperFunctionBackend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        services.AddScoped<DatabaseService>();

        services.AddScoped<LanguageRepository>();
        services.AddScoped<LanguageService>();
    })
    .Build();

host.Run();
