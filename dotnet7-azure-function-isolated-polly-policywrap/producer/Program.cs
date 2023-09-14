using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using producer;
using producer.Helpers;
using producer.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var currentDirectory = hostingContext.HostingEnvironment.ContentRootPath;

        config.SetBasePath(currentDirectory)
            .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
        config.Build();
    })
    .ConfigureServices((ctx, cfg) =>
    {
        var connectionString = ctx.Configuration.GetConnectionString(Constants.TableStorage);
        cfg.AddSingleton<ITableStorageHelper>(new TableStorageHelper(connectionString));
    })
    .Build();

host.Run();
