using Microsoft.Extensions.Logging;
using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;

public class DispatcherLoop
{
    private readonly int _windowSize;
    private readonly IDispatchItemStore _dispatchItemStore;
    private readonly IDispatcherClient _dispatcherClient;
    private readonly ILogger<DispatcherLoop> _logger;
    private readonly Queue<DispatchItem> _bufferedItems;
    private int _dispatchCount;

    public DispatcherLoop(
        IDispatchItemStore dispatchItemStore,
        IDispatcherClient dispatcherClient,
        ILogger<DispatcherLoop> logger)
    {
        _dispatchItemStore = dispatchItemStore;
        _dispatcherClient = dispatcherClient;
        _logger = logger;
        //TODO fill from config
        _windowSize = 1000;
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
            int finished = Task.WaitAny(tasks);
            UpdateAtIndex(finished);
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
        int FetchAttempt = 0;
        while (_bufferedItems.Count == 0) {
            FetchAttempt++;
            IReadOnlyList<DispatchItem> newItems = _dispatchItemStore.GetNext(32);
            if (newItems.Count > 0) {
                foreach (DispatchItem item in newItems) {
                    _bufferedItems.Enqueue(item);
                }
            }
            else {
                //TODO configurable, caps, amounts figure it out
                //https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp#polling-algorithm
                TimeSpan delay = TimeSpan.FromMilliseconds(100) * FetchAttempt;
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
            if (_dispatchCount % 100 == 0) {
                _logger.LogInformation($"Success Dispatch Count: {_dispatchCount}");
            }
        }
        catch (Exception) {
            ProcessFailure(@event);
            //logger.LogError(e.Message, e);
        }
    }

    private void ProcessFailure(DispatchItem @event)
    {
        if (@event.DispatchCount >= 3) {
            //TODO pause sub via dbcontext
            _dispatchItemStore.Remove(@event);
            return;
        }

        TimeSpan retryDelay = TimeSpan.FromMinutes(1);
        _dispatchItemStore.Enqueue(@event, retryDelay);
    }
}
