using Microsoft.Extensions.Logging;
using WebHook.DispatchItemStore.Client;
public class DispatcherLoop
{
    private readonly IDispatchItemStore dispatchItemStore;
    private readonly IDispatcherClient dispatcherClient;
    private readonly ILogger<DispatcherLoop> logger;

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
        await PreviousSessionCleanupAsync();
        while (cancellationToken.IsCancellationRequested is false)
        {
          await DispatchNextEventAsync();
        }
    }

    private async Task PreviousSessionCleanupAsync()
    {
        foreach (DispatchItem @event in dispatchItemStore.GetInFlightList())
        {
            await TryDispatch(@event);
        }
    }

    private async Task DispatchNextEventAsync()
    {
        var @event = dispatchItemStore.GetNextOrDefault();
        if (@event.HasValue is false)
        {
            logger.LogInformation("No events ready for dispatch in dispatch store");
            await Task.Delay(1000);
            return;
        }
        await TryDispatch(@event);
    }

    private async Task TryDispatch(DispatchItem? @event)
    {
        try
        {
            await dispatcherClient.DispatchAsync(@event.Value);
            dispatchItemStore.Remove(@event.Value);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message, e);
        }
    }
}