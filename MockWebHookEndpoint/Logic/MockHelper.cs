namespace MockWebHookEndpoint.Logic;

public class MockHelper
{
    private readonly Random random;

    public MockHelper()
    {
        random = new();
    }

    public int RandomNext(int count)
    {
        return random.Next(count);
    }

    public async Task RandomDelayAsync()
    {
        int delay = random.Next(500, 7000);
        await Task.Delay(delay);
    }
}