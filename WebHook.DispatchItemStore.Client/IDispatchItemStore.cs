namespace WebHook.DispatchItemStore.Client;
public interface IDispatchItemStore
{
    void Put(DispatchItem item);

    void Remove(DispatchItem item);

    DispatchItem? GetNextOrDefault();
    void DelayRequeue(DispatchItem item, TimeSpan delay);
    IReadOnlyList<DispatchItem> GetNext(int count);
}