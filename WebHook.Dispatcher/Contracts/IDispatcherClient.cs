using WebHook.DispatchItemStore.Client;

public interface IDispatcherClient
{
    Task DispatchAsync(DispatchItem item);
}