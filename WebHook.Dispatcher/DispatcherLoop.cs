using Microsoft.Extensions.Logging;
using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;

public class DispatcherLoop
{
    private readonly IDispatchItemStore dispatchItemStore;
    private readonly IDispatcherClient dispatcherClient;
    private readonly ILogger<DispatcherLoop> logger;
    private readonly Queue<DispatchItem> bufferedItems = new Queue<DispatchItem>();

    //TODO fill from config
    private readonly int windowSize;

    private int dispatchCount;

    public DispatcherLoop(
        IDispatchItemStore dispatchItemStore,
        IDispatcherClient dispatcherClient,
        ILogger<DispatcherLoop> logger)
    {
        this.dispatchItemStore = dispatchItemStore;
        this.dispatcherClient = dispatcherClient;
        this.logger = logger;
        this.windowSize = 1000;
        this.dispatchCount = 0;
    }

    public void Start(CancellationToken cancellationToken)
    {
        try
        {
            RunDispatcher(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Dispatcher loop critical failure");
            throw;
        }
    }

    private void RunDispatcher(CancellationToken cancellationToken)
    {
        Task[] tasks = new Task[windowSize];
        //Fill window
        for (int i = 0; i < windowSize; i++)
        {
            if (cancellationToken.IsCancellationRequested is true)
            {
                return;
            }

            UpdateTask(i);
        }

        //update item window
        while (cancellationToken.IsCancellationRequested is false)
        {
            int finished = Task.WaitAny(tasks);
            UpdateTask(finished);
        }
        return;

        void UpdateTask(int i)
        {
            DispatchItem item = GetNextItem();
            tasks[i] = TryDispatch(item);
        }
    }

    private DispatchItem GetNextItem()
    {
        int FetchAttempt = 0;
        while (bufferedItems.Count == 0)
        {
            FetchAttempt++;
            IReadOnlyList<DispatchItem> newItems = dispatchItemStore.GetNext(32);
            if (newItems.Count > 0)
            {
                foreach (DispatchItem item in newItems)
                {
                    bufferedItems.Enqueue(item);
                }
            }
            else
            {
                //TODO configurable, caps, amounts figure it out
                //https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp#polling-algorithm
                TimeSpan delay = TimeSpan.FromMilliseconds(100) * FetchAttempt;
                if (delay > TimeSpan.FromMinutes(1))
                {
                    delay = TimeSpan.FromMinutes(1);
                }
                Thread.Sleep(delay);
            }
        }

        return bufferedItems.Dequeue();
    }

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
        catch (Exception)
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
        dispatchItemStore.Enqueue(@event, retryDelay);
    }
}