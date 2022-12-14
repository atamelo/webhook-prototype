using WebHook.DispatchItemStore.Client;

public class DispatcherMockClient : IDispatcherClient
{
    public async Task DispatchAsync(DispatchItem item)
    {
        item.DispatchCount++;
        int min = 1000;
        int max = 7000;
        Random rand = new();
        int delay = rand.Next(min, max);
        await Task.Delay(delay);


        int failed = rand.Next(100);
        if (failed < 5)
        {
            throw new Exception("POST FAILED FOR A RANDOM REASON");
        }
        //TODO make a % of these throw errors to test out retry buffer and DLQ
    }
}
