using WebHook.DispatchItemStore.Client;

namespace WebHook.Producer.Mocks;

public class DispatchItemStoreMock : IDispatchItemStore
{
    public void PersistChanges()
    {
    }

    public void Put(DispatchItem item)
    {
    }

    public void Remove(DispatchItem item)
    {
    }
}
