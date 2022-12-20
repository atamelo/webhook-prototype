using WebHook.Core.Models;
using WebHook.DispatchItemStore.Client;
using WebHook.SubscriptionSotre.Client.Models;

public interface IDispatcherClient
{
    Task DispatchAsync(DispatchItem item);
}