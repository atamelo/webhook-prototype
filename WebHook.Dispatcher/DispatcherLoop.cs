using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using WebHook.Contracts.Events;
using WebHook.DispatchItemStore.Client;
public class DispatcherLoop
{
    private readonly IDispatchItemStore dispatchItemStore;
    private readonly IDispatcherClient dispatcherClient;
    private readonly ILogger<DispatcherLoop> logger;
    private readonly ConcurrentQueue<DispatchItem> queueEvents;
    private readonly int windowSize = 100;
    public DispatcherLoop(
        IDispatchItemStore dispatchItemStore,
        IDispatcherClient dispatcherClient,
        ILogger<DispatcherLoop> logger)
    {
        this.dispatchItemStore = dispatchItemStore;
        this.dispatcherClient = dispatcherClient;
        this.logger = logger;
        queueEvents = new();
    }
    public async Task Start(CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(() => RunDispatcher(cancellationToken), TaskCreationOptions.LongRunning);
        await PreviousSessionCleanupAsync();
        while (cancellationToken.IsCancellationRequested is false)
        {
            await DispatchNextEventAsync();
        }
    }

    /// <summary>
    /// Reprocess evertyhing that was inflight incase this container is booting up from a failure
    /// </summary>
    private async Task PreviousSessionCleanupAsync()
    {
        foreach (DispatchItem @event in dispatchItemStore.GetInFlightList())
        {
            queueEvents.Enqueue(@event);
        }
    }

    /// <summary>
    /// Process next event in the queue
    /// </summary>
    private async Task DispatchNextEventAsync()
    {
        if (queueEvents.Count < windowSize)
        {
            DispatchItem? @event = dispatchItemStore.GetNextOrDefault();
            if (@event.HasValue is false)
            {
                logger.LogInformation("No events ready for dispatch in dispatch store");
                await Task.Delay(1000);
                return;
            }
            queueEvents.Enqueue(@event.Value);
        }
    }
    private Task RunDispatcher(CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        while (cancellationToken.IsCancellationRequested is false)
        {
            WindowFill();
            while (tasks.Any())
            {
                int finished = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(finished);
                WindowFill();
            }
        }

        //Top off sliding window
        void WindowFill()
        {
            while (queueEvents.Any() && tasks.Count() < windowSize)
            {
                if (queueEvents.TryDequeue(out DispatchItem @event))
                {
                    var result = TryDispatch(@event);
                    tasks.Add(result);
                }
            }
        }
        return Task.CompletedTask;
    }

    private async Task TryDispatch(DispatchItem @event)
    {
        //TODO thread this thing out
        try
        {
            await dispatcherClient.DispatchAsync(@event);
            dispatchItemStore.Remove(@event);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message, e);
        }
    }
}