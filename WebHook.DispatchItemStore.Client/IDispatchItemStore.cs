using WebHook.Core.Models;

namespace WebHook.DispatchItemStore.Client;

public interface IDispatchItemStore
{
    void Remove(DispatchItem item);

    void Enqueue(DispatchItem item, TimeSpan delay = default);

    IReadOnlyList<DispatchItem> GetNext(int count);
}