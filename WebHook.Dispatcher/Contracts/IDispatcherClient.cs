using WebHook.Core.Models;

public interface IDispatcherClient
{
    Task DispatchAsync(DispatchItem item);
}