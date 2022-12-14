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
    private readonly int windowSize = 100;
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
        await RunDispatcher(cancellationToken);
    }

    
    private async Task RunDispatcher(CancellationToken cancellationToken)
    {
        List<Task> tasks = new();
        while (cancellationToken.IsCancellationRequested is false)
        {
            await WindowFillAsync();
            while (tasks.Any())
            {
                int finished = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(finished);
                await WindowFillAsync();
            }
        }

        //Top off sliding window
        async Task WindowFillAsync()
        {
            while (tasks.Count() < windowSize)
            {
                DispatchItem? item  = dispatchItemStore.GetNextOrDefault();
                if (item is not null)
                {
                    var result = TryDispatch(item);
                    tasks.Add(result);
                }
                else
                {
                    await Task.Delay(100);
                    break;
                }
            }
        }
        return;
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
            ProcessFailure(@event);
            logger.LogError(e.Message, e);
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
        dispatchItemStore.DelayRequeue(@event,retryDelay);
    }

}