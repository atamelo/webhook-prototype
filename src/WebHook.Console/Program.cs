using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using WebHook.Contracts.Events;
using WebHook.DispatchItemStore.Client;
using WebHook.DispatchItemStore.Client.Redis;
using WebHook.Producer.Mocks;
using WebHook.SubscriptionStore.Client;

namespace WebHook.Producer;

internal partial class Program
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
        services.AddHostedService<DispatchItemProducerService>();

        services.AddTransient<ProducerLoop, ProducerLoopMock>(factory =>
        {
            var subscriptionStore = factory.GetService<ISubscriptionStore>()!;
            var dispatchItemStore = factory.GetService<IDispatchItemStore>()!;
            var logger = factory.GetService<ILogger<ProducerLoop>>()!;

            BlockingCollection<IEvent> fakeEventQueue = new();

            // NOTE: unobserved task!!
            Task fakeGenerator = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    fakeEventQueue.Add(new DummyEvent(DateTime.Now.ToString()));

                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                }

            }, TaskCreationOptions.LongRunning);

            return new ProducerLoopMock(subscriptionStore, dispatchItemStore, logger, fakeEventQueue);
        });

        services.AddSingleton<ISubscriptionStore, SubscriptionStoreMock>();
        services.AddSingleton<IDispatchItemStore, RedisDispatchItemStore>();
    }

    
}