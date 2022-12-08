using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.Producer;

namespace WebHook.Producer;

// TODO: inherit form BackgroundService instead?
public class DispatchItemProducerService : IHostedService
{
    private readonly ProducerLoop producerLoop;
    private readonly ILogger<DispatchItemProducerService> logger;
    private readonly CancellationTokenSource cancellation;

    private Task producerLoopTask;

    public DispatchItemProducerService(ProducerLoop producerLoop, ILogger<DispatchItemProducerService> logger)
    {
        this.producerLoop = producerLoop;
        this.logger = logger;
        this.cancellation = new CancellationTokenSource();

        this.producerLoopTask = Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Starting the service...");

        // TODO: read it from the .NET config system
        ProducerLoop.ProducerConfig producerConfig = new();

        // TODO: read it from a config file/env/commandline
        ProducerLoop.EventLogConsumerConfig consumerConfig = new(Array.Empty<string>());

        this.producerLoopTask = Task.Factory.StartNew(() =>
        {
            // NOTE: when directly implementing IHostedService exceptions like this won't surface
            // until the stop (Ctrl-C) the service
            // Consider inheriting from BackgroundService instead
            //throw null;
            this.producerLoop.Start(consumerConfig, this.cancellation.Token, commitBatchSize: producerConfig.CommitBatchSize);
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Stopping the service...");

        this.cancellation.Cancel();
        // NOTE: here is the point where exeprions are going to surface
        await producerLoopTask;

        this.logger.LogInformation("Service stopped.");
    }
}