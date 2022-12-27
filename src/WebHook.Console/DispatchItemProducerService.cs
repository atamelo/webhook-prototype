using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace WebHook.Producer;

// TODO: inherit form BackgroundService instead?
public class DispatchItemProducerService : IHostedService
{
    private readonly ProducerLoop _producerLoop;
    private readonly ILogger<DispatchItemProducerService> _logger;
    private readonly CancellationTokenSource _cancellation;

    private Task _producerLoopTask;

    public DispatchItemProducerService(ProducerLoop producerLoop, ILogger<DispatchItemProducerService> logger)
    {
        _producerLoop = producerLoop;
        _logger = logger;
        _cancellation = new CancellationTokenSource();

        _producerLoopTask = Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the service...");

        // TODO: read it from the .NET config system
        ProducerLoop.ProducerConfig producerConfig = new();

        // TODO: read it from a config file/env/commandline
        ProducerLoop.EventLogConsumerConfig consumerConfig = new(new List<string> { "test-topic" }) {
            GroupId = "test-consumer-group",
            BootstrapServers = "localhost:9092",
            // Note: The AutoOffsetReset property determines the start offset in the event
            // there are not yet any committed offsets for the consumer group for the
            // topic/partitions of interest. By default, offsets are committed
            // automatically, so in this example, consumption will only start from the
            // earliest message in the topic 'my-topic' the first time you run the program.
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _producerLoopTask = Task.Factory.StartNew(() => {
            // NOTE: when directly implementing IHostedService exceptions like this won't surface
            // until the stop (Ctrl-C) the service
            // Consider inheriting from BackgroundService instead
            //throw null;
            _producerLoop.Start(consumerConfig, _cancellation.Token, commitBatchSize: producerConfig.CommitBatchSize);
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping the service...");

        _cancellation.Cancel();
        // NOTE: here is the point where exceptions are going to surface
        await _producerLoopTask;

        _logger.LogInformation("Service stopped.");
    }
}