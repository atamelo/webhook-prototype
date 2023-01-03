using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.DispatchItemQueue.Client;
using WebHook.DispatchItemQueue.Client.AzureQueueStorage;
using WebHook.DispatchItemQueue.Client.Redis;
using WebHook.SubscriptionStore.Client.Postgres.Extensions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IHost host =
            new HostBuilder()
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(loggingBuilder => {
                    loggingBuilder.AddSimpleConsole(options => options.UseUtcTimestamp = true);
                })
                .UseConsoleLifetime()
                .Build();

        host.Services.CreateDatabase();

        await host.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddSubscriptionStore();
        services.AddHostedService<DispatcherService>();
        services.AddSingleton<DispatcherLoop>();
        services.AddSingleton<IDispatcherClient, DispatcherMockClient>();
        services.AddSingleton<IDispatchItemQueue, RedisDispatchItemQueue>();
    }
}
