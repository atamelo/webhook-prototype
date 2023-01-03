using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.DispatchItemQueue.Client;
using WebHook.DispatchItemQueue.Client.AzureQueueStorage;
using WebHook.DispatchItemQueue.Client.Redis;
using WebHook.SubscriptionStore.Client;
using WebHook.SubscriptionStore.Client.Postgres;
using WebHook.SubscriptionStore.Client.Postgres.Extensions;

namespace WebHook.Producer;

internal partial class Program
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

        //Extension
        host.Services.CreateDatabase();

        await host.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddHostedService<DispatchItemProducerService>();

        //Extension
        services.AddSubscriptionStore();

        services.AddTransient<ProducerLoop>();

        services.AddSingleton<IDispatchItemQueue, RedisDispatchItemQueue>();
    }
}