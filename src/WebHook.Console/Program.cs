﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.DispatchItemStore.Client;
using WebHook.Producer;
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

        services.AddTransient<ProducerLoop, ProducerLoopMock>();

        services.AddSingleton<ISubscriptionStore, SubscriptionStoreMock>();
        services.AddSingleton<IDispatchItemStore, DispatchItemStoreMock>();
    }
}