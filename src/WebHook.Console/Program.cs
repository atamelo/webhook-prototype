using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal partial class Program
{
    private async static Task Main(string[] args)
    {
        IHost host =
            new HostBuilder()
               .ConfigureServices(ConfigureServices)
               .UseConsoleLifetime()
               .Build();

        await host.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddHostedService<DispatchItemProducerService>();

        // TODO: add dependencies
    }
}