using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal partial class Program
{
    private async static Task Main(string[] args)
    {
        IHost host =
            new HostBuilder()
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddHostedService<DispatchItemProducerService>();
               })
               .UseConsoleLifetime()
               .Build();

        await host.RunAsync();
    }
}