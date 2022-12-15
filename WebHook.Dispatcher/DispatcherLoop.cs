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

    //TODO fill from config
    private readonly int windowSize = 1000;
    public DispatcherLoop(
        IDispatchItemStore dispatchItemStore,
        IDispatcherClient dispatcherClient,
        ILogger<DispatcherLoop> logger)
    {
        this.dispatchItemStore = dispatchItemStore;
        this.dispatcherClient = dispatcherClient;
        this.logger = logger;
    }
    public async Task Start(CancellationToken cancellationToken)
    {
        try
        {
            await RunDispatcher(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Dispatcher loop critical failure");
            throw;
        }

    }


    private async Task RunDispatcher(CancellationToken cancellationToken)
    {
        List<Task> tasks = new();
        while (cancellationToken.IsCancellationRequested is false)
        {
            WindowFill();
            tasks.RemoveAll(t => t.IsCompleted);
        }

        //Top off sliding window
        void WindowFill()
        {
            while (tasks.Count() < windowSize)
            {
                IReadOnlyList<DispatchItem> items = dispatchItemStore.GetNext(windowSize - tasks.Count());
                if (items.Any() is false) 
                { 
                    Thread.Sleep(100); 
                }
                IEnumerable<Task> newTasks = items.Select(item => TryDispatch(item));
                tasks.AddRange(newTasks);
            }
        }
        return;
    }

    int dispatchCount = 0;
    private async Task TryDispatch(DispatchItem @event)
    {
        try
        {
            await dispatcherClient.DispatchAsync(@event);
            dispatchItemStore.Remove(@event);
            dispatchCount++;
            if (dispatchCount % 100 == 0)
            {
                logger.LogInformation($"Success Dispatch Count: {dispatchCount}");
            }

        }
        catch (Exception e)
        {
            ProcessFailure(@event);
            //logger.LogError(e.Message, e);
        }
    }
    private void ProcessFailure(DispatchItem @event)
    {
        if (@event.DispatchCount >= 3)
        {
            //TODO pause sub via dbcontext
            dispatchItemStore.Remove(@event);
            return;
        }

        TimeSpan retryDelay = TimeSpan.FromMinutes(1);
        dispatchItemStore.DelayRequeue(@event, retryDelay);
    }

}