namespace WebHook.Producer;

public partial class ProducerLoop
{
    public class ProducerConfig
    {
        public int CommitBatchSize { get; } = 50;
    }
}
