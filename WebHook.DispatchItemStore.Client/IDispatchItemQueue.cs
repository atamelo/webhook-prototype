using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client;

public interface IDispatchItemQueue
{
    void Remove(DispatchItem item);

    IReadOnlyList<DispatchItem> GetNext(int count);

    Task EnqueueAsync(DispatchItem item, TimeSpan? delay = null);
}
