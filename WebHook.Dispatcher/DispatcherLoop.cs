using Microsoft.Extensions.Logging;
using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;

public class DispatcherLoop
{
    private readonly int _windowSize;
    private readonly IDispatchItemQueue _dispatchItemStore;
    private readonly IDispatcherClient _dispatcherClient;
    private readonly ILogger<DispatcherLoop> _logger;
    private readonly Queue<DispatchItem> _bufferedItems;
    private int _dispatchCount;

    public DispatcherLoop(
        IDispatchItemQueue dispatchItemStore,
        IDispatcherClient dispatcherClient,
        ILogger<DispatcherLoop> logger)
    {
        _dispatchItemStore = dispatchItemStore;
        _dispatcherClient = dispatcherClient;
        _logger = logger;
        //TODO fill from config
        _windowSize = 100;
        _dispatchCount = 0;
        _bufferedItems = new();
    }

    public void Start(CancellationToken cancellationToken)
    {
        try {
            RunDispatcher(cancellationToken);
        }
        catch (Exception e) {
            _logger.LogError(e, "Dispatcher loop critical failure");
            throw;
        }
    }

    private void RunDispatcher(CancellationToken cancellationToken)
    {
        Task[] tasks = new Task[_windowSize];
        //Fill window
        for (int i = 0; i < _windowSize; i++) {
            if (cancellationToken.IsCancellationRequested is true) {
                return;
            }
            UpdateAtIndex(i);
        }

        //update item window
        while (cancellationToken.IsCancellationRequested is false) {
            int finishedTask = Task.WaitAny(tasks);
            UpdateAtIndex(finishedTask);
        }
        return;

        void UpdateAtIndex(int finishedTask)
        {
            DispatchItem item = GetNextItem();
            tasks[finishedTask] = TryDispatch(item);
        }
    }

    private DispatchItem GetNextItem()
    {
        int fetchAttempt = 0;
        while (_bufferedItems.Count == 0) {
            fetchAttempt++;
            IReadOnlyList<DispatchItem> newItems = _dispatchItemStore.GetNext(32);
            if (newItems.Count > 0) {
                foreach (DispatchItem item in newItems) {
                    _bufferedItems.Enqueue(item);
                }
            }
            else {
                //TODO configurable, caps, amounts figure it out
                //https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp#polling-algorithm
                TimeSpan delay = TimeSpan.FromMilliseconds(100) * fetchAttempt;
                if (delay > TimeSpan.FromMinutes(1)) {
                    delay = TimeSpan.FromMinutes(1);
                }
                Thread.Sleep(delay);
            }
        }
        return _bufferedItems.Dequeue();
    }

    private async Task TryDispatch(DispatchItem @event)
    {
        try {
            await _dispatcherClient.DispatchAsync(@event);
            _dispatchItemStore.Remove(@event);
            _dispatchCount++;
            if (_dispatchCount % 5 == 0) {
                Console.Clear();
                _logger.LogInformation($"Success Dispatch Count: {_dispatchCount}");
            }
        }
        catch (Exception) {
            await ProcessFailureAsync(@event);
        }
    }

    private async Task ProcessFailureAsync(DispatchItem @event)
    {
        //TODO logging metrics stuff
        if (@event.DispatchCount >= 3) {
            //TODO pause sub via dbcontext
            _dispatchItemStore.Remove(@event);
            return;
        }

        TimeSpan retryDelay = TimeSpan.FromMinutes(1);
        await _dispatchItemStore.EnqueueAsync(@event, retryDelay);
    }
}