using Microsoft.Extensions.Logging;
using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;

namespace WebHook.Producer.Mocks;

public class DispatchItemStoreMock : IDispatchItemStore
{
    private readonly ILogger<DispatchItemStoreMock> logger;

    public DispatchItemStoreMock(ILogger<DispatchItemStoreMock> logger)
    {
        this.logger = logger;
    }

    public void Enqueue(DispatchItem item, TimeSpan delay)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<DispatchItem> GetNext(int count)
    {
        throw new NotImplementedException();
    }

    public void Remove(DispatchItem item)
    {
        throw new NotImplementedException();
    }
}