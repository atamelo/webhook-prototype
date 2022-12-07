using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebHook.Console;

internal partial class Program
{
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
                this.producerLoop.Start(consumerConfig, CancellationToken.None, commitBatchSize: producerConfig.CommitBatchSize);
            }, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping service...");

            this.cancellation.Cancel();
            await producerLoopTask;

            this.logger.LogInformation("Service stopped.");
        }
    }
}