using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client;

public interface IDispatchItemQueue
{
    void Remove(DispatchItem item);

    void Enqueue(DispatchItem item, TimeSpan? delay = null);

    IReadOnlyList<DispatchItem> GetNext(int count);
}
