using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.DispatchItemStore.Client;
using WebHook.DispatchItemStore.Client.AzureQueueStorage;
using WebHook.DispatchItemStore.Client.Redis;

internal class Program
{
    private async static Task Main(string[] args)
    {
        IHost host =
            new HostBuilder()
               .ConfigureServices(ConfigureServices)
               .ConfigureLogging(loggingBuilder =>
               {
                   loggingBuilder.AddSimpleConsole(options => options.UseUtcTimestamp = true);
               })
               .UseConsoleLifetime()
               .Build();

        await host.RunAsync();
    }
    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddHostedService<DispatcherService>();
        services.AddSingleton<DispatcherLoop>();
        services.AddSingleton<IDispatcherClient, DispatcherMockClient>();
        services.AddSingleton<IDispatchItemStore, AzureDispatchItemStore>();
    }
}