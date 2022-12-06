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

        // TODO: this should be tracked on per-partition basis!!
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

            if (batchSizeReached) 
            {
                // NOTE: first get a successful "commit" from the dispatch store
                dispatchItemStore.PersistChanges();

                // NOTE: an only then do a commit at the source
                eventLog.AcknowledgeUpTo(envelope.Offset);
            }
        }
    }


    // TODO: think about offloading the real work on a separate thread maybe..
    // public void Stop() { }
}