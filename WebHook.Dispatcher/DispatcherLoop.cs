using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MiNET.Utils.Collections;
using System.Collections.Concurrent;
using System.Timers;
using WebHook.Contracts.Events;
using WebHook.DispatchItemStore.Client;
public class DispatcherLoop
{
    private readonly IDispatchItemStore dispatchItemStore;
    private readonly IDispatcherClient dispatcherClient;
    private readonly ILogger<DispatcherLoop> logger;
    private readonly ConcurrentQueue<DispatchItem> queueEvents;
    private readonly MemoryCache memoryCache;
    //TODO fill from config
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
        PreviousSessionCleanup();
        await StartEnqueueServiceAsync(cancellationToken);
    }

    private async Task StartEnqueueServiceAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            await EnqueueNextEventsAsync();
        }
    }

    /// <summary>
    /// Reprocess evertyhing that was inflight incase this container is booting up from a failure
    /// </summary>
    private void PreviousSessionCleanup()
    {
        foreach (DispatchItem @event in dispatchItemStore.GetInFlightList())
        {
            queueEvents.Enqueue(@event);
        }
    }

    /// <summary>
    /// Keep the window queue full for dispatch
    /// At max capacity the system will have window size events in progress and 
    /// the queue will be holding window size events ready for dispatch
    /// </summary>
    private async Task EnqueueNextEventsAsync()
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
        List<Task> tasks = new();
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
        try
        {
            await dispatcherClient.DispatchAsync(@event);
            dispatchItemStore.Remove(@event);
        }
        catch (Exception e)
        {
            
            TimeSpan retryDelay = TimeSpan.FromMinutes(1);

            //TODO update db that we have failed and are going to retry

            TimedEnqueue(@event, retryDelay);
            logger.LogError(e.Message, e);
        }
    }
    private void TimedEnqueue(DispatchItem @event, TimeSpan duration)
    {
        Task.Factory.StartNew(async () =>
        {
            await Task.Delay(duration);
            queueEvents.Enqueue(@event);
        });
    }

}