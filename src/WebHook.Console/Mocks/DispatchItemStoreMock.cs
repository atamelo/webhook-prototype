using Microsoft.Extensions.Logging;
using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;

namespace WebHook.Producer.Mocks;

public class DispatchItemStoreMock : IDispatchItemQueue
{
    private readonly ILogger<DispatchItemStoreMock> _logger;

    public DispatchItemStoreMock(ILogger<DispatchItemStoreMock> logger)
    {
        _logger = logger;
    }

    public Task EnqueueAsync(DispatchItem item, TimeSpan? delay = null) => throw new NotImplementedException();

    public IReadOnlyList<DispatchItem> GetNext(int count) => throw new NotImplementedException();

    public void Remove(DispatchItem item) => throw new NotImplementedException();
}