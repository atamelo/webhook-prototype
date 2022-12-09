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

    public IReadOnlyCollection<DispatchItem> GetInFlightList()
    {
        throw new NotImplementedException();
    }

    public DispatchItem? GetNextOrDefault()
    {
        throw new NotImplementedException();
    }

    public void PersistChanges()
    {
    }

    public void Put(DispatchItem item)
    {
        this.logger.LogInformation("Dispatch item for subscriber {SubscriberID} with URL={Url} received.", item.Event.SubscriberID, item.EndpointUrl);
    }

    public void Remove(DispatchItem item)
    {
    }
}
