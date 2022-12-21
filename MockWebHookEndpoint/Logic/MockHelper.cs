namespace MockWebHookEndpoint.Logic;

public class MockHelper
{
    private readonly Random _random;

    public MockHelper()
    {
        _random = new();
    }

    public int RandomNext(int count)
    {
        return _random.Next(count);
    }

    public async Task RandomDelayAsync()
    {
        int delay = _random.Next(500, 7000);
        await Task.Delay(delay);
    }
}
