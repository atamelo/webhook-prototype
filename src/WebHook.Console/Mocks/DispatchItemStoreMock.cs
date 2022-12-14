using Microsoft.Extensions.Logging;
using System.IO.Pipes;
using WebHook.DispatchItemStore.Client;

namespace WebHook.Producer.Mocks;

public class DispatchItemStoreMock : IDispatchItemStore
{
    private readonly ILogger<DispatchItemStoreMock> logger;

    public DispatchItemStoreMock(ILogger<DispatchItemStoreMock> logger)
    {
        this.logger = logger;
    }

    public void DelayRequeue(DispatchItem item, TimeSpan delay)
    {
        throw new NotImplementedException();
    }

    public DispatchItem? GetNextOrDefault()
    {
        throw new NotImplementedException();
    }


    public void Put(DispatchItem item)
    {
        this.logger.LogInformation("Dispatch item for subscriber {TenantID} with URL={Url} received.", item.Event.TenantID, item.Subscription.Url);
    }

    public void Remove(DispatchItem item)
    {
        throw new NotImplementedException();
    }
}
