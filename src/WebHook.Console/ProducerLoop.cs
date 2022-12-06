namespace WebHook.Console;

public class ProducerLoop
{
    private readonly IEventLog eventLog;
    private readonly ISubscriptionStore subscriptionStore;
    private readonly IDispatchItemStore dispatchItemStore;

    private bool isBreakRequested;

    public ProducerLoop(
        IEventLog eventLog,
        ISubscriptionStore subscriptionStore,
        IDispatchItemStore dispatchItemStore)
    {
        this.eventLog = eventLog;
        this.subscriptionStore = subscriptionStore;
        this.dispatchItemStore = dispatchItemStore;
    }

    public void Start(int commitBatchSize = 20)
    {
        isBreakRequested = false;

        // NOTE: this is kind of a global (i.e. across all the assigned partitions)
        // batch size counter
        long eventsProcessed = 0;

        while (isBreakRequested is false)
        {
            // MOTE: in Kafka client this is also a blocking call
            EventEnvelope envelope = this.eventLog.PollForNext();

            IReadOnlyList<string> urls = this.subscriptionStore.GetEndpointsFor(envelope.Event);

            foreach (string url in urls)
            {
                DispatchItem item = new(url, envelope.Event);
                dispatchItemStore.Put(item);
            }

            eventsProcessed++;

            bool batchSizeReached = eventsProcessed % commitBatchSize == 0;

            // in reality, when multiple Producer nodes are deployed, the batchSize
            // is only observed in scope of a single node. So, globally (or from the perspective
            // of the dispatchItemStore), the size of a batch can be somewhere from 1 to 
            // number_of_replicas * (batchSize - 1) items.
            if (batchSizeReached) 
            {
                // NOTE: first get a successful "commit" from the dispatch store
                dispatchItemStore.PersistChanges();

                // NOTE: an only then do a commit at the source
                eventLog.AcknowledgeUpTo(envelope);
            }
        }
    }


    // TODO: think about offloading the real work on a separate thread maybe..
    // public void Stop() { }
}