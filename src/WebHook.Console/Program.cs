using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.DispatchItemStore.Client;
using WebHook.DispatchItemStore.Client.AzureQueueStorage;
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
        host.CreateDB();

        await host.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddHostedService<DispatchItemProducerService>();

        //Extension
        services.AddSubscriptionStore();

        services.AddTransient<ProducerLoop>();
        services.AddSingleton<ISubscriptionStore, PostgresSubscriptionStore>();
        services.AddSingleton<IDispatchItemStore, AzureDispatchItemStore>();
    }
}
