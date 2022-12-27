using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DispatcherService : IHostedService
{
    private readonly CancellationTokenSource _cancellation;
    private readonly DispatcherLoop _dispatcherLoop;
    private readonly ILogger<DispatcherService> _logger;
    private Task _dispatcherLoopTask;

    public DispatcherService(
        DispatcherLoop dispatcherLoop,
        ILogger<DispatcherService> logger)
    {
        _dispatcherLoop = dispatcherLoop;
        _logger = logger;
        _cancellation = new CancellationTokenSource();
        _dispatcherLoopTask = Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the service...");

        _dispatcherLoopTask = Task.Factory.StartNew(() => {
            // NOTE: when directly implementing IHostedService exceptions like this won't surface
            // until the stop (Ctrl-C) the service
            // Consider inheriting from BackgroundService instead
            //throw null;
            _dispatcherLoop.Start(_cancellation.Token);
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping the service...");

        _cancellation.Cancel();
        // NOTE: here is the point where exceptions are going to surface
        await _dispatcherLoopTask;

        _logger.LogInformation("Service stopped.");
    }
}
