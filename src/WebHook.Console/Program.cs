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

        // TODO: read it from a config file/env/commandline
        ProducerConfig producerConfig = new();

        // TODO: wrap it into IHostedService
        ProducerLoop loop = new(null!, null!, null!);

        loop.Start(commitBatchSize: producerConfig.CommitBatchSize);
    }
}