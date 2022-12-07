using WebHook.Console;

internal class Program
{
    public class ProducerConfig
    {
        public int CommitBatchSize { get; } = 50;
    }

    private static void Main(string[] args)
    {
        Console.WriteLine("Starting the producer loop");

        // TODO: read it from the .NET config system
        ProducerConfig producerConfig = new();

        // TODO: read it from a config file/env/commandline
        ProducerLoop.EventLogConsumerConfig consumerConfig = new(Array.Empty<string>());

        // TODO: wrap it into IHostedService
        ProducerLoop loop = new(null!, null!);

        loop.Start(consumerConfig, CancellationToken.None, commitBatchSize: producerConfig.CommitBatchSize);
    }
}