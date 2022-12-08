using WebHook.DispatchItemStore.Client;

namespace WebHook.Producer;

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
