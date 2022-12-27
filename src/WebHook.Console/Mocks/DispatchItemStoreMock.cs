using Microsoft.Extensions.Logging;
using WebHook.Core.Models;
using WebHook.DispatchItemQueue.Client;

namespace WebHook.Producer.Mocks;

public class DispatchItemQueueMock : IDispatchItemQueue
{
    private readonly ILogger<DispatchItemQueueMock> _logger;

    public DispatchItemQueueMock(ILogger<DispatchItemQueueMock> logger)
    {
        _logger = logger;
    }

    public Task EnqueueAsync(DispatchItem item, TimeSpan? delay = null) => throw new NotImplementedException();

    public IReadOnlyList<DispatchItem> GetNext(int count) => throw new NotImplementedException();

    public void Remove(DispatchItem item) => throw new NotImplementedException();
}